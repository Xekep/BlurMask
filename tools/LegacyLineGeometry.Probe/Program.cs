using InkRecognition;
using System.Text.Json;

if (args.Length > 1) throw new ArgumentException("Supply at most one own line-geometry control JSON.");
if (args.Length == 1)
{
    if (new FileInfo(args[0]).Length > 8 * 1024 * 1024) throw new ArgumentException("Control file exceeds 8 MiB.");
    using var document = JsonDocument.Parse(File.ReadAllBytes(args[0]));
    int scales = 0, lines = 0;
    foreach (var item in document.RootElement.GetProperty("scales").EnumerateArray())
    {
        float[] samples = item.GetProperty("samples").EnumerateArray().Select(x => x.GetSingle()).ToArray();
        float[] p = item.GetProperty("profile").EnumerateArray().Select(x => x.GetSingle()).ToArray();
        var actual = LegacyLineScale.Select(samples, item.GetProperty("associated").GetInt32(),
            item.GetProperty("resolution").GetSingle(), new(p[0], p[1], p[2], p[3]));
        var expected = item.GetProperty("expected");
        Require(actual.Primary == expected[0].GetDouble() && actual.Secondary == expected[1].GetDouble(), $"Scale mismatch {scales}.");
        scales++;
    }
    foreach (var item in document.RootElement.GetProperty("lines").EnumerateArray())
    {
        LegacyLineCluster? cluster = null;
        LegacyLineBounds? bounds = null;
        if (item.TryGetProperty("cluster", out var c)) cluster = new(c[0].GetInt32(), c[1].GetSingle(), c[2].GetSingle(), c[3].GetSingle());
        if (item.TryGetProperty("bounds", out var b)) bounds = new(b[0].GetSingle(), b[1].GetSingle(), b[2].GetSingle(), b[3].GetSingle());
        var actual = LegacyLineGeometry.Materialize(cluster, item.GetProperty("row").GetInt32(), bounds);
        var expected = item.GetProperty("expected");
        Require(actual.Baseline == expected[0].GetSingle() && actual.Height == expected[1].GetSingle(), $"Line mismatch {lines}.");
        lines++;
    }
    Require(scales > 0 && lines > 0, "Empty references.");
    Console.WriteLine($"PASS {scales} independent scale and {lines} line-materialization controls.");
}
var profile = new LegacyLineScaleProfile(.5f, .5f, 0, 1);
var fallback = LegacyLineScale.Select([], 0, 0, profile);
Require(fallback == new LegacyLineScales(1, 1), "Missing resolution fallback.");
float[] ownSamples = [9, 1, 5];
Require(LegacyLineScale.Select(ownSamples, 0, 0, profile) == new LegacyLineScales(5, 9), "Primary uses n-1 while secondary uses n.");
Require(ownSamples.SequenceEqual(new float[] { 9, 1, 5 }), "Caller samples mutated.");
Require(LegacyLineGeometry.Materialize(new(1, 100, 50, 20), 0) == new LegacyLinePair(100, 20), "Single row.");
Require(LegacyLineGeometry.Materialize(new(2, 100, 50, -1), 0, new(0, 100, 20, 40)) == new LegacyLinePair(130, 10), "Nonpositive cluster fallback.");
Throws<ArgumentException>(() => LegacyLineGeometry.Materialize(new(2, 100, 50, 20), 2));
Throws<ArgumentException>(() => LegacyLineGeometry.Materialize(null, 0));
Throws<ArgumentException>(() => LegacyLineGeometry.Materialize(null, 0, new(0, float.NaN, 10, 20)));
Throws<ArgumentException>(() => LegacyLineScale.Select([float.NaN], 0, 0, profile));
Throws<ArgumentException>(() => LegacyLineScale.Select([0], 0, 0, profile));
Throws<ArgumentException>(() => LegacyLineScale.Select([1], 0, 0, profile with { Quantile = -1 }));
Throws<ArgumentException>(() => LegacyLineScale.Select([1], 0, 0, profile with { AssociatedRecordWeight = 0 }));
Throws<OperationCanceledException>(() => LegacyLineScale.Select([1], 0, 0, profile, new(true)));
Console.WriteLine("PASS own scale, immutable input, row, fallback, bounds and cancellation checks. Automatic line fitting is not tested.");
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
static void Throws<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new InvalidOperationException($"Expected {typeof(T).Name}."); }
