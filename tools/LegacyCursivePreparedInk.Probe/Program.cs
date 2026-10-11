using InkRecognition;
using System.Text.Json;

var sourcePoints = new List<InkPoint>{new(0,0),new(4,3),new(8,0)};
var source = new InkStroke[]{new(sourcePoints),new(new[]{new InkPoint(10,2)})};
var prepared=LegacyCursivePreparedInk.Create(source);
Require(prepared.Points.Span[0].SegmentLength==5 && prepared.Points.Span[2].DeltaX==2 && prepared.Points.Span[2].DeltaY==2,"Native flattened forward preparation differs.");
Require(prepared.Points.Span[3].Tag==1 && prepared.Points.Span[3].SegmentLength==0,"Physical start tag or final global zero differs.");
var inserted=prepared.Insert(0,1,new(2,1.5f),5);
Require(inserted.Points.Span[1].OriginalPointIndex==0 && inserted.Points.Span[2].OriginalPointIndex==1 && inserted.Points.Span[3].OriginalPointIndex==2 && inserted.Points.Span[4].OriginalPointIndex==3,"Inserted ownership did not duplicate the preceding raw index.");
Require(inserted.Points.Span[0].SegmentLength==2.5f && inserted.Points.Span[1].SegmentLength==2.5f && inserted.Points.Span[2]==prepared.Points.Span[1],"Insert modified unrelated prepared records.");
Require(inserted.StrokePointCounts.Span.SequenceEqual(new[]{4,1}) && prepared.StrokePointCounts.Span.SequenceEqual(new[]{3,1}),"Insert mutated previous counts.");
sourcePoints[0]=new(90,90);Require(prepared.Points.Span[0].X==0,"Raw caller data mutation leaked.");
var huge=LegacyCursivePreparedInk.Create(new[]{new InkStroke(new[]{new InkPoint(0,0),new InkPoint(1e30f,1e30f)})});
Require(float.IsFinite(huge.Points.Span[0].SegmentLength),"Length incorrectly squared float operands before promoting.");
var tags=inserted.Points.ToArray().Select(p=>p.Tag).ToArray();tags[2]=7;
var tagged=inserted.WithTags(tags);tags[2]=99;
Require(tagged.Points.Span[2].Tag==7 && inserted.Points.Span[2].Tag==0,"Tag replacement ownership differs.");
var markers=LegacyCursiveMarkerGeometry.ProjectMarkers(tagged.StrokePointCounts.Span,tagged.Points.ToArray().Select(p=>p.Tag).ToArray());
var atoms=LegacyCursiveInkAtoms.Build(tagged.StrokePointCounts.Span,markers);
var path=LegacyCursiveInkAtoms.Compose(atoms.Select(a=>a.Range).ToArray());
var selected=LegacyInkRangeExtraction.ExtractFloat(tagged.ToStrokes(),path);
Require(selected.Strokes[0].Points.Count==4 && selected.Strokes[0].Points[1]==new InkPoint(2,1.5f),"Prepared ink atom path dropped an inserted cut point.");
Reject(()=>prepared.Insert(0,0,new(1,1),2));Reject(()=>prepared.Insert(0,3,new(1,1),2));
Reject(()=>LegacyCursivePreparedInk.Create(new[]{new InkStroke(new[]{new InkPoint(float.NaN,0)})}));
Reject(()=>prepared.WithTags([1]));
using var cts=new CancellationTokenSource();cts.Cancel();
try{LegacyCursivePreparedInk.Create(source,cts.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
int controls=0;
if(args.Length!=0)
{
    using var json=JsonDocument.Parse(File.ReadAllBytes(args[0]));
    foreach(var test in json.RootElement.EnumerateArray())
    {
        var ink=test.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
        var actual=LegacyCursivePreparedInk.Create(ink);Check(actual,test.GetProperty("prepared"));
        foreach(var insertion in test.GetProperty("inserts").EnumerateArray())
        {
            actual=actual.Insert(insertion.GetProperty("stroke").GetInt32(),insertion.GetProperty("point").GetInt32(),new(insertion.GetProperty("x").GetSingle(),insertion.GetProperty("y").GetSingle()),insertion.GetProperty("tag").GetInt32());
            Check(actual,insertion.GetProperty("expected"));
        }
        controls++;
    }
}
Console.WriteLine($"PASS native297 raw preparation and insertion: float coordinates, flattened inter-stroke deltas, double hypot rounding, prepared ownership/map duplication, atom/range handoff, immutable snapshots, bounds, cancellation; {controls} independent controls.");
static void Check(LegacyCursivePreparedInk ink,JsonElement expected)
{
    var rows=expected.EnumerateArray().ToArray();Require(rows.Length==ink.Points.Length,"Prepared record count differs.");
    for(int i=0;i<rows.Length;i++)
    {
        var p=ink.Points.Span[i];var e=rows[i];
        float[] values=[p.X,p.Y,p.DeltaX,p.DeltaY,p.SegmentLength];
        for(int f=0;f<values.Length;f++)Require(BitConverter.SingleToInt32Bits(values[f])==e[f].GetInt32(),"Independent prepared float bits differ.");
        Require(p.Tag==e[5].GetInt32() && p.StrokeIndex==e[6].GetInt32() && p.OriginalPointIndex==e[7].GetInt32(),"Prepared point ownership/tag differs.");
    }
}
static void Require(bool ok,string why){if(!ok)throw new InvalidOperationException(why);}
static void Reject(Action action){try{action();throw new Exception("Malformed prepared ink accepted.");}catch(ArgumentException){}}
