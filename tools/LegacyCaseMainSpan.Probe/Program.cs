using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=7)throw new ArgumentException("AK LK engine-profile separation-table case-data publication-data capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);var model=LegacyRussianModelBundle.Load(ak,lk,engine);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));using var c=File.OpenRead(args[4]);var caseData=LegacyNativeCaseData.Load(c);using var p=File.OpenRead(args[5]);var publication=LegacyNativeTextPublicationData.Load(p);
var sampled=MainFixture.Create(ak,engine,table);var dictionary=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,sampled,caseData,publication);
var owner=new LegacyOwnedMainWordSpanPreparation(ak,dictionary,sampled,table);var graph=owner.SourceGraph;
if(!ReferenceEquals(owner.CommonModel,model)||!ReferenceEquals(owner.Contexts,dictionary.Contexts)||!ReferenceEquals(owner.CaseDictionaryOwner,dictionary)||!ReferenceEquals(graph,dictionary.Graph))throw new Exception("Common case binding escaped actual source/model/context ownership.");
try{_=owner.DictionaryOwner;throw new Exception("Case branch manufactured parallel DIRECT owner.");}catch(InvalidOperationException){}
var scalar=LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,owner);if(scalar.Owner!=owner||!owner.CopySourceHeaders().SequenceEqual(new uint[]{0,0xc0a}))throw new Exception("Common scalar/header branch mismatch.");
if(!owner.IsCaseAware)throw new Exception("Case span lost its scope marker.");
try{LegacyOwnedFreshMainWordRingPreparation.Prepare(scalar);throw new Exception("Unproved case ring binding was silently accepted.");}catch(NotSupportedException){}
LegacyPreparedDictionaryQuery Q()=>new(){Node=0,Capacity=512,ContextRoot=owner.Contexts.ContainerIdentity};
var raw=dictionary.Symbols.Symbols.CreateQuerySource(model.Lexicon,dictionary.Descriptor.Flags).Read(Q(),false);
uint terminal=0;for(int i=0;i<raw.ReturnedCount;i++){uint node=BinaryPrimitives.ReadUInt32LittleEndian(raw.Records.Span[(i*32)..]);if(model.WordIndex.TryGetWordId((int)node,out _)){terminal=node;break;}}
using var file=File.Create(args[6]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteString("raw",Convert.ToHexString(raw.Records.Span));w.WriteStartArray("spans");
int spans=0,computed=0,reused=0,records=0,changedMaps=0;LegacyOwnedMainWordSpan? previous=null;
for(int node=0;node<graph.NodeCount;node++)for(byte span=0;span<owner.GetSpanExtent(node);span++)
{
 //Independent prepared-source snapshots explicitly request the native V80
 //epoch. This is NOT an implementation of2144F0's within-node span scheduler.
 owner.AdvanceNodeEpoch(node-1);
 var actual=owner.Prepare(node,span);var pass=actual.PermissionPass;
 if(pass.PermissionResult.WorkingMap is null)throw new Exception("Installed case map absent.");
 byte[] eligibility=actual.Qualifier.GetEligibilitySnapshot().ToArray();byte[] expected=new byte[195];
 foreach(var row in pass.SelectedRows)for(int symbol=0;symbol<195;symbol++)if(row.Costs.Span[symbol]<0x4000000)expected[symbol]=1;
 if(!eligibility.AsSpan().SequenceEqual(expected))throw new Exception("Finite-cost union changed during case permission propagation.");
 var actualMap=pass.PermissionResult.WorkingMap.CopySelectedMap(false,0);var preCaseMap=dictionary.Symbols.Symbols.CreateWorkingMap(dictionary.Descriptor.Flags).Refresh(pass.PermissionResult.Permissions.Span).CopySelectedMap(false,0);
 if(!preCaseMap.AsSpan().SequenceEqual(actualMap))changedMaps++;
 var page=owner.Read(actual,8UL<<40,Q());
 if(page.ExtraCosts.Length!=0||Enumerable.Range(0,page.ReturnedCount).Any(i=>page.Records.Span[i*32+4]!=1))throw new Exception("Common case query used foreign source/capture state.");
 if(!model.AcousticAlphabet.TryGetLabel(".",out ushort period))throw new Exception("Fixture punctuation unavailable.");
 var metadata=owner.ReadWordTerminal(actual,terminal|(1UL<<32),period,7);
 if(BinaryPrimitives.ReadUInt32LittleEndian(metadata.Span)!=1||BinaryPrimitives.ReadInt32LittleEndian(metadata.Span[8..])!=8)throw new Exception("Actual234970 punctuation category/source stamp lost.");
 if(owner.ResolveWordRestart(actual,terminal|(1UL<<32)|(4UL<<40)|(3UL<<58))!=(1UL|(1UL<<32)|(12UL<<40)|(3UL<<58)))throw new Exception("Actual case restart bits lost in common span.");
 w.WriteStartObject();w.WriteNumber("node",node);w.WriteNumber("span",span);w.WriteNumber("preparedEpoch",node-1);w.WriteBase64String("permissions",pass.PermissionResult.Permissions.Span);w.WriteBase64String("eligibility",eligibility);w.WriteString("records",Convert.ToHexString(page.Records.Span));
 w.WriteStartArray("map");foreach(ushort value in actualMap)w.WriteNumberValue(value);w.WriteEndArray();
 w.WriteStartArray("costs");foreach(var row in pass.SelectedRows){w.WriteStartArray();foreach(int value in row.Costs.Span)w.WriteNumberValue(value);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
 spans++;computed+=pass.ComputedRows;reused+=pass.ReusedRows;records+=page.ReturnedCount;previous=actual;
}
w.WriteEndArray();w.WriteStartArray("contexts");foreach(var node in owner.Contexts.CopyNodes()){w.WriteStartArray();w.WriteNumberValue(node.Identity);w.WriteNumberValue(node.ParentLink);w.WriteNumberValue(node.Symbol);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();w.Flush();
if(changedMaps==0)throw new Exception("Case-expanded installed map never differed from the pre-wrapper map.");
owner.AdvanceNodeEpoch(-1);var next=owner.Prepare(0,0);try{owner.Read(previous!,8UL<<40,Q());throw new Exception("Stale span accepted after map change.");}catch(InvalidOperationException){}
var fresh=new LegacyOwnedMainWordSpanPreparation(ak,dictionary,sampled,table);var foreign=fresh.Prepare(0,0);
try{fresh.GetCosts(next,0,0);throw new Exception("Foreign span accepted.");}catch(InvalidOperationException){}
//Public session updates and another spanowner must invalidate an earlier
//map lease even if its local epoch number has not changed.
for(int operation=0;operation<3;operation++)
{
 var aOwner=new LegacyOwnedMainWordSpanPreparation(ak,dictionary,sampled,table);var aSpan=aOwner.Prepare(0,0);
 dictionary.Session.RefreshPermissions(new byte[192]);
 try
 {
  if(operation==0)aOwner.Read(aSpan,8UL<<40,Q());
  else if(operation==1)aOwner.ReadWordTerminal(aSpan,terminal|(1UL<<32),0,7);
  else aOwner.ResolveWordRestart(aSpan,terminal|(1UL<<32));
  throw new Exception("External session refresh left stale span usable.");
 }
 catch(InvalidOperationException){}
}
var firstOwner=new LegacyOwnedMainWordSpanPreparation(ak,dictionary,sampled,table);var firstSpan=firstOwner.Prepare(0,0);
var secondOwner=new LegacyOwnedMainWordSpanPreparation(ak,dictionary,sampled,table);var secondSpan=secondOwner.Prepare(0,0);
try{firstOwner.Read(firstSpan,8UL<<40,Q());throw new Exception("Second owner did not invalidate first map lease.");}catch(InvalidOperationException){}
secondOwner.AdvanceNodeEpoch(-1);secondSpan=secondOwner.Prepare(0,0);var before=secondOwner.Read(secondSpan,8UL<<40,Q());
try{dictionary.Session.RefreshPermissions([1]);throw new Exception("Invalid permission extent accepted.");}catch(ArgumentException){}
try{dictionary.Session.RefreshPermissions(new byte[192],new(true));throw new Exception("Canceled refresh accepted.");}catch(OperationCanceledException){}
var after=secondOwner.Read(secondSpan,8UL<<40,Q());if(!before.Records.Span.SequenceEqual(after.Records.Span))throw new Exception("Failed/canceled refresh changed installed map/backend.");
//Actual native cache behavior: source68 refresh is not the query-cache epoch.
//An exhausted persistent request stays exhausted after a newly allowing map.
dictionary.Session.RefreshPermissions(new byte[192]);var persistent=Q();
if(dictionary.Session.Read(8UL<<40,persistent).ReturnedCount!=0)throw new Exception("Empty permission map unexpectedly emitted root rows.");
dictionary.Session.RefreshPermissions(Enumerable.Repeat((byte)7,192).ToArray());
int staleCount=dictionary.Session.Read(8UL<<40,persistent).ReturnedCount;
int newlyAllocatedCount=dictionary.Session.Read(8UL<<40,Q()).ReturnedCount;
if(staleCount!=0)throw new Exception("Actual native query-cache epoch diagnostic changed.");
secondOwner.AdvanceNodeEpoch(-1);var epochSpan=secondOwner.Prepare(0,0);var epochRequest=Q();secondOwner.Read(epochSpan,8UL<<40,epochRequest);
var nextEpochSpan=secondOwner.Prepare(0,0);
try{secondOwner.Read(nextEpochSpan,8UL<<40,epochRequest);throw new Exception("Persistent query crossed a permission refresh without the native epoch producer.");}catch(InvalidOperationException){}
//The failed span owner retains its earlier fail-closed policy, but the shared
//session must remain usable by a new owner with a new actual map and request.
var survivorOwner=new LegacyOwnedMainWordSpanPreparation(ak,dictionary,sampled,table);survivorOwner.AdvanceNodeEpoch(-1);var survivorSpan=survivorOwner.Prepare(0,0);
if(survivorOwner.Read(survivorSpan,8UL<<40,Q()).ReturnedCount<=0)throw new Exception("Unsupported query reuse poisoned the shared session.");
if(survivorOwner.Read(survivorSpan,8UL<<40,persistent).ReturnedCount<=0)throw new Exception("Actual node epoch did not restore a formerly exhausted persistent request.");
var ringDictionary=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,sampled,caseData,publication);
LegacyPreparedDictionaryQuery RingQuery()=>new(){Node=0,Capacity=512,ContextRoot=ringDictionary.Contexts.ContainerIdentity};
ringDictionary.Session.RefreshPermissions(new byte[192]);var ringRequests=Enumerable.Range(0,8).Select(_=>RingQuery()).ToArray();foreach(var request in ringRequests)if(ringDictionary.Session.Read(8UL<<40,request).ReturnedCount!=0)throw new Exception("Empty cache-ring root unexpectedly emitted rows.");
ringDictionary.Session.RefreshPermissions(Enumerable.Repeat((byte)7,192).ToArray());
int recycledFreshCount=ringDictionary.Session.Read(8UL<<40,RingQuery()).ReturnedCount;
if(recycledFreshCount!=0)throw new Exception("Eight-slot exhaustion diagnostic changed.");
ringDictionary.Session.PrepareControllerNode(-1);int restoredPersistent=ringDictionary.Session.Read(8UL<<40,ringRequests[0]).ReturnedCount;
var restoredPage=ringDictionary.Session.Read(8UL<<40,RingQuery());int restoredNew=restoredPage.ReturnedCount;
if(restoredPersistent!=315||restoredNew!=315)throw new Exception("Native V80 inner+case reset did not restore same/new requests after all8 slots exhausted.");
if(restoredPage.ChildPage?.SlotIndex!=0)throw new Exception("231ED0 failed to reset8378 wrapper round-robin cursor.");
//Actual2144F0 order at the prepared source boundary: start−1, then each
//completed node. Retain one request and check every reset reaches the backend.
var nativeEpochCalls=new List<int>{-1};var scheduledRequest=RingQuery();
ringDictionary.Session.PrepareControllerNode(-1);
for(int node=0;node<graph.NodeCount;node++)
{
 if(ringDictionary.Session.Read(8UL<<40,scheduledRequest).ReturnedCount!=315)throw new Exception("Node epoch failed to restore persistent request.");
 ringDictionary.Session.PrepareControllerNode(node);nativeEpochCalls.Add(node);
}
if(!nativeEpochCalls.SequenceEqual(Enumerable.Range(-1,graph.NodeCount+1)))throw new Exception("Native V80 call order changed.");
Console.WriteLine($"PASS commoncase preparedactual{spans}spans/{computed}computed/{reused}reused/{records}queryrecords; {changedMaps}maps changed byexactone234840 propagation,145mask/195finiteunion retained,metadata/restarts/local+crossowner+externalrefresh guards. Cachedempty0/new{recycledFreshCount} after8 exhaustedslots; actualV80 restored same{restoredPersistent}/new{restoredNew}. DIRECT API preserved; unprovedcaseRingrefused; within-node persistent290 scheduling not included.");



