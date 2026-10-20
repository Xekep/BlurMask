using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyTextPublicationDocument.Probe")]
namespace InkRecognition;

/// <summary>One checked document/DOM/transport invocation over the actual
/// adopted WORD result. Borrows its live publication scope: closing that
/// scope invalidates all operations here. Dispose closes only this owner's
/// transport leases. Immutable Snapshot values remain available after close.
/// This is library-root transport/application, not native C20 acceptance.</summary>
public sealed class LegacyLibraryTextPublicationDocument : IDisposable
{
    private sealed class Claim {internal bool Entered;}
    private static readonly ConditionalWeakTable<LegacyOwnedMainWordSourceChoicesResult,Claim> claims=new();
    private readonly object gate=new();
    private readonly LegacyLibraryTextPublicationScope scope;
    private readonly LegacyOwnedMainWordSourceChoicesResult words;
    private readonly List<string> trace=["prepared"];
    private bool disposed,entered,faulted;
    private LegacyFreshMainDocumentCounterHandoff? handoff;
    private LegacyFreshMainSourceRangeUtf8? sources;
    private LegacyOwnedMainDetachedTextDom? dom;
    private LegacyLibraryTextDomTarget? target;
    private LegacyLibraryTextDomEditResult? edit;
    private LegacyLibraryTextDomTransfer? transfer;
    private LegacyLibraryTextDomTransportCache? cache;
    private LegacyLibraryTextDomTransportRow? row;
    private LegacyLibraryTextDomApplyResult? applied;
    private LegacyLibrarySelectedInputRegistryReplay? registryReplay;
    private LegacyLibraryOriginalInputPatchCompatibility? originalCompatibility;
    private LegacyLibraryTextDomPatchCompatibility? domCompatibility;
    private LegacyLibraryOriginalSourceApplication? originalSourceApplication;
    private LegacyLibraryOriginalFieldDomPublication? originalFieldDom;
    public LegacyLibraryOriginalFieldDomPublication? OriginalFieldDomPublication {get{lock(gate)return originalFieldDom;}}
    private LegacyLibraryTextPublicationDocumentSnapshot snapshot;
    public LegacyOwnedRawInkDocument Document=>scope.Document;
    public bool IsFaulted {get{lock(gate)return faulted;}}
    /// <summary>The one library selected-registry replay retained by this
    /// receipt. Its state is separate from historical INPUT and DOM apply.</summary>
    public LegacyLibrarySelectedInputRegistryReplay? SelectedRegistryReplay {get{lock(gate)return registryReplay;}}
    public LegacyLibraryOriginalInputPatchCompatibility? OriginalPatchCompatibility {get{lock(gate)return originalCompatibility;}}
    public LegacyLibraryTextDomPatchCompatibility? DomPatchCompatibility {get{lock(gate)return domCompatibility;}}
    public LegacyLibraryOriginalSourceApplication? OriginalSourceApplication {get{lock(gate)return originalSourceApplication;}}
    public LegacyLibraryTextPublicationDocumentSnapshot Snapshot {get{lock(gate)return snapshot;}}

    private LegacyLibraryTextPublicationDocument(LegacyLibraryTextPublicationScope scope,
        LegacyOwnedMainWordSourceChoicesResult words)
    {this.scope=scope;this.words=words;snapshot=Capture();}

