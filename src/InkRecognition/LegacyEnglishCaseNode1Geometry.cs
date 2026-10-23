using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
namespace InkRecognition;

/// <summary>Actual SAME-source MAIN297/108 active118/140 operands. Raw60 is
///signedINT, not an inactive NaN float. Does not widen old v106 admission.</summary>
internal sealed class LegacyEnglishCaseNode1GeometryProfile
{
    private readonly float[] matrix;
    public LegacyEnglish119MainKnowledge Knowledge{get;}
    public LegacySampler297WireProfile Sampler{get;}
    public bool PairEnabled{get;}
    public float PairScale{get;}
    public float PairOffset{get;}
    public float PairMaximum{get;}
    public int CrossLineCost{get;}
    public float GapOffset{get;}
    public float DefaultGap{get;}
    public float BlankThreshold{get;}
    public float TranslationThreshold{get;}
    public float GapSlope{get;}
    public float SourceGapScale{get;}
    public float SourceMatrixScale{get;}
    public int JoinMaximumCost{get;}
    public int MatrixMaximumCost{get;}
    public int BoundaryMaximum118{get;}
    public int BoundaryThreshold11C{get;}
    private LegacyEnglishCaseNode1GeometryProfile(LegacyEnglish119MainKnowledge policy,LegacySampler297WireProfile sampler,bool pair,
        float pairScale,float pairOffset,float pairMaximum,int crossLine,float gapOffset,float gap,float triple,float translation,float slope,float gapScale,float matrixScale,int joinMax,int matrixMax,int boundaryMax,int boundaryThreshold,float[] matrix)
    {Knowledge=policy;Sampler=sampler;PairEnabled=pair;PairScale=pairScale;PairOffset=pairOffset;PairMaximum=pairMaximum;CrossLineCost=crossLine;GapOffset=gapOffset;DefaultGap=gap;BlankThreshold=triple;TranslationThreshold=translation;GapSlope=slope;SourceGapScale=gapScale;SourceMatrixScale=matrixScale;JoinMaximumCost=joinMax;MatrixMaximumCost=matrixMax;BoundaryMaximum118=boundaryMax;BoundaryThreshold11C=boundaryThreshold;this.matrix=matrix;}
    public float[] CopyMatrix()=>matrix.ToArray();
    public LegacyCursiveCategoryProfile ReadCategories(CancellationToken token=default)=>Sampler.ReadCategories(token);
    public static LegacyEnglishCaseNode1GeometryProfile Prepare(LegacyEnglish119MainKnowledge policy,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(policy);Require(policy,token);var sampler=policy.MainWire.Sampler;
        if(!ReferenceEquals(sampler.Source,policy.Source)||sampler.Child.Version!=0x108||sampler.Child.Offset!=182412||sampler.Child.End!=183800)throw new NotSupportedException("Exact same actual MAIN108 sampler required.");
        LegacyResourceWireField Field(string name,int size,int count)
        {var f=sampler.Fields.Single(x=>x.Owner=="297"&&x.Destination==name);if(f.ElementSize!=size||f.Count!=count)throw new InvalidDataException("Whole typed108 fields required before copying.");return f;}
        string[] floats=["54","58","5C","68","6C","70","74","108","110","114"];
        string[] ints=["60","118","11C","120","124"];
        foreach(string n in floats.Concat(ints))_=Field(n,4,1);
        foreach(string n in new[]{"50","10C","134.present","1D0.present"})_=Field(n,1,1);
        for(int row=0;row<6;row++)_=Field($"{0x78+row*24:X}.matrixRow",4,6);
        byte[] Read(string name,int size=4,int count=1){token.ThrowIfCancellationRequested();return Field(name,size,count).CopyBytes();}
        float F(string name){float v=BinaryPrimitives.ReadSingleLittleEndian(Read(name));if(!float.IsFinite(v))throw new InvalidDataException("Finite ACTIVE108 operand required.");return v;}
        int I(string name)=>BinaryPrimitives.ReadInt32LittleEndian(Read(name));
        byte flag=Read("50",1)[0];if(flag!=1||Read("10C",1)[0]!=0||Read("134.present",1)[0]!=0||Read("1D0.present",1)[0]!=0)throw new NotSupportedException("Actual active pair with no optional108 branches required.");
        var matrix=new float[36];for(int row=0;row<6;row++){byte[] bytes=Read($"{0x78+row*24:X}.matrixRow",4,6);for(int col=0;col<6;col++){float v=BinaryPrimitives.ReadSingleLittleEndian(bytes.AsSpan(col*4));if(!float.IsFinite(v))throw new InvalidDataException("Finite matrix operand required.");matrix[row*6+col]=v;}}
        var result=new LegacyEnglishCaseNode1GeometryProfile(policy,sampler,true,F("54"),F("58"),F("5C"),I("60"),F("68"),F("6C"),F("70"),F("74"),F("108"),F("110"),F("114"),I("120"),I("124"),I("118"),I("11C"),matrix);
        if(result.PairMaximum<0||result.GapSlope<0||result.SourceGapScale<0||result.SourceMatrixScale<0||new[]{result.JoinMaximumCost,result.MatrixMaximumCost,result.BoundaryMaximum118,result.BoundaryThreshold11C}.Any(v=>v is<0 or>0x4000000))throw new NotSupportedException("Bounded ordinary108 cache configuration required.");
        Require(policy,token);return result;
    }
    internal void Validate(CancellationToken token=default)=>Require(Knowledge,token);
    private static void Require(LegacyEnglish119MainKnowledge knowledge,CancellationToken token)
    {token.ThrowIfCancellationRequested();if(!knowledge.Owner.IsComplete||knowledge.Owner.IsFaulted||!ReferenceEquals(knowledge.Source,knowledge.MainWire.Source))throw new InvalidOperationException("Same completed English MAIN source required.");}
}





