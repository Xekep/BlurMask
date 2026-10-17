using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
using InkRecognition.Math;

if (args.Length != 4) throw new ArgumentException("model.ikrmath rawMath2.res official-runtime-library capture.json");
if (!BitConverter.IsLittleEndian) throw new NotSupportedException("Little endian capture required.");
byte[] resource = File.ReadAllBytes(args[1]);
using var runtime = new OrtRuntime(Path.GetFullPath(args[2]));
using var run = runtime.CreateRun(CancellationToken.None);
using var stream = File.OpenRead(args[0]);
using var bundle = Math2Mode0ColdBundle.Read(runtime, stream, resource);
using var modelStream = File.OpenRead(args[0]);
var model = MathModel.Read(modelStream);
var features = MathPreprocessor.Process([
    new InkStroke([new InkPoint(0, 30), new(0, 0), new(10, 18), new(20, 0), new(20, 30)]),
    new InkStroke([new InkPoint(35, 0), new(35, 30)])]);
using var source = runtime.Tensor(features.Values, features.Frames, 1, 26);
using var downsampling = runtime.Tensor(Enumerable.Repeat(1L, features.Frames).ToArray(), features.Frames, 1);
using var ends = runtime.Tensor(features.StrokeEnds, 1, features.StrokeEnds.Length);
int guards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception e) when (e is ArgumentException or InvalidOperationException or InvalidDataException or OperationCanceledException or NotSupportedException)
    { guards++; return; }
    throw new InvalidDataException("Required prefix/Step lifecycle guard did not reject.");
}
TensorCapture Tensor(string name, int type, long[] shape, float[]? floats, long[]? longs)
{
    byte[] bytes = new byte[(floats?.Length ?? longs!.Length) * (type == 1 ? 4 : 8)];
    Buffer.BlockCopy((Array?)floats ?? longs!, 0, bytes, 0, bytes.Length);
    return new(name, type, shape, Convert.ToBase64String(bytes));
}
TensorCapture[] Inputs(Math2Mode0PrefixContext owner) => owner.CopyTensorInputs()
    .Select(x => Tensor(x.Name, x.Type, x.Shape, x.Floats, x.Int64s)).ToArray();
TensorCapture[] Outputs(Math2Mode0PrefixStep step) => Enumerable.Range(0, 4).Select(i =>
{
    var value = step.CopyOutput(i);
    return Tensor(i.ToString(), 1, value.Shape, value.Values, null);
}).ToArray();
GenerationCapture Generation(Math2Mode0PrefixGeneration generation)
    => new(generation.Offset, generation.ActiveCount, generation.CopyPrimary(),
        generation.CopySecondary(), generation.CopyAncestry());
var searchBounds = new List<SearchBoundCapture>();
Math2Mode0PrefixContext Create(int width = 3)
{
    var owner = Math2Mode0PrefixContext.Prepare(bundle, run, source, downsampling, ends, width);
    if (owner.InputFrameCount != features.Frames) throw new InvalidDataException("Original input N lost.");
    searchBounds.Add(new(owner.InputFrameCount, owner.EncoderMaximumPredictionCount, owner.ColdMaximumPosition));
    return owner;
}
Reject(() => Math2Mode0PrefixContext.Prepare(bundle, run, source, downsampling, ends, 0));
Reject(() => Math2Mode0PrefixContext.Prepare(bundle, run, source, downsampling, ends, 17));
using (var canceledPreparation = new CancellationTokenSource())
{
    canceledPreparation.Cancel();
    Reject(() => Math2Mode0PrefixContext.Prepare(bundle, run, source, downsampling, ends, 3, canceledPreparation.Token));
}

