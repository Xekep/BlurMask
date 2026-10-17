namespace InkRecognition;

/// <summary>Internal notifications at actual ordinary allocation sites. Existing
/// public graph projections do not expose native header/pointer layouts.</summary>
internal interface ILegacyOrdinaryGraphAllocations
{
    void SeedArc(int node,int ordinal,LegacyOrdinaryCursiveSeedArc arc);
    void MergedGroup(int group,int leftGroup,int rightGroup);
    void MergedArc(int node,int ordinal,int group,int leftNode,int leftOrdinal);
}
