using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace InkRecognition;

public readonly record struct LegacyFst13AOutgoingRow(uint TargetState, ushort SourceLabel,
    int Cost, sbyte TargetSignedByte6, byte TargetByte7);

/// <summary>Detached ordered V80 result. No alphabet conversion or lexical acceptance.</summary>
public sealed class LegacyFst13AOutgoingResult
{
    private readonly LegacyFst13AOutgoingRow[] rows;
    public uint State { get; }
    public int OutputFlag => 0;
    public int Count => rows.Length;
    internal LegacyFst13AOutgoingResult(uint state, LegacyFst13AOutgoingRow[] rows)
    { State = state; this.rows = rows; }
    public LegacyFst13AOutgoingRow GetRow(int index)
    {
        if ((uint)index >= (uint)rows.Length) throw new ArgumentOutOfRangeException(nameof(index));
        return rows[index];
    }
    public LegacyFst13AOutgoingRow[] CopyRows(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        var result = new LegacyFst13AOutgoingRow[rows.Length];
        for (int i = 0; i < rows.Length; i++)
        { if ((i & 1023) == 0) token.ThrowIfCancellationRequested(); result[i] = rows[i]; }
        token.ThrowIfCancellationRequested(); return result;
    }
}

/// <summary>Owning immutable13A.V80 adjacency primitive. Not28C, a recognizer,
/// automatic source selection, or a RawField20 initial-state interpretation.
/// No disposal: the immutable wire owner and copied cost table live with this source.</summary>
public sealed class LegacyFst13AOutgoingSource
{
    public const int MaximumValidationWork = LegacyFst23CWireLayout.MaximumStates + 1 + LegacyFst23CWireLayout.MaximumTransitionWords;
    public const int MaximumQueryRowBytes = LegacyFst23CWireLayout.MaximumTransitionWords * 16;
    private const string AcousticHash = "7A5F08809E81FB4A2D4C78B3EFD251D0986503B0D30D21DE3695DAB1F8A89978";
    private const string RootHash = "D26ED08BAF419361484958B3A04A5F740A228F4664F7E2643F43459223EF3E4E";
    private readonly LegacyFst23CWireLayout wire;
    private readonly int[]? costs;
    public LegacyLinguisticRoot13FWireLayout? SourceRoot { get; }
    public LegacyLinguisticRoot13FWireChild? SourceChild { get; }
    public bool HasActualFreshMain135Authority => SourceRoot is not null;
    public int? ActualAcousticSourceKind => HasActualFreshMain135Authority ? 0x135 : null;
    public string? ActualAcousticFingerprint => HasActualFreshMain135Authority ? AcousticHash : null;
    public int StateCount => wire.StateCount;
    public int TransitionWordCount => wire.TransitionWordCount;
    public string WireFingerprint => wire.Fingerprint;
    public bool HasCostTable => costs is not null;
    private LegacyFst13AOutgoingSource(LegacyFst23CWireLayout wire, int[]? costs,
        LegacyLinguisticRoot13FWireLayout? root, LegacyLinguisticRoot13FWireChild? child)
    { this.wire = wire; this.costs = costs; SourceRoot = root; SourceChild = child; }

    /// <summary>Only the pinned number13A and acoustic135, with fresh native
    /// constructor defaults and no property mutations. This does not select the source.</summary>
    public static LegacyFst13AOutgoingSource CreateActualRussianNumber(
        LegacyLinguisticRoot13FWireLayout root, LegacyFst23CWireLayout wire,
        ReadOnlySpan<byte> acousticResource, int validationBudget = MaximumValidationWork,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(root);
        ArgumentNullException.ThrowIfNull(wire);
        if (root.Fingerprint != RootHash || !ReferenceEquals(wire.SourceRoot, root) ||
            wire.SourceChild is not { Ordinal: 5, Offset: 299000, End: 301580, Kind: 0x13a, Version: 0x202 } child ||
            !ReferenceEquals(child, root.Children[5]) || wire.SourceOffset != 299576 || wire.SourceEnd != 301580)
            throw new ArgumentException("Exact retained actual number root/child/wire identity required.");
        ValidateGraph(wire, validationBudget, token);
        if (acousticResource.Length != 572763)
            throw new ArgumentException("Exact acoustic resource required.", nameof(acousticResource));
        // Caller memory can change. Hash and read kind from the SAME bounded
        // private snapshot; do not retain the full acoustic buffer afterwards.
        var acoustic = new byte[572763];
        for (int at = 0; at < acoustic.Length; at += 65536)
        {
            token.ThrowIfCancellationRequested();
            acousticResource.Slice(at, Math.Min(65536, acoustic.Length - at)).CopyTo(acoustic.AsSpan(at));
        }
        token.ThrowIfCancellationRequested();
        if (Convert.ToHexString(SHA256.HashData(acoustic)) != AcousticHash)
            throw new ArgumentException("Exact acoustic resource required.", nameof(acousticResource));
        if (BinaryPrimitives.ReadUInt32LittleEndian(acoustic.AsSpan(5233)) != 0x135)
            throw new InvalidDataException("Actual acoustic source kind differs.");
        var envelope = root.CopyChildEnvelope(child);
        if (BinaryPrimitives.ReadUInt32LittleEndian(envelope.AsSpan(100)) != 0x3f800000 || wire.LabelCount != 16)
            throw new InvalidDataException("Actual serialized source weight/alphabet differs.");
        var table = FreshMain135Costs(token);
        token.ThrowIfCancellationRequested(); return new(wire, table, root, child);
    }

