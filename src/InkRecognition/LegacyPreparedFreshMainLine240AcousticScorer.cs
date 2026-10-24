using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace InkRecognition;

/// <summary>Explicit fresh ordinary240 caller over an actual open multi-
/// physical main line, its post-sampler float context and main four-family
/// EXP classifier/cache. Linguistic masks remain genuine upstream preparation
/// data; this does not choose a live controller or manufacture its word winner.</summary>
public sealed class LegacyPreparedFreshMainLine240AcousticScorer
{
    private readonly LegacyPreparedGlyphClassifier classifier;
    private readonly LegacyOrdinaryAcousticClassCache cache;
    private readonly LegacyAcousticSymbolMasks masks;
    private readonly LegacyAcousticCachePair[] pairs;
    private readonly byte[] families;
    public LegacyFreshMainSingleLineSampled Source {get;}
    public LegacyFreshMainSingleLineContext Context {get;}
    public LegacyPreparedFreshMain240AcousticPolicy Policy {get;}
    public LegacyOrdinaryAcousticCacheProfile CacheProfile {get;}
    public int CacheWidth=>cache.CacheWidth;
    public float CostScale {get;}
    private LegacyPreparedFreshMainLine240AcousticScorer(LegacyFreshMainSingleLineSampled source,LegacyFreshMainSingleLineContext context,
        LegacyPreparedFreshMain240AcousticPolicy policy,LegacyPreparedGlyphClassifier classifier,LegacyOrdinaryAcousticClassCache cache,
        LegacyAcousticSymbolMasks masks,LegacyAcousticCachePair[] pairs,byte[] families,float scale,LegacyOrdinaryAcousticCacheProfile cacheProfile)
    {Source=source;Context=context;Policy=policy;this.classifier=classifier;this.cache=cache;this.masks=masks;this.pairs=pairs;this.families=families;CostScale=scale;CacheProfile=cacheProfile;}

    public static LegacyPreparedFreshMainLine240AcousticScorer Prepare(ReadOnlySpan<byte> resource,LegacyRussianModelBundle model,
        LegacyFreshMainSingleLineSampled source,LegacyFreshMainSingleLineContext actualContext,LegacyAcousticSymbolMasks preparedMasks,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(model);ArgumentNullException.ThrowIfNull(source);ArgumentNullException.ThrowIfNull(actualContext);
        ArgumentNullException.ThrowIfNull(preparedMasks);cancellationToken.ThrowIfCancellationRequested();
        if(!ReferenceEquals(actualContext.Source,source)||!Convert.ToHexString(SHA256.HashData(resource)).Equals(source.Owner.CategoryOwner.SourceFingerprint,StringComparison.OrdinalIgnoreCase)||
            source.Owner.AcousticOffset!=189734||model.CursiveSpanProfile!=source.Owner.SpanProfile||model.AcousticAlphabet.LabelCount!=source.Owner.AcousticAlphabet.LabelCount)
            throw new ArgumentException("Same actual main source/context/model owner required.");
        string name=LegacyAcousticSourceIdentifier.Read247V10d(resource);
        if(LegacyAcousticSourceIdentifier.HasBytePrefix(Encoding.UTF8.GetBytes(name),"zh_"u8))throw new NotSupportedException("Chinese confidence-mask intersection branch is separate.");
        int width=model.AcousticAlphabet.LabelCount;
        if(preparedMasks.Primary.Length!=width||preparedMasks.Secondary.Length!=width)throw new ArgumentException("Actual main linguistic masks required.");
        foreach(byte b in preparedMasks.Primary.Span)if(b>1)throw new ArgumentException("Binary linguistic mask required.");
        foreach(byte b in preparedMasks.Secondary.Span)if(b>1)throw new ArgumentException("Binary linguistic mask required.");
        var weights=LegacyDirectionalAcousticWeightConfiguration.LoadV110Prefix(resource[189734..],out _,cancellationToken);
        byte[] families=new byte[7];
        for(int i=0;i<families.Length;i++)
        {
            var slot=weights.Slots.Span[i];if(slot.NetworkMask!=(i<4?2:0)||i<4&&(!slot.FirstFlag||!slot.SecondFlag))
                throw new InvalidDataException("Actual main four second-only EXP families required.");families[i]=i<4?(byte)1:(byte)0;
        }
        int tail=checked(189734+8+I(resource,189738)),end=checked(8+I(resource,4));
        if(end-tail!=190||I(resource,tail)!=0)throw new InvalidDataException("Actual source metadata/no-extra-object tail required.");
        var meta=resource.Slice(tail+4,186);const int mode=13,variant=2;
        uint flags=0x21703u&~BinaryPrimitives.ReadUInt32LittleEndian(meta[(mode*4)..]);
        int arcs=BinaryPrimitives.ReadUInt16LittleEndian(meta[(64+mode*2)..]);
        int auxiliary=BinaryPrimitives.ReadUInt16LittleEndian(meta[(96+mode*2)..]);
        int alternatives=BinaryPrimitives.ReadUInt16LittleEndian(meta[(162+variant*2)..]);
        int contextEnd=checked(189755+8+I(resource,189759)),profile=contextEnd+65;
        short gate=BinaryPrimitives.ReadInt16LittleEndian(resource[profile..]);
        int cap=gate==0?short.MaxValue:BinaryPrimitives.ReadInt16LittleEndian(resource[(profile+6)..])/3;
        if(arcs<1||alternatives<1||cap<1)throw new InvalidDataException("Positive actual main caps required.");
        var policy=new LegacyPreparedFreshMain240AcousticPolicy(flags,arcs,auxiliary,alternatives,meta[128]>mode,cap);
        var cacheProfile=LegacyOrdinaryAcousticCacheProfile.ReadV110Node(resource[189734..],cancellationToken);
        var cache=new LegacyOrdinaryAcousticClassCache(cacheProfile,
            model.AcousticAlphabet,resource.Slice(99,16));
        return new(source,actualContext,policy,new(model.CreateGlyphClassifier(),weights),cache,
            new(preparedMasks.Primary.ToArray(),preparedMasks.Secondary.ToArray()),LegacyWideSymbolPairs.CreateUnrestricted(model.AcousticAlphabet),families,
            LegacyPreparedAcousticDictionaryPermissions.ReadDisabledPowerScale(resource[189734..],cancellationToken),cacheProfile);
    }

