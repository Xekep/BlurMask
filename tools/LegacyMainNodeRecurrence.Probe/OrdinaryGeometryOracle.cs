using InkRecognition;

/// <summary>Original numeric oracle from owned point ranges and resource
/// scalars; does not call the geometry caches/callbacks under test.</summary>
internal sealed class OrdinaryGeometryOracle
{
    private readonly LegacyFreshSingleGlyphMainSampled sampled;
    private readonly LegacyMainWordTransitionProfile profile;
    private readonly float[] log;
    private readonly (float Min, float Max)[][] boxes;
    private readonly float[][] low, high;

    internal OrdinaryGeometryOracle(LegacyFreshSingleGlyphMainSampled sampled, LegacyCursiveSeparationTable table)
    {
        this.sampled = sampled;
        profile = sampled.Owner.TransitionProfile;
        log = table.Values.ToArray();
        var graph = sampled.SourceRanges.Graph;
        var ink = sampled.Cuts.PreparedInk;
        var groupBoxes = graph.Groups.Select(group =>
        {
            float min = float.PositiveInfinity, max = float.NegativeInfinity;
            foreach (var range in group.Ranges.Span)
            {
                int offset = 0;
                for (int index = 0; index < range.StrokeIndex; index++) offset += ink.StrokePointCounts.Span[index];
                foreach (var point in ink.Points.Span.Slice(offset + range.StartPoint, range.EndPointExclusive - range.StartPoint))
                { min = Math.Min(min, point.X); max = Math.Max(max, point.X); }
            }
            return (Min: min, Max: max);
        }).ToArray();
        boxes = Enumerable.Range(0, graph.NodeCount).Select(node => graph.GetNode(node).Select(arc => groupBoxes[arc.GroupId]).ToArray()).ToArray();
        low = boxes.Select(row => row.Select(box => box.Min).ToArray()).ToArray();
        high = boxes.Select(row => row.Select(box => box.Max).ToArray()).ToArray();
        for (int node = graph.NodeCount - 2; node >= 0; node--)
            for (int ordinal = 0; ordinal < boxes[node].Length; ordinal++)
            {
                if (graph.GetNode(node)[ordinal].Span == node) continue;
                for (int next = 0; next < graph.GetNode(node + 1).Count; next++)
                {
                    var successor = graph.GetNode(node + 1)[next];
                    if (successor.Span == 0 && successor.PredecessorArcIndices.Span.Contains(ordinal))
                        low[node][ordinal] = Math.Min(low[node][ordinal], low[node + 1][next]);
                }
            }
        for (int node = 0; node < graph.NodeCount; node++)
            for (int ordinal = 0; ordinal < boxes[node].Length; ordinal++)
            {
                var arc = graph.GetNode(node)[ordinal];
                if (arc.Span == node) continue;
                foreach (int previous in arc.PredecessorArcIndices.Span)
                    high[node][ordinal] = Math.Max(high[node][ordinal], high[node - arc.Span - 1][previous]);
            }
    }

    internal int Correction(int node, ushort ordinal, int previous, ushort previousOrdinal, int kind, int firstId, int previousFirstId)
    {
        var graph = sampled.SourceRanges.Graph;
        var arc = graph.GetNode(node)[ordinal];
        if (kind == 2 && graph.Groups[arc.GroupId].Ranges.Span.ToArray().Any(range => range.StartPoint != 0))
            return 0x4000000;
        float prefix = float.NegativeInfinity;
        foreach (int before in arc.PredecessorArcIndices.Span)
            prefix = Math.Max(prefix, high[node - arc.Span - 1][before]);
        if (float.IsNegativeInfinity(prefix)) prefix = boxes[node][ordinal].Max;
        float gap = (low[node][ordinal] - prefix) / sampled.SourceHeight;
        float sourceGap = sampled.Preparation.SourceGap;
        float expectedGap = sourceGap < 0 ? profile.DefaultGap : sourceGap * profile.SourceGapScale;
        int correction;
        if (kind == 0)
            correction = Math.Min(Log((gap - expectedGap) * -profile.GapSlope, true), profile.JoinMaximumCost);
        else
        {
            float scale = sourceGap < 0 || profile.Matrix.Span[0] == 0 ? 1 : sourceGap * profile.SourceMatrixScale / profile.Matrix.Span[0];
            int index = firstId + previousFirstId * 6;
            correction = Log((gap - scale * profile.Matrix.Span[index]) * profile.GapSlope, false);
            correction = previousFirstId == 5 ? correction - 1 : firstId == 5 ? correction : Math.Min(correction, profile.MatrixMaximumCost);
        }
        float displacement = Math.Max(boxes[node][ordinal].Min - boxes[previous][previousOrdinal].Min,
            boxes[node][ordinal].Max - boxes[previous][previousOrdinal].Max) / sampled.SourceHeight;
        float translation = displacement <= profile.TranslationThreshold ? (displacement - profile.TranslationThreshold) * profile.GapSlope : 0;
        return unchecked(correction - (int)translation * 8192);
    }

    private int Log(float transformed, bool rounded)
    {
        float value = transformed > 4 ? 0 : transformed < -4 ? transformed :
            log[((int)((double)(transformed + 4f) * 255d + .5d)) / 8];
        float cost = -value * 8192f;
        return rounded ? (int)MathF.Round(cost, MidpointRounding.AwayFromZero) : (int)(cost <= 0 ? cost - .5f : cost + .5f);
    }
}
