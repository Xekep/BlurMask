using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
if(args.Length!=4)throw new ArgumentException("MODELS RECORDED-MANIFEST CAPTURE");
string m=args[0];byte[] R(string n)=>File.ReadAllBytes(Path.Combine(m,n));
byte[] ak=R("ru-RU/ru_RU-ak-cur.lite.res");
var models=LegacyFreshMainRecognitionContext.Load(ak,R("ru-RU/ru_RU-lk-text.lite.res"),R("ru-RU/legacy-engine-profile.json"),R("line-geometry/line-2e3-30.res"),R("line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(R("ru-RU/legacy-separation-profile.json"));
using var cs=File.OpenRead(Path.Combine(m,"text-unicode/legacy-case-data.json"));using var ps=File.OpenRead(Path.Combine(m,"text-unicode/legacy-text-publication-data.json"));
using var ns=File.OpenRead(Path.Combine(m,"text-unicode/legacy-normalization-data.json"));using var cis=File.OpenRead(Path.Combine(m,"text-unicode/legacy-circled-payload-data.json"));
var cd=LegacyNativeCaseData.Load(cs);var pd=LegacyNativeTextPublicationData.Load(ps);var norm=LegacyNativeUnicodeNormalizationData.Load(ns);var circles=LegacyNativeCircledPayloadData.Load(cis);
InkStroke M()=>new(new InkPoint[]{new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)});
var samples=new List<(string Name,InkStroke[] Ink)>{("M",[M()]),("MIR",[M(),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})]),("I",[new(new InkPoint[]{new(0,10),new(0,90)})]),("II",[new(new InkPoint[]{new(0,10),new(0,90)}),new(new InkPoint[]{new(40,10),new(40,90)})])};
var recorded=RecordedInputs.Load(args[1]);for(int i=0;i<recorded.Samples.Length;i++)samples.Add((recorded.Names[i],recorded.Samples[i]));

