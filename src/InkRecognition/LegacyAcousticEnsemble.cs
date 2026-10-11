namespace InkRecognition;

/// <summary>Caller-owned EXP probabilities and matching mode1 class calibration.</summary>
public sealed record LegacyAcousticFamilyScores(ReadOnlyMemory<int> Probability32768,
    ReadOnlyMemory<float> Centers, ReadOnlyMemory<float> Slopes);

/// <summary>
/// Checked single-profile mode1 branch of1bcbd0/13e8e0: class-specific logistic
/// calibration of each active family's EXP probabilities, then native-order
/// float summation and mean. Family selection, candidate masks, case policy and
/// other calibration branches are not inferred. No vendor coefficients bundled.
/// </summary>
public static class LegacyAcousticEnsemble
{
    public static float[] Combine(IReadOnlyList<LegacyAcousticFamilyScores?> familySlots,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(familySlots);
        if (familySlots.Count != 7) throw new ArgumentException("Supply the seven native family slots, with null for inactive families.");
        cancellationToken.ThrowIfCancellationRequested(); int width = 0, active = 0;
        foreach (var family in familySlots)
        {
            if (family is null) continue;
            int count = family.Probability32768.Length;
            if (count is < 1 or > 4096 || (width != 0 && width != count) ||
                family.Centers.Length != count || family.Slopes.Length != count)
                throw new ArgumentException("Active families require equal1..4096 class widths and matching calibration arrays.");
            width = count; active++;
            for (int i = 0; i < count; i++)
            {
                if (family.Probability32768.Span[i] is < 0 or > 32767 ||
                    !float.IsFinite(family.Centers.Span[i]) || !float.IsFinite(family.Slopes.Span[i]) || family.Slopes.Span[i] <= 0)
                    throw new ArgumentException("Expected quantized EXP probabilities and finite positive-slope calibration.");
            }
        }
        if (active == 0) throw new ArgumentException("At least one active acoustic family is required.");
        var output = new float[width];
        Span<float> confidence = stackalloc float[7];
        for (int label = 0; label < width; label++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            confidence.Clear();
            for (int familyIndex = 0; familyIndex < familySlots.Count; familyIndex++)
            {
                var family = familySlots[familyIndex]; if (family is null) continue;
                float probability = family.Probability32768.Span[label] * (1f / 32768);
                confidence[familyIndex] = LegacyClassScores.Calibrate(probability,
                    family.Centers.Span[label], family.Slopes.Span[label]);
            }
            // Native adds slot1 before slot0, then slots2..6; preserving float
            // stages matters for independent rounding controls.
            float sum = confidence[1] + confidence[0];
            for (int familyIndex = 2; familyIndex < 7; familyIndex++) sum += confidence[familyIndex];
            float mean = sum / active; output[label] = mean >= 1 ? 1 : mean;
        }
        return output;
    }
}
