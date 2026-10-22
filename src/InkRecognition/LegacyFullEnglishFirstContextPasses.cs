using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyFullEnglishFirstContext.Probe")]
namespace InkRecognition;

/// <summary>Actual full selected FIRST A8/B0/B8 only. Not the MAIN linguistic
/// decoder, terminal273, WORD/source720, final guide or application default.</summary>
public sealed class LegacyFullEnglishFirstContextPasses
{
    public LegacyFullEnglishFirstPrimary Owner{get;}
    private readonly object gate=new();
    private readonly LegacyFullEnglishFirstPrimaryChild[] children;
    private readonly LegacyFullEnglishFirstContextPolicy policy;
    private readonly LegacyFullEnglishFirstContextBudget budget;
    private readonly List<LegacyFullEnglishFirstContextChild> prefix=[];
    private bool entered,running,complete,faulted;
    public bool IsComplete{get{lock(gate)return complete;}}
    public bool IsFaulted{get{lock(gate)return faulted;}}
    public long AdditionalLogicalPeakBytes=>budget.Peak;
    public long LogicalCopyWorkBytes=>budget.Work;
    public long LogicalVisitWork=>budget.Visits;
    public int MaximumResolveAttempts=>budget.ResolveAttempts;
    private LegacyFullEnglishFirstContextPasses(LegacyFullEnglishFirstPrimary owner,long peak,long work,long visits,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(owner);token.ThrowIfCancellationRequested();Owner=owner;
        if(!owner.IsComplete||owner.IsFaulted||!owner.Owner.IsComplete||owner.Owner.IsFaulted||!owner.Owner.Owner.IsComplete||owner.Owner.Owner.IsFaulted||!owner.Owner.Owner.Parent.IsComplete||owner.Owner.Owner.Parent.IsFaulted)
            throw new ArgumentException("Complete full B3/B2/B1/A authority required.");
        children=owner.CopyCompletedChildren().ToArray();if(children.Length is <1 or >256)throw new ArgumentException("Actual full B3 child set required.");
        foreach(var child in children)
        {token.ThrowIfCancellationRequested();if(!ReferenceEquals(child.Owner,owner)||!ReferenceEquals(child.Initializer.Owner,owner.Owner)||!ReferenceEquals(child.Initializer.ActualCache.Owner.Child.Owner,owner.Owner.Owner.Parent))throw new ArgumentException("Same canonical full B3/B2 child required.");}
        budget=new(children,peak,work,visits,token);policy=new(owner,children[0],token);
        foreach(var child in children)if(!ReferenceEquals(child.Initializer.ActualCache.Model,policy.Primary.Profile.Model))throw new ArgumentException("Same actual full model required.");
    }
    public static LegacyFullEnglishFirstContextPasses Prepare(LegacyFullEnglishFirstPrimary completedB3,CancellationToken token=default)
        =>new(completedB3,LegacyFullEnglishFirstContextBudget.PeakLimit,LegacyFullEnglishFirstContextBudget.WorkLimit,LegacyFullEnglishFirstContextBudget.VisitLimit,token);
    internal static LegacyFullEnglishFirstContextPasses PrepareBounded(LegacyFullEnglishFirstPrimary owner,long peak,long work,long visits,CancellationToken token=default)=>new(owner,peak,work,visits,token);
    public void Run(CancellationToken token=default)=>RunCore(null,null,null,token);
    internal void RunPrepared(Action<string>? checkpoint,Action<LegacyFirstBlankContextCorrection>? observer,CancellationToken token=default,Action<LegacyFullEnglishFirstContextClassCost>? costObserver=null)=>RunCore(checkpoint,observer,costObserver,token);
    private void RunCore(Action<string>? checkpoint,Action<LegacyFirstBlankContextCorrection>? observer,Action<LegacyFullEnglishFirstContextClassCost>? costObserver,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();lock(gate){token.ThrowIfCancellationRequested();if(entered||running)throw new InvalidOperationException("B4 owner is one-use.");entered=running=true;}
        try
        {
            checkpoint?.Invoke("entry");token.ThrowIfCancellationRequested();int actualContexts=0;
            foreach(var child in children)
            {
                checkpoint?.Invoke("child:entry");token.ThrowIfCancellationRequested();var value=Build(child,checkpoint,observer,costObserver,token);
                actualContexts=checked(actualContexts+value.ContextCount);if(actualContexts>budget.ResolveAttempts)throw new InvalidDataException("Cumulative actual contexts exceed admitted attempted resolves.");
                lock(gate){token.ThrowIfCancellationRequested();prefix.Add(value);}
                checkpoint?.Invoke("child:published");token.ThrowIfCancellationRequested();
            }
            lock(gate){token.ThrowIfCancellationRequested();complete=true;}
        }
        catch{lock(gate)faulted=true;throw;}
        finally{lock(gate)running=false;}
    }
    private LegacyFullEnglishFirstContextChild Build(LegacyFullEnglishFirstPrimaryChild child,Action<string>? checkpoint,Action<LegacyFirstBlankContextCorrection>? observer,Action<LegacyFullEnglishFirstContextClassCost>? costObserver,CancellationToken token)
    {
        var bound=policy.Bind(child,token,costObserver);var graph=child.Initializer.ActualCache.Owner.SourceRanges.Graph;
        var geometry=new LegacyFullEnglishFirstPrimaryGeometry(policy.Primary,child.Initializer,token);
        var connections=new LegacyOrdinaryCandidateConnections(graph,token);var a8=new LegacyFirstAcousticShortlistTable[graph.NodeCount];
        for(int n=0;n<a8.Length;n++)
        {
            token.ThrowIfCancellationRequested();int node=n;
            a8[n]=LegacyFirstAcousticShortlistTable.Build(child.Primary.GetRowCopy(n),LegacyFullEnglishFirstContextPolicy.A0,policy.Primary.Blank,policy.Primary.Closed,arc=>bound.Candidates(node,arc,token),cancellationToken:token);
        }
        checkpoint?.Invoke("a8");token.ThrowIfCancellationRequested();var p=policy.Primary;
        var passes=LegacyFirstBlankContextKernel.Run(child.Primary,graph,connections,p.Width,LegacyFullEnglishFirstContextPolicy.A2,LegacyFullEnglishFirstContextPolicy.A4,p.Blank,p.Shadda,p.Newline,p.Space,p.NonbreakingSpace,p.Closed,
            false,p.ExcludeConsecutiveBlank,bound.Cost,geometry.Correct118,geometry.Correct140,observer,token);
        if(passes.Contexts.NodeCount>budget.ResolveAttempts)throw new InvalidDataException("Actual private context count exceeded admitted attempted resolves.");
        checkpoint?.Invoke("context");token.ThrowIfCancellationRequested();return new(this,child,a8,passes);
    }
    /// <summary>Immutable completed prefix; after fault it is historical data,
    /// never authority for a later terminal or publication stage.</summary>
    public IReadOnlyList<LegacyFullEnglishFirstContextChild> CopyCompletedChildren(){lock(gate)return Array.AsReadOnly(prefix.ToArray());}
}

