using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Default05F8E0 eight-byte scalar context records, with exact517-range CCC data.</summary>
public sealed class LegacyNativeCaseContext
{
    private readonly byte[] records;
    private readonly int[] scalars;
    public int Count => records.Length/8;
    public LegacyNativeUnicodeNormalizationData CombiningData {get;}
    private LegacyNativeCaseContext(byte[] records,int[] scalars,LegacyNativeUnicodeNormalizationData data)
    {this.records=records;this.scalars=scalars;CombiningData=data;}
    public byte[] CopyRecord(int index)
    {if((uint)index>=(uint)Count)throw new ArgumentOutOfRangeException(nameof(index));return records.AsSpan(index*8,8).ToArray();}

    public static LegacyNativeCaseContext Prepare(ReadOnlySpan<int> scalars,LegacyNativeUnicodeNormalizationData data,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(data);cancellationToken.ThrowIfCancellationRequested();
        if(scalars.Length is <1 or >4096)throw new ArgumentException("Nonempty bounded UTF32 context required.");
        byte[] bytes=new byte[checked(scalars.Length*8)];
        for(int i=0;i<scalars.Length;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if(scalars[i]<0 || scalars[i]>0x10ffff || scalars[i] is >=0xd800 and <=0xdfff)
                throw new ArgumentOutOfRangeException(nameof(scalars));
            int ccc=data.ReadCombiningClass(scalars[i]);
            BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(i*8),ccc);
            if(ccc==230)
                for(int j=i-1;j>=0 && BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(j*8))!=0;j--)bytes[j*8+4]=1;
            if(scalars[i]==0x307)
                for(int j=i-1;j>=0;j--)
                {int previous=BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(j*8));if(previous is 0 or 230)break;bytes[j*8+5]=1;}
            //05F8E0 initializes flags6/7 from the first scalar only, then
            //clears them on CCC0/230 and never sets them again. U+0049 and
            //all46 native soft-dotted entries have CCC0 in this exact data,
            //so both flags are identically zero for valid scalar inputs.
        }
        return new(bytes,scalars.ToArray(),data);
    }

    /// <summary>04F170 scalar row selection. No variant means append the original scalar in the later string consumer.</summary>
    public LegacyNativeCaseVariant? Select(LegacyNativeCaseData caseData,int index,uint requestFlags,int requestedLanguage)
    {
        ArgumentNullException.ThrowIfNull(caseData);
        if((uint)index>=(uint)Count)throw new ArgumentOutOfRangeException(nameof(index));
        var choices=caseData.ReadAlternatives(scalars[index],64).Written;
        int language=requestedLanguage!=0 && choices.Any(v=>v.LanguageFlags==requestedLanguage)?requestedLanguage:0;
        foreach(var v in choices)
        {
            if(v.LanguageFlags!=language)continue;
            bool selected=v.ContextFlags switch
            {
                0=>true,
                1=>(requestFlags&1)==0,
                2=>(requestFlags&2)!=0,
                4=>records[index*8+7]!=0,
                8=>records[index*8+4]!=0,
                16=>records[index*8+6]!=0,
                32=>records[index*8+5]==0,
                _=>false
            };
            if(selected)return v;
        }
        return null;
    }
}
