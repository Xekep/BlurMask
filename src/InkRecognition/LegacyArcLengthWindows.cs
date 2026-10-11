namespace InkRecognition;

/// <summary>Owned point memberships; empty trailing windows are explicit.</summary>
public sealed class LegacyArcLengthWindowSet
{
    private readonly byte[][] _windows;
    public int Count => _windows.Length;
    public ReadOnlyMemory<byte> this[int index] => _windows[index];
    internal LegacyArcLengthWindowSet(byte[][] windows) { _windows = windows; }
}

/// <summary>
/// Restores the checked temporal-window membership pass on caller-prepared
/// cumulative physical arc lengths. Distances across pen-up gaps must not be
/// included. Coordinate preparation and distance calculation are separate.
/// </summary>
public static class LegacyArcLengthWindows
{
    public static LegacyArcLengthWindowSet Partition(ReadOnlySpan<int> cumulativePhysicalLengths,
        int windowCount = 16, CancellationToken cancellationToken = default)
    {
        if (cumulativePhysicalLengths.Length is < 1 or > 255)
            throw new ArgumentException("The checked byte-indexed pass requires 1..255 prepared points.", nameof(cumulativePhysicalLengths));
        if (windowCount is < 1 or > 64) throw new ArgumentOutOfRangeException(nameof(windowCount));
        if (cumulativePhysicalLengths[0] != 0) throw new ArgumentException("Physical cumulative lengths must start at zero.", nameof(cumulativePhysicalLengths));
        cancellationToken.ThrowIfCancellationRequested();
        for (int i = 1; i < cumulativePhysicalLengths.Length; i++)
            if (cumulativePhysicalLengths[i] < cumulativePhysicalLengths[i - 1])
                throw new ArgumentException("Physical cumulative lengths must be nonnegative and nondecreasing.", nameof(cumulativePhysicalLengths));
        int total = cumulativePhysicalLengths[^1];
        // Reject the native overflow domain instead of emitting negative strides.
        if (total > int.MaxValue >> 10) throw new ArgumentException("The exponent-10 cumulative length exceeds the checked safe domain.", nameof(cumulativePhysicalLengths));
        var windows = new byte[windowCount][];
        Array.Fill(windows, Array.Empty<byte>());
        int points = cumulativePhysicalLengths.Length;
        if (points <= windowCount)
        {
            for (int i = 0; i < points; i++) windows[i] = [(byte)i];
        }
        else
        {
            int step = (total << 10) / windowCount, boundary = step + 512;
            int start = 0, window = 0;
            for (int i = 1; i < points; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                // Native is strict > and advances only once per input point,
                // even when a large segment crosses several ideal thresholds.
                if (cumulativePhysicalLengths[i] > (boundary >> 10))
                {
                    if (window >= windowCount - 1) throw new ArgumentException("Prepared lengths exceed the checked window partition domain.", nameof(cumulativePhysicalLengths));
                    windows[window++] = Range(start, i); start = i;
                    boundary += step;
                }
            }
            windows[window] = Range(start, points);
        }
        return new(windows);
    }
    private static byte[] Range(int start, int end)
    {
        var result = new byte[end - start];
        for (int i = 0; i < result.Length; i++) result[i] = (byte)(start + i);
        return result;
    }
}
