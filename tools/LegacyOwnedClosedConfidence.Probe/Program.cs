using InkRecognition;
using System.Text.Json;
if(args.Length!=3)throw new ArgumentException("actualAK.res engine-profile.json actualaudit.json");
byte[] resource=File.ReadAllBytes(args[0]);using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
void Require(bool value,string message){if(!value)throw new Exception(message);}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or NotSupportedException){return;}throw new Exception("Invalid closed confidence ownership admitted.");}
var producer=LegacyFirstOrdinaryGlyphScoreProducer.ReadFreshSelectedSource(resource,6120);var labelMap=LegacyAcousticConfidenceLabelMap.ReadFreshFirstSource(resource,6120);int groupCount=0,compared=0,actualRetained=0,mixedPhysicalGroups=0;var reports=new List<AuditRow>();
for(int iteration=0;iteration<32;iteration++)
{
 float w=60+iteration%5*5,h=80+iteration%7*3,scale=new[]{.5f,1f,2f}[iteration%3],dx=iteration/4*25,dy=-iteration/4*13;
 bool isolated=iteration%8==0,duplicate=iteration%4==1,reverse=iteration%2==1;
 InkPoint[] points=[new(0,0),new(w,0),new(w,h),new(0,h),new(0,0)];if(reverse)Array.Reverse(points);
 points=points.Select(p=>new InkPoint(p.X*scale+dx,p.Y*scale+dy)).ToArray();
 if(duplicate)points=points.SelectMany(p=>new[]{p,p,p}).ToArray();
 var raw=isolated?new[]{new InkStroke(points)}:new[]{new InkStroke(points),Draw((w*.3f*scale+dx,h*.3f*scale+dy),(w*.4f*scale+dx,h*.6f*scale+dy))};
 var factory=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,6120,raw);var row=factory.Frame.Row;var cuts=row.GenerateRussianCuts();
 var policy=LegacyFreshFirstControllerPolicy.ReadFirstSource(resource,6120);var scope=new LegacyFirstRowPrimitiveScope(row,cuts,raw.Select(s=>s.Points.Count).ToArray());
 var scorer=LegacyFirstOrdinaryCacheScorer.Prepare(resource,6120,row,activations);var mask=Enumerable.Repeat((byte)1,scorer.Alphabet.LabelCount).ToArray();
 var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,12727),new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());
 var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(seeds),5);
 var scores=new LegacyFirstOrdinaryCacheScores[graph.Groups.Count];
 for(int g=0;g<scores.Length;g++)
 {
  var ranges=graph.Groups[g].Ranges.ToArray();var order=cuts.Chronology.OrderRangeIndicesForNormalization(ranges);var normalized=order.Select(i=>ranges[i]).ToArray();var clip=LegacyCursiveGlyphBoundaryPolicy.Resolve(normalized,cuts.Cuts.PreparedInk.StrokePointCounts.Span,0,false,false);
  scores[g]=scorer.Score(cuts,ranges,clip.Start,clip.End,15,Math.Min(256,policy.MaximumNormalizedStrokes),mask,glyph=>policy.CreateContext(scope.Evaluate(ranges,glyph.MergedGeometry.Strokes.Count),[]),[]);
 }
 Require(scorer.Alphabet.TryGetLabel("\U000f0001",out ushort closed),"Actual closed primitive missing.");
 var owned=new LegacyFirstOwnedGlyphConfidence(producer,factory,cuts,graph,scores);
 var controlled=scores.Select(s=>s with{Cache=s.Cache with{Candidates=new LegacyAcousticCacheCandidate[]{new(closed,0)}}}).ToArray();
 var controlledOwned=new LegacyFirstOwnedGlyphConfidence(producer,factory,cuts,graph,controlled);
 for(int g=0;g<scores.Length;g++)
 {
  int physical=graph.Groups[g].Ranges.Span[0].StrokeIndex;float expected=row.OriginalSource.SourceStrokeProbabilities.Span[physical];
  Require(BitConverter.SingleToUInt32Bits(controlledOwned.ReadSymbolScore(g,closed))==BitConverter.SingleToUInt32Bits(expected),"Closed branch changed raw source probability, range order, or rounding.");
  if(expected>0&&graph.Groups[g].Ranges.ToArray().Any(r=>r.StrokeIndex!=physical))mixedPhysicalGroups++;
  int rank=Array.FindIndex(scores[g].Cache.Candidates.ToArray(),v=>v.Symbol==closed);float actual=owned.ReadSymbolScore(g,closed);
  Require(BitConverter.SingleToUInt32Bits(actual)==BitConverter.SingleToUInt32Bits(rank>=0&&rank<20?expected:0),"Actual cache retention/cutoff or source closed confidence differs.");
  if(rank>=0&&rank<20)actualRetained++;
  foreach(var candidate in scores[g].Cache.Candidates.Span)
   if(candidate.Symbol!=closed&&labelMap.TryResolveOutputLabel(candidate.Symbol,out _))Require(BitConverter.SingleToUInt32Bits(owned.ReadSymbolScore(g,candidate.Symbol))==BitConverter.SingleToUInt32Bits(producer.ReadSymbolScore(scores[g].Glyph.Scores,scores[g].Cache,candidate.Symbol)),"Owned branch altered normal/mapped neural confidence.");
  groupCount++;
 }
 float enclosure=row.OriginalSource.SourceStrokeProbabilities.Span[0];float analytic=isolated?0:12.566f*w*h/MathF.Pow(2*w+2*h,2);
 Require(MathF.Abs(enclosure-analytic)<.0002f,"Original raw enclosure producer disagrees with independent rectangle area/perimeter and strict cross-stroke condition.");
 reports.Add(new(iteration,w,h,scale,dx,dy,isolated,duplicate,reverse,row.OriginalSource.SourceStrokeProbabilities.ToArray(),scores.Length));
 if(iteration==1)
 {
  Reject(()=>producer.ReadSymbolScore(controlled[0].Glyph.Scores,controlled[0].Cache,closed));
  Reject(()=>controlledOwned.ReadSymbolScore(0,closed,new CancellationToken(true)));
  var foreign=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,6120,raw);Reject(()=>new LegacyFirstOwnedGlyphConfidence(producer,foreign,cuts,graph,controlled));
  var permuted=controlled.ToArray();int different=Enumerable.Range(1,permuted.Length-1).First(i=>!permuted[i].Glyph.Geometry.Ranges.Span.SequenceEqual(permuted[0].Glyph.Geometry.Ranges.Span));permuted[0]=permuted[different];
  var badGroup=new LegacyFirstOwnedGlyphConfidence(producer,factory,cuts,graph,permuted);Reject(()=>badGroup.ReadSymbolScore(0,closed));
  var cutoff=controlled.ToArray();cutoff[0]=cutoff[0] with{Cache=cutoff[0].Cache with{Candidates=Enumerable.Range(0,20).Select(v=>new LegacyAcousticCacheCandidate((ushort)v,0)).Append(new(closed,0)).ToArray()}};
  Require(new LegacyFirstOwnedGlyphConfidence(producer,factory,cuts,graph,cutoff).ReadSymbolScore(0,closed)==0,"Owned primitive branch bypasses rank20 cutoff.");
 }
 compared+=scores.Length;
}
using(var output=File.Create(args[2]))using(var writer=new Utf8JsonWriter(output))
{
 Require(mixedPhysicalGroups>0,"Controls must distinguish first physical probability from range averaging.");
 writer.WriteStartObject();writer.WriteNumber("groupCount",groupCount);writer.WriteNumber("compared",compared);writer.WriteNumber("actualRetained",actualRetained);writer.WriteNumber("mixedPhysicalGroups",mixedPhysicalGroups);writer.WriteStartArray("cases");
 foreach(var report in reports)
 {
  writer.WriteStartObject();writer.WriteNumber("iteration",report.Iteration);writer.WriteNumber("w",report.Width);writer.WriteNumber("h",report.Height);writer.WriteNumber("scale",report.Scale);writer.WriteNumber("dx",report.Dx);writer.WriteNumber("dy",report.Dy);
  writer.WriteBoolean("isolated",report.Isolated);writer.WriteBoolean("duplicate",report.Duplicate);writer.WriteBoolean("reverse",report.Reverse);writer.WriteNumber("groups",report.Groups);
  writer.WriteStartArray("probabilities");foreach(float value in report.Probabilities)writer.WriteNumberValue(value);writer.WriteEndArray();writer.WriteEndObject();
 }
 writer.WriteEndArray();writer.WriteEndObject();
}
Console.WriteLine($"PASS32 actual selected-source ingress/frame/cuts/NN/final-cache cases, {groupCount} owned graph groups and {actualRetained} actually retained closed candidates; controlled closed vectors use raw first physical source probability. Foreign source/cuts/group and cancellation guarded; no primitive decoding/OCR accuracy claim.");
static InkStroke Draw(params(float X,float Y)[] values)=>new(values.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
sealed record AuditRow(int Iteration,float Width,float Height,float Scale,float Dx,float Dy,bool Isolated,bool Duplicate,bool Reverse,float[] Probabilities,int Groups);
