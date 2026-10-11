using System.Text.Json;
using InkRecognition;

if (args.Length == 0) { InkRecognitionChecks.Run(); return; }
if (args.Length != 2) throw new ArgumentException("Usage: InkRecognition.Probe [model.ikrpack strokes.json]");
using var modelStream = File.OpenRead(args[0]);
var recognizer = StrokeTextRecognizer.Load(modelStream);
using var json = JsonDocument.Parse(File.ReadAllBytes(args[1]));
var strokes = new List<InkStroke>();
foreach (var stroke in json.RootElement.EnumerateArray())
{
    if (stroke.ValueKind == JsonValueKind.Object && stroke.GetProperty("type").GetString() != "stroke") continue;
    var points = new List<InkPoint>();
    foreach (var point in (stroke.ValueKind == JsonValueKind.Array ? stroke : stroke.GetProperty("pointers")).EnumerateArray())
        points.Add(point.ValueKind == JsonValueKind.Array ? new(point[0].GetSingle(), point[1].GetSingle()) : new(point.GetProperty("x").GetSingle(), point.GetProperty("y").GetSingle()));
    strokes.Add(new(points));
}
var started = System.Diagnostics.Stopwatch.StartNew();
var result = recognizer.Recognize(strokes);
Console.WriteLine($"Text: {result.Text}");
Console.WriteLine($"Confidence: {result.Confidence:F4}; frames: {result.Frames}; elapsed: {started.Elapsed.TotalMilliseconds:F1} ms");
