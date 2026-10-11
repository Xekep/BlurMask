namespace InkRecognition;

/// <summary>
/// The checked 16 local-window integer statistics (0b7f60), before channel
/// transformation/calibration. Memberships refer to prepared physical points;
/// windows are not interpreted as character boundaries. Directional and
/// intersection flags must be supplied by their separate restored passes.
/// </summary>
public static class LegacyDirectionalWindowStatistics
{
    public const int Width = 16;

    public static int[] Extract(LegacyDirectionalSamples samples, ReadOnlySpan<byte> pointIndices,
        ReadOnlySpan<byte> directionFlags, ReadOnlySpan<byte> intersectionFlags,
        bool includeCoordinateUnit = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(samples);
        return Extract(samples.Strokes, samples.Width, samples.Height, samples.ScaleShift,
            pointIndices, directionFlags, intersectionFlags, includeCoordinateUnit, cancellationToken);
    }

    public static int[] Extract(IReadOnlyList<LegacyInkStroke> preparedStrokes,
        int geometryWidth, int geometryHeight, int scaleShift, ReadOnlySpan<byte> pointIndices,
        ReadOnlySpan<byte> directionFlags, ReadOnlySpan<byte> intersectionFlags,
        bool includeCoordinateUnit = true, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preparedStrokes);
        if (preparedStrokes.Count is < 1 or > 15 || geometryWidth is < 1 or > 32768 || geometryHeight is < 1 or > 32768)
            throw new ArgumentException("Expected 1..15 prepared strokes and positive signed-short geometry dimensions.");
        if (scaleShift is < -16 or > 14) throw new ArgumentOutOfRangeException(nameof(scaleShift));
        if (pointIndices.Length > 255) throw new ArgumentException("A byte-counted window exceeds 255 points.", nameof(pointIndices));
        cancellationToken.ThrowIfCancellationRequested();
        int count = 0;
        foreach (var stroke in preparedStrokes)
        {
            ArgumentNullException.ThrowIfNull(stroke); count += stroke.Points.Length;
            if (count > 255) throw new ArgumentException("Prepared points exceed the checked byte-indexed domain.", nameof(preparedStrokes));
        }
        if (directionFlags.Length != count || intersectionFlags.Length != count)
            throw new ArgumentException("Both flag arrays must match the prepared point count.");
        var points = new LegacyInkPoint[count]; int offset = 0;
        foreach (var stroke in preparedStrokes)
        {
            stroke.Points.CopyTo(points.AsSpan(offset)); offset += stroke.Points.Length;
        }
        for (int i = 0; i < count; i++)
        {
            if (points[i].X < 0 || points[i].Y < 0 || points[i].X > geometryWidth || points[i].Y > geometryHeight ||
                (directionFlags[i] & ~0xdb) != 0 || intersectionFlags[i] > 3)
                throw new ArgumentException("Unsupported prepared coordinates or flag values.");
        }
        var result = new int[Width]; if (pointIndices.IsEmpty) return result;
        int sumX = 0, sumY = 0, minX = int.MaxValue, minY = int.MaxValue, maxX = 0, maxY = 0;
        foreach (byte index in pointIndices)
        {
            if (index >= count) throw new ArgumentException("Window membership refers to an unavailable point.", nameof(pointIndices));
            var point = points[index]; sumX += point.X; sumY += point.Y;
            minX = Math.Min(minX, point.X); minY = Math.Min(minY, point.Y);
            maxX = Math.Max(maxX, point.X); maxY = Math.Max(maxY, point.Y);
            byte direction = directionFlags[index], intersection = intersectionFlags[index];
            switch (direction & 7) { case 1: result[10]++; break; case 2: result[9]++; break; }
            switch ((direction >> 3) & 7) { case 1: result[12]++; break; case 2: result[11]++; break; }
            switch (intersection) { case 1: result[15]++; break; case 2: result[13]++; break; case 3: result[14]++; break; }
            int endpoint = direction >> 6;
            if (endpoint is 1 or 3) result[4]++;
            if (endpoint is 2 or 3) result[5]++;
        }
        int meanX = sumX / pointIndices.Length, meanY = sumY / pointIndices.Length;
        // Native local bbox unit differs from post-resampling geometry: a
        // negative shift right-shifts1 to0 here, whereas whole geometry adds1.
        int unit = includeCoordinateUnit && scaleShift >= 0 ? 1 << scaleShift : 0;
        result[0] = (meanX << 10) / geometryWidth;
        result[1] = (meanY << 10) / geometryHeight;
        result[2] = ((maxX - minX + unit) << 10) / geometryWidth;
        result[3] = ((maxY - minY + unit) << 10) / geometryHeight;
        result[15] = (result[15] << 10) / count;
        int xx = 0, yy = 0, xy = 0, shift = 0;
        foreach (byte index in pointIndices)
        {
            int x = points[index].X - meanX, y = points[index].Y - meanY;
            xx += (x * x) >> shift; yy += (y * y) >> shift; xy += (x * y) >> shift;
            while (xx >= 0x200000 || yy >= 0x200000) { xx >>= 1; yy >>= 1; xy >>= 1; shift++; }
        }
        int variance = xx + yy;
        if (variance > 0) { result[6] = (xx << 10) / variance; result[7] = (yy << 10) / variance; result[8] = (xy << 10) / variance; }
        return result;
    }
}
