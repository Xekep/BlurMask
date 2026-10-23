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
var guards=new List<string>();
void Require(bool good,string name){if(!good)throw new Exception(name);guards.Add(name);}
void Refuse<T>(string name,Action action)where T:Exception
{try{action();}catch(T){guards.Add(name);return;}throw new Exception("Missing refusal "+name);}
var extraKinds=new List<string>();int extraSelected=0;
int ContextWindows(LegacyFreshMainCompactOrdinaryGraph g)=>g.Source.Source.Steps.Where(s=>!s.IsPendingSeparator)
 .Sum(s=>Enumerable.Range(s.FirstPhysical+1,Math.Max(0,s.LastPhysical-s.FirstPhysical)).Sum(start=>Math.Min(3,s.LastPhysical-start+1)));
LegacyFst28CNumberInitialD0 Extra(string purpose,bool sameBoot=false)
{
 var boot=sameBoot?boots[0]:LegacyFst28CFreshPrimaryControllerBootstrap.CreateDirectSelectedNumberFresh(initial,graphs[0],ak);
 var m=LegacyFst28CNumberSymbolMasks.CreateActualDirectNumberFresh(boot,ak);
 var d=LegacyFst28CNumberInitialD0.CreateActualDirectNumberFresh(m,ak);
 extraKinds.Add(purpose);extraSelected+=d.AcousticEvaluations;
 if(ContextWindows(graphs[0])!=0||graphs[0].Source.Source.Preparation.Strokes.Count!=1)throw new Exception("Extra fixtures must have zero cross-stroke context windows.");
 return d;
}
Refuse<ArgumentNullException>("null-d0",()=>LegacyFst28CNumberInitialQueryBinding.CreateFreshInitialPrefix(null!));
Refuse<OperationCanceledException>("cancel-before-null",()=>LegacyFst28CNumberInitialQueryBinding.CreateFreshInitialPrefix(null!,new(true)));
foreach(int b in new[]{-1,8955,int.MaxValue})Refuse<ArgumentOutOfRangeException>("byte-domain-"+b,()=>LegacyFst28CNumberInitialQueryBinding.CreateBounded(owners[0],b,22202));
foreach(int b in new[]{-1,22203,int.MaxValue})Refuse<ArgumentOutOfRangeException>("work-domain-"+b,()=>LegacyFst28CNumberInitialQueryBinding.CreateBounded(owners[0],8954,b));
Refuse<InvalidDataException>("one-byte-short",()=>LegacyFst28CNumberInitialQueryBinding.CreateBounded(owners[0],8953,22202));
Refuse<InvalidDataException>("one-work-short",()=>LegacyFst28CNumberInitialQueryBinding.CreateBounded(owners[0],8954,22201));
Refuse<ArgumentException>("foreign-bootstrap-d0",()=>boots[0].RetainInitialNumberHeaderIdentity(owners[1]));
Require(boots.Select((b,i)=>b.RetainInitialNumberHeaderIdentity(owners[i])).Distinct(ReferenceEqualityComparer.Instance).Count()==4&&graphs.Distinct(ReferenceEqualityComparer.Instance).Count()==4&&owners.Distinct(ReferenceEqualityComparer.Instance).Count()==4,"four-independent-private-header-graph-owners");
using(var cs=new CancellationTokenSource())
{
 bool reached=false;Refuse<OperationCanceledException>("cancel-before-claim",()=>LegacyFst28CNumberInitialQueryBinding.CreateBounded(owners[0],8954,22202,cs.Token,s=>{if(s=="before-claim"){reached=true;cs.Cancel();}}));Require(reached,"before-claim-reached");
}
var bindings=new LegacyFst28CNumberInitialQueryBinding[4];
for(int i=0;i<4;i++)
{
 var b=bindings[i]=LegacyFst28CNumberInitialQueryBinding.CreateBounded(owners[i],8954,22202);
 Require(ReferenceEquals(b.D0,owners[i])&&ReferenceEquals(b.Bootstrap,boots[i])&&ReferenceEquals(b.Graph,graphs[i])&&ReferenceEquals(b.InitialState,initial)&&ReferenceEquals(b.Policy,policy)&&ReferenceEquals(b.Source,outgoing)&&b.RetainsExactHeaderIdentity(),"exact-owner-cohort-"+i);
 var q=b.ReadRequestDiagnostic();Require(q.HeaderSameBootstrap&&q.HeaderSameRequest&&q.CohortSame&&q.D0Same&&q.HeaderNonNull&&q.LiveTreeNonNull,"private-issued-prefix-observable-"+i);
 Require(!b.HasQueryAuthority&&!b.HasPagingAuthority&&b.LogicalByteReservation==8954&&b.WorkReservation==22202,"bounded-inspection-only-"+i);
 Require(boots[i].ReadConstructorState().TreeRootAbsent&&!boots[i].HasIssuedQuery&&initial.ReadCursor(0).State==0,"constructor-views-remain-historical-"+i);
 Refuse<InvalidOperationException>("same-d0-replay-"+i,()=>LegacyFst28CNumberInitialQueryBinding.CreateFreshInitialPrefix(owners[i]));
}
var same=Extra("distinct-d0-same-bootstrap",true);Require(!ReferenceEquals(same,owners[0])&&ReferenceEquals(same.Bootstrap,owners[0].Bootstrap),"real-distinct-d0-same-bootstrap");
Refuse<InvalidOperationException>("different-d0-same-bootstrap-replay",()=>LegacyFst28CNumberInitialQueryBinding.CreateFreshInitialPrefix(same));
foreach(string stage in new[]{"after-claim","after-map-copies","after-source-nodes","before-publish"})
{
 var d=Extra("cancel-"+stage);
 if(stage=="after-claim")
 {
  Require(!ReferenceEquals(d.Bootstrap,boots[0])&&ReferenceEquals(d.Graph,graphs[0])&&ReferenceEquals(d.InitialState,initial)&&ReferenceEquals(d.Policy,policy)&&d.CopyCurrentPrimary().SequenceEqual(owners[0].CopyCurrentPrimary())&&
   !ReferenceEquals(d.Bootstrap.RetainInitialNumberHeaderIdentity(d),boots[0].RetainInitialNumberHeaderIdentity(owners[0])),"equal-data-foreign-bootstrap-private-header");
  Refuse<ArgumentException>("equal-data-foreign-bootstrap-d0",()=>boots[0].RetainInitialNumberHeaderIdentity(d));
 }
 using var cs=new CancellationTokenSource();bool reached=false;
 Refuse<OperationCanceledException>("cancel-"+stage,()=>LegacyFst28CNumberInitialQueryBinding.CreateBounded(d,8954,22202,cs.Token,s=>{if(s==stage){reached=true;cs.Cancel();}}));Require(reached,"reached-"+stage);
 Refuse<InvalidOperationException>("poisoned-"+stage,()=>LegacyFst28CNumberInitialQueryBinding.CreateFreshInitialPrefix(d));
 Require(d.Bootstrap.ReadConstructorState().TreeRootAbsent&&d.CopyCurrentPrimary().SequenceEqual(owners[0].CopyCurrentPrimary()),"no-input-mutation-"+stage);
}
var fault=Extra("fault-after-source-nodes");Refuse<ApplicationException>("fault-after-source-nodes",()=>LegacyFst28CNumberInitialQueryBinding.CreateBounded(fault,8954,22202,default,s=>{if(s=="after-source-nodes")throw new ApplicationException();}));
Refuse<InvalidOperationException>("fault-poisoned",()=>LegacyFst28CNumberInitialQueryBinding.CreateFreshInitialPrefix(fault));
var concurrent=Extra("concurrent-claim");int won=0,lost=0;Parallel.For(0,2,_=>{try{LegacyFst28CNumberInitialQueryBinding.CreateFreshInitialPrefix(concurrent);Interlocked.Increment(ref won);}catch(InvalidOperationException){Interlocked.Increment(ref lost);}});
Require(won==1&&lost==1,"one-concurrent-bootstrap-claim-winner");
var v=bindings[0];var node0=v.ReadLiveNodes()[0];
Refuse<NotSupportedException>("readonly-node-list",()=>((IList<LegacySymbolContextNode>)v.ReadLiveNodes())[0]=node0 with{Symbol=123});
var detachedNode=node0 with{Symbol=123};Require(v.ReadLiveNodes()[0].Symbol==node0.Symbol&&detachedNode.Symbol!=node0.Symbol,"immutable-node-value");
var detachedAux=v.ReadAuxiliary(0)with{Key=123};Require(v.ReadAuxiliary(0).Key==0&&detachedAux.Key==123,"immutable-auxiliary-value");
var detachedReset=v.ReadResetWrite(0)with{Value=0};Require(v.ReadResetWrite(0).Value==-1&&detachedReset.Value==0,"immutable-reset-value");
foreach(int bad in new[]{-1,15})Refuse<ArgumentOutOfRangeException>("aux-index-"+bad,()=>v.ReadAuxiliary(bad));
foreach(int bad in new[]{-1,25})Refuse<ArgumentOutOfRangeException>("reset-index-"+bad,()=>v.ReadResetWrite(bad));
foreach(int bad in new[]{-1,5})Refuse<ArgumentOutOfRangeException>("prefix-index-"+bad,()=>v.ReadOuterPrefixNode(bad));
foreach(ushort bad in new ushort[]{16,192,65535})
{
 Refuse<ArgumentOutOfRangeException>("preserved-label-"+bad,()=>v.ReadPreservedCode(bad));Refuse<ArgumentOutOfRangeException>("current-label-"+bad,()=>v.ReadCurrentCode(bad));
 Refuse<ArgumentOutOfRangeException>("secondary-label-"+bad,()=>v.ReadSecondaryCode(bad));Refuse<ArgumentOutOfRangeException>("source-node-label-"+bad,()=>v.ReadSourceNode(bad));Refuse<ArgumentOutOfRangeException>("slot-label-"+bad,()=>v.ReadExistingSlot(bad));
}
Refuse<OperationCanceledException>("header-inspection-cancel",()=>v.RetainsExactHeaderIdentity(new(true)));
Refuse<OperationCanceledException>("request-inspection-cancel",()=>v.ReadRequestDiagnostic(new(true)));
Parallel.For(0,1024,_=>{if(v.ReadLiveNodes().Count!=18||v.LiveTreeRoot==0||!v.RetainsExactHeaderIdentity()||v.ReadRequestDiagnostic().Tag!=(8UL<<40))throw new Exception("Inspection race");});guards.Add("parallel-immutable-inspection-1024");
using var file=File.Create(args[1]);using var w=new Utf8JsonWriter(file);
void I(string? name,IEnumerable<long> values){if(name is not null)w.WritePropertyName(name);w.WriteStartArray();foreach(long n in values)w.WriteNumberValue(n);w.WriteEndArray();}
w.WriteStartObject();w.WriteNumber("schema",1);w.WriteString("scope","after-D0-library-active-overlay-first-prefix-only");w.WriteBoolean("query",false);w.WriteBoolean("paging",false);w.WriteNumber("bytes",8954);w.WriteNumber("work",22202);
w.WriteNumber("canonicalD0Owners",4);w.WriteNumber("canonicalSelectedGroups",owners.Sum(o=>o.AcousticEvaluations));w.WriteNumber("canonicalGlyphNetworkEvaluations",4*owners.Sum(o=>o.AcousticEvaluations));w.WriteNumber("canonicalContextNetworkEvaluations",graphs.Sum(ContextWindows));
w.WriteNumber("additionalGuardD0Owners",extraKinds.Count);w.WriteNumber("additionalGuardSelectedGroups",extraSelected);w.WriteNumber("additionalGuardGlyphNetworkEvaluations",4*extraSelected);w.WriteNumber("additionalGuardContextNetworkEvaluations",0);
w.WriteStartArray("additionalGuardPurposes");foreach(string kind in extraKinds)w.WriteStringValue(kind);w.WriteEndArray();
w.WriteStartArray("guards");foreach(string g in guards)w.WriteStringValue(g);w.WriteEndArray();w.WriteStartArray("cases");
for(int c=0;c<4;c++)
{
 var b=bindings[c];var nodes=b.ReadLiveNodes();var slots=nodes.ToDictionary(n=>n.Identity,n=>n.PageSlot);int Slot(ulong id)=>id==0?-1:slots[id];
 w.WriteStartObject();w.WriteNumber("index",c);w.WriteNumber("root",Slot(b.LiveTreeRoot));w.WriteNumber("capacity",b.CursorCapacity);w.WriteNumber("cursors",b.CursorCount);w.WriteNumber("next",b.NextCursor);
 I("preserved",Enumerable.Range(0,16).Select(i=>(long)b.ReadPreservedCode((ushort)i)));I("current",Enumerable.Range(0,16).Select(i=>(long)b.ReadCurrentCode((ushort)i)));I("secondary",Enumerable.Range(0,16).Select(i=>(long)b.ReadSecondaryCode((ushort)i)));
 I("sourceNodes",Enumerable.Range(0,16).Select(i=>(long)Slot(b.ReadSourceNode((ushort)i))));I("existingSlots",Enumerable.Range(0,16).Select(i=>(long)b.ReadExistingSlot((ushort)i)));I("prefixNodes",Enumerable.Range(0,5).Select(i=>(long)Slot(b.ReadOuterPrefixNode(i))));
 w.WriteStartArray("nodes");foreach(var n in nodes)I(null,[n.PageSlot,n.Symbol,Slot(n.Left),Slot(n.Right),n.Balance,n.ParentLink==0?0:1,n.ChildTreeRoot==0?0:1,n.PageIndex]);w.WriteEndArray();
 w.WriteStartArray("auxiliaries");for(int i=0;i<15;i++){var a=b.ReadAuxiliary(i);I(null,[a.Slot,a.Key,Slot(a.NodeIdentity),a.HistoryPayloadPresent?1:0,a.SecondaryPayloadPresent?1:0]);}w.WriteEndArray();
 w.WriteStartArray("resetWrites");for(int i=0;i<25;i++){var r=b.ReadResetWrite(i);I(null,[r.Offset,r.Width,r.Value]);}w.WriteEndArray();
 var q=b.ReadRequestDiagnostic();I("request",[(long)q.Tag,q.HeaderNonNull?1:0,q.HeaderSameBootstrap?1:0,q.HeaderSameRequest?1:0,q.CohortSame?1:0,q.D0Same?1:0,q.ParentAbsent?1:0,q.Source,q.Start,q.Span,q.Flags,q.Skip,q.Take,q.CachedOwnerAbsent?1:0,q.LiveTreeNonNull?1:0]);
 w.WriteEndObject();
}
w.WriteEndArray();w.WriteEndObject();w.Flush();Console.WriteLine($"PASS4canonical binding owners/{guards.Count}guards; additional{extraKinds.Count}genuine guardD0owners/{4*extraSelected}glyphNN, no contextNN; query/paging excluded.");
