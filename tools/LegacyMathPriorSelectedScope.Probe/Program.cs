using InkRecognition;
using InkRecognition.Math;
using System.Text.Json;
using System.Text.Json.Serialization;

if(args.Length!=4)throw new ArgumentException("model rawres officialORT evidence");
using var runtime=new OrtRuntime(args[2]);using var run=runtime.CreateRun(default);
using var stream=File.OpenRead(args[0]);using var bundle=Math2Mode0ColdBundle.Read(runtime,stream,File.ReadAllBytes(args[1]));
int controls=0;
void Require(bool condition){if(!condition)throw new InvalidDataException($"Prior owner assertion {controls}");controls++;}
void Reject(Action action)
{try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){controls++;return;}throw new InvalidDataException($"Prior owner refusal {controls}");}
using var canceled=new CancellationTokenSource();canceled.Cancel();
using var p=Pipeline.Create(runtime,run,bundle);
Reject(()=>p.Application.CreatePriorSelectedScope()); // Prepared, no source execution/DOM.
p.Application.RunSource();Reject(()=>p.Application.CreatePriorSelectedScope()); // Source-only.
p.Application.ApplyDomDiagnostic((stage,index,target)=>{if(stage==Math2Mode0OriginalStage.Complete)Reject(()=>target.CreatePriorSelectedScope());});var applied=p.Application.CopyResult();
using var scope=p.Application.CreatePriorSelectedScope();var selected=scope.CopySnapshot();
Require(selected.Epoch==1&&selected.Next==applied.ActiveNext&&selected.Next>1);
Require(ReferenceEquals(scope.NameOwner,p.Field.OriginalNameOwner)&&!ReferenceEquals(scope.NameOwner,p.Field.SelectedNameOwner));
Require(!ReferenceEquals(scope.SelectedInkIdentity,p.Field.OriginalInkIdentity)&&!ReferenceEquals(scope.SelectedInkIdentity,p.Field.SelectedInkIdentity));
Require(selected.SelectedField.Count(n=>n.Name=="mathResult"&&n.Kind==3)==1);
Require(selected.SelectedField.Where(n=>n.Name=="tag").Select(n=>n.Number).SequenceEqual(applied.ActivePage.Where(n=>n.Name=="tag").Select(n=>n.Number)));
Require(selected.Registry.Length==applied.ActiveRegistry.Length);
for(int i=0;i<selected.Registry.Length;i++)
{
 var a=applied.ActiveRegistry[i];var b=selected.Registry[i];
 Require(a.Id==b.Id&&a.Name==b.Name&&a.Category==b.Category&&a.Ranges.AsSpan().SequenceEqual(b.Ranges));
}
foreach(var a in selected.Registry)foreach(var b in selected.Registry)
 Require(ReferenceEquals(scope.SelectedC22Identity(a.Id),scope.SelectedC22Identity(b.Id))==(applied.ActiveRegistry.Single(x=>x.Id==a.Id).CanonicalC22Id==applied.ActiveRegistry.Single(x=>x.Id==b.Id).CanonicalC22Id));
