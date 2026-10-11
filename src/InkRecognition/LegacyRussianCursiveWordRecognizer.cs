using System.Text;

namespace InkRecognition;

/// <summary>Explicit line frame and original bounded search/context policy.</summary>
public sealed record LegacyCursiveWordSearchOptions(float SourceBaseline, float SourceHeight,
    float SourceStrokeProbability, float LanguageScale,
    int WordCategory = 0, bool ClearFirstCategoryTag = false, bool SuppressCategoryCost = true,
    bool IncludeBoundaryCost = false, int AdditionalPathCost = 0,
    int TotalCostLimit = LegacyWordCandidateScorer.ImpossibleCost, bool HeapIsFull = false,
    int BeamWidth = 64, int MaximumScoredGroups = 512, long MaximumExpandedChoices = 2_000_000,
    short TargetHeight = 100, short StartBoundary = 1024, short EndBoundary = 1024,
    bool IncludeBaselineFamilies = true, float CostMultiplier = 1,
    float ContextLeftGapOffset = 0, float ContextLeftVariance = 1, float ContextRightVariance = 1,
    bool UsePreparedBoundarySigns = true, bool SuppressStartBoundary = false, bool SuppressEndBoundary = false);

public sealed record LegacyCursiveRecognizedWord(string Text, LegacyVariableWordMatch Match,
    int WordId, LegacyWordCandidateScore Score);

public sealed record LegacyCursiveWordRecognitionResult(LegacyCursiveCutResult Cuts,
    LegacyCursiveCandidateLattice Lattice, IReadOnlyList<LegacyCursiveRecognizedWord> Words,
    int ContextWindowCount, int ClassifiedGroups, long ExpandedChoices);

/// <summary>
/// End-to-end diagnostic word recognition with actual Russian resources: cuts,
/// prepared ownership, variable groups, chronological glyph ranges, four neural
/// families, context and ordinary whole-word lexical scoring. Source-order
/// seeding, whole-line context frame and bounded dictionary beam are original
/// orchestration. Raw180 costs omit the native195-entry final-cache pipeline.
/// This class does not claim SDK-equivalent cursive recognition or auto line fit.
/// </summary>
public sealed class LegacyRussianCursiveWordRecognizer
{
    private readonly LegacyRussianModelBundle model;
    private readonly LegacyPreparedGlyphClassifier classifier;
    private readonly LegacyContextWindowScorer contextScorer;
    private readonly LegacyAcousticLexicalMapping mapping;

    public LegacyRussianCursiveWordRecognizer(LegacyRussianModelBundle model, int? relatedFormMode = null)
    {
        ArgumentNullException.ThrowIfNull(model); this.model = model;
        classifier = model.CreatePreparedGlyphClassifier(); contextScorer = model.CreateContextWindowScorer();
        mapping = new(model.AcousticAlphabet, model.LexicalAlphabet, model.CreateGlyphClassifier().OutputWidth, relatedFormMode);
    }

    /// <summary>
    /// Complete final-cache path using a separately configured native group
    /// scorer and explicit upstream source-scope resolver. Source-order seed,
    /// normalized context frame and dictionary beam remain original policies.
    /// Ordinary candidate kind is0; optional native confidence-prune/additional
    /// recognizer hooks and prefix-dependent corrections are not synthesized.
    /// </summary>
    public LegacyCursiveWordRecognitionResult RecognizeSourceOrderWithFinalCache(IReadOnlyList<InkStroke> source,
        LegacyCursiveWordSearchOptions options, LegacyCursiveFinalCacheGroupScorer finalCacheScorer,
        Func<LegacyCursiveCandidateGroup, LegacyPreparedGlyphClassification, LegacyOrdinaryAcousticCacheContext> sourceScope,
        bool useEndCompositeMap, bool zeroFirstId, bool useNativeShortlist,
        ReadOnlySpan<uint> history = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(finalCacheScorer); ArgumentNullException.ThrowIfNull(sourceScope);
        ArgumentNullException.ThrowIfNull(options);
        return RecognizeSourceOrderDiagnostic(source, options, history, (group, glyph) =>
            finalCacheScorer.Score(glyph, sourceScope(group, glyph), options.SourceHeight, 0,
                useEndCompositeMap, zeroFirstId, useNativeShortlist, cancellationToken).Choices,
            cancellationToken);
    }

