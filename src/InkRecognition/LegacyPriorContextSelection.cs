namespace InkRecognition;

/// <summary>Caller-precomputed accepted context window, with inclusive physical indices.</summary>
public sealed record LegacyPriorContextGroup(int LineId, int ScopeId, int StartPhysicalIndex,
    int EndPhysicalIndex, float CenterX, ReadOnlyMemory<float> Probabilities);

/// <summary>
/// Checked ca5f0 prior-group filtering on explicit caller metadata: same line,
/// optional scope and physical-ink exclusion. This does not discover lines,
/// enumerate accepted30-network windows, fit baselines or generate cursive cuts.
/// </summary>
public static class LegacyPriorContextSelection
{
    public static LegacyContextObservation[] Select(IReadOnlyList<LegacyPriorContextGroup> acceptedGroups,
        int targetLineId, ReadOnlySpan<int> targetPhysicalIndices, bool checkScope = false,
        int targetScopeId = -1, bool excludeGroupStartOnly = false,
        bool targetIsBoundary = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(acceptedGroups);
        if (acceptedGroups.Count > LegacyContextWeighting.MaximumObservations || targetPhysicalIndices.Length is < 1 or > 4096)
            throw new ArgumentException("Expected at most1024 groups and1..4096 target physical indices.");
        cancellationToken.ThrowIfCancellationRequested();
        var target = targetPhysicalIndices.ToArray();
        foreach (int index in target)
            if (index is < 0 or > 65534) throw new ArgumentException("Target physical context indices must be0..65534.");
        Array.Sort(target);
        var result = new List<LegacyContextObservation>();
        foreach (var group in acceptedGroups)
        {
            cancellationToken.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(group);
            if (group.StartPhysicalIndex is < 1 or > 65534 || group.EndPhysicalIndex < group.StartPhysicalIndex ||
                group.EndPhysicalIndex > 65534 || !float.IsFinite(group.CenterX) || group.Probabilities.Length != LegacyContextWeighting.Width)
                throw new ArgumentException("Accepted groups require bounded inclusive physical indices, finite center and28 probabilities.");
            foreach (float probability in group.Probabilities.Span)
                if (!float.IsFinite(probability) || probability is < 0 or > 1)
                    throw new ArgumentException("Accepted-group neural probabilities must be finite in0..1.");
            if (targetIsBoundary || group.LineId != targetLineId || (checkScope && group.ScopeId != targetScopeId)) continue;
            int position = Array.BinarySearch(target, group.StartPhysicalIndex);
            if (position >= 0) continue;
            position = ~position;
            // Native either excludes any target ID in the inclusive observed
            // range or only the first physical index. Binary-search preserves
            // that selection without scanning every integer in every range.
            if (!excludeGroupStartOnly && position < target.Length && target[position] <= group.EndPhysicalIndex) continue;
            result.Add(new LegacyContextObservation(group.CenterX, group.Probabilities));
        }
        return result.ToArray();
    }
}
