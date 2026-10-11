namespace InkRecognition;

public sealed record LegacyCursiveFinalCacheGroupScores(LegacyOrdinaryAcousticClassCacheResult Cache,
    ReadOnlyMemory<int> CompositeCategoryCosts, ReadOnlyMemory<LegacyIntegerLexicalCandidate> Choices);

/// <summary>
/// Resource-backed bridge from prepared glyph scores to final ordinary class
/// cache, sampler297 group geometry, native acoustic candidate score and lexical
/// labels. All upstream cache scope/flags, symbol masks, pairs, category-map
/// selector and shortlist selection remain caller policy, not inferred defaults.
/// </summary>
public sealed class LegacyCursiveFinalCacheGroupScorer
{
    private readonly LegacyOrdinaryAcousticClassCache cache;
    private readonly LegacyPreparedAcousticCandidateScorer scorer;
    private readonly LegacyCursiveCategoryMapper categories;
    private readonly LegacyCursiveSeparationProfile separation;
    private readonly LegacyCursiveSeparationTable table;
    private readonly byte[] enabled;
    private readonly LegacyAcousticCachePair[] pairs;
    private readonly ushort[][] lexicalLabels;

    public LegacyCursiveFinalCacheGroupScorer(LegacyOrdinaryAcousticClassCache cache,
        LegacyPreparedAcousticCandidateScorer scorer, LegacyCursiveCategoryMapper categories,
        LegacyCursiveSeparationProfile separation, LegacyCursiveSeparationTable table,
        LegacyInkAlphabet acousticAlphabet, LegacyInkAlphabet lexicalAlphabet,
        ReadOnlySpan<byte> enabledSymbols, ReadOnlySpan<LegacyAcousticCachePair> nativePairs,
        int? relatedFormMode = null)
    {
        ArgumentNullException.ThrowIfNull(cache); ArgumentNullException.ThrowIfNull(scorer);
        ArgumentNullException.ThrowIfNull(categories); ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(acousticAlphabet); ArgumentNullException.ThrowIfNull(lexicalAlphabet);
        if (cache.CacheWidth != acousticAlphabet.LabelCount + 3 || enabledSymbols.Length != acousticAlphabet.LabelCount ||
            acousticAlphabet.LabelCount > categories.SymbolEntryCount || relatedFormMode is < 0 or > 2 || nativePairs.Length > 4096)
            throw new ArgumentException("Final cache, category map and symbol mask must address the same bounded alphabet.");
        foreach (byte flag in enabledSymbols) if (flag > 1) throw new ArgumentException("Symbol mask requires0/1.");
        foreach (var pair in nativePairs)
            if (pair.First >= acousticAlphabet.LabelCount || pair.Second >= acousticAlphabet.LabelCount)
                throw new ArgumentException("Native pair is outside the acoustic alphabet.");
        if (!float.IsFinite(separation.GapOffset) || !float.IsFinite(separation.Slope) || separation.Slope < 0 ||
            separation.MaximumCost is < 0 or > LegacyCursiveGroupSeparationCost.ImpossibleCost)
            throw new ArgumentException("Invalid explicit separation profile.");
        this.cache = cache; this.scorer = scorer; this.categories = categories;
        this.separation = separation; this.table = table; enabled = enabledSymbols.ToArray(); pairs = nativePairs.ToArray();
        lexicalLabels = new ushort[acousticAlphabet.LabelCount][];
        for (ushort source = 0; source < lexicalLabels.Length; source++)
        {
            var labels = new List<ushort>(4);
            void Add(ushort symbol)
            {
                if (symbol != ushort.MaxValue && acousticAlphabet.TryGetText(symbol, out string text) &&
                    lexicalAlphabet.TryGetLabel(text, out ushort label) && !labels.Contains(label)) labels.Add(label);
            }
            Add(source);
            if (relatedFormMode is int mode && acousticAlphabet.TryGetRelatedForms(source, mode, out var forms))
            { Add(forms.First); Add(forms.Second); Add(forms.Third); }
            lexicalLabels[source] = labels.ToArray();
        }
    }

    /// <summary>
    /// Score one ordinary group with zero prior-path cost. This produces
    /// prefix-independent edge costs for an original bounded beam; native
    /// prefix/category penalties must be handled separately, not folded into
    /// this adapter with guessed flags. Optional related forms retain the
    /// original minimum-alias mapping policy, not native case scoring.
    /// </summary>
    public LegacyCursiveFinalCacheGroupScores Score(LegacyPreparedGlyphClassification glyph,
        LegacyOrdinaryAcousticCacheContext context, float sourceLineHeight, int candidateKind,
        bool useEndCompositeMap, bool zeroFirstId, bool useNativeShortlist,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(glyph); ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();
        var final = cache.Build(glyph.IntegerCosts.Span, glyph.Geometry.Strokes, enabled, context, pairs, cancellationToken);
        int geometryCost = LegacyCursiveGroupSeparationCost.Evaluate(glyph.Geometry.PreparedInk, glyph.Geometry.Ranges.Span,
            sourceLineHeight, separation.GapOffset, separation.Slope, table.Values.Span, separation.MaximumCost,
            cancellationToken: cancellationToken);
        var compositeCosts = Enumerable.Repeat(geometryCost, categories.CompositeCategoryCount).ToArray();
        var shortlisted = useNativeShortlist ? final.Candidates.ToArray().Select(c => c.Symbol).ToHashSet() : null;
        var choices = new Dictionary<ushort, int>();
        for (ushort symbol = 0; symbol < lexicalLabels.Length; symbol++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (lexicalLabels[symbol].Length == 0 || shortlisted is not null && !shortlisted.Contains(symbol)) continue;
            int category = categories.GetSymbolCompositeCategory(symbol, useEndCompositeMap, zeroFirstId);
            if (category < 0) continue;
            var score = scorer.Evaluate(final.Costs.Span, symbol, compositeCosts, category, candidateKind,
                0, LegacyWordCandidateScorer.ImpossibleCost, cancellationToken);
            if (!score.Accepted) continue;
            foreach (ushort label in lexicalLabels[symbol])
                if (!choices.TryGetValue(label, out int previous) || score.TotalCost < previous) choices[label] = score.TotalCost;
        }
        return new(final, compositeCosts,
            choices.OrderBy(c => c.Key).Select(c => new LegacyIntegerLexicalCandidate(c.Key, c.Value)).ToArray());
    }
}
