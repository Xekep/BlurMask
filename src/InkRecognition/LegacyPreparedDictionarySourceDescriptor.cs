using System.Buffers.Binary;
using System.Text;

namespace InkRecognition;

/// <summary>Scalar/name prefix read by13b300→139301→138100/133.</summary>
public sealed record LegacyDictionaryResourcePrefix(string Identifier, int Weight60,
    int Weight64, uint Field68, float RuntimeLanguageScale, int AlphabetPayloadOffset);

/// <summary>Fresh2b7 descriptor after the bounded verified092990 field updates.</summary>
public sealed record LegacyPreparedDictionarySourceDescriptor(uint Flags, float Weight24,
    float Weight28, float RuntimeLanguageScale, uint LanguageOrder, string Identifier);

/// <summary>
/// Native identifier substring service37 and prepared dictionary descriptor.
/// Language order is an n-gram order, not a case mode. No script/default alias
/// policy is inferred. Allocation and the language preparation result are explicit.
/// </summary>
public static class LegacyPreparedDictionarySourceDescriptorFactory
{
    /// <summary>
    /// Engine global031910 installs registration252408→20f3c0 service37,
    /// table20f290 slot10=089680. Querying(subword) is a byte-ordinal substring
    /// search, not metadata key lookup; NUL terminates an unspecified-length name.
    /// </summary>
    public static int FindIdentifierSubstring(ReadOnlySpan<byte> identifier, ReadOnlySpan<byte> query)
    {
        if (identifier.Length > 65535 || query.Length > 65535)
            throw new ArgumentException("Bounded prepared identifier strings required.");
        int end = identifier.IndexOf((byte)0); if (end >= 0) identifier = identifier[..end];
        end = query.IndexOf((byte)0); if (end >= 0) query = query[..end];
        return identifier.IndexOf(query);
    }

    /// <summary>
    /// Read the checked flags0 type13b/version300 scalar prefix, before its
    /// lexical alphabet and nested dictionary/language objects. This does not
    /// load the remaining objects or execute resource authentication.
    /// Only inherited139301/138100 and alphabet2b103 are supported here.
    /// </summary>
    public static LegacyDictionaryResourcePrefix ReadV300Prefix(ReadOnlySpan<byte> resource)
    {
        if (resource.Length is < 110 or > 64 * 1024 * 1024 ||
            U32(resource, 0) != 0x0101fe90 || U32(resource, 4) != resource.Length - 8 ||
            U32(resource, 8) != 0x13b || U32(resource, 12) != 0x03000000 ||
            U16(resource, 16) != 0x301 || U16(resource, 18) != 0x100 ||
            U32(resource, 20) != 0x00e17557 || U32(resource, 80) != 0xffffffff)
            throw new InvalidDataException("Unsupported prepared dictionary resource prefix.");
        byte[] name = resource.Slice(24, 56).ToArray();
        ReadOnlySpan<byte> xor = [0x14, 0x12, 0x19, 0x77];
        for (int i = 0; i < name.Length; i++) name[i] ^= xor[i & 3];
        int nul = name.AsSpan().IndexOf((byte)0);
        if (nul < 0 || name.AsSpan(nul).ContainsAnyExcept((byte)0))
            throw new InvalidDataException("Dictionary identifier is not a terminated56-byte resource name.");
        string identifier;
        try { identifier = new UTF8Encoding(false, true).GetString(name.AsSpan(0, nul)); }
        catch (DecoderFallbackException e) { throw new InvalidDataException("Invalid dictionary identifier UTF8.", e); }
        //133 identity bytes80..91 are data only, not activation checks.
        int cursor = 92;
        uint formatOrWeight = U32(resource, cursor); cursor += 4;
        int first, second;
        if ((formatOrWeight & 0xff000000) == 0)
        { first = (int)formatOrWeight; second = 0x10000; }
        else
        {
            if (resource.Length < 118) throw new InvalidDataException("Truncated explicit dictionary weights.");
            first = I32(resource, cursor); second = I32(resource, cursor + 4); cursor += 8;
        }
        uint field68 = U32(resource, cursor); cursor += 4;
        float scale = BitConverter.Int32BitsToSingle(I32(resource, cursor)); cursor += 4;
        if (!float.IsFinite(scale) || scale <= 0 || U32(resource, cursor) != 0x2b || U16(resource, cursor + 4) != 0x103)
            throw new InvalidDataException("Prepared dictionary scale/alphabet header differs.");
        return new(identifier, first, second, field68, scale, cursor + 6);
    }

