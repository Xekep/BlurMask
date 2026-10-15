namespace InkRecognition;

/// <summary>Already resolved container fields consumed by 1FA300. Fresh -1
/// sentinels must be resolved by the owning controller before storage planning.</summary>
public readonly record struct LegacyNativeWordRingParameters(int Field50, int Field54,
    int Field58, int Field68);

public readonly record struct LegacyNativeWordRingCapacityPlan(int LogicalCapacity,
    int DistinctRowCount, int RetentionDistance)
{
    /// <summary>The native alias is a reference to the previous ring row,
    /// rather than a copy of its record bytes.</summary>
    public int GetPhysicalRowIndex(int logicalNode)
    {
        if ((uint)logicalNode >= (uint)LogicalCapacity || DistinctRowCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(logicalNode));
        return logicalNode % DistinctRowCount;
    }
}

/// <summary>1FA300 signed scalar retention and 40-node growth arithmetic only.
/// Grouped heap allocation, existing-row migration, tracing buffers and native
/// allocation-failure rollback are not represented by this prepared plan.</summary>
public static class LegacyNativeWordRingCapacity
{
    public static int ResolveRetentionDistance(LegacyNativeWordRingParameters parameters)
    {
        int longest = Math.Max(parameters.Field50, Math.Max(parameters.Field58, parameters.Field68));
        int combined = Math.Max(parameters.Field68, unchecked(parameters.Field50 + parameters.Field58));
        return Math.Min(unchecked(longest + parameters.Field54), combined);
    }

    public static LegacyNativeWordRingCapacityPlan Prepare(int requestedNodeCount,
        LegacyNativeWordRingParameters parameters)
    {
        // This is an implementation extent budget, not a recovered threshold.
        if (requestedNodeCount is < 1 or > 1_000_000)
            throw new ArgumentOutOfRangeException(nameof(requestedNodeCount));
        int distance = ResolveRetentionDistance(parameters);
        if (distance <= 0)
            throw new ArgumentException("Resolved positive native ring distance required; unresolved sentinels are not storage ownership.", nameof(parameters));
        int capacity = checked(((requestedNodeCount + 39) / 40) * 40);
        return new(capacity, Math.Min(capacity, distance), distance);
    }
}
