using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathFinalCompletedOrdering.Probe")]

namespace InkRecognition.Math;

internal enum Math2Mode0FinalOrderingStage
{
    BeforeNumericOrdering, AfterNumericOrdering, BeforeOwnerMove, AfterOwnerMove,
    BeforeRelease, AfterRelease, BeforeCountCommit, AfterCountCommit, BeforeCompletion
}

internal sealed record Math2Mode0FinalOrderingStatistics(int OriginalCount, int CurrentCount,
    int Width, long PayloadReservation, int[] ReleasedOriginalOrdinals, bool Complete, bool Failed);

/// <summary>Same actual records after the success-only039BF2 final sort/tail
/// release. Formula/output transport and failure-exit native tail remain separate.</summary>
internal sealed class Math2Mode0RawOnlyFinalOrdering
{
    internal const long DefaultPayloadBudget = 16L * 1024 * 1024;
    internal Math2Mode0RawOnlyEf4 Ef4 { get; }
    private readonly int originalCount, width;
    private readonly long reservation;
    private readonly List<int> releasedOrdinals = new();
    private int currentCount;
    private int[] permutation = [];
    private object[] retained = [];
    private bool complete, failed;
    internal bool IsAcceptedFormula => false;

    internal Math2Mode0RawOnlyFinalOrdering(Math2Mode0RawOnlyEf4 ef4, int count, int width, long reservation)
    { Ef4 = ef4; originalCount = currentCount = count; this.width = width; this.reservation = reservation; }

    internal void Run(Action<Math2Mode0FinalOrderingStage, int, Math2Mode0RawOnlyFinalOrdering>? observer,
        CancellationToken token)
    {
        void Observe(Math2Mode0FinalOrderingStage stage, int index)
        {
            observer?.Invoke(stage, index, this);
            Ef4.ValidateFinalOrdering(this);
            token.ThrowIfCancellationRequested();
        }
        try
        {
            Ef4.PrepareFinalLookup(this);
            Ef4.FinalizeRecordOwners(this, width, Observe,
                ordinal => releasedOrdinals.Add(ordinal),
                count => currentCount = count,
                (order, owners) => { permutation = order; retained = owners; }, token);
            Observe(Math2Mode0FinalOrderingStage.BeforeCompletion, currentCount);
            complete = true;
        }
        catch { failed = true; Ef4.Source.Loop.FailRawAttentionHistory(); throw; }
    }

    internal Math2Mode0RawOnlyEf4RecordCopy[] CopyRecords()
        => Ef4.WithFinalOrdering(this, () =>
        {
            if (!complete || failed) throw new InvalidOperationException("Successful final ordering required.");
            return retained.Select(record => Ef4.CopyFinalRecord(this, record)).ToArray();
        });
    internal int[] CopySortPermutation()
        => Ef4.WithFinalOrdering(this, () =>
        {
            if (!complete || failed) throw new InvalidOperationException("Successful final ordering required.");
            return permutation.ToArray();
        });
    // Failure inspection returns detached logical counters, never accepted rows.
    internal Math2Mode0FinalOrderingStatistics CopyStatistics()
        => new(originalCount, currentCount, width, reservation, releasedOrdinals.ToArray(), complete, failed);
}

internal sealed partial class Math2Mode0RawOnlyEf4
{
    private bool finalOrderingClaimed;
    private Math2Mode0RawOnlyFinalOrdering? finalOrdering;
    private Dictionary<object, Cell>? finalCells;

    internal Math2Mode0RawOnlyFinalOrdering OrderFinal(long budget,
        Action<Math2Mode0FinalOrderingStage, int, Math2Mode0RawOnlyFinalOrdering>? observer,
        CancellationToken token)
        => Source.WithOwner(() => Source.Loop.WithRawOnlyPostprocessing(this, () =>
        {
            token.ThrowIfCancellationRequested();
            if (!complete || failed || released || finalOrderingClaimed)
                throw new InvalidOperationException("The exact successful unconsumed EF4 stage is required.");
            if (budget < 0 || budget > Math2Mode0RawOnlyFinalOrdering.DefaultPayloadBudget)
                throw new ArgumentOutOfRangeException(nameof(budget));
            // Named library safeguard, not a native record-count limit.
            if (cells.Length > 65536) throw new NotSupportedException("Library final-ordering record limit is65536.");
            foreach (var cell in cells) { token.ThrowIfCancellationRequested(); cell.Record.Validate(); }
            int width = Source.Loop.BeamWidth;
            if (width is < 1 or > 16) throw new InvalidDataException("Actual same-loop width required.");
            // Conservative combination of the existing history/EF4 reservation
            // and two168B projections + ordinal/position/seen arrays. References
            // and CLR metadata are outside this payload-only bound.
            long reservation = checked(planned + cells.LongLength * (336L + 17L));
            if (reservation > budget) throw new NotSupportedException("Combined payload reservation exceeds budget before claim.");
            token.ThrowIfCancellationRequested();
            finalOrdering = new(this, cells.Length, width, reservation);
            finalOrderingClaimed = true;
            finalOrdering.Run(observer, token);
            return finalOrdering;
        }));

