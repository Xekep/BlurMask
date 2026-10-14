using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if(args.Length!=5)throw new ArgumentException("AK, engine profile, separation profile and audit output required.");
byte[] resource=File.ReadAllBytes(args[0]);using var engine=JsonDocument.Parse(File.ReadAllBytes(args[2]));var a=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var logTable=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
InkStroke[] raw=[Draw((0,90),(0,90),(0,10),(25,60),(50,10),(50,90))];
var sampler=LegacyFreshSingleGlyphMainSampler.Prepare(resource,0,raw[0],0);
var post=sampler.Postprocessing;
var main=post.Input;
var factory=main.FirstFactory;var frame=factory.Frame;var row=frame.Row;var cuts=row.GenerateRussianCuts();
if(row.OriginalSource.Strokes[0].Points.Count!=5||main.Controller.Source.Strokes[0].Points.Count!=5)
 throw new Exception("Actual parent retained submission lost duplicate chronology.");
if(!main.Controller.Source.OriginalPointIndices[0].Span.SequenceEqual(new[]{0,2,3,4,5})||
 !row.OriginalSource.OriginalPointIndices[0].Span.SequenceEqual(new[]{0,1,2,3,4}))
 throw new Exception("Child raw source ordinals were replaced by the user's duplicate-containing ordinal owner.");
 var profile=LegacyFirstTransitionGeometryProfile.ReadSource(resource,6120);var categories=new LegacyCursiveCategoryMapper(profile.Categories);
 var policy=LegacyFreshFirstControllerPolicy.ReadFirstSource(resource,6120);var scope=new LegacyFirstRowPrimitiveScope(row,cuts,row.OriginalSource.Strokes.Select(s=>s.Points.Count).ToArray());
 var scorer=LegacyFirstOrdinaryCacheScorer.Prepare(resource,6120,row,activations);var mask=Enumerable.Repeat((byte)1,scorer.Alphabet.LabelCount).ToArray();
 var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,12727),new byte[row.OriginalSource.Strokes.Count],Enumerable.Repeat(-1,row.OriginalSource.Strokes.Count).ToArray());
 var sourceRanges=LegacyFreshFirstCandidateGraphPreparer.Prepare(resource,factory,cuts,seeds,logTable).SourceRanges;var graph=sourceRanges.Graph;var links=new LegacyOrdinaryCandidateConnections(graph);
 var geometry=new LegacyFirstOrdinaryTransitionGeometry(profile,frame,cuts,graph,logTable);var separation=LegacyCursiveSeparationProfile.ReadV106Node(resource,profile.SamplerOffset);
 var scores=new LegacyFirstOrdinaryCacheScores[graph.Groups.Count];var ranked=new LegacyOrdinaryRankedAcousticGroup[graph.Groups.Count];
 for(int g=0;g<scores.Length;g++)
 {
  var ranges=graph.Groups[g].Ranges.ToArray();var order=cuts.Chronology.OrderRangeIndicesForNormalization(ranges);var normalized=order.Select(i=>ranges[i]).ToArray();var clip=LegacyCursiveGlyphBoundaryPolicy.Resolve(normalized,cuts.Cuts.PreparedInk.StrokePointCounts.Span,0,false,false);
  scores[g]=scorer.Score(cuts,ranges,clip.Start,clip.End,15,Math.Min(256,policy.MaximumNormalizedStrokes),mask,glyph=>policy.CreateContext(scope.Evaluate(ranges,glyph.MergedGeometry.Strokes.Count),[]),[]);
  int cost=LegacyCursiveGroupSeparationCost.Evaluate(cuts.Cuts.PreparedInk,ranges,row.Frame.Line.Height,separation.GapOffset,separation.Slope,logTable.Values.Span,separation.MaximumCost);
  ranked[g]=new(scores[g].Cache,scorer.Alphabet,resource.AsSpan(scorer.AlphabetOffset,16),Enumerable.Repeat(cost,categories.CompositeCategoryCount).ToArray(),1);
 }
 var requests=Enumerable.Range(0,graph.NodeCount).Select(_=>new List<LegacyFirstOrdinaryGlyphRequest>()).ToArray();
 for(int node=0;node<graph.NodeCount;node++)for(byte span=(byte)Math.Min(4,node);;span--)
 {
  var arcs=graph.GetNode(node);for(int ordinal=0;ordinal<arcs.Count;ordinal++)if(arcs[ordinal].Span==span)
  {
   var group=ranked[arcs[ordinal].GroupId];foreach(var entry in group.Entries.Span[..Math.Min(20,group.Entries.Length)])
   {
    if(!scorer.Alphabet.TryGetText(entry.Symbol,out string text)||text.Length!=1||!(char.IsAsciiLetterOrDigit(text[0])||text[0] is >= '\u0400' and <= '\u04ff'))continue;
    foreach(bool end in new[]{false,true}){int category=categories.GetSymbolCompositeCategory(entry.Symbol,end);if(category>=0&&categories.GetFirstId(category)<=5)requests[node].Add(new((ushort)ordinal,span,entry.Symbol,(byte)category,group.Query(entry.Symbol,ushort.MaxValue,category,false,false),group.GetFirstControllerLikelihoodByte(entry.Symbol)));}
   }
  }if(span==0)break;
 }
 var primary=LegacyFirstOrdinaryAcousticRecurrence.Run(links,requests,5,geometry.Correct118,geometry.Correct140);
 int GlyphCost(int n,ushort arc,ushort symbol,byte category,CancellationToken token)=>ranked[graph.GetNode(n)[arc].GroupId].Query(symbol,ushort.MaxValue,category,true,false,token);
 var merged=LegacyFirstOrdinaryContextPasses.RunForFreshSelectedChild(primary,links,factory.InvocationPolicy.Binding,GlyphCost,geometry.Correct118,geometry.Correct140,new());

 var materializer=new LegacyFirstPlainWordMaterializer(resource,6120,factory,logTable);var result=materializer.Materialize(cuts,graph,primary,merged,scores);
