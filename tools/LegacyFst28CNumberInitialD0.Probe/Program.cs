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
var guards=new List<string>();void Require(bool good,string name){if(!good)throw new Exception(name);guards.Add(name);}
void Refuse<T>(string name,Action action)where T:Exception{try{action();}catch(T){guards.Add(name);return;}throw new Exception("Missing refusal "+name);}
for(int i=0;i<owners.Length;i++)
{
 var v=owners[i];Require(ReferenceEquals(v.Masks,masks[i])&&ReferenceEquals(v.Graph,graphs[i])&&ReferenceEquals(v.Bootstrap,boots[i])&&ReferenceEquals(v.InitialState,initial)&&ReferenceEquals(v.Policy,policy)&&ReferenceEquals(v.Source,outgoing),"same-cohort-"+i);
 Require(!v.HasQueryAuthority&&!v.ReusedPreviousCache&&v.CurrentNode==0&&v.CurrentSpan==0&&v.AcousticEvaluations==v.CopySelectedGroups().Distinct().Count(),"initial-only-once-"+i);
 Refuse<InvalidOperationException>("one-shot-replay-"+i,()=>LegacyFst28CNumberInitialD0.CreateActualDirectNumberFresh(masks[i],ak));
}
Require(owners.Distinct(ReferenceEqualityComparer.Instance).Count()==4&&!owners[0].IsSameIssuer(owners[1])&&graphs.Distinct(ReferenceEqualityComparer.Instance).Count()==4,"four-real-foreign-graphs");
Refuse<ArgumentNullException>("null-masks",()=>LegacyFst28CNumberInitialD0.CreateActualDirectNumberFresh(null!,ak));
Refuse<ArgumentException>("empty-ak",()=>LegacyFst28CNumberInitialD0.CreateActualDirectNumberFresh(NewMasks(),[]));
Refuse<ArgumentException>("actual-foreign-english-ak",()=>LegacyFst28CNumberInitialD0.CreateActualDirectNumberFresh(NewMasks(),Read("en-US/en_US-ak-cur.lite.res")));
var retry=NewMasks();
foreach(int at in new[]{0,99,5233,189755,195871,195895,283697,367899,467101,566416,572762})
{var bad=ak.ToArray();bad[at]^=1;Refuse<ArgumentException>("resource-mutation-"+at,()=>LegacyFst28CNumberInitialD0.CreateActualDirectNumberFresh(retry,bad));}
Require(LegacyFst28CNumberInitialD0.CreateActualDirectNumberFresh(retry,ak).CopyPermissions().SequenceEqual(owners[0].CopyPermissions()),"auth-failure-did-not-claim");
var max=LegacyFst28CNumberInitialD0.CalculateReservations(65535,65535,262144,32,32,4096,4096,256,64,96);
Require(max==(32983513L,69123317896L),"pure-maximum-cardinalities");
Refuse<NotSupportedException>("pure-domain-excess",()=>LegacyFst28CNumberInitialD0.CalculateReservations(65536,1,1,1,1,1,1,1,1,0));
foreach(long b in new[]{-1L,32983514L,long.MaxValue})Refuse<ArgumentOutOfRangeException>("byte-domain-"+b,()=>LegacyFst28CNumberInitialD0.CreateBounded(NewMasks(),ak,b,69123317896));
foreach(long b in new[]{-1L,69123317897L,long.MaxValue})Refuse<ArgumentOutOfRangeException>("work-domain-"+b,()=>LegacyFst28CNumberInitialD0.CreateBounded(NewMasks(),ak,32983513,b));
long actualBytes=owners[0].LogicalByteReservation,actualWork=owners[0].WorkReservation;
Refuse<InvalidDataException>("one-byte-short",()=>LegacyFst28CNumberInitialD0.CreateBounded(NewMasks(),ak,actualBytes-1,actualWork));
Refuse<InvalidDataException>("one-work-short",()=>LegacyFst28CNumberInitialD0.CreateBounded(NewMasks(),ak,actualBytes,actualWork-1));
Require(LegacyFst28CNumberInitialD0.CreateBounded(NewMasks(),ak,actualBytes,actualWork).CopyEligibility().SequenceEqual(owners[0].CopyEligibility()),"exact-actual-envelope");
Refuse<OperationCanceledException>("cancel-before-entry",()=>LegacyFst28CNumberInitialD0.CreateActualDirectNumberFresh(null!,[],new(true)));
foreach(string stage in new[]{"before-snapshot","after-snapshot","after-hash","after-claim","after-model","after-context","before-group-0","after-group-0","before-permissions","before-refresh","before-publish"})
{
 var fresh=NewMasks();using var cs=new CancellationTokenSource();bool reached=false;
 Refuse<OperationCanceledException>("cancel-"+stage,()=>LegacyFst28CNumberInitialD0.CreateBounded(fresh,ak,32983513,69123317896,cs.Token,s=>{if(s==stage){reached=true;cs.Cancel();}}));
 Require(reached,"checkpoint-reached-"+stage);
 if(stage is not("before-snapshot" or "after-snapshot" or "after-hash"))
  Refuse<InvalidOperationException>("poisoned-claim-"+stage,()=>LegacyFst28CNumberInitialD0.CreateActualDirectNumberFresh(fresh,ak));
}
var fault=NewMasks();Refuse<ApplicationException>("fault-after-group",()=>LegacyFst28CNumberInitialD0.CreateBounded(fault,ak,32983513,69123317896,default,s=>{if(s=="after-group-0")throw new ApplicationException();}));
Refuse<InvalidOperationException>("fault-poisoned",()=>LegacyFst28CNumberInitialD0.CreateActualDirectNumberFresh(fault,ak));
var concurrent=NewMasks();int won=0,lost=0;Parallel.For(0,2,_=>{try{LegacyFst28CNumberInitialD0.CreateActualDirectNumberFresh(concurrent,ak);Interlocked.Increment(ref won);}catch(InvalidOperationException){Interlocked.Increment(ref lost);}});
Require(won==1&&lost==1,"one-concurrent-claim-winner");
var caller=ak.ToArray();var detached=LegacyFst28CNumberInitialD0.CreateBounded(NewMasks(),caller,32983513,69123317896,default,s=>{if(s=="after-snapshot")Array.Fill(caller,(byte)0);});
Require(detached.CopyPermissions().SequenceEqual(owners[0].CopyPermissions()),"authenticated-private-snapshot");
var selected=owners[0].CopySelectedGroups();selected[0]=-1;var permissionCopy=owners[0].CopyPermissions();permissionCopy[0]^=255;
var rowCopy=owners[0].CopyGroup(0);rowCopy.RawCosts[0]=-7;rowCopy.FinalCosts[0]=-8;rowCopy.Normalized[0][0]=new(32767,32767);rowCopy.FamilyProbabilities[0][0]=-5;
Require(owners[0].CopySelectedGroups()[0]>=0&&owners[0].CopyGroup(0).RawCosts[0]>=0&&owners[0].CopyGroup(0).FinalCosts[0]>=0&&owners[0].CopyGroup(0).FamilyProbabilities[0][0]>=0,"defensive-group-and-mask-copies");
var primaryCopy=owners[0].CopyCurrentPrimary();primaryCopy[0]=17;var eligibilityCopy=owners[0].CopyEligibility();eligibilityCopy[0]=17;var secondaryCopy=owners[0].CopyUnchangedSecondary();secondaryCopy[0]=new(0,17);
Require(owners[0].CopyPermissions().SequenceEqual(detached.CopyPermissions())&&owners[0].CopyCurrentPrimary().SequenceEqual(detached.CopyCurrentPrimary())&&owners[0].CopyEligibility().SequenceEqual(detached.CopyEligibility())&&owners[0].CopyUnchangedSecondary().SequenceEqual(detached.CopyUnchangedSecondary()),"all-detached-output-buffers-defensive");
Refuse<ArgumentOutOfRangeException>("group-minus-one",()=>owners[0].CopyGroup(-1));Refuse<OperationCanceledException>("inspection-cancel",()=>owners[0].CopyGroup(0,new(true)));
Parallel.For(0,128,_=>{if(!owners[0].CopyCurrentPrimary().SequenceEqual(detached.CopyCurrentPrimary()))throw new Exception("Inspection race");});guards.Add("parallel-detached-inspection");

