using InkRecognition;

// Own multi-stroke printed ИП. Acquisition order is deliberately interleaved.
LegacyInkStroke[] ink = [Stroke((80,90), (80,10)), Stroke((10,10), (10,90)),
    Stroke((80,10), (130,10)), Stroke((10,90), (60,10)),
    Stroke((130,10), (130,90)), Stroke((60,10), (60,90))];
for (int scale = 1; scale <= 4; scale++)
{
    var shifted = ink.Select(stroke => new LegacyInkStroke(stroke.Points.ToArray()
        .Select(p => new LegacyInkPoint((short)(p.X * scale - 1000), (short)(p.Y * scale + 500))).ToArray())).ToArray();
    Require(LegacySeparatedInkSegmenter.TrySegment(shifted, out var groups) && groups.Length == 2, "Separated ИП groups differ under scaling/translation.");
    Require(groups[0].SequenceEqual(new[] { shifted[1], shifted[3], shifted[5] }) &&
        groups[1].SequenceEqual(new[] { shifted[0], shifted[2], shifted[4] }), "Physical stroke acquisition order was lost.");
}
// Late accent belongs to its horizontal stem, not the neighboring letter.
var dotted = new[] { Stroke((10,30), (10,90)), Stroke((80,10), (80,90)), Stroke((10,10), (10,10)) };
Require(LegacySeparatedInkSegmenter.TrySegment(dotted, out var dottedGroups) && dottedGroups.Length == 2 &&
    dottedGroups[0].SequenceEqual(new[] { dotted[0], dotted[2] }), "Late dot grouped with the wrong glyph.");
// A pen-down bridge can join multiple letters. Never manufacture a cut in it.
var connected = Stroke((10,90), (10,10), (60,90), (80,10), (130,90));
Require(LegacySeparatedInkSegmenter.TrySegment([connected], out var whole) && whole.Length == 1 &&
    ReferenceEquals(whole[0][0], connected), "A connected stroke was cut without evidence.");
Require(!LegacySeparatedInkSegmenter.TrySegment([], out _), "Empty line accepted.");
Require(!LegacySeparatedInkSegmenter.TrySegment([Stroke((1,1), (1,1))], out _), "Degenerate line accepted.");
Require(!LegacySeparatedInkSegmenter.TrySegment([Stroke((1,1), (20,1))], out _), "Horizontal line accepted as separated text.");
Require(!LegacySeparatedInkSegmenter.TrySegment([Stroke((10,0), (10,80)), Stroke((80,100), (80,180))], out _), "Different vertical bands accepted.");
Require(!LegacySeparatedInkSegmenter.TrySegment([Stroke((10,0), (10,80)), Stroke((10,200), (10,280))], out _), "Stacked rows merged.");
Require(!LegacySeparatedInkSegmenter.TrySegment(Enumerable.Repeat(ink[0], 16).ToArray(), out _), "Too many strokes in one glyph accepted.");
var dense = new LegacyInkStroke(Enumerable.Range(0, 4096).Select(i => new LegacyInkPoint(10, (short)(i % 81))).ToArray());
Require(!LegacySeparatedInkSegmenter.TrySegment([dense, dense], out _), "Oversized glyph accepted.");
try { LegacySeparatedInkSegmenter.TrySegment(Enumerable.Repeat(ink[0], 129).ToArray(), out _); throw new Exception("Line stroke cap ignored."); }
catch (ArgumentException) { }
try { LegacySeparatedInkSegmenter.TrySegment(Enumerable.Repeat(dense, 9).ToArray(), out _); throw new Exception("Line point cap ignored."); }
catch (ArgumentException) { }
using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
try { LegacySeparatedInkSegmenter.TrySegment(ink, out _, cancellation.Token); throw new Exception("Cancellation ignored."); }
catch (OperationCanceledException) { }
Console.WriteLine("PASS separated ink: reading/acquisition order, late dots, scale/translation, whole connected strokes, ambiguous rows, point/stroke caps and cancellation. Cursive boundaries are not inferred.");

static LegacyInkStroke Stroke(params (short X, short Y)[] points) => new(points.Select(p => new LegacyInkPoint(p.X, p.Y)).ToArray());
static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
