using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyGlobalInputTransforms.Probe")]
namespace InkRecognition;

public readonly record struct LegacyGlobalInputTransformPrefix(string Stage,bool VectorSwapped,bool ExportSucceeded,bool InputSpansCompleted,int TransformCount);
internal enum LegacyGlobalInputTransformStage { Entered,Exported,Swapped,InputSpans,Complete }

/// <summary>Actual global023260→037E80(writeDom=false)→026EF0 branch, using
/// the pre-NN original-selected seed. No synthetic empty document/transform
/// source or configured-two-choice result is admitted.</summary>
public sealed class LegacyFreshMainGlobalInputTransforms
{
    private sealed class Entry{internal bool Claimed;}
    private static readonly ConditionalWeakTable<LegacyFreshMainTextDocumentSeed,Entry> lifetimes=new();
    private readonly object gate=new();
    private readonly List<LegacyGlobalInputTransformPrefix> prefixes=[];
    private readonly LegacyDocumentTransformVector before;
    private LegacyDocumentTransformVector? output;
    private LegacyLibrarySelectedInputIdentityExport? sourceExport;
    private bool entered,swapped,exportSucceeded,inputSpans,complete,faulted;
    public LegacyFreshMainGlobalControlCleanup Cleanup{get;}
    public LegacyFreshMainTextDocumentSeed Seed{get;}
    public int DocumentAlternativeCountD8=>Cleanup.State.Publication.AlternativeCount;
    public bool IsComplete{get{lock(gate)return complete;}}
    public bool IsFaulted{get{lock(gate)return faulted;}}
    public bool VectorWasSwapped{get{lock(gate)return swapped;}}
    public bool ExportSucceeded{get{lock(gate)return exportSucceeded;}}
    public bool FollowingStage026EF0Executed{get{lock(gate)return inputSpans;}}
    public int TransformCount110{get{lock(gate)return output?.Count??0;}}
    public LegacyOwnedRawInkIdentityExport SourceGeometry{get{lock(gate)return(sourceExport??throw new InvalidOperationException("Actual source export has not completed.")).Geometry;}}
    public LegacyLibrarySelectedFieldRegistryEntry OriginalSelectedSourceInput{get{lock(gate)return sourceExport?.OriginalSelectedInput??throw new InvalidOperationException("Actual original selected export required.");}}
    private LegacyFreshMainGlobalInputTransforms(LegacyFreshMainGlobalControlCleanup cleanup)
    {Cleanup=cleanup;Seed=cleanup.State.Publication.Scope.Seed;before=Seed.CurrentTransforms;}
    public static LegacyFreshMainGlobalInputTransforms Prepare(LegacyFreshMainGlobalControlCleanup cleanup,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(cleanup);ValidateSeed(cleanup,token);var seed=cleanup.State.Publication.Scope.Seed;
        var entry=lifetimes.GetValue(seed,_=>new());
        lock(entry)
        {
            token.ThrowIfCancellationRequested();ValidateSeed(cleanup,token);
            if(entry.Claimed)throw new InvalidOperationException("This actual global INPUT transform lifetime is already claimed.");
            if(!ReferenceEquals(seed.CurrentTransforms,seed.InitialTransforms))throw new InvalidOperationException("Actual prepublication vector owner was replaced before this transform stage.");
            var result=new LegacyFreshMainGlobalInputTransforms(cleanup);entry.Claimed=true;return result;
        }
    }
    private static void ValidateSeed(LegacyFreshMainGlobalControlCleanup cleanup,CancellationToken token)
    {
        cleanup.ValidateCurrent(token);var parent=cleanup.State.Publication;var scope=parent.Scope;var seed=scope.Seed;
        if(!ReferenceEquals(seed,scope.Context.Result.PublicationSeed)||!ReferenceEquals(seed.ProcessingResult,scope.Context.Result)||
            !ReferenceEquals(seed.Document,scope.Document)||!ReferenceEquals(seed.PendingSelections,scope.SharedState.Selections)||
            !ReferenceEquals(seed.StringPool,parent.Wrapper.Choices.StringPool)||!ReferenceEquals(seed.InputBinding,scope.Context.InputBinding)||
            seed.OriginalSelectedInputContext is null||seed.SourceTag30!=scope.Context.InputBinding.RegistryId||
            seed.InitialDocumentAlternativeCountD8!=0||parent.AlternativeCount!=1)
            throw new NotSupportedException("Exact actual global one-ALT source30/pre-NN original-selected INPUT/3E/C4D seed required.");
    }
    public LegacyFreshMainGlobalInputTransforms Run(CancellationToken token=default)=>RunCore(null,null,token);
    internal LegacyFreshMainGlobalInputTransforms RunDiagnostic(Action<LegacyGlobalInputTransformStage>? observer,CancellationToken token=default)=>RunCore(null,observer,token);
    internal LegacyFreshMainGlobalInputTransforms RunPreparedExport(LegacyDocumentTransformVector prefix,bool succeeded,CancellationToken token=default)=>RunCore(new(prefix,succeeded),null,token);
    private sealed record PreparedExport(LegacyDocumentTransformVector Prefix,bool Succeeded);
    private LegacyFreshMainGlobalInputTransforms RunCore(PreparedExport? prepared,Action<LegacyGlobalInputTransformStage>? observer,CancellationToken token)
    {
        lock(gate)
        {
            token.ThrowIfCancellationRequested();if(entered)throw new InvalidOperationException("Global input transforms are single-use, including failed prefixes.");
            ValidateSeed(Cleanup,token);if(!ReferenceEquals(Seed.CurrentTransforms,before))throw new InvalidOperationException("Actual transform vector changed before entry.");entered=true;
            try
            {
                Record("entered");Observe(LegacyGlobalInputTransformStage.Entered);
                sourceExport=Seed.InputBinding.ExportIdentity(token);
                if(!ReferenceEquals(sourceExport.Binding,Seed.InputBinding)||sourceExport.OriginalSelectedInput is null||sourceExport.Geometry.NativeNonidentityTransformCount!=0)
                    throw new NotSupportedException("Actual original-selected full-current identity-excluded export required; nonidentity codec is not implemented.");
                Record("exported");Observe(LegacyGlobalInputTransformStage.Exported);
                ValidateSeed(Cleanup,token);
                if(!ReferenceEquals(Seed.CurrentTransforms,before))throw new InvalidOperationException("Actual initial vector changed before retained seed mutation.");
                var swap=Seed.SwapTransforms(prepared?.Prefix??LegacyDocumentTransformVector.Fresh(),prepared?.Succeeded??true);
                output=Seed.CurrentTransforms;swapped=true;exportSucceeded=swap.ExportSucceeded;
                if(!ReferenceEquals(swap.Released,before))throw new InvalidOperationException("Actual old seed vector was not released by the swap.");
                Record("swapped");Observe(LegacyGlobalInputTransformStage.Swapped);
                if(!exportSucceeded){faulted=true;Record("export-failed");return this;}
                //DAF32 comes from the selected INPUT children before NN, not
                //from the transform vector count or absence of raw text.
                if(Seed.CopyDafInputRows32().Length!=0)throw new NotSupportedException("Nonempty actual DAF32 matching is outside this bounded branch.");
                inputSpans=true;Record("input-spans");Observe(LegacyGlobalInputTransformStage.InputSpans);
                ValidateSeed(Cleanup,token);Observe(LegacyGlobalInputTransformStage.Complete);ValidateSeed(Cleanup,token);
                if(!ReferenceEquals(Seed.CurrentTransforms,output))throw new InvalidOperationException("Actual seed output vector changed during export.");
                complete=true;Record("complete");return this;
            }
            catch{faulted=true;Record("fault");throw;}
        }
        void Observe(LegacyGlobalInputTransformStage stage){observer?.Invoke(stage);token.ThrowIfCancellationRequested();}
    }
    public void ValidateCurrent(CancellationToken token=default)
    {lock(gate){token.ThrowIfCancellationRequested();if(!complete||faulted)throw new InvalidOperationException("Successful actual export and INPUT-span stages required.");ValidateSeed(Cleanup,token);if(!ReferenceEquals(Seed.CurrentTransforms,output))throw new InvalidOperationException("Actual output vector owner changed.");}}
    public byte[] CopyTransformScalarStorage64(){lock(gate)return(output??throw new InvalidOperationException("No exported vector.")).CopyScalars();}
    public LegacyGlobalInputTransformPrefix[] CopyPrefixes(){lock(gate)return prefixes.ToArray();}
    internal bool ReleasedInitialOwner=>ReferenceEquals(before,Seed.InitialTransforms);
    private void Record(string stage)=>prefixes.Add(new(stage,swapped,exportSucceeded,inputSpans,output?.Count??0));
}
