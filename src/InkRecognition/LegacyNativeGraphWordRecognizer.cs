using System.Text;

namespace InkRecognition;

public sealed record LegacyGraphRecognizedWord(string Text, LegacyGraphWordMatch Match,
    int WordId, LegacyWordCandidateScore Score);
public sealed record LegacyNativeGraphRecognitionResult(LegacyOrdinaryCursiveCutSource Source,
    LegacyOrdinaryCursiveSeedResult Seeds, LegacyOrdinaryCursiveCandidateGraph Graph,
    IReadOnlyList<LegacyGraphRecognizedWord> Words, int ClassifiedGroups,
    int ContextWindowCount, long ExpandedChoices);

/// <summary>
/// Resource-backed ordinary source/cut/seed/merged-graph/glyph/cache/lexical bridge.
/// Native single-deferred topology is retained. Dictionary beam, whole-line
/// frame/context and terminal selection are original bounded orchestration;
/// native winner, path corrections and acceptance confidence are not claimed.
/// </summary>
public sealed class LegacyNativeGraphWordRecognizer(LegacyRussianModelBundle model)
{
    private readonly LegacyRussianModelBundle model = model ?? throw new ArgumentNullException(nameof(model));

    public LegacyNativeGraphRecognitionResult Recognize(IReadOnlyList<InkStroke> rawSource,
        LegacyOrdinarySourceProfile sourceProfile, LegacyOrdinaryCursiveSeedProfile seedProfile,
        ReadOnlySpan<byte> sourceDirectionFlags, ReadOnlySpan<int> physicalBoundaryIds,
        LegacyCursiveWordSearchOptions options, LegacyCursiveFinalCacheGroupScorer finalCacheScorer,
        Func<LegacyGraphGlyphPosition, LegacyOrdinaryCursiveCandidateArc,
            LegacyPreparedGlyphClassification, LegacyOrdinaryAcousticCacheContext> sourceScope,
        bool useEndCompositeMap, bool zeroFirstId, bool useNativeShortlist,
        ReadOnlySpan<uint> history = default, int maximumCandidates = 131072,
        CancellationToken cancellationToken = default)
        => RecognizeCore(rawSource, sourceProfile, seedProfile, sourceDirectionFlags, physicalBoundaryIds,
            options, finalCacheScorer, sourceScope, useEndCompositeMap, zeroFirstId, useNativeShortlist,
            history, maximumCandidates, LegacyOrdinaryCursiveSeedGraph.CreateSingleDeferred, cancellationToken);

    /// <summary>
    /// Ordinary noNN/shift1 graph bridge including multiple deferred placements.
    /// The original bounded search and explicit scope/frame policies are the
    /// same as Recognize; native winner/confidence is not supplied.
    /// </summary>
    public LegacyNativeGraphRecognitionResult RecognizeOrdinaryDeferred(IReadOnlyList<InkStroke> rawSource,
        LegacyOrdinarySourceProfile sourceProfile, LegacyOrdinaryCursiveSeedProfile seedProfile,
        ReadOnlySpan<byte> sourceDirectionFlags, ReadOnlySpan<int> physicalBoundaryIds,
        LegacyCursiveWordSearchOptions options, LegacyCursiveFinalCacheGroupScorer finalCacheScorer,
        Func<LegacyGraphGlyphPosition, LegacyOrdinaryCursiveCandidateArc,
            LegacyPreparedGlyphClassification, LegacyOrdinaryAcousticCacheContext> sourceScope,
        bool useEndCompositeMap, bool zeroFirstId, bool useNativeShortlist,
        ReadOnlySpan<uint> history = default, int maximumCandidates = 131072,
        CancellationToken cancellationToken = default)
        => RecognizeCore(rawSource, sourceProfile, seedProfile, sourceDirectionFlags, physicalBoundaryIds,
            options, finalCacheScorer, sourceScope, useEndCompositeMap, zeroFirstId, useNativeShortlist,
            history, maximumCandidates, LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred, cancellationToken);

