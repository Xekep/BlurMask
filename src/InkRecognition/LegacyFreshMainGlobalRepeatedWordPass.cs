using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMainGlobalRepeatedPublication.Probe")]

namespace InkRecognition;

/// <summary>Explicit portable resource bounds, not native limits or process RSS.
/// Requested controller payload counts every allocator request, including
/// replacements. Compact payload is retained separately in its fixed pool.</summary>
public sealed class LegacyMainGlobalRepeatedPublicationBudget
{
    public int MaximumLogicalNodes { get; }
    public long MaximumRequestedControllerBytes { get; }
    public int MaximumCompactStorageBytes { get; }

    public LegacyMainGlobalRepeatedPublicationBudget(int maximumLogicalNodes,
        long maximumRequestedControllerBytes, int maximumCompactStorageBytes)
    {
        if (maximumLogicalNodes is < 1 or > 50000 ||
            maximumRequestedControllerBytes is < 1 or > 1073741824 ||
            maximumCompactStorageBytes is < 40 or > 67108864)
            throw new ArgumentOutOfRangeException(nameof(maximumLogicalNodes),
                "Explicit positive bounds within the portable node/payload domains required.");
        MaximumLogicalNodes = maximumLogicalNodes;
        MaximumRequestedControllerBytes = maximumRequestedControllerBytes;
        MaximumCompactStorageBytes = maximumCompactStorageBytes;
    }
}

/// <summary>Same owned global main pass with repeated native120-node LIVE
/// publication. It retains one dictionary, outer290, node epochs and compact
/// pool. No physical ring aliasing, winner or accepted text is manufactured.</summary>
public sealed class LegacyFreshMainGlobalRepeatedWordPass
{
    private readonly LegacyFreshMainGlobalWordPass pass;
    private long requestedBytes;
    private LegacyFreshMainGlobalRepeatedWordPassResult? completed;
    public LegacyMainGlobalRepeatedPublicationBudget Budget { get; }
    public LegacyFreshMainGlobalWordControllerPolicy Policy => pass.Policy;
    public long RequestedControllerPayloadBytes => requestedBytes;

    private LegacyFreshMainGlobalRepeatedWordPass(ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalWordControllerPolicy policy, LegacyMainDirectStorageProvider allocator,
        LegacyFreshMainGlobalGlyphConfidence confidence, ReadOnlySpan<byte> compactStorage,
        LegacyMainGlobalRepeatedPublicationBudget budget,
        Action<LegacyFreshMainGlobalIntermediatePublication>? observer, CancellationToken token)
    {
        Budget = budget;
        pass = LegacyFreshMainGlobalWordPass.CreateRepeatedPublicationCore(resource, policy,
            Allocate, confidence, compactStorage, observer, token);

        LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest request)
        {
            //Subtraction checks the remaining budget before addition or any
            //external allocation. A failed request retains the prior prefix.
            if (request.RequiredBytes < 0 || request.RequiredBytes >
                budget.MaximumRequestedControllerBytes - requestedBytes)
                throw new NotSupportedException("Repeated LIVE controller allocation payload budget exhausted.");
            requestedBytes += request.RequiredBytes;
            return allocator(request);
        }
    }

    public static LegacyFreshMainGlobalRepeatedWordPass Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalWordControllerPolicy policy, LegacyMainDirectStorageProvider allocator,
        LegacyFreshMainGlobalGlyphConfidence confidence, ReadOnlySpan<byte> compactStorage,
        LegacyMainGlobalRepeatedPublicationBudget budget,
        Action<LegacyFreshMainGlobalIntermediatePublication>? observer = null,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(budget); ArgumentNullException.ThrowIfNull(allocator);
        token.ThrowIfCancellationRequested();
        LegacyFreshMainGlobalCompactPublicationState.ValidateOwner(resource, policy, confidence);
        if (policy.Owner.Graph.State.NodeCount > budget.MaximumLogicalNodes ||
            compactStorage.Length > budget.MaximumCompactStorageBytes)
            throw new NotSupportedException("Actual global source or compact payload exceeds the explicit repeated LIVE budget.");
        //The common state constructor validates complete, nonempty40-byte
        //storage before the scheduler constructor performs any allocations.
        return new(resource, policy, allocator, confidence, compactStorage, budget, observer, token);
    }

    /// <summary>Single-use, including failed/canceled entered passes. Completed
    /// LIVE prefixes remain observable; no per-cycle query reset is introduced.</summary>
    public LegacyFreshMainGlobalRepeatedWordPassResult RunOwned(CancellationToken token = default)
    {
        var result = pass.RunOwned(token);
        return completed = new(this, result);
    }
    internal void ValidateCompleted(LegacyFreshMainGlobalRepeatedWordPassResult result, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (!ReferenceEquals(completed, result) || !ReferenceEquals(result.PassResult.Policy, Policy) ||
            result.PassResult.NodeCount > Budget.MaximumLogicalNodes || requestedBytes > Budget.MaximumRequestedControllerBytes)
            throw new InvalidOperationException("Actual completed repeated LIVE owner and its bounded result required.");
        result.PassResult.ValidateResultQueryLease(token);
    }
}

/// <summary>Completion token produced only by its budgeted repeated LIVE pass.
/// It retains the original global rows/pool; it is not an accepted result.</summary>
public sealed class LegacyFreshMainGlobalRepeatedWordPassResult
{
    public LegacyFreshMainGlobalRepeatedWordPass Owner { get; }
    public LegacyFreshMainGlobalWordPassResult PassResult { get; }
    internal LegacyFreshMainGlobalRepeatedWordPassResult(LegacyFreshMainGlobalRepeatedWordPass owner,
        LegacyFreshMainGlobalWordPassResult pass) { Owner = owner; PassResult = pass; }
    internal void Validate(CancellationToken token) => Owner.ValidateCompleted(this, token);
}
