using InkRecognition;
using System.Buffers.Binary;
using System.Text;
using System.Text.Json;

if(args.Length!=11)throw new ArgumentException("AK engine separation case publication line30 line50 capture LK normalization circles");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[8]),engine=File.ReadAllBytes(args[1]);
using var ed=JsonDocument.Parse(engine);var a=ed.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);byte[] line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
using var nf=File.OpenRead(args[9]);using var cpf=File.OpenRead(args[10]);var normalization=LegacyNativeUnicodeNormalizationData.Load(nf);var payloads=LegacyNativeCircledPayloadData.Load(cpf);
using var file=File.Create(args[7]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();int guards=0;
w.WriteStartArray("prepared");
for(int sample=0;sample<320;sample++)
{
 byte[] record=new byte[40];ushort symbol=(ushort)(sample<192?sample:sample<224?0xffeb+((sample-192)%21):(sample-224)%16);
 ushort kind=(ushort)(sample<224?0:sample<288?1:(sample-288)%8);
 BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(4),kind);BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(8),symbol);
 var input=new[]{new LegacyNativeGlyphRecordNode(7,4,record)};var pool=new LegacyNativeGlyphControlImport(input,model.AcousticAlphabet);pool.Execute();
 w.WriteStartObject();w.WriteNumber("sample",sample);w.WriteBase64String("original",record);WritePool(pool);
 if(pool.Succeeded)
 {
  int actualKind=pool.Resolve(7).Symbol.KindFlags&7;
  if(actualKind==4){Reject(()=>LegacyImportedControlGlyphBytes.ReadCore(pool,7,default));w.WriteBoolean("refusedAlternate",true);}
  else
  {
   var glyph=LegacyImportedControlGlyphBytes.ReadCore(pool,7,default);WriteGlyph(glyph);
   Reject(()=>LegacyImportedControlGlyphBytes.ReadCore(pool,7,new(true)));
   var copy=glyph.CopyBytes();if(copy.Length>0){copy[0]^=127;if(glyph.CopyBytes().AsSpan().SequenceEqual(copy))throw new Exception("Glyph byte storage leaked.");}
  }
 }
 else Reject(()=>LegacyImportedControlGlyphBytes.ReadCore(pool,7,default));
 w.WriteEndObject();
}
w.WriteEndArray();
InkStroke M(float y)=>new(new InkPoint[]{new(0,y+90),new(0,y+10),new(25,y+60),new(50,y+10),new(50,y+90)});
InkStroke V(float y)=>new(new InkPoint[]{new(0,y+10),new(0,y+90)});
var mir=new InkStroke[]{M(0),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})};
var rows=new[]{V(0),new InkStroke(V(0).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray()),V(140),new InkStroke(V(140).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray())};

