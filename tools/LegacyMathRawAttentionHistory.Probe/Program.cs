using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
using InkRecognition.Math;

if (args.Length != 4) throw new ArgumentException("model.ikrmath rawMath2.res official-runtime capture.json");
if (!BitConverter.IsLittleEndian) throw new NotSupportedException("Little-endian capture required.");
using var runtime = new OrtRuntime(Path.GetFullPath(args[2]));
using var run = runtime.CreateRun(CancellationToken.None);
byte[] resource = File.ReadAllBytes(args[1]);
using var stream = File.OpenRead(args[0]);
using var bundle = Math2Mode0ColdBundle.Read(runtime, stream, resource);
var gate = LegacyMath2ObservableTokens.BindBundledResource(resource);
var features = MathPreprocessor.Process([
    new InkStroke([new InkPoint(0, 30), new(0, 0), new(10, 18), new(20, 0), new(20, 30)]),
    new InkStroke([new InkPoint(35, 0), new(35, 30)])]);
using var source = runtime.Tensor(features.Values, features.Frames, 1, 26);
using var downsampling = runtime.Tensor(Enumerable.Repeat(1L, features.Frames).ToArray(), features.Frames, 1);
using var ends = runtime.Tensor(features.StrokeEnds, 1, features.StrokeEnds.Length);
using var canceled = new CancellationTokenSource(); canceled.Cancel();
int guards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception e) when (e is ArgumentException or InvalidOperationException or InvalidDataException or
        NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new InvalidDataException("Required raw-history/lifetime guard did not reject.");
}
string Bytes(float[] values) => Convert.ToBase64String(MemoryMarshal.AsBytes(values.AsSpan()));
bool Same(float[] a, float[] b) => MemoryMarshal.AsBytes(a.AsSpan()).SequenceEqual(MemoryMarshal.AsBytes(b.AsSpan()));
Math2Mode0ColdRecordingLoop Prepare(int width) => width == 1
    ? Math2Mode0ColdRecordingLoop.PrepareColdDefault(bundle, run, source, downsampling, ends)
    : Math2Mode0ColdRecordingLoop.PrepareWithPreparedPolicy(bundle, run, source, downsampling, ends, width, 1f, 1000f);

