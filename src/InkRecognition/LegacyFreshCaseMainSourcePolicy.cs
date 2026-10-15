using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Bounded NEW20E/actual13B-only fresh Russian main source factory.
/// It derives the absent associated13D and false sourceV60 gates; no reused,
/// externally supplied or mutable native factory configuration is accepted.</summary>
public sealed class LegacyFreshCaseMainSourcePolicy
{
    public LegacyOwnedFreshMainWordControllerPolicy Controller {get;}
    public LegacyFreshCaseRussianDictionaryPreparation DictionaryOwner {get;}
    public LegacyRussianModelBundle Model=>DictionaryOwner.Model;
    public LegacyOrdinaryCursiveCandidateGraph Graph=>DictionaryOwner.Graph;
    public string AcousticFingerprint {get;}
    public string LexicalFingerprint {get;}
    public int ConfiguredSourceCount=>1;
    public uint ConfiguredSourceType=>0x13b;
    public bool HasAssociatedSource=>false;
    public bool SupportsSecondaryGroupSourceV60=>false;
    public bool SchedulesSecondaryGroup1Fe740=>false;

    private LegacyFreshCaseMainSourcePolicy(LegacyOwnedFreshMainWordControllerPolicy controller,
        LegacyFreshCaseRussianDictionaryPreparation dictionary,string acoustic,string lexical)
    {Controller=controller;DictionaryOwner=dictionary;AcousticFingerprint=acoustic;LexicalFingerprint=lexical;}

    public static LegacyFreshCaseMainSourcePolicy Prepare(ReadOnlySpan<byte> acousticResource,
        ReadOnlySpan<byte> lexicalResource,LegacyOwnedFreshMainWordControllerPolicy controller,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(controller);cancellationToken.ThrowIfCancellationRequested();
        var span=controller.Owner;
        var dictionary=span.CaseDictionaryOwner??throw new ArgumentException("This policy requires the owned fresh case-aware factory, not a DIRECT or manually selected dictionary.");
        if(acousticResource.Length!=572763||lexicalResource.Length!=6200992)
            throw new InvalidDataException("Pinned Russian resource pair extents required before hashing.");
        string acoustic=Convert.ToHexString(SHA256.HashData(acousticResource));
        string lexical=Convert.ToHexString(SHA256.HashData(lexicalResource));
        cancellationToken.ThrowIfCancellationRequested();
        if(acoustic!="7A5F08809E81FB4A2D4C78B3EFD251D0986503B0D30D21DE3695DAB1F8A89978"||
            lexical!="D26ED08BAF419361484958B3A04A5F740A228F4664F7E2643F43459223EF3E4E"||
            !StringComparer.OrdinalIgnoreCase.Equals(acoustic,span.SelectedSourceOwner.SourceFingerprint)||
            span.SelectedSourceOwner.SelectedSourceOffset!=0)
            throw new ArgumentException("Factory policy requires the actual selected main0 resource owner.");
        if(!ReferenceEquals(span.CommonModel,dictionary.Model)||!ReferenceEquals(span.Contexts,dictionary.Contexts)||
            !ReferenceEquals(span.SourceGraph,dictionary.Graph)||!ReferenceEquals(dictionary.Session.Model,dictionary.Model)||
            !ReferenceEquals(dictionary.Session.Graph,dictionary.Graph)||!ReferenceEquals(dictionary.Session.Contexts,dictionary.Contexts))
            throw new ArgumentException("One actual main source/model/graph/context owner required.");
        if(dictionary.ContainerCaseMode90!=uint.MaxValue||dictionary.IgnoreCaseC4||dictionary.CaptureSideCosts2368||
            dictionary.SelectedSources.Count!=1||!ReferenceEquals(dictionary.SelectedSources[0].Descriptor,dictionary.Descriptor))
            throw new NotSupportedException("Only the fresh no-property/no-external one-source case factory is proved.");
        //Checked wire13B is a native139→27B descendant, NOT13D. NEW20E clears28;
        //1EAEB0's first27B branch adds it only to8/10, leaving associated28 zero.
        var prefix=LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lexicalResource.Slice(606964,4565174-606964));
        var descriptor=LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(prefix,dictionary.Model.Lexicon,
            LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lexicalResource.Slice(2513123,4565174-2513123)),true,cancellationToken);
        if(prefix.Identifier!="ru_RU-lk-text.lite.lex"||prefix.Field68!=1||descriptor!=dictionary.Descriptor||
            !span.CopySourceHeaders().SequenceEqual(new uint[]{0,dictionary.Descriptor.Flags}))
            throw new ArgumentException("The selected13B source/descriptor must be the checked fresh dictionary owner.");
        //1EB340 sees V50/20C8B0 == associated28 ==0, so V48/20C7F0 need not
        //change240+2300.292/290.V60=05A400 false;2144F0's first1FE740 gate fails.
        cancellationToken.ThrowIfCancellationRequested();return new(controller,dictionary,acoustic,lexical);
    }
}