public sealed partial class LegacyEnglish119MainLocalCandidates
{
    internal LegacyEnglishCaseNode1GeometrySource RetainNode1Source(LegacyEnglishCaseNode1GeometryProfile profile,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();profile.Validate(token);
        LegacyOrdinaryCursiveCandidateGraph graph;LegacyCursivePreparedInk ink;LegacyCursiveSeparationTable actualTable;float gap;
        lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(!ReferenceEquals(profile.Knowledge.Owner,this)||!complete||faulted||ranges is null||cuts is null||Owner.Owner.Gap is null)throw new InvalidOperationException("Actual completed SAME local/cuts/ranges/29B source required.");
            graph=ranges.Graph;ink=cuts.PreparedInk;actualTable=table;gap=Owner.Owner.Gap.Output.SourceGap;
        }
        //No binding/parent lock, point/range scan, hash or copy under local gate.
        var result=LegacyEnglishCaseNode1GeometrySource.Create(this,profile,graph,ink,actualTable,gap,token);
        profile.Validate(token);
        lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(!complete||faulted||ranges is null||cuts is null||!ReferenceEquals(graph,ranges.Graph)||!ReferenceEquals(ink,cuts.PreparedInk)||!ReferenceEquals(actualTable,table))throw new InvalidOperationException("Retained same immutable completed source changed before publication.");
        }
        return result;
    }
    internal void ValidateRetainedNode1Inputs(LegacyEnglishCaseNode1GeometryProfile profile,LegacyOrdinaryCursiveCandidateGraph graph,LegacyCursivePreparedInk ink,LegacyCursiveSeparationTable actualTable,float gap,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(!complete||faulted||ranges is null||cuts is null||Owner.Owner.Gap is null||!ReferenceEquals(profile.Knowledge.Owner,this)||!ReferenceEquals(graph,ranges.Graph)||!ReferenceEquals(ink,cuts.PreparedInk)||!ReferenceEquals(actualTable,table)||BitConverter.SingleToInt32Bits(gap)!=BitConverter.SingleToInt32Bits(Owner.Owner.Gap.Output.SourceGap))throw new InvalidOperationException("Exact retained graph/current-cut/table/29B input identity required; equal bytes do not confer source authority.");
        }
    }

}

