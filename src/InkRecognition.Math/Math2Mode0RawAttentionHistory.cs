using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

[assembly: InternalsVisibleTo("LegacyMathRawAttentionHistory.Probe")]

namespace InkRecognition.Math;

internal sealed record Math2Mode0AttentionHistoryStatistics(long BudgetBytes, long ReservedPayloadBytes,
    long RawPayloadBytes, long SnapshotPayloadBytes, int RawFrameCount, int SnapshotCount,
    int PayloadAllocationAttempts, bool Complete, bool Failed, bool Disposed, string[] RawFrameHashes);
internal sealed record Math2Mode0RawAttentionFrameCopy(int Segments, int Beams, float[] Values);

internal sealed partial class Math2Mode0ColdRecordingLoop
{
    private Math2Mode0RawAttentionHistory? rawAttentionHistory;
    internal Math2Mode0RawAttentionHistory RunWithRawAttentionHistory(CancellationToken token = default)
        => RunRawHistoryCore(Math2Mode0RawAttentionHistory.PortableDefaultPayloadBudget, null, token);
    internal Math2Mode0RawAttentionHistory RunWithPreparedRawAttentionBudget(long preparedBudgetBytes,
        CancellationToken token = default) => RunRawHistoryCore(preparedBudgetBytes, null, token);
    internal Math2Mode0RawAttentionHistory RunRawAttentionDiagnostic(long preparedBudgetBytes,
        Action<Math2Mode0RecordingObservationStage, Math2Mode0PrefixStep, Math2Mode0PrefixCandidates,
            Math2Mode0PrefixContext, Math2Mode0CompletedRecords, Math2Mode0RawAttentionHistory> observer,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(observer);
        return RunRawHistoryCore(preparedBudgetBytes, observer, token);
    }
    private Math2Mode0RawAttentionHistory RunRawHistoryCore(long budget,
        Action<Math2Mode0RecordingObservationStage, Math2Mode0PrefixStep, Math2Mode0PrefixCandidates,
            Math2Mode0PrefixContext, Math2Mode0CompletedRecords, Math2Mode0RawAttentionHistory>? observer,
        CancellationToken token)
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this); token.ThrowIfCancellationRequested();
            if (budget < 0 || budget > Math2Mode0RawAttentionHistory.PortableDefaultPayloadBudget)
                throw new ArgumentOutOfRangeException(nameof(budget), "Prepared lower payload budget must be within 0..16MiB.");
            if (claimed || rawAttentionHistory != null)
                throw new InvalidOperationException("Bind history before the same loop's single recording run.");
            var history = new Math2Mode0RawAttentionHistory(this, prefix, records, budget);
            rawAttentionHistory = history;
            try
            {
                RunCore((stage, step, candidates, source, vector, fallback) =>
                {
                    if (stage == Math2Mode0RecordingObservationStage.BeforeRecording)
                        history.AppendActualFrame(step, token);
                    observer?.Invoke(stage, step, candidates, source, vector, history);
                }, token);
                history.CompleteRecording();
                return history;
            }
            catch
            {
                if (!claimed) { history.Release(); rawAttentionHistory = null; }
                else { history.MarkFailed(); FailRawAttentionHistory(); }
                throw;
            }
        }
    }
    internal T WithRawAttentionHistory<T>(Math2Mode0RawAttentionHistory history, bool requireComplete, Func<T> action)
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (!ReferenceEquals(rawAttentionHistory, history)) throw new ArgumentException("Foreign history owner.");
            if (failed)
            { history.MarkFailed(); throw new InvalidOperationException("Failed recording/history owner cannot publish a snapshot."); }
            if (requireComplete && !finished) throw new InvalidOperationException("Recording must finish before reconstruction.");
            return action();
        }
    }
    internal void FailRawAttentionHistory()
    {
        failed = true;
        prefix.WithCompletedStorage(() => { prefix.FailCompletedRecording(); return true; });
    }
    private void DisposeRawAttentionHistory() => rawAttentionHistory?.Release();
    internal Math2Mode0AttentionHistoryStatistics CopyRawAttentionStatistics()
    {
        lock (gate)
        {
            var history = rawAttentionHistory ?? throw new InvalidOperationException("This loop has no opt-in history.");
            if (failed) history.MarkFailed();
            return history.CopyStatistics();
        }
    }
}

