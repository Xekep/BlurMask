using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyMainGlobalSourceCallbacks.Probe")]

namespace InkRecognition;

/// <summary>Actual global ordinary source geometry for frontend callbacks.
/// Reads the same live group bbox owner used by118/140, actual descriptor line
/// and guide0/4. Cross-source normalization union frames are not source guides.</summary>
public sealed class LegacyFreshMainGlobalSourceGeometry
{
    private readonly LegacyFreshMainGlobalWordTransitions transitions;
    private readonly LegacyNativeResidualGuide[] guides;
    private readonly LegacyInkRange[][] ranges;
    public LegacyFreshMainGlobalWordControllerPolicy Policy{get;}
    public LegacyFreshMainGlobalGlyphConfidence Confidence{get;}
    public LegacyFreshMainCompactOrdinaryGraph Graph=>Confidence.Graph;
    public bool CollectionC1=>Confidence.Owner.Model.SourceKind==0x134;
    private LegacyFreshMainGlobalSourceGeometry(LegacyFreshMainGlobalWordControllerPolicy policy,
        LegacyFreshMainGlobalGlyphConfidence confidence,LegacyFreshMainGlobalWordTransitions transitions,
        LegacyNativeResidualGuide[] guides,LegacyInkRange[][] ranges)
    {Policy=policy;Confidence=confidence;this.transitions=transitions;this.guides=guides;this.ranges=ranges;}
    public static LegacyFreshMainGlobalSourceGeometry Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalWordControllerPolicy policy,LegacyFreshMainGlobalGlyphConfidence confidence,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(policy);ArgumentNullException.ThrowIfNull(confidence);token.ThrowIfCancellationRequested();
        var graph=policy.Owner.Graph;var source=graph.Source.Source;var transitions=policy.Services.GlobalTransitions;
        if(!ReferenceEquals(confidence.Owner,policy.Owner)||!ReferenceEquals(confidence.Graph,graph)||
            transitions is null||!ReferenceEquals(transitions.Graph,graph)||!ReferenceEquals(transitions.Cache.Owner,policy.Owner)||
            policy.Owner.Model.SourceKind!=0x135||resource.Length!=572763||!Convert.ToHexString(SHA256.HashData(resource)).Equals(source.SourceFingerprint,StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("One actual global policy, confidence, live transition geometry and pinned source required.");
        var rows=source.CopyGuides();var guides=new LegacyNativeResidualGuide[rows.Length];
        for(int i=0;i<rows.Length;i++)
        {
            token.ThrowIfCancellationRequested();if(rows[i].Length!=40)throw new InvalidDataException("Actual source218 forty-byte guide required.");
            float baseline=BinaryPrimitives.ReadSingleLittleEndian(rows[i]),height=BinaryPrimitives.ReadSingleLittleEndian(rows[i].AsSpan(4));
            if(!float.IsFinite(baseline)||!float.IsFinite(height)||height<=0)throw new NotSupportedException("Finite assigned source guides required.");
            guides[i]=new(baseline,height);
        }
        var ranges=graph.State.Groups.Select(g=>g.CopyRanges()).ToArray();
        for(int group=0;group<ranges.Length;group++)
        {
            token.ThrowIfCancellationRequested();var actual=graph.ResolveGroupSource(group,token);
            if((uint)actual.Primary.LineId>=(uint)guides.Length||ranges[group].Length==0)
                throw new InvalidDataException("Actual global group descriptor/line/ranges required.");
        }
        return new(policy,confidence,transitions,guides,ranges);
    }
    internal int Group(int node,ushort arc,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var row=Graph.State.GetNode(node);
        if(arc>=row.Count)throw new NotSupportedException("Actual ordinary global arc required; higher overlays have a separate owner.");
        int group=checked((int)row[arc].GroupId);
        if((uint)group>=(uint)ranges.Length)throw new InvalidDataException("Owned global group allocation required.");return group;
    }
    public byte ReadKind(int node,ushort arc,CancellationToken token=default){_=Group(node,arc,token);return 0;}
    public int ReadLine(int node,ushort arc,CancellationToken token=default)
        =>Graph.ResolveGroupSource(Group(node,arc,token),token).Primary.LineId;
    public LegacyNativeResidualGuide ReadGuide(int actualLine,CancellationToken token=default)
    {token.ThrowIfCancellationRequested();if((uint)actualLine>=(uint)guides.Length)throw new ArgumentOutOfRangeException(nameof(actualLine));return guides[actualLine];}
    public LegacyNativeResidualBounds ReadBounds(int node,ushort arc,CancellationToken token=default)
    {
        var b=transitions.ReadOwnedGeometryState(Group(node,arc,token),token);
        return new(b.MinimumX,b.MinimumY,b.MaximumX,b.MaximumY);
    }
    public bool ReadTopMark(int node,ushort arc,CancellationToken token=default)
    {foreach(var range in ranges[Group(node,arc,token)]){token.ThrowIfCancellationRequested();if(range.StartPoint!=0)return false;}return true;}
    public bool ReadAlphabetFlag(ushort symbol,CancellationToken token=default)
    {token.ThrowIfCancellationRequested();return Policy.Owner.Model.AcousticAlphabet.IsSymbolFlagged(symbol);}
    public LegacyInkRange[] CopyStoredGroupRanges(int node,ushort arc,CancellationToken token=default)
        =>ranges[Group(node,arc,token)].ToArray();
    internal bool StoredGroupRangesEqual(int group,ReadOnlySpan<LegacyInkRange> other,CancellationToken token)
    {token.ThrowIfCancellationRequested();if((uint)group>=(uint)ranges.Length)throw new ArgumentOutOfRangeException(nameof(group));return ranges[group].AsSpan().SequenceEqual(other);}
    public LegacyOrdinaryGraphSourceReference[] ResolveSourceRanges(int node,ushort arc,CancellationToken token=default)
    {_=Group(node,arc,token);return Policy.Owner.Acoustic.Geometry.Context.Source.ResolveCandidateWithResiduals(node,arc,token);}
}
