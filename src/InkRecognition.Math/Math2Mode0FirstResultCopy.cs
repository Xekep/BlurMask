using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathFirstResultCopy.Probe")]

namespace InkRecognition.Math;

internal enum Math2Mode0FirstCopyStage
{ Claimed, AfterPool, AfterHeader, AfterPrimary, AfterSecondary, AfterUnits, AfterRuns, AfterIntern, AfterAlternative, AfterRow, BeforeCompletion }

//No decoder Source, RecordIdentity, event lease, model, history or original
//string pool is retained. The immutable original document binds copied ranges.
internal sealed record Math2Mode0FirstCopyRecord(float Score, long Frontier, int Parent,
    ushort[] Primary, ushort[] Secondary, uint[] Units, MathSegmentRun[] Runs,
    Math2Mode0OutputUnit[] UnitRows, LegacyNativeInkPositionRange[][] SourceRanges);
internal sealed record Math2Mode0FirstCopyStatistics(long PayloadReservation, bool Complete,
    bool Failed, bool Released, string Phase, int ScoreBits, long Frontier, int Parent, int PrimaryCount, int SecondaryCount,
    int UnitCount, int RunCount, int RowsWritten, int RangesWritten, int RangeRowsWritten, int StringsInterned, int ReleaseCount, LegacyNativeInkPositionRange[][] RangePrefix);

/// <summary>Restricted fresh043AC0/043740 first-result copy. A separate pool
/// owns copied intervals; disposal of the decoder does not invalidate them.
/// Actual source ranges retain their original document domain. No C25,
/// tensor/coordinate/auxiliary branch, ABI or outer acceptance claim.</summary>
internal sealed class Math2Mode0FirstResultCopy : IDisposable
{
    internal const long DefaultPayloadBudget = 16L * 1024 * 1024;
    private readonly object gate = new();
    private readonly long reservation;
    internal LegacyOwnedRawInkDocument Document { get; }
    private Math2Mode0EmptyLibrary? library;
    private float score;
    private long frontier;
    private int parent = -1; //Fresh F0 record+60, initialized by026F00 at F0+80.
    private ushort[] primary = [], secondary = [];
    private uint[] units = [];
    private MathSegmentRun[] runs = [];
    private Math2Mode0OutputUnit[] rows = [];
    private string phase = "claimed";
    private int written, rangesWritten, rangeRowsWritten;
    private LegacyNativeInkPositionRange[][] sourceRanges = [];
    private bool complete, failed, released;
    internal bool IsAcceptedFormula => false;
    private Math2Mode0FirstResultCopy(long reservation, LegacyOwnedRawInkDocument document)
    { this.reservation = reservation; Document = document; }

