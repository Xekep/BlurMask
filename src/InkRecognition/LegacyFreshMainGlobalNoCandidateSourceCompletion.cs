using System.Buffers.Binary;
namespace InkRecognition;

/// <summary>Actual one-WORD/no-bit8/no-pending global TOP.V68 source stage.
/// Retains its live original pool and primary alphabet. This is not an imported
/// byte result, a CHAR/TEXT source or accepted transcription.</summary>
public sealed class LegacyFreshMainGlobalNoCandidateSourceCompletion
{
    private readonly LegacyLibraryGlobalTextPublicationScope scope;
    private LegacyOrdinaryGraphSourceReference[] references=[];
    private LegacyNativeInkPositionRange[] ranges=[];
    private int started;
    public LegacyFreshMainGlobalPlainRecognitionResult Source {get;}
    public LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot Snapshot {get;}
    public LegacyNativePublishedWordNode Word {get;}
    public bool QuoteCleanupCompleted {get;private set;}
    public bool SourcesMarkedComplete {get;private set;}
    public bool RankingApplied {get;private set;}
    public bool SpanScoresApplied {get;private set;}
    public bool CheckboxGuardVerified {get;private set;}
    public bool IsTopV68Completed {get;private set;}
    public bool IsFaulted {get;private set;}
    public int QuotaAfter {get;private set;}
    public int ReferenceCount=>references.Length;
    internal LegacyFreshMainGlobalNoCandidateSourceCompletion(LegacyLibraryGlobalTextPublicationScope scope,
        LegacyFreshMainGlobalPlainRecognitionResult source,LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot snapshot,
        LegacyNativePublishedWordNode word)
    {this.scope=scope;Source=source;Snapshot=snapshot;Word=word;}

