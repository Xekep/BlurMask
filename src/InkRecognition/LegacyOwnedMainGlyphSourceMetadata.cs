using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("LegacyMainGlyphSourceMetadata.Probe")]

namespace InkRecognition;

/// <summary>One ordinary V140 source export, retaining the target owner key
/// separately from its original acoustic group and copied raw source metadata.</summary>
public sealed class LegacyOwnedMainGlyphSourceMetadataResult
{
    private readonly LegacyOrdinaryGraphSourceReference[] references;
    public LegacyOwnedMainGlyphSourceMetadata Owner {get;}
    public ulong TargetSourceKey {get;}
    public ulong BaseSourceKey {get;}
    public bool IsHigherSource {get;}
    public int BaseNode {get;}
    public ushort BaseArc {get;}
    public int AcousticGroupId {get;}
    public int ReferenceCount=>references.Length;
    public IReadOnlyList<LegacyOrdinaryGraphSourceReference> References {get;}
    internal LegacyOwnedMainGlyphSourceMetadataResult(LegacyOwnedMainGlyphSourceMetadata owner,ulong target,
        ulong original,bool higher,int node,ushort arc,int group,LegacyOrdinaryGraphSourceReference[] references)
    {Owner=owner;TargetSourceKey=target;BaseSourceKey=original;IsHigherSource=higher;BaseNode=node;BaseArc=arc;
        AcousticGroupId=group;this.references=references;References=Array.AsReadOnly(references);}
    public byte[] CopyReferenceStorage24()
    {
        byte[] result=new byte[checked(references.Length*24)];
        for(int i=0;i<references.Length;i++)references[i].CopyReferenceStorage24().CopyTo(result,i*24);
        return result;
    }
    public byte[] CopyResidualStorage2()
    {
        byte[] result=new byte[checked(references.Length*2)];
        for(int i=0;i<references.Length;i++){result[i*2]=references[i].FirstResidual;result[i*2+1]=references[i].LastResidual;}
        return result;
    }
    /// <summary>Native217670 allocation layout: all24-byte records first,
    /// followed by all two-byte endpoint pairs. No native pointer is invented.</summary>
    public byte[] CopySourceAllocation26()
    {
        byte[] result=new byte[checked(references.Length*26)];
        CopyReferenceStorage24().CopyTo(result,0);CopyResidualStorage2().CopyTo(result,references.Length*24);return result;
    }
}

/// <summary>Actual ordinary base/higher-kind0 graph.V140 source metadata,
/// bound to one materialization/geometry/overlay and its original point owner.
/// Negative/protocol references and lazy217670 score/pool mutation are separate.</summary>
public sealed class LegacyOwnedMainGlyphSourceMetadata
{
    private readonly LegacyOrdinaryGraphSourceRanges ranges;
    private readonly LegacySourcePointChronology prepared;
    private readonly LegacyOrdinarySourceInk original;
    private readonly LegacySourcePointKey[] keyRecords;
    public LegacyFreshMainPlainResultOwnership Ownership {get;}
    public LegacyOwnedRussianMainResidualSource SourceOverlay {get;}
    public LegacyOrdinaryCursiveCandidateGraph Graph=>Ownership.Geometry.Graph;

    private LegacyOwnedMainGlyphSourceMetadata(LegacyFreshMainPlainResultOwnership ownership,
        LegacyOwnedRussianMainResidualSource overlay,LegacyOrdinaryGraphSourceRanges ranges,
        LegacySourcePointKey[] keyRecords)
    {Ownership=ownership;SourceOverlay=overlay;this.ranges=ranges;prepared=ranges.PreparedChronology;
        original=ranges.OriginalSource;this.keyRecords=keyRecords;}

