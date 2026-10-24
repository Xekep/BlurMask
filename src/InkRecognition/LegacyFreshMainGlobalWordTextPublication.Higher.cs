namespace InkRecognition;

public sealed partial class LegacyFreshMainGlobalWordTextPublication
{
    private readonly record struct HigherAdmission(ulong Target,ulong OriginalLast,int Ranges);
    private HigherAdmission ReserveHigherMetadata(LegacyGlobalCompletedWordSpan span,int word,int offset,ulong reference,CancellationToken token)
    {
        span.RequireOwnedReference(this,word,offset,reference,token);
        var completed=Wrapper.ActualCompletion;completed.Validate(token);
        var refresh=Wrapper.Choices.Refresh;var metadata=refresh.SourceMetadata;
        var session=completed.Source.Session!;var overlay=session.Associations.SourceOverlay;
        if(batchWords is null||!ReferenceEquals(completed.BatchWords,batchWords)||!ReferenceEquals(refresh.ActualCompletion,completed)||
            !ReferenceEquals(metadata,session.Associations.Metadata)||!ReferenceEquals(metadata.SourceOverlay,overlay)||
            !ReferenceEquals(overlay.Geometry,session.Ownership.Geometry))
            throw new ArgumentException("Exact current completed batch WORD/session metadata overlay required.");
        ulong target=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference);
        int count=overlay.ReadHigherRangeCount(target,token);
        if(count==0)return default;
        var bound=LegacyMainHigherSourceMetadataProjection.BoundThreeReads(count);
        batchWords.ReserveHigherMetadata(bound.Copy,bound.Loops,token);
        return new(target,overlay.ResolveBaseReference(target,token),count);
    }
    private void RequireHigherMetadata(HigherAdmission admitted,LegacyFreshMainGlobalGlyphSourceMetadataResult source,CancellationToken token)
    {
        Wrapper.ValidatePreclaim(token);
        if(admitted.Ranges==0||!source.IsHigherSource||!ReferenceEquals(source.Owner,Wrapper.Choices.Refresh.SourceMetadata)||
            source.TargetSourceKey!=admitted.Target||source.BaseSourceKey!=admitted.OriginalLast||source.ReferenceCount>admitted.Ranges)
            throw new InvalidDataException("Same actual higher target/originalLast/projected range allocation required.");
    }
}
