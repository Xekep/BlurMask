using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
namespace InkRecognition;

public sealed partial class LegacyEnglish119MainLocalCandidates
{
    internal LegacyEnglish119MainWordSource RetainWordSource(LegacyEnglishMainWordTransitionProfile profile,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();profile.Validate(token);
        LegacyOrdinaryCursiveCandidateGraph graph;LegacyCursivePreparedInk ink;LegacyCursiveSeparationTable actualTable;float gap;
        lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(!ReferenceEquals(profile.Policy.QuerySession.Masks.Knowledge.Owner,this)||!complete||faulted||ranges is null||cuts is null||Owner.Owner.Gap is null)throw new InvalidOperationException("Actual completed SAME local/cuts/ranges/29B source required.");
            graph=ranges.Graph;ink=cuts.PreparedInk;actualTable=table;gap=Owner.Owner.Gap.Output.SourceGap;
        }
        //No binding/parent lock, point/range scan, hash or copy under local gate.
        var result=LegacyEnglish119MainWordSource.Create(this,profile,graph,ink,actualTable,gap,token);
        profile.Validate(token);
        lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(!complete||faulted||ranges is null||cuts is null||!ReferenceEquals(graph,ranges.Graph)||!ReferenceEquals(ink,cuts.PreparedInk)||!ReferenceEquals(actualTable,table))throw new InvalidOperationException("Retained same immutable completed source changed before publication.");
        }
        return result;
    }
    internal void ValidateRetainedWordInputs(LegacyEnglishMainWordTransitionProfile profile,LegacyOrdinaryCursiveCandidateGraph graph,LegacyCursivePreparedInk ink,LegacyCursiveSeparationTable actualTable,float gap,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(!complete||faulted||ranges is null||cuts is null||Owner.Owner.Gap is null||!ReferenceEquals(profile.Policy.QuerySession.Masks.Knowledge.Owner,this)||!ReferenceEquals(graph,ranges.Graph)||!ReferenceEquals(ink,cuts.PreparedInk)||!ReferenceEquals(actualTable,table)||BitConverter.SingleToInt32Bits(gap)!=BitConverter.SingleToInt32Bits(Owner.Owner.Gap.Output.SourceGap))throw new InvalidOperationException("Exact retained graph/current-cut/table/29B input identity required; equal bytes do not confer source authority.");
        }
    }

}

