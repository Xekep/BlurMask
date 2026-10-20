using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;
if(args.Length!=3)throw new ArgumentException("models controller-input.json capture.json");
var bytes=File.ReadAllBytes(Path.Combine(args[0],"en-US/en_US-ak-cur.lite.res"));var layout=LegacySource247WireLayout.Read(bytes);
var model=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(bytes,layout,layout.Children.First(c=>c.Kind==0x249));
var m30=File.ReadAllBytes(Path.Combine(args[0],"line-geometry/line-2e3-30.res"));var m50=File.ReadAllBytes(Path.Combine(args[0],"line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(args[0],"ru-RU/legacy-separation-profile.json")));
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[1]));
InkStroke[] Ink(JsonElement e)=>e.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
var cases=doc.RootElement.GetProperty("Rows").EnumerateArray().Select((x,i)=>(Name:"actual-"+i,Raw:Ink(x.GetProperty("Raw")),Resolution:96f)).ToList();
InkStroke Zig(float x,float y,int n=12)=>new(Enumerable.Range(0,n).Select(i=>new InkPoint(x+3*i,y+(i%2)*20)).ToArray());
cases.Add(("multiline-reverse",new[]{Zig(120,1000),Zig(0,1000),Zig(120,0),Zig(0,0)},96));
cases.Add(("sole-flat",new[]{new InkStroke([new(0,0),new(100,0)])},0));
cases.Add(("all-flat-multiple",new[]{new InkStroke([new(0,0),new(100,0)]),new InkStroke([new(0,20),new(100,20)])},0));
for(int i=0;i<4;i++)
{
 var row=cases[i].Raw;cases.Add(("duplicate-"+i,row.Select(st=>new InkStroke(st.Points.Concat(new[]{st.Points[^1]}).ToArray())).ToArray(),96));
 cases.Add(("near-duplicate-"+i,row.Select(st=>new InkStroke(st.Points.Concat(new[]{new InkPoint(MathF.BitIncrement(st.Points[^1].X),st.Points[^1].Y)}).ToArray())).ToArray(),96));
}
cases.Add(("filtered-parent-gap",new[]{Zig(0,0),new InkStroke([new(0,2),new(300,2)]),Zig(70,0)},0));
cases.Add(("multiline-forward",new[]{Zig(0,0),Zig(120,0),Zig(0,1000),Zig(120,1000)},96));
var results=new List<object>();var refusals=new List<object>();int guards=0;
void Guard(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException or NotSupportedException){guards++;return;}throw new Exception("Expected refusal.");}
float[][][] XY(IReadOnlyList<InkStroke> ink)=>ink.Select(s=>s.Points.Select(p=>new[]{p.X,p.Y}).ToArray()).ToArray();
int[] K(LegacySourcePointKey k)=>new[]{k.First,k.Second,k.Third};
int[][][] R(IReadOnlyList<LegacyFirstResultSourceRange> ranges)=>ranges.Select(r=>new[]{K(r.First),K(r.Last)}).ToArray();
object ChildCapture(LegacyEnglish119ParentChild child,LegacyEnglish119PlainTextResult text)
{
 var prep=child.Submission.Preparation;var candidates=text.Factory.Candidates;
 return D(("DescriptorOrdinal",child.DescriptorOrdinal),("Map",child.CopyCurrentParentMap()),("SoleFlat",child.IsSoleFlat),("Raw",XY(child.Submission.CopyRawChildInput())),("Deduplicated",XY(prep.CopyOriginalStrokes())),("Cut",XY(candidates.CopyCutStrokes())),("Height",prep.Line.Height),("Map200",prep.CopyPointKeys200()),("CutSourceIndices",candidates.CopyCutRecords().Select(r=>r.OriginalPointIndex).ToArray()),("CutCounts",candidates.CopyCutCounts()),("Atoms",candidates.CopyAtoms().Select(a=>new[]{a.Range.StrokeIndex,a.Range.StartPoint,a.Range.EndPointExclusive}).ToArray()),("Nodes",Enumerable.Range(0,candidates.NodeCount).Select(n=>candidates.CopyNode(n).Select(a=>new[]{a.StartContextId,a.EndContextId,a.Span}).ToArray()).ToArray()),("Words",text.NativeWords.Select(w=>w.Published.Word.FlatReferences.ToArray()).ToArray()),("Owners",text.CopyGlyphOwners().Select(o=>D(("Key",o.Key),("PackedCount",o.PackedCount),("Records",Convert.ToBase64String(o.Records.Span)))).ToArray()));
}

