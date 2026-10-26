namespace InkRecognition;

/// <summary>
/// Checked type226 extended30 context geometry profile (1c8400/1c91e0) for
/// float-coordinate physical strokes. Explicit caller line height and X/Y
/// references are required. This is not the313 directional extractor, prior
/// selection, a baseline estimator or the context network itself.
/// </summary>
public static class LegacyFloatContextGeometry
{
    public const int Width = 30;

    public static float[] Extract(IReadOnlyList<InkStroke> strokes, float lineHeight,
        float horizontalReference, float verticalReference, bool useMaximumX = false,
        bool usePointMeanY = false, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(strokes);
        if (strokes.Count is < 1 or > 15 || !float.IsFinite(lineHeight) || lineHeight <= 0 ||
            !float.IsFinite(horizontalReference) || !float.IsFinite(verticalReference))
            throw new ArgumentException("Expected1..15 strokes, positive finite line height and finite X/Y references.");
        cancellationToken.ThrowIfCancellationRequested();
        int count = 0; float minX = float.MaxValue, maxX = -float.MaxValue, minY = float.MaxValue, maxY = -float.MaxValue;
        foreach (var stroke in strokes)
        {
            ArgumentNullException.ThrowIfNull(stroke); ArgumentNullException.ThrowIfNull(stroke.Points);
            if (stroke.Points.Count == 0 || stroke.Points.Count > 4096 - count)
                throw new ArgumentException("Context geometry requires nonempty strokes and at most4096 physical points.");
            count += stroke.Points.Count;
            foreach (var point in stroke.Points)
            {
                if (!float.IsFinite(point.X) || !float.IsFinite(point.Y) || MathF.Abs(point.X)>1_000_000 || MathF.Abs(point.Y)>1_000_000)
                    throw new ArgumentException("Expected finite bounded float source geometry.");
                minX = Math.Min(minX, point.X); maxX = Math.Max(maxX, point.X);
                minY = Math.Min(minY, point.Y); maxY = Math.Max(maxY, point.Y);
            }
        }
        float rawWidth = maxX - minX, rawHeight = maxY - minY;
        //Native also takes this special branch when tiny float extents round
        //away in extent+1. Do not rasterize or average its skipped statistics.
        if (rawWidth + 1f == 1f && rawHeight + 1f == 1f)
            return LegacyDegenerateFloatContextGeometry.ExtractValidated(strokes,lineHeight,horizontalReference,
                verticalReference,minY,useMaximumX,cancellationToken);
        // Native1C8400 compares the rounded extent-plus-one fields, then
        // uses the corresponding raw extent. Equal rounded fields keep width.
        float rasterExtent = rawWidth + 1f < rawHeight + 1f ? rawHeight : rawWidth;
        float rasterScale = 127f / (rasterExtent + 1e-6f);
        var raster = new bool[128 * 128]; var directions = new float[4];
        var perStroke = new float[3, 4]; float arc = 0, ySum = 0;
        float xBoundary = -float.MaxValue; int strokeIndex = 0;
        foreach (var stroke in strokes)
        {
            cancellationToken.ThrowIfCancellationRequested(); var points = stroke.Points;
            float sxMin = points[0].X, sxMax = sxMin, syMin = points[0].Y, syMax = syMin, strokeArc = 0;
            if (usePointMeanY) ySum += points[0].Y;
            for (int i = 1; i < points.Count; i++)
            {
                var previous = points[i - 1]; var point = points[i];
                float dx = point.X - previous.X, dy = point.Y - previous.Y;
                directions[0] += Math.Max(-dy, 0); directions[1] += Math.Max(dy, 0);
                directions[2] += Math.Max(-dx, 0); directions[3] += Math.Max(dx, 0);
                float segment = (float)Math.Sqrt((double)dx * dx + (double)dy * dy);
                arc += segment; strokeArc += segment;
                ySum += usePointMeanY ? point.Y : ((point.Y + previous.Y) * .5f) * segment;
                Paint(raster, Pixel(previous.X, minX, rasterScale), Pixel(previous.Y, minY, rasterScale),
                    Pixel(point.X, minX, rasterScale), Pixel(point.Y, minY, rasterScale));
                sxMin = Math.Min(sxMin, point.X); sxMax = Math.Max(sxMax, point.X);
                syMin = Math.Min(syMin, point.Y); syMax = Math.Max(syMax, point.Y);
            }
            xBoundary = Math.Max(xBoundary, useMaximumX ? sxMax : sxMin);
            if (strokeIndex < 3)
            {
                perStroke[strokeIndex, 0] = syMin;
                perStroke[strokeIndex, 1] = (sxMax - sxMin) + 1f;
                perStroke[strokeIndex, 2] = (syMax - syMin) + 1f;
                perStroke[strokeIndex, 3] = strokeArc;
            }
            strokeIndex++;
        }
        bool nearZeroArc = Math.Abs(arc) <= Math.Max(Math.Abs(arc) * 2.3841858e-7f, 1.1920929e-7f);
        if (!nearZeroArc) for (int i = 0; i < 4; i++) directions[i] /= arc;
        float meanY;
        if (usePointMeanY) meanY = ySum / count;
        else if (nearZeroArc)
        {
            foreach (var stroke in strokes) ySum += stroke.Points[0].Y;
            meanY = ySum / strokes.Count;
        }
        else meanY = ySum / arc;
        int maxRasterX = Pixel(maxX, minX, rasterScale), maxRasterY = Pixel(maxY, minY, rasterScale);
        int verticalRuns = 0, horizontalRuns = 0;
        for (int x = 0; x <= maxRasterX; x++)
        {
            bool previous = false;
            for (int y = 0; y <= maxRasterY; y++) { bool marked = raster[y * 128 + x]; if (marked && !previous) verticalRuns++; previous = marked; }
        }
        for (int y = 0; y <= maxRasterY; y++)
        {
            bool previous = false;
            for (int x = 0; x <= maxRasterX; x++) { bool marked = raster[y * 128 + x]; if (marked && !previous) horizontalRuns++; previous = marked; }
        }
        float doubleHeight = lineHeight + lineHeight, fourHeight = lineHeight * 4f;
        var first = strokes[0].Points[0]; var last = strokes[^1].Points[^1];
        var result = new float[Width]; result[0] = 1;
        result[1] = Clip((minY - verticalReference) / fourHeight);
        result[2] = Clip((meanY - verticalReference) / fourHeight);
        result[3] = Clip((rawWidth + 1f) / doubleHeight - 1f);
        result[4] = Clip((rawHeight + 1f) / doubleHeight - 1f);
        result[5] = Clip(arc / fourHeight - 1f);
        result[6] = Clip((verticalRuns / (float)(maxRasterX + 1)) * .5f - 1f);
        result[7] = Clip((horizontalRuns / (float)(maxRasterY + 1)) * .5f - 1f);
        directions.CopyTo(result, 8);
        result[12] = Clip((xBoundary - horizontalReference) / fourHeight);
        result[13] = Clip((last.X - first.X) / doubleHeight);
        result[14] = Clip((last.Y - first.Y) / doubleHeight);
        for (int i = 0; i < 3; i++) result[15 + i] = strokes.Count == i + 1 ? 1 : -1;
        for (int i = 0; i < 3; i++)
        {
            float top = perStroke[i, 0];
            // First stroke always subtracts the reference. Later zero-valued
            // tops retain native's sentinel ambiguity, even for real Y=0.
            if (i == 0 || top != 0) top -= verticalReference;
            result[18 + i * 4] = Clip(top / fourHeight);
            result[19 + i * 4] = Clip(perStroke[i, 1] / doubleHeight - 1f);
            result[20 + i * 4] = Clip(perStroke[i, 2] / doubleHeight - 1f);
            result[21 + i * 4] = Clip(perStroke[i, 3] / fourHeight - 1f);
        }
        foreach (float value in result) if (!float.IsFinite(value)) throw new ArgumentException("Caller reference/height products exceed the finite arithmetic domain.");
        return result;
    }

    private static float Clip(float value) => value < -1 ? -1 : value > 1 ? 1 : value;
    private static int Pixel(float value, float minimum, float scale)
    {
        int pixel = (int)((value - minimum) * scale + .5f);
        return pixel is >= 0 and < 128 ? pixel : throw new ArgumentException("Context raster coordinates exceed the checked128 grid.");
    }
    private static void Paint(bool[] raster, int x0, int y0, int x1, int y1)
    {
        if (x1 < x0) { (x0, x1) = (x1, x0); (y0, y1) = (y1, y0); }
        int dx = x1 - x0, dy = Math.Abs(y1 - y0), signY = y1 >= y0 ? 1 : -1;
        int major = Math.Max(dx, dy), minor = Math.Min(dx, dy), error = 2 * minor - major;
        for (int step = 0; step <= major; step++)
        {
            raster[y0 * 128 + x0] = true;
            if (error >= 0) { x0++; y0 += signY; error += 2 * (minor - major); }
            else { if (dx >= dy) x0++; else y0 += signY; error += 2 * minor; }
        }
    }
}
