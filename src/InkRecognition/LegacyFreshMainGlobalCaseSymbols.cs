using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Actual fresh2B7→292 symbol enumeration and acoustic mask preparation
/// for the owned global main graph. Owns one shared symbol/context binding;
/// does not invent a concrete same-source graph or a parallel DIRECT backend.</summary>
public sealed class LegacyFreshMainGlobalCaseSymbols
{
    private readonly LegacyAcousticSymbolMasks masks;
    public LegacyFreshMainGlobalGlyphGeometry Geometry {get;}
    public LegacyRussianModelBundle Model {get;}
    public LegacyPreparedCaseDictionarySymbolBinding Symbols {get;}
    public LegacySymbolContextFactory Contexts=>Symbols.Symbols.Contexts;
    public LegacyDictionaryResourcePrefix ResourcePrefix {get;}
    public LegacyPreparedDictionarySourceDescriptor Descriptor {get;}
    public IReadOnlyList<LegacyPreparedSelectedDictionarySource> SelectedSources {get;}
    public LegacyPreparedLinguisticSourceSummary SourceSummary {get;}
    public LegacyNativeKnownWordLanguageRecord LanguageRecords {get;}
    private LegacyFreshMainGlobalCaseSymbols(LegacyFreshMainGlobalGlyphGeometry geometry,LegacyRussianModelBundle model,
        LegacyPreparedCaseDictionarySymbolBinding symbols,LegacyAcousticSymbolMasks masks,LegacyDictionaryResourcePrefix prefix,
        LegacyPreparedDictionarySourceDescriptor descriptor,IReadOnlyList<LegacyPreparedSelectedDictionarySource> selected,
        LegacyPreparedLinguisticSourceSummary summary,LegacyNativeKnownWordLanguageRecord language)
    {Geometry=geometry;Model=model;Symbols=symbols;this.masks=masks;ResourcePrefix=prefix;Descriptor=descriptor;SelectedSources=selected;SourceSummary=summary;LanguageRecords=language;}
    public LegacyAcousticSymbolMasks CopyMasks()=>new(masks.Primary.ToArray(),masks.Secondary.ToArray());
    public uint[] CopySourceHeaders()=>[0,Descriptor.Flags];

    public static LegacyFreshMainGlobalCaseSymbols Prepare(ReadOnlySpan<byte> acousticResource,ReadOnlySpan<byte> lexicalResource,
        LegacyRussianModelBundle model,LegacyFreshMainGlobalGlyphGeometry geometry,LegacyNativeCaseData caseData,
        LegacyNativeTextPublicationData publication,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(model);ArgumentNullException.ThrowIfNull(geometry);ArgumentNullException.ThrowIfNull(caseData);ArgumentNullException.ThrowIfNull(publication);token.ThrowIfCancellationRequested();
        if(acousticResource.Length!=572763||lexicalResource.Length!=6200992||
            !Convert.ToHexString(SHA256.HashData(acousticResource)).Equals(geometry.Graph.Source.Source.SourceFingerprint,StringComparison.OrdinalIgnoreCase)||
            !Convert.ToHexString(SHA256.HashData(lexicalResource)).Equals("D26ED08BAF419361484958B3A04A5F740A228F4664F7E2643F43459223EF3E4E",StringComparison.Ordinal))
            throw new InvalidDataException("Same pinned Russian acoustic/lexical source resources required.");
        if(geometry.CategoryOwner.SelectedSourceOffset!=0||geometry.Graph.Source.Source.SpanProfile!=model.CursiveSpanProfile||model.SourceKind!=0x135)
            throw new ArgumentException("Actual global main0 and selected model profiles required.");
        var prefix=LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lexicalResource.Slice(606964,4565174-606964));
        if(prefix.Identifier!="ru_RU-lk-text.lite.lex"||prefix.Field68!=1)throw new NotSupportedException("Actual fresh Russian2B7→292 selected source required.");
        if(!caseData.HasCaseAlternatives(model.LexicalAlphabet,publication))
            throw new NotSupportedException("Actual20C9D0 selected lexical alphabet must require the292 case wrapper.");
        uint order=LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lexicalResource.Slice(2513123,4565174-2513123));
        var contexts=new LegacySymbolContextFactory();var maps=LegacyDictionaryInitialSymbolMaps.Prepare(model.AcousticAlphabet,model.LexicalAlphabet,contexts,[],token);
        if(maps.CopyPrimary().Any(label=>label is 65535 or 65533))throw new NotSupportedException("Exact Russian symbol map without normalization fallback required.");
        var descriptor=LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(prefix,model.Lexicon,order,true,token);
        var selection=new LegacyPreparedDictionarySourceSelectionBuilder();selection.Add(LegacyPreparedDictionarySourceSelectionInput.FromObservedOrderTwoModel(descriptor,model.LanguageModel,contexts.ContainerIdentity));
        var selected=selection.Prepare(token);if(selected.Count!=1)throw new InvalidDataException("One actual selected Russian source required.");
        var preparation=LegacyNativeCasePreparation.Prepare(LegacyNativeCaseAlphabetRanges.Prepare(model.AcousticAlphabet,caseData,publication,token),publication,token);
        var binding=new LegacyPreparedCaseDictionarySymbolBinding(maps,preparation,model.LanguageModel,selected[0].Descriptor.Flags,token);
        var maskBinding=new LegacyPreparedOrdinaryLinguisticMaskBinding(model.AcousticAlphabet,acousticResource.Slice(99,16),[binding.CreateSymbolSource()],token);
        var knowledge=LegacyOrdinarySymbolMaskProducer.ReadV110KnowledgeGroups(acousticResource[189734..],model.AcousticAlphabet.LabelCount,out _,token);
        var result=maskBinding.Build(false,true,knowledge,[],[],LegacyWideSymbolPairs.CreateUnrestricted(model.AcousticAlphabet),token);
        if(result.ControllerOutcome!=LegacySymbolMaskControllerOutcome.Succeeded)throw new InvalidDataException("Actual case-aware symbol callbacks failed mask preparation.");
        var summary=LegacyPreparedLinguisticRefreshPolicy.Summarize([0,selected[0].Descriptor.Flags],false,false,-1,-1,token);int profile=model.ResourceLanguageProfileIndex;
        var language=new LegacyNativeKnownWordLanguageRecord(model.LanguageModel,model.LanguageProfiles[profile],model.AuxiliaryProfiles[profile],prefix.RuntimeLanguageScale);
        token.ThrowIfCancellationRequested();return new(geometry,model,binding,result.Masks,prefix,selected[0].Descriptor,selected,summary,language);
    }
}
