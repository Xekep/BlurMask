[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("LegacyEnglishOriginalInput.ActualEntry")]
namespace InkRecognition;

//SOURCE REVIEW ONLY: opt-in original input ownership; no default routing change.
//The native outer SDK field policy is not inferred from this library field name.
public sealed partial class LegacyEnglish119ParentSubmission : IDisposable
{
    private LegacyEnglishOriginalInputContext? originalInputContext;

    /// <summary>Construct and import the actual original/selected input before
    /// the existing parent preparation can run learned line classifiers. The
    /// returned parent must be disposed even if no owning graph run is started.
    /// This does not apply a document or enable whole-English admission.</summary>
    public static LegacyEnglish119ParentSubmission PrepareOriginalSelected(
        LegacyFirstAcoustic119WireModel model,IReadOnlyList<InkStroke> raw,float resolution,
        ReadOnlySpan<byte> spatial30,ReadOnlySpan<byte> physical50,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(model);ArgumentNullException.ThrowIfNull(raw);
        //Snapshot first bounds1..256 before its metadata arrays; it checks the
        //aggregate4096/finite points before allocating the copied point arrays.
        //No line model, FIRST model or graph owner is entered until input import.
        var preparation=LegacyEnglishOriginalInputBudget.PrechargeCompleteRawDomain(token);
        var input=LegacyEnglishOriginalInputContext.Create(Snapshot(raw,token),preparation,token);
        try
        {
            var parent=Prepare(model,input.Document.Strokes,resolution,spatial30,physical50,token);
            input.Attach(parent,token);parent.originalInputContext=input;return parent;
        }
        catch{input.Dispose();throw;}
    }
    internal LegacyEnglishOriginalInputContext? OriginalInputContext=>originalInputContext;
    internal void ReleaseOriginalInputContext()=>originalInputContext?.DisposeAfterPublication();
    public void Dispose()=>originalInputContext?.Dispose();
    internal void RequireOriginalInputPoints(LegacyOwnedRawInkDocument document,CancellationToken token)
    {
        if(raw.Length!=document.ObjectCount)throw new InvalidDataException("Original parent object count changed.");
        for(int i=0;i<raw.Length;i++)
        {
            token.ThrowIfCancellationRequested();var a=raw[i].Points;var b=document.Strokes[i].Points;
            if(a.Count!=b.Count)throw new InvalidDataException("Original parent point count changed.");
            for(int j=0;j<a.Count;j++)
            {
                token.ThrowIfCancellationRequested();
                if(BitConverter.SingleToInt32Bits(a[j].X)!=BitConverter.SingleToInt32Bits(b[j].X)||
                    BitConverter.SingleToInt32Bits(a[j].Y)!=BitConverter.SingleToInt32Bits(b[j].Y))
                    throw new InvalidDataException("Original parent point bits/order changed.");
            }
        }
    }
}

