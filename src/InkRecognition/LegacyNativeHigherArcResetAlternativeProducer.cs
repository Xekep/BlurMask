using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Explicit scalar dependencies of20e6a0's higher-arc reset branch.
/// Numeric suffixes identify native fields; their policy is not inferred.</summary>
public readonly record struct LegacyNativeHigherArcResetParameters(ushort SpecialLabel24ea, uint ResetSource22ec);

/// <summary>Nontracing20e6a0 higher-arc reset-source branch. The other higher
/// arc path requires its native118/140/F0/148/100 callbacks and remains separate.</summary>
public static class LegacyNativeHigherArcResetAlternativeProducer
{
    public static bool IsResetSource(byte mode22f6, uint sourceDescriptorHeader, ushort label24, ushort specialLabel24ea)
        => mode22f6 == 0 && (sourceDescriptorHeader & 0x1c) > 7 && label24 != specialLabel24ea;

    public static LegacyNativeInitialParentWordAlternatives Create(ReadOnlySpan<byte> heapStorage,
        int activeCount, uint currentNode, LegacyNativeWordControllerConfiguration configuration,
        LegacyNativeHigherArcResetParameters parameters, Func<byte, uint> sourceDescriptorHeaderLookup,
        LegacyNativeWordParentRowLookup parentRowLookup, LegacySymbolContextFactory contexts,
        Func<ushort, ushort> mapSymbol2360, ReadOnlySpan<byte> primaryStorage,
        ReadOnlySpan<byte> secondaryStorage, CancellationToken cancellationToken = default)
        => CreateCore(heapStorage, activeCount, currentNode, configuration, parameters, sourceDescriptorHeaderLookup, parentRowLookup, contexts, mapSymbol2360, primaryStorage, secondaryStorage, cancellationToken, null, null);

    // Only the dispatcher's private, already-owned pools may use this path.
    internal static void UpdateOwned(ReadOnlySpan<byte> heapStorage,
        int activeCount, uint currentNode, LegacyNativeWordControllerConfiguration configuration,
        LegacyNativeHigherArcResetParameters parameters, Func<byte, uint> sourceDescriptorHeaderLookup,
        LegacyNativeWordParentRowLookup parentRowLookup, LegacySymbolContextFactory contexts,
        Func<ushort, ushort> mapSymbol2360, byte[] primaryStorage,
        byte[] secondaryStorage, CancellationToken cancellationToken = default)
        => _ = CreateCore(heapStorage, activeCount, currentNode, configuration, parameters, sourceDescriptorHeaderLookup, parentRowLookup, contexts, mapSymbol2360, primaryStorage, secondaryStorage, cancellationToken, primaryStorage, secondaryStorage);

