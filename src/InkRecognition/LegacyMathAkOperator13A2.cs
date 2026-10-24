namespace InkRecognition;

internal sealed partial class LegacyMathAkOriginalInputContext
{
    //PRIVATE continuation only; no factory can presently issue it from a
    //finished/historical context and no caller arrays/markers are accepted.
    private Source13A2Output Run13A2Sampler(CancellationToken token)
    {
        Live();token.ThrowIfCancellationRequested();
        if(source13A0 is null||prefix is null)throw new InvalidOperationException("Same live13A0 source required.");
        var budget=new Source13A2Budget(this);
        try
        {
            budget.Charge(24L*input.StrokeCount,checked(2L*source13A0.PointCount),token);
            var metrics=Build13A2StrokeMetrics(token);
            var ink=SamplerFrame.Create(this,budget,token);var policy=ink.Policy;
            float height=prefix.EstimatedScale;Finite(height);
            if(height<0)throw new InvalidOperationException("Owned source scale must be nonnegative.");
            var horizontal=new LegacyCursiveHorizontalCutSettings(policy.LooseSlope,policy.StrongSlope,
                policy.MinimumArc,policy.LongArc,policy.GapRadius,policy.ReversalSpacing,policy.KeepOneAnchor);
            for(int stroke=0;ink.Test(stroke<input.StrokeCount,token);stroke++)
            {
                ink=Mark13A2Vertical(ink,stroke,token);
                ink=SamplerLoop.Apply(ink,stroke,height,policy.LoopSpacing,token);
                if(policy.HorizontalPass)ink=SamplerHorizontal.Apply(ink,stroke,height,horizontal,false,token);
                if(policy.ReversePass)ink=SamplerHorizontal.Apply(ink,stroke,height,horizontal,true,token);
                ink=SamplerFilters.BeforeBaseline(ink,stroke,token);
                //Unknown baseline does NOT erase the loop-exit branches here.
                ink=SamplerBaseline.Apply(ink,stroke,null,height,policy.GapRadius,token);
                ink=SamplerFilters.AfterBaselineUnknown(ink,stroke,null,height,token);
                Override13A2UnknownBaseline(ink,stroke,token);
            }
            ink=Prune13A2(ink,height,token);
            ink=Limit13A2(ink,height,token);
            Live();token.ThrowIfCancellationRequested();return ink.Complete(metrics);
        }
        catch{phase=-1;throw;} //Same claim remains consumed; no retry/reset.
    }
    private static SamplerFrame Mark13A2Vertical(SamplerFrame ink,int stroke,CancellationToken token)
    {
        var records=ink.CopyRecords();var tags=ink.CopyTags(records);int start=0;
        for(int s=0;ink.Test(s<stroke,token);s++)start+=ink.StrokePointCounts.Span[s];
        int direction=0,end=start+ink.StrokePointCounts.Span[stroke];
        for(int i=start;ink.Test(i<end-1,token);i++)
        {
            if(records[i].DeltaY<0){if(direction==-1)tags[i]=7;direction=1;}
            else if(records[i].DeltaY>0){if(direction==1&&i>0&&records[i-1].DeltaX>0)tags[i]=8;direction=-1;}
        }
        return ink.WithTags(tags);
    }
    private static void Override13A2UnknownBaseline(SamplerFrame ink,int stroke,CancellationToken token)
    {
        //073F00 scans this bbox then its leading baseline!=FLT_MAX test is
        //false. No synthetic known baseline/neural extension is substituted.
        var row=ink.OriginalRow(stroke);float minX=row[0].X,minY=row[0].Y,maxX=minX,maxY=minY;
        for(int i=1;ink.Test(i<row.Length,token);i++)
        {var p=row[i];if(maxX<=p.X)maxX=p.X;if(maxY<=p.Y)maxY=p.Y;if(!(minX<=p.X))minX=p.X;if(!(minY<=p.Y))minY=p.Y;}
        token.ThrowIfCancellationRequested();
    }
    private static SamplerFrame Prune13A2(SamplerFrame ink,float height,CancellationToken token)
    {
        var p=ink.CopyRecords();var tags=ink.CopyTags(p);float radius=height*ink.Policy.PruneRadius;Finite(radius);
        for(int i=0;ink.Test(i<tags.Length,token);i++)
        {
            if(tags[i]!=1)continue;
            if(i>0)
            {
                if(tags[i-1]!=1)tags[i-1]=0;float distance=0;
                for(int j=i-2;ink.Test(j>=0,token);j--)
                {distance+=p[j].SegmentLength;if(distance>=radius||tags[j]==1)break;tags[j]=0;}
            }
            float forward=0;
            for(int j=i+1;ink.Test(j<tags.Length,token);j++)
            {forward+=p[j-1].SegmentLength;if(forward>=radius||tags[j]==1)break;tags[j]=0;}
        }
        if(tags[^1]!=1)tags[^1]=0;float tail=0;
        for(int j=tags.Length-2;ink.Test(j>=0,token);j--)
        {tail+=p[j].SegmentLength;if(tail>=radius||tags[j]==1)break;tags[j]=0;}
        return ink.WithTags(tags);
    }
    private readonly record struct Source13A2LimitEntry(int Point,float Weight);
    private static SamplerFrame Limit13A2(SamplerFrame ink,float height,CancellationToken token)
    {
        var records=ink.CopyRecords();var tags=ink.CopyTags(records);int start=0;
        for(int stroke=0;ink.Test(stroke<ink.StrokePointCounts.Length,token);stroke++)
        {
            int end=start+ink.StrokePointCounts.Span[stroke];
            if(tags[start]==1)
            {
                int markerCount=0;for(int i=start;ink.Test(i<end,token);i++)if(Visible13A2(tags[i]))markerCount++;
                if(markerCount>ink.Policy.MinimumMarkerGate)
                {
                    var raw=ink.OriginalRow(stroke);float min=raw[0].X,max=min;
                    for(int i=1;ink.Test(i<raw.Length,token);i++){if(max<=raw[i].X)max=raw[i].X;if(!(min<=raw[i].X))min=raw[i].X;}
                    float scaled=((max-min)*ink.Policy.WidthScale)/height;
                    //Native CVTTSS2SI and signed32 subtraction, including0/0
                    //or overflow: do not invent positive-height admission.
                    int target=scaled>=-2147483648f&&scaled<2147483648f?(int)scaled:int.MinValue;
                    if(target<markerCount)
                    {
                        ink.Reserve(8L*markerCount,markerCount);
                        var entries=new Source13A2LimitEntry[markerCount];int size=1;
                        entries[0]=new(start,records[start].SegmentLength);
                        for(int i=start+1;ink.Test(i<end,token);i++)
                        {
                            if(Visible13A2(tags[i]))entries[size++]=new(i,records[i].SegmentLength);
                            var last=entries[size-1];entries[size-1]=last with{Weight=last.Weight+records[i].SegmentLength};
                        }
                        if(size!=markerCount)throw new InvalidOperationException("Owned limiter marker count changed.");
                        int attempts=unchecked(markerCount-target);
                        for(int attempt=0;ink.Test(attempt<attempts,token);attempt++)
                        {
                            float best=float.MaxValue;int selected=-1;
                            for(int i=1;ink.Test(i<size,token);i++)
                            {
                                float leftWeight=entries[i-1].Weight,rightWeight=entries[i].Weight;
                                float cost=rightWeight<=leftWeight?(float)((double)rightWeight+(double)leftWeight*.1):
                                    (float)((double)rightWeight*.1+(double)leftWeight);
                                int tag=tags[entries[i].Point];if(cost<best&&tag!=7&&tag!=4){best=cost;selected=i;}
                            }
                            if(selected<0)continue;
                            tags[entries[selected].Point]=0;
                            ink.Reserve(0,size-selected-1);
                            entries.AsSpan(selected+1,size-selected-1).CopyTo(entries.AsSpan(selected));size--;
                            var left=entries[selected-1];entries[selected-1]=left with{Weight=best};
                        }
                    }
                }
            }
            start=end;
        }
        return ink.WithTags(tags);
    }
}
