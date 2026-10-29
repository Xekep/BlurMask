using System.Buffers.Binary;
namespace InkRecognition;
internal readonly record struct LegacyEnglishCaseGraphLanguageInput(int Node,int Ordinal,ulong CurrentTag,ushort CurrentEnding,int AnchorNode,int AnchorOrdinal,int PreviousWordNode,bool IsTerminal);
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    private GraphLanguageScope? graphLanguageScope;
    // Connected SOURCE checkpoint: invoked before EndNode1 and before phase2,
    // from SAME freshly produced heap rows, never caller-returned snapshots.
    private LegacyEnglishCaseGraphLanguageInput[] PrepareGraphLanguageInputs(LegacyEnglishFreshCaseMainSource.Node1QueryLease lease,int node,
        LegacyEnglishCaseNode1Materialization.Images images,LegacyCursiveCategoryProfile categories,bool executeFirst,CancellationToken token,int primaryMinimum=0x4000000,GraphLoopOutput output=GraphLoopOutput.FinalSnapshot)
    {
        if(output==GraphLoopOutput.PeriodicWordSourceOnly&&(!graphPeriodicActive||!graphLoopActive||!graphWordSourceRequested||!graphFinalCompactRequested))
            throw new InvalidOperationException("Private running periodic WORD-source execution required.");
        RequireNode1(lease.Root,lease.Pass);lease.Root.ValidateNode1(lease);token.ThrowIfCancellationRequested();
        if(ring is null||node!=ring.CurrentNode+1||node>=ring.ActualNodeCount||images.Count is <0 or >125||images.Rows88.Length!=125*88||images.Alternatives80.Length!=125*80||images.LiveDescriptor is null||images.LiveDescriptor.AllocatedCapacity!=250||images.LiveDescriptor.Count!=images.Count||images.LivePrimaryOrdinals is null||images.LivePrimaryOrdinals.Length!=156||graphLanguageScope is not null)
            throw new InvalidOperationException("Exact same running producer/ring/current-node language boundary required.");
        // This entry currently joins the owning ordinary1F8D60 path only.
        // Its zeroed0..119 -> written88 prefix is fully defined. This theorem
        // does NOT apply to historical FIRST rows or arbitrary copied88 data.
        if(!graphLoopActive)ring.EnableGraphCursor(token);ring.EnterGraphNode(node,token);
        ledger.CopyBytes(LegacyEnglishCaseNode0Ledger.Site.StoredBlocks,images.Rows88,ring.CurrentRows);
        ledger.CopyBytes(LegacyEnglishCaseNode0Ledger.Site.PrimaryBlocks,images.Alternatives80,ring.CurrentPrimary);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.CurrentRowsMask,125*88);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.CurrentPrimaryMask,125*80);
        LegacyEnglishCaseGraphPrimaryDefinedness.Fill(images.Count,images.Alternatives80,ring.CurrentRowMask.AsSpan(0,125*88),ring.CurrentPrimaryMask);
        ring.ClearNode0Counters(125,125);
        if(graphLoopActive)ring.ClearCurrent3e(token); // actual21558D store even inactive primary rows
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,72);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,136);
        var scope=new GraphLanguageScope(this,lease,node,images.LiveDescriptor,images.InitialCount,images.LivePrimaryOrdinals,primaryMinimum);graphLanguageScope=scope;
        try
        {
            if(System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<LegacyEnglishCaseGraphLanguageInput>()||System.Runtime.CompilerServices.Unsafe.SizeOf<LegacyEnglishCaseGraphLanguageInput>()>48)throw new NotSupportedException("Reference-free bounded language diagnostic slot required.");
            if(!graphPeriodicActive){ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,checked(images.Count*48L));ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,checked(images.Count*48L));}
            var result=graphPeriodicActive?Array.Empty<LegacyEnglishCaseGraphLanguageInput>():new LegacyEnglishCaseGraphLanguageInput[images.Count];
            for(int ordinal=0;ordinal<images.Count;ordinal++)
            {
                scope.Require(token);var row=ring.CurrentRows.Slice(ordinal*88,88);
                var anchor=LegacyEnglishCaseLanguageAnchor.Resolve(ring,node,ordinal,token);
                // CURRENT00 feeds source70; anchorTag is never substituted.
                if(!graphPeriodicActive)result[ordinal]=new(node,ordinal,BinaryPrimitives.ReadUInt64LittleEndian(row),BinaryPrimitives.ReadUInt16LittleEndian(row[36..]),
                    anchor.AnchorNode,anchor.AnchorOrdinal,anchor.PreviousWordNode,(row[5]&2)!=0&&(row[7]&1)==0);
            }
            if(executeFirst)
            {
                if(!graphLoopActive||GraphLanguageRequired(scope,images.Count,token))
                {ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,8);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,8);RunGraphLanguageFirstOnly(scope,images.Count,categories,token);}
                else SealGraphEmptyLanguage(scope,token);
                _=CompleteGraphPhase1(scope,scope.Ordinals,token);
                // Native215DDE207CB0 precedes the following215E92 sourceV80.
                if(graphFinalCompactRequested&&node==ring.ActualNodeCount-1)
                {scope.SealCompactPhase();if(graphPeriodicActive)RunGraphPeriodicFinalCompact(scope,token);else RunGraphFinalCompact(scope,token);}
                scope.Lease.Pass.Initializer.PlanForAudit.ResetGraphNode(scope,token);
                if(!graphPeriodicActive||node==ring.ActualNodeCount-1&&output==GraphLoopOutput.FinalSnapshot)graphCompletedNode=LegacyEnglishCaseGraphNodeSnapshot.Capture(scope,token);
                else
                {
                    scope.RequireCompletedNode();
                    if(node==ring.ActualNodeCount-1)
                    {
                        // Preserve final Capture's owner/pattern/cancellation guards, without allocating discarded DATA.
                        scope.RequireFirstOnlyPattern();token.ThrowIfCancellationRequested();
                        scope.RequireCompletedNode();token.ThrowIfCancellationRequested();
                    }
                    graphPeriodicCompletedNode=node;
                } // This typed execution path consumes no detached node DATA.
            }
            scope.Require(token);return result; // historical value-only diagnostics
        }
        finally{graphLanguageScope=null;}
    }
    // Unforgeable active-reference authority: even an internal/friend-created
    // instance cannot satisfy owner's graphLanguageScope identity check.
    internal sealed partial class GraphLanguageScope
    {
        private readonly LegacyEnglishCaseNode0Completion owner;
        private readonly LegacyEnglishFreshCaseMainSource.Node1QueryLease lease;
        private readonly int node;
        private int acceptedClosed,phase1Entered,initializedN=-1,nodeEndReset;
        private int materializedMinimum;
        internal bool PeriodicEnabled=>owner.graphPeriodicActive;
        internal int MaterializedMinimum{get{Require();return materializedMinimum;}}
        internal void PublishMaterializedMinimum(int value){RequirePhase1();materializedMinimum=value;}
        private int firstOnlyPatternSealed=0;

        internal LegacyEnglishCaseNode0Completion Owner=>owner;
        internal int CurrentNode=>node;
        internal LegacyEnglishCaseNode0Ring Ring=>owner.ring??throw new InvalidOperationException("No live ring.");
        internal LegacyEnglishCaseNode0Ledger Ledger=>owner.ledger;
        internal LegacyEnglishFreshCaseMainSource.Node1QueryLease Lease=>lease;
        internal LegacyNativeWordHeapDescriptor Descriptor{get;}
        internal int OriginalCountT{get;}
        internal int[] Ordinals{get;}
        internal int InitializedCountN=>initializedN;
        internal GraphLanguageScope(LegacyEnglishCaseNode0Completion owner,LegacyEnglishFreshCaseMainSource.Node1QueryLease lease,int node,LegacyNativeWordHeapDescriptor descriptor,int t,int[] ordinals,int primaryMinimum)
        {this.owner=owner;this.lease=lease;this.node=node;Descriptor=descriptor;OriginalCountT=t;Ordinals=ordinals;materializedMinimum=primaryMinimum;}
        internal void Require(CancellationToken token=default)
        {
            token.ThrowIfCancellationRequested();owner.RequireNode1(lease.Root,lease.Pass);lease.Root.ValidateNode1(lease);
            if(!ReferenceEquals(owner.graphLanguageScope,this)||owner.ring is null||owner.ring.CurrentNode!=node)
                throw new InvalidOperationException("Same active generic language stage/current ring required.");
        }
        internal void RequireNumericHandles(LegacyEnglishCaseInitializationNumericTables.Node0Handles actual)
        {Require();if(!ReferenceEquals(owner.numericHandles,actual))throw new InvalidOperationException("Exact privately prepared generic numeric handles required.");}
        internal void RequireAccepting()
        {Require();if(acceptedClosed!=0||phase1Entered!=0)throw new InvalidOperationException("Live accepting language prefix required.");}
        internal void RequireOwnedFirstWriter(LegacyNativeWordSlotInitializer writer)
        {RequireAccepting();if(!ReferenceEquals(owner.graphFirstWriter,writer)||owner.graphFirstBusy!=1||!ReferenceEquals(owner.graphFirstScope,this)||owner.graphFirstMaskValidated!=1)throw new InvalidOperationException("Exact temporarily bound generic FIRST writer required.");}
        internal void CloseAcceptedPrefix(int n)
        {RequireAccepting();if((uint)n>124||n>OriginalCountT||Descriptor.Count!=n)throw new InvalidDataException("Actual accepted N must be bounded by original T.");initializedN=n;acceptedClosed=1;}
        internal void ClaimPhase1()
        {Require();if(acceptedClosed!=1||initializedN<0||phase1Entered!=0)throw new InvalidOperationException("One sealed accepted-prefix phase1 required.");phase1Entered=1;}
        internal void SealCanonicalFirstOnly()
        {Require();if(acceptedClosed!=1||initializedN<0||initializedN>OriginalCountT||!owner.graphLanguageFirstOnly)throw new InvalidOperationException("Completed canonical FIRST-only helper prefix required.");firstOnlyPatternSealed=1;}
        internal void RequireFirstOnlyPattern()
        {Require();if(firstOnlyPatternSealed!=1||initializedN<0||initializedN>OriginalCountT)throw new NotSupportedException("Canonical accepted FIRST writer-mask provenance has not been sealed.");}
        private int compactPhaseReady;
        internal void SealCompactPhase()
        {
            RequirePhase1();RequireFirstOnlyPattern();
            if(!owner.graphFinalCompactRequested||node!=Ring.ActualNodeCount-1||nodeEndReset!=0||compactPhaseReady!=0)
                throw new InvalidOperationException("One actual LAST phase1 completion before V80 required.");
            compactPhaseReady=1;
        }
        internal void RequireCompactPhase()
        {
            RequirePhase1();
            if(compactPhaseReady!=1||nodeEndReset!=0||!owner.graphFinalCompactRequested||node!=Ring.ActualNodeCount-1)
                throw new InvalidOperationException("Same sealed LAST phase1 before following V80 required.");
        }
        internal void RequireCompletedNode()
        {RequirePhase1();if(Volatile.Read(ref nodeEndReset)!=1)throw new InvalidOperationException("Actual pernode endreset must precede detached capture.");}
        internal void ClaimEndReset(LegacyEnglishCaseInitializationPlan plan)
        {RequirePhase1();if(!ReferenceEquals(lease.Pass.Initializer.PlanForAudit,plan)||Interlocked.CompareExchange(ref nodeEndReset,1,0)!=0)throw new InvalidOperationException("One same-owner generic current-node reset required.");}
        internal void ChargeReset()
        {RequirePhase1();owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Reset,5632);owner.ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.Reset,4464);owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Reset,3076+576);}
        internal void RequirePhase1()
        {Require();if(acceptedClosed!=1||phase1Entered!=1)throw new InvalidOperationException("Active generic phase1 required.");}
        internal void ChargeHeapVisit(long count)=>owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,count);
        internal void ChargeHeapCopy(long count)=>owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,count);
    }
}
