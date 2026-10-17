namespace InkRecognition.Math;

internal enum Math2Mode0RecordingObservationStage { BeforeRecording, AfterRecording, AfterApply }
internal sealed record Math2Mode0RecordingIteration(ulong Position, long PreviousOffset, int PreviousBeams,
    int OrdinaryEosEvents, Math2Mode0FallbackOutcome Fallback, int[] FallbackRanks,
    int CompletedCount, bool Applied);

/// <summary>Owning cold numerical recording loop through the exact maximum
/// position. Stops BEFORE039A96 completed ordering, attention/EF4 and output.
/// No host progress callback is bound or final formula acceptance inferred.</summary>
internal sealed partial class Math2Mode0ColdRecordingLoop : IDisposable
{
    private readonly object gate = new();
    private readonly Math2Mode0PrefixContext prefix;
    private readonly Math2Mode0CompletedRecords records;
    private readonly float temperature, delta;
    private readonly List<Math2Mode0RecordingIteration> iterations = new();
    private bool claimed, finished, failed, disposed;
    private ulong attemptedPosition;
    private int successfulDecoderCalls;
    internal bool UsesColdDefaults { get; }
    internal int BeamWidth => prefix.BeamWidth;
    internal long InputFrameCount => prefix.InputFrameCount;
    internal long EncoderMaximumPredictionCount => prefix.EncoderMaximumPredictionCount;
    internal ulong MaximumPosition => prefix.ColdMaximumPosition;
    internal ulong AttemptedPosition { get { lock (gate) return attemptedPosition; } }
    internal int SuccessfulDecoderCalls { get { lock (gate) return successfulDecoderCalls; } }
    internal bool IsFailed { get { lock (gate) return failed; } }
    internal bool IsFinishedRecording { get { lock (gate) return finished; } }

    private Math2Mode0ColdRecordingLoop(Math2Mode0PrefixContext prefix,
        bool coldDefaults, float temperature, float delta)
    {
        this.prefix = prefix; this.temperature = temperature; this.delta = delta;
        UsesColdDefaults = coldDefaults;
        records = prefix.CreateCompletedRecords();
    }

    // Cold EED constructor + fresh unconfigured EEB request + exact raw3
    // metadata: width1, temperature1, decoder delta_cost_max=-1. These constants
    // are not inferred from resource mode0 or a tensor dimension string.
    internal static Math2Mode0ColdRecordingLoop PrepareColdDefault(Math2Mode0ColdBundle bundle,
        OrtRun run, OrtTensor source, OrtTensor downsampling, OrtTensor strokeEnds,
        CancellationToken token = default)
        => PrepareCore(bundle, run, source, downsampling, strokeEnds, true, 1, 1f, -1f, token);

    internal static Math2Mode0ColdRecordingLoop PrepareWithPreparedPolicy(Math2Mode0ColdBundle bundle,
        OrtRun run, OrtTensor source, OrtTensor downsampling, OrtTensor strokeEnds,
        int preparedWidth, float preparedTemperature, float preparedDelta,
        CancellationToken token = default)
        => PrepareCore(bundle, run, source, downsampling, strokeEnds, false,
            preparedWidth, preparedTemperature, preparedDelta, token);

    private static Math2Mode0ColdRecordingLoop PrepareCore(Math2Mode0ColdBundle bundle,
        OrtRun run, OrtTensor source, OrtTensor downsampling, OrtTensor strokeEnds,
        bool coldDefaults, int width, float temperature, float delta, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (!float.IsFinite(temperature) || temperature <= 0f || !float.IsFinite(delta))
            throw new NotSupportedException("Only finite prepared delta and positive finite temperature are supported.");
        var prefix = Math2Mode0PrefixContext.Prepare(bundle, run, source, downsampling, strokeEnds, width, token);
        try { return new(prefix, coldDefaults, temperature, delta); }
        catch { prefix.Dispose(); throw; }
    }

    internal void RunToRecordedPrefix(CancellationToken token = default) => RunCore(null, token);

    // Friend-only source/capture/failure observer, not the native host progress
    // callback and not present in the production recording path.
    internal void RunDiagnostic(Action<Math2Mode0RecordingObservationStage, Math2Mode0PrefixStep,
        Math2Mode0PrefixCandidates, Math2Mode0PrefixContext, Math2Mode0CompletedRecords,
        Math2Mode0LastPositionFallback?> observer, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(observer);
        RunCore(observer, token);
    }

    private void RunCore(Action<Math2Mode0RecordingObservationStage, Math2Mode0PrefixStep,
        Math2Mode0PrefixCandidates, Math2Mode0PrefixContext, Math2Mode0CompletedRecords,
        Math2Mode0LastPositionFallback?>? observer, CancellationToken token)
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            token.ThrowIfCancellationRequested();
            if (claimed) throw new InvalidOperationException("A recording loop is single-use, including after failure.");
            claimed = true;
            try
            {
                while (true)
                {
                    token.ThrowIfCancellationRequested();
                    ulong previous = attemptedPosition;
                    attemptedPosition = checked(previous + 1UL);
                    // Native compares the previous position after increment,
                    // before scoring. There is no extra NN call at max+1.
                    if (previous >= MaximumPosition) break;
                    var step = prefix.RunStep(token);
                    successfulDecoderCalls++;
                    var candidates = step.CreateCandidates(temperature, delta, token);
                    observer?.Invoke(Math2Mode0RecordingObservationStage.BeforeRecording,
                        step, candidates, prefix, records, null);
                    token.ThrowIfCancellationRequested();
                    int eosEvents = candidates.CopyCompletedScoreFacts().Length;
                    var phase = records.RecordOrdinaryEosPhase(candidates, token);
                    var fallback = records.PrepareLastPositionFallback(candidates, phase, token);
                    while (fallback.CaptureNext(token) is { } lease)
                        records.AppendFallback(lease, token);
                    iterations.Add(new(attemptedPosition, step.Generation.Offset, step.Generation.ActiveCount,
                        eosEvents, fallback.Outcome, fallback.CopyEligibleRanks(), records.Count, false));
                    observer?.Invoke(Math2Mode0RecordingObservationStage.AfterRecording,
                        step, candidates, prefix, records, fallback);
                    token.ThrowIfCancellationRequested();
                    prefix.ApplySelected(candidates.Select(), token);
                    iterations[^1] = iterations[^1] with { Applied = true };
                    observer?.Invoke(Math2Mode0RecordingObservationStage.AfterApply,
                        step, candidates, prefix, records, fallback);
                    token.ThrowIfCancellationRequested();
                }
                finished = true;
            }
            catch
            {
                failed = true;
                prefix.WithCompletedStorage(() => { prefix.FailCompletedRecording(); return true; });
                throw;
            }
        }
    }

    internal Math2Mode0RecordingIteration[] CopyIterations()
    {
        lock (gate) return iterations.Select(value => value with { FallbackRanks = value.FallbackRanks.ToArray() }).ToArray();
    }
    internal Math2Mode0PrefixGeneration CopyPrefix()
    { lock (gate) return prefix.CopyGeneration(); }
    internal Math2Mode0CompletedRecordSnapshot[] CopyRecordedPrefix()
    { lock (gate) return records.CopyRecords(); }
    public void Dispose()
    {
        lock (gate)
        {
            if (disposed) return;
            disposed = true; prefix.Dispose();
        }
    }
}
