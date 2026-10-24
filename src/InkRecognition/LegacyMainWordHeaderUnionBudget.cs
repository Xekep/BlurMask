namespace InkRecognition;

//Numerical allocation-site bound only. This value grants no source authority.
internal static class LegacyMainWordHeaderUnionBudget
{
    internal readonly record struct Cost(long Payload,long Copies,long Visits,long Arrays);
    internal static Cost BeforeUnion(int retained,int additional)
    {
        if(retained<0||additional<0||retained>65536||additional>65536)
            throw new NotSupportedException("Bounded actual WORD Header30 append required before union allocation.");
        //M is only an allocation upper. Real Union retains its per-input
        //and actual per-Append output65536 checks, including duplicate hits.
        long m=checked((long)retained+additional);
        //Union: target clone16T, List backing growth total<=4M+8 slots,
        //final array<=M; metadata64. Each append shifts/removes<=M ranges.
        long arrays=checked(128+16L*retained+80*m);
        return new(checked(64+arrays),checked(128+32L*retained+16L*additional+64*m+48L*additional*m),
            checked(256+128L*(additional+1)*(m+1)),arrays);
    }
}
