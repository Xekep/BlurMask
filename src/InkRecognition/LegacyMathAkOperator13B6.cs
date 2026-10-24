using System.Numerics;

namespace InkRecognition;

internal sealed partial class LegacyMathAkOriginalInputContext
{
    //Actual13B6/0101 serialized8fields at7253..7285. All instances descend
    //from the actual authenticated profile; these are not caller parameters.
    private const float OverlapThreshold=0.5f, InsideThreshold=0.5f;
    private const float AngularClassLimit=0.6f, MaximumRunMultiplier=3f;
    private const int MinimumDirectionalRuns=5;
    private const float HalfPi=1.5707963705062866f, Pi=3.1415927410125732f;
    private const float TwoPi=6.2831854820251465f;

    private void RunFresh13B6(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();Claim();
        //Fresh source has no rectangles, old records or incremental suffix.
        //Native bbox construction and fixed capacities are already preflighted.
        for(int current=0;current<input.StrokeCount;current++)
        {
            Live();token.ThrowIfCancellationRequested();
            if(!Eligible(current,token))continue;
            var row=Row(current,token);
            var geometry=LegacyMathAkOperator13B6Geometry.AnalyzeForOperator(prefix!,row,current,token);
            geometry.Validate(prefix!,current);
            if(geometry.WasSkipped||!HasAnalysisFlag(geometry.FittedPoints,prefix!.EstimatedScale,token))continue;
            var affected=FindAffected(current,geometry.OriginalPoints,token);
            bool any=false;foreach(uint word in affected)any|=word!=0;
            if(!any)continue;
            int descriptor=AppendDescriptor(current,affected,token);
            WriteRangeOwner(current,descriptor,token);
        }
        Live();token.ThrowIfCancellationRequested();
    }

