using System.Buffers.Binary;
using System.Text;

namespace InkRecognition;

public readonly record struct LegacyFst28CInitialAuxiliarySlot(int Slot, ushort Key, bool HasHistoryValue, bool HasSecondaryValue);
public readonly record struct LegacyFst28CInitialCursor(int Slot, int LogicalOffset, int State, int Context0, int Context1,
    int Count, int PhysicalOffset, int AcceptedOffset);
public readonly record struct LegacyFst28CInitialRootRow(uint Target, ushort SourceLabel, ushort Primary, ushort Secondary,
    int Cost, sbyte TargetSignedByte6, byte TargetByte7, int? AuxiliarySlot);

/// <summary>Owning constructor-only initial prefix for the authenticated number28C
/// prerequisite. Not a query, paging API, history consumer or configured source.</summary>
public sealed class LegacyFst28CInitialState
{
    // Complete protocol/logical domains, NOT CLR object overhead or RSS:
    // envelope2580 +15(key16+two payload64)=270 +16 association32=64
    // +8(cursor80+12*(target32+label16+cost32+mapped16))=1792
    // +98 degree32=392 +decodedName56+UTF16capacity112 +rawrootrow16 +temporary keys30.
    public const int LogicalByteEnvelope = 5312;
    public const int WorkEnvelope = LogicalByteEnvelope + 99 + 299 + 16 * 16 + 8 + 98 + 56 + 1;
    private sealed class AuxiliarySlot(object cohort, ushort key)
    {
        internal readonly object Cohort = cohort;
        internal readonly ushort Key = key;
        internal readonly object? HistoryValue = null, SecondaryValue = null;
    }
    private sealed class Cursor(int slot, int capacity)
    {
        internal readonly uint[] Targets = new uint[capacity];
        internal readonly ushort[] Labels = new ushort[capacity], Mapped = new ushort[capacity];
        internal readonly int[] Costs = new int[capacity];
        internal readonly LegacyFst28CInitialCursor Scalar = new(slot, 0xc8 + slot * 0x50,
            slot == 0 ? 0 : -1, -1, -1, slot == 0 ? 1 : 0, slot == 0 ? 0 : -1, slot == 0 ? 0 : -1);
    }
    private readonly object cohort;
    private readonly AuxiliarySlot[] slots;
    private readonly int[] labelSlots, degrees;
    private readonly Cursor[] cursors;
    private readonly LegacyFst13AOutgoingRow rootOrigin;
    public LegacyFst28CTargetPreparation TargetPreparation { get; }
    public string DecodedSourceName { get; }
    public int Capacity => 12;
    public int NextCursor => 0;
    public string InitialCallback => "180240010";
    public int InitialAlternative => -1;
    private LegacyFst28CInitialState(LegacyFst28CTargetPreparation target, object cohort, AuxiliarySlot[] slots,
        int[] labelSlots, int[] degrees, Cursor[] cursors, LegacyFst13AOutgoingRow rootOrigin, string name)
    {
        TargetPreparation = target; this.cohort = cohort; this.slots = slots; this.labelSlots = labelSlots;
        this.degrees = degrees; this.cursors = cursors; this.rootOrigin = rootOrigin; DecodedSourceName = name;
    }
    public static LegacyFst28CInitialState CreateActualRussianNumberFresh(LegacyFst28CTargetPreparation target,
        int logicalByteBudget = LogicalByteEnvelope, int workBudget = WorkEnvelope, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(target);
        if (logicalByteBudget < 0 || logicalByteBudget > LogicalByteEnvelope) throw new ArgumentOutOfRangeException(nameof(logicalByteBudget));
        if (workBudget < 0 || workBudget > WorkEnvelope) throw new ArgumentOutOfRangeException(nameof(workBudget));
        if (logicalByteBudget < LogicalByteEnvelope || workBudget < WorkEnvelope)
            throw new InvalidDataException("Complete logical initialization envelope exceeds budget.");
        var source = target.PrimaryMap.Source; var root = source.SourceRoot; var child = source.SourceChild;
        if (!source.HasActualFreshMain135Authority || source.ActualAcousticSourceKind != 0x135 || root is null || child is null ||
            child.Ordinal != 5 || child.Offset != 299000 || child.End != 301580 || child.Kind != 0x13a || child.Version != 0x202 ||
            !ReferenceEquals(child, root.Children[5]) || source.StateCount != 98)
            throw new ArgumentException("Exact retained number target/source owner required.", nameof(target));
        var envelope = root.CopyChildEnvelope(child); token.ThrowIfCancellationRequested();
        ushort U16(int at) => BinaryPrimitives.ReadUInt16LittleEndian(envelope.AsSpan(at));
        uint U32(int at) => BinaryPrimitives.ReadUInt32LittleEndian(envelope.AsSpan(at));
        if (envelope.Length != 2580 || U16(16) != 0x301 || U16(18) != 0x100 || U32(80) != uint.MaxValue ||
            U32(96) != 0x101 || U16(576) != 0x109 || U16(578) != 16 || U32(580) != 98 || U32(584) != 299 || U32(1380) != 12)
            throw new InvalidDataException("Actual source prefix/layout differs.");
        var degrees = new int[98]; uint previous = U32(588);
        if (previous != 0) throw new InvalidDataException("Source first transition differs.");
        for (int i = 0; i < 98; i++)
        {
            token.ThrowIfCancellationRequested(); uint next = U32(588 + (i + 1) * 8);
            if (next < previous || next > 299 || next - previous > 12) throw new InvalidDataException("Whole graph exceeds cursor capacity.");
            degrees[i] = (int)(next - previous); previous = next;
        }
        if (previous != 299 || degrees[0] != 1) throw new InvalidDataException("Actual initial graph extent differs.");
        for (int i = 0; i < 299; i++)
        { token.ThrowIfCancellationRequested(); if (U32(1384 + i * 4) >= 98) throw new InvalidDataException("Transition escapes actual source."); }
        Span<byte> nameBytes = stackalloc byte[56]; ReadOnlySpan<byte> key = [0x14, 0x12, 0x19, 0x77];
        for (int i = 0; i < 56; i++) { token.ThrowIfCancellationRequested(); nameBytes[i] = (byte)(envelope[24 + i] ^ key[i % 4]); }
        int end = nameBytes.IndexOf((byte)0); if (end < 0) throw new InvalidDataException("Actual name terminator differs.");
        for (int i = 0; i < end; i++) if (nameBytes[i] > 127) throw new InvalidDataException("Only actual ASCII name is admitted.");
        string name = Encoding.ASCII.GetString(nameBytes[..end]);
        if (name != "ru_RU-lk-text.number") throw new InvalidDataException("Actual decoded source name differs.");
        // This is a logical key->stable slot adaptation of fresh0918D0, not a
        // recovered AVL/pointer allocation ABI. Both fresh payloads remain NULL.
        object cohort = new(); var slots = new List<AuxiliarySlot>(15); var keys = new List<ushort>(15); var labelSlots = new int[16];
        for (ushort label = 0; label < 16; label++)
        {
            token.ThrowIfCancellationRequested(); ushort code = target.PrimaryMap.MapLabel(label).Code; labelSlots[label] = -1;
            if (code is ushort.MaxValue or 0xfffd) continue;
            int found = FindOrInsertKey(keys, code);
            if (found == slots.Count) slots.Add(new(cohort, code));
            labelSlots[label] = found;
        }
        if (slots.Count != 15 || labelSlots[0] != -1) throw new InvalidDataException("Actual nonempty auxiliary slot domain differs.");
        // The query is the already owned13A primitive, NOT a28C query. Root rows
        // are populated only after whole graph/budget checks; no extra NN runs.
        var rootRows = source.Query(0, 1, 16, token);
        if (rootRows.Count != 1) throw new InvalidDataException("Actual state0 degree differs.");
        var origin = rootRows.GetRow(0);
        if (origin.TargetState != 97 || origin.SourceLabel != 0 ||
            target.PrimaryMap.MapLabel(0).Code != 0xfffd || target.MapSecondary(0).Code != 0xfffd)
            throw new InvalidDataException("Actual state0 row differs.");
        var cursors = new Cursor[8];
        for (int i = 0; i < cursors.Length; i++) { token.ThrowIfCancellationRequested(); cursors[i] = new(i, 12); }
        cursors[0].Targets[0] = origin.TargetState; cursors[0].Labels[0] = origin.SourceLabel;
        cursors[0].Costs[0] = origin.Cost; cursors[0].Mapped[0] = target.PrimaryMap.MapLabel(origin.SourceLabel).Code;
        token.ThrowIfCancellationRequested(); return new(target, cohort, slots.ToArray(), labelSlots, degrees, cursors, origin, name);
    }
    private static int FindOrInsertKey(List<ushort> keys, ushort key)
    {
        if (keys.Count > 15) throw new ArgumentException("Bounded fresh auxiliary domain required.");
        for (int i = 0; i < keys.Count; i++) if (keys[i] == key) return i;
        if (keys.Count == 15) throw new InvalidDataException("Fresh auxiliary domain exceeds15 slots.");
        int slot = keys.Count; keys.Add(key); return slot;
    }
    public LegacyFst28CInitialAuxiliarySlot[] CopyAuxiliarySlots(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested(); var copy = new LegacyFst28CInitialAuxiliarySlot[slots.Length];
        for (int i = 0; i < copy.Length; i++)
        { if (!ReferenceEquals(slots[i].Cohort, cohort)) throw new InvalidOperationException("Auxiliary cohort differs."); copy[i] = new(i, slots[i].Key, slots[i].HistoryValue is not null, slots[i].SecondaryValue is not null); }
        return copy;
    }
    public int? ReadAuxiliarySlot(ushort sourceLabel)
    { if (sourceLabel >= 16) throw new ArgumentOutOfRangeException(nameof(sourceLabel)); return labelSlots[sourceLabel] < 0 ? null : labelSlots[sourceLabel]; }
    public int[] CopyDegrees(CancellationToken token = default) { token.ThrowIfCancellationRequested(); return degrees.ToArray(); }
    public LegacyFst28CInitialCursor ReadCursor(int index)
    { if ((uint)index >= 8) throw new ArgumentOutOfRangeException(nameof(index)); return cursors[index].Scalar; }
    /// <summary>Only the single initialized row is observable. Unused native
    /// allocation bytes and later paging/history state are not represented.</summary>
    public LegacyFst28CInitialRootRow ReadInitialRootRow() => new(cursors[0].Targets[0], cursors[0].Labels[0], cursors[0].Mapped[0],
        TargetPreparation.MapSecondary(rootOrigin.SourceLabel).Code, cursors[0].Costs[0], rootOrigin.TargetSignedByte6,
        rootOrigin.TargetByte7, ReadAuxiliarySlot(rootOrigin.SourceLabel));
}
