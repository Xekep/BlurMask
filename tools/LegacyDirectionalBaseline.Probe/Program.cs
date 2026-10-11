using InkRecognition;
using System.Text.Json;
if (args.Length > 1) throw new ArgumentException("Supply at most one own baseline-control JSON.");
if (args.Length == 1)
{
    if (new FileInfo(args[0]).Length > 8 * 1024 * 1024) throw new ArgumentException("Control file exceeds8MiB.");
    using var document = JsonDocument.Parse(File.ReadAllBytes(args[0])); int count = 0;
    foreach (var item in document.RootElement.EnumerateArray())
    {
        var strokes = item.GetProperty("strokes").EnumerateArray().Select(s => new LegacyInkStroke(s.EnumerateArray().Select(p => new LegacyInkPoint(p[0].GetInt16(), p[1].GetInt16())).ToArray())).ToArray();
        var actual = LegacyDirectionalBaselineStatistics.ExtractPrepared(strokes, item.GetProperty("width").GetInt32(), item.GetProperty("height").GetInt32(),
            item.GetProperty("arc").GetInt32(), item.GetProperty("baselineY").GetInt32(), item.GetProperty("baselineHeight").GetInt32());
        Require(actual.SequenceEqual(item.GetProperty("expected").EnumerateArray().Select(p => p.GetInt32())), $"Baseline statistic mismatch {count}."); count++;
    }
    Require(count > 0, "Empty controls."); Console.WriteLine($"PASS {count} independent directional raw-baseline9 comparisons.");
}
var horizontal = LegacyDirectionalResampling.Prepare([new([new LegacyInkPoint(0, 0), new(10, 0)])]);
var baseline = LegacyDirectionalBaselineStatistics.Extract(horizontal, new LegacyInkBaseline(10, 10));
Require(baseline.SequenceEqual(new int[] { 1126, 102, 1024, -1024, 0, -921, 0, 1024, 0 }), "Source power-of-two baseline mapping mismatch.");
var translated = LegacyDirectionalResampling.Prepare([new([new LegacyInkPoint(-100, 200), new(-90, 200)])]);
Require(baseline.SequenceEqual(LegacyDirectionalBaselineStatistics.Extract(translated, new LegacyInkBaseline(210, 10))), "Source baseline translation invariance mismatch.");
var downscale = LegacyDirectionalResampling.Prepare([new([new LegacyInkPoint(0, 0), new(256, 0)])]);
Require(LegacyDirectionalBaselineStatistics.Extract(downscale, new LegacyInkBaseline(0, 1))[0] == 65*1024, "Negative-shift height must clamp zero to1.");
var zones = new LegacyInkStroke[] { new([new LegacyInkPoint(0, 0), new(0, 5), new(0, 10), new(0, 15)]) };
var fractions = LegacyDirectionalBaselineStatistics.ExtractPrepared(zones, 1, 16, 15, 10, 5);
Require(fractions.AsSpan(6).SequenceEqual(new int[] { 256, 512, 256 }), "Baseline zones must include both boundaries.");
Throws<ArgumentException>(() => LegacyDirectionalBaselineStatistics.Extract(horizontal, new LegacyInkBaseline(10, -1)));
Throws<ArgumentException>(() => LegacyDirectionalBaselineStatistics.Extract(LegacyDirectionalResampling.Prepare([new([new LegacyInkPoint(0, 0)])]), new LegacyInkBaseline(1, 1)));
Throws<ArgumentException>(() => LegacyDirectionalBaselineStatistics.ExtractPrepared(zones, 1, 16, 15, 10, 0));
Throws<OperationCanceledException>(() => LegacyDirectionalBaselineStatistics.ExtractPrepared(zones, 1, 16, 15, 10, 5, new CancellationToken(true)));
Console.WriteLine("PASS own source/prepared baseline mapping, zones, translation, negative-shift guard, rejected unavailable/ambiguous baselines and cancellation. Recognition accuracy is not tested.");
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
static void Throws<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new InvalidOperationException($"Expected {typeof(T).Name}."); }
