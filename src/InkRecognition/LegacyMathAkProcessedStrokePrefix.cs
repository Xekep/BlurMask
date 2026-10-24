using System.Numerics;

namespace InkRecognition;

/// <summary>
/// Internal initial-source bootstrap and first-two-operator prefix of the stored first vector, from original
/// Float32 XY. This is neither completed processed-source180/188 nor a matcher
/// request. No external rectangles or incremental-prefix input are admitted.
/// </summary>
internal sealed class LegacyMathAkProcessedStrokePrefix
{
    private readonly LegacyMathAkSourceGeometry originalOwner;
    private readonly Vector2[][] points;
    private readonly int[][] originalPointIndices;
    private readonly int[] originalStrokeIndices;
    internal const int MaximumExpandedPoints = LegacyMathAkSourceGeometry.MaximumPoints +
        4 * LegacyMathAkSourceGeometry.MaximumStrokes;
    internal float EstimatedScale { get; }
    internal int ExpandedStrokeCount { get; }
    internal int PointCount { get; }
    internal LegacyMathAkSourceGeometry OriginalOwner => originalOwner;
    internal bool HasProcessedGraphAuthority => false;
    internal bool HasMatcherRequestAuthority => false;

    private LegacyMathAkProcessedStrokePrefix(LegacyMathAkSourceGeometry owner,
        Vector2[][] points, int[][] indices, int[] strokeIndices, float scale, int expanded, int count)
    {
        originalOwner = owner; this.points = points; originalPointIndices = indices;
        originalStrokeIndices = strokeIndices; EstimatedScale = scale;
        ExpandedStrokeCount = expanded; PointCount = count;
    }

    internal static LegacyMathAkProcessedStrokePrefix Prepare(
        LegacyMathAkSourceGeometry original, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(original);
        token.ThrowIfCancellationRequested();
        //The sole input is a genuine geometry owner with its authenticated shared
        //profile. Copy ORIGINAL float input, never normalized short coordinates.
        Vector2[][] rows = original.CopyOriginalStrokes(token);
        if (rows.Length is < 1 or > LegacyMathAkSourceGeometry.MaximumStrokes)
            throw new InvalidOperationException("Original source stroke invariant failed.");
        int originalCount = 0;
        foreach (Vector2[] row in rows)
        {
            if (row.Length == 0 || row.Length > LegacyMathAkSourceGeometry.MaximumPoints - originalCount)
                throw new InvalidOperationException("Original source point invariant failed.");
            originalCount += row.Length;
        }
        if (originalCount != original.PointCount)
            throw new InvalidOperationException("Original source count invariant failed.");
        //009500 ->006A40 builds the source before the first operator. Near
        //duplicate decisions compare ADJACENT ORIGINAL points, not last kept.
        var indices = new int[rows.Length][];
        int originalBase = 0, currentCount = 0;
        for (int s = 0; s < rows.Length; s++)
        {
            token.ThrowIfCancellationRequested();
            Vector2[] raw = rows[s];
            int kept = CountInitialPoints(raw);
            var current = new Vector2[kept];
            var map = new int[kept];
            int written = 0;
            for (int i = 0; i < raw.Length; i++)
            {
                if (i == 0 || !NearOriginal(raw[i - 1], raw[i]))
                { current[written] = raw[i]; map[written++] = originalBase + i; }
                else if (i == raw.Length - 1)
                {
                    if (written == 1)
                    { current[written] = raw[i]; map[written++] = originalBase + i; }
                    else map[written - 1] = originalBase + i;
                }
            }
            if (written != kept) throw new InvalidOperationException("Initial source count invariant failed.");
            rows[s] = current; indices[s] = map;
            originalBase += raw.Length; currentCount += kept;
        }
        float scale = Estimate139D(rows, token);
        float delta = scale * 0.05f; //Stored139F/0100 Float32, not caller configuration.
        if (!float.IsFinite(delta) || delta < 0f)
            throw new NotSupportedException("139F delta outside the finite prefix subset.");
        var strokeIndices = new int[rows.Length];
        int resultCount = currentCount, expanded = 0;
        for (int s = 0; s < rows.Length; s++)
        {
            token.ThrowIfCancellationRequested();
            Vector2[] row = rows[s];
            strokeIndices[s] = s;
            if (row.Length == 1)
            {
                Vector2 p = row[0];
                float x = p.X + delta, y = p.Y + delta;
                //Literal01ED20 conjunction: BOTH coordinates must change.
                if (x != p.X && y != p.Y)
                {
                    if (!float.IsFinite(x) || !float.IsFinite(y) ||
                        resultCount > MaximumExpandedPoints - 4)
                        throw new NotSupportedException("139F expansion bound exceeded.");
                    rows[s] = new Vector2[] { p, new(p.X, y), new(x, y), new(x, p.Y), p };
                    int origin = indices[s][0];
                    indices[s] = new int[] { origin, origin, origin, origin, origin };
                    resultCount += 4; expanded++;
                }
            }

        }
        token.ThrowIfCancellationRequested();
        return new(original, rows, indices, strokeIndices, scale, expanded, resultCount);
    }

