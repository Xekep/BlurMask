namespace InkRecognition;

/// <summary>
/// Restores BC280 horizontal/vertical reversal and stroke-boundary flags used
/// by the actual native2B1 feature producer. These are landmarks, not detected
/// character boundaries. Input coordinates must fit signed shorts. Native coordinate preparation and intersection flags
/// are separate, and no pen-up interpolation is added by this pass.
/// </summary>
public static class LegacyDirectional2B1Landmarks
{
    public const byte HorizontalMinimum = 1, HorizontalMaximum = 2;
    public const byte VerticalMaximum = 8, VerticalMinimum = 16;
    public const byte StrokeStart = 64, StrokeEnd = 128;

    public static LegacyDirectionalLandmarkSet Extract(IReadOnlyList<LegacyInkStroke> strokes,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(strokes);
        if (strokes.Count is < 1 or > 15) throw new ArgumentException("The checked landmark pass requires 1..15 physical strokes.", nameof(strokes));
        cancellationToken.ThrowIfCancellationRequested();
        int count = 0; var ends = new int[strokes.Count];
        for (int i = 0; i < strokes.Count; i++)
        {
            ArgumentNullException.ThrowIfNull(strokes[i]);
            if (strokes[i].Points.IsEmpty) throw new ArgumentException("Each actual2B1 physical stroke must contain a point.", nameof(strokes));
            if (strokes[i].Points.Length > 4096 - count)
                throw new ArgumentException("The landmark pass exceeds4096 sample points.", nameof(strokes));
            count += strokes[i].Points.Length;
            // LegacyInkPoint stores short X/Y; subtraction promotes to int,
            // so every native signed-short coordinate difference is safe.
            ends[i] = count;
        }
        var flags = new byte[count]; int offset = 0;
        foreach (var stroke in strokes)
        {
            cancellationToken.ThrowIfCancellationRequested(); var points = stroke.Points;
            flags[offset] |= StrokeStart; flags[offset + points.Length - 1] |= StrokeEnd;
            int xDirection = FirstDirection(points, horizontal: true), yDirection = FirstDirection(points, horizontal: false);
            flags[offset] |= XFlag(xDirection); flags[offset] |= YFlag(yDirection);
            // Actual BC280 reloads point1Y before advancing to point2. The
            // historical generic B6D20 helper intentionally keeps point0Y.
            int previousY = points.Length > 1 ? points[1].Y : points[0].Y;
            for (int j = 2; j < points.Length; j++)
            {
                int x = Math.Sign(points[j].X - points[j - 1].X), y = Math.Sign(points[j].Y - previousY);
                if (x != 0)
                {
                    if (xDirection == -x) flags[offset + j - 1] |= XFlag(x);
                    xDirection = x;
                }
                if (y != 0)
                {
                    if (yDirection == -y) flags[offset + j - 1] |= YFlag(y);
                    yDirection = y;
                }
                previousY = points[j].Y;
            }
            flags[offset + points.Length - 1] |= XFlag(-xDirection);
            flags[offset + points.Length - 1] |= YFlag(-yDirection);
            offset += points.Length;
        }
        return new(flags, ends);
    }

    private static int FirstDirection(ReadOnlySpan<LegacyInkPoint> points, bool horizontal)
    {
        for (int i = 1; i < points.Length; i++)
        {
            int difference = horizontal ? points[i].X - points[i - 1].X : points[i].Y - points[i - 1].Y;
            if (difference != 0) return Math.Sign(difference);
        }
        return 0;
    }
    private static byte XFlag(int direction) => direction > 0 ? HorizontalMinimum : direction < 0 ? HorizontalMaximum : (byte)0;
    private static byte YFlag(int direction) => direction > 0 ? VerticalMinimum : direction < 0 ? VerticalMaximum : (byte)0;
}
