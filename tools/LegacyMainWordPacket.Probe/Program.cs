using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
if(args.Length!=5)throw new ArgumentException("AK LK engine separation capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
var source=MainFixture.Create(ak,engine,table);
var owner=new LegacyOwnedMainWordSpanPreparation(ak,lk,engine,source,table);
var policy=LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,owner);
var confidence=LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,owner.DictionaryOwner);
var geometry=LegacyOwnedRussianMainSourceGeometry.Prepare(ak,confidence,source);
ulong identity=1;
LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest r)
{var bytes=new byte[r.RequiredBytes];r.PreviousStorage.Span.CopyTo(bytes);return new(bytes,identity++);}
var pass=new LegacyOwnedMainDirectWordPass(ak,policy,table,Allocate).RunOwned();
var terminal=LegacyOwnedFreshMainTerminalInvocation.Prepare(policy).SelectOwnedPass(pass);
var compact=LegacyOwnedMainCompactWordMaterialization.Materialize(ak,terminal,confidence,new byte[4096*40]);
var resultOwner=LegacyFreshMainPlainResultOwnership.Create(compact,geometry);
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
    w.WriteEndArray();w.WriteStartArray("ownedWords");
    for(int n=0;n<64;n++)
    {
        byte[] residue=Enumerable.Range(0,120).Select(i=>(byte)(i*29+n)).ToArray();
        foreach(var part in new[]{(32,8),(88,12),(104,12)})residue.AsSpan(part.Item1,part.Item2).Clear();
        var word=resultOwner.CreateWorkingWord(residue);byte[] initial=word.CopyStorage();
        word.AppendGlyph(0,n,true,new byte[40]);
        var packet=new LegacyPreparedMainWordPublicationPacket(Enumerable.Range(0,64).Select(i=>(byte)(i*53+n)).ToArray());
        byte[] before=word.CopyStorage();word.ApplyMainPublicationPacket(packet);
        byte[] applied=word.CopyStorage();word.ResetPlain();
        if(word.HeadReference!=ulong.MaxValue||!ReferenceEquals(word.Pool,resultOwner.GlyphPool))throw new Exception("Plain reset lost owned pool/head");
        w.WriteStartObject();w.WriteString("residue",Convert.ToHexString(residue));w.WriteString("initial",Convert.ToHexString(initial));
        w.WriteString("before",Convert.ToHexString(before));w.WriteString("packet",Convert.ToHexString(packet.CopyStorage()));
        w.WriteString("applied",Convert.ToHexString(applied));w.WriteString("reset",Convert.ToHexString(word.CopyStorage()));w.WriteEndObject();
    }
    w.WriteEndArray();
    if(resultOwner.CaptureGlyphGeometry||resultOwner.OwnerC4||!ReferenceEquals(resultOwner.Materialization,compact)||!ReferenceEquals(resultOwner.Geometry,geometry))throw new Exception("Fresh main factory policies changed");
    Reject(()=>new LegacyPreparedMainWordPublicationPacket(new byte[63]));
    Reject(()=>new LegacyPreparedMainWordPublicationPacket(new byte[64]).CopyWordStorageWithPacketFields(new byte[119]));
    var lookalike=LegacyOwnedRussianMainSourceGeometry.Prepare(ak,
        LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,owner.DictionaryOwner),source);
    Reject(()=>LegacyFreshMainPlainResultOwnership.Create(compact,lookalike));
    Reject(()=>LegacyFreshMainPlainResultOwnership.Create(compact,geometry,new CancellationToken(true)));
    var foreign=LegacyFreshMainPlainResultOwnership.Create(compact,geometry).CreateWorkingWord(new byte[120]);
    Reject(()=>resultOwner.WordPublisher.Publish(foreign,(_,_)=>0,(_,_)=>0));
    var pointerResidue=new byte[120];pointerResidue[32]=1;
    Reject(()=>resultOwner.CreateWorkingWord(pointerResidue));
    w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
}
Console.WriteLine($"PASS256 prepared21FE30 field packets/{guards} guards; actual sourceFormat22F9={policy.Refresh.SourceFormat22f9}. No outer result publication claim.");
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid storage or ownership request accepted");}
