namespace InkRecognition;

/// <summary>
/// One prepared source.virtual30 enumeration. DictionaryMap is the explicit
/// source+58 ushort map; an empty map forwards tokens without conversion.
/// Completed records the enumerator's return value after the supplied tokens.
/// </summary>
public sealed record LegacyLinguisticSymbolSource(ReadOnlyMemory<uint> Tokens,
    ReadOnlyMemory<ushort> DictionaryMap, bool Completed = true);

/// <summary>A controller return value and seed suitable for Text1b8850.</summary>
public sealed record LegacyLinguisticSymbolMaskSeed(LegacySymbolMaskControllerOutcome Outcome,
    ReadOnlyMemory<byte> Mask);

/// <summary>
/// Bounded Textclass240.virtualA8 implementation for explicitly prepared
/// linguistic sources. Source construction, refresh/decorator state and the
/// application's runtime190 binding are separate upstream responsibilities.
/// </summary>
public static class LegacyLinguisticSymbolMaskController
{
    /// <summary>
    /// Text206720/2066d0, 236850 and093b10/093af0: prepare, clear, enumerate
    /// sources in order, optionally map their low16 label, then enable one
    /// label or all labels for tokens FFE3..FFEA. A failed source stops the
    /// chain and returns no accepted seed, letting1b8850 use its native fallback.
    /// </summary>
    public static LegacyLinguisticSymbolMaskSeed Build(int acousticSymbolCount,
        IReadOnlyList<LegacyLinguisticSymbolSource> sources, bool preparationSucceeded,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sources);
        cancellationToken.ThrowIfCancellationRequested();
        if (acousticSymbolCount is < 1 or > ushort.MaxValue || sources.Count > 256)
            throw new ArgumentException("Explicit bounded alphabet and source chain required.");
        if (!preparationSucceeded) return new(LegacySymbolMaskControllerOutcome.Failed, ReadOnlyMemory<byte>.Empty);
        int entries = 0;
        foreach (var source in sources)
        {
            ArgumentNullException.ThrowIfNull(source);
            if (source.Tokens.Length > 1_048_576 - entries || source.DictionaryMap.Length > ushort.MaxValue)
                throw new ArgumentException("Prepared linguistic source exceeds bounded dimensions.");
            entries += source.Tokens.Length;
        }
        byte[] mask = new byte[acousticSymbolCount];
        foreach (var source in sources)
        {
            var map = source.DictionaryMap.Span;
            foreach (uint token in source.Tokens.Span)
            {
                cancellationToken.ThrowIfCancellationRequested();
                ushort symbol = (ushort)(token & 0xffffu);
                if (!map.IsEmpty)
                {
                    if (symbol >= map.Length) throw new ArgumentException("Source token is outside its explicit dictionary map.");
                    symbol = map[symbol];
                }
                if (symbol is >= 0xffe3 and <= 0xffea) mask.AsSpan().Fill(1);
                else if (symbol < mask.Length) mask[symbol] = 1;
            }
            if (!source.Completed) return new(LegacySymbolMaskControllerOutcome.Failed, ReadOnlyMemory<byte>.Empty);
        }
        return new(LegacySymbolMaskControllerOutcome.Succeeded, mask);
    }

    /// <summary>
    /// Initial Text092990 map, with normalization fallback explicitly disabled.
    /// Exact text lookup and native control-token conversion only. This is not
    /// a Latin/Cyrillic alias map and does not apply later dictionary mutations.
    /// </summary>
    public static ushort[] CreateExactDictionaryMap(LegacyInkAlphabet acousticAlphabet,
        LegacyInkAlphabet lexicalAlphabet, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(acousticAlphabet); ArgumentNullException.ThrowIfNull(lexicalAlphabet);
        cancellationToken.ThrowIfCancellationRequested();
        var map = new ushort[lexicalAlphabet.LabelCount];
        for (int i = 0; i < map.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ushort label = (ushort)i;
            if (lexicalAlphabet.IsVirtual(label)) { map[i] = ushort.MaxValue; continue; }
            if (!lexicalAlphabet.TryGetText(label, out string text))
                throw new InvalidDataException("Nonvirtual lexical symbol lacks its source UTF8 text.");
            map[i] = ResolveExactDictionaryToken(text, acousticAlphabet);
        }
        return map;
    }

    /// <summary>Text1d6d50 enumerates every lexical label, not just reachable dictionary words.</summary>
    public static LegacyLinguisticSymbolSource CreateDictionarySource(LegacyInkAlphabet acousticAlphabet,
        LegacyInkAlphabet lexicalAlphabet, CancellationToken cancellationToken = default)
    {
        ushort[] map = CreateExactDictionaryMap(acousticAlphabet, lexicalAlphabet, cancellationToken);
        uint[] tokens = new uint[map.Length];
        for (int i = 0; i < tokens.Length; i++) tokens[i] = (uint)i;
        return new(tokens, map);
    }

    /// <summary>Exact initial dictionary-label conversion; missing acoustic text yields FFFF.</summary>
    public static ushort ResolveExactDictionaryToken(string text, LegacyInkAlphabet acousticAlphabet)
    {
        ArgumentNullException.ThrowIfNull(text); ArgumentNullException.ThrowIfNull(acousticAlphabet);
        ushort? special = text switch
        {
            "\0" => 0xfffd, " " => 0xfffb, "\n" => 0xfff9, "\u00a0" => 0xfffc,
            "\ufffd" => 0xffff,
            "\U000f000c" => 0xfffa,
            "\U000f0009" or "\U000f000a" or "\U000f000b" => 0xffff,
            "\U000f0000" => 0xfff6, "\U000f0006" => 0xfff8, "\U000f0007" => 0xfff7,
            "\U000f0010" => 0xfff3, "\U000f0011" => 0xfff2, "\U000f0012" => 0xfff1,
            "\U000f0013" => 0xfff0, "\U000f0014" => 0xffef, "\U000f0015" => 0xffee,
            "\U000f0016" => 0xffed, "\U000f0017" => 0xffec, "\U000f0018" => 0xffeb,
            "\U000f0005" => 0xffea,
            "\U000f0020" => 0xffe9, "\U000f0021" => 0xffe8, "\U000f0022" => 0xffe7,
            "\U000f0023" => 0xffe6, "\U000f0024" => 0xffe5, "\U000f0025" => 0xffe4,
            "\U000f0026" => 0xffe3,
            _ => null
        };
        return special ?? (acousticAlphabet.TryGetLabel(text, out ushort label) ? label : ushort.MaxValue);
    }
}
