using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>One immutable serialized tensor range. This describes retained wire
/// bytes; it does not claim a native matrix allocation, converted pointer or evaluator.</summary>
public sealed record LegacySelected106Tensor(string Name,int Kind,int Offset,int Bytes,
    int Rows,int Columns,int Input,int Code);
public sealed record LegacySelected106Layer(int Kind,int Width,int Code);

/// <summary>Data-only selected full-English 5F/106 wire owner. The actual Source247
/// child and absolute serialization origin are authenticated before copying. The
/// existing NativeEnglishTextResource converter remains the sole portable writer.
/// Native temporary-alignment, conversion/alias branches and CTC execution are not
/// represented by this wire layout.</summary>
public sealed class LegacySelectedEnglish106WireLayout
{
    private readonly byte[] childBytes;
    private readonly int payloadLocal,paddingLocal,tailLocal;
    private readonly LegacySource247WireLayout source;
    private readonly LegacySource247WireChild child;
    public int NetworkOffset {get;}
    public int NetworkEnd {get;}
    public int PayloadOffset {get;}
    public int PayloadBytes {get;}
    public int PaddingOffset {get;}
    public int PaddingBytes {get;}
    public int TailOffset {get;}
    public int TailBytes {get;}
    public IReadOnlyList<LegacySelected106Layer> Layers {get;}
    public IReadOnlyList<LegacySelected106Tensor> Tensors {get;}
    private LegacySelectedEnglish106WireLayout(LegacySource247WireLayout source,
        LegacySource247WireChild child,byte[] bytes,int networkOffset,int networkEnd,
        int payload,int payloadBytes,int padding,int paddingBytes,int tail,int tailBytes,
        LegacySelected106Layer[] layers,List<LegacySelected106Tensor> tensors)
    {
        this.source=source;this.child=child;childBytes=bytes;
        NetworkOffset=networkOffset;NetworkEnd=networkEnd;PayloadOffset=payload;
        PayloadBytes=payloadBytes;PaddingOffset=padding;PaddingBytes=paddingBytes;
        TailOffset=tail;TailBytes=tailBytes;payloadLocal=payload-child.Offset;
        paddingLocal=padding-child.Offset;tailLocal=tail-child.Offset;
        Layers=Array.AsReadOnly(layers);Tensors=tensors.AsReadOnly();
    }
    public byte[] CopyPayload(CancellationToken token=default)=>Copy(payloadLocal,PayloadBytes,token);
    public byte[] CopyAlignmentPadding(CancellationToken token=default)=>Copy(paddingLocal,PaddingBytes,token);
    public byte[] CopyOpaqueTail(CancellationToken token=default)=>Copy(tailLocal,TailBytes,token);
    private byte[] Copy(int offset,int count,CancellationToken token)
    {token.ThrowIfCancellationRequested();var result=childBytes.AsSpan(offset,count).ToArray();token.ThrowIfCancellationRequested();return result;}
    public bool BelongsTo(LegacySource247WireLayout sourceOwner,LegacySource247WireChild sourceChild)
        =>ReferenceEquals(source,sourceOwner)&&ReferenceEquals(child,sourceChild);

