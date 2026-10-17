using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
using InkRecognition.Math;

if (args.Length != 4) throw new ArgumentException("model.ikrmath rawMath2.res official-runtime capture.json");
if (!BitConverter.IsLittleEndian) throw new NotSupportedException("Little-endian capture required.");
using var runtime = new OrtRuntime(Path.GetFullPath(args[2]));
using var run = runtime.CreateRun(default);
byte[] resource = File.ReadAllBytes(args[1]);
using var stream = File.OpenRead(args[0]);
using var bundle = Math2Mode0ColdBundle.Read(runtime, stream, resource);
InkStroke[] ink = [
    new([new InkPoint(0, 30), new(0, 0), new(10, 18), new(20, 0), new(20, 30)]),
    new([new InkPoint(35, 0), new(35, 30)])];
using var canceled = new CancellationTokenSource(); canceled.Cancel();
int guards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception e) when (e is ArgumentException or InvalidOperationException or InvalidDataException or
        NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new InvalidDataException("Required raw-only EF4 guard did not reject.");
}
Math2Mode0RawOnlySource Prepare(int width) => width == 1
    ? Math2Mode0RawOnlySource.PrepareColdDefault(runtime, bundle, run, ink)
    : Math2Mode0RawOnlySource.PrepareWithPreparedSearch(runtime, bundle, run, ink, width);
string Bytes(float[] values) => Convert.ToBase64String(MemoryMarshal.AsBytes(values.AsSpan()));
var actual = new List<Actual>();
foreach (int width in new[] { 1, 3, 16 })
{
    var source = Prepare(width);
    Reject(() => source.ApplyEf4());
    Reject(() => source.RunToInitialOrder(canceled.Token));
    source.RunToInitialOrder();
    Reject(() => source.RunToInitialOrder());
    var ordered = source.ReadInitialForDiagnostic();
    var history = source.ReadHistoryForDiagnostic();
    var before = ordered.CopyRecords();
    var sources = Enumerable.Range(0, before.Length).Select(ordered.ReadOwnedSource).ToArray();
    var views = source.Loop.PrepareRawOnlyRecords(source, ordered, history, default);
    var frames = history.CopyFramesForInspection().Select(frame =>
        new Frame(frame.Segments, frame.Beams, Bytes(frame.Values))).ToArray();
    Reject(() => source.ApplyEf4(canceled.Token));
    Reject(() => source.ApplyWithPreparedPayloadBudget(-1));
    Reject(() => source.ApplyWithPreparedPayloadBudget(Math2Mode0RawOnlyEf4.DefaultPayloadBudget + 1));
    Reject(() => Math2Mode0RawOnlyEf4.Prepare(source, ordered, history, source.CopyStrokeEnds(),
        Math2Mode0RawOnlyEf4.DefaultPayloadBudget, default)); //value-equal foreign array
    using (var foreign = Prepare(width))
    {
        foreign.RunToInitialOrder();
        Reject(() => Math2Mode0RawOnlyEf4.Prepare(source, foreign.ReadInitialForDiagnostic(), history,
            source.CopyStrokeEnds(), Math2Mode0RawOnlyEf4.DefaultPayloadBudget, default));
        Reject(() => Math2Mode0RawOnlyEf4.Prepare(source, ordered, foreign.ReadHistoryForDiagnostic(),
            source.CopyStrokeEnds(), Math2Mode0RawOnlyEf4.DefaultPayloadBudget, default));
        Reject(() => new Math2Mode0CompletedRecords.RawOnlyRecord(
            (Math2Mode0CompletedRecords)ordered.VectorIdentity, foreign, views[0].RecordIdentity));
        Reject(() => new Math2Mode0CompletedRecords.RawOnlyRecord(
            (Math2Mode0CompletedRecords)ordered.VectorIdentity, source, new object()));
        var foreignEf4 = foreign.ApplyEf4();
        Reject(() => views[0].StoreScore(foreignEf4, 0f));
    }
    var result = source.ApplyEf4();
    var records = result.CopyRecords();
    if (result.IsFinalOrder || result.IsAcceptedFormula || records.Length != before.Length)
        throw new InvalidDataException("EF4 stage must retain every actual initially sorted owner.");
    var rows = new List<Row>();
    for (int index = 0; index < records.Length; index++)
    {
        var row = records[index];
        if (!ReferenceEquals(row.Source, sources[index]) || !ReferenceEquals(row.Library, before[index].Library) ||
            !ReferenceEquals(row.RecordIdentity, views[index].RecordIdentity) ||
            !ReferenceEquals(row.SourceFactoryIdentity, source.SourceFactoryIdentity) || row.Before != before[index].Score)
            throw new InvalidDataException("Source/Record/Library/Factory identity was detached or reordered.");
        rows.Add(new(row.Source.Offset, row.Source.Rank, row.Source.CompletedSourceParent,
            Convert.ToBase64String(row.Source.CopyRawRecord()),
            row.Primary, row.Source.CopyAncestry(), row.Segments, row.Units.ToArray(), row.Assignments.ToArray(),
            row.Runs.ToArray(), row.InitialMask.ToArray(), row.DeletionMask.ToArray(), row.Before, row.Penalty, row.After));
    }
    if (records.Length > 0)
    {
        records[0].Units.AsSpan().Fill(0xdeadbeef);
        records[0].Assignments.AsSpan().Fill(1234);
        records[0].InitialMask.AsSpan().Fill(1);
        var again = result.CopyRecords()[0];
        if (!again.Units.SequenceEqual(rows[0].Units) || !again.Assignments.SequenceEqual(rows[0].Assignments) ||
            !again.InitialMask.SequenceEqual(rows[0].Initial))
            throw new InvalidDataException("Result inspection leaked a private payload alias.");
    }
    Reject(() => ordered.CopyRecords());
    Reject(() => ordered.CopyOriginalOrdinals());
    Reject(() => ordered.ReadOwnedSource(0));
    Reject(() => source.ApplyEf4());
    var statistics = source.CopyProcessingStatistics();
    actual.Add(new(width, source.InputFrameCount, source.CopyStrokeEnds(), frames, rows.ToArray(), statistics));
    source.Dispose();
    Reject(() => result.CopyRecords());
    Reject(() => source.ApplyEf4());
    if (before.Any(row => !row.Library.IsDisposed || row.Library.ReleaseCount != 1))
        throw new InvalidDataException("Actual Library ownership was lost at disposal.");
}

