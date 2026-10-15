namespace InkRecognition;

/// <summary>Owned fresh DIRECT policy to1FA300 scalar storage plan. The50
/// sentinel remains the literal00C860 value;151BF0 resolves54/58, and20C8C0
/// resolves68. No guessed positive field or caller-selected ring distance is
/// introduced. This plans identity indices, not native pointer allocation.</summary>
public sealed class LegacyOwnedFreshMainWordRingPreparation
{
    public LegacyOwnedFreshMainWordControllerPolicy Owner { get; }
    public LegacyNativeWordRingParameters Parameters { get; }
    public LegacyNativeWordRingCapacityPlan CapacityPlan { get; }
    public int SourceNodeCount => Owner.Owner.SourceGraph.NodeCount;

    private LegacyOwnedFreshMainWordRingPreparation(LegacyOwnedFreshMainWordControllerPolicy owner,
        LegacyNativeWordRingParameters parameters,LegacyNativeWordRingCapacityPlan capacity)
    {Owner=owner;Parameters=parameters;CapacityPlan=capacity;}

    public static LegacyOwnedFreshMainWordRingPreparation Prepare(LegacyOwnedFreshMainWordControllerPolicy owner,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(owner);cancellationToken.ThrowIfCancellationRequested();
        //The policy has a private constructor and owns only the NEW00C860
        //no-property DIRECT source branch. Arbitrary prepared snapshots cannot
        //be converted into that ownership by supplying their scalar values.
        var parameters=new LegacyNativeWordRingParameters(-1,owner.MaximumParentDistance54,
            owner.FlagPrepassDistance58,owner.Refresh.MaximumWordLength68);
        var plan=LegacyNativeWordRingCapacity.Prepare(owner.Owner.SourceGraph.NodeCount,parameters);
        cancellationToken.ThrowIfCancellationRequested();
        return new(owner,parameters,plan);
    }

    /// <summary>Logical actual source node to the reusable row identity. The
    /// rounding-only padding in1FA300's logical capacity is not a source node.</summary>
    public int ResolvePhysicalRow(int actualSourceNode)
    {
        if((uint)actualSourceNode>=(uint)SourceNodeCount)throw new ArgumentOutOfRangeException(nameof(actualSourceNode));
        return CapacityPlan.GetPhysicalRowIndex(actualSourceNode);
    }
}
