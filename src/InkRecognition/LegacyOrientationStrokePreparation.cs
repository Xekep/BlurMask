using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyOrientationSmoothingProfile(float Minimum, float Maximum, float Extra,
    float OrthogonalLow, float OrthogonalHigh, float CornerLow, float CornerHigh, float FlatnessSquaredFactor)
{
    public static LegacyOrientationSmoothingProfile ReadV102Node(ReadOnlySpan<byte> resource, int offset)
    {
        if (offset < 0 || offset > resource.Length - 48 || BinaryPrimitives.ReadUInt32LittleEndian(resource[offset..]) != 0x0101fe90 ||
            BinaryPrimitives.ReadUInt32LittleEndian(resource[(offset + 4)..]) != 40 || BinaryPrimitives.ReadUInt32LittleEndian(resource[(offset + 8)..]) != 0x265 ||
            BinaryPrimitives.ReadUInt16LittleEndian(resource[(offset + 12)..]) != 0 || BinaryPrimitives.ReadUInt16LittleEndian(resource[(offset + 14)..]) != 0x102)
            throw new InvalidDataException("Expected unencoded48-byte265/v102 smoothing node.");
        var values = new float[8]; for (int i = 0; i < 8; i++) values[i] = BinaryPrimitives.ReadSingleLittleEndian(resource[(offset + 16 + 4 * i)..]);
        var profile = new LegacyOrientationSmoothingProfile(values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7]); profile.Validate(); return profile;
    }
    public void Validate()
    {
        if (!float.IsFinite(Minimum) || Minimum < 0 || Minimum > 10 || !float.IsFinite(Maximum) || Maximum < 0 || Maximum > 10 ||
            !float.IsFinite(Extra) || Extra < 0 || Extra > 10 || !float.IsFinite(FlatnessSquaredFactor) || FlatnessSquaredFactor <= 0 || FlatnessSquaredFactor > 10 ||
            new[] { OrthogonalLow, OrthogonalHigh, CornerLow, CornerHigh }.Any(x => !float.IsFinite(x) || x < -1 || x > 1))
            throw new ArgumentException("Invalid finite smoothing profile.");
    }
}
public readonly record struct LegacyOrientationStrokeMetrics(float OrthogonalFraction, float CornerFraction);
public sealed record LegacyOrientationStrokePreparationResult(IReadOnlyList<InkStroke> Strokes, LegacyOrientationStrokeMetrics Metrics, float SmoothingWeight);

/// <summary>
/// Ordinary no-associated-record/no-key-output child265 path173b90→1728c0,
/// including172680 metrics,171700 spline,171e00 segment distance and1722d0 subdivision.
/// Source-state metrics remain explicit; negative metrics trigger native recomputation.
/// </summary>
public static class LegacyOrientationStrokePreparation
{
    public static LegacyOrientationStrokeMetrics ExtractMetrics(IReadOnlyList<InkStroke> strokes, CancellationToken cancellationToken = default)
    {
        LegacyLineOrientationProjection.Validate(strokes, cancellationToken); int segments = 0, orthogonal = 0, corners = 0;
        foreach (var stroke in strokes)
        {
            var points = stroke.Points; segments += points.Count - 1;
            for (int i = 1; i < points.Count; i++)
            {
                bool vertical = LegacyLineOrientationProjection.Approximately(points[i].X, points[i - 1].X);
                bool horizontal = LegacyLineOrientationProjection.Approximately(points[i].Y, points[i - 1].Y);
                if (vertical || horizontal) orthogonal++;
                if (i + 1 < points.Count && ((vertical && LegacyLineOrientationProjection.Approximately(points[i].Y, points[i + 1].Y)) ||
                    (horizontal && LegacyLineOrientationProjection.Approximately(points[i].X, points[i + 1].X)))) corners++;
            }
        }
        return new(segments == 0 ? 0 : (float)orthogonal / segments, segments <= strokes.Count ? 0 : (float)corners / (segments - strokes.Count));
    }

