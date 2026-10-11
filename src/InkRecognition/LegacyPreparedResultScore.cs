namespace InkRecognition;

/// <summary>Explicit caller-owned inputs for the ordinary result-cost stage.</summary>
public readonly record struct LegacyPreparedResultScoreInputs(int CumulativePathCost,
    int CandidateCost, long PreviouslyAccountedCost, int SourceNodeEndInclusive,
    bool IncludeExtraSourceNode, float SourceScoreMultiplier, int LanguageCost);

/// <summary>Independent native result channels; Mass is unnormalised, not acceptance confidence.</summary>
public readonly record struct LegacyPreparedResultScoreValue(float NormalizedPathCost,
    float NormalizedLanguageCost, float LogMass, float Mass);

/// <summary>
/// Ordinary scalar result scoring from native225fb0, without alternate-source
/// correction or native source/backtrack construction. Source node span,
/// previous context offset and multiplier must be supplied explicitly.
/// </summary>
public static class LegacyPreparedResultScore
{
    public static LegacyPreparedResultScoreValue Evaluate(LegacyPreparedResultScoreInputs input,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        // Native multiplies the source span by8192 in signed32 arithmetic.
        // Reject requests outside the positive, non-overflowing checked scope.
        long span = (long)input.SourceNodeEndInclusive + 1 + (input.IncludeExtraSourceNode ? 1 : 0);
        if (input.SourceNodeEndInclusive < 0 || span > int.MaxValue / 8192)
            throw new ArgumentOutOfRangeException(nameof(input), "Source node span exceeds the checked signed32 domain.");
        if (!float.IsFinite(input.SourceScoreMultiplier) || input.SourceScoreMultiplier <= 0)
            throw new ArgumentOutOfRangeException(nameof(input), "Source multiplier must be explicitly positive and finite.");
        long sum = (long)input.CumulativePathCost + input.CandidateCost;
        if (sum is < int.MinValue or > int.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(input), "Combined native integer path cost would overflow.");

        // Cast BEFORE subtraction: native first rounds the integer sum and the
        // previously accounted int64 offset to float independently.
        float combined = (float)(int)sum;
        float remaining = combined - (float)input.PreviouslyAccountedCost;
        float normalized = remaining / (float)(int)(span * 8192);
        float language = (float)input.LanguageCost * 0.00012207031f;
        float logMass = -input.SourceScoreMultiplier * normalized;
        float mass = logMass < -75f ? 0f : logMass > 75f ? 3.73e32f : (float)Math.Exp((double)logMass);
        cancellationToken.ThrowIfCancellationRequested();
        return new(normalized, language, logMass, mass);
    }
}
