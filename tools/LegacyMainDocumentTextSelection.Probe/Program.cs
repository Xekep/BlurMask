using InkRecognition;
using System.Text.Json;
if(args.Length!=11)throw new ArgumentException("AK LK engine separation case publication normalization circles line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);using var nf=File.OpenRead(args[6]);using var cr=File.OpenRead(args[7]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var norm=LegacyNativeUnicodeNormalizationData.Load(nf);var circles=LegacyNativeCircledPayloadData.Load(cr);
byte[] l30=File.ReadAllBytes(args[8]),l50=File.ReadAllBytes(args[9]);
InkStroke[] m=[new(new InkPoint[]{new(0,90),new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)})];
InkStroke[][] samples=[m,Word(0).Concat(Word(300)).ToArray(),Word(450).Concat(Word(0)).ToArray()];
using var file=File.Create(args[10]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteStartArray("samples");int guards=0,rangeCount=0;
for(int index=0;index<samples.Length;index++)
{
 var residue=new LegacyMainConfiguredAlternativeResidue(new byte[120],new byte[72],new byte[40],new ushort[256],new byte[64],new byte[4],new byte[4],new byte[4],new byte[4]);
 var result=LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak,lk,engine,l30,l50,samples[index],0,table,cases,publication).Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]);
 var completed=LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(ak,result,norm,circles,new byte[4],new byte[4]);completed.Run();
 using var words=LegacyFreshMainWordSourceChoices.Prepare(ak,completed,table,publication,norm,circles);
 var choices=words.Publish(new byte[40],new byte[2040*4]);var lines=LegacyFreshMainSourceLinePublication.PublishFresh(choices);
 Reject(()=>LegacyFreshMainDocumentTextSelection.Prepare(lines,new(true)));
 var text=LegacyFreshMainDocumentTextSelection.Prepare(lines);Reject(()=>_=text.Selection);
 Reject(()=>text.Register(new(true)));var selection=text.Register();Reject(()=>text.Register());
 if(!ReferenceEquals(selection.Owner,text.CopyPendingSelections()[0].Owner)||selection.Name!="TEXT"||selection.SelectionId!=words.NextSelectionId-1)throw new InvalidDataException("TEXT command ownership lost.");
 var duplicate=LegacyFreshMainDocumentTextSelection.Prepare(lines);Reject(()=>duplicate.Register());if(!duplicate.IsFaulted)throw new InvalidDataException("Duplicate transaction registration must retain a faulted entry.");
 if(!choices.PendingSelections.SequenceEqual(text.CopyPendingSelections().Take(choices.PendingSelections.Count)))throw new InvalidDataException("Existing CHAR/WORD commands changed.");
 w.WriteStartObject();w.WriteNumber("input",index);w.WriteStartArray("pointCounts");foreach(var stroke in words.Document.Strokes)w.WriteNumberValue(stroke.Points.Count);w.WriteEndArray();
 w.WriteStartArray("selectedInputs");foreach(var row in text.CopySelectedInputs()){w.WriteStartArray();w.WriteNumberValue(row.SourceIndex);w.WriteNumberValue(row.PointCount);w.WriteNumberValue(row.FirstPoint);w.WriteNumberValue(row.LastPoint);w.WriteNumberValue(row.FirstFraction);w.WriteNumberValue(row.LastFraction);w.WriteBooleanValue(row.FirstClosed);w.WriteBooleanValue(row.LastClosed);w.WriteBooleanValue(row.IsKnownText);w.WriteEndArray();}w.WriteEndArray();
 WriteRanges("rootC0",text.CopyRanges());w.WriteNumber("textId",selection.SelectionId);w.WriteNumber("nextId",words.NextSelectionId);
 w.WriteNumber("prefixCount",choices.PendingSelections.Count);w.WriteStartArray("pending");foreach(var p in text.CopyPendingSelections()){w.WriteStartObject();w.WriteNumber("id",p.SelectionId);w.WriteString("name",p.Name);w.WriteNumber("code",p.OperationCode);w.WriteNumber("enabled",p.Enabled);w.WriteNumber("priority",p.Priority);w.WriteNumber("identity",p.CommandIdentity);w.WriteNumber("previous",p.PreviousCommandIdentity);WriteRanges("ranges",p.CopyRanges());w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();rangeCount+=text.CopyRanges().Length;
 var copy=text.CopyRanges();copy[0]=default;if(text.CopyRanges()[0]==default)throw new InvalidDataException("Root range snapshot aliases caller storage.");
 using(var badWords=LegacyFreshMainWordSourceChoices.Prepare(ak,completed,table,publication,norm,circles))
 {
  var badChoices=badWords.Publish(new byte[40],new byte[2040*4]);var badLines=LegacyFreshMainSourceLinePublication.PublishFresh(badChoices);
  var badText=LegacyFreshMainDocumentTextSelection.Prepare(badLines);var context=badChoices.PendingSelections[0].Owner;
  var genuineSource=badChoices.Source.Choices.SelectMany(row=>row.Glyphs).First(g=>g.ProcessedSource.IsOrdinarySource).ProcessedSource;
  context.CreateCharacterSelection(genuineSource);long next=context.NextSelectionId;Reject(()=>badText.Register());
  if(!badText.IsFaulted||context.NextSelectionId!=next)throw new InvalidDataException("Foreign registration prefix must refuse before the TEXT counter write.");
 }
 words.Dispose();Reject(()=>LegacyFreshMainDocumentTextSelection.Prepare(lines));
}
w.WriteEndArray();w.WriteEndObject();w.Flush();Console.WriteLine($"PASS3 genuine configured sources/6 actual alternatives/{rangeCount} NULL-projected original-object ranges/3 pendingTEXT/{guards} guards; no document commit or accepted string.");
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException){guards++;return;}throw new InvalidDataException("Missing TEXT lifecycle guard.");}
void WriteRanges(string name,LegacyNativeInkPositionRange[] ranges){w.WriteStartArray(name);foreach(var r in ranges){w.WriteStartArray();w.WriteNumberValue(r.First.Value);w.WriteNumberValue(r.Last.Value);w.WriteEndArray();}w.WriteEndArray();}
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
