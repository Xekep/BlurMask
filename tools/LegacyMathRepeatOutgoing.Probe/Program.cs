using InkRecognition;using InkRecognition.Math;using System.Text.Json;using System.Text.Json.Serialization;using System.Buffers.Binary;
if(args.Length!=7)throw new ArgumentException("pack rawresource ORT appendfixtures oldreference currentcontract output");int controls=0;
void Require(bool yes){if(!yes)throw new InvalidDataException($"Current selected assertion{controls}");controls++;}
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){controls++;return;}throw new InvalidDataException($"Current selected refusal{controls}");}
void Missing(Action a){try{a();}catch(KeyNotFoundException){controls++;return;}throw new InvalidDataException("Expected removed ID is still present.");}
// Prepared numeric callback branch controls, never owning/application inputs.
for(int scenario=0;scenario<10;scenario++)
{
 var rows=new NumericRows(new[]{new NumericRow(2,8),new NumericRow(2,scenario is 4 or 5 or 7 or 8?-1:9)});var events=new List<string>();long written=-99;int registered=0;
 long value=scenario switch{0=>long.MaxValue,1=>0,2=>123,3=>-33,9=>long.MinValue,_=>-2};
 if(scenario==9)rows=new NumericRows(new[]{new NumericRow(long.MinValue,11)});
 bool answer=Math2Cb8LateNumericKernel.Resolve(value,rows,r=>r.Id,r=>r.Cached,(r,id)=>{r.Cached=id;events.Add("cache");},
 r=>{registered++;events.Add("register");return scenario is 4 or 7?-7:5;},id=>{written=id;events.Add("write");return scenario!=5;},new(default));
 if(scenario==0)Require(!answer&&rows.Reads==0&&events.Count==0);
 if(scenario is 1 or 2)Require(answer&&rows.Reads==0&&events.Count==0);
 if(scenario==3)Require(!answer&&registered==0&&events.Count==0);
 if(scenario is 4 or 7)Require(!answer&&registered==1&&rows.Items[^1].Cached==-7&&events.SequenceEqual(new[]{"register","cache"})&&written==-99);
 if(scenario==5)Require(!answer&&registered==1&&rows.Items[^1].Cached==5&&events.SequenceEqual(new[]{"register","cache","write"})&&written==5);
 if(scenario==6)Require(answer&&registered==0&&written==9&&events.SequenceEqual(new[]{"write"}));
 if(scenario==8)Require(answer&&registered==1&&written==5&&events.SequenceEqual(new[]{"register","cache","write"}));
 if(scenario==9)Require(answer&&registered==0&&written==11);
}
{
 var rows=new NumericRows(new[]{new NumericRow(2,-1)});var exhausted=new Math2DomDiffWork(default){Visits=Math2DomDiffWork.MaximumVisits-1};
 Reject(()=>Math2Cb8LateNumericKernel.Resolve(-2,rows,r=>r.Id,r=>r.Cached,(r,id)=>r.Cached=id,r=>5,id=>true,exhausted));Require(rows.Reads==0);
 using var cancelled=new CancellationTokenSource();cancelled.Cancel();Reject(()=>Math2Cb8LateNumericKernel.Resolve(-2,rows,r=>r.Id,r=>r.Cached,(r,id)=>r.Cached=id,r=>5,id=>true,new(cancelled.Token)));Require(rows.Reads==0);
 Reject(()=>Math2DomValueConversion.CollectedTag(-1));Require(Math2DomValueConversion.CollectedTag(long.MaxValue)==long.MaxValue);
}
var fixtures=JsonSerializer.Deserialize(File.ReadAllText(args[3]),Json.Default.FixtureArray)!;using var oldReference=JsonDocument.Parse(File.ReadAllText(args[4]));using var expected=JsonDocument.Parse(File.ReadAllText(args[5]));
using var runtime=new OrtRuntime(args[2]);using var run=runtime.CreateRun(default);using var stream=File.OpenRead(args[0]);using var bundle=Math2Mode0ColdBundle.Read(runtime,stream,File.ReadAllBytes(args[1]));var actual=new List<Actual>();var unchanged=new List<Regression>();Math2Mode0PriorLateRegistration? previousPhase=null;Math2Mode0PriorRepeatOutgoing? previousOutgoing=null;
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
 late.WithRepeatInput(b=>{var denied=new Math2DomDiffWork(default);Reject(()=>b.BuildClaimed(outgoingRepeat,denied,null));Require(denied.Visits==0);return 0;});previousOutgoing=outgoingRepeat;
 var currentAfter=p.Application.WithChangedInput(receipt,b=>b.CopySnapshot());
 bool sameArenaAfter=p.Application.WithChangedInput(receipt,b=>ReferenceEquals(arenaBefore,b.AppliedArenaIdentity));Require(sameArenaAfter);
 Require(!diff.CopyResult().HeaderLive);
 actual.Add(new(index,width,reverse,child.Strokes.ToArray(),ownerSnapshot,prior,freeze.CopyResult(),preparedCurrent,current.CopyResult(),identity,nn,c,d,beforeApply,beforeTree,afterTree,application,currentAfter,sameArenaAfter,diff.CopyResult().HeaderLive,l,afterLateTree,export));
 next.ReleaseDecoder();Require(diff.CopyResult().Ready);Reject(()=>Math2Mode0PriorDomDiff.Create(candidate));
 if(index==0)p.Application.Dispose();else if(index==1)p.Field.Dispose();else if(index==2)scope.Dispose();else if(index==3)freeze.Dispose();else if(index==4)current.Dispose();else next.Dispose();
 if(index<5)Reject(()=>current.CopyResult());Reject(()=>next.CopyResult());Reject(()=>candidate.CopyResult());Reject(()=>diff.CopyResult());Reject(()=>temporary.CopyResult());Reject(()=>late.CopyResult());Reject(()=>outgoingRepeat.CopyResult());
}
var failures=new List<Failure>();for(int mode=0;mode<6;mode++)RunFailure(mode);var exportFailures=new List<ExportFailure>();for(int mode=0;mode<6;mode++)RunExportFailure(mode);
File.WriteAllText(args[6],JsonSerializer.Serialize(new Capture(controls,runtime.Version,unchanged.ToArray(),actual.ToArray(),failures.ToArray(),exportFailures.ToArray()),Json.Default.Capture));Console.WriteLine($"PASS6 genuine current-selected/identity/C23/NEWNN/candidate/diff, plus6 unchanged regressions; {controls} controls; genuine late-owner repeat exports plus failed prefixes; no original update.");
void RunFailure(int mode)
{
 using var p=Pipeline.Create(runtime,run,bundle,1,false);p.Application.RunSource();p.Application.ApplyDom();
 var receipt=p.Application.AppendCurrentInput(new(fixtures[0].Append.Select(v=>new InkPoint(v[0],v[1])).ToArray()));
 using var scope=p.Application.CreateCurrentSelectedScope(receipt);using var freeze=Math2Mode0PriorFreezePreparation.Create(scope);using var current=Math2Mode0PriorC23Preparation.Create(scope,freeze);
 using var next=Math2Mode0PriorRawOnlySource.PrepareCold(runtime,bundle,run,current);next.RunToFirstCopy();using var candidate=Math2Mode0PriorCandidatePreparation.Create(next);using var diff=Math2Mode0PriorDomDiff.Create(candidate);
 using var temporary=Math2Mode0PriorTemporaryApply.Create(diff);var readOnly=temporary.WithLateInput(b=>b);using var cts=new CancellationTokenSource();Math2Mode0PriorLateRegistration? seen=null;bool fired=false;
 var stop=mode switch{0=>Math2Mode0LateStage.Registered,1=>Math2Mode0LateStage.TagWritten,2=>Math2Mode0LateStage.Removed,3=>Math2Mode0LateStage.Completed,4=>Math2Mode0LateStage.Registered,_=>Math2Mode0LateStage.Claimed};
 try
 {
  using var unexpected=Math2Mode0PriorLateRegistration.CreateDiagnostic(temporary,Math2Mode0PriorCandidatePreparation.DefaultBudget,(stage,phase)=>
  {
   seen=phase;Require(!phase.CopySnapshot().Ready);Reject(()=>phase.CopyResult());Reject(()=>Math2Mode0PriorLateRegistration.ActiveBorrow.Issue(phase,readOnly));
   if(stage!=stop||fired)return;fired=true;
   if(mode==1){cts.Cancel();return;}if(mode==4){temporary.Dispose();return;}
   if(mode==5){Reject(()=>Math2Mode0PriorLateRegistration.Create(temporary));throw new IOException("frozen claim observer failure");}
   throw new IOException("frozen late prefix observer failure");
  },cts.Token);
  throw new InvalidDataException("Frozen failure boundary not reached.");
 }
 catch(Exception ex)when(ex is IOException or OperationCanceledException or ObjectDisposedException){controls++;}
 Require(fired&&seen is not null);var snap=seen!.CopySnapshot();Require(snap.Failed&&!snap.Ready&&!snap.Released);Reject(()=>seen.CopyResult());
 if(mode is 0 or 4)Require(snap.Registrations==1&&snap.TagsWritten==0&&snap.Observed!.Next==5);
 if(mode==1)Require(snap.Registrations==1&&snap.TagsWritten==1&&snap.Observed!.Next==5);
 if(mode==2)Require(snap.Removed.Length==1&&snap.Observed!.Registry.All(r=>r.Id!=snap.Removed[0]));
 if(mode==3)Require(snap.Complete&&snap.TagsWritten>0&&snap.Removed.Length>0);
 if(mode==5)Require(snap.Registrations==0&&snap.TagsWritten==0&&snap.Observed!.Next==4);
 if(mode!=4){Reject(()=>Math2Mode0PriorLateRegistration.Create(temporary));temporary.CopyResult();}
 Reject(()=>Math2Mode0PriorLateRegistration.ActiveBorrow.Issue(seen,readOnly));failures.Add(new(mode,snap,candidate.CopySnapshot()));seen.Dispose();
}
void RunExportFailure(int mode)
{
 using var p=Pipeline.Create(runtime,run,bundle,1,false);p.Application.RunSource();p.Application.ApplyDom();
 var receipt=p.Application.AppendCurrentInput(new(fixtures[0].Append.Select(v=>new InkPoint(v[0],v[1])).ToArray()));using var scope=p.Application.CreateCurrentSelectedScope(receipt);using var freeze=Math2Mode0PriorFreezePreparation.Create(scope);using var current=Math2Mode0PriorC23Preparation.Create(scope,freeze);
 using var next=Math2Mode0PriorRawOnlySource.PrepareCold(runtime,bundle,run,current);next.RunToFirstCopy();using var candidate=Math2Mode0PriorCandidatePreparation.Create(next);using var diff=Math2Mode0PriorDomDiff.Create(candidate);using var temporary=Math2Mode0PriorTemporaryApply.Create(diff);using var late=Math2Mode0PriorLateRegistration.Create(temporary);
 var originalBefore=p.Application.CopySnapshot().ActivePage;var registryBefore=p.Application.WithChangedInput(receipt,b=>b.CopySnapshot());var lateBefore=candidate.CopyResult();
 using var cts=new CancellationTokenSource();Math2Mode0PriorRepeatOutgoing? seen=null;bool fired=false;
 var stop=mode switch{0=>Math2Mode0RepeatOutgoingStage.DomLinked,1=>Math2Mode0RepeatOutgoingStage.DomSerialized,2 or 3=>Math2Mode0RepeatOutgoingStage.SourceCommand,4=>Math2Mode0RepeatOutgoingStage.DomProduced,_=>Math2Mode0RepeatOutgoingStage.Completed};
 try
 {
  using var unexpected=Math2Mode0PriorRepeatOutgoing.CreateDiagnostic(late,Math2Mode0PriorRepeatOutgoing.DefaultBudget,(stage,value)=>
  {
   seen=value;Require(!value.CopySnapshot().Ready);Reject(()=>value.CopyResult());Reject(()=>Math2Mode0PriorRepeatOutgoing.Create(late));
   if(stage!=stop||fired)return;fired=true;if(mode==3){cts.Cancel();return;}if(mode==4){late.Dispose();return;}throw new IOException("Frozen repeat export prefix observer failure");
  },cts.Token);
  throw new InvalidDataException("Frozen repeat export failure boundary not reached.");
 }
 catch(Exception ex)when(ex is IOException or OperationCanceledException or ObjectDisposedException){controls++;}
 Require(fired&&seen is not null);var snapshot=seen!.CopySnapshot();Require(snapshot.Failed&&!snapshot.Ready&&!snapshot.Released);Reject(()=>seen.CopyResult());Require(originalBefore.SequenceEqual(p.Application.CopySnapshot().ActivePage));
 var registryAfter=p.Application.WithChangedInput(receipt,b=>b.CopySnapshot());Require(registryBefore.Next==registryAfter.Next&&registryBefore.InputSequence==registryAfter.InputSequence);var after=candidate.CopySnapshot();
 Require(lateBefore.RegistryNext==after.RegistryNext&&lateBefore.Temporary.SequenceEqual(after.Temporary));
 if(mode==0)Require(snapshot.RawActions.Length==1&&snapshot.Dom.Length==0&&snapshot.Source.Length==0);
 if(mode==1)Require(snapshot.Dom.Length>0&&snapshot.Source.Length==0);
 if(mode is 2 or 3)Require(snapshot.Source.Length==1&&snapshot.Source[0].Operation==4);
 if(mode==4)Require(snapshot.RawActions.Length>0&&snapshot.Dom.Length==0);
 if(mode==5)Require(snapshot.Complete&&snapshot.Source[^1].Operation==7);
 exportFailures.Add(new(mode,snapshot,registryAfter,p.Application.CopySnapshot().ActivePage,after));seen.Dispose();
}
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
record Capture(int Controls,string Runtime,Regression[] Unchanged,Actual[] Actual,Failure[] Failures,ExportFailure[] ExportFailures);
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive=true)][JsonSerializable(typeof(Fixture[]))][JsonSerializable(typeof(Capture))]internal partial class Json:JsonSerializerContext;

sealed class NumericRow(long id,long cached){internal readonly long Id=id;internal long Cached=cached;}
sealed class NumericRows(NumericRow[] rows):IReadOnlyList<NumericRow>
{
 internal readonly NumericRow[] Items=rows;internal int Reads;
 public int Count=>Items.Length;public NumericRow this[int i]{get{Reads++;return Items[i];}}
 public IEnumerator<NumericRow> GetEnumerator()=>((IEnumerable<NumericRow>)Items).GetEnumerator();System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()=>GetEnumerator();
}