    /// <summary>
    /// Bounded language2d1/v100 prefix.0fcc10 assigns the first post-base uint
    /// (1..3) to language+60, its n-gram order. This does not validate later
    /// lookup tables; use the complete language loader for actual inference.
    /// </summary>
    public static uint ReadLanguageOrder(ReadOnlySpan<byte> languageObject)
    {
        if (languageObject.Length < 94 || U32(languageObject, 0) != 0x0101fe90 ||
            U32(languageObject, 4) != languageObject.Length - 8 || U32(languageObject, 8) != 0x2d1 ||
            U32(languageObject, 12) != 0x01000000 || U16(languageObject, 16) != 0x100)
            throw new InvalidDataException("Unsupported language object/base prefix.");
        uint order = U32(languageObject, 90);
        if (order is < 1 or > 3) throw new InvalidDataException("Native language order must lie in1..3.");
        return order;
    }

    /// <summary>
    ///092790 starts a fresh source header at0.092990 sets binding-presencebit1,
    /// language-orderbits2..4, selected-node-nonemptybit5 and kind800 only when
    /// node80's child interval is nonempty and its identifier contains(subword).
    /// Fresh092790 sets node80=1; this is not the public graph root0.
    /// Weight88/90 getters read signed139fields60/64, multiplied as float by2^-16.
    /// Runtime overrides/language allocation success must not be synthesized.
    /// </summary>
    public static LegacyPreparedDictionarySourceDescriptor CreateFresh(
        LegacyDictionaryResourcePrefix profile, LegacyPackedLexicon dictionary,
        uint? languageOrder, bool languageBindingPrepared, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile); ArgumentNullException.ThrowIfNull(dictionary);
        cancellationToken.ThrowIfCancellationRequested();
        if (languageOrder is > 3 or 0 || languageOrder is null && languageBindingPrepared ||
            !float.IsFinite(profile.RuntimeLanguageScale) || profile.RuntimeLanguageScale <= 0 ||
            profile.Identifier is null)
            throw new ArgumentException("Prepared matching dictionary/language descriptor facts required.");
        if (dictionary.NodeCount < 2) throw new ArgumentException("Fresh native dictionary node80=1 must exist.");
        bool selectedNodeHasChildren = dictionary.GetChildCount(1) != 0;
        byte[] identifier = Encoding.UTF8.GetBytes(profile.Identifier);
        uint kind = selectedNodeHasChildren && FindIdentifierSubstring(identifier, "subword"u8) >= 0 ? 0x800u : 0xc00u;
        uint flags = (languageBindingPrepared ? 2u : 0) | ((languageOrder ?? 0) * 4 & 0x1c) |
            (selectedNodeHasChildren ? 0x20u : 0) | kind;
        return new(flags, (float)profile.Weight60 * (1f / 65536), (float)profile.Weight64 * (1f / 65536),
            profile.RuntimeLanguageScale, languageOrder ?? 0, profile.Identifier);
    }

    private static uint U32(ReadOnlySpan<byte> b, int p) => BinaryPrimitives.ReadUInt32LittleEndian(b[p..]);
    private static int I32(ReadOnlySpan<byte> b, int p) => BinaryPrimitives.ReadInt32LittleEndian(b[p..]);
    private static ushort U16(ReadOnlySpan<byte> b, int p) => BinaryPrimitives.ReadUInt16LittleEndian(b[p..]);
}
