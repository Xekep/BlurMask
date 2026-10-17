using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyMainWordPublication.Probe")]
namespace InkRecognition;

/// <summary>Fresh ordinary21FE30 callback over actual CASE flush associations.
/// This publishes linked words; final2271F0 alternatives/quote/21F320 remain
/// separate. Explicit stack/allocator residue is never inferred as zeros.</summary>
public sealed class LegacyOwnedMainPlainWordPublication
{
    private readonly LegacyOwnedRussianOrdinaryGlyphProducer ordinary;
    private readonly LegacyOwnedRussianProtocolGlyphProducer protocol;
    public LegacyOwnedMainGlyphFlushSession Session {get;}
    public uint TopAcBits=>2;
    public uint TopB0Bits {get;}
    private LegacyOwnedMainPlainWordPublication(LegacyOwnedMainGlyphFlushSession session,
        LegacyOwnedRussianOrdinaryGlyphProducer ordinary,LegacyOwnedRussianProtocolGlyphProducer protocol,uint b0)
    {Session=session;this.ordinary=ordinary;this.protocol=protocol;TopB0Bits=b0;}

    public static LegacyOwnedMainPlainWordPublication Prepare(ReadOnlySpan<byte> resource,
        LegacyOwnedMainGlyphFlushSession session,LegacyCursiveSeparationTable table,
        LegacyOwnedRussianMainSourceSymbolCallbacks callbacks,LegacyOwnedRussianMainInteriorDelimiter interior,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(session);
        if(session.Ownership.SecondaryTerminal is not null)
            throw new NotSupportedException("Configured callback preparation requires PrepareConfigured.");
        return PrepareSelected(resource,session,table,callbacks,interior,token);
    }

    public static LegacyOwnedMainPlainWordPublication PrepareConfigured(ReadOnlySpan<byte> resource,
        LegacyOwnedMainGlyphFlushSession session,LegacyCursiveSeparationTable table,
        LegacyOwnedRussianMainSourceSymbolCallbacks callbacks,LegacyOwnedRussianMainInteriorDelimiter interior,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(session);
        if(session.Ownership.SecondaryTerminal is null)
            throw new ArgumentException("Actual configured selected-terminal flush session required.");
        return PrepareSelected(resource,session,table,callbacks,interior,token);
    }

    private static LegacyOwnedMainPlainWordPublication PrepareSelected(ReadOnlySpan<byte> resource,
        LegacyOwnedMainGlyphFlushSession session,LegacyCursiveSeparationTable table,
        LegacyOwnedRussianMainSourceSymbolCallbacks callbacks,LegacyOwnedRussianMainInteriorDelimiter interior,
        CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(session);ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(callbacks);ArgumentNullException.ThrowIfNull(interior);token.ThrowIfCancellationRequested();
        var ownership=session.Ownership;var policy=session.Plan.SourcePolicy;
        if(session.IsStopped||session.CompletedFlushCount!=0||policy.HasAssociatedSource||
            !ReferenceEquals(callbacks.Owner,ownership.SelectedOwner.ConfidenceOwner)||
            !ReferenceEquals(interior.Geometry,ownership.Geometry))
            throw new ArgumentException("Fresh exact same-owned ordinary source, geometry and first flush session required.");
        var ordinary=LegacyOwnedRussianOrdinaryGlyphProducer.Prepare(resource,policy,callbacks.Owner,table,token);
        var protocol=LegacyOwnedRussianProtocolGlyphProducer.PrepareCase(resource,policy,callbacks,interior,token);
        //2271F0 assigns AC=2 without an associated source. Its first221840
        //pass has no publication callback, so2188F0 sees each unassociated
        //native flush in reverse row order. Packet seed bytes are not used by
        //this width predicate: only actual compact subtype/source/row38/3A.
        if(!ownership.TryReadTopWidthStatistics(out uint widthBits))
        {
            if(!ReferenceEquals(ownership,ownership.TopOwner))
                throw new InvalidOperationException("The first alternative's genuine width pass must precede another TOP callback.");
            var width=ownership.SecondaryTerminal is {} secondary
                ? LegacyOwnedMainSourceWidthStatistics.PrepareConfigured(secondary,ownership.Geometry)
                : LegacyOwnedMainSourceWidthStatistics.Prepare(ownership.Materialization,ownership.Geometry);
            LegacyMainSourceWidthStatistics sum=default;
            foreach(var flush in session.Plan.Flushes)sum=width.AccumulatePreparedSegment(flush.CopyRows72(),sum,token);
            token.ThrowIfCancellationRequested();
            widthBits=ownership.EstablishTopWidthStatistics(BitConverter.SingleToUInt32Bits(sum.AverageWidth));
        }
        token.ThrowIfCancellationRequested();return new(session,ordinary,protocol,widthBits);
    }

