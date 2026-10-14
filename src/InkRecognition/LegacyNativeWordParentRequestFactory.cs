using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>
/// Literal regular201270 parent-row to80-byte transition/acoustic request.
/// This initializer differs from20e6a0: both FirstId fields are native zero,
/// the previous ending/source nodes differ, and30 inherits the parent increment.
/// Query/context enumeration, callback execution and allocation are consumers.
/// </summary>
public static class LegacyNativeWordParentRequestFactory
{
    public static LegacyNativeWordRecombinationRequest Create(uint endingNode, uint sourceSpan,
        int previousEndingNode, ReadOnlySpan<byte> storedParent88, ushort returnedSourceArc,
        ushort currentSymbol, int compositeCategory, ulong queryTag, byte queryLength, byte queryFlags,
        uint preparedCurrentHeader, uint preparedParentHeader, byte permission2309,
        LegacyCursiveCategoryProfile categories, LegacyNativeWordTransitionCostCell costCell,
        int queryBaseCost, ReadOnlySpan<byte> stackResidue80, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(categories);
        ArgumentNullException.ThrowIfNull(costCell);
        cancellationToken.ThrowIfCancellationRequested();
        if (endingNode > int.MaxValue || previousEndingNode < 0 || storedParent88.Length != 88 ||
            stackResidue80.Length != 80 || (uint)compositeCategory >= (uint)categories.CompositeCategories.Length)
            throw new ArgumentException("Bounded actual current/previous nodes, complete parent88 and request80 required.");
        int previousCategory = storedParent88[0x1a] & 63;
        if ((uint)previousCategory >= (uint)categories.CompositeCategories.Length)
            throw new ArgumentException("Actual parent's composite category lies outside the prepared sampler table.");
        var bytes = stackResidue80.ToArray();
        Write32(bytes, 0, endingNode);
        Write32(bytes, 4, BinaryPrimitives.ReadUInt16LittleEndian(storedParent88[0x18..]));
        Write32(bytes, 8, (uint)previousEndingNode);
        Write16(bytes, 12, returnedSourceArc);
        Write16(bytes, 14, (ushort)(BinaryPrimitives.ReadUInt32LittleEndian(storedParent88[0x18..]) >> 22));
        Write16(bytes, 16, BinaryPrimitives.ReadUInt16LittleEndian(storedParent88[0x1c..]));
        bytes[18] = checked((byte)categories.CompositeCategories.Span[compositeCategory].GeometryIndex);
        bytes[19] = checked((byte)categories.CompositeCategories.Span[previousCategory].GeometryIndex);
        Write32(bytes, 20, 0);
        Write32(bytes, 24, 0);
        bytes[28] = (storedParent88[6] & 1) != 0 ? (byte)7 : (byte)(queryFlags & 7);
        Write32(bytes, 32, sourceSpan);
        Write16(bytes, 36, currentSymbol);
        Write16(bytes, 38, BinaryPrimitives.ReadUInt16LittleEndian(storedParent88[0x24..]));
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(40), costCell.Identity);
        Write32(bytes, 48, BinaryPrimitives.ReadUInt32LittleEndian(storedParent88[0x14..]));
        Write32(bytes, 52, 0x4000000);
        bytes[56] = queryLength;
        bytes[57] = 0;
        bytes[58] = permission2309;
        bytes[60] = 0;
        Write32(bytes, 64, preparedCurrentHeader >> 9 & 15);
        Write32(bytes, 68, preparedParentHeader >> 9 & 15);
        bytes[72] = (byte)(queryTag >> 56 & 1);
        bytes[73] = (byte)(storedParent88[7] & 1);
        costCell.Value = unchecked(queryBaseCost + BinaryPrimitives.ReadInt32LittleEndian(storedParent88[0x10..]));
        cancellationToken.ThrowIfCancellationRequested();
        return new(bytes, costCell);
    }

    private static void Write16(Span<byte> bytes, int at, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(bytes[at..], value);
    private static void Write32(Span<byte> bytes, int at, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(bytes[at..], value);
}
