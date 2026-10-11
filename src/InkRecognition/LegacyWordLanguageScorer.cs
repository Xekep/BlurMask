namespace InkRecognition;

public readonly record struct LegacyWordLanguageScore(int WordClass, int? PreviousClass,
    int WordCost, int ClassCost, int CategoryCost, int BoundaryCost, int LexicalSubtotal);

/// <summary>
/// Restored known-word lexical subtotal for the inspected order2/flags3
/// native100090 scalar path, with caller-selected model/profile/history/flags.
/// No acoustic total, candidate search, history update or mode selection.
/// </summary>
public sealed class LegacyWordLanguageScorer
{
    private readonly LegacyWordLanguageModel _model;
    private readonly LegacyWordLanguageCostProfile _profile;
    private readonly LegacyWordAuxiliaryProfile _auxiliary;
    public float RuntimeScale { get; }

    public LegacyWordLanguageScorer(LegacyWordLanguageModel model,
        LegacyWordLanguageCostProfile profile, LegacyWordAuxiliaryProfile auxiliary,
        float runtimeScale)
    {
        ArgumentNullException.ThrowIfNull(model); ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(auxiliary);
        if (!float.IsFinite(runtimeScale) || runtimeScale <= 0) throw new ArgumentOutOfRangeException(nameof(runtimeScale));
        if (profile.WordSlope != profile.UnigramSlope || profile.WordOffset != auxiliary.WordOffset)
            throw new ArgumentException("Matching explicit native word and auxiliary profile required.");
        _model = model; _profile = profile; _auxiliary = auxiliary; RuntimeScale = runtimeScale;
    }

    /// <summary>
    /// Score one validated dictionary word ID. A zero/one history entry uses
    /// its lower24-bit prior class and high category nibble. No Unicode label
    /// conversion or word-history mutation is inferred; all flags are explicit.
    /// </summary>
    public LegacyWordLanguageScore Score(int wordId, ReadOnlySpan<uint> history,
        int category, bool clearFirstCategoryTag, bool suppressCategoryCost,
        bool includeBoundaryCost, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (history.Length > 1) throw new ArgumentException("Order2 supports at most one preceding class.", nameof(history));
        int wordClass = _model.GetWordClass(wordId);
        int? previousClass = history.IsEmpty ? null : (int)(history[0] & 0xffffff);
        // Native100090 reads word table, then adds category and boundary, then
        // class unigram/bigram. Checked overflow is the bounded API contract.
        int word = _profile.TransformWordCost(_model.GetStoredWordCost(wordId), RuntimeScale);
        int categoryCost = _auxiliary.ScoreCategory(category, history, clearFirstCategoryTag, suppressCategoryCost).Cost;
        int boundary = includeBoundaryCost ? _auxiliary.GetBoundaryCost(RuntimeScale) : 0;
        int cls = _profile.TransformClassCost(_model.GetStoredClassCost(wordClass, previousClass), previousClass is not null, RuntimeScale);
        int subtotal = checked(checked(checked(word + categoryCost) + boundary) + cls);
        return new(wordClass, previousClass, word, cls, categoryCost, boundary, subtotal);
    }
}
