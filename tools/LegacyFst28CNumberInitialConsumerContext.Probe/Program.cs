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
LegacyFst28CNumberInitialConsumerContext Bounded(LegacyFst28CNumberInitialD0 d,int bytes=55626,int work=226858,CancellationToken token=default,Action<string>? checkpoint=null)
 => ProbeAccess.Bounded(null!,d,bytes,work,token,checkpoint);
Refuse<ArgumentNullException>("null-d0",()=>LegacyFst28CNumberInitialConsumerContext.CreateFreshInitialContext(null!));
Refuse<OperationCanceledException>("cancel-before-null",()=>LegacyFst28CNumberInitialConsumerContext.CreateFreshInitialContext(null!,new(true)));
foreach(int n in new[]{-1,55627,int.MaxValue})Refuse<ArgumentOutOfRangeException>("byte-domain-"+n,()=>Bounded(owners[0],n));
foreach(int n in new[]{-1,226859,int.MaxValue})Refuse<ArgumentOutOfRangeException>("work-domain-"+n,()=>Bounded(owners[0],55626,n));
Refuse<InvalidDataException>("one-byte-short",()=>Bounded(owners[0],55625));Refuse<InvalidDataException>("one-work-short",()=>Bounded(owners[0],55626,226857));
ProbeAccess.ValidatePool(null!,owners[0],owners[0],default);guards.Add("same-genuine-pool-validation-no-claim");
Refuse<ArgumentException>("foreign-genuine-pool-d0",()=>ProbeAccess.ValidatePool(null!,owners[0],owners[1],default));
Require(boots.Select((b,i)=>ProbeAccess.Header(b,owners[i],default)).Distinct(ReferenceEqualityComparer.Instance).Count()==4,"four-private-header-identities");
using(var cs=new CancellationTokenSource())
{
 bool reached=false;Refuse<OperationCanceledException>("cancel-before-prefix-claim",()=>Bounded(owners[0],55626,226858,cs.Token,s=>{if(s=="before-prefix-claim"){reached=true;cs.Cancel();}}));Require(reached,"before-prefix-claim-reached");
}
var results=new LegacyFst28CNumberInitialConsumerContext[4];
for(int c=0;c<4;c++)
{
 var r=results[c]=Bounded(owners[c]);
 Require(ReferenceEquals(r.D0,owners[c])&&ReferenceEquals(r.D0.Graph,graphs[c])&&r.ContextPageCount==1,"same-genuine-owner-one-page-"+c);
 Require(!r.HasQueryAuthority&&!r.HasParentHistoryAuthority&&!r.HasCompleteWordStartup&&r.LogicalByteReservation==55626&&r.WorkReservation==226858,"context-stage-only-budget-"+c);
 Require(r.InitialContextNodeCount==18&&r.ActiveContextNodeCount==18+r.Count&&r.ActiveContextNodeCount<=30&&r.SourceNamespace==1&&r.SourceHeader==1&&r.SourceKind40==0,"bounded-active-number-context-"+c);
 var diag=ProbeAccess.Diagnostic(r,default);Require(diag.SequenceEqual(new[]{1,1,1,1,1,1,1,1,18,r.ActiveContextNodeCount,1,30,1}),"private-attach-header-pool-observable-"+c);
 diag[5]=0;Require(ProbeAccess.Diagnostic(r,default)[5]==1,"diagnostic-defensive-copy-"+c);
 Refuse<InvalidOperationException>("same-d0-consumer-replay-"+c,()=>LegacyFst28CNumberInitialConsumerContext.CreateFreshInitialContext(owners[c]));
 Refuse<InvalidOperationException>("consumed-consumer-refuses-default18-"+c,()=>LegacyFst28CNumberInitialQueryBinding.CreateFreshInitialPrefix(owners[c]));
 var first=r.ReadGraft(0);Require(first.Created&&first.SourceRecord.ContextIdentity!=first.GraftedContextIdentity&&r.ReadActiveContextNodes().Single(n=>n.Identity==first.SourceRecord.ContextIdentity).ParentLink==0,"source-leaf-not-replaced-"+c);
 var detached=first with{Created=false};Require(r.ReadGraft(0).Created&&detached.Created==false,"immutable-graft-value-"+c);
 var history=ProbeAccess.History(r,default);Require(history.Length==18&&history.All(n=>n.ParentLink==0&&n.ChildTreeRoot==0),"all-original18-historical-nodes-"+c);
 var originalNode=history[0];history[0]=history[0]with{Symbol=65535};Require(ProbeAccess.History(r,default)[0]==originalNode,"historical-snapshot-defensive-copy-"+c);
 var node=r.ReadActiveContextNodes()[0];Refuse<NotSupportedException>("readonly-active-node-list-"+c,()=>((IList<LegacySymbolContextNode>)r.ReadActiveContextNodes())[0]=node with{Symbol=65535});
 foreach(int bad in new[]{-1,r.Count})Refuse<ArgumentOutOfRangeException>("graft-index-"+c+"-"+bad,()=>r.ReadGraft(bad));
 Refuse<OperationCanceledException>("diagnostic-canceled-"+c,()=>ProbeAccess.Diagnostic(r,new(true)));
}
var extraKinds=new List<string>();int extraGroups=0;
int ContextWindows(LegacyFreshMainCompactOrdinaryGraph g)=>g.Source.Source.Steps.Where(s=>!s.IsPendingSeparator)
 .Sum(s=>Enumerable.Range(s.FirstPhysical+1,Math.Max(0,s.LastPhysical-s.FirstPhysical)).Sum(start=>Math.Min(3,s.LastPhysical-start+1)));
