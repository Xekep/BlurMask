using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Owned first247 sampler297/v106 settings consumed by1941b0,
///195c40 and195b30, including its original discrete category prototypes.</summary>
public sealed class LegacyFirstTransitionGeometryProfile
{
    public LegacyOrdinaryRowAcousticProfile RowProfile {get;}
    public LegacyInkAlphabet Alphabet {get;}
    public LegacyCursiveCategoryProfile Categories {get;}
    public int SamplerOffset {get;}
    public float PairScale {get;}
    public float PairOffset {get;}
    public float PairMaximum {get;}
    public float DefaultGap {get;}
    public float TranslationThreshold {get;}
    public float GapSlope {get;}
    public int JoinMaximumCost {get;}
    public int MatrixMaximumCost {get;}
    private readonly float[] matrix;
    public ReadOnlyMemory<float> GapMatrix=>matrix;
    private LegacyFirstTransitionGeometryProfile(LegacyOrdinaryRowAcousticProfile row,LegacyInkAlphabet alphabet,
        LegacyCursiveCategoryProfile categories,int offset,float pairScale,float pairOffset,float pairMaximum,
        float defaultGap,float translationThreshold,float gapSlope,int joinCap,int matrixCap,float[] matrix)
    {RowProfile=row;Alphabet=alphabet;Categories=categories;SamplerOffset=offset;PairScale=pairScale;PairOffset=pairOffset;
        PairMaximum=pairMaximum;DefaultGap=defaultGap;TranslationThreshold=translationThreshold;GapSlope=gapSlope;
        JoinMaximumCost=joinCap;MatrixMaximumCost=matrixCap;this.matrix=matrix;}

    public static LegacyFirstTransitionGeometryProfile ReadSource(ReadOnlySpan<byte> resource,int sourceOffset,
        CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();var model=LegacyFirstAcousticModel.ReadSource(resource,sourceOffset,cancellationToken);
        int smoothing=model.RowProfile.SmoothingNodeOffset;
        int offset=checked(smoothing+8+(int)BinaryPrimitives.ReadUInt32LittleEndian(resource[(smoothing+4)..])+1);
        var node=resource[offset..];uint length=BinaryPrimitives.ReadUInt32LittleEndian(node[4..]);
        if(length>node.Length-8||length<65||BinaryPrimitives.ReadUInt32LittleEndian(node)!=0x0101fe90||
            BinaryPrimitives.ReadUInt32LittleEndian(node[8..])!=0x297||BinaryPrimitives.ReadUInt16LittleEndian(node[12..])!=0||
            BinaryPrimitives.ReadUInt16LittleEndian(node[14..])!=0x106||node[64]!=1)
            throw new InvalidDataException("Expected owned sampler297/v106 category profile.");
        node=node[..checked((int)length+8)];var categories=LegacyCursiveCategoryProfile.Load(node[65..],offset+65,out int consumed,cancellationToken);
        int p=65+consumed;if(p>node.Length-214||p+214!=node.Length)
            throw new InvalidDataException("Invalid sampler106 scalar tail.");
        //196500 loads48,4c;byte50;54,58,5c,60,68,6c,70,74;
        //then six6-float matrices78..104;108,byte10c;110..124.
        var f=new float[36];for(int i=0;i<f.Length;i++){cancellationToken.ThrowIfCancellationRequested();f[i]=Float(node,p+41+i*4);}
        float pairScale=Float(node,p+9),pairOffset=Float(node,p+13),pairMaximum=Float(node,p+17);
        if(node[p+8]!=1||node[p+189]>1||Float(node,p+21)!=0||pairScale<0||pairMaximum<0)
            throw new NotSupportedException("First ordinary same-row geometry requires enabled pair geometry and zero cross-line cost.");
        int joinCap=BinaryPrimitives.ReadInt32LittleEndian(node[(p+206)..]),matrixCap=BinaryPrimitives.ReadInt32LittleEndian(node[(p+210)..]);
        if(joinCap is <0 or >0x4000000||matrixCap is <0 or >0x4000000)throw new InvalidDataException("Invalid geometry cost caps.");
        int a=checked(sourceOffset+20+4+56);int marker=BinaryPrimitives.ReadInt32LittleEndian(resource[a..]);a+=4;if(marker==-1)a+=4;a+=4;
        if(resource[a++]!=1||BinaryPrimitives.ReadUInt32LittleEndian(resource[a..])!=0x2b)throw new NotSupportedException("Owned acoustic alphabet required.");
        a+=4;ushort version=BinaryPrimitives.ReadUInt16LittleEndian(resource[a..]);a+=2;
        var alphabet=LegacyInkAlphabet.Load(resource[a..],version);
        if(categories.BaseEntryCount!=alphabet.LabelCount)throw new InvalidDataException("Geometry category labels differ from source alphabet.");
        return new(model.RowProfile,alphabet,categories,offset,pairScale,pairOffset,pairMaximum,Float(node,p+29),
            Float(node,p+37),Float(node,p+185),joinCap,matrixCap,f);
    }
    private static float Float(ReadOnlySpan<byte> data,int offset)
    {float value=BinaryPrimitives.ReadSingleLittleEndian(data[offset..]);if(!float.IsFinite(value))throw new InvalidDataException("Nonfinite sampler geometry field.");return value;}
}