    private LegacyNativeGraphRecognitionResult RecognizeCore(IReadOnlyList<InkStroke> rawSource,
        LegacyOrdinarySourceProfile sourceProfile, LegacyOrdinaryCursiveSeedProfile seedProfile,
        ReadOnlySpan<byte> sourceDirectionFlags, ReadOnlySpan<int> physicalBoundaryIds,
        LegacyCursiveWordSearchOptions options, LegacyCursiveFinalCacheGroupScorer finalCacheScorer,
        Func<LegacyGraphGlyphPosition, LegacyOrdinaryCursiveCandidateArc,
            LegacyPreparedGlyphClassification, LegacyOrdinaryAcousticCacheContext> sourceScope,
        bool useEndCompositeMap, bool zeroFirstId, bool useNativeShortlist,
        ReadOnlySpan<uint> history, int maximumCandidates,
        Func<LegacyOrdinaryCursiveSeedResult, CancellationToken, LegacyOrdinaryCursiveSeedGraph> graphFactory,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options); ArgumentNullException.ThrowIfNull(finalCacheScorer);
        ArgumentNullException.ThrowIfNull(sourceScope); cancellationToken.ThrowIfCancellationRequested();
        if (!float.IsFinite(options.LanguageScale) || options.LanguageScale <= 0 ||
            options.WordCategory is < 0 or > 12 || history.Length > 1 ||
            options.MaximumScoredGroups is < 1 or > 32768 ||
            options.TotalCostLimit > LegacyWordCandidateScorer.ImpossibleCost)
            throw new ArgumentException("Invalid explicit graph word policy.", nameof(options));
        var source = LegacyOrdinaryCursiveCutSource.Generate(rawSource, sourceProfile,
            options.SourceBaseline, options.SourceHeight, cancellationToken: cancellationToken);
        var seeds = LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(source.Cuts, options.SourceHeight,
            seedProfile, sourceDirectionFlags, physicalBoundaryIds, cancellationToken);
        var seedGraph = graphFactory(seeds, cancellationToken);
        var graph = LegacyOrdinaryCursiveCandidateGraph.Generate(seeds, seedGraph,
            model.CursiveSpanProfile.MaximumAtoms, maximumCandidates, cancellationToken);
        var evaluator = new LegacyPreparedWordGroupEvaluator(model, source, options, cancellationToken);
        var classifications = new Dictionary<int, LegacyPreparedGlyphClassification>();
        var wordScorer = new LegacyWordCandidateScorer(model.CreateResourceLanguageScorer(options.LanguageScale));
        var historySnapshot = history.ToArray();
        ReadOnlyMemory<LegacyIntegerLexicalCandidate> ScoreGlyph(LegacyGraphGlyphPosition position,
            LegacyOrdinaryCursiveCandidateArc arc, LegacyOrdinaryCursiveCandidateInk group)
        {
            if (!classifications.TryGetValue(arc.GroupId, out var glyph))
            {
                if (classifications.Count >= options.MaximumScoredGroups)
                    throw new InvalidOperationException("Graph word recognition exceeded its explicit neural-group budget.");
                glyph = evaluator.Classify(group.Ranges.Span, cancellationToken);
                classifications.Add(arc.GroupId, glyph);
            }
            var context = source.BindCacheProbability(sourceScope(position, arc, glyph), glyph.Geometry);
            return finalCacheScorer.Score(glyph, context, options.SourceHeight, 0,
                useEndCompositeMap, zeroFirstId, useNativeShortlist, cancellationToken).Choices;
        }
        (int Word, LegacyWordCandidateScore Score)? EvaluateTerminal(int state, int acoustic)
        {
            if (!model.WordIndex.TryGetWordId(state, out int word)) return null;
            long preLexical = (long)acoustic + options.AdditionalPathCost;
            if (preLexical is < int.MinValue or > int.MaxValue) return null;
            var score = wordScorer.Score(word, historySnapshot, options.WordCategory,
                options.ClearFirstCategoryTag, options.SuppressCategoryCost, options.IncludeBoundaryCost,
                (int)preLexical, options.TotalCostLimit, options.HeapIsFull, cancellationToken);
            return score.IsAccepted ? (word, score) : null;
        }
        int? ScoreTerminal(int state, int acoustic) => EvaluateTerminal(state, acoustic) is { } value
            ? (int)value.Score.TotalCost : null;
        var search = LegacyGraphWordSearch.Search(model.Lexicon, graph, ScoreGlyph, options.BeamWidth,
            options.MaximumExpandedChoices, ScoreTerminal, cancellationToken);
        var words = search.Matches.Select(match =>
        {
            var text = new StringBuilder();
            foreach (ushort label in match.Labels.Span)
                if (model.LexicalAlphabet.TryGetText(label, out string symbol)) text.Append(symbol);
                else throw new InvalidDataException("Dictionary produced an unmapped text label.");
            // Re-evaluate only the retained alternatives. Do not retain every
            // expanded (terminal,cost) pair or attach a discarded path's score.
            var final = EvaluateTerminal(match.TerminalState, match.AcousticCost)
                ?? throw new InvalidDataException("Retained terminal no longer satisfies the explicit lexical policy.");
            return new LegacyGraphRecognizedWord(text.ToString(), match, final.Word, final.Score);
        }).ToArray();
        return new(source, seeds, graph, Array.AsReadOnly(words), classifications.Count,
            evaluator.ContextWindowCount, search.ExpandedChoices);
    }
}
