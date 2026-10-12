namespace InkRecognition;

/// <summary>Original raw endpoint intervals through265 factor200 and297 insertion.
/// Unlike the earlier ordinary-only scope helper, this owns a smoothed row source.</summary>
public sealed class LegacyFirstRowPrimitiveScope
{
    private readonly LegacyOrdinaryRowAcousticInput row;
    private readonly LegacySmoothedRowCutResult cuts;
    public LegacySourcePointChronology EndKeys {get;}
    public LegacyFirstRowPrimitiveScope(LegacyOrdinaryRowAcousticInput row,LegacySmoothedRowCutResult cuts,
        ReadOnlySpan<int> rawPointCounts,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(row);ArgumentNullException.ThrowIfNull(cuts);cancellationToken.ThrowIfCancellationRequested();
        if(!ReferenceEquals(cuts.Owner,row)||rawPointCounts.Length!=row.OriginalSource.Strokes.Count)
            throw new ArgumentException("Owned cuts and one original raw count per stroke required.");
        this.row=row;this.cuts=cuts;var keys=new List<LegacySourcePointKey>();int total=0;
        for(int stroke=0;stroke<rawPointCounts.Length;stroke++)
        {
            int raw=rawPointCounts[stroke];var indices=row.OriginalSource.OriginalPointIndices[stroke].Span;
            if(raw is <1 or >4096||total>4096-raw||indices.IsEmpty||indices[0]!=0||indices[^1]>=raw)
                throw new ArgumentException("Original raw count disagrees with retained point ownership.");
            total+=raw;
            for(int point=0;point<indices.Length;point++)
            {
                cancellationToken.ThrowIfCancellationRequested();var start=row.OriginalSource.Chronology.Resolve(stroke,point).Key;
                int end=(point+1<indices.Length?indices[point+1]:raw)-1;
                keys.Add(new(start.First,start.Second,end));
            }
        }
        EndKeys=LegacySourcePointChronology.CreatePrepared(row.Prepared.PhysicalPointCounts.Span,keys.ToArray(),
            Enumerable.Range(0,keys.Count).ToArray(),row.Prepared.PointKeys200.ToArray().Select(k=>checked((uint)k)).ToArray(),200)
            .WithInsertedPreparedInk(cuts.Cuts.PreparedInk,cancellationToken);
    }

    public LegacySourcePrimitiveScope Evaluate(ReadOnlySpan<LegacyInkRange> nativeOrderedRanges,int normalizedStrokeCount,
        CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var counts=cuts.Cuts.PreparedInk.StrokePointCounts.Span;
        if(nativeOrderedRanges.Length is <1 or >15||normalizedStrokeCount<1||normalizedStrokeCount>nativeOrderedRanges.Length)
            throw new ArgumentException("Bounded ordered ranges and normalized stroke count required.");
        foreach(var r in nativeOrderedRanges)
            if((uint)r.StrokeIndex>=(uint)counts.Length||r.StartPoint<0||r.EndPointExclusive<=r.StartPoint||r.EndPointExclusive>counts[r.StrokeIndex])
                throw new ArgumentException("Range outside this prepared source.");
        var first=nativeOrderedRanges[0];var last=nativeOrderedRanges[^1];
        bool boundary=normalizedStrokeCount==1&&first.StartPoint==0&&last.EndPointExclusive==counts[last.StrokeIndex];
        if(!boundary)return new(false,false,-1,null);
        bool zero=cuts.Chronology.Resolve(first.StrokeIndex,0).Key.Third==0&&EndKeys.Resolve(last.StrokeIndex,last.EndPointExclusive-1).Key.Third==0;
        return new(true,zero,first.StrokeIndex,row.OriginalSource.SourceStrokeProbabilities.Span[first.StrokeIndex]);
    }
}
