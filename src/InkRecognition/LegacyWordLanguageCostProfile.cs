using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>
/// Explicit caller-selected positive-runtime-scale scalar cost conversion from
/// native 1800fe2a0. This does not infer a language mode, apply history/category
/// rules or combine acoustic costs. Four stored profiles are available.
/// </summary>
public sealed class LegacyWordLanguageCostProfile
{
    public float WordSlope { get; }
    public float WordOffset { get; }
    public float UnigramSlope { get; }
    public float UnigramOffset { get; }
    public float BigramSlope { get; }
    public float BigramOffset { get; }

    public LegacyWordLanguageCostProfile(float wordSlope, float wordOffset, float unigramSlope,
        float unigramOffset, float bigramSlope, float bigramOffset)
    {
        foreach (float value in new[] { wordSlope, wordOffset, unigramSlope, unigramOffset, bigramSlope, bigramOffset })
            if (!float.IsFinite(value)) throw new ArgumentException("Finite explicit cost coefficients required.");
        WordSlope = wordSlope; WordOffset = wordOffset; UnigramSlope = unigramSlope;
        UnigramOffset = unigramOffset; BigramSlope = bigramSlope; BigramOffset = bigramOffset;
    }

    /// <summary>
    /// Validate the complete checked language storage once and recover all four
    /// scalar profiles. This creates temporary raw lookup validation storage;
    /// load once and reuse. Native runtime mode selection remains the caller's.
    /// </summary>
    public static LegacyWordLanguageCostProfile[] LoadProfiles(ReadOnlySpan<byte> completeLanguageObject,
        CancellationToken cancellationToken = default)
    {
        _ = LegacyWordLanguageModel.Load(completeLanguageObject, cancellationToken);
        // Proven order2/flags3 tail: four model-major two-float offset arrays,
        // word offsets16, boundary offsets16, four two-float slope arrays,
        // additional calibration32, followed by 9498 punctuation bytes.
        const int punctuationBytes = 2 + 16 + 16 + 0x2a4 + 0x2254;
        int start = completeLanguageObject.Length - punctuationBytes - 128;
        static float Read(ReadOnlySpan<byte> data, int absolute) => BitConverter.Int32BitsToSingle(
            BinaryPrimitives.ReadInt32LittleEndian(data.Slice(absolute, 4)));
        var result = new LegacyWordLanguageCostProfile[4];
        for (int profile = 0; profile < 4; profile++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            float wordSlope = Read(completeLanguageObject, start + 64 + profile * 8);
            result[profile] = new(wordSlope, Read(completeLanguageObject, start + 32 + profile * 4), wordSlope, Read(completeLanguageObject, start + profile * 8),
                Read(completeLanguageObject, start + 64 + profile * 8 + 4), Read(completeLanguageObject, start + profile * 8 + 4));
        }
        return result;
    }

    /// <summary>Convert a stored per-word level with an explicit positive native runtime scale.</summary>
    public int TransformWordCost(int storedCost, float runtimeScale) => Transform(storedCost, WordSlope, WordOffset, runtimeScale);

    /// <summary>Convert a stored class unigram/bigram level; no history or word cost is added.</summary>
    public int TransformClassCost(int storedCost, bool bigram, float runtimeScale) =>
        Transform(storedCost, bigram ? BigramSlope : UnigramSlope, bigram ? BigramOffset : UnigramOffset, runtimeScale);

    private static int Transform(int stored, float slope, float offset, float runtimeScale)
    {
        if (!float.IsFinite(runtimeScale) || runtimeScale <= 0)
            throw new ArgumentOutOfRangeException(nameof(runtimeScale), "Only explicit finite positive runtime scales are restored.");
        float scaledSlope = runtimeScale * slope, scaledOffset = runtimeScale * offset;
        float level = (float)stored * (1f / 8192);
        float weighted = level * scaledSlope;
        float centered = weighted - scaledOffset;
        float value = centered * 8192f;
        // Unchecked native out-of-range casts are not a portable contract.
        if (!float.IsFinite(value) || value < -2147483648f || value >= 2147483648f)
            throw new ArgumentOutOfRangeException(nameof(stored), "Cost exceeds supported finite int32 conversion.");
        return (int)value;
    }
}
