namespace InkRecognition;

// SOURCE DRAFT ONLY. This receiving factory cannot issue an owner until the
// complete TRUE-language operation has independently proved whole admission.
// No caller bool, supplied budget, prepared request or historical page opens it.
internal sealed partial class LegacyFst28CNumberTrueLanguageSourceDraft
{
    internal const int RetentionCreationBytes=256, RetainedSourceBytes=3008;
    internal const int RetainedPeakExtra=3264, RetentionWork=3200;
    private readonly LegacyFst28CNumberInitialD0 d0;
    private LegacyFst28CNumberInitialConsumerContext.Pool? pool;
    private LegacyFst28CNumberInitialConsumerContext? context;
    private LegacyFst28CNumberInitialConsumer? consumer;
    private LegacyFst28CNumberInitialPage? page;
    private ILegacyFst28CNumberRetainedSourceDraft? source;
    private LegacyNativeWordRecordHeap? heap;
    private int stage;
    private LegacyFst28CNumberTrueLanguageSourceDraft(LegacyFst28CNumberInitialD0 d0)
    {this.d0=d0;}
    internal LegacyFst28CNumberInitialD0 ExpectedD0=>d0;
    internal LegacyFst28CNumberInitialConsumer ExpectedConsumer=>consumer??
        throw new InvalidOperationException("No actual fresh receiving consumer attached.");

