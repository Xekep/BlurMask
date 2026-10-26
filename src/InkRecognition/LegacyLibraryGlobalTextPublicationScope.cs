using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyGlobalTextPublication.Probe")]
namespace InkRecognition;

/// <summary>Pre-NN selected-input2DA/3E lifetime for a distinct actual global
/// invocation. Completes the bounded live TOP.V68 source stage, without control
/// import, CHAR/TEXT publication, document transport or acceptance.</summary>
public sealed partial class LegacyLibraryGlobalTextPublicationScope : IDisposable
{
    private readonly object gate=new();
    private readonly LegacyLibraryTextProcessingLease lease;
    private readonly LegacyFreshMainRecognitionContext models;
    private readonly LegacyMainSharedTextSourceState shared;
    private LegacyFreshMainGlobalPlainRecognition? recognition;
    private LegacyFreshMainGlobalPlainRecognitionResult? result;
    private bool disposed,started,enteredCompletion,faulted;
    public LegacyLibraryTextProcessingContext Context {get;}
    public LegacyOwnedRawInkDocument Document=>Context.Document;
    public LegacyFreshMainTextDocumentSeed Seed=>shared.DocumentSeed!;
    private LegacyGlobalSourceCallerResidue? completionResidue;
    public LegacyFreshMainGlobalSourceCompletion? SourceCompletion {get;private set;}
    public bool IsFaulted {get{lock(gate)return faulted;}}
    internal LegacyMainSharedTextSourceState SharedState=>shared;
    private LegacyLibraryGlobalTextPublicationScope(LegacyLibraryTextProcessingContext context,
        LegacyLibraryTextProcessingLease lease,LegacyFreshMainRecognitionContext models)
    {Context=context;this.lease=lease;this.models=models;shared=new(this,context);}

    public static LegacyLibraryGlobalTextPublicationScope PrepareOriginalSelected(
        LegacyFreshMainRecognitionContext models,LegacyLibraryOriginalSelectedFieldContext actualSelectedInput,
        float resolution,LegacyCursiveSeparationTable table,LegacyNativeCaseData caseData,
        LegacyNativeTextPublicationData publication,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(models);ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(caseData);ArgumentNullException.ThrowIfNull(publication);
        token.ThrowIfCancellationRequested();
        var context=LegacyLibraryTextProcessingContext.CreateForOriginalSelected(actualSelectedInput,token);
        LegacyLibraryTextProcessingLease? lease=null;LegacyLibraryGlobalTextPublicationScope? scope=null;
        try
        {
            context.PrepareInput(token);lease=context.RetainForPublication();
            scope=new(context,lease,models);
            scope.recognition=models.PrepareGlobalDocument(context.Document,resolution,table,caseData,publication,token);
            return scope;
        }
        catch{if(scope is not null)scope.Dispose();else{context.Dispose();lease?.Dispose();}throw;}
    }

    public LegacyFreshMainGlobalPlainRecognitionResult Run(ReadOnlySpan<byte> compactAllocatorStorage,
        ReadOnlySpan<byte> wordAllocatorResidue120,ReadOnlySpan<byte> recordAllocatorResidue40,
        ReadOnlySpan<ushort> labelStackResidue256,ReadOnlySpan<byte> requestStackResidue64,
        ReadOnlySpan<byte> backtrackStackResidue72,CancellationToken token=default)
    {
        lock(gate)
        {
            Validate();token.ThrowIfCancellationRequested();
            if(started)throw new InvalidOperationException("This adopted global invocation is single-use, including failed prefixes.");
            started=true;
            try{
                completionResidue=new(wordAllocatorResidue120,recordAllocatorResidue40,labelStackResidue256,requestStackResidue64,backtrackStackResidue72);
                return result=recognition!.Run(compactAllocatorStorage,completionResidue.Word,completionResidue.Glyph,
                completionResidue.Labels,completionResidue.Request,completionResidue.Backtrack,token);}
            catch{faulted=true;throw;}
        }
    }

    /// <summary>Capture, native quote cleanup, proven fresh-F4 return, then
    /// source count/fill, ranking and275 scores. Imported2A is never created.</summary>
    public LegacyFreshMainGlobalSourceCompletion CompleteSources(ReadOnlySpan<byte> allocation400,
        Span<byte> leadingScratch4,Span<byte> trailingScratch4,CancellationToken token=default)
    {
        lock(gate)
        {
            Validate();token.ThrowIfCancellationRequested();
            if(allocation400.Length!=400||leadingScratch4.Length!=4||trailingScratch4.Length!=4||leadingScratch4.Overlaps(trailingScratch4))
                throw new ArgumentException("Complete alternative400 and two distinct caller scratch4 slots required.");
            if(result is null||!ReferenceEquals(result.Owner.SourceDocument,Document)||!result.HasWinner||!result.PublicationCompleted)
                throw new InvalidOperationException("The same completed positive raw global invocation is required.");
            if(enteredCompletion)throw new InvalidOperationException("Global source completion is single-use.");
            enteredCompletion=true;
        }
        // Shared GLOBAL preflight may call back into scope validation. Never
        // hold the scope gate while waiting for that cancellable session lease.
        try
        {
            var completion=LegacyFreshMainGlobalSourceCompletion.Prepare(this,result!,models.AcousticResource,allocation400,token);
            lock(gate){Validate();SourceCompletion=completion;}
            completion.Run(completionResidue!,leadingScratch4,trailingScratch4,token);
            lock(gate){Validate();shared.BindActualGlobalTop(result!.Session!.Ownership);return completion;}
        }
        catch{lock(gate)faulted=true;throw;}
    }
    internal void Validate(){lock(gate){ObjectDisposedException.ThrowIf(disposed,this);lease.Validate();if(faulted)throw new InvalidOperationException("Faulted global publication scope.");}}
    public void Dispose(){DisposeCore();GC.SuppressFinalize(this);}
    private void DisposeCore(){lock(gate){if(disposed)return;disposed=true;try{shared.Dispose();}finally{Context.Dispose();lease.Dispose();}}}
    ~LegacyLibraryGlobalTextPublicationScope(){try{DisposeCore();}catch{/* Deterministic disposal is the contract. */}}
}
