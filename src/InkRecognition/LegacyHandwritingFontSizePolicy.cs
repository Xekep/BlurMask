namespace InkRecognition;

/// <summary>Explicit recognized range inputs consumed by iink 180860ac0. Coordinates and containing-line x-height use model units.</summary>
public readonly record struct LegacyFontSizeTextSample(int FirstCodePoint, int CharacterCount,
    float SourceLineXHeight, float WordLeft, float WordRight, uint Flags = 0);

/// <summary>Prepared embedded-math contribution from iink 180833b30; the producer remains caller supplied.</summary>
public readonly record struct LegacyFontSizeMathSample(ulong CharacterCount, float FontSize);

/// <summary>
/// Recovered handwriting-to-typeset-size value policy (iink 180860ac0), given recognized ranges,
/// their prepared ink geometry and actual rendered 'x' metrics. This does not produce recognition ranges.
/// Font size is in model units (millimeters in iink), not typographic points or display pixels.
/// </summary>
public static class LegacyHandwritingFontSizePolicy
{
    /// <param name="canUsePreparedSource">Native segment-condition == false and prepared-source pointer != null.</param>
    public static float MatchPrepared(float styleFontSize, float renderedReferenceXHeight,
        ReadOnlySpan<LegacyFontSizeTextSample> text, ReadOnlySpan<LegacyFontSizeMathSample> math = default,
        bool matchTextSize = true, bool canUsePreparedSource = true)
    {
        if (!float.IsFinite(styleFontSize) || styleFontSize <= 0 || !float.IsFinite(renderedReferenceXHeight))
            throw new ArgumentException("Expected positive finite style size and finite rendered reference height.");
        if (!matchTextSize || !canUsePreparedSource) return styleFontSize;
        float sum = 0; int groups = 0, textCharacters = 0;
        foreach (var item in text)
        {
            Validate(item);
            if ((item.Flags & ((1u << 10) | (1u << 21))) != 0 || IsExcludedCodePoint(item.FirstCodePoint)) continue;
            float sample;
            if (item.CharacterCount == 1 || UsesCharacterXHeight(item.FirstCodePoint)) sample = item.SourceLineXHeight;
            else
            {
                sample = (item.WordRight - item.WordLeft) / (float)item.CharacterCount;
                if (item.CharacterCount == 2) sample = (item.SourceLineXHeight + sample) / (float)item.CharacterCount;
            }
            sum = sample + sum;
            groups = checked(groups + 1); textCharacters = checked(textCharacters + item.CharacterCount);
        }
        if (groups == 0) return styleFontSize; // Native does not inspect math contributions without eligible text.
        float result = styleFontSize;
        if (renderedReferenceXHeight > 0) result = (result / renderedReferenceXHeight) * (sum / (float)groups);
        float mathSum = 0; ulong mathCharacters = 0, mathGroups = 0;
        foreach (var item in math)
        {
            if (!float.IsFinite(item.FontSize) || item.FontSize < 0) throw new ArgumentException("Invalid prepared math size.");
            if (item.FontSize == 0) continue;
            mathGroups = checked(mathGroups + 1); mathCharacters = checked(mathCharacters + item.CharacterCount);
            mathSum = mathSum + item.FontSize;
        }
        if (mathGroups != 0)
            result = ((mathSum / (float)mathGroups) * (float)mathCharacters + (float)textCharacters * result) /
                (float)checked((ulong)textCharacters + mathCharacters);
        if (!float.IsFinite(result)) throw new InvalidDataException("Prepared font-size arithmetic overflow.");
        return result;
    }

