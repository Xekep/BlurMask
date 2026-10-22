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


var guardNames=new List<string>();var cancelNames=new List<string>();bool tested=false;object? preparedPoolControl=null;
void Assert(bool value,string text){if(!value)throw new Exception(text);}
void Guard(string name,Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException or InvalidDataException){guards++;guardNames.Add(name);return;}throw new Exception("Expected refusal "+name);}
var actualItems=inputs.RootElement.ValueKind==JsonValueKind.Array?inputs.RootElement.EnumerateArray().ToArray():inputs.RootElement.GetProperty("samples").EnumerateArray().ToArray();
foreach(var original in actualItems)
{
 JsonElement item=original;string name;
 if(inputs.RootElement.ValueKind==JsonValueKind.Array)name=item.GetProperty("Name").GetString()!;
 else{name=original.GetProperty("id").GetString()!;using var transformed=JsonDocument.Parse(Encode(D(("Name",name),("Raw",original.GetProperty("strokes").EnumerateArray().Select(s=>s.EnumerateArray().Select(p=>new float[]{p[0].GetSingle(),p[1].GetSingle()}).ToArray()).ToArray()),("Resolution",0))));item=transformed.RootElement.Clone();}
 var post=Post(item);if(post.CopySnapshots()[0].CopyDescriptors().Length!=1){refused.Add(name);continue;}
 var final=LegacyEnglish119FinalizedSingleSource.Prepare(post).Run();var local=LegacyEnglish119MainLocalCandidates.Prepare(final,table).Run();var knowledge=LegacyEnglish119MainKnowledge.Create(local);
 LegacyEnglishCaseInitialQueryPass Fresh()=>LegacyEnglishCaseInitialQueryPass.Prepare(knowledge,numeric,caseData,publication);
 var value=Fresh();string before=SourceDigest(final);var stages=new List<string>();var pages=value.Run(stages.Add);var contexts=value.Initializer.PlanForAudit.BindingForAudit.Contexts;var nodes=contexts.CopyNodes().ToArray();var ordinal=nodes.Select((n,i)=>(n.Identity,Index:(long)i+1)).ToDictionary(x=>x.Identity,x=>x.Index);
 long Id(ulong raw)=>raw==0?0:ordinal[raw];
 object Node(LegacySymbolContextNode n)=>new long[]{Id(n.Identity),Id(n.ParentLink),Id(n.ChildTreeRoot),Id(n.Left),Id(n.Right),n.Symbol,n.Balance,n.PageIndex,n.PageSlot};
 object Page(LegacyEnglishCaseInitialQueryRecord p)
 {
  byte[] b=p.CopyRecords();for(int i=0;i<p.Count;i++){int at=i*32+8;ulong id=System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(b.AsSpan(at));System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(at),checked((ulong)Id(id)));}
  return D(("Skip",p.Skip),("Count",p.Count),("Accepted",p.Accepted),("Slot",p.Slot),("BackendCalls",p.BackendCalls),("Epoch",p.Epoch),("Records",Convert.ToHexStringLower(b)));
 }
 var receipt=value.D0!;
 if(!tested)
 {
  tested=true;Guard("whole-budget-low",()=>LegacyEnglishCaseInitialQueryPass.Prepare(knowledge,numeric,caseData,publication,90987784));Guard("whole-budget-high",()=>LegacyEnglishCaseInitialQueryPass.Prepare(knowledge,numeric,caseData,publication,134217729));
  Guard("second-run",()=>value.Run());Assert(!value.IsFaulted,"Completed second-run refusal poisoned source.");
  var poolReceipt=LegacyEnglishCaseExecutionPoolReceipt.Preflight(knowledge,numeric,134217728,default);
  var foreignKnowledge=LegacyEnglish119MainKnowledge.Create(local);
  Guard("foreign-knowledge-does-not-consume-pool",()=>LegacyEnglishCaseQueryInitialization.PrepareExecution(foreignKnowledge,numeric,caseData,publication,poolReceipt,null,default));
  var receiptInitializer=LegacyEnglishCaseQueryInitialization.PrepareExecution(knowledge,numeric,caseData,publication,poolReceipt,null,default);
  Guard("second-pool-claim",()=>LegacyEnglishCaseQueryInitialization.PrepareExecution(knowledge,numeric,caseData,publication,poolReceipt,null,default));
  Assert(!receiptInitializer.IsFaulted&&receiptInitializer.CopySnapshot().InitialContextCapacity==2048,"Rejected receipt claim damaged original initialization.");
  var fake=new LegacyNativeDelimiterQuery(){SourceTag=0,Request=new(){Node=0,Capacity=128,ContextRoot=contexts.ContainerIdentity}};
  Guard("same-owner-zero-state",()=>value.RequireIssuedForAudit(fake));fake.SourceTag=LegacyEnglishCaseInitialQueryPass.InitialTag;Guard("equal-fields-foreign-cell",()=>value.RequireIssuedForAudit(fake));
  using(var cts=new CancellationTokenSource()){cts.Cancel();var fresh=Fresh();Guard("cancel-before-reset",()=>fresh.Run(token:cts.Token));Assert(fresh.ResetCount==0&&!fresh.IsFaulted,"Preentry canceled execution consumed reset.");}
  foreach(string stage in new[]{"query:reset-normal","query:reset-auxiliary","query:reset-backend","query:reset","query:before-D0","span:entered","source:bytes","source:expanded","source:refreshed","source:forwarded","span:installed","query:D0","query:map-installed","query:before-read","query:page","query:publish"})
  {
   var fresh=Fresh();using var cts=new CancellationTokenSource();Guard("cancel:"+stage,()=>fresh.Run(s=>{if(s==stage)cts.Cancel();},cts.Token));
   bool partial=stage is "query:reset-normal" or "query:reset-auxiliary" or "query:reset-backend";
   Assert(fresh.IsFaulted&&fresh.ResetAttempts==1&&fresh.ResetCount==(partial?0:1),"Claimed cancel did not preserve reset/failure "+stage);
   if(stage=="query:reset-normal")Assert(fresh.ResetCompletedStages==1,"Lost normal-reset write prefix.");
   if(stage=="query:reset-auxiliary")Assert(fresh.ResetCompletedStages==2,"Lost auxiliary-reset write prefix.");
   if(stage=="query:reset-backend")Assert(fresh.ResetCompletedStages==3,"Lost backend-reset write prefix.");
   var prefix=fresh.CopyCompletedPages();Assert(prefix.Length==(stage=="query:page"?1:stage=="query:publish"?pages.Length:0),"Completed page prefix lost "+stage);
   if(prefix.Length>0){var copy=prefix[0].CopyRecords();byte saved=copy[0];copy[0]^=255;Assert(prefix[0].CopyRecords()[0]==saved,"Fault prefix exposed mutable records.");}
   Guard("canceled-retry:"+stage,()=>fresh.Run());cancels++;cancelNames.Add(stage);
  }
  void Blocked(string label,Action<Action<string>> operation,string stage,Action blocked)
  {
   using var entered=new ManualResetEventSlim();using var release=new ManualResetEventSlim();var task=Task.Run(()=>operation(s=>{if(s==stage){entered.Set();if(!release.Wait(TimeSpan.FromSeconds(30)))throw new Exception("Release timeout "+label);}}));
   try{Assert(entered.Wait(TimeSpan.FromSeconds(30)),"Entry timeout "+label);blocked();}finally{release.Set();}task.GetAwaiter().GetResult();
  }
  var busy=Fresh();int anchor=Enumerable.Range(0,local.NodeCount).First(n=>local.CopyNode(n).Length!=0);
  Blocked("preentry-score",cb=>busy.Scorer.ScoreArcCore(anchor,0,cb,default),"score:enter",()=>{Guard("busy-score-no-reset",()=>busy.Run());Assert(busy.ResetCount==0&&!busy.IsFaulted,"Busy preentry damaged actual score.");});Assert(busy.Run().Length>0,"Pristine retry after busy failed.");
  //Consumed reset must not be replayed if an ACTUAL score wins the next gate.
  var resetRace=Fresh();using(var entry=new ManualResetEventSlim())using(var release=new ManualResetEventSlim())
  {
   Task? score=null;
   try{Guard("post-reset-score-preemption",()=>resetRace.Run(stage=>
   {
    if(stage!="query:before-D0")return;
    score=Task.Run(()=>resetRace.Scorer.ScoreArcCore(anchor,0,s=>{if(s=="score:enter"){entry.Set();if(!release.Wait(TimeSpan.FromSeconds(30)))throw new Exception("Score release timeout.");}},default));
    Assert(entry.Wait(TimeSpan.FromSeconds(30)),"Actual score did not preempt first D0.");
   }));Assert(resetRace.IsFaulted&&!resetRace.Root.IsFaulted&&resetRace.ResetCount==1&&resetRace.Root.EnteredPreparations==0,"Consumed-reset local failure damaged active score.");}
   finally{release.Set();score?.GetAwaiter().GetResult();}
   Guard("consumed-reset-no-retry",()=>resetRace.Run());Assert(resetRace.ResetCount==1&&!resetRace.Root.IsFaulted,"Failed executor replayed reset or poisoned completed score.");
  }
  //An actual historical-snapshot lease can preempt the first query after D0.
  var d0Race=Fresh();using(var entry=new ManualResetEventSlim())using(var release=new ManualResetEventSlim())
  {
   Task? inspection=null;
   try{Guard("post-D0-inspection-preemption",()=>d0Race.Run(stage=>
   {
    if(stage!="query:D0")return;
    inspection=Task.Run(()=>d0Race.Initializer.CopySnapshotCore(s=>{if(s=="snapshot:claimed"){entry.Set();if(!release.Wait(TimeSpan.FromSeconds(30)))throw new Exception("Inspection release timeout.");}},default));
    Assert(entry.Wait(TimeSpan.FromSeconds(30)),"Actual inspection did not preempt query entry.");
   }));Assert(d0Race.IsFaulted&&!d0Race.Root.IsFaulted&&d0Race.D0 is not null&&d0Race.Root.EnteredPreparations==1&&d0Race.EnteredQueries==0,"Consumed-D0 failure damaged active inspection.");}
   finally{release.Set();inspection?.GetAwaiter().GetResult();}
   Guard("consumed-D0-no-retry",()=>d0Race.Run());Assert(d0Race.Root.EnteredPreparations==1&&d0Race.ResetCount==1&&!d0Race.Root.IsFaulted,"Failed executor replayed D0/reset.");
  }
  var running=Fresh();Blocked("actual-query",cb=>running.Run(cb),"query:before-read",()=>
  {
   Assert(!running.D0!.IsCurrent,"Observational D0 currentness must reject own query busy bit.");
   var wrong=new LegacyNativeDelimiterQuery(){SourceTag=0,Request=new(){Node=0,Capacity=128,ContextRoot=running.Initializer.PlanForAudit.BindingForAudit.Contexts.ContainerIdentity}};
   Guard("active-entry-zero-state",()=>running.ReadUnissuedFrameForAudit(wrong));wrong.SourceTag=LegacyEnglishCaseInitialQueryPass.InitialTag;Guard("active-entry-foreign-frame",()=>running.ReadUnissuedFrameForAudit(wrong));
   var foreignMap=new LegacyPreparedDictionaryWorkingMap(217,running.Root.DescriptorFlags,running.Root.CopyWorkingMap(),running.Root.CopyAuxiliaryMap());
   var forged=LegacyEnglishFreshCaseMainSource.InitialQueryLease.Issue(running.Root,running,running.D0.Epoch,foreignMap);
   Guard("equal-byte-foreign-map-lease",()=>running.Root.ValidateInitialQuery(forged));Guard("forged-query-complete",()=>running.Root.CompleteInitialQuery(forged));running.Root.PoisonInitialQuery(forged);
   Guard("query-blocks-score",()=>running.Scorer.ScoreArc(anchor,0));Guard("query-blocks-D0",()=>running.SpanSession.PrepareSpan(0,0));Guard("query-blocks-snapshot",()=>running.Initializer.CopySnapshot());Guard("query-blocks-second-run",()=>running.Run());Assert(!running.IsFaulted,"Busy refusal poisoned query.");
  });
  Assert(!running.IsFaulted,"Successful query failed after contention.");
  var foreignPass=Fresh();foreignPass.Run();var exactPass=Fresh();bool foreignChecked=false;
  exactPass.Run(stage=>
  {
   if(stage!="query:D0")return;foreignChecked=true;
   Guard("foreign-current-D0-receipt",()=>exactPass.Root.BeginInitialQuery(exactPass,foreignPass.SpanSession,foreignPass.D0!,default));
   Guard("foreign-query-owner",()=>exactPass.Root.BeginInitialQuery(foreignPass,exactPass.SpanSession,exactPass.D0!,default));
   Assert(!exactPass.IsFaulted&&!foreignPass.IsFaulted,"Rejected foreign owner consumed active query slot.");
  });Assert(foreignChecked&&!exactPass.IsFaulted,"Exact query did not follow rejected foreign claims.");
  Guard("historical-page-cannot-read",()=>exactPass.ReadUnissuedFrameForAudit(fake));
  var old=LegacyEnglishCaseQueryInitialization.Prepare(knowledge,numeric,caseData,publication);Assert(old.CopySnapshot().InitialContextCapacity==104,"Public initializer capacity changed.");
  Assert(value.Initializer.CopySnapshot().InitialContextCapacity==2048,"Private execution snapshot lies about capacity.");
  //Explicit PREPARED allocator control, not an actual query or ready source.
  var prepared=Fresh();var pc=prepared.Initializer.PlanForAudit.BindingForAudit.Contexts;var oc=old.PlanForAudit.BindingForAudit.Contexts;var keys=new List<ushort>();
  for(ushort key=0;pc.NodeCount<105;key++){pc.ResolveInitial(key);keys.Add(key);}
  int denied=0;foreach(ushort key in keys)if(oc.ResolveInitial(key).Identity==0)denied++;
  Assert(denied==1&&pc.NodeCount==105&&oc.NodeCount==104,"Private/public pool boundary conflated.");
  var pn=pc.CopyNodes().ToArray();var pi=pn.Select((n,i)=>(n.Identity,Index:(long)i+1)).ToDictionary(x=>x.Identity,x=>x.Index);long PId(ulong id)=>id==0?0:pi[id];
  preparedPoolControl=D(("Scope","PREPARED allocator only; never a query/ready owner"),("Public104NullAllocations",denied),("InsertedKeys",keys),("Nodes",pn.Select(n=>new long[]{PId(n.Identity),PId(n.ParentLink),PId(n.ChildTreeRoot),PId(n.Left),PId(n.Right),n.Symbol,n.Balance,n.PageIndex,n.PageSlot}).ToArray()),("Root",PId(pc.RootTreeIdentity)),("Pages",pc.CopyPageUsedCounts()),("PageCapacity",pc.PageDescriptorCapacity),("Cursor",pc.CurrentPageIndex));
  prepared.Root.PoisonInitializationOwner(prepared.Initializer);old.RootForAudit.PoisonInitializationOwner(old);Guard("prepared-mutated-owner-never-ready",()=>prepared.Run());
 }
 Assert(before==SourceDigest(final),"Original source720 changed.");Assert(!value.IsFaulted&&value.ResetCount==1&&value.EnteredQueries==1,"Execution prefix/counters incomplete.");
 rows.Add(D(("Name",name),("Stages",stages),("LogicalCeiling",value.LogicalCeiling),("ResetCount",value.ResetCount),("EnteredQueries",value.EnteredQueries),("ConsumedRows",value.ConsumedRows),("InitialTag",LegacyEnglishCaseInitialQueryPass.InitialTag),("ContextCapacity",value.Initializer.CopySnapshot().InitialContextCapacity),("Pages",pages.Select(Page).ToArray()),("Nodes",nodes.Select(Node).ToArray()),("Root",Id(contexts.RootTreeIdentity)),("PageCounts",contexts.CopyPageUsedCounts()),("PageCapacity",contexts.PageDescriptorCapacity),("PageCursor",contexts.CurrentPageIndex),("Working",receipt.CopyWorkingMap()),("RawPermissions",receipt.CopyRawPermissions()),("Permissions",receipt.CopyPermissions()),("Selected",receipt.CopySelectedRows().Select(z=>D(("Ordinal",z.ArcIndex),("Group",z.GroupId),("Costs",z.CopyCosts()))).ToArray()),("InitialSource68",value.Initializer.CopySnapshot().CopyInitialSource68().Select(Id).ToArray()),("Source720",before)));
 Console.WriteLine(name+" pages="+pages.Length+" rows="+value.ConsumedRows+" nodes="+nodes.Length);
}
using(var captureFile=File.Create(args[2]))using(var writer=new Utf8JsonWriter(captureFile)){Write(writer,D(("InputSha256",Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(args[1])))),("TargetsRead",false),("Scope","Actual first case290 node0/span0 query prefix only; no word/history/winner"),("Rows",rows),("MultilineRefused",refused),("Guards",guards),("GuardNames",guardNames),("Cancellations",cancels),("CancelNames",cancelNames),("PreparedPoolControl",preparedPoolControl)));}
Console.WriteLine("PASS rows="+rows.Count+" refused="+refused.Count+" guards="+guards+" cancels="+cancels);
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
