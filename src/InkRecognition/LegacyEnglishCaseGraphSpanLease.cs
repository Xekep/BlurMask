namespace InkRecognition;
public sealed partial class LegacyEnglishFreshCaseMainSource
{
    private int graphSpanNode=-1,graphSpan=-1,graphExpectedNode=1,graphExpectedSpan;
    internal Node1QueryLease BeginGraphSpan(LegacyEnglishCaseNode0Completion full,LegacyEnglishCaseInitialQueryPass pass,
        LegacyEnglishFreshCaseMainSpanSession.Receipt receipt,CancellationToken token)
    {
        full.RequireNode1(this,pass);full.RequireGraphLoopSpan(receipt.Anchor,receipt.Span);RequireCurrent();token.ThrowIfCancellationRequested();
        lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(faulted||busy||!ReferenceEquals(node1Owner,full)||!ReferenceEquals(queryOwner,pass)||
                !ReferenceEquals(spanOwner,pass.SpanSession)||!pass.SpanSession.IsCurrent(receipt)||receipt.Epoch!=version||
                receipt.Anchor!=graphExpectedNode||receipt.Span!=graphExpectedSpan||receipt.Anchor<1||receipt.Span>=8)
                throw new InvalidOperationException("Exact next owning graph span/current D0 required.");
            var next=new Node1QueryLease(this,full,pass,version,working,receipt.Span,receipt.Anchor);
            pass.Initializer.PlanForAudit.BindingForAudit.Dictionary.SetWorkingMap(working);
            node1QueryEntered=true;busy=true;node1Lease=next;graphSpanNode=receipt.Anchor;graphSpan=receipt.Span;return next;
        }
    }
    internal void EndGraphSpan(Node1QueryLease lease,bool lastSpan)
    {
        ValidateNode1(lease);lease.Full.RequireGraphLoopSpan(lease.Node,lease.Span);
        lock(gate)
        {
            if(graphSpanNode!=lease.Node||graphSpan!=lease.Span)throw new InvalidOperationException("Same graph span must close.");
            var next=lease.Full.NextActualGraphSpan(lease,lastSpan);graphExpectedNode=next.Node;graphExpectedSpan=next.Span;
            node1Lease=null;busy=false;
        }
    }
}
internal sealed partial class LegacyEnglishCaseInitializationPlan
{
    internal LegacyNativeDelimiterQueryPage ReadGraphSpan(LegacyEnglishFreshCaseMainSource.Node1QueryLease lease,LegacyNativeDelimiterQuery query,CancellationToken token)
    {
        if(executionPool is null||Volatile.Read(ref phase)!=5||Volatile.Read(ref executionReset)!=1||
            !ReferenceEquals(lease.Root,BoundRoot)||!ReferenceEquals(lease.Pass.Initializer,owner))throw new InvalidOperationException("Same initialized graph source required.");
        lease.Root.ValidateNode1(lease);lease.Full.RequireNode1Query(lease,query);ValidateParent(token);
        var result=normal!.Read(query,token);lease.Root.ValidateNode1(lease);token.ThrowIfCancellationRequested();return result;
    }
}
internal sealed partial class LegacyEnglishCaseNode1Geometry
{
    internal LegacyNativeWordAcousticGroupCosts CreateGraphGroupCosts(int node,ushort arc,ReadOnlySpan<int> costs,byte span,CancellationToken token)
    {
        source.Validate(token);
        if((uint)node>=(uint)source.Graph.NodeCount||(uint)arc>=(uint)source.Graph.GetNode(node).Count||source.Graph.GetNode(node)[arc].Span!=span||costs.Length!=220)
            throw new InvalidDataException("Same actual graph selected span/cache220 required.");
        var geometry=new int[24];Array.Fill(geometry,source.Separation[source.Graph.GetNode(node)[arc].GroupId]);return new(costs,geometry);
    }
}
