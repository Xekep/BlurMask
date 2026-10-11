using Avalonia;
using Avalonia.Media;
using InkRecognition;

namespace BlurMask;

internal static class AiPenProbe
{
    internal static void Run()
    {
        SelectionProbe.Run();
        InkRecognitionChecks.Run();
        var strokes = new[] { Stroke(10, 10), Stroke(32, 10), Stroke(10, 160), Stroke(32, 160) };
        var results = AiInkLayout.Recognize(strokes, new TestRecognizer());
        if (results.Count != 2 || results.Any(r => r.Text.Text != "hello" || r.Strokes.Length != 2))
            throw new InvalidOperationException("AI row layout split pen lifts or merged separate rows.");
        foreach (var result in results) VerifyReplacement(result);
        if (AiInkLayout.Recognize(strokes, new TestRecognizer(.2f)).Count != 0)
            throw new InvalidOperationException("Uncertain AI recognition replaced handwriting.");
        if (AiRecognitionModels.Current is null && AiInkLayout.Recognize(strokes).Count != 0)
            throw new InvalidOperationException("Missing model replaced handwriting.");
        using var invalid = new MemoryStream("not a model"u8.ToArray());
        try { AiRecognitionModels.Install(invalid); throw new InvalidOperationException("Invalid model installation succeeded."); }
        catch (InvalidDataException) { }
        VerifyLocalModel();
        Console.WriteLine("AI row layout, uncertainty, model validation, history and peer ink: passed.");
    }

    private static void VerifyLocalModel()
    {
        if (Environment.GetEnvironmentVariable("BLURMASK_AI_TEST_INPUT") is not { Length: > 0 } path) return;
        if (AiRecognitionModels.Current is null) throw new InvalidOperationException("The local model verification requires an installed model.");
        using var json = System.Text.Json.JsonDocument.Parse(File.ReadAllBytes(path));
        var marks = new List<InkMark>();
        foreach (var stroke in json.RootElement.EnumerateArray())
        {
            if (stroke.ValueKind == System.Text.Json.JsonValueKind.Object && stroke.GetProperty("type").GetString() != "stroke") continue;
            InkMark? mark = null;
            foreach (var point in (stroke.ValueKind == System.Text.Json.JsonValueKind.Array ? stroke : stroke.GetProperty("pointers")).EnumerateArray())
            {
                var position = point.ValueKind == System.Text.Json.JsonValueKind.Array ? new Point(point[0].GetDouble(), point[1].GetDouble()) :
                    new Point(point.GetProperty("x").GetDouble(), point.GetProperty("y").GetDouble());
                if (mark is null) mark = new(InkTool.Pen, Colors.Black, 2, position); else mark.AddPoint(position);
            }
            if (mark is not null) marks.Add(mark);
        }
        var result = AiInkLayout.Recognize(marks);
        var expected = Environment.GetEnvironmentVariable("BLURMASK_AI_TEST_TEXT") ?? "hello";
        if (result.Count != 1 || result[0].Text.Text != expected || result[0].Strokes.Length != marks.Count)
            throw new InvalidOperationException($"Local AI integration mismatch: expected {expected}, actual {string.Join(" / ", result.Select(r => r.Text.Text))}.");
        VerifyReplacement(result[0]);
        Console.WriteLine($"Local raw-stroke model → application layout/history: {result[0].Text.Text}; {marks.Count} original strokes preserved.");
    }

    private sealed class TestRecognizer(float confidence = .99f) : IStrokeTextRecognizer
    {
        public string Language => "en";
        public TextRecognition Recognize(IReadOnlyList<InkStroke> strokes, CancellationToken cancellationToken = default)
        {
            if (strokes.Count != 2 || strokes.Any(s => s.Points.Count != 3))
                throw new InvalidOperationException("Original stroke points were lost before inference.");
            return new("hello", confidence, 6);
        }
    }
    private static InkMark Stroke(double x, double y)
    {
        var mark = new InkMark(InkTool.Pen, Colors.Red, 3, new(x, y));
        mark.AddPoint(new(x + 10, y + 30)); mark.AddPoint(new(x + 20, y + 5)); return mark;
    }
    private static void VerifyReplacement(AiInkLayout.Result result)
    {
        var strokes = result.Strokes; var text = result.Text;
        var bounds = strokes.Select(m => m.Bounds).Aggregate((a, b) => a.Union(b));
        var document = new InkDocument(); foreach (var stroke in strokes) document.Add(stroke);
        var peer = new InkMark(InkTool.Pen, Colors.Blue, 3, new(200, 200)); document.Add(peer);
        if (!document.ReplaceRecognized(strokes, text) || document.Marks.Count != 2 || document.Marks[0] != text ||
            document.Marks[1] != peer || text.Bounds != bounds || text.Color != strokes[0].Color)
            throw new InvalidOperationException("AI conversion lost bounds, color, layers or peer ink.");
        document.Undo();
        if (!document.Marks.SequenceEqual(strokes.Append(peer))) throw new InvalidOperationException("AI undo did not restore handwriting.");
        document.Redo();
        if (!document.Marks.SequenceEqual(new[] { text, peer })) throw new InvalidOperationException("AI redo did not restore text.");
        document.Undo(); document.Undo(); document.Undo();
        if (document.ReplaceRecognized(strokes, text)) throw new InvalidOperationException("Stale AI result resurrected deleted handwriting.");
        var imported = new InkDocument(); imported.Import(new() { InkDocument.ExportMark(text) });
        if (imported.Marks.Single().Bounds != bounds || imported.Marks.Single().Text != text.Text)
            throw new InvalidOperationException("Saved/shared AI text lost its size.");
    }
}
