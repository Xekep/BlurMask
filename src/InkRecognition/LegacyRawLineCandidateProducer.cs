namespace InkRecognition;

public sealed record LegacyRawLineCandidate(LegacyPreparedLineGroup Moments, double OwnCost,
    double SuffixCost, int NextStroke, bool Horizontal);

public sealed record LegacyRawLineCandidates(IReadOnlyList<LegacyRawLineCandidate> HorizontalCandidates,
    IReadOnlyList<LegacyRawLineCandidate> VerticalCandidates, bool SelectedHorizontal)
{
    public IReadOnlyList<LegacyRawLineCandidate> SelectedCandidates => SelectedHorizontal ? HorizontalCandidates : VerticalCandidates;
    public IReadOnlyList<LegacyPreparedLineGroup> GetSelectedGroups()
    {
        var rows = SelectedCandidates; var groups = new List<LegacyPreparedLineGroup>();
        if (rows.Count is < 1 or > 256) throw new InvalidDataException("Invalid selected raw candidate table.");
        for (int i = 0; i < rows.Count;)
        {
            var row = rows[i];
            if (row.NextStroke <= i || row.NextStroke > rows.Count || row.Moments.FirstStroke != i ||
                row.Moments.LastStroke + 1 != row.NextStroke)
                throw new InvalidDataException("Invalid raw candidate successor.");
            groups.Add(row.Moments); i = row.NextStroke;
        }
        return Array.AsReadOnly(groups.ToArray());
    }
}

