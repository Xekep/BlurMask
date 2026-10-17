using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
using InkRecognition.Math;

if (args.Length != 4) throw new ArgumentException("model.ikrmath rawMath2.res official-runtime capture.json");
using var runtime = new OrtRuntime(Path.GetFullPath(args[2]));
using var run = runtime.CreateRun(CancellationToken.None);
byte[] resource = File.ReadAllBytes(args[1]);
using var pack = File.OpenRead(args[0]);
using var bundle = Math2Mode0ColdBundle.Read(runtime, pack, resource);
var gate = LegacyMath2ObservableTokens.BindBundledResource(resource);
int guards = 0;
var namedGuards = new List<string>();
void RejectNamed(string name, Action action) { Reject(action); namedGuards.Add(name); }
void Reject(Action action)
{
    try { action(); }
    catch (Exception e) when (e is ArgumentException or InvalidOperationException or InvalidDataException or OperationCanceledException or NotSupportedException)
    { guards++; return; }
    throw new InvalidDataException("Required recording-loop/phase guard did not reject.");
}
Tensor Tensor(string name, int type, long[] shape, float[]? values, long[]? integers = null)
{
    byte[] bytes = new byte[(values?.Length ?? integers!.Length) * (type == 1 ? 4 : 8)];
    Buffer.BlockCopy((Array?)values ?? integers!, 0, bytes, 0, bytes.Length);
    return new(name, type, shape, Convert.ToBase64String(bytes));
}
Tensor[] Inputs(Math2Mode0PrefixContext prefix) => prefix.CopyTensorInputs()
    .Select(value => Tensor(value.Name, value.Type, value.Shape, value.Floats, value.Int64s)).ToArray();
Tensor[] Outputs(Math2Mode0PrefixStep step) => Enumerable.Range(0, 4).Select(index =>
{
    var output = step.CopyOutput(index);
    return Tensor(index.ToString(), 1, output.Shape, output.Values);
}).ToArray();
Generation Generation(Math2Mode0PrefixGeneration generation) => new(generation.Offset,
    generation.ActiveCount, generation.CopyPrimary(), generation.CopySecondary(), generation.CopyAncestry());
Record[] Records(Math2Mode0CompletedRecords recorder)
    => recorder.CopyRecords().Select((row, index) =>
    {
        string kind; int rank; long offset; byte[] raw;
        ushort[] primary, secondary; int[][] ancestry;
        try
        {
            var source = recorder.ReadSourceEvent(index); kind = "OrdinaryEos";
            rank = source.Rank; offset = source.Offset; raw = source.CopyRawRecord();
            primary = source.CopyPrimary(); secondary = source.CopySecondary(); ancestry = source.CopyAncestry();
            Reject(() => recorder.ReadFallbackSourceEvent(index));
        }
        catch (InvalidOperationException)
        {
            var source = recorder.ReadFallbackSourceEvent(index); kind = "LastPositionFallback";
            rank = source.Rank; offset = source.Offset; raw = source.CopyRawRecord();
            primary = source.CopyPrimary(); secondary = source.CopySecondary(); ancestry = source.CopyAncestry();
            Reject(() => recorder.Append(source.Content));
        }
        return new Record(row.Score, row.Frontier, row.Parent, row.Primary, row.PrimaryCapacity,
            row.Secondary, row.SecondaryCapacity, row.Library.LogicalIdentity,
            row.Library.CopyInitializedPoolQwords(), row.NestedTensorCount,
            new(kind, rank, offset, Convert.ToBase64String(raw), primary, secondary, ancestry));
    }).ToArray();

