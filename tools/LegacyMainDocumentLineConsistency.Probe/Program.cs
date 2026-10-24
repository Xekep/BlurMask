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
using var file=File.Create(args[10]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteStartArray("samples");
int guards=0,visits=0,checks=0;
foreach(var raw in samples)
{
 var residue=new LegacyMainConfiguredAlternativeResidue(new byte[120],new byte[72],new byte[40],new ushort[256],new byte[64],new byte[4],new byte[4],new byte[4],new byte[4]);
 var result=LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak,lk,engine,l30,l50,raw,0,table,cases,publication).Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]);
 bool enabledAugmentationRefused=false;
 try{_ = LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(ak,result,norm,circles,new byte[4],new byte[4]);}
 catch(NotSupportedException e)when(e.GetType()==typeof(NotSupportedException)&&e.Message=="Enabled219580 candidate augmentation cannot be omitted."){enabledAugmentationRefused=true;}
 if(!enabledAugmentationRefused)throw new InvalidDataException("Exact enabled-bit8 NoCandidate refusal required before augmentation.");
 var completed=LegacyFreshMainConfiguredWordAugmentation.Prepare(ak,result,table,norm,circles,residue);completed.Run();
 if(!completed.IsTopV68Completed||completed.IsFaulted||!completed.SourcesMarkedComplete||
    completed.EligibleCandidates!=2||completed.AppendedWords!=0||completed.DuplicateWords!=2||completed.UpdatedNewDuplicates!=0||completed.QuotaAfter!=2)
  throw new InvalidDataException("Genuine fixed-fixture augmentation must complete the duplicate-only native mark/rank prefix.");
 using var words=LegacyFreshMainWordSourceChoices.PrepareAugmented(ak,completed,table,publication,norm,circles);
 var choices=words.Publish(new byte[40],new byte[2040*4]);var original=LegacyFreshMainSourceLinePublication.PublishFresh(choices);
 var text=LegacyFreshMainDocumentTextSelection.Prepare(original);Reject(()=>LegacyFreshMainDocumentLineConsistency.Apply(text));
 var command=text.Register();var commands=text.CopyPendingSelections().ToArray();
 Reject(()=>LegacyFreshMainDocumentLineConsistency.Apply(text,new(true)));
 byte[] beforeLines=original.CopyStorage56();var beforeWords=choices.Source.Choices.Select(row=>row.CopyScalarStorage48()).ToArray();
 var beforeChars=choices.Source.Choices.Select(row=>row.CharacterLinks.Select(link=>(link.CopyScalarStorage24(),link.Source.Flags,link.Source.SourceLineId)).ToArray()).ToArray();
 var processed=LegacyFreshMainDocumentLineConsistency.Apply(text);
 if(!ReferenceEquals(processed.Text,text)||!ReferenceEquals(processed.WordChoices,choices)||processed.LineCount!=1||processed.LastReferencedLine!=0||
  !original.CopyStorage56().AsSpan().SequenceEqual(beforeLines)||original.LineCount!=2||
  !text.CopyPendingSelections().SequenceEqual(commands)||!ReferenceEquals(text.Selection,command))throw new InvalidDataException("Postprocessing changed prior snapshots/commands or lost its exact owner.");
 w.WriteStartObject();w.WriteBase64String("before56",beforeLines);w.WriteBase64String("after56",processed.CopyStorage56());
 w.WriteBase64String("descriptor88",original.CopyDescriptor88());w.WriteBase64String("guide40",original.CopyGuide40());
 w.WriteNumber("sourceFlags",choices.Source.Flags);w.WriteNumber("maxLine",processed.LastReferencedLine);w.WriteNumber("visits",processed.WordVisits);
 w.WriteNumber("helperCalls",processed.HelperCalls025940);w.WriteNumber("adjacent",processed.AdjacentCharacterChecks);
 w.WriteNumber("pendingBefore",commands.Length);w.WriteNumber("pendingAfter",text.CopyPendingSelections().Count);w.WriteString("lastPending",commands[^1].Name);
 w.WriteStartArray("selectedWordIndices");foreach(var alternative in choices.Alternatives)w.WriteNumberValue(alternative.SelectedWordIndex);w.WriteEndArray();
 w.WriteStartArray("words");
 for(int i=0;i<choices.Source.Choices.Count;i++)
 {
  var row=choices.Source.Choices[i];var after=row.CopyScalarStorage48();if(!after.AsSpan().SequenceEqual(beforeWords[i]))throw new InvalidDataException("WORD48 changed in proven no-op branch.");
  w.WriteStartObject();w.WriteBase64String("before48",beforeWords[i]);w.WriteBase64String("after48",after);w.WriteStartArray("chars");
  for(int j=0;j<row.CharacterLinks.Count;j++)
  {
   var link=row.CharacterLinks[j];var before=beforeChars[i][j];var after24=link.CopyScalarStorage24();
   if(!before.Item1.AsSpan().SequenceEqual(after24)||before.Flags!=link.Source.Flags||before.SourceLineId!=link.Source.SourceLineId)throw new InvalidDataException("CHAR changed in025940 equality branch.");
   w.WriteStartObject();w.WriteNumber("line",link.Source.SourceLineId);w.WriteNumber("flags",link.Source.Flags);w.WriteBase64String("before24",before.Item1);w.WriteBase64String("after24",after24);w.WriteEndObject();
  }
  w.WriteEndArray();w.WriteEndObject();
 }
 w.WriteEndArray();w.WriteEndObject();visits+=processed.WordVisits;checks+=processed.AdjacentCharacterChecks;
 var copy=processed.CopyStorage56();copy[8]^=255;if(copy.AsSpan().SequenceEqual(processed.CopyStorage56()))throw new InvalidDataException("Updated source lines alias caller storage.");
 if(!LegacyFreshMainDocumentLineConsistency.Apply(text).CopyStorage56().AsSpan().SequenceEqual(processed.CopyStorage56()))throw new InvalidDataException("Immutable replay differs.");
 var genuine=choices.Source.Choices.SelectMany(row=>row.Glyphs).First(g=>g.ProcessedSource.IsOrdinarySource).ProcessedSource;
 commands[0].Owner.CreateCharacterSelection(genuine);Reject(()=>LegacyFreshMainDocumentLineConsistency.Apply(text));
 words.Dispose();Reject(()=>LegacyFreshMainDocumentLineConsistency.Apply(text));
}
w.WriteEndArray();w.WriteEndObject();w.Flush();Console.WriteLine($"PASS4 actual configured source owners/{visits} native WORD visits/{checks} actual adjacent checks/4 zero-tail trims/{guards} guards; pending TEXT only, no accepted document.");
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException){guards++;return;}throw new InvalidDataException("Missing line-consistency gate.");}
InkStroke[] Rotate(InkStroke[] raw,float a)=>raw.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*MathF.Cos(a)-p.Y*MathF.Sin(a),p.X*MathF.Sin(a)+p.Y*MathF.Cos(a))).ToArray())).ToArray();
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