/// <summary>
/// Ordinary06f6e0 with06e6a0/06e8f0 costs and06eb30/06ee90 merge gates.
/// Points arrive in source order with explicit cluster/primitive metadata.
/// This is the native non-special branch: temporal06f010 scoring and source
/// row remapping06aaf0 are separate, not silently approximated here.
/// </summary>
public static class LegacyRawLineCandidateProducer
{
    /// <param name="externallySuppliedScale">The ordinary native nonzero-count,
    /// positive explicit-scale branch, which disables the configured regression blend.
    /// This does not enable the separate temporal/special scoring branch.</param>
    public static LegacyRawLineCandidates Produce(IReadOnlyList<InkStroke> preparedStrokes,
        LegacyRawLineCandidateProfile profile, ReadOnlySpan<int> clusterLabels,
        ReadOnlySpan<bool> primitiveFlags, double primaryScale, double secondaryScale,
        bool externallySuppliedScale, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preparedStrokes); profile.Validate();
        cancellationToken.ThrowIfCancellationRequested();
        int count = preparedStrokes.Count;
        if (count is < 1 or > 256 || clusterLabels.Length != count || primitiveFlags.Length != count ||
            !double.IsFinite(primaryScale) || !double.IsFinite(secondaryScale) || primaryScale <= 0 || secondaryScale <= 0 ||
            !float.IsFinite((float)secondaryScale) || (float)secondaryScale <= 0)
            throw new ArgumentException("Bounded stroke source, matching metadata and positive finite scales required.");
        var strokes = new LegacyPreparedLineGroup[count];
        int total = 0;
        for (int i = 0; i < count; i++)
        {
            var points = preparedStrokes[i]?.Points ?? throw new ArgumentException("Null raw stroke.");
            if (points.Count is < 1 or > 4096 || total > 4096 - points.Count || clusterLabels[i] < -1)
                throw new ArgumentException("Expected1..4096 total points and explicit cluster IDs.");
            total += points.Count;
            strokes[i] = LegacyLineArcMoments.ExtractStroke(points.ToArray(), i, clusterLabel: clusterLabels[i], cancellationToken: cancellationToken);
        }
        bool[] flags = primitiveFlags.ToArray();
        float regression = externallySuppliedScale ? 0 : profile.RegressionBlendLength;
        var horizontal = profile.HorizontalEnabled ? Search(strokes, flags, true, profile, primaryScale, secondaryScale, regression, cancellationToken) : [];
        var vertical = profile.VerticalEnabled ? Search(strokes, flags, false, profile, primaryScale, secondaryScale, regression, cancellationToken) : [];
        //06f6e0 chooses vertical on an exact tie; no stable-sort replacement.
        bool useHorizontal = horizontal.Length != 0 && (vertical.Length == 0 || horizontal[0].SuffixCost < vertical[0].SuffixCost);
        return new(Array.AsReadOnly(horizontal), Array.AsReadOnly(vertical), useHorizontal);
    }

    private static LegacyRawLineCandidate[] Search(LegacyPreparedLineGroup[] strokes, bool[] flags, bool horizontal,
        LegacyRawLineCandidateProfile p, double primaryScale, double scale, float regression, CancellationToken token)
    {
        int n = strokes.Length; var rows = new LegacyRawLineCandidate[n];
        for (int first = n - 1; first >= 0; first--)
        {
            token.ThrowIfCancellationRequested();
            var state = new Aggregate(strokes[first], flags[first]);
            var xs = new List<(float Start, float End)>(); var ys = new List<(float Start, float End)>();
            var ordered = new List<(float Start, float End)>();
            AddIntervals(strokes[first], horizontal, xs, ys, ordered);
            double own = Score(state, xs, ys, ordered, horizontal, p, scale, regression);
            double best = own + (first + 1 < n ? rows[first + 1].SuffixCost : 0);
            var moments = state.ToGroup(first, first, horizontal, strokes[first].ClusterLabel);
            rows[first] = new(moments, own, Checked(best), first + 1, horizontal);
            uint unknown = 0;
            for (int last = first + 1; last < n; last++)
            {
                token.ThrowIfCancellationRequested();
                int label = strokes[last].ClusterLabel, bestLabel = rows[first].Moments.ClusterLabel;
                if ((label != -1 && bestLabel != -1 && label != bestLabel) || p.MaximumUnlabeledMerges < unknown) break;
                if (label == -1) unknown++;
                Merge(strokes, flags, first, last, state, horizontal, p, primaryScale, scale);
                double extent = horizontal ? (float)(state.MaxY - state.MinY) : (float)(state.MaxX - state.MinX);
                if (extent > (double)p.MaximumExtent * scale) break;
                AddIntervals(strokes[last], horizontal, xs, ys, ordered);
                own = Score(state, xs, ys, ordered, horizontal, p, scale, regression);
                double candidate = own + (last + 1 < n ? rows[last + 1].SuffixCost : 0);
                Checked(candidate);
                if (candidate >= rows[first].SuffixCost) continue;
                if (label != -1) bestLabel = label;
                moments = state.ToGroup(first, last, horizontal, bestLabel);
                rows[first] = new(moments, own, candidate, last + 1, horizontal);
            }
            //Native rejects a selected next record with zero weight.
            if (rows[first].NextStroke < n && rows[rows[first].NextStroke].Moments.Weight == 0)
                throw new InvalidDataException("Empty selected next group.");
        }
        return rows;
    }

    private static void Merge(LegacyPreparedLineGroup[] strokes, bool[] flags, int first, int last, Aggregate a,
        bool horizontal, LegacyRawLineCandidateProfile p, double primaryScale, double scale)
    {
        var raw = strokes[last];
        float xmin = (float)raw.MinX, xmax = (float)raw.MaxX, ymin = (float)raw.MinY, ymax = (float)raw.MaxY;
        float cost;
        if (horizontal)
        {
            if (p.DirectionalHorizontalMerge)
            {
                float gap = (xmin - a.MaxX) / (float)scale;
                cost = gap > 0 ? gap * p.MergeGapWeight : Overlap(raw.Weight, -gap, a.MinY + a.MaxY <= ymax + ymin, p, scale);
            }
            else if (xmin > a.MaxX) cost = ((xmin - a.MaxX) / (float)scale) * p.MergeGapWeight;
            else if (a.MinX > xmax) cost = ((a.MinX - xmax) / (float)scale) * p.MergeGapWeight;
            else
            {
                float intersection = MathF.Min(a.MaxX - xmin, xmax - a.MinX) / (float)scale;
                cost = Overlap(raw.Weight, intersection, a.MinY + a.MaxY <= ymax + ymin, p, scale);
            }
            if (a.Primitive || flags[last])
                for (int i = first; i < last; i++)
                {
                    if ((!flags[last] && !flags[i]) || xmin > (float)strokes[i].MaxX || (float)strokes[i].MinX > xmax) continue;
                    float gap = MathF.Max(0, ymin - (float)strokes[i].MaxY);
                    if (ymax < (float)strokes[i].MinY) gap = MathF.Max(gap, (float)strokes[i].MinY - ymax);
                    if (gap <= 0) continue;
                    double extra = (double)(gap * p.YGapWeight) / primaryScale;
                    if (flags[last] && flags[i]) extra += extra;
                    a.PrimitivePenalty = Checked(extra + a.PrimitivePenalty);
                }
        }
        else
        {
            float gap = (ymin - a.MaxY) / (float)scale;
            cost = gap > 0 ? gap * p.MergeGapWeight : Overlap(raw.Weight, -gap, xmax + xmin <= a.MaxX + a.MinX, p, scale);
        }
        //Native additions and the square happen in float before promotion.
        cost += 0f; a.MergePenalty = Checked((double)(cost * cost) + a.MergePenalty);
        a.Add(raw, flags[last]);
    }

    private static float Overlap(double weight, float overlap, bool lower, LegacyRawLineCandidateProfile p, double scale)
    {
        float length = (float)(weight / scale);
        float blend = length <= p.OverlapBlendLength ? length / p.OverlapBlendLength : 1f;
        if (p.MaximumOverlap <= overlap) overlap = p.MaximumOverlap;
        return blend * (lower ? p.LowerOverlapWeight : p.UpperOverlapWeight) * overlap;
    }

    private static double Score(Aggregate a, List<(float Start, float End)> xs, List<(float Start, float End)> ys,
        List<(float Start, float End)> ordered, bool horizontal, LegacyRawLineCandidateProfile p, double scale, float regression)
    {
        double dependent = horizontal ? a.Sy : a.Sx, square = horizontal ? a.Syy : a.Sxx;
        double variance = (square * a.Weight - dependent * dependent) / (a.Weight * a.Weight * scale * scale);
        if (regression > 0)
        {
            double independent = horizontal ? a.Sx : a.Sy, independentSquare = horizontal ? a.Sxx : a.Syy;
            double determinant = independentSquare * a.Weight - independent * independent;
            if (determinant > 0)
            {
                double slope = (a.Sxy * a.Weight - dependent * independent) / determinant;
                double intercept = (dependent - slope * independent) / a.Weight;
                double blend = a.Weight / scale;
                blend = blend <= regression ? blend / regression : 1;
                double residual = (((((slope * slope * independentSquare + square + intercept * a.Weight * intercept)
                    - (slope + slope) * a.Sxy) - (intercept + intercept) * dependent)
                    + (slope + slope) * intercept * independent) / (scale * a.Weight * scale));
                variance = variance * (1 - blend) + blend * residual;
            }
        }
        Checked(variance);
        double penalty = variance <= 0 ? p.ZeroVarianceCost : Math.Min(p.InverseVarianceWeight / variance, p.ZeroVarianceCost) + p.VarianceWeight * variance;
        double basic = p.BaseCost + a.MergePenalty + (horizontal ? a.PrimitivePenalty : 0) + penalty;
        double xgap = UnionGap(xs) * (p.XGapWeight / scale), ygap = UnionGap(ys) * (p.YGapWeight / scale);
        float orderedGap = 0;
        for (int i = 1; i < ordered.Count; i++)
        {
            float gap = ordered[i].Start - ordered[i - 1].End;
            if (gap > 0) orderedGap += gap;
        }
        double chronology = (p.OrderedGapWeight / scale) * (double)orderedGap;
        return Checked(chronology * chronology + xgap * xgap + basic + ygap * ygap);
    }

    private static void AddIntervals(LegacyPreparedLineGroup raw, bool horizontal,
        List<(float Start, float End)> xs, List<(float Start, float End)> ys, List<(float Start, float End)> ordered)
    {
        AddUnion(xs, (float)raw.MinX, (float)raw.MaxX); AddUnion(ys, (float)raw.MinY, (float)raw.MaxY);
        float start = (float)(horizontal ? raw.MinY : raw.MinX), end = (float)(horizontal ? raw.MaxY : raw.MaxX);
        for (int i = ordered.Count - 1; i >= 0 && ordered[i].Start > start; i--) ordered[i] = (start, ordered[i].End);
        if (ordered.Count > 0) end = MathF.Max(end, ordered[^1].End);
        ordered.Add((start, end));
    }

    //06d410 uses float one-unit tolerance;06d5c0 is the separate chronology list above.
    private static void AddUnion(List<(float Start, float End)> intervals, float start, float end)
    {
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

    private static double UnionGap(List<(float Start, float End)> intervals)
    {
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

    private static double Checked(double value) => double.IsFinite(value) ? value : throw new InvalidDataException("Raw line candidate arithmetic overflow.");

    private sealed class Aggregate(LegacyPreparedLineGroup first, bool primitive)
    {
        public float MinX = (float)first.MinX, MaxX = (float)first.MaxX, MinY = (float)first.MinY, MaxY = (float)first.MaxY;
        public double Weight = first.Weight, Sx = first.SumX, Sy = first.SumY, Sxx = first.SumXX, Syy = first.SumYY, Sxy = first.SumXY;
        public bool Primitive = primitive;
        public double MergePenalty, PrimitivePenalty;
        public void Add(LegacyPreparedLineGroup raw, bool flag)
        {
            MinX = MathF.Min(MinX, (float)raw.MinX); MaxX = MathF.Max(MaxX, (float)raw.MaxX);
            MinY = MathF.Min(MinY, (float)raw.MinY); MaxY = MathF.Max(MaxY, (float)raw.MaxY);
            Weight = raw.Weight + Weight; Sx = raw.SumX + Sx; Sy = raw.SumY + Sy;
            Sxx = raw.SumXX + Sxx; Syy = raw.SumYY + Syy; Sxy = raw.SumXY + Sxy; Primitive |= flag;
        }
        public LegacyPreparedLineGroup ToGroup(int first, int last, bool horizontal, int cluster) =>
            new(first, last, Weight, Sx, Sy, Sxx, Syy, Sxy, MinX, MaxX, MinY, MaxY, horizontal, int.MinValue, cluster);
    }
}
