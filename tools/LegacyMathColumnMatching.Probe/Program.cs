using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition.Math;
if (args.Length != 1) throw new ArgumentException("capture.json");
var rows = new List<Row>(); int guards = 0;
void Check(float[] input, int n, int m)
{
    var result = LegacyPreparedMathColumnMatching.EvaluatePrepared(input, n, m);
    rows.Add(new(input.ToArray(), n, m, result.CopyRows(), result.CopyColumns(), result.CopyAdjusted(), result.Rounds));
    Array.Fill(input, 123); var copy = result.CopyRows(); Array.Fill(copy, -99);
    if (!result.CopyRows().SequenceEqual(rows[^1].SelectedRows)) throw new InvalidDataException("Mutable matching result.");
}
void Reject(Action action)
{
    try { action(); } catch (Exception e) when (e is ArgumentException or OperationCanceledException) { guards++; return; }
    throw new InvalidDataException("Missing matching guard.");
}
Check([], 0, 7); Check([], 7, 0); Check([1, 4, 2, 3], 2, 2);
Check([0, 1, 2, 0, 0, 0, 0, 0, 0], 3, 3);
for (int trial = 0; trial < 4096; trial++)
{
    int n = 1 + trial % 13, m = 1 + (trial / 13) % 17;
    float[] values = Enumerable.Range(0, n * m).Select(i => ((trial % 5) switch
    { 0 => 0f, 1 => (float)(i / m), 2 => (float)(i % m), 3 => (float)((i * 8191 + trial * 31) % 101) / 128,
      _ => (float)((i * 7919 + trial * 37) % 53 - 26) / 32 })).ToArray();
    Check(values, n, m);
}
Reject(() => LegacyPreparedMathColumnMatching.EvaluatePrepared([], -1, 0));
Reject(() => LegacyPreparedMathColumnMatching.EvaluatePrepared([], 1001, 0));
Reject(() => LegacyPreparedMathColumnMatching.EvaluatePrepared([], 0, 3001));
Reject(() => LegacyPreparedMathColumnMatching.EvaluatePrepared([], 1000, 3000));
Reject(() => LegacyPreparedMathColumnMatching.EvaluatePrepared([1], 2, 1));
foreach (float bad in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity, float.MaxValue, -float.MaxValue })
    Reject(() => LegacyPreparedMathColumnMatching.EvaluatePrepared([bad], 1, 1));
Reject(() => LegacyPreparedMathColumnMatching.EvaluatePrepared([1], 1, 1, new CancellationToken(true)));
File.WriteAllText(args[0], JsonSerializer.Serialize(new Capture(rows.ToArray(), guards), CaptureJson.Default.Capture));
Console.WriteLine($"PASS {rows.Count} prepared column matching controls/{guards} guards; no source correction or record publication.");
record Row(float[] Input, int N, int M, int[] SelectedRows, int[] SelectedColumns, float[] Adjusted, int Rounds);
record Capture(Row[] Rows, int Guards);
[JsonSerializable(typeof(Capture))]
internal partial class CaptureJson : JsonSerializerContext;
