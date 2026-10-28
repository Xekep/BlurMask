using System.Numerics;

namespace InkRecognition.Math;

// Independent integer implementation of the reviewed mode2, nearest-even
// significant-digit contract. No floating arithmetic, locale or native runtime.
internal sealed class Math2DomExactDigits : IMath2DomDigitGenerator
{
    internal static readonly Math2DomExactDigits Instance = new();

    public int Generate(double value, int precision, Span<byte> digits, out int decimalPosition, out bool negative)
    {
        if (precision is not (9 or 17) || digits.Length < precision)
            throw new ArgumentException("Mode2 requires room for 9 or 17 significant digits.");
        ulong bits = unchecked((ulong)BitConverter.DoubleToInt64Bits(value));
        negative = (bits >> 63) != 0;
        int exponent = (int)((bits >> 52) & 0x7ff);
        ulong significand = bits & 0xfffffffffffffUL;
        if (exponent == 0x7ff || (exponent == 0 && significand == 0))
            throw new NotSupportedException("The exact finite generator excludes zero and special values.");
        int binaryPower;
        if (exponent == 0) binaryPower = -1074;
        else { significand |= 1UL << 52; binaryPower = exponent - 1075; }

        // Exact integer values up to UInt64 need neither BigInteger nor any
        // floating arithmetic. The same significant-digit rounding applies.
        if (TryInteger(significand, binaryPower, out ulong integer))
            return IntegerDigits(integer, precision, digits, out decimalPosition);

        BigInteger numerator = significand, denominator = BigInteger.One;
        if (binaryPower >= 0) numerator <<= binaryPower;
        else denominator <<= -binaryPower;

        // Integer approximation is only a seed; exact rational comparisons close
        // both sides, including subnormals and every decimal-power boundary.
        int binaryFloor = BitOperations.Log2(significand) + binaryPower;
        int decimalFloor = (binaryFloor * 78913) >> 18;
        while (ComparePower(numerator, denominator, decimalFloor) < 0) decimalFloor--;
        while (ComparePower(numerator, denominator, decimalFloor + 1) >= 0) decimalFloor++;
        decimalPosition = decimalFloor + 1;
        int scale = precision - decimalPosition;
        if (scale >= 0) numerator *= BigInteger.Pow(10, scale);
        else denominator *= BigInteger.Pow(10, -scale);

        BigInteger quotient = BigInteger.DivRem(numerator, denominator, out var remainder);
        int comparison = (remainder << 1).CompareTo(denominator);
        if (comparison > 0 || (comparison == 0 && !quotient.IsEven)) quotient++;
        ulong number = (ulong)quotient;
        ulong limit = precision == 17 ? 100000000000000000UL : 1000000000UL;
        if (number == limit) { number /= 10; decimalPosition++; }
        return WriteDigits(number, digits);
    }

    private static int ComparePower(BigInteger numerator, BigInteger denominator, int power)
        => power >= 0 ? numerator.CompareTo(denominator * BigInteger.Pow(10, power))
            : (numerator * BigInteger.Pow(10, -power)).CompareTo(denominator);

    private static bool TryInteger(ulong significand, int binaryPower, out ulong value)
    {
        value = 0;
        if (binaryPower >= 0)
        {
            if (binaryPower > 63 - BitOperations.Log2(significand)) return false;
            value = significand << binaryPower;
        }
        else
        {
            int shift = -binaryPower;
            if (shift >= 64 || (significand & ((1UL << shift) - 1)) != 0) return false;
            value = significand >> shift;
        }
        return true;
    }

    private static int IntegerDigits(ulong number, int precision, Span<byte> digits, out int dp)
    {
        dp = 0;
        for (ulong remaining = number; remaining != 0; remaining /= 10) dp++;
        if (dp > precision)
        {
            ulong divisor = 1;
            for (int i = precision; i < dp; i++) divisor *= 10;
            ulong quotient = number / divisor, remainder = number % divisor;
            if (remainder * 2 > divisor || (remainder * 2 == divisor && (quotient & 1) != 0)) quotient++;
            ulong limit = precision == 17 ? 100000000000000000UL : 1000000000UL;
            if (quotient == limit) { quotient /= 10; dp++; }
            number = quotient;
        }
        return WriteDigits(number, digits);
    }

    private static int WriteDigits(ulong number, Span<byte> digits)
    {
        while (number % 10 == 0) number /= 10;
        int count = 0;
        for (ulong remaining = number; remaining != 0; remaining /= 10) count++;
        for (int at = count - 1; at >= 0; at--)
        {
            digits[at] = (byte)('0' + number % 10);
            number /= 10;
        }
        return count;
    }
}