/// <summary>Private source receipt from actual local owner; no supplied graph,
///source facts, guide, gap, descriptor or cache authority is accepted publicly.</summary>
internal sealed class LegacyEnglish119MainWordSource
{
    //Not arc.Flags: native19C4E0 copies wide+50 to compact+6. These receipts
    //exist only on MainLocal's sealed ordinary0/-1 seed→merge/prune producer.
    private sealed class OrdinaryGroupMetadata(LegacyOrdinaryCursiveCandidateGraph graph,int allocatedGroupId)
    {
        internal void Require(LegacyOrdinaryCursiveCandidateGraph actual,int group)
        {if(!ReferenceEquals(graph,actual)||group!=allocatedGroupId)throw new InvalidOperationException("Same allocated group metadata owner required.");RequirePreparedMetadata(0,0);}
    }
    private readonly OrdinaryGroupMetadata[] metadata;
    internal static void RequirePreparedMetadata(byte compactByte6,byte kind)
    {if(compactByte6!=0||kind!=0)throw new NotSupportedException("Nonordinary native group metadata requires its separate producer.");}
    internal void RequireOrdinaryGroup(int node,int arc)
    {int group=Graph.GetNode(node)[arc].GroupId;metadata[group].Require(Graph,group);}
    internal LegacyEnglish119MainLocalCandidates Owner{get;}
    internal LegacyEnglishMainWordTransitionProfile Profile{get;}
    internal LegacyOrdinaryCursiveCandidateGraph Graph{get;}
    internal LegacyFirstTransitionBounds[] Bounds{get;}
    internal bool[] WholeStarts{get;}
    internal float[] Table{get;}
    internal float Height{get;}
    internal float Baseline{get;}
    internal float SourceGap{get;}
    internal int LineId{get;}
    internal ushort DescriptorFlags{get;}
    internal long LogicalAllocationEnvelope{get;}
    private LegacyEnglish119MainWordSource(LegacyEnglish119MainLocalCandidates owner,LegacyEnglishMainWordTransitionProfile profile,
        LegacyOrdinaryCursiveCandidateGraph graph,LegacyFirstTransitionBounds[] boxes,bool[] whole,float[] table,float height,float baseline,float gap,int line,ushort flags,long bytes)
    {Owner=owner;Profile=profile;Graph=graph;Bounds=boxes;WholeStarts=whole;Table=table;Height=height;Baseline=baseline;SourceGap=gap;LineId=line;DescriptorFlags=flags;LogicalAllocationEnvelope=bytes;metadata=Enumerable.Range(0,graph.Groups.Count).Select(g=>new OrdinaryGroupMetadata(graph,g)).ToArray();}
    internal static LegacyEnglish119MainWordSource Create(LegacyEnglish119MainLocalCandidates owner,LegacyEnglishMainWordTransitionProfile profile,
        LegacyOrdinaryCursiveCandidateGraph graph,LegacyCursivePreparedInk ink,LegacyCursiveSeparationTable table,float gap,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();owner.ValidateRetainedWordInputs(profile,graph,ink,table,gap,token);
        if(graph.NodeCount is<1 or>4096||graph.Groups.Count is<1 or>4096||graph.CandidateCount is<1 or>4096||ink.Points.Length is<1 or>4096||ink.StrokePointCounts.Length is<1 or>256||table.Values.Length!=256)throw new ArgumentException("Whole bounded actual source/cache domain required before source allocation.");
        var counts=ink.StrokePointCounts.Span;int sum=0;foreach(int n in counts){token.ThrowIfCancellationRequested();if(n<1||n>4096-sum)throw new InvalidDataException("Whole current physical point partition required.");sum+=n;}
        if(sum!=ink.Points.Length)throw new InvalidDataException("Current prepared point partition mismatch.");
        long ranges=0,visits=0,edges=0,arcs=0;
        int groupOrdinal=0;foreach(var group in graph.Groups)
        {
            token.ThrowIfCancellationRequested();if(group.GroupId!=groupOrdinal++||group.Ranges.Length<1)throw new InvalidDataException("Actual nonempty group source required.");
            foreach(var range in group.Ranges.Span)
            {
                token.ThrowIfCancellationRequested();if((uint)range.StrokeIndex>=(uint)counts.Length||range.StartPoint<0||range.EndPointExclusive<=range.StartPoint||range.EndPointExclusive>counts[range.StrokeIndex])throw new InvalidDataException("Current cuts, not raw/precut indices, required.");
                ranges++;visits=checked(visits+range.EndPointExclusive-range.StartPoint);if(ranges>131072||visits>4194304)throw new ArgumentException("Complete group-range/point work budget exceeded before allocation.");
            }
        }
        for(int node=0;node<graph.NodeCount;node++)foreach(var arc in graph.GetNode(node))
        {
            token.ThrowIfCancellationRequested();arcs++;
            if((uint)arc.GroupId>=(uint)graph.Groups.Count||arc.Span>=8||arc.Span>node||(arc.Flags&8)!=0)throw new NotSupportedException("Actual bounded nonsuppressed ordinary same-descriptor arc required.");
            edges=checked(edges+arc.PredecessorArcIndices.Length+arc.Successors.Count);
            if(edges>131072)throw new ArgumentException("Whole graph-edge work budget exceeded.");
        }
        if(arcs!=graph.CandidateCount)throw new InvalidDataException("Actual graph arc count required.");
        //Conservative complete logical allocation allowance for source + successor
        //lookup + bounds/future/prefix/threecosts +36-int matrix and snapshots.
        long bytes=checked(131072L+graph.Groups.Count*128L+graph.NodeCount*256L+arcs*1024L+edges*32L);
        if(bytes>16L*1024*1024)throw new ArgumentException("Whole source/cache logical allocation cap exceeded.");
        //All graph/range/point/work/allocation preflight precedes the fixed
        //native88/40 descriptor/primary-guide snapshots as well as table/boxes.
        byte[] descriptor=owner.Owner.CopyDescriptor(),guide=owner.Owner.CopyGuide();
        if(descriptor.Length!=88||guide.Length!=40)throw new InvalidDataException("Actual complete88/40 source frame required.");
        int line=BinaryPrimitives.ReadInt32LittleEndian(descriptor.AsSpan(16));ushort flags=BinaryPrimitives.ReadUInt16LittleEndian(descriptor);
        float baseline=BinaryPrimitives.ReadSingleLittleEndian(guide);float height=BinaryPrimitives.ReadSingleLittleEndian(guide.AsSpan(4));
        if(line<0||(flags&15)!=0||!float.IsFinite(baseline)||!float.IsFinite(height)||height<=0||!float.IsFinite(gap))throw new NotSupportedException("Assigned ordinary primary SAME descriptor/guide branch required.");
        var values=table.Values.ToArray();
        if(!SHA256.HashData(MemoryMarshal.AsBytes(values.AsSpan())).AsSpan().SequenceEqual(Convert.FromHexString("B23ED52A3A10808EAB597A8F371A7BBB66BDE2FAB5FE2666431B1B192ABC1BAF")))throw new ArgumentException("Actual native separation lookup table required.");
        var offsets=new int[counts.Length];for(int i=1;i<offsets.Length;i++)offsets[i]=offsets[i-1]+counts[i-1];
        var boxes=new LegacyFirstTransitionBounds[graph.Groups.Count];var whole=new bool[boxes.Length];
        foreach(var group in graph.Groups)
        {
            token.ThrowIfCancellationRequested();float x0=float.MaxValue,y0=float.MaxValue,x1=-float.MaxValue,y1=-float.MaxValue;bool starts=true;
            foreach(var range in group.Ranges.Span)
            {
                starts&=range.StartPoint==0;int offset=offsets[range.StrokeIndex];
                for(int i=range.StartPoint;i<range.EndPointExclusive;i++)
                {
                    token.ThrowIfCancellationRequested();var p=ink.Points.Span[offset+i];if(!float.IsFinite(p.X)||!float.IsFinite(p.Y))throw new InvalidDataException("Actual finite current point required.");
                    x0=Math.Min(x0,p.X);y0=Math.Min(y0,p.Y);x1=Math.Max(x1,p.X);y1=Math.Max(y1,p.Y);
                }
            }
            boxes[group.GroupId]=new(x0,y0,x1,y1);whole[group.GroupId]=starts;
        }
        profile.Validate(token);owner.ValidateRetainedWordInputs(profile,graph,ink,table,gap,token);return new(owner,profile,graph,boxes,whole,values,height,baseline,gap,line,flags,bytes);
    }
    internal void Validate(CancellationToken token)
    {Profile.Validate(token);if(!Owner.IsComplete||Owner.IsFaulted)throw new InvalidOperationException("Actual retained completed local source required.");}
}

