using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if(args.Length!=2)throw new ArgumentException("MODELS CAPTURE");
var root=args[0];byte[] Read(string path)=>File.ReadAllBytes(Path.Combine(root,path));
byte[] ak=Read("ru-RU/ru_RU-ak-cur.lite.res"),lk=Read("ru-RU/ru_RU-lk-text.lite.res");
using var engine=JsonDocument.Parse(Read("ru-RU/legacy-engine-profile.json"));var ac=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(ac.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),ac.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(Read("ru-RU/legacy-separation-profile.json"));
using var cf=File.OpenRead(Path.Combine(root,"text-unicode/legacy-case-data.json"));using var pf=File.OpenRead(Path.Combine(root,"text-unicode/legacy-text-publication-data.json"));
var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);var line30=Read("line-geometry/line-2e3-30.res");var line50=Read("line-geometry/line-2e3-50.res");
var linguistic=LegacyLinguisticRoot13FWireLayout.Read(lk);var wire=LegacyFst23CWireLayout.ReadActualRussianNumber(linguistic);
var outgoing=LegacyFst13AOutgoingSource.CreateActualRussianNumber(linguistic,wire,ak);
var primary=LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(outgoing,ak);
var policy=LegacyFst28CTargetPreparation.CreateActualRussianNumberFresh(primary,ak);var initial=LegacyFst28CInitialState.CreateActualRussianNumberFresh(policy);
LegacyFreshMainCompactOrdinaryGraph Graph(InkStroke[] strokes)
{
 var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,strokes,0,activations,table,cd,pd).Run();
 var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table);
 return LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table));
}
var patterns=new InkStroke[][]{
 [new(new InkPoint[]{new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)})],
 [new(new InkPoint[]{new(0,10),new(30,90)}),new(new InkPoint[]{new(40,90),new(70,10)})],
 [new(new InkPoint[]{new(0,10),new(40,90),new(80,10)}),new(new InkPoint[]{new(0,90),new(40,10),new(80,90)})],
 [new(new InkPoint[]{new(0,90),new(0,10)}),new(new InkPoint[]{new(30,90),new(30,10)}),new(new InkPoint[]{new(0,230),new(0,150)})]};
var graphs=patterns.Select(Graph).ToArray();
var boots=graphs.Select(g=>LegacyFst28CFreshPrimaryControllerBootstrap.CreateDirectSelectedNumberFresh(initial,g,ak)).ToArray();
LegacyFst28CNumberSymbolMasks NewMasks(int i=0)=>LegacyFst28CNumberSymbolMasks.CreateActualDirectNumberFresh(boots[i],ak);
var masks=Enumerable.Range(0,4).Select(i=>NewMasks(i)).ToArray();
var owners=masks.Select(v=>LegacyFst28CNumberInitialD0.CreateActualDirectNumberFresh(v,ak)).ToArray();
var bindings=owners.Select(o=>LegacyFst28CNumberInitialQueryBinding.CreateFreshInitialPrefix(o)).ToArray();
var guards=new List<string>();
void Require(bool good,string name){if(!good)throw new Exception(name);guards.Add(name);}
void Refuse<T>(string name,Action action)where T:Exception
{try{action();}catch(T){guards.Add(name);return;}throw new Exception("Missing refusal "+name);}
LegacyFst28CNumberInitialPage Bounded(LegacyFst28CNumberInitialQueryBinding b,int bytes=42288,int work=165168,CancellationToken token=default,Action<string>? checkpoint=null)
 => ProbeAccess.CreateBounded(null!,b,bytes,work,token,checkpoint);
