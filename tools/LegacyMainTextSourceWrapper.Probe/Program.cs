using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length<11||args.Length>15||args.Length%2!=1)throw new ArgumentException("AK LK engine separation case publication normalization circles line30 line50 capture [--controls prepared-json] [--recorded pinned-manifest]");
string? recorded=null;
for(int option=11;option<args.Length;option+=2)
{if(args[option]=="--controls")PreparedControls.Run(args[option+1]);else if(args[option]=="--recorded")recorded=args[option+1];else throw new ArgumentException("Unknown option.");}
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);using var nf=File.OpenRead(args[6]);using var cr=File.OpenRead(args[7]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);var norm=LegacyNativeUnicodeNormalizationData.Load(nf);var circles=LegacyNativeCircledPayloadData.Load(cr);
var context=LegacyFreshMainRecognitionContext.Load(ak,lk,engine,File.ReadAllBytes(args[8]),File.ReadAllBytes(args[9]));
var samples=new[]{Word(0),Word(0).Concat(Word(230)).ToArray(),Word(0).Concat(Word(300)).ToArray(),Word(0).Concat(Word(450)).ToArray(),Word(0).Concat(Word(700)).ToArray(),Word(450).Concat(Word(0)).ToArray(),Word(0).Select(s=>new InkStroke(s.Points.Concat(Enumerable.Repeat(s.Points[^1],3)).ToArray())).ToArray()};
string[]? recordedNames=null;
if(recorded is not null)(samples,recordedNames)=RecordedInputs.Load(recorded);
using var output=File.Create(args[10]);using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteStartArray("samples");int glyphs=0,reused=0,guards=0,sourceCount=0;
for(int sample=0;sample<samples.Length;sample++)
{
 var rawOwner=context.Prepare(samples[sample],0,table,cases,publication);var raw=rawOwner.Run(new byte[120],new byte[40],new ushort[256],new byte[64]);
 var candidate=LegacyFreshMainFirstCandidateCompletion.Complete(raw,new byte[400],new byte[4],new byte[4]);
 var original=candidate.RawWordsInNativeHeadOrder.Select(p=>p.Publication.PublishedWord!.Word.FlatReferences.Select(r=>(Reference:r,Record:raw.Session.Ownership.GlyphPool.CopyRecord(r))).ToArray()).ToArray();
 using var owner=LegacyFreshMainTextSourceWrapper.Prepare(ak,candidate,table,publication,norm,circles);
 Reject(()=>owner.Publish(new byte[39],new byte[2040*4]));Reject(()=>owner.Publish(new byte[40],new byte[2040*4],new CancellationToken(true)));
 var words=owner.Publish(new byte[40],new byte[2040*4]);Reject(()=>owner.Publish(new byte[40],new byte[2040*4]));
 var prefix=owner.WordSourceCollection;byte[] prefixBytes=prefix.CopySourceStorage26();
 if(prefixBytes.Length>0){byte saved=prefixBytes[0];prefixBytes[0]^=255;if(prefix.CopySourceStorage26()[0]!=saved)throw new InvalidDataException("Word source storage aliases returned bytes.");}
 Reject(()=>LegacyOwnedMainWordSourceCollection.ExportBeforeWordAugmentation(candidate,new CancellationToken(true)));
 w.WriteStartObject();w.WriteNumber("sample",sample);w.WriteNumber("line",raw.Sampled.Owner.SourceLineId);if(recordedNames is not null)w.WriteString("file",recordedNames[sample]);w.WriteStartArray("counts");foreach(var stroke in rawOwner.SourceDocument.Strokes)w.WriteNumberValue(stroke.Points.Count);w.WriteEndArray();w.WriteStartArray("words");
 int wordIndex=0;
 foreach(var word in words)
 {
  w.WriteStartObject();w.WriteNumber("sourceFlags",word.SourceFlags);w.WriteNumber("selectedFlags",word.SelectedFlags);WriteRanges("ordinaryRanges",word.CopyOrdinarySourceRanges());
  WriteRanges("wordClip",owner.WordSourceCollection.CopyRanges());w.WriteBase64String("wordSource26",owner.WordSourceCollection.CopySourceStorage26());
  w.WriteNumber("firstContext",owner.WordSourceCollection.FirstContext);w.WriteNumber("lastContext",owner.WordSourceCollection.LastContext);
  w.WriteStartArray("inputs");
  foreach(var input in original[wordIndex])
  {
   var actual=word.Glyphs.FirstOrDefault(g=>g.ProcessedSource.WordIndex==Array.IndexOf(original[wordIndex],input));
   w.WriteStartObject();w.WriteNumber("reference",input.Reference);w.WriteBase64String("initialGlyph40",input.Record);
   if(actual is not null)WriteRanges("originalRanges",actual.ProcessedSource.OriginalPositions.Ranges.ToArray());
   w.WriteEndObject();
  }
  w.WriteEndArray();w.WriteStartArray("glyphs");
  foreach(var glyph in word.Glyphs)
  {
   var source=glyph.Source;var processed=glyph.ProcessedSource;
   if(source.Field00!=0||source.Field08!=-1||source.Field1C!=0||source.Field40!=-1||source.Field48!=-1||source.Field64!=0||source.PriorRangeCount!=0)
    throw new InvalidDataException("Fresh024D40 scalar initialization changed.");
   if(!ReferenceEquals(processed.Owner.Document,rawOwner.SourceDocument)||!ReferenceEquals(source.SourceNode.Owner.Document,rawOwner.SourceDocument))throw new InvalidDataException("Original document identity lost.");
   w.WriteStartObject();w.WriteNumber("index",processed.WordIndex);w.WriteNumber("node",source.SourceNode.Identity);w.WriteBoolean("reused",glyph.ReusedSourceKey);w.WriteBoolean("positive",processed.IsOrdinarySource);
   w.WriteNumber("line",source.SourceLineId);w.WriteNumber("selectionId",source.SelectionId);w.WriteNumber("flags",source.Flags);w.WriteNumber("marker",glyph.SelectedMarker);
   var actualOwner=raw.Session.Ownership.GlyphPool.CopyOwnerNode(LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(glyph.GlyphReference));
   w.WriteBase64String("owner40",actualOwner.Records.Span);w.WriteBase64String("selectionRows4",glyph.InitialSelection.CopySelectionRows4());w.WriteNumber("initialMarker",glyph.InitialSelection.SelectedMarker!.Value);
   WriteRanges("key",processed.CopyRanges());WriteRanges("storedKey",source.SourceNode.FirstProcessedSource!.CopyRanges());
   w.WriteBase64String("selectedUtf8",glyph.CopySelectedUtf8());
   w.WriteStartArray("sourceRefs24");if(processed.OriginalPositions.OrdinarySource is{} ordinary)
    foreach(var r in ordinary.References){w.WriteStartArray();foreach(uint value in new[]{r.First.First,r.First.Second,r.First.Third,r.Last.First,r.Last.Second,r.Last.Third})w.WriteNumberValue(value);w.WriteEndArray();}
   w.WriteEndArray();w.WriteStartArray("records");foreach(var row in source.CopyRecords())
   {w.WriteStartObject();w.WriteBase64String("scalar40",row.CopyStorage40());w.WriteBase64String("utf8",row.Text!.ReadBytes());w.WriteNumber("stringId",row.Text.Identity);w.WriteEndObject();}w.WriteEndArray();
   w.WriteEndObject();glyphs++;if(glyph.ReusedSourceKey)reused++;
  }
  w.WriteEndArray();w.WriteEndObject();wordIndex++;
 }
 w.WriteEndArray();w.WriteNumber("nextId",owner.NextSelectionId);w.WriteStartArray("pending");
 foreach(var pending in owner.CopyPendingSelections())
 {w.WriteStartObject();w.WriteNumber("id",pending.SelectionId);w.WriteString("name",pending.Name);WriteRanges("ranges",pending.CopyRanges());w.WriteEndObject();sourceCount++;}
 w.WriteEndArray();w.WriteEndObject();
}
w.WriteEndArray();if(recorded is not null)w.WriteString("inputAssumption","Pinned recorded coordinates; pen-up metadata absent; one continuous stroke, expected labels never read.");w.WriteEndObject();w.Flush();Console.WriteLine($"PASS{samples.Length} actual candidates/{glyphs} processed glyph sources/{reused} reused keys/{sourceCount} pending CHAR registrations/{guards} guards; no0283B0 acceptance claim.");
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException){guards++;return;}throw new InvalidDataException("Required publication guard missing.");}
void WriteRanges(string name,LegacyNativeInkPositionRange[] ranges){w.WriteStartArray(name);foreach(var r in ranges){w.WriteStartArray();w.WriteNumberValue(r.First.Value);w.WriteNumberValue(r.Last.Value);w.WriteEndArray();}w.WriteEndArray();}
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