    internal static LegacyFreshMainGlobalNoCandidateSourceCompletion Prepare(LegacyLibraryGlobalTextPublicationScope scope,
        LegacyFreshMainGlobalPlainRecognitionResult source,ReadOnlySpan<byte> allocation400,CancellationToken token)
    {
        scope.Validate();token.ThrowIfCancellationRequested();source.Selection.Publication.Validate(token);
        var policy=source.Selection.Publication.Policy;var graph=policy.Owner.Graph;var cuts=graph.Source.Source;
        if(!ReferenceEquals(source.Owner.SourceDocument,scope.Document)||source.Session is not{} session||
            !session.IsComplete||session.IsStopped||!ReferenceEquals(session.Ownership.PublishedTerminal,source.Selection))
            throw new ArgumentException("Identical pre-NN document, current selected global result and completed callback session required.");
        //The actual shared graph was built from ordinary cuts only: neither
        //19D470 pending208 records nor1A9AE0 mode2's trailing metadata exist.
        if(cuts.Steps.Count!=1||cuts.Steps[0].Kind!=0||cuts.CopyTrailingPendingDescriptorOrdinals().Length!=0||
            cuts.Steps[0].CopyPendingDescriptorOrdinals().Length!=0||graph.Source.State.Contexts.Count!=0)
            throw new NotSupportedException("One ordinary descriptor with no pending/reverse source metadata required.");
        var word=session.Ownership.WordPublisher.Head;
        if(word is null||word.Next is not null||word.Span.First<0||word.Span.Last<word.Span.First)
            throw new NotSupportedException("Exactly one actual forward WORD/span is restored.");
        byte[] compact=source.Selection.Publication.CopyCompactStorage();
        RequireNoActiveAugmentation(compact,source.Selection.Publication.RecordCount2470,token);
        foreach(string text in new[]{"\u2610","\u2611","\u2612"})
            if(policy.Owner.Model.AcousticAlphabet.TryGetLabel(text,out _))throw new NotSupportedException("Actual checkbox rewriting requires its distinct source owner.");
        var publisher=session.Ownership.WordPublisher;
        var span=publisher.CaptureSpan(word,new[]{word},token);
        if(span.Words.Count!=1||span.SelectedNodes.Count!=1||span.SelectedOrdinals[0]!=0)
            throw new NotSupportedException("This live global route restores one actual275 payload and one279 selection.");
        var snapshot=LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot.Capture(source.Selection,session,source.Publications,allocation400,token);
        scope.Validate();return new(scope,source,snapshot,word);
    }
    internal static void RequireNoActiveAugmentation(ReadOnlySpan<byte> compact,int count,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(count<1||count>compact.Length/40)throw new InvalidDataException("Complete actual active compact40 extent required.");
        for(int i=0;i<count;i++)
        {token.ThrowIfCancellationRequested();if((compact[i*40+32]&8)!=0)throw new NotSupportedException("Active219580 bit8 augmentation is not restored.");}
    }
    internal void Run(Span<byte> leading,Span<byte> trailing,CancellationToken token)
    {
        Validate(token);if(Interlocked.CompareExchange(ref started,1,0)!=0)throw new InvalidOperationException("Entered live V68 cannot restart.");
        try
        {
            int evaluations=Source.CachedMainGroupEvaluations;
            LegacyFreshMainGlobalPublishedQuoteNormalization.ApplyPublished(Snapshot,leading,trailing,token);
            QuoteCleanupCompleted=true;
            _=LegacyFreshMainGlobalResultGlyphNormalization.Prepare(Snapshot.SourcePolicy,token).ApplyPublished(Snapshot,token);
            ExportSource(token);
            var publisher=Source.Session!.Ownership.WordPublisher;
            var span=publisher.CaptureSpan(Word,new[]{Word},token);SourcesMarkedComplete=true;
            var ranked=LegacyPreparedMainWordRanking.Rank(span.Words.Select(w=>w.CopyStorage()).ToArray(),
                span.SelectedOrdinals.ToArray(),LegacyPreparedMainWordRanking.InitialQuota(65535,65535,false),false,
                static (_,_)=>throw new InvalidOperationException("Single-payload ranking cannot compare a fabricated second WORD."),token);
            publisher.ApplySpanRanking(span,ranked.Words,ranked.OutputSourceOrdinals,ranked.OldToNew,token);
            RankingApplied=true;QuotaAfter=ranked.QuotaAfter;
            var scored=publisher.CaptureSpan(Word,new[]{Word},token);
            var scores=LegacyPreparedMainSpanScorePublication.Normalize(scored.Words.Select(w=>w.CopyStorage()).ToArray(),token);
            publisher.ApplySpanScores(scored,scores.Words,token);SpanScoresApplied=true;
            CheckboxGuardVerified=true;Validate(token);
            if(Snapshot.SourcePolicy.Owner.AcousticEvaluations!=evaluations)throw new InvalidOperationException("Live source completion cannot reevaluate NN.");
            IsTopV68Completed=true;
        }
        catch{IsFaulted=true;throw;}
    }
    private void ExportSource(CancellationToken token)
    {
        var sourceRanges=Snapshot.SourcePolicy.Owner.Acoustic.Geometry.Context.Source;
        var counted=sourceRanges.ResolveForwardWithResiduals((uint)Word.Span.First,(uint)Word.Span.Last,token);
        var filled=sourceRanges.ResolveForwardWithResiduals((uint)Word.Span.First,(uint)Word.Span.Last,token);
        if(!counted.AsSpan().SequenceEqual(filled))throw new InvalidDataException("Actual count/fill source changed.");
        var projection=new LegacyNativeTextSourceReference[filled.Length];
        for(int i=0;i<filled.Length;i++)
        {
            var r=filled[i];if(r.First.First!=0||r.Last.First!=0)throw new NotSupportedException("Fresh raw group0 source keys required.");
            projection[i]=new(new(r.First.First,r.First.Second,r.First.Third),new(r.Last.First,r.Last.Second,r.Last.Third),r.FirstResidual,r.LastResidual);
        }
        var doc=scope.Document;var map=new LegacyNativeTextSourceInputMap(doc.CopySelectedInputs(),[],0);
        LegacyNativeInkPositionRange[] original=[new(LegacyNativeInkPosition.Pack(0,0,3,doc.Strokes[0].Points.Count),
            LegacyNativeInkPosition.Pack((uint)doc.ObjectCount-1,doc.Strokes[^1].Points.Count-1,5,doc.Strokes[^1].Points.Count))];
        var positions=LegacyNativeTextSourcePositionPublication.Project(map,projection,true,original,token);
        references=filled;ranges=positions;
    }
    internal void Validate(CancellationToken token)
    {
        scope.Validate();token.ThrowIfCancellationRequested();Source.Selection.Publication.Validate(token);
        if(IsFaulted||!Source.PublicationCompleted||!ReferenceEquals(Word,Source.Session!.Ownership.WordPublisher.Head))
            throw new InvalidOperationException("The exact live completed global source/list must remain current.");
    }
    public LegacyOrdinaryGraphSourceReference[] CopyReferences(){Validate(default);return references.ToArray();}
    public LegacyNativeInkPositionRange[] CopyRanges(){Validate(default);return ranges.ToArray();}
    public byte[] CopySourceStorage26()
    {
        Validate(default);byte[] bytes=new byte[checked(references.Length*26)];int tail=references.Length*24;
        for(int i=0;i<references.Length;i++)
        {var r=references[i];Write(bytes.AsSpan(i*24),r.First);Write(bytes.AsSpan(i*24+12),r.Last);bytes[tail+i*2]=r.FirstResidual;bytes[tail+i*2+1]=r.LastResidual;}
        return bytes;
        static void Write(Span<byte> b,LegacySourcePointKey p)
        {BinaryPrimitives.WriteInt32LittleEndian(b,p.First);BinaryPrimitives.WriteInt32LittleEndian(b[4..],p.Second);BinaryPrimitives.WriteInt32LittleEndian(b[8..],p.Third);}
    }
}
