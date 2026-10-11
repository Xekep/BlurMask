namespace InkRecognition;

/// <summary>Native sample ownership: source stroke and half-open source point interval.</summary>
public readonly record struct LegacyInkRange(int StrokeIndex, int StartPoint, int EndPointExclusive);

/// <summary>An owned selection. Each range retains a separate pen-up boundary.</summary>
public sealed class LegacyInkRangeSelection
{
    public IReadOnlyList<LegacyInkStroke> Strokes { get; }
    public ReadOnlyMemory<LegacyInkRange> SourceRanges { get; }
    public short StartBoundary { get; }
    public short EndBoundary { get; }

    internal LegacyInkRangeSelection(LegacyInkStroke[] strokes, LegacyInkRange[] ranges,
        short start, short end)
    {
        Strokes = Array.AsReadOnly(strokes); SourceRanges = ranges;
        StartBoundary = start; EndBoundary = end;
    }
}

/// <summary>Float source points retained exactly, before normalization or quantization.</summary>
public sealed class LegacyFloatInkRangeSelection
{
    public IReadOnlyList<InkStroke> Strokes { get; }
    public ReadOnlyMemory<LegacyInkRange> SourceRanges { get; }
    public float StartBoundary { get; }
    public float EndBoundary { get; }

    internal LegacyFloatInkRangeSelection(InkStroke[] strokes, LegacyInkRange[] ranges,
        float start, float end)
    {
        Strokes = Array.AsReadOnly(strokes); SourceRanges = ranges;
        StartBoundary = start; EndBoundary = end;
    }
}

/// <summary>
/// Restored ordinary-sample adapter 1b5ba0 and boundary flags from 1b7f70.
/// It consumes caller-proposed point ranges, including interior cursive cuts;
/// it does not infer candidate cuts or their costs.
/// </summary>
public static class LegacyInkRangeExtraction
{
    /// <summary>
    /// Copy points in range order without sorting, merging, interpolation or
    /// normalization. Same-source adjacent ranges remain separate strokes.
    /// Logical start/end suppression corresponds to the native 1a5/1a6 flags.
    /// Boundaries use the explicit exponent10 feature convention (+/-1024).
    /// </summary>
    public static LegacyInkRangeSelection Extract(IReadOnlyList<LegacyInkStroke> source,
        ReadOnlySpan<LegacyInkRange> ranges, bool suppressStart = false, bool suppressEnd = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        cancellationToken.ThrowIfCancellationRequested();
        if (ranges.Length is < 1 or > 15) throw new ArgumentException("A glyph selection requires 1..15 nonempty ranges.", nameof(ranges));
        var owned = ranges.ToArray();
        Validate(source.Count, owned, i =>
        {
            ArgumentNullException.ThrowIfNull(source[i]); return source[i].Points.Length;
        }, cancellationToken);
        var strokes = new LegacyInkStroke[owned.Length];
        for (int i = 0; i < owned.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var range = owned[i];
            strokes[i] = new(source[range.StrokeIndex].Points.Slice(range.StartPoint,
                range.EndPointExclusive - range.StartPoint));
        }
        short start = !suppressStart && owned[0].StartPoint == 0 ? (short)1024 : (short)-1024;
        var last = owned[^1];
        short end = !suppressEnd && last.EndPointExclusive == source[last.StrokeIndex].Points.Length
            ? (short)1024 : (short)-1024;
        return new(strokes, owned, start, end);
    }

    /// <summary>
    /// Float counterpart of the native point-copy adapter. Coordinates retain
    /// their float32 bit patterns; boundaries use the native +/-1 convention.
    /// Nonfinite selected points and malformed/empty intervals are rejected.
    /// </summary>
    public static LegacyFloatInkRangeSelection ExtractFloat(IReadOnlyList<InkStroke> source,
        ReadOnlySpan<LegacyInkRange> ranges, bool suppressStart = false, bool suppressEnd = false,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        cancellationToken.ThrowIfCancellationRequested();
        if (ranges.Length is < 1 or > 15) throw new ArgumentException("A glyph selection requires 1..15 nonempty ranges.", nameof(ranges));
        var owned = ranges.ToArray();
        Validate(source.Count, owned, i =>
        {
            ArgumentNullException.ThrowIfNull(source[i]); ArgumentNullException.ThrowIfNull(source[i].Points);
            return source[i].Points.Count;
        }, cancellationToken);
        var strokes = new InkStroke[owned.Length];
        for (int i = 0; i < owned.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var range = owned[i]; var points = new InkPoint[range.EndPointExclusive - range.StartPoint];
            for (int p = 0; p < points.Length; p++)
            {
                if ((p & 31) == 0) cancellationToken.ThrowIfCancellationRequested();
                var point = source[range.StrokeIndex].Points[range.StartPoint + p];
                if (!float.IsFinite(point.X) || !float.IsFinite(point.Y))
                    throw new ArgumentException("Selected points must be finite.", nameof(source));
                points[p] = point;
            }
            strokes[i] = new(Array.AsReadOnly(points));
        }
        float start = !suppressStart && owned[0].StartPoint == 0 ? 1f : -1f;
        var last = owned[^1];
        float end = !suppressEnd && last.EndPointExclusive == source[last.StrokeIndex].Points.Count ? 1f : -1f;
        return new(strokes, owned, start, end);
    }

    private static void Validate(int sourceCount, LegacyInkRange[] ranges, Func<int, int> pointCount,
        CancellationToken token)
    {
        if (sourceCount is < 1 or > 256) throw new ArgumentException("Expected 1..256 source strokes.");
        if (ranges.Length is < 1 or > 15) throw new ArgumentException("A glyph selection requires 1..15 nonempty ranges.");
        int total = 0;
        foreach (var range in ranges)
        {
            token.ThrowIfCancellationRequested();
            if ((uint)range.StrokeIndex >= (uint)sourceCount)
                throw new ArgumentException("A range references an absent source stroke.");
            int count = pointCount(range.StrokeIndex);
            if (count is < 1 or > 4096 || range.StartPoint < 0 || range.StartPoint >= range.EndPointExclusive ||
                range.EndPointExclusive > count)
                throw new ArgumentException("Ranges require 0 <= start < end <= source point count, at most 4096.");
            total += range.EndPointExclusive - range.StartPoint;
            if (total > 4096) throw new ArgumentException("A selected glyph exceeds 4096 points.");
        }
    }
}
