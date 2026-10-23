namespace InkRecognition;

// CLOSED SOURCE SHADOW ONLY. Every prior full validation remains active.
// This is owned scalar storage, NOT a fast-validation or whole-admission grant.
internal sealed partial class LegacyFst28CNumberTrueLanguageSourceDraft
{
    internal void ValidateActualPhase0Seal(object proof,CancellationToken token)
    {
        RequireWholeAdmission(token);
        if(proof is not Phase0State p||!ReferenceEquals(p.receiver,this)||
            !p.contextPathsValidated||Volatile.Read(ref phase0Issued)!=1||
            p.descriptorCount!=0||p.descriptorCapacity!=124)
            throw new InvalidDataException("Only completed actual private phase0 may seal the readset.");
    }
}

public sealed partial class LegacyFst28CNumberInitialPage
{
    private sealed partial class RetainedSourceState
    {
        private FrozenNumberReadset? frozenReadset;
        private long readsetEpoch,readsetVerifiedEpoch;
        private int readsetSealIssued;
        private readonly record struct FrozenContext(ulong Identity,ulong ParentLink,ulong ChildTreeRoot,
            ulong Left,ulong Right,ushort Symbol,sbyte Balance,int PageIndex,int PageSlot);
        private sealed class FrozenNumberReadset
        {
            private readonly RetainedSourceState issuer;
            private readonly int thread;
            private readonly ushort[] primary=new ushort[16],secondary=new ushort[16];
            private readonly ulong[] nodes=new ulong[16];
            private readonly FrozenContext[] context;
            private readonly int[] offsets=new int[99];
            private readonly LegacyFst13AOutgoingRow[] outgoing=new LegacyFst13AOutgoingRow[299];
            private int revoked,poisoned;
            internal FrozenNumberReadset(RetainedSourceState issuer,CancellationToken token)
            {
                this.issuer=issuer;thread=Environment.CurrentManagedThreadId;
                if(issuer.input.Outgoing.StateCount!=98||issuer.input.Outgoing.TransitionWordCount!=299)
                    throw new InvalidDataException("Exact bounded outgoing source required before snapshot.");
                int count=issuer.Pool.OwnedContext.NodeCount;
                if(count is <18 or >30)throw new InvalidDataException("Bounded actual private context required.");
                // Historical same-context values are checked against fresh live Views;
                // only private scalar structs survive this constructor.
                var snapshots=issuer.Receiver.ExpectedConsumer.Context.ReadActiveContextNodes();
                if(snapshots.Count!=count)throw new InvalidDataException("Private context changed during seal.");
                context=new FrozenContext[count];
                for(int i=0;i<count;i++)
                {
                    token.ThrowIfCancellationRequested();var n=issuer.Pool.OwnedContext.ReadNode(snapshots[i].Identity);
                    if(n!=snapshots[i])throw new InvalidDataException("Live phase0 context fields differ from original owned topology.");
                    context[i]=new(n.Identity,n.ParentLink,n.ChildTreeRoot,n.Left,n.Right,n.Symbol,n.Balance,n.PageIndex,n.PageSlot);
                    for(int j=0;j<i;j++)if(context[j].Identity==n.Identity)
                        throw new InvalidDataException("Private context identities must be unique.");
                }
                for(ushort i=0;i<16;i++)
                {
                    token.ThrowIfCancellationRequested();primary[i]=issuer.current[i];nodes[i]=issuer.sourceNodes[i];
                    secondary[i]=issuer.input.D0.Policy.MapSecondary(i).Code;
                }
                int written=0;
                for(uint state=0;state<98;state++)
                {
                    token.ThrowIfCancellationRequested();offsets[(int)state]=written;
                    var actual=issuer.input.Outgoing.Query(state,12,192,token);
                    if(actual.State!=state||actual.OutputFlag!=0||actual.Count>12||actual.Count>299-written)
                        throw new InvalidDataException("Actual outgoing snapshot bounds differ.");
                    for(int j=0;j<actual.Count;j++)outgoing[written++]=actual.GetRow(j);
                }
                offsets[98]=written;
                if(written!=299)throw new InvalidDataException("Complete actual outgoing readset required.");
            }
            internal void RequireClean(RetainedSourceState actual,bool allowRevoked=false)
            {
                if(!ReferenceEquals(issuer,actual)||thread!=Environment.CurrentManagedThreadId||
                    Volatile.Read(ref poisoned)!=0||!allowRevoked&&Volatile.Read(ref revoked)!=0||
                    Volatile.Read(ref actual.readsetEpoch)!=Volatile.Read(ref actual.readsetVerifiedEpoch))
                    throw new InvalidOperationException("Private readset issuer/thread/epoch is dirty or revoked.");
                if(actual.Pool.OwnedContext.NodeCount!=context.Length)
                    throw new InvalidDataException("Private context count changed after seal.");
                for(ushort i=0;i<16;i++)
                    if(primary[i]!=actual.current[i]||nodes[i]!=actual.sourceNodes[i]||
                        secondary[i]!=actual.input.D0.Policy.MapSecondary(i).Code)
                        throw new InvalidDataException("Readset map shadow differs.");
            }
            internal void CompareOutgoing(LegacyFst13AOutgoingResult actual)
            {
                if(actual.State>=98||actual.Count!=offsets[(int)actual.State+1]-offsets[(int)actual.State])
                    throw new InvalidDataException("Owned outgoing shadow count differs.");
                for(int j=0;j<actual.Count;j++)if(actual.GetRow(j)!=outgoing[offsets[(int)actual.State]+j])
                    throw new InvalidDataException("Owned outgoing shadow row differs.");
            }
            internal ulong ReadCurrentParent(RetainedSourceState actual,ulong identity,CancellationToken token)
            {
                token.ThrowIfCancellationRequested();RequireClean(actual);
                foreach(var n in context)if(n.Identity==identity)return n.ParentLink;
                throw new InvalidDataException("Foreign context identity outside private frozen readset.");
            }
            internal void Revoke(){Volatile.Write(ref revoked,1);}
            internal void Poison(){Volatile.Write(ref poisoned,1);Volatile.Write(ref revoked,1);}
        }
        public void SealAfterActualPhase0(LegacyFst28CNumberTrueLanguageSourceDraft receiver,object proof,CancellationToken token)
        {
            receiver.ValidateActualPhase0Seal(proof,token);
            ValidateFullLiveLease(receiver,Page,Pool,token);
            if(Interlocked.CompareExchange(ref readsetSealIssued,1,0)!=0)
                throw new InvalidOperationException("Private readset seal already consumed.");
            try
            {
                var candidate=new FrozenNumberReadset(this,token);
                ValidateFullLiveLease(receiver,Page,Pool,token);
                token.ThrowIfCancellationRequested();frozenReadset=candidate;
            }
            catch{FailAuxiliaryOperation();throw;}
        }
        private ulong ReadCurrentContextParent(AuxiliaryFrame frame,AuxiliaryPageReceipt page,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();RequireFrozenReadset().RequireClean(this);
            Receiver.ValidateLiveOwner(this,Page,Pool,token);
            Page.ValidateIssuedRetainedSource(Receiver,Pool,this,token);
            if(!ReferenceEquals(frame,Frame(frame.Depth))||!ReferenceEquals(page,frame.CurrentPage)||
                !ReferenceEquals(input.Binding,Page.Binding)||!ReferenceEquals(input.D0,Receiver.ExpectedD0)||
                !ReferenceEquals(input.Context,Pool.OwnedContext)||!ReferenceEquals(input.Outgoing,Page.Binding.Source))
                throw new InvalidDataException("Exact current private frame/source identity required.");
            if(!ReferenceEquals(page,ValidateCurrentAuxiliaryPage(frame))||page.Consumed<=0)
                throw new InvalidDataException("Current context read requires a consumed issued row.");
            var actual=frame.Rows[page.Consumed-1];
            if(actual.Symbol!=65531||actual.ContextIdentity==0)
                throw new InvalidDataException("Only current consumed FFFB row supplies context identity.");
            return RequireFrozenReadset().ReadCurrentParent(this,actual.ContextIdentity,token);
        }
        public void ValidateOriginalParentReadLease(LegacyFst28CNumberTrueLanguageSourceDraft receiver,
            LegacyFst28CNumberInitialPage page,LegacyFst28CNumberInitialConsumerContext.Pool pool,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();RequireFrozenReadset().RequireClean(this);
            // Last complete successful seal/mutation postscan certifies all raw
            // cursors/current frame-page associations at this SAME clean epoch.
            // No observer callback occurs within the private parent-read loop.
            receiver.RequireWholeAdmission(token);
            page.ValidateIssuedRetainedSource(receiver,pool,this,token);
            receiver.ValidateLiveOwner(this,page,pool,token);
            if(!ReferenceEquals(Receiver,receiver)||!ReferenceEquals(Page,page)||!ReferenceEquals(Pool,pool)||
                !ReferenceEquals(input.Binding,page.Binding)||!ReferenceEquals(input.D0,receiver.ExpectedD0)||
                !ReferenceEquals(input.Context,pool.OwnedContext)||!ReferenceEquals(input.Outgoing,page.Binding.Source)||
                Volatile.Read(ref issued)!=1||Volatile.Read(ref entered)!=1||
                Volatile.Read(ref auxiliaryFailed)!=0||Volatile.Read(ref actualNode0EndReset)!=0||
                Volatile.Read(ref auxiliaryOperation)!=0||
                ring.Length!=8||current.Length!=16||sourceNodes.Length!=16)
                throw new InvalidDataException("Exact sealed original-parent read-only lease required.");
            // Deliberately NO raw-slot/frame payload scan here. Any supported
            // source mutation dirties epoch before writes and commits only after
            // the unchanged complete ValidateFullLiveLease postscan.
        }
        private FrozenNumberReadset RequireFrozenReadset()
        {return frozenReadset??throw new InvalidOperationException("Actual phase0 has not sealed its private readset.");}
        private void BeginReadsetMutation()
        {
            RequireFrozenReadset().RequireClean(this);
            readsetEpoch=checked(readsetEpoch+1);
        }
        private void CommitReadsetMutation(CancellationToken token)
        {
            // Intentionally keeps the old global validation; no scan is cached.
            ValidateFullLiveLease(Receiver,Page,Pool,token);
            token.ThrowIfCancellationRequested();readsetVerifiedEpoch=readsetEpoch;
            RequireFrozenReadset().RequireClean(this);
        }
    }
}
