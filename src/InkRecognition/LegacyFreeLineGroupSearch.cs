namespace InkRecognition;

public readonly record struct LegacyLineGroupRange(int FirstGroup, int LastGroup, int LineLabel, int ClusterLabel);
public sealed record LegacyFreeLineGroupSearchResult(double Cost, LegacyLineGroupRange[] Ranges);

/// <summary>18006bc00's unconstrained-row-count DP branch, on explicit prepared moments.</summary>
public static class LegacyFreeLineGroupSearch
{
    public static LegacyFreeLineGroupSearchResult Search(ReadOnlySpan<LegacyPreparedLineGroup> preparedGroups,
        LegacyLineGroupProjection projection, double scale, LegacyLineGroupCostProfile profile,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (preparedGroups.Length is < 1 or > 128) throw new ArgumentException("Expected 1..128 prepared groups.");
        var groups = preparedGroups.ToArray();
        foreach (var g in groups) LegacyPreparedLineGroupCost.Validate(g);
        int count = groups.Length;
        var costs = Enumerable.Repeat(double.MaxValue, count).ToArray();
        var labels = Enumerable.Repeat(int.MinValue, count).ToArray();
        var clusters = Enumerable.Repeat(-1, count).ToArray();
        var starts = Enumerable.Repeat(-1, count).ToArray();
        for (int end = 0; end < count; end++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (end + 1 < count &&
                ((groups[end].LineLabel != int.MinValue && groups[end + 1].LineLabel == groups[end].LineLabel) ||
                 (groups[end].ClusterLabel != -1 && groups[end + 1].ClusterLabel == groups[end].ClusterLabel))) continue;
            for (int start = end; start >= 0; start--)
            {
                var g = groups[start];
                // The native loop compares the current best state's labels and breaks, rather than
                // collecting a separate set of all labels. Unknown labels never clear the best state.
                if ((g.LineLabel != int.MinValue && labels[end] != int.MinValue && g.LineLabel != labels[end]) ||
                    (g.ClusterLabel != -1 && clusters[end] != -1 && g.ClusterLabel != clusters[end])) break;
                double score = LegacyPreparedLineGroupCost.Score(groups.AsSpan(start, end - start + 1), projection, scale, profile, cancellationToken);
                if (start > 0) score += costs[start - 1];
                if (score >= costs[end]) continue;
                costs[end] = score; starts[end] = start;
                if (g.LineLabel != int.MinValue) labels[end] = g.LineLabel;
                if (g.ClusterLabel != -1) clusters[end] = g.ClusterLabel;
            }
        }
        if (starts[^1] < 0 || !double.IsFinite(costs[^1]) || costs[^1] == double.MaxValue)
            throw new InvalidDataException("No finite prepared-group path.");
        var ranges = new List<LegacyLineGroupRange>();
        for (int end = count - 1; end >= 0;)
        {
            int start = starts[end];
            if (start < 0 || start > end) throw new InvalidDataException("Incomplete prepared-group path.");
            ranges.Add(new(start, end, labels[end], clusters[end])); end = start - 1;
        }
        ranges.Reverse();
        return new(costs[^1], ranges.ToArray());
    }
}
