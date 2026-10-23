using System.Buffers.Binary;

namespace InkRecognition;

// SOURCE ONLY. All calls remain behind the unconditional whole-admission gate.
// No caller bytes, historical completion, prepared owner, or detached snapshots
// can construct this private state. This is an issuer, not an acceptance API.
internal sealed partial class LegacyFst28CNumberTrueLanguageSourceDraft
{
    private sealed class Phase0State
    {
        private const int H=250,L=125,E=157,IMP=0x4000000;
        internal readonly LegacyFst28CNumberTrueLanguageSourceDraft receiver;
        internal int count,unique,descriptorCount,descriptorCapacity;
        internal byte[] heap=[],heapMask=[],primaryPool=[],secondaryPool=[],primaryPoolMask=[],secondaryPoolMask=[];
        internal byte[] stored=[],primary=[],secondary=[],storedMask=[],primaryMask=[],secondaryMask=[];
        private Phase0State(LegacyFst28CNumberTrueLanguageSourceDraft receiver){this.receiver=receiver;}
        internal static NumericContinuation Issue(LegacyFst28CNumberTrueLanguageSourceDraft receiver,CancellationToken token)
        {
            receiver.RequireWholeAdmission(token); // Always throws before any work.
            receiver.ValidatePhase0OriginalOwner(token);
            if(Interlocked.CompareExchange(ref receiver.phase0Issued,1,0)!=0)
                throw new InvalidOperationException("Actual private phase0 issuer is already consumed or poisoned.");
            var result=new Phase0State(receiver);
            // Exact actual private heap, handed off after the shared consumer
            // claim. The API's first32000B copy and this second32000B copy are
            // separately accounted. Historical public snapshots grant no authority.
            result.heap=receiver.heap!.GetSlotStorageSnapshot().ToArray();
            result.count=receiver.heap.Count;
            if(result.heap.Length!=H*128||result.count is <0 or >H)
                throw new InvalidDataException("Actual private250-slot root heap differs.");
            result.heapMask=new byte[H*128];
            for(int i=0;i<result.count;i++)
            {
                token.ThrowIfCancellationRequested();int at=i*128;
                // Initial consumer allocation cleared native120 bytes; the
                // remaining8 stay unknown. Ordinal is stamped before root pools.
                result.heapMask.AsSpan(at,120).Fill(255);
                if(result.heap[at+4]!=1||result.heap[at+32]!=0||I32(result.heap,at+56)!=0||
                    I32(result.heap,at+52)!=0||I32(result.heap,at+16)>=IMP)
                    throw new NotSupportedException("Actual finite source1 node0 root descriptor required.");
                uint state=(uint)U64(result.heap,at);
                if(!(state is >=41 and <=50 or 69 or 70))
                    throw new NotSupportedException("Actual Number root target state required.");
                W32(result.heap,at+88,i);result.heapMask.AsSpan(at+88,4).Fill(255);
            }
            result.FillActualRootPools(token);
            result.Sort(token);result.Reduce(token);result.RebuildAndQuota(token);result.Materialize(token);
            // Native212D80 clears descriptor count BEFORE changing capacity124.
            // No new zeroed working heap/pool replaces live phase0 residue.
            result.descriptorCount=0;result.descriptorCapacity=124;
            receiver.ValidatePhase0OriginalOwner(token);
            return new NumericContinuation(receiver,result);
        }
        private void FillActualRootPools(CancellationToken token)
        {
            primaryPool=new byte[H*80];secondaryPool=new byte[H*80];
            primaryPoolMask=new byte[H*80];secondaryPoolMask=new byte[H*80];
            var actual=receiver.context!;var contexts=receiver.pool!.OwnedContext;
            int n=actual.Count,before=contexts.NodeCount;
            // Two bounded12-entry tables, not copied graft/context containers.
            var rawIds=new ulong[12];var mappedIds=new ulong[12];
            for(int i=0;i<n;i++)
            {
                token.ThrowIfCancellationRequested();var g=actual.ReadGraft(i);var r=g.SourceRecord;
                if(!g.Created||r.Symbol>9&&r.Symbol!=138&&r.Symbol!=185||((r.Tag>>32)&255)!=1||
                    r.SourceCategory!=0||r.CaseFlags!=0||r.Extra!=0)
                    throw new InvalidDataException("Actual source1 ordinary graft required.");
                // Exactly three allocated51B Views/graft, <=36 in this method.
                var original=contexts.ReadNode(r.ContextIdentity);
                var child=contexts.ReadNode(g.GraftedContextIdentity);
                if(child.ParentLink==0)throw new InvalidDataException("Actual graft must retain its FFFA parent.");
                var root=contexts.ReadNode(child.ParentLink);
                if(original.ParentLink!=0||original.ChildTreeRoot!=0||original.Symbol!=r.Symbol||
                    child.ChildTreeRoot!=0||child.Symbol!=r.Symbol||child.ParentLink==0||
                    root.Symbol!=0xfffa||root.ParentLink!=0)
                    throw new InvalidDataException("Actual original/FFFA/grafted two-node path differs.");
                // Bounded specialization of1F7E30/1F7FF0 with prefixNULL,
                // suffixFFFA->ordinary symbol, and identity2360 mapping.
                // The mapped path drops initialFFFA; raw retains it. Every
                // resolution must find the SAME live node, never synthesize it.
                var mapped=contexts.ResolveInitial(r.Symbol,token);
                var sentinel=contexts.ResolveInitial(0xfffa,token);
                var raw=contexts.ResolveChild(sentinel.Identity,r.Symbol,token);
                if(mapped.Created||sentinel.Created||raw.Created||mapped.Identity!=r.ContextIdentity||
                    sentinel.Identity!=root.Identity||raw.Identity!=g.GraftedContextIdentity||contexts.NodeCount!=before)
                    throw new InvalidDataException("Root composition must resolve only pre-existing exact nodes.");
                for(int j=0;j<i;j++)if(rawIds[j]==raw.Identity)
                    throw new InvalidDataException("Duplicate actual graft context identity.");
                rawIds[i]=raw.Identity;mappedIds[i]=mapped.Identity;
            }
            for(int i=0;i<count;i++)
            {
                token.ThrowIfCancellationRequested();int row=i*128;ulong suffix=U64(heap,row+8);int found=-1;
                for(int j=0;j<n;j++)if(rawIds[j]==suffix){found=j;break;}
                if(found<0)throw new InvalidDataException("Root descriptor suffix is not an actual same-owner graft.");
                ulong mapped=mappedIds[found],raw=rawIds[found];
                for(int mode=0;mode<2;mode++)
                {
                    var p=mode==0?primaryPool:secondaryPool;var m=mode==0?primaryPoolMask:secondaryPoolMask;int at=i*80;
                    heap.AsSpan(row+16,8).CopyTo(p.AsSpan(at));W64(p,at+8,mapped);W64(p,at+16,raw);
                    heap.AsSpan(row+30,2).CopyTo(p.AsSpan(at+24));p[at+26]=heap[row+32];p[at+30]=heap[row+38];
                    p[at+31]=(byte)((heap[row+33]&31)|(mode==0?32:0));W32(p,at+40,IMP);W32(p,at+44,IMP);p[at+70]=0;
                    m.AsSpan(at,27).Fill(255);m[at+30]=255;m[at+31]=(byte)(mode==0?63:31);
                    m.AsSpan(at+40,8).Fill(255);m[at+70]=255;
                }
            }
        }
    private static int Compare(byte[] b,int a,int c)
    {
        int v=U64(b,a).CompareTo(U64(b,c));if(v!=0)return v;
        v=U16(b,a+36).CompareTo(U16(b,c+36));if(v!=0)return v;
        v=U32(b,a+24).CompareTo(U32(b,c+24));if(v!=0)return v;
        v=I32(b,a+48).CompareTo(I32(b,c+48));if(v!=0)return v;
        v=I32(b,a+52).CompareTo(I32(b,c+52));if(v!=0)return v;
        v=U16(b,a+28).CompareTo(U16(b,c+28));return v!=0?v:(b[a+33]>>7).CompareTo(b[c+33]>>7);
    }
    private static void Swap(byte[] bytes,int a,int b,int width)
    {if(a==b)return;Span<byte> tmp=stackalloc byte[128];bytes.AsSpan(a,width).CopyTo(tmp);bytes.AsSpan(b,width).CopyTo(bytes.AsSpan(a));tmp[..width].CopyTo(bytes.AsSpan(b));}
    private void Sort(CancellationToken token)
    {
        if(count<2)return;var pending=new int[H*2];int top=0;pending[top++]=0;pending[top++]=count-1;
        while(top!=0)
        {
            token.ThrowIfCancellationRequested();int upper=pending[--top],lower=pending[--top],left=lower,right=upper;
            var pivot=heap.AsSpan(((upper-lower)/2+lower)*128,128).ToArray();
            // Comparison uses only fully initialized native key fields. Join
            // a fixed two-slot view for the pivot without any mask pivot clone.
            int Cmp(int index)
            {var r=heap.AsSpan(index*128,128);int v=U64(r,0).CompareTo(U64(pivot,0));if(v!=0)return v;
             v=U16(r,36).CompareTo(U16(pivot,36));if(v!=0)return v;v=U32(r,24).CompareTo(U32(pivot,24));if(v!=0)return v;
             v=I32(r,48).CompareTo(I32(pivot,48));if(v!=0)return v;v=I32(r,52).CompareTo(I32(pivot,52));if(v!=0)return v;
             v=U16(r,28).CompareTo(U16(pivot,28));return v!=0?v:(r[33]>>7).CompareTo(pivot[33]>>7);}
            do
            {
                token.ThrowIfCancellationRequested();while(Cmp(left)<0)left++;while(Cmp(right)>0)right--;
                if(right<left)break;Swap(heap,left*128,right*128,128);Swap(heapMask,left*128,right*128,128);left++;right--;
            }while(left<=right);
            if(left<upper){if(top>pending.Length-2)throw new InvalidDataException("Partition bound differs.");pending[top++]=left;pending[top++]=upper;}
            if(lower<right){if(top>pending.Length-2)throw new InvalidDataException("Partition bound differs.");pending[top++]=lower;pending[top++]=right;}
        }
    }
    private readonly record struct Pick(int Row,int Index,ulong Identity);
    private void Merge(int leader,int duplicate,bool primary,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var p=primary?primaryPool:secondaryPool;var m=primary?primaryPoolMask:secondaryPoolMask;
        int a=I32(heap,leader+88),b=I32(heap,duplicate+88);
        if(!primary&&I32(p,b*80)<I32(p,a*80))
        {Swap(heap,leader,duplicate,128);Swap(heapMask,leader,duplicate,128);(a,b)=(b,a);}
        bool heapCosts=primary&&(p[a*80+31]&p[b*80+31]&32)!=0;
        bool firstA=!primary||(heapCosts?I32(heap,leader+16):I32(p,a*80))<(heapCosts?I32(heap,duplicate+16):I32(p,b*80));
        Span<Pick> chosen=stackalloc Pick[2];chosen[0]=new(firstA?a:b,0,U64(p,(firstA?a:b)*80+8));
        int n=1,ai=firstA?1:0,bi=firstA?0:1;
        while(n<2)
        {
            bool av=ai<2&&I32(p,a*80+ai*40)<IMP,bv=bi<2&&I32(p,b*80+bi*40)<IMP;if(!av&&!bv)break;
            bool left=av&&(!bv||I32(p,a*80+ai*40)<I32(p,b*80+bi*40));int row=left?a:b,index=left?ai++:bi++;
            ulong id=U64(p,row*80+index*40+8);if(id!=chosen[0].Identity)chosen[n++]=new(row,index,id);
        }
        for(int i=n-1;i>=0;i--)
        {
            var pick=chosen[i];if(pick.Row==a&&pick.Index==i)continue;
            int src=pick.Row*80+pick.Index*40,dst=a*80+i*40;
            // Mirror every LIVE scalar/block write, including source mask
            // replacement. Unknown source bits must erase prior knowledge.
            for(int mode=0;mode<2;mode++)
            {
                var x=mode==0?p:m;Copy(x,src,dst,8);x[dst+30]=x[src+30];Copy(x,src+24,dst+24,4);Copy(x,src+8,dst+8,16);
                int mask=primary?63:31;x[dst+31]=(byte)((x[dst+31]&~mask)|(x[src+31]&mask));
                if(!primary)Copy(x,src+32,dst+32,4);
            }
        }
        if(!primary)
        {
            int best=a*80;
            for(int mode=0;mode<2;mode++)
            {
                var x=mode==0?p:m;var h=mode==0?heap:heapMask;
                x.AsSpan(best,8).CopyTo(h.AsSpan(leader+16));h[leader+38]=x[best+30];
                x.AsSpan(best+24,2).CopyTo(h.AsSpan(leader+30));h[leader+32]=x[best+26];
                h[leader+33]=(byte)((h[leader+33]&192)|(x[best+31]&31)|(mode==1?32:0));
                x.AsSpan(best+32,2).CopyTo(h.AsSpan(leader+62));
            }
        }
    }
    private void Reduce(CancellationToken token)
    {
        unique=count==0?0:1;
        for(int i=1;i<count;i++)
        {
            token.ThrowIfCancellationRequested();int leader=(unique-1)*128,other=i*128;
            if(Compare(heap,leader,other)==0){Merge(leader,other,false,token);Merge(leader,other,true,token);}
            else{if(unique!=i){Copy(heap,other,unique*128,128);Copy(heapMask,other,unique*128,128);}unique++;}
        }
    }
    private static void Copy(byte[] b,int src,int dst,int n)=>b.AsSpan(src,n).CopyTo(b.AsSpan(dst,n));
    internal int Insert(int n,int capacity,ReadOnlySpan<byte> row,ReadOnlySpan<byte> mask)
    {
        int cost=I32(row,16);if(capacity==0||cost>=IMP)return n;int at;
        if(n==capacity)
        {
            if(I32(heap,16)<=cost)return n;at=0;
            while(at*2<n-1)
            {
                int child=at*2+1;if(child<n-1&&I32(heap,child*128+16)<=I32(heap,(child+1)*128+16))child++;
                if(I32(heap,child*128+16)<=cost)break;Copy(heap,child*128,at*128,128);Copy(heapMask,child*128,at*128,128);at=child;
            }
        }
        else
        {
            at=n++;
            while(at!=0)
            {int parent=(at-1)/2;if(cost<=I32(heap,parent*128+16))break;Copy(heap,parent*128,at*128,128);Copy(heapMask,parent*128,at*128,128);at=parent;}
        }
        row.CopyTo(heap.AsSpan(at*128));mask.CopyTo(heapMask.AsSpan(at*128));return n;
    }
    private void RebuildAndQuota(CancellationToken token)
    {
        int n=0;Span<byte> row=stackalloc byte[128];Span<byte> mask=stackalloc byte[128];
        for(int i=0;i<unique;i++)
        {token.ThrowIfCancellationRequested();heap.AsSpan(i*128,128).CopyTo(row);heapMask.AsSpan(i*128,128).CopyTo(mask);n=Insert(n,H,row,mask);}
        int old=n;
        if(n>L)
        {n=L;for(int i=L;i<old;i++){token.ThrowIfCancellationRequested();heap.AsSpan(i*128,128).CopyTo(row);heapMask.AsSpan(i*128,128).CopyTo(mask);n=Insert(n,L,row,mask);}}
        count=n;descriptorCount=n;descriptorCapacity=old>L?L:H;
    }
    private void Materialize(CancellationToken token)
    {
        stored=new byte[E*88];primary=new byte[L*80];secondary=new byte[E*80];
        storedMask=new byte[stored.Length];primaryMask=new byte[primary.Length];secondaryMask=new byte[secondary.Length];
        for(int i=0;i<L;i++)
        {
            token.ThrowIfCancellationRequested();W32(stored,i*88+16,IMP);W32(stored,i*88+20,IMP);storedMask.AsSpan(i*88+16,8).Fill(255);
            W32(primary,i*80,IMP);W32(primary,i*80+4,IMP);primaryMask.AsSpan(i*80,8).Fill(255);
            W32(secondary,i*80,IMP);W32(secondary,i*80+4,IMP);secondaryMask.AsSpan(i*80,8).Fill(255);
        }
        for(int i=0;i<count;i++)
        {
            token.ThrowIfCancellationRequested();int row=i*128,at=i*88,ordinal=I32(heap,row+88);
            if((uint)ordinal>=H)throw new InvalidDataException("Actual ordinal58 exceeds both enabled pools.");
            heap.AsSpan(row,88).CopyTo(stored.AsSpan(at));heapMask.AsSpan(row,88).CopyTo(storedMask.AsSpan(at));
            if(U16(stored,at+60)==ushort.MaxValue){W16(stored,at+60,i);storedMask.AsSpan(at+60,2).Fill(255);}
            primaryPool.AsSpan(ordinal*80,80).CopyTo(primary.AsSpan(i*80));primaryPoolMask.AsSpan(ordinal*80,80).CopyTo(primaryMask.AsSpan(i*80));
            secondaryPool.AsSpan(ordinal*80,80).CopyTo(secondary.AsSpan(i*80));secondaryPoolMask.AsSpan(ordinal*80,80).CopyTo(secondaryMask.AsSpan(i*80));
        }
        for(int i=0;i<L;i++){token.ThrowIfCancellationRequested();W16(stored,i*88+62,ushort.MaxValue);storedMask.AsSpan(i*88+62,2).Fill(255);}
    }
        private static int I32(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
        private static ushort U16(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(b[at..]);
        private static uint U32(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadUInt32LittleEndian(b[at..]);
        private static ulong U64(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadUInt64LittleEndian(b[at..]);
        private static void W32(byte[] b,int at,int value)=>BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(at),value);
        private static void W16(byte[] b,int at,int value)=>BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(at),checked((ushort)value));
        private static void W64(byte[] b,int at,ulong value)=>BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(at),value);
    }
    private int phase0Issued;
    private void ValidatePhase0OriginalOwner(CancellationToken token)
    {
        RequireWholeAdmission(token);
        if(pool is null||context is null||consumer is null||page is null||source is null||heap is null||
            Volatile.Read(ref stage)!=4||!ReferenceEquals(consumer.Context,context)||!ReferenceEquals(context.D0,d0)||
            !ReferenceEquals(pool.Owner,d0)||!consumer.ReadSkippedLanguageCompletionClaim()||context.Count is <0 or >12||
            pool.OwnedContext.NodeCount!=18+context.Count||heap.Capacity!=250||heap.Count is <0 or >250||
            d0.CurrentNode!=0||d0.CurrentSpan!=0||d0.ReusedPreviousCache)
            throw new InvalidDataException("Exact claimed original Number owner/pool/heap is required.");
        source.ValidateLiveLease(this,page,pool,token);
        var admitted=PreflightOriginalGraph(d0,token);
        if(!admitted.TrueBoundary)throw new NotSupportedException("Actual TRUE graph predicate required.");
        bool boundary=false;var graph=d0.Graph.State;
        for(int node=1;node<3&&node<graph.NodeCount;node++)
            foreach(var arc in graph.GetNode(node))
            {token.ThrowIfCancellationRequested();if(node-arc.Span-1==0&&(arc.Flags&4)!=0)boundary=true;}
        if(!boundary)throw new NotSupportedException("Only the actual retained TRUE following-boundary branch is admitted.");
    }
}