    private bool Eligible(int current,CancellationToken token)
    {
        for(int prior=0;prior<current;prior++)
        {
            token.ThrowIfCancellationRequested();int id=input.ReadStrokeId(prior);
            if(input.IsActive(id)&&!Marked(id)&&Coverage(bounds[current],bounds[prior])>OverlapThreshold)return true;
        }
        return false;
    }
    private static void Finite(float value)
    {if(!float.IsFinite(value))throw new NotSupportedException("Operator13B6 finite-intermediate subset required.");}
    private static float Coverage(Vector4 current,Vector4 prior)
    {
        //Native staged sum-of-widths minus union width; asymmetric PRIOR area.
        float maxX=prior.Z<=current.Z?current.Z:prior.Z;
        float minX=prior.X<=current.X?prior.X:current.X;
        float x=((current.Z-current.X)+(prior.Z-prior.X))-(maxX-minX);Finite(x);
        float area=0f;
        if(x>0f)
        {
            float maxY=prior.W<=current.W?current.W:prior.W;
            float minY=prior.Y<=current.Y?prior.Y:current.Y;
            float y=((prior.W-prior.Y)+(current.W-current.Y))-(maxY-minY);Finite(y);
            if(y>0f){area=y*x;Finite(area);}
        }
        if(area>0f){area/=((prior.W-prior.Y)*(prior.Z-prior.X));Finite(area);}
        return area;
    }
    private static float WrapHalf(float value)
    {
        float output=(float)((double)(value+HalfPi)%(double)Pi)-HalfPi;
        if(output< -HalfPi)output+=Pi;Finite(output);return output;
    }
    private static float WrapWhole(float value)
    {
        float output=(float)((double)(value+Pi)%(double)TwoPi)-Pi;
        if(output< -Pi)output+=TwoPi;Finite(output);return output;
    }
    private readonly record struct DirectionRun(int Class,float Length);
    private struct Bin {internal int Count;internal float Sum,Maximum;}
    private static bool HasAnalysisFlag(ReadOnlySpan<Vector2> points,float sourceScale,CancellationToken token)
    {
        //0205B0 initial out-cost=FloatMax/out-flag0; caller only consumes flag.
        if(points.Length<4)return false;
        int count=points.Length-1;
        var lengths=new float[count];var angles=new float[count];
        float mean=0f,total=0f,variance=0f;
        for(int i=0;i<count;i++)
        {
            token.ThrowIfCancellationRequested();
            float dx=points[i+1].X-points[i].X,dy=points[i+1].Y-points[i].Y;
            float squared=dx*dx+dy*dy;Finite(squared);
            float length=MathF.Sqrt(squared);Finite(length);lengths[i]=length;
            //Actual020973..02098C passes NEXT-CURRENT, promoted to double.
            float angle=(float)Math.Atan2((double)dy,(double)dx);Finite(angle);angles[i]=angle;
            float folded=WrapHalf(angle);
            if(HalfPi<=MathF.Abs(WrapWhole(mean-folded)))mean+=Pi;
            mean=WrapWhole(mean);folded=WrapWhole(folded);
            float updated=length+total;Finite(updated);
            if(!(updated>0f))throw new NotSupportedException("Positive fitted path length required.");
            mean=WrapHalf((mean*total+folded*length)/updated);total=updated;
        }
        //0702B0/070840 are FLOAT cos/sin of NEGATIVE mean, not the double
        //CRT calls in01FA60. Portable CLR policy, not vendor-bit equivalence.
        float cos=MathF.Cos(-mean),sin=MathF.Sin(-mean);Finite(cos);Finite(sin);
        float offset=((-sin)*0f-cos*0f)+0f;
        float minimum=float.MaxValue,maximum=-float.MaxValue;
        foreach(var point in points)
        {
            token.ThrowIfCancellationRequested();float value=sin*point.X+cos*point.Y+offset;Finite(value);
            if(value<=minimum)minimum=value;if(maximum<=value)maximum=value;
        }
        float spread=maximum-minimum;Finite(spread);
        if(spread<sourceScale*0.5f*(2f-FreshGuide*2f))return false;
        var runs=new DirectionRun[count];var medianLengths=new float[count];
        int runCount=0,medianCount=0,priorClass=0;float runLength=0f;
        for(int i=0;i<count;i++)
        {
            token.ThrowIfCancellationRequested();float difference=angles[i]-mean;
            float folded=WrapHalf(difference);variance+=lengths[i]*folded*folded;Finite(variance);
            float absolute=MathF.Abs(WrapWhole(difference));
            int cls=AngularClassLimit<=absolute?(absolute<=Pi-AngularClassLimit?0:-1):1;
            if(i==0){priorClass=cls;runLength=lengths[i];}
            else if(cls==priorClass){runLength+=lengths[i];Finite(runLength);}
            else
            {
                runs[runCount++]=new(priorClass,runLength);
                if(priorClass!=0)medianLengths[medianCount++]=runLength;
                priorClass=cls;runLength=lengths[i];
            }
        }
        runs[runCount++]=new(priorClass,runLength);
        //020FA9..021035 appends the final run length unconditionally, even
        //for terminal class zero; only transition appends filter class zero.
        medianLengths[medianCount++]=runLength;
        //Same finite FloatArray sort values; unused capacity never participates.
        Array.Sort(medianLengths,0,medianCount);
        float median=medianLengths[(int)((float)medianCount*0.5f)];
        Span<Bin> bins=stackalloc Bin[3];bins.Clear();
        for(int i=0;i<runCount;i++)
        {
            token.ThrowIfCancellationRequested();ref Bin bin=ref bins[runs[i].Class+1];
            bin.Count++;bin.Sum+=runs[i].Length;Finite(bin.Sum);
            if(bin.Maximum<runs[i].Length)bin.Maximum=runs[i].Length;
        }
        //Signed centered native bins[-1,0,+1], including the NEGATIVE sum
        //that the decompiler incorrectly constant-propagated as0.0.
        Bin negative=bins[0],neutral=bins[1],positive=bins[2];
        int smaller=Math.Min(negative.Count,positive.Count);
        if(smaller<=1||negative.Count-smaller>=6||positive.Count-smaller>=6||
            MinimumDirectionalRuns>negative.Count+positive.Count)return false;
        float negativeMean=negative.Sum/negative.Count,positiveMean=positive.Sum/positive.Count;
        float maximumAllowed=median*MaximumRunMultiplier;Finite(maximumAllowed);
        if(maximumAllowed<negative.Maximum||maximumAllowed<positive.Maximum)return false;
        float smallerMean=MathF.Min(negativeMean,positiveMean);
        if(!(smallerMean>0f))throw new NotSupportedException("Positive directional means required.");
        if(4f<negativeMean/smallerMean||4f<positiveMean/smallerMean)return false;
        if(!(neutral.Sum<=median*1.5f))return false;
        float ratio=variance/total;Finite(ratio);
        float score=(MathF.Sqrt(ratio)/HalfPi)*BitConverter.Int32BitsToSingle(0x3f471c72);Finite(score);
        return score<FreshGuide*2f*0.5f;
    }