var physical=PhysicalInputs.Load(args[1],recorded);
var timing=new Dictionary<LegacyLibraryGlobalTextPublicationScope,LegacyPhysicalRawInkIngress>();
var numerical=new List<byte[]>();
var captures=new List<byte[]>();int guards=0,untimedParentsRefused=0;
foreach(var (name,raw) in samples)
{
 using var scope=Scope(raw);var parent=Parent(scope);long initial=scope.SharedState.Selections.NextSelectionId;
 Reject(()=>parent.Publish(new byte[39],new byte[8160]));Reject(()=>parent.Publish(new byte[40],new byte[8159]));Reject(()=>parent.Publish(new byte[40],new byte[8160],new(true)));
 if(!ReferenceEquals(parent,LegacyFreshMainGlobalWordTextPublication.Prepare(parent.Wrapper)))throw new Exception("Typed parent identity changed.");
 byte[] residue=Enumerable.Range(0,40).Select(i=>(byte)(i*29+7)).ToArray();parent.Publish(residue,new byte[8160]);
 Reject(()=>parent.Publish(residue,new byte[8160]));Reject(()=>parent.Wrapper.Publish(residue,new byte[8160]));
 captures.Add(Json(w=>Capture(w,name,initial,parent)));
 numerical.Add(GeometryCapture.Capture(name,parent,timing[scope]));
 using(var untimed=UntimedScope(raw))
 {
  var xyParent=Parent(untimed);xyParent.Publish(new byte[40],new byte[8160]);var xyCrop=LegacyGlobalOrdinaryCroppedSource.Prepare(xyParent);
  if(ReferenceEquals(xyParent.Scope.Document,timing[scope].Document))throw new Exception("Untimed root was silently converted to timed ingress.");
  try{LegacyFreshMainGlobalOrdinaryGeometry.Prepare(xyParent,xyCrop,timing[scope].TimingReceipt);throw new Exception("Untimed existing root admitted a foreign physical receipt.");}
  catch(NotSupportedException){untimedParentsRefused++;}
 }
 var first=parent.WordSource.CopyCharacterLinks().FirstOrDefault();var text=first?.Source.CopyRecords()[0].Text;
 scope.Dispose();Reject(()=>parent.CopyPendingSelections());Reject(()=>parent.CopySourceLines56());Reject(()=>parent.OriginalWordSource.CopyOriginalKey());Reject(()=>parent.Text.CopyRanges());if(text is not null)Reject(()=>text.ReadBytes());
}
var failures=new List<byte[]>();
foreach(string kind in new[]{"CHAR","WORD","LINE","TEXT"})for(int stage=1;stage<=(kind=="LINE"?1:4);stage++)
{
 using var scope=Scope([M()]);var parent=Parent(scope);long initial=scope.SharedState.Selections.NextSelectionId;
 var fault=kind switch{"CHAR"=>new LegacyGlobalWordTextPreparedFault(Character:stage),"WORD"=>new(Word:stage),"LINE"=>new(LineAllocation:true),_=>new(Text:stage)};
 Reject(()=>parent.PublishPrepared(new byte[40],new byte[8160],fault));if(!parent.IsFaulted)throw new Exception("Native prefix refusal did not poison parent.");
 Reject(()=>parent.Publish(new byte[40],new byte[8160]));Reject(()=>parent.Wrapper.Publish(new byte[40],new byte[8160]));
 failures.Add(Json(w=>Capture(w,"prepared-"+kind+"-"+stage,initial,parent)));
}
var reservations=new List<byte[]>();foreach(bool cancelAtBoundary in new[]{false,true})
{
 using var scope=Scope([M()]);var parent=Parent(scope);var completion=parent.Wrapper.Completion;
 //A genuine foreign invocation over the SAME immutable original Document.
 var other=LegacyLibraryOriginalTextFieldBootstrap.Prepare(LegacyFreshRawInkCurrentView.Prepare(scope.Document),["other"]);other.Run();var selected=LegacyLibraryOriginalSelectedFieldContext.Prepare(other,other.State.Inputs[0]);selected.Run();using var foreign=LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected(models,selected,0,table,cd,pd);var foreignParent=Parent(foreign);
 byte[] wordBefore=completion.Word.Word.CopyStorage(),foreignBefore=foreignParent.Wrapper.Completion.Word.Word.CopyStorage();int stringsBefore=scope.Context.Strings.Count,foreignStrings=foreign.Context.Strings.Count;long initial=scope.SharedState.Selections.NextSelectionId;
 var pool=completion.Source.Session!.Ownership.GlyphPool;byte[][] ownersBefore=pool.CopyOwnerNodes().Select(n=>n.Records.ToArray()).ToArray();
 using var barrier=new LegacyGlobalWordTextReservationBarrier();using var cancel=new CancellationTokenSource();
 var running=Task.Run(()=>parent.PublishPreparedReservationBoundary(new byte[40],new byte[8160],barrier,cancel.Token));
 if(!barrier.Reserved.Wait(TimeSpan.FromSeconds(10))){cancel.Cancel();barrier.Resume.Set();throw new Exception("Reservation boundary not reached.");}
 if(parent.Wrapper.Choices.IsFaulted||parent.Wrapper.IsFaulted)throw new Exception("Reservation unexpectedly faulted before boundary checks.");
 try{parent.Wrapper.Choices.Publish(new byte[40],new byte[8160]);throw new Exception("Direct Choices publication escaped reservation.");}catch(InvalidOperationException e)when(e.Message=="This global glyph-choice publication already entered."){guards++;}
 Reject(()=>parent.Wrapper.Publish(new byte[40],new byte[8160]));Reject(()=>LegacyFreshMainGlobalWordTextPublication.Prepare(parent.Wrapper));
 if(scope.SharedState.Words.Count!=0||scope.SharedState.Characters.Count!=0||scope.SharedState.Selections.NextSelectionId!=initial||scope.Context.Strings.Count!=stringsBefore||!completion.Word.Word.CopyStorage().AsSpan().SequenceEqual(wordBefore)||!pool.CopyOwnerNodes().Select(n=>n.Records.ToArray()).Zip(ownersBefore).All(x=>x.First.AsSpan().SequenceEqual(x.Second)))throw new Exception("Reservation/public refusal mutated native WORD/CHAR/string/source prefix.");
 if(cancelAtBoundary)cancel.Cancel();else barrier.Resume.Set();
 try{running.GetAwaiter().GetResult();if(cancelAtBoundary)throw new Exception("Expected early cancellation.");}catch(OperationCanceledException)when(cancelAtBoundary){guards++;}
 Reject(()=>parent.Wrapper.Choices.Publish(new byte[40],new byte[8160]));Reject(()=>parent.Wrapper.Publish(new byte[40],new byte[8160]));Reject(()=>parent.Publish(new byte[40],new byte[8160]));
 if(cancelAtBoundary&&(!parent.IsFaulted||!parent.Wrapper.IsFaulted||!parent.Wrapper.Choices.IsFaulted||scope.SharedState.Words.Count!=0||scope.SharedState.Characters.Count!=0||scope.SharedState.Selections.NextSelectionId!=initial))throw new Exception("Early parent cancellation did not poison the actual wrapper/Choices claim.");
 if(!foreignParent.Wrapper.Completion.Word.Word.CopyStorage().AsSpan().SequenceEqual(foreignBefore)||foreign.Context.Strings.Count!=foreignStrings||foreign.SharedState.Words.Count!=0||foreign.SharedState.Characters.Count!=0||foreignParent.Wrapper.Choices.IsFaulted)throw new Exception("Foreign SAME Document scope was mutated.");
 reservations.Add(Json(w=>{w.WriteStartObject();w.WriteBoolean("cancelled",cancelAtBoundary);w.WriteBoolean("parentFaulted",parent.IsFaulted);w.WriteBoolean("wrapperFaulted",parent.Wrapper.IsFaulted);w.WriteBoolean("choicesFaulted",parent.Wrapper.Choices.IsFaulted);w.WriteNumber("reservedChoicesEnteredGuard",1);w.WriteNumber("wordNodes",scope.SharedState.Words.Count);w.WriteNumber("charNodes",scope.SharedState.Characters.Count);w.WriteNumber("nextBefore",initial);w.WriteNumber("nextAfter",scope.SharedState.Selections.NextSelectionId);w.WriteNumber("reservationEvents",parent.CopyTrace().Count(e=>e=="reserved"));w.WriteNumber("childEntryEvents",parent.Wrapper.CopyTrace().Count(e=>e=="claim"));w.WriteBoolean("foreignSameDocumentUnchanged",true);w.WriteStartArray("childTrace");foreach(var e in parent.Wrapper.CopyTrace())w.WriteStringValue(e);w.WriteEndArray();w.WriteEndObject();}));
}
byte[] preparedSkipped;
{
 using var scope=Scope([M()]);scope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]);var c=scope.CompleteSources(new byte[400],new byte[4],new byte[4]);
 //Explicit after-inference actual-pool mutation; not a natural OCR winner.
 var pool=c.Source.Session!.Ownership.GlyphPool;ulong reference=c.Word.Word.FlatReferences[0];byte[] row=pool.CopyRecord(reference);BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(4),1);BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(8),7);pool.ReplaceRecord(reference,row);
 var parent=LegacyFreshMainGlobalWordTextPublication.Prepare(LegacyFreshMainGlobalTextSourceWrapper.Prepare(scope,c,ak,pd,norm,circles));long initial=scope.SharedState.Selections.NextSelectionId;parent.Publish(new byte[40],new byte[8160]);
 if(parent.WordSource.SelectionId!=-1||parent.WordSource.CopyCharacterLinks().Count!=0||parent.CopyPendingSelections().Count!=1||parent.CopyPendingSelections()[0].Name!="TEXT")throw new Exception("Prepared skipped WORD registered an invented ordinary selection.");
 preparedSkipped=Json(w=>Capture(w,"prepared-post-inference-skip",initial,parent));
}
{
 using var scope=Scope([M()]);var parent=Parent(scope);var c=parent.Wrapper.Completion;var pool=c.Source.Session!.Ownership.GlyphPool;ulong reference=c.Word.Word.FlatReferences[0];byte[] before=pool.CopyRecord(reference);byte[] changed=before.ToArray();changed[6]^=1;pool.ReplaceRecord(reference,changed);
 Reject(()=>parent.Publish(new byte[40],new byte[8160]));pool.ReplaceRecord(reference,before);
 c.Word.RebindWord(1,c.Word.Word);Reject(()=>parent.Publish(new byte[40],new byte[8160]));c.Word.RebindWord(0,c.Word.Word);
 if(parent.CopyTrace().Length!=0||parent.IsFaulted||parent.Wrapper.Choices.IsFaulted||scope.SharedState.Words.Count!=0||scope.Context.Strings.Count!=0)throw new Exception("Prepared changed snapshot crossed WORD preclaim.");
 parent.Publish(new byte[40],new byte[8160]);
}
int degenerateCases=0;
foreach(uint source in new uint[]{0,1})foreach(int pointIndex in new[]{0,1})foreach(byte first in new byte[]{0,1,254,255})foreach(byte last in new byte[]{0,1,254,255})foreach(byte mode in new byte[]{1,3,4,5})foreach(byte endMode in new byte[]{1,3,4,5})
{
 var range=new LegacyNativeInkPositionRange(new(((ulong)first<<8)|mode),new(((ulong)source<<32)|((ulong)pointIndex<<16)|((ulong)last<<8)|endMode));bool expected=source==0&&pointIndex==0&&(first==last||first==255||last==255);
 if(LegacyFreshMainGlobalWordTextPublication.SameCoordinate(range)!=expected)throw new Exception("Prepared WORD degenerate predicate differs.");degenerateCases++;
}
{
 using var scope=Scope([M()]);var parent=Parent(scope);scope.SharedState.Words.InternPrepared(parent.OriginalWordSource.CopyOriginalKey());Reject(()=>parent.Publish(new byte[40],new byte[8160]));
 if(parent.CopyTrace().Length!=0||parent.IsFaulted)throw new Exception("Prepared WORD node crossed fresh preclaim.");
}
//Pure budget controls do not manufacture an actual WORD/source owner.
var budget=new LegacyGlobalWordTextWorkBudget();budget.AddSource(65536,0,4096,256);if(budget.Rows!=65536||budget.Ranges!=1048576||budget.Utf8Bytes!=262144)throw new Exception("Exact conservative work caps changed.");
Reject(()=>budget.AddSource(1,0,0,1));Reject(()=>budget.AddSource(0,0,1,1));Reject(()=>new LegacyGlobalWordTextWorkBudget().AddSource(-1,0,0,1));Reject(()=>new LegacyGlobalWordTextWorkBudget().AddSource(0,0,-1,1));Reject(()=>new LegacyGlobalWordTextWorkBudget().AddSource(0,0,1,257));
File.WriteAllBytes(args[2],Json(w=>{w.WriteStartObject();w.WriteNumber("guards",guards);w.WriteNumber("preparedBudgetGuards",5);w.WriteNumber("preparedLiveStateGuards",2);w.WriteNumber("preparedDegenerateCases",degenerateCases);w.WritePropertyName("preparedSkipped");w.WriteRawValue(preparedSkipped);w.WriteStartArray("actual");foreach(var value in captures)w.WriteRawValue(value);w.WriteEndArray();w.WriteStartArray("preparedFailures");foreach(var value in failures)w.WriteRawValue(value);w.WriteEndArray();w.WriteStartArray("preparedReservationBoundaries");foreach(var value in reservations)w.WriteRawValue(value);w.WriteEndArray();w.WriteEndObject();}));
byte[] geometryGuards=GeometryGuards.Capture(Scope,Parent,s=>timing[s]);
File.WriteAllBytes(args[3],Json(w=>{w.WriteStartObject();w.WriteNumber("untimedParentsRefused",untimedParentsRefused);w.WriteStartArray("samples");foreach(var item in numerical)w.WriteRawValue(item);w.WriteEndArray();w.WritePropertyName("preparedControls");w.WriteRawValue(geometryGuards);w.WriteEndObject();}));
Console.WriteLine($"PASS actual13 global WORD/TEXT; prepared CHAR4/WORD4/LINE1/TEXT4; guards{guards}; no document coverage/DOM adoption.");
LegacyLibraryGlobalTextPublicationScope Scope(InkStroke[] ink){var ingress=LegacyPhysicalRawInkIngress.Create(ink,physical.TryGetValue(ink,out var parameters)?parameters:PhysicalInputs.PreparedUniform(ink));var original=LegacyLibraryOriginalTextFieldBootstrap.Prepare(ingress.CurrentView,["a","b"]);original.Run();var selected=LegacyLibraryOriginalSelectedFieldContext.Prepare(original,original.State.Inputs[1]);selected.Run();var scope=LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected(models,selected,0,table,cd,pd);timing.Add(scope,ingress);return scope;}
LegacyLibraryGlobalTextPublicationScope UntimedScope(InkStroke[] ink){var view=LegacyFreshRawInkCurrentView.Prepare(LegacyOwnedRawInkDocument.Create(ink,default));var original=LegacyLibraryOriginalTextFieldBootstrap.Prepare(view,["a","b"]);original.Run();var selected=LegacyLibraryOriginalSelectedFieldContext.Prepare(original,original.State.Inputs[1]);selected.Run();return LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected(models,selected,0,table,cd,pd);}
LegacyFreshMainGlobalWordTextPublication Parent(LegacyLibraryGlobalTextPublicationScope scope){scope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]);var c=scope.CompleteSources(new byte[400],new byte[4],new byte[4]);return LegacyFreshMainGlobalWordTextPublication.Prepare(LegacyFreshMainGlobalTextSourceWrapper.Prepare(scope,c,ak,pd,norm,circles));}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Unsupported operation accepted.");}
static void Capture(Utf8JsonWriter w,string name,long initial,LegacyFreshMainGlobalWordTextPublication parent)
{
 var p=parent.WordSource;w.WriteStartObject();w.WriteString("name",name);w.WriteBoolean("complete",parent.IsComplete);w.WriteBoolean("faulted",parent.IsFaulted);w.WriteNumber("nextBefore",initial);w.WriteNumber("nextAfter",parent.Scope.SharedState.Selections.NextSelectionId);
 w.WriteNumber("rowUpperBound",parent.PreflightMaximumAlternativeRows);w.WriteNumber("node",p.Node.Identity);w.WriteNumber("wordSelection",p.SelectionId);w.WriteNumber("wordFlags",p.Flags);w.WriteNumber("wordChoiceFlags",p.ChoiceFlags);w.WriteNumber("choiceCount",p.ChoiceCount);w.WriteNumber("wordIndex",parent.SelectedWordIndex);w.WriteNumber("alternatives",parent.AlternativeCount);
 w.WriteNumber("rangeUpperBound",parent.PreflightMaximumProjectedRanges);
 w.WriteNumber("compactActive",parent.Wrapper.Completion.Source.Selection.Publication.RecordCount2470);w.WriteNumber("nnCached",parent.Wrapper.Completion.Source.CachedMainGroupEvaluations);w.WriteNumber("nnCurrent",parent.Wrapper.Completion.Snapshot.SourcePolicy.Owner.AcousticEvaluations);w.WriteNumber("actualAlternativeCount",parent.Wrapper.Completion.Snapshot.ActiveAlternativeCount60);w.WriteString("wordSource26",Hex(parent.Wrapper.Completion.CopySourceStorage26()));
 w.WriteStartArray("wordScalars");foreach(long x in new long[]{(long)p.Field00,p.Field08,p.SelectionId,p.Flags,p.Field40,p.Field48})w.WriteNumberValue(x);w.WriteEndArray();
 Ranges(w,"wordKey",p.OriginalSource.CopyOriginalKey());Ranges(w,"ordinary",p.CopyOrdinaryRanges());Ranges(w,"textKey",parent.CopyNullProjectedTextKey());w.WriteString("wordChoice48",Hex(p.CopyChoiceScalar48()));
 w.WriteStartArray("links");foreach(var link in p.CopyCharacterLinks())
 {
  var g=link.Choice;var source=g.ProcessedSource;var meta=source.OriginalPositions.Source;var payload=g.Payload;var scalar=payload.CopyAssignedScalars();
  w.WriteStartObject();w.WriteNumber("index",source.WordIndex);w.WriteNumber("reference",source.Reference);w.WriteNumber("node",payload.Node.Identity);w.WriteBoolean("ordinary",source.IsOrdinarySource);w.WriteBoolean("reused",g.Reused);w.WriteNumber("marker",g.Marker);w.WriteNumber("line",payload.SourceLineId!.Value);w.WriteNumber("metadataFlags",payload.Flags!.Value);w.WriteNumber("selectionId",payload.SelectionId!.Value);w.WriteNumber("priorVisits",payload.PriorVisits);w.WriteNumber("written",(int)payload.Written);
  w.WriteString("source24",Hex(meta.CopyReferenceStorage24()));w.WriteString("residual2",Hex(meta.CopyResidualStorage2()));w.WriteString("scalar24",Hex(link.CopyScalar24()));Ranges(w,"positions",source.OriginalPositions.CopyRanges());Ranges(w,"processed",source.CopyRanges());w.WriteStartArray("records");foreach(var row in payload.CopyRecords()){w.WriteStartObject();w.WriteString("scalar40",Hex(row.CopyStorage40()));w.WriteNumber("intern",row.Text!.Identity);w.WriteString("utf8",Hex(row.Text.ReadBytes()));w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
 }w.WriteEndArray();
 w.WriteStartArray("childPayloadPrefix");foreach(var payload in parent.Wrapper.CopyPayloadPrefix()){w.WriteStartObject();w.WriteNumber("written",(int)payload.Written);w.WriteBoolean("complete",payload.IsComplete);w.WriteNumber("node",payload.Node.Identity);w.WriteEndObject();}w.WriteEndArray();
 w.WriteNumber("documentAlternativeCount",parent.DocumentAlternativeCount);w.WriteBoolean("documentLink",parent.HasDocumentWordLink);if(parent.HasDocumentWordLink)w.WriteString("documentWord24",Hex(parent.CopyDocumentWordLink24()));if(parent.DocumentAlternativeCount!=0)w.WriteString("documentChoice32",Hex(parent.CopyDocumentChoice32()));
 w.WriteNumber("lineCount",parent.LineCount);w.WriteNumber("wrapperOffset",parent.WrapperLineOffset);w.WriteString("sourceLines56",Hex(parent.CopySourceLines56()));
 w.WriteBoolean("hasText",parent.HasTextReceipt);if(parent.HasTextReceipt&&parent.Text.SelectionId is{} id)w.WriteNumber("textSelection",id);else w.WriteNull("textSelection");
 w.WriteStartArray("descriptors");foreach(var d in parent.CopyDescriptors())w.WriteStringValue(Hex(d));w.WriteEndArray();w.WriteStartArray("guides");foreach(var g in parent.CopyGuides())w.WriteStringValue(Hex(g));w.WriteEndArray();
 w.WriteStartArray("commands");foreach(var c in parent.CopyPendingSelections()){w.WriteStartObject();w.WriteNumber("id",c.SelectionId);w.WriteString("name",c.Name);w.WriteNumber("command",c.CommandIdentity);w.WriteNumber("previous",c.PreviousCommandIdentity);w.WriteNumber("operation",c.OperationCode);w.WriteNumber("category",c.Category);w.WriteNumber("enabled",c.Enabled);Ranges(w,"ranges",c.CopyRanges());w.WriteEndObject();}w.WriteEndArray();
 w.WriteStartArray("trace");foreach(var e in parent.CopyTrace())w.WriteStringValue(e);w.WriteEndArray();w.WriteStartArray("childTrace");foreach(var e in parent.Wrapper.CopyTrace())w.WriteStringValue(e);w.WriteEndArray();
 w.WriteStartArray("prefixes");foreach(var e in parent.CopyPrefixes()){w.WriteStartObject();w.WriteString("stage",e.Stage);w.WriteNumber("wordNodes",e.WordNodes);w.WriteNumber("characterNodes",e.CharacterNodes);w.WriteNumber("documentAlternatives",e.DocumentAlternatives);w.WriteStartArray("assigned");foreach(long? n in new long?[]{e.WordFlags,e.ChoiceFlags,e.ChoiceCount,e.CharacterLinks,e.WordSelectionId}){if(n is{} value)w.WriteNumberValue(value);else w.WriteNullValue();}w.WriteEndArray();w.WriteNumber("next",e.NextCounter);w.WriteNumber("lines",e.Lines);w.WriteNumber("offset",e.WrapperOffset);w.WriteEndObject();}w.WriteEndArray();
 w.WriteEndObject();
}
static void Ranges(Utf8JsonWriter w,string name,IEnumerable<LegacyNativeInkPositionRange> ranges){w.WriteStartArray(name);foreach(var r in ranges){w.WriteStartArray();w.WriteNumberValue(r.First.Value);w.WriteNumberValue(r.Last.Value);w.WriteEndArray();}w.WriteEndArray();}
static string Hex(ReadOnlySpan<byte> value)=>Convert.ToHexStringLower(value);
static byte[] Json(Action<Utf8JsonWriter> write){using var stream=new MemoryStream();using var writer=new Utf8JsonWriter(stream);write(writer);writer.Flush();return stream.ToArray();}
