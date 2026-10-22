namespace InkRecognition;

/// <summary>Detached small result of one actual MAIN local arc. Scores are
///acoustic costs; this is not a global linguistic winner or accepted text.</summary>
public sealed class LegacyEnglishMainArcScores
{
    private readonly int[] costs;
    private readonly LegacyAcousticCacheCandidate[] candidates;
    public int GroupId{get;}
    public int Span{get;}
    public int FallbackCost{get;}
    public float PowerScale{get;}
    public int NegativeBaselineCost{get;}
    internal LegacyEnglishMainArcScores(int group,int span,LegacyOrdinaryAcousticClassCacheResult result,float power)
    {GroupId=group;Span=span;costs=result.Costs.ToArray();candidates=result.Candidates.ToArray();FallbackCost=result.FallbackCost;PowerScale=power;NegativeBaselineCost=(int)((float)-LegacyAcousticFixedLog.ProbabilityCost(1)*power);}
    public int[] CopyCosts()=>costs.ToArray();
    public LegacyAcousticCacheCandidate[] CopyCandidates()=>candidates.ToArray();
}

/// <summary>Actual completed MAIN119 local owner ? four actual MAIN107
///families ? fresh MAIN-only ordinary cache. No supplied vectors/maps/flags,
///native mutable pool, language query, controller search or text acceptance.</summary>
public sealed class LegacyEnglishMainOnlyAcousticScorer
{
    private readonly object gate=new();
    private readonly LegacyEnglishMainInkSnapshot ink;
    private readonly LegacyEnglishMainClassCacheKernel kernel;
    private readonly LegacyEnglishMainArcScores?[] cache;
    private readonly byte[] primary;
    public LegacyEnglishMainOnlySourceMasks Masks{get;}
    public LegacyEnglishMainAcousticModel Model{get;}
    public long LogicalCopiedPayloadEnvelopeBytes=>ink.LogicalReservationBytes;
    public int CachedGroupCount{get{lock(gate)return cache.Count(x=>x is not null);}}
    private LegacyEnglishMainOnlyAcousticScorer(LegacyEnglishMainOnlySourceMasks masks,Action<string>? checkpoint,CancellationToken token)
    {
        Masks=masks;token.ThrowIfCancellationRequested();
        //Reserve complete graph/snapshot/cache and transient workspace envelope
        //before copying matrices or graph payload. This is not process RSS.
        _=LegacyEnglishMainInkSnapshot.Preflight(masks.Knowledge.Owner.GroupCount,masks.Knowledge.Owner.NodeCount,masks.Knowledge.Owner.CandidateCount);
        checkpoint?.Invoke("prepare:preflight");token.ThrowIfCancellationRequested();
        Model=LegacyEnglishMainAcousticModel.Prepare(masks,token);checkpoint?.Invoke("prepare:model");token.ThrowIfCancellationRequested();ink=new(masks,token);checkpoint?.Invoke("prepare:ink");token.ThrowIfCancellationRequested();
        primary=masks.CopyPrimaryMask();cache=new LegacyEnglishMainArcScores?[ink.Ranges.Length];
        kernel=new(Model,masks);token.ThrowIfCancellationRequested();
    }
    public static LegacyEnglishMainOnlyAcousticScorer Prepare(LegacyEnglishMainOnlySourceMasks actualMasks,CancellationToken token=default)
    {ArgumentNullException.ThrowIfNull(actualMasks);return new(actualMasks,null,token);}
    internal static LegacyEnglishMainOnlyAcousticScorer PreparePrepared(LegacyEnglishMainOnlySourceMasks masks,Action<string> checkpoint,CancellationToken token)=>new(masks,checkpoint,token);
    private (int Group,int Span) Resolve(int node,int ordinal,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if((uint)node>=(uint)ink.Arcs.Length)throw new ArgumentOutOfRangeException(nameof(node));var arcs=ink.Arcs[node];
        if((uint)ordinal>=(uint)arcs.Length)throw new ArgumentOutOfRangeException(nameof(ordinal));var arc=arcs[ordinal];
        if((uint)arc.Group>=(uint)cache.Length||ink.Spans[arc.Group]!=arc.Span)throw new InvalidDataException("Actual unchanged local arc membership required.");
        return(arc.Group,arc.Span);
    }
    public LegacyEnglishMainArcScores ScoreArc(int node,int ordinal,CancellationToken token=default)
        =>ScoreArcCore(node,ordinal,null,token);
    internal LegacyEnglishMainArcScores ScoreArcPrepared(int node,int ordinal,Action<string> checkpoint,CancellationToken token)=>ScoreArcCore(node,ordinal,checkpoint,token);
    private LegacyEnglishMainArcScores ScoreArcCore(int node,int ordinal,Action<string>? checkpoint,CancellationToken token)
    {
        var key=Resolve(node,ordinal,token);lock(gate){token.ThrowIfCancellationRequested();if(cache[key.Group] is{} prior)return prior;}
        var computed=Compute(key.Group,key.Span,checkpoint,token);checkpoint?.Invoke("score:publish");token.ThrowIfCancellationRequested();
        lock(gate){token.ThrowIfCancellationRequested();return cache[key.Group]??=computed.Result;}
    }
    internal LegacyEnglishMainArcDiagnostic DiagnoseArc(int node,int ordinal,CancellationToken token=default)
    {var key=Resolve(node,ordinal,token);return Compute(key.Group,key.Span,null,token);}
    private LegacyEnglishMainArcDiagnostic Compute(int group,int span,Action<string>? checkpoint,CancellationToken token)
    {
        void Check(string stage){checkpoint?.Invoke(stage);token.ThrowIfCancellationRequested();}
        Check("score:enter");var features=LegacyEnglishMainGlyphFeatures.Prepare(ink,Model,group,token);Check("score:features");
        var networks=new LegacyEnglishMainNetworkResult[4];for(int i=0;i<4;i++){token.ThrowIfCancellationRequested();networks[i]=Model.Evaluate(i,features.Inputs[i],token);Check("score:network"+i);}
        int[] neural=Combine(networks,features.Dynamic.Mode2Weight,token);Check("score:ensemble");
        var result=kernel.Build(neural,features,span,token);Check("score:cache");
        float power=span==0?1f:(float)Math.Pow(span+1,(double)Masks.Knowledge.CachePower);
        token.ThrowIfCancellationRequested();return new(features,networks,neural,new(group,span,result,power));
    }
    private int[] Combine(LegacyEnglishMainNetworkResult[] networks,float weight,CancellationToken token)
        =>CombineCore(networks,weight,primary,Masks.Knowledge,token);
    // Pure numeric implementation: this result does not confer typed source authority.
    internal static int[] CombineCore(LegacyEnglishMainNetworkResult[] networks,float weight,
        byte[] primary,LegacyEnglish119MainKnowledge knowledge,CancellationToken token)
    {
        const int impossible=0x4000000;var costs=new int[182];float denominator=0;
        if(!float.IsFinite(weight)||weight<0||weight>1)throw new InvalidDataException("Actual257 mode2 weight domain required.");
        foreach(var network in networks)
        {
            token.ThrowIfCancellationRequested();denominator+=weight;
            for(int i=0;i<182;i++)if(primary[i]!=0)
            {int cost=LegacyAcousticFixedLog.ProbabilityCost(network.Probabilities[i]);costs[i]=checked(costs[i]+(weight==1f?cost:(int)((float)cost*weight)));}
        }
        //1B9E20 positive80 modifies the RAW weighted sum before division.
        int best=-1,bestCost=impossible;
        for(int i=0;i<182;i++)if(i!=181&&i!=180&&primary[i]!=0&&costs[i]<bestCost){best=i;bestCost=costs[i];}
        var alphabet=knowledge.Source.Alphabet;
        if(knowledge.RelatedCaseRelaxationEnabled&&best>=0&&alphabet.IsSymbolFlagged((ushort)best)&&alphabet.TryGetRelatedForms((ushort)best,0,out var forms))
        {
            int first=forms.First,second=forms.Second;
            if(first<182&&second<182&&primary[first]!=0&&primary[second]!=0)
            {
                float a=(denominator*knowledge.FirstRelatedCaseMultiplier)*8192f,b=(denominator*knowledge.SecondRelatedCaseMultiplier)*8192f;
                int pa=(int)(a<=0?a-.5f:a+.5f),pb=(int)(b<=0?b-.5f:b+.5f);
                if(costs[first]+pb<costs[second])costs[second]=costs[first]+pb;
                else if(costs[second]+pa<costs[first])costs[first]=costs[second]+pa;
            }
        }
        float inverse=denominator>0?1f/denominator:0;
        for(int i=0;i<182;i++)
        {token.ThrowIfCancellationRequested();costs[i]=denominator<=0||primary[i]==0?impossible:Math.Min(impossible,denominator==1f?costs[i]:denominator==2f?costs[i]>>1:(int)((float)costs[i]*inverse));}
        costs[181]=impossible;return costs;
    }
}
internal sealed record LegacyEnglishMainArcDiagnostic(LegacyEnglishMainGlyphFeatures Features,LegacyEnglishMainNetworkResult[] Networks,int[] NeuralCosts,LegacyEnglishMainArcScores Result);
