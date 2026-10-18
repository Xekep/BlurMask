using ScalarMath = System.Math;

namespace InkRecognition.Math;

internal sealed record MathFeatures(float[] Values, int Frames, long[] StrokeEnds, float Scale);

/// <summary>Ordinary pen-stroke Math2 features. Glyph objects and caller-provided Scale are excluded.</summary>
internal static class MathPreprocessor
{
    internal const int Channels = 26, MaxStrokes = 256, MaxPoints = 20000, MaxFrames = 6000;
    private const float Epsilon = 1.1920929e-7f;
    private readonly record struct Bounds(float Xmin, float Ymin, float Xmax, float Ymax);
    private readonly record struct Neighbor(float SquaredDistance, InkPoint Point);

    internal static MathFeatures Process(IReadOnlyList<InkStroke> strokes)
    {
        ArgumentNullException.ThrowIfNull(strokes);
        if (strokes.Count == 0) return new([], 0, [], 1);
        if (strokes.Count > MaxStrokes) throw new ArgumentException("Too many ink strokes.", nameof(strokes));
        var pointCount = 0;
        foreach (var stroke in strokes)
        {
            if (stroke?.Points is not { Count: > 0 }) throw new ArgumentException("Empty ink stroke.", nameof(strokes));
            if (stroke.Points.Count >= 0x8000 || stroke.Points.Count > MaxPoints - pointCount)
                throw new ArgumentException("Too many ink points.", nameof(strokes));
            pointCount += stroke.Points.Count;
            foreach (var p in stroke.Points)
                if (!float.IsFinite(p.X) || !float.IsFinite(p.Y) || MathF.Abs(p.X) > 100000 || MathF.Abs(p.Y) > 100000)
                    throw new ArgumentException("Ink coordinates must be finite and within 100000 units.", nameof(strokes));
        }
        // Ink01EEF0 uses01ED50: ties are stable only in partitions of at most20.
        var means = strokes.Select(stroke => MeanX(stroke.Points)).ToArray();
        var order = LegacyMath2MeanXOrder.CopySourceOrder(means);
        var ordered = order.Select(index => strokes[index]).ToArray();
        var scale = GroupExtremaScale(ordered);
        if (!float.IsFinite(scale) || scale <= 0) throw new ArgumentException("Invalid ink scale.", nameof(strokes));
        var sumX = 0f; var sumY = 0f;
        foreach (var stroke in ordered)
            foreach (var p in stroke.Points) { sumX += p.X; sumY += p.Y; }
        var centerX = sumX / pointCount; var centerY = sumY / pointCount;
        var sampled = new List<InkPoint>[ordered.Length];
        var ends = new long[ordered.Length]; var frames = 0;
        // A caller limit, independent of native semantics: bound interpolation work even
        // when the native 1/200 pointer deduplication suppresses many generated points.
        var interpolationBudget = MaxFrames * 8;
        for (var i = 0; i < ordered.Length; i++)
        {
            var normalized = ordered[i].Points.Select(p => new InkPoint((p.X - centerX) / scale, (p.Y - centerY) / scale)).ToArray();
            if (normalized.Any(p => !float.IsFinite(p.X) || !float.IsFinite(p.Y)))
                throw new ArgumentException("Normalized ink coordinates are not finite.", nameof(strokes));
            sampled[i] = Resample(normalized, MaxFrames - frames, ref interpolationBudget);
            frames += sampled[i].Count; ends[i] = frames;
        }
        var values = CreateFeatures(sampled, frames);
        if (Array.Exists(values, value => !float.IsFinite(value)))
            throw new ArgumentException("Ink features are not finite.", nameof(strokes));
        return new(values, frames, ends, scale);
    }

    private static float MeanX(IReadOnlyList<InkPoint> points)
    { var sum = 0f; foreach (var p in points) sum += p.X; return sum / points.Count; }

    private static bool Close(float a, float b) => MathF.Abs(a - b) <= MathF.Max(Epsilon,
        MathF.Max(MathF.Abs(a), MathF.Abs(b)) * 2.3841858e-7f);
    private static float Distance(InkPoint a, InkPoint b)
    { var x = b.X - a.X; var y = b.Y - a.Y; return MathF.Sqrt(x * x + y * y); }

