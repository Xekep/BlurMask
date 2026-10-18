using System.Security.Cryptography;
using System.Buffers.Binary;
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
    throw new InvalidDataException("Required final-ordering guard did not reject.");
}
(Math2Mode0RawOnlySource Source, Math2Mode0CompletedRecords Vector, Math2Mode0RawOnlyEf4 Ef4) Prepare(int width)
{
    var source = width == 1 ? Math2Mode0RawOnlySource.PrepareColdDefault(runtime, bundle, run, ink)
        : Math2Mode0RawOnlySource.PrepareWithPreparedSearch(runtime, bundle, run, ink, width);
    try
    {
        source.RunToInitialOrder();
        var vector = (Math2Mode0CompletedRecords)source.ReadInitialForDiagnostic().VectorIdentity;
        return (source, vector, source.ApplyEf4());
    }
    catch { source.Dispose(); throw; }
}
Row Project(Math2Mode0RawOnlyEf4RecordCopy row, int ordinal) => new(ordinal,
    BitConverter.SingleToInt32Bits(row.After), row.Primary, row.Segments, row.Units,
    row.Assignments, row.Runs, row.InitialMask, row.DeletionMask, row.Source.Offset,
    row.Source.CompletedSourceParent, Convert.ToBase64String(row.Source.CopyRawRecord()));
int[] ReadOrder(Math2Mode0CompletedRecords vector, Math2Mode0RawOnlyEf4RecordCopy[] original)
    => Enumerable.Range(0, vector.Count).Select(at => Array.FindIndex(original,
        row => ReferenceEquals(row.Source, vector.ReadSourceEvent(at)))).ToArray();
using var canceled = new CancellationTokenSource(); canceled.Cancel();
var cases = new List<Actual>();
foreach (int width in new[] { 1, 3, 16 })
{
    var (source, vector, ef4) = Prepare(width);
    var before = ef4.CopyRecords();
    long ef4Reservation = ef4.CopyStatistics().PlannedPayloadEnvelope;
    Reject(() => source.OrderFinal(canceled.Token));
    Reject(() => source.OrderFinalDiagnostic(-1, null));
    Reject(() => source.OrderFinalDiagnostic(Math2Mode0RawOnlyFinalOrdering.DefaultPayloadBudget + 1, null));
    Reject(() => source.OrderFinalDiagnostic(0, null));
    if (source.Loop.IsFailed || ef4.CopyRecords().Length != before.Length)
        throw new InvalidDataException("Preflight budget/cancellation must preserve reuse.");
    var moves = new List<int[]>();
    var releaseCounts = new List<int[]>();
    var final = source.OrderFinalDiagnostic(Math2Mode0RawOnlyFinalOrdering.DefaultPayloadBudget,
        (phase, index, current) =>
        {
            if (phase == Math2Mode0FinalOrderingStage.AfterOwnerMove) moves.Add(ReadOrder(vector, before));
            if (phase == Math2Mode0FinalOrderingStage.AfterRelease)
            {
                if (vector.Count != before.Length) throw new InvalidDataException("Native count changes only after all tail destruction.");
                releaseCounts.Add(before.Select(row => row.Library.ReleaseCount).ToArray());
            }
        });
    var after = final.CopyRecords();
    int[] permutation = final.CopySortPermutation();
    for (int at = 0; at < after.Length; at++)
    {
        var prior = before[permutation[at]];
        if (!ReferenceEquals(after[at].RecordIdentity, prior.RecordIdentity) ||
            !ReferenceEquals(after[at].Source, prior.Source) || !ReferenceEquals(after[at].Library, prior.Library) ||
            !ReferenceEquals(after[at].SourceFactoryIdentity, prior.SourceFactoryIdentity) || prior.Library.IsDisposed)
            throw new InvalidDataException("Actual Record/Source/Library/payload associations must remain intact.");
    }
    Reject(() => ef4.CopyRecords());
    Reject(() => source.OrderFinal());
    var foreign = new Math2Mode0RawOnlyFinalOrdering(ef4, before.Length, width, 0);
    Reject(() => foreign.CopyRecords());
    Reject(() => ef4.CopyFinalRecord(foreign, before[0].RecordIdentity));
    var (otherSource, _, otherEf4) = Prepare(width);
    using (otherSource)
        Reject(() => ef4.CopyFinalRecord(final, otherEf4.CopyRecords()[0].RecordIdentity));
    if (before.Length > width)
        Reject(() => ef4.CopyFinalRecord(final, before[permutation[width]].RecordIdentity));
    if (after.Length != System.Math.Min(width, before.Length) || final.IsAcceptedFormula)
        throw new InvalidDataException("Only actual width tail truncation, never accepted formula.");
    int[] detached = final.CopySortPermutation(); if (detached.Length != 0) detached[0] = -1;
    if (!final.CopySortPermutation().SequenceEqual(permutation)) throw new InvalidDataException("Mutable order alias.");
    if (after.Length > 0 && after[0].Units.Length > 0)
    {
        uint expected = after[0].Units[0]; after[0].Units[0] ^= 17;
        if (final.CopyRecords()[0].Units[0] != expected) throw new InvalidDataException("Mutable final payload alias.");
    }
    var projected = final.CopyRecords().Select((row, at) => Project(row, permutation[at])).ToArray();
    var stats = final.CopyStatistics();
    int[] releasesBeforeDispose = before.Select(row => row.Library.ReleaseCount).ToArray();
    source.Dispose(); source.Dispose();
    Reject(() => final.CopyRecords()); Reject(() => final.CopySortPermutation());
    if (before.Any(row => row.Library.ReleaseCount != 1 || !row.Library.IsPoolReleased))
        throw new InvalidDataException("Every original Library must be released exactly once.");
    cases.Add(new(width, ef4Reservation, before.Select(Project).ToArray(), projected, permutation, moves.ToArray(),
        releaseCounts.ToArray(), releasesBeforeDispose, before.Select(row => row.Library.ReleaseCount).ToArray(), stats));
}

