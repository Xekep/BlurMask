using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyMainGlobalResidual.Probe")]
namespace InkRecognition;

/// <summary>Ordinary1AC3C0 overlay on the actual shared global graph. Merged
/// ranges belong to node-local higher sources; originalLast retains the base
/// neural identity, live bbox, descriptor and assigned line.</summary>
public sealed partial class LegacyFreshMainGlobalResidualSource
{
    private readonly List<Extra>[] extras;
    private readonly object gate=new();
    private int totalExtras,totalRanges;
    public LegacyFreshMainGlobalSourceGeometry Geometry{get;}
    public LegacyFreshMainCompactOrdinaryGraph Graph=>Geometry.Graph;
    private LegacyFreshMainGlobalResidualSource(LegacyFreshMainGlobalSourceGeometry geometry)
    {Geometry=geometry;extras=Enumerable.Range(0,Graph.State.NodeCount).Select(_=>new List<Extra>()).ToArray();}
    public static LegacyFreshMainGlobalResidualSource Prepare(LegacyFreshMainGlobalSourceGeometry geometry,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(geometry);token.ThrowIfCancellationRequested();
        var policy=geometry.Policy;
        if(geometry.CollectionC1||policy.Owner.Model.SourceKind!=0x135||policy.Configuration.Trace2368!=0||
            policy.Configuration.Mode22f6!=0||policy.Refresh.KnowledgeOption22f8!=0)
            throw new NotSupportedException("Fresh ordinary135, no external knowledge99 wrapper required.");
        return new(geometry);
    }
    public ulong ResolveBaseReference(ulong reference,CancellationToken token=default)
    {
        lock(gate){var s=Decode(reference,token);int count=Graph.State.GetNode(s.Node).Count;
            return s.Arc<count?reference:extras[s.Node][s.Arc-count].OriginalLast;}
    }
    public LegacyInkRange[] CopySourceRanges(ulong reference,CancellationToken token=default)
    {
        lock(gate){var s=Decode(reference,token);int count=Graph.State.GetNode(s.Node).Count;
            return s.Arc<count?Geometry.CopyStoredGroupRanges(s.Node,s.Arc,token):extras[s.Node][s.Arc-count].Ranges.ToArray();}
    }
    public int ReadRangeCapacity(ulong reference,CancellationToken token=default)
    {
        lock(gate){var s=Decode(reference,token);int count=Graph.State.GetNode(s.Node).Count;
            if(s.Arc<count)throw new NotSupportedException("Native base range allocation capacity is not represented.");
            return extras[s.Node][s.Arc-count].Capacity;}
    }
    internal int ReadHigherRangeCount(ulong reference,CancellationToken token)
    {
        lock(gate)
        {
            var s=Decode(reference,token);int count=Graph.State.GetNode(s.Node).Count;
            return s.Arc<count?0:extras[s.Node][s.Arc-count].Ranges.Length;
        }
    }
    public LegacyNativeResidualBounds ReadBounds(ulong reference,CancellationToken token=default)
    {var b=LegacyNativeResidualSource.Decode(ResolveBaseReference(reference,token));return Geometry.ReadBounds(b.Node,b.Arc,token);}
    public LegacyNativeResidualMerge MergeSources(ReadOnlySpan<ulong> input,CancellationToken token=default)
    {
        lock(gate)
        {
            token.ThrowIfCancellationRequested();if(input.Length is <1 or >65536)throw new ArgumentException("Bounded nonempty source merge required.");
            if(input.Length==1){_=Decode(input[0],token);return new(true,input[0]);}
            ulong[] references=input.ToArray();var collected=new List<LegacyInkRange>();int capacity=0;
            foreach(ulong reference in references)
            {
                var source=Decode(reference,token);
                //Native concatenation reads base arc arrays BEFORE extra-cache lookup.
                if(source.Arc>=Graph.State.GetNode(source.Node).Count)
                    throw new NotSupportedException("Multi-input higher-source concatenation is outside the restored native base-array domain.");
                var ranges=Geometry.CopyStoredGroupRanges(source.Node,source.Arc,token);
                int required=checked(collected.Count+ranges.Length);
                if(required>1_048_576)throw new NotSupportedException("Portable merge range-work budget exceeded.");
                if(capacity<required)capacity=Math.Max(required,(int)(capacity*1.5));
                collected.AddRange(ranges);
            }
            var merged=LegacyOwnedRussianMainResidualSource.SortAndCoalesce(collected.ToArray(),token);
            var last=LegacyNativeResidualSource.Decode(references[^1]);int baseCount=Graph.State.GetNode(last.Node).Count;
            for(int i=0;i<extras[last.Node].Count;i++)
            {token.ThrowIfCancellationRequested();if(extras[last.Node][i].Ranges.AsSpan().SequenceEqual(merged))return new(true,Pack(last.Node,baseCount+i));}
            int matching=-1;
            for(int group=0;group<Graph.State.Groups.Count;group++)
            {
                token.ThrowIfCancellationRequested();
                if(Geometry.StoredGroupRangesEqual(group,merged,token)){matching=group;break;}
            }
            if(matching!=-1)
            {
                int low=int.MaxValue,high=int.MinValue;
                foreach(ulong reference in references)
                {var s=LegacyNativeResidualSource.Decode(reference);var a=Graph.State.GetNode(s.Node)[s.Arc];low=Math.Min(low,unchecked((int)a.StartContextId));high=Math.Max(high,unchecked((int)a.EndContextId));}
                for(int node=0;node<Graph.State.NodeCount;node++)for(int arc=0;arc<Graph.State.GetNode(node).Count;arc++)
                {token.ThrowIfCancellationRequested();var a=Graph.State.GetNode(node)[arc];if(a.GroupId==matching&&unchecked((int)a.StartContextId)==low&&unchecked((int)a.EndContextId)==high)return new(true,Pack(node,arc));}
            }
            if(baseCount+extras[last.Node].Count>=65536||totalExtras>=65536||merged.Length>1_048_576-totalRanges)
                throw new NotSupportedException("Portable higher-source storage budget exceeded.");
            token.ThrowIfCancellationRequested();int ordinal=baseCount+extras[last.Node].Count;
            extras[last.Node].Add(new(merged,capacity,references[^1]));totalExtras++;totalRanges+=merged.Length;
            return new(true,Pack(last.Node,ordinal));
        }
    }