    // Ink 18001c380: significant chords between vertical extrema, upper middle quantile.
    private static float GroupExtremaScale(IReadOnlyList<InkStroke> strokes)
    {
        var ymin = float.PositiveInfinity; var ymax = float.NegativeInfinity;
        foreach (var stroke in strokes)
            foreach (var p in stroke.Points) { ymin = MathF.Min(ymin, p.Y); ymax = MathF.Max(ymax, p.Y); }
        var height = ymax - ymin;
        if (height == 0) return 1;
        var minimum = height * .01f; var threshold = minimum * minimum;
        var lengths = new List<float>();
        foreach (var stroke in strokes)
        {
            var points = stroke.Points; var anchor = points[0]; var previousDy = 0f; var lastVerticalX = float.MaxValue;
            for (var i = 0; i < points.Count; i++)
            {
                var p = points[i]; var squaredDistance = 0f; var vertical = false;
                if (i < points.Count - 1)
                {
                    var dy = points[i + 1].Y - p.Y;
                    if (dy * previousDy < 0)
                    {
                        var dx = p.X - anchor.X; var ay = p.Y - anchor.Y;
                        squaredDistance = ay * ay + dx * dx;
                        vertical = Close(anchor.X, p.X); anchor = p;
                    }
                    if (dy != 0) previousDy = dy;
                }
                else
                {
                    var dx = p.X - anchor.X; var dy = p.Y - anchor.Y;
                    squaredDistance = dy * dy + dx * dx;
                }
                if (threshold >= squaredDistance || (vertical && Close(lastVerticalX, anchor.X))) continue;
                lengths.Add(MathF.Sqrt(squaredDistance));
                if (vertical) lastVerticalX = anchor.X;
            }
        }
        if (lengths.Count == 0) return height;
        lengths.Sort(); return lengths[lengths.Count / 2];
    }

    // Ink 180017060 v1: no interstroke points/noise; preserve centered coordinates.
    private static List<InkPoint> Resample(IReadOnlyList<InkPoint> points, int frameBudget, ref int interpolationBudget)
    {
        var length = 0f; var minimum = float.MaxValue;
        for (var i = 1; i < points.Count; i++)
        { var d = Distance(points[i - 1], points[i]); length += d; minimum = MathF.Min(minimum, d); }
        var step = MathF.Max(.001f, MathF.Max(1, minimum) * .4f);
        if (length > 0)
        {
            var ratio = length / step - .5f;
            if (!float.IsFinite(ratio) || ratio > int.MaxValue - 1) throw new ArgumentException("Ink spacing exceeds the frame limit.");
            var intervals = ScalarMath.Max(0, (int)ratio + 1);
            if (intervals == 0) throw new ArgumentException("Invalid resampling interval count.");
            step = length / intervals;
        }
        var sampled = new List<InkPoint>();
        void Add(InkPoint point)
        {
            if (sampled.Count >= frameBudget) throw new ArgumentException("Resampled ink exceeds the frame limit.");
            sampled.Add(point);
        }
        Add(points[0]); var residual = 0f; var lastIntegral = 0; var lastFraction = points.Count == 1 ? 255 : 0;
        for (var i = 1; i < points.Count; i++)
        {
            var previous = points[i - 1]; var current = points[i]; var distance = Distance(previous, current);
            var combined = residual + distance;
            if (step < combined && Epsilon < distance)
            {
                var fraction = (step - residual) / distance;
                while (fraction < 1)
                {
                    if (interpolationBudget-- <= 0) throw new ArgumentException("Ink interpolation exceeds the work limit.");
                    combined -= step;
                    var position = (float)(i - 1) + fraction;
                    var integral = (int)MathF.Floor(position);
                    var quantized = (int)((position - integral) * 200f + .5f);
                    if (quantized == 200) { integral++; quantized = integral == points.Count - 1 ? 255 : 0; }
                    // Native 18000db00 uses a 1/200 pointer fraction and endpoint marker255.
                    if (integral != lastIntegral || (quantized != lastFraction && quantized != 255 && lastFraction != 255))
                    {
                        Add(new(fraction * current.X + (1 - fraction) * previous.X,
                            fraction * current.Y + (1 - fraction) * previous.Y));
                        lastIntegral = integral; lastFraction = quantized;
                    }
                    var next = fraction + step / distance;
                    if (next <= fraction) throw new ArgumentException("Ink spacing is too small.");
                    fraction = next;
                }
            }
            residual = combined;
        }
        if (step < residual + residual) Add(points[^1]);
        if (points.Count > 1 && sampled.Count == 1) Add(points[^1]);
        return sampled;
    }