var actual = new List<Actual>();
foreach (int width in new[] { 1, 3, 16 })
{
    var loop = Prepare(width);
    Reject(() => loop.RunWithRawAttentionHistory(canceled.Token));
    Reject(() => loop.RunWithPreparedRawAttentionBudget(-1));
    Reject(() => loop.RunWithPreparedRawAttentionBudget(Math2Mode0RawAttentionHistory.PortableDefaultPayloadBudget + 1));
    var outputs = new List<RawFrame>();
    var sources = new List<Math2Mode0CompletedEventLease>();
    var carried = new List<Math2Mode0CompletedAttention>();
    Math2Mode0CompletedRecords? vector = null;
    Math2Mode0PrefixContext? actualPrefix = null;
    Math2Mode0PrefixStep? lastStep = null;
    var history = loop.RunRawAttentionDiagnostic(Math2Mode0RawAttentionHistory.PortableDefaultPayloadBudget,
        (stage, step, candidates, prefix, records, current) =>
        {
            vector = records;
            actualPrefix = prefix; lastStep = step;
            if (stage == Math2Mode0RecordingObservationStage.BeforeRecording)
            {
                var output = step.CopyOutput(1);
                outputs.Add(new(checked((int)output.Shape[0]), checked((int)output.Shape[1]), Bytes(output.Values)));
            }
            if (stage != Math2Mode0RecordingObservationStage.AfterRecording) return;
            for (int index = sources.Count; index < records.Count; index++)
            {
                var sourceEvent = records.ReadSourceEvent(index);
                sources.Add(sourceEvent); carried.Add(sourceEvent.CaptureAttention());
                Reject(() => current.CaptureCompleted(sourceEvent)); //not yet a complete history
            }
        });
    var before = loop.CopyRawAttentionStatistics();
    if (before.SnapshotCount != 0 || before.PayloadAllocationAttempts != outputs.Count)
        throw new InvalidDataException("History must not automatically materialize per-EOS matrices.");
    bool sorted = width != 1;
    int[] sortedSources;
    if (sorted)
    {
        var ordered = loop.OrderInitialCompleted();
        sortedSources = Enumerable.Range(0, ordered.CopyRecords().Length)
            .Select(index => sources.IndexOf(ordered.ReadOwnedSource(index))).ToArray();
    }
    else sortedSources = Enumerable.Range(0, sources.Count).ToArray();
    var copied = history.CopyFramesForInspection();
    if (!copied.Select(frame => Bytes(frame.Values)).SequenceEqual(outputs.Select(frame => frame.Values)))
        throw new InvalidDataException("Private history must contain the actual output1 payloads.");
    copied[0].Values.AsSpan().Fill(1234567);
    if (Bytes(history.CopyFramesForInspection()[0].Values) != outputs[0].Values)
        throw new InvalidDataException("Raw frame inspection leaked a mutable alias.");
    Reject(() => sources[0].CaptureAttention()); //actual Step is stale; new history works anyway

    // A separate real same-policy loop yields matching payload/shape sources,
    // but cannot substitute its recorded lease into this history.
    using var foreignLoop = Prepare(width);
    foreignLoop.RunToRecordedPrefix();
    var foreign = foreignLoop.OrderInitialCompleted();
    var foreignSource = Enumerable.Range(0, foreign.CopyRecords().Length).Select(foreign.ReadOwnedSource)
        .First(item => item.Offset == sources[0].Offset && item.CopyRawRecord().SequenceEqual(sources[0].CopyRawRecord()));
    Reject(() => history.CaptureCompleted(foreignSource));
    var matrices = new List<Matrix>();
    Math2Mode0HistoricalCompletedAttention? first = null;
    for (int index = 0; index < sources.Count; index++)
    {
        var item = sources[index]; var snapshot = history.CaptureCompleted(item);
        if (!ReferenceEquals(snapshot.Source, item) || !Same(snapshot.CopyAttention(), carried[index].CopyAttention()))
            throw new InvalidDataException("Backward raw-history matrix differs from actual pre-Apply carried state.");
        long reserved = loop.CopyRawAttentionStatistics().ReservedPayloadBytes;
        if (!ReferenceEquals(snapshot, history.CaptureCompleted(item)) || loop.CopyRawAttentionStatistics().ReservedPayloadBytes != reserved)
            throw new InvalidDataException("Repeated reconstruction must reuse its exact cached snapshot and budget charge.");
        float[] copy = snapshot.CopyAttention(); copy.AsSpan().Fill(7654321);
        if (!Same(snapshot.CopyAttention(), carried[index].CopyAttention()))
            throw new InvalidDataException("Snapshot inspection leaked a mutable alias.");
        matrices.Add(new(item.Offset, item.Rank, item.CompletedSourceParent, item.CopyPrimary(), item.CopyAncestry(),
            snapshot.Symbols, snapshot.Segments, Bytes(snapshot.CopyAttention()), Bytes(carried[index].CopyAttention())));
        first ??= snapshot;
    }
    var fake = new Math2Mode0HistoricalCompletedAttention(history, sources[0], first!.Symbols, first.Segments, new float[first.Symbols * first.Segments]);
    Reject(() => fake.CopyAttention());
    var foreignHistory = new Math2Mode0RawAttentionHistory(loop, actualPrefix!, vector!, 16 * 1024 * 1024);
    Reject(() => foreignHistory.CaptureCompleted(sources[0]));
    // Explicit foreign tensors exercise metadata-first guards; they are NOT
    // supplied model outputs or an owning-history production entry.
    int beams = lastStep!.Generation.ActiveCount, segments = outputs[0].Segments;
    long attempts = loop.CopyRawAttentionStatistics().PayloadAllocationAttempts;
    using (var wrongRank = runtime.Tensor([0f], 1, 1))
        Reject(() => wrongRank.CopyRawHistoryFrame(lastStep, history, gate, segments, beams, default));
    using (var wrongType = runtime.Tensor(new long[segments * beams], segments, beams, 1))
        Reject(() => wrongType.CopyRawHistoryFrame(lastStep, history, gate, segments, beams, default));
    using (var wrongTime = runtime.Tensor(new float[segments * beams * 2], segments, beams, 2))
        Reject(() => wrongTime.CopyRawHistoryFrame(lastStep, history, gate, segments, beams, default));
    using (var sameShapeForeign = runtime.Tensor(new float[segments * beams], segments, beams, 1))
        Reject(() => sameShapeForeign.CopyRawHistoryFrame(lastStep, history, gate, segments, beams, default));
    if (loop.CopyRawAttentionStatistics().PayloadAllocationAttempts != attempts ||
        !Same(first.CopyAttention(), carried[0].CopyAttention()))
        throw new InvalidDataException("Metadata/foreign-source guards allocated payload or consumed the valid owner.");
    var after = loop.CopyRawAttentionStatistics();
    Reject(() => loop.RunWithRawAttentionHistory());
    Reject(() => loop.RunToRecordedPrefix());
    loop.Dispose();
    Reject(() => history.CaptureCompleted(sources[0]));
    Reject(() => first.CopyAttention());
    var disposed = loop.CopyRawAttentionStatistics();
    if (!disposed.Disposed || disposed.RawFrameCount != 0 || disposed.SnapshotCount != 0 ||
        disposed.RawPayloadBytes != 0 || disposed.SnapshotPayloadBytes != 0)
        throw new InvalidDataException("Disposal must release private raw/snapshot payload references.");
    actual.Add(new(width, sorted, sortedSources, outputs.ToArray(), matrices.ToArray(), before, after, disposed));
}

