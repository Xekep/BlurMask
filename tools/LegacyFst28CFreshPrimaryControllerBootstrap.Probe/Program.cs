using InkRecognition;
using System.Text.Json;
using System.Runtime.CompilerServices;
if(args.Length!=2)throw new ArgumentException("MODELS CAPTURE");
var m=args[0];var guards=new List<string>();var canceled=new CancellationToken(true);
void Require(bool ok,string name){if(!ok)throw new Exception(name);}
void Refuse<T>(string name,Action call)where T:Exception{try{call();}catch(T){guards.Add(name);return;}throw new Exception("Missing refusal "+name);}
byte[] Read(string name)=>File.ReadAllBytes(Path.Combine(m,name));
var ak=Read("ru-RU/ru_RU-ak-cur.lite.res");var lk=Read("ru-RU/ru_RU-lk-text.lite.res");
using var engine=JsonDocument.Parse(Read("ru-RU/legacy-engine-profile.json"));var a=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(Read("ru-RU/legacy-separation-profile.json"));
using var cf=File.OpenRead(Path.Combine(m,"text-unicode/legacy-case-data.json"));using var pf=File.OpenRead(Path.Combine(m,"text-unicode/legacy-text-publication-data.json"));
var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);var line30=Read("line-geometry/line-2e3-30.res");var line50=Read("line-geometry/line-2e3-50.res");
LegacyFst28CInitialState Initial()
{
 var root=LegacyLinguisticRoot13FWireLayout.Read(lk);var wire=LegacyFst23CWireLayout.ReadActualRussianNumber(root);
 var source=LegacyFst13AOutgoingSource.CreateActualRussianNumber(root,wire,ak);var primary=LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(source,ak);
 return LegacyFst28CInitialState.CreateActualRussianNumberFresh(LegacyFst28CTargetPreparation.CreateActualRussianNumberFresh(primary,ak));
}
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
var initial=Initial();var graphs=patterns.Select(Graph).ToArray();var values=graphs.Select(g=>LegacyFst28CFreshPrimaryControllerBootstrap.CreateDirectSelectedNumberFresh(initial,g,ak)).ToArray();
for(int i=0;i<values.Length;i++)
{
 var v=values[i];Require(ReferenceEquals(v.InitialState,initial)&&ReferenceEquals(v.Graph,graphs[i])&&ReferenceEquals(Access.Source(v),graphs[i].Source.Source),"Retained owner identity differs");
 Require(v.ReadSourceMetadata()==new LegacyFst28CNumberSourceMetadata(1,0,1,0,1,uint.MaxValue,1),"Singleton metadata differs");
 Require(v.ReadConstructorState()==new LegacyFst28CBootstrapConstructorState(true,true,true,true,0,0,8,128,0,true,false),"Constructor state differs");
 Require(v.AdmittedSourceCount==1&&v.SelectedSourceCount==1&&v.SelectedOrdinal==0&&!v.HasIssuedQuery,"Unissued singleton scope differs");
}
guards.Add("same-exact-initial-graph-source-policy-cohort");
var graph=graphs[0];var value=values[0];int work=value.WorkReservation;
Require(LegacyFst28CFreshPrimaryControllerBootstrap.CalculateWork(graph.State.NodeCount,graph.State.Groups.Count,graph.State.CandidateCount,graph.State.MaximumAtoms)==work,"Work formula differs");
Require(LegacyFst28CFreshPrimaryControllerBootstrap.CalculateWork(65535,65535,262144,8)==969115,"Max bound differs");guards.Add("pure-max-count-calculator-no-graph-claim");
Require(LegacyFst28CFreshPrimaryControllerBootstrap.CreateBounded(initial,graph,ak,580473,work).ReadSourceMetadata()==value.ReadSourceMetadata(),"Exact work/bytes refused");guards.Add("exact-whole-byte-and-work-budget");
Refuse<ArgumentNullException>("null-initial",()=>LegacyFst28CFreshPrimaryControllerBootstrap.CreateDirectSelectedNumberFresh(null!,graph,ak));
Refuse<ArgumentNullException>("null-graph",()=>LegacyFst28CFreshPrimaryControllerBootstrap.CreateDirectSelectedNumberFresh(initial,null!,ak));
Refuse<ArgumentException>("short-ak",()=>LegacyFst28CFreshPrimaryControllerBootstrap.CreateDirectSelectedNumberFresh(initial,graph,ak.AsSpan(1)));
var bad=ak.ToArray();bad[5233]^=1;Refuse<ArgumentException>("wrong-ak-kind",()=>LegacyFst28CFreshPrimaryControllerBootstrap.CreateDirectSelectedNumberFresh(initial,graph,bad));
bad=ak.ToArray();bad[99]^=1;Refuse<ArgumentException>("wrong-ak-alphabet",()=>LegacyFst28CFreshPrimaryControllerBootstrap.CreateDirectSelectedNumberFresh(initial,graph,bad));
foreach(int b in new[]{-1,580474,int.MaxValue})Refuse<ArgumentOutOfRangeException>("byte-domain-"+b,()=>LegacyFst28CFreshPrimaryControllerBootstrap.CreateBounded(initial,graph,ak,b,work));
foreach(int b in new[]{-1,969116,int.MaxValue})Refuse<ArgumentOutOfRangeException>("work-domain-"+b,()=>LegacyFst28CFreshPrimaryControllerBootstrap.CreateBounded(initial,graph,ak,580473,b));
Refuse<InvalidDataException>("one-byte-short",()=>LegacyFst28CFreshPrimaryControllerBootstrap.CreateBounded(initial,graph,ak,580472,work));
Refuse<InvalidDataException>("one-work-short",()=>LegacyFst28CFreshPrimaryControllerBootstrap.CreateBounded(initial,graph,ak,580473,work-1));
foreach(var (n,g,r,c)in new[]{(0,1,1,8),(65536,1,1,8),(1,0,1,8),(1,65536,1,8),(1,1,0,8),(1,1,262145,8),(1,1,1,0),(1,1,1,9),(int.MaxValue,int.MaxValue,int.MaxValue,8)})
 Refuse<NotSupportedException>($"count-cap-{n}-{g}-{r}-{c}",()=>LegacyFst28CFreshPrimaryControllerBootstrap.CalculateWork(n,g,r,c));
