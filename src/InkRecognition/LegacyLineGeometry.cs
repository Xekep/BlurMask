namespace InkRecognition;

/// <summary>Prepared native user-guideline fields; these are not automatic cluster-fitting results.</summary>
public readonly record struct LegacyLineCluster(int RowCount, float Baseline, float Pitch, float Height);

public readonly record struct LegacyLineBounds(float X, float Y, float Width, float Height);

/// <summary>Line coordinates in input-ink units, with Y increasing downwards.</summary>
public readonly record struct LegacyLinePair(float Baseline, float Height);

/// <summary>
/// Checked scalar branches of Text 1801629c0. This materializes prepared records,
/// and does not infer a cluster or substitute the whole-ink bounds for a native fallback record.
/// </summary>
public static class LegacyLineGeometry
{
    public static LegacyLinePair Materialize(LegacyLineCluster? cluster, int rowIndex,
        LegacyLineBounds? fallback = null)
    {
        if (cluster is { } c && c.Height > 0)
        {
            if (c.RowCount < 1 || rowIndex < 0 || rowIndex >= c.RowCount ||
                !float.IsFinite(c.Baseline) || !float.IsFinite(c.Pitch) || !float.IsFinite(c.Height))
                throw new ArgumentException("A positive-height cluster needs finite fields and a valid row index.");
            // The native one-row branch ignores its item's stored row index.
            float rowOffset = (float)(c.RowCount == 1 ? 0 : rowIndex) * c.Pitch;
            float baseline = rowOffset + c.Baseline;
            if (!float.IsFinite(baseline)) throw new ArgumentException("Line coordinates overflow.");
            return new(baseline, c.Height);
        }
        if (fallback is not { } b || !float.IsFinite(b.X) || !float.IsFinite(b.Y) ||
            !float.IsFinite(b.Width) || !float.IsFinite(b.Height) || b.Width < 0 || b.Height <= 0)
            throw new ArgumentException("Without a positive-height cluster, a finite positive-height fallback record is required.");
        float fallbackOffset = b.Height * .75f;
        float fallbackBaseline = fallbackOffset + b.Y;
        float fallbackHeight = b.Height * .25f;
        if (!float.IsFinite(fallbackBaseline) || fallbackHeight <= 0)
            throw new ArgumentException("Fallback line coordinates overflow or underflow.");
        return new(fallbackBaseline, fallbackHeight);
    }
}
