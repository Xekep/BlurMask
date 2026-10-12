using System.Buffers.Binary;

namespace InkRecognition;

public sealed class LegacyNativeWordRecordOrderingResult
{
    public ReadOnlyMemory<byte> SortedSlotStorage { get; }
    public ReadOnlyMemory<byte> CompactedSlotStorage { get; }
    public int UniqueCount { get; }
    public LegacyNativeWordRecordHeap Heap { get; }

    internal LegacyNativeWordRecordOrderingResult(byte[] sorted, byte[] compacted,
        int uniqueCount, LegacyNativeWordRecordHeap heap)
    {
        SortedSlotStorage = sorted;
        CompactedSlotStorage = compacted;
        UniqueCount = uniqueCount;
        Heap = heap;
    }
}

/// <summary>Native 1fc650 quicksort -> 2144f0 ordinary one-alternative duplicate
/// compaction -> 203de0 rebuild. This uses literal native comparators, not a .NET
/// sort or a substituted global cost order.</summary>
public static class LegacyNativeWordRecordOrdering
{
    public static LegacyNativeWordRecordOrderingResult Prepare(ReadOnlySpan<byte> slotStorage,
        int activeCount, LegacyNativeWordRecordComparer comparer,
        ushort primaryAlternativeCount2402, ushort secondaryAlternativeCount2404,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(comparer);
        cancellationToken.ThrowIfCancellationRequested();
        if (slotStorage.Length % 128 != 0 || slotStorage.Length / 128 > 4096 ||
            (uint)activeCount > (uint)(slotStorage.Length / 128))
            throw new ArgumentException("Explicit native slot storage/count must cover every active 0x80 record.");
        if (primaryAlternativeCount2402 > 1 || secondaryAlternativeCount2404 > 1)
            throw new NotSupportedException("Native multiple-alternative merge writers 1f93a0/1f9830 are not substituted.");

        byte[] working = slotStorage.ToArray();
        QuickSort(working, activeCount, comparer, cancellationToken);
        byte[] sorted = working.ToArray();
        int unique = activeCount == 0 ? 0 : 1;
        for (int index = 1; index < activeCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int leader = unique - 1;
            if (comparer.Compare(Slot(working, leader), Slot(working, index)) == 0)
            {
                if (Cost(Slot(working, index)) < Cost(Slot(working, leader)))
                    Swap(working, leader, index);
            }
            else
            {
                if (unique != index) Slot(working, index).CopyTo(Slot(working, unique));
                unique++;
            }
        }

        // 203de0 snapshots each source slot before reinserting it. Reinsertion
        // targets never exceed that source index, so immutable snapshots are
        // equivalent to its in-place forward walk.
        var heap = new LegacyNativeWordRecordHeap(working.Length / 128, working);
        for (int index = 0; index < unique; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            byte[] source = Slot(working, index).ToArray();
            heap.TryAllocate(Cost(source), slot => source.CopyTo(slot));
        }
        return new(sorted, working, unique, heap);
    }

    private static void QuickSort(byte[] storage, int count, LegacyNativeWordRecordComparer comparer,
        CancellationToken cancellationToken)
    {
        if (count < 2) return;
        var partitions = new Stack<(int Lower, int Upper)>();
        partitions.Push((0, count - 1));
        while (partitions.Count != 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var (lower, upper) = partitions.Pop();
            int left = lower, right = upper;
            byte[] pivot = Slot(storage, (upper - lower) / 2 + lower).ToArray();
            do
            {
                while (comparer.Compare(Slot(storage, left), pivot) < 0)
                {
                    if (++left > upper)
                        throw new NotSupportedException("The native comparator would scan beyond its explicit partition.");
                }
                while (comparer.Compare(Slot(storage, right), pivot) > 0)
                {
                    if (--right < lower)
                        throw new NotSupportedException("The native comparator would scan beyond its explicit partition.");
                }
                if (right < left) break;
                Swap(storage, left, right);
                left++;
                right--;
            } while (left <= right);
            // Native recurses on the lower partition before the upper. An
            // explicit stack retains that order without unbounded C# recursion.
            if (left < upper) partitions.Push((left, upper));
            if (lower < right) partitions.Push((lower, right));
        }
    }

    private static Span<byte> Slot(byte[] storage, int index) => storage.AsSpan(index * 128, 128);
    private static int Cost(ReadOnlySpan<byte> value) => BinaryPrimitives.ReadInt32LittleEndian(value[0x10..]);
    private static void Swap(byte[] storage, int left, int right)
    {
        if (left == right) return;
        Span<byte> temporary = stackalloc byte[128];
        Slot(storage, left).CopyTo(temporary);
        Slot(storage, right).CopyTo(Slot(storage, left));
        temporary.CopyTo(Slot(storage, right));
    }
}
