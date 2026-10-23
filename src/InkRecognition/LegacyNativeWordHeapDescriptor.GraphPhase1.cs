using System.Buffers.Binary;
namespace InkRecognition;

// SAME-live generic FIRST-only numeric continuation. All OLD kernels remain
// unchanged. Uniform-mask reuse requires the privately sealed canonical writer
// theorem; later-history writers explicitly refuse until their masks are closed.
public sealed partial class LegacyNativeWordHeapDescriptor
{
    internal Node0Phase1Result CompleteGraphFirstPhase1(LegacyEnglishCaseNode0Completion.GraphLanguageScope stage,int[] ordinals,CancellationToken token)
    {
        stage.Require(token);stage.RequireFirstOnlyPattern();
        if(!ReferenceEquals(stage.Descriptor,this)||stage.Ring.CurrentNode!=stage.CurrentNode)
            throw new InvalidOperationException("Exact current live generic phase1 descriptor/ring required.");
        stage.ClaimPhase1();
        // One instance method-group checkpoint; logical delegate fields only,
        // not CLR header/padding/RSS or whole-owner allocation certification.
        stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,48);
        stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,48);
        return CompleteGraphFirstPhase1Core(stage.Ring,stage.Ledger,ordinals,stage.InitializedCountN,stage.RequirePhase1,token);
    }

    // Pure numeric prepared-data entry only: no GraphLanguageScope/lease or
    // owner is issued/returned. A graph runner never calls this overload; its
    // SAME-live descriptor identity gate remains above. Existing public numeric
    // kernels likewise accept explicit synthetic arrays without source authority.
    internal Node0Phase1Result CompletePreparedGraphFirstPhase1(LegacyEnglishCaseNode0Ring ring,
        LegacyEnglishCaseNode0Ledger ledger,int[] ordinals,int n,CancellationToken token)
        =>CompleteGraphFirstPhase1Core(ring,ledger,ordinals,n,PreparedNumericCheckpoint,token);
    private static void PreparedNumericCheckpoint() { }

    private Node0Phase1Result CompleteGraphFirstPhase1Core(LegacyEnglishCaseNode0Ring ring,
        LegacyEnglishCaseNode0Ledger ledger,int[] ordinals,int n,Action requirePhase1,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();requirePhase1();
        if(ordinals.Length!=156||(uint)n>124||Capacity!=124||AllocatedCapacity!=250||Count!=n||resizing||mutating)
            throw new InvalidOperationException("Sealed finite124/retained250 phase1 and156 ordinal storage required.");
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,16);
        var pivot=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,128);
        var temporary=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,128);
        var partitions=ledger.Ints(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,248);
        for(int i=0;i<n;i++)
        {
            token.ThrowIfCancellationRequested();ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Sort,64);
            var row=Node0Slot(i,ledger);
            // Count is BYTE64:65..67 are opaque and must never enter a DWORD
            // count read. Known history bounds make signed native differences
            // equivalent to ordinary three-way comparison without overflow.
            int category=BinaryPrimitives.ReadInt32LittleEndian(row[68..]);
            int currentClass=BinaryPrimitives.ReadInt32LittleEndian(row[72..]);
            if(row[4]!=1||row[64]!=2||row[32]!=255||Cost(row)>=0x4000000||
                BinaryPrimitives.ReadUInt64LittleEndian(row[40..])!=(8UL<<40)||
                BinaryPrimitives.ReadInt32LittleEndian(row[104..])!=-1||
                (uint)category>(12U<<24)||(category&0xffffff)!=0||(uint)currentClass>=1024||
                BinaryPrimitives.ReadInt32LittleEndian(row[80..])!=-1)
                throw new NotSupportedException("Exact known count2 fresh FIRST history domain required.");
        }
        GraphFirstSort(requirePhase1,ledger,ring,ordinals,n,false,pivot,temporary,partitions,token);
        int unique=0;
        for(int index=0;index<n;index++)
        {
            token.ThrowIfCancellationRequested();ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Sort,16);
            if(unique!=0&&Node0CompareHeap(Node0Slot(unique-1,ledger),Node0Slot(index,ledger),ledger)==0)
            {
                ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Sort,16);
                if(ReadCost(index)<ReadCost(unique-1))Node0SwapHeap(unique-1,index,temporary,ledger);
            }
            else
            {
                if(unique!=index)
                {
                    ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Sort,136);
                    ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Phase1Sort,128);CopySlot(index,unique);
                }
                unique++;
            }
        }
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Phase1Rebuild,4);Count=0;
        for(int index=0;index<unique;index++)
        {
            token.ThrowIfCancellationRequested();requirePhase1();
            //203DE0 captures the NEXT LIVE source before this allocation's
            // hole moves/clear can alias it. Never replace with one input clone.
            ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Rebuild,136);
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Phase1Rebuild,128);Node0Slot(index,ledger).CopyTo(temporary);
            if(!GraphFirstAllocateSnapshot(temporary,ledger,LegacyEnglishCaseNode0Ledger.Site.Phase1Rebuild,token))
                throw new InvalidDataException("Finite unique rebuild append unexpectedly rejected.");
        }
        if(Count>31)
        {
            int oldCount=Count;ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Phase1Quota,8);Capacity=31;Count=31;
            for(int index=31;index<oldCount;index++)
            {
                token.ThrowIfCancellationRequested();requirePhase1();
                ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Quota,136);
                ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Phase1Quota,128);Node0Slot(index,ledger).CopyTo(temporary);
                _=GraphFirstAllocateSnapshot(temporary,ledger,LegacyEnglishCaseNode0Ledger.Site.Phase1Quota,token);
            }
        }
        int retained=Count;var store=ring.CurrentRows;var masks=ring.CurrentRowMask.AsSpan();
        for(int i=0;i<retained;i++)
        {
            token.ThrowIfCancellationRequested();requirePhase1();ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,2048);
            int at=(125+i)*88;var source=Node0Slot(i,ledger);
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,176);source[..88].CopyTo(store[at..]);
            for(int j=0;j<88;j++)masks[at+j]=j is >=65 and <=67 or >=76 and <=79?(byte)0:(byte)255;
            if(BinaryPrimitives.ReadUInt16LittleEndian(store[(at+60)..])==ushort.MaxValue)
            {ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,2);BinaryPrimitives.WriteUInt16LittleEndian(store[(at+60)..],checked((ushort)(125+i)));}
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OrdinalVectors,4);ordinals[125+i]=BinaryPrimitives.ReadInt32LittleEndian(source[88..]);
        }
        GraphFirstSort(requirePhase1,ledger,ring,ordinals,retained,true,pivot,temporary,partitions,token);
        int final=retained==0?0:1;
        for(int next=1;next<retained;next++)
        {
            token.ThrowIfCancellationRequested();ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Sort,16);
            int leader=final-1;
            if(Node0CompareStored(store.Slice((125+leader)*88,88),store.Slice((125+next)*88,88),ledger)==0)
            {
                ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Sort,16);
                if(BinaryPrimitives.ReadInt32LittleEndian(store[((125+next)*88+16)..])<
                    BinaryPrimitives.ReadInt32LittleEndian(store[((125+leader)*88+16)..]))
                    Node0SwapStored(store,ordinals,leader,next,temporary,ledger);
            }
            else{Node0SwapStored(store,ordinals,final,next,temporary,ledger);final++;}
        }
        token.ThrowIfCancellationRequested();requirePhase1();
        return new(unique,125+retained,125+final,Capacity);
    }

    private void GraphFirstSort(Action requirePhase1,LegacyEnglishCaseNode0Ledger ledger,
        LegacyEnglishCaseNode0Ring ring,int[] ordinals,int count,bool stored,byte[] pivot,byte[] temporary,int[] partitions,CancellationToken token)
    {
        if(count<2)return;int top=0;
        Push(0,count-1);
        while(top!=0)
        {
            token.ThrowIfCancellationRequested();requirePhase1();ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Sort,24);
            int upper=partitions[--top],lower=partitions[--top],left=lower,right=upper;
            int middle=lower+((upper-lower)>>1),bytes=stored?88:128;
            ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Sort,bytes+8);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Phase1Sort,bytes);
            if(stored)ring.CurrentRows.Slice((125+middle)*88,88).CopyTo(pivot);else Node0Slot(middle,ledger).CopyTo(pivot);
            do
            {
                while(Compare(left)<0)if(++left>upper)throw new InvalidDataException("Native partition left scan escaped its admitted total-order pivot.");
                while(Compare(right)>0)if(--right<lower)throw new InvalidDataException("Native partition right scan escaped its admitted total-order pivot.");
                if(left>right)break;
                if(stored)Node0SwapStored(ring.CurrentRows,ordinals,left,right,temporary,ledger);else Node0SwapHeap(left,right,temporary,ledger);
                left++;right--;
            }while(left<=right);
            // Native recursion visits left partition first. Fixed LIFO pushes
            //right before left; no Stack<T> growth or recursive managed stack.
            if(left<upper)Push(left,upper);if(lower<right)Push(lower,right);
        }
        void Push(int lower,int upper)
        {
            ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Sort,8);
            if(top>partitions.Length-2)throw new InvalidDataException("Fixed248-int native partition workspace exhausted.");
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Phase1Sort,8);partitions[top++]=lower;partitions[top++]=upper;
        }
        int Compare(int index)
        {
            ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Sort,8);
            return stored?Node0CompareStored(ring.CurrentRows.Slice((125+index)*88,88),pivot.AsSpan(0,88),ledger):Node0CompareHeap(Node0Slot(index,ledger),pivot,ledger);
        }
    }

    private bool GraphFirstAllocateSnapshot(byte[] snapshot,
        LegacyEnglishCaseNode0Ledger ledger,LegacyEnglishCaseNode0Ledger.Site site,CancellationToken token)
    {
        ledger.Visit(site,32);
        if(snapshot.Length!=128)throw new InvalidDataException("Complete private128-byte source snapshot required.");
        int cost=Cost(snapshot);
        if(Capacity is not (31 or 124)||Count>Capacity||cost>=0x4000000||mutating||resizing)
            throw new InvalidDataException("Private full128 live snapshot and phase1 descriptor required.");
        if(Count==Capacity&&ReadCost(0)<=cost)return false;
        mutating=true;
        try
        {
            int target;
            if(Count==Capacity)
            {
                target=0;
                while(target*2<Count-1)
                {
                    token.ThrowIfCancellationRequested();ledger.Visit(site,32);int child=target*2+1;
                    if(child<Count-1&&ReadCost(child)<=ReadCost(child+1))child++;
                    if(ReadCost(child)<=cost)break;
                    ledger.Visit(site,136);ledger.Copy(site,128);CopySlot(child,target);target=child;
                }
            }
            else
            {
                ledger.Copy(site,4);target=Count++;
                while(target!=0)
                {
                    token.ThrowIfCancellationRequested();ledger.Visit(site,24);int parent=(target-1)/2;
                    if(cost<=ReadCost(parent))break;
                    ledger.Visit(site,136);ledger.Copy(site,128);CopySlot(parent,target);target=parent;
                }
            }
            // Literal transient stores remain charged even though snapshot
            //copy immediately overwrites them. No callback or native NULL ABI.
            ledger.Copy(site,252);var destination=Node0Slot(target,ledger);destination[..120].Clear();
            BinaryPrimitives.WriteInt32LittleEndian(destination[16..],cost);snapshot.CopyTo(destination);
            token.ThrowIfCancellationRequested();return true;
        }
        finally{mutating=false;}
    }
}
