using InkRecognition;
using System.Text.Json;

InkStroke[] source=[new(Enumerable.Range(0,7).Select(i=>new InkPoint(i,0)).ToArray())];
var input=LegacyCursivePreparedInk.Create(source).WithTags([1,7,4,2,2,2,2]);
var limited=LegacyCursiveMarkerLimit.Apply(input,source,100,LegacyCursiveMarkerLimitOptions.RussianVersion106());
Require(limited.Points.ToArray().Select(p=>p.Tag).SequenceEqual(new[]{1,7,4,0,0,0,0}),"Native protected tags or exhausted candidate behavior differs.");
Require(input.Points.Span[3].Tag==2,"Limit stage mutated input.");
InkStroke[] six=[new(Enumerable.Range(0,6).Select(i=>new InkPoint(i,0)).ToArray())];
var equalGate=LegacyCursivePreparedInk.Create(six).WithTags([1,2,2,2,2,2]);
Require(LegacyCursiveMarkerLimit.Apply(equalGate,six,100,LegacyCursiveMarkerLimitOptions.RussianVersion106()).Points.Span.SequenceEqual(equalGate.Points.Span),"Minimum-gate equality incorrectly pruned.");
var missingStart=input.WithTags([2,2,2,2,2,2,2]);
Require(LegacyCursiveMarkerLimit.Apply(missingStart,source,100,LegacyCursiveMarkerLimitOptions.RussianVersion106()).Points.Span.SequenceEqual(missingStart.Points.Span),"Missing first tag1 was synthesized.");
InkStroke[] closedSource=[new(new[]{new InkPoint(0,0),new InkPoint(1,0),new InkPoint(2,0),new InkPoint(3,0),new InkPoint(4,0),new InkPoint(6,0),new InkPoint(0,0)})];
var closed=LegacyCursivePreparedInk.Create(closedSource).WithTags([1,7,4,2,2,2,2]);
Require(LegacyCursiveMarkerLimit.Apply(closed,closedSource,2,LegacyCursiveMarkerLimitOptions.RussianVersion106()).Points.Span.SequenceEqual(closed.Points.Span),"Upstream closed category cap was invented.");
Require(LegacyCursiveMarkerLimit.Apply(closed,closedSource,2,LegacyCursiveMarkerLimitOptions.RussianVersion106(2)).Points.ToArray().Select(p=>p.Tag).SequenceEqual(new[]{1,7,4,0,0,0,0}),"Explicit closed category cap was ignored.");
Reject(()=>LegacyCursiveMarkerLimit.Apply(input,source,0,LegacyCursiveMarkerLimitOptions.RussianVersion106()));
Reject(()=>LegacyCursiveMarkerLimit.Apply(input,source,1,new(256,4,null)));
Reject(()=>LegacyCursiveMarkerLimit.Apply(input,source,1,new(6,4,-2)));
using var cts=new CancellationTokenSource();cts.Cancel();
try{LegacyCursiveMarkerLimit.Apply(input,source,1,new(6,4,null),cts.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
int controls=0,changed=0;
if(args.Length!=0)
{
    using var json=JsonDocument.Parse(File.ReadAllBytes(args[0]));
    foreach(var test in json.RootElement.EnumerateArray())
    {
        Require(BitConverter.SingleToInt32Bits(LegacyCursiveMarkerLimit.CombineWeights(test.GetProperty("merge_left").GetSingle(),test.GetProperty("merge_right").GetSingle()))==test.GetProperty("merge_bits").GetInt32(),"Double precision merge cost differs.");
        var strokes=test.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
        var ink=LegacyCursivePreparedInk.Create(strokes).WithTags(test.GetProperty("tags").EnumerateArray().Select(p=>p.GetInt32()).ToArray());
        int? cap=test.GetProperty("closed_cap").ValueKind==JsonValueKind.Null ? null:test.GetProperty("closed_cap").GetInt32();
        var result=LegacyCursiveMarkerLimit.Apply(ink,strokes,test.GetProperty("height").GetSingle(),new(6,4,cap));
        Require(result.Points.ToArray().Select(p=>p.Tag).SequenceEqual(test.GetProperty("expected").EnumerateArray().Select(p=>p.GetInt32())),$"Control{controls}: independent final marker selection differs.");
        for(int i=0;i<result.Points.Length;i++)Require(result.Points.Span[i] with{Tag=ink.Points.Span[i].Tag}==ink.Points.Span[i],"Limiter changed point geometry/ownership.");
        if(!result.Points.Span.SequenceEqual(ink.Points.Span))changed++;controls++;
    }
}
if(args.Length>1)
{
    var data=File.ReadAllBytes(args[1]);
    foreach(int offset in new[]{11506,188157})Require(data[offset+59]==6 && BitConverter.ToSingle(data,offset+60)==4,"RU final limit fields differ.");
}
Console.WriteLine($"PASS native191430 marker limits: explicit raw width/endpoints and upstream cap, gate equality, native duplicate initial weights, min+0.1max merge, protected7/4, tie order, owned tags, bounds, cancellation; {controls} independent controls ({changed} changed). Full191950 controller remains separate.");
static void Require(bool ok,string why){if(!ok)throw new InvalidOperationException(why);}
static void Reject(Action action){try{action();throw new Exception("Malformed marker limit accepted.");}catch(ArgumentException){}}
