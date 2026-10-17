using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>The219580 source-count/fill prefix for one genuine fresh275
/// span owner. This is not273.V68 completion: word augmentation, pruning and
/// cross-alternative rewrite remain distinct steps.</summary>
public sealed class LegacyOwnedMainWordSourceCollection
{
    private readonly LegacyOrdinaryGraphSourceReference[] references;
    private readonly LegacyNativeInkPositionRange[] ranges;
    public LegacyFreshMainCompletedFirstCandidate Candidate {get;}
    public LegacyNativePublishedWordNode Word {get;}
    public LegacyOwnedRawInkDocument Document=>Candidate.RawResult.Owner.SourceDocument;
    public int FirstContext=>Word.Span.First;
    public int LastContext=>Word.Span.Last;
    public int ReferenceCount=>references.Length;
    public bool IsTopV68Completed=>false;
    private LegacyOwnedMainWordSourceCollection(LegacyFreshMainCompletedFirstCandidate candidate,
        LegacyNativePublishedWordNode word,LegacyOrdinaryGraphSourceReference[] references,
        LegacyNativeInkPositionRange[] ranges)
    {Candidate=candidate;Word=word;this.references=references;this.ranges=ranges;}

    /// <summary>Bounded first fresh span-owner source export before219580's
    /// alternative append. The exact one-WORD restriction avoids substituting
    /// an exact-span dictionary for native278's containing-span ownership.</summary>
    public static LegacyOwnedMainWordSourceCollection ExportBeforeWordAugmentation(
        LegacyFreshMainCompletedFirstCandidate actualCandidate,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualCandidate);token.ThrowIfCancellationRequested();
        var raw=actualCandidate.RawResult;
        if(actualCandidate.RawWordsInNativeHeadOrder.Count!=1||
            !ReferenceEquals(actualCandidate.Snapshot.WordHead18,raw.Session.Ownership.WordPublisher.Head))
            throw new NotSupportedException("The source prefix currently owns exactly one fresh first-alternative WORD/span owner.");
        var word=actualCandidate.RawWordsInNativeHeadOrder[0].Publication.PublishedWord!;
        if(word.Next is not null||word.Span.First<0||word.Span.Last<word.Span.First)
            throw new NotSupportedException("A genuine forward ordinary first span is required.");
        //The existing typed factory closes fresh21B no-protocol construction,
        //registration, original F8 metadata and the single descriptor lifecycle.
        //In1A9AE0's successful forward branch mode2 differs from mode0 only
        //when the final-node pending protocol vector exists; this owner has none.
        var scope=LegacyOwnedMainSpanGlyphSource.Prepare(raw,token);
        var source=raw.Sampled.SourceRanges;
        if(!ReferenceEquals(scope.Graph,source.Graph)||!ReferenceEquals(source.Graph,actualCandidate.Snapshot.SourcePolicy.DictionaryOwner.Graph))
            throw new ArgumentException("Identical actual fresh source graph required.");
        var count=source.ResolveForwardWithResiduals((uint)word.Span.First,(uint)word.Span.Last,token);
        var fill=source.ResolveForwardWithResiduals((uint)word.Span.First,(uint)word.Span.Last,token);
        if(!count.AsSpan().SequenceEqual(fill))throw new InvalidDataException("Immutable word source changed between native count and fill.");
        var projection=new LegacyNativeTextSourceReference[fill.Length];
        for(int i=0;i<fill.Length;i++)
        {
            var r=fill[i];
            if(r.First.First!=0||r.Last.First!=0)throw new NotSupportedException("Actual raw document ingress group0 required.");
            projection[i]=new(new(r.First.First,r.First.Second,r.First.Third),new(r.Last.First,r.Last.Second,r.Last.Third),r.FirstResidual,r.LastResidual);
        }
        var doc=raw.Owner.SourceDocument;
        var map=new LegacyNativeTextSourceInputMap(doc.CopySelectedInputs(),[],0);
        LegacyNativeInkPositionRange[] original=[new(LegacyNativeInkPosition.Pack(0,0,3,doc.Strokes[0].Points.Count),
            LegacyNativeInkPosition.Pack((uint)doc.ObjectCount-1,doc.Strokes[^1].Points.Count-1,5,doc.Strokes[^1].Points.Count))];
        var positions=LegacyNativeTextSourcePositionPublication.Project(map,projection,true,original,token);
        token.ThrowIfCancellationRequested();return new(actualCandidate,word,fill,positions);
    }
    public LegacyOrdinaryGraphSourceReference[] CopyReferences()=>references.ToArray();
    public LegacyNativeInkPositionRange[] CopyRanges()=>ranges.ToArray();
    /// <summary>Exact26*N source allocation, with portable pointer ownership
    /// replaced by this immutable object. Residual bytes follow24*N bytes.</summary>
    public byte[] CopySourceStorage26()
    {
        byte[] data=new byte[references.Length*26];int tail=references.Length*24;
        for(int i=0;i<references.Length;i++)
        {
            var r=references[i];Write(data.AsSpan(i*24),r.First);Write(data.AsSpan(i*24+12),r.Last);
            data[tail+i*2]=r.FirstResidual;data[tail+i*2+1]=r.LastResidual;
        }
        return data;
        static void Write(Span<byte> row,LegacySourcePointKey p)
        {BinaryPrimitives.WriteInt32LittleEndian(row,p.First);BinaryPrimitives.WriteInt32LittleEndian(row[4..],p.Second);BinaryPrimitives.WriteInt32LittleEndian(row[8..],p.Third);}
    }
}