int completedGuards = 0;
var frontierControls = new List<FrontierControl>();
for (int i = 0; i < 512; i++)
{
    int width = 1 + i % 4, count = i % 7, vocabulary = 1 + i % 17;
    ushort[][] secondary = Enumerable.Range(0, width).Select(beam => Enumerable.Range(0, count)
        .Select(at => (ushort)(i % 13 == 0 ? 65535 : at + 10 + (i % 3 == 0 && beam == width - 1 && at == i % System.Math.Max(1, count) ? 1 : 0))).ToArray()).ToArray();
    float[] logits = Enumerable.Range(0, width * vocabulary).Select(at => (i % 8) switch
    {
        0 => float.NaN, 1 => float.NegativeInfinity, 2 => -float.MaxValue,
        3 => 0f, 4 => at % vocabulary == 0 ? float.NaN : at,
        5 => at % vocabulary == vocabulary - 1 ? float.PositiveInfinity : 0f,
        _ => (float)((at * 17 + i) % 19)
    }).ToArray();
    var frontier = Math2Mode0CommonSecondaryFrontier.EvaluatePrepared(secondary, logits, vocabulary, i % 5);
    byte[] bytes = new byte[logits.Length * 4]; Buffer.BlockCopy(logits, 0, bytes, 0, bytes.Length);
    frontierControls.Add(new(secondary, Convert.ToBase64String(bytes), vocabulary, i % 5, frontier.Value, frontier.Common, frontier.CopyArgmax()));
}
void RejectCompleted(Action action)
{
    try { action(); }
    catch (Exception e) when (e is ArgumentException or InvalidOperationException or InvalidDataException or OperationCanceledException or NotSupportedException)
    { completedGuards++; return; }
    throw new InvalidDataException("Required actual completed-event guard did not reject.");
}
RecordState RecordState(Math2Mode0CompletedRecords recorder)
{
    var rows = recorder.CopyRecords();
    for (int i = 0; i < rows.Length; i++)
        for (int j = 0; j < i; j++)
            if (ReferenceEquals(rows[i].Library, rows[j].Library) ||
                ReferenceEquals(rows[i].Library.PoolIdentity, rows[j].Library.PoolIdentity))
                throw new InvalidDataException("Distinct completed records shared Library3E.");
    return new(recorder.Count, recorder.Capacity, recorder.IsFailed, recorder.CreatedLibraries,
        recorder.ReleasedUnadoptedLibraries, rows.Select(row => new RecordFields(row.Score,
            row.Frontier, row.Parent, row.Primary, row.PrimaryCapacity, row.Secondary,
            row.SecondaryCapacity, row.Library.LogicalIdentity, row.Library.IsDisposed,
            row.Library.ObjectBytes, row.Library.PoolBytes, row.Library.CopyInitializedPoolQwords(),
            row.Library.InitializedPoolDword10, row.NestedTensorCount)).ToArray());
}
var recordedSteps = new List<RecordStep>();
using var foreignOwner = Create();
using var foreignRecorder = foreignOwner.CreateCompletedRecords();
var segmentEvents = new List<SegmentsEvent>();
var segmentControls = new List<SegmentsControl>();
int segmentGuards = 0;
void RejectSegments(Action a) { int old=guards;Reject(a);segmentGuards++;guards=old; }
var attention = new List<AttentionCapture>();
var snapshots = new List<Math2Mode0CompletedAttention>();
int attentionGuards = 0;
void RejectAttention(Action action) { int old = guards; Reject(action); attentionGuards++; guards = old; }
var runs = new List<RunCapture>();
foreach (var settings in new[] { (Width: 1, Delta: -1f), (Width: 3, Delta: 1000f), (Width: 3, Delta: -1f) })
{
    using var owner = Create(settings.Width);
    using var canceledRecorderPreparation = new CancellationTokenSource(); canceledRecorderPreparation.Cancel();
    RejectCompleted(() => owner.CreateCompletedRecords(canceledRecorderPreparation.Token));
    using var recorder = owner.CreateCompletedRecords();
    RejectCompleted(() => owner.CreateCompletedRecords());
    Math2Mode0CompletedEventLease? oldEvent = null;
    var initial = Inputs(owner); var initialGeneration = Generation(owner.CopyGeneration());
    var steps = new List<StepCapture>();
    Math2Mode0PrefixCandidates? oldCandidates = null;
    Math2Mode0PrefixSelection? oldSelection = null;
    for (int iteration = 0; iteration < 5; iteration++)
    {
        var before = Inputs(owner); var prefixBefore = Generation(owner.CopyGeneration());
        var step = owner.RunStep(); var outputs = Outputs(step);
        Reject(() => owner.RunStep());
        if (oldCandidates != null) Reject(() => oldCandidates.Select());
        if (oldSelection != null) Reject(() => owner.ApplySelected(oldSelection));
        var candidates = step.CreateCandidates(1f, settings.Delta);
        var selected = candidates.Select();
        if (oldEvent != null) RejectCompleted(() => recorder.Append(oldEvent));
        var recordBefore = RecordState(recorder);
        var frontier = step.ReadCompletedFrontier(default);
        using var canceledFrontier = new CancellationTokenSource(); canceledFrontier.Cancel();
        RejectCompleted(() => step.ReadCompletedFrontier(canceledFrontier.Token));
        using var unregistered = new Math2Mode0CompletedRecords(owner);
        RejectCompleted(() => unregistered.Capture(candidates, 0));
        var events = new List<RecordEvent>();
        var numericEvents = candidates.CopyCompletedScoreFacts();
        RejectCompleted(() => recorder.Capture(candidates, -1));
        RejectCompleted(() => recorder.Capture(candidates, numericEvents.Length));
        RejectCompleted(() => foreignRecorder.Capture(candidates, 0));
        foreach (int ordinal in Enumerable.Range(0, numericEvents.Length))
        {
            using var canceledEvent = new CancellationTokenSource(); canceledEvent.Cancel();
            RejectCompleted(() => recorder.Capture(candidates, ordinal, canceledEvent.Token));
            var lease = recorder.Capture(candidates, ordinal);
            RejectCompleted(() => recorder.Capture(candidates, ordinal));
            RejectCompleted(() => Math2Mode0CompletedEventLease.FromActual(recorder, candidates, ordinal, default));
            RejectCompleted(() => foreignRecorder.Append(lease));
            RejectCompleted(() => recorder.Append(lease, canceledEvent.Token));
            byte[] eventRow = lease.CopyRawRecord();
            ushort[] eventPrimary = lease.CopyPrimary(), eventSecondary = lease.CopySecondary();
            int[][] ancestry = lease.CopyAncestry();
            lease.CopyRawRecord().AsSpan().Fill(255); lease.CopyPrimary().AsSpan().Fill(65535);
            var alternate = step.CreateCandidates(1f, settings.Delta).Select();
            RejectCompleted(() => owner.ApplySelected(alternate));
            RejectCompleted(() => owner.ApplySelected(selected));
            RejectCompleted(() => recorder.Capture(step.CreateCandidates(1f, settings.Delta), ordinal));
            using (var cancelledAttention = new CancellationTokenSource())
            {
                cancelledAttention.Cancel();
                RejectAttention(() => lease.CaptureAttention(cancelledAttention.Token));
            }
            var snapshot = lease.CaptureAttention();
            if (!ReferenceEquals(snapshot, lease.CaptureAttention()) || !ReferenceEquals(snapshot.Source, lease))
                throw new InvalidDataException("Same event snapshot identity lost.");
            var values = snapshot.CopyAttention();
            snapshot.CopyAttention().AsSpan().Fill(255);
            if (!values.SequenceEqual(snapshot.CopyAttention())) throw new InvalidDataException("Snapshot aliases caller memory.");
            attention.Add(new(settings.Width, settings.Delta, lease.Offset, lease.Rank,
                BinaryPrimitives.ReadInt32LittleEndian(eventRow.AsSpan(40)), snapshot.Symbols, snapshot.Segments,
                Tensor("attention", 1, [snapshot.Symbols, snapshot.Segments], values, null),
                Tensor("sigmoid", 1, [snapshot.Symbols, snapshot.Segments], snapshot.ActivateSigmoid(), null)));
            var mapped=LegacyPreparedFreshMathSegments.Prepare(snapshot);
            if(!ReferenceEquals(mapped.Source,snapshot))throw new InvalidDataException("Foreign segment source.");
            int[] observed=mapped.CopyAssignments();mapped.CopyAssignments().AsSpan().Fill(999);
            if(!mapped.CopyAssignments().SequenceEqual(observed))throw new InvalidDataException("Segment results alias caller.");
            segmentEvents.Add(new(settings.Width,settings.Delta,lease.Offset,lease.Rank,mapped.CopyObservableIndices(),observed,mapped.CopyRuns()));
            snapshots.Add(snapshot);
            recorder.Append(lease);
            var copiedRecords = recorder.CopyRecords();
            copiedRecords[^1].Primary.AsSpan().Fill(65535);
            if (!recorder.CopyRecords()[^1].Primary.SequenceEqual(lease.CopyPrimary()))
                throw new InvalidDataException("Returned snapshot mutated owned primary storage.");
            if (!ReferenceEquals(recorder.ReadSourceEvent(recorder.Count - 1), lease))
                throw new InvalidDataException("Record lost its exact source event provenance.");
            RejectCompleted(() => recorder.Append(lease));
            events.Add(new(lease.Rank, lease.Offset, Convert.ToBase64String(eventRow),
                eventPrimary, eventSecondary, ancestry, lease.Frontier.Value, RecordState(recorder)));
            oldEvent = lease;
        }
        recordedSteps.Add(new(settings.Width, settings.Delta, prefixBefore.Offset, frontier.Value,
            frontier.Common, frontier.CopyArgmax(), recordBefore, events.ToArray(), RecordState(recorder)));
        if (selected.IsDiagnostic) throw new InvalidDataException("Actual sidecar provenance lost.");
        using var canceled = new CancellationTokenSource(); canceled.Cancel();
        Reject(() => owner.ApplySelected(selected, canceled.Token));
        var rows = selected.CopyRecords(); selected.CopyRecords().AsSpan().Fill(255);
        owner.CopyGeneration().CopyPrimary()[0].AsSpan().Fill(ushort.MaxValue);
        owner.ApplySelected(selected);
        foreach (var item in snapshots) RejectAttention(() => item.Source.CaptureAttention());
        Reject(() => step.CopyOutput(0));
        steps.Add(new(false, before, outputs, Convert.ToBase64String(rows), Inputs(owner), prefixBefore,
            Generation(owner.CopyGeneration()), Tensor("prior", 1, [prefixBefore.ActiveCount],
                iteration == 0 ? new float[1] : steps[^1].SelectedCosts(), null),
            Tensor("logs", 1, [prefixBefore.ActiveCount, 297], candidates.CopyLogProbabilities(), null),
            Tensor("costs", 1, [prefixBefore.ActiveCount, 297], candidates.CopyUnmaskedCosts(), null),
            Convert.ToBase64String(candidates.CopyInformation()), Convert.ToBase64String(candidates.CopyConditional()),
            Convert.ToBase64String(candidates.CopyHeapStorage()), Convert.ToBase64String(candidates.CopySortedStorage()),
            Convert.ToBase64String(candidates.CopyAfterEos()), Convert.ToBase64String(candidates.CopyAfterResort()),
            candidates.CopyCompletedScoreFacts().Select(x => new EosCapture(x.Rank, x.SourceBeam,
                x.CandidateCost, x.CompletedScore, x.Prefix)).ToArray()));
        oldCandidates = candidates; oldSelection = selected;
    }
    runs.Add(new("actual-logits/native-prepared-candidate-kernels", settings.Width, settings.Delta,
        initial, initialGeneration, steps.ToArray()));
}