    public LegacyPreparedFreshMain240AcousticScores Score(int groupIndex,CancellationToken cancellationToken=default)
        =>ScoreWithConfiguredDebit(groupIndex,cancellationToken,null);
    internal LegacyPreparedFreshMain240AcousticScores ScoreWithConfiguredDebit(int groupIndex,CancellationToken cancellationToken,LegacyConfiguredSourceOperationDebit? debit)
    {
        debit?.RequireLine(Source,Context,cancellationToken);
        var normal=Source.NormalizeGroup(groupIndex,cancellationToken);var ranges=normal.Ranges.Span;
        if(normal.Strokes.Count>Policy.MaximumNormalizedStrokes)throw new NotSupportedException("Actual main pen-up cap exceeded.");
        var context=Context.GetContext(groupIndex,cancellationToken);
        var signs=LegacyCursiveGlyphBoundaryPolicy.Resolve(ranges,Source.Cuts.PreparedInk.StrokePointCounts.Span,0,false,false,cancellationToken);
        var glyph=classifier.ClassifyWithConfiguredDebit(Source.Cuts.PreparedInk,ranges,Source.GetGroupMinimumX(groupIndex,cancellationToken),Source.Owner.SourceBaseline,
            Source.Owner.SourceHeight,Source.Owner.AcousticTargetHeight,Source.Owner.SpanProfile.MaximumAtoms,context,signs.Start,signs.End,true,families,1,
            masks.Primary.Span[..180],cancellationToken,debit);
        var primitive=Source.GetPrimitiveScope(groupIndex,cancellationToken);
        var cacheContext=new LegacyOrdinaryAcousticCacheContext(Policy.PruneCandidates,false,(Policy.FrontendFlags&0x10)!=0,
            primitive.BoundaryAlignedSingleNormalizedStroke,primitive.EndpointFieldsAreZero,primitive.SourceStrokeProbability,false,ReadOnlyMemory<ushort>.Empty);
        var result=cache.Build(glyph.IntegerCosts.Span,glyph.Geometry.Strokes,masks.Primary.Span,cacheContext,pairs,cancellationToken);
        return new(glyph,result,primitive,signs,context);
    }
    private static int I(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
}
