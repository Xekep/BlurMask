using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Fresh main247/240 scalar policy bound to the actual shared global
/// source, its one292 dictionary session, class/prototype costs and118/140.
/// No single-descriptor policy or configured/reused native owner is fabricated.</summary>
public sealed class LegacyFreshMainGlobalWordControllerPolicy
{
    public LegacyFreshMainGlobalCaseSession Owner {get;}
    public LegacyNativeWordControllerConfiguration Configuration {get;}
    public LegacyNativeWordScalarRefreshResult Refresh {get;}
    public int MaximumParentDistance54=>120;
    public int FlagPrepassDistance58 {get;}
    public bool KnowledgeF5=>false;
    public float SecondaryScaleEc=>0;
    public bool HasAssociatedSource=>false;
    public bool SchedulesSecondaryGroup1Fe740=>false;
    internal LegacyOwnedMainWordSourceServices Services {get;}
    private LegacyFreshMainGlobalWordControllerPolicy(LegacyFreshMainGlobalCaseSession owner,
        LegacyNativeWordControllerConfiguration configuration,LegacyNativeWordScalarRefreshResult refresh,
        LegacyOwnedMainWordSourceServices services)
    {Owner=owner;Configuration=configuration;Refresh=refresh;Services=services;FlagPrepassDistance58=owner.Acoustic.Policy.SourceAlternativeLimit;}
    public static LegacyFreshMainGlobalWordControllerPolicy Prepare(ReadOnlySpan<byte> resource,
        LegacyFreshMainGlobalCaseSession owner,LegacyFreshMainGlobalGroupCosts groupCosts,
        LegacyFreshMainGlobalWordTransitions transitions,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(owner);token.ThrowIfCancellationRequested();
        var source=owner.Graph.Source.Source;
        if(resource.Length!=572763||source.CategoryOwner.SelectedSourceOffset!=0||owner.Model.SourceKind!=0x135||
            !Convert.ToHexString(SHA256.HashData(resource)).Equals(source.SourceFingerprint,StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Actual global main0 resource/model/source required.");
        var services=LegacyOwnedMainWordSourceServices.FromGlobal(owner,groupCosts,transitions);
        if(owner.Symbols.SelectedSources.Count!=1||owner.Symbols.ResourcePrefix.Identifier!="ru_RU-lk-text.lite.lex"||
            owner.Symbols.ResourcePrefix.Field68!=1||services.CopySourceHeaders() is not[0,var header]||
            (header&0x1e00)!=0xc00||!owner.Symbols.LanguageRecords.IsForModel(owner.Model))
            throw new NotSupportedException("Actual fresh implicit0 and one order2/C00 selected292 source required.");
        var sourceProfile=LegacyOrdinarySourceInk.ReadV10dResourceProfile(resource,0);
        byte consecutive=resource[sourceProfile.ParametersOffset-6],format=resource[sourceProfile.ParametersOffset-5];
        if(consecutive>1||format>1)throw new InvalidDataException("Validated247B8/B9 flags required.");
        bool marker=owner.Model.AcousticAlphabet.TryGetLabel("\u202b",out _);
        var refresh=LegacyNativeWordScalarRefresh.Apply(0,services.SourceSummary.Flags,false,consecutive,0,format,-1,0,marker);
        var fresh=LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with{PackedSource22e8=refresh.PackedSource22e8};
        var prepared=LegacyNativeWordControllerLimitPreparation.Prepare(fresh,
            checked((uint)owner.Acoustic.Policy.MaximumSourceArcs),LegacyNativeWordKnowledgeLimits.CreateFresh00c860(),true);
        token.ThrowIfCancellationRequested();return new(owner,prepared.Configuration,refresh,services);
    }
    public bool AllowsParent(int currentEndingNode,int actualAnchorEndingNode,byte actualParentState21)
        =>unchecked((sbyte)actualParentState21)>=0&&
            (unchecked(currentEndingNode-actualAnchorEndingNode)<=MaximumParentDistance54||Configuration.SuppressPreviousContext2609!=0);
}