Require(selected.Registry.GroupBy(n=>n.CanonicalC22Id).Any(g=>g.Count()>1)); // Actual INPUT/full-terminal alias.
var selectedIdentities=selected.Registry.ToDictionary(r=>r.Id,r=>scope.SelectedC22Identity(r.Id));
Math2Mode0OriginalApplication.PriorBorrow? escaped=null;
p.Application.WithCompletedOriginalInput(b=>
{
 escaped=b;b.CheckSource(p.Field,p.Field.OriginalInkIdentity);
 foreach(var row in selected.Registry)
 {
  // A second scope was already issued; only compare opaque copied identity
  // outside a nested borrow. Actual original C22 must differ from selected.
  var old=b.OriginalC22Identity(row.Id);Require(old is not null&&!ReferenceEquals(old,selectedIdentities[row.Id]));
 }
 Reject(()=>p.Application.CreatePriorSelectedScope()); // Reentrant issuance.
 return 0;
});
Reject(()=>escaped!.Check());
Reject(()=>Math2Mode0PriorSelectedScope.Capture(escaped!,Math2Mode0PriorSelectedScope.DefaultBudget,default));
Math2Mode0OriginalApplication.PriorBorrow? callbackFailed=null;
try{p.Application.WithCompletedOriginalInput<int>(b=>{callbackFailed=b;throw new IOException("Borrow callback failure");});throw new InvalidDataException();}
catch(IOException){controls++;}
Reject(()=>callbackFailed!.Check());
using(var interrupted=new CancellationTokenSource())
{
 Math2Mode0OriginalApplication.PriorBorrow? stopped=null;
 Reject(()=>p.Application.WithCompletedOriginalInput(b=>{stopped=b;interrupted.Cancel();return 0;},interrupted.Token));
 Reject(()=>stopped!.Check());
}
Reject(()=>p.Field.CloneCurrentOriginalForPrior(escaped!));
using(var foreign=Pipeline.Create(runtime,run,bundle,LegacyFreshRawInkCurrentView.Prepare(p.Field.Document)))
{
 Require(ReferenceEquals(p.Field.Document,foreign.Field.Document));
 p.Application.WithCompletedOriginalInput(b=>{Reject(()=>b.CheckSource(foreign.Field,foreign.Field.OriginalInkIdentity));return 0;});
 Require(!scope.IsFrom(foreign.Application));
}
Reject(()=>p.Application.CreatePriorSelectedScope(-1));
Reject(()=>p.Application.CreatePriorSelectedScope(Math2Mode0PriorSelectedScope.DefaultBudget+1));
Reject(()=>p.Application.CreatePriorSelectedScope(0));
Reject(()=>p.Application.CreatePriorSelectedScope(selected.Reservation-1));
Reject(()=>p.Application.CreatePriorSelectedScope(token:canceled.Token));
using(var exact=p.Application.CreatePriorSelectedScope(selected.Reservation))Require(exact.CopySnapshot().Reservation==selected.Reservation);
var detached=scope.CopySnapshot();detached.Registry[0].Ranges.AsSpan().Fill(default);detached.SelectedField.AsSpan().Fill(null!);
Require(scope.CopySnapshot().SelectedField[0] is not null&&scope.CopySnapshot().Registry[0].Ranges[0]!=default);
Require(JsonSerializer.Serialize(applied,PriorJson.Default.Math2Mode0OriginalSnapshot)==JsonSerializer.Serialize(p.Application.CopyResult(),PriorJson.Default.Math2Mode0OriginalSnapshot));
scope.Dispose();Reject(()=>scope.CopySnapshot());
using(var rolled=Pipeline.Create(runtime,run,bundle))
{rolled.Application.RunSource();rolled.Application.RollbackSource();Reject(()=>rolled.Application.CreatePriorSelectedScope());}
using(var failed=Pipeline.Create(runtime,run,bundle))
{failed.Application.RunSource();Reject(()=>failed.Application.ApplyDomDiagnostic(null,true));Reject(()=>failed.Application.CreatePriorSelectedScope());}
using(var closed=Pipeline.Create(runtime,run,bundle))
{closed.Application.RunSource();closed.Application.ApplyDom();using var child=closed.Application.CreatePriorSelectedScope();closed.Application.Dispose();Reject(()=>child.CopySnapshot());Reject(()=>closed.Application.CreatePriorSelectedScope());}
using(var closed=Pipeline.Create(runtime,run,bundle))
{closed.Application.RunSource();closed.Application.ApplyDom();using var child=closed.Application.CreatePriorSelectedScope();closed.Outgoing.Dispose();Reject(()=>child.CopySnapshot());}
using(var closed=Pipeline.Create(runtime,run,bundle))
{closed.Application.RunSource();closed.Application.ApplyDom();using var child=closed.Application.CreatePriorSelectedScope();closed.Field.Dispose();Reject(()=>child.CopySnapshot());}
File.WriteAllText(args[3],JsonSerializer.Serialize(new PriorEvidence(controls,selected,applied,runtime.Version,true,false,false),PriorJson.Default.PriorEvidence));
Console.WriteLine($"PASS {controls} genuine applied-prior ownership controls; independent selected DOM/Ink; no nonfresh inference/diff.");

sealed class Pipeline:IDisposable
{
 internal Math2Mode0OriginalMathFieldSource Field=null!;
 internal Math2Mode0RawOnlySource Source=null!;
 internal Math2Mode0SelectedMathFieldCopy Receipt=null!;
 internal Math2Mode0FreshMathPublisher Publisher=null!;
 internal Math2Mode0OutgoingTransfer Outgoing=null!;
 internal Math2Mode0OriginalApplication Application=null!;
 internal static Pipeline Create(OrtRuntime runtime,OrtRun run,Math2Mode0ColdBundle bundle,LegacyFreshRawInkCurrentView? sameView=null)
 {
  var p=new Pipeline();
  InkStroke[] ink=[new([new InkPoint(0,30),new(0,0),new(10,18),new(20,0),new(20,30)]),new([new InkPoint(35,0),new(35,30)])];
  var features=Math2Mode0RawSourcePositions.PrepareFeatures(ink,default);
  var view=sameView??LegacyFreshRawInkCurrentView.Prepare(features.Positions.Document);
  p.Field=Math2Mode0OriginalMathFieldSource.Prepare(view,"formula");
  p.Source=Math2Mode0RawOnlySource.PrepareColdWithSelectedMathField(runtime,bundle,run,p.Field);
  p.Source.RunToInitialOrder();p.Source.ApplyEf4();
  var output=p.Source.OrderFinal().FormatFullCold().AttachSourcePositions();
  p.Receipt=output.CopySelectedMathField(p.Field);p.Publisher=Math2Mode0FreshMathPublisher.Create(p.Receipt);
  p.Outgoing=Math2Mode0OutgoingTransfer.Create(p.Publisher);p.Application=Math2Mode0OriginalApplication.Prepare(p.Field,p.Outgoing);
  return p;
 }
 public void Dispose(){Application?.Dispose();Outgoing?.Dispose();Publisher?.Dispose();Receipt?.Dispose();Source?.Dispose();Field?.Dispose();}
}

record PriorEvidence(int Controls,Math2Mode0PriorSnapshot Scope,Math2Mode0OriginalSnapshot Original,string Runtime,bool ActualNN,bool PriorNN,bool NonfreshDiff);
[JsonSerializable(typeof(PriorEvidence))][JsonSerializable(typeof(Math2Mode0OriginalSnapshot))]
internal partial class PriorJson:JsonSerializerContext;
