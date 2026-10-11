using System.Text.Json;
using InkRecognition;
using InkRecognition.Math;

Console.OutputEncoding = new System.Text.UTF8Encoding(false);

if (args.Length == 0)
{
    NumericEvaluatorChecks.Verify();
    void Equal(string? actual, string? expected) { if (actual != expected) throw new InvalidDataException($"Expected {expected}; got {actual}"); }
    Equal(MathLatexFormatter.Format(["y", "=", "3", "x", "+", "2"]), "y=3x+2");
    Equal(MathLatexFormatter.Format(["<frac>", "1", "<fracbar>", "x", "</frac>"]), "\\frac{1}{x}");
    Equal(MathLatexFormatter.Format(["x", "<sup>", "2", "</sup>"]), "x^{2}");
    Equal(MathLatexFormatter.Format(["<fence>", "{", "<table>", "√", "3", "</sqrt>", "<\\>", "√", "6", "</sqrt>", "</table>", "</fence>"]), "\\left\\{\\begin{matrix}\\sqrt{3}\\\\\\sqrt{6}\\end{matrix}\\right.");
    Equal(MathLatexFormatter.Format(["√", "3"]), null);
    Equal(MathLatexFormatter.Format(["<over>", "x", "</over>"]), null);
    foreach (var invalid in new[] { Array.Empty<byte>(), new byte[64] })
    {
        bool rejected = false;
        try { MathModel.Read(new MemoryStream(invalid)); } catch (Exception e) when (e is InvalidDataException or EndOfStreamException) { rejected = true; }
        if (!rejected) throw new InvalidDataException("Malformed model was accepted.");
    }
    Console.WriteLine("Math formatter/model guards PASS (8 checks)"); return;
}
if (args is ["--features", var fixture])
{
    using var stream = File.OpenRead(fixture);
    foreach (var check in MathFeatureChecks.Compare(stream)) Console.WriteLine($"{check.Name}: {check.Frames} frames, error {check.MaximumError}");
    return;
}
if (args is ["--runtime-native", var nativePath])
{
    using var native = new OrtRuntime(Path.GetFullPath(nativePath));
    using var value = native.Tensor([1f, -2f, 3.5f], 3);
    var read = value.Floats();
    if (!read.Values.AsSpan().SequenceEqual(new float[] { 1f, -2f, 3.5f }) || !read.Shape.AsSpan().SequenceEqual(new long[] { 3 })) throw new InvalidDataException("Native tensor differs.");
    using var empty = native.Tensor(Array.Empty<float>(), 2, 4, 0, 64);
    if (empty.Floats().Values.Length != 0) throw new InvalidDataException("Empty native tensor differs.");
    // Independently constructed ONNX IR8/opset13 Identity(float[3]), no weights.
    // Field encoding follows onnx/onnx v1.19.0 onnx.proto.
    var graph = Convert.FromBase64String("CAg6OwoQCgF4EgF5IghJZGVudGl0eRIFcHJvYmVaDwoBeBIKCggIARIECgIIA2IPCgF5EgoKCAgBEgQKAggDQgIQDQ==");
    using var session = native.CreateSession(graph, ["x"], ["y"]);
    using (var ordinaryRun = native.CreateRun(CancellationToken.None))
    {
        var results = session.Run(ordinaryRun, value);
        try { if (!results[0].Floats().Values.AsSpan().SequenceEqual(read.Values)) throw new InvalidDataException("Identity graph differs."); }
        finally { foreach (var output in results) output.Dispose(); }
    }
    using var cancellation = new CancellationTokenSource();
    using var run = native.CreateRun(cancellation.Token); cancellation.Cancel();
    bool terminated = false;
    try { foreach (var output in session.Run(run, value)) output.Dispose(); }
    catch (InvalidOperationException) { terminated = true; }
    if (!terminated) throw new InvalidDataException("Native run termination was not honored.");
    Console.WriteLine($"Native C API tensor/zero-cache/Identity inference/termination PASS; runtime {native.Version}"); return;
}
if (args is ["--runtime-checks", var pack, var runtime])
{
    var bytes = File.ReadAllBytes(pack);
    using var checkedRecognizer = StrokeMathRecognizer.Load(new MemoryStream(bytes), Path.GetFullPath(runtime));
    if (checkedRecognizer.Recognize(Array.Empty<InkStroke>()) is not { Frames: 0, Completed: true, Text: "" }) throw new InvalidDataException("Empty result differs.");
    using var canceled = new CancellationTokenSource(); canceled.Cancel();
    try { checkedRecognizer.Recognize(Array.Empty<InkStroke>(), canceled.Token); throw new InvalidDataException("Cancellation not observed."); }
    catch (OperationCanceledException) { }
    bytes[^1] ^= 1;
    try { MathModel.Read(new MemoryStream(bytes)); throw new Exception("Hash mutation accepted."); }
    catch (InvalidDataException) { }
    checkedRecognizer.Dispose(); checkedRecognizer.Dispose();
    try { checkedRecognizer.Recognize(Array.Empty<InkStroke>()); throw new Exception("Disposed recognizer used."); }
    catch (ObjectDisposedException) { }
    Console.WriteLine("Native runtime lifetime/empty/cancellation/hash guards PASS (5 checks)"); return;
}
if (args.Length != 3) throw new ArgumentException("Usage: InkRecognition.Math.Probe model.ikrmath strokes.json native-runtime-library");
using var model = File.OpenRead(args[0]);
using var recognizer = StrokeMathRecognizer.Load(model, Path.GetFullPath(args[2]));
using var json = JsonDocument.Parse(File.ReadAllBytes(args[1]));
var strokes = new List<InkStroke>();
foreach (var stroke in json.RootElement.EnumerateArray())
{
    if (stroke.ValueKind == JsonValueKind.Object && stroke.GetProperty("type").GetString() != "stroke") continue;
    var points = new List<InkPoint>();
    foreach (var p in (stroke.ValueKind == JsonValueKind.Array ? stroke : stroke.GetProperty("pointers")).EnumerateArray())
        points.Add(p.ValueKind == JsonValueKind.Array ? new(p[0].GetSingle(), p[1].GetSingle()) : new(p.GetProperty("x").GetSingle(), p.GetProperty("y").GetSingle()));
    strokes.Add(new(points));
}
var started = System.Diagnostics.Stopwatch.StartNew();
var result = recognizer.Recognize(strokes);
Console.WriteLine($"Text: {result.Text}");
Console.WriteLine($"LaTex: {result.LaTex ?? "[unsupported structure]"}");
Console.WriteLine($"Confidence: {result.Confidence:R}; frames: {result.Frames}; completed: {result.Completed}; elapsed: {started.Elapsed.TotalMilliseconds:F1} ms; runtime: {recognizer.RuntimeVersion}");
