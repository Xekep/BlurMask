using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>
/// Owned actual-RU dictionary preparation and its ordinary mask binding.
/// This constructs portable dictionary/context/language objects; it is not
/// the complete native240 factory, decorator290 query engine or refresh epoch.
/// </summary>
public sealed class LegacyOwnedRussianDictionaryPreparation
{
    private readonly LegacyFreshSingleGlyphMainSampled source;
    private readonly LegacyAcousticSymbolMasks masks;
    public LegacyRussianModelBundle Model { get; }
    public LegacySymbolContextFactory Contexts { get; }
    public LegacyDictionaryInitialSymbolMaps Symbols { get; }
    public LegacyPreparedDictionarySourceDescriptor Descriptor { get; }
    public IReadOnlyList<LegacyPreparedSelectedDictionarySource> SelectedSources { get; }
    public LegacyPreparedAggregateDictionaryQuerySource DictionaryQueries { get; }
    public LegacyPreparedDictionaryWordMetadata WordMetadata { get; }
    public LegacyNativeKnownWordLanguageRecord LanguageRecords { get; }
    public LegacyPreparedDictionaryRestartTags RestartTags { get; }
    public LegacyPreparedLinguisticSourceSummary SourceSummary { get; }
    public uint[] CopySourceHeaders() => [0, Descriptor.Flags];
    public LegacyAcousticSymbolMasks CopyMasks() => new(masks.Primary.ToArray(), masks.Secondary.ToArray());

    private LegacyOwnedRussianDictionaryPreparation(LegacyRussianModelBundle model,
        LegacyFreshSingleGlyphMainSampled source, LegacySymbolContextFactory contexts,
        LegacyDictionaryInitialSymbolMaps symbols, LegacyPreparedDictionarySourceDescriptor descriptor,
        IReadOnlyList<LegacyPreparedSelectedDictionarySource> selected,
        LegacyPreparedAggregateDictionaryQuerySource queries, LegacyPreparedDictionaryWordMetadata metadata,
        LegacyNativeKnownWordLanguageRecord language, LegacyPreparedDictionaryRestartTags restart,
        LegacyPreparedLinguisticSourceSummary summary, LegacyAcousticSymbolMasks masks)
    {
        Model = model; this.source = source; Contexts = contexts; Symbols = symbols; Descriptor = descriptor;
        SelectedSources = selected; DictionaryQueries = queries; WordMetadata = metadata;
        LanguageRecords = language; RestartTags = restart; SourceSummary = summary; this.masks = masks;
    }

