using System.Security.Cryptography;
namespace InkRecognition;

/// <summary>Fresh unmodified ordinary Russian20C9D0 selection:2B7→292, one
/// selected dictionary. Builds one shared backend for enumeration/permissions,
/// case-aware normal/aux queries, terminal metadata and restart. No reused
/// container properties or geometry-capture events are accepted by this API.</summary>
public sealed class LegacyFreshCaseRussianDictionaryPreparation
{
    private readonly MainSource source;
    private readonly LegacyAcousticSymbolMasks masks;
    public LegacyRussianModelBundle Model {get;}
    public LegacyPreparedCaseDictionarySymbolBinding Symbols {get;}
    public LegacyPreparedCaseDictionarySession Session {get;}
    public LegacySymbolContextFactory Contexts=>Symbols.Symbols.Contexts;
    public LegacyOrdinaryCursiveCandidateGraph Graph=>source.Graph;
    public LegacyPreparedDictionarySourceDescriptor Descriptor {get;}
    public IReadOnlyList<LegacyPreparedSelectedDictionarySource> SelectedSources {get;}
    public LegacyPreparedLinguisticSourceSummary SourceSummary {get;}
    public LegacyNativeKnownWordLanguageRecord LanguageRecords {get;}
    public uint ContainerCaseMode90=>uint.MaxValue;
    public bool IgnoreCaseC4=>false;
    public bool CaptureSideCosts2368=>false;
    public uint[] CopySourceHeaders()=>[0,Descriptor.Flags];
    public LegacyAcousticSymbolMasks CopyMasks()=>new(masks.Primary.ToArray(),masks.Secondary.ToArray());

    private LegacyFreshCaseRussianDictionaryPreparation(LegacyRussianModelBundle model,MainSource source,
        LegacyPreparedCaseDictionarySymbolBinding symbols,LegacyPreparedCaseDictionarySession session,
        LegacyPreparedDictionarySourceDescriptor descriptor,IReadOnlyList<LegacyPreparedSelectedDictionarySource> selected,
        LegacyPreparedLinguisticSourceSummary summary,LegacyNativeKnownWordLanguageRecord language,LegacyAcousticSymbolMasks masks)
    {Model=model;this.source=source;Symbols=symbols;Session=session;Descriptor=descriptor;SelectedSources=selected;SourceSummary=summary;LanguageRecords=language;this.masks=masks;}

    public static LegacyFreshCaseRussianDictionaryPreparation Prepare(ReadOnlySpan<byte> acousticResource,
        ReadOnlySpan<byte> lexicalResource,LegacyRussianModelBundle model,LegacyFreshSingleGlyphMainSampled actualSource,
        LegacyNativeCaseData caseData,LegacyNativeTextPublicationData publication,CancellationToken cancellationToken=default)
    {ArgumentNullException.ThrowIfNull(actualSource);return PrepareCore(acousticResource,lexicalResource,model,new(actualSource,null),caseData,publication,cancellationToken);}

    public static LegacyFreshCaseRussianDictionaryPreparation Prepare(ReadOnlySpan<byte> acousticResource,
        ReadOnlySpan<byte> lexicalResource,LegacyRussianModelBundle model,LegacyFreshMainSingleLineSampled actualSource,
        LegacyNativeCaseData caseData,LegacyNativeTextPublicationData publication,CancellationToken cancellationToken=default)
    {ArgumentNullException.ThrowIfNull(actualSource);return PrepareCore(acousticResource,lexicalResource,model,new(null,actualSource),caseData,publication,cancellationToken);}

