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


var guardNames=new List<string>();var cancelNames=new List<string>();bool tested=false;
void Assert(bool condition,string message){if(!condition)throw new Exception(message);}
void Guard(string name,Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException or InvalidDataException){guards++;guardNames.Add(name);return;}throw new Exception("Expected refusal "+name);}
var actualItems=inputs.RootElement.ValueKind==JsonValueKind.Array?inputs.RootElement.EnumerateArray().ToArray():inputs.RootElement.GetProperty("samples").EnumerateArray().ToArray();
foreach(var original in actualItems)
{
 JsonElement item=original;string name;
 if(inputs.RootElement.ValueKind==JsonValueKind.Array)name=item.GetProperty("Name").GetString()!;
 else{name=original.GetProperty("id").GetString()!;using var transformed=JsonDocument.Parse(Encode(D(("Name",name),("Raw",original.GetProperty("strokes").EnumerateArray().Select(s=>s.EnumerateArray().Select(p=>new float[]{p[0].GetSingle(),p[1].GetSingle()}).ToArray()).ToArray()),("Resolution",0))));item=transformed.RootElement.Clone();}
 var post=Post(item);if(post.CopySnapshots()[0].CopyDescriptors().Length!=1){refused.Add(name);continue;}
 var final=LegacyEnglish119FinalizedSingleSource.Prepare(post).Run();var local=LegacyEnglish119MainLocalCandidates.Prepare(final,table).Run();var knowledge=LegacyEnglish119MainKnowledge.Create(local);
 var consumer=LegacyEnglishCaseInitialQueryPass.PreparePrimaryConsumer(knowledge,numeric,caseData,publication);
 var pass=Access.Pass(consumer);var request=Access.Request(consumer);var before=SourceDigest(final);var traces=new List<object>();var checkpoints=new List<object>();var stages=new List<string>();
 var actualGraph=Access.Graph(Access.Frontend(consumer));var originalCosts=Access.ReadCosts(consumer);Access.ReadCosts(consumer)=(node,arc)=>
 {
  var raw=request.CopyHistorical();var b=raw.Bytes;Array.Clear(b,40,8);var row=originalCosts(node,arc);var origin=actualGraph.GetNode(node)[arc];var ranges=actualGraph.Groups[origin.GroupId].Ranges.ToArray();
  traces.Add(D(("Node",node),("Arc",arc),("Group",origin.GroupId),("Span",origin.Span),("StartContext",origin.StartContextId),("Ranges",ranges.Select(z=>new[]{z.StrokeIndex,z.StartPoint,z.EndPointExclusive}).ToArray()),("Bytes",Convert.ToHexStringLower(b)),("Mask",Convert.ToHexStringLower(raw.Mask)),("BaseCost",raw.Cost),("ClassCosts",row.ClassCosts.ToArray()),("Geometry",row.GeometryCosts.ToArray())));return row;
 };
 LegacyEnglishCaseInitialHeapSnapshot heap;
 try{heap=consumer.Run(stage=>{stages.Add(stage);if(stage=="query:page")checkpoints.Add(D(("Rows",consumer.CompletedRows),("Qualified",consumer.QualifiedRows),("Scoring",consumer.AttemptedScoringCalls),("Heap",consumer.CopyHistoricalHeap().CopySlots().Select(Convert.ToHexStringLower).ToArray())));});}
 catch(Exception e){throw new Exception("ACTUAL consumer failure "+name+" stages="+string.Join(',',stages),e);}
 var contexts=pass.Initializer.PlanForAudit.BindingForAudit.Contexts;var nodes=contexts.CopyNodes().ToArray();var ordinal=nodes.Select((n,i)=>(n.Identity,Index:(long)i+1)).ToDictionary(x=>x.Identity,x=>x.Index);
 long Id(ulong raw)=>raw==0?0:ordinal[raw];
 object Node(LegacySymbolContextNode n)=>new long[]{Id(n.Identity),Id(n.ParentLink),Id(n.ChildTreeRoot),Id(n.Left),Id(n.Right),n.Symbol,n.Balance,n.PageIndex,n.PageSlot};
 string Slot(byte[] b){var copy=b.ToArray();ulong id=System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(copy.AsSpan(8));System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(copy.AsSpan(8),checked((ulong)Id(id)));return Convert.ToHexStringLower(copy);}
 object Page(LegacyEnglishCaseInitialQueryRecord p){var b=p.CopyRecords();for(int i=0;i<p.Count;i++){int at=i*32+8;ulong id=System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(b.AsSpan(at));System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(b.AsSpan(at),checked((ulong)Id(id)));}return D(("Skip",p.Skip),("Count",p.Count),("Records",Convert.ToHexStringLower(b)));}

 if(!tested&&name=="actual-0")
 {
  tested=true;LegacyEnglishCaseInitialConsumer Fresh()=>LegacyEnglishCaseInitialQueryPass.PreparePrimaryConsumer(knowledge,numeric,caseData,publication);
  Guard("whole-envelope-low",()=>LegacyEnglishCaseInitialQueryPass.PreparePrimaryConsumer(knowledge,numeric,caseData,publication,126639368));
  Guard("whole-envelope-high",()=>LegacyEnglishCaseInitialQueryPass.PreparePrimaryConsumer(knowledge,numeric,caseData,publication,134217729));
  Guard("completed-second-run",()=>consumer.Run());Assert(!pass.IsFaulted,"Second run damaged completed pass.");
  var copy=heap.CopySlots();copy[0][0]^=255;Assert(copy[0][0]!=heap.CopySlots()[0][0],"Snapshot exposed mutable storage.");
  var maskCopy=heap.CopyWrittenMask();maskCopy[0]=0;Assert(heap.CopyWrittenMask()[0]==1,"Snapshot exposed mask storage.");
  var beforeCancel=Fresh();using(var cancel=new CancellationTokenSource()){cancel.Cancel();Guard("preentry-cancel",()=>beforeCancel.Run(token:cancel.Token));Assert(Access.Pass(beforeCancel).ResetCount==0&&!Access.Pass(beforeCancel).IsFaulted,"Preentry cancel consumed/faulted source.");}
  Assert(beforeCancel.Run().Count>0,"Preentry cancel prevented pristine retry.");
  foreach(string stage in new[]{"query:reset-normal","query:reset-auxiliary","query:reset-backend","query:reset","query:before-D0","span:entered","source:bytes","source:expanded","source:refreshed","source:forwarded","span:installed","query:D0","query:map-installed","query:before-read","query:page","query:publish"})
  {
   var fresh=Fresh();var fp=Access.Pass(fresh);using var cancel=new CancellationTokenSource();
   Guard("cancel:"+stage,()=>fresh.Run(s=>{if(s==stage)cancel.Cancel();},cancel.Token));
   Assert(fp.IsFaulted&&fp.ResetAttempts==1,"Claimed cancellation lost source fault/reset attempt.");
   if(stage=="query:page"||stage=="query:publish")Assert(fresh.CompletedRows==37&&fresh.CopyHistoricalHeap().Count==16,"Completed row/heap prefix lost.");
   else Assert(fresh.CopyHistoricalHeap().Count==0,"Preconsumer cancellation invented heap rows.");
   Guard("canceled-retry:"+stage,()=>fresh.Run());cancels++;cancelNames.Add(stage);
  }
  var mid=Fresh();var mp=Access.Pass(mid);var read=Access.ReadCosts(mid);int calls=0;using(var cancel=new CancellationTokenSource())
  {
   Access.ReadCosts(mid)=(node,arc)=>{var answer=read(node,arc);if(++calls==3)cancel.Cancel();return answer;};
   Guard("mid-acoustic-cancel",()=>mid.Run(token:cancel.Token));Assert(mp.IsFaulted&&mid.AttemptedScoringCalls==3&&mp.ConsumedRows==0&&mp.CopyCompletedPages().Length==1&&mid.CopyHistoricalHeap().Count==2,"Mid-score cancellation lost actual attempted/accepted/page prefix.");cancels++;cancelNames.Add("mid-acoustic");
  }
  var fault=Fresh();var fr=Access.ReadCosts(fault);int faultCalls=0;Access.ReadCosts(fault)=(node,arc)=>{if(++faultCalls==3)throw new InvalidDataException("Deterministic test-only callback failure.");return fr(node,arc);};
  Guard("mid-acoustic-fault",()=>fault.Run());Assert(Access.Pass(fault).IsFaulted&&fault.AttemptedScoringCalls==3&&fault.CopyHistoricalHeap().Count==2,"Acoustic fault rolled back or fabricated prefix.");
  void Blocked(string label,Action<Action<string>> operation,string stage,Action blocked)
  {
   using var entered=new ManualResetEventSlim();using var release=new ManualResetEventSlim();var task=Task.Run(()=>operation(s=>{if(s==stage){entered.Set();if(!release.Wait(TimeSpan.FromSeconds(30)))throw new Exception("Release timeout "+label);}}));
   try{Assert(entered.Wait(TimeSpan.FromSeconds(30)),"Entry timeout "+label);blocked();}finally{release.Set();}task.GetAwaiter().GetResult();
  }
  var busy=Fresh();var bp=Access.Pass(busy);
  Blocked("preentry-score",cb=>bp.Scorer.ScoreArcCore(0,0,cb,default),"score:enter",()=>{Guard("busy-score-before-reset",()=>busy.Run());Assert(!bp.Root.IsFaulted&&bp.ResetAttempts==0,"Busy preentry consumed reset/damaged foreign score.");});
  // Consumer is one-use even for a preentry busy attempt; the foreign operation stays valid.
  Assert(!bp.Root.IsFaulted,"Foreign score was poisoned after preentry refusal.");Guard("one-use-busy-consumer",()=>busy.Run());
  LegacyEnglishFreshCaseMainSource.InitialQueryLease? capturedLease=null;var running=Fresh();var rp=Access.Pass(running);var foreign=Fresh();var foreignPass=Access.Pass(foreign);foreign.Run();
  Blocked("actual-query",cb=>running.Run(cb),"query:page",()=>
  {
   var lease=Access.ActiveLease(rp)??throw new Exception("Missing actual active query lease.");capturedLease=lease;
   var rq=Access.Request(running);var accessor=new LegacyEnglishCaseInitialAcousticRequest.Accessor(rq,lease);
   Guard("masked-unwritten-read",()=>accessor.ReadInt32(29));Guard("masked-illegal-write",()=>accessor.WriteByte(64,1));
   Guard("historical-foreign-page",()=>running.Consume(lease,foreignPass.CopyCompletedPages()[0],default));
   var map=new LegacyPreparedDictionaryWorkingMap(217,rp.Root.DescriptorFlags,rp.Root.CopyWorkingMap(),rp.Root.CopyAuxiliaryMap());
   var forged=LegacyEnglishFreshCaseMainSource.InitialQueryLease.Issue(rp.Root,rp,lease.Epoch,map);
   Guard("equal-byte-foreign-lease",()=>running.RequireLease(forged));
   Guard("wrong-owner-lease",()=>foreign.RequireLease(lease));
   var fake=new LegacyNativeDelimiterQuery(){SourceTag=0,Request=new(){Node=0,Capacity=128,ContextRoot=rp.Initializer.PlanForAudit.BindingForAudit.Contexts.ContainerIdentity}};
   Guard("active-zero-state",()=>rp.ReadUnissuedFrameForAudit(fake));
   Guard("query-blocks-score",()=>rp.Scorer.ScoreArc(0,0));Guard("query-blocks-D0",()=>rp.SpanSession.PrepareSpan(0,0));Guard("query-blocks-snapshot",()=>rp.Initializer.CopySnapshot());Guard("query-blocks-second-run",()=>running.Run());
   Assert(!rp.IsFaulted,"Rejected concurrent/foreign operation poisoned actual owner.");
  });Assert(!rp.IsFaulted,"Actual query failed after rejected contention.");
  var retry=Fresh();var retryPass=Access.Pass(retry);
  Guard("preentry-historical-rejection-retry",()=>retry.Consume(capturedLease!,rp.CopyCompletedPages()[0],default));
  var preentryMap=new LegacyPreparedDictionaryWorkingMap(217,retryPass.Root.DescriptorFlags,retryPass.Root.CopyWorkingMap(),retryPass.Root.CopyAuxiliaryMap());
  var preentryFake=LegacyEnglishFreshCaseMainSource.InitialQueryLease.Issue(retryPass.Root,retryPass,0,preentryMap);
  Guard("preentry-malformed-rejection-retry",()=>retry.Consume(preentryFake,rp.CopyCompletedPages()[0],default));
  Assert(Access.IssuedLease(retry)is null&&retryPass.ResetCount==0&&retryPass.Root.EnteredPreparations==0&&!retryPass.IsFaulted,"Rejected preentry call mutated future lease/source.");
  bool retried=false;
  var retryHeap=retry.Run(stage=>
  {
   if(stage!="query:before-read"||retried)return;retried=true;var exact=Access.ActiveLease(retryPass)!;
   Guard("active-foreign-beforeissuer-retry",()=>retry.Consume(capturedLease!,rp.CopyCompletedPages()[0],default));
   var equalFields=LegacyEnglishFreshCaseMainSource.InitialQueryLease.Issue(retryPass.Root,retryPass,exact.Epoch,exact.Map);
   Guard("active-malformed-beforeissuer-retry",()=>retry.Consume(equalFields,rp.CopyCompletedPages()[0],default));
   Assert(Access.IssuedLease(retry)is null&&!retryPass.IsFaulted,"Rejected foreign/equal-field lease published issuer.");
  });Assert(retried&&retryHeap.Count==16&&retry.CompletedRows==37&&!retryPass.IsFaulted&&retryPass.ResetCount==1&&retryPass.Root.EnteredPreparations==1,"Legitimate first run failed after rejected preentry/foreign issuers.");
  // PREPARED corruption control only: inject a previously unsupported label
  // into an exact current page, restore storage/counter in finally, and require
  // whole-operation failure. This is never reported as a genuine source row.
  foreach(string text in new[]{"\U000f0002","\U000f0003","\U000f0004","\U000f0008"})
  {
   if(!layout.Alphabet.TryGetLabel(text,out ushort special))continue;
   var extra=Fresh();var ep=Access.Pass(extra);bool reached=false;
   Guard("prepared-special-extra:"+text,()=>extra.Run(stage=>
   {
    if(stage!="query:page"||reached)return;reached=true;var page=ep.CopyCompletedPages()[0];var bytes=Access.PageBytes(page);ushort saved=System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(20));int oldRows=Access.Rows(extra);
    try{Access.Rows(extra)=0;System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(20),special);extra.Consume(Access.ActiveLease(ep)!,page,default);}
    finally{Access.Rows(extra)=oldRows;System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(20),saved);}
   }));Assert(reached&&ep.IsFaulted&&extra.CompletedRows==37&&extra.CopyHistoricalHeap().Count==16,"Special refusal lost restored genuine historical prefix.");
  }
  var stale=Access.Request(running);Guard("completed-request-stale",()=>new LegacyEnglishCaseInitialAcousticRequest.Accessor(stale,capturedLease??throw new Exception("Missing preserved genuine historical lease.")).ReadInt32(0));
 }
 if(before!=SourceDigest(final))throw new Exception("Source720 changed.");
 rows.Add(D(("Name",name),("Stages",stages),("Rows",consumer.CompletedRows),("Qualified",consumer.QualifiedRows),("Scoring",consumer.AttemptedScoringCalls),("HeapAttempts",heap.Attempts),("HeapAccepted",heap.Accepted),("HeapReservedSlotWrites",heap.ReservedSlotWriteEvents),("Heap",heap.CopySlots().Select(Slot).ToArray()),("SlotMask",Convert.ToHexStringLower(heap.CopyWrittenMask())),("Pages",pass.CopyCompletedPages().Select(Page).ToArray()),("Nodes",nodes.Select(Node).ToArray()),("Root",Id(contexts.RootTreeIdentity)),("Traces",traces),("Source720",before)));
 Console.WriteLine(name+" consumerRows="+consumer.CompletedRows+" scoring="+consumer.AttemptedScoringCalls+" heap="+heap.Count);
}
using(var captureFile=File.Create(args[2]))using(var writer=new Utf8JsonWriter(captureFile)){Write(writer,D(("InputSha256",Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(args[1])))),("TargetsRead",false),("Scope","Actual first case290 primary allocation prefix only; no history/parent88/winner"),("Rows",rows),("MultilineRefused",refused),("Guards",guards),("GuardNames",guardNames),("Cancellations",cancels),("CancelNames",cancelNames)));}
Console.WriteLine("PASS actual="+rows.Count+" refused="+refused.Count);
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

