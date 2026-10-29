using System.Runtime.CompilerServices;
namespace InkRecognition.Math;

internal sealed partial class Math2Mode0PriorCandidatePreparation
{
    internal sealed partial class DiffBorrow
    {
        internal int TemporaryNodeCount{get{Check();return owner.temporary!.Nodes.Count;}}
        internal bool ApplyProducedTemporaryAction(Math2Mode0PriorDomDiff.TemporaryApplyBorrow authority,Math2DomDiffWork work)
        {
            Check();
            // No post-call callback/check here: the caller first records whether
            // this operation completed, then rechecks its still-locked owners.
            return authority.WithGrantedProducedAction(this,action=>owner.temporary!.ApplyTemporaryAction(action,work));
        }
        internal Math2Mode0OriginalDomNode[] CopyTemporary(Math2DomDiffWork work)
        {Check();work.Visit(checked(1L+owner.temporary!.Nodes.Count*4L));return owner.temporary.Snapshot();}
    }
}

internal sealed partial class Math2Mode0PriorDomDiff
{
    private int temporaryApplicationClaimed;
    private TemporaryApplyBorrow? activeTemporaryApplyBorrow;
    internal sealed partial class TemporaryApplyBorrow
    {
        private readonly Math2Mode0PriorDomDiff owner;
        private readonly Math2Mode0PriorCandidatePreparation.DiffBorrow input;
        private readonly Action check;
        private bool claimed;
        private int next;
        private Math2DomDiffAction? grantedAction;
        private TemporaryApplyBorrow(Math2Mode0PriorDomDiff owner,Math2Mode0PriorCandidatePreparation.DiffBorrow input,Action check)
        {this.owner=owner;this.input=input;this.check=check;}
        internal static TemporaryApplyBorrow Issue(Math2Mode0PriorDomDiff owner,Math2Mode0PriorCandidatePreparation.DiffBorrow input,Action check)
        {if(!Monitor.IsEntered(owner.gate)||owner.activeTemporaryApplyBorrow is not null)throw new InvalidOperationException("Exclusive actual diff gate required.");check();return new(owner,input,check);}
        internal void Check()
        {if(!Monitor.IsEntered(owner.gate)||!ReferenceEquals(owner.activeTemporaryApplyBorrow,this))throw new InvalidOperationException("Exact synchronous temporary-apply borrow required.");check();}
        internal object Identity{get{Check();return owner;}}
        internal bool HeaderLive{get{Check();return owner.headerLive;}}
        internal int Count{get{Check();return owner.kept.Count;}}
        internal long Plan(long budget,bool diagnosticSnapshots,Math2DomDiffWork work)
        {
            Check();work.Visit();if(!owner.headerLive)throw new InvalidOperationException("Actual diff header already consumed.");
            long nodes=input.TemporaryNodeCount,actions=owner.kept.Count,copies=diagnosticSnapshots?actions+2:1;
            long plan=checked(owner.reservation+65536+nodes*512*copies+actions*2048);
            if(plan>budget)throw new NotSupportedException("Whole temporary application/copy envelope exceeds budget before claim.");
            return plan;
        }
        internal void Claim(Math2DomDiffWork work)
        {
            Check();work.Visit();if(!owner.headerLive||Interlocked.CompareExchange(ref owner.temporaryApplicationClaimed,1,0)!=0)
                throw new InvalidOperationException("Actual diff temporary-apply claim already consumed.");
            claimed=true;
        }
        internal bool ApplyNext(Math2DomDiffWork work)
        {
            Check();work.Visit();if(!claimed||!owner.headerLive||next>=owner.kept.Count)throw new InvalidOperationException("Actual unconsumed produced action required.");
            if(grantedAction is not null)throw new InvalidOperationException("Exclusive current produced-action grant required.");
            grantedAction=owner.kept[next];
            try{bool success=input.ApplyProducedTemporaryAction(this,work);if(success)next++;return success;}
            finally{grantedAction=null;}
        }
        internal bool WithGrantedProducedAction(Math2Mode0PriorCandidatePreparation.DiffBorrow actualInput,Func<Math2DomDiffAction,bool> consume)
        {
            Check();
            if(!claimed||!owner.headerLive||!ReferenceEquals(input,actualInput)||grantedAction is null||next>=owner.kept.Count||!ReferenceEquals(owner.kept[next],grantedAction))
                throw new InvalidOperationException("Only this active next kept action may enter the temporary writer.");
            return consume(grantedAction);
        }
        internal void ConsumeHeader(Math2DomDiffWork work)
        {
            Check();work.Visit();if(!claimed||!owner.headerLive||next!=owner.kept.Count)throw new InvalidOperationException("Whole successful produced traversal required.");
            // Native E2560 consumes the header only after E4B90 returns success.
            // No callback, allocation or cancellation check after this commit;
            // the caller records the fact before performing its next check.
            owner.headerLive=false;
        }
        internal Math2Mode0OriginalDomNode[] CopyTemporary(Math2DomDiffWork work)
        {Check();return input.CopyTemporary(work);}
    }
    internal T WithTemporaryApplyInput<T>(Func<TemporaryApplyBorrow,T> consume,CancellationToken token)
        =>candidate.WithDiffInput(input=>
        {
            lock(gate)
            {
                void Check(){input.Check();ObjectDisposedException.ThrowIf(released,this);if(failed||!complete||!ready)throw new InvalidOperationException("Same ready actual diff required.");token.ThrowIfCancellationRequested();}
                Check();if(activeTemporaryApplyBorrow is not null)throw new InvalidOperationException("Exclusive temporary application borrow required.");
                var borrow=TemporaryApplyBorrow.Issue(this,input,Check);activeTemporaryApplyBorrow=borrow;
                try{var value=consume(borrow);borrow.Check();return value;}finally{activeTemporaryApplyBorrow=null;}
            }
        },token);
}

