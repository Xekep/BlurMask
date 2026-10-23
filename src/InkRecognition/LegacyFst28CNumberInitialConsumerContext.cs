namespace InkRecognition;

public readonly record struct LegacyFst28CNumberConsumerGraft(int Ordinal,
    LegacyFst28CNumberPageRecord SourceRecord,ulong GraftedContextIdentity,bool Created);
internal readonly record struct LegacyFst28CNumberConsumerDiagnostic(bool AttachedOnce,
    bool SameD0,bool SameBootstrap,bool SameGraph,bool SameSource,bool SameHeader,
    bool SameBindingAndPage,bool HistoricalInitialViewRetained,int InitialNodes,
    int ActiveNodes,int Pages,int PoolLimit,bool Completed);

/// <summary>Actual first ordinary1FD240 context graft prerequisite ONLY.
/// Owns a fresh private prefix/page/pool from UNUSED initial D0. A historical
/// page/Binding, caller contexts/capacity or prepared rows are never accepted.
/// Qualifier, categories, scorer/heap, history/query and text acceptance excluded.</summary>
public sealed class LegacyFst28CNumberInitialConsumerContext
{
    public const int LogicalByteEnvelope=55626;
    public const int WorkEnvelope=226858;
    private readonly Pool pool;
    private readonly LegacyFst28CNumberConsumerGraft[] grafts;
    private readonly LegacySymbolContextNode[] nodes;
    private readonly IReadOnlyList<LegacySymbolContextNode> nodeView;
    public LegacyFst28CNumberInitialD0 D0=>pool.Owner;
    public int Count {get;}
    public int InitialContextNodeCount=>18;
    public int ActiveContextNodeCount=>nodes.Length;
    public int ContextPageCount {get;}
    public int SourceNamespace=>1;
    public uint SourceHeader=>1;
    public int SourceKind40=>0;
    public bool HasQueryAuthority=>false;
    public bool HasParentHistoryAuthority=>false;
    public bool HasCompleteWordStartup=>false;
    public int LogicalByteReservation=>LogicalByteEnvelope;
    public int WorkReservation=>WorkEnvelope;
    private LegacyFst28CNumberInitialConsumerContext(Pool pool,
        LegacyFst28CNumberConsumerGraft[] grafts,int count,LegacySymbolContextNode[] nodes)
    {
        this.pool=pool;this.grafts=grafts;Count=count;this.nodes=nodes;
        nodeView=Array.AsReadOnly(nodes);ContextPageCount=pool.PageCount;
    }
    public LegacyFst28CNumberConsumerGraft ReadGraft(int ordinal)
    {if((uint)ordinal>=(uint)Count)throw new ArgumentOutOfRangeException(nameof(ordinal));return grafts[ordinal];}
    public IReadOnlyList<LegacySymbolContextNode> ReadActiveContextNodes()=>nodeView;
    public static LegacyFst28CNumberInitialConsumerContext CreateFreshInitialContext(
        LegacyFst28CNumberInitialD0 d0,CancellationToken token=default)
        =>CreateBounded(d0,LogicalByteEnvelope,WorkEnvelope,token);

