namespace InkRecognition;

/// <summary>One numeric2257C0 choice retaining its actual configured pass,
/// stored88 and secondary40 identity. Compact indices are raw/prepublication.</summary>
public sealed class LegacyOwnedMainSecondaryTerminalChoice
{
    private readonly byte[] row88, alternative40;
    public LegacyOwnedMainCaseWordPassResult PassResult { get; }
    public LegacyPreparedMainSecondaryTerminalWinner Selection { get; }
    public int SelectionCall { get; }
    internal LegacyOwnedMainSecondaryTerminalChoice(LegacyOwnedMainCaseWordPassResult pass,
        LegacyPreparedMainSecondaryTerminalWinner selection, int call, byte[] row, byte[] alternative)
    { PassResult = pass; Selection = selection; SelectionCall = call; row88 = row; alternative40 = alternative; }
    public byte[] CopyStoredRow88() => row88.ToArray();
    public byte[] CopySecondaryAlternative40() => alternative40.ToArray();
}

/// <summary>Actual owned configured2 matrix to2257C0 exclusions. This owns
/// only terminal selection state, not the accepted-output counter, duplicate
/// callbacks, rescue or a273 alternative/WORD publication loop.</summary>
public sealed class LegacyOwnedMainSecondaryTerminalCursor
{
    private readonly object gate = new();
    private readonly byte[] records, secondary;
    private readonly LegacyPreparedTerminalSource[] sources;
    private readonly List<ulong> exclusions = [];
    private bool exhausted;
    public LegacyOwnedMainCaseWordPassResult PassResult { get; }
    public LegacyFreshCaseMainSourcePolicy SourcePolicy => PassResult.SourcePolicy;
    public LegacyPreparedMainTerminalOptions Options { get; }
    public int ExclusionCapacity => PassResult.Storage.FinalSourceEnd;
    public int ConfiguredOutputLimit => SourcePolicy.Controller.Configuration.SecondaryAlternativeCount2404;
    public bool IsExhausted { get { lock (gate) return exhausted; } }
    private LegacyOwnedMainSecondaryTerminalCursor(LegacyOwnedMainCaseWordPassResult pass,
        byte[] records, byte[] secondary, LegacyPreparedTerminalSource[] sources,
        LegacyPreparedMainTerminalOptions options)
    { PassResult = pass; this.records = records; this.secondary = secondary; this.sources = sources; Options = options; }

    public static LegacyOwnedMainSecondaryTerminalCursor Prepare(LegacyOwnedMainCaseWordPassResult actualResult,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualResult); token.ThrowIfCancellationRequested();
        var source = actualResult.SourcePolicy;
        var controller = source.Controller;
        var model = source.Model;
        var dictionary = source.DictionaryOwner;
        var configuration = controller.Configuration;
        if (!ReferenceEquals(actualResult.Storage.ControllerOwner, controller) ||
            !ReferenceEquals(controller.Owner.CaseDictionaryOwner, dictionary) ||
            !ReferenceEquals(model, controller.Owner.CommonModel) ||
            !ReferenceEquals(source.Graph, controller.Owner.SourceGraph) || actualResult.NodeCount != source.Graph.NodeCount)
            throw new ArgumentException("One actual configured CASE pass/controller/model/graph required.");
        if (controller.SecondaryCountProperty12d0003 != 2 || configuration.SecondaryAlternativeCount2404 != 2 ||
            configuration.Mode22f6 != 0 || configuration.Trace2368 != 0 || controller.SecondaryScaleEc != 0 ||
            (configuration.PackedSource22e8 & 0x20) != 0 || source.HasAssociatedSource ||
            dictionary.IgnoreCaseC4 || dictionary.CaptureSideCosts2368)
            throw new NotSupportedException("Known preconstruction count2, no-rescue ordinary CASE source required.");
        int index = model.ResourceLanguageProfileIndex;
        if (model.SourceKind != 0x135 || index != 2 || dictionary.SelectedSources.Count != 1 ||
            !ReferenceEquals(dictionary.Descriptor, dictionary.SelectedSources[0].Descriptor) ||
            dictionary.Descriptor.LanguageOrder != 2 || (dictionary.Descriptor.Flags & 0x1c) != 8)
            throw new ArgumentException("The exact owned Russian source descriptor and language profile required.");
        actualResult.Storage.ValidateResultQueryEpoch(token);
        int penalty = LegacyOwnedRussianTerminalSources.PreparePositiveScalePenalty(model.LanguageModel,
            model.LanguageProfiles[index], dictionary.Descriptor.RuntimeLanguageScale, token);
        bool sentinels = model.SourceKind == 0x137 || model.AcousticAlphabet.TryGetLabel("\U000f0009", out _);
        var options = new LegacyPreparedMainTerminalOptions(configuration.PrimaryArcLimit230a, sentinels, false, false, false, 0);
        var final = actualResult.GetNodeSnapshot(actualResult.NodeCount - 1);
        if (final.SecondaryAlternatives.Length != final.Records.Length / 88 * 80)
            throw new ArgumentException("Owned configured2410 extent must match the actual final88 allocation.");
        token.ThrowIfCancellationRequested();
        return new(actualResult, final.Records.ToArray(), final.SecondaryAlternatives.ToArray(),
            [new(0, 0), new(dictionary.Descriptor.Flags, penalty)], options);
    }

    /// <summary>Appends the actual mapped identity08 to the native exclusion
    /// vector only after a successful choice. Returns null on native no-winner
    /// or the actual source-row exclusion budget. Does not accept a WORD.</summary>
    public LegacyOwnedMainSecondaryTerminalChoice? SelectNext(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        lock (gate)
        {
            if (exhausted) throw new InvalidOperationException("This native terminal exclusion cursor is exhausted.");
            PassResult.Storage.ValidateResultQueryEpoch(token);
            if (exclusions.Count == ExclusionCapacity) { exhausted = true; return null; }
            var result = LegacyNativeMainSecondaryTerminalSelection.SelectWithoutRescue(records,
                PassResult.Storage.FinalSourceEnd, secondary, 2, sources, Options,
                PassResult.Storage.FinalCumulativeCost, SourcePolicy.DictionaryOwner.Contexts, exclusions.ToArray(), token);
            if (!result.Winner.HasWinner) { exhausted = true; return null; }
            byte[] row = records.AsSpan(result.Winner.Ordinal * 88, 88).ToArray();
            byte[] alternative = secondary.AsSpan((result.Winner.Ordinal * 2 + result.SecondaryOrdinal) * 40, 40).ToArray();
            token.ThrowIfCancellationRequested();
            var choice = new LegacyOwnedMainSecondaryTerminalChoice(PassResult, result, exclusions.Count, row, alternative);
            exclusions.Add(result.ExclusionIdentity); return choice;
        }
    }
    public ulong[] CopyExclusions() { lock (gate) return exclusions.ToArray(); }
}
