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
for(int i=0;i<20;i++)
{
 int mode=i<6?-1:i-6;int chosen=i==18?0:i==19?2:i%2;
 var(scope,words,history,field)=Recognize(samples[i<6?i/2:0],chosen,i==18?1:i==19?3:2);
 using(scope)using(history)using(field)using(var document=LegacyLibraryTextPublicationDocument.PrepareOriginalSelected(scope,words))
 {
  document.BuildAndTransfer();var replay=LegacyLibrarySelectedInputRegistryReplay.PrepareOriginalSelected(document);
  var compatibility=LegacyLibraryOriginalInputPatchCompatibility.PrepareOriginalSelected(replay);compatibility.Run();
  var oldDom=LegacyLibraryTextDomPatchCompatibility.Prepare(document);oldDom.Run();
  var app=history.PrepareApplication(document,compatibility);Reject(()=>app.Run());
  var otherOriginal=LegacyLibraryOriginalTextFieldBootstrap.Prepare(history.CurrentView,new[]{"other"});otherOriginal.Run();
  using(var otherHistory=LegacyLibraryOriginalSourceHistory.Prepare(otherOriginal))
  using(var otherField=otherHistory.PrepareFieldDom(otherOriginal.State.Inputs[0]))
   Reject(()=>otherField.PreparePublication(document,app)); // SAME raw Document, different actual original/name owner.
  var pub=field.PreparePublication(document,app);
  if(!ReferenceEquals(document.OriginalFieldDomPublication,pub)||!ReferenceEquals(field.Publication,pub)||pub.ChildOrdinal!=1)
   throw new InvalidDataException("Lost actual FIELD publication receipt.");
  Reject(()=>field.PreparePublication(document,app));Reject(()=>app.Run());Reject(()=>pub.Apply());Reject(()=>pub.Preflight(new(true)));
  LegacyLibraryOriginalFieldDomPublicationState preflight;
  if(mode==0)preflight=pub.PreflightPrepared(fail:0);
  else if(mode==1)preflight=pub.PreflightPrepared(fail:3);
  else if(mode==2){Reject(()=>pub.PreflightPrepared(cancel:2));preflight=pub.State;}
  else preflight=pub.Preflight();
  Reject(()=>pub.Preflight());
  bool oldApplied=false;
  if(!preflight.IsReady){Reject(()=>app.Run());Reject(()=>pub.Apply());Reject(()=>document.ApplyToLibraryTarget());}
  else
  {
   replay.Run();Reject(()=>document.ApplyToLibraryTarget());
   if(mode==9)app.RunPrepared(failObserverHistory:0);
   else if(mode==10)app.RunPrepared(failTemporaryClone:0);
   else app.Run();
   if(i==0)PreparedTagAndMapControls(history,field,app,document);
   if(mode==11){app.Rollback();Reject(()=>pub.Apply());}
   else if(mode==9||mode==10){Reject(()=>pub.Apply());Reject(()=>document.ApplyToLibraryTarget());}
   else if(mode==7){}
   else if(mode==8){history.Dispose();Reject(()=>pub.Apply());}
   else
   {
    Reject(()=>document.ApplyToLibraryTarget());Reject(()=>pub.Apply(new(true)));
    if(mode==3)pub.ApplyPrepared(fail:0);
    else if(mode==4)pub.ApplyPrepared(fail:3);
    else if(mode==5)Reject(()=>pub.ApplyPrepared(failPostcheck:true));
    else if(mode==6)Reject(()=>pub.ApplyPrepared(cancel:2));
    else pub.Apply();
    Reject(()=>pub.Apply());Reject(()=>app.Rollback());
    if(pub.State.Succeeded){document.ApplyToLibraryTarget();oldApplied=true;Reject(()=>pub.Apply());}
    else Reject(()=>document.ApplyToLibraryTarget());
   }
  }
  w.WriteStartObject();w.WriteNumber("case",i);w.WriteNumber("mode",mode);w.WriteNumber("chosen",chosen);w.WriteBoolean("oldApplied",oldApplied);
  w.WriteString("sourceName",field.SourceName);w.WriteBase64String("newEdit40",pub.CopyEditScalarStorage40());
  w.WriteStartArray("oldEdit40");foreach(var row in document.Snapshot.CopyEditScalarRows40())w.WriteBase64StringValue(row);w.WriteEndArray();
  w.WriteStartObject("source");Metadata(document);w.WriteEndObject();w.WritePropertyName("writer");WriteNode(document.ActualDom.Root!);
  w.WriteStartArray("commands");foreach(var c in document.Snapshot.PendingCommands){w.WriteStartObject();w.WriteNumber("operation",c.Operation);w.WriteNumber("id",c.Identity);if(c.SelectionId is{} tag){w.WriteNumber("tag",tag);w.WriteString("name",c.Name);w.WriteBase64String("ranges16",Storage(c.CopyRanges()));}w.WriteEndObject();}w.WriteEndArray();
  w.WriteStartArray("registry");foreach(var e in app.State.Registry){w.WriteStartObject();w.WriteNumber("id",e.RegistryId);w.WriteString("name",e.Name);w.WriteBase64String("ranges16",Storage(e.CopyRanges()));w.WriteEndObject();}w.WriteEndArray();
  w.WritePropertyName("historicalPage");Page(field.HistoricalPage);w.WritePropertyName("preflight");State(preflight);w.WritePropertyName("final");State(pub.State);
  if(!ReferenceEquals(field.NameOwner,history.Original.NameOwner)||!ReferenceEquals(history.HistoricalOriginal,history.Original.State))throw new InvalidDataException("Historical original/name owner changed.");
  if(mode==7){scope.Dispose();Reject(()=>pub.Apply());}field.Dispose();Reject(()=>pub.Apply());w.WriteEndObject();
 }
}
w.WriteEndArray();w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();Console.WriteLine($"PASS20 actual original FIELD publication/preflight/source/apply runs/{guards} guards; no native CBB acceptance.");
void PreparedTagAndMapControls(LegacyLibraryOriginalSourceHistory history,LegacyLibraryOriginalFieldDom field,
 LegacyLibraryOriginalSourceApplication app,LegacyLibraryTextPublicationDocument document)
{
 var kernel=new LegacyOriginalFieldDomKernel(history.HistoricalOriginal,field.Input,default);kernel.Begin(-1,-1);
 var copy=kernel.ClonePayload(document.ActualDom.Root!,default);
 IEnumerable<LegacyOriginalFieldDomKernel.Node> Walk(LegacyOriginalFieldDomKernel.Node n)=>new[]{n}.Concat(n.Children.SelectMany(Walk));
 var tag=Walk(copy).SelectMany(n=>n.Attributes).First(a=>a.Name?.Text=="tag");
 var registry=app.State.Registry.ToDictionary(e=>e.RegistryId);var domain=field.Input.CopyRanges();
 LegacyLibraryOriginalFieldDomPublication.ValidateTags(copy,registry,domain,default);
 var original=tag.Value;tag.Tag=3;tag.Value=Encoding.ASCII.GetBytes("bad");
 Reject(()=>LegacyLibraryOriginalFieldDomPublication.ValidateTags(copy,registry,domain,default));
 tag.Tag=4;tag.Value=new byte[8];BinaryPrimitives.WriteInt64LittleEndian(tag.Value,long.MaxValue);
 Reject(()=>LegacyLibraryOriginalFieldDomPublication.ValidateTags(copy,registry,domain,default));tag.Value=original;
 long id=BinaryPrimitives.ReadInt64LittleEndian(original);var entry=registry[id];
 var outside=new LegacyLibraryOriginalInkSelection(history.Original,new[]{new LegacyNativeInkPositionRange(new(99UL<<32),new((99UL<<32)|0xff05))});
 registry[id]=new(history,id,entry.Name,entry.Category,outside,entry.SourceCommand);
 Reject(()=>LegacyLibraryOriginalFieldDomPublication.ValidateTags(copy,registry,domain,default));registry[id]=entry;
 registry.Remove(id);Reject(()=>LegacyLibraryOriginalFieldDomPublication.ValidateIdentityMap(field,app,registry));
 registry[id]=new(history,id+100,entry.Name,entry.Category,entry.RangesOwner,entry.SourceCommand);
 Reject(()=>LegacyLibraryOriginalFieldDomPublication.ValidateIdentityMap(field,app,registry));registry[id]=entry;
 _=LegacyLibraryOriginalFieldDomPublication.ValidateIdentityMap(field,app,registry);
}
void State(LegacyLibraryOriginalFieldDomPublicationState s)
{
 w.WriteStartObject();w.WriteBoolean("entered",s.PreflightEntered);w.WriteBoolean("worked",s.Worked);w.WriteBoolean("compatible",s.Compatible);w.WriteBoolean("faulted",s.IsFaulted);w.WriteBoolean("applyEntered",s.ApplyEntered);w.WriteBoolean("succeeded",s.Succeeded);w.WriteBoolean("ready",s.IsReady);w.WriteNumber("epoch",s.LibraryDomEpoch);w.WriteNumber("attempts",s.Attempts);w.WriteNumber("allocated",s.AllocatedNodeCount);
 w.WritePropertyName("page");Page(s.Page);if(s.ScratchField is{} scratch){w.WritePropertyName("scratch");Page(scratch);}
 w.WriteStartArray("trace");foreach(var t in s.Trace)w.WriteStringValue(t);w.WriteEndArray();w.WriteStartArray("map");foreach(var p in s.IdentityMap){w.WriteStartArray();w.WriteNumberValue(p.RequestedId);w.WriteNumberValue(p.ActualId);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
}
void Page(LegacyLibraryOriginalFieldDomNode n)
{
 w.WriteStartObject();w.WriteNumber("id",n.Identity);w.WriteNumber("original",n.OriginalIdentity);w.WriteNumber("kind",n.Kind);w.WriteString("name",n.Name);w.WriteNumber("tag",n.NativeValueTag);w.WriteBase64String("value",n.CopyValueBytes());w.WriteString("prefix",n.NamespacePrefixName?.Text);w.WriteString("uri",n.NamespaceUriName?.Text);
 if(!ReferenceEquals(n.NameOwner,n.CanonicalName?.Owner??n.NameOwner))throw new InvalidDataException("Original name-owner token mismatch.");
 w.WriteStartArray("attributes");foreach(var a in n.Attributes)Page(a);w.WriteEndArray();w.WriteStartArray("children");foreach(var c in n.Children)Page(c);w.WriteEndArray();w.WriteEndObject();
}
(LegacyLibraryTextPublicationScope,LegacyOwnedMainWordSourceChoicesResult,LegacyLibraryOriginalSourceHistory,LegacyLibraryOriginalFieldDom) Recognize(InkStroke[] raw,int chosen,int fieldCount=2)
{
 var doc=LegacyOwnedRawInkDocument.Create(raw,default);var original=LegacyLibraryOriginalTextFieldBootstrap.Prepare(LegacyFreshRawInkCurrentView.Prepare(doc),Enumerable.Range(0,fieldCount).Select(i=>"field"+i).ToArray());original.Run();
 var history=LegacyLibraryOriginalSourceHistory.Prepare(original);
 var field=history.PrepareFieldDom(original.State.Inputs[chosen]);
 Reject(()=>history.PrepareFieldDom(original.State.Inputs[chosen]));
 if(field.SourceName!="./page/content/textField[@id=\"field"+chosen+"\"]")throw new InvalidDataException("Actual field source name mismatch.");
 if(fieldCount>1)Reject(()=>history.CreateSelectedInput(original.State.Inputs[(chosen+1)%fieldCount]));
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
  return(scope,scope.PrepareAugmentedWords(completion,norm,circles).Publish(new byte[40],new byte[2040*4]),history,field);
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
