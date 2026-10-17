using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition;
if(args.Length!=3)throw new ArgumentException("models-directory raw-controls.json capture.json");
using var input=JsonDocument.Parse(File.ReadAllBytes(args[1]));int guards=0;
var canceled=new CancellationToken(true);
void Guard(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Expected owner/candidate refusal.");}
InkStroke[] Ink(JsonElement e)=>e.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
float[][][] Points(IEnumerable<InkStroke> s)=>s.Select(s=>s.Points.Select(p=>new[]{p.X,p.Y}).ToArray()).ToArray();
int[][][] Shorts(IEnumerable<LegacyInkStroke> s)=>s.Select(s=>s.Points.ToArray().Select(p=>new[]{(int)p.X,(int)p.Y}).ToArray()).ToArray();
int[][] Ranges(IEnumerable<LegacyInkRange>rs)=>rs.Select(r=>new[]{r.StrokeIndex,r.StartPoint,r.EndPointExclusive}).ToArray();
string path="en-US/en_US-ak-cur.lite.res";byte[]data=File.ReadAllBytes(Path.Combine(args[0],path));
var parent=LegacySource247WireLayout.Read(data);var model=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(data,parent,parent.Children.First(c=>c.Kind==0x249));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(args[0],"ru-RU/legacy-separation-profile.json")));
var cases=new List<CaseCapture>();int groups=0,inserted=0,rejected=0;
void Capture(InkStroke[] raw)
{
 var preparation=LegacyOwnedEnglishLiteFullPreprocessing.Prepare(model,raw);var owner=LegacyOwnedEnglish119Candidates.Prepare(preparation,table);
 if(!ReferenceEquals(owner.Model,model)||!ReferenceEquals(owner.Sampler.Source,preparation.Source)||owner.MaximumAtoms!=7)throw new Exception("Foreign source/model or invented graph cap.");
 var source=preparation.CopyPreparedStrokes();var records=owner.CopyCutRecords();var counts=owner.CopyCutCounts();
 var ids=new List<LegacySourcePointIdentity>();for(int s=0;s<counts.Length;s++)for(int p=0;p<counts[s];p++)ids.Add(owner.ResolveCutPoint(s,p));
 var nodes=Enumerable.Range(0,owner.NodeCount).Select(n=>owner.CopyNode(n).Select((a,i)=>new ArcCapture(a.GroupId,a.Span,a.Flags,a.StartContextId,a.EndContextId,
  a.PredecessorArcIndices.ToArray(),a.Successors.ToArray(),owner.ResolveCandidate(n,i))).ToArray()).ToArray();
 var results=new List<GroupCapture>();foreach(int g in owner.CopyLiveGroupIds())
 {
  var score=owner.ScoreGroup(g);var value=score.Features;
  if(!ReferenceEquals(score.Owner,owner)||!ReferenceEquals(value.Owner,owner))throw new Exception("Foreign classifier/feature owner.");
  results.Add(new(g,Ranges(owner.CopyGroupRanges(g)),value.CopyNormalizationOrder(),value.CopyStoredRanges().Select(r=>owner.ResolveCutPoint(r.StrokeIndex,r.StartPoint).Key).ToArray(),
   value.Boundary,value.DegenerateIntegerGeometry,Shorts(value.CopyNormalizedStrokes()),Shorts(value.CopyMergedStrokes()),value.CopyRetainedPenUpBoundaries(),
   value.CopyTrajectoryInput(),value.CopyRasterInput(),new[]{score.Trajectory,score.Raster}.Select(x=>new NetworkCapture(x.Owner.Slot,x.CopyHiddenDots(),x.CopyHiddenValues(),x.CopyOutputDots(),x.CopyProbabilities())).ToArray()));
 }
 inserted+=counts.Sum()-source.Sum(s=>s.Points.Count);rejected+=owner.RemovedArcCount;groups+=results.Count;
 var inputKeys=source.SelectMany((s,physical)=>s.Points.Select((_,p)=>preparation.ResolvePreparedPoint(physical,p))).ToArray();
 cases.Add(new(Points(raw),Points(source),preparation.Line,preparation.CopyPointKeys200(),inputKeys,
  owner.CopyPhysicalProbabilities(),ids.ToArray(),records,counts,owner.CopyMarkers(),owner.CopyAtoms(),nodes,Enumerable.Range(0,owner.GroupCount).Select(g=>Ranges(owner.CopyGroupRanges(g))).ToArray(),
  owner.CopyLiveGroupIds(),owner.RemovedArcCount,results));
 var copy=owner.CopyCutRecords();copy[0]=default;var rcopy=owner.CopyGroupRanges(0);rcopy[0]=default;
 if(owner.CopyCutRecords()[0]!=records[0]||owner.CopyGroupRanges(0)[0].EndPointExclusive==0)throw new Exception("Mutable candidate geometry.");
 Guard(()=>owner.ScoreGroup(-1));Guard(()=>owner.ScoreGroup(owner.GroupCount));Guard(()=>owner.ScoreGroup(0,canceled));
 var live=owner.CopyLiveGroupIds().ToHashSet();foreach(int dead in Enumerable.Range(0,owner.GroupCount).Where(g=>!live.Contains(g)))Guard(()=>owner.ScoreGroup(dead));
 Guard(()=>LegacyOwnedEnglish119Candidates.Prepare(preparation,table,canceled));
}
var all=input.RootElement.GetProperty("cases");foreach(var item in all.EnumerateArray())Capture(Ink(item));
Capture(new[]{new InkStroke(new[]{new InkPoint(0,90),new InkPoint(0,10),new InkPoint(25,70),new InkPoint(50,10),new InkPoint(50,90)}),
 new InkStroke(new[]{new InkPoint(65,90),new InkPoint(65,10)}),new InkStroke(new[]{new InkPoint(80,90),new InkPoint(110,10)})});
