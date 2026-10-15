using System.Buffers.Binary;
using System.Text;

namespace InkRecognition;

/// <summary>Three related symbol IDs in native stored order; 65535 means absent.</summary>
public readonly record struct LegacyInkRelatedForms(ushort First, ushort Second, ushort Third);

/// <summary>
/// Caller-supplied legacy alphabet payload. Acoustic and lexical resources use
/// different label IDs; map through text rather than assuming their IDs match.
/// </summary>
public sealed class LegacyInkAlphabet
{
    private readonly Dictionary<ushort, string> _text;
    private readonly Dictionary<string, ushort> _labels;
    private readonly ushort[] _reverse;
    private readonly byte[] _symbolFlags;
    private readonly LegacyInkRelatedForms[] _forms;
    private readonly short[]?[] _relationMaps;
    private readonly byte[] _serializedHeader;
    public int LabelCount { get; }
    public int BytesConsumed { get; }
    public bool IsCaseRangeMetadataEnabled { get; }

    private LegacyInkAlphabet(int count, int consumed, Dictionary<ushort, string> text,
        ushort[] reverse, byte[] symbolFlags, LegacyInkRelatedForms[] forms, short[]?[] relationMaps, bool caseMetadata,
        byte[] serializedHeader)
    {
        LabelCount = count; BytesConsumed = consumed; _text = text;
        _reverse = reverse; _symbolFlags = symbolFlags; _forms = forms; _relationMaps = relationMaps;
        IsCaseRangeMetadataEnabled=caseMetadata;
        _serializedHeader=serializedHeader;
        _labels = new(StringComparer.Ordinal);
        foreach (var item in text)
            if (!_labels.TryAdd(item.Value, item.Key) && _labels[item.Value] != item.Key)
                throw new InvalidDataException("Ambiguous alphabet text mapping.");
    }

    /// <summary>Read one type2b alphabet payload, version103 or104, without its resource header.</summary>
    public static LegacyInkAlphabet Load(ReadOnlySpan<byte> payload, ushort version)
    {
        if (version is not (0x103 or 0x104) || payload.Length is < 18 or > 64 * 1024 * 1024)
            throw Invalid("Unsupported alphabet version or length.");
        var reader = new Reader(payload);
        int count = 0;
        for (int i = 0; i < 8; i++) count = reader.U16();
        if (count == 0) throw Invalid("Empty alphabet is unsupported.");
        int entries = reader.U16();
        foreach (int increment in new[] { 32, 64, 1 })
        {
            if (entries is < 1 or > 65535) throw Invalid("Invalid alphabet lookup dimensions.");
            int last = -1;
            for (int i = 0; i < entries; i++) { int value = reader.U16(); if (value != 65535) last = value; }
            if (last < 0) throw Invalid("Empty alphabet lookup table.");
            entries = last + increment;
        }
        if (entries is < 1 or > 65535) throw Invalid("Invalid alphabet spans.");
        int lastSpan = 0, previous = 0;
        for (int i = 0; i <= entries; i++)
        {
            lastSpan = reader.U16();
            if (lastSpan < previous) throw Invalid("Nonmonotonic alphabet spans.");
            previous = lastSpan;
        }
        var offsets = new uint[lastSpan + 1]; var labels = new ushort[lastSpan + 1];
        for (int i = 0; i < offsets.Length; i++) { offsets[i] = reader.U32(); labels[i] = (ushort)reader.U32(); }
        if (offsets.Length < 2 || offsets[0] != 0 || offsets[^1] > 16 * 1024 * 1024)
            throw Invalid("Invalid alphabet string bounds.");
        var strings = reader.Bytes(checked((int)offsets[^1]));
        var text = new Dictionary<ushort, string>(); var utf8 = new UTF8Encoding(false, true);
        for (int i = 0; i < offsets.Length - 1; i++)
        {
            if (offsets[i + 1] <= offsets[i] || offsets[i + 1] > offsets[^1] || labels[i] >= count)
                throw Invalid("Invalid alphabet string entry.");
            string value;
            try { value = utf8.GetString(strings.Slice((int)offsets[i], (int)(offsets[i + 1] - offsets[i]))); }
            catch (DecoderFallbackException) { throw Invalid("Invalid UTF-8 alphabet entry."); }
            if (!text.TryAdd(labels[i], value) && text[labels[i]] != value) throw Invalid("Conflicting alphabet label mapping.");
        }
        var reverse = new ushort[count];
        for (int i = 0; i < count; i++)
        {
            reverse[i] = reader.U16();
            if (reverse[i] != ushort.MaxValue && reverse[i] >= lastSpan) throw Invalid("Invalid reverse alphabet slot.");
        }
        byte haveBits = reader.U8();
        if (haveBits > 1) throw Invalid("Invalid alphabet bitset flag.");
        byte[] symbolFlags = haveBits == 0 ? [] : reader.Bytes((count + 7) / 8).ToArray();
        int triplets = reader.U16();
        var forms = new LegacyInkRelatedForms[triplets];
        for (int i = 0; i < triplets; i++)
        {
            forms[i] = new(reader.U16(), reader.U16(), reader.U16());
            foreach (ushort label in new[] { forms[i].First, forms[i].Second, forms[i].Third })
                if (label != ushort.MaxValue && label >= count) throw Invalid("Related form exceeds alphabet.");
        }
        var relationMaps = new short[]?[3];
        if (triplets != 0)
        {
            uint flags = reader.U32();
            if ((flags & ~7u) != 0) throw Invalid("Unsupported alphabet triplet flags.");
            for (int mode = 0; mode < 3; mode++)
                if ((flags & (1u << mode)) != 0)
                {
                    var map = new short[count]; relationMaps[mode] = map;
                    for (int i = 0; i < count; i++)
                    {
                        int index = triplets < 128 ? unchecked((sbyte)reader.U8()) : unchecked((short)reader.U16());
                        if (index < -1 || index >= triplets) throw Invalid("Invalid related-form lookup index.");
                        map[i] = (short)index;
                    }
                }
        }
        bool caseMetadata=version==0x104 ? reader.U8()!=0 : triplets!=0;
        return new(count, reader.Position, text, reverse, symbolFlags, forms, relationMaps,caseMetadata,payload[..16].ToArray());
    }

