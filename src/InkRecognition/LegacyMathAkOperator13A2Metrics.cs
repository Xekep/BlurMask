using System.Numerics;
namespace InkRecognition;

internal sealed partial class LegacyMathAkOriginalInputContext
{
    //Literal24B source170 row, indexed by SAME fresh original stroke ID.
    private readonly record struct Source13A2StrokeMetric(float MinimumX,float MinimumY,
        float MaximumX,float MaximumY,float CenterX,float CenterY);

    //Private actual-source prepass of044000 before its297 invocation. This is
    //not a factory, graph issuer, sampler result, matcher request or candidate.
    //Future Run13A2 must call only while SAME fresh factory is still phase2.
    private Source13A2StrokeMetric[] Build13A2StrokeMetrics(CancellationToken token)
    {
        Live();token.ThrowIfCancellationRequested();
        var current=source13A0??throw new InvalidOperationException("Same live13A0 source required.");
        int count=input.StrokeCount;long points=0;
        if(count is<1 or>64||current.Points.Length!=count||current.OriginalIndices.Length!=count)
            throw new InvalidOperationException("Actual fresh source row header mismatch.");
        for(int s=0;s<count;s++)
        {
            token.ThrowIfCancellationRequested();
            if(input.ReadStrokeId(s)!=s||current.Points[s].Length<1||
                current.Points[s].Length!=current.OriginalIndices[s].Length)
                throw new InvalidOperationException("Original source ID or point map changed.");
            points=checked(points+current.Points[s].Length);
        }
        //Existing13A0 operation owns and has already reserved each12B rowpoint.
        //Use universal32MiB/12 bound, not the smaller at-max-input reservation.
        if(points!=current.PointCount||points>Source13A0LogicalPayloadLimit/12)
            throw new InvalidOperationException("Actual counted source header mismatch.");
        var result=new Source13A2StrokeMetric[count]; //24*S<=1536 logical values.
        for(int s=0;s<count;s++)
        {
            Live();token.ThrowIfCancellationRequested();var row=current.Points[s];
            float minX=row[0].X,minY=row[0].Y,maxX=minX,maxY=minY;
            Finite(minX);Finite(minY);
            for(int i=1;i<row.Length;i++)
            {
                token.ThrowIfCancellationRequested();float x=row[i].X,y=row[i].Y;Finite(x);Finite(y);
                //Native ordered ties: maxima take CURRENT, minima keep PRIOR.
                if(maxX<=x)maxX=x;if(maxY<=y)maxY=y;
                if(!(minX<=x))minX=x;if(!(minY<=y))minY=y;
            }
            double length=0,weightedX=0,weightedY=0;
            for(int i=1;i<row.Length;i++)
            {
                token.ThrowIfCancellationRequested();
                float dx=row[i].X-row[i-1].X,dy=row[i].Y-row[i-1].Y;Finite(dx);Finite(dy);
                float xx=dx*dx,yy=dy*dy;float squared=xx+yy;Finite(squared);
                double edge=Math.Sqrt((double)squared);
                float sumX=row[i-1].X+row[i].X,sumY=row[i-1].Y+row[i].Y;Finite(sumX);Finite(sumY);
                length+=edge;weightedX+=(double)sumX*edge;weightedY+=(double)sumY*edge;
            }
            float centerX=row[0].X,centerY=row[0].Y;
            if(length>0){centerX=(float)((weightedX*.5)/length);centerY=(float)((weightedY*.5)/length);}
            Finite(centerX);Finite(centerY);
            result[input.ReadStrokeId(s)]=new(minX,minY,maxX,maxY,centerX,centerY);
        }
        Live();token.ThrowIfCancellationRequested();return result;
    }
}
