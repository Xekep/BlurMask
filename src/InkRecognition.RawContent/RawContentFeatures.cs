namespace InkRecognition.RawContent;

internal sealed record RawContentGraph(float[] Nodes, long[] Edges, float[] EdgeFeatures)
{
    internal int EdgeCount => Edges.Length / 2;
}

internal static class RawContentFeatures
{
    private const double Epsilon = 1e-5;
    private sealed class StrokeGeometry
    {
        internal readonly InkPoint[] Points;
        internal readonly double Left, Top, Right, Bottom, Width, Height, Diagonal, Length, CenterX, CenterY, HullArea, Curvature, Radial;
        internal readonly SegmentTree Tree;
        internal StrokeGeometry(InkStroke stroke, CancellationToken token)
        {
            Points = stroke.Points.ToArray();
            Left = Points.Min(p => (double)p.X); Right = Points.Max(p => (double)p.X);
            Top = Points.Min(p => (double)p.Y); Bottom = Points.Max(p => (double)p.Y);
            Width = System.Math.Max(Right - Left, Epsilon); Height = System.Math.Max(Bottom - Top, Epsilon);
            Diagonal = Hypot(Width, Height);
            CenterX = Points.Average(p => (double)p.X); CenterY = Points.Average(p => (double)p.Y);
            double length = 0, cos = 0, sin = 0, turns = 0;
            for (int i = 0; i < Points.Length; i++)
            {
                if ((i & 255) == 0) token.ThrowIfCancellationRequested();
                if (i > 0) length += Distance(Points[i - 1], Points[i]);
                if (i > 1)
                {
                    double ax = (double)Points[i - 1].X - Points[i - 2].X, ay = (double)Points[i - 1].Y - Points[i - 2].Y;
                    double bx = (double)Points[i].X - Points[i - 1].X, by = (double)Points[i].Y - Points[i - 1].Y;
                    turns += System.Math.Abs(ApproxAngle(ax * bx + ay * by, bx * ay - by * ax));
                }
                var angle = ApproxAngle(CenterX - Points[i].X, CenterY - Points[i].Y) / System.Math.PI;
                cos += System.Math.Cos(angle); sin += System.Math.Sin(angle);
            }
            Length = length; Radial = Points.Length > 1 ? System.Math.Max(0, 1 - Hypot(cos / Points.Length, sin / Points.Length)) : 0;
            Curvature = turns / System.Math.PI; HullArea = PolygonArea(Hull(Points));
            Tree = new(Points);
        }
    }

