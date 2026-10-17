using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if(args.Length!=12)throw new ArgumentException("AK LK engine separation case publication line30 line50 normalization circles controls capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
using var nf=File.OpenRead(args[8]);using var cr=File.OpenRead(args[9]);
var norm=LegacyNativeUnicodeNormalizationData.Load(nf);var circles=LegacyNativeCircledPayloadData.Load(cr);
var raw=LegacyFreshMainSingleLinePlainRecognition.Prepare(ak,lk,engine,File.ReadAllBytes(args[6]),File.ReadAllBytes(args[7]),
    Word(0),0,table,cases,publication).Run(new byte[120],new byte[40],new ushort[256],new byte[64]);
var emission=LegacyFreshMainGlyphUtf8Emission.Prepare(raw,norm,circles);
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[10]));int tested=0,appends=0,reuses=0,failed=0,guards=0;
foreach(var test in controls.RootElement.GetProperty("cases").EnumerateArray())
{
    using var strings=new LegacyNativeUtf8StringPool();var initial=new List<LegacyPreparedTextGlyphReuseRecord>();
    foreach(var row in test.GetProperty("initial").EnumerateArray())
    {
        var text=row.GetProperty("text");initial.Add(new(Convert.FromHexString(row.GetProperty("storage").GetString()!),
            text.ValueKind==JsonValueKind.Null?null:strings.Intern(Convert.FromHexString(text.GetString()!))));
    }
    foreach(var value in test.GetProperty("preintern").EnumerateArray())strings.Intern(Convert.FromHexString(value.GetString()!));
    var owner=new LegacyPreparedMainTextGlyphReuse(emission,strings,publication,norm,initial,test.GetProperty("aggregate").GetUInt32());
    var inputs=test.GetProperty("incoming").EnumerateArray().Select(r=>new LegacyPreparedGlyphReuseInput(r.GetProperty("glyph").GetUInt16(),
        Convert.FromHexString(r.GetProperty("utf8").GetString()!),r.GetProperty("failure").GetInt32())).ToArray();
    var actual=owner.ApplyPrepared(inputs,test.GetProperty("marker").GetInt32());
    var expected=test.GetProperty("expected");
    if(actual.Succeeded!=expected.GetProperty("ok").GetBoolean()||actual.AdjustedMarkerIndex!=expected.GetProperty("marker").GetInt32()||
        actual.OriginalRecordCount!=expected.GetProperty("original").GetInt32()||actual.VisitedInputCount!=expected.GetProperty("visited").GetInt32()||
        actual.AppendedRecordCount!=expected.GetProperty("appended").GetInt32()||owner.AggregateFlags!=expected.GetProperty("aggregate").GetUInt32()||
        strings.Count!=expected.GetProperty("poolCount").GetInt32())throw new InvalidDataException($"Prepared21760 scalar case{tested} differs.");
    var rows=owner.CopyRecords();var targets=expected.GetProperty("records");
    if(rows.Count!=targets.GetArrayLength())throw new InvalidDataException("Partial row count differs.");
    for(int i=0;i<rows.Count;i++)
    {
        if(!rows[i].CopyStorage40().AsSpan().SequenceEqual(Convert.FromHexString(targets[i].GetProperty("storage").GetString()!))||
            rows[i].Text?.Identity!=(targets[i].GetProperty("identity").ValueKind==JsonValueKind.Null?null:targets[i].GetProperty("identity").GetInt32()))
            throw new InvalidDataException($"Prepared21760 complete row case{tested}/{i} differs.");
        byte[] altered=rows[i].CopyStorage40();altered[0]^=255;
        if(altered.AsSpan().SequenceEqual(owner.CopyRecords()[i].CopyStorage40()))throw new InvalidDataException("Record storage aliases caller.");
    }
    int incompleteAppend=!actual.Succeeded&&actual.VisitedInputCount<inputs.Length&&
        inputs[actual.VisitedInputCount].FailureStage is 4 or 5?1:0;
    appends+=actual.AppendedRecordCount;reuses+=actual.VisitedInputCount-actual.AppendedRecordCount+incompleteAppend;
    if(!actual.Succeeded){failed++;Reject(()=>owner.ApplyPrepared([],0));}
    tested++;
}
using var actualStrings=new LegacyNativeUtf8StringPool();var actualOwner=new LegacyPreparedMainTextGlyphReuse(emission,actualStrings,publication,norm,[]);
using var stream=File.Create(args[11]);using var writer=new Utf8JsonWriter(stream);writer.WriteStartObject();writer.WriteStartArray("sources");
int actualCalls=0,actualRecords=0;
foreach(var node in raw.Session.Ownership.GlyphPool.CopyOwnerNodes())
{
    int count=node.Records.Length/40;if(count==0)continue;var snapshot=new LegacyNativeGlyphRecordOwner([node]);var selected=new byte[count*8];
    for(int i=0;i<count*2;i++)BinaryPrimitives.WriteUInt16LittleEndian(selected.AsSpan(i*4),(ushort)(i%count));
    writer.WriteStartObject();writer.WriteNumber("key",node.Key);writer.WriteBase64String("actualGlyph40",node.Records.Span);
    writer.WriteBase64String("selection4",selected);writer.WriteStartArray("calls");
    for(int call=0;call<3;call++)
    {
        var result=actualOwner.Apply(snapshot,selected,node.Key,count*2-1);
        if(!result.Succeeded)throw new InvalidDataException("Actual owned emission refused unexpectedly.");
        writer.WriteStartObject();writer.WriteNumber("call",call);writer.WriteNumber("marker",result.AdjustedMarkerIndex);
        writer.WriteNumber("original",result.OriginalRecordCount);writer.WriteNumber("appended",result.AppendedRecordCount);writer.WriteNumber("aggregate",actualOwner.AggregateFlags);
        writer.WriteStartArray("records");foreach(var record in actualOwner.CopyRecords())
        {writer.WriteStartObject();writer.WriteBase64String("scalar40",record.CopyStorage40());writer.WriteNumber("identity",record.Text!.Identity);writer.WriteBase64String("utf8",record.Text.ReadBytes());writer.WriteEndObject();}writer.WriteEndArray();writer.WriteEndObject();actualCalls++;
    }
    writer.WriteEndArray();writer.WriteEndObject();actualRecords+=count;
}
writer.WriteEndArray();writer.WriteEndObject();writer.Flush();
using(var other=new LegacyNativeUtf8StringPool())
{
    var foreign=new LegacyPreparedTextGlyphReuseRecord(new byte[40],other.Intern("A"u8));
    Reject(()=>new LegacyPreparedMainTextGlyphReuse(emission,actualStrings,publication,norm,[foreign]));
}
using(var canceled=new CancellationTokenSource())
{
    canceled.Cancel();Reject(()=>actualOwner.ApplyPrepared([],0,canceled.Token));
}
using(var interruptedStrings=new LegacyNativeUtf8StringPool())
{
    var interrupted=new LegacyPreparedMainTextGlyphReuse(emission,interruptedStrings,publication,norm,[]);
    var first=interrupted.ApplyPrepared([new(0,"A"u8.ToArray())],0);var previous=interrupted.CopyRecords()[0].CopyStorage40();
    uint flags=interrupted.AggregateFlags;
    Reject(()=>interrupted.ApplyPrepared([new(0x1234,"B"u8.ToArray(),6)],0));
    var prefix=interrupted.CopyRecords();byte[] expected=new byte[40];BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(8),0x234);
    BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(32),1);
    if(prefix.Count!=2||!prefix[0].CopyStorage40().AsSpan().SequenceEqual(previous)||
        !prefix[1].CopyStorage40().AsSpan().SequenceEqual(expected)||prefix[1].Text is null||
        !prefix[1].Text!.ReadBytes().AsSpan().SequenceEqual("B"u8)||interrupted.AggregateFlags!=flags)
        throw new InvalidDataException("Portable cancellation discarded or over-completed the native write prefix.");
    Reject(()=>interrupted.ApplyPrepared([],0));
}
Console.WriteLine($"PASS {tested} independent prepared controls/{appends} appended rows/{reuses} reused rows/{failed} partial failures, {actualCalls} genuine calls/{actualRecords} actual glyph records and {guards} guards. Source-node state remains prepared; no accepted wrapper claim.");
void Reject(Action operation){try{operation();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Required guard ignored.");}
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots)
{var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
