namespace InkRecognition;

/// <summary>
/// Composes the restored temporal directional313 arithmetic passes. Caller
/// supplies a positive source baseline, trained calibration, context and
/// boundary values. This is not a context/baseline estimator, cursive segmenter,
/// model ensemble, automatic recognizer or the alternative4f profile.
/// </summary>
public static class LegacyTemporalDirectionalFeatures
{
    public static short[] Extract(IReadOnlyList<LegacyInkStroke> sourceStrokes,
        LegacyInkBaseline sourceBaseline, ReadOnlySpan<short> calibrationMinima,
        ReadOnlySpan<short> calibrationMaxima, ReadOnlySpan<short> context,
        short startBoundary, short endBoundary, CancellationToken cancellationToken = default)
    {
        if (calibrationMinima.Length != LegacyDirectionalFeatureAssembly.GeometryWidth ||
            calibrationMaxima.Length != LegacyDirectionalFeatureAssembly.GeometryWidth ||
            context.Length != LegacyDirectionalFeatureAssembly.ContextWidth)
            throw new ArgumentException("Explicit274-column calibration and28 context values are required.");
        cancellationToken.ThrowIfCancellationRequested();
        var samples = LegacyDirectionalResampling.Prepare(sourceStrokes, targetCount: 100,
            includeCoordinateUnit: true, cancellationToken);
        var direction = LegacyDirectionalLandmarks.Extract(samples.Strokes, cancellationToken);
        var intersection = LegacyDirectionalIntersections.Extract(samples, 10, cancellationToken);
        var global = LegacyDirectionalStatistics.Extract(samples.Strokes, samples.Width, samples.Height,
            samples.PhysicalArcLength, direction.Flags.Span, intersection.Flags.Span,
            intersection.SelfLoopCount, cancellationToken);
        var memberships = LegacyArcLengthWindows.Partition(samples.CumulativePhysicalLengths.Span, 16, cancellationToken);
        var local = new int[16][];
        for (int i = 0; i < local.Length; i++)
            local[i] = LegacyDirectionalWindowStatistics.Extract(samples, memberships[i].Span,
                direction.Flags.Span, intersection.Flags.Span, true, cancellationToken);
        var geometry = LegacyDirectionalFeatureAssembly.TransformGeometry(global, local,
            calibrationMinima, calibrationMaxima, cancellationToken);
        var baseline = LegacyDirectionalBaselineStatistics.Extract(samples, sourceBaseline, cancellationToken);
        return LegacyDirectionalFeatureAssembly.AppendInput(geometry, baseline, context,
            startBoundary, endBoundary, cancellationToken);
    }
}