    internal static RawContentGraph Create(IReadOnlyList<InkStroke> strokes, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(strokes); token.ThrowIfCancellationRequested();
        if (strokes.Count > 256 || strokes.Sum(s => (long)s.Points.Count) > 20000)
            throw new ArgumentException("Raw-content input exceeds 256 strokes or 20000 points.");
        foreach (var stroke in strokes)
            if (stroke.Points.Count == 0 || stroke.Points.Any(p => !float.IsFinite(p.X) || !float.IsFinite(p.Y) || System.Math.Abs(p.X) > 1000000 || System.Math.Abs(p.Y) > 1000000))
                throw new ArgumentException("Raw-content requires nonempty strokes with finite bounded coordinates.");
        if (strokes.Count == 0) return new([], [], []);
        var stats = strokes.Select(s => new StrokeGeometry(s, token)).ToArray();
        // ScalerDiagonal selects the upper median of nontrivial squared raw box
        // diagonals (not an average median), with a .1 lower bound.
        var diagonals = stats.Select(s => Hypot(s.Right - s.Left, s.Bottom - s.Top)).Where(d => d * d > .0001).Order().ToArray();
        var scale = diagonals.Length == 0 ? 1 : System.Math.Max(.1, diagonals[diagonals.Length / 2]);
        var nodes = new float[strokes.Count * 11];
        for (int i = 0; i < stats.Length; i++)
        {
            var s = stats[i]; var first = s.Points[0]; var last = s.Points[^1]; var end = Distance(first, last);
            double[] values = [111, s.Length / scale, s.HullArea / (scale * scale), s.Radial, end / System.Math.Max(s.Length, Epsilon),
                s.Width / scale, s.Height / scale, s.Length / s.Diagonal, System.Math.Abs(first.X - last.X) / s.Width,
                System.Math.Abs(first.Y - last.Y) / s.Height, end / s.Diagonal];
            nodes[i * 11] = 111;
            for (int j = 1; j < 11; j++) nodes[i * 11 + j] = Root(values[j]);
        }
        var pairs = new List<(int A, int B)>();
        for (int i = 0; i < stats.Length; i++)
        {
            var neighbors = Enumerable.Range(0, stats.Length).Where(j => j != i)
                .OrderBy(j => BoxDistance(stats[i], stats[j])).ThenBy(j => j).Take(20).ToHashSet();
            neighbors.Add(i);
            // Chronological edges are bidirectional, including when a stroke falls
            // outside the 20 spatial neighbors. The threshold is in normalized units.
            foreach (var j in new[] { i - 1, i + 1 })
                if (j >= 0 && j < stats.Length && BoxDistance(stats[i], stats[j]) <= 10 * scale) neighbors.Add(j);
            foreach (var j in neighbors.Order()) pairs.Add((i, j));
        }
        var edges = new long[pairs.Count * 2]; var features = new float[pairs.Count * 16];
        var distances = new Dictionary<(int, int), double>();
        for (int k = 0; k < pairs.Count; k++)
        {
            if ((k & 31) == 0) token.ThrowIfCancellationRequested();
            var (i, j) = pairs[k]; edges[k] = i; edges[k + pairs.Count] = j;
            if (i == j) continue;
            var a = stats[i]; var b = stats[j]; var key = (System.Math.Min(i, j), System.Math.Max(i, j));
            if (!distances.TryGetValue(key, out var distance))
            {
                var query = a.Points.Length <= b.Points.Length ? a : b; var target = query == a ? b : a;
                distance = target.Tree.DistanceTo(query.Points, token); distances[key] = distance;
            }
            double[] ends = [Distance(a.Points[0], b.Points[0]), Distance(a.Points[0], b.Points[^1]),
                Distance(a.Points[^1], b.Points[0]), Distance(a.Points[^1], b.Points[^1])];
            double[] values = [distance / scale, ends.Min() / scale, ends.Max() / scale,
                (a.Left + a.Right - b.Left - b.Right) * .5 / scale, (a.Top + a.Bottom - b.Top - b.Bottom) * .5 / scale,
                (a.CenterX - b.CenterX) / scale, (a.CenterY - b.CenterY) / scale,
                (a.Points[^1].X - b.Points[0].X) / scale, (a.Points[^1].Y - b.Points[0].Y) / scale,
                Difference(System.Math.Max((a.Right - a.Left) * (a.Bottom - a.Top), Epsilon * Epsilon), System.Math.Max((b.Right - b.Left) * (b.Bottom - b.Top), Epsilon * Epsilon)), Difference(a.Width, b.Width), Difference(a.Height, b.Height),
                Difference(a.Diagonal, b.Diagonal), Difference(System.Math.Max(a.Length, Epsilon), System.Math.Max(b.Length, Epsilon)),
                Difference(a.Curvature, b.Curvature), i < j ? 1 : -1];
            for (int f = 0; f < 16; f++) features[k * 16 + f] = Root(values[f]);
        }
        return new(nodes, edges, features);
    }
    private static float Root(double value) => (float)System.Math.CopySign(System.Math.Sqrt(System.Math.Abs(value)), value);
    private static double Difference(double a, double b) => System.Math.Abs(a + b) <= 1e-8 ? 0 : (a - b) / (a + b);
    private static double Hypot(double x, double y) => System.Math.Sqrt(x * x + y * y);
    private static double Distance(InkPoint a, InkPoint b) => Hypot((double)a.X - b.X, (double)a.Y - b.Y);
    private static double ApproxAngle(double x, double y)
    {
        if (x == 0) return y == 0 ? 0 : System.Math.CopySign(System.Math.PI / 2, y);
        if (System.Math.Abs(x) > System.Math.Abs(y))
        {
            var r = y / x; var angle = r * System.Math.PI / 4 + .273 * r * (1 - System.Math.Abs(r));
            return x < 0 ? angle + System.Math.CopySign(System.Math.PI, y) : angle;
        }
        var ratio = x / y;
        return System.Math.CopySign(System.Math.PI / 2, y) - (ratio * System.Math.PI / 4 + .273 * ratio * (1 - System.Math.Abs(ratio)));
    }
    private static double BoxDistance(StrokeGeometry a, StrokeGeometry b) => Hypot(
        System.Math.Max(0, System.Math.Max(a.Left - b.Right, b.Left - a.Right)),
        System.Math.Max(0, System.Math.Max(a.Top - b.Bottom, b.Top - a.Bottom)));
    private static double Cross(InkPoint o, InkPoint a, InkPoint b) => ((double)a.X - o.X) * ((double)b.Y - o.Y) - ((double)a.Y - o.Y) * ((double)b.X - o.X);
    private static double PolygonArea(IReadOnlyList<InkPoint> points)
    {
        double area = 0;
        for (int i = 1; i + 1 < points.Count; i++) area += Cross(points[0], points[i], points[i + 1]);
        return System.Math.Abs(area) * .5;
    }
    private static InkPoint[] Hull(InkPoint[] source)
    {
        var points = source.Distinct().OrderBy(p => p.X).ThenBy(p => p.Y).ToArray();
        if (points.Length < 3) return points;
        var hull = new List<InkPoint>();
        foreach (var p in points) { while (hull.Count >= 2 && Cross(hull[^2], hull[^1], p) <= 0) hull.RemoveAt(hull.Count - 1); hull.Add(p); }
        int lower = hull.Count;
        for (int i = points.Length - 2; i >= 0; i--) { while (hull.Count > lower && Cross(hull[^2], hull[^1], points[i]) <= 0) hull.RemoveAt(hull.Count - 1); hull.Add(points[i]); }
        hull.RemoveAt(hull.Count - 1); return hull.ToArray();
    }

