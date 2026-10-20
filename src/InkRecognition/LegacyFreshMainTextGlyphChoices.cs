using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Actual selected WORD reference and its refreshed, filtered,
/// classified UTF8 alternatives. Document source endpoints are separate.</summary>
public sealed class LegacyOwnedMainTextGlyphChoices
{
    public ulong SelectedReference { get; }
    public LegacyOwnedMainRefreshedGlyphAlternatives? OrdinaryRefreshed { get; }
    public LegacyOwnedMainRefreshedSpanGlyphOwner? SpanRefreshed { get; }
    public LegacyPreparedGlyphAlternativeSelection Selection { get; }
    public LegacyNativeTextGlyphCallerResult Published { get; }
    public LegacyOwnedMainGlyphDocumentPositions SourcePositions { get; }
    internal LegacyOwnedMainTextGlyphChoices(ulong reference, LegacyOwnedMainRefreshedGlyphAlternatives? refreshed,
        LegacyOwnedMainRefreshedSpanGlyphOwner? span, LegacyPreparedGlyphAlternativeSelection selection,
        LegacyNativeTextGlyphCallerResult published, LegacyOwnedMainGlyphDocumentPositions positions)
    { SelectedReference = reference; OrdinaryRefreshed = refreshed; SpanRefreshed = span; Selection = selection;
      Published = published; SourcePositions = positions; }
    /// <summary>No fallback to a different alternative when the native selected
    /// marker was removed or is unavailable.</summary>
    public byte[]? CopySelectedUtf8() => (uint)Published.AdjustedMarkerIndex < (uint)Published.Records.Count
        ? Published.Records[Published.AdjustedMarkerIndex].Text.ReadBytes() : null;
}

public sealed class LegacyOwnedMainTextWordChoices
{
    public LegacyNativePublishedWordNode Word { get; }
    public IReadOnlyList<LegacyOwnedMainTextGlyphChoices> Glyphs { get; }
    internal LegacyOwnedMainTextWordChoices(LegacyNativePublishedWordNode word, LegacyOwnedMainTextGlyphChoices[] glyphs)
    { Word = word; Glyphs = Array.AsReadOnly(glyphs); }
}

/// <summary>Actual selected-candidate source refresh ->273.V50 ->213A0 ->21000 join.
/// This owns the fresh string pool and proves wrapperHeaderEFilter=false from
/// the private fresh no-properties lifecycle. It does not implement024F30's
/// source-node geometry/dedup,021760 reuse,0283B0 postprocessing or acceptance.</summary>
public sealed class LegacyFreshMainTextGlyphChoices : IDisposable
{
    private readonly LegacyNativeUtf8StringPool strings;
    private readonly bool ownsStrings;
    private readonly LegacyOwnedMainSpanWordView[]? spanWords;
    private readonly LegacyNativeTextPublicationData publication;
    private readonly LegacyNativeUnicodeNormalizationData normalization;
    private readonly byte[] header;
    private int started;
    private bool disposed;
    private readonly LegacyFreshMainCompletedFirstCandidate? candidate;
    private readonly LegacyOwnedMainWordUtf8Projection[] actualWords;
    public LegacyFreshMainCompletedFirstCandidate Candidate => candidate ??
        throw new InvalidOperationException("Configured choices have no count1 first candidate.");
    public LegacyOwnedMainCandidateSource? CandidateSource { get; }
    public LegacyOwnedMainOrdinaryGlyphRefresh Refresh { get; }
    public LegacyOwnedMainSpanGlyphRefresh SpanRefresh { get; }
    public LegacyFreshMainGlyphUtf8Emission Emitter { get; }

