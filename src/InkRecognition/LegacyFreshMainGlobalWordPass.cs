namespace InkRecognition;

/// <summary>Explicit shared-global-graph main240 pass. Runs the same owned
/// node/span scheduler with actual case masks, pooled caches and native graph
/// callbacks. Does not select a default recognizer, publish text or claim a
/// final winner. Unported reachable language/protocol branches fail explicitly.</summary>
public sealed class LegacyFreshMainGlobalWordPass
{
    private readonly LegacyOwnedMainDirectWordPass scheduler;
    private readonly LegacyOwnedMainDirectWordPass.GlobalIntermediatePublication? publication;
    internal Action<LegacyMainSoftHyphenObservedCall>? SoftHyphenObserver
    { set => scheduler.SoftHyphenObserver = value; }
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
    private LegacyFreshMainGlobalWordPass(ReadOnlySpan<byte> resource, LegacyFreshMainGlobalWordControllerPolicy policy,
        LegacyMainDirectStorageProvider allocator, LegacyOwnedMainDirectWordPass.GlobalIntermediatePublication publication,
        CancellationToken token)
    {
        Policy = policy; this.publication = publication;
        scheduler = new(resource, policy, allocator, token);
    }
    /// <summary>Opt-in actual signed120-node207CB0 gate beforeV80. Retains the
    /// same source/backend and persistent compact pool. Logical row ownership
    /// does not reconstruct native physical ring allocation or acceptance.</summary>
    public static LegacyFreshMainGlobalWordPass CreateWithIntermediatePublication(ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalWordControllerPolicy policy, LegacyMainDirectStorageProvider allocator,
        LegacyFreshMainGlobalGlyphConfidence confidence, ReadOnlySpan<byte> compactAllocatorResidue40,
        Action<LegacyFreshMainGlobalIntermediatePublication>? publicationObserver = null,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(allocator); token.ThrowIfCancellationRequested();
        LegacyFreshMainGlobalCompactPublicationState.ValidateOwner(resource, policy, confidence);
        //215DC2 excludes the final node. Thus count240 has only gate119;
        //count241 would reach a second gate239 and remains unverified here.
        if (policy.Owner.Graph.State.NodeCount > 240)
            throw new NotSupportedException("This explicit global LIVE producer supports at most one intermediate120-node cycle (up to240 logical nodes).");
        var state = new LegacyFreshMainGlobalCompactPublicationState(policy, compactAllocatorResidue40);
        return new(resource, policy, allocator, new(confidence, state, publicationObserver), token);
    }
    //Only the separately budgeted owning adapter calls this repeated native
    //gate path; the original public factory retains its one-cycle scope.
    internal static LegacyFreshMainGlobalWordPass CreateRepeatedPublicationCore(ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalWordControllerPolicy policy, LegacyMainDirectStorageProvider allocator,
        LegacyFreshMainGlobalGlyphConfidence confidence, ReadOnlySpan<byte> residue,
        Action<LegacyFreshMainGlobalIntermediatePublication>? observer, CancellationToken token)
    {
        LegacyFreshMainGlobalCompactPublicationState.ValidateOwner(resource, policy, confidence);
        return new(resource, policy, allocator, new(confidence, new(policy, residue), observer), token);
    }

    //Friend-only prepared consumer seam. This does not change the public native gate.
    internal static LegacyFreshMainGlobalWordPass CreateWithPreparedPublicationAtNode(ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalWordControllerPolicy policy, LegacyMainDirectStorageProvider allocator,
        LegacyFreshMainGlobalGlyphConfidence confidence, ReadOnlySpan<byte> residue, int endingNode,
        Action<LegacyFreshMainGlobalIntermediatePublication>? observer = null, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(allocator); token.ThrowIfCancellationRequested();
        LegacyFreshMainGlobalCompactPublicationState.ValidateOwner(resource, policy, confidence);
        if (endingNode < 0 || endingNode >= policy.Owner.Graph.State.NodeCount - 1 ||
            policy.Owner.Graph.State.NodeCount > policy.MaximumParentDistance54)
            throw new ArgumentOutOfRangeException(nameof(endingNode));
        return new(resource, policy, allocator, new(confidence, new(policy, residue), observer, endingNode), token);
    }
    public LegacyFreshMainGlobalWordPassResult RunOwned(CancellationToken token=default)
    {
        var rows = publication is null ? scheduler.RunGlobal(token) : scheduler.RunGlobalWithIntermediatePublication(publication, token);
        var result = new LegacyFreshMainGlobalWordPassResult(Policy, rows, token, publication?.State,
            scheduler.RequireGlobalPublicationStorage(Policy));
        if (publication is not null) result.BindPublicationReader(scheduler.BindGlobalResultPublicationReader(result, token));
        return result;
    }
}

