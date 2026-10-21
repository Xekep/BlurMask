namespace InkRecognition;

/// <summary>Explicit actual1111/variant2 directional geometry. Raw88=1 excludes an added coordinate
/// unit from whole post-resampling dimensions AND local extents.
/// Baseline, four calibration arrays, context and boundaries remain caller data.
/// This is a numerical producer, not a model/recognition policy selector.</summary>
public static class LegacySaturatedDirectionalFeatures
{
    public static (short[] Temporal, short[] Spatial) Extract(
        IReadOnlyList<LegacyInkStroke> strokes, LegacyInkBaseline baseline,
        ReadOnlySpan<short> temporalMinima, ReadOnlySpan<short> temporalMaxima,
        ReadOnlySpan<short> spatialMinima, ReadOnlySpan<short> spatialMaxima,
        ReadOnlySpan<short> context, short startBoundary, short endBoundary,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(strokes); token.ThrowIfCancellationRequested();
        if (strokes.Count is < 1 or > 15 || baseline.Height <= 0 || context.Length != 28 ||
            temporalMinima.Length != 274 || temporalMaxima.Length != 274 ||
            spatialMinima.Length != 274 || spatialMaxima.Length != 274 ||
            startBoundary is not (-1024 or 1024) || endBoundary is not (-1024 or 1024))
            throw new ArgumentException("Actual bounded1111 variant2 source and explicit274/28 calibration/context required.");
        int count = 0;
        foreach (var stroke in strokes)
        {
            token.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(stroke);
            if (stroke.Points.Length > 4096 - count) throw new ArgumentException("Source exceeds4096 points.");
            count += stroke.Points.Length;
        }
        if (count == 0) throw new ArgumentException("Nonempty source geometry required.");
        var samples = LegacyDirectionalResampling.Prepare(strokes, 100, false, token);
        if (samples.Strokes.Sum(s => s.Points.Length) is < 1 or > 255)
            throw new ArgumentException("This actual2B1 facade admits1..255 prepared samples before byte-indexed consumers.");
        var direction = LegacyDirectional2B1Landmarks.Extract(samples.Strokes, token);
        var intersections = LegacyDirectionalIntersections.Extract(samples, 10, token);
        var global = LegacyDirectionalStatistics.Extract(samples.Strokes, samples.Width, samples.Height,
            samples.PhysicalArcLength, direction.Flags.Span, intersections.Flags.Span,
            intersections.SelfLoopCount, token);
        var baselineStats = samples.SourceStepFixed == 0
            ? LegacyDirectionalBaselineStatistics.ExtractPrepared(samples.Strokes, samples.Width,
                samples.Height, samples.PhysicalArcLength, baseline.Y, baseline.Height, token)
            : LegacyDirectionalBaselineStatistics.Extract(samples, baseline, token);
        var temporalMembership = LegacyArcLengthWindows.Partition(samples.CumulativePhysicalLengths.Span, 16, token);
        var spatialMembership = LegacySpatialWindows.Partition(samples.Strokes.SelectMany(s => s.Points.ToArray()).ToArray(),
            samples.Width, samples.Height, 4, 4, false, token);
        var temporal = new int[16][]; var spatial = new int[16][];
        for (int i = 0; i < 16; i++)
        {
            token.ThrowIfCancellationRequested();
            temporal[i] = LegacyDirectionalWindowStatistics.Extract(samples, temporalMembership[i].Span,
                direction.Flags.Span, intersections.Flags.Span, false, token);
            spatial[i] = LegacySpatialWindowStatistics.Extract(samples,
                spatialMembership[i].ToArray().Select(v => checked((byte)v)).ToArray(),
                direction.Flags.Span, intersections.Flags.Span, false, token);
        }
        var first = LegacySaturatedDirectionalFeatureAssembly.Compose(global, temporal, baselineStats,
            false, temporalMinima, temporalMaxima, token);
        var second = LegacySaturatedDirectionalFeatureAssembly.Compose(global, spatial, baselineStats,
            true, spatialMinima, spatialMaxima, token);
        return (Append(first, context, startBoundary, endBoundary, token),
            Append(second, context, startBoundary, endBoundary, token));
    }

    private static short[] Append(short[] geometry, ReadOnlySpan<short> context,
        short start, short end, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var result = new short[313]; geometry.CopyTo(result, 0); context.CopyTo(result.AsSpan(283, 28));
        result[311] = start; result[312] = end; return result;
    }
}
