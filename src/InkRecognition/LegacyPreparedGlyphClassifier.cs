namespace InkRecognition;

public sealed record LegacyPreparedGlyphClassification(LegacyNormalizedPreparedGlyph Geometry,
    LegacyDirectionalGlyphScores Scores,LegacyDynamicAcousticWeightResult DynamicWeights,
    ReadOnlyMemory<float> ResolvedWeights,ReadOnlyMemory<int> IntegerCosts);

/// <summary>
/// Composed checked endpoint: supplied prepared float ranges → FF/FF short
/// normalization → four-family classifier, plus resource-configured geometric
/// acoustic weights and integer costs. Cuts, line frame/order, context, clipping
/// flags, enabled families/classes and multiplier remain explicit upstream data.
/// </summary>
public sealed class LegacyPreparedGlyphClassifier
{
    private readonly LegacyDirectionalGlyphClassifier classifier;
    private readonly LegacyDirectionalAcousticWeightConfiguration weights;
    public LegacyPreparedGlyphClassifier(LegacyDirectionalGlyphClassifier classifier,
        LegacyDirectionalAcousticWeightConfiguration weights)
    {
        ArgumentNullException.ThrowIfNull(classifier);ArgumentNullException.ThrowIfNull(weights);
        for(int i=0;i<7;i++)
            if(weights.Slots.Span[i].NetworkMask!=(i<4?2:0))
                throw new ArgumentException("Expected checked four-family second-network profile.",nameof(weights));
        this.classifier=classifier;this.weights=weights;
    }

    public LegacyPreparedGlyphClassification Classify(LegacyCursivePreparedInk preparedInk,
        ReadOnlySpan<LegacyInkRange> orderedRanges,float candidateMinimumX,float sourceBaseline,
        float sourceHeight,short targetHeight,int maximumRangesBeforeMerge,
        ReadOnlySpan<short> context,short startBoundary,short endBoundary,
        bool includeBaselineFamilies,ReadOnlySpan<byte> enabledFamilySlots,float costMultiplier,
        ReadOnlySpan<byte> enabledClasses=default,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(enabledFamilySlots.Length!=7)throw new ArgumentException("Seven explicit enabled-family flags required.",nameof(enabledFamilySlots));
        foreach(byte flag in enabledFamilySlots)if(flag>1)throw new ArgumentException("Family flags require0/1.",nameof(enabledFamilySlots));
        var geometry=LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(preparedInk,orderedRanges,
            candidateMinimumX,sourceBaseline,sourceHeight,targetHeight,maximumRangesBeforeMerge,cancellationToken);
        var raw=new InkStroke[geometry.Ranges.Length];var counts=preparedInk.StrokePointCounts.Span;
        var starts=new int[counts.Length];for(int i=1;i<starts.Length;i++)starts[i]=starts[i-1]+counts[i-1];
        for(int i=0;i<raw.Length;i++)
        {
            var range=geometry.Ranges.Span[i];var points=new InkPoint[range.EndPointExclusive-range.StartPoint];
            for(int j=0;j<points.Length;j++)
            {
                var p=preparedInk.Points.Span[starts[range.StrokeIndex]+range.StartPoint+j];points[j]=new(p.X,p.Y);
            }
            raw[i]=new(Array.AsReadOnly(points));
        }
        var dynamic=LegacyDynamicAcousticWeights.Evaluate(raw,weights.DynamicProfile,cancellationToken);
        var scores=classifier.Classify(geometry.Strokes,geometry.Baseline,context,startBoundary,endBoundary,
            includeBaselineFamilies,cancellationToken);
        float[] resolved=weights.ResolveWeights(dynamic,cancellationToken);
        for(int i=0;i<7;i++)
        {
            if(enabledFamilySlots[i]==0)resolved[i]=0;
            else if(scores.FamilyProbabilities[i] is null)
                throw new ArgumentException("An enabled family was not evaluated by the supplied classifier.",nameof(enabledFamilySlots));
        }
        var costs=scores.GetIntegerCosts(resolved,costMultiplier,enabledClasses,cancellationToken);
        return new(geometry,scores,dynamic,resolved,costs);
    }
}
