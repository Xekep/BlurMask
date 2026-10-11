namespace InkRecognition;

/// <summary>
/// Restored18e1f0 loop/intersection stage,18db40 finite-segment intersection,
/// and18df60 double insertion through the owned prepared ink buffer.
/// This stage consumes prepared tags; it is not the whole191950 controller.
/// </summary>
public static class LegacyCursiveLoopCuts
{
    /// <summary>Actual RU sampler297 version106 state+30.</summary>
    public const float RussianReversalSpacing = .1f;

    public static LegacyCursivePreparedInk Apply(LegacyCursivePreparedInk ink, int strokeIndex,
        float lineHeight, float reversalSpacing, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(ink);
        cancellationToken.ThrowIfCancellationRequested();
        if((uint)strokeIndex >= (uint)ink.StrokePointCounts.Length)throw new ArgumentOutOfRangeException(nameof(strokeIndex));
        if(!float.IsFinite(lineHeight) || lineHeight <= 0 || !float.IsFinite(reversalSpacing) || reversalSpacing < 0 ||
            !float.IsFinite(lineHeight*reversalSpacing))throw new ArgumentException("Loop spacing requires finite nonnegative values.");
        int start=0;
        for(int stroke=0;stroke<strokeIndex;stroke++)start+=ink.StrokePointCounts.Span[stroke];
        var records=ink.Points.ToArray();
        for(int i=start;i<start+ink.StrokePointCounts.Span[strokeIndex]-1;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int end=start+ink.StrokePointCounts.Span[strokeIndex];
            var a=records[i];var b=records[i+1];
            for(int j=i+2;j<end-1;j++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var c=records[j];var d=records[j+1];
                if(!BoxesOverlap(a,b,c,d) || !TryIntersection(new(a.X,a.Y),new(b.X,b.Y),new(c.X,c.Y),new(d.X,d.Y),out var intersection))continue;
                int xDirection=0,yDirection=0,xReversals=0,yReversals=0;
                float lastX=-1,lastY=-1,turn=0;
                for(int k=i;k<j;k++)
                {
                    var p=records[k];var next=records[k+1];
                    if(p.SegmentLength>0 && next.SegmentLength>0)
                        turn+=(p.DeltaX*next.DeltaY-p.DeltaY*next.DeltaX)/(p.SegmentLength*next.SegmentLength);
                    Count(p.DeltaX,p.X,lineHeight*reversalSpacing,ref xDirection,ref lastX,ref xReversals);
                    Count(p.DeltaY,p.Y,lineHeight*reversalSpacing,ref yDirection,ref lastY,ref yReversals);
                }
                if(xReversals>2 || yReversals>2)continue;
                if(records.Length>4094)throw new ArgumentException("Loop insertion exceeds4096 prepared points.");
                //18df60 reserves two points and inserts the same intersection
                // before old i+1 and old j+1. End-first insertion keeps both
                // predecessor source-map identities exactly as its memmoves.
                ink=ink.Insert(strokeIndex,j+1-start,intersection,12,cancellationToken)
                    .Insert(strokeIndex,i+1-start,intersection,11,cancellationToken);
                records=ink.Points.ToArray();
                int innerStart=i+2,innerEnd=j+2;
                bool nested=false;
                for(int k=innerStart;k<innerEnd;k++)if(IsLoopTag(records[k].Tag)){nested=true;break;}
                for(int k=innerStart;k<innerEnd;k++)
                {
                    int old=records[k].Tag,nextTag=old;
                    if(turn<0)
                    {
                        if(old==0 || !nested && (old is 7 or 8))nextTag=13;
                    }
                    else if(nested)
                    {
                        if(old==0)nextTag=14;
                    }
                    else nextTag=old==7 ? 9:14;
                    records[k]=records[k] with{Tag=nextTag};
                }
                ink=ink.WithTags(records.Select(p=>p.Tag).ToArray());
                // Native advances initial point by two after the double insert,
                // then the outer loop increments once more.
                i+=2;
                break;
            }
        }
        return ink;
    }

    /// <summary>
    /// Native18db40 float32 determinant and inclusive segment boxes. Parallel
    /// or nearly parallel segments below1e-10 are rejected, without epsilon
    /// enlargement of the boxes. Output coordinates use the native operation
    /// order. Nonfinite input is rejected at this reusable API boundary.
    /// </summary>
    public static bool TryIntersection(InkPoint firstStart,InkPoint firstEnd,InkPoint secondStart,
        InkPoint secondEnd,out InkPoint point)
    {
        point=default;
        Validate(firstStart);Validate(firstEnd);Validate(secondStart);Validate(secondEnd);
        float a=firstEnd.X-firstStart.X,b=secondEnd.X-secondStart.X;
        float c=firstStart.Y-firstEnd.Y,d=secondStart.Y-secondEnd.Y;
        float e=-c*firstStart.X-a*firstStart.Y,g=-d*secondStart.X-b*secondStart.Y;
        float determinant=b*c-d*a;
        if(!(Math.Abs(determinant)>=1e-10f))return false;
        float x=(g*a-b*e)/determinant,y=(d*e-g*c)/determinant;
        point=new(x,y);
        return Inside(x,firstStart.X,firstEnd.X) && Inside(y,firstStart.Y,firstEnd.Y) &&
            Inside(x,secondStart.X,secondEnd.X) && Inside(y,secondStart.Y,secondEnd.Y);
    }

    private static bool Inside(float value,float first,float second)=>Math.Min(first,second)<=value && value<=Math.Max(first,second);
    private static void Validate(InkPoint p)
    {
        if(!float.IsFinite(p.X) || !float.IsFinite(p.Y))throw new ArgumentException("Intersection requires finite coordinates.");
    }
    private static bool IsLoopTag(int tag)=>unchecked((uint)(tag-11))<4;
    private static bool BoxesOverlap(LegacyCursivePreparedPoint a,LegacyCursivePreparedPoint b,
        LegacyCursivePreparedPoint c,LegacyCursivePreparedPoint d)=>
        Math.Min(a.X,b.X)<=Math.Max(c.X,d.X) && Math.Min(c.X,d.X)<=Math.Max(a.X,b.X) &&
        Math.Min(a.Y,b.Y)<=Math.Max(c.Y,d.Y) && Math.Min(c.Y,d.Y)<=Math.Max(a.Y,b.Y);

    private static void Count(float delta,float coordinate,float minimumDistance,ref int direction,
        ref float previousPosition,ref int count)
    {
        if(delta>0)
        {
            if(direction>0 && (previousPosition==-1 || minimumDistance<Math.Abs(coordinate-previousPosition)))
            {previousPosition=coordinate;count++;}
            direction=-1;
        }
        else if(delta<0)
        {
            if(direction<0 && (previousPosition==-1 || minimumDistance<Math.Abs(coordinate-previousPosition)))
            {previousPosition=coordinate;count++;}
            direction=1;
        }
    }
}
