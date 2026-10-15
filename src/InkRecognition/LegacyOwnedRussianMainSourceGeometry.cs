using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Actual ordinary21B source geometry shared by delimiter and residual
/// consumers. Owns copies of sampled coordinates/ranges and one assigned guide.
/// Dynamic1AC3C0 higher arcs and nonordinary source groups remain separate.</summary>
public sealed class LegacyOwnedRussianMainSourceGeometry
{
    private readonly LegacyCursivePreparedPoint[] points;
    private readonly int[] strokeStarts;
    private readonly LegacyInkRange[][] ranges;
    private readonly LegacyNativeResidualBounds?[] bounds;
    private readonly object boundsGate=new();
    private readonly int line;
    private readonly LegacyNativeResidualGuide guide;
    public LegacyOwnedRussianMainGlyphConfidence Owner {get;}
    public LegacyOrdinaryCursiveCandidateGraph Graph=>Owner.Graph;
    public bool CollectionC1=>Owner.Model.SourceKind==0x134;

    private LegacyOwnedRussianMainSourceGeometry(ReadOnlySpan<byte> resource,
        LegacyOwnedRussianMainGlyphConfidence owner,LegacyOrdinaryGraphSourceRanges source,
        LegacyCursivePreparedInk ink,ReadOnlySpan<byte> descriptor,float baseline,float height,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();Owner=owner;
        if(!ReferenceEquals(owner.Graph,source.Graph)||owner.Model.SourceKind!=0x135||
            resource.Length!=572763||!StringComparer.OrdinalIgnoreCase.Equals(Convert.ToHexString(SHA256.HashData(resource)),
                "7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978"))
            throw new ArgumentException("Exact actual Russian main confidence, sampled graph and acoustic resource required.");
        if(descriptor.Length<44||(BinaryPrimitives.ReadUInt16LittleEndian(descriptor)&15)!=0||
            (line=BinaryPrimitives.ReadInt32LittleEndian(descriptor[16..]))<0||
            !float.IsFinite(baseline)||!float.IsFinite(height)||height<=0)
            throw new NotSupportedException("One actual ordinary assigned descriptor and finite owned guide required.");
        guide=new(baseline,height);points=ink.Points.ToArray();
        int[] counts=ink.StrokePointCounts.ToArray();strokeStarts=new int[counts.Length];
        for(int i=1;i<counts.Length;i++)strokeStarts[i]=checked(strokeStarts[i-1]+counts[i-1]);
        ranges=source.Graph.Groups.Select(g=>g.Ranges.ToArray()).ToArray();bounds=new LegacyNativeResidualBounds?[ranges.Length];
        foreach(var group in ranges)
        {
            token.ThrowIfCancellationRequested();if(group.Length==0)throw new InvalidDataException("Actual nonempty ordinary group ranges required.");
            foreach(var r in group)
                if((uint)r.StrokeIndex>=(uint)counts.Length||r.StartPoint<0||r.EndPointExclusive<=r.StartPoint||r.EndPointExclusive>counts[r.StrokeIndex])
                    throw new InvalidDataException("Sampled group must address the same actual prepared point owner.");
        }
    }

    public static LegacyOwnedRussianMainSourceGeometry Prepare(ReadOnlySpan<byte> resource,
        LegacyOwnedRussianMainGlyphConfidence owner,LegacyFreshSingleGlyphMainSampled source,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(owner);ArgumentNullException.ThrowIfNull(source);
        return new(resource,owner,source.SourceRanges,source.Cuts.PreparedInk,
            source.Preparation.LinePreparation.Output.CopyDescriptor().Span,source.SourceBaseline,source.SourceHeight,token);
    }
    public static LegacyOwnedRussianMainSourceGeometry Prepare(ReadOnlySpan<byte> resource,
        LegacyOwnedRussianMainGlyphConfidence owner,LegacyFreshMainSingleLineSampled source,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(owner);ArgumentNullException.ThrowIfNull(source);
        return new(resource,owner,source.SourceRanges,source.Cuts.PreparedInk,source.Owner.CopyDescriptor().Span,
            source.Owner.SourceBaseline,source.Owner.SourceHeight,token);
    }
    private int Group(int node,ushort arc,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if((uint)node>=(uint)Graph.NodeCount||arc>=Graph.GetNode(node).Count)
            throw new NotSupportedException("Actual ordinary graph arc required; dynamic higher arcs use a separate owner.");
        int id=Graph.GetNode(node)[arc].GroupId;
        if((uint)id>=(uint)ranges.Length)throw new InvalidDataException("Actual graph group identity required.");
        return id;
    }
    internal LegacyInkRange[] CopyGroupRanges(int node,ushort arc,CancellationToken token=default)
        =>ranges[Group(node,arc,token)].ToArray();
    internal bool GroupRangesEqual(int group,ReadOnlySpan<LegacyInkRange> expected,CancellationToken token=default)
    {token.ThrowIfCancellationRequested();if((uint)group>=(uint)ranges.Length)throw new ArgumentOutOfRangeException(nameof(group));return ranges[group].AsSpan().SequenceEqual(expected);}
    public byte ReadKind(int node,ushort arc,CancellationToken token=default){_=Group(node,arc,token);return 0;}
    public int ReadLine(int node,ushort arc,CancellationToken token=default){_=Group(node,arc,token);return line;}
    public LegacyNativeResidualGuide ReadGuide(int actualLine,CancellationToken token=default)
    {token.ThrowIfCancellationRequested();if(actualLine!=line)throw new ArgumentException("Same assigned source guide required.");return guide;}
    public bool ReadTopMark(int node,ushort arc,CancellationToken token=default)
    {
        int id=Group(node,arc,token);
        foreach(var range in ranges[id]){token.ThrowIfCancellationRequested();if(range.StartPoint!=0)return false;}return true;
    }
    public bool ReadAlphabetFlag(ushort label,CancellationToken token=default)
    {token.ThrowIfCancellationRequested();return Owner.Model.AcousticAlphabet.IsSymbolFlagged(label);}
    public LegacyNativeResidualBounds ReadBounds(int node,ushort arc,CancellationToken token=default)
    {
        int id=Group(node,arc,token);
        lock(boundsGate)
        {
            token.ThrowIfCancellationRequested();if(bounds[id] is {} cached)return cached;
            float x0=float.MaxValue,y0=float.MaxValue,x1=-float.MaxValue,y1=-float.MaxValue;
            foreach(var range in ranges[id])for(int p=range.StartPoint;p<range.EndPointExclusive;p++)
            {
                token.ThrowIfCancellationRequested();var point=points[strokeStarts[range.StrokeIndex]+p];
                x0=Math.Min(x0,point.X);y0=Math.Min(y0,point.Y);x1=Math.Max(x1,point.X);y1=Math.Max(y1,point.Y);
            }
            var result=new LegacyNativeResidualBounds(x0,y0,x1,y1);bounds[id]=result;return result;
        }
    }
}
