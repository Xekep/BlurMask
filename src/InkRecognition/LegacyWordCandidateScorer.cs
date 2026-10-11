namespace InkRecognition;

public readonly record struct LegacyWordCandidateScore(int PreLexicalPathCost,
    long LexicalBudget, LegacyWordLanguageScore Language, long TotalCost,
    bool FitsNativeIntegerRange, bool IsAccepted);

/// <summary>
/// Known-word scoring for the ordinary language-enabled branch of native
/// 180204290. The caller supplies the already accumulated acoustic/segmentation
/// path cost, explicit lexical scorer and current cost limit. This does not
/// reconstruct native beam search, acoustic corrections or the language-bypass
/// branch, and does not choose a recognition mode.
/// </summary>
public sealed class LegacyWordCandidateScorer
{
    public const int ImpossibleCost = 0x4000000;
    private readonly LegacyWordLanguageScorer _language;

    public LegacyWordCandidateScorer(LegacyWordLanguageScorer language)
    {
        ArgumentNullException.ThrowIfNull(language); _language = language;
    }

    /// <summary>
    /// Subtract the existing path cost from the lexical query budget; add the
    /// returned lexical cost back to that path. A full native heap admits a
    /// strictly better candidate; an unfilled heap permits budget equality.
    /// Widened arithmetic rejects out-of-range native integers without wrapping.
    /// The supplied path must already include all caller-required ink/cut costs.
    /// </summary>
    public LegacyWordCandidateScore Score(int wordId, ReadOnlySpan<uint> history,
        int category, bool clearFirstCategoryTag, bool suppressCategoryCost,
        bool includeBoundaryCost, int preLexicalPathCost, int totalCostLimit,
        bool heapIsFull, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (totalCostLimit > ImpossibleCost)
            throw new ArgumentOutOfRangeException(nameof(totalCostLimit));
        var language = _language.Score(wordId, history, category,
            clearFirstCategoryTag, suppressCategoryCost, includeBoundaryCost, cancellationToken);
        long budget = (long)totalCostLimit - preLexicalPathCost;
        long total = (long)preLexicalPathCost + language.LexicalSubtotal;
        bool fits = budget is >= int.MinValue and <= int.MaxValue &&
                    total is >= int.MinValue and <= int.MaxValue;
        // 100090 allows lexical <= query budget. fb510 and 1f75b0 both reject
        // cost >= 0x4000000; full heaps reject equality with their worst cost.
        bool accepted = fits && language.LexicalSubtotal < ImpossibleCost &&
            language.LexicalSubtotal <= budget && total < ImpossibleCost &&
            (heapIsFull ? total < totalCostLimit : total <= totalCostLimit);
        cancellationToken.ThrowIfCancellationRequested();
        return new(preLexicalPathCost, budget, language, total, fits, accepted);
    }
}
