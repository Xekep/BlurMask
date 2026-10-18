using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMathMeanXOrder.Probe")]

namespace InkRecognition.Math;

/// <summary>Literal Ink01ED50 ordering of sixteen-byte mean-X records.
/// Equal keys are stable only in insertion-sort partitions of at most20.
/// Opaque payloads are test/storage values, not manufactured native pointers.</summary>
internal static class LegacyMath2MeanXOrder
{
    internal readonly record struct Entry(uint OriginalIndex, uint MeanBits, ulong Opaque)
    {
        internal float Mean => BitConverter.UInt32BitsToSingle(MeanBits);
    }

    internal static int[] CopySourceOrder(ReadOnlySpan<float> means, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (means.Length > MathPreprocessor.MaxStrokes) throw new ArgumentOutOfRangeException(nameof(means));
        var rows = new Entry[means.Length];
        for (int i = 0; i < rows.Length; i++)
            rows[i] = new((uint)i, BitConverter.SingleToUInt32Bits(means[i]), 0);
        Sort(rows, 0, rows.Length - 1, token);
        return rows.Select(row => checked((int)row.OriginalIndex)).ToArray();
    }

    // Prepared complete-record controls may include every float bit pattern.
    // Actual source means are separately bounded/validated by preprocessing.
    internal static Entry[] SortPrepared(ReadOnlySpan<Entry> input, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (input.Length > MathPreprocessor.MaxStrokes) throw new ArgumentOutOfRangeException(nameof(input));
        Entry[] rows = input.ToArray();
        Sort(rows, 0, rows.Length - 1, token);
        return rows;
    }

    private static void Sort(Entry[] rows, int first, int last, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (last - first < 20)
        {
            for (int at = first + 1; at <= last; at++)
            {
                token.ThrowIfCancellationRequested();
                Entry value = rows[at]; int prior = at - 1;
                while (prior >= first && rows[prior].Mean > value.Mean)
                { rows[prior + 1] = rows[prior]; prior--; }
                rows[prior + 1] = value;
            }
            return;
        }

        // Inclusive endpoints and the copied middle key follow01EDCD..01EDEC.
        float pivot = rows[first + (last - first) / 2].Mean;
        int left = first, right = last;
        do
        {
            token.ThrowIfCancellationRequested();
            while (pivot > rows[left].Mean) left++;
            while (rows[right].Mean > pivot) right--;
            if (right < left) break;
            (rows[left], rows[right]) = (rows[right], rows[left]);
            left++; right--;
        } while (left <= right);
        if (first < right) Sort(rows, first, right, token);
        if (left < last) Sort(rows, left, last, token);
    }
}
