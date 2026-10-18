using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathOuterSymbols.Probe")]

namespace InkRecognition.Math;

internal enum Math2Mode0OuterCopyStage
{
    Claimed, AfterPool, AfterVectorReserve, AfterRowInitialized, AfterCount,
    AfterMetadata, AfterAlternativeReserve, AfterIntern, AfterAlternative,
    AfterRanges, AfterRow, BeforeCompletion
}

internal sealed record Math2Mode0OuterAlternative(LegacyNativeInternedUtf8 Text, int ByteCount, uint ScalarBits);
internal sealed record Math2Mode0OuterSymbol(byte Flags, uint Metadata,
    Math2Mode0OuterAlternative[] Alternatives, LegacyNativeInkPositionRange[] Ranges);
internal sealed record Math2Mode0OuterAlternativePrefix(int Identity, int ByteCount, uint ScalarBits, byte[] Bytes);
internal sealed record Math2Mode0OuterSymbolPrefix(byte Flags, uint Metadata, int AlternativeCount,
    Math2Mode0OuterAlternativePrefix?[] AlternativeStorage, LegacyNativeInkPositionRange[] Ranges);
internal sealed record Math2Mode0OuterCopyStatistics(long PayloadReservation, bool Complete, bool Failed,
    bool Released, string Phase, int Capacity, int InitializedRows, int VectorCount, int MetadataRows,
    int AlternativesWritten, int RangeRows, int RangesWritten, int CopiedRows, int StringsInterned,
    int ReleaseCount, Math2Mode0OuterSymbolPrefix?[] Prefix);

/// <summary>ONE actual first-copy F0 symbol vector through026780 into a
/// separate outer3E/range owner. No multi-C23 grouping, CB8 tags or DOM edit.</summary>
internal sealed class Math2Mode0OuterSymbolVector : IDisposable
{
    internal const long DefaultPayloadBudget = 16L * 1024 * 1024;
    private sealed class Cell
    {
        internal byte Flags;
        internal uint Metadata = uint.MaxValue;
        internal Math2Mode0OuterAlternative?[] Alternatives = [];
        internal int AlternativeCount;
        internal LegacyNativeInkPositionRange[] Ranges = [];
    }

    private readonly object gate = new();
    private readonly long reservation;
    internal LegacyOwnedRawInkDocument Document { get; }
    private Math2Mode0EmptyLibrary? library;
    private Cell?[] cells = [];
    private bool complete, failed, released;
    private int initialized, count, metadataRows, alternativesWritten, rangeRows, rangesWritten, copiedRows;
    private string phase = "claimed";

    private Math2Mode0OuterSymbolVector(LegacyOwnedRawInkDocument document, long reservation)
    { Document = document; this.reservation = reservation; }

    internal static Math2Mode0OuterSymbolVector CreateSingle(Math2Mode0FirstResultCopy actualFirstCopy,
        CancellationToken token = default) => CreateSingleDiagnostic(actualFirstCopy, DefaultPayloadBudget, null, token);

