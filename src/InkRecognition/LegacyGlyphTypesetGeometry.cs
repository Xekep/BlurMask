namespace InkRecognition;

public readonly record struct LegacyFontGlyphMetrics(LegacyLineBounds Bounds, float LeftSideBearing, float RightSideBearing);
public readonly record struct LegacyGlyphTypesetGeometry(LegacyLineBounds Bounds, float BaselineFraction,
    float XHeightFraction, float LeftBearingFraction, float RightBearingEndpointFraction);

/// <summary>
/// Recovered iink 1806e8660 / 1806ec4a0 glyph geometry metadata construction.
/// Inputs are actual rendered-font metrics and an explicitly prepared glyph box;
/// this is not a handwriting-to-font-size estimator.
/// </summary>
public static class LegacyGlyphTypesetGeometryBuilder
{
    /// <summary>iink 180450590: use measured character height when positive, otherwise the style font line-height field.</summary>
    public static float ReferenceCharacterHeight(float? measuredHeight, float styleFontLineHeight)
    {
        if(!float.IsFinite(styleFontLineHeight) || styleFontLineHeight<=0 || measuredHeight is{} value && !float.IsFinite(value))
            throw new ArgumentException("Expected finite font metrics and positive style font line height.");
        return measuredHeight is{} height && height>0 ? height : styleFontLineHeight;
    }

    /// <summary>iink 180450d20: clamp negative provider box dimensions, preserve bearings.</summary>
    public static LegacyFontGlyphMetrics SanitizeProviderMetrics(LegacyFontGlyphMetrics metrics)
    {
        var b=metrics.Bounds;
        if(!float.IsFinite(b.X)||!float.IsFinite(b.Y)||!float.IsFinite(b.Width)||!float.IsFinite(b.Height)||
            !float.IsFinite(metrics.LeftSideBearing)||!float.IsFinite(metrics.RightSideBearing))
            throw new ArgumentException("Nonfinite provider glyph metric.");
        return metrics with { Bounds=b with { Width=MathF.Max(b.Width,0), Height=MathF.Max(b.Height,0) } };
    }

    public static LegacyGlyphTypesetGeometry NormalizePrepared(LegacyFontGlyphMetrics providerMetrics,
        LegacyLineBounds preparedBox, float baselineY, float referenceXHeight, float horizontalScale)
    {
        providerMetrics=SanitizeProviderMetrics(providerMetrics);
        if(!float.IsFinite(preparedBox.X)||!float.IsFinite(preparedBox.Y)||!float.IsFinite(preparedBox.Width)||
            !float.IsFinite(preparedBox.Height)||preparedBox.Width<0||preparedBox.Height<=0||!float.IsFinite(baselineY)||
            !float.IsFinite(referenceXHeight)||referenceXHeight<=0||!float.IsFinite(horizontalScale)||horizontalScale<=0)
            throw new ArgumentException("Expected an explicit positive-height prepared glyph box and font scale.");
        float baseline=(baselineY-preparedBox.Y)/preparedBox.Height;
        float xHeight=(referenceXHeight/preparedBox.Height)*horizontalScale;
        float left=0,right=1;
        if(providerMetrics.LeftSideBearing!=0||providerMetrics.RightSideBearing!=0)
        {
            if(providerMetrics.Bounds.Width<=0)throw new ArgumentException("Nonzero bearings need positive provider glyph width.");
            left=providerMetrics.LeftSideBearing/providerMetrics.Bounds.Width;
            right=providerMetrics.RightSideBearing/providerMetrics.Bounds.Width+1f;
        }
        if(!float.IsFinite(baseline)||!float.IsFinite(xHeight)||!float.IsFinite(left)||!float.IsFinite(right))
            throw new InvalidDataException("Glyph geometry fraction overflow.");
        return new(preparedBox,baseline,xHeight,left,right);
    }
}