    internal static LegacyFst28CNumberTrueLanguageSourceDraft CreateFresh(
        LegacyFst28CNumberInitialD0 d0,ReadOnlySpan<byte> sameAcousticResource,
        CancellationToken token=default)
    {
        return CreateFreshBoundedSource(d0,sameAcousticResource,token,null);
    }
    internal void RequireWholeAdmission(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        // Mandatory closed gate, not a placeholder reservation or success flag.
        throw new NotSupportedException("Whole TRUE-language admission is not implemented; no prefix may be claimed.");
    }
    internal void BeginPrivatePool(LegacyFst28CNumberInitialConsumerContext.Pool value,
        CancellationToken token)
    {
        RequireWholeAdmission(token);
        if(!ReferenceEquals(value.Owner,d0)||pool is not null||Volatile.Read(ref stage)!=0)
            throw new ArgumentException("Exact fresh receiving D0/pool required.");
        pool=value;
    }
    internal void ValidatePageRequest(LegacyFst28CNumberInitialQueryBinding binding,
        LegacyFst28CNumberInitialConsumerContext.Pool value,CancellationToken token)
    {
        RequireWholeAdmission(token);
        if(!ReferenceEquals(pool,value)||!ReferenceEquals(value.Owner,d0)||
            !ReferenceEquals(binding.D0,d0)||page is not null||source is not null||Volatile.Read(ref stage)!=0)
            throw new ArgumentException("Exact constructing private pool/binding/receiver required.");
        value.ValidateRetainedSourceBinding(this,binding,token);
    }
    internal void AcceptPageSource(LegacyFst28CNumberInitialPage actualPage,
        LegacyFst28CNumberInitialConsumerContext.Pool value,
        ILegacyFst28CNumberRetainedSourceDraft actualSource,CancellationToken token)
    {
        RequireWholeAdmission(token);ValidatePageRequest(actualPage.Binding,value,token);
        actualPage.ValidateIssuedRetainedSource(this,value,actualSource,token);
        actualSource.ValidateExactReceiver(this,actualPage,value,token);
        page=actualPage;source=actualSource;Volatile.Write(ref stage,1);
    }
    internal void AcceptCompletedContext(LegacyFst28CNumberInitialConsumerContext value,
        LegacyFst28CNumberInitialConsumerContext.Pool actualPool,CancellationToken token)
    {
        RequireWholeAdmission(token);
        if(!ReferenceEquals(pool,actualPool)||!ReferenceEquals(value.D0,d0)||
            context is not null||page is null||source is null||Volatile.Read(ref stage)!=1)
            throw new ArgumentException("Exact completed retained-source context required.");
        actualPool.ValidateRetainedSourcePage(this,page,token);
        source.ValidateExactReceiver(this,page,actualPool,token);
        context=value;Volatile.Write(ref stage,2);
    }
    internal void BindCompletedConsumer(LegacyFst28CNumberInitialConsumer value,CancellationToken token)
    {
        RequireWholeAdmission(token);
        if(context is null||!ReferenceEquals(value.Context,context)||!ReferenceEquals(value.D0,d0)||
            consumer is not null||Volatile.Read(ref stage)!=2)
            throw new ArgumentException("Exact fresh same-context initial consumer required.");
        consumer=value;Volatile.Write(ref stage,3);
    }
    internal void ValidateReceivingConsumer(LegacyFst28CNumberInitialConsumer value,CancellationToken token)
    {
        RequireWholeAdmission(token);
        if(!ReferenceEquals(consumer,value)||!ReferenceEquals(value.Context,context)||
            !ReferenceEquals(value.D0,d0)||pool is null||page is null||source is null||Volatile.Read(ref stage)!=3)
            throw new ArgumentException("Foreign or historical receiving consumer.");
        pool.ValidateRetainedSourcePage(this,page,token);
        source.ValidateExactReceiver(this,page,pool,token);
    }
    internal void ValidateLiveOwner(ILegacyFst28CNumberRetainedSourceDraft actualSource,
        LegacyFst28CNumberInitialPage actualPage,LegacyFst28CNumberInitialConsumerContext.Pool actualPool,
        CancellationToken token)
    {
        RequireWholeAdmission(token);
        if(!ReferenceEquals(source,actualSource)||!ReferenceEquals(page,actualPage)||!ReferenceEquals(pool,actualPool)||
            consumer is null||!ReferenceEquals(consumer.D0,d0)||!ReferenceEquals(consumer.Context,context)||
            Volatile.Read(ref stage)!=4||!consumer.ReadSkippedLanguageCompletionClaim())
            throw new InvalidDataException("Exact entered receiving state differs.");
    }
    internal void AcceptTrustedConsumer(LegacyFst28CNumberInitialConsumer value,
        LegacyFst28CNumberInitialConsumerContext.Pool actualPool,
        LegacyNativeWordRecordHeap actualHeap,CancellationToken token)
    {
        ValidateReceivingConsumer(value,token);
        if(!ReferenceEquals(pool,actualPool)||!value.ReadSkippedLanguageCompletionClaim()||heap is not null)
            throw new ArgumentException("Exact already-claimed receiving consumer/pool required.");
        source!.EnterExclusive(this,token);heap=actualHeap;Volatile.Write(ref stage,4);
        // No query, phase0/phase1, reset, history, result or Ready follows here.
    }
}

// Assembly-private marker/validation protocol only. An implementation cannot
// authorize itself: Page authenticates exact private holder reference first.
// No public/raw query API, arrays, snapshots, cursors or Binding escape.
// The private proof-authenticated auxiliary read is only for this same receiver.
internal interface ILegacyFst28CNumberRetainedSourceDraft
{
    void ValidateExactReceiver(LegacyFst28CNumberTrueLanguageSourceDraft receiver,
        LegacyFst28CNumberInitialPage page,LegacyFst28CNumberInitialConsumerContext.Pool pool,
        CancellationToken token);
    void SealAfterActualPhase0(LegacyFst28CNumberTrueLanguageSourceDraft receiver,object proof,CancellationToken token);
    void EnterExclusive(LegacyFst28CNumberTrueLanguageSourceDraft receiver,CancellationToken token);
    int ReadActualParentAuxiliary(LegacyFst28CNumberTrueLanguageSourceDraft receiver,object actualParentProof,
        CancellationToken token);
    void ResetAfterActualNode0(LegacyFst28CNumberTrueLanguageSourceDraft receiver,object actualNumericProof,CancellationToken token);
    void PoisonAfterClaim(LegacyFst28CNumberTrueLanguageSourceDraft receiver);
    // Read-only original-parent validation only; not source/cursor mutation authority.
    void ValidateOriginalParentReadLease(LegacyFst28CNumberTrueLanguageSourceDraft receiver,
        LegacyFst28CNumberInitialPage page,LegacyFst28CNumberInitialConsumerContext.Pool pool,
        CancellationToken token);
    void ValidateLiveLease(LegacyFst28CNumberTrueLanguageSourceDraft receiver,
        LegacyFst28CNumberInitialPage page,LegacyFst28CNumberInitialConsumerContext.Pool pool,
        CancellationToken token);
}