    /// <summary>Exact native range/mask filter, rather than a Unicode category approximation.</summary>
    public static bool IsExcludedCodePoint(int codePoint)
    {
        ValidateCodePoint(codePoint); uint cp = (uint)codePoint;
        return InRange(cp, 10, 0x37) && ((0x7f003fffc00009UL >> (int)((cp - 10) & 63)) & 1) != 0 ||
            InRange(cp, 0x5b, 0x24) && ((0xf0000003fUL >> (int)((cp - 0x5b) & 63)) & 1) != 0 ||
            InRange(cp, 0xa0, 0x20) || cp is 0xd7 or 0xf7 or 0x1680 or 0x202f or 0x205f or 0x3000 ||
            InRange(cp, 0x20a0, 0x1f) || InRange(cp, 0x2000, 11);
    }

    /// <summary>Native code-point branches which select the containing line's x-height; intervals retained literally.</summary>
    public static bool UsesCharacterXHeight(int codePoint)
    {
        ValidateCodePoint(codePoint); uint cp = (uint)codePoint;
        return InRange(cp, 0xff00, 0x61) || InRange(cp, 0x4e00, 0x5200) || InRange(cp, 0x3000, 400) ||
            InRange(cp, 0x3400, 0x19c0) || InRange(cp, 0x20000, 0xa6e0) || InRange(cp, 0x31a0, 0x20) ||
            InRange(cp, 0x1100, 0x100) || InRange(cp, 0xac00, 0x2bb0) || InRange(cp, 0x31f0, 0x10) ||
            InRange(cp, 0xd800, 0x400);
    }

    /// <summary>iink 18085b150: optional caller quantization about an explicit reference size.</summary>
    public static float QuantizePrepared(float fontSize, float referenceSize, float relativeStep)
    {
        if (!float.IsFinite(fontSize) || fontSize < 0 || !float.IsFinite(referenceSize) || referenceSize <= 0 ||
            !float.IsFinite(relativeStep) || relativeStep <= 0) throw new ArgumentException("Invalid size quantization inputs.");
        float ratio = fontSize / referenceSize;
        float rounded = referenceSize <= fontSize ? MathF.Round((ratio - 1f) / relativeStep, MidpointRounding.AwayFromZero) :
            MathF.Max(1f, MathF.Round(ratio * 4f, MidpointRounding.AwayFromZero)) - 4f;
        if (!float.IsFinite(rounded) || rounded < int.MinValue || rounded >= 2147483648f)
            throw new InvalidDataException("Font-size quantization index overflow.");
        int index = (int)rounded;
        float result = index < 0 ? (float)(index + 4) * referenceSize * 0.25f :
            ((float)index * relativeStep + 1f) * referenceSize;
        if (!float.IsFinite(result)) throw new InvalidDataException("Font-size quantization overflow.");
        return result;
    }

    /// <summary>iink 180869370: whether the native cached font-size setter marks geometry dirty.</summary>
    public static bool RequiresSizeUpdate(float previousSize, float nextSize)
    {
        if (!float.IsFinite(previousSize) || !float.IsFinite(nextSize)) throw new ArgumentException("Nonfinite font size.");
        float tolerance = MathF.Max(0.002f, MathF.Max(MathF.Abs(previousSize), MathF.Abs(nextSize)) * 1e-7f);
        return tolerance <= MathF.Abs(previousSize - nextSize);
    }

    private static bool InRange(uint codePoint, uint start, uint length) => unchecked(codePoint - start) < length;
    private static void ValidateCodePoint(int codePoint)
    {
        // Native branches include UTF-16 high surrogates explicitly; retain that domain as well as scalars.
        if ((uint)codePoint > 0x10ffff) throw new ArgumentOutOfRangeException(nameof(codePoint));
    }
    private static void Validate(LegacyFontSizeTextSample item)
    {
        ValidateCodePoint(item.FirstCodePoint);
        if (item.CharacterCount <= 0 || !float.IsFinite(item.SourceLineXHeight) || item.SourceLineXHeight < 0 ||
            !float.IsFinite(item.WordLeft) || !float.IsFinite(item.WordRight) || item.WordRight < item.WordLeft)
            throw new ArgumentException("Invalid prepared text range geometry.");
    }
}
