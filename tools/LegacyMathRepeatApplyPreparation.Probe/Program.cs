using InkRecognition;using InkRecognition.Math;using System.Text.Json;using System.Text.Json.Serialization;using System.Buffers.Binary;
if(args.Length!=7)throw new ArgumentException("pack rawresource ORT appendfixtures oldreference currentcontract output");int controls=0;
void Require(bool yes){if(!yes)throw new InvalidDataException($"Current selected assertion{controls}");controls++;}
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){controls++;return;}throw new InvalidDataException($"Current selected refusal{controls}");}
void Missing(Action a){try{a();}catch(KeyNotFoundException){controls++;return;}throw new InvalidDataException("Expected removed ID is still present.");}
var fixtures=JsonSerializer.Deserialize(File.ReadAllText(args[3]),Json.Default.FixtureArray)!;using var oldReference=JsonDocument.Parse(File.ReadAllText(args[4]));using var expected=JsonDocument.Parse(File.ReadAllText(args[5]));
using var runtime=new OrtRuntime(args[2]);using var run=runtime.CreateRun(default);using var stream=File.OpenRead(args[0]);using var bundle=Math2Mode0ColdBundle.Read(runtime,stream,File.ReadAllBytes(args[1]));var actual=new List<Actual>();var unchanged=new List<Regression>();Math2Mode0PriorLateRegistration? previousPhase=null;Math2Mode0PriorRepeatOutgoing? previousOutgoing=null;Math2Mode0RepeatApplyPreparation? previousPreparation=null;var leases=new List<LeaseObservation>();
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
 var afterTree=ReadTree(candidate);
 Reject(()=>Math2Mode0PriorLateRegistration.Create(temporary,cancel.Token));Reject(()=>Math2Mode0PriorLateRegistration.CreateDiagnostic(temporary,0,null));
 Math2Mode0PriorDomDiff.TemporaryApplyBorrow? escapedLate=null;temporary.WithLateInput(b=>{escapedLate=b;return 0;});Reject(()=>escapedLate!.Check());
 temporary.WithLateInput(b=>
 {
  Reject(()=>b.ResolveLate(null!,new(default),(_,_)=>{}));Reject(()=>b.RemoveLate(null!,Array.Empty<long>(),new(default),_=>{}));
  if(previousPhase is not null){Reject(()=>temporary.ClaimLate(b,previousPhase));Reject(()=>Math2Mode0PriorLateRegistration.ActiveBorrow.Issue(previousPhase,b));}
  return 0;
 });
 candidate.WithDiffInput(b=>{Reject(()=>b.ResolveLate(null!,null!,new(default),(_,_)=>{}));return 0;},default);
 var thirdBefore=Enumerable.Range(0,4).Select(i=>candidate.ThirdSelectionIdentity(i)).ToArray();
 using var late=Math2Mode0PriorLateRegistration.Create(temporary);var l=late.CopyResult();
 Require(l.Complete&&l.Ready&&!l.Failed&&!l.RegistrationInProgress&&l.Registrations==l.TagsWritten&&l.Observed is not null);
 Require(l.NewTags.All(t=>t>=0)&&l.Observed!.Next==4+l.Registrations);Reject(()=>Math2Mode0PriorLateRegistration.Create(temporary));
 for(int k=0;k<4;k++){if(l.Observed!.Registry.Any(r=>r.Id==k))Require(ReferenceEquals(thirdBefore[k],candidate.ThirdSelectionIdentity(k)));else Missing(()=>candidate.ThirdSelectionIdentity(k));}
 foreach(var terminal in l.Observed!.Registry.Where(r=>r.Name=="MATH_NODE"&&r.Ranges.SequenceEqual(l.Observed.Registry.Single(r=>r.Id==0).Ranges)))Require(ReferenceEquals(candidate.ThirdSelectionIdentity(0),candidate.ThirdSelectionIdentity(terminal.Id)));
 Require(scope.CopySnapshot().Next==4&&candidate.HasCanonicalNames);
 Reject(()=>Math2Mode0PriorLateRegistration.ActiveBorrow.Issue(late,escapedLate!));previousPhase=late;
 var afterLateTree=ReadTree(candidate);
 p.Application.WithChangedInput(receipt,b=>{var denied=new Math2DomDiffWork(default);Reject(()=>b.ReadRepeatOriginal(null!,null!,null!,denied));Require(denied.Visits==0);return 0;});
 Math2Mode0PriorLateRegistration.RepeatBorrow? expiredRepeat=null;
 late.WithRepeatInput(b=>{expiredRepeat=b;Reject(()=>Math2Mode0PriorLateRegistration.RepeatBorrow.Access.Issue(b,null,Math2Mode0PriorLateRegistration.RepeatBorrow.AccessKind.Plan));if(previousOutgoing is not null){var denied=new Math2DomDiffWork(default);Reject(()=>b.BuildClaimed(previousOutgoing,denied,null));Require(denied.Visits==0);}return 0;});
 Reject(()=>expiredRepeat!.Check());var expiredWork=new Math2DomDiffWork(default);Reject(()=>expiredRepeat!.Plan(Math2Mode0PriorRepeatOutgoing.DefaultBudget,expiredWork));Require(expiredWork.Visits==0);
 Reject(()=>Math2Mode0PriorRepeatOutgoing.Create(late,cancel.Token));Reject(()=>Math2Mode0PriorRepeatOutgoing.CreateDiagnostic(late,0,null));
 var oldReferences=p.Application.WithChangedInput(receipt,b=>Enumerable.Range(0,4).Select(i=>b.SelectionIdentity(i)).ToArray());var thirdReferences=l.Observed!.Registry.Select(r=>(r.Id,Identity:candidate.ThirdSelectionIdentity(r.Id))).ToArray();
 using var outgoingRepeat=Math2Mode0PriorRepeatOutgoing.Create(late);var export=outgoingRepeat.CopyResult();Require(export.Complete&&export.Ready&&!export.Failed&&!export.Released&&export.DistinctFourthNames&&export.SameCurrentProjection);
 Require(export.OldNext==4&&export.NewNext==6&&export.Epoch==1&&export.Geometry.Matrices==3&&export.Geometry.Nonidentity==0);
 Require(export.OriginalBefore.SequenceEqual(export.OriginalAfter)&&export.LateBefore.SequenceEqual(export.LateAfter));Require(export.RawActions.Length==export.Dom.Length&&export.Dom.All(d=>d.Operation is 2 or 4 or 5 or 6 or 7));
 for(int k=0;k<4;k++)Require(p.Application.WithChangedInput(receipt,b=>ReferenceEquals(oldReferences[k],b.SelectionIdentity(k))));foreach(var entry in thirdReferences)Require(ReferenceEquals(entry.Identity,candidate.ThirdSelectionIdentity(entry.Id)));
 Require(candidate.CopyResult().RegistryNext==6&&scope.CopySnapshot().Next==4);Reject(()=>Math2Mode0PriorRepeatOutgoing.Create(late));
 late.WithRepeatInput(b=>{var denied=new Math2DomDiffWork(default);Reject(()=>b.BuildClaimed(outgoingRepeat,denied,null));Require(denied.Visits==0);return 0;});
 var currentAfter=p.Application.WithChangedInput(receipt,b=>b.CopySnapshot());
 bool sameArenaAfter=p.Application.WithChangedInput(receipt,b=>ReferenceEquals(arenaBefore,b.AppliedArenaIdentity));Require(sameArenaAfter);
 Require(!diff.CopyResult().HeaderLive);
 actual.Add(new(index,width,reverse,child.Strokes.ToArray(),ownerSnapshot,prior,freeze.CopyResult(),preparedCurrent,current.CopyResult(),identity,nn,c,d,beforeApply,beforeTree,afterTree,application,currentAfter,sameArenaAfter,diff.CopyResult().HeaderLive,l,afterLateTree,export));
 // Payload lease proof only: no original mutation or receipt consumption.
 Reject(()=>Math2Mode0RepeatApplyPreparation.Prepare(outgoingRepeat,cancel.Token));Require(outgoingRepeat.CopyResult().Ready);
 p.Application.WithChangedInput(receipt,b=>{Reject(()=>Math2Mode0RepeatApplyPreparation.Prepare(outgoingRepeat));return 0;});
 late.WithRepeatInput(b=>{Reject(()=>Math2Mode0RepeatApplyPreparation.Prepare(outgoingRepeat));return 0;});
 if(previousPreparation is not null)
 {
  Reject(()=>outgoingRepeat.WithOriginalApplyPreparation(previousPreparation,new(default),default));
  Require(outgoingRepeat.CopyResult().Ready);
 }
 Reject(()=>Math2Mode0PriorRepeatOutgoing.OriginalPayloadLease.Issue(outgoingRepeat,null!));
 using var preparation=Math2Mode0RepeatApplyPreparation.Prepare(outgoingRepeat);
 var leasePrepared=preparation.CopyOwnedResult();Require(leasePrepared.Prepared&&leasePrepared.Ready&&!leasePrepared.Failed&&!leasePrepared.Released&&!leasePrepared.OldReceiptConsumed&&!leasePrepared.MutationEntered);
 Require(leasePrepared.DomOperations==export.Dom.Length&&leasePrepared.SourceCommands==export.Source.Length&&leasePrepared.RetainedNodes==export.FourthWriter.Length&&leasePrepared.RawObjects==3);
 Require(leasePrepared.OldEpoch==ownerSnapshot.AppliedEpoch&&leasePrepared.OldNext==ownerSnapshot.Next);
 Require(preparation.IsFor(outgoingRepeat));if(previousOutgoing is not null)Require(!preparation.IsFor(previousOutgoing));
 Reject(()=>Math2Mode0RepeatApplyPreparation.Prepare(outgoingRepeat));
 // Exact strict current read is still usable: preparing a payload has no C51/DOM effect.
 var stillCurrent=p.Application.WithChangedInput(receipt,b=>b.CopySnapshot());Require(stillCurrent.Next==4&&stillCurrent.AppliedEpoch==1&&stillCurrent.ObjectCount==3);
 for(int k=0;k<4;k++)Require(p.Application.WithChangedInput(receipt,b=>ReferenceEquals(oldReferences[k],b.SelectionIdentity(k))));
 Require(p.Application.CopySnapshot().ActivePage.SequenceEqual(export.OriginalAfter));
 outgoingRepeat.Dispose();Reject(()=>outgoingRepeat.CopyResult());
 var afterExporter=preparation.CopyOwnedResult();Require(afterExporter==leasePrepared);Require(outgoingRepeat.CopySnapshot().FourthWriter.Length==leasePrepared.RetainedNodes);
 // Releasing an ancestor cannot revive original read authority through the lease.
 if(index==0)late.Dispose();else if(index==1)temporary.Dispose();else if(index==2)candidate.Dispose();else if(index==3)next.Dispose();else if(index==4)p.Application.Dispose();else p.Field.Dispose();
 Reject(()=>late.CopyResult());var afterAncestor=preparation.CopyOwnedResult();Require(afterAncestor==leasePrepared);
 preparation.Dispose();Reject(()=>preparation.CopyOwnedResult());Require(preparation.CopySnapshot().Released);
 var afterRelease=outgoingRepeat.CopySnapshot();Require(afterRelease.FourthWriter.Length==0&&afterRelease.Dom.Length==0&&afterRelease.Source.Length==0);
 leases.Add(new(index,leasePrepared,afterExporter,afterAncestor,preparation.CopySnapshot(),afterRelease.FourthWriter.Length));
 previousPreparation=preparation;previousOutgoing=outgoingRepeat;
}

