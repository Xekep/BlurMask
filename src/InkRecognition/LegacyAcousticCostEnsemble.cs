namespace InkRecognition;

/// <summary>Caller-owned quantized probabilities and explicit native family weight.</summary>
public sealed record LegacyAcousticCostFamily(ReadOnlyMemory<int> Probability32768, float Weight);

/// <summary>
/// Bounded restored integer-cost scalar branch from native1801b9e20 for one
/// seven-slot profile. Related-form penalties and additional native profiles
/// are deliberately not inferred. This does not combine language costs.
/// </summary>
public static class LegacyAcousticCostEnsemble
{
    public static int[] Combine(IReadOnlyList<LegacyAcousticCostFamily?> slots,
        float costMultiplier, ReadOnlySpan<byte> enabledClasses = default,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(slots); cancellationToken.ThrowIfCancellationRequested();
        if (slots.Count != 7) throw new ArgumentException("Expected exactly seven explicit ordered family slots.", nameof(slots));
        if (!float.IsFinite(costMultiplier) || costMultiplier <= 0)
            throw new ArgumentOutOfRangeException(nameof(costMultiplier), "Explicit finite positive multiplier required.");
        int width = 0;
        foreach (var family in slots)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (family is null) continue;
            if (family.Probability32768.Length is < 1 or > 512 ||
                (width != 0 && family.Probability32768.Length != width) ||
                !float.IsFinite(family.Weight) || family.Weight < 0)
                throw new ArgumentException("Matching bounded probabilities and finite nonnegative weights required.", nameof(slots));
            width = family.Probability32768.Length;
            foreach (int probability in family.Probability32768.Span)
                if ((uint)probability > 32768) throw new ArgumentException("Probability outside quantized domain.", nameof(slots));
        }
        if (width == 0) throw new ArgumentException("At least one family provides the output width.", nameof(slots));
        if (!enabledClasses.IsEmpty && enabledClasses.Length != width)
            throw new ArgumentException("Class mask must match output width.", nameof(enabledClasses));
        var result = new int[width]; float denominator = 0;
        foreach (var family in slots)
        {
            cancellationToken.ThrowIfCancellationRequested(); if (family is null) continue;
            denominator += family.Weight;
            if (!float.IsFinite(denominator)) throw new ArgumentException("Weight sum overflow.", nameof(slots));
            float scaledWeight = family.Weight * costMultiplier;
            if (!float.IsFinite(scaledWeight)) throw new ArgumentException("Scaled weight overflow.", nameof(slots));
            for (int i = 0; i < width; i++)
            {
                if (!enabledClasses.IsEmpty && enabledClasses[i] == 0) continue;
                int cost = LegacyAcousticFixedLog.ProbabilityCost(family.Probability32768.Span[i]);
                int contribution = scaledWeight == 1f ? cost : ToInt((float)cost * scaledWeight);
                result[i] = checked(result[i] + contribution);
            }
        }
        float inverse = denominator > 0 ? 1f / denominator : 0;
        for (int i = 0; i < width; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (denominator <= 0 || (!enabledClasses.IsEmpty && enabledClasses[i] == 0))
                result[i] = LegacyAcousticFixedLog.ImpossibleCost;
            else
            {
                int value = denominator == 1f ? result[i] : denominator == 2f ? result[i] >> 1 : ToInt((float)result[i] * inverse);
                result[i] = Math.Min(LegacyAcousticFixedLog.ImpossibleCost, value);
            }
        }
        return result;
    }

    private static int ToInt(float value)
    {
        if (!float.IsFinite(value) || value < 0 || value >= 2147483648f)
            throw new ArgumentOutOfRangeException(nameof(value), "Weighted cost exceeds supported finite int32 conversion.");
        return (int)value;
    }
}
