using System.Buffers.Binary;
namespace InkRecognition;

// Explicit private same-owner opt-in; no prepared row/minimum/cumulative ingress.
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    private bool graphPeriodicActive;
    private int graphPeriodicInvalidationFirst=-1;
    private int graphPeriodicCompletedNode=-1;
    private void CompleteGraphPeriodicCost(GraphLanguageScope scope,int materializedMinimum,CancellationToken token)
    {
        scope.RequirePhase1();token.ThrowIfCancellationRequested();
        var graph=scope.Lease.Pass.Initializer.PlanForAudit.BindingForAudit.Graph;
        if(!graphPeriodicActive||!graphLoopActive||scope.CurrentNode!=graphLoopNode||graph.NodeCount is <2 or >120||
            graph.MaximumAtoms!=8||scope.Ring.ActualNodeCount!=graph.NodeCount||scope.Ring.DistinctRows!=scope.Ring.Length)
            throw new InvalidOperationException("Same fresh distinct-slot periodic graph owner required.");
        fresh.RequireGraphPeriodicSource(scope,token);
        // Literal common tail: current2520 copies previous regardless of minimum/event.
        scope.Ring.PropagatePeriodicCumulative(scope,token);
        graphPeriodicInvalidationFirst=-1;
        int node=scope.CurrentNode;
        if((node+1)%50!=0||unchecked((uint)(materializedMinimum-1))>=0x3ffffff)return;
        // Actual frontend151FF0 -> ordinary graph1A75D0; inclusive k<=A0.
        int backward=Math.Min(node+1,graph.MaximumAtoms);
        int first=Math.Max(0,node-backward);
        for(int prior=node;prior>=first;prior--)
        {
            token.ThrowIfCancellationRequested();scope.RequirePhase1();
            scope.Ring.NormalizePeriodicSource(scope,prior,materializedMinimum,token);
        }
        graphPeriodicInvalidationFirst=first;
        scope.RequirePhase1();token.ThrowIfCancellationRequested();
    }
}

internal sealed partial class LegacyEnglishCaseNode0Ring
{
    internal void PropagatePeriodicCumulative(LegacyEnglishCaseNode0Completion.GraphLanguageScope scope,CancellationToken token)
    {
        scope.RequirePhase1();token.ThrowIfCancellationRequested();
        if(!scope.PeriodicEnabled||!ReferenceEquals(scope.Ring,this)||currentNode!=scope.CurrentNode||ActualNodeCount>120||DistinctRows!=Length)
            throw new InvalidOperationException("Exact fresh periodic ring required.");
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Vector2520,8);
        int previous=currentNode==0?0:BinaryPrimitives.ReadInt32LittleEndian(vectors[8].AsSpan((currentNode-1)*4,4));
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Vector2520,4);
        BinaryPrimitives.WriteInt32LittleEndian(vectors[8].AsSpan(currentNode*4,4),previous);
    }
    internal void NormalizePeriodicSource(LegacyEnglishCaseNode0Completion.GraphLanguageScope scope,int node,int minimum,CancellationToken token)
    {
        scope.RequirePhase1();token.ThrowIfCancellationRequested();
        if(!scope.PeriodicEnabled||!ReferenceEquals(scope.Ring,this)||scope.CurrentNode!=currentNode||
            node>currentNode||node<Math.Max(0,currentNode-8)||unchecked((uint)(minimum-1))>=0x3ffffff)
            throw new InvalidOperationException("Exact positive finite current backward periodic extent required.");
        int end=SourceEndAt(node);if(end is <125 or >156)throw new InvalidDataException("Complete actual SourceEnd required.");
        var values=OwnedRowsAt(node);var mask=OwnedRowMaskAt(node);
        for(int ordinal=0;ordinal<end;ordinal++)
        {
            token.ThrowIfCancellationRequested();ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.StoredBlocks,8);
            int at=ordinal*88+16;
            for(int j=0;j<4;j++)if(mask[at+j]!=255)throw new NotSupportedException("Actual materialized cost10 must be defined.");
            int cost=BinaryPrimitives.ReadInt32LittleEndian(values.Slice(at,4));
            if(cost<0x4000000)
            {
                ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.StoredBlocks,4);
                BinaryPrimitives.WriteInt32LittleEndian(values.Slice(at,4),unchecked(cost-minimum));
            }
        }
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Vector2520,4);
        int cumulative=BinaryPrimitives.ReadInt32LittleEndian(vectors[8].AsSpan(node*4,4));
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Vector2520,4);
        BinaryPrimitives.WriteInt32LittleEndian(vectors[8].AsSpan(node*4,4),unchecked(cumulative+minimum));
        // Cost14, primary80, masks and allocation rows beyondSourceEnd are unchanged.
    }
    internal int ReadPeriodicCumulative(LegacyEnglishCaseNode0Completion.GraphLanguageScope scope,int node,CancellationToken token)
    {
        scope.RequireCompactPhase();token.ThrowIfCancellationRequested();if(!scope.PeriodicEnabled||!ReferenceEquals(scope.Ring,this)||(uint)node>=(uint)ActualNodeCount)
            throw new InvalidOperationException("Same LAST periodic cumulative vector required.");
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Vector2520,4);return BinaryPrimitives.ReadInt32LittleEndian(vectors[8].AsSpan(node*4,4));
    }
    internal int[] CopyPeriodicCumulative(LegacyEnglishCaseNode0Completion.GraphLanguageScope scope,CancellationToken token)
    {
        scope.RequireCompactPhase();token.ThrowIfCancellationRequested();
        if(!scope.PeriodicEnabled||!ReferenceEquals(scope.Ring,this)||scope.CurrentNode!=ActualNodeCount-1||ActualNodeCount>120)
            throw new InvalidOperationException("Same LAST periodic cumulative owner required.");
        var result=ledger.Ints(LegacyEnglishCaseNode0Ledger.Site.Vector2520,ActualNodeCount);
        ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Vector2520,checked(4L*ActualNodeCount));
        ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Vector2520,checked(4L*ActualNodeCount));
        for(int node=0;node<result.Length;node++)
        {token.ThrowIfCancellationRequested();result[node]=BinaryPrimitives.ReadInt32LittleEndian(vectors[8].AsSpan(node*4,4));}
        return result;
    }
}
