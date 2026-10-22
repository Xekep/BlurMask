using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

public readonly record struct LegacyFst28CTargetField(byte Slot, ushort Code);
public readonly record struct LegacyFst28CRangeCheck(byte Slot, char Character, int ExpectedCode, ushort ActualCode);
public readonly record struct LegacyFst28CSecondaryCode(ushort SourceLabel, ushort Code);

/// <summary>Owned target-special/range and constructor-only fresh secondary
/// prerequisite for one authenticated number13A/MAIN135 pair. Not a complete
/// 28C owner, query, history or configured external-knowledge receipt.</summary>
public sealed class LegacyFst28CTargetPreparation
{
    public const int PreparationWork = 572763 + 2580 + 192 + 59 + 16 + 14;
    private const string AcousticHash = "7A5F08809E81FB4A2D4C78B3EFD251D0986503B0D30D21DE3695DAB1F8A89978";
    private readonly LegacyFst28CTargetField[] fields;
    private readonly LegacyFst28CRangeCheck[] rangeChecks;
    private readonly LegacyFst28CSecondaryCode[] secondary;
    // A NEW library-owned constructor-only policy. It is not inferred to have
    // existed inside PrimaryMap and grants no configured/native query authority.
    private sealed class FreshKnowledgePolicy(LegacyFst28CPrimaryAlphabetMap primary)
    {
        internal readonly LegacyFst28CPrimaryAlphabetMap Primary = primary;
        internal bool C4 => false; // Actual00C860 constructor, no property events.
    }
    private readonly FreshKnowledgePolicy policy;
    public LegacyFst28CPrimaryAlphabetMap PrimaryMap => policy.Primary;
    private LegacyFst28CTargetPreparation(FreshKnowledgePolicy policy,
        LegacyFst28CTargetField[] fields, LegacyFst28CRangeCheck[] rangeChecks,
        LegacyFst28CSecondaryCode[] secondary)
    { this.policy = policy; this.fields = fields; this.rangeChecks = rangeChecks; this.secondary = secondary; }

