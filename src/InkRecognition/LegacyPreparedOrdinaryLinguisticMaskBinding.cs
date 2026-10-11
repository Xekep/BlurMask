namespace InkRecognition;

/// <summary>A prepared controller result and the two downstream ordinary masks.</summary>
public sealed record LegacyPreparedOrdinaryLinguisticMaskResult(
    LegacySymbolMaskControllerOutcome ControllerOutcome, LegacyAcousticSymbolMasks Masks);

/// <summary>
/// Bounded class2b3 → class222 → ordinary21d integration of a prepared class240
/// source enumeration. This owns snapshots, not a vendor object or its factory.
/// Source refresh/decorators and runtime preparation remain explicit inputs.
/// </summary>
public sealed class LegacyPreparedOrdinaryLinguisticMaskBinding
{
    private readonly LegacyOrdinarySymbolMaskProducer producer;
    private readonly LegacyLinguisticSymbolSource[] sources;
    private readonly int symbolCount;

    /// <summary>
    /// Attach immutable copies of the already prepared source tokens/maps to
    /// this ordinary alphabet. No linguistic source factory is inferred.
    /// </summary>
    public LegacyPreparedOrdinaryLinguisticMaskBinding(LegacyInkAlphabet acousticAlphabet,
        ReadOnlySpan<byte> serializedAlphabetHeader,
        IReadOnlyList<LegacyLinguisticSymbolSource> preparedSources,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(acousticAlphabet);
        ArgumentNullException.ThrowIfNull(preparedSources);
        cancellationToken.ThrowIfCancellationRequested();
        if (preparedSources.Count > 256)
            throw new ArgumentException("Prepared source chain exceeds the bounded controller scope.");
        symbolCount = acousticAlphabet.LabelCount;
        producer = new(acousticAlphabet, serializedAlphabetHeader);
        sources = new LegacyLinguisticSymbolSource[preparedSources.Count];
        int entries = 0;
        for (int i = 0; i < sources.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = preparedSources[i]; ArgumentNullException.ThrowIfNull(source);
            if (source.Tokens.Length > 1_048_576 - entries || source.DictionaryMap.Length > ushort.MaxValue)
                throw new ArgumentException("Prepared linguistic source exceeds bounded dimensions.");
            entries += source.Tokens.Length;
            // Validate mapped tokens even when a later caller bypasses this controller.
            if (!source.DictionaryMap.IsEmpty)
                foreach (uint token in source.Tokens.Span)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if ((token & 0xffffu) >= source.DictionaryMap.Length)
                        throw new ArgumentException("Source token is outside its prepared dictionary map.");
                }
            sources[i] = new(source.Tokens.ToArray(), source.DictionaryMap.ToArray(), source.Completed);
        }
    }

    /// <summary>
    /// Text151ce0/1c0ce0 attach the controller;1b8850 invokes it only when not
    /// bypassed. Preparation failure and failed enumeration use native fallback.
    /// No default preparation success or default application bypass is supplied.
    /// </summary>
    public LegacyPreparedOrdinaryLinguisticMaskResult Build(bool languageControllerBypassed,
        bool preparationSucceeded, IReadOnlyList<LegacySymbolMaskKnowledgeGroup> knowledgeGroups,
        ReadOnlySpan<byte> packedUserRestriction, ReadOnlySpan<LegacyContextualSymbolForms> contextualForms,
        ReadOnlySpan<LegacyAcousticCachePair> widePairs, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var seed = languageControllerBypassed
            ? new LegacyLinguisticSymbolMaskSeed(LegacySymbolMaskControllerOutcome.UnboundOrBypassed,
                ReadOnlyMemory<byte>.Empty)
            : LegacyLinguisticSymbolMaskController.Build(symbolCount, sources, preparationSucceeded, cancellationToken);
        return new(seed.Outcome, producer.Build(seed.Outcome, seed.Mask.Span, knowledgeGroups,
            packedUserRestriction, contextualForms, widePairs, cancellationToken));
    }
}
