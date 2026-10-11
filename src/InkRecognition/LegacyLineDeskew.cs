namespace InkRecognition;

public readonly record struct LegacyDeskewedLine(LegacyLinePair Line, float Slope, float Cosine, float Sine,
    float TranslationX, float TranslationY, InkPoint[][] Strokes);

/// <summary>Text 18016e570 postcallback prepared-line rotation and height normalization.</summary>
public static class LegacyLineDeskew
{
    public static LegacyDeskewedLine Apply(IReadOnlyList<InkPoint[]> preparedStrokes, LegacyLinePair line,
        float slope, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(preparedStrokes); cancellationToken.ThrowIfCancellationRequested();
        if (preparedStrokes.Count is < 1 or > 4096 || !float.IsFinite(line.Baseline) ||
            !float.IsFinite(line.Height) || !float.IsFinite(slope)) throw new ArgumentException("Invalid explicit line geometry.");
        float minX=float.MaxValue,maxX=-float.MaxValue; int count=0;
        foreach(var stroke in preparedStrokes)
        {
            if(stroke is null || stroke.Length==0 || stroke.Length>1_000_000-count) throw new ArgumentException("Expected bounded nonempty strokes.");
            count+=stroke.Length;
            foreach(var point in stroke)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if(!float.IsFinite(point.X)||!float.IsFinite(point.Y)) throw new ArgumentException("Nonfinite point.");
                minX=MathF.Min(minX,point.X);maxX=MathF.Max(maxX,point.X);
            }
        }
        float squaredSlope=slope*slope;
        float cosine=1f/(float)Math.Sqrt((double)squaredSlope+1.0);
        float sine=(float)Math.Sqrt((double)(1f-cosine*cosine)); if(slope>0)sine=-sine;
        float centerX=(maxX+minX)*.5f;
        float centerY=centerX*slope+line.Baseline;
        float translationY=(-sine*centerX-centerY*cosine)+centerY;
        float translationX=(centerY*sine-centerX*cosine)+centerX;
        float baseline=line.Baseline*cosine+sine*0f+translationY;
        float height=((line.Baseline+line.Height)*cosine+sine*0f+translationY)-baseline;
        float tolerance=MathF.Max(MathF.Abs(height)*2.3841858e-7f,1.1920929e-7f);
        if(MathF.Abs(height)<=tolerance)
        {
            float fallback=(maxX-minX)*.15f;float half=fallback*.5f;
            height=MathF.Max(fallback,.1f);baseline-=half;
        }
        if(!float.IsFinite(squaredSlope)||!float.IsFinite(cosine)||!float.IsFinite(sine)||
            !float.IsFinite(translationX)||!float.IsFinite(translationY)||!float.IsFinite(baseline)||!float.IsFinite(height))
            throw new InvalidDataException("Deskew geometry overflow.");
        var result=new InkPoint[preparedStrokes.Count][];
        for(int i=0;i<result.Length;i++)
        {
            result[i]=new InkPoint[preparedStrokes[i].Length];
            for(int j=0;j<result[i].Length;j++)
            {
                cancellationToken.ThrowIfCancellationRequested();var point=preparedStrokes[i][j];
                float x=point.Y*-sine+point.X*cosine+translationX;
                float y=point.Y*cosine+point.X*sine+translationY;
                if(!float.IsFinite(x)||!float.IsFinite(y)) throw new InvalidDataException("Deskew point overflow.");
                result[i][j]=new(x,y);
            }
        }
        // Native leaves the separately stored slope value intact after transforming the points.
        return new(new(baseline,height),slope,cosine,sine,translationX,translationY,result);
    }
}
