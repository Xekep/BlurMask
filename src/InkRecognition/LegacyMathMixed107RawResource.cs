using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

[assembly: InternalsVisibleTo("LegacyMathMixed107Raw.Probe")]

namespace InkRecognition;

/// <summary>Owning, fingerprint-bound projection of six Math raw215/FEA0-107
/// architecture02 models. No features, activations, evaluator212/213 or inference.</summary>
public sealed class LegacyMathMixed107RawResource : IDisposable
{
    public const string ResourceFingerprint = "8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1";
    public const int ResourceBytes = 3_685_881;
    private const int Chunk = 16_384, MatrixChunk = 2_048;
    private readonly object gate = new();
    private readonly Model[] models;
    private readonly Layout[] layouts;
    private readonly Data[] values;
    private readonly IReadOnlyList<Model> view;
    private bool disposed;
    private static readonly (int Start,int End,ushort Kind,int Inputs,int ParentStart,int ParentEnd)[] Actual =
    [ (8702,177650,0x3f,276,7465,177763), (1007739,1151090,0x1f,222,1005980,1789568),
      (1151097,1288050,0x2f,204,1005980,1789568), (1289175,1458130,0x3f,279,1005980,1789568),
      (1459255,1628210,0x4f,279,1005980,1789568), (1629111,1772466,0x5f,223,1005980,1789568) ];
    internal enum Phase { Capture, Preflight, Hash, FirstWeights, SecondWeights, Scales, Publish }
    internal readonly record struct Layout(int Start,int End,ushort Kind,int Inputs,int ParentStart,int ParentEnd,
        int FirstStart,int FirstStride,int SecondBefore,int SecondStart,int SecondStride,int ScaleStart);
    private LegacyMathMixed107RawResource(Layout[] layout,Data[] data)
    {
        layouts=layout;values=data;models = new Model[layout.Length];
        for(int i=0;i<models.Length;i++)models[i]=new Model(this,i);
        view=Array.AsReadOnly(models);
    }
    public IReadOnlyList<Model> Models {get {lock(gate){EnsureLive();return view;}}}
    public bool IsDisposed {get {lock(gate)return disposed;}}
    public static LegacyMathMixed107RawResource LoadActualResource(ReadOnlySpan<byte> source,CancellationToken token=default)
        =>LoadCore(source,null,token);
    internal static LegacyMathMixed107RawResource LoadWithDiagnosticObserver(ReadOnlySpan<byte> source,
        Action<Phase,int,int> observer,CancellationToken token=default)
        =>LoadCore(source,observer??throw new ArgumentNullException(nameof(observer)),token);
    private static void Boundary(Action<Phase,int,int>? observer,Phase phase,int model,int offset,CancellationToken token)
    {token.ThrowIfCancellationRequested();observer?.Invoke(phase,model,offset);token.ThrowIfCancellationRequested();}
    private static LegacyMathMixed107RawResource LoadCore(ReadOnlySpan<byte> source,Action<Phase,int,int>? observer,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(source.Length!=ResourceBytes)throw new ArgumentException("Exact bounded Math resource length required.",nameof(source));
        byte[] capture=new byte[ResourceBytes];
        for(int at=0;at<capture.Length;at+=Chunk)
        {Boundary(observer,Phase.Capture,-1,at,token);source.Slice(at,Math.Min(Chunk,capture.Length-at)).CopyTo(capture.AsSpan(at));}
        //Validate all enclosing native resource boundaries, then all six complete
        //wire layouts and scales. No model matrix payload has been copied yet.
        Frame(capture,0,0x1389,0x109,ResourceBytes);
        if(U16(capture,16)!=0x104)throw new NotSupportedException("Actual base1F47/version104 required.");
        Frame(capture,7432,0x13a3,0x101,180009);
        if(capture[7448]!=1)throw new InvalidDataException("Actual present13A3 model required.");
        Frame(capture,7449,0x13a4,0x105,180009);Frame(capture,7465,0x13a5,0x104,177763);
        Frame(capture,1005964,0x13a4,0x105,1791853);Frame(capture,1005980,0x13a5,0x104,1789568);
        var layout=new Layout[Actual.Length];long elements=0,payloadBytes=0;
        for(int i=0;i<Actual.Length;i++)
        {
            Boundary(observer,Phase.Preflight,i,0,token);var a=Actual[i];
            if(a.Start<a.ParentStart+16||a.End>a.ParentEnd||a.End-a.Start is <33 or >16*1024*1024)
                throw new InvalidDataException("Model container extent mismatch.");
            if(U16(capture,a.Start)!=0xfea0||U16(capture,a.Start+2)!=0x107||U32(capture,a.Start+4)!=a.End-a.Start-8)
                throw new InvalidDataException("Complete FEA0/version107 envelope required.");
            if(capture[a.Start+8]!=2||U32(capture,a.Start+19)!=0||U32(capture,a.Start+23)!=0||U16(capture,a.Start+31)!=10)
                throw new NotSupportedException("Only actual architecture02/no-middle-layer/quantizer10 supported.");
            int inputs=Width(capture,a.Start+11),hidden=Width(capture,a.Start+15),outputs=Width(capture,a.Start+27);
            if(inputs!=a.Inputs||hidden!=201||outputs!=258||U16(capture,a.Start+9)!=a.Kind)
                throw new InvalidDataException("Actual model dimensions/kind differ.");
            int first=Align(checked(a.Start+33)),firstStride=Align(inputs);
            int secondBefore=checked(first+checked(firstStride*200*2)),second=Align(secondBefore),secondStride=Align(hidden);
            int scale=checked(second+checked(secondStride*257));
            if(checked(scale+258)!=a.End)throw new InvalidDataException("Aligned model endpoint mismatch.");
            if(capture[scale]!=1)throw new InvalidDataException("Leading scale1 required by bounded model domain.");
            for(int n=0;n<258;n++)if(capture[scale+n]==0)throw new InvalidDataException("Zero row scale outside model domain.");
            elements=checked(elements+(long)firstStride*200+(long)secondStride*257);
            payloadBytes=checked(payloadBytes+a.End-a.Start);
            if(elements>1_048_576||payloadBytes>1_048_576)throw new InvalidDataException("Whole model work budget exceeded.");
            layout[i]=new(a.Start,a.End,a.Kind,inputs,a.ParentStart,a.ParentEnd,first,firstStride,secondBefore,second,secondStride,scale);
        }
        using(var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256))
        {
            for(int at=0;at<capture.Length;at+=Chunk)
            {Boundary(observer,Phase.Hash,-1,at,token);hash.AppendData(capture,at,Math.Min(Chunk,capture.Length-at));}
            if(!Convert.ToHexString(hash.GetHashAndReset()).Equals(ResourceFingerprint,StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Actual Math resource fingerprint required.");
        }
        var values=new Data[layout.Length];
        for(int i=0;i<layout.Length;i++)
        {
            Layout l=layout[i];var first=new short[checked(l.FirstStride*200)];var second=new sbyte[checked(l.SecondStride*257)];
            for(int n=0;n<first.Length;n++)
            {if((n&(MatrixChunk-1))==0)Boundary(observer,Phase.FirstWeights,i,n,token);first[n]=BinaryPrimitives.ReadInt16LittleEndian(capture.AsSpan(l.FirstStart+n*2,2));}
            for(int n=0;n<second.Length;n++)
            {if((n&(MatrixChunk-1))==0)Boundary(observer,Phase.SecondWeights,i,n,token);second[n]=unchecked((sbyte)capture[l.SecondStart+n]);}
            Boundary(observer,Phase.Scales,i,0,token);
            values[i]=new(first,second,capture.AsSpan(l.ScaleStart,258).ToArray(),
                capture.AsSpan(l.Start+33,l.FirstStart-l.Start-33).ToArray(),capture.AsSpan(l.SecondBefore,l.SecondStart-l.SecondBefore).ToArray());
        }
        Boundary(observer,Phase.Publish,-1,0,token);
        return new(layout,values);
    }
    private static int Align(int value)=>checked(value+15)&~15;
    private static ushort U16(ReadOnlySpan<byte> d,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(d.Slice(at,2));
    private static uint U32(ReadOnlySpan<byte> d,int at)=>BinaryPrimitives.ReadUInt32LittleEndian(d.Slice(at,4));
    private static int Width(ReadOnlySpan<byte>d,int at)
    {uint v=U32(d,at);return v is >=2 and <=4096?(int)v:throw new InvalidDataException("Portable width bound exceeded.");}
    private static void Frame(ReadOnlySpan<byte>d,int at,uint kind,ushort version,int end)
    {
        if(U32(d,at)!=0x0101fe90||U32(d,at+8)!=kind||U16(d,at+12)!=0||U16(d,at+14)!=version||U32(d,at+4)!=end-at-8)
            throw new InvalidDataException("Actual enclosing native frame differs.");
    }
    private void EnsureLive(){if(disposed)throw new ObjectDisposedException(nameof(LegacyMathMixed107RawResource));}
    public void Dispose()
    {lock(gate){if(disposed)return;disposed=true;Array.Clear(values);}}
    private sealed record Data(short[] First,sbyte[] Second,byte[] Scales,byte[] FirstAlignment,byte[] SecondAlignment);
    public sealed class Model
    {
        private readonly LegacyMathMixed107RawResource owner;
        private readonly Layout layout;
        private readonly int index;
        internal Model(LegacyMathMixed107RawResource owner,int index)
        {this.owner=owner;this.index=index;layout=owner.layouts[index];}
        public int NativeRawTypeId=>0x215;
        public int OriginalStart=>layout.Start;
        public int OriginalEnd=>layout.End;
        public ushort Kind=>layout.Kind;
        public int InputWidth=>layout.Inputs;
        public int HiddenRows=>200;
        public int OutputRows=>257;
        public int Quantizer=>10;
        public int Containing13A5Start=>layout.ParentStart;
        public int Containing13A5End=>layout.ParentEnd;
        public Snapshot Capture(CancellationToken token=default)
        {
            lock(owner.gate)
            {
                owner.EnsureLive();token.ThrowIfCancellationRequested();Data d=owner.values[index];
                return new(layout,Copy(d.First,token),Copy(d.Second,token),Copy(d.Scales,token),Copy(d.FirstAlignment,token),Copy(d.SecondAlignment,token));
            }
        }
    }
    private static T[] Copy<T>(T[] source,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var result=new T[source.Length];
        for(int at=0;at<source.Length;at+=MatrixChunk)
        {token.ThrowIfCancellationRequested();Array.Copy(source,at,result,at,Math.Min(MatrixChunk,source.Length-at));}
        token.ThrowIfCancellationRequested();return result;
    }
    /// <summary>Detached historical raw storage. It has no owner lease or evaluator authority.</summary>
    public sealed class Snapshot
    {
        private readonly Layout layout;
        private readonly short[] first;
        private readonly sbyte[] second;
        private readonly byte[] scales,firstAlignment,secondAlignment;
        internal Snapshot(Layout l,short[] first,sbyte[] second,byte[] scales,byte[] firstAlignment,byte[] secondAlignment)
        {layout=l;this.first=first;this.second=second;this.scales=scales;this.firstAlignment=firstAlignment;this.secondAlignment=secondAlignment;}
        public string SourceFingerprint=>ResourceFingerprint;
        public int NativeRawTypeId=>0x215;
        public int OriginalStart=>layout.Start;
        public int OriginalEnd=>layout.End;
        public ushort Kind=>layout.Kind;
        public int InputWidth=>layout.Inputs;
        public int HiddenRows=>200;
        public int OutputRows=>257;
        public int Quantizer=>10;
        public int FirstStride=>layout.FirstStride;
        public int SecondStride=>layout.SecondStride;
        public int FirstWeightStart=>layout.FirstStart;
        public int SecondWeightStart=>layout.SecondStart;
        public int Containing13A5Start=>layout.ParentStart;
        public int Containing13A5End=>layout.ParentEnd;
        public short[] CopyFirstWeights(CancellationToken token=default)=>Copy(first,token);
        public sbyte[] CopySecondWeights(CancellationToken token=default)=>Copy(second,token);
        public byte[] CopyScales(CancellationToken token=default)=>Copy(scales,token);
        public byte[] CopyFirstAlignment(CancellationToken token=default)=>Copy(firstAlignment,token);
        public byte[] CopySecondAlignment(CancellationToken token=default)=>Copy(secondAlignment,token);
        public byte[] CopyEnvelope(CancellationToken token=default)
        {
            token.ThrowIfCancellationRequested();var result=new byte[layout.End-layout.Start];
            W16(result,0,0xfea0);W16(result,2,0x107);W32(result,4,result.Length-8);result[8]=2;
            W16(result,9,layout.Kind);W32(result,11,layout.Inputs);W32(result,15,201);W32(result,27,258);W16(result,31,10);
            firstAlignment.CopyTo(result,33);secondAlignment.CopyTo(result,layout.SecondBefore-layout.Start);
            for(int i=0;i<first.Length;i++)
            {if((i&(MatrixChunk-1))==0)token.ThrowIfCancellationRequested();BinaryPrimitives.WriteInt16LittleEndian(result.AsSpan(layout.FirstStart-layout.Start+i*2,2),first[i]);}
            for(int i=0;i<second.Length;i++)
            {if((i&(MatrixChunk-1))==0)token.ThrowIfCancellationRequested();result[layout.SecondStart-layout.Start+i]=unchecked((byte)second[i]);}
            scales.CopyTo(result,layout.ScaleStart-layout.Start);token.ThrowIfCancellationRequested();return result;
        }
        private static void W16(Span<byte>d,int at,ushort v)=>BinaryPrimitives.WriteUInt16LittleEndian(d.Slice(at,2),v);
        private static void W32(Span<byte>d,int at,int v)=>BinaryPrimitives.WriteInt32LittleEndian(d.Slice(at,4),v);
    }
}
