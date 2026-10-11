using System.Buffers.Binary;

namespace InkRecognition;

public delegate void LegacyNativeWordSlotInitializer(Span<byte> slot);

/// <summary>
/// Checked1f75b0 signed-cost max heap over owned0x80 slots. Allocation zeroes
/// bytes0..77, preserves78..7f, and returns a slot to the explicit initializer.
/// Native language/bucket routing, root promotion and winner policy are separate.
/// </summary>
public sealed class LegacyNativeWordRecordHeap
{
    public const int SlotSize = 0x80;
    private readonly byte[] slots;
    private bool mutating;
    public int Capacity { get; }
    public int Count { get; private set; }

    public LegacyNativeWordRecordHeap(int capacity, ReadOnlySpan<byte> initialSlotStorage = default)
    {
        if (capacity is < 0 or > 4096)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        if (!initialSlotStorage.IsEmpty && initialSlotStorage.Length != capacity * SlotSize)
            throw new ArgumentException("Explicit initial storage must cover every native0x80 slot.");
        Capacity = capacity;
        slots = initialSlotStorage.IsEmpty ? new byte[capacity * SlotSize] : initialSlotStorage.ToArray();
    }

    public ReadOnlyMemory<byte> GetSlotSnapshot(int slotIndex)
    {
        if ((uint)slotIndex >= (uint)Count)
            throw new ArgumentOutOfRangeException(nameof(slotIndex));
        return slots.AsSpan(slotIndex * SlotSize, SlotSize).ToArray();
    }

    /// <summary>
    /// A full heap rejects equal/worse cost; equal children prefer the right
    /// child. An optional eviction callback runs before movement, as in native.
    /// The initializer must retain the accepted cost at10. Callback failure can
    /// leave an allocated/partially initialized slot; no transactional rollback
    /// is claimed. Reentrant mutation of this heap is rejected.
    /// </summary>
    public bool TryAllocate(int candidateCost, LegacyNativeWordSlotInitializer initializeSlot,
        Action<ReadOnlyMemory<byte>>? onEvictedRoot = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(initializeSlot); cancellationToken.ThrowIfCancellationRequested();
        if (mutating) throw new InvalidOperationException("Native word heap mutation is not reentrant.");
        if (Capacity == 0 || candidateCost > 0x3ffffff) return false;
        mutating = true;
        try
        {
            int target;
            if (Count == Capacity)
            {
                if (ReadCost(0) <= candidateCost) return false;
                onEvictedRoot?.Invoke(GetSlotSnapshot(0));
                target = 0;
                while (target * 2 < Count - 1)
                {
                    int child = target * 2 + 1;
                    if (child < Count - 1 && ReadCost(child) <= ReadCost(child + 1)) child++;
                    if (ReadCost(child) <= candidateCost) break;
                    CopySlot(child, target); target = child;
                }
            }
            else
            {
                target = Count++;
                while (target != 0)
                {
                    int parent = (target - 1) / 2;
                    if (candidateCost <= ReadCost(parent)) break;
                    CopySlot(parent, target); target = parent;
                }
            }
            var slot = slots.AsSpan(target * SlotSize, SlotSize);
            slot[..0x78].Clear();
            BinaryPrimitives.WriteInt32LittleEndian(slot[0x10..], candidateCost);
            initializeSlot(slot);
            if (ReadCost(target) != candidateCost)
                throw new InvalidOperationException("Word initializer changed the accepted heap cost.");
            return true;
        }
        finally { mutating = false; }
    }

    /// <summary>Exact allocator to checked initial prefix writer; tail initialization remains explicit.</summary>
    public bool TryInsertInitialPrefix(LegacyInitialWordRecordFields fields, float secondaryScale,
        LegacyNativeWordSlotInitializer? initializeTail = null,
        Action<ReadOnlyMemory<byte>>? onEvictedRoot = null, CancellationToken cancellationToken = default)
    {
        var prefix = LegacyInitialWordRecordProducer.InitializeAllocatedPrefix(fields, secondaryScale, cancellationToken);
        return TryAllocate(fields.CandidateCost10, slot =>
        {
            prefix.CopyTo(slot);
            initializeTail?.Invoke(slot[0x58..]);
        }, onEvictedRoot, cancellationToken);
    }

    /// <summary>Checked nontracing allocator ->initial prefix ->anchor/history tail.</summary>
    public bool TryInsertInitialRecord(LegacyInitialWordRecordFields fields, float secondaryScale,
        bool suppressPreviousContext2609,
        Func<int, ushort, ReadOnlyMemory<byte>>? lookupWordRecord = null,
        Action<ReadOnlyMemory<byte>>? onEvictedRoot = null, CancellationToken cancellationToken = default)
    {
        var prefix = LegacyInitialWordRecordProducer.InitializeAllocatedPrefix(fields, secondaryScale, cancellationToken);
        return TryAllocate(fields.CandidateCost10, slot =>
        {
            prefix.CopyTo(slot);
            LegacyInitialWordHeapTail.Initialize(slot, suppressPreviousContext2609, lookupWordRecord);
        }, onEvictedRoot, cancellationToken);
    }

    private int ReadCost(int index) => BinaryPrimitives.ReadInt32LittleEndian(slots.AsSpan(index * SlotSize + 0x10));
    private void CopySlot(int source, int target) =>
        slots.AsSpan(source * SlotSize, SlotSize).CopyTo(slots.AsSpan(target * SlotSize, SlotSize));
}
