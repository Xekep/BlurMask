using System.Numerics;

namespace InkRecognition;

internal sealed partial class LegacyMathAkOriginalInputContext
{
    private const long Source13A2SegmentPayloadLimit=64L*1024*1024;
    private const long Source13A2SegmentWorkLimit=64L*1024*1024;

    //Successful fresh044000 tail only. No pool, FIRST request, classifier cache,
    //graph-ready flag or external prepared-record constructor is issued here.
    private Source13A2SegmentFrame Build13A2Segments(Source13A2Output sampled,CancellationToken token)
        =>Source13A2SegmentFrame.Create(this,sampled,token);

    private readonly record struct Source13A2NodeRange(int OriginalId,int Start,int InclusiveEnd);
    private struct Source13A2SegmentRecord
    {
        internal Source13A2NodeRange Range;
        internal Source13A2StrokeMetric Metric;
        internal byte EndsOriginal;
    }
    private sealed class Source13A2SegmentBudget
    {
        private readonly LegacyMathAkOriginalInputContext owner;
        internal long Bytes {get;private set;}=24;
        internal long Work {get;private set;}
        internal Source13A2SegmentBudget(LegacyMathAkOriginalInputContext owner){owner.Live();this.owner=owner;}
        internal void Charge(long bytes,long work,CancellationToken token)
        {
            owner.Live();token.ThrowIfCancellationRequested();
            if(bytes<0||work<0||bytes>Source13A2SegmentPayloadLimit-Bytes||work>Source13A2SegmentWorkLimit-Work)
                throw new NotSupportedException("Source13A2 segment reservation exhausted.");
            Bytes+=bytes;Work+=work;
        }
        internal bool Test(bool condition,CancellationToken token){Charge(0,1,token);return condition;}
    }
    private sealed class Source13A2SegmentFrame
    {
        private readonly LegacyMathAkOriginalInputContext owner;
        private readonly Source13A2Output sampled;
        private readonly Source13A0Frame original;
        private readonly LegacyMathAkOriginalInputFrame inputFrame;
        private readonly LegacyMathAkFeatureProfile profile;
        private readonly object documentIdentity;
        private readonly Source13A2SegmentRecord[] segments;
        private readonly int[] originalStarts;
        private readonly int totalMembership;
        internal long ChargedBytes {get;}
        internal long ChargedWork {get;}
        private Source13A2SegmentFrame(LegacyMathAkOriginalInputContext owner,Source13A2Output sampled,
            Source13A0Frame original,Source13A2SegmentRecord[] segments,int[] originalStarts,int total,
            long bytes,long work)
        {
            this.owner=owner;this.sampled=sampled;this.original=original;inputFrame=owner.input;
            profile=inputFrame.OriginalOwner.FeatureProfile;documentIdentity=owner.documentIdentity;
            this.segments=segments;this.originalStarts=originalStarts;totalMembership=total;
            ChargedBytes=bytes;ChargedWork=work;
        }
        internal int Count=>segments.Length;
        internal int OriginalCount=>originalStarts.Length-1;
        internal int TotalMembership=>totalMembership;
        internal int RequiredWorkWords=>(segments.Length>>5)+1;
        //NEW private nested source join only: never accepts an exported DATA
        //record, and does not expose the actual sampled backing or owner.
        internal bool MatchesSampled(Source13A2Output actual,CancellationToken token)
        { Validate(token);return ReferenceEquals(sampled,actual); }
        //RequiredWorkWords is metadata. At zero native slots no query mask is
        //allocated. Existing13B6 masks do not become a complete108B pool here.
        internal void Validate(CancellationToken token)
        {
            owner.Live();token.ThrowIfCancellationRequested();
            if(!ReferenceEquals(sampled.Owner,owner)||!ReferenceEquals(owner.source13A0,original)||
                !ReferenceEquals(owner.input,inputFrame)||!ReferenceEquals(owner.documentIdentity,documentIdentity)||
                !ReferenceEquals(inputFrame.OriginalOwner.FeatureProfile,profile))
                throw new InvalidOperationException("Same live segment source lineage required.");
        }
        internal Source13A2SegmentRecord Read(int index,CancellationToken token)
        {
            Validate(token);if((uint)index>=(uint)segments.Length)throw new ArgumentOutOfRangeException(nameof(index));
            return segments[index];
        }
        internal int ReadOriginalStart(int index,CancellationToken token)
        {
            Validate(token);if((uint)index>=(uint)originalStarts.Length)throw new ArgumentOutOfRangeException(nameof(index));
            return originalStarts[index];
        }
        internal static Source13A2SegmentFrame Create(LegacyMathAkOriginalInputContext owner,
            Source13A2Output sampled,CancellationToken token)
        {
            owner.Live();token.ThrowIfCancellationRequested();
            var original=owner.source13A0;
            if(!ReferenceEquals(sampled.Owner,owner)||original is null)
                throw new InvalidOperationException("Same freshly sampled source required.");
            int s=owner.input.StrokeCount,m=sampled.Markers.Length,q=sampled.Points.Length;
            if(s is<1 or>64||sampled.Ends.Length!=s||sampled.OriginalIndices.Length!=q||sampled.Metrics.Length!=s)
                throw new InvalidOperationException("Segment source header mismatch.");
            var budget=new Source13A2SegmentBudget(owner);
            //48B is a conservative value-storage allowance: three I32 range
            //fields, six F32 metrics and one byte, with padding; no row objects.
            budget.Charge(checked(128+48L*m+4L*(s+1)),0,token);
            int previousEnd=0;
            for(int stroke=0;budget.Test(stroke<s,token);stroke++)
            {
                if(owner.input.ReadStrokeId(stroke)!=stroke||owner.input.ReadStrokeOrdinal(stroke)!=stroke||
                    sampled.Ends[stroke]<=previousEnd||sampled.Ends[stroke]>q)
                    throw new InvalidOperationException("Fresh original ID or sampled end changed.");
                previousEnd=sampled.Ends[stroke];
            }
            if(previousEnd!=q)throw new InvalidOperationException("Segment point total mismatch.");
            long pointVisits=0;int lastStroke=-1,lastOffset=-1;
            for(int i=0;budget.Test(i<m,token);i++)
            {
                var marker=sampled.Markers[i];
                if((uint)marker.Stroke>=(uint)s||marker.Stroke<lastStroke||
                    marker.Stroke==lastStroke&&marker.Offset<=lastOffset)
                    throw new InvalidOperationException("Private marker chronology changed.");
                var range=sampled.Range(i);int count=sampled.Ends[range.Stroke]-(range.Stroke==0?0:sampled.Ends[range.Stroke-1]);
                if(range.Stroke!=marker.Stroke||range.Start!=marker.Offset||range.Start<0||range.Start>=range.End||range.End>count)
                    throw new InvalidOperationException("Private marker range changed.");
                pointVisits=checked(pointVisits+range.End-range.Start);lastStroke=marker.Stroke;lastOffset=marker.Offset;
            }
            //Unique ordered markers overlap only the shared endpoint. No first
            //marker==0 assumption or extra shape narrowing is needed for this.
            if(pointVisits>checked((long)q+m))throw new InvalidOperationException("Range visit bound failed.");
            long expectedWork=checked(14L*m+5L*s+2*pointVisits+21);
            if(expectedWork>Source13A2SegmentWorkLimit)
                throw new NotSupportedException("Source13A2 segment reservation exhausted.");
            budget.Charge(0,checked((long)m+s+1),token);
            var records=new Source13A2SegmentRecord[m];var starts=new int[s+1];int at=0,total=0;
            for(int originalId=0;budget.Test(originalId<s,token);originalId++)
            {
                budget.Charge(0,1,token);starts[originalId]=at;int first=at;
                while(budget.Test(at<m&&sampled.Markers[at].Stroke==originalId,token))
                {
                    var range=sampled.Range(at);int global=originalId==0?0:sampled.Ends[originalId-1];
                    var values=Measure13A2Segment(sampled.Points.AsSpan(checked(global+range.Start),range.End-range.Start),budget,token);
                    budget.Charge(0,10,token);
                    records[at]=new Source13A2SegmentRecord{Range=new(originalId,range.Start,range.End-1),Metric=values,EndsOriginal=0};
                    //Literal044000 writes the PREVIOUS row, not current start.
                    if(at>0){budget.Charge(0,1,token);records[at-1].EndsOriginal=range.Start==0?(byte)1:(byte)0;}
                    at++;
                }
                total=checked(total+at-first);
            }
            budget.Charge(0,1,token);starts[s]=at;
            if(at!=m||total!=m)throw new InvalidOperationException("Original membership total changed.");
            if(m>0){budget.Charge(0,1,token);records[m-1].EndsOriginal=1;}
            budget.Charge(0,16,token); //Conservative fixed frame/control stores.
            if(budget.Work!=expectedWork)throw new InvalidOperationException("Segment work-site chronology mismatch.");
            owner.Live();token.ThrowIfCancellationRequested();
            return new(owner,sampled,original,records,starts,total,budget.Bytes,budget.Work);
        }
    }
    private static Source13A2StrokeMetric Measure13A2Segment(ReadOnlySpan<Vector2> row,
        Source13A2SegmentBudget budget,CancellationToken token)
    {
        if(row.Length<1)throw new InvalidOperationException("Empty sampled segment range.");
        float minX=row[0].X,minY=row[0].Y,maxX=minX,maxY=minY;
        Finite(minX);Finite(minY);
        for(int i=1;budget.Test(i<row.Length,token);i++)
        {
            var p=row[i];Finite(p.X);Finite(p.Y);
            if(maxX<=p.X)maxX=p.X;if(maxY<=p.Y)maxY=p.Y;
            if(!(minX<=p.X))minX=p.X;if(!(minY<=p.Y))minY=p.Y;
        }
        double arc=0,centerX=0,centerY=0;
        for(int i=1;budget.Test(i<row.Length,token);i++)
        {
            var p=row[i-1];var next=row[i];float dx=next.X-p.X,dy=next.Y-p.Y;
            float square=dx*dx+dy*dy;Finite(square);double length=Math.Sqrt(square);
            arc+=length;centerX+=(double)(next.X+p.X)*length;centerY+=(double)(next.Y+p.Y)*length;
        }
        float x=arc>0?(float)((centerX*.5)/arc):row[0].X,y=arc>0?(float)((centerY*.5)/arc):row[0].Y;
        Finite(x);Finite(y);return new(minX,minY,maxX,maxY,x,y);
    }
}
