namespace InkRecognition;

/// <summary>Prepared numeric consumer of15F050's grouped ordinary horizontal,
/// coordinate-mode2 branch. Physical associations are?1 and no direction/boundary
/// categories are present. It implements interval insertion, not an X-coordinate
/// sort. Actual provider groups and current points are explicit prepared inputs;
/// source E8/guide lifecycle and provider state are not synthesized.</summary>
public static class LegacyNativeUnknownPhysicalIntervalOrder
{
    public static int[] Order(IReadOnlyList<InkStroke> currentPhysicalPoints,
        IReadOnlyList<ReadOnlyMemory<int>> actualProviderGroups,double primarySourceScale,
        float sourceOrderTolerance,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(currentPhysicalPoints);ArgumentNullException.ThrowIfNull(actualProviderGroups);
        cancellationToken.ThrowIfCancellationRequested();
        int count=currentPhysicalPoints.Count;
        if(count is <1 or >256||actualProviderGroups.Count<1||actualProviderGroups.Count>count||
            !double.IsFinite(primarySourceScale)||primarySourceScale<=0||primarySourceScale>1_000_000||
            !float.IsFinite(sourceOrderTolerance)||sourceOrderTolerance<0||sourceOrderTolerance>100)
            throw new ArgumentException("Bounded ordinary unknown physical groups/current points/source tolerance required.");
        var left=new float[count];var right=new float[count];int total=0;
        for(int i=0;i<count;i++)
        {
            var points=currentPhysicalPoints[i]?.Points??throw new ArgumentException("Null current physical source.");
            if(points.Count<1||total>4096-points.Count)throw new ArgumentException("Bounded current source points required.");total+=points.Count;
            left[i]=float.MaxValue;right[i]=-float.MaxValue;
            foreach(var p in points)
            {
                if(!float.IsFinite(p.X)||!float.IsFinite(p.Y)||MathF.Abs(p.X)>1_000_000||MathF.Abs(p.Y)>1_000_000)
                    throw new ArgumentException("Finite bounded current physical points required.");
                if(p.X<left[i])left[i]=p.X;if(right[i]<p.X)right[i]=p.X;
            }
        }
        bool[] seen=new bool[count];var result=new List<int>(count);double gap=sourceOrderTolerance*primarySourceScale;
        foreach(var group in actualProviderGroups)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if(group.Length<1)throw new ArgumentException("Empty provider group.");
            int begin=result.Count;int[] ids=group.ToArray();Array.Sort(ids);
            foreach(int id in ids)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if((uint)id>=(uint)count||seen[id])throw new ArgumentException("Invalid/duplicate actual provider physical ID.");seen[id]=true;
                int scan=begin;
                //15FBA1..15FD18 uses double source tolerance/primary scale.
                //Freshassociation?1 keeps the exact X box (no0.3 contraction).
                while(scan<result.Count&&!(gap+(double)right[id]<(double)left[result[scan]]))scan++;
                if(scan==result.Count){result.Add(id);continue;}
                int at=scan-1;
                //15FD40 walks backwards by strict float left-edge comparison.
                while(at>=begin&&left[id]<left[result[at]])at--;
                if(at>=begin)
                {
                    float anchorRight=right[result[at]];
                    //15FDC0/15FE1C advances under one FIXED anchor right edge;
                    //touching intervals stop because equality is not overlap.
                    while(at<scan-1&&left[result[at+1]]<anchorRight)at++;
                }
                result.Insert(at+1,id);
            }
        }
        if(seen.Any(v=>!v))throw new ArgumentException("Provider groups omit physical strokes.");
        return result.ToArray();
    }
}
