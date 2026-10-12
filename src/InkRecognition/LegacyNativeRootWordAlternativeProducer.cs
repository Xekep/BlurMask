using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyNativeRootWordAlternatives(
    ReadOnlyMemory<byte> PrimaryStorage, ReadOnlyMemory<byte> SecondaryStorage);

/// <summary>Checked root(previousNode == -1) branch of nontracing20e6a0.
/// Uses actual slot08 context lineage and supplied ordinal58/storage. It does
/// not allocate ordinals, inherit stored parent alternatives or select a winner.</summary>
public static class LegacyNativeRootWordAlternativeProducer
{
    public static LegacyNativeRootWordAlternatives Create(ReadOnlySpan<byte> heapStorage,
        int activeCount, uint currentNode, LegacyNativeWordControllerConfiguration configuration,
        LegacySymbolContextFactory contexts, Func<ushort, ushort> mapSymbol2360,
        ReadOnlySpan<byte> primaryStorage, ReadOnlySpan<byte> secondaryStorage,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(mapSymbol2360);
        cancellationToken.ThrowIfCancellationRequested();
        if (configuration.Trace2368 != 0)
            throw new NotSupportedException("Trace640-byte clone branch is not covered.");
        ushort primaryCount = configuration.PrimaryAlternativeCount2402;
        ushort secondaryCount = configuration.SecondaryAlternativeCount2404;
        if (primaryCount is < 1 or > 16 || secondaryCount is < 1 or > 16)
            throw new NotSupportedException("Checked alternative counts are 1..16.");
        if (heapStorage.Length % 128 != 0 || activeCount < 0 || activeCount > heapStorage.Length / 128)
            throw new ArgumentException("Complete 128-byte heap storage and valid active count required.");
        int primaryRowBytes = primaryCount * 0x28;
        int secondaryRowBytes = secondaryCount * 0x28;
        if (primaryStorage.Length % primaryRowBytes != 0 || secondaryStorage.Length % secondaryRowBytes != 0)
            throw new ArgumentException("Complete alternative pool rows required.");

        if (primaryCount == 1 && secondaryCount == 1)
            return new(primaryStorage.ToArray(), secondaryStorage.ToArray());

        // Refuse unsupported parent branches before changing the shared index.
        for (int slotIndex = 0; slotIndex < activeCount; slotIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var slot = heapStorage.Slice(slotIndex * 128, 128);
            uint previousNode = unchecked(currentNode - (uint)(int)(sbyte)slot[0x20] - 1);
            if (previousNode != uint.MaxValue)
                throw new NotSupportedException("Stored parent alternative branch remains separate.");
            int ordinal = BinaryPrimitives.ReadInt32LittleEndian(slot[0x58..]);
            if (ordinal < 0 || primaryCount > 1 && ordinal >= primaryStorage.Length / primaryRowBytes ||
                secondaryCount > 1 && ordinal >= secondaryStorage.Length / secondaryRowBytes)
                throw new ArgumentException("Active native ordinal58 must address each enabled pool.");
            ulong suffix = BinaryPrimitives.ReadUInt64LittleEndian(slot[8..]);
            if (suffix != 0)
                _ = contexts.ReadNode(suffix);
        }

        byte[] primary = primaryStorage.ToArray();
        byte[] secondary = secondaryStorage.ToArray();
        for (int slotIndex = 0; slotIndex < activeCount; slotIndex++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var slot = heapStorage.Slice(slotIndex * 128, 128);
            int ordinal = BinaryPrimitives.ReadInt32LittleEndian(slot[0x58..]);
            ulong suffix = BinaryPrimitives.ReadUInt64LittleEndian(slot[8..]);
            ulong mapped = LegacyNativeWordContextComposition.CombineMapped(contexts, 0, suffix,
                mapSymbol2360, cancellationToken);
            ulong raw = LegacyNativeWordContextComposition.CombineRaw(contexts, 0, suffix, cancellationToken);
            if (primaryCount > 1)
                Fill(primary.AsSpan(ordinal * primaryRowBytes, primaryRowBytes), primaryCount, slot,
                    mapped, raw, primary: true);
            if (secondaryCount > 1)
                Fill(secondary.AsSpan(ordinal * secondaryRowBytes, secondaryRowBytes), secondaryCount, slot,
                    mapped, raw, primary: false);
        }
        return new(primary, secondary);
    }

    private static void Fill(Span<byte> row, ushort count, ReadOnlySpan<byte> slot,
        ulong mapped, ulong raw, bool primary)
    {
        var first = row[..0x28];
        slot.Slice(0x10, 8).CopyTo(first);
        BinaryPrimitives.WriteUInt64LittleEndian(first[8..], mapped);
        BinaryPrimitives.WriteUInt64LittleEndian(first[0x10..], raw);
        slot.Slice(0x1e, 2).CopyTo(first[0x18..]);
        first[0x1a] = slot[0x20];
        first[0x1e] = slot[0x26];
        first[0x1f] = (byte)((first[0x1f] & (primary ? 0xc0 : 0xe0)) |
            (slot[0x21] & 0x1f) | (primary ? 0x20 : 0));
        for (int index = 1; index < count; index++)
        {
            var unavailable = row.Slice(index * 0x28, 0x28);
            BinaryPrimitives.WriteInt32LittleEndian(unavailable, 0x4000000);
            BinaryPrimitives.WriteInt32LittleEndian(unavailable[4..], 0x4000000);
            unavailable[0x1e] = 0;
        }
    }
}
