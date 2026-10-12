namespace InkRecognition;

/// <summary>Actual prepared affine glyph geometry and optional C3D baseline/x-height fraction pair.</summary>
public readonly record struct LegacyPreparedGlyphLineMetricInput(float OriginY,
    float VerticalVectorX, float VerticalVectorY, float? BaselineFraction, float? XHeightFraction)
{
    /// <summary>Retain the prepared affine origin/second vector and actual payload fraction readback.</summary>
    public static LegacyPreparedGlyphLineMetricInput FromPreparedGeometry(LegacyGlyphAffineGeometry geometry,
        LegacyGlyphNormalizedLines? lines)
        => new(geometry.Y, geometry.SecondX, geometry.SecondY, lines?.Baseline, lines?.XHeight);
}

/// <summary>Source2da line-record values written by0376c0/0378e0 after060820.</summary>
public readonly record struct LegacyPreparedGlyphLineMetricValues(double OriginX, double BaselineY,
    double Angle, double XHeight)
{
    /// <summary>Document177590 narrows line double+20 to SDK lineInfo float+18; no stroke-box estimate.</summary>
    public float ToSdkSourceLineXHeight()
    {
        if (!double.IsFinite(XHeight) || XHeight < 0) throw new ArgumentException("Finite nonnegative prepared line x-height required.");
        float value = (float)XHeight;
        if (!float.IsFinite(value)) throw new InvalidDataException("Prepared line x-height exceeds the SDK float range.");
        return value;
    }

    public LegacyFontSizeTextSample ToFontSizeSample(int firstCodePoint, int characterCount,
        float wordLeft, float wordRight, uint flags = 0)
        => new(firstCodePoint, characterCount, ToSdkSourceLineXHeight(), wordLeft, wordRight, flags);
}

/// <summary>
/// Document060820 prepared-glyph -> source2da line metric producer. This is not
/// the initial handwriting line estimator: actual affine glyph geometry and
/// optional payload fractions must already exist. Empty input is a native no-op.
/// </summary>
public static class LegacyPreparedGlyphLineMetricProducer
{
    public static LegacyPreparedGlyphLineMetricValues? Prepare(ReadOnlySpan<LegacyPreparedGlyphLineMetricInput> glyphs,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (glyphs.Length > 4096) throw new ArgumentException("Prepared glyph line exceeds4096 explicit items.");
        if (glyphs.IsEmpty) return null;
        double baselineSum = 0, heightSum = 0;
        foreach (var glyph in glyphs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!float.IsFinite(glyph.OriginY) || !float.IsFinite(glyph.VerticalVectorX) || !float.IsFinite(glyph.VerticalVectorY) ||
                glyph.BaselineFraction.HasValue != glyph.XHeightFraction.HasValue ||
                glyph.BaselineFraction is { } b && !float.IsFinite(b) ||
                glyph.XHeightFraction is { } h && (!float.IsFinite(h) || h < 0))
                throw new ArgumentException("Finite actual prepared affine geometry and matched optional C3D fractions required.");
            // Native hypot promotes both geometry floats to double before evaluation.
            double norm = HypotPrepared(glyph.VerticalVectorX, glyph.VerticalVectorY);
            if (glyph.BaselineFraction is { } baseline)
            {
                baselineSum = baselineSum + (double)baseline * norm + glyph.OriginY;
                heightSum += (double)glyph.XHeightFraction!.Value * norm;
            }
            else
            {
                baselineSum = baselineSum + glyph.OriginY + norm;
                heightSum += norm;
            }
        }
        double y = baselineSum / glyphs.Length, height = heightSum / glyphs.Length;
        if (!double.IsFinite(y) || !double.IsFinite(height)) throw new InvalidDataException("Prepared line metric arithmetic overflow.");
        //060820 passes x/a=0 to source virtual1E0/1E8, and these means as fifth arguments.
        return new(0, y, 0, height);
    }

    // Document19dd00 CRT_hypot's exact finite promoted-binary32 branch. Every
    // nonzero input exponent lies in874..1150, so its double rescaling and
    // subnormal branches cannot be entered. Preserve unsigned exponent test.
    private static double HypotPrepared(float first, float second)
    {
        double x = Math.Abs((double)first), y = Math.Abs((double)second);
        if (x == 0) return y;
        if (y == 0) return x;
        uint ex = (uint)(BitConverter.DoubleToUInt64Bits(x) >> 52);
        uint ey = (uint)(BitConverter.DoubleToUInt64Bits(y) >> 52);
        return unchecked(ex - ey + 0x36) < 0x6d ? Math.Sqrt(x * x + y * y) : x + y;
    }
}
