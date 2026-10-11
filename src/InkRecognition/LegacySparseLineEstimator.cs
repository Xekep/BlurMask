namespace InkRecognition;

public readonly record struct LegacySparseLineEstimate(LegacyLinePair Line, float MeanY,
    float StandardDeviationY, float HalfBand, int VerticalRunCount);

/// <summary>
/// Exact sparse branch of Text class 24a / 18016e0c0. Returns false when the
/// native branch would invoke its unreconstructed four-component Gaussian fit.
/// </summary>
public static class LegacySparseLineEstimator
{
    public static bool TryEstimate(IReadOnlyList<InkPoint[]> preparedStrokes, out LegacySparseLineEstimate estimate,
        byte sparseRunThreshold = 10, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preparedStrokes); cancellationToken.ThrowIfCancellationRequested();
        if (preparedStrokes.Count is < 1 or > 4096 || sparseRunThreshold == 0)
            throw new ArgumentException("Expected bounded explicit strokes and a nonzero sparse-run threshold.");
        int total = 0, runs = 0;
        float minX=float.MaxValue,minY=float.MaxValue,maxX=-float.MaxValue,maxY=-float.MaxValue;
        foreach(var stroke in preparedStrokes)
        {
            if(stroke is null || stroke.Length==0 || stroke.Length>1_000_000-total)
                throw new ArgumentException("Expected nonempty strokes with at most one million total points.");
            total+=stroke.Length;int direction=0;
            for(int i=0;i<stroke.Length;i++)
            {
                cancellationToken.ThrowIfCancellationRequested();var p=stroke[i];
                if(!float.IsFinite(p.X)||!float.IsFinite(p.Y))throw new ArgumentException("Nonfinite stroke coordinate.");
                minX=MathF.Min(minX,p.X);minY=MathF.Min(minY,p.Y);maxX=MathF.Max(maxX,p.X);maxY=MathF.Max(maxY,p.Y);
                if(i==0)continue;
                float previous=stroke[i-1].Y;
                if(p.Y<previous){if(direction<=0)runs++;direction=1;}
                else if(previous<p.Y){if(direction>=0)runs++;direction=-1;}
            }
            if(direction!=0)runs++;
        }
        estimate=default;
        if(runs>=sparseRunThreshold)return false;
        // 18016b050: anchored float differences and squared differences, double accumulation.
        float origin=preparedStrokes[0][0].Y;double sum=0,squareSum=0;
        foreach(var stroke in preparedStrokes) foreach(var point in stroke)
        {
            cancellationToken.ThrowIfCancellationRequested();float difference=point.Y-origin;
            sum+=(double)difference;squareSum+=(double)(difference*difference);
        }
        double variance=(squareSum*total-sum*sum)/((double)total*total);
        float mean=(float)(sum/total+(double)origin);
        float std=(float)Math.Sqrt(variance);
        float width=maxX-minX,height=maxY-minY;
        float factor=height==0?5f:Math.Clamp(width/height-2f,1f,5f);
        float spread;
        if(height==0&&width==0)spread=1f;
        else spread=MathF.Max(std,width*.05f);
        float halfBand=factor*spread;
        if((double)MathF.Abs(halfBand)<.0001)halfBand=MathF.Max(MathF.Abs(width)*.01f,.01f);
        float baseline=halfBand+mean,top=mean-halfBand;
        float lineHeight=(float)((double)baseline-(double)top);
        if(!double.IsFinite(variance)||variance<0||!float.IsFinite(mean)||!float.IsFinite(std)||
            !float.IsFinite(halfBand)||!float.IsFinite(baseline)||!float.IsFinite(lineHeight)||lineHeight<=0)
            throw new InvalidDataException("Sparse native line geometry overflow or invalid variance.");
        estimate=new(new LegacyLinePair(baseline,lineHeight),mean,std,halfBand,runs);
        return true;
    }
}
