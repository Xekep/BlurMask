using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>2144F0's nontracing periodic cost10 shift over the explicit source
///extent. The current minimum and source-backward bound are separate owning
///controller producers. Row14 and all allocation bytes outside10 survive.</summary>
public static class LegacyNativeWordPeriodicCostNormalization
{
    public static ReadOnlyMemory<byte> ApplySourceRows(ReadOnlySpan<byte> records, int sourceEnd23f0,
        int positiveMinimum, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (records.Length % 88 != 0 || records.Length / 88 > ushort.MaxValue ||
            (uint)sourceEnd23f0 > records.Length / 88 || positiveMinimum is < 1 or >= 0x4000000)
            throw new ArgumentException("Complete owned source rows and the native positive finite minimum required.");
        byte[] output = records.ToArray();
        for (int ordinal = 0; ordinal < sourceEnd23f0; ordinal++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var row = output.AsSpan(ordinal * 88, 88);
            int cost = BinaryPrimitives.ReadInt32LittleEndian(row[16..]);
            if (cost < 0x4000000)
                BinaryPrimitives.WriteInt32LittleEndian(row[16..], unchecked(cost - positiveMinimum));
        }
        return output;
    }

    /// <summary>Literal inclusive backward loop: current node plus at most
    ///frontendA0 previous nodes. Values before the returned range are retained.</summary>
    public static ReadOnlyMemory<int> AddCumulative(ReadOnlySpan<int> preparedPath2520,
        int currentEndingNode, int actualBackwardExtentA0, int positiveMinimum,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if ((uint)currentEndingNode >= preparedPath2520.Length || preparedPath2520.Length > 1_000_000 ||
            actualBackwardExtentA0 < 0 || positiveMinimum is < 1 or >= 0x4000000)
            throw new ArgumentException("Owned current cumulative vector, actual backward extent and native minimum required.");
        int[] output = preparedPath2520.ToArray();
        int first = Math.Max(0, currentEndingNode - actualBackwardExtentA0);
        for (int node = currentEndingNode; node >= first; node--)
        {
            cancellationToken.ThrowIfCancellationRequested();
            output[node] = unchecked(output[node] + positiveMinimum);
        }
        return output;
    }
}