var failures = new List<Failure>();
foreach (var phase in Enum.GetValues<Math2Mode0RawOnlyEf4Stage>())
    foreach (bool cancel in new[] { false, true })
    {
        using var source = Prepare(1);
        source.RunToInitialOrder();
        var ordered = source.ReadInitialForDiagnostic();
        var before = ordered.CopyRecords();
        int actualSegments = ordered.ReadOwnedSource(0).RawOnlySegmentCount;
        using var stop = new CancellationTokenSource();
        Math2Mode0RawOnlyEf4? observed = null;
        Reject(() => source.ApplyDiagnostic(Math2Mode0RawOnlyEf4.DefaultPayloadBudget,
            (current, index, owner) =>
            {
                observed = owner;
                if (current != phase) return;
                if (cancel) stop.Cancel();
                else throw new InvalidOperationException("Prepared phase fault.");
            }, stop.Token));
        var state = source.CopyProcessingStatistics();
        if (observed == null || !state.Failed || state.Complete || !source.Loop.IsFailed)
            throw new InvalidDataException("Consuming failure must poison the entire stage/loop.");
        Reject(() => observed.CopyRecords());
        Reject(() => ordered.CopyRecords());
        Reject(() => source.ApplyEf4());
        failures.Add(new(phase.ToString(), cancel, state, before.Select(row => row.Score).ToArray(),
            before[0].Primary.Length, actualSegments));
    }
using (var source = Prepare(1))
{
    source.RunToInitialOrder();
    var old = source.ReadInitialForDiagnostic();
    Reject(() => source.ApplyWithPreparedPayloadBudget(0));
    var state = source.CopyProcessingStatistics();
    if (!state.Failed || state.StartedRecords != 0 || state.PlannedPayloadEnvelope <= 0 ||
        source.Loop.CopyRawAttentionStatistics().SnapshotCount != 0)
        throw new InvalidDataException("Total budget must fail before any EF4 payload is allocated.");
    Reject(() => old.CopyRecords()); Reject(() => source.ApplyEf4());
    failures.Add(new("budget-before-payload", false, state, []));
}
long exactEnvelope = actual[0].Statistics.PlannedPayloadEnvelope;
using (var source = Prepare(1))
{
    source.RunToInitialOrder();
    Math2Mode0RawOnlyEf4? observed = null;
    Reject(() => source.ApplyDiagnostic(Math2Mode0RawOnlyEf4.DefaultPayloadBudget,
        (phase, index, owner) =>
        {
            observed = owner;
            if (phase == Math2Mode0RawOnlyEf4Stage.BeforeCompletion) source.Dispose();
        }));
    if (observed == null || !observed.CopyStatistics().Failed)
        throw new InvalidDataException("Disposal in final callback must fail the entire stage.");
    Reject(() => observed.CopyRecords());
    failures.Add(new("dispose-before-completion", false, observed.CopyStatistics(), []));
}
using (var source = Prepare(1))
{
    source.RunToInitialOrder();
    var exact = source.ApplyWithPreparedPayloadBudget(exactEnvelope);
    if (exact.CopyRecords().Length != actual[0].Rows.Length ||
        exact.CopyStatistics().PlannedPayloadEnvelope != exactEnvelope)
        throw new InvalidDataException("Exact payload envelope must be sufficient.");
}
using (var source = Prepare(1))
{
    source.RunToInitialOrder();
    Reject(() => source.ApplyWithPreparedPayloadBudget(exactEnvelope - 1));
    var state = source.CopyProcessingStatistics();
    if (state.StartedRecords != 0 || source.Loop.CopyRawAttentionStatistics().SnapshotCount != 0)
        throw new InvalidDataException("One byte below envelope must fail before payload reconstruction.");
    failures.Add(new("budget-below-envelope", false, state, []));
}
var prepared = new List<Prepared>();
var preparedEmpty = new List<PreparedEmpty>();
for (int control = 0; control < 256; control++)
{
    uint bits = control switch { 0 => 0x7fc12345u, 1 => 0x7f800000u, 2 => 0xff800000u,
        _ => unchecked(0x9e3779b9u * (uint)(control + 1)) };
    float score = BitConverter.UInt32BitsToSingle(bits);
    int captureCalls = 0; var units = new List<uint>();
    if (Math2Mode0RawOnlyEf4.ShouldProcessPrimaryPrepared(0))
    { captureCalls++; score = 42f; units.Add(299); }
    uint after = BitConverter.SingleToUInt32Bits(score);
    if (after != bits || captureCalls != 0 || units.Count != 0)
        throw new InvalidDataException("Prepared empty records must preserve even nonfinite score bits and empty output.");
    preparedEmpty.Add(new(bits, after, captureCalls, units.Count));
}
if (!Math2Mode0RawOnlyEf4.ShouldProcessPrimaryPrepared(1) || !Math2Mode0RawOnlyEf4.ShouldProcessPrimaryPrepared(1000))
    throw new InvalidDataException("Nonempty prepared record predicate must still enter.");
