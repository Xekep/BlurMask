using InkRecognition;
using InkRecognition.Math;
using System.Text.Json;
using System.Text.Json.Serialization;

if(args.Length!=4)throw new ArgumentException("model rawres officialORT evidence");
using var runtime=new OrtRuntime(args[2]);using var run=runtime.CreateRun(default);
using var stream=File.OpenRead(args[0]);using var bundle=Math2Mode0ColdBundle.Read(runtime,stream,File.ReadAllBytes(args[1]));
int controls=0;
void Require(bool condition){if(!condition)throw new InvalidDataException($"Freeze owner assertion {controls}");controls++;}
void Reject(Action action)
{try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){controls++;return;}throw new InvalidDataException($"Freeze owner refusal {controls}");}
using var canceled=new CancellationTokenSource();canceled.Cancel();
var actual=new List<Actual>();
foreach(int width in new[]{1,3,16})foreach(bool reversed in new[]{false,true})
{
 using var p=Pipeline.Create(runtime,run,bundle,width,reversed);p.Application.RunSource();p.Application.ApplyDom();
 using var scope=p.Application.CreatePriorSelectedScope();var before=scope.CopySnapshot();
 Reject(()=>Math2Mode0PriorFreezePreparation.CreateDiagnostic(scope,0,null));
 Reject(()=>Math2Mode0PriorFreezePreparation.Create(scope,canceled.Token));
 using var preparation=Math2Mode0PriorFreezePreparation.Create(scope);var result=preparation.CopyResult();
 Require(result.Complete&&!result.Failed&&!result.Released&&result.Rows.Length==0&&result.SequenceChildren>0);
 Require(JsonSerializer.Serialize(before,CaptureJson.Default.Math2Mode0PriorSnapshot)==JsonSerializer.Serialize(scope.CopySnapshot(),CaptureJson.Default.Math2Mode0PriorSnapshot));
 Reject(()=>Math2Mode0PriorFreezePreparation.Create(scope));
 actual.Add(new(width,reversed,before,result));
 using var next=p.Application.CreatePriorSelectedScope();
 Reject(()=>Math2Mode0PriorFreezePreparation.CreateDiagnostic(next,result.Reservation-1,null));
 using var exact=Math2Mode0PriorFreezePreparation.CreateDiagnostic(next,result.Reservation,null);
 Require(exact.CopyResult().Reservation==result.Reservation);
 Math2Mode0PriorSelectedScope.FreezeBorrow? escaped=null;
 scope.WithFreezeInput(input=>{escaped=input;Reject(()=>scope.WithFreezeInput(_=>0));return 0;});
 Reject(()=>escaped!.Check());
 preparation.Dispose();Reject(()=>preparation.CopyResult());Require(scope.CopySnapshot().Epoch==1);
}
foreach(var stage in new[]{Math2Mode0FreezeStage.Claimed,Math2Mode0FreezeStage.Complete})
foreach(bool cancel in new[]{false,true})
{
 using var p=Pipeline.Create(runtime,run,bundle);p.Application.RunSource();p.Application.ApplyDom();using var scope=p.Application.CreatePriorSelectedScope();
 using var stop=new CancellationTokenSource();Math2Mode0PriorFreezePreparation? retained=null;
 try
 {
  Math2Mode0PriorFreezePreparation.CreateDiagnostic(scope,Math2Mode0PriorFreezePreparation.DefaultBudget,(s,i,t)=>
  {retained=t;if(s==stage){if(cancel)stop.Cancel();else throw new IOException("Injected logical observer failure");}},stop.Token);
  throw new InvalidDataException();
 }
 catch(Exception e)when(e is IOException or OperationCanceledException){controls++;}
 Require(retained is not null&&retained.CopySnapshot().Failed&&retained.CopySnapshot().Rows.Length==0);
 Reject(()=>retained!.CopyResult());Reject(()=>Math2Mode0PriorFreezePreparation.Create(scope));retained!.Dispose();
}
foreach(string close in new[]{"scope","outgoing","application","field"})
{
 using var p=Pipeline.Create(runtime,run,bundle);p.Application.RunSource();p.Application.ApplyDom();using var scope=p.Application.CreatePriorSelectedScope();
 using var preparation=Math2Mode0PriorFreezePreparation.Create(scope);
 if(close=="scope")scope.Dispose();else if(close=="outgoing")p.Outgoing.Dispose();else if(close=="application")p.Application.Dispose();else p.Field.Dispose();
 Reject(()=>preparation.CopyResult());
}
using(var p=Pipeline.Create(runtime,run,bundle))
{
 p.Application.RunSource();p.Application.ApplyDom();using var scope=p.Application.CreatePriorSelectedScope();Math2Mode0PriorFreezePreparation? retained=null;
 Reject(()=>Math2Mode0PriorFreezePreparation.CreateDiagnostic(scope,Math2Mode0PriorFreezePreparation.DefaultBudget,(s,i,t)=>{retained=t;if(s==Math2Mode0FreezeStage.Complete)p.Application.Dispose();}));
 Require(retained is not null&&retained.CopySnapshot().Failed);Reject(()=>retained!.CopyResult());retained!.Dispose();
}
var prepared=PreparedControls.Run();
File.WriteAllText(args[3],JsonSerializer.Serialize(new Capture(controls,runtime.Version,actual.ToArray(),prepared),CaptureJson.Default.Capture));
Console.WriteLine($"PASS {actual.Count} actual current-result scopes -> zero-freeze extraction; {controls} owning controls; {prepared.Length} separate prepared branch cases. No new NN binding/nonfresh diff.");

record Actual(int Width,bool Reversed,Math2Mode0PriorSnapshot Prior,Math2Mode0FreezeSnapshot Freeze);
record Capture(int OwnerControls,string Runtime,Actual[] Actual,PreparedCase[] Prepared);
[JsonSerializable(typeof(Capture))][JsonSerializable(typeof(Math2Mode0PriorSnapshot))]
internal partial class CaptureJson:JsonSerializerContext;
