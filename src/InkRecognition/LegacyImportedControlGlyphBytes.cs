using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("LegacyImportedWordBytes.Probe")]

namespace InkRecognition;

/// <summary>One imported glyph's raw13A0B0 byte interval BEFORE0491A0.
/// No canonical/circled processing, Unicode repair or accepted text is implied.</summary>
public sealed class LegacyImportedControlGlyphBytes
{
    private readonly byte[] record,bytes;
    public ulong Reference {get;}
    public LegacyNativeGlyphSymbolDescriptor Descriptor {get;}
    public int TokenCount {get;}
    public bool IsValidUtf8 {get;}
    public bool ContainsNativeUnitLiteral {get;}
    private LegacyImportedControlGlyphBytes(ulong reference,byte[] record,byte[] bytes,int count,bool unit)
    {
        Reference=reference;this.record=record;this.bytes=bytes;TokenCount=count;ContainsNativeUnitLiteral=unit;
        Descriptor=new(System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(record.AsSpan(4)),
            System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(record.AsSpan(8)));
        IsValidUtf8=ValidUtf8(bytes);
        if(!IsValidUtf8&&!unit)throw new NotSupportedException("Unrestored malformed resource/codec byte stage.");
    }
    public byte[] CopyRecord40()=>record.ToArray();
    public byte[] CopyBytes()=>bytes.ToArray();

    public static LegacyImportedControlGlyphBytes Read(LegacyFreshMainGlobalControlImport owner,ulong reference,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(owner);owner.ValidateCompletedProjection(token);
        var result=ReadCore(owner.ImportedPool,reference,token);owner.ValidateCompletedProjection(token);return result;
    }
    internal static LegacyImportedControlGlyphBytes ReadCore(LegacyNativeGlyphControlImport pool,ulong reference,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(!pool.Succeeded||!pool.HasFinished||pool.IsStopped)
            throw new InvalidOperationException("Only successful owned imported pool records can be projected.");
        var glyph=pool.Resolve(reference);var descriptor=glyph.Symbol;
        //The imported lifecycle owns a NON-NULL empty alternate vector. Native
        //13A0B0 fails its count<2 check; do not use its null-table primary fallback.
        if((descriptor.KindFlags&7)==4)throw new NotSupportedException("Actual imported alternate table is empty.");
        var tokens=LegacyNativeGlyphUtf8Tokens.Expand(descriptor,
            value=>pool.LookupControlBytes(Encoding.UTF8.GetBytes(value),token),token);
        using var output=new MemoryStream();bool unit=false;
        foreach(var item in tokens)
        {
            token.ThrowIfCancellationRequested();
            if(item.UseAlternateAlphabet)throw new NotSupportedException("Actual nonempty alternate owner is required.");
            ReadOnlySpan<byte> value;
            byte[]? owned=null;
            if(item.Label<pool.ControlSymbolCount)
            {
                owned=pool.ReadControlSymbolBytes(item.Label,token);value=owned;
                //The private2A accepts validUTF8 or precisely139DC0's nativeC2
                //unit insertion. No arbitrary malformed bytes can be supplied.
                if(value.Length==1&&value[0]==0xc2)unit=true;
                else if(!ValidUtf8(value))throw new NotSupportedException("Unrestored malformed resource/codec byte stage.");
            }
            else value=Reserved(item.Label);
            if(value.Length>1_048_576-output.Length)throw new NotSupportedException("Portable glyph byte budget exceeded.");
            output.Write(value);
        }
        token.ThrowIfCancellationRequested();return new(reference,glyph.ReadRecord(),output.ToArray(),tokens.Length,unit);
    }
    internal static bool ValidUtf8(ReadOnlySpan<byte> bytes)
    {
        while(!bytes.IsEmpty){if(Rune.DecodeFromUtf8(bytes,out _,out int consumed)!=OperationStatus.Done)return false;bytes=bytes[consumed..];}
        return true;
    }
    private static ReadOnlySpan<byte> Reserved(ushort label)=>label switch
    {
        0xfffc=>"\u00a0"u8,0xfffb=>" "u8,0xfff9=>"\n"u8,0xfff6=>"\U000f0000"u8,
        0xfff3=>"\U000f0010"u8,0xfff2=>"\U000f0011"u8,0xfff1=>"\U000f0012"u8,
        0xfff0=>"\U000f0013"u8,0xffef=>"\U000f0014"u8,0xffee=>"\U000f0015"u8,
        0xffed=>"\U000f0016"u8,0xffec=>"\U000f0017"u8,0xffeb=>"\U000f0018"u8,_=>"\ufffd"u8
    };
}
