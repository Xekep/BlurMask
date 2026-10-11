namespace InkRecognition;

/// <summary>
/// Checked temporal directional channel order/transforms (0b9720): bias,
/// global17, sixteen local16 windows, explicit274-column calibration, baseline9,
/// context28 and two boundary values. No calibration/model assets or baseline
/// estimates are bundled. This is the checked3f temporal layout, not the4f
/// alternative window layout, a context estimator or a cursive decoder.
/// </summary>
public static class LegacyDirectionalFeatureAssembly
{
    public const int GeometryWidth = 274, BaselineWidth = 9, ContextWidth = 28, InputWidth = 313;

    public static short[] TransformGeometry(ReadOnlySpan<int> globalStatistics,
        IReadOnlyList<int[]> localWindows, ReadOnlySpan<short> calibrationMinima,
        ReadOnlySpan<short> calibrationMaxima, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(localWindows);
        if (globalStatistics.Length != 17 || localWindows.Count != 16 ||
            calibrationMinima.Length != GeometryWidth || calibrationMaxima.Length != GeometryWidth)
            throw new ArgumentException("Expected global17, sixteen local16 windows and two274-column calibration arrays.");
        cancellationToken.ThrowIfCancellationRequested();
        for (int i = 0; i < GeometryWidth; i++)
            if (calibrationMaxima[i] < calibrationMinima[i]) throw new ArgumentException("Calibration ranges must be nondecreasing.");
        var result = new short[GeometryWidth]; int index = 0; result[index++] = 1024;
        for (int s = 0; s < 17; s++)
        {
            int value = globalStatistics[s];
            result[index++] = s switch
            {
                2 => unchecked((short)(unchecked(value * 2048 - 2048) / 5 - 1024)),
                3 => Ratio(value, 3072),
                8 => unchecked((short)((short)value * 2)),
                >= 9 and <= 15 => Ratio(value, 3),
                _ => Ratio(value, 1024)
            };
        }
        foreach (var window in localWindows)
        {
            cancellationToken.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(window);
            if (window.Length != 16) throw new ArgumentException("Every local window requires16 raw statistics.", nameof(localWindows));
            for (int s = 0; s < 16; s++)
            {
                int value = window[s];
                result[index++] = s switch
                {
                    4 or 5 or >= 9 and <= 14 => unchecked((short)((short)value * 2048 - 1024)),
                    8 => unchecked((short)((short)value * 2)),
                    15 => Ratio(value, 128), // 2048 / sixteen windows
                    _ => Ratio(value, 1024)
                };
            }
        }
        // The checked274-column profile calibrates global/windows before the
        // nine baseline columns are appended. Bias remains1024, including its
        // equal min/max native fallback slot. Output is not clamped.
        for (int i = 1; i < GeometryWidth; i++)
        {
            int range = calibrationMaxima[i] - calibrationMinima[i];
            if (range != 0) result[i] = unchecked((short)(((result[i] - calibrationMinima[i]) * 2048) / range - 1024));
        }
        return result;
    }

    public static short[] AppendInput(ReadOnlySpan<short> calibratedGeometry,
        ReadOnlySpan<int> baselineRawStatistics, ReadOnlySpan<short> context,
        short startBoundary, short endBoundary, CancellationToken cancellationToken = default)
    {
        if (calibratedGeometry.Length != GeometryWidth || baselineRawStatistics.Length != BaselineWidth || context.Length != ContextWidth)
            throw new ArgumentException("Expected calibrated274 geometry, explicit9 raw baseline statistics and28 context columns.");
        cancellationToken.ThrowIfCancellationRequested();
        var result = new short[InputWidth]; calibratedGeometry.CopyTo(result);
        for (int s = 0; s < BaselineWidth; s++)
        {
            int value = s == 3 ? unchecked(baselineRawStatistics[s] + 4096) : baselineRawStatistics[s];
            result[GeometryWidth + s] = Ratio(value, s switch { 2 or 3 => 8192, >= 6 => 1024, _ => 4096 });
        }
        context.CopyTo(result.AsSpan(GeometryWidth + BaselineWidth));
        result[^2] = startBoundary; result[^1] = endBoundary; return result;
    }

    private static short Ratio(int value, int divisor) => unchecked((short)(unchecked(value * 2048) / divisor - 1024));
}
