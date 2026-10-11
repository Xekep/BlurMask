namespace InkRecognition;

/// <summary>Native class-2dd 18006e080 stroke moments: arc-weighted midpoints plus a unit endpoint.</summary>
public static class LegacyLineArcMoments
{
    public static LegacyPreparedLineGroup ExtractStroke(ReadOnlySpan<InkPoint> preparedPoints, int originalStrokeIndex,
        bool specialSingle = false, int lineLabel = int.MinValue, int clusterLabel = -1,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (preparedPoints.Length is < 1 or > 1_000_000 || originalStrokeIndex < 0 || clusterLabel < -1)
            throw new ArgumentException("Expected bounded prepared stroke points and explicit metadata.");
        float minX=float.MaxValue,minY=float.MaxValue,maxX=-float.MaxValue,maxY=-float.MaxValue;
        double weight=0,sx=0,sy=0,sxx=0,syy=0,sxy=0;
        for(int i=0;i<preparedPoints.Length;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var p=preparedPoints[i];
            if(!float.IsFinite(p.X)||!float.IsFinite(p.Y))throw new ArgumentException("Nonfinite stroke coordinate.");
            minX=MathF.Min(minX,p.X);minY=MathF.Min(minY,p.Y);maxX=MathF.Max(maxX,p.X);maxY=MathF.Max(maxY,p.Y);
            if(i+1==preparedPoints.Length)break;
            var q=preparedPoints[i+1];
            if(!float.IsFinite(q.X)||!float.IsFinite(q.Y))throw new ArgumentException("Nonfinite stroke coordinate.");
            // Midpoint addition and coordinate differences happen in float before double arithmetic.
            double x=(double)(p.X+q.X)*.5,y=(double)(p.Y+q.Y)*.5;
            double dx=(double)(q.X-p.X),dy=(double)(q.Y-p.Y);
            double length=Math.Sqrt(dy*dy+dx*dx);
            weight=length+weight;sx=length*x+sx;sy=length*y+sy;
            sxx=x*x*length+sxx;syy=y*y*length+syy;sxy=y*x*length+sxy;
        }
        var last=preparedPoints[^1];
        weight+=1;sx=(double)last.X+sx;sy=(double)last.Y+sy;
        sxx=(double)last.X*last.X+sxx;syy=(double)last.Y*last.Y+syy;sxy=(double)last.Y*last.X+sxy;
        var result=new LegacyPreparedLineGroup(originalStrokeIndex,originalStrokeIndex,weight,sx,sy,sxx,syy,sxy,
            minX,maxX,minY,maxY,specialSingle,lineLabel,clusterLabel);
        // Float intermediate overflow is a malformed-input error; never silently substitute a heuristic.
        if(!double.IsFinite(weight)||!double.IsFinite(sx)||!double.IsFinite(sy)||!double.IsFinite(sxx)||!double.IsFinite(syy)||!double.IsFinite(sxy))
            throw new InvalidDataException("Stroke moment overflow.");
        return result;
    }
}
