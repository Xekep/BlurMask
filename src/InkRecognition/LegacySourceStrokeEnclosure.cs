namespace InkRecognition;

/// <summary>
/// Text source-service2ad, virtual20 (088970/0880f0). Measures a near-closed
/// contour enclosing a strict majority of another physical stroke's points.
/// This is neither a baseline confidence nor a generic circle classifier.
/// </summary>
public static class LegacySourceStrokeEnclosure
{
    /// <summary>
    /// Evaluate all ordinary physical strokes in an explicitly supplied source
    /// geometry snapshot. Native source219 record+4 eligibility and source+48
    /// enablement remain caller policy; no special/opaque source is inferred.
    /// </summary>
    public static float[] Extract(IReadOnlyList<InkStroke> source,
        CancellationToken cancellationToken = default)
    {
        InkPoint[][] strokes = Snapshot(source, cancellationToken);
        float[] result = new float[strokes.Length];
        for (int i = 0; i < strokes.Length; i++)
            result[i] = Evaluate(strokes, i, cancellationToken);
        return result;
    }

    public static float Evaluate(IReadOnlyList<InkStroke> source, int strokeIndex,
        CancellationToken cancellationToken = default)
    {
        InkPoint[][] strokes = Snapshot(source, cancellationToken);
        if ((uint)strokeIndex >= (uint)strokes.Length)
            throw new ArgumentOutOfRangeException(nameof(strokeIndex));
        return Evaluate(strokes, strokeIndex, cancellationToken);
    }

    private static InkPoint[][] Snapshot(IReadOnlyList<InkStroke> source, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(source);
        ct.ThrowIfCancellationRequested();
        if (source.Count is < 1 or > 256)
            throw new ArgumentException("Expected1..256 physical strokes.", nameof(source));
        InkPoint[][] copy = new InkPoint[source.Count][];
        int total = 0;
        for (int s = 0; s < copy.Length; s++)
        {
            var stroke = source[s] ?? throw new ArgumentException("Null physical stroke.", nameof(source));
            if (stroke.Points.Count is < 1 or > 4096 || total > 4096 - stroke.Points.Count)
                throw new ArgumentException("Expected1..4096 total source points.", nameof(source));
            copy[s] = stroke.Points.ToArray(); total += copy[s].Length;
            foreach (InkPoint point in copy[s])
            {
                ct.ThrowIfCancellationRequested();
                if (!float.IsFinite(point.X) || !float.IsFinite(point.Y) ||
                    MathF.Abs(point.X) > 1_000_000 || MathF.Abs(point.Y) > 1_000_000)
                    throw new ArgumentException("Expected finite coordinates within ±1000000.", nameof(source));
            }
        }
        return copy;
    }

