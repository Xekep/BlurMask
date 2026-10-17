using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Deep-owned scalar publication result. No TOP273/WORD/source
/// ownership or acceptance confidence is inferred from these channels.</summary>
public sealed class LegacyMainPublishedResultScores
{
    private readonly byte[] storage;
    public int ActiveCount { get; }
    public float? RawFirstMass08 { get; }
    public float Top80 { get; }
    internal LegacyMainPublishedResultScores(byte[] storage, int count, float? rawFirst, float top80)
    { this.storage = storage; ActiveCount = count; RawFirstMass08 = rawFirst; Top80 = top80; }
    public byte[] CopyStorage() => storage.ToArray();
}

/// <summary>273.V60/13CBA0 followed by227CCC..227D57. This normalizes an
/// explicitly supplied whole alternative prefix; it does not sort, append,
/// select or calibrate words.</summary>
public static class LegacyMainResultScorePublication
{
    public static LegacyMainPublishedResultScores Normalize(ReadOnlySpan<byte> fullStorage40,
        int activeCount, float actualPreviousTop80, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (fullStorage40.Length % 40 != 0 || fullStorage40.Length / 40 > 4096 ||
            activeCount < 0 || activeCount > fullStorage40.Length / 40)
            throw new ArgumentException("Bounded complete alternative storage and its actual active count required.");
        byte[] result = fullStorage40.ToArray();
        if (activeCount == 0) return new(result, 0, null, actualPreviousTop80);
        float rawFirst = Read(result, 0, 8);
        if (activeCount == 1) Write(result, 0, 8, 1f);
        else
        {
            float denominator = 0;
            for (int row = activeCount - 1; row > 0; row--)
            {
                cancellationToken.ThrowIfCancellationRequested();
                float mass = Read(result, row, 8);
                denominator = mass + rawFirst;
                Write(result, row, 8, mass / denominator);
            }
            Write(result, 0, 8, rawFirst / denominator);
        }
        if (activeCount == 1) Write(result, 0, 12, 1f);
        else
        {
            float minimum = float.MaxValue;
            for (int row = 0; row < activeCount; row++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                minimum = MinimumSource(minimum, Read(result, row, 20));
            }
            for (int row = 0; row < activeCount; row++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                float difference = Read(result, row, 20) - minimum;
                Write(result, row, 12, MathF.Exp(-difference)); //Native CRT expf, no clipping.
            }
        }
        //COMISS1,raw/JBE takes the divide branch for unordered raw values.
        //Save rawFirst BEFORE normalizing row08; singleton08=1 is not this input.
        float ceiling = rawFirst < 1f ? rawFirst * .9f : 1f - .1f / rawFirst;
        float top80 = MinimumSource(actualPreviousTop80, ceiling);
        cancellationToken.ThrowIfCancellationRequested();
        return new(result, activeCount, rawFirst, top80);
    }

    //MINSS returns its second operand for unordered comparisons and equal
    //signed zeros. Math.Min would change the native NaN/zero selection rule.
    private static float MinimumSource(float current, float next)
        => float.IsNaN(current) || float.IsNaN(next) || next <= current ? next : current;
    private static float Read(ReadOnlySpan<byte> storage, int row, int offset)
        => BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(storage[(row * 40 + offset)..]));
    private static void Write(Span<byte> storage, int row, int offset, float value)
        => BinaryPrimitives.WriteInt32LittleEndian(storage[(row * 40 + offset)..], BitConverter.SingleToInt32Bits(value));
}
