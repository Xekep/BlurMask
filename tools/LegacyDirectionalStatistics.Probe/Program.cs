using InkRecognition;
using System.Text.Json;

if (args.Length > 1) throw new ArgumentException("Supply at most one own reference JSON.");
if (args.Length == 1)
{
    if (new FileInfo(args[0]).Length > 8 * 1024 * 1024) throw new ArgumentException("Control file exceeds 8 MiB.");
    using var document = JsonDocument.Parse(File.ReadAllBytes(args[0])); int count = 0;
    foreach (var item in document.RootElement.EnumerateArray())
    {
        var strokes = item.GetProperty("strokes").EnumerateArray().Select(s => new LegacyInkStroke(
            s.EnumerateArray().Select(p => new LegacyInkPoint(p[0].GetInt16(), p[1].GetInt16())).ToArray())).ToArray();
        var flags = item.GetProperty("flags").EnumerateArray().Select(p => p.GetByte()).ToArray();
        var intersections = item.GetProperty("intersections").EnumerateArray().Select(p => p.GetByte()).ToArray();
        var actual = LegacyDirectionalStatistics.Extract(strokes, item.GetProperty("width").GetInt32(),
            item.GetProperty("height").GetInt32(), item.GetProperty("arc").GetInt32(), flags, intersections,
            item.GetProperty("intersectionCount").GetInt32());
        Require(actual.SequenceEqual(item.GetProperty("expected").EnumerateArray().Select(p => p.GetInt32())), $"Reference mismatch: {count}.");
        count++;
    }
    Require(count > 0, "Empty reference controls."); Console.WriteLine($"PASS {count} independent 17-statistic comparisons.");
}
var line = new LegacyInkStroke[] { new([new LegacyInkPoint(0, 0), new(10, 0)]) };
var lineFlags = LegacyDirectionalLandmarks.Extract(line);
var values = LegacyDirectionalStatistics.Extract(line, 11, 1, 10, lineFlags.Flags.Span, new byte[2], 0);
Require(values.SequenceEqual(new int[] { 938, 85, 1, 853, 465, 0, 1024, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0 }), "Analytic horizontal profile mismatch.");
var diagonal = new LegacyInkStroke[] { new([new LegacyInkPoint(0, 10), new(10, 0)]) };
var negativeCovariance = LegacyDirectionalStatistics.Extract(diagonal, 11, 11, 14,
    LegacyDirectionalLandmarks.Extract(diagonal).Flags.Span, new byte[] { 1, 3 }, 7);
Require(negativeCovariance[6] == 512 && negativeCovariance[7] == 512 && negativeCovariance[8] == -512 &&
    negativeCovariance[14] == 1 && negativeCovariance[15] == 7 && negativeCovariance[16] == 512, "Signed covariance/intersection classes mismatch.");
Throws<OperationCanceledException>(() => LegacyDirectionalStatistics.Extract(line, 11, 1, 10, new byte[2], new byte[2], 0, new CancellationToken(true)));
Throws<ArgumentException>(() => LegacyDirectionalStatistics.Extract(line, 11, 1, 10, new byte[1], new byte[2], 0));
Throws<ArgumentException>(() => LegacyDirectionalStatistics.Extract(line, 11, 1, 10, new byte[] { 4, 0 }, new byte[2], 0));
Throws<ArgumentException>(() => LegacyDirectionalStatistics.Extract(line, 11, 1, 10, new byte[2], new byte[] { 0, 4 }, 0));
Throws<ArgumentException>(() => LegacyDirectionalStatistics.Extract(line, 9, 1, 10, new byte[2], new byte[2], 0));
Throws<ArgumentException>(() => LegacyDirectionalStatistics.Extract(line, 0, 1, 10, new byte[2], new byte[2], 0));
Console.WriteLine("PASS own global directional/intersection statistics, signed covariance, bounded input and cancellation. Native coordinate preparation, intersections and complete313 recognition are not implemented here.");
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
static void Throws<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new InvalidOperationException($"Expected {typeof(T).Name}."); }
