namespace InkRecognition;

/// <summary>Native1908c0 geometry override with the confirmed Russian106 optional-neural flag disabled.</summary>
public static class LegacyCursiveStrokeOverride
{
    public static LegacyCursivePreparedInk ApplyRussianVersion106(LegacyCursivePreparedInk ink,
        IReadOnlyList<InkStroke> original,int strokeIndex,float? baseline,float lineHeight,
        int firstSelectedStroke,int lastSelectedStroke,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(ink);ArgumentNullException.ThrowIfNull(original);
        cancellationToken.ThrowIfCancellationRequested();
        if(original.Count!=ink.StrokePointCounts.Length || (uint)strokeIndex>=(uint)original.Count ||
            firstSelectedStroke<0 || firstSelectedStroke>strokeIndex || lastSelectedStroke<strokeIndex || lastSelectedStroke>=original.Count)
            throw new ArgumentException("Override requires corresponding raw/prepared strokes and an inclusive selected range.");
        if(!float.IsFinite(lineHeight) || lineHeight<=0 || baseline is float b && !float.IsFinite(b))
            throw new ArgumentException("Override requires finite positive height and an explicit finite or unknown baseline.");
        var raw=original[strokeIndex]?.Points ?? throw new ArgumentException("Null raw physical stroke.");
        if(raw.Count is <1 or >4096)throw new ArgumentException("Raw physical stroke count is out of bounds.");
        float minX=float.MaxValue,maxX=-float.MaxValue,minY=float.MaxValue,maxY=-float.MaxValue;
        foreach(var p in raw)
        {
            if(!float.IsFinite(p.X) || !float.IsFinite(p.Y))throw new ArgumentException("Raw override coordinates must be finite.");
            minX=Math.Min(minX,p.X);maxX=Math.Max(maxX,p.X);minY=Math.Min(minY,p.Y);maxY=Math.Max(maxY,p.Y);
        }
        if(baseline is not float known || known==float.MaxValue || !(lineHeight<maxX-minX && maxY-minY<lineHeight*.5f && maxY<known-lineHeight*.5f))return ink;
        int[] starts=new int[original.Count+1];for(int i=0;i<original.Count;i++)starts[i+1]=starts[i]+ink.StrokePointCounts.Span[i];
        var points=ink.Points.Span;var intersections=new List<(InkPoint Point,int Segment)>(10);
        float farthest=0;int selectedFirst=0,selectedSecond=0;
        for(int other=firstSelectedStroke;other<=lastSelectedStroke;other++)
        {
            cancellationToken.ThrowIfCancellationRequested();if(other==strokeIndex)continue;
            for(int i=starts[other];i<starts[other+1]-1;i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                for(int j=starts[strokeIndex];j<starts[strokeIndex+1]-1;j++)
                {
                    if(intersections.Count==10)continue;
                    if(!LegacyCursiveLoopCuts.TryIntersection(new(points[i].X,points[i].Y),new(points[i+1].X,points[i+1].Y),
                        new(points[j].X,points[j].Y),new(points[j+1].X,points[j+1].Y),out var cross))continue;
                    intersections.Add((cross,j));
                    // Native rescans all ordered pairs after every admitted intersection.
                    for(int a=0;a<intersections.Count;a++)for(int c=0;c<intersections.Count;c++)
                    {
                        if(a==c)continue;
                        float dy=intersections[a].Point.Y-intersections[c].Point.Y,dx=intersections[a].Point.X-intersections[c].Point.X;
                        float squared=dy*dy+dx*dx;
                        if(farthest<squared){farthest=squared;selectedFirst=a;selectedSecond=c;}
                    }
                }
            }
        }
        if(intersections.Count>1 && lineHeight*lineHeight*.25f<farthest)
        {
            int low=intersections[Math.Min(selectedFirst,selectedSecond)].Segment;
            int high=intersections[Math.Max(selectedFirst,selectedSecond)].Segment;
            float total=0;for(int i=low;i<=high;i++)total+=points[i].SegmentLength;
            float midpoint=total*.5f,distance=0;
            for(int i=low;i<high;i++)
            {
                distance+=points[i].SegmentLength;
                if(midpoint<distance)
                {
                    float fraction=(midpoint-(distance-points[i].SegmentLength))/points[i].SegmentLength;
                    return ink.Insert(strokeIndex,i+1-starts[strokeIndex],new(fraction*points[i].DeltaX+points[i].X,fraction*points[i].DeltaY+points[i].Y),2,cancellationToken);
                }
            }
            return ink;
        }
        var tags=points.ToArray().Select(p=>p.Tag).ToArray();
        for(int i=starts[strokeIndex]+1;i<starts[strokeIndex+1];i++)tags[i]=0;
        return ink.WithTags(tags);
    }
}
