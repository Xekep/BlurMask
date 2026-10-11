namespace InkRecognition;

/// <summary>Owned normalized shorts and their exact prepared float source.</summary>
public sealed class LegacyNormalizedPreparedGlyph
{
    public LegacyCursivePreparedInk PreparedInk{get;}
    public ReadOnlyMemory<LegacyInkRange> Ranges{get;}
    public IReadOnlyList<LegacyInkStroke> Strokes{get;}
    public LegacyInkBaseline Baseline{get;}
    internal LegacyNormalizedPreparedGlyph(LegacyCursivePreparedInk source,LegacyInkRange[] ranges,
        LegacyInkStroke[] strokes,short height)
    {PreparedInk=source;Ranges=ranges;Strokes=Array.AsReadOnly(strokes);Baseline=new(0,height);}
}

/// <summary>
/// Scoped float-to-short branch1b4e40 for explicit source flagsFF/FF and
/// positive target height. Native chronological range ordering, source line
/// resolution and post-normalization pen-up merging remain caller operations.
/// </summary>
public static class LegacyPreparedGlyphNormalization
{
    /// <summary>
    /// The caller supplies candidate minimumX, resolved source baseline/height,
    /// stored target height and range-count threshold before native pen-up merge.
    /// Fractional prepared points are scaled before truncation, never rounded
    /// directly in their original source units. A range is one output stroke.
    /// Inputs that would wrap below signed-short minimum are outside this bounded
    /// supported branch and rejected; native only clamps the upper endpoint.
    /// </summary>
    public static LegacyNormalizedPreparedGlyph NormalizeOrderedRanges(LegacyCursivePreparedInk source,
        ReadOnlySpan<LegacyInkRange> orderedRanges,float candidateMinimumX,float sourceBaseline,
        float sourceHeight,short targetHeight,int maximumRangesBeforeMerge,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(source);cancellationToken.ThrowIfCancellationRequested();
        if(!float.IsFinite(candidateMinimumX)||!float.IsFinite(sourceBaseline)||
            !float.IsFinite(sourceHeight)||sourceHeight<=0||targetHeight<=0)
            throw new ArgumentException("Explicit finite source frame, positive source/target heights required.");
        if(maximumRangesBeforeMerge is < 1 or > 15||orderedRanges.Length<1||orderedRanges.Length>maximumRangesBeforeMerge)
            throw new ArgumentException("Range count is outside the caller-confirmed pre-merge bound.",nameof(orderedRanges));
        var ranges=orderedRanges.ToArray();var counts=source.StrokePointCounts.Span;
        var starts=new int[counts.Length];for(int i=1;i<starts.Length;i++)starts[i]=starts[i-1]+counts[i-1];
        var strokes=new LegacyInkStroke[ranges.Length];int pointCount=0;
        for(int i=0;i<ranges.Length;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();var range=ranges[i];
            if((uint)range.StrokeIndex>=(uint)counts.Length||range.StartPoint<0||
                range.EndPointExclusive<=range.StartPoint||range.EndPointExclusive>counts[range.StrokeIndex])
                throw new ArgumentException("Range lies outside its prepared physical stroke.",nameof(orderedRanges));
            int size=range.EndPointExclusive-range.StartPoint;
            if(size>4096-pointCount)throw new ArgumentException("Selected glyph exceeds4096 points.",nameof(orderedRanges));
            pointCount+=size;var points=new LegacyInkPoint[size];
            for(int j=0;j<size;j++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var p=source.Points.Span[starts[range.StrokeIndex]+range.StartPoint+j];
                float x=((p.X-candidateMinimumX)*targetHeight)/sourceHeight;
                float y=((p.Y-sourceBaseline)*targetHeight)/sourceHeight;
                points[j]=new(Convert(x),Convert(y));
            }
            strokes[i]=new(points);
        }
        return new(source,ranges,strokes,targetHeight);
    }

    private static short Convert(float value)
    {
        if(!float.IsFinite(value)||value<short.MinValue)
            throw new ArgumentException("Normalized geometry would leave the supported signed-short domain.");
        return value>short.MaxValue?short.MaxValue:(short)(int)value;
    }
}