    public static LegacyFst28CTargetPreparation CreateActualRussianNumberFresh(
        LegacyFst28CPrimaryAlphabetMap primary, ReadOnlySpan<byte> acousticResource,
        int preparationBudget = PreparationWork, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(primary);
        if (preparationBudget < 0 || preparationBudget > PreparationWork)
            throw new ArgumentOutOfRangeException(nameof(preparationBudget));
        if (preparationBudget < PreparationWork) throw new InvalidDataException("Whole target preparation exceeds budget.");
        var source = primary.Source; var root = source.SourceRoot; var child = source.SourceChild;
        if (!source.HasActualFreshMain135Authority || source.ActualAcousticSourceKind != 0x135 ||
            source.ActualAcousticFingerprint != AcousticHash || root is null || child is null ||
            child.Ordinal != 5 || child.Offset != 299000 || child.End != 301580 ||
            child.Kind != 0x13a || child.Version != 0x202 || !ReferenceEquals(child, root.Children[5]) ||
            primary.SourceLabelCount != 16 || primary.AcousticLabelCount != 192)
            throw new ArgumentException("Actual retained primary owner required.", nameof(primary));
        if (acousticResource.Length != 572763) throw new ArgumentException("Exact acoustic resource required.", nameof(acousticResource));
        var snapshot = new byte[572763];
        for (int at = 0; at < snapshot.Length; at += 65536)
        {
            token.ThrowIfCancellationRequested();
            acousticResource.Slice(at, Math.Min(65536, snapshot.Length - at)).CopyTo(snapshot.AsSpan(at));
        }
        token.ThrowIfCancellationRequested();
        if (Convert.ToHexString(SHA256.HashData(snapshot)) != AcousticHash ||
            BinaryPrimitives.ReadUInt32LittleEndian(snapshot.AsSpan(5233)) != 0x135 ||
            BinaryPrimitives.ReadUInt32LittleEndian(snapshot.AsSpan(93)) != 0x2b ||
            BinaryPrimitives.ReadUInt16LittleEndian(snapshot.AsSpan(97)) != 0x103)
            throw new ArgumentException("Same authenticated MAIN135 snapshot required.", nameof(acousticResource));
        var envelope = root.CopyChildEnvelope(child);
        if (envelope.Length != 2580 || BinaryPrimitives.ReadUInt32LittleEndian(envelope.AsSpan(96)) != 0x101 ||
            BinaryPrimitives.ReadUInt32LittleEndian(envelope.AsSpan(104)) != 0x2b ||
            BinaryPrimitives.ReadUInt16LittleEndian(envelope.AsSpan(108)) != 0x103)
            throw new InvalidDataException("Actual retained source alphabet differs.");
        var alphabet = LegacyInkAlphabet.Load(snapshot.AsSpan(99, 5134), 0x103);
        token.ThrowIfCancellationRequested();
        if (alphabet.LabelCount != 192 || alphabet.BytesConsumed != 5134) throw new InvalidDataException("Actual acoustic alphabet dimensions differ.");
        var header = alphabet.CopySerializedHeader(); var fields = new LegacyFst28CTargetField[14];
        ushort Lookup(string text) => alphabet.TryGetLabel(text, out ushort id) ? id : ushort.MaxValue;
        void Set(byte slot, ushort id) => fields[(slot - 0xa0) / 2] = new(slot, id);
        Set(0xa0, Lookup("\U000f0001"));
        Set(0xa2, BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(8)));
        Set(0xa4, BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(10)));
        Set(0xa6, BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(12)));
        Set(0xa8, Lookup("\U000f0002")); Set(0xaa, Lookup("\U000f0003"));
        Set(0xac, Lookup("\U000f0004")); Set(0xae, Lookup("\U000f0008"));
        Set(0xb0, Lookup("\ufffd")); Set(0xb2, Lookup("\u00df")); Set(0xb4, Lookup("\u00ad"));
        var checks = new List<LegacyFst28CRangeCheck>(59);
        foreach (var (slot, text) in new (byte, string)[] { (0xb6,"0123456789"), (0xb8,"abcdefghijklmnopqrstuvwxyz"), (0xba,"ABCDEFGHIJKLMNOPQRSTUVWXYZ") })
        {
            ushort start = Lookup(text[..1]); bool contiguous = true;
            for (int i = 1; i < text.Length; i++)
            {
                token.ThrowIfCancellationRequested(); ushort actual = Lookup(text[i].ToString());
                checks.Add(new(slot, text[i], start + i, actual));
                if (actual != start + i) { contiguous = false; break; }
            }
            Set(slot, contiguous ? start : ushort.MaxValue);
        }
        var policy = new FreshKnowledgePolicy(primary); var secondary = new LegacyFst28CSecondaryCode[16];
        for (ushort i = 0; i < secondary.Length; i++)
        {
            token.ThrowIfCancellationRequested(); ushort code = primary.MapLabel(i).Code;
            secondary[i] = new(i, code >= 192 && (!policy.C4 || ((code + 7) & 0xfffd) != 0) ? code : ushort.MaxValue);
        }
        token.ThrowIfCancellationRequested(); return new(policy, fields, checks.ToArray(), secondary);
    }
    public ushort GetTargetField(byte slot)
    {
        if (slot < 0xa0 || slot > 0xba || (slot & 1) != 0) throw new ArgumentOutOfRangeException(nameof(slot));
        return fields[(slot - 0xa0) / 2].Code;
    }
    public LegacyFst28CSecondaryCode MapSecondary(ushort label)
    {
        if (label >= secondary.Length) throw new ArgumentOutOfRangeException(nameof(label));
        return secondary[label];
    }
    public LegacyFst28CTargetField[] CopyTargetFields(CancellationToken token = default)
    { token.ThrowIfCancellationRequested(); return fields.ToArray(); }
    public LegacyFst28CRangeCheck[] CopyRangeChecks(CancellationToken token = default)
    { token.ThrowIfCancellationRequested(); return rangeChecks.ToArray(); }
    public LegacyFst28CSecondaryCode[] CopySecondary(CancellationToken token = default)
    { token.ThrowIfCancellationRequested(); return secondary.ToArray(); }
}
