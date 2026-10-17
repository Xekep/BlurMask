using InkRecognition;
using System.Buffers.Binary;
using System.Text;
using System.Text.Json;

if(args.Length!=9)throw new ArgumentException("AK engine separation case publication line30 line50 capture LK");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[8]),engine=File.ReadAllBytes(args[1]);
using var ed=JsonDocument.Parse(engine);var a=ed.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);byte[] line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
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
//Prepared byte-owner boundary: valid4096-byte2A token and retainedkind2 record.
//This exercises actual interval composition acrossWORDS, not a cap predicate.
var byteAlphabet=LegacyNativeDynamicControlAlphabet.CreateFresh();byte[] largeRecord=new byte[40];
BinaryPrimitives.WriteUInt16LittleEndian(largeRecord.AsSpan(4),2);
var bytePool=new LegacyNativeGlyphControlImport(new[]{new LegacyNativeGlyphRecordNode(7,4,largeRecord)},model.AcousticAlphabet,byteAlphabet);
byte[] repeated=Enumerable.Repeat((byte)'x',4096).ToArray();if(byteAlphabet.InsertUtf8(repeated)!=0)throw new Exception("Prepared byte owner ID changed.");bytePool.Execute();
ulong[][] wordRefs=Enumerable.Range(0,65).Select(_=>Enumerable.Repeat(7UL,32).ToArray()).ToArray();
var exact=LegacyFreshMainImportedWordBytes.CopyWordIntervals(bytePool,wordRefs.Take(64).ToArray(),default);
if(exact.Length!=64||exact.Sum(v=>v.Bytes.Length)!=8_388_608||exact.Any(v=>v.Bytes.Length!=131_072||v.Bytes.Any(b=>b!=(byte)'x')))
 throw new Exception("Actual aggregate boundary bytes changed.");
