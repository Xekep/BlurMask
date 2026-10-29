namespace InkRecognition;
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    private LegacyNativeDelimiterQueryPage? graphPeriodicQueryPage;
    internal sealed class GraphPeriodicRows:ILegacyEnglishPeriodicSuccessorScanBudget
    {
        private readonly LegacyEnglishCaseNode0Completion owner;
        private readonly LegacyEnglishCaseInitialQueryPass pass;
        private readonly LegacyEnglishCaseNode0Ring ring;
        private readonly GraphPeriodicWorkspace workspace;
        private readonly int node,ringNode,issuedThread;
        private GraphPeriodicRows(LegacyEnglishCaseNode0Completion owner,int node,GraphPeriodicWorkspace workspace,CancellationToken token)
        {this.owner=owner;this.node=node;this.workspace=workspace;pass=owner.node1Pass!;ring=owner.ring!;ringNode=ring.CurrentNode;issuedThread=Environment.CurrentManagedThreadId;Require(token);}
        internal static GraphPeriodicRows Issue(LegacyEnglishCaseNode0Completion owner,int node,GraphPeriodicWorkspace workspace,CancellationToken token)
        {
            owner.ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,48);owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,48);
            return new(owner,node,workspace,token);
        }
        private void Require(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();if(!owner.graphPeriodicActive||!owner.graphLoopActive||owner.graphLoopNode!=node||
                Volatile.Read(ref owner.phase)!=1||owner.thread!=issuedThread||issuedThread!=Environment.CurrentManagedThreadId||
                !ReferenceEquals(owner.node1Pass,pass)||!ReferenceEquals(owner.ring,ring)||!ReferenceEquals(owner.graphPeriodicWorkspace,workspace)||
                ring.ActualNodeCount>120||ring.CurrentNode!=ringNode||ringNode!=node-1)
                throw new InvalidOperationException("Same synchronous periodic node/pass/ring/thread before materialization required.");
        }
        internal void RequireWorkspace(GraphPeriodicWorkspace actual,CancellationToken token)
        {Require(token);if(!ReferenceEquals(actual,workspace))throw new InvalidOperationException("Same workspace required.");}
        internal ReadOnlySpan<byte> Record(int ending,ushort ordinal,CancellationToken token)
        {
            Require(token);if(ending<0||ending>=node||(uint)ordinal>=(uint)ring.SourceEndAt(ending))throw new InvalidDataException("Actual earlier retained source row required.");
            owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,88);return ring.OwnedRowsAt(ending).Slice(ordinal*88,88);
        }
        internal ReadOnlySpan<byte> Primary(int ending,ushort ordinal,CancellationToken token)
        {
            Require(token);if(ending<0||ending>=node||ordinal>=125)throw new InvalidDataException("Actual earlier primary alternatives required.");
            owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,80);return ring.OwnedPrimaryAt(ending).Slice(ordinal*80,80);
        }
        internal void RequirePage(ReadOnlySpan<byte> query,ReadOnlySpan<byte> parent,int previous,ushort ordinal,
            LegacyOrdinaryRecognitionGraph graph,LegacySymbolContextFactory contexts,LegacyInkAlphabet alphabet,CancellationToken token)
        {
            Require(token);var page=owner.graphPeriodicQueryPage;
            if(page is null||owner.graphLoopQuery is null||owner.graphLoopLease is null||previous!=owner.graphLoopPrevious||ordinal!=owner.graphLoopParent||
                !ReferenceEquals(graph.Identity,owner.graphLoopGeometry!.ActualGraph)||!ReferenceEquals(contexts,pass.Initializer.PlanForAudit.BindingForAudit.Contexts)||
                !ReferenceEquals(alphabet,pass.Root.Source.Alphabet)||query.Length!=page.Records.Length||
                query.Length!=0&&(!query.Overlaps(page.Records.Span,out int qoffset)||qoffset!=0))
                throw new InvalidOperationException("Only the privately installed current actual page may consume periodic parents.");
            var actual=Record(previous,ordinal,token);if(parent.Length!=88||!parent.Overlaps(actual,out int offset)||offset!=0)
                throw new InvalidOperationException("Same live retained parent span required.");
        }
        internal void ChargePage(int bytes,int successors)
        {
            Require(default);owner.ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,checked(256L+successors*4L));
            owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,checked(256L+successors*4L));owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,checked(bytes+successors*4L));
        }
        void ILegacyEnglishPeriodicSuccessorScanBudget.BeforeSuccessorRead(CancellationToken token)
        {Require(token);owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,LegacyEnglishPeriodicSuccessorScan.VisitsPerRead);}
        internal void ChargeRequest()
        {
            Require(default);owner.ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,256);
            owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,512);owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,1024);
        }
    }
}
public static partial class LegacyInitialWordHeapTail
{
    internal static void InitializePeriodic(Span<byte> heapSlot,LegacyEnglishCaseNode0Completion.GraphPeriodicRows rows,CancellationToken token)
    {
        if(heapSlot.Length!=128)throw new ArgumentException("Actual128 slot required.");
        System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(heapSlot[88..],ulong.MaxValue);int previous=-1;ulong context=0;
        int anchorNode=System.Buffers.Binary.BinaryPrimitives.ReadInt32LittleEndian(heapSlot[56..]);ushort anchorOrdinal=System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(heapSlot[60..]);
        ReadOnlySpan<byte> anchor=heapSlot[..88];if(anchorOrdinal!=ushort.MaxValue)anchor=rows.Record(anchorNode,anchorOrdinal,token);
        previous=unchecked(~unchecked((sbyte)anchor[32])+anchorNode);
        if(previous>=0){ushort ordinal=System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(anchor[30..]);context=System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(rows.Record(previous,ordinal,token)[40..]);}
        System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(heapSlot[104..],previous);System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(heapSlot[112..],context);
    }
}
public sealed partial class LegacyNativeWordHeapRouter
{
    internal LegacyNativeWordHeapAllocation TryInsertPeriodicInitialRecord(LegacyInitialWordRecordFields fields,
        LegacyEnglishCaseNode0Completion.GraphPeriodicRows rows,CancellationToken token)
    {
        rows.ChargeRequest();var prefix=LegacyInitialWordRecordProducer.InitializeAllocatedPrefix(fields,0,token);
        return TryAllocate(1,0,false,fields.CandidateCost10,slot=>{prefix.CopyTo(slot);LegacyInitialWordHeapTail.InitializePeriodic(slot,rows,token);},token);
    }
}
