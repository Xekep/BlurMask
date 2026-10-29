using System.Buffers.Binary;
namespace InkRecognition;
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    // Exact LAST-scope synchronous borrow. No Memory/array view of the ring is exported.
    internal sealed class GraphPeriodicCompactStore:ILegacyOwnedCompactFrameBudget
    {
        private readonly LegacyEnglishCaseNode0Completion owner;
        private readonly GraphLanguageScope stage;
        private readonly LegacyEnglishCaseNode0Ring ring;
        private readonly int thread;
        private readonly GraphPeriodicContextBudget contextBudget;
        private readonly byte[] storage;
        private readonly int[] starts,prefix;
        private GraphPeriodicCompactStore(GraphLanguageScope stage,int records,CancellationToken token)
        {
            owner=stage.Owner;this.stage=stage;ring=stage.Ring;contextBudget=owner.graphPeriodicContextBudget!;thread=Environment.CurrentManagedThreadId;Require(token);
            storage=stage.Ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.FinalImages,checked(records*40));
            starts=stage.Ledger.Ints(LegacyEnglishCaseNode0Ledger.Site.FinalImages,ring.ActualNodeCount+1);
            prefix=stage.Ledger.Ints(LegacyEnglishCaseNode0Ledger.Site.FinalImages,ring.ActualNodeCount+1);
        }
        internal static GraphPeriodicCompactStore Issue(GraphLanguageScope stage,int records,CancellationToken token)
        {
            stage.Owner.RequireGraphPublication(stage,token);
            if(!stage.PeriodicEnabled||records is<1 or>1048576)throw new InvalidOperationException("Actual periodic LAST compact allocation required.");
            stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,64);
            stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,64);
            return new(stage,records,token);
        }
        internal void Require(CancellationToken token)
        {
            owner.RequireGraphPublication(stage,token);
            if(!stage.PeriodicEnabled||!ReferenceEquals(stage.Ring,ring)||ring.CurrentNode!=ring.ActualNodeCount-1||
                thread!=Environment.CurrentManagedThreadId||!ReferenceEquals(owner.graphLanguageScope,stage))
                throw new InvalidOperationException("Same active LAST ring/root/pass/thread required.");
        }
        internal int NodeCount=>ring.ActualNodeCount;
        internal int SourceEnd(int node){Require(default);return ring.SourceEndAt(node);}
        internal Span<byte> Rows(int node){Require(default);return ring.OwnedRowsAt(node);}
        internal Span<byte> Primary(int node){Require(default);return ring.OwnedPrimaryAt(node);}
        internal ReadOnlySpan<byte> RowMask(int node){Require(default);return ring.OwnedRowMaskAt(node);}
        internal ReadOnlySpan<byte> PrimaryMask(int node){Require(default);return ring.OwnedPrimaryMaskAt(node);}
        // Only non-ring compact arrays leave this private owned path, for synchronous RawWord consumption.
        internal byte[] CompactStorage(){Require(default);return storage;}
        internal int[] Starts(){Require(default);return starts;}
        internal int[] PrefixOwners(){Require(default);return prefix;}
        internal void BeforeFrame(ulong current,ulong previous){Require(default);contextBudget.BeforeFrame(current,previous);}
        //Read-only fixed authority gate: reject escaped/revoked callers BEFORE
        //the non-thread-safe ledger is touched. Full store validation remains
        //after its explicit precharge; no structural proof is cached.
        private void RequireMeterAuthority(CancellationToken token)
        {
            stage.RequireCompactMeterIdentity(token);contextBudget.RequireMeterAuthority();
            if(!ReferenceEquals(owner.graphPeriodicContextBudget,contextBudget)||
                !ReferenceEquals(stage.Ring,ring)||thread!=Environment.CurrentManagedThreadId||
                ring.CurrentNode!=ring.ActualNodeCount-1)
                throw new InvalidOperationException("Same current private LAST meter authority required.");
            token.ThrowIfCancellationRequested();
        }
        internal byte[] CreateOrdinaryOwnedFrame(LegacySymbolContextFactory actualContexts,ulong current,ulong previous,
            ReadOnlySpan<byte> row,uint sourceNode,byte difference,LegacyPreparedCompactWordContextPolicy policy,
            Func<ushort,byte> category,CancellationToken token)
        {
            RequireMeterAuthority(token);contextBudget.Require(actualContexts);contextBudget.BeforeOwnedFrameEntryGate();
            Require(token);contextBudget.Require(actualContexts);contextBudget.BeforeOwnedFrame(current,previous,token);
            return LegacyPreparedCompactWordFrameProducer.CreateOrdinaryOwned(actualContexts,current,previous,row,sourceNode,difference,policy,category,this,token);
        }
        void ILegacyOwnedCompactFrameBudget.BeforeNormalize(ReadOnlySpan<byte> frame,CancellationToken token)
        {
            RequireMeterAuthority(token);contextBudget.BeforeOwnedFrameNormalizationGate();
#if ENGLISH_FRESH_CONTINUATION_CONTROLS
            LegacyFreshContinuationTestHooks.Checkpoint?.Invoke("before-owned-compact-normalize");
#endif
            Require(token);contextBudget.BeforeOwnedFrameNormalization(frame,token);
        }
        internal void Allocate(long bytes){Require(default);stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,bytes);}
        internal void Copy(long bytes){Require(default);stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,bytes);}
        internal void Visit(long units){Require(default);stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,units);}
        internal void ClearRowMark(int node,int ordinal)
        {Copy(2);var row=Rows(node);row[ordinal*88+6]&=0xfd;ring.OwnedRowMaskAt(node)[ordinal*88+6]|=2;}
        internal void ClearPrimaryMark(int node,int index)
        {Copy(2);Primary(node)[index*40+27]&=0x7f;ring.OwnedPrimaryMaskAt(node)[index*40+27]|=0x80;}
        internal void MarkRow(int node,int ordinal)
        {Copy(2);Rows(node)[ordinal*88+6]|=2;ring.OwnedRowMaskAt(node)[ordinal*88+6]|=2;}
        internal void MarkPrimary(int node,int ordinal,int alternative)
        {Copy(2);int at=(ordinal*2+alternative)*40+27;Primary(node)[at]|=0x80;ring.OwnedPrimaryMaskAt(node)[at]|=0x80;}
        internal void WriteRowOrdinal(int node,int ordinal,ushort value)
        {Copy(4);BinaryPrimitives.WriteUInt16LittleEndian(Rows(node)[(ordinal*88+34)..],value);ring.OwnedRowMaskAt(node).Slice(ordinal*88+34,2).Fill(255);}
        internal void WritePrimaryOrdinal(int node,int ordinal,int alternative,ushort value)
        {Copy(4);int at=(ordinal*2+alternative)*40+28;BinaryPrimitives.WriteUInt16LittleEndian(Primary(node)[at..],value);ring.OwnedPrimaryMaskAt(node).Slice(at,2).Fill(255);}
    }
    private int PrepareGraphPeriodicCompactCapacity(GraphLanguageScope stage,LegacyOrdinaryRecognitionGraph graph,
        LegacySymbolContextFactory contexts,CancellationToken token)
    {
        stage.RequireCompactPhase();if(!graphPeriodicActive||graph.NodeCount!=stage.Ring.ActualNodeCount||graph.NodeCount>120)
            throw new InvalidOperationException("Same distinct-slot periodic graph required.");
        int records=checked(graph.RequireSameSource().CandidateCount*2);
        for(int node=0;node<graph.NodeCount;node++)
        {
            int end=stage.Ring.SourceEndAt(node);var rows=stage.Ring.OwnedRowsAt(node);var masks=stage.Ring.OwnedRowMaskAt(node);
            var alternatives=stage.Ring.OwnedPrimaryAt(node);var alternativeMasks=stage.Ring.OwnedPrimaryMaskAt(node);
            if(end is<125 or>156)throw new InvalidDataException("Actual complete source23F0 extent required.");
            for(int ordinal=0;ordinal<end;ordinal++)
            {
                token.ThrowIfCancellationRequested();stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,128);
                int at=ordinal*88;Known(masks,at+16,4);if(BinaryPrimitives.ReadInt32LittleEndian(rows[(at+16)..])>=0x4000000)continue;
                Known(masks,at+8,8);records=checked(records+Math.Max(1,Length(BinaryPrimitives.ReadUInt64LittleEndian(rows[(at+8)..]))));
                if(ordinal>=125)continue;
                for(int k=0;k<2;k++)
                {
                    int p=ordinal*80+k*40;stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,64);Known(alternativeMasks,p,4);
                    if(BinaryPrimitives.ReadInt32LittleEndian(alternatives[p..])>=0x4000000)break;
                    Known(alternativeMasks,p+16,8);records=checked(records+Math.Max(1,Length(BinaryPrimitives.ReadUInt64LittleEndian(alternatives[(p+16)..]))));
                }
            }
        }
        if(records is<1 or>1048576)throw new NotSupportedException("Complete actual compact allocation exceeds existing domain.");
        return records;
        int Length(ulong identity)
        {
            int count=0;while(identity!=0)
            {
                token.ThrowIfCancellationRequested();if(count==50)throw new NotSupportedException("Actual context ancestry exceeds the native fifty-symbol frame.");
                stage.Ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,64);identity=contexts.ReadPeriodicParentLink(graphPeriodicContextBudget!,identity);count++;
            }
            return count;
        }
        static void Known(ReadOnlySpan<byte> mask,int at,int length)
        {for(int i=at;i<at+length;i++)if(mask[i]!=255)throw new NotSupportedException("Opaque cost/context cannot supply compact capacity.");}
    }
    private void RunGraphPeriodicFinalCompact(GraphLanguageScope stage,CancellationToken token)
    {
        stage.RequireCompactPhase();stage.RequireFirstOnlyPattern();
        if(!graphPeriodicActive||graphFinalCompactBusy||graphCompactData is not null||graphRawWord is not null||!graphWordSourceRequested)
            throw new InvalidOperationException("One actual periodic WORD-source LAST publication required.");
        var root=stage.Lease.Root;var graph=stage.Lease.Pass.Initializer.PlanForAudit.BindingForAudit.Graph;
        var contexts=stage.Lease.Pass.Initializer.PlanForAudit.BindingForAudit.Contexts;var policy=graphLoopSourcePolicy??throw new InvalidOperationException("Same source policy required.");
        var geometry=graphLoopGeometry??throw new InvalidOperationException("Same source geometry required.");var c=policy.Heaps.Configuration;
        if(root.DescriptorFlags!=0xc0e||root.NumericOwner.Profile!=2||root.NumericOwner.SourceScale!=1||c.HeaderCount2400!=2||
            c.PrimaryAlternativeCount2402!=2||c.SecondaryAlternativeCount2404!=1||c.PrimaryArcLimit230a!=125||c.Mode22f6!=0||
            c.Trace2368!=0||policy.Heaps.SecondaryShareEc!=0||root.Source.Alphabet.TryGetLabel("\U000f0009",out _))
            throw new NotSupportedException("Same ordinary2/2/1 no-sentinel selected source required.");
        int records=PrepareGraphPeriodicCompactCapacity(stage,graph,contexts,token);
        graphFinalCompactBusy=true;
        try
        {
            var store=GraphPeriodicCompactStore.Issue(stage,records,token);
            stage.Lease.Pass.Initializer.PlanForAudit.WithGraphPublicationReader(stage,reader=>
            {
                var reached=LegacyPreparedMainCompactReachability.RunPeriodic(store,c,graph.MaximumAtoms,reader,contexts.ContainerIdentity,token,5);
                var published=LegacyPreparedMainCompactPublication.RunPeriodic(store,c,graph,contexts,root.Source.Alphabet,geometry.Categories,
                    root.NumericOwner.FinalUnigram94D,reached.LanguageProbe,group=>LegacyMainCompactGroupScores.FromEnglishGraph(stage,group,token),token,[0,root.DescriptorFlags]);
                store.Require(token);
                var winner=LegacyNativeMainTerminalSelection.SelectSingleAlternativeWithoutRescue(stage.Ring.CurrentRows,
                    checked((ushort)stage.Ring.SourceEndAt(stage.CurrentNode)),[new(0,0),new(root.DescriptorFlags,root.NumericOwner.FinalUnigram94D)],
                    new(125,false,false,false,false,0),stage.Ring.ReadPeriodicCumulative(stage,stage.CurrentNode,token),contexts,token);
                graphRawWord=BuildGraphRawWord(stage,published,winner,token);store.Require(token);
            },token);
        }
        finally{graphFinalCompactBusy=false;}
    }
    internal sealed partial class GraphLanguageScope
    {
        internal void RequireCompactMeterIdentity(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            owner.RequireNode1(lease.Root,lease.Pass);
            if(!ReferenceEquals(owner.graphLanguageScope,this)||owner.ring is null||
                owner.ring.CurrentNode!=node||acceptedClosed!=1||phase1Entered!=1||
                compactPhaseReady!=1||nodeEndReset!=0||!owner.graphFinalCompactRequested||
                !owner.graphFinalCompactBusy||node!=owner.ring.ActualNodeCount-1)
                throw new InvalidOperationException("Same live compact phase meter identity required.");
            lease.Root.RequireNode1ContinuationIdentity(lease,token);
            token.ThrowIfCancellationRequested();
        }
    }
}
