using System.Numerics;

namespace InkRecognition;

/// <summary>Experimental legacy exponent-10 class score conversion, without candidate or language decoding.</summary>
public static partial class LegacyClassScores
{
    private const int Bias = 4 * 1024 * 1024;
    private const int Threshold = (351 << 16) - Bias;
    // Mathematical reconstruction; no vendor lookup table is distributed.
    private static readonly int[] Exponential = Enumerable.Range(0, 352)
        .Select(i => (int)(25 * Math.Exp(-4 + 22.0 * i / 351))).ToArray();

    /// <summary>
    /// Convert full, unmasked output dots to the legacy quantized probability and
    /// unnormalized log-score channels. This is not the sigmoid-output network path.
    /// Candidate masks and special mixed sigmoid/EXP classes are not supported here.
    /// </summary>
    public static LegacyClassScoreVector Convert(ReadOnlySpan<int> raw, CancellationToken cancellationToken = default)
    {
        if (raw.Length is < 1 or > 4096) throw new ArgumentException("Invalid class count.", nameof(raw));
        cancellationToken.ThrowIfCancellationRequested();
        int max = Threshold;
        foreach (int value in raw) max = Math.Max(max, value);
        long offset = (long)max - Threshold;
        // Native integer subtraction is not valid for an arbitrary overflowing vector.
        if (offset > int.MaxValue) throw new ArgumentException("Unsupported score dynamic range.", nameof(raw));
        var values = new uint[raw.Length];
        for (int i = 0; i < raw.Length; i++)
        {
            if ((long)raw[i] - offset < int.MinValue)
                throw new ArgumentException("Unsupported score dynamic range.", nameof(raw));
            long shifted = (long)raw[i] - offset + Bias;
            values[i] = shifted < 0 ? 0 : (uint)Exponential[(int)Math.Min(351, shifted >> 16)];
        }
        ulong sum;
        do
        {
            sum = 0; foreach (uint value in values) sum += value;
            if (sum > int.MaxValue) for (int i = 0; i < values.Length; i++) values[i] >>= 1;
        } while (sum > int.MaxValue);
        var probability = new int[raw.Length]; var logarithms = new float[raw.Length];
        uint maximum = 0; foreach (uint value in values) maximum = Math.Max(maximum, value);
        if (maximum == 0)
        {
            Array.Fill(logarithms, -4f);
            return new(probability, logarithms, (int)offset);
        }
        int sumShift = Math.Max(0, BitOperations.Log2((uint)sum) - 6);
        uint divisor = (uint)sum >> sumShift;
        int valueShift = 30 - BitOperations.Log2(maximum);
        uint reconstructedSum = divisor << sumShift;
        float sumLog = (float)Math.Log((float)reconstructedSum / 25f);
        for (int i = 0; i < values.Length; i++)
        {
            if ((i & 31) == 0) cancellationToken.ThrowIfCancellationRequested();
            uint fraction = unchecked(values[i] << valueShift) / divisor >> ((valueShift + sumShift - 15) & 31);
            probability[i] = (int)Math.Min(fraction, 32767);
            logarithms[i] = probability[i] == 0 ? -4f :
                sumLog + (float)Math.Log(probability[i] * (1f / 32768)) + (float)offset * (1f / 1048576);
        }
        return new(probability, logarithms, (int)offset);
    }

    /// <summary>Per-class confidence calibration; coefficients must belong to the same model/profile.</summary>
    public static float Calibrate(float score, float center = 0f, float slope = .575f)
    {
        if (!float.IsFinite(score) || !float.IsFinite(center) || !float.IsFinite(slope) || slope <= 0)
            throw new ArgumentException("Finite score and positive calibration slope required.");
        double exponent = Math.Exp(-((double)score - center) * slope);
        return exponent >= float.MaxValue ? 0f : (float)(1 / (1 + exponent));
    }
}

/// <summary>Class channels in canonical network output order. No text mapping is inferred.</summary>
public sealed class LegacyClassScoreVector
{
    public ReadOnlyMemory<int> Probability32768 { get; }
    public ReadOnlyMemory<float> LogScores { get; }
    public int StabilizationOffset { get; }
    internal LegacyClassScoreVector(int[] probability, float[] logarithms, int offset)
    { Probability32768 = probability; LogScores = logarithms; StabilizationOffset = offset; }
}
