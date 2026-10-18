using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
using InkRecognition.Math;

if(args.Length!=4)throw new ArgumentException("pack rawMath2.res official-runtime capture.json");
using var runtime=new OrtRuntime(Path.GetFullPath(args[2]));using var run=runtime.CreateRun(default);
using var stream=File.OpenRead(args[0]);using var bundle=Math2Mode0ColdBundle.Read(runtime,stream,File.ReadAllBytes(args[1]));
int guards=0;
void Reject(Action action)
{try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Required outer-owner refusal missing.");}
using var canceled=new CancellationTokenSource();canceled.Cancel();
var actual=new List<Actual>();int aliases=0;long exactBudget=0;
foreach(var(width,repeated)in new[]{(1,false),(3,false),(16,false),(1,true)})
{
 var prepared=Prepare(runtime,run,bundle,width,repeated);using var input=prepared.Copy;
 Collect();if(prepared.Decoder.IsAlive)throw new InvalidDataException("First copy retained the disposed decoder.");
 var before=ProjectInput(input.CopyRecord());
 Reject(()=>Math2Mode0OuterSymbolVector.CreateSingle(null!));
 Reject(()=>Math2Mode0OuterSymbolVector.CreateSingleDiagnostic(input,-1,null));
 Reject(()=>Math2Mode0OuterSymbolVector.CreateSingleDiagnostic(input,Math2Mode0OuterSymbolVector.DefaultPayloadBudget+1,null));
 Reject(()=>Math2Mode0OuterSymbolVector.CreateSingleDiagnostic(input,0,null));
 Reject(()=>Math2Mode0OuterSymbolVector.CreateSingle(input,canceled.Token));
 using var outer=width==1&&!repeated
  ?Math2Mode0OuterSymbolVector.CreateSingleDiagnostic(input,Math2Mode0OuterSymbolVector.DefaultPayloadBudget,(stage,index,target)=>
   {if(stage==Math2Mode0OuterCopyStage.AfterPool){Reject(()=>target.CopySymbols());Reject(()=>target.ValidateDocument(input.Document));}})
  :Math2Mode0OuterSymbolVector.CreateSingle(input);
 outer.ValidateDocument(input.Document);
 if(!ReferenceEquals(input.Document,outer.Document))throw new InvalidDataException("Outer range domain changed.");
 var copied=outer.CopySymbols();var after=Project(copied);
 for(int i=0;i<copied.Length;i++)
 {
  if(ReferenceEquals(copied[i].Alternatives[0].Text,input.CopyRecord().UnitRows[i].Text))throw new InvalidDataException("Outer interval aliases first-copy pool.");
  for(int j=0;j<i;j++)if(copied[i].Alternatives[0].Text.ReadBytes().SequenceEqual(copied[j].Alternatives[0].Text.ReadBytes()))
  {if(!ReferenceEquals(copied[i].Alternatives[0].Text,copied[j].Alternatives[0].Text))throw new InvalidDataException("Outer interval interning lost identity.");aliases++;}
 }
 var stats=outer.CopyStatistics();if(width==1&&!repeated)exactBudget=stats.PayloadReservation;
 copied[0].Alternatives.AsSpan().Fill(new(copied[0].Alternatives[0].Text,1234,5678));
 foreach(var row in copied)row.Ranges.AsSpan().Fill(default);
 if(!Same(after,Project(outer.CopySymbols())))throw new InvalidDataException("Outer inspection aliases storage.");
 //Creating another fresh destination is a read-only copy, not a reset or
 //second decoder run. It must have independent pool identities.
 using(var second=Math2Mode0OuterSymbolVector.CreateSingle(input))
 {
  if(!Same(after,Project(second.CopySymbols()))||ReferenceEquals(second.CopySymbols()[0].Alternatives[0].Text,outer.CopySymbols()[0].Alternatives[0].Text))
   throw new InvalidDataException("Fresh outer destinations share mutable pool ownership.");
 }
 var foreign=Prepare(runtime,run,bundle,1,false);using(foreign.Copy)
 {
  Reject(()=>outer.ValidateDocument(foreign.Copy.Document));
  Reject(()=>outer.ValidateDocument(input.Document,canceled.Token));
 }
 input.Dispose();Reject(()=>input.CopyRecord());Reject(()=>Math2Mode0OuterSymbolVector.CreateSingle(input));
 Collect();if(!Same(after,Project(outer.CopySymbols())))throw new InvalidDataException("First-copy disposal invalidated outer rows.");
 outer.ValidateDocument(outer.Document);
 var texts=outer.CopySymbols().SelectMany(r=>r.Alternatives).Select(a=>a.Text).ToArray();
 outer.Dispose();outer.Dispose();Reject(()=>outer.CopySymbols());Reject(()=>outer.ValidateDocument(outer.Document));
 foreach(var text in texts)Reject(()=>text.ReadBytes());
 if(outer.CopyStatistics().ReleaseCount!=1)throw new InvalidDataException("Outer pool release is not exact once.");
 actual.Add(new(width,repeated,prepared.Decoder.IsAlive,before,after,input.CopyStatistics().PayloadReservation,stats));
}
var budgetSource=Prepare(runtime,run,bundle,1,false);using(budgetSource.Copy)
{
 Reject(()=>Math2Mode0OuterSymbolVector.CreateSingleDiagnostic(budgetSource.Copy,exactBudget-1,null));
 using var exact=Math2Mode0OuterSymbolVector.CreateSingleDiagnostic(budgetSource.Copy,exactBudget,null);
 if(exact.CopyStatistics().PayloadReservation!=exactBudget)throw new InvalidDataException("Exact outer budget differs.");
}
var failureSource=Prepare(runtime,run,bundle,3,false);using var failureInput=failureSource.Copy;
var unchangedInput=ProjectInput(failureInput.CopyRecord());var failures=new List<Failure>();
foreach(var stage in Enum.GetValues<Math2Mode0OuterCopyStage>())foreach(bool cancel in new[]{false,true})
{
 Math2Mode0OuterSymbolVector? retained=null;Math2Mode0OuterCopyStatistics? prefix=null;using var stop=new CancellationTokenSource();
 bool later=stage is Math2Mode0OuterCopyStage.AfterRowInitialized or Math2Mode0OuterCopyStage.AfterMetadata or Math2Mode0OuterCopyStage.AfterAlternativeReserve or
  Math2Mode0OuterCopyStage.AfterIntern or Math2Mode0OuterCopyStage.AfterAlternative or Math2Mode0OuterCopyStage.AfterRanges or Math2Mode0OuterCopyStage.AfterRow;
 try
 {
  Math2Mode0OuterSymbolVector.CreateSingleDiagnostic(failureInput,Math2Mode0OuterSymbolVector.DefaultPayloadBudget,(current,index,target)=>
  {retained=target;if(current!=stage||later&&index!=1)return;prefix=target.CopyStatistics();if(cancel)stop.Cancel();else throw new IOException("Prepared outer-copy failure.");},stop.Token);
  throw new InvalidDataException("Required outer-copy fault not reached.");
 }
 catch(Exception e)when(e is IOException or OperationCanceledException){}
 if(retained is null||prefix is null||!retained.CopyStatistics().Failed)throw new InvalidDataException("Outer failure lost its logical prefix.");
 Reject(()=>retained.CopySymbols());Reject(()=>retained.ValidateDocument(failureInput.Document));
 if(!SameInput(unchangedInput,ProjectInput(failureInput.CopyRecord())))throw new InvalidDataException("Outer failure mutated its first-copy input.");
 failures.Add(new(stage.ToString(),cancel,prefix));retained.Dispose();retained.Dispose();
 if(retained.CopyStatistics().ReleaseCount!=(stage==Math2Mode0OuterCopyStage.Claimed?0:1))throw new InvalidDataException("Failure released pool incorrectly.");
}
//Disposal performed by callbacks must be detected before another payload write.
foreach(bool disposeSource in new[]{false,true})
{
 var source=Prepare(runtime,run,bundle,3,false);using(source.Copy)
 {
  Math2Mode0OuterSymbolVector? seen=null;
  Reject(()=>Math2Mode0OuterSymbolVector.CreateSingleDiagnostic(source.Copy,Math2Mode0OuterSymbolVector.DefaultPayloadBudget,(stage,index,target)=>
  {seen=target;if(stage!=Math2Mode0OuterCopyStage.AfterAlternative||index!=0)return;if(disposeSource)source.Copy.Dispose();else target.Dispose();}));
  if(seen is null||!seen.CopyStatistics().Failed)throw new InvalidDataException("Disposed callback did not poison the new target.");
  seen.Dispose();if(seen.CopyStatistics().ReleaseCount!=1)throw new InvalidDataException("Disposed target pool leaked.");
 }
}
//A live but incomplete/failed first-copy token is not an input vector.
using(var source=PrepareRaw(runtime,run,bundle,1,false))
{
 var attached=source.OrderFinal().FormatFullCold().AttachSourcePositions();Math2Mode0FirstResultCopy? failed=null;
 try
 {
  Math2Mode0FirstResultCopy.CreateDiagnostic(attached,Math2Mode0FirstResultCopy.DefaultPayloadBudget,(stage,index,target)=>
  {failed=target;if(stage!=Math2Mode0FirstCopyStage.AfterPool)return;Reject(()=>Math2Mode0OuterSymbolVector.CreateSingle(target));throw new IOException("Prepared failed first copy.");});
 }
 catch(IOException){}
 if(failed is null)throw new InvalidDataException("Missing failed source.");
 Reject(()=>Math2Mode0OuterSymbolVector.CreateSingle(failed));failed.Dispose();
}
var positive=Detached(runtime,run,bundle,false);var negative=Detached(runtime,run,bundle,true);Collect();
if(positive.Input.IsAlive||!negative.Input.IsAlive)throw new InvalidDataException("First-copy GC controls did not discriminate.");
if(!Same(positive.Before,Project(positive.Outer.CopySymbols())))throw new InvalidDataException("GC changed independent outer payload.");
var gc=new GcEvidence(!positive.Input.IsAlive,negative.Input.IsAlive,positive.Before);
positive.Outer.Dispose();negative.Outer.Dispose();GC.KeepAlive(negative.Keep);
File.WriteAllText(args[3],JsonSerializer.Serialize(new Capture(Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(args[1]))),actual.ToArray(),failures.ToArray(),gc,guards,aliases),CaptureJson.Default.Capture));
Console.WriteLine($"PASS outer copy {actual.Count} actual sources/{aliases} aliases/{failures.Count} whole-prefix faults/{guards} guards; new library/source ranges/GC; no multi-C23/DOM");

