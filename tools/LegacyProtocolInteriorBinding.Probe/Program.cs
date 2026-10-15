using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;
if(args.Length!=5)throw new ArgumentException("AK LK engine separation output.json");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]),sep=File.ReadAllBytes(args[3]);
var fixture=MainFixture.Create(ak,lk,engine,sep);var owner=new LegacyOwnedMainWordSpanPreparation(ak,lk,engine,fixture.Sampled,fixture.Table);
var policy=LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,owner);var confidence=LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,owner.DictionaryOwner);
var callbacks=LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak,confidence,fixture.Sampled);
var geometry=LegacyOwnedRussianMainSourceGeometry.Prepare(ak,confidence,fixture.Sampled);
var service=LegacyOwnedRussianMainInteriorDelimiter.Prepare(ak,callbacks,geometry);
var producer=LegacyOwnedRussianProtocolGlyphProducer.Prepare(ak,policy,callbacks,service);
if(!ReferenceEquals(producer.InteriorDelimiter,service)||!ReferenceEquals(producer.SourceCallbacks,callbacks))throw new InvalidDataException("Typed service identity lost.");
int controls=0,guards=0,emitted=0;byte[] valid=new byte[72];
W32(valid,0,owner.SourceGraph.NodeCount-1);W16(valid,4,0);W32(valid,12,-1);W16(valid,16,65535);
W32(valid,24,0);W16(valid,28,0);W32(valid,32,-1);W32(valid,36,-1);W16(valid,40,65535);W16(valid,56,74);W16(valid,58,0xfffb);valid[60]=3;
using var output=File.Create(args[4]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteNumber("nodeCount",owner.SourceGraph.NodeCount);w.WriteStartArray("cases");
for(int seed=0;seed<256;seed++)for(int mode=0;mode<4;mode++)
{
    byte[] row=valid.ToArray();if((mode&1)!=0){W32(row,24,-1);W16(row,28,65535);}if((mode&2)!=0)W16(row,56,65535);
    int cursor=new int[]{0,1,255,256}[seed&3];ushort[] labels=Enumerable.Range(0,256).Select(i=>(ushort)(seed*17+i*31)).ToArray();
    var scratch=new LegacyOrdinaryGlyphProductionScratch(labels,cursor,7,9,0x7fc12345,0x80000000,-123,0x55aa);
    byte[] residue=Enumerable.Range(0,64).Select(i=>(byte)(seed*13+i*23+1)).ToArray();uint b0=0x7fc10000u+(uint)seed;
    var result=producer.Produce(row,[],scratch,2,b0,residue);
    int current=I32(row,0)+(U16(row,56)==65535?1:0);bool expect=cursor>0&&I32(row,24)!=-1&&current!=owner.SourceGraph.NodeCount;
    if(scratch.RemainingCursor!=cursor-(expect?1:0)||scratch.ComponentFlag!=0||scratch.ExternalVariant!=0||scratch.Score0CBits!=0x3f800000||scratch.Score10Bits!=0x3f800000||scratch.Cost14!=0||scratch.PublicationFlags06!=0)throw new InvalidDataException("FFFB typed callback transport differs.");
    if(expect){labels[cursor-1]=0xfffb;emitted++;}if(!labels.AsSpan().SequenceEqual(scratch.CopyLabels()))throw new InvalidDataException("Native label prefix changed.");
    w.WriteStartObject();w.WriteBase64String("row",row);w.WriteBase64String("residue",residue);w.WriteNumber("seed",seed);w.WriteNumber("cursor",cursor);w.WriteNumber("b0",b0);
    w.WriteBase64String("request",result.CopyRequest());w.WriteNumber("remaining",scratch.RemainingCursor);w.WriteBase64String("labels",Bytes(scratch.CopyLabels()));
    w.WriteNumber("score0C",scratch.Score0CBits);w.WriteNumber("score10",scratch.Score10Bits);w.WriteNumber("cost",scratch.Cost14);w.WriteNumber("component",scratch.ComponentFlag);w.WriteNumber("external",scratch.ExternalVariant);w.WriteNumber("publication",scratch.PublicationFlags06);w.WriteEndObject();controls++;
}
w.WriteEndArray();
Reject(()=>LegacyOwnedRussianProtocolGlyphProducer.Prepare(ak,policy,callbacks,(LegacyOwnedRussianMainInteriorDelimiter)null!));
Reject(()=>LegacyOwnedRussianProtocolGlyphProducer.Prepare(ak,policy,callbacks,service,new(true)));
var cloneCallbacks=LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak,confidence,fixture.Sampled);var cloneService=LegacyOwnedRussianMainInteriorDelimiter.Prepare(ak,cloneCallbacks,geometry);
Reject(()=>LegacyOwnedRussianProtocolGlyphProducer.Prepare(ak,policy,callbacks,cloneService));
var foreignConfidence=LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,owner.DictionaryOwner);var foreignCallbacks=LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak,foreignConfidence,fixture.Sampled);
var foreignGeometry=LegacyOwnedRussianMainSourceGeometry.Prepare(ak,foreignConfidence,fixture.Sampled);var foreignService=LegacyOwnedRussianMainInteriorDelimiter.Prepare(ak,foreignCallbacks,foreignGeometry);
Reject(()=>LegacyOwnedRussianProtocolGlyphProducer.Prepare(ak,policy,callbacks,foreignService));
//The original API remains explicit about its unsupported interior geometry.
var original=LegacyOwnedRussianProtocolGlyphProducer.Prepare(ak,policy,callbacks);var partial=new LegacyOrdinaryGlyphProductionScratch(new ushort[256],256,7,9,17,19,23,29);
Reject(()=>original.Produce(valid,[],partial,2,0,new byte[64]));
if(partial.ComponentFlag!=0||partial.ExternalVariant!=0||partial.RemainingCursor!=256||partial.Score0CBits!=17||partial.Score10Bits!=19||partial.Cost14!=23||partial.PublicationFlags06!=29||partial.CopyLabels().Any(v=>v!=0))throw new InvalidDataException("Original unsupported-prefix behavior changed.");
w.WriteNumber("controls",controls);w.WriteNumber("emitted",emitted);w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS{controls} full216BC0 request/scratch/unit-score bindings,{emitted} actualFFFB callback emissions,{guards} owner/cancel/oldAPI guards. PacketAC2 and row72 are prepared, not native packet production.");
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Invalid interior owner accepted.");}
static byte[] Bytes(ushort[] values){byte[] bytes=new byte[values.Length*2];for(int i=0;i<values.Length;i++)W16(bytes,i*2,values[i]);return bytes;}
static int I32(ReadOnlySpan<byte>b,int o)=>BinaryPrimitives.ReadInt32LittleEndian(b[o..]);
static ushort U16(ReadOnlySpan<byte>b,int o)=>BinaryPrimitives.ReadUInt16LittleEndian(b[o..]);
static void W32(Span<byte>b,int o,int v)=>BinaryPrimitives.WriteInt32LittleEndian(b[o..],v);
static void W16(Span<byte>b,int o,ushort v)=>BinaryPrimitives.WriteUInt16LittleEndian(b[o..],v);
