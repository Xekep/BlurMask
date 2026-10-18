namespace InkRecognition;

/// <summary>Explicit library publication lifetime for an input-imported2C1/2DA.
/// The same raw document, selected INPUT/C22, seed and strings3E precede NN
/// preparation. This bounded route uses configured2; it is not049370 resource
/// scheduling, SDK-page history or C20 acceptance. Dispose ends live child
/// operations; detached byte/tree copies remain usable. Do not dispose this
/// scope concurrently with downstream document/DOM operations.</summary>
public sealed class LegacyLibraryTextPublicationScope : IDisposable
{
    private readonly object gate=new();
    private readonly LegacyLibraryTextProcessingLease lease;
    private readonly LegacyFreshMainRecognitionContext models;
    private readonly LegacyCursiveSeparationTable table;
    private readonly LegacyNativeTextPublicationData publication;
    private readonly LegacyMainSharedTextSourceState shared;
    private LegacyFreshMainConfiguredSingleLinePlainRecognition? recognition;
    private LegacyFreshMainConfiguredSingleLinePlainRecognitionResult? result;
    private LegacyFreshMainWordSourceChoices? words;
    private bool disposed,enteredWords,faulted;
    public LegacyLibraryTextProcessingContext Context {get;}
    public LegacyOwnedRawInkDocument Document=>Context.Document;
    public LegacyFreshMainTextDocumentSeed Seed=>shared.DocumentSeed!;
    public bool IsFaulted {get{lock(gate)return faulted;}}

    private LegacyLibraryTextPublicationScope(LegacyLibraryTextProcessingContext context,
        LegacyLibraryTextProcessingLease lease,LegacyFreshMainRecognitionContext models,
        LegacyCursiveSeparationTable table,LegacyNativeTextPublicationData publication)
    {Context=context;this.lease=lease;this.models=models;this.table=table;this.publication=publication;
     shared=new(this,context);}

    /// <summary>Create/import the selected field and construct its publication
    /// seed before any raw grouping/first-source neural preparation.</summary>
    public static LegacyLibraryTextPublicationScope PrepareConfigured(
        LegacyFreshMainRecognitionContext models,IReadOnlyList<InkStroke> raw,float resolution,
        LegacyCursiveSeparationTable table,LegacyNativeCaseData caseData,
        LegacyNativeTextPublicationData publication,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(models);ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(caseData);ArgumentNullException.ThrowIfNull(publication);
        token.ThrowIfCancellationRequested();
        var document=LegacyOwnedRawInkDocument.Create(raw,token);
        var input=LegacyFreshTextFieldInputContext.Prepare(LegacyFreshRawInkCurrentView.Prepare(document,token),token).CreateSelectedCopy(token);
        var context=LegacyLibraryTextProcessingContext.CreateFresh(input,token);
        LegacyLibraryTextProcessingLease? lease=null;LegacyLibraryTextPublicationScope? scope=null;
        try
        {
            context.PrepareInput(token);lease=context.RetainForPublication();
            scope=new(context,lease,models,table,publication);
            scope.recognition=models.PrepareConfiguredDocument(document,resolution,table,caseData,publication,token);
            return scope;
        }
        catch{if(scope is not null)scope.Dispose();else{context.Dispose();lease?.Dispose();}throw;}
    }

    /// <summary>Pre-NN publication over the exact selected C20/field copied
    /// from a retained original bootstrap. Creates its2C1/result/shared3E
    /// before recognition; this does not enable original CBB/page acceptance.</summary>
    public static LegacyLibraryTextPublicationScope PrepareConfiguredOriginalSelected(
        LegacyFreshMainRecognitionContext models,LegacyLibraryOriginalSelectedFieldContext actualSelectedInput,
        float resolution,LegacyCursiveSeparationTable table,LegacyNativeCaseData caseData,
        LegacyNativeTextPublicationData publication,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(models);ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(caseData);ArgumentNullException.ThrowIfNull(publication);
        token.ThrowIfCancellationRequested();
        var context=LegacyLibraryTextProcessingContext.CreateForOriginalSelected(actualSelectedInput,token);
        LegacyLibraryTextProcessingLease? lease=null;LegacyLibraryTextPublicationScope? scope=null;
        try
        {
            context.PrepareInput(token);lease=context.RetainForPublication();
            scope=new(context,lease,models,table,publication);
            scope.recognition=models.PrepareConfiguredDocument(context.Document,resolution,table,caseData,publication,token);
            return scope;
        }
        catch{if(scope is not null)scope.Dispose();else{context.Dispose();lease?.Dispose();}throw;}
    }

    public LegacyFreshMainConfiguredSingleLinePlainRecognitionResult Run(
        LegacyMainConfiguredAlternativeResidue residue,LegacyMainAlternativeStorageAllocator allocator,
        ReadOnlySpan<byte> compactResidue40,CancellationToken token=default)
    {
        lock(gate)
        {
            Validate();token.ThrowIfCancellationRequested();
            if(result is not null)throw new InvalidOperationException("This adopted raw recognition is single-use.");
            try{return result=recognition!.Run(residue,allocator,compactResidue40,token);}
            catch{faulted=true;throw;}
        }
    }

    public LegacyFreshMainWordSourceChoices PrepareWords(
        LegacyFreshMainConfiguredNoCandidateSourceCompletion actualCompletion,
        LegacyNativeUnicodeNormalizationData normalization,LegacyNativeCircledPayloadData circles,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualCompletion);
        ArgumentNullException.ThrowIfNull(normalization);ArgumentNullException.ThrowIfNull(circles);
        lock(gate)
        {
            Validate();token.ThrowIfCancellationRequested();
            if(result is null||!ReferenceEquals(actualCompletion.Sources.CandidateSource.ConfiguredResult,result)||
                !ReferenceEquals(result.SourceDocument,Document)||!actualCompletion.IsTopV68Completed||actualCompletion.IsFaulted)
                throw new ArgumentException("Successful source completion of this exact adopted raw invocation required.");
            if(enteredWords)throw new InvalidOperationException("Adopted shared WORD publication is single-use.");
            enteredWords=true;
            try{return words=LegacyFreshMainWordSourceChoices.PrepareAdopted(models.AcousticResource,actualCompletion,
                shared,table,publication,normalization,circles,token);}
            catch{faulted=true;throw;}
        }
    }

    internal void Validate()
    {lock(gate){ObjectDisposedException.ThrowIf(disposed,this);lease.Validate();if(faulted)throw new InvalidOperationException("Faulted publication scope.");}}
    internal LegacyMainSharedTextSourceState SharedState=>shared;
    public void Dispose(){DisposeCore();GC.SuppressFinalize(this);}
    private void DisposeCore()
    {
        lock(gate)
        {
            if(disposed)return;disposed=true;
            try{words?.Dispose();shared.Dispose();}
            finally{Context.Dispose();lease.Dispose();}
        }
    }
    ~LegacyLibraryTextPublicationScope(){try{DisposeCore();}catch{/* Finalizer fallback only; deterministic disposal is the contract. */}}
}
