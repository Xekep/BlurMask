using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>
/// Nontracing1f8d60 heap-tail consumer of initialized prefix and explicit
/// native word-row references. It never creates a previous record or context.
/// </summary>
public static partial class LegacyInitialWordHeapTail
{
    public static void Initialize(Span<byte> heapSlot, bool suppressPreviousContext2609,
        Func<int, ushort, ReadOnlyMemory<byte>>? lookupWordRecord = null)
    {
        if (heapSlot.Length != 0x80)
            throw new ArgumentException("Native word heap slot must contain0x80 bytes.");
        BinaryPrimitives.WriteUInt64LittleEndian(heapSlot[0x58..], ulong.MaxValue);
        int previousNode = -1;
        ulong context = 0;
        if (!suppressPreviousContext2609)
        {
            int anchorNode = BinaryPrimitives.ReadInt32LittleEndian(heapSlot[0x38..]);
            ushort anchorArc = BinaryPrimitives.ReadUInt16LittleEndian(heapSlot[0x3c..]);
            ReadOnlySpan<byte> anchor = heapSlot[..0x58];
            if (anchorArc != ushort.MaxValue)
                anchor = ReadRecord(anchorNode, anchorArc).Span;
            previousNode = unchecked(~unchecked((sbyte)anchor[0x20]) + anchorNode);
            if (previousNode >= 0)
            {
                ushort previousArc = BinaryPrimitives.ReadUInt16LittleEndian(anchor[0x1e..]);
                context = BinaryPrimitives.ReadUInt64LittleEndian(ReadRecord(previousNode, previousArc).Span[0x28..]);
            }
        }
        BinaryPrimitives.WriteInt32LittleEndian(heapSlot[0x68..], previousNode);
        BinaryPrimitives.WriteUInt64LittleEndian(heapSlot[0x70..], context);

        ReadOnlyMemory<byte> ReadRecord(int node, ushort arc)
        {
            if (lookupWordRecord is null)
                throw new NotSupportedException("Native anchor/history lookup requires supplied word-row records.");
            var record = lookupWordRecord(node, arc);
            if (record.Length < 0x58)
                throw new ArgumentException("Supplied native word-row record is truncated.");
            return record;
        }
    }
}
