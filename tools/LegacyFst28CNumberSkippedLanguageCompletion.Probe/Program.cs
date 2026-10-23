using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
using System.Text.Json.Serialization;
if(args.Length!=2)throw new ArgumentException("MODELS CAPTURE");var root=args[0];var capturePath=args[1];byte[] Read(string path)=>File.ReadAllBytes(Path.Combine(root,path));
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

// Source only until root authorizes compilation AND execution. No labels or
// copied historical candidates authorize these actual factories.
var contexts=owners.Select(d=>LegacyFst28CNumberInitialConsumerContext.CreateFreshInitialContext(d)).ToArray();
var consumers=contexts.Select(c=>LegacyFst28CNumberInitialConsumer.CreateActualDirectNumberInitialConsumer(c,ak)).ToArray();
var guards=new List<string>();var exceptions=new List<ExceptionDto>();
void Require(bool b,string name){if(!b)throw new Exception(name);guards.Add(name);}
void Refuse<T>(string name,Action action)where T:Exception
{try{action();}catch(T e){if(e.GetType()!=typeof(T))throw;guards.Add(name);return;}throw new Exception("Missing refusal "+name);}
void Cancel(string name,CancellationToken expected,Action action,Func<bool>? reached=null)
{
 try{action();}catch(OperationCanceledException e)
 {bool hit=reached?.Invoke()??true;if(e.GetType()!=typeof(OperationCanceledException)||e.CancellationToken!=expected||!expected.IsCancellationRequested||!hit)throw new Exception("Wrong cancellation "+name,e);
  guards.Add(name);exceptions.Add(new(name,e.GetType().FullName!,"",hit,true,true));return;}
 throw new Exception("Missing cancellation "+name);
}
LegacyFst28CNumberSkippedLanguageCompletion Run(LegacyFst28CNumberInitialConsumer c,
 int bytes=LegacyFst28CNumberSkippedLanguageCompletion.LogicalBytes,int work=LegacyFst28CNumberSkippedLanguageCompletion.Work,
 CancellationToken token=default,Action<string>? cp=null)
 =>LegacyFst28CNumberSkippedLanguageCompletion.CreateBounded(c,bytes,work,token,cp);
