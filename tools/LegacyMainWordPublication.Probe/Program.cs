using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
using System.Security.Cryptography;
if(args.Length is not (7 or 11))throw new ArgumentException("AK LK engine separation case publication capture [line30 line50 manifest word-index]");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);var cases=LegacyNativeCaseData.Load(cf);var data=LegacyNativeTextPublicationData.Load(pf);
LegacyFreshCaseRussianDictionaryPreparation dictionary;
LegacyOwnedMainWordSpanPreparation owner;
LegacyFreshMainSingleLineContext? lineContext = null;
if (args.Length == 7)
{
    var sampled = MainFixture.Create(ak, engine, table);
    dictionary = LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak, lk, model, sampled, cases, data);
    owner = new LegacyOwnedMainWordSpanPreparation(ak, dictionary, sampled, table);
}
else
{
    using var manifest = JsonDocument.Parse(File.ReadAllBytes(args[9]));
    if (manifest.RootElement.GetProperty("commit").GetString() != "c6629026b1fdef51fd08e3d2fddffd82f97e60a5" ||
        manifest.RootElement.GetProperty("penUpMetadata").GetString() != "absent")
        throw new InvalidDataException("Explicitly unsegmented pinned recorded coordinates required.");
    var sample = manifest.RootElement.GetProperty("samples").EnumerateArray()
        .Where(s => s.GetProperty("kind").GetString() == "word").ElementAt(int.Parse(args[10]));
    string name = sample.GetProperty("file").GetString()!;
    if (name != Path.GetFileName(name)) throw new InvalidDataException("Direct sibling track required.");
    byte[] bytes = File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(args[9]))!, name));
    if (bytes.Length > 200000 || Convert.ToHexStringLower(SHA256.HashData(bytes)) != sample.GetProperty("trackSha256").GetString())
        throw new InvalidDataException("Recorded track hash changed.");
    using var parsed = JsonDocument.Parse(bytes);
    var points = parsed.RootElement.GetProperty("points").EnumerateArray()
        .Select(p => new InkPoint(p[0].GetSingle(), -p[1].GetSingle())).ToArray();
    if (points.Length is < 2 or > 4096) throw new InvalidDataException("Bounded recorded point vector required.");
    //Pen-up is absent in this dataset. One unsegmented path is an explicit
    //diagnostic input assumption; expected transcription is never read.
    var input = TrackedCaseInput.Create(ak, engine, table, File.ReadAllBytes(args[7]), File.ReadAllBytes(args[8]),
        [new InkStroke(points)], cases, data);
    dictionary = LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak, lk, model, input.Source, cases, data);
    owner = new LegacyOwnedMainWordSpanPreparation(ak, dictionary, input.Source, input.Context, table);
    lineContext = input.Context;
}

