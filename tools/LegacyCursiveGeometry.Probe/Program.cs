using InkRecognition;
using System.Text.Json;

int[] tags = [1,2,2,2,2,1];
var pruned = LegacyCursiveMarkerGeometry.PruneNearPenLifts([5,1],[1,1,1,1,0,0],tags,4,.5f);
Require(pruned.SequenceEqual(new[]{1,0,2,0,0,1}),"Pen-lift radius or equality differs.");
Require(tags.SequenceEqual(new[]{1,2,2,2,2,1}),"Prepared tags mutated.");
Require(LegacyCursiveMarkerGeometry.PruneNearPenLifts([3],[1,1,0],[1,7,8],0,.5f).SequenceEqual(new[]{1,7,0}),"Final-point clearing incorrectly depends on distance.");
var reversals = LegacyCursiveMarkerGeometry.MarkVerticalReversals([6],[1,1,-1,1,1,0],[1,-1,0,1,-1,0],[1,0,0,0,0,99]);
Require(reversals.SequenceEqual(new[]{1,7,0,0,7,99}),"Zero-dy state, previous-dx gate or final-point handling differs.");
Require(LegacyCursiveMarkerGeometry.MarkVerticalReversals([3],[1,1,0],[-1,1,0],[1,0,0])[1]==8,"Right-going positive dy reversal not marked8.");
var markers=LegacyCursiveMarkerGeometry.ProjectMarkers([8],[0,1,10,11,12,13,14,15]);
Require(markers.SequenceEqual(new LegacyCursiveCutMarker[]{new(1,0,1),new(10,0,2),new(15,0,7)}),"Native unsigned tag projection differs.");
Reject(()=>LegacyCursiveMarkerGeometry.PruneNearPenLifts([2],[1],[1,2],1,.5f));
Reject(()=>LegacyCursiveMarkerGeometry.PruneNearPenLifts([1],[-1],[1],1,.5f));
Reject(()=>LegacyCursiveMarkerGeometry.PruneNearPenLifts([1],[0],[1],float.MaxValue,2));
Reject(()=>LegacyCursiveMarkerGeometry.MarkVerticalReversals([1],[float.NaN],[0],[1]));
Reject(()=>LegacyCursiveMarkerGeometry.ProjectMarkers([4096,1],new int[4097]));
using var cts=new CancellationTokenSource();cts.Cancel();
try{LegacyCursiveMarkerGeometry.ProjectMarkers([1],[1],cts.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
int controls=0;
if(args.Length != 0)
{
    using var json=JsonDocument.Parse(File.ReadAllBytes(args[0]));
    foreach(var test in json.RootElement.EnumerateArray())
    {
        var counts=Ints(test.GetProperty("counts"));var input=Ints(test.GetProperty("tags"));
        var actual=LegacyCursiveMarkerGeometry.MarkVerticalReversals(counts,Floats(test.GetProperty("dx")),Floats(test.GetProperty("dy")),input);
        Require(actual.SequenceEqual(Ints(test.GetProperty("reversed"))),"Independent reversal reference differs.");
        var cleaned=LegacyCursiveMarkerGeometry.PruneNearPenLifts(counts,Floats(test.GetProperty("lengths")),actual,test.GetProperty("height").GetSingle(),test.GetProperty("radius").GetSingle());
        Require(cleaned.SequenceEqual(Ints(test.GetProperty("pruned"))),"Independent pruning reference differs.");
        var projected=LegacyCursiveMarkerGeometry.ProjectMarkers(counts,cleaned);
        var expected=test.GetProperty("markers").EnumerateArray().Select(p=>new LegacyCursiveCutMarker(p[0].GetInt32(),p[1].GetInt32(),p[2].GetInt32()));
        Require(projected.SequenceEqual(expected),"Independent ordered marker reference differs.");controls++;
    }
}
Console.WriteLine($"PASS recovered sampler297 geometry stages: vertical reversals, distance equality/pen-lift pruning, ordered tag projection, immutability, bounds, cancellation; {controls} independent controls. Full candidate-cut producer remains separate.");
static int[] Ints(JsonElement e)=>e.EnumerateArray().Select(p=>p.GetInt32()).ToArray();
static float[] Floats(JsonElement e)=>e.EnumerateArray().Select(p=>p.GetSingle()).ToArray();
static void Require(bool ok,string why){if(!ok)throw new InvalidOperationException(why);}
static void Reject(Action action){try{action();throw new Exception("Malformed geometry accepted.");}catch(ArgumentException){}}
