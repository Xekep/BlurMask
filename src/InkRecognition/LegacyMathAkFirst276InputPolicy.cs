namespace InkRecognition;

public sealed partial class LegacyMathAkFeatureProfile
{
    //Four actual scalar parameters (five field bytes) per PROFILE, distinct native objects/offsets.
    private readonly First276InputPolicy firstInputPolicy;
    internal const int First276InputPolicyPayloadBytes=5;
    internal const int First276InputPolicyCopyBytes=49;
    internal const int First276InputPolicySourceUnits=404; //Derived by SOURCE audit.
    internal First276InputPolicy ReadFirst276InputPolicy(CancellationToken token)
    {token.ThrowIfCancellationRequested();return firstInputPolicy;}
    private struct First276InputPolicyReservation
    {
        private long bytes,copies,work;
        internal void Reserve(long b,long c,long u)
        {
            if(b<0||c<0||u<0||b>First276InputPolicyPayloadBytes-bytes||
                c>First276InputPolicyCopyBytes-copies||u>First276InputPolicySourceUnits-work)
                throw new NotSupportedException("FIRST input policy retention reservation exhausted.");
            bytes=checked(bytes+b);copies=checked(copies+c);work=checked(work+u);
        }
    }
    internal readonly record struct First276InputPolicy(byte SuffixLimit,byte RangeLimit,byte SegmentLimit,ushort InputQuantizer)
    {
        internal static First276InputPolicy ReadAuthenticated(byte[] capture,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var reservation=new First276InputPolicyReservation();
            reservation.Reserve(First276InputPolicyPayloadBytes,First276InputPolicyCopyBytes,First276InputPolicySourceUnits);
            //Sole caller is PrepareCore AFTER full SHA on its private capture.
            //No resource reload, GridScale alias or selected-tail coercion.
            if(capture.Length!=LegacyMathMixed107RawResource.ResourceBytes||
                capture[1790682]!=8||capture[178873]!=4||capture[178874]!=8||capture[8733]!=10||capture[8734]!=0)
                throw new InvalidDataException("Actual FIRST/source policy bytes differ.");
            return new(capture[1790682],capture[178873],capture[178874],(ushort)(capture[8733]|capture[8734]<<8));
        }
    }
}
