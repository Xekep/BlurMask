using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyContinuationWordRecordFields(
    ulong SourceTag, int CandidateCost10, int IncrementalCost14, ushort State3e,
    ReadOnlyMemory<uint> Payload40, ulong Auxiliary08, ulong Context28,
    byte SmallState21, byte RequestedSecondaryFlag, ulong Auxiliary5c);

/// <summary>
/// Checked nontracing 1f9080: immutable parent row -> bucket router -> allocator
/// -> inherited record prefix and exact continuation/history fields. Parent row
/// selection and runtime search state remain explicit.
/// </summary>
public static class LegacyContinuationWordRecordProducer
{
    public static LegacyNativeWordHeapAllocation TryInsert(LegacyNativeWordHeapRouter router,
        int parentEndingNode, ushort parentArc, Func<int, ushort, ReadOnlyMemory<byte>> lookupWordRecord,
        LegacyContinuationWordRecordFields fields, float secondaryScale, bool suppressPreviousContext2609,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(router);
        ArgumentNullException.ThrowIfNull(lookupWordRecord);
        Validate(fields, secondaryScale);
        cancellationToken.ThrowIfCancellationRequested();
        if (parentEndingNode < 0)
            throw new ArgumentOutOfRangeException(nameof(parentEndingNode));
        var parent = ReadParent(lookupWordRecord(parentEndingNode, parentArc).Span);
        byte effectiveFlag = secondaryScale == 0 ? (byte)0 : fields.RequestedSecondaryFlag;
        // Continuation routing uses the old parent's byte4, before SourceTag
        // replaces the header. Its primary branch does not use 2458 override.
        return router.TryAllocate(parent[4], effectiveFlag, false, fields.CandidateCost10, slot =>
            InitializeAllocatedSlot(slot, parent, parentArc, fields, secondaryScale,
                suppressPreviousContext2609, lookupWordRecord), cancellationToken);
    }

    public static void InitializeAllocatedSlot(Span<byte> slot, ReadOnlySpan<byte> parentRowRecord,
        ushort parentArc, LegacyContinuationWordRecordFields fields, float secondaryScale,
        bool suppressPreviousContext2609, Func<int, ushort, ReadOnlyMemory<byte>>? lookupWordRecord = null,
        CancellationToken cancellationToken = default)
    {
        Validate(fields, secondaryScale);
        cancellationToken.ThrowIfCancellationRequested();
        if (slot.Length != LegacyNativeWordRecordHeap.SlotSize)
            throw new ArgumentException("An allocated native 0x80 slot is required.", nameof(slot));
        if (parentRowRecord.Length != 0x58)
            throw new ArgumentException("An immutable native 0x58 parent row record is required.", nameof(parentRowRecord));

        parentRowRecord.CopyTo(slot);
        slot[0x21] = (byte)((slot[0x21] & 0xc0) | (fields.SmallState21 & 0x3f));
        BinaryPrimitives.WriteUInt64LittleEndian(slot, fields.SourceTag);
        byte effectiveFlag = secondaryScale == 0 ? (byte)0 : fields.RequestedSecondaryFlag;
        slot[0x27] = (byte)((slot[0x27] & 0xfe) | (effectiveFlag & 1));
        BinaryPrimitives.WriteInt32LittleEndian(slot[0x14..], fields.IncrementalCost14);
        BinaryPrimitives.WriteUInt16LittleEndian(slot[0x3e..], fields.State3e);
        BinaryPrimitives.WriteInt32LittleEndian(slot[0x10..], fields.CandidateCost10);
        BinaryPrimitives.WriteUInt16LittleEndian(slot[0x1e..], parentArc);
        slot[0x20] = 0xff;
        for (int index = 0; index < 5; index++)
            BinaryPrimitives.WriteUInt32LittleEndian(slot[(0x40 + index * 4)..], fields.Payload40.Span[index]);
        BinaryPrimitives.WriteUInt64LittleEndian(slot[8..], fields.Auxiliary08);
        BinaryPrimitives.WriteUInt64LittleEndian(slot[0x28..], fields.Context28);
        BinaryPrimitives.WriteUInt64LittleEndian(slot[0x5c..], fields.Auxiliary5c);
        BinaryPrimitives.WriteUInt32LittleEndian(slot[0x58..], uint.MaxValue);

        int previousNode = -1;
        ulong previousContext = 0;
        if (!suppressPreviousContext2609)
        {
            ReadOnlySpan<byte> anchor = slot[..0x58];
            int anchorEndingNode = BinaryPrimitives.ReadInt32LittleEndian(slot[0x38..]);
            ushort anchorArc = BinaryPrimitives.ReadUInt16LittleEndian(slot[0x3c..]);
            if (anchorArc != ushort.MaxValue)
                anchor = Lookup(lookupWordRecord, anchorEndingNode, anchorArc).Span;
            previousNode = unchecked(~(sbyte)anchor[0x20] + anchorEndingNode);
            if (previousNode >= 0)
            {
                ushort previousArc = BinaryPrimitives.ReadUInt16LittleEndian(anchor[0x1e..]);
                previousContext = BinaryPrimitives.ReadUInt64LittleEndian(
                    Lookup(lookupWordRecord, previousNode, previousArc).Span[0x28..]);
            }
        }
        BinaryPrimitives.WriteInt32LittleEndian(slot[0x68..], previousNode);
        BinaryPrimitives.WriteUInt64LittleEndian(slot[0x70..], previousContext);
    }

    private static void Validate(LegacyContinuationWordRecordFields fields, float secondaryScale)
    {
        ArgumentNullException.ThrowIfNull(fields);
        if (!float.IsFinite(secondaryScale))
            throw new ArgumentOutOfRangeException(nameof(secondaryScale));
        if (fields.Payload40.Length != 5)
            throw new ArgumentException("Continuation payload must contain exactly five native dwords.", nameof(fields));
    }

    private static byte[] ReadParent(ReadOnlySpan<byte> parent)
    {
        if (parent.Length != 0x58)
            throw new ArgumentException("Parent lookup must return exactly one native 0x58 row record.");
        return parent.ToArray();
    }

    private static ReadOnlyMemory<byte> Lookup(Func<int, ushort, ReadOnlyMemory<byte>>? lookup,
        int node, ushort arc)
    {
        if (lookup is null)
            throw new NotSupportedException("The exact native anchor/history row lookup is required.");
        var record = lookup(node, arc);
        if (record.Length != 0x58)
            throw new ArgumentException("Anchor/history lookup must return exactly one native 0x58 row record.");
        return record;
    }
}
