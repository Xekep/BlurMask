using System.Text.Json;
using InkRecognition;

if (args.Length is not (1 or 2)) throw new ArgumentException("controls.json [RU acoustic.res]");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases = 0, rows = 0;
foreach (var c in controls.RootElement.EnumerateArray())
{
    float[] p = c.GetProperty("p").EnumerateArray().Select(x => x.GetSingle()).ToArray();
    var profile = new LegacyRawLineCandidateProfile(c.GetProperty("budget").GetUInt32(), p[0],
        c.GetProperty("horizontal").GetBoolean(), c.GetProperty("vertical").GetBoolean(), c.GetProperty("directional").GetBoolean(),
        p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8], p[9], p[10], p[11], p[12], p[13]);
    var strokes = c.GetProperty("strokes").EnumerateArray().Select(s => new InkStroke(s.EnumerateArray()
        .Select(point => new InkPoint(point[0].GetSingle(), point[1].GetSingle())).ToArray())).ToArray();
    int[] labels = c.GetProperty("labels").EnumerateArray().Select(x => x.GetInt32()).ToArray();
    bool[] flags = c.GetProperty("flags").EnumerateArray().Select(x => x.GetBoolean()).ToArray();
    var result = LegacyRawLineCandidateProducer.Produce(strokes, profile, labels, flags,
        c.GetProperty("primary").GetDouble(), c.GetProperty("scale").GetDouble(), c.GetProperty("external").GetBoolean());
    Require(result.SelectedHorizontal == c.GetProperty("selectedX").GetBoolean(), $"Projection selection differs in case{cases}.");
    Verify(result.HorizontalCandidates, c.GetProperty("x")); Verify(result.VerticalCandidates, c.GetProperty("y"));
    var groups = result.GetSelectedGroups();
    Require(groups[0].FirstStroke == 0 && groups[^1].LastStroke == strokes.Length - 1 &&
        groups.Zip(groups.Skip(1)).All(pair => pair.First.LastStroke + 1 == pair.Second.FirstStroke), "Selected ranges omit or overlap source strokes.");
    cases++;
}
Console.WriteLine($"PASS {cases} independent raw-source candidate cases / {rows} full candidate records; exhaustive suffix partitions, weighted raw costs, primitive/overlap penalties, chronological/union gaps, both projections, strict ties, labeled best-state gates and externally supplied scale.");
InkStroke Stroke(params (float X, float Y)[] points) => new(points.Select(p => new InkPoint(p.X, p.Y)).ToArray());
var zero = new LegacyRawLineCandidateProfile(200, 10, true, true, true, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 5);
var singleton = new[] { Stroke((0, 0)) };
var tie = LegacyRawLineCandidateProducer.Produce(singleton, zero, [-1], [false], 1, 1, true);
Require(!tie.SelectedHorizontal && tie.GetSelectedGroups().Count == 1, "Equal projection cost must select vertical.");
var equal = LegacyRawLineCandidateProducer.Produce([Stroke((0,0)), Stroke((0,0))], zero, [-1,-1], [false,false], 1, 1, true);
Require(equal.HorizontalCandidates[0].NextStroke == 1, "Equal suffix cost replaced earlier singleton candidate.");
Reject(() => LegacyRawLineCandidateProducer.Produce(singleton, zero, [], [false], 1, 1, false));
Reject(() => LegacyRawLineCandidateProducer.Produce(singleton, zero, [-2], [false], 1, 1, false));
Reject(() => LegacyRawLineCandidateProducer.Produce(singleton, zero with { HorizontalEnabled=false, VerticalEnabled=false }, [-1], [false], 1, 1, false));
Reject(() => LegacyRawLineCandidateProducer.Produce(singleton, zero, [-1], [false], 1, 0, false));
Reject(() => LegacyRawLineCandidateProducer.Produce([Stroke((float.MaxValue,0),(float.MaxValue,1))], zero, [-1], [false], 1, 1, false));
Reject(() => LegacyRawLineCandidateProducer.Produce(singleton, zero, [-1], [false], 1, 1, false, new CancellationToken(true)));
Reject(() => new LegacyRawLineCandidates([tie.VerticalCandidates[0] with {NextStroke=0}],[],true).GetSelectedGroups());
Reject(() => new LegacyRawLineCandidates([],[],true).GetSelectedGroups());
if (args.Length == 1) return;
byte[] acoustic = File.ReadAllBytes(args[1]);
var outer = LegacyRawLineCandidateProfile.ReadV100Node(acoustic, 5325);
var inner = LegacyRawLineCandidateProfile.ReadV100Node(acoustic, 5654);
Require(outer == inner && inner == new LegacyRawLineCandidateProfile(200,10,true,false,true,1.3f,.2f,3,8,60,.01f,5,20,.5f,.5f,1,4,5),
    "Actual RU2a9 fields differ.");
