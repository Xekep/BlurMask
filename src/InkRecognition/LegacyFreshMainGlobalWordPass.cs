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
        =>new(Policy,scheduler.RunGlobal(token),token);
}

/// <summary>Owned logical global-node rows. It is not a same-source pass result,
/// native compact publication, decoded string or accepted recognition.</summary>
public sealed class LegacyFreshMainGlobalWordPassResult
{
    private readonly LegacyMainDirectWordNodeResult[] nodes;
    private readonly LegacyFreshMainGlobalCaseSession.ResultQueryLease resultQueries;
    private int publicationStarted;
    public LegacyFreshMainGlobalWordControllerPolicy Policy {get;}
    public LegacyFreshMainCompactOrdinaryGraph Graph=>Policy.Owner.Graph;
    public int NodeCount=>nodes.Length;
    internal LegacyFreshMainGlobalWordPassResult(LegacyFreshMainGlobalWordControllerPolicy policy,
        IReadOnlyList<LegacyMainDirectWordNodeResult> source,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(source.Count!=policy.Owner.Graph.State.NodeCount)throw new ArgumentException("Actual global source/node extent required.");
        Policy=policy;nodes=source.Select(Copy).ToArray();
        resultQueries=policy.Owner.CaptureResultQueryLease(token);
    }
    public LegacyMainDirectWordNodeResult GetNodeSnapshot(int node)
    {if((uint)node>=(uint)nodes.Length)throw new ArgumentOutOfRangeException(nameof(node));return Copy(nodes[node]);}
    public int[] CopyCumulativePathCosts()=>nodes.Select(n=>n.CumulativePathCost2520).ToArray();
    internal ReadOnlySpan<byte> FinalRecords=>nodes[^1].Records.Span;
    internal ushort FinalSourceEnd=>nodes[^1].SourceEnd23f0;
    internal int FinalCumulativeCost=>nodes[^1].CumulativePathCost2520;
    internal void ValidateResultQueryLease(CancellationToken token)=>Policy.Owner.ValidateResultQueryLease(resultQueries,token);
    internal LegacyAggregateDictionaryQueryPage ReadResultQuery(ulong tag,LegacyPreparedDictionaryQuery query,bool auxiliary,CancellationToken token)
        =>Policy.Owner.ReadResultQuery(resultQueries,tag,query,auxiliary,token);
    internal void BeginCompactPublication(CancellationToken token)
    {
        ValidateResultQueryLease(token);
        if(Interlocked.Exchange(ref publicationStarted,1)!=0)
            throw new InvalidOperationException("Global compact publication is single-use, including failed/canceled reconstruction.");
    }
    private static LegacyMainDirectWordNodeResult Copy(LegacyMainDirectWordNodeResult source)=>source with
    {Records=source.Records.ToArray(),PrimaryAlternatives=source.PrimaryAlternatives.ToArray(),SecondaryAlternatives=source.SecondaryAlternatives.ToArray()};
}
