using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Explicit 210720 fallback initializer fields. Offsets preserve the
/// native layout without assigning unproved word-policy meanings.</summary>
public readonly record struct LegacyNativeRejectionFallbackFields(int Node08, int Node0c, int Node10,
    ushort Value14, ushort Value16, byte Value1e);

/// <summary>Explicit scalar arguments of the ordinary 210870 36-byte append.
/// Collection ancestry/cost production is not replaced by this writer.</summary>
public readonly record struct LegacyNativePreparedRejectionFields(int Cost00, int Correction04,
    int StartNode08, int EndNode0c, int PreviousNode10, ushort GroupKey14,
    ushort EndArc16, ushort DistinctKey18, ushort PreviousArc1a, ushort TraceArc1c,
    byte SourceAlternative1e, byte TargetAlternative1f);

public static class LegacyNativePreparedRejectionProducer
{
    public static byte[] Create(ReadOnlySpan<byte> stackResidue, LegacyNativePreparedRejectionFields fields,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (stackResidue.Length != 36) throw new ArgumentException("Actual 36-byte stack residue required.");
        var record = stackResidue.ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(record, fields.Cost00);
        BinaryPrimitives.WriteInt32LittleEndian(record.AsSpan(4), fields.Correction04);
        BinaryPrimitives.WriteInt32LittleEndian(record.AsSpan(8), fields.StartNode08);
        BinaryPrimitives.WriteInt32LittleEndian(record.AsSpan(12), fields.EndNode0c);
        BinaryPrimitives.WriteInt32LittleEndian(record.AsSpan(16), fields.PreviousNode10);
        BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(20), fields.GroupKey14);
        BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(22), fields.EndArc16);
        BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(24), fields.DistinctKey18);
        BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(26), fields.PreviousArc1a);
        BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(28), fields.TraceArc1c);
        record[30] = fields.SourceAlternative1e;
        record[31] = fields.TargetAlternative1f;
        record[32] = 0;
        return record;
    }
}

/// <summary>Literal 36-byte 210720 fallback writer over caller-owned stack
/// residue. Collection/buffer reallocation remains a separate producer.</summary>
public static class LegacyNativeRejectionFallbackProducer
{
    public static byte[] Create(ReadOnlySpan<byte> stackResidue, ushort primaryArcLimit230a,
        LegacyNativeRejectionFallbackFields fields, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (stackResidue.Length != 36) throw new ArgumentException("Actual 36-byte stack residue required.");
        var record = stackResidue.ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(record, 0x4000000);
        BinaryPrimitives.WriteInt32LittleEndian(record.AsSpan(4), 0x4000000);
        BinaryPrimitives.WriteInt32LittleEndian(record.AsSpan(8), fields.Node08);
        BinaryPrimitives.WriteInt32LittleEndian(record.AsSpan(12), fields.Node0c);
        BinaryPrimitives.WriteInt32LittleEndian(record.AsSpan(16), fields.Node10);
        BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(20), fields.Value14);
        BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(22), fields.Value16);
        BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(24), primaryArcLimit230a);
        BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(26), ushort.MaxValue);
        BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(28), primaryArcLimit230a);
        record[30] = fields.Value1e;
        record[31] = 0;
        record[32] = 0; // Native keeps stack-derived bytes33..35 untouched.
        return record;
    }
}

/// <summary>Literal 210540 / 210570 / 211210 sorting and marking of complete
/// 36-byte rejection candidates. Selection is not candidate collection or final
/// source mutation; caller owns the actual 36-byte records and initial flags.</summary>
public static class LegacyNativeRejectionCandidateSelection
{
    public static int Compare(ReadOnlySpan<byte> left, ReadOnlySpan<byte> right)
    {
        if (left.Length != 36 || right.Length != 36) throw new ArgumentException("Complete36-byte records required.");
        int leftNode = I32(left, 8), rightNode = I32(right, 8);
        if (leftNode != rightNode) return leftNode < rightNode ? -1 : 1;
        ushort leftValue = U16(left, 20), rightValue = U16(right, 20);
        if (leftValue != rightValue) return leftValue < rightValue ? -1 : 1;
        return unchecked(I32(left, 0) - I32(right, 0));
    }

    public static byte[] Prepare(ReadOnlySpan<byte> storage, int activeCount, ushort primaryCount2402,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (storage.Length % 36 != 0 || storage.Length / 36 > 65536 ||
            (uint)activeCount > (uint)(storage.Length / 36) || primaryCount2402 > 20)
            throw new ArgumentException("Bounded complete records and native 20-slot selection vector required.");
        var records = storage.ToArray();
        if (activeCount == 0) return records;
        var partitions = new Stack<(int Left, int Right)>();
        partitions.Push((0, activeCount - 1));
        Span<byte> pivot = stackalloc byte[36], temporary = stackalloc byte[36];
        while (partitions.Count != 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var (left, right) = partitions.Pop();
            int i = left, j = right;
            Slot(records, left + (right - left) / 2).CopyTo(pivot);
            while (i <= j)
            {
                while (Compare(Slot(records, i), pivot) < 0)
                {
                    if (++i > right) throw new NotSupportedException("Native wrapped comparator scans outside the partition.");
                }
                while (Compare(Slot(records, j), pivot) > 0)
                {
                    if (--j < left) throw new NotSupportedException("Native wrapped comparator scans outside the partition.");
                }
                if (i > j) break;
                Slot(records, i).CopyTo(temporary);
                Slot(records, j).CopyTo(Slot(records, i));
                temporary.CopyTo(Slot(records, j));
                i++; j--;
            }
            // LIFO reproduces native lower partition recursion first.
            if (i < right) partitions.Push((i, right));
            if (left < j) partitions.Push((left, j));
        }
        Span<ushort> selected = stackalloc ushort[20];
        for (int groupStart = 0; groupStart < activeCount;)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int groupEnd = groupStart + 1;
            while (groupEnd < activeCount && I32(Slot(records, groupEnd), 8) == I32(Slot(records, groupStart), 8) &&
                U16(Slot(records, groupEnd), 20) == U16(Slot(records, groupStart), 20)) groupEnd++;
            selected.Fill(ushort.MaxValue);
            int selectedCount = 0;
            for (int index = groupStart; index < groupEnd && selectedCount < primaryCount2402; index++)
            {
                ushort key = U16(Slot(records, index), 24);
                bool duplicate = false;
                for (int k = 0; k < selectedCount; k++) if (selected[k] == key) { duplicate = true; break; }
                if (!duplicate) selected[selectedCount++] = key;
            }
            for (int index = groupStart; index < groupEnd; index++)
            {
                var record = Slot(records, index);
                for (int k = 0; k < primaryCount2402 && selected[k] != ushort.MaxValue; k++)
                    if (U16(record, 24) == selected[k] && I32(record, 0) <= 0x3ffffff)
                    { record[32] = 1; break; }
                // Native only sets qualifying flags, never clears prior flags.
            }
            groupStart = groupEnd;
        }
        cancellationToken.ThrowIfCancellationRequested();
        return records;
    }
    private static Span<byte> Slot(byte[] records, int index) => records.AsSpan(index * 36, 36);
    private static int I32(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadInt32LittleEndian(data[offset..]);
    private static ushort U16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
}