Capture(new[]{new InkStroke(new[]{new InkPoint(0,90),new InkPoint(0,10),new InkPoint(40,90)}),
 new InkStroke(new[]{new InkPoint(1000,90),new InkPoint(1000,10)})});
Capture(new[]{new InkStroke(Enumerable.Range(0,25).Select(i=>new InkPoint((float)(30+30*Math.Cos(i*Math.Tau/24)),(float)(50+40*Math.Sin(i*Math.Tau/24)))).ToArray()),
 new InkStroke(new[]{new InkPoint(30,40),new InkPoint(30,60)})});
Guard(()=>LegacyOwnedEnglish119Candidates.Prepare(null!,table));
foreach(int at in new[]{16968,18338,18339,18356,18647})
{
 var bad=data.ToArray();if(at==18356)bad[at]=0;else bad[at]^=1;
 Guard(()=>{var p=LegacySource247WireLayout.Read(bad);var m=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(bad,p,p.Children.First(c=>c.Kind==0x249));
  var prep=LegacyOwnedEnglishLiteFullPreprocessing.Prepare(m,Ink(all[0]));LegacyOwnedEnglish119Candidates.Prepare(prep,table);});
}
var foreignParent=LegacySource247WireLayout.Read(data);var foreignModel=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(data,foreignParent,foreignParent.Children.First(c=>c.Kind==0x249));
Guard(()=>model.Source.ReadSampler(foreignModel.Source.Children.Single(c=>c.Destination=="247.130")));
if(groups==0||rejected==0)throw new Exception($"Actual pruning/classifier coverage missing: {rejected}/{groups}.");
var result=new CaptureData(path,model.Source.Offset,model.Source.End,model.Source.Fingerprint,table.Values.ToArray(),
 LegacyOwnedFirst119Activations.Prepare(model).CopyHiddenTable(),LegacyOwnedFirst119Activations.Prepare(model).CopyOutputTable(),cases,groups,inserted,rejected,guards);
File.WriteAllText(args[2],JsonSerializer.Serialize(result,CaptureJsonContext.Default.CaptureData));
Console.WriteLine($"ActualEN119 candidates: {cases.Count} raw owners/{groups} live group evaluations/{inserted} cut insertions/{rejected} pruned arcs/{guards} guards; no cache/controller/winner.");
record NetworkCapture(int Slot,int[] HiddenDots,short[] HiddenActivations,int[] RawDots,int[] Probability32768);
record GroupCapture(int Id,int[][] Ranges,int[] Order,LegacySourcePointKey[] StartKeys,LegacyCursiveGlyphBoundarySigns Boundary,bool Degenerate,int[][][] Normalized,int[][][] Merged,int[] Retained,short[] Trajectory,short[] Raster,NetworkCapture[] Networks);
record ArcCapture(int GroupId,byte Span,byte Flags,uint Start,uint End,int[] Previous,LegacyCursiveCandidateSuccessor[] Next,LegacyFirstResultSourceRange[] SourceRanges);
record CaseCapture(float[][][] Raw,float[][][] Prepared,LegacyLinePair Line,int[] Map200,LegacySourcePointIdentity[] InputIdentities,float[] Probabilities,
 LegacySourcePointIdentity[] OutputIdentities,LegacyCursivePreparedPoint[] Records,int[] Counts,LegacyCursiveCutMarker[] Markers,LegacyCursiveInkAtom[] Atoms,ArcCapture[][] Nodes,int[][][] AllocatedGroups,int[] LiveGroups,int Removed,List<GroupCapture> Groups);
record CaptureData(string Path,int SourceOffset,int SourceEnd,string Fingerprint,float[] Table,int[] HiddenTable,int[] OutputTable,List<CaseCapture> Cases,int Groups,int Inserted,int Rejected,int Guards);
[JsonSerializable(typeof(CaptureData))]
partial class CaptureJsonContext:JsonSerializerContext;
