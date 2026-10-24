using InkRecognition;
using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
if(args.Length!=11)throw new ArgumentException("AK LK engine separation case publication normalization circles line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);using var nf=File.OpenRead(args[6]);using var cr=File.OpenRead(args[7]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var norm=LegacyNativeUnicodeNormalizationData.Load(nf);var circles=LegacyNativeCircledPayloadData.Load(cr);
byte[] line30=File.ReadAllBytes(args[8]),line50=File.ReadAllBytes(args[9]);
var models=LegacyFreshMainRecognitionContext.Load(ak,lk,engine,line30,line50);

using var file=File.Create(args[10]);using var w=new Utf8JsonWriter(file);int guards=0;
InkStroke[] m=[new(new InkPoint[]{new(0,90),new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)})];
InkStroke[][] samples=[m,Word(0).Concat(Word(300)).ToArray(),Word(450).Concat(Word(0)).ToArray()];
w.WriteStartObject();w.WriteStartArray("actual");
for(int i=0;i<6;i++)
{
 var (scope,words)=Recognize(samples[i/2],i%2);using(scope)
 {
  Reject(()=>LegacyLibraryTextPublicationDocument.Prepare(scope,words));
  Reject(()=>LegacyLibraryTextPublicationDocument.PrepareOriginalSelected(scope,words,new(true)));
  using var owner=LegacyLibraryTextPublicationDocument.PrepareOriginalSelected(scope,words);
  Reject(()=>LegacyLibraryTextPublicationDocument.PrepareOriginalSelected(scope,words));
  Reject(()=>owner.BuildAndTransfer(new(true)));scope.Context.Dispose();
  var before=owner.BuildAndTransfer();Reject(()=>owner.BuildAndTransfer());
  var selected=scope.Context.OriginalSelectedInput!;var handoff=owner.ActualSources.CounterHandoff;
  var transforms=handoff.Transforms;var entry=selected.State.Registry.Single(x=>x.RegistryId==selected.OriginalInput.RegistryId);
  if(!ReferenceEquals(transforms.OriginalSelectedSourceInput,entry)||!ReferenceEquals(transforms.SourceGeometry.Owner,selected.CurrentView)||
    !ReferenceEquals(transforms.ReleasedVectorOwner,scope.Seed.InitialTransforms)||
    !ReferenceEquals(transforms.VectorOwner,scope.Seed.CurrentTransforms)||ReferenceEquals(transforms.VectorOwner,scope.Seed.InitialTransforms)||
    handoff.InitialPendingCounter!=2||!ReferenceEquals(handoff.TransactionOwner.InputBinding,scope.Context.InputBinding)||
    !ReferenceEquals(owner.ActualTransportRow!.Owner.OriginalSelectedInputContext,selected))throw new InvalidDataException("Actual selected export/vector/C4D transport ownership differs.");
  Reject(()=>{_=transforms.SourceExport;});Reject(()=>{_=owner.ActualTransportRow!.Owner.SelectedInputContext;});
  Reject(()=>LegacyFreshMainDocumentInputTransforms.Apply(transforms.Cleanup));
  Reject(()=>LegacyFreshMainDocumentInputTransforms.ApplyOriginalSelected(transforms.Cleanup));
  var replay=LegacyLibrarySelectedInputRegistryReplay.PrepareOriginalSelected(owner);Reject(()=>owner.ApplyToLibraryTarget());
  var initial=replay.State;var original=LegacyLibraryOriginalInputPatchCompatibility.PrepareOriginalSelected(replay);
  var dom=LegacyLibraryTextDomPatchCompatibility.Prepare(owner);
  Reject(()=>replay.Run());Reject(()=>owner.ApplyToLibraryTarget());Reject(()=>original.Run(new(true)));
  if(!ReferenceEquals(replay.OriginalSelectedInitialContext,selected)||!ReferenceEquals(replay.OriginalSelectedInput,entry)||
     !ReferenceEquals(original.OriginalBootstrap,selected.Original)||!ReferenceEquals(original.OriginalBootstrapInput,selected.OriginalInput)||
     !replay.InitialRegistryIds.SequenceEqual(new long[]{0,1}))throw new InvalidDataException("All selected registry/original identities required.");
  Reject(()=>{_=replay.InitialContext;});Reject(()=>{_=replay.Input;});Reject(()=>{_=original.OriginalContext;});Reject(()=>{_=original.OriginalInput;});
  var other=LegacyLibraryOriginalTextFieldBootstrap.Prepare(selected.CurrentView,new[]{"other"});other.Run();
  Reject(()=>LegacyLibraryOriginalInputPatchCompatibility.ValidateOriginalSelected(replay,other,other.State.Inputs[0]));
  using(var foreign=LegacyLibraryTextPublicationScope.PrepareConfiguredOriginalSelected(models,selected,0,table,cases,publication))
   Reject(()=>new LegacyLibrarySelectedInputRegistryReplay(owner,foreign,replay.TransportRow,handoff,default));
  var o=original.Run();if(!o.IsReady)throw new InvalidDataException("Actual original domain preflight failed.");
  Reject(()=>original.Run());Reject(()=>replay.Run());var d=dom.Run();if(!d.IsReady)throw new InvalidDataException("Actual DOM scratch preflight failed.");
  Reject(()=>dom.Run());Reject(()=>replay.Run(new(true)));var state=replay.Run();Reject(()=>replay.Run());
  if(!state.Succeeded||state.IsFaulted||state.NextRegistryId!=handoff.FinalPendingCounter||state.MaximumRegistryId!=state.NextRegistryId-1||
    selected.State.NextRegistryId!=2||initial.NextRegistryId!=2||initial.Selections.Count!=0)throw new InvalidDataException("Selected replay mutated historical registry or lost counters.");
  foreach(var s in state.Selections)
  {
   Reject(()=>{_=s.TargetContext;});
   if(!ReferenceEquals(s.OriginalSelectedTargetContext,selected)||!ReferenceEquals(s.SourceCommand.Owner,scope.Seed.PendingSelections)||
     !s.CopyRangeStorage16().AsSpan().SequenceEqual(Storage(s.SourceCommand.CopyRanges())))throw new InvalidDataException("Actual cloned command provenance required.");
  }
  var after=owner.ApplyToLibraryTarget();Reject(()=>owner.ApplyToLibraryTarget());
  w.WriteStartObject();w.WriteNumber("sample",i/2);w.WriteNumber("chosen",i%2);Metadata(owner);Input(scope,transforms);State(replay.State);
  w.WriteBase64String("originalDomain",Storage(original.CopyOriginalInkDomain()));w.WriteBase64String("originalPresence",o.CopyRawPresence());w.WriteBase64String("originalOutput",o.CopyOutputBytes());
  w.WritePropertyName("before");Snapshot(before);w.WritePropertyName("after");Snapshot(after);w.WriteEndObject();
  var row=owner.ActualTransportRow;owner.Dispose();Reject(()=>replay.Run());if(!row.IsReleased||scope.Context.ResourcesReleased)throw new InvalidDataException("Borrowed owner released prematurely.");
  scope.Dispose();Reject(()=>scope.Seed.Validate(words.Owner,default));if(!scope.Context.ResourcesReleased)throw new InvalidDataException("Retained context lease not released.");
 }
}
w.WriteEndArray();w.WriteStartArray("faults");
for(int mode=0;mode<5;mode++)
{
 var (scope,words)=Recognize(m,mode%2);using(scope)using(var owner=LegacyLibraryTextPublicationDocument.PrepareOriginalSelected(scope,words))
 {
  w.WriteStartObject();w.WriteNumber("mode",mode);
  if(mode<2)
  {
   owner.BuildPrepared(domFailure:mode==0?0:-1,exportFailure:mode==1?0:-1);Reject(()=>owner.BuildAndTransfer());Reject(()=>owner.ApplyToLibraryTarget());
   Metadata(owner);w.WritePropertyName("prefix");Snapshot(owner.Snapshot);
  }
  else if(mode==2)
  {
   Reject(()=>owner.BuildPrepared(transferFailure:true));var prefix=owner.Snapshot;
   if(owner.IsFaulted||owner.ActualTransfer!.PendingEdit is null||owner.ActualTransfer.PendingTransaction is null)throw new InvalidDataException("Transfer reserve consumed slots.");
   Reject(()=>owner.TransferPending(new(true)));var recovered=owner.TransferPending();Metadata(owner);
   w.WritePropertyName("prefix");Snapshot(prefix);w.WritePropertyName("recovered");Snapshot(recovered);
  }
  else if(mode==3)
  {
   owner.BuildAndTransfer();Reject(()=>owner.ApplyPrepared(failure:0));Reject(()=>owner.ApplyToLibraryTarget());
   Metadata(owner);w.WritePropertyName("prefix");Snapshot(owner.Snapshot);
  }
  else
  {
   owner.BuildAndTransfer();scope.Dispose();Reject(()=>owner.ApplyToLibraryTarget());Reject(()=>owner.BuildAndTransfer());
   w.WritePropertyName("prefix");Snapshot(owner.Snapshot);
  }
  w.WriteEndObject();
 }
}
w.WriteEndArray();w.WriteStartArray("replayFaults");
for(int mode=0;mode<6;mode++)
{
 var (scope,words)=Recognize(m,mode%2);using(scope)using(var owner=LegacyLibraryTextPublicationDocument.PrepareOriginalSelected(scope,words))
 {
  owner.BuildAndTransfer();var replay=LegacyLibrarySelectedInputRegistryReplay.PrepareOriginalSelected(owner);
  int fail=-1,cancel=-1;bool preparedCompatibility=false;LegacyLibraryOriginalInputPatchCompatibility? compatibility=null;
  if(mode<3)
  {
   int n=replay.TransactionOwner.CopyPendingSelections().Count;
   if(mode<2){fail=mode==0?0:n/2;replay.RunPrepared(failCloneAtCommand:fail);}
   else{cancel=n/2;Reject(()=>replay.RunPrepared(cancelBeforeCommand:cancel));}
  }
  else
  {
   compatibility=LegacyLibraryOriginalInputPatchCompatibility.PrepareOriginalSelected(replay);preparedCompatibility=true;
   if(mode<5)compatibility.RunPrepared(failureStage:mode-3);else compatibility.RunPrepared(incompatibleAtCommand:0);
   Reject(()=>replay.Run());
  }
  Reject(()=>owner.ApplyToLibraryTarget());
  if(mode<3)Reject(()=>replay.Run());
  w.WriteStartObject();w.WriteNumber("mode",mode);w.WriteNumber("fail",fail);w.WriteNumber("cancel",cancel);w.WriteBoolean("preparedCompatibility",preparedCompatibility);
  w.WriteStartArray("commands");foreach(var c in owner.Snapshot.PendingCommands)Command(c);w.WriteEndArray();State(replay.State);
  if(compatibility is not null){w.WriteBase64String("compatibilityOutput",compatibility.State.CopyOutputBytes());w.WriteBoolean("worked",compatibility.State.Worked);w.WriteBoolean("ready",compatibility.State.IsReady);}
  w.WriteEndObject();
 }
}
w.WriteEndArray();
{
 var (scope,words)=Recognize(m,1);using(scope)
 {
  var text=LegacyFreshMainDocumentTextSelection.Prepare(LegacyFreshMainSourceLinePublication.PublishFresh(words));text.Register();
  var state=LegacyFreshMainDocumentSelectedSources.Prepare(LegacyFreshMainDocumentCharacterCoverage.Prepare(LegacyFreshMainDocumentSourceCoverage.Prepare(LegacyFreshMainDocumentLineConsistency.Apply(text))));
  var indices=LegacyFreshMainDocumentSourceIndices.Prepare(state);indices.Run();var cleanup=LegacyFreshMainDocumentControlCleanup.Prepare(indices);cleanup.Run();
  var initial=scope.Seed.CurrentTransforms;var bytes=Enumerable.Range(0,64).Select(i=>(byte)(i^0xA7)).ToArray();
  var prefix=LegacyDocumentTransformVector.Prepared(bytes,[scope.Context.InputBinding.CopyRanges()]);
  Reject(()=>LegacyFreshMainDocumentInputTransforms.ApplyOriginalSelected(cleanup,new(true)));
  var failed=LegacyFreshMainDocumentInputTransforms.ApplyPreparedExport(cleanup,prefix,false);
  if(failed.Succeeded||failed.FollowingStage026EF0Executed||!ReferenceEquals(failed.VectorOwner,prefix)||
    !ReferenceEquals(scope.Seed.CurrentTransforms,prefix)||!ReferenceEquals(failed.ReleasedVectorOwner,initial)||
    !ReferenceEquals(failed.OriginalSelectedSourceInput,scope.Context.InputBinding.OriginalSelectedEntry))
   throw new InvalidDataException("Failed original-selected export lost unconditional same-seed swap or source provenance.");
  Reject(()=>LegacyFreshMainDocumentInputTransforms.ApplyOriginalSelected(cleanup));Reject(()=>LegacyFreshMainDocumentCounterHandoff.Apply(failed));
  w.WriteStartObject("transformFault");w.WriteBase64String("exported64",failed.CopyTransformScalarStorage64());w.WriteNumber("count",failed.TransformCount110);w.WriteNumber("daf",failed.DafInputCount);w.WriteBoolean("success",failed.Succeeded);w.WriteBoolean("following",failed.FollowingStage026EF0Executed);w.WriteNumber("tag",failed.OriginalSelectedSourceInput!.RegistryId);w.WriteEndObject();
 }
}
w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS6 original-selected documents/registry-all replay+DOM application,5 transport/DOM prefixes+6 registry/preflight prefixes/{guards} guards; no CBB acceptance.");
(LegacyLibraryTextPublicationScope,LegacyOwnedMainWordSourceChoicesResult) Recognize(InkStroke[] raw,int chosen)
{
 var doc=LegacyOwnedRawInkDocument.Create(raw,default);var original=LegacyLibraryOriginalTextFieldBootstrap.Prepare(LegacyFreshRawInkCurrentView.Prepare(doc),new[]{"left","right"});original.Run();
 var selected=LegacyLibraryOriginalSelectedFieldContext.Prepare(original,original.State.Inputs[chosen]);selected.Run();
 var scope=LegacyLibraryTextPublicationScope.PrepareConfiguredOriginalSelected(models,selected,0,table,cases,publication);
 try
 {
  var residue=new LegacyMainConfiguredAlternativeResidue(new byte[120],new byte[72],new byte[40],new ushort[256],new byte[64],new byte[4],new byte[4],new byte[4],new byte[4]);
  var result=scope.Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]);
  bool enabledAugmentationRefused=false;
 try{_ = LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(ak,result,norm,circles,new byte[4],new byte[4]);}
 catch(NotSupportedException e)when(e.GetType()==typeof(NotSupportedException)&&e.Message=="Enabled219580 candidate augmentation cannot be omitted."){enabledAugmentationRefused=true;}
 if(!enabledAugmentationRefused)throw new InvalidDataException("Exact enabled-bit8 NoCandidate refusal required before augmentation.");
 var completion=LegacyFreshMainConfiguredWordAugmentation.Prepare(ak,result,table,norm,circles,residue);completion.Run();
 if(!completion.IsTopV68Completed||completion.IsFaulted||!completion.SourcesMarkedComplete||
    completion.EligibleCandidates!=2||completion.AppendedWords!=0||completion.DuplicateWords!=2||completion.UpdatedNewDuplicates!=0||completion.QuotaAfter!=2)
  throw new InvalidDataException("Genuine fixed-fixture augmentation must complete the duplicate-only native mark/rank prefix.");
  return(scope,scope.PrepareAugmentedWords(completion,norm,circles).Publish(new byte[40],new byte[2040*4]));
 }
 catch{scope.Dispose();throw;}
}
void Input(LegacyLibraryTextPublicationScope scope,LegacyFreshMainDocumentInputTransforms t)
{
 var s=scope.Context.OriginalSelectedInput!;w.WriteNumber("sourceTag",scope.Seed.SourceTag30);w.WriteNumber("initialNext",s.State.NextRegistryId);w.WriteNumber("initialMax",s.State.MaximumRegistryId);
 w.WriteStartArray("allInitialIds");foreach(var entry in s.State.Registry)w.WriteNumberValue(entry.RegistryId);w.WriteEndArray();
 w.WriteBase64String("inputRanges",Storage(t.OriginalSelectedSourceInput!.CopyRanges()));w.WriteBase64String("matrices48",t.SourceGeometry.CopyResolvedMatrices());
 w.WriteStartArray("matches");foreach(int id in t.SourceGeometry.CopyMatchedViewIndices())w.WriteNumberValue(id);w.WriteEndArray();
 w.WriteStartArray("counts");foreach(var stroke in scope.Document.Strokes)w.WriteNumberValue(stroke.Points.Count);w.WriteEndArray();
 w.WriteBase64String("seed20",scope.Seed.CopyInitialScalarStorage20());w.WriteNumber("transforms",t.TransformCount110);w.WriteNumber("daf",t.DafInputCount);
}
void State(LegacyLibrarySelectedInputRegistryState state)
{
 w.WriteStartObject("registry");w.WriteBoolean("success",state.Succeeded);w.WriteBoolean("faulted",state.IsFaulted);w.WriteNumber("completed",state.CompletedCommandCount);w.WriteBase64String("counter16",state.CopyCounterStorage16());
 w.WriteStartArray("initialIds");foreach(long id in state.InitialRegistryIds)w.WriteNumberValue(id);w.WriteEndArray();w.WriteStartArray("selections");
 foreach(var s in state.Selections){w.WriteStartObject();w.WriteNumber("command",s.SourceCommand.CommandIdentity);w.WriteNumber("id",s.RegistryId);w.WriteString("name",s.Name);w.WriteNumber("category",s.Category);w.WriteBase64String("ranges16",s.CopyRangeStorage16());w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
}
void Command(LegacyLibraryTextPublicationCommandSnapshot c)
{
 w.WriteStartObject();w.WriteNumber("operation",c.Operation);w.WriteNumber("id",c.Identity);w.WriteNumber("next",c.NextIdentity);
 if(c.SelectionId is{} id){w.WriteNumber("tag",id);w.WriteString("name",c.Name);w.WriteNumber("category",c.Category!.Value);w.WriteBase64String("ranges16",Storage(c.CopyRanges()));}else w.WriteBase64String("body56",c.CopyCounterBody56()!);w.WriteEndObject();
}
void Snapshot(LegacyLibraryTextPublicationDocumentSnapshot s)
{
 w.WriteStartObject();w.WriteBoolean("faulted",s.IsFaulted);w.WriteBoolean("transferred",s.WasTransferred);w.WriteBoolean("applied",s.WasAppliedToLibraryTarget);
 w.WriteStartArray("steps");foreach(string v in s.Steps)w.WriteStringValue(v);w.WriteEndArray();
 w.WritePropertyName("writer");if(s.WriterRoot is{} wr)WriteNode(wr);else w.WriteNullValue();w.WritePropertyName("target");if(s.TargetRoot is{} tr)WriteNode(tr);else w.WriteNullValue();
 w.WriteStartArray("edit40");foreach(var row in s.CopyEditScalarRows40())w.WriteBase64StringValue(row);w.WriteEndArray();var t=s.CopyTransportScalarRow40();if(t is null)w.WriteNull("transport40");else w.WriteBase64String("transport40",t);
 w.WriteStartArray("commands");foreach(var c in s.PendingCommands)
 {w.WriteStartObject();w.WriteNumber("operation",c.Operation);w.WriteNumber("id",c.Identity);w.WriteNumber("next",c.NextIdentity);if(c.SelectionId is{} id){w.WriteNumber("tag",id);w.WriteString("name",c.Name);w.WriteNumber("category",c.Category!.Value);w.WriteBase64String("ranges16",Storage(c.CopyRanges()));}else w.WriteBase64String("body56",c.CopyCounterBody56()!);w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
}

void Metadata(LegacyLibraryTextPublicationDocument owner)
{
 var source=owner.ActualSources;var state=source.CounterHandoff.Transforms.Cleanup.State;var choices=state.Text.WordChoices;
 w.WriteBase64String("uuid36",source.Seed.LibraryIdentifier38.CopyUtf8Storage36());
 w.WriteStartArray("characters");foreach(var s in source.Sources.Where(s=>s.CharacterSource is not null))
 {
  var p=s.CharacterSource!;w.WriteStartObject();w.WriteNumber("id",s.SourceNodeIdentity);w.WriteNumber("flags",s.SourceFlags);w.WriteNumber("tag",p.SelectionId);w.WriteNumber("line",p.SourceLineId);w.WriteNumber("freeze",p.Field1C);w.WriteNumber("force",unchecked((int)p.Field40));w.WriteBase64String("ranges16",Storage(s.CopyOriginalRanges()));
  w.WriteStartArray("records40");foreach(var row in p.CopyRecords()){w.WriteStartObject();w.WriteBase64String("raw",row.CopyStorage40());w.WriteBase64String("text",row.Text!.ReadBytes());w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
 }
 w.WriteEndArray();var word=source.Sources.Single(s=>s.WordSource is not null);w.WriteStartObject("word");w.WriteNumber("id",word.SourceNodeIdentity);w.WriteNumber("flags",word.SourceFlags);w.WriteNumber("tag",word.WordSource!.SelectionId);w.WriteBase64String("ranges16",Storage(word.CopyOriginalRanges()));w.WriteStartArray("rows48");
 foreach(var row in word.WordSource.Choices)
 {w.WriteStartObject();w.WriteBase64String("raw",row.CopyScalarStorage48());w.WriteStartArray("refs24");foreach(var link in row.CharacterLinks){w.WriteStartObject();w.WriteNumber("charId",link.Source.SourceNode.Identity);w.WriteBase64String("raw",link.CopyScalarStorage24());w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();}
 w.WriteEndArray();w.WriteEndObject();w.WriteStartObject("text");w.WriteNumber("tag",state.Text.Selection.SelectionId);w.WriteNumber("selected",owner.ActualDom.NativeTextSelectedBC);w.WriteNumber("freeze",owner.ActualDom.NativeTextFreezeB8);w.WriteBase64String("ranges16",Storage(state.Text.CopyRanges()));w.WriteStartArray("rows32");foreach(var a in choices.Alternatives){w.WriteStartObject();w.WriteBase64String("raw",a.CopyScalarStorage32());w.WriteBase64String("ref24",a.CopyScalarStorage24());w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();w.WriteBase64String("lines56",state.CharacterCoverage.SourceCoverage.LineConsistency.CopyStorage56());

}
void WriteNode(LegacyLibraryTextDomNode n){w.WriteStartObject();w.WriteNumber("id",n.Identity);w.WriteNumber("kind",n.Kind);w.WriteString("name",n.Name);w.WriteNumber("nameId",n.NameIdentity);w.WriteNumber("tag",n.Value.NativeTag);w.WriteBase64String("value",n.Value.CopyBytes());w.WriteStartArray("attributes");foreach(var a in n.Attributes)WriteNode(a);w.WriteEndArray();w.WriteStartArray("children");foreach(var c in n.Children)WriteNode(c);w.WriteEndArray();w.WriteEndObject();}
byte[] Storage(LegacyNativeInkPositionRange[] ranges){var b=new byte[ranges.Length*16];for(int i=0;i<ranges.Length;i++){BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(i*16),ranges[i].First.Value);BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(i*16+8),ranges[i].Last.Value);}return b;}
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}

void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException or NotSupportedException){guards++;return;}throw new InvalidDataException("Missing document lifetime/failure guard.");}
