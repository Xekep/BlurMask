using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Explicit inputs for2276A4..227750. TotalCost already includes
/// the terminal's cumulative2520 offset; this API does not select that terminal.</summary>
public readonly record struct LegacyMainResultScoreInput(int TotalCost, int ReferenceCost,
    int SourceNodeCount, int SelectedRowCost14, int SourceKind);

/// <summary>Separate main alternative channels. Mass08 is unnormalised and
/// is not handwriting acceptance confidence.</summary>
public readonly record struct LegacyMainResultScoreValue(int IntegerDifference,
    float NormalizedPathCost10, float NormalizedRowCost14, float LogMass, float Mass08);

public static class LegacyMainResultScore
{
    /// <summary>Exact main signed32 SUB-before-float and SHL13 arithmetic.
    /// A positive explicit source count is required by the native nonempty
    /// branch. Native integer wrap and floating zero-divisor outcomes are
    /// preserved; source-node ownership remains the caller's responsibility.</summary>
    public static LegacyMainResultScoreValue Evaluate(LegacyMainResultScoreInput input,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (input.SourceNodeCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(input), "The native nonempty source branch requires a positive count.");
        int difference = unchecked(input.TotalCost - input.ReferenceCost);
        int divisor = unchecked(input.SourceNodeCount << 13);
        float normalized = (float)difference / (float)divisor;
        float secondary = (float)input.SelectedRowCost14 * 0.00012207031f;
        float multiplier = input.SourceKind == 0x135 ? 5f : 1f;
        float logMass = -multiplier * normalized;
        float mass = logMass < -75f ? 0f : logMass > 75f ? 3.73e32f : (float)Math.Exp((double)logMass);
        cancellationToken.ThrowIfCancellationRequested();
        return new(difference, normalized, secondary, logMass, mass);
    }

    /// <summary>Write only alternative08/10/14 into one explicit complete
    ///40-byte row. Constructor fields, row0C and source/WORD identities remain
    /// untouched; this is not273.V8 allocation or an owning result builder.</summary>
    public static byte[] WriteCostChannels(ReadOnlySpan<byte> actualRow40, LegacyMainResultScoreInput input,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (actualRow40.Length != 40) throw new ArgumentException("One complete main alternative40 row required.", nameof(actualRow40));
        var value = Evaluate(input, cancellationToken);
        byte[] result = actualRow40.ToArray();
        Write(result, 8, value.Mass08);
        Write(result, 0x10, value.NormalizedPathCost10);
        Write(result, 0x14, value.NormalizedRowCost14);
        cancellationToken.ThrowIfCancellationRequested();
        return result;
    }

    private static void Write(Span<byte> storage, int at, float value)
        => BinaryPrimitives.WriteInt32LittleEndian(storage[at..], BitConverter.SingleToInt32Bits(value));
}
