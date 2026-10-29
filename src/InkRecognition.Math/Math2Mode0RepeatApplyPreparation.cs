namespace InkRecognition.Math;

internal sealed partial class Math2Mode0OriginalApplication
{
    internal sealed partial class CurrentInputBorrow
    {
        internal Math2Mode0CurrentInputHandle RequireHandleForApply(Math2Mode0PriorCandidatePreparation.DiffBorrow candidate,
            Math2Mode0PriorDomDiff.TemporaryApplyBorrow caller,Math2Mode0PriorLateRegistration.RepeatBorrow.Access access)
        {candidate.RequireRepeatCurrent(this,caller,access);Check();return handle;}
    }
}
internal sealed partial class Math2Mode0PriorLateRegistration
{
    internal sealed partial class RepeatBorrow
    {
        internal void PrepareOriginalApply(Math2Mode0PriorRepeatOutgoing target,Math2Mode0RepeatApplyPreparation preparation,Math2DomDiffWork work)
        {WithAccess(target,AccessKind.Validate,a=>{target.PrepareOriginalApplyClaimed(this,preparation,input.ReadRepeatCohort(a,work));return 0;});}
    }
}
internal sealed partial class Math2Mode0PriorRepeatOutgoing
{
    private Math2Mode0RepeatApplyPreparation? activeApplyPreparation;
    private OriginalPayloadLease? activeApplyLease;private bool originalApplyClaimed;
    // The lease retains the ACTUAL immutable serialized objects. Dispose of the
    // exporting facade may revoke its reads, but cannot clear leased storage.
    internal sealed class OriginalPayloadLease:IDisposable
    {
        private readonly object leaseGate=new();private readonly Math2Mode0PriorRepeatOutgoing owner;
        private readonly Math2Mode0RepeatApplyPreparation consumer;
        private bool released;
        private OriginalPayloadLease(Math2Mode0PriorRepeatOutgoing owner,Math2Mode0RepeatApplyPreparation consumer)
        {this.owner=owner;this.consumer=consumer;}
        internal static OriginalPayloadLease Issue(Math2Mode0PriorRepeatOutgoing owner,Math2Mode0RepeatApplyPreparation consumer)
        {
            if(!Monitor.IsEntered(owner.gate)||!ReferenceEquals(owner.activeApplyPreparation,consumer)||owner.originalApplyClaimed||owner.activeApplyLease is not null||owner.released||owner.failed||!owner.ready||!owner.complete)
                throw new InvalidOperationException("Only the private ready outgoing preparation may retain its actual payload bundle.");
            return new(owner,consumer);
        }
        internal void Check(Math2Mode0RepeatApplyPreparation actual)
        {
            lock(leaseGate)
            {
                if(released||!ReferenceEquals(actual,consumer))
                    throw new InvalidOperationException("Exact unrevoked owned outgoing payload lease required.");
            }
        }
        internal (int Dom,int Source,int Nodes) Counts(Math2Mode0RepeatApplyPreparation actual)
        {lock(leaseGate){Check(actual);return(owner.dom.Count,owner.source.Count,owner.fourth.Nodes.Count);}}
        public void Dispose()
        {lock(owner.gate){lock(leaseGate){if(released)return;released=true;if(ReferenceEquals(owner.activeApplyLease,this))owner.activeApplyLease=null;if(owner.released)owner.ClearRepeatApplyPayloads();}}}
    }
    private void ClearRepeatApplyPayloads()
    {dom.Clear();source.Clear();raw.Clear();fourth.ClearRepeatOwned();originalBefore=[];lateBefore=[];originalAfter=[];lateAfter=[];originalTree=[];lateTree=[];}
    internal void WithOriginalApplyPreparation(Math2Mode0RepeatApplyPreparation target,Math2DomDiffWork work,CancellationToken token)
        =>parent.WithRepeatInput(b=>
        {
            lock(gate)
            {
                ObjectDisposedException.ThrowIf(released,this);if(failed||!complete||!ready||activeApplyPreparation is not null||originalApplyClaimed)throw new InvalidOperationException("Exclusive unclaimed ready repeat export required.");
                activeApplyPreparation=target;activeValidateBorrow=b;
                try{b.PrepareOriginalApply(this,target,work);return 0;}
                finally{activeValidateBorrow=null;activeApplyPreparation=null;}
            }
        },token);
    internal void PrepareOriginalApplyClaimed(Math2Mode0PriorLateRegistration.RepeatBorrow input,Math2Mode0RepeatApplyPreparation target,Math2Mode0RepeatCohort cohort)
    {
        RequirePrivateAccess(input,Math2Mode0PriorLateRegistration.RepeatBorrow.AccessKind.Validate);cohort.Check();
        if(!ReferenceEquals(activeApplyPreparation,target)||!target.IsFor(this))throw new InvalidOperationException("Exact active private preparation required.");
        var lease=OriginalPayloadLease.Issue(this,target);activeApplyLease=lease;originalApplyClaimed=true;
        try{target.BindActualLease(this,lease,cohort);}catch{lease.Dispose();throw;}
    }
}
internal sealed record Math2Mode0RepeatApplyPreparationSnapshot(bool Prepared,bool Ready,bool Failed,bool Released,
    bool OldReceiptConsumed,bool MutationEntered,int DomOperations,int SourceCommands,int RetainedNodes,long OldEpoch,
    long OldNext,int RawObjects,string Scope);