//Explicit diagnostic parent/label selection over actual NN steps. Sidecars
//and costs still come from those steps; these are not observed beam winners.
using (var owner = Create())
{
    var initial = Inputs(owner); var initialGeneration = Generation(owner.CopyGeneration());
    var steps = new List<StepCapture>();
    foreach (int[] parents in new[] { new[] { 0, 0, 0 }, new[] { 2, 0, 2 }, new[] { 2, 1 }, new[] { 1, 0 }, new[] { 0 } })
    {
        var before = Inputs(owner); var prefixBefore = Generation(owner.CopyGeneration());
        var step = owner.RunStep(); var outputs = Outputs(step); var prior = step.CopyPriorCosts();
        var actual = step.CreateCandidates(1f, 1000f);
        var labels = parents.Select((_, i) => (ushort)(i == 0 ? 262 : 2 + i)).ToArray();
        Reject(() => step.BindDiagnosticParents([-1], [2]));
        Reject(() => step.BindDiagnosticParents([prefixBefore.ActiveCount], [2]));
        Reject(() => step.BindDiagnosticParents([0], [297]));
        Reject(() => step.BindDiagnosticParents([0, 0], [2]));
        var selected = step.BindDiagnosticParents(parents, labels);
        if (!selected.IsDiagnostic) throw new InvalidDataException("Diagnostic selection must remain explicit.");
        var rows = selected.CopyRecords();
        owner.ApplySelected(selected);
        steps.Add(new(true, before, outputs, Convert.ToBase64String(rows), Inputs(owner), prefixBefore,
            Generation(owner.CopyGeneration()), Tensor("prior", 1, [prior.Length], prior, null),
            Tensor("logs", 1, [prefixBefore.ActiveCount, 297], actual.CopyLogProbabilities(), null),
            Tensor("costs", 1, [prefixBefore.ActiveCount, 297], actual.CopyUnmaskedCosts(), null),
            Convert.ToBase64String(actual.CopyInformation()), Convert.ToBase64String(actual.CopyConditional()),
            "", "", "", "", Array.Empty<EosCapture>()));
    }
    runs.Add(new("explicit-diagnostic-parent-selections", 3, 1000f, initial, initialGeneration, steps.ToArray()));
}

