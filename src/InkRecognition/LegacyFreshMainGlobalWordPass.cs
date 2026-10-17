namespace InkRecognition;

/// <summary>Explicit shared-global-graph main240 pass. Runs the same owned
/// node/span scheduler with actual case masks, pooled caches and native graph
/// callbacks. Does not select a default recognizer, publish text or claim a
/// final winner. Unported reachable language/protocol branches fail explicitly.</summary>
public sealed class LegacyFreshMainGlobalWordPass
{
    private readonly LegacyOwnedMainDirectWordPass scheduler;
    public LegacyFreshMainGlobalWordControllerPolicy Policy {get;}
    public LegacyFreshMainGlobalWordPass(ReadOnlySpan<byte> resource,LegacyFreshMainGlobalWordControllerPolicy policy,
        LegacyMainDirectStorageProvider allocator,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(policy);ArgumentNullException.ThrowIfNull(allocator);token.ThrowIfCancellationRequested();
        //At node119 with later nodes native2144F0 must call207CB0 beforeV80.
        //That global publication owner is not replaced by this logical-row pass.
        if(policy.Owner.Graph.State.NodeCount>policy.MaximumParentDistance54)
            throw new NotSupportedException("Global sources requiring live intermediate compact publication are not supported by this logical-row pass.");
        Policy=policy;
        scheduler=new(resource,policy,allocator,token);
    }
    public LegacyFreshMainGlobalWordPassResult RunOwned(CancellationToken token=default)
        =>new(Policy,scheduler.RunGlobal(token));
}

/// <summary>Owned logical global-node rows. It is not a same-source pass result,
/// native compact publication, decoded string or accepted recognition.</summary>
public sealed class LegacyFreshMainGlobalWordPassResult
{
    private readonly LegacyMainDirectWordNodeResult[] nodes;
    public LegacyFreshMainGlobalWordControllerPolicy Policy {get;}
    public LegacyFreshMainCompactOrdinaryGraph Graph=>Policy.Owner.Graph;
    public int NodeCount=>nodes.Length;
    internal LegacyFreshMainGlobalWordPassResult(LegacyFreshMainGlobalWordControllerPolicy policy,
        IReadOnlyList<LegacyMainDirectWordNodeResult> source)
    {
        if(source.Count!=policy.Owner.Graph.State.NodeCount)throw new ArgumentException("Actual global source/node extent required.");
        Policy=policy;nodes=source.Select(Copy).ToArray();
    }
    public LegacyMainDirectWordNodeResult GetNodeSnapshot(int node)
    {if((uint)node>=(uint)nodes.Length)throw new ArgumentOutOfRangeException(nameof(node));return Copy(nodes[node]);}
    public int[] CopyCumulativePathCosts()=>nodes.Select(n=>n.CumulativePathCost2520).ToArray();
    private static LegacyMainDirectWordNodeResult Copy(LegacyMainDirectWordNodeResult source)=>source with
    {Records=source.Records.ToArray(),PrimaryAlternatives=source.PrimaryAlternatives.ToArray(),SecondaryAlternatives=source.SecondaryAlternatives.ToArray()};
}
