using System.Text.Json;
using InkRecognition;
if(args.Length!=5)throw new ArgumentException("controls.json acoustic.res lexical.res engine-profile.json separation-profile.json");
var profile=LegacyOrdinarySourceInk.ReadV10dResourceProfile(File.ReadAllBytes(args[1]));
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[0]));int cases=0,points=0,scopes=0;
foreach(var test in controls.RootElement.EnumerateArray())
{
 var raw=test.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
 var source=LegacyOrdinaryCursiveCutSource.Generate(raw,profile,90,80);var counts=raw.Select(s=>s.Points.Count).ToArray();
 var metadata=new LegacyOrdinarySourceEndpointMetadata(source,counts);var ends=test.GetProperty("endOrdinals").EnumerateArray().Select(x=>x.GetInt32()).ToArray();
 int global=0;
 for(int stroke=0;stroke<source.Cuts.PreparedInk.StrokePointCounts.Length;stroke++)
  for(int point=0;point<source.Cuts.PreparedInk.StrokePointCounts.Span[stroke];point++)
  {
   int filtered=source.Cuts.PreparedInk.Points.Span[global++].OriginalPointIndex;
   var actual=metadata.EndKeys.Resolve(stroke,point).Key;
   Require(actual.First==0 && actual.Second==stroke && actual.Third==ends[filtered],"Native end-key did not retain independent raw ownership interval.");points++;
  }
 var preparedCounts=source.Cuts.PreparedInk.StrokePointCounts.Span.ToArray();
 for(int stroke=0;stroke<raw.Length;stroke++)
 {
  var whole=new[]{new LegacyInkRange(stroke,0,preparedCounts[stroke])};var actual=metadata.EvaluateOrderedRanges(whole,1);
  Require(actual.BoundaryAlignedSingleNormalizedStroke && actual.ProbabilityStrokeIndex==stroke && actual.SourceStrokeProbability==source.GetSourceStrokeProbability(stroke) && actual.EndpointFieldsAreZero==(raw[stroke].Points.Count==1),"Whole-source primitive predicate differs.");scopes++;
  var glyph=LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(source.Cuts.PreparedInk,whole,source.Source.Strokes[stroke].Points[0].X,90,80,100,10);
  for(int bits=0;bits<32;bits++)
  {
   ReadOnlyMemory<ushort> penalties=(bits&16)!=0?new ushort[]{1,3}:ReadOnlyMemory<ushort>.Empty;
   var template=new LegacyOrdinaryAcousticCacheContext((bits&1)!=0,(bits&2)!=0,(bits&4)!=0,false,true,.37f,(bits&8)!=0,penalties);
   var bound=metadata.BindContext(glyph,template);
   Require(bound==template with{IsWholePhysicalStroke=true,WholeStrokeEndpointCategoriesZero=actual.EndpointFieldsAreZero,SourceStrokeProbability=actual.SourceStrokeProbability},"Source scope binding changed an unrelated explicit cache field.");
  }
  if(preparedCounts[stroke]>1)
  {
   var partial=metadata.EvaluateOrderedRanges(new[]{new LegacyInkRange(stroke,0,preparedCounts[stroke]-1)},1);
   Require(!partial.BoundaryAlignedSingleNormalizedStroke && partial.SourceStrokeProbability is null && partial.ProbabilityStrokeIndex==-1,"Partial source exposed whole-stroke primitive.");scopes++;
  }
 }
 if(raw.Length>1)
 {
  var ranges=new[]{new LegacyInkRange(0,0,preparedCounts[0]),new LegacyInkRange(raw.Length-1,0,preparedCounts[^1])};
  var merged=metadata.EvaluateOrderedRanges(ranges,1);var unmerged=metadata.EvaluateOrderedRanges(ranges,2);
  Require(merged.BoundaryAlignedSingleNormalizedStroke && merged.ProbabilityStrokeIndex==0 && merged.SourceStrokeProbability==source.GetSourceStrokeProbability(0) && !unmerged.BoundaryAlignedSingleNormalizedStroke,"Native normalized-count predicate was replaced by physical range count.");scopes+=2;
 }
 try{new LegacyOrdinarySourceEndpointMetadata(source,counts.Select(c=>0).ToArray());throw new Exception("Invalid raw counts accepted.");}catch(ArgumentException){}
 try{metadata.EvaluateOrderedRanges(new[]{new LegacyInkRange(0,0,preparedCounts[0]+1)},1);throw new Exception("Invalid source ownership accepted.");}catch(ArgumentException){}
 try{metadata.EvaluateOrderedRanges(new[]{new LegacyInkRange(0,0,preparedCounts[0])},1,new CancellationToken(true));throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
 if(cases==0)
 {
  var foreign=LegacyCursivePreparedInk.Create(source.Source.Strokes);var glyph=LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(foreign,new[]{new LegacyInkRange(0,0,1)},0,90,80,100,10);
  try{metadata.Evaluate(glyph);throw new Exception("Foreign source geometry accepted.");}catch(ArgumentException){}
  try{new LegacyOrdinarySourceEndpointMetadata(source,counts.AsSpan(1));throw new Exception("Missing raw physical count accepted.");}catch(ArgumentException){}
 }
 cases++;
}
Console.WriteLine($"PASS {cases} independent raw-interval controls/{points} prepared end identities/{scopes} native source-primitive scopes, inserted mapping, trailing dropped points, normalized vs physical count and limits/cancel.");
var acoustic=File.ReadAllBytes(args[1]);var model=LegacyRussianModelBundle.Load(acoustic,File.ReadAllBytes(args[2]),File.ReadAllBytes(args[3]));
var cache=new LegacyOrdinaryAcousticClassCache(LegacyOrdinaryAcousticCacheProfile.ReadV110Node(acoustic.AsSpan(189734)),model.AcousticAlphabet,acoustic.AsSpan(99,16));
var candidateScorer=LegacyPreparedAcousticCandidateScorer.ForAlphabet(LegacyAcousticCandidateThresholds.ReadV110Node(acoustic.AsSpan(189734),out _),model.AcousticAlphabet);
var bridge=new LegacyCursiveFinalCacheGroupScorer(cache,candidateScorer,model.CreateCursiveCategoryMapper(1),LegacyCursiveSeparationProfile.ReadV106Node(acoustic,188157),LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[4])),model.AcousticAlphabet,model.LexicalAlphabet,Enumerable.Repeat((byte)1,model.AcousticAlphabet.LabelCount).ToArray(),LegacyWideSymbolPairs.CreateUnrestricted(model.AcousticAlphabet),relatedFormMode:0);
InkStroke Stroke(params(float X,float Y)[] pts)=>new(pts.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
var real=new[]{Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),Stroke((70,10),(70,90),(120,10),(120,90)),Stroke((140,90),(140,10),(180,10),(190,20),(190,40),(180,50),(140,50)),Stroke((10,0),(20,0)),Stroke((75,0),(80,0))};
var loop=new[]{Stroke((0,50),(0,20),(20,10),(40,20),(40,70),(20,90),(0,70),(0,50))};
var enclosed=new[]{Stroke((0,10),(40,10),(40,90),(0,90),(0,10)),Stroke((15,40),(20,45),(25,50))};
var options=new LegacyCursiveWordSearchOptions(90,80,0,1,BeamWidth:128,MaximumExpandedChoices:10_000_000);
var seedProfile=LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(acoustic,189379);var recognizer=new LegacyNativeGraphWordRecognizer(model);
var templateScope=new LegacyOrdinaryAcousticCacheContext(true,true,false,false,true,.37f,false,ReadOnlyMemory<ushort>.Empty);
foreach(var input in new[]{real,loop,enclosed})
{
 var snapshot=LegacyOrdinaryCursiveCutSource.Generate(input,profile,90,80);var metadata=new LegacyOrdinarySourceEndpointMetadata(snapshot,input.Select(s=>s.Points.Count).ToArray());
 if(input==enclosed)
 {
  Require(snapshot.GetSourceStrokeProbability(0)>0,"Enclosed fixture lacks positive native source probability.");
  var ranges=new[]{new LegacyInkRange(0,0,snapshot.Cuts.PreparedInk.StrokePointCounts.Span[0])};
  var glyph=LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(snapshot.Cuts.PreparedInk,ranges,0,90,80,100,10);
  var bound=metadata.BindContext(glyph,templateScope with{PruneCandidates=false});
  var costs=cache.Build(Enumerable.Repeat(10000,180).ToArray(),glyph.Strokes,Enumerable.Repeat((byte)1,model.AcousticAlphabet.LabelCount).ToArray(),bound,LegacyWideSymbolPairs.CreateUnrestricted(model.AcousticAlphabet));
  Require(model.AcousticAlphabet.TryGetLabel("\U000f0001",out ushort primitive) && costs.Costs.Span[primitive]<LegacyAcousticFixedLog.ImpossibleCost,"Actual final cache did not consume positive source-record primitive probability.");
 }
 LegacyOrdinaryAcousticCacheContext ExactScope(LegacyGraphGlyphPosition p,LegacyOrdinaryCursiveCandidateArc a,LegacyPreparedGlyphClassification glyph)
 {
  var scope=metadata.EvaluateOrderedRanges(glyph.Geometry.Ranges.Span,glyph.Geometry.Strokes.Count);
  return templateScope with{IsWholePhysicalStroke=scope.BoundaryAlignedSingleNormalizedStroke,WholeStrokeEndpointCategoriesZero=scope.EndpointFieldsAreZero,SourceStrokeProbability=scope.SourceStrokeProbability};
 }
 var computed=recognizer.RecognizeOrdinaryDeferred(input,profile,seedProfile,new byte[input.Length],Enumerable.Repeat(-1,input.Length).ToArray(),options,bridge,(p,a,g)=>templateScope,false,false,false,useNativeSourcePrimitiveScope:true);
 var explicitScope=recognizer.RecognizeOrdinaryDeferred(input,profile,seedProfile,new byte[input.Length],Enumerable.Repeat(-1,input.Length).ToArray(),options,bridge,ExactScope,false,false,false);
 Require(computed.Words.Count>0 && computed.Words.Select(w=>(w.Text,w.Match.AcousticCost,w.Match.Cost)).SequenceEqual(explicitScope.Words.Select(w=>(w.Text,w.Match.AcousticCost,w.Match.Cost))),"Opt-in source primitive/cache/lexical consumer differs from explicit native scope producer.");
 Console.WriteLine($"ACTUAL opt-in source fields: physical={input.Length}, probability={string.Join(",",snapshot.Source.SourceStrokeProbabilities.ToArray())}; {computed.ClassifiedGroups} glyphs, top={string.Join(",",computed.Words.Take(3).Select(w=>$"{w.Text}:{w.Match.Cost}"))}. Other scope/mask/frame flags explicit; no acceptance claim.");
}
// The zero-field shortcut is driven by raw endpoint ownership. A raw dot and
// two coincident raw points normalize identically, but native end-field3 is
// respectively0 and1; only the first gets the native minimum-minus-one rule.
var dotCosts=new List<int>();model.AcousticAlphabet.TryGetLabel(".",out ushort dot);
foreach(int rawCount in new[]{1,2})
{
 var input=new[]{new InkStroke(Enumerable.Repeat(new InkPoint(0,0),rawCount).ToArray())};var source=LegacyOrdinaryCursiveCutSource.Generate(input,profile,90,80);
 var metadata=new LegacyOrdinarySourceEndpointMetadata(source,new[]{rawCount});var ranges=new[]{new LegacyInkRange(0,0,1)};
 var glyph=LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(source.Cuts.PreparedInk,ranges,0,90,80,100,10);
 var context=metadata.BindContext(glyph,templateScope with{PruneCandidates=false});
 var output=cache.Build(Enumerable.Repeat(10000,180).ToArray(),glyph.Strokes,Enumerable.Repeat((byte)1,model.AcousticAlphabet.LabelCount).ToArray(),context,LegacyWideSymbolPairs.CreateUnrestricted(model.AcousticAlphabet));dotCosts.Add(output.Costs.Span[dot]);
}
Require(dotCosts[0]<dotCosts[1],"Native raw endpoint-zero shortcut was not consumed by actual final cache.");
Console.WriteLine("PASS opt-in graph-word native source-primitive binding, all other explicit context fields preserved, actual195-cache endpoint-zero rule and whole-word lexical consumer.");
static void Require(bool value,string message){if(!value)throw new InvalidDataException(message);}
