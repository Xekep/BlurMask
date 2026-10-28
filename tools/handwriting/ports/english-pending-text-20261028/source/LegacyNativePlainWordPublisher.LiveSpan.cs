namespace InkRecognition;

public sealed partial class LegacyNativePlainWordPublisher
{
    // Scalar read-only audit sizes; no source/WORD handles or authority.
    internal int ReadPublicationHistoryCount()=>interning.Publishers.Count;

    //Read-only. No apply/claim or payload clone. Only NEW private span factory supplies its snapshot.
    internal void RequireLiveSpanRows(LegacyNativePlainWordSpanSnapshot snapshot,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(released||!interning.Spans.TryGetValue(snapshot.Span,out var words)||
            !snapshot.RetainsNativeList(words)||words.Count!=snapshot.Words.Count)
            throw new InvalidOperationException("Actual private275 list changed after all-WORD preparation.");
        for(int i=0;i<words.Count;i++)
        {token.ThrowIfCancellationRequested();if(!ReferenceEquals(words[i],snapshot.Words[i]))throw new InvalidOperationException("Actual275 payload identity changed after all-WORD preparation.");}
        foreach(var selected in snapshot.SelectedNodes)
            if((uint)selected.WordIndex>=words.Count||!ReferenceEquals(selected.Word,words[selected.WordIndex]))
                throw new InvalidOperationException("Actual selected279 binding changed after all-WORD preparation.");
    }
}