Refuse<ArgumentNullException>("null-consumer",()=>Run(null!));
var cancelled=new CancellationToken(true);Cancel("cancel-before-null",cancelled,()=>Run(null!,token:cancelled));
foreach(int n in new[]{-1,2097153,int.MaxValue})Refuse<ArgumentOutOfRangeException>("byte-domain-"+n,()=>Run(consumers[0],bytes:n));
foreach(int n in new[]{-1,67108865,int.MaxValue})Refuse<ArgumentOutOfRangeException>("work-domain-"+n,()=>Run(consumers[0],work:n));
Refuse<InvalidDataException>("one-byte-short",()=>Run(consumers[0],bytes:1211567));
Refuse<InvalidDataException>("one-work-short",()=>Run(consumers[0],work:61261519));
Refuse<ArgumentException>("foreign-genuine-consumer",()=>LegacyFst28CNumberSkippedLanguageCompletion.ValidateAssociation(consumers[0],consumers[2]));
using(var cts=new CancellationTokenSource())
{bool hit=false;Cancel("cancel-before-claim",cts.Token,()=>Run(consumers[0],token:cts.Token,cp:s=>{if(s=="before-claim"){hit=true;cts.Cancel();}}),()=>hit);}
Require(!LegacyFst28CNumberSkippedLanguageCompletion.ReadClaimDiagnostic(consumers[0]),"preclaim-cancel-no-claim");
string[] Records(LegacyFst28CNumberInitialConsumer c)=>Enumerable.Range(0,c.CandidateCount).Select(i=>Convert.ToHexString(c.CopyInitializedHeapRecord(i))).ToArray();
ArcDto[][] Lookahead(int i)=>Enumerable.Range(1,Math.Min(2,graphs[i].State.NodeCount-1)).Select(n=>graphs[i].State.GetNode(n).Select((a,k)=>new ArcDto(n,k,a.Span,a.Flags)).ToArray()).ToArray();
// Diagnostic-only scalar export admission, not production reservations/RSS.
const int IdentityDiagnosticBytes=16*1024,IdentityDiagnosticWork=64*1024;
if(4*26*(51+16)>IdentityDiagnosticBytes||4*26*(51+16)*4>IdentityDiagnosticWork)throw new InvalidDataException("Context diagnostic envelope");
var cases=new List<CaseDto>();
for(int i=0;i<4;i++)
{
 var before=Records(consumers[i]);var typedNodes=contexts[i].ReadActiveContextNodes();
 if(typedNodes.Count!=26)throw new InvalidDataException("Exact current26-node diagnostic context required.");
 var beforeNodes=typedNodes.ToArray();
 // Export actual identities and all symbolic fields; never rewrite owned rows.
 var identityMap=beforeNodes.Select((n,k)=>new ContextIdentityDto(n.Identity,(ulong)k+1)).ToArray();
 var contextNodes=beforeNodes.Select(n=>new ContextNodeDto(n.Identity,n.Symbol,n.Left,n.Right,n.Balance,n.ParentLink,n.ChildTreeRoot,n.PageIndex,n.PageSlot)).ToArray();
 LegacyFst28CNumberSkippedLanguageCompletion? result=null;bool refused=false;
 try{result=Run(consumers[i]);}catch(NotSupportedException e)
 {if(e.Message!="Actual number node0 requires the separate language branch.")throw;refused=true;guards.Add("native-language-required-"+i);
  Require(!LegacyFst28CNumberSkippedLanguageCompletion.ReadClaimDiagnostic(consumers[i]),"native-language-preclaim-no-claim-"+i);}
 LegacyFst28CNumberCompletionSnapshot? snapshot=null;int[] association=[];
 if(result is not null)
 {
  snapshot=result.CopySnapshot();association=result.ReadAssociation();
  Require(!result.HasLiveQueryAuthority&&!result.HasParentHistoryAuthority&&!result.HasNode1Authority&&!result.HasCompleteController,"restricted-owner-"+i);
  Require(snapshot.Phase1Count==0&&snapshot.Phase1Capacity==124&&snapshot.AllocatedCapacity==250&&snapshot.SourceEnd==125&&snapshot.RetainedEnd==125,"complete-skipped-tail-"+i);
  Refuse<InvalidOperationException>("same-consumer-replay-"+i,()=>Run(consumers[i]));
  var copies=new[]{snapshot.Stored,snapshot.Primary,snapshot.Secondary,snapshot.StoredDefined,snapshot.PrimaryDefined,snapshot.SecondaryDefined};
  foreach(var a in copies)if(a.Length!=0)a[0]^=255;
  var next=result.CopySnapshot();Require(next.Stored[0]!=(snapshot.Stored[0])&&next.Primary[0]!=(snapshot.Primary[0])&&next.Secondary[0]!=(snapshot.Secondary[0])&&
    next.StoredDefined[0]!=snapshot.StoredDefined[0]&&next.PrimaryDefined[0]!=snapshot.PrimaryDefined[0]&&next.SecondaryDefined[0]!=snapshot.SecondaryDefined[0],"actual-returned-buffer-mutations-"+i);
  snapshot=next;Cancel("canceled-inspection-"+i,cancelled,()=>result.CopySnapshot(cancelled));
  var reads=Task.WhenAll(Task.Run(()=>result.CopySnapshot()),Task.Run(()=>result.CopySnapshot())).GetAwaiter().GetResult();
  Require(reads[0].Stored.SequenceEqual(reads[1].Stored)&&reads[0].StoredDefined.SequenceEqual(reads[1].StoredDefined),"parallel-detached-inspection-"+i);
 }
 Require(before.SequenceEqual(Records(consumers[i]))&&beforeNodes.SequenceEqual(contexts[i].ReadActiveContextNodes()),"old-consumer-and-context-unchanged-"+i);
 cases.Add(new(i,before,identityMap,contextNodes,Lookahead(i),owners[i].CopyCurrentPrimary(),owners[i].CopyPermissions(),owners[i].CopyEligibility(),refused,snapshot,association));
}
// Minimal extra owners are proposal-only until separately authorized. Same
// graph0/initial/AK, but a genuine fresh Bootstrap/header/D0/context/consumer.
int ContextCount(LegacyFreshMainCompactOrdinaryGraph graph)
{int total=0;foreach(var s in graph.Source.Source.Steps){if(s.IsPendingSeparator)continue;int k=s.LastPhysical-s.FirstPhysical;total+=k==0?0:k==1?1:3*k-3;}return total;}
int extras=0,extraGlyph=0,extraContext=0;
LegacyFst28CNumberInitialConsumer Extra()
{
 var b=LegacyFst28CFreshPrimaryControllerBootstrap.CreateDirectSelectedNumberFresh(initial,graphs[0],ak);
 var m=LegacyFst28CNumberSymbolMasks.CreateActualDirectNumberFresh(b,ak);
 var d=LegacyFst28CNumberInitialD0.CreateActualDirectNumberFresh(m,ak);extras++;extraGlyph+=d.AcousticEvaluations*4;extraContext+=ContextCount(graphs[0]);
 return LegacyFst28CNumberInitialConsumer.CreateActualDirectNumberInitialConsumer(LegacyFst28CNumberInitialConsumerContext.CreateFreshInitialContext(d),ak);
}
var stages=new[]{"after-claim","before-root-alternatives","before-sort","before-merge","before-heap-rebuild","before-materialize","before-phase1-reset","before-common-tail","before-logical-source-reset","before-publish"};
for(int k=0;k<stages.Length;k++)
{
 var extra=Extra();
 if(k==0)
 {
  Require(ReferenceEquals(extra.D0.Graph,consumers[0].D0.Graph)&&!ReferenceEquals(extra,consumers[0])&&!ReferenceEquals(extra.Context,consumers[0].Context)&&!ReferenceEquals(extra.D0.Bootstrap,consumers[0].D0.Bootstrap),"equal-data-foreign-genuine-owner");
  Refuse<ArgumentException>("equal-data-receiver-refusal",()=>LegacyFst28CNumberSkippedLanguageCompletion.ValidateAssociation(consumers[0],extra));
 }
 using var cts=new CancellationTokenSource();bool hit=false;string stage=stages[k];
 Cancel("cancel-"+stage,cts.Token,()=>Run(extra,token:cts.Token,cp:s=>{if(s==stage){hit=true;cts.Cancel();}}),()=>hit);
 Refuse<InvalidOperationException>("poison-after-cancel-"+stage,()=>Run(extra));
}
var faultOwner=Extra();bool faultReached=false;
try{Run(faultOwner,cp:s=>{if(s=="before-common-tail"){faultReached=true;throw new IOException("bounded diagnostic fault");}});throw new Exception("Missing injected fault");}
catch(IOException e)
{if(e.GetType()!=typeof(IOException)||e.Message!="bounded diagnostic fault"||!faultReached)throw;guards.Add("exact-postclaim-fault");exceptions.Add(new("exact-postclaim-fault",e.GetType().FullName!,e.Message,true,false,false));}
Refuse<InvalidOperationException>("poison-after-fault",()=>Run(faultOwner));
var raced=Extra();RaceDto race;
using(var barrier=new Barrier(2))
{
 Task<RaceAttemptDto> Attempt()=>Task.Run(()=>{
  bool before=false,after=false;
  try{Run(raced,cp:s=>{if(s=="before-claim"){before=true;if(!barrier.SignalAndWait(TimeSpan.FromSeconds(20)))throw new Exception("barrier timeout");}if(s=="after-claim")after=true;});return new RaceAttemptDto(true,before,after,"","");}
  catch(InvalidOperationException e){if(e.GetType()!=typeof(InvalidOperationException)||e.Message!="This exact initial Number consumer already claimed node0 completion."||!before||after)throw;return new RaceAttemptDto(false,before,after,e.GetType().FullName!,e.Message);}
 });
 var a=Attempt();var b=Attempt();var values=Task.WhenAll(a,b).GetAwaiter().GetResult();
 Require(values.Count(v=>v.Won)==1&&values.All(v=>v.BeforeClaim)&&values.Count(v=>v.AfterClaim)==1,"same-consumer-actual-claim-race");race=new(values.OrderByDescending(v=>v.Won).ToArray());
}
var prepared=new List<PreparedDto>();
var merges=new List<MergeDto>();
void W(byte[] b,int o,long v,int n=4){for(int k=0;k<n;k++)b[o+k]=(byte)(v>>(8*k));}
void InitRootMask(byte[] m,bool primary)
{for(int i=0;i<2;i++){int at=i*80;m.AsSpan(at,27).Fill(255);m[at+30]=255;m[at+31]=(byte)(primary?63:31);m.AsSpan(at+40,8).Fill(255);m[at+70]=255;}}
foreach(bool first in new[]{false,true})foreach(string mode in new[]{"native-unknown","prepared-partial","prepared-known"})
{
 var rows=new byte[250*128];var rm=new byte[rows.Length];rm.AsSpan(0,120).Fill(255);rm.AsSpan(128,120).Fill(255);
 W(rows,88,0);W(rows,128+88,1);W(rows,16,10);W(rows,128+16,20);
 var p=new byte[250*80];var pm=new byte[p.Length];InitRootMask(pm,first);
 for(int i=0;i<2;i++){int at=i*80;W(p,at,10+i*10);W(p,at+4,2+i);W(p,at+8,100+i,8);W(p,at+16,200+i,8);p[at+31]=(byte)(first?32:0);W(p,at+40,0x4000000);W(p,at+44,0x4000000);new byte[]{17,34,51,68}.AsSpan().CopyTo(p.AsSpan(at+32));}
 pm[71]=(byte)(first?128:160);pm.AsSpan(72,4).Fill(255);
 byte[] suffix=mode=="native-unknown"?[0,0,0,0]:mode=="prepared-partial"?[240,15,170,85]:[255,255,255,255];suffix.CopyTo(pm,112);pm[111]=(byte)(first?37:17);
 merges.Add(new((first?"primary":"secondary")+"-FIRST-to-SECOND-"+mode,LegacyFst28CNumberSkippedLanguageCompletion.RunPreparedMerge(rows,rm,p,pm,first)));
}
{
 var rows=new byte[250*128];var rm=new byte[rows.Length];rm.AsSpan(0,120).Fill(255);rm.AsSpan(128,128).Fill(255);W(rows,88,0);W(rows,128+88,1);
 var p=new byte[250*80];var pm=new byte[p.Length];InitRootMask(pm,false);W(p,0,20);W(p,80,10);W(p,8,1,8);W(p,88,2,8);W(p,40,0x4000000);W(p,120,0x4000000);
 merges.Add(new("secondary-full-slot-swap-before-narrow-writes",LegacyFst28CNumberSkippedLanguageCompletion.RunPreparedMerge(rows,rm,p,pm,false)));
}
foreach(int n in new[]{0,1,125,126,250})foreach(bool duplicate in new[]{false,true})
{
 var rows=new byte[250*128];var rm=Enumerable.Repeat((byte)255,rows.Length).ToArray();var p=new byte[250*80];var q=new byte[p.Length];var pm=Enumerable.Repeat((byte)255,p.Length).ToArray();
 for(int i=0;i<n;i++)
 {
  int group=duplicate?i/3:i,at=i*128;W(rows,at,group+1,8);W(rows,at+36,group%192,2);W(rows,at+16,1000-i);W(rows,at+20,-i);W(rows,at+60,65535,2);W(rows,at+62,65535,2);W(rows,at+88,i);
  for(int mode=0;mode<2;mode++)for(int j=0;j<2;j++)
  {var pool=mode==0?p:q;int x=i*80+j*40;W(pool,x,1000-i+j*5);W(pool,x+4,-i-j);W(pool,x+8,i*2+j,8);W(pool,x+16,i*2+j+1000,8);pool[x+31]=(byte)(mode==0?32:0);W(pool,x+32,65535,2);}
 }
 var snapshot=LegacyFst28CNumberSkippedLanguageCompletion.RunPreparedKernel(rows,rm,n,p,pm,q,pm);
 prepared.Add(new("prepared-"+n+"-"+(duplicate?"duplicate":"unique"),snapshot));
}
var cleanups=new List<PreparedDto>();
foreach(string mode in new[]{"known-write","unknown-flag-write","unknown-parent-write","partial-cost-write","unknown-flag-no-write","known-flag-skip","known-parent-skip","known-cost-skip"})
{
 var rows=new byte[250*128];var rm=Enumerable.Repeat((byte)255,rows.Length).ToArray();
 var p=new byte[250*80];var q=new byte[p.Length];var pm=Enumerable.Repeat((byte)255,p.Length).ToArray();var qm=pm.ToArray();
 W(rows,16,10);W(rows,60,65535,2);W(rows,62,65535,2);
 W(p,0,10);W(p,24,125,2);W(p,40,0x4000000);W(q,0,0x4000000);W(q,40,0x4000000);
 if(mode=="unknown-flag-write"||mode=="unknown-flag-no-write")pm[31]&=223;
 if(mode=="unknown-parent-write")pm[24]=0;
 if(mode=="partial-cost-write")pm[0]=240;
 if(mode=="unknown-flag-no-write"||mode=="known-flag-skip")p[31]=32;
 if(mode=="known-parent-skip")W(p,24,0,2);
 if(mode=="known-cost-skip")W(p,0,0x4000000);
 cleanups.Add(new("cleanup-"+mode,LegacyFst28CNumberSkippedLanguageCompletion.RunPreparedKernel(rows,rm,1,p,pm,q,qm)));
}
var capture=new CaptureDto("number-skipped-language-node0-v1",false,false,false,false,1211568,61261520,
 owners.Sum(o=>o.AcousticEvaluations)*4,graphs.Sum(ContextCount),extras,extraGlyph,extraContext,IdentityDiagnosticBytes,IdentityDiagnosticWork,guards.ToArray(),exceptions.ToArray(),cases.ToArray(),prepared.ToArray(),merges.ToArray(),cleanups.ToArray(),race);