// Fixed geometric catalogue; no expected strings are read or compared.
InkStroke[][] fixtures =
[
    [new([new(0, 30), new(0, 0), new(10, 18), new(20, 0), new(20, 30)]), new([new(35, 0), new(35, 30)])],
    [new([new(0, 0), new(0, 30)])],
    [new([new(0, 0), new(30, 30)]), new([new(30, 0), new(0, 30)])],
    [new(Enumerable.Range(0, 20).Select(i => new InkPoint(i * 5, i % 2 * 30)).ToArray())],
    [new(Enumerable.Range(0, 17).Select(i => new InkPoint((float)(15 + 15 * System.Math.Cos(i * System.Math.PI / 8)),
        (float)(15 + 15 * System.Math.Sin(i * System.Math.PI / 8)))).ToArray())],
    Enumerable.Range(0, 3).Select(i => new InkStroke([new(i * 30, 0), new(i * 30, 30)])).ToArray(),
    Enumerable.Range(0, 8).Select(i => new InkStroke([new(i * 20, 0), new(i * 20, 30)])).ToArray(),
    [new([new(0, 0), new(10, 30), new(20, 0), new(30, 30), new(40, 0)]), new([new(50, 0), new(60, 30), new(70, 0)])]
];
var captures = new List<RunCapture>();
foreach (int fixtureIndex in Enumerable.Range(0, fixtures.Length))
foreach (var policy in fixtureIndex == 0 ? new[] { (Cold: true, Width: 1, Delta: -1f),
    (Cold: false, Width: 3, Delta: 1000f), (Cold: false, Width: 3, Delta: -1f) }
    : new[] { (Cold: true, Width: 1, Delta: -1f) })
{
    var features = MathPreprocessor.Process(fixtures[fixtureIndex]);
    using var source = runtime.Tensor(features.Values, features.Frames, 1, 26);
    using var downsampling = runtime.Tensor(Enumerable.Repeat(1L, features.Frames).ToArray(), features.Frames, 1);
    using var ends = runtime.Tensor(features.StrokeEnds, 1, features.StrokeEnds.Length);
    using var loop = policy.Cold
        ? Math2Mode0ColdRecordingLoop.PrepareColdDefault(bundle, run, source, downsampling, ends)
        : Math2Mode0ColdRecordingLoop.PrepareWithPreparedPolicy(bundle, run, source, downsampling, ends, policy.Width, 1f, policy.Delta);
    using var canceled = new CancellationTokenSource(); canceled.Cancel();
    Reject(() => loop.RunToRecordedPrefix(canceled.Token));
    var steps = new List<Step>(); Step? current = null;
    loop.RunDiagnostic((stage, step, candidates, prefix, recorder, fallback) =>
    {
        if (stage == Math2Mode0RecordingObservationStage.BeforeRecording)
        {
            var before = Generation(prefix.CopyGeneration());
            current = new(loop.AttemptedPosition, before, Inputs(prefix), Outputs(step),
                Tensor("prior", 1, [before.ActiveCount], step.CopyPriorCosts()),
                Tensor("logs", 1, [before.ActiveCount, 297], candidates.CopyLogProbabilities()),
                Tensor("costs", 1, [before.ActiveCount, 297], candidates.CopyUnmaskedCosts()),
                Convert.ToBase64String(candidates.CopyInformation()), Convert.ToBase64String(candidates.CopyConditional()),
                Convert.ToBase64String(candidates.CopyHeapStorage()), Convert.ToBase64String(candidates.CopySortedStorage()),
                Convert.ToBase64String(candidates.CopyAfterEos()), Convert.ToBase64String(candidates.CopyAfterResort()),
                candidates.CopyCompletedScoreFacts().Select(value => new Eos(value.Rank, value.SourceBeam,
                    value.CandidateCost, value.CompletedScore, value.Prefix)).ToArray(), Records(recorder));
        }
        else if (stage == Math2Mode0RecordingObservationStage.AfterRecording)
        {
            var frontier = step.ReadCompletedFrontier(default);
            current = current! with { Fallback = fallback!.Outcome.ToString(),
                FallbackRanks = fallback.CopyEligibleRanks(), RecordsAfter = Records(recorder),
                Frontier = frontier.Value, Common = frontier.Common, RawArgmax = frontier.CopyArgmax(),
                Selected48 = Convert.ToBase64String(candidates.Select().CopyRecords()) };
            Reject(() => recorder.PrepareLastPositionFallback(candidates,
                new Math2Mode0RecordedStepPhase(recorder, candidates, step)));
        }
        else
        {
            current = current! with { After = Inputs(prefix), PrefixAfter = Generation(prefix.CopyGeneration()) };
            steps.Add(current);
            Reject(() => step.CopyOutput(0));
            Reject(() => fallback!.CaptureNext());
        }
    });
    if (!loop.IsFinishedRecording || loop.IsFailed || loop.SuccessfulDecoderCalls != (int)loop.MaximumPosition ||
        loop.AttemptedPosition != loop.MaximumPosition + 1 || loop.CopyPrefix().Offset != (long)loop.MaximumPosition)
        throw new InvalidDataException("Exact native bound/counter prefix was not preserved.");
    Reject(() => loop.RunToRecordedPrefix());
    captures.Add(new(fixtureIndex, policy.Cold, policy.Width, policy.Delta, features.Frames,
        loop.EncoderMaximumPredictionCount, loop.MaximumPosition, loop.AttemptedPosition,
        loop.SuccessfulDecoderCalls, steps.ToArray(), loop.CopyIterations()));
}

