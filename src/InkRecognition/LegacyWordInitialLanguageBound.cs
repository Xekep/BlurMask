namespace InkRecognition;

public readonly record struct LegacyInitialLanguageBound(int WordCost,
    int UnigramCost, int BigramCost, int NativeInitialBound);

/// <summary>
/// Restored fe150/100070 initial cache bound for the checked order2/flags3
/// language storage with a word codebook and no backoff table. Category and
/// boundary costs are excluded. This is not a universal mathematical lower
/// bound for arbitrary caller calibration or unordered stored codebooks.
/// </summary>
public static class LegacyWordInitialLanguageBound
{
    public static LegacyInitialLanguageBound GetNativeInitialLanguageBound(
        LegacyWordLanguageModel model, LegacyWordLanguageCostProfile profile,
        float runtimeScale, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model); ArgumentNullException.ThrowIfNull(profile);
        cancellationToken.ThrowIfCancellationRequested();
        if (profile.WordSlope != profile.UnigramSlope)
            throw new ArgumentException("Matching inspected native word/unigram slope required.", nameof(profile));
        int word = profile.TransformWordCost(model.GetStoredInitialWordLevel(), runtimeScale);
        int uni = profile.TransformClassCost(model.GetStoredInitialClassLevel(false), false, runtimeScale);
        int bi = profile.TransformClassCost(model.GetStoredInitialClassLevel(true), true, runtimeScale);
        int bound = Math.Min(LegacyWordCandidateScorer.ImpossibleCost,
            Math.Min(checked(word + uni), checked(word + bi)));
        cancellationToken.ThrowIfCancellationRequested();
        return new(word, uni, bi, bound);
    }
}
