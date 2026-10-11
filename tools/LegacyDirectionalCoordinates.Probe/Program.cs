using InkRecognition;
using System.Text.Json;
if (args.Length > 1) throw new ArgumentException("Supply at most one own reference JSON.");
if (args.Length == 1)
{
    if (new FileInfo(args[0]).Length > 8 * 1024 * 1024) throw new ArgumentException("Control file exceeds 8 MiB.");
    using var document = JsonDocument.Parse(File.ReadAllBytes(args[0])); int cases = 0;
    foreach (var item in document.RootElement.EnumerateArray())
    {
        var actual = LegacyDirectionalCoordinatePreparation.Normalize(item.GetProperty("source").EnumerateArray().Select(s => new LegacyInkStroke(
            s.EnumerateArray().Select(p => new LegacyInkPoint(p[0].GetInt16(), p[1].GetInt16())).ToArray())).ToArray());
        var expected = item.GetProperty("strokes");
        Require(actual.ScaleShift == item.GetProperty("shift").GetInt32(), $"Scale mismatch {cases}.");
        Require(actual.OriginalMinimumY == item.GetProperty("minY").GetInt32(), $"Origin mismatch {cases}.");
        Require(actual.Strokes.Count == expected.GetArrayLength(), "Stroke count mismatch.");
        for (int i = 0; i < actual.Strokes.Count; i++)
            Require(actual.Strokes[i].Points.SequenceEqual(expected[i].EnumerateArray().Select(p => new LegacyInkPoint(p[0].GetInt16(), p[1].GetInt16())).ToArray()), $"Coordinate mismatch {cases}:{i}.");
        Require(actual.CumulativePhysicalLengths.Span.SequenceEqual(item.GetProperty("cumulative").EnumerateArray().Select(p => p.GetInt32()).ToArray()), $"Physical lengths mismatch {cases}.");
        var sampleExpected = item.GetProperty("sampling");
        var sampled = LegacyDirectionalResampling.Resample(actual, sampleExpected.GetProperty("target").GetInt32());
        Require(sampled.Width == sampleExpected.GetProperty("width").GetInt32() && sampled.Height == sampleExpected.GetProperty("height").GetInt32() &&
            sampled.SourceStepFixed == sampleExpected.GetProperty("step").GetInt32(), $"Resampled geometry mismatch {cases}.");
        var expectedStrokes = sampleExpected.GetProperty("strokes");
        for (int i = 0; i < sampled.Strokes.Count; i++)
            Require(sampled.Strokes[i].Points.SequenceEqual(expectedStrokes[i].EnumerateArray().Select(p => new LegacyInkPoint(p[0].GetInt16(), p[1].GetInt16())).ToArray()), $"Resampled points mismatch {cases}:{i}.");
        Require(sampled.CumulativePhysicalLengths.Span.SequenceEqual(sampleExpected.GetProperty("cumulative").EnumerateArray().Select(p => p.GetInt32()).ToArray()), $"Resampled length mismatch {cases}.");
        cases++;
    }
    Require(cases > 0, "Empty references."); Console.WriteLine($"PASS {cases} independent power-of-two coordinate and target100 resampling comparisons.");
}
LegacyInkStroke[] Line(short distance) => [new([new LegacyInkPoint(0, 0), new(distance, 0)])];
foreach (var (length, shift, normalized) in new (short, int, short)[] { (1, 6, 64), (63, 1, 126), (64, 0, 64), (127, 0, 127), (128, -1, 64), (256, -2, 64) })
{
    var value = LegacyDirectionalCoordinatePreparation.Normalize(Line(length));
    Require(value.ScaleShift == shift && value.Strokes[0].Points[1].X == normalized && value.PhysicalArcLength == normalized, "Power-of-two segment threshold mismatch.");
}
var noGap = LegacyDirectionalCoordinatePreparation.Normalize([new([new LegacyInkPoint(0, 0), new(64, 0)]), new([new LegacyInkPoint(10000, 10000), new(10064, 10000)])]);
Require(noGap.PhysicalArcLength == 128 && noGap.CumulativePhysicalLengths.Span.SequenceEqual(new int[] { 0, 64, 64, 128 }), "Pen-up distance was added.");
var degenerate = LegacyDirectionalCoordinatePreparation.Normalize([new([new LegacyInkPoint(10, 20), new(10, 20)])]);
Require(degenerate.ScaleShift == 0 && degenerate.PhysicalArcLength == 0 && degenerate.Strokes[0].Points[0] == new LegacyInkPoint(0, 0), "Degenerate origin removal mismatch.");
Throws<ArgumentException>(() => LegacyDirectionalCoordinatePreparation.Normalize([]));
Throws<ArgumentException>(() => LegacyDirectionalCoordinatePreparation.Normalize([new(new LegacyInkPoint[4096]), new([new LegacyInkPoint(0, 0)])]));
Throws<ArgumentException>(() => LegacyDirectionalCoordinatePreparation.Normalize([new([new LegacyInkPoint(short.MinValue, 0)]), new([new LegacyInkPoint(short.MaxValue, 0)])]));
Throws<OperationCanceledException>(() => LegacyDirectionalCoordinatePreparation.Normalize(Line(64), new CancellationToken(true)));
var straight = LegacyDirectionalResampling.Prepare(Line(64));
Require(straight.Strokes[0].Points.Length == 101 && straight.Strokes[0].Points[^1] == new LegacyInkPoint(64, 0), "Native rounded repeated points or endpoint handling mismatch.");
Require(straight.Width == 65 && straight.Height == 1, "Prepared coordinate-unit dimensions mismatch.");
var extentOnly = LegacyDirectionalResampling.Prepare(Line(64), includeCoordinateUnit: false);
Require(extentOnly.Width == 64 && extentOnly.Height == 1, "Extent-only geometry mode mismatch.");
var single = LegacyDirectionalResampling.Prepare([new([new LegacyInkPoint(10, 20), new(10, 20)])]);
Require(single.Strokes[0].Points.Length == 1 && single.PhysicalArcLength == 0, "Zero-length stroke was not collapsed to its first point.");
Throws<ArgumentOutOfRangeException>(() => LegacyDirectionalResampling.Prepare(Line(64), 0));
Throws<OperationCanceledException>(() => LegacyDirectionalResampling.Resample(
    LegacyDirectionalCoordinatePreparation.Normalize(Line(64)), cancellationToken: new CancellationToken(true)));
Console.WriteLine("PASS own normalization thresholds, target100 physical resampling, repeated points, dimensions, pen-up exclusion, degenerate geometry, bounds and cancellation. Intersections and 313 recognition are not implemented here.");
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
static void Throws<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new InvalidOperationException($"Expected {typeof(T).Name}."); }