Refuse<ArgumentNullException>("null-binding",()=>LegacyFst28CNumberInitialPage.CreateFirstNormalPage(null!));
Refuse<OperationCanceledException>("cancel-before-null",()=>LegacyFst28CNumberInitialPage.CreateFirstNormalPage(null!,new(true)));
foreach(int n in new[]{-1,42289,int.MaxValue})Refuse<ArgumentOutOfRangeException>("byte-domain-"+n,()=>Bounded(bindings[0],n));
foreach(int n in new[]{-1,165169,int.MaxValue})Refuse<ArgumentOutOfRangeException>("work-domain-"+n,()=>Bounded(bindings[0],42288,n));
Refuse<InvalidDataException>("one-byte-short",()=>Bounded(bindings[0],42287));
Refuse<InvalidDataException>("one-work-short",()=>Bounded(bindings[0],42288,165167));
using(var cs=new CancellationTokenSource())
{
 bool reached=false;Refuse<OperationCanceledException>("cancel-before-claim",()=>Bounded(bindings[0],42288,165168,cs.Token,s=>{if(s=="before-claim"){reached=true;cs.Cancel();}}));Require(reached,"before-claim-reached");
}
Require(boots.Select((b,i)=>ProbeAccess.Header(b,owners[i],default)).Distinct(ReferenceEqualityComparer.Instance).Count()==4,"four-pairwise-private-header-identities");
Refuse<ArgumentException>("foreign-bootstrap-header",()=>ProbeAccess.Header(boots[0],owners[1],default));
var pages=new LegacyFst28CNumberInitialPage[4];
for(int c=0;c<4;c++)
{
 var b=bindings[c];var before=b.ReadLiveNodes().ToArray();var page=pages[c]=Bounded(b);
 Require(ReferenceEquals(page.Binding,b)&&ReferenceEquals(page.Binding.D0,owners[c])&&ReferenceEquals(page.Binding.Graph,graphs[c])&&b.RetainsExactHeaderIdentity(),"same-actual-owner-header-"+c);
 Require(!page.HasGeneralQueryAuthority&&!page.HasPagingAuthority&&page.LogicalByteReservation==42288&&page.WorkReservation==165168,"first-page-only-budget-"+c);
 Require(page.BackendCallCount==6&&page.SourceFillCount==3&&page.SourceRowsCopied==14&&page.OuterDepthUsed==3&&page.SourceNextCursor==3&&page.ContextNodesAdded==0,"complete-call-fill-domains-"+c);
 Require(b.ReadLiveNodes().SequenceEqual(before)&&b.LiveNodeCount==18&&b.NextCursor==0&&boots[c].ReadConstructorState().TreeRootAbsent,"historical-and-live-inputs-unchanged-"+c);
 Refuse<InvalidOperationException>("replay-"+c,()=>LegacyFst28CNumberInitialPage.CreateFirstNormalPage(b));
 byte[] bytes=page.CopyRecordBytes();var saved=page.ReadRecord(0);bytes[0]^=255;
 Require(page.ReadRecord(0)==saved&&page.CopyRecordBytes()[0]!=(byte)bytes[0],"defensive-record-bytes-"+c);
 var detached=saved with{Cost=-1};Require(saved.Cost!=detached.Cost&&page.ReadRecord(0)==saved,"detached-record-value-"+c);
 Require(page.ReadRecord(0).ContextIdentity==b.ReadSourceNode(1)&&b.ReadExistingSlot(1)==0&&b.ReadSourceNode(0)==0,"zero-slot-not-null-context-"+c);
 foreach(int bad in new[]{-1,page.Count})Refuse<ArgumentOutOfRangeException>("record-index-"+c+"-"+bad,()=>page.ReadRecord(bad));
 foreach(int bad in new[]{-1,6})Refuse<ArgumentOutOfRangeException>("call-index-"+c+"-"+bad,()=>page.ReadBackendCall(bad));
 foreach(int bad in new[]{-1,3})Refuse<ArgumentOutOfRangeException>("cursor-index-"+c+"-"+bad,()=>page.ReadSourceCursor(bad));
 Refuse<OperationCanceledException>("inspection-canceled-"+c,()=>page.CopyRecordBytes(new(true)));
}
var extraKinds=new List<string>();int extraGroups=0;
int ContextWindows(LegacyFreshMainCompactOrdinaryGraph g)=>g.Source.Source.Steps.Where(s=>!s.IsPendingSeparator)
 .Sum(s=>Enumerable.Range(s.FirstPhysical+1,Math.Max(0,s.LastPhysical-s.FirstPhysical)).Sum(start=>Math.Min(3,s.LastPhysical-start+1)));