var failures = new List<Failure>();
foreach (var selectedStage in Enum.GetValues<Math2Mode0RecordingObservationStage>())
foreach (bool cancel in new[] { false, true })
{
    var features = MathPreprocessor.Process(fixtures[0]);
    using var source = runtime.Tensor(features.Values, features.Frames, 1, 26);
    using var downsampling = runtime.Tensor(Enumerable.Repeat(1L, features.Frames).ToArray(), features.Frames, 1);
    using var ends = runtime.Tensor(features.StrokeEnds, 1, features.StrokeEnds.Length);
    using var loop = Math2Mode0ColdRecordingLoop.PrepareColdDefault(bundle, run, source, downsampling, ends);
    using var token = new CancellationTokenSource();
    Generation? before = null; Record[] retained = [];
    Reject(() => loop.RunDiagnostic((stage, step, candidates, prefix, recorder, fallback) =>
    {
        if (step.Generation.Offset != 2) return;
        if (stage == Math2Mode0RecordingObservationStage.BeforeRecording) before = Generation(step.Generation);
        if (stage != selectedStage) return;
        retained = Records(recorder);
        if (cancel) token.Cancel();
        else throw new InvalidDataException("Prepared outer recording observer failure.");
    }, token.Token));
    if (!loop.IsFailed || loop.IsFinishedRecording) throw new InvalidDataException("Failed recording loop resumed.");
    Reject(() => loop.RunToRecordedPrefix());
    failures.Add(new(selectedStage.ToString(), cancel, loop.AttemptedPosition, loop.SuccessfulDecoderCalls,
        before!, Generation(loop.CopyPrefix()), retained, loop.CopyIterations()));
}

var prepared = new List<Prepared>();
ushort observable = (ushort)Enumerable.Range(0, 297).First(index => gate.IsObservable((ushort)index));
for (int control = 0; control < 1024; control++)
{
    int width = 1 + control % 16, beams = 1 + control % System.Math.Min(width, 4);
    ushort[][] old = Enumerable.Range(0, beams).Select(beam => (control % 5) switch
    {
        0 => Array.Empty<ushort>(), 1 => new ushort[] { 0, 1, 262 },
        2 => new ushort[] { 262, observable, 0 },
        3 => beam % 2 == 0 ? new ushort[] { observable } : new ushort[] { 262 },
        _ => new ushort[] { 65535, observable, 262 }
    }).ToArray();
    byte[] rows = new byte[width * 2 * 48];
    for (int rank = 0; rank < width * 2; rank++)
    {
        var row = rows.AsSpan(rank * 48, 48);
        for (int at = 0; at < 48; at++) row[at] = unchecked((byte)(control * 17 + rank * 23 + at));
        float value = ((control + rank) % 9) switch
        {
            0 => float.MaxValue, 1 => float.NaN, 2 => float.PositiveInfinity,
            3 => float.NegativeInfinity, 4 => -1f, _ => rank
        };
        BinaryPrimitives.WriteSingleLittleEndian(row, value);
        BinaryPrimitives.WriteInt32LittleEndian(row[40..], rank % beams);
    }
    byte[] unchanged = rows.ToArray();
    var eligible = LegacyPreparedMathLastPositionFallback.EligibleRanks(rows, width, old, gate);
    if (!rows.AsSpan().SequenceEqual(unchanged)) throw new InvalidDataException("Prepared input pool mutated.");
    prepared.Add(new(width, old, Convert.ToBase64String(rows), eligible));
}