    internal LegacyNativeResidualAssociationResult Associate(ReadOnlySpan<byte> rows72,ReadOnlySpan<ulong> residual,
        CancellationToken token=default)
    {
        lock(gate)
        {
            LegacyNativeResidualSource Base(LegacyNativeResidualSource s)=>LegacyNativeResidualSource.Decode(ResolveBaseReference(Pack(s.Node,s.Arc),token));
            var operations=new LegacyNativeResidualAssociationOperations(
                s=>{var b=Base(s);return Geometry.ReadKind(b.Node,b.Arc,token);},
                s=>{var b=Base(s);return Geometry.ReadLine(b.Node,b.Arc,token);},
                s=>ReadBounds(Pack(s.Node,s.Arc),token),line=>Geometry.ReadGuide(line,token),
                label=>Geometry.ReadAlphabetFlag(label,token),
                s=>{var b=Base(s);return Geometry.ReadTopMark(b.Node,b.Arc,token);},MergeSources);
            //Fresh constructor/property scope excludes2458B0's temporary99 override.
            return LegacyNativeResidualGlyphAssociation.Apply(rows72,residual,false,Geometry.CollectionC1,
                checked((ushort)Geometry.Policy.Owner.Model.AcousticAlphabet.LabelCount),operations,token);
        }
    }
    private LegacyNativeResidualSource Decode(ulong reference,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var s=LegacyNativeResidualSource.Decode(reference);
        if((uint)s.Node>=(uint)Graph.State.NodeCount||s.Arc>=Graph.State.GetNode(s.Node).Count+extras[s.Node].Count)
            throw new ArgumentException("Owned global base or allocated higher source reference required.");return s;
    }
    private sealed record Extra(LegacyInkRange[] Ranges,int Capacity,ulong OriginalLast);
    private static ulong Pack(int node,int arc)=>((uint)node&0xffffffUL)|((ulong)checked((ushort)arc)<<24);
}
