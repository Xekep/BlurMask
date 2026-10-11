namespace InkRecognition;

/// <summary>Owned legacy intersection flags and nonoverlapping self-loop count.</summary>
public sealed class LegacyDirectionalIntersectionSet
{
    public ReadOnlyMemory<byte> Flags { get; }
    public int SelfLoopCount { get; }
    internal LegacyDirectionalIntersectionSet(byte[] flags, int loops) { Flags = flags; SelfLoopCount = loops; }
}

/// <summary>
/// Checked spatial-grid intersection pass (0b6270) on prepared directional
/// sample points. Flags1 mark self-loop interiors,2 self-crossing endpoints,
/// and3 cross-stroke endpoints. Native lookup order, nearest vertices, angular
/// filtering and first-write behavior are preserved. This does not find cuts
/// between characters or assemble/evaluate the complete313 network input.
/// </summary>
public static class LegacyDirectionalIntersections
{
    private readonly record struct Segment(int Stroke, int LocalStart, int GlobalStart,
        LegacyInkPoint Start, LegacyInkPoint End, int Next);

    public static LegacyDirectionalIntersectionSet Extract(LegacyDirectionalSamples samples,
        int gridScale = 10, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(samples);
        return Extract(samples.Strokes, samples.Width, samples.Height, samples.SourceStepFixed, gridScale, cancellationToken);
    }

    public static LegacyDirectionalIntersectionSet Extract(IReadOnlyList<LegacyInkStroke> preparedStrokes,
        int geometryWidth, int geometryHeight, int sourceStepFixed, int gridScale = 10,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preparedStrokes);
        if (preparedStrokes.Count is < 1 or > 15 || geometryWidth is < 1 or > 32768 || geometryHeight is < 1 or > 32768)
            throw new ArgumentException("Expected 1..15 prepared strokes and positive signed-short geometry dimensions.");
        if (gridScale is < 1 or > 1024) throw new ArgumentOutOfRangeException(nameof(gridScale));
        cancellationToken.ThrowIfCancellationRequested();
        int pointCount = 0;
        foreach (var stroke in preparedStrokes)
        {
            ArgumentNullException.ThrowIfNull(stroke); pointCount += stroke.Points.Length;
            if (pointCount > 255) throw new ArgumentException("Expected at most 255 prepared sample points.", nameof(preparedStrokes));
            foreach (var p in stroke.Points)
                if (p.X < 0 || p.Y < 0 || p.X > geometryWidth || p.Y > geometryHeight)
                    throw new ArgumentException("Coordinates must be prepared nonnegative points within the supplied geometry dimensions.", nameof(preparedStrokes));
        }
        var flags = new byte[pointCount]; int step = sourceStepFixed;
        if (step < 0) throw new ArgumentOutOfRangeException(nameof(sourceStepFixed));
        if (step == 0) return new(flags, 0);
        if ((long)step * gridScale + 512 > int.MaxValue || step > int.MaxValue / 2)
            throw new ArgumentException("Prepared stride exceeds the checked grid arithmetic domain.", nameof(sourceStepFixed));
        int cellSize = Math.Max(1, (step * gridScale + 512) >> 10);
        int columns = Math.Min(100, geometryWidth / cellSize + 4);
        int rows = Math.Min(100, geometryHeight / cellSize + 4);
        var heads = new int[columns * rows]; Array.Fill(heads, -1);
        var segments = new List<Segment>(pointCount); int global = 0, loops = 0;
        for (int stroke = 0; stroke < preparedStrokes.Count; stroke++)
        {
            var points = preparedStrokes[stroke].Points;
            for (int local = 0; local < points.Length - 1; local++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var start = points[local]; var end = points[local + 1];
                int queryX = Math.Clamp(start.X / cellSize + 1, 0, columns - 2);
                int queryY = Math.Clamp(start.Y / cellSize + 1, 0, rows - 2);
                for (int neighbor = 0; neighbor < 4; neighbor++)
                {
                    int x = queryX + (neighbor & 1), y = queryY + (neighbor >> 1);
                    for (int node = heads[x * rows + y]; node >= 0; node = segments[node].Next)
                    {
                        var prior = segments[node];
                        if (prior.Stroke == stroke && prior.LocalStart == local - 1) continue;
                        int distance = LegacyDirectionalCoordinatePreparation.Distance(start, prior.Start);
                        if ((distance << 10) >= step * 2) continue;
                        if (!LegacyQuantizedSegmentIntersection.TryIntersect(start, end, prior.Start, prior.End, out var cross)) continue;
                        int ax = end.X - start.X, ay = end.Y - start.Y;
                        int bx = prior.End.X - prior.Start.X, by = prior.End.Y - prior.Start.Y;
                        int signedCross = unchecked(by * ax - bx * ay);
                        int magnitude = signedCross < 0 ? -signedCross : signedCross;
                        int aLength = LegacyDirectionalCoordinatePreparation.Distance(start, end);
                        int bLength = LegacyDirectionalCoordinatePreparation.Distance(prior.Start, prior.End);
                        if (unchecked(aLength * bLength * 1024) >= unchecked(magnitude * 3413)) continue;
                        int currentVertex = global + local;
                        if (SquareDistance(cross, end) < SquareDistance(cross, start)) currentVertex++;
                        int priorVertex = prior.GlobalStart;
                        if (SquareDistance(cross, prior.End) < SquareDistance(cross, prior.Start)) priorVertex++;
                        byte classification = prior.Stroke == stroke ? (byte)2 : (byte)3;
                        if (flags[currentVertex] == 0) flags[currentVertex] = classification;
                        if (flags[priorVertex] == 0) flags[priorVertex] = classification;
                        if (prior.Stroke != stroke) continue;
                        bool overlap = false;
                        for (int i = priorVertex + 1; i < currentVertex; i++)
                            if (flags[i] == 1) { overlap = true; break; }
                        if (overlap) continue;
                        for (int i = priorVertex + 1; i < currentVertex; i++)
                            if (flags[i] == 0) flags[i] = 1;
                        loops++;
                    }
                }
                int storeX = Math.Clamp(start.X / cellSize + 1, 0, columns - 1);
                int storeY = Math.Clamp(start.Y / cellSize + 1, 0, rows - 1);
                int bin = storeX * rows + storeY;
                segments.Add(new(stroke, local, global + local, start, end, heads[bin]));
                heads[bin] = segments.Count - 1;
            }
            global += points.Length;
        }
        return new(flags, loops);
    }

    private static int SquareDistance(LegacyIntersectionPoint cross, LegacyInkPoint point)
    {
        int x = unchecked(cross.X - point.X), y = unchecked(cross.Y - point.Y);
        return unchecked(x * x + y * y);
    }
}
