using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=5||File.Exists(args[4]))throw new ArgumentException("publication normalization reuse-controls source-controls fresh-capture");
using var pf=File.OpenRead(args[0]);var publication=LegacyNativeTextPublicationData.Load(pf);
using var nf=File.OpenRead(args[1]);var norm=LegacyNativeUnicodeNormalizationData.Load(nf);
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[2]));int tested=0,appends=0,reuses=0,failed=0,fullRecordRows=0;
foreach(var test in controls.RootElement.GetProperty("cases").EnumerateArray())
{
    using var strings=new LegacyNativeUtf8StringPool();var initial=new List<LegacyPreparedTextGlyphReuseRecord>();
    foreach(var row in test.GetProperty("initial").EnumerateArray())
    {
        var text=row.GetProperty("text");initial.Add(new(Convert.FromHexString(row.GetProperty("storage").GetString()!),
            text.ValueKind==JsonValueKind.Null?null:strings.Intern(Convert.FromHexString(text.GetString()!))));
    }
    foreach(var value in test.GetProperty("preintern").EnumerateArray())strings.Intern(Convert.FromHexString(value.GetString()!));
    var owner=LegacyPreparedMainTextGlyphReuse.PrepareEnglishNumeric(strings,publication,norm,initial,test.GetProperty("aggregate").GetUInt32());
    var inputs=test.GetProperty("incoming").EnumerateArray().Select(r=>new LegacyPreparedGlyphReuseInput(r.GetProperty("glyph").GetUInt16(),
        Convert.FromHexString(r.GetProperty("utf8").GetString()!),r.GetProperty("failure").GetInt32())).ToArray();
    var actual=owner.ApplyPrepared(inputs,test.GetProperty("marker").GetInt32());
    var expected=test.GetProperty("expected");
    if(actual.Succeeded!=expected.GetProperty("ok").GetBoolean()||actual.AdjustedMarkerIndex!=expected.GetProperty("marker").GetInt32()||
        actual.OriginalRecordCount!=expected.GetProperty("original").GetInt32()||actual.VisitedInputCount!=expected.GetProperty("visited").GetInt32()||
        actual.AppendedRecordCount!=expected.GetProperty("appended").GetInt32()||owner.AggregateFlags!=expected.GetProperty("aggregate").GetUInt32()||
        strings.Count!=expected.GetProperty("poolCount").GetInt32())throw new InvalidDataException($"Prepared21760 scalar case{tested} differs.");
    var rows=owner.CopyRecords();fullRecordRows+=rows.Count;var targets=expected.GetProperty("records");
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

using var sourceControls=JsonDocument.Parse(File.ReadAllBytes(args[3]));var processed=new List<object>();
foreach(var item in sourceControls.RootElement.GetProperty("Cases").EnumerateArray())
{
 var counts=item.GetProperty("Counts").EnumerateArray().Select(x=>x.GetInt32()).ToArray();
 var document=LegacyOwnedRawInkDocument.Create(counts.Select(n=>new InkStroke(Enumerable.Range(0,n).Select(i=>new InkPoint(i,i)).ToArray())).ToArray(),default);
 var ops=LegacyOwnedRawInkRangeOperations.Prepare(document);
 LegacyNativeInkPositionRange[] Read(JsonElement value)=>value.EnumerateArray().Select(x=>new LegacyNativeInkPositionRange(new(x[0].GetUInt64()),new(x[1].GetUInt64()))).ToArray();
 var original=Read(item.GetProperty("Original"));var ranges=item.GetProperty("Ranges").EnumerateArray().Select(Read).ToArray();
 var positive=item.GetProperty("Positive").EnumerateArray().Select(x=>x.GetBoolean()).ToArray();var joiners=item.GetProperty("Joiner").EnumerateArray().Select(x=>x.GetBoolean()).ToArray();
 var skipped=new bool[ranges.Length];var output=new List<object>();var interner=LegacyOwnedMainTextSourceInterner.Prepare(document);
 for(int i=0;i<ranges.Length;i++)
 {
  var result=LegacyPreparedMainTextSourceProcessing.Process(ops,original,ranges,skipped,positive,joiners,i,default);ranges[i]=result.Key;
  var node=interner.InternPrepared(result.Key);output.Add(new{Ordinary=result.Ordinary,Key=result.Key.Select(x=>new[]{x.First.Value,x.Last.Value}).ToArray(),Node=node.Node.Identity,Inserted=node.Inserted});
 }
 processed.Add(new{Id=item.GetProperty("Id").GetString(),Steps=output,Tree=interner.CopyTree(),Root=interner.RootIdentity});
}
var flagsOutput=new List<uint>();
foreach(var item in sourceControls.RootElement.GetProperty("Flags").EnumerateArray())
{
 var key=item.GetProperty("Key").EnumerateArray().Select(x=>new LegacyNativeInkPositionRange(new(x[0].GetUInt64()),new(x[1].GetUInt64()))).ToArray();
 flagsOutput.Add(LegacyEnglishCaseNode0Completion.CharacterFlags(item.GetProperty("Ordinary").GetBoolean(),item.GetProperty("Prior").GetByte(),item.GetProperty("Aggregate").GetUInt32(),key));
}
var rangeBudgets=new List<object>();
foreach(int a in new[]{0,1,2,8,64,4096,65536})foreach(int b in new[]{0,1,2,8,64,4096,65536})foreach(int r in new[]{1,2,128})
{
 var reserve=LegacyEnglishCaseNode0Completion.GraphCharacterRangeReservation.Start(a,r).AddProbe(b);var total=reserve.Finish();
 rangeBudgets.Add(new{A=a,B=b,R=r,reserve.Slots,reserve.Moves,reserve.Controls,reserve.KeyBound,reserve.Probes,total.Payload,total.Copies,total.Visits});
}
int growthChecks=0;var rng=new Random(0x24f30);
var docGrowth=LegacyOwnedRawInkDocument.Create(new[]{new InkStroke(Enumerable.Range(0,16).Select(i=>new InkPoint(i,i)).ToArray())},default);
var opsGrowth=LegacyOwnedRawInkRangeOperations.Prepare(docGrowth);
for(int c=0;c<1024;c++)
{
 int a=rng.Next(9),b=rng.Next(9);
 LegacyNativeInkPositionRange[] Generate(int n)=>Enumerable.Range(0,n).Select(_=>{int x=rng.Next(16),y=rng.Next(16);return new LegacyNativeInkPositionRange(new(((ulong)Math.Min(x,y))<<16|3UL),new(((ulong)Math.Max(x,y))<<16|5UL));}).ToArray();
 var current=Generate(a);var later=Generate(b);var normalized=opsGrowth.Union([],later);var overlap=opsGrowth.Intersect(normalized,current);
 if(normalized.Length>b||overlap.LongLength>(long)a*b)throw new InvalidDataException("Range cardinality certificate violated.");
 foreach(var target in new[]{current,later})
 {
  try{var cut=opsGrowth.Subtract(target,overlap);if(cut.LongLength>target.LongLength+overlap.LongLength)throw new InvalidDataException("One-cut split certificate violated.");}
  catch(ArgumentOutOfRangeException){/* Unsorted generic numeric inputs may refuse; no actual owner. */}
 }
 growthChecks++;
}
File.WriteAllText(args[4],JsonSerializer.Serialize(new{Scope="PREPARED_NEUTRAL_SOURCE_PROCESS_INTERN_REUSE_NOT_ACTUAL_CHAR",ReuseCases=tested,Appends=appends,Reuses=reuses,FailedPrefixes=failed,FullRecordRows=fullRecordRows,Flags=flagsOutput,SourceCases=processed,RangeBudgets=rangeBudgets,GrowthChecks=growthChecks,ActualOwnerExecuted=false,NN=false}));
void Reject(Action action){try{action();}catch(InvalidOperationException e){if(e.GetType()!=typeof(InvalidOperationException))throw;return;}throw new Exception("Retry not refused");}
