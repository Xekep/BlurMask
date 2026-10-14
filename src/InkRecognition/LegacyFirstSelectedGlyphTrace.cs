using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>An actual merged cell visited by1f42d0's reverse glyph walk.
/// Node/arc are prepared graph identities, not source-stroke/point ordinals.</summary>
public sealed class LegacyFirstSelectedGlyphCell
{
    private readonly byte[] record;
    public int Node { get; }
    public int Row { get; }
    public int Column { get; }
    public ushort Arc { get; }
    public ushort Symbol { get; }
    public byte Span { get; }
    public bool WordBoundaryBit { get; }
    public int PreviousNode { get; }
    public ushort PreviousArc { get; }
    internal LegacyFirstSelectedGlyphCell(int node, int row, int column, ReadOnlySpan<byte> record,
        int previousNode, ushort previousArc)
    {
        Node = node; Row = row; Column = column; this.record = record.ToArray();
        Arc = BinaryPrimitives.ReadUInt16LittleEndian(record[16..]);
        Symbol = BinaryPrimitives.ReadUInt16LittleEndian(record[12..]);
        ushort packed = BinaryPrimitives.ReadUInt16LittleEndian(record[14..]);
        Span = (byte)((packed >> 1) & 7); WordBoundaryBit = (packed & 1) != 0;
        PreviousNode = previousNode; PreviousArc = previousArc;
    }
    public ReadOnlyMemory<byte> CopyRecord() => record.ToArray();
}

/// <summary>Exact selected B8 predecessor traversal inside1f42d0. Returns
/// source-order cells; no glyph alternatives, source ranges, casing, word
/// delimiters or geometry are invented from the numeric context identity.</summary>
public static class LegacyFirstSelectedGlyphTrace
{
    public static IReadOnlyList<LegacyFirstSelectedGlyphCell> Read(LegacyFirstMergedRecordGrid grid,
        LegacyFirstTerminalChoice selected, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(grid); cancellationToken.ThrowIfCancellationRequested();
        int node = grid.NodeCount - 1, row = selected.Row, column = selected.Column;
        var terminal = grid.Record(node, row, column);
        if (BinaryPrimitives.ReadUInt64LittleEndian(terminal[24..]) != selected.ContextIdentity)
            throw new ArgumentException("Selection does not belong to the supplied merged cell.");
        var reverse = new List<LegacyFirstSelectedGlyphCell>();
        while (node >= 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var record = grid.Record(node, row, column);
            if (BinaryPrimitives.ReadInt32LittleEndian(record) >= 0x4000000)
                throw new InvalidDataException("A selected glyph predecessor is not a finite merged record.");
            int previous = node - ((BinaryPrimitives.ReadUInt16LittleEndian(record[14..]) >> 1) & 7) - 1;
            if (previous < -1) throw new InvalidDataException("Selected glyph span escapes the prepared graph.");
            ushort previousArc = ushort.MaxValue;
            if (previous >= 0)
                previousArc = BinaryPrimitives.ReadUInt16LittleEndian(grid.Record(previous, record[9], record[10])[16..]);
            reverse.Add(new(node, row, column, record, previous, previousArc));
            //The native root sentinel ends traversal before its unused predecessor bytes are consumed.
            row = record[9]; column = record[10]; node = previous;
        }
        reverse.Reverse(); cancellationToken.ThrowIfCancellationRequested();
        return reverse.AsReadOnly();
    }
}
