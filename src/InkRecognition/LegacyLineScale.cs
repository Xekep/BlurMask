namespace InkRecognition;

/// <summary>Explicit configuration fields of the checked 219 geometry-scale profile.</summary>
public readonly record struct LegacyLineScaleProfile(float Quantile, float SparseQuantile,
    float BlendNumerator, float AssociatedRecordWeight);

public readonly record struct LegacyLineScales(double Primary, double Secondary);

/// <summary>
/// Text 18015af40's sorted-sample selection and resolution-floor arithmetic.
/// Sample production and associated-record selection remain separate upstream operations.
/// </summary>
public static class LegacyLineScale
{
    public static LegacyLineScales Select(ReadOnlySpan<float> samples, int associatedRecordCount,
        float inputResolution, LegacyLineScaleProfile profile, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (samples.Length > 1_000_000 || associatedRecordCount < 0 || associatedRecordCount > 1_000_000 ||
            !float.IsFinite(inputResolution) || inputResolution < 0 ||
            !float.IsFinite(profile.Quantile) || profile.Quantile is < 0 or > 1 ||
            !float.IsFinite(profile.SparseQuantile) || profile.SparseQuantile is < 0 or > 1 ||
            !float.IsFinite(profile.BlendNumerator) || profile.BlendNumerator < 0 ||
            !float.IsFinite(profile.AssociatedRecordWeight) || profile.AssociatedRecordWeight <= 0 ||
            profile.AssociatedRecordWeight > 1_000_000)
            throw new ArgumentException("Unsupported sample count, resolution or line-scale profile.");
        double resolutionFloor = (double)inputResolution / 25.4;
        if (samples.IsEmpty)
        {
            double fallback = resolutionFloor == 0 ? 1 : resolutionFloor;
            return new(fallback, fallback);
        }
        float[] sorted = samples.ToArray();
        for (int i = 0; i < sorted.Length; i++)
        {
            if ((i & 255) == 0) cancellationToken.ThrowIfCancellationRequested();
            if (!float.IsFinite(sorted[i]) || sorted[i] <= 0)
                throw new ArgumentException("Prepared scale samples must be finite and positive.", nameof(samples));
        }
        Array.Sort(sorted);
        cancellationToken.ThrowIfCancellationRequested();
        float quantilePosition = (float)(sorted.Length - 1) * profile.Quantile;
        float roundedPosition = quantilePosition <= 0 ? quantilePosition - .5f : quantilePosition + .5f;
        int primaryIndex = Math.Min((int)roundedPosition, sorted.Length - 1);
        // Native updates a uint count with an integer-truncated weight per associated record.
        long weightedCount = sorted.Length + (long)associatedRecordCount * (int)profile.AssociatedRecordWeight;
        if (weightedCount > int.MaxValue) throw new ArgumentException("Prepared record weighting exceeds the checked count range.");
        float floatWeightedCount = (float)weightedCount;
        float denominatorPart = floatWeightedCount / profile.AssociatedRecordWeight;
        double blend = (double)profile.BlendNumerator / ((double)denominatorPart + 1);
        double blendForQuantile = Math.Min(blend, 1);
        double densePart = (1 - blendForQuantile) * (double)profile.Quantile;
        double sparsePart = (double)profile.SparseQuantile * blendForQuantile;
        double secondaryPosition = (densePart + sparsePart) * sorted.Length;
        double roundedSecondary = secondaryPosition <= 0 ? secondaryPosition - .5 : secondaryPosition + .5;
        int secondaryIndex = Math.Min((int)roundedSecondary, sorted.Length - 1);
        double primary = sorted[primaryIndex];
        double secondary = (double)sorted[secondaryIndex] * (blend + 1);
        if (resolutionFloor > 0 && primary < resolutionFloor)
        {
            secondary = (secondary / primary) * resolutionFloor;
            primary = resolutionFloor;
        }
        return new(primary, secondary);
    }
}
