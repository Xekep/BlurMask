using System.Buffers.Binary;
namespace InkRecognition;

// Typed SAME-live generic stage overloads. No historical descriptor promotion;
// accepted initializer is authenticated before hole mutation. OLD methods exact.
public sealed partial class LegacyNativeWordHeapDescriptor
{
    internal bool TryAllocateGraphAfterHole(LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,int candidateCost,
        LegacyNativeWordSlotInitializer initializeAcceptedSlot,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(stage);ArgumentNullException.ThrowIfNull(initializeAcceptedSlot);token.ThrowIfCancellationRequested();
        stage.RequireOwnedFirstWriter(initializeAcceptedSlot);stage.ChargeHeapVisit(8);
        if(!ReferenceEquals(stage.Descriptor,this)||Capacity!=124||AllocatedCapacity!=250||resizing||mutating)
            throw new InvalidOperationException("Exact live retained250/logical124 generic language descriptor required.");
        if(candidateCost>=0x4000000)return false;
        if(Count==Capacity&&ReadCost(0)<=candidateCost)return false;
        if(Count<Capacity&&Count>=stage.OriginalCountT)throw new InvalidDataException("Accepted append exceeds original native initialized prefix T.");
        mutating=true;
        try
        {
            int target;
            if(Count==Capacity)
            {
                target=0;
                while(target*2<Count-1)
                {
                    token.ThrowIfCancellationRequested();stage.ChargeHeapVisit(8);
                    int child=target*2+1;
                    if(child<Count-1&&ReadCost(child)<=ReadCost(child+1))child++;
                    if(ReadCost(child)<=candidateCost)break;
                    stage.ChargeHeapCopy(128);CopySlot(child,target);target=child;
                }
            }
            else
            {
                stage.ChargeHeapCopy(4);target=Count++;
                while(target!=0)
                {
                    token.ThrowIfCancellationRequested();stage.ChargeHeapVisit(4);int parent=(target-1)/2;
                    if(candidateCost<=ReadCost(parent))break;
                    stage.ChargeHeapCopy(128);CopySlot(parent,target);target=parent;
                }
            }
            // Actual destination is selected after all full128-byte hole moves.
            // Bytes78..7F belong to this post-move slot and are never cleared.
            var slot=backing.AsSpan(target*128,128);stage.ChargeHeapCopy(120);slot[..120].Clear();
            stage.ChargeHeapCopy(4);BinaryPrimitives.WriteInt32LittleEndian(slot[16..],candidateCost);
            stage.Require();token.ThrowIfCancellationRequested();initializeAcceptedSlot(slot);
            stage.ChargeHeapVisit(1);if(Cost(slot)!=candidateCost)throw new InvalidDataException("Accepted generic FIRST writer changed its selected candidate cost.");
            token.ThrowIfCancellationRequested();stage.Require();return true;
        }
        finally{mutating=false;}
        // An accepted callback fault intentionally leaves partial private state.
        // The entered owner/query is poisoned; no rollback or retry is admitted.
    }

    internal int StampGraphPhysicalOrdinals(LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();stage.Require();stage.ChargeHeapVisit(4);
        if(!ReferenceEquals(stage.Descriptor,this)||Capacity!=124||AllocatedCapacity!=250||resizing||mutating)
            throw new InvalidOperationException("Exact post204290 descriptor required before ordinal stamp.");
        stage.CloseAcceptedPrefix(Count);
        // N is captured BEFORE any phase1 sort/compaction/rebuild/quota. Every
        // full128 move in later phase1 must preserve this physical provenance.
        int n=Count;
        for(int i=0;i<n;i++)
        {token.ThrowIfCancellationRequested();stage.ChargeHeapVisit(2);stage.ChargeHeapCopy(4);BinaryPrimitives.WriteInt32LittleEndian(backing.AsSpan(i*128+88),i);}
        return n;
    }
}
