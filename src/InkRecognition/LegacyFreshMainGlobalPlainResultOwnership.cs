namespace InkRecognition;

/// <summary>Fresh global TOP273 glyph pool and plain279/275 ownership. This
/// derives the same fresh capture/C4/B9 policy; it does not select alternatives
/// or manufacture WORD allocator bytes.</summary>
public sealed class LegacyFreshMainGlobalPlainResultOwnership
{
    internal LegacyGlobalSelectedCompactOwner CompactOwner {get;}
    public LegacyFreshMainGlobalCompactWordMaterialization Materialization=>CompactOwner.RequireOld();
    public LegacyFreshMainGlobalPublishedTerminalResult PublishedTerminal=>CompactOwner.RequirePublished();
    public LegacyFreshMainGlobalSourceGeometry Geometry{get;}
    public LegacyNativeGlyphRecordPool GlyphPool{get;}
    public LegacyNativePlainWordPublisher WordPublisher{get;}
    public bool CaptureGlyphGeometry=>false;
    public bool OwnerC4=>false;
    private LegacyFreshMainGlobalPlainResultOwnership(LegacyGlobalSelectedCompactOwner materialization,
        LegacyFreshMainGlobalSourceGeometry geometry,LegacyNativeGlyphRecordPool pool,LegacyNativePlainWordPublisher publisher)
    {CompactOwner=materialization;Geometry=geometry;GlyphPool=pool;WordPublisher=publisher;}
    public static LegacyFreshMainGlobalPlainResultOwnership Create(LegacyFreshMainGlobalCompactWordMaterialization materialization,
        LegacyFreshMainGlobalSourceGeometry geometry,CancellationToken token=default)
        => CreateCore(LegacyGlobalSelectedCompactOwner.FromOld(materialization),geometry,token);
    public static LegacyFreshMainGlobalPlainResultOwnership CreatePublished(LegacyFreshMainGlobalPublishedTerminalResult selected,
        LegacyFreshMainGlobalSourceGeometry geometry,CancellationToken token=default)
        => CreateCore(LegacyGlobalSelectedCompactOwner.FromPublished(selected,token),geometry,token);
    private static LegacyFreshMainGlobalPlainResultOwnership CreateCore(LegacyGlobalSelectedCompactOwner materialization,
        LegacyFreshMainGlobalSourceGeometry geometry,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(materialization);ArgumentNullException.ThrowIfNull(geometry);token.ThrowIfCancellationRequested();
        var pass=materialization.Pass;var policy=pass.Policy;
        if(!ReferenceEquals(materialization.Confidence,geometry.Confidence)||!ReferenceEquals(policy,geometry.Policy)||
            !ReferenceEquals(policy.Owner.Graph,geometry.Graph))
            throw new ArgumentException("Same actual global compact/source geometry/controller/confidence owner required.");
        if(policy.Owner.Model.SourceKind!=0x135||policy.HasAssociatedSource||policy.Configuration.Trace2368!=0||
            policy.Configuration.Mode22f6!=0||policy.Refresh.KnowledgeOption22f8!=0||policy.Refresh.SourceFormat22f9!=0)
            throw new NotSupportedException("Fresh nontracing no-associated global ordinary135 plain result required.");
        pass.ValidateResultQueryLease(token);
        //The private fresh raw factory excludes reused/event property state:
        //155DA0/155EF0 driver220=0 ->157C60 TOP41 ->274capture10=0.
        //157D29/15855E pass that SAME TOP to240.V80=2271F0; its273 reset
        //13BB50 preserves41 and the274 pool. See the specific global auditor.
        //00C860 C4=0; actual selected247B9=SourceFormat22F9 selects no sort/finalize.
        var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false,token);
        var publisher=new LegacyNativePlainWordPublisher(pool,false,false,false);
        pass.ValidateResultQueryLease(token);return new(materialization,geometry,pool,publisher);
    }
    public LegacyNativeWordBuilder CreateWorkingWord(ReadOnlySpan<byte> emptyAllocatorResidue120,CancellationToken token=default)
    {
        CompactOwner.Validate(token);
        return LegacyNativeWordBuilder.CreateEmpty(GlyphPool,emptyAllocatorResidue120,token);
    }
}