    /// <summary>
    /// The supplied source is one ordinary word in source stroke order. Original
    /// timestamps are not invented: the ordinary unsampled key writer uses
    /// (0,stroke,point). Inserted-cut points retain that raw identity. Optional
    /// costAdapter can connect a separately restored final-cache producer; it
    /// must return prefix-independent lexical group costs. Its presence does
    /// not change this endpoint's original source-order/beam policy.
    /// </summary>
    public LegacyCursiveWordRecognitionResult RecognizeSourceOrderDiagnostic(IReadOnlyList<InkStroke> source,
        LegacyCursiveWordSearchOptions options, ReadOnlySpan<uint> history = default,
        Func<LegacyCursiveCandidateGroup, LegacyPreparedGlyphClassification,
            ReadOnlyMemory<LegacyIntegerLexicalCandidate>>? costAdapter = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();
        if (!float.IsFinite(options.SourceBaseline) || !float.IsFinite(options.SourceHeight) || options.SourceHeight <= 0 ||
            !float.IsFinite(options.SourceStrokeProbability) || options.SourceStrokeProbability is < 0 or > 1 || !float.IsFinite(options.LanguageScale) || options.LanguageScale <= 0 ||
            !float.IsFinite(options.CostMultiplier) || options.CostMultiplier <= 0 ||
            !float.IsFinite(options.ContextLeftGapOffset) || !float.IsFinite(options.ContextLeftVariance) || options.ContextLeftVariance <= 0 ||
            !float.IsFinite(options.ContextRightVariance) || options.ContextRightVariance <= 0 ||
            options.TargetHeight <= 0 || options.WordCategory is < 0 or > 12 || history.Length > 1 ||
            options.BeamWidth is < 1 or > 256 || options.MaximumScoredGroups is < 1 or > 32768 ||
            options.MaximumExpandedChoices is < 1 or > 100_000_000 || options.TotalCostLimit > LegacyWordCandidateScorer.ImpossibleCost)
            throw new ArgumentException("Invalid explicit cursive line/search policy.", nameof(options));
        var historySnapshot = history.ToArray();
        var cuts = LegacyCursiveCutProducer.GenerateRussianVersion106(source, options.SourceBaseline,
            options.SourceHeight, options.SourceStrokeProbability, cancellationToken: cancellationToken);
        int[] rawCounts = source.Select(s => s.Points.Count).ToArray();
        var chronology = LegacySourcePointChronology.CreateOrdinary(rawCounts).WithInsertedPreparedInk(cuts.PreparedInk, cancellationToken);
        // Ordinary physical boundary service records are initialized to -1.
        // Source-order atoms are an explicit original seed policy; the large
        // native line reorder/skip/special-token controller is not inferred.
        var atoms = cuts.Atoms.ToArray(); var boundaryIds = Enumerable.Repeat(-1, atoms.Length).ToArray();
        var lattice = LegacyCursiveCandidateLattice.GenerateOrdinaryChain(atoms, boundaryIds,
            model.CursiveSpanProfile.MaximumAtoms, cancellationToken);
        var rawPrepared = LegacyCursivePreparedInk.Create(source, cancellationToken);
        float lineMinX = source.SelectMany(s => s.Points).Min(p => p.X);
        var normalizedLine = new LegacyInkStroke[source.Count];
        for (int stroke = 0; stroke < source.Count; stroke++)
            normalizedLine[stroke] = LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(rawPrepared,
                [new LegacyInkRange(stroke, 0, rawCounts[stroke])], lineMinX, options.SourceBaseline,
                options.SourceHeight, options.TargetHeight, 1, cancellationToken).Strokes[0];
        var contextGroups = contextScorer.ScoreLine(normalizedLine, 0, options.TargetHeight, 0,
            new int[source.Count], cancellationToken: cancellationToken);
        var wordScorer = new LegacyWordCandidateScorer(model.CreateResourceLanguageScorer(options.LanguageScale));
        var finalScores = new Dictionary<int, (int WordId, LegacyWordCandidateScore Score)>();
        int classified = 0; byte[] enabled = options.IncludeBaselineFamilies ? [1,1,1,1,0,0,0] : [1,1,0,0,0,0,0];
        ReadOnlyMemory<LegacyIntegerLexicalCandidate> ScoreGroup(LegacyCursiveCandidateGroup group)
        {
            if (++classified > options.MaximumScoredGroups)
                throw new InvalidOperationException("Cursive recognition exceeded its explicit neural-group budget.");
            int[] order = chronology.OrderRangeIndicesForNormalization(group.Ranges.Span, cancellationToken);
            var ranges = order.Select(i => group.Ranges.Span[i]).ToArray();
            var counts = cuts.PreparedInk.StrokePointCounts.Span;
            var starts = new int[counts.Length]; for (int i = 1; i < starts.Length; i++) starts[i] = starts[i-1] + counts[i-1];
            float minimumX = float.MaxValue, sumX = 0; int count = 0;
            foreach (var range in ranges)
                for (int point = range.StartPoint; point < range.EndPointExclusive; point++)
                {
                    var p = cuts.PreparedInk.Points.Span[starts[range.StrokeIndex] + point];
                    minimumX = Math.Min(minimumX, p.X); sumX += p.X; count++;
                }
            var physicalIds = ranges.Select(r => r.StrokeIndex).Distinct().ToArray();
            var observations = LegacyPriorContextSelection.SelectNative(contextGroups, 0, physicalIds,
                model.AcousticWeights.ContextSelectionMode, cancellationToken: cancellationToken);
            // Context window centers and target must use the same line frame.
            float centerX = ((sumX / count - lineMinX) * options.TargetHeight) / options.SourceHeight;
            var context = LegacyFloatInputQuantization.Quantize(LegacyContextWeighting.Combine(observations,
                centerX, options.TargetHeight, options.ContextLeftGapOffset, options.ContextLeftVariance,
                options.ContextRightVariance, cancellationToken), cancellationToken: cancellationToken);
            var signs = options.UsePreparedBoundarySigns ? LegacyCursiveGlyphBoundaryPolicy.Resolve(ranges,
                cuts.PreparedInk.StrokePointCounts.Span, 0, options.SuppressStartBoundary, options.SuppressEndBoundary, cancellationToken) :
                new LegacyCursiveGlyphBoundarySigns(options.StartBoundary, options.EndBoundary);
            var classification = classifier.Classify(cuts.PreparedInk, ranges, minimumX, options.SourceBaseline,
                options.SourceHeight, options.TargetHeight, 10, context, signs.Start, signs.End,
                options.IncludeBaselineFamilies, enabled, options.CostMultiplier, cancellationToken: cancellationToken);
            return costAdapter is null ? mapping.MapIntegerCosts(classification.IntegerCosts.Span, cancellationToken) :
                costAdapter(group, classification);
        }
        int? ScoreTerminal(int state, int acoustic)
        {
            if (!model.WordIndex.TryGetWordId(state, out int wordId)) return null;
            long preLexical = (long)acoustic + options.AdditionalPathCost;
            if (preLexical is < int.MinValue or > int.MaxValue) return null;
            var score = wordScorer.Score(wordId, historySnapshot, options.WordCategory, options.ClearFirstCategoryTag,
                options.SuppressCategoryCost, options.IncludeBoundaryCost, (int)preLexical, options.TotalCostLimit,
                options.HeapIsFull, cancellationToken);
            if (!score.IsAccepted) return null;
            finalScores[state] = (wordId, score); return (int)score.TotalCost;
        }
        var search = LegacyVariableWordSearch.Search(model.Lexicon, lattice, ScoreGroup, options.BeamWidth,
            options.MaximumExpandedChoices, ScoreTerminal, cancellationToken);
        var words = search.Matches.Select(match =>
        {
            var text = new StringBuilder(); foreach (ushort label in match.Labels.Span)
                if (model.LexicalAlphabet.TryGetText(label, out string symbol)) text.Append(symbol);
                else throw new InvalidDataException("Dictionary produced an unmapped text label.");
            var final = finalScores[match.TerminalState];
            return new LegacyCursiveRecognizedWord(text.ToString(), match, final.WordId, final.Score);
        }).ToArray();
        return new(cuts, lattice, Array.AsReadOnly(words), contextGroups.Length, classified, search.ExpandedChoices);
    }
}
