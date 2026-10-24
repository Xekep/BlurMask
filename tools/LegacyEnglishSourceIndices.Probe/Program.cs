using InkRecognition;
using System.Text.Json;
if(args.Length!=2||args[0] is not("--validate-plan" or "--prepared")||File.Exists(args[1]))
    throw new ArgumentException("--validate-plan|--prepared <fresh-json>");
if(args[0]=="--validate-plan")
{
    File.WriteAllBytes(args[1],JsonSerializer.SerializeToUtf8Bytes(new{Scope="MODEL_FREE_SOURCE_INDICES_PLAN_ONLY",Validated=true,RuntimeCalls=0,NN=0,OwnerCalls=0,ModelsLoaded=0,DocumentApplied=false}));
    return;
}
//PROPOSED model-free controls; execution requires separate ROOT review.
//No GraphParent, Global receipt, model/resource load or owning source issuer.
var doc=LegacyOwnedRawInkDocument.Create(Enumerable.Range(0,3).Select(s=>new InkStroke(Enumerable.Range(0,3).Select(p=>new InkPoint(s*10+p,p)).ToArray())).ToArray(),default);
var operations=LegacyOwnedRawInkRangeOperations.Prepare(doc);
LegacyNativeInkPositionRange R(int id)=>new(LegacyNativeInkPosition.Pack((uint)id,0,3,3),LegacyNativeInkPosition.Pack((uint)id,2,5,3));
LegacyNativeInkPositionRange[] parent=[];
var choiceKeys=new[]{new[]{R(0)},new[]{R(2)},new[]{R(0)}};
var unions=new List<object>();
foreach(var key in choiceKeys)
{
    var before=parent.ToArray();var bound=LegacyMainWordHeaderUnionBudget.BeforeUnion(parent.Length,key.Length);
    parent=operations.Union(parent,key);
    unions.Add(new{Before=Pairs(before),Additional=Pairs(key),After=Pairs(parent),bound.Payload,bound.Copies,bound.Visits,bound.Arrays});
}
if(parent.Length!=2||parent[0]!=R(0)||parent[1]!=R(2)||parent.AsSpan().SequenceEqual(choiceKeys[0]))throw new InvalidDataException("Actual neutral C21 all-choice union discriminator failed.");
var duplicate=new LegacyMainDocumentSourceIndexKernel<object>();object oldSource=new(),lastSource=new();
for(int i=0;i<65537;i++)duplicate.Add(i==65536?lastSource:oldSource,[R(0)]);
if(duplicate.Intervals.Nodes.Count!=1||duplicate.Vectors.Nodes.Count!=1||duplicate.Memberships.Count!=65537||duplicate.SourcesAdded!=65537||
 duplicate.Intervals.Root!.MembershipHead!=65537||!ReferenceEquals(duplicate.Vectors.Root!.CurrentSource,lastSource)||!duplicate.Dirty)
    throw new InvalidDataException("Real raw duplicate membership/latest vector owner mismatch.");
var ordered=new LegacyMainDocumentSourceIndexKernel<object>();object a=new(),b=new();ordered.Add(a,[R(0),R(2)]);ordered.Add(b,[R(2),R(0)]);
if(ordered.Intervals.Nodes.Count!=2||ordered.Vectors.Nodes.Count!=2||ordered.Memberships.Count!=4||ordered.SourcesAdded!=2)
    throw new InvalidDataException("Exact ordered-vector/raw membership semantics failed.");
var empty=new LegacyMainDocumentSourceIndexKernel<object>();object e=new();empty.Add(e,[]);
if(empty.Intervals.Nodes.Count!=0||empty.Vectors.Nodes.Count!=1||empty.Memberships.Count!=0||empty.Dirty||!ReferenceEquals(empty.Vectors.Root!.CurrentSource,e))
    throw new InvalidDataException("Empty completed vector semantics failed.");
//Synthetic neutral endpoint arithmetic only; not a raw-document or EN issuer.
var full=Enumerable.Range(0,65536).Select(i=>new LegacyNativeInkPositionRange(new LegacyNativeInkPosition((ulong)i<<16|3UL),new LegacyNativeInkPosition((ulong)i<<16|5UL))).ToArray();
_ = LegacyMainWordHeaderUnionBudget.BeforeUnion(65536,1);
var duplicateFull=operations.Union(full,[full[32768]]);
var mergedFull=operations.Union(full,[new(full[0].First,full[^1].Last)]);
if(duplicateFull.Length!=65536||mergedFull.Length!=1)throw new InvalidDataException("Real neutral full-cap duplicate/merge semantics failed.");
var costs=new List<object>();foreach(var pair in new[]{(0,0),(0,1),(1,1),(65535,1),(65536,0),(4096,4096)})
{var q=LegacyMainWordHeaderUnionBudget.BeforeUnion(pair.Item1,pair.Item2);costs.Add(new{T=pair.Item1,A=pair.Item2,q.Payload,q.Copies,q.Visits,q.Arrays});}
int refusals=0;foreach(var pair in new[]{(-1,0),(0,-1),(65537,0),(0,65537),(int.MaxValue,int.MaxValue)})
{try{_ = LegacyMainWordHeaderUnionBudget.BeforeUnion(pair.Item1,pair.Item2);}catch(NotSupportedException){refusals++;continue;}throw new InvalidDataException("Exact union preflight refusal required.");}
File.WriteAllBytes(args[1],JsonSerializer.SerializeToUtf8Bytes(new{Scope="REAL_NEUTRAL_C21_UNION_AND_SOURCE_INDEX_KERNEL_NO_OWNING",Unions=unions,DuplicateMemberships=duplicate.Memberships.Count,DuplicateIntervals=duplicate.Intervals.Nodes.Count,DuplicateVectors=duplicate.Vectors.Nodes.Count,LatestOwnerMatched=true,OrderedVectors=ordered.Vectors.Nodes.Count,EmptyVectorSourceMatched=true,Costs=costs,Refusals=refusals,CacheZeroRuntimeFieldObserved=false,CacheZeroContract="SOURCE_AND_NATIVE_V78_COMPLETED_ADD_NO_CACHE_FILL_NOT_A_RUNTIME_CACHE_FIELD_TEST",OwnerCalls=0,ModelsLoaded=0,NN=0,DocumentApplied=false}));
static ulong[] Pairs(IEnumerable<LegacyNativeInkPositionRange> r)=>r.SelectMany(x=>new[]{x.First.Value,x.Last.Value}).ToArray();
