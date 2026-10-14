using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Actual274 owner-tree key/count and its contiguous40-byte records, without native pointer substitution.</summary>
public readonly record struct LegacyNativeGlyphRecordNode(ulong Key, ushort PackedCount, ReadOnlyMemory<byte> Records);

/// <summary>Owned default13AE90 glyph lookup snapshot. Virtual08 returns its64-byte AVL owner;120-byte276 words are separate linked references.</summary>
public sealed class LegacyNativeGlyphRecordOwner
{
    private readonly Dictionary<ulong, byte[]> _records = [];
    public LegacyNativeGlyphRecordOwner(IReadOnlyList<LegacyNativeGlyphRecordNode> actualNodes)
    {
        ArgumentNullException.ThrowIfNull(actualNodes);
        if (actualNodes.Count > 4096) throw new ArgumentOutOfRangeException(nameof(actualNodes));
        int total = 0;
        foreach (var node in actualNodes)
        {
            int count = unchecked((short)(node.PackedCount << 2)) >> 4;
            if (count < 0 || node.Key != NormalizeOwnerKey(node.Key) || node.Records.Length != count * 40)
                throw new ArgumentException("Actual normalized274 key, signed packed count and complete40-byte records required.");
            total = checked(total + count);
            if (total > 65536 || !_records.TryAdd(node.Key, node.Records.ToArray()))
                throw new ArgumentException("Owner record budget exceeded or duplicate native tree key.");
        }
    }

    /// <summary>13AE90 strips bits40..55 from the key, retains only bit56 above the arc, then uses a signed16-bit record index.</summary>
    public LegacyNativeOwnedGlyphRecord Resolve(ulong actualReference)
    {
        short index = unchecked((short)(actualReference >> 40)); ulong key = NormalizeOwnerKey(actualReference);
        if (!_records.TryGetValue(key, out var rows) || index < 0 || index >= rows.Length / 40)
            throw new ArgumentException("Actual glyph reference is absent or outside this274 owner.", nameof(actualReference));
        return new(this, actualReference, rows.AsSpan(index * 40, 40));
    }

    public static ulong NormalizeOwnerKey(ulong reference) =>
        (reference & 0x00ffffffUL) | (reference & 0x000000ffff000000UL) | (reference & 0x0100000000000000UL);
}

/// <summary>One immutable actual40-byte glyph274 record and its retained owner/reference identity.</summary>
public sealed class LegacyNativeOwnedGlyphRecord
{
    public LegacyNativeGlyphRecordOwner Owner { get; }
    public ulong Reference { get; }
    private readonly byte[] _record;
    public LegacyNativeGlyphSymbolDescriptor Symbol => new(BinaryPrimitives.ReadUInt16LittleEndian(_record.AsSpan(4)), BinaryPrimitives.ReadUInt16LittleEndian(_record.AsSpan(8)));
    public ushort PublicationFlags => BinaryPrimitives.ReadUInt16LittleEndian(_record.AsSpan(6));
    internal LegacyNativeOwnedGlyphRecord(LegacyNativeGlyphRecordOwner owner, ulong reference, ReadOnlySpan<byte> record)
    { Owner = owner; Reference = reference; _record = record.ToArray(); }
    public byte[] ReadRecord() => (byte[])_record.Clone();
}
