using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglishMainMasks.Probe")]
namespace InkRecognition;

/// <summary>Actual MAIN21D/119 groups and parameters read from the SAME completed
///MainWire. Positive80/B8/100 are retained here, not executed as FIRST scores.</summary>
public sealed class LegacyEnglish119MainKnowledge
{
    private readonly ushort[][] groups;
    private readonly float[] coefficients;
    public LegacyEnglish119MainLocalCandidates Owner{get;}
    public LegacyEnglish119MainWireContext MainWire=>Owner.Owner.MainWire;
    public LegacySource247WireLayout Source=>MainWire.Source;
    public int GroupCount=>groups.Length;
    public int GroupSymbolCount{get;}
    public bool RelatedCaseRelaxationEnabled{get;}
    public float FirstRelatedCaseMultiplier{get;}
    public float SecondRelatedCaseMultiplier{get;}
    public bool SizeCostsEnabled{get;}
    public float DotSizeDivisor{get;}
    public float PunctuationSizeDivisor{get;}
    public float CachePower{get;}
    public long LogicalCopyWorkBytes{get;}
    public LegacySymbolMaskKnowledgeGroup[] CopyGroups()=>groups.Select((g,i)=>new LegacySymbolMaskKnowledgeGroup(g.ToArray(),coefficients[i])).ToArray();
    private LegacyEnglish119MainKnowledge(LegacyEnglish119MainLocalCandidates owner,ushort[][] groups,float[] weights,
        bool related,float first,float second,bool size,float dot,float punctuation,float power,long budget)
    {Owner=owner;this.groups=groups;coefficients=weights;GroupSymbolCount=groups.Sum(g=>g.Length);RelatedCaseRelaxationEnabled=related;FirstRelatedCaseMultiplier=first;SecondRelatedCaseMultiplier=second;SizeCostsEnabled=size;DotSizeDivisor=dot;PunctuationSizeDivisor=punctuation;CachePower=power;LogicalCopyWorkBytes=budget;}
    public static LegacyEnglish119MainKnowledge Create(LegacyEnglish119MainLocalCandidates actualCompleted,CancellationToken token=default)=>CreateCore(actualCompleted,null,token);
    internal static LegacyEnglish119MainKnowledge CreatePrepared(LegacyEnglish119MainLocalCandidates owner,Action<string> checkpoint,CancellationToken token=default)=>CreateCore(owner,checkpoint,token);
    private static LegacyEnglish119MainKnowledge CreateCore(LegacyEnglish119MainLocalCandidates owner,Action<string>? checkpoint,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(owner);token.ThrowIfCancellationRequested();
        if(!owner.IsComplete||owner.IsFaulted||!owner.Owner.IsComplete||owner.Owner.IsFaulted)throw new InvalidOperationException("Completed actual EN single/local owner required.");
        var wire=owner.Owner.MainWire;var source=wire.Source;
        if(source.SourceKind!=0x135||source.Alphabet.LabelCount!=217||!source.Fingerprint.Equals("ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657",StringComparison.OrdinalIgnoreCase)||wire.Acoustic.Offset!=184155||wire.Acoustic.End!=545680||wire.Acoustic.Version!=0x119||wire.Acoustic.Kind!=0x21d||wire.Networks.Count!=4)
            throw new InvalidDataException("Actual full EN MAIN119 source required.");
        void Check(string stage){checkpoint?.Invoke(stage);token.ThrowIfCancellationRequested();}
        //Only metadata is inspected here. Never CopyBytes on107 capsule fields.
        long budget=Preflight(wire.Fields.Count,7,25,wire.Networks.Count,104,217);int at=wire.Acoustic.Offset;
        foreach(var field in wire.Fields)
        {
            token.ThrowIfCancellationRequested();long bytes=checked((long)field.ElementSize*field.Count);
            if(field.ElementSize<1||field.Count<0||field.Offset!=at||bytes>wire.Acoustic.End-at)throw new InvalidDataException("Whole immutable119 field partition required.");at=checked(at+(int)bytes);
        }
        if(at!=wire.Acoustic.End)throw new InvalidDataException("Full119 end required.");
        int[] widths=[228,210,285,285];for(int i=0;i<4;i++){token.ThrowIfCancellationRequested();var n=wire.Networks[i];if(n.InputWidth!=widths[i]||n.OutputWidth!=182||n.Kind!=0x1f+i*16)throw new InvalidDataException("Actual four MAIN107 resources required.");}
        LegacyResourceWireField Field(string name,int size,int count)
        {var f=wire.Fields.Single(f=>f.Owner=="21D"&&f.Destination==name);if(f.ElementSize!=size||f.Count!=count)throw new InvalidDataException("Bounded typed119 field required.");return f;}
        //Preflight ALL small payload lengths before their first snapshot copy.
        _=Field("C8",2,1);_=Field("D0",2,8);_=Field("D8",2,25);
        foreach(string name in new[]{"80","B8"})_=Field(name,1,1);
        foreach(string name in new[]{"84","88","BC","C0","100"})_=Field(name,4,1);
        for(int i=0;i<7;i++)_=Field($"E0[{i}]",4,1);
        Check("knowledge:preflight");
        byte[] Read(string name,int size,int count){token.ThrowIfCancellationRequested();return Field(name,size,count).CopyBytes();}
        float F(string name){float v=BinaryPrimitives.ReadSingleLittleEndian(Read(name,4,1));if(!float.IsFinite(v))throw new InvalidDataException("Finite119 parameter required.");return v;}
        bool Flag(string name){byte v=Read(name,1,1)[0];if(v>1)throw new InvalidDataException("Binary119 field required.");return v!=0;}
        if(BinaryPrimitives.ReadUInt16LittleEndian(Read("C8",2,1))!=7)throw new InvalidDataException("Actual seven groups required.");
        byte[] offsets=Read("D0",2,8),labels=Read("D8",2,25);int last=0;var groups=new ushort[7][];var weights=new float[7];
        for(int i=0;i<8;i++)
        {
            Check("knowledge:group-boundary");int next=BinaryPrimitives.ReadUInt16LittleEndian(offsets.AsSpan(i*2));if(next<last||next>25||i==0&&next!=0||i==7&&next!=25)throw new InvalidDataException("Actual ordered119 group ranges required.");
            if(i>0){var group=new ushort[next-last];for(int j=0;j<group.Length;j++){token.ThrowIfCancellationRequested();ushort label=BinaryPrimitives.ReadUInt16LittleEndian(labels.AsSpan((last+j)*2));if(label>=217)throw new InvalidDataException("Actual AL217 group ordinal required.");group[j]=label;}groups[i-1]=group;weights[i-1]=F($"E0[{i-1}]");}last=next;
        }
        bool related=Flag("80"),sizeCosts=Flag("B8");float first=F("84"),second=F("88"),dot=F("BC"),punctuation=F("C0"),power=F("100");
        Check("knowledge:complete");return new(owner,groups,weights,related,first,second,sizeCosts,dot,punctuation,power,budget);
    }
    //Conservative logical copy/work reservation, not native allocation or processRSS.
    internal static long Preflight(int fields,int groups,int symbols,int networks,int lexical,int acoustic)
    {
        if(fields is<1 or>256||groups!=7||symbols!=25||networks!=4||lexical!=104||acoustic!=217)throw new ArgumentException("Exact bounded English MAIN dimensions required.");
        long bytes=checked(4096L+fields*128L+groups*128L+symbols*16L+networks*64L+lexical*1024L+acoustic*256L);
        if(bytes>1048576)throw new ArgumentException("Small MAIN119 copy/work envelope exceeded.");return bytes;
    }
}
