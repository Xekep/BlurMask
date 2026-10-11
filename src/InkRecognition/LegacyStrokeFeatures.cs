namespace InkRecognition;

/// <summary>An already quantized legacy ink coordinate. Y increases downward.</summary>
public readonly record struct LegacyInkPoint(short X, short Y);

/// <summary>
/// Experimental single- and multi-stroke geometry for the legacy 30-sample, exponent-10
/// trajectory profile. This is not a word recognizer or a cursive segmenter.
/// </summary>
public static class LegacyStrokeFeatures
{
    public const int SampleCount = 30;
    public const int GeometryWidth = 217;
    public const int IsolatedInputWidth = 247;
    private const int Unit = 1024;

    /// <summary>
    /// Extract 210 trajectory channels and 7 shape statistics. Input must describe
    /// one complete physical stroke. Translation is removed by normalization.
    /// </summary>
    public static short[] Extract(ReadOnlySpan<LegacyInkPoint> stroke, CancellationToken cancellationToken = default)
    {
        if (stroke.Length is < 2 or > 4096)
            throw new ArgumentException("A single stroke of 2..4096 quantized points is required.", nameof(stroke));
        cancellationToken.ThrowIfCancellationRequested();
        int minX = short.MaxValue, minY = short.MaxValue, maxX = short.MinValue, maxY = short.MinValue;
        foreach (var p in stroke)
        {
            minX = Math.Min(minX, p.X); minY = Math.Min(minY, p.Y);
            maxX = Math.Max(maxX, p.X); maxY = Math.Max(maxY, p.Y);
        }
        int width = maxX - minX, height = maxY - minY, extent = Math.Max(width, height);
        if (extent == 0) throw new ArgumentException("The stroke has no spatial extent.", nameof(stroke));
        int scale = 130048 / extent;
        int normalizedWidth = width * scale >> 10, normalizedHeight = height * scale >> 10;
        int center = (128 - Math.Min(normalizedWidth, normalizedHeight) - 1) >> 1;
        var x = new int[stroke.Length]; var y = new int[stroke.Length];
        for (int i = 0; i < stroke.Length; i++)
        {
            x[i] = ((stroke[i].X - minX) * scale >> 10) + (width < height ? center : 0);
            y[i] = ((stroke[i].Y - minY) * scale >> 10) + (height < width ? center : 0);
        }
        var lengths = new int[stroke.Length - 1]; int total = 0;
        for (int i = 0; i < lengths.Length; i++)
        {
            lengths[i] = Distance(x[i], y[i], x[i + 1], y[i + 1]); total += lengths[i];
        }
        var sx = new int[SampleCount]; var sy = new int[SampleCount];
        Resample(x, y, lengths, total, sx, sy);
        cancellationToken.ThrowIfCancellationRequested();

        return Compose(sx, sy, Enumerable.Repeat((short)Unit, SampleCount).ToArray(), 1, normalizedWidth, normalizedHeight);
    }

    /// <summary>Extract the same profile for a complete glyph of 1..15 physical strokes.</summary>
    public static short[] Extract(IReadOnlyList<LegacyInkStroke> strokes, CancellationToken cancellationToken = default)
    {
        var ink = LegacyGlyphGeometry.Create(strokes, cancellationToken);
        return Compose(ink.SampleX, ink.SampleY, ink.PenState, strokes.Count, ink.Width, ink.Height);
    }

