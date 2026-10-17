using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace InkRecognition.Math;

/// <summary>EF3 classification for the fingerprinted bundled Math2 alphabet.
/// It classifies observable prefix tokens, not grammar/acceptance eligibility.</summary>
internal sealed class LegacyMath2ObservableTokens
{
    private readonly LegacyInkAlphabet alphabet;
    private readonly ushort[] slots;
    private LegacyMath2ObservableTokens(LegacyInkAlphabet alphabet,ushort[] slots)
    {this.alphabet=alphabet;this.slots=slots;}
    internal ushort[] CopySlots()=>slots.ToArray();
    internal static LegacyMath2ObservableTokens BindBundledResource(ReadOnlySpan<byte> resource,
        CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(resource.Length>64*1024*1024||!Convert.ToHexStringLower(SHA256.HashData(resource)).Equals(
            "2dc179aadb1bce347bb1ebd57aa765d4c822d6ea12eca7593a8b9f1b89d53e8a",StringComparison.Ordinal))
            throw new NotSupportedException("The exact supported raw Math2 resource is required; other render/alias profiles are not restored.");
        int end=checked(98+(int)BinaryPrimitives.ReadUInt32LittleEndian(resource[94..]));
        if(BinaryPrimitives.ReadUInt32LittleEndian(resource[90..])!=0x101fe90||
            BinaryPrimitives.ReadUInt32LittleEndian(resource[98..])!=0x2b||
            BinaryPrimitives.ReadUInt16LittleEndian(resource[102..])!=0||end>resource.Length)
            throw new InvalidDataException("Actual unencoded source alphabet envelope required.");
        var alphabet=LegacyInkAlphabet.Load(resource.Slice(106,end-106),BinaryPrimitives.ReadUInt16LittleEndian(resource[104..]));
        if(alphabet.BytesConsumed!=end-106||alphabet.LabelCount!=299)
            throw new InvalidDataException("Complete bundled source alphabet required.");
        var fields=alphabet.CopySerializedHeader();
        for(int i=0;i<7;i++)if(BinaryPrimitives.ReadUInt16LittleEndian(fields.AsSpan(i*2))!=ushort.MaxValue)
            throw new NotSupportedException("Bundled absent-special-field profile required.");
        var slots=Enumerable.Repeat(ushort.MaxValue,8).ToArray();
        //Native AL+12 is FFFF for this fingerprint. No general wire-to-object
        //field mapping or arbitrary alphabet rendering is inferred here.
        ushort Find(string text)=>alphabet.TryGetLabel(text,out ushort value)?value:ushort.MaxValue;
        ushort controlA=Find("\u2402"),controlB=Find("\u2403");
        for(int i=0;i<alphabet.LabelCount;i++)
        {
            token.ThrowIfCancellationRequested();
            if(i==controlA||i==controlB||!alphabet.TryGetText((ushort)i,out string text))continue;
            var runes=text.EnumerateRunes().ToArray();if(runes.Length!=1)continue;
            int scalar=runes[0].Value;
            if(scalar is >=9 and <=13 or 0x20 or 0xa0 or 0x2028 or 0x2029)continue;
            if(slots[0]==ushort.MaxValue)slots[0]=(ushort)i;slots[2]=(ushort)i;
        }
        slots[1]=Find("\u221a");slots[3]=Find("<fracbar>");slots[4]=Find("<slantedfracbar>");
        slots[6]=Find("\u22c5");slots[7]=Find("<vopbar>");
        return new(alphabet,slots);
    }
    internal bool IsObservable(ushort symbol)
    {
        if(symbol==slots[5])return false;
        if(slots[0]<=symbol&&symbol<=slots[2])return true;
        return slots[3]!=ushort.MaxValue&&symbol==slots[3]||slots[4]!=ushort.MaxValue&&symbol==slots[4]||
            slots[6]!=ushort.MaxValue&&symbol==slots[6]||slots[7]!=ushort.MaxValue&&symbol==slots[7];
    }
}
