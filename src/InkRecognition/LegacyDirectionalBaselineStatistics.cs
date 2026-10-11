namespace InkRecognition;

/// <summary>
/// The checked directional baseline's nine raw statistics (0b9380), prior to
/// 0b9720 channel transformation. Baselines are explicit caller data, not
/// estimated from a glyph. This profile does not inherit the old raster
/// profile's unavailable-baseline behavior.
/// </summary>
public static class LegacyDirectionalBaselineStatistics
{
    public const int Width = 9;

    public static int[] Extract(LegacyDirectionalSamples samples, LegacyInkBaseline sourceBaseline,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(samples);
        if (sourceBaseline.Height <= 0) throw new ArgumentException("This checked profile requires an explicit positive source baseline height.", nameof(sourceBaseline));
        // Native's zero-physical-length normalization branch leaves baseline
        // coordinates unchanged. The sample object does not retain which raw
        // branch was taken; do not infer it from degenerate prepared geometry.
        if (samples.SourceStepFixed == 0) throw new ArgumentException("Degenerate source baseline mapping is ambiguous; supply an explicitly prepared baseline.", nameof(samples));
        int y = LegacyDirectionalCoordinatePreparation.Scale(sourceBaseline.Y - samples.OriginalMinimumY, samples.ScaleShift);
        int height = LegacyDirectionalCoordinatePreparation.Scale(sourceBaseline.Height, samples.ScaleShift);
        if (height == 0) height = 1; // Checked negative-shift guard in 0b4d10.
        return ExtractPrepared(samples.Strokes, samples.Width, samples.Height, samples.PhysicalArcLength, y, height, cancellationToken);
    }

    public static int[] ExtractPrepared(IReadOnlyList<LegacyInkStroke> preparedStrokes,
        int geometryWidth, int geometryHeight, int physicalArcLength,
        int baselineY, int baselineHeight, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preparedStrokes);
        if (preparedStrokes.Count is < 1 or > 15 || geometryWidth is < 1 or > 32768 || geometryHeight is < 1 or > 32768)
            throw new ArgumentException("Expected 1..15 prepared strokes and positive signed-short geometry dimensions.");
        if (baselineHeight <= 0 || physicalArcLength < 0) throw new ArgumentException("Baseline height must be positive and physical arc length nonnegative.");
        cancellationToken.ThrowIfCancellationRequested();
        int count = 0, sumY = 0, above = 0, within = 0, below = 0;
        int top = unchecked(baselineY - baselineHeight);
        foreach (var stroke in preparedStrokes)
        {
            ArgumentNullException.ThrowIfNull(stroke); count += stroke.Points.Length;
            if (count > 255) throw new ArgumentException("Prepared sample points exceed 255.", nameof(preparedStrokes));
            foreach (var point in stroke.Points)
            {
                if (point.X < 0 || point.Y < 0 || point.X > geometryWidth || point.Y > geometryHeight)
                    throw new ArgumentException("Coordinates must be prepared nonnegative points within the supplied dimensions.", nameof(preparedStrokes));
                sumY += point.Y;
                if (point.Y < top) above++;
                else if (point.Y > baselineY) below++;
                else within++;
            }
        }
        return [
            (geometryWidth << 10) / baselineHeight,
            (geometryHeight << 10) / baselineHeight,
            unchecked(physicalArcLength << 10) / baselineHeight,
            unchecked((sumY / count - baselineY) * 1024) / baselineHeight,
            unchecked(top * 1024) / baselineHeight,
            unchecked((geometryHeight - baselineY) * 1024) / baselineHeight,
            (above << 10) / count, (within << 10) / count, (below << 10) / count
        ];
    }
}
