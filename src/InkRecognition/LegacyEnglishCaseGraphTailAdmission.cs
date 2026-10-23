namespace InkRecognition;
internal sealed partial class LegacyEnglishCasePrimaryCompletion
{
    internal void RequireTwoNodeGraphTail(LegacyEnglishCaseNode0Completion owner,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(!ReferenceEquals(node0,owner)||!owner.IsPreparedFor(this)||Volatile.Read(ref phase)!=0)
            throw new InvalidOperationException("Exact unused owning graph tail required before entry.");
        if(!knowledge.Owner.IsComplete||knowledge.Owner.IsFaulted||knowledge.Owner.NodeCount!=2)
            throw new NotSupportedException("Connected last-node204290 currently admits the real two-node graph; larger wholegraph scheduling remains unimplemented.");
    }
}