    public static LegacyOrientationStrokePreparationResult Prepare(IReadOnlyList<InkStroke> strokes, LegacyOrientationSmoothingProfile profile,
        float scale, LegacyOrientationStrokeMetrics sourceMetrics, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile); profile.Validate(); LegacyLineOrientationProjection.Validate(strokes, cancellationToken);
        if (!float.IsFinite(scale) || scale <= 0 || scale > 1_000_000 || !float.IsFinite(sourceMetrics.OrthogonalFraction) || !float.IsFinite(sourceMetrics.CornerFraction) ||
            sourceMetrics.OrthogonalFraction < -1 || sourceMetrics.CornerFraction < -1 || sourceMetrics.OrthogonalFraction > 4096 || sourceMetrics.CornerFraction > 4096)
            throw new ArgumentException("Invalid supplied smoothing scale/metrics.");
        var metrics = sourceMetrics.OrthogonalFraction < 0 || sourceMetrics.CornerFraction < 0 ? ExtractMetrics(strokes, cancellationToken) : sourceMetrics;
        float weight = profile.Minimum;
        if (profile.Minimum < profile.Maximum && profile.OrthogonalLow > 0 && profile.OrthogonalLow < profile.OrthogonalHigh && metrics.OrthogonalFraction >= profile.OrthogonalLow)
            weight = metrics.OrthogonalFraction > profile.OrthogonalHigh ? profile.Maximum :
                ((profile.Maximum - profile.Minimum) * (metrics.OrthogonalFraction - profile.OrthogonalLow)) / (profile.OrthogonalHigh - profile.OrthogonalLow) + profile.Minimum;
        if (profile.Extra > 0 && profile.CornerLow > 0 && profile.CornerLow < profile.CornerHigh)
        {
            if (metrics.CornerFraction > profile.CornerHigh) weight += profile.Extra;
            else if (metrics.CornerFraction > profile.CornerLow) weight += ((metrics.CornerFraction - profile.CornerLow) * profile.Extra) / (profile.CornerHigh - profile.CornerLow);
        }
        var result = new List<InkStroke>(); int total = 0;
        foreach (var stroke in strokes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            //Native bypasses all smoothing for fewer than3 original points.
            var unique = new List<InkPoint> { stroke.Points[0] };
            for (int i = 1; i < stroke.Points.Count; i++)
                if (!LegacyLineOrientationProjection.Approximately(stroke.Points[i].X, unique[^1].X) || !LegacyLineOrientationProjection.Approximately(stroke.Points[i].Y, unique[^1].Y)) unique.Add(stroke.Points[i]);
            var output = new List<InkPoint>();
            if (stroke.Points.Count < 3 || unique.Count < 2) output.AddRange(stroke.Points);
            else if (unique.Count < 4) output.AddRange(unique);
            else
            {
                var lengths = new double[unique.Count - 1]; double arc = 0;
                for (int i = 0; i < lengths.Length; i++)
                {
                    float dx = unique[i + 1].X - unique[i].X, dy = unique[i + 1].Y - unique[i].Y;
                    lengths[i] = Math.Sqrt((double)dx * dx + (double)dy * dy); arc += lengths[i];
                }
                double penalty = (double)((float)unique.Count * weight * weight * weight * weight * scale * scale * scale * scale) / arc;
                var x = LegacyCubicStrokeSmoothing.Fit(unique.Select(p => p.X).ToArray(), lengths, penalty, cancellationToken);
                var y = LegacyCubicStrokeSmoothing.Fit(unique.Select(p => p.Y).ToArray(), lengths, penalty, cancellationToken);
                double threshold = scale * profile.FlatnessSquaredFactor * scale;
                for (int i = 0; i < lengths.Length; i++)
                {
                    double h = lengths[i]; var bx = ToBezier(x[i], h); var by = ToBezier(y[i], h);
                    if (i == 0) Add(new((float)bx[0], (float)by[0]));
                    Flatten(bx, by, 0);
                }
                void Flatten(double[] bx, double[] by, int depth)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (depth >= 11 || (Distance(bx[1], by[1], bx[0], by[0], bx[3], by[3]) < threshold && Distance(bx[2], by[2], bx[0], by[0], bx[3], by[3]) < threshold))
                    { Add(new((float)bx[3], (float)by[3])); return; }
                    var (leftX, rightX) = Split(bx); var (leftY, rightY) = Split(by);
                    Flatten(leftX, leftY, depth + 1); Flatten(rightX, rightY, depth + 1);
                }
                void Add(InkPoint point)
                {
                    if (output.Count >= 4096 - total || !float.IsFinite(point.X) || !float.IsFinite(point.Y)) throw new InvalidDataException("Bounded orientation smoothing output exceeded or nonfinite.");
                    output.Add(point);
                }
            }
            if (output.Count > 4096 - total) throw new InvalidDataException("Bounded orientation smoothing output exceeded.");
            total += output.Count; result.Add(new(output.ToArray()));
        }
        return new(result.AsReadOnly(), metrics, weight);
    }

    private static double[] ToBezier(LegacyCubicCoefficients c, double length) =>
        [c.Value, (c.Linear * length) / 3 + c.Value, ((c.Linear + c.Linear + c.Quadratic * length) * length) / 3 + c.Value,
            ((length * c.Cubic + c.Quadratic) * length + c.Linear) * length + c.Value];
    private static (double[] Left, double[] Right) Split(double[] b)
    {
        double first = (b[0] + b[1]) * .5, middle = (b[1] + b[2]) * .5, last = (b[2] + b[3]) * .5;
        double left = (middle + first) * .5, right = (last + middle) * .5, center = (right + left) * .5;
        return ([b[0], first, left, center], [center, right, last, b[3]]);
    }
    private static double Distance(double x, double y, double x0, double y0, double x1, double y1)
    {
        double dx = x - x0, dy = y - y0, segmentX = x1 - x0, segmentY = y1 - y0;
        double dot = dy * segmentY + dx * segmentX;
        if (dot <= 0) return dy * dy + dx * dx;
        double squared = segmentY * segmentY + segmentX * segmentX;
        if (squared <= dot) return (y - y1) * (y - y1) + (x - x1) * (x - x1);
        double cross = dx * segmentY - dy * segmentX; return cross * cross / squared;
    }
}
