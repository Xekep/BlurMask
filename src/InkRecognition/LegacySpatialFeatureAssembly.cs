namespace InkRecognition;

/// <summary>
/// Checked4f spatial serializer0ba060, first4×4 family: calibrated global17
/// and sixteen spatial16 cells. Baseline/context/boundary append remains the
/// shared checked arithmetic. Caller supplies the matching spatial calibration.
/// </summary>
public static class LegacySpatialFeatureAssembly
{
    public static short[] TransformGeometry(ReadOnlySpan<int> globalStatistics,
        IReadOnlyList<int[]> spatialCells, ReadOnlySpan<short> calibrationMinima,
        ReadOnlySpan<short> calibrationMaxima, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(spatialCells);
        if (globalStatistics.Length != 17 || spatialCells.Count != 16 ||
            calibrationMinima.Length != 274 || calibrationMaxima.Length != 274)
            throw new ArgumentException("Expected global17, first4×4 spatial16 cells and matching274-column calibration.");
        cancellationToken.ThrowIfCancellationRequested();
        for (int i = 0; i < 274; i++)
            if (calibrationMaxima[i] < calibrationMinima[i]) throw new ArgumentException("Calibration ranges must be nondecreasing.");
        var result = new short[274]; int index = 0; result[index++] = 1024;
        // Identical native global prefix to the temporal serializer; spatial
        // local denominator differences begin only after these seventeen slots.
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
        foreach (var cell in spatialCells)
        {
            cancellationToken.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(cell);
            if (cell.Length != 16) throw new ArgumentException("Every spatial cell requires16 raw statistics.");
            for (int s = 0; s < 16; s++)
                result[index++] = s switch
                {
                    // Native width1024/columns, height1024/rows, density and
                    // loop fraction2048/(rows+columns), each256 in4×4.
                    0 or 1 or 2 or 15 => Ratio(cell[s], 256),
                    4 or 5 or >= 9 and <= 14 => unchecked((short)((short)cell[s] * 2048 - 1024)),
                    8 => unchecked((short)((short)cell[s] * 2)),
                    _ => Ratio(cell[s], 1024)
                };
        }
        for (int i = 1; i < result.Length; i++)
        {
            int range = calibrationMaxima[i] - calibrationMinima[i];
            if (range != 0) result[i] = unchecked((short)(((result[i] - calibrationMinima[i]) * 2048) / range - 1024));
        }
        return result;
    }

    private static short Ratio(int value, int divisor) => unchecked((short)(unchecked(value * 2048) / divisor - 1024));
}
