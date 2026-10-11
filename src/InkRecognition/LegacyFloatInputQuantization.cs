namespace InkRecognition;

/// <summary>
/// Checked type212 float-input append arithmetic (ebb60): float multiplication,
/// integer truncation and signed-short wrapping. No rounding or saturation.
/// The caller must choose the exponent of the same network profile.
/// </summary>
public static class LegacyFloatInputQuantization
{
    public static short[] Quantize(ReadOnlySpan<float> values, int exponent = 10,
        CancellationToken cancellationToken = default)
    {
        if (values.Length is < 1 or > 4096) throw new ArgumentException("Expected1..4096 float network inputs.", nameof(values));
        // The native exponent is byte-masked; restrict to a positive, useful
        // signed-short scale domain instead of reproducing negative1<<31.
        if (exponent is < 0 or > 14) throw new ArgumentOutOfRangeException(nameof(exponent));
        cancellationToken.ThrowIfCancellationRequested();
        float scale = 1 << exponent; var result = new short[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            if ((i & 31) == 0) cancellationToken.ThrowIfCancellationRequested();
            float scaled = values[i] * scale;
            // CVTTSS2SI has an indefinite out-of-range result; reject that
            // platform-dependent domain instead of guessing managed casts.
            if (!float.IsFinite(scaled) || (double)scaled < int.MinValue || (double)scaled >= 2147483648d)
                throw new ArgumentException("Scaled float inputs must be finite and representable as signed int32.", nameof(values));
            result[i] = unchecked((short)(int)scaled);
        }
        return result;
    }
}
