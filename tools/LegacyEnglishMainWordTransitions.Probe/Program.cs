using System.Buffers.Binary;
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
InkStroke[] Ink(JsonElement e)=>e.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
LegacyEnglish119PlainTextResult Recognize(LegacyEnglish119SourceSubmission s)
{var c=LegacyOwnedEnglish119Candidates.Prepare(s.Preparation,table);var cache=LegacyOwnedEnglish119AcousticCache.Prepare(c);var confidence=LegacyOwnedEnglish119GlyphConfidence.Prepare(cache);var costs=LegacyOwnedEnglish119ControllerCosts.Prepare(confidence,table);var primary=LegacyOwnedEnglish119FirstPrimary.Prepare(costs,table).Run();var contexts=LegacyOwnedEnglish119ContextPasses.Prepare(primary).Run();return LegacyOwnedEnglish119PlainText.Materialize(LegacyOwnedEnglish119Terminal.Select(contexts),new(new byte[120],new byte[40]));}
LegacyEnglish119ParentPostprocessing Post(JsonElement item,LegacyFirstAcoustic119WireModel? selected=null)
{var p=LegacyEnglish119ParentSubmission.Prepare(selected??model,Ink(item.GetProperty("Raw")),item.GetProperty("Resolution").GetSingle(),m30,m50);p.Publish(p.Children.Select(c=>Recognize(c.Submission)).ToArray());return LegacyEnglish119ParentPostprocessing.Prepare(p,caseData,publication).Run();}

