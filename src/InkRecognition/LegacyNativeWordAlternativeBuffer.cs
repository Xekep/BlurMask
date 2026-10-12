using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Owned 0x28 alternative-record pool and literal nontracing 1f93a0 /
/// 1f9830 duplicate merges. Opaque U64 identity8 is compared, never dereferenced.
/// Alternative creation/scoring, trace clones and controller quotas are separate.</summary>
public sealed class LegacyNativeWordAlternativeBuffer
{
    private readonly record struct Selection(int Row, int Index, ulong Identity);
    private readonly byte[] storage;
    public ushort AlternativeCount { get; }
    public int RowCount { get; }

    public LegacyNativeWordAlternativeBuffer(ushort alternativeCount, ReadOnlySpan<byte> storage)
    {
        // A bounded subset of the native stack-selection vector; no claim that
        // 16 is the SDK's configurable maximum. Fresh20baa0 count2 is supported.
        if (alternativeCount is < 1 or > 16)
            throw new ArgumentOutOfRangeException(nameof(alternativeCount));
        int rowSize = alternativeCount * 0x28;
        if (storage.Length % rowSize != 0)
            throw new ArgumentException("Alternative storage must cover complete native count*0x28 rows.");
        AlternativeCount = alternativeCount;
        RowCount = storage.Length / rowSize;
        this.storage = storage.ToArray();
    }

    public ReadOnlyMemory<byte> GetSnapshot() => storage.ToArray();

    /// <summary>Primary1f93a0: writes merged alternatives into the leader's
    /// original pool row and leaves both 128-byte heap records unchanged.</summary>
    public int MergePrimary(ReadOnlySpan<byte> leaderSlot, ReadOnlySpan<byte> duplicateSlot,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int leader = Ordinal(leaderSlot), duplicate = Ordinal(duplicateSlot);
        var first = Alternative(leader, 0);
        var second = Alternative(duplicate, 0);
        bool compareHeapCosts = (first[0x1f] & second[0x1f] & 0x20) != 0;
        int firstCost = compareHeapCosts ? I32(leaderSlot, 0x10) : I32(first, 0);
        int secondCost = compareHeapCosts ? I32(duplicateSlot, 0x10) : I32(second, 0);
        // Primary seed ties prefer the duplicate row, as native.
        Span<Selection> selected = stackalloc Selection[16];
        int count = Select(leader, duplicate, firstCost < secondCost, selected);
        CopySelections(leader, selected[..count], primary: true);
        return count;
    }

    /// <summary>Secondary1f9830: first-alt cost can swap the complete heap slots;
    /// merges into that winning ordinal and updates the leader from its best alt.</summary>
    public int MergeSecondary(Span<byte> leaderSlot, Span<byte> duplicateSlot,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int leader = Ordinal(leaderSlot), duplicate = Ordinal(duplicateSlot);
        if (I32(Alternative(duplicate, 0), 0) < I32(Alternative(leader, 0), 0))
        {
            Span<byte> temporary = stackalloc byte[128];
            leaderSlot.CopyTo(temporary);
            duplicateSlot.CopyTo(leaderSlot);
            temporary.CopyTo(duplicateSlot);
            (leader, duplicate) = (duplicate, leader);
        }
        // Secondary seed ties retain the leader. Remaining merge ties prefer
        // the duplicate's entry, just like the primary cursor comparison.
        Span<Selection> selected = stackalloc Selection[16];
        int count = Select(leader, duplicate, leaderFirst: true, selected);
        CopySelections(leader, selected[..count], primary: false);
        var best = Alternative(leader, 0);
        BinaryPrimitives.WriteInt32LittleEndian(leaderSlot[0x10..], I32(best, 0));
        BinaryPrimitives.WriteInt32LittleEndian(leaderSlot[0x14..], I32(best, 4));
        leaderSlot[0x26] = best[0x1e];
        BinaryPrimitives.WriteUInt16LittleEndian(leaderSlot[0x1e..], U16(best, 0x18));
        leaderSlot[0x20] = best[0x1a];
        leaderSlot[0x21] = (byte)((leaderSlot[0x21] & 0xc0) | (best[0x1f] & 0x1f));
        BinaryPrimitives.WriteUInt16LittleEndian(leaderSlot[0x3e..], U16(best, 0x20));
        return count;
    }

    private int Select(int leader, int duplicate, bool leaderFirst, Span<Selection> selected)
    {
        int left = leaderFirst ? 1 : 0, right = leaderFirst ? 0 : 1;
        int firstRow = leaderFirst ? leader : duplicate;
        selected[0] = new(firstRow, 0, U64(Alternative(firstRow, 0), 8));
        int count = 1;
        while (count < AlternativeCount)
        {
            bool leftAvailable = left < AlternativeCount && I32(Alternative(leader, left), 0) < 0x4000000;
            bool rightAvailable = right < AlternativeCount && I32(Alternative(duplicate, right), 0) < 0x4000000;
            if (!leftAvailable && !rightAvailable) break;
            bool chooseLeft = leftAvailable && (!rightAvailable ||
                I32(Alternative(leader, left), 0) < I32(Alternative(duplicate, right), 0));
            int row = chooseLeft ? leader : duplicate;
            int index = chooseLeft ? left++ : right++;
            ulong identity = U64(Alternative(row, index), 8);
            bool exists = false;
            for (int previous = 0; previous < count; previous++)
                if (selected[previous].Identity == identity) { exists = true; break; }
            if (!exists) selected[count++] = new(row, index, identity);
        }
        return count;
    }

    private void CopySelections(int destinationRow, ReadOnlySpan<Selection> selected, bool primary)
    {
        // Native copies in descending destination order from live source
        // entries. Do not snapshot all selected entries: with repeated IDs
        // inside an input row, an earlier write can replace a later source.
        for (int index = selected.Length - 1; index >= 0; index--)
        {
            var source = selected[index];
            if (source.Row == destinationRow && source.Index == index) continue;
            var input = Alternative(source.Row, source.Index);
            var output = Alternative(destinationRow, index);
            input[..8].CopyTo(output);
            output[0x1e] = input[0x1e];
            input.Slice(0x18, 4).CopyTo(output[0x18..]);
            input.Slice(8, 16).CopyTo(output[8..]);
            byte mask = primary ? (byte)0x3f : (byte)0x1f;
            output[0x1f] = (byte)((output[0x1f] & ~mask) | (input[0x1f] & mask));
            if (!primary) input.Slice(0x20, 4).CopyTo(output[0x20..]);
        }
    }

    private int Ordinal(ReadOnlySpan<byte> slot)
    {
        if (slot.Length != 128) throw new ArgumentException("Merge requires complete native 0x80 heap slots.");
        int ordinal = I32(slot, 0x58);
        if ((uint)ordinal >= (uint)RowCount)
            throw new ArgumentException("Slot58 ordinal does not address the explicit alternative pool.");
        return ordinal;
    }

    private Span<byte> Alternative(int row, int index) => storage.AsSpan((row * AlternativeCount + index) * 0x28, 0x28);
    private static int I32(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadInt32LittleEndian(value[offset..]);
    private static ushort U16(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(value[offset..]);
    private static ulong U64(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(value[offset..]);
}
