using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
using InkRecognition.Math;

if (args.Length != 4) throw new ArgumentException("model.ikrmath rawMath2.res official-runtime capture.json");
using var runtime = new OrtRuntime(Path.GetFullPath(args[2]));
using var run = runtime.CreateRun(default);
byte[] resource = File.ReadAllBytes(args[1]);
using var stream = File.OpenRead(args[0]);
using var bundle = Math2Mode0ColdBundle.Read(runtime, stream, resource);
InkStroke[] ink = [new([new InkPoint(0, 30), new(0, 0), new(10, 18), new(20, 0), new(20, 30)]),
    new([new InkPoint(35, 0), new(35, 30)])];
int guards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception e) when (e is ArgumentException or InvalidOperationException or InvalidDataException or
        NotSupportedException or OperationCanceledException) { guards++; return; }
    throw new InvalidDataException("Required ordinary-output guard did not reject.");
}
(Math2Mode0RawOnlySource Source, Math2Mode0RawOnlyFinalOrdering Final) Prepare(int width)
{
    var source = width == 1 ? Math2Mode0RawOnlySource.PrepareColdDefault(runtime, bundle, run, ink)
        : Math2Mode0RawOnlySource.PrepareWithPreparedSearch(runtime, bundle, run, ink, width);
    try { source.RunToInitialOrder(); source.ApplyEf4(); return (source, source.OrderFinal()); }
    catch { source.Dispose(); throw; }
}
Row Project(Math2Mode0OutputRecord row) => new(row.Units, row.OldToNewMap, row.Runs,
    row.UnitRows.Select(unit => new Unit(unit.Unit, unit.Text.Identity, unit.ByteCount,
        unit.ScalarBits, unit.Flags, unit.Metadata, Convert.ToBase64String(unit.Text.ReadBytes()))).ToArray(),
    BitConverter.SingleToInt32Bits(row.Score), row.Library.LogicalIdentity);
