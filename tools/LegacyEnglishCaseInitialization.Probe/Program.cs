using System.Text.Json;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using InkRecognition;
if(args.Length!=3)throw new ArgumentException("models blind-raw capture");
var m=args[0];var bytes=File.ReadAllBytes(Path.Combine(m,"en-US/en_US-ak-cur.lite.res"));var layout=LegacySource247WireLayout.Read(bytes);
var model=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(bytes,layout,layout.Children.First(c=>c.Kind==0x249));
var lk=File.ReadAllBytes(Path.Combine(m,"en-US/en_US-lk-text.lite.res"));
var packed=LegacyEnglishLkPackedData.Read(layout,lk);var language=LegacyEnglishLkSparseLanguageData.Read(packed,lk);var numeric=LegacyEnglishLkFreshNumericBinding.Create(language);
var m30=File.ReadAllBytes(Path.Combine(m,"line-geometry/line-2e3-30.res"));var m50=File.ReadAllBytes(Path.Combine(m,"line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(m,"ru-RU/legacy-separation-profile.json")));
using var cf=File.OpenRead(Path.Combine(m,"text-unicode/legacy-case-data.json"));using var pf=File.OpenRead(Path.Combine(m,"text-unicode/legacy-text-publication-data.json"));var caseData=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
using var inputs=JsonDocument.Parse(File.ReadAllBytes(args[1]));var rows=new List<object>();var refused=new List<string>();int guards=0,cancels=0;
InkStroke[] Ink(JsonElement e)=>e.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
LegacyEnglish119PlainTextResult Recognize(LegacyEnglish119SourceSubmission s)
{var c=LegacyOwnedEnglish119Candidates.Prepare(s.Preparation,table);var cache=LegacyOwnedEnglish119AcousticCache.Prepare(c);var confidence=LegacyOwnedEnglish119GlyphConfidence.Prepare(cache);var costs=LegacyOwnedEnglish119ControllerCosts.Prepare(confidence,table);var primary=LegacyOwnedEnglish119FirstPrimary.Prepare(costs,table).Run();var contexts=LegacyOwnedEnglish119ContextPasses.Prepare(primary).Run();return LegacyOwnedEnglish119PlainText.Materialize(LegacyOwnedEnglish119Terminal.Select(contexts),new(new byte[120],new byte[40]));}
LegacyEnglish119ParentPostprocessing Post(JsonElement item,LegacyFirstAcoustic119WireModel? selected=null)
{var p=LegacyEnglish119ParentSubmission.Prepare(selected??model,Ink(item.GetProperty("Raw")),item.GetProperty("Resolution").GetSingle(),m30,m50);p.Publish(p.Children.Select(c=>Recognize(c.Submission)).ToArray());return LegacyEnglish119ParentPostprocessing.Prepare(p,caseData,publication).Run();}
string SourceDigest(LegacyEnglish119FinalizedSingleSource f)
{using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);foreach(var r in f.CopyRows()){hash.AppendData(r.CopyStorage().Span);foreach(var x in r.Ranges)foreach(int v in new[]{x.First.First,x.First.Second,x.First.Third,x.Last.First,x.Last.Second,x.Last.Third})hash.AppendData(BitConverter.GetBytes(v));}return Convert.ToHexString(hash.GetHashAndReset());}

var guardNames=new List<string>();var cancelNames=new List<string>();bool tested=false;object? numericDump=null;
void Guard(string name,Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException or InvalidDataException){guards++;guardNames.Add(name);return;}throw new Exception("Expected strict refusal: "+name);}
void Assert(bool value,string message){if(!value)throw new Exception(message);}
object Snapshot(LegacyEnglishCaseInitializationSnapshot s)
{
 var before=s.CopyInitialNodes();var post=s.CopyPostControlNodes();var ids=post.Select((n,i)=>(n.Identity,Ordinal:i+1)).ToDictionary(x=>x.Identity,x=>x.Ordinal);long Id(ulong raw)=>raw==0?0:ids[raw];
 object Nodes(LegacySymbolContextNode[] nodes)=>nodes.Select(n=>new long[]{Id(n.Identity),Id(n.ParentLink),Id(n.ChildTreeRoot),Id(n.Left),Id(n.Right),n.Symbol,n.Balance,n.PageIndex,n.PageSlot}).ToArray();
 return D(("Initial",Nodes(before)),("Post",Nodes(post)),("InitialRoot",Id(s.InitialRootIdentity)),("PostRoot",Id(s.PostControlRootIdentity)),("InitialPages",s.CopyInitialPages()),("PostPages",s.CopyPostControlPages()),("InitialPageCapacity",s.InitialPageCapacity),("InitialCursor",s.InitialPageCursor),("PostPageCapacity",s.PostControlPageCapacity),("PostCursor",s.PostControlPageCursor),("Preserved",s.CopyPreservedMap()),("Auxiliary",s.CopyAuxiliaryMap()),("Source68",s.CopyInitialSource68().Select(Id).ToArray()),("Ordinary",s.CopyClassifications()),("Alternate",s.CopyClassifications(true)),("Controls",s.CopyControls().Select(c=>new long[]{c.Symbol,Id(c.Identity),c.Created?1:0}).ToArray()),("ContextCapacity",s.InitialContextCapacity),("SourceKind",s.SourceKind),("CoordinateMode",s.CoordinateMode),("CaseVersion",s.CasePathVersion),("Descriptor",s.DescriptorFlags),("Priority",s.SelectedPriority));
}
var actualItems=inputs.RootElement.ValueKind==JsonValueKind.Array?inputs.RootElement.EnumerateArray().ToArray():inputs.RootElement.GetProperty("samples").EnumerateArray().ToArray();
foreach(var original in actualItems)
{
 JsonElement item=original;string name;
 if(inputs.RootElement.ValueKind==JsonValueKind.Array)name=item.GetProperty("Name").GetString()!;
 else{name=original.GetProperty("id").GetString()!;using var transformed=JsonDocument.Parse(Encode(D(("Name",name),("Raw",original.GetProperty("strokes").EnumerateArray().Select(s=>s.EnumerateArray().Select(p=>new float[]{p[0].GetSingle(),p[1].GetSingle()}).ToArray()).ToArray()),("Resolution",0))));item=transformed.RootElement.Clone();}
 var post=Post(item);if(post.CopySnapshots()[0].CopyDescriptors().Length!=1){refused.Add(name);continue;}
 var final=LegacyEnglish119FinalizedSingleSource.Prepare(post).Run();var local=LegacyEnglish119MainLocalCandidates.Prepare(final,table).Run();var knowledge=LegacyEnglish119MainKnowledge.Create(local);
 LegacyEnglishCaseQueryInitialization Fresh(long limit=3L*1024*1024)=>LegacyEnglishCaseQueryInitialization.Prepare(knowledge,numeric,caseData,publication,limit);
 var value=Fresh();var sourceBefore=SourceDigest(final);var snapshot=value.CopySnapshot();var root=value.RootForAudit;
 if(!tested)
 {
  tested=true;
  var publicNames=typeof(LegacyEnglishCaseQueryInitialization).GetMethods(System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.DeclaredOnly).Select(x=>x.Name).Order().ToArray();
  Assert(publicNames.SequenceEqual(new[]{"CopySnapshot","Prepare","get_AdditionalLogicalEnvelopeBytes","get_IsFaulted"}.Order()),"Unexpected public execution API.");
  Guard("budget-before-entry",()=>Fresh(2907400));Guard("budget-upper",()=>Fresh(16777217));Assert(Fresh(2907401).CopySnapshot().CopyPostControlNodes().Length==89,"Exact budget rejected.");
  using(var cts=new CancellationTokenSource()){cts.Cancel();Guard("prepare-canceled-before-entry",()=>LegacyEnglishCaseQueryInitialization.Prepare(knowledge,numeric,caseData,publication,3145728,cts.Token));Guard("snapshot-canceled-before-entry",()=>value.CopySnapshot(cts.Token));Assert(!value.IsFaulted,"Preentry snapshot cancel poisoned initializer.");}
  foreach(string stage in new[]{"init:preflight","init:inputs","root:case:preflight","root:case:maps","init:initial-snapshot","init:case-preparation","init:dictionary","init:case-binding","init:controls","init:numeric","init:prefix-complete","root:case:enumerated","init:root-bound","root:case:complete","init:publish","init:published"})
  {
   using var cts=new CancellationTokenSource();LegacyEnglishCaseQueryInitialization? partial=null;
   Guard("cancel:"+stage,()=>LegacyEnglishCaseQueryInitialization.PrepareCore(knowledge,numeric,caseData,publication,3145728,(o,t)=>{partial=o;if(t==stage)cts.Cancel();},cts.Token));
   Assert(partial is not null&&partial.IsFaulted,"Canceled plan escaped reusable: "+stage);Guard("partial-refuses:"+stage,()=>partial!.CopySnapshot());cancels++;cancelNames.Add(stage);
  }
  foreach(string stage in new[]{"snapshot:claimed","snapshot:copied"})
  {var owned=Fresh();using var cts=new CancellationTokenSource();Guard("cancel:"+stage,()=>owned.CopySnapshotCore(t=>{if(t==stage)cts.Cancel();},cts.Token));Assert(owned.IsFaulted&&owned.RootForAudit.IsFaulted,"Claimed snapshot failure did not poison own root.");Guard("failed-snapshot-retry:"+stage,()=>owned.CopySnapshot());cancels++;cancelNames.Add(stage);}
  Guard("one-use-inputs",()=>value.PlanForAudit.RequireInputs(knowledge,numeric,caseData,publication,default));Assert(!value.IsFaulted,"Rejected repeat plan poisoned published owner.");
  var foreignContext=new LegacySymbolContextFactory(104);var foreignMaps=LegacyDictionaryInitialSymbolMaps.Prepare(layout.Alphabet,packed.LexicalAlphabet,foreignContext,[],default);
  Guard("one-use-prefix",()=>value.PlanForAudit.PrepareSourcePrefix(foreignMaps,foreignContext,default));Assert(foreignContext.NodeCount==86&&!value.IsFaulted,"Repeated prefix mutated foreign source.");
  bool bindingChecked=false;
  var bindingOwner=LegacyEnglishCaseQueryInitialization.PrepareCore(knowledge,numeric,caseData,publication,3145728,(o,t)=>
  {
   if(t=="init:prefix-complete"){var foreign=LegacyEnglishFreshCaseMainSource.Prepare(knowledge,numeric,caseData,publication);Guard("foreign-context-root-before-bind",()=>o.PlanForAudit.BindPrivateRoot(foreign,default));Assert(!foreign.IsFaulted&&foreign.CopyInitialContextNodes().Length==86,"Foreign-context claim mutated root.");return;}
   if(t!="init:case-binding")return;bindingChecked=true;var b=o.PlanForAudit.BindingForAudit;
   var fake=LegacyEnglishCaseInitializationBindings.Issue(o.PlanForAudit,b.Dictionary,b.Normal,b.Auxiliary,b.Eligibility,b.Contexts,b.Graph);
   Guard("forged-binding-before-publication",()=>LegacyTaggedDictionaryChild.BindEnglishInitialization(fake));
   Guard("repeat-binding-before-publication",()=>LegacyTaggedDictionaryChild.BindEnglishInitialization(b));
   Assert(b.Contexts.NodeCount==86,"Rejected adapter mutated source contexts.");
  },default);Assert(bindingChecked&&!bindingOwner.IsFaulted,"Binding issuer test failed.");
  Guard("second-plan-issuer",()=>LegacyEnglishCaseInitializationPlan.Issue(value,knowledge,numeric,caseData,publication,null));Assert(!value.IsFaulted,"Rejected second plan poisoned owner.");
  var other=Fresh();Guard("foreign-owner-lease",()=>root.BeginInitializationAccess(other,default));Assert(!root.IsFaulted&&!other.IsFaulted,"Rejected foreign lease poisoned source.");
  var foreignRoot=LegacyEnglishFreshCaseMainSource.Prepare(knowledge,numeric,caseData,publication);Guard("repeat-foreign-bind",()=>value.PlanForAudit.BindPrivateRoot(foreignRoot,default));Assert(!foreignRoot.IsFaulted&&!value.IsFaulted,"Rejected bind mutated foreign root.");
  var foreignLayout=LegacySource247WireLayout.Read(bytes);var foreignPacked=LegacyEnglishLkPackedData.Read(foreignLayout,lk);var foreignNumeric=LegacyEnglishLkFreshNumericBinding.Create(LegacyEnglishLkSparseLanguageData.Read(foreignPacked,lk));Guard("foreign-numeric-source",()=>LegacyEnglishCaseQueryInitialization.Prepare(knowledge,foreignNumeric,caseData,publication));
  var copied=snapshot.CopyInitialNodes();copied[0]=copied[0]with{Symbol=65535};Assert(value.CopySnapshot().CopyInitialNodes()[0].Symbol!=65535,"Snapshot node aliases exported mutation.");var map=snapshot.CopyPreservedMap();map[0]=65535;Assert(value.CopySnapshot().CopyPreservedMap()[0]!=65535,"Map alias escaped.");var controls=snapshot.CopyControls();controls[0]=new(0,0,true);Assert(value.CopySnapshot().CopyControls()[0].Symbol==0xfffb,"Control alias escaped.");
  void Blocked(string label,Action<Action<string>> operation,string stage,Action blocked)
  {
   using var entered=new ManualResetEventSlim();using var release=new ManualResetEventSlim();var task=Task.Run(()=>operation(t=>{if(t==stage){entered.Set();if(!release.Wait(TimeSpan.FromSeconds(20)))throw new Exception("Blocked release timeout "+label);}}));
   try{Assert(entered.Wait(TimeSpan.FromSeconds(20)),"Actual operation did not enter "+label);blocked();}finally{release.Set();}task.GetAwaiter().GetResult();
  }
  var inspect=Fresh();var inspectRoot=inspect.RootForAudit;var sc=LegacyEnglishFreshCaseMainScorer.Prepare(inspectRoot);var ss=LegacyEnglishFreshCaseMainSpanSession.Prepare(sc,new(4096,16384));
  int anchor=Enumerable.Range(0,local.NodeCount).First(n=>local.CopyNode(n).Length!=0);byte span=local.CopyNode(anchor)[0].Span;
  Blocked("inspection",cb=>inspect.CopySnapshotCore(cb,default),"snapshot:claimed",()=>
  {
   Guard("parallel-snapshot",()=>inspect.CopySnapshot());Guard("snapshot-blocks-score",()=>sc.ScoreArc(anchor,0));Guard("snapshot-blocks-D0",()=>ss.PrepareSpan(anchor,span));
   using var cts=new CancellationTokenSource();cts.Cancel();Guard("busy-canceled-snapshot",()=>inspect.CopySnapshot(cts.Token));
   var forged=LegacyEnglishFreshCaseMainSource.InitializationLease.Issue(inspectRoot,inspect,1);Guard("forged-inspection-complete",()=>inspectRoot.CompleteInitializationAccess(forged));inspectRoot.PoisonInitializationAccess(forged);
   Guard("forged-poison-did-not-unblock",()=>inspect.CopySnapshot());Assert(!inspect.IsFaulted,"Rejected concurrent attempt poisoned active inspection.");
  });Assert(!inspect.IsFaulted,"Active inspection was damaged.");
  Blocked("score",cb=>sc.ScoreArcCore(anchor,0,cb,default),"score:enter",()=>{Guard("score-blocks-snapshot",()=>inspect.CopySnapshot());Assert(!inspect.IsFaulted,"Busy score was poisoned by rejected snapshot.");});
  Blocked("D0",cb=>ss.PrepareSpanCore(anchor,span,false,cb,default),"span:entered",()=>{Guard("D0-blocks-snapshot",()=>inspect.CopySnapshot());Assert(!inspect.IsFaulted,"Busy D0 was poisoned by rejected snapshot.");});
  Assert(inspect.CopySnapshot().CopyPostControlNodes().Length==89&&!inspect.IsFaulted,"Genuine D0/snapshot lifecycle failed.");
  var leaseOwner=Fresh();var leaseRoot=leaseOwner.RootForAudit;var lease=leaseRoot.BeginInitializationAccess(leaseOwner,default);leaseRoot.CompleteInitializationAccess(lease);var next=leaseRoot.BeginInitializationAccess(leaseOwner,default);
  Guard("stale-complete",()=>leaseRoot.CompleteInitializationAccess(lease));leaseRoot.PoisonInitializationAccess(lease);Assert(!leaseRoot.IsFaulted,"Stale poison damaged later lease.");leaseRoot.CompleteInitializationAccess(next);
  var numericTables=value.NumericForAudit;numericDump=D(("Codes",new[]{"98.wordCodes","C0.0","C0.1","C0.2","C8.0","C8.1"}.ToDictionary(n=>n,n=>(object?)Convert.ToBase64String(numericTables.CopyCodesForAudit(n)))),("Costs",Enumerable.Range(0,6).Select(n=>numericTables.CopyCostsForAudit(n)).ToArray()),("Forms",Enumerable.Range(0,217).Select(i=>{var prep=value.PlanForAudit.BindingForAudit.Normal.Expansion.Preparation;var one=prep.TryReadForms((ushort)i,false,out var f);var two=prep.TryReadForms((ushort)i,true,out var g);return new object?[]{i,one?new ushort[]{f.First,f.Second,f.Third}:null,two?new ushort[]{g.First,g.Second,g.Third}:null};}).ToArray()),("Policies",value.PlanForAudit.BindingForAudit.Normal.Expansion.Policy.CopyPolicies()),("Selectors",value.PlanForAudit.BindingForAudit.Normal.Expansion.Policy.CopySelectors()));
 }
 Assert(sourceBefore==SourceDigest(final),"Initializer mutated owning raw source.");
 rows.Add(D(("Name",name),("Snapshot",Snapshot(snapshot)),("Tokens",root.CopyEnumerationTokens()),("Seed",root.CopyControllerSeed()),("Primary",root.CopyPrimaryMask()),("Secondary",root.CopySecondaryMask()),("Budget",value.AdditionalLogicalEnvelopeBytes),("SourceUnchanged",true)));
}
Assert(tested&&rows.Count>0,"No genuine owner covered.");
using(var stream=File.Create(args[2]))using(var writer=new Utf8JsonWriter(stream)){Write(writer,D(("Scope","fresh-English-initialization-only/no-query-history-terminal"),("InputSha256",Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(args[1]))).ToLowerInvariant()),("Rows",rows),("Refused",refused),("Guards",guards),("GuardNames",guardNames),("Cancellations",cancels),("CancelNames",cancelNames),("Numeric",numericDump),("TargetsRead",false),("VendorExecuted",false)));}
Console.WriteLine($"PASS initialization owners={rows.Count} refused={refused.Count} guards={guards} cancel={cancels}");
static Dictionary<string,object?> D(params(string Key,object? Value)[] rows)=>rows.ToDictionary(x=>x.Key,x=>x.Value);
static string Encode(object value){using var stream=new MemoryStream();using(var writer=new Utf8JsonWriter(stream))Write(writer,value);return System.Text.Encoding.UTF8.GetString(stream.ToArray());}
static void Write(Utf8JsonWriter w,object? value)
{
 switch(value)
 {
 case null:w.WriteNullValue();break;
 case string s:w.WriteStringValue(s);break;
 case bool b:w.WriteBooleanValue(b);break;
 case int n:w.WriteNumberValue(n);break;
 case uint n:w.WriteNumberValue(n);break;
 case long n:w.WriteNumberValue(n);break;
 case ulong n:w.WriteNumberValue(n);break;
 case byte n:w.WriteNumberValue(n);break;
 case short n:w.WriteNumberValue(n);break;
 case ushort n:w.WriteNumberValue(n);break;
 case float n:w.WriteNumberValue(n);break;
 case double n:w.WriteNumberValue(n);break;
 case Dictionary<string,object?> d:w.WriteStartObject();foreach(var item in d){w.WritePropertyName(item.Key);Write(w,item.Value);}w.WriteEndObject();break;
 case System.Collections.IEnumerable list:w.WriteStartArray();foreach(var item in list)Write(w,item);w.WriteEndArray();break;
 default:throw new InvalidDataException("Explicit static JSON scalar required.");
 }
}
