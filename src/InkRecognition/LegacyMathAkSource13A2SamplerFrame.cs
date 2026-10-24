using System.Numerics;
namespace InkRecognition;

internal sealed partial class LegacyMathAkOriginalInputContext
{
    //New SOURCE policy: cumulative logical allocated payload / source-loop and
    //copy cardinalities. Not RSS, native allocation layout or CPU instructions.
    private const long Source13A2LogicalPayloadLimit=128L*1024*1024;
    private const long Source13A2WorkLimit=1_000_000_000;
    private sealed class Source13A2Budget
    {
        private readonly LegacyMathAkOriginalInputContext owner;
        private long bytes,work;
        internal Source13A2Budget(LegacyMathAkOriginalInputContext owner){owner.Live();this.owner=owner;bytes=24;}
        internal void Charge(long b,long w,CancellationToken token)
        {
            owner.Live();token.ThrowIfCancellationRequested();
            if(b<0||w<0||b>Source13A2LogicalPayloadLimit-bytes||w>Source13A2WorkLimit-work)
                throw new NotSupportedException("Math13A2 cumulative operation reservation exhausted.");
            bytes+=b;work+=w;
        }
        internal long Bytes=>bytes;
        internal long Work=>work;
    }
    private sealed class SamplerFrame
    {
        private readonly LegacyMathAkOriginalInputContext owner;
        private readonly Source13A0Frame original;
        private readonly LegacyMathAkFeatureProfile profile;
        private readonly LegacyCursivePreparedPoint[] points;
        private readonly int[] counts,originalStarts;
        private readonly Source13A2Budget budget;
        private readonly CancellationToken token;
        internal LegacyMathAkSource13A2Policy Policy {get;}
        //No SamplerFrame reference/Memory is exported from this private type.
        internal ReadOnlyMemory<LegacyCursivePreparedPoint> Points=>points;
        internal ReadOnlyMemory<int> StrokePointCounts=>counts;
        private SamplerFrame(LegacyMathAkOriginalInputContext owner,Source13A0Frame original,
            LegacyCursivePreparedPoint[] points,int[] counts,int[] originalStarts,
            Source13A2Budget budget,CancellationToken token)
        {this.owner=owner;this.original=original;this.points=points;this.counts=counts;
         this.originalStarts=originalStarts;this.budget=budget;this.token=token;
         profile=owner.input.OriginalOwner.FeatureProfile;Policy=profile.Source13A2Policy;}
        internal static SamplerFrame Create(LegacyMathAkOriginalInputContext owner,Source13A2Budget budget,CancellationToken token)
        {
            owner.Live();token.ThrowIfCancellationRequested();
            var original=owner.source13A0??throw new InvalidOperationException("Same live13A0 source required.");
            int s=owner.input.StrokeCount,q=original.PointCount;
            if(s is<1 or>64||original.Points.Length!=s||original.OriginalIndices.Length!=s||q<1||q>Source13A0LogicalPayloadLimit/12)
                throw new InvalidOperationException("Owned Math13A2 row cardinality mismatch.");
            long total=0;
            for(int i=0;i<s;i++)
            {budget.Charge(0,1,token);if(owner.input.ReadStrokeId(i)!=i||original.Points[i].Length<1||
              original.Points[i].Length!=original.OriginalIndices[i].Length)throw new InvalidOperationException("Owned Math13A2 row map mismatch.");total+=original.Points[i].Length;}
            if(total!=q)throw new InvalidOperationException("Owned Math13A2 total mismatch.");
            budget.Charge(checked(128+32L*q+4L*s+4L*(s+1)),checked(3L*q+2L*s),token);
            var p=new LegacyCursivePreparedPoint[q];var counts=new int[s];var starts=new int[s+1];int at=0;
            for(int stroke=0;stroke<s;stroke++)
            {
                starts[stroke]=at;counts[stroke]=original.Points[stroke].Length;
                for(int j=0;j<counts[stroke];j++)
                {token.ThrowIfCancellationRequested();var xy=original.Points[stroke][j];Finite(xy.X);Finite(xy.Y);
                 p[at]=new(xy.X,xy.Y,0,0,0,j==0?1:0,stroke,at);at++;}
            }
            starts[s]=at;
            for(int i=0;i<q-1;i++){token.ThrowIfCancellationRequested();p[i]=Forward(p[i],p[i+1]);}
            return new(owner,original,p,counts,starts,budget,token);
        }
        internal bool Test(bool condition,CancellationToken cancellationToken)
        {Validate();budget.Charge(0,1,cancellationToken);return condition;}
        internal void Reserve(long bytes,long work=0){Validate();budget.Charge(bytes,work,token);}
        private void Validate()
        {
            owner.Live();
            if(!ReferenceEquals(owner.source13A0,original)||!ReferenceEquals(owner.input.OriginalOwner.FeatureProfile,profile))
                throw new InvalidOperationException("Owned sampler source/profile changed.");
        }
        internal LegacyCursivePreparedPoint[] CopyRecords()
        {Validate();budget.Charge(32L*points.Length,points.Length,token);return (LegacyCursivePreparedPoint[])points.Clone();}
        internal int[] CopyTags(LegacyCursivePreparedPoint[] values)
        {
            Validate();budget.Charge(4L*values.Length,values.Length,token);var tags=new int[values.Length];
            for(int i=0;i<tags.Length;i++){token.ThrowIfCancellationRequested();tags[i]=values[i].Tag;}return tags;
        }
        internal SamplerFrame WithTags(ReadOnlySpan<int> tags)
        {
            Validate();if(tags.Length!=points.Length)throw new InvalidOperationException("Owned tag cardinality mismatch.");
            budget.Charge(checked(128+32L*points.Length+4L*counts.Length),checked(2L*points.Length+counts.Length),token);
            var p=(LegacyCursivePreparedPoint[])points.Clone();var c=(int[])counts.Clone();
            for(int i=0;i<p.Length;i++){token.ThrowIfCancellationRequested();p[i]=p[i] with{Tag=tags[i]};}
            return new(owner,original,p,c,originalStarts,budget,token);
        }
        internal SamplerFrame Insert(int stroke,int before,InkPoint xy,int tag,CancellationToken cancellationToken)
        {
            Validate();cancellationToken.ThrowIfCancellationRequested();
            if((uint)stroke>=(uint)counts.Length||before<1||before>=counts[stroke])throw new InvalidOperationException("Owned insertion needs same-stroke neighbors.");
            Finite(xy.X);Finite(xy.Y);int at=before;for(int i=0;i<stroke;i++){budget.Charge(0,1,cancellationToken);at=checked(at+counts[i]);}
            int n=checked(points.Length+1);
            budget.Charge(checked(128+32L*n+4L*counts.Length),checked(2L*n+counts.Length+32),cancellationToken);
            var p=new LegacyCursivePreparedPoint[n];points.AsSpan(0,at).CopyTo(p);points.AsSpan(at).CopyTo(p.AsSpan(at+1));
            p[at]=new(xy.X,xy.Y,0,0,0,tag,stroke,points[at-1].OriginalPointIndex);
            p[at-1]=Forward(p[at-1],p[at]);p[at]=Forward(p[at],p[at+1]);
            var c=(int[])counts.Clone();c[stroke]=checked(c[stroke]+1);
            return new(owner,original,p,c,originalStarts,budget,token);
        }
        private static LegacyCursivePreparedPoint Forward(LegacyCursivePreparedPoint a,LegacyCursivePreparedPoint b)
        {
            float dx=b.X-a.X,dy=b.Y-a.Y;Finite(dx);Finite(dy);
            float distance=(float)Math.Sqrt((double)dy*dy+(double)dx*dx);Finite(distance);
            return a with{DeltaX=dx,DeltaY=dy,SegmentLength=distance};
        }
        internal ReadOnlySpan<Vector2> OriginalRow(int stroke)
        {Validate();if((uint)stroke>=(uint)original.Points.Length)throw new ArgumentOutOfRangeException(nameof(stroke));return original.Points[stroke];}
        internal Source13A2Output Complete(Source13A2StrokeMetric[] metrics)
        {
            Validate();int markers=0;
            for(int i=0;i<points.Length;i++)if(Test(Visible13A2(points[i].Tag),token))markers=checked(markers+1);
            int q=points.Length,s=counts.Length;
            budget.Charge(checked(128+12L*q+4L*s+12L*markers),checked(5L*q+3L*s+markers),token);
            var xy=new Vector2[q];var map=new int[q];var ends=new int[s];var cuts=new Source13A2Marker[markers];int at=0,k=0;
            for(int stroke=0;stroke<s;stroke++)
            {
                for(int local=0;local<counts[stroke];local++,at++)
                {
                    token.ThrowIfCancellationRequested();var p=points[at];int old=p.OriginalPointIndex-originalStarts[stroke];
                    if(p.StrokeIndex!=stroke||(uint)old>=(uint)original.OriginalIndices[stroke].Length)
                        throw new InvalidOperationException("Owned sampled point map changed source.");
                    xy[at]=new(p.X,p.Y);map[at]=original.OriginalIndices[stroke][old];
                    if(Visible13A2(p.Tag))cuts[k++]=new(p.Tag,stroke,local);
                }
                ends[stroke]=at;
            }
            if(at!=q||k!=markers)throw new InvalidOperationException("Owned sampled output count mismatch.");
            Validate();token.ThrowIfCancellationRequested();return new(owner,xy,map,ends,cuts,metrics,budget.Bytes,budget.Work);
        }
    }
    private static bool Visible13A2(int tag)=>tag!=0&&unchecked((uint)(tag-11))>3;
    private readonly record struct Source13A2Marker(int Tag,int Stroke,int Offset);
    private sealed class Source13A2Output
    {
        internal readonly LegacyMathAkOriginalInputContext Owner;
        internal readonly Vector2[] Points;
        internal readonly int[] OriginalIndices,Ends;
        internal readonly Source13A2Marker[] Markers;
        internal readonly Source13A2StrokeMetric[] Metrics;
        internal readonly long ChargedBytes,ChargedWork;
        internal Source13A2Output(LegacyMathAkOriginalInputContext owner,Vector2[] p,int[] map,int[] ends,
            Source13A2Marker[] markers,Source13A2StrokeMetric[] metrics,long bytes,long work)
        {Owner=owner;Points=p;OriginalIndices=map;Ends=ends;Markers=markers;Metrics=metrics;ChargedBytes=bytes;ChargedWork=work;}
        internal (int Stroke,int Start,int End) Range(int index)
        {
            Owner.Live();if((uint)index>=(uint)Markers.Length)throw new ArgumentOutOfRangeException(nameof(index));
            var m=Markers[index];int end=index+1<Markers.Length&&Markers[index+1].Stroke==m.Stroke?
                checked(Markers[index+1].Offset+1):Ends[m.Stroke]-(m.Stroke==0?0:Ends[m.Stroke-1]);
            return(m.Stroke,m.Offset,end);
        }
    }
}