var policy=LegacyFreshCaseMainSourcePolicy.Prepare(ak,lk,LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,owner));
ulong identity=1;
LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageRequest r){var bytes=new byte[r.RequiredBytes];r.PreviousStorage.Span.CopyTo(bytes);return new(bytes,identity++);}
var pass=new LegacyOwnedMainCaseWordPass(ak,policy,lineContext is null?null:owner.MainLineAcoustic,table,Allocate).RunOwned();
var terminal=LegacyOwnedFreshCaseMainTerminalInvocation.Prepare(policy).SelectOwnedPass(pass);
var confidence=lineContext is null?LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,dictionary):LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,dictionary,lineContext);
var compact=LegacyOwnedMainCaseCompactWordMaterialization.Materialize(ak,policy,terminal,confidence,new byte[4096*40]);
var callbacks=lineContext is null?LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak,confidence,owner.Source):LegacyOwnedRussianMainSourceSymbolCallbacks.Prepare(ak,confidence,owner.MainLineSource);
var geometry=lineContext is null?LegacyOwnedRussianMainSourceGeometry.Prepare(ak,confidence,owner.Source):LegacyOwnedRussianMainSourceGeometry.Prepare(ak,confidence,owner.MainLineSource);
var interior=LegacyOwnedRussianMainInteriorDelimiter.Prepare(ak,callbacks,geometry);
var backtrack=LegacyOwnedMainGlyphBacktrackRows.Prepare(ak,policy,compact,callbacks,new byte[72]);
var plan=LegacyOwnedMainGlyphFlushPlan.Prepare(policy,backtrack);
int guards=0,words=0,flushes=0;
using var output=File.Create(args[6]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteStartArray("cases");
for(int n=0;n<32;n++)
{
    var ownership=LegacyFreshMainPlainResultOwnership.Create(compact,geometry);
    byte[] residue=Enumerable.Range(0,120).Select(i=>(byte)(i*37+n)).ToArray();
    foreach(var (offset,length) in new[]{(32,8),(88,12),(104,12)})residue.AsSpan(offset,length).Clear();
    var session=LegacyOwnedMainGlyphFlushSession.Prepare(ownership,plan,residue);
    var publisher=LegacyOwnedMainPlainWordPublication.Prepare(ak,session,table,callbacks,interior);
    byte[] record=Enumerable.Range(0,40).Select(i=>(byte)(i*17+n)).ToArray();
    ushort[] labels=Enumerable.Range(0,256).Select(i=>(ushort)(i*13+n)).ToArray();
    byte[] request=Enumerable.Range(0,64).Select(i=>(byte)(i*19+n)).ToArray();
    w.WriteStartObject();w.WriteNumber("ac",publisher.TopAcBits);w.WriteNumber("b0",publisher.TopB0Bits);
    w.WriteString("initial",Convert.ToHexString(session.CopyInitialWordStorage()));w.WriteStartArray("flushes");
    while(!session.IsComplete)
    {
        var associated=session.PrepareNext();byte[] before=associated.WorkingWord.CopyStorage();
        if(n==0&&associated.Ordinal==0)
        {
            Reject(()=>publisher.Publish(associated,new byte[39],labels,request));
            Reject(()=>publisher.Publish(associated,record,new ushort[255],request));
            Reject(()=>publisher.Publish(associated,record,labels,new byte[63]));
            using var canceled=new CancellationTokenSource();canceled.Cancel();
            try{publisher.Publish(associated,record,labels,request,canceled.Token);throw new Exception("Cancellation accepted");}
            catch(OperationCanceledException){guards++;}
            var foreignSession=LegacyOwnedMainGlyphFlushSession.Prepare(ownership,plan,residue);
            var foreignAssociation=foreignSession.PrepareNext();
            Reject(()=>publisher.Publish(foreignAssociation,record,labels,request));
            if(session.IsStopped||!associated.WorkingWord.CopyStorage().AsSpan().SequenceEqual(before))
                throw new Exception("Rejected precondition consumed or mutated actual pending callback");
        }
        var result=publisher.Publish(associated,record,labels,request);
        if(!result.Succeeded)throw new Exception("Actual owning CASE publication failed");
        byte[] after=associated.WorkingWord.CopyStorage();
        if(BinaryPrimitives.ReadUInt64LittleEndian(after.AsSpan(8))!=ulong.MaxValue)throw new Exception("Working WORD was not reset");
        w.WriteStartObject();w.WriteString("packet",Convert.ToHexString(associated.Packet.CopyStorage()));
        w.WriteString("rows",Convert.ToHexString(associated.CopyRows72()));w.WriteString("before",Convert.ToHexString(before));w.WriteString("after",Convert.ToHexString(after));
        w.WriteBoolean("succeeded",result.Succeeded);
        if(result.PublishedWord is {} node)
        {
            words++;w.WriteString("word",Convert.ToHexString(node.Word.CopyStorage()));
            w.WriteStartArray("span");w.WriteNumberValue(node.Span.First);w.WriteNumberValue(node.Span.Last);w.WriteEndArray();
            w.WriteStartArray("references");foreach(ulong reference in node.Word.FlatReferences)w.WriteNumberValue(reference);w.WriteEndArray();
        }
        else w.WriteNull("word");
        w.WriteStartArray("pool");foreach(var poolNode in ownership.GlyphPool.CopyOwnerNodes())
        {w.WriteStartObject();w.WriteNumber("key",poolNode.Key);w.WriteNumber("packed",poolNode.PackedCount);w.WriteString("records",Convert.ToHexString(poolNode.Records.Span));w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
        Reject(()=>publisher.Publish(associated,record,labels,request));
        session.CompleteCurrent(result);Reject(()=>session.CompleteCurrent(result));flushes++;
    }
    if(session.CompletedFlushCount!=plan.Flushes.Count||ownership.WordPublisher.Head is null)throw new Exception("Actual flush chronology incomplete");
    w.WriteEndArray();w.WriteEndObject();
}
w.WriteEndArray();w.WriteNumber("guards",guards);w.WriteNumber("words",words);w.WriteNumber("flushes",flushes);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS actual CASE compact→flush→217DB0→218560→21FE30: {words} words/{flushes} flush callbacks/{guards} single-use guards; no expected text or complete2271F0 claim.");
void Reject(Action action){try{action();}catch(Exception e)when(e is InvalidOperationException or ArgumentException){guards++;return;}throw new Exception("Stale publication accepted");}