using var canceled = new CancellationTokenSource(); canceled.Cancel();
var cases = new List<Actual>();
Math2Mode0RawOnlyOutputPolicy? actualPolicy = null;
long exactBudget = 0;
foreach (int width in new[] { 1, 3 })
{
    var (source, final) = Prepare(width);
    var original = final.CopyRecords();
    long finalReservation = final.CopyStatistics().PayloadReservation;
    Reject(() => final.FormatOrdinary(canceled.Token));
    Reject(() => final.FormatOrdinaryDiagnostic(-1, null));
    Reject(() => final.FormatOrdinaryDiagnostic(Math2Mode0RawOnlyFinalOrdering.DefaultPayloadBudget + 1, null));
    Reject(() => final.FormatOrdinaryDiagnostic(0, null));
    if (source.Loop.IsFailed || final.CopyRecords().Length != width || original.Any(row => row.Library.OutputStringCount != 0))
        throw new InvalidDataException("Preflight must preserve reusable owner and empty same-record pools.");
    Math2Mode0RawOnlyOutput? observed = null;
    var stages = new List<string>();
    var output = final.FormatOrdinaryDiagnostic(Math2Mode0RawOnlyFinalOrdering.DefaultPayloadBudget,
        (phase, index, current) => { observed = current; stages.Add($"{phase}:{index}"); });
    if (!ReferenceEquals(observed, output)) throw new InvalidDataException("Actual output claim required.");
    actualPolicy ??= output.Policy;
    if (width == 1) exactBudget = output.CopyStatistics().PayloadReservation;
    GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
    var rows = output.CopyRecords();
    for (int at = 0; at < rows.Length; at++)
    {
        if (!ReferenceEquals(rows[at].RecordIdentity, original[at].RecordIdentity) ||
            !ReferenceEquals(rows[at].Source, original[at].Source) || !ReferenceEquals(rows[at].Library, original[at].Library) ||
            !ReferenceEquals(rows[at].Source.ReadAttentionAlphabet(), output.Policy.Gate) ||
            BitConverter.SingleToInt32Bits(rows[at].Score) != BitConverter.SingleToInt32Bits(original[at].After))
            throw new InvalidDataException("Same actual Source/Record/Library/AL associations required.");
        foreach (var unit in rows[at].UnitRows)
        {
            int before = rows[at].Library.OutputStringCount;
            var duplicate = rows[at].Library.InternOutput(unit.Text.ReadBytes(), default);
            if (!ReferenceEquals(duplicate, unit.Text) || rows[at].Library.OutputStringCount != before)
                throw new InvalidDataException("Interning must reuse SAME actual record Library.");
        }
    }
    if (rows.Length > 1)
        foreach (var a in rows[0].UnitRows)
        foreach (var b in rows[1].UnitRows)
            if (a.Text.ReadBytes().SequenceEqual(b.Text.ReadBytes()) && ReferenceEquals(a.Text, b.Text))
                throw new InvalidDataException("Different actual record libraries must not share interned identity.");
    Reject(() => final.CopyRecords()); Reject(() => final.CopySortPermutation()); Reject(() => final.FormatOrdinary());
    var fake = new Math2Mode0RawOnlyOutput(final, output.Policy, original.Select(row => row.RecordIdentity).ToArray(), 0);
    Reject(() => fake.CopyRecords());
    Reject(() => final.Ef4.StoreOutputUnits(final, fake, original[0].RecordIdentity, []));
    Reject(() => final.Ef4.StoreOutputRuns(final, output, new object(), []));
    var (foreignSource, foreignFinal) = Prepare(1);
    using (foreignSource)
    {
        Reject(() => foreignFinal.ValidateOutput(output));
        Reject(() => final.Ef4.StoreOutputUnits(final, output, foreignFinal.CopyRecords()[0].RecordIdentity, []));
    }
    var expected = rows[0].Units.ToArray(); rows[0].Units[0] ^= 1; rows[0].OldToNewMap[0] = 999;
    rows[0].UnitRows[0] = rows[0].UnitRows[0] with { ScalarBits = 42 };
    if (!output.CopyRecords()[0].Units.SequenceEqual(expected) || output.CopyRecords()[0].OldToNewMap[0] != 0 ||
        output.CopyRecords()[0].UnitRows[0].ScalarBits != 0)
        throw new InvalidDataException("No mutable output payload alias.");
    var captured = output.CopyRecords().Select(Project).ToArray();
    var strings = output.CopyRecords().SelectMany(row => row.UnitRows).Select(unit => unit.Text).ToArray();
    var stats = output.CopyStatistics();
    source.Dispose(); source.Dispose();
    Reject(() => output.CopyRecords()); foreach (var text in strings) Reject(() => text.ReadBytes());
    if (original.Any(row => row.Library.ReleaseCount != 1 || !row.Library.IsPoolReleased || row.Library.OutputStringCount != 0))
        throw new InvalidDataException("Library and its lazy intern pool must release exactly once.");
    cases.Add(new(width, finalReservation, original.Select(row => BitConverter.SingleToInt32Bits(row.After)).ToArray(),
        original.Select(row => new[] { row.Primary.Length,
        row.Assignments.Length, row.InitialMask.Length, row.DeletionMask.Length }).ToArray(),
        original.Select(row => row.Units).ToArray(), original.Select(row => row.Runs).ToArray(),
        captured, stages.ToArray(), stats, original.Select(row => row.Library.ReleaseCount).ToArray()));
}
if (actualPolicy == null) throw new InvalidDataException("Actual policy owner required.");
var (structuredSource, structuredFinal) = Prepare(16);
using (structuredSource)
{
    var before = structuredFinal.CopyRecords();
    Reject(() => structuredFinal.FormatOrdinary());
    if (structuredSource.Loop.IsFailed || before.Any(row => row.Library.OutputStringCount != 0) ||
        !before.SelectMany(row => row.Units).Any(unit => (ushort)unit is 278 or 264))
        throw new InvalidDataException("Actual structured retained vector must refuse before mutation, with no filtering.");
    var after = structuredFinal.CopyRecords();
    for (int at = 0; at < before.Length; at++)
        if (!before[at].Units.SequenceEqual(after[at].Units) || !ReferenceEquals(before[at].RecordIdentity, after[at].RecordIdentity))
            throw new InvalidDataException("Whole-vector refusal must preserve actual retained identities and bytes.");
}
var failures = new List<Failure>();
foreach (var target in Enum.GetValues<Math2Mode0OutputStage>())
foreach (bool cancel in new[] { false, true })
{
    var (source, final) = Prepare(1); var original = final.CopyRecords();
    Math2Mode0RawOnlyOutput? observed = null; bool reached = false;
    using var stop = new CancellationTokenSource();
    Reject(() => final.FormatOrdinaryDiagnostic(Math2Mode0RawOnlyFinalOrdering.DefaultPayloadBudget,
        (phase, index, current) =>
        {
            observed = current; if (phase != target) return;
            reached = true; if (cancel) stop.Cancel(); else throw new InvalidDataException("Prepared output failure.");
        }, stop.Token));
    if (!reached || observed == null || !observed.CopyStatistics().Failed || !source.Loop.IsFailed)
        throw new InvalidDataException("Every selected consuming prefix must poison the entire owner.");
    Reject(() => observed.CopyRecords()); Reject(() => final.CopyRecords()); Reject(() => final.FormatOrdinary());
    int[] pools = original.Select(row => row.Library.OutputStringCount).ToArray();
    var stats = observed.CopyStatistics(); source.Dispose();
    if (original.Any(row => row.Library.ReleaseCount != 1 || row.Library.OutputStringCount != 0))
        throw new InvalidDataException("Fault prefix cleanup must release each actual Library once.");
    failures.Add(new(target.ToString(), cancel, pools, stats, original.Select(row => row.Library.ReleaseCount).ToArray()));
}
var (budgetSource, budgetFinal) = Prepare(1);
using (budgetSource)
{
    Reject(() => budgetFinal.FormatOrdinaryDiagnostic(exactBudget - 1, null));
    if (budgetSource.Loop.IsFailed) throw new InvalidDataException("Preflight does not consume.");
    if (budgetFinal.FormatOrdinaryDiagnostic(exactBudget, null).CopyStatistics().PayloadReservation != exactBudget)
        throw new InvalidDataException("Exact payload envelope must suffice.");
}
var (preusedSource, preusedFinal) = Prepare(1);
using (preusedSource)
{
    var row = preusedFinal.CopyRecords()[0];
    row.Library.InternOutput(new byte[] { 0x78 }, default); // Explicit prepared misuse, not native inference.
    Reject(() => preusedFinal.FormatOrdinary());
    if (preusedSource.Loop.IsFailed || preusedFinal.CopyRecords().Length != 1)
        throw new InvalidDataException("Preexisting library misuse refuses before output claim.");
}
var prepared = new List<Prepared>();
for (int flags = 0; flags < 8; flags++)
{
    uint[] units = [0x71000000u | (ushort)(actualPolicy.Zero + 1), 0x82000000u | actualPolicy.Dot,
        0x93000000u | (ushort)(actualPolicy.Zero + 2)];
    for (int at = 0; at < 3; at++) if ((flags & (1 << at)) != 0) units[at] |= 0x00010000;
    prepared.Add(new(flags, units, actualPolicy.NormalizeDecimalPrepared(units)));
}
foreach (uint unsupported in new uint[] { 0, 297, 298, 299, 65535, 0x0001002d, 278, 264 })
    Reject(() => actualPolicy.ValidateUnit(unsupported));
