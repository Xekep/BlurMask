namespace InkRecognition;

//Retains the actual scheduler's allocator, never a fallback zero allocator.
//Its only products are numeric allocator residue; the separate lease union
//below owns all source70/language authority.
internal sealed class LegacyFreshMainGlobalPublicationStorage(
    LegacyFreshMainGlobalWordControllerPolicy policy,LegacyMainDirectStorageProvider allocator)
{
    internal bool IsFor(LegacyFreshMainGlobalWordControllerPolicy actual)=>ReferenceEquals(policy,actual);
    internal LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageKind kind,int node,int bytes,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(kind is not(LegacyMainDirectStorageKind.PublicationLanguageHeap or LegacyMainDirectStorageKind.PublicationCandidateHeap)||
            node<0||node>=policy.Owner.Graph.State.NodeCount||bytes is <1 or >2_000_000)
            throw new ArgumentException("Actual bounded GLOBAL publication allocation required.");
        var value=allocator(new(kind,node,bytes,ReadOnlyMemory<byte>.Empty,0));token.ThrowIfCancellationRequested();
        if(value.Storage.Length!=bytes)throw new ArgumentException("Publication allocator must supply the exact native extent.");
        return value;
    }
}
internal sealed class LegacyFreshMainGlobalPublicationContext
{
    private readonly LegacyFreshMainGlobalWordControllerPolicy policy;
    private readonly LegacyFreshMainGlobalPublicationStorage storage;
    private readonly LegacyFreshMainGlobalCaseSession.LiveQueryLease? live;
    private readonly LegacyFreshMainGlobalWordPassResult? result;
    private int nextNode;
    private bool failed;
    private LegacyFreshMainGlobalPublicationContext(LegacyFreshMainGlobalWordControllerPolicy policy,
        LegacyFreshMainGlobalPublicationStorage storage,LegacyFreshMainGlobalCaseSession.LiveQueryLease? live,
        LegacyFreshMainGlobalWordPassResult? result,int startingNode)
    {this.policy=policy;this.storage=storage;this.live=live;this.result=result;nextNode=startingNode;}
    internal static LegacyFreshMainGlobalPublicationContext ForLive(LegacyFreshMainGlobalWordControllerPolicy policy,
        LegacyFreshMainGlobalPublicationStorage storage,LegacyFreshMainGlobalCaseSession.LiveQueryLease lease,int startingNode,CancellationToken token)
    {
        if(!storage.IsFor(policy)||startingNode<0||startingNode>lease.Span.EndingNode)throw new ArgumentException("Actual GLOBAL publication storage/range required.");
        policy.Owner.ValidateLiveQueryLease(lease,token);return new(policy,storage,lease,null,startingNode);
    }
    internal static LegacyFreshMainGlobalPublicationContext ForResult(LegacyFreshMainGlobalWordPassResult result,int startingNode,CancellationToken token)
    {
        result.ValidateResultQueryLease(token);
        if(startingNode<0||startingNode>=result.NodeCount)throw new ArgumentOutOfRangeException(nameof(startingNode));
        return new(result.Policy,result.RequirePublicationStorage(token),null,result,startingNode);
    }
    internal bool IsFor(LegacyNativeWordControllerConfiguration configuration,LegacySymbolContextFactory contexts)
        =>configuration==policy.Configuration&&ReferenceEquals(contexts,policy.Owner.Contexts);
    private void Validate(CancellationToken token)
    {
        if(failed)throw new InvalidOperationException("Failed publication context cannot be reused.");
        if(live is not null)policy.Owner.ValidateLiveQueryLease(live,token);else result!.ValidateResultQueryLease(token);
        if(policy.Knowledge2540!=0||policy.HasAssociatedSource||policy.Configuration.Trace2368!=0)
            throw new NotSupportedException("Actual closed fresh/no-property GLOBAL publication required.");
    }
    internal sealed record Node(LegacyFreshMainGlobalPublicationLanguage Language,
        LegacyNativePublicationCandidateHeap Candidates,ulong LanguageAllocationIdentity,ulong CandidateAllocationIdentity);
    internal Node PrepareNode(int node,CancellationToken token)
    {
        try
        {
            Validate(token);
            if(node!=nextNode)throw new InvalidOperationException("Publication allocator nodes must follow actual207CB0 order.");
            int alternatives=policy.Configuration.PrimaryAlternativeCount2402;
            int candidateBytes=checked((alternatives*10+1)*56);
            int maximumEvaluations=checked(policy.Configuration.PrimaryArcLimit230a*alternatives);
            var lexical=storage.Allocate(LegacyMainDirectStorageKind.PublicationLanguageHeap,node,440,token);Validate(token);
            var language=live is not null?LegacyFreshMainGlobalPublicationLanguage.FromLive(policy,live,node,lexical.Storage.Span,
                maximumEvaluations,440,token):LegacyFreshMainGlobalPublicationLanguage.FromResult(result!,node,lexical.Storage.Span,
                maximumEvaluations,440,token);
            var candidate=storage.Allocate(LegacyMainDirectStorageKind.PublicationCandidateHeap,node,candidateBytes,token);Validate(token);
            var heap=new LegacyNativePublicationCandidateHeap(checked((ushort)alternatives),candidate.Storage.Span,candidateBytes,token);
            nextNode=checked(nextNode+1);return new(language,heap,lexical.Identity,candidate.Identity);
        }
        catch{failed=true;throw;}
    }
}
