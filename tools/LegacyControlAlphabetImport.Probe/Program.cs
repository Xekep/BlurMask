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
w.WriteStartArray("traces");
for(int seed=0;seed<256;seed++)
{
 var alphabet=LegacyNativeDynamicControlAlphabet.CreateFresh();w.WriteStartObject();w.WriteNumber("seed",seed);w.WriteStartArray("operations");
 for(int i=0;i<64;i++)
 {
  string text=(i%13) switch{0=>"\n",1=>" ",2=>"\u00a0",3=>"A",4=>"a",5=>"e\u0301",6=>"\u00e9",7=>"\0",8=>"\U0001f642",_=>$"{seed%11}:{i%23}"};
  byte[] bytes=Encoding.UTF8.GetBytes(text);ushort id=alphabet.InsertUtf8(bytes);
  if(alphabet.LookupBytes(bytes)!=id)throw new Exception("Insertion/lookup diverged.");
  w.WriteStartObject();w.WriteBase64String("bytes",bytes);w.WriteNumber("id",id);State(alphabet.ReadState());w.WriteEndObject();
 }
 w.WriteEndArray();Symbols(alphabet);w.WriteEndObject();
}
w.WriteEndArray();
w.WriteStartArray("faults");
foreach(var stage in Enum.GetValues<LegacyControlAlphabetAllocation>())foreach(byte residue in new byte[]{0,1,255})foreach(int occurrence in stage==LegacyControlAlphabetAllocation.SymbolVector?new[]{1,2}:new[]{1})
{
 var allocations=new List<(LegacyControlAlphabetAllocation Stage,int Bytes)>();int hits=0;bool failed=false;
 var alphabet=LegacyNativeDynamicControlAlphabet.CreateWithAllocator((s,n)=>{allocations.Add((s,n));if(s==stage&&++hits==occurrence){failed=true;return false;}return true;},residue);
 w.WriteStartObject();w.WriteString("stage",stage.ToString());w.WriteNumber("occurrence",occurrence);w.WriteNumber("residue",residue);w.WriteStartArray("operations");
 var texts=occurrence==2?Enumerable.Range(0,17).Select(i=>$"s{i}").Concat(new[]{"s16","z","\n"," "}):new[]{"x","x","y","\n"," "};
 foreach(string text in texts){byte[] b=Encoding.UTF8.GetBytes(text);ushort id=alphabet.InsertUtf8(b);w.WriteStartObject();w.WriteBase64String("bytes",b);w.WriteNumber("id",id);State(alphabet.ReadState());w.WriteEndObject();}
 if(!failed)throw new Exception("Prepared allocator failure was not reached.");
 w.WriteEndArray();w.WriteStartArray("allocations");foreach(var allocation in allocations){w.WriteStartArray();w.WriteStringValue(allocation.Stage.ToString());w.WriteNumberValue(allocation.Bytes);w.WriteEndArray();}w.WriteEndArray();Symbols(alphabet);w.WriteEndObject();
}
w.WriteEndArray();
var collisionBuckets=new Dictionary<uint,byte[]>();byte[]? collisionA=null,collisionB=null;uint selectedBucket=0;
for(int i=0;i<100000&&collisionA is null;i++){byte[] bytes=Encoding.ASCII.GetBytes($"collision{i}");uint hash=LegacyNativeDynamicControlAlphabet.ComputeHash(bytes),bucket=(hash&0x7ff)|((hash>>16&15)<<11);if(collisionBuckets.TryGetValue(bucket,out var previous)){collisionA=previous;collisionB=bytes;selectedBucket=bucket;}else collisionBuckets.Add(bucket,bytes);}
if(collisionA is null||collisionB is null)throw new Exception("Independent collision search did not reach a native bucket.");
var collision=LegacyNativeDynamicControlAlphabet.CreateFresh();if(collision.InsertUtf8(collisionA)!=0||collision.InsertUtf8(collisionB)!=1||collision.LookupBytes(collisionA)!=0||collision.LookupBytes(collisionB)!=1)throw new Exception("Hash bucket collision replaced byte identity.");
w.WriteStartObject("collision");w.WriteBase64String("first",collisionA);w.WriteBase64String("second",collisionB);w.WriteNumber("bucket",selectedBucket);State(collision.ReadState());Symbols(collision);w.WriteEndObject();
LegacyNativeDynamicControlAlphabet? inFlight=null;ushort observed=65535;
inFlight=LegacyNativeDynamicControlAlphabet.CreateWithAllocator((stage,bytes)=>{if(stage==LegacyControlAlphabetAllocation.SymbolVector){observed=inFlight!.LookupBytes("x"u8);return false;}return true;},0);
if(inFlight.InsertUtf8("x"u8)!=65535||observed!=0||inFlight.LookupBytes("x"u8)!=65535)throw new Exception("Native leaf zero prefix/storeFFFF ordering changed.");
w.WriteStartObject("inFlightLeaf");w.WriteNumber("observedBeforeVectorResult",observed);State(inFlight.ReadState());w.WriteEndObject();
var capacity=LegacyNativeDynamicControlAlphabet.CreateFresh();for(int i=0;i<65520;i++)if(capacity.InsertUtf8(Encoding.ASCII.GetBytes($"c{i}"))!=i)throw new Exception("Native U16 IDs changed.");
Reject(()=>capacity.InsertUtf8("overflow"u8));if(capacity.Count!=65520||capacity.Capacity!=65520)throw new Exception("Portable capacity guard mutated the owner.");
Reject(()=>capacity.InsertUtf8(new byte[]{0xc2}));Reject(()=>capacity.InsertUtf8("x"u8,new(true)));Reject(()=>capacity.ReadSymbolBytes(65535));
var bounded=LegacyNativeDynamicControlAlphabet.CreateFresh();Reject(()=>bounded.InsertUtf8(new byte[1_048_577]));
byte[] copied=capacity.ReadSymbolBytes(0);copied[0]^=127;if(!capacity.ReadSymbolBytes(0).AsSpan().SequenceEqual("c0"u8))throw new Exception("Control bytes leaked mutable storage.");
w.WriteStartObject("capacity");State(capacity.ReadState());w.WriteEndObject();
w.WriteStartArray("preparedImports");
for(int seed=0;seed<512;seed++)
{
 var nodes=new List<LegacyNativeGlyphRecordNode>();
 for(int n=2;n>=0;n--){int count=1+(seed+n)%5;byte[] records=new byte[count*40];for(int j=0;j<records.Length;j++)records[j]=(byte)(j*13+seed*17+n);for(int r=0;r<count;r++){var row=records.AsSpan(r*40,40);W32(row,0,seed*71+r);ushort kind=(ushort)((seed+n+r)%8),symbol=(ushort)((seed*37+n*71+r*11)%195);if(kind==1)symbol=(ushort)((((seed+r)%4)<<2)|((seed+n)%4));if(seed%7==0&&kind==0)symbol=ushort.MaxValue;if(seed%17==0&&r==0){kind=0;symbol=178;}W16(row,4,(ushort)((U16(row,4)&0xfff8)|kind));W16(row,8,symbol);}nodes.Add(new((ulong)(uint)(n+3)|((ulong)(uint)(n*7)<<24),(ushort)(count<<2),records));}
 var import=new LegacyNativeGlyphControlImport(nodes,model.AcousticAlphabet);import.Execute();WriteImport(seed,nodes,import);
 Reject(()=>import.Execute());
}
//Every raw model label is a separate source-derived input, not a selected winner.
for(int symbol=0;symbol<model.AcousticAlphabet.LabelCount;symbol++){byte[] row=new byte[40];W16(row,8,(ushort)symbol);var nodes=new[]{new LegacyNativeGlyphRecordNode(7,4,row)};var import=new LegacyNativeGlyphControlImport(nodes,model.AcousticAlphabet);import.Execute();WriteImport(512+symbol,nodes,import);}
//Actual native C2 unit and macro/partial-prefix/failure controls.
foreach(int failure in Enumerable.Range(0,7))
{
 var nodes=new[]{new LegacyNativeGlyphRecordNode(1,12,new byte[120])};byte[] records=nodes[0].Records.ToArray();W16(records,8,22);W16(records.AsSpan(40),4,1);W16(records.AsSpan(40),8,(ushort)(failure==0?5:failure==1?0:failure==2?4:failure==3?6:failure==4?7:0));W16(records.AsSpan(80),8,178);nodes[0]=new(1,12,records);
 int hits=0;var alphabet=failure==5?LegacyNativeDynamicControlAlphabet.CreateWithAllocator((s,n)=>s!=LegacyControlAlphabetAllocation.SymbolVector||++hits!=1,0):LegacyNativeDynamicControlAlphabet.CreateFresh();
 var import=new LegacyNativeGlyphControlImport(nodes,model.AcousticAlphabet,alphabet);import.Execute();WriteImport(704+failure,nodes,import,failure==5?"SymbolVector":null);
}
w.WriteEndArray();
w.WriteStartArray("canceledImports");
for(int i=0;i<4;i++)
{
 using var cancel=new CancellationTokenSource();int allocation=0;
 var alphabet=LegacyNativeDynamicControlAlphabet.CreateWithAllocator((stage,bytes)=>{if(stage==LegacyControlAlphabetAllocation.SymbolVector&&++allocation==1)cancel.Cancel();return true;},0);
 byte[] record=new byte[40];W16(record,8,(ushort)(22+i));var inputNodes=new[]{new LegacyNativeGlyphRecordNode(4,4,record)};
 var imported=new LegacyNativeGlyphControlImport(inputNodes,model.AcousticAlphabet,alphabet);
 Reject(()=>imported.Execute(cancel.Token));if(!imported.IsStopped||imported.HasFinished||imported.Succeeded||imported.ProcessedRecordCount!=0||imported.ControlSymbolCount!=1)throw new Exception("Canceled import lost its control-only prefix.");
 if(!imported.CopyNodes()[0].Records.Span.SequenceEqual(record))throw new Exception("Canceled import wrote the current glyph after interning.");
 w.WriteStartObject();w.WriteNumber("seed",i);WriteNodes("before",inputNodes);WriteNodes("after",imported.CopyNodes());WriteConverted(imported);w.WriteEndObject();
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
    var import=LegacyFreshMainGlobalControlImport.Prepare(snapshot);var originals=ownership.GlyphPool.CopyOwnerNodes();
    var converted=import.Run();if(!import.Succeeded||!converted.Succeeded||!import.OwnsCopiedAlternateStorage||import.AlternateByteCapacity!=1)throw new Exception("Actual no-aux import failed.");
    foreach(var original in originals){var actual=ownership.GlyphPool.CopyOwnerNode(original.Key);if(!actual.Records.Span.SequenceEqual(original.Records.Span)||actual.PackedCount!=original.PackedCount)throw new Exception("Original pool changed during copied import.");}
    if(owner.AcousticEvaluations!=evaluations)throw new Exception("Import reran NN.");
    w.WriteStartObject();w.WriteNumber("owner",ownerIndex);w.WriteNumber("nn",evaluations);WriteNodes("before",originals);WriteNodes("after",converted.CopyNodes());WriteConverted(converted);w.WriteStartArray("words");
    foreach(var word in import.Words){w.WriteStartObject();w.WriteBase64String("storage120",word.CopyStorage120());w.WriteStartArray("references");foreach(ulong reference in word.CopyReferences())w.WriteNumberValue(reference);w.WriteEndArray();w.WriteEndObject();}
    w.WriteEndArray();w.WriteEndObject();Reject(()=>import.Run());
    var canceled=LegacyFreshMainGlobalControlImport.Prepare(snapshot);Reject(()=>canceled.Run(new(true)));if(canceled.OwnsCopiedAlternateStorage)throw new Exception("Pre-entry cancellation allocated alternate storage.");canceled.Run();
    if(converted.ControlSymbolCount>0){byte[] text=converted.ReadControlSymbolBytes(0),saved=text.ToArray();if(text.Length>0)text[0]^=127;if(!converted.ReadControlSymbolBytes(0).AsSpan().SequenceEqual(saved))throw new Exception("Imported control bytes are mutable through output.");}
    var copy=converted.CopyNodes();if(copy.Count>0&&copy[0].Records.Length>0){var mutated=copy[0].Records.ToArray();mutated[4]^=127;if(converted.CopyNodes()[0].Records.Span.SequenceEqual(mutated))throw new Exception("Imported pool leaked mutable storage.");}
    var stale=LegacyFreshMainGlobalControlImport.Prepare(snapshot);if(originals.Count>0&&originals[0].Records.Length>0){ulong reference=originals[0].Key;byte[] record=ownership.GlyphPool.CopyRecord(reference);record[12]^=1;ownership.GlyphPool.ReplaceRecord(reference,record);Reject(()=>stale.Run());if(stale.OwnsCopiedAlternateStorage)throw new Exception("Changed source passed the import prewrite boundary.");}
    Console.WriteLine($"PASS actualcontrolimport owner{ownerIndex}/allpool{converted.ProcessedRecordCount}/symbols{converted.ControlSymbolCount}/NN{evaluations} unchanged.");ownerIndex++;
}
w.WriteEndArray();
for(int offsetIndex=0;offsetIndex<2;offsetIndex++)foreach(int count in new[]{1,-1,int.MaxValue}){byte[] word=new byte[120];W32(word,offsetIndex==0?96:112,count);Reject(()=>LegacyFreshMainGlobalControlImport.ValidateAuxiliaryCounts(word));}
Reject(()=>LegacyFreshMainGlobalControlImport.ValidateAuxiliaryCounts(new byte[119]));
w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();Console.WriteLine($"PASS {ownerIndex} actualglobalimports/256traces/18faulttraces/711preparedimports/{guards}guards; no postimportV50/acceptedtext claim.");
void WriteImport(int seed,IReadOnlyList<LegacyNativeGlyphRecordNode> before,LegacyNativeGlyphControlImport import,string? faultStage=null){w.WriteStartObject();w.WriteNumber("seed",seed);if(faultStage is not null)w.WriteString("faultStage",faultStage);WriteNodes("before",before);WriteNodes("after",import.CopyNodes());WriteConverted(import);w.WriteEndObject();}
void WriteConverted(LegacyNativeGlyphControlImport import){w.WriteBoolean("succeeded",import.Succeeded);w.WriteBoolean("stopped",import.IsStopped);w.WriteNumber("processed",import.ProcessedRecordCount);w.WriteNumber("changed",import.ChangedRecordCount);if(import.FailureReference is ulong reference)w.WriteNumber("failureReference",reference);else w.WriteNull("failureReference");State(import.ControlState);w.WriteStartArray("symbols");for(ushort i=0;i<import.ControlSymbolCount;i++)w.WriteBase64StringValue(import.ReadControlSymbolBytes(i));w.WriteEndArray();}
void WriteNodes(string name,IReadOnlyList<LegacyNativeGlyphRecordNode> nodes){w.WriteStartArray(name);foreach(var node in nodes){w.WriteStartObject();w.WriteNumber("key",node.Key);w.WriteNumber("packed",node.PackedCount);w.WriteBase64String("records",node.Records.Span);w.WriteEndObject();}w.WriteEndArray();}
void Symbols(LegacyNativeDynamicControlAlphabet alphabet){w.WriteStartArray("symbols");for(ushort i=0;i<alphabet.Count;i++)w.WriteBase64StringValue(alphabet.ReadSymbolBytes(i));w.WriteEndArray();}
void State(LegacyControlAlphabetState state){w.WriteStartArray("state");foreach(int value in new[]{state.Count,state.Capacity,state.LineFeed,state.Space,state.NonbreakingSpace,state.HashRoot?1:0,state.HashMiddleCount,state.HashBucketCount,state.LeafCount,state.ByteCount,state.CacheInvalidations})w.WriteNumberValue(value);w.WriteEndArray();}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or InvalidOperationException or OperationCanceledException){guards++;return;}throw new Exception("Invalid control import input accepted.");}
static ushort U16(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(b[at..]);
static void W16(Span<byte> b,int at,ushort n)=>BinaryPrimitives.WriteUInt16LittleEndian(b[at..],n);
static void W32(Span<byte> b,int at,int n)=>BinaryPrimitives.WriteInt32LittleEndian(b[at..],n);