var sampled=sampler.ApplyFirstResult(result,sourceRanges,logTable);
var prepared=sampled.Preparation;
var mainGeometry=sampled.CreatePrepared240TransitionGeometry(logTable);
var mainModel=LegacyRussianModelBundle.Load(resource,File.ReadAllBytes(args[1]),File.ReadAllBytes(args[2]));
//Own explicit unrestricted prepared source token. Tests mask transport, not
//a claim that a live linguistic owner automatically emits this token.
var seed=LegacyLinguisticSymbolMaskController.Build(mainModel.AcousticAlphabet.LabelCount,[new LegacyLinguisticSymbolSource(new uint[]{0xffe3},ReadOnlyMemory<ushort>.Empty)],true);
var producer=new LegacyOrdinarySymbolMaskProducer(mainModel.AcousticAlphabet,resource.AsSpan(99,16));
var nativePairs=LegacyWideSymbolPairs.CreateUnrestricted(mainModel.AcousticAlphabet);
var mainMask=producer.Build(seed.Outcome,seed.Mask.Span,[],[],[],nativePairs);
var mainScorer=LegacyPreparedFreshMain240AcousticScorer.Prepare(resource,mainModel,sampled,mainMask);
if(mainScorer.Policy.FrontendFlags!=0x21400||mainScorer.Policy.MaximumSourceArcs!=125||mainScorer.Policy.SourceAlternativeLimit!=20||!mainScorer.Policy.PruneCandidates||mainScorer.Policy.MaximumNormalizedStrokes!=10||mainScorer.ContextWindowCount!=0||mainScorer.CacheWidth!=195)throw new Exception("Actual main policy/context/cache differs.");
short[] Calibration(string part,string field)=>engine.RootElement.GetProperty(part).GetProperty(field).EnumerateArray().Select(v=>v.GetInt16()).ToArray();
var temporalMin=Calibration("temporal","minima");var temporalMax=Calibration("temporal","maxima");var spatialMin=Calibration("spatial","minima");var spatialMax=Calibration("spatial","maxima");
var mainScores=Enumerable.Range(0,sampled.SourceRanges.Graph.Groups.Count).Select(g=>mainScorer.Score(g)).ToArray();
if(mainScores.Any(s=>s.Glyph.Scores.FamilyProbabilities.Take(4).Any(p=>p is null)||s.Glyph.Scores.FamilyProbabilities.Skip(4).Any(p=>p is not null)||s.Cache.Costs.Length!=195||s.Context.Span.ToArray().Any(v=>v!=-1024)))throw new Exception("Actual four-family/main cache context differs.");
var restrictedPrimary=mainMask.Primary.ToArray();restrictedPrimary[107]=0;
var restricted=LegacyPreparedFreshMain240AcousticScorer.Prepare(resource,mainModel,sampled,new(restrictedPrimary,mainMask.Secondary));
var restrictedScores=restricted.Score(0);
if(restrictedScores.Glyph.IntegerCosts.Span[107]!=0x4000000||restrictedScores.Cache.Costs.Span[107]!=0x4000000||!restrictedScores.Glyph.Scores.FamilyProbabilities[0]!.Value.Span.SequenceEqual(mainScores[0].Glyph.Scores.FamilyProbabilities[0]!.Value.Span))throw new Exception("Prepared mask changed raw probabilities or bypassed integer/cache masks.");
Reject(()=>mainScorer.Score(-1));Reject(()=>mainScorer.Score(0,new CancellationToken(true)));
Reject(()=>LegacyPreparedFreshMain240AcousticScorer.Prepare(resource,mainModel,sampled,new(new byte[1],new byte[1])));
if(sampler.CategoryOwner.SelectedSourceOffset!=0||sampler.CategoryOwner.CategoryProfileOffset!=188222||sampler.SpanProfile.MaximumAtoms!=8||sampler.AcousticOffset!=189734||sampler.AcousticTargetHeight!=100)throw new Exception("Main profile ownership differs.");
Reject(()=>sampled.NormalizeGroup(-1));
for(int n=0;n<sampled.SourceRanges.Graph.NodeCount;n++)for(int arc=0;arc<sampled.SourceRanges.Graph.GetNode(n).Count;arc++)if(sampled.GetSourceLineIdForArc(n,arc)!=0)throw new Exception("Actual E8 source line differs.");
Reject(()=>sampled.GetSourceLineIdForArc(0,int.MaxValue));
Reject(()=>sampler.ApplyFirstResult(result,sourceRanges,logTable,new CancellationToken(true)));
var badPrefix=resource.ToArray();badPrefix[sampler.CategoryOwner.SamplerNodeOffset+16]^=1;
Reject(()=>LegacyFreshSingleGlyphMainSampler.Prepare(badPrefix,0,raw[0],0));
if(prepared.SourceGap!=1.5f||prepared.Shear!=0||prepared.Smoothed.Strokes.Count!=1||prepared.Smoothed.Strokes[0].Points.Count<=5)
 throw new Exception("Actual owned main24D/29B/265 output differs.");
