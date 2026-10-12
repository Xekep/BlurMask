using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyNativeSecondaryRecombinationProjectionResult(ReadOnlyMemory<byte> HeapSlot,
    ReadOnlyMemory<byte> PrimaryStorage, ReadOnlyMemory<byte> SecondaryStorage);

/// <summary>
/// Nontracing 20e6a0 consumer following an accepted secondary recombination.
/// Copies best fields to the heap and refreshes contexts using actual chosen
/// parent alternatives. Eligibility and incoming cost production are separate.
/// </summary>
public static class LegacyNativeSecondaryRecombinationProjection
{
    public static LegacyNativeSecondaryRecombinationProjectionResult Project(ReadOnlySpan<byte> heapSlot,
        uint currentNode, LegacyNativeWordControllerConfiguration configuration,
        ReadOnlySpan<byte> primaryStorage, ReadOnlySpan<byte> mergedSecondaryStorage,
        LegacyNativeWordParentRowLookup parentRowLookup, LegacySymbolContextFactory contexts,
        Func<ushort, ushort> mapSymbol2360, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parentRowLookup);
        ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(mapSymbol2360);
        cancellationToken.ThrowIfCancellationRequested();
        if (configuration.Trace2368 != 0)
            throw new NotSupportedException("Native tracing side tables are not supplied.");
        ushort pc = configuration.PrimaryAlternativeCount2402, sc = configuration.SecondaryAlternativeCount2404;
        if (pc is < 1 or > 20 || sc is < 2 or > 20 || heapSlot.Length != 128 ||
            primaryStorage.Length % 40 != 0 || mergedSecondaryStorage.Length % 40 != 0 ||
            pc > 1 && primaryStorage.Length < pc * 40 || mergedSecondaryStorage.Length < sc * 40)
            throw new ArgumentException("Actual single heap slot and configured enabled secondary blocks required.");
        uint previous = unchecked(currentNode - (uint)(int)(sbyte)heapSlot[32] - 1);
        if (previous > int.MaxValue)
            throw new NotSupportedException("Actual nonnegative stored parent node is required for context refresh.");
        var slot = heapSlot.ToArray();
        var primary = primaryStorage.ToArray();
        var secondary = mergedSecondaryStorage.ToArray();
        var best = secondary.AsSpan(0, 40);
        BinaryPrimitives.WriteInt32LittleEndian(slot.AsSpan(16), I32(best, 0));
        BinaryPrimitives.WriteInt32LittleEndian(slot.AsSpan(20), I32(best, 4));
        slot[38] = best[30];
        BinaryPrimitives.WriteUInt16LittleEndian(slot.AsSpan(30), U16(best, 24));
        if (pc > 1) primary[27] = (byte)((primary[27] & 0x80) | (best[27] & 0x7f));

        ulong suffix = U64(slot, 8);
        for (int index = 0; index < sc; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var alternative = secondary.AsSpan(index * 40, 40);
            if (I32(alternative, 0) >= 0x4000000) continue;
            ushort parentArc = U16(alternative, 24);
            int parentAlternative = alternative[27] & 0x7f;
            if (parentAlternative >= sc)
                throw new ArgumentException("Chosen native parent alternative must fit its configured secondary block.");
            var parent = parentRowLookup((int)previous, parentArc);
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(parent);
            if (parent.Record.Length != 88 || parent.SecondaryAlternatives.Length != sc * 40)
                throw new ArgumentException("Actual owning stored parent and configured secondary block required.");
            var source = parent.SecondaryAlternatives.Span.Slice(parentAlternative * 40, 40);
            ulong mapped = U64(source, 8), raw = U64(source, 16);
            mapped = LegacyNativeWordContextComposition.CombineMapped(contexts, mapped, suffix, mapSymbol2360, cancellationToken);
            raw = LegacyNativeWordContextComposition.CombineRaw(contexts, raw, suffix, cancellationToken);
            alternative[31] = (byte)((alternative[31] & 0xe0) | (slot[33] & 31));
            BinaryPrimitives.WriteUInt64LittleEndian(alternative[16..], raw);
            BinaryPrimitives.WriteUInt64LittleEndian(alternative[8..], mapped);
            alternative[26] = slot[32];
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(slot, primary, secondary);
    }

    private static int I32(ReadOnlySpan<byte> source, int offset) => BinaryPrimitives.ReadInt32LittleEndian(source[offset..]);
    private static ushort U16(ReadOnlySpan<byte> source, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(source[offset..]);
    private static ulong U64(ReadOnlySpan<byte> source, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(source[offset..]);
}
