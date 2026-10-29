using System.Buffers.Binary;

namespace InkRecognition;

public readonly record struct LegacyNativeWordHeapAllocation(
    bool Accepted, int HeapIndex, bool PromotionAttempted, bool PromotionAccepted);

/// <summary>
/// The 1f8c90 bucket decision and 1f8b70 whole-record promotion, over explicit
/// live descriptor buckets and heaps. Heap capacity and descriptor construction
/// are inputs; this is not the language controller or final word selection.
/// </summary>
public sealed partial class LegacyNativeWordHeapRouter
{
    private readonly LegacyNativeWordRecordHeap[] heaps;
    private readonly byte[] sourceStateBuckets;
    private readonly int overrideState22f0;
    private readonly uint overrideHeap2458;

    public LegacyNativeWordHeapRouter(IReadOnlyList<LegacyNativeWordRecordHeap> heaps,
        ReadOnlySpan<byte> sourceStateBuckets, int overrideState22f0, uint overrideHeap2458)
    {
        ArgumentNullException.ThrowIfNull(heaps);
        if (heaps.Count == 0)
            throw new ArgumentException("An explicit native bucket array is required.", nameof(heaps));
        this.heaps = heaps.ToArray();
        for (int index = 0; index < this.heaps.Length; index++)
        {
            ArgumentNullException.ThrowIfNull(this.heaps[index]);
            for (int previous = 0; previous < index; previous++)
                if (ReferenceEquals(this.heaps[index], this.heaps[previous]))
                    throw new ArgumentException("Native heap descriptors must own distinct slot storage.", nameof(heaps));
        }
        this.sourceStateBuckets = sourceStateBuckets.ToArray();
        this.overrideState22f0 = overrideState22f0;
        this.overrideHeap2458 = overrideHeap2458;
    }

    public LegacyNativeWordHeapAllocation TryAllocate(int sourceStateIndex,
        byte requestedSecondaryFlag, bool useOverride, int candidateCost,
        LegacyNativeWordSlotInitializer initializeSlot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(initializeSlot);
        cancellationToken.ThrowIfCancellationRequested();
        if ((uint)sourceStateIndex >= (uint)sourceStateBuckets.Length)
            throw new ArgumentOutOfRangeException(nameof(sourceStateIndex));

        int primaryIndex = sourceStateBuckets[sourceStateIndex];
        int targetIndex = primaryIndex;
        bool promote = false;
        if (requestedSecondaryFlag == 0)
        {
            if (useOverride && overrideHeap2458 != uint.MaxValue && sourceStateIndex == overrideState22f0)
                targetIndex = (byte)overrideHeap2458;
        }
        else
        {
            // Native adds after the shift as uint, not as a byte.
            targetIndex = (heaps.Length >> 1) + primaryIndex;
            var secondary = GetHeap(targetIndex);
            if (secondary.Count == secondary.Capacity && secondary.Capacity != 0)
            {
                int rootCost = BinaryPrimitives.ReadInt32LittleEndian(secondary.GetSlotSnapshot(0).Span[0x10..]);
                if (rootCost <= candidateCost)
                    targetIndex = primaryIndex;
                else
                    promote = true;
            }
        }

        var target = GetHeap(targetIndex);
        var primary = promote ? GetHeap(primaryIndex) : null;
        if (primary is not null && ReferenceEquals(primary, target))
            throw new NotSupportedException("Native promotion requires separate primary and secondary heaps.");
        bool attempted = false, promoted = false;
        Action<ReadOnlyMemory<byte>>? onEvicted = promote ? root =>
        {
            attempted = true;
            int cost = BinaryPrimitives.ReadInt32LittleEndian(root.Span[0x10..]);
            // Promotion runs before the secondary heap moves its root. The
            // allocator owns the snapshot; every byte, including 78..7f, copies.
            promoted = primary!.TryAllocate(cost, slot => root.Span.CopyTo(slot));
        } : null;
        bool accepted = target.TryAllocate(candidateCost, initializeSlot, onEvicted, cancellationToken);
        return new(accepted, targetIndex, attempted, promoted);
    }

    /// <summary>Actual primary bucket owner for source22e0's state index.
    /// This excludes optional2458 override and secondary routing; no heap is
    /// constructed or replaced. Language204290 reads this owner's live limit.</summary>
    public LegacyNativeWordRecordHeap GetPrimaryHeap(int sourceStateIndex)
    {
        if ((uint)sourceStateIndex >= (uint)sourceStateBuckets.Length)
            throw new ArgumentOutOfRangeException(nameof(sourceStateIndex));
        return GetHeap(sourceStateBuckets[sourceStateIndex]);
    }

    /// <summary>
    /// Initial writer -> live bucket routing -> allocator -> nontracing tail.
    /// A fallback to the primary heap retains the effective secondary flag in
    /// the record, just as 1f8d60. Explicit scale zero forces that flag to zero.
    /// </summary>
    public LegacyNativeWordHeapAllocation TryInsertInitialRecord(int sourceStateIndex, bool useOverride,
        LegacyInitialWordRecordFields fields, float secondaryScale, bool suppressPreviousContext2609,
        Func<int, ushort, ReadOnlyMemory<byte>>? lookupWordRecord = null,
        CancellationToken cancellationToken = default)
    {
        var prefix = LegacyInitialWordRecordProducer.InitializeAllocatedPrefix(fields, secondaryScale, cancellationToken);
        byte effectiveFlag = secondaryScale == 0 ? (byte)0 : fields.RequestedSecondaryFlag;
        return TryAllocate(sourceStateIndex, effectiveFlag, useOverride, fields.CandidateCost10, slot =>
        {
            prefix.CopyTo(slot);
            LegacyInitialWordHeapTail.Initialize(slot, suppressPreviousContext2609, lookupWordRecord);
        }, cancellationToken);
    }

    private LegacyNativeWordRecordHeap GetHeap(int index)
    {
        if ((uint)index >= (uint)heaps.Length)
            throw new ArgumentException("The live bucket/override index exceeds the supplied native heap array.");
        return heaps[index];
    }
}
