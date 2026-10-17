namespace InkRecognition;

/// <summary>
/// Ordinary sampler297 group geometry193860/195520. A common grouping cost
/// is assigned to every composite category; the native lookup table and
/// source line height remain explicit, rather than guessed from glyph bounds.
/// </summary>
public static class LegacyCursiveGroupSeparationCost
{
    public const int ImpossibleCost=0x4000000;

    public static int Evaluate(LegacyCursivePreparedInk ink,ReadOnlySpan<LegacyInkRange> orderedRanges,
        float resolvedLineHeight,float gapOffset,float slope,ReadOnlySpan<float> nativeLogSigmoid256,
        int maximumCost,bool ordinaryCandidate=true,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(ink);cancellationToken.ThrowIfCancellationRequested();
        if(orderedRanges.Length is <1 or >256 || !float.IsFinite(resolvedLineHeight) || resolvedLineHeight<=0 ||
            !float.IsFinite(gapOffset) || !float.IsFinite(slope) || slope<0 || maximumCost is <0 or >ImpossibleCost ||
            nativeLogSigmoid256.Length!=256)
            throw new ArgumentException("Expected bounded ranges, positive explicit line height, finite geometry profile and256 native table entries.");
        foreach(float value in nativeLogSigmoid256)
            if(!float.IsFinite(value)||value>0)throw new ArgumentException("Native log-probability table must be finite and nonpositive.");
        var counts=ink.StrokePointCounts.Span;var starts=new int[counts.Length];
        for(int i=1;i<starts.Length;i++)starts[i]=starts[i-1]+counts[i-1];
        float[] minimum=new float[orderedRanges.Length],maximum=new float[orderedRanges.Length];
        for(int i=0;i<orderedRanges.Length;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();var range=orderedRanges[i];
            if((uint)range.StrokeIndex>=(uint)counts.Length || range.StartPoint<0 ||
                range.EndPointExclusive<=range.StartPoint || range.EndPointExclusive>counts[range.StrokeIndex])
                throw new ArgumentException("Candidate range lies outside prepared ink.");
            float low=float.MaxValue,high=-float.MaxValue;
            foreach(var point in ink.Points.Span.Slice(starts[range.StrokeIndex]+range.StartPoint,range.EndPointExclusive-range.StartPoint))
            {low=Math.Min(low,point.X);high=Math.Max(high,point.X);}
            minimum[i]=low;maximum[i]=high;
        }
        if(!ordinaryCandidate || orderedRanges.Length==1)return 0;
        float[] suffixMinimum=minimum.ToArray(),suffixMaximum=maximum.ToArray();
        for(int i=minimum.Length-2;i>=0;i--)
        {suffixMinimum[i]=Math.Min(suffixMinimum[i],suffixMinimum[i+1]);suffixMaximum[i]=Math.Max(suffixMaximum[i],suffixMaximum[i+1]);}
        float prefixMinimum=minimum[0],prefixMaximum=maximum[0];int sum=0;
        for(int i=1;i<minimum.Length;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            float gap=Math.Max(suffixMinimum[i]-prefixMaximum,prefixMinimum-suffixMaximum[i]);
            float transformed=(gap/resolvedLineHeight-gapOffset)*-slope;
            if(!float.IsFinite(transformed))throw new ArgumentException("Separation exceeds the finite native arithmetic domain.");
            float logProbability=transformed;
            if(transformed>4)logProbability=0;
            else if(transformed>=-4)
            {
                //193860 ADDSS, CVTSS2SD, MULSD, ADDSD, CVTTSD2SI.
                //Keeping the multiply/add in float can select the next bucket.
                int index=(int)((double)(transformed+4f)*255d+.5d);
                logProbability=nativeLogSigmoid256[index>>3];
            }
            float cost=-logProbability*8192f;
            // The <9 optimized branch saturates each float comparison against
            // the sentinel. The large branch adds signed integers; this API
            // rejects overflow instead of reproducing native wraparound.
            if(minimum.Length<9)
            {
                float rounded=RoundNativeFloat(cost);
                sum=(float)sum+rounded<ImpossibleCost ? checked(sum+(int)rounded):ImpossibleCost;
            }
            else
            {
                float rounded=cost<=0 ? cost-.5f:cost+.5f;
                if(!float.IsFinite(rounded) || rounded>=int.MaxValue)throw new ArgumentException("Separation cost exceeds checked signed arithmetic.");
                sum=checked(sum+(int)rounded);
            }
            prefixMinimum=Math.Min(prefixMinimum,minimum[i]);prefixMaximum=Math.Max(prefixMaximum,maximum[i]);
        }
        return Math.Min(sum,maximumCost);
    }

    // 24d8a0 rounds float bits, with halves away from zero. Adding .5f is
    // different above 2^23: that addition can change an already integral float.
    private static float RoundNativeFloat(float value)
    {
        uint bits=BitConverter.SingleToUInt32Bits(value),magnitude=bits&0x7fffffffu;
        if(magnitude>=0x4b000000u)return value;
        if(magnitude<0x3f800000u)
            bits=(bits&0x80000000u)|(magnitude>=0x3f000000u ? 0x3f800000u:0u);
        else
        {
            int shift=150-(int)(magnitude>>23);
            bits=((bits>>shift)+((bits>>(shift-1))&1u))<<shift;
        }
        return BitConverter.UInt32BitsToSingle(bits);
    }
}
