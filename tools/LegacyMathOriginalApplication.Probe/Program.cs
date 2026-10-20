using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;
using InkRecognition;
using InkRecognition.Math;
if(args.Length!=4)throw new ArgumentException("pack res official-runtime capture");
using var runtime=new OrtRuntime(Path.GetFullPath(args[2]));using var run=runtime.CreateRun(default);
using var stream=File.OpenRead(args[0]);using var bundle=Math2Mode0ColdBundle.Read(runtime,stream,File.ReadAllBytes(args[1]));
int guards=0;void Reject(Action call){try{call();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Missing original Math guard.");}
using var canceled=new CancellationTokenSource();canceled.Cancel();var actual=new List<Actual>();var events=new List<Step>();
foreach(int width in new[]{1,3,16})foreach(bool reverse in new[]{false,true})
{
 using var p=Prepare(runtime,run,bundle,width,reverse);using var pub=Math2Mode0FreshMathPublisher.Create(p.Receipt);using var outgoing=Math2Mode0OutgoingTransfer.Create(pub);
 var historical=p.Field.CopySnapshot();var publication=pub.CopyResult();var transfer=outgoing.CopyResult();
 Reject(()=>Math2Mode0OriginalApplication.Prepare(p.Field,outgoing,canceled.Token));Reject(()=>Math2Mode0OriginalApplication.PrepareDiagnostic(p.Field,outgoing,0,null));
 Reject(()=>p.Field.WithOriginalApplicationInput(outgoing,p.Field.OriginalInkIdentity,_=>0,default));
 void Observe(string flow,Math2Mode0OriginalStage stage,int index,Math2Mode0OriginalApplication target){if(width==1&&!reverse)events.Add(new(flow,stage.ToString(),index));}
 using var application=Math2Mode0OriginalApplication.PrepareDiagnostic(p.Field,outgoing,Math2Mode0OriginalApplication.DefaultPayloadBudget,(s,i,t)=>Observe("Prepare",s,i,t));
 var prepared=application.CopySnapshot();Reject(()=>application.ApplyDom());Reject(()=>application.RollbackSource());
 application.RunSourceDiagnostic((s,i,t)=>Observe("Source",s,i,t));var source=application.CopySnapshot();
 if(!SameField(historical,p.Field.CopySnapshot())||!SamePub(publication,pub.CopyResult())||!SameTransfer(transfer,outgoing.CopyResult()))throw new InvalidDataException("Historical/selected/third/outgoing changed during original source application.");
 application.ApplyDomDiagnostic((s,i,t)=>Observe("Dom",s,i,t));var final=application.CopyResult();
 if(!SameField(historical,p.Field.CopySnapshot()))throw new InvalidDataException("Old historical Math field snapshot changed.");
 Reject(()=>application.RollbackSource());Reject(()=>application.RunSource());Reject(()=>application.ApplyDom());Reject(()=>Math2Mode0OriginalApplication.Prepare(p.Field,outgoing));
 var detached=application.CopyResult();detached.ActivePage.AsSpan().Fill(null!);detached.ActiveRegistry[0].Ranges.AsSpan().Fill(default);
 if(application.CopyResult().ActivePage[0]==null||application.CopyResult().ActiveRegistry[0].Ranges[0]==default)throw new InvalidDataException("Original inspection aliases actual state.");
 actual.Add(new(width==1?"ColdDefault":"PreparedSearch",width,reverse,p.Evidence,publication,transfer,prepared,source,final));
 outgoing.Dispose();Reject(()=>application.CopyResult());if(!SameOriginal(final,application.CopySnapshot()))throw new InvalidDataException("Borrowed closure changed owned original snapshots.");
 application.Dispose();application.Dispose();if(application.CopySnapshot().CleanupCount!=1)throw new InvalidDataException("Double application cleanup.");p.Field.CopySnapshot();
}
var failures=new List<Failure>();
foreach(var step in events)foreach(bool cancel in new[]{false,true})
{
 using var p=Prepare(runtime,run,bundle,1,false);using var pub=Math2Mode0FreshMathPublisher.Create(p.Receipt);using var outgoing=Math2Mode0OutgoingTransfer.Create(pub);using var stop=new CancellationTokenSource();
 Math2Mode0OriginalApplication? retained=null;Math2Mode0OriginalSnapshot? before=null;
 void Observe(Math2Mode0OriginalStage stage,int index,Math2Mode0OriginalApplication target){if(stage.ToString()!=step.Name||index!=step.Index)return;retained=target;before=target.CopySnapshot();if(cancel)stop.Cancel();else throw new IOException("Prepared original stage.");}
 try
 {
  var application=Math2Mode0OriginalApplication.PrepareDiagnostic(p.Field,outgoing,Math2Mode0OriginalApplication.DefaultPayloadBudget,step.Flow=="Prepare"?Observe:null,stop.Token);retained=application;
  if(step.Flow=="Source")application.RunSourceDiagnostic(Observe,token:stop.Token);
  if(step.Flow=="Dom"){application.RunSource();application.ApplyDomDiagnostic(Observe,token:stop.Token);}
  throw new InvalidDataException("Missing original prefix.");
 }
 catch(Exception e)when(e is IOException or OperationCanceledException){}
 if(retained is null||before is null||!retained.CopySnapshot().Failed)throw new InvalidDataException("Lost original failed prefix.");
 failures.Add(new(step.Flow,step.Name,step.Index,cancel,before,retained.CopySnapshot()));Reject(()=>retained.CopyResult());Reject(()=>Math2Mode0OriginalApplication.Prepare(p.Field,outgoing));retained.Dispose();
 if(retained.CopySnapshot().CleanupCount!=1)throw new InvalidDataException();
}
var unsafeSources=new List<UnsafeSource>();
foreach(bool after in new[]{false,true})foreach(int at in new[]{0,1,2})
{
 using var p=Prepare(runtime,run,bundle,1,false);using var pub=Math2Mode0FreshMathPublisher.Create(p.Receipt);using var outgoing=Math2Mode0OutgoingTransfer.Create(pub);using var application=Math2Mode0OriginalApplication.Prepare(p.Field,outgoing);
 application.RunSourceDiagnostic(null,after?-1:at,after?at:-1);var state=application.CopySnapshot();
 if(!state.SourceExecutionSucceeded||state.JournalComplete||state.DomEntered||state.Failed)throw new InvalidDataException("Observer refusal is distinct from source refusal.");
 Reject(()=>application.ApplyDom());Reject(()=>application.RollbackSource());unsafeSources.Add(new(after,at,state));
}
Math2Mode0OriginalSnapshot inverse;var inverseEvents=new List<Step>();
using(var p=Prepare(runtime,run,bundle,1,false))
{
 using var pub=Math2Mode0FreshMathPublisher.Create(p.Receipt);using var outgoing=Math2Mode0OutgoingTransfer.Create(pub);using var application=Math2Mode0OriginalApplication.Prepare(p.Field,outgoing);application.RunSource();long finalNext=application.CopySnapshot().ActiveNext;
 application.RollbackSourceDiagnostic((s,i,t)=>inverseEvents.Add(new("Inverse",s.ToString(),i)));inverse=application.CopySnapshot();
 if(inverse.ActiveRegistry.Length!=1||inverse.ActiveNext!=finalNext||!inverse.RolledBack||inverse.DomEntered)throw new InvalidDataException("Source inverse must preserve monotonic Next.");Reject(()=>application.ApplyDom());Reject(()=>application.RollbackSource());
}
var inverseFailures=new List<Failure>();
foreach(var step in inverseEvents)foreach(bool cancel in new[]{false,true})
{
 using var p=Prepare(runtime,run,bundle,1,false);using var pub=Math2Mode0FreshMathPublisher.Create(p.Receipt);using var outgoing=Math2Mode0OutgoingTransfer.Create(pub);using var application=Math2Mode0OriginalApplication.Prepare(p.Field,outgoing);application.RunSource();using var stop=new CancellationTokenSource();Math2Mode0OriginalSnapshot? before=null;
 try{application.RollbackSourceDiagnostic((s,i,t)=>{if(s.ToString()!=step.Name||i!=step.Index)return;before=t.CopySnapshot();if(cancel)stop.Cancel();else throw new IOException("Prepared inverse prefix.");},stop.Token);throw new InvalidDataException();}catch(Exception e)when(e is IOException or OperationCanceledException){}
 if(before is null)throw new InvalidDataException();inverseFailures.Add(new("Inverse",step.Name,step.Index,cancel,before,application.CopySnapshot()));Reject(()=>application.RollbackSource());
}
Math2Mode0OriginalSnapshot postcheck;
using(var p=Prepare(runtime,run,bundle,1,false))
{
 using var pub=Math2Mode0FreshMathPublisher.Create(p.Receipt);using var outgoing=Math2Mode0OutgoingTransfer.Create(pub);using var application=Math2Mode0OriginalApplication.Prepare(p.Field,outgoing);application.RunSource();Reject(()=>application.ApplyDomDiagnostic(null,true));postcheck=application.CopySnapshot();
 if(!postcheck.DomEntered||!postcheck.Failed||!postcheck.ActivePage.Any(n=>n.Name=="mathResult"))throw new InvalidDataException("Postcheck failure lost live mutation prefix.");Reject(()=>application.RollbackSource());
}
using(var p=Prepare(runtime,run,bundle,1,false))
{
 using var pub=Math2Mode0FreshMathPublisher.Create(p.Receipt);using var outgoing=Math2Mode0OutgoingTransfer.Create(pub);using var foreign=Math2Mode0OriginalMathFieldSource.Prepare(p.View,"foreign");
 if(!ReferenceEquals(p.Field.Document,foreign.Document))throw new InvalidDataException();Reject(()=>Math2Mode0OriginalApplication.Prepare(foreign,outgoing));
 Reject(()=>Math2Mode0OriginalApplication.Prepare(null!,outgoing));Reject(()=>Math2Mode0OriginalApplication.Prepare(p.Field,null!));Reject(()=>Math2Mode0OriginalApplication.PrepareDiagnostic(p.Field,outgoing,-1,null));
 using var application=Math2Mode0OriginalApplication.Prepare(p.Field,outgoing);long budget=application.CopySnapshot().Reservation;
 outgoing.WithOriginalApplicationInput(p.Field,(input,transfer)=>
 {
  var command=transfer.Commands.First(c=>c.Operation==4);var clone=input.CloneSelection(command.Ranges);Reject(()=>input.Register(command with{},clone));
  void Plan(Math2Mode0OriginalInput original,Math2Mode0OriginalTransferInput value)=>Math2Mode0OriginalKernel.Plan(original,value,Math2Mode0OriginalApplication.DefaultPayloadBudget,default);
  Reject(()=>Plan(input,transfer with{Path=[0]}));Reject(()=>Plan(input,transfer with{Nodes=transfer.Nodes.Select(n=>n.Id==transfer.PayloadRoot?n with{Parent=n.Id}:n).ToArray()}));
  Reject(()=>Plan(input with{InitialNext=2},transfer));Reject(()=>Plan(input with{InputC22=new object()},transfer));
  var arena=new Math2Mode0OriginalArena(input.Names);arena.Import(input.HistoricalNodes);int field=arena.Nodes.Single(n=>n.Name.Text=="mathField").Id;
  var tag=arena.Nodes.Single(n=>n.Name.Text=="tag");var actualRegistry=input.ReadRegistry();
  tag.Tag=7;Reject(()=>Math2Mode0OriginalKernel.ValidateTags(arena,field,actualRegistry,input.InputRanges,default));tag.Tag=4;
  tag.Number=null;Reject(()=>Math2Mode0OriginalKernel.ValidateTags(arena,field,actualRegistry,input.InputRanges,default));tag.Number=999;
  Reject(()=>Math2Mode0OriginalKernel.ValidateTags(arena,field,actualRegistry,input.InputRanges,default));tag.Number=input.InputId;
  var outside=new LegacyNativeInkPositionRange(LegacyNativeInkPosition.Pack(123,0,3,1),LegacyNativeInkPosition.Pack(123,0,5,1));
  Reject(()=>Math2Mode0OriginalKernel.ValidateTags(arena,field,actualRegistry.Select(r=>r with{Ranges=[outside]}).ToArray(),input.InputRanges,default));
  if(input.ReadRegistry().Length!=1)throw new InvalidDataException("Prepared controls mutated original registry.");return 0;
 });
 using var q=Prepare(runtime,run,bundle,1,false);using var qpub=Math2Mode0FreshMathPublisher.Create(q.Receipt);using var qout=Math2Mode0OutgoingTransfer.Create(qpub);
 Reject(()=>Math2Mode0OriginalApplication.PrepareDiagnostic(q.Field,qout,budget-1,null));using var exact=Math2Mode0OriginalApplication.PrepareDiagnostic(q.Field,qout,budget,null);if(exact.CopySnapshot().Reservation!=budget)throw new InvalidDataException();
 Reject(()=>application.RunSource(canceled.Token));application.RunSource();Reject(()=>application.ApplyDom(canceled.Token));application.ApplyDom();
}
int completed=0,refused=0;
using(var p=Prepare(runtime,run,bundle,1,false))
{
 using var pub=Math2Mode0FreshMathPublisher.Create(p.Receipt);using var outgoing=Math2Mode0OutgoingTransfer.Create(pub);
 Parallel.For(0,2,_=>{try{using var application=Math2Mode0OriginalApplication.Prepare(p.Field,outgoing);Interlocked.Increment(ref completed);}catch(InvalidOperationException){Interlocked.Increment(ref refused);}});
 if(completed!=1||refused!=1)throw new InvalidDataException("Concurrent original claim.");
}
foreach(int close in new[]{0,1,2})
{
 using var p=Prepare(runtime,run,bundle,1,false);using var pub=Math2Mode0FreshMathPublisher.Create(p.Receipt);using var outgoing=Math2Mode0OutgoingTransfer.Create(pub);using var application=Math2Mode0OriginalApplication.Prepare(p.Field,outgoing);
 Reject(()=>application.RunSourceDiagnostic((s,i,t)=>{if(s==Math2Mode0OriginalStage.Registered&&i==0){if(close==0)outgoing.Dispose();else if(close==1)p.Field.Dispose();else p.Receipt.Dispose();}}));
 var state=application.CopySnapshot();if(!state.Failed||state.ActiveRegistry.Length!=2||state.DomEntered)throw new InvalidDataException("Closure crossed next original mutation boundary.");
}
var positive=Detached(runtime,run,bundle,false);var negative=Detached(runtime,run,bundle,true);GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();
if(positive.Source.IsAlive||!negative.Source.IsAlive)throw new InvalidDataException("Original owner decoder retention control.");var gc=positive.Application.CopyResult();positive.Dispose();negative.Dispose();GC.KeepAlive(negative.Keep);
File.WriteAllText(args[3],JsonSerializer.Serialize(new Capture(Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(args[1]))),actual.ToArray(),events.ToArray(),failures.ToArray(),unsafeSources.ToArray(),inverse,inverseEvents.ToArray(),inverseFailures.ToArray(),postcheck,gc,completed,refused,guards),CaptureJson.Default.Capture));
Console.WriteLine($"PASS6 actual original Math source/FIELD applications;{failures.Count}+{inverseFailures.Count} prefixes/{guards} guards;6unsafe observer cases/monotonic inverse/GC;no CBB acceptance.");
static bool SameField(Math2Mode0MathFieldSnapshot a,Math2Mode0MathFieldSnapshot b)=>JsonSerializer.Serialize(a,CaptureJson.Default.Math2Mode0MathFieldSnapshot)==JsonSerializer.Serialize(b,CaptureJson.Default.Math2Mode0MathFieldSnapshot);
static bool SamePub(Math2Mode0PublishSnapshot a,Math2Mode0PublishSnapshot b)=>JsonSerializer.Serialize(a,CaptureJson.Default.Math2Mode0PublishSnapshot)==JsonSerializer.Serialize(b,CaptureJson.Default.Math2Mode0PublishSnapshot);
static bool SameTransfer(Math2Mode0TransferSnapshot a,Math2Mode0TransferSnapshot b)=>JsonSerializer.Serialize(a,CaptureJson.Default.Math2Mode0TransferSnapshot)==JsonSerializer.Serialize(b,CaptureJson.Default.Math2Mode0TransferSnapshot);
static bool SameOriginal(Math2Mode0OriginalSnapshot a,Math2Mode0OriginalSnapshot b)=>JsonSerializer.Serialize(a,CaptureJson.Default.Math2Mode0OriginalSnapshot)==JsonSerializer.Serialize(b,CaptureJson.Default.Math2Mode0OriginalSnapshot);
[MethodImpl(MethodImplOptions.NoInlining)]
static DetachedOwner Detached(OrtRuntime runtime,OrtRun run,Math2Mode0ColdBundle bundle,bool keep)
{
 var p=Prepare(runtime,run,bundle,1,false);var pub=Math2Mode0FreshMathPublisher.Create(p.Receipt);var outgoing=Math2Mode0OutgoingTransfer.Create(pub);var app=Math2Mode0OriginalApplication.Prepare(p.Field,outgoing);app.RunSource();app.ApplyDom();
 var weak=new WeakReference(p.Source);object? retained=keep?p.Output:null;p.Output=null;p.Source.Dispose();p.Source=null!;return new(p,pub,outgoing,app,weak,retained);
}
static InkStroke[] Ink(bool reverse)
{InkStroke[] a=[new([new InkPoint(0,30),new(0,0),new(10,18),new(20,0),new(20,30)]),new([new InkPoint(35,0),new(35,30)])];return reverse?a.Reverse().ToArray():a;}
static Prepared Prepare(OrtRuntime runtime,OrtRun run,Math2Mode0ColdBundle bundle,int width,bool reverse)
{
 var features=Math2Mode0RawSourcePositions.PrepareFeatures(Ink(reverse),default);
 return PrepareWithView(runtime,run,bundle,width,reverse,LegacyFreshRawInkCurrentView.Prepare(features.Positions.Document));
}
static Prepared PrepareWithView(OrtRuntime runtime,OrtRun run,Math2Mode0ColdBundle bundle,int width,bool reverse,LegacyFreshRawInkCurrentView view)
{
 var field=Math2Mode0OriginalMathFieldSource.Prepare(view,"formula");
 var features=Math2Mode0RawSourcePositions.PrepareFeatures(field.Document,default);
 var source=width==1?Math2Mode0RawOnlySource.PrepareColdWithSelectedMathField(runtime,bundle,run,field):
  Math2Mode0RawOnlySource.PrepareWithPreparedSearchAndSelectedMathField(runtime,bundle,run,field,width);
 source.RunToInitialOrder();source.ApplyEf4();var output=source.OrderFinal().FormatFullCold().AttachSourcePositions();var raw=source.Loop.CopyRawAttentionStatistics();
 var receipt=output.CopySelectedMathField(field);var positions=source.ReadSourcePositions();
 var rows=receipt.Copy.CopyRecord();var evidence=new Evidence(Ink(reverse),field.CopySnapshot(),features.Features.Frames,features.Features.Values.Select(BitConverter.SingleToInt32Bits).ToArray(),
  positions.CopyPositions(),positions.CopySeparators(),positions.CopySourceOrder(),positions.PayloadReservation,raw.RawFrameHashes,
  rows.Primary,rows.Secondary,rows.Units,rows.Runs,rows.SourceRanges.Select(a=>a.Select(r=>new Range(r.First.Value,r.Last.Value)).ToArray()).ToArray(),
  rows.UnitRows.Select(r=>new Unit(r.Unit,r.Text.Identity,r.ByteCount,Convert.ToBase64String(r.Text.ReadBytes()))).ToArray(),output.CopyStatistics().PayloadReservation,receipt.Copy.CopyStatistics().PayloadReservation);
 return new(source,field,receipt,view,evidence,output);
}
sealed class Prepared(Math2Mode0RawOnlySource source,Math2Mode0OriginalMathFieldSource field,Math2Mode0SelectedMathFieldCopy receipt,LegacyFreshRawInkCurrentView view,Evidence evidence,Math2Mode0SourceBoundOutput output):IDisposable
{
 internal Math2Mode0RawOnlySource Source=source;internal readonly Math2Mode0OriginalMathFieldSource Field=field;internal readonly Math2Mode0SelectedMathFieldCopy Receipt=receipt;
 internal readonly LegacyFreshRawInkCurrentView View=view;internal readonly Evidence Evidence=evidence;internal Math2Mode0SourceBoundOutput? Output=output;
 public void Dispose(){Source?.Dispose();Receipt.Dispose();Field.Dispose();}
}
record Range(ulong First,ulong Last);
record Unit(uint Word,int Identity,int ByteCount,string Bytes);
record Evidence(InkStroke[] Ink,Math2Mode0MathFieldSnapshot Field,int Frames,int[] FeatureBits,ulong[] Positions,int[] Separators,int[] Order,long PositionReservation,string[] RawHashes,ushort[] Primary,ushort[] Secondary,uint[] Units,MathSegmentRun[] Runs,Range[][] Ranges,Unit[] Rows,long AttachmentReservation,long CopyReservation);
record Actual(string Policy,int Width,bool Reversed,Evidence Evidence,Math2Mode0PublishSnapshot Publication,Math2Mode0TransferSnapshot Transfer,Math2Mode0OriginalSnapshot Prepared,Math2Mode0OriginalSnapshot Source,Math2Mode0OriginalSnapshot Final);
record Step(string Flow,string Name,int Index);
record Failure(string Flow,string Stage,int Index,bool Canceled,Math2Mode0OriginalSnapshot Before,Math2Mode0OriginalSnapshot After);
record UnsafeSource(bool AfterObserver,int Index,Math2Mode0OriginalSnapshot State);
record Capture(string Resource,Actual[] Actual,Step[] Events,Failure[] Failures,UnsafeSource[] UnsafeSources,Math2Mode0OriginalSnapshot Inverse,Step[] InverseEvents,Failure[] InverseFailures,Math2Mode0OriginalSnapshot Postcheck,Math2Mode0OriginalSnapshot Gc,int ConcurrentCompleted,int ConcurrentRefused,int Guards);
sealed record DetachedOwner(Prepared Prepared,Math2Mode0FreshMathPublisher Publisher,Math2Mode0OutgoingTransfer Outgoing,Math2Mode0OriginalApplication Application,WeakReference Source,object? Keep):IDisposable
{public void Dispose(){Application.Dispose();Outgoing.Dispose();Publisher.Dispose();Prepared.Dispose();}}
[JsonSerializable(typeof(Capture))][JsonSerializable(typeof(Math2Mode0MathFieldSnapshot))][JsonSerializable(typeof(Math2Mode0PublishSnapshot))][JsonSerializable(typeof(Math2Mode0TransferSnapshot))][JsonSerializable(typeof(Math2Mode0OriginalSnapshot))]
internal partial class CaptureJson:JsonSerializerContext;
