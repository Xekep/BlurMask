using System.Buffers.Binary;

namespace InkRecognition;

public sealed class LegacyNativeStoredWordRecord
{
    public ReadOnlyMemory<byte> Record { get; }
    public ReadOnlyMemory<byte> PrimaryAlternatives { get; }
    public ReadOnlyMemory<byte> SecondaryAlternatives { get; }

    internal LegacyNativeStoredWordRecord(byte[] record, byte[] primary, byte[] secondary)
    {
        Record = record;
        PrimaryAlternatives = primary;
        SecondaryAlternatives = secondary;
    }
}

/// <summary>
/// Checked nontracing 213410 heap-slot to stored-row writer. Destination order,
/// optional alternative tables and their runtime counts remain explicit.
/// </summary>
public static class LegacyNativeWordRowMaterializer
{
    public static LegacyNativeStoredWordRecord Materialize(ReadOnlySpan<byte> heapSlot,
        ushort destinationArc, ushort primaryArcLimit230a,
        ushort primaryAlternativeCount2402, ushort secondaryAlternativeCount2404,
        ReadOnlySpan<byte> primaryAlternativeStorage2428 = default,
        ReadOnlySpan<byte> secondaryAlternativeStorage2430 = default,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (heapSlot.Length != LegacyNativeWordRecordHeap.SlotSize)
            throw new ArgumentException("Materialization requires a complete native 0x80 heap slot.", nameof(heapSlot));
        var record = heapSlot[..0x58].ToArray();
        if (BinaryPrimitives.ReadUInt16LittleEndian(record.AsSpan(0x3c)) == ushort.MaxValue)
            BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(0x3c), destinationArc);

        int sourceOrdinal = BinaryPrimitives.ReadInt32LittleEndian(heapSlot[0x58..]);
        byte[] primary = primaryAlternativeCount2402 > 1 && destinationArc < primaryArcLimit230a
            ? CopyAlternatives(primaryAlternativeStorage2428, sourceOrdinal, primaryAlternativeCount2402)
            : [];
        byte[] secondary = secondaryAlternativeCount2404 > 1
            ? CopyAlternatives(secondaryAlternativeStorage2430, sourceOrdinal, secondaryAlternativeCount2404)
            : [];
        return new(record, primary, secondary);
    }

    private static byte[] CopyAlternatives(ReadOnlySpan<byte> source, int ordinal, ushort count)
    {
        if (ordinal < 0)
            throw new ArgumentException("An enabled native alternative table needs the exact nonnegative slot58 ordinal.");
        int size = count * 0x28;
        long offset = (long)ordinal * size;
        // Native does a signed32 multiply before pointer indexing. Reject
        // overflow rather than claiming that a wrapped pointer is meaningful.
        if ((long)ordinal * count > int.MaxValue || offset > source.Length - size)
            throw new ArgumentException("The explicit alternative table does not cover the native ordinal/count block.");
        return source.Slice((int)offset, size).ToArray();
    }
}
