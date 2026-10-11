using InkRecognition;
using System.Text.Json;
if (args.Length > 1) throw new ArgumentException("Supply at most one own line-sample control JSON.");
if (args.Length == 1)
{
    if (new FileInfo(args[0]).Length > 8 * 1024 * 1024) throw new ArgumentException("Control file exceeds 8 MiB.");
    using var document = JsonDocument.Parse(File.ReadAllBytes(args[0])); int count = 0;
    foreach (var item in document.RootElement.EnumerateArray())
    {
        var points = item.GetProperty("points").EnumerateArray().Select(p => new InkPoint(p[0].GetSingle(), p[1].GetSingle())).ToArray();
        bool half = item.GetProperty("half").GetBoolean();
        Require(LegacyLineScaleSamples.Extract(points, half).SequenceEqual(item.GetProperty("raw").EnumerateArray().Select(x => x.GetSingle())), $"Raw mismatch {count}.");
        Require(LegacyLineScaleSamples.PrepareAndExtract(points, half).SequenceEqual(item.GetProperty("prepared").EnumerateArray().Select(x => x.GetSingle())), $"Prepared mismatch {count}.");
        count++;
    }
    Require(count > 0, "Empty controls."); Console.WriteLine($"PASS {count * 2} independent raw/prepared Y-extremum sample comparisons.");
}
InkPoint[] triangle = [new(0, 0), new(3, 4), new(5, 4), new(9, 0)];
Require(LegacyLineScaleSamples.Extract(triangle).SequenceEqual(new float[] { 5, 4 }), "Plateau selects its last point.");
Require(LegacyLineScaleSamples.Extract(triangle, true).SequenceEqual(new float[] { 4, 4 }), "Half-X chord profile.");
Require(LegacyLineScaleSamples.Extract([new(0, 0), new(10, 0)]).Length == 0, "Horizontal endpoint is omitted.");
Require(LegacyLineScaleSamples.Extract([new(0, 0), new(1, 5), new(10, 5)]).SequenceEqual(new float[] { 10 }), "Horizontal final plateau retains full endpoint X.");
Require(triangle.SequenceEqual(new InkPoint[] { new(0, 0), new(3, 4), new(5, 4), new(9, 0) }), "Caller points mutated.");
Throws<ArgumentException>(() => LegacyLineScaleSamples.Extract([new(float.NaN, 0)]));
Throws<ArgumentException>(() => LegacyLineScaleSamples.PrepareAndExtract([new(0, float.PositiveInfinity)]));
Throws<OperationCanceledException>(() => LegacyLineScaleSamples.Extract([], false, new(true)));
Console.WriteLine("PASS own plateaus, endpoints, half-X weighting, immutable input, finite bounds and cancellation.");
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
static void Throws<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new InvalidOperationException($"Expected {typeof(T).Name}."); }
