using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Bounded port of Text1801b95a0: ordinal category bounds clear both acoustic masks.</summary>
public static class LegacyAcousticSymbolRangeFilter
{
    /// <summary>
    /// If either bounds table is absent (empty), native filtering is disabled.
    /// Zero bounds are unconstrained; equality is allowed. This API does not
    /// infer script, case or the upstream meaning of the current category.
    /// Returns an original diagnostic count of mask positions cleared.
    /// </summary>
    public static int Apply(Span<byte> primary, Span<byte> secondary,
        ReadOnlySpan<byte> minimumCategories, ReadOnlySpan<byte> maximumCategories,
        byte currentCategory, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (primary.Length != secondary.Length || primary.Length > ushort.MaxValue)
            throw new ArgumentException("Acoustic masks must have the same bounded symbol count.");
        if (minimumCategories.IsEmpty || maximumCategories.IsEmpty) return 0;
        if (minimumCategories.Length != primary.Length || maximumCategories.Length != primary.Length)
            throw new ArgumentException("Bounds must cover the acoustic symbol alphabet.");
        if (minimumCategories.Overlaps(primary) || minimumCategories.Overlaps(secondary) ||
            maximumCategories.Overlaps(primary) || maximumCategories.Overlaps(secondary))
            throw new ArgumentException("Bounds must not alias mutable masks.");
        int cleared = 0;
        for (int symbol = 0; symbol < primary.Length; symbol++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (primary[symbol] == 0 && secondary[symbol] == 0) continue;
            byte minimum = minimumCategories[symbol], maximum = maximumCategories[symbol];
            if ((minimum != 0 && currentCategory < minimum) || (maximum != 0 && maximum < currentCategory))
            { primary[symbol] = secondary[symbol] = 0; cleared++; }
        }
        return cleared;
    }

    /// <summary>
    /// Extract explicit native unsigned packed fields from little-endian uint32
    /// storage, including fields crossing a word boundary. The supported category
    /// branch has 1..8 bits per field; serialized object headers are not guessed.
    /// </summary>
    public static byte[] DecodePackedFields(ReadOnlySpan<byte> words, int bitWidth, int count)
    {
        if (bitWidth is < 1 or > 8 || count is < 0 or > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(bitWidth), "Expected 1..8-bit fields and bounded symbol count.");
        int requiredWords = (count * bitWidth + 31) / 32;
        if (words.Length != requiredWords * 4) throw new InvalidDataException("Packed category storage length differs.");
        var result = new byte[count];
        uint mask = (1u << bitWidth) - 1;
        for (int symbol = 0; symbol < count; symbol++)
        {
            int bit = symbol * bitWidth, word = bit / 32, shift = bit % 32;
            uint value = BinaryPrimitives.ReadUInt32LittleEndian(words[(word * 4)..]) >> shift;
            if (32 - shift < bitWidth)
                value |= BinaryPrimitives.ReadUInt32LittleEndian(words[((word + 1) * 4)..]) << (32 - shift);
            result[symbol] = (byte)(value & mask);
        }
        return result;
    }
}
