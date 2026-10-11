namespace InkRecognition;

/// <summary>
/// Prepared Y-extremum chord scales from Text 180068fa0, including its endpoint
/// and horizontal-half-weight branches. No clustering or baseline regression.
/// </summary>
public static class LegacyLineScaleSamples
{
    public static float[] Extract(ReadOnlySpan<InkPoint> preparedPoints, bool halfHorizontalWeight = false,
        CancellationToken cancellationToken = default)
    {
        Validate(preparedPoints, cancellationToken);
        if (preparedPoints.Length < 2) return [];
        var result = new List<float>();
        InkPoint anchor = preparedPoints[0];
        float previousDistinctY = anchor.Y;
        for (int i = 1; i < preparedPoints.Length; i++)
        {
            if ((i & 255) == 0) cancellationToken.ThrowIfCancellationRequested();
            InkPoint previous = preparedPoints[i - 1], current = preparedPoints[i];
            if (current.Y == previous.Y) continue;
            bool extremum = (previous.Y > previousDistinctY && previous.Y > current.Y) ||
                (previous.Y < previousDistinctY && previous.Y < current.Y);
            previousDistinctY = previous.Y;
            if (!extremum) continue;
            result.Add(Chord(anchor, previous, halfHorizontalWeight));
            anchor = previous;
        }
        InkPoint last = preparedPoints[^1];
        // The endpoint branch excludes a horizontal final chord, even if X changes.
        if (last.Y != anchor.Y) result.Add(Chord(anchor, last, halfHorizontalWeight));
        return result.ToArray();
    }

    /// <summary>
    /// Remove points nearly equal to the last accepted point as in 180069c80 /
    /// 180077140, then extract one prepared stroke. The record's half-X flag is explicit.
    /// </summary>
    public static float[] PrepareAndExtract(ReadOnlySpan<InkPoint> points, bool halfHorizontalWeight = false,
        CancellationToken cancellationToken = default)
    {
        Validate(points, cancellationToken);
        if (points.IsEmpty) return [];
        var prepared = new List<InkPoint>(points.Length) { points[0] };
        InkPoint previous = points[0];
        for (int i = 1; i < points.Length; i++)
        {
            if ((i & 255) == 0) cancellationToken.ThrowIfCancellationRequested();
            InkPoint point = points[i];
            if (!Different(point.X, previous.X) && !Different(point.Y, previous.Y)) continue;
            prepared.Add(point);
            previous = point;
        }
        return Extract(System.Runtime.InteropServices.CollectionsMarshal.AsSpan(prepared), halfHorizontalWeight, cancellationToken);
    }

    private static bool Different(float a, float b)
    {
        float magnitude = MathF.Max(MathF.Abs(a), MathF.Abs(b));
        float relativeTolerance = magnitude * 2.384185791015625e-7f;
        float tolerance = MathF.Max(relativeTolerance, 1.1920928955078125e-7f);
        float difference = MathF.Abs(a - b);
        return tolerance < difference;
    }

    private static float Chord(InkPoint start, InkPoint end, bool halfHorizontalWeight)
    {
        float dx = MathF.Abs(end.X - start.X);
        float dy = MathF.Abs(end.Y - start.Y);
        float horizontal = halfHorizontalWeight ? dx * .5f : dx;
        float value = MathF.Max(dy, horizontal);
        if (!float.IsFinite(value) || value <= 0)
            throw new ArgumentException("Prepared chord scale overflows or degenerates.");
        return value;
    }

    private static void Validate(ReadOnlySpan<InkPoint> points, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (points.Length > 1_000_000) throw new ArgumentException("Stroke exceeds one million prepared points.", nameof(points));
        for (int i = 0; i < points.Length; i++)
        {
            if ((i & 255) == 0) cancellationToken.ThrowIfCancellationRequested();
            if (!float.IsFinite(points[i].X) || !float.IsFinite(points[i].Y))
                throw new ArgumentException("Line geometry requires finite ink coordinates.", nameof(points));
        }
    }
}
