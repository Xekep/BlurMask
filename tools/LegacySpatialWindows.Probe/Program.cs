using InkRecognition;
using System.Text.Json;

var corners = new LegacyInkPoint[]{new(0,0),new(8,0),new(0,8),new(8,8)};
var uniform=LegacySpatialWindows.Partition(corners,8,8,2,2);
Require(uniform.Select(p=>p.ToArray()).Zip(new[]{new[]{0},new[]{1},new[]{2},new[]{3}}).All(p=>p.First.SequenceEqual(p.Second)),"Uniform spatial row-major layout or outside-final rule failed.");
var repeated=LegacySpatialWindows.Partition([new(0,0),new(0,0),new(0,0)],1,1,4,4,true);
Require(repeated[15].Span.SequenceEqual(new[]{0,1,2})&&repeated.Take(15).All(p=>p.IsEmpty),"Quantile zero-extent axes must fall through to the final bin.");
var negative=LegacySpatialWindows.Partition([new(-1,-1),new(0,0)],1,1,2,2);
Require(negative[3].Span.SequenceEqual(new[]{0})&&negative[0].Span.SequenceEqual(new[]{1}),"Negative unmatched coordinates must use the native final-bin fallback.");
Throws<ArgumentException>(()=>LegacySpatialWindows.Partition([],1,1));
Throws<ArgumentException>(()=>LegacySpatialWindows.Partition(corners,0,1));
Throws<ArgumentException>(()=>LegacySpatialWindows.Partition(corners,1,1,17,1));
Throws<ArgumentException>(()=>LegacySpatialWindows.Partition(new LegacyInkPoint[256],1,1));
Throws<OperationCanceledException>(()=>LegacySpatialWindows.Partition(corners,8,8,cancellationToken:new CancellationToken(true)));
int count=0;
if(args.Length>0)
{
    if(args.Length!=1||new FileInfo(args[0]).Length>8*1024*1024)throw new ArgumentException("Usage: LegacySpatialWindows.Probe [own-reference.json up to8MiB]");
    using var document=JsonDocument.Parse(File.ReadAllBytes(args[0]));
    foreach(var item in document.RootElement.GetProperty("cases").EnumerateArray())
    {
        var points=item.GetProperty("points").EnumerateArray().Select(p=>new LegacyInkPoint(p[0].GetInt16(),p[1].GetInt16())).ToArray();
        var groups=LegacySpatialWindows.Partition(points,item.GetProperty("width").GetInt32(),item.GetProperty("height").GetInt32(),item.GetProperty("rows").GetInt32(),item.GetProperty("columns").GetInt32(),item.GetProperty("quantiles").GetBoolean());
        var expected=item.GetProperty("groups").EnumerateArray().Select(p=>p.EnumerateArray().Select(v=>v.GetInt32()).ToArray()).ToArray();
        Require(groups.Length==expected.Length&&groups.Zip(expected).All(p=>p.First.Span.SequenceEqual(p.Second)), $"Independent spatial case{count} failed.");
        Require(groups.SelectMany(p=>p.ToArray()).Order().SequenceEqual(Enumerable.Range(0,points.Length)),"Each sample must occur once.");
        count++;
    }
}
Console.WriteLine($"PASS spatial membership: {count} independent controls, uniform/quantile row-major layout, duplicate boundaries, unmatched-final rule, malformed/cancellation. Not character segmentation or handwriting accuracy.");
static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
static void Throws<T>(Action action)where T:Exception{try{action();}catch(T){return;}throw new InvalidOperationException($"Expected{typeof(T).Name}.");}
