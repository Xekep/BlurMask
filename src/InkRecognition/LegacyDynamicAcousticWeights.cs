namespace InkRecognition;

/// <summary>Explicit type257 configuration; no resource defaults are inferred.</summary>
public readonly record struct LegacyDynamicAcousticWeightProfile(int SampleCount,
    float RadiusFraction, float ExclusionFraction, float SameStrokeFactor,
    float SineLimit, float LogisticCenter, float LogisticSlope,
    float LowerWeight, float UpperWeight);

public readonly record struct LegacyDynamicAcousticWeightResult(float Overlap,
    float Mode1Weight, float Mode2Weight, int ResampledPointCount);

/// <summary>
/// Type257 geometric parallel-segment overlap and complementary acoustic
/// weights (ce610/cece0, getters cebc0/cebd0/cebe0). This is not a baseline
/// position estimator or a neural network. Resource configuration stays explicit.
/// </summary>
public static class LegacyDynamicAcousticWeights
{
    public static LegacyDynamicAcousticWeightResult Evaluate(IReadOnlyList<InkStroke> strokes,
        LegacyDynamicAcousticWeightProfile profile, CancellationToken cancellationToken = default)
        =>EvaluateWithConfiguredDebit(strokes,profile,cancellationToken,null);
    internal static LegacyDynamicAcousticWeightResult EvaluateWithConfiguredDebit(IReadOnlyList<InkStroke> strokes,
        LegacyDynamicAcousticWeightProfile profile,CancellationToken cancellationToken,LegacyConfiguredSourceOperationDebit? debit)
    {
        ArgumentNullException.ThrowIfNull(strokes); cancellationToken.ThrowIfCancellationRequested();
        Validate(profile);
        if(strokes.Count is < 1 or > 256) throw new ArgumentException("Expected 1..256 physical strokes.", nameof(strokes));
        int sourceCount=0;
        foreach(var stroke in strokes)
        {
            ArgumentNullException.ThrowIfNull(stroke);
            if(stroke.Points.Count<1 || stroke.Points.Count>4096-sourceCount)
                throw new ArgumentException("Expected 1..4096 total source points.", nameof(strokes));
            sourceCount+=stroke.Points.Count;
            foreach(var p in stroke.Points)
                if(!float.IsFinite(p.X)||!float.IsFinite(p.Y)||Math.Abs(p.X)>1e6f||Math.Abs(p.Y)>1e6f)
                    throw new ArgumentException("Finite bounded float geometry required.", nameof(strokes));
        }
        debit?.DynamicInput(sourceCount,cancellationToken);
        var (points,owners)=Resample(strokes,profile.SampleCount,cancellationToken,debit);
        float minX=points[0].X,maxX=minX,minY=points[0].Y,maxY=minY;
        foreach(var p in points){minX=Math.Min(minX,p.X);maxX=Math.Max(maxX,p.X);minY=Math.Min(minY,p.Y);maxY=Math.Max(maxY,p.Y);}
        float extent=Math.Max(maxX-minX,maxY-minY);
        float radius=extent*profile.RadiusFraction, exclusion=extent*profile.ExclusionFraction;
        float radiusSquare=radius*radius,exclusionSquare=exclusion*exclusion;
        int segments=points.Length-1;
        var dx=new float[segments];var dy=new float[segments];var mx=new float[segments];var my=new float[segments];var lengths=new float[segments];
        for(int i=0;i<segments;i++)
        {
            dx[i]=points[i+1].X-points[i].X;dy[i]=points[i+1].Y-points[i].Y;
            mx[i]=(points[i].X+points[i+1].X)*.5f;my[i]=(points[i].Y+points[i+1].Y)*.5f;
            lengths[i]=Root(dx[i]*dx[i]+dy[i]*dy[i]);
        }
        double sum=0;
        for(int i=0;i<segments;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if(owners[i]!=owners[i+1]||lengths[i]==0)continue;
            float best=0;
            int before=i-1;
            while(before>=0&&owners[before]==owners[i]&&DistanceSquare(i,before)<=exclusionSquare)before--;
            for(int j=before;j>=0;j--)Consider(j);
            int after=i+1;
            while(after<segments&&owners[after]==owners[i]&&DistanceSquare(i,after)<=exclusionSquare)after++;
            for(int j=after;j<segments;j++)Consider(j);
            sum+=best;

            float DistanceSquare(int a,int b){float x=mx[a]-mx[b],y=my[a]-my[b];return y*y+x*x;}
            void Consider(int j)
            {
                if(owners[j]!=owners[j+1]||lengths[j]==0)return;
                float square=DistanceSquare(i,j);
                if(square>=radiusSquare)return;
                float sine=Math.Abs(dx[i]*dy[j]-dy[i]*dx[j])/(lengths[j]*lengths[i]);
                if(sine>=profile.SineLimit)return;
                float value=((radius-Root(square))/radius)*((profile.SineLimit-sine)/profile.SineLimit);
                if(owners[i]==owners[j])value*=profile.SameStrokeFactor;
                if(value>best)best=value;
            }
        }
        double mean=segments>0?sum/segments:sum;
        float lower=profile.LowerWeight,upper=profile.UpperWeight;
        // Constructor ce4b0 resets equal endpoints to [0,1].
        if(lower==upper){lower=0;upper=1;}
        double exponent=((1-mean)-profile.LogisticCenter)*-(double)profile.LogisticSlope;
        float mode2=(float)((1/(Math.Exp(exponent)+1))*Math.Abs((double)(upper-lower))+lower);
        return new((float)mean,1f-mode2,mode2,points.Length);
    }