    /// <summary>
    /// Build an actual selected dictionary, its three maps and shared contexts,
    /// resource-selected language profile and source descriptor. Both pinned
    /// resources are checked; the exact maps need no normalization fallback.
    /// The prepared normal ordinary mask branch has no caller-provided success
    /// flag, fabricated wildcard token, language alias or script replacement.
    /// Main live controller construction/configuration remains a separate stage.
    /// </summary>
    public static LegacyOwnedRussianDictionaryPreparation Prepare(ReadOnlySpan<byte> acousticResource,
        ReadOnlySpan<byte> lexicalResource, LegacyRussianModelBundle selectedModel,
        LegacyFreshSingleGlyphMainSampled sampledSource, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(selectedModel); ArgumentNullException.ThrowIfNull(sampledSource);
        cancellationToken.ThrowIfCancellationRequested();
        if (acousticResource.Length != 572763 || lexicalResource.Length != 6200992)
            throw new InvalidDataException("The actual Russian resource pair has fixed validated lengths.");
        string acousticHash = Convert.ToHexString(SHA256.HashData(acousticResource));
        if (acousticHash != "7A5F08809E81FB4A2D4C78B3EFD251D0986503B0D30D21DE3695DAB1F8A89978" ||
            !SHA256.HashData(lexicalResource).AsSpan().SequenceEqual(
                Convert.FromHexString("d26ed08baf419361484958b3a04a5f740a228f4664f7e2643f43459223ef3e4e")))
            throw new InvalidDataException("The actual owned Russian resource pair is required.");
        if (sampledSource.Owner.CategoryOwner.SelectedSourceOffset != 0 ||
            !StringComparer.OrdinalIgnoreCase.Equals(sampledSource.Owner.CategoryOwner.SourceFingerprint, acousticHash) ||
            sampledSource.Owner.SpanProfile != selectedModel.CursiveSpanProfile || selectedModel.SourceKind != 0x135)
            throw new ArgumentException("Dictionary preparation must bind the actual sampled main0 owner.");
        var prefix = LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lexicalResource.Slice(606964, 4565174 - 606964));
        uint order = LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lexicalResource.Slice(2513123, 4565174 - 2513123));
        int profile = selectedModel.ResourceLanguageProfileIndex;
        var language = new LegacyNativeKnownWordLanguageRecord(selectedModel.LanguageModel,
            selectedModel.LanguageProfiles[profile], selectedModel.AuxiliaryProfiles[profile], prefix.RuntimeLanguageScale);
        var descriptor = LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(prefix, selectedModel.Lexicon, order, true, cancellationToken);
        var contexts = new LegacySymbolContextFactory();
        var symbols = LegacyDictionaryInitialSymbolMaps.Prepare(selectedModel.AcousticAlphabet,
            selectedModel.LexicalAlphabet, contexts, [], cancellationToken);
        if (symbols.CopyPrimary().Any(x => x is ushort.MaxValue or 0xfffd))
            throw new NotSupportedException("Exact supported Russian map must not need normalization fallback.");
        var selection = new LegacyPreparedDictionarySourceSelectionBuilder();
        selection.Add(LegacyPreparedDictionarySourceSelectionInput.FromObservedOrderTwoModel(descriptor, selectedModel.LanguageModel, contexts.ContainerIdentity));
        var selected = selection.Prepare(cancellationToken);
        var dictionary = symbols.CreateQuerySource(selectedModel.Lexicon, selected[0].Descriptor.Flags);
        var child = LegacyOrdinaryDictionarySourceOwner.BindDictionary(dictionary, contexts, sampledSource.SourceRanges.Graph);
        var queries = new LegacyPreparedAggregateDictionaryQuerySource([child]);
        var binding = new LegacyPreparedOrdinaryLinguisticMaskBinding(selectedModel.AcousticAlphabet,
            acousticResource.Slice(99, 16), [symbols.CreateSymbolSource()], cancellationToken);
        var knowledge = LegacyOrdinarySymbolMaskProducer.ReadV110KnowledgeGroups(acousticResource[189734..],
            selectedModel.AcousticAlphabet.LabelCount, out _, cancellationToken);
        var masks = binding.Build(false, true, knowledge, [], [],
            LegacyWideSymbolPairs.CreateUnrestricted(selectedModel.AcousticAlphabet), cancellationToken);
        if (masks.ControllerOutcome != LegacySymbolMaskControllerOutcome.Succeeded)
            throw new InvalidDataException("Owned dictionary enumeration did not produce an accepted mask.");
        //Fresh235F30 initializes implicit descriptor10 header0/weights0,1.
        //Its index0 is distinct from this single selected dictionary's source1.
        var summary = LegacyPreparedLinguisticRefreshPolicy.Summarize([0, selected[0].Descriptor.Flags],
            false, false, -1, -1, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return new(selectedModel, sampledSource, contexts, symbols, selected[0].Descriptor, selected,
            queries, new(selectedModel.Lexicon, selectedModel.WordIndex), language,
            LegacyPreparedDictionaryRestartTags.CreateFresh(), summary, masks.Masks);
    }

    public LegacyPreparedFreshMain240AcousticScorer CreateAcousticScorer(ReadOnlySpan<byte> actualResource,
        CancellationToken cancellationToken = default)
        => LegacyPreparedFreshMain240AcousticScorer.Prepare(actualResource, Model, source, masks, cancellationToken);
}
