using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
[assembly:InternalsVisibleTo("LegacyEnglishLkSparse101.Probe")]
namespace InkRecognition;

public enum LegacyEnglishSparsePackedField{WordClasses,Order2Labels,Order3Labels,Order1Offsets,Order2Offsets}
public readonly record struct LegacyEnglishSparseField(string Name,int Offset,int End,string Sha256);
/// <summary>Actual English2D2/v101 DATA; no query/controller/masks owner.</summary>
public sealed partial class LegacyEnglishLkSparseLanguageData
{
    internal const int AbsoluteStart=1252412,AbsoluteEnd=2832716;
    private readonly byte[] wire;
    private readonly LegacyEnglishSparseField[] fields;
    private readonly Dictionary<string,(int Offset,int Length)> slices;
    private readonly Dictionary<string,Packed> packed;
    private readonly record struct Packed(int Offset,int Count,int Width);
    public LegacyEnglishLkPackedData DataOwner{get;}
    public int WordSlots=>260075;
    public int ClassSlots=>1024;
    public int Order=>3;
    public int ReservedWordIndex=>260074;
    public int StoredDefault268C=>unchecked((int)0xfc000000);
    public long RetainedLanguageWireBytes=>wire.LongLength;
    private LegacyEnglishLkSparseLanguageData(LegacyEnglishLkPackedData owner,byte[] raw,
        LegacyEnglishSparseField[] fields,Dictionary<string,(int,int)> slices,Dictionary<string,Packed> packed)
    {DataOwner=owner;wire=raw;this.fields=fields;this.slices=slices;this.packed=packed;}
    public static LegacyEnglishLkSparseLanguageData Read(LegacyEnglishLkPackedData owner,ReadOnlySpan<byte> actualLexical,CancellationToken token=default)
        =>ReadCore(owner,actualLexical,null,true,token);
    internal static LegacyEnglishLkSparseLanguageData ReadPrepared(LegacyEnglishLkPackedData owner,ReadOnlySpan<byte> raw,Action<string>? checkpoint,bool checkHash,CancellationToken token=default)
        =>ReadCore(owner,raw,checkpoint,checkHash,token);
    private static LegacyEnglishLkSparseLanguageData ReadCore(LegacyEnglishLkPackedData owner,ReadOnlySpan<byte> input,Action<string>? checkpoint,bool checkHash,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(owner);token.ThrowIfCancellationRequested();LegacyEnglishSparseCopyBudget.Preflight();
        if(input.Length!=3300812||owner.Source.SourceKind!=0x135||owner.Source.Version!=0x110||owner.AcousticAlphabet.LabelCount!=217||
            !owner.Source.Fingerprint.Equals("ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657",StringComparison.OrdinalIgnoreCase)||owner.Prefix.RuntimeLanguageScale!=1||owner.Fingerprint!="9f4ef50bca4a1e3cc42189c279b72bf8fdf03f62e638673079d57075e168f7c9")throw new InvalidDataException("Actual selected English data owner required.");
        void Check(string s){checkpoint?.Invoke(s);token.ThrowIfCancellationRequested();}
        Check("sha:input");if(checkHash)Hash(input,token,checkpoint,"sha:input:chunk");Check("copy:resource");byte[] snapshot=input.ToArray();
        Check("sha:snapshot");if(checkHash)Hash(snapshot,token,checkpoint,"sha:snapshot:chunk");
        if(BinaryPrimitives.ReadUInt32LittleEndian(snapshot.AsSpan(184))!=0x101fe90||BinaryPrimitives.ReadUInt32LittleEndian(snapshot.AsSpan(188))!=2832524||BinaryPrimitives.ReadUInt32LittleEndian(snapshot.AsSpan(192))!=0x13b||BinaryPrimitives.ReadUInt16LittleEndian(snapshot.AsSpan(196))!=0||BinaryPrimitives.ReadUInt16LittleEndian(snapshot.AsSpan(198))!=0x300)throw new InvalidDataException("Actual main13B envelope required.");
        Check("copy:language");byte[] raw=snapshot.AsSpan(AbsoluteStart,AbsoluteEnd-AbsoluteStart).ToArray();
        if(BinaryPrimitives.ReadUInt32LittleEndian(raw)!=0x101fe90||BinaryPrimitives.ReadUInt32LittleEndian(raw.AsSpan(4))!=raw.Length-8||BinaryPrimitives.ReadUInt32LittleEndian(raw.AsSpan(8))!=0x2d2||BinaryPrimitives.ReadUInt16LittleEndian(raw.AsSpan(12))!=0||BinaryPrimitives.ReadUInt16LittleEndian(raw.AsSpan(14))!=0x101)throw new InvalidDataException("Actual sparse2D2/v101 required.");
        var fs=new List<LegacyEnglishSparseField>(75);var slices=new Dictionary<string,(int Offset,int Length)>(75,StringComparer.Ordinal);var ps=new Dictionary<string,Packed>(5,StringComparer.Ordinal);int at=16;
        (int Offset,int Length) Take(int length,string name)
        {
            token.ThrowIfCancellationRequested();if(length<0||at>raw.Length-length)throw new InvalidDataException("Whole bounded2D2 field required.");int start=at;at=checked(at+length);
            using var h=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);for(int n=0;n<length;n+=32768){token.ThrowIfCancellationRequested();h.AppendData(raw.AsSpan(start+n,Math.Min(32768,length-n)));}
            fs.Add(new(name,AbsoluteStart+start,AbsoluteStart+at,Convert.ToHexString(h.GetHashAndReset()).ToLowerInvariant()));slices.Add(name,(start,length));return(start,length);
        }
        uint U(string name){var s=Take(4,name);return BinaryPrimitives.ReadUInt32LittleEndian(raw.AsSpan(s.Offset));}
        void Equal(uint value,uint expected){if(value!=expected)throw new InvalidDataException("Actual English sparse field value required.");}
        var version=Take(2,"2CD.version");if(BinaryPrimitives.ReadUInt16LittleEndian(raw.AsSpan(version.Offset))!=0x102)throw new InvalidDataException("Actual inherited2CD/v102 required.");
        var inherited=Take(12,"2CD.64,68,6C");if(BinaryPrimitives.ReadUInt32LittleEndian(raw.AsSpan(inherited.Offset))!=260074||BinaryPrimitives.ReadUInt64LittleEndian(raw.AsSpan(inherited.Offset+4))!=ulong.MaxValue)throw new InvalidDataException("Actual reserved word row required.");
        Take(72,"138.baseMetadata");Equal(U("26AC.maxOrder"),3);Equal(U("wire.flags"),7);Equal(U("70"),260075);Equal(U("74"),1024);var sizes=Take(12,"78");int[] lengths=[1024,113643,155484];for(int i=0;i<3;i++)Equal(BinaryPrimitives.ReadUInt32LittleEndian(raw.AsSpan(sizes.Offset+i*4)),(uint)lengths[i]);
        void ReadPacked(string name,int expectedCount,int expectedWidth)
        {
            Equal(U(name+".version"),0x01000001);Take((-(AbsoluteStart+at))&3,name+".countAlign");Equal(U(name+".count"),(uint)expectedCount);var width=Take(1,name+".width");if(raw[width.Offset]!=expectedWidth)throw new InvalidDataException("Actual packed width required.");var metadata=Take(1,name+".metadata");if(raw[metadata.Offset]!=1)throw new InvalidDataException("Actual packed metadata required.");Take((-(AbsoluteStart+at))&3,name+".dataAlign");int bytes=checked(((expectedCount*expectedWidth+31)/32)*4);var bits=Take(bytes,name+".data");ps.Add(name,new(bits.Offset,expectedCount,expectedWidth));
        }
        ReadPacked("88",260075,10);Take(260075,"98.wordCodes");Equal(U("A0"),256);Take(1024,"A8");ReadPacked("B0.1",113643,10);ReadPacked("B0.2",155484,10);ReadPacked("B8.0",1025,17);ReadPacked("B8.1",113644,18);
        for(int i=0;i<3;i++)Take(lengths[i],"C0."+i);for(int i=0;i<2;i++)Take(lengths[i],"C8."+i);
        var d0=Take(12,"D0");for(int i=0;i<3;i++)Equal(BinaryPrimitives.ReadUInt32LittleEndian(raw.AsSpan(d0.Offset+i*4)),256);for(int i=0;i<3;i++)Take(1024,"D8."+i);
        var e0=Take(12,"E0");for(int i=0;i<3;i++)Equal(BinaryPrimitives.ReadUInt32LittleEndian(raw.AsSpan(e0.Offset+i*4)),i<2?256u:0u);for(int i=0;i<2;i++)Take(1024,"E8."+i);
        for(int i=0;i<4;i++)Take(12,"F0."+i);Take(16,"110");Take(16,"120");for(int i=0;i<4;i++)Take(12,"130."+i);Take(16,"266C");Take(16,"267C");var cp=Take(2,"150.casePathVersion");if(BinaryPrimitives.ReadUInt16LittleEndian(raw.AsSpan(cp.Offset))!=0x200)throw new InvalidDataException("Actual casePath0200 required.");Take(16,"154");Take(16,"164");Take(0x2a4,"174");Take(0x2254,"418");
        if(at!=raw.Length||fs.Count!=75)throw new InvalidDataException("Whole actual sparse partition required.");
        Check("validate:packed");var result=new LegacyEnglishLkSparseLanguageData(owner,raw,fs.ToArray(),slices,ps);
        for(int i=0;i<260075;i++){if((i&4095)==0)Check("validate:wordClasses");if(result.PackedValue(ps["88"],i)>=1024)throw new InvalidDataException("Bounded word class required.");}
        if(result.PackedValue(ps["88"],260074)!=0)throw new InvalidDataException("Actual reserved initial class0 required.");
        for(int level=0;level<2;level++)
        {
            var offsets=ps["B8."+level];var labels=ps["B0."+(level+1)];if(result.PackedValue(offsets,0)!=0||result.PackedValue(offsets,offsets.Count-1)!=labels.Count)throw new InvalidDataException("Whole sparse endpoint required.");
            for(int i=0;i<offsets.Count-1;i++)
            {
                if((i&4095)==0)Check("validate:rows");int first=result.PackedValue(offsets,i),last=result.PackedValue(offsets,i+1);if(first>last||last>labels.Count)throw new InvalidDataException("Monotone sparse offsets required.");int previous=-1;
                for(int j=first;j<last;j++){if((j&4095)==0)token.ThrowIfCancellationRequested();int label=result.PackedValue(labels,j);if(label<=previous||label>=1024)throw new InvalidDataException("Strict bounded sparse labels required.");previous=label;}
            }
        }
        //Actual byte-coded tables have256 entries, so every byte is in range.
        //Reject nonfinite serialized coefficients before a future numeric copy.
        foreach(string name in new[]{"F0.0","F0.1","F0.2","F0.3","110","120","130.0","130.1","130.2","130.3","266C","267C","154","164","174","418"})
            for(int i=0;i<result.slices[name].Length/4;i++){if((i&255)==0)token.ThrowIfCancellationRequested();if(!float.IsFinite(result.Float(name,i)))throw new InvalidDataException("Finite actual language coefficient required.");}
        Check("publish");return result;
    }
    private static void Hash(ReadOnlySpan<byte> bytes,CancellationToken token,Action<string>? checkpoint,string stage)
    {
        using var h=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);for(int at=0;at<bytes.Length;at+=32768){checkpoint?.Invoke(stage);token.ThrowIfCancellationRequested();h.AppendData(bytes.Slice(at,Math.Min(32768,bytes.Length-at)));}
        if(!Convert.ToHexString(h.GetHashAndReset()).Equals("9F4EF50BCA4A1E3CC42189C279B72BF8FDF03F62E638673079D57075E168F7C9",StringComparison.Ordinal))throw new InvalidDataException("Exact actual English LK fingerprint required.");token.ThrowIfCancellationRequested();
    }
    private static string Key(LegacyEnglishSparsePackedField field)=>field switch{LegacyEnglishSparsePackedField.WordClasses=>"88",LegacyEnglishSparsePackedField.Order2Labels=>"B0.1",LegacyEnglishSparsePackedField.Order3Labels=>"B0.2",LegacyEnglishSparsePackedField.Order1Offsets=>"B8.0",LegacyEnglishSparsePackedField.Order2Offsets=>"B8.1",_=>throw new ArgumentOutOfRangeException(nameof(field))};
    private int PackedValue(Packed p,int index)
    {
        if((uint)index>=(uint)p.Count)throw new ArgumentOutOfRangeException(nameof(index));int bit=checked(index*p.Width),q=p.Offset+(bit>>5)*4,s=bit&31;uint value=BinaryPrimitives.ReadUInt32LittleEndian(wire.AsSpan(q))>>s;if(s+p.Width>32)value|=BinaryPrimitives.ReadUInt32LittleEndian(wire.AsSpan(q+4))<<(32-s);return(int)(value&((1u<<p.Width)-1));
    }
    public int GetPackedCount(LegacyEnglishSparsePackedField field)=>packed[Key(field)].Count;
    public int GetPackedWidth(LegacyEnglishSparsePackedField field)=>packed[Key(field)].Width;
    public int GetPackedValue(LegacyEnglishSparsePackedField field,int index)=>PackedValue(packed[Key(field)],index);
    public int[] CopyPackedValues(LegacyEnglishSparsePackedField field,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();var p=packed[Key(field)];LegacyEnglishSparseCopyBudget.Snapshot(checked((long)p.Count*4));int[] result=new int[p.Count];for(int i=0;i<result.Length;i++){if((i&4095)==0)token.ThrowIfCancellationRequested();result[i]=PackedValue(p,i);}token.ThrowIfCancellationRequested();return result;
    }
    public LegacyEnglishSparseField[] CopyFields(){LegacyEnglishSparseCopyBudget.Snapshot(checked((long)fields.Length*256));return fields.ToArray();}
    public byte[] CopyFieldPayload(string name)
    {
        ArgumentNullException.ThrowIfNull(name);if(!slices.TryGetValue(name,out var s))throw new ArgumentException("Known sparse field required.",nameof(name));LegacyEnglishSparseCopyBudget.Snapshot(s.Length);return wire.AsSpan(s.Offset,s.Length).ToArray();
    }
    internal int Int32(string name,int index){var s=slices[name];if((uint)index>=(uint)(s.Length/4))throw new ArgumentOutOfRangeException(nameof(index));return BinaryPrimitives.ReadInt32LittleEndian(wire.AsSpan(s.Offset+index*4));}
    internal float Float(string name,int index)=>BitConverter.Int32BitsToSingle(Int32(name,index));
}

internal static class LegacyEnglishSparseCopyBudget
{
    internal const long Maximum=32L*1024*1024;
    //Conservative logical bytes: input+owned input, two language-sized regions,
    //one full decoded checking/snapshot workspace, twice cost/case copies,
    //fixed field descriptors and hashing scratch. Not managed/native processRSS.
    internal static long Preflight(long limit=Maximum)
    {
        long bytes=checked(2L*3300812+2L*1580304+643871L*4+2L*(1536+144+1728)*4+75L*256+32768);
        if(limit<0||bytes>limit||bytes>Maximum)throw new InvalidDataException("Bounded complete English sparse copy envelope required.");return bytes;
    }
    internal static void Snapshot(long bytes){if(bytes<0||bytes>Maximum)throw new InvalidDataException("Bounded defensive snapshot required.");}
}