LegacyEnglish119PlainTextResult Recognize(LegacyEnglish119SourceSubmission s)
{
 var c=LegacyOwnedEnglish119Candidates.Prepare(s.Preparation,table);var cache=LegacyOwnedEnglish119AcousticCache.Prepare(c);
 var confidence=LegacyOwnedEnglish119GlyphConfidence.Prepare(cache);var costs=LegacyOwnedEnglish119ControllerCosts.Prepare(confidence,table);
 var primary=LegacyOwnedEnglish119FirstPrimary.Prepare(costs,table).Run();var contexts=LegacyOwnedEnglish119ContextPasses.Prepare(primary).Run();
 return LegacyOwnedEnglish119PlainText.Materialize(LegacyOwnedEnglish119Terminal.Select(contexts),new(new byte[120],new byte[40]));
}
foreach(var item in cases)
{
 
 LegacyEnglish119ParentSubmission p;
 try{p=LegacyEnglish119ParentSubmission.Prepare(model,item.Raw,item.Resolution,m30,m50);}
 catch(NotSupportedException e){refusals.Add(D(("Name",item.Name),("Reason",e.Message)));continue;}
 var texts=p.Children.Select(c=>Recognize(c.Submission)).ToArray();
 var before=p.CopyDescriptors();
 Guard(()=>p.Publish(texts,new(true)));if(p.IsFaulted||p.CopyRowPrefix().Count!=0)throw new Exception("Pre-entry cancel mutated parent.");
 var foreign=LegacyEnglish119ParentSubmission.Prepare(model,item.Raw,item.Resolution,m30,m50);
 Guard(()=>foreign.Publish(texts));
 p.Publish(texts);Guard(()=>p.Publish(texts));
 var rawCopy=p.CopyRaw();if(rawCopy[0].Points is InkPoint[] writable)writable[0]=new(99999,99999);
 if(!p.CopyRaw()[0].Points.SequenceEqual(item.Raw[0].Points))throw new Exception("Raw owner escaped.");
 var current=p.CopyCurrentStrokes();var order=p.CopyCurrentOriginalPhysical();
 var meta=current.Select((st,si)=>st.Points.Select((v,pi)=>D(("Key",K(p.ResolveCurrentPoint(si,pi).Key)),("End",K(p.ResolveCurrentEndingMetadata(si,pi).Key)),("MetadataPointIndex",p.ResolveCurrentPoint(si,pi).MetadataPointIndex),("KeyRecordIndex",p.ResolveCurrentPoint(si,pi).KeyRecordIndex),("RemainderByte",p.ResolveCurrentPoint(si,pi).RemainderByte))).ToArray()).ToArray();
 var rows=p.CopyRowPrefix();var origin=p.CopyOriginPrefix();
 foreach(var o in origin)if(!ReferenceEquals(o.Text,texts[p.Children.ToList().IndexOf(o.Child)]))throw new Exception("Actual richresult provenance lost.");
 results.Add(D(("Name",item.Name),("Resolution",item.Resolution),("Raw",XY(p.CopyRaw())),("Retained",XY(p.CopyRetainedIngress())),("Split",XY(p.CopySplitStrokes())),("Current",XY(current)),("SourcePrimary",p.SourceScales.Primary),("SourceSecondary",p.SourceScales.Secondary),("Samples",p.CopyScaleSamples()),("InitialNeuralScale",p.InitialNeuralScale),("Groups",p.CopyProviderGroups()),("SplitOrder",p.CopySplitOriginalPhysical()),("Order",order),("Calls",p.CopyReceiverCalls().Select(c=>D(("DescriptorOrdinal",c.DescriptorOrdinal),("StartingNeuralScale",c.StartingNeuralScale),("EndingNeuralScale",c.EndingNeuralScale),("LocalPhysicalCount",c.LocalPhysicalCount),("ProviderGroupCount",c.ProviderGroupCount),("VerticalStripCount",c.VerticalStripCount))).ToArray()),("Metadata",meta),("DescriptorsBefore",before.Select(Convert.ToHexString).ToArray()),("Descriptors",p.CopyDescriptors().Select(Convert.ToHexString).ToArray()),("Child",p.Children.Select((c,i)=>ChildCapture(c,texts[i])).ToArray()),("Rows",rows.Select((r,i)=>D(("Descriptor",origin[i].Child.DescriptorOrdinal),("Word",origin[i].WordOrdinal),("Ordinal",origin[i].ReferenceOrdinal),("Reference",origin[i].Reference),("Flat",origin[i].SoleFlatOverride),("ChildRanges",R(origin[i].CopyChildRanges())),("Ranges",R(r.Ranges)),("Storage",Convert.ToBase64String(r.CopyStorage().Span)),("Labels",r.LabelCount))).ToArray()),("Trace",p.CopyTrace())));
}
//Genuine raw ingress variations plus exact first-recorded input for failure
//scheduling. Prepared controls do not substitute expected recognition labels.
var failureRaw=cases[0].Raw;var failures=new List<object>();
foreach(string point in new[]{"enter","row:0","child-close:0"})
{
 var owner=LegacyEnglish119ParentSubmission.Prepare(model,failureRaw,96,m30,m50);var texts=owner.Children.Select(c=>Recognize(c.Submission)).ToArray();using var cts=new CancellationTokenSource();
 Guard(()=>owner.PublishPrepared(texts,stage=>{if(stage==point)cts.Cancel();},cts.Token));
 if(!owner.IsFaulted||owner.IsComplete)throw new Exception("Entered failure not poisoned.");
 Guard(()=>owner.Publish(texts));failures.Add(D(("Point",point),("Rows",owner.CopyRowPrefix().Count),("Trace",owner.CopyTrace()),("Descriptors",owner.CopyDescriptors().Select(Convert.ToHexString).ToArray())));
}
var preflightOwner=LegacyEnglish119ParentSubmission.Prepare(model,failureRaw,96,m30,m50);
var preflightTexts=preflightOwner.Children.Select(c=>Recognize(c.Submission)).ToArray();using(var cts=new CancellationTokenSource())
{
 Guard(()=>preflightOwner.Publish(new CancelTexts(preflightTexts,cts),cts.Token));
 if(preflightOwner.IsFaulted||preflightOwner.CopyRowPrefix().Count!=0||preflightOwner.CopyTrace().Length!=0)throw new Exception("Pre-entry cancellation entered publication.");
 preflightOwner.Publish(preflightTexts);
}
Guard(()=>LegacyEnglish119ParentSubmission.Prepare(model,failureRaw,96,m30,m50,new(true)));
Guard(()=>LegacyEnglish119ParentSubmission.Prepare(model,Enumerable.Repeat(new InkStroke([new(0,0)]),257).ToArray(),96,m30,m50));
var emptyOwner=LegacyEnglish119ParentSubmission.Prepare(model,failureRaw,96,m30,m50);
var completedChild=Recognize(emptyOwner.Children[0].Submission);var emptyStorage=completedChild.Factory.CreateStorage(default);
var preparedEmpty=new LegacyEnglish119PlainTextResult(completedChild.Factory,new(emptyStorage,completedChild.Factory.Alternative.Choice,[],[],[],[],0),completedChild.AllocationStorageFingerprint,default);
Guard(()=>emptyOwner.Publish(new[]{preparedEmpty}));
if(emptyOwner.IsFaulted||emptyOwner.CopyRowPrefix().Count!=0)throw new Exception("Excluded zerochild mutated parent.");
var one=new[]{new InkStroke(Enumerable.Range(0,4096).Select(i=>new InkPoint(i,0)).ToArray())};
var exactRaw=LegacyEnglish119ParentSubmission.Prepare(model,one,0,m30,m50);
if(exactRaw.CopyRaw().Sum(s=>s.Points.Count)!=4096)throw new Exception("Exact raw aggregate cap not admitted.");
var range=new LegacyFirstResultSourceRange(new(0,0,0),new(0,0,4095));int rangesBudget=0,visitsBudget=0;
LegacyEnglish119ParentSubmission.PreflightParentRanges(Enumerable.Repeat(range,256).ToArray(),new[]{0},one,ref rangesBudget,ref visitsBudget,default);
if(visitsBudget!=1_048_576)throw new Exception("Exact total geometry cap not covered.");
Guard(()=>{int r=rangesBudget,v=visitsBudget;LegacyEnglish119ParentSubmission.PreflightParentRanges(new[]{range},new[]{0},one,ref r,ref v,default);});
var singleton=new[]{new InkStroke([new(0,0)])};var singletonRange=new LegacyFirstResultSourceRange(new(0,0,0),new(0,0,0));rangesBudget=0;visitsBudget=0;
for(int i=0;i<16;i++)LegacyEnglish119ParentSubmission.PreflightParentRanges(Enumerable.Repeat(singletonRange,4096).ToArray(),new[]{0},singleton,ref rangesBudget,ref visitsBudget,default);
if(rangesBudget!=65536)throw new Exception("Exact total range cap not covered.");
Guard(()=>{int r=rangesBudget,v=visitsBudget;LegacyEnglish119ParentSubmission.PreflightParentRanges(new[]{singletonRange},new[]{0},singleton,ref r,ref v,default);});
Guard(()=>LegacyEnglish119ParentSubmission.Prepare(model,[],96,m30,m50));
Guard(()=>LegacyEnglish119ParentSubmission.Prepare(model,one.Concat(new[]{singleton[0]}).ToArray(),96,m30,m50));
Guard(()=>LegacyEnglish119ParentSubmission.Prepare(model,failureRaw,float.NaN,m30,m50));
using var captureFile=File.Create(args[2]);using var writer=new Utf8JsonWriter(captureFile);Write(writer,D(("Actual",results),("Refused",refusals),("PreparedFailures",failures),("Guards",guards),("PreparedRangeCap",rangesBudget),("PreparedPointVisitCap",1048576),("PreparedRawCap",4096),("PreparedZeroChildRefusal",1),("PreparedMidPreflightCancellation",1)));

