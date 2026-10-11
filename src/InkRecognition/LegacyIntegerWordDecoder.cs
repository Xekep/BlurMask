namespace InkRecognition;

public sealed record LegacyIntegerWordMatch(ReadOnlyMemory<ushort> Labels,
    int TerminalState, int WordId, int AcousticCost, LegacyWordCandidateScore Score);

/// <summary>
/// Original bounded dictionary search over caller-prepared integer glyph
/// choices, with restored ordinary word terminal scoring before final pruning.
/// This is not native SDK search or an automatic handwriting segmenter.
/// </summary>
public sealed class LegacyIntegerWordDecoder
{
    private readonly LegacyPackedLexicon _graph;
    private readonly LegacyLexiconWordIndex _index;
    private readonly LegacyWordCandidateScorer _scorer;

    public LegacyIntegerWordDecoder(LegacyPackedLexicon graph,
        LegacyLexiconWordIndex wordIndex, LegacyWordCandidateScorer scorer)
    {
        ArgumentNullException.ThrowIfNull(graph); ArgumentNullException.ThrowIfNull(wordIndex);
        ArgumentNullException.ThrowIfNull(scorer);
        if (!wordIndex.IsForGraph(graph)) throw new ArgumentException("Word index belongs to another graph.", nameof(wordIndex));
        if (wordIndex.WordCount != scorer.WordCount) throw new ArgumentException("Dictionary and language word counts differ.", nameof(scorer));
        _graph = graph; _index = wordIndex; _scorer = scorer;
    }

    /// <summary>
    /// All category/history flags and heap limits are explicit. Additional path
    /// cost includes caller-required prior/cut/correction costs. Prefix pruning
    /// uses acoustic cost only; negative terminal lexical costs are preserved.
    /// The final bounded search width is independent of the supplied heap flag.
    /// </summary>
    public IReadOnlyList<LegacyIntegerWordMatch> Decode(
        IReadOnlyList<ReadOnlyMemory<LegacyIntegerLexicalCandidate>> positions,
        ReadOnlySpan<uint> history, int category, bool clearFirstCategoryTag,
        bool suppressCategoryCost, bool includeBoundaryCost, int additionalPathCost,
        int totalCostLimit, bool heapIsFull, int beamWidth,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(positions); cancellationToken.ThrowIfCancellationRequested();
        if (history.Length > 1) throw new ArgumentException("Order2 supports at most one preceding class.", nameof(history));
        if ((uint)category > 12) throw new ArgumentOutOfRangeException(nameof(category));
        if (totalCostLimit > LegacyWordCandidateScorer.ImpossibleCost) throw new ArgumentOutOfRangeException(nameof(totalCostLimit));
        uint[] retainedHistory = history.ToArray();
        LegacyWordCandidateScore? Score(int state, int acoustic)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!_index.TryGetWordId(state, out int wordId)) throw new InvalidDataException("Expected indexed terminal.");
            long path = (long)acoustic + additionalPathCost;
            if (path is < int.MinValue or > int.MaxValue) return null;
            var score = _scorer.Score(wordId, retainedHistory, category, clearFirstCategoryTag,
                suppressCategoryCost, includeBoundaryCost, (int)path, totalCostLimit, heapIsFull, cancellationToken);
            return score.IsAccepted ? score : null;
        }
        int? ScoreTerminal(int state, int acoustic) => Score(state, acoustic) is { } score ? (int)score.TotalCost : null;
        var matches = LegacyIntegerLexicalDecoder.Decode(_graph, positions, beamWidth, ScoreTerminal, cancellationToken);
        var result = new List<LegacyIntegerWordMatch>(matches.Count);
        foreach (var match in matches)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!_index.TryGetWordId(match.TerminalState, out int wordId) || Score(match.TerminalState, match.AcousticCost) is not { } score)
                throw new InvalidDataException("Terminal scoring changed during pure decoding.");
            result.Add(new(match.Labels, match.TerminalState, wordId, match.AcousticCost, score));
        }
        return result;
    }
}
