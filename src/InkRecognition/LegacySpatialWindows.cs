namespace InkRecognition;

/// <summary>
/// Checked spatial memberships from0b7ce0 (uniform extents) and0b7840
/// (coordinate quantiles). Caller supplies prepared physical samples and
/// dimensions. Memberships retain sample order; they are not character cuts.
/// </summary>
public static class LegacySpatialWindows
{
    public static ReadOnlyMemory<int>[] Partition(ReadOnlySpan<LegacyInkPoint> points,
        int width, int height, int rows = 4, int columns = 4, bool useCoordinateQuantiles = false,
        CancellationToken cancellationToken = default)
    {
        if (points.Length is < 1 or > 255 || width is < 1 or > 65535 || height is < 1 or > 65535 ||
            rows is < 1 or > 16 || columns is < 1 or > 16)
            throw new ArgumentException("Expected1..255 prepared points, positive16-bit dimensions and1..16 rows/columns.");
        cancellationToken.ThrowIfCancellationRequested();
        int[] xBounds = Boundaries(points, true, width, columns, useCoordinateQuantiles);
        int[] yBounds = Boundaries(points, false, height, rows, useCoordinateQuantiles);
        var groups = new List<int>[rows * columns];
        for (int i = 0; i < groups.Length; i++) groups[i] = [];
        for (int i = 0; i < points.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int x = Locate(points[i].X << 10, xBounds), y = Locate(points[i].Y << 10, yBounds);
            groups[y * columns + x].Add(i);
        }
        var result = new ReadOnlyMemory<int>[groups.Length];
        for (int i = 0; i < result.Length; i++) result[i] = groups[i].ToArray();
        return result;
    }

    private static int[] Boundaries(ReadOnlySpan<LegacyInkPoint> points, bool xAxis,
        int dimension, int count, bool quantiles)
    {
        var bounds = new int[count + 1];
        if (quantiles)
        {
            var coordinates = new int[points.Length];
            for (int i = 0; i < points.Length; i++) coordinates[i] = (xAxis ? points[i].X : points[i].Y) << 10;
            Array.Sort(coordinates);
            for (int i = 0; i < count; i++) bounds[i] = coordinates[i * coordinates.Length / count];
            bounds[count] = coordinates[^1];
        }
        else
        {
            // Native divides the exponent-10 extent before repeated addition.
            // Flooring per boundary instead would change truncated-edge cases.
            int step = (dimension << 10) / count;
            for (int i = 0; i <= count; i++) bounds[i] = i * step;
        }
        return bounds;
    }

    private static int Locate(int coordinate, int[] bounds)
    {
        for (int i = 0; i < bounds.Length - 1; i++)
            if (bounds[i] <= coordinate && coordinate < bounds[i + 1]) return i;
        // Native assigns all unmatched values to the final bin, including a
        // quantile maximum, repeated-boundary axis and negative/outside point.
        return bounds.Length - 2;
    }
}
