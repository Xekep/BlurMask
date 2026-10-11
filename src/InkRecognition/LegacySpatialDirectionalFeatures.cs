namespace InkRecognition;

/// <summary>
/// Checked first4×4 uniform spatial4f directional313 profile. Source baseline,
/// matching spatial calibration, context and boundaries remain caller data.
/// No automatic line fit, acoustic ensemble or connected-character segmentation.
/// </summary>
public static class LegacySpatialDirectionalFeatures
{
    public static short[] Extract(IReadOnlyList<LegacyInkStroke> sourceStrokes,
        LegacyInkBaseline sourceBaseline, ReadOnlySpan<short> calibrationMinima,
        ReadOnlySpan<short> calibrationMaxima, ReadOnlySpan<short> context,
        short startBoundary, short endBoundary, CancellationToken cancellationToken = default)
    {
        if (calibrationMinima.Length != 274 || calibrationMaxima.Length != 274 || context.Length != 28)
            throw new ArgumentException("Explicit matching spatial274 calibration and28 context values are required.");
        cancellationToken.ThrowIfCancellationRequested();
        var samples = LegacyDirectionalResampling.Prepare(sourceStrokes, targetCount:100,
            includeCoordinateUnit:true, cancellationToken);
        var direction = LegacyDirectionalLandmarks.Extract(samples.Strokes, cancellationToken);
        var intersection = LegacyDirectionalIntersections.Extract(samples, 10, cancellationToken);
        var global = LegacyDirectionalStatistics.Extract(samples.Strokes, samples.Width, samples.Height,
            samples.PhysicalArcLength, direction.Flags.Span, intersection.Flags.Span,
            intersection.SelfLoopCount, cancellationToken);
        var points = samples.Strokes.SelectMany(stroke => stroke.Points.ToArray()).ToArray();
        var memberships = LegacySpatialWindows.Partition(points, samples.Width, samples.Height,
            cancellationToken:cancellationToken);
        var local = new int[16][];
        for (int i = 0; i < local.Length; i++)
        {
            var indices = memberships[i].ToArray().Select(index => checked((byte)index)).ToArray();
            local[i] = LegacySpatialWindowStatistics.Extract(samples, indices,
                direction.Flags.Span, intersection.Flags.Span, true, cancellationToken);
        }
        var geometry = LegacySpatialFeatureAssembly.TransformGeometry(global, local,
            calibrationMinima, calibrationMaxima, cancellationToken);
        var baseline = LegacyDirectionalBaselineStatistics.Extract(samples, sourceBaseline, cancellationToken);
        return LegacyDirectionalFeatureAssembly.AppendInput(geometry, baseline, context,
            startBoundary, endBoundary, cancellationToken);
    }
}