    internal void ValidateFinalOrdering(Math2Mode0RawOnlyFinalOrdering candidate)
        => WithFinalOrdering(candidate, () => true);
    internal T WithFinalOrdering<T>(Math2Mode0RawOnlyFinalOrdering candidate, Func<T> action)
        => Source.WithOwner(() => Source.Loop.WithRawOnlyPostprocessing(this, () =>
        {
            if (!finalOrderingClaimed || !ReferenceEquals(finalOrdering, candidate) || released)
                throw new ArgumentException("The exact final-ordering owner is required.");
            return action();
        }));
    internal void PrepareFinalLookup(Math2Mode0RawOnlyFinalOrdering candidate)
        => WithFinalOrdering(candidate, () =>
        {
            var lookup = new Dictionary<object, Cell>(cells.Length, ReferenceEqualityComparer.Instance);
            foreach (var cell in cells) lookup.Add(cell.Record.RecordIdentity, cell);
            finalCells = lookup;
            return true;
        });
    private Cell FindFinalCell(object record)
        => finalCells != null && finalCells.TryGetValue(record, out var cell) ? cell
            : throw new ArgumentException("Foreign actual Record identity.");
    internal Math2Mode0RawOnlyEf4RecordCopy CopyFinalRecord(Math2Mode0RawOnlyFinalOrdering candidate, object record)
        => WithFinalOrdering<Math2Mode0RawOnlyEf4RecordCopy>(candidate, () =>
    {
        var cell = FindFinalCell(record);
        cell.Record.Validate();
        return new(cell.Record.Source, cell.Record.Library, cell.Record.RecordIdentity, Source.SourceFactoryIdentity,
            cell.Record.Source.CopyPrimary(), cell.Segments, cell.Phase == "skipped-empty",
            cell.Units.ToArray(), cell.Assignments.ToArray(), cell.Runs.ToArray(), cell.Initial.ToArray(),
            cell.Deleted.ToArray(), cell.Before, cell.Penalty, cell.Record.Score);
    });
    internal void FinalizeRecordOwners(Math2Mode0RawOnlyFinalOrdering candidate, int width,
        Action<Math2Mode0FinalOrderingStage, int> observe, Action<int> releasedOrdinal,
        Action<int> countChanged, Action<int[], object[]> publish, CancellationToken token)
        => WithFinalOrdering(candidate, () =>
        {
            var vector = (Math2Mode0CompletedRecords)ordered.VectorIdentity;
            vector.FinalizeRawOnlyOwners(this, width, observe, record => FindFinalCell(record).Release(),
                releasedOrdinal, countChanged, publish, token);
            return true;
        });
}

internal sealed partial class Math2Mode0CompletedRecords
{
    internal void FinalizeRawOnlyOwners(Math2Mode0RawOnlyEf4 stage, int width,
        Action<Math2Mode0FinalOrderingStage, int> observe, Action<object> releasePayload, Action<int> releasedOrdinal,
        Action<int> countChanged, Action<int[], object[]> publish, CancellationToken token)
        => owner.WithCompletedStorage(() =>
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            if (failed || !initialOrderClaimed || !stage.Source.Loop.OwnsRawOnlyVector(this))
                throw new InvalidOperationException("Actual successful postprocessed vector required.");
            Record[] original = records.ToArray();
            try
            {
                byte[] projection = new byte[checked(original.Length * 168)];
                for (int at = 0; at < original.Length; at++)
                {
                    token.ThrowIfCancellationRequested();
                    var row = projection.AsSpan(at * 168, 168);
                    BinaryPrimitives.WriteSingleLittleEndian(row, original[at].Score);
                    BinaryPrimitives.WriteInt32LittleEndian(row[8..], at);
                }
                observe(Math2Mode0FinalOrderingStage.BeforeNumericOrdering, -1);
                byte[] sorted = original.Length > 1 ? LegacyPreparedMathCandidateOrdering.Sort(projection, token) : projection;
                int[] order = new int[original.Length];
                bool[] seen = new bool[original.Length];
                for (int at = 0; at < order.Length; at++)
                {
                    token.ThrowIfCancellationRequested();
                    var row = sorted.AsSpan(at * 168, 168);
                    int prior = BinaryPrimitives.ReadInt32LittleEndian(row[8..]);
                    if (prior < 0 || prior >= order.Length || seen[prior] || !row.SequenceEqual(projection.AsSpan(prior * 168, 168)))
                        throw new InvalidDataException("Exact numeric identity permutation required.");
                    seen[prior] = true; order[at] = prior;
                }
                observe(Math2Mode0FinalOrderingStage.AfterNumericOrdering, -1);
                int[] atPosition = Enumerable.Range(0, original.Length).ToArray();
                int[] positionOf = atPosition.ToArray();
                for (int at = 0; at < order.Length; at++)
                {
                    int target = order[at], from = positionOf[target];
                    if (from == at) continue;
                    observe(Math2Mode0FinalOrderingStage.BeforeOwnerMove, at);
                    int displaced = atPosition[at];
                    (records[at], records[from]) = (records[from], records[at]);
                    (atPosition[at], atPosition[from]) = (atPosition[from], atPosition[at]);
                    positionOf[target] = at; positionOf[displaced] = from;
                    observe(Math2Mode0FinalOrderingStage.AfterOwnerMove, at);
                }
                //035000 full-tail branch: ascending destructors, then one count
                //write. Keep every original ref during injected managed faults.
                if (records.Count > 1 && records.Count > width)
                {
                    for (int at = width; at < records.Count; at++)
                    {
                        observe(Math2Mode0FinalOrderingStage.BeforeRelease, at);
                        var record = records[at];
                        record.Primary.Clear(); record.Secondary.Clear();
                        releasePayload(record);
                        record.Library.Release();
                        releasedOrdinal(order[at]);
                        observe(Math2Mode0FinalOrderingStage.AfterRelease, at);
                    }
                    observe(Math2Mode0FinalOrderingStage.BeforeCountCommit, width);
                    records.RemoveRange(width, records.Count - width);
                    countChanged(records.Count);
                    observe(Math2Mode0FinalOrderingStage.AfterCountCommit, width);
                }
                publish(order, records.Cast<object>().ToArray());
            }
            catch { failed = true; owner.FailCompletedRecording(); throw; }
            finally { GC.KeepAlive(original); }
            return true;
        });
}
