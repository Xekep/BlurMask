namespace InkRecognition;

/// <summary>Complete native190350 baseline/loop-exit cut stage on prepared ink.</summary>
public static class LegacyCursiveBaselineCuts
{
    /// <summary>Actual RU297 version106 state+1c.</summary>
    public const float RussianGapRadius = .35f;

    /// <summary>
    /// Consume explicit line metrics and preceding native tags. Null baseline
    /// means the native FLT_MAX unknown-baseline branch. This method does not
    /// infer line metrics or replace other191950 geometry stages.
    /// </summary>
    public static LegacyCursivePreparedInk Apply(LegacyCursivePreparedInk ink,int strokeIndex,
        float? baseline,float lineHeight,float gapRadius,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(ink);
        cancellationToken.ThrowIfCancellationRequested();
        if((uint)strokeIndex>=(uint)ink.StrokePointCounts.Length)throw new ArgumentOutOfRangeException(nameof(strokeIndex));
        if(baseline.HasValue && !float.IsFinite(baseline.Value) || !float.IsFinite(lineHeight) || lineHeight<=0 ||
            !float.IsFinite(gapRadius) || gapRadius<0 || !float.IsFinite(lineHeight*gapRadius))
            throw new ArgumentException("Baseline cuts require finite line metrics and radius.");
        if(baseline==float.MaxValue)baseline=null;
        int start=0;for(int s=0;s<strokeIndex;s++)start+=ink.StrokePointCounts.Span[s];
        var records=ink.Points.ToArray();var tags=records.Select(p=>p.Tag).ToArray();
        bool pending=false;int pendingPoint=0,anchor=start;
        for(int i=start;i<start+ink.StrokePointCounts.Span[strokeIndex];i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int tag=tags[i];
            if(tag==12){pending=true;pendingPoint=i;continue;}
            if(tag==0 || Hidden(tag))continue;
            if(pending && !Hidden(tags[i-1]))
            {
                if(tag is 6 or 8)
                {
                    if(baseline.HasValue)
                    {
                        float y=baseline.Value;
                        if(tags[anchor]==9)
                        {
                            if(Math.Abs(records[anchor].Y-y)<Math.Abs(records[pendingPoint].Y-y))
                            {anchor=i;pending=false;continue;}
                            tags[anchor]=14;
                        }
                        if(records[pendingPoint].Y<=y)
                        {
                            float distance=0;
                            // Native sums this branch in reverse point order.
                            for(int j=i-1;j>=pendingPoint;j--)distance+=records[j].SegmentLength;
                            if(lineHeight*gapRadius<distance)tags[pendingPoint]=2;
                        }
                        else
                        {
                            float distance=0;int candidate=i;
                            while(pendingPoint<candidate)
                            {
                                distance+=records[candidate-1].SegmentLength;
                                if(lineHeight*gapRadius<distance)break;
                                candidate--;
                            }
                            if(records[candidate].Y<=y)
                            {
                                for(int j=pendingPoint;j<i;j++)if(records[j].Y<y){tags[j]=2;break;}
                            }
                            else tags[candidate]=2;
                        }
                    }
                }
                else if(tag==11)
                {
                    float total=0;for(int j=pendingPoint;j<i;j++)total+=records[j].SegmentLength;
                    float distance=0;
                    for(int j=pendingPoint;j<i;j++)
                    {
                        distance+=records[j].SegmentLength;
                        if(total*.5f<distance)
                        {
                            float fraction=(total*.5f-(distance-records[j].SegmentLength))/records[j].SegmentLength;
                            var p=records[j];
                            var point=new InkPoint(fraction*p.DeltaX+p.X,fraction*p.DeltaY+p.Y);
                            ink=ink.WithTags(tags).Insert(strokeIndex,j+1-start,point,2,cancellationToken);
                            records=ink.Points.ToArray();tags=records.Select(r=>r.Tag).ToArray();i++;
                            break;
                        }
                    }
                }
                else if((tag is 7 or 4) && records[i].X>records[pendingPoint].X)
                {
                    float distance=0;for(int j=pendingPoint;j<i;j++)distance+=records[j].SegmentLength;
                    if(lineHeight<distance)tags[pendingPoint]=2;
                }
            }
            anchor=i;pending=false;
        }
        return ink.WithTags(tags);
    }

    private static bool Hidden(int tag)=>unchecked((uint)(tag-13))<2;
}
