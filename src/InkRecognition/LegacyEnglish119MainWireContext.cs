using System.Buffers.Binary;
namespace InkRecognition;

/// <summary>Actual EN MAIN247.130/138/140 immutable wire context. Positive
///dynamic/80/B8/100 bytes are retained, NOT executed or relabeled as FIRST119.</summary>
public sealed class LegacyEnglish119MainWireContext
{
    public LegacyEnglish119ParentPostprocessing Owner{get;}
    public LegacySource247WireLayout Source{get;}
    public LegacySampler297WireProfile Sampler{get;}
    public LegacySource247WireChild Graph{get;}
    public LegacySource247WireChild Acoustic{get;}
    public IReadOnlyList<LegacyResourceWireField> Fields{get;}
    public IReadOnlyList<LegacyInkNetwork107Wire> Networks{get;}
    public int CacheWidth=>Source.Alphabet.LabelCount+3;
    public int PrefixEnd{get;}
    private LegacyEnglish119MainWireContext(LegacyEnglish119ParentPostprocessing owner,LegacySource247WireLayout source,
        LegacySampler297WireProfile sampler,LegacySource247WireChild graph,LegacySource247WireChild acoustic,
        List<LegacyResourceWireField> fields,LegacyInkNetwork107Wire[] networks,int end)
    {Owner=owner;Source=source;Sampler=sampler;Graph=graph;Acoustic=acoustic;Fields=fields.AsReadOnly();Networks=Array.AsReadOnly(networks);PrefixEnd=end;}
    internal static LegacyEnglish119MainWireContext Read(LegacyEnglish119ParentPostprocessing owner,LegacySource247WireLayout source,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(!ReferenceEquals(owner.Parent.Model.ParentSource,source))throw new ArgumentException("Actual completed parent source required.");
        var sampler=source.Children.Single(c=>c.Destination=="247.130");var graph=source.Children.Single(c=>c.Destination=="247.138");var acoustic=source.Children.Single(c=>c.Destination=="247.140");
        if(sampler.Offset!=182412||sampler.Version!=0x108||graph.Offset!=183800||graph.Kind!=0x21b||graph.Version!=0x10a||acoustic.Offset!=184155||acoustic.Kind!=0x21d||acoustic.Version!=0x119||acoustic.End!=545680)
            throw new InvalidDataException("Actual EN MAIN108/10A/119 children required.");
        var profile=LegacySampler297WireProfile.Read(source,sampler,source.CopyChildEnvelopeBytes(sampler,token),token);
        byte[] bytes=source.CopyChildEnvelopeBytes(acoustic,token);var r=new LegacyResourceWireReader(bytes,acoustic.Offset,token);
        r.Take("21D","envelope",1,16);if(r.U8("21D","08")!=255||r.U8("21D","09")!=255||r.U16("21D","0A")!=100||Float(r,"21D","0C")!=1)
            throw new InvalidDataException("Actual MAIN FF/FF100 required.");
        if(r.Flag("21D","18.initialPresent")||!r.Flag("21D","10.dynamicPresent"))throw new InvalidDataException("Actual MAIN noinitial/dynamic branch required.");
        if(r.U16("257","08.sampleCount")!=30)throw new InvalidDataException("Actual dynamic sample count required.");
        foreach(string name in new[]{"0C","10","14","18","1C","20","24","28","21D.1C4"})Float(r,"257",name);
        if(r.U8("21D","68.profileCount")!=1||r.Flag("profile0","00.gridPresent"))throw new InvalidDataException("Actual one nongrid profile required.");
        r.Take("profile0","14..44.raw",1,44);
        Lists(r,"48",1);Lists(r,"58",2);Lists(r,"70",2);r.Take("profile0","88..9E.raw",1,16);
        if(r.Flag("profile0","optionalA0")||r.Flag("profile0","optionalPair"))throw new InvalidDataException("Actual absent optional profile children required.");
        var networks=new List<LegacyInkNetwork107Wire>();int[] widths=[228,210,285,285];
        for(int slot=0;slot<7;slot++)
        {
            token.ThrowIfCancellationRequested();string scope="slot"+slot;
            if(r.Flag(scope,"conversionPresent"))throw new InvalidDataException("Actual absent conversions required.");
            bool first=r.Flag(scope,"first"),second=r.Flag(scope,"second");byte mode=r.U8(scope,"mode");
            if(first||second!=(slot<4)||mode!=(slot<4?2:0))throw new InvalidDataException("Actual MAIN second-only four-family slots required.");
            if(mode==0&&Float(r,scope,"weight")!=0)throw new InvalidDataException("Actual inactive zero weight required.");
            for(int n=0;n<3;n++)
            {
                bool present=r.Flag(scope,$"network{n}.present");if(present!=(slot<4&&n==1))throw new InvalidDataException("Actual MAIN107 slots required.");
                if(!present)continue;
                int p=r.Position;if(bytes.Length-p<8)throw new InvalidDataException("Truncated MAIN107.");uint size=BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(p+4));
                if(size>16*1024*1024||size>bytes.Length-p-8)throw new InvalidDataException("Bounded MAIN107 required.");
                var nn=LegacyInkNetwork107Wire.Read(source,bytes.AsSpan(p,checked((int)size+8)),r.Absolute,token);
                if(nn.InputWidth!=widths[slot]||nn.OutputWidth!=182||nn.Kind!=0x1f+slot*16)throw new InvalidDataException("Actual MAIN107 dimensions required.");
                networks.Add(nn);r.Take(scope,"network1.107",1,checked((int)size+8));
            }
        }
        int prefixEnd=r.Absolute;
        r.Flag("21D","78");Float(r,"21D","7C");r.Flag("21D","80");Float(r,"21D","88");Float(r,"21D","84");
        int lists=r.U16("21D","90");if(lists>512)throw new InvalidDataException("Bounded cache lists required.");for(int i=0;i<lists;i++)Lists(r,"98["+i+"]",1,"21D");
        foreach(string name in new[]{"A4","A8","AC","B0","B4"})Float(r,"21D",name);
        r.Flag("21D","B8");Float(r,"21D","BC");Float(r,"21D","C0");int groups=unchecked((short)r.U16("21D","C8"));
        if(groups is<0 or>512)throw new InvalidDataException("Bounded MAIN positive groups required.");
        if(groups!=0)
        {
            byte[] ends=r.Take("21D","D0",2,groups+1);int last=0;
            for(int i=0;i<=groups;i++){int n=BinaryPrimitives.ReadUInt16LittleEndian(ends.AsSpan(i*2));if(n<last||i==0&&n!=0)throw new InvalidDataException("Ordered MAIN group bounds required.");last=n;}
            byte[] labels=r.Take("21D","D8",2,last);for(int i=0;i<last;i++)if(BinaryPrimitives.ReadUInt16LittleEndian(labels.AsSpan(i*2))>=source.Alphabet.LabelCount)throw new InvalidDataException("Actual MAIN AL group labels required.");
            for(int i=0;i<groups;i++)Float(r,"21D",$"E0[{i}]");
        }
        r.U32("21D","E8");r.U16("21D","EC");r.U16("21D","EE");foreach(string n in new[]{"F0","F4","F8"})r.U32("21D",n);
        r.Flag("21D","FC");Float(r,"21D","100");if(r.U32("21D","108")!=0||r.U32("21D","128")!=0||r.Flag("21D","148/150.present"))throw new InvalidDataException("Actual absent calibration/correction/optional MAIN children required.");
        Float(r,"21D","120");Float(r,"21D","124");r.RequireEnd();token.ThrowIfCancellationRequested();
        return new(owner,source,profile,graph,acoustic,r.Fields,networks.ToArray(),prefixEnd);
    }
    private static float Float(LegacyResourceWireReader r,string scope,string name)
    {float x=BitConverter.Int32BitsToSingle(unchecked((int)r.U32(scope,name)));if(!float.IsFinite(x))throw new InvalidDataException("Finite actual wire value required.");return x;}
    private static void Lists(LegacyResourceWireReader r,string name,int copies,string scope="profile0")
    {int n=unchecked((short)r.U16(scope,name+".count"));if(n is<0 or>1024)throw new InvalidDataException("Bounded wire list required.");r.Take(scope,name+".values",2,n*copies);}
}
