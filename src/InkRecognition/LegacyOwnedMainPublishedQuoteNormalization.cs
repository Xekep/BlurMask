namespace InkRecognition;

/// <summary>Actual completed single-line candidate to21FBF0's published WORD
/// list. This is a post-callback quote stage, not final text acceptance.</summary>
public static class LegacyOwnedMainPublishedQuoteNormalization
{
    public static void Apply(LegacyFreshMainSingleLinePlainRecognitionResult actualResult,
        Span<byte> leadingCallerStorage4,Span<byte> trailingCallerStorage4,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualResult);token.ThrowIfCancellationRequested();
        if(leadingCallerStorage4.Length!=4||trailingCallerStorage4.Length!=4||
            leadingCallerStorage4.Overlaps(trailingCallerStorage4))
            throw new ArgumentException("Two separate complete caller scratch slots required.");
        var session=actualResult.Session;var ownership=session.Ownership;
        if(!actualResult.PublicationCompleted||session.IsStopped||
            !ReferenceEquals(ownership.Materialization.TerminalResult,actualResult.Terminal)||
            actualResult.Publications.Count!=session.CompletedFlushCount)
            throw new ArgumentException("Completed same-terminal actual publication sequence required.");
        var words=new List<LegacyNativePlainWordRecord>();var node=ownership.WordPublisher.Head;
        for(int i=actualResult.Publications.Count-1;i>=0;i--)
        {
            token.ThrowIfCancellationRequested();var publication=actualResult.Publications[i];
            if(!publication.Succeeded||!ReferenceEquals(publication.Association.Owner,session)||
                publication.Association.Ordinal!=i)
                throw new ArgumentException("Exact ordered successful same-session publication tokens required.");
            if(publication.PublishedWord is not {} published)continue;
            if(!ReferenceEquals(published,node)||!ReferenceEquals(published.Word.Pool,ownership.GlyphPool))
                throw new ArgumentException("Actual published head/list was changed outside this result sequence.");
            words.Add(published.Word);node=published.Next;
        }
        if(node is not null)throw new ArgumentException("Unrelated extra published words cannot enter quote normalization.");
        var normalizer=LegacyNativeWordQuoteNormalization.Prepare(ownership.Materialization.ConfidenceOwner.Model.AcousticAlphabet,token);
        //A nonempty control descriptor requires the distinct TOP273+18 owner.
        //That owner is not substituted with the acoustic AL. The scalar reader
        //retains its existing explicit refusal when such a branch is reached.
        normalizer.NormalizePublished(ownership.GlyphPool,words,leadingCallerStorage4,
            trailingCallerStorage4,null,token);
        token.ThrowIfCancellationRequested();
    }
}