    /// <summary>Caller-prepared diagnostic graph/table. Empty table is the
    /// native null-cost branch. Never acquires actual source authority.</summary>
    public static LegacyFst13AOutgoingSource CreateDiagnostic(LegacyFst23CWireLayout wire,
        ReadOnlySpan<int> table, int validationBudget = MaximumValidationWork,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(wire);
        if (table.Length is not (0 or 256)) throw new ArgumentException("Empty or complete signed-byte table required.", nameof(table));
        ValidateBudget(wire, validationBudget);
        int[]? owned = table.IsEmpty ? null : table.ToArray();
        ValidateGraph(wire, validationBudget, token);
        token.ThrowIfCancellationRequested(); return new(wire, owned, null, null);
    }

    private static void ValidateGraph(LegacyFst23CWireLayout wire, int budget, CancellationToken token)
    {
        ValidateBudget(wire, budget);
        uint previous = 0;
        for (int i = 0; i < wire.DescriptorCount; i++)
        {
            if ((i & 1023) == 0) token.ThrowIfCancellationRequested();
            uint offset = wire.GetDescriptor(i).RawField0;
            if (offset < previous || offset > (uint)wire.TransitionWordCount)
                throw new InvalidDataException("Nonmonotonic or out-of-range graph offsets.");
            previous = offset;
        }
        for (int i = 0; i < wire.TransitionWordCount; i++)
        {
            if ((i & 1023) == 0) token.ThrowIfCancellationRequested();
            if (wire.GetTransitionWord(i) >= (uint)wire.StateCount)
                throw new InvalidDataException("Target outside complete graph state domain.");
        }
        token.ThrowIfCancellationRequested();
    }
    private static void ValidateBudget(LegacyFst23CWireLayout wire, int budget)
    {
        if (budget < 0 || budget > MaximumValidationWork) throw new ArgumentOutOfRangeException(nameof(budget));
        long work = (long)wire.DescriptorCount + wire.TransitionWordCount;
        if (work > budget) throw new InvalidDataException("Whole graph work exceeds admission budget.");
    }

    public LegacyFst13AOutgoingResult Query(uint state,
        int rowBudget = LegacyFst23CWireLayout.MaximumTransitionWords,
        int rowByteBudget = MaximumQueryRowBytes, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (state >= (uint)wire.StateCount) throw new ArgumentOutOfRangeException(nameof(state));
        if (rowBudget < 0 || rowBudget > LegacyFst23CWireLayout.MaximumTransitionWords)
            throw new ArgumentOutOfRangeException(nameof(rowBudget));
        if (rowByteBudget < 0 || rowByteBudget > MaximumQueryRowBytes)
            throw new ArgumentOutOfRangeException(nameof(rowByteBudget));
        int first = (int)wire.GetDescriptor((int)state).RawField0;
        int count = (int)wire.GetDescriptor((int)state + 1).RawField0 - first;
        if (count > rowBudget) throw new InvalidDataException("Outgoing row budget exceeded.");
        if ((long)count * 16 > rowByteBudget) throw new InvalidDataException("Outgoing row byte budget exceeded.");
        var result = new LegacyFst13AOutgoingRow[count];
        for (int i = 0; i < count; i++)
        {
            if ((i & 1023) == 0) token.ThrowIfCancellationRequested();
            uint target = wire.GetTransitionWord(first + i); var descriptor = wire.GetDescriptor((int)target);
            sbyte tag = unchecked((sbyte)descriptor.RawField6);
            result[i] = new(target, descriptor.RawLabel, costs is null ? 0 : costs[tag + 128], tag, descriptor.RawField7);
        }
        token.ThrowIfCancellationRequested(); return new(state, result);
    }

    public int[]? CopyCostTable(CancellationToken token = default)
    { token.ThrowIfCancellationRequested(); var copy = costs?.ToArray(); token.ThrowIfCancellationRequested(); return copy; }

    // NoInlining isolates every float32 operation, preventing multiply/add
    // fusion and retaining native single-precision rounding boundaries in JIT/AOT.
    [MethodImpl(MethodImplOptions.NoInlining)] private static float Add(float x, float y) => x + y;
    [MethodImpl(MethodImplOptions.NoInlining)] private static float Mul(float x, float y) => x * y;
    [MethodImpl(MethodImplOptions.NoInlining)] private static float Div(float x, float y) => x / y;
    private static int[] FreshMain135Costs(CancellationToken token)
    {
        float multiplier = Mul(1f, 1f), bias = Mul(multiplier, 1.5f), scale = Mul(multiplier, .15f);
        float middle = Div(-bias, scale); var result = new int[256];
        result[0] = (int)Mul(19f, -8192f);
        for (int signed = -127; signed <= 127; signed++)
        {
            token.ThrowIfCancellationRequested();
            if (signed == 0) { result[128] = 0; continue; }
            float t = Div(signed < 0 ? signed + 127 : signed, 127f);
            float value = Add(Mul(Add(1f, -t), signed < 0 ? -3f : middle), Mul(t, signed < 0 ? middle : -17f));
            result[signed + 128] = (int)Mul(Add(Mul(value, scale), bias), -8192f);
        }
        token.ThrowIfCancellationRequested(); return result;
    }
}
