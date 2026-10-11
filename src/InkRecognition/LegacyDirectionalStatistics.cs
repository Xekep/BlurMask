namespace InkRecognition;

/// <summary>
/// The 17 global integer statistics of the checked legacy 313 feature family.
/// Input must already have native coordinate preparation; width, height and
/// physical arc length are supplied rather than guessed. Intersection flags
/// (0..3) and their aggregate count are also caller-supplied. This restores one
/// arithmetic pass, not the full 313-channel extractor or a cursive segmenter.
/// </summary>
public static class LegacyDirectionalStatistics
{
    public const int Width = 17;

    public static int[] Extract(IReadOnlyList<LegacyInkStroke> preparedStrokes,
        int geometryWidth, int geometryHeight, int physicalArcLength,
        ReadOnlySpan<byte> directionFlags, ReadOnlySpan<byte> intersectionFlags,
        int intersectionCount, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preparedStrokes);
        if (preparedStrokes.Count is < 1 or > 15) throw new ArgumentException("Expected 1..15 prepared physical strokes.", nameof(preparedStrokes));
        if (geometryWidth is < 1 or > 32768 || geometryHeight is < 1 or > 32768)
            throw new ArgumentException("Prepared geometry dimensions must be 1..32768.");
        if (physicalArcLength < 0 || intersectionCount < 0) throw new ArgumentException("Lengths and counts must be nonnegative.");
        cancellationToken.ThrowIfCancellationRequested();
        int points = 0;
        foreach (var stroke in preparedStrokes)
        {
            ArgumentNullException.ThrowIfNull(stroke); points += stroke.Points.Length;
            if (points > 4096) throw new ArgumentException("Prepared geometry exceeds 4096 points.", nameof(preparedStrokes));
        }
        if (directionFlags.Length != points || intersectionFlags.Length != points)
            throw new ArgumentException("Both flag arrays must exactly match the prepared point count.");

        var result = new int[Width]; int sumX = 0, sumY = 0, index = 0;
        foreach (var stroke in preparedStrokes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var point in stroke.Points)
            {
                if (point.X < 0 || point.Y < 0 || point.X > geometryWidth || point.Y > geometryHeight)
                    throw new ArgumentException("Coordinates must be prepared nonnegative points within the supplied dimensions.", nameof(preparedStrokes));
                byte direction = directionFlags[index], intersection = intersectionFlags[index++];
                if ((direction & ~0xdb) != 0 || intersection > 3) throw new ArgumentException("Unsupported landmark/intersection flag bits.");
                sumX += point.X; sumY += point.Y;
                switch (direction & 7) { case 1: result[10]++; break; case 2: result[9]++; break; }
                switch ((direction >> 3) & 7) { case 1: result[12]++; break; case 2: result[11]++; break; }
                switch (intersection) { case 1: result[16]++; break; case 2: result[13]++; break; case 3: result[14]++; break; }
            }
        }
        int dimensionSum = geometryWidth + geometryHeight;
        result[0] = (geometryWidth << 10) / dimensionSum;
        result[1] = (geometryHeight << 10) / dimensionSum;
        result[2] = preparedStrokes.Count;
        // Native reduction wraps its exponent-10 numerator in signed int32.
        result[3] = unchecked(physicalArcLength << 10) / dimensionSum;
        int meanX = sumX / points, meanY = sumY / points;
        result[4] = (meanX << 10) / geometryWidth;
        result[5] = (meanY << 10) / geometryHeight;
        result[16] = (result[16] << 10) / points;

        int xx = 0, yy = 0, xy = 0, shift = 0;
        foreach (var stroke in preparedStrokes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var point in stroke.Points)
            {
                int x = point.X - meanX, y = point.Y - meanY;
                xx += (x * x) >> shift; yy += (y * y) >> shift; xy += (x * y) >> shift;
                while (xx >= 0x200000 || yy >= 0x200000)
                {
                    xx >>= 1; yy >>= 1; xy >>= 1; shift++;
                }
            }
        }
        int variance = xx + yy;
        if (variance > 0)
        {
            result[6] = (xx << 10) / variance;
            result[7] = (yy << 10) / variance;
            result[8] = (xy << 10) / variance;
        }
        result[15] = intersectionCount;
        return result;
    }
}
