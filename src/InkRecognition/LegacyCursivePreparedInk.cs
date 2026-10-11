namespace InkRecognition;

/// <summary>One native297 prepared record with its original point-map entry.</summary>
public readonly record struct LegacyCursivePreparedPoint(float X, float Y, float DeltaX, float DeltaY,
    float SegmentLength, int Tag, int StrokeIndex, int OriginalPointIndex);

/// <summary>
/// Owned native191950 point preparation and18dde0 insertion. Inserted points
/// belong to prepared ink; their source map repeats the preceding raw index.
/// Complete candidate-location geometry is a separate producer.
/// </summary>
public sealed class LegacyCursivePreparedInk
{
    private readonly LegacyCursivePreparedPoint[] points;
    private readonly int[] counts;
    public ReadOnlyMemory<LegacyCursivePreparedPoint> Points => points;
    public ReadOnlyMemory<int> StrokePointCounts => counts;

    private LegacyCursivePreparedInk(LegacyCursivePreparedPoint[] points, int[] counts)
    {
        this.points = points;
        this.counts = counts;
    }

    /// <summary>
    /// Copy raw float coordinates, global raw-point identities and physical
    /// stroke identities. Compute forward deltas across the flattened buffer,
    /// including inter-stroke gaps; the final global record has zero deltas.
    /// Each physical stroke's first record is tagged1.
    /// </summary>
    public static LegacyCursivePreparedInk Create(IReadOnlyList<InkStroke> source,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(source);
        cancellationToken.ThrowIfCancellationRequested();
        if (source.Count is < 1 or > 256) throw new ArgumentException("Expected1..256 physical strokes.", nameof(source));
        var counts = new int[source.Count];
        var records = new List<LegacyCursivePreparedPoint>();
        for (int stroke = 0; stroke < source.Count; stroke++)
        {
            var ink = source[stroke] ?? throw new ArgumentException("Null physical stroke.", nameof(source));
            if (ink.Points.Count is < 1 or > 4096 || records.Count > 4096 - ink.Points.Count)
                throw new ArgumentException("Prepared ink requires1..4096 total points.", nameof(source));
            counts[stroke] = ink.Points.Count;
            for (int point = 0; point < ink.Points.Count; point++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var p = ink.Points[point];
                ValidatePoint(p);
                records.Add(new(p.X, p.Y, 0, 0, 0, point == 0 ? 1 : 0, stroke, records.Count));
            }
        }
        var points = records.ToArray();
        for (int i = 0; i < points.Length - 1; i++) points[i] = Forward(points[i], points[i+1]);
        return new(points, counts);
    }

    /// <summary>
    /// Insert an explicitly supplied cut point before a prepared local point.
    /// Location selection/interpolation belongs to the geometry caller. Native
    ///18dde0 recomputes only the preceding and inserted records; other prepared
    /// records remain unchanged. The original map copies the preceding entry.
    /// </summary>
    public LegacyCursivePreparedInk Insert(int strokeIndex, int beforePoint, InkPoint point, int tag,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if ((uint)strokeIndex >= (uint)counts.Length || beforePoint < 1 || beforePoint >= counts[strokeIndex])
            throw new ArgumentOutOfRangeException(nameof(beforePoint), "Insert requires two neighboring points within one physical stroke.");
        if (points.Length == 4096) throw new ArgumentException("Prepared ink exceeds4096 points.");
        ValidatePoint(point);
        int insertion = beforePoint;
        for (int stroke = 0; stroke < strokeIndex; stroke++) insertion += counts[stroke];
        var result = new LegacyCursivePreparedPoint[points.Length + 1];
        points.AsSpan(0, insertion).CopyTo(result);
        points.AsSpan(insertion).CopyTo(result.AsSpan(insertion+1));
        result[insertion] = new(point.X, point.Y, 0, 0, 0, tag, strokeIndex, points[insertion-1].OriginalPointIndex);
        result[insertion-1] = Forward(result[insertion-1], result[insertion]);
        result[insertion] = Forward(result[insertion], result[insertion+1]);
        var nextCounts = counts.ToArray(); nextCounts[strokeIndex]++;
        return new(result, nextCounts);
    }

    /// <summary>Replace only tags after an exact geometry pass, preserving ink and point ownership.</summary>
    public LegacyCursivePreparedInk WithTags(ReadOnlySpan<int> tags)
    {
        if(tags.Length != points.Length) throw new ArgumentException("Tags must match prepared points.", nameof(tags));
        var copy=points.ToArray();
        for(int i=0;i<copy.Length;i++)copy[i]=copy[i] with { Tag=tags[i] };
        return new(copy, counts.ToArray());
    }

    /// <summary>
    /// Return copied prepared float strokes. Native atom ranges refer to these
    /// coordinates/counts, which may differ from the original ink after inserts.
    /// </summary>
    public IReadOnlyList<InkStroke> ToStrokes()
    {
        var strokes = new InkStroke[counts.Length]; int offset = 0;
        for (int stroke = 0; stroke < counts.Length; stroke++)
        {
            var coordinates = new InkPoint[counts[stroke]];
            for (int i = 0; i < coordinates.Length; i++) coordinates[i] = new(points[offset+i].X, points[offset+i].Y);
            strokes[stroke] = new(Array.AsReadOnly(coordinates)); offset += coordinates.Length;
        }
        return Array.AsReadOnly(strokes);
    }

    private static LegacyCursivePreparedPoint Forward(LegacyCursivePreparedPoint current, LegacyCursivePreparedPoint next)
    {
        float dx = next.X-current.X, dy = next.Y-current.Y;
        if (!float.IsFinite(dx) || !float.IsFinite(dy)) throw new ArgumentException("Prepared segment deltas overflowed.");
        // Native25bab0(_hypotf) promotes already rounded float deltas to double,
        // squares/adds in double, takes sqrt and finally rounds back to float.
        double x = dx, y = dy;
        float length = (float)Math.Sqrt(y*y + x*x);
        if (!float.IsFinite(length)) throw new ArgumentException("Prepared segment length overflowed.");
        return current with { DeltaX=dx, DeltaY=dy, SegmentLength=length };
    }

    private static void ValidatePoint(InkPoint point)
    {
        if (!float.IsFinite(point.X) || !float.IsFinite(point.Y)) throw new ArgumentException("Prepared ink requires finite coordinates.");
    }
}