    public LegacyOwnedMainWordPublicationResult Publish(LegacyOwnedMainAssociatedGlyphFlush association,
        ReadOnlySpan<byte> allocatedRecordResidue40,ReadOnlySpan<ushort> labelStackResidue256,
        ReadOnlySpan<byte> requestStackResidue64,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(association);
        if(!ReferenceEquals(association.Owner,Session))throw new ArgumentException("Publication belongs to a different flush session.");
        if(allocatedRecordResidue40.Length!=40||labelStackResidue256.Length!=256||requestStackResidue64.Length!=64)
            throw new ArgumentException("Complete explicit record/label/request residue required.");
        Session.ValidateCurrent(association,token);association.BeginPublication(token);
        try
        {
            var word=association.WorkingWord;var packet=association.Packet;bool succeeded=false;
            if(association.Succeeded)
            {
                var residual=association.CopyResidualStorage();
                //2271F0 clears the callback's low-byte duplicate argument for
                //this ordinary first-result branch; conversion argument is0.
                var secondary=Session.Ownership.SecondaryTerminal;
                succeeded=(secondary is null
                    ? LegacyPreparedOrdinaryGlyphChain.Apply(Session.Ownership.Materialization,word,association.CopyRows72(),
                        residual.AsSpan(0,association.RemainingCount),ordinary,protocol,packet.PreviousSymbol,
                        TopAcBits,TopB0Bits,false,allocatedRecordResidue40,labelStackResidue256,requestStackResidue64,0,token)
                    : LegacyPreparedOrdinaryGlyphChain.ApplyConfigured(secondary,word,association.CopyRows72(),
                        residual.AsSpan(0,association.RemainingCount),ordinary,protocol,packet.PreviousSymbol,
                        TopAcBits,TopB0Bits,false,allocatedRecordResidue40,labelStackResidue256,requestStackResidue64,0,token)).Succeeded;
            }
            //21FE30 asks for span even when218560 failed. Zero-span retains chain success
            //without allocating a word; positive-span requires both steps.
            int span=word.GetSymbolSpan(token);LegacyNativePublishedWordNode? published=null;
            if(span>0)
            {
                if(succeeded)published=Session.Ownership.WordPublisher.Publish(word,First,Last,
                    packet.FirstBoundaryOverride,packet.LastBoundaryOverride,token);
                succeeded=published is not null;
            }
            word.ResetPlain(token);
            return new(association,succeeded,published);
        }
        catch{Session.AbortPublication(association);throw;}
    }

    private LegacyNativeResidualSource Base(int node,ushort arc)
    {
        ulong key=((uint)node&0xffffffUL)|((ulong)arc<<24);
        return LegacyNativeResidualSource.Decode(Session.SourceOverlay.ResolveBaseReference(key));
    }
    private int First(int node,ushort arc)
    {var source=Base(node,arc);return unchecked((int)Session.SourceOverlay.Graph.GetNode(source.Node)[source.Arc].StartContextId);}
    private int Last(int node,ushort arc)
    {var source=Base(node,arc);return unchecked((int)Session.SourceOverlay.Graph.GetNode(source.Node)[source.Arc].EndContextId);}
}
