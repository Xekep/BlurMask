using System.Numerics;

namespace InkRecognition;

/// <summary>
/// Private source-derived0202D0 temporary geometry foroperator13B6's
/// analysis. This does not execute the remaining13B6 cuts/range/bitset writes.
/// CRT atan2/cos/sin use explicit portable double semantics, not a bundled
/// CRT/SIMD/FTZ bit-identity promise. No caller-prepared geometry is accepted.
/// </summary>
internal sealed class LegacyMathAkOperator13B6Geometry
{
    private readonly LegacyMathAkProcessedStrokePrefix prefix;
    private readonly Vector2[]?[] polylines;
    private readonly int[]?[] selectedSourceIndices;
    internal LegacyMathAkProcessedStrokePrefix Prefix => prefix;
    internal bool HasProcessedGraphAuthority => false;
    private LegacyMathAkOperator13B6Geometry(LegacyMathAkProcessedStrokePrefix prefix,
        Vector2[]?[] polylines, int[]?[] indices)
    { this.prefix = prefix; this.polylines = polylines; selectedSourceIndices = indices; }

    internal static LegacyMathAkOperator13B6Geometry Prepare(
        LegacyMathAkProcessedStrokePrefix prefix, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(prefix); token.ThrowIfCancellationRequested();
        if (prefix.PointCount is < 1 or > LegacyMathAkProcessedStrokePrefix.MaximumExpandedPoints)
            throw new InvalidOperationException("Owned prefix cardinality invariant failed.");
        float spacing = prefix.EstimatedScale / 100f; //Actual0205B0 ->0202D0 arguments.
        if (!(spacing > 0f) || !float.IsFinite(spacing))
            throw new NotSupportedException("Stroke-break spacing outside positive finite subset.");
        Vector2[][] input = prefix.CopyPoints(token);
        if (input.Length != prefix.OriginalOwner.StrokeCount || input.Length is < 1 or > 64)
            throw new InvalidOperationException("Owned prefix stroke count invariant failed.");
        var indices = new int[]?[input.Length];
        var result = new Vector2[]?[input.Length];
        int sum = 0;
        for (int s = 0; s < input.Length; s++)
        {
            token.ThrowIfCancellationRequested();
            Vector2[] row = input[s];
            if (row.Length == 0 || row.Length > prefix.PointCount - sum)
                throw new InvalidOperationException("Owned prefix row invariant failed.");
            sum += row.Length;
            AnalyzeRowCore(row,spacing,prefix.EstimatedScale,token,out result[s],out indices[s]);
        }
        if (sum != prefix.PointCount) throw new InvalidOperationException("Owned prefix point count mismatch.");
        token.ThrowIfCancellationRequested(); return new(prefix, result, indices);
    }

    //Same arithmetic, called only from genuine owned-prefix row captures.
    private static void AnalyzeRowCore(ReadOnlySpan<Vector2> row,float spacing,float estimatedScale,
        CancellationToken token,out Vector2[]? polyline,out int[]? selectedIndices)
    {
        //Actual0205B0 bbox early return. Missing is distinct from an
        //empty backend. Fresh005C00 source138=.5; stored13B6 factor=.5.
        float minX = row[0].X, maxX = minX, minY = row[0].Y, maxY = minY;
        foreach (Vector2 p in row)
        {
            if (maxX <= p.X) maxX = p.X; if (maxY <= p.Y) maxY = p.Y;
            if (minX > p.X) minX = p.X; if (minY > p.Y) minY = p.Y;
        }
        float threshold = estimatedScale * 0.5f * 1f;
        if (MathF.Abs(maxX - minX) < threshold && MathF.Abs(maxY - minY) < threshold)
        { polyline=null; selectedIndices=null; return; }
        int[] selected = Reduce(row, spacing, token);
        selectedIndices = selected;
        if (selected.Length == 1) polyline = new[] { row[0] };
        else
        {
            var starts = new Vector2[selected.Length - 1];
            var ends = new Vector2[starts.Length];
            for (int i = 0; i < starts.Length; i++)
            {
                token.ThrowIfCancellationRequested();
                Fit(row, selected[i], selected[i + 1], out starts[i], out ends[i], token);
            }
            polyline = Join(starts, ends, token);
        }
        foreach (Vector2 p in polyline!)
            if (!float.IsFinite(p.X) || !float.IsFinite(p.Y))
                throw new NotSupportedException("Native operator13B6 geometry outside finite output subset.");
    }

