using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyFst28CNumberSkippedLanguageCompletion.Probe")]
namespace InkRecognition;

// Internal stage only: historical Number node0 materialization plus a NEW
// library logical reset receipt. No live PageEngine/query/node1/controller.
internal sealed class LegacyFst28CNumberSkippedLanguageCompletion
{
    internal const int LogicalBytes=1211568,Work=61261520;
    internal const int DefaultByteCeiling=2*1024*1024,DefaultWorkCeiling=64*1024*1024;
    internal const int SnapshotLogicalCeiling=96*1024,SnapshotWorkCeiling=192*1024;
    private const int H=250,L=125,E=156,IMP=0x4000000;
    internal LegacyFst28CNumberInitialConsumer ExpectedConsumer {get;}
    private LegacyFst28CNumberInitialConsumerContext.Pool? pool;
    private LegacyNativeWordRecordHeap? originalHeap;
    private LegacyFst28CNumberConsumerGraft[] grafts=[];
    private int attached,state,count,unique,cleanup;
    private byte[] heap=[],heapMask=[],primaryPool=[],secondaryPool=[],primaryPoolMask=[],secondaryPoolMask=[];
    private byte[] stored=[],primary=[],secondary=[],storedMask=[],primaryMask=[],secondaryMask=[];
    private int descriptorCount,descriptorCapacity=250,sourceEnd,retainedEnd,cumulative,backward;
    private LogicalReset? reset;
    private LegacyFst28CNumberSkippedLanguageCompletion(LegacyFst28CNumberInitialConsumer source)
    {ExpectedConsumer=source;}
    internal bool HasLiveQueryAuthority=>false;
    internal bool HasParentHistoryAuthority=>false;
    internal bool HasNode1Authority=>false;
    internal bool HasCompleteController=>false;
    internal static LegacyFst28CNumberSkippedLanguageCompletion Create(
        LegacyFst28CNumberInitialConsumer consumer,CancellationToken token=default)
        =>CreateBounded(consumer,LogicalBytes,Work,token);

    internal static LegacyFst28CNumberSkippedLanguageCompletion CreateBounded(
        LegacyFst28CNumberInitialConsumer consumer,int byteBudget,int workBudget,
        CancellationToken token=default,Action<string>? checkpoint=null)
    {
        token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(consumer);
        if(byteBudget is <0 or >DefaultByteCeiling)throw new ArgumentOutOfRangeException(nameof(byteBudget));
        if(workBudget is <0 or >DefaultWorkCeiling)throw new ArgumentOutOfRangeException(nameof(workBudget));
        if(byteBudget<LogicalBytes||workBudget<Work)
            throw new InvalidDataException("Whole Number node0 value/mask envelope required before handoff/claim.");
        void Step(string stage){token.ThrowIfCancellationRequested();checkpoint?.Invoke(stage);token.ThrowIfCancellationRequested();}
        var result=new LegacyFst28CNumberSkippedLanguageCompletion(consumer);
        consumer.AttachSkippedLanguageCompletion(result,token);
        result.Preflight(token);Step("after-preflight");
        // The native complete language-required path is unsupported. Its gate
        // comes ONLY from the SAME genuine retained graph, never a caller bool.
        if(result.HasFollowingBoundary(token))
            throw new NotSupportedException("Actual number node0 requires the separate language branch.");
        Step("before-claim");bool claimed=false;
        try
        {
            consumer.ClaimSkippedLanguageCompletion(result);
            claimed=true;Step("after-claim");result.ValidateRetained(token);
            result.heap=result.originalHeap!.GetSlotStorageSnapshot().ToArray();
            if(result.heap.Length!=H*128)throw new InvalidDataException("Exact retained250-slot heap required.");
            result.count=result.originalHeap.Count;result.heapMask=new byte[H*128];
            for(int i=0;i<result.count;i++)
            {
                token.ThrowIfCancellationRequested();result.heapMask.AsSpan(i*128,120).Fill(255);
                W32(result.heap,i*128+88,i);result.heapMask.AsSpan(i*128+88,4).Fill(255);
            }
            Step("before-root-alternatives");result.FillRootPools(token);
            Step("before-sort");result.Sort(token);
            Step("before-merge");result.Reduce(token);
            Step("before-heap-rebuild");result.RebuildAndQuota(token);
            Step("before-materialize");result.Materialize(token);
            Step("before-phase1-reset");
            //212D80(1): count is cleared BEFORE capacity changes; no growth,
            //language scoring, secondary quota, or postreset heap snapshot.
            result.descriptorCount=0;result.descriptorCapacity=124;
            Step("before-common-tail");result.FinishCommonTail(token);
            Step("before-logical-source-reset");result.reset=LogicalReset.Create(result,token);
            result.ValidateRetained(token);Step("before-publish");
            Volatile.Write(ref result.state,2);return result;
        }
        catch{if(claimed)Volatile.Write(ref result.state,3);throw;}
    }

