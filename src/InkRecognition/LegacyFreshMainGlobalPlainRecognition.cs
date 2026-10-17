namespace InkRecognition;

/// <summary>Reusable-context entry to raw preprocessing, the actual global
/// main controller, final publication before selection and plain WORD callbacks.
/// This bounded composition does not accept transcription or enable app OCR.</summary>
public sealed class LegacyFreshMainGlobalPlainRecognition
{
    private readonly byte[] acoustic, linguistic;
    private readonly LegacyRussianModelBundle model;
    private readonly LegacyInkActivations activations;
    private readonly LegacyCursiveSeparationTable table;
    private readonly LegacyNativeCaseData caseData;
    private readonly LegacyNativeTextPublicationData publication;
    private int started;
    public LegacyFreshMainOwnedTextInputPreparation Input { get; }
    public LegacyOwnedRawInkDocument SourceDocument { get; }

    private LegacyFreshMainGlobalPlainRecognition(byte[] acoustic, byte[] linguistic,
        LegacyRussianModelBundle model, LegacyInkActivations activations,
        LegacyCursiveSeparationTable table, LegacyNativeCaseData caseData,
        LegacyNativeTextPublicationData publication, LegacyFreshMainOwnedTextInputPreparation input,
        LegacyOwnedRawInkDocument document)
    {
        this.acoustic=acoustic; this.linguistic=linguistic; this.model=model;
        this.activations=activations; this.table=table; this.caseData=caseData;
        this.publication=publication; Input=input; SourceDocument=document;
    }

    internal static LegacyFreshMainGlobalPlainRecognition PrepareOwnedModel(byte[] acoustic,
        byte[] linguistic, LegacyRussianModelBundle model, LegacyInkActivations activations,
        byte[] spatial30, byte[] physical50, IReadOnlyList<InkStroke> raw, float resolution,
        LegacyCursiveSeparationTable table, LegacyNativeCaseData caseData,
        LegacyNativeTextPublicationData publication, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(table); ArgumentNullException.ThrowIfNull(caseData);
        ArgumentNullException.ThrowIfNull(publication); token.ThrowIfCancellationRequested();
        var document=LegacyOwnedRawInkDocument.Create(raw,token);
        var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(acoustic,spatial30,physical50,
            document.Strokes,resolution,activations,table,caseData,publication,token);
        return new(acoustic,linguistic,model,activations,table,caseData,publication,input,document);
    }

    /// <summary>Explicit allocator/stack storage; single-use once source
    /// processing begins. Logical sources are bounded to240 nodes/one LIVE
    /// cycle. Unsupported reachable native branches still refuse explicitly.</summary>
    public LegacyFreshMainGlobalPlainRecognitionResult Run(ReadOnlySpan<byte> compactAllocatorStorage,
        ReadOnlySpan<byte> wordAllocatorResidue120, ReadOnlySpan<byte> recordAllocatorResidue40,
        ReadOnlySpan<ushort> labelStackResidue256, ReadOnlySpan<byte> requestStackResidue64,
        ReadOnlySpan<byte> backtrackStackResidue72, CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if (compactAllocatorStorage.IsEmpty || compactAllocatorStorage.Length%40!=0 ||
            compactAllocatorStorage.Length>64*1024*1024 || wordAllocatorResidue120.Length!=120 ||
            recordAllocatorResidue40.Length!=40 || labelStackResidue256.Length!=256 ||
            requestStackResidue64.Length!=64 || backtrackStackResidue72.Length!=72)
            throw new ArgumentException("Complete bounded compact40 storage, WORD120, record40, labels256, request64 and row72 required.");
        if (Interlocked.CompareExchange(ref started,1,0)!=0)
            throw new InvalidOperationException("This actual global source invocation already started, including failed prefixes.");
        var preprocessed=Input.Run(token);
        var cuts=LegacyFreshMainDescriptorCutSource.Prepare(acoustic,preprocessed.Smoothed,token);
        var local=LegacyFreshMainDescriptorGraphAppend.Prepare(acoustic,cuts,table,token);
        var global=LegacyFreshMainGlobalOrdinaryState.Prepare(acoustic,local,table,token);
        var graph=LegacyFreshMainCompactOrdinaryGraph.Prepare(acoustic,global,token);
        var source=LegacyFreshMainGlobalSourceRanges.Prepare(graph,token);
        var context=LegacyFreshMainGlobalContext.Prepare(acoustic,source,activations,token);
        var geometry=LegacyFreshMainGlobalGlyphGeometry.Prepare(acoustic,context,token);
        var owner=LegacyFreshMainGlobalCaseSession.Prepare(acoustic,linguistic,model,geometry,caseData,publication,token);
        var cache=LegacyFreshMainGlobalTransitionCache.Prepare(acoustic,owner,table,token);
        var transitions=LegacyFreshMainGlobalWordTransitions.Prepare(acoustic,cache,token);
        var costs=LegacyFreshMainGlobalGroupCosts.Prepare(acoustic,owner,table,token);
        var policy=LegacyFreshMainGlobalWordControllerPolicy.Prepare(acoustic,owner,costs,transitions,token);
        var confidence=LegacyFreshMainGlobalGlyphConfidence.Prepare(acoustic,owner,token);
        var invocation=LegacyFreshMainGlobalTerminalInvocation.Prepare(policy,token);
        ulong identity=1;
        LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest request)
        {
            token.ThrowIfCancellationRequested();
            var bytes=new byte[request.RequiredBytes]; request.PreviousStorage.Span.CopyTo(bytes);
            return new(bytes,identity++);
        }
        var pass=LegacyFreshMainGlobalWordPass.CreateWithIntermediatePublication(acoustic,policy,
            Allocate,confidence,compactAllocatorStorage,token:token).RunOwned(token);
        int evaluations=owner.AcousticEvaluations;
        var final=LegacyFreshMainGlobalFinalPublication.Append(pass,confidence,acoustic,token);
        var selected=invocation.SelectPublished(final,token);
        if (!selected.Winner.HasWinner)
            return new(this,preprocessed,selected,null,[],[],evaluations);

