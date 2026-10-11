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

public sealed record LegacyNativeSeedWordRecognitionResult(LegacyOrdinaryCursiveSeedResult Seeds,
    LegacyCursiveWordRecognitionResult Recognition);

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
    private readonly LegacyAcousticLexicalMapping mapping;

    public LegacyRussianCursiveWordRecognizer(LegacyRussianModelBundle model, int? relatedFormMode = null)
    {
        ArgumentNullException.ThrowIfNull(model); this.model = model;
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
    /// Ordinary resource-configured ingress, per-physical-stroke enclosure and
    /// cuts, with raw chronology preserved through deduplication/insertion.
    /// SourceStrokeProbability in options is unused on this endpoint: cut and
    /// final-cache consumers receive the actual vector, never a scalar average.
    /// Source-order seed/context frame and bounded beam remain original policy.
    /// Scope flags remain explicit; whole scope is supported only for a single
    /// complete physical range. This is not SDK-equivalent automatic recognition.
    /// </summary>
    public LegacyCursiveWordRecognitionResult RecognizeOrdinarySourceWithFinalCache(
        IReadOnlyList<InkStroke> rawSource, LegacyOrdinarySourceProfile sourceProfile,
        LegacyCursiveWordSearchOptions options, LegacyCursiveFinalCacheGroupScorer finalCacheScorer,
        Func<LegacyCursiveCandidateGroup, LegacyPreparedGlyphClassification, LegacyOrdinaryAcousticCacheContext> sourceScope,
        bool useEndCompositeMap, bool zeroFirstId, bool useNativeShortlist,
        ReadOnlySpan<uint> history = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options); ArgumentNullException.ThrowIfNull(finalCacheScorer);
        ArgumentNullException.ThrowIfNull(sourceScope);
        var prepared = LegacyOrdinaryCursiveCutSource.Generate(rawSource, sourceProfile,
            options.SourceBaseline, options.SourceHeight, cancellationToken: cancellationToken);
        return RecognizePreparedDiagnostic(prepared.Source.Strokes, options, history, (group, glyph) =>
            finalCacheScorer.Score(glyph, prepared.BindCacheProbability(sourceScope(group, glyph), glyph.Geometry),
                options.SourceHeight, 0, useEndCompositeMap, zeroFirstId, useNativeShortlist, cancellationToken).Choices,
            null, cancellationToken, prepared);
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
        => RecognizePreparedDiagnostic(source, options, history, costAdapter, null, cancellationToken);

    /// <summary>
    /// Connect exact native19f850 ordinary LTR seeds to the restored ordinary
    /// lattice, neural/context, native final-cache and lexical stages. Explicit
    /// source70/F0 metadata is checked; deferred skip arcs are refused. Whole-line
    /// context frame and bounded dictionary beam remain original orchestration.
    /// This diagnostic endpoint does not produce acceptance confidence.
    /// </summary>
    public LegacyNativeSeedWordRecognitionResult RecognizeNondeferredNativeSeedsWithFinalCache(
        IReadOnlyList<InkStroke> source, LegacyCursiveWordSearchOptions options,
        LegacyOrdinaryCursiveSeedProfile seedProfile, ReadOnlySpan<byte> sourceDirectionFlags,
        ReadOnlySpan<int> physicalBoundaryIds, LegacyCursiveFinalCacheGroupScorer finalCacheScorer,
        Func<LegacyCursiveCandidateGroup, LegacyPreparedGlyphClassification, LegacyOrdinaryAcousticCacheContext> sourceScope,
        bool useEndCompositeMap, bool zeroFirstId, bool useNativeShortlist,
        ReadOnlySpan<uint> history = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options); ArgumentNullException.ThrowIfNull(finalCacheScorer);
        ArgumentNullException.ThrowIfNull(sourceScope);
        var directions = sourceDirectionFlags.ToArray(); var boundaries = physicalBoundaryIds.ToArray();
        LegacyOrdinaryCursiveSeedResult? seeds = null;
        LegacyCursiveCandidateLattice MakeLattice(LegacyCursiveCutResult cuts)
        {
            seeds = LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts, options.SourceHeight,
                seedProfile, directions, boundaries, cancellationToken);
            return seeds.CreateNondeferredOrdinaryChain(model.CursiveSpanProfile.MaximumAtoms, cancellationToken);
        }
        var result = RecognizePreparedDiagnostic(source, options, history, (group, glyph) =>
            finalCacheScorer.Score(glyph, sourceScope(group, glyph), options.SourceHeight, 0,
                useEndCompositeMap, zeroFirstId, useNativeShortlist, cancellationToken).Choices,
            MakeLattice, cancellationToken);
        return new(seeds ?? throw new InvalidOperationException("Native source seeds were not produced."), result);
    }

    private LegacyCursiveWordRecognitionResult RecognizePreparedDiagnostic(IReadOnlyList<InkStroke> source,
        LegacyCursiveWordSearchOptions options, ReadOnlySpan<uint> history,
        Func<LegacyCursiveCandidateGroup, LegacyPreparedGlyphClassification,
            ReadOnlyMemory<LegacyIntegerLexicalCandidate>>? costAdapter,
        Func<LegacyCursiveCutResult, LegacyCursiveCandidateLattice>? latticeFactory,
        CancellationToken cancellationToken, LegacyOrdinaryCursiveCutSource? ordinarySource = null)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();
        if (!float.IsFinite(options.SourceBaseline) || !float.IsFinite(options.SourceHeight) || options.SourceHeight <= 0 ||
            (ordinarySource is null && (!float.IsFinite(options.SourceStrokeProbability) || options.SourceStrokeProbability is < 0 or > 1)) || !float.IsFinite(options.LanguageScale) || options.LanguageScale <= 0 ||
            !float.IsFinite(options.CostMultiplier) || options.CostMultiplier <= 0 ||
            !float.IsFinite(options.ContextLeftGapOffset) || !float.IsFinite(options.ContextLeftVariance) || options.ContextLeftVariance <= 0 ||
            !float.IsFinite(options.ContextRightVariance) || options.ContextRightVariance <= 0 ||
            options.TargetHeight <= 0 || options.WordCategory is < 0 or > 12 || history.Length > 1 ||
            options.BeamWidth is < 1 or > 256 || options.MaximumScoredGroups is < 1 or > 32768 ||
            options.MaximumExpandedChoices is < 1 or > 100_000_000 || options.TotalCostLimit > LegacyWordCandidateScorer.ImpossibleCost)
            throw new ArgumentException("Invalid explicit cursive line/search policy.", nameof(options));
        var historySnapshot = history.ToArray();
        var cuts = ordinarySource?.Cuts ?? LegacyCursiveCutProducer.GenerateRussianVersion106(source, options.SourceBaseline,
            options.SourceHeight, options.SourceStrokeProbability, cancellationToken: cancellationToken);
        int[] rawCounts = source.Select(s => s.Points.Count).ToArray();
        var chronology = ordinarySource?.Chronology ?? LegacySourcePointChronology.CreateOrdinary(rawCounts).WithInsertedPreparedInk(cuts.PreparedInk, cancellationToken);
        // Ordinary physical boundary service records are initialized to -1.
        // Source-order atoms are an explicit original seed policy; the large
        // native line reorder/skip/special-token controller is not inferred.
        var atoms = cuts.Atoms.ToArray(); var boundaryIds = Enumerable.Repeat(-1, atoms.Length).ToArray();
        var lattice = latticeFactory is null ? LegacyCursiveCandidateLattice.GenerateOrdinaryChain(atoms, boundaryIds,
            model.CursiveSpanProfile.MaximumAtoms, cancellationToken) : latticeFactory(cuts);
        var evaluator = new LegacyPreparedWordGroupEvaluator(model, source, cuts.PreparedInk, chronology,
            options, cancellationToken);
        var wordScorer = new LegacyWordCandidateScorer(model.CreateResourceLanguageScorer(options.LanguageScale));
        var finalScores = new Dictionary<int, (int WordId, LegacyWordCandidateScore Score)>();
        int classified = 0;
        ReadOnlyMemory<LegacyIntegerLexicalCandidate> ScoreGroup(LegacyCursiveCandidateGroup group)
        {
            if (++classified > options.MaximumScoredGroups)
                throw new InvalidOperationException("Cursive recognition exceeded its explicit neural-group budget.");
            var classification = evaluator.Classify(group.Ranges.Span, cancellationToken);
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
        return new(cuts, lattice, Array.AsReadOnly(words), evaluator.ContextWindowCount, classified, search.ExpandedChoices);
    }
}
