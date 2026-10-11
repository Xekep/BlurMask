namespace InkRecognition;

/// <summary>One restored sampler252 boundary decision, independent of the NN producer.</summary>
public readonly record struct LegacyCutBoundaryDecision(float RawProbability,
    float EffectiveProbability, bool GeometryPromoted, bool Retained, int CutCost, int JoinCost);

/// <summary>
/// Boundary retention and separate cut/join costs from native1800c2fb0.
/// These positions belong to the sampler's prepared ink; this component does
/// not guess their mapping onto original continuous-stroke points.
/// </summary>
public static class LegacyCutBoundaryPolicy
{
    public const int ImpossibleCost = 0x4000000;

    /// <summary>
    /// Threshold arithmetic is float32, in the native multiplication order.
    /// A current weight of exactly -1 bypasses the adjacent-weight adjustment.
    /// Caller weights and returned threshold must lie in the checked finite domain.
    /// </summary>
    public static float AdjustThreshold(float baseThreshold, float previousWeight, float currentWeight)
    {
        Probability(baseThreshold, nameof(baseThreshold));
        if (!float.IsFinite(previousWeight) || !float.IsFinite(currentWeight) ||
            previousWeight < -1 || currentWeight < -1)
            throw new ArgumentOutOfRangeException(nameof(currentWeight));
        float threshold = currentWeight == -1 ? baseThreshold : baseThreshold * (previousWeight + currentWeight) * .5f;
        Probability(threshold, nameof(currentWeight));
        return threshold;
    }

    /// <summary>
    /// Preserve the raw NN probability separately. A current or preceding
    /// geometry hint promotes effective probability up to the threshold.
    /// Retain only positive effective probability at or above the threshold.
    /// Cost scale is8192 with nearest signed rounding; it is not integer
    /// truncation or log(confidence) from the acoustic-word adapter.
    /// </summary>
    public static LegacyCutBoundaryDecision Evaluate(float neuralProbability, float threshold,
        bool currentGeometryHint, bool previousGeometryHint, float runtimeScale)
    {
        Probability(neuralProbability, nameof(neuralProbability));
        Probability(threshold, nameof(threshold));
        if (!float.IsFinite(runtimeScale) || runtimeScale <= 0) throw new ArgumentOutOfRangeException(nameof(runtimeScale));
        bool promoted = (currentGeometryHint || previousGeometryHint) && neuralProbability < threshold;
        float effective = promoted ? threshold : neuralProbability;
        bool retained = effective > 0 && effective >= threshold;
        int join = !retained ? 0 : effective >= 1 ? ImpossibleCost : LogCost(1f - effective, runtimeScale);
        int cut = effective <= 0 ? ImpossibleCost : LogCost(effective, runtimeScale);
        return new(neuralProbability, effective, promoted, retained, cut, join);
    }

    private static int LogCost(float probability, float scale)
    {
        // Native promotes the already float-rounded probability to double
        // before log and cost multiplication; all multiplications are double.
        double cost = -Math.Log(probability) * 8192d * scale;
        double rounded = cost <= 0 ? cost - .5d : cost + .5d;
        if (!double.IsFinite(rounded) || rounded < int.MinValue || rounded >= 2147483648d)
            throw new ArgumentOutOfRangeException(nameof(scale), "Boundary cost is outside the checked signed-int32 domain.");
        return (int)rounded;
    }

    private static void Probability(float value, string name)
    {
        if (!float.IsFinite(value) || value < 0 || value > 1) throw new ArgumentOutOfRangeException(name);
    }
}
