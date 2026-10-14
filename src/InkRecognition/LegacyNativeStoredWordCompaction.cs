using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyNativeStoredWordCompactionResult(ReadOnlyMemory<byte> FullRecordStorage,
    ReadOnlyMemory<byte> FullSecondaryAlternativeStorage, ushort RetainedCount);

/// <summary>Nontracing212b10: literal212900 sort of secondary stored88 rows,
/// adjacent1fce60 dedup, strict lower-cost winner, and0200..0A00 source retention.
/// Every swap also carries the complete count2404×40-byte alternative block.
/// This stage does not choose the final word or synthesize alternative payloads.</summary>
public static class LegacyNativeStoredWordCompaction
{
    public static LegacyNativeStoredWordCompactionResult Prepare(ReadOnlySpan<byte> fullRowStorage,
        ushort activeCount23f0, ushort primaryLimit230a, ushort secondaryCount2404,
        ReadOnlySpan<byte> fullSecondaryAlternatives, LegacyNativeStoredWordComparer comparer,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        cancellationToken.ThrowIfCancellationRequested();
        int capacity = fullRowStorage.Length / 88;
        if (fullRowStorage.Length % 88 != 0 || capacity > ushort.MaxValue || activeCount23f0 > capacity)
            throw new ArgumentException("Actual full-capacity stored88-row extent and active count required.");
        long alternativeExtent = (long)capacity * secondaryCount2404 * 40;
        if (secondaryCount2404 > 1 && ((long)capacity * secondaryCount2404 > int.MaxValue ||
            alternativeExtent > fullSecondaryAlternatives.Length))
            throw new ArgumentException("Complete owned secondary alternative storage must cover the native signed indexing domain.");
        for (int i = primaryLimit230a; i < activeCount23f0; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            comparer.GetHeader(fullRowStorage[i * 88 + 4]);
        }
        byte[] rows = fullRowStorage.ToArray(), alternatives = fullSecondaryAlternatives.ToArray();
        if (activeCount23f0 <= primaryLimit230a)
            return new(rows, alternatives, activeCount23f0);

        QuickSort(primaryLimit230a, activeCount23f0 - 1);
        int leader = primaryLimit230a;
        for (int next = leader + 1; next < activeCount23f0; next++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (comparer.Compare(Row(leader), Row(next)) == 0)
            {
                if (Cost(next) < Cost(leader)) Swap(leader, next);
            }
            else
            {
                leader++;
                Swap(leader, next);
            }
        }
        bool retainedDictionary = false;
        for (int i = primaryLimit230a; i <= leader; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            uint kind = comparer.GetHeader(Row(i)[4]) & 0x1e00;
            if (kind is >= 0x200 and <= 0xa00) { retainedDictionary = true; break; }
        }
        if (!retainedDictionary)
            for (int i = leader + 1; i < activeCount23f0; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                uint kind = comparer.GetHeader(Row(i)[4]) & 0x1e00;
                if (kind is not (>= 0x200 and <= 0xa00)) continue;
                Swap(leader + 1, i);
                leader++;
                break;
            }
        cancellationToken.ThrowIfCancellationRequested();
        return new(rows, alternatives, checked((ushort)(leader + 1)));

        Span<byte> Row(int ordinal) => rows.AsSpan(ordinal * 88, 88);
        int Cost(int ordinal) => BinaryPrimitives.ReadInt32LittleEndian(Row(ordinal)[16..]);

        void Swap(int left, int right)
        {
            if (left == right) return;
            Span<byte> temporary = stackalloc byte[88];
            Row(left).CopyTo(temporary);
            Row(right).CopyTo(Row(left));
            temporary.CopyTo(Row(right));
            if (secondaryCount2404 <= 1) return;
            int block = secondaryCount2404 * 40;
            Span<byte> unit = stackalloc byte[40];
            for (int offset = 0; offset < block; offset += 40)
            {
                var a = alternatives.AsSpan(left * block + offset, 40);
                var b = alternatives.AsSpan(right * block + offset, 40);
                a.CopyTo(unit);
                b.CopyTo(a);
                unit.CopyTo(b);
            }
        }

        void QuickSort(int lower, int upper)
        {
            var partitions = new Stack<(int Lower, int Upper)>();
            partitions.Push((lower, upper));
            while (partitions.Count != 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                (lower, upper) = partitions.Pop();
                int left = lower, right = upper;
                byte[] pivot = Row((upper - lower) / 2 + lower).ToArray();
                do
                {
                    while (comparer.Compare(Row(left), pivot) < 0)
                        if (++left > upper)
                            throw new NotSupportedException("Native stored-row comparison would scan beyond its owned partition.");
                    while (comparer.Compare(Row(right), pivot) > 0)
                        if (--right < lower)
                            throw new NotSupportedException("Native stored-row comparison would scan beyond its owned partition.");
                    if (right < left) break;
                    Swap(left, right);
                    left++;
                    if (right == 0)
                        throw new NotSupportedException("Native ushort right-index would wrap toFFFF; no safe ordering is invented.");
                    right--;
                } while (left <= right);
                // Native lower recursion runs before its upper tail recursion.
                if (left < upper) partitions.Push((left, upper));
                if (lower < right) partitions.Push((lower, right));
            }
        }
    }
}
