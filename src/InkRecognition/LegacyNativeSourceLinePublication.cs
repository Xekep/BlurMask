namespace InkRecognition;

/// <summary>Relevant actual fields in a source218 forty-byte line guide, not a glyph bounding rectangle.</summary>
public readonly record struct LegacyNativeSourceLineGuide(float Slope, float Baseline, float Height,
    float CorrectedBaseline, float CorrectedHeight);

/// <summary>Mixed32-byte source218.virtual100 output for the null external-transform branch.</summary>
public readonly record struct LegacyNativeSourceLineInfo(double Angle, float BaselineOffset, float Height,
    double OriginX, double OriginY);

/// <summary>Actual source218.virtual10 descriptor fields used by021ab0's line-count producer.</summary>
public readonly record struct LegacyNativeSourceLineInput(byte Flags, int SourceLineId);

/// <summary>Portable source2DA F8 line record, preserving its actual metric/geometry ownership fields.</summary>
public readonly record struct LegacyNativePublishedSourceLine(bool IsPrepared, double OriginX, double OriginY,
    double Angle, double XHeight, long OwnerTag, double UnadjustedBaseline);

/// <summary>
/// Legacy273 consumer0283b0 ->021ab0 source218 guide publication into2DA F8.
/// Prepared input descriptors, source guides and source rotation remain genuine caller-owned data.
/// </summary>
public sealed class LegacyNativeSourceLinePublication
{
    private readonly LegacyNativePublishedSourceLine[] _lines;
    public ReadOnlySpan<LegacyNativePublishedSourceLine> Lines => _lines;
    public int NextWrapperLineOffset { get; }

    private LegacyNativeSourceLinePublication(LegacyNativePublishedSourceLine[] lines, int nextOffset)
    { _lines = lines; NextWrapperLineOffset = nextOffset; }

    /// <summary>1692b0 null external-transform branch. Includes the native angle-based offset formula.</summary>
    public static LegacyNativeSourceLineInfo ReadPreparedLineInfo(LegacyNativeSourceLineGuide guide,
        float sourceRotation)
    {
        if (!float.IsFinite(guide.Slope) || !float.IsFinite(guide.Baseline) || !float.IsFinite(guide.Height) ||
            !float.IsFinite(guide.CorrectedBaseline) || !float.IsFinite(guide.CorrectedHeight) ||
            !float.IsFinite(sourceRotation)) throw new ArgumentException("Finite actual prepared line fields required.");
        bool corrected = guide.CorrectedHeight > 0f;
        float slope = corrected ? 0f : guide.Slope;
        float height = corrected ? guide.CorrectedHeight : guide.Height;
        double intercept = corrected ? (double)guide.CorrectedBaseline : (double)guide.Baseline;
        double rotation = (double)sourceRotation;
        double sine = rotation == 0d ? 0d : Math.Sin(rotation);
        double cosine = rotation == 0d ? 1d : Math.Cos(rotation);
        double originX = -sine * intercept;
        double originY = intercept * cosine;
        double angle = Math.Atan2((double)slope, 1d) + rotation;
        if (Math.Cos(angle) == 0d) throw new InvalidDataException("Native prepared line cannot publish a vertical angle.");
        //169456 multiplies originX by xmm9=ANGLE, not bytan(angle). Preserve the actual native operation.
        double xTerm = originX * angle;
        float offset = (float)(originY - xTerm);
        if (!double.IsFinite(originX) || !double.IsFinite(originY) || !double.IsFinite(angle) || !float.IsFinite(offset))
            throw new InvalidDataException("Prepared line projection exceeded the finite portable output contract.");
        return new(angle, offset, height, originX, originY);
    }

    /// <summary>
    /// Exact021ab0 active input-ID count, append allocation and wrapper-offset writes.
    /// A null guide denotes native virtual40 returningNULL and leaves the allocated record zero-filled.
    /// </summary>
    public static LegacyNativeSourceLinePublication Append(ReadOnlySpan<LegacyNativePublishedSourceLine> existingLines,
        int wrapperLineOffset, ReadOnlySpan<LegacyNativeSourceLineInput> inputs,
        ReadOnlySpan<LegacyNativeSourceLineGuide?> guides, float sourceRotation,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (existingLines.Length > 4096 || inputs.Length > 65536 || guides.Length > 4096 || wrapperLineOffset < 0 ||
            !float.IsFinite(sourceRotation)) throw new ArgumentOutOfRangeException(nameof(inputs));
        int appendedCount = 0;
        foreach (var input in inputs)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if ((input.Flags & 15) == 3 || input.SourceLineId < 0) continue;
            if (input.SourceLineId >= 4096) throw new ArgumentOutOfRangeException(nameof(inputs));
            appendedCount = Math.Max(appendedCount, input.SourceLineId + 1);
        }
        if (appendedCount > 4096 - existingLines.Length || wrapperLineOffset > existingLines.Length)
            throw new ArgumentOutOfRangeException(nameof(wrapperLineOffset));
        var lines = new LegacyNativePublishedSourceLine[existingLines.Length + appendedCount];
        existingLines.CopyTo(lines);
        for (int line = 0; line < appendedCount; line++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (line >= guides.Length || guides[line] is not { } guide) continue;
            var info = ReadPreparedLineInfo(guide, sourceRotation);
            lines[wrapperLineOffset + line] = new(true, info.OriginX, info.OriginY, info.Angle,
                (double)info.Height, -1, (double)guide.Baseline);
        }
        return new(lines, checked(wrapperLineOffset + appendedCount));
    }

    /// <summary>Actual published line height doubles -> existing2DB owner and SDK metric narrowing. No fit acceptance is inferred.</summary>
    public LegacyPreparedResultLineMetrics CreateResultLineMetrics(ReadOnlySpan<int> derivedCharacterSourceLineIds,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var heights = new double[_lines.Length];
        for (int line = 0; line < heights.Length; line++)
        { cancellationToken.ThrowIfCancellationRequested(); heights[line] = _lines[line].XHeight; }
        return LegacyPreparedResultLineMetrics.Create(heights, derivedCharacterSourceLineIds, cancellationToken);
    }
}
