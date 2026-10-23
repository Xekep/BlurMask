namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    // Private completed DATA only: no Stage/lease/descriptor/query authority and
    // no backing view/export API. Final receipt publication remains refused.
    private sealed class CompletedBacking
    {
        private readonly byte[] values,masks;
        private readonly int originalT,initializedN,count,capacity,graphNodeCount,maximumAtoms;
        private readonly GraphProjection graph;
        private CompletedBacking(byte[] values,byte[] masks,int t,int n,int count,int capacity,GraphProjection graph,int nodes,int atoms)
        {this.values=values;this.masks=masks;originalT=t;initializedN=n;this.count=count;this.capacity=capacity;this.graph=graph;graphNodeCount=nodes;maximumAtoms=atoms;}
        internal bool Matches(int t,int n,int expectedCount,int expectedCapacity)=>values.Length==32000&&masks.Length==32000&&originalT==t&&initializedN==n&&
            count==expectedCount&&capacity==expectedCapacity&&(uint)count<=31&&(capacity is 31 or 124)&&graph.NodeCount==graphNodeCount&&graph.MaximumAtoms==maximumAtoms&&
            graphNodeCount is >=1 and <=4096&&maximumAtoms==8;
        internal static CompletedBacking Capture(Stage stage,LegacyEnglishCaseNode0Ledger ledger,int n,CancellationToken token)
        {
            stage.RequireCompletedCapture(ledger,n);token.ThrowIfCancellationRequested();
            ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,512); //DRAFT exact graph-source/helper bound
            int t=stage.OriginalCountT;
            if((uint)t>250||(uint)n>124||n>t)throw new InvalidDataException("Actual originalT/sealedN mask domains required.");
            // Actual core graph scalar projection; never retain the original
            // graph's Identity/GlobalSource/State/history ancestry in DATA.
            var graph=GraphProjection.Capture(stage,ledger,token);
            int nodes=graph.NodeCount,atoms=graph.MaximumAtoms;
            if(nodes is <1 or >4096||atoms!=8)throw new InvalidDataException("Exact completed core graph domain required.");
            var values=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.FinalImages,32000);
            var masks=ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.FinalImages,32000);
            stage.Descriptor.CopyNode0CompletedBacking(stage,ledger,values,n,token);
            // Independent mask function: no row value or unknown field is read.
            // OLD genuine allocator/initial receiving writer gives patternA in
            // [0,T); canonical guarded FIRST gives patternB in[0,N). Phase1
            // moves stay within N and transport the same B; remaining T tail A
            // and outsideT wholly unknown are retained as separate domains.
            ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.FinalImages,8L*32000);
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,32000);
            for(int slot=0;slot<250;slot++)
            {
                token.ThrowIfCancellationRequested();
                for(int offset=0;offset<128;offset++)masks[slot*128+offset]=KnownMaskByte(slot,offset,t,n);
            }
            stage.RequireCompletedCapture(ledger,n);token.ThrowIfCancellationRequested();
            // Semantic three refs+six ints=48, not actual CLR object size/RSS.
            ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,48);
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,48);
            return new(values,masks,t,n,stage.Descriptor.Count,stage.Descriptor.Capacity,graph,nodes,atoms);
        }
        internal (byte[] Values,byte[] Masks) CopyDataImages(LegacyEnglishCaseNode0Completion owner,CancellationToken token)
        {
            owner.RequirePendingCapture(this);token.ThrowIfCancellationRequested();
            return(owner.ledger.Snapshot(LegacyEnglishCaseNode0Ledger.Site.Export,values),
                   owner.ledger.Snapshot(LegacyEnglishCaseNode0Ledger.Site.Export,masks));
        }
        internal GraphProjection ReadDetachedGraph()=>graph;
        private static byte KnownMaskByte(int slot,int offset,int t,int n)=>
            slot>=t||offset>=120||slot<n&&(offset is >=65 and <=67 or >=76 and <=79)?(byte)0:(byte)255;
    }
}
