using InkRecognition;
using System.Text.Json;

if (args.Length != 1) throw new ArgumentException("Supply independent prepared group controls.");
if (new FileInfo(args[0]).Length > 8 * 1024 * 1024) throw new ArgumentException("Controls exceed 8 MiB.");
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0])); int count = 0, momentsCount=0;
foreach (var item in document.RootElement.EnumerateArray())
{
    var groups = item.GetProperty("groups").EnumerateArray().Select(ReadGroup).ToArray();
    var strokes=item.GetProperty("strokes").EnumerateArray().ToArray();
    for(int i=0;i<strokes.Length;i++)
    {
        var points=strokes[i].EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray();
        var actual=LegacyLineArcMoments.ExtractStroke(points,i);var expected=ReadGroup(item.GetProperty("moments")[i]);
        double[] a=[actual.Weight,actual.SumX,actual.SumY,actual.SumXX,actual.SumYY,actual.SumXY,actual.MinX,actual.MaxX,actual.MinY,actual.MaxY];
        double[] e=[expected.Weight,expected.SumX,expected.SumY,expected.SumXX,expected.SumYY,expected.SumXY,expected.MinX,expected.MaxX,expected.MinY,expected.MaxY];
        for(int j=0;j<a.Length;j++)Equal(a[j],e[j],"Arc moment");momentsCount++;
    }
    var p = item.GetProperty("profile").EnumerateArray().Select(x => x.GetSingle()).ToArray();
    var profile = new LegacyLineGroupCostProfile(p[0],p[1],p[2],p[3],p[4],p[5],p[6],p[7],p[8],p[9]);
    var axis = item.GetProperty("axis").GetString() == "X" ? LegacyLineGroupProjection.X : LegacyLineGroupProjection.Y;
    double scale = item.GetProperty("scale").GetDouble();
    Equal(LegacyPreparedLineGroupCost.Score(groups, axis, scale, profile), item.GetProperty("expected").GetDouble(), "Group cost");
    var found = LegacyFreeLineGroupSearch.Search(groups, axis, scale, profile);
    Equal(found.Cost, item.GetProperty("searchCost").GetDouble(), "Exhaustive partition cost");
    Require(found.Ranges[0].FirstGroup == 0 && found.Ranges[^1].LastGroup == groups.Length-1, "Path endpoints.");
    for (int i=1;i<found.Ranges.Length;i++) Require(found.Ranges[i-1].LastGroup+1==found.Ranges[i].FirstGroup,"Path continuity.");
    count++;
}
var own = new LegacyPreparedLineGroup(0,0,1,0,0,0,0,0,0,0,0,0);
var zero = new LegacyLineGroupCostProfile(0,0,0,0,0,0,0,0,0,0);
var tie = LegacyFreeLineGroupSearch.Search([own, own with { FirstStroke=1,LastStroke=1 }], LegacyLineGroupProjection.X,1,zero);
Require(tie.Ranges.Length==2,"Native strict-cost tie keeps shortest last group.");
var label = LegacyFreeLineGroupSearch.Search([own with {LineLabel=7},own with {FirstStroke=1,LastStroke=1,LineLabel=7}], LegacyLineGroupProjection.X,1,zero);
Require(label.Ranges.Length==1 && label.Ranges[0].LineLabel==7,"Known equal row prevents boundary.");
var cluster = LegacyFreeLineGroupSearch.Search([own with {ClusterLabel=2},own with {FirstStroke=1,LastStroke=1,ClusterLabel=2}], LegacyLineGroupProjection.X,1,zero);
Require(cluster.Ranges.Length==1 && cluster.Ranges[0].ClusterLabel==2,"Known equal cluster prevents boundary.");
var split = LegacyFreeLineGroupSearch.Search([own with {LineLabel=7},own with {FirstStroke=1,LastStroke=1,LineLabel=8}], LegacyLineGroupProjection.X,1,zero);
Require(split.Ranges.Length==2,"Known unequal labels stop candidate growth.");
Throws<ArgumentException>(()=>LegacyPreparedLineGroupCost.Score([],LegacyLineGroupProjection.X,1,zero));
Throws<ArgumentException>(()=>LegacyPreparedLineGroupCost.Score([own with {Weight=0}],LegacyLineGroupProjection.X,1,zero));
Throws<ArgumentException>(()=>LegacyPreparedLineGroupCost.Score([own],LegacyLineGroupProjection.X,0,zero));
Throws<ArgumentException>(()=>LegacyPreparedLineGroupCost.Score([own],LegacyLineGroupProjection.X,1,zero with {BaseCost=float.NaN}));
Throws<InvalidDataException>(()=>LegacyPreparedLineGroupCost.Score([own with {SumX=double.MaxValue}],LegacyLineGroupProjection.X,1,zero with {VarianceWeight=1}));
Throws<OperationCanceledException>(()=>LegacyFreeLineGroupSearch.Search([own],LegacyLineGroupProjection.X,1,zero,new(true)));
Throws<ArgumentException>(()=>LegacyLineArcMoments.ExtractStroke([],0));
Throws<ArgumentException>(()=>LegacyLineArcMoments.ExtractStroke([new(float.NaN,0)],0));
Throws<InvalidDataException>(()=>LegacyLineArcMoments.ExtractStroke([new(float.MaxValue,0),new(float.MaxValue,1)],0));
Throws<OperationCanceledException>(()=>LegacyLineArcMoments.ExtractStroke([new(0,0)],0,cancellationToken:new(true)));
Console.WriteLine($"PASS {momentsCount} arc moment records; {count} prepared moment costs and exhaustive group paths; ties, labels, clusters, finite checks and cancellation.");
static LegacyPreparedLineGroup ReadGroup(JsonElement a)=>new(a[0].GetInt32(),a[1].GetInt32(),a[2].GetDouble(),a[3].GetDouble(),a[4].GetDouble(),a[5].GetDouble(),a[6].GetDouble(),a[7].GetDouble(),a[8].GetDouble(),a[9].GetDouble(),a[10].GetDouble(),a[11].GetDouble(),a[12].GetBoolean(),a[13].GetInt32(),a[14].GetInt32());
static void Equal(double a,double b,string message){Require(double.IsFinite(a)&&Math.Abs(a-b)<=1e-9*Math.Max(1,Math.Abs(b)),message+$": {a} vs {b}");}
static void Require(bool test,string message){if(!test)throw new InvalidOperationException(message);}
static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new InvalidOperationException($"Expected {typeof(T).Name}");}
