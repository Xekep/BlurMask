using System.Text;

namespace InkRecognition;

/// <summary>Actual glyph274 ushort+4 and ushort+8 fields. Field+6 result flags are a separate consumer.</summary>
public readonly record struct LegacyNativeGlyphSymbolDescriptor(ushort KindFlags, ushort SymbolOrRunLength);

/// <summary>Actual four-byte139c90 token: ushort label, byte alternate-alphabet flag, zero padding.</summary>
public readonly record struct LegacyNativeGlyphUtf8Token(ushort Label, bool UseAlternateAlphabet);

/// <summary>Owned byte-offset table consumed by13a0b0 when the glyph carries an alternate-alphabet token.</summary>
public sealed class LegacyNativeGlyphAlternateUtf8Table
{
    private readonly byte[] _bytes;
    private readonly int[] _offsets;
    public int SymbolCount => _offsets.Length - 1;

    public LegacyNativeGlyphAlternateUtf8Table(ReadOnlySpan<byte> bytes, ReadOnlySpan<int> offsets)
    {
        if (bytes.Length > 1_048_576 || offsets.Length is < 2 or > 65536)
            throw new ArgumentOutOfRangeException(nameof(offsets));
        int previous = 0;
        foreach (int offset in offsets)
        {
            if (offset < previous || offset > bytes.Length) throw new ArgumentException("Ordered actual alternate-byte offsets required.");
            previous = offset;
        }
        _bytes = bytes.ToArray(); _offsets = offsets.ToArray();
    }

    public ReadOnlySpan<byte> ReadSymbol(ushort label)
    {
        if (label >= SymbolCount) throw new ArgumentOutOfRangeException(nameof(label));
        return _bytes.AsSpan(_offsets[label], _offsets[label + 1] - _offsets[label]);
    }
}

/// <summary>
/// Native273.virtual50=13c920 ->274 token expansion13c840/139c90 ->13a0b0 raw byte emission.
/// This is BEFORE Engine0491a0 canonical composition/circled-marker processing and021000 classification.
/// </summary>
public static class LegacyNativeGlyphUtf8Tokens
{
    private static readonly UTF8Encoding Utf8 = new(false, true);

    public static int ReadTokenCount(LegacyNativeGlyphSymbolDescriptor descriptor) =>
        (descriptor.KindFlags & 7) is 1 or 3 ? (descriptor.SymbolOrRunLength >> 2) & 0x1fff : 1;

    public static LegacyNativeGlyphUtf8Token[] Expand(LegacyNativeGlyphSymbolDescriptor descriptor,
        Func<string, ushort>? actualControlLabelLookup = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        int count = ReadTokenCount(descriptor);
        if (count == 0) return [];
        int kind = descriptor.KindFlags & 7;
        ushort label = descriptor.SymbolOrRunLength;
        bool alternate = false;
        if (kind == 1) label = unchecked((ushort)((descriptor.SymbolOrRunLength & 3) - 7));
        else if (kind == 3)
        {
            string lookup = (descriptor.SymbolOrRunLength & 3) switch
            { 0 => "\n", 2 => " ", 3 => "\u00a0", _ => throw new NotSupportedException("Native kind3/subkind1 leaves an undefined token; no label is invented.") };
            //139c90 looks up result273+18, distinct from the renderer's primary alphabet parameter.
            //Its actual2A lookup owner is not substituted with the acoustic alphabet or a hardcoded ID.
            if (actualControlLabelLookup is null)
                throw new ArgumentException("Actual result273+18 control-label lookup required for kind3.", nameof(actualControlLabelLookup));
            label = actualControlLabelLookup(lookup);
        }
        else if (kind == 4) alternate = true;
        else if (kind is not (0 or 2)) label = ushort.MaxValue;
        var token = new LegacyNativeGlyphUtf8Token(label, alternate);
        var result = new LegacyNativeGlyphUtf8Token[count];
        for (int i = 0; i < result.Length; i++) { cancellationToken.ThrowIfCancellationRequested(); result[i] = token; }
        return result;
    }

    /// <summary>
    /// Exact byte emission before native normalization, without trailing storage NUL (which is outside its byte count).
    /// Missing alternate table follows the native primary-alphabet branch. Native output is not yet accepted text.
    /// </summary>
    public static byte[] EmitBeforeCanonicalComposition(ReadOnlySpan<LegacyNativeGlyphUtf8Token> tokens,
        LegacyInkAlphabet actualPrimaryAlphabet, LegacyNativeGlyphAlternateUtf8Table? alternateTable = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actualPrimaryAlphabet); cancellationToken.ThrowIfCancellationRequested();
        if (tokens.Length > 65536) throw new ArgumentOutOfRangeException(nameof(tokens));
        using var output = new MemoryStream();
        var primaryCache = new Dictionary<ushort, byte[]>();
        foreach (var token in tokens)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReadOnlySpan<byte> bytes;
            if (token.UseAlternateAlphabet && alternateTable is not null) bytes = alternateTable.ReadSymbol(token.Label);
            else if (token.Label < actualPrimaryAlphabet.LabelCount)
            {
                if (!primaryCache.TryGetValue(token.Label, out var cached))
                {
                    if (!actualPrimaryAlphabet.TryGetText(token.Label, out string text))
                        throw new InvalidDataException("Primary alphabet label has no stored UTF8 entry.");
                    if (Utf8.GetByteCount(text) > 1_048_576 - output.Length)
                        throw new ArgumentOutOfRangeException(nameof(tokens), "Portable UTF8 byte budget exceeded.");
                    cached = Utf8.GetBytes(text); primaryCache.Add(token.Label, cached);
                }
                bytes = cached;
            }
            else bytes = token.Label switch
            {
                0xfffc => "\u00a0"u8, 0xfffb => " "u8, 0xfff9 => "\n"u8,
                0xfff6 => "\U000f0000"u8, 0xfff3 => "\U000f0010"u8,
                0xfff2 => "\U000f0011"u8, 0xfff1 => "\U000f0012"u8,
                0xfff0 => "\U000f0013"u8, 0xffef => "\U000f0014"u8,
                0xffee => "\U000f0015"u8, 0xffed => "\U000f0016"u8,
                0xffec => "\U000f0017"u8, 0xffeb => "\U000f0018"u8,
                _ => "\ufffd"u8
            };
            if (bytes.Length > 1_048_576 - output.Length)
                throw new ArgumentOutOfRangeException(nameof(tokens), "Portable UTF8 byte budget exceeded.");
            output.Write(bytes);
        }
        return output.ToArray();
    }
}
