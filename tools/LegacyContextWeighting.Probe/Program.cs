using InkRecognition;
using System.Text.Json;
if (args.Length > 1) throw new ArgumentException("Supply at most one own context-control JSON.");
if (args.Length == 1)
{
    if (new FileInfo(args[0]).Length > 8 * 1024 * 1024) throw new ArgumentException("Controls exceed8MiB.");
    using var document = JsonDocument.Parse(File.ReadAllBytes(args[0])); int count = 0;
    foreach (var item in document.RootElement.EnumerateArray())
    {
        var observations = item.GetProperty("observations").EnumerateArray().Select(o => new LegacyContextObservation(
            o.GetProperty("x").GetSingle(), o.GetProperty("probabilities").EnumerateArray().Select(p => p.GetSingle()).ToArray())).ToArray();
        var actual = LegacyContextWeighting.Combine(observations, item.GetProperty("target").GetSingle(), item.GetProperty("height").GetSingle(),
            item.GetProperty("offset").GetSingle(), item.GetProperty("left").GetSingle(), item.GetProperty("right").GetSingle());
        var expected = item.GetProperty("expected").EnumerateArray().Select(p => p.GetSingle()).ToArray();
        Require(expected.Length == 28 && actual.Zip(expected).All(pair => Math.Abs(pair.First - pair.Second) <= 2e-7f), $"Context comparison failed{count}."); count++;
    }
    Require(count > 0, "Empty controls."); Console.WriteLine($"PASS {count} independent float32 context-weighting comparisons (absolute tolerance2e-7).");
}
var probabilities = Enumerable.Repeat(.25f, 28).ToArray(); probabilities[0] = 1;
var first = new LegacyContextObservation(0, probabilities);
Require(LegacyContextWeighting.Combine([], 0, 10, 0, 1, 1).All(x => x == -1), "Empty context must be−1.");
var exact = LegacyContextWeighting.Combine([first], 0, 10, 0, 1, 1);
Require(exact[0] == 1 && exact[1] == -.5f, "Zero-distance channel conversion failed.");
Require(exact.SequenceEqual(LegacyContextWeighting.Combine([first, first], 0, 10, 0, 1, 1)), "Duplicate observations must max, not sum.");
Require(exact.SequenceEqual(LegacyContextWeighting.Combine([new(-10, probabilities)], 0, 10, 2, 1, 1)), "Left gap positive overlap plateau failed.");
var translated = LegacyContextWeighting.Combine([new(100, probabilities)], 100, 10, 0, 1, 1);
Require(exact.SequenceEqual(translated), "Context translation invariance failed.");
Require(LegacyContextWeighting.Combine([new(1000, probabilities)], 0, 10, 0, 1, 1).All(x => x == -1), "Exponent cutoff failed.");
var nearZero = Enumerable.Repeat(.004f, 28).ToArray();
Require(LegacyContextWeighting.Combine([new(0, nearZero)], 0, 10, 0, 1, 1).All(x => x == -1), "Near−1 snap failed.");
Throws<ArgumentException>(() => LegacyContextWeighting.Combine([new(0, new float[27])], 0, 10, 0, 1, 1));
Throws<ArgumentException>(() => LegacyContextWeighting.Combine([], 0, 0, 0, 1, 1));
Throws<ArgumentException>(() => LegacyContextWeighting.Combine([], float.NaN, 10, 0, 1, 1));
Throws<ArgumentException>(() => LegacyContextWeighting.Combine([], 0, 10, 0, -1, 1));
Throws<ArgumentException>(() => LegacyContextWeighting.Combine(Enumerable.Repeat(first, 1025).ToArray(), 0, 10, 0, 1, 1));
probabilities[1] = float.NaN;
Throws<ArgumentException>(() => LegacyContextWeighting.Combine([first], 0, 10, 0, 1, 1));
Throws<OperationCanceledException>(() => LegacyContextWeighting.Combine([], 0, 10, 0, 1, 1, new CancellationToken(true)));
Console.WriteLine("PASS own empty context, max aggregation, asymmetric plateau, translation, exponent cutoff, snap, validation/work cap/cancellation. Prior selection/30-feature extraction and handwriting accuracy are not tested.");
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
static void Throws<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new InvalidOperationException($"Expected{typeof(T).Name}."); }
