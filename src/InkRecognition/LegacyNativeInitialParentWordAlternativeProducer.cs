using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyNativeInitialParentWordAlternatives(
    ReadOnlyMemory<byte> PrimaryStorage, ReadOnlyMemory<byte> SecondaryStorage);

/// <summary>Nontracing20e6a0 stored-parent branch with slot1e below230a.
/// Exact parent rows, source token contexts and prior pool bytes are supplied;
/// the higher-arc/secondary-parent branches remain separate.</summary>
public static class LegacyNativeInitialParentWordAlternativeProducer
{
    public static bool ResetsPrimaryContext(ushort parentSourceToken)
        => parentSourceToken is 0xfff5 or 0xfff9 or 0xfffa or 0xfffb;

    public static LegacyNativeInitialParentWordAlternatives Create(ReadOnlySpan<byte> heapStorage,
        int activeCount, uint currentNode, LegacyNativeWordControllerConfiguration configuration,
        LegacyNativeWordParentRowLookup parentRowLookup, LegacySymbolContextFactory contexts,
        Func<ushort, ushort> mapSymbol2360, ReadOnlySpan<byte> primaryStorage,
        ReadOnlySpan<byte> secondaryStorage, CancellationToken cancellationToken = default)
        => CreateCore(heapStorage, activeCount, currentNode, configuration, parentRowLookup, contexts, mapSymbol2360, primaryStorage, secondaryStorage, cancellationToken, null, null);

    // Only the dispatcher's private, already-owned pools may use this path.
    internal static void UpdateOwned(ReadOnlySpan<byte> heapStorage,
        int activeCount, uint currentNode, LegacyNativeWordControllerConfiguration configuration,
        LegacyNativeWordParentRowLookup parentRowLookup, LegacySymbolContextFactory contexts,
        Func<ushort, ushort> mapSymbol2360, byte[] primaryStorage,
        byte[] secondaryStorage, CancellationToken cancellationToken = default)
        => _ = CreateCore(heapStorage, activeCount, currentNode, configuration, parentRowLookup, contexts, mapSymbol2360, primaryStorage, secondaryStorage, cancellationToken, primaryStorage, secondaryStorage);