    private static int CountInitialPoints(Vector2[] row)
    {
        int kept = 0;
        for (int i = 0; i < row.Length; i++)
        {
            if (i == 0 || !NearOriginal(row[i - 1], row[i])) kept++;
            else if (i == row.Length - 1 && kept == 1) kept++;
        }
        return kept;
    }
    private static bool NearOriginal(Vector2 previous, Vector2 point)
    {
        //Exact006A40 F32 constants: relative2^-22, absolute2^-23.
        float x = MathF.Abs(previous.X), px = MathF.Abs(point.X);
        if (x <= px) x = px;
        float tolerance = x * 2.384185791015625e-7f;
        if (tolerance <= 1.1920928955078125e-7f) tolerance = 1.1920928955078125e-7f;
        if (tolerance < MathF.Abs(point.X - previous.X)) return false;
        float y = MathF.Abs(previous.Y), py = MathF.Abs(point.Y);
        if (y <= py) y = py;
        tolerance = y * 2.384185791015625e-7f;
        if (tolerance <= 1.1920928955078125e-7f) tolerance = 1.1920928955078125e-7f;
        return !(tolerance < MathF.Abs(point.Y - previous.Y));
    }

    private static float Estimate139D(Vector2[][] rows, CancellationToken token)
    {
        //Stored139D0104: q0=.5,q1=.7,window2,lo=.1,hi=1.1,
        //meanBlend2,finalMultiplier1.4. Rectangles are absent by this input type.
        var bounds = new Bounds[rows.Length];
        var values = new float[LegacyMathAkSourceGeometry.MaximumPoints];
        int count = 0;
        float total = 0f, nearestScale = 0f;
        for (int s = 0; s < rows.Length; s++)
        { token.ThrowIfCancellationRequested(); bounds[s] = GetBounds(rows[s]); }
        if (rows.Length < 5)
        {
            for (int s = 0; s < rows.Length; s++)
            {
                float best = float.MaxValue;
                for (int j = 0; j < rows.Length; j++)
                {
                    float dx0 = bounds[s].MinX - bounds[j].MaxX;
                    if (dx0 <= 0f) dx0 = 0f;
                    float dx1 = bounds[j].MinX - bounds[s].MaxX;
                    float dx = dx1 <= dx0 ? dx0 : dx1;
                    float dy0 = bounds[s].MinY - bounds[j].MaxY;
                    if (dy0 <= 0f) dy0 = 0f;
                    float dy1 = bounds[j].MinY - bounds[s].MaxY;
                    float dy = dy1 <= dy0 ? dy0 : dy1;
                    float distance = dx + dy;
                    if (distance > 0f && distance <= best) best = distance;
                }
                if (best < float.MaxValue) { values[count++] = best; total += best; }
            }
            if (count != 0) nearestScale = QuantileMean(values, count, total, 0.5f);
        }
        count = 0; total = 0f;
        foreach (Bounds b in bounds)
        {
            float extent = b.MaxY - b.MinY, width = b.MaxX - b.MinX;
            if (extent <= width) extent = width;
            if (extent > 0f) { values[count++] = extent; total += extent; }
        }
        float scale = count == 0 ? 1f : QuantileMean(values, count, total, 0.5f);
        count = 0; total = 0f;
        foreach (Vector2[] row in rows)
        {
            token.ThrowIfCancellationRequested();
            if (row.Length < 2) continue;
            //045A30 picks first/end and Y-direction reversals. Flat Y segments
            //do not replace the previous nonzero direction or its X endpoint.
            var turns = new Vector2[row.Length];
            int turnCount = 1;
            turns[0] = row[0];
            float priorX = row[1].X, priorDY = row[1].Y - row[0].Y;
            for (int i = 2; i < row.Length; i++)
            {
                float previousY = row[i - 1].Y;
                float dy = row[i].Y - previousY;
                if ((priorDY < 0f && dy > 0f) || (priorDY > 0f && dy < 0f))
                {
                    float x = (priorX + row[i - 1].X) * 0.5f;
                    turns[turnCount++] = new(x, previousY);
                }
                if (dy != 0f) { priorX = row[i].X; priorDY = dy; }
            }
            turns[turnCount++] = row[^1];
            for (int start = 0; start < turnCount; start += 2)
            {
                Bounds b = GetBounds(turns.AsSpan(start, Math.Min(3, turnCount - start)));
                float extent = b.MaxY - b.MinY, width = b.MaxX - b.MinX;
                if (extent <= width) extent = width;
                float lower = scale * 0.1f, upper = scale * 1.1f;
                if (lower < extent && extent < upper)
                {
                    if (count == values.Length)
                        throw new InvalidOperationException("139D refinement cardinality exceeded.");
                    values[count++] = extent; total += extent;
                }
            }
        }
        if (count != 0) scale = QuantileMean(values, count, total, 0.7f) * 1.4f;
        float twiceNearest = nearestScale + nearestScale;
        if (twiceNearest <= scale) twiceNearest = scale;
        if (!float.IsFinite(twiceNearest) || twiceNearest <= 0f)
            throw new NotSupportedException("139D scale outside the finite positive subset.");
        return twiceNearest;
    }