static Math2Mode0RawOnlySource PrepareRaw(OrtRuntime runtime,OrtRun run,Math2Mode0ColdBundle bundle,int width,bool repeated)
{
 InkStroke[] ink=[new([new InkPoint(0,30),new(0,0),new(10,18),new(20,0),new(20,30)]),new([new InkPoint(35,0),new(35,30)])];
 if(repeated)ink=[new([new InkPoint(0,30),new(0,0),new(10,18),new(20,0),new(20,30)]),new([new InkPoint(35,30),new(35,0),new(45,18),new(55,0),new(55,30)])];
 if(width==3)ink=ink.Reverse().ToArray();
 var source=width==1?Math2Mode0RawOnlySource.PrepareColdWithSourcePositions(runtime,bundle,run,ink):Math2Mode0RawOnlySource.PrepareWithPreparedSearchAndSourcePositions(runtime,bundle,run,ink,width);
 try{source.RunToInitialOrder();source.ApplyEf4();return source;}catch{source.Dispose();throw;}
}
[MethodImpl(MethodImplOptions.NoInlining)]
static Prepared Prepare(OrtRuntime runtime,OrtRun run,Math2Mode0ColdBundle bundle,int width,bool repeated)
{
 using var source=PrepareRaw(runtime,run,bundle,width,repeated);
 var attached=source.OrderFinal().FormatFullCold().AttachSourcePositions();
 return new(Math2Mode0FirstResultCopy.Create(attached),new(source));
}
[MethodImpl(MethodImplOptions.NoInlining)]
static DetachedOwner Detached(OrtRuntime runtime,OrtRun run,Math2Mode0ColdBundle bundle,bool keep)
{
 using var input=Prepare(runtime,run,bundle,3,false).Copy;var outer=Math2Mode0OuterSymbolVector.CreateSingle(input);
 return new(outer,new(input),keep?input:null,Project(outer.CopySymbols()));
}
static void Collect(){GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();}
static Symbol[] Project(Math2Mode0OuterSymbol[] rows)=>rows.Select(r=>new Symbol(r.Flags,r.Metadata,
 r.Alternatives.Select(a=>new Alternative(a.Text.Identity,a.ByteCount,a.ScalarBits,Convert.ToBase64String(a.Text.ReadBytes()))).ToArray(),
 r.Ranges.Select(p=>new Range(p.First.Value,p.Last.Value)).ToArray())).ToArray();
