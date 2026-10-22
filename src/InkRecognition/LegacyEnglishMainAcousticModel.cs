using System.Runtime.CompilerServices;
using System.Buffers.Binary;
[assembly:InternalsVisibleTo("LegacyEnglishMainAcoustic.Probe")]
namespace InkRecognition;

/// <summary>Actual same-Source four MAIN107 matrices/1111 profile. No FIRST/RU
///model, supplied feature arrays, configured fullroot or accepted text owner.</summary>
public sealed class LegacyEnglishMainAcousticModel
{
    private static readonly ConditionalWeakTable<LegacySource247WireLayout,LegacyEnglishMainAcousticModel> Shared=new();
    private readonly LegacyEnglishMainNetwork[] networks;
    public LegacySource247WireLayout Source{get;}
    public int FamilyCount=>4;
    public int ClassCount=>182;
    public long CopiedMatrixPayloadBytes{get;}
    public LegacyDynamicAcousticWeightProfile DynamicProfile{get;}
    public uint FreshMode13Flags{get;}
    public bool PruneCandidates{get;}
    internal LegacyOrdinaryAcousticCacheProfile CacheProfile{get;}
    public string[] CopyNetworkFingerprints()=>networks.Select(n=>n.Wire.Fingerprint).ToArray();
    public int[] CopyNetworkOffsets()=>networks.Select(n=>n.Wire.Offset).ToArray();
    internal static LegacyEnglishMainAcousticModel Prepare(LegacyEnglishMainOnlySourceMasks masks,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(masks);token.ThrowIfCancellationRequested();
        if(!masks.Knowledge.Owner.IsComplete||masks.Knowledge.Owner.IsFaulted)throw new InvalidOperationException("Complete actual MAIN owner required.");
        _=LegacyEnglishMainAcousticInputs.FromIndependent(masks); // Validate policy even if case preparation populated Shared first.
        var model=Shared.GetValue(masks.Source,_=>new(masks,token));
        for(int i=0;i<4;i++){token.ThrowIfCancellationRequested();var n=masks.Knowledge.MainWire.Networks[i];if(!ReferenceEquals(model.Source,n.Source)||model.networks[i].Wire.Offset!=n.Offset||model.networks[i].Wire.Fingerprint!=n.Fingerprint)throw new ArgumentException("Same actual MAIN network lineage required.");}
        return model;
    }
    // PROPOSED typed factory: no supplied mask bytes or old-owner coercion.
    internal static LegacyEnglishMainAcousticModel PrepareFreshCase(LegacyEnglishMainAcousticInputs inputs,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(inputs);inputs.RequireFreshCase();inputs.RequireCurrent();token.ThrowIfCancellationRequested();
        var model=Shared.GetValue(inputs.Source,_=>new(inputs,token));
        for(int i=0;i<4;i++){token.ThrowIfCancellationRequested();var n=inputs.Knowledge.MainWire.Networks[i];if(!ReferenceEquals(model.Source,n.Source)||model.networks[i].Wire.Offset!=n.Offset||model.networks[i].Wire.Fingerprint!=n.Fingerprint)throw new ArgumentException("Same actual MAIN network lineage required.");}
        inputs.RequireCurrent();return model;
    }
    private LegacyEnglishMainAcousticModel(LegacyEnglishMainOnlySourceMasks masks,CancellationToken token)
        :this(LegacyEnglishMainAcousticInputs.FromIndependent(masks),token){}
    private LegacyEnglishMainAcousticModel(LegacyEnglishMainAcousticInputs inputs,CancellationToken token)
    {
        inputs.RequireCurrent();Source=inputs.Source;var wire=inputs.Knowledge.MainWire;var fields=wire.Fields;
        byte[] Field(string owner,string name,int size,int count)
        {token.ThrowIfCancellationRequested();var f=fields.Single(f=>f.Owner==owner&&f.Destination==name);if(f.ElementSize!=size||f.Count!=count)throw new InvalidDataException("Exact bounded MAIN field required.");return f.CopyBytes();}
        float F(string scope,string name)=>BinaryPrimitives.ReadSingleLittleEndian(Field(scope,name,4,1));
        int I(string name)=>BinaryPrimitives.ReadInt32LittleEndian(Field("21D",name,4,1));
        ushort H(string name)=>BinaryPrimitives.ReadUInt16LittleEndian(Field("21D",name,2,1));
        if(!ReferenceEquals(Source,wire.Source)||wire.Networks.Count!=4||inputs.NeuralClassCount!=182)
            throw new ArgumentException("Actual completed independent-main mask owner required.");
        if(!Field("profile0","14..44.raw",1,44).AsSpan().SequenceEqual(Convert.FromHexString("11113F0000001E00100008000A001111FF1F1E00FF010000FF1F0E00FF1F0E00FF000000640000000A000000"))||
            !Field("profile0","88..9E.raw",1,16).AsSpan().SequenceEqual(Convert.FromHexString("01020000000000000000000000000000")))throw new NotSupportedException("Actual1111/2B1 profile required.");
        foreach(var entry in new[]{("48",new ushort[]{16}),("58",new ushort[]{4,4}),("70",new ushort[]{5,5})})
        {if(BinaryPrimitives.ReadUInt16LittleEndian(Field("profile0",entry.Item1+".count",2,1))!=1)throw new NotSupportedException("Actual single window/grid family required.");byte[] v=Field("profile0",entry.Item1+".values",2,entry.Item2.Length);for(int i=0;i<entry.Item2.Length;i++)if(BinaryPrimitives.ReadUInt16LittleEndian(v.AsSpan(i*2))!=entry.Item2[i])throw new NotSupportedException("Actual window/grid dimensions required.");}
        if(Field("profile0","optionalA0",1,1)[0]!=0||Field("profile0","optionalPair",1,1)[0]!=0||Field("21D","78",1,1)[0]!=0||I("108")!=0||I("128")!=0||Field("21D","148/150.present",1,1)[0]!=0)
            throw new NotSupportedException("Optional calibration/correction/class-mask paths are excluded.");
        //Actual1B95A0 checks BOTH148/150 pointers before any source-count
        //mask edit. The exact119 reader's absent flag initializes bothNULL;
        //the earlier present-flag check therefore proves the filter is a no-op.
        //FF/FF are normalization flags, never substituted for count bounds.
        if(Field("21D","FC",1,1)[0]!=1)throw new NotSupportedException("Actual graph-span+1 power count required.");
        long bytes=0;foreach(var n in wire.Networks)foreach(var m in n.Matrices){token.ThrowIfCancellationRequested();bytes=checked(bytes+(long)m.Rows*m.Stride+m.Rows+1);}
        if(bytes!=360960||bytes+32768>1048576)throw new ArgumentException("Bounded exact four-network payload required.");CopiedMatrixPayloadBytes=bytes;
        int samples=BinaryPrimitives.ReadUInt16LittleEndian(Field("257","08.sampleCount",2,1));
        DynamicProfile=new(samples,F("257","0C"),F("257","10"),F("257","14"),F("257","18"),F("257","1C"),F("257","20"),F("257","24"),F("257","28"));
        byte[] meta=Source.Fields.Single(f=>f.Owner=="247"&&f.Destination=="16C").CopyBytes();FreshMode13Flags=0x21703u&~BinaryPrimitives.ReadUInt32LittleEndian(meta.AsSpan(13*4));PruneCandidates=Source.Fields.Single(f=>f.Owner=="247"&&f.Destination=="1EC").CopyBytes()[0]>13;
        if(FreshMode13Flags!=0x21400||!PruneCandidates)throw new NotSupportedException("Actual fresh mode13/variant2 configuration required.");
        CacheProfile=new(F("21D","7C"),F("21D","A4"),F("21D","A8"),F("21D","AC"),F("21D","B0"),F("21D","B4"),I("E8"),H("EC"),H("EE"));
        int[] hidden=new int[48],output=new int[128];for(int i=0;i<48;i++){token.ThrowIfCancellationRequested();hidden[i]=(int)Math.Floor(1024*Math.Tanh(3d*i/47));}for(int i=0;i<127;i++)output[i]=(int)Math.Floor(1024/(1+Math.Exp(-8d*i/127)));output[127]=1024;
        var activations=new LegacyInkActivations(hidden,output);networks=new LegacyEnglishMainNetwork[4];for(int i=0;i<4;i++){token.ThrowIfCancellationRequested();networks[i]=new(Source,wire.Networks[i],i,activations,token);}token.ThrowIfCancellationRequested();
    }
    internal LegacyEnglishMainNetworkResult Evaluate(int slot,ReadOnlySpan<short> input,CancellationToken token)=>networks[slot].Evaluate(input,token);
}

