using InkRecognition;
using System.Text.Json;
int controls=0,inserts=0;
if(args.Length!=0)
{
    using var json=JsonDocument.Parse(File.ReadAllBytes(args[0]));
    foreach(var test in json.RootElement.EnumerateArray())
    {
        var strokes=test.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
        var ink=LegacyCursivePreparedInk.Create(strokes);
        float? baseline=test.GetProperty("baseline").ValueKind==JsonValueKind.Null ? null:test.GetProperty("baseline").GetSingle();
        int? cap=test.GetProperty("cap").ValueKind==JsonValueKind.Null ? null:test.GetProperty("cap").GetInt32();
        var output=LegacyCursiveCutProducer.GenerateRussianVersion106(strokes,baseline,test.GetProperty("height").GetSingle(),test.GetProperty("confidence").EnumerateArray().Select(c=>c.GetSingle()).ToArray(),test.GetProperty("whole").GetBoolean(),cap);
        var result=output.PreparedInk;
        var expectedMarkers=test.GetProperty("markers").EnumerateArray().Select(m=>new LegacyCursiveCutMarker(m[0].GetInt32(),m[1].GetInt32(),m[2].GetInt32())).ToArray();
        Require(output.Markers.SequenceEqual(expectedMarkers),"Pipeline projected markers differ.");
        Require(output.Atoms.Count==output.Markers.Count,"Pipeline atom count differs.");
        for(int atom=0;atom<output.Atoms.Count;atom++)
        {
            var marker=output.Markers[atom];int end=atom+1<output.Markers.Count && output.Markers[atom+1].StrokeIndex==marker.StrokeIndex ? output.Markers[atom+1].PointIndex+1:result.StrokePointCounts.Span[marker.StrokeIndex];
            Require(output.Atoms[atom]==new LegacyCursiveInkAtom(marker,new LegacyInkRange(marker.StrokeIndex,marker.PointIndex,end)),"Pipeline prepared atom ownership differs.");
        }
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
if(args.Length>1)
{
    var data=File.ReadAllBytes(args[1]);foreach(int offset in new[]{11506,188157})Require(data[offset+33]==1 && data[offset+34]==0 && BitConverter.ToSingle(data,offset+47)==.5f,"Actual Russian directional flags/pruning radius differ.");
}
InkStroke[] own=[new(new[]{new InkPoint(0,0),new InkPoint(1,0)})];
Reject(()=>LegacyCursiveCutProducer.GenerateRussianVersion106(own,null,0,0));Reject(()=>LegacyCursiveCutProducer.GenerateRussianVersion106(own,null,1,float.NaN));
Reject(()=>LegacyCursiveCutProducer.GenerateRussianVersion106(own,null,1,Array.Empty<float>()));
var scalar=LegacyCursiveCutProducer.GenerateRussianVersion106(own,null,1,0);
var perStroke=LegacyCursiveCutProducer.GenerateRussianVersion106(own,null,1,new float[]{0});
Require(scalar.PreparedInk.Points.Span.SequenceEqual(perStroke.PreparedInk.Points.Span),"Explicit constant-confidence overload differs.");
using var cts=new CancellationTokenSource();cts.Cancel();
try{LegacyCursiveCutProducer.GenerateRussianVersion106(own,null,1,0,false,null,cts.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
Console.WriteLine($"PASS Russian106 whole-buffer191950 cut producer: {controls} independent full-record float-bit pipeline controls ({inserts} with inserts), runtime mode/confidence branches, mandatory override, projected markers and prepared atom ownership. Candidate lattice/acoustic normalization and modern neural wrappers remain separate.");
static void Require(bool ok,string why){if(!ok)throw new InvalidOperationException(why);}
static void Reject(Action action){try{action();throw new Exception("Malformed producer accepted.");}catch(ArgumentException){}}
