using System.Runtime.CompilerServices;
namespace InkRecognition;

// No prepared/DATA constructor. Both actual configured factories share one
// private CWT entry for TOP and a locked preflight-through-claim transition.
internal static class LegacyConfiguredSourceCompletionClaim
{
    internal interface IPreflightLease : IDisposable
    { void Enter(object actualOwner,bool bindThread,CancellationToken token); }
    private sealed class State
    {
        private readonly object gate=new();
        private object? owner;
        private int stage,thread,preflightThread;
        private bool bindThread,sourceMarked;
        private sealed class Lease : IPreflightLease
        {
            private readonly State state;
            private bool disposed;
            internal Lease(State state){this.state=state;}
            public void Enter(object actualOwner,bool bind,CancellationToken token)
            {
                ArgumentNullException.ThrowIfNull(actualOwner);
                token.ThrowIfCancellationRequested();
                if(disposed||state.preflightThread!=Environment.CurrentManagedThreadId||!Monitor.IsEntered(state.gate)||state.stage!=0)throw new InvalidOperationException("This actual configured TOP already entered source completion.");
                state.owner=actualOwner;state.bindThread=bind;state.thread=Environment.CurrentManagedThreadId;state.stage=1;
            }
            public void Dispose()
            {
                if(disposed)return;
                if(state.preflightThread!=Environment.CurrentManagedThreadId||!Monitor.IsEntered(state.gate))
                    throw new InvalidOperationException("Only the owning preflight thread may release this lease.");
                disposed=true;state.preflightThread=0;Monitor.Exit(state.gate);
            }
        }
        internal IPreflightLease Acquire(CancellationToken token)
        {
            // Bounded wait slices, not a wall-clock completion deadline.
            // Cancellation is observed even while another preparation holds it.
            while(true)
            {
                token.ThrowIfCancellationRequested();
                if(Monitor.TryEnter(gate,25))break;
            }
            try
            {
                token.ThrowIfCancellationRequested();
                if(stage!=0)throw new InvalidOperationException("This actual configured TOP already entered source completion.");
                if(preflightThread!=0)
                    throw new InvalidOperationException("Nested configured source preflight cannot borrow an active lease.");
                var lease=new Lease(this);
                preflightThread=Environment.CurrentManagedThreadId;return lease;
            }
            catch{Monitor.Exit(gate);throw;}
        }
        internal void Require(object actualOwner,bool completed,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();lock(gate)
                if(!ReferenceEquals(owner,actualOwner)||stage!=(completed?2:1)||
                    !completed&&bindThread&&thread!=Environment.CurrentManagedThreadId)
                    throw new InvalidOperationException("Same entered configured source owner required.");
        }
        internal void MarkAfterCallback(object actualOwner)
        {
            lock(gate)
            {
                if(stage!=1||!ReferenceEquals(owner,actualOwner))throw new InvalidOperationException("Actual source callback claim required.");
                sourceMarked=true;
            }
        }
        internal void Complete(object actualOwner,CancellationToken token)
        {
            Require(actualOwner,false,token);lock(gate)
            {if(!sourceMarked)throw new InvalidOperationException("Source callback mark missing.");stage=2;}
        }
        internal void Poison(object actualOwner)
        {lock(gate)if(ReferenceEquals(owner,actualOwner)&&stage==1)stage=-1;}
    }
    private static readonly ConditionalWeakTable<LegacyFreshMainPlainResultOwnership,State> states=new();
    internal static IPreflightLease Acquire(LegacyFreshMainPlainResultOwnership top,CancellationToken token)
    {token.ThrowIfCancellationRequested();return states.GetValue(top.TopOwner,static _=>new()).Acquire(token);}
    private static State Actual(LegacyFreshMainPlainResultOwnership top)
        =>states.TryGetValue(top.TopOwner,out var state)?state:throw new InvalidOperationException("Actual configured source claim missing.");
    internal static void Require(LegacyFreshMainPlainResultOwnership top,object owner,bool completed,CancellationToken token)
        =>Actual(top).Require(owner,completed,token);
    //13E886 occurs AFTERcallback evenfalse; never a fallible budget debit.
    internal static void MarkAfterCallback(LegacyFreshMainPlainResultOwnership top,object owner)
        =>Actual(top).MarkAfterCallback(owner);
    internal static void Complete(LegacyFreshMainPlainResultOwnership top,object owner,CancellationToken token)
        =>Actual(top).Complete(owner,token);
    internal static void Poison(LegacyFreshMainPlainResultOwnership top,object owner)
    {if(states.TryGetValue(top.TopOwner,out var state))state.Poison(owner);}
}
