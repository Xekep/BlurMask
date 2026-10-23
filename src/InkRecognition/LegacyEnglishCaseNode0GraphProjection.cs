namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    // Detached scalar DATA. No original graph, identity, state, connections,
    // query owner, array view or caller-supplied backing is retained.
    internal sealed class GraphProjection
    {
        private readonly int[] offsets,arcs;
        internal int NodeCount=>offsets.Length-1;
        internal int ArcCount=>arcs.Length/5;
        internal int MaximumAtoms=>8;
        private GraphProjection(int[] offsets,int[] arcs){this.offsets=offsets;this.arcs=arcs;}
        internal int CountAt(int node)
        {if((uint)node>=(uint)NodeCount)throw new ArgumentOutOfRangeException(nameof(node));return offsets[node+1]-offsets[node];}
        internal (int GroupId,byte Span,byte Flags,uint StartContextId,uint EndContextId) ReadArc(int node,int ordinal)
        {
            int count=CountAt(node);if((uint)ordinal>=(uint)count)throw new ArgumentOutOfRangeException(nameof(ordinal));
            int p=checked(5*(offsets[node]+ordinal));
            return(arcs[p],checked((byte)arcs[p+1]),checked((byte)arcs[p+2]),unchecked((uint)arcs[p+3]),unchecked((uint)arcs[p+4]));
        }
        internal static GraphProjection Capture(Stage stage,LegacyEnglishCaseNode0Ledger ledger,CancellationToken token)
        {
            stage.Require();token.ThrowIfCancellationRequested();
            // DRAFT source allowance, not a whole-owner sufficiency theorem.
            // Two bounded passes; charge before the first core graph access.
            ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Graph,128L*(4096+4096)+512);
            var graph=stage.Lease.Owner.Initializer.PlanForAudit.BindingForAudit.Graph;
            int nodes=graph.NodeCount,total=0;
            if(nodes is <1 or >4096||graph.MaximumAtoms!=8)throw new InvalidDataException("Bounded actual graph required.");
            for(int node=0;node<nodes;node++)
            {
                token.ThrowIfCancellationRequested();int count=graph.GetNode(node).Count;
                if(count<0||count>4096-total)throw new NotSupportedException("Detached graph total arc admission4096 required.");
                total+=count;
            }
            var offsets=ledger.Ints(LegacyEnglishCaseNode0Ledger.Site.Graph,nodes+1);
            var arcs=ledger.Ints(LegacyEnglishCaseNode0Ledger.Site.Graph,checked(5*total));
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Graph,4L*(nodes+1)+20L*total);
            int at=0;
            for(int node=0;node<nodes;node++)
            {
                token.ThrowIfCancellationRequested();offsets[node]=at;var row=graph.GetNode(node);
                for(int i=0;i<row.Count;i++)
                {
                    var arc=row[i];int p=checked(5*at++);
                    arcs[p]=arc.GroupId;arcs[p+1]=arc.Span;arcs[p+2]=arc.Flags;
                    arcs[p+3]=unchecked((int)arc.StartContextId);arcs[p+4]=unchecked((int)arc.EndContextId);
                }
            }
            offsets[nodes]=at;
            if(at!=total)throw new InvalidDataException("Immutable actual graph changed during projection.");
            stage.Require();token.ThrowIfCancellationRequested();
            ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.Graph,16);
            ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Graph,16);
            return new(offsets,arcs);
        }
    }
}