// Ordered history is a producer capability: skipping earlier phases cannot
// later authorize a fallback merely by leaving the completed vector empty.
{
    var features = MathPreprocessor.Process(fixtures[0]);
    using var source = runtime.Tensor(features.Values, features.Frames, 1, 26);
    using var downsampling = runtime.Tensor(Enumerable.Repeat(1L, features.Frames).ToArray(), features.Frames, 1);
    using var ends = runtime.Tensor(features.StrokeEnds, 1, features.StrokeEnds.Length);
    using var prefix = Math2Mode0PrefixContext.Prepare(bundle, run, source, downsampling, ends, 1);
    using var recorder = prefix.CreateCompletedRecords();
    var first = prefix.RunStep(); var firstCandidates = first.CreateCandidates(1f, -1f);
    Reject(() => recorder.PrepareLastPositionFallback(firstCandidates,
        new Math2Mode0RecordedStepPhase(recorder, firstCandidates, first)));
    prefix.ApplySelected(firstCandidates.Select());
    var second = prefix.RunStep(); var secondCandidates = second.CreateCandidates(1f, -1f);
    Reject(() => recorder.RecordOrdinaryEosPhase(secondCandidates));
    Reject(() => recorder.PrepareLastPositionFallback(secondCandidates,
        new Math2Mode0RecordedStepPhase(recorder, secondCandidates, second)));
}
// Separate exact-shape ownership/cancellation guards. No artificial successful
// last-position owner is created by these deliberately rejected constructors.
{
    var features = MathPreprocessor.Process(fixtures[0]);
    using var source = runtime.Tensor(features.Values, features.Frames, 1, 26);
    using var downsampling = runtime.Tensor(Enumerable.Repeat(1L, features.Frames).ToArray(), features.Frames, 1);
    using var ends = runtime.Tensor(features.StrokeEnds, 1, features.StrokeEnds.Length);
    using var canceled = new CancellationTokenSource(); canceled.Cancel();
    RejectNamed("prepare-entry-cancel", () => Math2Mode0ColdRecordingLoop.PrepareColdDefault(bundle, run,
        source, downsampling, ends, canceled.Token));
    RejectNamed("prepared-width-zero", () => Math2Mode0ColdRecordingLoop.PrepareWithPreparedPolicy(bundle, run,
        source, downsampling, ends, 0, 1f, -1f));
    RejectNamed("prepared-temperature-zero", () => Math2Mode0ColdRecordingLoop.PrepareWithPreparedPolicy(bundle, run,
        source, downsampling, ends, 1, 0f, -1f));
    RejectNamed("prepared-delta-nan", () => Math2Mode0ColdRecordingLoop.PrepareWithPreparedPolicy(bundle, run,
        source, downsampling, ends, 1, 1f, float.NaN));
    using var a = Math2Mode0PrefixContext.Prepare(bundle, run, source, downsampling, ends, 1);
    using var b = Math2Mode0PrefixContext.Prepare(bundle, run, source, downsampling, ends, 1);
    using var ar = a.CreateCompletedRecords(); using var br = b.CreateCompletedRecords();
    var astep = a.RunStep(); var bstep = b.RunStep();
    var ac = astep.CreateCandidates(1f, -1f); var bc = bstep.CreateCandidates(1f, -1f);
    RejectNamed("ordinary-phase-entry-cancel", () => ar.RecordOrdinaryEosPhase(ac, canceled.Token));
    var phaseA = ar.RecordOrdinaryEosPhase(ac); var phaseB = br.RecordOrdinaryEosPhase(bc);
    RejectNamed("ordinary-phase-replay", () => ar.RecordOrdinaryEosPhase(ac));
    RejectNamed("ordinary-phase-foreign-candidate", () => ar.RecordOrdinaryEosPhase(bc));
    RejectNamed("fallback-foreign-phase", () => ar.PrepareLastPositionFallback(ac, phaseB));
    RejectNamed("fallback-foreign-candidate", () => ar.PrepareLastPositionFallback(bc, phaseA));
    RejectNamed("fallback-foreign-vector", () => br.PrepareLastPositionFallback(ac, phaseA));
    RejectNamed("fallback-unissued-phase", () => ar.PrepareLastPositionFallback(ac,
        new Math2Mode0RecordedStepPhase(ar, ac, astep)));
    RejectNamed("fallback-entry-cancel", () => ar.PrepareLastPositionFallback(ac, phaseA, canceled.Token));
    var nativeNoOp = ar.PrepareLastPositionFallback(ac, phaseA);
    if (nativeNoOp.Outcome != Math2Mode0FallbackOutcome.NotLastPosition || nativeNoOp.CopyEligibleRanks().Length != 0)
        throw new InvalidDataException("Not-last branch did not remain a true no-op.");
    RejectNamed("fallback-repeat-prepare", () => ar.PrepareLastPositionFallback(ac, phaseA));
    RejectNamed("fallback-empty-read-entry-cancel", () => nativeNoOp.CaptureNext(canceled.Token));
    if (nativeNoOp.CaptureNext() != null) throw new InvalidDataException("No-op fabricated a fallback lease.");
    var another = astep.CreateCandidates(1f, -1f);
    RejectNamed("ordered-phase-selection-identity", () => a.ApplySelected(another.Select()));
    var fakeBatch = new Math2Mode0LastPositionFallback(ar, astep, ac, ac.CopyAfterResort(),
        Math2Mode0FallbackOutcome.LastPositionEmpty, [0], a.ColdMaximumPosition, a.ColdMaximumPosition);
    RejectNamed("unregistered-fallback-batch", () => fakeBatch.CaptureNext());
    RejectNamed("forged-fallback-static-factory", () => Math2Mode0CompletedEventLease.CaptureFallback(fakeBatch, default));
    bool invalidCallback = false;
    RejectNamed("unregistered-fallback-content-callback", () => fakeBatch.CaptureCanonical(data =>
    { invalidCallback = true; throw new InvalidDataException("An unregistered source reached construction."); }, default));
    if (invalidCallback) throw new InvalidDataException("Unregistered batch invoked content constructor.");
    if (ar.Count != 0 || ar.IsFailed || a.IsFailed) throw new InvalidDataException("Pure source guard consumed valid state.");
    a.ApplySelected(ac.Select());
    RejectNamed("stale-fallback-phase", () => ar.PrepareLastPositionFallback(ac, phaseA));
    RejectNamed("stale-fallback-pool", () => nativeNoOp.CaptureNext());
    RejectNamed("stale-phase-replay", () => ar.RecordOrdinaryEosPhase(ac));
    // Native skips the parent for FMAX, but JP sends NaN to the parent read.
    byte[] skip = new byte[96];
    BinaryPrimitives.WriteSingleLittleEndian(skip, float.MaxValue);
    BinaryPrimitives.WriteInt32LittleEndian(skip.AsSpan(40), -1);
    if (LegacyPreparedMathLastPositionFallback.EligibleRanks(skip, 1, [[]], gate).Length != 0)
        throw new InvalidDataException("FMAX parent was consumed.");
    BinaryPrimitives.WriteSingleLittleEndian(skip, float.NaN);
    RejectNamed("unordered-row-foreign-parent", () => LegacyPreparedMathLastPositionFallback.EligibleRanks(skip, 1, [[]], gate));
    BinaryPrimitives.WriteSingleLittleEndian(skip, 1f);
    RejectNamed("eligible-row-foreign-parent", () => LegacyPreparedMathLastPositionFallback.EligibleRanks(skip, 1, [[]], gate));
    RejectNamed("prepared-width-shape", () => LegacyPreparedMathLastPositionFallback.EligibleRanks(skip, 0, [[]], gate));
    RejectNamed("prepared-empty-prefix-table", () => LegacyPreparedMathLastPositionFallback.EligibleRanks(skip, 1, [], gate));
    RejectNamed("prepared-entry-cancel", () => LegacyPreparedMathLastPositionFallback.EligibleRanks(skip, 1, [[]], gate, canceled.Token));
}

