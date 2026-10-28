using InkRecognition;
using InkRecognition.Math;
using System.Text.Json;
using System.Text.Json.Serialization;

if(args.Length!=4)throw new ArgumentException("model rawres officialORT evidence");
using var runtime=new OrtRuntime(args[2]);using var run=runtime.CreateRun(default);
using var stream=File.OpenRead(args[0]);using var bundle=Math2Mode0ColdBundle.Read(runtime,stream,File.ReadAllBytes(args[1]));
int controls=0;
void Require(bool condition){if(!condition)throw new InvalidDataException($"Current C23 assertion {controls}");controls++;}
void Reject(Action action)
{try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){controls++;return;}throw new InvalidDataException($"Current C23 refusal {controls}");}
using var canceled=new CancellationTokenSource();canceled.Cancel();
var actual=new List<Actual>();
foreach(int width in new[]{1,3,16})foreach(bool reversed in new[]{false,true})
{
 using var p=Pipeline.Create(runtime,run,bundle,width,reversed);p.Application.RunSource();p.Application.ApplyDom();
 using var scope=p.Application.CreatePriorSelectedScope();using var freeze=Math2Mode0PriorFreezePreparation.Create(scope);var before=scope.CopySnapshot();
 Reject(()=>Math2Mode0PriorC23Preparation.CreateDiagnostic(scope,freeze,0,null));
 Reject(()=>Math2Mode0PriorC23Preparation.Create(scope,freeze,canceled.Token));
 using var current=Math2Mode0PriorC23Preparation.Create(scope,freeze);var selected=current.CopyResult();
 Require(selected.Complete&&!selected.Failed&&selected.WorkCount==1&&selected.InitialComputeFlag==1&&!selected.WorkCompleted&&selected.TransformCount==0&&selected.FreezeCount==0);
 Require(ReferenceEquals(current.SelectedInkIdentity,scope.SelectedInkIdentity));
 Require(ReferenceEquals(current.InputC22Identity,scope.SelectedC22Identity(selected.InputId)));
 Require(!ReferenceEquals(current.C23Identity,current.InputC22Identity)&&!ReferenceEquals(current.C23Identity,current.SelectedInkIdentity));
 Require(ReferenceEquals(current.Document,p.Field.Document));
 Require(selected.C23Ranges.AsSpan().SequenceEqual(selected.WorkRanges));
 Reject(()=>Math2Mode0PriorC23Preparation.Create(scope,freeze));
 Reject(()=>Math2Mode0PriorRawOnlySource.PrepareCold(runtime,bundle,run,current,canceled.Token));
 Require(!current.CopyResult().InferenceClaimed);
 Reject(()=>Math2Mode0PriorRawOnlySource.PrepareDiagnostic(runtime,bundle,run,current,0));
 using var next=width==1?Math2Mode0PriorRawOnlySource.PrepareCold(runtime,bundle,run,current):Math2Mode0PriorRawOnlySource.PrepareDiagnostic(runtime,bundle,run,current,width);
 Require(!ReferenceEquals(next.DecoderIdentity,p.Source));Require(ReferenceEquals(next.SourceDocument,p.Field.Document));
 Require(current.CopyResult().InferenceClaimed&&current.CopyResult().SourcePrepared);
 Reject(()=>Math2Mode0PriorRawOnlySource.PrepareCold(runtime,bundle,run,current));
 Reject(()=>next.CopyResult());
 Reject(()=>current.WithPreparedSource(new object(),()=>0));
 Reject(()=>next.RunToFirstCopy(canceled.Token));
 next.RunToFirstCopy();var result=next.CopyResult();
 Require(result.Complete&&!result.Failed&&result.DecoderCalls>0&&result.RawHashes.Length==result.DecoderCalls&&result.FirstCopy is not null);
 Require(current.CopyResult().WorkCompleted);
 Require(next.WithCompletedResult(copy=>ReferenceEquals(copy.Document,p.Field.Document)));
 Require(JsonSerializer.Serialize(before,CaptureJson.Default.Math2Mode0PriorSnapshot)==JsonSerializer.Serialize(scope.CopySnapshot(),CaptureJson.Default.Math2Mode0PriorSnapshot));
 Reject(()=>next.RunToFirstCopy());
 var detached=current.CopyResult();detached.C23Ranges.AsSpan().Fill(default);Require(current.CopyResult().C23Ranges[0]!=default);
 var bytes=result.FirstCopy!.Text[0].Bytes;bytes.AsSpan().Fill(0);Require(next.CopyResult().FirstCopy!.Text[0].Bytes.Any(b=>b!=0));
 result=next.CopyResult();next.ReleaseDecoder();Require(next.CopyResult().DecoderReleased);
 actual.Add(new(width,reversed,p.Field.Document.Strokes.ToArray(),before,selected,current.CopyResult(),result));
 using var retryScope=p.Application.CreatePriorSelectedScope();using var retryFreeze=Math2Mode0PriorFreezePreparation.Create(retryScope);
 Reject(()=>Math2Mode0PriorC23Preparation.CreateDiagnostic(retryScope,retryFreeze,selected.Reservation-1,null));
 using var exact=Math2Mode0PriorC23Preparation.CreateDiagnostic(retryScope,retryFreeze,selected.Reservation,null);Require(exact.CopyResult().Reservation==selected.Reservation);
 Reject(()=>Math2Mode0PriorC23Preparation.Create(retryScope,freeze));
 p.Outgoing.Dispose();Reject(()=>next.CopyResult());
}
foreach(var stage in new[]{Math2Mode0CurrentC23Stage.Claimed,Math2Mode0CurrentC23Stage.Selected,Math2Mode0CurrentC23Stage.Work,Math2Mode0CurrentC23Stage.Complete})
foreach(bool cancel in new[]{false,true})
{
 using var p=Pipeline.Create(runtime,run,bundle);p.Application.RunSource();p.Application.ApplyDom();using var scope=p.Application.CreatePriorSelectedScope();using var freeze=Math2Mode0PriorFreezePreparation.Create(scope);
 using var stop=new CancellationTokenSource();Math2Mode0PriorC23Preparation? retained=null;
 try{Math2Mode0PriorC23Preparation.CreateDiagnostic(scope,freeze,Math2Mode0PriorC23Preparation.DefaultBudget,(s,t)=>{retained=t;if(s==stage){if(cancel)stop.Cancel();else throw new IOException("Current C23 logical prefix");}},stop.Token);throw new InvalidDataException();}
 catch(Exception e)when(e is IOException or OperationCanceledException){controls++;}
 Require(retained is not null&&retained.CopySnapshot().Failed);Reject(()=>retained!.CopyResult());Reject(()=>Math2Mode0PriorC23Preparation.Create(scope,freeze));retained!.Dispose();
}
foreach(string close in new[]{"scope","freeze","current","outgoing"})
{
 using var p=Pipeline.Create(runtime,run,bundle);p.Application.RunSource();p.Application.ApplyDom();using var scope=p.Application.CreatePriorSelectedScope();using var freeze=Math2Mode0PriorFreezePreparation.Create(scope);using var current=Math2Mode0PriorC23Preparation.Create(scope,freeze);using var next=Math2Mode0PriorRawOnlySource.PrepareCold(runtime,bundle,run,current);
 if(close=="scope")scope.Dispose();else if(close=="freeze")freeze.Dispose();else if(close=="current")current.Dispose();else p.Outgoing.Dispose();
 Reject(()=>next.RunToFirstCopy());
}
using(var p=Pipeline.Create(runtime,run,bundle))
{
 p.Application.RunSource();p.Application.ApplyDom();using var scope=p.Application.CreatePriorSelectedScope();using var freeze=Math2Mode0PriorFreezePreparation.Create(scope);using var current=Math2Mode0PriorC23Preparation.Create(scope,freeze);
 Math2Mode0PriorC23Preparation.NnBorrow? escaped=null;
 Reject(()=>current.WithNewNn(b=>{escaped=b;return 0;},default));
 Reject(()=>escaped!.Check());Require(current.CopySnapshot().Failed);
 Reject(()=>Math2Mode0PriorRawOnlySource.PrepareCold(runtime,bundle,run,current));
}
foreach(var stage in Enum.GetValues<Math2Mode0PriorNnStage>())foreach(bool cancel in new[]{false,true})
{
 using var p=Pipeline.Create(runtime,run,bundle);p.Application.RunSource();p.Application.ApplyDom();using var scope=p.Application.CreatePriorSelectedScope();using var freeze=Math2Mode0PriorFreezePreparation.Create(scope);using var current=Math2Mode0PriorC23Preparation.Create(scope,freeze);using var next=Math2Mode0PriorRawOnlySource.PrepareCold(runtime,bundle,run,current);using var stop=new CancellationTokenSource();
 try{next.RunDiagnostic(s=>{if(s==stage){if(cancel)stop.Cancel();else throw new IOException("Entered prior NN logical prefix");}},stop.Token);throw new InvalidDataException();}
 catch(Exception e)when(e is IOException or OperationCanceledException){controls++;}
 Require(next.CopySnapshot().Failed&&current.CopySnapshot().Failed);
 Require(next.CopySnapshot().Complete==(stage==Math2Mode0PriorNnStage.Completed));
 Reject(()=>next.CopyResult());Reject(()=>next.WithCompletedResult(_=>0));
}
foreach(string close in new[]{"scope","current","outgoing","self"})
{
 using var p=Pipeline.Create(runtime,run,bundle);p.Application.RunSource();p.Application.ApplyDom();using var scope=p.Application.CreatePriorSelectedScope();using var freeze=Math2Mode0PriorFreezePreparation.Create(scope);using var current=Math2Mode0PriorC23Preparation.Create(scope,freeze);using var next=Math2Mode0PriorRawOnlySource.PrepareCold(runtime,bundle,run,current);
 Reject(()=>next.RunDiagnostic(s=>{if(s!=Math2Mode0PriorNnStage.Completed)return;if(close=="scope")scope.Dispose();else if(close=="current")current.Dispose();else if(close=="outgoing")p.Outgoing.Dispose();else next.Dispose();}));
 Require(next.CopySnapshot().Failed);Reject(()=>next.CopyResult());
}
File.WriteAllText(args[3],JsonSerializer.Serialize(new Capture(controls,runtime.Version,actual.ToArray()),CaptureJson.Default.Capture));
Console.WriteLine($"PASS{actual.Count} genuine current C23/singleF0 -> NEW model/ORT decoding and owned first copy;{controls} guards/identity/prefix controls. No repeated DOM update.");

record Actual(int Width,bool Reversed,InkStroke[] Ink,Math2Mode0PriorSnapshot Prior,Math2Mode0CurrentC23Snapshot Prepared,Math2Mode0CurrentC23Snapshot Finished,Math2Mode0PriorNnSnapshot NewNn);
record Capture(int Controls,string Runtime,Actual[] Actual);
[JsonSerializable(typeof(Capture))][JsonSerializable(typeof(Math2Mode0PriorSnapshot))]
internal partial class CaptureJson:JsonSerializerContext;
