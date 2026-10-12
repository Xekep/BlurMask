using InkRecognition;
using System.Text.Json;
if(args.Length is not (1 or 2))throw new ArgumentException("Usage: LegacyShiftZeroSeed.Probe controls.json [ru-ak.res]");
var profile=new LegacyOrdinaryCursiveSeedProfile(true,0,.3f,.6f);
if(args.Length==2)
{
 profile=LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(File.ReadAllBytes(args[1]),12727);
 if(profile.InsertionShift!=0||!profile.Reorder||profile.BackwardHeightTolerance!=.3f||profile.InsertionFraction!=.6f)throw new Exception("Actual first21b shift-zero profile differs.");
}
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[0]));int cases=0,arcs=0,deferredFirst=0,adjacent=0;
foreach(var fixture in controls.RootElement.EnumerateArray())
{
 int n=fixture.GetProperty("count").GetInt32();var raw=Enumerable.Range(0,n).Select(i=>Stroke(i*100,i*100+20)).ToList();
 foreach(var mark in fixture.GetProperty("marks").EnumerateArray()){int x=mark[0].GetInt32()*100+mark[1].GetInt32();raw.Add(Stroke(x,x));}
 var prepared=LegacyCursivePreparedInk.Create(raw);var markers=Enumerable.Range(0,raw.Count).Select(i=>new LegacyCursiveCutMarker(0,i,0)).ToArray();
 var cuts=new LegacyCursiveCutResult(prepared,markers,LegacyCursiveInkAtoms.Build(prepared.StrokePointCounts.Span,markers));
 var source=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,80,profile,new byte[raw.Count],Enumerable.Repeat(-1,raw.Count).ToArray());
 var expected=fixture.GetProperty("order").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
 if(!source.Seeds.Select(s=>s.SourceAtomIndex).SequenceEqual(expected)||source.InsertionShift!=0||source.Seeds.Any(s=>s.InsertionDisplacement!=0))throw new Exception("Independent shift-zero sort/insertion order differs.");
 var graph=LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(source);
 if(graph.NodeCount!=raw.Count||graph.ArcCount!=raw.Count||graph.SeedReferenceCounts.Span.IndexOfAnyExcept((byte)1)>=0)throw new Exception("Shift-zero added placement alternatives/refcounts.");
 for(int node=0;node<graph.NodeCount;node++)
 {
  var arc=graph.GetNode(node).Single();
  if(arc.SeedGroupIndex!=node||arc.PlacementOffset!=0||!arc.PredecessorArcIndices.Span.SequenceEqual(new[]{node==0?-1:0})||!arc.SuccessorArcIndices.Span.SequenceEqual(node+1<graph.NodeCount?new[]{0}:Array.Empty<int>())||arc.StartContextId!=node*128u||arc.EndContextId!=node*128u+3||arc.Flags!=4)throw new Exception("Independent linear noNN shift-zero root/contexts/flags differ.");
 }
 if(source.Seeds[0].Deferred)deferredFirst++;
 for(int i=1;i<source.Seeds.Count;i++)if(source.Seeds[i-1].Deferred&&source.Seeds[i].Deferred)adjacent++;
 ExpectInvalid(()=>LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(source));
 using var cancel=new CancellationTokenSource();cancel.Cancel();try{LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(source,cancel.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
 arcs+=graph.ArcCount;cases++;
}
// Multiple cut atoms of one physical stroke exercise prefix/suffix overlap,
// separately from each-point/physical-index uniqueness controls above.
var points=new[]{new InkPoint(0,10),new InkPoint(20,90),new InkPoint(40,10),new InkPoint(60,90)};
var split=LegacyCursivePreparedInk.Create(new[]{new InkStroke(points)});var splitMarkers=new[]{new LegacyCursiveCutMarker(0,0,0),new LegacyCursiveCutMarker(1,0,1),new LegacyCursiveCutMarker(1,0,2)};
var splitCuts=new LegacyCursiveCutResult(split,splitMarkers,LegacyCursiveInkAtoms.Build(split.StrokePointCounts.Span,splitMarkers));
var splitSource=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(splitCuts,80,profile,new byte[1],new[]{-1});
var splitGraph=LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(splitSource);
for(int i=0;i<splitGraph.NodeCount;i++)if(splitGraph.GetNode(i).Single().Flags!=(i==0?4:0))throw new Exception("Physical source spanning prefix/suffix didn't clearbit4.");
var shifted=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(splitCuts,80,profile with{InsertionShift=1},new byte[1],new[]{-1});ExpectInvalid(()=>LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(shifted));
ExpectInvalid(()=>LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(splitCuts,80,profile with{InsertionShift=2},new byte[1],new[]{-1}));
if(deferredFirst==0||adjacent==0)throw new Exception("Independent zero-shift corpus missed deferred-first/adjacent scope.");
Console.WriteLine($"PASS {cases} independent first21b zero-shift sort/linear graph controls, {arcs} arcs/{deferredFirst} deferred-first/{adjacent} adjacent-deferred positions, ordinal context/root, physical overlap, cancellation and shift ownership.");
static InkStroke Stroke(float x,float max)=>new(new[]{new InkPoint(x,10),new InkPoint(max,90)});
static void ExpectInvalid(Action action){try{action();}catch(ArgumentException){return;}throw new Exception("Invalid shift scope accepted.");}
