using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if(args.Length is <1 or >2)throw new ArgumentException("Usage: controls.json [ru_RU-ak-cur.lite.res]");
var controls=JsonSerializer.Deserialize(File.ReadAllText(args[0]),ControlsContext.Default.Controls)!;
int count=0;
foreach(var test in controls.Cases)
{
    var source=test.Strokes.Select(s=>new InkStroke(s.Select(p=>new InkPoint(p[0],p[1])).ToArray())).ToArray();
    var prepared=LegacyOrdinarySourceInk.Prepare(source,new(0x135,true,0,0));
    for(int s=0;s<source.Length;s++)
    {
        if(!prepared.OriginalPointIndices[s].Span.SequenceEqual(test.Indices[s]))throw new Exception($"Index oracle differs at{count}/{s}.");
        for(int p=0;p<test.Indices[s].Length;p++)
        {
            if(prepared.Strokes[s].Points[p]!=source[s].Points[test.Indices[s][p]])throw new Exception("Ingress retained wrong point.");
            if(prepared.Chronology.Resolve(s,p).Key!=new LegacySourcePointKey(0,s,test.Indices[s][p]))throw new Exception("Source identity lost.");
        }
    }
    if(!prepared.SourceStrokeProbabilities.Span.SequenceEqual(LegacySourceStrokeEnclosure.Extract(prepared.Strokes)))throw new Exception("Ingress geometry not used by enclosure.");
    var disabled=LegacyOrdinarySourceInk.Prepare(source,new(0x135,false,0,0));
    if(disabled.SourceStrokeProbabilities.Span.ContainsAnyExcept(0f))throw new Exception("Disabled source probability must be zero.");
    count++;
}
byte[] Own(bool enabled,bool longMarker,int mode)
{
    using var ms=new MemoryStream();using var w=new BinaryWriter(ms);
    w.Write(0x0101fe90u);w.Write(0u);w.Write(0x247u);w.Write((ushort)0);w.Write((ushort)0x10d);
    w.Write((ushort)0x201);w.Write((ushort)0x100);w.Write(123456u);w.Write(new byte[56]);
    w.Write(longMarker?-1:0x07000000);if(longMarker)w.Write(0x07000000);w.Write(0u);
    w.Write((byte)0);w.Write(0x135);w.Write((byte)mode);
    if(mode==2){w.Write((byte)1);w.Write(.5f);w.Write(.5f);w.Write(1f);w.Write(new byte[7]);}
    w.Write((byte)0);w.Write((byte)0);w.Write((byte)0);w.Write((ushort)0);w.Write((ushort)65535);
    for(int i=0;i<4;i++)w.Write(1f);w.Write(new byte[4]);w.Write(.25f);
    w.Write((byte)(enabled?1:0));w.Write((byte)0);w.Write(0f);w.Write((byte)0);w.Write(0f);w.Write((byte)0);w.Write(0);
    byte[] data=ms.ToArray();BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(4),checked((uint)data.Length-8));return data;
}
foreach(bool enabled in new[]{false,true})foreach(bool longMarker in new[]{false,true})foreach(int mode in new[]{0,1,2})
{
    byte[] bytes=Own(enabled,longMarker,mode);var profile=LegacyOrdinarySourceInk.ReadV10dResourceProfile(bytes);
    if(profile.SourceKind!=0x135||profile.EnclosureEnabled!=enabled||bytes[profile.EnclosureFlagOffset]!=(enabled?1:0))throw new Exception("Own source profile field forwarding differs.");
    for(int cut=0;cut<bytes.Length;cut++)
    {
        byte[] truncated=bytes[..cut];
        if(cut>=16)BinaryPrimitives.WriteUInt32LittleEndian(truncated.AsSpan(4),checked((uint)cut-8));
        try{LegacyOrdinarySourceInk.ReadV10dResourceProfile(truncated);throw new Exception("Truncated source accepted.");}catch(InvalidDataException){}
    }
    byte[] badFlag=(byte[])bytes.Clone();badFlag[profile.EnclosureFlagOffset]=2;
    try{LegacyOrdinarySourceInk.ReadV10dResourceProfile(badFlag);throw new Exception("Nonbinary enable flag accepted.");}catch(InvalidDataException){}
    byte[] missingNode=(byte[])bytes.Clone();BinaryPrimitives.WriteInt32LittleEndian(missingNode.AsSpan(missingNode.Length-4),1);
    try{LegacyOrdinarySourceInk.ReadV10dResourceProfile(missingNode);throw new Exception("Missing declared processor accepted.");}catch(InvalidDataException){}
}
var square=new InkStroke(new InkPoint[]{new(0,0),new(10,0),new(10,10),new(0,10),new(0,0)});
var enclosed=new InkStroke(new InkPoint[]{new(3,3),new(4,4),new(5,5)});
var roundtrip=LegacyOrdinarySourceInk.Prepare([square,enclosed],LegacyOrdinarySourceInk.ReadV10dResourceProfile(Own(true,true,2)));
if(Math.Abs(roundtrip.SourceStrokeProbabilities.Span[0]-12.566f/16)>1e-6)throw new Exception("Resource→ordinary source→enclosure roundtrip differs.");
try{LegacyOrdinarySourceInk.Prepare([square],new(0x135,true,0,0),new CancellationToken(true));throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
Console.WriteLine($"PASS {count} independent raw-adjacency/map/chronology controls;12 resource profiles, all truncations, enabled/disabled source→enclosure roundtrip/cancellation.");
if(args.Length==2)
{
    var profile=LegacyOrdinarySourceInk.ReadV10dResourceProfile(File.ReadAllBytes(args[1]));
    if(profile!=new LegacyOrdinarySourceProfile(0x135,true,5285,6088))throw new Exception("Actual Russian source profile differs.");
    var ownMir=new InkStroke[]{
        new(new InkPoint[]{new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)}),
        new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),
        new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})};
    var actual=LegacyOrdinarySourceInk.Prepare(ownMir,profile);
    if(actual.SourceStrokeProbabilities.Span.ContainsAnyExcept(0f))throw new Exception("Own printed MIR enclosure score changed.");
    Console.WriteLine("PASS actualRU sourceKind135 parameters@5285 enclosureEnabled1@6088; own printed MIR computed enclosure [0,0,0], not assumed defaults.");
}
sealed record Controls(Case[] Cases);
sealed record Case(float[][][] Strokes,int[][] Indices);
[System.Text.Json.Serialization.JsonSerializable(typeof(Controls))]
partial class ControlsContext:System.Text.Json.Serialization.JsonSerializerContext;
