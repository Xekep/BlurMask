using System.Numerics;

namespace InkRecognition;

public static partial class LegacyClassScores
{
    // Probability-only E9360 branch. No native mixed log channel is inferred.
    internal static int[] ConvertMixedProbabilities(ReadOnlySpan<int> raw, int specialOutputLabel,
        CancellationToken cancellationToken = default)
    {
        if (raw.Length is < 2 or > 512 || (uint)specialOutputLabel >= (uint)raw.Length)
            throw new ArgumentException("A bounded output vector and its actual AL special label are required.");
        cancellationToken.ThrowIfCancellationRequested();
        int maximumDot = Threshold;
        for (int i = 0; i < raw.Length; i++)
            if (i != specialOutputLabel) maximumDot = Math.Max(maximumDot, raw[i]);
        long offset = (long)maximumDot - Threshold;
        if (offset > int.MaxValue) throw new NotSupportedException("Mixed score stabilization exceeds the checked int32 domain.");
        var values = new uint[raw.Length];
        for (int i = 0; i < raw.Length; i++)
        {
            if (i == specialOutputLabel) continue;
            long adjusted = (long)raw[i] - offset;
            if (adjusted < int.MinValue) throw new NotSupportedException("Mixed score subtraction exceeds the checked int32 domain.");
            long shifted = adjusted + Bias;
            // The checked route excludes native wrapped raw+Bias operands.
            if (shifted > int.MaxValue) throw new NotSupportedException("Mixed EXP lookup addition exceeds the checked int32 domain.");
            values[i] = shifted < 0 ? 0 : (uint)Exponential[(int)Math.Min(351, shifted >> 16)];
        }
        ulong sum;
        do
        {
            cancellationToken.ThrowIfCancellationRequested();
            sum = 0;
            for (int i = 0; i < values.Length; i++) if (i != specialOutputLabel) sum += values[i];
            if (sum > int.MaxValue)
                for (int i = 0; i < values.Length; i++) if (i != specialOutputLabel) values[i] >>= 1;
        } while (sum > int.MaxValue);
        uint maximum = 0;
        for (int i = 0; i < values.Length; i++) if (i != specialOutputLabel) maximum = Math.Max(maximum, values[i]);
        if (maximum == 0)
            throw new NotSupportedException("All-zero ordinary EXP leaves native cached probabilities history-dependent.");
        int special = MixedSigmoidQ15(raw[specialOutputLabel]);
        var probability = new int[raw.Length];
        int sumShift = Math.Max(0, BitOperations.Log2((uint)sum) - 6);
        uint divisor = (uint)sum >> sumShift;
        int valueShift = 30 - BitOperations.Log2(maximum);
        for (int i = 0; i < values.Length; i++)
        {
            if ((i & 31) == 0) cancellationToken.ThrowIfCancellationRequested();
            if (i == specialOutputLabel) { probability[i] = special; continue; }
            uint fraction = unchecked(values[i] << valueShift) / divisor >> ((valueShift + sumShift - 15) & 31);
            probability[i] = (int)Math.Min(fraction, 32767);
        }
        return probability;
    }

    internal static int MixedSigmoidQ15(int raw)
    {
        // E9552..E9579: separately rounded CVTDQ2PS, MULSS, DIVSS; q10 only.
        float scaled = (float)raw * -2f;
        float exponent = scaled / 1048576f;
        if (exponent > 75f) return 0;
        double rounded = 32768.0 / (Math.Exp((double)exponent) + 1.0) + .5;
        // Admission assumes the scoped native/platform-exp error bound documented
        // by the independent interval oracle, NOT universal CRT bit identity.
        // Reject output rounding within2^-24 of an integer boundary.
        const double margin = 1.0 / 16777216.0;
        if (Math.Floor(rounded - margin) != Math.Floor(rounded + margin))
            throw new NotSupportedException("Mixed sigmoid integer rounding is outside the checked margin.");
        int probability = (int)rounded;
        if ((uint)probability > 32768) throw new NotSupportedException("Mixed sigmoid result outside Q15 domain.");
        return probability;
    }
}