    private static LegacyNativeInitialParentWordAlternatives CreateCore(ReadOnlySpan<byte> heapStorage,
        int activeCount, uint currentNode, LegacyNativeWordControllerConfiguration configuration,
        LegacyNativeWordParentRowLookup parentRowLookup, LegacySymbolContextFactory contexts,
        Func<ushort, ushort> mapSymbol2360, ReadOnlySpan<byte> primaryStorage,
        ReadOnlySpan<byte> secondaryStorage, CancellationToken cancellationToken, byte[]? ownedPrimary, byte[]? ownedSecondary)
    {
        ArgumentNullException.ThrowIfNull(parentRowLookup);
        ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(mapSymbol2360);
        cancellationToken.ThrowIfCancellationRequested();
        if (configuration.Trace2368 != 0)
            throw new NotSupportedException("Trace640-byte copying remains separate.");
        ushort pc = configuration.PrimaryAlternativeCount2402, sc = configuration.SecondaryAlternativeCount2404;
        if (pc is < 1 or > 16 || sc is < 1 or > 16)
            throw new NotSupportedException("Checked alternative counts are 1..16.");
        if (heapStorage.Length % 128 != 0 || heapStorage.Length / 128 > 4096 ||
            (uint)activeCount > (uint)(heapStorage.Length / 128) ||
            primaryStorage.Length % (pc * 40) != 0 || secondaryStorage.Length % (sc * 40) != 0)
            throw new ArgumentException("Complete bounded heap and alternative row storage required.");
        if (pc == 1 && sc == 1)
            return new((ownedPrimary ?? primaryStorage.ToArray()), (ownedSecondary ?? secondaryStorage.ToArray()));
        for (int index = 0; index < activeCount; index++)
        {
            var slot = heapStorage.Slice(index * 128, 128);
            uint previous = unchecked(currentNode - (uint)(int)(sbyte)slot[0x20] - 1);
            if (previous > int.MaxValue || BinaryPrimitives.ReadUInt16LittleEndian(slot[0x1e..]) >= configuration.PrimaryArcLimit230a)
                throw new NotSupportedException("Root or higher-arc secondary-parent branch is not covered.");
            int ordinal = BinaryPrimitives.ReadInt32LittleEndian(slot[0x58..]);
            if (ordinal < 0 || pc > 1 && ordinal >= primaryStorage.Length / (pc * 40) ||
                sc > 1 && ordinal >= secondaryStorage.Length / (sc * 40))
                throw new ArgumentException("Ordinal58 must cover each enabled candidate pool.");
        }
        byte[] primary = (ownedPrimary ?? primaryStorage.ToArray()), secondary = (ownedSecondary ?? secondaryStorage.ToArray());
        for (int index = 0; index < activeCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var slot = heapStorage.Slice(index * 128, 128);
            int previous = (int)unchecked(currentNode - (uint)(int)(sbyte)slot[0x20] - 1);
            ushort arc = BinaryPrimitives.ReadUInt16LittleEndian(slot[0x1e..]);
            var parent = parentRowLookup(previous, arc)
                ?? throw new ArgumentException("Actual stored parent is required.");
            cancellationToken.ThrowIfCancellationRequested();
            if (parent.Record.Length != 0x58 || pc > 1 && parent.PrimaryAlternatives.Length != pc * 40 ||
                sc > 1 && parent.SecondaryAlternatives.Length != sc * 40)
                throw new ArgumentException("Parent's configured stored88/pool blocks are required.");
            var parentRecord = parent.Record.ToArray();
            byte[] parentPrimary = parent.PrimaryAlternatives.ToArray(), parentSecondary = parent.SecondaryAlternatives.ToArray();
            ulong parentSource = BinaryPrimitives.ReadUInt64LittleEndian(parentRecord.AsSpan(8));
            if (parentSource == 0)
                throw new ArgumentException("Parent source08 needs its actual symbol node; native dereferences node20.");
            bool reset = ResetsPrimaryContext(contexts.ReadNode(parentSource).Symbol);
            bool append = BinaryPrimitives.ReadUInt16LittleEndian(slot[0x18..]) == currentNode || (sbyte)slot[5] >= 0;
            ulong suffix = BinaryPrimitives.ReadUInt64LittleEndian(slot[8..]);
            if (suffix != 0) _ = contexts.ReadNode(suffix);
            int ordinal = BinaryPrimitives.ReadInt32LittleEndian(slot[0x58..]);
            int delta = unchecked(I32(slot, 0x10) - I32(parentRecord, 0x10));
            int incrementDelta = unchecked(I32(slot, 0x14) - I32(parentRecord, 0x14));

            if (pc > 1)
            {
                var row = primary.AsSpan(ordinal * pc * 40, pc * 40);
                Header(row, slot, primary: true);
                row[0x1b] &= 0x80;
                if (reset)
                {
                    var first = row[..40];
                    BinaryPrimitives.WriteInt32LittleEndian(first, delta);
                    BinaryPrimitives.WriteInt32LittleEndian(first[4..], incrementDelta);
                    BinaryPrimitives.WriteUInt64LittleEndian(first[8..],
                        LegacyNativeWordContextComposition.CombineMapped(contexts, 0, suffix, mapSymbol2360, cancellationToken));
                    BinaryPrimitives.WriteUInt64LittleEndian(first[0x10..],
                        LegacyNativeWordContextComposition.CombineRaw(contexts, 0, suffix, cancellationToken));
                    first[0x1e] = slot[0x26];
                    first[0x1f] |= 0x20;
                    for (int k = 1; k < pc; k++) Invalidate(row.Slice(k * 40, 40));
                }
                else
                    for (int k = 0; k < pc; k++)
                        Inherit(row.Slice(k * 40, 40), parentPrimary.AsSpan(k * 40, 40), slot,
                            delta, incrementDelta, k, primary: true, append, contexts, suffix, mapSymbol2360, cancellationToken);
            }
            if (sc > 1)
            {
                var row = secondary.AsSpan(ordinal * sc * 40, sc * 40);
                Header(row, slot, primary: false);
                row[0x1b] &= 0x80;
                var firstParent = parentSecondary.AsSpan(0, 40);
                ulong mapped = BinaryPrimitives.ReadUInt64LittleEndian(firstParent[8..]);
                ulong raw = BinaryPrimitives.ReadUInt64LittleEndian(firstParent[0x10..]);
                if (reset || append)
                {
                    mapped = reset ? LegacyNativeWordContextComposition.CombineRaw(contexts, mapped, suffix, cancellationToken)
                        : LegacyNativeWordContextComposition.CombineMapped(contexts, mapped, suffix, mapSymbol2360, cancellationToken);
                    raw = LegacyNativeWordContextComposition.CombineRaw(contexts, raw, suffix, cancellationToken);
                }
                BinaryPrimitives.WriteUInt64LittleEndian(row[8..], mapped);
                BinaryPrimitives.WriteUInt64LittleEndian(row[0x10..], raw);
                for (int k = 1; k < sc; k++)
                    Inherit(row.Slice(k * 40, 40), parentSecondary.AsSpan(k * 40, 40), slot,
                        delta, incrementDelta, k, primary: false, append, contexts, suffix, mapSymbol2360, cancellationToken);
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(primary, secondary);
    }

    private static void Header(Span<byte> row, ReadOnlySpan<byte> slot, bool primary)
    {
        slot.Slice(0x1e, 2).CopyTo(row[0x18..]);
        row[0x1a] = slot[0x20];
        row[0x1f] = (byte)((row[0x1f] & (primary ? 0xc0 : 0xe0)) | (slot[0x21] & 0x1f));
        if (!primary)
        {
            slot.Slice(0x10, 8).CopyTo(row);
            row[0x1e] = slot[0x26];
        }
    }

    private static void Inherit(Span<byte> output, ReadOnlySpan<byte> source, ReadOnlySpan<byte> slot,
        int delta, int incrementDelta, int index, bool primary, bool append,
        LegacySymbolContextFactory contexts, ulong suffix, Func<ushort, ushort> map, CancellationToken token)
    {
        if (I32(source, 0) >= 0x4000000)
        {
            Invalidate(output);
            return; // Unlike211eb0, a later available parent alternative is still processed.
        }
        BinaryPrimitives.WriteInt32LittleEndian(output, unchecked(I32(source, 0) + delta));
        BinaryPrimitives.WriteInt32LittleEndian(output[4..], unchecked(I32(source, 4) + incrementDelta));
        slot.Slice(0x1e, 2).CopyTo(output[0x18..]);
        output[0x1a] = slot[0x20];
        output[0x1b] = (byte)((output[0x1b] & 0x80) | (index & 0x7f));
        output[0x1e] = primary ? slot[0x26] : (byte)0;
        ulong mapped = BinaryPrimitives.ReadUInt64LittleEndian(source[8..]);
        ulong raw = BinaryPrimitives.ReadUInt64LittleEndian(source[0x10..]);
        if (append)
        {
            mapped = LegacyNativeWordContextComposition.CombineMapped(contexts, mapped, suffix, map, token);
            raw = LegacyNativeWordContextComposition.CombineRaw(contexts, raw, suffix, token);
        }
        BinaryPrimitives.WriteUInt64LittleEndian(output[8..], mapped);
        BinaryPrimitives.WriteUInt64LittleEndian(output[0x10..], raw);
        output[0x1f] = (byte)((output[0x1f] & (primary ? 0xc0 : 0xe0)) | (slot[0x21] & 0x1f));
    }

    private static void Invalidate(Span<byte> output)
    {
        BinaryPrimitives.WriteInt32LittleEndian(output, 0x4000000);
        BinaryPrimitives.WriteInt32LittleEndian(output[4..], 0x4000000);
        output[0x1e] = 0;
    }

    private static int I32(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadInt32LittleEndian(value[offset..]);
}
