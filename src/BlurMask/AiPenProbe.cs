using System.Diagnostics;
using System.Text.Json;
using Avalonia;
using Avalonia.Media;

namespace BlurMask;

internal static class AiPenProbe
{
    internal static void Run()
    {
        var model = AiSymbolNetwork.Instance.Value;
        SelectionProbe.Run();
        VerifyHandwritingNetwork();
        VerifyEnglishNetwork();
        VerifyExpression();
        using var stream = File.OpenRead(Path.Combine("tools", "ai-pen-fixtures.bin"));
        using var reader = new BinaryReader(stream);
        var examples = reader.ReadInt32(); var largestError = 0f; var pixels = new float[1024];
        for (var example = 0; example < examples; example++)
        {
            var expected = (char)reader.ReadInt32();
            for (var i = 0; i < pixels.Length; i++) pixels[i] = reader.ReadByte() / 255f;
            var logits = model.Predict(pixels);
            if (reader.ReadInt32() != logits.Length) throw new InvalidDataException("Fixture vocabulary differs from the model.");
            for (var i = 0; i < logits.Length; i++) largestError = Math.Max(largestError, Math.Abs(logits[i] - reader.ReadSingle()));
            var result = model.Recognize(pixels);
            if (result.Symbol != expected || result.Confidence < .75f)
                throw new InvalidOperationException($"AI fixture mismatch: {expected} -> {result.Symbol}, {result.Confidence}.");
        }
        if (largestError > .002f) throw new InvalidOperationException($"C# and training inference disagree: {largestError}.");
        // Rasterize real multi-stroke pen input instead of feeding only pre-normalized images.
        foreach (var (symbol, paths) in new (char, Point[][])[]
        {
            ('+', [[new(10, 50), new(90, 50)], [new(50, 10), new(50, 90)]]),
            ('=', [[new(10, 30), new(90, 30)], [new(10, 70), new(90, 70)]]),
            ('×', [[new(10, 10), new(90, 90)], [new(90, 10), new(10, 90)]])
        })
        {
            var strokes = paths.Select(points =>
            { var mark = new InkMark(InkTool.Pen, Colors.Red, 5, points[0]); foreach (var point in points.Skip(1)) mark.AddPoint(point); return mark; }).ToArray();
            var bounds = strokes.Select(m => m.Bounds).Aggregate((a, b) => a.Union(b));
            var result = model.Recognize(AiPenRecognizer.Rasterize(strokes, bounds));
            if (result.Symbol != symbol || result.Confidence < .75f)
                throw new InvalidOperationException($"Drawn symbol mismatch: {symbol} -> {result.Symbol}, {result.Confidence}.");
            VerifyReplacement(strokes, bounds, symbol);
        }
        var start = Stopwatch.GetTimestamp(); var allocated = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < 100; i++) model.Recognize(pixels);
        var report = new Dictionary<string, PerformanceMetric>
        {
            ["ai_symbol_100_cpu_inferences"] = new(Stopwatch.GetElapsedTime(start).TotalMilliseconds,
                GC.GetAllocatedBytesForCurrentThread() - allocated, 100),
            ["ai_training_logit_max_error"] = new(largestError, 0, examples)
        };
        Console.WriteLine(JsonSerializer.Serialize(report, PerformanceJsonContext.Default.DictionaryStringPerformanceMetric));
    }
    private static void VerifyExpression()
    {
        var paths = new Point[][]
        {
            [new(10, 50), new(90, 50)], [new(50, 10), new(50, 90)],
            [new(130, 28), new(210, 28)], [new(130, 66), new(210, 66)],
            [new(250, 12), new(330, 92)], [new(330, 12), new(250, 92)]
        };
        var strokes = paths.Select(path => { var mark = new InkMark(InkTool.Pen, Colors.Red, 5, path[0]); mark.AddPoint(path[1]); return mark; }).ToArray();
        var expression = AiInkLayout.Recognize(strokes, "en").Single();
        var bounds = strokes.Select(m => m.Bounds).Aggregate((a, b) => a.Union(b));
        if (expression.Text.Text != "+=×" || expression.Text.Bounds != bounds || expression.Strokes.Length != 6)
            throw new InvalidOperationException($"Expression was split, reordered or resized: {expression.Text.Text}.");
        var rows = AiInkLayout.Recognize(strokes.Concat(strokes.Select(s => s.Translate(new(0, 150)))).ToArray(), "en");
        if (rows.Count != 2 || rows.Any(r => r.Text.Text != "+=×")) throw new InvalidOperationException("Separate handwriting rows were merged.");
    }
    private static void VerifyEnglishNetwork()
    {
        var network = AiHandwritingNetwork.English.Value;
        using var reader = new BinaryReader(File.OpenRead(Path.Combine("tools", "ai-english-fixture.bin")));
        var pixels = new float[32 * 128]; for (var i = 0; i < pixels.Length; i++) pixels[i] = reader.ReadSingle();
        var start = Stopwatch.GetTimestamp(); var memory = network.Encode(pixels); var error = 0f;
        for (var i = 0; i < memory.Length; i++) error = Math.Max(error, Math.Abs(memory[i] - reader.ReadSingle()));
        var logits = network.Decode(memory, [95,18,15], [0,1,2]);
        for (var i = 0; i < logits.Length; i++) error = Math.Max(error, Math.Abs(logits[i] - reader.ReadSingle()));
        if (error > .005f) throw new InvalidOperationException($"English inference differs from PyTorch: {error}.");
        Console.WriteLine($"English encoder/decoder: {Stopwatch.GetElapsedTime(start).TotalMilliseconds:F1} ms, max error {error:G4}.");
        using var samples = new BinaryReader(File.OpenRead(Path.Combine("tools", "ai-english-words.bin")));
        var count = samples.ReadInt32(); start = Stopwatch.GetTimestamp();
        for (var sample = 0; sample < count; sample++)
        {
            var expected = System.Text.Encoding.UTF8.GetString(samples.ReadBytes(samples.ReadInt32()));
            for (var i = 0; i < pixels.Length; i++) pixels[i] = samples.ReadSingle();
            var result = network.Recognize(pixels);
            if (result.Text != expected || result.Confidence < .75f) throw new InvalidOperationException($"English word mismatch: {expected} -> {result.Text}.");
        }
        Console.WriteLine($"English synthetic cursive/numbers/expression: {count} samples, {Stopwatch.GetElapsedTime(start).TotalMilliseconds:F1} ms total.");
    }
    private static void VerifyHandwritingNetwork()
    {
        var network = AiHandwritingNetwork.Instance.Value;
        using var reader = new BinaryReader(File.OpenRead(Path.Combine("tools", "ai-handwriting-fixture.bin")));
        var pixels = new float[48 * 512]; for (var i = 0; i < pixels.Length; i++) pixels[i] = reader.ReadSingle();
        var start = Stopwatch.GetTimestamp(); var memory = network.Encode(pixels); var error = 0f;
        for (var i = 0; i < memory.Length; i++) error = Math.Max(error, Math.Abs(memory[i] - reader.ReadSingle()));
        Console.WriteLine($"Native handwriting encoder: {Stopwatch.GetElapsedTime(start).TotalMilliseconds:F1} ms; max error {error:G4}.");
        if (error > .005f) throw new InvalidOperationException($"Native handwriting encoder differs from PyTorch: {error}.");
        var logits = network.Decode(memory, [233, 129, 140], [0, 1, 2]); error = 0;
        for (var i = 0; i < logits.Length; i++) error = Math.Max(error, Math.Abs(logits[i] - reader.ReadSingle()));
        Console.WriteLine($"Native handwriting decoder max error {error:G4}.");
        if (error > .005f) throw new InvalidOperationException($"Native handwriting decoder differs from PyTorch: {error}.");
        using var cursive = new BinaryReader(File.OpenRead(Path.Combine("tools", "ai-cursive-fixtures.bin")));
        var samples = cursive.ReadInt32(); start = Stopwatch.GetTimestamp();
        for (var sample = 0; sample < samples; sample++)
        {
            var expected = System.Text.Encoding.UTF8.GetString(cursive.ReadBytes(cursive.ReadInt32()));
            for (var i = 0; i < pixels.Length; i++) pixels[i] = cursive.ReadSingle();
            var result = network.Recognize(pixels);
            if (result.Text != expected || result.Confidence < .75f)
                throw new InvalidOperationException($"Real cursive mismatch: {expected} -> {result.Text}, {result.Confidence}.");
        }
        Console.WriteLine($"Native real cursive: {samples} examples, {Stopwatch.GetElapsedTime(start).TotalMilliseconds:F1} ms total.");
    }
    private static void VerifyReplacement(InkMark[] strokes, Rect bounds, char symbol)
    {
        var document = new InkDocument(); foreach (var stroke in strokes) document.Add(stroke);
        var peer = new InkMark(InkTool.Pen, Colors.Blue, 3, new(200, 200)); document.Add(peer);
        var text = new InkMark(InkTool.Text, strokes[0].Color, strokes[0].Width, bounds.TopLeft)
            { Id = strokes[0].Id, Text = symbol.ToString() };
        text.AddPoint(bounds.BottomRight);
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
