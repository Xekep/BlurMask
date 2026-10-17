using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Actual global geometry/context and case-aware masks→four-family
/// main classifier/final cache. Model networks are reused, each source keeps
/// fresh geometry/symbol ownership. Does not create a fake concrete graph to
/// satisfy older dictionary/controller APIs.</summary>
public sealed class LegacyFreshMainGlobalAcousticScorer
{
    private readonly LegacyPreparedGlyphClassifier classifier;
    private readonly LegacyOrdinaryAcousticClassCache cache;
    private readonly LegacyAcousticSymbolMasks masks;
    private readonly LegacyAcousticCachePair[] pairs;
    private readonly byte[] families;
    public LegacyFreshMainGlobalCaseSymbols Owner {get;}
    public LegacyFreshMainGlobalGlyphGeometry Geometry=>Owner.Geometry;
    public LegacyPreparedFreshMain240AcousticPolicy Policy {get;}
    public LegacyOrdinaryAcousticCacheProfile CacheProfile {get;}
    public float CostScale {get;}
    public int CacheWidth=>cache.CacheWidth;
    private LegacyFreshMainGlobalAcousticScorer(LegacyFreshMainGlobalCaseSymbols owner,LegacyPreparedFreshMain240AcousticPolicy policy,
        LegacyOrdinaryAcousticCacheProfile cacheProfile,LegacyOrdinaryAcousticClassCache cache,LegacyAcousticSymbolMasks masks,byte[] families,float scale)
    {Owner=owner;Policy=policy;CacheProfile=cacheProfile;this.cache=cache;this.masks=masks;this.families=families;CostScale=scale;
        classifier=owner.Model.CreatePreparedGlyphClassifier();pairs=LegacyWideSymbolPairs.CreateUnrestricted(owner.Model.AcousticAlphabet);}

    public static LegacyFreshMainGlobalAcousticScorer Prepare(ReadOnlySpan<byte> resource,LegacyFreshMainGlobalCaseSymbols owner,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(owner);token.ThrowIfCancellationRequested();var geometry=owner.Geometry;
        if(resource.Length!=572763||!Convert.ToHexString(SHA256.HashData(resource)).Equals(geometry.Graph.Source.Source.SourceFingerprint,StringComparison.OrdinalIgnoreCase)||
            !ReferenceEquals(owner.Symbols.Preparation.Alphabet,owner.Model.AcousticAlphabet))
            throw new ArgumentException("Actual global geometry/model/symbol owner required.");
        var masks=owner.CopyMasks();int width=owner.Model.AcousticAlphabet.LabelCount;
        if(width!=192||masks.Primary.Length!=width||masks.Secondary.Length!=width)throw new InvalidDataException("Actual192-label masks required.");
        byte[] families=new byte[7];for(int i=0;i<7;i++)
        {
            var slot=owner.Model.AcousticWeights.Slots.Span[i];
            if(slot.NetworkMask!=(i<4?2:0)||i<4&&(!slot.FirstFlag||!slot.SecondFlag))throw new InvalidDataException("Actual main four second-only EXP family profile required.");
            families[i]=i<4?(byte)1:(byte)0;
        }
        int tail=checked(189734+8+I(resource,189738)),end=checked(8+I(resource,4));
        if(end-tail!=190||I(resource,tail)!=0)throw new InvalidDataException("Actual metadata/no-extra-object source tail required.");
        var meta=resource.Slice(tail+4,186);const int mode=13,variant=2;
        uint flags=0x21703u&~BinaryPrimitives.ReadUInt32LittleEndian(meta[(mode*4)..]);
        int arcs=BinaryPrimitives.ReadUInt16LittleEndian(meta[(64+mode*2)..]),aux=BinaryPrimitives.ReadUInt16LittleEndian(meta[(96+mode*2)..]);
        int alternatives=BinaryPrimitives.ReadUInt16LittleEndian(meta[(162+variant*2)..]);
        int contextEnd=checked(189755+8+I(resource,189759)),profile=contextEnd+65;short gate=BinaryPrimitives.ReadInt16LittleEndian(resource[profile..]);
        int cap=gate==0?short.MaxValue:BinaryPrimitives.ReadInt16LittleEndian(resource[(profile+6)..])/3;
        if(arcs<1||alternatives<1||cap<1)throw new InvalidDataException("Actual positive main profile caps required.");
        var policy=new LegacyPreparedFreshMain240AcousticPolicy(flags,arcs,aux,alternatives,meta[128]>mode,cap);
        var cacheProfile=LegacyOrdinaryAcousticCacheProfile.ReadV110Node(resource[189734..],token);
        var cache=new LegacyOrdinaryAcousticClassCache(cacheProfile,owner.Model.AcousticAlphabet,resource.Slice(99,16));
        return new(owner,policy,cacheProfile,cache,masks,families,LegacyPreparedAcousticDictionaryPermissions.ReadDisabledPowerScale(resource[189734..],token));
    }

    public LegacyPreparedFreshMain240AcousticScores Score(int group,CancellationToken token=default)
    {
        var frame=Geometry.ReadFrame(group,token);var ranges=Geometry.CopyChronologicalRanges(group,token);
        if(ranges.Length>Policy.MaximumNormalizedStrokes)throw new NotSupportedException("Actual main pen-up cap exceeded.");
        var context=Geometry.Context.GetContext(group,token);var signs=Geometry.ReadBoundarySigns(group,token);
        var glyph=classifier.Classify(Geometry.CopyPreparedInk(token),ranges,Geometry.ReadMinimumX(group,token),frame.Baseline,frame.Height,
            Geometry.TargetHeight,Geometry.Graph.State.MaximumAtoms,context,signs.Start,signs.End,true,families,1,masks.Primary.Span[..180],token);
        var primitive=Geometry.ReadPrimitiveScope(group,token);
        var cacheContext=new LegacyOrdinaryAcousticCacheContext(Policy.PruneCandidates,false,(Policy.FrontendFlags&0x10)!=0,
            primitive.BoundaryAlignedSingleNormalizedStroke,primitive.EndpointFieldsAreZero,primitive.SourceStrokeProbability,false,ReadOnlyMemory<ushort>.Empty);
        var result=cache.Build(glyph.IntegerCosts.Span,glyph.Geometry.Strokes,masks.Primary.Span,cacheContext,pairs,token);
        return new(glyph,result,primitive,signs,context);
    }
    private static int I(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
}