var unchangedGuide=prepared.LinePreparation.Output.CopyGuides()[0];
if(!prepared.CopyGuide().Span.SequenceEqual(unchangedGuide))throw new Exception("24D/29B/265 must retain actual main guide bytes.");
for(int p=0;p<prepared.Smoothed.Strokes[0].Points.Count;p++)
 if(prepared.Chronology.Resolve(0,p).Key.First!=0)throw new Exception("Generated265 key lost its actual retained metadata owner.");
Reject(()=>post.ApplyFirstResult(result,sourceRanges,new CancellationToken(true)));
var changed=resource.ToArray();BinaryPrimitives.WriteSingleLittleEndian(changed.AsSpan(post.Processor29BOffset+42),2.75f);
var changedPost=LegacyFreshSingleGlyphMainPostprocessing.Prepare(changed,0,raw[0],0);
if(changedPost.SerializedZeroGap!=2.75f)throw new Exception("29B serialized gap was replaced with a default.");
Reject(()=>changedPost.ApplyFirstResult(result,sourceRanges));
var invalid=resource.ToArray();invalid[post.Processor24DOffset+17]=invalid[post.Processor24DOffset+16];
Reject(()=>LegacyFreshSingleGlyphMainPostprocessing.Prepare(invalid,0,raw[0],0));
using var stream=File.Create(args[4]);using var writer=new Utf8JsonWriter(stream);
writer.WriteStartObject();writer.WriteNumber("groups",scores.Length);writer.WriteNumber("sourceRows",prepared.LinePreparation.SourceRows.Count);
writer.WriteNumber("shear",prepared.Shear);writer.WriteNumber("gap",prepared.SourceGap);
writer.WriteString("guide",Convert.ToBase64String(prepared.CopyGuide().Span));
writer.WriteString("before720",Convert.ToBase64String(prepared.LinePreparation.Output.Deskew!.Rows[0].CopyStorage().Span));
writer.WriteString("after720",Convert.ToBase64String(prepared.SourceRow.CopyStorage().Span));
writer.WriteStartArray("ranges");foreach(var r in prepared.SourceRow.Ranges){writer.WriteStartArray();foreach(var v in new[]{r.First.First,r.First.Second,r.First.Third,r.Last.First,r.Last.Second,r.Last.Third})writer.WriteNumberValue(v);writer.WriteEndArray();}writer.WriteEndArray();
writer.WriteStartArray("original");foreach(var p in main.Controller.Source.Strokes[0].Points){writer.WriteStartArray();writer.WriteNumberValue(p.X);writer.WriteNumberValue(p.Y);writer.WriteEndArray();}writer.WriteEndArray();
writer.WriteStartArray("before265");foreach(var p in prepared.CopyBefore265Points()[0]){writer.WriteStartArray();writer.WriteNumberValue(p.X);writer.WriteNumberValue(p.Y);writer.WriteEndArray();}writer.WriteEndArray();
writer.WriteStartArray("smoothed");foreach(var p in prepared.Smoothed.Strokes[0].Points){writer.WriteStartArray();writer.WriteNumberValue(p.X);writer.WriteNumberValue(p.Y);writer.WriteEndArray();}writer.WriteEndArray();
writer.WriteStartArray("keys200");foreach(int key in prepared.Smoothed.PointKeys200.Span)writer.WriteNumberValue(key);writer.WriteEndArray();
writer.WriteStartArray("sourceKeys");for(int point=0;point<prepared.Smoothed.Strokes[0].Points.Count;point++)
{var key=prepared.Chronology.Resolve(0,point).Key;writer.WriteStartArray();writer.WriteNumberValue(key.First);writer.WriteNumberValue(key.Second);writer.WriteNumberValue(key.Third);writer.WriteEndArray();}writer.WriteEndArray();
writer.WriteStartArray("metrics");writer.WriteNumberValue(prepared.OriginalSourceMetrics.OrthogonalFraction);writer.WriteNumberValue(prepared.OriginalSourceMetrics.CornerFraction);writer.WriteEndArray();
writer.WriteStartArray("profile");foreach(float v in new[]{post.Smoothing.Minimum,post.Smoothing.Maximum,post.Smoothing.Extra,post.Smoothing.OrthogonalLow,post.Smoothing.OrthogonalHigh,post.Smoothing.CornerLow,post.Smoothing.CornerHigh,post.Smoothing.FlatnessSquaredFactor})writer.WriteNumberValue(v);writer.WriteEndArray();
writer.WriteNumber("weight",prepared.Smoothed.SmoothingWeight);
writer.WriteNumber("mainGroups",sampled.SourceRanges.Graph.Groups.Count);writer.WriteNumber("mainGraphCap",sampler.SpanProfile.MaximumAtoms);
writer.WriteNumber("mainRemoved",sampled.Pruning.RemovedArcCount);writer.WriteNumber("target",sampler.AcousticTargetHeight);
writer.WriteNumber("baseline",sampled.SourceBaseline);writer.WriteNumber("height",sampled.SourceHeight);
writer.WriteNumber("sourceProbability",main.Controller.Source.SourceStrokeProbabilities.Span[0]);
writer.WriteStartArray("cutRows");foreach(var q in sampled.Cuts.PreparedInk.Points.Span){writer.WriteStartArray();foreach(var value in new[]{q.X,q.Y,q.DeltaX,q.DeltaY,q.SegmentLength})writer.WriteNumberValue(value);writer.WriteNumberValue(q.Tag);writer.WriteEndArray();}writer.WriteEndArray();
writer.WriteStartArray("cutMap");foreach(uint key in sampled.Chronology.PreparedPointMap.Span)writer.WriteNumberValue(key);writer.WriteEndArray();
writer.WriteStartArray("cutSourceKeys");for(int i=0;i<sampled.Cuts.PreparedInk.StrokePointCounts.Span[0];i++){var identity=sampled.Chronology.Resolve(0,i);writer.WriteStartArray();writer.WriteNumberValue(identity.Key.First);writer.WriteNumberValue(identity.Key.Second);writer.WriteNumberValue(identity.Key.Third);writer.WriteNumberValue(identity.RemainderByte);writer.WriteEndArray();}writer.WriteEndArray();
writer.WriteStartArray("normalized");for(int g=0;g<sampled.SourceRanges.Graph.Groups.Count;g++){var normal=sampled.NormalizeGroup(g);writer.WriteStartObject();writer.WriteStartArray("ranges");foreach(var r in normal.Ranges.Span){writer.WriteStartArray();writer.WriteNumberValue(r.StrokeIndex);writer.WriteNumberValue(r.StartPoint);writer.WriteNumberValue(r.EndPointExclusive);writer.WriteEndArray();}writer.WriteEndArray();writer.WriteStartArray("points");foreach(var stroke in normal.Strokes)foreach(var point in stroke.Points){writer.WriteStartArray();writer.WriteNumberValue(point.X);writer.WriteNumberValue(point.Y);writer.WriteEndArray();}writer.WriteEndArray();writer.WriteEndObject();}writer.WriteEndArray();
writer.WriteStartObject("specialLabels");foreach(string text in new[]{"-","_","\u30fc","\u4e00","\U000f0001","\U000f0002","\U000f0003","\uFFFD",".","\u00ad","\'","\""})if(mainModel.AcousticAlphabet.TryGetLabel(text,out ushort label))writer.WriteNumber(text,label);writer.WriteEndObject();
writer.WriteStartArray("cacheProfile");foreach(float v in new[]{mainScorer.CacheProfile.FallbackPenalty,mainScorer.CacheProfile.LongDashWeight,mainScorer.CacheProfile.CjkDashWeight,mainScorer.CacheProfile.NegativeDirectionDivisor,mainScorer.CacheProfile.PositiveDirectionDivisor,mainScorer.CacheProfile.ClosedPrimitiveWeight})writer.WriteNumberValue(v);writer.WriteNumberValue(mainScorer.CacheProfile.CandidateThreshold);writer.WriteNumberValue(mainScorer.CacheProfile.MinimumCandidates);writer.WriteNumberValue(mainScorer.CacheProfile.CandidateLimit);writer.WriteEndArray();
writer.WriteStartObject("mainPolicy");writer.WriteNumber("flags",mainScorer.Policy.FrontendFlags);writer.WriteNumber("arcs",mainScorer.Policy.MaximumSourceArcs);writer.WriteNumber("aux",mainScorer.Policy.SourceAuxiliaryLimit);writer.WriteNumber("alternatives",mainScorer.Policy.SourceAlternativeLimit);writer.WriteNumber("penup",mainScorer.Policy.MaximumNormalizedStrokes);writer.WriteBoolean("prune",mainScorer.Policy.PruneCandidates);writer.WriteEndObject();
writer.WriteStartArray("mask");foreach(byte flag in mainMask.Primary.Span)writer.WriteNumberValue(flag);writer.WriteEndArray();
writer.WriteStartArray("acousticGroups");foreach(var scored in mainScores){writer.WriteStartObject();
var geo=scored.Glyph.Geometry;var cx=scored.Context.Span;var bounds=scored.BoundarySigns;
short[][] inputs=[LegacyStrokeFeatures.ExtractInputWithBaseline(geo.Strokes,geo.Baseline,cx,bounds.Start,bounds.End),LegacyRasterFeatures.ExtractInputWithBaseline(geo.Strokes,geo.Baseline,cx,bounds.Start,bounds.End),LegacyTemporalDirectionalFeatures.Extract(geo.Strokes,geo.Baseline,temporalMin,temporalMax,cx,bounds.Start,bounds.End),LegacySpatialDirectionalFeatures.Extract(geo.Strokes,geo.Baseline,spatialMin,spatialMax,cx,bounds.Start,bounds.End)];
writer.WriteStartArray("inputs");foreach(var vector in inputs){writer.WriteStartArray();foreach(short v in vector)writer.WriteNumberValue(v);writer.WriteEndArray();}writer.WriteEndArray();writer.WriteStartArray("probabilities");foreach(var family in scored.Glyph.Scores.FamilyProbabilities.Take(4)){writer.WriteStartArray();foreach(int v in family!.Value.Span)writer.WriteNumberValue(v);writer.WriteEndArray();}writer.WriteEndArray();writer.WriteStartArray("weights");foreach(float v in scored.Glyph.ResolvedWeights.Span)writer.WriteNumberValue(v);writer.WriteEndArray();writer.WriteStartArray("confidence");foreach(float v in scored.Glyph.Scores.Confidence.Span)writer.WriteNumberValue(v);writer.WriteEndArray();writer.WriteStartArray("integerCosts");foreach(int v in scored.Glyph.IntegerCosts.Span)writer.WriteNumberValue(v);writer.WriteEndArray();writer.WriteStartArray("cacheCosts");foreach(int v in scored.Cache.Costs.Span)writer.WriteNumberValue(v);writer.WriteEndArray();writer.WriteStartArray("candidates");foreach(var v in scored.Cache.Candidates.Span){writer.WriteStartArray();writer.WriteNumberValue(v.Symbol);writer.WriteNumberValue(v.Cost);writer.WriteEndArray();}writer.WriteEndArray();writer.WriteStartArray("boundaries");writer.WriteNumberValue(scored.BoundarySigns.Start);writer.WriteNumberValue(scored.BoundarySigns.End);writer.WriteEndArray();writer.WriteBoolean("whole",scored.PrimitiveScope.BoundaryAlignedSingleNormalizedStroke);writer.WriteBoolean("zeroEndpoints",scored.PrimitiveScope.EndpointFieldsAreZero);writer.WriteEndObject();}writer.WriteEndArray();
writer.WriteEndObject();writer.Flush();
Console.WriteLine($"PASS actual raw/first NN{scores.Length}groups ->source720/24B/24A-assigned/24D-zero-gap/29B-loaded/265:{prepared.Smoothed.Strokes[0].Points.Count}points +factor200chronology. Main sampler/ownedcap8/prune/normalization/transition geometry also passed; Actual main4EXPfamilies/180integers/195cache also passed; prepared mask remains explicit and finalmainwinner excluded.");
static InkStroke Draw(params(float X,float Y)[]points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
static void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException or InvalidDataException){return;}throw new Exception("Invalid ownership/profile admitted.");}
