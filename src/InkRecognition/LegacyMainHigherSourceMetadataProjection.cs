namespace InkRecognition;

//Ordinary1AA470 higher-kind0 projection. Actual factories retain source ownership.
internal static class LegacyMainHigherSourceMetadataProjection
{
    //Three reads bound one parent preflight and future refresh count/fill.
    //Actual strides, not invented native/CLR layouts; no allocation here.
    internal static (long Copy,long Loops) BoundThreeReads(int ranges)
    {
        if(ranges is <1 or >65536)throw new NotSupportedException("Bounded actual higher-kind0 range owner required.");
        long n=ranges,row=System.Runtime.CompilerServices.Unsafe.SizeOf<Row>(),output=System.Runtime.CompilerServices.Unsafe.SizeOf<LegacyOrdinaryGraphSourceReference>();
        //Ranges clone; conservative List/OrderBy buffers/map/output; RemoveAt
        //row shifts; conditional framework sort-map store estimate. New26N
        //refresh allocation/temp copies are charged once, not for every read.
        long copied=checked(3*(12*n+8*row*n+16*n+8*output*n+row*n*(n-1)/2+16*n*(n+1))+128*n+1536);
        //Own range/compaction/output loops; additionally reserve every possible
        //RemoveAt shifted row. BCL sort/helper CPU is explicitly separate.
        return(copied,checked(3*(3*n+n*(n-1)/2)));
    }
    internal static LegacyOrdinaryGraphSourceReference[] Project(LegacySourcePointChronology prepared,
        LegacyOrdinarySourceInk original,LegacySourcePointKey[] keyRecords,
        LegacyInkRange[] actualRanges,CancellationToken token)
    {
        if(actualRanges.Length is <1 or >65536)throw new NotSupportedException("Bounded actual higher-kind0 range owner required.");
        var rows=new List<Row>(actualRanges.Length);var counts=prepared.StrokePointCounts.Span;
        foreach(var range in actualRanges)
        {
            token.ThrowIfCancellationRequested();
            if((uint)range.StrokeIndex>=(uint)counts.Length||range.StartPoint<0||range.EndPointExclusive<=range.StartPoint||range.EndPointExclusive>counts[range.StrokeIndex])
                throw new InvalidDataException("Owned higher range escapes its actual prepared source.");
            var first=prepared.Resolve(range.StrokeIndex,range.StartPoint);
            var lastStart=prepared.Resolve(range.StrokeIndex,range.EndPointExclusive-1);
            rows.Add(new(first,original.ResolveEndingMetadata(lastStart,token),lastStart,range.EndPointExclusive==counts[range.StrokeIndex]));
        }
        rows=rows.OrderBy(r=>r,RowComparer.Instance).ToList();
        for(int i=rows.Count-1;i>0;i--)
        {
            token.ThrowIfCancellationRequested();var current=rows[i];var previous=rows[i-1];
            bool equal=current.First.Key==previous.LastStart.Key&&current.First.RemainderByte==previous.LastStart.RemainderByte;
            int next=previous.LastStart.KeyRecordIndex+1;
            bool adjacent=previous.Tail&&next<keyRecords.Length&&current.First.Key==keyRecords[next];
            if(equal||adjacent){rows[i-1]=previous with{Last=current.Last,LastStart=current.LastStart,Tail=current.Tail};rows.RemoveAt(i);}
        }
        return rows.Select(r=>new LegacyOrdinaryGraphSourceReference(r.First.Key,r.Tail?r.Last.Key:r.LastStart.Key,
            r.First.RemainderByte,r.Tail?r.Last.RemainderByte:r.LastStart.RemainderByte)).ToArray();
    }
    private readonly record struct Row(LegacySourcePointIdentity First,LegacySourcePointIdentity Last,LegacySourcePointIdentity LastStart,bool Tail);
    private sealed class RowComparer:IComparer<Row>
    {
        public static readonly RowComparer Instance=new();
        public int Compare(Row a,Row b)
        {
            int c=Key(a.First.Key,b.First.Key);if(c!=0)return c;
            c=b.First.RemainderByte.CompareTo(a.First.RemainderByte);if(c!=0)return c;
            c=Key(a.Last.Key,b.Last.Key);return c!=0?c:b.Last.RemainderByte.CompareTo(a.Last.RemainderByte);
        }
        private static int Key(LegacySourcePointKey a,LegacySourcePointKey b)
        {int c=a.First.CompareTo(b.First);if(c!=0)return c;c=a.Second.CompareTo(b.Second);return c!=0?c:a.Third.CompareTo(b.Third);}
    }
}
