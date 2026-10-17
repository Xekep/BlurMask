using System.Buffers.Binary;

namespace InkRecognition.Math;

/// <summary>The numeric037430 completed-hypothesis score with no active
/// constraint model. This prepared kernel does not infer native configuration,
/// execute tensor states or accept recognized math.</summary>
public static class LegacyPreparedMathCompletionScore
{
    /// <summary>Default alpha is1, so native powf returns the positive
    /// float32 normalization base unchanged. Count excludes the final EOS;
    /// cumulative cost includes the candidate's EOS. One million is a portable
    /// work bound, not a recovered native sequence-length setting.</summary>
    public static float WithoutConstraintModel(float cumulativeCost, int prefixSymbolCount,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (!float.IsFinite(cumulativeCost) || cumulativeCost < 0)
            throw new ArgumentOutOfRangeException(nameof(cumulativeCost), "Finite nonnegative prepared cost required.");
        if (prefixSymbolCount < 0 || prefixSymbolCount > 1_000_000)
            throw new ArgumentOutOfRangeException(nameof(prefixSymbolCount), "Bounded prepared prefix count required.");
        float normalizer = prefixSymbolCount + 1;
        normalizer += 5f;
        normalizer /= 6f;
        return cumulativeCost / normalizer;
    }

    /// <summary>Write only the first float in a caller-prepared168-byte
    /// candidate snapshot. Other native record writes and allocation lifetimes
    /// remain the responsibility of their owning stages.</summary>
    public static byte[] WritePreparedRecord(ReadOnlySpan<byte> record168,
        float cumulativeCost, int prefixSymbolCount, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (record168.Length != 168) throw new ArgumentException("Complete prepared168-byte candidate required.");
        float score = WithoutConstraintModel(cumulativeCost, prefixSymbolCount, token);
        byte[] result = record168.ToArray();
        BinaryPrimitives.WriteSingleLittleEndian(result, score);
        return result;
    }
}
