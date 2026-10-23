using System.Buffers.Binary;

namespace InkRecognition;

// SOURCE ONLY; no compilation or execution approved. All constructor paths
// remain closed. This file cannot receive prepared/caller buffers or an owner.
internal sealed partial class LegacyFst28CNumberTrueLanguageSourceDraft
{
    private const int WriterSliceBytes=209752,WriterSliceWork=8357000;

    private NumericContinuation CreateNumericContinuation(CancellationToken token)
    {
        RequireWholeAdmission(token); // Still unconditionally throws in receiver.
        return Phase0State.Issue(this,token);
    }

    // The private phase0 issuer moves its SAME actual heap/pool/materialized
    // arrays here. No external prepared arrays or historical owner can enter.
    private sealed partial class NumericContinuation
    {
        private const int H=250,L=125,DescriptorCapacity=124,IMP=0x4000000;
        private readonly LegacyFst28CNumberTrueLanguageSourceDraft receiver;
        private readonly Phase0State phase0;
        private readonly byte[] heap,heapMask,secondary,secondaryMask;
        private readonly byte[] parents,parentMasks,parentSecondary,parentSecondaryMasks;
        private int count;
        private readonly ParentReadScope parentReadScope;
        // Scope ONLY original materialized parent reads before ordinal/phase1.
        // Object and arrays never leave this private NumericContinuation.
        private sealed class ParentReadScope
        {
            private readonly NumericContinuation issuer;
            private readonly int thread;
            private int closed;
            internal ParentReadScope(NumericContinuation issuer)
            {this.issuer=issuer;thread=Environment.CurrentManagedThreadId;}
            internal void Require(NumericContinuation actual)
            {
                if(!ReferenceEquals(issuer,actual)||thread!=Environment.CurrentManagedThreadId||
                    Volatile.Read(ref closed)!=0||Volatile.Read(ref actual.finalization)!=0)
                    throw new InvalidOperationException("Original private parent-read scope is foreign, closed or cross-thread.");
            }
            internal void Close()=>Volatile.Write(ref closed,1);
        }
        internal NumericContinuation(LegacyFst28CNumberTrueLanguageSourceDraft receiver,Phase0State phase0)
        {
            if(!ReferenceEquals(phase0.receiver,receiver))throw new ArgumentException("Foreign private phase0 issuer.");
            this.receiver=receiver;this.phase0=phase0;
            heap=phase0.heap;heapMask=phase0.heapMask;
            secondary=phase0.secondaryPool;secondaryMask=phase0.secondaryPoolMask;
            parents=phase0.stored;parentMasks=phase0.storedMask;
            parentSecondary=phase0.secondary;parentSecondaryMasks=phase0.secondaryMask;
            count=phase0.descriptorCount;parentReadScope=new(this);
        }
        private static int I32(byte[] a,int o)=>BinaryPrimitives.ReadInt32LittleEndian(a.AsSpan(o,4));
        private static ushort U16(byte[] a,int o)=>BinaryPrimitives.ReadUInt16LittleEndian(a.AsSpan(o,2));
        private static ulong U64(byte[] a,int o)=>BinaryPrimitives.ReadUInt64LittleEndian(a.AsSpan(o,8));
        private static void Known(byte[] mask,int o,int n)
        {for(int i=0;i<n;i++)if(mask[o+i]!=255)throw new NotSupportedException("Continuation operand is not fully defined.");}
        private static void W(byte[] a,byte[] m,int o,int value)
        {BinaryPrimitives.WriteInt32LittleEndian(a.AsSpan(o,4),value);m.AsSpan(o,4).Fill(255);}
        private static void W16(byte[] a,byte[] m,int o,ushort value)
        {BinaryPrimitives.WriteUInt16LittleEndian(a.AsSpan(o,2),value);m.AsSpan(o,2).Fill(255);}
        private static void W64(byte[] a,byte[] m,int o,ulong value)
        {BinaryPrimitives.WriteUInt64LittleEndian(a.AsSpan(o,8),value);m.AsSpan(o,8).Fill(255);}
        private void Validate(CancellationToken token)
        {
            receiver.RequireWholeAdmission(token);
            if(receiver.pool is null||receiver.source is null||receiver.consumer is null||receiver.page is null||
                Volatile.Read(ref receiver.stage)!=4||!receiver.consumer.ReadSkippedLanguageCompletionClaim())
                throw new InvalidOperationException("Exact already-entered private receiving state required.");
            receiver.source.ValidateLiveLease(receiver,receiver.page,receiver.pool,token);
            if(!ReferenceEquals(phase0.receiver,receiver)||!ReferenceEquals(heap,phase0.heap)||
                !ReferenceEquals(heapMask,phase0.heapMask)||!ReferenceEquals(secondary,phase0.secondaryPool)||
                !ReferenceEquals(parents,phase0.stored)||phase0.descriptorCount!=0||phase0.descriptorCapacity!=124||
                heap.Length!=H*128||heapMask.Length!=heap.Length||secondary.Length!=H*80||
                secondaryMask.Length!=secondary.Length||parents.Length!=157*88||parentMasks.Length!=parents.Length||
                parentSecondary.Length!=157*80||parentSecondaryMasks.Length!=parentSecondary.Length||count<0||count>DescriptorCapacity)
                throw new InvalidDataException("Private fixed continuation storage differs.");
        }
        private void ValidateReadOnlyParent(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();parentReadScope.Require(this);
            receiver.RequireWholeAdmission(token);
            if(receiver.pool is null||receiver.source is null||receiver.consumer is null||receiver.page is null||
                Volatile.Read(ref receiver.stage)!=4||!receiver.consumer.ReadSkippedLanguageCompletionClaim())
                throw new InvalidOperationException("Exact already-entered private receiving state required.");
            receiver.source.ValidateOriginalParentReadLease(receiver,receiver.page,receiver.pool,token);
            if(!ReferenceEquals(phase0.receiver,receiver)||!ReferenceEquals(heap,phase0.heap)||
                !ReferenceEquals(heapMask,phase0.heapMask)||!ReferenceEquals(secondary,phase0.secondaryPool)||
                !ReferenceEquals(parents,phase0.stored)||phase0.descriptorCount!=0||phase0.descriptorCapacity!=124||
                heap.Length!=H*128||heapMask.Length!=heap.Length||secondary.Length!=H*80||
                secondaryMask.Length!=secondary.Length||parents.Length!=157*88||parentMasks.Length!=parents.Length||
                parentSecondary.Length!=157*80||parentSecondaryMasks.Length!=parentSecondary.Length||count<0||count>DescriptorCapacity)
                throw new InvalidDataException("Private fixed continuation storage differs.");
        }
        private void MoveHeap(int from,int to)
        {
            heap.AsSpan(from*128,128).CopyTo(heap.AsSpan(to*128));
            heapMask.AsSpan(from*128,128).CopyTo(heapMask.AsSpan(to*128));
        }
        // 1F75B0 one primary descriptor, EC0/headerbucket0; returns -1 for
        // normal threshold rejection. No allocation/refusal-as-success receipt.
        private int Allocate(int cost,CancellationToken token)
        {
            Validate(token);if(cost>=IMP)return -1;
            int at;
            if(count==DescriptorCapacity)
            {
                Known(heapMask,16,4);if(I32(heap,16)<=cost)return -1;at=0;
                while(at*2<count-1)
                {
                    token.ThrowIfCancellationRequested();int child=at*2+1;
                    Known(heapMask,child*128+16,4);
                    if(child<count-1)
                    {Known(heapMask,(child+1)*128+16,4);if(I32(heap,child*128+16)<=I32(heap,(child+1)*128+16))child++;}
                    if(I32(heap,child*128+16)<=cost)break;MoveHeap(child,at);at=child;
                }
            }
            else
            {
                at=count++;
                while(at!=0)
                {token.ThrowIfCancellationRequested();int parent=(at-1)/2;Known(heapMask,parent*128+16,4);
                 if(cost<=I32(heap,parent*128+16))break;MoveHeap(parent,at);at=parent;}
            }
            heap.AsSpan(at*128,120).Clear();heapMask.AsSpan(at*128,120).Fill(255);
            // Literal allocator transient store occurs BEFORE parent88 copy.
            // The writer subsequently overwrites it, but prefix chronology and
            // mask/write accounting must preserve this eight-visit operation.
            W(heap,heapMask,at*128+16,cost);
            return at;
        }
        internal bool TryReadActualParent(int ordinal,out ulong tag,out int cost,CancellationToken token)
        {
            ValidateReadOnlyParent(token);if((uint)ordinal>=L)throw new ArgumentOutOfRangeException(nameof(ordinal));
            int at=ordinal*88;Known(parentMasks,at+16,4);cost=I32(parents,at+16);tag=0;
            if(cost>=IMP)return false;
            Known(parentMasks,at,88);
            if(parents[at+4]!=1||parents[at+0x20]!=0||I32(parents,at+0x38)!=0||
                U16(parents,at+0x3c)!=ordinal||U16(parents,at+0x3e)!=65535)
                throw new NotSupportedException("Actual finite materialized node0 parent is required.");
            tag=U64(parents,at);uint state=(uint)tag;
            if(!((state>=41&&state<=50)||state is 69 or 70)||((tag>>32)&255)!=1)
                throw new NotSupportedException("Actual number primary target state/source namespace differs.");
            return true;
        }
        internal bool TryReadActualParentForReceiver(LegacyFst28CNumberTrueLanguageSourceDraft expected,
            int ordinal,out ulong tag,out int cost,CancellationToken token)
        {
            if(!ReferenceEquals(receiver,expected))throw new ArgumentException("Foreign private phase0 state.");
            return TryReadActualParent(ordinal,out tag,out cost,token);
        }
        internal void ValidateParentReceiver(LegacyFst28CNumberTrueLanguageSourceDraft expected,CancellationToken token)
        {if(!ReferenceEquals(receiver,expected))throw new ArgumentException("Foreign private phase0 state.");Validate(token);}
        internal bool WriteActualCurrentTerminal(int ordinal,CancellationToken token)=>WriteCurrentTerminal(ordinal,token);
        // Called only by future private same-source AUX/current-terminal loop.
        // Arguments are scalars derived inside that loop, never a public request.
        private bool WriteCurrentTerminal(int parent,CancellationToken token)
        {
            Validate(token);
            if((uint)parent>=L)
                throw new ArgumentOutOfRangeException(nameof(parent));
            int p=parent*88;Known(parentMasks,p,88);
            // Actual current-terminal injection copies this same materialized
            // parent's tag, uses query cost0 and genuine fixed13A.V50 restart.
            // No scalar request/cost ingress exists, even privately.
            ulong queryTag=U64(parents,p),restart=8UL<<40;int candidate=I32(parents,p+16);
            if(((queryTag>>32)&255)!=1||((queryTag>>40)&2)==0||((queryTag>>56)&1)!=0)
                throw new NotSupportedException("Exact singleton current-terminal parent tag required.");
            if(parents[p+0x20]!=0||I32(parents,p+0x38)!=0||U16(parents,p+0x3c)!=parent||
                U16(parents,p+0x3e)!=65535||parents[p+4]!=1)
                throw new NotSupportedException("Actual materialized node0/header1 parent required.");
            int increment=I32(parents,p+0x14),slot=Allocate(candidate,token);if(slot<0)return false;
            int at=slot*128;
            parents.AsSpan(p,88).CopyTo(heap.AsSpan(at));parentMasks.AsSpan(p,88).CopyTo(heapMask.AsSpan(at));
            heap[at+0x21]&=192;heapMask[at+0x21]|=63;heap[at+0x27]&=254;heapMask[at+0x27]|=1;
            W64(heap,heapMask,at,queryTag);W(heap,heapMask,at+0x14,increment);
            W16(heap,heapMask,at+0x3e,65535);W(heap,heapMask,at+0x10,candidate);
            W16(heap,heapMask,at+0x1e,(ushort)parent);heap[at+0x20]=255;heapMask[at+0x20]=255;
            heap.AsSpan(at+0x40,16).Clear();heapMask.AsSpan(at+0x40,20).Fill(255);W(heap,heapMask,at+0x50,-1);
            W64(heap,heapMask,at+8,0);W64(heap,heapMask,at+0x28,restart);
            W(heap,heapMask,at+0x58,-1);W64(heap,heapMask,at+0x5c,0x04000000FFFFFFFFUL);
            W(heap,heapMask,at+0x68,-1);W64(heap,heapMask,at+0x70,0);
            return true;
        }
        internal void AssignActualContinuationOrdinals(CancellationToken token)
        {
            Validate(token);parentReadScope.Close();
            for(int i=0;i<count;i++){token.ThrowIfCancellationRequested();W(heap,heapMask,i*128+0x58,i);}
        }
        internal void ProduceActualContinuationAlternatives(CancellationToken token)
        {Validate(token);for(int i=0;i<count;i++)ProduceAlternatives(i,token);}
        // Writer initializes58=-1. Future orchestration MUST stamp all active
        // ordinals after the complete writer loop, before invoking this method.
        // No such issuance/stamping stage exists in this closed source draft.
        private void ProduceAlternatives(int slot,CancellationToken token)
        {
            Validate(token);if((uint)slot>=(uint)count)throw new ArgumentOutOfRangeException(nameof(slot));
            int at=slot*128;Known(heapMask,at+0x1e,2);Known(heapMask,at+0x3e,2);Known(heapMask,at+8,8);
            Known(heapMask,at+0x58,4);Known(heapMask,at+0x21,1);
            int parent=U16(heap,at+0x1e),ordinal=I32(heap,at+0x58);
            if(parent>=L||(uint)ordinal>=H||U16(heap,at+0x3e)!=65535||U64(heap,at+8)!=0)
                throw new NotSupportedException("Only actual unanchored zero-suffix continuation is closed.");
            int p=parent*88;Known(parentMasks,p+16,8);Known(heapMask,at+16,8);
            for(int j=0;j<2;j++)
            {
                token.ThrowIfCancellationRequested();int src=parent*80+j*40,dst=ordinal*80+j*40;
                Known(parentSecondaryMasks,src,4);
                if(I32(parentSecondary,src)>=IMP)
                {for(int tail=j;tail<2;tail++)W(secondary,secondaryMask,ordinal*80+tail*40,IMP);break;}
                Known(parentSecondaryMasks,src+4,4);
                W(secondary,secondaryMask,dst,unchecked(I32(heap,at+16)-I32(parents,p+16)+I32(parentSecondary,src)));
                W(secondary,secondaryMask,dst+4,unchecked(I32(parentSecondary,src+4)-I32(parents,p+20)+I32(heap,at+20)));
                parentSecondary.AsSpan(src+8,16).CopyTo(secondary.AsSpan(dst+8));
                parentSecondaryMasks.AsSpan(src+8,16).CopyTo(secondaryMask.AsSpan(dst+8));
                W16(secondary,secondaryMask,dst+24,(ushort)parent);
                secondary[dst+26]=255;secondaryMask[dst+26]=255;secondary[dst+27]=(byte)j;secondaryMask[dst+27]=255;
                W16(secondary,secondaryMask,dst+28,65535);secondary[dst+30]=0;secondaryMask[dst+30]=255;
                secondary[dst+31]=(byte)((secondary[dst+31]&224)|(heap[at+0x21]&31));
                secondaryMask[dst+31]=(byte)((secondaryMask[dst+31]&224)|(heapMask[at+0x21]&31));
                W16(secondary,secondaryMask,dst+32,65535);secondary[dst+34]=255;secondaryMask[dst+34]=255;
                // Bytes35..39 and high3 bits31 retain actual previous contents/masks.
            }
        }
    }
}
