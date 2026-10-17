using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Actual249-selected first247/110 acoustic119 no-initial/no-dynamic
/// immutable wire owner. Does not execute24C, extract features, score or decode.</summary>
public sealed class LegacyFirstAcoustic119WireModel
{
    public LegacySource247WireLayout ParentSource {get;}
    public LegacySource247WireChild SelectedProcessor {get;}
    public LegacySource247WireLayout Source {get;}
    public LegacySource247WireChild AcousticChild {get;}
    public IReadOnlyList<LegacyResourceWireField> Fields {get;}
    public IReadOnlyList<LegacyAcoustic119Slot> Slots {get;}
    public float SourceScoreExponent {get;}
    /// <summary>Serialized signed0A; no runtime zero-to100 substitution.</summary>
    public short RawNormalizationTarget {get;}
    public float CostMultiplier {get;}
    public int PrefixEnd {get;}
    public string Fingerprint {get;}
    private LegacyFirstAcoustic119WireModel(LegacySource247WireLayout parent,LegacySource247WireChild selected,
        LegacySource247WireLayout source,LegacySource247WireChild acoustic,List<LegacyResourceWireField> fields,
        LegacyAcoustic119Slot[] slots,float exponent,short target,float multiplier,int prefixEnd,string fingerprint)
    {ParentSource=parent;SelectedProcessor=selected;Source=source;AcousticChild=acoustic;Fields=fields.AsReadOnly();
        Slots=Array.AsReadOnly(slots);SourceScoreExponent=exponent;RawNormalizationTarget=target;CostMultiplier=multiplier;PrefixEnd=prefixEnd;Fingerprint=fingerprint;}