    internal sealed class OperatorRow
    {
        private readonly LegacyMathAkProcessedStrokePrefix.OperatorSourceRow source;
        private readonly Vector2[]? polyline;
        private OperatorRow(LegacyMathAkProcessedStrokePrefix.OperatorSourceRow source,Vector2[]? polyline)
        {this.source=source;this.polyline=polyline;}
        internal bool WasSkipped => polyline is null;
        internal ReadOnlySpan<Vector2> OriginalPoints => source.Points;
        internal ReadOnlySpan<Vector2> FittedPoints => polyline;
        internal void Validate(LegacyMathAkProcessedStrokePrefix expected,int ordinal) => source.Validate(expected,ordinal);
        internal static OperatorRow Capture(LegacyMathAkProcessedStrokePrefix prefix,
            LegacyMathAkProcessedStrokePrefix.OperatorSourceRow source,int ordinal,CancellationToken token)
        {
            ArgumentNullException.ThrowIfNull(prefix);ArgumentNullException.ThrowIfNull(source);token.ThrowIfCancellationRequested();
            source.Validate(prefix,ordinal);
            if(prefix.PointCount is <1 or >LegacyMathAkProcessedStrokePrefix.MaximumExpandedPoints)
                throw new InvalidOperationException("Owned prefix cardinality invariant failed.");
            float spacing=prefix.EstimatedScale/100f;
            if(!(spacing>0)||!float.IsFinite(spacing))throw new NotSupportedException("Invalid operator spacing.");
            AnalyzeRowCore(source.Points,spacing,prefix.EstimatedScale,token,out var polyline,out _);
            token.ThrowIfCancellationRequested();return new OperatorRow(source,polyline);
        }
    }
    internal static OperatorRow AnalyzeForOperator(LegacyMathAkProcessedStrokePrefix prefix,
        LegacyMathAkProcessedStrokePrefix.OperatorSourceRow source,int ordinal,CancellationToken token)
        => OperatorRow.Capture(prefix,source,ordinal,token);

    private static int[] Reduce(ReadOnlySpan<Vector2> points, float spacing, CancellationToken token)
    {
        int n = points.Length;
        var previous = new int[n]; var next = new int[n]; var priority = new float[n];
        float endpoint = BitConverter.Int32BitsToSingle(0x7c23d70a);
        for (int i = 0; i < n; i++)
        {
            token.ThrowIfCancellationRequested(); previous[i] = i - 1; next[i] = i + 1 < n ? i + 1 : -1;
            priority[i] = i == 0 || i == n - 1 ? endpoint : Priority(points[i - 1], points[i + 1], points[i]);
        }
        float reciprocal = 1f / spacing;
        int head = 0, count = n;
        while (count > 2)
        {
            token.ThrowIfCancellationRequested();
            int selected = head; float best = float.MaxValue;
            for (int at = head; at >= 0; at = next[at])
                if (priority[at] < best) { selected = at; best = priority[at]; }
            float scaled = best * reciprocal;
            if (!(scaled <= 5f)) break; //Includes native NaN stop semantics.
            int a = previous[selected], b = next[selected];
            //Finite bounded source priorities are far below the endpoint
            //sentinel. An endpoint cannot be removed in the admitted subset.
            if (a < 0 || b < 0) throw new NotSupportedException("Unexpected endpoint simplification.");
            if (previous[a] >= 0) priority[a] = Priority(points[previous[a]], points[b], points[a]);
            if (next[b] >= 0) priority[b] = Priority(points[a], points[next[b]], points[b]);
            next[a] = b; previous[b] = a; count--;
        }
        var selectedIndices = new int[count]; int written = 0;
        for (int at = head; at >= 0; at = next[at]) selectedIndices[written++] = at;
        if (written != count) throw new InvalidOperationException("Priority-list count mismatch.");
        return selectedIndices;
    }