    internal void AcceptTrustedInitial(LegacyFst28CNumberInitialConsumer actual,
        LegacyFst28CNumberInitialConsumerContext.Pool actualPool,LegacyNativeWordRecordHeap actualHeap,
        ReadOnlySpan<LegacyFst28CNumberConsumerGraft> actualGrafts,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(actual,ExpectedConsumer)||!ReferenceEquals(actualPool.Owner,actual.D0)||
            actualGrafts.Length>12||actualHeap.Capacity!=H||pool is not null||
            Interlocked.CompareExchange(ref attached,1,0)!=0)
            throw new ArgumentException("Exact one-use receiving Number consumer/pool required.");
        pool=actualPool;originalHeap=actualHeap;grafts=actualGrafts.ToArray();ValidateRetained(token);
    }
    private void ValidateRetained(CancellationToken token)
    {
        var p=pool??throw new InvalidOperationException("No trusted Number pool.");
        var d=p.ReadDiagnostic(18+grafts.Length,token);var d0=ExpectedConsumer.D0;
        if(!d.Completed||!d.SameD0||!d.SameBootstrap||!d.SameGraph||!d.SameSource||!d.SameHeader||
            !d.SameBindingAndPage||!d.HistoricalInitialViewRetained||d.Pages!=1||d.ActiveNodes!=18+grafts.Length||
            !ReferenceEquals(p.Owner,d0)||!ReferenceEquals(d0.Graph,d0.Bootstrap.Graph)||
            !ReferenceEquals(d0.Source,d0.Policy.PrimaryMap.Source))
            throw new InvalidOperationException("Same completed source/header/context cohort required.");
    }
    private void Preflight(CancellationToken token)
    {
        ValidateRetained(token);var source=ExpectedConsumer;var d0=source.D0;var graph=d0.Graph.State;
        if(graph.NodeCount is <2 or >65535||graph.MaximumAtoms!=8||d0.CurrentNode!=0||d0.CurrentSpan!=0||d0.ReusedPreviousCache||
            originalHeap!.Capacity!=H||originalHeap.Count is <0 or >H||pool!.OwnedContext.NodeCount!=18+grafts.Length||
            d0.Bootstrap.ReadSourceMetadata(token)!=new LegacyFst28CNumberSourceMetadata(1,0,1,0,1,uint.MaxValue,1))
            throw new NotSupportedException("Exact nonfinal node0/ordinary Number preparation required.");
        for(int node=1;node<Math.Min(graph.NodeCount,3);node++)
            if(graph.GetNode(node).Count>256)throw new NotSupportedException("Bounded genuine following-arc rows required.");
        var contexts=pool.OwnedContext;
        foreach(var g in grafts)
        {
            token.ThrowIfCancellationRequested();var r=g.SourceRecord;
            if(!g.Created||r.Symbol>9&&r.Symbol!=138&&r.Symbol!=185||(byte)(r.Tag>>32)!=1||
                r.SourceCategory!=0||r.CaseFlags!=0||r.Extra!=0||
                contexts.ReadNode(r.ContextIdentity) is not {ParentLink:0,ChildTreeRoot:0} root||root.Symbol!=r.Symbol||
                contexts.ReadNode(g.GraftedContextIdentity) is not {ChildTreeRoot:0} child||child.Symbol!=r.Symbol||
                child.ParentLink==0||contexts.ReadNode(child.ParentLink) is not {Symbol:0xfffa,ParentLink:0})
                throw new InvalidDataException("Exact existing root/mapped/raw graft paths required before claim.");
        }
        // Scalar-only inspection, no CopyGroup/NN/tensors/ranges/resource copy.
        for(int i=0;i<source.CandidateCount;i++)
        {
            token.ThrowIfCancellationRequested();var row=source.ReadCandidate(i);
            bool member=false;foreach(var g in grafts)if(g.GraftedContextIdentity==row.ContextIdentity&&g.SourceRecord.Symbol==row.Symbol){member=true;break;}
            if((byte)(row.SourceTag>>32)!=1||row.Symbol>9&&row.Symbol!=138&&row.Symbol!=185||row.Auxiliary34!=0||
                row.Cost>=IMP||!member)
                throw new NotSupportedException("Exact finite ordinary Number root records required.");
        }
    }
    private bool HasFollowingBoundary(CancellationToken token)
    {
        var graph=ExpectedConsumer.D0.Graph.State;
        for(int node=1;node<3&&node<graph.NodeCount;node++)
            foreach(var arc in graph.GetNode(node))
            {token.ThrowIfCancellationRequested();if(node-arc.Span-1==0&&(arc.Flags&4)!=0)return true;}
        return false;
    }
    private void FillRootPools(CancellationToken token)
    {
        primaryPool=new byte[H*80];secondaryPool=new byte[H*80];
        primaryPoolMask=new byte[H*80];secondaryPoolMask=new byte[H*80];
        var contexts=pool!.OwnedContext;int before=contexts.NodeCount;
        for(int i=0;i<count;i++)
        {
            token.ThrowIfCancellationRequested();int row=i*128;ulong suffix=U64(heap,row+8);
            ulong mapped=LegacyNativeWordContextComposition.CombineMapped(contexts,0,suffix,static s=>s,token);
            ulong raw=LegacyNativeWordContextComposition.CombineRaw(contexts,0,suffix,token);
            bool member=false;foreach(var g in grafts)if(g.GraftedContextIdentity==raw&&g.SourceRecord.ContextIdentity==mapped){member=true;break;}
            if(contexts.NodeCount!=before||!member)
                throw new InvalidDataException("Root composition changed the admitted existing context paths.");
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
    private int Insert(int n,int capacity,ReadOnlySpan<byte> row,ReadOnlySpan<byte> mask)
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
    private void FinishCommonTail(CancellationToken token)
    {
        CleanupAndCounters(token);
        // Native2528 is allocated by1FA300. Header1 kind0, anchor0/aux34=0
        // resolve through222.V178→21B.VD0 to0. No borrowed English policy.
        for(int i=0;i<count;i++)
            if(stored[i*88+4]!=1||I32(stored,i*88+56)!=0||I32(stored,i*88+52)!=0)
                throw new InvalidDataException("Bounded node0 inverse-context tail differs.");
        //151BF0 fresh preparation54=120,2490=0 =>120+0>node0+1.
        //No207CB0 publication, 50-node normalization, or external observer.
    }
    private void CleanupAndCounters(CancellationToken token)
    {
        sourceEnd=retainedEnd=L;
        for(int i=0;i<L*2;i++)
        {
            token.ThrowIfCancellationRequested();int at=i*40;
            bool ck=All(primaryMask,at,4),fk=(primaryMask[at+31]&32)!=0,pk=All(primaryMask,at+24,2);
            bool skip=ck&&I32(primary,at)>=IMP||fk&&(primary[at+31]&32)!=0||pk&&U16(primary,at+24)<L;
            if(I32(primary,at)<IMP&&(primary[at+31]&32)==0&&U16(primary,at+24)>=L)
            {W32(primary,at,IMP);cleanup++;if(ck&&fk&&pk)primaryMask.AsSpan(at,4).Fill(255);}
            if(!(ck&&fk&&pk||skip))primaryMask.AsSpan(at,4).Clear();
        }
        cumulative=0;backward=0;
    }
    private static bool All(byte[] b,int at,int n)
    {for(int i=0;i<n;i++)if(b[at+i]!=255)return false;return true;}

    private sealed class LogicalReset
    {
        private readonly LegacyFst28CNumberSkippedLanguageCompletion owner;
        private readonly ushort[] current,preserved,secondary;
        private LogicalReset(LegacyFst28CNumberSkippedLanguageCompletion owner,ushort[] current,ushort[] preserved,ushort[] secondary)
        {this.owner=owner;this.current=current;this.preserved=preserved;this.secondary=secondary;}
        internal static LogicalReset Create(LegacyFst28CNumberSkippedLanguageCompletion owner,CancellationToken token)
        {
            owner.ValidateRetained(token);var d0=owner.ExpectedConsumer.D0;
            var current=d0.CopyCurrentPrimary(token);var preserved=d0.Masks.CopyPreservedMap(token);var secondary=new ushort[16];
            for(ushort i=0;i<16;i++){token.ThrowIfCancellationRequested();secondary[i]=d0.Policy.MapSecondary(i).Code;}
            return new(owner,current,preserved,secondary);
        }
        internal LegacyFst28CNumberLogicalResetSnapshot CopyDiagnostic()
        {
            var writes=new List<ulong[]>(25){new ulong[]{0xc8,4,uint.MaxValue}};
            for(int i=0;i<8;i++){writes.Add([(ulong)(0x100+80*i),8,ulong.MaxValue]);writes.Add([(ulong)(0x110+80*i),8,ulong.MaxValue]);writes.Add([(ulong)(0x118+80*i),4,i==7?0:uint.MaxValue]);}
            return new(true,false,false,false,ReferenceEquals(owner.reset,this),0,writes.ToArray(),
                current.ToArray(),preserved.ToArray(),secondary.ToArray());
        }
    }
    private void RequireCompleted(){if(Volatile.Read(ref state)!=2)throw new InvalidOperationException("No completed scoped Number node0 receipt.");}
    internal LegacyFst28CNumberCompletionSnapshot CopySnapshot(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();RequireCompleted();ValidateRetained(token);
        byte[] CopyChecked(byte[] source){token.ThrowIfCancellationRequested();return source.ToArray();}
        var result=new LegacyFst28CNumberCompletionSnapshot(CopyChecked(stored),CopyChecked(primary),CopyChecked(secondary),CopyChecked(storedMask),CopyChecked(primaryMask),CopyChecked(secondaryMask),
            count,unique,sourceEnd,retainedEnd,descriptorCount,descriptorCapacity,H,cleanup,cumulative,backward,reset!.CopyDiagnostic());
        token.ThrowIfCancellationRequested();return result;
    }
    internal int[] ReadAssociation(CancellationToken token=default)
    {token.ThrowIfCancellationRequested();RequireCompleted();ValidateRetained(token);var d=pool!.ReadDiagnostic(18+grafts.Length,token);
     return [ReferenceEquals(pool.Owner,ExpectedConsumer.D0)?1:0,d.SameBootstrap?1:0,d.SameGraph?1:0,d.SameHeader?1:0,pool.OwnedContext.NodeCount,0,0,0];}
    internal static void ValidateAssociation(LegacyFst28CNumberInitialConsumer expected,LegacyFst28CNumberInitialConsumer actual,CancellationToken token=default)
    {token.ThrowIfCancellationRequested();actual.AttachSkippedLanguageCompletion(new(expected),token);}
    internal static bool ReadClaimDiagnostic(LegacyFst28CNumberInitialConsumer source)
    {ArgumentNullException.ThrowIfNull(source);return source.ReadSkippedLanguageCompletionClaim();}
    // Explicit PREPARED diagnostics: bounded numeric/mask operations only.
    // A temporary private scratch instance never receives a consumer/pool,
    // never claims, never reaches state2, and NEVER escapes as an owner.
    internal static LegacyFst28CNumberCompletionSnapshot RunPreparedKernel(
        ReadOnlySpan<byte> values,ReadOnlySpan<byte> masks,int active,ReadOnlySpan<byte> primary,
        ReadOnlySpan<byte> primaryMask,ReadOnlySpan<byte> secondary,ReadOnlySpan<byte> secondaryMask,
        CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();CheckPrepared(values,masks,active,primary,primaryMask,secondary,secondaryMask);
        var scratch=new LegacyFst28CNumberSkippedLanguageCompletion(null!){heap=values.ToArray(),heapMask=masks.ToArray(),count=active,
            primaryPool=primary.ToArray(),primaryPoolMask=primaryMask.ToArray(),secondaryPool=secondary.ToArray(),secondaryPoolMask=secondaryMask.ToArray()};
        CheckPrepared(scratch.heap,scratch.heapMask,active,scratch.primaryPool,scratch.primaryPoolMask,scratch.secondaryPool,scratch.secondaryPoolMask);
        scratch.Sort(token);scratch.Reduce(token);scratch.RebuildAndQuota(token);scratch.Materialize(token);
        scratch.descriptorCount=0;scratch.descriptorCapacity=124;scratch.CleanupAndCounters(token);
        return new(scratch.stored,scratch.primary,scratch.secondary,scratch.storedMask,scratch.primaryMask,scratch.secondaryMask,
            scratch.count,scratch.unique,125,125,0,124,250,scratch.cleanup,0,0,new(true,false,false,false,false,0,[],[],[],[]));
    }
    internal static LegacyFst28CNumberPreparedMergeSnapshot RunPreparedMerge(
        ReadOnlySpan<byte> values,ReadOnlySpan<byte> masks,ReadOnlySpan<byte> pool,ReadOnlySpan<byte> poolMask,
        bool primary,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();CheckPrepared(values,masks,2,pool,poolMask,pool,poolMask);
        var scratch=new LegacyFst28CNumberSkippedLanguageCompletion(null!){heap=values.ToArray(),heapMask=masks.ToArray()};
        if(primary){scratch.primaryPool=pool.ToArray();scratch.primaryPoolMask=poolMask.ToArray();}
        else{scratch.secondaryPool=pool.ToArray();scratch.secondaryPoolMask=poolMask.ToArray();}
        var owned=primary?scratch.primaryPool:scratch.secondaryPool;var ownedMask=primary?scratch.primaryPoolMask:scratch.secondaryPoolMask;
        CheckPrepared(scratch.heap,scratch.heapMask,2,owned,ownedMask,owned,ownedMask);
        scratch.Merge(0,128,primary,token);
        return new(scratch.heap,scratch.heapMask,primary?scratch.primaryPool:scratch.secondaryPool,primary?scratch.primaryPoolMask:scratch.secondaryPoolMask,false);
    }
    private static void CheckPrepared(ReadOnlySpan<byte> values,ReadOnlySpan<byte> masks,int active,
        ReadOnlySpan<byte> primary,ReadOnlySpan<byte> primaryMask,ReadOnlySpan<byte> secondary,ReadOnlySpan<byte> secondaryMask)
    {
        if(values.Length!=H*128||masks.Length!=values.Length||active is <0 or >H||primary.Length!=H*80||secondary.Length!=H*80||
            primaryMask.Length!=primary.Length||secondaryMask.Length!=secondary.Length)
            throw new ArgumentException("Exact bounded prepared numeric/mask arrays required; no owner authority.");
        Span<(int Offset,int Size)> fields=stackalloc (int,int)[7];
        fields[0]=(0,8);fields[1]=(36,2);fields[2]=(24,4);fields[3]=(48,4);fields[4]=(52,4);fields[5]=(28,2);fields[6]=(88,4);
        for(int i=0;i<active;i++)
        {
            int at=i*128;if((uint)I32(values,at+88)>=H)throw new ArgumentException("Prepared ordinal outside both pools.");
            foreach(var field in fields)
                for(int k=0;k<field.Size;k++)if(masks[at+field.Offset+k]!=255)
                    throw new NotSupportedException("Prepared comparator/ordinal fields must be fully defined.");
            if((masks[at+33]&128)==0)throw new NotSupportedException("Prepared key statebit7 must be defined.");
        }
    }
    private static int I32(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
    private static ushort U16(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(b[at..]);
    private static uint U32(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadUInt32LittleEndian(b[at..]);
    private static ulong U64(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadUInt64LittleEndian(b[at..]);
    private static void W32(byte[] b,int at,int value)=>BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(at),value);
    private static void W16(byte[] b,int at,int value)=>BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(at),checked((ushort)value));
    private static void W64(byte[] b,int at,ulong value)=>BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(at),value);
}

internal sealed record LegacyFst28CNumberCompletionSnapshot(byte[] Stored,byte[] Primary,byte[] Secondary,
    byte[] StoredDefined,byte[] PrimaryDefined,byte[] SecondaryDefined,int Count,int Unique,int SourceEnd,int RetainedEnd,
    int Phase1Count,int Phase1Capacity,int AllocatedCapacity,int Cleanup,int Cumulative2520,int Backward2528,LegacyFst28CNumberLogicalResetSnapshot LogicalReset);
internal sealed record LegacyFst28CNumberLogicalResetSnapshot(bool LogicalOnly,bool LivePageEngine,bool LiveQuery,
    bool NextNodeAuthority,bool SameOwner,int SourceNode,ulong[][] Stores,ushort[] Current,ushort[] Preserved,ushort[] Secondary);
internal sealed record LegacyFst28CNumberPreparedMergeSnapshot(byte[] Heap,byte[] HeapDefined,byte[] Pool,byte[] PoolDefined,bool HasOwnerAuthority);
