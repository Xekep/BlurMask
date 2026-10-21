using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglishMainD0.Probe")]
namespace InkRecognition;

public readonly record struct LegacyEnglishMainD0Bounds(int MaximumPreparations,int MaximumSelectedRows,long MaximumLogicalBytes=64L*1024*1024);

/// <summary>Owned fresh rebuildfalse MAIN D0. Genuine EN graph and pooled rows,
///not pristine-score substitution. Reuse=true/startup/controller remain separate.</summary>
public sealed partial class LegacyEnglishMainSpanSession
{
    private readonly object gate=new();
    private readonly LegacyOrdinaryCursiveCandidateGraph graph;
    private readonly (int Node,int Arc)[] representative;
    private readonly List<string> trace=[];
    private LegacyOrdinaryGraphAcousticPermissionSession? preparation;
    private LegacyPreparedDictionaryWorkingMap currentMap=null!;
    private LegacyEnglishMainD0Receipt? latest;
    private bool busy,faulted;
    private int preparations,selectedRows;
    public LegacyEnglishFreshDictionaryQuerySession QuerySession{get;}
    public LegacyEnglishMainOnlyAcousticScorer Scorer=>QuerySession.Scorer;
    public LegacyEnglishMainD0Bounds Bounds{get;}
    public long LogicalAllocationEnvelopeBytes{get;}
    public bool IsFaulted{get{lock(gate)return faulted;}}
    public int EnteredPreparations{get{lock(gate)return preparations;}}
    public int ConsumedSelectedRows{get{lock(gate)return selectedRows;}}
    public LegacyEnglishMainD0Receipt? LastCompletedReceipt{get{lock(gate)return latest;}}
    public string[] CopyTrace(){lock(gate)return trace.ToArray();}
    private LegacyEnglishMainSpanSession(LegacyEnglishFreshDictionaryQuerySession binding,LegacyEnglishMainD0Bounds bounds,
        LegacyOrdinaryCursiveCandidateGraph actual,(int,int)[] representatives,long envelope)
    {QuerySession=binding;Bounds=bounds;graph=actual;representative=representatives;LogicalAllocationEnvelopeBytes=envelope;}
    public static LegacyEnglishMainSpanSession Prepare(LegacyEnglishFreshDictionaryQuerySession binding,LegacyEnglishMainD0Bounds bounds,CancellationToken token=default)
        =>PrepareCore(binding,bounds,null,token);
    internal static LegacyEnglishMainSpanSession PrepareCore(LegacyEnglishFreshDictionaryQuerySession binding,LegacyEnglishMainD0Bounds bounds,Action<string>? observer,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(binding);token.ThrowIfCancellationRequested();
        var owner=binding.Scorer.Masks.Knowledge.Owner;
        long envelope=Preflight(owner.GroupCount,owner.NodeCount,owner.CandidateCount,bounds);
        if(bounds.MaximumPreparations>binding.Bounds.MaximumOperations)throw new ArgumentException("Complete source-operation capacity required before binding.");
        var graph=owner.RetainCompletedD0Graph(token);if(graph.MaximumAtoms!=8)throw new InvalidDataException("Actual shift1/cap8 MAIN graph required.");
        var rep=Enumerable.Repeat((-1,-1),graph.Groups.Count).ToArray();long arcs=0,ranges=0;
        for(int group=0;group<graph.Groups.Count;group++){token.ThrowIfCancellationRequested();ranges=checked(ranges+graph.Groups[group].Ranges.Length);if(ranges>131072)throw new InvalidDataException("Certified whole graph range bound exceeded.");}
        for(int node=0;node<graph.NodeCount;node++)foreach(var arc in graph.GetNode(node))
        {
            token.ThrowIfCancellationRequested();if((uint)arc.GroupId>=(uint)rep.Length||arc.Span>=8||++arcs>4096)throw new InvalidDataException("Actual bounded group/span domain required.");
        }
        for(int node=0;node<graph.NodeCount;node++)
        {var row=graph.GetNode(node);for(int ordinal=0;ordinal<row.Count;ordinal++){token.ThrowIfCancellationRequested();if(rep[row[ordinal].GroupId].Item1<0)rep[row[ordinal].GroupId]=(node,ordinal);}}
        var result=new LegacyEnglishMainSpanSession(binding,bounds,graph,rep,envelope);bool claimed=false;
        try
        {
            token.ThrowIfCancellationRequested();result.currentMap=binding.ClaimD0Owner(result,token);claimed=true;observer?.Invoke("owner:claimed");token.ThrowIfCancellationRequested();
            byte[] header=binding.Source.Alphabet.CopySerializedHeader();
            var reserved=LegacyPreparedAcousticDictionaryPermissions.ReadReservedSlots(binding.Source.Alphabet,header);
            result.preparation=new(graph,result.currentMap,reserved,(group,cancel)=>
            {
                cancel.ThrowIfCancellationRequested();var p=representativeFor(group.GroupId);var actual=binding.Scorer.ScoreArc(p.Node,p.Arc,cancel);
                return new(actual.CopyCosts(),actual.PowerScale);
            });
            (int Node,int Arc) representativeFor(int group)
            {if((uint)group>=(uint)rep.Length||rep[group].Item1<0)throw new InvalidDataException("Real live group representative required.");return rep[group];}
            observer?.Invoke("owner:ready");token.ThrowIfCancellationRequested();return result;
        }
        catch{if(claimed){result.faulted=true;binding.PoisonD0(result);}throw;}
    }
    public static long Preflight(int groups,int nodes,int candidates,LegacyEnglishMainD0Bounds b)
    {
        if(groups is<1 or>4096||nodes is<1 or>4096||candidates is<1 or>4096||b.MaximumPreparations is<1 or>4096||b.MaximumSelectedRows is<0 or>16384)
            throw new ArgumentOutOfRangeException(nameof(b),"Whole bounded actual graph and cumulative preparation domain required.");
        long bytes=checked(65536L+groups*(220L*4+64)+131072L*16+b.MaximumSelectedRows*(220L*4*3+64)+b.MaximumPreparations*1024L);
        if(bytes>b.MaximumLogicalBytes||bytes>64L*1024*1024)throw new ArgumentException("Whole D0 copied-payload/workspace budget exceeded.");return bytes;
    }
    public LegacyEnglishMainD0Receipt PrepareSpan(int anchor,byte span,CancellationToken token=default)=>PrepareSpanCore(anchor,span,false,null,token);
    internal LegacyEnglishMainD0Receipt PrepareSpanCore(int anchor,byte span,bool unsupportedReuse,Action<string>? observer,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(unsupportedReuse)throw new NotSupportedException("Native reused-record postpass costs require a separate proved receipt; this owner rebuilds only.");
        if((uint)anchor>=(uint)graph.NodeCount||span>=8)throw new ArgumentOutOfRangeException(nameof(anchor));
        int count=graph.GetNode(anchor).Count(a=>a.Span==span);bool entered=false;
        lock(gate)
        {
            token.ThrowIfCancellationRequested();if(faulted||busy)throw new InvalidOperationException("One live serialized D0 owner required.");
            if(preparations>=Bounds.MaximumPreparations||count>Bounds.MaximumSelectedRows-selectedRows)throw new ArgumentException("Complete remaining D0 work budget required before entry.");
            busy=true;
        }
        try
        {
            var lease=QuerySession.BeginD0(this,currentMap,token);entered=true;
            lock(gate){preparations++;selectedRows+=count;trace.Add("entered");}
            observer?.Invoke("span:entered");token.ThrowIfCancellationRequested();
            var pass=preparation!.Evaluate(anchor,span,false,token);observer?.Invoke("span:prepared");token.ThrowIfCancellationRequested();
            if(!pass.Succeeded||!pass.PermissionResult.Succeeded||pass.PermissionResult.WorkingMap is null||pass.SelectedRows.Count!=count||pass.SelectedPositions.Count!=count)
                throw new InvalidDataException("Complete actual D0 selection/source refresh required.");
            var rows=new LegacyEnglishMainD0SelectedRow[count];var eligibility=new byte[220];int selected=0;var arcs=graph.GetNode(anchor);
            for(int ordinal=0;ordinal<arcs.Count;ordinal++)
            {
                token.ThrowIfCancellationRequested();var arc=arcs[ordinal];if(arc.Span!=span)continue;var actual=pass.SelectedRows[selected];
                if(actual.Position!=new LegacyGraphGlyphPosition(anchor,ordinal)||pass.SelectedPositions[selected]!=actual.Position||actual.GroupId!=arc.GroupId||actual.Costs.Length!=220)
                    throw new InvalidDataException("Exact actual graph/pool selection required.");
                rows[selected++]=new(anchor,ordinal,arc,actual.Costs);
                for(int symbol=0;symbol<220;symbol++)if(actual.Costs.Span[symbol]<0x4000000)eligibility[symbol]=1;
            }
            var receipt=new LegacyEnglishMainD0Receipt(lease,anchor,span,rows,pass.PermissionResult,eligibility,pass.ComputedRows,pass.ReusedRows);
            observer?.Invoke("span:snapshot");token.ThrowIfCancellationRequested();QuerySession.CommitD0(lease,receipt,token);
            lock(gate)trace.Add("map:installed");observer?.Invoke("span:installed");token.ThrowIfCancellationRequested();
            currentMap=pass.PermissionResult.WorkingMap;lock(gate){latest=receipt;trace.Add("complete");}return receipt;
        }
        catch
        {
            if(entered){lock(gate){faulted=true;trace.Add("faulted");}QuerySession.PoisonD0(this);}throw;
        }
        finally{lock(gate)busy=false;}
    }
    internal bool IsCurrent(LegacyEnglishMainD0Receipt receipt)
    {lock(gate){if(faulted||busy||!ReferenceEquals(receipt.Owner,this)||!ReferenceEquals(latest,receipt))return false;}return QuerySession.IsCurrentD0(this,receipt.Epoch);}
}