    private LegacyFreshMainTextGlyphChoices(ReadOnlySpan<byte> resource, LegacyFreshMainCompletedFirstCandidate candidate,
        LegacyCursiveSeparationTable table, LegacyNativeTextPublicationData publication,
        LegacyNativeUnicodeNormalizationData normalization, LegacyNativeCircledPayloadData circles, CancellationToken token)
    {
        strings=new();ownsStrings=true;
        this.candidate = candidate; this.publication = publication; this.normalization = normalization;
        actualWords = candidate.RawWordsInNativeHeadOrder.ToArray();
        var source = candidate.Snapshot.SourcePolicy; var raw = candidate.RawResult;
        if (!ReferenceEquals(candidate.GlyphNormalization.Snapshot, candidate.Snapshot) ||
            !ReferenceEquals(candidate.GlyphNormalization.Policy.SourcePolicy, source) ||
            !ReferenceEquals(candidate.Snapshot.Session, raw.Session) ||
            !ReferenceEquals(candidate.Snapshot.WordHead18, raw.Session.Ownership.WordPublisher.Head) ||
            source.HasAssociatedSource || source.Controller.Configuration.Mode22f6 != 0)
            throw new ArgumentException("Completed actual fresh first candidate and unchanged source/head ownership required.");
        Refresh = LegacyOwnedMainOrdinaryGlyphRefresh.Prepare(resource, raw, table, token);
        SpanRefresh = LegacyOwnedMainSpanGlyphRefresh.Prepare(raw, token);
        Emitter = LegacyFreshMainGlyphUtf8Emission.Prepare(raw, normalization, circles, token);
        header = resource.Slice(99, 16).ToArray();
    }

    private LegacyFreshMainTextGlyphChoices(ReadOnlySpan<byte> resource, LegacyOwnedMainCandidateSource candidate,
        LegacyCursiveSeparationTable table, LegacyNativeTextPublicationData publication,
        LegacyNativeUnicodeNormalizationData normalization, LegacyNativeCircledPayloadData circles, CancellationToken token,
        LegacyNativeUtf8StringPool? sharedStrings=null,LegacyOwnedMainSpanWordView[]? actualSpanWords=null)
    {
        strings=sharedStrings??new();ownsStrings=sharedStrings is null;spanWords=actualSpanWords?.ToArray();
        CandidateSource = candidate; this.publication = publication; this.normalization = normalization;
        candidate.Validate(token);
        if (!candidate.IsConfigured) throw new ArgumentException("An actual configured candidate is required.");
        actualWords = candidate.CopyRawWordsInNativeHeadOrder(token);
        Refresh = LegacyOwnedMainOrdinaryGlyphRefresh.PrepareConfigured(resource, candidate, table, token);
        SpanRefresh = LegacyOwnedMainSpanGlyphRefresh.PrepareConfigured(candidate, token);
        Emitter = LegacyFreshMainGlyphUtf8Emission.PrepareConfigured(candidate, normalization, circles, token);
        header = resource.Slice(99, 16).ToArray();
    }

    internal static LegacyFreshMainTextGlyphChoices PrepareSpanWords(ReadOnlySpan<byte> resource,
        LegacyOwnedMainCandidateSource candidate,LegacyOwnedMainSpanWordView[] actualWords,
        LegacyNativeUtf8StringPool sharedStrings,LegacyCursiveSeparationTable table,
        LegacyNativeTextPublicationData publication,LegacyNativeUnicodeNormalizationData normalization,
        LegacyNativeCircledPayloadData circles,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(sharedStrings);ArgumentNullException.ThrowIfNull(actualWords);
        candidate.Validate(token);
        if(actualWords.Length==0||actualWords.Any(w=>!ReferenceEquals(w.Owner.TopOwner,candidate.TopOwner)||
            !ReferenceEquals(w.Owner.Sources.CandidateSource.SourceIdentity,candidate.SourceIdentity)))
            throw new ArgumentException("Actual275 WORD views from this identical source/TOP required.");
        return new(resource,candidate,table,publication,normalization,circles,token,sharedStrings,actualWords);
    }

    /// <summary>Same refresh/selection/TOP.V50/0213A0 publication over a genuine
    /// configured alternative. The actual candidate/session and shared TOP are
    /// retained; this does not run024F30 word/source interning or acceptance.</summary>
    public static LegacyFreshMainTextGlyphChoices PrepareConfigured(ReadOnlySpan<byte> resource,
        LegacyOwnedMainCandidateSource candidate, LegacyCursiveSeparationTable table,
        LegacyNativeTextPublicationData publication, LegacyNativeUnicodeNormalizationData normalization,
        LegacyNativeCircledPayloadData circles, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(candidate); ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(publication); ArgumentNullException.ThrowIfNull(normalization);
        ArgumentNullException.ThrowIfNull(circles); token.ThrowIfCancellationRequested();
        return new(resource, candidate, table, publication, normalization, circles, token);
    }

