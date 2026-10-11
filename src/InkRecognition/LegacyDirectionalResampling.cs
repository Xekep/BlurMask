namespace InkRecognition;

/// <summary>Owned prepared sample points and physical geometry for directional profiles.</summary>
public sealed class LegacyDirectionalSamples
{
    public IReadOnlyList<LegacyInkStroke> Strokes { get; }
    public int Width { get; }
    public int Height { get; }
    public int ScaleShift { get; }
    public int OriginalMinimumY { get; }
    public int SourceStepFixed { get; }
    public ReadOnlyMemory<int> CumulativePhysicalLengths { get; }
    public int PhysicalArcLength => CumulativePhysicalLengths.Span[^1];
    internal LegacyDirectionalSamples(LegacyInkStroke[] strokes, int width, int height,
        LegacyDirectionalCoordinates source, int step, int[] cumulative)
    {
        Strokes = Array.AsReadOnly(strokes); Width = width; Height = height;
        ScaleShift = source.ScaleShift; OriginalMinimumY = source.OriginalMinimumY;
        SourceStepFixed = step; CumulativePhysicalLengths = cumulative;
    }
}

/// <summary>
/// Checked constant-step physical-stroke resampling (0b4d10) and prepared
/// geometry reduction (0b4af0). The checked profile uses target100. Short
/// source segments carry the last accepted point, repeated rounded points
/// remain, and pen-up gaps are excluded. Intersection flags, baseline/context
/// features and the full 313-channel network input remain separate.
/// </summary>
public static class LegacyDirectionalResampling
{
    public static LegacyDirectionalSamples Prepare(IReadOnlyList<LegacyInkStroke> strokes,
        int targetCount = 100, bool includeCoordinateUnit = true, CancellationToken cancellationToken = default)
        => Resample(LegacyDirectionalCoordinatePreparation.Normalize(strokes, cancellationToken),
            targetCount, includeCoordinateUnit, cancellationToken);

    public static LegacyDirectionalSamples Resample(LegacyDirectionalCoordinates source,
        int targetCount = 100, bool includeCoordinateUnit = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (targetCount is < 1 or > 200) throw new ArgumentOutOfRangeException(nameof(targetCount));
        cancellationToken.ThrowIfCancellationRequested();
        if (source.PhysicalArcLength > int.MaxValue >> 10)
            throw new ArgumentException("Source physical arc length exceeds the checked exponent-10 safe domain.", nameof(source));
        int step = (source.PhysicalArcLength << 10) / targetCount;
        var strokes = new LegacyInkStroke[source.Strokes.Count];
        int count = 0;
        for (int s = 0; s < strokes.Length; s++)
        {
            cancellationToken.ThrowIfCancellationRequested(); var points = source.Strokes[s].Points;
            var output = new List<LegacyInkPoint> { points[0] }; count++;
            LegacyInkPoint accepted = points[0];
            if (step != 0)
            {
                for (int p = 1; p < points.Length; p++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    int distance = LegacyDirectionalCoordinatePreparation.Distance(accepted, points[p]);
                    int repetitions = (distance << 10) / step;
                    if (repetitions == 0) continue;
                    var origin = accepted;
                    for (int n = 1; n <= repetitions; n++)
                    {
                        int fraction = (n * step) / distance;
                        int x = (fraction * points[p].X + (1024 - fraction) * origin.X + 512) >> 10;
                        int y = (fraction * points[p].Y + (1024 - fraction) * origin.Y + 512) >> 10;
                        accepted = new((short)x, (short)y); output.Add(accepted);
                        if (++count > 255) throw new ArgumentException("Resampled geometry exceeds the checked byte-indexed 255-point domain.", nameof(source));
                    }
                }
                if (accepted != points[^1])
                {
                    output.Add(points[^1]);
                    if (++count > 255) throw new ArgumentException("Resampled geometry exceeds the checked byte-indexed 255-point domain.", nameof(source));
                }
            }
            strokes[s] = new(output.ToArray());
        }
        var cumulative = new int[count]; int total = 0, index = 0, maxX = 0, maxY = 0;
        foreach (var stroke in strokes)
        {
            var points = stroke.Points;
            for (int i = 0; i < points.Length; i++)
            {
                maxX = Math.Max(maxX, points[i].X); maxY = Math.Max(maxY, points[i].Y);
                if (i != 0) total += LegacyDirectionalCoordinatePreparation.Distance(points[i - 1], points[i]);
                cumulative[index++] = total;
            }
        }
        int unit = source.ScaleShift < 0 ? 1 : 1 << source.ScaleShift;
        int width = includeCoordinateUnit ? maxX + unit : Math.Max(1, maxX);
        int height = includeCoordinateUnit ? maxY + unit : Math.Max(1, maxY);
        return new(strokes, width, height, source, step, cumulative);
    }
}