LegacyFst28CNumberInitialQueryBinding Extra(string purpose)
{
 if(ContextWindows(graphs[0])!=0||graphs[0].Source.Source.Preparation.Strokes.Count!=1)throw new Exception("Extra owners require genuine single-track fixture/no context NN.");
 var boot=LegacyFst28CFreshPrimaryControllerBootstrap.CreateDirectSelectedNumberFresh(initial,graphs[0],ak);
 var m=LegacyFst28CNumberSymbolMasks.CreateActualDirectNumberFresh(boot,ak);
 var d=LegacyFst28CNumberInitialD0.CreateActualDirectNumberFresh(m,ak);extraGroups+=d.AcousticEvaluations;extraKinds.Add(purpose);
 return LegacyFst28CNumberInitialQueryBinding.CreateFreshInitialPrefix(d);
}
foreach(string stage in new[]{"after-claim","after-owned-copies","before-source-fill","after-source-fill","after-backend-call","after-complete-page","before-publish"})
{
 var b=Extra("cancel-"+stage);var before=b.ReadLiveNodes().ToArray();
 if(stage=="after-claim")
 {
  Require(ReferenceEquals(b.Graph,bindings[0].Graph)&&ReferenceEquals(b.InitialState,bindings[0].InitialState)&&ReferenceEquals(b.Policy,bindings[0].Policy)&&
   !ReferenceEquals(b.Bootstrap,bindings[0].Bootstrap)&&!ReferenceEquals(ProbeAccess.Header(b.Bootstrap,b.D0,default),ProbeAccess.Header(boots[0],owners[0],default)),"equal-data-independent-header-owner");
  Refuse<ArgumentException>("equal-data-foreign-header-refused",()=>ProbeAccess.Header(boots[0],b.D0,default));
 }
 using var cs=new CancellationTokenSource();bool reached=false;
 Refuse<OperationCanceledException>("cancel-"+stage,()=>Bounded(b,42288,165168,cs.Token,s=>{if(s==stage){reached=true;cs.Cancel();}}));Require(reached,"reached-"+stage);
 Refuse<InvalidOperationException>("poisoned-"+stage,()=>LegacyFst28CNumberInitialPage.CreateFirstNormalPage(b));
 Require(b.ReadLiveNodes().SequenceEqual(before)&&b.LiveNodeCount==18&&b.RetainsExactHeaderIdentity(),"no-input-mutation-"+stage);
}
var fault=Extra("fault-after-backend-call");Refuse<ApplicationException>("fault-after-backend-call",()=>Bounded(fault,42288,165168,default,s=>{if(s=="after-backend-call")throw new ApplicationException();}));
Refuse<InvalidOperationException>("fault-poisoned",()=>LegacyFst28CNumberInitialPage.CreateFirstNormalPage(fault));
var concurrent=Extra("concurrent-claim");int won=0,lost=0;
Parallel.For(0,2,_=>{try{LegacyFst28CNumberInitialPage.CreateFirstNormalPage(concurrent);Interlocked.Increment(ref won);}catch(InvalidOperationException){Interlocked.Increment(ref lost);}});
Require(won==1&&lost==1,"one-concurrent-page-claim-winner");
Parallel.For(0,1024,_=>{var p=pages[0];if(p.ReadRecord(0)!=pages[0].ReadRecord(0)||p.Binding.LiveNodeCount!=18||p.ReadBackendCall(5).Returned!=0)throw new Exception("Mutable inspection");});guards.Add("parallel-immutable-inspection-1024");
using var file=File.Create(args[1]);using var w=new Utf8JsonWriter(file);
void I(string? name,IEnumerable<long> values){if(name is not null)w.WritePropertyName(name);w.WriteStartArray();foreach(long n in values)w.WriteNumberValue(n);w.WriteEndArray();}
w.WriteStartObject();w.WriteNumber("schema",1);w.WriteString("scope","complete-actual-first-normal-number-page-only");w.WriteBoolean("paging",false);w.WriteBoolean("generalQuery",false);w.WriteNumber("bytes",42288);w.WriteNumber("work",165168);
w.WriteNumber("canonicalD0Owners",4);w.WriteNumber("canonicalGlyphNetworkEvaluations",4*owners.Sum(o=>o.AcousticEvaluations));w.WriteNumber("canonicalContextNetworkEvaluations",graphs.Sum(ContextWindows));
w.WriteNumber("additionalGuardD0Owners",extraKinds.Count);w.WriteNumber("additionalGuardGlyphNetworkEvaluations",4*extraGroups);w.WriteNumber("additionalGuardContextNetworkEvaluations",0);
w.WriteStartArray("additionalGuardPurposes");foreach(var kind in extraKinds)w.WriteStringValue(kind);w.WriteEndArray();
w.WriteStartArray("guards");foreach(string g in guards)w.WriteStringValue(g);w.WriteEndArray();w.WriteStartArray("cases");
for(int c=0;c<4;c++)
{
 var p=pages[c];var b=p.Binding;var nodes=b.ReadLiveNodes();var slots=nodes.ToDictionary(n=>n.Identity,n=>n.PageSlot);long Slot(ulong id)=>id==0?-1:slots[id];
 w.WriteStartObject();w.WriteNumber("index",c);w.WriteNumber("visible",p.Count);w.WriteNumber("backendCalls",p.BackendCallCount);w.WriteNumber("sourceV80Fills",p.SourceFillCount);w.WriteNumber("sourceRowsCopied",p.SourceRowsCopied);w.WriteNumber("sourceNextCursor",p.SourceNextCursor);w.WriteNumber("outerDepthUsed",p.OuterDepthUsed);w.WriteNumber("contextNodesAdded",p.ContextNodesAdded);
 I("current",Enumerable.Range(0,16).Select(i=>(long)b.ReadCurrentCode((ushort)i)));I("sourceNodes",Enumerable.Range(0,16).Select(i=>Slot(b.ReadSourceNode((ushort)i))));
 w.WriteStartArray("nodes");foreach(var n in nodes)I(null,[n.PageSlot,n.Symbol,Slot(n.Left),Slot(n.Right),n.Balance,n.ParentLink==0?0:1,n.ChildTreeRoot==0?0:1,n.PageIndex]);w.WriteEndArray();
 w.WriteStartArray("records");for(int i=0;i<p.Count;i++){var r=p.ReadRecord(i);I(null,[(long)r.Tag,Slot(r.ContextIdentity),r.Cost,r.Symbol,r.SourceCategory,r.CaseFlags,(long)r.Extra]);}w.WriteEndArray();
 w.WriteStartArray("calls");for(int i=0;i<p.BackendCallCount;i++){var r=p.ReadBackendCall(i);I(null,[r.State,(long)r.Tag,r.Skip,r.Depth,r.SourceSlot,r.PhysicalBefore,r.AcceptedBefore,r.PhysicalAfter,r.AcceptedAfter,r.Returned]);}w.WriteEndArray();
 w.WriteStartArray("cursorFinal");for(int i=0;i<p.SourceFillCount;i++){var r=p.ReadSourceCursor(i);I(null,[r.State,r.SourceSlot,r.Physical,r.Accepted,r.RawCount]);}w.WriteEndArray();
 // Normalize ONLY context IDs to same-owner page/slot in diagnostic bytes.
 var raw=p.CopyRecordBytes();for(int i=0;i<p.Count;i++)BinaryPrimitives.WriteInt64LittleEndian(raw.AsSpan(i*32+8,8),Slot(p.ReadRecord(i).ContextIdentity));w.WriteString("normalizedRecordHex",Convert.ToHexString(raw).ToLowerInvariant());
 w.WriteEndObject();
}
w.WriteEndArray();w.WriteEndObject();w.Flush();Console.WriteLine($"PASS4 complete number first pages/{guards.Count} guards; {extraKinds.Count} extra genuine D0 guard owners/{4*extraGroups} glyphNN separately, no contextNN.");

static class ProbeAccess
{
 [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.StaticMethod,Name="CreateBounded")]
 internal static extern LegacyFst28CNumberInitialPage CreateBounded(LegacyFst28CNumberInitialPage owner,LegacyFst28CNumberInitialQueryBinding binding,int bytes,int work,CancellationToken token,Action<string>? checkpoint);
 [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Method,Name="RetainInitialNumberHeaderIdentity")]
 internal static extern object Header(LegacyFst28CFreshPrimaryControllerBootstrap owner,LegacyFst28CNumberInitialD0 d0,CancellationToken token);
}