internal sealed class LegacyEnglishMainNetwork
{
    private readonly sbyte[][] weights;
    private readonly byte[][] scales;
    private readonly LegacyInkActivations activations;
    internal LegacyInkNetwork107Wire Wire{get;}
    internal int Slot{get;}
    internal LegacyEnglishMainNetwork(LegacySource247WireLayout source,LegacyInkNetwork107Wire wire,int slot,LegacyInkActivations activations,CancellationToken token)
    {
        Wire=wire;Slot=slot;this.activations=activations;int[] offsets=[184305,270606,353694,449582],widths=[228,210,285,285];
        if(!ReferenceEquals(wire.Source,source)||wire.Offset!=offsets[slot]||wire.InputWidth!=widths[slot]||wire.HiddenWidth!=200||wire.OutputWidth!=182||wire.Kind!=0x1f+slot*16)throw new ArgumentException("Actual MAIN107 matrix owner required.");
        weights=new sbyte[2][];scales=new byte[2][];for(int layer=0;layer<2;layer++){token.ThrowIfCancellationRequested();weights[layer]=wire.CopyMatrixWeights(layer);scales[layer]=wire.CopyMatrixScales(layer);var m=wire.Matrices[layer];for(int row=0;row<m.Rows;row++){if((row&15)==0)token.ThrowIfCancellationRequested();for(int col=m.Columns;col<m.Stride;col++)if(weights[layer][row*m.Stride+col]!=0)throw new NotSupportedException("Nonzero padding needs an actual padded-input owner.");}}
    }
    internal LegacyEnglishMainNetworkResult Evaluate(ReadOnlySpan<short> input,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(input.Length!=Wire.InputWidth)throw new ArgumentException("Actual complete MAIN vector required.");
        int Dot(ReadOnlySpan<short> v,int layer,int row){int sum=0,offset=row*Wire.Matrices[layer].Stride;unchecked{for(int i=0;i<v.Length;i++)sum+=v[i]*weights[layer][offset+i];return sum*scales[layer][row+1];}}
        int[] hiddenDots=new int[200],raw=new int[182];short[] hidden=new short[201];hidden[0]=1024;
        for(int row=0;row<200;row++){if((row&15)==0)token.ThrowIfCancellationRequested();int dot=Dot(input,0,row);if(dot==int.MinValue)throw new NotSupportedException("INT_MIN activation is outside the proved LUT domain.");hiddenDots[row]=dot;hidden[row+1]=(short)activations.Hidden(dot);}
        for(int row=0;row<182;row++){if((row&15)==0)token.ThrowIfCancellationRequested();raw[row]=Dot(hidden,1,row);}
        //Actual raw78=0 chooses unmasked mixed EXP/sigmoid special180.212V78
        //andV80 are the pinned RET0 no-op, not a missing renormalization stage.
        int[] ordinary=new int[181];for(int i=0,p=0;i<182;i++)if(i!=180)ordinary[p++]=raw[i];var exp=LegacyClassScores.Convert(ordinary,token);int[] probabilities=new int[182];for(int i=0,p=0;i<182;i++)if(i!=180)probabilities[i]=exp.Probability32768.Span[p++];
        float argument=((float)raw[180]*-2f)/1048576f;probabilities[180]=argument>75f?0:(int)(32768d/(Math.Exp((double)argument)+1d)+.5d);
        token.ThrowIfCancellationRequested();return new(Slot,hiddenDots,hidden,raw,probabilities,exp.StabilizationOffset);
    }
}
internal sealed record LegacyEnglishMainNetworkResult(int Slot,int[] HiddenDots,short[] Hidden,int[] Raw,int[] Probabilities,int StabilizationOffset);
