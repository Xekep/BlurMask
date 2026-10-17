using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition.Math;
if (args.Length != 1) throw new ArgumentException("capture.json");
var rows = new List<Row>(); int guards = 0;
void Check(uint[] existing, uint[] initial, int[] map, float[] matrix)
{
    var result = LegacyPreparedMathExistingUnitCorrection.EvaluatePrepared(existing, initial, map, matrix);
    rows.Add(new(existing.ToArray(), initial.ToArray(), map.ToArray(), matrix.ToArray(), result.CopyUnits(),
        result.CopyAssociations(), result.CopyAveraged(), result.CopyMatchingRows(), result.CopyMatchingColumns()));
    Array.Fill(existing, 0xffffffff); Array.Fill(initial, 0xffffffff); Array.Fill(map, -1); Array.Fill(matrix, 7);
    var copy = result.CopyUnits(); Array.Fill(copy, 0xffffffff);
    if (!result.CopyUnits().SequenceEqual(rows[^1].Output)) throw new InvalidDataException("Mutable correction result.");
}
for (int trial = 0; trial < 2048; trial++)
{
    int unitCount = 1 + trial % 11, pairs = (trial / 11) % 8;
    int[] map = Enumerable.Range(0, unitCount).Where(i => (i + trial) % 3 != 0).ToArray();
    uint[] initial = Enumerable.Range(0, unitCount).Select(i => 0x01000000u | (uint)(42 + i)).ToArray();
    var source = new List<uint>();
    for (int pair = 0; pair < pairs; pair++)
    {
        source.Add((uint)(0x10000 * ((pair + trial) % 256) + 100 + pair));
        source.Add((uint)(0x10000 * ((pair + trial) % 256) + 100 + pair));
        if ((pair + trial) % 2 == 0) source.Add(0xabcd0123);
    }
    if (trial % 5 == 0) source.Add(0x0100010b);
    float[] matrix = Enumerable.Range(0, map.Length * source.Count).Select(i => trial % 4 == 0 ? .5f :
        (float)((i * 8191 + trial * 31) % 997) / 1024).ToArray();
    Check(source.ToArray(), initial, map, matrix);
}
void Reject(Action action)
{
    try { action(); } catch (Exception e) when (e is ArgumentException or NotSupportedException or OperationCanceledException) { guards++; return; }
    throw new InvalidDataException("Missing existing-unit correction guard.");
}
Reject(() => LegacyPreparedMathExistingUnitCorrection.EvaluatePrepared([], [], [], []));
Reject(() => LegacyPreparedMathExistingUnitCorrection.EvaluatePrepared([42], [43], [0], [.5f]));
Reject(() => LegacyPreparedMathExistingUnitCorrection.EvaluatePrepared([42, 0x0100002a], [43], [0], [.5f,.5f]));
Reject(() => LegacyPreparedMathExistingUnitCorrection.EvaluatePrepared([42,42], [43], [1], [.5f,.5f]));
Reject(() => LegacyPreparedMathExistingUnitCorrection.EvaluatePrepared([42,42], [43,44], [0,0], new float[4]));
Reject(() => LegacyPreparedMathExistingUnitCorrection.EvaluatePrepared([42,42], [43,44], [1,0], new float[4]));
Reject(() => LegacyPreparedMathExistingUnitCorrection.EvaluatePrepared([42,42], [43], [0], []));
foreach (float bad in new[] { float.NaN, float.PositiveInfinity, -.01f, 1.01f })
    Reject(() => LegacyPreparedMathExistingUnitCorrection.EvaluatePrepared([42,42], [43], [0], [bad,.5f]));
Reject(() => LegacyPreparedMathExistingUnitCorrection.EvaluatePrepared(new uint[6001], [43], [], []));
Reject(() => LegacyPreparedMathExistingUnitCorrection.EvaluatePrepared([], new uint[1001], [], []));
Reject(() => LegacyPreparedMathExistingUnitCorrection.EvaluatePrepared([], [43], [], [], new CancellationToken(true)));
File.WriteAllText(args[0], JsonSerializer.Serialize(new Capture(rows.ToArray(), guards), CaptureJson.Default.Capture));
Console.WriteLine($"PASS {rows.Count} prepared existing-unit corrections/{guards} guards; no actual C25 or EF4 owner.");
record Row(uint[] Existing,uint[] Initial,int[] Map,float[] Matrix,uint[] Output,uint[] Associations,
    float[] Average,int[] MatchingRows,int[] MatchingColumns);
record Capture(Row[] Rows,int Guards);
[JsonSerializable(typeof(Capture))]
internal partial class CaptureJson : JsonSerializerContext;
