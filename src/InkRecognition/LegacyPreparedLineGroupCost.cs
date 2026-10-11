namespace InkRecognition;

/// <summary>Explicit moments of a native 0x90 prepared group; not a raw-ink grouping rule.</summary>
public readonly record struct LegacyPreparedLineGroup(int FirstStroke, int LastStroke,
    double Weight, double SumX, double SumY, double SumXX, double SumYY, double SumXY,
    double MinX, double MaxX, double MinY, double MaxY, bool SpecialSingle = false,
    int LineLabel = int.MinValue, int ClusterLabel = -1);

/// <summary>Caller-supplied native class-2e2 cost fields, in their original float precision.</summary>
public readonly record struct LegacyLineGroupCostProfile(float BaseCost, float SpecialSingleCost,
    float AdjacentStrokeCost, float XGapWeight, float YGapWeight, float AdjacentGapWeight,
    float VarianceWeight, float InverseVarianceWeight, float ZeroVarianceCost, float RegressionBlendLength);

public enum LegacyLineGroupProjection { X, Y }

/// <summary>Checked 180068950 / 1800682f0 prepared-group cost; no implicit baseline or clustering.</summary>
public static class LegacyPreparedLineGroupCost
{
    public static double Score(ReadOnlySpan<LegacyPreparedLineGroup> groups,
        LegacyLineGroupProjection projection, double scale, LegacyLineGroupCostProfile profile,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (groups.Length is < 1 or > 512 || !double.IsFinite(scale) || scale <= 0 ||
            projection is not (LegacyLineGroupProjection.X or LegacyLineGroupProjection.Y))
            throw new ArgumentException("Expected bounded prepared groups and a positive scale.");
        ReadOnlySpan<float> fields = [profile.BaseCost, profile.SpecialSingleCost, profile.AdjacentStrokeCost,
            profile.XGapWeight, profile.YGapWeight, profile.AdjacentGapWeight, profile.VarianceWeight,
            profile.InverseVarianceWeight, profile.ZeroVarianceCost, profile.RegressionBlendLength];
        foreach (float field in fields) if (!float.IsFinite(field) || field < 0) throw new ArgumentException("Invalid cost profile.");
        bool x = projection == LegacyLineGroupProjection.X;
        bool regression = profile.RegressionBlendLength > 0;
        double n = 0, totalWeight = 0, sx = 0, sy = 0, sxx = 0, syy = 0, sxy = 0;
        double gap = 0, adjacentGap = 0;
        double basic = groups.Length == 1 && groups[0].SpecialSingle ? profile.SpecialSingleCost : profile.BaseCost;
        var intervals = new List<(float Start, float End)>();
        for (int i = 0; i < groups.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var g = groups[i]; Validate(g);
            n += 1;
            if (x || !regression)
            {
                sx += g.SumX / g.Weight; sy += g.SumY / g.Weight;
                sxx += g.SumXX / g.Weight; syy += g.SumYY / g.Weight; sxy += g.SumXY / g.Weight;
            }
            else
            {
                // 1800682f0's enabled-regression branch computes a reciprocal first.
                double reciprocal = 1 / g.Weight;
                sx += reciprocal * g.SumX; sy += reciprocal * g.SumY;
                sxx += reciprocal * g.SumXX; syy += reciprocal * g.SumYY; sxy += reciprocal * g.SumXY;
            }
            totalWeight += g.Weight;
            if (i + 1 < groups.Length)
            {
                var next = groups[i + 1]; Validate(next);
                double distance = x ? g.MinX - next.MaxX : next.MinY - g.MaxY;
                bool adjacent = (long)g.LastStroke + 1 == next.FirstStroke;
                if (distance > 0) { gap += distance; if (adjacent) adjacentGap += distance; }
                if (adjacent) basic += profile.AdjacentStrokeCost;
            }
            if (regression) AddInterval(intervals, (float)(x ? g.MinY : g.MinX), (float)(x ? g.MaxY : g.MaxX));
        }
        double scaledGap = gap * ((x ? profile.XGapWeight : profile.YGapWeight) / scale);
        double scaledAdjacent = adjacentGap * (profile.AdjacentGapWeight / scale);
        basic = scaledGap * scaledGap + basic + scaledAdjacent * scaledAdjacent;
        double dependent = x ? sx : sy, dependentSquare = x ? sxx : syy;
        double variance = (dependentSquare * n - dependent * dependent) / (n * n * scale * scale);
        if (!double.IsFinite(variance)) throw new InvalidDataException("Prepared group variance overflow.");
        double result = basic + Penalty(variance, profile);
        if (regression)
        {
            double independent = x ? sy : sx, independentSquare = x ? syy : sxx;
            double determinant = independentSquare * n - independent * independent;
            if (!double.IsFinite(determinant)) throw new InvalidDataException("Prepared group determinant overflow.");
            if (determinant > 0 && Math.Abs(independent * independent) + Math.Abs(independentSquare * n) <= Math.Abs(determinant) * 8388608)
            {
                double slope = (sxy * n - independent * dependent) / determinant;
                double intercept = (dependent - slope * independent) / n;
                double residual = (((((slope * slope * independentSquare + dependentSquare + intercept * n * intercept)
                    - (slope + slope) * sxy) - (intercept + intercept) * dependent)
                    + (slope + slope) * intercept * independent) / (n * scale * scale));
                double blend = totalWeight / scale;
                blend = blend <= profile.RegressionBlendLength ? blend / profile.RegressionBlendLength : 1;
                double mixed = (1 - blend) * variance + blend * residual;
                if (!double.IsFinite(mixed)) throw new InvalidDataException("Prepared group regression overflow.");
                if (mixed > 0 && (x || variance > 0))
                {
                    double intervalGap = IntervalGap(intervals);
                    double intervalWeight = x ? profile.YGapWeight : profile.XGapWeight;
                    double unionPenalty = (intervalGap * intervalWeight) / scale;
                    unionPenalty *= unionPenalty;
                    double fitted = basic + unionPenalty + Penalty(mixed, profile);
                    result = Math.Min(fitted, basic + Penalty(variance, profile));
                }
                else result = basic + Penalty(mixed, profile);
            }
        }
        if (!double.IsFinite(result)) throw new InvalidDataException("Prepared group cost overflow.");
        return result;
    }

