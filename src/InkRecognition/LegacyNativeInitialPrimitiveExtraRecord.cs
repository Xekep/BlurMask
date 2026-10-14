namespace InkRecognition;

/// <summary>Literal nontracing1FD240 B8/22F6/22E8 decision and second1F8D60
/// arguments. It transports an already scored ordinary initial candidate;
/// source/decorator query generation and acoustic costs are not invented.</summary>
public static class LegacyNativeInitialPrimitiveExtraRecord
{
    public static bool TryCreate(LegacyInitialWordRecordFields scoredInitial,
        int originalQueryBaseCost, byte actualGraphKindB8, bool mode22f6,
        byte preparedSourceSummary22e8, ReadOnlySpan<ushort> fourResolvedPrimitiveLabels,
        out LegacyInitialWordRecordFields? extra)
    {
        ArgumentNullException.ThrowIfNull(scoredInitial);
        if (actualGraphKindB8 > 2 || fourResolvedPrimitiveLabels.Length != 4)
            throw new ArgumentException("Actual ordinary graph kind and four exact resolved labels required.");
        extra = null;
        if (actualGraphKindB8 != 0 || mode22f6 || (preparedSourceSummary22e8 & 7) <= 1 ||
            scoredInitial.Label24 == ushort.MaxValue || !fourResolvedPrimitiveLabels.Contains(scoredInitial.Label24))
            return false;
        extra = scoredInitial with
        {
            CandidateCost10 = unchecked(scoredInitial.CandidateCost10 - originalQueryBaseCost),
            Count26 = 0,
            StateBit6 = 0,
            StateBit7 = 1
        };
        return true;
    }
}
