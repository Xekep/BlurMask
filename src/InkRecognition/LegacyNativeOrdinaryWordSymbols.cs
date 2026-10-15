namespace InkRecognition;

/// <summary>Bounded native ordinary query labels. Unicode letter categories
/// are not a native query filter. Replacement, join-control, Arabic shadda and
/// virtual/private protocol branches require their separate owning policies.</summary>
internal static class LegacyNativeOrdinaryWordSymbols
{
    internal static bool Supports(LegacyInkAlphabet alphabet, ushort symbol)
        => symbol < alphabet.LabelCount && !alphabet.IsVirtual(symbol) &&
            alphabet.TryGetText(symbol, out string text) && text.Length == 1 &&
            text[0] is not ('\ufffd' or '\u200c' or '\u0651' or '\ufff5' or '\ufff9' or '\ufffa' or '\ufffb');

    internal static int VariantCount(LegacyInkAlphabet alphabet, ushort symbol, ushort inputFlags)
        => (inputFlags & 16) == 0 || IsQuote(alphabet, symbol) ? 2 : 1;

    private static bool IsQuote(LegacyInkAlphabet alphabet, ushort symbol)
        => alphabet.TryGetText(symbol, out string text) && text is "'" or "\"";

    internal static void CheckQuoteContext(LegacyInkAlphabet alphabet, ushort symbol,
        LegacySymbolContextFactory contexts, ulong context)
    {
        //1fe740 has an apostrophe/ZWNJ/FFFB rewrite after context composition.
        //This narrower consumer does not compose that protocol context. Reject
        //joiner history explicitly rather than infer its post-composition shape.
        if (!alphabet.TryGetText(symbol, out string text) || text != "'" ||
            !alphabet.TryGetLabel("\u200c", out ushort joiner)) return;
        ulong parent = contexts.ReadNode(context).ParentLink;
        int visited = 0;
        while (parent != 0)
        {
            if (++visited > 65535) throw new NotSupportedException("Bounded ordinary quote history required.");
            var previous = contexts.ReadNode(parent);
            if (previous.Symbol == joiner)
                throw new NotSupportedException("Apostrophe/joiner history needs its native context rewrite producer.");
            parent = previous.ParentLink;
        }
    }
}