    private static double Penalty(double variance, LegacyLineGroupCostProfile p) => variance > 0
        ? Math.Min(p.InverseVarianceWeight / variance, p.ZeroVarianceCost) + p.VarianceWeight * variance
        : p.ZeroVarianceCost;

    // 180067fd0: sorted interval union with a one-unit tolerance in float arithmetic.
    private static void AddInterval(List<(float Start, float End)> intervals, float start, float end)
    {
        if (!float.IsFinite(start) || !float.IsFinite(end)) throw new InvalidDataException("Interval coordinate overflow.");
        int last = intervals.Count - 1;
        while (last >= 0 && intervals[last].Start > end + 1f) last--;
        for (int first = 0; first <= last; first++)
        {
            if (start - 1f > intervals[first].End) continue;
            intervals[first] = (MathF.Min(start, intervals[first].Start), MathF.Max(end, intervals[last].End));
            if (last > first) intervals.RemoveRange(first + 1, last - first);
            return;
        }
        intervals.Insert(last + 1, (start, end));
    }

    private static double IntervalGap(List<(float Start, float End)> intervals)
    {
        // 180068230 sums backwards in batches of four; each subtraction remains float.
        double gap = 0; int i = intervals.Count - 1;
        while (i >= 4)
        {
            gap = gap + (double)(intervals[i].Start - intervals[i - 1].End)
                + (double)(intervals[i - 1].Start - intervals[i - 2].End)
                + (double)(intervals[i - 2].Start - intervals[i - 3].End)
                + (double)(intervals[i - 3].Start - intervals[i - 4].End);
            i -= 4;
        }
        for (; i > 0; i--) gap += (double)(intervals[i].Start - intervals[i - 1].End);
        return gap;
    }

    internal static void Validate(LegacyPreparedLineGroup g)
    {
        ReadOnlySpan<double> values = [g.Weight, g.SumX, g.SumY, g.SumXX, g.SumYY, g.SumXY, g.MinX, g.MaxX, g.MinY, g.MaxY];
        foreach (double value in values) if (!double.IsFinite(value)) throw new ArgumentException("Invalid prepared group moments.");
        if (g.Weight <= 0 || g.MinX > g.MaxX || g.MinY > g.MaxY ||
            g.FirstStroke < 0 || g.LastStroke < g.FirstStroke || g.ClusterLabel < -1)
            throw new ArgumentException("Invalid prepared group moments, indices or bounds.");
    }
}
