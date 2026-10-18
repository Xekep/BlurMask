using InkRecognition;
using System.Text.Json;
if(args.Length!=11)throw new ArgumentException("AK LK engine separation case publication normalization circles line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);using var nf=File.OpenRead(args[6]);using var cr=File.OpenRead(args[7]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var norm=LegacyNativeUnicodeNormalizationData.Load(nf);var circles=LegacyNativeCircledPayloadData.Load(cr);
byte[] line30=File.ReadAllBytes(args[8]),line50=File.ReadAllBytes(args[9]);
var models=LegacyFreshMainRecognitionContext.Load(ak,lk,engine,line30,line50);
InkStroke[][] samples=[[new(new InkPoint[]{new(0,90),new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)})],Word(0).Concat(Word(300)).ToArray(),Word(450).Concat(Word(0)).ToArray()];
using var output=File.Create(args[10]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteStartArray("samples");
int guards=0,totalWords=0,totalGlyphs=0,reused=0; LegacyFreshMainConfiguredNoCandidateSourceCompletion? foreignCompletion=null;
var originals=samples.Select(raw=>{var d=LegacyOwnedRawInkDocument.Create(raw,default);var o=LegacyLibraryOriginalTextFieldBootstrap.Prepare(LegacyFreshRawInkCurrentView.Prepare(d),new[]{"left","right"});o.Run();return o;}).ToArray();
for(int invocation=0;invocation<samples.Length*2;invocation++)

{
 int sample=invocation/2,chosen=invocation%2;var original=originals[sample];
 var selected=LegacyLibraryOriginalSelectedFieldContext.Prepare(original,original.State.Inputs[chosen]);selected.Run();
 var residue=new LegacyMainConfiguredAlternativeResidue(new byte[120],new byte[72],new byte[40],new ushort[256],new byte[64],new byte[4],new byte[4],new byte[4],new byte[4]);
 using var scope=LegacyLibraryTextPublicationScope.PrepareConfiguredOriginalSelected(models,selected,0,table,cases,publication);
 var context=scope.Context;var seed=scope.Seed;var selectedInput=selected;
 Reject(()=>{_=context.SelectedInput;});Reject(()=>{_=seed.SelectedInputContext;});Reject(()=>{_=seed.Input;});
 Reject(()=>{_=context.Result.BoundInput28;});Reject(()=>{_=context.Result.BoundRequest!.SelectedInput;});Reject(()=>{_=context.Result.BoundRequest!.Input;});
 byte[] importedRanges=context.Result.CopyInputRangeStorage();uint importedFlags=context.State18;
 if(importedFlags!=1||context.Result.CopyIdentifier38().Any(b=>b!=0)||scope.SharedState.Strings.Count!=0||
   !ReferenceEquals(context.Result.PublicationSeed,seed)||!ReferenceEquals(seed.ProcessingResult,context.Result)||
   !ReferenceEquals(seed.StringPool,context.Strings)||!ReferenceEquals(seed.PendingSelections,scope.SharedState.Selections)||
   !ReferenceEquals(seed.OriginalSelectedInputContext,selected)||!ReferenceEquals(context.Result.OriginalSelectedInput28,selected)||!ReferenceEquals(scope.Document,context.Document))
  throw new InvalidDataException("Pre-NN actual input/result/seed/3E ownership differs.");
 Reject(()=>{_=scope.SharedState.TopOwner;});Reject(()=>{_=seed.LibraryIdentifier38;});
 Reject(()=>context.RetainForPublication());
 Reject(()=>scope.Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40],new(true)));
 var configured=scope.Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]);
 if(!ReferenceEquals(configured.SourceDocument,context.Document)||!ReferenceEquals(configured.InputOwner.SourceDocument,scope.Document)||scope.SharedState.Strings.Count!=0)
  throw new InvalidDataException("Recognition replaced the preimported raw document/3E.");
 var completion=LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(ak,configured,norm,circles,new byte[4],new byte[4]);
 Reject(()=>scope.PrepareWords(completion,norm,circles));completion.Run();
 if(foreignCompletion is not null)Reject(()=>scope.PrepareWords(foreignCompletion,norm,circles));
 Reject(()=>scope.PrepareWords(completion,norm,circles,new(true)));
 context.Dispose();if(context.ResourcesReleased)throw new InvalidDataException("Context prematurely released the publication lease.");
 Reject(()=>context.PrepareInput());
 var retainedRequest=context.Result.BoundRequest!;var retainedRequestRoot=retainedRequest.Root;
 var actualIndices=configured.Alternatives.CopyActiveAlternatives().Select(a=>LegacyOwnedMainCandidateSource.FromConfigured(configured,a).WordHead18!.WordIndex).ToArray();
 using var owner=scope.PrepareWords(completion,norm,circles);
 Reject(()=>scope.PrepareWords(completion,norm,circles));
 if(!ReferenceEquals(owner.DocumentSeed,seed)||!ReferenceEquals(owner.SelectionContext,seed.PendingSelections)||!ReferenceEquals(owner.Document,scope.Document)||
   !context.Result.CopyIdentifier38().AsSpan().SequenceEqual(seed.LibraryIdentifier38.CopyRawStorage16()))throw new InvalidDataException("Publication replaced the adopted seed/identifier/3E.");
 if(!ReferenceEquals(owner.OriginalSelectedInputContext,selectedInput)||owner.NextSelectionId!=selectedInput.State.NextRegistryId||owner.CopyPendingSelections().Count!=0)throw new InvalidDataException("Native05F050 pending state is not bound to selected INPUT.");
 Reject(()=>{_=owner.SelectedInputContext;});Reject(()=>{_=owner.SelectionContext.SelectedInputContext;});
 if(!ReferenceEquals(owner.SelectionContext.OriginalSelectedInputContext,selected)||owner.NextSelectionId!=2||selected.State.Registry.Count!=2)throw new InvalidDataException("Complete selected registry lost before callbacks.");
 var initial=owner.WordViews.Select(word=>word.FlatReferences.Select(reference=>(Reference:reference,Record:owner.TopOwner.GlyphPool.CopyRecord(reference))).ToArray()).ToArray();
 Reject(()=>owner.Publish(new byte[39],new byte[2040*4]));Reject(()=>owner.Publish(new byte[40],new byte[2040*4],new(true)));
 var result=owner.Publish(new byte[40],new byte[2040*4]);Reject(()=>owner.Publish(new byte[40],new byte[2040*4]));
 if(result.Alternatives.Count!=2||result.Alternatives[0].ReusedWordSourceKey||!result.Alternatives[1].ReusedWordSourceKey||
    !ReferenceEquals(result.Alternatives[0].Source,result.Alternatives[1].Source))throw new InvalidDataException("Two actual alternatives must share one WORD source payload.");
 w.WriteStartObject();w.WriteBase64String("importedRanges",importedRanges);w.WriteNumber("importedFlags",importedFlags);w.WriteBoolean("zeroIdentifierBeforePublication",true);w.WriteBoolean("sameResultSeedStrings",true);w.WriteBoolean("retainedAfterContextDispose",!context.ResourcesReleased);w.WriteNumber("retainedRootKind",retainedRequestRoot.Kind);w.WriteNumber("inputTag",selectedInput.OriginalInput.RegistryId);w.WriteNumber("inputNext",selectedInput.State.NextRegistryId);w.WriteBase64String("inputCounter",selectedInput.State.CopyRegistryCounterStorage());w.WriteBase64String("inputRanges",context.Result.CopyInputRangeStorage());w.WriteNumber("inputSpans",selectedInput.State.Nodes.Single(n=>n.Identity==selectedInput.State.SelectedInputIdentity).Children.Count);w.WriteNumber("sample",sample);w.WriteNumber("line",completion.Sources.CandidateSource.Sampled.Owner.SourceLineId);
 w.WriteStartArray("nativeSelectedWordIndices");foreach(int index in actualIndices)w.WriteNumberValue(index);w.WriteEndArray();
 w.WriteStartArray("counts");foreach(var stroke in owner.Document.Strokes)w.WriteNumberValue(stroke.Points.Count);w.WriteEndArray();
 w.WriteStartArray("words");
 foreach(var row in result.Source.Choices)
 {
  w.WriteStartObject();w.WriteNumber("ordinal",row.Word.WordOrdinal);w.WriteBase64String("word120",row.Word.CopyStorage120());
  w.WriteBase64String("scalar48",row.CopyScalarStorage48());w.WriteNumber("selectedFlags",row.Flags);
  // Native first pass clears WORD's virtual bit before later C21 processing.
  w.WriteNumber("sourceFlags",row.Glyphs.Any(g=>unchecked((short)(g.GlyphReference>>24))>=0)?0:0x200000);
  var ops=LegacyOwnedRawInkRangeOperations.Prepare(owner.Document);LegacyNativeInkPositionRange[] ordinary=[];
  foreach(var glyph in row.Glyphs)if(glyph.ProcessedSource.IsOrdinarySource)ordinary=ops.Union(ordinary,glyph.Source.SourceNode.FirstProcessedSource!.CopyRanges());
  WriteRanges("ordinaryRanges",ordinary);WriteRanges("wordClip",completion.Sources.CopyRanges());w.WriteBase64String("wordSource26",completion.Sources.CopySourceStorage26());
  w.WriteStartArray("inputs");
  for(int i=0;i<initial[row.Word.WordOrdinal].Length;i++)
  {var input=initial[row.Word.WordOrdinal][i];var glyph=row.Glyphs.FirstOrDefault(g=>g.ProcessedSource.WordIndex==i);w.WriteStartObject();w.WriteNumber("reference",input.Reference);w.WriteBase64String("initialGlyph40",input.Record);if(glyph is not null)WriteRanges("originalRanges",glyph.ProcessedSource.OriginalPositions.Ranges.ToArray());w.WriteEndObject();}
  w.WriteEndArray();w.WriteStartArray("glyphs");
  foreach(var glyph in row.Glyphs)
  {
   var source=glyph.Source;var processed=glyph.ProcessedSource;
   w.WriteStartObject();w.WriteNumber("index",processed.WordIndex);w.WriteNumber("node",source.SourceNode.Identity);w.WriteBoolean("reused",glyph.ReusedSourceKey);w.WriteBoolean("positive",processed.IsOrdinarySource);
   w.WriteNumber("line",source.SourceLineId);w.WriteNumber("selectionId",source.SelectionId);w.WriteNumber("flags",source.Flags);w.WriteNumber("marker",glyph.SelectedMarker);
   var actualOwner=owner.TopOwner.GlyphPool.CopyOwnerNode(LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(glyph.GlyphReference));
   w.WriteBase64String("owner40",actualOwner.Records.Span);w.WriteBase64String("selectionRows4",glyph.InitialSelection.CopySelectionRows4());w.WriteNumber("initialMarker",glyph.InitialSelection.SelectedMarker!.Value);
   WriteRanges("key",processed.CopyRanges());WriteRanges("storedKey",source.SourceNode.FirstProcessedSource!.CopyRanges());w.WriteBase64String("selectedUtf8",glyph.CopySelectedUtf8());
   w.WriteStartArray("sourceRefs24");if(processed.OriginalPositions.OrdinarySource is{} refs)foreach(var r in refs.References){w.WriteStartArray();foreach(uint value in new[]{r.First.First,r.First.Second,r.First.Third,r.Last.First,r.Last.Second,r.Last.Third})w.WriteNumberValue(value);w.WriteEndArray();}w.WriteEndArray();
   w.WriteStartArray("records");foreach(var r in source.CopyRecords()){w.WriteStartObject();w.WriteBase64String("scalar40",r.CopyStorage40());w.WriteBase64String("utf8",r.Text!.ReadBytes());w.WriteNumber("stringId",r.Text.Identity);w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
   totalGlyphs++;if(glyph.ReusedSourceKey)reused++;
  }
  w.WriteEndArray();w.WriteStartArray("links24");foreach(var link in row.CharacterLinks)w.WriteBase64StringValue(link.CopyScalarStorage24());w.WriteEndArray();w.WriteEndObject();totalWords++;
 }
 w.WriteEndArray();WriteRanges("sourceKey",result.Source.CopySourceKeyRanges());WriteRanges("ordinaryRanges",result.Source.CopyOrdinaryRanges());
 w.WriteNumber("flags",result.Source.Flags);w.WriteNumber("selectionId",result.Source.SelectionId);
 w.WriteStartArray("alternatives");foreach(var a in result.Alternatives){w.WriteStartObject();w.WriteNumber("wordIndex",a.SelectedWordIndex);w.WriteBoolean("reused",a.ReusedWordSourceKey);w.WriteBase64String("scalar24",a.CopyScalarStorage24());w.WriteBase64String("scalar32",a.CopyScalarStorage32());w.WriteEndObject();}w.WriteEndArray();
 w.WriteNumber("nextId",owner.NextSelectionId);w.WriteStartArray("pending");foreach(var p in result.PendingSelections){w.WriteStartObject();w.WriteNumber("id",p.SelectionId);w.WriteString("name",p.Name);WriteRanges("ranges",p.CopyRanges());w.WriteEndObject();}w.WriteEndArray();
 Unsupported(()=>LegacyLibraryTextPublicationDocument.Prepare(scope,result));Unsupported(()=>LegacyLibraryTextPublicationDocument.Prepare(scope,result));
 var lines=LegacyFreshMainSourceLinePublication.PublishFresh(result);var text=LegacyFreshMainDocumentTextSelection.Prepare(lines);long beforeText=owner.NextSelectionId;var textSelection=text.Register();
 if(textSelection.SelectionId!=beforeText||owner.NextSelectionId!=beforeText+1||!ReferenceEquals(owner.OriginalSelectedInputContext,selectedInput)||selectedInput.State.NextRegistryId!=2)throw new InvalidDataException("TEXT lost the pending C20/INPUT lineage.");
 w.WriteNumber("textId",textSelection.SelectionId);w.WriteNumber("afterTextNext",owner.NextSelectionId);w.WriteString("textName",textSelection.Name);WriteRanges("textRanges",textSelection.CopyRanges());w.WriteNumber("afterTextPending",owner.CopyPendingSelections().Count);
 var consistency=LegacyFreshMainDocumentLineConsistency.Apply(text);
 var state=LegacyFreshMainDocumentSelectedSources.Prepare(LegacyFreshMainDocumentCharacterCoverage.Prepare(LegacyFreshMainDocumentSourceCoverage.Prepare(consistency)));
 var indices=LegacyFreshMainDocumentSourceIndices.Prepare(state);indices.Run();
 var cleanup=LegacyFreshMainDocumentControlCleanup.Prepare(indices);cleanup.Run();
 Unsupported(()=>LegacyFreshMainDocumentInputTransforms.Apply(cleanup));Unsupported(()=>LegacyFreshMainDocumentInputTransforms.Apply(cleanup));
 if(!ReferenceEquals(seed.CurrentTransforms,seed.InitialTransforms)||seed.CurrentTransforms.Count!=0)throw new InvalidDataException("Unsupported new transform route mutated or claimed its vector.");
 byte[] retainedCopy=context.Result.CopyInputRangeStorage();byte[] uuidCopy=context.Result.CopyIdentifier38();
 var retainedString=result.Source.Choices.SelectMany(c=>c.Glyphs).SelectMany(g=>g.Source.CopyRecords()).Select(r=>r.Text!).First();
 byte[] copiedString=retainedString.ReadBytes();
 if(!ReferenceEquals(owner.DocumentSeed,seed)||context.ResourcesReleased)throw new InvalidDataException("Downstream seed escaped its retained context lease.");
 w.WriteBoolean("sameSeedThroughSourceText",true);
 w.WriteNumber("chosen",chosen);w.WriteStartArray("allInputIds");foreach(var e in selected.State.Registry)w.WriteNumberValue(e.RegistryId);w.WriteEndArray();
 w.WriteNumber("sourceTag30",context.Result.SourceTag30);w.WriteNumber("originalNext",original.State.NextRegistryId);
 w.WriteBase64String("seed20",seed.CopyInitialScalarStorage20());
 w.WritePropertyName("requestTree");Tree(retainedRequest.Root);
 w.WriteStartArray("namespacePairs");foreach(var p in retainedRequest.Arena.CopyNamespacePairs()){w.WriteStartArray();w.WriteNumberValue(p.Identity);w.WriteStringValue(p.Prefix);w.WriteStringValue(p.Uri);w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("names");foreach(string name in retainedRequest.Arena.NamePool.CopyNames())w.WriteStringValue(name);w.WriteEndArray();w.WriteEndObject();
 foreach(var row in result.Source.Choices)
 {var copy=row.CopyScalarStorage48();copy[8]^=255;if(row.CopyScalarStorage48()[8]==copy[8])throw new InvalidDataException("WORD48 aliases its scalar snapshot.");}
 owner.Dispose();Reject(()=>owner.Publish(new byte[40],new byte[2040*4]));
 if(context.ResourcesReleased||!retainedString.ReadBytes().AsSpan().SequenceEqual(copiedString))throw new InvalidDataException("WORD disposal freed borrowed strings.");
 scope.Dispose();scope.Dispose();context.Dispose();
 if(!context.ResourcesReleased||!context.Result.CopyInputRangeStorage().AsSpan().SequenceEqual(retainedCopy)||!context.Result.CopyIdentifier38().AsSpan().SequenceEqual(uuidCopy))throw new InvalidDataException("Final child lease did not release storage or damaged detached snapshots.");
 Reject(()=>retainedString.ReadBytes());Reject(()=>seed.Validate(owner,default));Reject(()=>scope.PrepareWords(completion,norm,circles));Reject(()=>{_=retainedRequest.Root;});
 if(copiedString.Length==0)throw new InvalidDataException("Actual copied scalar UTF8 required.");foreignCompletion=completion;
}
w.WriteEndArray();
var failInput=LegacyLibraryOriginalSelectedFieldContext.Prepare(originals[0],originals[0].State.Inputs[1]);failInput.Run();
using(var failed=LegacyLibraryTextPublicationScope.PrepareConfiguredOriginalSelected(models,failInput,0,table,cases,publication))
{
 var context=failed.Context;var residue=new LegacyMainConfiguredAlternativeResidue(new byte[120],new byte[72],new byte[40],new ushort[256],new byte[64],new byte[4],new byte[4],new byte[4],new byte[4]);
 Reject(()=>failed.Run(residue,r=>new byte[r.RequiredBytes],new byte[39]));
 if(!failed.IsFaulted||context.ResourcesReleased||context.Result.CopyIdentifier38().Any(b=>b!=0))throw new InvalidDataException("Failed adopted pipeline lost its exact input/zero-UUID prefix.");
 Reject(()=>failed.Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]));
 failed.Dispose();if(!context.ResourcesReleased)throw new InvalidDataException("Failed scope lease was not released.");
}
w.WriteStartArray("importFaults");
for(int failure=0;failure<3;failure++)
{
 using var context=LegacyLibraryTextProcessingContext.CreateForOriginalSelected(failInput);
 if(failure<2)Reject(()=>context.PrepareInputPrepared(preparationFailure:failure));
 else Reject(()=>context.PrepareInputPrepared(cancellationStage:2));
 if(!context.IsFaulted||context.PreparedState18!=0||!ReferenceEquals(failInput.Original,originals[0]))throw new InvalidDataException("Failed selected import detached or cleared its original owner.");
 Reject(()=>context.PrepareInput());
 w.WriteStartObject();w.WriteNumber("stage",failure);w.WriteString("role",failure==2?"preparedCancellation":"preparedRefusal");w.WriteNumber("sourceTag30",context.Result.SourceTag30);w.WriteNumber("rangeCount",context.Result.RangeCount210);w.WriteNumber("flags",context.PreparedState18);w.WriteBoolean("boundOriginal",ReferenceEquals(context.Result.OriginalSelectedInput28,failInput));w.WriteEndObject();
 context.Dispose();if(!context.ResourcesReleased)throw new InvalidDataException("Unpublished faulted context retained resources.");
}
w.WriteEndArray();
using(var reusable=LegacyLibraryTextProcessingContext.CreateForOriginalSelected(failInput))
{
 Reject(()=>reusable.PrepareInput(new(true)));if(reusable.IsFaulted)throw new InvalidDataException("Preentry selected import cancellation consumed source.");
 reusable.PrepareInput();if(reusable.Result.SourceTag30!=1)throw new InvalidDataException("Reused preentry context lost chosen tag.");
}
w.WriteBoolean("sharedSpanWordChoices",true);w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS6 original-selected pre-NN contexts/12 alternatives/{totalWords} native WORD48/{totalGlyphs} glyph links/{reused} CHAR key reuses/{guards} guards; selected INPUT/C22-derived pending CHAR+WORD+TEXT, no document acceptance.");
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException or NotSupportedException){guards++;return;}throw new InvalidDataException("Required guard missing.");}
void Unsupported(Action a){try{a();}catch(NotSupportedException){guards++;return;}throw new InvalidDataException("An early unsupported-route guard must precede claims or mutation.");}
void WriteRanges(string name,LegacyNativeInkPositionRange[] ranges){w.WriteStartArray(name);foreach(var r in ranges){w.WriteStartArray();w.WriteNumberValue(r.First.Value);w.WriteNumberValue(r.Last.Value);w.WriteEndArray();}w.WriteEndArray();}
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}

void Tree(LegacyLibraryTextDomNode node)
{w.WriteStartObject();w.WriteNumber("id",node.Identity);w.WriteNumber("kind",node.Kind);w.WriteString("name",node.Name);w.WriteNumber("nameId",node.NameIdentity);w.WriteNumber("tag",node.Value.NativeTag);w.WriteBase64String("value",node.Value.CopyBytes());w.WriteStartArray("attributes");foreach(var a in node.Attributes)Tree(a);w.WriteEndArray();w.WriteStartArray("children");foreach(var c in node.Children)Tree(c);w.WriteEndArray();w.WriteEndObject();}