    public bool TryGetText(ushort label, out string text) => _text.TryGetValue(label, out text!);
    public bool TryGetLabel(string text, out ushort label) => _labels.TryGetValue(text, out label);
    /// <summary>Owned copy of the eight serialized special-label/count fields.</summary>
    public byte[] CopySerializedHeader() => _serializedHeader.ToArray();
    /// <summary>Native052BD0 checks the three stored relation-map pointers before its Unicode fallback scan.</summary>
    public bool HasStoredRelationMaps => _relationMaps.Any(map => map is not null);
    internal LegacyInkRelatedForms[] CopyStoredCaseTriplets() => _forms.ToArray();

    /// <summary>Native reverse slot65535 marks a virtual symbol, independent of Unicode text.</summary>
    public bool IsVirtual(ushort label) { ValidateLabel(label); return _reverse[label] == ushort.MaxValue; }

    /// <summary>Read the stored per-symbol bit; its higher-level recognition role is not inferred.</summary>
    public bool IsSymbolFlagged(ushort label)
    {
        ValidateLabel(label); return _symbolFlags.Length != 0 && (_symbolFlags[label >> 3] & (1 << (label & 7))) != 0;
    }

    /// <summary>
    /// Restore the checked native three-form accessor. Mode1/2 use their own
    /// lookup if present, otherwise mode0. This exposes supplied relationships;
    /// it does not infer case conversion or score/correct a word.
    /// </summary>
    public bool TryGetRelatedForms(ushort label, int mode, out LegacyInkRelatedForms forms)
    {
        ValidateLabel(label);
        if (mode is < 0 or > 2) throw new ArgumentOutOfRangeException(nameof(mode));
        var map = _relationMaps[mode] ?? _relationMaps[0];
        if (map is null || map[label] == -1) { forms = new(ushort.MaxValue, ushort.MaxValue, ushort.MaxValue); return false; }
        forms = _forms[map[label]];
        if (label != forms.First && label != forms.Second && label != forms.Third)
        {
            if (forms.First == ushort.MaxValue) forms = forms with { First = label };
            else if (forms.Second == ushort.MaxValue) forms = forms with { Second = label };
        }
        return true;
    }

    private void ValidateLabel(ushort label)
    {
        if (label >= LabelCount) throw new ArgumentOutOfRangeException(nameof(label));
    }

    private static InvalidDataException Invalid(string message) => new(message);
    private ref struct Reader
    {
        private readonly ReadOnlySpan<byte> _data;
        public Reader(ReadOnlySpan<byte> data) { _data = data; Position = 0; }
        public int Position { get; private set; }
        public ReadOnlySpan<byte> Bytes(int count)
        {
            if (count < 0 || count > _data.Length - Position) throw Invalid("Truncated alphabet field.");
            var result = _data.Slice(Position, count); Position += count; return result;
        }
        public byte U8() => Bytes(1)[0];
        public ushort U16() => BinaryPrimitives.ReadUInt16LittleEndian(Bytes(2));
        public uint U32() => BinaryPrimitives.ReadUInt32LittleEndian(Bytes(4));
    }
}
