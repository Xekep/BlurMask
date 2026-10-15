namespace InkRecognition;

/// <summary>Owned ordinary Russian source descriptors and0FE2A0's2534
/// penalty for2257C0. This does not select terminal options, construct final
/// word rows or run the optional rescue/multiple-alternative branches.</summary>
public sealed class LegacyOwnedRussianTerminalSources
{
    private readonly LegacyPreparedTerminalSource[] sources;
    public LegacyOwnedRussianDictionaryPreparation DictionaryOwner { get; }
    public int StoredFinalUnigramLevel { get; }
    public int LanguageProfileIndex { get; }
    public float RuntimeLanguageScale { get; }
    public int LanguagePenalty2534 { get; }

    private LegacyOwnedRussianTerminalSources(LegacyOwnedRussianDictionaryPreparation owner,
        int level, int index, float scale, int penalty)
    {
        DictionaryOwner = owner; StoredFinalUnigramLevel = level;
        LanguageProfileIndex = index; RuntimeLanguageScale = scale;
        LanguagePenalty2534 = penalty;
        //Fresh235F30's implicit source0 has no language binding/order bits.
        //2257C0 consequently never calls its2534 getter; the zero field is
        //unused storage, not an inferred language penalty for that source.
        sources = [new(0, 0), new(owner.Descriptor.Flags, penalty)];
    }

    /// <summary>Use the checked selected model's actual source-kind profile
    /// and the descriptor's loaded139 runtime scale. Pointer ownership is
    /// represented by object identity, not reconstructed native addresses.</summary>
    public static LegacyOwnedRussianTerminalSources Prepare(
        LegacyOwnedRussianDictionaryPreparation actualDictionary,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actualDictionary);
        cancellationToken.ThrowIfCancellationRequested();
        var model = actualDictionary.Model;
        int index = model.ResourceLanguageProfileIndex;
        if (model.SourceKind != 0x135 || index != 2 ||
            !actualDictionary.LanguageRecords.IsForModel(model) ||
            actualDictionary.SelectedSources.Count != 1 ||
            !ReferenceEquals(actualDictionary.Descriptor, actualDictionary.SelectedSources[0].Descriptor) ||
            actualDictionary.Descriptor.LanguageOrder != 2 ||
            (actualDictionary.Descriptor.Flags & 0x1c) != 8 ||
            (actualDictionary.Descriptor.Flags & 2) == 0)
            throw new ArgumentException("The actual owned order2 Russian dictionary/language binding is required.");
        float scale = actualDictionary.Descriptor.RuntimeLanguageScale;
        int penalty = PreparePositiveScalePenalty(model.LanguageModel, model.LanguageProfiles[index], scale,
            cancellationToken);
        return new(actualDictionary, model.LanguageModel.GetStoredFinalClassLevel(false), index, scale, penalty);
    }

    /// <summary>0FE2A0's strictly positive finite scale branch. D0[0] is
    /// the unigram codebook count and D8[0] owns its levels. The final level
    /// is converted with local128[0]/local100[0], not the word coefficients.
    /// Zero/negative scales and other native storage formats are separate.</summary>
    public static int PreparePositiveScalePenalty(LegacyWordLanguageModel model,
        LegacyWordLanguageCostProfile selectedProfile, float runtimeScale,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(model); ArgumentNullException.ThrowIfNull(selectedProfile);
        cancellationToken.ThrowIfCancellationRequested();
        int result = selectedProfile.TransformClassCost(model.GetStoredFinalClassLevel(false), false, runtimeScale);
        cancellationToken.ThrowIfCancellationRequested();
        return result;
    }

    public LegacyPreparedTerminalSource[] CopySources() => (LegacyPreparedTerminalSource[])sources.Clone();
}
