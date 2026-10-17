using System.Buffers.Binary;

namespace InkRecognition;

public readonly record struct LegacyNativeGlyphRecordInsertion(ulong Reference, bool ExistingDescriptor);

/// <summary>274 per-source-key record production1399f0/13a670/13acc0 and
/// normal symbol update13af30. Portable key storage replaces native AVL
/// pointers/rotations; native balance bits are not reconstructed. Owned
/// snapshots retain lookup-relevant count/duplicate flags and complete rows.</summary>
public sealed partial class LegacyNativeGlyphRecordPool
{
    private const int MaximumOwners = 4096;
    private const int MaximumTotalRecords = 65536;
    private readonly Dictionary<ulong, Node> nodes = [];
    private int totalRecords;

    private LegacyNativeGlyphRecordPool() { }

    /// <summary>The source274 capture flag is actual caller configuration,
    /// not inferred from the alphabet, TOP273 scores or first model. The
    /// geometry280 payload allocator branch requires a separate producer.</summary>
    public static LegacyNativeGlyphRecordPool CreateWithoutGeometryCapture(bool actualCaptureFlag,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (actualCaptureFlag) throw new NotSupportedException("Native geometry280 capture requires its actual payload factory.");
        return new();
    }

    /// <summary>Native owner key is already canonical low24 node/arc16/bit56.
    /// DescriptorId is signedI32 field00, not the row's symbolU16 field08.
    /// Opaque bytes in an allocated row remain explicit caller storage.</summary>
    public LegacyNativeGlyphRecordInsertion AppendDescriptor(ulong ownerKey, int descriptorId,
        bool forceDuplicate, ReadOnlySpan<byte> initialRowStorage40,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (ownerKey != LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(ownerKey) || initialRowStorage40.Length != 40)
            throw new ArgumentException("Canonical native key and complete allocated-row storage required.");
        nodes.TryGetValue(ownerKey, out Node? node);
        if (node is not null && !forceDuplicate && !node.NoDescriptorDedup)
            for (int ordinal = 0; ordinal < node.Rows.Count; ordinal++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (ReadI32(node.Rows[ordinal], 0) == descriptorId) return new(ownerKey | ((ulong)ordinal << 40), true);
            }
        //13a670 grows capacity by ten and refuses the next growth above7FE.
        //Thus a fresh native node can hold2040 rows, not2046 or2047.
        int count = node?.Rows.Count ?? 0, capacity = node?.Capacity ?? 0;
        int nextCapacity = count < capacity ? capacity : capacity + 10;
        if (nextCapacity > 0x7fe || totalRecords == MaximumTotalRecords ||
            node is null && nodes.Count == MaximumOwners)
            throw new ArgumentException("Glyph pool exceeds native capacity or portable ownership budget.");
        byte[] row = initialRowStorage40.ToArray();
        WriteI32(row, 0, descriptorId);
        WriteU16(row, 4, (ushort)(ReadU16(row, 4) & 0xfff0));
        WriteI32(row, 6, unchecked((int)0xffff0000));
        WriteI32(row, 12, 0x3f800000); WriteI32(row, 16, unchecked((int)0xbf800000));
        WriteI32(row, 20, 0);
        row.AsSpan(24, 8).Clear(); row.AsSpan(32, 8).Fill(255);
        cancellationToken.ThrowIfCancellationRequested();
        if (node is null) { node = new(); nodes.Add(ownerKey, node); }
        node.Capacity = nextCapacity; node.NoDescriptorDedup |= forceDuplicate;
        node.Rows.Add(row); totalRecords++;
        return new(ownerKey | ((ulong)count << 40), false);
    }

    /// <summary>Exact13af30 ordinary one-component symbol branch: kind0 and
    /// raw alphabet label08. Encoded macro labelsFFF9..FFFC/count-zero/kind4
    /// remain separate. Existing upper flags and publication word06 survive.</summary>
    public void SetOrdinarySymbol(ulong reference, ushort symbol,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (symbol is >= 0xfff9 and <= 0xfffc)
            throw new NotSupportedException("Native encoded macro/count branch requires its actual component fields.");
        byte[] row = ResolveRow(reference);
        WriteU16(row, 4, (ushort)(ReadU16(row, 4) & 0xfff8)); WriteU16(row, 8, symbol);
    }

    public byte[] CopyRecord(ulong reference, CancellationToken cancellationToken = default)
    { cancellationToken.ThrowIfCancellationRequested(); return ResolveRow(reference).ToArray(); }

    /// <summary>Publish actual classifier/geometry/trait channels after their
    /// independent native producers. Descriptor identity00 must remain owned;
    /// this does not derive glyph scores from TOP273 mass.</summary>
    public void ReplaceRecord(ulong reference, ReadOnlySpan<byte> actualRecord40,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        byte[] row = ResolveRow(reference);
        if (actualRecord40.Length != 40 || ReadI32(actualRecord40, 0) != ReadI32(row, 0))
            throw new ArgumentException("Complete actual record with unchanged descriptor identity required.");
        actualRecord40.CopyTo(row);
    }

    /// <summary>13ac80 owner lookup (not13ae90 single-record lookup). Key is
    /// already canonical. Missing owners are unavailable, not empty glyphs.</summary>
    public LegacyNativeGlyphRecordNode CopyOwnerNode(ulong canonicalKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (canonicalKey != LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(canonicalKey) ||
            !nodes.TryGetValue(canonicalKey, out Node? node))
            throw new ArgumentException("Actual canonical glyph owner is unavailable.");
        return Snapshot(canonicalKey, node, cancellationToken);
    }

    /// <summary>Lookup snapshots for the existing13ae90 resolver. Low2 AVL
    /// balance bits are portablezero; count extraction discards those bits.
    /// This is not a byte-for-byte dump of native64-byte AVL nodes.</summary>
    public IReadOnlyList<LegacyNativeGlyphRecordNode> CopyOwnerNodes(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = new List<LegacyNativeGlyphRecordNode>(nodes.Count);
        foreach (var item in nodes) result.Add(Snapshot(item.Key, item.Value, cancellationToken));
        return result.AsReadOnly();
    }

    private static LegacyNativeGlyphRecordNode Snapshot(ulong key, Node node, CancellationToken token)
    {
        byte[] records = new byte[node.Rows.Count * 40];
        for (int ordinal = 0; ordinal < node.Rows.Count; ordinal++)
        { token.ThrowIfCancellationRequested(); node.Rows[ordinal].CopyTo(records, ordinal * 40); }
        ushort packed = (ushort)((node.Rows.Count << 2) | (node.NoDescriptorDedup ? 0x8000 : 0));
        return new(key, packed, records);
    }

    private byte[] ResolveRow(ulong reference)
    {
        int ordinal = unchecked((short)(reference >> 40));
        ulong key = LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference);
        if (!nodes.TryGetValue(key, out Node? node) || ordinal < 0 || ordinal >= node.Rows.Count)
            throw new ArgumentException("Glyph record reference is outside this actual owned pool.");
        return node.Rows[ordinal];
    }

    private sealed class Node
    { public readonly List<byte[]> Rows = []; public int Capacity; public bool NoDescriptorDedup; }
    private static int ReadI32(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadInt32LittleEndian(data[offset..]);
    private static ushort ReadU16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
    private static void WriteI32(byte[] data, int offset, int value) => BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(offset), value);
    private static void WriteU16(byte[] data, int offset, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(offset), value);
}
