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
for(int i=0;i<14;i++)
{
 int mode=i<6||i>=12?-1:i-6;int chosen=i==13?2:i==12?0:i%2;
 var (scope,words,history)=Recognize(samples[i<6?i/2:0],chosen,i==12?1:i==13?3:2);
 using(scope)using(history)using(var document=LegacyLibraryTextPublicationDocument.PrepareOriginalSelected(scope,words))
 {
  document.BuildAndTransfer();var replay=LegacyLibrarySelectedInputRegistryReplay.PrepareOriginalSelected(document);
  var compatibility=LegacyLibraryOriginalInputPatchCompatibility.PrepareOriginalSelected(replay);
  var dom=LegacyLibraryTextDomPatchCompatibility.Prepare(document);
  Reject(()=>history.PrepareApplication(document,compatibility));compatibility.Run();
  Reject(()=>history.PrepareApplication(document,compatibility));dom.Run();
  var foreign=LegacyLibraryOriginalTextFieldBootstrap.Prepare(history.CurrentView,new[]{"other"});foreign.Run();
  using(var h=LegacyLibraryOriginalSourceHistory.Prepare(foreign))Reject(()=>h.PrepareApplication(document,compatibility));
  var app=history.PrepareApplication(document,compatibility);
  if(!ReferenceEquals(app,document.OriginalSourceApplication)||!ReferenceEquals(history,history.Original.SourceHistory)||
   !ReferenceEquals(app.TransactionOwner,replay.TransactionOwner)||!ReferenceEquals(history.Document,scope.Document))throw new InvalidDataException("Lost original/pre-NN/receipt ownership.");
  Reject(()=>history.PrepareApplication(document,compatibility));
  Reject(()=>app.Rollback());Reject(()=>app.Run(new(true)));
  replay.Run();Reject(()=>document.ApplyToLibraryTarget());
  int n=app.TransactionOwner.CopyPendingSelections().Count,half=n/2;
  LegacyLibraryOriginalSourceState forward;
  if(mode==0)forward=app.RunPrepared(failTemporaryClone:half);
  else if(mode==1)forward=app.RunPrepared(failObserverHistory:half);
  else if(mode==2){Reject(()=>app.RunPrepared(cancelBeforeCommand:half));forward=app.State;}
  else forward=app.Run();
  Reject(()=>app.Run());if(!ReferenceEquals(forward,app.State)||!ReferenceEquals(forward,history.ActiveState))throw new InvalidDataException("Snapshot transport mismatch.");
  foreach(var e in forward.Registry.Where(e=>e.SourceCommand is not null))
  {
   if(!ReferenceEquals(e.RangesOwner.Owner,history.Original)||ReferenceEquals(e.RangesOwner,scope.Context.OriginalSelectedInput!.State.Registry[0].RetainedSelection))throw new InvalidDataException("Original/worker C22 targets conflated.");
  }
  foreach(var j in forward.GeneratedJournal)
   if(!ReferenceEquals(j.Owner,history)||!ReferenceEquals(j.CopiedSelection.Owner,history.Original)||
      ReferenceEquals(j.CopiedSelection,forward.Registry.Single(e=>e.RegistryId==j.RegistryId).RangesOwner))throw new InvalidDataException("Generated history clone shares registry C22.");
  bool undo=(i<6&&i%2==1)||mode is 0 or 2 or 3;
  bool applied=false;
  if(mode==1){Reject(()=>app.Rollback());Reject(()=>document.ApplyToLibraryTarget());}
  else if(mode==3){Reject(()=>app.RollbackPrepared(cancelBeforeReverse:1));Reject(()=>app.Rollback());Reject(()=>document.ApplyToLibraryTarget());}
  else if(undo){app.Rollback();Reject(()=>app.Rollback());Reject(()=>document.ApplyToLibraryTarget());}
  else if(mode==4){history.Dispose();Reject(()=>document.ApplyToLibraryTarget());Reject(()=>app.Rollback());}
  else if(mode==5){}
  else {document.ApplyToLibraryTarget();applied=true;Reject(()=>app.Rollback());}
  w.WriteStartObject();w.WriteNumber("case",i);w.WriteNumber("mode",mode);w.WriteNumber("chosen",chosen);w.WriteBoolean("applied",applied);
  w.WriteStartArray("counts");foreach(var stroke in scope.Document.Strokes)w.WriteNumberValue(stroke.Points.Count);w.WriteEndArray();
  w.WriteNumber("temporaryFailure",mode==0?half:-1);w.WriteNumber("observerFailure",mode==1?half:-1);w.WriteNumber("cancellation",mode==2?half:-1);
  w.WriteStartObject("source");Metadata(document);w.WriteEndObject();
  w.WriteStartArray("commands");foreach(var c in document.Snapshot.PendingCommands)Command(c);w.WriteEndArray();
  w.WritePropertyName("forward");WriteState(forward);w.WritePropertyName("final");WriteState(app.State);
  w.WritePropertyName("writer");WriteNode(document.ActualDom.Root!);w.WritePropertyName("target");WriteNode(document.ActualTarget!.Root);
  w.WriteStartArray("originalNodes");foreach(var node in history.HistoricalOriginal.Nodes)
  {w.WriteStartObject();w.WriteNumber("id",node.Identity);w.WriteNumber("kind",node.Kind);w.WriteString("name",node.Name);w.WriteBase64String("value",node.CopyValueBytes());w.WriteNumber("parent",node.ParentIdentity);w.WriteEndObject();}w.WriteEndArray();
  w.WriteBase64String("originalCounter",history.HistoricalOriginal.CopyRegistryCounterStorage());w.WriteBase64String("selectedCounter",scope.Context.OriginalSelectedInput!.State.CopyRegistryCounterStorage());
  w.WriteStartArray("initialIds");foreach(var e in history.HistoricalOriginal.Inputs)w.WriteNumberValue(e.RegistryId);w.WriteEndArray();
  if(history.Original.State!=history.HistoricalOriginal)throw new InvalidDataException("Historical original was rewritten.");
  w.WriteEndObject();if(mode==5){scope.Dispose();Reject(()=>document.ApplyToLibraryTarget());Reject(()=>app.Rollback());}
  history.Dispose();Reject(()=>app.Run());
 }
}
w.WriteEndArray();w.WriteStartArray("counterControls");var rng=new Random(170018);
for(int i=0;i<4096;i++)
{
 long next=rng.NextInt64(-10,100000),maximum=rng.NextInt64(-10,100000),old=rng.NextInt64(-10,100000),value=rng.NextInt64(-10,100000);
 if(i%11==0)old=-1;if(i%13==0)value=-1;
 bool worked=LegacyPreparedOriginalSourceJournal.ReverseCounter(next,maximum,old,value,out long result);
 w.WriteStartArray();w.WriteNumberValue(next);w.WriteNumberValue(maximum);w.WriteNumberValue(old);w.WriteNumberValue(value);w.WriteBooleanValue(worked);w.WriteNumberValue(result);w.WriteEndArray();
}
w.WriteEndArray();w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();Console.WriteLine($"PASS14 actual original source histories/generated frames/rollback prefixes,4096 inverse-counter controls/{guards} guards; no CBB acceptance.");
void WriteState(LegacyLibraryOriginalSourceState s)
{
 w.WriteStartObject();w.WriteBoolean("entered",s.HasEntered);w.WriteBoolean("execution",s.ExecutionSucceeded);w.WriteBoolean("faulted",s.IsFaulted);w.WriteBoolean("journalComplete",s.JournalComplete);w.WriteBoolean("rolledBack",s.WasRolledBack);w.WriteBoolean("recording",s.RecordingEnabled);w.WriteBoolean("safe",s.IsSafeForLibraryApply);w.WriteNumber("completed",s.CompletedCommandCount);w.WriteNumber("reversed",s.ReversedCommandCount);w.WriteBase64String("counter17",s.CopyCounterStorage17());
 w.WriteStartArray("registry");foreach(var e in s.Registry){w.WriteStartObject();w.WriteNumber("id",e.RegistryId);w.WriteString("name",e.Name);w.WriteNumber("category",e.Category);w.WriteNumber("command",e.SourceCommand?.CommandIdentity??0);w.WriteBase64String("ranges16",Storage(e.CopyRanges()));w.WriteEndObject();}w.WriteEndArray();
 w.WriteStartArray("journal");foreach(var e in s.GeneratedJournal){w.WriteStartObject();w.WriteNumber("id",e.RegistryId);w.WriteNumber("command",e.CommandIdentity);w.WriteNumber("operation",e.Operation);w.WriteString("name",e.Name);w.WriteNumber("category",e.Category);w.WriteBase64String("ranges16",Storage(e.CopiedSelection.CopyRanges()));w.WriteEndObject();}w.WriteEndArray();
 w.WriteStartArray("trace");foreach(string t in s.Trace)w.WriteStringValue(t);w.WriteEndArray();w.WriteEndObject();
}
void Command(LegacyLibraryTextPublicationCommandSnapshot c)
{w.WriteStartObject();w.WriteNumber("operation",c.Operation);w.WriteNumber("id",c.Identity);w.WriteNumber("next",c.NextIdentity);if(c.SelectionId is{} id){w.WriteNumber("tag",id);w.WriteString("name",c.Name);w.WriteNumber("category",c.Category!.Value);w.WriteBase64String("ranges16",Storage(c.CopyRanges()));}else w.WriteBase64String("body56",c.CopyCounterBody56()!);w.WriteEndObject();}
(LegacyLibraryTextPublicationScope,LegacyOwnedMainWordSourceChoicesResult,LegacyLibraryOriginalSourceHistory) Recognize(InkStroke[] raw,int chosen,int fieldCount=2)
{
 var doc=LegacyOwnedRawInkDocument.Create(raw,default);var original=LegacyLibraryOriginalTextFieldBootstrap.Prepare(LegacyFreshRawInkCurrentView.Prepare(doc),Enumerable.Range(0,fieldCount).Select(i=>"field"+i).ToArray());original.Run();
 var history=LegacyLibraryOriginalSourceHistory.Prepare(original);
 Reject(()=>LegacyLibraryOriginalSourceHistory.Prepare(original));
 Reject(()=>history.CreateSelectedInput(original.State.Inputs[chosen],new(true)));
 var selected=history.CreateSelectedInput(original.State.Inputs[chosen]);selected.Run();
 Reject(()=>history.CreateSelectedInput(original.State.Inputs[chosen]));
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
  return(scope,scope.PrepareAugmentedWords(completion,norm,circles).Publish(new byte[40],new byte[2040*4]),history);
 }
 catch{scope.Dispose();throw;}
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