    internal static short[] Compose(int[] sx, int[] sy, short[] pen, int strokeCount, int normalizedWidth, int normalizedHeight)
    {
        var dx = new int[SampleCount - 1]; var dy = new int[SampleCount - 1];
        var distances = new int[SampleCount - 1]; int sampledLength = 0;
        for (int i = 0; i < dx.Length; i++)
        {
            dx[i] = (sx[i + 1] - sx[i]) * 16; dy[i] = (sy[i + 1] - sy[i]) * 16;
            distances[i] = Distance(sx[i], sy[i], sx[i + 1], sy[i + 1]); sampledLength += distances[i];
        }
        var result = new short[GeometryWidth]; var direction = new int[6];
        for (int i = 0; i < dx.Length; i++)
        {
            if (pen[i] != Unit || (i + 1 < dx.Length && pen[i + 1] != Unit)) continue;
            AddDirection(direction, 0, dx[i], distances[i]); AddDirection(direction, 2, dy[i], distances[i]);
            if (i + 1 < dx.Length)
                AddDirection(direction, 4, (dx[i] * dy[i + 1] - dy[i] * dx[i + 1]) >> 8,
                    distances[i] + distances[i + 1]);
        }
        for (int i = 0; i < 6; i += 2)
        {
            int sum = direction[i] + direction[i + 1];
            result[i] = (short)(sum == 0 ? -Unit : 2 * Unit * direction[i] / sum - Unit);
            result[i + 1] = (short)(sum == 0 ? -Unit : 2 * Unit * direction[i + 1] / sum - Unit);
        }
        int sumX = 0, sumY = 0, physicalPoints = 0;
        for (int i = 0; i < SampleCount; i++)
        {
            result[6 + i] = pen[i];
            result[6 + SampleCount + i] = (short)(16 * sx[i] - Unit);
            result[6 + 2 * SampleCount + i] = (short)(16 * sy[i] - Unit);
            if (pen[i] == Unit) { sumX += sx[i]; sumY += sy[i]; physicalPoints++; }
        }
        for (int i = 0; i < dx.Length; i++)
        {
            result[6 + 3 * SampleCount + i] = (short)(distances[i] == 0 ? 0 : Unit * dx[i] / distances[i]);
            result[5 + 4 * SampleCount + i] = (short)(distances[i] == 0 ? 0 : Unit * dy[i] / distances[i]);
        }
        for (int i = 0; i < SampleCount - 2; i++)
        {
            int ax = result[6 + 3 * SampleCount + i], bx = result[7 + 3 * SampleCount + i];
            int ay = result[5 + 4 * SampleCount + i], by = result[6 + 4 * SampleCount + i];
            result[4 + 5 * SampleCount + i] = (short)((ax * bx + ay * by) >> 10);
            result[2 + 6 * SampleCount + i] = (short)((ax * by - bx * ay) >> 10);
        }
        int physicalLength = 0;
        for (int i = 0; i < distances.Length; i++)
            if (pen[i] == Unit && pen[i + 1] == Unit) physicalLength += distances[i];
        result[210] = Unit;
        result[211] = (short)Math.Min(Unit, 256 * (normalizedHeight + 1) / (normalizedWidth + 1) - Unit);
        result[212] = (short)(16 * sumX / physicalPoints - Unit);
        result[213] = (short)(16 * sumY / physicalPoints - Unit);
        result[214] = (short)(256 * strokeCount - Unit);
        result[215] = (short)Math.Clamp(physicalLength / 8 - Unit, -Unit, Unit);
        result[216] = (short)(sampledLength == 0 ? -Unit : 2 * Unit * physicalLength / sampledLength - Unit);
        return result;
    }

    /// <summary>
    /// Assemble the first trajectory classifier's 247 channels for an isolated
    /// complete glyph with no prior candidates. The 28 context values are -1024,
    /// and the two stroke-boundary values are +1024. Do not use this for word segments.
    /// </summary>
    public static short[] ExtractIsolatedInput(ReadOnlySpan<LegacyInkPoint> stroke, CancellationToken cancellationToken = default)
    {
        var geometry = Extract(stroke, cancellationToken); var input = new short[IsolatedInputWidth];
        geometry.CopyTo(input, 0);
        input.AsSpan(GeometryWidth, 28).Fill(-Unit);
        input[^2] = Unit; input[^1] = Unit;
        return input;
    }

