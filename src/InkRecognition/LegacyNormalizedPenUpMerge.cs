namespace InkRecognition;

public sealed record LegacyNormalizedPenUpMergeResult(IReadOnlyList<LegacyInkStroke> Strokes,
    ReadOnlyMemory<int> RetainedBoundaryIndices);

/// <summary>Native1b4bd0 pen-up reduction with descending gap quicksort1b4ae0.
/// All points remain in source order; only physical stroke boundaries change.</summary>
public static class LegacyNormalizedPenUpMerge
{
    public static LegacyNormalizedPenUpMergeResult Merge(IReadOnlyList<LegacyInkStroke> strokes,
        int maximumStrokeCount,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(strokes);cancellationToken.ThrowIfCancellationRequested();
        if(strokes.Count is < 1 or > 256||maximumStrokeCount is < 1 or > 256)
            throw new ArgumentException("Bounded positive stroke count/threshold required.");
        int points=0;
        foreach(var stroke in strokes)
        {ArgumentNullException.ThrowIfNull(stroke);points+=stroke.Points.Length;if(points>4096)throw new ArgumentException("Ink exceeds4096 points.",nameof(strokes));}
        var boundaries=new bool[Math.Max(0,strokes.Count-1)];
        if(strokes.Count<=maximumStrokeCount)Array.Fill(boundaries,true);
        else
        {
            var gaps=new (int Square,int Index)[boundaries.Length];
            for(int i=0;i<gaps.Length;i++)
            {
                var a=strokes[i].Points[^1];var b=strokes[i+1].Points[0];
                long x=b.X-a.X,y=b.Y-a.Y,square=x*x+y*y;
                if(square>int.MaxValue)throw new ArgumentException("Squared pen-up gap leaves checked signed-int domain.",nameof(strokes));
                gaps[i]=((int)square,i);
            }
            Sort(0,gaps.Length-1);
            for(int i=0;i<maximumStrokeCount-1;i++)boundaries[gaps[i].Index]=true;
            void Sort(int left,int right)
            {
                do
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    int i=left,j=right,pivot=gaps[(left+right)/2].Square;
                    do
                    {
                        while(pivot<gaps[i].Square)i++;
                        while(gaps[j].Square<pivot)j--;
                        if(i>j)break;
                        (gaps[i],gaps[j])=(gaps[j],gaps[i]);i++;j--;
                    }while(i<=j);
                    if(left<j)Sort(left,j);
                    if(right<=i)return;
                    left=i;
                }while(true);
            }
        }
        var result=new List<LegacyInkStroke>();var retained=new List<int>();var group=new List<LegacyInkPoint>();
        for(int i=0;i<strokes.Count;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach(var point in strokes[i].Points)group.Add(point);
            if(i==strokes.Count-1||boundaries[i])
            {
                result.Add(new(group.ToArray()));group.Clear();
                if(i<strokes.Count-1)retained.Add(i);
            }
        }
        return new(result.AsReadOnly(),retained.ToArray());
    }
}
