namespace InkRecognition;

/// <summary>Actual fresh global one292/no-associated terminal invocation.
/// It selects numeric prepublication rows; row22 is not a compact record ID.</summary>
public sealed partial class LegacyFreshMainGlobalTerminalInvocation
{
    private readonly LegacyPreparedTerminalSource[] sources;
    public LegacyFreshMainGlobalWordControllerPolicy Policy{get;}
    public LegacyPreparedMainTerminalOptions Options{get;}
    public int LanguagePenalty2534{get;}
    private LegacyFreshMainGlobalTerminalInvocation(LegacyFreshMainGlobalWordControllerPolicy policy,
        LegacyPreparedMainTerminalOptions options,int penalty)
    {Policy=policy;Options=options;LanguagePenalty2534=penalty;sources=[new(0,0),new(policy.Owner.Symbols.Descriptor.Flags,penalty)];}
    public static LegacyFreshMainGlobalTerminalInvocation Prepare(LegacyFreshMainGlobalWordControllerPolicy policy,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(policy);token.ThrowIfCancellationRequested();
        var owner=policy.Owner;var model=owner.Model;var descriptor=owner.Symbols.Descriptor;var c=policy.Configuration;
        if(c.Mode22f6!=0||c.SecondaryAlternativeCount2404!=1||c.Trace2368!=0||policy.SecondaryScaleEc!=0||
            (c.PackedSource22e8&0x20)!=0||policy.HasAssociatedSource||model.SourceKind!=0x135||
            model.ResourceLanguageProfileIndex!=2||!owner.Symbols.LanguageRecords.IsForModel(model)||
            owner.Symbols.SelectedSources.Count!=1||!ReferenceEquals(descriptor,owner.Symbols.SelectedSources[0].Descriptor)||
            descriptor.LanguageOrder!=2||(descriptor.Flags&0x1c)!=8||(descriptor.Flags&2)==0)
            throw new NotSupportedException("Actual fresh global mode0/secondary1/EC0/order2 with inactive rescue required.");
        int penalty=LegacyOwnedRussianTerminalSources.PreparePositiveScalePenalty(model.LanguageModel,
            model.LanguageProfiles[model.ResourceLanguageProfileIndex],descriptor.RuntimeLanguageScale,token);
        bool sentinels=model.SourceKind==0x137||model.AcousticAlphabet.TryGetLabel("\U000f0009",out _);
        token.ThrowIfCancellationRequested();
        return new(policy,new(c.PrimaryArcLimit230a,sentinels,false,false,false,0),penalty);
    }
    public LegacyFreshMainGlobalTerminalResult Select(LegacyFreshMainGlobalWordPassResult result,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(result);token.ThrowIfCancellationRequested();
        if(!ReferenceEquals(result.Policy,Policy))throw new ArgumentException("Actual global pass/controller identity required.");
        result.ValidateResultQueryLease(token);
        var winner=LegacyNativeMainTerminalSelection.SelectSingleAlternativeWithoutRescue(result.FinalRecords,
            result.FinalSourceEnd,sources,Options,result.FinalCumulativeCost,Policy.Owner.Contexts,token);
        byte[] selected=winner.HasWinner?result.FinalRecords.Slice(checked(winner.Ordinal*88),88).ToArray():[];
        result.ValidateResultQueryLease(token);return new(this,result,winner,selected);
    }
    public LegacyPreparedTerminalSource[] CopySources()=>sources.ToArray();
}

/// <summary>Owned global numeric selection, distinct from old concrete results
/// and from compact/text publication or acceptance confidence.</summary>
public sealed class LegacyFreshMainGlobalTerminalResult
{
    private readonly byte[] selected;
    public LegacyFreshMainGlobalTerminalInvocation Invocation{get;}
    public LegacyFreshMainGlobalWordPassResult PassResult{get;}
    public LegacyPreparedMainTerminalWinner Winner{get;}
    internal LegacyFreshMainGlobalTerminalResult(LegacyFreshMainGlobalTerminalInvocation invocation,
        LegacyFreshMainGlobalWordPassResult result,LegacyPreparedMainTerminalWinner winner,byte[] selected)
    {Invocation=invocation;PassResult=result;Winner=winner;this.selected=selected;}
    public byte[] CopySelectedRecord()=>selected.ToArray();
}
