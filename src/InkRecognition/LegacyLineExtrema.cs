namespace InkRecognition;

/// <summary>The explicit-stroke extremum producer in Text 18000b3d0, before Gaussian fitting.</summary>
public static class LegacyLineExtrema
{
    public static (InkPoint[] Lower, InkPoint[] Upper) Extract(IReadOnlyList<InkPoint[]> preparedStrokes,
        bool includeEndpoints = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preparedStrokes); cancellationToken.ThrowIfCancellationRequested();
        if (preparedStrokes.Count is < 1 or > 4096) throw new ArgumentException("Expected 1..4096 explicit strokes.");
        var lower = new List<InkPoint>(); var upper = new List<InkPoint>(); int total = 0;
        foreach (var stroke in preparedStrokes)
        {
            if (stroke is null || stroke.Length == 0 || stroke.Length > 1_000_000 - total)
                throw new ArgumentException("Expected nonempty strokes with at most one million points.");
            total += stroke.Length; int direction = 0;
            for (int i = 0; i < stroke.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested(); var point = stroke[i];
                if (!float.IsFinite(point.X) || !float.IsFinite(point.Y)) throw new ArgumentException("Nonfinite coordinate.");
                if (i == 0) continue;
                var previous = stroke[i - 1];
                if (point.Y < previous.Y)
                {
                    if (includeEndpoints ? direction < 1 : direction == -1) lower.Add(previous);
                    direction = 1;
                }
                else if (previous.Y < point.Y)
                {
                    if (includeEndpoints ? direction >= 0 : direction == 1) upper.Add(previous);
                    direction = -1;
                }
            }
            if (includeEndpoints)
            {
                if (direction == -1) lower.Add(stroke[^1]);
                else if (direction == 1) upper.Add(stroke[^1]);
            }
            if (lower.Count + upper.Count > LegacyGaussianLineFit.MaximumExtrema)
                throw new ArgumentException("Extremum count exceeds the bounded fitter limit.");
        }
        // Native no-extrema fallback duplicates the first point, even for flat strokes.
        if (lower.Count + upper.Count == 0) { lower.Add(preparedStrokes[0][0]); upper.Add(preparedStrokes[0][0]); }
        return (lower.ToArray(), upper.ToArray());
    }
}
