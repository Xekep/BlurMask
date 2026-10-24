using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace InkRecognition;

/// <summary>Fresh constructor options for an explicitly prepared ordinary240
///caller. Source247's own metadata supplies flags and caps. This does not choose
///240 from an arbitrary live owner, nor preserve a reused/custom configuration.</summary>
public sealed record LegacyPreparedFreshMain240AcousticPolicy(uint FrontendFlags,
    int MaximumSourceArcs,int SourceAuxiliaryLimit,int SourceAlternativeLimit,
    bool PruneCandidates,int MaximumNormalizedStrokes)
{
    internal static LegacyPreparedFreshMain240AcousticPolicy Read(ReadOnlySpan<byte> resource,
        LegacyFreshSingleGlyphMainSampler owner)
    {
        int ordinary=owner.AcousticOffset;
        int tail=checked(ordinary+8+(int)BinaryPrimitives.ReadUInt32LittleEndian(resource[(ordinary+4)..]));
        int end=checked(owner.CategoryOwner.SelectedSourceOffset+8+(int)BinaryPrimitives.ReadUInt32LittleEndian(resource[(owner.CategoryOwner.SelectedSourceOffset+4)..]));
        if(end-tail!=190||BinaryPrimitives.ReadInt32LittleEndian(resource[tail..])!=0)
            throw new NotSupportedException("Owned main v10d metadata/no-extra-object branch required.");
        var metadata=resource.Slice(tail+4,186);
        const int mode=13,variant=2; //NEW00C860 owner18=020DFFFF; no option overrides.
        uint flags=0x21703u&~BinaryPrimitives.ReadUInt32LittleEndian(metadata[(mode*4)..]);
        int arcs=BinaryPrimitives.ReadUInt16LittleEndian(metadata[(64+mode*2)..]);
        int auxiliary=BinaryPrimitives.ReadUInt16LittleEndian(metadata[(96+mode*2)..]);
        int alternatives=BinaryPrimitives.ReadUInt16LittleEndian(metadata[(162+variant*2)..]);
        bool prune=metadata[128]>mode;
        int network=ordinary+21;
        int contextEnd=checked(network+8+(int)BinaryPrimitives.ReadUInt32LittleEndian(resource[(network+4)..]));
        //Checked dynamic257 prefix:24 context bytes,flag,count,8 floats,
        //separate1C4 threshold,single profile count,no-grid flag,then profile14.
        int profile=contextEnd+65;
        short gate=BinaryPrimitives.ReadInt16LittleEndian(resource[profile..]);
        int cap=gate==0?short.MaxValue:BinaryPrimitives.ReadInt16LittleEndian(resource[(profile+6)..])/3;
        if(arcs<1||alternatives<1||cap<1)throw new InvalidDataException("Positive main source/candidate/pen-up caps required.");
        return new(flags,arcs,auxiliary,alternatives,prune,cap);
    }
}

public sealed record LegacyPreparedFreshMain240AcousticScores(
    LegacyPreparedGlyphClassification Glyph,LegacyOrdinaryAcousticClassCacheResult Cache,
    LegacySourcePrimitiveScope PrimitiveScope,LegacyCursiveGlyphBoundarySigns BoundarySigns,
    ReadOnlyMemory<short> Context);

/// <summary>Owned main source normalization/features/four EXP families and195
///class cache for the restored nonclosed singleton source stage. The240 caller
///is explicitly prepared, with fresh unmodified constructor options. The symbol
///masks remain actual upstream linguistic preparation data; no all-enabled mask
///or script aliases are inferred. No final word/confidence selection is restored.</summary>
public sealed class LegacyPreparedFreshMain240AcousticScorer
{
    private readonly LegacyPreparedGlyphClassifier classifier;
    private readonly LegacyOrdinaryAcousticClassCache cache;
    private readonly LegacyAcousticSymbolMasks masks;
    private readonly LegacyAcousticCachePair[] pairs;
    private readonly short[] context;
    private readonly byte[] enabledFamilies;
    public LegacyFreshSingleGlyphMainSampled Source {get;}
    public LegacyPreparedFreshMain240AcousticPolicy Policy {get;}
    public LegacyOrdinaryAcousticCacheProfile CacheProfile {get;}
    public string SourceIdentifier {get;}
    public int ContextWindowCount=>0;
    public int CacheWidth=>cache.CacheWidth;
    public float CostScale {get;}
    private LegacyPreparedFreshMain240AcousticScorer(LegacyFreshSingleGlyphMainSampled source,
        LegacyPreparedFreshMain240AcousticPolicy policy,LegacyOrdinaryAcousticCacheProfile cacheProfile,
        LegacyPreparedGlyphClassifier classifier,LegacyOrdinaryAcousticClassCache cache,
        LegacyAcousticSymbolMasks masks,LegacyAcousticCachePair[] pairs,short[] context,byte[] enabledFamilies,float scale,string identifier)
    {Source=source;Policy=policy;CacheProfile=cacheProfile;this.classifier=classifier;this.cache=cache;
        this.masks=masks;this.pairs=pairs;this.context=context;this.enabledFamilies=enabledFamilies;CostScale=scale;SourceIdentifier=identifier;}

