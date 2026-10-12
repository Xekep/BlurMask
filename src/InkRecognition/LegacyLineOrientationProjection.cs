namespace InkRecognition;

public readonly record struct LegacyOrientationProjection(int AngleIndex, float Score);
public sealed record LegacyOrientationScale(float Value, ReadOnlyMemory<float> HorizontalSamples, ReadOnlyMemory<float> VerticalSamples);

/// <summary>Exact prepared2d0 numerical stages0809c0/012890/081240/080570. This does not run child265 or child249.</summary>
public static class LegacyLineOrientationProjection
{
    public static float[] CreateAngles(LegacyLineOrientationProfile profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile); profile.Validate(); cancellationToken.ThrowIfCancellationRequested();
        var result = new List<float>(); float angle = profile.MinimumDegrees;
        while (!Approximately(angle, profile.MaximumDegrees))
        {
            cancellationToken.ThrowIfCancellationRequested(); result.Add(angle);
            float next = angle + profile.StepDegrees;
            if (!(next > angle) || (next > profile.MaximumDegrees && !Approximately(next, profile.MaximumDegrees)) || result.Count > 128)
                throw new ArgumentException("Angle endpoint is not reachable by the finite native step.");
            angle = next;
        }
        result.Add(profile.MaximumDegrees); return result.ToArray();
    }

    public static LegacyOrientationScale ExtractScale(IReadOnlyList<InkStroke> preparedStrokes, float quantile,
        CancellationToken cancellationToken = default)
    {
        Validate(preparedStrokes, cancellationToken);
        if (!float.IsFinite(quantile) || quantile is < 0 or > 1) throw new ArgumentException("Invalid orientation quantile.");
        var horizontal = new List<float>(); var vertical = new List<float>();
        foreach (var stroke in preparedStrokes)
        {
            ExtractAxis(stroke.Points, true, horizontal); ExtractAxis(stroke.Points, false, vertical);
        }
        float Pick(List<float> samples)
        {
            if (samples.Count == 0) return float.MaxValue;
            var sorted = samples.ToArray(); Array.Sort(sorted);
            float position = (samples.Count - 1) * quantile;
            int index = (int)(position <= 0 ? position - .5f : position + .5f);
            return sorted[Math.Min(samples.Count - 1, index)];
        }
        float scale = MathF.Min(Pick(horizontal), Pick(vertical));
        if (!(scale > 0) || scale == float.MaxValue || !float.IsFinite(scale))
            throw new ArgumentException("Orientation scale has no finite nonzero axis amplitude.");
        return new(scale, horizontal.ToArray(), vertical.ToArray());
    }

    public static InkStroke[] Rotate(IReadOnlyList<InkStroke> preparedStrokes, float angleDegrees,
        CancellationToken cancellationToken = default)
    {
        Validate(preparedStrokes, cancellationToken);
        if (!float.IsFinite(angleDegrees) || MathF.Abs(angleDegrees) > 360) throw new ArgumentException("Invalid orientation angle.");
        double radians = angleDegrees * 0.017453292519943295d, cosine = Math.Cos(radians), sine = Math.Sin(radians);
        var result = new InkStroke[preparedStrokes.Count];
        for (int s = 0; s < result.Length; s++)
        {
            var points = new InkPoint[preparedStrokes[s].Points.Count];
            for (int p = 0; p < points.Length; p++)
            {
                cancellationToken.ThrowIfCancellationRequested(); InkPoint input = preparedStrokes[s].Points[p];
                points[p] = radians == 0 ? input : new((float)(cosine * input.X + sine * input.Y), (float)(cosine * input.Y - sine * input.X));
            }
            result[s] = new(points);
        }
        return result;
    }

    public static LegacyOrientationProjection[] Score(IReadOnlyList<InkStroke> preparedStrokes, ReadOnlySpan<float> angles,
        float scale, CancellationToken cancellationToken = default)
    {
        Validate(preparedStrokes, cancellationToken);
        if (angles.Length is < 1 or > 129 || !float.IsFinite(scale) || scale <= 0) throw new ArgumentException("Invalid projection configuration.");
        float width = scale / 10f;
        if (width <= 0) throw new ArgumentException("Projection bin width underflows.");
        var result = new LegacyOrientationProjection[angles.Length];
        for (int a = 0; a < angles.Length; a++)
        {
            cancellationToken.ThrowIfCancellationRequested(); var rotated = Rotate(preparedStrokes, angles[a], cancellationToken);
            float minimum = float.MaxValue, maximum = -float.MaxValue;
            foreach (var stroke in rotated) foreach (var p in stroke.Points) { minimum = MathF.Min(minimum, p.Y); maximum = MathF.Max(maximum, p.Y); }
            float quotient = (maximum - minimum) / width;
            if (!float.IsFinite(quotient) || quotient > 1_000_000) throw new ArgumentException("Projection histogram exceeds the bounded bin count.");
            var bins = new float[(int)quotient + 1];
            foreach (var stroke in rotated) foreach (var p in stroke.Points) bins[(int)((p.Y - minimum) / width)] += 1f;
            float score = 0;
            for (int b = 0; b < bins.Length; b++)
            {
                if ((b & 1023) == 0) cancellationToken.ThrowIfCancellationRequested();
                float count = bins[b]; if (count > 0) score += (float)Math.Log(count) * count;
            }
            result[a] = new(a, score);
        }
        Sort(result, 0, result.Length - 1); return result;
    }

    internal static bool Approximately(float a, float b) => MathF.Abs(a - b) <= MathF.Max(MathF.Max(MathF.Abs(a), MathF.Abs(b)) * 2.384185791015625e-7f, 1.1920928955078125e-7f);

    internal static void Validate(IReadOnlyList<InkStroke> strokes, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(strokes); cancellationToken.ThrowIfCancellationRequested();
        if (strokes.Count is < 1 or > 256) throw new ArgumentException("Expected1..256 prepared orientation strokes.");
        int total = 0;
        foreach (var stroke in strokes)
        {
            if (stroke?.Points is null || stroke.Points.Count < 1 || stroke.Points.Count > 4096 - total) throw new ArgumentException("Expected nonempty strokes with at most4096 points.");
            total += stroke.Points.Count;
            foreach (var point in stroke.Points)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!float.IsFinite(point.X) || !float.IsFinite(point.Y) || MathF.Abs(point.X) > 1_000_000 || MathF.Abs(point.Y) > 1_000_000)
                    throw new ArgumentException("Invalid prepared orientation point.");
            }
        }
    }

    private static void ExtractAxis(IReadOnlyList<InkPoint> points, bool xAxis, List<float> output)
    {
        float At(int index) => xAxis ? points[index].X : points[index].Y;
        float anchor = At(0), previousDistinct = anchor;
        for (int i = 1; i < points.Count; i++)
        {
            float previous = At(i - 1), current = At(i);
            if (current == previous) continue;
            bool extremum = (previousDistinct < previous && current < previous) || (previous < previousDistinct && previous < current);
            previousDistinct = previous;
            if (extremum) { output.Add(MathF.Abs(previous - anchor)); anchor = previous; }
        }
        float last = At(points.Count - 1); if (last != anchor) output.Add(MathF.Abs(last - anchor));
    }

    private static void Sort(LegacyOrientationProjection[] values, int first, int last)
    {
        while (first < last)
        {
            int left = first, right = last; float pivot = values[(first + last) / 2].Score;
            while (left <= right)
            {
                while (values[left].Score > pivot) left++;
                while (values[right].Score < pivot) right--;
                if (left > right) break;
                (values[left], values[right]) = (values[right], values[left]); left++; right--;
            }
            if (first < right) Sort(values, first, right); first = left;
        }
    }
}
