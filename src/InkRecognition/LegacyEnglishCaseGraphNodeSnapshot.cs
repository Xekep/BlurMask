namespace InkRecognition;
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    private LegacyEnglishCaseGraphNodeSnapshot? graphCompletedNode;
}
// Detached DATA ONLY. No root/session/stage/source/callback/context-owner refs.
// It cannot be reused as a query, final-result or terminal-publication issuer.
internal sealed class LegacyEnglishCaseGraphNodeSnapshot
{
    private readonly byte[] rows,rowMask,alternatives,alternativeMask,heap,heapMask;
    private readonly int[] ordinals;
    internal int Node{get;}internal int SourceEnd{get;}internal int RetainedEnd{get;}internal int OriginalT{get;}internal int InitializedN{get;}
    private LegacyEnglishCaseGraphNodeSnapshot(int node,int sourceEnd,int retainedEnd,int t,int n,byte[] rows,byte[] rowMask,byte[] alternatives,byte[] alternativeMask,byte[] heap,byte[] heapMask,int[] ordinals)
    {Node=node;SourceEnd=sourceEnd;RetainedEnd=retainedEnd;OriginalT=t;InitializedN=n;this.rows=rows;this.rowMask=rowMask;this.alternatives=alternatives;this.alternativeMask=alternativeMask;this.heap=heap;this.heapMask=heapMask;this.ordinals=ordinals;}
    internal static LegacyEnglishCaseGraphNodeSnapshot Capture(LegacyEnglishCaseNode0Completion.GraphLanguageScope scope,CancellationToken token)
    {
        scope.RequireCompletedNode();scope.RequireFirstOnlyPattern();token.ThrowIfCancellationRequested();var ledger=scope.Ledger;
        var rows=ledger.Snapshot(LegacyEnglishCaseNode0Ledger.Site.FinalImages,scope.Ring.CurrentRows);
        var rowMask=ledger.Snapshot(LegacyEnglishCaseNode0Ledger.Site.FinalImages,scope.Ring.CurrentRowMask);
        var alternatives=ledger.Snapshot(LegacyEnglishCaseNode0Ledger.Site.FinalImages,scope.Ring.CurrentPrimary);
        var alternativeMask=ledger.Snapshot(LegacyEnglishCaseNode0Ledger.Site.FinalImages,scope.Ring.CurrentPrimaryMask);
        // Existing getter is defensive and ToArray makes one further clone;
        // reserve BOTH complete allocations before either copy executes.
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,64000);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,64000);
        var heap=scope.Descriptor.GetBackingStorageSnapshot().ToArray();
        var heapMask=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.FinalImages,32000);
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,32000);ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,32000);
        int t=scope.OriginalCountT,n=scope.InitializedCountN;
        for(int slot=0;slot<250;slot++)for(int at=0;at<128;at++)heapMask[slot*128+at]=slot>=t||at>=120||slot<n&&(at is >=65 and <=67 or >=76 and <=79)?(byte)0:(byte)255;
        var ordinals=ledger.Ints(LegacyEnglishCaseNode0Ledger.Site.FinalImages,156);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,624);scope.Ordinals.CopyTo(ordinals,0);
        ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,84);ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,84);
        var result=new LegacyEnglishCaseGraphNodeSnapshot(scope.CurrentNode,scope.Ring.SourceEndAt(scope.CurrentNode),scope.Ring.RetainedEndAt(scope.CurrentNode),t,n,rows,rowMask,alternatives,alternativeMask,heap,heapMask,ordinals);
        scope.RequireCompletedNode();token.ThrowIfCancellationRequested();return result;
    }
    internal byte[] CopyRows()=>rows.ToArray();internal byte[] CopyRowMasks()=>rowMask.ToArray();internal byte[] CopyAlternatives()=>alternatives.ToArray();internal byte[] CopyAlternativeMasks()=>alternativeMask.ToArray();internal byte[] CopyHeap()=>heap.ToArray();internal byte[] CopyHeapMasks()=>heapMask.ToArray();internal int[] CopyOrdinals()=>ordinals.ToArray();
}
