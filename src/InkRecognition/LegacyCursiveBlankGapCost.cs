namespace InkRecognition;

/// <summary>Scalar1941B0 arc cache10 for ordinary kind5. Geometry ownership,
/// source flags and dispatch/eligibility are separate producers.</summary>
public static class LegacyCursiveBlankGapCost
{
    public static int Evaluate(float currentMinimumX,float currentMaximumX,float prefixMaximumX,
        float lineHeight,float threshold70,float slope108,bool useMaximumX10C,ReadOnlySpan<float> logTable256)
    {
        if(!float.IsFinite(currentMinimumX)||!float.IsFinite(currentMaximumX)||currentMinimumX>currentMaximumX||
            !float.IsFinite(prefixMaximumX)||!float.IsFinite(lineHeight)||lineHeight<=0||
            !float.IsFinite(threshold70)||!float.IsFinite(slope108)||logTable256.Length!=256)
            throw new ArgumentException("Finite ordinary bbox/prefix/profile and positive line height required.");
        //Native SUBSS,DIVSS,SUBSS,MULSS in this order, then24D8A0 rounding.
        float x=useMaximumX10C?currentMaximumX:currentMinimumX;
        float transformed=((x-prefixMaximumX)/lineHeight-threshold70)*-slope108;
        if(float.IsNaN(transformed))throw new NotSupportedException("Undefined blank gap arithmetic.");
        if(transformed>4)return 0;
        float log=transformed;
        if(transformed>=-4)log=logTable256[((int)((double)(transformed+4f)*255d+.5d))>>3];
        if(float.IsNaN(log)||log>0)throw new ArgumentException("Nonpositive log probability required.");
        //1941B0 compares the rounded float withIMP before its integer store.
        //Large tails therefore cap, rather than overflowing CVTTSS2SI here.
        if(-log*8192f>=0x4000000)return 0x4000000;
        return LegacyFirstOrdinaryTransitionGeometry.LogCost(transformed,logTable256,true);
    }
}