    public static LegacyFirstAcoustic119WireModel ReadSelectedFirst(ReadOnlySpan<byte> resource,
        LegacySource247WireLayout actualParent,LegacySource247WireChild actual249,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualParent);actualParent.ValidateChild(actual249,token);
        if(actual249.Kind!=0x249||actual249.Version!=0x100||!actual249.Destination.StartsWith("parametersC0.58[",StringComparison.Ordinal))
            throw new NotSupportedException("Actual selected249/100 processor required.");
        if(resource.Length>64*1024*1024||actualParent.End>resource.Length||
            !Convert.ToHexString(SHA256.HashData(resource[actualParent.Offset..actualParent.End])).Equals(actualParent.Fingerprint,StringComparison.Ordinal))
            throw new ArgumentException("Resource does not match the retained parent owner.");
        //Keep absolute wire positions:107 matrix alignment is not relative to a
        //copied FEA0 envelope. Each returned child retains this selected source.
        var source=LegacySource247WireLayout.Read(resource,checked(actual249.Offset+16),token);
        if(source.End!=actual249.End-4||source.Version!=0x110||source.SourceKind!=0x135)
            throw new NotSupportedException("Exact ordinary first247/110 plus trailing exponent required.");
        float exponent=BinaryPrimitives.ReadSingleLittleEndian(resource[source.End..]);
        if(!float.IsFinite(exponent)||exponent<0)throw new InvalidDataException("Invalid first-source score exponent.");
        var child=source.Children.SingleOrDefault(c=>c.Destination=="247.140")??throw new NotSupportedException("The selected first source has no actual acoustic140 child.");
        if(child.Version!=0x119)throw new NotSupportedException("This separate reader requires acoustic119.");
        byte[] bytes=source.CopyChildEnvelopeBytes(child,token);var r=new LegacyResourceWireReader(bytes,child.Offset,token);
        r.Take("21D","envelope",1,16);r.U8("21D","08");r.U8("21D","09");
        short target=unchecked((short)r.U16("21D","0A"));if(target<0)throw new InvalidDataException("Negative normalization target.");
        float multiplier=Float(r,"21D","0C");
        if(multiplier<0)throw new InvalidDataException("Negative acoustic cost multiplier.");
        if(r.Flag("21D","18.initialPresent"))throw new NotSupportedException("Initial-network119 branch remains unported.");
        if(r.Flag("21D","10.dynamicPresent"))throw new NotSupportedException("Dynamic119 branch remains unported.");
        if(r.U8("21D","68.profileCount")!=1||r.Flag("profile0","00.gridPresent"))
            throw new NotSupportedException("Only one non-grid119 profile is supported.");
        r.U16("profile0","14");r.U32("profile0","18");
        foreach(string f in new[]{"1C","1E","20","22","28"})r.U16("profile0",f);
        foreach(string f in new[]{"2C","30","34","38","3C","40","44"})Float(r,"profile0",f);
        ShortLists(r,"48","50",1);ShortLists(r,"58","60/68",2);ShortLists(r,"70","78/80",2);
        r.U8("profile0","88");r.U8("profile0","89");r.U16("profile0","90");
        Float(r,"profile0","94");Float(r,"profile0","98");r.U16("profile0","9C");r.U16("profile0","9E");
        if(r.Flag("profile0","optionalA0")||r.Flag("profile0","optionalPair"))
            throw new NotSupportedException("Positive optional profile payloads remain unported.");
        var slots=new LegacyAcoustic119Slot[7];
        for(int i=0;i<slots.Length;i++)
        {
            string name=$"slot{i}";token.ThrowIfCancellationRequested();
            if(r.Flag(name,"conversionPresent"))throw new NotSupportedException("Slot conversion payload remains unported.");
            bool first=r.Flag(name,"first"),second=r.Flag(name,"second");byte mode=r.U8(name,"mode");
            if(mode>2)throw new InvalidDataException("Invalid slot mode.");
            float? weight=mode==0?Float(r,name,"weight"):null;var networks=new List<LegacyInkNetwork107Wire>();
            for(int n=0;n<3;n++)if(r.Flag(name,$"network{n}.present"))
            {
                if(n!=1)throw new NotSupportedException("Only second-network EXP slots are supported.");
                int p=r.Position;
                if(bytes.Length-p<8)throw new InvalidDataException("Truncated network envelope.");
                uint size=BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(p+4));
                if(size>16*1024*1024||size>bytes.Length-p-8)throw new InvalidDataException("Invalid bounded network envelope.");
                var network=LegacyInkNetwork107Wire.Read(source,bytes.AsSpan(p,checked((int)size+8)),r.Absolute,token);
                if(network.Kind!=0x1f+i*16||network.OutputWidth>source.Alphabet.LabelCount)
                    throw new InvalidDataException("Network family or output domain differs from its actual source.");
                //Take retains raw bytes as a named field for independent audit;
                //network owns its separate immutable matrix snapshot.
                r.Take(name,$"network{n}.107",1,checked((int)size+8));networks.Add(network);
            }
            if(networks.Count!=0?first||!second||mode!=2: first||second||mode!=0||weight!=0)
                throw new NotSupportedException("Only actual second-only mode2 or inactive zero-weight slots are supported.");
            slots[i]=new(i,first,second,mode,weight,networks.AsReadOnly());
        }
        int prefixEnd=r.Absolute;
        r.Flag("21D","78");Float(r,"21D","7C");r.Flag("21D","80");Float(r,"21D","88");Float(r,"21D","84");
        int count=r.U16("21D","90");if(count>512)throw new InvalidDataException("Too many acoustic lists.");
        for(int i=0;i<count;i++)ShortLists(r,$"98[{i}].count",$"98[{i}]",1,"21D");
        foreach(string f in new[]{"A4","A8","AC","B0","B4"})Float(r,"21D",f);
        r.Flag("21D","B8");Float(r,"21D","BC");Float(r,"21D","C0");
        int groups=unchecked((short)r.U16("21D","C8"));if(groups is <0 or >512)throw new InvalidDataException("Invalid positive-group count.");
        if(groups!=0)
        {
            byte[] boundaries=r.Take("21D","D0",2,groups+1);int last=0;
            for(int i=0;i<=groups;i++){int v=BinaryPrimitives.ReadUInt16LittleEndian(boundaries.AsSpan(i*2));if(v<last||i==0&&v!=0)throw new InvalidDataException("Invalid positive-group boundaries.");last=v;}
            byte[] labels=r.Take("21D","D8",2,last);
            for(int i=0;i<last;i++)if(BinaryPrimitives.ReadUInt16LittleEndian(labels.AsSpan(i*2))>=source.Alphabet.LabelCount)throw new InvalidDataException("Positive-group label outside actual AL.");
            for(int i=0;i<groups;i++)Float(r,"21D",$"E0[{i}]");
        }
        r.U32("21D","E8");r.U16("21D","EC");r.U16("21D","EE");
        foreach(string f in new[]{"F0","F4","F8"})r.U32("21D",f);r.Flag("21D","FC");Float(r,"21D","100");
        if(r.U32("21D","108")!=0)throw new NotSupportedException("Positive calibration119 payload remains unported.");
        if(r.U32("21D","128")!=0)throw new NotSupportedException("Positive correction119 payload remains unported.");
        if(r.Flag("21D","148/150.present"))throw new NotSupportedException("Optional119 model children remain unported.");
        Float(r,"21D","120");Float(r,"21D","124");r.RequireEnd();
        return new(actualParent,actual249,source,child,r.Fields,slots,exponent,target,multiplier,prefixEnd,Convert.ToHexString(SHA256.HashData(bytes)));
    }
    private static float Float(LegacyResourceWireReader r,string owner,string field)
    {float f=BitConverter.Int32BitsToSingle(unchecked((int)r.U32(owner,field)));if(!float.IsFinite(f))throw new InvalidDataException("Nonfinite acoustic wire scalar.");return f;}
    private static void ShortLists(LegacyResourceWireReader r,string countField,string dataField,int copies,string owner="profile0")
    {int n=unchecked((short)r.U16(owner,countField));if(n is <0 or >1024)throw new InvalidDataException("Portable short-list bound exceeded.");r.Take(owner,dataField,2,checked(n*copies));}
}

public sealed record LegacyAcoustic119Slot(int Index,bool First,bool Second,byte Mode,float? Weight,IReadOnlyList<LegacyInkNetwork107Wire> Networks);
