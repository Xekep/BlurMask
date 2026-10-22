using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

public enum LegacyFst28CPrimaryCodeKind { AcousticLabel, Special, Missing }
public readonly record struct LegacyFst28CPrimaryCode(ushort SourceLabel, ushort Code,
    LegacyFst28CPrimaryCodeKind Kind);

/// <summary>The primary16-label conversion prerequisite for one actual fresh
/// number13A/MAIN135 pair. Not a prepared28C owner, secondary map, query result,
/// cursor, history, alphabet admission policy or a recognizer.</summary>
public sealed class LegacyFst28CPrimaryAlphabetMap
{
    // Explicit independent library admission work: snapshot bytes plus bounded
    // source/acoustic label visits. Not an invented native limit.
    public const int PreparationWork = 572763 + 2580 + 16 + 192;
    private const string RootHash = "D26ED08BAF419361484958B3A04A5F740A228F4664F7E2643F43459223EF3E4E";
    private const string AcousticHash = "7A5F08809E81FB4A2D4C78B3EFD251D0986503B0D30D21DE3695DAB1F8A89978";
    private readonly LegacyFst28CPrimaryCode[] entries;
    public LegacyFst13AOutgoingSource Source { get; }
    public int SourceLabelCount => entries.Length;
    public int AcousticLabelCount => 192;
    public uint SerializedSourceFlags68 => 0x101;
    public int SourceAlphabetPayloadOffset => 299110;
    public int SourceAlphabetPayloadEnd => 299573;
    public string AcousticFingerprint => AcousticHash;
    private LegacyFst28CPrimaryAlphabetMap(LegacyFst13AOutgoingSource source,
        LegacyFst28CPrimaryCode[] entries) { Source = source; this.entries = entries; }

    /// <summary>Re-derives labels from actual retained source bytes and a private
    /// authenticated acoustic snapshot. No caller labels or detached query result
    /// can acquire source authority. Fresh normalization fallback is off.</summary>
    public static LegacyFst28CPrimaryAlphabetMap CreateActualRussianNumber(
        LegacyFst13AOutgoingSource source, ReadOnlySpan<byte> acousticResource,
        int preparationBudget = PreparationWork, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(source);
        if (preparationBudget < 0 || preparationBudget > PreparationWork)
            throw new ArgumentOutOfRangeException(nameof(preparationBudget));
        if (preparationBudget < PreparationWork) throw new InvalidDataException("Whole primary-map preparation exceeds budget.");
        var root = source.SourceRoot; var child = source.SourceChild;
        if (!source.HasActualFreshMain135Authority || source.ActualAcousticSourceKind != 0x135 ||
            source.ActualAcousticFingerprint != AcousticHash || root is null || child is null ||
            root.Fingerprint != RootHash || child.Ordinal != 5 || child.Offset != 299000 || child.End != 301580 ||
            child.Kind != 0x13a || child.Version != 0x202 || !ReferenceEquals(child, root.Children[5]))
            throw new ArgumentException("Actual retained number outgoing source required.", nameof(source));
        if (acousticResource.Length != 572763)
            throw new ArgumentException("Exact acoustic resource required.", nameof(acousticResource));
        var envelope = root.CopyChildEnvelope(child);
        if (envelope.Length != 2580 || BinaryPrimitives.ReadUInt32LittleEndian(envelope.AsSpan(96)) != 0x101 ||
            BinaryPrimitives.ReadUInt32LittleEndian(envelope.AsSpan(104)) != 0x2b ||
            BinaryPrimitives.ReadUInt16LittleEndian(envelope.AsSpan(108)) != 0x103)
            throw new InvalidDataException("Actual source flags/alphabet header differs.");
        var acousticBytes = new byte[572763];
        for (int at = 0; at < acousticBytes.Length; at += 65536)
        {
            token.ThrowIfCancellationRequested();
            acousticResource.Slice(at, Math.Min(65536, acousticBytes.Length - at)).CopyTo(acousticBytes.AsSpan(at));
        }
        token.ThrowIfCancellationRequested();
        if (Convert.ToHexString(SHA256.HashData(acousticBytes)) != AcousticHash ||
            BinaryPrimitives.ReadUInt32LittleEndian(acousticBytes.AsSpan(5233)) != 0x135)
            throw new ArgumentException("Actual acoustic snapshot differs.", nameof(acousticResource));
        // Existing owned stored alphabet parser; spans are EXACT authenticated
        // v103 payloads, not raw scans or caller-provided model identities.
        var input = LegacyInkAlphabet.Load(envelope.AsSpan(110, 463), 0x103);
        var output = LegacyInkAlphabet.Load(acousticBytes.AsSpan(99, 5134), 0x103);
        token.ThrowIfCancellationRequested();
        if (input.LabelCount != 16 || input.BytesConsumed != 463 || output.LabelCount != 192 || output.BytesConsumed != 5134)
            throw new InvalidDataException("Exact source/acoustic alphabet dimensions differ.");
        var entries = new LegacyFst28CPrimaryCode[16];
        for (ushort label = 0; label < entries.Length; label++)
        {
            token.ThrowIfCancellationRequested(); ushort code;
            if (input.IsVirtual(label)) code = ushort.MaxValue;
            else
            {
                if (!input.TryGetText(label, out string text)) throw new InvalidDataException("Source label has no stored text.");
                // Native special precedence. These are the only special tokens
                // occurring in this exact authenticated16-label source.
                if (text == "\0") code = 0xfffd;
                else if (text == " ") code = 0xfffb;
                else if (!output.TryGetLabel(text, out code)) code = ushort.MaxValue;
            }
            var kind = code < output.LabelCount ? LegacyFst28CPrimaryCodeKind.AcousticLabel :
                code == ushort.MaxValue ? LegacyFst28CPrimaryCodeKind.Missing : LegacyFst28CPrimaryCodeKind.Special;
            entries[label] = new(label, code, kind);
        }
        token.ThrowIfCancellationRequested(); return new(source, entries);
    }

    public LegacyFst28CPrimaryCode MapLabel(ushort label)
    {
        if (label >= entries.Length) throw new ArgumentOutOfRangeException(nameof(label));
        return entries[label];
    }
    public LegacyFst28CPrimaryCode[] CopyEntries(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested(); var copy = new LegacyFst28CPrimaryCode[entries.Length];
        for (int i = 0; i < entries.Length; i++) { token.ThrowIfCancellationRequested(); copy[i] = entries[i]; }
        token.ThrowIfCancellationRequested(); return copy;
    }
}