    public static LegacyPreparedFreshMain240AcousticScorer Prepare(ReadOnlySpan<byte> actualResource,
        LegacyRussianModelBundle actualMainModel,LegacyFreshSingleGlyphMainSampled actualSource,
        LegacyAcousticSymbolMasks preparedLinguisticMasks,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualMainModel);ArgumentNullException.ThrowIfNull(actualSource);
        ArgumentNullException.ThrowIfNull(preparedLinguisticMasks);cancellationToken.ThrowIfCancellationRequested();
        if(actualResource.Length!=572763||!SHA256.HashData(actualResource).AsSpan().SequenceEqual(Convert.FromHexString("7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978")))
            throw new InvalidDataException("Pinned actual main Russian model resource required.");
        var owned=LegacySourceSamplerCategoryOwner.ReadV10dSource(actualResource,0,cancellationToken);
        string identifier=LegacyAcousticSourceIdentifier.Read247V10d(actualResource);
        //150D00 ordinary initializerparam4 is selectedsource+14;1B0760
        //passes it to global90.virtual58=089AD0, an ordinal PREFIX predicate.
        //Only a zh_ prefix allocates the additional2D0 confidence mask.
        if(LegacyAcousticSourceIdentifier.HasBytePrefix(Encoding.UTF8.GetBytes(identifier),"zh_"u8))
            throw new NotSupportedException("Chinese confidence-intersection branch is outside this main scorer.");
        if(actualSource.Owner.CategoryOwner.SelectedSourceOffset!=0||owned.SourceFingerprint!=actualSource.Owner.CategoryOwner.SourceFingerprint||
            actualSource.Owner.SpanProfile!=actualMainModel.CursiveSpanProfile||actualSource.Owner.AcousticOffset!=189734||
            actualMainModel.AcousticAlphabet.LabelCount!=actualSource.Owner.AcousticAlphabet.LabelCount)
            throw new ArgumentException("Actual main0 resource/model/prepared source owner required.");
        int width=actualMainModel.AcousticAlphabet.LabelCount;
        if(preparedLinguisticMasks.Primary.Length!=width||preparedLinguisticMasks.Secondary.Length!=width)
            throw new ArgumentException("Prepared linguistic masks must match the owned main alphabet.");
        foreach(byte flag in preparedLinguisticMasks.Primary.Span)if(flag>1)throw new ArgumentException("Mask requires0/1.");
        foreach(byte flag in preparedLinguisticMasks.Secondary.Span)if(flag>1)throw new ArgumentException("Mask requires0/1.");
        var weights=LegacyDirectionalAcousticWeightConfiguration.LoadV110Prefix(actualResource[189734..],out _,cancellationToken);
        byte[] families=new byte[7];
        for(int i=0;i<7;i++)
        {
            var slot=weights.Slots.Span[i];
            if(slot.NetworkMask!=(i<4?2:0)||i<4&&(!slot.FirstFlag||!slot.SecondFlag))
                throw new NotSupportedException("Four actual second-only EXP families with appended context/boundaries required.");
            families[i]=slot.NetworkMask==0?(byte)0:(byte)1;
        }
        //1B0760 global271/272 use first/third network pointers:both absent.
        //Thus1B8EB0 copies masks without neural preselection. V110 profile148/
        //150 source-count constraints are NULL, so no extra count filter here.
        var maskCopy=new LegacyAcousticSymbolMasks(preparedLinguisticMasks.Primary.ToArray(),preparedLinguisticMasks.Secondary.ToArray());
        var policy=LegacyPreparedFreshMain240AcousticPolicy.Read(actualResource,actualSource.Owner);
        var profile=LegacyOrdinaryAcousticCacheProfile.ReadV110Node(actualResource[189734..],cancellationToken);
        int network=189734+21,contextEnd=checked(network+8+(int)BinaryPrimitives.ReadUInt32LittleEndian(actualResource[(network+4)..]));
        float offset=F(actualResource,contextEnd),lv=F(actualResource,contextEnd+4),rv=F(actualResource,contextEnd+8);
        //Fresh/no-prior1C9870 resets82=0. Descriptor physical interval0..0:
        //the ordinary window loop starts24+1=1,greater than28=0. Empty
        //observations produce28 FLOAT -1 channels, then exact fixed input.
        var descriptor=actualSource.Preparation.LinePreparation.Output.CopyDescriptor().Span;
        if(BinaryPrimitives.ReadInt32LittleEndian(descriptor[36..])!=0||BinaryPrimitives.ReadInt32LittleEndian(descriptor[40..])!=0)
            throw new NotSupportedException("Owned single physical main context branch required.");
        var context=LegacyFloatInputQuantization.Quantize(LegacyContextWeighting.Combine([],0,actualSource.SourceHeight,offset,lv,rv,cancellationToken),cancellationToken:cancellationToken);
        var cache=new LegacyOrdinaryAcousticClassCache(profile,actualMainModel.AcousticAlphabet,actualResource.Slice(99,16));
        return new(actualSource,policy,profile,new(actualMainModel.CreateGlyphClassifier(),weights),cache,maskCopy,
            LegacyWideSymbolPairs.CreateUnrestricted(actualMainModel.AcousticAlphabet),context,families,
            LegacyPreparedAcousticDictionaryPermissions.ReadDisabledPowerScale(actualResource[189734..],cancellationToken),identifier);
    }

    public LegacyPreparedFreshMain240AcousticScores Score(int groupIndex,CancellationToken cancellationToken=default)
        =>ScoreWithConfiguredDebit(groupIndex,cancellationToken,null);
    internal LegacyPreparedFreshMain240AcousticScores ScoreWithConfiguredDebit(int groupIndex,CancellationToken cancellationToken,LegacyConfiguredSourceOperationDebit? debit)
    {
        if(debit is not null)throw new ArgumentException("Configured LINE debit cannot use a single-glyph scorer.");
        cancellationToken.ThrowIfCancellationRequested();
        var normal=Source.NormalizeGroup(groupIndex,cancellationToken);var ranges=normal.Ranges.Span;
        if(normal.Strokes.Count>Policy.MaximumNormalizedStrokes)throw new NotSupportedException("Actual main pen-up cap exceeded.");
        float minimum=float.MaxValue;for(int p=ranges[0].StartPoint;p<ranges[0].EndPointExclusive;p++)minimum=Math.Min(minimum,Source.Cuts.PreparedInk.Points.Span[p].X);
        var signs=LegacyCursiveGlyphBoundaryPolicy.Resolve(ranges,Source.Cuts.PreparedInk.StrokePointCounts.Span,0,false,false,cancellationToken);
        var glyph=classifier.Classify(Source.Cuts.PreparedInk,ranges,minimum,Source.SourceBaseline,Source.SourceHeight,
            Source.Owner.AcousticTargetHeight,1,context,signs.Start,signs.End,true,enabledFamilies,1,
            masks.Primary.Span[..180],cancellationToken);
        var range=ranges[0];bool whole=range.StartPoint==0&&range.EndPointExclusive==Source.Cuts.PreparedInk.StrokePointCounts.Span[0];
        var original=Source.Owner.Postprocessing.Input.Controller.Source;
        bool zero=whole&&Source.Chronology.Resolve(0,0).Key.Third==0&&
            original.ResolveEndingMetadata(Source.Chronology.Resolve(0,range.EndPointExclusive-1),cancellationToken).Key.Third==0;
        var primitive=new LegacySourcePrimitiveScope(whole,zero,whole?0:-1,whole?original.SourceStrokeProbabilities.Span[0]:null);
        //Fresh runtime1A4=0,sourceF5=0; sourceD8 scope count148=0. Read
        //knowledge switch from this MAIN frontend,not first-source flags.
        var policy=new LegacyOrdinaryAcousticCacheContext(Policy.PruneCandidates,false,(Policy.FrontendFlags&0x10)!=0,
            whole,zero,primitive.SourceStrokeProbability,false,ReadOnlyMemory<ushort>.Empty);
        var final=cache.Build(glyph.IntegerCosts.Span,glyph.Geometry.Strokes,masks.Primary.Span,policy,pairs,cancellationToken);
        return new(glyph,final,primitive,signs,context.ToArray());
    }
    private static float F(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadSingleLittleEndian(b[at..]);
}
