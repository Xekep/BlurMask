using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Owned merged B8 node tables, each holding F8 rows of A4 complete
/// 40-byte records. The merge passes must produce these tables themselves;
/// this type does not substitute primary-only records for merged contexts.</summary>
public sealed class LegacyFirstMergedRecordGrid
{
    private readonly byte[][] nodes;
    public int ColumnCountA4 { get; }
    public int RowCountF8 { get; }
    public int NodeCount => nodes.Length;

    public LegacyFirstMergedRecordGrid(int columnCountA4, int rowCountF8,
        IReadOnlyList<ReadOnlyMemory<byte>> nodeStorage, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(nodeStorage);
        cancellationToken.ThrowIfCancellationRequested();
        if (columnCountA4 is < 0 or > 256 || rowCountF8 is < 0 or > 256 ||
            nodeStorage.Count is < 1 or > 4096 || (long)columnCountA4 * rowCountF8 * nodeStorage.Count > 1_048_576)
            throw new ArgumentException("Bounded actual merged-table dimensions and a nonempty source required.");
        ColumnCountA4 = columnCountA4; RowCountF8 = rowCountF8;
        nodes = new byte[nodeStorage.Count][];
        int bytes = checked(columnCountA4 * rowCountF8 * 40);
        for (int node = 0; node < nodes.Length; node++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (nodeStorage[node].Length != bytes) throw new ArgumentException("Each B8 node must cover its exact merged grid.");
            nodes[node] = nodeStorage[node].ToArray();
        }
    }

    public ReadOnlyMemory<byte> CopyRecord(int node, int row, int column) => Record(node, row, column).ToArray();

    internal ReadOnlySpan<byte> Record(int node, int row, int column)
    {
        if ((uint)node >= (uint)nodes.Length || (uint)row >= (uint)RowCountF8 || (uint)column >= (uint)ColumnCountA4)
            throw new ArgumentOutOfRangeException(nameof(node), "The actual merged predecessor cell is unavailable.");
        return nodes[node].AsSpan((ColumnCountA4 * row + column) * 40, 40);
    }
}

/// <summary>Actual controller/configuration fields read by nonempty1f42d0.
/// No source-language, trailing-label or reference-cost default is inferred.</summary>
public readonly record struct LegacyFirstTerminalSelectionPolicy(ushort TrailingSymbolE6, int TraitPenaltyF4,
    bool Configuration99, bool ConfigurationA8, int SourceKind, long ExistingReferenceCost78);

/// <summary>Literal1f42d0 scalar result channels. Mass is unnormalised and can
/// underflow/overflow as native exp does; it is not acceptance confidence.</summary>
public readonly record struct LegacyFirstTerminalScore(int RelativeCost, float NormalizedCost10,
    float LogMass, float Mass08, float NormalizedAcousticCost14);

public readonly record struct LegacyFirstTerminalChoice(int Row, int Column, int TotalCost, int AcousticCost,
    ulong ContextIdentity, int TraitNode, int TraitRow, int TraitColumn, LegacyFirstTerminalScore Score);
public sealed record LegacyFirstTerminalSelectionResult(IReadOnlyList<LegacyFirstTerminalChoice> Choices,
    long ReferenceCost78);

/// <summary>Nonempty1f42d0 terminal scan/identity exclusion and scalar output
/// preparation. It requires the actual final merged grid. Result273 allocation,
/// character/source backtracking and empty-source frontend dispatch are separate.</summary>
public static class LegacyFirstTerminalSelection
{
    private const int Impossible = 0x4000000;

    public static LegacyFirstTerminalSelectionResult Select(LegacyFirstMergedRecordGrid grid,
        LegacyFirstTerminalSelectionPolicy policy, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(grid);
        cancellationToken.ThrowIfCancellationRequested();
        var output = new List<LegacyFirstTerminalChoice>();
        var emitted = new HashSet<ulong>();
        long reference = policy.ExistingReferenceCost78;
        int firstCost = 0;
        for (int iteration = 0; iteration < grid.ColumnCountA4; iteration++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int bestCost = Impossible, bestRow = -1, bestColumn = -1, bestAcoustic = 0;
            int bestTraitNode = -1, bestTraitRow = -1, bestTraitColumn = -1;
            for (int row = 0; row < grid.RowCountF8; row++)
                for (int column = 0; column < grid.ColumnCountA4; column++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var terminal = grid.Record(grid.NodeCount - 1, row, column);
                    int cost = I32(terminal, 0);
                    if (cost >= Impossible) break; // Native stops this row, not the entire table.
                    int traitNode = grid.NodeCount - 1, traitRow = row, traitColumn = column;
                    var traitRecord = terminal;
                    while (U16(traitRecord, 12) == policy.TrailingSymbolE6)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        int previous = traitNode - ((U16(traitRecord, 14) >> 1) & 7) - 1;
                        if (previous == -1) break; // Root's predecessor bytes are not consumed.
                        if (previous < 0) throw new NotSupportedException("Native trailing predecessor would escape the source table.");
                        traitRow = traitRecord[9]; traitColumn = traitRecord[10]; traitNode = previous;
                        traitRecord = grid.Record(traitNode, traitRow, traitColumn);
                    }
                    if ((traitRecord[32] & 0x13) == 1) cost = unchecked(cost + policy.TraitPenaltyF4);
                    if (cost >= bestCost || emitted.Contains(U64(terminal, 24))) continue;
                    bestCost = cost; bestRow = row; bestColumn = column; bestAcoustic = I32(terminal, 4);
                    bestTraitNode = traitNode; bestTraitRow = traitRow; bestTraitColumn = traitColumn;
                }
            if (bestRow == -1) break;
            ulong identity = U64(grid.Record(grid.NodeCount - 1, bestRow, bestColumn), 24);
            emitted.Add(identity); // Zero is also a native deduplication identity.
            int relative = bestCost;
            if (!policy.Configuration99 && !policy.ConfigurationA8)
            {
                if (iteration == 0) firstCost = bestCost;
                relative = unchecked(bestCost - firstCost);
                reference = firstCost; // Native sign-extends this signed32 value into result78.
            }
            float normalized = (float)relative / (float)(grid.NodeCount << 13);
            float multiplier = policy.SourceKind == 0x135 ? 5f : 1f;
            float logMass = (-multiplier * (float)relative) * 0.00012207031f;
            float mass = (float)Math.Exp((double)logMass); //1f42d0 calls CRT exp, without the ±75 clamp.
            float acoustic = (float)bestAcoustic * 0.00012207031f;
            output.Add(new(bestRow, bestColumn, bestCost, bestAcoustic, identity,
                bestTraitNode, bestTraitRow, bestTraitColumn, new(relative, normalized, logMass, mass, acoustic)));
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(output.AsReadOnly(), reference);
    }

    private static int I32(ReadOnlySpan<byte> record, int offset) => BinaryPrimitives.ReadInt32LittleEndian(record[offset..]);
    private static ushort U16(ReadOnlySpan<byte> record, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(record[offset..]);
    private static ulong U64(ReadOnlySpan<byte> record, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(record[offset..]);
}
