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

object Points(IEnumerable<LegacyInkStroke> strokes)=>strokes.Select(s=>s.Points.ToArray().Select(p=>new int[]{p.X,p.Y}).ToArray()).ToArray();
object Floats(IEnumerable<InkStroke> strokes)=>strokes.Select(s=>s.Points.Select(p=>new float[]{p.X,p.Y}).ToArray()).ToArray();
object Ranges(IEnumerable<LegacyInkRange> ranges)=>ranges.Select(r=>new int[]{r.StrokeIndex,r.StartPoint,r.EndPointExclusive}).ToArray();
var preparedControls=new List<object>();var calibrationControls=new List<object>();
var catalog=new Dictionary<string,object?>();int calibrationGuards=0;
bool tested=false;int totalPasses=0,totalSelected=0;
void Guard(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException or InvalidDataException){guards++;return;}throw new Exception("Expected strict case refusal.");}
int totalGroups=0,totalArcs=0;var timing=System.Diagnostics.Stopwatch.StartNew();
var actualItems=inputs.RootElement.ValueKind==JsonValueKind.Array?inputs.RootElement.EnumerateArray().ToArray():inputs.RootElement.GetProperty("samples").EnumerateArray().ToArray();
foreach(var originalItem in actualItems)
{
 JsonElement item=originalItem;string name;
 if(inputs.RootElement.ValueKind!=JsonValueKind.Array)
 {name=originalItem.GetProperty("id").GetString()!;using var transformed=JsonDocument.Parse(Encode(D(("Name",name),("Raw",originalItem.GetProperty("strokes").EnumerateArray().Select(s=>s.EnumerateArray().Select(p=>new float[]{p[0].GetSingle(),p[1].GetSingle()}).ToArray()).ToArray()),("Resolution",0))));item=transformed.RootElement.Clone();}
 else name=item.GetProperty("Name").GetString()!;
 var post=Post(item);
 if(post.CopySnapshots()[0].CopyDescriptors().Length!=1){refused.Add(name);continue;}
 var final=LegacyEnglish119FinalizedSingleSource.Prepare(post).Run();var local=LegacyEnglish119MainLocalCandidates.Prepare(final,table).Run();
 var knowledge=LegacyEnglish119MainKnowledge.Create(local);var masks=LegacyEnglishFreshCaseMainSource.Prepare(knowledge,numeric,caseData,publication);
 string before=SourceDigest(final);var scorer=LegacyEnglishFreshCaseMainScorer.Prepare(masks);
 var groups=new List<object>();var graph=new List<object>();var seen=new HashSet<int>();
 for(int node=0;node<local.NodeCount;node++)
 {
  var arcs=local.CopyNode(node);graph.Add(arcs.Select(a=>new int[]{a.GroupId,a.Span}).ToArray());
  for(int a=0;a<arcs.Length;a++)
  {
   totalArcs++;if(!seen.Add(arcs[a].GroupId))continue;
   var d=scorer.DiagnoseArc(node,a);var f=d.Features;var produced=scorer.ScoreArc(node,a);
   if(!produced.CopyCosts().SequenceEqual(d.Result.CopyCosts()))throw new Exception("Cached production/diagnostic costs changed.");
   groups.Add(D(("Group",f.Group),("Span",arcs[a].Span),("Stored",Ranges(f.Stored)),("StoredStartKeys",f.Stored.Select(r=>{var k=local.ResolveCutPoint(r.StrokeIndex,r.StartPoint).Key;return new int[]{k.First,k.Second,k.Third};}).ToArray()),("Ordered",Ranges(f.Ordered)),("Order",f.Order),("Normalized",Points(f.Normalized)),("Merged",Points(f.Merged)),("RetainedPenUps",f.RetainedPenUps),
    ("Inputs",f.Inputs),("Boundary",new short[]{f.Boundary.Start,f.Boundary.End}),("Dynamic",new float[]{f.Dynamic.Overlap,f.Dynamic.Mode1Weight,f.Dynamic.Mode2Weight}),("ResampledPointCount",f.Dynamic.ResampledPointCount),
    ("SourceCount",f.SourceCount),("Degenerate",f.IsDegenerate),("WholePhysicalStroke",f.WholePhysicalStroke),("EndpointKeysZero",f.EndpointKeysZero),("SourceProbability",f.SourceProbability),
    ("Networks",d.Networks.Select(n=>D(("Slot",n.Slot),("HiddenDots",n.HiddenDots),("Hidden",n.Hidden),("Raw",n.Raw),("Probabilities",n.Probabilities),("StabilizationOffset",n.StabilizationOffset))).ToArray()),
    ("NeuralCosts",d.NeuralCosts),("Costs",produced.CopyCosts()),("Candidates",produced.CopyCandidates().Select(c=>new int[]{c.Symbol,c.Cost}).ToArray()),("FallbackCost",produced.FallbackCost),("PowerScale",produced.PowerScale),("NegativeBaselineCost",produced.NegativeBaselineCost)));
  }
 }

 LegacyEnglishFreshCaseMainSource Fresh()=>LegacyEnglishFreshCaseMainSource.Prepare(knowledge,numeric,caseData,publication);
 var coordinates=new List<(int Node,byte Span)>();for(int node=0;node<local.NodeCount;node++)foreach(byte span in local.CopyNode(node).Select(a=>a.Span).Distinct().Order())coordinates.Add((node,span));
 var first=coordinates[0];
 if(!tested)
 {
  foreach(string stage in new[]{"case:preflight","case:maps","case:enumerated","case:complete"})
  {using var cts=new CancellationTokenSource();Guard(()=>LegacyEnglishFreshCaseMainSource.PrepareCore(knowledge,numeric,caseData,publication,t=>{if(t==stage)cts.Cancel();},cts.Token));cancels++;}
  Guard(()=>LegacyEnglishFreshCaseMainScorer.Prepare(masks));if(masks.IsFaulted)throw new Exception("Failed second claim poisoned source.");
  foreach(string stage in new[]{"score:claimed","score:model","score:ready"})
  {var src=Fresh();using var cts=new CancellationTokenSource();Guard(()=>LegacyEnglishFreshCaseMainScorer.PrepareCore(src,t=>{if(t==stage)cts.Cancel();},cts.Token));cancels++;if(!src.IsFaulted)throw new Exception("Entered scorer preparation did not poison.");}
  foreach(string stage in new[]{"score:enter","score:features","score:network0","score:network1","score:network2","score:network3","score:ensemble","score:cache","score:publish"})
  {var src=Fresh();var sc=LegacyEnglishFreshCaseMainScorer.Prepare(src);using var cts=new CancellationTokenSource();Guard(()=>sc.ScoreArcCore(first.Node,0,t=>{if(t==stage)cts.Cancel();},cts.Token));cancels++;if(!src.IsFaulted)throw new Exception("Entered score cancellation did not poison.");Guard(()=>sc.ScoreArc(first.Node,0));}
  foreach(string stage in new[]{"owner:claimed","owner:ready"})
  {var src=Fresh();var sc=LegacyEnglishFreshCaseMainScorer.Prepare(src);using var cts=new CancellationTokenSource();Guard(()=>LegacyEnglishFreshCaseMainSpanSession.PrepareCore(sc,new(4096,16384),t=>{if(t==stage)cts.Cancel();},cts.Token));cancels++;if(!src.IsFaulted)throw new Exception("Entered D0 preparation did not poison.");}
  foreach(string stage in new[]{"span:entered","source:bytes","source:expanded","source:refreshed","source:forwarded","span:prepared","span:snapshot","span:installed"})
  {
   var src=Fresh();var sc=LegacyEnglishFreshCaseMainScorer.Prepare(src);var ss=LegacyEnglishFreshCaseMainSpanSession.Prepare(sc,new(4096,16384));var prior=ss.PrepareSpan(first.Node,first.Span);var old=prior.CopyPermissions();using var cts=new CancellationTokenSource();
   Guard(()=>ss.PrepareSpanCore(first.Node,first.Span,false,t=>{if(t==stage)cts.Cancel();},cts.Token));cancels++;
   if(!src.IsFaulted||src.EnteredPreparations!=2||src.ConsumedSelectedRows!=2*local.CopyNode(first.Node).Count(a=>a.Span==first.Span)||prior.IsCurrent||!old.SequenceEqual(prior.CopyPermissions()))throw new Exception("Entered D0 prefix/cancellation mismatch.");Guard(()=>ss.PrepareSpan(first.Node,first.Span));
  }
  var low=Fresh();var lowSc=LegacyEnglishFreshCaseMainScorer.Prepare(low);var limited=LegacyEnglishFreshCaseMainSpanSession.Prepare(lowSc,new(1,0));Guard(()=>limited.PrepareSpan(first.Node,first.Span));if(low.IsFaulted||low.EnteredPreparations!=0)throw new Exception("Budget preentry consumed state.");
  Guard(()=>scorer.ScoreArc(-1,0));Guard(()=>scorer.ScoreArc(first.Node,-1));
  var concurrently=Fresh();var concurrentSc=LegacyEnglishFreshCaseMainScorer.Prepare(concurrently);
  _=concurrentSc.ScoreArcCore(first.Node,0,t=>{if(t=="score:enter"){Guard(()=>concurrentSc.ScoreArc(first.Node,0));_=concurrently.CopyWorkingMap();}},default);
  if(concurrently.IsFaulted)throw new Exception("Concurrent preentry guard poisoned ongoing score.");
  // Populate shared matrices via case first, then validate independent origin on every call.
  var independent=LegacyEnglishMainOnlySourceMasks.Create(knowledge,numeric);_=LegacyEnglishMainOnlyAcousticScorer.Prepare(independent);
  var foreign=LegacySource247WireLayout.Read(bytes);var foreignNumeric=LegacyEnglishLkFreshNumericBinding.Create(LegacyEnglishLkSparseLanguageData.Read(LegacyEnglishLkPackedData.Read(foreign,lk),lk));Guard(()=>LegacyEnglishFreshCaseMainSource.Prepare(knowledge,foreignNumeric,caseData,publication));
  using(var reached=new ManualResetEventSlim())using(var release=new ManualResetEventSlim())
  {
   var src=Fresh();var sc=LegacyEnglishFreshCaseMainScorer.Prepare(src);var task=Task.Run(()=>sc.ScoreArcCore(first.Node,0,t=>{if(t=="score:enter"){reached.Set();if(!release.Wait(TimeSpan.FromSeconds(15)))throw new Exception("Score concurrency barrier.");}},default));
   if(!reached.Wait(TimeSpan.FromSeconds(15)))throw new Exception("Score entry barrier.");Guard(()=>sc.ScoreArc(first.Node,0));release.Set();task.GetAwaiter().GetResult();if(src.IsFaulted)throw new Exception("Concurrent score refusal poisoned source.");
  }
  using(var reached=new ManualResetEventSlim())using(var release=new ManualResetEventSlim())
  {
   var src=Fresh();var sc=LegacyEnglishFreshCaseMainScorer.Prepare(src);var ss=LegacyEnglishFreshCaseMainSpanSession.Prepare(sc,new(4096,16384));var task=Task.Run(()=>ss.PrepareSpanCore(first.Node,first.Span,false,t=>{if(t=="source:expanded"){reached.Set();if(!release.Wait(TimeSpan.FromSeconds(15)))throw new Exception("D0 concurrency barrier.");}},default));
   if(!reached.Wait(TimeSpan.FromSeconds(15)))throw new Exception("D0 entry barrier.");Guard(()=>ss.PrepareSpan(first.Node,first.Span));Guard(()=>sc.ScoreArc(first.Node,0));_=src.CopyWorkingMap();release.Set();var receipt=task.GetAwaiter().GetResult();if(src.IsFaulted||!receipt.IsCurrent||src.EnteredPreparations!=1||src.ConsumedSelectedRows!=local.CopyNode(first.Node).Count(a=>a.Span==first.Span))throw new Exception("Concurrent D0 refusal changed state.");
   var costs=receipt.CopySelectedRows();if(costs.Length>0){var c=costs[0].CopyCosts();int old=c[0];c[0]^=1;if(costs[0].CopyCosts()[0]!=old)throw new Exception("Cost alias.");costs[0]=null!;if(receipt.CopySelectedRows()[0]is null)throw new Exception("Row array alias.");}
   var raw=receipt.CopyRawPermissions();raw[0]^=1;if(raw.SequenceEqual(receipt.CopyRawPermissions()))throw new Exception("Raw permission alias.");var ex=receipt.CopyPermissions();ex[0]^=1;if(ex.SequenceEqual(receipt.CopyPermissions()))throw new Exception("Expanded permission alias.");
   var w=receipt.CopyWorkingMap();w[0]^=1;if(w.SequenceEqual(receipt.CopyWorkingMap()))throw new Exception("Map alias.");var q=receipt.CopyEligibility();q[0]^=1;if(q.SequenceEqual(receipt.CopyEligibility()))throw new Exception("Qualifier alias.");
   _=ss.PrepareSpan(first.Node,first.Span);if(receipt.IsCurrent)throw new Exception("Stale receipt retained authority.");Guard(()=>ss.PrepareSpanCore(first.Node,first.Span,true,null,default));
   var bogusMap=new LegacyPreparedDictionaryWorkingMap(217,src.DescriptorFlags,src.CopyPreservedMap(),src.CopyAuxiliaryMap());var fake=LegacyEnglishFreshCaseMainSource.D0Lease.Issue(src,ss,bogusMap,src.EnteredPreparations);Guard(()=>src.ValidateLease(fake));
  }
  tested=true;
 }
 var spanOwner=LegacyEnglishFreshCaseMainSpanSession.Prepare(scorer,new(4096,16384));var passes=new List<object>();
 var source68Before=masks.CopyInitialSource68();
 foreach(var (node,span) in coordinates.Concat(coordinates.AsEnumerable().Reverse()).Concat(new[]{(0,(byte)7),first}))
 {
  var receipt=spanOwner.PrepareSpan(node,span);if(!receipt.IsCurrent)throw new Exception("Completed receipt missing current identity.");
  passes.Add(D(("Anchor",node),("Span",span),("Epoch",receipt.Epoch),("Computed",receipt.ComputedRows),("Reused",receipt.ReusedRows),("Selected",receipt.CopySelectedRows().Select(z=>D(("Ordinal",z.ArcIndex),("Group",z.GroupId),("Costs",z.CopyCosts()))).ToArray()),("RawPermissions",receipt.CopyRawPermissions()),("Permissions",receipt.CopyPermissions()),("Eligibility",receipt.CopyEligibility()),("Working",receipt.CopyWorkingMap())));
  totalPasses++;totalSelected+=receipt.SelectedRowCount;
 }
 if(!source68Before.SequenceEqual(masks.CopyInitialSource68()))throw new Exception("source68 reset initial context map.");
 if(before!=SourceDigest(final))throw new Exception("Source720 changed.");
 totalGroups+=groups.Count;rows.Add(D(("Name",name),("Baseline",local.Baseline),("Height",local.Height),("CutInk",Floats(local.CopyCutStrokes())),("CutCounts",local.CopyCutCounts()),("Graph",graph),("Groups",groups),("CaseMasks",D(("Tokens",masks.CopyEnumerationTokens()),("Seed",masks.CopyControllerSeed()),("Primary",masks.CopyPrimaryMask()),("Secondary",masks.CopySecondaryMask()),("Preserved",masks.CopyPreservedMap()),("Auxiliary",masks.CopyAuxiliaryMap()),("InitialSource68",masks.CopyInitialSource68()),("Nodes",masks.CopyInitialContextNodes().Select(n=>new long[]{(long)n.Identity,(long)n.ParentLink,(long)n.ChildTreeRoot,(long)n.Left,(long)n.Right,n.Symbol,n.Balance,n.PageIndex,n.PageSlot}).ToArray()),("Pages",masks.CopyInitialPageCounts()),("Root",masks.InitialRootIdentity),("Descriptor",masks.DescriptorFlags))),("Passes",passes),("Source720",before),("LogicalReservationBytes",scorer.LogicalCopiedPayloadEnvelopeBytes)));
 Console.WriteLine(name+" groups="+groups.Count);
}
if(inputs.RootElement.ValueKind==JsonValueKind.Array){if(actualItems.Length!=141||rows.Count!=134||refused.Count!=7)throw new Exception("Original blind141 owner denominator changed.");}
else if(actualItems.Length!=100||rows.Count!=100||refused.Count!=0)throw new Exception("PluColl100 owner denominator changed.");
using(var captureFile=File.Create(args[2]))using(var writer=new Utf8JsonWriter(captureFile)){Write(writer,D(("InputSha256",Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(args[1])))),("Scope","Actual fresh one13B case M/N/D0/source68 component; not complete290 query or worddecoder"),("TargetsRead",false),("Rows",rows),("MultilineRefused",refused),("PassCount",totalPasses),("SelectedRowCount",totalSelected),("GroupCount",totalGroups),("ArcCount",totalArcs),("Guards",guards),("Cancellations",cancels),("PreparedAssemblyControls",preparedControls),("CalibrationCatalog",catalog),("PreparedCalibrationControls",calibrationControls),("CalibrationGuards",calibrationGuards)));}
Console.WriteLine($"PASS MAIN rows{rows.Count} multiline{refused.Count} groups{totalGroups} arcs{totalArcs} seconds{timing.Elapsed.TotalSeconds:F2}");
static string Encode(object value){using var stream=new MemoryStream();using(var writer=new Utf8JsonWriter(stream))Write(writer,value);return System.Text.Encoding.UTF8.GetString(stream.ToArray());}
static Dictionary<string,object?> D(params (string Name,object? Value)[] values)=>values.ToDictionary(v=>v.Name,v=>v.Value);
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
