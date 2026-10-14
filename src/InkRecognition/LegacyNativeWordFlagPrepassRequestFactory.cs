using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Exact2136D0 ordinary flag-prepass80-byte packet. This is distinct
///from normal201270 and20E6A0; the actual virtual118/140/128 consumer follows.</summary>
public static class LegacyNativeWordFlagPrepassRequestFactory
{
    public static LegacyNativeWordRecombinationRequest Create(int currentEndingNode, byte currentSpan,
        int previousEndingNode, ReadOnlySpan<byte> actualParent88, ushort returnedCurrentArc,
        uint preparedParentSourceHeader, LegacyCursiveCategoryProfile categories,
        LegacyNativeWordTransitionCostCell cost, ReadOnlySpan<byte> existingRequestResidue80,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(categories);
        ArgumentNullException.ThrowIfNull(cost);
        cancellationToken.ThrowIfCancellationRequested();
        if (currentEndingNode < 0 || previousEndingNode < 0 || currentSpan > 14 ||
            currentEndingNode - currentSpan - 1 != previousEndingNode || actualParent88.Length != 88 ||
            existingRequestResidue80.Length != 80)
            throw new ArgumentException("Actual adjacent source span, parent88 and reused80 packet required.");
        int previousCategory = actualParent88[26] & 63;
        if ((uint)previousCategory >= categories.CompositeCategories.Length)
            throw new ArgumentException("Actual parent's composite category must own its prepared sampler entry.");
        int blankSymbol = categories.BaseEntryCount;
        byte[] bytes = existingRequestResidue80.ToArray();
        W32(bytes, 0, currentEndingNode);
        W32(bytes, 4, U16(actualParent88, 24));
        W32(bytes, 8, previousEndingNode);
        W16(bytes, 12, returnedCurrentArc);
        W16(bytes, 14, (ushort)(BinaryPrimitives.ReadUInt32LittleEndian(actualParent88[24..]) >> 22));
        W16(bytes, 16, U16(actualParent88, 28));
        bytes[18] = unchecked((byte)categories.StartGeometryMap.Span[blankSymbol]);
        bytes[19] = checked((byte)categories.CompositeCategories.Span[previousCategory].GeometryIndex);
        W32(bytes, 20, 6);
        W32(bytes, 24, 0);
        bytes[28] = 5;
        W32(bytes, 32, currentSpan);
        W16(bytes, 36, checked((ushort)blankSymbol));
        W16(bytes, 38, U16(actualParent88, 36));
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(40), cost.Identity);
        W32(bytes, 48, BinaryPrimitives.ReadInt32LittleEndian(actualParent88[20..]));
        //2136D0 preserves the reused native request's limit34 and untouched
        //residue fields. Caller2144F0 initializes limit34=IMP before this pass.
        bytes[56] = 0;
        bytes[72] = 0;
        bytes[73] = (byte)(actualParent88[7] & 1);
        W32(bytes, 64, (int)(preparedParentSourceHeader >> 9 & 15));
        W32(bytes, 68, (int)(preparedParentSourceHeader >> 9 & 15));
        cost.Value = BinaryPrimitives.ReadInt32LittleEndian(actualParent88[16..]);
        cancellationToken.ThrowIfCancellationRequested();
        return new(bytes, cost);
    }

    private static ushort U16(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(bytes[offset..]);
    private static void W16(Span<byte> bytes, int offset, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(bytes[offset..], value);
    private static void W32(Span<byte> bytes, int offset, int value) => BinaryPrimitives.WriteInt32LittleEndian(bytes[offset..], value);
}
