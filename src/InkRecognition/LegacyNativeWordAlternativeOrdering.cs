using System.Buffers.Binary;

namespace InkRecognition;

public sealed class LegacyNativeWordAlternativeOrderingResult
{
    public ReadOnlyMemory<byte> SortedSlotStorage { get; }
    public ReadOnlyMemory<byte> CompactedSlotStorage { get; }
    public int UniqueCount { get; }
    public LegacyNativeWordRecordHeap Heap { get; }
    public ReadOnlyMemory<byte> PrimaryAlternativeStorage { get; }
    public ReadOnlyMemory<byte> SecondaryAlternativeStorage { get; }

    internal LegacyNativeWordAlternativeOrderingResult(byte[] sorted, byte[] compacted,
        int unique, LegacyNativeWordRecordHeap heap, ReadOnlyMemory<byte> primary, ReadOnlyMemory<byte> secondary)
    {
        SortedSlotStorage = sorted;
        CompactedSlotStorage = compacted;
        UniqueCount = unique;
        Heap = heap;
        PrimaryAlternativeStorage = primary;
        SecondaryAlternativeStorage = secondary;
    }
}

/// <summary>Nontracing 2144f0 ordering with native primary and secondary
/// alternative pools; default2/2 is read from the explicit prepared controller
/// configuration, not replaced by the single-alternative diagnostic branch.</summary>
public static class LegacyNativeWordAlternativeOrdering
{
    public static LegacyNativeWordAlternativeOrderingResult Prepare(ReadOnlySpan<byte> slotStorage,
        int activeCount, bool continuation, LegacyNativeWordControllerConfiguration configuration,
        ReadOnlySpan<uint> sourceDescriptorHeaders22e0, bool frontend280Flag,
        LegacyNativeWordAlternativeBuffer? primaryPool, LegacyNativeWordAlternativeBuffer? secondaryPool,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (configuration.Trace2368 != 0)
            throw new NotSupportedException("Native trace clones are separate from the nontracing alternative pool.");
        if (slotStorage.Length % 128 != 0 || slotStorage.Length / 128 > 4096 ||
            (uint)activeCount > (uint)(slotStorage.Length / 128))
            throw new ArgumentException("Explicit slot storage must cover all active native records.");
        CheckPool(configuration.PrimaryAlternativeCount2402, primaryPool);
        CheckPool(configuration.SecondaryAlternativeCount2404, secondaryPool);
        if (configuration.PrimaryAlternativeCount2402 > 1 && configuration.SecondaryAlternativeCount2404 > 1 &&
            ReferenceEquals(primaryPool, secondaryPool))
            throw new ArgumentException("Native primary and secondary pools require separate owned storage.");
        var comparer = new LegacyNativeWordRecordComparer(LegacyNativeWordRecordComparer.Select(continuation,
            configuration.Mode22f6 != 0, (byte)(configuration.PackedSource22e8 & 0xff), frontend280Flag),
            sourceDescriptorHeaders22e0);
        var working = slotStorage.ToArray();
        QuickSort(working, activeCount, comparer, cancellationToken);
        var sorted = working.ToArray();
        int unique = activeCount == 0 ? 0 : 1;
        for (int index = 1; index < activeCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int leader = unique - 1;
            if (comparer.Compare(Slot(working, leader), Slot(working, index)) == 0)
            {
                if (configuration.SecondaryAlternativeCount2404 > 1)
                    secondaryPool!.MergeSecondary(Slot(working, leader), Slot(working, index));
                else if (Cost(Slot(working, index)) < Cost(Slot(working, leader)))
                    Swap(working, leader, index);
                if (configuration.PrimaryAlternativeCount2402 > 1)
                    primaryPool!.MergePrimary(Slot(working, leader), Slot(working, index));
            }
            else
            {
                if (unique != index) Slot(working, index).CopyTo(Slot(working, unique));
                unique++;
            }
        }
        var heap = new LegacyNativeWordRecordHeap(working.Length / 128, working);
        for (int index = 0; index < unique; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var record = Slot(working, index).ToArray();
            heap.TryAllocate(Cost(record), slot => record.CopyTo(slot));
        }
        return new(sorted, working, unique, heap, primaryPool?.GetSnapshot() ?? ReadOnlyMemory<byte>.Empty,
            secondaryPool?.GetSnapshot() ?? ReadOnlyMemory<byte>.Empty);
    }

    private static void CheckPool(ushort count, LegacyNativeWordAlternativeBuffer? pool)
    {
        if (count > 1 && (pool is null || pool.AlternativeCount != count))
            throw new ArgumentException("Enabled native alternative count requires its matching explicit pool.");
    }

    // Same literal1fc650 partition/swap order as the frozen one-alternative
    // implementation. Kept separate here to avoid changing that released API.
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
            var pivot = Slot(storage, (upper - lower) / 2 + lower).ToArray();
            do
            {
                while (comparer.Compare(Slot(storage, left), pivot) < 0)
                    if (++left > upper) throw new NotSupportedException("Native comparator scans outside the explicit partition.");
                while (comparer.Compare(Slot(storage, right), pivot) > 0)
                    if (--right < lower) throw new NotSupportedException("Native comparator scans outside the explicit partition.");
                if (right < left) break;
                Swap(storage, left, right);
                left++;
                right--;
            } while (left <= right);
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
