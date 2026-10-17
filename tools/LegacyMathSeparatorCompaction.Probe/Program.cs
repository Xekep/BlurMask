using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition.Math;
if (args.Length != 1) throw new ArgumentException("capture.json");
var rows = new List<Row>(); int guards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception e) when (e is ArgumentException or OperationCanceledException) { guards++; return; }
    throw new InvalidDataException("Required separator compaction refusal absent.");
}
void Check(uint[] units, ushort marker, int[] assignments)
{
    var result = LegacyPreparedMathSeparatorCompaction.EvaluatePrepared(units, marker, assignments);
    var output = result.CopyUnits(); var mapped = result.CopyAssignments(); var map = result.CopyOldToNew();
    rows.Add(new(units.ToArray(), marker, assignments.ToArray(), output.ToArray(), mapped.ToArray(), map.ToArray()));
    Array.Fill(units, 0xffffffff); Array.Fill(assignments, -1);
    Array.Fill(output, 0xffffffff); Array.Fill(mapped, -999); Array.Fill(map, -999);
    var row = rows[^1];
    if (!row.Output.SequenceEqual(result.CopyUnits()) || !row.Mapped.SequenceEqual(result.CopyAssignments()) ||
        !row.Map.SequenceEqual(result.CopyOldToNew())) throw new InvalidDataException("Mutable compaction result.");
    var repeated = LegacyPreparedMathSeparatorCompaction.EvaluatePrepared(row.Output, marker, row.Mapped);
    if (!row.Output.SequenceEqual(repeated.CopyUnits()) || !row.Mapped.SequenceEqual(repeated.CopyAssignments()))
        throw new InvalidDataException("Compaction is not idempotent.");
}
foreach (ushort marker in new ushort[] { 0, 267, 65535 })
for (int length = 0; length <= 10; length++)
for (int pattern = 0; pattern < (1 << length); pattern++)
{
    uint[] units = Enumerable.Range(0, length).Select(i =>
    {
        ushort label = (pattern & (1 << i)) != 0 ? marker : (ushort)(marker == 0 ? 1 + i % 3 : i % 3);
        return (uint)label | ((uint)((i * 7919 + pattern * 37) & 65535) << 16);
    }).ToArray();
    int[] assignments = Enumerable.Range(-1, length + 1).Concat(Enumerable.Range(0, length).Reverse()).ToArray();
    Check(units, marker, assignments);
}
Check(Enumerable.Repeat(0xabcd010bu, 1000).ToArray(), 267, Enumerable.Range(0, 6000).Select(i => i % 1000).ToArray());
Check(Enumerable.Range(0, 1000).Select(i => 0xabcd0000u | (uint)(i % 2 == 0 ? 267 : 42)).ToArray(), 267,
    Enumerable.Range(0, 6000).Select(i => i % 1000).ToArray());
Reject(() => LegacyPreparedMathSeparatorCompaction.EvaluatePrepared(new uint[1001], 0, []));
Reject(() => LegacyPreparedMathSeparatorCompaction.EvaluatePrepared([42], 0, new int[6001]));
Reject(() => LegacyPreparedMathSeparatorCompaction.EvaluatePrepared([], 0, [0]));
Reject(() => LegacyPreparedMathSeparatorCompaction.EvaluatePrepared([42], 0, [1]));
Reject(() => LegacyPreparedMathSeparatorCompaction.EvaluatePrepared([42], 0, [-2]));
Reject(() => LegacyPreparedMathSeparatorCompaction.EvaluatePrepared([42], 0, [0], new CancellationToken(true)));
File.WriteAllText(args[0], JsonSerializer.Serialize(new Capture(rows.ToArray(), guards), CaptureJson.Default.Capture));
Console.WriteLine($"PASS {rows.Count} prepared four-byte separator/assignment controls; {guards} guards; no request producer or EF4 publication.");
record Row(uint[] Input, ushort Marker, int[] Assignments, uint[] Output, int[] Mapped, int[] Map);
record Capture(Row[] Rows, int Guards);
[JsonSerializable(typeof(Capture))]
internal partial class CaptureJson : JsonSerializerContext;