using (var a = Create())
using (var b = Create())
{
    using var canceled = new CancellationTokenSource(); canceled.Cancel();
    Reject(() => a.RunStep(canceled.Token));
    var stepA = a.RunStep(); var stepB = b.RunStep();
    var candidatesA = stepA.CreateCandidates(1f, 1000f);
    var selectedA = candidatesA.Select(); var selectedB = stepB.CreateCandidates(1f, 1000f).Select();
    Reject(() => a.ApplySelected(selectedB)); Reject(() => b.ApplySelected(selectedA));
    Reject(() => stepA.CreateCandidates(1f, 1000f, canceled.Token));
    Reject(bundle.Dispose); Reject(runtime.Dispose); Reject(run.Dispose);
    a.ApplySelected(selectedA); b.ApplySelected(selectedB);
    Reject(() => candidatesA.Select()); Reject(() => a.ApplySelected(selectedA));
}

var failures = new List<FailureCapture>();
foreach (bool cancelAfterPrefix in new[] { false, true })
{
    using var owner = Create(); using var cancel = new CancellationTokenSource();
    var step = owner.RunStep(); var selected = step.CreateCandidates(1f, 1000f).Select();
    var before = Generation(owner.CopyGeneration()); var rows = selected.CopyRecords();
    Reject(() => owner.ApplySelectedDiagnostic(selected, () =>
    {
        if (cancelAfterPrefix) cancel.Cancel();
        else throw new InvalidDataException("Explicit fault after prefix writes.");
    }, cancel.Token));
    if (!owner.IsFailed) throw new InvalidDataException("A partial prefix update remained reusable.");
    failures.Add(new(cancelAfterPrefix, before, Convert.ToBase64String(rows), Generation(owner.CopyGeneration())));
    Reject(() => owner.RunStep()); Reject(() => owner.ApplySelected(selected)); Reject(() => owner.CopyTensorInputs());
}
using (var termination = new CancellationTokenSource())
using (var terminatedRun = runtime.CreateRun(termination.Token))
{
    using var owner = Math2Mode0PrefixContext.Prepare(bundle, terminatedRun, source, downsampling, ends, 3);
    termination.Cancel();
    Reject(() => owner.RunStep());
    if (!owner.IsFailed) throw new InvalidDataException("Failed actual ORT run remained reusable.");
    Reject(() => owner.RunStep()); Reject(() => owner.CopyTensorInputs());
    Reject(terminatedRun.Dispose);
    owner.Dispose(); terminatedRun.Dispose();
    Reject(() => owner.RunStep());
}

