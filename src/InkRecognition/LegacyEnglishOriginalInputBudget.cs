namespace InkRecognition;

//Independent frontend-preparation ledger. No later Node0 balance is credited.
internal static class LegacyEnglishOriginalInputBudget
{
    internal readonly record struct Reservation(long PayloadCharged,long CopyCharged,long SourceVisitsCharged)
    {
        internal void Require()
        {
            if(PayloadCharged!=MaximumPayload||CopyCharged!=MaximumCopy||SourceVisitsCharged!=MaximumSourceVisits)
                throw new InvalidOperationException("The entire original-input preparation must be prepaid.");
        }
    }
    private const long N=256,P=4096;
    internal const long MaximumPayload=191442L+1408L*N+48L*P+0L*N*N;
    internal const long MaximumCopy=382362L+2816L*N+160L*P+0L*N*N;
    internal const long MaximumSourceVisits=11342224L+2752L*N+416L*P+4L*N*N;
    internal static Reservation PrechargeCompleteRawDomain(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        //Fixed full1..256/4096 domain avoids unsafe small reservations if a
        //caller changes its list between a preliminary count and Snapshot.
        //The value is the complete one-shot charge, never refunded/replaced.
        return new(MaximumPayload,MaximumCopy,MaximumSourceVisits);
    }
}
