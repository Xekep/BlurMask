namespace InkRecognition;

/// <summary>Text2C1.virtual18=053f90: fresh ordinary selected-input point coordinates -> Ink position range.</summary>
public static class LegacyNativeTextSourceRangeProjection
{
    public static LegacyNativeInkPositionRange Project(LegacyNativeSelectedInkRange selectedInput,
        float? firstOverride = null, float? lastOverride = null)
    {
        if (selectedInput.IsKnownText || selectedInput.SourceIndex == uint.MaxValue || selectedInput.PointCount is < 1 or > 65536)
            throw new ArgumentException("An actual ordinary source input is required.", nameof(selectedInput));
        int firstPoint = selectedInput.FirstPoint;
        int lastPoint = selectedInput.LastPoint == -1 ? selectedInput.PointCount - 1 : selectedInput.LastPoint;
        float firstFraction = selectedInput.FirstFraction, lastFraction = selectedInput.LastFraction;
        if (firstPoint < 0 || lastPoint < firstPoint || lastPoint >= selectedInput.PointCount ||
            !float.IsFinite(firstFraction) || !float.IsFinite(lastFraction) || firstFraction is < 0 or >= 1 ||
            lastFraction is < 0 or >= 1 || (firstPoint == lastPoint && firstFraction > lastFraction) ||
            (firstFraction != 0 && firstPoint + 1 >= selectedInput.PointCount) ||
            (lastFraction != 0 && lastPoint + 1 >= selectedInput.PointCount))
            throw new ArgumentException("Finite ordered prepared source-input fields required.", nameof(selectedInput));
        float length = (float)((lastFraction > 0 ? 1 : 0) - firstPoint + lastPoint);
        float first = firstOverride is null or -1f ? 0f : firstOverride.Value;
        float last = lastOverride is null or -1f ? length : lastOverride.Value;
        if (!float.IsFinite(first) || !float.IsFinite(last) || first < 0 || last < first || last > length)
            throw new ArgumentOutOfRangeException(nameof(firstOverride), "Native relative point overrides exceed the selected view.");
        float startCoordinate = Map(first), endCoordinate = Map(last);
        var start = LegacyNativeInkPosition.Pack(selectedInput.SourceIndex, startCoordinate, 3, selectedInput.PointCount);
        var end = LegacyNativeInkPosition.Pack(selectedInput.SourceIndex, endCoordinate, 5, selectedInput.PointCount);
        //053f90 removes terminalFF before applying the caller's open-end flag, then restores the closed terminal case.
        byte endFraction = end.Fraction == 255 ? (byte)0 : end.Fraction;
        ulong endValue = (end.Value & ~0xff00UL) | ((ulong)endFraction << 8);
        if (!selectedInput.FirstClosed && first == 0 && last != 0) start = new((start.Value & ~255UL) | 7);
        float quantizedLast = lastFraction * 200f;
        byte originalLastFraction = (byte)(int)(quantizedLast + .5f);
        if (!selectedInput.LastClosed && end.PointIndex == lastPoint && endFraction == originalLastFraction &&
            (start.PointIndex != lastPoint || start.Fraction != originalLastFraction))
            endValue = (endValue & ~255UL) | 1;
        else if ((byte)endValue == 5 && end.PointIndex == selectedInput.PointCount - 1)
            endValue = (endValue & ~0xff00UL) | 0xff00;
        end = new(endValue);
        if (end.Value < start.Value) throw new InvalidDataException("Native source projection produced a reversed position range.");
        return new(start, end);

        float Map(float coordinate)
        {
            if (firstPoint == lastPoint)
            {
                float extent = lastFraction - firstFraction;
                float scaled = extent * coordinate;
                float absolute = scaled + (float)firstPoint;
                return absolute + firstFraction;
            }
            if (coordinate >= 1f)
            {
                float absolute = (float)firstPoint + coordinate;
                if (absolute > (float)lastPoint)
                {
                    float remainder = absolute - (float)lastPoint;
                    float scaled = remainder * lastFraction;
                    absolute = scaled + (float)lastPoint;
                }
                return absolute;
            }
            else
            {
                float scale = 1f - firstFraction;
                float scaled = scale * coordinate;
                float absolute = scaled + (float)firstPoint;
                return absolute + firstFraction;
            }
        }
    }
}
