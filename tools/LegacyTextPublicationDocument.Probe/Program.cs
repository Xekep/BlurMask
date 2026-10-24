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

using var file=File.Create(args[10]);using var w=new Utf8JsonWriter(file);int guards=0;LegacyOwnedMainWordSourceChoicesResult? foreign=null;
InkStroke[] m=[new(new InkPoint[]{new(0,90),new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)})];
InkStroke[][] samples=[m,Word(0).Concat(Word(300)).ToArray(),Word(450).Concat(Word(0)).ToArray(),Rotate(m,.18f)];
w.WriteStartObject();w.WriteStartArray("actual");
for(int i=0;i<samples.Length;i++)
{
 var (scope,words)=Recognize(samples[i]);using(scope)
 {
  if(foreign is not null)Reject(()=>LegacyLibraryTextPublicationDocument.Prepare(scope,foreign));
  Reject(()=>LegacyLibraryTextPublicationDocument.Prepare(scope,words,new(true)));
  using var owner=LegacyLibraryTextPublicationDocument.Prepare(scope,words);
  Reject(()=>LegacyLibraryTextPublicationDocument.Prepare(scope,words));
  Reject(()=>owner.ApplyToLibraryTarget());Reject(()=>owner.TransferPending());
  Reject(()=>owner.BuildAndTransfer(new(true)));
  scope.Context.Dispose();
  var before=owner.BuildAndTransfer();Reject(()=>owner.BuildAndTransfer());Reject(()=>owner.TransferPending());
  if(!before.CopyIdentifier38().AsSpan().SequenceEqual(scope.Context.Result.CopyIdentifier38())||
    !before.CopyIdentifier38().AsSpan().SequenceEqual(scope.Seed.LibraryIdentifier38.CopyRawStorage16()))
   throw new InvalidDataException("DOM/document snapshot lost the adopted result-owned identifier.");
  if(before.IsFaulted||!before.WasTransferred||before.WasAppliedToLibraryTarget||before.TargetRoot!.Children.Count!=0||owner.ActualTarget!.Epoch!=0)
   throw new InvalidDataException("Transport must not implicitly apply a target or accept C20.");
  var actual=owner.ActualTransportRow!;var handoff=owner.ActualSources.CounterHandoff;
  if(!ReferenceEquals(actual.TransactionOwner,scope.Seed.PendingSelections)||!ReferenceEquals(actual.DomEdit.Source.Sources.Seed,scope.Seed)||
    !ReferenceEquals(scope.Seed.ProcessingResult,scope.Context.Result)||actual.DomEdit.Rows.Count!=1||
    ReferenceEquals(actual.DomEdit.Rows[0].LivePayload,owner.ActualDom.LiveRoot))throw new InvalidDataException("Lost cloned export or actual pending/result ownership.");
  Reject(()=>owner.ApplyToLibraryTarget(new(true)));var after=owner.ApplyToLibraryTarget();Reject(()=>owner.ApplyToLibraryTarget());
  if(after.TargetRoot!.Children.Count!=1||!after.WasAppliedToLibraryTarget||before.TargetRoot.Children.Count!=0||
    ReferenceEquals(after.TargetRoot.Children[0].Arena,before.WriterRoot!.Arena))throw new InvalidDataException("Apply must clone into a separate arena and preserve prior snapshots.");
  w.WriteStartObject();w.WriteNumber("sample",i);Metadata(owner);w.WritePropertyName("before");Snapshot(before);w.WritePropertyName("after");Snapshot(after);w.WriteEndObject();
  var pending=handoff.TransactionOwner.CopyPendingCommands();owner.Dispose();owner.Dispose();
  Reject(()=>owner.ApplyToLibraryTarget());Reject(()=>owner.BuildAndTransfer());
  if(!actual.IsReleased||pending.Count!=handoff.TransactionOwner.CopyPendingCommands().Count||scope.Context.ResourcesReleased)
   throw new InvalidDataException("Document disposal damaged borrowed actual commands or closed the scope.");
  var copy=after.CopyTransportScalarRow40()!;copy[0]=0;if(after.CopyTransportScalarRow40()![0]!=255)throw new InvalidDataException("Snapshot aliases transport bytes.");
  scope.Dispose();Reject(()=>scope.Seed.LibraryIdentifier38.CopyRawStorage16());
  if(!ReferenceEquals(owner.Snapshot,after)||after.TargetRoot.Children.Count!=1)throw new InvalidDataException("Detached document snapshot did not survive close.");
  foreign=words;
 }
}
w.WriteEndArray();w.WriteStartArray("faults");
for(int mode=0;mode<5;mode++)
{
 var (scope,words)=Recognize(m);using(scope)using(var owner=LegacyLibraryTextPublicationDocument.Prepare(scope,words))
 {
  w.WriteStartObject();w.WriteNumber("mode",mode);
  if(mode==0||mode==1)
  {
   var prefix=owner.BuildPrepared(domFailure:mode==0?0:-1,exportFailure:mode==1?0:-1);
   if(!prefix.IsFaulted||prefix.WasTransferred||prefix.TargetRoot!.Children.Count!=0)throw new InvalidDataException("Native refusal published a target.");
   Reject(()=>owner.BuildAndTransfer());Reject(()=>owner.ApplyToLibraryTarget());
   Metadata(owner);w.WritePropertyName("prefix");Snapshot(prefix);
  }
  else if(mode==2)
  {
   Reject(()=>owner.BuildPrepared(transferFailure:true));var prefix=owner.Snapshot;
   if(owner.IsFaulted||owner.ActualTransfer!.PendingEdit is null||owner.ActualTransfer.PendingTransaction is null||prefix.WasTransferred)
    throw new InvalidDataException("Failed reserve did not retain actual incoming slots.");
   Reject(()=>owner.TransferPending(new(true)));var transported=owner.TransferPending();
   Metadata(owner);w.WritePropertyName("prefix");Snapshot(prefix);w.WritePropertyName("recovered");Snapshot(transported);
   if(!transported.WasTransferred||owner.ActualTransfer.PendingEdit is not null||owner.ActualTransfer.PendingTransaction is not null)
    throw new InvalidDataException("Successful retry did not clear incoming slots.");
  }
  else if(mode==3)
  {
   owner.BuildAndTransfer();Reject(()=>owner.ApplyPrepared(failure:0));
   if(!owner.IsFaulted||!owner.ActualTarget!.IsFaulted||owner.Snapshot.TargetRoot!.Children.Count!=0)throw new InvalidDataException("Entered clone failure did not poison the retained prefix.");
   Reject(()=>owner.ApplyToLibraryTarget());Metadata(owner);w.WritePropertyName("prefix");Snapshot(owner.Snapshot);
  }
  else
  {
   owner.BuildAndTransfer();var before=owner.Snapshot;scope.Dispose();
   Reject(()=>owner.ApplyToLibraryTarget());Reject(()=>owner.TransferPending());Reject(()=>owner.BuildAndTransfer());
   if(!ReferenceEquals(before,owner.Snapshot)||before.TargetRoot!.Children.Count!=0)throw new InvalidDataException("Disposed scope allowed a live target operation.");
   w.WritePropertyName("prefix");Snapshot(before);
  }
  w.WriteEndObject();
 }
}
w.WriteEndArray();w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS4 adopted document/DOM/transport/apply trees +5 real failure/lifetime prefixes/{guards} guards; library target only, no native C20 acceptance.");
(LegacyLibraryTextPublicationScope,LegacyOwnedMainWordSourceChoicesResult) Recognize(InkStroke[] raw)
{
 var scope=LegacyLibraryTextPublicationScope.PrepareConfigured(models,raw,0,table,cases,publication);
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
  var words=scope.PrepareAugmentedWords(completion,norm,circles).Publish(new byte[40],new byte[2040*4]);return(scope,words);
 }
 catch{scope.Dispose();throw;}
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
InkStroke[] Rotate(InkStroke[] raw,float a)=>raw.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*MathF.Cos(a)-p.Y*MathF.Sin(a),p.X*MathF.Sin(a)+p.Y*MathF.Cos(a))).ToArray())).ToArray();
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}

void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException or NotSupportedException){guards++;return;}throw new InvalidDataException("Missing document lifetime/failure guard.");}