    public static LegacyLibraryTextPublicationDocument Prepare(LegacyLibraryTextPublicationScope actualScope,
        LegacyOwnedMainWordSourceChoicesResult actualWords,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualScope);
        if(actualScope.Context.OriginalSelectedInput is not null)
            throw new NotSupportedException("Use the named original-selected document route.");
        return PrepareCore(actualScope,actualWords,token);
    }
    public static LegacyLibraryTextPublicationDocument PrepareOriginalSelected(LegacyLibraryTextPublicationScope actualScope,
        LegacyOwnedMainWordSourceChoicesResult actualWords,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualScope);
        if(actualScope.Context.OriginalSelectedInput is null)
            throw new ArgumentException("Actual original-bootstrap selected scope required.");
        return PrepareCore(actualScope,actualWords,token);
    }
    private static LegacyLibraryTextPublicationDocument PrepareCore(LegacyLibraryTextPublicationScope actualScope,
        LegacyOwnedMainWordSourceChoicesResult actualWords,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(actualScope);ArgumentNullException.ThrowIfNull(actualWords);
        actualScope.Validate();token.ThrowIfCancellationRequested();
        actualScope.Seed.Validate(actualWords.Owner,token);
        if(!ReferenceEquals(actualWords.Owner.DocumentSeed,actualScope.Seed)||
            !ReferenceEquals(actualWords.Owner.Document,actualScope.Document)||
            !ReferenceEquals(actualScope.Seed.ProcessingResult,actualScope.Context.Result))
            throw new ArgumentException("Exact adopted result/Seed/document/shared WORD owner required.");
        var claim=claims.GetValue(actualWords,_=>new());
        lock(claim)
        {
            token.ThrowIfCancellationRequested();
            if(claim.Entered)throw new InvalidOperationException("This actual WORD publication already owns a document invocation.");
            var owner=new LegacyLibraryTextPublicationDocument(actualScope,actualWords);claim.Entered=true;return owner;
        }
    }

    /// <summary>Native-order source/selection/postprocessing, detached text DOM,
    /// V50 edit and actual pending-C4D transfer into a NEW library cache.
    /// The target is not modified until ApplyToLibraryTarget is called.</summary>
    public LegacyLibraryTextPublicationDocumentSnapshot BuildAndTransfer(CancellationToken token=default)
        =>BuildCore(-1,-1,false,token);
    internal LegacyLibraryTextPublicationDocumentSnapshot BuildPrepared(int domFailure=-1,
        int exportFailure=-1,bool transferFailure=false,CancellationToken token=default)
        =>BuildCore(domFailure,exportFailure,transferFailure,token);
    private LegacyLibraryTextPublicationDocumentSnapshot BuildCore(int domFailure,int exportFailure,
        bool transferFailure,CancellationToken token)
    {
        lock(gate)
        {
            Validate(token);
            if(entered)throw new InvalidOperationException("Document processing retains its entered prefix and is single-use.");
            entered=true;
            try
            {
                var lines=LegacyFreshMainSourceLinePublication.PublishFresh(words,token);Record("source-lines");
                var text=LegacyFreshMainDocumentTextSelection.Prepare(lines,token);text.Register(token);Record("text-registered");
                var consistency=LegacyFreshMainDocumentLineConsistency.Apply(text,token);Record("line-consistency");
                var coverage=LegacyFreshMainDocumentSourceCoverage.Prepare(consistency,token);
                var characters=LegacyFreshMainDocumentCharacterCoverage.Prepare(coverage,token);
                var state=LegacyFreshMainDocumentSelectedSources.Prepare(characters,token);Record("coverage");
                var indices=LegacyFreshMainDocumentSourceIndices.Prepare(state,token);indices.Run(token);Record("source-indices");
                var cleanup=LegacyFreshMainDocumentControlCleanup.Prepare(indices,token);cleanup.Run(token);Record("control-cleanup");
                var transforms=scope.Seed.OriginalSelectedInputContext is null?
                    LegacyFreshMainDocumentInputTransforms.Apply(cleanup,token):
                    LegacyFreshMainDocumentInputTransforms.ApplyOriginalSelected(cleanup,token);Record("input-transforms");
                handoff=LegacyFreshMainDocumentCounterHandoff.Apply(transforms,token);Record("counter-handoff");
                sources=LegacyFreshMainSourceRangeUtf8.Prepare(handoff,token);Record("source-ranges");
                target=LegacyLibraryTextDomTarget.CreateFresh(scope.Seed);
                dom=LegacyFreshMainDetachedTextDom.PrepareForTarget(sources,target,token).BuildPrepared(domFailure,token:token);
                if(!dom.Succeeded){faulted=true;Record("dom-refused");return snapshot;}
                Record("dom-built");
                edit=LegacyLibraryTextDomInsertion.Prepare(dom,token).ExportPrepared(exportFailure,token:token);
                if(!edit.Succeeded){faulted=true;Record("export-refused");return snapshot;}
                transfer=LegacyLibraryTextDomTransfer.Prepare(edit,token);
                cache=LegacyLibraryTextDomTransportCache.CreateFresh(target);Record("exported");
                TransferCore(transferFailure,token);return snapshot;
            }
            catch
            {
                // A failed cache reserve retains both actual incoming slots;
                // retrying TransferPending uses that receipt, not a new C4D.
                if(trace[^1]!="exported")faulted=true;
                snapshot=Capture();throw;
            }
        }
    }

    /// <summary>Retry only a failed reserve over the already exported receipt.</summary>
    public LegacyLibraryTextPublicationDocumentSnapshot TransferPending(CancellationToken token=default)
    {lock(gate){Validate(token);TransferCore(false,token);return snapshot;}}
    private void TransferCore(bool fail,CancellationToken token)
    {
        if(transfer is null||cache is null||row is not null||trace[^1]!="exported")
            throw new InvalidOperationException("An unconsumed actual exported transfer is required.");
        row=transfer.TransferPrepared(cache,fail,token);Record("transported");
    }

    /// <summary>Apply the real cloned BE2 edit to this private library root.
    /// It does not execute the pending C4D commands or native history receiver.</summary>
    public LegacyLibraryTextPublicationDocumentSnapshot ApplyToLibraryTarget(CancellationToken token=default)
        =>ApplyCore(-1,-1,token);
    internal LegacyLibraryTextPublicationDocumentSnapshot ApplyPrepared(int failure=-1,int cancellation=-1,
        CancellationToken token=default)=>ApplyCore(failure,cancellation,token);
    private LegacyLibraryTextPublicationDocumentSnapshot ApplyCore(int failure,int cancellation,CancellationToken token)
    {
        lock(gate)
        {
            Validate(token);
            if(row is null||edit is null||target is null||applied is not null)
                throw new InvalidOperationException("One successfully transported unapplied edit required.");
            if(registryReplay is not null&&!registryReplay.State.Succeeded)
                throw new InvalidOperationException("A claimed selected-registry replay must succeed before this library DOM apply.");
            originalSourceApplication?.ValidateForLibraryApply(token);
            if(originalFieldDom is not null&&!originalFieldDom.State.Succeeded)throw new InvalidOperationException("Publish original FIELD DOM before optional kind4 library target apply.");
            using var domOperation=EnterDomCompatibility(token);
            try{applied=target.ApplyPrepared(edit,failure,cancellation,token);Record("library-target-applied");return snapshot;}
            catch{faulted=true;snapshot=Capture();throw;}
        }
    }

    private void Validate(CancellationToken token)
    {ObjectDisposedException.ThrowIf(disposed,this);scope.Validate();token.ThrowIfCancellationRequested();
     if(faulted)throw new InvalidOperationException("Faulted document invocation.");scope.Seed.Validate(words.Owner,token);}
    private void Record(string step){trace.Add(step);snapshot=Capture();}
    private LegacyLibraryTextPublicationDocumentSnapshot Capture()=>new(trace.ToArray(),faulted,
        dom?.Succeeded==true?dom.Root:null,target?.Root,edit?.Rows.Select(r=>r.CopyScalarStorage40()).ToArray()??[],
        row?.CopyScalarStorage40(),applied is not null,scope.Context.Result.CopyIdentifier38(),
        words.Owner.SelectionContext.CopyPendingCommands().Select(c=>new LegacyLibraryTextPublicationCommandSnapshot(c)).ToArray());
    internal LegacyFreshMainSourceRangeUtf8 ActualSources=>sources??throw new InvalidOperationException("Source formatting has not completed.");
    internal LegacyLibraryOriginalSourceApplication ClaimOriginalSourceApplication(LegacyLibraryOriginalSourceHistory history,
        LegacyLibraryOriginalInputPatchCompatibility compatibility,CancellationToken token)
    {
        lock(gate)
        {
            Validate(token);ValidateRegistrySource(token);
            if(originalSourceApplication is not null||!ReferenceEquals(compatibility,originalCompatibility)||
                !ReferenceEquals(compatibility.Replay,registryReplay)||registryReplay.HasEntered||!compatibility.State.IsReady)
                throw new InvalidOperationException("One actual ready original preflight before registry entry required.");
            using var operation=EnterDomCompatibility(token);
            var value=history.ClaimApplication(this,compatibility,token);originalSourceApplication=value;return value;
        }
    }
    internal T WithOriginalSourceApplication<T>(LegacyLibraryOriginalSourceApplication actual,Func<T> action,CancellationToken token)
    {
        lock(gate)
        {
            Validate(token);ValidateRegistrySource(token);
            if(!ReferenceEquals(actual,originalSourceApplication)||!ReferenceEquals(actual.Compatibility,originalCompatibility)||
                !actual.Compatibility.State.IsReady)throw new InvalidOperationException("Exact claimed original source application required.");
            using var operation=EnterDomCompatibility(token);return action();
        }
    }
    internal LegacyLibraryOriginalFieldDomPublication ClaimOriginalFieldDom(LegacyLibraryOriginalFieldDom owner,
        LegacyLibraryOriginalSourceApplication application,CancellationToken token)
    {
        lock(gate)
        {
            Validate(token);ValidateRegistrySource(token);
            if(originalFieldDom is not null||!ReferenceEquals(originalSourceApplication,application)||application.State.HasEntered)
                throw new InvalidOperationException("One FIELD edit must be prepared before actual original source application.");
            using var operation=EnterDomCompatibility(token);
            var value=application.Owner.WithApplication(application,()=>owner.ClaimPublication(this,application,token),token);
            originalFieldDom=value;return value;
        }
    }
    internal T WithOriginalFieldDom<T>(LegacyLibraryOriginalFieldDomPublication actual,Func<T> action,CancellationToken token)
    {
        lock(gate)
        {
            Validate(token);ValidateRegistrySource(token);
            if(!ReferenceEquals(originalFieldDom,actual)||!ReferenceEquals(originalSourceApplication,actual.SourceApplication))
                throw new ArgumentException("Exact retained original FIELD/document/source receipt required.");
            using var operation=EnterDomCompatibility(token);
            return actual.SourceApplication.Owner.WithApplication(actual.SourceApplication,action,token);
        }
    }
    internal LegacyOwnedMainDetachedTextDom ActualDom=>dom??throw new InvalidOperationException("DOM construction has not entered.");
    internal LegacyLibraryTextDomTransportRow? ActualTransportRow=>row;
    internal LegacyLibraryTextDomTarget? ActualTarget=>target;
    internal LegacyLibraryTextDomTransfer? ActualTransfer=>transfer;
    internal LegacyLibrarySelectedInputRegistryReplay ClaimRegistryReplay(CancellationToken token)
    {
        lock(gate)
        {
            Validate(token);
            if(registryReplay is not null)throw new InvalidOperationException("This receipt already owns a selected-registry replay.");
            ValidateRegistrySource(token);
            var replay=new LegacyLibrarySelectedInputRegistryReplay(this,scope,row!,handoff!,token);
            registryReplay=replay;return replay;
        }
    }
    internal T WithRegistryReplay<T>(LegacyLibrarySelectedInputRegistryReplay actualReplay,Func<T> action,CancellationToken token)
    {
        lock(gate)
        {
            Validate(token);ValidateRegistrySource(token);
            if(!ReferenceEquals(actualReplay,registryReplay))throw new ArgumentException("Exact receipt-owned replay required.");
            if(originalCompatibility is not null&&!originalCompatibility.State.IsReady)
                throw new InvalidOperationException("The claimed original-input preflight must finish successfully and report compatibility.");
            using var domOperation=EnterDomCompatibility(token);
            return action();
        }
    }
    internal LegacyLibraryOriginalInputPatchCompatibility ClaimOriginalCompatibility(
        LegacyLibrarySelectedInputRegistryReplay actualReplay,CancellationToken token)
    {
        lock(gate)
        {
            Validate(token);ValidateRegistrySource(token);
            if(!ReferenceEquals(actualReplay,registryReplay))throw new ArgumentException("Exact receipt-owned replay required.");
            if(originalCompatibility is not null||actualReplay.HasEntered)
                throw new InvalidOperationException("One original-input preflight must precede registry replay entry.");
            var owner=new LegacyLibraryOriginalInputPatchCompatibility(actualReplay,token);
            originalCompatibility=owner;return owner;
        }
    }
    internal T WithOriginalCompatibility<T>(LegacyLibraryOriginalInputPatchCompatibility actual,Func<T> action,CancellationToken token)
    {
        lock(gate)
        {
            Validate(token);ValidateRegistrySource(token);
            if(!ReferenceEquals(actual,originalCompatibility)||!ReferenceEquals(actual.Replay,registryReplay)||actual.Replay.HasEntered)
                throw new InvalidOperationException("Exact unconsumed original-input preflight required.");
            return action();
        }
    }
    private void ValidateRegistrySource(CancellationToken token)
    {
        if(row is null||row.IsReleased||handoff is null||applied is not null||
            !ReferenceEquals(row.TransactionOwner,handoff.TransactionOwner)||
            !ReferenceEquals(row.TransactionOwner,scope.Seed.PendingSelections)||
            !ReferenceEquals(row.Owner.InputBinding,scope.Seed.InputBinding)||
            !ReferenceEquals(row.DomEdit,edit))
            throw new InvalidOperationException("Actual transferred pending C4D and unapplied same-scope selected INPUT required.");
        LegacyFreshMainDocumentCounterHandoff.Validate(handoff.Transforms,token);
    }
    internal LegacyLibraryTextDomPatchCompatibility ClaimDomCompatibility(CancellationToken token)
    {
        lock(gate)
        {
            Validate(token);ValidateRegistrySource(token);
            if(domCompatibility is not null||registryReplay?.HasEntered==true)
                throw new InvalidOperationException("One DOM preflight must precede any selected-registry replay entry.");
            var service=new LegacyLibraryTextDomPatchCompatibility(this,scope,row!,token);
            domCompatibility=service;return service;
        }
    }
    internal T WithDomCompatibility<T>(LegacyLibraryTextDomPatchCompatibility actual,Func<T> action,CancellationToken token)
    {
        lock(gate)
        {
            Validate(token);ValidateRegistrySource(token);
            if(!ReferenceEquals(actual,domCompatibility)||registryReplay?.HasEntered==true||
                !ReferenceEquals(actual.Edit,edit)||!ReferenceEquals(actual.Target,target)||!ReferenceEquals(actual.TransportRow,row))
                throw new InvalidOperationException("Exact claimed unapplied DOM preflight before registry entry required.");
            return action();
        }
    }
    private IDisposable? EnterDomCompatibility(CancellationToken token)
    {
        if(domCompatibility is not null)
        {
            if(!domCompatibility.State.IsReady)
                throw new InvalidOperationException("The claimed same-edit DOM preflight must work and report compatibility.");
            //Readiness is a historical result. A different explicit target
            //operation cannot authorize replay against that stale epoch.
            return domCompatibility.Target.EnterOperation(domCompatibility.Edit.Epoch,token);
        }
        return null;
    }
    public void Dispose()
    {lock(gate){if(disposed)return;disposed=true;transfer?.Dispose();cache?.Dispose();}}
}