    private static float Priority(Vector2 a, Vector2 b, Vector2 p)
    {
        float ay = p.Y - a.Y, ax = p.X - a.X;
        float bx = p.X - b.X, by = p.Y - b.Y;
        float cross0 = ax * by, cross1 = ay * bx;
        float cross = cross0 - cross1;
        float dot0 = by * ay, dot1 = ax * bx;
        float dot = dot0 + dot1;
        float angle = (float)Math.Atan2((double)cross, (double)dot);
        const float halfPi = 1.5707963705062866f, pi = 3.1415927410125732f;
        float shifted = angle + halfPi;
        float folded = (float)((double)shifted % (double)pi) - halfPi;
        if (folded < -halfPi) folded += pi;
        float dy = a.Y - b.Y, dx = a.X - b.X;
        float denominator = dy * dy + dx * dx;
        float distanceSquared;
        if (denominator != 0f)
        {
            float ex = b.X - a.X, ey = b.Y - a.Y;
            float t = (ex * (p.X - a.X) + ey * (p.Y - a.Y)) / denominator;
            if (t >= 0f)
            {
                if (t <= 1f)
                { dx = (ex * t + a.X) - p.X; dy = (ey * t + a.Y) - p.Y; }
                else { dy = b.Y - p.Y; dx = b.X - p.X; }
                distanceSquared = dy * dy + dx * dx;
                return (float)Math.Sqrt((double)distanceSquared) * (MathF.Abs(folded) / halfPi + 0.25f);
            }
        }
        dy = a.Y - p.Y; dx = a.X - p.X;
        distanceSquared = dy * dy + dx * dx;
        return (float)Math.Sqrt((double)distanceSquared) * (MathF.Abs(folded) / halfPi + 0.25f);
    }

    private static void Fit(ReadOnlySpan<Vector2> row, int first, int last, out Vector2 a, out Vector2 b, CancellationToken token)
    {
        if (first < 0 || last <= first || last >= row.Length)
            throw new InvalidOperationException("Selected fit interval invariant failed.");
        if (last - first == 1) { a = row[first]; b = row[last]; return; }
        float mass = 0f, sx = 0f, sy = 0f, xx = 0f, xy = 0f, yy = 0f, previousHalf = 0f;
        for (int i = first; i <= last; i++)
        {
            if ((i & 255) == 0) token.ThrowIfCancellationRequested();
            float half;
            if (i < last)
            {
                float dx = row[i].X - row[i + 1].X, dy = row[i].Y - row[i + 1].Y;
                half = MathF.Sqrt(dy * dy + dx * dx) * 0.5f;
                float mx = (row[i + 1].X + row[i].X) * 0.5f;
                float my = (row[i + 1].Y + row[i].Y) * 0.5f;
                mass += half;
                float wx = mx * half, wy = my * half;
                sx += wx; sy += wy;
                xx += wx * mx; xy += wx * my; yy += wy * my;
            }
            else half = previousHalf;
            if (i == first) previousHalf = half;
            float weight = (half + previousHalf) * 0.5f;
            float vx = row[i].X * weight, vy = row[i].Y * weight;
            mass += weight; sx += vx; sy += vy;
            xx += vx * row[i].X; xy += vx * row[i].Y; yy += vy * row[i].Y;
            previousHalf = half;
        }
        if (!(mass > 0f) || !float.IsFinite(mass) || !float.IsFinite(sx) || !float.IsFinite(sy) ||
            !float.IsFinite(xx) || !float.IsFinite(xy) || !float.IsFinite(yy))
            throw new NotSupportedException("PCA moments outside finite positive-mass subset.");
        //The decompiler omits these second moments at the opaque CRT call.
        //Actual1FC9A..1FD0D pins their F32 ordering explicitly.
        float difference = xx - yy;
        float cx = sx / mass, cy = sy / mass;
        float covariance = xy * mass;
        float anisotropy = difference * mass;
        covariance -= sy * sx;
        float sy2 = sy * sy, sx2 = sx * sx;
        covariance += covariance;
        sy2 -= sx2;
        sy2 += anisotropy;
        if (!float.IsFinite(covariance) || !float.IsFinite(sy2))
            throw new NotSupportedException("PCA products outside finite subset.");
        float angle = (float)Math.Atan2((double)covariance, (double)sy2);
        float halfAngle = angle * 0.5f;
        float ux = (float)Math.Cos((double)halfAngle), uy = (float)Math.Sin((double)halfAngle);
        float minimum = float.MaxValue, maximum = -float.MaxValue;
        for (int i = first; i <= last; i++)
        {
            if ((i & 255) == 0) token.ThrowIfCancellationRequested();
            float projection = (row[i].Y - cy) * uy + (row[i].X - cx) * ux;
            if (projection > maximum) maximum = projection;
            if (projection < minimum) minimum = projection;
        }
        a = new(ux * minimum + cx, uy * minimum + cy);
        b = new(ux * maximum + cx, uy * maximum + cy);
        float ax = a.X - row[first].X, ay = a.Y - row[first].Y;
        float bx = a.X - row[last].X, by = a.Y - row[last].Y;
        if (by * by + bx * bx < ay * ay + ax * ax) (a, b) = (b, a);
    }