// Exercise the non-diagnostic opt-in entry: source selected AFTER initial sort.
using (var loop = Prepare(1))
{
    var history = loop.RunWithRawAttentionHistory();
    var sorted = loop.OrderInitialCompleted();
    if (history.CaptureCompleted(sorted.ReadOwnedSource(0)).CopyAttention().Length == 0)
        throw new InvalidDataException("Named production history entry must reconstruct actual source data.");
}
using (var loop = Prepare(1))
{
    loop.RunToRecordedPrefix();
    Reject(() => loop.CopyRawAttentionStatistics()); //old API does not opt in
}

var failures = new List<Failure>();
foreach (long budget in new[] { 0L, 51L, 52L })
{
    using var loop = Prepare(3);
    Math2Mode0RawAttentionHistory? retained = null;
    Reject(() => loop.RunRawAttentionDiagnostic(budget, (stage, step, candidates, prefix, records, history) => retained = history));
    var stats = loop.CopyRawAttentionStatistics();
    if (!loop.IsFailed || !stats.Failed || stats.Complete || stats.SnapshotCount != 0 ||
        stats.PayloadAllocationAttempts != (budget == 52 ? 1 : 0))
        throw new InvalidDataException("Raw budget failure must precede the disallowed payload allocation.");
    if (retained != null) Reject(() => retained.CopyFramesForInspection());
    Reject(() => loop.RunWithRawAttentionHistory());
    failures.Add(new("RawBudget", budget, loop.SuccessfulDecoderCalls, stats, null));
}

// Retain one valid matrix, then fail BEFORE allocating the next one. This
// invalidates previously returned historical snapshot reads as well.
{
    long raw = actual[1].Before.RawPayloadBytes;
    var matrix0 = actual[1].Matrices[actual[1].SortedSourceOrder[0]];
    var matrix1 = actual[1].Matrices[actual[1].SortedSourceOrder[1]];
    long budget = raw + matrix0.Symbols * matrix0.Segments * 4L + matrix1.Symbols * matrix1.Segments * 4L - 1;
    using var loop = Prepare(3);
    var history = loop.RunWithPreparedRawAttentionBudget(budget);
    var sorted = loop.OrderInitialCompleted();
    var first = history.CaptureCompleted(sorted.ReadOwnedSource(0));
    int allocations = loop.CopyRawAttentionStatistics().PayloadAllocationAttempts;
    Reject(() => history.CaptureCompleted(sorted.ReadOwnedSource(1)));
    var stats = loop.CopyRawAttentionStatistics();
    if (!loop.IsFailed || !stats.Failed || stats.SnapshotCount != 1 || stats.PayloadAllocationAttempts != allocations)
        throw new InvalidDataException("Reconstruction budget failure allocated or exposed a partial snapshot.");
    Reject(() => first.CopyAttention()); Reject(() => sorted.CopyRecords());
    failures.Add(new("SnapshotBudget", budget, loop.SuccessfulDecoderCalls, stats, null));
}
{
    using var loop = Prepare(1);
    var history = loop.RunWithRawAttentionHistory();
    var sorted = loop.OrderInitialCompleted(); var sourceEvent = sorted.ReadOwnedSource(0);
    var snapshot = history.CaptureCompleted(sourceEvent);
    Reject(() => history.CaptureCompleted(sourceEvent, canceled.Token));
    Reject(() => snapshot.CopyAttention());
    var stats = loop.CopyRawAttentionStatistics();
    if (!loop.IsFailed || !stats.Failed) throw new InvalidDataException("Cancelled reconstruction must poison the history owner.");
    failures.Add(new("SnapshotCancel", stats.BudgetBytes, loop.SuccessfulDecoderCalls, stats, null));
}
foreach (bool cancel in new[] { false, true })
{
    using var loop = Prepare(1); using var stop = new CancellationTokenSource();
    Math2Mode0RawAttentionHistory? retained = null; Math2Mode0CompletedEventLease? sourceEvent = null;
    Reject(() => loop.RunRawAttentionDiagnostic(Math2Mode0RawAttentionHistory.PortableDefaultPayloadBudget,
        (stage, step, candidates, prefix, records, history) =>
        {
            retained = history;
            if (stage == Math2Mode0RecordingObservationStage.AfterRecording && records.Count != 0)
            {
                sourceEvent = records.ReadSourceEvent(0);
                if (cancel) stop.Cancel(); else throw new InvalidDataException("Prepared post-recording history failure.");
            }
        }, stop.Token));
    var stats = loop.CopyRawAttentionStatistics();
    if (!stats.Failed || stats.Complete || sourceEvent == null || retained == null)
        throw new InvalidDataException("The actual counted source/failure prefix must be retained, not accepted.");
    Reject(() => retained.CaptureCompleted(sourceEvent));
    failures.Add(new(cancel ? "RecordingCancel" : "RecordingFault", stats.BudgetBytes, loop.SuccessfulDecoderCalls, stats, sourceEvent.Offset));
}

