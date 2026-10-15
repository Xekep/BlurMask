namespace InkRecognition;

/// <summary>The fresh no-associated292 source's single2271F0 invocation.
/// Owns the actual selected dictionary descriptor and2534 language penalty;
/// it does not manufacture a parallel DIRECT dictionary or publish text.</summary>
public sealed class LegacyOwnedFreshCaseMainTerminalInvocation
{
    private readonly LegacyPreparedTerminalSource[] sources;
    public LegacyFreshCaseMainSourcePolicy SourcePolicy { get; }
    public LegacyOwnedFreshMainWordControllerPolicy ControllerOwner => SourcePolicy.Controller;
    public LegacyFreshCaseRussianDictionaryPreparation DictionaryOwner => SourcePolicy.DictionaryOwner;
    public LegacyPreparedMainTerminalOptions Options { get; }
    public int LanguagePenalty2534 { get; }
    public bool NativeRescueRequested7 => true;
    public bool NativeRescueGateActive => false;
    public int AlternativeIndex => 0;
    public int MaximumInvocationCount => 1;

    private LegacyOwnedFreshCaseMainTerminalInvocation(LegacyFreshCaseMainSourcePolicy policy,
        LegacyPreparedMainTerminalOptions options, int penalty)
    {
        SourcePolicy = policy; Options = options; LanguagePenalty2534 = penalty;
        //Fresh235F30's source0 is implicit. Its header has no language bits,
        //so2257C0 never asks it for a penalty. The selected source keeps the
        //actual case factory's descriptor, including its native order flags.
        sources = [new(0, 0), new(policy.DictionaryOwner.Descriptor.Flags, penalty)];
    }

    public static LegacyOwnedFreshCaseMainTerminalInvocation Prepare(
        LegacyFreshCaseMainSourcePolicy actualPolicy, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actualPolicy);
        cancellationToken.ThrowIfCancellationRequested();
        var controller = actualPolicy.Controller;
        var owner = controller.Owner;
        var dictionary = actualPolicy.DictionaryOwner;
        var model = dictionary.Model;
        var configuration = controller.Configuration;
        if (!ReferenceEquals(owner.CaseDictionaryOwner, dictionary) ||
            !ReferenceEquals(owner.CommonModel, model) || !ReferenceEquals(owner.SourceGraph, dictionary.Graph) ||
            !ReferenceEquals(owner.Contexts, dictionary.Contexts))
            throw new ArgumentException("One actual case source/model/graph/context owner required.");
        if (configuration.Mode22f6 != 0 || configuration.SecondaryAlternativeCount2404 != 1 ||
            configuration.Trace2368 != 0 || controller.SecondaryScaleEc != 0 ||
            (configuration.PackedSource22e8 & 0x20) != 0 || actualPolicy.HasAssociatedSource ||
            dictionary.IgnoreCaseC4 || dictionary.CaptureSideCosts2368)
            throw new NotSupportedException("Fresh case mode0/secondary1/EC0 with inactive rescue and no associated source required.");
        int index = model.ResourceLanguageProfileIndex;
        if (model.SourceKind != 0x135 || index != 2 || !dictionary.LanguageRecords.IsForModel(model) ||
            dictionary.SelectedSources.Count != 1 ||
            !ReferenceEquals(dictionary.Descriptor, dictionary.SelectedSources[0].Descriptor) ||
            dictionary.Descriptor.LanguageOrder != 2 || (dictionary.Descriptor.Flags & 0x1c) != 8 ||
            (dictionary.Descriptor.Flags & 2) == 0)
            throw new ArgumentException("Actual owned order2 Russian selected language binding required.");
        int penalty = LegacyOwnedRussianTerminalSources.PreparePositiveScalePenalty(model.LanguageModel,
            model.LanguageProfiles[index], dictionary.Descriptor.RuntimeLanguageScale, cancellationToken);
        //FreshC4=0.00ED90 passes9C=0/NULL through00F460, so2271F0.Filter4
        //is0 regardless of frontendV240. EC0 initializesResultFlag8=0; the
        //secondary1 loop therefore runs index0 once withRequire6=false and
        //param7=true. Packed22E8.bit20 is clear, disabling its rescue body.
        bool sentinels = model.SourceKind == 0x137 || model.AcousticAlphabet.TryGetLabel("\U000f0009", out _);
        var options = new LegacyPreparedMainTerminalOptions(configuration.PrimaryArcLimit230a,
            sentinels, false, false, false, 0);
        cancellationToken.ThrowIfCancellationRequested();
        return new(actualPolicy, options, penalty);
    }

    /// <summary>Select prepublication numeric rows from this exact case pass.
    /// Row22 remains raw until207CB0; this is not a text result.</summary>
    public LegacyOwnedMainTerminalResult SelectOwnedPass(LegacyOwnedMainCaseWordPassResult result,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(result);
        cancellationToken.ThrowIfCancellationRequested();
        if (!ReferenceEquals(result.SourcePolicy, SourcePolicy) ||
            !ReferenceEquals(result.Storage.ControllerOwner, ControllerOwner))
            throw new ArgumentException("Case invocation and completed storage must have the same actual source/controller owner.");
        var storage = result.Storage;
        var winner = LegacyNativeMainTerminalSelection.SelectSingleAlternativeWithoutRescue(
            storage.FinalRecords, storage.FinalSourceEnd, sources, Options,
            storage.FinalCumulativeCost, DictionaryOwner.Contexts, cancellationToken);
        return LegacyOwnedMainTerminalResult.Create(storage, winner, cancellationToken);
    }

    public LegacyPreparedTerminalSource[] CopySources() => (LegacyPreparedTerminalSource[])sources.Clone();
}