Refuse<OperationCanceledException>("cancel-before-entry",()=>LegacyFst28CFreshPrimaryControllerBootstrap.CreateDirectSelectedNumberFresh(initial,graph,ak,canceled));
Refuse<OperationCanceledException>("cancel-null-invalid-first",()=>LegacyFst28CFreshPrimaryControllerBootstrap.CreateBounded(null!,null!,[], -1,-1,canceled));
Refuse<OperationCanceledException>("cancel-source-snapshot",()=>value.ReadSourceMetadata(canceled));
Refuse<OperationCanceledException>("cancel-constructor-snapshot",()=>value.ReadConstructorState(canceled));
var checkpoints=new[]{"before-snapshot","after-snapshot","after-hash","after-graph","after-scan","before-publish"};
foreach(string stage in checkpoints)
{
 using var cancel=new CancellationTokenSource();bool reached=false;
 Refuse<OperationCanceledException>("cancel-stage-"+stage,()=>LegacyFst28CFreshPrimaryControllerBootstrap.CreateBounded(initial,graph,ak,580473,work,cancel.Token,point=>{if(point==stage){reached=true;cancel.Cancel();}}));
 Require(reached,"Cancellation checkpoint not reached "+stage);
}
var snapshotInput=ak.ToArray();var snapshotValue=LegacyFst28CFreshPrimaryControllerBootstrap.CreateBounded(initial,graph,snapshotInput,580473,work,default,point=>{if(point=="after-snapshot")Array.Fill(snapshotInput,(byte)0);});
Require(snapshotValue.ReadSourceMetadata()==value.ReadSourceMetadata(),"Source hash read mutable caller after snapshot");guards.Add("after-snapshot-caller-mutation-safe");
Refuse<InvalidOperationException>("checkpoint-throw-no-receipt",()=>LegacyFst28CFreshPrimaryControllerBootstrap.CreateBounded(initial,graph,ak,580473,work,default,point=>{if(point=="before-publish")throw new InvalidOperationException("test");}));
var independent=LegacyFst28CFreshPrimaryControllerBootstrap.CreateDirectSelectedNumberFresh(initial,graph,ak);
var secondInitial=Initial();var second=LegacyFst28CFreshPrimaryControllerBootstrap.CreateDirectSelectedNumberFresh(secondInitial,graph,ak);
Require(value.IsSameIssuer(value)&&!value.IsSameIssuer(independent)&&!value.IsSameIssuer(values[1])&&!value.IsSameIssuer(second),"Cohorts merged");
Require(!ReferenceEquals(Access.Cohort(value),Access.Cohort(independent))&&ReferenceEquals(second.InitialState,secondInitial),"Foreign equal-content owner substituted");guards.Add("same-ak-and-equal-root-independent-issuer");
var meta=value.ReadSourceMetadata();meta=meta with{Header=999,AggregateNamespace=99};Require(value.ReadSourceMetadata().Header==1,"Metadata value escaped");guards.Add("defensive-value-snapshots");
Require(initial.CopyAuxiliarySlots().Length==15&&initial.ReadAuxiliarySlot(0)is null&&!initial.CopyAuxiliarySlots().Any(s=>s.HasHistoryValue||s.HasSecondaryValue)&&initial.ReadInitialRootRow().Cost==3454,"Initial owner changed");guards.Add("fifteen-null-payload-slots-not-empty-aux");
Array.Fill(ak,(byte)0);Array.Fill(lk,(byte)0);GC.Collect();GC.WaitForPendingFinalizers();Require(value.ReadSourceMetadata().Header==1&&value.ReadConstructorState().HeaderPresent,"Caller bytes retained");guards.Add("caller-mutation-gc");
Parallel.For(0,1024,i=>Require(value.ReadSourceMetadata().AggregateNamespace==1&&!value.ReadConstructorState().ContextIssued&&ReferenceEquals(value.InitialState,initial),"Parallel immutable changed"));guards.Add("parallel-immutable-1024");
using var file=File.Create(args[1]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteNumber("schema",1);w.WriteNumber("logicalBytes",580473);w.WriteNumber("fixedWork",575901);w.WriteNumber("maxWork",969115);
w.WriteStartArray("cases");for(int i=0;i<values.Length;i++)
{
 var v=values[i];w.WriteStartObject();w.WriteNumber("index",i);w.WriteString("kind","procedural-owning-graph-not-word-label");w.WriteNumber("nodes",v.Graph.State.NodeCount);w.WriteNumber("groups",v.Graph.State.Groups.Count);w.WriteNumber("arcs",v.Graph.State.CandidateCount);w.WriteNumber("cap",v.Graph.State.MaximumAtoms);w.WriteNumber("work",v.WorkReservation);
 var md=v.ReadSourceMetadata();w.WriteStartArray("source");foreach(long x in new long[]{md.Header,BitConverter.SingleToInt32Bits(md.Weight24),BitConverter.SingleToInt32Bits(md.Weight28),md.SourceIndex,md.Priority,md.AcceptedSkipBase,md.AggregateNamespace})w.WriteNumberValue(x);w.WriteEndArray();
 var c=v.ReadConstructorState();w.WriteStartArray("constructor");w.WriteBooleanValue(c.Primary);w.WriteBooleanValue(c.HeaderPresent);w.WriteBooleanValue(c.TreeRootAbsent);w.WriteBooleanValue(c.ParentAbsent);foreach(int x in new[]{(int)c.State,c.Source,c.EnumerationFlags,c.Take,c.Skip})w.WriteNumberValue(x);w.WriteBooleanValue(c.CachedOwnerAbsent);w.WriteBooleanValue(c.ContextIssued);w.WriteEndArray();
 w.WriteBoolean("sameInitial",ReferenceEquals(v.InitialState,initial));w.WriteBoolean("sameGraph",ReferenceEquals(v.Graph,graphs[i]));w.WriteBoolean("sameSource",ReferenceEquals(Access.Source(v),graphs[i].Source.Source));w.WriteBoolean("hasIssuedQuery",v.HasIssuedQuery);w.WriteEndObject();
}
w.WriteEndArray();var rootRow=initial.ReadInitialRootRow();w.WriteStartArray("initialRootRow");foreach(long x in new long[]{rootRow.Target,rootRow.SourceLabel,rootRow.Primary,rootRow.Secondary,rootRow.Cost,rootRow.TargetSignedByte6,rootRow.TargetByte7})w.WriteNumberValue(x);w.WriteNullValue();w.WriteEndArray();w.WriteStartArray("guards");foreach(string g in guards)w.WriteStringValue(g);w.WriteEndArray();foreach(string name in new[]{"queryImplemented","historyImplemented","ready","fullrootDefault","numberOCR"})w.WriteBoolean(name,false);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS direct-number bootstrap {values.Length}genuine graphs/{guards.Count}guards; noissuedQ/history/OCR.");
static class Access
{
 [UnsafeAccessor(UnsafeAccessorKind.Field,Name="cohort")]internal static extern ref object Cohort(LegacyFst28CFreshPrimaryControllerBootstrap value);
 [UnsafeAccessor(UnsafeAccessorKind.Field,Name="sourceOwner")]internal static extern ref object Source(LegacyFst28CFreshPrimaryControllerBootstrap value);
}
