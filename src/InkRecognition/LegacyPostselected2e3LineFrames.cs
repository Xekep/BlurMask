namespace InkRecognition;

/// <summary>Ordinary unknown-row2e3 NN grouping plus07a850 group postselection and explicit24a geometry.</summary>
public static class LegacyPostselected2e3LineFrames
{
    public static LegacyPrepared2e3LineFrameSnapshot Infer(LegacyOrdinaryLineScaleSnapshot source,
        PreparedLineBoundaryClassifier spatial30, PreparedLineBoundaryClassifier physical50,
        LegacyAutomaticLineProfile geometryProfile, float windowWidthMultiplier, float overlap,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(source.Source);
        ArgumentNullException.ThrowIfNull(spatial30); ArgumentNullException.ThrowIfNull(physical50);
        geometryProfile.Validate(); cancellationToken.ThrowIfCancellationRequested();
        if(spatial30.HiddenWidth!=30 || physical50.HiddenWidth!=50 || !float.IsFinite(windowWidthMultiplier) ||
            windowWidthMultiplier<=0 || windowWidthMultiplier>100 || !float.IsFinite(overlap) || overlap<0 || overlap>=1 ||
            !float.IsFinite(source.PrimaryScale) || source.PrimaryScale<=0)
            throw new ArgumentException("Invalid explicit prepared2e3 configuration.");
        var strokes=source.Source.Strokes.Select(s=>s.Points.ToArray()).ToArray();
        var records=strokes.Select(s=>LegacyLineStrokeStatistics.Extract(s,cancellationToken)).ToArray();
        LegacyUnknownLineGroups selected;LegacyNeuralLinePair[] pairs;
        if(strokes.Length==1)
        {
            selected=new(Array.AsReadOnly(new ReadOnlyMemory<int>[] { new int[] {0} }),new int[]{0});pairs=[];
        }
        else
        {
            var proposals=LegacyNeuralLineProposals.Prepare(records,Enumerable.Repeat(true,strokes.Length).ToArray(),
                source.PrimaryScale*windowWidthMultiplier,overlap,cancellationToken);
            pairs=LegacyNeuralLineProposals.Score(records,proposals,source.PrimaryScale,spatial30,physical50,cancellationToken);
            selected=LegacyUnknownLineGroupSelector.Select(records,pairs,source.PrimaryScale,cancellationToken);
        }
        selected=LegacyUnknownLinePostselection.Apply(records,selected,source.PrimaryScale,cancellationToken);
        var output=new List<LegacyPrepared2e3LineFrame>();
        foreach(var group in selected.Groups)
        {
            cancellationToken.ThrowIfCancellationRequested();int[] ids=group.ToArray();Array.Sort(ids);
            var row=ids.Select(i=>strokes[i]).ToArray();
            var estimate=LegacyAutomaticLineEstimator.Estimate(row,geometryProfile,cancellationToken);
            output.Add(new(ids,estimate,LegacyLineDeskew.Apply(row,estimate.Line,(float)estimate.Slope,cancellationToken)));
        }
        return new(source,output.AsReadOnly(),selected.StrokeGroupIndices,Array.AsReadOnly(pairs));
    }
}
