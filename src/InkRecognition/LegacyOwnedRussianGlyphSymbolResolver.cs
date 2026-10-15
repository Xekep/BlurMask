namespace InkRecognition;

public readonly record struct LegacyRussianGlyphSymbolResolution(ushort EmittedSymbol, ushort NormalizedOriginal);

/// <summary>Checked216860 ordinary Russian symbol resolution, bound to an
///actual fresh case dictionary/preparation. No Arabic/wide/case-shaping service,
///replacement fallback, glyph score or native winner is synthesized.</summary>
public sealed class LegacyOwnedRussianGlyphSymbolResolver
{
    public LegacyFreshCaseRussianDictionaryPreparation DictionaryOwner {get;}
    public LegacyNativeCasePreparation Preparation {get;}
    public LegacyInkAlphabet Alphabet => DictionaryOwner.Model.AcousticAlphabet;
    public ushort SyntheticApostrophe {get;}
    public ushort SyntheticQuote {get;}
    private readonly ushort apostrophe, quote, replacement;

    private LegacyOwnedRussianGlyphSymbolResolver(LegacyFreshCaseRussianDictionaryPreparation dictionary,
        LegacyNativeCasePreparation preparation, ushort apostrophe, ushort quote, ushort replacement)
    {
        DictionaryOwner=dictionary;Preparation=preparation;
        this.apostrophe=apostrophe;this.quote=quote;this.replacement=replacement;
        SyntheticApostrophe=checked((ushort)(Alphabet.LabelCount+1));
        SyntheticQuote=checked((ushort)(Alphabet.LabelCount+2));
    }

    public static LegacyOwnedRussianGlyphSymbolResolver Prepare(
        LegacyFreshCaseRussianDictionaryPreparation dictionary, LegacyNativeCasePreparation actualPreparation,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(dictionary);ArgumentNullException.ThrowIfNull(actualPreparation);
        cancellationToken.ThrowIfCancellationRequested();
        var alphabet=dictionary.Model.AcousticAlphabet;
        if(!ReferenceEquals(dictionary.Symbols.Preparation,actualPreparation)||
            !ReferenceEquals(actualPreparation.Alphabet,alphabet)||
            !ReferenceEquals(dictionary.Symbols.Symbols.AcousticAlphabet,alphabet)||
            dictionary.Model.SourceKind!=0x135)
            throw new ArgumentException("Actual fresh Russian dictionary, acoustic alphabet and same preparation instance required.");
        //The private successful CasePreparation producer already scanned every
        //scalar with the pinned script service and refused Arabic script5.
        //It therefore proves Engine88 absent. Stored case triplets are not88.
        if(LegacyWideSymbolPairs.CreateUnrestricted(alphabet,cancellationToken).Length!=0)
            throw new NotSupportedException("Only the actual no-wide-pair alphabet is supported.");
        if(alphabet.LabelCount>ushort.MaxValue-2||!alphabet.TryGetLabel("'",out ushort apostrophe)||
            !alphabet.TryGetLabel("\"",out ushort quote)||!alphabet.TryGetLabel("\ufffd",out ushort replacement))
            throw new NotSupportedException("Actual quote/replacement labels and bounded synthetic quote labels required.");
        cancellationToken.ThrowIfCancellationRequested();
        return new(dictionary,actualPreparation,apostrophe,quote,replacement);
    }

    /// <summary>Only the low byte of conversionArgument controls the native
    ///early synthetic-quote branch. knowledge99 is an explicit prepared flag;
    ///when true, the native scalar bypass returns the input unchanged. Other ushort values,
    ///including protocol sentinels, pass unchanged at this scalar stage; their
    ///higher-level216BC0 semantics are not implemented by this resolver.</summary>
    public LegacyRussianGlyphSymbolResolution Resolve(ushort symbol, ulong conversionArgument,
        bool knowledge99=false, CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(unchecked((byte)conversionArgument)!=0)
        {
            if(symbol==SyntheticApostrophe)return new(apostrophe,apostrophe);
            if(symbol==SyntheticQuote)return new(quote,quote);
        }
        //216860 checks99 only after both early quote substitutions. Do not
        //move this bypass before them and accidentally change that ordering.
        if(knowledge99)return new(symbol,symbol);
        if(symbol==replacement)throw new NotSupportedException("FFFD requires the actual frontend1A8 fallback producer.");
        //EngineV80 reports zero wide-pair count and V68 noArabic88; the
        //remaining native ordinary path writes the unchanged symbol twice.
        return new(symbol,symbol);
    }
}
