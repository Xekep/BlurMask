namespace InkRecognition;

public sealed record LegacyPrepared2e3LineFrame(ReadOnlyMemory<int> PhysicalStrokeIndices,
    LegacyAutomaticLineEstimate Estimate, LegacyDeskewedLine Deskewed);

public sealed record LegacyPrepared2e3LineFrameSnapshot(LegacyOrdinaryLineScaleSnapshot ScaleSource,
    IReadOnlyList<LegacyPrepared2e3LineFrame> Lines, ReadOnlyMemory<int> StrokeLineIndices,
    IReadOnlyList<LegacyNeuralLinePair> PairEvidence);

/// <summary>
/// Resource-backed ordinary source→scale→prepared2e3 proposals/NN→unknown-row
/// selection→explicit24a geometry callback/deskew. Handles upstream07ca80's
/// single-active-stroke branch. Full07a850 postselection, configuration-based
/// choice2e2/2e3 and final native winner/acceptance remain outside this scope.
/// </summary>
public static class LegacyPrepared2e3LineFrames
{
    public static LegacyPrepared2e3LineFrameSnapshot Infer(IReadOnlyList<InkStroke> rawSource,
        ReadOnlySpan<byte> acousticResource, float inputResolution,
        PreparedLineBoundaryClassifier spatial30, PreparedLineBoundaryClassifier physical50,
        LegacyAutomaticLineProfile geometryProfile, float windowWidthMultiplier, float overlap,
        CancellationToken cancellationToken = default)
    {
        var source = LegacyOrdinaryLineScaleSource.Prepare(rawSource, acousticResource, inputResolution,
            cancellationToken: cancellationToken);
        return Infer(source, spatial30, physical50, geometryProfile, windowWidthMultiplier, overlap, cancellationToken);
    }

    public static LegacyPrepared2e3LineFrameSnapshot Infer(LegacyOrdinaryLineScaleSnapshot source,
        PreparedLineBoundaryClassifier spatial30, PreparedLineBoundaryClassifier physical50,
        LegacyAutomaticLineProfile geometryProfile, float windowWidthMultiplier, float overlap,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(source.Source);
        ArgumentNullException.ThrowIfNull(spatial30); ArgumentNullException.ThrowIfNull(physical50);
        geometryProfile.Validate(); cancellationToken.ThrowIfCancellationRequested();
        if (spatial30.HiddenWidth != 30 || physical50.HiddenWidth != 50 ||
            !float.IsFinite(windowWidthMultiplier) || windowWidthMultiplier <= 0 || windowWidthMultiplier > 100 ||
            !float.IsFinite(overlap) || overlap < 0 || overlap >= 1 || source.PrimaryScale <= 0 || !float.IsFinite(source.PrimaryScale))
            throw new ArgumentException("Invalid explicit prepared2e3 configuration.");
        var strokes = source.Source.Strokes.Select(s => s.Points.ToArray()).ToArray();
        LegacyUnknownLineGroups selected; LegacyNeuralLinePair[] pairs;
        if (strokes.Length == 1)
        {
            //07d7c0→07ca80: one active ordinary stroke forms one physical group.
            selected = new(Array.AsReadOnly(new ReadOnlyMemory<int>[] { new int[] { 0 } }), new int[] { 0 });
            pairs = [];
        }
        else
        {
            var records = strokes.Select(s => LegacyLineStrokeStatistics.Extract(s, cancellationToken)).ToArray();
            var proposals = LegacyNeuralLineProposals.Prepare(records, Enumerable.Repeat(true, strokes.Length).ToArray(),
                source.PrimaryScale * windowWidthMultiplier, overlap, cancellationToken);
            pairs = LegacyNeuralLineProposals.Score(records, proposals, source.PrimaryScale, spatial30, physical50, cancellationToken);
            selected = LegacyUnknownLineGroupSelector.Select(records, pairs, source.PrimaryScale, cancellationToken);
        }
        var output = new List<LegacyPrepared2e3LineFrame>();
        foreach (var group in selected.Groups)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int[] ids = group.ToArray(); Array.Sort(ids); //source15b260 before geometry callback.
            var row = ids.Select(i => strokes[i]).ToArray();
            var estimate = LegacyAutomaticLineEstimator.Estimate(row, geometryProfile, cancellationToken);
            var transformed = LegacyLineDeskew.Apply(row, estimate.Line, (float)estimate.Slope, cancellationToken);
            output.Add(new(ids, estimate, transformed));
        }
        return new(source, output.AsReadOnly(), selected.StrokeGroupIndices, Array.AsReadOnly(pairs));
    }
}