/// <summary>Detached immutable historical snapshot. Native pointer lanes in
/// scalar rows remain zero with their actual owners retained privately.</summary>
public sealed class LegacyLibraryTextPublicationDocumentSnapshot
{
    private readonly byte[][] edits;
    private readonly byte[]? transport;
    private readonly byte[] identifier;
    public IReadOnlyList<string> Steps {get;}
    public bool IsFaulted {get;}
    public bool WasTransferred=>transport is not null;
    public bool WasAppliedToLibraryTarget {get;}
    public LegacyLibraryTextDomNode? WriterRoot {get;}
    public LegacyLibraryTextDomNode? TargetRoot {get;}
    public IReadOnlyList<LegacyLibraryTextPublicationCommandSnapshot> PendingCommands {get;}
    internal LegacyLibraryTextPublicationDocumentSnapshot(string[] steps,bool faulted,LegacyLibraryTextDomNode? writer,
        LegacyLibraryTextDomNode? target,byte[][] edits,byte[]? transport,bool applied,byte[] identifier,
        LegacyLibraryTextPublicationCommandSnapshot[] commands)
    {Steps=Array.AsReadOnly(steps);IsFaulted=faulted;WriterRoot=writer;TargetRoot=target;this.edits=edits;
     this.transport=transport;WasAppliedToLibraryTarget=applied;this.identifier=identifier;PendingCommands=Array.AsReadOnly(commands);}
    public byte[][] CopyEditScalarRows40()=>edits.Select(r=>r.ToArray()).ToArray();
    public byte[]? CopyTransportScalarRow40()=>transport?.ToArray();
    public byte[] CopyIdentifier38()=>identifier.ToArray();
}

public sealed class LegacyLibraryTextPublicationCommandSnapshot
{
    private readonly LegacyNativeInkPositionRange[] ranges;
    private readonly byte[]? counter;
    public byte Operation {get;}
    public int Identity {get;}
    public int NextIdentity {get;}
    public long? SelectionId {get;}
    public string? Name {get;}
    public int? Category {get;}
    internal LegacyLibraryTextPublicationCommandSnapshot(LegacyOwnedFreshTextPendingCommand command)
    {Operation=command.OperationCode;Identity=command.CommandIdentity;NextIdentity=command.NextCommandIdentity;
     if(command.Selection is{} s){SelectionId=s.SelectionId;Name=s.Name;Category=s.Category;ranges=s.CopyRanges();}
     else{counter=command.Counter!.CopyAssignedBody56();ranges=[];}}
    public LegacyNativeInkPositionRange[] CopyRanges()=>ranges.ToArray();
    public byte[]? CopyCounterBody56()=>counter?.ToArray();
}
