using InkRecognition;
using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
if(args.Length!=11)throw new ArgumentException("AK LK engine separation case publication normalization circles line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);using var nf=File.OpenRead(args[6]);using var cr=File.OpenRead(args[7]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var norm=LegacyNativeUnicodeNormalizationData.Load(nf);var circles=LegacyNativeCircledPayloadData.Load(cr);
byte[] line30=File.ReadAllBytes(args[8]),line50=File.ReadAllBytes(args[9]);
using var file=File.Create(args[10]);using var w=new Utf8JsonWriter(file);int guards=0,totalNodes=0,totalFaults=0;
w.WriteStartObject();w.WriteStartArray("actual");
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
 var handoff=LegacyFreshMainDocumentCounterHandoff.Apply(LegacyFreshMainDocumentInputTransforms.Apply(cleanup));var source=LegacyFreshMainSourceRangeUtf8.Prepare(handoff);
 var registry=selected.CopyRegistryCounterStorage();var pending=handoff.Commands.Select(c=>c.CommandIdentity).ToArray();
 Reject(()=>LegacyFreshMainDetachedTextDom.Prepare(source,new(true)));
 var builder=LegacyFreshMainDetachedTextDom.Prepare(source);Reject(()=>builder.Build(new(true)));var result=builder.Build();
 if(!result.Succeeded||!ReferenceEquals(result.Sources,source)||!ReferenceEquals(result.Arena.Seed,words.DocumentSeed)||!ReferenceEquals(result.Arena.Document,configured.SourceDocument))throw new InvalidDataException("Lost exact detached DOM ownership.");
 Reject(()=>builder.Build());Reject(()=>LegacyFreshMainDetachedTextDom.Prepare(source));
 Check(result.Root,result.Arena);totalNodes+=result.Arena.CopyStates().Length;
 w.WriteStartObject();w.WriteNumber("sample",index);w.WriteBase64String("uuid36",words.DocumentSeed!.LibraryIdentifier38.CopyUtf8Storage36());
 w.WriteBase64String("registryBefore",registry);w.WriteBase64String("registryAfter",selected.CopyRegistryCounterStorage());w.WriteStartArray("pendingBefore");foreach(int id in pending)w.WriteNumberValue(id);w.WriteEndArray();w.WriteStartArray("pendingAfter");foreach(var c in handoff.TransactionOwner.CopyPendingCommands())w.WriteNumberValue(c.CommandIdentity);w.WriteEndArray();
 w.WriteStartArray("characters");foreach(var s in source.Sources.Where(s=>s.CharacterSource is not null))
 {
  var p=s.CharacterSource!;w.WriteStartObject();w.WriteNumber("id",s.SourceNodeIdentity);w.WriteNumber("flags",s.SourceFlags);w.WriteNumber("tag",p.SelectionId);w.WriteNumber("line",p.SourceLineId);w.WriteNumber("freeze",p.Field1C);w.WriteNumber("force",unchecked((int)p.Field40));w.WriteBase64String("ranges16",Storage(s.CopyOriginalRanges()));
  w.WriteStartArray("records40");foreach(var row in p.CopyRecords()){w.WriteStartObject();w.WriteBase64String("raw",row.CopyStorage40());w.WriteBase64String("text",row.Text!.ReadBytes());w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
 }
 w.WriteEndArray();var word=source.Sources.Single(s=>s.WordSource is not null);w.WriteStartObject("word");w.WriteNumber("id",word.SourceNodeIdentity);w.WriteNumber("flags",word.SourceFlags);w.WriteNumber("tag",word.WordSource!.SelectionId);w.WriteBase64String("ranges16",Storage(word.CopyOriginalRanges()));w.WriteStartArray("rows48");
 foreach(var row in word.WordSource.Choices)
 {w.WriteStartObject();w.WriteBase64String("raw",row.CopyScalarStorage48());w.WriteStartArray("refs24");foreach(var link in row.CharacterLinks){w.WriteStartObject();w.WriteNumber("charId",link.Source.SourceNode.Identity);w.WriteBase64String("raw",link.CopyScalarStorage24());w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();}
 w.WriteEndArray();w.WriteEndObject();w.WriteStartObject("text");w.WriteNumber("tag",text.Selection.SelectionId);w.WriteNumber("selected",result.NativeTextSelectedBC);w.WriteNumber("freeze",result.NativeTextFreezeB8);w.WriteBase64String("ranges16",Storage(text.CopyRanges()));w.WriteStartArray("rows32");foreach(var a in choices.Alternatives){w.WriteStartObject();w.WriteBase64String("raw",a.CopyScalarStorage32());w.WriteBase64String("ref24",a.CopyScalarStorage24());w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();w.WriteBase64String("lines56",state.CharacterCoverage.SourceCoverage.LineConsistency.CopyStorage56());
 w.WritePropertyName("result");WriteResult(builder,result);
 if(index==0)
 {
  w.WriteStartArray("faults");for(int failure=0;failure<result.Arena.Attempts;failure++)
  {
   var b=LegacyFreshMainDetachedTextDom.Prepare(LegacyFreshMainSourceRangeUtf8.Prepare(handoff));LegacyOwnedMainDetachedTextDom? r=null;string status="result";
   try{r=b.BuildPrepared(failure);}catch(NotSupportedException){status="unsupported";}
   if(!b.IsFaulted||r?.Succeeded==true)throw new InvalidDataException("Prepared native-service failure published a root.");
   if(r is not null)Reject(()=>_=r.Root);Reject(()=>b.Build());totalFaults++;
   w.WriteStartObject();w.WriteNumber("fail",failure);w.WriteString("status",status);w.WritePropertyName("result");WriteResult(b,r);w.WriteEndObject();
  }
  w.WriteEndArray();
  Reject(()=>LegacyFreshMainDetachedTextDom.ValidateOrdinaryFlags(0x400));Reject(()=>LegacyFreshMainDetachedTextDom.ValidateOrdinaryFlags(0x800));
  var cancelled=LegacyFreshMainDetachedTextDom.Prepare(LegacyFreshMainSourceRangeUtf8.Prepare(handoff));Reject(()=>cancelled.BuildPrepared(cancelAt:5));Reject(()=>cancelled.Build());
  w.WritePropertyName("cancelled");WriteResult(cancelled,null);
  var a=new LegacyLibraryTextDomArena(words.DocumentSeed);var foreign=new LegacyLibraryTextDomArena(words.DocumentSeed);var parent=a.Element("p")!;var child=foreign.Element("q")!;
  Reject(()=>a.Append(parent,child));Reject(()=>a.Append(parent,parent));a.Release(parent);Reject(()=>a.StringAttribute(parent,"dead",[1]));
  w.WriteStartArray("literal");foreach(int length in new[]{0,1,15,16,17,500,501})
  {
   byte[] raw=Enumerable.Repeat((byte)'&',length).Concat(new byte[]{0,60,62}).ToArray();var arena=new LegacyLibraryTextDomArena(words.DocumentSeed);var n=arena.Text(raw)!;
   if(n.Value.Length!=length)throw new InvalidDataException("Literal NUL/SBO boundary differs.");
   w.WriteStartObject();w.WriteBase64String("raw",raw);w.WriteNumber("tag",n.Tag);w.WriteBase64String("text",n.Value);
   if(length>=16){var failedArena=new LegacyLibraryTextDomArena(words.DocumentSeed,1);if(failedArena.Text(raw)is not null)throw new InvalidDataException("Missing native value-copy failure.");w.WriteStartArray("failureTrace");foreach(var t in failedArena.Trace)w.WriteStringValue(t);w.WriteEndArray();}
   w.WriteEndObject();
  }w.WriteEndArray();
  w.WriteStartArray("numeric");foreach(byte tag in new byte[]{4,5,6,7,8})
  {
   int width=tag is 4 or 5?8:tag==6?1:4;var arena=new LegacyLibraryTextDomArena(words.DocumentSeed);var node=arena.Element("numeric")!;
   byte[] raw=Enumerable.Repeat((byte)255,width).ToArray();if(!arena.NumericAttribute(node,"v",tag,raw))throw new InvalidDataException("Numeric assignment failed.");
   var value=arena.Snapshot(node).Attributes[0].Value;w.WriteStartObject();w.WriteNumber("tag",tag);w.WriteBase64String("raw",raw);w.WriteBase64String("value",value.CopyBytes());w.WriteEndObject();
  }w.WriteEndArray();
  var operations=LegacyOwnedRawInkRangeOperations.Prepare(source.Document);int capacity=0;
  LegacyNativeInkPositionRange R(uint id,ushort first,ushort last)=>new(new((ulong)id<<32|(ulong)first<<16|3),new((ulong)id<<32|(ulong)last<<16|0xff05));
  LegacyNativeInkPositionRange[][] scratchInputs=[[R(0,0,1)],[R(2147483647,65534,65535)],[R(0,2,8),R(0,0,4)],[R(0,0,1)]];
  w.WriteStartArray("scratchControls");foreach(var rows in scratchInputs)
  {
   int before=capacity;var attempts=new List<string>();var formatted=LegacyNativeSourceRangeUtf8.FormatIntoScratch(rows,operations,capacity,kind=>{attempts.Add(kind);return true;});capacity=formatted.Capacity;
   w.WriteStartObject();w.WriteNumber("before",before);w.WriteNumber("after",capacity);w.WriteBase64String("ranges16",Storage(rows));w.WriteBase64String("utf8",formatted.Bytes);w.WriteStartArray("allocations");foreach(var kind in attempts)w.WriteStringValue(kind);w.WriteEndArray();w.WriteEndObject();
  }w.WriteEndArray();
 }
 w.WriteEndObject();
}
w.WriteEndArray();w.WriteEndObject();w.Flush();Console.WriteLine($"PASS4 actual detached trees/{totalNodes} arena nodes/{totalFaults} service-failure prefixes/{guards} guards; no parent/BE2/C20 acceptance.");
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException or NotSupportedException){guards++;return;}throw new InvalidDataException("Missing DOM lifetime/foreign-owner guard.");}
void Check(LegacyLibraryTextDomNode n,LegacyLibraryTextDomArena a){if(!ReferenceEquals(n.Arena,a))throw new InvalidDataException("Foreign tree owner.");foreach(var c in n.Attributes.Concat(n.Children))Check(c,a);var b=n.Value.CopyBytes();if(b.Length!=0){byte before=b[0];b[0]^=255;if(n.Value.CopyBytes()[0]!=before)throw new InvalidDataException("Mutable DOM value exposure.");}}
void WriteResult(LegacyFreshMainDetachedTextDom b,LegacyOwnedMainDetachedTextDom? r)
{
 var arena=b.Arena!;w.WriteStartObject();w.WriteBoolean("succeeded",r?.Succeeded??false);w.WriteNumber("attempts",arena.Attempts);w.WriteNumber("capacity",b.ScratchCapacity);w.WriteBase64String("scratch",b.ScratchBytes);
 w.WriteStartArray("trace");foreach(var t in arena.Trace)w.WriteStringValue(t);w.WriteEndArray();w.WritePropertyName("tree");if(r?.Succeeded==true)WriteNode(r.Root);else w.WriteNullValue();
 w.WriteStartArray("arena");foreach(var n in arena.CopyStates()){w.WriteStartObject();w.WriteNumber("id",n.Identity);w.WriteNumber("kind",n.Kind);w.WriteString("name",n.Name);w.WriteNumber("nameId",n.NameIdentity);w.WriteNumber("tag",n.Tag);w.WriteBase64String("value",n.Value);w.WriteNumber("parent",n.Parent);w.WriteBoolean("released",n.Released);w.WriteStartArray("attributes");foreach(int id in n.Attributes)w.WriteNumberValue(id);w.WriteEndArray();w.WriteStartArray("children");foreach(int id in n.Children)w.WriteNumberValue(id);w.WriteEndArray();w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
}
void WriteNode(LegacyLibraryTextDomNode n){w.WriteStartObject();w.WriteNumber("id",n.Identity);w.WriteNumber("kind",n.Kind);w.WriteString("name",n.Name);w.WriteNumber("nameId",n.NameIdentity);w.WriteNumber("tag",n.Value.NativeTag);w.WriteBase64String("value",n.Value.CopyBytes());w.WriteStartArray("attributes");foreach(var a in n.Attributes)WriteNode(a);w.WriteEndArray();w.WriteStartArray("children");foreach(var c in n.Children)WriteNode(c);w.WriteEndArray();w.WriteEndObject();}
byte[] Storage(LegacyNativeInkPositionRange[] ranges){var b=new byte[ranges.Length*16];for(int i=0;i<ranges.Length;i++){BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(i*16),ranges[i].First.Value);BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(i*16+8),ranges[i].Last.Value);}return b;}
InkStroke[] Rotate(InkStroke[] raw,float a)=>raw.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*MathF.Cos(a)-p.Y*MathF.Sin(a),p.X*MathF.Sin(a)+p.Y*MathF.Cos(a))).ToArray())).ToArray();
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
