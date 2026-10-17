using InkRecognition;
using System.Buffers.Binary;
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
using var file=File.Create(args[10]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteStartArray("samples");int guards=0,discriminators=0;
for(int index=0;index<samples.Length;index++)
{
 var residue=new LegacyMainConfiguredAlternativeResidue(new byte[120],new byte[72],new byte[40],new ushort[256],new byte[64],new byte[4],new byte[4],new byte[4],new byte[4]);
 var result=LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak,lk,engine,l30,l50,samples[index],0,table,cases,publication).Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]);
 var completed=LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(ak,result,norm,circles,new byte[4],new byte[4]);completed.Run();
 using var words=LegacyFreshMainWordSourceChoices.Prepare(ak,completed,table,publication,norm,circles);
 var choices=words.Publish(new byte[40],new byte[2040*4]);Reject(()=>LegacyFreshMainSourceLinePublication.PublishFresh(choices,new(true)));
 var lines=LegacyFreshMainSourceLinePublication.PublishFresh(choices);
 if(!ReferenceEquals(lines.WordChoices,choices)||lines.LineCount!=2||lines.Alternatives.Count!=2||lines.Alternatives[0].PreviousLineCount!=0||lines.Alternatives[1].PreviousLineCount!=1)throw new InvalidDataException("Native append counts lost.");
 if(!lines.CopyStorage56().AsSpan(56).SequenceEqual(new byte[56]))throw new InvalidDataException("Second native append is a zero tail, not a second write index.");
 byte[] guide=lines.CopyGuide40();float currentHeight=BinaryPrimitives.ReadSingleLittleEndian(guide.AsSpan(4)),originalHeight=BinaryPrimitives.ReadSingleLittleEndian(guide.AsSpan(16));
 if(currentHeight!=originalHeight)discriminators++;
 w.WriteStartObject();w.WriteNumber("input",index);w.WriteNumber("rotation",lines.SourceRotation);w.WriteBase64String("descriptor88",lines.CopyDescriptor88());w.WriteBase64String("guide40",guide);w.WriteBase64String("storage56",lines.CopyStorage56());
 w.WriteStartArray("calls");foreach(var call in lines.Alternatives){w.WriteStartObject();w.WriteNumber("before",call.PreviousLineCount);w.WriteNumber("count",call.LineCount);w.WriteNumber("wrapperBefore",call.WrapperOffsetBefore);w.WriteNumber("wrapperAfter",call.WrapperOffsetAfter);w.WriteNumber("wordIndex",call.Alternative.SelectedWordIndex);w.WriteBase64String("storage56",call.CopyStorage56());w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
 guide[16]^=255;if(guide[16]==lines.CopyGuide40()[16])throw new InvalidDataException("Guide snapshot aliases output.");
 words.Dispose();Reject(()=>LegacyFreshMainSourceLinePublication.PublishFresh(choices));
}
w.WriteEndArray();w.WriteNumber("heightDomainDiscriminators",discriminators);w.WriteEndObject();w.Flush();
if(discriminators==0)throw new InvalidDataException("Actual raw input must distinguish guide0/4 from pre-deskew guideC/10.");
Console.WriteLine($"PASS4 actual raw/configured2/sharedWORD owners/8 native021AB0 calls/8 full56B allocations/{discriminators} actual height-domain discriminators/{guards} guards; no accepted document/font claim.");
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException){guards++;return;}throw new InvalidDataException("Missing source-line ownership guard.");}
InkStroke[] Rotate(InkStroke[] input,float angle)=>input.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint((float)(p.X*Math.Cos(angle)-p.Y*Math.Sin(angle)),(float)(p.X*Math.Sin(angle)+p.Y*Math.Cos(angle)))).ToArray())).ToArray();
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
