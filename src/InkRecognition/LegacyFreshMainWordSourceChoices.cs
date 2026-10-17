using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Actual configured shared275 WORD array before0283B0 document
/// acceptance. All payloads retain their real records, flat references and
/// selected279 ordinals; no candidate-local rendered results are combined.</summary>
public sealed class LegacyFreshMainWordSourceChoices : IDisposable
{
    private readonly LegacyNativePlainWordPublisher publisher;
    private readonly LegacyNativePublishedWordNode[] heads;
    private readonly LegacyNativePlainWordSpanSnapshot snapshot;
    private readonly LegacyOwnedMainSpanWordView[] words;
    private readonly LegacyOwnedMainCandidateSource[] candidates;
    private readonly LegacyMainSharedTextSourceState shared;
    private readonly LegacyFreshMainTextSourceWrapper renderer;
    private readonly object gate=new();
    private bool started;
    private bool disposed;
    public bool IsFaulted {get;private set;}
    public LegacyFreshMainConfiguredNoCandidateSourceCompletion SourceCompletion {get;}
    public LegacyOwnedMainWordSourceCollection Sources=>SourceCompletion.Sources;
    public LegacyOwnedRawInkDocument Document=>Sources.Document;
    public LegacyFreshMainPlainResultOwnership TopOwner=>Sources.CandidateSource.TopOwner;
    public IReadOnlyList<LegacyOwnedMainSpanWordView> WordViews {get;}
    internal LegacyOwnedFreshTextSourceContext SelectionContext=>shared.Selections;
    public LegacyFreshTextFieldInputContext? SelectedInputContext=>shared.Selections.SelectedInputContext;
    public long NextSelectionId=>shared.Selections.NextSelectionId;
    public IReadOnlyList<LegacyOwnedFreshTextSelection> CopyPendingSelections()=>shared.Selections.CopyPendingSelections();

    private LegacyFreshMainWordSourceChoices(ReadOnlySpan<byte> resource,LegacyFreshMainConfiguredNoCandidateSourceCompletion completion,
        LegacyOwnedMainCandidateSource[] candidates,LegacyNativePublishedWordNode[] heads,
        LegacyNativePlainWordSpanSnapshot snapshot,LegacyCursiveSeparationTable table,
        LegacyNativeTextPublicationData publication,LegacyNativeUnicodeNormalizationData normalization,
        LegacyNativeCircledPayloadData circles,LegacyFreshTextFieldInputContext? selectedInput,CancellationToken token)
    {
        SourceCompletion=completion;this.candidates=candidates;this.heads=heads;this.snapshot=snapshot;
        publisher=candidates[0].Ownership.WordPublisher;
        words=snapshot.Words.Select((record,index)=>new LegacyOwnedMainSpanWordView(this,index,record)).ToArray();
        WordViews=Array.AsReadOnly(words);
        shared=new(Document,TopOwner,selectedInput);
        try
        {
            var choices=LegacyFreshMainTextGlyphChoices.PrepareSpanWords(resource,candidates[0],words,shared.Strings,
                table,publication,normalization,circles,token);
            renderer=LegacyFreshMainTextSourceWrapper.PrepareSpanWords(choices,completion,shared,publication,normalization,token);
        }
        catch{shared.Dispose();throw;}
    }

