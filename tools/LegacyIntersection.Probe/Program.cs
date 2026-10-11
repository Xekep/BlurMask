using InkRecognition;
using System.Text.Json;
if (args.Length > 1) throw new ArgumentException("Supply at most one own intersection-control JSON.");
if (args.Length == 1)
{
    if (new FileInfo(args[0]).Length > 8 * 1024 * 1024) throw new ArgumentException("Control file exceeds 8 MiB.");
    using var document = JsonDocument.Parse(File.ReadAllBytes(args[0])); int count = 0;
    foreach (var item in document.RootElement.EnumerateArray())
    {
        if (item.TryGetProperty("strokes", out var ink))
        {
            var strokes = ink.EnumerateArray().Select(s => new LegacyInkStroke(s.EnumerateArray().Select(p => new LegacyInkPoint(p[0].GetInt16(), p[1].GetInt16())).ToArray())).ToArray();
            var classified = LegacyDirectionalIntersections.Extract(strokes, item.GetProperty("width").GetInt32(), item.GetProperty("height").GetInt32(),
                item.GetProperty("step").GetInt32(), item.GetProperty("gridScale").GetInt32());
            Require(classified.Flags.Span.SequenceEqual(item.GetProperty("flags").EnumerateArray().Select(p => p.GetByte()).ToArray()), $"Intersection classification mismatch {count}.");
            Require(classified.SelfLoopCount == item.GetProperty("loops").GetInt32(), $"Self-loop count mismatch {count}.");
            count++; continue;
        }
        var p = item.GetProperty("points").EnumerateArray().Select(p => new LegacyInkPoint(p[0].GetInt16(), p[1].GetInt16())).ToArray();
        bool actual = LegacyQuantizedSegmentIntersection.TryIntersect(p[0], p[1], p[2], p[3], out var point);
        Require(actual == item.GetProperty("hit").GetBoolean(), $"Intersection predicate mismatch {count}.");
        if (actual) Require(point.X == item.GetProperty("x").GetInt32() && point.Y == item.GetProperty("y").GetInt32(), $"Quantized crossing location mismatch {count}.");
        count++;
    }
    Require(count > 0, "Empty controls."); Console.WriteLine($"PASS {count} independent quantized segment intersection comparisons.");
}
Require(LegacyQuantizedSegmentIntersection.TryIntersect(new(0, 0), new(10, 10), new(0, 10), new(10, 0), out var cross) && cross == new LegacyIntersectionPoint(5, 5), "Diagonal crossing mismatch.");
Require(LegacyQuantizedSegmentIntersection.TryIntersect(new(5, 0), new(5, 10), new(0, 5), new(10, 5), out cross) && cross == new LegacyIntersectionPoint(5, 5), "Vertical crossing mismatch.");
Require(!LegacyQuantizedSegmentIntersection.TryIntersect(new(0, 0), new(10, 0), new(0, 1), new(10, 1), out _), "Parallel lines intersected.");
Require(LegacyQuantizedSegmentIntersection.TryIntersect(new(1, 0), new(1, 10), new(1, 5), new(1, 20), out cross) && cross == new LegacyIntersectionPoint(1, 7), "Native overlap representative mismatch.");
Require(!LegacyQuantizedSegmentIntersection.TryIntersect(new(1, 0), new(1, 4), new(1, 5), new(1, 20), out _), "Disjoint collinear segments intersected.");
var selfLoop = LegacyDirectionalIntersections.Extract([new([new LegacyInkPoint(0, 0), new(10, 10), new(0, 10), new(10, 0)])], 11, 11, 12 * 1024);
Require(selfLoop.Flags.Span.SequenceEqual(new byte[] { 2, 1, 2, 0 }) && selfLoop.SelfLoopCount == 1, "Self-crossing nearest vertices/interior interval mismatch.");
var between = LegacyDirectionalIntersections.Extract([new([new LegacyInkPoint(0, 0), new(10, 10)]), new([new LegacyInkPoint(0, 10), new(10, 0)])], 11, 11, 12 * 1024);
Require(between.Flags.Span.SequenceEqual(new byte[] { 3, 0, 3, 0 }) && between.SelfLoopCount == 0, "Cross-stroke flags mismatch.");
var noStep = LegacyDirectionalIntersections.Extract([new([new LegacyInkPoint(0, 0), new(10, 10)])], 11, 11, 0);
Require(noStep.Flags.Span.SequenceEqual(new byte[2]), "Zero stride must leave classifications empty.");
var preparedCross = LegacyDirectionalResampling.Prepare([new([new LegacyInkPoint(0, 0), new(64, 64), new(0, 64), new(64, 0)])]);
Require(LegacyDirectionalIntersections.Extract(preparedCross).SelfLoopCount > 0, "Actual normalization/resampling path did not retain the own crossing.");
Throws<ArgumentException>(() => LegacyDirectionalIntersections.Extract([], 1, 1, 1));
Throws<ArgumentException>(() => LegacyDirectionalIntersections.Extract([new([new LegacyInkPoint(-1, 0)])], 1, 1, 1));
Throws<ArgumentException>(() => LegacyDirectionalIntersections.Extract([new(new LegacyInkPoint[256])], 1, 1, 1));
Throws<ArgumentException>(() => LegacyDirectionalIntersections.Extract([new([new LegacyInkPoint(0, 0)])], 1, 1, int.MaxValue));
Throws<OperationCanceledException>(() => LegacyDirectionalIntersections.Extract(preparedCross, cancellationToken: new CancellationToken(true)));
Console.WriteLine("PASS own quantized crossing, spatial-grid classes, nearest vertices, self-loop intervals, actual prepared ink, bounds and cancellation. Complete313 recognition is not tested.");
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
static void Throws<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new InvalidOperationException($"Expected {typeof(T).Name}."); }
