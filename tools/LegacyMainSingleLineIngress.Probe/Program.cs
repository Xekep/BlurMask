using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
if(args.Length!=9)throw new ArgumentException("AK, engineprofile,separationprofile,line30,line50,auditoutput,case-data,publication-data,LK.");
byte[] ak=File.ReadAllBytes(args[0]),line30=File.ReadAllBytes(args[3]),line50=File.ReadAllBytes(args[4]);
using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));
var raw=new List<InkStroke>();
for(int i=0;i<14;i++)raw.Add(new InkStroke(new InkPoint[]{new(i*70,90),new(i*70,10),new(i*70+25,60),new(i*70+50,10),new(i*70+50,90)}));
var grouping=LegacyFreshMainPhysicalGrouping.Prepare(ak,line30,line50,raw,0);
var reordered=LegacyFreshMainPhysicalReordering.Prepare(LegacyFreshMainDescriptorSplit.Prepare(grouping),line30,line50);
var invocation=LegacyFreshMainFirstResultLines.Prepare(ak,reordered);
var words=invocation.FirstSubmissions.Select(Build).ToArray();var result=invocation.ApplyFirstResults(words);
using var caseStream=File.OpenRead(args[6]);var caseData=LegacyNativeCaseData.Load(caseStream);
using var publicationStream=File.OpenRead(args[7]);var publication=LegacyNativeTextPublicationData.Load(publicationStream);
var shear=LegacyFreshMainShearPreparation.Prepare(ak,result,caseData,publication);
var active=LegacyFreshMainActiveGapPreparation.Prepare(ak,shear);
var smoothed=active.Smooth(ak);
var sampler=LegacyFreshMainSingleLineSampler.Prepare(ak,smoothed);
var sampled=sampler.Sample(table);
var mainContext=LegacyFreshMainSingleLineContext.Prepare(ak,sampled,activations);
var model=LegacyRussianModelBundle.Load(ak,File.ReadAllBytes(args[8]),File.ReadAllBytes(args[1]));
var dictionary=LegacyOwnedRussianDictionaryPreparation.Prepare(ak,File.ReadAllBytes(args[8]),model,sampled);
var masks=dictionary.CopyMasks();var scorer=dictionary.CreateAcousticScorer(ak,mainContext);
Throws<InvalidOperationException>(()=>dictionary.CreateAcousticScorer(ak));
Require(mainContext.Windows.Count==36&&mainContext.Groups.Count>0,"Main14physical must evaluate nonempty actual189755 context windows.");
Console.WriteLine($"ACTUAL main14physical: {sampled.Cuts.PreparedInk.Points.Length} cutpoints/{sampled.SourceRanges.Graph.Groups.Count} groups, {mainContext.Windows.Count} context windows/{mainContext.Groups.Count} accepted.");
var scores=Enumerable.Range(0,sampled.SourceRanges.Graph.Groups.Count).Select(g=>scorer.Score(g)).ToArray();
Throws<OperationCanceledException>(()=>sampler.Sample(table,new(true)));
Throws<OperationCanceledException>(()=>LegacyFreshMainSingleLineContext.Prepare(ak,sampled,activations,new(true)));
Throws<OperationCanceledException>(()=>scorer.Score(0,new(true)));
Require(scorer.Policy.FrontendFlags==0x21400&&scorer.Policy.MaximumNormalizedStrokes==10&&sampler.SpanProfile.MaximumAtoms==8,"Actualmain caps/mode metadata.");
using(var stream=File.Create(args[5]))using(var w=new Utf8JsonWriter(stream))
{
 w.WriteStartObject();w.WriteNumber("height",sampler.SourceHeight);w.WriteNumber("baseline",sampler.SourceBaseline);w.WriteNumber("target",sampler.AcousticTargetHeight);w.WriteNumber("gap",smoothed.SourceGap);w.WriteNumber("removed",sampled.Pruning.RemovedArcCount);
 w.WriteStartArray("cacheProfile");foreach(float v in new[]{scorer.CacheProfile.FallbackPenalty,scorer.CacheProfile.LongDashWeight,scorer.CacheProfile.CjkDashWeight,scorer.CacheProfile.NegativeDirectionDivisor,scorer.CacheProfile.PositiveDirectionDivisor,scorer.CacheProfile.ClosedPrimitiveWeight})w.WriteNumberValue(v);w.WriteNumberValue(scorer.CacheProfile.CandidateThreshold);w.WriteNumberValue(scorer.CacheProfile.MinimumCandidates);w.WriteNumberValue(scorer.CacheProfile.CandidateLimit);w.WriteEndArray();
 w.WriteStartObject("labels");for(ushort label=0;label<model.AcousticAlphabet.LabelCount;label++)if(model.AcousticAlphabet.TryGetText(label,out string text))w.WriteNumber(text,label);w.WriteEndObject();
 w.WriteStartArray("mask");foreach(byte v in masks.Primary.Span)w.WriteNumberValue(v);w.WriteEndArray();
 w.WriteStartArray("points");int at=0;foreach(int count in sampled.Cuts.PreparedInk.StrokePointCounts.Span){w.WriteStartArray();for(int n=0;n<count;n++){var point=sampled.Cuts.PreparedInk.Points.Span[at++];w.WriteStartArray();w.WriteNumberValue(point.X);w.WriteNumberValue(point.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("windows");foreach(var window in mainContext.Windows){w.WriteStartObject();w.WriteNumber("start",window.StartPhysical);w.WriteNumber("end",window.EndPhysical);w.WriteNumber("center",window.CenterX);w.WriteBoolean("accepted",window.Accepted);w.WriteStartArray("features");foreach(float v in window.Features.Span)w.WriteNumberValue(v);w.WriteEndArray();w.WriteStartArray("fixed");foreach(short v in window.FixedInput.Span)w.WriteNumberValue(v);w.WriteEndArray();w.WriteStartArray("probabilities");foreach(int v in window.Probability32768.Span)w.WriteNumberValue(v);w.WriteEndArray();w.WriteEndObject();}w.WriteEndArray();
 w.WriteStartArray("groups");
 short[] Calibration(string part,string field)=>engine.RootElement.GetProperty(part).GetProperty(field).EnumerateArray().Select(v=>v.GetInt16()).ToArray();
 var tm=Calibration("temporal","minima");var tx=Calibration("temporal","maxima");var sm=Calibration("spatial","minima");var sx=Calibration("spatial","maxima");
 for(int i=0;i<scores.Length;i++)
 {
  var score=scores[i];var geo=score.Glyph.Geometry;var bounds=score.BoundarySigns;var cx=score.Context.Span;
  short[][] inputs=[LegacyStrokeFeatures.ExtractInputWithBaseline(geo.Strokes,geo.Baseline,cx,bounds.Start,bounds.End),LegacyRasterFeatures.ExtractInputWithBaseline(geo.Strokes,geo.Baseline,cx,bounds.Start,bounds.End),LegacyTemporalDirectionalFeatures.Extract(geo.Strokes,geo.Baseline,tm,tx,cx,bounds.Start,bounds.End),LegacySpatialDirectionalFeatures.Extract(geo.Strokes,geo.Baseline,sm,sx,cx,bounds.Start,bounds.End)];
  w.WriteStartObject();w.WriteStartArray("ranges");foreach(var range in geo.Ranges.Span){w.WriteStartArray();w.WriteNumberValue(range.StrokeIndex);w.WriteNumberValue(range.StartPoint);w.WriteNumberValue(range.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();
  w.WriteStartArray("normalized");foreach(var stroke in geo.Strokes){w.WriteStartArray();foreach(var point in stroke.Points){w.WriteStartArray();w.WriteNumberValue(point.X);w.WriteNumberValue(point.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
  w.WriteStartArray("context");foreach(short v in cx)w.WriteNumberValue(v);w.WriteEndArray();w.WriteStartArray("inputs");foreach(var vector in inputs){w.WriteStartArray();foreach(short v in vector)w.WriteNumberValue(v);w.WriteEndArray();}w.WriteEndArray();
  w.WriteStartArray("probabilities");foreach(var family in score.Glyph.Scores.FamilyProbabilities.Take(4)){w.WriteStartArray();foreach(int v in family!.Value.Span)w.WriteNumberValue(v);w.WriteEndArray();}w.WriteEndArray();
  w.WriteStartArray("weights");foreach(float v in score.Glyph.ResolvedWeights.Span)w.WriteNumberValue(v);w.WriteEndArray();w.WriteStartArray("integerCosts");foreach(int v in score.Glyph.IntegerCosts.Span)w.WriteNumberValue(v);w.WriteEndArray();w.WriteStartArray("cacheCosts");foreach(int v in score.Cache.Costs.Span)w.WriteNumberValue(v);w.WriteEndArray();w.WriteStartArray("candidates");foreach(var v in score.Cache.Candidates.Span){w.WriteStartArray();w.WriteNumberValue(v.Symbol);w.WriteNumberValue(v.Cost);w.WriteEndArray();}w.WriteEndArray();
  w.WriteBoolean("whole",score.PrimitiveScope.BoundaryAlignedSingleNormalizedStroke);w.WriteBoolean("zero",score.PrimitiveScope.EndpointFieldsAreZero);w.WriteStartArray("boundaries");w.WriteNumberValue(bounds.Start);w.WriteNumberValue(bounds.End);w.WriteEndArray();w.WriteEndObject();
 }
 w.WriteEndArray();w.WriteEndObject();w.Flush();
}

Console.WriteLine($"Main actual4EXP/cached{scores.Length} groups; nonneutralcontexts={scores.Count(s=>s.Context.Span.IndexOfAnyExcept((short)-1024)>=0)}.");
LegacyMainSelectedFirstWord Build(LegacyMainFirstSubmission submission)
{
    var factory=submission.Factory;var row=factory.Frame.Row;var cuts=row.GenerateRussianCuts();
    var profile=LegacyFirstTransitionGeometryProfile.ReadSource(ak,6120);var policy=LegacyFreshFirstControllerPolicy.ReadFirstSource(ak,6120);
    var scope=new LegacyFirstRowPrimitiveScope(row,cuts,row.OriginalSource.Strokes.Select(s=>s.Points.Count).ToArray());
    var scorer=LegacyFirstOrdinaryCacheScorer.Prepare(ak,6120,row,activations);byte[] preparedMask=Enumerable.Repeat((byte)1,scorer.Alphabet.LabelCount).ToArray();
    int graphOffset=profile.SamplerOffset+8+BinaryPrimitives.ReadInt32LittleEndian(ak.AsSpan(profile.SamplerOffset+4));
    var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,
        LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(ak,graphOffset),new byte[row.OriginalSource.Strokes.Count],Enumerable.Repeat(-1,row.OriginalSource.Strokes.Count).ToArray());
    var prepared=LegacyFreshFirstCandidateGraphPreparer.Prepare(ak,factory,cuts,seeds,table);var graph=prepared.SourceRanges.Graph;
    var scores=new LegacyFirstOrdinaryCacheScores[graph.Groups.Count];
    for(int g=0;g<scores.Length;g++)
    {
        var ranges=graph.Groups[g].Ranges.ToArray();var order=cuts.Chronology.OrderRangeIndicesForNormalization(ranges);
        var clip=LegacyCursiveGlyphBoundaryPolicy.Resolve(order.Select(i=>ranges[i]).ToArray(),cuts.Cuts.PreparedInk.StrokePointCounts.Span,0,false,false);
        scores[g]=scorer.Score(cuts,ranges,clip.Start,clip.End,15,Math.Min(256,policy.MaximumNormalizedStrokes),preparedMask,
            glyph=>policy.CreateContext(scope.Evaluate(ranges,glyph.MergedGeometry.Strokes.Count),[]),[]);
    }
    var plan=LegacyFreshFirstProtocolOwnership.Create(factory).PreparePrimary(ak,cuts,prepared,scores,table);
    var primary=plan.Run();var context=plan.RunSupportedNonSkippedContext();
    var word=new LegacyFirstPlainWordMaterializer(ak,6120,factory,table).Materialize(cuts,graph,primary,context,scores);
    return new(submission.DescriptorOrdinal,word,prepared.SourceRanges);
}
static void Require(bool v,string m){if(!v)throw new InvalidDataException(m);}
static void Throws<T>(Action a)where T:Exception{try{a();}catch(T){return;}throw new InvalidDataException($"Expected {typeof(T).Name}.");}