//No public constructor, caller snapshot, field ID, C20 state or lease injection.
//Only PrepareOriginalSelected creates this cohort, before Parent.Prepare.
internal sealed class LegacyEnglishOriginalInputContext : IDisposable
{
    internal const string FieldId="english-field"; //library application policy
    private readonly int thread=Environment.CurrentManagedThreadId;
    private readonly LegacyEnglishOriginalInputBudget.Reservation preparation;
    internal readonly LegacyOwnedRawInkDocument Document;
    internal readonly LegacyFreshRawInkCurrentView View;
    internal readonly LegacyLibraryOriginalTextFieldBootstrap Original;
    internal readonly LegacyLibraryOriginalTextFieldBootstrapState OriginalState;
    internal readonly LegacyLibraryOriginalFieldSelection Input;
    internal readonly LegacyLibraryOriginalSelectedFieldContext Selected;
    internal readonly LegacyLibraryOriginalSelectedFieldState SelectedState;
    internal readonly LegacyLibrarySelectedInputBinding Binding;
    private readonly LegacyLibraryPreparedDomNames preparedDomNames;
    private readonly LegacyOwnedRawInkIdentityExport verifiedOriginalGeometry;
    private readonly LegacyLibraryTextProcessingContext processing;
    private readonly LegacyLibraryTextInputRequest request;
    private readonly LegacyLibraryTextProcessingLease lease;
    private LegacyEnglish119ParentSubmission? parent;
    private bool publication,disposed,identifierClaimed;
    internal int Points{get;}
    private LegacyEnglishOriginalInputContext(InkStroke[] raw,LegacyEnglishOriginalInputBudget.Reservation preparation,CancellationToken token)
    {
        preparation.Require();this.preparation=preparation;
        Document=LegacyOwnedRawInkDocument.Create(raw,token);
        Points=Document.Strokes.Sum(s=>s.Points.Count);
        View=LegacyFreshRawInkCurrentView.Prepare(Document,token);
        Original=LegacyLibraryOriginalTextFieldBootstrap.Prepare(View,[FieldId],token);
        OriginalState=Original.Run(token);
        if(!OriginalState.Succeeded||OriginalState.IsFaulted||OriginalState.Inputs.Count!=1)
            throw new InvalidOperationException("Original input bootstrap failed.");
        Input=OriginalState.Inputs[0];
        Selected=LegacyLibraryOriginalSelectedFieldContext.PrepareRetainingActualGeometry(Original,Input,out verifiedOriginalGeometry,token);
        SelectedState=Selected.Run(token);
        if(!SelectedState.Succeeded||SelectedState.IsFaulted||SelectedState.Registry.Count!=1)
            throw new InvalidOperationException("Selected original input clone failed.");
        processing=LegacyLibraryTextProcessingContext.CreateForOriginalSelected(Selected,token);
        Binding=processing.InputBinding;
        try{var prepared=processing.PrepareInputRetainingDomNames(token);request=prepared.Request;preparedDomNames=prepared.Names;lease=processing.RetainForPublication();}
        catch{processing.Dispose();throw;}
    }
    internal static LegacyEnglishOriginalInputContext Create(InkStroke[] raw,LegacyEnglishOriginalInputBudget.Reservation preparation,CancellationToken token)=>new(raw,preparation,token);
    internal void Attach(LegacyEnglish119ParentSubmission actual,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(disposed||parent is not null||thread!=Environment.CurrentManagedThreadId)
            throw new InvalidOperationException("One original input parent on its creating thread required.");
        //Prepare consumed ONLY the private immutable Document.Strokes. Validate
        //the retained parent snapshot bit-for-bit; no ordinal/target substitution.
        actual.RequireOriginalInputPoints(Document,token);
        parent=actual;Require(actual,token);
    }
    internal void Require(LegacyEnglish119ParentSubmission actual,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();preparation.Require();lease.Validate();
        if(disposed||thread!=Environment.CurrentManagedThreadId||!ReferenceEquals(parent,actual)||actual.IsFaulted||
            !ReferenceEquals(View.Document,Document)||!ReferenceEquals(Original.CurrentView,View)||
            !ReferenceEquals(Original.State,OriginalState)||!OriginalState.Succeeded||OriginalState.IsFaulted||
            !ReferenceEquals(Selected.Original,Original)||!ReferenceEquals(Selected.OriginalInput,Input)||
            !ReferenceEquals(Selected.State,SelectedState)||!SelectedState.Succeeded||SelectedState.IsFaulted||
            !ReferenceEquals(Binding.OriginalSelected,Selected)||!ReferenceEquals(processing.InputBinding.OriginalSelected,Selected)||
            !ReferenceEquals(processing.Document,Document)||processing.PreparedState18!=1||
            !ReferenceEquals(processing.PreparedRequest,request)||!request.Succeeded||
            !ReferenceEquals(processing.Result.BoundInputBinding,processing.InputBinding))
            throw new InvalidOperationException("Same live pre-NN original-selected/imported2DA cohort required.");
    }
    internal LegacyLibraryTextResultIdentifier InitializeGraphPublicationIdentifier(LegacyEnglish119ParentSubmission actual,CancellationToken token)
    {
        Require(actual,token);
        if(!publication||identifierClaimed||processing.Result.PublicationSeed is not null)
            throw new InvalidOperationException("One fresh English publication identifier required.");
        identifierClaimed=true;
        var previous=processing.Result.CopyIdentifier38();
        //01C010's actual constructor zeroed38/40; no accepted/adopted result
        //can enter this fresh private context. UUID version bits prove that
        //NextDistinct needs exactly one generation against this actual value.
        if(previous.Length!=16||previous.Any(b=>b!=0))throw new NotSupportedException("Actual fresh constructor identifier required.");
        var value=LegacyLibraryUuidGenerator.Create().NextDistinct(previous,token);
        processing.Result.CaptureEnglishIdentifier(this,actual,value,token);return value;
    }
    internal (LegacyLibraryTextInputRequest Request,LegacyLibraryTextDomArena Reader,LegacyDomNode Field,LegacyLibraryPreparedDomNames Names) BorrowFreshDomTarget(
        LegacyEnglish119ParentSubmission actual,CancellationToken token)
    {
        Require(actual,token);
        if(!publication||!identifierClaimed)throw new InvalidOperationException("Live actual fresh result identifier required.");
        var target=request.BorrowEnglishDomTarget(this,actual,token);
        return(target.Request,target.Reader,target.Field,preparedDomNames);
    }
    internal void RequireProcessingResult(LegacyLibraryTextProcessingResult result,LegacyEnglish119ParentSubmission actual,CancellationToken token)
    {Require(actual,token);if(!publication||!identifierClaimed||!ReferenceEquals(processing.Result,result))throw new InvalidOperationException("Exact live English processing result required.");}
    //This is the actual export produced before Selected.Run, not a supplied
    //identity flag. CurrentView/C49/ranges are immutable; exact selected C22
    //cloning and both retained selection owners are rechecked before reuse.
    internal LegacyOwnedRawInkIdentityExport BorrowFreshTransformGeometry(LegacyEnglish119ParentSubmission actual,
        LegacyLibrarySelectedInputBinding actualBinding,out long sourceTag,out int oldTextCount,CancellationToken token)
    {
        Require(actual,token);
        if(!publication||!ReferenceEquals(actualBinding,Binding)||!ReferenceEquals(verifiedOriginalGeometry.Owner,View))
            throw new InvalidOperationException("Exact live retained original geometry required.");
        var entry=Binding.OriginalSelectedEntry??throw new InvalidOperationException("Actual selected INPUT required.");
        if(!ReferenceEquals(entry.Owner,Selected)||!ReferenceEquals(entry.OriginalEntry,Input)||
            !ReferenceEquals(entry.RetainedSelection.Owner,Selected)||!ReferenceEquals(Input.RetainedSelection.Owner,Original))
            throw new InvalidOperationException("Exact cloned selection lineage required.");
        var ranges=entry.CopyRanges();var verified=verifiedOriginalGeometry.CopyRanges();
        if(ranges.Length!=1||verified.Length!=1||!ranges.AsSpan().SequenceEqual(verified))
            throw new NotSupportedException("The actual preverified full C22 changed.");
        sourceTag=processing.Result.SourceTag30;oldTextCount=processing.Result.TextCountD8;
        if(sourceTag!=entry.RegistryId||oldTextCount!=0)
            throw new NotSupportedException("Actual fresh imported source tag and empty old result required.");
        token.ThrowIfCancellationRequested();return verifiedOriginalGeometry;
    }
    internal void BeginPublication(LegacyEnglish119ParentSubmission actual,CancellationToken token)
    {Require(actual,token);publication=true;}
    internal LegacyNativeUtf8StringPool BorrowStrings(LegacyEnglish119ParentSubmission actual,CancellationToken token)
    {Require(actual,token);if(!publication)throw new InvalidOperationException("Actual publication has not started.");return processing.Strings;}
    internal void DisposeAfterPublication(){if(disposed)return;publication=false;Dispose();}
    public void Dispose()
    {
        if(disposed)return;
        if(thread!=Environment.CurrentManagedThreadId||publication)
            throw new InvalidOperationException("Active original input publication cannot be externally disposed.");
        disposed=true;lease.Dispose();processing.Dispose();
    }
}

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    internal void RequireOriginalInputRoute(GraphLanguageScope stage,LegacyEnglish119ParentSubmission parent,CancellationToken token)
    {
        RequireGraphPublication(stage,token);
        if(!graphOriginalSelectedRequested||!ReferenceEquals(stage.Lease.Root.Knowledge.Owner.Owner.Owner.Parent,parent))
            throw new NotSupportedException("Pre-NN original input requires the same original-selected graph route.");
    }
}

internal sealed partial class LegacyEnglishCasePrimaryCompletion
{
    //Knowledge is retained before Run; cleanup does not depend on creating a
    //later graph scope. Thus initial-primary failure cannot strand the lease.
    internal void RequireOriginalInputContext(CancellationToken token)
    {
        var parent=knowledge.Owner.Owner.Owner.Parent;
        parent.OriginalInputContext?.Require(parent,token);
    }
    // This new explicit route may NOT replace a missing pre-NN context with a late bootstrap.
    internal void RequirePreNeuralOriginalInputContext(CancellationToken token)
    {
        var parent=knowledge.Owner.Owner.Owner.Parent;
        var input=parent.OriginalInputContext??throw new InvalidOperationException("Authentic pre-NN original-selected input required.");
        input.Require(parent,token);
    }
    internal void ReleaseOriginalInputContext()=>knowledge.Owner.Owner.Owner.Parent.ReleaseOriginalInputContext();
}
