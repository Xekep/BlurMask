using InkRecognition;
using System.Text.Json;
var source=LegacyCursivePreparedInk.Create(new[]{new InkStroke(new[]{new InkPoint(0,0),new InkPoint(1,2),new InkPoint(2,1),new InkPoint(3,-1),new InkPoint(4,-2),new InkPoint(5,0)})}).WithTags([1,4,0,0,11,13]);
var moved=LegacyCursiveControllerFilters.AfterBaselineRussianVersion106(source,0,0,2);
Require(moved.Points.ToArray().Select(p=>p.Tag).SequenceEqual(new[]{1,0,2,0,11,13}),"Previous-point baseline relocation differs.");
var zeroGuard=LegacyCursivePreparedInk.Create(new[]{new InkStroke(new[]{new InkPoint(0,0),new InkPoint(1,4),new InkPoint(2,-1),new InkPoint(3,0),new InkPoint(4,0)})}).WithTags([1,4,0,2,1]);
Require(LegacyCursiveControllerFilters.AfterBaselineRussianVersion106(zeroGuard,0,0,2).Points.Span.SequenceEqual(zeroGuard.Points.Span),"Exact zero-Y guard was changed into a baseline test.");
Require(source.Points.Span[1].Tag==4,"Filters mutated input.");
Reject(()=>LegacyCursiveControllerFilters.BeforeBaseline(source,1));Reject(()=>LegacyCursiveControllerFilters.AfterBaselineRussianVersion106(source,0,0,0));
Reject(()=>LegacyCursiveControllerFilters.AfterBaselineRussianVersion106(source,0,float.NaN,2));
using var cts=new CancellationTokenSource();cts.Cancel();
try{LegacyCursiveControllerFilters.BeforeBaseline(source,0,cts.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
int controls=0,beforeChanged=0,afterChanged=0;
if(args.Length>0)
{
    using var json=JsonDocument.Parse(File.ReadAllBytes(args[0]));
    foreach(var test in json.RootElement.EnumerateArray())
    {
        var strokes=test.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
        var ink=LegacyCursivePreparedInk.Create(strokes).WithTags(test.GetProperty("tags").EnumerateArray().Select(p=>p.GetInt32()).ToArray());
        int stroke=test.GetProperty("stroke").GetInt32();float? baseline=test.GetProperty("baseline").ValueKind==JsonValueKind.Null ? null:test.GetProperty("baseline").GetSingle();
        var before=LegacyCursiveControllerFilters.BeforeBaseline(ink,stroke);
        var after=LegacyCursiveControllerFilters.AfterBaselineRussianVersion106(ink,stroke,baseline,test.GetProperty("height").GetSingle());
        Require(before.Points.ToArray().Select(p=>p.Tag).SequenceEqual(test.GetProperty("before").EnumerateArray().Select(p=>p.GetInt32())),$"Control{controls}: pre-baseline tags differ.");
        Require(after.Points.ToArray().Select(p=>p.Tag).SequenceEqual(test.GetProperty("after").EnumerateArray().Select(p=>p.GetInt32())),$"Control{controls}: post-baseline tags differ.");
        for(int i=0;i<ink.Points.Length;i++)Require(before.Points.Span[i] with{Tag=ink.Points.Span[i].Tag}==ink.Points.Span[i] && after.Points.Span[i] with{Tag=ink.Points.Span[i].Tag}==ink.Points.Span[i],"Filters changed point geometry/provenance.");
        if(!before.Points.Span.SequenceEqual(ink.Points.Span))beforeChanged++;if(!after.Points.Span.SequenceEqual(ink.Points.Span))afterChanged++;controls++;
    }
}
if(args.Length>1)
{
    var data=File.ReadAllBytes(args[1]);foreach(int offset in new[]{11506,188157})Require(BitConverter.ToSingle(data,offset+51)==LegacyCursiveControllerFilters.RussianHighAnchorMargin && BitConverter.ToSingle(data,offset+39)==LegacyCursiveControllerFilters.RussianLoopDropThreshold,"Actual Russian controller fields differ.");
}
Console.WriteLine($"PASS191950 before/after-baseline controller filters: {controls} independent controls ({beforeChanged} before/{afterChanged} after changed), strict slopes, ordered loop anchors, previous-point relocation, exact zero-Y guard and preserved geometry/provenance. Full runtime controller remains separate.");
static void Require(bool ok,string why){if(!ok)throw new InvalidOperationException(why);}
static void Reject(Action action){try{action();throw new Exception("Malformed filters accepted.");}catch(ArgumentException){}}
