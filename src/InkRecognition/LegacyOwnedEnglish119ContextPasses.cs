namespace InkRecognition;

/// <summary>Actual selected-front-end FIRST119 A8/B0/B8 producer. Owns the
/// completed primary98 and reuses the same pruned source and cached groups.
/// Does not select a terminal273 or publish English words/text.</summary>
public sealed class LegacyOwnedEnglish119ContextPasses
{
    public LegacyEnglish119PrimaryResult Primary {get;}
    public LegacyOwnedEnglish119FirstPolicy Policy=>Primary.Owner.Policy;
    public int ShortlistColumnsA0=>1;
    public int ContextColumnsA2=>1;
    public int MergedColumnsA4=>1;
    private readonly LegacyFirstAcousticShortlistTable[] shortlist;
    private readonly LegacyOrdinaryCandidateConnections connections;
    private readonly object gate=new();
    private LegacyEnglish119ContextResult? completed;

    private LegacyOwnedEnglish119ContextPasses(LegacyEnglish119PrimaryResult primary,
        CancellationToken token)
    {
        Primary=primary;var policy=Policy;var graph=policy.SourceRanges.Graph;
        if(!ReferenceEquals(primary.Owner.Owner,policy.Owner)||primary.Primary.NodeCount!=graph.NodeCount||
            primary.Primary.BeamWidth!=policy.PrimaryBeamWidth||policy.NativeModeE0)
            throw new ArgumentException("Same completed ordinary selected119 primary/source/cache required.");
        if((long)primary.Primary.NodeCount*primary.Primary.BeamWidth*(ContextColumnsA2+MergedColumnsA4)>1_048_576)
            throw new ArgumentException("Portable context cell budget exceeded.");
        //150D00 selected249 getter -> NONNULL original247 ->158DF0 pair bind
        //-> child1D8 ->156630 container14/16/18 ->1EF070 A0/A2/A4=1.
        //Standalone unbound249 would have A0=20 and is not this factory scope.
        connections=new(graph,token);shortlist=new LegacyFirstAcousticShortlistTable[graph.NodeCount];
        for(int n=0;n<shortlist.Length;n++)
        {
            token.ThrowIfCancellationRequested();int node=n;
            shortlist[n]=LegacyFirstAcousticShortlistTable.Build(primary.Primary.GetRowCopy(n),ShortlistColumnsA0,
                policy.Blank,policy.Closed,arc=>
                {
                    token.ThrowIfCancellationRequested();var group=policy.Owner.ScoreGroup(graph.GetNode(node)[arc].GroupId,token);
                    var entries=group.CopyEntries();return entries.AsSpan(0,Math.Min(20,entries.Length)).ToArray()
                        .Select(e=>new LegacyFirstAcousticShortlistCandidate(e.Symbol,e.NormalizedLikelihood)).ToArray();
                },cancellationToken:token);
        }
        token.ThrowIfCancellationRequested();
    }
    public static LegacyOwnedEnglish119ContextPasses Prepare(LegacyEnglish119PrimaryResult primary,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(primary);token.ThrowIfCancellationRequested();return new(primary,token);
    }
    public LegacyFirstAcousticShortlistTable GetA8(int node)
    {if((uint)node>=(uint)shortlist.Length)throw new ArgumentOutOfRangeException(nameof(node));return shortlist[node];}
    public LegacyEnglish119ContextResult Run(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {token.ThrowIfCancellationRequested();if(completed is not null)return completed;var result=RunCore(null,token);token.ThrowIfCancellationRequested();completed=result;return result;}
    }
    /// <summary>A fresh numeric run with scalar-only observation. Callbacks
    /// cannot provide success/costs; private context allocations stay private
    /// if an observer throws or cancellation interrupts the run.</summary>
    public LegacyEnglish119ContextResult RunWithObserver(Action<LegacyFirstBlankContextCorrection> observer,
        CancellationToken token=default)
    {ArgumentNullException.ThrowIfNull(observer);return RunCore(observer,token);}
    private LegacyEnglish119ContextResult RunCore(Action<LegacyFirstBlankContextCorrection>? observer,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var p=Policy;var geometry=Primary.Owner.Geometry;
        var result=LegacyFirstBlankContextKernel.Run(Primary.Primary,p.SourceRanges.Graph,connections,p.PrimaryBeamWidth,
            ContextColumnsA2,MergedColumnsA4,p.Blank,p.Shadda,p.Newline,p.Space,p.NonbreakingSpace,p.Closed,
            p.NativeModeE0,p.ExcludeConsecutiveBlank,ReadGroupCost,geometry.Correct118,geometry.Correct140,observer,token);
        return new(this,result);
    }
    internal int ReadGroupCost(int node,ushort arc,ushort symbol,byte composite,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var graph=Policy.SourceRanges.Graph;
        if((uint)node>=(uint)graph.NodeCount||arc>=graph.GetNode(node).Count||
            composite>=Policy.Categories.CompositeCategories.Length)throw new ArgumentOutOfRangeException(nameof(node));
        int prototype=Policy.Categories.CompositeCategories.Span[composite].GeometryIndex;
        return Policy.Owner.ScoreGroup(graph.GetNode(node)[arc].GroupId,token).ReadClassCost(symbol,ushort.MaxValue,prototype,true,false,token);
    }
}

/// <summary>Completed same-owner A8/B0/B8 stage. Public table/node access is
/// detached; the mutable symbolic factory is not exposed by this owner.</summary>
public sealed class LegacyEnglish119ContextResult
{
    public LegacyOwnedEnglish119ContextPasses Owner {get;}
    internal LegacyFirstOrdinaryContextPasses Passes {get;}
    internal LegacyEnglish119ContextResult(LegacyOwnedEnglish119ContextPasses owner,LegacyFirstOrdinaryContextPasses passes)
    {Owner=owner;Passes=passes;}
    public int NodeCount=>Passes.NodeCount;
    public int BeamWidth=>Passes.BeamWidth;
    public LegacyFirstAcousticRecordTable GetB0RowCopy(int node,int row)=>Passes.GetB0RowCopy(node,row);
    public LegacyFirstAcousticRecordTable GetB8RowCopy(int node,int row)=>Passes.GetB8RowCopy(node,row);
    public IReadOnlyList<LegacySymbolContextNode> CopySymbolContexts()=>Passes.Contexts.CopyNodes();
}