try{_=LegacyFreshMainImportedWordBytes.CopyWordIntervals(bytePool,wordRefs,default);throw new Exception("Aggregate WORD byte cap was not enforced.");}
catch(NotSupportedException e)when(e.Message=="Portable aggregate imported byte budget exceeded."){guards++;}
using(var hash=System.Security.Cryptography.IncrementalHash.CreateHash(System.Security.Cryptography.HashAlgorithmName.SHA256))
{
 foreach(var interval in exact)hash.AppendData(interval.Bytes);
 w.WriteStartObject("aggregateBoundary");w.WriteBase64String("symbol",repeated);w.WriteNumber("words",64);w.WriteNumber("referencesPerWord",32);w.WriteNumber("bytes",8_388_608);w.WriteString("sha256",Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());w.WriteNumber("refusedWords",65);w.WriteEndObject();
}
byte[] zeroRecord=new byte[40];BinaryPrimitives.WriteUInt16LittleEndian(zeroRecord.AsSpan(4),1);
var zeroPool=new LegacyNativeGlyphControlImport(new[]{new LegacyNativeGlyphRecordNode(7,4,zeroRecord)},model.AcousticAlphabet);zeroPool.Execute();
ulong[][] zeroRefs=Enumerable.Range(0,16).Select(_=>Enumerable.Repeat(7UL,4096).ToArray()).ToArray();
var zeros=LegacyFreshMainImportedWordBytes.CopyWordIntervals(zeroPool,zeroRefs,default);
if(zeros.Sum(v=>v.Glyphs.Length)!=65536||zeros.Any(v=>v.Bytes.Length!=0||v.Glyphs.Any(g=>g.TokenCount!=0)))throw new Exception("Zero-token reference boundary changed.");
try{_=LegacyFreshMainImportedWordBytes.CopyWordIntervals(zeroPool,zeroRefs.Concat(new[]{new[]{7UL}}).ToArray(),default);throw new Exception("Aggregate zero-token reference cap was not enforced.");}
catch(NotSupportedException e)when(e.Message=="Portable aggregate imported reference budget exceeded."){guards++;}
w.WriteStartObject("referenceBoundary");w.WriteNumber("accepted",65536);w.WriteNumber("refused",65537);w.WriteNumber("bytes",zeros.Sum(v=>v.Bytes.Length));w.WriteNumber("glyphs",zeros.Sum(v=>v.Glyphs.Length));w.WriteEndObject();
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
    var import=LegacyFreshMainGlobalControlImport.Prepare(snapshot);Reject(()=>LegacyFreshMainImportedWordBytes.Project(import));import.Run();
    var before=import.ImportedPool.CopyNodes();var projected=LegacyFreshMainImportedWordBytes.Project(import);
    if(projected.Count!=import.Words.Count)throw new Exception("Actual WORD vector was filtered.");
    w.WriteStartObject();w.WriteNumber("owner",ownerIndex);w.WriteNumber("nn",evaluations);WriteNodes("originalNodes",ownership.GlyphPool.CopyOwnerNodes());WritePool(import.ImportedPool);w.WriteStartArray("words");
    foreach(var word in projected)
    {
     if(!ReferenceEquals(word.Owner,import))throw new Exception("Imported WORD owner identity lost.");
     w.WriteStartObject();w.WriteNumber("index",word.WordIndex);w.WriteBase64String("bytes",word.CopyBytes());w.WriteBoolean("valid",word.IsValidUtf8);w.WriteBoolean("unit",word.ContainsNativeUnitLiteral);
     w.WriteStartArray("glyphs");foreach(var glyph in word.Glyphs){w.WriteStartObject();WriteGlyph(glyph);var separate=LegacyImportedControlGlyphBytes.Read(import,glyph.Reference);if(!separate.CopyBytes().AsSpan().SequenceEqual(glyph.CopyBytes()))throw new Exception("Scalar/WORD bytes diverged.");w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
    }
    w.WriteEndArray();w.WriteEndObject();
    if(owner.AcousticEvaluations!=evaluations)throw new Exception("Projection reran NN.");
    foreach(var node in before)if(!import.ImportedPool.CopyNodes().Single(n=>n.Key==node.Key).Records.Span.SequenceEqual(node.Records.Span))throw new Exception("Projection mutated imported pool.");
    Reject(()=>LegacyFreshMainImportedWordBytes.Project(import,new(true)));Reject(()=>LegacyImportedControlGlyphBytes.Read(import,ulong.MaxValue));
    if(projected.Count>0){byte[] copy=projected[0].CopyBytes();if(copy.Length>0){copy[0]^=127;if(projected[0].CopyBytes().AsSpan().SequenceEqual(copy))throw new Exception("WORD output storage leaked.");}}
    var node0=ownership.GlyphPool.CopyOwnerNodes().First();if(node0.Records.Length>0){byte[] r=ownership.GlyphPool.CopyRecord(node0.Key);r[12]^=1;ownership.GlyphPool.ReplaceRecord(node0.Key,r);Reject(()=>LegacyFreshMainImportedWordBytes.Project(import));}
    Console.WriteLine($"PASS importedraw owner{ownerIndex}/words{projected.Count}/NN{evaluations} unchanged.");ownerIndex++;
}
w.WriteEndArray();w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();Console.WriteLine($"PASS {ownerIndex} actualowners/320 prepared records/{guards}guards; RAW PRE-CANONICAL ONLY.");
void WritePool(LegacyNativeGlyphControlImport pool){w.WriteBoolean("succeeded",pool.Succeeded);w.WriteStartArray("symbols");for(ushort n=0;n<pool.ControlSymbolCount;n++)w.WriteBase64StringValue(pool.ReadControlSymbolBytes(n));w.WriteEndArray();w.WriteStartArray("nodes");foreach(var n in pool.CopyNodes()){w.WriteStartObject();w.WriteNumber("key",n.Key);w.WriteNumber("packed",n.PackedCount);w.WriteBase64String("records",n.Records.Span);w.WriteEndObject();}w.WriteEndArray();}
void WriteGlyph(LegacyImportedControlGlyphBytes glyph){w.WriteNumber("reference",glyph.Reference);w.WriteBase64String("record",glyph.CopyRecord40());w.WriteBase64String("bytes",glyph.CopyBytes());w.WriteBoolean("valid",glyph.IsValidUtf8);w.WriteBoolean("unit",glyph.ContainsNativeUnitLiteral);w.WriteNumber("tokens",glyph.TokenCount);}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or InvalidOperationException or OperationCanceledException){guards++;return;}throw new Exception("Unsupported imported projection accepted.");}
void WriteNodes(string name,IReadOnlyList<LegacyNativeGlyphRecordNode> nodes){w.WriteStartArray(name);foreach(var n in nodes){w.WriteStartObject();w.WriteNumber("key",n.Key);w.WriteNumber("packed",n.PackedCount);w.WriteBase64String("records",n.Records.Span);w.WriteEndObject();}w.WriteEndArray();}
