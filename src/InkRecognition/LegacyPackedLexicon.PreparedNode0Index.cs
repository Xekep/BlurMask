namespace InkRecognition;

public sealed partial class LegacyPackedLexicon
{
    // Default-null fields only: no constructor initializer, allocation or change
    // to existing Load/ReadEnglish102. Exact model lifetime owns this value.
    private LegacyEnglishCaseNode0LexiconIndex.PreparedData? preparedNode0Index;
    private int node0IndexPreparing,node0IndexPreparingThread;
    internal LegacyEnglishCaseNode0LexiconIndex.PreparedData RequirePreparedNode0Index(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var result=Volatile.Read(ref preparedNode0Index);
        if(result is null||!result.IsFor(this))
            throw new NotSupportedException("Explicit same-model immutable Node0 index preparation required.");
        return result;
    }
    internal bool TryClaimNode0IndexPreparation(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var result=Volatile.Read(ref preparedNode0Index);
        if(result is not null)
        {
            if(!result.IsFor(this))throw new InvalidDataException("Foreign immutable index cache.");
            return false;
        }
        if(Interlocked.CompareExchange(ref node0IndexPreparing,1,0)!=0)
            throw new InvalidOperationException("Same model index preparation already active; no waiting/callback.");
        // A previous winner may have published between our first read and CAS.
        if(Volatile.Read(ref preparedNode0Index) is not null)
        {Volatile.Write(ref node0IndexPreparing,0);return false;}
        node0IndexPreparingThread=Environment.CurrentManagedThreadId;
        return true;
    }
    internal void PublishNode0Index(LegacyEnglishCaseNode0LexiconIndex.PreparedData result)
    {
        if(Volatile.Read(ref node0IndexPreparing)!=1||node0IndexPreparingThread!=Environment.CurrentManagedThreadId||!result.IsFor(this)||
            Volatile.Read(ref preparedNode0Index) is not null)
            throw new InvalidOperationException("Exact once-published immutable model index required.");
        Volatile.Write(ref preparedNode0Index,result);
    }
    internal void ReleaseNode0IndexPreparation()
    {
        if(Volatile.Read(ref node0IndexPreparing)!=1||node0IndexPreparingThread!=Environment.CurrentManagedThreadId)
            throw new InvalidOperationException("Only actual model-index preparation thread may release its claim.");
        node0IndexPreparingThread=0;Volatile.Write(ref node0IndexPreparing,0);
    }
}