    public static LegacyFreshMainTextGlyphChoices Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainCompletedFirstCandidate candidate, LegacyCursiveSeparationTable table,
        LegacyNativeTextPublicationData publication, LegacyNativeUnicodeNormalizationData normalization,
        LegacyNativeCircledPayloadData circles,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(candidate); ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(publication); ArgumentNullException.ThrowIfNull(normalization);
        ArgumentNullException.ThrowIfNull(circles);
        token.ThrowIfCancellationRequested(); return new(resource, candidate, table, publication, normalization, circles, token);
    }

    internal LegacyNativeUtf8StringPool StringPool
    {get{ObjectDisposedException.ThrowIf(disposed,this);return strings;}}

    internal void BeginPublication(ReadOnlySpan<byte> allocatedRecordStorage40,
        ReadOnlySpan<byte> selectionAllocation4,CancellationToken token)
    {
        ObjectDisposedException.ThrowIf(disposed,this);token.ThrowIfCancellationRequested();
        CandidateSource?.Validate(token);
        if(allocatedRecordStorage40.Length!=40||selectionAllocation4.Length%4!=0||selectionAllocation4.Length>65536*4)
            throw new ArgumentException("Explicit complete row and bounded whole selection allocation required.");
        if(Interlocked.CompareExchange(ref started,1,0)!=0)
            throw new InvalidOperationException("This text-choice publication has already started, including failed prefixes.");
    }

    private void CheckWord(LegacyNativePublishedWordNode word)
    {
        ObjectDisposedException.ThrowIf(disposed,this);
        if(started!=1||!actualWords.Any(p=>ReferenceEquals(p.Publication.PublishedWord,word)))
            throw new ArgumentException("Publication must have started for an actual WORD of this identical candidate.");
    }
    private void CheckSpanWord(LegacyOwnedMainSpanWordView word,CancellationToken token)
    {
        ObjectDisposedException.ThrowIf(disposed,this);
        if(started!=1||spanWords is null||!spanWords.Contains(word))
            throw new ArgumentException("Publication must have started for an actual275 WORD view of this owner.");
        word.Validate(token);
    }
    private void CheckSources(LegacyPreparedMainTextWordSources source,CancellationToken token)
    {
        if(!ReferenceEquals(source.Owner,this))throw new ArgumentException("Same actual WORD source owner required.");
        if(source.PublishedWord is { } node)CheckWord(node);else CheckSpanWord(source.SpanWord!,token);
    }

    internal LegacyPreparedMainTextWordSources RefreshWordSources(LegacyNativePublishedWordNode word,
        ReadOnlySpan<byte> allocatedRecordStorage40,CancellationToken token)
        =>RefreshWordSourcesCore(word,null,allocatedRecordStorage40,false,[],token);

    internal LegacyPreparedMainTextWordSources RefreshWordSourcesForWrapper(LegacyNativePublishedWordNode word,
        ReadOnlySpan<byte> allocatedRecordStorage40,ReadOnlySpan<LegacyNativeInkPositionRange> actualWordClip,CancellationToken token)
        =>RefreshWordSourcesCore(word,null,allocatedRecordStorage40,true,actualWordClip,token);

    internal LegacyPreparedMainTextWordSources RefreshSpanWordSourcesForWrapper(LegacyOwnedMainSpanWordView word,
        ReadOnlySpan<byte> allocatedRecordStorage40,ReadOnlySpan<LegacyNativeInkPositionRange> actualWordClip,CancellationToken token)
        =>RefreshWordSourcesCore(null,word,allocatedRecordStorage40,true,actualWordClip,token);

    private LegacyPreparedMainTextWordSources RefreshWordSourcesCore(LegacyNativePublishedWordNode? word,
        LegacyOwnedMainSpanWordView? spanWord,
        ReadOnlySpan<byte> allocatedRecordStorage40,bool wrapperFirstPass,
        ReadOnlySpan<LegacyNativeInkPositionRange> actualWordClip,CancellationToken token)
    {
        if(word is not null)CheckWord(word);else CheckSpanWord(spanWord!,token);token.ThrowIfCancellationRequested();
        var record=word?.Word??spanWord!.Record;
        CandidateSource?.Validate(token);
        var pool=Emitter.TopOwner.GlyphPool;
        var positions=new Dictionary<ulong,LegacyOwnedMainGlyphDocumentPositions>();
        var descriptors=new LegacyNativeGlyphSymbolDescriptor[record.FlatReferences.Count];int descriptorIndex=0;
        var initialRanges=new LegacyNativeInkPositionRange[record.FlatReferences.Count][];
        var document=CandidateSource?.SourceDocument??Candidate.RawResult.Owner.SourceDocument;var rangeOperations=LegacyOwnedRawInkRangeOperations.Prepare(document);
        LegacyNativeInkPositionRange[] originalRanges=[new(LegacyNativeInkPosition.Pack(0,0,3,document.Strokes[0].Points.Count),
            LegacyNativeInkPosition.Pack((uint)document.ObjectCount-1,document.Strokes[^1].Points.Count-1,5,document.Strokes[^1].Points.Count))];
        //Every actual WORD reference is refreshed before any of its ordered
        //alternative selections. This seam deliberately does not call213A0.
        foreach(ulong reference in record.FlatReferences)
        {
            token.ThrowIfCancellationRequested();var storage=pool.CopyRecord(reference,token);
            var descriptor=new LegacyNativeGlyphSymbolDescriptor(BinaryPrimitives.ReadUInt16LittleEndian(storage.AsSpan(4)),
                BinaryPrimitives.ReadUInt16LittleEndian(storage.AsSpan(8)));
            descriptors[descriptorIndex++]=descriptor;
            if(wrapperFirstPass&&IsWrapperSkipped(descriptor))continue;
            ulong key=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference);
            if(unchecked((short)(key>>24))>=0)
            {
                var refreshed=Refresh.RefreshOwner(key,allocatedRecordStorage40,token);
                if(!positions.ContainsKey(key))positions.Add(key,CandidateSource is { } selected
                    ?document.ProjectCandidate(selected,refreshed.Source,token)
                    :document.Project(Candidate.RawResult,refreshed.Source,token));
            }
            else
            {
                var refreshed=SpanRefresh.RefreshOwner(key,token);
                if(!positions.ContainsKey(key))positions.Add(key,CandidateSource is { } selected
                    ?document.ProjectSpanCandidate(selected,refreshed.Source,token)
                    :document.Project(Candidate.RawResult,refreshed.Source,token));
            }
            var range=positions[key].Ranges.ToArray();
            initialRanges[descriptorIndex-1]=wrapperFirstPass&&unchecked((short)(key>>24))>=0
                ?rangeOperations.Intersect(range,actualWordClip,token):range;
        }
        return new(this,word,spanWord,positions,descriptors,initialRanges,originalRanges,wrapperFirstPass);
    }

    internal LegacyPreparedMainTextGlyphSelection PrepareGlyph(LegacyPreparedMainTextWordSources sources,int index,
        ReadOnlySpan<byte> allocatedRecordStorage40,ReadOnlySpan<byte> selectionAllocation4,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(sources);CheckSources(sources,token);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(sources.Owner,this)||(uint)index>=(uint)sources.Record.FlatReferences.Count)
            throw new ArgumentException("Actual WORD first-pass sources and index from this identical publication required.");
        if(sources.WrapperFirstPass&&IsWrapperSkipped(sources.ReadDescriptor(index)))
            throw new InvalidOperationException("Native024F30 skipped source has no glyph-selection request.");
        ulong reference=sources.Record.FlatReferences[index];
        ulong key=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference);
        var pool=Emitter.TopOwner.GlyphPool;
        LegacyOwnedMainRefreshedGlyphAlternatives? ordinary=null;LegacyOwnedMainRefreshedSpanGlyphOwner? span=null;
        LegacyPreparedGlyphAlternativeSelection selection;
        if(unchecked((short)(key>>24))>=0)
        {ordinary=Refresh.Select(reference,allocatedRecordStorage40,selectionAllocation4,token:token);selection=ordinary.Selection;}
        else
        {
            span=SpanRefresh.RefreshOwner(key,token);
            uint marker=unchecked((uint)(int)(short)(reference>>40));
            selection=LegacyPreparedGlyphAlternativeSelection.Select(pool.CopyOwnerNode(key,token),marker,selectionAllocation4,cancellationToken:token);
        }
        if(!selection.Succeeded)throw new NotSupportedException("Actual refreshed glyph selection allocation refused; no accepted text fabricated.");
        var owner=new LegacyNativeGlyphRecordOwner([pool.CopyOwnerNode(key,token)]);
        return new(this,sources,index,reference,ordinary,span,selection,owner,sources.Read(key));
    }

    internal static bool IsWrapperSkipped(LegacyNativeGlyphSymbolDescriptor descriptor)
        =>(descriptor.KindFlags&7)==1&&(descriptor.SymbolOrRunLength&0x7ffc)!=0&&(descriptor.SymbolOrRunLength&3)==3;

    internal LegacyNativeTextGlyphCallerResult PublishFreshGlyph(LegacyPreparedMainTextGlyphSelection input,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(input);CheckSources(input.Sources,token);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(input.Owner,this))throw new ArgumentException("Same publication selection owner required.");
        //0283B0 zeroes local138.byte8 before024F30 forwards it to0213A0.
        //Only a prior-result property12D0065 can overwrite that byte. This
        //fresh no-prior-result route excludes that branch; knowledge99 is
        //a separate value, not the direct producer of this wrapper filter.
        return LegacyNativeTextGlyphCaller.Produce(input.GlyphOwner,Emitter.PrimaryAlphabet,
            header,input.Selection.CopySelectionRows4(),LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(input.Reference),false,
            unchecked((int)input.Selection.SelectedMarker!.Value),strings,publication,normalization,
            actualControlLabelLookup:text=>Emitter.ReadControlLabel(text,token),
            actualFullGlyphEmitter:glyph=>Emitter.Emit(glyph,token),cancellationToken:token);
    }

    public IReadOnlyList<LegacyOwnedMainTextWordChoices> Publish(ReadOnlySpan<byte> allocatedRecordStorage40,
        ReadOnlySpan<byte> selectionAllocation4, CancellationToken token = default)
    {
        BeginPublication(allocatedRecordStorage40,selectionAllocation4,token);
        var words=new List<LegacyOwnedMainTextWordChoices>();int referenceCount=0,recordCount=0,byteCount=0;
        foreach(var projected in actualWords)
        {
            token.ThrowIfCancellationRequested();var word=projected.Publication.PublishedWord!;var refs=word.Word.FlatReferences;
            if(refs.Count>4096-referenceCount)throw new NotSupportedException("Total text-choice source-reference budget exceeded.");
            referenceCount+=refs.Count;var sources=RefreshWordSources(word,allocatedRecordStorage40,token);
            var glyphs=new List<LegacyOwnedMainTextGlyphChoices>();
            for(int index=0;index<refs.Count;index++)
            {
                var input=PrepareGlyph(sources,index,allocatedRecordStorage40,selectionAllocation4,token);
                var published=PublishFreshGlyph(input,token);
                if(published.Records.Count>65536-recordCount)throw new NotSupportedException("Total text-choice publication record budget exceeded.");
                recordCount+=published.Records.Count;
                foreach(var record in published.Records)
                {if(record.ByteCount>8_388_608-byteCount)throw new NotSupportedException("Total text-choice UTF8 budget exceeded.");byteCount+=(int)record.ByteCount;}
                glyphs.Add(new(input.Reference,input.Ordinary,input.Span,input.Selection,published,input.SourcePositions));
            }
            words.Add(new(word,glyphs.ToArray()));
        }
        token.ThrowIfCancellationRequested();return words.AsReadOnly();
    }

    public void Dispose() { if (disposed) return; disposed = true; if(ownsStrings)strings.Dispose(); }
}

