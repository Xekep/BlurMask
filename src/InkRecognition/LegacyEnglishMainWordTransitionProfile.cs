using System.Buffers.Binary;
namespace InkRecognition;

/// <summary>Actual SAME-source MAIN297/108 active118/140 operands. Raw60 is
///signedINT, not an inactive NaN float. Does not widen old v106 admission.</summary>
public sealed class LegacyEnglishMainWordTransitionProfile
{
    private readonly float[] matrix;
    public LegacyEnglishFreshMainWordPolicy Policy{get;}
    public LegacySampler297WireProfile Sampler{get;}
    public bool PairEnabled{get;}
    public float PairScale{get;}
    public float PairOffset{get;}
    public float PairMaximum{get;}
    public int CrossLineCost{get;}
    public float GapOffset{get;}
    public float DefaultGap{get;}
    public float BlankThreshold{get;}
    public float TranslationThreshold{get;}
    public float GapSlope{get;}
    public float SourceGapScale{get;}
    public float SourceMatrixScale{get;}
    public int JoinMaximumCost{get;}
    public int MatrixMaximumCost{get;}
    public int BoundaryMaximum118{get;}
    public int BoundaryThreshold11C{get;}
    private LegacyEnglishMainWordTransitionProfile(LegacyEnglishFreshMainWordPolicy policy,LegacySampler297WireProfile sampler,bool pair,
        float pairScale,float pairOffset,float pairMaximum,int crossLine,float gapOffset,float gap,float triple,float translation,float slope,float gapScale,float matrixScale,int joinMax,int matrixMax,int boundaryMax,int boundaryThreshold,float[] matrix)
    {Policy=policy;Sampler=sampler;PairEnabled=pair;PairScale=pairScale;PairOffset=pairOffset;PairMaximum=pairMaximum;CrossLineCost=crossLine;GapOffset=gapOffset;DefaultGap=gap;BlankThreshold=triple;TranslationThreshold=translation;GapSlope=slope;SourceGapScale=gapScale;SourceMatrixScale=matrixScale;JoinMaximumCost=joinMax;MatrixMaximumCost=matrixMax;BoundaryMaximum118=boundaryMax;BoundaryThreshold11C=boundaryThreshold;this.matrix=matrix;}
    public float[] CopyMatrix()=>matrix.ToArray();
    public LegacyCursiveCategoryProfile ReadCategories(CancellationToken token=default)=>Sampler.ReadCategories(token);
    public static LegacyEnglishMainWordTransitionProfile Prepare(LegacyEnglishFreshMainWordPolicy policy,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(policy);policy.Validate(token);var sampler=policy.QuerySession.Masks.Knowledge.MainWire.Sampler;
        if(!ReferenceEquals(sampler.Source,policy.Source)||sampler.Child.Version!=0x108||sampler.Child.Offset!=182412||sampler.Child.End!=183800)throw new NotSupportedException("Exact same actual MAIN108 sampler required.");
        LegacyResourceWireField Field(string name,int size,int count)
        {var f=sampler.Fields.Single(x=>x.Owner=="297"&&x.Destination==name);if(f.ElementSize!=size||f.Count!=count)throw new InvalidDataException("Whole typed108 fields required before copying.");return f;}
        string[] floats=["54","58","5C","68","6C","70","74","108","110","114"];
        string[] ints=["60","118","11C","120","124"];
        foreach(string n in floats.Concat(ints))_=Field(n,4,1);
        foreach(string n in new[]{"50","10C","134.present","1D0.present"})_=Field(n,1,1);
        for(int row=0;row<6;row++)_=Field($"{0x78+row*24:X}.matrixRow",4,6);
        byte[] Read(string name,int size=4,int count=1){token.ThrowIfCancellationRequested();return Field(name,size,count).CopyBytes();}
        float F(string name){float v=BinaryPrimitives.ReadSingleLittleEndian(Read(name));if(!float.IsFinite(v))throw new InvalidDataException("Finite ACTIVE108 operand required.");return v;}
        int I(string name)=>BinaryPrimitives.ReadInt32LittleEndian(Read(name));
        byte flag=Read("50",1)[0];if(flag!=1||Read("10C",1)[0]!=0||Read("134.present",1)[0]!=0||Read("1D0.present",1)[0]!=0)throw new NotSupportedException("Actual active pair with no optional108 branches required.");
        var matrix=new float[36];for(int row=0;row<6;row++){byte[] bytes=Read($"{0x78+row*24:X}.matrixRow",4,6);for(int col=0;col<6;col++){float v=BinaryPrimitives.ReadSingleLittleEndian(bytes.AsSpan(col*4));if(!float.IsFinite(v))throw new InvalidDataException("Finite matrix operand required.");matrix[row*6+col]=v;}}
        var result=new LegacyEnglishMainWordTransitionProfile(policy,sampler,true,F("54"),F("58"),F("5C"),I("60"),F("68"),F("6C"),F("70"),F("74"),F("108"),F("110"),F("114"),I("120"),I("124"),I("118"),I("11C"),matrix);
        if(result.PairMaximum<0||result.GapSlope<0||result.SourceGapScale<0||result.SourceMatrixScale<0||new[]{result.JoinMaximumCost,result.MatrixMaximumCost,result.BoundaryMaximum118,result.BoundaryThreshold11C}.Any(v=>v is<0 or>0x4000000))throw new NotSupportedException("Bounded ordinary108 cache configuration required.");
        policy.Validate(token);return result;
    }
    internal void Validate(CancellationToken token=default)=>Policy.Validate(token);
}
