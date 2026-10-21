using System.Text.Json;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using InkRecognition;
if(args.Length!=3)throw new ArgumentException("models blind-raw capture");
var m=args[0];var bytes=File.ReadAllBytes(Path.Combine(m,"en-US/en_US-ak-cur.lite.res"));var layout=LegacySource247WireLayout.Read(bytes);
var model=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(bytes,layout,layout.Children.First(c=>c.Kind==0x249));
var lk=File.ReadAllBytes(Path.Combine(m,"en-US/en_US-lk-text.lite.res"));
var packed=LegacyEnglishLkPackedData.Read(layout,lk);var language=LegacyEnglishLkSparseLanguageData.Read(packed,lk);var numeric=LegacyEnglishLkFreshNumericBinding.Create(language);
var foreign=LegacySource247WireLayout.Read(bytes);var foreignNumeric=LegacyEnglishLkFreshNumericBinding.Create(LegacyEnglishLkSparseLanguageData.Read(LegacyEnglishLkPackedData.Read(foreign,lk),lk));
var m30=File.ReadAllBytes(Path.Combine(m,"line-geometry/line-2e3-30.res"));var m50=File.ReadAllBytes(Path.Combine(m,"line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(m,"ru-RU/legacy-separation-profile.json")));
using var cf=File.OpenRead(Path.Combine(m,"text-unicode/legacy-case-data.json"));using var pf=File.OpenRead(Path.Combine(m,"text-unicode/legacy-text-publication-data.json"));var caseData=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
using var inputs=JsonDocument.Parse(File.ReadAllBytes(args[1]));var rows=new List<object>();var refused=new List<string>();int guards=0,cancels=0;
void Guard(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException or InvalidDataException){guards++;return;}throw new Exception("Expected refusal.");}
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
bool guarded=false;var preparedControls=new List<object>();var calibrationControls=new List<object>();
var catalog=D(("TemporalMin",LegacyDirectional2B1DefaultCalibration.CopyMinimum(false)),("TemporalMax",LegacyDirectional2B1DefaultCalibration.CopyMaximum(false)),("SpatialMin",LegacyDirectional2B1DefaultCalibration.CopyMinimum(true)),("SpatialMax",LegacyDirectional2B1DefaultCalibration.CopyMaximum(true)));
int calibrationGuards=0;
void CalibrationGuard(Action action){int before=guards;Guard(action);if(guards!=before+1)throw new Exception("Calibration refusal.");calibrationGuards++;}

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
 var masks=LegacyEnglishMainOnlySourceMasks.Create(LegacyEnglish119MainKnowledge.Create(local),numeric);
 string before=SourceDigest(final);var scorer=LegacyEnglishMainOnlyAcousticScorer.Prepare(masks);
 if(!guarded)
 {
  Guard(()=>LegacyEnglishMainOnlyAcousticScorer.Prepare(masks,new(true)));
  var selectedForeign=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(bytes,foreign,foreign.Children.First(c=>c.Kind==0x249));var foreignPost=Post(item,selectedForeign);
  var foreignFinal=LegacyEnglish119FinalizedSingleSource.Prepare(foreignPost).Run();var foreignLocal=LegacyEnglish119MainLocalCandidates.Prepare(foreignFinal,table).Run();var foreignMasks=LegacyEnglishMainOnlySourceMasks.Create(LegacyEnglish119MainKnowledge.Create(foreignLocal),foreignNumeric);
  var foreignOwnedModel=LegacyEnglishMainAcousticModel.Prepare(foreignMasks,default);
  if(ReferenceEquals(foreignOwnedModel,scorer.Model)||ReferenceEquals(foreignOwnedModel.Source,scorer.Model.Source))throw new Exception("Equal-byte foreign source identity conflated.");
  Guard(()=>new LegacyEnglishMainClassCacheKernel(scorer.Model,foreignMasks));Guard(()=>new LegacyEnglishMainClassCacheKernel(foreignOwnedModel,masks));
  Guard(()=>scorer.Model.Evaluate(0,new short[1],default));
  foreach(string stage in new[]{"prepare:preflight","prepare:model","prepare:ink"})
  {using var cts=new CancellationTokenSource();Guard(()=>LegacyEnglishMainOnlyAcousticScorer.PreparePrepared(masks,s=>{if(s==stage)cts.Cancel();},cts.Token));cancels++;}
  int testNode=Enumerable.Range(0,local.NodeCount).First(i=>local.CopyNode(i).Length!=0);
  foreach(string stage in new[]{"score:enter","score:features","score:network0","score:network1","score:network2","score:network3","score:ensemble","score:cache","score:publish"})
  {using var cts=new CancellationTokenSource();Guard(()=>scorer.ScoreArcPrepared(testNode,0,s=>{if(s==stage)cts.Cancel();},cts.Token));cancels++;if(scorer.CachedGroupCount!=0)throw new Exception("Canceled computation published cache entry.");}
  Guard(()=>scorer.ScoreArc(-1,0));Guard(()=>scorer.ScoreArc(local.NodeCount,0));Guard(()=>scorer.ScoreArc(testNode,-1));Guard(()=>scorer.ScoreArc(testNode,local.CopyNode(testNode).Length));
  foreach(int bad in new[]{-1,0,4097,int.MaxValue}){Guard(()=>LegacyEnglishMainInkSnapshot.Preflight(bad,1,1));Guard(()=>LegacyEnglishMainInkSnapshot.Preflight(1,bad,1));Guard(()=>LegacyEnglishMainInkSnapshot.Preflight(1,1,bad));}
  if(LegacyEnglishMainInkSnapshot.Preflight(4096,4096,4096)>16L*1024*1024)throw new Exception("Bound arithmetic drift.");
  using(var barrier=new System.Threading.Barrier(2))
  {
   var one=Task.Run(()=>scorer.ScoreArcPrepared(testNode,0,s=>{if(s=="score:publish"&&!barrier.SignalAndWait(TimeSpan.FromSeconds(5)))throw new Exception("Publication barrier.");},default));
   var two=Task.Run(()=>scorer.ScoreArcPrepared(testNode,0,s=>{if(s=="score:publish"&&!barrier.SignalAndWait(TimeSpan.FromSeconds(5)))throw new Exception("Publication barrier.");},default));Task.WaitAll(one,two);
   if(!ReferenceEquals(one.Result,two.Result)||scorer.CachedGroupCount!=1)throw new Exception("Concurrent publication did not share one immutable result.");
   var costsCopy=one.Result.CopyCosts();costsCopy[0]^=1;var originalCandidates=one.Result.CopyCandidates();var candidatesCopy=one.Result.CopyCandidates();if(candidatesCopy.Length!=0)candidatesCopy[0]=default;
   if(!originalCandidates.SequenceEqual(one.Result.CopyCandidates()))throw new Exception("Candidate snapshot alias.");
   if(costsCopy.SequenceEqual(one.Result.CopyCosts()))throw new Exception("Cost snapshot alias.");
   Guard(()=>scorer.ScoreArc(testNode,0,new(true)));
  }
  for(int control=0;control<16;control++)
  {
   int Pick(int i)=>new[]{int.MinValue,int.MaxValue,-65536,-32768,-1024,-1,0,1,128,1024,32767,65535}[Math.Abs((control+i)%12)];
   var global=Enumerable.Range(0,17).Select(i=>Pick(i)).ToArray();var windows=Enumerable.Range(0,16).Select(j=>Enumerable.Range(0,16).Select(i=>Pick(j*16+i)).ToArray()).ToArray();var baseline=Enumerable.Range(0,9).Select(i=>Pick(300+i)).ToArray();
   preparedControls.Add(D(("Name","prepared-saturated2B1-"+control),("Global",global),("Windows",windows),("Baseline",baseline),("Temporal",LegacyEnglishMainFeatureAssembly.Compose(global,windows,baseline,false,default)),("Spatial",LegacyEnglishMainFeatureAssembly.Compose(global,windows,baseline,true,default))));
  }
  var cg=new int[17];var cw=Enumerable.Range(0,16).Select(_=>new int[16]).ToArray();var cb=new int[9];
  var cm=LegacyDirectional2B1DefaultCalibration.CopyMinimum(false);var cx=LegacyDirectional2B1DefaultCalibration.CopyMaximum(false);
  CalibrationGuard(()=>LegacySaturatedDirectionalFeatureAssembly.Compose(cg.AsSpan(1),cw,cb,false,cm,cx));
  CalibrationGuard(()=>LegacySaturatedDirectionalFeatureAssembly.Compose(cg,cw.Take(15).ToArray(),cb,false,cm,cx));
  CalibrationGuard(()=>LegacySaturatedDirectionalFeatureAssembly.Compose(cg,cw,cb.AsSpan(1),false,cm,cx));
  CalibrationGuard(()=>LegacySaturatedDirectionalFeatureAssembly.Compose(cg,cw,cb,false,cm.AsSpan(1),cx));
  CalibrationGuard(()=>LegacySaturatedDirectionalFeatureAssembly.Compose(cg,cw,cb,false,cm,cx.AsSpan(1)));
  using(var ct=new CancellationTokenSource()){ct.Cancel();CalibrationGuard(()=>LegacySaturatedDirectionalFeatureAssembly.Compose(cg,cw,cb,false,cm,cx,ct.Token));}
  var untouched=LegacyDirectional2B1DefaultCalibration.CopyMinimum(false);cm[1]^=1;
  if(!untouched.SequenceEqual(LegacyDirectional2B1DefaultCalibration.CopyMinimum(false)))throw new Exception("Public catalog copy mutated private data.");
  for(int c=0;c<4;c++)
  {
   var g=Enumerable.Repeat(c==0?0:c==1?32767:c==2?-32768:int.MaxValue,17).ToArray();var w=Enumerable.Range(0,16).Select(_=>Enumerable.Repeat(c==0?1:c==1?65535:c==2?-65536:int.MinValue,16).ToArray()).ToArray();
   var lo=new short[274];var hi=new short[274];if(c==1){Array.Fill(lo,(short)32767);Array.Fill(hi,(short)-32768);}else if(c>=2){Array.Fill(lo,(short)-32768);Array.Fill(hi,(short)32767);} // Equal and negative ranges are literal native arithmetic, not coerced.
   calibrationControls.Add(D(("Global",g),("Windows",w),("Baseline",cb),("Minima",lo),("Maxima",hi),("Temporal",LegacySaturatedDirectionalFeatureAssembly.Compose(g,w,cb,false,lo,hi)),("Spatial",LegacySaturatedDirectionalFeatureAssembly.Compose(g,w,cb,true,lo,hi))));
  }
  guarded=true;
 }
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
 if(before!=SourceDigest(final))throw new Exception("Source720 changed.");
 totalGroups+=groups.Count;rows.Add(D(("Name",name),("Baseline",local.Baseline),("Height",local.Height),("CutInk",Floats(local.CopyCutStrokes())),("CutCounts",local.CopyCutCounts()),("Graph",graph),("Groups",groups),("Source720",before),("LogicalReservationBytes",scorer.LogicalCopiedPayloadEnvelopeBytes)));
 Console.WriteLine(name+" groups="+groups.Count);
}
if(inputs.RootElement.ValueKind==JsonValueKind.Array){if(actualItems.Length!=141||rows.Count!=134||refused.Count!=7)throw new Exception("Original blind141 owner denominator changed.");}
else if(actualItems.Length!=100||rows.Count!=100||refused.Count!=0)throw new Exception("PluColl100 owner denominator changed.");
File.WriteAllText(args[2],Encode(D(("InputSha256",Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(args[1])))),("Scope","Actual MAIN119 four107 family acoustic caches; no worddecoder or accepted text"),("TargetsRead",false),("Rows",rows),("MultilineRefused",refused),("GroupCount",totalGroups),("ArcCount",totalArcs),("Guards",guards),("Cancellations",cancels),("PreparedAssemblyControls",preparedControls),("CalibrationCatalog",catalog),("PreparedCalibrationControls",calibrationControls),("CalibrationGuards",calibrationGuards))));
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
