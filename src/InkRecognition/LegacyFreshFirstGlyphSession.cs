namespace InkRecognition;

/// <summary>Already-selected fresh ordinary row: raw ownership,24a/265 frame,
/// 297 cuts, first float context/NN, constructor/resource controller policy and
/// final cache. This is class23f acoustic-only input, not class240 dictionary search.</summary>
public sealed class LegacyFreshFirstGlyphSession
{
    public LegacyOrdinaryRowAcousticInput Row {get;}
    public LegacySmoothedRowCutResult Cuts {get;}
    public LegacyFreshFirstControllerPolicy Policy {get;}
    public LegacyFirstOrdinaryCacheScorer Scorer {get;}
    public LegacyFirstRowPrimitiveScope Scope {get;}
    private LegacyFreshFirstGlyphSession(LegacyOrdinaryRowAcousticInput row,LegacySmoothedRowCutResult cuts,
        LegacyFreshFirstControllerPolicy policy,LegacyFirstOrdinaryCacheScorer scorer,LegacyFirstRowPrimitiveScope scope)
    {Row=row;Cuts=cuts;Policy=policy;Scorer=scorer;Scope=scope;}

    public static LegacyFreshFirstGlyphSession Prepare(ReadOnlySpan<byte> resource,int sourceOffset,
        IReadOnlyList<InkStroke> rawSelectedRow,LegacyInkActivations activations,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(rawSelectedRow);cancellationToken.ThrowIfCancellationRequested();
        byte[] snapshot=resource.ToArray();var policy=LegacyFreshFirstControllerPolicy.ReadFirstSource(snapshot,sourceOffset,cancellationToken);
        var profile=LegacyOrdinaryRowAcousticProfile.ReadResourceNode(snapshot,sourceOffset);
        var row=LegacyOrdinaryRowAcousticInput.Prepare(rawSelectedRow,profile,cancellationToken);
        var cuts=row.GenerateRussianCuts(cancellationToken:cancellationToken);
        var scope=new LegacyFirstRowPrimitiveScope(row,cuts,rawSelectedRow.Select(s=>s.Points.Count).ToArray(),cancellationToken);
        var scorer=LegacyFirstOrdinaryCacheScorer.Prepare(snapshot,sourceOffset,row,activations,cancellationToken);
        return new(row,cuts,policy,scorer,scope);
    }

    /// <summary>Caller supplies native group ranges/kind and confirmed symbol mask/pairs.
    /// Pre-merge range bound remains explicit; output cap comes from the owned resource.
    /// Fresh sourceD8=false: no custom source-scope penalties are injected.</summary>
    public LegacyFirstOrdinaryCacheScores Score(ReadOnlySpan<LegacyInkRange> nativeOrderedRanges,int candidateKind,
        int maximumRangesBeforeMerge,ReadOnlySpan<byte> enabledSymbols,ReadOnlySpan<LegacyAcousticCachePair> nativePairs,
        CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();var ranges=nativeOrderedRanges.ToArray();
        int[] indices=Cuts.Chronology.OrderRangeIndicesForNormalization(ranges,cancellationToken);
        var normalizedOrder=indices.Select(i=>ranges[i]).ToArray();
        var boundary=LegacyCursiveGlyphBoundaryPolicy.Resolve(normalizedOrder,Cuts.Cuts.PreparedInk.StrokePointCounts.Span,
            candidateKind,false,false,cancellationToken);
        //The merge helper is bounded to256 strokes; no merge can be needed above
        //that under the supported <=15 source ranges, even when native cap32767.
        return Scorer.Score(Cuts,ranges,boundary.Start,boundary.End,maximumRangesBeforeMerge,
            Math.Min(256,Policy.MaximumNormalizedStrokes),enabledSymbols,
            glyph=>Policy.CreateContext(Scope.Evaluate(ranges,glyph.MergedGeometry.Strokes.Count,cancellationToken),[]),
            nativePairs,cancellationToken);
    }
}