//Pure prepared row-layout checks expose alternate-sentinel/U16 semantics;
//they are not configured-alias inference or tensor selections.
var prepared = new List<PreparedCapture>();
var generation = Math2Mode0PrefixGeneration.Cold(3);
foreach (int[] parents in new[] { new[] { 0, 0, 0 }, new[] { 2, 0, 2 }, new[] { 1, 0 }, new[] { 1 } })
{
    byte[] rows = new byte[parents.Length * 48];
    for (int i = 0; i < parents.Length; i++)
    {
        var row = rows.AsSpan(i * 48, 48);
        BinaryPrimitives.WriteUInt64LittleEndian(row[8..], (ulong)(0x1002a + i));
        BinaryPrimitives.WriteUInt64LittleEndian(row[16..], (ulong)(0x100c8 + i));
        BinaryPrimitives.WriteUInt64LittleEndian(row[32..], i == 0 ? ulong.MaxValue : 0xffffffff0000ffffUL);
        BinaryPrimitives.WriteInt32LittleEndian(row[40..], parents[i]);
    }
    var before = Generation(generation); generation = generation.Append(rows, default);
    prepared.Add(new(before, Convert.ToBase64String(rows), Generation(generation)));
}
Reject(() => Math2Mode0PrefixGeneration.Cold(0)); Reject(() => Math2Mode0PrefixGeneration.Cold(17));
Reject(() => generation.Append(new byte[47], default));
byte[] invalidParent = new byte[48]; BinaryPrimitives.WriteInt32LittleEndian(invalidParent.AsSpan(40), generation.ActiveCount);
Reject(() => generation.Append(invalidParent, default));

var argmax = new List<ArgmaxCapture>();
uint random = 12345;
for (int i = 0; i < 320; i++)
{
    float[] values = new float[1 + i % 297];
    for (int j = 0; j < values.Length; j++)
    {
        random = unchecked(random * 1664525 + 1013904223);
        values[j] = (i % 8) switch { 0 => -float.MaxValue, 1 => float.NaN,
            2 => float.NegativeInfinity, 3 => 0f, _ => (int)(random % 2000) - 1000f };
    }
    if (i % 8 == 4) values[^1] = float.PositiveInfinity;
    byte[] bytes = new byte[values.Length * 4]; Buffer.BlockCopy(values, 0, bytes, 0, bytes.Length);
    argmax.Add(new(Convert.ToBase64String(bytes), Math2Mode0ActualSidecars.FirstStrictMaximum(values)));
}

