using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace InkRecognition;

internal readonly record struct LegacyPreparedQuoteSourceReference(int Node,ushort Arc);
internal sealed record LegacyPreparedQuoteCategorySlice(int SamplerNodeOffset,int CategoryProfileOffset,
    string SamplerFingerprint,LegacyCursiveCategoryProfile Categories);

/// <summary>Prepared source services for21F320. AL/category tables are read
/// together from explicit actual247 resource slices; the source-kind callback and live TOP2A
/// are explicit prepared owners. This does not create an enabled controller.</summary>
internal sealed class LegacyPreparedContextualQuoteSource
{
    private readonly Func<LegacyPreparedQuoteSourceReference,CancellationToken,byte> readKind;
    private readonly LegacyCursiveCategoryMapper categories;
    internal LegacyNativeGlyphRecordPool Pool{get;}
    internal LegacyNativeDynamicControlAlphabet TopControlAlphabet{get;}
    internal LegacyInkAlphabet Alphabet{get;}
    internal LegacyPreparedQuoteCategorySlice CategoryOwner{get;}
    internal LegacyContextualQuoteLabels Labels{get;}
    internal string ResourceFingerprint{get;}
    private LegacyPreparedContextualQuoteSource(LegacyNativeGlyphRecordPool pool,LegacyNativeDynamicControlAlphabet controls,
        LegacyInkAlphabet alphabet,LegacyPreparedQuoteCategorySlice owner,string fingerprint,
        Func<LegacyPreparedQuoteSourceReference,CancellationToken,byte> kind,LegacyContextualQuoteLabels? preparedFields=null)
    {
        Pool=pool;TopControlAlphabet=controls;Alphabet=alphabet;CategoryOwner=owner;ResourceFingerprint=fingerprint;
        readKind=kind;categories=new(owner.Categories);
        ushort Label(string text)=>alphabet.TryGetLabel(text,out ushort label)?label:ushort.MaxValue;
        Labels=preparedFields??new(Label("'"),Label("\""),Label("\u201c"),Label("\u201d"),Label("\u2018"),Label("\u2019"),Label("\u201e"));
    }
    internal static LegacyPreparedContextualQuoteSource Prepare(ReadOnlySpan<byte> actualMainResource,int actualSamplerNodeOffset,
        LegacyNativeGlyphRecordPool pool,LegacyNativeDynamicControlAlphabet actualTopControlAlphabet,
        Func<LegacyPreparedQuoteSourceReference,CancellationToken,byte> sourceKind,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(pool);ArgumentNullException.ThrowIfNull(actualTopControlAlphabet);
        ArgumentNullException.ThrowIfNull(sourceKind);token.ThrowIfCancellationRequested();
        //This is a PREPARED AL/category-slice binding. In particular it does
        //not substitute English247v110/297v108 into the oldv10d ingress reader.
        int rootEnd=NodeEnd(actualMainResource,0,0x247,actualMainResource.Length);
        ushort rootVersion=BinaryPrimitives.ReadUInt16LittleEndian(actualMainResource[14..]);
        if(rootEnd!=actualMainResource.Length||rootVersion is not(0x10d or 0x110)||actualMainResource.Length<115||
            actualMainResource[92]!=1||BinaryPrimitives.ReadUInt32LittleEndian(actualMainResource[93..])!=0x2b)
            throw new NotSupportedException("Prepared inline AL from the actual supported247 resource required.");
        var alphabet=LegacyInkAlphabet.Load(actualMainResource[99..],BinaryPrimitives.ReadUInt16LittleEndian(actualMainResource[97..]));
        int samplerEnd=NodeEnd(actualMainResource,actualSamplerNodeOffset,0x297,rootEnd);
        ushort samplerVersion=BinaryPrimitives.ReadUInt16LittleEndian(actualMainResource[(actualSamplerNodeOffset+14)..]);
        if(samplerVersion is not(0x106 or 0x108)||actualSamplerNodeOffset<99+alphabet.BytesConsumed||
            samplerEnd-actualSamplerNodeOffset<69||actualMainResource[actualSamplerNodeOffset+64]!=1)
            throw new NotSupportedException("Actual >=106 category prefix in a supported297 slice required.");
        int at=actualSamplerNodeOffset+65;
        var categories=LegacyCursiveCategoryProfile.Load(actualMainResource.Slice(at,samplerEnd-at),at,out _,token);
        var category=new LegacyPreparedQuoteCategorySlice(actualSamplerNodeOffset,at,
            Convert.ToHexString(SHA256.HashData(actualMainResource.Slice(actualSamplerNodeOffset,samplerEnd-actualSamplerNodeOffset))),categories);
        if(category.Categories.BaseEntryCount!=alphabet.LabelCount)
            throw new NotSupportedException("The source's exact AL/category symbol domain is required.");
        token.ThrowIfCancellationRequested();return new(pool,actualTopControlAlphabet,alphabet,category,
            Convert.ToHexString(SHA256.HashData(actualMainResource)),sourceKind);
    }
    internal byte ReadKind(ulong reference,CancellationToken token)
    {
        int node=(int)(reference&0xffffff);if((node&0x800000)!=0)node-=0x1000000;
        ushort arc=(ushort)(reference>>24);
        if(unchecked((short)arc)<0)throw new ArgumentException("Negative arc must skip the source callback.");
        token.ThrowIfCancellationRequested();byte value=readKind(new(node,arc),token);token.ThrowIfCancellationRequested();return value;
    }
    internal int ReadPreviousCategory(ushort label,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(label>=Alphabet.LabelCount)throw new ArgumentOutOfRangeException(nameof(label));
        int composite=categories.GetSymbolCompositeCategory(label);
        if(composite<0)throw new NotSupportedException("Native data50 negative index would require unsafe data70 access.");
        return categories.GetFirstId(composite);
    }
    internal ushort LookupControl(string text,CancellationToken token)
        =>TopControlAlphabet.LookupUtf8(Encoding.UTF8.GetBytes(text),token);
    //Isolated prepared controller-field seam: this is NOT an AL lookup or an
    //observed configured owner. Actual resource-bound Labels remain unchanged.
    internal LegacyPreparedContextualQuoteSource WithPreparedLowDoubleField(ushort field24be)
        =>new(Pool,TopControlAlphabet,Alphabet,CategoryOwner,ResourceFingerprint,readKind,Labels with{LowDouble=field24be});
    private static int NodeEnd(ReadOnlySpan<byte> resource,int at,int kind,int end)
    {
        if(resource.Length>64*1024*1024||at<0||at>end-16||end>resource.Length||
            BinaryPrimitives.ReadUInt32LittleEndian(resource[at..])!=0x101fe90||
            BinaryPrimitives.ReadInt32LittleEndian(resource[(at+8)..])!=kind||
            BinaryPrimitives.ReadUInt16LittleEndian(resource[(at+12)..])!=0)
            throw new InvalidDataException("Complete unencoded actual resource envelope required.");
        uint length=BinaryPrimitives.ReadUInt32LittleEndian(resource[(at+4)..]);
        if(length<8||length>end-at-8)throw new InvalidDataException("Truncated actual resource envelope.");
        return checked(at+8+(int)length);
    }
}