Reject(() => Math2Mode0RawOnlyEf4.ShouldProcessPrimaryPrepared(-1));
Reject(() => Math2Mode0RawOnlyEf4.ShouldProcessPrimaryPrepared(1001));
for (int control = 0; control < 256; control++)
{
    int n = 2 + control % 5, segments = 1 + control % 7;
    float[] matrix = new float[n * segments];
    for (int row = 0; row < n; row++)
        for (int segment = 0; segment < segments; segment++)
            matrix[row * segments + segment] = row == 0 ? (1 + control % 4) / 16f : (12 + row % 4) / 16f;
    int[] remapped = new int[segments]; // prepared original1 became retained0
    float before = (control % 11) / 8f;
    var literal = Math2Mode0RawOnlyEf4.EvaluateLiteralScorePrepared(new byte[n], matrix,
        segments, remapped, before, BitConverter.Int32BitsToSingle(0x3dcccccd));
    var wrong = Math2Mode0RawOnlyEf4.EvaluateLiteralScorePrepared(new byte[n - 1], matrix.AsSpan(segments),
        segments, remapped, before, BitConverter.Int32BitsToSingle(0x3dcccccd));
    if (literal.Penalty == wrong.Penalty) throw new InvalidDataException("Prepared literal basis discriminator must differ.");
    prepared.Add(new(n, segments, matrix, remapped, before, literal.Penalty, literal.After, wrong.Penalty));
}
Reject(() => Math2Mode0RawOnlySource.PrepareColdDefault(runtime, bundle, run, [], default));
Reject(() => Math2Mode0RawOnlySource.PrepareWithPreparedSearch(runtime, bundle, run, ink, 17));
Reject(() => Math2Mode0RawOnlySource.PrepareColdDefault(runtime, bundle, run, ink, canceled.Token));
File.WriteAllText(args[3], JsonSerializer.Serialize(new Capture(runtime.Version,
    Convert.ToHexStringLower(SHA256.HashData(resource)), actual.ToArray(), failures.ToArray(), prepared.ToArray(), preparedEmpty.ToArray(), guards), CaptureJson.Default.Capture));
Console.WriteLine($"PASS{actual.Count} actual raw-only runs/{actual.Sum(item => item.Rows.Length)} EF4 records; " +
    $"{failures.Count} budget/failure prefixes/256 basis+256 empty prepared controls/{guards} guards; no final sort/output");

record Frame(int Segments, int Beams, string Values);
record Row(long Offset, int Rank, int Parent, string RawCandidate, ushort[] Primary, int[][] Ancestry, int Segments,
    uint[] Units, int[] Assignments, MathSegmentRun[] Runs, byte[] Initial, byte[] Deleted,
    float Before, float Penalty, float After);
record Actual(int PreparedWidth, int InputFrames, long[] StrokeEnds, Frame[] Frames,
    Row[] Rows, Math2Mode0RawOnlyEf4Statistics Statistics);
record Failure(string Phase, bool Cancel, Math2Mode0RawOnlyEf4Statistics Statistics, float[] Before,
    int PrimaryCount = 0, int Segments = 0);
record Prepared(int Symbols, int Segments, float[] OriginalMatrix, int[] RemappedAssignments,
    float Before, float Penalty, float After, float WrongRetainedPenalty);
record PreparedEmpty(uint BeforeBits, uint AfterBits, int CaptureCalls, int OutputUnits);
record Capture(string Runtime, string Resource, Actual[] Actual, Failure[] Failures, Prepared[] Prepared,
    PreparedEmpty[] PreparedEmpty, int Guards);
[JsonSerializable(typeof(Capture))]
[JsonSerializable(typeof(Row[]))]
partial class CaptureJson : JsonSerializerContext { }
