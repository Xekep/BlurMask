namespace InkRecognition;

public sealed record LegacyFirstPreparedAcousticScores(LegacyNormalizedPreparedGlyph Geometry,
    LegacyNormalizedPenUpMergeResult MergedGeometry,LegacyFirstAcousticScores Scores,ReadOnlyMemory<int> IntegerCosts);

/// <summary>Owned first-model source row/cut geometry with explicit caller context,
/// clipped boundaries and pen-up threshold. No automatic context or final class cache.</summary>
public sealed class LegacyFirstPreparedAcousticClassifier
{
    public LegacyFirstAcousticModel Model {get;}
    private readonly LegacyInkActivations activations;
    private LegacyFirstPreparedAcousticClassifier(LegacyFirstAcousticModel model,LegacyInkActivations activations)
    {Model=model;this.activations=activations;}

    public static LegacyFirstPreparedAcousticClassifier ReadSource(ReadOnlySpan<byte> resource,int sourceOffset,
        LegacyInkActivations activations,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(activations);var model=LegacyFirstAcousticModel.ReadSource(resource,sourceOffset,cancellationToken);
        if(resource[model.OrdinaryNodeOffset+16]!=0xff||resource[model.OrdinaryNodeOffset+17]!=0xff)
            throw new NotSupportedException("Only checked first-model FF/FF source normalization is composed.");
        return new(model,activations);
    }

    public LegacyFirstPreparedAcousticScores Classify(LegacyOrdinaryRowAcousticInput row,LegacySmoothedRowCutResult cuts,
        ReadOnlySpan<LegacyInkRange> ranges,ReadOnlySpan<short> context,short startBoundary,short endBoundary,
        int maximumRangesBeforeMerge,int maximumStrokesAfterMerge,
        ReadOnlySpan<byte> enabledClasses=default,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(row);ArgumentNullException.ThrowIfNull(cuts);cancellationToken.ThrowIfCancellationRequested();
        if(!ReferenceEquals(cuts.Owner,row)||row.Profile!=Model.RowProfile)
            throw new ArgumentException("Row/cuts must belong to the loaded first acoustic source profile.");
        if(context.Length!=28||startBoundary is not (-1024 or 1024)||endBoundary is not (-1024 or 1024))
            throw new ArgumentException("Explicit28 context and +/-1024 boundary channels required.");
        var ink=cuts.Cuts.PreparedInk;int[] order=cuts.Chronology.OrderRangeIndicesForNormalization(ranges,cancellationToken);
        var ordered=new LegacyInkRange[order.Length];for(int i=0;i<order.Length;i++)ordered[i]=ranges[order[i]];
        var counts=ink.StrokePointCounts.Span;var starts=new int[counts.Length];for(int i=1;i<starts.Length;i++)starts[i]=starts[i-1]+counts[i-1];
        float minimumX=float.MaxValue;
        foreach(var range in ordered)
        {
            if((uint)range.StrokeIndex>=(uint)counts.Length||range.StartPoint<0||range.EndPointExclusive<=range.StartPoint||range.EndPointExclusive>counts[range.StrokeIndex])
                throw new ArgumentException("Invalid prepared range.",nameof(ranges));
            for(int p=range.StartPoint;p<range.EndPointExclusive;p++)
            {cancellationToken.ThrowIfCancellationRequested();minimumX=Math.Min(minimumX,ink.Points.Span[starts[range.StrokeIndex]+p].X);}
        }
        var geometry=LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(ink,ordered,minimumX,row.Frame.Line.Baseline,
            row.Frame.Line.Height,Model.NormalizationTarget,maximumRangesBeforeMerge,cancellationToken);
        var merged=LegacyNormalizedPenUpMerge.Merge(geometry.Strokes,maximumStrokesAfterMerge,cancellationToken);
        short[] trajectory=new short[247],raster=new short[229];
        LegacyStrokeFeatures.Extract(merged.Strokes,cancellationToken).CopyTo(trajectory,0);
        LegacyRasterFeatures.Extract(merged.Strokes,cancellationToken).CopyTo(raster,0);
        context.CopyTo(trajectory.AsSpan(217,28));context.CopyTo(raster.AsSpan(199,28));
        trajectory[^2]=raster[^2]=startBoundary;trajectory[^1]=raster[^1]=endBoundary;
        var scores=Model.EvaluatePreparedInputs(trajectory,raster,activations,cancellationToken);
        return new(geometry,merged,scores,scores.GetFreshIntegerCosts(enabledClasses,cancellationToken));
    }
}
