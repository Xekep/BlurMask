using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglishCaseNode0Completion.Probe")]
namespace InkRecognition;

// CONCRETE SOURCE DRAFT ONLY. No mutable compile/runtime authorization. The
// final receipt remains deliberately refused after the real OLD Run completes.
// This increment makes lease/charge/ring/fault chronology reviewable; it is
// never an alternative default engine or a completed whole-node component.
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    private readonly LegacyEnglishCasePrimaryCompletion primary;
    private readonly LegacyEnglishCaseNode0Ledger ledger;
    private readonly LegacyEnglishCaseNode0LexiconIndex index;
    private readonly LegacyEnglishCaseNode0NativeFreshPolicy fresh;
    private readonly LegacyEnglishCaseInitializationNumericTables.Node0Handles numericHandles;
    private int numericPreparingThread;
    private readonly object stageIssuer=new();
    private LegacyEnglishCaseNode0Ring? ring;
    private Stage? activeStage;
    private int[]? storedOrdinals;
    private AuxiliaryState? auxiliaryState;
    private LegacyNativeWordHeapDescriptor.Node0Phase1Result? phase1Result;
    private int originalT=-1,initializedN=-1;
    private int phase,thread,initialized,completed,endReset;
    private bool pending;
    private CompletedBacking? completedBacking;
    private PendingReceipt? pendingReceipt;
    private int oldRunCompleted,exportClaimed;
    private LegacyEnglishCaseNode0Completion(LegacyEnglishCasePrimaryCompletion primary,LegacyEnglishCaseNode0Ledger ledger,LegacyEnglishCaseNode0LexiconIndex index,LegacyEnglishCaseNode0NativeFreshPolicy fresh)
    {
        this.primary=primary;this.ledger=ledger;this.index=index;this.fresh=fresh;
        numericPreparingThread=Environment.CurrentManagedThreadId;
        try{numericHandles=primary.PrepareNode0NumericHandles(this);}
        finally{numericPreparingThread=0;}
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FirstAcceptedWriter,48);
        firstWriter=WriteAcceptedCurrentFirst;
    }
    internal static LegacyEnglishCaseNode0Completion Prepare(LegacyEnglish119MainKnowledge knowledge,LegacyEnglishLkFreshNumericBinding numeric,
        LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication,long maximumLogicalBytes=128L*1024*1024,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(knowledge);ArgumentNullException.ThrowIfNull(numeric);
        // Fixed ledger construction precedes the OLD root claim. No resource
        // copy/hash/load, code-cache dictionary or terminal index is hidden here.
        var ledger=LegacyEnglishCaseNode0Ledger.Create(maximumLogicalBytes);
        var fresh=LegacyEnglishCaseNode0NativeFreshPolicy.Prepare(knowledge,numeric,ledger,token);
        var index=LegacyEnglishCaseNode0LexiconIndex.Prepare(numeric,ledger,token);
        // NEW immutable holder only: owner8 + packed44 + four code refs32.
        // OLD NumericTables constructor remains exact original. Seven lookups
        // execute only below through the exact private prepared-consumer seam.
        // 12256 is conditional .NET10.0.12 Release source work, not pack/CPU proof.
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.NumericHandleMetadata,84);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.NumericHandleMetadata,180);
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.NumericHandlePrepare,12256);
        // Additional node0 attachment field on the existing OLD primary type.
        // OLD standalone guards/reservations are not changed.
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,8);
        var primary=LegacyEnglishCasePrimaryCompletion.Prepare(knowledge,numeric,cases,publication,maximumLogicalBytes,token);
        try
        {
            ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,264+48+9); //source-index request bool+DATAref, before object allocation.
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,264+8+9);
            var result=new LegacyEnglishCaseNode0Completion(primary,ledger,index,fresh);token.ThrowIfCancellationRequested();primary.AttachNode0Completion(result);return result;
        }
        catch{primary.FaultUnattachedNode0Preparation();throw;}
    }
    internal void RequireNumericPreparation(LegacyEnglishCasePrimaryCompletion actual)
    {
        if(!ReferenceEquals(primary,actual)||Volatile.Read(ref phase)!=0||
            numericPreparingThread!=Environment.CurrentManagedThreadId||numericPreparingThread==0)
            throw new InvalidOperationException("Exact constructor-local numeric preparation owner/thread required.");
    }
    internal bool IsPreparedFor(LegacyEnglishCasePrimaryCompletion value)=>ReferenceEquals(primary,value)&&Volatile.Read(ref phase)==0;
    internal bool IsRunningFor(LegacyEnglishCasePrimaryCompletion value)=>ReferenceEquals(primary,value)&&Volatile.Read(ref phase)==1&&thread==Environment.CurrentManagedThreadId;
    // Scalar absence diagnostics only: no pending data/query reference escapes.
    // Dedicated probe reads these after a failed operation, never as authority.
    internal bool HasPendingCaptureForProbe=>completedBacking is not null;
    internal bool HasPendingReceiptForProbe=>pendingReceipt is not null;
    internal DataExport Run(Action<string>? observer=null,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();if(Interlocked.CompareExchange(ref phase,1,0)!=0)throw new InvalidOperationException("One whole-node draft entry required.");
        thread=Environment.CurrentManagedThreadId;
        try
        {
            ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Authentication,256); //DRAFT entry/publication/fault reservation
            var primaryDiagnostic=primary.Run(observer,token);
            // This checkpoint remains AFTER every OLD Run/publication/lease
            // close. A failure here must fault the closed exact original pass.
            observer?.Invoke("node0:after-oldRun");token.ThrowIfCancellationRequested();
            Volatile.Write(ref oldRunCompleted,1);
            if(!pending)throw new InvalidOperationException("No private pending node0 state.");
            ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Authentication,128); //DRAFT completed DATA validation
            if((uint)originalT>250||(uint)initializedN>124||initializedN>originalT||storedOrdinals is null||storedOrdinals.Length!=156||
                (auxiliaryState is null)!=(phase1Result is null)||completedBacking is null||!completedBacking.Matches(originalT,initializedN,phase1Result?.SourceEnd-125??0,phase1Result?.LogicalCapacity??124))throw new InvalidDataException("Pending node0 domains/state are incomplete.");
            pendingReceipt=PendingReceipt.Create(this,token);
            RequireWholeNodeReadiness();
            var result=pendingReceipt.CopyOnce(primaryDiagnostic,token);
            // No callback after this final cancellation check; publication
            // returns detached caller DATA, never pending owner authority.
            token.ThrowIfCancellationRequested();
            pending=false;pendingReceipt=null;completedBacking=null;
            Volatile.Write(ref phase,2);return result;
        }
        catch
        {
            pending=false;activeStage=null;completedBacking=null;pendingReceipt=null;
            // Authenticate while this full Run/thread is still active. Unlike
            // primary completionPass, the consumer's original pass exists even
            // when ring provisioning failed before primary completion began.
            primary.FaultNode0Completion(this);Volatile.Write(ref phase,3);throw;
        }
        finally{thread=0;}
    }
    private static void RequireWholeNodeReadiness()=>
        throw new NotSupportedException("DRAFT: receipt remains withheld pending complete budgets/owning oracle review.");
    internal void Provision(LegacyEnglishCasePrimaryCompletion actual,LegacyEnglishCaseInitialConsumer consumer,
        LegacyEnglishCaseInitialQueryPass pass,LegacyEnglishFreshCaseMainSource.InitializationLease lease,CancellationToken token)
    {
        RequireRunning(actual);token.ThrowIfCancellationRequested();
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Authentication,256);
        actual.RequireNode0Initialization(this,consumer,pass,lease);
        if(Interlocked.CompareExchange(ref initialized,1,0)!=0)throw new InvalidOperationException("One after-reset ring provisioning required.");
        var graph=pass.Initializer.PlanForAudit.BindingForAudit.Graph;
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Graph,4);
        if(graph.NodeCount is <1 or >4096||graph.MaximumAtoms!=8)throw new NotSupportedException("Exact bounded actual graph required.");
        fresh.BindInitialization(this,actual,consumer,pass,lease,token);
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,64);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,64);
        ring=new(graph.NodeCount,fresh.Retention,ledger,token);
    }
    internal void CompleteOwned(LegacyEnglishCasePrimaryCompletion actual,LegacyEnglishCaseInitialConsumer consumer,
        LegacyEnglishCaseInitialHeap heap,LegacyEnglishCaseInitialSourcePolicy policy,LegacyEnglishFreshCaseMainSource.InitialQueryLease lease,
        LegacyNativeWordHeapDescriptor descriptor,ReadOnlySpan<byte> rows,ReadOnlySpan<byte> alternatives,ReadOnlySpan<byte> rowMasks,
        ReadOnlySpan<byte> alternativeMasks,ReadOnlySpan<int> primaryOrdinals,int originalCount,CancellationToken token)
    {
        RequireRunning(actual);ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Authentication,256);actual.RequireNode0Live(this,consumer,heap,policy,lease);token.ThrowIfCancellationRequested();
        if(ring is null||Volatile.Read(ref initialized)!=1||Interlocked.CompareExchange(ref completed,1,0)!=0||
            rows.Length!=125*88||alternatives.Length!=125*80||rowMasks.Length!=rows.Length||alternativeMasks.Length!=alternatives.Length||
            primaryOrdinals.Length!=250||(uint)originalCount>250||descriptor.AllocatedCapacity!=250||descriptor.Count>125||descriptor.Count>originalCount)
            throw new InvalidOperationException("One genuine primary descriptor and prepared ring required.");
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,96);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,96);
        var stage=Stage.Issue(stageIssuer,this,actual,consumer,heap,policy,lease,descriptor,originalCount);activeStage=stage;
        try
        {
            stage.Require();
            ledger.CopyBytes(LegacyEnglishCaseNode0Ledger.Site.StoredBlocks,rows,ring.CurrentRows);
            ledger.CopyBytes(LegacyEnglishCaseNode0Ledger.Site.PrimaryBlocks,alternatives,ring.CurrentPrimary);
            ledger.CopyBytes(LegacyEnglishCaseNode0Ledger.Site.CurrentRowsMask,rowMasks,ring.CurrentRowMask);
            ledger.CopyBytes(LegacyEnglishCaseNode0Ledger.Site.CurrentPrimaryMask,alternativeMasks,ring.CurrentPrimaryMask);
            storedOrdinals=ledger.Ints(LegacyEnglishCaseNode0Ledger.Site.OrdinalVectors,156);
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OrdinalVectors,624);Array.Fill(storedOrdinals,-1);
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OrdinalVectors,500);primaryOrdinals[..125].CopyTo(storedOrdinals);
            ring.ClearCurrent3e(token);
            int primaryCount=descriptor.Count;ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Descriptor,4);
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Descriptor,12);
            descriptor.ClearActiveCount();
            if(!descriptor.Resize(124,0,null,cancellationToken:token)||descriptor.Count!=0||descriptor.AllocatedCapacity!=250)
                throw new InvalidDataException("Logical124 retained250 reset required without growth.");
            int sourceEnd=125,retainedEnd=125,n=0;
            if(NeedsLanguage(stage,rows,primaryCount,token))
            {
                // Precharge BEFORE RunAux enters: its compiler display frame
                // is allocated at method entry. Conditional64bit CoreCLR
                // instance-field envelopes, not CLR headers/padding/RSS.
                ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryMetadata,105541);
                // Constructor fields plus Case.with ROM setters16000,
                // helper query4750, List firstAdd extra1000 and captured
                // activeOrdinal1000. Higher delimiter/state copy closure
                // remains explicitly unproved; this is not whole sufficiency.
                ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.AuxiliaryMetadata,128291);
                auxiliaryState=RunAuxiliaryPrefixDraft(stage,primaryCount,token);
                n=stage.InitializedCountN;
                phase1Result=descriptor.CompleteNode0Phase1(stage,ring,ledger,storedOrdinals,token);
                sourceEnd=phase1Result.Value.SourceEnd;retainedEnd=phase1Result.Value.RetainedEnd;
            }
            // Both gate branches execute the literal common tail and actual
            // sourceV80(0). No zero delegate substitutes for an auxiliary read.
            _=ring.Cleanup(token);ring.ClearNode0Counters(sourceEnd,retainedEnd);stage.Require();
            lease.Owner.Initializer.PlanForAudit.ResetNode0Completion(stage,token);
            // Capture genuine full250 backing AFTER the actual endreset while
            // this exact query lease remains live; retain data privately only.
            completedBacking=CompletedBacking.Capture(stage,ledger,n,token);
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Descriptor,8);originalT=originalCount;initializedN=n;
            pending=true;
        }
        finally{activeStage=null;}
    }
    private bool NeedsLanguage(Stage stage,ReadOnlySpan<byte> rows,int primaryCount,CancellationToken token)
    {
        stage.Require();var graph=stage.Lease.Owner.Initializer.PlanForAudit.BindingForAudit.Graph;
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Graph,1);if(graph.NodeCount==1)return true;
        // Native wrapper cap8, current0 -> lookahead2. Full graph stays owned;
        // future probe must bind its exact ordered contents to immutable234.
        for(int n=1;n<Math.Min(graph.NodeCount,3);n++)
        {
            ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Graph,1);
            var arcs=graph.GetNode(n);if(arcs.Count>4096)throw new NotSupportedException("Bounded same-source lookahead row required.");
            foreach(var arc in arcs)
            {token.ThrowIfCancellationRequested();ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Graph,8);if(n-arc.Span-1==0&&(arc.Flags&4)!=0)return true;}
        }
        for(int i=0;i<primaryCount;i++)
        {token.ThrowIfCancellationRequested();ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Graph,1);if(BinaryPrimitives.ReadUInt16LittleEndian(rows[(i*88+36)..])==0xfffd)return true;}
        return false;
    }
    private void RequireRunning(LegacyEnglishCasePrimaryCompletion actual)
    {if(!IsRunningFor(actual))throw new InvalidOperationException("Exact running full owner/thread required.");}
    internal void RequireNativeFreshPolicy(LegacyEnglishCaseNode0NativeFreshPolicy actual)
    {RequireRunning(primary);if(!ReferenceEquals(fresh,actual))throw new InvalidOperationException("Exact privately prepared fresh policy required.");}
    internal sealed class Stage
    {
        private readonly LegacyEnglishCaseNode0Completion owner;
        private readonly LegacyEnglishCasePrimaryCompletion primary;
        private readonly LegacyEnglishCaseInitialConsumer consumer;
        private readonly LegacyEnglishCaseInitialHeap heap;
        private readonly LegacyEnglishCaseInitialSourcePolicy policy;
        private int acceptedClosed,initializedCountN=-1,phase1Entered;
        internal LegacyEnglishFreshCaseMainSource.InitialQueryLease Lease{get;}
        internal LegacyNativeWordHeapDescriptor Descriptor{get;}
        internal int OriginalCountT{get;}
        private Stage(LegacyEnglishCaseNode0Completion owner,LegacyEnglishCasePrimaryCompletion primary,LegacyEnglishCaseInitialConsumer consumer,
            LegacyEnglishCaseInitialHeap heap,LegacyEnglishCaseInitialSourcePolicy policy,LegacyEnglishFreshCaseMainSource.InitialQueryLease lease,
            LegacyNativeWordHeapDescriptor descriptor,int originalCount)
        {this.owner=owner;this.primary=primary;this.consumer=consumer;this.heap=heap;this.policy=policy;Lease=lease;Descriptor=descriptor;OriginalCountT=originalCount;}
        internal static Stage Issue(object issuer,LegacyEnglishCaseNode0Completion owner,LegacyEnglishCasePrimaryCompletion primary,
            LegacyEnglishCaseInitialConsumer consumer,LegacyEnglishCaseInitialHeap heap,LegacyEnglishCaseInitialSourcePolicy policy,
            LegacyEnglishFreshCaseMainSource.InitialQueryLease lease,LegacyNativeWordHeapDescriptor descriptor,int originalCount)
        {
            if(!ReferenceEquals(owner.stageIssuer,issuer)||owner.activeStage is not null||!owner.IsRunningFor(primary))
                throw new InvalidOperationException("Private full-owner stage issuer required.");
            return new(owner,primary,consumer,heap,policy,lease,descriptor,originalCount);
        }
        internal void Require()
        {
            owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Authentication,256);
            owner.RequireRunning(primary);if(!ReferenceEquals(owner.activeStage,this))throw new InvalidOperationException("Exact active-reference node0 stage required.");
            primary.RequireNode0Live(owner,consumer,heap,policy,Lease);
        }
        internal void ClaimEndReset(LegacyEnglishCaseInitializationPlan plan)
        {
            Require();if(!ReferenceEquals(Lease.Owner.Initializer.PlanForAudit,plan)||Interlocked.CompareExchange(ref owner.endReset,1,0)!=0)
                throw new InvalidOperationException("One same-live end-node reset required.");
        }
        internal void RequireCompletedCapture(LegacyEnglishCaseNode0Ledger ledger,int n)
        {
            Require();owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,32);
            if(!ReferenceEquals(owner.ledger,ledger)||Volatile.Read(ref owner.endReset)!=1||n<0||n>OriginalCountT||
                (Volatile.Read(ref acceptedClosed)==0?(n!=0||owner.auxiliaryState is not null||owner.phase1Result is not null):
                 (initializedCountN!=n||Volatile.Read(ref phase1Entered)!=1||owner.auxiliaryState is null||owner.phase1Result is null)))
                throw new InvalidOperationException("Same-owner ledger and completed actual endreset/prefix required for capture.");
        }
        internal void ChargeReset()
        {
            owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Reset,5632);
            // Additional ENDreset only, not OLD initialV80(-1): normal64 +
            // auxiliary8 new query objects,54 semantic bytes each. Match new
            // array policy: allocation initialization is not an extra Copy;
            // charge all3076 explicit reset field writes separately first.
            owner.ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.Reset,3888);
            owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Reset,3076);
        }
        internal void ChargeHeapVisit(int units)=>owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Descriptor,units);
        internal void ChargeHeapCopy(int bytes)=>owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Descriptor,bytes);
        internal void RequireNumericHandles(LegacyEnglishCaseInitializationNumericTables.Node0Handles actual)
        {
            if(!ReferenceEquals(owner.numericHandles,actual))
                throw new InvalidOperationException("Exact owner-private numeric handles required.");
        }
        internal void ChargeFirstKernel(int units)=>owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FirstKernel,units);
        internal int ReadBeginningPenalty(byte storedCategory,CancellationToken token)
        {
            Require();owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FirstKernel,512);
            return consumer.ReadNode0BeginningPenalty(primary,heap,policy,Lease,storedCategory,token);
        }
        internal void RequireAccepting()
        {Require();if(Volatile.Read(ref acceptedClosed)!=0)throw new InvalidOperationException("Accepted prefix already closed before phase1.");}
        internal void RequireOwnedFirstWriter(LegacyNativeWordSlotInitializer initializer)
        {
            RequireAccepting();owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FirstAcceptedWriter,8);
            if(!ReferenceEquals(initializer,owner.firstWriter)||owner.firstWriterBusy!=1||owner.firstWriterMaskValidated!=1||
                !ReferenceEquals(owner.firstWriterStage,this))
                throw new InvalidOperationException("Only exact private FIRST writer with validated actual mask sources may allocate.");
        }
        internal void CloseAcceptedPrefix(int count)
        {
            Require();if((uint)count>124||count>OriginalCountT)throw new InvalidDataException("Initialized phase1 prefix N must be bounded by original T and124.");
            if(Interlocked.CompareExchange(ref acceptedClosed,1,0)!=0)throw new InvalidOperationException("One post204290 physical ordinal stamp required.");
            initializedCountN=count;
        }
        internal int InitializedCountN{get{Require();if(Volatile.Read(ref acceptedClosed)!=1||initializedCountN<0)throw new InvalidOperationException("Post204290 prefix not closed.");return initializedCountN;}}
        internal void BeginPhase1(LegacyEnglishCaseNode0Ring ring,LegacyEnglishCaseNode0Ledger ledger,int[] ordinals)
        {
            Require();owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,32);
            if(Volatile.Read(ref acceptedClosed)!=1||initializedCountN<0||!ReferenceEquals(owner.ring,ring)||
                !ReferenceEquals(owner.ledger,ledger)||!ReferenceEquals(owner.storedOrdinals,ordinals)||ordinals.Length!=156||
                Interlocked.CompareExchange(ref phase1Entered,1,0)!=0)
                throw new InvalidOperationException("One same-owner closed-prefix phase1 and actual ordinal vector required.");
        }
        internal void RequireNativeFreshPolicy(LegacyEnglishCaseNode0NativeFreshPolicy actual)=>owner.RequireNativeFreshPolicy(actual);
    }
}