int ownerIndex=0;ulong identity=0x1000;w.WriteStartArray("actual");
foreach(var raw in new[]{new[]{M(0),M(140)},new[]{M(0),M(250)},new[]{V(0),V(140)},new[]{V(0),V(250)},mir,mir.Concat(mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+160)).ToArray()))).ToArray(),rows,Enumerable.Range(0,14).Select(n=>new InkStroke(M(0).Points.Select(p=>new InkPoint(p.X+n*70,p.Y)).ToArray())).ToArray()})
{
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd).Run();
    var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table);var global=LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table);
    var graph=LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,global);var source=LegacyFreshMainGlobalSourceRanges.Prepare(graph);var context=LegacyFreshMainGlobalContext.Prepare(ak,source,activations);var geometry=LegacyFreshMainGlobalGlyphGeometry.Prepare(ak,context);
    var owner=LegacyFreshMainGlobalCaseSession.Prepare(ak,lk,model,geometry,cd,pd);var cache=LegacyFreshMainGlobalTransitionCache.Prepare(ak,owner,table);var transitions=LegacyFreshMainGlobalWordTransitions.Prepare(ak,cache);
    var costs=LegacyFreshMainGlobalGroupCosts.Prepare(ak,owner,table);var policy=LegacyFreshMainGlobalWordControllerPolicy.Prepare(ak,owner,costs,transitions);
    var confidence=LegacyFreshMainGlobalGlyphConfidence.Prepare(ak,owner);var terminal=LegacyFreshMainGlobalTerminalInvocation.Prepare(policy);

    var pass=new LegacyFreshMainGlobalWordPass(ak,policy,r=>{byte[] bytes=new byte[r.RequiredBytes];r.PreviousStorage.Span.CopyTo(bytes);return new(bytes,++identity);}).RunOwned();
    int evaluations=owner.AcousticEvaluations;var selected=terminal.Select(pass);
    byte[] residue=Enumerable.Repeat((byte)0xa7,4096*40).ToArray();
    var materialized=LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak,selected,confidence,residue);

    byte[] compact=materialized.CopyCompactStorage();var geometryOwner=LegacyFreshMainGlobalSourceGeometry.Prepare(ak,policy,confidence);
    var callbacks=LegacyFreshMainGlobalSourceSymbolCallbacks.Prepare(ak,geometryOwner);
    var walk=LegacyFreshMainGlobalGlyphBacktrackRows.Prepare(ak,materialized,callbacks,new byte[72]);

    var plan=LegacyFreshMainGlobalGlyphFlushPlan.Prepare(walk);
    var ownership=LegacyFreshMainGlobalPlainResultOwnership.Create(materialized,geometryOwner);
    var execution=LegacyFreshMainGlobalGlyphExecutionSession.Prepare(ak,ownership,plan,new byte[120]);
    var tokens=new List<LegacyFreshMainGlobalGlyphExecutionResult>();
    while(!execution.IsComplete){var result=execution.ExecuteNext(new byte[40],new ushort[256],new byte[64]);if(!result.Succeeded)throw new Exception("Actual callback failed.");tokens.Add(result);}
    var snapshot=LegacyFreshMainGlobalFirstAlternativeSnapshot.Capture(selected,execution,tokens,new byte[400]);
    var import=LegacyFreshMainGlobalControlImport.Prepare(snapshot);Reject(()=>LegacyFreshMainImportedWordBytes.Project(import));Reject(()=>LegacyFreshMainImportedWordEmission.Complete(import,normalization,payloads));import.Run();
    var before=import.ImportedPool.CopyNodes();var projected=LegacyFreshMainImportedWordBytes.Project(import);var completed=LegacyFreshMainImportedWordEmission.Complete(import,normalization,payloads);
    if(projected.Count!=import.Words.Count)throw new Exception("Actual WORD vector was filtered.");
    w.WriteStartObject();w.WriteNumber("owner",ownerIndex);w.WriteNumber("nn",evaluations);WriteNodes("originalNodes",ownership.GlyphPool.CopyOwnerNodes());WritePool(import.ImportedPool);w.WriteStartArray("words");
    foreach(var word in projected)
    {
     if(!ReferenceEquals(word.Owner,import))throw new Exception("Imported WORD owner identity lost.");
     w.WriteStartObject();w.WriteNumber("index",word.WordIndex);var finished=completed.Single(v=>v.Raw.WordIndex==word.WordIndex);if(!ReferenceEquals(finished.Owner,import)||!ReferenceEquals(finished.Normalization,normalization)||!ReferenceEquals(finished.Payloads,payloads))throw new Exception("Completed WORD owner identity changed.");w.WriteBase64String("completed",finished.CopyBytes());w.WriteBoolean("changed",finished.AnyGlyphChanged);w.WriteBase64String("bytes",word.CopyBytes());w.WriteBoolean("valid",word.IsValidUtf8);w.WriteBoolean("unit",word.ContainsNativeUnitLiteral);
     w.WriteStartArray("glyphs");foreach(var glyph in word.Glyphs){w.WriteStartObject();WriteGlyph(glyph);var separate=LegacyImportedControlGlyphBytes.Read(import,glyph.Reference);if(!separate.CopyBytes().AsSpan().SequenceEqual(glyph.CopyBytes()))throw new Exception("Scalar/WORD bytes diverged.");var separateComplete=LegacyImportedGlyphUtf8Completion.Complete(import,glyph.Reference,normalization,payloads);var bound=finished.Glyphs.First(g=>g.Raw.Reference==glyph.Reference);if(separateComplete.Changed!=bound.Changed||!separateComplete.CopyBytes().AsSpan().SequenceEqual(bound.CopyBytes()))throw new Exception("Public scalar/WORD helper results diverged.");w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
    }
    w.WriteEndArray();w.WriteEndObject();
    if(owner.AcousticEvaluations!=evaluations)throw new Exception("Projection reran NN.");
    foreach(var node in before)if(!import.ImportedPool.CopyNodes().Single(n=>n.Key==node.Key).Records.Span.SequenceEqual(node.Records.Span))throw new Exception("Projection mutated imported pool.");
    Reject(()=>LegacyFreshMainImportedWordBytes.Project(import,new(true)));Reject(()=>LegacyFreshMainImportedWordEmission.Complete(import,normalization,payloads,new(true)));Reject(()=>LegacyImportedControlGlyphBytes.Read(import,ulong.MaxValue));Reject(()=>LegacyImportedGlyphUtf8Completion.Complete(import,ulong.MaxValue,normalization,payloads));
    if(projected.Count>0){byte[] copy=projected[0].CopyBytes();if(copy.Length>0){copy[0]^=127;if(projected[0].CopyBytes().AsSpan().SequenceEqual(copy))throw new Exception("WORD output storage leaked.");}}
    var node0=ownership.GlyphPool.CopyOwnerNodes().First();if(node0.Records.Length>0){byte[] r=ownership.GlyphPool.CopyRecord(node0.Key);r[12]^=1;ownership.GlyphPool.ReplaceRecord(node0.Key,r);Reject(()=>LegacyFreshMainImportedWordBytes.Project(import));Reject(()=>LegacyFreshMainImportedWordEmission.Complete(import,normalization,payloads));}
    Console.WriteLine($"PASS importedraw owner{ownerIndex}/words{projected.Count}/NN{evaluations} unchanged.");ownerIndex++;
}
w.WriteEndArray();WriteGroupedControls();WriteCompletedBoundaries();w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();Console.WriteLine($"PASS {ownerIndex} actualowners/320 prepared records/{guards}guards; PER-GLYPH DEFAULT HELPER; no accepted text.");
void WritePool(LegacyNativeGlyphControlImport pool){w.WriteBoolean("succeeded",pool.Succeeded);w.WriteStartArray("symbols");for(ushort n=0;n<pool.ControlSymbolCount;n++)w.WriteBase64StringValue(pool.ReadControlSymbolBytes(n));w.WriteEndArray();w.WriteStartArray("nodes");foreach(var n in pool.CopyNodes()){w.WriteStartObject();w.WriteNumber("key",n.Key);w.WriteNumber("packed",n.PackedCount);w.WriteBase64String("records",n.Records.Span);w.WriteEndObject();}w.WriteEndArray();}
void WriteGlyph(LegacyImportedControlGlyphBytes glyph){w.WriteNumber("reference",glyph.Reference);w.WriteBase64String("record",glyph.CopyRecord40());w.WriteBase64String("bytes",glyph.CopyBytes());w.WriteBoolean("valid",glyph.IsValidUtf8);w.WriteBoolean("unit",glyph.ContainsNativeUnitLiteral);w.WriteNumber("tokens",glyph.TokenCount);WriteCompletion(glyph);}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or InvalidOperationException or OperationCanceledException){guards++;return;}throw new Exception("Unsupported imported projection accepted.");}
void WriteNodes(string name,IReadOnlyList<LegacyNativeGlyphRecordNode> nodes){w.WriteStartArray(name);foreach(var n in nodes){w.WriteStartObject();w.WriteNumber("key",n.Key);w.WriteNumber("packed",n.PackedCount);w.WriteBase64String("records",n.Records.Span);w.WriteEndObject();}w.WriteEndArray();}