/// <summary>Private source receipt from actual local owner; no supplied graph,
///source facts, guide, gap, descriptor or cache authority is accepted publicly.</summary>
internal sealed class LegacyEnglishCaseNode1GeometrySource
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
    internal LegacyEnglishCaseNode1GeometryProfile Profile{get;}
    internal LegacyOrdinaryCursiveCandidateGraph Graph{get;}
    internal LegacyFirstTransitionBounds[] Bounds{get;}
    internal bool[] WholeStarts{get;}
    internal float[] Table{get;}
    internal int[] Separation=[];
    internal float Height{get;}
    internal float Baseline{get;}
    internal float SourceGap{get;}
    internal int LineId{get;}
    internal ushort DescriptorFlags{get;}
    internal long LogicalAllocationEnvelope{get;}
    private LegacyEnglishCaseNode1GeometrySource(LegacyEnglish119MainLocalCandidates owner,LegacyEnglishCaseNode1GeometryProfile profile,
        LegacyOrdinaryCursiveCandidateGraph graph,LegacyFirstTransitionBounds[] boxes,bool[] whole,float[] table,float height,float baseline,float gap,int line,ushort flags,long bytes)
    {Owner=owner;Profile=profile;Graph=graph;Bounds=boxes;WholeStarts=whole;Table=table;Height=height;Baseline=baseline;SourceGap=gap;LineId=line;DescriptorFlags=flags;LogicalAllocationEnvelope=bytes;metadata=Enumerable.Range(0,graph.Groups.Count).Select(g=>new OrdinaryGroupMetadata(graph,g)).ToArray();}
    internal static LegacyEnglishCaseNode1GeometrySource Create(LegacyEnglish119MainLocalCandidates owner,LegacyEnglishCaseNode1GeometryProfile profile,
        LegacyOrdinaryCursiveCandidateGraph graph,LegacyCursivePreparedInk ink,LegacyCursiveSeparationTable table,float gap,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();owner.ValidateRetainedNode1Inputs(profile,graph,ink,table,gap,token);
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
        var separation=new int[graph.Groups.Count];
        foreach(var group in graph.Groups){token.ThrowIfCancellationRequested();separation[group.GroupId]=LegacyCursiveGroupSeparationCost.Evaluate(ink,group.Ranges.Span,height,profile.GapOffset,profile.GapSlope,values,profile.BoundaryMaximum118,cancellationToken:token);}
        profile.Validate(token);owner.ValidateRetainedNode1Inputs(profile,graph,ink,table,gap,token);var result=new LegacyEnglishCaseNode1GeometrySource(owner,profile,graph,boxes,whole,values,height,baseline,gap,line,flags,bytes);result.Separation=separation;return result;
    }
    internal void Validate(CancellationToken token)
    {Profile.Validate(token);if(!Owner.IsComplete||Owner.IsFaulted)throw new InvalidOperationException("Actual retained completed local source required.");}
}
internal sealed class LegacyEnglishCaseNode1Geometry
{
    private const int Impossible=0x4000000;
    internal LegacyOrdinaryCursiveCandidateGraph ActualGraph=>source.Graph;
    internal LegacyInkAlphabet Alphabet=>Profile.Knowledge.Source.Alphabet;
    internal LegacyCursiveCategoryProfile Categories=>categories;
    internal LegacyNativeWordAcousticGroupCosts CreateGroupCosts(int node,ushort arc,ReadOnlySpan<int> costs,CancellationToken token)
    {
        source.Validate(token);if(node!=1||source.Graph.GetNode(node)[arc].Span!=0||costs.Length!=220)throw new InvalidDataException("Same actual node1 span0/cache220 required.");
        var geometry=new int[24];Array.Fill(geometry,source.Separation[source.Graph.GetNode(node)[arc].GroupId]);return new(costs,geometry);
    }
    internal LegacyNativeHigherArcTransitionOperations Operations()=>LegacyNativeHigherArcTransitionOperations.ForOrdinaryConnections(connections,Calculate118,Validate140);