    internal static LegacyFst28CNumberInitialConsumerContext CreateBounded(
        LegacyFst28CNumberInitialD0 d0,int byteBudget,int workBudget,
        CancellationToken token=default,Action<string>? checkpoint=null)
        =>CreateCore(d0,byteBudget,workBudget,token,checkpoint,null);
    internal static LegacyFst28CNumberInitialConsumerContext CreateRetainedForTrueLanguage(
        LegacyFst28CNumberTrueLanguageSourceDraft receiver,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(receiver);
        receiver.RequireWholeAdmission(token); // No whole TRUE admission exists in this draft.
        return CreateCore(receiver.ExpectedD0,LogicalByteEnvelope,WorkEnvelope,token,null,receiver);
    }
    private static LegacyFst28CNumberInitialConsumerContext CreateCore(
        LegacyFst28CNumberInitialD0 d0,int byteBudget,int workBudget,CancellationToken token,
        Action<string>? checkpoint,LegacyFst28CNumberTrueLanguageSourceDraft? receiver)
    {
        token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(d0);
        if(byteBudget is <0 or >LogicalByteEnvelope)throw new ArgumentOutOfRangeException(nameof(byteBudget));
        if(workBudget is <0 or >WorkEnvelope)throw new ArgumentOutOfRangeException(nameof(workBudget));
        if(byteBudget<LogicalByteEnvelope||workBudget<WorkEnvelope)
            throw new InvalidDataException("Complete consumer pool/prefix/page/graft envelope required.");
        ValidateOwner(d0,token);receiver?.RequireWholeAdmission(token);
        // This SAME authenticated header will be attached by the original
        // Binding core, not a new/equal-byte detached context container.
        var header=d0.Bootstrap.RetainInitialNumberHeaderIdentity(d0,token);
        var pool=Pool.Create(d0,header);receiver?.BeginPrivatePool(pool,token);
        void Checkpoint(string stage){token.ThrowIfCancellationRequested();checkpoint?.Invoke(stage);token.ThrowIfCancellationRequested();}
        try
        {
            Checkpoint("before-prefix-claim");
            var binding=LegacyFst28CNumberInitialQueryBinding.CreateForConsumerPool(d0,pool,token,
                s=>Checkpoint("binding-"+s));
            Checkpoint("after-prefix");
            var page=receiver is null
                ?LegacyFst28CNumberInitialPage.CreateFirstNormalPage(binding,token)
                :LegacyFst28CNumberInitialPage.CreateRetainedForTrueLanguage(binding,receiver,pool,token);
            Checkpoint("after-page");
            pool.BeginGraft(binding,page,token);
            var context=pool.OwnedContext;
            var records=new LegacyFst28CNumberConsumerGraft[12]; //12*56 logical.
            var seen=new bool[192]; //bounded scratch within1024 metadata.
            ulong sentinel=binding.ReadOuterPrefixNode(4);
            if(sentinel==0||context.ReadNode(sentinel) is not {Symbol:0xfffa,ParentLink:0,ChildTreeRoot:0})
                throw new InvalidDataException("Exact existing FFFA root required.");
            for(int i=0;i<page.Count;i++)
            {
                token.ThrowIfCancellationRequested();var row=page.ReadRecord(i);
                if(row.Symbol>=192||seen[row.Symbol]||row.SourceCategory!=0||row.CaseFlags!=0||row.Extra!=0||
                    (byte)(row.Tag>>32)!=1||row.ContextIdentity==0)
                    throw new InvalidDataException("Actual unique ordinary number row required.");
                seen[row.Symbol]=true;int label=-1;
                for(ushort n=0;n<16;n++)if(binding.ReadCurrentCode(n)==row.Symbol)
                {if(label!=-1)throw new InvalidDataException("Aliased ordinary label outside admitted source.");label=n;}
                if(label<0||binding.ReadSourceNode((ushort)label)!=row.ContextIdentity||
                    context.ReadNode(row.ContextIdentity) is not {ParentLink:0,ChildTreeRoot:0} source||source.Symbol!=row.Symbol)
                    throw new InvalidDataException("Same original ordinary source leaf required.");
                Checkpoint("before-graft-"+i);
                int before=context.NodeCount;
                ulong graft=LegacyNativeWordStartupRequestFactory.GraftOrdinaryInitialContext(context,row.ContextIdentity,token);
                if(graft==0||graft==row.ContextIdentity||context.NodeCount!=before+1||context.NodeCount>30||
                    context.ReadNode(graft) is not {ChildTreeRoot:0} child||child.Symbol!=row.Symbol||child.ParentLink!=sentinel)
                    throw new InvalidDataException("Fresh bounded FFFA child allocation/link differs.");
                records[i]=new(i,row,graft,true);
                Checkpoint("after-graft-"+i);
            }
            int count=18+page.Count;
            if(context.NodeCount!=count||context.PageCount!=1||page.Count>12)
                throw new InvalidDataException("Complete active-context bound differs.");
            // No CopyNodes/sort/whole-context/ID-ledger pipeline. Scalar reads
            // from original18 IDs and actual produced child IDs, once each.
            var snapshot=new LegacySymbolContextNode[count];
            var initial=binding.ReadLiveNodes();
            for(int i=0;i<count;i++)
            {
                token.ThrowIfCancellationRequested();
                ulong id=i<18?initial[i].Identity:records[i-18].GraftedContextIdentity;
                snapshot[i]=context.ReadNode(id);
            }
            bool Member(ulong id){if(id==0)return true;for(int i=0;i<count;i++)if(snapshot[i].Identity==id)return true;return false;}
            for(int i=0;i<count;i++)
            {
                token.ThrowIfCancellationRequested();var n=snapshot[i];
                if(n.PageIndex!=0||n.PageSlot!=i||n.Balance is <-1 or >1||!Member(n.Left)||!Member(n.Right)||
                    !Member(n.ParentLink)||!Member(n.ChildTreeRoot)||(i<18?n.ParentLink!=0:n.ParentLink!=sentinel))
                    throw new InvalidDataException("Produced active-context/AVL membership differs.");
            }
            for(int i=0;i<page.Count;i++)
                if(records[i].SourceRecord!=page.ReadRecord(i))throw new InvalidDataException("Historical source page changed.");
            Checkpoint("after-active-snapshot");pool.Complete(count,token);
            Checkpoint("before-publish");var result=new LegacyFst28CNumberInitialConsumerContext(pool,records,page.Count,snapshot);
            receiver?.AcceptCompletedContext(result,pool,token);return result;
        }
        catch{pool.Poison();throw;}
    }
    // Trusted receiving handoff to the next owned initial consumer. Historical
    // diagnostics/snapshots are not accepted as pool, page or source authority.
    internal void AttachInitialConsumer(LegacyFst28CNumberInitialConsumer receiver,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(receiver);
        if(!ReferenceEquals(receiver.ExpectedContext,this))throw new ArgumentException("Foreign receiving context.");
        var d=pool.ReadDiagnostic(nodes.Length,token);
        if(!d.Completed||!d.SameD0||!d.SameBootstrap||!d.SameGraph||!d.SameSource||!d.SameHeader||
            !d.SameBindingAndPage||!d.HistoricalInitialViewRetained||d.ActiveNodes!=18+Count||d.Pages!=1)
            throw new InvalidOperationException("Exact completed private number pool required.");
        receiver.AcceptTrustedContext(this,pool,grafts.AsSpan(0,Count),token);
    }
    private static void ValidateOwner(LegacyFst28CNumberInitialD0 d0,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(d0.CurrentNode!=0||d0.CurrentSpan!=0||d0.ReusedPreviousCache||d0.AcousticEvaluations is <1 or >64||
            !ReferenceEquals(d0.Bootstrap.Graph,d0.Graph)||!ReferenceEquals(d0.Bootstrap.InitialState,d0.InitialState)||
            !ReferenceEquals(d0.InitialState.TargetPreparation,d0.Policy)||!ReferenceEquals(d0.Policy.PrimaryMap.Source,d0.Source)||
            d0.Source.StateCount!=98||d0.Source.TransitionWordCount!=299||!d0.Source.HasActualFreshMain135Authority||
            d0.Policy.PrimaryMap.SourceLabelCount!=16||d0.InitialState.Capacity!=12)
            throw new ArgumentException("Exact genuine initial number D0 required.");
    }
    internal LegacyFst28CNumberConsumerDiagnostic ReadDiagnostic(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();return pool.ReadDiagnostic(nodes.Length,token);
    }
    // Bounded detached test inspection, no private owner/context/Binding/page
    // escapes. Allocation is per caller inspection, outside retained factory B.
    internal int[] ReadOwnerDiagnostic(CancellationToken token=default)
    {
        var d=ReadDiagnostic(token);
        return [d.AttachedOnce?1:0,d.SameD0?1:0,d.SameBootstrap?1:0,d.SameGraph?1:0,
            d.SameSource?1:0,d.SameHeader?1:0,d.SameBindingAndPage?1:0,
            d.HistoricalInitialViewRetained?1:0,d.InitialNodes,d.ActiveNodes,d.Pages,d.PoolLimit,d.Completed?1:0];
    }
    internal LegacySymbolContextNode[] CopyHistoricalInitialNodes(CancellationToken token=default)
        =>pool.CopyHistoricalInitialNodes(token);
    // Nonmutating typed-pool validation control, no receipt/result/claim. It
    // exercises the SAME method used by the original core before its claim.
    internal static void ValidatePoolAssociation(LegacyFst28CNumberInitialD0 owner,
        LegacyFst28CNumberInitialD0 requested,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(owner);ArgumentNullException.ThrowIfNull(requested);
        ValidateOwner(owner,token);ValidateOwner(requested,token);
        var pool=Pool.Create(owner,owner.Bootstrap.RetainInitialNumberHeaderIdentity(owner,token));
        pool.ValidateBeforeBindingClaim(requested,requested.Bootstrap.RetainInitialNumberHeaderIdentity(requested,token),token);
    }

