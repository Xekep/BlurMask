namespace InkRecognition.Math;

/// <summary>Portable CPU adaptation of the proven last-frame F32 score
/// arithmetic. MathF exp/log replace the unported native CPU dispatch; this is
/// not a bit-equivalence, calibrated confidence or full EEF lifecycle claim.</summary>
internal sealed class LegacyPortableMathLogProbabilities
{
    private readonly float[] logProbabilities,cumulativeCosts;
    private LegacyPortableMathLogProbabilities(float[] logs,float[] costs)
    {logProbabilities=logs;cumulativeCosts=costs;}
    internal float[] CopyLogProbabilities()=>logProbabilities.ToArray();
    internal float[] CopyCumulativeCosts()=>cumulativeCosts.ToArray();
    internal static LegacyPortableMathLogProbabilities TransformLastFrame(
        ReadOnlySpan<float> logits,int frames,int beams,int vocabulary,float temperature,
        ReadOnlySpan<float> previousCosts,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(frames<1||frames>1024||beams<1||beams>16||vocabulary!=297||
            (long)frames*beams*vocabulary!=logits.Length||logits.Length>1_000_000||previousCosts.Length!=beams)
            throw new ArgumentException("Bounded complete time/batch/297-class logits and same-beam prior costs required.");
        if(!float.IsFinite(temperature)||temperature<=0)throw new ArgumentOutOfRangeException(nameof(temperature));
        float inverse=1f/temperature;
        if(!float.IsFinite(inverse))throw new NotSupportedException("Finite inverse temperature required by the portable adapter.");
        foreach(float value in logits)if(!float.IsFinite(value))throw new NotSupportedException("Finite input logits required.");
        foreach(float value in previousCosts)if(!float.IsFinite(value)||value<0)throw new NotSupportedException("Finite nonnegative previous costs required.");
        var logs=new float[beams*vocabulary];var costs=new float[logs.Length];
        int start=(frames-1)*beams*vocabulary;
        for(int beam=0;beam<beams;beam++)
        {
            token.ThrowIfCancellationRequested();
            int at=start+beam*vocabulary;float maximum=float.MinValue;
            for(int i=0;i<vocabulary;i++)if(logits[at+i]>maximum)maximum=logits[at+i];
            float sum=0;
            for(int i=0;i<vocabulary;i++)
            {
                float shifted=logits[at+i]-maximum;shifted*=inverse;
                if(!float.IsFinite(shifted))throw new NotSupportedException("Finite scaled differences required by the portable adapter.");
                float exponential=MathF.Exp(shifted);sum+=exponential;
            }
            float logSum=MathF.Log(sum);float scaledMaximum=maximum*inverse;
            float normalizer=logSum+scaledMaximum;
            for(int i=0;i<vocabulary;i++)
            {
                float value=logits[at+i]*inverse;value-=normalizer;
                if(!float.IsFinite(value))throw new NotSupportedException("Finite log probabilities required by the portable adapter.");
                logs[beam*vocabulary+i]=value;
                float cost=BitConverter.UInt32BitsToSingle(BitConverter.SingleToUInt32Bits(value)^0x80000000u);
                cost+=previousCosts[beam];
                if(!float.IsFinite(cost))throw new NotSupportedException("Finite cumulative cost required by the portable adapter.");
                costs[beam*vocabulary+i]=cost;
            }
        }
        return new(logs,costs);
    }
}