File.WriteAllText(capturePath,JsonSerializer.Serialize(capture,ProbeJsonContext.Default.CaptureDto));
Console.WriteLine($"PASS scoped Number node0;{cases.Count(c=>!c.Refused)} completions/{cases.Count(c=>c.Refused)} preclaim refusals/{guards.Count}guards;19canonicalNN/{extraGlyph}extra glyphNN; no live query/controller");

record ArcDto(int Node,int Ordinal,byte Span,byte Flags);
record ContextIdentityDto(ulong Raw,ulong Canonical);
record ContextNodeDto(ulong Identity,ushort Symbol,ulong Left,ulong Right,sbyte Balance,ulong ParentLink,ulong ChildTreeRoot,int PageIndex,int PageSlot);
record CaseDto(int Index,string[] InitialRecords,ContextIdentityDto[] ContextIdentityMap,ContextNodeDto[] ContextNodes,ArcDto[][] Lookahead,ushort[] Current,byte[] Permissions,byte[] Eligibility,
 bool Refused,LegacyFst28CNumberCompletionSnapshot? Result,int[] Association);
record ExceptionDto(string Name,string Type,string Message,bool Reached,bool TokenMatched,bool TokenCanceled);
record PreparedDto(string Name,LegacyFst28CNumberCompletionSnapshot Result);
record MergeDto(string Name,LegacyFst28CNumberPreparedMergeSnapshot Result);
record RaceAttemptDto(bool Won,bool BeforeClaim,bool AfterClaim,string Type,string Message);
record RaceDto(RaceAttemptDto[] Attempts);
record CaptureDto(string Schema,bool LiveQuery,bool Node1,bool ParentHistory,bool CompleteController,int Bytes,int Work,
 int CanonicalGlyphNN,int CanonicalContextNN,int ExtraOwners,int ExtraGlyphNN,int ExtraContextNN,int IdentityDiagnosticBytes,int IdentityDiagnosticWork,string[] Guards,
 ExceptionDto[] Exceptions,CaseDto[] Cases,PreparedDto[] Prepared,MergeDto[] Merges,PreparedDto[] Cleanups,RaceDto Race);
[JsonSourceGenerationOptions(PropertyNamingPolicy=JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(CaptureDto))]
partial class ProbeJsonContext:JsonSerializerContext{}
