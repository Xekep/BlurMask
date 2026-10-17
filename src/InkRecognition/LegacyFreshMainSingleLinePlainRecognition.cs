namespace InkRecognition;

/// <summary>Bounded fresh raw ink to the first CASE terminal's plain WORD list.
/// One actual assigned line is required. This does not assemble or normalize
/// TOP alternatives, calibrate acceptance confidence, or enable an app engine.</summary>
public sealed class LegacyFreshMainSingleLinePlainRecognition
{
    private readonly byte[] acoustic, linguistic;
    private readonly LegacyRussianModelBundle model;
    private readonly LegacyInkActivations activations;
    private readonly LegacyCursiveSeparationTable table;
    private readonly LegacyNativeCaseData caseData;
    private readonly LegacyNativeTextPublicationData publication;
    public LegacyFreshMainOwnedTextInputPreparation Input { get; }
    public LegacyOwnedRawInkDocument SourceDocument { get; }
    internal bool UsesOwnedModel(LegacyRussianModelBundle expected,byte[] acousticResource,byte[] linguisticResource) =>
        ReferenceEquals(model,expected) && ReferenceEquals(acoustic,acousticResource) && ReferenceEquals(linguistic,linguisticResource);

    private LegacyFreshMainSingleLinePlainRecognition(byte[] acoustic, byte[] linguistic,
        LegacyRussianModelBundle model, LegacyInkActivations activations,
        LegacyCursiveSeparationTable table, LegacyFreshMainOwnedTextInputPreparation input,
        LegacyNativeCaseData caseData, LegacyNativeTextPublicationData publication, LegacyOwnedRawInkDocument sourceDocument)
    { this.acoustic=acoustic; this.linguistic=linguistic; this.model=model;
      this.activations=activations; this.table=table; Input=input; this.caseData=caseData; this.publication=publication; SourceDocument=sourceDocument; }

    public static LegacyFreshMainSingleLinePlainRecognition Prepare(
        ReadOnlySpan<byte> acousticResource, ReadOnlySpan<byte> linguisticResource,
        ReadOnlySpan<byte> engineProfile, ReadOnlySpan<byte> spatial30, ReadOnlySpan<byte> physical50,
        IReadOnlyList<InkStroke> raw, float resolution, LegacyCursiveSeparationTable table,
        LegacyNativeCaseData caseData, LegacyNativeTextPublicationData publication,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(table); ArgumentNullException.ThrowIfNull(caseData);
        ArgumentNullException.ThrowIfNull(publication); token.ThrowIfCancellationRequested();
        var context=LegacyFreshMainRecognitionContext.Load(acousticResource,linguisticResource,
            engineProfile,spatial30,physical50,token);
        return context.Prepare(raw,resolution,table,caseData,publication,token);
    }

    internal static LegacyFreshMainSingleLinePlainRecognition PrepareOwnedModel(byte[] acousticResource,
        byte[] linguisticResource,LegacyRussianModelBundle model,LegacyInkActivations activations,
        byte[] spatial30,byte[] physical50,IReadOnlyList<InkStroke> raw,float resolution,
        LegacyCursiveSeparationTable table,LegacyNativeCaseData caseData,
        LegacyNativeTextPublicationData publication,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(table);ArgumentNullException.ThrowIfNull(caseData);
        ArgumentNullException.ThrowIfNull(publication);token.ThrowIfCancellationRequested();
        var document=LegacyOwnedRawInkDocument.Create(raw,token);
        return PrepareOwnedDocument(acousticResource,linguisticResource,model,activations,spatial30,physical50,
            document,resolution,table,caseData,publication,token);
    }

    internal static LegacyFreshMainSingleLinePlainRecognition PrepareOwnedDocument(byte[] acousticResource,
        byte[] linguisticResource,LegacyRussianModelBundle model,LegacyInkActivations activations,
        byte[] spatial30,byte[] physical50,LegacyOwnedRawInkDocument document,float resolution,
        LegacyCursiveSeparationTable table,LegacyNativeCaseData caseData,
        LegacyNativeTextPublicationData publication,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(document);ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(caseData);ArgumentNullException.ThrowIfNull(publication);
        token.ThrowIfCancellationRequested();
        var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(acousticResource,spatial30,
            physical50,document.Strokes,resolution,activations,table,caseData,publication,token);
        token.ThrowIfCancellationRequested();
        return new(acousticResource,linguisticResource,model,activations,table,input,caseData,publication,document);
    }

