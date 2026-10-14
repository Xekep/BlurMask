using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Owned Engine2B0527E0 UTF8 lookup. Preserves the resource's DJB2-XOR
/// tables and sorted collision spans; there is no normalization, label alias,
/// fallback label or mutable2A symbol insertion. Source219's actual inline
/// alphabet can be extracted by ReadInlineV10dSource.</summary>
public sealed class LegacySourceAlphabetByteLookup
{
    private readonly ushort[] first,second,third,spans,labels;
    private readonly uint[] offsets;
    private readonly byte[] strings;
    public int LabelCount {get;}
    public int BytesConsumed {get;}
    public int SourceNodeOffset {get;}
    public int AlphabetPayloadOffset {get;}
    private LegacySourceAlphabetByteLookup(int count,int consumed,ushort[][] tables,ushort[] spans,uint[] offsets,
        ushort[] labels,byte[] strings,int nodeOffset,int payloadOffset)
    {LabelCount=count;BytesConsumed=consumed;first=tables[0];second=tables[1];third=tables[2];this.spans=spans;
        this.offsets=offsets;this.labels=labels;this.strings=strings;SourceNodeOffset=nodeOffset;AlphabetPayloadOffset=payloadOffset;}

    /// <summary>Read the resource-owned selected247 alphabet that150D00 copies
    /// from selectedSource60 into163970 config0→object8→secondary0. A node
    /// with no inline alphabet is rejected; its external parent is not guessed.</summary>
    public static LegacySourceAlphabetByteLookup ReadInlineV10dSource(ReadOnlySpan<byte> resource,int sourceOffset)
    {
        _=LegacyOrdinarySourceInk.ReadV10dResourceProfile(resource,sourceOffset);
        int end=checked(sourceOffset+8+(int)BinaryPrimitives.ReadUInt32LittleEndian(resource[(sourceOffset+4)..]));
        int at=sourceOffset+80;
        int marker=ReadI32(resource,ref at,end);if(marker==-1)Skip(ref at,4,end);Skip(ref at,4,end);
        byte inline=ReadU8(resource,ref at,end);
        if(inline!=1)throw new NotSupportedException("Selected source has no owned inline alphabet; actual external parent binding required.");
        if(ReadI32(resource,ref at,end)!=0x2b)throw new InvalidDataException("Source inline alphabet is not type2B.");
        ushort version=ReadU16(resource,ref at,end);int payload=at;
        return ReadPayload(resource.Slice(at,end-at),version,sourceOffset,payload);
    }

    public static LegacySourceAlphabetByteLookup ReadPayload(ReadOnlySpan<byte> payload,ushort version)=>ReadPayload(payload,version,-1,-1);
    private static LegacySourceAlphabetByteLookup ReadPayload(ReadOnlySpan<byte> payload,ushort version,int sourceOffset,int payloadOffset)
    {
        //Existing complete parser validates all header/reverse/form metadata;
        //this additional owner retains the native lookup tables it discards.
        var validated=LegacyInkAlphabet.Load(payload,version);int at=16,end=validated.BytesConsumed;
        int entries=ReadU16(payload,ref at,end);var tables=new ushort[3][];
        int[] increments=[32,64,1];
        for(int n=0;n<3;n++)
        {
            tables[n]=new ushort[entries];int last=-1;
            for(int i=0;i<entries;i++){ushort v=ReadU16(payload,ref at,end);tables[n][i]=v;if(v!=65535)last=v;}
            if(last<0)throw new InvalidDataException("Empty native lookup table.");entries=last+increments[n];
        }
        var spans=new ushort[entries+1];for(int i=0;i<spans.Length;i++)spans[i]=ReadU16(payload,ref at,end);
        int records=spans[^1]+1;var offsets=new uint[records];var labels=new ushort[records];
        for(int i=0;i<records;i++){offsets[i]=unchecked((uint)ReadI32(payload,ref at,end));labels[i]=unchecked((ushort)ReadI32(payload,ref at,end));}
        int size=checked((int)offsets[^1]);Skip(ref at,size,end);byte[] strings=payload.Slice(at-size,size).ToArray();
        //052982 masks select exactly16/32/64 entries at successive offsets.
        if(tables[0].Length<16)throw new InvalidDataException("Native hash root requires16 entries.");
        foreach(ushort v in tables[0])if(v!=65535&&v+32>tables[1].Length)throw new InvalidDataException("Second native hash table reference out of bounds.");
        foreach(ushort v in tables[1])if(v!=65535&&v+64>tables[2].Length)throw new InvalidDataException("Third native hash table reference out of bounds.");
        foreach(ushort v in tables[2])if(v!=65535&&v+1>=spans.Length)throw new InvalidDataException("Native collision span out of bounds.");
        return new(validated.LabelCount,validated.BytesConsumed,tables,spans,offsets,labels,strings,sourceOffset,payloadOffset);
    }

    public ushort LookupUtf8(ReadOnlySpan<byte> bytes)
    {
        if(bytes.Length>1_048_576)throw new ArgumentException("UTF8 lookup work bound exceeded.");
        uint hash=0x1505;foreach(byte v in bytes)hash=unchecked(hash*33)^v;
        ushort index=first[(hash>>16)&15];if(index==65535)return 65535;
        index=second[index+((hash>>6)&31)];if(index==65535)return 65535;
        index=third[index+(hash&63)];if(index==65535)return 65535;
        for(int row=spans[index];row<spans[index+1];row++)
        {
            var stored=strings.AsSpan((int)offsets[row],checked((int)(offsets[row+1]-offsets[row])));
            int compare=bytes.SequenceCompareTo(stored);
            if(compare==0)return labels[row];if(compare<0)break;
        }
        return 65535;
    }

    /// <summary>170220's seven literal lookups in source219's exact alphabet.</summary>
    public LegacyFirstResultLineMarkers ResolveLineMarkers()=>new(LookupUtf8([0xf3,0xb0,0x80,0x82]),LookupUtf8([0xf3,0xb0,0x80,0x83]),
        LookupUtf8([0x2d]),LookupUtf8([0x5f]),LookupUtf8([0xe3,0x83,0xbc]),LookupUtf8([0xe4,0xb8,0x80]),LookupUtf8([0xef,0xbf,0xbd]));
    private static void Skip(ref int at,int n,int end){if(n<0||n>end-at)throw new InvalidDataException("Truncated inline lookup payload.");at+=n;}
    private static byte ReadU8(ReadOnlySpan<byte>b,ref int at,int end){int old=at;Skip(ref at,1,end);return b[old];}
    private static ushort ReadU16(ReadOnlySpan<byte>b,ref int at,int end){int old=at;Skip(ref at,2,end);return BinaryPrimitives.ReadUInt16LittleEndian(b[old..]);}
    private static int ReadI32(ReadOnlySpan<byte>b,ref int at,int end){int old=at;Skip(ref at,4,end);return BinaryPrimitives.ReadInt32LittleEndian(b[old..]);}
}