static InputRow ProjectInput(Math2Mode0FirstCopyRecord row)=>new(row.UnitRows.Select(r=>new InputSymbol(r.Unit,r.Flags,r.Metadata,
 new Alternative(r.Text.Identity,r.ByteCount,r.ScalarBits,Convert.ToBase64String(r.Text.ReadBytes())))).ToArray(),
 row.SourceRanges.Select(r=>r.Select(p=>new Range(p.First.Value,p.Last.Value)).ToArray()).ToArray());
static bool Same(Symbol[] a,Symbol[] b)=>JsonSerializer.Serialize(a,CaptureJson.Default.SymbolArray)==JsonSerializer.Serialize(b,CaptureJson.Default.SymbolArray);
static bool SameInput(InputRow a,InputRow b)=>JsonSerializer.Serialize(a,CaptureJson.Default.InputRow)==JsonSerializer.Serialize(b,CaptureJson.Default.InputRow);
record Range(ulong First,ulong Last);
record Alternative(int Identity,int ByteCount,uint ScalarBits,string Bytes);
record Symbol(byte Flags,uint Metadata,Alternative[] Alternatives,Range[] Ranges);
record InputSymbol(uint Unit,byte Flags,uint Metadata,Alternative Alternative);
record InputRow(InputSymbol[] Rows,Range[][] Ranges);
record Actual(int Width,bool Repeated,bool DecoderRetained,InputRow Before,Symbol[] After,long FirstCopyReservation,Math2Mode0OuterCopyStatistics Statistics);
record Failure(string Stage,bool Canceled,Math2Mode0OuterCopyStatistics Prefix);
record GcEvidence(bool AllowsFirstCopyCollection,bool ExplicitFirstCopyKeepAlive,Symbol[] Rows);
record Capture(string Resource,Actual[] Actual,Failure[] Failures,GcEvidence Gc,int Guards,int Aliases);
record Prepared(Math2Mode0FirstResultCopy Copy,WeakReference Decoder);
record DetachedOwner(Math2Mode0OuterSymbolVector Outer,WeakReference Input,object? Keep,Symbol[] Before);
[JsonSerializable(typeof(Capture))][JsonSerializable(typeof(Symbol[]))][JsonSerializable(typeof(InputRow))]
partial class CaptureJson:JsonSerializerContext;