    private static LegacyNativeInitialParentWordAlternatives CreateCore(ReadOnlySpan<byte> heapStorage,
        int activeCount, uint currentNode, LegacyNativeWordControllerConfiguration configuration,
        LegacyNativeHigherArcResetParameters parameters, Func<byte, uint> sourceDescriptorHeaderLookup,
        LegacyNativeWordParentRowLookup parentRowLookup, LegacySymbolContextFactory contexts,
        Func<ushort, ushort> mapSymbol2360, ReadOnlySpan<byte> primaryStorage,
        ReadOnlySpan<byte> secondaryStorage, CancellationToken cancellationToken, byte[]? ownedPrimary, byte[]? ownedSecondary)
    {
        ArgumentNullException.ThrowIfNull(sourceDescriptorHeaderLookup);
        ArgumentNullException.ThrowIfNull(parentRowLookup);
        ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(mapSymbol2360);
        cancellationToken.ThrowIfCancellationRequested();
        if (configuration.Trace2368 != 0) throw new NotSupportedException("Trace640-byte copying remains separate.");
        ushort pc = configuration.PrimaryAlternativeCount2402, sc = configuration.SecondaryAlternativeCount2404;
        if (pc is < 1 or > 16 || sc is < 1 or > 16 || heapStorage.Length % 128 != 0 || heapStorage.Length / 128 > 4096 ||
            (uint)activeCount > (uint)(heapStorage.Length / 128) || primaryStorage.Length % (pc * 40) != 0 ||
            secondaryStorage.Length % (sc * 40) != 0)
            throw new ArgumentException("Complete bounded heap and alternative row storage required.");
        if (pc == 1 && sc == 1) return new((ownedPrimary ?? primaryStorage.ToArray()), (ownedSecondary ?? secondaryStorage.ToArray()));
        for (int index = 0; index < activeCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var slot = heapStorage.Slice(index * 128, 128);
            uint previous = unchecked(currentNode - (uint)(int)(sbyte)slot[0x20] - 1);
            ushort parentArc = BinaryPrimitives.ReadUInt16LittleEndian(slot[0x1e..]);
            int ordinal = BinaryPrimitives.ReadInt32LittleEndian(slot[0x58..]);
            if (previous > int.MaxValue || parentArc < configuration.PrimaryArcLimit230a)
                throw new NotSupportedException("Root and primary-parent scopes have separate producers.");
            if (ordinal < 0 || pc > 1 && ordinal >= primaryStorage.Length / (pc * 40) ||
                sc > 1 && ordinal >= secondaryStorage.Length / (sc * 40))
                throw new ArgumentException("Actual ordinal58 must address enabled pools.");
        }
        byte[] primary = (ownedPrimary ?? primaryStorage.ToArray()), secondary = (ownedSecondary ?? secondaryStorage.ToArray());
        for (int index = 0; index < activeCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var slot = heapStorage.Slice(index * 128, 128);
            uint header = sourceDescriptorHeaderLookup(slot[4]);
            cancellationToken.ThrowIfCancellationRequested();
            if (!IsResetSource(configuration.Mode22f6, header,
                BinaryPrimitives.ReadUInt16LittleEndian(slot[0x24..]), parameters.SpecialLabel24ea))
                throw new NotSupportedException("Higher-arc callback/recombination scope remains separate.");
            int previous = (int)unchecked(currentNode - (uint)(int)(sbyte)slot[0x20] - 1);
            ushort parentArc = BinaryPrimitives.ReadUInt16LittleEndian(slot[0x1e..]);
            var parent = parentRowLookup(previous, parentArc)
                ?? throw new ArgumentException("Actual owned higher-arc parent row required.");
            cancellationToken.ThrowIfCancellationRequested();
            if (parent.Record.Length != 0x58 || sc > 1 && parent.SecondaryAlternatives.Length != sc * 40)
                throw new ArgumentException("Parent requires stored88 and its configured secondary alternatives.");
            byte[] parentRecord = parent.Record.ToArray(), parentSecondary = parent.SecondaryAlternatives.ToArray();
            int ordinal = BinaryPrimitives.ReadInt32LittleEndian(slot[0x58..]);
            ulong suffix = BinaryPrimitives.ReadUInt64LittleEndian(slot[8..]);
            if (pc > 1)
            {
                var row = primary.AsSpan(ordinal * pc * 40, pc * 40);
                Header(row, slot, primary: true);
                row[0x1b] &= 0x80;
                BinaryPrimitives.WriteInt32LittleEndian(row, unchecked(I32(slot, 0x10) - I32(parentRecord, 0x10)));
                BinaryPrimitives.WriteInt32LittleEndian(row[4..], unchecked(I32(slot, 0x14) - I32(parentRecord, 0x14)));
                BinaryPrimitives.WriteUInt64LittleEndian(row[8..], LegacyNativeWordContextComposition.CombineMapped(
                    contexts, 0, suffix, mapSymbol2360, cancellationToken));
                BinaryPrimitives.WriteUInt64LittleEndian(row[0x10..], LegacyNativeWordContextComposition.CombineRaw(
                    contexts, 0, suffix, cancellationToken));
                byte bit5 = slot[4] == parameters.ResetSource22ec ? (byte)((slot[5] & 1) << 5) : (byte)0x20;
                row[0x1f] = (byte)((row[0x1f] & 0xdf) | bit5);
                for (int k = 1; k < pc; k++) Invalidate(row.Slice(k * 40, 40));
            }
            if (sc > 1)
            {
                var row = secondary.AsSpan(ordinal * sc * 40, sc * 40);
                Header(row, slot, primary: false);
                row[0x1b] &= 0x80;
                BinaryPrimitives.WriteUInt64LittleEndian(row[8..], LegacyNativeWordContextComposition.CombineRaw(contexts,
                    BinaryPrimitives.ReadUInt64LittleEndian(parentSecondary.AsSpan(8)), suffix, cancellationToken));
                BinaryPrimitives.WriteUInt64LittleEndian(row[0x10..], LegacyNativeWordContextComposition.CombineRaw(contexts,
                    BinaryPrimitives.ReadUInt64LittleEndian(parentSecondary.AsSpan(0x10)), suffix, cancellationToken));
                for (int k = 1; k < sc; k++) Invalidate(row.Slice(k * 40, 40));
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(primary, secondary);
    }

    private static void Header(Span<byte> row, ReadOnlySpan<byte> slot, bool primary)
    {
        slot.Slice(0x1e, 2).CopyTo(row[0x18..]);
        row[0x1a] = slot[0x20];
        row[0x1e] = slot[0x26];
        row[0x1f] = (byte)((row[0x1f] & (primary ? 0xc0 : 0xe0)) | (slot[0x21] & 0x1f));
        if (!primary) slot.Slice(0x10, 8).CopyTo(row);
    }
    private static void Invalidate(Span<byte> value)
    {
        BinaryPrimitives.WriteInt32LittleEndian(value, 0x4000000);
        BinaryPrimitives.WriteInt32LittleEndian(value[4..], 0x4000000);
        value[0x1e] = 0;
    }
    private static int I32(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadInt32LittleEndian(value[offset..]);
}