        var sourceGeometry=LegacyFreshMainGlobalSourceGeometry.Prepare(acoustic,policy,confidence,token);
        var callbacks=LegacyFreshMainGlobalSourceSymbolCallbacks.Prepare(acoustic,sourceGeometry,token);
        var backtrack=LegacyFreshMainGlobalGlyphBacktrackRows.PreparePublished(acoustic,selected,
            callbacks,backtrackStackResidue72,token);
        var plan=LegacyFreshMainGlobalGlyphFlushPlan.Prepare(backtrack,token);
        var ownership=LegacyFreshMainGlobalPlainResultOwnership.CreatePublished(selected,sourceGeometry,token);
        var session=LegacyFreshMainGlobalGlyphExecutionSession.Prepare(acoustic,ownership,plan,wordAllocatorResidue120,token);
        var publications=new List<LegacyFreshMainGlobalGlyphExecutionResult>();
        while (!session.IsComplete)
        {
            var current=session.ExecuteNext(recordAllocatorResidue40,labelStackResidue256,requestStackResidue64,token);
            publications.Add(current);
            if (!current.Succeeded) break;
        }
        var projected=new List<LegacyFreshMainGlobalWordUtf8Projection>();
        if (session.IsComplete)
        {
            var byNode=publications.Where(p=>p.PublishedWord is not null).ToDictionary(p=>p.PublishedWord!);
            for (var node=ownership.WordPublisher.Head; node is not null; node=node.Next)
            {
                token.ThrowIfCancellationRequested();
                if (!byNode.TryGetValue(node,out var actual))
                    throw new InvalidDataException("Actual WORD list lost its original global callback.");
                projected.Add(LegacyFreshMainGlobalWordUtf8Projection.Project(actual,token));
            }
        }
        if (owner.AcousticEvaluations!=evaluations)
            throw new InvalidOperationException("Final publication and WORD execution cannot reevaluate main NN groups.");
        pass.ValidateResultQueryLease(token);
        return new(this,preprocessed,selected,session,publications.ToArray(),projected.ToArray(),evaluations);
    }
}

/// <summary>Raw owning result or explicit no-winner. An empty WORD array for
/// no-winner is not an accepted empty transcription. Failed callback prefixes
/// remain available through the actual session and publication tokens.</summary>
public sealed class LegacyFreshMainGlobalPlainRecognitionResult
{
    public LegacyFreshMainGlobalPlainRecognition Owner { get; }
    public LegacyFreshMainOwnedTextPreprocessedInput Preprocessed { get; }
    public LegacyFreshMainGlobalPublishedTerminalResult Selection { get; }
    public LegacyFreshMainGlobalGlyphExecutionSession? Session { get; }
    public IReadOnlyList<LegacyFreshMainGlobalGlyphExecutionResult> Publications { get; }
    public IReadOnlyList<LegacyFreshMainGlobalWordUtf8Projection> RawWordsInNativeHeadOrder { get; }
    public bool HasWinner=>Selection.Winner.HasWinner;
    public bool PublicationCompleted=>Session?.IsComplete==true;
    public int CachedMainGroupEvaluations { get; }
    internal LegacyFreshMainGlobalPlainRecognitionResult(LegacyFreshMainGlobalPlainRecognition owner,
        LegacyFreshMainOwnedTextPreprocessedInput preprocessed, LegacyFreshMainGlobalPublishedTerminalResult selection,
        LegacyFreshMainGlobalGlyphExecutionSession? session, LegacyFreshMainGlobalGlyphExecutionResult[] publications,
        LegacyFreshMainGlobalWordUtf8Projection[] words, int evaluations)
    {
        Owner=owner; Preprocessed=preprocessed; Selection=selection; Session=session;
        Publications=Array.AsReadOnly(publications); RawWordsInNativeHeadOrder=Array.AsReadOnly(words);
        CachedMainGroupEvaluations=evaluations;
    }
}
