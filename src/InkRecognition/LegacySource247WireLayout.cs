using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>One serialized scalar/array, with its native destination offset.
/// Raw bits are retained; reading a layout does not execute the native owner.</summary>
public sealed class LegacyResourceWireField
{
    private readonly byte[] bytes;
    public string Owner {get;}
    public string Destination {get;}
    public int Offset {get;}
    public int ElementSize {get;}
    public int Count {get;}
    internal LegacyResourceWireField(string owner,string destination,int offset,int size,int count,byte[] bytes)
    {Owner=owner;Destination=destination;Offset=offset;ElementSize=size;Count=count;this.bytes=bytes;}
    public byte[] CopyBytes()=>bytes.ToArray();
}

/// <summary>Delegated child envelope, not a deserialized child processor/model.</summary>
public sealed class LegacySource247WireChild
{
    internal LegacySource247WireLayout Owner {get;}
    public string Destination {get;}
    public int Offset {get;}
    public int End {get;}
    public uint Kind {get;}
    public ushort Version {get;}
    internal LegacySource247WireChild(LegacySource247WireLayout owner,LegacyWireChildSpec spec)
    {Owner=owner;Destination=spec.Destination;Offset=spec.Offset;End=spec.End;Kind=spec.Kind;Version=spec.Version;}
}
internal readonly record struct LegacyWireChildSpec(string Destination,int Offset,int End,uint Kind,ushort Version);

/// <summary>Immutable source247/10D or110 wire layout through the exact parent
/// end. Child envelopes retain native ownership/order, but their payloads are
/// not advertised as executable/deserialized processors or acoustic weights.</summary>
public sealed class LegacySource247WireLayout
{
    private readonly byte[] bytes;
    public int Offset {get;}
    public int End=>checked(Offset+bytes.Length);
    public ushort Version {get;}
    public ushort Base134Version {get;}
    public string Fingerprint {get;}
    public LegacyInkAlphabet Alphabet {get;}
    public int SourceKind {get;}
    public byte CoordinateMode {get;}
    public int ParametersOffset {get;}
    public int EnclosureFlagOffset {get;}
    public IReadOnlyList<LegacyResourceWireField> Fields {get;}
    public IReadOnlyList<LegacySource247WireChild> Children {get;}
    private LegacySource247WireLayout(byte[] bytes,int offset,ushort version,ushort baseVersion,
        LegacyInkAlphabet alphabet,int kind,byte mode,int parameters,int enclosure,
        List<LegacyResourceWireField> fields,List<LegacyWireChildSpec> children)
    {
        this.bytes=bytes;Offset=offset;Version=version;Base134Version=baseVersion;Alphabet=alphabet;
        SourceKind=kind;CoordinateMode=mode;ParametersOffset=parameters;EnclosureFlagOffset=enclosure;
        Fingerprint=Convert.ToHexString(SHA256.HashData(bytes));Fields=fields.AsReadOnly();
        Children=Array.AsReadOnly(children.Select(s=>new LegacySource247WireChild(this,s)).ToArray());
    }

