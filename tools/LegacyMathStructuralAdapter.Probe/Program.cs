using System.Text;
using System.Text.Json;
using System.Runtime.CompilerServices;
using InkRecognition;
using InkRecognition.Math;
using InkMath.Solver;
if(args.Length<5)throw new ArgumentException("pack rawMath2 officialORT capture manifests...");
using var runtime=new OrtRuntime(args[2]);using var stream=File.OpenRead(args[0]);using var bundle=Math2Mode0ColdBundle.Read(runtime,stream,File.ReadAllBytes(args[1]));
using var owner=Load();using var file=File.Create(args[3]);using var writer=new Utf8JsonWriter(file,new(){Indented=true});
var samples=new List<(string Id,InkStroke[] Ink,JsonElement Metadata)>();
foreach(string path in args.Skip(4))
{
 using var manifest=JsonDocument.Parse(File.ReadAllBytes(path));foreach(var sample in manifest.RootElement.GetProperty("samples").EnumerateArray())
 {
  string name=sample.GetProperty("strokes").GetString()!;if(Path.GetFileName(name)!=name)throw new Exception("Invalid member.");
  using var ink=JsonDocument.Parse(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(path)!,name)));
  var strokes=ink.RootElement.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
  samples.Add((sample.GetProperty("id").GetString()!,strokes,sample.Clone()));
 }
}
int guards=0;void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or InvalidDataException or NotSupportedException or OperationCanceledException or FormatException){guards++;return;}throw new Exception("Required refusal missing.");}
using var stop=new CancellationTokenSource();stop.Cancel();
writer.WriteStartObject();writer.WriteStartArray("actual");
foreach(var sample in samples)
{
 writer.WriteStartObject();writer.WriteString("id",sample.Id);writer.WriteStartArray("runs");
 foreach(int width in new[]{1,3,16})
 {
  using var prepared=Prepare(sample.Ink,width);var result=ExperimentalColdMathRecognizer.Detach(prepared.Source,prepared.Receipt,ExperimentalColdMathRecognizer.DefaultResultBudget,null,default);
  if(width==1)
  {var direct=owner.Recognize(sample.Ink);if(Project(direct)!=Project(result))throw new Exception("Public cold owner differs from actual cold first receipt.");}
  writer.WriteStartObject();writer.WriteNumber("width",width);writer.WritePropertyName("result");Write(writer,result);
  var record=prepared.Receipt.Copy.CopyRecord();writer.WriteStartArray("runMap");foreach(var r in record.Runs){writer.WriteStartArray();writer.WriteNumberValue(r.PrimaryIndex);writer.WriteNumberValue(r.EndSegmentExclusive);writer.WriteEndArray();}writer.WriteEndArray();
  var positions=prepared.Source.ReadSourcePositions();writer.WriteStartArray("positions");foreach(ulong v in positions.CopyPositions())writer.WriteNumberValue(v);writer.WriteEndArray();writer.WriteStartArray("ends");foreach(int v in positions.CopySeparators())writer.WriteNumberValue(v);writer.WriteEndArray();writer.WriteEndObject();
  if(result.Confidence is not null||result.Admission!=ExperimentalMathAdmission.Uncalibrated)throw new Exception("False calibrated admission.");
  var copy=result.Tokens[0].CopyUtf8();copy.AsSpan().Fill(0);if(result.Tokens[0].CopyUtf8().SequenceEqual(copy))throw new Exception("UTF8 alias.");
  prepared.Dispose();if(Project(result).Length<10)throw new Exception("Detached result vanished.");
 }writer.WriteEndArray();writer.WriteString("expectedPostInference",sample.Metadata.GetProperty("expected").GetString());writer.WriteEndObject();Console.WriteLine("Actual structural "+sample.Id);
}
writer.WriteEndArray();
var baseline=samples.Single(s=>s.Id=="procedural-definite-integral-1-10-x-squared-dx-equals").Ink;
string golden=Project(owner.Recognize(baseline));var events=new List<(ExperimentalMathStage Stage,int Index)>();
owner.RecognizeDiagnostic(baseline,ExperimentalColdMathRecognizer.DefaultResultBudget,(s,i)=>events.Add((s,i)),default);
writer.WriteStartArray("prefixes");
foreach(var selected in events)foreach(bool canceled in new[]{false,true})
{
 using var cancel=new CancellationTokenSource();var seen=new List<(ExperimentalMathStage Stage,int Index)>();
 Reject(()=>owner.RecognizeDiagnostic(baseline,ExperimentalColdMathRecognizer.DefaultResultBudget,(s,i)=>
 {seen.Add((s,i));if((s,i)==selected){if(canceled)cancel.Cancel();else throw new InvalidOperationException("Prepared adapter boundary fault.");}},cancel.Token));
 if(!seen.SequenceEqual(events.Take(seen.Count))||seen[^1]!=selected)throw new Exception("Adapter failure prefix changed.");
 if(Project(owner.Recognize(baseline))!=golden)throw new Exception("Failure corrupted model owner.");
 writer.WriteStartObject();writer.WriteString("stage",selected.Stage.ToString());writer.WriteNumber("index",selected.Index);writer.WriteBoolean("canceled",canceled);writer.WriteNumber("observations",seen.Count);writer.WriteEndObject();
}writer.WriteEndArray();
using(var p=Prepare(baseline,1))
{
 var first=ExperimentalColdMathRecognizer.Detach(p.Source,p.Receipt,ExperimentalColdMathRecognizer.DefaultResultBudget,null,default);
 Reject(()=>ExperimentalColdMathRecognizer.Detach(p.Source,p.Receipt,first.PayloadReservation-1,null,default));
 var exact=ExperimentalColdMathRecognizer.Detach(p.Source,p.Receipt,first.PayloadReservation,null,default);if(Project(first)!=Project(exact))throw new Exception("Exact result budget changed.");
 Reject(()=>ExperimentalColdMathRecognizer.Detach(p.Source,p.Receipt,ExperimentalColdMathRecognizer.DefaultResultBudget,null,stop.Token));
 using(var foreign=Prepare(baseline,1))Reject(()=>ExperimentalColdMathRecognizer.Detach(p.Source,foreign.Receipt,ExperimentalColdMathRecognizer.DefaultResultBudget,null,default));
 p.Receipt.Dispose();Reject(()=>ExperimentalColdMathRecognizer.Detach(p.Source,p.Receipt,ExperimentalColdMathRecognizer.DefaultResultBudget,null,default));
}
Reject(()=>owner.Recognize(baseline,0));Reject(()=>owner.Recognize(baseline,-1));Reject(()=>owner.Recognize(baseline,ExperimentalColdMathRecognizer.DefaultResultBudget+1));Reject(()=>owner.Recognize(baseline,token:stop.Token));
Reject(()=>owner.Recognize([]));Reject(()=>owner.Recognize([new InkStroke([])]));Reject(()=>owner.Recognize([new InkStroke([new(float.NaN,0)])]));Reject(()=>owner.Recognize([new InkStroke(Enumerable.Repeat(new InkPoint(0,0),4097).ToArray())]));
string[][] bad=[[],[""],["<sup>","2","</sup>"],["x","<sup>","</sup>"],["<frac>","1","<fracbar>","</frac>"],["<fence>","(","x","]","</fence>"],["<&>"],["</sup>"],["√","1"],["<unknown>","x","</unknown>"],["<fence>","x","</fence>"]];
writer.WriteStartArray("preparedSyntax");foreach(var tokens in bad)Syntax(tokens,false);
Syntax(["x","<sup>","2","</sup>"],true);Syntax(["<frac>","1","<fracbar>","2","</frac>"],true);Syntax(["<fence>","(","x",")","</fence>"],true);
Syntax(["<table>","1","<&>","2","<\\>","3","<&>","4","</table>"],true);
Syntax(new[]{"x"}.Concat(Enumerable.Range(0,33).SelectMany(_=>new[]{"<sup>","x"})).Concat(Enumerable.Repeat("</sup>",33)).ToArray(),false);
writer.WriteEndArray();
var preparedTokens=Tokens(["x"]);Reject(()=>ExperimentalMathSyntax.Parse(preparedTokens,1,default));Reject(()=>ExperimentalMathSyntax.Parse(preparedTokens,10,stop.Token));
var detached=Detached(false);var negative=Detached(true);Collect();if(detached.Source.IsAlive||!negative.Source.IsAlive)throw new Exception("Detached/source-backed GC control failed.");GC.KeepAlive(negative.Keep);
if(Project(detached.Result).Length<10)throw new Exception("GC changed detached result.");writer.WriteBoolean("detachedSourceCollected",true);writer.WriteBoolean("sourceBackedRetained",true);
using(var race=Load())
{
 using var entered=new ManualResetEventSlim();using var release=new ManualResetEventSlim();using var disposeEntered=new ManualResetEventSlim();
 var work=Task.Run(()=>race.RecognizeDiagnostic(baseline,ExperimentalColdMathRecognizer.DefaultResultBudget,(s,i)=>{if(s==ExperimentalMathStage.BeforeInference){entered.Set();release.Wait();}},default));
 if(!entered.Wait(10000))throw new Exception("No active adapter request.");
 var closing=Task.Run(()=>{disposeEntered.Set();race.Dispose();});
 try{if(!disposeEntered.Wait(10000))throw new Exception("No concurrent close request.");Reject(()=>race.Recognize(baseline,token:stop.Token));if(closing.IsCompleted)throw new Exception("Tensor owner disposed in-flight.");}
 finally{release.Set();}
 if(!Task.WaitAll([work,closing],30000))throw new Exception("Adapter close race stuck.");
 if(Project(work.Result)!=golden)throw new Exception("Close race altered output.");race.Dispose();Reject(()=>race.Recognize(baseline));
}
var parallel=Task.WhenAll(Task.Run(()=>owner.Recognize(baseline)),Task.Run(()=>owner.Recognize(baseline))).GetAwaiter().GetResult();if(parallel.Any(r=>Project(r)!=golden))throw new Exception("Concurrent requests interleaved.");
writer.WriteBoolean("disposeRace",true);writer.WriteNumber("serializedConcurrentRequests",2);writer.WriteNumber("guards",guards);writer.WriteEndObject();writer.Flush();
Console.WriteLine($"PASS{samples.Count} inputs/{samples.Count*3} actual first-record ASTs/publiccold{samples.Count}/{events.Count*2} boundary prefixes/{guards} guards/GC/close race.");

