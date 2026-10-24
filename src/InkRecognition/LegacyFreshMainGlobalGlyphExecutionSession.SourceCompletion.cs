namespace InkRecognition;

public sealed partial class LegacyFreshMainGlobalGlyphExecutionSession
{
    // One actual source-completion mutation owner per completed TOP; no global
    // table, diagnostic reconstruction or publicly transferable capability.
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
    internal void FaultSourceCompletion(object actualOwner)
    {
        lock(gate)
        {
            if(!ReferenceEquals(sourceCompletionOwner,actualOwner))throw new InvalidOperationException("Claimed source-completion owner required.");
            stopped=true;
        }
    }
}
