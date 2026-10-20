using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
using InkRecognition.Math;
if(args.Length!=4)throw new ArgumentException("pack res official-runtime capture");
using var runtime=new OrtRuntime(Path.GetFullPath(args[2]));using var run=runtime.CreateRun(default);
using var stream=File.OpenRead(args[0]);using var bundle=Math2Mode0ColdBundle.Read(runtime,stream,File.ReadAllBytes(args[1]));
int guards=0;void Reject(Action call){try{call();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Missing outgoing transfer guard.");}
using var canceled=new CancellationTokenSource();canceled.Cancel();
var actual=new List<Actual>();var events=new List<Stage>();
foreach(int width in new[]{1,3,16})foreach(bool reverse in new[]{false,true})
{
 using var p=Prepare(runtime,run,bundle,width,reverse);using var publisher=Math2Mode0FreshMathPublisher.Create(p.Receipt);
 var before=publisher.CopyResult();Reject(()=>Math2Mode0OutgoingTransfer.Create(publisher,canceled.Token));Reject(()=>Math2Mode0OutgoingTransfer.CreateDiagnostic(publisher,0,null));
 using var transfer=Math2Mode0OutgoingTransfer.CreateDiagnostic(publisher,Math2Mode0OutgoingTransfer.DefaultPayloadBudget,(s,i,t)=>{if(width==1&&!reverse)events.Add(new(s.ToString(),i));});
 var result=transfer.CopyResult();Reject(()=>Math2Mode0OutgoingTransfer.Create(publisher));
 var copy=transfer.CopyResult();copy.Writer.AsSpan().Fill(null!);copy.Commands[0].Ranges.AsSpan().Fill(default);
 if(transfer.CopyResult().Writer[0]==null||transfer.CopyResult().Commands[0].Ranges[0]==default)throw new InvalidDataException("Outgoing snapshot aliases retained payload.");
 if(!Same(before,publisher.CopyResult()))throw new InvalidDataException("Outgoing transfer modified retained publisher.");
 p.Source.Dispose();transfer.CopyResult();p.Receipt.Validate(p.Receipt.Copy,p.Field);
 actual.Add(new(width==1?"ColdDefault":"PreparedSearch",width,reverse,p.Evidence,before,result));
 p.Field.Dispose();Reject(()=>transfer.CopyResult());transfer.Dispose();transfer.Dispose();if(transfer.CopySnapshot().CleanupCount!=1)throw new InvalidDataException("Double cleanup.");
}
var failures=new List<Failure>();
foreach(var step in events)foreach(bool cancel in new[]{false,true})
{
 using var p=Prepare(runtime,run,bundle,1,false);using var publisher=Math2Mode0FreshMathPublisher.Create(p.Receipt);using var stop=new CancellationTokenSource();
 Math2Mode0OutgoingTransfer? retained=null;Math2Mode0TransferSnapshot? before=null;
 try{Math2Mode0OutgoingTransfer.CreateDiagnostic(publisher,Math2Mode0OutgoingTransfer.DefaultPayloadBudget,(s,i,t)=>{if(s.ToString()!=step.Name||i!=step.Index)return;retained=t;before=t.CopySnapshot();if(cancel)stop.Cancel();else throw new IOException("Prepared logical prefix.");},stop.Token);throw new InvalidDataException("Missing prefix.");}
 catch(Exception e)when(e is IOException or OperationCanceledException){}
 if(retained is null||before is null||!retained.CopySnapshot().Failed)throw new InvalidDataException("Lost outgoing prefix.");
 failures.Add(new(step.Name,step.Index,cancel,before,retained.CopySnapshot()));Reject(()=>retained.CopyResult());Reject(()=>Math2Mode0OutgoingTransfer.Create(publisher));
 retained.Dispose();retained.Dispose();if(retained.CopySnapshot().CleanupCount!=1)throw new InvalidDataException();
}
using(var p=Prepare(runtime,run,bundle,1,false))
{
 using var pub=Math2Mode0FreshMathPublisher.Create(p.Receipt);using var t=Math2Mode0OutgoingTransfer.Create(pub);long exact=t.CopySnapshot().Reservation;
 using var q=Prepare(runtime,run,bundle,1,false);using var other=Math2Mode0FreshMathPublisher.Create(q.Receipt);
 Reject(()=>Math2Mode0OutgoingTransfer.CreateDiagnostic(other,exact-1,null));using var same=Math2Mode0OutgoingTransfer.CreateDiagnostic(other,exact,null);
 if(same.CopyResult().Reservation!=exact)throw new InvalidDataException("Envelope boundary.");
 Reject(()=>Math2Mode0OutgoingTransfer.Create(null!));Reject(()=>Math2Mode0OutgoingTransfer.CreateDiagnostic(pub,-1,null));Reject(()=>Math2Mode0OutgoingTransfer.CreateDiagnostic(pub,Math2Mode0OutgoingTransfer.DefaultPayloadBudget+1,null));
 //Prepared kernel counterexamples are separate from actual NN-produced states.
 pub.WithCompletedTransferInput(input=>
 {
  void Plan(Math2Mode0OutgoingInput value)=>Math2Mode0OutgoingKernel.Plan(value,Math2Mode0OutgoingTransfer.DefaultPayloadBudget,default);
  Reject(()=>Plan(input with{ThirdInk=input.Field.SelectedInk}));
  Reject(()=>Plan(input with{Names=p.Field.OriginalNameOwner}));
  Reject(()=>Plan(input with{NodeCount=65537}));
  Reject(()=>Plan(input with{Registry=input.Registry.Select(r=>r.Id==input.Field.InputId?r:r with{Category=999}).ToArray()}));
  Reject(()=>Plan(input with{Registry=input.Registry.Select(r=>r.Id==input.Field.InputId?r with{Ranges=[]}:r).ToArray()}));
  var nodes=input.Nodes().ToArray();int resultTag=Array.FindIndex(nodes,n=>n.Name=="tag"&&n.Number>0);
  Reject(()=>Plan(input with{Nodes=()=>nodes.Select((n,i)=>i==resultTag?n with{Number=-9}:n)}));
  Reject(()=>Plan(input with{Nodes=()=>nodes.Select(n=>n.Id==1?n with{Parent=1}:n)}));
  Reject(()=>Plan(input with{Registry=input.Registry.Select((r,i)=>i==input.Registry.Count-1?r with{Id=input.Field.InputId}:r).ToArray()}));
  return 0;
 });
 var retainedPayload=t.CopySnapshot();pub.Dispose();Reject(()=>t.CopyResult());
 if(!SameTransfer(retainedPayload,t.CopySnapshot()))throw new InvalidDataException("Disposed publisher changed independent outgoing storage.");
 p.Receipt.Validate(p.Receipt.Copy,p.Field);
}
foreach(int close in new[]{0,1,2})
{
 using var p=Prepare(runtime,run,bundle,1,false);using var pub=Math2Mode0FreshMathPublisher.Create(p.Receipt);Math2Mode0OutgoingTransfer? target=null;
 Reject(()=>Math2Mode0OutgoingTransfer.CreateDiagnostic(pub,Math2Mode0OutgoingTransfer.DefaultPayloadBudget,(s,i,t)=>{target=t;if(s==Math2Mode0TransferStage.DomExported){if(close==0)pub.Dispose();else if(close==1)p.Field.Dispose();else p.Receipt.Dispose();}}));
 if(target is null||!target.CopySnapshot().Failed||!target.CopySnapshot().DomExported||target.CopySnapshot().SourceExported)throw new InvalidDataException("Closure crossed mutation boundary.");target.Dispose();
}
int completed=0,refused=0;
using(var p=Prepare(runtime,run,bundle,1,false))
{
 using var pub=Math2Mode0FreshMathPublisher.Create(p.Receipt);
 Parallel.For(0,2,_=>{try{using var t=Math2Mode0OutgoingTransfer.Create(pub);t.CopyResult();Interlocked.Increment(ref completed);}catch(InvalidOperationException){Interlocked.Increment(ref refused);}});
 if(completed!=1||refused!=1)throw new InvalidDataException("Concurrent one-use claim.");pub.CopyResult();p.Receipt.Validate(p.Receipt.Copy,p.Field);
}
File.WriteAllText(args[3],JsonSerializer.Serialize(new Capture(Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(args[1]))),actual.ToArray(),events.ToArray(),failures.ToArray(),completed,refused,guards),CaptureJson.Default.Capture));
Console.WriteLine($"PASS {actual.Count} actual outgoing transfers; {failures.Count} stage prefixes/{guards} guards; no original-page acceptance.");
static bool Same(Math2Mode0PublishSnapshot a,Math2Mode0PublishSnapshot b)=>JsonSerializer.Serialize(a,CaptureJson.Default.Math2Mode0PublishSnapshot)==JsonSerializer.Serialize(b,CaptureJson.Default.Math2Mode0PublishSnapshot);
static bool SameTransfer(Math2Mode0TransferSnapshot a,Math2Mode0TransferSnapshot b)=>JsonSerializer.Serialize(a,CaptureJson.Default.Math2Mode0TransferSnapshot)==JsonSerializer.Serialize(b,CaptureJson.Default.Math2Mode0TransferSnapshot);
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
record Actual(string Policy,int Width,bool Reversed,Evidence Evidence,Math2Mode0PublishSnapshot Publication,Math2Mode0TransferSnapshot Transfer);
record Stage(string Name,int Index);
record Failure(string Stage,int Index,bool Canceled,Math2Mode0TransferSnapshot Before,Math2Mode0TransferSnapshot After);
record Capture(string Resource,Actual[] Actual,Stage[] Events,Failure[] Failures,int ConcurrentCompleted,int ConcurrentRefused,int Guards);
[JsonSerializable(typeof(Capture))][JsonSerializable(typeof(Math2Mode0PublishSnapshot))][JsonSerializable(typeof(Math2Mode0TransferSnapshot))]
internal partial class CaptureJson:JsonSerializerContext;