ExperimentalColdMathRecognizer Load(){using var p=File.OpenRead(args[0]);return ExperimentalColdMathRecognizer.Load(p,File.ReadAllBytes(args[1]),args[2]);}
Prepared Prepare(InkStroke[] ink,int width)
{
 var d=Math2Mode0RawSourcePositions.PrepareFeatures(ink,default).Positions.Document;var view=LegacyFreshRawInkCurrentView.Prepare(d);var field=Math2Mode0OriginalMathFieldSource.Prepare(view,"formula");var run=runtime.CreateRun(default);
 Math2Mode0RawOnlySource? source=null;Math2Mode0SelectedMathFieldCopy? receipt=null;
 try{source=width==1?Math2Mode0RawOnlySource.PrepareColdWithSelectedMathField(runtime,bundle,run,field):Math2Mode0RawOnlySource.PrepareWithPreparedSearchAndSelectedMathField(runtime,bundle,run,field,width);source.RunToInitialOrder();source.ApplyEf4();var output=source.OrderFinal().FormatFullCold().AttachSourcePositions();receipt=output.CopySelectedMathField(field);return new(source,field,receipt,run,output);}
 catch{receipt?.Dispose();source?.Dispose();field.Dispose();run.Dispose();throw;}
}
[MethodImpl(MethodImplOptions.NoInlining)]
Gc Detached(bool keep){var p=Prepare(baseline,1);var result=ExperimentalColdMathRecognizer.Detach(p.Source,p.Receipt,ExperimentalColdMathRecognizer.DefaultResultBudget,null,default);var weak=new WeakReference(p.Source);object? retained=keep?p.Output:null;p.Dispose();return new(result,weak,retained);}
void Syntax(string[] texts,bool valid)
{
 var t=Tokens(texts);ExperimentalMathSyntaxNode[]? nodes=null;
 if(valid)nodes=ExperimentalMathSyntax.Parse(t,texts.Length*2+1,default);else Reject(()=>ExperimentalMathSyntax.Parse(t,texts.Length*2+1,default));
 writer.WriteStartObject();writer.WriteStartArray("tokens");foreach(string text in texts)writer.WriteStringValue(text);writer.WriteEndArray();writer.WriteBoolean("valid",valid);if(nodes is not null)WriteNodes(writer,nodes);writer.WriteEndObject();
}
static ExperimentalMathToken[] Tokens(string[] texts)=>texts.Select((s,i)=>new ExperimentalMathToken((uint)i,Encoding.UTF8.GetBytes(s+'\0'),s,[])).ToArray();
static void Collect(){GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();}
static string Project(ExperimentalMathResult result){using var m=new MemoryStream();using(var w=new Utf8JsonWriter(m)){Write(w,result);w.Flush();}return Encoding.UTF8.GetString(m.ToArray());}
static void Write(Utf8JsonWriter w,ExperimentalMathResult r)
{
 w.WriteStartObject();w.WriteString("policy",r.Policy);w.WriteNumber("scoreBits",r.ScoreBits);w.WriteNumber("frames",r.Frames);w.WriteNumber("reservation",r.PayloadReservation);w.WriteNull("confidence");w.WriteString("admission",r.Admission.ToString());w.WriteString("latex",r.LaTex);w.WriteString("unsupported",r.UnsupportedReason);w.WriteBoolean("request",r.HasCalculationRequest);
 w.WriteString("explicitCalculation",ExperimentalMathCalculation.Calculate(r,(s,t)=>PolynomialIntegralSolver.Solve(s,t)?.LaTex));
 w.WriteStartArray("tokens");foreach(var t in r.Tokens){w.WriteStartObject();w.WriteNumber("unit",t.Unit);w.WriteString("utf8",Convert.ToBase64String(t.CopyUtf8()));w.WriteString("text",t.Text);w.WriteStartArray("ranges");foreach(var range in t.Ranges){w.WriteStartArray();w.WriteNumberValue(range.First);w.WriteNumberValue(range.Last);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();}w.WriteEndArray();WriteNodes(w,r.Syntax);w.WriteEndObject();
}
static void WriteNodes(Utf8JsonWriter w,IReadOnlyList<ExperimentalMathSyntaxNode> nodes)
{w.WriteStartArray("nodes");foreach(var n in nodes){w.WriteStartObject();w.WriteNumber("id",n.Id);w.WriteString("kind",n.Kind);w.WriteNumber("first",n.FirstToken);w.WriteNumber("end",n.EndTokenExclusive);w.WriteStartArray("children");foreach(int child in n.Children)w.WriteNumberValue(child);w.WriteEndArray();w.WriteEndObject();}w.WriteEndArray();}
sealed class Prepared(Math2Mode0RawOnlySource source,Math2Mode0OriginalMathFieldSource field,Math2Mode0SelectedMathFieldCopy receipt,OrtRun run,Math2Mode0SourceBoundOutput output):IDisposable
{internal Math2Mode0RawOnlySource Source=source;internal Math2Mode0SelectedMathFieldCopy Receipt=receipt;internal Math2Mode0SourceBoundOutput Output=output;public void Dispose(){Receipt.Dispose();Source.Dispose();field.Dispose();run.Dispose();}}
record Gc(ExperimentalMathResult Result,WeakReference Source,object? Keep);