internal enum Math2Mode0TemporaryApplyStage{Claimed,BeforeTraversal,ActionCompleted,HeaderConsumed,Completed}
internal sealed record Math2Mode0TemporaryApplySnapshot(bool Complete,bool Ready,bool Failed,bool Released,long Reservation,long Visits,
    int CompletedActions,int ObservedAfterActions,bool HeaderConsumed,bool PartialActionMayExist,Math2Mode0OriginalDomNode[] ObservedTemporary);
internal sealed partial class Math2Mode0PriorTemporaryApply:IDisposable
{
    private readonly object gate=new();
    private readonly Math2Mode0PriorDomDiff diff;
    private readonly long reservation;
    private readonly Math2DomDiffWork work;
    private Math2Mode0OriginalDomNode[] observed=[];
    private int completedActions,observedAfterActions;
    private bool complete,ready,failed,released,headerConsumed,actionInProgress;
    private Math2Mode0PriorTemporaryApply(Math2Mode0PriorDomDiff diff,long reservation,Math2DomDiffWork work)
    {this.diff=diff;this.reservation=reservation;this.work=work;}
    internal static Math2Mode0PriorTemporaryApply Create(Math2Mode0PriorDomDiff diff,CancellationToken token=default)
        =>CreateDiagnostic(diff,Math2Mode0PriorCandidatePreparation.DefaultBudget,null,token);
    internal static Math2Mode0PriorTemporaryApply CreateDiagnostic(Math2Mode0PriorDomDiff diff,long budget,
        Action<Math2Mode0TemporaryApplyStage,Math2Mode0PriorTemporaryApply>? observe,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(diff);token.ThrowIfCancellationRequested();if(budget<0||budget>Math2Mode0PriorCandidatePreparation.DefaultBudget)throw new ArgumentOutOfRangeException(nameof(budget));Math2Mode0PriorTemporaryApply? target=null;
        try
        {
            var result=diff.WithTemporaryApplyInput(input=>
            {
                var work=new Math2DomDiffWork(token);long plan=input.Plan(budget,observe is not null,work);input.Claim(work);
                target=new(diff,plan,work);lock(target.gate)
                {
                    void Stage(Math2Mode0TemporaryApplyStage stage){observe?.Invoke(stage,target);input.Check();ObjectDisposedException.ThrowIf(target.released,target);token.ThrowIfCancellationRequested();}
                    void ObserveNodes(){target.observed=input.CopyTemporary(work);target.observedAfterActions=target.completedActions;}
                    if(observe is not null)ObserveNodes();Stage(Math2Mode0TemporaryApplyStage.Claimed);Stage(Math2Mode0TemporaryApplyStage.BeforeTraversal);
                    int count=input.Count;
                    for(int i=0;i<count;i++)
                    {
                        target.actionInProgress=true;
                        if(!input.ApplyNext(work))throw new NotSupportedException("Unknown produced operation stopped temporary traversal; header and prior prefix retained.");
                        target.completedActions++;target.actionInProgress=false;input.Check();
                        if(observe is not null)ObserveNodes();Stage(Math2Mode0TemporaryApplyStage.ActionCompleted);
                    }
                    input.ConsumeHeader(work);target.headerConsumed=true;
                    Stage(Math2Mode0TemporaryApplyStage.HeaderConsumed);ObserveNodes();target.complete=true;Stage(Math2Mode0TemporaryApplyStage.Completed);return target;
                }
            },token);
            lock(result.gate){ObjectDisposedException.ThrowIf(result.released,result);if(result.failed||!result.complete||!result.headerConsumed)throw new InvalidOperationException("Temporary application completion fence failed.");result.ready=true;}return result;
        }
        catch{if(target is not null){lock(target.gate){target.failed=true;if(observe is null)target.Dispose();}}throw;}
    }
    internal Math2Mode0TemporaryApplySnapshot CopyResult(CancellationToken token=default)
        =>diff.WithTemporaryApplyInput(input=>{lock(gate){input.Check();ObjectDisposedException.ThrowIf(released,this);if(failed||!complete||!ready||!headerConsumed||input.HeaderLive||!ReferenceEquals(input.Identity,diff))throw new InvalidOperationException("Same ready actual temporary application required.");return CopySnapshot();}},token);
    internal Math2Mode0TemporaryApplySnapshot CopySnapshot()
    {lock(gate)return new(complete,ready,failed,released,reservation,work.Visits,completedActions,observedAfterActions,headerConsumed,actionInProgress,observed.ToArray());}
    public void Dispose(){lock(gate){if(released)return;released=true;observed=[];}}
}
