using InkRecognition;
using System.Text.Json;
if(args.Length!=5)throw new ArgumentException("AK LK engine separation capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
var source=MainFixture.Create(ak,engine,table);
var owner=new LegacyOwnedMainWordSpanPreparation(ak,lk,engine,source,table);
var policy=LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,owner);
int guards=0;
using(var file=File.Create(args[4]))using(var w=new Utf8JsonWriter(file))
{
    w.WriteStartObject();w.WriteNumber("sourceFormat22F9",policy.Refresh.SourceFormat22f9);w.WriteStartArray("rows");
    for(int n=0;n<256;n++)
    {
        byte[] bytes=Enumerable.Range(0,64).Select(i=>(byte)(i*73+n)).ToArray();
        byte[] word=Enumerable.Range(0,120).Select(i=>(byte)(i*37+n*3)).ToArray();
        var packet=new LegacyPreparedMainWordPublicationPacket(bytes);byte[] result=packet.CopyWordStorageWithPacketFields(word);
        bytes[0]^=255;byte[] copy=packet.CopyStorage();copy[1]^=255;
        if(!result.AsSpan().SequenceEqual(packet.CopyWordStorageWithPacketFields(word)))throw new Exception("Packet storage escaped");
        w.WriteStartObject();w.WriteString("packet",Convert.ToHexString(packet.CopyStorage()));w.WriteString("word",Convert.ToHexString(word));w.WriteString("result",Convert.ToHexString(result));
        w.WriteNumber("previous",packet.PreviousSymbol);w.WriteNumber("first",packet.FirstBoundaryOverride);w.WriteNumber("last",packet.LastBoundaryOverride);
        w.WriteNumber("hint38",packet.BoundaryHint38Bits);w.WriteNumber("hint3c",packet.BoundaryHint3CBits);w.WriteEndObject();
    }
    w.WriteEndArray();Reject(()=>new LegacyPreparedMainWordPublicationPacket(new byte[63]));
    Reject(()=>new LegacyPreparedMainWordPublicationPacket(new byte[64]).CopyWordStorageWithPacketFields(new byte[119]));
    w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
}
Console.WriteLine($"PASS256 prepared21FE30 field packets/{guards} guards; actual sourceFormat22F9={policy.Refresh.SourceFormat22f9}. No outer result publication claim.");
void Reject(Action a){try{a();}catch(ArgumentException){guards++;return;}throw new Exception("Invalid extent accepted");}
