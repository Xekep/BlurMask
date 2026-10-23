namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    // SOURCE ONLY private pending immutable DATA receipt. Run never returns it.
    // It seals existing privately owned data refs; no122k intermediate clone.
    private sealed class PendingReceipt
    {
        private readonly LegacyEnglishCaseNode0Completion owner;
        private readonly CompletedBacking backing;
        private readonly LegacyEnglishCaseNode0Ring ring;
        private readonly AuxiliaryState? auxiliary;
        private readonly int[] ordinals;
        private readonly int t,n,count,capacity,sourceEnd,retainedEnd;
        private PendingReceipt(LegacyEnglishCaseNode0Completion owner,CompletedBacking backing,LegacyEnglishCaseNode0Ring ring,
            AuxiliaryState? auxiliary,int[] ordinals,int t,int n,int count,int capacity,int sourceEnd,int retainedEnd)
        {this.owner=owner;this.backing=backing;this.ring=ring;this.auxiliary=auxiliary;this.ordinals=ordinals;
         this.t=t;this.n=n;this.count=count;this.capacity=capacity;this.sourceEnd=sourceEnd;this.retainedEnd=retainedEnd;}
        internal static PendingReceipt Create(LegacyEnglishCaseNode0Completion owner,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();owner.RequirePendingCapture(owner.completedBacking);
            owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Export,512); //DRAFT data-only guard/helper bound
            if(owner.ring is null||owner.completedBacking is null||owner.storedOrdinals is null||owner.storedOrdinals.Length!=156)
                throw new InvalidOperationException("Actual private completed data owners required.");
            int source=owner.phase1Result?.SourceEnd??125,retained=owner.phase1Result?.RetainedEnd??125;
            int count=source-125,capacity=owner.phase1Result?.LogicalCapacity??124;
            if(source is <125 or >156||retained<125||retained>source||
                !owner.completedBacking.Matches(owner.originalT,owner.initializedN,count,capacity)||
                (owner.phase1Result is null&&(count!=0||capacity!=124||owner.auxiliaryState is not null))||
                (owner.phase1Result is not null&&owner.auxiliaryState is null))
                throw new InvalidDataException("Exact falsegate0/124 or actual phase1 endpoints/capacity required.");
            // Fixed semantic owner/ref/scalar envelope, not a CLR/RSS claim.
            owner.ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.Export,256);
            owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Export,256);
            return new(owner,owner.completedBacking,owner.ring,owner.auxiliaryState,owner.storedOrdinals,
                owner.originalT,owner.initializedN,count,capacity,source,retained);
        }
        // Future output API may expose this one defensive caller-DATA export;
        // neither this receipt nor any query/Stage/descriptor can be recreated
        // from it. No caller provides arrays or historical data to this method.
        internal DataExport CopyOnce(LegacyEnglishCasePrimaryCompletionReceipt primaryDiagnostic,CancellationToken token)
        {
            owner.RequirePendingReceipt(this,backing);token.ThrowIfCancellationRequested();
            if(Interlocked.CompareExchange(ref owner.exportClaimed,1,0)!=0)
                throw new InvalidOperationException("One completed node0 data export required.");
            owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Export,512); //DRAFT bounded export source checks
            if(!backing.Matches(t,n,count,capacity)||sourceEnd!=125+count||retainedEnd<125||retainedEnd>sourceEnd||ordinals.Length!=156)
                throw new InvalidDataException("Sealed actual completed data domains changed.");
            // Full maximum one-export payload: count both source-byte reads
            // and destination stores BEFORE any clone, separate from helpers.
            owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Export,2L*122032);
            var storedRows=ring.CurrentRows;var storedMasks=ring.CurrentRowMask;
            var primaryRows=ring.CurrentPrimary;var primaryMasks=ring.CurrentPrimaryMask;
            if(storedRows.Length!=13728||storedMasks.Length!=13728||primaryRows.Length!=10000||primaryMasks.Length!=10000)
                throw new InvalidDataException("Exact privately sealed row/primary image sizes required.");
            var images=backing.CopyDataImages(owner,token);
            var rows=owner.ledger.Snapshot(LegacyEnglishCaseNode0Ledger.Site.Export,storedRows);
            var rowMask=owner.ledger.Snapshot(LegacyEnglishCaseNode0Ledger.Site.Export,storedMasks);
            var primary=owner.ledger.Snapshot(LegacyEnglishCaseNode0Ledger.Site.Export,primaryRows);
            var primaryMask=owner.ledger.Snapshot(LegacyEnglishCaseNode0Ledger.Site.Export,primaryMasks);
            var ord=owner.ledger.Ints(LegacyEnglishCaseNode0Ledger.Site.Export,156);
            owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Export,624);ordinals.CopyTo(ord,0);
            byte[]? first=null,second=null,firstMask=null,secondMask=null,controller=null,controllerMask=null;
            if(auxiliary is not null)
            {
                if(auxiliary.First.Length!=440||auxiliary.Second.Length!=440||auxiliary.FirstMask.Length!=440||auxiliary.SecondMask.Length!=440||
                   auxiliary.Controller.Length!=4096||auxiliary.ControllerMask.Length!=4096)
                    throw new InvalidDataException("Exact sealed private auxiliary image sizes required.");
                first=owner.ledger.Snapshot(LegacyEnglishCaseNode0Ledger.Site.Export,auxiliary.First);
                second=owner.ledger.Snapshot(LegacyEnglishCaseNode0Ledger.Site.Export,auxiliary.Second);
                firstMask=owner.ledger.Snapshot(LegacyEnglishCaseNode0Ledger.Site.Export,auxiliary.FirstMask);
                secondMask=owner.ledger.Snapshot(LegacyEnglishCaseNode0Ledger.Site.Export,auxiliary.SecondMask);
                controller=owner.ledger.Snapshot(LegacyEnglishCaseNode0Ledger.Site.Export,auxiliary.Controller);
                controllerMask=owner.ledger.Snapshot(LegacyEnglishCaseNode0Ledger.Site.Export,auxiliary.ControllerMask);
            }
            token.ThrowIfCancellationRequested();owner.RequirePendingReceipt(this,backing);
            owner.ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.Export,256);
            owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Export,256);
            return new(images.Values,images.Masks,rows,rowMask,primary,primaryMask,ord,
                first,second,firstMask,secondMask,controller,controllerMask,t,n,count,capacity,sourceEnd,retainedEnd,
                backing.ReadDetachedGraph(),primaryDiagnostic);
        }
    }
    // Detached caller-owned defensive copies only. No completion owner, ring,
    // auxiliary helper, ledger, root, lease or original graph is retained.
    // Readiness still refuses every Run before this result can be published.
    internal sealed class DataExport(byte[] values,byte[] masks,byte[] rows,byte[] rowMask,byte[] primary,byte[] primaryMask,int[] ordinals,
        byte[]? first,byte[]? second,byte[]? firstMask,byte[]? secondMask,byte[]? controller,byte[]? controllerMask,
        int t,int n,int count,int capacity,int sourceEnd,int retainedEnd,GraphProjection graph,
        LegacyEnglishCasePrimaryCompletionReceipt primaryDiagnostic)
    {
        internal readonly byte[] Values=values,Masks=masks,Rows=rows,RowMask=rowMask,Primary=primary,PrimaryMask=primaryMask;
        internal readonly int[] Ordinals=ordinals;
        internal readonly byte[]? First=first,Second=second,FirstMask=firstMask,SecondMask=secondMask,Controller=controller,ControllerMask=controllerMask;
        internal readonly int T=t,N=n,Count=count,Capacity=capacity,SourceEnd=sourceEnd,RetainedEnd=retainedEnd;
        internal readonly GraphProjection Graph=graph;
        // Existing OLD receipt contains only its private data arrays/scalars.
        // Its own once-export policy and already reserved OLD payload apply.
        private readonly LegacyEnglishCasePrimaryCompletionReceipt primaryDiagnostic=primaryDiagnostic;
        internal LegacyEnglishCasePrimaryCompletionSnapshot ExportPrimaryDiagnostic(CancellationToken token=default)=>primaryDiagnostic.Export(token);
    }
    private void RequirePendingCapture(CompletedBacking? capture)
    {
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Export,128); //DRAFT data-only auth, no closed lease use
        if(Volatile.Read(ref phase)!=1||thread!=Environment.CurrentManagedThreadId||Volatile.Read(ref oldRunCompleted)!=1||
            !pending||capture is null||!ReferenceEquals(completedBacking,capture)||activeStage is not null)
            throw new InvalidOperationException("Exact withheld post-oldRun completed data owner/thread required.");
    }
    private void RequirePendingReceipt(PendingReceipt receipt,CompletedBacking backing)
    {
        RequirePendingCapture(backing);ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Export,16);
        if(!ReferenceEquals(pendingReceipt,receipt))throw new InvalidOperationException("Exact private pending data receipt required.");
    }
}
