namespace InkRecognition;

/// <summary>
/// Reusable neural/context group stage over owned prepared cuts and source
/// chronology. Whole-line context frame is the existing original diagnostic
/// policy; line metrics, boundary suppression and neural families are explicit.
/// This stage does not create cuts, seed graphs, caches or word-search defaults.
/// </summary>
public sealed class LegacyPreparedWordGroupEvaluator
{
    private readonly LegacyRussianModelBundle model;
    private readonly LegacyPreparedGlyphClassifier classifier;
    private readonly LegacyCursivePreparedInk ink;
    private readonly LegacySourcePointChronology chronology;
    private readonly LegacyCursiveWordSearchOptions options;
    private readonly LegacyPriorContextGroup[] contextGroups;
    private readonly int[] starts;
    private readonly byte[] enabled;
    private readonly float lineMinX;
    public int ContextWindowCount => contextGroups.Length;

    public LegacyPreparedWordGroupEvaluator(LegacyRussianModelBundle model,
        LegacyOrdinaryCursiveCutSource source, LegacyCursiveWordSearchOptions options,
        CancellationToken cancellationToken = default)
        : this(model, source?.Source.Strokes ?? throw new ArgumentNullException(nameof(source)),
            source.Cuts.PreparedInk, source.Chronology, options, cancellationToken) { }

    /// <summary>
    /// Prepared ink and chronology must address the supplied source snapshot.
    /// The caller owns that provenance on this explicit endpoint; use the
    /// ordinary-source overload to compose verified ingress/point identities.
    /// </summary>
    public LegacyPreparedWordGroupEvaluator(LegacyRussianModelBundle model,
        IReadOnlyList<InkStroke> source, LegacyCursivePreparedInk preparedInk,
        LegacySourcePointChronology chronology, LegacyCursiveWordSearchOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model); ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(preparedInk); ArgumentNullException.ThrowIfNull(chronology);
        ArgumentNullException.ThrowIfNull(options); cancellationToken.ThrowIfCancellationRequested();
        if (!float.IsFinite(options.SourceBaseline) || !float.IsFinite(options.SourceHeight) || options.SourceHeight <= 0 ||
            options.TargetHeight <= 0 || !float.IsFinite(options.CostMultiplier) || options.CostMultiplier <= 0 ||
            !float.IsFinite(options.ContextLeftGapOffset) || !float.IsFinite(options.ContextLeftVariance) || options.ContextLeftVariance <= 0 ||
            !float.IsFinite(options.ContextRightVariance) || options.ContextRightVariance <= 0 ||
            source.Count != preparedInk.StrokePointCounts.Length ||
            !preparedInk.StrokePointCounts.Span.SequenceEqual(chronology.StrokePointCounts.Span))
            throw new ArgumentException("Matching source/prepared chronology and explicit finite line/context policy required.");
        this.model = model; this.options = options; ink = preparedInk; this.chronology = chronology;
        classifier = model.CreatePreparedGlyphClassifier();
        var original = LegacyCursivePreparedInk.Create(source, cancellationToken);
        lineMinX = original.Points.Span.ToArray().Min(p => p.X);
        var counts = original.StrokePointCounts.Span;
        var normalizedLine = new LegacyInkStroke[source.Count];
        for (int s = 0; s < source.Count; s++)
            normalizedLine[s] = LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(original,
                [new(s, 0, counts[s])], lineMinX, options.SourceBaseline, options.SourceHeight,
                options.TargetHeight, 1, cancellationToken).Strokes[0];
        contextGroups = model.CreateContextWindowScorer().ScoreLine(normalizedLine, 0, options.TargetHeight,
            0, new int[source.Count], cancellationToken: cancellationToken);
        starts = new int[ink.StrokePointCounts.Length];
        for (int i = 1; i < starts.Length; i++) starts[i] = starts[i-1] + ink.StrokePointCounts.Span[i-1];
        enabled = options.IncludeBaselineFamilies ? [1,1,1,1,0,0,0] : [0,0,1,1,0,0,0];
    }

    /// <summary>
    /// Evaluate arbitrary supplied group ranges, including those obtained from
    /// a nonlinear candidate graph. Neural-group budget is caller search policy.
    /// Normalization sorts by native raw-point keys, retaining equal-key swaps.
    /// </summary>
    public LegacyPreparedGlyphClassification Classify(ReadOnlySpan<LegacyInkRange> ranges,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (ranges.Length is < 1 or > 10)
            throw new ArgumentException("Expected1..10 pre-merge glyph ranges.", nameof(ranges));
        int[] order = chronology.OrderRangeIndicesForNormalization(ranges, cancellationToken);
        var sorted = new LegacyInkRange[order.Length];
        for (int i = 0; i < sorted.Length; i++) sorted[i] = ranges[order[i]];
        float minimumX = float.MaxValue, sumX = 0; int count = 0;
        foreach (var range in sorted)
            for (int point = range.StartPoint; point < range.EndPointExclusive; point++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var p = ink.Points.Span[starts[range.StrokeIndex] + point];
                minimumX = Math.Min(minimumX, p.X); sumX += p.X; count++;
            }
        var physicalIds = sorted.Select(r => r.StrokeIndex).Distinct().ToArray();
        var observations = LegacyPriorContextSelection.SelectNative(contextGroups, 0, physicalIds,
            model.AcousticWeights.ContextSelectionMode, cancellationToken: cancellationToken);
        float centerX = ((sumX / count - lineMinX) * options.TargetHeight) / options.SourceHeight;
        var context = LegacyFloatInputQuantization.Quantize(LegacyContextWeighting.Combine(observations,
            centerX, options.TargetHeight, options.ContextLeftGapOffset, options.ContextLeftVariance,
            options.ContextRightVariance, cancellationToken), cancellationToken: cancellationToken);
        var signs = options.UsePreparedBoundarySigns ? LegacyCursiveGlyphBoundaryPolicy.Resolve(sorted,
            ink.StrokePointCounts.Span, 0, options.SuppressStartBoundary, options.SuppressEndBoundary, cancellationToken) :
            new LegacyCursiveGlyphBoundarySigns(options.StartBoundary, options.EndBoundary);
        return classifier.Classify(ink, sorted, minimumX, options.SourceBaseline, options.SourceHeight,
            options.TargetHeight, 10, context, signs.Start, signs.End, options.IncludeBaselineFamilies,
            enabled, options.CostMultiplier, cancellationToken: cancellationToken);
    }
}