/// <summary>Complete same-source A8/B0/B8 with private symbolic factory.
/// Returned tables, storage and nodes are defensive value snapshots.</summary>
public sealed class LegacyFullEnglishFirstContextChild
{
    public LegacyFullEnglishFirstContextPasses Owner{get;}
    public LegacyFullEnglishFirstPrimaryChild Primary{get;}
    private readonly LegacyFirstAcousticShortlistTable[] a8;
    private readonly LegacyFirstOrdinaryContextPasses passes;
    public int NodeCount=>passes.NodeCount;
    public int ContextCount=>passes.Contexts.NodeCount;
    public int ContextPageCount=>passes.Contexts.PageCount;
    public int ContextPageDescriptorCapacity=>passes.Contexts.PageDescriptorCapacity;
    public int CurrentContextPage=>passes.Contexts.CurrentPageIndex;
    public ulong ContextRoot=>passes.Contexts.RootTreeIdentity;
    internal LegacyFullEnglishFirstContextChild(LegacyFullEnglishFirstContextPasses owner,LegacyFullEnglishFirstPrimaryChild primary,LegacyFirstAcousticShortlistTable[] a8,LegacyFirstOrdinaryContextPasses passes)
    {Owner=owner;Primary=primary;this.a8=a8;this.passes=passes;}
    public LegacyFirstAcousticShortlistTable GetA8(int node)
    {if((uint)node>=(uint)a8.Length)throw new ArgumentOutOfRangeException(nameof(node));return a8[node];}
    public LegacyFirstAcousticRecordTable GetB0RowCopy(int node,int row)=>passes.GetB0RowCopy(node,row);
    public LegacyFirstAcousticRecordTable GetB8RowCopy(int node,int row)=>passes.GetB8RowCopy(node,row);
    public IReadOnlyList<LegacySymbolContextNode> CopySymbolContexts()=>passes.Contexts.CopyNodes();
    public int[] CopyContextPageUsedCounts()=>passes.Contexts.CopyPageUsedCounts();
}
