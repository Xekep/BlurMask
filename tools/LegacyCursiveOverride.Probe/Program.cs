using InkRecognition;
using System.Text.Json;

InkStroke target=new(new[]{new InkPoint(0,0),new InkPoint(2,0),new InkPoint(4,0),new InkPoint(6,0),new InkPoint(8,0)});
InkStroke left=new(new[]{new InkPoint(1,-1),new InkPoint(1,1)}),right=new(new[]{new InkPoint(7,-1),new InkPoint(7,1)});
InkStroke[] original=[target,left,right];
var input=LegacyCursivePreparedInk.Create(original).WithTags([1,7,4,7,4,1,0,1,0]);
var midpoint=LegacyCursiveStrokeOverride.ApplyRussianVersion106(input,original,0,2,2,0,2);
Require(midpoint.Points.Length==10 && midpoint.Points.Span[3].X==4 && midpoint.Points.Span[3].Tag==2 && midpoint.Points.Span[3].OriginalPointIndex==2,"Native strict midpoint/provenance differs.");
InkStroke[] backward=[target,right,left];var reverse=LegacyCursivePreparedInk.Create(backward).WithTags([1,7,4,7,4,1,0,1,0]);
Require(LegacyCursiveStrokeOverride.ApplyRussianVersion106(reverse,backward,0,2,2,0,2).Points.Span.SequenceEqual(reverse.Points.Span),"Intersection encounter order was replaced with sorted segment indices.");
Require(LegacyCursiveStrokeOverride.ApplyRussianVersion106(input,original,0,null,2,0,2).Points.Span.SequenceEqual(input.Points.Span),"Unknown baseline incorrectly inferred.");
Require(LegacyCursiveStrokeOverride.ApplyRussianVersion106(input,original,0,float.MaxValue,2,0,2).Points.Span.SequenceEqual(input.Points.Span),"Unknown baseline sentinel differs.");
Require(LegacyCursiveStrokeOverride.ApplyRussianVersion106(input,original,0,1,2,0,2).Points.Span.SequenceEqual(input.Points.Span),"Envelope equality was accepted.");
var isolated=LegacyCursiveStrokeOverride.ApplyRussianVersion106(input,original,0,2,2,0,0);
Require(isolated.Points.ToArray().Take(5).Select(p=>p.Tag).SequenceEqual(new[]{1,0,0,0,0}),"Failed intersections did not clear following tags.");
Require(input.Points.Span[1].Tag==7,"Override mutated source.");
Reject(()=>LegacyCursiveStrokeOverride.ApplyRussianVersion106(input,original,0,2,0,0,2));
Reject(()=>LegacyCursiveStrokeOverride.ApplyRussianVersion106(input,original,0,float.NaN,2,0,2));
Reject(()=>LegacyCursiveStrokeOverride.ApplyRussianVersion106(input,original,0,2,2,1,2));
using var cts=new CancellationTokenSource();cts.Cancel();
try{LegacyCursiveStrokeOverride.ApplyRussianVersion106(input,original,0,2,2,0,2,cts.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}

int controls=0,inserts=0;
if(args.Length!=0)
{
    using var json=JsonDocument.Parse(File.ReadAllBytes(args[0]));
    foreach(var test in json.RootElement.EnumerateArray())
    {
        var strokes=test.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
        var ink=LegacyCursivePreparedInk.Create(strokes).WithTags(test.GetProperty("tags").EnumerateArray().Select(p=>p.GetInt32()).ToArray());
        float? baseline=test.GetProperty("baseline").ValueKind==JsonValueKind.Null ? null:test.GetProperty("baseline").GetSingle();
        var result=LegacyCursiveStrokeOverride.ApplyRussianVersion106(ink,strokes,test.GetProperty("stroke").GetInt32(),baseline,test.GetProperty("height").GetSingle(),test.GetProperty("first").GetInt32(),test.GetProperty("last").GetInt32());
        var rows=test.GetProperty("expected").EnumerateArray().ToArray();Require(result.Points.Length==rows.Length,$"Control{controls}: override point count differs.");
        for(int i=0;i<rows.Length;i++)
        {
            var p=result.Points.Span[i];var e=rows[i];float[] values=[p.X,p.Y,p.DeltaX,p.DeltaY,p.SegmentLength];
            for(int slot=0;slot<5;slot++)Require(BitConverter.SingleToInt32Bits(values[slot])==e[slot].GetInt32(),$"Control{controls},point{i},slot{slot}: override float bits differ.");
            Require(p.Tag==e[5].GetInt32() && p.StrokeIndex==e[6].GetInt32() && p.OriginalPointIndex==e[7].GetInt32(),$"Control{controls},point{i}: override tags/ownership differ.");
        }
        Require(result.StrokePointCounts.Span.SequenceEqual(test.GetProperty("counts").EnumerateArray().Select(p=>p.GetInt32()).ToArray()),"Override counts differ.");
        if(result.Points.Length>ink.Points.Length)inserts++;controls++;
    }
}
Console.WriteLine($"PASS native1908c0 Russian106 override: {controls} independent float-bit controls ({inserts} with inserts), raw envelope, bounded selected range, ordered intersections and provenance. Optional neural runtime branch remains separate.");
static void Require(bool ok,string why){if(!ok)throw new InvalidOperationException(why);}
static void Reject(Action action){try{action();throw new Exception("Malformed override accepted.");}catch(ArgumentException){}}
