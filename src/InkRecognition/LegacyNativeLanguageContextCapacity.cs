namespace InkRecognition;

/// <summary>204290/20564A..20565F context heap capacity from the actual target
/// bucket's logical uint32 capacity, not its allocated byte extent or count.</summary>
public static class LegacyNativeLanguageContextCapacity
{
    public static int FromHeapCapacity(uint logicalCapacity)
    {
        //Native zero-extends ECX, CVTSI2SS from int64, MULSS by3DCCCCCD,
        //CVTTSS2SI to int64, then unsignedCMOVA against10. The full uint32
        //input domain fits the resulting positive int32 capacity.
        uint scaled = (uint)(long)((float)(ulong)logicalCapacity * 0.1f);
        return checked((int)Math.Max(10u, scaled));
    }
}