    public static LegacyOwnedMainGlyphSourceMetadata Prepare(LegacyFreshMainPlainResultOwnership ownership,
        LegacyOwnedRussianMainResidualSource actualOverlay,LegacyOrdinaryGraphSourceRanges actualSourceRanges,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(ownership);ArgumentNullException.ThrowIfNull(actualOverlay);
        ArgumentNullException.ThrowIfNull(actualSourceRanges);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(ownership.Geometry,actualOverlay.Geometry)||
            !ReferenceEquals(ownership.Materialization.ConfidenceOwner,actualOverlay.ConfidenceOwner)||
            !ReferenceEquals(ownership.Geometry.Graph,actualSourceRanges.Graph)||
            !ReferenceEquals(ownership.Controller.Owner.SourceGraph,actualSourceRanges.Graph))
            throw new ArgumentException("One actual materialization, sampled graph, geometry, source ranges and residual overlay required.");
        if(ownership.CaptureGlyphGeometry||ownership.Controller.Configuration.Trace2368!=0||
            ownership.Controller.Configuration.Mode22f6!=0||actualOverlay.Geometry.CollectionC1)
            throw new NotSupportedException("Actual fresh ordinary135/no-capture source metadata required.");
        var original=actualSourceRanges.OriginalSource;
        var keys=new LegacySourcePointKey[original.Strokes.Sum(s=>s.Points.Count)];var assigned=new bool[keys.Length];
        for(int s=0;s<original.Strokes.Count;s++)for(int p=0;p<original.Strokes[s].Points.Count;p++)
        {
            token.ThrowIfCancellationRequested();var identity=original.Chronology.Resolve(s,p);
            if((uint)identity.KeyRecordIndex>=(uint)keys.Length||assigned[identity.KeyRecordIndex])
                throw new ArgumentException("Complete actual originalF8 key-record owner required.");
            assigned[identity.KeyRecordIndex]=true;keys[identity.KeyRecordIndex]=identity.Key;
        }
        return new(ownership,actualOverlay,actualSourceRanges,keys);
    }

    public LegacyOwnedMainGlyphSourceMetadataResult Read(ulong actualCanonicalSourceKey,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(actualCanonicalSourceKey!=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(actualCanonicalSourceKey))
            throw new ArgumentException("Actual canonical owner key required, without a glyph-alternative ordinal.");
        var target=LegacyNativeResidualSource.Decode(actualCanonicalSourceKey);
        if((uint)target.Node>=(uint)Graph.NodeCount||unchecked((short)target.Arc)<0)
            throw new NotSupportedException("Ordinary nonnegative source arc required; span/protocol source publication is separate.");
        ulong baseKey=SourceOverlay.ResolveBaseReference(actualCanonicalSourceKey,token);
        if(baseKey!=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(baseKey))
            throw new NotSupportedException("Higher originalLast must be an actual canonical source key, not a glyph-alternative reference.");
        var baseSource=LegacyNativeResidualSource.Decode(baseKey);
        if((uint)baseSource.Node>=(uint)Graph.NodeCount||baseSource.Arc>=Graph.GetNode(baseSource.Node).Count)
            throw new InvalidDataException("Actual higher originalLast must resolve to one base ordinary graph arc.");
        bool higher=target.Arc>=Graph.GetNode(target.Node).Count;
        var result=higher?ConvertHigher(SourceOverlay.CopySourceRanges(actualCanonicalSourceKey,token),token):
            ranges.ResolveCandidateWithResiduals(target.Node,target.Arc,token);
        if(result.Length==0)throw new NotSupportedException("Actual nonempty source membership required, not a fabricated empty interval.");
        token.ThrowIfCancellationRequested();return new(this,actualCanonicalSourceKey,baseKey,higher,
            baseSource.Node,baseSource.Arc,Graph.GetNode(baseSource.Node)[baseSource.Arc].GroupId,result);
    }

    private LegacyOrdinaryGraphSourceReference[] ConvertHigher(LegacyInkRange[] actualRanges,CancellationToken token)
    {
        if(actualRanges.Length is <1 or >65536)throw new NotSupportedException("Bounded actual higher-kind0 range owner required.");
        var rows=new List<Row>(actualRanges.Length);var counts=prepared.StrokePointCounts.Span;
        foreach(var range in actualRanges)
        {
            token.ThrowIfCancellationRequested();
            if((uint)range.StrokeIndex>=(uint)counts.Length||range.StartPoint<0||range.EndPointExclusive<=range.StartPoint||range.EndPointExclusive>counts[range.StrokeIndex])
                throw new InvalidDataException("Owned higher range escapes its actual prepared source.");
            var first=prepared.Resolve(range.StrokeIndex,range.StartPoint);
            var lastStart=prepared.Resolve(range.StrokeIndex,range.EndPointExclusive-1);
            rows.Add(new(first,original.ResolveEndingMetadata(lastStart,token),lastStart,range.EndPointExclusive==counts[range.StrokeIndex]));
        }
        rows=rows.OrderBy(r=>r,RowComparer.Instance).ToList();
        for(int i=rows.Count-1;i>0;i--)
        {
            token.ThrowIfCancellationRequested();var current=rows[i];var previous=rows[i-1];
            bool equal=current.First.Key==previous.LastStart.Key&&current.First.RemainderByte==previous.LastStart.RemainderByte;
            int next=previous.LastStart.KeyRecordIndex+1;
            bool adjacent=previous.Tail&&next<keyRecords.Length&&current.First.Key==keyRecords[next];
            if(equal||adjacent){rows[i-1]=previous with{Last=current.Last,LastStart=current.LastStart,Tail=current.Tail};rows.RemoveAt(i);}
        }
        return rows.Select(r=>new LegacyOrdinaryGraphSourceReference(r.First.Key,r.Tail?r.Last.Key:r.LastStart.Key,
            r.First.RemainderByte,r.Tail?r.Last.RemainderByte:r.LastStart.RemainderByte)).ToArray();
    }
    private readonly record struct Row(LegacySourcePointIdentity First,LegacySourcePointIdentity Last,LegacySourcePointIdentity LastStart,bool Tail);
    private sealed class RowComparer:IComparer<Row>
    {
        public static readonly RowComparer Instance=new();
        public int Compare(Row a,Row b)
        {
            int c=Key(a.First.Key,b.First.Key);if(c!=0)return c;
            c=b.First.RemainderByte.CompareTo(a.First.RemainderByte);if(c!=0)return c;
            c=Key(a.Last.Key,b.Last.Key);return c!=0?c:b.Last.RemainderByte.CompareTo(a.Last.RemainderByte);
        }
        private static int Key(LegacySourcePointKey a,LegacySourcePointKey b)
        {int c=a.First.CompareTo(b.First);if(c!=0)return c;c=a.Second.CompareTo(b.Second);return c!=0?c:a.Third.CompareTo(b.Third);}
    }
}