    private static float Length(Vector2 a, Vector2 b)
    { float dy = a.Y - b.Y, dx = a.X - b.X; return MathF.Sqrt(dy * dy + dx * dx); }
    private static Vector2[] Join(Vector2[] starts, Vector2[] ends, CancellationToken token)
    {
        var output = new Vector2[starts.Length + 1]; output[0] = starts[0];
        float previousLength = Length(starts[0], ends[0]);
        for (int i = 1; i < starts.Length; i++)
        {
            token.ThrowIfCancellationRequested();
            Vector2 s = starts[i], e = ends[i], ps = starts[i - 1], pe = ends[i - 1];
            float currentLength = Length(s, e);
            float rx = s.X - e.X, ry = s.Y - e.Y;
            float y = (pe.Y * previousLength + currentLength * s.Y) / (currentLength + previousLength);
            float x = (pe.X * previousLength + currentLength * s.X) / (currentLength + previousLength);
            float px = pe.X - ps.X, py = pe.Y - ps.Y;
            float determinant = py * rx - ry * px;
            if (determinant != 0f)
            {
                float dx = e.X - ps.X, dy = e.Y - ps.Y;
                float t = (dy * px - dx * py) / determinant;
                float u = (dy * rx - dx * ry) / determinant;
                if (t > 0f && u > 0f)
                {
                    float ix = rx * t + e.X, iy = ry * t + e.Y;
                    float gx = s.X - pe.X, gy = s.Y - pe.Y;
                    float gap = gy * gy + gx * gx;
                    float ey = iy - pe.Y, ex = ix - pe.X;
                    float cy = iy - s.Y, cx = ix - s.X;
                    float residual = ey * ey + ex * ex + cy * cy + cx * cx;
                    if (gap > 0f && residual / gap < 10f)
                    {
                        x = (ix * gap + x * residual) / (residual + gap);
                        y = (iy * gap + y * residual) / (residual + gap);
                    }
                }
            }
            output[i] = new(x, y); previousLength = currentLength;
        }
        output[^1] = ends[^1]; return output;
    }

    internal Vector2[]?[] CopyPolylines(CancellationToken token)
    {
        var copy = new Vector2[]?[polylines.Length];
        for (int s = 0; s < copy.Length; s++)
        { token.ThrowIfCancellationRequested(); copy[s] = polylines[s] is { } row ? (Vector2[])row.Clone() : null; }
        token.ThrowIfCancellationRequested(); return copy;
    }
    internal int[]?[] CopySelectedSourceIndices(CancellationToken token)
    {
        var copy = new int[]?[selectedSourceIndices.Length];
        for (int s = 0; s < copy.Length; s++)
        { token.ThrowIfCancellationRequested(); copy[s] = selectedSourceIndices[s] is { } row ? (int[])row.Clone() : null; }
        token.ThrowIfCancellationRequested(); return copy;
    }
}
