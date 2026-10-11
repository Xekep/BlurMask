using InkRecognition;
using System.Text.Json;
if (args.Length is not (1 or 3)) throw new ArgumentException("Supply independent pair-feature controls and optionally two embedded classifier .res files.");
if (new FileInfo(args[0]).Length > 8 * 1024 * 1024) throw new ArgumentException("Control file exceeds 8 MiB.");
PreparedLineBoundaryClassifier[] models = [];
if (args.Length == 3)
{
    using var first = File.OpenRead(args[1]); using var second = File.OpenRead(args[2]);
    models = [PreparedLineBoundaryClassifier.Load(first), PreparedLineBoundaryClassifier.Load(second)];
}
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0])); int count = 0, statistics = 0, channels = 0;
foreach (var item in document.RootElement.EnumerateArray())
{
    var strokePoints = item.GetProperty("strokes").EnumerateArray().Select(s => s.EnumerateArray().Select(p => new InkPoint(p[0].GetSingle(), p[1].GetSingle())).ToArray()).ToArray();
    var records = strokePoints.Select(s => LegacyLineStrokeStatistics.Extract(s)).ToArray();
    var expectedStatistics = item.GetProperty("statistics").EnumerateArray().ToArray();
    for (int i = 0; i < records.Length; i++)
    {
        float[] actual = records[i].ToArray(); var expected = expectedStatistics[i];
        for (int j = 0; j < actual.Length; j++) Equal(actual[j], expected[j].GetSingle(), $"Stroke statistic {count}:{i}:{j}.");
        statistics++;
    }
    int[] indices = item.GetProperty("indices").EnumerateArray().Select(x => x.GetInt32()).ToArray();
    int[] skipped = item.GetProperty("skipped").EnumerateArray().Select(x => x.GetInt32()).ToArray();
    float[] values = LegacyLinePairFeatures.Extract(records, indices, item.GetProperty("width").GetSingle(), item.GetProperty("scale").GetSingle(), skipped);
    var reference = item.GetProperty("expected"); Require(values.Length == reference.GetArrayLength(), "Feature matrix shape.");
    for (int i = 0; i < values.Length; i++) { Equal(values[i], reference[i].GetSingle(), $"Feature {count}:{i}."); channels++; }
    if (count < 20) for (int modelIndex = 0; modelIndex < models.Length; modelIndex++)
    {
        float[] probabilities = models[modelIndex].Score(values, indices.Length);
        Require(probabilities.Length == indices.Length * 2 && probabilities.All(p => float.IsFinite(p) && p is >= 0 and <= 1), "Prepared ink-to-neural chain.");
        if (item.TryGetProperty("neural", out var expectedNeural))
            for (int i = 0; i < probabilities.Length; i++)
                Require(MathF.Abs(probabilities[i] - expectedNeural[modelIndex][i].GetSingle()) <= 2e-4f, $"Prepared ink-to-neural reference mismatch {count}:{modelIndex}:{i}.");
    }
    count++;
}
float[] ownFields = new float[17]; ownFields[2] = 10; ownFields[3] = 10;
var own = new LegacyLineStrokeStatistics(ownFields); ownFields[2] = 999;
Require(own.ToArray()[2] == 10, "Stored statistics alias caller fields.");
Throws<ArgumentException>(() => new LegacyLineStrokeStatistics(new float[16]));
Throws<ArgumentException>(() => LegacyLineStrokeStatistics.Extract([]));
Throws<ArgumentException>(() => LegacyLineStrokeStatistics.Extract([new(float.NaN, 0)]));
Throws<OperationCanceledException>(() => LegacyLineStrokeStatistics.Extract([new(0, 0)], new(true)));
Throws<ArgumentException>(() => LegacyLinePairFeatures.Extract([own], [0], 0, 10));
Throws<ArgumentException>(() => LegacyLinePairFeatures.Extract([own], [1], 10, 10));
Throws<ArgumentException>(() => LegacyLinePairFeatures.Extract([own], [0], 10, 10, [0, 0]));
Throws<OperationCanceledException>(() => LegacyLinePairFeatures.Extract([own], [0], 10, 10, default, new(true)));
Require(count > 0, "Empty references.");
Console.WriteLine($"PASS {statistics} independent prepared stroke records and {channels} pair-feature channels; {models.Length} real neural models; immutable records, bounds and cancellation.");
static void Equal(float actual, float expected, string message) { Require(float.IsFinite(actual) && MathF.Abs(actual-expected) <= 2e-5f * MathF.Max(1, MathF.Abs(expected)), message + $" {actual} vs {expected}"); }
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
static void Throws<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new InvalidOperationException($"Expected {typeof(T).Name}."); }