    internal static Math2Mode0OuterSymbolVector CreateSingleDiagnostic(Math2Mode0FirstResultCopy source,
        long budget, Action<Math2Mode0OuterCopyStage, int, Math2Mode0OuterSymbolVector>? observer,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(source); token.ThrowIfCancellationRequested();
        if (budget < 0 || budget > DefaultPayloadBudget) throw new ArgumentOutOfRangeException(nameof(budget));
        return source.WithSymbolRowsForOuterCopy((rows, sourceRanges, sourceReservation) =>
        {
            if (rows.Length > 65535 || sourceRanges.Length != rows.Length)
                throw new NotSupportedException("A bounded actual single first-copy vector is required.");
            long strings = 0, peak = 0, rangeCount = 0;
            var unique = new HashSet<LegacyNativeInternedUtf8>(ReferenceEqualityComparer.Instance);
            for (int i = 0; i < rows.Length; i++)
            {
                token.ThrowIfCancellationRequested(); var row = rows[i];
                if (row is null || row.ByteCount < 1 || row.ByteCount != row.Text.ByteCount || sourceRanges[i] is null)
                    throw new NotSupportedException("The actual nonempty first-copy alternatives and source ranges are required.");
                if (unique.Add(row.Text)) strings = checked(strings + 5L * row.ByteCount + 1);
                peak = System.Math.Max(peak, 5L * row.ByteCount);
                rangeCount = checked(rangeCount + sourceRanges[i].LongLength);
            }
            //Conservative chain payload plus NEW24+72 library,72 per row
            //(40B semantics/one16B alternative/reference slots),16 per range,
            //owned interval+hex-key storage and peak temporary string work.
            //CLR headers/metadata and detached inspection arrays are excluded.
            long planned = checked(sourceReservation + 96 + rows.LongLength * 72 + rangeCount * 16 + strings + peak);
            if (planned > budget) throw new NotSupportedException("Combined outer-copy payload exceeds budget before claim.");
            source.ValidateForOuterCopy(token);
            var target = new Math2Mode0OuterSymbolVector(source.Document, planned);
            lock (target.gate)
            {
                void Observe(Math2Mode0OuterCopyStage stage, int index = -1)
                {
                    target.phase = stage.ToString(); observer?.Invoke(stage, index, target);
                    ObjectDisposedException.ThrowIf(target.released, target);
                    source.ValidateForOuterCopy(token); token.ThrowIfCancellationRequested();
                }
                try
                {
                    Observe(Math2Mode0OuterCopyStage.Claimed);
                    target.library = new(1); target.library.InitializeEmptyPool();
                    Observe(Math2Mode0OuterCopyStage.AfterPool);
                    target.cells = new Cell[rows.Length]; Observe(Math2Mode0OuterCopyStage.AfterVectorReserve);
                    for (int i = 0; i < rows.Length; i++)
                    {
                        token.ThrowIfCancellationRequested(); target.cells[i] = new(); target.initialized++;
                        Observe(Math2Mode0OuterCopyStage.AfterRowInitialized, i);
                    }
                    //02687C adopts the full initialized row count before any
                    //source flags, alternatives or range payload are copied.
                    target.count = rows.Length; Observe(Math2Mode0OuterCopyStage.AfterCount);
                    for (int i = 0; i < rows.Length; i++)
                    {
                        token.ThrowIfCancellationRequested(); var row = rows[i]; var cell = target.cells[i]!;
                        cell.Flags = row.Flags; cell.Metadata = row.Metadata; target.metadataRows++;
                        Observe(Math2Mode0OuterCopyStage.AfterMetadata, i);
                        cell.Alternatives = new Math2Mode0OuterAlternative[1];
                        Observe(Math2Mode0OuterCopyStage.AfterAlternativeReserve, i);
                        var text = target.library.InternOutput(row.Text.ReadBytes(), token);
                        Observe(Math2Mode0OuterCopyStage.AfterIntern, i);
                        cell.Alternatives[0] = new(text, row.ByteCount, row.ScalarBits);
                        cell.AlternativeCount = 1; target.alternativesWritten++;
                        Observe(Math2Mode0OuterCopyStage.AfterAlternative, i);
                        cell.Ranges = sourceRanges[i].ToArray(); target.rangeRows++;
                        target.rangesWritten += sourceRanges[i].Length;
                        Observe(Math2Mode0OuterCopyStage.AfterRanges, i);
                        target.copiedRows++; Observe(Math2Mode0OuterCopyStage.AfterRow, i);
                    }
                    Observe(Math2Mode0OuterCopyStage.BeforeCompletion); target.complete = true;
                    return target;
                }
                catch
                {
                    target.failed = true;
                    //This fresh target owns its partial rows/pool. Input is
                    //never reset; generic native existing-target reuse/ABI
                    //allocation failure order is not claimed.
                    if (observer is null) target.Dispose();
                    throw;
                }
            }
        }, token);
    }

    internal Math2Mode0OuterSymbol[] CopySymbols()
    {
        lock (gate)
        {
            ObjectDisposedException.ThrowIf(released, this);
            if (!complete || failed) throw new InvalidOperationException("Complete outer symbol copy required.");
            return cells.Select(cell => new Math2Mode0OuterSymbol(cell!.Flags, cell.Metadata,
                cell.Alternatives.Take(cell.AlternativeCount).Select(row => row!).ToArray(), cell.Ranges.ToArray())).ToArray();
        }
    }

    internal void ValidateDocument(LegacyOwnedRawInkDocument actualDocument, CancellationToken token = default)
    {
        lock (gate)
        {
            token.ThrowIfCancellationRequested(); ObjectDisposedException.ThrowIf(released, this);
            if (!complete || failed) throw new InvalidOperationException("Complete outer symbol copy required.");
            if (!ReferenceEquals(actualDocument, Document))
                throw new ArgumentException("The exact same original raw document is required.");
        }
    }

    internal Math2Mode0OuterCopyStatistics CopyStatistics()
    {
        lock (gate)
            return new(reservation, complete, failed, released, phase, cells.Length, initialized, count, metadataRows,
                alternativesWritten, rangeRows, rangesWritten, copiedRows, library?.OutputStringCount ?? 0, library?.ReleaseCount ?? 0,
                cells.Select(cell => cell is null ? null : new Math2Mode0OuterSymbolPrefix(cell.Flags, cell.Metadata,
                    cell.AlternativeCount, cell.Alternatives.Select(row => row is null ? null :
                        new Math2Mode0OuterAlternativePrefix(row.Text.Identity, row.ByteCount, row.ScalarBits, row.Text.ReadBytes())).ToArray(),
                    cell.Ranges.ToArray())).ToArray());
    }

    public void Dispose()
    {
        lock (gate)
        {
            if (released) return;
            released = true; cells = []; initialized = count = 0; library?.Release();
        }
    }
}
