using System.Diagnostics;
using System.Text.Json;
using InkRecognition;
using InkRecognition.RawContent;

void Check(bool value, string message) { if (!value) throw new InvalidDataException(message); }
InkStroke Stroke(params float[] coordinates) => new(Enumerable.Range(0, coordinates.Length / 2).Select(i => new InkPoint(coordinates[2 * i], coordinates[2 * i + 1])).ToArray());
void Near(float actual, double expected, string name, double tolerance = 2e-5) => Check(System.Math.Abs(actual - expected) <= tolerance, $"{name}: {actual} differs from {expected}");

if (args.Length == 0)
{
    var strokes = new[] { Stroke(0, 0, 2, 0), Stroke(1, -1, 1, 1), Stroke(10, 0, 11, 0, 11, 1) };
    var graph = RawContentFeatures.Create(strokes);
    Check(graph.EdgeCount == 9, "Small graph must contain all neighbors and self edges.");
    Near(graph.Nodes[0], 111, "INK alphabet index");
    Near(graph.Nodes[1], 1, "Median diagonal normalization");
    Near(graph.Nodes[2], 0, "Line hull area");
    Near(graph.Nodes[4], 1, "Open line endpoint/path ratio");
    Near(graph.Nodes[7], 1, "Line length/diagonal");
    int crossEdge = Enumerable.Range(0, graph.EdgeCount).Single(k => graph.Edges[k] == 0 && graph.Edges[k + graph.EdgeCount] == 1);
    Near(graph.EdgeFeatures[crossEdge * 16], 0, "Intersecting polylines (no shared sample point)");
    Near(graph.EdgeFeatures[crossEdge * 16 + 1], System.Math.Sqrt(System.Math.Sqrt(2) / 2), "Endpoint minimum");
    Near(graph.EdgeFeatures[crossEdge * 16 + 15], 1, "Forward temporal order");
    Near(graph.EdgeFeatures[(crossEdge + 1) * 16 + 14], -1, "Straight versus right-angle curvature");
    // Classification regions require mutual positive edges, matching categories,
    // and confident predictions. Drawings never become text/math.
    float[] logits = Enumerable.Repeat(-10f, 4 * 14).ToArray();
    logits[12] = 10; logits[14 + 12] = 10; logits[28 + 5] = 10; logits[42 + 9] = 10;
    var result = RawContentSegmentation.Decode(logits, [2f, 2f, 2f, 2f], [0, 1, 1, 2, 1, 0, 2, 1], 4);
    Check(result.Regions.Count == 2 && result.Regions[0].StrokeIndices.SequenceEqual(new[] { 0, 1 }) && result.Regions[1].Category == InkCategory.Math, "Mixed segmentation merged categories or converted shape.");
    result = RawContentSegmentation.Decode(logits, [2f, -2f], [0, 1, 1, 0], 4);
    Check(result.Regions.Count == 3, "One-way edge joined text regions.");
    logits.AsSpan(0, 14).Clear(); result = RawContentSegmentation.Decode(logits, [], [], 4);
    Check(result.Regions.All(r => !r.StrokeIndices.Contains(0)), "Uncertain category converted.");
    var moved = strokes.Select(s => new InkStroke(s.Points.Select(p => new InkPoint(p.X * 8 + 1024, p.Y * 8 - 64)).ToArray())).ToArray();
    var changed = RawContentFeatures.Create(moved);
    // Degenerate box epsilon is in source coordinates; skip those tiny channels.
    for (int k = 0; k < graph.Nodes.Length; k++) if (System.Math.Abs(graph.Nodes[k]) > .01) Near(changed.Nodes[k], graph.Nodes[k], "Scale/translation node invariance");
    var many = Enumerable.Range(0, 25).Select(i => Stroke(i * 2, 0, i * 2 + 1, 1)).ToArray();
    Check(RawContentFeatures.Create(many).EdgeCount <= 25 * 23, "Graph growth is unbounded.");
    foreach (var invalid in new[] { new[] { Stroke() }, new[] { Stroke(float.NaN, 0) }, Enumerable.Repeat(strokes[0], 257).ToArray() })
    {
        bool rejected = false; try { RawContentFeatures.Create(invalid); } catch (ArgumentException) { rejected = true; }
        Check(rejected, "Malformed or oversized input accepted.");
    }
    using var cancel = new CancellationTokenSource(); cancel.Cancel();
    try { RawContentFeatures.Create(strokes, cancel.Token); throw new Exception("Cancellation ignored."); } catch (OperationCanceledException) { }
    foreach (var bytes in new[] { Array.Empty<byte>(), new byte[80] })
    {
        bool rejected = false; try { RawContentAnalyzer.Load(new MemoryStream(bytes)); } catch (Exception e) when (e is InvalidDataException or EndOfStreamException) { rejected = true; }
        Check(rejected, "Invalid pack accepted.");
    }
    var watch = Stopwatch.StartNew(); var longStrokes = new[] { new InkStroke(Enumerable.Range(0, 10000).Select(i => new InkPoint(i * .01f, System.MathF.Sin(i * .001f))).ToArray()), new InkStroke(Enumerable.Range(0, 10000).Select(i => new InkPoint(i * .01f, 20 + System.MathF.Sin(i * .001f))).ToArray()) };
    long allocated = GC.GetAllocatedBytesForCurrentThread(); RawContentFeatures.Create(longStrokes);
    allocated = GC.GetAllocatedBytesForCurrentThread() - allocated;
    Check(allocated < 32 * 1024 * 1024, "Long strokes allocate a quadratic distance matrix.");
    Console.WriteLine($"Raw-content feature/segmentation/guard checks PASS; 20000-point geometry: {watch.Elapsed.TotalMilliseconds:F1} ms, {allocated / 1024.0:F0} KiB allocated"); return;
}
if (args is ["--runtime-native", var library])
{
    using var fixture = typeof(Program).Assembly.GetManifestResourceStream("InkRecognition.RawContent.Probe.runtime-fixture.ikrraw")!;
    var bytes = new byte[fixture.Length]; fixture.ReadExactly(bytes);
    using var runtime = RawContentAnalyzer.Load(new MemoryStream(bytes), Path.GetFullPath(library));
    var inputStrokes = new[] { Stroke(0, 0, 10, 10), Stroke(12, 0, 22, 10) };
    var nativeResult = runtime.Analyze(inputStrokes);
    Check(nativeResult.Strokes.All(s => s.Category == InkCategory.Text && s.Confidence > .99f) && nativeResult.Regions.Count == 1 &&
        nativeResult.Regions[0].StrokeIndices.SequenceEqual(new[] { 0, 1 }), "Native logits/edges/category/segmentation ABI differs.");
    Check(runtime.Analyze([]).Strokes.Count == 0, "Native empty input differs.");
    using var cancel = new CancellationTokenSource(); cancel.Cancel();
    try { runtime.Analyze(inputStrokes, cancel.Token); throw new Exception("Native pre-cancellation ignored."); } catch (OperationCanceledException) { }
    bytes[^1] ^= 1;
    try { RawContentAnalyzer.Load(new MemoryStream(bytes), Path.GetFullPath(library)); throw new Exception("Hash mutation accepted."); } catch (InvalidDataException) { }
    runtime.Dispose(); runtime.Dispose();
    try { runtime.Analyze([]); throw new Exception("Disposed analyzer used."); } catch (ObjectDisposedException) { }
    Console.WriteLine($"Raw-content three-input/two-output CPU ABI, logits, mutual segmentation, lifetime and checksum PASS; runtime {runtime.RuntimeVersion}"); return;
}
if (args.Length != 3) throw new ArgumentException("Usage: probe model.ikrraw strokes.json native-onnx-runtime-library");
using var file = File.OpenRead(args[0]); using var analyzer = RawContentAnalyzer.Load(file, Path.GetFullPath(args[2]));
using var json = JsonDocument.Parse(File.ReadAllBytes(args[1]));
var input = new List<InkStroke>();
var root = json.RootElement;
if (root.ValueKind == JsonValueKind.Object) root = root.GetProperty("raw_strokes");
foreach (var stroke in root.EnumerateArray())
{
    if (stroke.ValueKind == JsonValueKind.Object && stroke.GetProperty("type").GetString() != "stroke") continue;
    var points = stroke.ValueKind == JsonValueKind.Array ? stroke : stroke.GetProperty("pointers");
    input.Add(new(points.EnumerateArray().Select(p => p.ValueKind == JsonValueKind.Array ? new InkPoint(p[0].GetSingle(), p[1].GetSingle()) : new InkPoint(p.GetProperty("x").GetSingle(), p.GetProperty("y").GetSingle())).ToArray()));
}
var started = Stopwatch.StartNew(); var prediction = analyzer.Analyze(input);
foreach (var stroke in prediction.Strokes) Console.WriteLine($"Stroke {stroke.StrokeIndex}: {stroke.Category} ({stroke.Confidence:F4})");
foreach (var region in prediction.Regions) Console.WriteLine($"Region {region.Category}: {string.Join(',', region.StrokeIndices)} ({region.Confidence:F4})");
Console.WriteLine($"Elapsed {started.Elapsed.TotalMilliseconds:F1} ms; runtime {analyzer.RuntimeVersion}");
var expected = Environment.GetEnvironmentVariable("BLURMASK_RAW_EXPECTED_CATEGORY");
if (expected is { Length: > 0 }) Check(prediction.Strokes.All(s => s.Category.ToString() == expected && s.Confidence >= .85f) && prediction.Regions.Count == 1 && prediction.Regions[0].StrokeIndices.Count == input.Count, "Real model category/segmentation differs.");
using var canceled = new CancellationTokenSource(); canceled.Cancel();
try { analyzer.Analyze(input, canceled.Token); throw new Exception("Cancellation ignored."); } catch (OperationCanceledException) { }
analyzer.Dispose(); analyzer.Dispose();
try { analyzer.Analyze([]); throw new Exception("Disposed analyzer used."); } catch (ObjectDisposedException) { }
