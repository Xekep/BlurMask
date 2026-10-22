using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyFullEnglishFirstPrimary.Probe")]
namespace InkRecognition;

/// <summary>Full selected249 FIRST primary98 only. No context, B8, terminal,
/// WORD, source720, final guide or application-default authority is supplied.</summary>
public sealed class LegacyFullEnglishFirstPrimary
{
    public LegacyFullEnglishFirstControllerCosts Owner {get;}
    private readonly object gate=new();
    private readonly LegacyFullEnglishFirstControllerChild[] children;
    private readonly LegacyFullEnglishFirstPrimaryPolicy policy;
    private readonly LegacyFullEnglishFirstPrimaryBudget budget;
    private readonly List<LegacyFullEnglishFirstPrimaryChild> prefix=[];
    private bool entered,running,complete,faulted;
    public bool IsComplete{get{lock(gate)return complete;}}
    public bool IsFaulted{get{lock(gate)return faulted;}}
    public long AdditionalLogicalPeakBytes=>budget.Peak;
    public long LogicalCopyWorkBytes=>budget.Work;
    public long LogicalVisitWork=>budget.Visits;
    private LegacyFullEnglishFirstPrimary(LegacyFullEnglishFirstControllerCosts owner,long peak,long work,long visits,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(owner);token.ThrowIfCancellationRequested();Owner=owner;
        if(!owner.IsComplete||owner.IsFaulted||!owner.Owner.IsComplete||owner.Owner.IsFaulted||!owner.Owner.Parent.IsComplete||owner.Owner.Parent.IsFaulted)
            throw new ArgumentException("Complete full B2/B1/A authority required.");
        children=owner.CopyCompletedChildren().ToArray();if(children.Length is <1 or >256)throw new ArgumentException("Actual completed full children required.");
        budget=new(children,peak,work,visits,token);policy=new(owner,children[0],token);
        foreach(var child in children)
        {
            token.ThrowIfCancellationRequested();var c=child.ActualCache;
            if(!ReferenceEquals(child.Owner,owner)||!ReferenceEquals(c.Owner.Child.Owner,owner.Owner.Parent)||!ReferenceEquals(c.Model,policy.Profile.Model)||!ReferenceEquals(c.Owner.Sampler.Source,policy.Profile.Model.Source))
                throw new ArgumentException("Same canonical full child/model/source required.");
        }
    }
    public static LegacyFullEnglishFirstPrimary Prepare(LegacyFullEnglishFirstControllerCosts completedB2,CancellationToken token=default)
        =>new(completedB2,LegacyFullEnglishFirstPrimaryBudget.PeakLimit,LegacyFullEnglishFirstPrimaryBudget.WorkLimit,LegacyFullEnglishFirstPrimaryBudget.VisitLimit,token);
    internal static LegacyFullEnglishFirstPrimary PrepareBounded(LegacyFullEnglishFirstControllerCosts owner,long peak,long work,long visits,CancellationToken token=default)=>new(owner,peak,work,visits,token);
    public void Run(CancellationToken token=default)=>RunCore(null,null,token);
    internal void RunPrepared(Action<string>? checkpoint,Action<int,LegacyFirstAncestorTransitionRequest,int,LegacyFirstOrdinaryTransitionResult>? observer,CancellationToken token=default)=>RunCore(checkpoint,observer,token);
    private void RunCore(Action<string>? checkpoint,Action<int,LegacyFirstAncestorTransitionRequest,int,LegacyFirstOrdinaryTransitionResult>? observer,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();lock(gate){token.ThrowIfCancellationRequested();if(entered||running)throw new InvalidOperationException("B3 owner is one-use.");entered=running=true;}
        try
        {
            checkpoint?.Invoke("entry");token.ThrowIfCancellationRequested();
            foreach(var child in children)
            {
                checkpoint?.Invoke("child:entry");token.ThrowIfCancellationRequested();var result=Build(child,checkpoint,observer,token);
                lock(gate){token.ThrowIfCancellationRequested();prefix.Add(result);}
                checkpoint?.Invoke("child:published");token.ThrowIfCancellationRequested();
            }
            lock(gate){token.ThrowIfCancellationRequested();complete=true;}
        }
        catch{lock(gate)faulted=true;throw;}
        finally{lock(gate)running=false;}
    }
    private LegacyFullEnglishFirstPrimaryChild Build(LegacyFullEnglishFirstControllerChild child,Action<string>? checkpoint,
        Action<int,LegacyFirstAncestorTransitionRequest,int,LegacyFirstOrdinaryTransitionResult>? observer,CancellationToken token)
    {
        var c=child.ActualCache.Owner;var graph=c.SourceRanges.Graph;var categories=policy.Categories;
        var geometry=new LegacyFullEnglishFirstPrimaryGeometry(policy,child,token);
        var connections=new LegacyOrdinaryCandidateConnections(graph,token);
        var ordered=new LegacyFirstOrdinaryGlyphRequest[graph.NodeCount][];var ordinals=new ushort[graph.NodeCount][];
        //Copy each live ranked shortlist once; never rerun a network or copy on callbacks.
        var entries=new Dictionary<int,LegacyRankedAcousticEntry[]>();var ranked=new Dictionary<int,LegacyFullEnglishFirstRankedGroup>();
        foreach(int id in c.CopyLiveGroupIds())
        {
            token.ThrowIfCancellationRequested();var actual=child.ActualCache.ScoreGroup(id);var r=child.ReadGroup(actual);var e=r.CopyEntries();
            if(e.Length>150||!ReferenceEquals(r.Owner,Owner))throw new InvalidDataException("Canonical bounded full ranked group required.");entries.Add(id,e);ranked.Add(id,r);
        }
        int total=0,blanks=0;
        for(int n=0;n<graph.NodeCount;n++)
        {
            token.ThrowIfCancellationRequested();var requests=new List<LegacyFirstOrdinaryGlyphRequest>();var shortlist=new List<ushort>();var arcs=graph.GetNode(n);
            for(byte span=0;span<=7;span++)for(ushort a=0;a<arcs.Count;a++)
            {
                token.ThrowIfCancellationRequested();if(arcs[a].Span!=span)continue;int id=arcs[a].GroupId;var e=entries[id];var group=ranked[id];int top=Math.Min(20,e.Length);
                bool hasSpace=false;for(int i=0;i<top;i++)if(e[i].Symbol==policy.Space)hasSpace=true;
                for(int i=0;i<top;i++)
                {
                    ushort symbol=e[i].Symbol;
                    if(symbol==policy.Shadda||symbol==policy.Newline||symbol==policy.Space||hasSpace&&symbol==policy.NonbreakingSpace||n==span&&symbol==policy.Closed)continue;
                    for(int end=0;end<2;end++)
                    {
                        if(symbol>=categories.StartCompositeMap.Length||symbol>=categories.EndCompositeMap.Length)throw new InvalidDataException("Actual full symbol category domain required.");
                        int category=(end==0?categories.StartCompositeMap:categories.EndCompositeMap).Span[symbol];if(category<0)continue;Add(symbol,category,checked((ushort)i));
                    }
                }
                if(policy.HasBlankHeader&&span<n)
                {int category=categories.StartCompositeMap.Span[policy.Blank];if(category<0)throw new NotSupportedException("Actual blank category required.");Add(policy.Blank,category,ushort.MaxValue);blanks++;}
                void Add(ushort symbol,int category,ushort ordinal)
                {
                    token.ThrowIfCancellationRequested();if((uint)category>=(uint)categories.CompositeCategories.Length||(symbol==policy.Blank?categories.CompositeCategories.Span[category].FirstId!=6:categories.CompositeCategories.Span[category].FirstId is <0 or >5))throw new NotSupportedException("Resolved actual regular0..5/blank6 category required.");
                    int prototype=categories.CompositeCategories.Span[category].GeometryIndex;
                    if((uint)prototype>=(uint)categories.Prototypes.Length)throw new InvalidDataException("Actual full geometry category required.");
                    if(++total>41*budget.Arcs)throw new InvalidDataException("Derived request bound exceeded.");
                    requests.Add(new(a,span,symbol,checked((byte)category),group.PreparedClassQuery(symbol,ordinal,prototype,true,false,token),group.PreparedLikelihood(symbol,token)));shortlist.Add(ordinal);
                }
            }
            ordered[n]=requests.ToArray();ordinals[n]=shortlist.ToArray();
        }
        checkpoint?.Invoke("requests");token.ThrowIfCancellationRequested();
        var p=policy;var rows=LegacyFirstPrimaryRecurrenceKernel.Run(ordered,p.Width,p.Blank,p.Replacement,p.Closed,p.Shadda,p.Newline,p.Space,p.NonbreakingSpace,
            p.ExcludeConsecutiveBlank,connections,geometry.Correct118,geometry.Correct140,observer,token);
        checkpoint?.Invoke("primary");token.ThrowIfCancellationRequested();return new(this,child,rows,ordered,ordinals,total,blanks);
    }
    /// <summary>Detached complete child prefix. After fault it remains historical
    /// data; it supplies no authority to start later context/terminal stages.</summary>
    public IReadOnlyList<LegacyFullEnglishFirstPrimaryChild> CopyCompletedChildren(){lock(gate)return Array.AsReadOnly(prefix.ToArray());}
}

