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
    private LegacyLibraryTextPublicationDocumentSnapshot snapshot;
    public LegacyOwnedRawInkDocument Document=>scope.Document;
    public bool IsFaulted {get{lock(gate)return faulted;}}
    public LegacyLibraryTextPublicationDocumentSnapshot Snapshot {get{lock(gate)return snapshot;}}

    private LegacyLibraryTextPublicationDocument(LegacyLibraryTextPublicationScope scope,
        LegacyOwnedMainWordSourceChoicesResult words)
    {this.scope=scope;this.words=words;snapshot=Capture();}

    public static LegacyLibraryTextPublicationDocument Prepare(LegacyLibraryTextPublicationScope actualScope,
        LegacyOwnedMainWordSourceChoicesResult actualWords,CancellationToken token=default)
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
                var transforms=LegacyFreshMainDocumentInputTransforms.Apply(cleanup,token);Record("input-transforms");
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
    internal LegacyOwnedMainDetachedTextDom ActualDom=>dom??throw new InvalidOperationException("DOM construction has not entered.");
    internal LegacyLibraryTextDomTransportRow? ActualTransportRow=>row;
    internal LegacyLibraryTextDomTarget? ActualTarget=>target;
    internal LegacyLibraryTextDomTransfer? ActualTransfer=>transfer;
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