LegacyFst28CNumberInitialD0 Extra(string purpose)
{
 if(ContextWindows(graphs[0])!=0||graphs[0].Source.Source.Preparation.Strokes.Count!=1)throw new Exception("Guard fixture must have no contextNN.");
 var boot=LegacyFst28CFreshPrimaryControllerBootstrap.CreateDirectSelectedNumberFresh(initial,graphs[0],ak);
 var m=LegacyFst28CNumberSymbolMasks.CreateActualDirectNumberFresh(boot,ak);var d=LegacyFst28CNumberInitialD0.CreateActualDirectNumberFresh(m,ak);
 extraKinds.Add(purpose);extraGroups+=d.AcousticEvaluations;return d;
}
foreach(string stage in new[]{"binding-after-claim","after-prefix","after-page","before-graft-0","after-graft-0","after-active-snapshot","before-publish"})
{
 var d=Extra("cancel-"+stage);
 if(stage=="binding-after-claim")
 {
  Require(ReferenceEquals(d.Graph,owners[0].Graph)&&d.CopyCurrentPrimary().SequenceEqual(owners[0].CopyCurrentPrimary())&&!ReferenceEquals(d.Bootstrap,boots[0]),"equal-data-foreign-pool-owner");
  Refuse<ArgumentException>("equal-data-foreign-pool-refused",()=>ProbeAccess.ValidatePool(null!,d,owners[0],default));
 }
 using var cs=new CancellationTokenSource();bool reached=false;
 Refuse<OperationCanceledException>("cancel-"+stage,()=>Bounded(d,55626,226858,cs.Token,s=>{if(s==stage){reached=true;cs.Cancel();}}));Require(reached,"reached-"+stage);
 Refuse<InvalidOperationException>("poisoned-consumer-"+stage,()=>LegacyFst28CNumberInitialConsumerContext.CreateFreshInitialContext(d));
 Refuse<InvalidOperationException>("poisoned-default18-"+stage,()=>LegacyFst28CNumberInitialQueryBinding.CreateFreshInitialPrefix(d));
 Require(d.Bootstrap.ReadConstructorState().TreeRootAbsent&&d.CopyCurrentPrimary().SequenceEqual(owners[0].CopyCurrentPrimary()),"no-d0-mutation-"+stage);
}
var fault=Extra("fault-after-graft-0");Refuse<ApplicationException>("fault-after-graft-0",()=>Bounded(fault,55626,226858,default,s=>{if(s=="after-graft-0")throw new ApplicationException();}));
Refuse<InvalidOperationException>("fault-shared-claim-poisoned",()=>LegacyFst28CNumberInitialQueryBinding.CreateFreshInitialPrefix(fault));
var prior=Extra("completed-default18");var historical=LegacyFst28CNumberInitialQueryBinding.CreateFreshInitialPrefix(prior);Require(historical.LiveNodeCount==18,"default18-stays18");
Refuse<InvalidOperationException>("historical-default18-not-upgraded",()=>LegacyFst28CNumberInitialConsumerContext.CreateFreshInitialContext(prior));Require(historical.LiveNodeCount==18&&historical.ReadLiveNodes().All(n=>n.ChildTreeRoot==0),"default18-unchanged-after-rejection");
// Both actual paths pause after validation BEFORE the SAME Bootstrap CWT claim.
// Race winner is deliberately not serialized; only one valid completed path.
var race=Extra("default18-vs-private30-race");using var arrived=new CountdownEvent(2);using var release=new ManualResetEventSlim(false);int won=0,lost=0,valid=0;
void WaitClaim(string s,string wanted){if(s==wanted){arrived.Signal();if(!release.Wait(TimeSpan.FromSeconds(30)))throw new Exception("claim barrier timeout");}}
var t1=Task.Run(()=>{try{var b=ProbeAccess.DefaultBounded(null!,race,8954,22202,default,s=>WaitClaim(s,"before-claim"));if(b.LiveNodeCount==18)Interlocked.Increment(ref valid);Interlocked.Increment(ref won);}catch(InvalidOperationException){Interlocked.Increment(ref lost);}});
var t2=Task.Run(()=>{try{var r=Bounded(race,55626,226858,default,s=>WaitClaim(s,"binding-before-claim"));if(r.ActiveContextNodeCount==18+r.Count&&r.ContextPageCount==1)Interlocked.Increment(ref valid);Interlocked.Increment(ref won);}catch(InvalidOperationException){Interlocked.Increment(ref lost);}});
if(!arrived.Wait(TimeSpan.FromSeconds(30)))throw new Exception("both claim entries not reached");release.Set();Task.WaitAll(t1,t2);Require(won==1&&lost==1&&valid==1,"shared-default18-private30-claim-one-winner");
Parallel.For(0,1024,_=>{if(results[0].ReadActiveContextNodes().Count!=18+results[0].Count||ProbeAccess.Diagnostic(results[0],default)[12]!=1)throw new Exception("Mutable active snapshot");});guards.Add("parallel-immutable-active-inspection-1024");
// These use ONLY public generic kernels and caller-prepared contexts. They
// are numeric controls, explicitly NOT typed actual consumer/pool receipts.
var prepared=new List<(string Name,int Capacity,LegacySymbolContextFactory Context,ulong[] Grafts)>();
ushort[] preserved=Enumerable.Range(0,16).Select(i=>policy.PrimaryMap.MapLabel((ushort)i).Code).ToArray();
ushort[] leaves=Enumerable.Range(1,10).Concat(new[]{14,15}).Select(i=>preserved[i]).ToArray();
foreach(var spec in new[]{("empty",30,0,false),("one",30,1,false),("eight",30,8,false),("twelve",30,12,false),("twelve-reversed",30,12,true),("old18-allocation-refusal",18,12,false)})
{
 var ctx=new LegacySymbolContextFactory(spec.Item2);ctx.CreateInitialSourceMap(preserved);foreach(ushort code in new ushort[]{0xfffb,0xfff9,0xfffb,0xfffc,0xfffa})ctx.ResolveInitial(code);
 var codes=leaves.Take(spec.Item3).ToArray();if(spec.Item4)Array.Reverse(codes);var ids=new ulong[codes.Length];for(int i=0;i<codes.Length;i++)ids[i]=LegacyNativeWordStartupRequestFactory.GraftOrdinaryInitialContext(ctx,ctx.ResolveInitial(codes[i]).Identity);
 Require(ctx.NodeCount==(spec.Item2==18?18:18+codes.Length)&&ids.All(id=>spec.Item2==18?id==0:id!=0),"prepared-only-"+spec.Item1);prepared.Add((spec.Item1,spec.Item2,ctx,ids));
}
using var file=File.Create(args[1]);using var w=new Utf8JsonWriter(file);
void I(string? name,IEnumerable<long> values){if(name is not null)w.WritePropertyName(name);w.WriteStartArray();foreach(long n in values)w.WriteNumberValue(n);w.WriteEndArray();}
void Nodes(IEnumerable<LegacySymbolContextNode> values){var list=values.ToArray();var index=list.Select((n,i)=>(n.Identity,i)).ToDictionary(v=>v.Identity,v=>v.i);long Slot(ulong id)=>id==0?-1:index[id];w.WriteStartArray("nodes");foreach(var n in list)I(null,[Slot(n.Identity),n.Symbol,Slot(n.Left),Slot(n.Right),n.Balance,Slot(n.ParentLink),Slot(n.ChildTreeRoot),n.PageIndex,n.PageSlot]);w.WriteEndArray();}
w.WriteStartObject();w.WriteNumber("schema",1);w.WriteString("scope","actual-initial-number-consumer-context-only");w.WriteBoolean("query",false);w.WriteBoolean("history",false);w.WriteBoolean("completeWordStartup",false);w.WriteNumber("bytes",55626);w.WriteNumber("work",226858);
w.WriteNumber("canonicalD0Owners",4);w.WriteNumber("canonicalGlyphNN",4*owners.Sum(o=>o.AcousticEvaluations));w.WriteNumber("canonicalContextNN",graphs.Sum(ContextWindows));w.WriteNumber("additionalGuardD0Owners",extraKinds.Count);w.WriteNumber("additionalGuardGlyphNN",4*extraGroups);w.WriteNumber("additionalGuardContextNN",0);
w.WriteStartArray("additionalGuardPurposes");foreach(string kind in extraKinds)w.WriteStringValue(kind);w.WriteEndArray();w.WriteStartArray("guards");foreach(var g in guards)w.WriteStringValue(g);w.WriteEndArray();w.WriteStartArray("cases");
for(int c=0;c<4;c++)
{
 var r=results[c];var nodes=r.ReadActiveContextNodes();var slots=nodes.Select((n,i)=>(n.Identity,i)).ToDictionary(v=>v.Identity,v=>v.i);long Slot(ulong id)=>id==0?-1:slots[id];
 w.WriteStartObject();w.WriteNumber("index",c);w.WriteNumber("count",r.Count);w.WriteNumber("initialNodes",r.InitialContextNodeCount);w.WriteNumber("activeNodes",r.ActiveContextNodeCount);w.WriteNumber("pages",r.ContextPageCount);w.WriteNumber("sourceNamespace",r.SourceNamespace);w.WriteNumber("sourceHeader",r.SourceHeader);w.WriteNumber("sourceKind40",r.SourceKind40);I("ownerDiagnostic",ProbeAccess.Diagnostic(r,default).Select(n=>(long)n));Nodes(nodes);
 var historic=ProbeAccess.History(r,default);var historicSlots=historic.Select((n,i)=>(n.Identity,i)).ToDictionary(v=>v.Identity,v=>v.i);long HSlot(ulong id)=>id==0?-1:historicSlots[id];
 w.WriteStartArray("historicalInitialNodes");foreach(var n in historic)I(null,[HSlot(n.Identity),n.Symbol,HSlot(n.Left),HSlot(n.Right),n.Balance,HSlot(n.ParentLink),HSlot(n.ChildTreeRoot),n.PageIndex,n.PageSlot]);w.WriteEndArray();
 w.WriteStartArray("grafts");for(int i=0;i<r.Count;i++){var g=r.ReadGraft(i);I(null,[g.Ordinal,Slot(g.SourceRecord.ContextIdentity),Slot(g.GraftedContextIdentity),g.SourceRecord.Symbol,Slot(nodes.Single(n=>n.Identity==g.GraftedContextIdentity).ParentLink),g.Created?1:0]);}w.WriteEndArray();
 w.WriteStartArray("sourceRecords");for(int i=0;i<r.Count;i++){var g=r.ReadGraft(i).SourceRecord;I(null,[(long)g.Tag,Slot(g.ContextIdentity),g.Cost,g.Symbol,g.SourceCategory,g.CaseFlags,(long)g.Extra]);}w.WriteEndArray();w.WriteEndObject();
}
w.WriteEndArray();w.WriteStartArray("preparedOnly");foreach(var v in prepared){var nodes=v.Context.CopyNodes();var slots=nodes.ToDictionary(n=>n.Identity,n=>n.PageSlot);w.WriteStartObject();w.WriteString("name",v.Name);w.WriteBoolean("actualAuthority",false);w.WriteNumber("capacity",v.Capacity);w.WriteNumber("count",v.Context.NodeCount);I("graftSlots",v.Grafts.Select(id=>id==0?-1L:slots[id]));Nodes(nodes);w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS4 actual consumer-context owners/{guards.Count}guards; {extraKinds.Count}extraD0/{4*extraGroups}extraGlyphNN, no extraContextNN; no complete-word/query/history claim.");

static class ProbeAccess
{
 [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.StaticMethod,Name="CreateBounded")]
 internal static extern LegacyFst28CNumberInitialConsumerContext Bounded(LegacyFst28CNumberInitialConsumerContext owner,LegacyFst28CNumberInitialD0 d0,int bytes,int work,CancellationToken token,Action<string>? checkpoint);
 [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Method,Name="ReadOwnerDiagnostic")]
 internal static extern int[] Diagnostic(LegacyFst28CNumberInitialConsumerContext owner,CancellationToken token);
 [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Method,Name="CopyHistoricalInitialNodes")]
 internal static extern LegacySymbolContextNode[] History(LegacyFst28CNumberInitialConsumerContext owner,CancellationToken token);
 [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.StaticMethod,Name="ValidatePoolAssociation")]
 internal static extern void ValidatePool(LegacyFst28CNumberInitialConsumerContext owner,LegacyFst28CNumberInitialD0 expected,LegacyFst28CNumberInitialD0 requested,CancellationToken token);
 [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Method,Name="RetainInitialNumberHeaderIdentity")]
 internal static extern object Header(LegacyFst28CFreshPrimaryControllerBootstrap owner,LegacyFst28CNumberInitialD0 d0,CancellationToken token);
 [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.StaticMethod,Name="CreateBounded")]
 internal static extern LegacyFst28CNumberInitialQueryBinding DefaultBounded(LegacyFst28CNumberInitialQueryBinding owner,LegacyFst28CNumberInitialD0 d0,int bytes,int work,CancellationToken token,Action<string>? checkpoint);
}
