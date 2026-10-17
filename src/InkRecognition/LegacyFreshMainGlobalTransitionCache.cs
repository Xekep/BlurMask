using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
namespace InkRecognition;

public sealed record LegacyMainGlobalTransitionState(int DescriptorOrdinal,int LineId,float PrimaryHeight,
    LegacyMainGraphAppendBounds Bounds,float FutureMinimumX,float PrefixMaximumX,
    int JoinCost,int TripleJoinCost,int BlankCost,ReadOnlyMemory<int> CategoryMatrix);

/// <summary>Actual ordinary main1941B0 cache folds on the shared global graph.
/// Selector order and resolved source line IDs decide propagation. Transition
/// denominators use the current PRIMARY guide, separately from normalization
/// and193860 union height. No cross-descriptor status dispatcher is inferred.</summary>
public sealed class LegacyFreshMainGlobalTransitionCache
{
    private readonly LegacyMainGlobalTransitionState[][] states;
    public LegacyFreshMainGlobalCaseSession Owner {get;}
    public LegacyFreshMainCompactOrdinaryGraph Graph=>Owner.Graph;
    public LegacyMainWordTransitionProfile Profile {get;}
    private LegacyFreshMainGlobalTransitionCache(LegacyFreshMainGlobalCaseSession owner,
        LegacyMainWordTransitionProfile profile,LegacyMainGlobalTransitionState[][] states)
    {Owner=owner;Profile=profile;this.states=states;}

