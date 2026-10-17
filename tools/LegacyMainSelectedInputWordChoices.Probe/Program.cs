using InkRecognition;
using System.Text.Json;
if(args.Length!=11)throw new ArgumentException("AK LK engine separation case publication normalization circles line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);using var nf=File.OpenRead(args[6]);using var cr=File.OpenRead(args[7]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var norm=LegacyNativeUnicodeNormalizationData.Load(nf);var circles=LegacyNativeCircledPayloadData.Load(cr);
byte[] line30=File.ReadAllBytes(args[8]),line50=File.ReadAllBytes(args[9]);
InkStroke[][] samples=[[new(new InkPoint[]{new(0,90),new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)})],Word(0).Concat(Word(300)).ToArray(),Word(450).Concat(Word(0)).ToArray()];
using var output=File.Create(args[10]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteStartArray("samples");
int guards=0,totalWords=0,totalGlyphs=0,reused=0; LegacyFreshTextFieldInputContext? foreignInput=null;
for(int sample=0;sample<samples.Length;sample++)
{
 var residue=new LegacyMainConfiguredAlternativeResidue(new byte[120],new byte[72],new byte[40],new ushort[256],new byte[64],new byte[4],new byte[4],new byte[4],new byte[4]);
 var configured=LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak,lk,engine,line30,line50,samples[sample],0,table,cases,publication).Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]);
 var rootInput=LegacyFreshTextFieldInputContext.Prepare(LegacyFreshRawInkCurrentView.Prepare(configured.SourceDocument));
 var selectedInput=rootInput.CreateSelectedCopy();
 var completion=LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(ak,configured,norm,circles,new byte[4],new byte[4]);
 Reject(()=>LegacyFreshMainWordSourceChoices.PrepareSelectedInput(ak,completion,selectedInput,table,publication,norm,circles));completion.Run();
 var actualIndices=configured.Alternatives.CopyActiveAlternatives().Select(a=>LegacyOwnedMainCandidateSource.FromConfigured(configured,a).WordHead18!.WordIndex).ToArray();
 using var owner=LegacyFreshMainWordSourceChoices.PrepareSelectedInput(ak,completion,selectedInput,table,publication,norm,circles);
 if(!ReferenceEquals(owner.SelectedInputContext,selectedInput)||owner.NextSelectionId!=selectedInput.NextRegistryId||owner.CopyPendingSelections().Count!=0)throw new InvalidDataException("Native05F050 pending state is not bound to selected INPUT.");
 Reject(()=>LegacyFreshMainWordSourceChoices.PrepareSelectedInput(ak,completion,rootInput,table,publication,norm,circles));
 Reject(()=>LegacyFreshMainWordSourceChoices.PrepareSelectedInput(ak,completion,selectedInput,table,publication,norm,circles,new(true)));
 if(foreignInput is not null)Reject(()=>LegacyFreshMainWordSourceChoices.PrepareSelectedInput(ak,completion,foreignInput,table,publication,norm,circles));
 foreignInput=selectedInput;
 var initial=owner.WordViews.Select(word=>word.FlatReferences.Select(reference=>(Reference:reference,Record:owner.TopOwner.GlyphPool.CopyRecord(reference))).ToArray()).ToArray();
 Reject(()=>owner.Publish(new byte[39],new byte[2040*4]));Reject(()=>owner.Publish(new byte[40],new byte[2040*4],new(true)));
 var result=owner.Publish(new byte[40],new byte[2040*4]);Reject(()=>owner.Publish(new byte[40],new byte[2040*4]));
 if(result.Alternatives.Count!=2||result.Alternatives[0].ReusedWordSourceKey||!result.Alternatives[1].ReusedWordSourceKey||
    !ReferenceEquals(result.Alternatives[0].Source,result.Alternatives[1].Source))throw new InvalidDataException("Two actual alternatives must share one WORD source payload.");
 w.WriteStartObject();w.WriteNumber("inputTag",selectedInput.Input.RegistryId);w.WriteNumber("inputNext",selectedInput.NextRegistryId);w.WriteBase64String("inputCounter",selectedInput.CopyRegistryCounterStorage());w.WriteBase64String("inputRanges",selectedInput.Input.CopyRangeStorage());w.WriteNumber("inputSpans",selectedInput.Input.InputSpanCount);w.WriteNumber("sample",sample);w.WriteNumber("line",completion.Sources.CandidateSource.Sampled.Owner.SourceLineId);
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
 var lines=LegacyFreshMainSourceLinePublication.PublishFresh(result);var text=LegacyFreshMainDocumentTextSelection.Prepare(lines);long beforeText=owner.NextSelectionId;var textSelection=text.Register();
 if(textSelection.SelectionId!=beforeText||owner.NextSelectionId!=beforeText+1||!ReferenceEquals(owner.SelectedInputContext,selectedInput)||selectedInput.NextRegistryId!=1)throw new InvalidDataException("TEXT lost the pending C20/INPUT lineage.");
 w.WriteNumber("textId",textSelection.SelectionId);w.WriteNumber("afterTextNext",owner.NextSelectionId);w.WriteString("textName",textSelection.Name);WriteRanges("textRanges",textSelection.CopyRanges());w.WriteNumber("afterTextPending",owner.CopyPendingSelections().Count);w.WriteEndObject();
 foreach(var row in result.Source.Choices)
 {var copy=row.CopyScalarStorage48();copy[8]^=255;if(row.CopyScalarStorage48()[8]==copy[8])throw new InvalidDataException("WORD48 aliases its scalar snapshot.");}
 using(var failed=LegacyFreshMainWordSourceChoices.PrepareSelectedInput(ak,completion,selectedInput,table,publication,norm,circles))
 {Reject(()=>failed.Publish(new byte[40],[]));if(!failed.IsFaulted||failed.CopyPendingSelections().Count!=0)throw new InvalidDataException("Allocation refusal must poison its entered prefix before first CHAR registration.");Reject(()=>failed.Publish(new byte[40],new byte[2040*4]));}
 owner.Dispose();Reject(()=>owner.Publish(new byte[40],new byte[2040*4]));
}
w.WriteEndArray();w.WriteBoolean("sharedSpanWordChoices",true);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS3 actual configured shared spans/6 alternatives/{totalWords} native WORD48/{totalGlyphs} glyph links/{reused} CHAR key reuses/{guards} guards; selected INPUT/C22-derived pending CHAR+WORD+TEXT, no document acceptance.");
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException or NotSupportedException){guards++;return;}throw new InvalidDataException("Required guard missing.");}
void WriteRanges(string name,LegacyNativeInkPositionRange[] ranges){w.WriteStartArray(name);foreach(var r in ranges){w.WriteStartArray();w.WriteNumberValue(r.First.Value);w.WriteNumberValue(r.Last.Value);w.WriteEndArray();}w.WriteEndArray();}
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
