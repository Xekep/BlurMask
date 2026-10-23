using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
namespace InkRecognition;
public sealed partial class LegacyEnglish119MainLocalCandidates
{
    internal LegacyEnglishCaseInitialFrontend RetainInitialConsumerSource(LegacyEnglish119MainKnowledge knowledge,CancellationToken token)
    {
        LegacyOrdinaryCursiveCandidateGraph graph;LegacyCursivePreparedInk ink;LegacyCursiveSeparationTable actualTable;
        token.ThrowIfCancellationRequested();
        lock(gate)
        {
            if(!ReferenceEquals(knowledge.Owner,this)||!complete||faulted||ranges is null||cuts is null)throw new InvalidOperationException("Same completed immutable MAIN cut/graph source required.");
            graph=ranges.Graph;ink=cuts.PreparedInk;actualTable=table;
        }
        // Every large scan/copy and cross-owner read is outside local gate.
        var next=LegacyEnglishCaseInitialFrontend.Create(knowledge,graph,ink,actualTable,token);
        ValidateInitialConsumerInputs(knowledge,graph,ink,actualTable,token);return next;
    }
    internal void ValidateInitialConsumerInputs(LegacyEnglish119MainKnowledge knowledge,LegacyOrdinaryCursiveCandidateGraph graph,LegacyCursivePreparedInk ink,LegacyCursiveSeparationTable actualTable,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {if(!complete||faulted||!ReferenceEquals(knowledge.Owner,this)||!ReferenceEquals(ranges?.Graph,graph)||!ReferenceEquals(cuts?.PreparedInk,ink)||!ReferenceEquals(table,actualTable))throw new InvalidOperationException("Exact retained actual source identity required.");}
    }
}
internal sealed class LegacyEnglishCaseInitialFrontend
{
    private readonly LegacyEnglish119MainKnowledge knowledge;
    private readonly LegacyOrdinaryCursiveCandidateGraph graph;
    private readonly LegacyCursivePreparedInk ink;
    private readonly LegacyCursiveSeparationTable table;
    private readonly float gapOffset,slope,height;
    private readonly int maximum;
    private readonly int[] rootArcs;
    private readonly Dictionary<ushort,LegacyNativeWordAcousticGroupCosts> costs=[];
    private readonly OrdinaryGroupReceipt[] metadata;
    // Receipt is issued only by the retained private MainLocal ordinary producer.
    // Allocated group identity is separate from arc flags and numeric NodeID.
    private sealed class OrdinaryGroupReceipt(LegacyOrdinaryCursiveCandidateGraph graph,int group)
    {
        internal void Require(LegacyOrdinaryCursiveCandidateGraph actual,int id)
        {if(!ReferenceEquals(graph,actual)||group!=id)throw new InvalidOperationException("Same ordinary allocated-group owner required.");}
    }
    private LegacyEnglishCaseInitialQueryPass? pass;
    internal LegacyCursiveCategoryProfile Categories{get;}
    internal LegacyNativeWordAcousticRequestScorer Kernel{get;}
    internal int ArcCount=>rootArcs.Length;
    internal ReadOnlySpan<int> RootArcs=>rootArcs;
    private LegacyEnglishCaseInitialFrontend(LegacyEnglish119MainKnowledge knowledge,LegacyOrdinaryCursiveCandidateGraph graph,LegacyCursivePreparedInk ink,LegacyCursiveSeparationTable table,float height,float gap,float slope,int maximum,int[] rootArcs,LegacyCursiveCategoryProfile categories,LegacyAcousticCandidateThresholds thresholds)
    {this.knowledge=knowledge;this.graph=graph;this.ink=ink;this.table=table;this.height=height;gapOffset=gap;this.slope=slope;this.maximum=maximum;this.rootArcs=rootArcs;metadata=Enumerable.Range(0,graph.Groups.Count).Select(id=>new OrdinaryGroupReceipt(graph,id)).ToArray();Categories=categories;Kernel=LegacyNativeWordAcousticRequestScorer.ForAlphabet(thresholds,knowledge.Source.Alphabet);}
    internal static LegacyEnglishCaseInitialFrontend Create(LegacyEnglish119MainKnowledge knowledge,LegacyOrdinaryCursiveCandidateGraph graph,LegacyCursivePreparedInk ink,LegacyCursiveSeparationTable table,CancellationToken token)
    {
        if(graph.NodeCount is<1 or>4096||graph.Groups.Count is<1 or>4096||graph.CandidateCount is<1 or>4096||ink.Points.Length is<1 or>4096||ink.StrokePointCounts.Length is<1 or>256||table.Values.Length!=256)throw new ArgumentException("Whole geometry/source budget exceeded before private pass claim.");
        int sum=0;foreach(int n in ink.StrokePointCounts.Span){token.ThrowIfCancellationRequested();if(n<1||n>4096-sum)throw new InvalidDataException("Whole physical partition required.");sum+=n;}if(sum!=ink.Points.Length)throw new InvalidDataException("Complete point partition required.");
        long ranges=0,visits=0,edges=0;int arcs=0,groupOrdinal=0;
        foreach(var group in graph.Groups)
        {
            token.ThrowIfCancellationRequested();if(group.GroupId!=groupOrdinal++||group.Ranges.Length is<1 or>256)throw new NotSupportedException("Bounded ordinary native range branch required.");
            foreach(var range in group.Ranges.Span)
            {if((uint)range.StrokeIndex>=(uint)ink.StrokePointCounts.Length||range.StartPoint<0||range.EndPointExclusive<=range.StartPoint||range.EndPointExclusive>ink.StrokePointCounts.Span[range.StrokeIndex])throw new InvalidDataException("Actual current-cut range required.");ranges++;visits+=range.EndPointExclusive-range.StartPoint;if(ranges>131072||visits>4194304)throw new ArgumentException("Whole range/point work cap exceeded before allocation.");}
        }
        for(int n=0;n<graph.NodeCount;n++)foreach(var arc in graph.GetNode(n))
        {token.ThrowIfCancellationRequested();arcs++;if(arcs>4096)throw new ArgumentException("Whole actual arc count cap exceeded.");if((uint)arc.GroupId>=(uint)graph.Groups.Count||arc.Span>n||arc.Span>7||(arc.Flags&8)!=0)throw new NotSupportedException("Sealed ordinary generated group branch required.");edges+=arc.PredecessorArcIndices.Length+arc.Successors.Count;if(edges>131072)throw new ArgumentException("Whole edge work cap exceeded before allocation.");}
        if(ranges>131072||visits>4194304||edges>131072||arcs!=graph.CandidateCount||400L*2*graph.GetNode(0).Count>4194304)throw new ArgumentException("Complete geometry/edge/scoring work cap exceeded before allocation.");
        foreach(var p in ink.Points.Span){token.ThrowIfCancellationRequested();if(!float.IsFinite(p.X)||!float.IsFinite(p.Y))throw new InvalidDataException("Finite current points required.");}
        var descriptor=knowledge.Owner.Owner.CopyDescriptor();var guide=knowledge.Owner.Owner.CopyGuide();
        if(descriptor.Length!=88||guide.Length!=40||BinaryPrimitives.ReadInt32LittleEndian(descriptor.AsSpan(16))<0||(BinaryPrimitives.ReadUInt16LittleEndian(descriptor)&15)!=0)throw new NotSupportedException("Actual assigned ordinary guide/descriptor required.");
        float height=BinaryPrimitives.ReadSingleLittleEndian(guide.AsSpan(4));if(!float.IsFinite(height)||height<=0)throw new InvalidDataException("Actual positive guide height required.");
        if(!SHA256.HashData(MemoryMarshal.AsBytes(table.Values.Span)).AsSpan().SequenceEqual(Convert.FromHexString("B23ED52A3A10808EAB597A8F371A7BBB66BDE2FAB5FE2666431B1B192ABC1BAF")))throw new InvalidDataException("Pinned native geometry table required.");
        var sampler=knowledge.MainWire.Sampler;
        byte[] SF(string name,int size=4){var f=sampler.Fields.Single(f=>f.Owner=="297"&&f.Destination==name);if(f.ElementSize!=size||f.Count!=1)throw new InvalidDataException("Complete sampler scalar required.");return f.CopyBytes();}
        float F(string name){float f=BinaryPrimitives.ReadSingleLittleEndian(SF(name));if(!float.IsFinite(f))throw new InvalidDataException("Finite sampler scalar required.");return f;}
        int maximum=BinaryPrimitives.ReadInt32LittleEndian(SF("118"));float gap=F("68"),slope=F("108");
        if(maximum is<0 or>0x4000000||slope<0||SF("134.present",1)[0]!=0||SF("1D0.present",1)[0]!=0)throw new NotSupportedException("Ordinary MAIN geometry branch required.");
        int T(string name){var f=knowledge.MainWire.Fields.Single(f=>f.Owner=="21D"&&f.Destination==name);if(f.ElementSize!=4||f.Count!=1)throw new InvalidDataException("Actual MAIN threshold field required.");return BinaryPrimitives.ReadInt32LittleEndian(f.CopyBytes());}
        var roots=new List<int>();for(int a=0;a<graph.GetNode(0).Count;a++)if(graph.GetNode(0)[a].Span==0&&graph.GetNode(0)[a].PredecessorArcIndices.Span.Contains(-1))roots.Add(a);
        var categories=sampler.ReadCategories(token);if(categories.BaseEntryCount!=217||categories.Prototypes.Length!=24)throw new InvalidDataException("Actual same MAIN category profile required.");
        return new(knowledge,graph,ink,table,height,gap,slope,maximum,roots.ToArray(),categories,new(T("F0"),T("F4"),T("F8")));
    }
    internal void Bind(LegacyEnglishCaseInitialQueryPass value)
    {if(pass is not null||!ReferenceEquals(value.Root.Knowledge,knowledge))throw new InvalidOperationException("One same-source fresh pass required.");pass=value;}
    internal void PrepareCurrentCosts(LegacyEnglishFreshCaseMainSource.InitialQueryLease lease,CancellationToken token)
    {
        Require(lease,token);if(costs.Count!=0)throw new InvalidOperationException("Only one native first D0 cost image may be installed.");
        var receipt=pass!.D0??throw new InvalidOperationException("Actual D0 receipt required.");var rows=receipt.CopySelectedRows();
        if(rows.Length!=rootArcs.Length)throw new NotSupportedException("Exact root successor/current pooled D0 membership required.");
        for(int i=0;i<rows.Length;i++)
        {
            token.ThrowIfCancellationRequested();var row=rows[i];int arc=rootArcs[i];var actual=graph.GetNode(0)[arc];metadata[actual.GroupId].Require(graph,actual.GroupId);
            if(row.Anchor!=0||row.ArcIndex!=arc||row.GroupId!=actual.GroupId||row.Span!=0||row.Costs.Length!=220)throw new InvalidDataException("Same actual ordered pooled selected rows required.");
            // Kind0/compact6=0 follows the private MainLocal ordinary0/-1 seed
            // producer and its same graph merge/prune lineage, not arc.Flags.
            int geometry=LegacyCursiveGroupSeparationCost.Evaluate(ink,graph.Groups[actual.GroupId].Ranges.Span,height,gapOffset,slope,table.Values.Span,maximum,cancellationToken:token);
            var geometryValues=new int[24];Array.Fill(geometryValues,geometry);
            costs.Add(checked((ushort)arc),new(row.Costs,geometryValues));
        }
        Require(lease,token);
    }
    internal LegacyNativeWordAcousticGroupCosts ReadCosts(int node,ushort arc)
    {if(node!=0||!costs.TryGetValue(arc,out var row))throw new InvalidOperationException("Current native root arc cache required.");return row;}
    internal int StartContext(ushort arc)=>unchecked((int)graph.GetNode(0)[arc].StartContextId);
    internal bool Suppressed(int node,ushort arc)
    {if(node!=0||!costs.ContainsKey(arc))throw new InvalidOperationException("Current selected ordinary source arc required.");var actual=graph.GetNode(node)[arc];metadata[actual.GroupId].Require(graph,actual.GroupId);return false;}
    internal void Require(LegacyEnglishFreshCaseMainSource.InitialQueryLease lease,CancellationToken token)
    {if(pass is null||!ReferenceEquals(lease.Owner,pass))throw new InvalidOperationException("Exact owning frontend lease required.");lease.Root.ValidateInitialQuery(lease);knowledge.Owner.ValidateInitialConsumerInputs(knowledge,graph,ink,table,token);}
}
