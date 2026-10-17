namespace InkRecognition;

//Actual global adapter. No public prepared producer/cost callbacks and no
//single-descriptor materialization wrapper are substituted for this source.
internal static class LegacyFreshMainGlobalGlyphChain
{
    internal static LegacyPreparedGlyphChainResult Apply(LegacyFreshMainGlobalPlainResultOwnership ownership,
        LegacyNativeWordBuilder word,LegacyFreshMainGlobalAssociatedGlyphFlush association,
        LegacyFreshMainGlobalOrdinaryGlyphProducer ordinary,LegacyFreshMainGlobalProtocolGlyphProducer protocol,
        ushort previousSymbol,uint topAC,uint topB0,ReadOnlySpan<byte> recordResidue40,
        ReadOnlySpan<ushort> labelStackResidue256,ReadOnlySpan<byte> requestResidue64,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(ownership);ArgumentNullException.ThrowIfNull(word);ArgumentNullException.ThrowIfNull(association);
        ArgumentNullException.ThrowIfNull(ordinary);ArgumentNullException.ThrowIfNull(protocol);token.ThrowIfCancellationRequested();
        var materialization=ownership.Materialization;var source=association.Session.SourceOverlay.Geometry;
        if(!ReferenceEquals(word.Pool,ownership.GlyphPool)||!ReferenceEquals(source,ownership.Geometry)||
            !ReferenceEquals(association.Flush.BacktrackRows.Materialization,materialization)||
            !ReferenceEquals(ordinary.Geometry,source)||!ReferenceEquals(protocol.Geometry,source))
            throw new ArgumentException("Same actual global glyph/pool/flush/producer source required.");
        var residual=association.CopyResidualStorage();
        return LegacyMainOrdinaryGlyphChainKernel.Apply(word,association.CopyRows72(),residual.AsSpan(0,association.RemainingCount),
            source.Graph.State.NodeCount,materialization.RecordCount,materialization.CopyNodeStarts2478(),materialization.CopyCompactStorage(),
            node=>source.Graph.State.GetNode(node).Count,ordinary.Produce,
            (row,compact,scratch,ac,b0,request,cancellation)=>protocol.Produce(row,compact,scratch,ac,b0,request,cancellation).Finalization,
            previousSymbol,topAC,topB0,false,recordResidue40,labelStackResidue256,requestResidue64,0,token);
    }
}
