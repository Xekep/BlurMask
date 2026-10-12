namespace InkRecognition;

public sealed record LegacySourceOrientationPreparationResult(LegacyOrdinarySourceInk Source,
    LegacyOrientationScale Scale, LegacyOrientationStrokeMetrics SourceMetrics,
    LegacyOrientationStrokePreparationResult Prepared, ReadOnlyMemory<LegacyOrientationProjection> Projections);

/// <summary>
/// Fresh ordinary source219 metrics15abe0→2d0 scale0809c0→optional265→projection081240.
/// No prior-state ten-percent metric carry, associated records, orientation property defaults or child249 scorer.
/// </summary>
public static class LegacySourceOrientationPreparation
{
    public static LegacyOrientationStrokeMetrics ExtractMetrics(LegacyOrdinarySourceInk source, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source); cancellationToken.ThrowIfCancellationRequested();
        int segments = 0, orthogonal = 0, corners = 0;
        foreach (var stroke in source.Strokes)
        {
            var points = stroke.Points; segments += points.Count - 1;
            for (int i = 1; i < points.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                //15abe0 uses exact equality, not172680's optional tolerant fallback.
                bool vertical = points[i].X == points[i - 1].X, horizontal = points[i].Y == points[i - 1].Y;
                if (vertical || horizontal) orthogonal++;
                if (i + 1 < points.Count && ((vertical && points[i].Y == points[i + 1].Y) ||
                    (horizontal && points[i].X == points[i + 1].X))) corners++;
            }
        }
        return new(segments == 0 ? 0 : (float)orthogonal / segments, segments <= source.Strokes.Count ? 0 : (float)corners / (segments - source.Strokes.Count));
    }

    public static LegacySourceOrientationPreparationResult Prepare(LegacyOrdinarySourceInk source, LegacyLineOrientationProfile orientation,
        LegacyOrientationSmoothingProfile? smoothing, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(orientation); orientation.Validate();
        cancellationToken.ThrowIfCancellationRequested();
        if ((orientation.SecondaryProcessorOffset is not null) != (smoothing is not null))
            throw new ArgumentException("Explicit smoothing profile must match orientation's optional265 presence.");
        var metrics = ExtractMetrics(source, cancellationToken);
        var scale = LegacyLineOrientationProjection.ExtractScale(source.Strokes, orientation.ScaleQuantile, cancellationToken);
        var prepared = smoothing is null ? new LegacyOrientationStrokePreparationResult(source.Strokes, metrics, 0) :
            LegacyOrientationStrokePreparation.Prepare(source.Strokes, smoothing, scale.Value, metrics, cancellationToken);
        var projections = LegacyLineOrientationProjection.Score(prepared.Strokes, LegacyLineOrientationProjection.CreateAngles(orientation, cancellationToken), scale.Value, cancellationToken);
        return new(source, scale, metrics, prepared, projections);
    }
}
