namespace InkRecognition.Math;

// Default Engine service37. Numeric text primitives, not formula evaluation.
internal static class Math2DomStringService
{
    // V88/089DB0's bounded base10 branch. A successful prefix returns its end
    // offset; failure returns -1 and leaves the output unchanged.
    internal static int ParseDecimal(ReadOnlySpan<byte> input, byte flags, ref long value)
    {
        int at = 0;
        if ((flags & 1) != 0)
            while (at < input.Length && (input[at] is >= 9 and <= 13 or 32)) at++;
        bool negative = false;
        if (at < input.Length && input[at] is (byte)'+' or (byte)'-')
        {
            negative = input[at++] == (byte)'-';
            if ((flags & 4) != 0) return -1;
        }
        int first = at;
        ulong magnitude = 0;
        while (at < input.Length && input[at] is >= (byte)'0' and <= (byte)'9')
            magnitude = unchecked(magnitude * 10 + input[at++] - (byte)'0');
        if (at == first) return -1;
        value = unchecked((long)(negative ? 0UL - magnitude : magnitude));
        return at;
    }

    internal static int FormatSigned(long value, Span<byte> destination)
        => FormatMagnitude(value < 0 ? unchecked(0UL - (ulong)value) : (ulong)value, value < 0, destination);

    // B0/08AE70 restricted to the base10 used by Document0078D0.
    internal static int FormatUnsigned(ulong value, Span<byte> destination)
        => FormatMagnitude(value, false, destination);

    private static int FormatMagnitude(ulong magnitude, bool negative, Span<byte> destination)
    {
        ulong remaining = magnitude;
        int length = negative ? 1 : 0;
        do { length++; remaining /= 10; } while (remaining != 0);
        if (destination.Length <= length) return length;
        destination[length] = 0;
        int at = length;
        do
        {
            destination[--at] = (byte)('0' + magnitude % 10);
            magnitude /= 10;
        } while (magnitude != 0);
        if (negative) destination[0] = (byte)'-';
        return length;
    }

    internal static int FormatBoolean(bool value, Span<byte> destination)
    {
        ReadOnlySpan<byte> text = value ? "true\0"u8 : "false\0"u8;
        if (destination.Length >= text.Length) text.CopyTo(destination);
        return text.Length - 1;
    }
}
