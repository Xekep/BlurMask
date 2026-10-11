namespace InkRecognition;

/// <summary>
/// Ordinary167210 ingress and source218 virtual60 connected to Russian191950
/// cuts. Point identities address the original raw input after deduplication and
/// insertion. Source lifecycle/grouping, opaque input and resampling are separate.
/// </summary>
public sealed class LegacyOrdinaryCursiveCutSource
{
    public LegacyOrdinarySourceInk Source { get; }
    public LegacyCursiveCutResult Cuts { get; }
    public LegacySourcePointChronology Chronology { get; }

    private LegacyOrdinaryCursiveCutSource(LegacyOrdinarySourceInk source,
        LegacyCursiveCutResult cuts, LegacySourcePointChronology chronology)
    { Source = source; Cuts = cuts; Chronology = chronology; }

    public static LegacyOrdinaryCursiveCutSource Generate(IReadOnlyList<InkStroke> rawSource,
        LegacyOrdinarySourceProfile sourceProfile, float? baseline, float lineHeight,
        bool wholeStrokeOnly = false, int? closedStrokeCategoryLimit = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var source = LegacyOrdinarySourceInk.Prepare(rawSource, sourceProfile, cancellationToken);
        var cuts = LegacyCursiveCutProducer.GenerateRussianVersion106(source.Strokes, baseline,
            lineHeight, source.SourceStrokeProbabilities.ToArray(), wholeStrokeOnly,
            closedStrokeCategoryLimit, cancellationToken);
        var chronology = source.Chronology.WithInsertedPreparedInk(cuts.PreparedInk, cancellationToken);
        return new(source, cuts, chronology);
    }

    /// <summary>Source218 virtual60 reads one physical record+C; no aggregate.</summary>
    public float GetSourceStrokeProbability(int physicalStrokeIndex)
    {
        if ((uint)physicalStrokeIndex >= (uint)Source.Strokes.Count)
            throw new ArgumentOutOfRangeException(nameof(physicalStrokeIndex));
        return Source.SourceStrokeProbabilities.Span[physicalStrokeIndex];
    }

    /// <summary>
    /// Supply the same source record to the1bb020 closed primitive. Other scope
    /// flags remain explicit caller policy. Supported whole scope is one complete
    /// prepared physical range, not a merged group or a partial cut. A foreign
    /// geometry snapshot is refused. Inactive/partial scope has no probability.
    /// </summary>
    public LegacyOrdinaryAcousticCacheContext BindCacheProbability(
        LegacyOrdinaryAcousticCacheContext context, LegacyNormalizedPreparedGlyph glyph)
    {
        ArgumentNullException.ThrowIfNull(context); ArgumentNullException.ThrowIfNull(glyph);
        if (!ReferenceEquals(glyph.PreparedInk, Cuts.PreparedInk))
            throw new ArgumentException("Glyph must address this ordinary source's prepared cuts.", nameof(glyph));
        if (!context.IsWholePhysicalStroke) return context with { SourceStrokeProbability = null };
        if (glyph.Ranges.Length != 1 || glyph.Strokes.Count != 1)
            throw new NotSupportedException("Whole source probability requires one complete physical range.");
        var range = glyph.Ranges.Span[0];
        if (range.StartPoint != 0 || range.EndPointExclusive != Cuts.PreparedInk.StrokePointCounts.Span[range.StrokeIndex])
            throw new ArgumentException("Partial prepared range cannot claim a whole physical source stroke.", nameof(context));
        return context with { SourceStrokeProbability = GetSourceStrokeProbability(range.StrokeIndex) };
    }
}