void WriteCompletion(LegacyImportedControlGlyphBytes glyph)
{
 if(!glyph.IsValidUtf8){Reject(()=>LegacyImportedGlyphUtf8Completion.CompleteCore(glyph,normalization,payloads,default));w.WriteBoolean("refusedCodec",true);return;}
 var result=LegacyImportedGlyphUtf8Completion.CompleteCore(glyph,normalization,payloads,default);
 if(!ReferenceEquals(result.Raw,glyph))throw new Exception("Raw glyph identity lost.");
 w.WriteBase64String("completed",result.CopyBytes());w.WriteBoolean("changed",result.Changed);
 Reject(()=>LegacyImportedGlyphUtf8Completion.CompleteCore(glyph,normalization,payloads,new(true)));
 byte[] bytes=result.CopyBytes();if(bytes.Length>0){bytes[0]^=127;if(result.CopyBytes().AsSpan().SequenceEqual(bytes))throw new Exception("Completed bytes leaked mutable storage.");}
}
void WriteGroupedControls()
{
 const string start="\U000f0000",end="\U000f0001";
 var cases=new List<string[]>{new[]{"e\u0301"},new[]{"\ufeffe\u0301"},new[]{"e\u0301"+start+"A"+end},new[]{"e\u0301",start+"A"+end},new[]{start,"A",end},new[]{start+"A",end},new[]{start+"A"+end+"e\u0301"},new[]{"\ufeffe\u0301"+start+"A"+end},new[]{"\ufeffe\u0301",start+"A"+end},new[]{"\ufffe"+start+"A"+end},new[]{"\ufffe",start+"A"+end},new[]{start+"?"+end},new[]{start+"A\0"+end},new[]{start+"A"+end+start+"B"+end},new[]{start+"e\u0301"+end}};
 foreach(char c in "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz")cases.Add(new[]{start+c+end});
 for(int i=10;i<=50;i++)cases.Add(new[]{start+i.ToString(System.Globalization.CultureInfo.InvariantCulture)+end});
 using var data=JsonDocument.Parse(File.ReadAllBytes(args[10]));foreach(var row in data.RootElement.GetProperty("literalPayloads").EnumerateArray())cases.Add(new[]{start+Encoding.UTF8.GetString(Convert.FromHexString(row[0].GetString()!))+end});
 w.WriteStartArray("grouped");int sample=0;
 foreach(var parts in cases)
 {
  var alphabet=LegacyNativeDynamicControlAlphabet.CreateFresh();byte[] rows=new byte[parts.Length*40];
  for(int i=0;i<parts.Length;i++){BinaryPrimitives.WriteUInt16LittleEndian(rows.AsSpan(i*40+4),2);BinaryPrimitives.WriteUInt16LittleEndian(rows.AsSpan(i*40+8),(ushort)i);}
  var pool=new LegacyNativeGlyphControlImport(new[]{new LegacyNativeGlyphRecordNode(7,(ushort)(parts.Length<<2),rows)},model.AcousticAlphabet,alphabet);
  for(int i=0;i<parts.Length;i++)if(alphabet.InsertUtf8(Encoding.UTF8.GetBytes(parts[i]))!=i)throw new Exception("Prepared parts require their actual insertion IDs.");pool.Execute();
  var glyphs=Enumerable.Range(0,parts.Length).Select(i=>LegacyImportedControlGlyphBytes.ReadCore(pool,7UL|((ulong)i<<40),default)).ToArray();
  var completed=LegacyFreshMainImportedWordEmission.CompleteIntervals(new IReadOnlyList<LegacyImportedControlGlyphBytes>[]{glyphs},normalization,payloads,default)[0];
  w.WriteStartObject();w.WriteNumber("sample",sample++);w.WriteStartArray("parts");foreach(string part in parts)w.WriteBase64StringValue(Encoding.UTF8.GetBytes(part));w.WriteEndArray();w.WriteStartArray("glyphs");foreach(var glyph in glyphs){w.WriteStartObject();WriteGlyph(glyph);w.WriteEndObject();}w.WriteEndArray();w.WriteBase64String("completed",completed.Bytes);w.WriteEndObject();
 }
 w.WriteEndArray();
}

