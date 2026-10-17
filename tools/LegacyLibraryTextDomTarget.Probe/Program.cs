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
using var file=File.Create(args[10]);using var w=new Utf8JsonWriter(file);int guards=0,totalNodes=0,totalFaults=0; LegacyLibraryTextDomTarget? previousTarget=null;
w.WriteStartObject();w.WriteStartArray("actual");
InkStroke[] m=[new(new InkPoint[]{new(0,90),new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)})];
InkStroke[][] samples=[m,Word(0).Concat(Word(300)).ToArray(),Word(450).Concat(Word(0)).ToArray(),Rotate(m,.18f)];
for(int index=0;index<samples.Length;index++)
{
 var residue=new LegacyMainConfiguredAlternativeResidue(new byte[120],new byte[72],new byte[40],new ushort[256],new byte[64],new byte[4],new byte[4],new byte[4],new byte[4]);
 var configured=LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak,lk,engine,line30,line50,samples[index],0,table,cases,publication).Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]);
 var selected=LegacyFreshTextFieldInputContext.Prepare(LegacyFreshRawInkCurrentView.Prepare(configured.SourceDocument)).CreateSelectedCopy();
 var completion=LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(ak,configured,norm,circles,new byte[4],new byte[4]);completion.Run();
 using var words=LegacyFreshMainWordSourceChoices.PrepareSelectedInput(ak,completion,selected,table,publication,norm,circles);
 var choices=words.Publish(new byte[40],new byte[2040*4]);var text=LegacyFreshMainDocumentTextSelection.Prepare(LegacyFreshMainSourceLinePublication.PublishFresh(choices));text.Register();
 var state=LegacyFreshMainDocumentSelectedSources.Prepare(LegacyFreshMainDocumentCharacterCoverage.Prepare(LegacyFreshMainDocumentSourceCoverage.Prepare(LegacyFreshMainDocumentLineConsistency.Apply(text))));
 var indices=LegacyFreshMainDocumentSourceIndices.Prepare(state);indices.Run();var cleanup=LegacyFreshMainDocumentControlCleanup.Prepare(indices);cleanup.Run();
 var handoff=LegacyFreshMainDocumentCounterHandoff.Apply(LegacyFreshMainDocumentInputTransforms.Apply(cleanup));var source=LegacyFreshMainSourceRangeUtf8.Prepare(handoff);
 var registry=selected.CopyRegistryCounterStorage();var pending=handoff.Commands.Select(c=>c.CommandIdentity).ToArray();
 Reject(()=>LegacyFreshMainDetachedTextDom.Prepare(source,new(true)));
 var target=LegacyLibraryTextDomTarget.CreateFresh(source.Seed);var builder=LegacyFreshMainDetachedTextDom.PrepareForTarget(source,target);Reject(()=>builder.Build(new(true)));var result=builder.Build();
 if(!result.Succeeded||!ReferenceEquals(result.Sources,source)||!ReferenceEquals(result.Arena.Seed,words.DocumentSeed)||!ReferenceEquals(result.Arena.Document,configured.SourceDocument))throw new InvalidDataException("Lost exact detached DOM ownership.");
 Reject(()=>builder.Build());Reject(()=>LegacyFreshMainDetachedTextDom.Prepare(source));
 Check(result.Root,result.Arena);totalNodes+=result.Arena.CopyStates().Length;
 w.WriteStartObject();w.WriteNumber("sample",index);w.WriteBase64String("uuid36",words.DocumentSeed!.LibraryIdentifier38.CopyUtf8Storage36());
 w.WriteBase64String("registryBefore",registry);w.WriteBase64String("registryAfter",selected.CopyRegistryCounterStorage());w.WriteStartArray("pendingBefore");foreach(int id in pending)w.WriteNumberValue(id);w.WriteEndArray();w.WriteStartArray("pendingAfter");foreach(var c in handoff.TransactionOwner.CopyPendingCommands())w.WriteNumberValue(c.CommandIdentity);w.WriteEndArray();
 w.WriteStartArray("characters");foreach(var s in source.Sources.Where(s=>s.CharacterSource is not null))
 {
  var p=s.CharacterSource!;w.WriteStartObject();w.WriteNumber("id",s.SourceNodeIdentity);w.WriteNumber("flags",s.SourceFlags);w.WriteNumber("tag",p.SelectionId);w.WriteNumber("line",p.SourceLineId);w.WriteNumber("freeze",p.Field1C);w.WriteNumber("force",unchecked((int)p.Field40));w.WriteBase64String("ranges16",Storage(s.CopyOriginalRanges()));
  w.WriteStartArray("records40");foreach(var row in p.CopyRecords()){w.WriteStartObject();w.WriteBase64String("raw",row.CopyStorage40());w.WriteBase64String("text",row.Text!.ReadBytes());w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
 }
 w.WriteEndArray();var word=source.Sources.Single(s=>s.WordSource is not null);w.WriteStartObject("word");w.WriteNumber("id",word.SourceNodeIdentity);w.WriteNumber("flags",word.SourceFlags);w.WriteNumber("tag",word.WordSource!.SelectionId);w.WriteBase64String("ranges16",Storage(word.CopyOriginalRanges()));w.WriteStartArray("rows48");
 foreach(var row in word.WordSource.Choices)
 {w.WriteStartObject();w.WriteBase64String("raw",row.CopyScalarStorage48());w.WriteStartArray("refs24");foreach(var link in row.CharacterLinks){w.WriteStartObject();w.WriteNumber("charId",link.Source.SourceNode.Identity);w.WriteBase64String("raw",link.CopyScalarStorage24());w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();}
 w.WriteEndArray();w.WriteEndObject();w.WriteStartObject("text");w.WriteNumber("tag",text.Selection.SelectionId);w.WriteNumber("selected",result.NativeTextSelectedBC);w.WriteNumber("freeze",result.NativeTextFreezeB8);w.WriteBase64String("ranges16",Storage(text.CopyRanges()));w.WriteStartArray("rows32");foreach(var a in choices.Alternatives){w.WriteStartObject();w.WriteBase64String("raw",a.CopyScalarStorage32());w.WriteBase64String("ref24",a.CopyScalarStorage24());w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();w.WriteBase64String("lines56",state.CharacterCoverage.SourceCoverage.LineConsistency.CopyStorage56());
 w.WritePropertyName("result");WriteResult(builder,result);
 w.WritePropertyName("targetBefore");WriteNode(target.Root);
 if(ReferenceEquals(result.Arena,target.Arena)||!ReferenceEquals(result.Arena.NamePool,target.Arena.NamePool))throw new InvalidDataException("Writer/target BEE ownership lost.");
 int buildAttempts=result.Arena.Attempts;var insertion=LegacyLibraryTextDomInsertion.Prepare(result);Reject(()=>insertion.Export(new(true)));var edit=insertion.Export();
 if(!edit.Succeeded||edit.Rows.Count!=1||edit.Rows[0].Payload.Identity==result.Root.Identity)throw new InvalidDataException("Missing V50 payload copy.");
 Reject(()=>insertion.Export());Reject(()=>LegacyLibraryTextDomInsertion.Prepare(result));
 w.WritePropertyName("export");WriteEdit(edit);w.WritePropertyName("writerAfterExport");WriteArena(result.Arena);
 if(previousTarget is not null)Reject(()=>previousTarget.Apply(edit));
 Reject(()=>target.Apply(edit,new(true)));var applied=target.Apply(edit);Reject(()=>target.Apply(edit));
 w.WritePropertyName("targetAfter");WriteNode(applied.AfterRoot);w.WritePropertyName("targetArenaAfter");WriteArena(target.Arena);
 var oldSnapshot=applied.AfterRoot;var nextBuilder=LegacyFreshMainDetachedTextDom.PrepareForTarget(LegacyFreshMainSourceRangeUtf8.Prepare(handoff),target);var next=nextBuilder.Build();
 w.WritePropertyName("repeatBuild");WriteResult(nextBuilder,next);var nextExport=LegacyLibraryTextDomInsertion.Prepare(next).Export();
 if(nextExport.Rows.Count!=2||nextExport.Rows[0].Operation!=4||nextExport.Rows[1].Operation!=2)throw new InvalidDataException("Repeated target did not queue native removal then insertion.");
 w.WritePropertyName("repeatExport");WriteEdit(nextExport);w.WritePropertyName("repeatWriter");WriteArena(next.Arena);
 var replaced=target.Apply(nextExport);if(target.Epoch!=2||oldSnapshot.Children.Count!=1||target.Root.Children.Count!=1)throw new InvalidDataException("Lost immutable replacement snapshot.");
 w.WritePropertyName("replacedTarget");WriteNode(replaced.AfterRoot);w.WritePropertyName("replacedArena");WriteArena(target.Arena);
 if(index==0)
 {
  (LegacyLibraryTextDomTarget Target,LegacyFreshMainDetachedTextDom Builder,LegacyOwnedMainDetachedTextDom Dom) Round()
  {var t=LegacyLibraryTextDomTarget.CreateFresh(source.Seed);var b=LegacyFreshMainDetachedTextDom.PrepareForTarget(LegacyFreshMainSourceRangeUtf8.Prepare(handoff),t);return(t,b,b.Build());}
  w.WriteStartArray("queueFaults");foreach(int mode in new[]{0,1,2})
  {
   var t=LegacyLibraryTextDomTarget.CreateFresh(source.Seed);
   if(mode!=0){var first=LegacyFreshMainDetachedTextDom.PrepareForTarget(LegacyFreshMainSourceRangeUtf8.Prepare(handoff),t).Build();t.Apply(LegacyLibraryTextDomInsertion.Prepare(first).Export());}
   var b=LegacyFreshMainDetachedTextDom.PrepareForTarget(LegacyFreshMainSourceRangeUtf8.Prepare(handoff),t);
   var f=b.BuildPrepared(mode==0?buildAttempts-1:mode-1);
   if(f.Succeeded||!b.IsFaulted||t.IsFaulted||t.Root.Children.Count!=(mode==0?0:1))throw new InvalidDataException("Queue failure prefix lost native ownership.");
   Reject(()=>b.Build());Reject(()=>LegacyLibraryTextDomInsertion.Prepare(f));w.WriteStartObject();w.WriteNumber("mode",mode);w.WritePropertyName("result");WriteResult(b,f);w.WriteEndObject();totalFaults++;
  }w.WriteEndArray();
  var baseline=Round();var baseStart=baseline.Dom.Arena.Attempts;var baseInsertion=LegacyLibraryTextDomInsertion.Prepare(baseline.Dom);var baseEdit=baseInsertion.Export();int exportAttempts=baseline.Dom.Arena.Attempts-baseStart;
  w.WritePropertyName("faultBuildBaseline");WriteResult(baseline.Builder,baseline.Dom);
  w.WriteStartArray("exportFaults");for(int fault=0;fault<exportAttempts;fault++)
  {var q=Round();var owner=LegacyLibraryTextDomInsertion.Prepare(q.Dom);w.WriteStartObject();w.WriteNumber("fault",fault);w.WritePropertyName("before");WriteArena(q.Dom.Arena);var f=owner.ExportPrepared(fault);if(f.Succeeded||q.Target.IsFaulted||q.Target.Root.Children.Count!=0)throw new InvalidDataException("Export failure altered target.");Reject(()=>owner.Export());w.WritePropertyName("edit");WriteEdit(f);w.WritePropertyName("after");WriteArena(q.Dom.Arena);w.WriteEndObject();totalFaults++;}w.WriteEndArray();
  var aRound=Round();var aEdit=LegacyLibraryTextDomInsertion.Prepare(aRound.Dom).Export();int initial=aRound.Target.Arena.Attempts;aRound.Target.Apply(aEdit);int applyAttempts=aRound.Target.Arena.Attempts-initial;
  w.WriteStartArray("applyFaults");for(int fault=0;fault<applyAttempts;fault++)
  {var q=Round();var e=LegacyLibraryTextDomInsertion.Prepare(q.Dom).Export();w.WriteStartObject();w.WriteNumber("fault",fault);w.WritePropertyName("before");WriteArena(q.Target.Arena);Reject(()=>q.Target.ApplyPrepared(e,fault));if(!q.Target.IsFaulted||q.Target.Root.Children.Count!=0)throw new InvalidDataException("Apply null-clone refusal lost prefix.");Reject(()=>q.Target.Apply(e));w.WritePropertyName("after");WriteArena(q.Target.Arena);w.WriteEndObject();totalFaults++;}w.WriteEndArray();
  w.WriteStartArray("replaceFaults");foreach(int fault in new[]{0,2,applyAttempts-1})
  {var q=Round();q.Target.Apply(LegacyLibraryTextDomInsertion.Prepare(q.Dom).Export());var before=q.Target.Root;var b=LegacyFreshMainDetachedTextDom.PrepareForTarget(LegacyFreshMainSourceRangeUtf8.Prepare(handoff),q.Target);var d=b.Build();var e=LegacyLibraryTextDomInsertion.Prepare(d).Export();w.WriteStartObject();w.WriteNumber("fault",fault);w.WritePropertyName("before");WriteArena(q.Target.Arena);Reject(()=>q.Target.ApplyPrepared(e,fault));if(!q.Target.IsFaulted||q.Target.Root.Children.Count!=0||before.Children.Count!=1)throw new InvalidDataException("Native partial replacement was rolled back.");w.WritePropertyName("after");WriteArena(q.Target.Arena);w.WriteEndObject();totalFaults++;}w.WriteEndArray();
  var cancellation=Round();var ce=LegacyLibraryTextDomInsertion.Prepare(cancellation.Dom).Export();Reject(()=>cancellation.Target.ApplyPrepared(ce,cancel:2));if(!cancellation.Target.IsFaulted)throw new InvalidDataException("Missing entered cancellation poison.");
  w.WritePropertyName("cancelledArena");WriteArena(cancellation.Target.Arena);
  var exportedCancel=Round();var cancelledInsertion=LegacyLibraryTextDomInsertion.Prepare(exportedCancel.Dom);Reject(()=>cancelledInsertion.ExportPrepared(cancel:3));Reject(()=>cancelledInsertion.Export());if(exportedCancel.Target.IsFaulted)throw new InvalidDataException("Export cancellation mutated target.");w.WritePropertyName("cancelledExportArena");WriteArena(exportedCancel.Dom.Arena);
  var oldInvocation=LegacyFreshMainSourceRangeUtf8.Prepare(handoff);_=LegacyFreshMainDetachedTextDom.Prepare(oldInvocation);Reject(()=>LegacyFreshMainDetachedTextDom.PrepareForTarget(oldInvocation,target));
  var staleTarget=LegacyLibraryTextDomTarget.CreateFresh(source.Seed);var staleBuilder=LegacyFreshMainDetachedTextDom.PrepareForTarget(LegacyFreshMainSourceRangeUtf8.Prepare(handoff),staleTarget);
  var d1=LegacyFreshMainDetachedTextDom.PrepareForTarget(LegacyFreshMainSourceRangeUtf8.Prepare(handoff),staleTarget).Build();var d2=LegacyFreshMainDetachedTextDom.PrepareForTarget(LegacyFreshMainSourceRangeUtf8.Prepare(handoff),staleTarget).Build();
  var e1=LegacyLibraryTextDomInsertion.Prepare(d1).Export();var e2=LegacyLibraryTextDomInsertion.Prepare(d2).Export();staleTarget.Apply(e1);Reject(()=>staleTarget.Apply(e2));Reject(()=>staleBuilder.Build());if(staleTarget.IsFaulted)throw new InvalidDataException("Stale preflight poisoned target.");
 }
 if(previousTarget is not null)Reject(()=>LegacyFreshMainDetachedTextDom.PrepareForTarget(LegacyFreshMainSourceRangeUtf8.Prepare(handoff),previousTarget));previousTarget=target;
 w.WriteEndObject();
}
w.WriteEndArray();w.WriteEndObject();w.Flush();Console.WriteLine($"PASS4 actual library targets/8 edits/{totalFaults} fault prefixes/{guards} guards; no SDK/C20 acceptance.");
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException or NotSupportedException){guards++;return;}throw new InvalidDataException("Missing DOM lifetime/foreign-owner guard.");}
void Check(LegacyLibraryTextDomNode n,LegacyLibraryTextDomArena a){if(!ReferenceEquals(n.Arena,a))throw new InvalidDataException("Foreign tree owner.");foreach(var c in n.Attributes.Concat(n.Children))Check(c,a);var b=n.Value.CopyBytes();if(b.Length!=0){byte before=b[0];b[0]^=255;if(n.Value.CopyBytes()[0]!=before)throw new InvalidDataException("Mutable DOM value exposure.");}}
void WriteResult(LegacyFreshMainDetachedTextDom b,LegacyOwnedMainDetachedTextDom? r)
{
 var arena=b.Arena!;w.WriteStartObject();w.WriteBoolean("succeeded",r?.Succeeded??false);w.WriteNumber("attempts",arena.Attempts);w.WriteNumber("capacity",b.ScratchCapacity);w.WriteBase64String("scratch",b.ScratchBytes);
 w.WriteStartArray("trace");foreach(var t in arena.Trace)w.WriteStringValue(t);w.WriteEndArray();w.WritePropertyName("tree");if(r?.Succeeded==true)WriteNode(r.Root);else w.WriteNullValue();
 w.WriteStartArray("arena");foreach(var n in arena.CopyStates()){w.WriteStartObject();w.WriteNumber("id",n.Identity);w.WriteNumber("kind",n.Kind);w.WriteString("name",n.Name);w.WriteNumber("nameId",n.NameIdentity);w.WriteNumber("tag",n.Tag);w.WriteBase64String("value",n.Value);w.WriteNumber("parent",n.Parent);w.WriteBoolean("released",n.Released);w.WriteStartArray("attributes");foreach(int id in n.Attributes)w.WriteNumberValue(id);w.WriteEndArray();w.WriteStartArray("children");foreach(int id in n.Children)w.WriteNumberValue(id);w.WriteEndArray();w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
}
void WriteNode(LegacyLibraryTextDomNode n){w.WriteStartObject();w.WriteNumber("id",n.Identity);w.WriteNumber("kind",n.Kind);w.WriteString("name",n.Name);w.WriteNumber("nameId",n.NameIdentity);w.WriteNumber("tag",n.Value.NativeTag);w.WriteBase64String("value",n.Value.CopyBytes());w.WriteStartArray("attributes");foreach(var a in n.Attributes)WriteNode(a);w.WriteEndArray();w.WriteStartArray("children");foreach(var c in n.Children)WriteNode(c);w.WriteEndArray();w.WriteEndObject();}
void WriteEdit(LegacyLibraryTextDomEditResult e){w.WriteStartObject();w.WriteBoolean("succeeded",e.Succeeded);w.WriteNumber("epoch",e.Epoch);w.WriteStartArray("rows");foreach(var row in e.Rows){w.WriteStartObject();w.WriteBase64String("scalar40",row.CopyScalarStorage40());w.WritePropertyName("payload");WriteNode(row.Payload);w.WriteStartArray("path");foreach(uint v in row.Path)w.WriteNumberValue(v);w.WriteEndArray();w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();}
void WriteArena(LegacyLibraryTextDomArena arena){w.WriteStartObject();w.WriteNumber("attempts",arena.Attempts);w.WriteStartArray("names");foreach(var name in arena.NamePool.CopyNames())w.WriteStringValue(name);w.WriteEndArray();w.WriteStartArray("trace");foreach(var t in arena.Trace)w.WriteStringValue(t);w.WriteEndArray();w.WriteStartArray("nodes");foreach(var n in arena.CopyStates()){w.WriteStartObject();w.WriteNumber("id",n.Identity);w.WriteNumber("kind",n.Kind);w.WriteString("name",n.Name);w.WriteNumber("nameId",n.NameIdentity);w.WriteNumber("tag",n.Tag);w.WriteBase64String("value",n.Value);w.WriteNumber("parent",n.Parent);w.WriteBoolean("released",n.Released);w.WriteStartArray("attributes");foreach(int id in n.Attributes)w.WriteNumberValue(id);w.WriteEndArray();w.WriteStartArray("children");foreach(int id in n.Children)w.WriteNumberValue(id);w.WriteEndArray();w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();}
byte[] Storage(LegacyNativeInkPositionRange[] ranges){var b=new byte[ranges.Length*16];for(int i=0;i<ranges.Length;i++){BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(i*16),ranges[i].First.Value);BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(i*16+8),ranges[i].Last.Value);}return b;}
InkStroke[] Rotate(InkStroke[] raw,float a)=>raw.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*MathF.Cos(a)-p.Y*MathF.Sin(a),p.X*MathF.Sin(a)+p.Y*MathF.Cos(a))).ToArray())).ToArray();
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
