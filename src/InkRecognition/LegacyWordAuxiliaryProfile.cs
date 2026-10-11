using System.Buffers.Binary;

namespace InkRecognition;

public readonly record struct LegacyCategoryCost(int Cost, int TaggedHistoryPrefixLength);

/// <summary>
/// Restored explicit order2 boundary and category cost branches from1800fe2a0
/// and1800ffcf0. This does not choose category labels, a runtime mode or a total
/// recognition score. Order2 supports at most one preceding history slot.
/// </summary>
public sealed class LegacyWordAuxiliaryProfile
{
    private readonly int[] _categoryPairs;
    public ushort CategoryFlags { get; }
    public float WordOffset { get; }
    public float BoundaryOffset { get; }
    public float CategoryOffset { get; }
    public float CategorySlope { get; }

    public LegacyWordAuxiliaryProfile(ushort categoryFlags, float wordOffset,
        float boundaryOffset, float categoryOffset, float categorySlope,
        ReadOnlySpan<float> categoryTransitions13By13)
    {
        if (categoryTransitions13By13.Length != 169 || !float.IsFinite(wordOffset) ||
            !float.IsFinite(boundaryOffset) || !float.IsFinite(categoryOffset) || !float.IsFinite(categorySlope))
            throw new ArgumentException("Exact13-by13 transitions and finite explicit coefficients required.");
        CategoryFlags = categoryFlags; WordOffset = wordOffset; BoundaryOffset = boundaryOffset;
        CategoryOffset = categoryOffset; CategorySlope = categorySlope;
        _categoryPairs = new int[169];
        for (int i = 0; i < 169; i++)
        {
            if (!float.IsFinite(categoryTransitions13By13[i])) throw new ArgumentException("Nonfinite category transition.");
            // Native initializes only nonzero-category pairs; zero-tag history
            // uses the fixed fallback branch instead of a transition lookup.
            if (i / 13 != 0 && i % 13 != 0)
            {
                float value = (categorySlope * categoryTransitions13By13[i] + categoryOffset) * 8192f;
                float rounded = value <= 0 ? value - .5f : value + .5f;
                _categoryPairs[i] = checked(-ToInt(rounded));
            }
        }
    }

    public static LegacyWordAuxiliaryProfile[] LoadProfiles(ReadOnlySpan<byte> completeLanguageObject,
        CancellationToken cancellationToken = default)
    {
        _ = LegacyWordLanguageModel.Load(completeLanguageObject, cancellationToken);
        const int punctuationBytes = 2 + 16 + 16 + 0x2a4 + 0x2254;
        int start = completeLanguageObject.Length - punctuationBytes, profilesStart = start - 128;
        static float Read(ReadOnlySpan<byte> data, int offset) => BitConverter.Int32BitsToSingle(
            BinaryPrimitives.ReadInt32LittleEndian(data.Slice(offset, 4)));
        ushort flags = BinaryPrimitives.ReadUInt16LittleEndian(completeLanguageObject.Slice(start, 2));
        var pairs = new float[169];
        for (int i = 0; i < pairs.Length; i++) pairs[i] = Read(completeLanguageObject, start + 34 + i * 4);
        // This checked profile is order2, so the stored second-history table
        // is not selected by100090. Validate its floats, do not guess history3.
        for (int i = 0; i < 2197; i++)
        {
            if ((i & 255) == 0) cancellationToken.ThrowIfCancellationRequested();
            if (!float.IsFinite(Read(completeLanguageObject, start + 34 + 676 + i * 4)))
                throw new ArgumentException("Nonfinite second-history category storage.");
        }
        var result = new LegacyWordAuxiliaryProfile[4];
        for (int p = 0; p < result.Length; p++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            result[p] = new(flags, Read(completeLanguageObject, profilesStart + 32 + p * 4),
                Read(completeLanguageObject, profilesStart + 48 + p * 4),
                Read(completeLanguageObject, start + 2 + p * 4), Read(completeLanguageObject, start + 18 + p * 4), pairs);
        }
        return result;
    }

    /// <summary>
    /// Caller decides whether the boundary flag is present. This returns the
    /// cache94c value for the proven positive-scale branch with word levels.
    /// </summary>
    public int GetBoundaryCost(float runtimeScale)
    {
        if (!float.IsFinite(runtimeScale) || runtimeScale <= 0) throw new ArgumentOutOfRangeException(nameof(runtimeScale));
        float boundary = runtimeScale * BoundaryOffset, word = runtimeScale * WordOffset;
        return ToInt((boundary - word) * -8192f);
    }

    /// <summary>
    /// Explicit native category0..12 and zero/one history slot. Category tag
    /// is bits24..27; lower class bits and top nibble do not affect this cost.
    /// clearFirstCategoryTag emulates native mutation without modifying input.
    /// suppressCategoryCost corresponds to the separate native suppress flag.
    /// </summary>
    public LegacyCategoryCost ScoreCategory(int category, ReadOnlySpan<uint> history,
        bool clearFirstCategoryTag, bool suppressCategoryCost)
    {
        if ((uint)category > 12 || history.Length > 1) throw new ArgumentOutOfRangeException(nameof(category));
        int tag = history.IsEmpty ? 0 : (int)((history[0] >> 24) & 15);
        if (tag > 12) throw new ArgumentException("Unsupported stored category tag.", nameof(history));
        if (clearFirstCategoryTag) tag = 0;
        int count = CategoryFlags != 0 && category != 0 && tag != 0 ? 1 : 0;
        if (suppressCategoryCost) return new(0, count);
        if (count == 1) return new(_categoryPairs[tag * 13 + category], count);
        int fallback = category switch { 3 => clearFirstCategoryTag ? -2 : 1, 5 or 6 => 2, 10 or 11 or 12 => 3, _ => 0 };
        return new(fallback, count);
    }

    private static int ToInt(float value)
    {
        if (!float.IsFinite(value) || value < -2147483648f || value >= 2147483648f)
            throw new ArgumentOutOfRangeException(nameof(value), "Auxiliary cost exceeds supported finite int32 conversion.");
        return (int)value;
    }
}
