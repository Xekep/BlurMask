using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if(args.Length!=5)throw new ArgumentException("controls.json AK LK engine.json separation.json required");
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases=0, allocations=0, failures=0;
foreach(var test in controls.RootElement.GetProperty("limits").EnumerateArray())
{
 var prior=test.GetProperty("prior");var knowledge=test.GetProperty("knowledge");
 var config=LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with{HeaderCount2400=prior[0].GetUInt16(),PrimaryAlternativeCount2402=prior[1].GetUInt16(),SecondaryAlternativeCount2404=prior[2].GetUInt16(),Mode22f6=test.GetProperty("mode").GetByte(),PackedSource22e8=test.GetProperty("packed").GetUInt32()};
 var input=new LegacyNativeWordKnowledgeLimits(knowledge[0].GetInt16(),knowledge[1].GetInt16(),knowledge[2].GetInt16());
 var image=Enumerable.Repeat((byte)0xa9,0x20).ToArray();BinaryPrimitives.WriteInt16LittleEndian(image.AsSpan(0x14),input.Header14);BinaryPrimitives.WriteInt16LittleEndian(image.AsSpan(0x16),input.Primary16);BinaryPrimitives.WriteInt16LittleEndian(image.AsSpan(0x18),input.Secondary18);
 Require(LegacyNativeWordKnowledgeLimits.ReadPreparedSnapshot(image)==input,"Prepared signed knowledge fields differ.");
 var prepared=LegacyNativeWordControllerLimitPreparation.Prepare(config,test.GetProperty("arc").GetUInt32(),input,test.GetProperty("initial").GetBoolean());
 var result=prepared.Configuration;var expected=test.GetProperty("expected");
 Require(result.HeaderCount2400==expected[0].GetUInt16()&&result.PrimaryAlternativeCount2402==expected[1].GetUInt16()&&result.SecondaryAlternativeCount2404==expected[2].GetUInt16()&&result.PrimaryArcLimit230a==unchecked((ushort)test.GetProperty("arc").GetUInt32())&&prepared.RequiresAlternativePoolReset==test.GetProperty("reset").GetBoolean()&&result.PackedSource22e8==config.PackedSource22e8,"Initial controller limits/sentinel/mode/default/pool reset/word narrowing differ.");
}
Require(LegacyNativeWordKnowledgeLimits.CreateFresh00c860()==new LegacyNativeWordKnowledgeLimits(-1,-1,-1)&&LegacyNativeWordKnowledgeLimits.CreateChild156630(true)==new LegacyNativeWordKnowledgeLimits(1,1,1)&&LegacyNativeWordKnowledgeLimits.CreateChild156630(false)==new LegacyNativeWordKnowledgeLimits(20,1,1),"Constructor/first-child knowledge limits differ.");
Reject(()=>LegacyNativeWordKnowledgeLimits.ReadPreparedSnapshot(new byte[25]));
foreach(var test in controls.RootElement.GetProperty("cases").EnumerateArray())
{
 var sources=test.GetProperty("sources").EnumerateArray().Select(s=>new LegacyNativeWordPreparedSource(s[0].GetUInt32(),s[1].GetSingle(),s[2].GetSingle())).ToArray();
 var config=LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with{PrimaryArcLimit230a=test.GetProperty("budget").GetUInt16(),ExpansionFactor230c=test.GetProperty("factor").GetSingle()};
 var prepared=LegacyNativeWordHeapPreparation.PrepareFresh(sources,config,test.GetProperty("share").GetSingle());
 Require(prepared.Sources==new LegacyNativeWordSourceSummary(test.GetProperty("packed").GetUInt32(),test.GetProperty("subword").GetInt32(),test.GetProperty("overrideSource").GetInt32()),"Fresh packed header/category source indices differ.");
 Require(prepared.Configuration.PackedSource22e8==prepared.Sources.PackedSource22e8&&prepared.Configuration.OverrideHeap2458==test.GetProperty("overrideHeap").GetUInt32(),"Prepared configuration lost source summary or override bucket.");
 var expected=Convert.FromBase64String(test.GetProperty("image").GetString()!);
 Require(prepared.CopyPreparedDescriptorStorage().Span.SequenceEqual(expected),$"Full56-byte descriptor metadata differs in case{cases}.");
 Require(prepared.CopySourceBuckets().Span.SequenceEqual(test.GetProperty("buckets").EnumerateArray().Select(x=>x.GetByte()).ToArray()),"Source byte buckets differ.");
 Require(prepared.Limits.Count*56==expected.Length,"Descriptor count differs.");
 for(int idx=0;idx<prepared.Limits.Count;idx++)Require(prepared.Limits[idx]==LegacyNativeWordHeapQuotaLimits.ReadDescriptorSnapshot(expected.AsSpan(idx*56,56)),"Typed limits differ from actual descriptor image.");
 if(prepared.Limits.All(l=>Math.Max(l.PrimaryInitialCapacity20,l.SecondaryInitialCapacity2c)<=4096))
 {
  int calls=0;
  LegacyNativeWordHeapGrowthResult? Grow(int idx,LegacyNativeWordHeapGrowthRequest request)
  {
   Require(request.PreviousStorage.Length==0&&request.PreviousStorageTag10==0&&request.RequiredAllocatedSlots==(int)Math.Max(prepared.Limits[idx].PrimaryInitialCapacity20,prepared.Limits[idx].SecondaryInitialCapacity2c),"Initial allocation used guessed capacity or backing bytes.");
   calls++;return new(Bytes(idx,request.RequiredAllocatedSlots),(ulong)(idx+1001));
  }
  var initialized=prepared.Initialize(Grow);Require(initialized.Succeeded&&initialized.InitializedDescriptorCount==prepared.Limits.Count,"Initial allocation unexpectedly failed.");
  Require(calls==prepared.Limits.Count(l=>Math.Max(l.PrimaryInitialCapacity20,l.SecondaryInitialCapacity2c)!=0),"Zero-capacity descriptor made an allocator request.");allocations+=calls;
  for(int idx=0;idx<initialized.Descriptors.Count;idx++)
  {
   var d=initialized.Descriptors[idx];int capacity=(int)Math.Max(prepared.Limits[idx].PrimaryInitialCapacity20,prepared.Limits[idx].SecondaryInitialCapacity2c);
   Require(d.Count==0&&d.Capacity==capacity&&d.AllocatedCapacity==capacity&&d.Limits==prepared.Limits[idx]&&d.StorageTag10==(capacity==0?0ul:(ulong)(idx+1001))&&d.GetBackingStorageSnapshot().Span.SequenceEqual(Bytes(idx,capacity)),"Initialization cleared arbitrary native record bytes or lost storage provenance.");
  }
  int failAt=Enumerable.Range(0,prepared.Limits.Count).Where(idx=>Math.Max(prepared.Limits[idx].PrimaryInitialCapacity20,prepared.Limits[idx].SecondaryInitialCapacity2c)>0).Last();
  var failed=prepared.Initialize((idx,request)=>idx==failAt?null:Grow(idx,request));Require(!failed.Succeeded&&failed.InitializedDescriptorCount==failAt&&failed.Descriptors[failAt].Capacity>0&&failed.Descriptors[failAt].AllocatedCapacity==0&&failed.Descriptors[failAt].StorageTag10==0,"Initial allocation failure rolled back prewritten capacity or continued beyond failure.");
  for(int idx=failAt+1;idx<failed.Descriptors.Count;idx++)Require(failed.Descriptors[idx].Capacity==0&&failed.Descriptors[idx].AllocatedCapacity==0,"Failure initialized a later descriptor.");failures++;
 }
 cases++;
}
var fresh=LegacyNativeWordControllerConfiguration.CreateFresh20baa0();
Reject(()=>LegacyNativeWordHeapPreparation.PrepareFresh([],fresh,float.NaN));Reject(()=>LegacyNativeWordHeapPreparation.PrepareFresh([],fresh,-.1f));Reject(()=>LegacyNativeWordHeapPreparation.PrepareFresh([],fresh,1.1f));
Reject(()=>LegacyNativeWordHeapPreparation.PrepareFresh([],fresh with{ExpansionFactor230c=float.PositiveInfinity},0));Reject(()=>LegacyNativeWordHeapPreparation.PrepareFresh([],fresh with{OverrideHeap2458=0},0));Reject(()=>LegacyNativeWordHeapPreparation.PrepareFresh([new(0,float.NaN,1)],fresh,0));Reject(()=>LegacyNativeWordHeapPreparation.PrepareFresh(new LegacyNativeWordPreparedSource[256],fresh,0));
Reject(()=>LegacyNativeWordHeapPreparation.PrepareFresh([],fresh,0,new CancellationToken(true)));
var guard=LegacyNativeWordHeapPreparation.PrepareFresh([],fresh,0);
foreach(bool failed in new[]{false,true})
{
 using var cancel=new CancellationTokenSource();
 Reject(()=>guard.Initialize((idx,request)=>{cancel.Cancel();return failed?null:new LegacyNativeWordHeapGrowthResult(Bytes(idx,request.RequiredAllocatedSlots),1);},cancel.Token));
}
//255 distinct weighted sources are supported; positive share appends256
// descriptors in one native memset, producing a paired512-descriptor array.
var many=Enumerable.Repeat(new LegacyNativeWordPreparedSource(0x800,.01f,.01f),255).ToArray();var maximal=LegacyNativeWordHeapPreparation.PrepareFresh(many,fresh,.5f);Require(maximal.Limits.Count==512&&maximal.CopySourceBuckets().Span[^1]==255,"Maximal prepared descriptor count or byte bucket differs.");
Console.WriteLine($"PASS 4096 independent controller-limit views /{cases} full56-byte preparations; {allocations} explicit initial allocations/{failures} partial failures, packed headers/last source indices, zero-weight general routing, split rounding,512 descriptors, provenance and cancellation.");

