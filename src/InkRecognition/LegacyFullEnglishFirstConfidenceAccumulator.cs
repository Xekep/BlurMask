namespace InkRecognition;

/// <summary>Invocation-private182-float streaming storage, not retained NN history.</summary>
internal sealed class LegacyFullEnglishFirstConfidenceAccumulator
{
    private readonly LegacyFullEnglishFirstController owner;
    private readonly LegacyFullEnglishFirstAcousticCache cache;
    private readonly int group;
    private readonly float[] sums=new float[182];
    private int family;
    private bool failed;
    internal LegacyFullEnglishFirstConfidenceAccumulator(LegacyFullEnglishFirstController owner,LegacyFullEnglishFirstAcousticCache cache,int group)
    {this.owner=owner;this.cache=cache;this.group=group;}
    internal bool Matches(LegacyFullEnglishFirstController controller,LegacyFullEnglishFirstCachedGroup actual)
        =>ReferenceEquals(owner,controller)&&ReferenceEquals(cache,actual.Owner)&&group==actual.GroupId;
    internal static float Calibrate(int probability)
    {if((uint)probability>32768)throw new ArgumentOutOfRangeException(nameof(probability));return(float)(1/(1+Math.Exp(-(double)(probability*(1f/32768))*.575)));}
    internal void Feed(int index,ReadOnlySpan<int> probabilities,CancellationToken token)
    {
        try
        {
            token.ThrowIfCancellationRequested();if(failed||index!=family||index>=4||probabilities.Length!=182)throw new ArgumentException("Same four ordered actual full families required.");
            for(int i=0;i<182;i++){token.ThrowIfCancellationRequested();float value=Calibrate(probabilities[i]);sums[i]=index==0?value:sums[i]+value;}
            family++;
        }
        catch{failed=true;throw;}
    }
    internal float[] Finish(LegacyFullEnglishFirstCachedGroup actual,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(failed||family!=4||!ReferenceEquals(actual.Owner,cache)||actual.GroupId!=group)throw new ArgumentException("Same actual complete four-family group required.");
        var candidates=actual.CopyCandidates();if(candidates.Length>10)throw new NotSupportedException("Actual EE10 final shortlist required.");
        var values=new float[candidates.Length];
        for(int i=0;i<values.Length;i++)
        {
            token.ThrowIfCancellationRequested();ushort symbol=candidates[i].Symbol;
            if(symbol==owner.Closed)
            {
                values[i]=ReadStoredProbability(cache,group,token);
            }
            else
            {
                if(!owner.Map.TryResolveOutputLabel(symbol,out ushort output)||output>=182)throw new NotSupportedException("Unresolved actual full probability service.");
                //Native scalar sum family1+0+2+3+three absent zeros+initial0.
                //Finite positive calibrated operands make0+1 exactly commutative.
                values[i]=ReadMean(output);
            }
        }
        return values;
    }
    internal float ReadMean(int output)
    {if(failed||family!=4||(uint)output>=182)throw new InvalidOperationException("Four actual ordered families required.");float sum=sums[output];for(int absent=0;absent<4;absent++)sum+=0f;float value=sum/4;return value>=1?1:value;}
    internal static float ReadStoredProbability(LegacyFullEnglishFirstAcousticCache cache,int group,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(!cache.Owner.IsLive(group))throw new ArgumentException("Actual live full group required.");
        var ranges=cache.Owner.CopyGroupRanges(group);if(ranges.Length==0)throw new InvalidDataException("Actual stored ranges required.");
        var probabilities=cache.Owner.Preparation.OriginalSource.SourceStrokeProbabilities.Span;
        int physical=ranges[0].StrokeIndex;if((uint)physical>=(uint)probabilities.Length)throw new InvalidDataException("Stored physical source identity required.");
        float value=probabilities[physical];if(!float.IsFinite(value)||value<0||value>1)throw new InvalidDataException("Actual source probability required.");return value;
    }
}