    internal static Math2Mode0FirstResultCopy Create(Math2Mode0SourceBoundOutput output,
        CancellationToken token = default) => CreateDiagnostic(output, DefaultPayloadBudget, null, token);
    internal static Math2Mode0FirstResultCopy CreateDiagnostic(Math2Mode0SourceBoundOutput output,
        long budget, Action<Math2Mode0FirstCopyStage, int, Math2Mode0FirstResultCopy>? observer,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(output); token.ThrowIfCancellationRequested();
        if (budget < 0 || budget > DefaultPayloadBudget) throw new ArgumentOutOfRangeException(nameof(budget));
        return output.WithFirstSourceRecordForCopy((source, actualRanges, outputReservation) =>
        {
            token.ThrowIfCancellationRequested();
            if (source.Source.Offset is < 0 or > 1000 || source.Units.Length > 65535 ||
                source.UnitRows.Length != source.Units.Length || actualRanges.Length != source.Units.Length)
                throw new NotSupportedException("Complete bounded raw-only first result required.");
            long strings = 0, peak = 0;
            var seen = new HashSet<LegacyNativeInternedUtf8>(ReferenceEqualityComparer.Instance);
            for (int i = 0; i < source.UnitRows.Length; i++)
            {
                token.ThrowIfCancellationRequested(); var row = source.UnitRows[i];
                if (row is null || row.Unit != source.Units[i] || row.ByteCount != row.Text.ByteCount || row.ByteCount < 1)
                    throw new NotSupportedException("Actual nonempty ordinary alternatives required.");
                if (seen.Add(row.Text)) strings = checked(strings + 5L * row.ByteCount + 1);
                peak = System.Math.Max(peak, 5L * row.ByteCount);
            }
            //Combined live output + copied primitive/string/key payload.
            //Metadata/CLR headers and caller inspection arrays are excluded.
            long rangeCount = actualRanges.Sum(row => row.LongLength);
            long planned = checked(outputReservation + 96 + source.Source.Offset * 4 +
                source.Units.LongLength * (4 + 40 + 16 + 8) + source.Runs.LongLength * 8 +
                rangeCount * 16L + strings + peak);
            if (planned > budget) throw new NotSupportedException("Combined first-copy payload exceeds budget before claim.");
            token.ThrowIfCancellationRequested();
            output.Output.ClaimFirstCopy();
            Math2Mode0FirstResultCopy target;
            try { target = new(planned, output.Document); }
            catch { output.Output.Owner.Ef4.Source.Loop.FailRawAttentionHistory(); throw; }
            lock (target.gate)
            {
            void Observe(Math2Mode0FirstCopyStage stage, int index = -1)
            {
                target.phase = stage.ToString(); observer?.Invoke(stage, index, target);
                if (target.released) throw new ObjectDisposedException(nameof(Math2Mode0FirstResultCopy));
                output.ValidateForCopy(output.Output, token); token.ThrowIfCancellationRequested();
            }
            try
            {
                Observe(Math2Mode0FirstCopyStage.Claimed);
                target.library = new(1); target.library.InitializeEmptyPool(); Observe(Math2Mode0FirstCopyStage.AfterPool);
                target.score = source.Score; target.frontier = source.Source.Frontier.Value;
                target.parent = source.Source.CompletedSourceParent; Observe(Math2Mode0FirstCopyStage.AfterHeader);
                target.primary = source.Source.CopyPrimary(); Observe(Math2Mode0FirstCopyStage.AfterPrimary);
                target.secondary = source.Source.CopySecondary(); Observe(Math2Mode0FirstCopyStage.AfterSecondary);
                target.units = source.Units.ToArray(); Observe(Math2Mode0FirstCopyStage.AfterUnits);
                target.runs = source.Runs.ToArray(); Observe(Math2Mode0FirstCopyStage.AfterRuns);
                target.rows = new Math2Mode0OutputUnit[source.UnitRows.Length];
                target.sourceRanges = new LegacyNativeInkPositionRange[source.UnitRows.Length][];
                for (int i = 0; i < source.UnitRows.Length; i++)
                {
                    token.ThrowIfCancellationRequested(); var row = source.UnitRows[i];
                    var text = target.library.InternOutput(row.Text.ReadBytes(), token);
                    Observe(Math2Mode0FirstCopyStage.AfterIntern, i);
                    target.rows[i] = row with { Text = text }; target.written++;
                    Observe(Math2Mode0FirstCopyStage.AfterAlternative, i);
                    target.sourceRanges[i] = actualRanges[i].ToArray();
                    target.rangesWritten += actualRanges[i].Length; target.rangeRowsWritten++;
                    Observe(Math2Mode0FirstCopyStage.AfterRow, i);
                }
                Observe(Math2Mode0FirstCopyStage.BeforeCompletion); target.complete = true;
                return target;
            }
            catch
            {
                target.failed = true;
                output.Output.Owner.Ef4.Source.Loop.FailRawAttentionHistory();
                //Production convenience creation owns unfinished targets.
                //A friend observer explicitly retains diagnostic failure state.
                if (observer is null) target.Dispose();
                throw;
            }
            }
        });
    }
    internal Math2Mode0FirstCopyRecord CopyRecord()
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(released, this);
            if (!complete || failed) throw new InvalidOperationException("Complete detached first copy required.");
            return new(score, frontier, parent, primary.ToArray(), secondary.ToArray(), units.ToArray(), runs.ToArray(), rows.ToArray(), sourceRanges.Select(row => row.ToArray()).ToArray());
        }
    }
    internal Math2Mode0FirstCopyStatistics CopyStatistics()
    { lock (gate) return new(reservation, complete, failed, released, phase, BitConverter.SingleToInt32Bits(score), frontier, parent, primary.Length,
        secondary.Length, units.Length, runs.Length, written, rangesWritten, rangeRowsWritten, library?.OutputStringCount ?? 0, library?.ReleaseCount ?? 0,
        sourceRanges.Select(row => row?.ToArray() ?? []).ToArray()); }
    public void Dispose()
    {
        lock (gate)
        {
            if (released) return; released = true; library?.Release();
            primary = []; secondary = []; units = []; runs = []; rows = []; sourceRanges = [];
        }
    }
}

internal sealed partial class Math2Mode0RawOnlyOutput
{
    private bool firstCopyClaimed;
    internal T WithFirstForCopy<T>(Func<Math2Mode0OutputRecord, long, T> action)
        => owner.WithOutput(this, () =>
        {
            if (!complete || failed || firstCopyClaimed || output.Length == 0 || output[0] is not { } first)
                throw new InvalidOperationException("Complete unconsumed same-owner first output required.");
            return action(first, reservation);
        });
    internal void ClaimFirstCopy()
    { if (firstCopyClaimed) throw new InvalidOperationException("First copy already claimed."); firstCopyClaimed = true; }
}

internal sealed partial class Math2Mode0SourceBoundOutput
{
    //Metadata/private immutable ranges only. Caller copies payload after budget
    //and one-use claim, while the SAME output/source gate stays held.
    internal T WithFirstSourceRecordForCopy<T>(Func<Math2Mode0OutputRecord,
        LegacyNativeInkPositionRange[][], long, T> action)
        => Output.WithFirstForCopy((source, _) =>
        {
            ValidateForCopy(Output);
            if (records.Length == 0 || attached.Length != records.Length ||
                !ReferenceEquals(records[0], source.RecordIdentity) || attached[0].Length != source.UnitRows.Length)
                throw new InvalidOperationException("Same actual first record and complete attached source ranges required.");
            return action(source, attached[0], reservation);
        });
}
