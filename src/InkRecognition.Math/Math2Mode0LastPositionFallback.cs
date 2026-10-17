using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathRecordingLoop.Probe")]

namespace InkRecognition.Math;

internal enum Math2Mode0FallbackOutcome { NotLastPosition, CompletedVectorNotEmpty, LastPositionEmpty }

internal sealed partial class Math2Mode0PrefixCandidates
{
    private Math2Mode0LastPositionFallback? fallback;
    internal bool HasCapturedCompletedEvents => completedLeases.Count != 0;
    internal T WithRecordingPhase<T>(Func<Math2Mode0PrefixStep, int, T> action)
        => step.WithActive(() => action(step, handoff.Scores.Length));
    internal void ValidateFallbackOwner(Math2Mode0LastPositionFallback source)
    {
        if (!ReferenceEquals(fallback, source))
            throw new ArgumentException("The exact candidate-owned fallback phase is required.");
    }
    internal void ValidateFallbackConsumption() => fallback?.ValidateSelection();
    internal Math2Mode0LastPositionFallback PrepareFallback(Math2Mode0CompletedRecords recorder,
        Math2Mode0RecordedStepPhase phase, CancellationToken token)
        => phase.WithActual(recorder, this, () => step.WithActive(() =>
        {
            token.ThrowIfCancellationRequested(); recorder.CheckOwner(step.PrefixOwner);
            if (fallback != null) throw new InvalidOperationException("This candidate phase already attempted fallback.");
            ulong position = checked((ulong)step.Generation.Offset + 1UL);
            ulong maximum = step.PrefixOwner.ColdMaximumPosition;
            if (position > maximum)
                throw new NotSupportedException("The diagnostic Step is past the actual cold outer bound.");
            Math2Mode0FallbackOutcome outcome;
            int[] ranks;
            if (position != maximum)
            { outcome = Math2Mode0FallbackOutcome.NotLastPosition; ranks = []; }
            else if (recorder.Count != 0)
            { outcome = Math2Mode0FallbackOutcome.CompletedVectorNotEmpty; ranks = []; }
            else
            {
                if (handoff.Scores.Length != 0)
                    throw new InvalidOperationException("Ordinary EOS events must be recorded before last-position fallback.");
                outcome = Math2Mode0FallbackOutcome.LastPositionEmpty;
                ranks = LegacyPreparedMathLastPositionFallback.EligibleRanks(handoff.CopyAfterResort(),
                    step.PreparedWidth, step.Generation.CopyPrimary().Take(step.Generation.ActiveCount).ToArray(),
                    step.TensorStep.Owner.ObservableGate, token);
                step.BindCompletedCandidates(this);
            }
            token.ThrowIfCancellationRequested();
            fallback = new(recorder, step, this, handoff.CopyAfterResort(), outcome, ranks, position, maximum);
            return fallback;
        }), token);
}

/// <summary>Exact ordered ordinary-EOS phase completion. Recorder identity
/// and progression, not a caller empty/last-position flag, authorize fallback.</summary>
internal sealed class Math2Mode0RecordedStepPhase
{
    private readonly Math2Mode0CompletedRecords recorder;
    private readonly Math2Mode0PrefixCandidates candidates;
    private readonly Math2Mode0PrefixStep step;
    internal Math2Mode0RecordedStepPhase(Math2Mode0CompletedRecords recorder,
        Math2Mode0PrefixCandidates candidates, Math2Mode0PrefixStep step)
    { this.recorder = recorder; this.candidates = candidates; this.step = step; }
    internal T WithActual<T>(Math2Mode0CompletedRecords target, Math2Mode0PrefixCandidates source,
        Func<T> action, CancellationToken token)
        => candidates.WithRecordingPhase((actualStep, _) =>
        {
            token.ThrowIfCancellationRequested();
            recorder.CheckOwner(actualStep.PrefixOwner);
            if (!ReferenceEquals(target, recorder) || !ReferenceEquals(source, candidates) ||
                !ReferenceEquals(step, actualStep) || !recorder.OwnsRecordedPhase(this))
                throw new ArgumentException("Foreign, stale or unissued ordinary recording phase.");
            return action();
        });
}

