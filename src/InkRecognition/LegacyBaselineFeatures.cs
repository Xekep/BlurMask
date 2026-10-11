namespace InkRecognition;

/// <summary>Legacy baseline and x-height in the same coordinate units as ink. Negative height means unavailable.</summary>
public readonly record struct LegacyInkBaseline(short Y, short Height);

internal static class LegacyBaselineFeatures
{
    internal static short[] Extract(LegacyGlyphGeometry ink, LegacyInkBaseline baseline, bool[] pixels)
    {
        var result = new short[9];
        if (baseline.Height < 0) return result;
        int y = unchecked((baseline.Y - ink.MinY) * ink.Scale) >> 10;
        y += ink.CenterY;
        int height = unchecked(baseline.Height * ink.Scale) >> 10;
        if (height < 0) throw new ArgumentException("The baseline height overflows the legacy fixed-point transform.", nameof(baseline));
        int count = 0, meanY = 0, physicalLength = 0;
        for (int i = 0; i < 30; i++)
        {
            if (ink.PenState[i] == 1024) { count++; meanY += ink.SampleY[i]; }
            if (i < 29 && ink.PenState[i] == 1024 && ink.PenState[i + 1] == 1024)
                physicalLength += LegacyStrokeFeatures.Distance(ink.SampleX[i], ink.SampleY[i], ink.SampleX[i + 1], ink.SampleY[i + 1]);
        }
        if (height == 0)
        {
            result[0] = result[1] = result[2] = result[3] = result[7] = result[8] = 1024;
        }
        else
        {
            result[0] = Upper(512 * (ink.Width + 1) / height - 1024);
            result[1] = Upper(512 * (ink.Height + 1) / height - 1024);
            result[2] = Clamp(256 * (meanY / count - y) / height);
            result[3] = Upper(256 * (physicalLength >> 4) / height - 1024);
            result[7] = Clamp(512 * (y - height - ink.CenterY) / height - 1024);
            result[8] = Clamp(512 * (ink.Height + ink.CenterY - y) / height - 1024);
        }
        int above = 0, within = 0, below = 0;
        for (int index = 0; index < pixels.Length; index++)
        {
            if (!pixels[index]) continue;
            int pixelY = index / 128;
            if (pixelY < y - height) above++;
            else if (pixelY > y) below++;
            else within++;
        }
        int total = above + within + below;
        if (total == 0) result[4] = result[5] = result[6] = 1024;
        else
        {
            result[4] = Clamp(2048 * above / total - 1024);
            result[5] = Clamp(2048 * within / total - 1024);
            result[6] = Clamp(2048 * below / total - 1024);
        }
        return result;
    }

    private static short Upper(int value) => (short)Math.Min(1024, (int)unchecked((short)value));
    private static short Clamp(int value) => (short)Math.Clamp((int)unchecked((short)value), -1024, 1024);
}
