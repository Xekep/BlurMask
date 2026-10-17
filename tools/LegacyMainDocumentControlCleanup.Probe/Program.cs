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
using var file=File.Create(args[10]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteStartArray("samples");int guards=0,chars=0,rowCount=0;
foreach(var raw in samples)
{
 var residue=new LegacyMainConfiguredAlternativeResidue(new byte[120],new byte[72],new byte[40],new ushort[256],new byte[64],new byte[4],new byte[4],new byte[4],new byte[4]);
 var result=LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak,lk,engine,l30,l50,raw,0,table,cases,publication).Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]);
 var completed=LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(ak,result,norm,circles,new byte[4],new byte[4]);completed.Run();
 using var words=LegacyFreshMainWordSourceChoices.Prepare(ak,completed,table,publication,norm,circles);
 var choices=words.Publish(new byte[40],new byte[2040*4]);var lines=LegacyFreshMainSourceLinePublication.PublishFresh(choices);
 var text=LegacyFreshMainDocumentTextSelection.Prepare(lines);text.Register();var lineResult=LegacyFreshMainDocumentLineConsistency.Apply(text);
 var source=LegacyFreshMainDocumentSourceCoverage.Prepare(lineResult);
 var coverage=LegacyFreshMainDocumentCharacterCoverage.Prepare(source);
 var state=LegacyFreshMainDocumentSelectedSources.Prepare(coverage);


 var indices=LegacyFreshMainDocumentSourceIndices.Prepare(state);
 Reject(()=>LegacyFreshMainDocumentControlCleanup.Prepare(indices)); //native020920 must precede023260
 indices.Run();
 var commands=text.CopyPendingSelections().ToArray();long next=words.NextSelectionId;
 uint wordBefore=state.WordFlags;var wordRows=choices.Source.Choices.Select(c=>c.CopyScalarStorage48()).ToArray();
 var records=state.Characters.ToDictionary(c=>c.OriginalSource.SourceNode.Identity,c=>c.OriginalSource.CopyRecords().Select(r=>r.CopyStorage40()).ToArray());
 Reject(()=>LegacyFreshMainDocumentControlCleanup.Prepare(indices,new(true)));
 var cleanup=LegacyFreshMainDocumentControlCleanup.Prepare(indices);Reject(()=>cleanup.Run(new(true)));
 if(cleanup.Succeeded||cleanup.IsFaulted||cleanup.WordMaskCleared||cleanup.WordRowsInspected!=0)throw new InvalidDataException("Pre-entry cancellation changed cleanup prefix.");
 Reject(()=>LegacyFreshMainDocumentControlCleanup.Prepare(indices));cleanup.Run();
 if(!cleanup.Succeeded||cleanup.IsFaulted||!cleanup.WordMaskCleared||cleanup.WordRowsInspected!=wordRows.Length||cleanup.WordFlags!=(wordBefore&0xffefffff))throw new InvalidDataException("Native WORD pass or flag clear differs.");
 Reject(()=>cleanup.Run());
 var insertion=state.Characters[0].OriginalSource.SourceNode.Owner.CopyInsertionOrder();
 if(!cleanup.CopyCharacterVisitOrder().SequenceEqual(insertion.Select(n=>n.Identity)))throw new InvalidDataException("CHAR pass did not preserve insertion-list order.");
 if(state.WordFlags!=wordBefore||!text.CopyPendingSelections().SequenceEqual(commands)||words.NextSelectionId!=next)throw new InvalidDataException("Historical flags or pending commands changed.");
 w.WriteStartObject();w.WriteNumber("wordBefore",wordBefore);w.WriteNumber("wordAfter",cleanup.WordFlags);w.WriteBoolean("readyBefore",indices.IsReady);w.WriteBoolean("succeeded",cleanup.Succeeded);w.WriteBoolean("wordCleared",cleanup.WordMaskCleared);w.WriteNumber("rowsInspected",cleanup.WordRowsInspected);w.WriteNumber("pendingBefore",commands.Length);w.WriteNumber("pendingAfter",text.CopyPendingSelections().Count);
 w.WriteStartArray("words");for(int i=0;i<wordRows.Length;i++)
 {
  var row=choices.Source.Choices[i];var after=row.CopyScalarStorage48();if(!after.AsSpan().SequenceEqual(wordRows[i]))throw new InvalidDataException("WORD48 changed.");
  w.WriteStartObject();w.WriteBase64String("before48",wordRows[i]);w.WriteBase64String("after48",after);w.WriteStartArray("selectedLinks");foreach(var link in row.CharacterLinks){w.WriteStartArray();w.WriteNumberValue(link.Source.SourceNode.Identity);w.WriteNumberValue(link.SelectedMarker);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
 }
 w.WriteEndArray();w.WriteStartArray("characters");
 foreach(var node in insertion)
 {
  var c=state.Characters.Single(c=>c.OriginalSource.SourceNode.Identity==node.Identity);var rows=c.OriginalSource.CopyRecords();
  w.WriteStartObject();w.WriteNumber("id",node.Identity);w.WriteNumber("flags",c.Flags);w.WriteStartArray("records");
  for(int i=0;i<rows.Count;i++){var after=rows[i].CopyStorage40();if(!after.AsSpan().SequenceEqual(records[node.Identity][i]))throw new InvalidDataException("CHAR40 changed.");w.WriteStartObject();w.WriteBase64String("before40",records[node.Identity][i]);w.WriteBase64String("after40",after);w.WriteBase64String("utf8",rows[i].Text!.ReadBytes());w.WriteEndObject();}
  w.WriteEndArray();w.WriteEndObject();
 }
 w.WriteEndArray();w.WriteStartArray("visits");foreach(int id in cleanup.CopyCharacterVisitOrder())w.WriteNumberValue(id);w.WriteEndArray();w.WriteEndObject();
 chars+=insertion.Count;rowCount+=wordRows.Length;
 state.OriginalWordSource.SourceNode.Owner.InternPrepared([]);Reject(()=>cleanup.ValidateCurrent());words.Dispose();Reject(()=>cleanup.ValidateCurrent());
}
w.WriteEndArray();w.WriteEndObject();w.Flush();Console.WriteLine($"PASS4 genuine sources/{rowCount} actualWORD48 rows/{chars} insertion-order CHAR owners/{guards} guards; scoped023260 no-control only, transforms/finalacceptance pending.");
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException or NotSupportedException){guards++;return;}throw new InvalidDataException("Missing cleanup owner gate.");}
InkStroke[] Rotate(InkStroke[] raw,float a)=>raw.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*MathF.Cos(a)-p.Y*MathF.Sin(a),p.X*MathF.Sin(a)+p.Y*MathF.Cos(a))).ToArray())).ToArray();
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
