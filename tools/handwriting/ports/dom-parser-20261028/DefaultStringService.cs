using System;

namespace DomParserPort;

// Bounded default Engine service37 kernels. No vendor code is executed.
public static class DefaultStringService
{
    // V88 / 180089DB0, restricted to the caller's base10 branch.
    // Returns -1 for native null, otherwise the first unconsumed byte offset.
    // Failure preserves the caller's output storage. NUL stops as an ordinary non-digit.
    public static int ParseDecimal(ReadOnlySpan<byte> input, byte flags, ref long value)
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

    // B8 / 18008AEF0: returned size excludes NUL; short buffers remain untouched.
    public static int FormatSigned(long value, Span<byte> destination)
    {
        ulong magnitude = value < 0 ? unchecked(0UL - (ulong)value) : (ulong)value;
        ulong remaining = magnitude;
        int length = value < 0 ? 1 : 0;
        do { length++; remaining /= 10; } while (remaining != 0);
        if (destination.Length <= length) return length;
        destination[length] = 0;
        int at = length;
        do
        {
            destination[--at] = (byte)('0' + magnitude % 10);
            magnitude /= 10;
        } while (magnitude != 0);
        if (value < 0) destination[0] = (byte)'-';
        return length;
    }

    // A8 / 18008AE00 (PE bytes; missing entry in the existing Ghidra project).
    public static int FormatBoolean(bool value, Span<byte> destination)
    {
        ReadOnlySpan<byte> text = value ? "true\0"u8 : "false\0"u8;
        if (destination.Length >= text.Length) text.CopyTo(destination);
        return text.Length - 1;
    }
}
