using InkRecognition;
using System.Text.Json;

if (args.Length != 1) throw new ArgumentException("Supply independent proposal/neural controls.");
if (new FileInfo(args[0]).Length > 8 * 1024 * 1024) throw new ArgumentException("Oversized controls.");
using var doc = JsonDocument.Parse(File.ReadAllBytes(args[0]));
var models = doc.RootElement.GetProperty("models").EnumerateArray().Select(pair => pair.EnumerateArray().Select(path =>
{
    using var stream = File.OpenRead(path.GetString()!); return PreparedLineBoundaryClassifier.Load(stream);
}).ToArray()).ToArray();
int cases = 0, proposals = 0, neuralPairs = 0;
foreach (var item in doc.RootElement.GetProperty("controls").EnumerateArray())
{
    var records = item.GetProperty("strokes").EnumerateArray().Select(s => LegacyLineStrokeStatistics.Extract(
        s.EnumerateArray().Select(p => new InkPoint(p[0].GetSingle(), p[1].GetSingle())).ToArray())).ToArray();
    bool[] active = item.GetProperty("active").EnumerateArray().Select(x => x.GetBoolean()).ToArray();
    var batch = LegacyNeuralLineProposals.Prepare(records, active, item.GetProperty("nominalWidth").GetSingle(), item.GetProperty("overlap").GetSingle());
    float expectedWidth = item.GetProperty("windowWidth").GetSingle();
    Require(MathF.Abs(batch.WindowWidth - expectedWidth) <= 1e-5f * MathF.Max(1, MathF.Abs(expectedWidth)), "Window width.");
    var expected = item.GetProperty("proposals").EnumerateArray().ToArray();
    Require(batch.Proposals.Count == expected.Length, $"Proposal count {cases}.");
    for (int i = 0; i < expected.Length; i++)
    {
        Require(batch.Proposals[i].WindowIndex == expected[i].GetProperty("window").GetInt32() &&
            batch.Proposals[i].Ordering == expected[i].GetProperty("ordering").GetInt32() &&
            batch.Proposals[i].StrokeIndices.Span.SequenceEqual(expected[i].GetProperty("ids").EnumerateArray().Select(x => x.GetInt32()).ToArray()),
            $"Proposal order {cases}:{i}."); proposals++;
    }
    if (item.TryGetProperty("neural", out var neural))
        for (int m = 0; m < models.Length; m++)
        {
            var actual = LegacyNeuralLineProposals.Score(records, batch, item.GetProperty("scale").GetSingle(), models[m][0], models[m][1]);
            var expectedPairs = neural[m].EnumerateArray().ToArray();
            Require(actual.Length == expectedPairs.Length, "Aggregated neural pair count.");
            for (int i = 0; i < actual.Length; i++)
            {
                var p = expectedPairs[i];
                Require(actual[i].FirstStroke == p[0].GetInt32() && actual[i].SecondStroke == p[1].GetInt32() &&
                    actual[i].LabelOne == p[3].GetBoolean() && MathF.Abs(actual[i].Confidence - p[2].GetSingle()) <= 2e-4f,
                    $"Neural pair {cases}:{m}:{i}."); neuralPairs++;
            }
        }
    cases++;
}
var merged = LegacyNeuralLineProposals.Aggregate([new(3, 1, .8f, true), new(1, 3, .8f, false), new(2, 1, .7f, false), new(3, 1, .9f, false)]);
Require(merged.SequenceEqual(new LegacyNeuralLinePair[] { new(1, 3, .9f, false), new(1, 2, .7f, false) }), "Unordered identity, ties and first order.");
var field = new float[17]; field[2] = 10; field[3] = 10;
var record = new LegacyLineStrokeStatistics(field);
Throws<ArgumentException>(() => LegacyNeuralLineProposals.Prepare([record], [false], 100, .75f));
Throws<ArgumentException>(() => LegacyNeuralLineProposals.Prepare([record], [true], 100, 1));
Throws<ArgumentException>(() => LegacyNeuralLineProposals.Prepare([record], [true], 0, .75f));
Throws<ArgumentException>(() => LegacyNeuralLineProposals.Aggregate([new(0, 0, .8f, false)]));
Throws<ArgumentException>(() => LegacyNeuralLineProposals.Aggregate([new(0, 1, float.NaN, false)]));
Throws<OperationCanceledException>(() => LegacyNeuralLineProposals.Prepare([record], [true], 100, .75f, new(true)));
Throws<OperationCanceledException>(() => LegacyNeuralLineProposals.Aggregate([], new(true)));
Throws<ArgumentException>(() => LegacyNeuralLineProposals.Score([record], new(10, [new(0, 3, new int[] { 1 })]), 10, models[0][0], models[0][1]));
Throws<ArgumentException>(() => LegacyNeuralLineProposals.Score([record], new(10, []), 10, models[0][1], models[0][0]));
Throws<OperationCanceledException>(() => LegacyNeuralLineProposals.Score([record], new(10, []), 10, models[0][0], models[0][1], new(true)));
Console.WriteLine($"PASS 2e3 proposal/skip-first/neural chain: {cases} cases, {proposals} native proposals, {neuralPairs} independently scored pairs.");

static void Require(bool condition, string text) { if (!condition) throw new InvalidOperationException(text); }
static void Throws<T>(Action action) where T : Exception
{
    try { action(); } catch (T) { return; }
    throw new InvalidOperationException($"Expected {typeof(T).Name}.");
}
