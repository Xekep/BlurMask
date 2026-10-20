using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;
if(args.Length!=3)throw new ArgumentException("models-directory audited-controller.json output.json");
using var input=JsonDocument.Parse(File.ReadAllBytes(args[1]));int guards=0,rows=0,multi=0,skipped=0;var canceled=new CancellationToken(true);
void Guard(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or NotSupportedException){guards++;return;}throw new Exception("Expected refusal.");}
InkStroke[] Ink(JsonElement e)=>e.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
string path="en-US/en_US-ak-cur.lite.res";var bytes=File.ReadAllBytes(Path.Combine(args[0],path));var parent=LegacySource247WireLayout.Read(bytes);
var model=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(bytes,parent,parent.Children.First(c=>c.Kind==0x249));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(args[0],"ru-RU/legacy-separation-profile.json")));
var inputs=input.RootElement.GetProperty("Rows").EnumerateArray().Select(r=>Ink(r.GetProperty("Raw"))).ToList();
int originalCount=inputs.Count;var all=inputs.Select((r,i)=>(Name:$"actual-{i}",Raw:r)).ToList();
//Real raw ingress variations, followed by actual NN/primary/terminal selection.
for(int v=0;v<2;v++)for(int i=0;i<4;i++)
{
 var raw=inputs[i].Select(s=>new InkStroke(s.Points.ToArray())).ToArray();
 for(int s=0;s<raw.Length;s++){var list=raw[s].Points.ToList();var last=list[^1];list.Add(v==0?last:new(MathF.BitIncrement(last.X),last.Y));raw[s]=new(list.ToArray());}
 all.Add((v==0?$"duplicate-{i}":$"near-duplicate-{i}",raw));
}
using var file=File.Create(args[2]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteString("Path",path);w.WriteString("Fingerprint",model.Source.Fingerprint);w.WriteNumber("Exponent",model.SourceScoreExponent);w.WriteStartArray("Actual");
void Raw(string name,IReadOnlyList<InkStroke> strokes){w.WriteStartArray(name);foreach(var stroke in strokes){w.WriteStartArray();foreach(var p in stroke.Points){w.WriteStartArray();w.WriteNumberValue(p.X);w.WriteNumberValue(p.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();}
void Key(LegacySourcePointKey key){w.WriteStartArray();w.WriteNumberValue(key.First);w.WriteNumberValue(key.Second);w.WriteNumberValue(key.Third);w.WriteEndArray();}
void Ranges(string name,IReadOnlyList<LegacyFirstResultSourceRange> source){w.WriteStartArray(name);foreach(var r in source){w.WriteStartArray();Key(r.First);Key(r.Last);w.WriteEndArray();}w.WriteEndArray();}
int index=0;LegacyEnglish119SourceSubmission? firstSubmission=null;LegacyEnglish119PlainTextResult? firstText=null;
foreach(var inputCase in all)
{
 var raw=inputCase.Raw;var submission=LegacyEnglish119SourceSubmission.Prepare(model,raw);firstSubmission??=submission;
 if(raw[0].Points is InkPoint[] caller){var saved=caller[0];caller[0]=new(999999,999999);if(submission.CopyRawChildInput()[0].Points[0]!=saved)throw new Exception("Caller storage escaped.");caller[0]=saved;}
 //Caller storage and returned snapshots must not alter retained source input.
 var copy=submission.CopyRawChildInput();var point=copy[0].Points[0];((InkPoint[])copy[0].Points)[0]=new(999999,999999);
 if(submission.CopyRawChildInput()[0].Points[0]!=point)throw new Exception("Raw snapshot escaped.");
 var prepared=submission.Preparation;var candidates=LegacyOwnedEnglish119Candidates.Prepare(prepared,table);
 var cache=LegacyOwnedEnglish119AcousticCache.Prepare(candidates);var confidence=LegacyOwnedEnglish119GlyphConfidence.Prepare(cache);
 var costs=LegacyOwnedEnglish119ControllerCosts.Prepare(confidence,table);var primary=LegacyOwnedEnglish119FirstPrimary.Prepare(costs,table).Run();
 var contexts=LegacyOwnedEnglish119ContextPasses.Prepare(primary).Run();var terminal=LegacyOwnedEnglish119Terminal.Select(contexts);
 var text=LegacyOwnedEnglish119PlainText.Materialize(terminal,new(new byte[120],new byte[40]));firstText??=text;
 int scored=cache.ScoredGroupCount;var source=LegacyOwnedEnglish119SourceRecords.Materialize(submission,text);
 var again=LegacyOwnedEnglish119SourceRecords.Materialize(submission,text);
 if(!ReferenceEquals(source.Submission,submission)||!ReferenceEquals(source.Text,text)||source.Rows.Count!=again.Rows.Count||cache.ScoredGroupCount!=scored)throw new Exception("Actual ownership/cache changed.");
 for(int r=0;r<source.Rows.Count;r++)
 {
  if(!source.Rows[r].CopyStorage().Span.SequenceEqual(again.Rows[r].CopyStorage().Span))throw new Exception("Unstable source720.");
  var ranges=source.Origins[r].CopyChildRanges();var saved=ranges[0];ranges[0]=default;
  if(source.Origins[r].CopyChildRanges()[0]!=saved)throw new Exception("Mutable provenance escaped.");
  var data=source.Rows[r].CopyStorage().ToArray();byte b=data[0];data[0]^=255;if(source.Rows[r].CopyStorage().Span[0]!=b)throw new Exception("Mutable row escaped.");
 }
 Guard(()=>LegacyOwnedEnglish119SourceRecords.Materialize(submission,text,canceled));
 Guard(()=>LegacyEnglish119SourceSubmission.Prepare(model,raw,canceled));
 Guard(()=>LegacyOwnedEnglish119SourceRecords.Materialize(LegacyEnglish119SourceSubmission.Prepare(model,raw),text));
 Guard(()=>((IList<LegacyFirstResultSourceRecord>)source.Rows).Add(null!));
 Guard(()=>((IList<LegacyEnglish119SourceRecordOrigin>)source.Origins).Add(null!));
 if(text.NativeWords.Count>1)
 {
  //Explicit corrupted prepared publication controls over a completed actual
  //result; these do not claim native malformed owners are reachable.
  LegacyEnglish119PlainTextResult Corrupt(LegacyFirstPublishedTextWord[] words)=>new(text.Factory,new(text.Storage,text.Factory.Alternative.Choice,words,[],[],[],0),text.AllocationStorageFingerprint,default);
  Guard(()=>LegacyOwnedEnglish119SourceRecords.Materialize(submission,Corrupt(text.NativeWords.Reverse().ToArray())));
  Guard(()=>LegacyOwnedEnglish119SourceRecords.Materialize(submission,Corrupt(text.NativeWords.Take(text.NativeWords.Count-1).ToArray())));
 }
 w.WriteStartObject();w.WriteNumber("Index",index++);w.WriteString("Name",inputCase.Name);Raw("Raw",submission.CopyRawChildInput());Raw("Deduplicated",prepared.CopyOriginalStrokes());Raw("Cut",candidates.CopyCutStrokes());
 w.WriteNumber("Height",prepared.Line.Height);w.WriteNumber("ScoredBefore",scored);w.WriteNumber("ScoredAfter",cache.ScoredGroupCount);
 w.WriteStartArray("Map200");foreach(int k in prepared.CopyPointKeys200())w.WriteNumberValue(k);w.WriteEndArray();
 w.WriteStartArray("CutSourceIndices");foreach(var r in candidates.CopyCutRecords())w.WriteNumberValue(r.OriginalPointIndex);w.WriteEndArray();
 w.WriteStartArray("CutCounts");foreach(int n in candidates.CopyCutCounts())w.WriteNumberValue(n);w.WriteEndArray();
 w.WriteStartArray("Atoms");foreach(var a in candidates.CopyAtoms()){w.WriteStartArray();w.WriteNumberValue(a.Range.StrokeIndex);w.WriteNumberValue(a.Range.StartPoint);w.WriteNumberValue(a.Range.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("Nodes");for(int n=0;n<candidates.NodeCount;n++){w.WriteStartArray();foreach(var a in candidates.CopyNode(n)){w.WriteStartArray();w.WriteNumberValue(a.StartContextId);w.WriteNumberValue(a.EndContextId);w.WriteNumberValue(a.Span);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("Words");foreach(var word in text.Words){w.WriteStartArray();foreach(ulong r in word.CopyReadingOrderReferences())w.WriteNumberValue(r);w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("Owners");foreach(var owner in text.CopyGlyphOwners()){w.WriteStartObject();w.WriteNumber("Key",owner.Key);w.WriteNumber("PackedCount",owner.PackedCount);w.WriteBase64String("Records",owner.Records.Span);w.WriteEndObject();}w.WriteEndArray();
 w.WriteStartArray("Rows");
 for(int r=0;r<source.Rows.Count;r++)
 {
  var origin=source.Origins[r];w.WriteStartObject();w.WriteNumber("Word",origin.WordOrdinal);w.WriteNumber("Ordinal",origin.ReferenceOrdinal);w.WriteNumber("Reference",origin.GlyphReference);
  Ranges("ChildRanges",origin.CopyChildRanges());Ranges("Ranges",source.Rows[r].Ranges);w.WriteBase64String("Storage",source.Rows[r].CopyStorage().Span);w.WriteNumber("Labels",source.Rows[r].LabelCount);
  var owner=text.Storage.GlyphPool.CopyOwnerNode(LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(origin.GlyphReference));var k=LegacyFirstResultGlyphKey.Decode(origin.GlyphReference);
  //Explicit pure-kernel control1 is not the actual loaded249 invocation.
  var control=LegacyFirstResultSourceRecord.Create(owner.Records.Span,k.Alternative,origin.CopyChildRanges(),submission.PhysicalMap,submission.Raw,text.Factory.Categories,1);
  w.WriteBase64String("PreparedPureExponentOneControl",control.CopyStorage().Span);w.WriteEndObject();
 }
 w.WriteEndArray();w.WriteEndObject();rows+=source.Rows.Count;multi+=text.Words.Count(x=>!x.IsDelimiter)>1?1:0;
 skipped+=text.Words.Sum(x=>x.CopyReadingOrderReferences().Count(r=>LegacyFirstResultGlyphKey.Decode(r).Arc<0));
}
w.WriteEndArray();
//Named prepared controls exercise bounded ingress/refusal, not NN winner claims.
w.WriteStartObject("PreparedControls");
Guard(()=>LegacyEnglish119SourceSubmission.Prepare(model,[]));Guard(()=>LegacyEnglish119SourceSubmission.Prepare(model,Enumerable.Repeat(new InkStroke([new(0,0)]),257).ToArray()));
Guard(()=>LegacyEnglish119SourceSubmission.Prepare(model,[new InkStroke(new InkPoint[4097])]));
Guard(()=>LegacyEnglish119SourceSubmission.Prepare(model,[new InkStroke(new InkPoint[2049]),new InkStroke(new InkPoint[2048])]));
foreach(float f in new[]{float.NaN,float.PositiveInfinity,float.NegativeInfinity,1_000_001f})Guard(()=>LegacyEnglish119SourceSubmission.Prepare(model,[new InkStroke([new(f,0)])]));
Guard(()=>LegacyOwnedEnglish119SourceRecords.Materialize(null!,firstText!));Guard(()=>LegacyOwnedEnglish119SourceRecords.Materialize(firstSubmission!,null!));
var sameParent=LegacySource247WireLayout.Read(bytes);var equalModel=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(bytes,sameParent,sameParent.Children.First(c=>c.Kind==0x249));
Guard(()=>LegacyOwnedEnglish119SourceRecords.Materialize(LegacyEnglish119SourceSubmission.Prepare(equalModel,inputs[0]),firstText!));
using(var ct=new CancellationTokenSource())
{
 var cancelPoints=new CancelPoints(new InkPoint[]{new(0,0),new(1,2),new(2,1),new(3,4)},ct,1);
 Guard(()=>LegacyEnglish119SourceSubmission.Prepare(model,[new InkStroke(cancelPoints)],ct.Token));
 if(!ct.IsCancellationRequested)throw new Exception("Prepared mid-ingress cancel did not trigger.");
}
var baseStroke=inputs.SelectMany(s=>s).OrderByDescending(s=>s.Points.Count).First().Points;
var bounded=Enumerable.Repeat(baseStroke[0],4096-baseStroke.Count).Concat(baseStroke).ToArray();
var maximum=LegacyEnglish119SourceSubmission.Prepare(model,[new InkStroke(bounded)]);if(maximum.CopyRawChildInput().Sum(s=>s.Points.Count)!=4096)throw new Exception("Exact raw cap failed.");
var oneRange=new LegacyFirstResultSourceRange(new(0,0,0),new(0,0,0));InkStroke[] one=[new InkStroke([new(0,0)])];
void Preflight(IReadOnlyList<LegacyFirstResultSourceRange> ranges,IReadOnlyList<InkStroke> raw,int rb=65536,int pb=1048576,CancellationToken token=default)
{LegacyOwnedEnglish119SourceRecords.PreflightRanges(ranges,raw,ref rb,ref pb,token);}
Guard(()=>Preflight([],one));Guard(()=>Preflight(Enumerable.Repeat(oneRange,4097).ToArray(),one));
foreach(var bad in new[]{new LegacyFirstResultSourceRange(new(1,0,0),new(2,0,0)),new(new(0,-1,0),new(0,0,0)),new(new(0,0,0),new(0,1,0)),new(new(0,0,-1),new(0,0,0)),new(new(0,0,0),new(0,0,1)),new(new(0,0,1),new(0,0,0))})Guard(()=>Preflight([bad],one));
Guard(()=>Preflight([oneRange],one,0));Guard(()=>Preflight([oneRange],one,65536,0));
int rangeRemaining=65536,pointRemaining=1048576;var block=Enumerable.Repeat(oneRange,4096).ToArray();
for(int i=0;i<16;i++)LegacyOwnedEnglish119SourceRecords.PreflightRanges(block,one,ref rangeRemaining,ref pointRemaining);
if(rangeRemaining!=0||pointRemaining!=1048576-65536)throw new Exception("Exact range cap changed.");
Guard(()=>LegacyOwnedEnglish119SourceRecords.PreflightRanges([oneRange],one,ref rangeRemaining,ref pointRemaining));
rangeRemaining=65536;pointRemaining=1048576;var fullRange=new LegacyFirstResultSourceRange(new(0,0,0),new(0,0,4095));
InkStroke[] full=[new InkStroke(new InkPoint[4096])];LegacyOwnedEnglish119SourceRecords.PreflightRanges(Enumerable.Repeat(fullRange,256).ToArray(),full,ref rangeRemaining,ref pointRemaining);
if(pointRemaining!=0)throw new Exception("Exact point cap changed.");Guard(()=>LegacyOwnedEnglish119SourceRecords.PreflightRanges([oneRange],full,ref rangeRemaining,ref pointRemaining));
using(var ct=new CancellationTokenSource()){Guard(()=>Preflight(new CancelRanges([oneRange,oneRange],ct),one,token:ct.Token));if(!ct.IsCancellationRequested)throw new Exception("Prepared source preflight cancellation did not trigger.");}
var emptyStorage=firstText!.Factory.CreateStorage(default);
var emptyText=new LegacyEnglish119PlainTextResult(firstText.Factory,new(emptyStorage,firstText.Factory.Alternative.Choice,[],[],[],[],0),firstText.AllocationStorageFingerprint,default);
var empty=LegacyOwnedEnglish119SourceRecords.Materialize(firstSubmission!,emptyText);if(empty.Rows.Count!=0||empty.Origins.Count!=0)throw new Exception("Prepared empty source emitted a fabricated row.");
w.WriteNumber("ExactRawCap",4096);w.WriteNumber("ExactRangeCap",65536);w.WriteNumber("ExactPointVisitCap",1048576);w.WriteNumber("PreparedEmptyRows",0);
w.WriteNumber("ForeignEqualModel",1);w.WriteNumber("MidIngressCancellation",1);w.WriteNumber("PreparedMidSourcePreflightCancellation",1);w.WriteEndObject();
w.WriteNumber("Sources",index);w.WriteNumber("Rows",rows);w.WriteNumber("MultiWord",multi);w.WriteNumber("SkippedDelimiters",skipped);w.WriteNumber("Guards",guards);w.WriteEndObject();w.Flush();
if(originalCount!=124||index!=132||multi==0||skipped==0||model.SourceScoreExponent!=.75f)throw new Exception("Missing genuine source coverage.");
Console.WriteLine($"PASS132 actual raw EN119 inputs/{rows}source720/{multi}multiword/{skipped}skipped delimiter refs/{guards}guards; loaded249 .75, no native parent claim.");
sealed class CancelPoints(IReadOnlyList<InkPoint> points,CancellationTokenSource source,int at):IReadOnlyList<InkPoint>
{
 public int Count=>points.Count;public InkPoint this[int i]{get{if(i==at)source.Cancel();return points[i];}}
 public IEnumerator<InkPoint> GetEnumerator(){for(int i=0;i<Count;i++)yield return this[i];}System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()=>GetEnumerator();
}
sealed class CancelRanges(IReadOnlyList<LegacyFirstResultSourceRange> ranges,CancellationTokenSource source):IReadOnlyList<LegacyFirstResultSourceRange>
{
 public int Count=>ranges.Count;public LegacyFirstResultSourceRange this[int i]{get{if(i==1)source.Cancel();return ranges[i];}}
 public IEnumerator<LegacyFirstResultSourceRange> GetEnumerator(){for(int i=0;i<Count;i++)yield return this[i];}System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()=>GetEnumerator();
}
