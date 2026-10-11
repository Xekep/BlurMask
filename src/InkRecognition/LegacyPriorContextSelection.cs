namespace InkRecognition;

/// <summary>Caller-precomputed accepted context window, with inclusive physical indices.</summary>
public sealed record LegacyPriorContextGroup(int LineId, int ScopeId, int StartPhysicalIndex,
    int EndPhysicalIndex, float CenterX, ReadOnlyMemory<float> Probabilities);

/// <summary>Native ca5f0 object+28 modes; all other native values use the same-line branch.</summary>
public enum LegacyNativePriorContextMode
{
    AllContained=0,
    Disjoint=1,
    SameLineAll=2
}

/// <summary>
/// Checked ca5f0 prior-group filtering on explicit caller metadata: same line,
/// optional scope and explicit selection mode. This does not discover lines,
/// enumerate accepted30-network windows, fit baselines or generate cursive cuts.
/// </summary>
public static class LegacyPriorContextSelection
{
    /// <summary>Exact mode0 contained/mode1 disjoint/other same-line selection.
    /// targetIsBoundary denotes the native candidate kind1, not every special candidate.</summary>
    public static LegacyContextObservation[] SelectNative(IReadOnlyList<LegacyPriorContextGroup> acceptedGroups,
        int targetLineId, ReadOnlySpan<int> targetPhysicalIndices, LegacyNativePriorContextMode mode,
        bool checkScope=false, int targetScopeId=-1, bool targetIsBoundary=false,
        CancellationToken cancellationToken=default)
    {
        if(mode is not LegacyNativePriorContextMode.AllContained and not LegacyNativePriorContextMode.Disjoint and not LegacyNativePriorContextMode.SameLineAll)
            throw new ArgumentOutOfRangeException(nameof(mode));
        return SelectCore(acceptedGroups,targetLineId,targetPhysicalIndices,(int)mode,
            checkScope,targetScopeId,targetIsBoundary,cancellationToken);
    }

    /// <summary>Compatibility API: default matches native mode1. The optional
    /// start-only exclusion is an original heuristic, not native mode0.</summary>
    public static LegacyContextObservation[] Select(IReadOnlyList<LegacyPriorContextGroup> acceptedGroups,
        int targetLineId, ReadOnlySpan<int> targetPhysicalIndices, bool checkScope = false,
        int targetScopeId = -1, bool excludeGroupStartOnly = false,
        bool targetIsBoundary = false, CancellationToken cancellationToken = default)
        =>SelectCore(acceptedGroups,targetLineId,targetPhysicalIndices,excludeGroupStartOnly ? 3:1,
            checkScope,targetScopeId,targetIsBoundary,cancellationToken);

    private static LegacyContextObservation[] SelectCore(IReadOnlyList<LegacyPriorContextGroup> acceptedGroups,
        int targetLineId, ReadOnlySpan<int> targetPhysicalIndices, int mode,
        bool checkScope,int targetScopeId,bool targetIsBoundary,CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(acceptedGroups);
        if (acceptedGroups.Count > LegacyContextWeighting.MaximumObservations || targetPhysicalIndices.Length is < 1 or > 4096)
            throw new ArgumentException("Expected at most1024 groups and1..4096 target physical indices.");
        cancellationToken.ThrowIfCancellationRequested();
        var target = targetPhysicalIndices.ToArray();
        foreach (int index in target)
            if (index is < 0 or > 65534) throw new ArgumentException("Target physical context indices must be0..65534.");
        Array.Sort(target);
        int uniqueCount=0;
        foreach(int index in target)
            if(uniqueCount==0 || target[uniqueCount-1]!=index)target[uniqueCount++]=index;
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
            int start=Array.BinarySearch(target,0,uniqueCount,group.StartPhysicalIndex);
            int lower=start>=0 ? start:~start;
            if(mode==0)
            {
                int end=Array.BinarySearch(target,0,uniqueCount,group.EndPhysicalIndex);
                // Unique sorted IDs contain every integer in the observed range
                // exactly when both endpoints exist and their rank distance
                // equals the inclusive interval length. No native range scan.
                if(start<0 || end<0 || end-start!=group.EndPhysicalIndex-group.StartPhysicalIndex)continue;
            }
            else if(mode==1 && lower<uniqueCount && target[lower]<=group.EndPhysicalIndex)continue;
            else if(mode==3 && start>=0)continue;
            result.Add(new LegacyContextObservation(group.CenterX, group.Probabilities));
        }
        return result.ToArray();
    }
}