    private static float QuantileMean(float[] values, int count, float sum, float quantile)
    {
        //Engine context+A8 -> service0063 -> V58 093350 -> 0937F0,
        //ascending finite-float sort. All admitted values here are positive.
        Array.Sort(values, 0, count);
        float rank = (count - 1) * quantile;
        rank = rank <= 0f ? rank - 0.5f : rank + 0.5f;
        int at = (int)rank;
        if ((uint)at >= (uint)count) throw new InvalidOperationException("139D quantile index invalid.");
        float denominator = (2f - 1f) + count;
        float blend = 2f / denominator;
        float oneMinus = 1f - blend;
        float a = oneMinus * values[at], b = blend * (sum / count);
        return a + b;
    }
    private readonly record struct Bounds(float MinX, float MinY, float MaxX, float MaxY);
    private static Bounds GetBounds(ReadOnlySpan<Vector2> row)
    {
        if (row.IsEmpty) throw new InvalidOperationException("Empty prefix row.");
        float minX = row[0].X, maxX = minX, minY = row[0].Y, maxY = minY;
        for (int i = 1; i < row.Length; i++)
        {
            float x = row[i].X, y = row[i].Y;
            if (maxX <= x) maxX = x;
            if (maxY <= y) maxY = y;
            if (minX > x) minX = x;
            if (minY > y) minY = y;
        }
        return new(minX, minY, maxX, maxY);
    }
    //Detached diagnostics, not processed source/request ingress.
    internal Vector2[][] CopyPoints(CancellationToken token) => Copy(points, token);
    internal int[][] CopyOriginalPointIndices(CancellationToken token) => Copy(originalPointIndices, token);
    internal int[] CopyOriginalStrokeIndices(CancellationToken token)
    { token.ThrowIfCancellationRequested(); return (int[])originalStrokeIndices.Clone(); }
    private static T[][] Copy<T>(T[][] rows, CancellationToken token)
    {
        var copy = new T[rows.Length][];
        for (int i = 0; i < rows.Length; i++)
        { token.ThrowIfCancellationRequested(); copy[i] = (T[])rows[i].Clone(); }
        token.ThrowIfCancellationRequested(); return copy;
    }
}
