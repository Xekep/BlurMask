using System.Runtime.CompilerServices;
using System.Text;

namespace InkRecognition.Math;

internal sealed record Math2Mode0OriginalSelection(object Owner,object Identity,LegacyNativeInkPositionRange[] Ranges);
internal sealed record Math2Mode0OriginalRegistry(long Id,string Name,int Category,LegacyNativeInkPositionRange[] Ranges,object Entry,object C22);
internal sealed record Math2Mode0OriginalInput(Math2Mode0OriginalMathFieldSource Source,object Ink,
    LegacyLibraryOriginalDomNameOwner Names,IReadOnlyList<Math2Mode0MathFieldNode> HistoricalNodes,string FieldId,
    long InputId,long InitialNext,object InputC22,LegacyNativeInkPositionRange[] InputRanges,
    Func<Math2Mode0OriginalRegistry[]> ReadRegistry,Func<long> Next,
    Func<LegacyNativeInkPositionRange[],Math2Mode0OriginalSelection> CloneSelection,
    Func<Math2Mode0TransferCommand,Math2Mode0OriginalSelection,Math2Mode0OriginalRegistry> Register,
    Action<long,object> Delete,Action<CancellationToken> Check);
internal sealed record Math2Mode0OriginalTransferInput(Math2Mode0OutgoingTransfer Owner,
    LegacyLibraryOriginalDomNameOwner Names,IReadOnlyList<Math2Mode0PublishNode> Nodes,int PayloadRoot,
    byte Operation,int[] Path,int Ordinal,IReadOnlyList<Math2Mode0TransferCommand> Commands,long Reservation,
    Action<CancellationToken> Check);
internal enum Math2Mode0OriginalStage { Claimed, HistoricalPage, ScratchNode, ScratchPayloadNode, Preflight,
    TemporarySelection, HistoryPrepared, Registered, HistoryCommitted, Counter, SourceComplete,
    DomEntered, LivePayloadNode, DomInserted, PostValidated, Complete, SourceDeleted, RolledBack }
internal sealed record Math2Mode0OriginalDomNode(int Id,byte Kind,string Name,int Parent,int OriginalId,int PayloadOrigin,
    byte ValueTag,string? Text,long? Number,string? NamespaceUri,string? NamespacePrefix);
internal sealed record Math2Mode0OriginalRegistryRow(long Id,string Name,int Category,LegacyNativeInkPositionRange[] Ranges,int CanonicalC22Id);
internal sealed record Math2Mode0OriginalJournalRow(long Id,string Name,int Category,LegacyNativeInkPositionRange[] Ranges,
    bool DistinctHistoryClone,bool Reversed);
internal sealed record Math2Mode0OriginalSnapshot(bool Prepared,bool Failed,bool Released,string Phase,long Reservation,
    bool SourceEntered,bool SourceExecutionSucceeded,bool JournalComplete,bool DomEntered,bool Complete,bool RolledBack,
    long InitialNext,long ActiveNext,long LibraryEpoch,int CleanupCount,int ScratchField,int OriginalField,
    Math2Mode0OriginalDomNode[] HistoricalPage,Math2Mode0OriginalDomNode[] Scratch,Math2Mode0OriginalDomNode[] ActivePage,
    Math2Mode0OriginalRegistryRow[] ActiveRegistry,Math2Mode0OriginalJournalRow[] Journal,string[] Trace,bool CanonicalOriginalNames);

