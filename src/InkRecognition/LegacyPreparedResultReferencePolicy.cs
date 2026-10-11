namespace InkRecognition;

/// <summary>
/// Proven ordinary result-controller metadata operations. Source graph counts,
/// first terminal-path winner and configuration flags remain caller-owned.
/// This does not construct native source graphs or select2257c0's winner.
/// </summary>
public static class LegacyPreparedResultReferencePolicy
{
    /// <summary>Native1a73c0: positive joined count wins; otherwise sum source graph node counts.</summary>
    public static int GetAvailableSourceNodeCount(ReadOnlySpan<int> sourceGraphNodeCounts,
        int cachedJoinedNodeCount, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (cachedJoinedNodeCount > 0) return cachedJoinedNodeCount;
        long total = 0;
        foreach (int count in sourceGraphNodeCounts)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (count < 0) throw new ArgumentException("Ordinary source graph node counts must be nonnegative.", nameof(sourceGraphNodeCounts));
            total += count;
            if (total > int.MaxValue) throw new ArgumentException("Summed source count exceeds the checked native signed32 domain.", nameof(sourceGraphNodeCounts));
        }
        return (int)total;
    }

    /// <summary>
    /// Native2271f0 ordinary reference update. Existing fresh result contexts
    /// start at0; only a valid first alternative with both flags clear updates
    /// the reference to the sign-extended supplied terminal-path winner.
    /// </summary>
    public static long UpdateReferenceCost(long existingReferenceCost, int terminalPathCost,
        int cumulativePathCostAtLastNode, bool isFirstOutputAlternative,
        bool configuration99, bool configurationA8,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        long impossibleWithCumulative = (long)cumulativePathCostAtLastNode + LegacyWordCandidateScorer.ImpossibleCost;
        if (impossibleWithCumulative is < int.MinValue or > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(cumulativePathCostAtLastNode), "Native sentinel addition would overflow.");
        bool present = terminalPathCost != LegacyWordCandidateScorer.ImpossibleCost &&
            terminalPathCost != (int)impossibleWithCumulative;
        return present && isFirstOutputAlternative && !configuration99 && !configurationA8
            ? (long)terminalPathCost : existingReferenceCost;
    }

    /// <summary>Explicit source-kind dispatch at2271f0 and21ffd0; no language inference.</summary>
    public static float GetSourceScoreMultiplier(int sourceKind) => sourceKind == 0x135 ? 5f : 1f;
}