static class Access
{
 [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Field,Name="issuedLease")]internal static extern ref LegacyEnglishFreshCaseMainSource.InitialQueryLease? IssuedLease(LegacyEnglishCaseInitialConsumer value);
 [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Field,Name="frontend")]internal static extern ref LegacyEnglishCaseInitialFrontend Frontend(LegacyEnglishCaseInitialConsumer value);
 [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Field,Name="graph")]internal static extern ref LegacyOrdinaryCursiveCandidateGraph Graph(LegacyEnglishCaseInitialFrontend value);
 [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Field,Name="records")]internal static extern ref byte[] PageBytes(LegacyEnglishCaseInitialQueryRecord value);
 [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Field,Name="rows")]internal static extern ref int Rows(LegacyEnglishCaseInitialConsumer value);
 [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Field,Name="activeQueryLease")]internal static extern ref LegacyEnglishFreshCaseMainSource.InitialQueryLease? ActiveLease(LegacyEnglishCaseInitialQueryPass value);
 [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Field,Name="pass")]internal static extern ref LegacyEnglishCaseInitialQueryPass Pass(LegacyEnglishCaseInitialConsumer value);
 [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Field,Name="request")]internal static extern ref LegacyEnglishCaseInitialAcousticRequest Request(LegacyEnglishCaseInitialConsumer value);
 [System.Runtime.CompilerServices.UnsafeAccessor(System.Runtime.CompilerServices.UnsafeAccessorKind.Field,Name="readCosts")]internal static extern ref Func<int,ushort,LegacyNativeWordAcousticGroupCosts> ReadCosts(LegacyEnglishCaseInitialConsumer value);
}
