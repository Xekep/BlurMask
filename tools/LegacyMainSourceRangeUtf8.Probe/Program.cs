using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if(args.Length!=12)throw new ArgumentException("AK LK engine separation case publication normalization circles line30 line50 controls capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);using var nf=File.OpenRead(args[6]);using var cr=File.OpenRead(args[7]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var norm=LegacyNativeUnicodeNormalizationData.Load(nf);var circles=LegacyNativeCircledPayloadData.Load(cr);
byte[] line30=File.ReadAllBytes(args[8]),line50=File.ReadAllBytes(args[9]);
using var file=File.Create(args[11]);using var w=new Utf8JsonWriter(file);using var controls=JsonDocument.Parse(File.ReadAllBytes(args[10]));
var doc=LegacyOwnedRawInkDocument.Create([new(new InkPoint[]{new(0,0),new(1,1)})],default);
var operations=LegacyOwnedRawInkRangeOperations.Prepare(doc);int guards=0,sourceCount=0;
w.WriteStartObject();w.WriteStartArray("prepared");
foreach(var c in controls.RootElement.GetProperty("cases").EnumerateArray())
{
 var rows=c.GetProperty("rows").EnumerateArray().Select(r=>new LegacyNativeInkPositionRange(new(r[0].GetUInt64()),new(r[1].GetUInt64()))).ToArray();
 var result=LegacyNativeSourceRangeUtf8.Format(rows,operations,c.GetProperty("fail").GetInt32(),c.GetProperty("final").GetBoolean());
 var expected=c.GetProperty("expected");
 if(result.Succeeded!=expected.GetProperty("succeeded").GetBoolean()||!result.Bytes.AsSpan().SequenceEqual(expected.GetProperty("utf8").GetBytesFromBase64()))throw new InvalidDataException("Native formatter status/prefix differs from independent oracle.");
 w.WriteStartObject();w.WriteBoolean("succeeded",result.Succeeded);w.WriteBoolean("canonical",result.WasCanonical);
 w.WriteStartArray("rows");foreach(var r in result.Ranges){w.WriteStartArray();w.WriteNumberValue(r.First.Value);w.WriteNumberValue(r.Last.Value);w.WriteEndArray();}w.WriteEndArray();
 uint flags=c.GetProperty("flags").GetUInt32();bool refused=result.Succeeded&&result.Bytes.Length==0&&(flags&0x400000)!=0;
 if(refused)Reject(()=>LegacyNativeSourceRangeUtf8.CallerFirstBrace(result.Bytes,flags));
 var caller=result.Succeeded&&!refused?LegacyNativeSourceRangeUtf8.CallerFirstBrace(result.Bytes,flags):result.Bytes;
 w.WriteBase64String("utf8",result.Bytes);w.WriteBase64String("callerUtf8",caller);w.WriteBoolean("callerRefused",refused);
 w.WriteNumber("attempts",result.AllocationAttempts);w.WriteNumber("capacity",result.Capacity);w.WriteEndObject();
}
w.WriteEndArray();
for(int f=200;f<255;f++)Reject(()=>LegacyNativeSourceRangeUtf8.Format([new(new((ulong)f<<8|3),new((ulong)f<<8|5))],operations));
Reject(()=>LegacyNativeSourceRangeUtf8.Format([new(new(0x8000000000000003),new(0x8000000000000005))],operations));
Reject(()=>LegacyNativeSourceRangeUtf8.Format([],operations,token:new(true)));
//The exact200 carry is independently checked for every quantization bin.
for(int f=0;f<200;f++)
{
 var position=LegacyNativeInkPosition.Pack(0,f/200f,3,2);
 if(position.PointIndex!=0||position.Fraction!=f)throw new InvalidDataException("Actual C79 fraction domain differs.");
}
var carry=LegacyNativeInkPosition.Pack(0,.99999f,3,2);
if(carry.PointIndex!=1||carry.Fraction!=255)throw new InvalidDataException("Rounded200 was not carried to actual terminalFF.");
w.WriteStartArray("actual");
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
 var handoff=LegacyFreshMainDocumentCounterHandoff.Apply(LegacyFreshMainDocumentInputTransforms.Apply(cleanup));
 var before=handoff.TransactionOwner.CopyPendingCommands().ToArray();var registry=selected.CopyRegistryCounterStorage();
 Reject(()=>LegacyFreshMainSourceRangeUtf8.Prepare(handoff,new(true)));
 var result=LegacyFreshMainSourceRangeUtf8.Prepare(handoff);
 if(!ReferenceEquals(result.CounterHandoff,handoff)||!ReferenceEquals(result.Seed,words.DocumentSeed)||!ReferenceEquals(result.Document,configured.SourceDocument))throw new InvalidDataException("Lost retained native source context.");
 var insertion=state.Characters[0].OriginalSource.SourceNode.Owner.CopyInsertionOrder();
 if(!insertion.Select(n=>n.Identity).SequenceEqual(result.Sources.Where(s=>s.Kind=="CHAR").Select(s=>s.SourceNodeIdentity)))throw new InvalidDataException("Native CHAR insertion order replaced by AVL order.");
 w.WriteStartObject();w.WriteNumber("sample",index);w.WriteStartArray("pointCounts");foreach(var s in configured.SourceDocument.Strokes)w.WriteNumberValue(s.Points.Count);w.WriteEndArray();
 w.WriteBase64String("registryBefore",registry);w.WriteBase64String("registryAfter",selected.CopyRegistryCounterStorage());w.WriteNumber("pendingBefore",before.Length);w.WriteNumber("pendingAfter",handoff.TransactionOwner.CopyPendingCommands().Count);
 w.WriteStartArray("sources");foreach(var source in result.Sources)
 {
  sourceCount++;if(!ReferenceEquals(source.Owner,result))throw new InvalidDataException("Lost source format owner.");
  w.WriteStartObject();w.WriteString("kind",source.Kind);w.WriteNumber("id",source.SourceNodeIdentity);w.WriteNumber("flags",source.SourceFlags);
  w.WriteBase64String("original16",Storage(source.CopyOriginalRanges()));w.WriteBase64String("normalized16",Storage(source.CopyNormalizedRanges()));w.WriteBase64String("utf8",source.CopyUtf8());w.WriteBase64String("callerUtf8",source.CopyCallerUtf8());w.WriteEndObject();
  var original=source.CopyOriginalRanges();var bytes=source.CopyUtf8();var oldOriginal=original.ToArray();var oldBytes=bytes.ToArray();
  if(original.Length!=0)original[0]=default;if(bytes.Length!=0)bytes[0]=0;
  if(!source.CopyOriginalRanges().AsSpan().SequenceEqual(oldOriginal)||!source.CopyUtf8().AsSpan().SequenceEqual(oldBytes))throw new InvalidDataException("Formatted source snapshot exposes mutable arrays.");
  if(!ReferenceEquals(source.Owner.Document,configured.SourceDocument))throw new InvalidDataException("Foreign source document.");
 }
 w.WriteEndArray();w.WriteEndObject();
 if(before.Where((c,i)=>c.Selection!=handoff.TransactionOwner.CopyPendingCommands()[i].Selection||c.Counter!=handoff.TransactionOwner.CopyPendingCommands()[i].Counter).Any())throw new InvalidDataException("Formatting modified pending command owners.");
}
w.WriteEndArray();w.WriteEndObject();w.Flush();Console.WriteLine($"PASS2053 raw formatter controls/4 actual documents/{sourceCount} retained source strings/{guards} guards; no DOM construction/acceptance.");
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException or NotSupportedException){guards++;return;}throw new InvalidDataException("Missing range formatting guard.");}
byte[] Storage(LegacyNativeInkPositionRange[] ranges){var b=new byte[ranges.Length*16];for(int i=0;i<ranges.Length;i++){BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(i*16),ranges[i].First.Value);BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(i*16+8),ranges[i].Last.Value);}return b;}
InkStroke[] Rotate(InkStroke[] raw,float a)=>raw.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*MathF.Cos(a)-p.Y*MathF.Sin(a),p.X*MathF.Sin(a)+p.Y*MathF.Cos(a))).ToArray())).ToArray();
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
