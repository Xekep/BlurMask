namespace InkRecognition;

/// <summary>
/// The checked 42-channel pair-feature producer (1800759c0), given explicit
/// ordered stroke IDs, group width, scale and skipped-ID metadata.
/// </summary>
public static class LegacyLinePairFeatures
{
    public const int Width = 42;

    public static float[] Extract(IReadOnlyList<LegacyLineStrokeStatistics> strokes,
        IReadOnlyList<int> orderedStrokeIndices, float groupWidth, float scale,
        ReadOnlySpan<int> skippedStrokeIndices = default, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(strokes); ArgumentNullException.ThrowIfNull(orderedStrokeIndices);
        cancellationToken.ThrowIfCancellationRequested();
        if (strokes.Count is < 1 or > 4096 || orderedStrokeIndices.Count is < 1 or > 4096 ||
            skippedStrokeIndices.Length > 4096 || !float.IsFinite(groupWidth) || groupWidth <= 0 ||
            !float.IsFinite(scale) || scale <= 0)
            throw new ArgumentException("Expected bounded prepared strokes and positive explicit group width/scale.");
        for (int i = 0; i < skippedStrokeIndices.Length; i++)
            if (skippedStrokeIndices[i] < 0 || skippedStrokeIndices[i] >= strokes.Count ||
                (i != 0 && skippedStrokeIndices[i] <= skippedStrokeIndices[i - 1]))
                throw new ArgumentException("Skipped stroke IDs must be in range and strictly sorted.");
        foreach (int index in orderedStrokeIndices)
            if (index < 0 || index >= strokes.Count || strokes[index] is null) throw new ArgumentException("Invalid ordered stroke ID.");
        float floor = MathF.Max(scale * .05f, 3f);
        float[] output = new float[orderedStrokeIndices.Count * Width];
        int previousIndex = orderedStrokeIndices[0];
        for (int row = 0; row < orderedStrokeIndices.Count; row++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int currentIndex = orderedStrokeIndices[row];
            var a = strokes[previousIndex].Values; var b = strokes[currentIndex].Values;
            Span<float> f = output.AsSpan(row * Width, Width);
            float aHeight = a[3] - a[1], bHeight = b[3] - b[1];
            float ah = MathF.Max(aHeight, floor), bh = MathF.Max(bHeight, floor);
            float unionHeight = MathF.Max(b[3], a[3]) - MathF.Min(b[1], a[1]);
            float uh = MathF.Max(unionHeight, floor);
            float gapY = b[1] - a[3], meanY = b[5] - a[5];
            f[0] = gapY / ah; f[1] = gapY / bh; f[2] = gapY / uh; f[3] = gapY / scale;
            f[4] = meanY / ah; f[5] = meanY / bh; f[6] = meanY / uh; f[7] = meanY / scale;
            float meanXInverse = a[4] - b[4];
            float meanYInverse = a[5] - b[5];
            float centerAngle = MathF.Atan2(meanYInverse, meanXInverse);
            float topAngle = MathF.Atan2(a[1] - b[1], meanXInverse);
            float bottomAngle = MathF.Atan2(a[3] - b[3], meanXInverse);
            f[8] = MathF.Cos(centerAngle); f[9] = MathF.Sin(centerAngle);
            f[10] = MathF.Cos(topAngle); f[11] = MathF.Sin(topAngle);
            f[12] = MathF.Cos(bottomAngle); f[13] = MathF.Sin(bottomAngle);
            float aArea = (a[2] - a[0]) * aHeight, bArea = (b[2] - b[0]) * bHeight;
            float sa = MathF.Sqrt(MathF.Max(aArea, 1)); float sb = MathF.Sqrt(MathF.Max(bArea, 1));
            float unionWidth = MathF.Max(a[2], b[2]) - MathF.Min(a[0], b[0]);
            float unionArea = unionWidth * unionHeight;
            float su = MathF.Sqrt(MathF.Max(unionArea, 1));
            f[14] = sa / ah; f[15] = sa / bh; f[16] = sa / uh;
            f[17] = sb / ah; f[18] = sb / bh; f[19] = sb / uh;
            f[20] = su / ah; f[21] = su / bh; f[22] = su / uh;
            f[23] = b[6] / uh; f[24] = a[6] / uh;
            f[25] = b[15]; f[26] = a[15]; f[27] = b[16]; f[28] = a[16];
            f[29] = Dominant(b); f[30] = Dominant(a);
            float gapX = b[0] - a[2], meanX = b[4] - a[4];
            f[31] = gapX / ah; f[32] = gapX / bh; f[33] = gapX / uh; f[34] = gapX / groupWidth; f[35] = gapX / scale;
            f[36] = meanX / ah; f[37] = meanX / bh; f[38] = meanX / uh; f[39] = meanX / groupWidth; f[40] = meanX / scale;
            int delta = currentIndex - previousIndex, distance = Math.Abs(delta);
            int skipped = 0;
            // Retain the native count guard, including its strict inequality.
            if (skippedStrokeIndices.Length != 0 && distance < skippedStrokeIndices.Length)
                foreach (int index in skippedStrokeIndices)
                    if (Math.Min(currentIndex, previousIndex) < index && index < Math.Max(currentIndex, previousIndex)) skipped++;
            f[41] = distance < 2 || skipped >= distance - 1 ? 0 : delta < -1 ? -1 : 1;
            foreach (float value in f) if (!float.IsFinite(value)) throw new ArgumentException("Pair-feature geometry overflows.");
            previousIndex = currentIndex;
        }
        return output;
    }

    private static int Dominant(ReadOnlySpan<float> fields)
    {
        int dominant = 0; float maximum = fields[7];
        for (int i = 1; i < 8; i++) if (maximum < fields[7+i]) { dominant = i; maximum = fields[7+i]; }
        return dominant;
    }
}
