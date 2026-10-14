using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Actual fresh first-source default scalar confidence branch of
///1bcbd0: selected candidate's EXP channels, default mode0 calibration and
/// two-family mean. No TOP273 mass, integer-cost exponential or OCR correction.
/// Network input indices follow the actual3A0 source-derived prototype map,
/// preserving output labels. Unmapped primitive/wide services are refused.</summary>
public sealed class LegacyFirstOrdinaryGlyphScoreProducer
{
    private readonly string fingerprint;
    private readonly LegacyAcousticConfidenceLabelMap labelMap;
    internal string SourceFingerprint=>fingerprint;
    internal bool HasNeuralOutputFor(ushort symbol)=>labelMap.TryResolveOutputLabel(symbol,out _);
    public int NetworkClassCount {get;}
    private LegacyFirstOrdinaryGlyphScoreProducer(LegacyFirstAcousticModel model,LegacyAcousticConfidenceLabelMap labelMap)
    {fingerprint=model.SourceFingerprint;NetworkClassCount=model.TrajectoryNetwork.OutputWidth;this.labelMap=labelMap;}

    public static LegacyFirstOrdinaryGlyphScoreProducer ReadFreshSelectedSource(ReadOnlySpan<byte> resource,
        int sourceOffset,CancellationToken cancellationToken=default)
    {
        _=LegacyOrdinaryAcousticCacheProfile.ReadFirstSource(resource,sourceOffset,out var model,cancellationToken);
        var policy=LegacyFreshFirstControllerPolicy.ReadFirstSource(resource,sourceOffset,cancellationToken);
        //150d00 packet19=(frontendFlags>>11)&1;1b0760 forwards it to1a8.
        if((policy.FrontendFlags&0x800)!=0)
            throw new NotSupportedException("This source requests individual per-network score channels.");
        int alphabet=checked(sourceOffset+20+4+56);
        int marker=I32(resource,alphabet);alphabet+=4;if(marker==-1)alphabet+=4;alphabet+=4;
        if(resource.Length-alphabet<23||resource[alphabet++]!=1||U32(resource,alphabet)!=0x2b)
            throw new NotSupportedException("Actual inline first-source alphabet required.");
        alphabet+=4;
        if(U16(resource,alphabet)!=0x103)throw new NotSupportedException("Actual first alphabet103 required.");
        alphabet+=2;
        //First loader proves no mode1/3 network, no dynamic converter and two
        //EXP families.1bcbd0 therefore chooses D8 when header8 exists.
        if(U16(resource,alphabet)==ushort.MaxValue)
            throw new NotSupportedException("No-blank alphabet selects the separate log-score calibration branch.");
        int end=checked(model.OrdinaryNodeOffset+8+(int)U32(resource,model.OrdinaryNodeOffset+4));
        var tail=new TailReader(resource.Slice(model.OrdinaryNodeOffset+model.PrefixBytesConsumed,
            end-model.OrdinaryNodeOffset-model.PrefixBytesConsumed));
        tail.Skip(1+4+1+8);int lists=tail.U16();
        for(int i=0;i<lists;i++){cancellationToken.ThrowIfCancellationRequested();tail.Skip(tail.U16()*2);}
        tail.Skip(4+20+1+8+1);
        if(tail.U16()!=0)throw new NotSupportedException("Grouped probability smoothing is not this fresh branch.");
        tail.Skip(4+2+2+12+1+4);
        if(tail.Byte()!=0||tail.I32()!=0)
            throw new NotSupportedException("Actual source requires obsolete conversion or nondefault score calibration.");
        var map=LegacyAcousticConfidenceLabelMap.ReadFreshFirstSource(resource,sourceOffset,cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();return new(model,map);
    }

    /// <summary>Final native candidate order determines the top20 cutoff. This
    /// producer returns candidate.vector10[0], not candidate24.field0. A missing
    /// label returns0 before invoking neural/calibration channels.</summary>
    public float ReadSymbolScore(LegacyFirstAcousticScores actualScores,
        LegacyOrdinaryAcousticClassCacheResult actualFinalCache,ushort symbol,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualScores);ArgumentNullException.ThrowIfNull(actualFinalCache);
        cancellationToken.ThrowIfCancellationRequested();
        if(actualScores.SourceFingerprint!=fingerprint||
            actualScores.TrajectoryProbability32768.Length!=NetworkClassCount||
            actualScores.RasterProbability32768.Length!=NetworkClassCount||actualFinalCache.Candidates.Length>150)
            throw new ArgumentException("Actual matching owned first-source score channels and bounded final candidates required.");
        int index=0;
        foreach(var candidate in actualFinalCache.Candidates.Span)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if(candidate.Symbol==symbol)break;index++;
        }
        if(index>=actualFinalCache.Candidates.Length||index>=20)return 0;
        if(!labelMap.TryResolveOutputLabel(symbol,out ushort outputLabel))
            throw new NotSupportedException("Selected label requires an unresolved primitive/wide probability service; no out-of-bounds neural lookup is performed.");
        float a=CalibrateDefault(actualScores.TrajectoryProbability32768.Span[outputLabel]*(1f/32768));
        float b=CalibrateDefault(actualScores.RasterProbability32768.Span[outputLabel]*(1f/32768));
        //Native order: family1 + family0, then five absent zero families.
        float sum=b+a;for(int i=2;i<7;i++)sum+=0f;
        float mean=sum/2;return mean>=1?1:mean;
    }

    //13e8e0 mode0 fallback uses a DOUBLE0.575. The separate mode1 helper
    //accepts stored float slopes; passing0.575f here changes float rounding.
    private static float CalibrateDefault(float probability)=>(float)(1/(1+Math.Exp(-(double)probability*.575)));

    private static int I32(ReadOnlySpan<byte> b,int p)=>BinaryPrimitives.ReadInt32LittleEndian(b.Slice(p,4));
    private static uint U32(ReadOnlySpan<byte> b,int p)=>BinaryPrimitives.ReadUInt32LittleEndian(b.Slice(p,4));
    private static ushort U16(ReadOnlySpan<byte> b,int p)=>BinaryPrimitives.ReadUInt16LittleEndian(b.Slice(p,2));
    private ref struct TailReader(ReadOnlySpan<byte> bytes)
    {
        private readonly ReadOnlySpan<byte> data=bytes;private int index;
        private ReadOnlySpan<byte> Take(int n){if(n<0||n>data.Length-index)throw new InvalidDataException("Truncated confidence configuration.");var s=data.Slice(index,n);index+=n;return s;}
        public void Skip(int n)=>Take(n);public byte Byte()=>Take(1)[0];public ushort U16()=>BinaryPrimitives.ReadUInt16LittleEndian(Take(2));public int I32()=>BinaryPrimitives.ReadInt32LittleEndian(Take(4));
    }
}