/// <summary>Owned logical global-node rows. It is not a same-source pass result,
/// native compact publication, decoded string or accepted recognition.</summary>
public sealed class LegacyFreshMainGlobalWordPassResult
{
    private readonly LegacyMainDirectWordNodeResult[] nodes;
    private readonly LegacyFreshMainGlobalCaseSession.ResultQueryLease resultQueries;
    private readonly LegacyFreshMainGlobalPublicationStorage? publicationStorage;
    internal LegacyFreshMainGlobalPublicationStorage RequirePublicationStorage(CancellationToken token)
    {
        ValidateResultQueryLease(token);
        if(publicationStorage is null||!publicationStorage.IsFor(Policy))
            throw new InvalidOperationException("Actual producing GLOBAL scheduler allocator required.");
        return publicationStorage;
    }
    private int publicationStarted;
    private LegacyNativeDelimiterQuerySource? publicationReader;
    internal void BindPublicationReader(LegacyNativeDelimiterQuerySource reader)
    {
        ArgumentNullException.ThrowIfNull(reader);
        if (publicationReader is not null || IntermediatePublicationState is null)
            throw new InvalidOperationException("Only the producing LIVE pass can bind its retained outer290 once.");
        publicationReader = reader;
    }
    internal LegacyNativeDelimiterQuerySource RequirePublicationReader(CancellationToken token)
    {
        ValidateResultQueryLease(token);
        return publicationReader ?? throw new InvalidOperationException("Actual retained global LIVE/final outer290 required.");
    }
    internal LegacyFreshMainGlobalCompactPublicationState? IntermediatePublicationState { get; }
    public int IntermediatePublicationCount => IntermediatePublicationState?.PublicationCount ?? 0;
    public int NextCompactPublicationNode2490 => IntermediatePublicationState?.NextNode ?? 0;
    public int IntermediateCompactRecordCount2470 => IntermediatePublicationState?.Count ?? 0;
    public byte[] CopyIntermediateCompactStorage() => IntermediatePublicationState?.Storage.ToArray() ?? [];
    public int[] CopyIntermediateNodeStarts2478() => IntermediatePublicationState is { NextNode: > 0 } state
        ? state.Starts.AsSpan(0, state.NextNode + 1).ToArray() : [];
    public int[] CopyIntermediatePrefixOwners2488() => IntermediatePublicationState is { NextNode: > 0 } state
        ? state.PrefixOwners.AsSpan(0, state.NextNode + 1).ToArray() : [];
    public LegacyFreshMainGlobalWordControllerPolicy Policy {get;}
    public LegacyFreshMainCompactOrdinaryGraph Graph=>Policy.Owner.Graph;
    public int NodeCount=>nodes.Length;
    internal LegacyFreshMainGlobalWordPassResult(LegacyFreshMainGlobalWordControllerPolicy policy,
        IReadOnlyList<LegacyMainDirectWordNodeResult> source,CancellationToken token,
        LegacyFreshMainGlobalCompactPublicationState? intermediateState = null,
        LegacyFreshMainGlobalPublicationStorage? publicationStorage = null)
    {
        token.ThrowIfCancellationRequested();
        if(source.Count!=policy.Owner.Graph.State.NodeCount)throw new ArgumentException("Actual global source/node extent required.");
        if (intermediateState is not null && !ReferenceEquals(intermediateState.Policy, policy))
            throw new ArgumentException("Actual global intermediate pool/controller identity required.");
        if(publicationStorage is not null&&!publicationStorage.IsFor(policy))throw new ArgumentException("Foreign GLOBAL publication storage.");
        this.publicationStorage=publicationStorage;
        Policy=policy;nodes=source.Select(Copy).ToArray(); IntermediatePublicationState=intermediateState?.Copy();
        resultQueries=policy.Owner.CaptureResultQueryLease(token);
    }
    public LegacyMainDirectWordNodeResult GetNodeSnapshot(int node)
    {if((uint)node>=(uint)nodes.Length)throw new ArgumentOutOfRangeException(nameof(node));return Copy(nodes[node]);}
    public int[] CopyCumulativePathCosts()=>nodes.Select(n=>n.CumulativePathCost2520).ToArray();
    internal ReadOnlySpan<byte> FinalRecords=>nodes[^1].Records.Span;
    internal ushort FinalSourceEnd=>nodes[^1].SourceEnd23f0;
    internal int FinalCumulativeCost=>nodes[^1].CumulativePathCost2520;
    internal void ValidateResultQueryLease(CancellationToken token)=>Policy.Owner.ValidateResultQueryLease(resultQueries,token);
    internal ReadOnlyMemory<byte> ReadResultTerminal(ulong tag,ushort endingSymbol,byte boundaryArgument,CancellationToken token)
        =>Policy.Owner.ReadResultTerminal(resultQueries,tag,endingSymbol,boundaryArgument,token);
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
