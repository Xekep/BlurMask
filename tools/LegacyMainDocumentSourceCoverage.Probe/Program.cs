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
using var file=File.Create(args[10]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteStartArray("samples");int guards=0;
foreach(var raw in samples)
{
 var residue=new LegacyMainConfiguredAlternativeResidue(new byte[120],new byte[72],new byte[40],new ushort[256],new byte[64],new byte[4],new byte[4],new byte[4],new byte[4]);
 var result=LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak,lk,engine,l30,l50,raw,0,table,cases,publication).Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]);
 var completed=LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(ak,result,norm,circles,new byte[4],new byte[4]);completed.Run();
 using var words=LegacyFreshMainWordSourceChoices.Prepare(ak,completed,table,publication,norm,circles);
 var choices=words.Publish(new byte[40],new byte[2040*4]);var original=LegacyFreshMainSourceLinePublication.PublishFresh(choices);
 var text=LegacyFreshMainDocumentTextSelection.Prepare(original);text.Register();var lines=LegacyFreshMainDocumentLineConsistency.Apply(text);
 byte[] before=lines.CopyStorage56();var pending=text.CopyPendingSelections().ToArray();
 Reject(()=>LegacyFreshMainDocumentSourceCoverage.Prepare(lines,new(true)));
 var source=LegacyFreshMainDocumentSourceCoverage.Prepare(lines);
 if(source.NativeSelectedAlternativeIndex!=0||source.SourceMetadataEnabled||source.WordSourceNodeCount!=1||
  !ReferenceEquals(source.LineConsistency,lines)||!ReferenceEquals(source.Text,text)||!ReferenceEquals(source.Document,words.Document)||
  !ReferenceEquals(source.SelectedAlternative,choices.Alternatives[0])||!text.CopyPendingSelections().SequenceEqual(pending)||
  !source.CopySourceLineStorage56().AsSpan().SequenceEqual(before)||!original.CopyStorage56().AsSpan(56).SequenceEqual(new byte[56]))
  throw new InvalidDataException("Source-owned postprocessing changed owner/order/lines/commands.");
 w.WriteStartObject();w.WriteNumber("selected",source.NativeSelectedAlternativeIndex);w.WriteBoolean("metadataEnabled",source.SourceMetadataEnabled);
 w.WriteNumber("sourceNodes",source.WordSourceNodeCount);w.WriteNumber("sourceOffset",completed.Sources.CandidateSource.Preprocessed.Lines.Owner.Controller.SourceOffset);
 w.WriteBase64String("selectedAlternative32",source.SelectedAlternative.CopyScalarStorage32());w.WriteBase64String("beforeLines56",before);w.WriteBase64String("afterLines56",source.CopySourceLineStorage56());
 w.WriteNumber("pendingBefore",pending.Length);w.WriteNumber("pendingAfter",text.CopyPendingSelections().Count);w.WriteString("lastPending",pending[^1].Name);
 WriteRanges("wordSourceKey",choices.Source.CopySourceKeyRanges());WriteRanges("wordUnion",source.CopyWordRanges());WriteRanges("textKey",source.CopyTextRanges());w.WriteEndObject();
 var copy=source.CopyWordRanges();copy[0]=default;if(source.CopyWordRanges()[0]==default)throw new InvalidDataException("Source union aliases caller storage.");
 var glyph=choices.Source.Choices.SelectMany(row=>row.Glyphs).First(g=>g.ProcessedSource.IsOrdinarySource).ProcessedSource;
 pending[0].Owner.CreateCharacterSelection(glyph);Reject(()=>LegacyFreshMainDocumentSourceCoverage.Prepare(lines));
 words.Dispose();Reject(()=>LegacyFreshMainDocumentSourceCoverage.Prepare(lines));
}
w.WriteEndArray();w.WriteStartArray("preparedEqualityDiscriminators");
for(int i=0;i<256;i++)
{
 ulong last=((ulong)(i+1)<<16)|0xff05;
 LegacyNativeInkPositionRange[] left=[new(new(3),new(last))],right=[new(new(0xff03),new(last))];
 bool equal=LegacyOwnedRawInkRangeOperations.Equal(left,right),contains=LegacyOwnedRawInkRangeOperations.Contains(left,right[0]);
 if(equal||!contains)throw new InvalidDataException("Raw C21.VE8 must differ from normalized containment.");
 w.WriteStartObject();WriteRanges("left",left);WriteRanges("right",right);w.WriteBoolean("equal",equal);w.WriteBoolean("contains",contains);w.WriteEndObject();
}
w.WriteEndArray();w.WriteEndObject();w.Flush();Console.WriteLine($"PASS4 actual source owners/nativeBC0/live22Afalse/singleWORD027670/4 exact0248D0 keys/256 prepared equality discriminators/{guards} guards; no accepted document.");
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException){guards++;return;}throw new InvalidDataException("Missing source coverage gate.");}
void WriteRanges(string name,LegacyNativeInkPositionRange[] ranges){w.WriteStartArray(name);foreach(var range in ranges){w.WriteStartArray();w.WriteNumberValue(range.First.Value);w.WriteNumberValue(range.Last.Value);w.WriteEndArray();}w.WriteEndArray();}
InkStroke[] Rotate(InkStroke[] raw,float a)=>raw.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*MathF.Cos(a)-p.Y*MathF.Sin(a),p.X*MathF.Sin(a)+p.Y*MathF.Cos(a))).ToArray())).ToArray();
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