    private readonly record struct Hull(Vector2[] Points,int Count);
    private static bool Less(Vector2 a,Vector2 b)=>a.X<b.X||(a.X==b.X&&a.Y<b.Y);
    private static Hull ConvexHull(ReadOnlySpan<Vector2> source,CancellationToken token)
    {
        if(source.Length<1||source.Length>LegacyMathAkProcessedStrokePrefix.MaximumExpandedPoints)
            throw new InvalidOperationException("Owned hull source count invalid.");
        var sorted=source.ToArray();var pending=new int[checked(source.Length*2)];
        int used=0;pending[used++]=0;pending[used++]=sorted.Length-1;
        while(used!=0)
        {
            token.ThrowIfCancellationRequested();int right=pending[--used],left=pending[--used];if(right<=left)continue;
            int i=left,j=right;Vector2 pivot=sorted[left+(right-left)/2];
            do
            {
                while(Less(sorted[i],pivot))i++;while(Less(pivot,sorted[j]))j--;
                if(j<i)break;(sorted[i],sorted[j])=(sorted[j],sorted[i]);i++;j--;
            }while(i<=j);
            //Native recurses LEFT then tails RIGHT; explicit stack preserves
            //that swap schedule without unproved CLR recursion capacity.
            if(i<right){if(used>pending.Length-2)throw new InvalidOperationException("Hull work stack bound failed.");pending[used++]=i;pending[used++]=right;}
            if(left<j){if(used>pending.Length-2)throw new InvalidOperationException("Hull work stack bound failed.");pending[used++]=left;pending[used++]=j;}
        }
        var hull=new Vector2[checked(sorted.Length*2)];int count=0;
        for(int i=0;i<sorted.Length;i++)
        {
            token.ThrowIfCancellationRequested();
            while(count>1&&!(Turn(hull[count-2],hull[count-1],sorted[i])>0f))count--;
            hull[count++]=sorted[i];
        }
        int threshold=count+1;
        for(int i=sorted.Length-2;i>=0;i--)
        {
            token.ThrowIfCancellationRequested();
            while(count>=threshold&&!(Turn(hull[count-2],hull[count-1],sorted[i])>0f))count--;
            hull[count++]=sorted[i];
        }
        count=count<2?1:count-1;return new(hull,count);
    }
    private static float Turn(Vector2 a,Vector2 b,Vector2 p)
    {float cross=(b.X-a.X)*(p.Y-a.Y)-(b.Y-a.Y)*(p.X-a.X);Finite(cross);return cross;}
    private static int Sign(float x)=>x<0f?-1:x!=0f?1:0;
    private static bool Inside(Vector2 point,Hull hull)
    {
        int prior=Sign(Turn(hull.Points[0],hull.Points[1],point));
        for(int i=1;i<hull.Count;i++)
        {
            int current=Sign(Turn(hull.Points[i],hull.Points[(i+1)%hull.Count],point));
            if(current*prior==-1)return false;if(current!=0)prior=current;
        }
        return true;
    }
    private static float InsideLengthRatio(ReadOnlySpan<Vector2> prior,Hull hull,CancellationToken token)
    {
        if(prior.IsEmpty||hull.Count<3)return 0f;
        bool wasInside=Inside(prior[0],hull);float inside=0f,total=0f;
        for(int i=1;i<prior.Length;i++)
        {
            token.ThrowIfCancellationRequested();bool nowInside=Inside(prior[i],hull);
            float dy=prior[i-1].Y-prior[i].Y,dx=prior[i-1].X-prior[i].X;
            float length=MathF.Sqrt(dy*dy+dx*dx);Finite(length);total+=length;Finite(total);
            if(wasInside&&nowInside)inside+=length;else if(wasInside!=nowInside)inside+=length*0.5f;
            Finite(inside);wasInside=nowInside;
        }
        if(total==0f)return wasInside?1f:0f;float ratio=inside/total;Finite(ratio);return ratio;
    }
    private uint[] FindAffected(int current,ReadOnlySpan<Vector2> source,CancellationToken token)
    {
        Live();Hull hull=ConvexHull(source,token);var mask=new uint[wordCount];
        for(int prior=0;prior<current;prior++)
        {
            token.ThrowIfCancellationRequested();int id=input.ReadStrokeId(prior);
            if(!input.IsActive(id)||Marked(id)||!(Coverage(bounds[current],bounds[prior])>OverlapThreshold))continue;
            if(InsideLengthRatio(Row(prior,token).Points,hull,token)>InsideThreshold)mask[id>>5]|=1u<<(id&31);
        }
        return mask;
    }
    private int AppendDescriptor(int current,ReadOnlySpan<uint> affected,CancellationToken token)
    {
        Live();if(affected.Length!=wordCount||descriptorCount>=descriptorOrdinals.Length)
            throw new InvalidOperationException("Descriptor preflight invariant failed.");
        int slot=descriptorCount++;int offset=checked(slot*wordCount);int currentId=input.ReadStrokeId(current);
        descriptorOrdinals[slot]=current;
        for(int id=0;id<currentId;id++)
        {
            token.ThrowIfCancellationRequested();
            if(input.IsActive(id)&&!Marked(id)&&Bit(affected,id))descriptorMasks[offset+(id>>5)]|=1u<<(id&31);
        }
        return slot;
    }
    private RangeValue RangeFor(int ordinal)
    {
        int id=input.ReadStrokeId(ordinal);
        int first=prefix!.ReadOperatorFirstOriginalIndex(ordinal),last=prefix.ReadOperatorLastOriginalIndex(ordinal);
        if(id<0||id>=input.ItemCount||first<0||last<first||last>=input.PointCount)
            throw new InvalidOperationException("Original range provenance mismatch.");
        uint delta=checked((uint)(last-first));
        if(delta>=LegacyMathAkSourceGeometry.MaximumPoints)throw new InvalidOperationException("Original range extent out of bounds.");
        return new((uint)id,0,unchecked((uint)BitConverter.SingleToInt32Bits((float)delta)));
    }
    private void WriteRangeOwner(int current,int descriptor,CancellationToken token)
    {
        Live();if((uint)descriptor>=(uint)descriptorCount||descriptorOrdinals[descriptor]!=current)
            throw new InvalidOperationException("Foreign descriptor ordinal.");
        ReadOnlySpan<uint> affected=descriptorMasks.AsSpan(descriptor*wordCount,wordCount);
        int currentId=input.ReadStrokeId(current);
        //Literal native write BEFORE active guard; inactive early success has
        //only this mark, whereas all LATE paths execute common prior OR.
        processed[currentId>>5]|=1u<<(currentId&31);
        if(!input.IsActive(currentId))return;
        RangeOwner? owner=null;bool retained=false;
        try
        {
            token.ThrowIfCancellationRequested();var currentValues=new[]{RangeFor(current)};
            int count=0;for(int s=0;s<input.StrokeCount;s++)if(Bit(affected,input.ReadStrokeId(s)))count++;
            var priorValues=new RangeValue[count];int written=0;
            for(int s=0;s<input.StrokeCount;s++)
            {token.ThrowIfCancellationRequested();if(Bit(affected,input.ReadStrokeId(s)))priorValues[written++]=RangeFor(s);}
            if(written!=count)throw new InvalidOperationException("Range count changed.");
            owner=new RangeOwner(this,currentValues,priorValues);
            if(resultCount>=results.Length)throw new InvalidOperationException("Result preflight invariant failed.");
            int slot=resultCount++;results[slot]=null;owner.Retain(this);results[slot]=owner;retained=true;
        }
        finally
        {
            for(int w=0;w<wordCount;w++)processed[w]|=affected[w];
            if(!retained)owner?.ReleaseUnretained();
        }
    }
}