    private static (InkPoint[] Points,int[] Owners) Resample(IReadOnlyList<InkStroke> source,int target,CancellationToken token,LegacyConfiguredSourceOperationDebit? debit)
    {
        float length=0;
        foreach(var stroke in source)
            for(int i=1;i<stroke.Points.Count;i++){float x=stroke.Points[i].X-stroke.Points[i-1].X,y=stroke.Points[i].Y-stroke.Points[i-1].Y;length+=Root(y*y+x*x);}
        float step=length/(target-1);
        var points=new List<InkPoint>();var owners=new List<int>();
        void Add(InkPoint p,int owner){debit?.DynamicPoint(token);if(points.Count==4096)throw new ArgumentException("Resampled geometry exceeds 4096 points.");points.Add(p);owners.Add(owner);}
        if(step==0)
        {
            for(int s=0;s<source.Count;s++)Add(source[s].Points[0],s);
            debit?.DynamicSamples(points.Count,token);
        return(points.ToArray(),owners.ToArray());
        }
        float carry=0;
        for(int s=0;s<source.Count;s++)
        {
            var stroke=source[s].Points;
            if(s!=0)Add(stroke[0],s);
            for(int i=1;i<stroke.Count;i++)
            {
                token.ThrowIfCancellationRequested();
                var a=stroke[i-1];var b=stroke[i];float x=b.X-a.X,y=b.Y-a.Y;
                float total=carry+Root(y*y+x*x);
                if(total<step){carry=total;continue;}
                float segment=total-carry;
                float tolerance=Math.Max(Math.Abs(segment)*2.3841858e-7f,1.1920929e-7f);
                if(Math.Abs(segment)<=tolerance){Add(b,s);carry=segment;continue;}
                float fraction=(step-carry)/segment;
                float remaining=carry+(segment-step);
                while(true)
                {
                    Add(new((1f-fraction)*a.X+fraction*b.X,(1f-fraction)*a.Y+fraction*b.Y),s);
                    fraction+=step/segment;
                    if(fraction>=1)break;
                    remaining-=step;
                }
                carry=remaining;
            }
        }
        if(carry/step>.5f)Add(source[^1].Points[^1],source.Count-1);
        if(points.Count==0)throw new InvalidDataException("Unsupported empty native resampling result.");
        debit?.DynamicSamples(points.Count,token);
        return(points.ToArray(),owners.ToArray());
    }

    private static float Root(float square)=>(float)Math.Sqrt((double)square);
    private static void Validate(LegacyDynamicAcousticWeightProfile p)
    {
        if(p.SampleCount is < 2 or > 512||!float.IsFinite(p.RadiusFraction)||p.RadiusFraction<=0||p.RadiusFraction>10||
            !float.IsFinite(p.ExclusionFraction)||p.ExclusionFraction<0||p.ExclusionFraction>10||
            !float.IsFinite(p.SameStrokeFactor)||p.SameStrokeFactor<0||p.SameStrokeFactor>10||
            !float.IsFinite(p.SineLimit)||p.SineLimit<=0||p.SineLimit>1||
            !float.IsFinite(p.LogisticCenter)||!float.IsFinite(p.LogisticSlope)||
            !float.IsFinite(p.LowerWeight)||p.LowerWeight is < 0 or > 1||!float.IsFinite(p.UpperWeight)||p.UpperWeight is < 0 or > 1||p.LowerWeight>p.UpperWeight)
            throw new ArgumentException("Bounded explicit type257 profile required.",nameof(p));
    }
}
