using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
if(args.Length!=9)throw new ArgumentException("AK, engineprofile,separationprofile,line30,line50,auditoutput,case-data,publication-data,LK.");
byte[] ak=File.ReadAllBytes(args[0]),line30=File.ReadAllBytes(args[3]),line50=File.ReadAllBytes(args[4]);
using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));
var raw=new List<InkStroke>();
for(int i=0;i<31;i++)raw.Add(new InkStroke(new InkPoint[]{new(i*70,90),new(i*70,10),new(i*70+25,60),new(i*70+50,10),new(i*70+50,90)}));
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
var spanOwner=new LegacyOwnedMainWordSpanPreparation(ak,File.ReadAllBytes(args[8]),File.ReadAllBytes(args[1]),sampled,mainContext,table);
var policy=LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,spanOwner);
var ring=LegacyOwnedFreshMainWordRingPreparation.Prepare(policy);
Require(ReferenceEquals(ring.Owner,policy)&&ring.SourceNodeCount==sampled.SourceRanges.Graph.NodeCount,
    "Ring input count/policy must belong to the actual graph owner.");
Require(ring.Parameters==new LegacyNativeWordRingParameters(-1,120,20,120),"Owned native field producers changed.");
//Independent literal signed min/max equation. The retained50 sentinel is NOT
//replaced with a positive value merely to obtain valid storage.
int longest=Math.Max(-1,Math.Max(20,120));int combined=Math.Max(120,unchecked(-1+20));
int distance=Math.Min(unchecked(longest+120),combined);
int capacity=((ring.SourceNodeCount+39)/40)*40;int rows=Math.Min(capacity,distance);
Require(distance==120&&ring.CapacityPlan==new LegacyNativeWordRingCapacityPlan(capacity,rows,distance)&&
    ring.SourceNodeCount>distance,"Actual raw source must reach a real repeated ring-row identity.");
var lastOwner=Enumerable.Repeat(-1,rows).ToArray();int aliases=0;
for(int node=0;node<ring.SourceNodeCount;node++)
{
    int expected=node%rows;int actual=ring.ResolvePhysicalRow(node);
    Require(actual==expected,"Native row alias index differs.");
    if(node>=rows){Require(lastOwner[actual]==node-rows,"Row identity did not refer to its previous retained owner.");aliases++;}
    else Require(lastOwner[actual]==-1,"New native row alias was already occupied.");
    lastOwner[actual]=node;
}
Require(aliases>0,"No actual ring-row reuse tested.");
Throws<ArgumentOutOfRangeException>(()=>ring.ResolvePhysicalRow(-1));
Throws<ArgumentOutOfRangeException>(()=>ring.ResolvePhysicalRow(ring.SourceNodeCount));
Throws<ArgumentNullException>(()=>LegacyOwnedFreshMainWordRingPreparation.Prepare(null!));
Throws<OperationCanceledException>(()=>LegacyOwnedFreshMainWordRingPreparation.Prepare(policy,new(true)));
using(var stream=File.Create(args[5]))using(var w=new Utf8JsonWriter(stream))
{
    w.WriteStartObject();w.WriteNumber("physicals",smoothed.Strokes.Count);w.WriteNumber("nodes",ring.SourceNodeCount);
    w.WriteNumber("groups",sampled.SourceRanges.Graph.Groups.Count);w.WriteNumber("capacity",capacity);w.WriteNumber("rows",rows);
    w.WriteNumber("distance",distance);w.WriteNumber("aliases",aliases);w.WriteStartArray("fields");w.WriteNumberValue(ring.Parameters.Field50);
    w.WriteNumberValue(ring.Parameters.Field54);w.WriteNumberValue(ring.Parameters.Field58);w.WriteNumberValue(ring.Parameters.Field68);w.WriteEndArray();
    w.WriteEndObject();w.Flush();
}
Console.WriteLine($"PASS actual {smoothed.Strokes.Count}physical/main {ring.SourceNodeCount}nodes/{sampled.SourceRanges.Graph.Groups.Count}groups, fields(-1,120,20,120) to capacity{capacity}/rows{rows}/retention{distance}/{aliases} actual row aliases; 4 ownership/domain guards. Native heap allocation remains separate.");

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
    var primary=plan.Run();var context=LegacyFirstBlankContextPasses.Run(ak,factory,cuts,prepared,plan,table);
    var word=new LegacyFirstPlainWordMaterializer(ak,6120,factory,table).Materialize(cuts,graph,primary,context,scores);
    return new(submission.DescriptorOrdinal,word,prepared.SourceRanges);
}
static void Require(bool v,string m){if(!v)throw new InvalidDataException(m);}
static void Throws<T>(Action a)where T:Exception{try{a();}catch(T){return;}throw new InvalidDataException($"Expected {typeof(T).Name}.");}