//Internal preparation seams retain the real candidate/WORD/source/selection
//identities. They cannot be constructed by application callers.
internal sealed class LegacyPreparedMainTextWordSources
{
    private readonly Dictionary<ulong,LegacyOwnedMainGlyphDocumentPositions> positions;
    internal LegacyFreshMainTextGlyphChoices Owner{get;}
    internal LegacyNativePublishedWordNode? PublishedWord{get;}
    internal LegacyOwnedMainSpanWordView? SpanWord{get;}
    internal LegacyNativePlainWordRecord Record=>PublishedWord?.Word??SpanWord!.Record;
    internal LegacyNativePublishedWordNode Word=>PublishedWord??throw new InvalidOperationException("An actual275 payload view is not a279 published node.");
    private readonly LegacyNativeGlyphSymbolDescriptor[] descriptors;
    private readonly LegacyNativeInkPositionRange[][] initialRanges;
    private readonly LegacyNativeInkPositionRange[] originalRanges;
    internal bool WrapperFirstPass{get;}
    internal LegacyPreparedMainTextWordSources(LegacyFreshMainTextGlyphChoices owner,LegacyNativePublishedWordNode? word,
        LegacyOwnedMainSpanWordView? spanWord,
        Dictionary<ulong,LegacyOwnedMainGlyphDocumentPositions> positions,LegacyNativeGlyphSymbolDescriptor[] descriptors,
        LegacyNativeInkPositionRange[][] initialRanges,LegacyNativeInkPositionRange[] originalRanges,bool wrapperFirstPass)
    {Owner=owner;PublishedWord=word;SpanWord=spanWord;this.positions=positions;this.descriptors=descriptors;this.initialRanges=initialRanges;
     this.originalRanges=originalRanges;WrapperFirstPass=wrapperFirstPass;}
    internal LegacyNativeInkPositionRange[] CopyInitialRanges(int index)=>initialRanges[index]?.ToArray()??[];
    internal LegacyNativeInkPositionRange[] CopyOriginalRanges()=>originalRanges.ToArray();
    internal LegacyNativeGlyphSymbolDescriptor ReadDescriptor(int index)=>descriptors[index];
    internal LegacyOwnedMainGlyphDocumentPositions Read(ulong key)=>positions[key];
}
internal sealed class LegacyPreparedMainTextGlyphSelection
{
    internal LegacyFreshMainTextGlyphChoices Owner{get;}
    internal LegacyPreparedMainTextWordSources Sources{get;}
    internal LegacyNativePublishedWordNode Word=>Sources.Word;
    internal int Index{get;}
    internal ulong Reference{get;}
    internal LegacyOwnedMainRefreshedGlyphAlternatives? Ordinary{get;}
    internal LegacyOwnedMainRefreshedSpanGlyphOwner? Span{get;}
    internal LegacyPreparedGlyphAlternativeSelection Selection{get;}
    internal LegacyNativeGlyphRecordOwner GlyphOwner{get;}
    internal LegacyOwnedMainGlyphDocumentPositions SourcePositions{get;}
    internal LegacyPreparedMainTextGlyphSelection(LegacyFreshMainTextGlyphChoices owner,LegacyPreparedMainTextWordSources sources,int index,
        ulong reference,LegacyOwnedMainRefreshedGlyphAlternatives? ordinary,LegacyOwnedMainRefreshedSpanGlyphOwner? span,
        LegacyPreparedGlyphAlternativeSelection selection,LegacyNativeGlyphRecordOwner glyphOwner,LegacyOwnedMainGlyphDocumentPositions positions)
    {Owner=owner;Sources=sources;Index=index;Reference=reference;Ordinary=ordinary;Span=span;Selection=selection;GlyphOwner=glyphOwner;SourcePositions=positions;}
}
