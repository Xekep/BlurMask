namespace InkRecognition;

/// <summary>The only possible2271F0 terminal invocation for the owned fresh
/// DIRECT/no-property result route00ED90/00F460. Native param7 is true, but
/// its rescue body is unreachable under this owner's actual22E8 flags.
/// Candidate rows, source-end extent and2520 cumulative storage still require
/// their own native producers. No case-aware or reused SDK context is accepted.</summary>
public sealed class LegacyOwnedFreshMainTerminalInvocation
{
    public LegacyOwnedFreshMainWordControllerPolicy ControllerOwner { get; }
    public LegacyOwnedRussianTerminalSources SourceOwner { get; }
    public LegacyPreparedMainTerminalOptions Options { get; }
    public bool NativeRescueRequested7 => true;
    public bool NativeRescueGateActive => false;
    public int AlternativeIndex => 0;
    public int MaximumInvocationCount => 1;

    private LegacyOwnedFreshMainTerminalInvocation(LegacyOwnedFreshMainWordControllerPolicy owner,
        LegacyOwnedRussianTerminalSources sources, LegacyPreparedMainTerminalOptions options)
    { ControllerOwner = owner; SourceOwner = sources; Options = options; }

    public static LegacyOwnedFreshMainTerminalInvocation Prepare(
        LegacyOwnedFreshMainWordControllerPolicy actualPolicy,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(actualPolicy); cancellationToken.ThrowIfCancellationRequested();
        var owner = actualPolicy.Owner; var configuration = actualPolicy.Configuration;
        if (owner.IsCaseAware || configuration.Mode22f6 != 0 ||
            configuration.SecondaryAlternativeCount2404 != 1 || actualPolicy.SecondaryScaleEc != 0 ||
            (configuration.PackedSource22e8 & 0x20) != 0)
            throw new NotSupportedException("Owned fresh DIRECT mode0/secondary1/EC0 with inactive rescue gate required.");
        var model = owner.DictionaryOwner.Model;
        var sources = LegacyOwnedRussianTerminalSources.Prepare(owner.DictionaryOwner, cancellationToken);
        //20C1C0's24B8 reads fresh ownerC4 (zero), source kind and exactAL.
        bool sentinels = model.SourceKind == 0x137 || model.AcousticAlphabet.TryGetLabel("\U000f0009", out _);
        //00C860 clears the complete98..9F word. Without external properties,
        //00ED90 forwards9C=0 and literalNULL to00F460;00F460 forwards those
        //to2271F0. Its V240 conditional picks either0 or caller0 forFilter4.
        //EC0 initializes ResultFlag8=0;2257C0 can only clear it. Thus the
        //secondary1 loop executes index0 once, withRequire6=false,param7=true.
        var options = new LegacyPreparedMainTerminalOptions(configuration.PrimaryArcLimit230a,
            sentinels, false, false, false, 0);
        cancellationToken.ThrowIfCancellationRequested();
        return new(actualPolicy, sources, options);
    }

    /// <summary>Apply the proven single invocation to explicit prepared source
    /// rows and the actual2520 value. This method does not manufacture either
    /// input or claim that an arbitrary row buffer came from the native loop.</summary>
    public LegacyPreparedMainTerminalWinner SelectPreparedRows(ReadOnlySpan<byte> records88,
        ushort sourceEnd23f0, int cumulativePathCost2520, CancellationToken cancellationToken = default)
        => LegacyNativeMainTerminalSelection.SelectSingleAlternativeWithoutRescue(records88,
            sourceEnd23f0, SourceOwner.CopySources(), Options, cumulativePathCost2520,
            ControllerOwner.Owner.Contexts, cancellationToken);
}
