using Avalonia;
using Avalonia.Media;
using BlurMask.Shared;

namespace BlurMask;

internal static class StrokeGeometryChecks
{
    internal static void Run()
    {
        var points = new[] { new Point(0, 0), new Point(100, 50), new Point(200, 0) };
        var spans = InkStrokeGeometry.Segments(points).ToArray();
        var curve = spans.Single(s => s.Curve);
        Check(curve.Start == new Point(50, 25) && curve.Control == points[1] && curve.End == new Point(150, 25),
            "Quadratic smoothing remained limited by pen width.");
        // Independent quadratic Bernstein weights at t=1/2 give (100,37.5).
        var onCurve = new Point(100, 37.5);
        foreach (var tool in new[] { InkTool.Pen, InkTool.Highlighter })
        {
            var mark = new InkMark(tool, Colors.Red, 1, points[0]);
            foreach (var point in points.Skip(1)) mark.AddPoint(point);
            Check(mark.Hit(onCurve, .1) && !mark.Hit(points[1], .1), "Selection/eraser hit the old polygon rather than the visible curve.");
            var transformed = mark.WithTransform(Matrix.CreateRotation(.7) * Matrix.CreateTranslation(200, 100));
            Check(transformed.Hit(onCurve.Transform(transformed.Transform), .1), "Transformed curve hit test failed.");
            var wire = InkDocument.ExportMark(transformed);
            var peer = new InkDocument(); peer.Import([wire]);
            Check(peer.Marks[0].Same(transformed) && peer.Marks[0].Hit(onCurve.Transform(transformed.Transform), .1),
                "Shared curve changed original samples or transformation.");
            Check(mark.Points.SequenceEqual(points), "Smoothing rewrote AI/wire samples.");
        }
        var unequal = InkStrokeGeometry.Segments([new(0, 0), new(100, 50), new(120, 50), new(220, 100)]).ToArray();
        for (var i = 1; i < unequal.Length; i++)
        {
            Check(unequal[i - 1].End == unequal[i].Start, "Uneven samples introduced a gap.");
            var incoming = unequal[i - 1].End - (unequal[i - 1].Curve ? unequal[i - 1].Control : unequal[i - 1].Start);
            var outgoing = (unequal[i].Curve ? unequal[i].Control : unequal[i].End) - unequal[i].Start;
            Check(Math.Abs(incoming.X * outgoing.Y - incoming.Y * outgoing.X) < .00001 && Vector.Dot(incoming, outgoing) > 0,
                "Smooth spans do not share tangent directions.");
        }
        var corner = InkStrokeGeometry.Segments([new(0, 0), new(100, 100), new(200, 0)]).ToArray();
        Check(corner.All(s => !s.Curve) && corner[0].End == new Point(100, 100), "Sharp intentional corner was rounded.");
        Check(!InkStrokeGeometry.Segments([new(0, 0)]).Any() &&
              InkStrokeGeometry.Segments([new(0, 0), new(0, 0), new(10, 10)]).All(s => double.IsFinite(s.End.X + s.End.Y)),
            "Dot/duplicate samples generated an invalid curve.");
        Console.WriteLine("Stroke curves: quadratic goldens, tangent continuity, sharp corners, curved hit testing and unchanged peer/AI samples passed.");
    }
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
