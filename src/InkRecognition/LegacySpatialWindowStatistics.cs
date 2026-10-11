namespace InkRecognition;

/// <summary>
/// Checked0b82c0 spatial local16 raw statistics. The first four channels are
/// cell bbox width/height, point density and mean cumulative physical position;
/// the remaining flags/covariance share the restored temporal arithmetic.
/// Caller-prepared memberships are not interpreted as character boundaries.
/// </summary>
public static class LegacySpatialWindowStatistics
{
    public static int[] Extract(LegacyDirectionalSamples samples, ReadOnlySpan<byte> pointIndices,
        ReadOnlySpan<byte> directionFlags, ReadOnlySpan<byte> intersectionFlags,
        bool includeCoordinateUnit = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(samples);
        return Extract(samples.Strokes, samples.Width, samples.Height, samples.ScaleShift,
            samples.CumulativePhysicalLengths.Span, pointIndices, directionFlags, intersectionFlags,
            includeCoordinateUnit, cancellationToken);
    }

    public static int[] Extract(IReadOnlyList<LegacyInkStroke> preparedStrokes,
        int geometryWidth, int geometryHeight, int scaleShift, ReadOnlySpan<int> cumulativePhysicalLengths,
        ReadOnlySpan<byte> pointIndices, ReadOnlySpan<byte> directionFlags, ReadOnlySpan<byte> intersectionFlags,
        bool includeCoordinateUnit = true, CancellationToken cancellationToken = default)
    {
        // Both native helpers use exactly the same selected-point means,
        // covariance rescaling, endpoint and intersection counts. Reuse the
        // checked common arithmetic and replace their different first4 slots.
        var result = LegacyDirectionalWindowStatistics.Extract(preparedStrokes, geometryWidth,
            geometryHeight, scaleShift, pointIndices, directionFlags, intersectionFlags,
            includeCoordinateUnit, cancellationToken);
        if (cumulativePhysicalLengths.Length != directionFlags.Length)
            throw new ArgumentException("Cumulative physical lengths must match prepared points.");
        int prior = 0;
        foreach (int distance in cumulativePhysicalLengths)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (distance < prior || distance > int.MaxValue >> 10)
                throw new ArgumentException("Unsupported cumulative physical arc domain/order.");
            prior = distance;
        }
        if (pointIndices.IsEmpty) return result;
        result[0] = result[2]; result[1] = result[3];
        result[2] = (pointIndices.Length << 10) / cumulativePhysicalLengths.Length;
        int sum = 0; foreach (byte index in pointIndices) sum += cumulativePhysicalLengths[index];
        // Native first truncates the selected mean, then scales/divides by the
        // total physical arc. Pen-up gaps are absent from this caller array.
        result[3] = sum / pointIndices.Length;
        if (prior != 0) result[3] = (result[3] << 10) / prior;
        return result;
    }
}