    private static LegacyFreshCaseRussianDictionaryPreparation PrepareCore(ReadOnlySpan<byte> acousticResource,
        ReadOnlySpan<byte> lexicalResource,LegacyRussianModelBundle model,MainSource source,
        LegacyNativeCaseData caseData,LegacyNativeTextPublicationData publication,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(model);ArgumentNullException.ThrowIfNull(caseData);ArgumentNullException.ThrowIfNull(publication);token.ThrowIfCancellationRequested();
        if(acousticResource.Length!=572763||lexicalResource.Length!=6200992)throw new InvalidDataException("Exact Russian resource pair lengths required before hashing.");
        string fingerprint=Convert.ToHexString(SHA256.HashData(acousticResource));
        if(fingerprint!="7A5F08809E81FB4A2D4C78B3EFD251D0986503B0D30D21DE3695DAB1F8A89978"||
            !Convert.ToHexString(SHA256.HashData(lexicalResource)).Equals("D26ED08BAF419361484958B3A04A5F740A228F4664F7E2643F43459223EF3E4E",StringComparison.Ordinal))
            throw new InvalidDataException("Pinned Russian source resources required.");
        if(source.Categories.SelectedSourceOffset!=0||!StringComparer.OrdinalIgnoreCase.Equals(source.Categories.SourceFingerprint,fingerprint)||
            source.SpanProfile!=model.CursiveSpanProfile||model.SourceKind!=0x135)
            throw new ArgumentException("Actual sampled main0 source and selected model profile required.");
        var prefix=LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lexicalResource.Slice(606964,4565174-606964));
        if(prefix.Identifier!="ru_RU-lk-text.lite.lex"||prefix.Field68!=1)
            throw new NotSupportedException("Fresh Russian wrapping sequence2B7→292, without293/Asian/German wrappers, required.");
        uint order=LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lexicalResource.Slice(2513123,4565174-2513123));
        var contexts=new LegacySymbolContextFactory();
        var maps=LegacyDictionaryInitialSymbolMaps.Prepare(model.AcousticAlphabet,model.LexicalAlphabet,contexts,[],token);
        if(maps.CopyPrimary().Any(label=>label is 65535 or 65533))throw new NotSupportedException("Exact Russian map without normalization fallback required.");
        var descriptor=LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(prefix,model.Lexicon,order,true,token);
        var selection=new LegacyPreparedDictionarySourceSelectionBuilder();
        selection.Add(LegacyPreparedDictionarySourceSelectionInput.FromObservedOrderTwoModel(descriptor,model.LanguageModel,contexts.ContainerIdentity));
        var selected=selection.Prepare(token);
        if(selected.Count!=1)throw new InvalidDataException("Fresh single selected source required.");
        var preparation=LegacyNativeCasePreparation.Prepare(LegacyNativeCaseAlphabetRanges.Prepare(model.AcousticAlphabet,caseData,publication,token),publication,token);
        var binding=new LegacyPreparedCaseDictionarySymbolBinding(maps,preparation,model.LanguageModel,selected[0].Descriptor.Flags,token);
        //Fresh00C860 mode90=FFFFFFFF,C4=0.20BAA0 capture2368=0;
        //20D8F0 forwards capture assource.V10 sixtharg,2324E0→runtime18.
        //This API constructs fresh/no-event configuration; it doesnot guess
        //that a previously configured or reused native object still has0.
        var session=new LegacyPreparedCaseDictionarySession(model,prefix,uint.MaxValue,false,caseData,publication,
            binding,source.Graph,LegacyPreparedDictionaryRestartTags.CreateFresh(),false);
        var maskBinding=new LegacyPreparedOrdinaryLinguisticMaskBinding(model.AcousticAlphabet,acousticResource.Slice(99,16),[binding.CreateSymbolSource()],token);
        var knowledge=LegacyOrdinarySymbolMaskProducer.ReadV110KnowledgeGroups(acousticResource[189734..],model.AcousticAlphabet.LabelCount,out _,token);
        var maskResult=maskBinding.Build(false,true,knowledge,[],[],LegacyWideSymbolPairs.CreateUnrestricted(model.AcousticAlphabet),token);
        if(maskResult.ControllerOutcome!=LegacySymbolMaskControllerOutcome.Succeeded)throw new InvalidDataException("Owned case-aware symbol callbacks failed mask preparation.");
        var summary=LegacyPreparedLinguisticRefreshPolicy.Summarize([0,selected[0].Descriptor.Flags],false,false,-1,-1,token);
        int profile=model.ResourceLanguageProfileIndex;
        var language=new LegacyNativeKnownWordLanguageRecord(model.LanguageModel,model.LanguageProfiles[profile],model.AuxiliaryProfiles[profile],prefix.RuntimeLanguageScale);
        token.ThrowIfCancellationRequested();return new(model,source,binding,session,selected[0].Descriptor,selected,summary,language,maskResult.Masks);
    }

    public LegacyPreparedFreshMain240AcousticScorer CreateAcousticScorer(ReadOnlySpan<byte> actualResource,CancellationToken token=default)
        =>LegacyPreparedFreshMain240AcousticScorer.Prepare(actualResource,Model,source.Glyph??throw new InvalidOperationException("Actual line context overload required."),masks,token);
    public LegacyPreparedFreshMainLine240AcousticScorer CreateAcousticScorer(ReadOnlySpan<byte> actualResource,
        LegacyFreshMainSingleLineContext actualContext,CancellationToken token=default)
        =>LegacyPreparedFreshMainLine240AcousticScorer.Prepare(actualResource,Model,source.Line??throw new InvalidOperationException("Single glyph source uses its original scorer overload."),actualContext,masks,token);

    private sealed record MainSource(LegacyFreshSingleGlyphMainSampled? Glyph,LegacyFreshMainSingleLineSampled? Line)
    {
        public LegacySourceSamplerCategoryOwner Categories=>Glyph is{} glyph?glyph.Owner.CategoryOwner:Line!.Owner.CategoryOwner;
        public LegacyCursiveLatticeSpanProfile SpanProfile=>Glyph is{} glyph?glyph.Owner.SpanProfile:Line!.Owner.SpanProfile;
        public LegacyOrdinaryCursiveCandidateGraph Graph=>Glyph is{} glyph?glyph.SourceRanges.Graph:Line!.SourceRanges.Graph;
    }
}
