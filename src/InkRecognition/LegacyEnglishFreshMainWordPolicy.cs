using System.Buffers.Binary;
namespace InkRecognition;

/// <summary>NEW private fresh OUTER2B3 direct208/20E binding policy. Not an
///imported/copied/configured/high-driver default; not an acoustic word winner.</summary>
public sealed class LegacyEnglishFreshMainWordPolicy
{
    public LegacyEnglishFreshDictionaryQuerySession QuerySession{get;}
    public LegacySource247WireLayout Source=>QuerySession.Source;
    public string Scope=>"fresh-outer2B3/direct208-20E/no-copy/no-properties-events-associated-reuse/one13B-only";
    public uint AcousticSourceFlags{get;}
    public int MaximumSourceArcs{get;}
    public int AuxiliarySourceArcs{get;}
    public int MaximumParentDistance54=>120;
    public int FlagPrepassDistance58{get;}
    public bool KnowledgeF5=>false;
    public bool Graph1Dc=>false;
    public float SecondaryScaleEc=>0;
    public byte Permission2309=>0;
    public LegacyNativeWordSourceSummary Summary{get;}
    public LegacyNativeWordScalarRefreshResult Refresh{get;}
    public LegacyNativeWordControllerConfiguration Configuration{get;}
    private LegacyEnglishFreshMainWordPolicy(LegacyEnglishFreshDictionaryQuerySession binding,uint flags,int arcs,int auxiliary,int distance,
        LegacyNativeWordSourceSummary summary,LegacyNativeWordScalarRefreshResult refresh,LegacyNativeWordControllerConfiguration configuration)
    {QuerySession=binding;AcousticSourceFlags=flags;MaximumSourceArcs=arcs;AuxiliarySourceArcs=auxiliary;FlagPrepassDistance58=distance;Summary=summary;Refresh=refresh;Configuration=configuration;}
    public static LegacyEnglishFreshMainWordPolicy Prepare(LegacyEnglishFreshDictionaryQuerySession binding,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(binding);token.ThrowIfCancellationRequested();ValidateBinding(binding);
        var source=binding.Source;
        LegacyResourceWireField Field(string name,int size,int count)
        {var f=source.Fields.Single(x=>x.Owner=="247"&&x.Destination==name);if(f.ElementSize!=size||f.Count!=count)throw new InvalidDataException("Complete typed selected metadata required before copying.");return f;}
        //All selected field payload extents before the first copy; no caller scalar ingress.
        _=Field("B8",1,1);_=Field("B9",1,1);_=Field("16C",4,16);_=Field("1AC",2,16);_=Field("1CC",2,16);_=Field("214",2,6);
        byte[] Read(string name,int size,int count){token.ThrowIfCancellationRequested();return Field(name,size,count).CopyBytes();}
        byte b8=Read("B8",1,1)[0],b9=Read("B9",1,1)[0];
        uint flags=0x21703u&~BinaryPrimitives.ReadUInt32LittleEndian(Read("16C",4,16).AsSpan(13*4));
        int arcs=BinaryPrimitives.ReadUInt16LittleEndian(Read("1AC",2,16).AsSpan(13*2));
        int auxiliary=BinaryPrimitives.ReadUInt16LittleEndian(Read("1CC",2,16).AsSpan(13*2));
        int distance=BinaryPrimitives.ReadUInt16LittleEndian(Read("214",2,6).AsSpan(2*2));
        if(b8>1||b9>1||arcs<1||distance<1)throw new InvalidDataException("Actual positive selected source limits required.");
        var language=binding.LanguageQuery;var data=language.DataOwner;
        var descriptor=LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(data.Prefix,data.Dictionary,(uint)language.LanguageOwner.Order,true,token);
        if(descriptor.Flags!=binding.DescriptorFlags||binding.SelectedSourceIndex!=0||binding.SelectedPriority!=1024)throw new InvalidDataException("Same actual single selected dictionary descriptor required.");
        //235F30 implicit source0(0,0,1), then actual selected dictionary source1.
        var summary=LegacyNativeWordHeapPreparation.SummarizeFresh([new(0,0,1),LegacyNativeWordPreparedSource.FromDictionary(descriptor)],token);
        var refresh=LegacyNativeWordScalarRefresh.Apply(0,summary.PackedSource22e8,false,b8,0,b9,-1,0,source.Alphabet.TryGetLabel("\u202b",out _));
        var fresh=LegacyNativeWordControllerConfiguration.CreateFresh20baa0()with{PackedSource22e8=refresh.PackedSource22e8};
        var limits=LegacyNativeWordControllerLimitPreparation.Prepare(fresh,(uint)arcs,LegacyNativeWordKnowledgeLimits.CreateFresh00c860(),true);
        token.ThrowIfCancellationRequested();ValidateBinding(binding);
        return new(binding,flags,arcs,auxiliary,distance,summary,refresh,limits.Configuration);
    }
    internal void Validate(CancellationToken token=default){token.ThrowIfCancellationRequested();ValidateBinding(QuerySession);}
    private static void ValidateBinding(LegacyEnglishFreshDictionaryQuerySession binding)
    {
        var local=binding.Masks.Knowledge.Owner;
        if(binding.IsFaulted||!local.IsComplete||local.IsFaulted||!local.Owner.IsComplete||local.Owner.IsFaulted)throw new InvalidOperationException("Live same-source completed English owners required.");
        if(!ReferenceEquals(binding.Source,local.Owner.MainWire.Source)||!ReferenceEquals(binding.Source,binding.LanguageQuery.DataOwner.Source)||binding.Source.SourceKind!=0x135||binding.Masks.Policy!="independent-main13B-only/nonbypassed/no-user-restriction"||!binding.Source.Fingerprint.Equals("ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657",StringComparison.OrdinalIgnoreCase))throw new ArgumentException("Exact actual English source instance required; no copied/child knowledge coercion.");
    }
}
