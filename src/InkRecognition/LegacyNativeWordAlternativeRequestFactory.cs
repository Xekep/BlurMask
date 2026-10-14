using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Literal regular1fe740 geometry request. Its initial cost contains
/// only the parent cost; the new query cost is added after per-arc reduction.
/// Both FirstId fields use the sampler table, unlike regular201270.</summary>
public static class LegacyNativeWordAlternativeRequestFactory
{
    public static LegacyNativeWordRecombinationRequest Create(uint endingNode, uint span,
        int previousEndingNode, ReadOnlySpan<byte> parent88, ReadOnlySpan<byte> related88,
        ushort currentArc, ushort symbol, int compositeCategory, ulong queryTag,
        byte queryLength, byte queryFlags, uint currentHeader, uint parentHeader,
        byte permission, LegacyCursiveCategoryProfile categories,
        LegacyNativeWordTransitionCostCell cell, ReadOnlySpan<byte> residue80,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(categories);
        ArgumentNullException.ThrowIfNull(cell);
        cancellationToken.ThrowIfCancellationRequested();
        if (endingNode > int.MaxValue || previousEndingNode < 0 || parent88.Length != 88 ||
            related88.Length != 88 || residue80.Length != 80 ||
            (uint)compositeCategory >= (uint)categories.CompositeCategories.Length)
            throw new ArgumentException("Actual bounded nodes, parent/related88 rows and request80 required.");
        int previousCategory = parent88[0x1a] & 63;
        if ((uint)previousCategory >= (uint)categories.CompositeCategories.Length)
            throw new ArgumentException("Actual previous composite category is outside the sampler table.");
        var current = categories.CompositeCategories.Span[compositeCategory];
        var previous = categories.CompositeCategories.Span[previousCategory];
        byte[] bytes = residue80.ToArray();
        W32(0, endingNode);
        W32(4, BinaryPrimitives.ReadUInt16LittleEndian(parent88[0x18..]));
        W32(8, (uint)previousEndingNode);
        W16(12, currentArc);
        W16(14, (ushort)(BinaryPrimitives.ReadUInt32LittleEndian(parent88[0x18..]) >> 22));
        W16(16, BinaryPrimitives.ReadUInt16LittleEndian(parent88[0x1c..]));
        bytes[18] = checked((byte)current.GeometryIndex);
        bytes[19] = checked((byte)previous.GeometryIndex);
        W32(20, unchecked((uint)current.FirstId));
        W32(24, unchecked((uint)previous.FirstId));
        bytes[28] = (byte)(queryFlags & 7);
        W32(32, span);
        W16(36, symbol);
        W16(38, BinaryPrimitives.ReadUInt16LittleEndian(parent88[0x24..]));
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(40), cell.Identity);
        W32(48, BinaryPrimitives.ReadUInt32LittleEndian(parent88[0x14..]));
        W32(52, 0x4000000);
        bytes[56] = queryLength;
        bytes[57] = 0;
        bytes[58] = permission;
        bytes[60] = bytes[61] = 0;
        W32(64, currentHeader >> 9 & 15);
        W32(68, parentHeader >> 9 & 15);
        bytes[72] = (byte)(queryTag >> 56 & 1);
        bytes[73] = (byte)(related88[7] & 1);
        cell.Value = BinaryPrimitives.ReadInt32LittleEndian(parent88[0x10..]);
        cancellationToken.ThrowIfCancellationRequested();
        return new(bytes, cell);

        void W16(int offset, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset), value);
        void W32(int offset, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset), value);
    }
}
