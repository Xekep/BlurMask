using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Prepared21FFD0 cursor. Record indices refer to compact40 storage;
/// node/arc are packed source references, not character bounds or XY positions.</summary>
public readonly record struct LegacyCompactWordBacktrackCursor(
    int RecordIndex, int SourceNode, ushort Arc, ushort Alternative)
{
    public bool IsEnd => SourceNode == -1;
}

/// <summary>21FFD0's initialization and predecessor/alternative resolution.
/// The compact2460 pool, per-node2478 starts and selected alternative must have
/// already been materialized. This does not implement glyph publication,
/// protocol filtering, callbacks or final text selection.</summary>
public static class LegacyPreparedCompactWordPredecessor
{
    public static LegacyCompactWordBacktrackCursor Begin(ReadOnlySpan<byte> compact40,
        ReadOnlySpan<int> nodeStarts2478, int sourceNode, int relativeRecord,
        ushort selectedAlternative = ushort.MaxValue, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidatePool(compact40);
        if ((uint)sourceNode >= nodeStarts2478.Length || relativeRecord < 0)
            throw new ArgumentException("Actual selected source node and nonnegative compact-relative ordinal required.");
        int index = unchecked(nodeStarts2478[sourceNode] + relativeRecord);
        var row = Row(compact40, index);
        ushort alternative = selectedAlternative == ushort.MaxValue ? U16(row, 30) : selectedAlternative;
        return new(index, sourceNode, Arc(row), alternative);
    }

    public static LegacyCompactWordBacktrackCursor Advance(ReadOnlySpan<byte> compact40,
        ReadOnlySpan<int> nodeStarts2478, LegacyCompactWordBacktrackCursor current,
        ushort special24ea, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidatePool(compact40);
        if (current.IsEnd) throw new ArgumentException("End cursor requires the remaining21FFD0 flush stage, not another predecessor step.");
        var row = Row(compact40, current.RecordIndex);
        int previous = BinaryPrimitives.ReadInt32LittleEndian(row[16..]);
        if (previous == -1) return new(-1, -1, ushort.MaxValue, current.Alternative);
        var predecessor = Row(compact40, previous);
        ushort alternative = current.Alternative;
        //The comparison is signed24 versus the current signed node. The
        //marker test remains literal even when24EA itself equalsFFFF.
        bool select = U16(predecessor, 30) != ushort.MaxValue ||
            (U16(row, 28) == special24ea && Node(predecessor) < current.SourceNode);
        if (select)
        {
            //FFFE/FFFF keep the direct predecessor, but still load its1E.
            if (alternative < 0xfffe)
            {
                int node = Node(predecessor);
                if ((uint)node >= nodeStarts2478.Length)
                    throw new InvalidDataException("Compact predecessor node lies outside the supplied2478 table.");
                previous = unchecked(nodeStarts2478[node] + alternative);
                predecessor = Row(compact40, previous);
            }
            alternative = U16(predecessor, 30);
        }
        return new(previous, Node(predecessor), Arc(predecessor), alternative);
    }

    private static void ValidatePool(ReadOnlySpan<byte> pool)
    {
        if (pool.IsEmpty || pool.Length % 40 != 0)
            throw new ArgumentException("Nonempty complete compact40 storage required.");
    }
    private static ReadOnlySpan<byte> Row(ReadOnlySpan<byte> pool, int index)
    {
        if ((uint)index >= pool.Length / 40)
            throw new InvalidDataException("Compact index lies outside the supplied materialized pool.");
        return pool.Slice(index * 40, 40);
    }
    private static int Node(ReadOnlySpan<byte> row)
        => (BinaryPrimitives.ReadInt32LittleEndian(row[8..]) << 8) >> 8;
    private static ushort Arc(ReadOnlySpan<byte> row)
        => (ushort)(BinaryPrimitives.ReadUInt64LittleEndian(row[8..]) >> 24);
    private static ushort U16(ReadOnlySpan<byte> row, int offset)
        => BinaryPrimitives.ReadUInt16LittleEndian(row[offset..]);
}
