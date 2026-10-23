using System.Buffers.Binary;
namespace InkRecognition;

public sealed partial class LegacyNativeWordHeapDescriptor
{
    internal readonly record struct Node0Phase1Result(int Unique,int SourceEnd,int RetainedEnd,int LogicalCapacity);

    // Exclusive fresh-FIRST phase1. All initialized heap masks are patternB;
    // these full128 moves therefore transport values while preserving that
    // same mask. No full250 mask array is fabricated or silently allocated.
    internal Node0Phase1Result CompleteNode0Phase1(LegacyEnglishCaseNode0Completion.Stage stage,
        LegacyEnglishCaseNode0Ring ring,LegacyEnglishCaseNode0Ledger ledger,int[] ordinals,CancellationToken token)
    {
        stage.Require();token.ThrowIfCancellationRequested();
        int n=stage.InitializedCountN;
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,16);
        if(!ReferenceEquals(stage.Descriptor,this)||Capacity!=124||AllocatedCapacity!=250||Count!=n||resizing||mutating)
            throw new InvalidOperationException("Exact sealed initialized124/retained250 phase1 descriptor required.");
        stage.BeginPhase1(ring,ledger,ordinals);
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
        Node0Sort(stage,ledger,ring,ordinals,n,false,pivot,temporary,partitions,token);
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
            token.ThrowIfCancellationRequested();stage.Require();
            //203DE0 captures the NEXT LIVE source before this allocation's
            // hole moves/clear can alias it. Never replace with one input clone.
            ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Rebuild,136);
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Phase1Rebuild,128);Node0Slot(index,ledger).CopyTo(temporary);
            if(!Node0AllocateSnapshot(stage,temporary,ledger,LegacyEnglishCaseNode0Ledger.Site.Phase1Rebuild,token))
                throw new InvalidDataException("Finite unique rebuild append unexpectedly rejected.");
        }
        if(Count>31)
        {
            int oldCount=Count;ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Phase1Quota,8);Capacity=31;Count=31;
            for(int index=31;index<oldCount;index++)
            {
                token.ThrowIfCancellationRequested();stage.Require();
                ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Quota,136);
                ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Phase1Quota,128);Node0Slot(index,ledger).CopyTo(temporary);
                _=Node0AllocateSnapshot(stage,temporary,ledger,LegacyEnglishCaseNode0Ledger.Site.Phase1Quota,token);
            }
        }
        int retained=Count;var store=ring.CurrentRows;var masks=ring.CurrentRowMask.AsSpan();
        for(int i=0;i<retained;i++)
        {
            token.ThrowIfCancellationRequested();stage.Require();ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,2048);
            int at=(125+i)*88;var source=Node0Slot(i,ledger);
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,176);source[..88].CopyTo(store[at..]);
            for(int j=0;j<88;j++)masks[at+j]=j is >=65 and <=67 or >=76 and <=79?(byte)0:(byte)255;
            if(BinaryPrimitives.ReadUInt16LittleEndian(store[(at+60)..])==ushort.MaxValue)
            {ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,2);BinaryPrimitives.WriteUInt16LittleEndian(store[(at+60)..],checked((ushort)(125+i)));}
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OrdinalVectors,4);ordinals[125+i]=BinaryPrimitives.ReadInt32LittleEndian(source[88..]);
        }
        Node0Sort(stage,ledger,ring,ordinals,retained,true,pivot,temporary,partitions,token);
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
        token.ThrowIfCancellationRequested();stage.Require();
        return new(unique,125+retained,125+final,Capacity);
    }

    private Span<byte> Node0Slot(int index,LegacyEnglishCaseNode0Ledger ledger)
    {ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Sort,8);return backing.AsSpan(index*128,128);}
    private static int Node0CompareHeap(ReadOnlySpan<byte> a,ReadOnlySpan<byte> b,LegacyEnglishCaseNode0Ledger ledger)
    {
        // Independently expanded .NET10.0.12 Release/little-endian source
        // closure; Slice/BinaryPrimitives/CompareTo are not blanket1 leaves.
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Sort,698);
        int d=BinaryPrimitives.ReadUInt64LittleEndian(a[40..]).CompareTo(BinaryPrimitives.ReadUInt64LittleEndian(b[40..]));if(d!=0)return d;
        d=(a[33]&63).CompareTo(b[33]&63);if(d!=0)return d;
        d=BinaryPrimitives.ReadUInt16LittleEndian(a[36..]).CompareTo(BinaryPrimitives.ReadUInt16LittleEndian(b[36..]));if(d!=0)return d;
        d=BinaryPrimitives.ReadUInt32LittleEndian(a[24..]).CompareTo(BinaryPrimitives.ReadUInt32LittleEndian(b[24..]));if(d!=0)return d;
        d=BinaryPrimitives.ReadUInt16LittleEndian(a[28..]).CompareTo(BinaryPrimitives.ReadUInt16LittleEndian(b[28..]));if(d!=0)return d;
        d=(a[33]>>7).CompareTo(b[33]>>7);if(d!=0)return d;
        d=BinaryPrimitives.ReadInt32LittleEndian(a[104..]).CompareTo(BinaryPrimitives.ReadInt32LittleEndian(b[104..]));if(d!=0)return d;
        d=BinaryPrimitives.ReadInt32LittleEndian(a[72..]).CompareTo(BinaryPrimitives.ReadInt32LittleEndian(b[72..]));if(d!=0)return d;
        d=BinaryPrimitives.ReadInt32LittleEndian(a[68..]).CompareTo(BinaryPrimitives.ReadInt32LittleEndian(b[68..]));if(d!=0)return d;
        return BinaryPrimitives.ReadInt32LittleEndian(a[80..]).CompareTo(BinaryPrimitives.ReadInt32LittleEndian(b[80..]));
    }
    private static int Node0CompareStored(ReadOnlySpan<byte> a,ReadOnlySpan<byte> b,LegacyEnglishCaseNode0Ledger ledger)
    {
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Sort,396);
        int d=BinaryPrimitives.ReadUInt64LittleEndian(a[40..]).CompareTo(BinaryPrimitives.ReadUInt64LittleEndian(b[40..]));if(d!=0)return d;
        d=(a[33]&63).CompareTo(b[33]&63);if(d!=0)return d;
        d=(a[39]&1).CompareTo(b[39]&1);if(d!=0)return d;
        d=BinaryPrimitives.ReadUInt16LittleEndian(a[36..]).CompareTo(BinaryPrimitives.ReadUInt16LittleEndian(b[36..]));if(d!=0)return d;
        d=BinaryPrimitives.ReadUInt32LittleEndian(a[24..]).CompareTo(BinaryPrimitives.ReadUInt32LittleEndian(b[24..]));if(d!=0)return d;
        d=BinaryPrimitives.ReadUInt16LittleEndian(a[28..]).CompareTo(BinaryPrimitives.ReadUInt16LittleEndian(b[28..]));if(d!=0)return d;
        return(a[33]>>7).CompareTo(b[33]>>7);
    }
    private void Node0SwapHeap(int a,int b,byte[] temporary,LegacyEnglishCaseNode0Ledger ledger)
    {
        if(a==b)return;ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Sort,416);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Phase1Sort,384);
        Node0Slot(a,ledger).CopyTo(temporary);CopySlot(b,a);temporary.CopyTo(Node0Slot(b,ledger));
    }
    private static void Node0SwapStored(Span<byte> store,int[] ordinals,int a,int b,byte[] temporary,LegacyEnglishCaseNode0Ledger ledger)
    {
        if(a==b)return;ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Sort,304);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Phase1Sort,264);
        var x=store.Slice((125+a)*88,88);var y=store.Slice((125+b)*88,88);
        x.CopyTo(temporary);y.CopyTo(x);temporary.AsSpan(0,88).CopyTo(y);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OrdinalVectors,8);
        int value=ordinals[125+a];ordinals[125+a]=ordinals[125+b];ordinals[125+b]=value;
        //Every secondary mask is identical patternB88; its abstract swap is
        //an identity, while all VALUE and ordinal swaps (incl suffix) are real.
    }
    private void Node0Sort(LegacyEnglishCaseNode0Completion.Stage stage,LegacyEnglishCaseNode0Ledger ledger,
        LegacyEnglishCaseNode0Ring ring,int[] ordinals,int count,bool stored,byte[] pivot,byte[] temporary,int[] partitions,CancellationToken token)
    {
        if(count<2)return;int top=0;
        Push(0,count-1);
        while(top!=0)
        {
            token.ThrowIfCancellationRequested();stage.Require();ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Sort,24);
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
    private bool Node0AllocateSnapshot(LegacyEnglishCaseNode0Completion.Stage stage,byte[] snapshot,
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