    // No constructor/capacity/context/factory ingress. Only this NEW owning
    // component creates this sealed typed pool; only OLD core attaches it.
    internal sealed class Pool
    {
        internal LegacyFst28CNumberInitialD0 Owner {get;}
        private readonly object header;
        private LegacySymbolContextFactory? context;
        private LegacyFst28CNumberInitialQueryBinding? binding;
        private LegacyFst28CNumberInitialPage? page;
        //0validated-empty,1taken,2attached,3grafting,4completed,5poisoned.
        private int state;
        private Pool(LegacyFst28CNumberInitialD0 owner,object header){Owner=owner;this.header=header;}
        internal static Pool Create(LegacyFst28CNumberInitialD0 owner,object header)=>new(owner,header);
        internal int PageCount=>context?.PageCount??0;
        internal LegacySymbolContextFactory OwnedContext=>context??throw new InvalidOperationException("Pool not taken.");
        internal void ValidateBeforeBindingClaim(LegacyFst28CNumberInitialD0 owner,object actualHeader,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(!ReferenceEquals(owner,Owner)||!ReferenceEquals(actualHeader,header)||
                !ReferenceEquals(owner.Bootstrap,Owner.Bootstrap)||!ReferenceEquals(owner.Graph,Owner.Graph)||
                !ReferenceEquals(owner.Source,Owner.Source))throw new ArgumentException("Foreign typed pool/D0/header.");
            if(Volatile.Read(ref state)!=0||context is not null||binding is not null||page is not null)
                throw new InvalidOperationException("Typed pool was already taken/attached/poisoned.");
        }
        internal LegacySymbolContextFactory TakeEmptyContext(LegacyFst28CNumberInitialD0 owner,object actualHeader,CancellationToken token)
        {
            ValidateBeforeBindingClaim(owner,actualHeader,token);
            if(Interlocked.CompareExchange(ref state,1,0)!=0)throw new InvalidOperationException("Pool take raced.");
            return context=new LegacySymbolContextFactory(30);
        }
        internal void AttachInitialBinding(LegacyFst28CNumberInitialQueryBinding value,
            LegacySymbolContextFactory actualContext,object actualHeader,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(!ReferenceEquals(actualContext,context)||!ReferenceEquals(actualHeader,header)||!ReferenceEquals(value.D0,Owner)||
                !ReferenceEquals(value.Bootstrap,Owner.Bootstrap)||!ReferenceEquals(value.Graph,Owner.Graph)||
                !ReferenceEquals(value.Source,Owner.Source)||!ReferenceEquals(value.Policy,Owner.Policy)||
                !value.RetainsExactHeaderIdentity(token)||actualContext.NodeCount!=18||actualContext.PageCount!=1)
                throw new ArgumentException("Exact initial pool/context/header/binding attachment required.");
            if(Interlocked.CompareExchange(ref state,2,1)!=1||binding is not null)
                throw new InvalidOperationException("Pool already attached or poisoned.");
            binding=value;
        }
        internal void ValidateRetainedSourceBinding(LegacyFst28CNumberTrueLanguageSourceDraft receiver,
            LegacyFst28CNumberInitialQueryBinding value,CancellationToken token)
        {
            receiver.RequireWholeAdmission(token);
            if(!ReferenceEquals(receiver.ExpectedD0,Owner)||!ReferenceEquals(value,binding)||
                !ReferenceEquals(value.D0,Owner)||!value.RetainsExactHeaderIdentity(token)||
                Volatile.Read(ref state)!=2||page is not null||context is null||context.NodeCount!=18)
                throw new ArgumentException("Exact live private prefix before graft required.");
        }
        internal void ValidateRetainedSourcePage(LegacyFst28CNumberTrueLanguageSourceDraft receiver,
            LegacyFst28CNumberInitialPage actualPage,CancellationToken token)
        {
            receiver.RequireWholeAdmission(token);
            if(!ReferenceEquals(receiver.ExpectedD0,Owner)||!ReferenceEquals(actualPage,page)||
                !ReferenceEquals(actualPage.Binding,binding)||Volatile.Read(ref state)!=4||context is null)
                throw new ArgumentException("Exact completed retained Page/private pool required.");
            actualPage.ValidateRetainedSourceReceiver(receiver,this,token);
        }
        internal void BeginGraft(LegacyFst28CNumberInitialQueryBinding value,LegacyFst28CNumberInitialPage actualPage,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(!ReferenceEquals(value,binding)||!ReferenceEquals(actualPage.Binding,value)||actualPage.Count>12||
                !ReferenceEquals(value.D0,Owner)||!value.RetainsExactHeaderIdentity(token)||OwnedContext.NodeCount!=18)
                throw new ArgumentException("Same private initialized pool/binding/page required.");
            if(Interlocked.CompareExchange(ref state,3,2)!=2||page is not null)
                throw new InvalidOperationException("Consumer context already begun or poisoned.");
            page=actualPage;
        }
        internal void Complete(int nodes,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(OwnedContext.NodeCount!=nodes||nodes is <18 or >30||OwnedContext.PageCount!=1||
                !ReferenceEquals(binding?.D0,Owner)||!ReferenceEquals(page?.Binding,binding))
                throw new InvalidDataException("Complete owning context differs.");
            if(Interlocked.CompareExchange(ref state,4,3)!=3)throw new InvalidOperationException("Pool cannot complete.");
        }
        internal LegacySymbolContextNode[] CopyHistoricalInitialNodes(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(Volatile.Read(ref state)!=4||binding is null)throw new InvalidOperationException("No completed historical snapshot.");
            var original=binding.ReadLiveNodes();var copy=new LegacySymbolContextNode[original.Count];
            for(int i=0;i<copy.Length;i++){token.ThrowIfCancellationRequested();copy[i]=original[i];}
            return copy;
        }
        internal void Poison()=>Interlocked.Exchange(ref state,5);
        internal LegacyFst28CNumberConsumerDiagnostic ReadDiagnostic(int activeNodes,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();var b=binding??throw new InvalidOperationException("Pool unattached.");
            var initial=b.ReadLiveNodes();bool historical=initial.Count==18;
            for(int i=0;i<initial.Count;i++)historical&=initial[i].ParentLink==0&&initial[i].ChildTreeRoot==0;
            return new(Volatile.Read(ref state)==4,ReferenceEquals(b.D0,Owner),ReferenceEquals(b.Bootstrap,Owner.Bootstrap),
                ReferenceEquals(b.Graph,Owner.Graph),ReferenceEquals(b.Source,Owner.Source),
                ReferenceEquals(header,Owner.Bootstrap.RetainInitialNumberHeaderIdentity(Owner,token)),
                ReferenceEquals(page?.Binding,b),historical,18,OwnedContext.NodeCount,
                OwnedContext.PageCount,30,Volatile.Read(ref state)==4&&activeNodes==OwnedContext.NodeCount);
        }
    }
}