File.WriteAllText(args[3], JsonSerializer.Serialize(new Capture(runtime.Version,
    Convert.ToHexStringLower(SHA256.HashData(resource)), captures.ToArray(), failures.ToArray(), prepared.ToArray(), namedGuards.ToArray(), guards),
    CaptureJson.Default.Capture));
Console.WriteLine($"PASS{captures.Count} actual cold/prepared-policy recording runs/{captures.Sum(value => value.DecoderCalls)} NN steps; " +
    $"{captures.Sum(value => value.Steps.Sum(step => step.FallbackRanks?.Length ?? 0))} actual fallback rows; " +
    $"1024 prepared eligibility controls/{failures.Count} outer-failure cases/{guards} guards; pre-EF4 only");

record Tensor(string Name, int Type, long[] Shape, string Bytes);
record Generation(long Offset, int ActiveCount, ushort[][] Primary, ushort[][] Secondary, int[][] Ancestry);
record SourceEvent(string Kind, int Rank, long Offset, string Raw48, ushort[] Primary, ushort[] Secondary, int[][] Ancestry);
record Record(float Score, long Frontier, int Parent, ushort[] Primary, int PrimaryCapacity,
    ushort[] Secondary, int SecondaryCapacity, int LibraryIdentity, ulong[] LibraryQwords,
    int NestedTensorCount, SourceEvent Source);
