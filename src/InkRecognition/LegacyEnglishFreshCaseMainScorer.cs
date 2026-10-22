namespace InkRecognition;

/// <summary>Actual case-M MAIN costs on the same completed graph. No supplied
///vectors, borrowed independent-scorer costs or word/controller authority.</summary>
public sealed class LegacyEnglishFreshCaseMainScorer
{
    private readonly LegacyEnglishMainAcousticInputs inputs;
    private LegacyEnglishMainInkSnapshot ink=null!;
    private LegacyEnglishMainClassCacheKernel kernel=null!;
    private LegacyEnglishMainArcScores?[] cache=[];
    private byte[] primary=[];
    public LegacyEnglishFreshCaseMainSource CaseSource{get;}
    public LegacyEnglishMainAcousticModel Model{get;private set;}=null!;
    public long LogicalCopiedPayloadEnvelopeBytes=>ink.LogicalReservationBytes;
    private LegacyEnglishFreshCaseMainScorer(LegacyEnglishFreshCaseMainSource source)
    {CaseSource=source;inputs=LegacyEnglishMainAcousticInputs.FromFreshCase(source);}
    public static LegacyEnglishFreshCaseMainScorer Prepare(LegacyEnglishFreshCaseMainSource source,CancellationToken token=default)
        =>PrepareCore(source,null,token);
    internal static LegacyEnglishFreshCaseMainScorer PrepareCore(LegacyEnglishFreshCaseMainSource source,Action<string>? observer,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(source);source.RequireCurrent();token.ThrowIfCancellationRequested();
        var local=source.Knowledge.Owner;_=LegacyEnglishMainInkSnapshot.Preflight(local.GroupCount,local.NodeCount,local.CandidateCount);
        var result=new LegacyEnglishFreshCaseMainScorer(source);bool claimed=false;
        try
        {
            source.ClaimScorer(result,token);claimed=true;observer?.Invoke("score:claimed");token.ThrowIfCancellationRequested();
            result.Model=LegacyEnglishMainAcousticModel.PrepareFreshCase(result.inputs,token);observer?.Invoke("score:model");token.ThrowIfCancellationRequested();
            result.ink=new(result.inputs,token);result.primary=result.inputs.CopyPrimaryMask();result.cache=new LegacyEnglishMainArcScores?[result.ink.Ranges.Length];result.kernel=new(result.Model,result.inputs);
            observer?.Invoke("score:ready");token.ThrowIfCancellationRequested();source.RequireCurrent();return result;
        }
        catch{if(claimed)source.PoisonScorer(result);throw;}
    }
    private (int Group,int Span) Resolve(int node,int ordinal,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if((uint)node>=(uint)ink.Arcs.Length)throw new ArgumentOutOfRangeException(nameof(node));var arcs=ink.Arcs[node];
        if((uint)ordinal>=(uint)arcs.Length)throw new ArgumentOutOfRangeException(nameof(ordinal));var arc=arcs[ordinal];
        if((uint)arc.Group>=(uint)cache.Length||ink.Spans[arc.Group]!=arc.Span)throw new InvalidDataException("Exact case graph/group/span required.");return(arc.Group,arc.Span);
    }
    public LegacyEnglishMainArcScores ScoreArc(int node,int ordinal,CancellationToken token=default)
        =>ScoreArcCore(node,ordinal,null,token);
    internal LegacyEnglishMainArcScores ScoreArcCore(int node,int ordinal,Action<string>? observer,CancellationToken token)
    {
        var key=Resolve(node,ordinal,token);var operation=CaseSource.BeginScore(this,token);
        try{var result=cache[key.Group];if(result is null){result=Compute(key.Group,key.Span,observer,token).Result;observer?.Invoke("score:publish");token.ThrowIfCancellationRequested();cache[key.Group]=result;}token.ThrowIfCancellationRequested();CaseSource.FinishScore(this,operation);return result;}
        catch{CaseSource.PoisonScorer(this);throw;}
    }
    internal LegacyEnglishMainArcDiagnostic DiagnoseArc(int node,int ordinal,CancellationToken token=default)
    {
        var key=Resolve(node,ordinal,token);var operation=CaseSource.BeginScore(this,token);
        try{var result=Compute(key.Group,key.Span,null,token);token.ThrowIfCancellationRequested();CaseSource.FinishScore(this,operation);return result;}
        catch{CaseSource.PoisonScorer(this);throw;}
    }
    internal LegacyEnglishMainArcScores ScoreForD0(int group,LegacyEnglishFreshCaseMainSource.D0Lease lease,CancellationToken token)
    {
        CaseSource.ValidateLease(lease);if(!ReferenceEquals(lease.Root,CaseSource)||!ReferenceEquals(lease.Owner.Scorer,this)||(uint)group>=(uint)cache.Length||ink.Spans[group]<0)throw new InvalidOperationException("Actual same-case D0 group required.");
        var result=cache[group]??=Compute(group,ink.Spans[group],null,token).Result;token.ThrowIfCancellationRequested();CaseSource.ValidateLease(lease);return result;
    }
    private LegacyEnglishMainArcDiagnostic Compute(int group,int span,Action<string>? observer,CancellationToken token)
    {
        void Check(string stage){observer?.Invoke(stage);token.ThrowIfCancellationRequested();}
        Check("score:enter");var features=LegacyEnglishMainGlyphFeatures.Prepare(ink,Model,group,token);Check("score:features");
        var networks=new LegacyEnglishMainNetworkResult[4];for(int i=0;i<4;i++){token.ThrowIfCancellationRequested();networks[i]=Model.Evaluate(i,features.Inputs[i],token);Check("score:network"+i);}
        int[] neural=LegacyEnglishMainOnlyAcousticScorer.CombineCore(networks,features.Dynamic.Mode2Weight,primary,inputs.Knowledge,token);Check("score:ensemble");
        var result=kernel.Build(neural,features,span,token);Check("score:cache");
        float power=span==0?1f:(float)Math.Pow(span+1,(double)inputs.Knowledge.CachePower);
        token.ThrowIfCancellationRequested();return new(features,networks,neural,new(group,span,result,power));
    }
}