/// <summary>Actual post-EOS candidate pool plus native last-position/empty
/// entry proof. It cannot be constructed from prepared eligibility results.</summary>
internal sealed class Math2Mode0LastPositionFallback
{
    private readonly Math2Mode0CompletedRecords recorder;
    private readonly Math2Mode0PrefixStep step;
    private readonly Math2Mode0PrefixCandidates candidates;
    private readonly byte[] records;
    private readonly int[] ranks;
    private Math2Mode0LastPositionFallbackLease? pending;
    private int cursor;
    internal Math2Mode0FallbackOutcome Outcome { get; }
    internal ulong Position { get; }
    internal ulong Maximum { get; }
    internal int[] CopyEligibleRanks() => ranks.ToArray();
    internal byte[] CopySortedPool() => records.ToArray();
    internal Math2Mode0LastPositionFallback(Math2Mode0CompletedRecords recorder, Math2Mode0PrefixStep step,
        Math2Mode0PrefixCandidates candidates, byte[] records, Math2Mode0FallbackOutcome outcome,
        int[] ranks, ulong position, ulong maximum)
    {
        this.recorder = recorder; this.step = step; this.candidates = candidates;
        this.records = records; this.ranks = ranks; Outcome = outcome; Position = position; Maximum = maximum;
    }
    internal sealed record Data(Math2Mode0CompletedRecords Recorder, Math2Mode0PrefixStep Step,
        Math2Mode0PrefixCandidates Candidates, int Rank, byte[] Row, ushort[] Primary,
        ushort[] Secondary, int[][] Ancestry, Math2Mode0CommonSecondaryFrontier Frontier);
    internal Math2Mode0LastPositionFallbackLease? CaptureNext(CancellationToken token = default)
        => Math2Mode0CompletedEventLease.CaptureFallback(this, token);
    internal Math2Mode0LastPositionFallbackLease? CaptureCanonical(
        Func<Data, Math2Mode0LastPositionFallbackLease> create, CancellationToken token)
        => step.WithActive(() =>
        {
            ArgumentNullException.ThrowIfNull(create);
            token.ThrowIfCancellationRequested(); recorder.CheckOwner(step.PrefixOwner);
            candidates.ValidateFallbackOwner(this);
            if (pending != null && !pending.IsConsumed)
                throw new InvalidOperationException("Record the preceding fallback row before advancing.");
            if (cursor == ranks.Length) return null;
            if (Outcome != Math2Mode0FallbackOutcome.LastPositionEmpty || Position != Maximum ||
                recorder.Count >= step.PreparedWidth)
                throw new InvalidOperationException("The actual last-position fallback frontier no longer applies.");
            int rank = ranks[cursor];
            var row = records.AsSpan(rank * 48, 48).ToArray();
            int parent = BinaryPrimitives.ReadInt32LittleEndian(row.AsSpan(40));
            var data = new Data(recorder, step, candidates, rank, row,
                step.Generation.CopyPrimary()[parent], step.Generation.CopySecondary()[parent],
                step.Generation.CopyAncestry(), step.ReadCompletedFrontier(token));
            var lease = create(data);
            lease.ValidateCapture(this, data);
            token.ThrowIfCancellationRequested();
            pending = lease; cursor++;
            return lease;
        });
    internal void ValidateSelection()
    {
        if (cursor != ranks.Length || pending is { IsConsumed: false })
            throw new InvalidOperationException("Complete this actual fallback scan before applying selection.");
    }
    internal void ValidateConsumer(Math2Mode0LastPositionFallbackLease source,
        Math2Mode0CompletedRecords target)
    {
        candidates.ValidateFallbackOwner(this);
        if (!ReferenceEquals(source, pending) || !ReferenceEquals(recorder, target) ||
            Outcome != Math2Mode0FallbackOutcome.LastPositionEmpty || recorder.Count >= step.PreparedWidth)
            throw new ArgumentException("The exact pending fallback row and vector owner are required.");
    }
}