public sealed partial class LegacyFst28CNumberInitialPage
{
    private readonly RetainedSourceState? retainedSource;
    internal void ValidateRetainedSourceReceiver(LegacyFst28CNumberTrueLanguageSourceDraft receiver,
        LegacyFst28CNumberInitialConsumerContext.Pool pool,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();receiver.RequireWholeAdmission(token);
        if(retainedSource is null)throw new InvalidOperationException("Historical Page has no retained source authority.");
        ValidateIssuedRetainedSource(receiver,pool,retainedSource,token);
    }
    internal void ValidateIssuedRetainedSource(LegacyFst28CNumberTrueLanguageSourceDraft receiver,
        LegacyFst28CNumberInitialConsumerContext.Pool pool,ILegacyFst28CNumberRetainedSourceDraft source,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();receiver.RequireWholeAdmission(token);
        if(retainedSource is null||!ReferenceEquals(retainedSource,source)||
            !ReferenceEquals(retainedSource.Receiver,receiver)||!ReferenceEquals(retainedSource.Pool,pool)||
            !ReferenceEquals(retainedSource.Page,this)||!ReferenceEquals(Binding.D0,receiver.ExpectedD0))
            throw new ArgumentException("Only this actual Page's privately issued source holder is admissible.");
    }
    private sealed partial class RetainedSourceState:ILegacyFst28CNumberRetainedSourceDraft
    {
        internal readonly LegacyFst28CNumberTrueLanguageSourceDraft Receiver;
        internal readonly LegacyFst28CNumberInitialConsumerContext.Pool Pool;
        internal readonly LegacyFst28CNumberInitialPage Page;
        private readonly Input input;
        private readonly SourceCursor[] ring;
        private readonly ushort[] current;
        private readonly ulong[] sourceNodes;
        private readonly int nextCursor;
        private int issued,entered;
        // Captured only by the old Engine while live, issued only after its
        // one-way close. Actual objects/arrays, not diagnostic reconstruction.
        internal RetainedSourceState(LegacyFst28CNumberInitialPage page,
            LegacyFst28CNumberTrueLanguageSourceDraft receiver,
            LegacyFst28CNumberInitialConsumerContext.Pool pool,Input input,
            SourceCursor[] ring,ushort[] current,ulong[] sourceNodes,int nextCursor)
        {
            Page=page;Receiver=receiver;Pool=pool;this.input=input;this.ring=ring;
            this.current=current;this.sourceNodes=sourceNodes;this.nextCursor=nextCursor;
            // No Engine, Level, CancellationToken, delegate or captured closure.
        }
        internal void MarkIssuedAfterEngineClose()
        {
            if(Interlocked.CompareExchange(ref issued,1,0)!=0)
                throw new InvalidOperationException("Retained source issuance was already completed.");
        }
        public void ValidateExactReceiver(LegacyFst28CNumberTrueLanguageSourceDraft receiver,
            LegacyFst28CNumberInitialPage page,LegacyFst28CNumberInitialConsumerContext.Pool pool,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();receiver.RequireWholeAdmission(token);
            page.ValidateIssuedRetainedSource(receiver,pool,this,token);
            if(!ReferenceEquals(Receiver,receiver)||!ReferenceEquals(Page,page)||!ReferenceEquals(Pool,pool)||
                !ReferenceEquals(input.Binding,page.Binding)||!ReferenceEquals(input.D0,receiver.ExpectedD0)||
                !ReferenceEquals(input.Context,pool.OwnedContext)||!ReferenceEquals(input.Outgoing,page.Binding.Source)||
                Volatile.Read(ref issued)!=1||ring.Length!=8||current.Length!=16||sourceNodes.Length!=16||nextCursor!=3||
                pool.OwnedContext.NodeCount is <18 or >30)
                throw new InvalidDataException("Exact retained source/Input/private context association differs.");
            for(ushort label=0;label<16;label++)
            {
                token.ThrowIfCancellationRequested();
                if(current[label]!=page.Binding.ReadCurrentCode(label)||sourceNodes[label]!=page.Binding.ReadSourceNode(label))
                    throw new InvalidDataException("Moved maps/nodes differ from the same original binding.");
            }
            for(int slot=0;slot<8;slot++)
            {
                token.ThrowIfCancellationRequested();var cursor=ring[slot];
                if(cursor.Rows.Length!=12||cursor.Mapped.Length!=12||cursor.Count is <0 or >12||
                    cursor.Physical<0||cursor.Physical>cursor.Count||cursor.Accepted<0||cursor.Accepted>cursor.Count)
                    throw new InvalidDataException("Actual bounded source cursor differs.");
                if(slot>=3)
                {
                    // Never inspect/promote unused row-buffer contents.
                    if(cursor.Owner is not null||cursor.AuxiliaryLoaded is not null||cursor.Count!=0||cursor.Physical!=0||cursor.Accepted!=0)
                        throw new InvalidDataException("Previously unused actual source slot differs.");
                    continue;
                }
                if(cursor.AuxiliaryLoaded is not null)throw new InvalidDataException("AUX owner appeared before initial handoff.");
                var owner=cursor.Owner??throw new InvalidDataException("Missing original source-frame owner.");
                uint state=slot==0?0u:slot==1?97u:65u;
                ulong tag=slot==0?8UL<<40:state|(1UL<<32)|(slot==1?1UL<<40:0);
                if(!ReferenceEquals(owner.Input,input)||owner.Slot!=slot||owner.State!=state||
                    !ReferenceEquals(owner.Query.CachedOwner,owner)||!ReferenceEquals(owner.Query.Header,input.Header)||
                    !ReferenceEquals(owner.Query.Cohort,input.Cohort)||!ReferenceEquals(owner.Query.InitialRequest,input.Request)||
                    owner.Query.Tag!=tag||cursor.Count!=(slot==0?1:slot==1?3:10)||
                    cursor.Physical!=cursor.Count||owner.Query.AcceptedSkip!=cursor.Accepted)
                    throw new InvalidDataException("Original frame/source-ring identity differs.");
                for(int i=0;i<cursor.Count;i++)
                {
                    token.ThrowIfCancellationRequested();var row=cursor.Rows[i];
                    if(row.SourceLabel>=16||row.TargetState>=98||cursor.Mapped[i]!=current[row.SourceLabel])
                        throw new InvalidDataException("Original populated cursor row/map differs.");
                }
            }
        }
        // Distinct from initial pristine-state validation. Query operations
        // evolve physical/accepted cursors and evict original ring owners.
        // No state-ID/snapshot can issue this private holder or reopen entered.
        public void ValidateLiveLease(LegacyFst28CNumberTrueLanguageSourceDraft receiver,
            LegacyFst28CNumberInitialPage page,LegacyFst28CNumberInitialConsumerContext.Pool pool,CancellationToken token)
        {
            // Dirty/cross-thread capabilities refuse BEFORE reading live buffers.
            // Before actual phase0 seal, original deep checks remain mandatory.
            if(frozenReadset is not null)frozenReadset.RequireClean(this,Volatile.Read(ref actualNode0EndReset)==2);
            ValidateFullLiveLease(receiver,page,pool,token);
        }
        private void ValidateFullLiveLease(LegacyFst28CNumberTrueLanguageSourceDraft receiver,
            LegacyFst28CNumberInitialPage page,LegacyFst28CNumberInitialConsumerContext.Pool pool,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();receiver.RequireWholeAdmission(token);
            page.ValidateIssuedRetainedSource(receiver,pool,this,token);
            receiver.ValidateLiveOwner(this,page,pool,token);
            if(!ReferenceEquals(Receiver,receiver)||!ReferenceEquals(Page,page)||!ReferenceEquals(Pool,pool)||
                !ReferenceEquals(input.Binding,page.Binding)||!ReferenceEquals(input.D0,receiver.ExpectedD0)||
                !ReferenceEquals(input.Context,pool.OwnedContext)||!ReferenceEquals(input.Outgoing,page.Binding.Source)||
                Volatile.Read(ref issued)!=1||Volatile.Read(ref entered)!=1||
                ring.Length!=8||current.Length!=16||sourceNodes.Length!=16||pool.OwnedContext.NodeCount is <18 or >30)
                throw new InvalidDataException("Exact live retained source lease/receiver differs.");
            // Values/counts can evolve; exact identities and bounded shape may
            // not. Only the original private owner can mutate these arrays.
            for(int slot=0;slot<8;slot++)
            {
                token.ThrowIfCancellationRequested();var cursor=ring[slot];
                if(cursor.Rows.Length!=12||cursor.Mapped.Length!=12||cursor.Count is <0 or >12||
                    cursor.Physical<0||cursor.Physical>cursor.Count||cursor.Accepted<0||cursor.Accepted>cursor.Count)
                    throw new InvalidDataException("Actual live source cursor bounds differ.");
                if(cursor.NumberEndResetDomains is not null)
                {
                    ValidateActualEndResetDomains(cursor.NumberEndResetDomains,slot);
                    continue; // Counts/rows remain retained storage, not a valid loaded-state key.
                }
                var owner=cursor.Owner;
                if(cursor.AuxiliaryLoaded is not null)
                {
                    if(owner is not null)throw new InvalidDataException("Two source-owner modes share a live cursor.");
                    ValidateAuxiliaryOwner(cursor.AuxiliaryLoaded,cursor,slot,token);
                    continue;
                }
                if(owner is null)
                {
                    if(cursor.Count!=0||cursor.Physical!=0||cursor.Accepted!=0)
                        throw new InvalidDataException("Unowned live source cursor contains active state.");
                    continue; // Unused row buffer bytes are not examined.
                }
                if(!ReferenceEquals(owner.Input,input)||owner.Slot!=slot||owner.State>=98||
                    !ReferenceEquals(owner.Query.CachedOwner,owner)||!ReferenceEquals(owner.Query.Header,input.Header)||
                    !ReferenceEquals(owner.Query.Cohort,input.Cohort)||!ReferenceEquals(owner.Query.InitialRequest,input.Request)||
                    (uint)owner.Query.Tag!=owner.State||owner.Query.AcceptedSkip!=cursor.Accepted)
                    throw new InvalidDataException("Live source frame/header/cohort association differs.");
                for(int i=0;i<cursor.Count;i++)
                {
                    token.ThrowIfCancellationRequested();var row=cursor.Rows[i];
                    if(row.SourceLabel>=16||row.TargetState>=98||cursor.Mapped[i]!=current[row.SourceLabel])
                        throw new InvalidDataException("Live populated source row/map association differs.");
                }
            }
            ValidateIssuedAuxiliaryPages(token);
        }
        public void EnterExclusive(LegacyFst28CNumberTrueLanguageSourceDraft receiver,CancellationToken token)
        {
            ValidateExactReceiver(receiver,Page,Pool,token);
            if(Interlocked.CompareExchange(ref entered,1,0)!=0)
                throw new InvalidOperationException("Actual retained source lease was already entered or poisoned.");
            // No source cursor operation or result follows in this draft.
        }
    }
}