// PREPARATION ONLY in this increment: no original mutation, successor, receiver
// or apply API is supplied. Mutation will require an independently pinned grant.
internal sealed class Math2Mode0RepeatApplyPreparation:IDisposable
{
    private readonly object gate=new();private readonly Math2Mode0PriorRepeatOutgoing outgoing;
    private Math2Mode0PriorRepeatOutgoing.OriginalPayloadLease? lease;
    private Math2Mode0OriginalApplication? application;private Math2Mode0CurrentInputHandle? handle;
    private object? originalInk,originalArena;private LegacyOwnedRawInkDocument? document;private Math2Mode0CurrentInputReceipt? receipt;
    private long epoch,next;private bool prepared,ready,failed,released;private int dom,source,nodes;
    private Math2Mode0RepeatApplyPreparation(Math2Mode0PriorRepeatOutgoing outgoing){this.outgoing=outgoing;}
    internal bool IsFor(Math2Mode0PriorRepeatOutgoing actual)=>ReferenceEquals(outgoing,actual);
    internal static Math2Mode0RepeatApplyPreparation Prepare(Math2Mode0PriorRepeatOutgoing outgoing,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(outgoing);token.ThrowIfCancellationRequested();var result=new Math2Mode0RepeatApplyPreparation(outgoing);
        try
        {
            outgoing.WithOriginalApplyPreparation(result,new(token),token);
            lock(result.gate)
            {
                ObjectDisposedException.ThrowIf(result.released,result);if(!result.prepared||result.lease is null)throw new InvalidOperationException("Whole owned payload preparation required.");
                result.lease.Check(result);result.ready=true;return result;
            }
        }
        catch{lock(result.gate)result.failed=true;result.Dispose();throw;}
    }
    internal void BindActualLease(Math2Mode0PriorRepeatOutgoing actual,Math2Mode0PriorRepeatOutgoing.OriginalPayloadLease payload,Math2Mode0RepeatCohort cohort)
    {
        lock(gate)
        {
            if(prepared||released||failed||!ReferenceEquals(outgoing,actual))throw new InvalidOperationException("Private synchronous preparation required.");
            cohort.Check();payload.Check(this);lease=payload;application=cohort.Application;handle=cohort.Handle;originalInk=cohort.OriginalInk;originalArena=cohort.Original;document=cohort.Document;receipt=cohort.Receipt;epoch=cohort.Epoch;next=cohort.OldNext;
            var counts=payload.Counts(this);dom=counts.Dom;source=counts.Source;nodes=counts.Nodes;prepared=true;
        }
    }
    internal Math2Mode0RepeatApplyPreparationSnapshot CopySnapshot()
    {lock(gate)return new(prepared,ready,failed,released,false,false,dom,source,nodes,epoch,next,document?.ObjectCount??0,"Actual owned export lease/preparation only; no mutation or original apply.");}
    internal Math2Mode0RepeatApplyPreparationSnapshot CopyOwnedResult()
    {lock(gate){ObjectDisposedException.ThrowIf(released,this);if(failed||!prepared||!ready||lease is null)throw new InvalidOperationException("Ready owning payload preparation required.");lease.Check(this);var counts=lease.Counts(this);if(counts!=(dom,source,nodes))throw new InvalidOperationException("Owned payload storage changed.");return CopySnapshot();}}
    public void Dispose()
    {
        Math2Mode0PriorRepeatOutgoing.OriginalPayloadLease? release;
        lock(gate){if(released)return;released=true;release=lease;lease=null;application=null;handle=null;originalInk=null;originalArena=null;document=null;receipt=null;}
        release?.Dispose();
    }
}
