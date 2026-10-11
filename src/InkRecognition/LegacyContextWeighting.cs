namespace InkRecognition;

/// <summary>A caller-selected prior group's mean X and28 network probabilities.</summary>
public sealed record LegacyContextObservation(float CenterX, ReadOnlyMemory<float> Probabilities);

/// <summary>
/// Checked ca5f0 context weighting after native prior-group selection. Inputs
/// remain explicit: selected prior observations, target mean X, line height,
/// offset and left/right variances. This restores neither the30-channel context
/// geometry, its neural probabilities nor prior-group/line selection.
/// </summary>
public static class LegacyContextWeighting
{
    public const int Width = 28, MaximumObservations = 1024;

    public static float[] Combine(IReadOnlyList<LegacyContextObservation> selectedObservations,
        float targetMeanX, float lineHeight, float leftGapOffset, float leftVariance,
        float rightVariance, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(selectedObservations);
        if (selectedObservations.Count > MaximumObservations) throw new ArgumentException("Selected context observations exceed the bounded work domain.");
        if (!float.IsFinite(targetMeanX) || !float.IsFinite(lineHeight) || lineHeight <= 0 ||
            !float.IsFinite(leftGapOffset) || !float.IsFinite(leftVariance) || leftVariance <= 0 ||
            !float.IsFinite(rightVariance) || rightVariance <= 0)
            throw new ArgumentException("Finite context geometry, positive line height and positive variances are required.");
        cancellationToken.ThrowIfCancellationRequested();
        var result = new float[Width];
        foreach (var observation in selectedObservations)
        {
            cancellationToken.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(observation);
            var probabilities = observation.Probabilities.Span;
            if (!float.IsFinite(observation.CenterX) || probabilities.Length != Width)
                throw new ArgumentException("Each selected observation needs finite center X and28 probabilities.");
            foreach (float probability in probabilities)
                if (!float.IsFinite(probability) || probability is < 0 or > 1)
                    throw new ArgumentException("Context observations must contain finite network probabilities in0..1.");
            float distance = (observation.CenterX - targetMeanX) / lineHeight;
            float exponent;
            if (distance >= 0) exponent = (distance * distance * -.5f) / rightVariance;
            else
            {
                distance += leftGapOffset;
                exponent = distance <= 0 ? (distance * distance * -.5f) / leftVariance : 0;
            }
            if (exponent < -75f) continue;
            // Native computes exp(double(floatExponent)), then narrows tofloat
            // before multiplication and max. Preserve the rounding stages.
            float weight = (float)Math.Exp(exponent);
            for (int i = 0; i < Width; i++)
            {
                float candidate = weight * probabilities[i];
                if (result[i] < candidate) result[i] = candidate;
            }
        }
        for (int i = 0; i < Width; i++)
        {
            result[i] = (result[i] + result[i]) - 1f;
            if (result[i] < -.99f) result[i] = -1f;
        }
        return result;
    }
}
