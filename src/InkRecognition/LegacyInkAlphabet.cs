using System.Buffers.Binary;
using System.Text;

namespace InkRecognition;

/// <summary>
/// Caller-supplied legacy alphabet payload. Acoustic and lexical resources use
/// different label IDs; map through text rather than assuming their IDs match.
/// </summary>
public sealed class LegacyInkAlphabet
{
    private readonly Dictionary<ushort, string> _text;
    private readonly Dictionary<string, ushort> _labels;
    public int LabelCount { get; }
    public int BytesConsumed { get; }

    private LegacyInkAlphabet(int count, int consumed, Dictionary<ushort, string> text)
    {
        LabelCount = count; BytesConsumed = consumed; _text = text;
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
        reader.Bytes(count * 2); // Reverse lookup slots, retained by the native reader.
        byte haveBits = reader.U8();
        if (haveBits > 1) throw Invalid("Invalid alphabet bitset flag.");
        if (haveBits != 0) reader.Bytes((count + 7) / 8);
        int triplets = reader.U16(); reader.Bytes(triplets * 6);
        if (triplets != 0)
        {
            uint flags = reader.U32();
            if ((flags & ~7u) != 0) throw Invalid("Unsupported alphabet triplet flags.");
            int width = triplets < 128 ? 1 : 2;
            for (int bit = 1; bit <= 4; bit *= 2) if ((flags & bit) != 0) reader.Bytes(count * width);
        }
        if (version == 0x104) reader.U8();
        return new(count, reader.Position, text);
    }

    public bool TryGetText(ushort label, out string text) => _text.TryGetValue(label, out text!);
    public bool TryGetLabel(string text, out ushort label) => _labels.TryGetValue(text, out label);

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