var recordFailures = new List<RecordFailure>();
foreach (var stage in Enum.GetValues<Math2Mode0CompletedStage>())
foreach (bool cancelAfterWrite in new[] { false, true })
{
    using var owner = Create();
    using var recorder = owner.CreateCompletedRecords();
    for (int warm = 0; warm < 2; warm++)
    {
        var step = owner.RunStep();
        owner.ApplySelected(step.CreateCandidates(1f, 1000f).Select());
    }
    var actualStep = owner.RunStep();
    var candidates = actualStep.CreateCandidates(1f, 1000f);
    if (candidates.CopyCompletedScoreFacts().Length != 3)
        throw new InvalidDataException("Failure seam requires the observed actual three EOS events.");
    var lease = recorder.Capture(candidates, 0);
    using var cancel = new CancellationTokenSource();
    var log = new List<StageEvent>();
    RejectCompleted(() => recorder.AppendDiagnostic(lease, (at, index) =>
    {
        log.Add(new(at.ToString(), index, RecordState(recorder)));
        if (at == stage)
        {
            if (cancelAfterWrite) cancel.Cancel();
            else throw new InvalidDataException("Prepared logical failure at " + at);
        }
    }, cancel.Token));
    if (!owner.IsFailed || !recorder.IsFailed)
        throw new InvalidDataException("A partial completed-record failure remained reusable.");
    recordFailures.Add(new(stage.ToString(), cancelAfterWrite, Convert.ToBase64String(lease.CopyRawRecord()),
        lease.CopyPrimary(), lease.CopySecondary(), lease.Frontier.Value, log.ToArray(), RecordState(recorder)));
    RejectCompleted(() => recorder.Append(lease));
    RejectCompleted(() => recorder.Capture(candidates, 1));
    RejectCompleted(() => owner.RunStep());
    RejectCompleted(() => owner.ApplySelected(candidates.Select()));
    var retained = recorder.CopyRecords();
    recorder.Dispose(); recorder.Dispose();
    if (retained.Any(row => !row.Library.IsDisposed || !row.Library.IsPoolReleased || row.Library.ReleaseCount != 1))
        throw new InvalidDataException("Owned retained libraries were not released.");
}
// Callback-free owner disposal must also release child libraries once.
using (var owner = Create(1))
{
    var recorder = owner.CreateCompletedRecords();
    for (int i = 0; i < 2; i++)
    { var step = owner.RunStep(); owner.ApplySelected(step.CreateCandidates(1f, -1f).Select()); }
    var last = owner.RunStep(); var c = last.CreateCandidates(1f, -1f);
    recorder.Append(recorder.Capture(c, 0));
    var retained = recorder.CopyRecords();
    owner.Dispose();
    if (!retained.Single().Library.IsDisposed) throw new InvalidDataException("Context did not dispose its vector.");
    RejectCompleted(() => recorder.Capture(c, 0));
}

var preparedAttention = new List<PreparedAttention>();
foreach (var c in new[] { (S:1,B:1,N:1,P:0), (S:13,B:3,N:5,P:2), (S:17,B:8,N:31,P:0), (S:3,B:16,N:101,P:15) })
{
    float[] input = Enumerable.Range(0,c.S*c.B*c.N).Select(i=>(float)(i-100)).ToArray();
    using var value = runtime.Tensor(input,c.S,c.B,c.N);
    var result = value.CopyTransposedF32Beam(c.P,c.B,c.N,default);
    preparedAttention.Add(new(c.P, Tensor("source",1,[c.S,c.B,c.N],input,null),
        Tensor("selected",1,[c.N,c.S],result.Values,null)));
    RejectAttention(()=>value.CopyTransposedF32Beam(-1,c.B,c.N,default));
    RejectAttention(()=>value.CopyTransposedF32Beam(c.B,c.B,c.N,default));
    RejectAttention(()=>value.CopyTransposedF32Beam(c.P,c.B+1,c.N,default));
    RejectAttention(()=>value.CopyTransposedF32Beam(c.P,c.B,c.N+1,default));
}
using (var invalidRank=runtime.Tensor(new float[4],2,2)) RejectAttention(()=>invalidRank.CopyTransposedF32Beam(0,2,2,default));
using (var invalidValues=runtime.Tensor(new[]{float.NaN},1,1,1)) RejectAttention(()=>invalidValues.CopyTransposedF32Beam(0,1,1,default));
using (var oversized=runtime.Tensor(new float[1001000],1001,1,1000))
{
    long allocated=GC.GetAllocatedBytesForCurrentThread();
    RejectAttention(()=>oversized.CopyTransposedF32Beam(0,1,1000,default));
    if(GC.GetAllocatedBytesForCurrentThread()-allocated>65536)throw new InvalidDataException("Oversized output allocated before budget refusal.");
}

