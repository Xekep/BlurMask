using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args is not ["--prepared",var models,var output])throw new ArgumentException("Only prepared controls are enabled; actual NN is not authorized.");
var source=LegacySource247WireLayout.Read(File.ReadAllBytes(Path.Combine(models,"en-US/en_US-ak-cur.lite.res")));
var sampler=source.ReadSampler(source.Children.Single(c=>c.Destination=="247.130"));var categories=sampler.ReadCategories();var alphabet=source.Alphabet;
var config=LegacyNativeWordControllerLimitPreparation.Prepare(LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with {PackedSource22e8=3},125,LegacyNativeWordKnowledgeLimits.CreateFresh00c860(),true).Configuration;
var preparation=LegacyNativeWordHeapPreparation.PrepareFresh([new(0,0,1),new(0xc0e,0,1)],config,0);
if(preparation.Limits.Count!=1||preparation.Limits[0].PrimaryInitialCapacity20!=250)throw new InvalidDataException("Prepared actual shape125/250 required.");
var maskControls=ParentMaskControls.Run();
var controls=new List<object>();int refusals=0;
foreach(int cost in new[]{-8192,-1,0,1,8192,123456,0x3ffffff})foreach(int increment in new[]{-100,0,100})
{
 var contexts=new LegacySymbolContextFactory(4096);ulong a=contexts.ResolveInitial(1).Identity,b=contexts.ResolveInitial(2).Identity;
 var parent=new byte[88];W64(parent,0,0x100000001UL);W64(parent,8,a);W32(parent,16,100);W32(parent,20,20);W16(parent,36,1);W16(parent,60,0);W16(parent,62,65535);
 var alt=new byte[80];for(int k=0;k<2;k++){int at=40*k;W32(alt,at,10+k*10);W32(alt,at+4,2+k);W64(alt,at+8,a);W64(alt,at+16,b);alt[at+30]=1;alt[at+31]=32;}
 var parents=new LegacyNativeStoredWordRecord[125];for(int p=0;p<125;p++){var pr=parent.ToArray();if(p!=0){W32(pr,16,0x4000000);W32(pr,20,0x4000000);}parents[p]=new(pr,alt.ToArray(),[]);}
 var heap=new LegacyNativeWordRecordHeap(250);
 var fields=new LegacyInitialWordRecordFields(0x100000002UL,0,cost,increment,2,1,0,0,0,0,0,0,0,0,0,0,0,0,0,-1,0,0,0);
 var prefix=LegacyInitialWordRecordProducer.InitializeAllocatedPrefix(fields,0);
 heap.TryAllocate(cost,slot=>prefix.CopyTo(slot));var before=heap.GetSlotStorageSnapshot().ToArray();
 var result=LegacyEnglishCaseNode1Materialization.Build(heap,preparation,categories,contexts,parents,alphabet,default);
 if(result.Count!=1||result.InitialCount!=1)throw new Exception("One prepared row must survive quota.");
 var q=new LegacyPreparedDictionaryQuery{Capacity=128,ContextRoot=contexts.ContainerIdentity};var tag=LegacyOrdinaryDictionaryQueryPreparation.ApplyContinuation(q,new(0,0,false,false,120),parent,1,0,0,false);
 controls.Add(new{Cost=cost,Increment=increment,Parent=Convert.ToHexString(parent),ParentAlt=Convert.ToHexString(alt),Before=Convert.ToHexString(before),Heap=Convert.ToHexString(result.Heap128),Rows=Convert.ToHexString(result.Rows88),Alts=Convert.ToHexString(result.Alternatives80),Query=new{Tag=tag,Node=q.Node,q.Skip,q.Parameter1c,q.Parameter20,q.Flags24,q.Flags26,q.Parameter19}});
 var retained=result.Rows88.ToArray();result.Rows88[0]^=255;if(retained[0]==result.Rows88[0])throw new Exception("Mutation control failed.");
 using var stop=new CancellationTokenSource();stop.Cancel();try{_=LegacyEnglishCaseNode1Materialization.Build(heap,preparation,categories,contexts,parents,alphabet,stop.Token);throw new Exception("Cancellation accepted.");}catch(OperationCanceledException e){if(e.CancellationToken!=stop.Token)throw;refusals++;}
 try{_=LegacyEnglishCaseNode1Materialization.Build(heap,preparation,categories,contexts,parents[..124],alphabet,default);throw new Exception("Truncated parents accepted.");}catch(NotSupportedException){refusals++;}
}
// Explicit native rejection gates: disabled parent leaves all reused query fields unchanged.
var invalid=new byte[88];invalid[33]=128;var untouched=new LegacyPreparedDictionaryQuery{Node=17,Skip=23};
if(LegacyOrdinaryDictionaryQueryPreparation.ApplyContinuation(untouched,new(0,0,false,false,120),invalid,1,0,0,false)!=null||untouched.Node!=17||untouched.Skip!=23)throw new Exception("Rejected continuation mutated query.");refusals++;
File.WriteAllText(output,JsonSerializer.Serialize(new{Scope="PREPARED numeric joins only; no actual graph/ink/NN/owner authority",Controls=controls,Refusals=refusals,NNCalls=0,ParentMaskControls=maskControls}));
Console.WriteLine($"Prepared controls={controls.Count}, refusals={refusals}, NN=0");
static void W16(byte[] b,int at,ushort v)=>BinaryPrimitives.WriteUInt16LittleEndian(b.AsSpan(at),v);
static void W32(byte[] b,int at,int v)=>BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(at),v);
static void W64(byte[] b,int at,ulong v)=>BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(at),v);
