using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyOrdinaryGapMixtureProfile(float FirstMean,float FirstVariance,float FirstPrior,
    float SecondMean,float SecondVariance,float SecondPrior)
{
    public static LegacyOrdinaryGapMixtureProfile ReadV100Node(ReadOnlySpan<byte> resource,int offset)
    {
        _=LegacyOrdinaryGapProfile.ReadV100Node(resource,offset);
        float[] values=new float[6];for(int i=0;i<values.Length;i++)values[i]=BinaryPrimitives.ReadSingleLittleEndian(resource[(offset+16+i*4)..]);
        var p=new LegacyOrdinaryGapMixtureProfile(values[0],values[1],values[2],values[3],values[4],values[5]);p.Validate();return p;
    }
    internal void Validate()
    {
        if(!float.IsFinite(FirstMean)||!float.IsFinite(SecondMean)||!float.IsFinite(FirstVariance)||!float.IsFinite(SecondVariance)||
            FirstVariance<=0||SecondVariance<=0||!float.IsFinite(FirstPrior)||!float.IsFinite(SecondPrior)||FirstPrior<=0||SecondPrior<=0||
            !float.IsFinite(FirstPrior+SecondPrior))throw new ArgumentException("Finite positive native29B mixture variances/priors required.");
    }
}

public sealed record LegacyOrdinaryGapMixtureAttempt(int Index,int Iterations,float FirstMean,float SecondMean,
    float FirstVariance,float SecondVariance,float FirstPrior,float SecondPrior,double Intersection);
public sealed record LegacyOrdinaryGapMixtureResult(double Intersection,IReadOnlyList<LegacyOrdinaryGapMixtureAttempt> Attempts);

/// <summary>Text175350 active29B fixed-variance two-component EM with native
/// profile pseudo-observations, float mean/prior writes, density guard and up to
/// five variance-shrinking retries. Caller observations must be the native
/// qualifying-gap vector, not all gaps or OCR labels.</summary>
public static class LegacyNativeOrdinaryGapMixture
{
    public static LegacyOrdinaryGapMixtureResult Fit(LegacyOrdinaryGapMixtureProfile profile,ReadOnlySpan<float> qualifyingGaps,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(profile);profile.Validate();cancellationToken.ThrowIfCancellationRequested();
        if(qualifyingGaps.Length>4096)throw new ArgumentException("Bounded native gap vector required.");
        float[] observations=qualifyingGaps.ToArray();foreach(float value in observations)if(!float.IsFinite(value)||value<0||value>4)
            throw new ArgumentException("Native normalized gaps must be finite0..4.");
        float seedMass=profile.FirstPrior+profile.SecondPrior;
        double seedFirst=(double)(profile.FirstPrior/seedMass),seedSecond=(double)(profile.SecondPrior/seedMass);
        float previousFirst=-1,previousSecond=-1;var attempts=new List<LegacyOrdinaryGapMixtureAttempt>();double intersection=0;
        for(int attempt=0;attempt<5;attempt++)
        {
            float firstMean=profile.FirstMean,secondMean=profile.SecondMean;
            float firstVariance=profile.FirstVariance/(float)(1<<attempt),secondVariance=profile.SecondVariance/(float)(1<<attempt);
            float firstPrior=profile.FirstPrior,secondPrior=profile.SecondPrior;int iterations=0;
            do
            {
                cancellationToken.ThrowIfCancellationRequested();if(++iterations>4096)throw new InvalidDataException("Native29B convergence exceeded portable bound.");
                double firstWeight=seedFirst,secondWeight=seedSecond,mass=(double)seedMass;
                double firstWeighted=(double)firstMean*seedFirst,secondWeighted=(double)secondMean*seedSecond;
                foreach(float x in observations)
                {
                    double d=(double)x-firstMean;double firstDensity=Math.Exp(((d*-.5)*d)/(double)firstVariance)*(double)firstPrior;
                    d=(double)x-secondMean;double secondDensity=Math.Exp(((d*-.5)*d)/(double)secondVariance)*(double)secondPrior;
                    double density=secondDensity+firstDensity;
                    double threshold=Math.Max(Math.Abs(density)*4.440892098500626e-16,2.220446049250313e-16);
                    if(threshold<Math.Abs(density))
                    {
                        mass+=1;double first=firstDensity/density,second=secondDensity/density;
                        firstWeight+=first;firstWeighted+=(double)x*first;secondWeighted+=(double)x*second;secondWeight+=second;
                    }
                }
                firstMean=(float)(firstWeighted/firstWeight);secondMean=(float)(secondWeighted/secondWeight);
                firstPrior=(float)(firstWeight/mass);secondPrior=(float)(secondWeight/mass);
                if(!float.IsFinite(firstMean)||!float.IsFinite(secondMean)||!float.IsFinite(firstPrior)||!float.IsFinite(secondPrior))
                    throw new InvalidDataException("Native29B mixture state overflow.");
                float firstChange=Math.Abs(firstMean-previousFirst),secondChange=Math.Abs(secondMean-previousSecond);
                if((double)firstChange<1e-6&&(double)secondChange<1e-6)break;
                previousFirst=firstMean;previousSecond=secondMean;
            }while(true);
            double firstCoefficient=-.5/(double)firstVariance,secondCoefficient=-.5/(double)secondVariance;
            double quadratic=firstCoefficient-secondCoefficient;
            double firstLinear=(double)firstMean*firstCoefficient,secondLinear=(double)secondMean*secondCoefficient;
            double halfLinear=secondLinear-firstLinear;
            double ratio=((double)firstPrior/Math.Sqrt((double)firstVariance*6.283185307179586))/
                ((double)secondPrior/Math.Sqrt((double)secondVariance*6.283185307179586));
            double constant=Math.Log(ratio)+(firstLinear*(double)firstMean-secondLinear*(double)secondMean);
            double discriminant=halfLinear*halfLinear-quadratic*constant;
            if(quadratic==0)intersection=(constant*-.5)/halfLinear;
            else if(discriminant>=0)
            {
                double root=Math.Sqrt(discriminant),plus=(root-halfLinear)/quadratic,minus=(-halfLinear-root)/quadratic;
                double d=plus-firstMean;double plusDensity=Math.Exp(((d*-.5)*d)/firstVariance)*firstPrior;
                d=minus-firstMean;double minusDensity=Math.Exp(((d*-.5)*d)/firstVariance)*firstPrior;
                intersection=minusDensity<=plusDensity?plus:minus;
            }
            else intersection=(double)(firstMean+secondMean)*.5;
            if(!double.IsFinite(intersection))throw new InvalidDataException("Native29B mixture intersection is nonfinite.");
            attempts.Add(new(attempt,iterations,firstMean,secondMean,firstVariance,secondVariance,firstPrior,secondPrior,intersection));
            if((double)Math.Abs(firstMean-secondMean)>1e-6&&firstMean<intersection&&intersection<secondMean)break;
        }
        return new(intersection,attempts.AsReadOnly());
    }
}
