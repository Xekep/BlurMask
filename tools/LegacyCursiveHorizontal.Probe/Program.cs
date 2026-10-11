using InkRecognition;
using System.Text.Json;

var settings=LegacyCursiveHorizontalCutSettings.RussianVersion106;
var input=LegacyCursivePreparedInk.Create(new[]{new InkStroke(new[]{new InkPoint(0,0),new InkPoint(2,0),new InkPoint(4,0),new InkPoint(6,0)})});
var unchanged=LegacyCursiveHorizontalCuts.Apply(input,0,10,settings);
Require(unchanged.Points.Span.SequenceEqual(input.Points.Span),"Straight arc without surrounding native cut evidence changed.");
var zeroChord=LegacyCursivePreparedInk.Create(new[]{new InkStroke(new[]{new InkPoint(0,0),new InkPoint(2,0),new InkPoint(4,0),new InkPoint(0,0)})});
var closed=LegacyCursiveHorizontalCuts.Apply(zeroChord,0,2,settings);
Require(closed.Points.Length==5 && closed.Points.Span[3].Tag==5 && closed.Points.Span[3].X==4 && closed.Points.Span[3].OriginalPointIndex==2,"Unordered zero-chord comparisons or strict arc crossing differ.");
Reject(()=>LegacyCursiveHorizontalCuts.Apply(input,0,0,settings));
Reject(()=>LegacyCursiveHorizontalCuts.Apply(input,3,1,settings));
Reject(()=>LegacyCursiveHorizontalCuts.Apply(input,0,1,settings with{LooseSlope=0}));
using var cts=new CancellationTokenSource();cts.Cancel();
try{LegacyCursiveHorizontalCuts.Apply(input,0,1,settings,cancellationToken:cts.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
int controls=0,insertedControls=0;
if(args.Length!=0)
{
    using var json=JsonDocument.Parse(File.ReadAllBytes(args[0]));
    foreach(var test in json.RootElement.EnumerateArray())
    {
        var strokes=test.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
        var ink=LegacyCursivePreparedInk.Create(strokes).WithTags(test.GetProperty("tags").EnumerateArray().Select(p=>p.GetInt32()).ToArray());
        var actual=LegacyCursiveHorizontalCuts.Apply(ink,test.GetProperty("stroke").GetInt32(),test.GetProperty("height").GetSingle(),settings with{KeepOneAnchor=test.GetProperty("keep").GetBoolean()},test.GetProperty("reverse").GetBoolean());
        var rows=test.GetProperty("expected").EnumerateArray().ToArray();
        Require(actual.Points.Length==rows.Length,"Independent horizontal record count differs.");
        for(int i=0;i<rows.Length;i++)
        {
            var p=actual.Points.Span[i];var e=rows[i];float[] values=[p.X,p.Y,p.DeltaX,p.DeltaY,p.SegmentLength];
            for(int f=0;f<values.Length;f++)Require(BitConverter.SingleToInt32Bits(values[f])==e[f].GetInt32(),$"Control{controls},point{i},slot{f}: independent horizontal float bits differ.");
            Require(p.Tag==e[5].GetInt32() && p.StrokeIndex==e[6].GetInt32() && p.OriginalPointIndex==e[7].GetInt32(),$"Control{controls},point{i}: independent horizontal tag/ownership differs.");
        }
        Require(actual.StrokePointCounts.Span.SequenceEqual(test.GetProperty("counts").EnumerateArray().Select(p=>p.GetInt32()).ToArray()),"Horizontal insertion counts differ.");
        if(actual.Points.Length>ink.Points.Length)insertedControls++;
        controls++;
    }
}
if(args.Length>1)
{
    var data=File.ReadAllBytes(args[1]);
    foreach(int offset in new[]{11506,188157})
    {
        var floats=new[]{settings.LooseSlope,settings.StrongSlope,settings.MinimumArc,settings.LongArc};
        for(int i=0;i<4;i++)Require(BitConverter.ToSingle(data,offset+16+i*4)==floats[i],"RU serialized slope/arc settings differ.");
        Require(data[offset+32]==0 && data[offset+33]==1 && data[offset+34]==0,"RU horizontal-stage flags differ.");
        Require(BitConverter.ToSingle(data,offset+35)==settings.GapRadius && BitConverter.ToSingle(data,offset+43)==settings.ReversalSpacing,"RU gap/spacing settings differ.");
    }
}
Console.WriteLine($"PASS native18e960 stage: segment state, reversal cases, loop barriers, explicit RU settings, inserted point float bits/maps, cleanup, bounds, cancellation; {controls} independent controls ({insertedControls} with inserts). Full191950 producer remains separate.");
static void Require(bool ok,string why){if(!ok)throw new InvalidOperationException(why);}
static void Reject(Action action){try{action();throw new Exception("Malformed stage accepted.");}catch(ArgumentException){}}
