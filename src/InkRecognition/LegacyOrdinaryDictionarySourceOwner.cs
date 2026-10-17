namespace InkRecognition;

/// <summary>
/// Prepared dictionary owner with the proven class222 frontend / unwrapped21b graph.
/// Does not allocate a frontend, prepare children, or infer wrapper selection.
/// </summary>
public static class LegacyOrdinaryDictionarySourceOwner
{
    /// <summary>
    /// 093ab0 consults owner.virtual280 only for category800. Class222's155710
    /// forwards graph.virtual1C8; ordinary21b resolves it to05a400 (false).
    /// The supplied typed graph identifies this bounded ordinary implementation.
    /// </summary>
    public static LegacyAggregateDictionaryChild BindDictionary(LegacyPreparedDictionaryQuerySource source,
        LegacySymbolContextFactory contexts, LegacyOrdinaryCursiveCandidateGraph ordinaryGraph)
    {
        ArgumentNullException.ThrowIfNull(ordinaryGraph);
        return BindOwnedDictionary(source,contexts,LegacyOrdinaryRecognitionGraph.FromSameSource(ordinaryGraph));
    }

    internal static LegacyAggregateDictionaryChild BindOwnedDictionary(LegacyPreparedDictionaryQuerySource source,
        LegacySymbolContextFactory contexts, LegacyOrdinaryRecognitionGraph ordinaryGraph)
    {
        ArgumentNullException.ThrowIfNull(ordinaryGraph);
        return LegacyAggregateDictionaryChild.BindDictionary(source, contexts, static () => false);
    }
}
