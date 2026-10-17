namespace InkRecognition;

/// <summary>218560 ordinary nontracing chain execution over an explicit
/// post-association segment. Segmentation, residual association and packet
/// scalar production remain separate; the actual compact owner supplies2478.</summary>
public static class LegacyPreparedOrdinaryGlyphChain
{
    public static LegacyPreparedGlyphChainResult Apply(LegacyOwnedMainCompactWordMaterialization materialization,
        LegacyNativeWordBuilder word,ReadOnlySpan<byte> preparedSegmentRows72,
        ReadOnlySpan<ulong> residualSourceReferences,LegacyOwnedRussianOrdinaryGlyphProducer ordinary,
        LegacyOwnedRussianProtocolGlyphProducer protocol,ushort previousSymbol,
        uint preparedPacketAcBits,uint preparedPacketB0Bits,bool initialForceDuplicate,
        ReadOnlySpan<byte> allocatedRecordResidue40,ReadOnlySpan<ushort> labelStackResidue256,
        ReadOnlySpan<byte> requestStackResidue64,ulong conversionArgument=0,
        CancellationToken token=default)
        => ApplySelected(LegacyMainSelectedCompactWordOwner.FromSingle(materialization),word,preparedSegmentRows72,residualSourceReferences,ordinary,protocol,previousSymbol,
            preparedPacketAcBits,preparedPacketB0Bits,initialForceDuplicate,allocatedRecordResidue40,
            labelStackResidue256,requestStackResidue64,conversionArgument,token);

    public static LegacyPreparedGlyphChainResult ApplyConfigured(LegacyOwnedMainSecondaryCompactTerminal terminal,
        LegacyNativeWordBuilder word,ReadOnlySpan<byte> preparedSegmentRows72,
        ReadOnlySpan<ulong> residualSourceReferences,LegacyOwnedRussianOrdinaryGlyphProducer ordinary,
        LegacyOwnedRussianProtocolGlyphProducer protocol,ushort previousSymbol,
        uint preparedPacketAcBits,uint preparedPacketB0Bits,bool initialForceDuplicate,
        ReadOnlySpan<byte> allocatedRecordResidue40,ReadOnlySpan<ushort> labelStackResidue256,
        ReadOnlySpan<byte> requestStackResidue64,ulong conversionArgument=0,
        CancellationToken token=default)
        => ApplySelected(LegacyMainSelectedCompactWordOwner.FromSecondary(terminal),word,preparedSegmentRows72,residualSourceReferences,ordinary,protocol,previousSymbol,
            preparedPacketAcBits,preparedPacketB0Bits,initialForceDuplicate,allocatedRecordResidue40,
            labelStackResidue256,requestStackResidue64,conversionArgument,token);

    private static LegacyPreparedGlyphChainResult ApplySelected(LegacyMainSelectedCompactWordOwner materialization,
        LegacyNativeWordBuilder word,ReadOnlySpan<byte> preparedSegmentRows72,
        ReadOnlySpan<ulong> residualSourceReferences,LegacyOwnedRussianOrdinaryGlyphProducer ordinary,
        LegacyOwnedRussianProtocolGlyphProducer protocol,ushort previousSymbol,
        uint preparedPacketAcBits,uint preparedPacketB0Bits,bool initialForceDuplicate,
        ReadOnlySpan<byte> allocatedRecordResidue40,ReadOnlySpan<ushort> labelStackResidue256,
        ReadOnlySpan<byte> requestStackResidue64,ulong conversionArgument=0,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(materialization);ArgumentNullException.ThrowIfNull(word);
        ArgumentNullException.ThrowIfNull(ordinary);ArgumentNullException.ThrowIfNull(protocol);
        token.ThrowIfCancellationRequested();var policy=materialization.Controller;
        var casePolicy=materialization.CaseSourcePolicy;
        bool matchingOrdinary=casePolicy is null?ReferenceEquals(ordinary.DirectPolicy,policy):
            ordinary.DirectPolicy is null&&ReferenceEquals(ordinary.Policy,casePolicy);
        if(!matchingOrdinary||!ReferenceEquals(protocol.CasePolicy,casePolicy)||!ReferenceEquals(protocol.Policy,policy)||
            !ReferenceEquals(protocol.SourceCallbacks.Owner,materialization.ConfidenceOwner))
            throw new ArgumentException("One actual typed compact/source/score owner required.");
        return LegacyMainOrdinaryGlyphChainKernel.Apply(word,preparedSegmentRows72,residualSourceReferences,
            policy.Owner.SourceGraph.NodeCount,materialization.RecordCount,materialization.CopyNodeStarts2478(),
            materialization.CopyCompactStorage(),node=>policy.Owner.SourceGraph.GetNode(node).Count,
            ordinary.Produce,(row,compact,scratch,ac,b0,request,cancellation)=>protocol.Produce(row,compact,scratch,ac,b0,request,cancellation).Finalization,
            previousSymbol,preparedPacketAcBits,preparedPacketB0Bits,initialForceDuplicate,allocatedRecordResidue40,
            labelStackResidue256,requestStackResidue64,conversionArgument,token);
    }
}

public readonly record struct LegacyPreparedGlyphChainResult(bool Succeeded,ulong FirstNewReference,
    ulong LastProcessedReference,int CreatedCount,int ReusedCount);