    /// <summary>Allocator and stack residues remain explicit. A completed
    /// publication is not an accepted OCR result or a probability estimate.</summary>
    public LegacyFreshMainSingleLinePlainRecognitionResult Run(
        ReadOnlySpan<byte> wordAllocatorResidue120, ReadOnlySpan<byte> recordAllocatorResidue40,
        ReadOnlySpan<ushort> labelStackResidue256, ReadOnlySpan<byte> requestStackResidue64,
        CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(wordAllocatorResidue120.Length!=120||recordAllocatorResidue40.Length!=40||
            labelStackResidue256.Length!=256||requestStackResidue64.Length!=64)
            throw new ArgumentException("Explicit complete WORD, record, label and request storage required.");
        var preprocessed=Input.Run(token);
        //The existing sampler enforces one genuine descriptor, its complete
        //physical interval and assigned guide. Multiple lines are not flattened.
        var sampled=LegacyFreshMainSingleLineSampler.Prepare(acoustic,preprocessed.Smoothed,token).Sample(table,token);
        var context=LegacyFreshMainSingleLineContext.Prepare(acoustic,sampled,activations,token);
        var dictionary=LegacyFreshCaseRussianDictionaryPreparation.Prepare(acoustic,linguistic,
            model,sampled,caseData,publication,token);
        var owner=new LegacyOwnedMainWordSpanPreparation(acoustic,dictionary,sampled,context,table,token);
        var policy=LegacyFreshCaseMainSourcePolicy.Prepare(acoustic,linguistic,
            LegacyOwnedFreshMainWordControllerPolicy.Prepare(acoustic,owner,token),token);
        ulong identity=1;
        LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest request)
        {
            token.ThrowIfCancellationRequested();
            var bytes=new byte[request.RequiredBytes]; request.PreviousStorage.Span.CopyTo(bytes);
            return new(bytes,identity++);
        }
        var confidence=LegacyOwnedRussianMainGlyphConfidence.Prepare(acoustic,dictionary,context,token);
        int intermediatePublications=0;
        var pass=new LegacyOwnedMainCaseWordPass(acoustic,policy,owner.MainLineAcoustic,table,Allocate)
            .RunOwnedWithIntermediatePublication(acoustic,confidence,new byte[4096*40],
                _=>intermediatePublications++,token);
        var terminal=LegacyOwnedFreshCaseMainTerminalInvocation.Prepare(policy,token).SelectOwnedPass(pass,token);
        if(!terminal.Winner.HasWinner)
            throw new NotSupportedException("Actual first CASE terminal has no winner; no word or confidence can be fabricated.");
        var compact=LegacyOwnedMainCaseCompactWordMaterialization.Materialize(acoustic,policy,terminal,
            confidence,new byte[4096*40],token);
        var callbacks=LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(acoustic,confidence,sampled,token);
        var geometry=LegacyOwnedRussianMainSourceGeometry.Prepare(acoustic,confidence,sampled,token);
        var interior=LegacyOwnedRussianMainInteriorDelimiter.Prepare(acoustic,callbacks,geometry,token);
        var backtrack=LegacyOwnedMainGlyphBacktrackRows.Prepare(acoustic,policy,compact,callbacks,new byte[72],token);
        var plan=LegacyOwnedMainGlyphFlushPlan.Prepare(policy,backtrack,token);
        var ownership=LegacyFreshMainPlainResultOwnership.Create(compact,geometry,token);
        var session=LegacyOwnedMainGlyphFlushSession.Prepare(ownership,plan,wordAllocatorResidue120,token);
        var publisher=LegacyOwnedMainPlainWordPublication.Prepare(acoustic,session,table,callbacks,interior,token);
        var publications=new List<LegacyOwnedMainWordPublicationResult>();
        while(!session.IsComplete)
        {
            token.ThrowIfCancellationRequested();
            var current=session.PrepareNext(token);
            var result=publisher.Publish(current,recordAllocatorResidue40,labelStackResidue256,requestStackResidue64,token);
            publications.Add(result); session.CompleteCurrent(result,token);
            if(!result.Succeeded)break;
        }
        token.ThrowIfCancellationRequested();
        return new(this,preprocessed,sampled,terminal,session,publications.ToArray(),intermediatePublications);
    }
}

public sealed class LegacyFreshMainSingleLinePlainRecognitionResult
{
    public LegacyFreshMainSingleLinePlainRecognition Owner { get; }
    public LegacyFreshMainOwnedTextPreprocessedInput Preprocessed { get; }
    public LegacyFreshMainSingleLineSampled Sampled { get; }
    public LegacyOwnedMainTerminalResult Terminal { get; }
    public LegacyOwnedMainGlyphFlushSession Session { get; }
    public IReadOnlyList<LegacyOwnedMainWordPublicationResult> Publications { get; }
    public bool PublicationCompleted=>Session.IsComplete;
    public int IntermediatePublicationCount { get; }
    internal LegacyFreshMainSingleLinePlainRecognitionResult(LegacyFreshMainSingleLinePlainRecognition owner,
        LegacyFreshMainOwnedTextPreprocessedInput preprocessed,LegacyFreshMainSingleLineSampled sampled,
        LegacyOwnedMainTerminalResult terminal,LegacyOwnedMainGlyphFlushSession session,
        LegacyOwnedMainWordPublicationResult[] publications,int intermediatePublicationCount)
    { Owner=owner; Preprocessed=preprocessed; Sampled=sampled; Terminal=terminal;
      Session=session; Publications=Array.AsReadOnly(publications); IntermediatePublicationCount=intermediatePublicationCount; }
}
