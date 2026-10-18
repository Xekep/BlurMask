using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
if(args.Length!=4)throw new ArgumentException("models-directory actual-controller-cost-capture controls.json output.json");
using var input=JsonDocument.Parse(File.ReadAllBytes(args[1]));using var controlInput=JsonDocument.Parse(File.ReadAllBytes(args[2]));var canceled=new CancellationToken(true);
int guards=0,actualCallbacks=0,actualBlankRows=0,actualSkipped=0,primaryRows=0;void Guard(Action action)
{try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or NotSupportedException or InvalidDataException or ProbeException){guards++;return;}throw new Exception("Expected refusal.");}
InkStroke[] Ink(JsonElement e)=>e.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
string path="en-US/en_US-ak-cur.lite.res";byte[] bytes=File.ReadAllBytes(Path.Combine(args[0],path));var parent=LegacySource247WireLayout.Read(bytes);
var model=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(bytes,parent,parent.Children.First(c=>c.Kind==0x249));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(args[0],"ru-RU/legacy-separation-profile.json")));
LegacyOwnedEnglish119FirstPrimary Make(InkStroke[] raw)
{
 var candidates=LegacyOwnedEnglish119Candidates.Prepare(LegacyOwnedEnglishLiteFullPreprocessing.Prepare(model,raw),table);
 var cache=LegacyOwnedEnglish119AcousticCache.Prepare(candidates);return LegacyOwnedEnglish119FirstPrimary.Prepare(LegacyOwnedEnglish119ControllerCosts.Prepare(LegacyOwnedEnglish119GlyphConfidence.Prepare(cache),table),table);
}
Call Snapshot(int stage,LegacyFirstAncestorTransitionRequest r,int incoming,LegacyFirstOrdinaryTransitionResult result)
{
 var p=r.Previous;var o=p.OriginalRecord;var g=p.GeometryRecord;
 return new(new[]{r.CurrentNode,(int)r.CurrentArc,r.CurrentSymbol,r.CurrentCategory,r.CurrentSpan,r.Kind,p.OriginalNode,p.OriginalAlternative,
  o.ArcOrdinal,o.Symbol,o.CompositeCategory,o.Span,o.PreviousAlternative,o.TotalCost,o.AcousticCost,o.PathBit?1:0,
  p.GeometryNode,p.GeometryAlternative,g.ArcOrdinal,g.Symbol,g.CompositeCategory,g.Span,g.PreviousAlternative,g.TotalCost,g.AcousticCost,g.PathBit?1:0,
  p.SkippedCount,incoming,result.Cost,result.Accepted?1:0,stage});
}
Row Capture(LegacyOwnedEnglish119FirstPrimary producer,List<Call> calls,LegacyFirstOrdinaryAcousticRecurrence primary,InkStroke[]? raw=null)
{
 var candidates=producer.Owner.Owner.Owner.Owner;var graph=producer.Policy.SourceRanges.Graph;var states=new List<State>();var nodes=new List<Arc[]>();
 for(int n=0;n<graph.NodeCount;n++)
 {
  var arcs=graph.GetNode(n);nodes.Add(arcs.Select((a,i)=>new Arc(i,a.Span,a.GroupId,a.PredecessorArcIndices.ToArray())).ToArray());
  for(int a=0;a<arcs.Count;a++){var b=producer.Geometry.ReadBounds(n,a);var s=producer.Geometry.CopyState(n,a);states.Add(new(n,a,b,s.Future,s.Prefix,s.Join,s.Blank,s.Matrix));}
 }
 return new((raw??candidates.Preparation.CopyOriginalStrokes()).Select(s=>s.Points.Select(p=>new[]{p.X,p.Y}).ToArray()).ToArray(),nodes,
  Enumerable.Range(0,producer.NodeCount).Select(producer.CopyRequests).ToArray(),states,calls,
  Enumerable.Range(0,primary.NodeCount).Select(n=>new Records(primary.GetRowCopy(n).Count,primary.GetRowCopy(n).CopyStorage())).ToArray(),
  producer.RequestCount,producer.FallbackProposalCount,candidates.NodeCount,candidates.GroupCount,producer.Owner.Owner.Owner.ScoredGroupCount,
  candidates.Preparation.Line.Height,candidates.CopyCutStrokes().Select(s=>s.Points.Select(p=>new[]{p.X,p.Y}).ToArray()).ToArray(),
  Enumerable.Range(0,graph.Groups.Count).Select(candidates.CopyGroupRanges).ToArray(),
  Enumerable.Range(0,producer.NodeCount).Select(producer.CopyClassRequests).ToArray());
}
var actual=new List<Row>();LegacyOwnedEnglish119FirstPrimary? first=null;bool failedPrefix=false;
foreach(var row in input.RootElement.GetProperty("Rows").EnumerateArray())
{
 var raw=Ink(row.GetProperty("Raw"));var producer=Make(raw);first??=producer;var p=producer.Policy;var cache=producer.Owner.Owner.Owner;
 if(p.PrimaryBeamWidth!=5||p.NativeModeE0||p.NativeProtocolExclusion194||p.ExcludeConsecutiveBlank||p.Blank!=217||p.Closed!=211||p.Shadda!=65535||p.Newline!=184||p.Space!=182||p.NonbreakingSpace!=183)
  throw new Exception("Actual native selected-source policy changed.");
 int scored=cache.ScoredGroupCount;var calls=new List<Call>();var observed=producer.RunWithObserver((stage,r,incoming,result)=>calls.Add(Snapshot(stage,r,incoming,result)));
 var result=producer.Run();if(!ReferenceEquals(producer,result.Owner)||!ReferenceEquals(result,producer.Run())||scored!=cache.ScoredGroupCount)throw new Exception("Primary detached source or repeated NN.");
 for(int n=0;n<result.Primary.NodeCount;n++)
 {
  var tableCopy=result.Primary.GetRowCopy(n);if(!tableCopy.CopyStorage().AsSpan().SequenceEqual(observed.Primary.GetRowCopy(n).CopyStorage()))throw new Exception("Observer changed primary rows.");
  primaryRows+=tableCopy.Count;for(int a=0;a<tableCopy.Count;a++)if(tableCopy.Get(a).Symbol==217)actualBlankRows++;
  if(tableCopy.Count>0){byte[] copy=tableCopy.CopyStorage();copy[0]^=255;if(result.Primary.GetRowCopy(n).CopyStorage()[0]==copy[0])throw new Exception("Mutable primary storage escaped.");}
 }
 actualCallbacks+=calls.Count;actualSkipped+=calls.Count(c=>c.Data[26]>0);actual.Add(Capture(producer,calls,result.Primary,raw));
 Guard(()=>producer.Run(canceled));Guard(()=>producer.RunWithObserver(null!));
 if(calls.Count>0)
 {
  Guard(()=>producer.RunWithObserver((_,_,_,_)=>throw new ProbeException()));
  using var cts=new CancellationTokenSource();Guard(()=>producer.RunWithObserver((_,_,_,_)=>cts.Cancel(),cts.Token));
 }
 if(!failedPrefix&&calls.Count>20)
 {
  var recovery=LegacyOwnedEnglish119FirstPrimary.Prepare(producer.Owner,table);int seen=0;
  Guard(()=>recovery.RunWithObserver((_,_,_,_)=>{if(++seen==20)throw new ProbeException();}));
  if(seen!=20)throw new Exception("Failure did not follow an actual callback prefix.");
  using var cts=new CancellationTokenSource();seen=0;
  Guard(()=>recovery.RunWithObserver((_,_,_,_)=>{if(++seen==20)cts.Cancel();},cts.Token));
  var recovered=recovery.Run();for(int n=0;n<result.Primary.NodeCount;n++)
   if(!recovered.Primary.GetRowCopy(n).CopyStorage().AsSpan().SequenceEqual(result.Primary.GetRowCopy(n).CopyStorage()))throw new Exception("Failed private prefix changed a subsequent valid run.");
  if(scored!=cache.ScoredGroupCount)throw new Exception("Failure recovery repeated NN.");failedPrefix=true;
 }
 Guard(()=>LegacyOwnedEnglish119FirstPrimary.Prepare(producer.Owner,table,canceled));Guard(()=>producer.CopyRequests(-1));
 var requests=producer.CopyRequests(0);if(requests.Length>0){var value=requests[0];requests[0]=default;if(producer.CopyRequests(0)[0]!=value)throw new Exception("Mutable requests escaped.");}
}
//Prepared requests use a genuine separate four-physical source/graph/geometry,
//but their costs/root header/category overrides are explicit numeric controls.
//They are never passed to production Prepare or described as actual candidates.
InkStroke V(float x)=>new(new InkPoint[]{new(x,10),new(x,30),new(x,60),new(x,90)});
var preparedOwner=Make(new[]{V(0),V(40),V(80),V(120)});var preparedGraph=preparedOwner.Policy.SourceRanges.Graph;
if(preparedGraph.NodeCount!=4)throw new Exception("Prepared four-atom graph changed.");
var prepared=new List<Prepared>();int preparedSkipped=0,preparedRootStops=0;
foreach(var control in controlInput.RootElement.EnumerateArray())
{
 bool header=control.GetProperty("rootHeader").GetBoolean();bool blank1=control.GetProperty("blank1").GetBoolean(),blank2=control.GetProperty("blank2").GetBoolean();
 int[] costs=control.GetProperty("costs").EnumerateArray().Select(e=>e.GetInt32()).ToArray();var requests=new LegacyFirstOrdinaryGlyphRequest[4][];
 for(int n=0;n<4;n++)
 {
  ushort symbol=n==0&&header?(ushort)182:n==1&&blank1||n==2&&blank2?(ushort)217:(ushort)10;
  int category=symbol==217?preparedOwner.Policy.Categories.StartCompositeMap.Span[217]:preparedOwner.Policy.Categories.StartCompositeMap.Span[10];
  ushort arc=(ushort)preparedGraph.GetNode(n).Select((a,i)=>(a,i)).First(x=>x.a.Span==0).i;
  requests[n]=[new(arc,0,symbol,(byte)category,costs[n],1)];
 }
 var calls=new List<Call>();var p=preparedOwner.Policy;
 var result=LegacyFirstPrimaryRecurrenceKernel.Run(requests,p.PrimaryBeamWidth,p.Blank,p.Replacement,p.Closed,p.Shadda,p.Newline,p.Space,p.NonbreakingSpace,p.ExcludeConsecutiveBlank,
  new(preparedGraph),preparedOwner.Geometry.Correct118,preparedOwner.Geometry.Correct140,(stage,r,incoming,result)=>calls.Add(Snapshot(stage,r,incoming,result)),default);
 preparedSkipped+=calls.Count(c=>c.Data[26]>0);preparedRootStops+=calls.Count(c=>c.Data[16]==0&&c.Data[19]==182);
 prepared.Add(new(requests,Enumerable.Range(0,4).Select(n=>new Records(result.GetRowCopy(n).Count,result.GetRowCopy(n).CopyStorage())).ToArray(),calls));
}
var fixture=Capture(preparedOwner,[],preparedOwner.Run().Primary);
Guard(()=>LegacyOwnedEnglish119FirstPrimary.Prepare(null!,table));Guard(()=>LegacyOwnedEnglish119FirstPrimary.Prepare(first!.Owner,null!));
if(actual.Count!=124||prepared.Count!=512||preparedSkipped==0||preparedRootStops==0||!failedPrefix)throw new Exception("Missing independent primary/ancestor coverage.");
File.WriteAllText(args[3],JsonSerializer.Serialize(new Output(path,model.Source.Fingerprint,actual,fixture,prepared,actualCallbacks,actualBlankRows,actualSkipped,primaryRows,preparedSkipped,preparedRootStops,guards),CaptureJson.Default.Output));
Console.WriteLine($"Owned119 primary124/{primaryRows}records/{actualCallbacks}callbacks/{actualBlankRows}realblankrows/{actualSkipped}realskipped; prepared512/{preparedSkipped}skipped/{preparedRootStops}rootretained/{guards}guards. NoB0/B8/wordwinner.");
record Arc(int Ordinal,byte Span,int Group,int[] Predecessors);
record State(int Node,int Arc,LegacyFirstTransitionBounds Bounds,float Future,float Prefix,int Join,int Blank,int[] Matrix);
record Call(int[] Data);
record Records(int Count,byte[] Bytes);
record Row(float[][][] Raw,List<Arc[]> Nodes,LegacyFirstOrdinaryGlyphRequest[][] Requests,List<State> States,List<Call> Calls,Records[] Primary,int RequestCount,int BlankProposals,int NodesCount,int Groups,int CachedGroups,
 float Height,float[][][] Cut,LegacyInkRange[][] GroupRanges,int[][][] ClassRequests);
record Prepared(LegacyFirstOrdinaryGlyphRequest[][] Requests,Records[] Primary,List<Call> Calls);
record Output(string Path,string Fingerprint,List<Row> Actual,Row PreparedOwner,List<Prepared> Prepared,int ActualCallbacks,int ActualBlankRows,int ActualSkipped,int PrimaryRows,int PreparedSkipped,int PreparedRootStops,int Guards);
sealed class ProbeException:Exception;
[JsonSerializable(typeof(Output))]partial class CaptureJson:JsonSerializerContext;
