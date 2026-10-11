namespace InkRecognition;

/// <summary>Origin and two edge vectors of the native six-float glyph geometry.</summary>
public readonly record struct LegacyGlyphAffineGeometry(
    float X, float Y, float FirstX, float FirstY, float SecondX, float SecondY);

public readonly record struct LegacyGlyphNormalizedLines(float Baseline, float XHeight);

/// <summary>
/// The value portion of Document 18005b5d0's mutable glyph update. This is not
/// the Document object ownership/COW machinery or a handwriting/font-size policy.
/// </summary>
public sealed class LegacyGlyphGeometryPayload
{
    private float[] _corners = new float[8];
    private LegacyGlyphNormalizedLines _storedLines;
    public bool HasNormalizedLines { get; private set; }

    /// <summary>Four corners in native order: origin, first, first+second, second.</summary>
    public ReadOnlyMemory<float> Corners => _corners;

    public void SetGeometry(LegacyGlyphAffineGeometry geometry, LegacyGlyphNormalizedLines? lines)
    {
        Validate(geometry.X); Validate(geometry.Y);
        Validate(geometry.FirstX); Validate(geometry.FirstY);
        Validate(geometry.SecondX); Validate(geometry.SecondY);
        if (lines is { } value)
        {
            Validate(value.Baseline); Validate(value.XHeight);
            if (value.XHeight < 0) throw new ArgumentException("Normalized x-height must be nonnegative.");
        }
        // Preserve native left-associated single-precision additions.
        float[] corners = [geometry.X, geometry.Y,
            geometry.X + geometry.FirstX, geometry.Y + geometry.FirstY,
            (geometry.X + geometry.FirstX) + geometry.SecondX,
            (geometry.Y + geometry.FirstY) + geometry.SecondY,
            geometry.X + geometry.SecondX, geometry.Y + geometry.SecondY];
        foreach (float corner in corners)
            if (!float.IsFinite(corner)) throw new InvalidDataException("Glyph corner overflow.");
        _corners = corners;
        HasNormalizedLines = lines.HasValue;
        // Native removal clears the presence byte, retaining the old pair.
        if (lines is { } supplied) _storedLines = supplied;
    }

    /// <summary>Ink 180054190/1800541b0 presence and pair readback.</summary>
    public bool TryGetNormalizedLines(out LegacyGlyphNormalizedLines lines)
    {
        lines = _storedLines;
        return HasNormalizedLines;
    }

    private static void Validate(float value)
    {
        if (!float.IsFinite(value)) throw new ArgumentException("Glyph geometry must be finite.");
    }
}
