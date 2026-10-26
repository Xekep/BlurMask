namespace InkRecognition;

public sealed partial class LegacyFreshMainGlobalGlyphExecutionSession
{
    // One actual source-completion mutation owner per completed TOP; no global
    // table, diagnostic reconstruction or publicly transferable capability.
    private int sourcePreflightThread;
    internal sealed class SourcePreflight : IDisposable
    {
        private readonly LegacyFreshMainGlobalGlyphExecutionSession session;
        private bool disposed;
        internal SourcePreflight(LegacyFreshMainGlobalGlyphExecutionSession session){this.session=session;}
        public void Dispose()
        {
            if(disposed)return;
            if(session.sourcePreflightThread!=Environment.CurrentManagedThreadId||!Monitor.IsEntered(session.gate))
                throw new InvalidOperationException("Only the actual GLOBAL preflight thread may release its lease.");
            disposed=true;session.sourcePreflightThread=0;Monitor.Exit(session.gate);
        }
    }
    internal SourcePreflight AcquireSourcePreflight(CancellationToken token)
    {
        while(true){token.ThrowIfCancellationRequested();if(Monitor.TryEnter(gate,25))break;}
        try
        {
            token.ThrowIfCancellationRequested();
            if(sourcePreflightThread!=0||sourceCompletionOwner is not null||IsStopped||!IsComplete||Volatile.Read(ref active)!=0)
                throw new InvalidOperationException("The actual GLOBAL TOP is unavailable for source preflight.");
            var lease=new SourcePreflight(this);sourcePreflightThread=Environment.CurrentManagedThreadId;return lease;
        }
        catch{Monitor.Exit(gate);throw;}
    }
    private object? sourceCompletionOwner;
    private int sourceCompletionThread;
    internal void ClaimSourceCompletion(object actualOwner,bool bindThread,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(actualOwner);token.ThrowIfCancellationRequested();
        lock(gate)
        {
            if(sourceCompletionOwner is not null||IsStopped||!IsComplete||Volatile.Read(ref active)!=0)
                throw new InvalidOperationException("Actual completed global TOP source stage already claimed or unavailable.");
            Ownership.CompactOwner.Validate(token);
            sourceCompletionOwner=actualOwner;sourceCompletionThread=bindThread?Environment.CurrentManagedThreadId:0;
        }
    }
    internal void RequireSourceCompletion(object actualOwner,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {
            if(!ReferenceEquals(sourceCompletionOwner,actualOwner)||IsStopped||!IsComplete||
                sourceCompletionThread!=0&&sourceCompletionThread!=Environment.CurrentManagedThreadId)
                throw new InvalidOperationException("Exact live global source-completion owner/thread required.");
            Ownership.CompactOwner.Validate(token);
        }
    }
    internal void CompleteSourceCompletion(object actualOwner,CancellationToken token)
    {
        RequireSourceCompletion(actualOwner,token);
        lock(gate){sourceCompletionThread=0;}
    }
    internal void FaultSourceCompletion(object actualOwner)
    {
        lock(gate)
        {
            if(!ReferenceEquals(sourceCompletionOwner,actualOwner))throw new InvalidOperationException("Claimed source-completion owner required.");
            stopped=true;
        }
    }
}
