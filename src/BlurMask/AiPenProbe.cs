using Avalonia;
using Avalonia.Media;
using InkRecognition;
using InkRecognition.Math;

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
        VerifyMath();
        VerifyAutomatic();
        Console.WriteLine("AI row layout, uncertainty, model validation, history and peer ink: passed.");
    }

    private static void VerifyLocalModel()
    {
        if (Environment.GetEnvironmentVariable("BLURMASK_AI_TEST_INPUT") is not { Length: > 0 } path) return;
        if (AiRecognitionModels.Current is null) throw new InvalidOperationException("The local model verification requires an installed model.");
        var marks = ReadStrokes(path);
        var result = AiInkLayout.Recognize(marks);
        var expected = Environment.GetEnvironmentVariable("BLURMASK_AI_TEST_TEXT") ?? "hello";
        if (result.Count != 1 || result[0].Text.Text != expected || result[0].Strokes.Length != marks.Count)
            throw new InvalidOperationException($"Local AI integration mismatch: expected {expected}, actual {string.Join(" / ", result.Select(r => r.Text.Text))}.");
        VerifyReplacement(result[0]);
        Console.WriteLine($"Local raw-stroke model → application layout/history: {result[0].Text.Text}; {marks.Count} original strokes preserved.");
    }

    internal static List<InkMark> ReadStrokes(string path)
    {
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
        return marks;
    }

    private static void VerifyMath()
    {
        var latex = "\\left\\{\\begin{matrix}\\sqrt{3}\\\\\\sqrt{6}\\end{matrix}\\right.";
        var strokes = new[] { Stroke(10, 10), Stroke(32, 160) };
        var calls = 0;
        MathRecognitionResult Recognize(IReadOnlyList<InkStroke> ink, CancellationToken token)
        {
            token.ThrowIfCancellationRequested(); calls++;
            if (ink.Count != 2 || ink[1].Points[0].Y != 160) throw new InvalidOperationException("Formula was split into text rows.");
            return new("structural tokens", latex, [], .95f, 10, true);
        }
        var result = AiMathLayout.Recognize(strokes, Recognize);
        if (calls != 1 || result.Count != 1 || result[0].Text.Text != latex || result[0].Text.Font != "STIX Two Math")
            throw new InvalidOperationException("Formula layout lost structure or fonts.");
        VerifyReplacement(result[0]);
        foreach (var rejected in new[]
        {
            new MathRecognitionResult("", latex, [], .2f, 10, true),
            new MathRecognitionResult("", latex, [], .95f, 10, false),
            new MathRecognitionResult("", "\\unsupported{x}", [], .95f, 10, true),
            new MathRecognitionResult("", "\\frac{1}", [], .95f, 10, true),
            new MathRecognitionResult("", " ", [], .95f, 10, true)
        })
            if (AiMathLayout.Recognize(strokes, (_, _) => rejected).Count != 0) throw new InvalidOperationException("Uncertain/unsupported math replaced strokes.");
        foreach (var accepted in new[] { "y=3x+2", "x_{1}^{2}", "\\frac{1}{x}", latex, "\\int x\\,dx".Replace("\\,", " ") })
            if (!MathTypesetting.Supported(accepted)) throw new InvalidOperationException("Supported math cannot be typeset.");
        if (MathTypesetting.Supported(new string('{', 33) + "x" + new string('}', 33))) throw new InvalidOperationException("Unbounded formula nesting.");
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        try { AiMathLayout.Recognize(strokes, Recognize, canceled.Token); throw new InvalidOperationException("Math cancellation ignored."); }
        catch (OperationCanceledException) { }
        var previous = File.Exists(AiMathRecognitionModels.PathName) ? File.ReadAllBytes(AiMathRecognitionModels.PathName) : null;
        using var invalid = new MemoryStream("not a math model"u8.ToArray());
        try { AiMathRecognitionModels.Install(invalid); throw new InvalidOperationException("Invalid math model installation succeeded."); }
        catch (InvalidDataException) { }
        if (previous is not null && !previous.AsSpan().SequenceEqual(File.ReadAllBytes(AiMathRecognitionModels.PathName))) throw new InvalidOperationException("Invalid math import replaced the installed model.");
        if (Environment.GetEnvironmentVariable("BLURMASK_MATH_TEST_INPUT") is { Length: > 0 } path)
        {
            var actual = AiMathLayout.Recognize(ReadStrokes(path));
            var expected = Environment.GetEnvironmentVariable("BLURMASK_MATH_TEST_LATEX") ?? "y=3x+2";
            if (actual.Count != 1 || actual[0].Text.Text != expected) throw new InvalidOperationException($"Raw math application mismatch: expected {expected}; actual {string.Join(" / ", actual.Select(r => r.Text.Text))}");
            VerifyReplacement(actual[0]);
            Console.WriteLine($"Local raw math → application/history/persistence: {expected}");
        }
        Console.WriteLine("Math expression structure, typesetting syntax, confidence, cancellation and atomic installation: passed.");
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
    private sealed class DelegateRecognizer(Func<IReadOnlyList<InkStroke>, CancellationToken, TextRecognition> recognize) : IStrokeTextRecognizer
    {
        public string Language => "en";
        public TextRecognition Recognize(IReadOnlyList<InkStroke> strokes, CancellationToken cancellationToken = default) => recognize(strokes, cancellationToken);
    }
    private static void VerifyAutomatic()
    {
        var strokes = new[] { Stroke(10, 10), Stroke(32, 10) };
        var original = strokes.Select(InkDocument.ExportMark).ToArray();
        void Preserved()
        {
            if (!strokes.Select(InkDocument.ExportMark).Zip(original).All(p => BlurMask.Shared.BoardValidation.Same(p.First, p.Second)))
                throw new InvalidOperationException("Auto dispatch mutated source ink before committing a replacement.");
        }
        var textCalls = 0; var mathCalls = 0;
        var natural = new DelegateRecognizer((input, token) =>
        {
            token.ThrowIfCancellationRequested(); textCalls++;
            if (input.Count != 2 || input.Any(s => s.Points.Count != 3)) throw new InvalidOperationException("Auto dispatch lost pen chronology/points.");
            return new("hello", .99f, 6);
        });
        var hello = AiAutomaticLayout.Recognize(strokes, natural, (_, _) =>
        { mathCalls++; throw new InvalidOperationException("Confident natural text loaded the Math engine."); });
        if (textCalls != 1 || mathCalls != 0 || hello.Count != 1 || hello[0].Text.Text != "hello" || hello[0].Text.Font != "Inter")
            throw new InvalidOperationException("Auto dispatch did not preserve the fast natural-text route.");
        Preserved(); VerifyReplacement(hello[0]);

        foreach (var (latex, expected, grows) in new[]
        {
            ("2+2", "2+2=4", true), ("2+2=", "2+2=4", true), ("2+2=4", "2+2=4", false),
            ("\\frac{1}{2}", "\\frac{1}{2}=0.5", true), ("\\sqrt{9}", "\\sqrt{9}=3", true),
            ("2^{3}", "2^{3}=8", true), ("y=3x+2", "y=3x+2", false), ("\\frac{1}{x}", "\\frac{1}{x}", false)
        })
        {
            textCalls = mathCalls = 0;
            var text = new DelegateRecognizer((_, _) => { textCalls++; return new(latex == "2+2" ? "2+2" : "unclear", .99f, 6); });
            // Structural math fixtures need uncertain text so that natural text cannot take first refusal.
            if (latex != "2+2") text = new DelegateRecognizer((_, _) => { textCalls++; return new("unclear", .2f, 6); });
            var result = AiAutomaticLayout.Recognize(strokes, text, (input, token) =>
            {
                token.ThrowIfCancellationRequested(); mathCalls++;
                if (input.Count != 2) throw new InvalidOperationException("Automatic formula split original strokes.");
                return new(latex, latex, [], .95f, 10, true);
            });
            if (textCalls != 1 || mathCalls != 1 || result.Count != 1 || result[0].Text.Text != expected || result[0].Text.Font != "STIX Two Math")
                throw new InvalidOperationException($"Automatic formula/calculation mismatch for {latex}.");
            Preserved(); VerifyReplacement(result[0], grows);
        }

        var uncertain = new DelegateRecognizer((_, _) => new("hello", .8f, 6));
        var ambiguous = AiAutomaticLayout.Recognize(strokes, uncertain, (_, _) => new("2+2", "2+2", [], .8f, 10, true));
        if (ambiguous.Count != 0) throw new InvalidOperationException("Both uncertain auto candidates replaced handwriting.");
        Preserved();
        var fallback = AiAutomaticLayout.Recognize(strokes, new DelegateRecognizer((_, _) => new("2+2", .99f, 6)), (_, _) => null);
        if (fallback.Count != 1 || fallback[0].Text.Text != "2+2=4") throw new InvalidOperationException("Numeric text fallback required a Math model.");
        Preserved(); VerifyReplacement(fallback[0], true);
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        try
        {
            AiAutomaticLayout.Recognize(strokes, natural, (_, _) => throw new InvalidOperationException("Canceled auto request invoked Math."), canceled.Token);
            throw new InvalidOperationException("Automatic dispatch ignored pre-cancellation.");
        }
        catch (OperationCanceledException) { }
        using var during = new CancellationTokenSource();
        try
        {
            AiAutomaticLayout.Recognize(strokes, new DelegateRecognizer((_, _) => new("unclear", .2f, 6)), (_, _) =>
            { during.Cancel(); return new("2+2", "2+2", [], .95f, 10, true); }, during.Token);
            throw new InvalidOperationException("Automatic dispatch published a result after Math cancellation.");
        }
        catch (OperationCanceledException) { }
        Preserved();
        if (Environment.GetEnvironmentVariable("BLURMASK_AI_TEST_INPUT") is { Length: > 0 } rawText)
        {
            var source = ReadStrokes(rawText);
            var expected = Environment.GetEnvironmentVariable("BLURMASK_AI_TEST_TEXT") ?? "hello";
            var actual = AiAutomaticLayout.Recognize(source, mathRecognizer: (_, _) => throw new InvalidOperationException("Real confident English unnecessarily loaded Math."));
            if (actual.Count != 1 || actual[0].Text.Text != expected) throw new InvalidOperationException("Raw English automatic route differed.");
            VerifyReplacement(actual[0]);
            Console.WriteLine($"Local raw English automatic dispatch: {expected}; Math skipped.");
        }
        if (Environment.GetEnvironmentVariable("BLURMASK_MATH_TEST_INPUT") is { Length: > 0 } rawMath)
        {
            var actual = AiAutomaticLayout.Recognize(ReadStrokes(rawMath));
            var expected = Environment.GetEnvironmentVariable("BLURMASK_MATH_TEST_LATEX") ?? "y=3x+2";
            if (actual.Count != 1 || actual[0].Text.Text != expected || actual[0].Text.Font != "STIX Two Math") throw new InvalidOperationException("Raw Math automatic route differed.");
            VerifyReplacement(actual[0]);
            Console.WriteLine($"Local raw Math automatic dispatch: {expected}; symbolic equation preserved.");
        }
        Console.WriteLine("Automatic text/math dispatch, numeric answers, uncertainty, cancellation, history and persistence: passed.");
    }
    private static InkMark Stroke(double x, double y)
    {
        var mark = new InkMark(InkTool.Pen, Colors.Red, 3, new(x, y));
        mark.AddPoint(new(x + 10, y + 30)); mark.AddPoint(new(x + 20, y + 5)); return mark;
    }
    private static void VerifyReplacement(AiInkLayout.Result result, bool grows = false)
    {
        var strokes = result.Strokes; var text = result.Text;
        var bounds = strokes.Select(m => m.Bounds).Aggregate((a, b) => a.Union(b));
        var document = new InkDocument(); foreach (var stroke in strokes) document.Add(stroke);
        var peer = new InkMark(InkTool.Pen, Colors.Blue, 3, new(200, 200)); document.Add(peer);
        var correctBounds = grows ? text.Bounds.TopLeft == bounds.TopLeft && text.Bounds.Height == bounds.Height && text.Bounds.Width > bounds.Width : text.Bounds == bounds;
        if (!document.ReplaceRecognized(strokes, text) || document.Marks.Count != 2 || document.Marks[0] != text ||
            document.Marks[1] != peer || !correctBounds || text.Id != strokes[0].Id || text.Color != strokes[0].Color)
            throw new InvalidOperationException("AI conversion lost bounds, color, layers or peer ink.");
        document.Undo();
        if (!document.Marks.SequenceEqual(strokes.Append(peer))) throw new InvalidOperationException("AI undo did not restore handwriting.");
        document.Redo();
        if (!document.Marks.SequenceEqual(new[] { text, peer })) throw new InvalidOperationException("AI redo did not restore text.");
        document.Undo(); document.Undo(); document.Undo();
        if (document.ReplaceRecognized(strokes, text)) throw new InvalidOperationException("Stale AI result resurrected deleted handwriting.");
        var imported = new InkDocument(); imported.Import(new() { InkDocument.ExportMark(text) });
        if (imported.Marks.Single().Bounds != text.Bounds || imported.Marks.Single().Text != text.Text || imported.Marks.Single().Font != text.Font || !BlurMask.Shared.BoardValidation.Valid(InkDocument.ExportMark(text)))
            throw new InvalidOperationException("Saved/shared AI text lost its size.");
    }
}