    private static float Evaluate(InkPoint[][] strokes, int index, CancellationToken ct)
    {
        InkPoint[] p = strokes[index]; int n = p.Length;
        if (n > 500 || n < 2) return 0;
        float[] length = new float[n];
        float total = 0; int i = 1;
        // Preserve the native four-segment accumulation order.
        for (; i + 3 < n; i += 4)
        {
            float a = Distance(p[i-1], p[i]); length[i] = a + total;
            float b = Distance(p[i], p[i+1]); b = b + a + total; length[i+1] = b;
            float c = Distance(p[i+1], p[i+2]); c += b; length[i+2] = c;
            total = Distance(p[i+2], p[i+3]) + c; length[i+3] = total;
        }
        for (; i < n; i++) { total += Distance(p[i-1], p[i]); length[i] = total; }

        int[] extrema = new int[n]; extrema[0] = 1;
        int signX = 0, signY = 0;
        float lastX = -1, lastY = -1, threshold = total * .01f;
        for (i = 1; i < n; i++)
        {
            float dx = p[i].X - p[i-1].X, dy = p[i].Y - p[i-1].Y;
            if (dx < 0)
            {
                if (signX < 0 && (lastX == -1 || MathF.Abs(p[i-1].X-lastX) > threshold))
                { extrema[i-1]++; lastX = p[i-1].X; }
                signX = 1;
            }
            else if (dx > 0)
            {
                if (signX > 0 && (lastX == -1 || MathF.Abs(p[i-1].X-lastX) > threshold))
                { extrema[i-1]++; lastX = p[i-1].X; }
                signX = -1;
            }
            if (dy < 0)
            {
                if (signY < 0 && (lastY == -1 || MathF.Abs(p[i-1].Y-lastY) > threshold))
                { extrema[i-1]++; lastY = p[i-1].Y; }
                signY = 1;
            }
            else if (dy > 0)
            {
                if (signY > 0 && (lastY == -1 || MathF.Abs(p[i-1].Y-lastY) > threshold))
                { extrema[i-1]++; lastY = p[i-1].Y; }
                signY = -1;
            }
            extrema[i] = extrema[i-1];
        }
        extrema[n-1]++;
        float quarter = total * .25f, maxSquared = total * total * .1f;
        float best = 0; int bestStart = -1, bestEnd = -1;
        for (int end = n-1; end > 0; end--)
        {
            ct.ThrowIfCancellationRequested();
            float tail = total - length[end];
            if (!(tail < quarter)) continue;
            for (int start = end-1; start >= 0; start--)
            {
                float head = length[start], outside = head + tail;
                if (!(head < quarter && outside < quarter)) continue;
                float dx = p[start].X - p[end].X, dy = p[start].Y - p[end].Y;
                float squared = dx * dx + dy * dy;
                if (!(squared < maxSquared) || extrema[end]-extrema[start] >= 11) continue;
                float distance = (float)Math.Sqrt((double)squared);
                float path = length[end] - head;
                float penalty = (distance + distance + outside) / (distance + path);
                if (!(penalty < .5f && distance / path < .25f)) continue;
                float perimeter = Distance(p[end-1], p[start]) + (length[end-1] - head);
                float score = Area(p, start, end) / (perimeter * perimeter);
                if (score > .0159f)
                {
                    score = (1 - penalty) * score;
                    if (score > best) { best = score; bestStart = start; bestEnd = end; }
                }
            }
        }
        if (best == 0) return 0;
        InkPoint first = p[bestStart];
        float minX = first.X, minY = first.Y, maxX = first.X, maxY = first.Y;
        for (i = bestStart+1; i < bestEnd; i++)
        {
            minX = MathF.Min(minX, p[i].X); minY = MathF.Min(minY, p[i].Y);
            maxX = MathF.Max(maxX, p[i].X); maxY = MathF.Max(maxY, p[i].Y);
        }
        // Native scans physical strokes backwards and ignores the contour itself.
        for (int other = strokes.Length-1; other >= 0; other--)
        {
            ct.ThrowIfCancellationRequested();
            if (other == index) continue;
            int inside = 0;
            foreach (InkPoint q in strokes[other])
                if (q.X >= minX && q.X <= maxX && q.Y >= minY && q.Y <= maxY &&
                    Inside(p, bestStart, bestEnd, q)) inside++;
            if (inside * 2 > strokes[other].Length) return best * 12.566f;
        }
        return 0;
    }

    private static float Distance(InkPoint a, InkPoint b)
    {
        float x = a.X-b.X, y = a.Y-b.Y;
        return (float)Math.Sqrt((double)x*x + (double)y*y);
    }

    private static float Area(InkPoint[] p, int start, int end)
    {
        float y = p[start].Y, x = p[start].X;
        float area = (y-p[end-2].Y)*p[end-1].X + (y-p[end-1].Y)*x + (p[start+1].Y-y)*x;
        for (int i = start; i <= end-3; i++) area += (p[i+2].Y-p[i].Y)*p[i+1].X;
        return MathF.Abs(area) * .5f;
    }

    private static bool Inside(InkPoint[] p, int start, int end, InkPoint q)
    {
        int winding = 0; InkPoint previous = p[start];
        for (int i = start+1; i <= end; i++)
        {
            InkPoint current = i == end ? p[start] : p[i];
            float cross = (current.X-previous.X)*(q.Y-previous.Y) -
                (current.Y-previous.Y)*(q.X-previous.X);
            if (q.Y < previous.Y) { if (current.Y <= q.Y && cross < 0) winding--; }
            else if (q.Y < current.Y && cross > 0) winding++;
            previous = current;
        }
        return winding != 0;
    }
}
