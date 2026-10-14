using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
if(args.Length!=9)throw new ArgumentException("AK, engineprofile,separationprofile,line30,line50,auditoutput,case-data,publication-data,controls.");
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
MixtureControls.Run(args[8],LegacyOrdinaryGapProfile.ReadV100Node(ak,188050),LegacyOrdinaryGapMixtureProfile.ReadV100Node(ak,188050));
Require(active.Mixture is not null&&result.PublishedRows.Count==14&&active.QualifyingGaps.Count==13&&active.Blend>0,"Actual first-NN long row must activate native29B mixture.");
Throws<OperationCanceledException>(()=>LegacyFreshMainActiveGapPreparation.Prepare(ak,shear,new(true)));

var smoothed=active.Smooth(ak);
Require(ReferenceEquals(smoothed.Input,active)&&smoothed.Strokes.Count==raw.Count&&smoothed.SourceGap==active.Output.SourceGap,"Actual active29B owner retained by265.");
Throws<OperationCanceledException>(()=>active.Smooth(ak,new(true)));
using(var stream=File.Create(args[5]))using(var w=new Utf8JsonWriter(stream))
{
 w.WriteStartObject();w.WriteNumber("gap",active.Output.SourceGap);w.WriteNumber("blend",active.Blend);w.WriteNumber("mean",active.Output.Mean);w.WriteNumber("variance",active.Output.Variance);
 w.WriteStartArray("qualifying");foreach(float v in active.QualifyingGaps)w.WriteNumberValue(v);w.WriteEndArray();
 w.WriteStartArray("observations");foreach(float v in active.Output.NormalizedGaps)w.WriteNumberValue(v);w.WriteEndArray();
 w.WriteStartArray("descriptors");foreach(var d in result.CopyDescriptors())w.WriteStringValue(Convert.ToBase64String(d));w.WriteEndArray();
 w.WriteStartArray("guides");foreach(var g in result.CopyGuides())w.WriteStringValue(Convert.ToBase64String(g));w.WriteEndArray();
 w.WriteStartArray("boxes");foreach(var row in shear.Output.Rows){w.WriteStartArray();var b=row.CopyStorage().Span;for(int k=24;k<40;k+=4)w.WriteNumberValue(BinaryPrimitives.ReadSingleLittleEndian(b[k..]));w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("attempts");foreach(var attempt in active.Mixture!.Attempts){w.WriteStartObject();w.WriteNumber("index",attempt.Index);w.WriteNumber("iterations",attempt.Iterations);w.WriteNumber("firstMean",attempt.FirstMean);w.WriteNumber("secondMean",attempt.SecondMean);w.WriteNumber("firstVariance",attempt.FirstVariance);w.WriteNumber("secondVariance",attempt.SecondVariance);w.WriteNumber("firstPrior",attempt.FirstPrior);w.WriteNumber("secondPrior",attempt.SecondPrior);w.WriteNumber("intersection",attempt.Intersection);w.WriteEndObject();}w.WriteEndArray();
 w.WriteNumber("intersection",active.Mixture.Intersection);
 w.WriteStartArray("points");foreach(var st in smoothed.Strokes){w.WriteStartArray();foreach(var point in st.Points){w.WriteStartArray();w.WriteNumberValue(point.X);w.WriteNumberValue(point.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("beforePoints");foreach(var st in shear.Output.CopyPhysicalPoints()){w.WriteStartArray();foreach(var point in st){w.WriteStartArray();w.WriteNumberValue(point.X);w.WriteNumberValue(point.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("parentPoints");foreach(var st in invocation.Controller.Source.Strokes){w.WriteStartArray();foreach(var point in st.Points){w.WriteStartArray();w.WriteNumberValue(point.X);w.WriteNumberValue(point.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
 w.WriteStartArray("metrics");w.WriteNumberValue(smoothed.OriginalSourceMetrics.OrthogonalFraction);w.WriteNumberValue(smoothed.OriginalSourceMetrics.CornerFraction);w.WriteEndArray();
 w.WriteStartArray("inputKeys200");foreach(uint key in smoothed.InputPointKeys200.Span)w.WriteNumberValue(key);w.WriteEndArray();
 w.WriteStartArray("mappedKeys200");foreach(uint key in smoothed.Chronology.PreparedPointMap.Span)w.WriteNumberValue(key);w.WriteEndArray();
 w.WriteStartArray("beforeMap");foreach(uint key in result.Chronology.PreparedPointMap.Span)w.WriteNumberValue(key);w.WriteEndArray();
 w.WriteEndObject();w.Flush();
}
Console.WriteLine($"ACTUAL {raw.Count}raw/{invocation.FirstSubmissions.Count}descriptors/{result.PublishedRows.Count}NN720 blend={active.Blend}, gap={active.Output.SourceGap}, activeMixture={active.Mixture is not null}.");
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