    public static LegacyFreshMainWordSourceChoices Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainConfiguredNoCandidateSourceCompletion actualCompletedSources,LegacyCursiveSeparationTable table,
        LegacyNativeTextPublicationData publication,LegacyNativeUnicodeNormalizationData normalization,
        LegacyNativeCircledPayloadData circles,CancellationToken token=default)
        =>PrepareCore(resource,actualCompletedSources,null,table,publication,normalization,circles,token);

    /// <summary>The same actual shared275 WORD source pipeline with the
    /// Text05F050 pending cursor derived from a retained selected INPUT/C22.
    /// This does not complete result2DA transform or document acceptance.</summary>
    public static LegacyFreshMainWordSourceChoices PrepareSelectedInput(ReadOnlySpan<byte> resource,
        LegacyFreshMainConfiguredNoCandidateSourceCompletion actualCompletedSources,
        LegacyFreshTextFieldInputContext actualSelectedInput,LegacyCursiveSeparationTable table,
        LegacyNativeTextPublicationData publication,LegacyNativeUnicodeNormalizationData normalization,
        LegacyNativeCircledPayloadData circles,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualCompletedSources);ArgumentNullException.ThrowIfNull(actualSelectedInput);
        token.ThrowIfCancellationRequested();
        if(actualSelectedInput.Parent is null||
            !ReferenceEquals(actualSelectedInput.Document,actualCompletedSources.Sources.Document))
            throw new ArgumentException("Actual selected INPUT/C22 must retain this same original recognition document.");
        return PrepareCore(resource,actualCompletedSources,actualSelectedInput,table,publication,normalization,circles,token);
    }

    private static LegacyFreshMainWordSourceChoices PrepareCore(ReadOnlySpan<byte> resource,
        LegacyFreshMainConfiguredNoCandidateSourceCompletion actualCompletedSources,
        LegacyFreshTextFieldInputContext? selectedInput,LegacyCursiveSeparationTable table,
        LegacyNativeTextPublicationData publication,LegacyNativeUnicodeNormalizationData normalization,
        LegacyNativeCircledPayloadData circles,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(actualCompletedSources);ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(publication);ArgumentNullException.ThrowIfNull(normalization);
        ArgumentNullException.ThrowIfNull(circles);token.ThrowIfCancellationRequested();
        if(!actualCompletedSources.IsTopV68Completed||actualCompletedSources.IsFaulted)
            throw new ArgumentException("Successfully completed actual configured shared-span source owner required.");
        var source=actualCompletedSources.Sources.CandidateSource;
        var candidates=source.ConfiguredResult.Alternatives.CopyActiveAlternatives()
            .Select(a=>LegacyOwnedMainCandidateSource.FromConfigured(source.ConfiguredResult,a,token)).ToArray();
        if(candidates.Length!=2)throw new NotSupportedException("The bounded stage requires two actual retained alternatives.");
        foreach(var c in candidates)_=actualCompletedSources.SourcesFor(c,token);
        var heads=candidates.Select(c=>c.WordHead18??throw new InvalidDataException("Actual WORD head missing.")).ToArray();
        var snapshot=candidates[0].Ownership.WordPublisher.CaptureSpan(actualCompletedSources.Sources.Word,heads,token);
        if(snapshot.Words.Count is <1 or >4095)throw new NotSupportedException("Bounded nonempty actual275 payload array required.");
        var graph=source.SourcePolicy.Graph;int referenceCount=0;
        foreach(var word in snapshot.Words)
        {
            token.ThrowIfCancellationRequested();byte[] data=word.CopyStorage();
            if(word.FlatReferences.Count>65536-referenceCount)throw new NotSupportedException("Bounded complete275 source-reference budget exceeded.");
            referenceCount+=word.FlatReferences.Count;
            if(word.FlatReferences.Count==0||BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(88))!=0||
                BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(96))!=0||
                BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(104))!=0||
                BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(112))!=0)
                throw new NotSupportedException("Actual plain WORD without unowned auxiliary vectors required.");
            foreach(ulong reference in word.FlatReferences)
            {
                int node=(int)(reference&0xffffff);int arc=unchecked((short)(reference>>24));
                if(node>=graph.NodeCount||arc<0||arc>=graph.GetNode(node).Count)
                    throw new NotSupportedException("Only actual ordinary base sources are supported; no higher/protocol source is omitted.");
            }
        }
        return new(resource,actualCompletedSources,candidates,heads,snapshot,table,publication,normalization,circles,selectedInput,token);
    }

    public LegacyOwnedMainWordSourceChoicesResult Publish(ReadOnlySpan<byte> recordResidue40,
        ReadOnlySpan<byte> selectionResidue4,CancellationToken token=default)
    {
        lock(gate)
        {
            ObjectDisposedException.ThrowIf(disposed,this);token.ThrowIfCancellationRequested();
            if(started)throw new InvalidOperationException("The shared WORD source publication retains an entered prefix and is single-use.");
            foreach(var word in words)ValidateWord(word,token);
            renderer.BeginSpanPublication(recordResidue40,selectionResidue4,token);started=true;
            try
            {
                var first=shared.Words.InternWordSource(Sources,token);
                if(!first.Inserted)throw new InvalidOperationException("The fresh WORD source tree unexpectedly contained a prior key.");
                var rows=new LegacyOwnedMainWordSourceChoiceRow[words.Length];
                for(int i=0;i<words.Length;i++)rows[i]=new(words[i],renderer.RenderSpanWord(words[i],recordResidue40,selectionResidue4,token));
                var payload=new LegacyOwnedMainWordSourcePayload(this,first.Node,rows);payload.Register(token);
                var alternatives=new LegacyOwnedMainWordSourceAlternative[candidates.Length];
                for(int i=0;i<candidates.Length;i++)
                {
                    var intern=i==0?first:shared.Words.InternWordSource(SourceCompletion.SourcesFor(candidates[i],token),token);
                    if(!ReferenceEquals(intern.Node,first.Node))throw new InvalidDataException("Actual same-span WORD key did not reuse its source node.");
                    alternatives[i]=new(candidates[i],payload,!intern.Inserted);
                }
                token.ThrowIfCancellationRequested();return new(this,payload,alternatives,shared.Selections.CopyPendingSelections().ToArray());
            }
            catch{IsFaulted=true;throw;}
        }
    }

    internal void ValidateWord(LegacyOwnedMainSpanWordView word,CancellationToken token)
    {
        ObjectDisposedException.ThrowIf(disposed,this);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(word.Owner,this)||(uint)word.WordOrdinal>=(uint)words.Length||
            !ReferenceEquals(words[word.WordOrdinal],word))throw new ArgumentException("Exact actual275 WORD view required.");
        foreach(var candidate in candidates)candidate.Validate(token);
        var current=publisher.CaptureSpan(Sources.Word,heads,token);
        if(current.Words.Count!=snapshot.Words.Count||current.Words.Where((r,i)=>!ReferenceEquals(r,snapshot.Words[i])).Any())
            throw new InvalidOperationException("The actual275 WORD array changed after preparation.");
    }
    public void Dispose(){if(disposed)return;disposed=true;renderer.Dispose();shared.Dispose();}
}
