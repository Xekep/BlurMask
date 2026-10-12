using System.Buffers.Binary;

namespace InkRecognition;

public readonly record struct LegacyNativeWordHeapQuotaLimits(
    uint PrimaryMinimum18, uint PrimaryTransition1c, uint PrimaryInitialCapacity20,
    uint SecondaryMinimum24, uint SecondaryTransition28, uint SecondaryInitialCapacity2c,
    int SourceIndex30)
{
    public static LegacyNativeWordHeapQuotaLimits ReadDescriptorSnapshot(ReadOnlySpan<byte> descriptor)
    {
        if (descriptor.Length < 0x34) throw new ArgumentException("Native quota descriptor must cover through source30.");
        return new(U32(descriptor, 0x18), U32(descriptor, 0x1c), U32(descriptor, 0x20),
            U32(descriptor, 0x24), U32(descriptor, 0x28), U32(descriptor, 0x2c),
            BinaryPrimitives.ReadInt32LittleEndian(descriptor[0x30..]));
    }
    private static uint U32(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadUInt32LittleEndian(value[offset..]);
}

public readonly record struct LegacyNativeWordHeapGrowthRequest(
    ReadOnlyMemory<byte> PreviousStorage, ulong PreviousStorageTag10, int RequiredAllocatedSlots);
public readonly record struct LegacyNativeWordHeapGrowthResult(ReadOnlyMemory<byte> Storage, ulong StorageTag10);
public delegate LegacyNativeWordHeapGrowthResult? LegacyNativeWordHeapGrowthProvider(
    int descriptorIndex, LegacyNativeWordHeapGrowthRequest request);

/// <summary>Owned descriptor storage for 1f73d0/212ec0. Logical capacity may
/// differ from retained backing allocation. New allocation bytes and opaque
/// storage identity come from an explicit successful-growth provider.</summary>
public sealed class LegacyNativeWordHeapDescriptor
{
    private byte[] backing;
    private bool resizing;
    private bool mutating;
    public int Count { get; private set; }
    public int Capacity { get; private set; }
    public int AllocatedCapacity => backing.Length / 128;
    public ulong StorageTag10 { get; private set; }
    public LegacyNativeWordHeapQuotaLimits Limits { get; }
    internal int RootCost => Count != 0 ? ReadCost(0) :
        throw new InvalidOperationException("Empty native heap has no root cost.");

    public LegacyNativeWordHeapDescriptor(ReadOnlySpan<byte> backingStorage, int capacity, int activeCount,
        ulong storageTag10, LegacyNativeWordHeapQuotaLimits limits)
    {
        if (backingStorage.Length % 128 != 0 || backingStorage.Length / 128 > 4096 ||
            capacity < 0 || capacity > backingStorage.Length / 128 || (uint)activeCount > (uint)capacity)
            throw new ArgumentException("Descriptor needs complete explicit backing storage and valid capacity/count.");
        backing = backingStorage.ToArray();
        Capacity = capacity;
        StorageTag10 = storageTag10;
        Limits = limits;
        ValidateHeap(activeCount, backing);
        Count = activeCount;
    }

    public LegacyNativeWordRecordHeap GetHeapSnapshot()
    {
        if (Capacity > AllocatedCapacity)
            throw new InvalidOperationException("Failed native allocation has no complete capacity snapshot.");
        return RestoreHeap(Capacity, Count, backing);
    }

    public ReadOnlyMemory<byte> GetBackingStorageSnapshot()
    {
        return backing.ToArray();
    }

    public bool TryInsertRecord(ReadOnlyMemory<byte> record,
        Action<ReadOnlyMemory<byte>>? onEvictedRoot = null)
    {
        if (resizing) throw new InvalidOperationException("Native descriptor cannot promote into itself during resize.");
        if (Capacity > AllocatedCapacity)
            throw new InvalidOperationException("Failed native allocation left an incomplete descriptor; search must stop.");
        if (record.Length != 128) throw new ArgumentException("Promotion needs a complete native record.");
        return AllocateRecord(record, onEvictedRoot);
    }

    public bool Resize(uint newCapacity, int descriptorIndex, LegacyNativeWordHeapGrowthProvider? growthProvider,
        Action<ReadOnlyMemory<byte>>? onDroppedRecord = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (newCapacity > 4096) throw new NotSupportedException("Native capacity exceeds this checked owned-storage subset.");
        if (resizing || mutating) throw new InvalidOperationException("Descriptor resize is not reentrant.");
        resizing = true;
        try
        {
            int oldCount = Count;
            Capacity = (int)newCapacity; // Native writes descriptor+4 before allocator failure.
            if (AllocatedCapacity < Capacity)
            {
                if (growthProvider is null)
                    throw new NotSupportedException("Native growth needs explicit allocator bytes and storage identity.");
                var grown = growthProvider(descriptorIndex, new(backing.ToArray(), StorageTag10, Capacity));
                if (grown is null) return false;
                if (grown.Value.Storage.Length != Capacity * 128 ||
                    !grown.Value.Storage.Span[..backing.Length].SequenceEqual(backing))
                    throw new ArgumentException("Native realloc provider must preserve old backing bytes and supply the entire new allocation.");
                backing = grown.Value.Storage.ToArray();
                StorageTag10 = grown.Value.StorageTag10;
            }
            Count = Math.Min(oldCount, Capacity);
            for (int index = Capacity; index < oldCount; index++)
            {
                // Tail source slots are beyond the new capacity, so prefix
                // movement cannot change their live bytes during reinsertion.
                var record = backing.AsMemory(index * 128, 128);
                if (!AllocateRecord(record, onDroppedRecord))
                    onDroppedRecord?.Invoke(record.ToArray());
            }
            return true;
        }
        finally { resizing = false; }
    }

    private bool AllocateRecord(ReadOnlyMemory<byte> record, Action<ReadOnlyMemory<byte>>? onEvictedRoot)
    {
        if (mutating) throw new InvalidOperationException("Native descriptor allocation is not reentrant.");
        int cost = Cost(record.Span);
        if (Capacity == 0 || cost > 0x3ffffff) return false;
        mutating = true;
        try
        {
            int target;
            if (Count == Capacity)
            {
                if (ReadCost(0) <= cost) return false;
                onEvictedRoot?.Invoke(backing.AsMemory(0, 128).ToArray());
                target = 0;
                while (target * 2 < Count - 1)
                {
                    int child = target * 2 + 1;
                    if (child < Count - 1 && ReadCost(child) <= ReadCost(child + 1)) child++;
                    if (ReadCost(child) <= cost) break;
                    CopySlot(child, target);
                    target = child;
                }
            }
            else
            {
                target = Count++;
                while (target != 0)
                {
                    int parent = (target - 1) / 2;
                    if (cost <= ReadCost(parent)) break;
                    CopySlot(parent, target);
                    target = parent;
                }
            }
            var slot = backing.AsSpan(target * 128, 128);
            slot[..0x78].Clear();
            // Every caller supplies a full existing native record, including
            // its accepted cost and opaque78..7f bytes.
            record.Span.CopyTo(slot);
            return true;
        }
        finally { mutating = false; }
    }

    private int ReadCost(int index) => Cost(backing.AsSpan(index * 128, 128));
    private void CopySlot(int source, int target) => backing.AsSpan(source * 128, 128).CopyTo(backing.AsSpan(target * 128));

    private static LegacyNativeWordRecordHeap RestoreHeap(int capacity, int count, ReadOnlySpan<byte> storage)
    {
        var result = new LegacyNativeWordRecordHeap(capacity, storage[..(capacity * 128)]);
        for (int index = 0; index < count; index++)
        {
            var record = storage.Slice(index * 128, 128).ToArray();
            if (index != 0 && Cost(record) > Cost(storage.Slice(((index - 1) / 2) * 128, 128)))
                throw new ArgumentException("Explicit active storage must already be a native signed-cost max heap.");
            if (!result.TryAllocate(Cost(record), slot => record.CopyTo(slot)))
                throw new ArgumentException("Explicit active storage contains an unallocated native cost.");
        }
        return result;
    }

    private static void ValidateHeap(int count, ReadOnlySpan<byte> storage)
    {
        for (int index = 0; index < count; index++)
        {
            int cost = Cost(storage.Slice(index * 128, 128));
            if (cost > 0x3ffffff || index != 0 && cost > Cost(storage.Slice(((index - 1) / 2) * 128, 128)))
                throw new ArgumentException("Explicit active storage must already be a native allocated signed-cost max heap.");
        }
    }

    private static int Cost(ReadOnlySpan<byte> value) => BinaryPrimitives.ReadInt32LittleEndian(value[0x10..]);
}
