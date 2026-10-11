namespace InkRecognition;

/// <summary>
/// Experimental 128-square legacy raster profile: seven shape statistics followed
/// by twelve 16-bin projection/contour banks. Not an image OCR or word decoder.
/// </summary>
public static class LegacyRasterFeatures
{
    public const int GeometryWidth = 199;
    public const int IsolatedInputWidth = 229;

    public static short[] Extract(IReadOnlyList<LegacyInkStroke> strokes, CancellationToken cancellationToken = default)
    {
        var ink = LegacyGlyphGeometry.Create(strokes, cancellationToken);
        var result = new short[GeometryWidth];
        LegacyStrokeFeatures.Compose(ink.SampleX, ink.SampleY, ink.PenState, strokes.Count, ink.Width, ink.Height)
            .AsSpan(210, 7).CopyTo(result);
        var pixels = Rasterize(ink);
        Project(pixels).CopyTo(result, 7);
        cancellationToken.ThrowIfCancellationRequested();
        return result;
    }

    /// <summary>Append empty prior context and complete-stroke boundaries for the 229-input profile.</summary>
    public static short[] ExtractIsolatedInput(IReadOnlyList<LegacyInkStroke> strokes, CancellationToken cancellationToken = default)
    {
        var result = new short[IsolatedInputWidth];
        Extract(strokes, cancellationToken).CopyTo(result, 0);
        result.AsSpan(GeometryWidth, 28).Fill(-1024); result[^2] = 1024; result[^1] = 1024;
        return result;
    }

    /// <summary>The second raster profile stores statistics, baseline, then raster channels.</summary>
    public static short[] ExtractWithBaseline(IReadOnlyList<LegacyInkStroke> strokes, LegacyInkBaseline baseline,
        CancellationToken cancellationToken = default)
    {
        var ink = LegacyGlyphGeometry.Create(strokes, cancellationToken);
        var result = new short[208]; var pixels = Rasterize(ink);
        LegacyStrokeFeatures.Compose(ink.SampleX, ink.SampleY, ink.PenState, strokes.Count, ink.Width, ink.Height)
            .AsSpan(210, 7).CopyTo(result);
        LegacyBaselineFeatures.Extract(ink, baseline, pixels).CopyTo(result, 7);
        Project(pixels).CopyTo(result, 16);
        cancellationToken.ThrowIfCancellationRequested(); return result;
    }

    public static short[] ExtractIsolatedInputWithBaseline(IReadOnlyList<LegacyInkStroke> strokes, LegacyInkBaseline baseline,
        CancellationToken cancellationToken = default)
    {
        var result = new short[238];
        ExtractWithBaseline(strokes, baseline, cancellationToken).CopyTo(result, 0);
        result.AsSpan(208, 28).Fill(-1024); result[^2] = 1024; result[^1] = 1024; return result;
    }

    internal static bool[] Rasterize(LegacyGlyphGeometry ink)
    {
        var pixels = new bool[128 * 128]; int offset = 0;
        for (int group = 0; group < ink.Counts.Length; group++)
        {
            int count = ink.Counts[group];
            if ((group & 1) == 0)
            {
                if (count == 1)
                {
                    int x = ink.SampleX[offset], y = ink.SampleY[offset];
                    x = x == 127 ? x - 2 : x > 0 ? x - 1 : 0;
                    y = y == 127 ? y - 2 : y > 0 ? y - 1 : 0;
                    for (int dy = 0; dy < 3; dy++) for (int dx = 0; dx < 3; dx++) Mark(pixels, x + dx, y + dy);
                }
                else
                    for (int i = offset; i < offset + count - 1; i++)
                        Line(pixels, ink.SampleX[i], ink.SampleY[i], ink.SampleX[i + 1], ink.SampleY[i + 1]);
            }
            offset += count;
        }
        return pixels;
    }

    private static void Mark(bool[] pixels, int x, int y)
    {
        if ((uint)x >= 128 || (uint)y >= 128) throw new ArgumentException("A normalized raster sample lies outside its grid.");
        pixels[y * 128 + x] = true;
    }

    private static bool Has(bool[] pixels, int x, int y) => (uint)x < 128 && (uint)y < 128 && pixels[y * 128 + x];

    // Native integer Bresenham: left-to-right orientation and diagonal tie on error >= 0.
    private static void Line(bool[] pixels, int ax, int ay, int bx, int by)
    {
        if (ax > bx) { (ax, bx) = (bx, ax); (ay, by) = (by, ay); }
        int dx = bx - ax, dy = Math.Abs(by - ay), yStep = by < ay ? -1 : 1;
        bool steep = dy > dx;
        int major = Math.Max(dx, dy), minor = Math.Min(dx, dy), error = 2 * minor - major;
        for (int i = 0; i <= major; i++)
        {
            Mark(pixels, ax, ay);
            if (error < 0)
            {
                if (steep) ay += yStep; else ax++;
                error += 2 * minor;
            }
            else
            {
                ax++; ay += yStep; error += 2 * (minor - major);
            }
        }
    }

