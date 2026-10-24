[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("LegacyEnglishEarlySelected.Probe")]
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("LegacyEnglishEarlySelected.ActualEntry")]
namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    private bool graphOriginalSelectedRequested;
    private GraphEarlySelectedInputData? graphEarlySelectedInputData;
    internal sealed record GraphEarlySelectedInputData(string FieldId,int OriginalObjects,int OriginalPoints,
        long OriginalInputId,long OriginalNext,long SelectedNextBeforeRegistration,long PendingNextAfterText,
        int OriginalDomNodes,int SelectedDomNodes,int SelectedRegistryCount,bool DocumentApplied,string Scope);
    internal sealed record GraphEarlySelectedInputResult(GraphPostTextCoverageResult Publication,GraphEarlySelectedInputData Bootstrap);

    //New explicit route. The literal field id is a library-created application
    //field policy, NOT a lexicon/native resource value or recognized text.
    internal GraphEarlySelectedInputResult RunGraphFirstOriginalSelectedPostText(LegacyNativeUnicodeNormalizationData normalization,
        CancellationToken token=default,Action<GraphMainConfidenceObservation>? observer=null,bool captureSourceInputs=false)
    {
        primary.RequireGraphLoop(this,token);ArgumentNullException.ThrowIfNull(normalization);
        if(graphOriginalSelectedRequested||graphWordPublicationRequested||graphCharactersRequested)
            throw new InvalidOperationException("Original-selected bootstrap must precede every CHAR/WORD registration.");
        graphOriginalSelectedRequested=true;
        var publication=RunGraphFirstPostTextCoverage(normalization,token,observer,captureSourceInputs);
        return new(publication,graphEarlySelectedInputData??throw new InvalidOperationException("No live original-selected bootstrap receipt."));
    }

    internal sealed partial class GraphWordPublicationContext
    {
        private GraphEarlySelectedInputReceipt? earlySelected;
        private LegacyOwnedFreshTextSourceContext PrepareEarlyOriginalSelected(CancellationToken token)
        {
            owner.RequireGraphPublication(Stage,token);
            if(!owner.graphOriginalSelectedRequested||earlySelected is not null||wordNode is not null||next!=0||current!=-1)
                throw new InvalidOperationException("Private constructor-time original-selected bootstrap required.");
            //This existing owned document factory already bounds N<=256,P<=4096.
            //Charge the scalar traversal before reading all original counts.
            int n=Document.ObjectCount;Reserve(128,256,checked(4096+n*128L));
            int points=0;for(int i=0;i<n;i++){token.ThrowIfCancellationRequested();points=checked(points+Document.Strokes[i].Points.Count);}
            if(n is<1 or>256||points is<1 or>4096)throw new NotSupportedException("Existing complete immutable raw-document domain required.");
            //SOURCE envelope for one library-created field: view XY, original
            //PAGE/INPUT/C51, selected raw+DOM clone, identity exports/snapshots,
            //binding and pending owner. No ledger cap is raised or refunded.
            Reserve(checked(70656+n*1216L+points*32L),checked(141312+n*2432L+points*96L),
                checked(352256+n*1344L+points*128L+4L*n*n+TotalReferences*1024L));
            earlySelected=GraphEarlySelectedInputReceipt.Issue(this,points,token);
            return earlySelected.Pending;
        }
        //Constructor-only identity predicate, before this parent is installed.
        private bool MatchesGraphSelectionBindingAtPreparation()
            =>!owner.graphOriginalSelectedRequested?Selections.InputBinding is null:
                earlySelected is not null&&earlySelected.Matches(Selections);
        internal bool MatchesGraphSelectionBinding(LegacyOwnedFreshTextSourceContext actual,CancellationToken token)
        {
            if(!owner.graphOriginalSelectedRequested)return ReferenceEquals(actual,Selections)&&
                ReferenceEquals(actual.Document,Document)&&actual.InputBinding is null&&earlySelected is null;
            token.ThrowIfCancellationRequested();
            //No repeated whole275 scan here. Mutation callers establish their
            //own phase; authenticate this installed parent/source cohort only.
            if(poisoned||thread!=Environment.CurrentManagedThreadId||!owner.graphFinalCompactBusy||
                !ReferenceEquals(owner.graphLanguageScope,Stage)||!ReferenceEquals(owner.graphWordPublicationContext,this)||
                !ReferenceEquals(actual,Selections)||!ReferenceEquals(actual.Document,Document))return false;
            return earlySelected is not null&&earlySelected.RequireActual(this,actual,token);
        }
        internal GraphEarlySelectedInputData CopyEarlySelectedInputData(CancellationToken token)
        {
            Reserve(1024,2048,checked(4096+Count*512L)); //before the ONE final span validation
            Require(token);
            if(!postTextFinished||!finished||textPhase!=4||earlySelected is null||!MatchesGraphSelectionBinding(Selections,token))
                throw new InvalidOperationException("Same live completed selected post-TEXT prefix required.");
            return earlySelected.CopyData(Selections.NextSelectionId);
        }
    }

    //All capabilities are private. Output is scalars only; no bootstrap,
    //selected owner, registry entry, mutable DOM or range issuer escapes.
    private sealed class GraphEarlySelectedInputReceipt
    {
        private const string FieldId="english-field";
        private readonly GraphWordPublicationContext parent;
        private readonly int thread=Environment.CurrentManagedThreadId;
        private readonly int points;
        private readonly LegacyFreshRawInkCurrentView view;
        private readonly LegacyLibraryOriginalTextFieldBootstrap original;
        private readonly LegacyLibraryOriginalTextFieldBootstrapState originalState;
        private readonly LegacyLibraryOriginalFieldSelection input;
        private readonly LegacyLibraryOriginalSelectedFieldContext selected;
        private readonly LegacyLibraryOriginalSelectedFieldState selectedState;
        private readonly LegacyLibrarySelectedInputBinding binding;
        internal readonly LegacyOwnedFreshTextSourceContext Pending;
        private GraphEarlySelectedInputReceipt(GraphWordPublicationContext parent,int points,CancellationToken token)
        {
            this.parent=parent;this.points=points;
            view=LegacyFreshRawInkCurrentView.Prepare(parent.Document,token);
            original=LegacyLibraryOriginalTextFieldBootstrap.Prepare(view,[FieldId],token);
            originalState=original.Run(token);
            if(!originalState.Succeeded||originalState.IsFaulted||originalState.Inputs.Count!=1)
                throw new InvalidOperationException("Actual original PAGE/INPUT bootstrap failed; no replacement permitted.");
            input=originalState.Inputs[0];
            selected=LegacyLibraryOriginalSelectedFieldContext.Prepare(original,input,token);
            selectedState=selected.Run(token);
            if(!selectedState.Succeeded||selectedState.IsFaulted||selectedState.Registry.Count!=1)
                throw new InvalidOperationException("Actual selected raw/DOM clone failed; no replacement permitted.");
            binding=LegacyLibrarySelectedInputBinding.FromOriginal(selected);
            Pending=LegacyOwnedFreshTextSourceContext.PrepareSelectedBinding(binding);
            if(!Matches(Pending)||Pending.NextSelectionId!=selectedState.NextRegistryId||Pending.CopyPendingSelections().Count!=0)
                throw new InvalidOperationException("Original selected Next must precede all pending registrations.");
        }
        internal static GraphEarlySelectedInputReceipt Issue(GraphWordPublicationContext parent,int points,CancellationToken token)
            =>new(parent,points,token);
        internal bool Matches(LegacyOwnedFreshTextSourceContext actual)
            =>ReferenceEquals(actual,Pending)&&ReferenceEquals(actual.Document,parent.Document)&&ReferenceEquals(actual.InputBinding,binding)&&
                ReferenceEquals(binding.OriginalSelected,selected)&&ReferenceEquals(view.Document,parent.Document)&&
                ReferenceEquals(original.CurrentView,view)&&ReferenceEquals(selected.Original,original)&&ReferenceEquals(selected.OriginalInput,input)&&
                ReferenceEquals(original.State,originalState)&&ReferenceEquals(selected.State,selectedState)&&
                originalState.Succeeded&&!originalState.IsFaulted&&selectedState.Succeeded&&!selectedState.IsFaulted&&
                selectedState.NextRegistryId==originalState.NextRegistryId&&selectedState.Registry.Count==1&&
                ReferenceEquals(selectedState.Registry[0].OriginalEntry,input)&&ReferenceEquals(selectedState.Registry[0].Owner,selected);
        internal bool RequireActual(GraphWordPublicationContext actualParent,LegacyOwnedFreshTextSourceContext actual,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return thread==Environment.CurrentManagedThreadId&&ReferenceEquals(actualParent,parent)&&Matches(actual)&&
                actual.NextSelectionId>=selectedState.NextRegistryId&&!actual.Failed;
        }
        internal GraphEarlySelectedInputData CopyData(long next)
            =>new(FieldId,parent.Document.ObjectCount,points,input.RegistryId,originalState.NextRegistryId,selectedState.NextRegistryId,next,
                originalState.Nodes.Count,selectedState.Nodes.Count,selectedState.Registry.Count,false,
                "EARLY_SAME_DOCUMENT_ORIGINAL_PAGE_INPUT_SELECTED_C20_BEFORE_CHAR_WORD_TEXT_NO_DOCUMENT_APPLY");
    }

    internal sealed partial class GraphCharacterContext
    {
        internal bool MatchesGraphSelectionBinding(LegacyOwnedFreshTextSourceContext actual,CancellationToken token)
        {
            if(!owner.graphOriginalSelectedRequested)return ReferenceEquals(actual,Selections)&&actual.InputBinding is null;
            var parent=owner.graphWordPublicationContext;
            return parent is not null&&ReferenceEquals(parent.Document,Document)&&ReferenceEquals(parent.Selections,Selections)&&
                ReferenceEquals(actual,Selections)&&parent.MatchesGraphSelectionBinding(actual,token);
        }
    }
}
