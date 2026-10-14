using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
if(args.Length!=6)throw new ArgumentException("AK, engineprofile,separationprofile,line30,line50,auditoutput.");
byte[] ak=File.ReadAllBytes(args[0]),line30=File.ReadAllBytes(args[3]),line50=File.ReadAllBytes(args[4]);
using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));
var raw=new List<InkStroke>();
foreach(int row in new[]{1,0})
{
    raw.Add(Draw(row,(0,90),(0,90),(0,10),(25,60),(50,10),(50,90)));
    raw.Add(Draw(row,(70,10),(70,90),(120,10),(120,90)));
    raw.Add(Draw(row,(140,90),(140,10),(180,10),(190,20),(190,40),(180,50),(140,50)));
}
var grouping=LegacyFreshMainPhysicalGrouping.Prepare(ak,line30,line50,raw,0);
var reordered=LegacyFreshMainPhysicalReordering.Prepare(LegacyFreshMainDescriptorSplit.Prepare(grouping),line30,line50);
var invocation=LegacyFreshMainFirstResultLines.Prepare(ak,reordered);
Require(invocation.FirstSubmissions.Count==2,"Actual two-row owned memberships were not split.");
var words=invocation.FirstSubmissions.Select(Build).ToArray();var result=invocation.ApplyFirstResults(words);
Require(result.Outputs.Count==2&&result.CopyGuides().Length==2&&result.PublishedRows.Count>0,"Actual first NN/source720 failed two main guides.");
int ownedRows=0;var descriptors=result.CopyDescriptors();
for(int i=0;i<invocation.FirstSubmissions.Count;i++)
{
    var submission=invocation.FirstSubmissions[i];var d=descriptors[submission.DescriptorOrdinal];
    int line=BinaryPrimitives.ReadInt32LittleEndian(d.AsSpan(16)),first=BinaryPrimitives.ReadInt32LittleEndian(d.AsSpan(52)),end=BinaryPrimitives.ReadInt32LittleEndian(d.AsSpan(56));
    Require(line==i&&first==ownedRows&&end>first,"Actual parent10/34/38 and guide ownership.");ownedRows=end;
    var output=result.Outputs[i];Require(output.NewLineFit is not null&&output.Deskew is not null,"Native24B fit/deskew not executed.");
    var guide=result.CopyGuides()[line];Require(BinaryPrimitives.ReadSingleLittleEndian(guide.AsSpan(4))>0,"Actual positive main height required.");
}
Throws<ArgumentException>(()=>invocation.ApplyFirstResults([words[0],words[0]]));
var foreign=LegacyFreshMainFirstResultLines.Prepare(ak,reordered);Throws<ArgumentException>(()=>foreign.ApplyFirstResults(words));
Throws<OperationCanceledException>(()=>invocation.ApplyFirstResults(words,new(true)));
using var stream=File.Create(args[5]);using var writer=new Utf8JsonWriter(stream);
writer.WriteStartObject();
writer.WriteStartArray("published");foreach(var r in result.PublishedRows)writer.WriteStringValue(Convert.ToBase64String(r.CopyStorage().Span));writer.WriteEndArray();
writer.WriteStartArray("parentPoints");foreach(var st in invocation.Controller.Source.Strokes){writer.WriteStartArray();foreach(var p in st.Points){writer.WriteStartArray();writer.WriteNumberValue(p.X);writer.WriteNumberValue(p.Y);writer.WriteEndArray();}writer.WriteEndArray();}writer.WriteEndArray();
writer.WriteStartArray("currentPoints");foreach(var st in result.CopyPhysicalPoints()){writer.WriteStartArray();foreach(var p in st){writer.WriteStartArray();writer.WriteNumberValue(p.X);writer.WriteNumberValue(p.Y);writer.WriteEndArray();}writer.WriteEndArray();}writer.WriteEndArray();
writer.WriteStartArray("markers");foreach(var v in new[]{invocation.Controller.Markers.First,invocation.Controller.Markers.Second,invocation.Controller.Markers.Hyphen,invocation.Controller.Markers.Underscore,invocation.Controller.Markers.ProlongedSound,invocation.Controller.Markers.IdeographOne,invocation.Controller.Markers.Replacement})writer.WriteNumberValue(v);writer.WriteEndArray();
var fp=invocation.Controller.Profile.PrepareFitProfile(0,0,[]);writer.WriteStartArray("fitProfile");foreach(float v in new[]{fp.MinimumItemCount,fp.MaximumItemCount,fp.MinimumSlope,fp.MaximumSlope,fp.MinimumHeight,fp.MaximumBoxWidth,fp.KnownGlyphWeightCap})writer.WriteNumberValue(v);writer.WriteEndArray();
writer.WriteStartArray("guides");foreach(var g in result.CopyGuides())writer.WriteStringValue(Convert.ToBase64String(g));writer.WriteEndArray();
writer.WriteStartArray("source720");foreach(var r in result.CurrentRows)writer.WriteStringValue(Convert.ToBase64String(r.CopyStorage().Span));writer.WriteEndArray();
writer.WriteStartArray("descriptors");foreach(var d in result.CopyDescriptors())writer.WriteStringValue(Convert.ToBase64String(d));writer.WriteEndArray();
writer.WriteEndObject();writer.Flush();
Console.WriteLine($"PASS actual reversed raw6physical -> ownedmainNN/groups/reorders -> {words.Length} actualfirstNN results/{ownedRows} source720 -> {result.CopyGuides().Length} main24B guides; no suppliedbaseline/height/boxes, no fullmainwinner or24D/29Bclaim.");
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
static InkStroke Draw(int row,params(float X,float Y)[] points)=>new(points.Select(p=>new InkPoint(p.X,p.Y+row*250)).ToArray());
static void Require(bool v,string m){if(!v)throw new InvalidDataException(m);}
static void Throws<T>(Action a)where T:Exception{try{a();}catch(T){return;}throw new InvalidDataException($"Expected {typeof(T).Name}.");}