/// <summary>Opt-in SAME-loop immutable raw[S,B,1] history. The 16MiB default
/// bounds retained F32 payload, not RSS or a native configuration property.</summary>
internal sealed class Math2Mode0RawAttentionHistory
{
    internal const long PortableDefaultPayloadBudget = 16L * 1024 * 1024;
    private sealed record Frame(int Segments, int Beams, float[] Values);
    private readonly Math2Mode0ColdRecordingLoop owner;
    private readonly Math2Mode0PrefixContext prefix;
    private readonly Math2Mode0CompletedRecords vector;
    private readonly long budget;
    private readonly List<Frame> frames = new();
    private readonly Dictionary<Math2Mode0CompletedEventLease, Math2Mode0HistoricalCompletedAttention> snapshots = new();
    private Math2Mode0PrefixStep? captureStep;
    private LegacyMath2ObservableTokens? gate;
    private long reserved, rawBytes, snapshotBytes;
    private int allocations, segments;
    private bool complete, failed, disposed;
    internal Math2Mode0RawAttentionHistory(Math2Mode0ColdRecordingLoop owner,
        Math2Mode0PrefixContext prefix, Math2Mode0CompletedRecords vector, long budget)
    { this.owner = owner; this.prefix = prefix; this.vector = vector; this.budget = budget; }
    internal void AppendActualFrame(Math2Mode0PrefixStep step, CancellationToken token)
        => owner.WithRawAttentionHistory(this, false, () => step.WithActive(() =>
        {
            if (complete || disposed || failed || captureStep != null ||
                !ReferenceEquals(step.PrefixOwner, prefix) || step.Generation.Offset != frames.Count)
                throw new InvalidOperationException("Only the next actual same-loop raw frame can be retained.");
            prefix.RequireActualAttentionHistory(step);
            captureStep = step;
            try
            {
                var captured = step.TensorStep.Owner.CopyRawAttentionForHistory(step, this, token);
                frames.Add(new(captured.Segments, captured.Beams, captured.Values));
                rawBytes += checked(captured.Values.LongLength * 4);
                return true;
            }
            finally { captureStep = null; }
        }));
    // Called only while this exact Step's tensor lifetime is locked. Shape and
    // complete budget checks happen BEFORE the unsafe reader allocates payload.
    internal void ReserveActualRawFrame(Math2Mode0PrefixStep step, LegacyMath2ObservableTokens actualGate,
        int sourceSegments, int beams, long bytes, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (!ReferenceEquals(step, captureStep) || !ReferenceEquals(step.PrefixOwner, prefix) ||
            sourceSegments < 1 || sourceSegments > 6000 || beams != step.Generation.ActiveCount ||
            bytes != checked((long)sourceSegments * beams * 4) ||
            segments != 0 && segments != sourceSegments || gate != null && !ReferenceEquals(gate, actualGate))
            throw new ArgumentException("Foreign or inconsistent actual raw attention metadata.");
        if (gate == null) gate = actualGate;
        if (segments == 0) segments = sourceSegments;
        ReservePayload(bytes, token);
    }
    private void ReservePayload(long bytes, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (bytes < 0 || bytes > budget - reserved)
            throw new NotSupportedException("The explicit total retained attention payload budget is exhausted.");
        reserved += bytes; allocations++;
    }
    internal void CompleteRecording()
        => owner.WithRawAttentionHistory(this, true, () =>
        {
            if (failed || disposed || complete || captureStep != null ||
                frames.Count != checked((int)prefix.ColdMaximumPosition) || gate == null)
                throw new InvalidOperationException("Complete successful raw history is required.");
            complete = true; return true;
        });
    private void CheckComplete()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (failed || !complete) throw new InvalidOperationException("Only complete successful raw history can reconstruct.");
    }
    internal Math2Mode0HistoricalCompletedAttention CaptureCompleted(Math2Mode0CompletedEventLease source,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        return owner.WithRawAttentionHistory(this, true, () =>
        {
            CheckComplete();
            // Managed foreign-source guards precede consumption. They cannot
            // change a valid history into a source belonging to another loop.
            var proof = source.ReadHistoricalSource(prefix, vector);
            if (!vector.ContainsRecordedSource(source)) throw new ArgumentException("The actual recorded source is required.");
            try
            {
                token.ThrowIfCancellationRequested();
                if (snapshots.TryGetValue(source, out var saved)) return saved;
                int symbols = proof.Generation.CompletedParentLength(proof.Parent);
                if (symbols < 1 || symbols > 1000 || symbols != source.Offset || symbols >= frames.Count)
                    throw new NotSupportedException("Nonempty actual old-prefix history before its EOS/fallback frame required.");
                long bytes = checked((long)symbols * segments * 4);
                ReservePayload(bytes, token);
                float[] values = new float[checked(symbols * segments)];
                var beams = frames.Select(frame => frame.Beams).ToArray();
                LegacyPreparedMathAttentionHistory.WritePrepared(values, symbols, segments, proof.Parent,
                    time => proof.Generation.ReadHistoricalPrimary(proof.Parent, time),
                    proof.Generation.ReadHistoricalAncestor, beams,
                    (time, ancestor, segment) => frames[time].Values[segment * frames[time].Beams + ancestor],
                    gate!, token);
                var result = new Math2Mode0HistoricalCompletedAttention(this, source, symbols, segments, values);
                snapshots.Add(source, result); snapshotBytes += bytes;
                return result;
            }
            catch { MarkFailed(); owner.FailRawAttentionHistory(); throw; }
        });
    }
    internal T WithSnapshot<T>(Math2Mode0HistoricalCompletedAttention source,
        CancellationToken token, Func<T> action)
        => owner.WithRawAttentionHistory(this, true, () =>
        {
            CheckComplete();
            if (!snapshots.TryGetValue(source.Source, out var actual) || !ReferenceEquals(source, actual) ||
                !vector.ContainsRecordedSource(source.Source)) throw new ArgumentException("Foreign historical snapshot.");
            try { token.ThrowIfCancellationRequested(); return action(); }
            catch { MarkFailed(); owner.FailRawAttentionHistory(); throw; }
        });
    internal Math2Mode0RawAttentionFrameCopy[] CopyFramesForInspection()
        => owner.WithRawAttentionHistory(this, true, () =>
        {
            CheckComplete();
            try
            { return frames.Select(frame => new Math2Mode0RawAttentionFrameCopy(frame.Segments, frame.Beams, frame.Values.ToArray())).ToArray(); }
            catch { MarkFailed(); owner.FailRawAttentionHistory(); throw; }
        });
    internal void MarkFailed() => failed = true;
    internal void Release()
    {
        disposed = true;
        foreach (var snapshot in snapshots.Values) snapshot.ReleasePayload();
        frames.Clear(); snapshots.Clear(); rawBytes = snapshotBytes = 0;
    }
    internal Math2Mode0AttentionHistoryStatistics CopyStatistics()
        => new(budget, reserved, rawBytes, snapshotBytes, frames.Count, snapshots.Count, allocations,
            complete, failed, disposed, frames.Select(frame => Convert.ToHexStringLower(
                SHA256.HashData(MemoryMarshal.AsBytes(frame.Values.AsSpan())))).ToArray());
}