var failures = new List<Failure>();
foreach (var target in Enum.GetValues<Math2Mode0FinalOrderingStage>())
foreach (bool cancel in new[] { false, true })
{
    var (source, vector, ef4) = Prepare(16);
    var before = ef4.CopyRecords();
    Math2Mode0RawOnlyFinalOrdering? observed = null;
    bool reached = false;
    using var stop = new CancellationTokenSource();
    Reject(() => source.OrderFinalDiagnostic(Math2Mode0RawOnlyFinalOrdering.DefaultPayloadBudget,
        (phase, index, current) =>
        {
            observed = current;
            if (phase != target) return;
            reached = true;
            if (cancel) stop.Cancel(); else throw new InvalidDataException("Prepared final-ordering failure.");
        }, stop.Token));
    if (!reached || observed == null || !observed.CopyStatistics().Failed || !source.Loop.IsFailed)
        throw new InvalidDataException("Every selected logical failure boundary must be reached and poison its owner.");
    Reject(() => observed.CopyRecords()); Reject(() => ef4.CopyRecords()); Reject(() => source.OrderFinal());
    int[] retainedOrder = ReadOrder(vector, before);
    int[] releases = before.Select(row => row.Library.ReleaseCount).ToArray();
    var statistics = observed.CopyStatistics();
    source.Dispose();
    if (before.Any(row => row.Library.ReleaseCount != 1)) throw new InvalidDataException("Failure cleanup lost/duplicated Library ownership.");
    failures.Add(new(target.ToString(), cancel, before.Select(Project).ToArray(), retainedOrder, releases,
        before.Select(row => row.Library.ReleaseCount).ToArray(), statistics));
}
using (var source = Prepare(1).Source)
{
    long exact = cases[0].Statistics.PayloadReservation;
    Reject(() => source.OrderFinalDiagnostic(exact - 1, null));
    if (source.Loop.IsFailed) throw new InvalidDataException("Budget preflight must not claim/poison.");
    var result = source.OrderFinalDiagnostic(exact, null);
    if (result.CopyStatistics().PayloadReservation != exact) throw new InvalidDataException("Exact reservation must suffice.");
}
var prepared = new List<Prepared>();
int[] counts = [0, 1, 2, 19, 20, 21, 22, 41, 42, 64];
uint[] bits = [0xff800000, 0xbf800000, 0x80000000, 0, 0x3f800000, 0x7f7fffff, 0x7f800000, 0x7fc00001];
for (int control = 0; control < 256; control++)
{
    int count = counts[control % counts.Length], width = 1 + control % 16;
    byte[] rows = new byte[count * 168];
    for (int at = 0; at < count; at++)
    {
        var row = rows.AsSpan(at * 168, 168);
        for (int item = 0; item < 168; item++) row[item] = unchecked((byte)(control * 17 + at * 31 + item));
        BinaryPrimitives.WriteUInt32LittleEndian(row, control % 3 == 0 ? 0x3f800000u : bits[(at + control) % bits.Length]);
        BinaryPrimitives.WriteInt32LittleEndian(row[8..], at);
    }
    byte[] sorted = count > 1 ? LegacyPreparedMathCandidateOrdering.Sort(rows) : rows.ToArray();
    prepared.Add(new(count, width, Convert.ToBase64String(rows), Convert.ToBase64String(sorted)));
}
var capture = new Capture("1.23.2", Convert.ToHexString(SHA256.HashData(resource)).ToLowerInvariant(),
    cases.ToArray(), failures.ToArray(), prepared.ToArray(), guards);
File.WriteAllText(args[3], JsonSerializer.Serialize(capture, CaptureJson.Default.Capture));
Console.WriteLine($"PASS3 actual EF4 final-order vectors/{cases.Sum(item => item.Before.Length)} records; " +
    $"{cases.Sum(item => item.Statistics.ReleasedOriginalOrdinals.Length)} actual tail releases; {failures.Count} logical failures/{guards} guards; no formula/output");

record Row(int Ordinal, int ScoreBits, ushort[] Primary, int Segments, uint[] Units, int[] Assignments,
    MathSegmentRun[] Runs, byte[] InitialMask, byte[] DeletionMask, long SourceOffset, int SourceParent, string Raw48);
record Actual(int Width, long Ef4Reservation, Row[] Before, Row[] After, int[] Permutation, int[][] Moves, int[][] Releases,
    int[] BeforeDispose, int[] AfterDispose, Math2Mode0FinalOrderingStatistics Statistics);
record Failure(string Stage, bool Cancel, Row[] Before, int[] RetainedOrder, int[] Releases,
    int[] AfterDispose, Math2Mode0FinalOrderingStatistics Statistics);
record Prepared(int Count, int Width, string Input, string Output);
record Capture(string Runtime, string Resource, Actual[] Cases, Failure[] Failures, Prepared[] Prepared, int Guards);
[JsonSerializable(typeof(Capture))]
partial class CaptureJson : JsonSerializerContext { }