/// <summary>Complete canonical full child primary98, not a final word.</summary>
public sealed class LegacyFullEnglishFirstPrimaryChild
{
    public LegacyFullEnglishFirstPrimary Owner{get;}
    public LegacyFullEnglishFirstControllerChild Initializer{get;}
    public LegacyFirstOrdinaryAcousticRecurrence Primary{get;}
    public int RequestCount{get;}
    public int FallbackProposalCount{get;}
    private readonly LegacyFirstOrdinaryGlyphRequest[][] requests;
    private readonly ushort[][] ordinals;
    internal LegacyFullEnglishFirstPrimaryChild(LegacyFullEnglishFirstPrimary owner,LegacyFullEnglishFirstControllerChild child,LegacyFirstOrdinaryAcousticRecurrence rows,
        LegacyFirstOrdinaryGlyphRequest[][] requests,ushort[][] ordinals,int count,int blanks)
    {Owner=owner;Initializer=child;Primary=rows;this.requests=requests;this.ordinals=ordinals;RequestCount=count;FallbackProposalCount=blanks;}
    public LegacyFirstOrdinaryGlyphRequest[] CopyRequests(int node)
    {if((uint)node>=(uint)requests.Length)throw new ArgumentOutOfRangeException(nameof(node));return requests[node].ToArray();}
    internal ushort[] CopyShortlistOrdinals(int node)
    {if((uint)node>=(uint)ordinals.Length)throw new ArgumentOutOfRangeException(nameof(node));return ordinals[node].ToArray();}
}
