namespace InkRecognition;
internal sealed partial class LegacyEnglishCasePrimaryCompletion
{
    internal void RequireGraphLoop(LegacyEnglishCaseNode0Completion owner,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(!ReferenceEquals(node0,owner)||!owner.IsPreparedFor(this)||Volatile.Read(ref phase)!=0)
            throw new InvalidOperationException("Exact unused graph owner required before entry.");
        if(!knowledge.Owner.IsComplete||knowledge.Owner.IsFaulted||knowledge.Owner.NodeCount is <2 or >49)
            throw new NotSupportedException("Current bounded graph loop requires2..49 completed nodes.");
        LegacyEnglishCaseNode0Completion.PreflightGraphLoopPayload(knowledge,token);
        var graph=knowledge.Owner.RetainCompletedD0Graph(token);
        for(int node=1;node<graph.NodeCount;node++){token.ThrowIfCancellationRequested();if(graph.GetNode(node).Count is <1 or >1024)throw new NotSupportedException("Empty or overlarge node requires its exact no-span lifecycle; no artificial D0 is substituted.");}
    }
    internal void RequireTwoNodeGraphTail(LegacyEnglishCaseNode0Completion owner,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(!ReferenceEquals(node0,owner)||!owner.IsPreparedFor(this)||Volatile.Read(ref phase)!=0)
            throw new InvalidOperationException("Exact unused owning graph tail required before entry.");
        if(!knowledge.Owner.IsComplete||knowledge.Owner.IsFaulted||knowledge.Owner.NodeCount!=2)
            throw new NotSupportedException("Connected last-node204290 currently admits the real two-node graph; larger wholegraph scheduling remains unimplemented.");
    }
}
