using InkRecognition;
using System.Text.Json;

var input=LegacyCursivePreparedInk.Create(new[]{new InkStroke(new[]{new InkPoint(0,0),new InkPoint(1,0),new InkPoint(2,0),new InkPoint(3,0)})}).WithTags([1,12,0,6]);
Require(LegacyCursiveBaselineCuts.Apply(input,0,1,2,.35f).Points.ToArray().Select(p=>p.Tag).SequenceEqual(new[]{1,2,0,6}),"Below-baseline loop exit cut differs.");
Require(LegacyCursiveBaselineCuts.Apply(input,0,null,2,.35f).Points.Span.SequenceEqual(input.Points.Span),"Unknown baseline incorrectly inferred.");
Require(LegacyCursiveBaselineCuts.Apply(input,0,float.MaxValue,2,.35f).Points.Span.SequenceEqual(input.Points.Span),"Native unknown-baseline sentinel differs.");
var nextLoop=LegacyCursiveBaselineCuts.Apply(input.WithTags([1,12,0,11]),0,null,2,.35f);
Require(nextLoop.Points.Length==5 && nextLoop.Points.Span[3].X==2 && nextLoop.Points.Span[3].Tag==2 && nextLoop.Points.Span[3].OriginalPointIndex==2,"Consecutive loop half-arc insertion differs.");
Require(nextLoop.Points.ToArray().Select(p=>p.Tag).SequenceEqual(new[]{1,12,0,2,11}),"Inserted loop-exit tag order differs.");
Require(input.Points.Length==4 && input.Points.Span[1].Tag==12,"Baseline stage mutated input.");
Reject(()=>LegacyCursiveBaselineCuts.Apply(input,0,float.NaN,2,.35f));Reject(()=>LegacyCursiveBaselineCuts.Apply(input,3,1,2,.35f));
using var cts=new CancellationTokenSource();cts.Cancel();
try{LegacyCursiveBaselineCuts.Apply(input,0,1,2,.35f,cts.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
int controls=0,inserts=0;
if(args.Length!=0)
{
    using var json=JsonDocument.Parse(File.ReadAllBytes(args[0]));
    foreach(var test in json.RootElement.EnumerateArray())
    {
        var strokes=test.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
        var ink=LegacyCursivePreparedInk.Create(strokes).WithTags(test.GetProperty("tags").EnumerateArray().Select(p=>p.GetInt32()).ToArray());
        float? baseline=test.GetProperty("baseline").ValueKind==JsonValueKind.Null ? null:test.GetProperty("baseline").GetSingle();
        var result=LegacyCursiveBaselineCuts.Apply(ink,test.GetProperty("stroke").GetInt32(),baseline,test.GetProperty("height").GetSingle(),.35f);
        var rows=test.GetProperty("expected").EnumerateArray().ToArray();Require(result.Points.Length==rows.Length,$"Control{controls}: baseline point count differs.");
        for(int i=0;i<rows.Length;i++)
        {
            var p=result.Points.Span[i];var e=rows[i];float[] values=[p.X,p.Y,p.DeltaX,p.DeltaY,p.SegmentLength];
            for(int slot=0;slot<5;slot++)Require(BitConverter.SingleToInt32Bits(values[slot])==e[slot].GetInt32(),$"Control{controls},point{i},slot{slot}: baseline float bits differ.");
            Require(p.Tag==e[5].GetInt32() && p.StrokeIndex==e[6].GetInt32() && p.OriginalPointIndex==e[7].GetInt32(),$"Control{controls},point{i}: baseline tags/ownership differ.");
        }
        Require(result.StrokePointCounts.Span.SequenceEqual(test.GetProperty("counts").EnumerateArray().Select(p=>p.GetInt32()).ToArray()),"Baseline counts differ.");
        if(result.Points.Length>ink.Points.Length)inserts++;controls++;
    }
}
if(args.Length>1)
{
    var data=File.ReadAllBytes(args[1]);
    foreach(int offset in new[]{11506,188157})Require(BitConverter.ToSingle(data,offset+35)==LegacyCursiveBaselineCuts.RussianGapRadius,"RU baseline gap radius differs.");
}
Console.WriteLine($"PASS native190350 baseline cuts: explicit/unknown baseline, loop exit tests, reverse/forward arc order, strict midpoints, raw-point provenance, bounds, cancellation; {controls} independent controls ({inserts} with inserts). Full191950 controller remains separate.");
static void Require(bool ok,string why){if(!ok)throw new InvalidOperationException(why);}
static void Reject(Action action){try{action();throw new Exception("Malformed baseline stage accepted.");}catch(ArgumentException){}}
