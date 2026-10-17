using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathInitialCompletedOrdering.Probe")]

namespace InkRecognition.Math;

internal enum Math2Mode0InitialOrderingStage
{
    BeforeNumericOrdering, AfterNumericOrdering, BeforeOwnerMove, AfterOwnerMove, BeforeCompletion
}

internal sealed partial class Math2Mode0ColdRecordingLoop
{
    private bool initialOrderingClaimed;
    private Math2Mode0InitiallyOrderedCompleted? initialOrdering;

    internal Math2Mode0InitiallyOrderedCompleted OrderInitialCompleted(CancellationToken token = default)
        => OrderInitialCore(null, token);

    // Logical managed failure observations, not native sorter callbacks.
    internal Math2Mode0InitiallyOrderedCompleted OrderInitialDiagnostic(
        Action<Math2Mode0InitialOrderingStage, int> observer, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(observer);
        return OrderInitialCore(observer, token);
    }

    private Math2Mode0InitiallyOrderedCompleted OrderInitialCore(
        Action<Math2Mode0InitialOrderingStage, int>? observer, CancellationToken token)
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            token.ThrowIfCancellationRequested();
            if (!finished || failed)
                throw new InvalidOperationException("A successful complete recording stage is required.");
            if (initialOrderingClaimed)
                throw new InvalidOperationException("Initial completed ordering is single-use, including after failure.");
            if (records.Count > 65536)
                throw new NotSupportedException("The existing portable ordering bound is65536 records.");
            initialOrderingClaimed = true;
            try
            {
                var permutation = records.MoveInitiallyOrderedOwners(observer, token);
                token.ThrowIfCancellationRequested();
                initialOrdering = new(this, records, permutation);
                return initialOrdering;
            }
            catch
            {
                failed = true;
                prefix.WithCompletedStorage(() => { prefix.FailCompletedRecording(); return true; });
                throw;
            }
        }
    }

    internal T WithInitialOrdering<T>(Math2Mode0InitiallyOrderedCompleted source, Func<T> action)
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (failed || rawOnlyPostprocessClaimed || !ReferenceEquals(initialOrdering, source))
                throw new ArgumentException("The exact successful initial-order result is required.");
            return action();
        }
    }
}

/// <summary>Same actual vector after039AB3 initial sort, BEFORE optional work,
/// required036F20/EF4, final sort or truncation. No final formula owner.</summary>
internal sealed class Math2Mode0InitiallyOrderedCompleted
{
    private readonly Math2Mode0ColdRecordingLoop owner;
    private readonly Math2Mode0CompletedRecords vector;
    private readonly int[] originalOrdinals;
    internal bool IsPostprocessed => false;
    internal bool IsFinalOrder => false;
    internal bool HasTruncatedAlternatives => false;
    internal object VectorIdentity => vector;
    internal Math2Mode0InitiallyOrderedCompleted(Math2Mode0ColdRecordingLoop owner,
        Math2Mode0CompletedRecords vector, int[] originalOrdinals)
    { this.owner = owner; this.vector = vector; this.originalOrdinals = originalOrdinals; }
    internal int[] CopyOriginalOrdinals()
        => owner.WithInitialOrdering(this, () => originalOrdinals.ToArray());
    internal Math2Mode0CompletedRecordSnapshot[] CopyRecords()
        => owner.WithInitialOrdering(this, vector.CopyRecords);
    internal Math2Mode0CompletedEventLease ReadOwnedSource(int ordinal)
        => owner.WithInitialOrdering(this, () => vector.ReadInitiallyOrderedSource(ordinal));
}

internal sealed partial class Math2Mode0CompletedRecords
{
    private bool initialOrderClaimed;

    internal Math2Mode0CompletedEventLease ReadInitiallyOrderedSource(int ordinal)
        => owner.WithCompletedStorage(() => records[ordinal].Source);

    internal int[] MoveInitiallyOrderedOwners(Action<Math2Mode0InitialOrderingStage, int>? observer,
        CancellationToken token)
        => owner.WithCompletedStorage(() =>
        {
            token.ThrowIfCancellationRequested(); CheckOwner(owner);
            if (initialOrderClaimed) throw new InvalidOperationException("This actual vector was already ordered.");
            initialOrderClaimed = true;
            // Retain all original actual owners throughout numeric work and
            // every move. The projection is NOT a native pointer/padding image.
            Record[] original = records.ToArray();
            void Observe(Math2Mode0InitialOrderingStage stage, int index = -1)
            { observer?.Invoke(stage, index); token.ThrowIfCancellationRequested(); }
            try
            {
                byte[] projection = new byte[checked(original.Length * LegacyPreparedMathCandidateOrdering.RecordSize)];
                for (int index = 0; index < original.Length; index++)
                {
                    token.ThrowIfCancellationRequested();
                    var row = projection.AsSpan(index * 168, 168);
                    BinaryPrimitives.WriteSingleLittleEndian(row, original[index].Score);
                    BinaryPrimitives.WriteInt32LittleEndian(row[8..], index);
                }
                Observe(Math2Mode0InitialOrderingStage.BeforeNumericOrdering);
                // Native039A96 skips036810 for count0/1. Preserve that guard.
                byte[] sorted = original.Length > 1
                    ? LegacyPreparedMathCandidateOrdering.Sort(projection, token) : projection;
                int[] permutation = new int[original.Length];
                var seen = new bool[original.Length];
                for (int index = 0; index < original.Length; index++)
                {
                    token.ThrowIfCancellationRequested();
                    var row = sorted.AsSpan(index * 168, 168);
                    int prior = BinaryPrimitives.ReadInt32LittleEndian(row[8..]);
                    if (prior < 0 || prior >= original.Length || seen[prior] ||
                        !row.SequenceEqual(projection.AsSpan(prior * 168, 168)))
                        throw new InvalidDataException("The numeric sorter must return an exact identity permutation.");
                    seen[prior] = true; permutation[index] = prior;
                }
                Observe(Math2Mode0InitialOrderingStage.AfterNumericOrdering);
                // Apply the validated permutation to ACTUAL Record references.
                // Atomic swaps keep the owner set complete even if a managed
                // cancellation/failure is injected between moves. This commit
                // sequence is not a native168 write/failure-order claim.
                int[] atPosition = Enumerable.Range(0, original.Length).ToArray();
                int[] positionOf = atPosition.ToArray();
                for (int index = 0; index < original.Length; index++)
                {
                    int target = permutation[index], from = positionOf[target];
                    if (from == index) continue;
                    Observe(Math2Mode0InitialOrderingStage.BeforeOwnerMove, index);
                    int displaced = atPosition[index];
                    (records[index], records[from]) = (records[from], records[index]);
                    (atPosition[index], atPosition[from]) = (atPosition[from], atPosition[index]);
                    positionOf[target] = index; positionOf[displaced] = from;
                    Observe(Math2Mode0InitialOrderingStage.AfterOwnerMove, index);
                }
                Observe(Math2Mode0InitialOrderingStage.BeforeCompletion);
                return permutation;
            }
            catch
            {
                failed = true; owner.FailCompletedRecording(); throw;
            }
            finally { GC.KeepAlive(original); }
        });
}
