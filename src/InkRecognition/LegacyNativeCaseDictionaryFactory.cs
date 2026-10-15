namespace InkRecognition;

/// <summary>Prepared20C9D0 case-source selection for an already owned dictionary.
/// Actual container setting90 is required; constructor defaults alone do not
/// prove that no later setter changed it.</summary>
public static class LegacyNativeCaseDictionaryFactory
{
    public static uint ResolveSelectedMode(LegacyDictionaryResourcePrefix source,uint containerCaseMode90)
    {
        ArgumentNullException.ThrowIfNull(source);
        return containerCaseMode90==uint.MaxValue?source.Field68&7:containerCaseMode90;
    }
    public static LegacyNativeCaseNormalQuerySource Bind292(LegacyDictionaryResourcePrefix source,
        uint containerCaseMode90,LegacyWordLanguageModel language,LegacyInkAlphabet sourceAlphabet,
        LegacyNativeCaseData caseData,LegacyNativeTextPublicationData publication,LegacyNativeCasePreparation preparation,
        LegacySymbolContextFactory contexts,LegacyPreparedDictionaryQuerySource dictionary,
        LegacyOrdinaryCursiveCandidateGraph graph,bool extraCostSideTable)
    {
        ArgumentNullException.ThrowIfNull(language);ArgumentNullException.ThrowIfNull(preparation);
        ArgumentNullException.ThrowIfNull(sourceAlphabet);ArgumentNullException.ThrowIfNull(caseData);
        ArgumentNullException.ThrowIfNull(publication);
        if(ResolveSelectedMode(source,containerCaseMode90)!=1)
            throw new NotSupportedException("Prepared native selection must actually choose292, not291/unwrapped sources.");
        //20C9D0 tests the WORD RESOURCE's lexical AL.virtual58, not the
        //frontend acoustic AL used later by2324E0. The actual RU lexical AL
        //has no stored maps and therefore requires the native Unicode fallback.
        if(!caseData.HasCaseAlternatives(sourceAlphabet,publication))
            throw new NotSupportedException("The word resource alphabet does not select a native case wrapper.");
        var policy=LegacyNativeCasePathPolicy.Create(language.CasePathVersion);
        var expansion=new LegacyNativeCaseRecordExpansion(preparation,policy,contexts);
        bool dutch=source.Identifier.Contains("nl_",StringComparison.Ordinal);
        return LegacyNativeCaseNormalQuerySource.BindOrdinaryDictionary(expansion,dictionary,graph,dutch,extraCostSideTable);
    }
}
