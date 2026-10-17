using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyTextResultIdentifier.Probe")]

namespace InkRecognition;

/// <summary>Library-owned model62 algorithm. Its initial state comes from
/// cross-platform cryptographic bytes, an explicit portability adaptation.
/// Native092CF0's process-address/time initialization is not reproduced.</summary>
public sealed class LegacyLibraryUuidGenerator
{
    private readonly object gate=new();
    private ulong state;
    internal ulong InitialState {get;}
    internal ulong CurrentStateForControls {get{lock(gate)return state;}}
    private LegacyLibraryUuidGenerator(ulong initial){state=InitialState=initial;}
    public static LegacyLibraryUuidGenerator Create()
    {
        Span<byte> bytes=stackalloc byte[8];RandomNumberGenerator.Fill(bytes);
        return new(BinaryPrimitives.ReadUInt64LittleEndian(bytes));
    }
    internal static LegacyLibraryUuidGenerator FromPreparedState(ulong initial)=>new(initial);
    public LegacyLibraryTextResultIdentifier NextIdentifier(CancellationToken token=default)
    {
        lock(gate){token.ThrowIfCancellationRequested();byte[] raw=new byte[16];
            LegacyModel62Uuid.Generate(ref state,raw);return new(this,raw);}
    }
    internal LegacyLibraryTextResultIdentifier NextDistinct(ReadOnlySpan<byte> previous16,CancellationToken token=default)
    {
        if(previous16.Length!=16)throw new ArgumentException("Complete previous result identifier required.");
        lock(gate)
        {
            token.ThrowIfCancellationRequested();byte[] raw=new byte[16];
            do{token.ThrowIfCancellationRequested();LegacyModel62Uuid.Generate(ref state,raw);}
            while(raw.AsSpan().SequenceEqual(previous16));
            return new(this,raw);
        }
    }
}

/// <summary>Immutable result-owned raw16 and native raw-order UUID text.
/// Native byte6 and byte8 contain version/variant bits. No Guid field-byte
/// reversal or recognized labels participate.</summary>
public sealed class LegacyLibraryTextResultIdentifier
{
    private readonly byte[] raw;
    public LegacyLibraryUuidGenerator Owner {get;}
    internal LegacyLibraryTextResultIdentifier(LegacyLibraryUuidGenerator owner,byte[] raw)
    {Owner=owner;this.raw=raw;}
    public byte[] CopyRawStorage16()=>raw.ToArray();
    public byte[] CopyUtf8Storage36()
    {
        Span<byte> buffer=stackalloc byte[37];LegacyModel62Uuid.Format(raw,buffer);return buffer[..36].ToArray();
    }
    public override string ToString()=>System.Text.Encoding.ASCII.GetString(CopyUtf8Storage36());
}

internal static class LegacyModel62Uuid
{
    internal static void Generate(ref ulong state,Span<byte> output16)
    {
        if(output16.Length!=16)throw new ArgumentException("Complete16-byte UUID destination required.");
        BinaryPrimitives.WriteUInt64LittleEndian(output16,Next(ref state));
        output16[6]=(byte)((output16[6]&15)|0x40);
        BinaryPrimitives.WriteUInt64LittleEndian(output16[8..],Next(ref state));
        output16[8]=(byte)((output16[8]&63)|0x80);
    }
    private static ulong Next(ref ulong state)
    {
        unchecked
        {
            state+=0x9e3779b97f4a7c15UL;ulong x=state;
            x=(x^(x>>30))*0xbf58476d1ce4e5b9UL;
            x=(x^(x>>27))*0x94d049bb133111ebUL;
            return x^(x>>31);
        }
    }
    internal static void Format(ReadOnlySpan<byte> raw16,Span<byte> utf8WithNull37)
    {
        if(raw16.Length!=16||utf8WithNull37.Length<37)throw new ArgumentException("Complete raw16 and UTF8 destination37 required.");
        ReadOnlySpan<byte> digits="0123456789abcdef"u8;int at=0;
        for(int index=0;index<16;index++)
        {
            if(index is 4 or 6 or 8 or 10)utf8WithNull37[at++]=(byte)'-';
            utf8WithNull37[at++]=digits[raw16[index]>>4];utf8WithNull37[at++]=digits[raw16[index]&15];
        }
        utf8WithNull37[at]=0;
    }
}