using var inputs=JsonDocument.Parse(File.ReadAllBytes(args[1]));var rows=new List<object>();var refused=new List<string>();int guards=0,cancellations=0;bool guarded=false;var prefixes=new List<object>();
void Guard(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or OperationCanceledException or NotSupportedException or InvalidDataException){guards++;return;}throw new Exception("Expected strict transition refusal.");}
var items=inputs.RootElement.ValueKind==JsonValueKind.Array?inputs.RootElement.EnumerateArray().ToArray():inputs.RootElement.GetProperty("samples").EnumerateArray().ToArray();
foreach(var item0 in items)
{
 JsonElement item=item0;string name;
 if(inputs.RootElement.ValueKind==JsonValueKind.Array)name=item.GetProperty("Name").GetString()!;
 else
 {
  name=item0.GetProperty("id").GetString()!;
  using var normalized=JsonDocument.Parse(Encode(D(("Name",name),("Raw",item0.GetProperty("strokes").EnumerateArray().Select(s=>s.EnumerateArray().Select(p=>new float[]{p[0].GetSingle(),p[1].GetSingle()}).ToArray()).ToArray()),("Resolution",0))));item=normalized.RootElement.Clone();
 }
 var post=Post(item);if(post.CopySnapshots()[0].CopyDescriptors().Length!=1){refused.Add(name);continue;}
 var final=LegacyEnglish119FinalizedSingleSource.Prepare(post).Run();var local=LegacyEnglish119MainLocalCandidates.Prepare(final,table).Run();
 var masks=LegacyEnglishMainOnlySourceMasks.Create(LegacyEnglish119MainKnowledge.Create(local),numeric);var scorer=LegacyEnglishMainOnlyAcousticScorer.Prepare(masks);var query=LegacyEnglishSparseTerminalQuery.Create(masks);
 var binding=LegacyEnglishFreshDictionaryQuerySession.Prepare(scorer,query,new(4096,4096,16384,1));
 var policy=LegacyEnglishFreshMainWordPolicy.Prepare(binding);var profile=LegacyEnglishMainWordTransitionProfile.Prepare(policy);var transitions=LegacyEnglishMainWordTransitions.Prepare(profile);
 var requests=new List<object>();var categories=profile.ReadCategories();if(!layout.Alphabet.TryGetLabel("a",out ushort letter))throw new Exception("Actual AL ordinary control label absent.");
 LegacyNativeWordRecombinationRequest Request(int n,int a,int prev,int priorArc,int endingArc,byte kind,int incoming=17)
 {
  byte[] residue=Enumerable.Range(0,80).Select(i=>(byte)(0x40+i)).ToArray();var cell=new LegacyNativeWordTransitionCostCell(1,incoming);byte span=local.CopyNode(n)[a].Span;
  if(prev<0)return LegacyNativeWordStartupRequestFactory.Create((uint)n,span,(ushort)a,letter,0,0,0,kind,binding.DescriptorFlags,0,categories,cell,incoming,residue);
  int ending=n-span-1;byte[] parent=new byte[88];BinaryPrimitives.WriteUInt32LittleEndian(parent.AsSpan(24),(uint)(prev|(priorArc<<22)));BinaryPrimitives.WriteUInt16LittleEndian(parent.AsSpan(28),(ushort)endingArc);BinaryPrimitives.WriteUInt16LittleEndian(parent.AsSpan(36),letter);BinaryPrimitives.WriteInt32LittleEndian(parent.AsSpan(20),31);
  return LegacyNativeWordParentRequestFactory.Create((uint)n,span,ending,parent,(ushort)a,letter,0,0,0,kind,binding.DescriptorFlags,binding.DescriptorFlags,0,categories,cell,incoming,residue);
 }
 void Record(LegacyNativeWordRecombinationRequest request,bool pair,string prepared)
 {byte[] before=request.GetSnapshot().ToArray();int cost=request.Cost.Value;bool accepted=pair?transitions.Validate140(request):transitions.Calculate118(request);requests.Add(D(("Prepared",true),("Control",prepared),("Pair",pair),("Before",Convert.ToBase64String(before)),("After",Convert.ToBase64String(request.GetSnapshot().Span)),("Incoming",cost),("Outgoing",request.Cost.Value),("Accepted",accepted)));}
 var graph=new List<object>();var states=new List<LegacyEnglishMainWordTransitionState[]>();var groups=new SortedDictionary<int,object>();
 for(int group=0;group<local.GroupCount;group++)groups[group]=local.CopyGroupRanges(group);
 for(int n=0;n<local.NodeCount;n++)
 {
  var arcs=local.CopyNode(n);graph.Add(arcs.Select(a=>D(("GroupId",a.GroupId),("Span",a.Span),("Flags",a.Flags),("StartContextId",a.StartContextId),("EndContextId",a.EndContextId),("Previous",a.PredecessorArcIndices.ToArray()),("Next",a.Successors.ToArray()))).ToArray());
  states.Add(arcs.Select((a,i)=>transitions.CopyState(n,i)).ToArray());
  for(int a=0;a<arcs.Length;a++)
  {
   int ending=n-arcs[a].Span-1;
   if(ending<0)Record(Request(n,a,-1,0,0,0),true,"actual-initial-numeric-only");
   else
   {
    int last=arcs[a].PredecessorArcIndices.Span[0];
    Record(Request(n,a,ending,last,last,0),false,"ordinary0-gap-numeric-only");Record(Request(n,a,ending,last,last,2),false,"ordinary2-gap-numeric-only");
    Record(Request(n,a,ending,last,last,0),true,"same-source-pair-numeric-only");
    Record(Request(n,a,ending,last,last,0,int.MaxValue),true,"signed-wrap-pair-numeric-only");
    if(ending>0)Record(Request(n,a,0,0,last,0),true,"source-ending-union-numeric-only");
   }
  }
  foreach(var a in arcs)if(!groups.ContainsKey(a.GroupId))groups[a.GroupId]=local.CopyGroupRanges(a.GroupId);
 }
 rows.Add(D(("Name",name),("Policy",D(("Scope",policy.Scope),("AcousticSourceFlags",policy.AcousticSourceFlags),("MaximumSourceArcs",policy.MaximumSourceArcs),("AuxiliarySourceArcs",policy.AuxiliarySourceArcs),("MaximumParentDistance54",policy.MaximumParentDistance54),("FlagPrepassDistance58",policy.FlagPrepassDistance58),("KnowledgeF5",policy.KnowledgeF5),("Graph1Dc",policy.Graph1Dc),("Permission2309",policy.Permission2309),("Summary",policy.Summary),("Refresh",policy.Refresh),("Configuration",policy.Configuration))),("Profile",D(("PairEnabled",profile.PairEnabled),("PairScale",profile.PairScale),("PairOffset",profile.PairOffset),("PairMaximum",profile.PairMaximum),("CrossLineCost",profile.CrossLineCost),("GapOffset",profile.GapOffset),("DefaultGap",profile.DefaultGap),("BlankThreshold",profile.BlankThreshold),("TranslationThreshold",profile.TranslationThreshold),("GapSlope",profile.GapSlope),("SourceGapScale",profile.SourceGapScale),("SourceMatrixScale",profile.SourceMatrixScale),("JoinMaximumCost",profile.JoinMaximumCost),("MatrixMaximumCost",profile.MatrixMaximumCost),("BoundaryMaximum118",profile.BoundaryMaximum118),("BoundaryThreshold11C",profile.BoundaryThreshold11C),("Matrix",profile.CopyMatrix()))),("Descriptor",Convert.ToBase64String(final.CopyDescriptor())),("Guide",Convert.ToBase64String(final.CopyGuide())),("Gap",post.Gap!.Output.SourceGap),("Cuts",local.CopyCutRecords()),("Counts",local.CopyCutCounts()),("Graph",graph),("Groups",groups),("PreparedGroupBounds",transitions.CopyPreparedGroupBounds()),("States",states),("Requests",requests),("Budget",transitions.LogicalAllocationEnvelope)));
 if(!guarded)
 {
  guarded=true;var pristine=Request(0,0,-1,0,0,0);using(var cancel=new CancellationTokenSource()){cancel.Cancel();Guard(()=>transitions.Validate140(pristine,cancel.Token));if(transitions.IsFaulted)throw new Exception("Preentry cancellation poisoned component.");}
  foreach(string stage in new[]{"transition:entered","transition:validated","transition:cost","transition:accumulated"})
  {
   var instance=LegacyEnglishMainWordTransitions.Prepare(profile);var request=Request(0,0,-1,0,0,0);byte[] before=request.GetSnapshot().ToArray();int incoming=request.Cost.Value;using var cancel=new CancellationTokenSource();Guard(()=>instance.ExecutePrepared(request,true,t=>{if(t==stage)cancel.Cancel();},cancel.Token));cancellations++;prefixes.Add(D(("Prepared",true),("Name",name),("Stage",stage),("Before",Convert.ToBase64String(before)),("After",Convert.ToBase64String(request.GetSnapshot().Span)),("Incoming",incoming),("Outgoing",request.Cost.Value),("Faulted",instance.IsFaulted)));if(!instance.IsFaulted)throw new Exception("Entered cancellation must poison component.");Guard(()=>instance.Validate140(request));
  }
  var serialized=LegacyEnglishMainWordTransitions.Prepare(profile);var first=Request(0,0,-1,0,0,0);
  _=serialized.ExecutePrepared(first,true,t=>{if(t=="transition:entered"){Guard(()=>serialized.Validate140(Request(0,0,-1,0,0,0)));Task.Run(()=>Guard(()=>serialized.Validate140(Request(0,0,-1,0,0,0)))).GetAwaiter().GetResult();}});
  if(serialized.IsFaulted)throw new Exception("Rejected concurrent/reentrant caller poisoned active owner.");
  foreach(byte groupFlag in new byte[]{1,2,4,8,255})Guard(()=>LegacyEnglish119MainWordSource.RequirePreparedMetadata(groupFlag,0));
  foreach(byte groupKind in new byte[]{1,2,6,255})Guard(()=>LegacyEnglish119MainWordSource.RequirePreparedMetadata(0,groupKind));
  LegacyEnglish119MainWordSource.RequirePreparedMetadata(0,0);
  var mutable=profile.CopyMatrix();mutable[0]=float.NaN;if(!float.IsFinite(profile.CopyMatrix()[0]))throw new Exception("Profile snapshot alias.");
  var snap=transitions.CopyState(0,0);snap.CategoryMatrix[0]=int.MinValue;if(transitions.CopyState(0,0).CategoryMatrix[0]==int.MinValue)throw new Exception("Cache snapshot alias.");guards+=2;
  //Equal-byte foreign source instance: its own policy is valid but never
  //authorizes this local graph. Actual source receipt constructor also checks
  //same graph/current ink/table identity before numerical work.
  var layout2=LegacySource247WireLayout.Read(bytes);var model2=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(bytes,layout2,layout2.Children.First(c=>c.Kind==0x249));
  var language2=LegacyEnglishLkSparseLanguageData.Read(LegacyEnglishLkPackedData.Read(layout2,lk),lk);var numeric2=LegacyEnglishLkFreshNumericBinding.Create(language2);
  var final2=LegacyEnglish119FinalizedSingleSource.Prepare(Post(item,model2)).Run();var local2=LegacyEnglish119MainLocalCandidates.Prepare(final2,table).Run();var masks2=LegacyEnglishMainOnlySourceMasks.Create(LegacyEnglish119MainKnowledge.Create(local2),numeric2);var binding2=LegacyEnglishFreshDictionaryQuerySession.Prepare(LegacyEnglishMainOnlyAcousticScorer.Prepare(masks2),LegacyEnglishSparseTerminalQuery.Create(masks2),new(4096,4096,16384,1));
  var profile2=LegacyEnglishMainWordTransitionProfile.Prepare(LegacyEnglishFreshMainWordPolicy.Prepare(binding2));Guard(()=>local.RetainWordSource(profile2,default));
  var actualSource=local.RetainWordSource(profile,default);var fakeGraph=new LegacyOrdinaryCursiveCandidateGraph([[new(0,0,0,0,0,new int[]{-1},[])]],[new(0,new int[]{0},new LegacyInkRange[]{new(0,0,1)})],8);
  Guard(()=>LegacyEnglish119MainWordSource.Create(local,profile,fakeGraph,LegacyCursivePreparedInk.Create(final.CopyStrokes()),table,post.Gap!.Output.SourceGap,default));
  if(local.IsFaulted||binding.IsFaulted||local2.IsFaulted||binding2.IsFaulted)throw new Exception("Preentry foreign identity refusal mutated owners.");
  var unclaimed=binding.CopyPreparationSnapshot();if(unclaimed.Operations!=0||unclaimed.AttemptedInsertions!=0||unclaimed.OutputRows!=0)throw new Exception("Numerical component acquired query authority.");guards++;
  foreach(int at in new[]{0,12,28,32,64})
  {
   var instance=LegacyEnglishMainWordTransitions.Prepare(profile);var bad=Request(0,0,-1,0,0,0);bad.WriteByte(at,255);Guard(()=>instance.Validate140(bad));if(!instance.IsFaulted)throw new Exception("Reached unsupported mutation must poison component.");
  }
 }
 Console.WriteLine(name+" "+local.NodeCount+"/"+local.CandidateCount);
}
File.WriteAllText(args[2],Encode(D(("Scope","actual English fresh OUTER and MAIN ordinary transition component; no word decoder"),("InputSha256",Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(args[1])))),("TargetsRead",false),("Rows",rows),("Refused",refused),("Guards",guards),("Cancellations",cancellations),("CanceledPrefixes",prefixes))));
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
 case LegacyEnglishMainWordTransitionState v:Write(w,D(("GroupId",v.GroupId),("Bounds",v.Bounds),("FutureMinimumX",v.FutureMinimumX),("PrefixMaximumX",v.PrefixMaximumX),("JoinCost",v.JoinCost),("TripleJoinCost",v.TripleJoinCost),("BlankCost",v.BlankCost),("CategoryMatrix",v.CategoryMatrix)));break;
 case LegacyFirstTransitionBounds v:Write(w,D(("MinimumX",v.MinimumX),("MinimumY",v.MinimumY),("MaximumX",v.MaximumX),("MaximumY",v.MaximumY)));break;
 case LegacyCursivePreparedPoint v:Write(w,D(("X",v.X),("Y",v.Y),("DeltaX",v.DeltaX),("DeltaY",v.DeltaY),("SegmentLength",v.SegmentLength),("Tag",v.Tag),("StrokeIndex",v.StrokeIndex),("OriginalPointIndex",v.OriginalPointIndex)));break;
 case LegacyInkRange v:Write(w,D(("StrokeIndex",v.StrokeIndex),("StartPoint",v.StartPoint),("EndPointExclusive",v.EndPointExclusive)));break;
 case LegacyCursiveCandidateSuccessor v:Write(w,D(("NodeOffset",v.NodeOffset),("ArcIndex",v.ArcIndex)));break;
 case LegacyNativeWordSourceSummary v:Write(w,D(("PackedSource22e8",v.PackedSource22e8),("SubwordSourceIndex22ec",v.SubwordSourceIndex22ec),("OverrideSourceIndex22f0",v.OverrideSourceIndex22f0)));break;
 case LegacyNativeWordScalarRefreshResult v:Write(w,D(("PackedSource22e8",v.PackedSource22e8),("ConsecutiveBlank22f7",v.ConsecutiveBlank22f7),("KnowledgeOption22f8",v.KnowledgeOption22f8),("SourceFormat22f9",v.SourceFormat22f9),("MaximumWordLength68",v.MaximumWordLength68),("KnowledgeMarkerA0",v.KnowledgeMarkerA0)));break;
 case LegacyNativeWordControllerConfiguration v:Write(w,D(("HeaderCount2400",v.HeaderCount2400),("PrimaryAlternativeCount2402",v.PrimaryAlternativeCount2402),("SecondaryAlternativeCount2404",v.SecondaryAlternativeCount2404),("PrimaryArcLimit230a",v.PrimaryArcLimit230a),("ExpansionFactor230c",v.ExpansionFactor230c),("Mode22f6",v.Mode22f6),("PackedSource22e8",v.PackedSource22e8),("Trace2368",v.Trace2368),("SuppressPreviousContext2609",v.SuppressPreviousContext2609),("OverrideHeap2458",v.OverrideHeap2458)));break;
 case System.Collections.IDictionary dict:w.WriteStartObject();foreach(System.Collections.DictionaryEntry item in dict){w.WritePropertyName(Convert.ToString(item.Key,System.Globalization.CultureInfo.InvariantCulture)!);Write(w,item.Value);}w.WriteEndObject();break;
 case System.Collections.IEnumerable list:w.WriteStartArray();foreach(var item in list)Write(w,item);w.WriteEndArray();break;
 default:throw new InvalidDataException("Explicit static JSON scalar required.");
 }
}