record Eos(int Rank, int SourceBeam, float CandidateCost, float CompletedScore, ushort[] Prefix);
record Step(ulong Position, Generation PrefixBefore, Tensor[] Before, Tensor[] Outputs, Tensor Prior,
    Tensor Logs, Tensor Costs, string Information64, string Conditional32, string Heap, string Sorted,
    string AfterEos, string AfterResort, Eos[] CompletedScores, Record[] RecordsBefore,
    string? Fallback = null, int[]? FallbackRanks = null, Record[]? RecordsAfter = null,
    long Frontier = 0, bool Common = false, int[]? RawArgmax = null, string? Selected48 = null,
    Tensor[]? After = null, Generation? PrefixAfter = null);
record RunCapture(int Fixture, bool ColdDefault, int Width, float Delta, int InputFrames,
    long EncoderMaximum, ulong Maximum, ulong Attempted, int DecoderCalls, Step[] Steps,
    Math2Mode0RecordingIteration[] Iterations);
record Failure(string Stage, bool Canceled, ulong Position, int DecoderCalls, Generation Before,
    Generation After, Record[] Records, Math2Mode0RecordingIteration[] Iterations);
record Prepared(int Width, ushort[][] OldPrimary, string Sorted48, int[] EligibleRanks);
record Capture(string Runtime, string Resource, RunCapture[] Runs, Failure[] Failures, Prepared[] Prepared, string[] NamedGuards, int Guards);
[JsonSerializable(typeof(Capture))]
partial class CaptureJson : JsonSerializerContext;
