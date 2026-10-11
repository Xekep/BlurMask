namespace InkRecognition;

internal static class StrokeFeatures
{
    internal const int MaxPoints = 20000, MaxStrokes = 256, MaxFrames = 6000;

    internal static float[] Create(IReadOnlyList<InkStroke> strokes, float[] means, float[] scales,
        float spacing, int minPoints, CancellationToken cancellationToken)
    {
        if (strokes.Count > MaxStrokes) throw new ArgumentException("Too many ink strokes.", nameof(strokes));
        var points = 0; var xmin = float.PositiveInfinity; var ymin = float.PositiveInfinity; var ymax = float.NegativeInfinity;
        foreach (var stroke in strokes)
        {
            if (stroke?.Points is not { Count: > 0 }) throw new ArgumentException("Empty ink stroke.", nameof(strokes));
            if (stroke.Points.Count > MaxPoints - points) throw new ArgumentException("Too many ink points.", nameof(strokes));
            points += stroke.Points.Count;
            foreach (var point in stroke.Points)
            {
                if (!float.IsFinite(point.X) || !float.IsFinite(point.Y) || MathF.Abs(point.X) > 100000 || MathF.Abs(point.Y) > 100000)
                    throw new ArgumentException("Ink coordinates must be finite and within 100000 units.", nameof(strokes));
                xmin = MathF.Min(xmin, point.X); ymin = MathF.Min(ymin, point.Y); ymax = MathF.Max(ymax, point.Y);
            }
        }
        if (points == 0 || ymax - ymin < .001f) return [];
        if (means.Length != 13 || scales.Length != 13 || spacing <= 0 || !float.IsFinite(spacing) || minPoints is < 1 or > 8)
            throw new InvalidDataException("Invalid feature configuration.");
        // Whole-word bounding-box geometry is deliberately a first-stage heuristic.
        // Line clustering and the vendor lexical decoder have not been reconstructed.
        var height = .5f * (ymax - ymin); var baseline = ymin + .9f * (ymax - ymin);
        var sampled = Resample(strokes, xmin, height, baseline, spacing, minPoints, cancellationToken);
        var frames = sampled.Sum(s => s.Count);
        if (frames > MaxFrames) throw new ArgumentException("Resampled ink exceeds the frame limit.", nameof(strokes));
        var features = new float[frames * 13];
        var previousX = 0f; var previousY = 0f; var previous = new float[6]; var frame = 0;
        foreach (var stroke in sampled)
        {
            cancellationToken.ThrowIfCancellationRequested();
            for (var i = 0; i < stroke.Count; i++, frame++)
            {
                var point = stroke[i]; var dx = point.X - previousX; var dy = point.Y - previousY;
                var length = MathF.Sqrt(dx * dx + dy * dy); var offset = frame * 13;
                features[offset] = i == 0 ? -1 : 1;
                features[offset + 1] = dx; features[offset + 2] = dy; features[offset + 3] = length;
                features[offset + 4] = length > 0 ? dx / length : 0; features[offset + 5] = length > 0 ? dy / length : 0;
                for (var channel = 0; channel < 6; channel++)
                {
                    var value = features[offset + channel];
                    features[offset + 6 + channel] = frame == 0 ? 0 : value - previous[channel];
                    previous[channel] = value;
                }
                features[offset + 12] = point.Y; previousX = point.X; previousY = point.Y;
            }
        }
        Array.Clear(features, 1, 5);
        if (frames > 1) Array.Clear(features, 20, 5);
        for (var i = 0; i < features.Length; i++) features[i] = (features[i] - means[i % 13]) * scales[i % 13];
        return features;
    }

    private static float Distance(InkPoint a, InkPoint b)
    { var x = b.X - a.X; var y = b.Y - a.Y; return MathF.Sqrt(x * x + y * y); }

    private static List<List<InkPoint>> Resample(IReadOnlyList<InkStroke> strokes, float xmin, float height,
        float baseline, float spacing, int minPoints, CancellationToken cancellationToken)
    {
        var lengths = new float[strokes.Count]; var minimums = new float[strokes.Count]; var total = 0f;
        for (var i = 0; i < strokes.Count; i++)
        {
            var stroke = strokes[i].Points; var minimum = float.PositiveInfinity; var length = 0f;
            for (var p = 1; p < stroke.Count; p++) { var distance = Distance(stroke[p - 1], stroke[p]); length += distance; minimum = MathF.Min(minimum, distance); }
            lengths[i] = length; minimums[i] = stroke.Count > 1 ? minimum : 0; total += length;
        }
        var target = MathF.Max(.001f, height * spacing); var limit = Math.Max(5000 - strokes.Count, strokes.Count);
        if (total > 0 && limit < (int)(total / target - .5f) + 1) target = total / limit;
        var scale = 1f / height; var result = new List<List<InkPoint>>(strokes.Count); var count = 0;
        InkPoint Normalize(InkPoint p) => new((p.X - xmin) * scale, (p.Y - baseline) * scale);
        for (var s = 0; s < strokes.Count; s++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var stroke = strokes[s].Points; var length = lengths[s];
            var step = MathF.Max(target, MathF.Max(.001f, MathF.Max(height, minimums[s]) * spacing));
            var intervals = Math.Max(minPoints - 1, 0);
            if (length > 0) { intervals = Math.Max(intervals, (int)(length / step - .5f) + 1); step = length / intervals; }
            var sampled = new List<InkPoint> { Normalize(stroke[0]) }; var residual = 0f; var previous = stroke[0];
            for (var p = 1; p < stroke.Count; p++)
            {
                var current = stroke[p]; var distance = Distance(previous, current); var combined = residual + distance;
                if (step < combined && distance > 1.1920929e-7f)
                {
                    var fraction = (step - residual) / distance;
                    while (fraction < 1)
                    {
                        combined -= step;
                        sampled.Add(Normalize(new(fraction * current.X + (1 - fraction) * previous.X,
                            fraction * current.Y + (1 - fraction) * previous.Y)));
                        if (++count > MaxFrames) throw new ArgumentException("Resampled ink exceeds the frame limit.");
                        var next = fraction + step / distance;
                        if (next <= fraction) throw new ArgumentException("Ink spacing is too small.");
                        fraction = next;
                    }
                }
                residual = combined; previous = current;
            }
            if (step < residual + residual) sampled.Add(Normalize(stroke[^1]));
            while (sampled.Count < minPoints) sampled.Add(Normalize(stroke[^1]));
            result.Add(sampled);
        }
        return result;
    }
}
