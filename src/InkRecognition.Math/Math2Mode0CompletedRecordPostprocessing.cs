namespace InkRecognition.Math;

internal sealed partial class Math2Mode0TensorState
{
    // Immutable encoder-derived extent, not a live output read or new inference.
    internal int RawOnlySegmentCount => checked((int)segments);
}
internal sealed partial class Math2Mode0CompletedEventLease
{
    internal int RawOnlySegmentCount => step.TensorStep.Owner.RawOnlySegmentCount;
}

internal sealed partial class Math2Mode0ColdRecordingLoop
{
    private bool rawOnlyPostprocessClaimed;
    private Math2Mode0RawOnlyEf4? rawOnlyPostprocessing;
    internal bool OwnsRawOnlyVector(Math2Mode0CompletedRecords vector)
    { lock (gate) return ReferenceEquals(records, vector) && prefix.OwnsCompletedRecords(vector); }
    internal Math2Mode0CompletedRecords.RawOnlyRecord[] PrepareRawOnlyRecords(
        Math2Mode0RawOnlySource source, Math2Mode0InitiallyOrderedCompleted ordered,
        Math2Mode0RawAttentionHistory history, CancellationToken token)
    {
        lock (gate)
        {
            token.ThrowIfCancellationRequested();
            if (!ReferenceEquals(source.Loop, this)) throw new ArgumentException("Foreign raw source loop.");
            source.ValidatePreparedOwners(ordered, history);
            return WithInitialOrdering(ordered, () => records.CreateRawOnlyViews(source, token));
        }
    }
    internal void ClaimRawOnlyPostprocessing(Math2Mode0RawOnlyEf4 stage,
        Math2Mode0InitiallyOrderedCompleted ordered, Math2Mode0RawAttentionHistory history,
        CancellationToken token)
    {
        lock (gate)
        {
            token.ThrowIfCancellationRequested();
            stage.Source.ValidateProcessing(stage);
            if (!ReferenceEquals(stage.Source.Loop, this)) throw new ArgumentException("Foreign processing loop.");
            stage.Source.ValidatePreparedOwners(ordered, history);
            WithInitialOrdering(ordered, () => true);
            WithRawAttentionHistory(history, true, () => true);
            rawOnlyPostprocessClaimed = true;
            rawOnlyPostprocessing = stage;
        }
    }
    internal T WithRawOnlyPostprocessing<T>(Math2Mode0RawOnlyEf4 stage, Func<T> action)
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (failed || !ReferenceEquals(rawOnlyPostprocessing, stage))
                throw new InvalidOperationException("Same successful raw-only postprocessing lifetime required.");
            stage.Source.ValidateProcessing(stage);
            return action();
        }
    }
}

internal sealed partial class Math2Mode0CompletedRecords
{
    internal sealed class RawOnlyRecord
    {
        private readonly Math2Mode0CompletedRecords owner;
        private readonly Record record;
        internal Math2Mode0RawOnlySource RawSource { get; }
        internal object RecordIdentity => record;
        internal Math2Mode0CompletedEventLease Source => record.Source;
        internal Math2Mode0EmptyLibrary Library => record.Library;
        internal int PrimaryCount => record.Primary.Count;
        internal float Score => record.Score;
        internal ushort PrimaryAt(int index) => record.Primary[index];
        internal RawOnlyRecord(Math2Mode0CompletedRecords owner, Math2Mode0RawOnlySource source, object actual)
        {
            this.owner = owner; RawSource = source;
            record = actual as Record ?? throw new ArgumentException("Actual recording storage required.");
            Validate();
        }
        internal void Validate()
        {
            if (!RawSource.Loop.OwnsRawOnlyVector(owner) || owner.disposed || owner.failed || !owner.initialOrderClaimed ||
                !owner.records.Contains(record) || record.Library.IsDisposed || !record.Source.IsConsumed)
                throw new InvalidOperationException("Actual counted Source/Record/Library ownership required.");
        }
        internal void StoreScore(Math2Mode0RawOnlyEf4 stage, float score)
        {
            Validate();
            if (!ReferenceEquals(stage.Source, RawSource)) throw new ArgumentException("Foreign Source/Record stage.");
            if (!float.IsFinite(score)) throw new NotSupportedException("Finite postprocessing score required.");
            RawSource.Loop.WithRawOnlyPostprocessing(stage, () => { record.Score = score; return true; });
        }
    }
    internal RawOnlyRecord[] CreateRawOnlyViews(Math2Mode0RawOnlySource source, CancellationToken token)
        => owner.WithCompletedStorage(() =>
        {
            ObjectDisposedException.ThrowIf(disposed, this); token.ThrowIfCancellationRequested();
            if (failed || !initialOrderClaimed || !owner.OwnsCompletedRecords(this))
                throw new InvalidOperationException("Successful actual initially ordered vector required.");
            if (records.Select(record => record.Source).Distinct().Count() != records.Count)
                throw new InvalidDataException("Each counted record must retain its distinct actual Source.");
            return records.Select(record => new RawOnlyRecord(this, source, record)).ToArray();
        });
}
