using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>The219580 source-count/fill prefix for one genuine fresh275
/// span owner. This is not273.V68 completion: word augmentation, pruning and
/// cross-alternative rewrite remain distinct steps.</summary>
public sealed class LegacyOwnedMainWordSourceCollection
{
    private readonly LegacyOrdinaryGraphSourceReference[] references;
    private readonly LegacyNativeInkPositionRange[] ranges;
    private readonly LegacyFreshMainCompletedFirstCandidate? candidate;
    public LegacyFreshMainCompletedFirstCandidate Candidate=>candidate??
        throw new InvalidOperationException("Configured source collection has no historical first-candidate object.");
    public LegacyOwnedMainCandidateSource CandidateSource {get;}
    public LegacyNativePublishedWordNode Word {get;}
    public LegacyOwnedRawInkDocument Document=>CandidateSource.SourceDocument;
    public int FirstContext=>Word.Span.First;
    public int LastContext=>Word.Span.Last;
    public int ReferenceCount=>references.Length;
    public bool IsTopV68Completed=>false;
    private LegacyOwnedMainWordSourceCollection(LegacyOwnedMainCandidateSource source,
        LegacyNativePublishedWordNode word,LegacyOrdinaryGraphSourceReference[] references,
        LegacyNativeInkPositionRange[] ranges)
    {CandidateSource=source;candidate=source.IsConfigured?null:source.FirstCandidate;Word=word;this.references=references;this.ranges=ranges;}

    //One native275 span owns one26N allocation. A second genuine configured
    //279 selection of that span gets an immutable view, not another count/fill.
    internal LegacyOwnedMainWordSourceCollection BindSameSpanCandidate(LegacyOwnedMainCandidateSource source,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(source);source.Validate(token);
        if(!CandidateSource.IsConfigured||!source.IsConfigured||
            !ReferenceEquals(source.SourceIdentity,CandidateSource.SourceIdentity)||
            !ReferenceEquals(source.TopOwner,CandidateSource.TopOwner)||
            source.WordHead18 is not{} word||word.Next is not null||word.Span!=Word.Span)
            throw new ArgumentException("A genuine same-source/TOP configured selection of the identical shared span required.");
        return new(source,word,references,ranges);
    }
    internal bool SharesSourceAllocation(LegacyOwnedMainWordSourceCollection other)
        =>ReferenceEquals(references,other.references)&&ReferenceEquals(ranges,other.ranges);

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
        return ExportCore(LegacyOwnedMainCandidateSource.FromFirst(actualCandidate,token),token);
    }

    /// <summary>Source-count/fill prefix for one actual selected configured
    /// candidate. Alternatives may share275 ownership; this does not complete
    /// that shared owner's lazy export/augmentation/ranking or mark TOP.V68.</summary>
    public static LegacyOwnedMainWordSourceCollection PrepareConfigured(
        LegacyOwnedMainCandidateSource actualCandidate,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualCandidate);actualCandidate.Validate(token);
        if(!actualCandidate.IsConfigured)throw new ArgumentException("Actual configured candidate source required.");
        return ExportCore(actualCandidate,token);
    }

    private static LegacyOwnedMainWordSourceCollection ExportCore(LegacyOwnedMainCandidateSource actualCandidate,
        CancellationToken token)
    {
        actualCandidate.Validate(token);
        var words=actualCandidate.CopyRawWordsInNativeHeadOrder(token);
        if(words.Length!=1||!ReferenceEquals(actualCandidate.WordHead18,actualCandidate.Ownership.WordPublisher.Head))
            throw new NotSupportedException("The source prefix owns exactly one actual WORD/span per selected candidate.");
        var word=words[0].Publication.PublishedWord!;
        if(word.Next is not null||word.Span.First<0||word.Span.Last<word.Span.First)
            throw new NotSupportedException("A genuine forward ordinary first span is required.");
        //The existing typed factory closes fresh21B no-protocol construction,
        //registration, original F8 metadata and the single descriptor lifecycle.
        //In1A9AE0's successful forward branch mode2 differs from mode0 only
        //when the final-node pending protocol vector exists; this owner has none.
        var scope=actualCandidate.IsConfigured?LegacyOwnedMainSpanGlyphSource.PrepareConfigured(actualCandidate,token):
            LegacyOwnedMainSpanGlyphSource.Prepare(actualCandidate.RawResult,token);
        var source=actualCandidate.Sampled.SourceRanges;
        if(!ReferenceEquals(scope.Graph,source.Graph)||!ReferenceEquals(source.Graph,actualCandidate.SourcePolicy.DictionaryOwner.Graph))
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
        var doc=actualCandidate.SourceDocument;
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
