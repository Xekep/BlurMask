namespace InkRecognition;
public sealed partial class LegacyEnglishFreshCaseMainSource
{
    private LegacyEnglishCaseNode0Completion? node1Owner;
    private Node1QueryLease? node1Lease;
    private bool node1QueryEntered;
    private int node1CompletedSpan=-1;
    internal void ClaimNode1(LegacyEnglishCaseNode0Completion full,LegacyEnglishCaseInitialQueryPass pass,CancellationToken token)
    {
        full.RequireNode1(this,pass);RequireCurrent();token.ThrowIfCancellationRequested();
        lock(gate)
        {
            if(faulted||busy||!firstQueryEntered||activeQuery is not null||!ReferenceEquals(queryOwner,pass)||node1Owner is not null||version!=1||entered!=1)
                throw new InvalidOperationException("One same-owner after-node0 continuation claim required.");
            node1Owner=full;
        }
    }
    internal Node1QueryLease BeginNode1(LegacyEnglishCaseNode0Completion full,LegacyEnglishCaseInitialQueryPass pass,
        LegacyEnglishFreshCaseMainSpanSession.Receipt receipt,CancellationToken token)
    {
        full.RequireNode1(this,pass);RequireCurrent();token.ThrowIfCancellationRequested();
        lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(faulted||busy||node1QueryEntered||!ReferenceEquals(node1Owner,full)||!ReferenceEquals(queryOwner,pass)||
                !ReferenceEquals(spanOwner,pass.SpanSession)||!pass.SpanSession.IsCurrent(receipt)||receipt.Anchor!=1||receipt.Span!=0||receipt.Epoch!=version||entered!=2)
                throw new InvalidOperationException("Same current node1 D0 owner/map required.");
            var next=new Node1QueryLease(this,full,pass,version,working);
            pass.Initializer.PlanForAudit.BindingForAudit.Dictionary.SetWorkingMap(working);
            node1QueryEntered=true;busy=true;node1Lease=next;return next;
        }
    }
    internal Node1QueryLease BeginNode1WholeStart(LegacyEnglishCaseNode0Completion full,LegacyEnglishCaseInitialQueryPass pass,
        LegacyEnglishFreshCaseMainSpanSession.Receipt receipt,CancellationToken token)
    {
        full.RequireNode1(this,pass);RequireCurrent();token.ThrowIfCancellationRequested();
        lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(faulted||busy||!node1QueryEntered||node1CompletedSpan!=0||!ReferenceEquals(node1Owner,full)||!ReferenceEquals(queryOwner,pass)||
                !ReferenceEquals(spanOwner,pass.SpanSession)||!pass.SpanSession.IsCurrent(receipt)||receipt.Anchor!=1||receipt.Span!=1||receipt.Epoch!=version||entered!=3)
                throw new InvalidOperationException("Same node1 postspan0/whole-start D0 epoch required.");
            var next=new Node1QueryLease(this,full,pass,version,working,1);
            pass.Initializer.PlanForAudit.BindingForAudit.Dictionary.SetWorkingMap(working);
            busy=true;node1Lease=next;return next;
        }
    }
    internal void ValidateNode1(Node1QueryLease lease)
    {
        lease.Full.RequireNode1(this,lease.Pass);
        lock(gate)if(faulted||!busy||!ReferenceEquals(node1Lease,lease)||!ReferenceEquals(node1Owner,lease.Full)||
            !ReferenceEquals(queryOwner,lease.Pass)||lease.Version!=version||!ReferenceEquals(lease.Map,working))
            throw new InvalidOperationException("Exact active node1 continuation lease required.");
    }
    internal void EndNode1(Node1QueryLease lease)
    {ValidateNode1(lease);lock(gate){node1CompletedSpan=lease.Span;node1Lease=null;busy=false;}}
    internal void FaultNode1(LegacyEnglishCaseNode0Completion full)
    {lock(gate)if(ReferenceEquals(node1Owner,full)){faulted=true;node1Lease=null;busy=false;}}
    internal sealed class Node1QueryLease
    {
        internal Node1QueryLease(LegacyEnglishFreshCaseMainSource root,LegacyEnglishCaseNode0Completion full,LegacyEnglishCaseInitialQueryPass pass,long version,LegacyPreparedDictionaryWorkingMap map,byte span=0)
        {Root=root;Full=full;Pass=pass;Version=version;Map=map;Span=span;}
        internal readonly LegacyEnglishFreshCaseMainSource Root;
        internal readonly LegacyEnglishCaseNode0Completion Full;
        internal readonly LegacyEnglishCaseInitialQueryPass Pass;
        internal readonly long Version;
        internal readonly byte Span;
        internal readonly LegacyPreparedDictionaryWorkingMap Map;
    }
}
internal sealed partial class LegacyEnglishCaseInitializationPlan
{
    internal LegacyNativeDelimiterQueryPage ReadNode1(LegacyEnglishFreshCaseMainSource.Node1QueryLease lease,LegacyNativeDelimiterQuery query,CancellationToken token)
    {
        if(executionPool is null||Volatile.Read(ref phase)!=5||Volatile.Read(ref executionReset)!=1||
            !ReferenceEquals(lease.Root,BoundRoot)||!ReferenceEquals(lease.Pass.Initializer,owner))
            throw new InvalidOperationException("Exact original initialized/reset node1 source required.");
        lease.Root.ValidateNode1(lease);lease.Full.RequireNode1Query(lease,query);ValidateParent(token);
        var result=normal!.Read(query,token);lease.Root.ValidateNode1(lease);token.ThrowIfCancellationRequested();return result;
    }
}
