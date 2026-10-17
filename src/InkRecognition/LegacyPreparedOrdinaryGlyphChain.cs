using System.Buffers.Binary;

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
        if(preparedSegmentRows72.Length%72!=0||preparedSegmentRows72.Length>4096*72||
            allocatedRecordResidue40.Length!=40||labelStackResidue256.Length!=256||requestStackResidue64.Length!=64)
            throw new ArgumentException("Bounded complete segment and explicit record/label/request storage required.");
        if(!residualSourceReferences.IsEmpty)
            throw new NotSupportedException("Residual217DB0 association must be performed by its genuine source overlay before this executor.");
        int[] starts=materialization.CopyNodeStarts2478();byte[] compact=materialization.CopyCompactStorage();
        var existing=word.CopyChain(token);ulong tail=existing.Length==0?ulong.MaxValue:existing[^1];
        if(existing.Length+preparedSegmentRows72.Length/72>4096)
            throw new NotSupportedException("Completed chain would exceed the WORD owner's portable4096-reference budget.");
        ulong previous=ulong.MaxValue,firstNew=ulong.MaxValue;bool forceCarry=initialForceDuplicate;
        int count=preparedSegmentRows72.Length/72,created=0,reused=0;
        for(int i=count-1;i>=0;i--)
        {
            token.ThrowIfCancellationRequested();var row=preparedSegmentRows72.Slice(i*72,72);
            int node=I32(row,0),index=I32(row,8),neighbor=I32(row,32);ushort arc=U16(row,4);
            var graph=policy.Owner.SourceGraph;
            if((uint)node>=(uint)graph.NodeCount||arc>=graph.GetNode(node).Count||
                (uint)index>=(uint)materialization.RecordCount||index<starts[node]||index>=starts[node+1]||
                neighbor!=-1&&(uint)neighbor>=(uint)materialization.RecordCount)
                throw new ArgumentException("Prepared segment row must resolve into its actual compact/source owner.");
            ulong key=BinaryPrimitives.ReadUInt64LittleEndian(row[48..]);
            int descriptor=unchecked(index-starts[node]);
            bool force=row[68]!=0||(key&(1UL<<39))!=0||forceCarry;
            var insertion=word.AppendGlyph(key,descriptor,force,allocatedRecordResidue40,token);
            if(insertion.ExistingDescriptor&&previous!=ulong.MaxValue&&I32(row,12)!=-1&&
                BinaryPrimitives.ReadUInt64LittleEndian(word.Pool.CopyRecord(insertion.Reference,token).AsSpan(32))!=previous)
                insertion=word.AppendGlyph(key,descriptor,true,allocatedRecordResidue40,token);
            ulong current=insertion.Reference;
            if(!insertion.ExistingDescriptor)
            {
                byte[] record=word.Pool.CopyRecord(current,token);
                BinaryPrimitives.WriteUInt64LittleEndian(record.AsSpan(32),previous);
                word.Pool.ReplaceRecord(current,record,token);forceCarry=true;
                var scratch=new LegacyOrdinaryGlyphProductionScratch(labelStackResidue256,256);
                LegacyPreparedGlyphFinalizationInput finalized;
                if(row[60]==0)
                    finalized=ordinary.Produce(row,scratch,conversionArgument,compact,token);
                else if(row[60]==3)
                    finalized=protocol.Produce(row,compact,scratch,preparedPacketAcBits,preparedPacketB0Bits,requestStackResidue64,token).Finalization;
                else throw new NotSupportedException("Additional216BC0 subtype writers remain separate.");
                //The current ordinary and protocol scalar branches do not
                //consume2173E0's previous-symbol input; retain its real value
                //at this boundary instead of substituting a guessed marker.
                _=i>0?U16(preparedSegmentRows72,(i-1)*72+56):previousSymbol;
                if(!LegacyPreparedGlyphFinalization.Apply(word.Pool,current,row,finalized,false,token))
                    return new(false,firstNew,previous,created,reused);
                if(firstNew==ulong.MaxValue)firstNew=current;
                created++;
            }
            else reused++;
            previous=current;
        }
        if(tail==ulong.MaxValue)word.ReplaceHeadReference(previous,token);
        else
        {
            byte[] last=word.Pool.CopyRecord(tail,token);
            BinaryPrimitives.WriteUInt64LittleEndian(last.AsSpan(32),previous);
            word.Pool.ReplaceRecord(tail,last,token);
        }
        return new(true,firstNew,previous,created,reused);
    }

    private static ushort U16(ReadOnlySpan<byte> bytes,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(bytes[at..]);
    private static int I32(ReadOnlySpan<byte> bytes,int at)=>BinaryPrimitives.ReadInt32LittleEndian(bytes[at..]);
}

public readonly record struct LegacyPreparedGlyphChainResult(bool Succeeded,ulong FirstNewReference,
    ulong LastProcessedReference,int CreatedCount,int ReusedCount);
