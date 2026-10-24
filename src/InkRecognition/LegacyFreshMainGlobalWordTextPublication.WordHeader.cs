namespace InkRecognition;
public sealed partial class LegacyFreshMainGlobalWordTextPublication
{
    //Additional parent-header union only. Existing per-choice ranges/copies
    //retain their historical accounting. No batch pre-entry reserve is reused.
    internal void ReserveWordHeaderUnion(LegacyMainWordHeaderUnionBudget.Cost cost,CancellationToken token)
    {
        ValidateLive(token);
        long total=checked(AllWordEnumeratedArrayEnvelopeBytes+cost.Payload);
        if(total>MaximumAllChoiceEnumeratedArrays)
            throw new NotSupportedException("Actual cumulative WORD Header30 arrays exceed existing128MiB envelope.");
        AllWordEnumeratedArrayEnvelopeBytes=total;
        //Includes added union metadata64 in the existing conservative
        //enumerated envelope. No new instance fields or late batch reserve.
        //Cost.Copies/Visits are SOURCE bounds, not a claimed whole Global
        //runtime work meter, CLR/RSS or CPU instruction certificate.
    }
}
