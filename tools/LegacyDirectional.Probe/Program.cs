using InkRecognition;
using System.Text.Json;

if (args.Length > 1) throw new ArgumentException("Supply at most one independently generated control JSON.");
if (args.Length == 1)
{
    if (new FileInfo(args[0]).Length > 4 * 1024 * 1024) throw new ArgumentException("Control file exceeds 4 MiB.");
    using var document = JsonDocument.Parse(File.ReadAllBytes(args[0]));
    int cases = 0;
    foreach (var item in document.RootElement.EnumerateArray())
    {
        var strokes = item.GetProperty("strokes").EnumerateArray().Select(s => new LegacyInkStroke(
            s.EnumerateArray().Select(p => new LegacyInkPoint(p[0].GetInt16(), p[1].GetInt16())).ToArray())).ToArray();
        var actual = LegacyDirectionalLandmarks.Extract(strokes);
        Require(actual.Flags.Span.SequenceEqual(item.GetProperty("flags").EnumerateArray().Select(p => p.GetByte()).ToArray()),
            $"Independent flags mismatch in case {cases}.");
        Require(actual.StrokeEnds.Span.SequenceEqual(item.GetProperty("ends").EnumerateArray().Select(p => p.GetInt32()).ToArray()),
            $"Independent stroke boundaries mismatch in case {cases}.");
        cases++;
    }
    Require(cases > 0, "No reference cases were supplied.");
    Console.WriteLine($"PASS {cases} independent directional reference comparisons. Vendor engine was not executed.");
}

static LegacyInkStroke Stroke(params LegacyInkPoint[] points) => new(points);
var vertical = LegacyDirectionalLandmarks.Extract([Stroke(new(0, 0), new(0, 10))]);
Require(vertical.Flags.Span.SequenceEqual(new byte[] { 0x50, 0x88 }), "Vertical boundary/minimum/maximum flags mismatch.");
var turn = LegacyDirectionalLandmarks.Extract([Stroke(new(0, 0), new(20, 10), new(0, 20))]);
Require(turn.Flags.Span.SequenceEqual(new byte[] { 0x51, 0x02, 0x89 }), "Horizontal reversal was not marked at the preceding vertex.");
var earlyYTurn = LegacyDirectionalLandmarks.Extract([Stroke(new(0, 0), new(10, 20), new(20, 10))]);
Require(earlyYTurn.Flags.Span.SequenceEqual(new byte[] { 0x51, 0, 0x8a }),
    "Native first-Y carry quirk must remain distinct from a generic local-extrema pass.");
var plateau = LegacyDirectionalLandmarks.Extract([Stroke(new(0, 0), new(10, 0), new(10, 0), new(0, 0))]);
Require(plateau.Flags.Span.SequenceEqual(new byte[] { 0x41, 0, 2, 0x81 }), "Flat run must defer reversal until its final vertex.");
var single = LegacyDirectionalLandmarks.Extract([Stroke(new LegacyInkPoint(5, 7))]);
Require(single.Flags.Span[0] == 0xc0, "Single-point stroke lost its two boundaries.");
var separated = LegacyDirectionalLandmarks.Extract([Stroke(new(0, 0), new(10, 10)), Stroke(new(10, 10), new(0, 0))]);
Require(separated.StrokeEnds.Span.SequenceEqual(new int[] { 2, 4 }) &&
    separated.Flags.Span.SequenceEqual(new byte[] { 0x51, 0x8a, 0x4a, 0x91 }), "Pen-up must reset direction and preserve physical endpoints.");
var transformed = LegacyDirectionalLandmarks.Extract([Stroke(new(-100, 300), new(-40, 330), new(-100, 360))]);
Require(transformed.Flags.Span.SequenceEqual(turn.Flags.Span), "Translation and positive scaling changed turn flags.");
using var canceled = new CancellationTokenSource(); canceled.Cancel();
try { LegacyDirectionalLandmarks.Extract([Stroke(new LegacyInkPoint(0, 0))], canceled.Token); throw new InvalidOperationException("Cancellation ignored."); }
catch (OperationCanceledException) { }
try { LegacyDirectionalLandmarks.Extract([]); throw new InvalidOperationException("Empty landmarks accepted."); }
catch (ArgumentException) { }
try { LegacyDirectionalLandmarks.Extract(Enumerable.Repeat(Stroke(new LegacyInkPoint(0, 0)), 16).ToArray()); throw new InvalidOperationException("Unbounded stroke count accepted."); }
catch (ArgumentException) { }
try { LegacyDirectionalLandmarks.Extract([new LegacyInkStroke(new LegacyInkPoint[4096]), Stroke(new LegacyInkPoint(0, 0))]); throw new InvalidOperationException("Unbounded point count accepted."); }
catch (ArgumentException) { }
Console.WriteLine("PASS own directional minima/maxima, native first-Y carry, plateau, physical boundaries, affine invariance and bounded-input checks. Character cuts and 313 network features are not tested.");
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
