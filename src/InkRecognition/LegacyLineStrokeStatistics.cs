namespace InkRecognition;

/// <summary>The checked 2e3 prepared stroke's 17 float32 fields (180077140).</summary>
public sealed class LegacyLineStrokeStatistics
{
    private readonly float[] _values;
    internal ReadOnlySpan<float> Values => _values;

    public LegacyLineStrokeStatistics(ReadOnlySpan<float> fields)
    {
        if (fields.Length != 17) throw new ArgumentException("Expected 17 prepared stroke fields.", nameof(fields));
        _values = fields.ToArray();
        foreach (float value in _values) if (!float.IsFinite(value)) throw new ArgumentException("Stroke fields must be finite.");
        if (_values[2] < _values[0] || _values[3] < _values[1] || _values.AsSpan(6).ContainsAnyInRange(float.NegativeInfinity, -float.Epsilon))
            throw new ArgumentException("Expected ordered bounds and nonnegative length, bins and ratios.");
    }

    /// <summary>Copy and prepare one physical float32 stroke; no line/group assignment is inferred.</summary>
    public static LegacyLineStrokeStatistics Extract(ReadOnlySpan<InkPoint> source,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (source.Length is < 1 or > 1_000_000) throw new ArgumentException("Expected one to one million stroke points.");
        var points = new List<InkPoint>(source.Length);
        for (int i = 0; i < source.Length; i++)
        {
            if ((i & 255) == 0) cancellationToken.ThrowIfCancellationRequested();
            InkPoint point = source[i];
            if (!float.IsFinite(point.X) || !float.IsFinite(point.Y)) throw new ArgumentException("Stroke points must be finite.");
            if (points.Count != 0 && !Different(point.X, points[^1].X) && !Different(point.Y, points[^1].Y)) continue;
            points.Add(point);
        }
        float[] fields = new float[17];
        fields[0] = fields[2] = fields[4] = points[0].X;
        fields[1] = fields[3] = fields[5] = points[0].Y;
        for (int i = 1; i < points.Count; i++)
        {
            if ((i & 255) == 0) cancellationToken.ThrowIfCancellationRequested();
            InkPoint previous = points[i - 1], current = points[i];
            fields[0] = MathF.Min(fields[0], current.X); fields[1] = MathF.Min(fields[1], current.Y);
            fields[2] = MathF.Max(fields[2], current.X); fields[3] = MathF.Max(fields[3], current.Y);
            fields[4] += current.X; fields[5] += current.Y;
            float dx = current.X - previous.X, dy = current.Y - previous.Y;
            float dxSquared = dx * dx, dySquared = dy * dy;
            float segment = MathF.Sqrt(dySquared + dxSquared);
            fields[6] += segment;
            float angle = (float)Math.Atan2(dy, dx);
            float binPosition = angle / .7853982f;
            int bin = Math.Min((int)MathF.Floor(binPosition) + 4, 7);
            fields[7 + bin] += segment;
        }
        fields[4] /= points.Count; fields[5] /= points.Count;
        float width = fields[2] - fields[0], height = fields[3] - fields[1];
        float widthSquared = width * width, heightSquared = height * height;
        float diagonal = MathF.Sqrt(heightSquared + widthSquared);
        if (diagonal > 0)
        {
            float dx = points[^1].X - points[0].X, dy = points[^1].Y - points[0].Y;
            float dxSquared = dx * dx, dySquared = dy * dy;
            fields[15] = MathF.Sqrt(dySquared + dxSquared) / diagonal;
        }
        fields[16] = fields[6] <= 0 ? 1 : diagonal / fields[6];
        return new(fields);
    }

    /// <summary>Returns an independent field copy for serialization or diagnostics.</summary>
    public float[] ToArray() => (float[])_values.Clone();

    private static bool Different(float a, float b)
    {
        float relative = MathF.Max(MathF.Abs(a), MathF.Abs(b)) * 2.384185791015625e-7f;
        float tolerance = MathF.Max(relative, 1.1920928955078125e-7f);
        return tolerance < MathF.Abs(a - b);
    }
}
