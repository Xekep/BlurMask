using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyEnglishMainVector10.Probe")]
namespace InkRecognition;

// Native1BCBD0 +13E8E0 default-table branch for actual MAIN four182-output families.
// Internal numerical API; only the owning capture may supply actual group outputs.
internal static class LegacyEnglishCaseMainConfidenceKernel
{
    internal static float Calibrate(int probability32768)
    {
        if((uint)probability32768>32768)throw new ArgumentOutOfRangeException(nameof(probability32768));
        float probability=probability32768*(1f/32768);
        double exponent=Math.Exp((double)probability*-.575d);
        return exponent>=float.MaxValue?0f:(float)(1d/(exponent+1d));
    }
    internal static float[] Evaluate(LegacyEnglishCaseMainConfidenceProfile profile,
        ReadOnlySpan<LegacyAcousticCacheCandidate> candidates,IReadOnlyList<ReadOnlyMemory<int>> family,
        float actualFirstStoredSourceProbability,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(profile);ArgumentNullException.ThrowIfNull(family);token.ThrowIfCancellationRequested();
        if(candidates.Length>150||family.Count!=4)throw new ArgumentException("Bounded original four-family MAIN confidence inputs required.");
        foreach(var values in family)
        {
            if(values.Length!=182)throw new ArgumentException("Actual MAIN182 class domain required.");
            foreach(int value in values.Span)if((uint)value>32768)throw new ArgumentOutOfRangeException(nameof(family));
        }
        var result=new float[candidates.Length];
        for(int i=0;i<candidates.Length;i++)
        {
            token.ThrowIfCancellationRequested();if(i>=20)continue;ushort symbol=candidates[i].Symbol;
            if(symbol==profile.Closed)
            {
                if(!float.IsFinite(actualFirstStoredSourceProbability))throw new InvalidDataException("Finite actual source probability required.");
                result[i]=actualFirstStoredSourceProbability;continue;
            }
            if(!profile.TryResolve(symbol,out ushort output))throw new NotSupportedException("Retained MAIN symbol requires its separate wide/unmapped confidence service.");
            float a=Calibrate(family[0].Span[output]),b=Calibrate(family[1].Span[output]);
            float sum=b+a;sum+=Calibrate(family[2].Span[output]);sum+=Calibrate(family[3].Span[output]);
            sum+=0f;sum+=0f;sum+=0f;sum+=0f;float mean=sum/4;result[i]=mean>=1?1:mean;
        }
        token.ThrowIfCancellationRequested();return result;
    }
}
