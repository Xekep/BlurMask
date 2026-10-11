using System.Text.Json;
using InkRecognition;
if(args.Length!=5)throw new ArgumentException("controls.json acoustic.res lexical.res engine-profile.json separation-profile.json");
var profile=LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(File.ReadAllBytes(args[1]),189379);
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[0]));int cases=0,paths=0,arcs=0,multiple=0,partitions=0;
foreach(var test in controls.RootElement.EnumerateArray())
{
 var raw=test.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
 var prepared=LegacyCursivePreparedInk.Create(raw);var markers=Enumerable.Range(0,raw.Length).Select(i=>new LegacyCursiveCutMarker(0,i,0)).ToArray();
 var cuts=new LegacyCursiveCutResult(prepared,markers,LegacyCursiveInkAtoms.Build(prepared.StrokePointCounts.Span,markers));
 var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,80,profile,new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());
 var deferred=test.GetProperty("deferred").EnumerateArray().Select(x=>x.GetInt32()).ToArray();
 Require(seeds.Seeds.Select((s,i)=>(s,i)).Where(x=>x.s.Deferred).Select(x=>x.i).SequenceEqual(deferred),"Raw fixture did not produce the independently specified deferred pattern.");
 var graph=LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds);
 var expected=test.GetProperty("paths").EnumerateArray().Select(p=>string.Join(",",p.EnumerateArray().Select(x=>x.GetInt32()))).ToHashSet();
 var actual=new HashSet<string>();var path=new List<int>();
 void Walk(int node,int ordinal)
 {
  var arc=graph.GetNode(node)[ordinal];path.Add(arc.SeedGroupIndex);
  if(node==graph.NodeCount-1)actual.Add(string.Join(",",path));
  else foreach(int child in arc.SuccessorArcIndices.Span)Walk(node+1,child);
  path.RemoveAt(path.Count-1);
 }
 for(int ordinal=0;ordinal<graph.GetNode(0).Count;ordinal++)Walk(0,ordinal);
 Require(actual.SetEquals(expected),$"Native multiple-deferred paths differ in case {cases}.");
 var refs=new int[raw.Length];
 for(int node=0;node<graph.NodeCount;node++)
  for(int ordinal=0;ordinal<graph.GetNode(node).Count;ordinal++)
  {
   var arc=graph.GetNode(node)[ordinal];refs[arc.SeedGroupIndex]++;
   Require(arc.PlacementOffset==(seeds.Seeds[arc.SeedGroupIndex].Deferred?node-arc.SeedGroupIndex:0),"Deferred placement byte differs.");
   foreach(int parent in arc.PredecessorArcIndices.Span)
    Require(node==0?parent==-1:graph.GetNode(node-1)[parent].SuccessorArcIndices.Span.Contains(ordinal),"Lost reciprocal native predecessor.");
   foreach(int child in arc.SuccessorArcIndices.Span)
    Require(node+1<graph.NodeCount && graph.GetNode(node+1)[child].PredecessorArcIndices.Span.Contains(ordinal),"Lost reciprocal native successor.");
  }
 Require(refs.Select(x=>(byte)Math.Min(x,200)).SequenceEqual(graph.SeedReferenceCounts.ToArray()),"Native seed reference counts differ.");
 if(deferred.Length>1){multiple++;try{LegacyOrdinaryCursiveSeedGraph.CreateSingleDeferred(seeds);throw new Exception("SingleDeferred compatibility guard lost.");}catch(NotSupportedException){}}
 else Require(LegacyOrdinaryCursiveSeedGraph.CreateSingleDeferred(seeds).ArcCount==graph.ArcCount,"SingleDeferred behavior changed.");
 // The restored merger is a real consumer of the new topology. With span
 // cap1, every complete candidate path must retain the oracle's singleton
 // order, including multiple adjacent deferred alternatives.
 var merged=LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,graph,1);
 Require(merged.CandidateCount==graph.ArcCount,"Singleton merge changed native topology.");
 if(test.TryGetProperty("partitions",out var expectedPartitions))
 {
  var candidates=LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,graph,3);var actualPartitions=new HashSet<string>();var parts=new List<string>();
  void Visit(int node,int ordinal)
  {
   var arc=candidates.GetNode(node)[ordinal];parts.Add(string.Join(",",candidates.Groups[arc.GroupId].SeedGroupIndices.Span.ToArray().Order()));
   if(node==candidates.NodeCount-1)actualPartitions.Add(string.Join("|",parts));
   else foreach(var next in arc.Successors)Visit(node+next.NodeOffset,next.ArcIndex);
   parts.RemoveAt(parts.Count-1);
  }
  for(int node=0;node<candidates.NodeCount;node++)for(int ordinal=0;ordinal<candidates.GetNode(node).Count;ordinal++)
  {var arc=candidates.GetNode(node)[ordinal];if(node-arc.Span==0 && arc.PredecessorArcIndices.Span.Contains(-1))Visit(node,ordinal);}
  Require(actualPartitions.SetEquals(expectedPartitions.EnumerateArray().Select(x=>x.GetString()!)),"Merged multiple-deferred graph differs from independent full partitions.");
  partitions+=actualPartitions.Count;
 }
 cases++;paths+=actual.Count;arcs+=graph.ArcCount;
}
Console.WriteLine($"PASS {cases} exhaustive native deferred patterns ({multiple} multiple), {paths} complete stable interleavings/{arcs} arcs/{partitions} merged partitions; actual RU seed policy, prepared ownership, reciprocal adjacency, placement, references, old API guard and merged-graph consumer.");
var acoustic=File.ReadAllBytes(args[1]);var model=LegacyRussianModelBundle.Load(acoustic,File.ReadAllBytes(args[2]),File.ReadAllBytes(args[3]));
var sourceProfile=LegacyOrdinarySourceInk.ReadV10dResourceProfile(acoustic);
var cache=new LegacyOrdinaryAcousticClassCache(LegacyOrdinaryAcousticCacheProfile.ReadV110Node(acoustic.AsSpan(189734)),model.AcousticAlphabet,acoustic.AsSpan(99,16));
var candidateScorer=LegacyPreparedAcousticCandidateScorer.ForAlphabet(LegacyAcousticCandidateThresholds.ReadV110Node(acoustic.AsSpan(189734),out _),model.AcousticAlphabet);
var bridge=new LegacyCursiveFinalCacheGroupScorer(cache,candidateScorer,model.CreateCursiveCategoryMapper(1),LegacyCursiveSeparationProfile.ReadV106Node(acoustic,188157),LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[4])),model.AcousticAlphabet,model.LexicalAlphabet,Enumerable.Repeat((byte)1,model.AcousticAlphabet.LabelCount).ToArray(),LegacyWideSymbolPairs.CreateUnrestricted(model.AcousticAlphabet),relatedFormMode:0);
InkStroke Stroke(params(float X,float Y)[] pts)=>new(pts.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
var real=new[]{Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),Stroke((70,10),(70,90),(120,10),(120,90)),Stroke((140,90),(140,10),(180,10),(190,20),(190,40),(180,50),(140,50)),Stroke((10,0),(20,0)),Stroke((75,0),(80,0))};
var options=new LegacyCursiveWordSearchOptions(90,80,0,1,BeamWidth:128,MaximumExpandedChoices:10_000_000);
var recognizer=new LegacyNativeGraphWordRecognizer(model);
LegacyOrdinaryAcousticCacheContext Scope(LegacyGraphGlyphPosition p,LegacyOrdinaryCursiveCandidateArc a,LegacyPreparedGlyphClassification g)=>new(true,true,true,false,false,null,false,ReadOnlyMemory<ushort>.Empty);
LegacyNativeGraphRecognitionResult Recognize(IReadOnlyList<InkStroke> input,LegacyCursiveWordSearchOptions settings,int maximumCandidates=131072,CancellationToken ct=default)=>recognizer.RecognizeOrdinaryDeferred(input,sourceProfile,profile,new byte[input.Count],Enumerable.Repeat(-1,input.Count).ToArray(),settings,bridge,Scope,false,false,false,maximumCandidates:maximumCandidates,cancellationToken:ct);
var result=Recognize(real,options);
Require(result.Seeds.DeferredCount==2 && result.Words.Count>0 && result.ClassifiedGroups>0,"Actual2late source failed to reach multi-deferred neural/cache/lexical consumer.");
var dup=real.Select(s=>new InkStroke(s.Points.SelectMany(p=>new[]{p,p}).ToArray())).ToArray();var repeated=Recognize(dup,options);
Require(result.Words.Select(w=>(w.Text,w.Match.AcousticCost,w.Match.Cost)).SequenceEqual(repeated.Words.Select(w=>(w.Text,w.Match.AcousticCost,w.Match.Cost))),"Multiple-deferred source dedup changed retained scores.");
foreach(var word in result.Words)
{
 int previousNode=-1,previousArc=-1;var ranges=new List<LegacyInkRange>();
 foreach(var position in word.Match.Glyphs)
 {
  var arc=result.Graph.GetNode(position.NodeIndex)[position.ArcIndex];
  Require(position.NodeIndex-arc.Span-1==previousNode && arc.PredecessorArcIndices.Span.Contains(previousArc),"Retained multi-deferred word crossed incompatible placement arcs.");
  ranges.AddRange(result.Graph.Groups[arc.GroupId].Ranges.ToArray());previousNode=position.NodeIndex;previousArc=position.ArcIndex;
 }
 Require(previousNode==result.Graph.NodeCount-1 && word.Match.Cost==word.Score.TotalCost,"Retained multi-deferred terminal/span mismatch.");
 for(int stroke=0;stroke<real.Length;stroke++)for(int point=0;point<result.Source.Cuts.PreparedInk.StrokePointCounts.Span[stroke];point++)
  Require(ranges.Any(r=>r.StrokeIndex==stroke && r.StartPoint<=point && point<r.EndPointExclusive),"Multiple-deferred retained word lost prepared source ink.");
}
try{recognizer.Recognize(real,sourceProfile,profile,new byte[real.Length],Enumerable.Repeat(-1,real.Length).ToArray(),options,bridge,Scope,false,false,false);throw new Exception("Old single endpoint guard lost.");}catch(NotSupportedException){}
try{Recognize(real,options,1);throw new Exception("Candidate budget ignored.");}catch(InvalidOperationException){}
try{Recognize(real,options with{MaximumScoredGroups=1});throw new Exception("Neural budget ignored.");}catch(InvalidOperationException){}
try{Recognize(real,options with{MaximumExpandedChoices=1});throw new Exception("Expansion budget ignored.");}catch(InvalidOperationException){}
try{Recognize(real,options,ct:new CancellationToken(true));throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
Console.WriteLine($"ACTUAL2late {result.Graph.NodeCount} nodes/{result.Graph.CandidateCount} candidate arcs/{result.ClassifiedGroups} classified geometry groups; top={string.Join(",",result.Words.Take(5).Select(w=>$"{w.Text}:{w.Match.Cost}"))}. Exact ingress/cuts/native multi-deferred topology/final195/lexical transport; explicit partial scope/all-symbol mask, original beam, no acceptance/accuracy claim.");
Console.WriteLine("PASS Native ordinary multiple-deferred word endpoint, raw dedup, retained physical ownership and neural/candidate/expansion/cancellation guards.");
static void Require(bool value,string message){if(!value)throw new InvalidDataException(message);}
