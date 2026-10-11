namespace InkRecognition;

/// <summary>Caller-scored consecutive atom group, with resolved seven-slot acoustic weights.</summary>
public sealed record LegacyPreparedScoredGlyph(int StartAtom, int AtomCount,
    LegacyDirectionalGlyphScores Scores, ReadOnlyMemory<float> Weights, float Multiplier);

/// <summary>Retained prepared-ink ownership of one scored glyph.</summary>
public sealed record LegacyPreparedGlyphOwnership(int StartAtom, int AtomCount,
    ReadOnlyMemory<LegacyInkRange> Ranges);

public sealed record LegacyPreparedSequenceWordResult(
    IReadOnlyList<LegacyPreparedGlyphOwnership> Glyphs, IReadOnlyList<LegacyIntegerWordMatch> Matches);

/// <summary>
/// Checked endpoint connecting supplied sampler297 atom groups and glyph
/// probability scores to integer acoustic costs and ordinary terminal word
/// scoring. The fixed sequence and bounded prefix beam are caller/original
/// policies, not native cut search or automatic cursive recognition.
/// </summary>
public sealed class LegacyPreparedSequenceWordSearch
{
    private readonly LegacyAcousticLexicalMapping mapping;
    private readonly LegacyIntegerWordDecoder decoder;

    public LegacyPreparedSequenceWordSearch(LegacyAcousticLexicalMapping mapping,
        LegacyIntegerWordDecoder decoder)
    {
        ArgumentNullException.ThrowIfNull(mapping); ArgumentNullException.ThrowIfNull(decoder);
        this.mapping = mapping; this.decoder = decoder;
    }

    /// <summary>
    /// Atom markers are rebuilt against this prepared ink, so inserted-point
    /// ownership cannot silently refer to raw point counts. Groups must partition
    /// the supplied atom sequence. Scores must already have been produced from
    /// the corresponding normalized geometry: this method neither converts
    /// float coordinates to short nor infers a baseline, category or weights.
    /// Whole-word lexical category and all cut/prior/correction costs are explicit.
    /// </summary>
    public LegacyPreparedSequenceWordResult Search(LegacyCursivePreparedInk preparedInk,
        ReadOnlySpan<LegacyCursiveCutMarker> markers, IReadOnlyList<LegacyPreparedScoredGlyph> glyphs,
        ReadOnlySpan<uint> history, int wordCategory, bool clearFirstCategoryTag,
        bool suppressCategoryCost, bool includeBoundaryCost, int additionalPathCost,
        int totalCostLimit, bool heapIsFull, int beamWidth,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preparedInk); ArgumentNullException.ThrowIfNull(glyphs);
        cancellationToken.ThrowIfCancellationRequested();
        if (glyphs.Count is < 1 or > 128) throw new ArgumentException("Expected 1..128 supplied glyph groups.", nameof(glyphs));
        if ((uint)wordCategory > 12) throw new ArgumentOutOfRangeException(nameof(wordCategory));
        var atoms = LegacyCursiveInkAtoms.Build(preparedInk.StrokePointCounts.Span, markers, cancellationToken);
        var ownership = new LegacyPreparedGlyphOwnership[glyphs.Count];
        var choices = new ReadOnlyMemory<LegacyIntegerLexicalCandidate>[glyphs.Count];
        int next = 0;
        for (int i = 0; i < glyphs.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var glyph = glyphs[i] ?? throw new ArgumentException("Null supplied glyph.", nameof(glyphs));
            if (glyph.StartAtom != next || glyph.AtomCount < 1 || glyph.AtomCount > atoms.Length - next)
                throw new ArgumentException("Glyph groups must be a consecutive partition of prepared atoms.", nameof(glyphs));
            ArgumentNullException.ThrowIfNull(glyph.Scores);
            if (glyph.Scores.Confidence.Length != mapping.OutputWidth)
                throw new ArgumentException("Glyph score width differs from the supplied alphabet mapping.", nameof(glyphs));
            var ranges = new LegacyInkRange[glyph.AtomCount];
            for (int j = 0; j < ranges.Length; j++) ranges[j] = atoms[next+j].Range;
            ownership[i] = new(next, ranges.Length, LegacyCursiveInkAtoms.Compose(ranges, cancellationToken));
            choices[i] = mapping.MapIntegerCosts(glyph.Scores.GetIntegerCosts(glyph.Weights.Span,
                glyph.Multiplier, cancellationToken: cancellationToken), cancellationToken);
            next += glyph.AtomCount;
        }
        if (next != atoms.Length) throw new ArgumentException("Supplied groups omit prepared atoms.", nameof(glyphs));
        var matches = decoder.Decode(choices, history, wordCategory, clearFirstCategoryTag,
            suppressCategoryCost, includeBoundaryCost, additionalPathCost, totalCostLimit,
            heapIsFull, beamWidth, cancellationToken);
        return new(Array.AsReadOnly(ownership), matches);
    }
}