// Any diagnostic prefix history is refused even when all chosen labels happen
// to have compatible observable status. No numerical-history proxy is accepted.
using (var diagnostic = Create())
{
    using var recorder = diagnostic.CreateCompletedRecords();
    var step = diagnostic.RunStep();
    diagnostic.ApplySelected(step.BindDiagnosticParents([0], [45]));
    bool tested = false;
    for (int i = 0; i < 4; i++)
    {
        step = diagnostic.RunStep(); var candidates = step.CreateCandidates(1f, -1f);
        if (candidates.CopyCompletedScoreFacts().Length > 0)
        {
            var lease = recorder.Capture(candidates, 0);
            RejectAttention(() => lease.CaptureAttention());
            recorder.Append(lease); tested = true;
            break;
        }
        diagnostic.ApplySelected(candidates.Select());
    }
    if (!tested) throw new InvalidDataException("Diagnostic refusal did not reach actual EOS.");
}
foreach (var snapshot in snapshots)
{
    // Contexts have all been disposed; owned values still need no live ORT state.
    if (snapshot.CopyAttention().Length != snapshot.Symbols * snapshot.Segments)
        throw new InvalidDataException("Snapshot lifetime depended on disposed tensor owner.");
}
File.WriteAllText(args[3] + ".attention.json", JsonSerializer.Serialize(new AttentionCaptureSet(attention.ToArray(), preparedAttention.ToArray(), attentionGuards), CaptureJson.Default.AttentionCaptureSet));
Console.WriteLine($"PASS{attention.Count} actual event-bound attention snapshots/{attentionGuards} guards; owned copies survive Apply/disposal");

for(int trial=0;trial<512;trial++)
{
    int rows=1+trial%17, columns=1+trial%31;
    int[] indices=Enumerable.Range(0,rows).Select(i=>i*3+trial%3).ToArray();
    float[] probabilities=Enumerable.Range(0,rows*columns).Select(i=>trial%7==0?0.5f:(float)((i*37+trial*13)%11)/10f).ToArray();
    var assigned=LegacyPreparedFreshMathSegments.AssignPreparedProbabilities(probabilities,indices,columns);
    var compressedRuns=LegacyPreparedFreshMathSegments.CompressPrepared(assigned);
    segmentControls.Add(new(indices,Tensor("probabilities",1,[rows,columns],probabilities,null),assigned,compressedRuns));
}
RejectSegments(()=>LegacyPreparedFreshMathSegments.AssignPreparedProbabilities([0f],[],1));
RejectSegments(()=>LegacyPreparedFreshMathSegments.AssignPreparedProbabilities([0f],[0],0));
RejectSegments(()=>LegacyPreparedFreshMathSegments.AssignPreparedProbabilities([0f,1f],[0],1));
RejectSegments(()=>LegacyPreparedFreshMathSegments.AssignPreparedProbabilities([0f,1f],[1,1],1));
RejectSegments(()=>LegacyPreparedFreshMathSegments.AssignPreparedProbabilities([0f,1f],[2,1],1));
RejectSegments(()=>LegacyPreparedFreshMathSegments.AssignPreparedProbabilities([0f],[-1],1));
RejectSegments(()=>LegacyPreparedFreshMathSegments.AssignPreparedProbabilities([0f],[1000],1));
foreach(float invalid in new[]{float.NaN,float.PositiveInfinity,-0.1f,1.1f})RejectSegments(()=>LegacyPreparedFreshMathSegments.AssignPreparedProbabilities([invalid],[0],1));
RejectSegments(()=>LegacyPreparedFreshMathSegments.AssignPreparedProbabilities([0f],[0],1,new CancellationToken(true)));
RejectSegments(()=>LegacyPreparedFreshMathSegments.CompressPrepared([-1]));
RejectSegments(()=>LegacyPreparedFreshMathSegments.CompressPrepared([1000]));
RejectSegments(()=>LegacyPreparedFreshMathSegments.CompressPrepared([0],new CancellationToken(true)));
if(LegacyPreparedFreshMathSegments.CompressPrepared([]).Length!=0)throw new InvalidDataException("Empty compression changed.");
File.WriteAllText(args[3]+".segments.json",JsonSerializer.Serialize(new SegmentsCapture(segmentEvents.ToArray(),segmentControls.ToArray(),segmentGuards),CaptureJson.Default.SegmentsCapture));
Console.WriteLine($"PASS{segmentEvents.Count} actual prepared-branch segment maps/512 controls/{segmentGuards} guards; no EF4 policy/output/score");

var capture = new Capture(runtime.Version, model.Graphs.Select(x => Convert.ToHexStringLower(SHA256.HashData(x.Span))).ToArray(),
    Convert.ToHexStringLower(SHA256.HashData(resource)), features.Frames, runs.ToArray(), failures.ToArray(),
    prepared.ToArray(), argmax.ToArray(), guards);
File.WriteAllText(args[3], JsonSerializer.Serialize(capture, CaptureJson.Default.Capture));
File.WriteAllText(args[3] + ".bounds.json", JsonSerializer.Serialize(searchBounds.ToArray(), CaptureJson.Default.SearchBoundCaptureArray));
File.WriteAllText(args[3] + ".records.json", JsonSerializer.Serialize(new CompletedCapture(
    recordedSteps.ToArray(), recordFailures.ToArray(), frontierControls.ToArray(), completedGuards), CaptureJson.Default.CompletedCapture));
