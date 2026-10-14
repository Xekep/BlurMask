namespace InkRecognition;

public readonly record struct LegacyNativeAlternativeWordArcBest(ushort ArcOrdinal,
    int Cost, int Increment, ushort ParentRowOrdinal, byte CallbackFlags);

/// <summary>Regular1fe740's four1024-entry arrays. Arc identity is ordinal;
/// first encounter determines output order, strict signed cost improvement
/// keeps the first parent on ties. No generic sorting or alias deduplication.</summary>
public sealed class LegacyNativeAlternativeWordArcReduction
{
    private readonly List<LegacyNativeAlternativeWordArcBest> slots = [];
    public int Count => slots.Count;
    public bool Full => slots.Count == 1024;

    public int FindOrAdd(ushort arcOrdinal)
    {
        for (int i = 0; i < slots.Count; i++)
            if (slots[i].ArcOrdinal == arcOrdinal) return i;
        if (Full) throw new InvalidOperationException("Native per-query arc table is full.");
        slots.Add(new(arcOrdinal, 0x4000000, 0x4000000, 0, 0));
        return slots.Count - 1;
    }

    public bool TryImprove(int slotIndex, int cost, int increment, ushort parentRowOrdinal,
        byte request3c, byte request3d, bool boundary254c)
    {
        if ((uint)slotIndex >= (uint)slots.Count) throw new ArgumentOutOfRangeException(nameof(slotIndex));
        var old = slots[slotIndex];
        if (cost >= old.Cost) return false;
        byte flags = boundary254c ? (byte)0 : (byte)((request3c != 0 ? 1 : 0) | (request3d != 0 ? 2 : 0));
        slots[slotIndex] = new(old.ArcOrdinal, cost, increment, parentRowOrdinal, flags);
        return true;
    }

    public ReadOnlyMemory<LegacyNativeAlternativeWordArcBest> GetSnapshot() => slots.ToArray();
}
