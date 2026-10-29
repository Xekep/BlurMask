using InkRecognition;using InkRecognition.Math;using System.Text.Json;using System.Text.Json.Serialization;using System.Buffers.Binary;
if(args.Length!=7)throw new ArgumentException("pack rawresource ORT appendfixtures oldreference currentcontract output");int controls=0;
void Require(bool yes){if(!yes)throw new InvalidDataException($"Current selected assertion{controls}");controls++;}
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){controls++;return;}throw new InvalidDataException($"Current selected refusal{controls}");}
var fixtures=JsonSerializer.Deserialize(File.ReadAllText(args[3]),Json.Default.FixtureArray)!;using var oldReference=JsonDocument.Parse(File.ReadAllText(args[4]));using var expected=JsonDocument.Parse(File.ReadAllText(args[5]));
using var runtime=new OrtRuntime(args[2]);using var run=runtime.CreateRun(default);using var stream=File.OpenRead(args[0]);using var bundle=Math2Mode0ColdBundle.Read(runtime,stream,File.ReadAllBytes(args[1]));var actual=new List<Actual>();var unchanged=new List<Regression>();
for(int index=0;index<6;index++)
{
 var e=expected.RootElement.GetProperty("cases")[index];int width=e.GetProperty("oldPipeline").GetProperty("Width").GetInt32();bool reverse=e.GetProperty("oldPipeline").GetProperty("Reversed").GetBoolean();
 using var p=Pipeline.Create(runtime,run,bundle,width,reverse);p.Application.RunSource();p.Application.ApplyDom();
 // Existing unchanged-input C23/NN route stays exact; these are real models.
 using(var oldScope=p.Application.CreatePriorSelectedScope())using(var oldFreeze=Math2Mode0PriorFreezePreparation.Create(oldScope))using(var oldCurrent=Math2Mode0PriorC23Preparation.Create(oldScope,oldFreeze))
 {
  var prepared=oldCurrent.CopyResult();using var oldNn=width==1?Math2Mode0PriorRawOnlySource.PrepareCold(runtime,bundle,run,oldCurrent):Math2Mode0PriorRawOnlySource.PrepareDiagnostic(runtime,bundle,run,oldCurrent,width);oldNn.RunToFirstCopy();
  unchanged.Add(new(width,reverse,oldScope.CopySnapshot(),prepared,oldCurrent.CopyResult(),oldNn.CopyResult()));
 }
 var receipt=p.Application.AppendCurrentInput(new(fixtures[index].Append.Select(v=>new InkPoint(v[0],v[1])).ToArray()));
 var ownerSnapshot=p.Application.WithChangedInput(receipt,b=>b.CopySnapshot());var child=p.Application.WithChangedInput(receipt,b=>b.Document);var originalInk=p.Application.WithChangedInput(receipt,b=>b.OriginalInkIdentity);
 var originalC22=p.Application.WithChangedInput(receipt,b=>Enumerable.Range(0,4).Select(i=>b.SelectionIdentity(i)).ToArray());
 using var cancel=new CancellationTokenSource();cancel.Cancel();Reject(()=>p.Application.CreateCurrentSelectedScope(receipt,token:cancel.Token));Reject(()=>p.Application.CreateCurrentSelectedScope(receipt,0));Reject(()=>p.Application.CreateCurrentSelectedScope(new()));Reject(()=>p.Application.CreatePriorSelectedScope());
 using var scope=p.Application.CreateCurrentSelectedScope(receipt);var prior=scope.CopySnapshot();Require(prior.Next==4&&prior.Epoch==1);Require(!ReferenceEquals(scope.SelectedInkIdentity,originalInk)&&ReferenceEquals(scope.NameOwner,p.Field.OriginalNameOwner));
 for(int k=0;k<4;k++)
 {
  var selected=scope.SelectedC22Identity(k);Require(!ReferenceEquals(selected,originalC22[k]));var a=prior.Registry[k];var b=ownerSnapshot.Registry[k];Require(a.Id==b.Id&&a.Name==b.Name&&a.Category==b.Category&&a.Ranges.SequenceEqual(b.Ranges)&&a.CanonicalC22Id==b.CanonicalC22Id);
 }
 Require(!ReferenceEquals(scope.SelectedC22Identity(0),scope.SelectedC22Identity(3)));Require(prior.Registry.Select(r=>r.CanonicalC22Id).SequenceEqual(new[]{1,2,3,4}));
 Math2Mode0PriorSelectedScope.FreezeBorrow? escapedFreeze=null;
 scope.WithFreezeInput(b=>{escapedFreeze=b;Require(b.ChildCount>=1);Reject(()=>scope.WithFreezeInput(_=>0));return 0;});Reject(()=>escapedFreeze!.Check());
 Math2Mode0PriorSelectedScope.FreezeBorrow? exceptionFreeze=null;
 try{scope.WithFreezeInput<int>(b=>{exceptionFreeze=b;throw new IOException("Current freeze borrow-only exception");});throw new Exception();}catch(IOException){controls++;}
 Reject(()=>exceptionFreeze!.Check());Require(scope.CopySnapshot().Next==4);
 Reject(()=>Math2Mode0PriorFreezePreparation.Create(scope,cancel.Token));using var freeze=Math2Mode0PriorFreezePreparation.Create(scope);Require(freeze.CopyResult().Rows.Length==0);Reject(()=>Math2Mode0PriorFreezePreparation.Create(scope));
 Reject(()=>Math2Mode0PriorC23Preparation.Create(scope,freeze,cancel.Token));using var current=Math2Mode0PriorC23Preparation.Create(scope,freeze);var preparedCurrent=current.CopyResult();var identity=current.CopyIdentitySnapshot();
 Require(identity.Matrices==3&&identity.Nonidentity==0&&identity.MatrixBytes.Length==144);for(int i=0;i<3;i++)for(int j=0;j<6;j++)Require(BinaryPrimitives.ReadDoubleLittleEndian(identity.MatrixBytes.AsSpan(i*48+j*8))==(j is 0 or 4?1:0));
 Require(preparedCurrent.C23Ranges.SequenceEqual(prior.Registry[0].Ranges)&&preparedCurrent.WorkRanges.SequenceEqual(preparedCurrent.C23Ranges));Require(ReferenceEquals(current.Document,child)&&!ReferenceEquals(child,p.Field.Document)&&current.Document.ObjectCount==3);
 Require(ReferenceEquals(current.InputC22Identity,scope.SelectedC22Identity(0))&&ReferenceEquals(current.SelectedInkIdentity,scope.SelectedInkIdentity));Reject(()=>Math2Mode0PriorC23Preparation.Create(scope,freeze));
 using var next=width==1?Math2Mode0PriorRawOnlySource.PrepareCold(runtime,bundle,run,current):Math2Mode0PriorRawOnlySource.PrepareDiagnostic(runtime,bundle,run,current,width);
 Reject(()=>Math2Mode0PriorRawOnlySource.PrepareCold(runtime,bundle,run,current));next.RunToFirstCopy();var nn=next.CopyResult();Require(nn.Complete&&nn.FirstCopy is not null&&nn.DecoderCalls>0&&nn.Frames>0);Require(nn.Positions.Any(v=>(v>>32)==2));Require(nn.Order.Order().SequenceEqual(new[]{0,1,2}));Require(ReferenceEquals(next.SourceDocument,child));Reject(()=>next.RunToFirstCopy());
 using var candidate=Math2Mode0PriorCandidatePreparation.Create(next);var c=candidate.CopyResult();Require(c.RegistryNext==4&&c.Registry.Length==4&&c.Registry.Select(r=>r.CanonicalSelection).SequenceEqual(new[]{1,2,3,4}));Require(c.OldTags.SequenceEqual(new long[]{1,2,3}));Require(candidate.HasCanonicalNames&&!ReferenceEquals(candidate.ThirdInkIdentity,scope.SelectedInkIdentity));
 for(int k=0;k<4;k++){Require(!ReferenceEquals(candidate.ThirdSelectionIdentity(k),scope.SelectedC22Identity(k)));Require(c.Registry[k].Ranges.SequenceEqual(prior.Registry[k].Ranges));}
 using var diff=Math2Mode0PriorDomDiff.Create(candidate);var d=diff.CopyResult();Require(d.Complete&&d.Ready&&d.Produced==d.Kept+d.Filtered&&d.HeaderLive);Require(d.Actions.Length==d.Produced);Require(d.Kept>=0); // No predicted keep count or fake action.
 var beforeApply=candidate.CopyResult().Temporary;var beforeTree=ReadTree(candidate);
 var arenaBefore=p.Application.WithChangedInput(receipt,b=>b.AppliedArenaIdentity);
 using var temporary=Math2Mode0PriorTemporaryApply.Create(diff);var application=temporary.CopyResult();
 Require(application.CompletedActions==d.Kept&&application.HeaderConsumed&&!application.PartialActionMayExist);
 var afterTree=ReadTree(candidate);var currentAfter=p.Application.WithChangedInput(receipt,b=>b.CopySnapshot());
 bool sameArenaAfter=p.Application.WithChangedInput(receipt,b=>ReferenceEquals(arenaBefore,b.AppliedArenaIdentity));Require(sameArenaAfter);
 Require(!diff.CopyResult().HeaderLive);
 actual.Add(new(index,width,reverse,child.Strokes.ToArray(),ownerSnapshot,prior,freeze.CopyResult(),preparedCurrent,current.CopyResult(),identity,nn,c,d,beforeApply,beforeTree,afterTree,application,currentAfter,sameArenaAfter,diff.CopyResult().HeaderLive));
 next.ReleaseDecoder();Require(diff.CopyResult().Ready);Reject(()=>Math2Mode0PriorDomDiff.Create(candidate));
 if(index==0)p.Application.Dispose();else if(index==1)p.Field.Dispose();else if(index==2)scope.Dispose();else if(index==3)freeze.Dispose();else if(index==4)current.Dispose();else next.Dispose();
 if(index<5)Reject(()=>current.CopyResult());Reject(()=>next.CopyResult());Reject(()=>candidate.CopyResult());Reject(()=>diff.CopyResult());Reject(()=>temporary.CopyResult());
}
File.WriteAllText(args[6],JsonSerializer.Serialize(new Capture(controls,runtime.Version,unchanged.ToArray(),actual.ToArray()),Json.Default.Capture));Console.WriteLine($"PASS6 genuine current-selected/identity/C23/NEWNN/candidate/diff, plus6 unchanged regressions; {controls} controls; actual kept actions applied to temporary arena, no original update.");
TreeNode[] ReadTree(Math2Mode0PriorCandidatePreparation candidate)=>candidate.WithDiffInput(b=>
{
 var work=new Math2DomDiffWork(default);b.Views(work,out var root,out _);var rows=new List<TreeNode>();
 void Walk(Math2DomDiffNode n){rows.Add(new(n.Id,n.Kind,n.Name.Text,n.Scalar.Tag,n.Scalar.CopyStorage(),n.Scalar.CopyHeap(),n.Attributes.Select(a=>a.Id).ToArray(),n.Children.Select(c=>c.Id).ToArray()));foreach(var a in n.Attributes)Walk(a);foreach(var c in n.Children)Walk(c);}
 Walk(root);return rows.ToArray();
},default);
record Fixture(int Case,float[][] Append);
record Regression(int Width,bool Reversed,Math2Mode0PriorSnapshot Prior,Math2Mode0CurrentC23Snapshot Prepared,Math2Mode0CurrentC23Snapshot Finished,Math2Mode0PriorNnSnapshot NewNn);
record TreeNode(int Id,byte Kind,string Name,byte ValueTag,byte[] Storage,byte[] Heap,int[] Attributes,int[] Children);
record Actual(int Case,int Width,bool Reversed,InkStroke[] Ink,Math2Mode0CurrentInputSnapshot CurrentInput,Math2Mode0PriorSnapshot Prior,Math2Mode0FreezeSnapshot Freeze,Math2Mode0CurrentC23Snapshot Prepared,Math2Mode0CurrentC23Snapshot Finished,Math2Mode0CurrentIdentitySnapshot Identity,Math2Mode0PriorNnSnapshot NewNn,Math2Mode0PriorCandidateSnapshot Candidate,Math2Mode0DomDiffSnapshot Diff,Math2Mode0OriginalDomNode[] BeforeApply,TreeNode[] BeforeTree,TreeNode[] AfterTree,Math2Mode0TemporaryApplySnapshot Application,Math2Mode0CurrentInputSnapshot OriginalCurrentAfter,bool SameOriginalArenaAfter,bool DiffHeaderLiveAfter);
record Capture(int Controls,string Runtime,Regression[] Unchanged,Actual[] Actual);
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive=true)][JsonSerializable(typeof(Fixture[]))][JsonSerializable(typeof(Capture))]internal partial class Json:JsonSerializerContext;
