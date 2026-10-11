using InkRecognition;
using System.Text.Json;

LegacyInkStroke a = new([new(0,0), new(10,80), new(20,0), new(30,80), new(40,0)]);
LegacyInkStroke b = new([new(15,-20), new(15,-19)]);
LegacyInkStroke[] source = [a,b];
LegacyInkRange[] ranges = [new(0,1,4), new(1,0,2)];
var clipped = LegacyInkRangeExtraction.Extract(source, ranges);
Require(clipped.Strokes.Count == 2 && clipped.Strokes[0].Points.SequenceEqual(a.Points.Slice(1,3)), "Interior point range changed.");
Require(clipped.Strokes[1].Points.SequenceEqual(b.Points), "Late dot ownership changed.");
Require(clipped.StartBoundary == -1024 && clipped.EndBoundary == 1024, "Clipped start boundary lost.");
ranges[0] = new(0,0,1);
Require(clipped.SourceRanges.Span[0] == new LegacyInkRange(0,1,4), "Caller range mutation leaked.");
var adjacent = LegacyInkRangeExtraction.Extract(source, [new(0,0,2), new(0,2,5)]);
Require(adjacent.Strokes.Count == 2 && adjacent.Strokes[0].Points.Length == 2 && adjacent.Strokes[1].Points.Length == 3,
    "Adjacent same-source ranges merged or duplicated a boundary point.");
var reversed = LegacyInkRangeExtraction.Extract(source, [new(1,0,2), new(0,1,4)]);
Require(reversed.StartBoundary == 1024 && reversed.EndBoundary == -1024 && reversed.Strokes[0].Points.SequenceEqual(b.Points),
    "Range order was sorted or global minimum/maximum replaced endpoint flags.");
Require(LegacyInkRangeExtraction.Extract(source,[new(0,0,5)],true,true) is {StartBoundary:-1024,EndBoundary:-1024}, "Logical suppression ignored.");
// Float adapter copies exact bits, including negative zero; it must not normalize.
InkPoint[] floats = [new(-0f,1.25f), new(20.5f,-1.125f), new(10.25f,80.5f)];
InkStroke[] floatSource = [new(floats)];
var selectedFloat = LegacyInkRangeExtraction.ExtractFloat(floatSource,[new(0,0,3)]);
Require(BitConverter.SingleToInt32Bits(selectedFloat.Strokes[0].Points[0].X) == BitConverter.SingleToInt32Bits(-0f), "Float bits changed.");
floats[1] = new(999,999);
Require(selectedFloat.Strokes[0].Points[1] == new InkPoint(20.5f,-1.125f), "Caller float mutation leaked.");
Require(selectedFloat is {StartBoundary:1,EndBoundary:1}, "Float boundaries differ.");
foreach (var bad in new LegacyInkRange[][] { [], [new(-1,0,1)], [new(2,0,1)], [new(0,-1,1)], [new(0,1,1)], [new(0,3,2)], [new(0,0,6)] })
    Reject(() => LegacyInkRangeExtraction.Extract(source,bad));
Reject(() => LegacyInkRangeExtraction.Extract(source,Enumerable.Repeat(new LegacyInkRange(0,0,5),16).ToArray()));
Reject(() => LegacyInkRangeExtraction.Extract([], [new(0,0,1)]));
Reject(() => LegacyInkRangeExtraction.ExtractFloat([new([new(float.NaN,0)])],[new(0,0,1)]));
var dense = new LegacyInkStroke(Enumerable.Repeat(new LegacyInkPoint(0,0),4096).ToArray());
Reject(() => LegacyInkRangeExtraction.Extract([dense],[new(0,0,4096),new(0,0,1)]));
using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
try { LegacyInkRangeExtraction.Extract(source,[new(0,0,1)],cancellationToken:cancelled.Token); throw new Exception("Cancellation ignored."); }
catch (OperationCanceledException) { }

int referenceCases = 0;
if (args.Length != 0)
{
    using var json = JsonDocument.Parse(File.ReadAllBytes(args[0]));
    foreach (var control in json.RootElement.EnumerateArray())
    {
        var src = control.GetProperty("source").EnumerateArray().Select(s => new LegacyInkStroke(s.EnumerateArray()
            .Select(p => new LegacyInkPoint(p[0].GetInt16(),p[1].GetInt16())).ToArray())).ToArray();
        var intervals = control.GetProperty("ranges").EnumerateArray().Select(r => new LegacyInkRange(r[0].GetInt32(),r[1].GetInt32(),r[2].GetInt32())).ToArray();
        bool suppressStart = control.GetProperty("suppressStart").GetBoolean(), suppressEnd = control.GetProperty("suppressEnd").GetBoolean();
        var actual = LegacyInkRangeExtraction.Extract(src,intervals,suppressStart,suppressEnd);
        var expected = control.GetProperty("selected").EnumerateArray().ToArray();
        Require(actual.Strokes.Count == expected.Length, "Reference stroke count differs.");
        for (int i=0;i<expected.Length;i++)
        {
            var points = expected[i].EnumerateArray().Select(p => new LegacyInkPoint(p[0].GetInt16(),p[1].GetInt16())).ToArray();
            Require(actual.Strokes[i].Points.SequenceEqual(points), "Reference points/order differ.");
        }
        Require(actual.StartBoundary == control.GetProperty("startBoundary").GetInt16() && actual.EndBoundary == control.GetProperty("endBoundary").GetInt16(), "Reference boundary flags differ.");
        var fs = src.Select(s=>new InkStroke(s.Points.ToArray().Select(p=>new InkPoint(p.X,p.Y)).ToArray())).ToArray();
        var af = LegacyInkRangeExtraction.ExtractFloat(fs,intervals,suppressStart,suppressEnd);
        for (int i=0;i<actual.Strokes.Count;i++)
            Require(af.Strokes[i].Points.SequenceEqual(actual.Strokes[i].Points.ToArray().Select(p=>new InkPoint(p.X,p.Y))), "Float reference differs.");
        Require(af.StartBoundary * 1024 == actual.StartBoundary && af.EndBoundary * 1024 == actual.EndBoundary, "Float reference flags differ.");
        referenceCases++;
    }
}
Console.WriteLine($"PASS native point ranges: half-open interior cuts, late dots, source order, pen-up boundaries, clipped/logical flags, immutable copies, float bits, work bounds, cancellation; {referenceCases} independent references. Candidate cut inference is separate.");
static void Require(bool ok,string why) { if (!ok) throw new InvalidOperationException(why); }
static void Reject(Action action) { try { action(); throw new Exception("Malformed range accepted."); } catch (ArgumentException) { } }
