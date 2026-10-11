namespace InkRecognition;

/// <summary>An integer crossing location produced by the checked legacy helper.</summary>
public readonly record struct LegacyIntersectionPoint(int X, int Y);

/// <summary>
/// Checked exponent-10 segment-crossing helper (0b5fa0). Fixed-point slopes,
/// signed division, dominant-axis inclusion and parallel overlap locations are
/// preserved; this is intentionally not a general exact geometric predicate.
/// The directional intersection-classification pass remains separate.
/// </summary>
public static class LegacyQuantizedSegmentIntersection
{
    public static bool TryIntersect(LegacyInkPoint a0, LegacyInkPoint a1,
        LegacyInkPoint b0, LegacyInkPoint b1, out LegacyIntersectionPoint intersection)
    {
        intersection = default;
        OrderByY(ref a0, ref a1); OrderByY(ref b0, ref b1);
        if (b0.Y < a0.Y) { (a0, b0) = (b0, a0); (a1, b1) = (b1, a1); }
        int dxA = a1.X - a0.X, dxB = b1.X - b0.X;
        int dyA = a1.Y - a0.Y, dyB = b1.Y - b0.Y;
        int slopeA = dxA == 0 ? int.MaxValue : (dyA << 10) / dxA;
        int slopeB = dxB == 0 ? int.MaxValue : (dyB << 10) / dxB;
        int offsetA = dxA == 0 ? int.MaxValue : unchecked((a0.Y << 10) - slopeA * a0.X);
        int offsetB = dxB == 0 ? int.MaxValue : unchecked((b0.Y << 10) - slopeB * b0.X);
        if (dxA == 0 && dxB == 0)
        {
            if (a0.X != b0.X || b0.Y > a1.Y) return false;
            intersection = new(a0.X, (a1.Y + b0.Y) / 2); return true;
        }
        if (slopeA == slopeB)
        {
            if (offsetA != offsetB) return false;
            int minA = Math.Min(a0.X, a1.X), maxA = Math.Max(a0.X, a1.X);
            int minB = Math.Min(b0.X, b1.X), maxB = Math.Max(b0.X, b1.X);
            bool overlap = slopeA < 1025
                ? minA < minB ? minB <= maxA : minA <= maxB
                : b0.Y <= a1.Y;
            if (!overlap) return false;
            intersection = new((a1.X + b0.X) / 2, (a1.Y + b0.Y) / 2); return true;
        }
        int x;
        if (dxA == 0) x = a0.X;
        else if (dxB == 0) x = b0.X;
        else
        {
            int numerator = unchecked(offsetB - offsetA), denominator = slopeA - slopeB;
            // Native signed division would fault in this overflow domain. Reject
            // the unsupported arithmetic instead of allowing a process crash.
            if (numerator == int.MinValue && denominator == -1) return false;
            x = numerator / denominator;
        }
        int y = dxA == 0 ? unchecked(slopeB * x + 512 + offsetB) >> 10
            : unchecked(slopeA * x + 512 + offsetA) >> 10;
        if (!Inside(a0, a1, x, y) || !Inside(b0, b1, x, y)) return false;
        intersection = new(x, y); return true;
    }

    private static void OrderByY(ref LegacyInkPoint first, ref LegacyInkPoint last)
    {
        if (first.Y > last.Y) (first, last) = (last, first);
    }
    private static bool Inside(LegacyInkPoint low, LegacyInkPoint high, int x, int y)
        => Math.Abs(high.X - low.X) < high.Y - low.Y
            ? low.Y <= y && y <= high.Y
            : Math.Min(low.X, high.X) <= x && x <= Math.Max(low.X, high.X);
}