    internal static short[] Project(bool[] pixels)
    {
        var rowEnds = new int[128]; var columnEnds = new int[128];
        var diagonalEnds = new int[255]; var antiDiagonalEnds = new int[255];
        var left = Enumerable.Repeat(128, 128).ToArray(); var top = Enumerable.Repeat(128, 128).ToArray();
        var right = Enumerable.Repeat(128, 128).ToArray(); var bottom = Enumerable.Repeat(128, 128).ToArray();
        var diagonalY = new int[255]; var diagonalX = new int[255];
        var antiDiagonalLow = new int[255]; var antiDiagonalHigh = new int[255];
        for (int i = 0; i < 255; i++)
            diagonalY[i] = diagonalX[i] = antiDiagonalLow[i] = antiDiagonalHigh[i] = i < 128 ? i + 1 : 255 - i;
        for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
        {
            if (!Has(pixels, x, y)) continue;
            bool r = Has(pixels, x + 1, y), up = Has(pixels, x, y - 1), down = Has(pixels, x, y + 1);
            bool l = Has(pixels, x - 1, y), sw = Has(pixels, x - 1, y + 1), se = Has(pixels, x + 1, y + 1);
            if (!r) rowEnds[y]++; if (!up) columnEnds[x]++;
            int diagonal = x + y, anti = x - y + 127;
            if (!sw) diagonalEnds[diagonal]++;
            else if (!down && !l)
            {
                int extra = anti - 1;
                antiDiagonalEnds[extra]++;
                antiDiagonalLow[extra] = Math.Min(antiDiagonalLow[extra], Math.Min(x - 1, y));
                antiDiagonalHigh[extra] = Math.Min(antiDiagonalHigh[extra], y + 1 < x ? 127 - x : 126 - y);
            }
            if (!se) antiDiagonalEnds[anti]++;
            else if (!down && !r)
            {
                int extra = diagonal + 1;
                diagonalEnds[extra]++;
                diagonalY[extra] = Math.Min(diagonalY[extra], extra < 128 ? y : 126 - x);
                diagonalX[extra] = Math.Min(diagonalX[extra], extra < 128 ? x : 126 - y);
            }
            left[y] = Math.Min(left[y], x); top[x] = Math.Min(top[x], y);
            right[y] = Math.Min(right[y], 127 - x); bottom[x] = Math.Min(bottom[x], 127 - y);
            diagonalY[diagonal] = Math.Min(diagonalY[diagonal], diagonal < 128 ? y : 127 - x);
            diagonalX[diagonal] = Math.Min(diagonalX[diagonal], diagonal < 128 ? x : 127 - y);
            antiDiagonalLow[anti] = Math.Min(antiDiagonalLow[anti], Math.Min(x, y));
            antiDiagonalHigh[anti] = Math.Min(antiDiagonalHigh[anti], 127 - Math.Max(x, y));
        }
        var result = new short[192];
        for (int bin = 0; bin < 16; bin++)
        {
            int row = 0, column = 0, sumLeft = 0, sumTop = 0, sumRight = 0, sumBottom = 0;
            for (int i = bin * 8; i < bin * 8 + 8; i++)
            {
                row += rowEnds[i]; column += columnEnds[i];
                sumLeft += left[i]; sumTop += top[i]; sumRight += right[i]; sumBottom += bottom[i];
            }
            result[bin] = (short)Math.Clamp(row * 64 - 1024, -1024, 1024);
            result[32 + bin] = (short)Math.Clamp(column * 64 - 1024, -1024, 1024);
            result[64 + bin] = (short)(sumLeft * 2 - 1024); result[96 + bin] = (short)(sumTop * 2 - 1024);
            result[128 + bin] = (short)(sumRight * 2 - 1024); result[160 + bin] = (short)(sumBottom * 2 - 1024);
            int start = bin * 16 - (bin < 8 ? 0 : 1);
            int diag = 0, antiDiag = 0, sumDY = 0, sumDX = 0, sumAL = 0, sumAH = 0;
            for (int i = start; i < start + 16; i++)
            {
                int cells = bin < 8 ? i + 1 : 255 - i;
                diag += diagonalEnds[i]; antiDiag += antiDiagonalEnds[i];
                sumDY += (diagonalY[i] << 8) / cells; sumDX += (diagonalX[i] << 8) / cells;
                sumAL += (antiDiagonalLow[i] << 8) / cells; sumAH += (antiDiagonalHigh[i] << 8) / cells;
            }
            result[16 + bin] = (short)Math.Clamp(diag * 32 - 1024, -1024, 1024);
            result[48 + bin] = (short)Math.Clamp(antiDiag * 32 - 1024, -1024, 1024);
            result[80 + bin] = (short)((sumDY * 128 >> 8) - 1024);
            result[112 + bin] = (short)((sumAL * 128 >> 8) - 1024);
            result[144 + bin] = (short)((sumDX * 128 >> 8) - 1024);
            result[176 + bin] = (short)((sumAH * 128 >> 8) - 1024);
        }
        return result;
    }
}
