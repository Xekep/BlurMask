namespace InkRecognition;

/// <summary>Actual no-associated2271F0 TOP.AC and221840/2188F0 TOP.B0
/// prepass over the global source's genuine flush plan. WORD120 and label-stack
/// allocator residue are separate; this type invents neither seed storage nor
/// a publication callback.</summary>
public sealed class LegacyFreshMainGlobalSourceWidthStatistics
{
    public LegacyFreshMainGlobalGlyphFlushPlan Plan{get;}
    public LegacyFreshMainGlobalSourceGeometry Geometry{get;}
    public LegacyMainSourceWidthStatistics Statistics{get;}
    public uint TopAcBits=>2;
    public uint TopB0Bits=>BitConverter.SingleToUInt32Bits(Statistics.AverageWidth);
    private LegacyFreshMainGlobalSourceWidthStatistics(LegacyFreshMainGlobalGlyphFlushPlan plan,
        LegacyFreshMainGlobalSourceGeometry geometry,LegacyMainSourceWidthStatistics statistics)
    {Plan=plan;Geometry=geometry;Statistics=statistics;}
    public static LegacyFreshMainGlobalSourceWidthStatistics Prepare(LegacyFreshMainGlobalGlyphFlushPlan plan,
        LegacyFreshMainGlobalSourceGeometry geometry,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(plan);ArgumentNullException.ThrowIfNull(geometry);token.ThrowIfCancellationRequested();
        var backtrack=plan.BacktrackRows;var materialization=backtrack.CompactOwner;
        var pass=materialization.Pass;var policy=pass.Policy;
        if(!ReferenceEquals(backtrack.SourceCallbacks.Geometry,geometry)||!ReferenceEquals(geometry.Policy,policy)||
            !ReferenceEquals(geometry.Confidence,materialization.Confidence))
            throw new ArgumentException("Actual global compact/flush/source geometry owner identity required.");
        if(policy.HasAssociatedSource||policy.Owner.Model.SourceKind!=0x135||policy.Configuration.Trace2368!=0||
            policy.Configuration.Mode22f6!=0||policy.Refresh.KnowledgeOption22f8!=0)
            throw new NotSupportedException("Fresh no-associated ordinary global width prepass required.");
        pass.ValidateResultQueryLease(token);byte[] compact=materialization.CopyCompactStorage();
        //221840 initializes the double sum and signed count to zero. This
        //is an actual accumulator initialization, not a guessed WORD seed.
        LegacyMainSourceWidthStatistics sum=default;
        foreach(var flush in plan.Flushes)
        {
            token.ThrowIfCancellationRequested();
            if(!ReferenceEquals(flush.BacktrackRows,backtrack))throw new InvalidDataException("One actual flush chronology required.");
            sum=LegacyMainSourceWidthKernel.Accumulate(flush.CopyRows72(),compact,materialization.RecordCount,sum,
                (node,arc)=>geometry.ReadBounds(node,arc,token),token);
        }
        //2219DA explicitly writes TOP.B0 zero for a nonpositive count. With
        //a positive count2219D0 stores the float conversion of the double mean.
        pass.ValidateResultQueryLease(token);return new(plan,geometry,sum);
    }
}
