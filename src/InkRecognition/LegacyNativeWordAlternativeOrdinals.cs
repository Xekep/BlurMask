using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyNativeWordOrdinalAssignment(ReadOnlyMemory<byte> SlotStorage, uint NextOrdinal);

/// <summary>Literal2144f0 slot58 ordinal loops. Physical heap/slot order is
/// supplied by the controller; this writer neither sorts nor allocates pools.</summary>
public static class LegacyNativeWordAlternativeOrdinals
{
    public static LegacyNativeWordOrdinalAssignment AssignInitial(ReadOnlySpan<byte> slotStorage,
        int activeCount, uint firstOrdinal = 0, CancellationToken cancellationToken = default)
        => Assign(slotStorage, activeCount, firstOrdinal, continuation: false, cancellationToken);

    public static LegacyNativeWordOrdinalAssignment AssignContinuation(ReadOnlySpan<byte> slotStorage,
        int activeCount, ushort firstOrdinal = 0, CancellationToken cancellationToken = default)
        => Assign(slotStorage, activeCount, firstOrdinal, continuation: true, cancellationToken);

    private static LegacyNativeWordOrdinalAssignment Assign(ReadOnlySpan<byte> slotStorage,
        int activeCount, uint firstOrdinal, bool continuation, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (slotStorage.Length % 128 != 0 || slotStorage.Length / 128 > 4096 ||
            (uint)activeCount > (uint)(slotStorage.Length / 128))
            throw new ArgumentException("Owned complete heap storage with valid active count required.");
        var storage = slotStorage.ToArray();
        uint next = firstOrdinal;
        for (int index = 0; index < activeCount; index++)
        {
            token.ThrowIfCancellationRequested();
            BinaryPrimitives.WriteUInt32LittleEndian(storage.AsSpan(index * 128 + 0x58), next);
            next = continuation ? unchecked((ushort)(next + 1)) : unchecked(next + 1);
        }
        return new(storage, next);
    }
}
