namespace InkRecognition;

/// <summary>An already quantized legacy ink coordinate. Y increases downward.</summary>
public readonly record struct LegacyInkPoint(short X, short Y);

/// <summary>
/// Experimental single-stroke geometry for the legacy 30-sample, exponent-10
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
        int sumX = 0, sumY = 0;
        for (int i = 0; i < SampleCount; i++)
        {
            result[6 + i] = Unit;
            result[6 + SampleCount + i] = (short)(16 * sx[i] - Unit);
            result[6 + 2 * SampleCount + i] = (short)(16 * sy[i] - Unit);
            sumX += sx[i]; sumY += sy[i];
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
        result[210] = Unit;
        result[211] = (short)Math.Min(Unit, 256 * (normalizedHeight + 1) / (normalizedWidth + 1) - Unit);
        result[212] = (short)(16 * sumX / SampleCount - Unit);
        result[213] = (short)(16 * sumY / SampleCount - Unit);
        result[214] = -768; // One physical stroke: 256 * 1 - 1024.
        result[215] = (short)Math.Clamp(sampledLength / 8 - Unit, -Unit, Unit);
        result[216] = (short)(sampledLength == 0 ? -Unit : Unit);
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

    private static void AddDirection(int[] directions, int index, int sign, int length)
    {
        if (sign > 0) directions[index] += length;
        else if (sign < 0) directions[index + 1] += length;
        else { directions[index] += length >> 1; directions[index + 1] += length >> 1; }
    }

    // The native 16K small-square table equals round(sqrt(n)) for every entry.
    // Derive it mathematically rather than distributing the vendor table.
    private static int Distance(int ax, int ay, int bx, int by)
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
    private static void Resample(int[] x, int[] y, int[] distances, int total, int[] sx, int[] sy)
    {
        int step = total / (SampleCount - 1), segment = 0, residual = 0, previousResidual = 0, emitted = 0;
        if (step == 0) throw new ArgumentException("The normalized stroke is too short.");
        sx[0] = x[0]; sy[0] = y[0];
        while (emitted < SampleCount - 1)
        {
            while (residual < step && segment < distances.Length)
            {
                previousResidual = residual; residual += distances[segment++];
            }
            if (segment == 0 || distances[segment - 1] == 0)
                throw new ArgumentException("The stroke cannot be resampled.");
            int index = segment - 1, distance = distances[index], before = residual;
            emitted++;
            sx[emitted] = x[index] + (step - previousResidual) * (x[index + 1] - x[index]) / distance;
            sy[emitted] = y[index] + (step - previousResidual) * (y[index + 1] - y[index]) / distance;
            residual = distance - step + previousResidual;
            int multiple = 2;
            while (residual >= step && emitted < SampleCount - 1)
            {
                residual -= step; emitted++;
                sx[emitted] = x[index] + (x[index + 1] - x[index]) * multiple * step / before;
                sy[emitted] = y[index] + (y[index + 1] - y[index]) * multiple * step / before;
                multiple++;
            }
        }
    }
}