var prepared = new List<Prepared>(); uint random = 0x36f20;
uint Next() { random ^= random << 13; random ^= random >> 17; random ^= random << 5; return random; }
ushort[] labels = [0, 1, 2, 261, 262, 267, 268, 269, 276, 296];
for (int control = 0; control < 128; control++)
{
    int width = 1 + (int)(Next() % 8), segments = 1 + (int)(Next() % 7), times = 2 + (int)(Next() % 10);
    var generation = Math2Mode0PrefixGeneration.Cold(width);
    var frames = new List<float[]>(); var beams = new List<int>(); var plans = new List<Plan>();
    for (int time = 0; time < times; time++)
    {
        int active = generation.ActiveCount; beams.Add(active);
        frames.Add(Enumerable.Range(0, segments * active).Select(at => (float)(time * 10000 + at)).ToArray());
        int count = 1 + (int)(Next() % (uint)width);
        int[] parents = Enumerable.Range(0, count).Select(_ => (int)(Next() % (uint)active)).ToArray();
        ushort[] chosen = Enumerable.Range(0, count).Select(_ => labels[Next() % (uint)labels.Length]).ToArray();
        var rows = new byte[count * 48];
        for (int rank = 0; rank < count; rank++)
        {
            var row = rows.AsSpan(rank * 48, 48);
            BinaryPrimitives.WriteUInt64LittleEndian(row[8..], chosen[rank]);
            BinaryPrimitives.WriteUInt64LittleEndian(row[32..], ulong.MaxValue);
            BinaryPrimitives.WriteInt32LittleEndian(row[40..], parents[rank]);
        }
        generation = generation.Append(rows, default); plans.Add(new(parents, chosen));
    }
    var primary = generation.CopyPrimary(); var ancestry = generation.CopyAncestry();
    var results = new List<string>();
    for (int parent = 0; parent < generation.ActiveCount; parent++)
    {
        int currentParent = parent; float[] values = new float[times * segments];
        LegacyPreparedMathAttentionHistory.WritePrepared(values, times, segments, parent,
            time => primary[currentParent][time], (rank, time) => ancestry[rank][time], beams,
            (time, rank, segment) => frames[time][segment * beams[time] + rank], gate);
        results.Add(Bytes(values));
    }
    prepared.Add(new(control, width, segments, beams.ToArray(), frames.Select(Bytes).ToArray(), plans.ToArray(), primary, ancestry, results.ToArray()));
}
File.WriteAllText(args[3], JsonSerializer.Serialize(new Capture(runtime.Version,
    Convert.ToHexStringLower(SHA256.HashData(resource)), actual.ToArray(), failures.ToArray(), prepared.ToArray(), guards),
    CaptureJson.Default.Capture));
Console.WriteLine($"PASS{actual.Count} actual history runs/{actual.Sum(item => item.Matrices.Length)} source matrices; " +
    $"{failures.Count} budget/failure cases/128 prepared branching histories/{guards} guards; no EF4/output");

record RawFrame(int Segments, int Beams, string Values);
record Matrix(long Offset, int Rank, int Parent, ushort[] Primary, int[][] Ancestry,
    int Symbols, int Segments, string Historical, string CarriedBeforeApply);
record Actual(int PreparedWidth, bool InitiallySorted, int[] SortedSourceOrder, RawFrame[] Frames, Matrix[] Matrices,
    Math2Mode0AttentionHistoryStatistics Before, Math2Mode0AttentionHistoryStatistics After,
    Math2Mode0AttentionHistoryStatistics Disposed);
record Failure(string Kind, long Budget, int DecoderCalls, Math2Mode0AttentionHistoryStatistics Statistics, long? SourceOffset);
record Plan(int[] Parents, ushort[] Labels);
record Prepared(int Control, int Width, int Segments, int[] Beams, string[] Frames, Plan[] Plans,
    ushort[][] Primary, int[][] Ancestry, string[] Results);
record Capture(string Runtime, string Resource, Actual[] Actual, Failure[] Failures, Prepared[] Prepared, int Guards);
[JsonSerializable(typeof(Capture))]
partial class CaptureJson : JsonSerializerContext { }