Console.WriteLine($"PASS11 actual owned EOS records +{recordFailures.Count} prepared failure/cancel boundaries; {completedGuards} record guards; no full search loop");

record TensorCapture(string Name, int Type, long[] Shape, string Bytes);
record GenerationCapture(long Offset, int ActiveCount, ushort[][] Primary, ushort[][] Secondary, int[][] Ancestry);
record StepCapture(bool Diagnostic, TensorCapture[] Before, TensorCapture[] Outputs, string Selected48,
    TensorCapture[] After, GenerationCapture PrefixBefore, GenerationCapture PrefixAfter,
    TensorCapture Prior, TensorCapture Logs, TensorCapture Costs, string Information64, string Conditional32,
    string Heap, string Sorted, string AfterEos, string AfterResort, EosCapture[] CompletedScores)
{
    internal float[] SelectedCosts()
    {
        byte[] rows = Convert.FromBase64String(Selected48);
        return Enumerable.Range(0, rows.Length / 48).Select(i => BinaryPrimitives.ReadSingleLittleEndian(rows.AsSpan(i * 48))).ToArray();
    }
}
record RunCapture(string Kind, int Width, float Delta, TensorCapture[] Initial, GenerationCapture InitialGeneration, StepCapture[] Steps);
record FailureCapture(bool Canceled, GenerationCapture Before, string Selected48, GenerationCapture After);
record PreparedCapture(GenerationCapture Before, string Selected48, GenerationCapture After);
record ArgmaxCapture(string ValuesF32, long Index);
record EosCapture(int Rank, int SourceBeam, float CandidateCost, float CompletedScore, ushort[] Prefix);
record Capture(string Runtime, string[] Models, string Resource, int Frames, RunCapture[] Runs,
    FailureCapture[] Failures, PreparedCapture[] PreparedPlanes, ArgmaxCapture[] PreparedArgmax, int Guards);
record SearchBoundCapture(long OriginalInputFrames, long EncoderMaximumPredictionCount, ulong ColdMaximumPosition);
record RecordFields(float Score, long Frontier, int Parent, ushort[] Primary, int PrimaryCapacity,
    ushort[] Secondary, int SecondaryCapacity, int LibraryIdentity, bool LibraryDisposed,
    int LibraryObjectBytes, int LibraryPoolBytes, ulong[] LibraryInitializedQwords, uint LibraryInitializedDword10, int NestedTensorCount);
record RecordState(int Count, int Capacity, bool Failed, int CreatedLibraries, int ReleasedUnadoptedLibraries, RecordFields[] Records);
record RecordEvent(int Rank, long Offset, string Raw48, ushort[] Primary, ushort[] Secondary,
    int[][] Ancestry, long Frontier, RecordState After);
record RecordStep(int Width, float Delta, long Offset, long Frontier, bool Common, int[] RawArgmax,
    RecordState Before, RecordEvent[] Events, RecordState After);
record StageEvent(string Stage, int Index, RecordState State);
record RecordFailure(string Stage, bool Canceled, string Raw48, ushort[] Primary,
    ushort[] Secondary, long Frontier, StageEvent[] Events, RecordState After);
record FrontierControl(ushort[][] Secondary, string LogitsF32, int Vocabulary, long Previous, long Frontier, bool Common, int[] Argmax);
record CompletedCapture(RecordStep[] Steps, RecordFailure[] Failures, FrontierControl[] PreparedFrontiers, int Guards);
record AttentionCapture(int Width, float Delta, long Offset, int Rank, int Parent, int Symbols, int Segments, TensorCapture Attention, TensorCapture Sigmoid);
record PreparedAttention(int Parent, TensorCapture Source, TensorCapture Selected);
record AttentionCaptureSet(AttentionCapture[] Events, PreparedAttention[] Prepared, int Guards);
record SegmentsEvent(int Width,float Delta,long Offset,int Rank,int[] ObservableIndices,int[] Assignments,MathSegmentRun[] Runs);
record SegmentsControl(int[] Indices,TensorCapture Probabilities,int[] Assignments,MathSegmentRun[] Runs);
record SegmentsCapture(SegmentsEvent[] Events,SegmentsControl[] Prepared,int Guards);
[JsonSerializable(typeof(SegmentsCapture))]
[JsonSerializable(typeof(AttentionCaptureSet))]
[JsonSerializable(typeof(CompletedCapture))]
[JsonSerializable(typeof(Capture))]
[JsonSerializable(typeof(SearchBoundCapture[]))]
partial class CaptureJson : JsonSerializerContext;