    public static LegacySource247WireLayout Read(ReadOnlySpan<byte> resource,int offset=0,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(resource.Length>64*1024*1024||offset<0||offset>resource.Length-16)
            throw new InvalidDataException("Bounded complete source envelope required.");
        var header=resource[offset..];uint length=BinaryPrimitives.ReadUInt32LittleEndian(header[4..]);
        if(BinaryPrimitives.ReadUInt32LittleEndian(header)!=0x101fe90||length<8||length>resource.Length-offset-8||
            BinaryPrimitives.ReadUInt32LittleEndian(header[8..])!=0x247||BinaryPrimitives.ReadUInt16LittleEndian(header[12..])!=0)
            throw new InvalidDataException("Unencoded247 envelope required.");
        ushort version=BinaryPrimitives.ReadUInt16LittleEndian(header[14..]);
        if(version is not(0x10d or 0x110))throw new NotSupportedException("Only proved247 versions10D/110 are supported.");
        byte[] data=header[..checked((int)length+8)].ToArray();
        var r=new LegacyResourceWireReader(data,offset,token);r.Take("247","envelope",1,16);
        ushort baseVersion=r.U16("247","base134Version");
        if(baseVersion is not(0x201 or 0x300)||r.U16("134","base133Version")!=0x100)
            throw new NotSupportedException("Only inline134/201 or300 with133/100 is proved.");
        r.U32("133","10");r.Take("133","14.metadata",1,56);
        int marker=unchecked((int)r.U32("133","identityMarker"));
        if(marker==-1)r.U32("133","4C");r.U32("133","50");
        if(!r.Flag("134","alphabetPresent"))throw new NotSupportedException("Inline actual alphabet is required.");
        if(r.U32("134","alphabetKind")!=0x2b)throw new NotSupportedException("Expected actual2B alphabet.");
        ushort alphabetVersion=r.U16("134","alphabetVersion");
        var alphabet=LegacyInkAlphabet.Load(data.AsSpan(r.Position),alphabetVersion);
        r.Take("2B","payload",1,alphabet.BytesConsumed);
        var children=new List<LegacyWireChildSpec>();
        if(baseVersion>=0x300)
        {
            int count=r.U8("134","70.additionalCount");
            for(int i=0;i<count;i++)children.Add(r.Child($"134.68[{i}]"));
        }
        int kind=unchecked((int)r.U32("247","78"));
        if(kind is <0x134 or >0x137)throw new NotSupportedException("Only native134..137 source kinds are laid out.");
        byte mode=r.U8("247","80");
        if(mode>2)throw new NotSupportedException("Unknown coordinate mode.");
        if(mode==2)
        {
            int n=r.U8("coordinate88","00.count");r.Take("coordinate88","08.values",4,n);
            r.U32("coordinate88","10");r.U32("coordinate88","14");
            foreach(string field in new[]{"18","19","1A","24","25","26","27"})r.U8("coordinate88",field);
        }
        if(version<0x10f)r.U8("247","legacySkippedByte");
        r.Flag("247","B8");r.Flag("247","B9");r.U16("247","BA");r.U16("247","BC");
        int parameters=r.Absolute;
        foreach(string field in new[]{"10","14","18","1C"})r.U32("parametersC0",field);
        bool p5=r.Flag("parametersC0","20"),p6a=r.Flag("parametersC0","21"),
            p7=r.Flag("parametersC0","legacyThirdPresence"),p6b=r.Flag("parametersC0","22");
        r.U32("parametersC0","24");
        if(p5)children.Add(r.Child("parametersC0.28"));
        if(p6a||p6b)children.Add(r.Child("parametersC0.30"));
        if(p7)children.Add(r.Child("parametersC0.38"));
        int enclosure=r.Absolute;
        r.Flag("parametersC0","40");r.Flag("parametersC0","41");r.U32("parametersC0","44");
        r.Flag("parametersC0","48");r.U32("parametersC0","4C");r.Flag("parametersC0","50");
        int processors=r.Count("parametersC0","60.count",256);
        for(int i=0;i<processors;i++)children.Add(r.Child($"parametersC0.58[{i}]"));
        if(version>=0x10e&&r.Flag("247","150.present"))children.Add(r.Child("247.150"));
        if(r.Flag("247","trioPresent"))
        {
            children.Add(r.Child("247.130",0x297));children.Add(r.Child("247.138",0x21b));children.Add(r.Child("247.140",0x21d));
        }
        if(version>=0x110&&r.Flag("247","148.present"))children.Add(r.Child("247.148"));
        int extras=r.Count("247","160.count",256);
        for(int i=0;i<extras;i++)children.Add(r.Child($"247.158[{i}]"));
        foreach(var f in new[]{("16C",4,16),("1AC",2,16),("1CC",2,16),("1EC",1,1),
            ("1F0",4,4),("200",2,4),("208",2,4),("210",1,1),("214",2,6),("220",2,6)})
            r.Take("247",f.Item1,f.Item2,f.Item3);
        r.RequireEnd();return new(data,offset,version,baseVersion,alphabet,kind,mode,parameters,enclosure,r.Fields,children);
    }

