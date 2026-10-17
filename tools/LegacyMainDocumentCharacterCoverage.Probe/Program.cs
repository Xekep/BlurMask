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
InkStroke[][] samples=[m,Word(0).Concat(Word(300)).ToArray(),Word(450).Concat(Word(0)).ToArray(),Rotate(m,.18f)];
using var file=File.Create(args[10]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteStartArray("samples");int guards=0,subtractions=0;
foreach(var raw in samples)
{
 var residue=new LegacyMainConfiguredAlternativeResidue(new byte[120],new byte[72],new byte[40],new ushort[256],new byte[64],new byte[4],new byte[4],new byte[4],new byte[4]);
 var result=LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak,lk,engine,l30,l50,raw,0,table,cases,publication).Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]);
 var completed=LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(ak,result,norm,circles,new byte[4],new byte[4]);completed.Run();
 using var words=LegacyFreshMainWordSourceChoices.Prepare(ak,completed,table,publication,norm,circles);
 var choices=words.Publish(new byte[40],new byte[2040*4]);var lines=LegacyFreshMainSourceLinePublication.PublishFresh(choices);
 var text=LegacyFreshMainDocumentTextSelection.Prepare(lines);text.Register();var lineResult=LegacyFreshMainDocumentLineConsistency.Apply(text);
 var source=LegacyFreshMainDocumentSourceCoverage.Prepare(lineResult);var commands=text.CopyPendingSelections().ToArray();
 var before48=choices.Source.Choices.Select(c=>c.CopyScalarStorage48()).ToArray();uint flagsBefore=choices.Source.Flags;
 Reject(()=>LegacyFreshMainDocumentCharacterCoverage.Prepare(source,new(true)));
 var coverage=LegacyFreshMainDocumentCharacterCoverage.Prepare(source);
 if(!ReferenceEquals(coverage.SourceCoverage,source)||!ReferenceEquals(coverage.Text,text)||!text.CopyPendingSelections().SequenceEqual(commands)||choices.Source.Flags!=flagsBefore)
  throw new InvalidDataException("Per-WORD range scan changed its source/selection state.");
 w.WriteStartObject();w.WriteStartArray("counts");foreach(var stroke in words.Document.Strokes)w.WriteNumberValue(stroke.Points.Count);w.WriteEndArray();
 WriteRanges("wordOriginal",choices.Source.CopySourceKeyRanges());WriteRanges("wordOrdinary",choices.Source.CopyOrdinaryRanges());
 w.WriteNumber("wordCount",choices.Source.Choices.Count);w.WriteNumber("subtractions",coverage.CharacterSubtractionCount);
 w.WriteBase64String("lineBefore56",source.CopySourceLineStorage56());w.WriteBase64String("lineAfter56",source.CopySourceLineStorage56());
 w.WriteNumber("pendingBefore",commands.Length);w.WriteNumber("pendingAfter",text.CopyPendingSelections().Count);w.WriteString("lastPending",commands[^1].Name);
 w.WriteStartArray("words");
 foreach(var call in coverage.Words)
 {
  int index=call.NativeWordChoiceIndex;if(!ReferenceEquals(call.WordChoice,choices.Source.Choices[index])||call.RemainingRangeCount!=0)throw new InvalidDataException("Actual WORD source subtraction mismatch.");
  var after=call.WordChoice.CopyScalarStorage48();if(!after.AsSpan().SequenceEqual(before48[index]))throw new InvalidDataException("WORD48 changed during temporary range subtraction.");
  w.WriteStartObject();w.WriteNumber("index",index);w.WriteBoolean("ordinary",call.UsesOrdinaryWordRanges);w.WriteBase64String("before48",before48[index]);w.WriteBase64String("after48",after);
  WriteRanges("initial",call.CopyInitialRanges());WriteRanges("remaining",call.CopyRemainingRanges());w.WriteStartArray("chars");
  foreach(var c in call.Characters)
  {
   if(!ReferenceEquals(c.Source,call.WordChoice.CharacterLinks[c.NativeCharacterIndex].Source))throw new InvalidDataException("CHAR source identity changed.");
   w.WriteStartObject();w.WriteNumber("index",c.NativeCharacterIndex);w.WriteBoolean("prior",c.UsesPriorRanges);w.WriteBoolean("positive",c.Source.FirstSource.IsOrdinarySource);
   w.WriteNumber("priorCount",c.Source.PriorRangeCount);w.WriteBase64String("sourceKey16",c.Source.SourceNode.CopyRanges16());WriteRanges("key",c.CopyRanges());w.WriteEndObject();
   var copied=c.CopyRanges();if(copied.Length!=0){copied[0]=default;if(c.CopyRanges()[0]==default)throw new InvalidDataException("CHAR range key is a mutable alias.");}
  }
  w.WriteEndArray();w.WriteEndObject();
  var initial=call.CopyInitialRanges();if(initial.Length!=0){initial[0]=default;if(call.CopyInitialRanges()[0]==default)throw new InvalidDataException("WORD range key is a mutable alias.");}
 }
 w.WriteEndArray();w.WriteEndObject();subtractions+=coverage.CharacterSubtractionCount;
 var genuine=choices.Source.Choices.SelectMany(c=>c.Glyphs).First(g=>g.ProcessedSource.IsOrdinarySource).ProcessedSource;
 commands[0].Owner.CreateCharacterSelection(genuine);Reject(()=>LegacyFreshMainDocumentCharacterCoverage.Prepare(source));
 words.Dispose();Reject(()=>LegacyFreshMainDocumentCharacterCoverage.Prepare(source));
}
w.WriteEndArray();w.WriteEndObject();w.Flush();Console.WriteLine($"PASS4 actual configured sources/8 reverse-order WORD calls/{subtractions} forward CHAR subtractions/8 empty remainders/{guards} guards; no024A90 split or document acceptance.");
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException){guards++;return;}throw new InvalidDataException("Missing character coverage gate.");}
void WriteRanges(string name,LegacyNativeInkPositionRange[] ranges){w.WriteStartArray(name);foreach(var range in ranges){w.WriteStartArray();w.WriteNumberValue(range.First.Value);w.WriteNumberValue(range.Last.Value);w.WriteEndArray();}w.WriteEndArray();}
InkStroke[] Rotate(InkStroke[] raw,float a)=>raw.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*MathF.Cos(a)-p.Y*MathF.Sin(a),p.X*MathF.Sin(a)+p.Y*MathF.Cos(a))).ToArray())).ToArray();
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
