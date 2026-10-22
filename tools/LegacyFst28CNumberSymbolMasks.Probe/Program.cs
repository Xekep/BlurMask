using InkRecognition;
using System.Text.Json;

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
var initial=Initial();var graphs=patterns.Select(Graph).ToArray();
var bootstraps=graphs.Select(g=>LegacyFst28CFreshPrimaryControllerBootstrap.CreateDirectSelectedNumberFresh(initial,g,ak)).ToArray();
var values=bootstraps.Select(b=>LegacyFst28CNumberSymbolMasks.CreateActualDirectNumberFresh(b,ak)).ToArray();
for(int i=0;i<values.Length;i++)
{
 var v=values[i];Require(ReferenceEquals(v.Bootstrap,bootstraps[i])&&ReferenceEquals(v.InitialState,initial)&&ReferenceEquals(v.Graph,graphs[i])&&ReferenceEquals(v.Source,initial.TargetPreparation.PrimaryMap.Source)&&ReferenceEquals(v.Policy,initial.TargetPreparation),"Exact owning lineage differs");
 Require(!v.HasQueryAuthority&&!v.HasD0PermissionAuthority&&!v.Bootstrap.HasIssuedQuery,"Unexpected authority");
 Require(v.CopySourceTokens().SequenceEqual(Enumerable.Range(0,16).Select(i=>(uint)i)),"Ascending source enumeration differs");
}
guards.Add("exact-bootstrap-initial-source-policy-graph-identity");
Require(graphs.Distinct(ReferenceEqualityComparer.Instance).Count()==4&&bootstraps.Distinct(ReferenceEqualityComparer.Instance).Count()==4&&values.Skip(1).All(v=>!values[0].IsSameIssuer(v)&&!ReferenceEquals(v.Graph,values[0].Graph)),"Independent graph/bootstrap issuers merged");guards.Add("four-independent-graph-bootstrap-issuers");
var value=values[0];var bootstrap=bootstraps[0];
var exact=LegacyFst28CNumberSymbolMasks.CreateBounded(bootstrap,ak,595475,1160978);
Require(exact.CopySeed().SequenceEqual(value.CopySeed()),"Exact budgets refused");guards.Add("exact-whole-logical-and-work-budgets");
Refuse<ArgumentNullException>("null-bootstrap",()=>LegacyFst28CNumberSymbolMasks.CreateActualDirectNumberFresh(null!,ak));
Refuse<ArgumentException>("short-resource",()=>LegacyFst28CNumberSymbolMasks.CreateActualDirectNumberFresh(bootstrap,ak.AsSpan(1)));
Refuse<ArgumentException>("empty-resource",()=>LegacyFst28CNumberSymbolMasks.CreateActualDirectNumberFresh(bootstrap,[]));
foreach(int offset in new[]{0,99,5233,189742,189748,566380,572762})
{
 var bad=ak.ToArray();bad[offset]^=1;Refuse<ArgumentException>("wrong-resource-byte-"+offset,()=>LegacyFst28CNumberSymbolMasks.CreateActualDirectNumberFresh(bootstrap,bad));
}
foreach(int b in new[]{-1,595476,int.MaxValue})Refuse<ArgumentOutOfRangeException>("byte-domain-"+b,()=>LegacyFst28CNumberSymbolMasks.CreateBounded(bootstrap,ak,b,1160978));
foreach(int b in new[]{-1,1160979,int.MaxValue})Refuse<ArgumentOutOfRangeException>("work-domain-"+b,()=>LegacyFst28CNumberSymbolMasks.CreateBounded(bootstrap,ak,595475,b));
Refuse<InvalidDataException>("one-byte-short",()=>LegacyFst28CNumberSymbolMasks.CreateBounded(bootstrap,ak,595474,1160978));
Refuse<InvalidDataException>("one-work-short",()=>LegacyFst28CNumberSymbolMasks.CreateBounded(bootstrap,ak,595475,1160977));
Refuse<OperationCanceledException>("cancel-before-entry",()=>LegacyFst28CNumberSymbolMasks.CreateActualDirectNumberFresh(bootstrap,ak,canceled));
Refuse<OperationCanceledException>("cancel-before-invalid",()=>LegacyFst28CNumberSymbolMasks.CreateBounded(null!,[],-1,-1,canceled));
foreach(string stage in new[]{"before-snapshot","after-snapshot","after-hash","after-alphabet","after-seed","after-masks","before-publish"})
{
 using var cs=new CancellationTokenSource();bool reached=false;
 Refuse<OperationCanceledException>("cancel-stage-"+stage,()=>LegacyFst28CNumberSymbolMasks.CreateBounded(bootstrap,ak,595475,1160978,cs.Token,p=>{if(p==stage){reached=true;cs.Cancel();}}));Require(reached,"Checkpoint unreachable "+stage);
}
Refuse<OperationCanceledException>("cancel-copy-tokens",()=>value.CopySourceTokens(canceled));
Refuse<OperationCanceledException>("cancel-copy-preserved",()=>value.CopyPreservedMap(canceled));
Refuse<OperationCanceledException>("cancel-copy-seed",()=>value.CopySeed(canceled));
Refuse<OperationCanceledException>("cancel-copy-primary",()=>value.CopyPrimaryMask(canceled));
Refuse<OperationCanceledException>("cancel-copy-secondary",()=>value.CopySecondaryMask(canceled));
Refuse<InvalidOperationException>("throw-before-publish-no-owner",()=>LegacyFst28CNumberSymbolMasks.CreateBounded(bootstrap,ak,595475,1160978,default,p=>{if(p=="before-publish")throw new InvalidOperationException("test");}));
var independent=LegacyFst28CNumberSymbolMasks.CreateActualDirectNumberFresh(bootstrap,ak);
var other=LegacyFst28CNumberSymbolMasks.CreateActualDirectNumberFresh(LegacyFst28CFreshPrimaryControllerBootstrap.CreateDirectSelectedNumberFresh(Initial(),graphs[0],ak),ak);
Require(value.IsSameIssuer(value)&&!value.IsSameIssuer(independent)&&!value.IsSameIssuer(other)&&!ReferenceEquals(other.InitialState,initial),"Equal-content issuers merged");guards.Add("equal-content-foreign-initial-not-merged");
var input=ak.ToArray();var safe=LegacyFst28CNumberSymbolMasks.CreateBounded(bootstrap,input,595475,1160978,default,p=>{if(p=="after-snapshot")Array.Fill(input,(byte)0);});
Require(safe.CopySeed().SequenceEqual(value.CopySeed()),"Caller snapshot mutation affected owned result");guards.Add("after-snapshot-input-mutation");
var tokens=value.CopySourceTokens();Array.Fill(tokens,uint.MaxValue);var map=value.CopyPreservedMap();Array.Fill(map,(ushort)0);
var seed=value.CopySeed();Array.Fill(seed,(byte)9);var primary=value.CopyPrimaryMask();Array.Fill(primary,(byte)9);var secondary=value.CopySecondaryMask();Array.Fill(secondary,(byte)9);
Require(value.CopySourceTokens()[0]==0&&value.CopyPreservedMap()[0]==65533&&value.CopySeed().All(v=>v<=1)&&value.CopyPrimaryMask().All(v=>v<=1)&&value.CopySecondaryMask().All(v=>v<=1),"Mutable output escaped");guards.Add("all-five-output-copies-defensive");
Parallel.For(0,1024,i=>Require(value.CopySeed().SequenceEqual(safe.CopySeed())&&value.CopyPrimaryMask().SequenceEqual(safe.CopyPrimaryMask())&&ReferenceEquals(value.Graph,graphs[0]),"Parallel immutable changed"));guards.Add("parallel-1024-immutable-inspections");
Require(initial.CopyAuxiliarySlots().Length==15&&!initial.CopyAuxiliarySlots().Any(v=>v.HasHistoryValue||v.HasSecondaryValue)&&initial.ReadInitialRootRow().Cost==3454&&!bootstrap.HasIssuedQuery,"Source/initial mutated");guards.Add("source-initial-aux15-query-state-unchanged");
Array.Fill(ak,(byte)0);Array.Fill(lk,(byte)0);GC.Collect();GC.WaitForPendingFinalizers();Require(value.CopySeed().SequenceEqual(safe.CopySeed())&&value.CopyPrimaryMask().SequenceEqual(safe.CopyPrimaryMask()),"Caller retained");guards.Add("caller-mutation-gc-inspection");
using var file=File.Create(args[1]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteNumber("schema",1);w.WriteNumber("logicalBytes",595475);w.WriteNumber("work",1160978);
w.WriteStartArray("cases");for(int i=0;i<values.Length;i++)
{
 var v=values[i];w.WriteStartObject();w.WriteNumber("index",i);w.WriteString("kind","procedural-owning-graph-not-OCR");
 w.WriteNumber("nodes",v.Graph.State.NodeCount);w.WriteNumber("groups",v.Graph.State.Groups.Count);w.WriteNumber("arcs",v.Graph.State.CandidateCount);
 void Arr<T>(string name,IEnumerable<T> row){w.WriteStartArray(name);foreach(var x in row)w.WriteNumberValue(Convert.ToInt64(x));w.WriteEndArray();}
 Arr("sourceTokens",v.CopySourceTokens());Arr("preserved",v.CopyPreservedMap());Arr("seed",v.CopySeed());Arr("primary",v.CopyPrimaryMask());Arr("secondary",v.CopySecondaryMask());
 foreach(string key in new[]{"sameBootstrap","sameInitial","sameSource","sameGraph","samePolicy"})w.WriteBoolean(key,true);
 w.WriteBoolean("queryAuthority",v.HasQueryAuthority);w.WriteBoolean("d0PermissionAuthority",v.HasD0PermissionAuthority);w.WriteBoolean("bootstrapIssued",v.Bootstrap.HasIssuedQuery);w.WriteEndObject();
}
w.WriteEndArray();w.WriteStartArray("guards");foreach(string g in guards)w.WriteStringValue(g);w.WriteEndArray();foreach(string k in new[]{"queryImplemented","permissionRefreshImplemented","D0Implemented","numberOCR","fullrootDefault"})w.WriteBoolean(k,false);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS direct number source/M masks {values.Length}genuine graph associations/{guards.Count}guards; noD0/query/OCR.");
