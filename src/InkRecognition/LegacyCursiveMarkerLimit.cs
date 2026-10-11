namespace InkRecognition;

/// <summary>Explicit191430 profile and upstream closed-stroke category limit.</summary>
public readonly record struct LegacyCursiveMarkerLimitOptions(int MinimumGate,float WidthScale,int? ClosedStrokeCategoryLimit)
{
    /// <summary>Russian297 version106 has gate6 and width multiplier4. No upstream limit is inferred.</summary>
    public static LegacyCursiveMarkerLimitOptions RussianVersion106(int? closedStrokeCategoryLimit=null)=>new(6,4,closedStrokeCategoryLimit);
}

/// <summary>
/// Complete native191430 final marker-limiting stage for a whole prepared ink
/// buffer. Original raw strokes supply widths and endpoint distances; inserted
/// prepared records supply tags and segment weights. Runtime limits are explicit.
/// </summary>
public static class LegacyCursiveMarkerLimit
{
    public static LegacyCursivePreparedInk Apply(LegacyCursivePreparedInk ink,IReadOnlyList<InkStroke> original,
        float lineHeight,LegacyCursiveMarkerLimitOptions options,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(ink);ArgumentNullException.ThrowIfNull(original);
        cancellationToken.ThrowIfCancellationRequested();
        if(original.Count!=ink.StrokePointCounts.Length)throw new ArgumentException("Original and prepared physical stroke counts differ.",nameof(original));
        if(!float.IsFinite(lineHeight) || lineHeight<=0 || !float.IsFinite(options.WidthScale) || options.WidthScale<0 ||
            options.MinimumGate is <0 or >255 || options.ClosedStrokeCategoryLimit is <0 or >4096)
            throw new ArgumentException("Marker limits require finite positive line height and bounded nonnegative settings.");
        var raw=new (float Width,double EndpointDistance)[original.Count];int total=0;
        for(int stroke=0;stroke<original.Count;stroke++)
        {
            var points=original[stroke]?.Points ?? throw new ArgumentException("Null original physical stroke.",nameof(original));
            if(points.Count is <1 or >4096 || total>4096-points.Count)throw new ArgumentException("Original ink exceeds4096 points.",nameof(original));
            total+=points.Count;
            float minimum=float.MaxValue,maximum=-float.MaxValue;
            foreach(var p in points)
            {
                if(!float.IsFinite(p.X) || !float.IsFinite(p.Y))throw new ArgumentException("Original point coordinates must be finite.",nameof(original));
                minimum=Math.Min(minimum,p.X);maximum=Math.Max(maximum,p.X);
            }
            float dx=points[0].X-points[^1].X,dy=points[0].Y-points[^1].Y;
            //191430 differs from_hypotf: square/add in float first, then
            // promote that rounded sum to double for sqrt and comparison.
            float squared=dy*dy+dx*dx;
            raw[stroke]=(maximum-minimum,Math.Sqrt((double)squared));
        }
        var records=ink.Points.ToArray();var tags=records.Select(p=>p.Tag).ToArray();int start=0;
        for(int stroke=0;stroke<original.Count;stroke++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int end=start+ink.StrokePointCounts.Span[stroke];
            if(tags[start]==1)
            {
                int markerCount=0;for(int i=start;i<end;i++)if(Visible(tags[i]))markerCount++;
                if(markerCount>options.MinimumGate)
                {
                    float scaled=(raw[stroke].Width*options.WidthScale)/lineHeight;
                    if(!float.IsFinite(scaled) || scaled>=int.MaxValue)throw new ArgumentException("Marker width limit exceeds bounded integer arithmetic.");
                    int target=(int)scaled;
                    if(options.ClosedStrokeCategoryLimit is int closed && raw[stroke].EndpointDistance<(double)(lineHeight*.8f) && closed<target)target=closed;
                    if(target<markerCount)
                    {
                        var entries=new List<Entry>(markerCount){new(start,records[start].SegmentLength)};
                        for(int i=start+1;i<end;i++)
                        {
                            if(Visible(tags[i]))entries.Add(new(i,records[i].SegmentLength));
                            // Native preinitializes each new entry with this
                            // length, then adds it again in the ordinary loop.
                            var last=entries[^1];entries[^1]=last with{Weight=last.Weight+records[i].SegmentLength};
                        }
                        int attempts=markerCount-target;
                        for(int attempt=0;attempt<attempts;attempt++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            float best=float.MaxValue;int selected=-1;
                            for(int i=1;i<entries.Count;i++)
                            {
                                float right=entries[i].Weight,left=entries[i-1].Weight;
                                float cost=CombineUnchecked(left,right);
                                int tag=tags[entries[i].Point];
                                if(cost<best && tag!=7 && tag!=4){best=cost;selected=i;}
                            }
                            if(selected<0)continue;
                            tags[entries[selected].Point]=0;
                            entries.RemoveAt(selected);
                            entries[selected-1]=entries[selected-1] with{Weight=best};
                        }
                    }
                }
            }
            start=end;
        }
        return ink.WithTags(tags);
    }

    /// <summary>Native191430 min+0.1max cost: double arithmetic, one final float rounding.</summary>
    public static float CombineWeights(float left,float right)
    {
        if(!float.IsFinite(left) || !float.IsFinite(right) || left<0 || right<0)
            throw new ArgumentException("Merge weights must be finite and nonnegative.");
        return CombineUnchecked(left,right);
    }

    private static float CombineUnchecked(float left,float right)=>right<=left ? (float)((double)right+(double)left*.1) : (float)((double)right*.1+(double)left);

    private static bool Visible(int tag)=>tag!=0 && unchecked((uint)(tag-11))>3;
    private readonly record struct Entry(int Point,float Weight);
}