    // The observed model uses zero for intersecting polylines, otherwise the
    // minimum sample-to-sample distance. Preserve that trained metric, using
    // bounding-box pruning instead of a quadratic temporary distance matrix.
    private sealed class SegmentTree
    {
        private readonly record struct Segment(InkPoint A, InkPoint B)
        {
            internal double Left => System.Math.Min(A.X, B.X); internal double Right => System.Math.Max(A.X, B.X);
            internal double Top => System.Math.Min(A.Y, B.Y); internal double Bottom => System.Math.Max(A.Y, B.Y);
        }
        private sealed class Node
        {
            internal required double Left, Right, Top, Bottom;
            internal Node? First, Second; internal Segment[]? Segments;
        }
        private readonly Node _root;
        internal SegmentTree(InkPoint[] points)
        {
            var segments = points.Length == 1 ? new[] { new Segment(points[0], points[0]) } :
                Enumerable.Range(1, points.Length - 1).Select(i => new Segment(points[i - 1], points[i])).ToArray();
            _root = Build(segments);
        }
        private static Node Build(Segment[] segments)
        {
            var node = new Node { Left = segments.Min(s => s.Left), Right = segments.Max(s => s.Right), Top = segments.Min(s => s.Top), Bottom = segments.Max(s => s.Bottom) };
            if (segments.Length <= 8) { node.Segments = segments; return node; }
            bool horizontal = node.Right - node.Left >= node.Bottom - node.Top;
            Array.Sort(segments, (a, b) => (horizontal ? a.Left + a.Right : a.Top + a.Bottom).CompareTo(horizontal ? b.Left + b.Right : b.Top + b.Bottom));
            node.First = Build(segments[..(segments.Length / 2)]); node.Second = Build(segments[(segments.Length / 2)..]); return node;
        }
        internal double DistanceTo(InkPoint[] points, CancellationToken token)
        {
            double best = double.PositiveInfinity;
            for (int i = 0; i < System.Math.Max(1, points.Length - 1) && best > 0; i++)
            {
                if ((i & 63) == 0) token.ThrowIfCancellationRequested();
                Query(_root, new(points[i], points[System.Math.Min(i + 1, points.Length - 1)]), ref best);
            }
            return System.Math.Sqrt(best);
        }
        private static double LowerBound(Node n, Segment s)
        {
            var dx = System.Math.Max(0, System.Math.Max(n.Left - s.Right, s.Left - n.Right));
            var dy = System.Math.Max(0, System.Math.Max(n.Top - s.Bottom, s.Top - n.Bottom)); return dx * dx + dy * dy;
        }
        private static void Query(Node n, Segment s, ref double best)
        {
            if (LowerBound(n, s) >= best) return;
            if (n.Segments is { } segments)
            {
                foreach (var other in segments) { best = System.Math.Min(best, SegmentDistance(s, other)); if (best == 0) return; }
                return;
            }
            var first = n.First!; var second = n.Second!;
            if (LowerBound(first, s) > LowerBound(second, s)) (first, second) = (second, first);
            Query(first, s, ref best); Query(second, s, ref best);
        }
        private static double PointDistance(InkPoint p, Segment s)
        {
            double dx = (double)s.B.X - s.A.X, dy = (double)s.B.Y - s.A.Y;
            var norm = dx * dx + dy * dy;
            var t = norm == 0 ? 0 : System.Math.Clamp((((double)p.X - s.A.X) * dx + ((double)p.Y - s.A.Y) * dy) / norm, 0, 1);
            dx = p.X - (s.A.X + t * dx); dy = p.Y - (s.A.Y + t * dy); return dx * dx + dy * dy;
        }
        private static double SegmentDistance(Segment a, Segment b)
        {
            var c1 = Cross(a.A, a.B, b.A); var c2 = Cross(a.A, a.B, b.B);
            var c3 = Cross(b.A, b.B, a.A); var c4 = Cross(b.A, b.B, a.B);
            if ((c1 > 0 && c2 < 0 || c1 < 0 && c2 > 0) && (c3 > 0 && c4 < 0 || c3 < 0 && c4 > 0)) return 0;
            // Collinear overlap and a shared endpoint also count as intersection.
            if ((c1 == 0 && PointDistance(b.A, a) == 0) || (c2 == 0 && PointDistance(b.B, a) == 0) ||
                (c3 == 0 && PointDistance(a.A, b) == 0) || (c4 == 0 && PointDistance(a.B, b) == 0)) return 0;
            double Squared(InkPoint p, InkPoint q) { double x = (double)p.X - q.X, y = (double)p.Y - q.Y; return x * x + y * y; }
            return System.Math.Min(System.Math.Min(Squared(a.A, b.A), Squared(a.A, b.B)), System.Math.Min(Squared(a.B, b.A), Squared(a.B, b.B)));
        }
    }
}