var drawing = new[] { Stroke((0,90),(0,10),(25,60),(50,10),(50,90)), Stroke((70,10),(70,90),(120,10),(120,90)), Stroke((140,90),(140,10),(180,10),(190,20),(190,40),(180,50),(140,50)) };
foreach (bool external in new[] {false,true})
{
    var actual = LegacyRawLineCandidateProducer.Produce(drawing, inner, [-1,-1,-1], [false,false,false], 55, 55, external);
    var groups = actual.GetSelectedGroups();
    Require(actual.SelectedHorizontal && groups.Sum(g => g.LastStroke-g.FirstStroke+1) == drawing.Length,
        "Actual profile lost raw source geometry.");
    Console.WriteLine($"ACTUAL RU2a9 ordinary raw grouping: externalScale={external},groups={groups.Count},cost={actual.SelectedCandidates[0].SuffixCost:R}; no automatic child249/native winner claim.");
}
byte[] corrupt = acoustic.ToArray(); corrupt[5654+25]=2;
Reject(() => LegacyRawLineCandidateProfile.ReadV100Node(corrupt,5654));
Reject(() => LegacyRawLineCandidateProfile.ReadV100Node(acoustic[..5680],5654));
Console.WriteLine("PASS real RU2a9 resource fields, ordinary grouping composition, malformed profile/metadata/overflow and cancellation guards.");

void Verify(IReadOnlyList<LegacyRawLineCandidate> actual, JsonElement expected)
{
    Require(actual.Count == expected.GetArrayLength(), "Candidate table length differs.");
    for (int i=0;i<actual.Count;i++)
    {
        var a=actual[i];var e=expected[i];var g=a.Moments;
        Require(g.FirstStroke==i && g.LastStroke==e.GetProperty("end").GetInt32() && g.ClusterLabel==e.GetProperty("cluster").GetInt32() &&
            g.LineLabel==int.MinValue && a.NextStroke==g.LastStroke+1 && g.SpecialSingle==a.Horizontal, $"Candidate metadata differs case{cases}/row{i}.");
        Close(a.OwnCost,e.GetProperty("own").GetDouble());Close(a.SuffixCost,e.GetProperty("suffix").GetDouble());
        double[] values=[g.Weight,g.SumX,g.SumY,g.SumXX,g.SumYY,g.SumXY,g.MinX,g.MaxX,g.MinY,g.MaxY];
        int k=0;foreach(var v in e.GetProperty("moments").EnumerateArray())Close(values[k++],v.GetDouble());
        rows++;
    }
}
void Close(double actual,double expected)
{
    if(!double.IsFinite(actual)||Math.Abs(actual-expected)>1e-8*Math.Max(1,Math.Abs(expected)))
        throw new InvalidDataException($"Case{cases} numerical value differs: {actual:R} vs {expected:R}.");
}
static void Require(bool ok,string message){if(!ok)throw new InvalidDataException(message);}
static void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidDataException or OperationCanceledException){return;}throw new Exception("Malformed raw line input accepted.");}