    private static float[] CreateFeatures(IReadOnlyList<List<InkPoint>> strokes, int frames)
    {
        // Zero-initialized arrays reproduce the default Engine calloc's missing KNN cells.
        var values = new float[checked(frames * Channels)]; var points = new InkPoint[frames];
        var bounds = new Bounds[strokes.Count]; var frame = 0;
        for (var s = 0; s < strokes.Count; s++)
        {
            var xmin = float.PositiveInfinity; var ymin = float.PositiveInfinity;
            var xmax = float.NegativeInfinity; var ymax = float.NegativeInfinity;
            for (var p = 0; p < strokes[s].Count; p++, frame++)
            {
                var point = strokes[s][p]; points[frame] = point; var offset = frame * Channels;
                values[offset] = 297; values[offset + 1] = point.X; values[offset + 2] = point.Y;
                values[offset + 3] = p == strokes[s].Count - 1 ? 1 : 0;
                xmin = MathF.Min(xmin, point.X); ymin = MathF.Min(ymin, point.Y);
                xmax = MathF.Max(xmax, point.X); ymax = MathF.Max(ymax, point.Y);
            }
            bounds[s] = new(xmin, ymin, xmax, ymax);
        }
        // math7's forward deltas cross stroke boundaries in the observed configuration.
        for (var i = 0; i < frames - 1; i++)
        {
            var offset = i * Channels;
            values[offset + 4] = points[i + 1].X - points[i].X;
            values[offset + 5] = points[i + 1].Y - points[i].Y;
            if (i < frames - 2)
            {
                values[offset + 6] = points[i + 2].X - points[i].X;
                values[offset + 7] = points[i + 2].Y - points[i].Y;
            }
        }
        frame = 0; var heap = new Neighbor[6];
        for (var own = 0; own < strokes.Count; own++)
            foreach (var point in strokes[own])
            {
                var count = 0;
                for (var other = 0; other < strokes.Count; other++)
                    if (other != own) InsertNeighbor(heap, ref count, ClosestOnPerimeter(point, bounds[other]));
                // Stable insertion sort of the native small heap preserves distance ties.
                for (var i = 1; i < count; i++)
                {
                    var candidate = heap[i]; var j = i - 1;
                    while (j >= 0 && candidate.SquaredDistance < heap[j].SquaredDistance) { heap[j + 1] = heap[j]; j--; }
                    heap[j + 1] = candidate;
                }
                var offset = frame++ * Channels;
                for (var i = 0; i < count; i++)
                {
                    var distance = MathF.Sqrt(heap[i].SquaredDistance);
                    values[offset + 8 + i] = 1 / (distance + 1);
                    if (distance > 0)
                    {
                        values[offset + 14 + i] = (heap[i].Point.X - point.X) / distance;
                        values[offset + 20 + i] = (heap[i].Point.Y - point.Y) / distance;
                    }
                }
            }
        return values;
    }

    private static Neighbor ClosestOnPerimeter(InkPoint p, Bounds b)
    {
        var x = ScalarMath.Clamp(p.X, b.Xmin, b.Xmax); var y = ScalarMath.Clamp(p.Y, b.Ymin, b.Ymax);
        var best = Candidate(new(x, b.Ymin));
        Consider(new(b.Xmin, y)); Consider(new(b.Xmax, y)); Consider(new(x, b.Ymax));
        return best;
        Neighbor Candidate(InkPoint q) { var dx = q.X - p.X; var dy = q.Y - p.Y; return new(dx * dx + dy * dy, q); }
        void Consider(InkPoint q) { var candidate = Candidate(q); if (candidate.SquaredDistance < best.SquaredDistance) best = candidate; }
    }

    // Native bounded maxheap 180009ba0: reject equal-to-root replacements; choose right child on ties.
    private static void InsertNeighbor(Neighbor[] heap, ref int count, Neighbor candidate)
    {
        int index;
        if (count == heap.Length)
        {
            if (heap[0].SquaredDistance <= candidate.SquaredDistance) return;
            index = 0;
            while (index * 2 + 1 < count)
            {
                var child = index * 2 + 1;
                if (child < count - 1 && heap[child].SquaredDistance <= heap[child + 1].SquaredDistance) child++;
                if (heap[child].SquaredDistance <= candidate.SquaredDistance) break;
                heap[index] = heap[child]; index = child;
            }
        }
        else
        {
            index = count++;
            while (index > 0)
            {
                var parent = (index - 1) / 2;
                if (candidate.SquaredDistance <= heap[parent].SquaredDistance) break;
                heap[index] = heap[parent]; index = parent;
            }
        }
        heap[index] = candidate;
    }
}
