namespace InkRecognition;

/// <summary>One complete physical legacy stroke. Coordinates are copied and bounded.</summary>
public sealed class LegacyInkStroke
{
    private readonly LegacyInkPoint[] _points;
    public ReadOnlySpan<LegacyInkPoint> Points => _points;
    public LegacyInkStroke(ReadOnlySpan<LegacyInkPoint> points)
    {
        if (points.Length is < 1 or > 4096) throw new ArgumentException("A stroke requires 1..4096 points.", nameof(points));
        _points = points.ToArray();
    }
}

// Shared normalized/resampled geometry for trajectory, raster and baseline profiles.
internal sealed class LegacyGlyphGeometry
{
    internal int Width, Height, Scale, MinY, CenterY;
    internal int[] SampleX = new int[30], SampleY = new int[30], Counts = [];
    internal short[] PenState = new short[30];

    internal static LegacyGlyphGeometry Create(IReadOnlyList<LegacyInkStroke> strokes, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(strokes);
        if (strokes.Count is < 1 or > 15) throw new ArgumentException("The 30-sample profile requires 1..15 strokes.", nameof(strokes));
        token.ThrowIfCancellationRequested();
        int minX = short.MaxValue, minY = short.MaxValue, maxX = short.MinValue, maxY = short.MinValue, points = 0;
        foreach (var stroke in strokes)
        {
            ArgumentNullException.ThrowIfNull(stroke);
            points += stroke.Points.Length;
            if (points > 4096) throw new ArgumentException("A glyph exceeds 4096 points.", nameof(strokes));
            foreach (var p in stroke.Points)
            {
                minX = Math.Min(minX, p.X); minY = Math.Min(minY, p.Y);
                maxX = Math.Max(maxX, p.X); maxY = Math.Max(maxY, p.Y);
            }
        }
        int width = maxX - minX, height = maxY - minY, extent = Math.Max(width, height);
        if (extent == 0) throw new ArgumentException("The glyph has no spatial extent.", nameof(strokes));
        var result = new LegacyGlyphGeometry { MinY = minY, Scale = 130048 / extent };
        result.Width = width * result.Scale >> 10; result.Height = height * result.Scale >> 10;
        int center = (128 - Math.Min(result.Width, result.Height) - 1) >> 1;
        result.CenterY = height < width ? center : 0;
        int centerX = width < height ? center : 0;
        var x = new int[strokes.Count][]; var y = new int[strokes.Count][]; var distances = new int[strokes.Count][];
        var lengths = new int[strokes.Count]; var gaps = new int[strokes.Count - 1];
        for (int i = 0; i < strokes.Count; i++)
        {
            int count = strokes[i].Points.Length;
            x[i] = new int[count]; y[i] = new int[count]; distances[i] = new int[count - 1];
            for (int j = 0; j < count; j++)
            {
                var p = strokes[i].Points[j];
                x[i][j] = ((p.X - minX) * result.Scale >> 10) + centerX;
                y[i][j] = ((p.Y - minY) * result.Scale >> 10) + result.CenterY;
                if (j != 0)
                {
                    distances[i][j - 1] = LegacyStrokeFeatures.Distance(x[i][j - 1], y[i][j - 1], x[i][j], y[i][j]);
                    lengths[i] += distances[i][j - 1];
                }
            }
            if (i != 0) gaps[i - 1] = LegacyStrokeFeatures.Distance(x[i - 1][^1], y[i - 1][^1], x[i][0], y[i][0]);
        }
        int total = lengths.Sum() + gaps.Sum(), stride = total / 30;
        int groups = strokes.Count * 2 - 1;
        result.Counts = Enumerable.Repeat(1, groups).ToArray();
        var weights = new int[groups];
        for (int i = 0; i < strokes.Count; i++)
        {
            weights[i * 2] = lengths[i];
            if (i < gaps.Length) weights[i * 2 + 1] = gaps[i] - 2 * stride;
        }
        for (int i = groups; i < 30; i++)
        {
            int best = 0;
            for (int j = 1; j < weights.Length; j++) if (weights[j] > weights[best]) best = j;
            weights[best] -= stride; result.Counts[best]++;
        }
        int offset = 0;
        for (int i = 0; i < strokes.Count; i++)
        {
            token.ThrowIfCancellationRequested();
            int count = result.Counts[i * 2];
            LegacyStrokeFeatures.Resample(x[i], y[i], distances[i], lengths[i], result.SampleX, result.SampleY, offset, count);
            result.PenState.AsSpan(offset, count).Fill(1024); offset += count;
            if (i >= gaps.Length) continue;
            count = result.Counts[i * 2 + 1];
            if (gaps[i] == 0)
            {
                // The native zero-gap branch emits exactly one pen-up sample.
                if (count != 1) throw new NotSupportedException("Repeated zero-gap samples are not established for this legacy profile.");
                result.SampleX[offset] = result.SampleX[offset - 1]; result.SampleY[offset] = result.SampleY[offset - 1];
                result.PenState[offset++] = -1024;
            }
            else
            {
                int remaining = gaps[i], step = remaining / (count + 1);
                for (int j = 0; j < count; j++)
                {
                    result.SampleX[offset] = result.SampleX[offset - 1] + (x[i + 1][0] - result.SampleX[offset - 1]) * step / remaining;
                    result.SampleY[offset] = result.SampleY[offset - 1] + (y[i + 1][0] - result.SampleY[offset - 1]) * step / remaining;
                    result.PenState[offset++] = -1024; remaining -= step;
                }
            }
        }
        if (offset != 30) throw new InvalidDataException("Legacy sample counts do not cover the fixed vector.");
        return result;
    }
}