    /// <summary>Assemble isolated context for a complete multi-stroke glyph, without a word decoder.</summary>
    public static short[] ExtractIsolatedInput(IReadOnlyList<LegacyInkStroke> strokes, CancellationToken cancellationToken = default)
    {
        var geometry = Extract(strokes, cancellationToken); var input = new short[IsolatedInputWidth];
        geometry.CopyTo(input, 0); input.AsSpan(GeometryWidth, 28).Fill(-Unit);
        input[^2] = Unit; input[^1] = Unit; return input;
    }

    /// <summary>Append nine caller-supplied baseline channels for the second trajectory profile.</summary>
    public static short[] ExtractWithBaseline(IReadOnlyList<LegacyInkStroke> strokes, LegacyInkBaseline baseline,
        CancellationToken cancellationToken = default)
    {
        var ink = LegacyGlyphGeometry.Create(strokes, cancellationToken);
        var result = new short[226];
        Compose(ink.SampleX, ink.SampleY, ink.PenState, strokes.Count, ink.Width, ink.Height).CopyTo(result, 0);
        LegacyBaselineFeatures.Extract(ink, baseline, LegacyRasterFeatures.Rasterize(ink)).CopyTo(result, GeometryWidth);
        cancellationToken.ThrowIfCancellationRequested(); return result;
    }

    public static short[] ExtractIsolatedInputWithBaseline(IReadOnlyList<LegacyInkStroke> strokes, LegacyInkBaseline baseline,
        CancellationToken cancellationToken = default)
    {
        var result = new short[256];
        ExtractWithBaseline(strokes, baseline, cancellationToken).CopyTo(result, 0);
        result.AsSpan(226, 28).Fill(-Unit); result[^2] = Unit; result[^1] = Unit; return result;
    }

    private static void AddDirection(int[] directions, int index, int sign, int length)
    {
        if (sign > 0) directions[index] += length;
        else if (sign < 0) directions[index + 1] += length;
        else { directions[index] += length >> 1; directions[index + 1] += length >> 1; }
    }

    // The native 16K small-square table equals round(sqrt(n)) for every entry.
    // Derive it mathematically rather than distributing the vendor table.
    internal static int Distance(int ax, int ay, int bx, int by)
    {
        int dx = (bx - ax) * 16, dy = (by - ay) * 16, square = dx * dx + dy * dy, shift = 0;
        if (square >= 16384)
        {
            shift = 2;
            while ((square >> (shift * 2)) > 16383) shift++;
        }
        return (int)(Math.Sqrt(square >> (shift * 2)) + .5) << shift;
    }

    // Preserve integer rounding and the repeated-segment branch of the observed
    // legacy resampler. Generic floating-point interpolation changes model inputs.
    internal static void Resample(int[] x, int[] y, int[] distances, int total, int[] sx, int[] sy, int outputOffset = 0, int count = SampleCount)
    {
        sx[outputOffset] = x[0]; sy[outputOffset] = y[0];
        if (count == 1) return;
        int step = total / (count - 1), segment = 0, residual = 0, previousResidual = 0, emitted = 0;
        if (step == 0) throw new ArgumentException("The normalized stroke is too short.");
        while (emitted < count - 1)
        {
            while (residual < step && segment < distances.Length)
            {
                previousResidual = residual; residual += distances[segment++];
            }
            if (segment == 0 || distances[segment - 1] == 0)
                throw new ArgumentException("The stroke cannot be resampled.");
            int index = segment - 1, distance = distances[index], before = residual;
            emitted++;
            sx[outputOffset + emitted] = x[index] + (step - previousResidual) * (x[index + 1] - x[index]) / distance;
            sy[outputOffset + emitted] = y[index] + (step - previousResidual) * (y[index + 1] - y[index]) / distance;
            residual = distance - step + previousResidual;
            int multiple = 2;
            while (residual >= step && emitted < count - 1)
            {
                residual -= step; emitted++;
                sx[outputOffset + emitted] = x[index] + (x[index + 1] - x[index]) * multiple * step / before;
                sy[outputOffset + emitted] = y[index] + (y[index + 1] - y[index]) * multiple * step / before;
                multiple++;
            }
        }
    }
}