File.WriteAllText(args[6],JsonSerializer.Serialize(new Capture(controls,runtime.Version,unchanged.ToArray(),actual.ToArray(),[],[],leases.ToArray()),Json.Default.Capture));
Console.WriteLine($"PASS {controls} six genuine repeat-export payload-lease controls; no original mutation or apply.");
TreeNode[] ReadTree(Math2Mode0PriorCandidatePreparation candidate)=>candidate.WithDiffInput(b=>
{
 var work=new Math2DomDiffWork(default);b.Views(work,out var root,out _);var rows=new List<TreeNode>();
 void Walk(Math2DomDiffNode n){rows.Add(new(n.Id,n.Kind,n.Name.Text,n.Scalar.Tag,n.Scalar.CopyStorage(),n.Scalar.CopyHeap(),n.Attributes.Select(a=>a.Id).ToArray(),n.Children.Select(c=>c.Id).ToArray()));foreach(var a in n.Attributes)Walk(a);foreach(var c in n.Children)Walk(c);}
 Walk(root);return rows.ToArray();
},default);
record Fixture(int Case,float[][] Append);
record Regression(int Width,bool Reversed,Math2Mode0PriorSnapshot Prior,Math2Mode0CurrentC23Snapshot Prepared,Math2Mode0CurrentC23Snapshot Finished,Math2Mode0PriorNnSnapshot NewNn);
record TreeNode(int Id,byte Kind,string Name,byte ValueTag,byte[] Storage,byte[] Heap,int[] Attributes,int[] Children);
record Actual(int Case,int Width,bool Reversed,InkStroke[] Ink,Math2Mode0CurrentInputSnapshot CurrentInput,Math2Mode0PriorSnapshot Prior,Math2Mode0FreezeSnapshot Freeze,Math2Mode0CurrentC23Snapshot Prepared,Math2Mode0CurrentC23Snapshot Finished,Math2Mode0CurrentIdentitySnapshot Identity,Math2Mode0PriorNnSnapshot NewNn,Math2Mode0PriorCandidateSnapshot Candidate,Math2Mode0DomDiffSnapshot Diff,Math2Mode0OriginalDomNode[] BeforeApply,TreeNode[] BeforeTree,TreeNode[] AfterTree,Math2Mode0TemporaryApplySnapshot Application,Math2Mode0CurrentInputSnapshot OriginalCurrentAfter,bool SameOriginalArenaAfter,bool DiffHeaderLiveAfter,Math2Mode0LateSnapshot Late,TreeNode[] AfterLateTree,Math2Mode0RepeatOutgoingSnapshot Repeat);
record Failure(int Mode,Math2Mode0LateSnapshot Snapshot,Math2Mode0PriorCandidateSnapshot Candidate);
record ExportFailure(int Mode,Math2Mode0RepeatOutgoingSnapshot Snapshot,Math2Mode0CurrentInputSnapshot OriginalCurrent,Math2Mode0OriginalDomNode[] OriginalAfter,Math2Mode0PriorCandidateSnapshot Candidate);
record Capture(int Controls,string Runtime,Regression[] Unchanged,Actual[] Actual,Failure[] Failures,ExportFailure[] ExportFailures,LeaseObservation[] Leases);
record LeaseObservation(int Case,Math2Mode0RepeatApplyPreparationSnapshot Prepared,Math2Mode0RepeatApplyPreparationSnapshot AfterExporterDispose,Math2Mode0RepeatApplyPreparationSnapshot AfterAncestorDispose,Math2Mode0RepeatApplyPreparationSnapshot Released,int ExporterNodesAfterRelease);
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive=true)][JsonSerializable(typeof(Fixture[]))][JsonSerializable(typeof(Capture))]internal partial class Json:JsonSerializerContext;
