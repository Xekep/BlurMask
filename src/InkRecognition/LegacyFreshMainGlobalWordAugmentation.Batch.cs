namespace InkRecognition;

public sealed partial class LegacyFreshMainGlobalWordAugmentation
{
    private readonly LegacyFreshMainGlobalSourceBatchCompletion.SpanScope? batchScope;
    private SpanWork batchWork;
    internal static LegacyFreshMainGlobalWordAugmentation CreateBatchWorker(
        LegacyFreshMainGlobalSourceBatchCompletion.SpanScope scope,ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalPlainRecognitionResult source,LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot snapshot,
        byte[] compact,int[] starts,LegacyNativePublishedWordNode span)
        =>new(scope,resource,source,snapshot,compact,starts,span);
    private LegacyFreshMainGlobalWordAugmentation(LegacyFreshMainGlobalSourceBatchCompletion.SpanScope scope,
        ReadOnlySpan<byte> resource,LegacyFreshMainGlobalPlainRecognitionResult source,
        LegacyFreshMainGlobalPublishedFirstAlternativeSnapshot snapshot,byte[] compact,int[] starts,LegacyNativePublishedWordNode span)
        :this(resource,source,snapshot,compact,starts,span){batchScope=scope;}
    internal void RunBatchSource(LegacyNativeWordBuilder workingWord,byte[] recordStorage,ushort[] labelStorage,
        byte[] requestStorage,byte[] walkStorage,Span<byte> left,Span<byte> right,
        LegacyGlobalWordAugmentationEnvelope envelope,CancellationToken token)
    {
        if(batchScope is null)throw new InvalidOperationException("Private batch worker required.");
        batchScope.RequireSource(this,token);
        batchWork=AugmentCurrentSpan(workingWord,recordStorage,labelStorage,requestStorage,walkStorage,left,right,envelope,token);
        if(controller24a0!=0)throw new InvalidOperationException("Actual batch span scratch remains pending.");
    }
    internal Result RankBatchSource(int quota,Span<byte> left,Span<byte> right,
        LegacyGlobalWordAugmentationEnvelope envelope,CancellationToken token)
    {
        if(batchScope is null)throw new InvalidOperationException("Private batch worker required.");
        batchScope.RequireRanking(this,token);return RankCurrentSpan(batchWork,quota,left,right,envelope,token);
    }
    internal int BatchTextReferenceCount(LegacyFreshMainGlobalSourceBatchCompletion.SpanScope actual,CancellationToken token)
    {if(!ReferenceEquals(batchScope,actual))throw new ArgumentException("Exact completed batch worker required.");actual.RequireText(this,token);return sourceReferences.Length;}
    internal LegacyNativeInkPositionRange[] ProjectBatchTextRanges(LegacyFreshMainGlobalSourceBatchCompletion.SpanScope actual,
        LegacyNativeTextSourceInputMap map,LegacyNativeInkPositionRange[] original,CancellationToken token)
    {
        _=BatchTextReferenceCount(actual,token);
        var projection=new LegacyNativeTextSourceReference[sourceReferences.Length];
        for(int i=0;i<projection.Length;i++)
        {
            token.ThrowIfCancellationRequested();var r=sourceReferences[i];
            if(r.First.First!=0||r.Last.First!=0)throw new NotSupportedException("Fresh original group0 batch source only.");
            projection[i]=new(new(r.First.First,r.First.Second,r.First.Third),new(r.Last.First,r.Last.Second,r.Last.Third),r.FirstResidual,r.LastResidual);
        }
        var ranges=LegacyNativeTextSourcePositionPublication.Project(map,projection,true,original,token);
        actual.RequireText(this,token);return ranges;
    }
}