    public byte[] CopyChildEnvelopeBytes(LegacySource247WireChild actualChild,CancellationToken token=default)
    {ValidateChild(actualChild,token);return bytes.AsSpan(actualChild.Offset-Offset,actualChild.End-actualChild.Offset).ToArray();}
    internal void ValidateChild(LegacySource247WireChild child,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(child);
        if(!ReferenceEquals(child.Owner,this))throw new ArgumentException("The delegated child belongs to another source layout.");
    }
    public LegacySampler297WireProfile ReadSampler(LegacySource247WireChild actualChild,CancellationToken token=default)
    {ValidateChild(actualChild,token);return LegacySampler297WireProfile.Read(this,actualChild,bytes.AsSpan(actualChild.Offset-Offset,actualChild.End-actualChild.Offset),token);}
}

internal sealed class LegacyResourceWireReader(byte[] data,int originalOffset,CancellationToken token)
{
    internal int Position {get;private set;}
    internal int Absolute=>checked(originalOffset+Position);
    internal List<LegacyResourceWireField> Fields {get;}=[];
    internal byte[] Take(string owner,string destination,int size,int count)
    {
        token.ThrowIfCancellationRequested();
        if(size is not(1 or 2 or 4)||count<0||count>(64*1024*1024)/size||count*size>data.Length-Position)
            throw new InvalidDataException("Truncated/bounded serialized field.");
        int n=count*size;byte[] value=data.AsSpan(Position,n).ToArray();
        Fields.Add(new(owner,destination,Absolute,size,count,value));Position+=n;return value;
    }
    internal byte U8(string owner,string field)=>Take(owner,field,1,1)[0];
    internal ushort U16(string owner,string field)=>BinaryPrimitives.ReadUInt16LittleEndian(Take(owner,field,2,1));
    internal uint U32(string owner,string field)=>BinaryPrimitives.ReadUInt32LittleEndian(Take(owner,field,4,1));
    internal bool Flag(string owner,string field)
    {byte v=U8(owner,field);if(v>1)throw new InvalidDataException("Only canonical serialized Boolean flags are supported.");return v!=0;}
    internal int Count(string owner,string field,int maximum)
    {int n=unchecked((int)U32(owner,field));if(n<0||n>maximum)throw new InvalidDataException("Portable resource-list count exceeded.");return n;}
    internal LegacyWireChildSpec Child(string destination,uint? kind=null)
    {
        token.ThrowIfCancellationRequested();int start=Position;
        if(data.Length-start<16)throw new InvalidDataException("Truncated delegated child header.");
        var d=data.AsSpan(start);uint length=BinaryPrimitives.ReadUInt32LittleEndian(d[4..]),actualKind=BinaryPrimitives.ReadUInt32LittleEndian(d[8..]);
        ushort version=BinaryPrimitives.ReadUInt16LittleEndian(d[14..]);
        if(BinaryPrimitives.ReadUInt32LittleEndian(d)!=0x101fe90||length<8||length>data.Length-start-8||
            BinaryPrimitives.ReadUInt16LittleEndian(d[12..])!=0||actualKind==0||kind is{} expected&&actualKind!=expected)
            throw new InvalidDataException("Invalid unencoded delegated child envelope/type.");
        int end=checked(start+8+(int)length);Position=end;
        return new(destination,checked(originalOffset+start),checked(originalOffset+end),actualKind,version);
    }
    internal void RequireEnd(){token.ThrowIfCancellationRequested();if(Position!=data.Length)throw new InvalidDataException("Unconsumed parent wire fields.");}
}