    public static LegacySelectedEnglish106WireLayout Read(LegacySource247WireLayout source,
        LegacySource247WireChild child,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(source);ArgumentNullException.ThrowIfNull(child);
        token.ThrowIfCancellationRequested();source.ValidateChild(child,token);
        Require(source.Offset==0&&source.End==3783049&&source.Version==0x110&&source.SourceKind==0x135&&
            source.Alphabet.LabelCount==217&&source.Fingerprint.Equals(NativeEnglishTextResource.SourceSha256,StringComparison.OrdinalIgnoreCase),
            "Pinned complete English source owner required.");
        Require(child.Destination=="247.148"&&child.Offset==1774246&&child.End==3782859&&
            child.Kind==0x21e&&child.Version==0x103,"Actual selected English adapter child required.");
        // This fixed child length bounds the only retained copy before allocation.
        Require(child.End-child.Offset==2008613,"Bounded selected child required.");
        byte[] bytes=source.CopyChildEnvelopeBytes(child,token);
        var r=new Reader(bytes,child.Offset,child.Offset,child.End);
        r.Header(0x21e,0x103,child.End);
        Require(r.U32()==1&&r.U32()==0&&r.U32()==4,"Actual feature scheme required.");
        int wrapper=r.Position;Require(wrapper==1774274,"Actual wrapper position required.");
        int wrapperEnd=r.Header(0x53,0x100);
        Require(r.Take(9).SequenceEqual(new byte[]{1,1,0,0,0,1,0,0,0}),"Actual opaque wrapper prefix required.");
        int network=r.Position;int networkEnd=r.Header(0x5f,0x106,wrapperEnd);
        Require(network==1774299&&networkEnd==3782348,"Actual selected network required.");
        r.End=networkEnd;
        Require(r.U32()==1&&r.U32()==4,"Actual sequential four-layer network required.");
        var layers=new LegacySelected106Layer[4];
        for(int i=0;i<layers.Length;i++)
        {
            token.ThrowIfCancellationRequested();int kind=r.U32(),width=r.U32(),flags=r.U32(),code=r.U32();
            Require(kind==(i==0?1:8)&&width==(i==0?13:127)&&flags==0&&code==2,"Actual selected layer metadata required.");
            layers[i]=new(kind,width,code);
        }
        foreach(int expected in new[]{0,182,1,0,0,0,0,0,0,1,1,1,3,2,0,8,2007776})
            Require(r.U32()==expected,"Actual selected network metadata required.");
        int padding=r.Position;int paddingBytes=(-padding)&31;
        Require(padding==1774455&&paddingBytes==9,"Absolute serialization alignment required.");
        foreach(byte value in r.Take(paddingBytes))Require(value==255,"Actual alignment padding required.");
        int payload=r.Position;const int payloadBytes=2007776;r.Take(payloadBytes);
        int tailBytes=r.U32();Require(tailBytes==104,"Actual opaque tail length required.");
        int tail=r.Position;r.Take(tailBytes);Require(r.Position==r.End,"Unconsumed selected network bytes.");
        var tensors=new List<LegacySelected106Tensor>(67);int cursor=0;
        static int Align(int value,int multiple=8)=>checked((value+multiple-1)/multiple*multiple);
        void Matrix(string name,int rows,int input)
        {
            int columns=Align(Align(input+1),16);int count=checked(rows*columns*2+Align(rows)*8+32);
            Require(cursor<=payloadBytes-count,"Matrix exceeds serialized payload.");
            tensors.Add(new(name,1,cursor,count,rows,columns,input,2));cursor=checked(cursor+count);
        }
        int previous=13;
        for(int layer=1;layer<4;layer++)
        {
            token.ThrowIfCancellationRequested();
            foreach(string role in new[]{"input","recurrent"})foreach(string direction in new[]{"forward","backward"})
            {
                foreach(string gate in new[]{"g","i","f","o"})Matrix($"layer{layer}.{direction}.{role}.{gate}",127,role=="input"?previous:127);
                if(role=="recurrent")foreach(string gate in new[]{"i","f","o"})
                {
                    int count=Align(127)*4;Require(cursor<=payloadBytes-count,"Peephole exceeds serialized payload.");
                    tensors.Add(new($"layer{layer}.{direction}.recurrent.p{gate}",2,cursor,count,127,1,0,1));cursor=checked(cursor+count);
                }
            }
            previous=254;
        }
        Matrix("output",182,254);Require(cursor==payloadBytes&&tensors.Count==67,"Whole serialized tensor partition required.");
        token.ThrowIfCancellationRequested();
        return new(source,child,bytes,network,networkEnd,payload,payloadBytes,padding,paddingBytes,tail,tailBytes,layers,tensors);
    }
    private static void Require(bool condition,string message)
    {if(!condition)throw new InvalidDataException(message);}
    private sealed class Reader(byte[] bytes,int origin,int start,int end)
    {
        public int Position {get;private set;}=start;
        public int End {get;set;}=end;
        public ReadOnlySpan<byte> Take(int count)
        {
            Require(count>=0&&Position>=origin&&End>=origin&&End-origin<=bytes.Length&&Position<=End-count,"Truncated selected network field.");
            var result=bytes.AsSpan(Position-origin,count);Position+=count;return result;
        }
        public int U32(){uint v=BinaryPrimitives.ReadUInt32LittleEndian(Take(4));Require(v<=int.MaxValue,"Selected integer exceeds bounded range.");return (int)v;}
        public int Header(uint kind,ushort version,int? exactEnd=null)
        {
            int at=Position;var raw=Take(16);uint length=BinaryPrimitives.ReadUInt32LittleEndian(raw[4..]);
            Require(BinaryPrimitives.ReadUInt32LittleEndian(raw)==0x101fe90&&length>=8&&length<=(uint)(End-at-8)&&
                BinaryPrimitives.ReadUInt32LittleEndian(raw[8..])==kind&&BinaryPrimitives.ReadUInt16LittleEndian(raw[12..])==0&&
                BinaryPrimitives.ReadUInt16LittleEndian(raw[14..])==version,"Invalid selected resource header.");
            int result=checked(at+8+(int)length);Require(!exactEnd.HasValue||result==exactEnd.Value,"Selected child end mismatch.");return result;
        }
    }
}
