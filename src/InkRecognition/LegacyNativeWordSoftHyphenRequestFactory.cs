using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>213C00 ordinary soft-hyphen packet immediately before F0.
/// Caller node/span fields are explicit; current arc0C and unknown residue
/// survive until F8 returns an actual successor. This stage does not score.
/// </summary>
public static class LegacyNativeWordSoftHyphenRequestFactory
{
    public static LegacyNativeWordRecombinationRequest Create(int currentEndingNode, byte currentSpan,
        int previousEndingNode, ReadOnlySpan<byte> actualParent88, uint preparedSourceHeader,
        ushort softHyphen24c2, ushort dash24c0, LegacyCursiveCategoryProfile categories,
        LegacyNativeWordTransitionCostCell cost, ReadOnlySpan<byte> callerRequest80,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(categories); ArgumentNullException.ThrowIfNull(cost);
        cancellationToken.ThrowIfCancellationRequested();
        if (currentEndingNode < 0 || previousEndingNode < 0 || currentSpan > 14 ||
            currentEndingNode - currentSpan - 1 != previousEndingNode || actualParent88.Length != 88 ||
            callerRequest80.Length != 80 || softHyphen24c2 == ushort.MaxValue ||
            dash24c0 >= categories.StartGeometryMap.Length)
            throw new ArgumentException("Actual adjacent source span, parent88, label maps and complete caller80 required.");
        int previousCategory = actualParent88[26] & 63;
        if ((uint)previousCategory >= categories.CompositeCategories.Length)
            throw new ArgumentException("The parent's actual sampler category must own its entry.");
        byte[] bytes = callerRequest80.ToArray();
        W32(bytes, 0, currentEndingNode);
        W32(bytes, 4, U16(actualParent88, 24));
        W32(bytes, 8, previousEndingNode);
        W16(bytes, 14, (ushort)(BinaryPrimitives.ReadUInt32LittleEndian(actualParent88[24..]) >> 22));
        W16(bytes, 16, U16(actualParent88, 28));
        bytes[18] = unchecked((byte)categories.StartGeometryMap.Span[dash24c0]);
        bytes[19] = checked((byte)categories.CompositeCategories.Span[previousCategory].GeometryIndex);
        W32(bytes, 20, 3);
        W32(bytes, 24, 0);
        bytes[28] = 0;
        W32(bytes, 32, currentSpan);
        W16(bytes, 36, softHyphen24c2);
        W16(bytes, 38, U16(actualParent88, 36));
        BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(40), cost.Identity);
        bytes[56] = 0;
        W32(bytes, 64, (int)(preparedSourceHeader >> 9 & 15));
        W32(bytes, 68, (int)(preparedSourceHeader >> 9 & 15));
        bytes[72] = 0;
        bytes[73] = (byte)(actualParent88[7] & 1);
        //No cost/48/0C write occurs before native F0/F8. The pointer at28
        //names the same cost cell; value10 and increment30 are set per arc.
        cancellationToken.ThrowIfCancellationRequested();
        return new(bytes, cost);
    }

    private static ushort U16(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(bytes[offset..]);
    private static void W16(Span<byte> bytes, int offset, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(bytes[offset..], value);
    private static void W32(Span<byte> bytes, int offset, int value) => BinaryPrimitives.WriteInt32LittleEndian(bytes[offset..], value);
}
