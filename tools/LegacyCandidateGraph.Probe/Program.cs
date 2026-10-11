using System.Text.Json;
using InkRecognition;
if(args.Length!=5)throw new ArgumentException("controls.json acoustic.res lexical.res engine-profile.json separation-profile.json");
var profile=LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(File.ReadAllBytes(args[1]),189379);
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[0]));int cases=0,candidates=0,partitions=0,matchedWords=0;
var toyLexicon=LegacyPackedLexicon.Load(Convert.FromBase64String(controls.RootElement.GetProperty("lexicon").GetString()!));
InkStroke Stroke(float x,float maxX)=>new(new[]{new InkPoint(x,10),new InkPoint(maxX,90)});
foreach(var test in controls.RootElement.GetProperty("cases").EnumerateArray())
{
 int count=test.GetProperty("count").GetInt32(),selected=test.GetProperty("selected").GetInt32(),cap=test.GetProperty("cap").GetInt32();
 var raw=Enumerable.Range(0,count).Select(i=>Stroke(i*100,i*100+20)).Append(Stroke(selected*100+9,selected*100+9)).ToArray();
 var prepared=LegacyCursivePreparedInk.Create(raw);var markers=Enumerable.Range(0,raw.Length).Select(i=>new LegacyCursiveCutMarker(0,i,0)).ToArray();
 var cuts=new LegacyCursiveCutResult(prepared,markers,LegacyCursiveInkAtoms.Build(prepared.StrokePointCounts.Span,markers));
 var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,80,profile,new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());
 var seedGraph=LegacyOrdinaryCursiveSeedGraph.CreateSingleDeferred(seeds);
 var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,seedGraph,cap);
 var expected=test.GetProperty("partitions").EnumerateArray().Select(x=>x.GetString()!).ToHashSet();
 var actual=Enumerate(graph);Require(actual.SetEquals(expected),$"Native candidate graph partitions differ in case{cases}; actual{actual.Count}, expected{expected.Count}.");
 for(int node=0;node<graph.NodeCount;node++)
  for(int ordinal=0;ordinal<graph.GetNode(node).Count;ordinal++)
  {
   var arc=graph.GetNode(node)[ordinal];var group=graph.Groups[arc.GroupId];
   Require(arc.Span<cap && group.SeedGroupIndices.Length==arc.Span+1,"Merged span replaced placement offset or lost seed ownership.");
   var expectedRanges=LegacyCursiveInkAtoms.Compose(group.SeedGroupIndices.Span.ToArray().Select(i=>seeds.Seeds[i].Atom.Range).OrderBy(r=>r.StrokeIndex).ThenBy(r=>r.StartPoint).ToArray());
   Require(group.Ranges.Span.SequenceEqual(expectedRanges),"Merged prepared range composition differs.");
   int preceding=node-arc.Span-1;
   foreach(int parent in arc.PredecessorArcIndices.Span)
   {
    if(preceding<0){Require(parent==-1,"Nonroot ordinal used for source root.");continue;}
    Require(graph.GetNode(preceding)[parent].Successors.Contains(new LegacyCursiveCandidateSuccessor(node-preceding,ordinal)),"Merged predecessor lacks reciprocal offset vector.");
   }
   foreach(var successor in arc.Successors)
   {
    var next=graph.GetNode(node+successor.NodeOffset)[successor.ArcIndex];
    Require(node+successor.NodeOffset-next.Span-1==node && next.PredecessorArcIndices.Span.Contains(ordinal),"Merged successor lost its predecessor/source span.");
   }
  }
 candidates+=graph.CandidateCount;partitions+=actual.Count;cases++;
 ReadOnlyMemory<LegacyIntegerLexicalCandidate> Choices(LegacyGraphGlyphPosition position,LegacyOrdinaryCursiveCandidateArc arc,LegacyOrdinaryCursiveCandidateInk group)
 {
  int value=group.SeedGroupIndices.Span.ToArray().Sum(i=>(i+1)*(i+1))+group.SeedGroupIndices.Length*7;
  var choices=Enumerable.Range(1,3).Select(label=>new LegacyIntegerLexicalCandidate((ushort)label,value*label*13%127-40)).ToList();
  choices.Add(new(2,value*2*13%127-39));choices.Add(new(9,-1000));choices.Add(new(1,LegacyAcousticFixedLog.ImpossibleCost));return choices.ToArray();
 }
 int? Terminal(int state,int acoustic){int total=acoustic+state*31%107-53;return total<=180?total:null;}
 var search=LegacyGraphWordSearch.Search(toyLexicon,graph,Choices,256,20_000_000,Terminal);
 var expectedWords=test.GetProperty("expected");Require(search.Matches.Count==expectedWords.GetArrayLength(),"Graph-aware search lost/gained dictionary words.");
 for(int index=0;index<search.Matches.Count;index++)
 {
  var word=search.Matches[index];var reference=expectedWords[index];
  Require(word.Labels.Span.SequenceEqual(reference.GetProperty("labels").EnumerateArray().Select(x=>(ushort)x.GetInt32()).ToArray()) && word.AcousticCost==reference.GetProperty("acoustic").GetInt32() && word.Cost==reference.GetProperty("total").GetInt32() && word.TerminalState==reference.GetProperty("state").GetInt32(),$"Independent graph word minimum differs in case {cases}.");
  int previousNode=-1,previousArc=-1;
  foreach(var position in word.Glyphs)
  {
   var arc=graph.GetNode(position.NodeIndex)[position.ArcIndex];
   Require(position.NodeIndex-arc.Span-1==previousNode && arc.PredecessorArcIndices.Span.Contains(previousArc),"Retained word crossed an unlinked native placement.");
   previousNode=position.NodeIndex;previousArc=position.ArcIndex;
  }
  Require(previousNode==graph.NodeCount-1,"Retained word omitted source graph suffix.");
 }
 matchedWords+=search.Matches.Count;
 try{LegacyGraphWordSearch.Search(toyLexicon,graph,Choices,256,1,Terminal);throw new Exception("Expansion budget ignored.");}catch(InvalidOperationException){}
 try{LegacyGraphWordSearch.Search(toyLexicon,graph,Choices,cancellationToken:new CancellationToken(true));throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
 try{LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,seedGraph,cap,seedGraph.ArcCount-1);throw new Exception("Candidate budget ignored.");}catch(InvalidOperationException){}
}
Console.WriteLine($"PASS {cases} independent candidate partition/word references; {candidates} native graph candidates/{partitions} unique complete partitions/{matchedWords} exact dictionary minima, reciprocal distance vectors, span vs placement, prepared ownership and retained linked paths.");
var acoustic=File.ReadAllBytes(args[1]);
var model=LegacyRussianModelBundle.Load(acoustic,File.ReadAllBytes(args[2]),File.ReadAllBytes(args[3]));
var sourceProfile=LegacyOrdinarySourceInk.ReadV10dResourceProfile(acoustic);
var nativeCache=new LegacyOrdinaryAcousticClassCache(LegacyOrdinaryAcousticCacheProfile.ReadV110Node(acoustic.AsSpan(189734)),model.AcousticAlphabet,acoustic.AsSpan(99,16));
var scorer=LegacyPreparedAcousticCandidateScorer.ForAlphabet(LegacyAcousticCandidateThresholds.ReadV110Node(acoustic.AsSpan(189734),out _),model.AcousticAlphabet);
var cacheBridge=new LegacyCursiveFinalCacheGroupScorer(nativeCache,scorer,model.CreateCursiveCategoryMapper(1),LegacyCursiveSeparationProfile.ReadV106Node(acoustic,188157),LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[4])),model.AcousticAlphabet,model.LexicalAlphabet,Enumerable.Repeat((byte)1,model.AcousticAlphabet.LabelCount).ToArray(),LegacyWideSymbolPairs.CreateUnrestricted(model.AcousticAlphabet),relatedFormMode:0);
InkStroke Draw(params (float X,float Y)[] points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
var printed=new[]{Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((70,10),(70,90),(120,10),(120,90)),Draw((140,90),(140,10),(180,10),(190,20),(190,40),(180,50),(140,50))};
var late=printed.Append(Draw((10,0),(20,0))).ToArray();
var options=new LegacyCursiveWordSearchOptions(90,80,0,1,BeamWidth:128,MaximumExpandedChoices:10_000_000);
LegacyOrdinaryAcousticCacheContext ExplicitPartialScope(LegacyPreparedGlyphClassification glyph)=>new(true,true,true,false,false,null,false,ReadOnlyMemory<ushort>.Empty);
var recognizer=new LegacyNativeGraphWordRecognizer(model);
LegacyNativeGraphRecognitionResult Recognize(IReadOnlyList<InkStroke> raw)=>recognizer.Recognize(raw,sourceProfile,profile,new byte[raw.Count],Enumerable.Repeat(-1,raw.Count).ToArray(),options,cacheBridge,(position,arc,glyph)=>ExplicitPartialScope(glyph),false,false,false);
var ordinary=Recognize(printed);
var direct=new LegacyRussianCursiveWordRecognizer(model,relatedFormMode:0).RecognizeOrdinarySourceWithFinalCache(printed,sourceProfile,options,cacheBridge,(group,glyph)=>ExplicitPartialScope(glyph),false,false,false);
Require(ordinary.Seeds.DeferredCount==0 && ordinary.Words.Count>0 && ordinary.Words.Take(5).Select(w=>(w.Text,(long)w.Match.Cost)).SequenceEqual(direct.Words.Take(5).Select(w=>(w.Text,w.Score.TotalCost))),"Actual nondeferred graph bridge top5 differs from restored ordinary chain/cache/lexical endpoint.");
foreach(var raw in new[]{printed,late})
{
 var result=raw==printed?ordinary:Recognize(raw);
 Require(result.Words.Count>0 && result.ClassifiedGroups>0 && result.Graph.NodeCount==result.Seeds.Seeds.Count,"Resource-backed graph bridge did not produce retained words.");
 if(raw==late)Require(result.Seeds.DeferredCount==1,"Late-stroke fixture did not reach single-deferred graph consumer.");
 var duplicate=raw.Select(s=>new InkStroke(s.Points.SelectMany(p=>new[]{p,p}).ToArray())).ToArray();var repeat=Recognize(duplicate);
 Require(result.Words.Select(w=>(w.Text,w.Match.Cost,w.Match.AcousticCost)).SequenceEqual(repeat.Words.Select(w=>(w.Text,w.Match.Cost,w.Match.AcousticCost))),"Raw dedup/inserted chronology changed graph output costs.");
 foreach(var word in result.Words)
 {
  Require(word.Score.TotalCost==word.Match.Cost,"Retained terminal score belongs to a discarded alternative.");
  var ranges=word.Match.Glyphs.SelectMany(p=>result.Graph.Groups[result.Graph.GetNode(p.NodeIndex)[p.ArcIndex].GroupId].Ranges.ToArray()).ToArray();
  foreach(int physical in Enumerable.Range(0,raw.Length))
   for(int point=0;point<result.Source.Cuts.PreparedInk.StrokePointCounts.Span[physical];point++)
    Require(ranges.Any(r=>r.StrokeIndex==physical && r.StartPoint<=point && r.EndPointExclusive>point),"Retained deferred word omitted a prepared physical point.");
 }
 Console.WriteLine($"ACTUAL graph words: deferred={result.Seeds.DeferredCount}, nodes={result.Graph.NodeCount}, candidates={result.Graph.CandidateCount}, classified={result.ClassifiedGroups}; top={string.Join(",",result.Words.Take(5).Select(w=>$"{w.Text}:{w.Match.Cost}"))}. Explicit partial source scope/all-symbol mask; original bounded beam, no accuracy/acceptance claim.");
}
try{recognizer.Recognize(printed,sourceProfile,profile,new byte[printed.Length],Enumerable.Repeat(-1,printed.Length).ToArray(),options with{MaximumScoredGroups=1},cacheBridge,(p,a,g)=>ExplicitPartialScope(g),false,false,false);throw new Exception("Neural group budget ignored.");}catch(InvalidOperationException){}
Console.WriteLine("PASS resource ordinary ingress/enclosure/cuts/native seed+deferred graph/merged glyph/context/final195-cache/whole-word lexical bridge, dedup invariance and retained source ownership.");
static HashSet<string> Enumerate(LegacyOrdinaryCursiveCandidateGraph graph)
{
 var result=new HashSet<string>();var parts=new List<string>();
 void Walk(int node,int ordinal)
 {
  var arc=graph.GetNode(node)[ordinal];parts.Add(string.Join(",",graph.Groups[arc.GroupId].SeedGroupIndices.Span.ToArray().Order()));
  if(node==graph.NodeCount-1)result.Add(string.Join("|",parts));
  else foreach(var successor in arc.Successors)Walk(node+successor.NodeOffset,successor.ArcIndex);
  parts.RemoveAt(parts.Count-1);
 }
 for(int node=0;node<graph.NodeCount;node++)for(int ordinal=0;ordinal<graph.GetNode(node).Count;ordinal++)
 {
  var arc=graph.GetNode(node)[ordinal];
  if(node-arc.Span==0 && arc.PredecessorArcIndices.Span.Contains(-1))Walk(node,ordinal);
 }
 return result;
}
static void Require(bool okay,string message){if(!okay)throw new InvalidDataException(message);}
