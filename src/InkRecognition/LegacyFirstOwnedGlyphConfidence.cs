namespace InkRecognition;

/// <summary>First1BCBD0 ordinary-group confidence, including its raw source21860
/// enclosure branch. Owns the selected factory, prepared cuts, candidate graph
/// and their actual NN/cache groups. No symbol selection or script correction.</summary>
public sealed class LegacyFirstOwnedGlyphConfidence
{
    private readonly LegacyFreshFirstResultOwnershipFactory factory;
    private readonly LegacySmoothedRowCutResult cuts;
    private readonly LegacyOrdinaryCursiveCandidateGraph graph;
    private readonly LegacyFirstOrdinaryCacheScores[] scores;
    private readonly LegacyFirstOrdinaryGlyphScoreProducer neural;
    private readonly ushort closedPrimitive;

    public LegacyFirstOwnedGlyphConfidence(LegacyFirstOrdinaryGlyphScoreProducer neural,
        LegacyFreshFirstResultOwnershipFactory factory,LegacySmoothedRowCutResult cuts,
        LegacyOrdinaryCursiveCandidateGraph graph,IReadOnlyList<LegacyFirstOrdinaryCacheScores> actualGroupScores,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(neural);ArgumentNullException.ThrowIfNull(factory);ArgumentNullException.ThrowIfNull(cuts);
        ArgumentNullException.ThrowIfNull(graph);ArgumentNullException.ThrowIfNull(actualGroupScores);
        cancellationToken.ThrowIfCancellationRequested();
        if(neural.SourceFingerprint!=factory.SourceFingerprint||
            !ReferenceEquals(cuts.Owner,factory.Frame.Row)||actualGroupScores.Count!=graph.Groups.Count||
            cuts.Cuts.PreparedInk.StrokePointCounts.Length!=factory.Frame.Row.OriginalSource.Strokes.Count)
            throw new ArgumentException("Actual first factory/cuts/graph groups must share this selected source owner.");
        var alphabet=factory.InvocationPolicy.Binding.Alphabet;
        if(!alphabet.TryGetLabel("\U000f0001",out closedPrimitive)||neural.HasNeuralOutputFor(closedPrimitive))
            throw new NotSupportedException("This owned source branch requires a closed primitive without an NN prototype.");
        this.neural=neural;
        this.factory=factory;this.cuts=cuts;this.graph=graph;scores=actualGroupScores.ToArray();
        if(scores.Any(s=>s is null))throw new ArgumentException("Null actual group scores.",nameof(actualGroupScores));
    }

    public float ReadSymbolScore(int nativeGroupIndex,ushort symbol,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if((uint)nativeGroupIndex>=(uint)scores.Length)throw new ArgumentOutOfRangeException(nameof(nativeGroupIndex));
        var actual=scores[nativeGroupIndex];ArgumentNullException.ThrowIfNull(actual.Glyph);ArgumentNullException.ThrowIfNull(actual.Cache);
        factory.ValidateScores(actual.Glyph.Scores,cancellationToken);
        if(!ReferenceEquals(actual.Glyph.Geometry.PreparedInk,cuts.Cuts.PreparedInk)||actual.Cache.Candidates.Length>150)
            throw new ArgumentException("Actual NN/cache group must retain this prepared source owner.");
        var ranges=graph.Groups[nativeGroupIndex].Ranges.Span;
        if(ranges.Length is <1 or >15)throw new ArgumentException("Bounded ordinary range-vector required.");
        int[] order=cuts.Chronology.OrderRangeIndicesForNormalization(ranges,cancellationToken);
        var normalizedRanges=actual.Glyph.Geometry.Ranges.Span;
        if(normalizedRanges.Length!=ranges.Length)throw new ArgumentException("Graph/NN ranges differ.");
        for(int i=0;i<order.Length;i++)if(normalizedRanges[i]!=ranges[order[i]])throw new ArgumentException("Graph/NN range ownership differs.");
        if(symbol!=closedPrimitive)return neural.ReadSymbolScore(actual.Glyph.Scores,actual.Cache,symbol,cancellationToken);
        int rank=0;
        foreach(var candidate in actual.Cache.Candidates.Span)
        {cancellationToken.ThrowIfCancellationRequested();if(candidate.Symbol==symbol)break;rank++;}
        if(rank>=actual.Cache.Candidates.Length||rank>=20)return 0;
        //1A7970 copies the ordinary group's actual range-vector.1BCBD0 then
        //dereferences its first U32 physical ID and calls218.V60=168C00.
        //This uses graph range order, not normalized/chronological point order.
        int physical=ranges[0].StrokeIndex;
        var source=factory.Frame.Row.OriginalSource;
        if((uint)physical>=(uint)source.SourceStrokeProbabilities.Length)
            throw new ArgumentException("Group first physical range is outside its original source.");
        float value=source.SourceStrokeProbabilities.Span[physical];
        if(!float.IsFinite(value))throw new ArgumentException("Nonfinite original source enclosure probability.");
        return value; //No sigmoid, two-family mean, averaging or clamp.
    }
}
