using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Final scalar writes of Text1f42d0 into the newly appended273
/// forty-byte alternatives. Requires the actual post-materialization rows;
/// this does not allocate glyphs or produce recognition confidence.</summary>
public static class LegacyFirstResultScorePublication
{
    public static byte[] Publish(ReadOnlySpan<byte> alternatives, int existingCount,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (alternatives.Length % 40 != 0 || alternatives.Length / 40 > 4096 ||
            existingCount < 0 || existingCount > alternatives.Length / 40)
            throw new ArgumentException("Bounded complete273 alternative rows and their actual prior count required.");
        byte[] result = alternatives.ToArray();
        int count = result.Length / 40;
        if (existingCount == count) return result; //Native leaves all fields untouched if no new result survived.

        float anchor = Read(result, existingCount, 8);
        if (existingCount + 1 < count)
        {
            float denominator = 0;
            //Native walks backwards; xmm7 retains the final, immediately following pair's denominator.
            for (int row = count - 1; row > existingCount; row--)
            {
                cancellationToken.ThrowIfCancellationRequested();
                float mass = Read(result, row, 8);
                denominator = mass + anchor;
                Write(result, row, 8, mass / denominator);
            }
            Write(result, existingCount, 8, anchor / denominator);
        }
        else Write(result, existingCount, 8, 1f);

        if (count == 1) Write(result, 0, 12, 1f);
        else
        {
            float minimum = float.MaxValue;
            for (int row = existingCount; row < count; row++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                float next = Read(result, row, 20);
                //MINSS chooses its source for unordered values and equal signed zeros.
                if (float.IsNaN(minimum) || float.IsNaN(next) || next <= minimum) minimum = next;
            }
            for (int row = existingCount; row < count; row++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                float difference = Read(result, row, 20) - minimum;
                Write(result, row, 12, MathF.Exp(-difference)); //25c840 is CRT expf, without clipping.
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        return result;
    }

    private static float Read(byte[] storage, int row, int offset) =>
        BitConverter.Int32BitsToSingle(BinaryPrimitives.ReadInt32LittleEndian(storage.AsSpan(row * 40 + offset)));
    private static void Write(byte[] storage, int row, int offset, float value) =>
        BinaryPrimitives.WriteInt32LittleEndian(storage.AsSpan(row * 40 + offset), BitConverter.SingleToInt32Bits(value));
}