void WriteCompletedBoundaries()
{
 var alphabet=LegacyNativeDynamicControlAlphabet.CreateFresh();byte[] record=new byte[40];BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(4),2);
 var pool=new LegacyNativeGlyphControlImport(new[]{new LegacyNativeGlyphRecordNode(7,4,record)},model.AcousticAlphabet,alphabet);
 byte[] input=Enumerable.Repeat((byte)'x',4096).ToArray();if(alphabet.InsertUtf8(input)!=0)throw new Exception("Prepared byte label changed.");pool.Execute();
 var glyph=LegacyImportedControlGlyphBytes.ReadCore(pool,7,default);
 IReadOnlyList<LegacyImportedControlGlyphBytes>[] words=Enumerable.Range(0,65).Select(_=>(IReadOnlyList<LegacyImportedControlGlyphBytes>)Enumerable.Repeat(glyph,32).ToArray()).ToArray();
 var exact=LegacyFreshMainImportedWordEmission.CompleteIntervals(words.Take(64).ToArray(),normalization,payloads,default);
 if(exact.Sum(w=>w.Bytes.Length)!=8_388_608||exact.Any(w=>w.Bytes.Any(b=>b!=(byte)'x')||w.Glyphs.Any(g=>g.Changed)))throw new Exception("Completed aggregate boundary bytes changed.");
 try{_=LegacyFreshMainImportedWordEmission.CompleteIntervals(words,normalization,payloads,default);throw new Exception("Missing aggregate completed cap.");}
 catch(NotSupportedException e)when(e.Message=="Portable aggregate completed byte budget exceeded."){guards++;}
 //A real count-zero run bounds retained empty completion objects.
 byte[] zeroRecord=new byte[40];BinaryPrimitives.WriteUInt16LittleEndian(zeroRecord.AsSpan(4),1);
 var zeroPool=new LegacyNativeGlyphControlImport(new[]{new LegacyNativeGlyphRecordNode(7,4,zeroRecord)},model.AcousticAlphabet);zeroPool.Execute();
 var zero=LegacyImportedControlGlyphBytes.ReadCore(zeroPool,7,default);
 IReadOnlyList<LegacyImportedControlGlyphBytes>[] refs=Enumerable.Range(0,16).Select(_=>(IReadOnlyList<LegacyImportedControlGlyphBytes>)Enumerable.Repeat(zero,4096).ToArray()).ToArray();
 var accepted=LegacyFreshMainImportedWordEmission.CompleteIntervals(refs,normalization,payloads,default);
 if(accepted.Sum(w=>w.Glyphs.Length)!=65536||accepted.Any(w=>w.Bytes.Length!=0||w.Glyphs.Any(g=>g.Changed)))throw new Exception("Completed empty references changed.");
 try{_=LegacyFreshMainImportedWordEmission.CompleteIntervals(refs.Concat(new IReadOnlyList<LegacyImportedControlGlyphBytes>[]{new[]{zero}}).ToArray(),normalization,payloads,default);throw new Exception("Missing completed reference cap.");}
 catch(NotSupportedException e)when(e.Message=="Portable aggregate completed reference budget exceeded."){guards++;}
 byte[] unitRecord=new byte[40];BinaryPrimitives.WriteUInt16LittleEndian(unitRecord.AsSpan(8),178);var unitPool=new LegacyNativeGlyphControlImport(new[]{new LegacyNativeGlyphRecordNode(7,4,unitRecord)},model.AcousticAlphabet);unitPool.Execute();
 var unit=LegacyImportedControlGlyphBytes.ReadCore(unitPool,7,default);Reject(()=>LegacyFreshMainImportedWordEmission.CompleteIntervals(new IReadOnlyList<LegacyImportedControlGlyphBytes>[]{new[]{unit}},normalization,payloads,default));
 //Canonical-excluded0958 expands only when this glyph's circle causes writeback.
 //Completed output crosses its cap while all raw input bytes remain below8MiB.
 var expandedAlphabet=LegacyNativeDynamicControlAlphabet.CreateFresh();byte[] expandedRecord=new byte[40];BinaryPrimitives.WriteUInt16LittleEndian(expandedRecord.AsSpan(4),2);
 var expandedPool=new LegacyNativeGlyphControlImport(new[]{new LegacyNativeGlyphRecordNode(7,4,expandedRecord)},model.AcousticAlphabet,expandedAlphabet);
 byte[] expandedInput=Encoding.UTF8.GetBytes(string.Concat(Enumerable.Repeat("\u0958",1024))+"\U000f0000A\U000f0001");expandedAlphabet.InsertUtf8(expandedInput);expandedPool.Execute();
 var expandedRaw=LegacyImportedControlGlyphBytes.ReadCore(expandedPool,7,default);
 byte[] expectedExpanded=Encoding.UTF8.GetBytes(string.Concat(Enumerable.Repeat("\u0915\u093c",1024))+"\u24b6");
 IReadOnlyList<LegacyImportedControlGlyphBytes>[] expandedWords=Enumerable.Range(0,43).Select(_=>(IReadOnlyList<LegacyImportedControlGlyphBytes>)Enumerable.Repeat(expandedRaw,32).ToArray()).ToArray();
 var expandedAccepted=LegacyFreshMainImportedWordEmission.CompleteIntervals(expandedWords.Take(42).ToArray(),normalization,payloads,default);
 if(expandedAccepted.SelectMany(v=>v.Glyphs).Any(g=>!g.Changed||!g.CopyBytes().AsSpan().SequenceEqual(expectedExpanded))||expandedInput.Length*43*32>=8_388_608)throw new Exception("Completed-vs-raw byte boundary control lost its discriminator.");
 try{_=LegacyFreshMainImportedWordEmission.CompleteIntervals(expandedWords,normalization,payloads,default);throw new Exception("Expanded completed-byte cap was not enforced.");}
 catch(NotSupportedException e)when(e.Message=="Portable aggregate completed byte budget exceeded."){guards++;}
 using var hash=System.Security.Cryptography.IncrementalHash.CreateHash(System.Security.Cryptography.HashAlgorithmName.SHA256);foreach(var word in exact)hash.AppendData(word.Bytes);
 w.WriteStartObject("completedBoundaries");w.WriteNumber("aggregateBytes",8_388_608);w.WriteNumber("acceptedWords",64);w.WriteNumber("rejectedWords",65);w.WriteString("sha256",Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());w.WriteNumber("acceptedReferences",65536);w.WriteNumber("rejectedReferences",65537);
 w.WriteNumber("expandedRawPerGlyph",expandedInput.Length);w.WriteNumber("expandedCompletedPerGlyph",expectedExpanded.Length);w.WriteNumber("acceptedExpandedWords",42);w.WriteNumber("rejectedExpandedWords",43);foreach(var word in expandedAccepted)hash.AppendData(word.Bytes);w.WriteString("expandedSha256",Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());w.WriteEndObject();
}
