namespace InkRecognition;

//Private constructor-local DATA cache: no source/context/pool/NN capability or callbacks.
internal sealed class LegacyWholeWordPreflightScalarCache
{
    private struct Entry { internal ulong Key; internal int Matching,References; }
    private readonly Entry[] slots;
    private readonly int maximumOwners;
    private int count;
    internal int Count=>count;
    internal int Capacity=>slots.Length;
    internal long ReservedBytes{get;}
    private LegacyWholeWordPreflightScalarCache(int capacity,int maximumOwners,long reserved)
    {this.maximumOwners=maximumOwners;ReservedBytes=reserved;slots=new Entry[capacity];}
    internal static LegacyWholeWordPreflightScalarCache Create(int referenceUses,long workspaceCeiling,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(referenceUses is <1 or >65536)throw new ArgumentOutOfRangeException(nameof(referenceUses));
        int owners=Math.Min(referenceUses,4096),capacity=2;
        while(capacity<2*owners)capacity*=2;
        //16-byte unmanaged entry; generous128 bytes covers this object/array headers.
        //Check BEFORE object/backing allocation; local workspace, not a cumulative debit.
        long reserved=checked(16L*capacity+128);
        if(reserved>workspaceCeiling)throw new NotSupportedException("Preflight scalar-cache workspace exceeds existing array ceiling.");
        return new(capacity,owners,reserved);
    }
    internal bool TryGet(ulong key,out int matching,out int references,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();matching=references=0;int at=Hash(key)&(slots.Length-1);
        for(int work=0;work<slots.Length;work++,at=(at+1)&(slots.Length-1))
        {
            token.ThrowIfCancellationRequested();ref var entry=ref slots[at];
            if(entry.References==0)return false;
            if(entry.Key==key){matching=entry.Matching;references=entry.References;return true;}
        }
        throw new InvalidOperationException("Scalar cache has no empty slot.");
    }
    internal void Store(ulong key,int matching,int references,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(matching<0||references<1)throw new ArgumentOutOfRangeException(nameof(references));
        int at=Hash(key)&(slots.Length-1);
        for(int work=0;work<slots.Length;work++,at=(at+1)&(slots.Length-1))
        {
            token.ThrowIfCancellationRequested();ref var entry=ref slots[at];
            if(entry.References!=0)
            {
                if(entry.Key!=key)continue;
                if(entry.Matching!=matching||entry.References!=references)throw new InvalidOperationException("Immutable preflight scalar changed.");
                return;
            }
            if(count==maximumOwners)throw new InvalidOperationException("Preflight owner bound exceeded.");
            entry.Key=key;entry.Matching=matching;entry.References=references;count++;return;
        }
        throw new InvalidOperationException("Scalar cache has no empty slot.");
    }
    //Canonical owner keys pack node24/arc16/flag56. Mix every field before
    //masking: XOR-fold alone ignores arc low bits and clusters a node's arcs.
    private static int Hash(ulong key)
    {
        unchecked
        {
            key^=key>>30;key*=0xbf58476d1ce4e5b9UL;
            key^=key>>27;key*=0x94d049bb133111ebUL;
            return (int)(key^(key>>31));
        }
    }
}
