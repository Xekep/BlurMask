using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathCandidateOrdering.Probe")]

namespace InkRecognition.Math;

/// <summary>Prepared owned0351E0 48-byte max heap. This does not evaluate
/// logits, apply masks, expand aliases or construct a native beam.</summary>
internal sealed class LegacyPreparedMathCandidateHeap
{
    private readonly byte[] storage;
    private readonly int capacity;
    private int writableSlot=-1;
    internal int Count {get;private set;}
    internal LegacyPreparedMathCandidateHeap(int capacity, ReadOnlySpan<byte> initialStorage)
    {
        if(capacity<0||capacity>32||initialStorage.Length!=capacity*48)
            throw new ArgumentException("Explicit prepared storage for at most32 heap slots required.");
        this.capacity=capacity;storage=initialStorage.ToArray();
    }
    internal byte[] CopyStorage()=>storage.ToArray();
    private float Cost(int slot)=>BinaryPrimitives.ReadSingleLittleEndian(storage.AsSpan(slot*48));
    private void Move(int from,int to)=>storage.AsSpan(from*48,48).CopyTo(storage.AsSpan(to*48));
    internal int TryOffer(float cost,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();writableSlot=-1;
        if(capacity==0)return -1;
        int hole;
        if(Count==capacity)
        {
            //COMISS cost,root / JAE: unordered also falls through.
            if(cost>=Cost(0))return -1;
            hole=0;
            while(2*hole<Count-1)
            {
                int child=2*hole+1;
                //Equal children select the right. An unordered comparison
                //keeps the left; a generic comparer changes that behavior.
                if(child<Count-1&&Cost(child+1)>=Cost(child))child++;
                if(cost>=Cost(child))break;
                Move(child,hole);hole=child;
            }
        }
        else
        {
            hole=Count++;
            while(hole!=0)
            {
                int parent=(hole-1)/2;
                if(Cost(parent)>=cost)break;
                Move(parent,hole);hole=parent;
            }
        }
        storage.AsSpan(hole*48,48).Clear();
        BinaryPrimitives.WriteSingleLittleEndian(storage.AsSpan(hole*48),cost);
        writableSlot=hole;return hole;
    }
    /// <summary>Explicit prepared caller write after0351E0 returns. Single-use
    /// until the next offer; it is not a restored native caller or pointer.</summary>
    internal void WritePreparedPayload(int returnedSlot,ReadOnlySpan<byte> payload44)
    {
        if(returnedSlot<0||returnedSlot!=writableSlot||payload44.Length!=44)
            throw new ArgumentException("Current returned slot and complete prepared44-byte payload required.");
        payload44.CopyTo(storage.AsSpan(returnedSlot*48+4));writableSlot=-1;
    }
}
