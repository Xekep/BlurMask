using InkRecognition;
using System.Text.Json;

Require(LegacyCursiveLoopCuts.TryIntersection(new(0,0),new(4,4),new(0,4),new(4,0),out var point) && point==new InkPoint(2,2),"Finite diagonal intersection differs.");
Require(!LegacyCursiveLoopCuts.TryIntersection(new(0,0),new(4,0),new(0,1),new(4,1),out _),"Parallel segments intersected.");
Require(LegacyCursiveLoopCuts.TryIntersection(new(0,0),new(1,0),new(1,0),new(1,1),out point) && point==new InkPoint(1,0),"Inclusive endpoint intersection differs.");
Require(LegacyCursiveLoopCuts.TryIntersection(new(0,0),new(1,0),new(0,0),new(0,1e-10f),out _),"Determinant equality was rejected.");
float smaller=BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(1e-10f)-1);
Require(!LegacyCursiveLoopCuts.TryIntersection(new(0,0),new(1,0),new(0,0),new(0,smaller),out _),"Below-cutoff determinant was accepted.");
var input=LegacyCursivePreparedInk.Create(new[]{new InkStroke(new[]{new InkPoint(0,0),new InkPoint(4,4),new InkPoint(0,4),new InkPoint(4,0)})});
var actual=LegacyCursiveLoopCuts.Apply(input,0,10,.1f);
Require(actual.Points.Length==6 && actual.Points.Span[1].X==2 && actual.Points.Span[4].Y==2,"Double intersection insertion differs.");
Require(actual.Points.ToArray().Select(p=>p.Tag).SequenceEqual(new[]{1,11,14,14,12,0}),"Positive loop interior policy differs.");
Require(actual.Points.ToArray().Select(p=>p.OriginalPointIndex).SequenceEqual(new[]{0,0,1,2,2,3}),"Double insert raw-map provenance differs.");
Require(input.Points.Length==4 && input.Points.Span[1].Tag==0,"Input snapshot changed.");
var reversed=LegacyCursivePreparedInk.Create(new[]{new InkStroke(new[]{new InkPoint(0,4),new InkPoint(4,0),new InkPoint(0,0),new InkPoint(4,4)})});
Require(LegacyCursiveLoopCuts.Apply(reversed,0,10,.1f).Points.ToArray().Select(p=>p.Tag).SequenceEqual(new[]{1,11,13,13,12,0}),"Negative loop interior policy differs.");
var tagged=input.WithTags([1,7,4,0]);
Require(LegacyCursiveLoopCuts.Apply(tagged,0,10,.1f).Points.ToArray().Select(p=>p.Tag).SequenceEqual(new[]{1,11,9,14,12,0}),"Non-nested positive loop7→9 policy differs.");
Require(LegacyCursiveLoopCuts.Apply(input.WithTags([1,7,13,0]),0,10,.1f).Points.ToArray().Select(p=>p.Tag).SequenceEqual(new[]{1,11,7,13,12,0}),"Nested loop preservation differs.");
Reject(()=>LegacyCursiveLoopCuts.Apply(input,0,0,.1f));Reject(()=>LegacyCursiveLoopCuts.Apply(input,2,10,.1f));
Reject(()=>LegacyCursiveLoopCuts.TryIntersection(new(float.NaN,0),new(1,1),new(0,1),new(1,0),out _));
using var cts=new CancellationTokenSource();cts.Cancel();
try{LegacyCursiveLoopCuts.Apply(input,0,1,.1f,cts.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
int controls=0,inserted=0;
if(args.Length!=0)
{
    using var json=JsonDocument.Parse(File.ReadAllBytes(args[0]));
    foreach(var test in json.RootElement.EnumerateArray())
    {
        var strokes=test.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
        var ink=LegacyCursivePreparedInk.Create(strokes).WithTags(test.GetProperty("tags").EnumerateArray().Select(p=>p.GetInt32()).ToArray());
        var result=LegacyCursiveLoopCuts.Apply(ink,test.GetProperty("stroke").GetInt32(),test.GetProperty("height").GetSingle(),.1f);
        var rows=test.GetProperty("expected").EnumerateArray().ToArray();Require(result.Points.Length==rows.Length,$"Control{controls}: record count differs.");
        for(int i=0;i<rows.Length;i++)
        {
            var p=result.Points.Span[i];var e=rows[i];float[] values=[p.X,p.Y,p.DeltaX,p.DeltaY,p.SegmentLength];
            for(int slot=0;slot<5;slot++)Require(BitConverter.SingleToInt32Bits(values[slot])==e[slot].GetInt32(),$"Control{controls},point{i},slot{slot}: loop float bits differ.");
            Require(p.Tag==e[5].GetInt32() && p.StrokeIndex==e[6].GetInt32() && p.OriginalPointIndex==e[7].GetInt32(),$"Control{controls},point{i}: loop tag/provenance differs.");
        }
        Require(result.StrokePointCounts.Span.SequenceEqual(test.GetProperty("counts").EnumerateArray().Select(p=>p.GetInt32()).ToArray()),"Loop counts differ.");
        if(result.Points.Length>ink.Points.Length)inserted++;controls++;
    }
}
if(args.Length>1)
{
    var data=File.ReadAllBytes(args[1]);
    foreach(int offset in new[]{11506,188157})Require(BitConverter.ToSingle(data,offset+55)==LegacyCursiveLoopCuts.RussianReversalSpacing,"RU loop spacing profile differs.");
}
Console.WriteLine($"PASS native18e1f0 loops: determinant/bounds, spaced reversals, turn sign, double insertion11/12, nested13/14/9 policy, raw-map ownership, bounds, cancellation; {controls} independent controls ({inserted} with loops). Full191950 controller remains separate.");
static void Require(bool ok,string why){if(!ok)throw new InvalidOperationException(why);}
static void Reject(Action action){try{action();throw new Exception("Malformed loop stage accepted.");}catch(ArgumentException){}}
