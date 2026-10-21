using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
[assembly:InternalsVisibleTo("LegacyEnglishLkPacked102.Probe")]
namespace InkRecognition;

public readonly record struct LegacyEnglishLkResourceNode(int Offset,int End,uint Kind,ushort Version);
/// <summary>Actual typed English lexical DATA. No language model, masks,
///controller selection/epoch or handwriting winner is inferred.</summary>
public sealed class LegacyEnglishLkPackedData
{
    private readonly LegacyEnglishLkResourceNode[] children;
    public LegacySource247WireLayout Source{get;}
    public LegacyInkAlphabet AcousticAlphabet=>Source.Alphabet;
    public LegacyInkAlphabet LexicalAlphabet{get;}
    public int LexicalAlphabetAbsoluteOffset=>294;
    public LegacyPackedLexicon Dictionary{get;}
    public LegacyDictionaryResourcePrefix Prefix{get;}
    public LegacyEnglishLkResourceNode UnsupportedLanguageBoundary=>new(1252412,2832716,0x2d2,0x101);
    public string Fingerprint=>"9f4ef50bca4a1e3cc42189c279b72bf8fdf03f62e638673079d57075e168f7c9";
    public long RetainedDictionaryStorageBytes=>Dictionary.StorageBytes;
    public LegacyEnglishLkResourceNode[] CopySerializedRootChildren()=>children.ToArray();
    private LegacyEnglishLkPackedData(LegacySource247WireLayout source,LegacyInkAlphabet alphabet,
        LegacyPackedLexicon dictionary,LegacyDictionaryResourcePrefix prefix,LegacyEnglishLkResourceNode[] children)
    {Source=source;LexicalAlphabet=alphabet;Dictionary=dictionary;Prefix=prefix;this.children=children;}
    public static LegacyEnglishLkPackedData Read(LegacySource247WireLayout actualEnglishAcoustic,
        ReadOnlySpan<byte> actualLexicalResource,CancellationToken token=default)
        =>ReadCore(actualEnglishAcoustic,actualLexicalResource,null,token);
    internal static LegacyEnglishLkPackedData ReadPrepared(LegacySource247WireLayout source,
        ReadOnlySpan<byte> raw,Action<string> checkpoint,CancellationToken token=default)
        =>ReadCore(source,raw,checkpoint,token);
    private static LegacyEnglishLkPackedData ReadCore(LegacySource247WireLayout source,ReadOnlySpan<byte> input,
        Action<string>? checkpoint,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(source);token.ThrowIfCancellationRequested();LegacyEnglishLkPackedCopyBudget.Preflight();
        if(source.Offset!=0||source.Version!=0x110||source.End!=545871||source.SourceKind!=0x135||source.Alphabet.LabelCount!=217||
            !source.Fingerprint.Equals("ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657",StringComparison.OrdinalIgnoreCase)||input.Length!=3300812)
            throw new InvalidDataException("Actual English acoustic/lexical data owners required.");
        void Check(string s){checkpoint?.Invoke(s);token.ThrowIfCancellationRequested();}
        Check("sha:input");CheckHash(input,token,checkpoint,"sha:input:chunk");Check("copy:resource");byte[] snapshot=input.ToArray();
        Check("sha:snapshot");CheckHash(snapshot,token,checkpoint,"sha:snapshot:chunk");ReadOnlySpan<byte> data=snapshot;
        void Node(int at,int end,uint kind,ushort version)
        {
            if(BinaryPrimitives.ReadUInt32LittleEndian(snapshot.AsSpan(at))!=0x101fe90||BinaryPrimitives.ReadUInt32LittleEndian(snapshot.AsSpan(at+4))!=end-at-8||
                BinaryPrimitives.ReadUInt32LittleEndian(snapshot.AsSpan(at+8))!=kind||BinaryPrimitives.ReadUInt16LittleEndian(snapshot.AsSpan(at+12))!=0||BinaryPrimitives.ReadUInt16LittleEndian(snapshot.AsSpan(at+14))!=version)
                throw new InvalidDataException("Actual English LK node envelope required.");
        }
        Node(0,3300812,0x13f,0x102);Node(184,2832716,0x13b,0x300);Node(1252412,2832716,0x2d2,0x101);
        if(BinaryPrimitives.ReadUInt32LittleEndian(data[88..])!=21)throw new InvalidDataException("Whole actual serialized LK inventory required.");
        var inventory=new LegacyEnglishLkResourceNode[21];int at=92;
        for(int i=0;i<21;i++)
        {
            token.ThrowIfCancellationRequested();if(at>data.Length-16)throw new InvalidDataException("Bounded root child envelope required.");
            uint size=BinaryPrimitives.ReadUInt32LittleEndian(data[(at+4)..]);if(size<8||size>data.Length-at-8)throw new InvalidDataException("Whole serialized child required.");
            int end=checked(at+(int)size+8);uint kind=BinaryPrimitives.ReadUInt32LittleEndian(data[(at+8)..]);ushort version=BinaryPrimitives.ReadUInt16LittleEndian(data[(at+14)..]);
            if(kind!=(i==0?0x13du:i==1?0x13bu:0x13au))throw new InvalidDataException("Actual raw child order required.");Node(at,end,kind,version);inventory[i]=new(at,end,kind,version);at=i<20?checked((end+3)&~3):end;
        }
        if(at!=data.Length||BinaryPrimitives.ReadUInt16LittleEndian(data[200..])!=0x301||BinaryPrimitives.ReadUInt16LittleEndian(data[202..])!=0x100||
            BinaryPrimitives.ReadUInt32LittleEndian(data[276..])!=0||BinaryPrimitives.ReadUInt32LittleEndian(data[280..])!=1||BinaryPrimitives.ReadUInt32LittleEndian(data[284..])!=0x3f800000||
            BinaryPrimitives.ReadUInt32LittleEndian(data[288..])!=0x2b||BinaryPrimitives.ReadUInt16LittleEndian(data[292..])!=0x104||BinaryPrimitives.ReadUInt32LittleEndian(data[2568..])!=1)
                throw new InvalidDataException("Actual main13B inherited prefix/AL104 fields required.");
        byte[] name=data.Slice(208,56).ToArray();ReadOnlySpan<byte> xor=[0x14,0x12,0x19,0x77];for(int i=0;i<name.Length;i++)name[i]^=xor[i&3];
        int nul=name.AsSpan().IndexOf((byte)0);if(nul<0||name.AsSpan(nul).ContainsAnyExcept((byte)0)||Encoding.UTF8.GetString(name,0,nul)!="en_US-lk-text.lite.lex")throw new InvalidDataException("Actual English main identifier required.");
        Check("alphabet:read");var alphabet=LegacyInkAlphabet.Load(data.Slice(294,2274),0x104);
        if(alphabet.LabelCount!=104||alphabet.BytesConsumed!=2274)throw new InvalidDataException("Whole lexical104 payload required.");
        Check("dictionary:read");var dictionary=LegacyPackedLexicon.ReadEnglish102(data,token,checkpoint);Check("publish");
        var prefix=new LegacyDictionaryResourcePrefix("en_US-lk-text.lite.lex",0,65536,1,1,110);
        return new(source,alphabet,dictionary,prefix,inventory);
    }
    private static void CheckHash(ReadOnlySpan<byte> bytes,CancellationToken token,Action<string>? checkpoint,string stage)
    {
        using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        for(int at=0;at<bytes.Length;at+=32768){checkpoint?.Invoke(stage);token.ThrowIfCancellationRequested();hash.AppendData(bytes.Slice(at,Math.Min(32768,bytes.Length-at)));}
        token.ThrowIfCancellationRequested();if(!Convert.ToHexString(hash.GetHashAndReset()).Equals("9F4EF50BCA4A1E3CC42189C279B72BF8FDF03F62E638673079D57075E168F7C9",StringComparison.Ordinal))throw new InvalidDataException("Exact English LK resource fingerprint required.");
    }
}
