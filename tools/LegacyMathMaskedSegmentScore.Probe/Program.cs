using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition.Math;

if (args.Length != 1) throw new ArgumentException("capture.json");
var rows = new List<Row>(); int guards = 0, noDeletion = 0;
var canceled = new CancellationToken(true);
void Reject(Action action)
{
    try { action(); }
    catch (Exception e) when (e is ArgumentException or NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new InvalidDataException("Required prepared score refusal absent.");
}
for (int trial = 0; trial < 4096; trial++)
{
    int symbols = 1 + trial % 19, segments = 1 + trial % 43;
    byte[] mask = Enumerable.Range(0, symbols).Select(i => (trial % 7) switch
    { 0 => (byte)0, 1 => (byte)255, _ => (byte)(((i + trial) % 3) == 0 ? (i % 2 == 0 ? 2 : 255) : 0) }).ToArray();
    int kept = mask.Count(v => v == 0);
    float[] probabilities = Enumerable.Range(0, kept * segments).Select(i => ((i + trial) % 9) switch
    {
        0 => 0f, 1 => -0f, 2 => 1e-12f, 3 => 1e-10f,
        4 => float.BitIncrement(1e-10f), 5 => .125f,
        6 => .5f, 7 => float.BitDecrement(1f), _ => 1f
    }).ToArray();
    int[] assignments = Enumerable.Range(0, segments).Select(i => (trial % 11) switch
    { 0 => -1, _ => (i + trial) % 5 == 0 ? -1 : (i * 7 + trial) % symbols }).ToArray();
    float before = (trial % 5) switch { 0 => 0f, 1 => -3.125f, 2 => 1e-7f, 3 => 91.125f, _ => -91.125f };
    float alpha = (trial % 7) switch { 0 => 0f, 1 => 1f, 2 => -1f, 3 => .75f, 4 => 1e-5f, 5 => 7.12345f, _ => -7.12345f };
    var result = LegacyPreparedMaskedMathSegmentScore.EvaluatePrepared(mask, probabilities, segments, assignments, before, alpha);
    int[] remap = result.CopyOriginalToCompressedRows(); float[] diagnostics = result.CopyDiagnosticValues();
    var captured = new Row(mask.ToArray(), probabilities.ToArray(), segments, assignments.ToArray(), before, alpha,
        result.Penalty, result.After, result.AssignedCount, result.ContributingCount, remap.ToArray(), diagnostics.ToArray());
    Array.Fill(mask, (byte)99); Array.Fill(probabilities, .333f); Array.Fill(assignments, -1);
    Array.Fill(remap, -100); Array.Fill(diagnostics, -100f);
    if (!result.CopyOriginalToCompressedRows().SequenceEqual(captured.Remap) ||
        !result.CopyDiagnosticValues().SequenceEqual(captured.Diagnostics)) throw new InvalidDataException("Mutable result storage.");
    if (captured.Mask.All(v => v == 0))
    {
        var fresh = LegacyPreparedFreshMathSegmentScore.EvaluatePrepared(captured.Probabilities,
            symbols, segments, captured.Assignments, before, alpha);
        if (BitConverter.SingleToInt32Bits(fresh.Penalty) != BitConverter.SingleToInt32Bits(result.Penalty) ||
            BitConverter.SingleToInt32Bits(fresh.After) != BitConverter.SingleToInt32Bits(result.After))
            throw new InvalidDataException("Old no-deletion numerical route changed.");
        noDeletion++;
    }
    rows.Add(captured);
}
Reject(() => LegacyPreparedMaskedMathSegmentScore.EvaluatePrepared([], [], 1, [-1], 0, 1));
Reject(() => LegacyPreparedMaskedMathSegmentScore.EvaluatePrepared(new byte[1001], [], 1, [-1], 0, 1));
Reject(() => LegacyPreparedMaskedMathSegmentScore.EvaluatePrepared([0], [], 0, [], 0, 1));
Reject(() => LegacyPreparedMaskedMathSegmentScore.EvaluatePrepared([0], [], 6001, [], 0, 1));
Reject(() => LegacyPreparedMaskedMathSegmentScore.EvaluatePrepared(new byte[1000], new float[1001000], 1001, new int[1001], 0, 1));
Reject(() => LegacyPreparedMaskedMathSegmentScore.EvaluatePrepared([0, 1], [.5f, .5f], 1, [0], 0, 1));
Reject(() => LegacyPreparedMaskedMathSegmentScore.EvaluatePrepared([0], [.5f], 1, [-2], 0, 1));
Reject(() => LegacyPreparedMaskedMathSegmentScore.EvaluatePrepared([0], [.5f], 1, [1], 0, 1));
Reject(() => LegacyPreparedMaskedMathSegmentScore.EvaluatePrepared([0], [.5f], 1, [], 0, 1));
foreach (float invalid in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity, -.1f, 1.1f })
    Reject(() => LegacyPreparedMaskedMathSegmentScore.EvaluatePrepared([0], [invalid], 1, [0], 0, 1));
foreach (float invalid in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
{
    Reject(() => LegacyPreparedMaskedMathSegmentScore.EvaluatePrepared([1], [], 1, [0], invalid, 1));
    Reject(() => LegacyPreparedMaskedMathSegmentScore.EvaluatePrepared([1], [], 1, [0], 0, invalid));
}
Reject(() => LegacyPreparedMaskedMathSegmentScore.EvaluatePrepared([0], [0], 1, [0], 0, float.MaxValue));
Reject(() => LegacyPreparedMaskedMathSegmentScore.EvaluatePrepared([0], [0], 1, [0], 0, 1, canceled));
Reject(() => LegacyPreparedMaskedMathSegmentScore.EvaluatePrepared([1], [], 1, [0], 0, 1, canceled));
File.WriteAllText(args[0], JsonSerializer.Serialize(new Capture(rows.ToArray(), noDeletion, guards), CaptureJson.Default.Capture));
Console.WriteLine($"PASS {rows.Count} prepared mask/remap scores; {noDeletion} exact old fresh controls; {guards} guards; no EF4 request dispatch or accepted formula.");
record Row(byte[] Mask, float[] Probabilities, int Segments, int[] Assignments, float Before, float Alpha,
    float Penalty, float After, int Assigned, int Contributing, int[] Remap, float[] Diagnostics);
record Capture(Row[] Rows, int NoDeletionControls, int Guards);
[JsonSerializable(typeof(Capture))]
internal partial class CaptureJson : JsonSerializerContext;
