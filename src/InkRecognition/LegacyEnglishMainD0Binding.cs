namespace InkRecognition;

public sealed partial class LegacyEnglish119MainLocalCandidates
{
    internal LegacyOrdinaryCursiveCandidateGraph RetainCompletedD0Graph(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(!complete||faulted||ranges is null)throw new InvalidOperationException("Complete actual EN MAIN graph required.");
            return ranges.Graph;
        }
    }
}

// A permanently claimed source binding rejects the older PREPARED transport
// controls through their existing active guard. No public raw map/lease API.
public sealed partial class LegacyEnglishFreshDictionaryQuerySession
{
    private LegacyEnglishMainSpanSession? d0Owner;
    private bool d0Busy;
    private long d0Version;
    internal LegacyPreparedDictionaryWorkingMap ClaimD0Owner(LegacyEnglishMainSpanSession owner,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {
            token.ThrowIfCancellationRequested();ValidateMutable();
            if(d0Owner is not null||!ReferenceEquals(owner.QuerySession,this)||operations!=0||attempts!=0||outputs!=0||checkpoints!=1||epoch!=1||contexts.NodeCount!=InitialContextCount)
                throw new InvalidOperationException("Exactly one pristine fresh source binding required before D0 ownership.");
            d0Owner=owner;active=true;return working;
        }
    }
    internal LegacyEnglishD0MapLease BeginD0(LegacyEnglishMainSpanSession owner,LegacyPreparedDictionaryWorkingMap expected,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {
            token.ThrowIfCancellationRequested();Validate(Scorer,LanguageQuery);
            if(faulted||!active||d0Busy||!ReferenceEquals(d0Owner,owner)||!ReferenceEquals(working,expected))throw new InvalidOperationException("Current same-owner D0 map required.");
            if(operations>=Bounds.MaximumOperations)throw new ArgumentException("Remaining source operation budget required before entry.");
            var lease=new LegacyEnglishD0MapLease(this,owner,working,checked(d0Version+1));
            d0Busy=true;operations++;return lease;
        }
    }
    internal void CommitD0(LegacyEnglishD0MapLease lease,LegacyEnglishMainD0Receipt receipt,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(faulted||!active||!d0Busy||!ReferenceEquals(lease.Binding,this)||!ReferenceEquals(d0Owner,lease.Owner)||!ReferenceEquals(working,lease.Previous)||lease.Version!=d0Version+1)
                throw new InvalidOperationException("Current unconsumed D0 lease required.");
            var next=receipt.TakeMapForInstallation(lease);
            if(next.DescriptorFlags!=DescriptorFlags||next.AcousticSymbolCount!=working.AcousticSymbolCount||next.LexicalSymbolCount!=working.LexicalSymbolCount||!next.SharesAuxiliaryMap(working))
                throw new InvalidDataException("Actual same-source refreshed map lineage required.");
            // Native refresh changes working50. Initial68 and query ring survive.
            // SetWorkingMap validates dimensions and assigns without callbacks.
            querySource.SetWorkingMap(next);working=next;d0Version=lease.Version;d0Busy=false;
        }
    }
    internal bool IsCurrentD0(LegacyEnglishMainSpanSession owner,long version)
    {lock(gate)return !faulted&&active&&!d0Busy&&ReferenceEquals(d0Owner,owner)&&version==d0Version;}
    internal void PoisonD0(LegacyEnglishMainSpanSession owner)
    {lock(gate){if(ReferenceEquals(d0Owner,owner)){faulted=true;d0Busy=false;active=true;}}}
}

internal sealed class LegacyEnglishD0MapLease
{
    internal LegacyEnglishFreshDictionaryQuerySession Binding{get;}
    internal LegacyEnglishMainSpanSession Owner{get;}
    internal LegacyPreparedDictionaryWorkingMap Previous{get;}
    internal long Version{get;}
    internal LegacyEnglishD0MapLease(LegacyEnglishFreshDictionaryQuerySession binding,LegacyEnglishMainSpanSession owner,LegacyPreparedDictionaryWorkingMap previous,long version)
    {Binding=binding;Owner=owner;Previous=previous;Version=version;}
}