    public static LegacyFreshMainGlobalTransitionCache Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalCaseSession owner,LegacyCursiveSeparationTable table,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(owner);ArgumentNullException.ThrowIfNull(table);token.ThrowIfCancellationRequested();
        var source=owner.Graph.Source.Source;var graph=owner.Graph.State;
        if(resource.Length!=572763||!Convert.ToHexString(SHA256.HashData(resource)).Equals(source.SourceFingerprint,StringComparison.OrdinalIgnoreCase)||
            source.CategoryOwner.SelectedSourceOffset!=0||graph.NodeCount is <1 or >4096||graph.CandidateCount>131072)
            throw new ArgumentException("Actual bounded fresh main global graph/source resource required.");
        int sampler=source.CategoryOwner.SamplerNodeOffset;
        var profile=LegacyMainWordTransitionProfile.ReadV106Node(resource,sampler,token);
        _=LegacyCursiveCategoryProfile.Load(resource[(sampler+65)..],sampler+65,out int consumed,token);
        int tail=sampler+65+consumed;
        if(tail+214!=sampler+8+BinaryPrimitives.ReadInt32LittleEndian(resource[(sampler+4)..])||resource[tail+189]!=0)
            throw new NotSupportedException("Actual ordinary no10C main profile required.");
        float blankThreshold=BinaryPrimitives.ReadSingleLittleEndian(resource[(tail+33)..]);
        var values=table.Values.ToArray();
        if(!SHA256.HashData(MemoryMarshal.AsBytes(values.AsSpan())).AsSpan().SequenceEqual(Convert.FromHexString("B23ED52A3A10808EAB597A8F371A7BBB66BDE2FAB5FE2666431B1B192ABC1BAF")))
            throw new ArgumentException("Actual checked native grouping table required.");
        float sourceGap=source.Preparation.SourceGap;
        float defaultGap=sourceGap>=0?sourceGap*profile.SourceGapScale:profile.DefaultGap;
        float matrixScale=sourceGap<0||profile.Matrix.Span[0]==0?1:sourceGap*profile.SourceMatrixScale/profile.Matrix.Span[0];
        var rows=new LegacyMainGlobalTransitionState[graph.NodeCount][];
        for(int node=0;node<rows.Length;node++)
        {
            token.ThrowIfCancellationRequested();var arcs=graph.GetNode(node);rows[node]=new LegacyMainGlobalTransitionState[arcs.Count];
            for(int a=0;a<arcs.Count;a++)
            {
                var group=graph.Groups[checked((int)arcs[a].GroupId)];var primary=owner.Graph.ResolveGroupSource((int)arcs[a].GroupId,token).Primary;
                if(primary.Kind!=0||primary.LineId<0||!float.IsFinite(primary.Height)||primary.Height<=0)
                    throw new NotSupportedException("Actual assigned ordinary source guide required; unassigned/special policies are separate.");
                rows[node][a]=new(primary.DescriptorOrdinal,primary.LineId,primary.Height,group.Bounds,group.Bounds.MinimumX,group.Bounds.MaximumX,
                    0x4000000,0x4000000,0x4000000,Enumerable.Repeat(0x4000000,36).ToArray());
            }
        }
        for(int node=rows.Length-1;node>=0;node--)
            for(int a=0;a<rows[node].Length;a++)
            {
                token.ThrowIfCancellationRequested();var arc=graph.GetNode(node)[a];var current=rows[node][a];float low=current.Bounds.MinimumX;
                if(node!=arc.Span&&node+1<rows.Length)
                    foreach(ushort next in arc.CopySelector(1))
                    {
                        if(next>=rows[node+1].Length)throw new InvalidDataException("Actual successor ordinal escaped its graph.");
                        if(rows[node+1][next].LineId==current.LineId)low=Math.Min(low,rows[node+1][next].FutureMinimumX);
                    }
                rows[node][a]=current with {FutureMinimumX=low};
            }
        for(int node=0;node<rows.Length;node++)
            for(int a=0;a<rows[node].Length;a++)
            {
                token.ThrowIfCancellationRequested();var arc=graph.GetNode(node)[a];var current=rows[node][a];
                if(node==arc.Span)continue;
                int previous=node-arc.Span-1;if(previous<0)throw new InvalidDataException("Actual ordinary span exceeds its node prefix.");
                float high=-float.MaxValue;
                foreach(ushort prior in arc.CopySelector(0))
                {
                    if(prior>=rows[previous].Length)throw new InvalidDataException("Actual predecessor ordinal escaped its graph.");
                    if(rows[previous][prior].LineId==current.LineId)high=Math.Max(high,rows[previous][prior].PrefixMaximumX);
                }
                if(high==-float.MaxValue)high=current.Bounds.MaximumX;
                float gap=(current.FutureMinimumX-high)/current.PrimaryHeight;
                int join=Math.Min(LegacyFirstOrdinaryTransitionGeometry.LogCost((gap-defaultGap)*-profile.GapSlope,values,true),profile.JoinMaximumCost);
                int triple=Math.Min(LegacyFirstOrdinaryTransitionGeometry.LogCost((gap-defaultGap*3f)*-profile.GapSlope,values,true),profile.JoinMaximumCost);
                int blank=LegacyCursiveBlankGapCost.Evaluate(current.Bounds.MinimumX,current.Bounds.MaximumX,high,current.PrimaryHeight,
                    blankThreshold,profile.GapSlope,false,values);
                var matrix=new int[36];for(int i=0;i<matrix.Length;i++)
                {
                    int value=LegacyFirstOrdinaryTransitionGeometry.LogCost((gap-matrixScale*profile.Matrix.Span[i])*profile.GapSlope,values,false);
                    matrix[i]=i/6==5?unchecked(value-1):i%6==5?value:Math.Min(value,profile.MatrixMaximumCost);
                }
                rows[node][a]=current with {PrefixMaximumX=Math.Max(current.Bounds.MaximumX,high),JoinCost=join,TripleJoinCost=triple,
                    BlankCost=blank,CategoryMatrix=matrix};
            }
        token.ThrowIfCancellationRequested();return new(owner,profile,rows);
    }
    public LegacyMainGlobalTransitionState ReadState(int node,ushort ordinal,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();if((uint)node>=(uint)states.Length||ordinal>=states[node].Length)
            throw new ArgumentOutOfRangeException(nameof(ordinal));
        var row=states[node][ordinal];return row with {CategoryMatrix=row.CategoryMatrix.ToArray()};
    }
}
