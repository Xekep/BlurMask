namespace InkRecognition;

/// <summary>Recovered native191950 filters surrounding the separate190350 baseline stage.</summary>
public static class LegacyCursiveControllerFilters
{
    public const float RussianHighAnchorMargin=.5f;
    public const float RussianLoopDropThreshold=.2f;

    public static LegacyCursivePreparedInk BeforeBaseline(LegacyCursivePreparedInk ink,int strokeIndex,CancellationToken cancellationToken=default)
    {
        var (points,tags,start,end)=Prepare(ink,strokeIndex,cancellationToken);
        bool pending=false;int anchor=0;
        for(int i=start;i<end-1;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();int tag=tags[i];
            if(tag is 4 or 7){pending=true;anchor=i;continue;}
            if(tag is 0 or 13 or 14)continue;
            if(pending && tag==11 && points[anchor].Y-points[i].Y>0)
            {
                float dy=points[anchor].Y-points[i].Y,dx=points[i].X-points[anchor].X;
                for(int j=anchor+1;j<=i;j++)
                {
                    float x=points[j].X-points[anchor].X;
                    bool outside=dx==0 ? x<0 : x==0 ? dx>0 : (points[anchor].Y-points[j].Y)/x>dy/dx;
                    if(outside){tags[i]=2;break;}
                }
            }
            pending=false;
        }
        pending=false;anchor=0;
        for(int i=start;i<end;i++)
        {
            int tag=tags[i];
            if(tag==11)
            {
                if(!pending || i==0 || points[i-1].DeltaX<=0){pending=true;anchor=i;}
                else{if(anchor!=0)tags[anchor]=0;tags[i]=2;pending=false;anchor=0;}
            }
            else if(tag is 12 or 1)pending=false;
        }
        return ink.WithTags(tags);
    }

    public static LegacyCursivePreparedInk AfterBaselineRussianVersion106(LegacyCursivePreparedInk ink,int strokeIndex,float? baseline,float lineHeight,CancellationToken cancellationToken=default)
    {
        if(!float.IsFinite(lineHeight) || lineHeight<=0 || baseline is float b && !float.IsFinite(b))throw new ArgumentException("Controller filters require finite positive line height and explicit/unknown baseline.");
        var (points,tags,start,end)=Prepare(ink,strokeIndex,cancellationToken);
        int anchor=start;float anchorY=points[start].Y;
        for(int i=start+1;i<end;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();if(tags[i] is 0 or 1 or 13 or 14)continue;
            if(baseline is float known && known!=float.MaxValue && points[i].Y<0 && known+lineHeight*RussianHighAnchorMargin<anchorY &&
                points[anchor].X<points[i].X && tags[anchor] is 4 or 5 or 7)
            {
                for(int j=anchor;j<i;j++)if(points[j].Y<known)
                {
                    // With a positive margin, the high anchor itself cannot satisfy this test.
                    tags[j-1]=2;tags[anchor]=0;break;
                }
            }
            anchor=i;anchorY=points[i].Y;
        }
        float previousY=points[start].Y;
        for(int i=start+1;i<end-1;i++)
        {
            if(tags[i] is 0 or 1 or 13 or 14)continue;
            if(tags[i]==11 && tags[i+1]==13 && lineHeight*RussianLoopDropThreshold<points[i].Y-previousY)tags[i]=2;
            previousY=points[i].Y;
        }
        return ink.WithTags(tags);
    }

    private static (LegacyCursivePreparedPoint[] Points,int[] Tags,int Start,int End) Prepare(LegacyCursivePreparedInk ink,int strokeIndex,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(ink);token.ThrowIfCancellationRequested();
        if((uint)strokeIndex>=(uint)ink.StrokePointCounts.Length)throw new ArgumentOutOfRangeException(nameof(strokeIndex));
        int start=0;for(int i=0;i<strokeIndex;i++)start+=ink.StrokePointCounts.Span[i];
        var points=ink.Points.ToArray();return(points,points.Select(p=>p.Tag).ToArray(),start,start+ink.StrokePointCounts.Span[strokeIndex]);
    }
}
