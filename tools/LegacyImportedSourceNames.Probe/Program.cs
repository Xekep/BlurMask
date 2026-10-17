using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if(args.Length!=9)throw new ArgumentException("AK engine separation case publication line30 line50 capture LK");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[8]),engine=File.ReadAllBytes(args[1]);
using var ed=JsonDocument.Parse(engine);var a=ed.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var separation=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));
using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);
var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);byte[] line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
using var file=File.Create(args[7]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();int guards=0;

w.WriteStartArray("prepared");
for(int sample=0;sample<512;sample++)
{
 int count=sample==511?256:sample%17;var names=new byte[]?[count];
 for(int n=0;n<count;n++)names[n]=((sample+n)%5) switch
 {0=>null,1=>[],2=>new byte[]{65,0,66},3=>new byte[]{0xc2,0x80,(byte)(sample%127+1)},_=>Enumerable.Range(0,(sample+n)%33).Select(j=>(byte)(j+1)).ToArray()};
 var calls=new List<int>();long reserved=-1;
 byte[]? Read(int id){calls.Add(id);return names[id==-1?count-1:id-1];}
 var table=LegacyDetachedSourceNameTable.Copy(count,Read,default,n=>{reserved=n;return true;});
 w.WriteStartObject();w.WriteNumber("count",count);w.WriteStartArray("sources");foreach(var name in names)Bytes(name);w.WriteEndArray();
 w.WriteNumber("allocation",table.NativeAllocationByteCount);w.WriteNumber("reserved",reserved);Ints("calls",calls);Ints("offsets",table.CopySlotOffsets());w.WriteBase64String("payload",table.CopyWrittenPayload());
 w.WriteStartArray("reads");for(short id=-2;id<=count;id++)Bytes(table.Read(id));w.WriteEndArray();w.WriteEndObject();
 Reject(()=>table.Read(-3));Reject(()=>table.Read((short)(count+1)));Reject(()=>table.Read(0,new(true)));
 byte[] payload=table.CopyWrittenPayload();if(payload.Length>0){payload[0]^=127;if(table.CopyWrittenPayload().AsSpan().SequenceEqual(payload))throw new Exception("Payload storage leaked.");}
 int[] offsets=table.CopySlotOffsets();offsets[0]=73;if(table.CopySlotOffsets()[0]!=-1)throw new Exception("Slot storage leaked.");
 foreach(var name in names)if(name is{Length:>0})name[0]^=127;
 if(payload.Length>0){payload[0]^=127;if(!table.CopyWrittenPayload().AsSpan().SequenceEqual(payload))throw new Exception("Source byte arrays remained aliased.");}
 calls.Clear();Reject(()=>LegacyDetachedSourceNameTable.Copy(count,Read,default,_=>false));
 if(calls.Count!=count)throw new Exception("Failed allocation performed a second source pass.");
}
w.WriteEndArray();
w.WriteStartArray("cancellations");
for(int after=1;after<=8;after++)
{
 using var cts=new CancellationTokenSource();var calls=new List<int>();
 Reject(()=>LegacyDetachedSourceNameTable.Copy(4,id=>{calls.Add(id);if(calls.Count==after)cts.Cancel();return new byte[]{65};},cts.Token));
 if(calls.Count!=after)throw new Exception("Canceled copy kept traversing sources.");
 IntsValue(calls);
}
w.WriteEndArray();
w.WriteStartArray("copyFailures");
for(int after=1;after<=8;after++)
{
 var calls=new List<int>();
 Reject(()=>LegacyDetachedSourceNameTable.Copy(4,id=>{calls.Add(id);if(calls.Count==after)throw new InvalidOperationException("Prepared source-copy failure.");return new byte[]{65};},default));
 if(calls.Count!=after)throw new Exception("Failed copy kept traversing sources.");
 IntsValue(calls);
}
w.WriteEndArray();
Reject(()=>LegacyDetachedSourceNameTable.Copy(-1,_=>throw new Exception("Invalid count reached source."),default));
Reject(()=>LegacyDetachedSourceNameTable.Copy(257,_=>throw new Exception("Invalid count reached source."),default));
Reject(()=>LegacyDetachedSourceNameTable.Copy(1,_=>null,new(true)));
int changed=0;Reject(()=>LegacyDetachedSourceNameTable.Copy(1,_=>++changed==1?new byte[]{65}:null,default));
byte[] huge=Enumerable.Repeat((byte)65,4*1024*1024).ToArray();
Reject(()=>LegacyDetachedSourceNameTable.Copy(2,_=>huge,default));
byte[] edge=Enumerable.Repeat((byte)65,8*1024*1024-1).ToArray();
var exact=LegacyDetachedSourceNameTable.Copy(1,_=>edge,default);
if(exact.CopyWrittenPayload().Length!=8*1024*1024)throw new Exception("Exact aggregate boundary changed.");
Reject(()=>LegacyDetachedSourceNameTable.Copy(1,_=>new byte[8*1024*1024].Select(_=>(byte)65).ToArray(),default));

InkStroke M(float y)=>new(new InkPoint[]{new(0,y+90),new(0,y+10),new(25,y+60),new(50,y+10),new(50,y+90)});
InkStroke V(float y)=>new(new InkPoint[]{new(0,y+10),new(0,y+90)});
var mir=new InkStroke[]{M(0),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})};
var rows=new[]{V(0),new InkStroke(V(0).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray()),V(140),new InkStroke(V(140).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray())};
int ownerIndex=0;ulong identity=0x1000;LegacyImportedControlWord? previousWord=null;w.WriteStartArray("actual");
foreach(var raw in new[]{new[]{M(0),M(140)},new[]{M(0),M(250)},new[]{V(0),V(140)},new[]{V(0),V(250)},mir,mir.Concat(mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+160)).ToArray()))).ToArray(),rows,Enumerable.Range(0,14).Select(n=>new InkStroke(M(0).Points.Select(p=>new InkPoint(p.X+n*70,p.Y)).ToArray())).ToArray()})
{
 var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,separation,cd,pd).Run();
 var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,separation);var global=LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,separation);
 var graph=LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,global);var source=LegacyFreshMainGlobalSourceRanges.Prepare(graph);var context=LegacyFreshMainGlobalContext.Prepare(ak,source,activations);var geometry=LegacyFreshMainGlobalGlyphGeometry.Prepare(ak,context);
 var owner=LegacyFreshMainGlobalCaseSession.Prepare(ak,lk,model,geometry,cd,pd);var cache=LegacyFreshMainGlobalTransitionCache.Prepare(ak,owner,separation);var transitions=LegacyFreshMainGlobalWordTransitions.Prepare(ak,cache);
 var costs=LegacyFreshMainGlobalGroupCosts.Prepare(ak,owner,separation);var policy=LegacyFreshMainGlobalWordControllerPolicy.Prepare(ak,owner,costs,transitions);
 var confidence=LegacyFreshMainGlobalGlyphConfidence.Prepare(ak,owner);var terminal=LegacyFreshMainGlobalTerminalInvocation.Prepare(policy);
 var pass=new LegacyFreshMainGlobalWordPass(ak,policy,r=>{byte[] bytes=new byte[r.RequiredBytes];r.PreviousStorage.Span.CopyTo(bytes);return new(bytes,++identity);}).RunOwned();
 int evaluations=owner.AcousticEvaluations;var selected=terminal.Select(pass);
 var materialized=LegacyFreshMainGlobalCompactWordMaterialization.Materialize(ak,selected,confidence,Enumerable.Repeat((byte)0xa7,4096*40).ToArray());
 var geometryOwner=LegacyFreshMainGlobalSourceGeometry.Prepare(ak,policy,confidence);var callbacks=LegacyFreshMainGlobalSourceSymbolCallbacks.Prepare(ak,geometryOwner);
 var walk=LegacyFreshMainGlobalGlyphBacktrackRows.Prepare(ak,materialized,callbacks,new byte[72]);var plan=LegacyFreshMainGlobalGlyphFlushPlan.Prepare(walk);
 var ownership=LegacyFreshMainGlobalPlainResultOwnership.Create(materialized,geometryOwner);
 var execution=LegacyFreshMainGlobalGlyphExecutionSession.Prepare(ak,ownership,plan,new byte[120]);var tokens=new List<LegacyFreshMainGlobalGlyphExecutionResult>();
 while(!execution.IsComplete){var result=execution.ExecuteNext(new byte[40],new ushort[256],new byte[64]);if(!result.Succeeded)throw new Exception("Actual callback failed.");tokens.Add(result);}
 var snapshot=LegacyFreshMainGlobalFirstAlternativeSnapshot.Capture(selected,execution,tokens,new byte[400]);
 var import=LegacyFreshMainGlobalControlImport.Prepare(snapshot);Reject(()=>LegacyImportedResultSourceNames.Capture(import));import.Run();
 var names=LegacyImportedResultSourceNames.Capture(import);Reject(()=>LegacyImportedResultSourceNames.Capture(import,new(true)));
 if(!ReferenceEquals(names.Owner,import)||names.NativeSourceCount!=2)throw new Exception("Actual source owner/count changed.");
 if(previousWord is not null)Reject(()=>names.ReadWord(previousWord));
 w.WriteStartObject();w.WriteNumber("owner",ownerIndex);w.WriteNumber("nn",evaluations);w.WriteNumber("count",names.NativeSourceCount);w.WriteNumber("allocation",names.NativeAllocationByteCount);Ints("offsets",names.CopySlotOffsets());w.WriteBase64String("payload",names.CopyWrittenPayload());w.WriteStartArray("words");
 foreach(var word in import.Words)
 {
  w.WriteStartObject();w.WriteBase64String("storage",word.CopyStorage120());w.WritePropertyName("name");Bytes(names.ReadWord(word));w.WriteEndObject();
  Reject(()=>names.ReadWord(word,new(true)));byte[]? copy=names.ReadWord(word);if(copy is{Length:>0}){copy[0]^=127;if(names.ReadWord(word)!.AsSpan().SequenceEqual(copy))throw new Exception("Name storage leaked.");}
 }
 w.WriteEndArray();w.WriteEndObject();if(owner.AcousticEvaluations!=evaluations)throw new Exception("Metadata reran NN.");
 previousWord=import.Words.FirstOrDefault();
 //Detached names survive later mutation, while a NEW capture must reject it.
 var node=ownership.GlyphPool.CopyOwnerNodes().First();byte[] record=ownership.GlyphPool.CopyRecord(node.Key);record[12]^=1;ownership.GlyphPool.ReplaceRecord(node.Key,record);
 Reject(()=>LegacyImportedResultSourceNames.Capture(import));foreach(var word in import.Words)_=names.ReadWord(word);
 Console.WriteLine($"PASS source names actual{ownerIndex}/words{import.Words.Count}/NN{evaluations} unchanged.");ownerIndex++;
}
w.WriteEndArray();w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();Console.WriteLine($"PASS512 prepared tables/8 actual owners/{guards} guards; detached source metadata only.");
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or InvalidOperationException or OperationCanceledException or OutOfMemoryException){guards++;return;}throw new Exception("Invalid metadata input accepted.");}
void Bytes(byte[]? bytes){if(bytes is null)w.WriteNullValue();else w.WriteBase64StringValue(bytes);}
void Ints(string name,IEnumerable<int> values){w.WritePropertyName(name);IntsValue(values);}
void IntsValue(IEnumerable<int> values){w.WriteStartArray();foreach(int value in values)w.WriteNumberValue(value);w.WriteEndArray();}
