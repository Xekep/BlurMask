namespace InkRecognition.Math;

// Mode2's digit producer is still a separate proof obligation. Implementations
// must preserve signbit, round to the requested significant precision, strip
// trailing zeros, and return the native decimal position. No .NET G substitution.
internal interface IMath2DomDigitGenerator
{
    int Generate(double value, int precision, Span<byte> digits, out int decimalPosition, out bool negative);
}

internal sealed class Math2DomFloatingFormatter(IMath2DomDigitGenerator? generator = null) : IMath2DomFloatingFormatter
{
    internal static readonly Math2DomFloatingFormatter SpecialValuesOnly = new();

    public int Format(double value, Span<byte> destination, int precision, int nativeParameter)
    {
        if (nativeParameter != -1 || precision is not (9 or 17))
            throw new NotSupportedException("Only Document0078D0's floating caller is admitted.");
        if (destination.Length < 50)
            throw new ArgumentException("DOM floating conversion requires 50 bytes.", nameof(destination));

        ulong bits = unchecked((ulong)BitConverter.DoubleToInt64Bits(value));
        ulong magnitude = bits & 0x7fffffffffffffffUL;
        bool negative = (bits >> 63) != 0;
        // 08BC7C..08BDB7: extract sign before classifying zero/NaN/infinity.
        if (magnitude == 0)
            return Compose("0"u8, 1, negative, precision, destination);
        if ((magnitude >> 52) == 0x7ff)
            return Compose((magnitude & 0xfffffffffffffUL) == 0 ? "Infinity"u8 : "NaN"u8,
                9999, negative, precision, destination);

        if (generator is null)
            throw new NotSupportedException("Finite nonzero DOM dtoa digit generation is not yet connected.");
        Span<byte> digits = stackalloc byte[17];
        int count = generator.Generate(value, precision, digits[..precision], out int dp, out negative);
        if (count < 1 || count > precision)
            throw new NotSupportedException("Mode2 digit count exceeds the reviewed caller contract.");
        if (negative != ((bits >> 63) != 0) || dp == 9999)
            throw new NotSupportedException("Finite digit generation changed the input sign or classification.");
        return Compose(digits[..count], dp, negative, precision, destination);
    }

    // Engine08AFD0's first pass, parameter=-1, capacity>=50 and strlen<=precision.
    // This API consumes a checked helper tuple, not an IEEE value or owner token.
    internal static int Compose(ReadOnlySpan<byte> digits, int decimalPosition, bool negative,
        int precision, Span<byte> destination)
    {
        if (precision is not (9 or 17) || digits.Length < 1 || digits.Length > precision)
            throw new NotSupportedException("Unreviewed floating precision or digit count.");
        if (destination.Length < 50)
            throw new ArgumentException("DOM floating composition requires 50 bytes.", nameof(destination));
        if (digits.Overlaps(destination))
            throw new ArgumentException("Native dtoa scratch and destination are disjoint.", nameof(destination));
        if (decimalPosition == 9999)
        {
            if (!digits.SequenceEqual("Infinity"u8) && !digits.SequenceEqual("NaN"u8))
                throw new NotSupportedException("Unknown native special-value literal.");
        }
        else
        {
            if (decimalPosition is < -323 or > 309 || digits.IndexOfAnyExceptInRange((byte)'0', (byte)'9') >= 0
                || (digits[0] == '0' && (digits.Length != 1 || decimalPosition != 1)))
                throw new NotSupportedException("Invalid bounded mode2 digit tuple.");
        }

        int dp = decimalPosition, exponent = 0;
        // 08B080..08B0A3: notation depends on BOTH dp and strlen(digits).
        if (dp < 0 || digits.Length < dp)
        {
            exponent = dp - 1;
            if (exponent is >= -3 and <= 5) exponent = 0;
            else dp = 1;
        }

        // Finite maximum: sign1 + digits17 + point1 + e1 + exponent sign1
        // + exponent digits3 =24. Fixed form is at most23; special form<=15.
        // Thus capacity50 never reaches native retry or secondary rounding.
        int at = 0;
        if (negative) destination[at++] = (byte)'-';
        if (dp <= 0)
        {
            destination[at++] = (byte)'0'; destination[at++] = (byte)'.';
            destination.Slice(at, -dp).Fill((byte)'0'); at -= dp;
            digits.CopyTo(destination[at..]); at += digits.Length;
        }
        else if (dp < digits.Length)
        {
            digits[..dp].CopyTo(destination[at..]); at += dp;
            destination[at++] = (byte)'.';
            digits[dp..].CopyTo(destination[at..]); at += digits.Length - dp;
        }
        else
        {
            digits.CopyTo(destination[at..]); at += digits.Length;
            destination.Slice(at, dp - digits.Length).Fill((byte)'0'); at += dp - digits.Length;
        }
        if (exponent != 0)
        {
            destination[at++] = (byte)'e';
            at += Math2DomStringService.FormatSigned(exponent, destination[at..]);
        }
        destination[at] = 0;
        return at;
    }
}