static Dictionary<string,object?> D(params (string Name,object? Value)[] values)=>values.ToDictionary(v=>v.Name,v=>v.Value);
static void Write(Utf8JsonWriter w,object? value)
{
 switch(value)
 {
 case null:w.WriteNullValue();break;
 case string s:w.WriteStringValue(s);break;
 case bool b:w.WriteBooleanValue(b);break;
 case int n:w.WriteNumberValue(n);break;
 case uint n:w.WriteNumberValue(n);break;
 case long n:w.WriteNumberValue(n);break;
 case ulong n:w.WriteNumberValue(n);break;
 case byte n:w.WriteNumberValue(n);break;
 case short n:w.WriteNumberValue(n);break;
 case ushort n:w.WriteNumberValue(n);break;
 case float n:w.WriteNumberValue(n);break;
 case double n:w.WriteNumberValue(n);break;
 case Dictionary<string,object?> d:w.WriteStartObject();foreach(var item in d){w.WritePropertyName(item.Key);Write(w,item.Value);}w.WriteEndObject();break;
 case System.Collections.IEnumerable list:w.WriteStartArray();foreach(var item in list)Write(w,item);w.WriteEndArray();break;
 default:throw new InvalidDataException("Explicit static JSON scalar required.");
 }
}

sealed class CancelTexts(IReadOnlyList<LegacyEnglish119PlainTextResult> values,CancellationTokenSource cts):IReadOnlyList<LegacyEnglish119PlainTextResult>
{
 public int Count=>values.Count;
 public LegacyEnglish119PlainTextResult this[int i]{get{cts.Cancel();return values[i];}}
 public IEnumerator<LegacyEnglish119PlainTextResult> GetEnumerator()=>values.GetEnumerator();
 System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()=>GetEnumerator();
}