using var file=File.Create(args[1]);using var w=new Utf8JsonWriter(file);
void I(string? name,IEnumerable<int> values){if(name is not null)w.WritePropertyName(name);w.WriteStartArray();foreach(int v in values)w.WriteNumberValue(v);w.WriteEndArray();}
void F(string? name,IEnumerable<float> values){if(name is not null)w.WritePropertyName(name);w.WriteStartArray();foreach(float v in values)w.WriteNumberValue(v);w.WriteEndArray();}
void R(string name,IEnumerable<LegacyInkRange> ranges){w.WriteStartArray(name);foreach(var r in ranges)I(null,[r.StrokeIndex,r.StartPoint,r.EndPointExclusive]);w.WriteEndArray();}
w.WriteStartObject();w.WriteNumber("schema",1);w.WriteString("policy","actual-direct-number-fresh-node0-span0-reusefalse");w.WriteBoolean("query",false);
w.WriteStartArray("guards");foreach(string guard in guards)w.WriteStringValue(guard);w.WriteEndArray();w.WriteStartArray("cases");
for(int i=0;i<owners.Length;i++)
{
 var owner=owners[i];var graph=graphs[i];var source=graph.Source.Source;var points=source.CopyStrokes();var chrono=source.Chronology;var groups=graph.State.Groups;
 w.WriteStartObject();w.WriteNumber("index",i);w.WriteNumber("bytes",owner.LogicalByteReservation);w.WriteNumber("work",owner.WorkReservation);I("selected",owner.CopySelectedGroups());w.WriteNumber("target",100);
 w.WriteStartObject("counts");w.WriteNumber("n",graph.State.NodeCount);w.WriteNumber("g",groups.Count);w.WriteNumber("a",Enumerable.Range(0,graph.State.NodeCount).Sum(n=>graph.State.GetNode(n).Count));w.WriteNumber("s",points.Count);w.WriteNumber("d",source.Steps.Count);w.WriteNumber("p0",source.Preparation.Input.Input.Input.Owner.Controller.Source.Strokes.Sum(s=>s.Points.Count));w.WriteNumber("p",points.Sum(s=>s.Points.Count));w.WriteEndObject();
 w.WriteStartArray("points");foreach(var stroke in points){w.WriteStartArray();foreach(var p in stroke.Points)F(null,[p.X,p.Y]);w.WriteEndArray();}w.WriteEndArray();
 var original=source.Preparation.Input.Input.Input.Owner.Controller.Source;
 F("sourceProbabilities",original.SourceStrokeProbabilities.ToArray());
 w.WriteStartArray("pointKeys");for(int physical=0;physical<points.Count;physical++){w.WriteStartArray();for(int p=0;p<points[physical].Points.Count;p++){var key=chrono.Resolve(physical,p).Key;I(null,[key.First,key.Second,key.Third]);}w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("endingKeys");for(int physical=0;physical<points.Count;physical++){w.WriteStartArray();for(int p=0;p<points[physical].Points.Count;p++){var key=original.ResolveEndingMetadata(chrono.Resolve(physical,p)).Key;I(null,[key.First,key.Second,key.Third]);}w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("descriptors");foreach(var d in source.Steps.Where(s=>!s.IsPendingSeparator)){w.WriteStartObject();w.WriteNumber("ordinal",d.DescriptorOrdinal);w.WriteNumber("line",d.LineId);w.WriteNumber("first",d.FirstPhysical);w.WriteNumber("last",d.LastPhysical);w.WriteNumber("baseline",d.Baseline);w.WriteNumber("height",d.Height);w.WriteEndObject();}w.WriteEndArray();
 w.WriteStartArray("allGroups");foreach(var g in groups){w.WriteStartObject();w.WriteNumber("id",g.AllocationId);w.WriteNumber("descriptor",g.DescriptorOrdinal);w.WriteNumber("secondary",g.SecondaryDescriptorOrdinal);w.WriteNumber("kind",g.Kind07);R("ranges",g.CopyRanges());w.WriteEndObject();}w.WriteEndArray();
 w.WriteStartArray("arcs");foreach(var a in graph.State.GetNode(0)){w.WriteStartObject();w.WriteNumber("group",a.GroupId);w.WriteNumber("span",a.Span);w.WriteEndObject();}w.WriteEndArray();
 I("mask",masks[i].CopyPrimaryMask().Select(v=>(int)v));I("secondaryMask",masks[i].CopySecondaryMask().Select(v=>(int)v));I("permissions",owner.CopyPermissions().Select(v=>(int)v));I("eligibility",owner.CopyEligibility().Select(v=>(int)v));I("primary",owner.CopyCurrentPrimary().Select(v=>(int)v));I("secondary",owner.CopyUnchangedSecondary().Select(v=>(int)v.Code));
 w.WriteStartArray("rows");for(int j=0;j<owner.AcousticEvaluations;j++)
 {
  var row=owner.CopyGroup(j);var actual=groups[row.Group];var frame=graph.ResolveGroupSource(row.Group);
  w.WriteStartObject();w.WriteNumber("group",row.Group);w.WriteNumber("descriptor",actual.DescriptorOrdinal);w.WriteNumber("secondary",actual.SecondaryDescriptorOrdinal);w.WriteNumber("minX",actual.Bounds.MinimumX);w.WriteNumber("baseline",row.Frame.Baseline);w.WriteNumber("height",row.Frame.Height);w.WriteNumber("primaryBaseline",frame.Primary.Baseline);w.WriteNumber("primaryHeight",frame.Primary.Height);
  R("ranges",row.Ranges);R("storedRanges",actual.CopyRanges());w.WriteStartArray("startKeys");foreach(var r in row.Ranges){var k=chrono.Resolve(r.StrokeIndex,r.StartPoint).Key;I(null,[k.First,k.Second,k.Third]);}w.WriteEndArray();
  w.WriteStartArray("normalized");foreach(var stroke in row.Normalized){w.WriteStartArray();foreach(var p in stroke)I(null,[p.X,p.Y]);w.WriteEndArray();}w.WriteEndArray();
  //Diagnostic replay of the exact public numeric helpers; these are not intercepted private NN input buffers.
  var glyph=row.Normalized.Select(s=>new LegacyInkStroke(s)).ToArray();
  var directional=LegacySaturatedDirectionalFeatures.Extract(glyph,row.Baseline,LegacyDirectional2B1DefaultCalibration.CopyMinimum(false),LegacyDirectional2B1DefaultCalibration.CopyMaximum(false),LegacyDirectional2B1DefaultCalibration.CopyMinimum(true),LegacyDirectional2B1DefaultCalibration.CopyMaximum(true),row.Context,row.StartBoundary,row.EndBoundary);
  short[][] inputs=[LegacyStrokeFeatures.ExtractInputWithBaseline(glyph,row.Baseline,row.Context,row.StartBoundary,row.EndBoundary),LegacyRasterFeatures.ExtractInputWithBaseline(glyph,row.Baseline,row.Context,row.StartBoundary,row.EndBoundary),directional.Temporal,directional.Spatial];
  w.WriteStartArray("recomputedInputs");foreach(var vector in inputs)I(null,vector.Select(v=>(int)v));w.WriteEndArray();
  I("baselineShort",[row.Baseline.Y,row.Baseline.Height]);I("context",row.Context.Select(v=>(int)v));I("boundaries",[row.StartBoundary,row.EndBoundary]);
  w.WriteStartArray("probabilities");foreach(var family in row.FamilyProbabilities)I(null,family);w.WriteEndArray();I("integerCosts",row.RawCosts);I("cacheCosts",row.FinalCosts);F("weights",row.Weights);F("dynamic",[row.Dynamic.Overlap,row.Dynamic.Mode1Weight,row.Dynamic.Mode2Weight]);w.WriteNumber("dynamicPoints",row.Dynamic.ResampledPointCount);
  w.WriteStartArray("candidates");foreach(var c in row.Candidates)I(null,[c.Symbol,c.Cost]);w.WriteEndArray();w.WriteBoolean("whole",row.Primitive.BoundaryAlignedSingleNormalizedStroke);w.WriteBoolean("zero",row.Primitive.EndpointFieldsAreZero);if(row.Primitive.SourceStrokeProbability is{} probability)w.WriteNumber("probability",probability);else w.WriteNull("probability");w.WriteEndObject();
 }
 w.WriteEndArray();w.WriteEndObject();
}
w.WriteEndArray();w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS {owners.Length} actual D0 owners/{owners.Sum(v=>v.AcousticEvaluations)} selected groups/{guards.Count} guards; no issued query.");
