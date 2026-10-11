namespace InkRecognition;

/// <summary>Owned normalized source strokes before the native resampling pass.</summary>
public sealed class LegacyDirectionalCoordinates
{
    public IReadOnlyList<LegacyInkStroke> Strokes { get; }
    public int ScaleShift { get; }
    public int OriginalMinimumY { get; }
    public ReadOnlyMemory<int> CumulativePhysicalLengths { get; }
    public int PhysicalArcLength => CumulativePhysicalLengths.Span[^1];
    internal LegacyDirectionalCoordinates(LegacyInkStroke[] strokes, int shift, int minY, int[] cumulative)
    {
        Strokes = Array.AsReadOnly(strokes); ScaleShift = shift;
        OriginalMinimumY = minY; CumulativePhysicalLengths = cumulative;
    }
}

/// <summary>
/// The coordinate normalization portion of checked legacy pass 0b4d10. It uses
/// a power-of-two scale selected from the first largest physical segment and
/// the signed-short extent cap, rather than the 229-network's square raster.
/// Output remains source points: native resampling and prepared post-resampling
/// dimensions, baseline, flags and 313 channel assembly are separate.
/// </summary>
public static class LegacyDirectionalCoordinatePreparation
{
    public static LegacyDirectionalCoordinates Normalize(IReadOnlyList<LegacyInkStroke> strokes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(strokes);
        if (strokes.Count is < 1 or > 15) throw new ArgumentException("Expected 1..15 physical strokes.", nameof(strokes));
        cancellationToken.ThrowIfCancellationRequested();
        int minX = short.MaxValue, minY = short.MaxValue, maxX = short.MinValue, maxY = short.MinValue;
        int count = 0, largestSquare = 0; LegacyInkPoint largestStart = default, largestEnd = default;
        foreach (var stroke in strokes)
        {
            ArgumentNullException.ThrowIfNull(stroke); count += stroke.Points.Length;
            if (count > 4096) throw new ArgumentException("Source geometry exceeds 4096 points.", nameof(strokes));
            var points = stroke.Points;
            for (int i = 0; i < points.Length; i++)
            {
                var point = points[i]; minX = Math.Min(minX, point.X); minY = Math.Min(minY, point.Y);
                maxX = Math.Max(maxX, point.X); maxY = Math.Max(maxY, point.Y);
                if (i == 0) continue;
                long x = point.X - points[i - 1].X, y = point.Y - points[i - 1].Y;
                int square = (int)Math.Min(int.MaxValue, x * x + y * y);
                if (square > largestSquare) { largestSquare = square; largestStart = points[i - 1]; largestEnd = point; }
            }
        }
        int shift = 0;
        if (largestSquare > 0)
        {
            if (largestSquare < 16384)
            {
                int square = largestSquare;
                while (square * 4 < 16384) { square *= 4; shift++; }
            }
            else
            {
                // Shift the selected endpoints relative to the bounding origin;
                // shifting their difference would change odd-coordinate rounding.
                int ax = largestStart.X - minX, ay = largestStart.Y - minY;
                int bx = largestEnd.X - minX, by = largestEnd.Y - minY;
                long square;
                do
                {
                    ax >>= 1; ay >>= 1; bx >>= 1; by >>= 1; shift--;
                    long x = bx - ax, y = by - ay; square = x * x + y * y;
                } while (square >= 16384);
            }
            int extent = Math.Max(maxX - minX, maxY - minY), cap = 14;
            for (int half = extent >> 1; half > 0; half >>= 1) cap--;
            shift = Math.Min(shift, cap);
        }
        var normalized = new LegacyInkStroke[strokes.Count]; var cumulative = new int[count];
        int index = 0, total = 0;
        for (int s = 0; s < strokes.Count; s++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = strokes[s].Points; var points = new LegacyInkPoint[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                int x = Scale(source[i].X - minX, shift), y = Scale(source[i].Y - minY, shift);
                // Native's all-zero physical length branch retains shift0. Its
                // widely separated singletons can wrap short coordinates; reject
                // that unverified domain instead of presenting wrapped geometry.
                if (x > short.MaxValue || y > short.MaxValue)
                    throw new ArgumentException("Normalized coordinates exceed the checked nonnegative signed-short domain.", nameof(strokes));
                points[i] = new((short)x, (short)y);
                if (i != 0) total += Distance(points[i - 1], points[i]);
                cumulative[index++] = total;
            }
            normalized[s] = new(points);
        }
        return new(normalized, shift, minY, cumulative);
    }

    internal static int Scale(int value, int shift) => shift < 0 ? value >> -shift : value << shift;
    internal static int Distance(LegacyInkPoint a, LegacyInkPoint b)
    {
        int x = b.X - a.X, y = b.Y - a.Y;
        int square = x * x + y * y, shift = 0;
        if (square >= 16384)
        {
            shift = 2;
            while ((square >> (shift * 2)) > 16383) shift++;
        }
        // The checked 16384-entry native LUT equals round(sqrt(i)) for every i.
        return (int)(Math.Sqrt(square >> (shift * 2)) + .5) << shift;
    }
}
