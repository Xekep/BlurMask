using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;
if(args.Length!=6)throw new ArgumentException("AK LK engine separation publication output.json");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]),sep=File.ReadAllBytes(args[3]);
using var pubFile=File.OpenRead(args[4]);var publication=LegacyNativeTextPublicationData.Load(pubFile);
var fixture=MainFixture.Create(ak,lk,engine,sep);var owner=new LegacyOwnedMainWordSpanPreparation(ak,lk,engine,fixture.Sampled,fixture.Table);
var policy=LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,owner);
var confidence=LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,owner.DictionaryOwner);
var callbacks=LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak,confidence,fixture.Sampled);
var ordinary=LegacyOwnedRussianOrdinaryGlyphProducer.Prepare(ak,policy,confidence,fixture.Table,callbacks,publication);
var protocol=LegacyOwnedRussianProtocolGlyphProducer.Prepare(ak,policy,callbacks);
ulong allocation=1;LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest r)
{var d=new byte[r.RequiredBytes];r.PreviousStorage.Span.CopyTo(d);return new(d,allocation++);}
var pass=new LegacyOwnedMainDirectWordPass(ak,policy,fixture.Table,Allocate).RunOwned();
var terminal=LegacyOwnedFreshMainTerminalInvocation.Prepare(policy).SelectOwnedPass(pass);
var compact=LegacyOwnedMainCompactWordMaterialization.Materialize(ak,terminal,confidence,Enumerable.Repeat((byte)0xa7,4096*40).ToArray());
var append=LegacyOwnedMainGlyphBacktrackRows.Prepare(ak,compact,callbacks,new byte[72]);byte[] all=append.CopyRows();
byte[] regular=Enumerable.Range(0,append.Count).Select(i=>all.AsSpan(i*72,72).ToArray()).First(r=>r[60]==0);
byte[] delimiter=Enumerable.Range(0,append.Count).Select(i=>all.AsSpan(i*72,72).ToArray()).First(r=>r[60]==3&&U16(r,58)==0xfffa);
int controls=0,guards=0,created=0,reused=0;
using var output=File.Create(args[5]);using var w=new Utf8JsonWriter(output);
w.WriteStartObject();w.WriteBase64String("compact",compact.CopyCompactStorage());
w.WriteStartArray("starts");foreach(int v in compact.CopyNodeStarts2478())w.WriteNumberValue(v);w.WriteEndArray();
w.WriteStartArray("sourceHeaders");foreach(uint header in owner.CopySourceHeaders())w.WriteNumberValue(header);w.WriteEndArray();
w.WriteStartArray("groups");for(int g=0;g<owner.SourceGraph.Groups.Count;g++)
{var scores=confidence.ScoreGroup(g);w.WriteStartObject();w.WriteStartArray("candidates");foreach(var c in scores.Candidates.Span){w.WriteStartArray();w.WriteNumberValue(c.Symbol);w.WriteNumberValue(c.Cost);w.WriteNumberValue(BitConverter.SingleToUInt32Bits(c.Confidence));w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();}w.WriteEndArray();
w.WriteStartArray("graph");for(int n=0;n<owner.SourceGraph.NodeCount;n++){w.WriteStartArray();foreach(var a in owner.SourceGraph.GetNode(n))w.WriteNumberValue(a.GroupId);w.WriteEndArray();}w.WriteEndArray();
w.WriteStartArray("graphFlags");for(int n=0;n<owner.SourceGraph.NodeCount;n++){w.WriteStartArray();foreach(var a in owner.SourceGraph.GetNode(n))w.WriteNumberValue(a.Flags);w.WriteEndArray();}w.WriteEndArray();
w.WriteStartArray("cases");
for(int seed=0;seed<64;seed++)for(int mode=0;mode<12;mode++)
{
    byte[] residue=Enumerable.Range(0,40).Select(i=>(byte)(seed*37+i*17+3)).ToArray();
    ushort[] labelResidue=Enumerable.Range(0,256).Select(i=>(ushort)(0x8000+i*13+seed)).ToArray();
    byte[] requestResidue=Enumerable.Range(0,64).Select(i=>(byte)(seed*29+i*11)).ToArray();
    var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
    byte[] storage=Enumerable.Range(0,120).Select(i=>(byte)(seed*7+i*19)).ToArray();
    W64(storage,32,0);W64(storage,88,0);W64(storage,104,0);W32(storage,96,0);W32(storage,112,0);
    var word=LegacyNativeWordBuilder.CreateEmpty(pool,storage);
    byte[] first=regular.ToArray(),second=regular.ToArray();W32(second,8,I32(second,8)+1);
    byte[] rows=mode==1?delimiter.ToArray():mode is 2 or 3 or 4 or 7?first.Concat(second).ToArray():first;
    if(mode is 8 or 9){rows=delimiter.ToArray();W16(rows,58,mode==8?(ushort)0xfffc:(ushort)0xfffa);W32(rows,24,0);W16(rows,28,0);W32(rows,32,I32(regular,8));}
    if(mode>=10)rows=[];
    if(mode==7)rows[68]=1;
    if(mode is 3 or 4)
    {
        ulong key=BinaryPrimitives.ReadUInt64LittleEndian(first.AsSpan(48));int[] starts=compact.CopyNodeStarts2478();
        var a=pool.AppendDescriptor(key,I32(first,8)-starts[I32(first,0)],false,residue);
        var b=pool.AppendDescriptor(key,I32(second,8)-starts[I32(second,0)],false,residue);
        if(mode==3){byte[] row=pool.CopyRecord(a.Reference);W64(row,32,b.Reference);pool.ReplaceRecord(a.Reference,row);}
    }
    if(mode is 5 or 11)word.AppendGlyph(BinaryPrimitives.ReadUInt64LittleEndian(first.AsSpan(48)),777,false,residue);
    var before=pool.CopyOwnerNodes();byte[] beforeWord=word.CopyStorage();uint ac=(uint)(seed*3),b0=0x7fc12345u+(uint)seed;
    var result=LegacyPreparedOrdinaryGlyphChain.Apply(compact,word,rows,[],ordinary,protocol,0xffff,ac,b0,mode==6,residue,labelResidue,requestResidue);
    Require(result.Succeeded,"Prepared explicit segment failed.");created+=result.CreatedCount;reused+=result.ReusedCount;
    w.WriteStartObject();w.WriteNumber("mode",mode);w.WriteBase64String("rows",rows);w.WriteBase64String("residue",residue);
    w.WriteBase64String("requestResidue",requestResidue);w.WriteNumber("ac",ac);w.WriteNumber("b0",b0);
    w.WriteBase64String("beforeWord",beforeWord);Nodes("beforePool",before);w.WriteBase64String("afterWord",word.CopyStorage());Nodes("afterPool",pool.CopyOwnerNodes());
    w.WriteNumber("firstNew",result.FirstNewReference);w.WriteNumber("last",result.LastProcessedReference);w.WriteNumber("created",result.CreatedCount);w.WriteNumber("reused",result.ReusedCount);
    if(mode is 1 or 8 or 9)
    {
        var scratch=new LegacyOrdinaryGlyphProductionScratch(labelResidue,256);
        var produced=protocol.Produce(rows,compact.CopyCompactStorage(),scratch,ac,b0,requestResidue);
        w.WriteBase64String("protocolRequest",produced.CopyRequest());w.WriteNumber("protocolRemaining",scratch.RemainingCursor);
    }
    w.WriteEndObject();controls++;
}
w.WriteEndArray();
//Explicit compact scalar controls distinguish24C0 ASCII '-' from24C2 soft
//hyphen and exercise the extra protocol predecessor skip. Not native winners.
w.WriteStartArray("lookback");
for(int n=0;n<16;n++)for(int mode=0;mode<3;mode++)
{
    byte[] prepared=delimiter.ToArray();W16(prepared,58,0xfffc);W32(prepared,24,0);W16(prepared,28,0);W32(prepared,32,0);
    byte[] neighbors=new byte[120];W16(neighbors,28,mode==2?(ushort)183:(ushort)185);W32(neighbors,16,1);
    W16(neighbors,68,mode==1?(ushort)0xfffa:(ushort)74);W32(neighbors,56,2);W64(neighbors,48,0);
    W16(neighbors,108,74);W32(neighbors,96,-1);W64(neighbors,88,1);
    byte[] requestResidue=Enumerable.Range(0,64).Select(i=>(byte)(n*23+i*41)).ToArray();
    var scratch=new LegacyOrdinaryGlyphProductionScratch(new ushort[256],256);
    var produced=protocol.Produce(prepared,neighbors,scratch,0x80000000u+(uint)n,0x7fc01234u+(uint)n,requestResidue);
    Require(scratch.RemainingCursor==255&&scratch.CopyLabels()[255]==0xfffc,"Real FFFC callback output differs.");
    byte[] copy=produced.CopyRequest();copy[0]^=0xff;Require(!copy.AsSpan().SequenceEqual(produced.CopyRequest()),"Protocol request escaped ownership.");
    w.WriteStartObject();w.WriteNumber("mode",mode);w.WriteBase64String("row",prepared);w.WriteBase64String("compact",neighbors);
    w.WriteBase64String("residue",requestResidue);w.WriteNumber("ac",0x80000000u+(uint)n);w.WriteNumber("b0",0x7fc01234u+(uint)n);
    w.WriteBase64String("request",produced.CopyRequest());w.WriteNumber("remaining",scratch.RemainingCursor);
    w.WriteNumber("score0C",scratch.Score0CBits);w.WriteNumber("score10",scratch.Score10Bits);w.WriteNumber("cost",scratch.Cost14);w.WriteEndObject();
}
w.WriteEndArray();
//Cancellation/structural guards precede any mutation. Residuals are not silently
//discarded; this executor has no claim of a genuine217DB0 source overlay.
var guardPool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);var guardWord=LegacyNativeWordBuilder.CreateEmpty(guardPool,new byte[120]);
Reject(()=>LegacyPreparedOrdinaryGlyphChain.Apply(compact,guardWord,regular,new ulong[]{1},ordinary,protocol,0,0,0,false,new byte[40],new ushort[256],new byte[64]));
Reject(()=>LegacyPreparedOrdinaryGlyphChain.Apply(compact,guardWord,regular,[],ordinary,protocol,0,0,0,false,new byte[39],new ushort[256],new byte[64]));
Reject(()=>LegacyPreparedOrdinaryGlyphChain.Apply(compact,guardWord,regular,[],ordinary,protocol,0,0,0,false,new byte[40],new ushort[256],new byte[64],token:new(true)));
Require(guardPool.CopyOwnerNodes().Count==0&&guardWord.HeadReference==ulong.MaxValue,"Preflight guard mutated output.");
Reject(()=>LegacyPreparedOrdinaryGlyphChain.Apply(compact,guardWord,regular,[],null!,protocol,0,0,0,false,new byte[40],new ushort[256],new byte[64]));
Reject(()=>LegacyPreparedOrdinaryGlyphChain.Apply(compact,guardWord,regular,[],ordinary,null!,0,0,0,false,new byte[40],new ushort[256],new byte[64]));
Reject(()=>LegacyPreparedOrdinaryGlyphChain.Apply(compact,guardWord,regular,[],ordinary,protocol,0,0,0,false,new byte[40],new ushort[255],new byte[64]));
Reject(()=>LegacyPreparedOrdinaryGlyphChain.Apply(compact,guardWord,regular,[],ordinary,protocol,0,0,0,false,new byte[40],new ushort[256],new byte[63]));
var foreignPolicy=LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,owner);
var foreignProtocol=LegacyOwnedRussianProtocolGlyphProducer.Prepare(ak,foreignPolicy,callbacks);
Reject(()=>LegacyPreparedOrdinaryGlyphChain.Apply(compact,guardWord,regular,[],ordinary,foreignProtocol,0,0,0,false,new byte[40],new ushort[256],new byte[64]));
var corrupt=ak.ToArray();corrupt[0]^=1;Reject(()=>LegacyOwnedRussianProtocolGlyphProducer.Prepare(corrupt,policy,callbacks));
Reject(()=>LegacyOwnedRussianProtocolGlyphProducer.Prepare(ak,policy,null!));
Reject(()=>LegacyOwnedRussianProtocolGlyphProducer.Prepare(ak,null!,callbacks));
var partialRows=regular.Concat(regular).ToArray();partialRows[60]=6;
Reject(()=>LegacyPreparedOrdinaryGlyphChain.Apply(compact,guardWord,partialRows,[],ordinary,protocol,0,0,0,false,new byte[40],new ushort[256],new byte[64]));
Require(guardPool.CopyOwnerNodes().Sum(n=>n.Records.Length/40)==2,"Unsupported later writer erased its native allocated prefix.");
w.WriteBase64String("partialRows",partialRows);w.WriteBase64String("partialWord",guardWord.CopyStorage());Nodes("partialPool",guardPool.CopyOwnerNodes());
w.WriteNumber("controls",controls);w.WriteNumber("guards",guards);w.WriteNumber("created",created);w.WriteNumber("reused",reused);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS{controls} explicit218560 segments, {created} new/{reused} reused records, fullWORD/pool bytes;{guards} guards. Segmentation and217DB0 remain separate.");
void Nodes(string name,IReadOnlyList<LegacyNativeGlyphRecordNode> nodes){w.WriteStartArray(name);foreach(var n in nodes){w.WriteStartObject();w.WriteNumber("key",n.Key);w.WriteNumber("packed",n.PackedCount);w.WriteBase64String("records",n.Records.Span);w.WriteEndObject();}w.WriteEndArray();}
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidDataException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid chain accepted.");}
static void Require(bool v,string m){if(!v)throw new Exception(m);}
static ushort U16(ReadOnlySpan<byte>d,int o)=>BinaryPrimitives.ReadUInt16LittleEndian(d[o..]);
static int I32(ReadOnlySpan<byte>d,int o)=>BinaryPrimitives.ReadInt32LittleEndian(d[o..]);
static void W16(Span<byte>d,int o,ushort v)=>BinaryPrimitives.WriteUInt16LittleEndian(d[o..],v);
static void W32(Span<byte>d,int o,int v)=>BinaryPrimitives.WriteInt32LittleEndian(d[o..],v);
static void W64(Span<byte>d,int o,ulong v)=>BinaryPrimitives.WriteUInt64LittleEndian(d[o..],v);
