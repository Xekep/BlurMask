namespace InkRecognition;
internal sealed class LegacyEnglishPeriodicRequestMembershipIndex
{
    private readonly int[] slots=new int[256];
    internal int Find(LegacyPreparedDictionaryQuery request,ReadOnlySpan<LegacyPreparedDictionaryQuery?> requests,int count,out int free,ILegacyEnglishPeriodicCaseWrapperProbeBudget? budget=null)
    {
        ArgumentNullException.ThrowIfNull(request);
        if(requests.Length!=80||(uint)count>80)throw new ArgumentOutOfRangeException(nameof(count));
        int at=System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(request)&255;
        for(int probe=0;probe<=count;probe++)
        {
            budget?.BeforeProbe(); //32 logical units BEFORE slot/ordinal/request reads
            int encoded=slots[at];
            if(encoded==0){free=at;return -1;}
            int ordinal=encoded-1;
            if((uint)ordinal>=(uint)count||requests[ordinal] is not{} enrolled)
                throw new InvalidDataException("Private query enrollment population changed.");
            if(ReferenceEquals(enrolled,request)){free=-1;return ordinal;}
            at=(at+1)&255;
        }
        throw new InvalidDataException("Private query enrollment probe bound changed.");
    }
    internal void Publish(int slot,int ordinal)
    {
        if((uint)slot>=256||(uint)ordinal>=80||slots[slot]!=0)throw new InvalidOperationException("One empty enrollment-index slot required.");
        slots[slot]=ordinal+1;
    }
    internal void Clear()=>Array.Clear(slots);
}