    private readonly LegacyEnglishCaseNode1GeometrySource source;
    private readonly LegacyOrdinaryCandidateConnections connections;
    private readonly LegacyCursiveCategoryProfile categories;
    private readonly float[][] future,prefix;
    private readonly int[][] join,triple,blank;
    private readonly int[][][] matrix;
    private int active;
    private bool faulted;
    public LegacyEnglishCaseNode1GeometryProfile Profile=>source.Profile;
    public LegacyEnglish119MainLocalCandidates Owner=>source.Owner;
    public long LogicalAllocationEnvelope=>source.LogicalAllocationEnvelope;
    public int NodeCount=>source.Graph.NodeCount;
    public bool IsFaulted=>Volatile.Read(ref faulted);
    private LegacyEnglishCaseNode1Geometry(LegacyEnglishCaseNode1GeometrySource source,CancellationToken token)
    {
        this.source=source;var graph=source.Graph;categories=Profile.ReadCategories(token);connections=new(graph,token);
        int count=graph.NodeCount;future=new float[count][];prefix=new float[count][];join=new int[count][];triple=new int[count][];blank=new int[count][];matrix=new int[count][][];
        for(int n=0;n<count;n++)
        {
            token.ThrowIfCancellationRequested();int length=graph.GetNode(n).Count;future[n]=new float[length];prefix[n]=new float[length];join[n]=new int[length];triple[n]=new int[length];blank[n]=new int[length];matrix[n]=new int[length][];
            for(int a=0;a<length;a++){matrix[n][a]=Enumerable.Repeat(Impossible,36).ToArray();join[n][a]=triple[n][a]=blank[n][a]=Impossible;}
        }
        //1941B0 backwards immediate-successor fold, then native forward prefix.
        for(int n=count-1;n>=0;n--)for(ushort a=0;a<future[n].Length;a++)
        {
            token.ThrowIfCancellationRequested();float low=Box(n,a).MinimumX;
            if(n!=graph.GetNode(n)[a].Span&&n+1<count)foreach(int next in connections.GetSuccessorOrdinals(n,a,0,token).Span)low=Math.Min(low,future[n+1][next]);
            future[n][a]=low;
        }
        float defaultGap=source.SourceGap>=0?source.SourceGap*Profile.SourceGapScale:Profile.DefaultGap;
        var margins=Profile.CopyMatrix();float scale=source.SourceGap<0||margins[0]==0?1:source.SourceGap*Profile.SourceMatrixScale/margins[0];
        for(int n=0;n<count;n++)for(int a=0;a<future[n].Length;a++)
        {
            token.ThrowIfCancellationRequested();var arc=graph.GetNode(n)[a];var box=Box(n,a);prefix[n][a]=box.MaximumX;
            if(n==arc.Span)continue;
            int previous=n-arc.Span-1;float high=-float.MaxValue;
            foreach(int prior in arc.PredecessorArcIndices.Span){token.ThrowIfCancellationRequested();if((uint)prior>=(uint)prefix[previous].Length)throw new InvalidDataException("Actual previous ordinal out of bounds.");high=Math.Max(high,prefix[previous][prior]);}
            if(high==-float.MaxValue)high=box.MaximumX;prefix[n][a]=Math.Max(box.MaximumX,high);
            float gap=(future[n][a]-high)/source.Height;
            join[n][a]=Math.Min(LegacyFirstOrdinaryTransitionGeometry.LogCost((gap-defaultGap)*-Profile.GapSlope,source.Table,true),Profile.JoinMaximumCost);
            triple[n][a]=Math.Min(LegacyFirstOrdinaryTransitionGeometry.LogCost((gap-defaultGap*3f)*-Profile.GapSlope,source.Table,true),Profile.JoinMaximumCost);
            blank[n][a]=LegacyCursiveBlankGapCost.Evaluate(box.MinimumX,box.MaximumX,high,source.Height,Profile.BlankThreshold,Profile.GapSlope,false,source.Table);
            for(int i=0;i<36;i++)
            {
                int cost=LegacyFirstOrdinaryTransitionGeometry.LogCost((gap-scale*margins[i])*Profile.GapSlope,source.Table,false);
                matrix[n][a][i]=i/6==5?unchecked(cost-1):i%6==5?cost:Math.Min(cost,Profile.MatrixMaximumCost);
            }
        }
        source.Validate(token);
    }
    public static LegacyEnglishCaseNode1Geometry Prepare(LegacyEnglishCaseNode1GeometryProfile profile,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(profile);profile.Validate(token);
        var actual=profile.Knowledge.Owner.RetainNode1Source(profile,token);
        return new(actual,token);
    }
    public LegacyEnglishMainWordTransitionState CopyState(int node,int arc,CancellationToken token=default)
    {
        source.Validate(token);if(IsFaulted)throw new InvalidOperationException("Faulted transition component.");var box=Box(node,arc);var actual=source.Graph.GetNode(node)[arc];
        return new(actual.GroupId,box,future[node][arc],prefix[node][arc],join[node][arc],triple[node][arc],blank[node][arc],matrix[node][arc].ToArray());
    }
    internal LegacyFirstTransitionBounds[] CopyPreparedGroupBounds(CancellationToken token=default)
    {source.Validate(token);if(IsFaulted)throw new InvalidOperationException("Faulted transition component.");return source.Bounds.ToArray();}
    public bool Calculate118(LegacyNativeWordRecombinationRequest request,CancellationToken token=default)=>Execute(request,false,token);
    public bool Validate140(LegacyNativeWordRecombinationRequest request,CancellationToken token=default)=>Execute(request,true,token);
    internal bool ExecutePrepared(LegacyNativeWordRecombinationRequest request,bool pair,Action<string> checkpoint,CancellationToken token=default)=>Execute(request,pair,token,checkpoint);
    private bool Execute(LegacyNativeWordRecombinationRequest r,bool pair,CancellationToken token,Action<string>? checkpoint=null)
    {
        ArgumentNullException.ThrowIfNull(r);token.ThrowIfCancellationRequested();source.Validate(token);
        if(Interlocked.CompareExchange(ref active,1,0)!=0)throw new InvalidOperationException("One serialized transition operation required.");
        try
        {
            if(IsFaulted)throw new InvalidOperationException("Faulted transition component.");
            checkpoint?.Invoke("transition:entered");token.ThrowIfCancellationRequested();
            Check(r,pair,token);checkpoint?.Invoke("transition:validated");token.ThrowIfCancellationRequested();int before=r.Cost.Value;bool accepted;
            if(pair)accepted=Pair(r,token);else accepted=Gap(r,token);
            checkpoint?.Invoke("transition:cost");token.ThrowIfCancellationRequested();
            //1525E0/1528C0 frontend accumulator delta, including rejected IMP.
            Write32(r,48,accepted?unchecked(r.ReadInt32(48)+r.Cost.Value-before):Impossible);
            checkpoint?.Invoke("transition:accumulated");token.ThrowIfCancellationRequested();source.Validate(token);return accepted;
        }
        catch{Volatile.Write(ref faulted,true);throw;}
        finally{Volatile.Write(ref active,0);}
    }
    private bool Gap(LegacyNativeWordRecombinationRequest r,CancellationToken token)
    {
        int n=r.ReadInt32(0),a=r.ReadUInt16(12),previous=r.ReadInt32(4),p=r.ReadUInt16(14);byte kind=r.ReadByte(28);
        if(kind==2&&!source.WholeStarts[source.Graph.GetNode(n)[a].GroupId]){r.Cost.Value=Impossible;return false;}
        int correction=join[n][a];
        if(kind==2)
        {
            int current=r.ReadInt32(20),prior=r.ReadInt32(24);if(current is<0 or>5||prior is<0 or>5)throw new NotSupportedException("Actual ordinary first-ID matrix values0..5 required.");
            correction=matrix[n][a][current+prior*6];
        }
        var currentBox=Box(n,a);var previousBox=Box(previous,p);
        float movement=Math.Max(currentBox.MinimumX-previousBox.MinimumX,currentBox.MaximumX-previousBox.MaximumX)/source.Height;
        float value=movement<=Profile.TranslationThreshold?(movement-Profile.TranslationThreshold)*Profile.GapSlope:0;
        if(!float.IsFinite(value)||value>=2147483648f||value<int.MinValue)throw new NotSupportedException("Native translation conversion outside certified domain.");
        r.Cost.Value=unchecked(r.Cost.Value-(int)value*8192+correction);token.ThrowIfCancellationRequested();return r.Cost.Value<r.ReadInt32(52);
    }
    private bool Pair(LegacyNativeWordRecombinationRequest r,CancellationToken token)
    {
        int n=r.ReadInt32(0),a=r.ReadUInt16(12),previous=r.ReadInt32(4);var current=Box(n,a);int currentGeometry=unchecked((sbyte)r.ReadByte(18));
        if((uint)currentGeometry>=(uint)categories.Prototypes.Length)throw new NotSupportedException("Actual signed current geometry index required.");
        int delta;
        if(r.ReadUInt16(38)==ushort.MaxValue)
        {
            var prototype=categories.Prototypes.Span[currentGeometry];float inverse=1f/source.Height;
            float residual=MathF.Abs(prototype.Second-(current.MaximumY-current.MinimumY)*inverse)+MathF.Abs(prototype.First-(current.MaximumY-source.Baseline)*inverse);
            float value=Math.Min(residual*Profile.PairScale,Profile.PairMaximum)*8192f;
            delta=Truncate(value<=0?value-.5f:value+.5f);
        }
        else
        {
            int previousGeometry=unchecked((sbyte)r.ReadByte(19));if((uint)previousGeometry>=(uint)categories.Prototypes.Length)throw new NotSupportedException("Actual signed previous geometry index required.");
            var prior=Box(previous,r.ReadUInt16(14));int ending=r.ReadInt32(8),endingArc=r.ReadUInt16(16);
            if(previous!=ending||r.ReadUInt16(14)!=endingArc)
            {
                //Native1ACFA0 expands the SOURCE bbox by ordinary ENDING bbox.
                var last=Box(ending,endingArc);prior=new(Math.Min(prior.MinimumX,last.MinimumX),Math.Min(prior.MinimumY,last.MinimumY),Math.Max(prior.MaximumX,last.MaximumX),Math.Max(prior.MaximumY,last.MaximumY));
            }
            delta=LegacyFirstOrdinaryTransitionGeometry.PairCost(current,prior,categories.Prototypes.Span[currentGeometry],categories.Prototypes.Span[previousGeometry],source.Height,Profile.PairScale,Profile.PairOffset,Profile.PairMaximum);
        }
        token.ThrowIfCancellationRequested();r.Cost.Value=unchecked(r.Cost.Value+delta);return r.Cost.Value<r.ReadInt32(52);
    }
    private void Check(LegacyNativeWordRecombinationRequest r,bool pair,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var graph=source.Graph;int n=r.ReadInt32(0),a=r.ReadUInt16(12);
        if((uint)n>=(uint)graph.NodeCount||(uint)a>=(uint)graph.GetNode(n).Count||r.ReadUInt32(32)!=graph.GetNode(n)[a].Span||r.ReadByte(28)is not(0 or 2)||r.ReadUInt32(64)!=((0xc0eU>>9)&15))throw new NotSupportedException("Actual bounded current arc/span/source kind and ordinary0/2 required.");
        source.RequireOrdinaryGroup(n,a);ushort symbol=r.ReadUInt16(36);RequireOrdinary(symbol);
        int previous=r.ReadInt32(4),ending=r.ReadInt32(8),p=r.ReadUInt16(14),last=r.ReadUInt16(16);
        if(r.ReadUInt16(38)==ushort.MaxValue)
        {
            if(!pair||previous!=-1||ending!=-1||p!=ushort.MaxValue||last!=ushort.MaxValue||r.ReadUInt32(68)!=0)throw new NotSupportedException("Actual ordinary initial140 endpoints required.");return;
        }
        RequireOrdinary(r.ReadUInt16(38));
        if((uint)previous>=(uint)graph.NodeCount||(uint)ending>=(uint)graph.NodeCount||previous>ending||ending!=n-graph.GetNode(n)[a].Span-1||(uint)p>=(uint)graph.GetNode(previous).Count||(uint)last>=(uint)graph.GetNode(ending).Count||r.ReadUInt32(68)!=r.ReadUInt32(64)||!graph.GetNode(n)[a].PredecessorArcIndices.Span.Contains(last))throw new NotSupportedException("Actual same-source previous SOURCE/ENDING and predecessor selector required.");
        source.RequireOrdinaryGroup(previous,p);source.RequireOrdinaryGroup(ending,last);
        if(pair&&symbol==Label(".")&&r.ReadUInt16(38)==symbol)throw new NotSupportedException("Reached native dot-pair special geometry requires its separate producer.");
    }
    private ushort Label(string text)=>Profile.Knowledge.Source.Alphabet.TryGetLabel(text,out ushort label)?label:ushort.MaxValue;
    private void RequireOrdinary(ushort symbol)
    {if(!LegacyNativeOrdinaryWordSymbols.Supports(Profile.Knowledge.Source.Alphabet,symbol)||symbol==Label("\u2610"))throw new NotSupportedException("Reached protocol/replacement/box geometry branch is outside this ordinary component.");}
    private LegacyFirstTransitionBounds Box(int n,int a)
    {if((uint)n>=(uint)source.Graph.NodeCount||(uint)a>=(uint)source.Graph.GetNode(n).Count)throw new ArgumentOutOfRangeException(nameof(n));return source.Bounds[source.Graph.GetNode(n)[a].GroupId];}
    private static int Truncate(float value)
    {if(!float.IsFinite(value)||value>=2147483648f||value<int.MinValue)throw new NotSupportedException("Native pair conversion outside certified domain.");return(int)value;}
    private static void Write32(LegacyNativeWordRecombinationRequest r,int at,int value)
    {Span<byte>b=stackalloc byte[4];BinaryPrimitives.WriteInt32LittleEndian(b,value);for(int i=0;i<4;i++)r.WriteByte(at+i,b[i]);}
}
