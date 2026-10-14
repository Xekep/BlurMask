using InkRecognition;
using System.Text.Json;
if(args.Length!=4)throw new ArgumentException("Supply controls, actualAK, embedded30, embedded50.");
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[0]));
byte[] resource=File.ReadAllBytes(args[1]),spatial=File.ReadAllBytes(args[2]),physical=File.ReadAllBytes(args[3]);
int count=0,pairCount=0,ownedKeys=0,multipleStrips=0;
foreach(var item in doc.RootElement.GetProperty("cases").EnumerateArray())
{
    var raw=item.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
    var result=LegacyFreshMainPhysicalGrouping.Prepare(resource,spatial,physical,raw,96);
    Require(ReferenceEquals(result.Scale.Source,result.Packet.Source)&&ReferenceEquals(result.Grouping.Source,result.Scale),"One retained source owner.");
    Require(result.ProviderType==0x2e3&&result.CoordinateMode==2&&result.QuarterVerticalWhenHalfHorizontal&&result.InternalScaleQuantile==.8f&&result.WindowWidthMultiplier==5&&result.WindowOverlap==.75f&&result.HorizontalOrderTolerance==.25f,"Actual resource/constructor policy.");
    Require(result.Packet.DescriptorCount==1&&result.Packet.Descriptor.FirstPhysicalStroke==0&&result.Packet.Descriptor.LastPhysicalStroke==raw.Length-1&&result.Packet.Descriptor.CurrentLineId==-1,"Initial coalesced descriptor ownership.");
    Require(result.Scale.PrimaryScale==item.GetProperty("primary").GetSingle(),"Independent primary source scale.");
    var clean=item.GetProperty("clean").EnumerateArray().ToArray();
    for(int s=0;s<raw.Length;s++)
    {
        var points=result.Packet.Source.Strokes[s].Points;var expected=clean[s].EnumerateArray().ToArray();
        Require(points.Count==expected.Length,"Retained original point count.");
        for(int p=0;p<points.Count;p++)
        {
            Require(points[p].X==expected[p][0].GetSingle()&&points[p].Y==expected[p][1].GetSingle(),"No substituted/resampled source point.");
            var key=result.Packet.Source.Chronology.Resolve(s,p).Key;
            Require(key.First==0&&key.Second==s&&key.Third==result.Packet.Source.OriginalPointIndices[s].Span[p],"Original physical/key chronology.");ownedKeys++;
        }
    }
    var actual=result.Grouping;var expectedGrouping=item.GetProperty("expected");
    Require(actual.NeuralScale==expectedGrouping.GetProperty("neuralScale").GetSingle(),$"Independent neural scale{count}.");
    Require(actual.VerticalStripCount==expectedGrouping.GetProperty("strips").GetInt32(),$"Independent strip count{count}.");if(actual.VerticalStripCount>1)multipleStrips++;
    Require(actual.TinyPhysicalStrokeIndices.Span.SequenceEqual(expectedGrouping.GetProperty("tiny").EnumerateArray().Select(x=>x.GetInt32()).ToArray()),"Actual tiny-record indices.");
    Require(actual.Selected.StrokeGroupIndices.Span.SequenceEqual(expectedGrouping.GetProperty("membership").EnumerateArray().Select(x=>x.GetInt32()).ToArray()),$"Independent membership{count}.");
    var groups=expectedGrouping.GetProperty("groups").EnumerateArray().ToArray();Require(actual.Selected.Groups.Count==groups.Length,"Actual group count.");
    for(int g=0;g<groups.Length;g++)Require(actual.Selected.Groups[g].ToArray().Order().SequenceEqual(groups[g].EnumerateArray().Select(x=>x.GetInt32())),"Independent grouped physical IDs.");
    var pairs=expectedGrouping.GetProperty("pairs").EnumerateArray().ToArray();Require(actual.NeuralPairs.Count==pairs.Length,"Actual neural pair count.");
    for(int p=0;p<pairs.Length;p++)
    {
        var a=actual.NeuralPairs[p];var e=pairs[p];Require(a.FirstStroke==e[0].GetInt32()&&a.SecondStroke==e[1].GetInt32()&&a.LabelOne==e[3].GetBoolean()&&MathF.Abs(a.Confidence-e[2].GetSingle())<2e-5f,"Independent actual affine/BLSTM pair.");pairCount++;
    }
    //Old Infer still receives an explicit geometric profile. The grouping API
    //does not select that profile or attach those test-only frame estimates.
    using var stream30=new MemoryStream(spatial,false);using var stream50=new MemoryStream(physical,false);
    var old=LegacyOrdinaryNeuralLineInferenceFactory.Infer(result.Scale,96,new bool[raw.Length],true,.8f,
        PreparedLineBoundaryClassifier.Load(stream30),PreparedLineBoundaryClassifier.Load(stream50),LegacyAutomaticLineProfile.ReadResourceNode(resource,187976),5,.75f);
    Require(old.NeuralScale==actual.NeuralScale&&old.VerticalStripCount==actual.VerticalStripCount&&old.Frames.StrokeLineIndices.Span.SequenceEqual(actual.Selected.StrokeGroupIndices.Span)&&old.Frames.PairEvidence.SequenceEqual(actual.NeuralPairs),"Old Infer numerical/grouping regression.");count++;
}
InkStroke stroke=new(new InkPoint[]{new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)});
var zero=LegacyFreshMainPhysicalGrouping.Prepare(resource,spatial,physical,[stroke],0);
Require(zero.PhysicalResolution==0&&zero.Grouping.Selected.Groups.Count==1&&zero.Grouping.NeuralPairs.Count==0,"Actual zero runtime resolution/singleton branch.");
Throws<ArgumentException>(()=>LegacyFreshMainPhysicalGrouping.Prepare(resource,spatial,physical,[stroke],float.NaN));
Throws<ArgumentException>(()=>LegacyFreshMainPhysicalGrouping.Prepare(resource,spatial,physical,[],96));
Throws<ArgumentException>(()=>LegacyFreshMainPhysicalGrouping.Prepare(resource,spatial,physical,Enumerable.Repeat(stroke,257).ToArray(),96));
Throws<OperationCanceledException>(()=>LegacyFreshMainPhysicalGrouping.Prepare(resource,spatial,physical,[stroke],96,new(true)));
foreach(int at in new[]{5237,5301,5302,5305,5350}){byte[] corrupt=resource.ToArray();corrupt[at]^=1;Throws<InvalidDataException>(()=>LegacyFreshMainPhysicalGrouping.Prepare(corrupt,spatial,physical,[stroke],96));}
byte[] bad30=spatial.ToArray();bad30[^1]^=1;Throws<InvalidDataException>(()=>LegacyFreshMainPhysicalGrouping.Prepare(resource,bad30,physical,[stroke],96));
byte[] bad50=physical.ToArray();bad50[^1]^=1;Throws<InvalidDataException>(()=>LegacyFreshMainPhysicalGrouping.Prepare(resource,spatial,bad50,[stroke],96));
Require(multipleStrips>0&&pairCount>0,"Independent controls must execute actual NN and deferred vertical strips.");
Console.WriteLine($"PASS {count} actual-model independent grouping controls/{pairCount} NN pairs/{ownedKeys} original keys/{multipleStrips} multi-strip cases; old Infer preserved, no24A default or full15F050 claim.");
static void Require(bool v,string m){if(!v)throw new InvalidDataException(m);}
static void Throws<T>(Action a)where T:Exception{try{a();}catch(T){return;}throw new InvalidDataException($"Expected {typeof(T).Name}.");}
