using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Native 1f73d0 shrinking branch over an immutable heap snapshot.
/// Capacity selection belongs to the 212ec0 controller and remains explicit.</summary>
public static class LegacyNativeWordHeapPruning
{
    public static LegacyNativeWordRecordHeap ReduceCapacity(LegacyNativeWordRecordHeap source,
        int newCapacity, Action<ReadOnlyMemory<byte>>? onDroppedRecord = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        cancellationToken.ThrowIfCancellationRequested();
        if (newCapacity < 0 || newCapacity > source.Count)
            throw new ArgumentOutOfRangeException(nameof(newCapacity),
                "This bounded branch shrinks to at most the active count; allocation/growing storage is separate.");
        var records = Enumerable.Range(0, source.Count).Select(source.GetSlotSnapshot).ToArray();
        var retainedStorage = new byte[newCapacity * 128];
        for (int index = 0; index < newCapacity; index++)
            records[index].Span.CopyTo(retainedStorage.AsSpan(index * 128));
        var target = new LegacyNativeWordRecordHeap(newCapacity, retainedStorage);
        // A physical prefix of a max heap is still a max heap. Appending its
        // slots in native order does not move them, including equal costs.
        for (int index = 0; index < newCapacity; index++)
        {
            var record = records[index];
            int cost = BinaryPrimitives.ReadInt32LittleEndian(record.Span[0x10..]);
            if (!target.TryAllocate(cost, slot => record.Span.CopyTo(slot)))
                throw new InvalidDataException("Existing native heap prefix was not a valid allocated record.");
        }
        for (int index = newCapacity; index < records.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var record = records[index];
            int cost = BinaryPrimitives.ReadInt32LittleEndian(record.Span[0x10..]);
            if (!target.TryAllocate(cost, slot => record.Span.CopyTo(slot), onDroppedRecord))
                onDroppedRecord?.Invoke(record);
        }
        return target;
    }
}