byte[] ak=File.ReadAllBytes(args[1]),lk=File.ReadAllBytes(args[2]);var model=LegacyRussianModelBundle.Load(ak,lk,File.ReadAllBytes(args[3]));
var prefix=LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lk.AsSpan(606964,4565174-606964));uint order=LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lk.AsSpan(2513123,4565174-2513123));var dictionary=LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(prefix,model.Lexicon,order,true);
var realSource=LegacyNativeWordPreparedSource.FromDictionary(dictionary);Require(realSource==new LegacyNativeWordPreparedSource(0xc0a,0,1),"Observed RU source descriptor differs.");
var limits=LegacyNativeWordControllerLimitPreparation.Prepare(fresh with{PackedSource22e8=LegacyNativeWordHeapPreparation.SummarizeFresh([realSource]).PackedSource22e8},3,LegacyNativeWordKnowledgeLimits.CreateFresh00c860(),true);
Require(limits.RequiresAlternativePoolReset&&limits.Configuration.HeaderCount2400==2&&limits.Configuration.PrimaryAlternativeCount2402==2&&limits.Configuration.SecondaryAlternativeCount2404==1,"Fresh knowledge with actual RU order2 did not prepare counts2/2/1.");
var actual=LegacyNativeWordHeapPreparation.PrepareFresh([realSource],limits.Configuration,0);Require(actual.Limits.Count==1&&actual.CopySourceBuckets().Span[0]==0&&actual.Configuration.OverrideHeap2458==uint.MaxValue&&actual.Limits[0].PrimaryInitialCapacity20==6&&actual.Limits[0].SecondaryInitialCapacity2c==4,"Actual RU weights did not produce native general-only capacity6/4.");
var allocation=actual.Initialize((idx,request)=>new(Bytes(idx,request.RequiredAllocatedSlots),5001));var heap=allocation.Descriptors[0].GetHeapSnapshot();
var router=new LegacyNativeWordHeapRouter([heap],actual.CopySourceBuckets().Span,actual.Sources.OverrideSourceIndex22f0,actual.Configuration.OverrideHeap2458);
InkStroke Stroke(params(float X,float Y)[] p)=>new(p.Select(x=>new InkPoint(x.X,x.Y)).ToArray());
var raw=new[]{Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),Stroke((70,10),(70,90),(120,10),(120,90)),Stroke((140,90),(140,10),(180,10),(190,20),(190,40),(180,50),(140,50))};
var source=LegacyOrdinaryCursiveCutSource.Generate(raw,LegacyOrdinarySourceInk.ReadV10dResourceProfile(ak),90,80);
var categories=model.CreateCursiveCategoryMapper(1);var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(source.Cuts,80,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(ak,189379),new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());
var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds),model.CursiveSpanProfile.MaximumAtoms);int node=graph.NodeCount-1;int arc=Enumerable.Range(0,graph.GetNode(node).Count).First(idx=>graph.GetNode(node)[idx].Span==node);var candidate=graph.GetNode(node)[arc];
var evaluator=new LegacyPreparedWordGroupEvaluator(model,source,new(90,80,0,1));var glyph=evaluator.Classify(graph.Groups[candidate.GroupId].Ranges.Span);
var cache=new LegacyOrdinaryAcousticClassCache(LegacyOrdinaryAcousticCacheProfile.ReadV110Node(ak.AsSpan(189734)),model.AcousticAlphabet,ak.AsSpan(99,16));
var scorer=LegacyPreparedAcousticCandidateScorer.ForAlphabet(LegacyAcousticCandidateThresholds.ReadV110Node(ak.AsSpan(189734),out _),model.AcousticAlphabet);
var bridge=new LegacyCursiveFinalCacheGroupScorer(cache,scorer,categories,LegacyCursiveSeparationProfile.ReadV106Node(ak,188157),LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[4])),model.AcousticAlphabet,model.LexicalAlphabet,Enumerable.Repeat((byte)1,model.AcousticAlphabet.LabelCount).ToArray(),LegacyWideSymbolPairs.CreateUnrestricted(model.AcousticAlphabet),relatedFormMode:0);
var scope=new LegacyOrdinaryAcousticCacheContext(true,true,true,false,false,null,false,ReadOnlyMemory<ushort>.Empty);var scored=bridge.Score(glyph,source.BindCacheProbability(scope,glyph.Geometry),80,0,false,false,false);var costs=new List<int>();int accepted=0;
for(ushort symbol=0;symbol<model.AcousticAlphabet.LabelCount;symbol++)
{
 int category=categories.GetSymbolCompositeCategory(symbol);if(category<0)continue;
 var score=scorer.Evaluate(scored.Cache.Costs.Span,symbol,scored.CompositeCategoryCosts.Span,category,0,0,LegacyAcousticFixedLog.ImpossibleCost);if(!score.Accepted)continue;
 costs.Add(score.TotalCost);
 var fields=new LegacyInitialWordRecordFields(0,0,score.TotalCost,score.TotalCost,symbol,(ushort)node,(ushort)arc,(ushort)arc,(byte)category,ushort.MaxValue,(sbyte)candidate.Span,0,0,0,0,0,0,0,0,0,0,node,ushort.MaxValue);
 if(router.TryInsertInitialRecord(0,false,fields,0,false).Accepted)accepted++;
}
Require(costs.Count>3&&heap.Count>3&&accepted>3,"Actual RU cache did not exercise native capacity then quota reduction.");
var retained=new LegacyNativeWordHeapDescriptor(heap.GetSlotStorageSnapshot().Span,heap.Capacity,heap.Count,allocation.Descriptors[0].StorageTag10,actual.Limits[0]);
Require(LegacyNativeWordQuotaController.Apply([retained],actual.Configuration,0,0)&&retained.Count==3,"Actual prepared quota did not retain its exact primary arc budget.");
var best=Enumerable.Range(0,retained.Count).Select(idx=>BinaryPrimitives.ReadInt32LittleEndian(retained.GetBackingStorageSnapshot().Span.Slice(idx*128+0x10,4))).Order();Require(best.SequenceEqual(costs.Order().Take(3)),"Actual prepared general heap retained incorrect signed-cost candidates.");
foreach(int idx in Enumerable.Range(0,retained.Count))Require(retained.GetBackingStorageSnapshot().Span.Slice(idx*128+0x78,8).ToArray().Any(x=>x!=0),"Native allocator's opaque tail was replaced by inferred zero bytes.");
var priorBacking=retained.GetBackingStorageSnapshot().ToArray();ulong priorTag=retained.StorageTag10;
Require(LegacyNativeWordPassReset.Reset([retained],actual.Configuration,1,(_,_)=>throw new InvalidDataException("Reset discarded sufficient owned allocation."))&&retained.Count==0&&retained.Capacity==4&&retained.AllocatedCapacity==6&&retained.StorageTag10==priorTag&&retained.GetBackingStorageSnapshot().Span.SequenceEqual(priorBacking),"Native expanded-pass reset did not clear active NN results while retaining all allocated bytes.");
Require(LegacyNativeWordPassReset.Reset([retained],actual.Configuration,0,(_,_)=>throw new InvalidDataException("Repeated reset unexpectedly allocated."))&&retained.Count==0&&retained.Capacity==6&&retained.GetBackingStorageSnapshot().Span.SequenceEqual(priorBacking),"Primary-pass reset did not restore the prepared source capacity.");
Console.WriteLine($"PASS actual RU descriptorC0A/weights0,1 + fresh knowledge -> prepared counts2/2/1 -> general-only source bucket -> native capacity6/4 allocation -> {costs.Count} real NN/cache candidates -> primary-budget3 global quota -> both212d80 resets with retained opaque tails/allocation. Frontend budget remains explicit; final word scheduling/winner remain separate.");

static byte[] Bytes(int index,int capacity)=>Enumerable.Range(0,capacity*128).Select(n=>(byte)(1+(index*17+n*13)%254)).ToArray();
static void Require(bool condition,string message){if(!condition)throw new InvalidDataException(message);}
static void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException){return;}throw new Exception("Unsupported/malformed preparation accepted.");}
