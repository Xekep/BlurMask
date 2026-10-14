namespace InkRecognition;

public readonly record struct LegacyCharacterLineInputBounds(float Left, float Top, float Right, float Bottom);
public enum LegacyCharacterLineFitBasis { PrimaryProfileFit, RejectedPrimaryBoundingBox, TooSmallPrimaryBoundingBox }
/// <summary>Ink0221A0 output. Bounding-box cases are an explicit native fallback,
/// not successful learned profile inference or calibrated recognition confidence.</summary>
public readonly record struct LegacyCharacterLineCompletedFit(float Baseline, float Height, float Slope,
    LegacyCharacterLineFitBasis Basis, LegacyPreparedCharacterLineFitResult? Primary);

/// <summary>Exact prepared C1A0221A0 acceptance/fallback after verified021110 IRLS.
/// Text16F6F0 calls this through C1A with forceZeroSlope=false. This completes that
/// scalar helper; source219 postprocessor orchestration and publication are separate.</summary>
public static class LegacyCharacterLineFitCompletion
{
    public static LegacyCharacterLineCompletedFit Estimate(ReadOnlySpan<LegacyPreparedCharacterLineItem> items,
        LegacyCharacterLineFitProfile profile, LegacyCharacterLineInputBounds bounds, bool forceZeroSlope = false,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested(); profile.Validate();
        if (!float.IsFinite(bounds.Left) || !float.IsFinite(bounds.Right) || !float.IsFinite(bounds.Top) ||
            !float.IsFinite(bounds.Bottom) || bounds.Left > bounds.Right || bounds.Top > bounds.Bottom)
            throw new ArgumentException("Finite ordered actual physical-input bounds required.");
        float width = bounds.Right-bounds.Left, height = bounds.Bottom-bounds.Top;
        float minimumFraction = height*.05f;
        if (!float.IsFinite(width) || !float.IsFinite(height) || !float.IsFinite(minimumFraction))
            throw new InvalidDataException("C1A physical-input extent overflow.");
        var primary = LegacyPreparedCharacterLineFit.Fit(items,profile,forceZeroSlope,cancellationToken:cancellationToken);
        bool accepted = primary is not null && primary.Value.RawProfileAffinity > 0;
        if (accepted)
        {
            var fit = primary!.Value.ToPreparedFitOutput();
            if (minimumFraction <= fit.XHeight)
                return new(fit.BaselineY,fit.XHeight,fit.Slope,LegacyCharacterLineFitBasis.PrimaryProfileFit,primary);
        }
        float baseline;
        float outputHeight;
        if (!accepted && width < profile.MinimumHeight && height < profile.MinimumHeight)
        { baseline=bounds.Bottom; outputHeight=profile.MinimumHeight; }
        else
        {
            baseline=(bounds.Bottom+bounds.Top)*.5f;
            outputHeight=minimumFraction <= profile.MinimumHeight ? profile.MinimumHeight : minimumFraction;
        }
        if (!float.IsFinite(baseline) || !float.IsFinite(outputHeight))
            throw new InvalidDataException("C1A fallback overflow.");
        return new(baseline,outputHeight,0,accepted ? LegacyCharacterLineFitBasis.TooSmallPrimaryBoundingBox :
            LegacyCharacterLineFitBasis.RejectedPrimaryBoundingBox,primary);
    }

    /// <summary>16F6F0 scans the contiguous descriptor24/28 physical interval.
    /// This differs from each720-row's cropped membership and glyph box.</summary>
    public static LegacyCharacterLineInputBounds ScanPhysicalInterval(IReadOnlyList<InkStroke> strokes,
        int firstPhysical, int lastPhysical, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(strokes); cancellationToken.ThrowIfCancellationRequested();
        if (strokes.Count > 256 || firstPhysical < 0 || lastPhysical < firstPhysical || lastPhysical >= strokes.Count)
            throw new ArgumentException("Bounded inclusive descriptor physical interval required.");
        bool any=false;float left=0,right=0,top=0,bottom=0;int points=0;
        for(int s=firstPhysical;s<=lastPhysical;s++)
        {
            if(strokes[s] is null || strokes[s].Points.Count>4096)throw new ArgumentException("Bounded physical input required.");
            foreach(var p in strokes[s].Points)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if(!float.IsFinite(p.X)||!float.IsFinite(p.Y))throw new ArgumentException("Finite physical points required.");
                if(++points>1_048_576)throw new ArgumentException("Physical point work bound exceeded.");
                if(!any){left=right=p.X;top=bottom=p.Y;any=true;}
                else{if(p.X<left)left=p.X;if(right<p.X)right=p.X;if(p.Y<top)top=p.Y;if(bottom<p.Y)bottom=p.Y;}
            }
        }
        return new(left,top,right,bottom);
    }
}