/// <summary>Actual post-recording history snapshot. Reads retain the SAME
/// successful history lifetime; no active/stale-Step substitution is involved.</summary>
internal sealed class Math2Mode0HistoricalCompletedAttention
{
    private readonly Math2Mode0RawAttentionHistory owner;
    private float[] values;
    internal Math2Mode0CompletedEventLease Source { get; }
    internal int Symbols { get; }
    internal int Segments { get; }
    internal Math2Mode0HistoricalCompletedAttention(Math2Mode0RawAttentionHistory owner,
        Math2Mode0CompletedEventLease source, int symbols, int segments, float[] values)
    { this.owner = owner; Source = source; Symbols = symbols; Segments = segments; this.values = values; }
    internal float[] CopyAttention(CancellationToken token = default)
        => owner.WithSnapshot(this, token, () => values.ToArray());
    internal void ReleasePayload() => values = Array.Empty<float>();
}

internal sealed partial class Math2Mode0CompletedEventLease
{
    internal (Math2Mode0PrefixGeneration Generation, int Parent) ReadHistoricalSource(
        Math2Mode0PrefixContext actualPrefix, Math2Mode0CompletedRecords actualVector)
    {
        if (!ReferenceEquals(step.PrefixOwner, actualPrefix) || !ReferenceEquals(recorder, actualVector) ||
            !ReferenceEquals(step.Generation, generation) || !consumed)
            throw new ArgumentException("The consumed actual same-loop source generation is required.");
        generation.CompletedParentLength(sourceParent);
        return (generation, sourceParent);
    }
}
internal sealed partial class Math2Mode0CompletedRecords
{
    internal bool ContainsRecordedSource(Math2Mode0CompletedEventLease source)
        => owner.WithCompletedStorage(() => records.Any(record => ReferenceEquals(record.Source, source)));
}
internal sealed partial class Math2Mode0PrefixGeneration
{
    internal ushort ReadHistoricalPrimary(int parent, int time)
    { CompletedParentLength(parent); return primary[parent][time]; }
    internal int ReadHistoricalAncestor(int rank, int time)
    {
        if ((uint)rank >= (uint)ancestry.Length || (uint)time >= (uint)ancestry[rank].Length)
            throw new ArgumentException("Cold rank/time ancestry address is outside the immutable generation.");
        return ancestry[rank][time];
    }
}