/// <summary>Distinct source type; old ordinary-EOS append/getter entries refuse
/// its private common content lease. The current row label is not appended.</summary>
internal sealed class Math2Mode0LastPositionFallbackLease
{
    private readonly Math2Mode0LastPositionFallback batch;
    internal Math2Mode0CompletedEventLease Content { get; }
    internal bool IsConsumed => Content.IsConsumed;
    internal int Rank => Content.Rank;
    internal long Offset => Content.Offset;
    internal long Frontier => Content.Frontier.Value;
    internal Math2Mode0LastPositionFallbackLease(Math2Mode0LastPositionFallback batch,
        Math2Mode0CompletedEventLease content) { this.batch = batch; Content = content; }
    internal byte[] CopyRawRecord() => Content.CopyRawRecord();
    internal ushort[] CopyPrimary() => Content.CopyPrimary();
    internal ushort[] CopySecondary() => Content.CopySecondary();
    internal int[][] CopyAncestry() => Content.CopyAncestry();
    internal void ValidateConsumer(Math2Mode0CompletedRecords target) => batch.ValidateConsumer(this, target);
    internal void ValidateCapture(Math2Mode0LastPositionFallback source, Math2Mode0LastPositionFallback.Data data)
    {
        if (!ReferenceEquals(source, batch) || !Content.IsExactFallbackSource(this))
            throw new ArgumentException("The exact newly issued typed fallback source is required.");
        Content.ValidateCapture(data.Recorder, data.Step, data.Candidates, data.Rank);
    }
}

internal sealed partial class Math2Mode0CompletedEventLease
{
    private Math2Mode0LastPositionFallbackLease? fallbackSource;
    internal bool IsFallback => fallbackSource != null;
    internal static Math2Mode0LastPositionFallbackLease? CaptureFallback(Math2Mode0LastPositionFallback batch,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(batch);
        return batch.CaptureCanonical(data =>
        {
            var content = new Math2Mode0CompletedEventLease(data.Recorder, data.Step, data.Candidates,
                data.Rank, data.Row, data.Primary, data.Secondary, data.Ancestry, data.Frontier);
            var source = new Math2Mode0LastPositionFallbackLease(batch, content);
            content.fallbackSource = source;
            return source;
        }, token);
    }
    internal bool IsExactFallbackSource(Math2Mode0LastPositionFallbackLease source)
        => ReferenceEquals(source, fallbackSource);
    internal void ValidateFallbackConsumer(Math2Mode0CompletedRecords target)
        => fallbackSource?.ValidateConsumer(target);
    internal Math2Mode0LastPositionFallbackLease ReadFallbackSource()
        => fallbackSource ?? throw new InvalidOperationException("This record has an ordinary EOS source.");
}

internal sealed partial class Math2Mode0CompletedRecords
{
    private long nextOrdinaryPhaseOffset;
    private Math2Mode0RecordedStepPhase? ordinaryPhase;
    internal bool OwnsRecordedPhase(Math2Mode0RecordedStepPhase phase) => ReferenceEquals(ordinaryPhase, phase);
    internal Math2Mode0RecordedStepPhase RecordOrdinaryEosPhase(Math2Mode0PrefixCandidates candidates,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        return candidates.WithRecordingPhase((step, count) =>
        {
            token.ThrowIfCancellationRequested(); CheckOwner(step.PrefixOwner);
            if (step.Generation.Offset != nextOrdinaryPhaseOffset ||
                checked((ulong)step.Generation.Offset) >= owner.ColdMaximumPosition ||
                candidates.HasCapturedCompletedEvents)
                throw new InvalidOperationException("The exact cold ordered EOS history from offset0 is required.");
            try
            {
                step.BindCompletedCandidates(candidates);
                for (int ordinal = 0; ordinal < count; ordinal++)
                    Append(Capture(candidates, ordinal, token), token);
                token.ThrowIfCancellationRequested();
                ordinaryPhase = new(this, candidates, step);
                nextOrdinaryPhaseOffset++;
                return ordinaryPhase;
            }
            catch
            {
                failed = true; owner.FailCompletedRecording(); throw;
            }
        });
    }
    internal Math2Mode0LastPositionFallback PrepareLastPositionFallback(Math2Mode0PrefixCandidates candidates,
        Math2Mode0RecordedStepPhase phase, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(candidates); ArgumentNullException.ThrowIfNull(phase);
        return candidates.PrepareFallback(this, phase, token);
    }
    internal void AppendFallback(Math2Mode0LastPositionFallbackLease lease, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(lease);
        AppendCore(lease.Content, null, token, allowFallback: true);
    }
    internal Math2Mode0LastPositionFallbackLease ReadFallbackSourceEvent(int ordinal)
        => owner.WithCompletedStorage(() => records[ordinal].Source.ReadFallbackSource());
}
