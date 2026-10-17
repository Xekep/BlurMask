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
using var file=File.Create(args[10]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteStartArray("samples");int guards=0,subtractions=0,sampleIndex=0,totalNodes=0,totalWrites=0;
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
 var beforeCharacters=choices.Source.Choices.SelectMany(c=>c.CharacterLinks).Select(c=>c.Source).Distinct().ToDictionary(c=>c.SourceNode.Identity,c=>(Flags:c.Flags,Index:unchecked((int)(c.Field40>>32))));
 var coverage=LegacyFreshMainDocumentCharacterCoverage.Prepare(source);
 Reject(()=>LegacyFreshMainDocumentSelectedSources.Prepare(coverage,new(true)));
 var state=LegacyFreshMainDocumentSelectedSources.Prepare(coverage);
 state.Validate();
 int actualSelected=choices.Alternatives[0].SelectedWordIndex;
 var actualTree=state.Characters[0].OriginalSource.SourceNode.Owner;
 if(!ReferenceEquals(state.SelectedWord,choices.Source.Choices[actualSelected])||state.WordSelectedChoiceIndex44!=actualSelected||
  !actualTree.CopyTree().SequenceEqual(state.CharacterTree)||actualTree.RootIdentity!=state.CharacterTreeRootIdentity||actualTree.Count!=state.Characters.Count)
  throw new InvalidDataException("Selected WORD or complete actual CHAR tree differs.");
 if(!ReferenceEquals(coverage.SourceCoverage,source)||!ReferenceEquals(coverage.Text,text)||!text.CopyPendingSelections().SequenceEqual(commands)||choices.Source.Flags!=flagsBefore)
  throw new InvalidDataException("Per-WORD range scan changed its source/selection state.");
 w.WriteStartObject();w.WriteStartArray("counts");foreach(var stroke in words.Document.Strokes)w.WriteNumberValue(stroke.Points.Count);w.WriteEndArray();
 WriteRanges("wordOriginal",choices.Source.CopySourceKeyRanges());WriteRanges("wordOrdinary",choices.Source.CopyOrdinaryRanges());
 w.WriteNumber("wordCount",choices.Source.Choices.Count);w.WriteNumber("subtractions",coverage.CharacterSubtractionCount);
 w.WriteBase64String("lineBefore56",source.CopySourceLineStorage56());w.WriteBase64String("lineAfter56",source.CopySourceLineStorage56());
 w.WriteNumber("pendingBefore",commands.Length);w.WriteNumber("pendingAfter",text.CopyPendingSelections().Count);w.WriteString("lastPending",commands[^1].Name);
 w.WriteStartObject("selectedSources");w.WriteNumber("root",actualTree.RootIdentity);w.WriteNumber("actualTreeCount",actualTree.Count);w.WriteStartArray("tree");
 foreach(var n in state.CharacterTree){w.WriteStartArray();w.WriteNumberValue(n.Identity);w.WriteNumberValue(n.Left);w.WriteNumberValue(n.Right);w.WriteNumberValue(n.Balance);w.WriteEndArray();}w.WriteEndArray();
 w.WriteNumber("selectedWord",state.WordSelectedChoiceIndex44);w.WriteNumber("nativeSelectedWord",actualSelected);w.WriteNumber("wordBefore",flagsBefore);w.WriteNumber("wordMetadata",state.WordMetadataFlags);w.WriteNumber("wordFlags",state.WordFlags);
 w.WriteStartArray("selectedLinks");foreach(var link in choices.Source.Choices[actualSelected].CharacterLinks){w.WriteStartArray();w.WriteNumberValue(link.Source.SourceNode.Identity);w.WriteNumberValue(link.SelectedMarker);w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("writes");foreach(var write in state.SelectionWrites){w.WriteStartArray();w.WriteNumberValue(write.TreeOrdinal);w.WriteNumberValue(write.SourceNodeIdentity);w.WriteNumberValue(write.SelectedMarker);w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("characters");foreach(var c in state.Characters)
 {
  var old=beforeCharacters[c.OriginalSource.SourceNode.Identity];if(old.Flags!=c.OriginalSource.Flags||old.Index!=unchecked((int)(c.OriginalSource.Field40>>32)))throw new InvalidDataException("Previous CHAR payload snapshot was mutated.");
  w.WriteStartObject();w.WriteNumber("node",c.OriginalSource.SourceNode.Identity);w.WriteNumber("before",old.Flags);w.WriteNumber("metadata",c.MetadataFlags);w.WriteNumber("flags",c.Flags);w.WriteNumber("before44",old.Index);w.WriteNumber("index44",c.SelectedChoiceIndex44);w.WriteNumber("selectionId",c.OriginalSource.SelectionId);w.WriteBase64String("key16",c.OriginalSource.SourceNode.CopyRanges16());w.WriteEndObject();
 }
 w.WriteEndArray();w.WriteEndObject();totalNodes+=state.Characters.Count;totalWrites+=state.SelectionWrites.Count;
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
 if(sampleIndex++%2==0)commands[0].Owner.CreateCharacterSelection(genuine);
 else state.Characters[0].OriginalSource.SourceNode.Owner.InternPrepared([]); //controlled missing-payload guard, not an inferred source
 Reject(()=>LegacyFreshMainDocumentSelectedSources.Prepare(coverage));
 Reject(()=>state.Validate());
 words.Dispose();Reject(()=>LegacyFreshMainDocumentSelectedSources.Prepare(coverage));Reject(()=>state.Validate());
}
w.WriteEndArray();w.WriteStartArray("preparedAliasControls");
for(int i=0;i<256;i++)
{
 int n=2+i%7;uint[] before=Enumerable.Range(0,n).Select(k=>unchecked((uint)(i*1640531527L+k*2654435761L))).ToArray();int[] beforeIndices=Enumerable.Repeat(-1,n).ToArray();
 int[] sourceOrdinals=[0,1,n-1,0,1,0];int[] markers=[i%11,-1,int.MinValue,5,int.MaxValue,-i];var flags=before.ToArray();var indices=beforeIndices.ToArray();
 LegacyFreshMainDocumentSelectedSources.ApplySelectedCharacters(flags,indices,sourceOrdinals,markers);
 w.WriteStartObject();w.WriteStartArray("beforeFlags");foreach(uint f in before)w.WriteNumberValue(f);w.WriteEndArray();w.WriteStartArray("beforeIndices");foreach(int value in beforeIndices)w.WriteNumberValue(value);w.WriteEndArray();w.WriteStartArray("ordinals");foreach(int value in sourceOrdinals)w.WriteNumberValue(value);w.WriteEndArray();w.WriteStartArray("markers");foreach(int value in markers)w.WriteNumberValue(value);w.WriteEndArray();w.WriteStartArray("flags");foreach(uint value in flags)w.WriteNumberValue(value);w.WriteEndArray();w.WriteStartArray("indices");foreach(int value in indices)w.WriteNumberValue(value);w.WriteEndArray();w.WriteEndObject();
}
w.WriteEndArray();Reject(()=>LegacyFreshMainDocumentSelectedSources.ApplySelectedCharacters(new uint[1],new int[2],[],[]));
Reject(()=>LegacyFreshMainDocumentSelectedSources.ApplySelectedCharacters(new uint[1],new int[1],[1],[0]));
int[] overlap=[0];Reject(()=>LegacyFreshMainDocumentSelectedSources.ApplySelectedCharacters(new uint[1],overlap,overlap,[0]));
w.WriteEndObject();w.Flush();Console.WriteLine($"PASS4 actual configured sources/{totalNodes} complete CHAR nodes/{totalWrites} selected writes/256 prepared alias-order controls/{guards} guards; immutable01FD40+020000 only, no020920 or document acceptance.");
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException or NotSupportedException){guards++;return;}throw new InvalidDataException("Missing selected-source gate.");}
void WriteRanges(string name,LegacyNativeInkPositionRange[] ranges){w.WriteStartArray(name);foreach(var range in ranges){w.WriteStartArray();w.WriteNumberValue(range.First.Value);w.WriteNumberValue(range.Last.Value);w.WriteEndArray();}w.WriteEndArray();}
InkStroke[] Rotate(InkStroke[] raw,float a)=>raw.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*MathF.Cos(a)-p.Y*MathF.Sin(a),p.X*MathF.Sin(a)+p.Y*MathF.Cos(a))).ToArray())).ToArray();
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