/// <summary>Bounded original Math registry/journal + original FIELD publication.
/// This is explicit library state, not native CBB/revisions/automatic rollback.</summary>
internal sealed class Math2Mode0OriginalApplication:IDisposable
{
    internal const long DefaultPayloadBudget=64L*1024*1024;
    private sealed class Claim {internal int Entered;}
    private static readonly ConditionalWeakTable<Math2Mode0OriginalMathFieldSource,Claim> Claims=new();
    private readonly object gate=new();
    private readonly Math2Mode0OriginalMathFieldSource field;
    private readonly Math2Mode0OutgoingTransfer outgoing;
    private readonly object actualInk;
    private readonly LegacyLibraryOriginalDomNameOwner originalNames;
    private readonly Math2Mode0OriginalArena historical,active,scratch;
    private readonly List<Math2Mode0OriginalJournal> journal=[];
    private readonly List<string> trace=[];
    private readonly long reservation,initialNext;
    private Math2Mode0OriginalRegistry[] registry=[];
    private long activeNext,epoch;
    private int originalField,scratchField,cleanupCount;
    private bool prepared,failed,released,sourceEntered,sourceSucceeded,journalComplete=true,domEntered,complete,rolledBack;
    private string phase="claimed";
    private Math2Mode0OriginalApplication(Math2Mode0OriginalInput original,Math2Mode0OriginalTransferInput transfer,long reservation)
    {field=original.Source;outgoing=transfer.Owner;actualInk=original.Ink;this.reservation=reservation;initialNext=original.InitialNext;activeNext=initialNext;
     originalNames=original.Names;historical=new(original.Names);active=new(original.Names);scratch=new(original.Names);}
    internal static Math2Mode0OriginalApplication Prepare(Math2Mode0OriginalMathFieldSource original,Math2Mode0OutgoingTransfer outgoing,CancellationToken token=default)
        =>PrepareDiagnostic(original,outgoing,DefaultPayloadBudget,null,token);
    internal static Math2Mode0OriginalApplication PrepareDiagnostic(Math2Mode0OriginalMathFieldSource original,Math2Mode0OutgoingTransfer outgoing,long budget,
        Action<Math2Mode0OriginalStage,int,Math2Mode0OriginalApplication>? observer,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(original);ArgumentNullException.ThrowIfNull(outgoing);token.ThrowIfCancellationRequested();
        if(budget<0||budget>DefaultPayloadBudget)throw new ArgumentOutOfRangeException(nameof(budget));
        return outgoing.WithOriginalApplicationInput(original,(input,transfer)=>
        {
            long planned=Math2Mode0OriginalKernel.Plan(input,transfer,budget,token);
            var claim=Claims.GetValue(original,_=>new());if(Interlocked.CompareExchange(ref claim.Entered,1,0)!=0)throw new InvalidOperationException("Original Math source already claimed by an application.");
            var target=new Math2Mode0OriginalApplication(input,transfer,planned);
            lock(target.gate)
            {
                try{target.PrepareCore(input,transfer,observer,token);return target;}
                catch{target.failed=true;if(observer is null)target.Dispose();throw;}
            }
        },token);
    }
    private void Observe(Math2Mode0OriginalStage stage,int index,Math2Mode0OriginalInput input,Math2Mode0OriginalTransferInput transfer,
        Action<Math2Mode0OriginalStage,int,Math2Mode0OriginalApplication>? observer,CancellationToken token)
    {phase=stage.ToString();observer?.Invoke(stage,index,this);ObjectDisposedException.ThrowIf(released,this);input.Check(token);transfer.Check(token);}
    private void PrepareCore(Math2Mode0OriginalInput input,Math2Mode0OriginalTransferInput transfer,
        Action<Math2Mode0OriginalStage,int,Math2Mode0OriginalApplication>? observer,CancellationToken token)
    {
        void Stage(Math2Mode0OriginalStage s,int i=-1)=>Observe(s,i,input,transfer,observer,token);
        registry=input.ReadRegistry();Stage(Math2Mode0OriginalStage.Claimed);
        historical.Import(input.HistoricalNodes);active.Import(input.HistoricalNodes);
        originalField=historical.Nodes.Single(n=>n.Kind==3&&n.Name.Text=="mathField").Id;Stage(Math2Mode0OriginalStage.HistoricalPage);
        scratchField=scratch.CloneOriginal(historical,originalField,0,n=>Stage(Math2Mode0OriginalStage.ScratchNode,n));
        int payload=scratch.ClonePayload(transfer.Nodes,transfer.PayloadRoot,0,n=>Stage(Math2Mode0OriginalStage.ScratchPayloadNode,n));
        scratch.Attach(scratchField,payload,transfer.Ordinal);Math2Mode0OriginalKernel.ValidateTags(scratch,scratchField,
            transfer.Commands.Where(c=>c.Operation==4).Select(c=>new Math2Mode0OriginalRegistry(c.Id,c.Name!,c.Category,c.Ranges,c,c)).Concat(registry).ToArray(),input.InputRanges,token);
        prepared=true;trace.Add("whole-kind3:scratch:compatible");Stage(Math2Mode0OriginalStage.Preflight);
    }
    private T WithLive<T>(Func<Math2Mode0OriginalInput,Math2Mode0OriginalTransferInput,T> action,CancellationToken token)
        =>outgoing.WithOriginalApplicationInput(field,(input,transfer)=>
        {lock(gate){ObjectDisposedException.ThrowIf(released,this);if(!ReferenceEquals(input.Ink,actualInk)||!prepared||failed)throw new InvalidOperationException("Exact prepared live original application required.");return action(input,transfer);}},token);
    internal void RunSource(CancellationToken token=default)=>RunSourceDiagnostic(null,-1,-1,token);
    internal void RunSourceDiagnostic(Action<Math2Mode0OriginalStage,int,Math2Mode0OriginalApplication>? observer,int beforeFailure=-1,int afterFailure=-1,CancellationToken token=default)
    {
        if(beforeFailure< -1||afterFailure< -1)throw new ArgumentOutOfRangeException(nameof(beforeFailure));
        WithLive((input,transfer)=>
        {
            token.ThrowIfCancellationRequested();if(sourceEntered||domEntered||rolledBack)throw new InvalidOperationException("Original source application already entered.");sourceEntered=true;
            try
            {
                int index=0;
                foreach(var command in transfer.Commands)
                {
                    input.Check(token);transfer.Check(token);
                    if(command.Operation==7)
                    {
                        if(input.Next()<command.NewCounter18)throw new NotSupportedException("Actual fresh counter7 must be skipped after contiguous registrations.");
                        trace.Add("counter7:skip");Observe(Math2Mode0OriginalStage.Counter,-1,input,transfer,observer,token);continue;
                    }
                    var temporary=input.CloneSelection(command.Ranges);Observe(Math2Mode0OriginalStage.TemporarySelection,index,input,transfer,observer,token);
                    Math2Mode0OriginalSelection? history=null;
                    if(index==beforeFailure){journal.Clear();journalComplete=false;trace.Add("observer90:refused:clear-frame");}
                    else history=input.CloneSelection(temporary.Ranges);
                    Observe(Math2Mode0OriginalStage.HistoryPrepared,index,input,transfer,observer,token);
                    var registered=input.Register(command,temporary);registry=input.ReadRegistry();activeNext=input.Next();
                    if(registered.Id!=command.Id)throw new NotSupportedException("Nonidentity original registration is outside this bounded FIELD application.");
                    trace.Add("original:register4:"+registered.Id);trace.Add("CB7:event0:discard:"+registered.Id);
                    Observe(Math2Mode0OriginalStage.Registered,index,input,transfer,observer,token);
                    if(index==afterFailure){journal.Clear();journalComplete=false;trace.Add("observer98:refused:clear-frame");}
                    else if(history is not null)journal.Add(new(command,history,temporary,registered));
                    Observe(Math2Mode0OriginalStage.HistoryCommitted,index,input,transfer,observer,token);index++;
                }
                sourceSucceeded=true;trace.Add("original:source:execution-succeeded");Observe(Math2Mode0OriginalStage.SourceComplete,-1,input,transfer,observer,token);return 0;
            }
            catch{failed=true;journalComplete=false;throw;}
        },token);
    }
    internal void ApplyDom(CancellationToken token=default)=>ApplyDomDiagnostic(null,false,token);
    internal void ApplyDomDiagnostic(Action<Math2Mode0OriginalStage,int,Math2Mode0OriginalApplication>? observer,bool refusePostcheck=false,CancellationToken token=default)
    {
        WithLive((input,transfer)=>
        {
            token.ThrowIfCancellationRequested();if(!sourceSucceeded||!journalComplete||domEntered||rolledBack)throw new InvalidOperationException("Complete original source execution/journal required before DOM entry.");
            domEntered=true;
            try
            {
                Observe(Math2Mode0OriginalStage.DomEntered,-1,input,transfer,observer,token);
                Math2Mode0OriginalKernel.ValidateActiveRegistry(input,transfer,registry);
                int payload=active.ClonePayload(transfer.Nodes,transfer.PayloadRoot,0,n=>Observe(Math2Mode0OriginalStage.LivePayloadNode,n,input,transfer,observer,token));
                active.Attach(originalField,payload,transfer.Ordinal);trace.Add("original:DOM:insert2:after-INPUT");Observe(Math2Mode0OriginalStage.DomInserted,-1,input,transfer,observer,token);
                if(refusePostcheck)throw new NotSupportedException("Prepared original postcheck refusal after mutation.");
                Math2Mode0OriginalKernel.ValidateTags(active,originalField,registry,input.InputRanges,token);Observe(Math2Mode0OriginalStage.PostValidated,-1,input,transfer,observer,token);
                epoch++;complete=true;trace.Add("library:DOM:epoch1");Observe(Math2Mode0OriginalStage.Complete,-1,input,transfer,observer,token);return 0;
            }
            catch{failed=true;throw;}
        },token);
    }
    internal void RollbackSource(CancellationToken token=default)=>RollbackSourceDiagnostic(null,token);
    internal void RollbackSourceDiagnostic(Action<Math2Mode0OriginalStage,int,Math2Mode0OriginalApplication>? observer,CancellationToken token=default)
    {
        WithLive((input,transfer)=>
        {
            token.ThrowIfCancellationRequested();if(!sourceSucceeded||!journalComplete||domEntered||rolledBack)throw new InvalidOperationException("Safe source-only inverse is unavailable after DOM entry or incomplete journal.");
            try
            {
                for(int i=journal.Count-1;i>=0;i--)
                {
                    input.Check(token);transfer.Check(token);var row=journal[i];input.Delete(row.Command.Id,row.Registered.Entry);row.Reversed=true;
                    registry=input.ReadRegistry();activeNext=input.Next();trace.Add("original:delete4:"+row.Command.Id);trace.Add("CB7:no-original-tag-match:"+row.Command.Id);
                    Observe(Math2Mode0OriginalStage.SourceDeleted,i,input,transfer,observer,token);
                }
                rolledBack=true;trace.Add("original:inverse:Next-monotonic");Observe(Math2Mode0OriginalStage.RolledBack,-1,input,transfer,observer,token);return 0;
            }
            catch{failed=true;throw;}
        },token);
    }
    internal Math2Mode0OriginalSnapshot CopySnapshot()
    {
        lock(gate)
        {
            var canonical=new Dictionary<object,int>(ReferenceEqualityComparer.Instance);
            var entries=registry.Select(r=>{if(!canonical.TryGetValue(r.C22,out int id)){id=canonical.Count+1;canonical.Add(r.C22,id);}return new Math2Mode0OriginalRegistryRow(r.Id,r.Name,r.Category,r.Ranges.ToArray(),id);}).ToArray();
            return new(prepared,failed,released,phase,reservation,sourceEntered,sourceSucceeded,journalComplete,domEntered,complete,rolledBack,
                initialNext,activeNext,epoch,cleanupCount,scratchField,originalField,historical.Snapshot(),scratch.Snapshot(),active.Snapshot(),entries,
                journal.Select(j=>new Math2Mode0OriginalJournalRow(j.Command.Id,j.Command.Name!,j.Command.Category,j.History.Ranges.ToArray(),!ReferenceEquals(j.History.Identity,j.Temporary.Identity)&&!ReferenceEquals(j.History.Identity,j.Registered.C22),j.Reversed)).ToArray(),trace.ToArray(),historical.HasCanonicalNames(originalNames)&&active.HasCanonicalNames(originalNames)&&scratch.HasCanonicalNames(originalNames));
        }
    }
    internal Math2Mode0OriginalSnapshot CopyResult(CancellationToken token=default)=>WithLive((_,_)=>
    {if(!complete||rolledBack)throw new InvalidOperationException("Completed original FIELD library application required.");return CopySnapshot();},token);
    public void Dispose(){lock(gate){if(released)return;released=true;cleanupCount++;}}
}

internal sealed class Math2Mode0OriginalJournal(Math2Mode0TransferCommand command,Math2Mode0OriginalSelection history,
    Math2Mode0OriginalSelection temporary,Math2Mode0OriginalRegistry registered)
{internal readonly Math2Mode0TransferCommand Command=command;internal readonly Math2Mode0OriginalSelection History=history,Temporary=temporary;internal readonly Math2Mode0OriginalRegistry Registered=registered;internal bool Reversed;}