var capture = new Capture("1.23.2", Convert.ToHexStringLower(SHA256.HashData(resource)),
    cases.ToArray(), failures.ToArray(), prepared.ToArray(), guards);
File.WriteAllText(args[3], JsonSerializer.Serialize(capture, CaptureJson.Default.Capture));
Console.WriteLine($"PASS ordinary output {cases.Count} actual vectors/{cases.Sum(item => item.Rows.Length)} records; " +
    $"actual width16 whole-vector refusal; {failures.Count} consuming prefixes/{guards} guards; rich intervals, no formula");

record Unit(uint RawUnit, int StringIdentity, int ByteCount, uint ScalarBits, byte Flags, uint Metadata, string Utf8);
record Row(uint[] Units, ushort[] Map, MathSegmentRun[] Runs, Unit[] UnitRows, int ScoreBits, int LibraryIdentity);
record Actual(int Width, long FinalReservation, int[] OriginalScoreBits, int[][] WorkspaceLengths,
    uint[][] Before, MathSegmentRun[][] BeforeRuns, Row[] Rows, string[] Stages,
    Math2Mode0OutputStatistics Statistics, int[] Releases);
record Failure(string Stage, bool Cancel, int[] PoolCounts, Math2Mode0OutputStatistics Statistics, int[] Releases);
record Prepared(int Flags, uint[] Input, uint[] Output);
record Capture(string Runtime, string Resource, Actual[] Cases, Failure[] Failures, Prepared[] Prepared, int Guards);
[JsonSerializable(typeof(Capture))]
partial class CaptureJson : JsonSerializerContext { }
