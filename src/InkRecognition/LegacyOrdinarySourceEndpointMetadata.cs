namespace InkRecognition;

public readonly record struct LegacySourcePrimitiveScope(bool BoundaryAlignedSingleNormalizedStroke,
    bool EndpointFieldsAreZero, int ProbabilityStrokeIndex, float? SourceStrokeProbability);

/// <summary>
/// Ordinary167210 secondary108 end-key writer and218 virtualA0 lookup. Native
/// endpoint field3 addresses raw point ownership, not a guessed glyph category.
/// Raw counts are explicit because trailing discarded points are not recoverable
/// from the retained start-key table alone.
/// </summary>
public sealed class LegacyOrdinarySourceEndpointMetadata
{
    private readonly LegacyOrdinaryCursiveCutSource source;
    public LegacySourcePointChronology EndKeys { get; }

    public LegacyOrdinarySourceEndpointMetadata(LegacyOrdinaryCursiveCutSource source,
        ReadOnlySpan<int> rawPhysicalPointCounts, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source); cancellationToken.ThrowIfCancellationRequested();
        if (rawPhysicalPointCounts.Length != source.Source.Strokes.Count)
            throw new ArgumentException("One original raw point count required per physical source stroke.");
        this.source = source;
        var counts = source.Source.Strokes.Select(s => s.Points.Count).ToArray();
        var keys = new List<LegacySourcePointKey>(); int total = 0;
        for (int stroke = 0; stroke < counts.Length; stroke++)
        {
            int rawCount = rawPhysicalPointCounts[stroke];
            if (rawCount is < 1 or > 4096 || total > 4096 - rawCount)
                throw new ArgumentException("Expected1..4096 original raw points.");
            total += rawCount;
            var ordinals = source.Source.OriginalPointIndices[stroke].Span;
            if (ordinals.Length != counts[stroke] || ordinals.IsEmpty || ordinals[0] != 0 || ordinals[^1] >= rawCount)
                throw new ArgumentException("Raw point counts disagree with ordinary retained source metadata.");
            for (int point = 0; point < ordinals.Length; point++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int endOrdinal = (point + 1 < ordinals.Length ? ordinals[point + 1] : rawCount) - 1;
                var start = source.Source.Chronology.Resolve(stroke, point).Key;
                keys.Add(new(start.First, start.Second, endOrdinal));
            }
        }
        EndKeys = LegacySourcePointChronology.CreatePrepared(counts, keys.ToArray(),
            Enumerable.Range(0, keys.Count).ToArray()).WithInsertedPreparedInk(source.Cuts.PreparedInk, cancellationToken);
    }

    /// <summary>
    /// Restore1bb020's source-boundary and normalized-count predicate and its
    /// first-physical probability lookup. Source range order is supplied by the
    /// existing normalization producer. Multiple ranges are not relabelled as
    /// one physical stroke: the result states only the exact native predicate.
    /// Other cache scope, mask and controller flags remain separate inputs.
    /// </summary>
    public LegacySourcePrimitiveScope Evaluate(LegacyNormalizedPreparedGlyph glyph,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(glyph); cancellationToken.ThrowIfCancellationRequested();
        if (!ReferenceEquals(glyph.PreparedInk, source.Cuts.PreparedInk) || glyph.Ranges.IsEmpty)
            throw new ArgumentException("Glyph must address this ordinary source's prepared ranges.");
        return EvaluateOrderedRanges(glyph.Ranges.Span, glyph.Strokes.Count, cancellationToken);
    }

    /// <summary>Replace only the three checked1bb020 source-primitive fields.</summary>
    public LegacyOrdinaryAcousticCacheContext BindContext(LegacyNormalizedPreparedGlyph glyph,
        LegacyOrdinaryAcousticCacheContext template, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(template);
        var scope = Evaluate(glyph, cancellationToken);
        return template with
        {
            IsWholePhysicalStroke = scope.BoundaryAlignedSingleNormalizedStroke,
            WholeStrokeEndpointCategoriesZero = scope.EndpointFieldsAreZero,
            SourceStrokeProbability = scope.SourceStrokeProbability
        };
    }

    /// <summary>
    /// Data-only predicate for supplied native ordered ranges and resolved
    /// post-normalization stroke count. It does not infer pen-up reduction.
    /// </summary>
    public LegacySourcePrimitiveScope EvaluateOrderedRanges(ReadOnlySpan<LegacyInkRange> ranges,
        int normalizedStrokeCount, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (ranges.Length is < 1 or > 15 || normalizedStrokeCount < 1 || normalizedStrokeCount > ranges.Length)
            throw new ArgumentException("Bounded ordered ranges and resolved normalized stroke count required.");
        var counts = source.Cuts.PreparedInk.StrokePointCounts.Span;
        foreach (var range in ranges)
            if ((uint)range.StrokeIndex >= (uint)counts.Length || range.StartPoint < 0 ||
                range.EndPointExclusive <= range.StartPoint || range.EndPointExclusive > counts[range.StrokeIndex])
                throw new ArgumentException("Ordered range lies outside this source's prepared physical ink.");
        var first = ranges[0]; var last = ranges[^1];
        bool boundaryAligned = normalizedStrokeCount == 1 && first.StartPoint == 0 &&
            last.EndPointExclusive == source.Cuts.PreparedInk.StrokePointCounts.Span[last.StrokeIndex];
        if (!boundaryAligned) return new(false, false, -1, null);
        bool endpointsZero = source.Chronology.Resolve(first.StrokeIndex, 0).Key.Third == 0 &&
            EndKeys.Resolve(last.StrokeIndex, last.EndPointExclusive - 1).Key.Third == 0;
        return new(true, endpointsZero, first.StrokeIndex, source.GetSourceStrokeProbability(first.StrokeIndex));
    }
}
