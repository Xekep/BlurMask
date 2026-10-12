using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyOrdinaryRowAcousticProfile(LegacyOrdinarySourceProfile Source, int GeometryNodeOffset,
    int SmoothingNodeOffset, LegacyAutomaticLineProfile Geometry, LegacyOrientationSmoothingProfile Smoothing)
{
    /// <summary>
    /// Read the current per-row source247/v10d, not the outer247's later postprocessor list.
    /// Supported fresh profile has no early processors and exactly24a then265.
    /// </summary>
    public static LegacyOrdinaryRowAcousticProfile ReadResourceNode(ReadOnlySpan<byte> resource, int sourceOffset)
    {
        var source = LegacyOrdinarySourceInk.ReadV10dResourceProfile(resource, sourceOffset);
        int flags = checked(source.ParametersOffset + 16);
        if (resource.Slice(flags, 4).IndexOfAnyExcept((byte)0) >= 0)
            throw new NotSupportedException("Per-row acoustic source requires no early grouping/orientation processors.");
        int list = checked(source.EnclosureFlagOffset + 12);
        if (BinaryPrimitives.ReadInt32LittleEndian(resource[list..]) != 2)
            throw new NotSupportedException("Per-row acoustic source requires exactly24a then265.");
        int geometry = list + 4;
        var geometryProfile = LegacyAutomaticLineProfile.ReadResourceNode(resource, geometry);
        int smoothing = geometry + 36;
        var smoothingProfile = LegacyOrientationSmoothingProfile.ReadV102Node(resource, smoothing);
        return new(source, geometry, smoothing, geometryProfile, smoothingProfile);
    }
}

public sealed class LegacySmoothedRowCutResult
{
    internal LegacyOrdinaryRowAcousticInput Owner { get; }
    public LegacyCursiveCutResult Cuts { get; }
    public LegacySourcePointChronology Chronology { get; }
    internal LegacySmoothedRowCutResult(LegacyOrdinaryRowAcousticInput owner, LegacyCursiveCutResult cuts, LegacySourcePointChronology chronology)
    { Owner=owner;Cuts=cuts;Chronology=chronology; }
}

/// <summary>
/// Fresh already-selected ordinary row: current247 profile→24a estimate/16e570 deskew→265 smoothing/key map.
/// Input row selection and postrecognition normalization are separate; no native word winner is synthesized.
/// </summary>
public sealed class LegacyOrdinaryRowAcousticInput
{
    public LegacyOrdinarySourceInk OriginalSource { get; }
    public LegacyOrdinaryRowAcousticProfile Profile { get; }
    public LegacyAutomaticLineEstimate Estimate { get; }
    public LegacyDeskewedLine Frame { get; }
    public LegacyOrientationStrokePreparationWithKeysResult Prepared { get; }
    public LegacySourcePointChronology Chronology { get; }

    private LegacyOrdinaryRowAcousticInput(LegacyOrdinarySourceInk source, LegacyOrdinaryRowAcousticProfile profile,
        LegacyAutomaticLineEstimate estimate, LegacyDeskewedLine frame, LegacyOrientationStrokePreparationWithKeysResult prepared,
        LegacySourcePointChronology chronology)
    { OriginalSource=source;Profile=profile;Estimate=estimate;Frame=frame;Prepared=prepared;Chronology=chronology; }

    public static LegacyOrdinaryRowAcousticInput Prepare(IReadOnlyList<InkStroke> rawRow,
        LegacyOrdinaryRowAcousticProfile profile, CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(profile);cancellationToken.ThrowIfCancellationRequested();
        var source=LegacyOrdinarySourceInk.Prepare(rawRow,profile.Source,cancellationToken);
        var metrics=LegacySourceOrientationPreparation.ExtractMetrics(source,cancellationToken);
        var original=source.Strokes.Select(s=>s.Points.ToArray()).ToArray();
        var estimate=LegacyAutomaticLineEstimator.Estimate(original,profile.Geometry,cancellationToken);
        //Fresh16a130 state168 is zero: height is the fitted band, rather than caller forced scale.
        var frame=LegacyLineDeskew.Apply(original,estimate.Line,(float)estimate.Slope,cancellationToken);
        var transformed=frame.Strokes.Select(points=>new InkStroke(points)).ToArray();
        var prepared=LegacyOrientationStrokePreparation.PrepareWithPointKeys(transformed,profile.Smoothing,frame.Line.Height,metrics,cancellationToken);
        var originalKeys=new List<LegacySourcePointKey>();
        for(int s=0;s<source.Strokes.Count;s++)for(int p=0;p<source.Strokes[s].Points.Count;p++)
            originalKeys.Add(source.Chronology.Resolve(s,p).Key);
        //Native16a130 initializes sourceFactor128 to200. The original unsampled
        //chronology uses factor1 as a convenience; do not truncate265's fractions to1.
        var chronology=LegacySourcePointChronology.CreatePrepared(prepared.PhysicalPointCounts.Span,originalKeys.ToArray(),
            Enumerable.Range(0,originalKeys.Count).ToArray(),prepared.PointKeys200.ToArray().Select(k=>checked((uint)k)).ToArray(),200);
        return new(source,profile,estimate,frame,prepared,chronology);
    }

    public LegacySmoothedRowCutResult GenerateRussianCuts(bool wholeStrokeOnly=false,int? closedCategoryLimit=null,
        CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var cuts=LegacyCursiveCutProducer.GenerateRussianVersion106(Prepared.Strokes,Frame.Line.Baseline,Frame.Line.Height,
            OriginalSource.SourceStrokeProbabilities.ToArray(),wholeStrokeOnly,closedCategoryLimit,cancellationToken);
        return new(this,cuts,Chronology.WithInsertedPreparedInk(cuts.PreparedInk,cancellationToken));
    }

    public LegacyPreparedWordGroupEvaluator CreateEvaluator(LegacyRussianModelBundle model, LegacySmoothedRowCutResult cuts,
        LegacyCursiveWordSearchOptions options, CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(cuts);ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(model);
        if(!ReferenceEquals(cuts.Owner,this) || model.SourceKind!=Profile.Source.SourceKind ||
            options.SourceBaseline!=Frame.Line.Baseline || options.SourceHeight!=Frame.Line.Height)
            throw new ArgumentException("Evaluator must consume this row's native prepared frame.");
        return new(model,Prepared.Strokes,cuts.Cuts.PreparedInk,cuts.Chronology,options,cancellationToken);
    }
}
