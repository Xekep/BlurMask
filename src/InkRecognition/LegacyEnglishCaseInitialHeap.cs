namespace InkRecognition;
// One actual general descriptor: historical primary prefix only, never parent88.
internal sealed class LegacyEnglishCaseInitialHeap
{
    private readonly object gate=new();
    private readonly LegacyNativeWordRecordHeap heap;
    private readonly LegacyNativeWordHeapRouter router;
    private long attempts,accepted,moveAllowance;
    private bool transferred;
    internal LegacyEnglishCaseInitialHeap(LegacyEnglishCaseInitialSourcePolicy policy)
    {
        var h=policy.Heaps;
        if(h.Limits.Count!=1||h.Limits[0].PrimaryInitialCapacity20!=250||h.SecondaryShareEc!=0||h.CopySourceBuckets().Span.Length!=2||h.CopySourceBuckets().Span[1]!=0)throw new NotSupportedException("Actual one general primary descriptor/capacity250 required.");
        heap=new(250);router=new([heap],h.CopySourceBuckets().Span,h.Sources.OverrideSourceIndex22f0,h.Configuration.OverrideHeap2458);
    }
    internal bool Insert(LegacyInitialWordRecordFields fields,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {
            if(transferred)throw new InvalidOperationException("Primary mutation authority has been consumed.");
            if(++attempts>4194304||moveAllowance>33554432-8)throw new InvalidDataException("Whole heap attempts/hole-write budget exhausted.");
            // Native1F75B0 hole propagation: <=7 full slot copies, final hole
            // fill charged once. No promotion callback or swapped second slot.
            moveAllowance+=8;
            var allocation=router.TryInsertInitialRecord(1,false,fields,0,false,cancellationToken:token);
            if(allocation.Accepted)accepted++;return allocation.Accepted;
        }
    }
    internal (Memory<byte> Storage,int Count) TransferPrimary(LegacyEnglishCasePrimaryCompletion owner,
        LegacyEnglishCaseInitialConsumer consumer,LegacyEnglishCaseInitialSourcePolicy policy,
        LegacyEnglishFreshCaseMainSource.InitialQueryLease lease,CancellationToken token)
    {
        consumer.RequirePrimaryCompletion(owner,this,policy,lease);token.ThrowIfCancellationRequested();
        lock(gate)
        {
            consumer.RequirePrimaryCompletion(owner,this,policy,lease);token.ThrowIfCancellationRequested();
            if(transferred)throw new InvalidOperationException("One privately authenticated primary transfer required.");
            transferred=true; //Any later failure poisons the entered operation; never roll this back.
            //The public kernel returns a fresh full-capacity array. This private
            //ownership transfer unwraps that exact new array without another copy.
            return (System.Runtime.InteropServices.MemoryMarshal.AsMemory(heap.GetSlotStorageSnapshot()),heap.Count);
        }
    }
    internal LegacyEnglishCaseInitialHeapSnapshot CopyHistorical()
    {
        lock(gate)
        {
            var slots=new byte[heap.Count][];for(int i=0;i<slots.Length;i++)slots[i]=heap.GetSlotSnapshot(i).ToArray();
            var mask=new byte[128];Array.Fill(mask,(byte)1,0,0x78);
            return new(slots,mask,attempts,accepted,moveAllowance);
        }
    }
}
internal sealed class LegacyEnglishCaseInitialHeapSnapshot
{
    private readonly byte[][] slots;
    private readonly byte[] written;
    internal long Attempts{get;}
    internal long Accepted{get;}
    internal long ReservedSlotWriteEvents{get;}
    internal int Count=>slots.Length;
    internal LegacyEnglishCaseInitialHeapSnapshot(byte[][] slots,byte[] written,long attempts,long accepted,long moves)
    {this.slots=slots;this.written=written;Attempts=attempts;Accepted=accepted;ReservedSlotWriteEvents=moves;}
    internal byte[][] CopySlots()=>slots.Select(x=>x.ToArray()).ToArray();
    internal byte[] CopyWrittenMask()=>written.ToArray();
}
