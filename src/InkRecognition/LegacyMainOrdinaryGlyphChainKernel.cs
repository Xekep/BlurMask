using System.Buffers.Binary;
namespace InkRecognition;

internal delegate LegacyPreparedGlyphFinalizationInput LegacyMainOrdinaryGlyphRowProduction(
    ReadOnlySpan<byte> row,LegacyOrdinaryGlyphProductionScratch scratch,ulong conversion,
    ReadOnlySpan<byte> compact,CancellationToken token);
internal delegate LegacyPreparedGlyphFinalizationInput LegacyMainProtocolGlyphRowProduction(
    ReadOnlySpan<byte> row,ReadOnlySpan<byte> compact,LegacyOrdinaryGlyphProductionScratch scratch,
    uint ac,uint b0,ReadOnlySpan<byte> requestResidue,CancellationToken token);

//218560 numeric chain lifecycle. Old/global factories privately bind actual
//graph, producer, pool and materialization ownership before entering this core.
internal static class LegacyMainOrdinaryGlyphChainKernel
{
    internal static LegacyPreparedGlyphChainResult Apply(LegacyNativeWordBuilder word,
        ReadOnlySpan<byte> preparedSegmentRows72,ReadOnlySpan<ulong> residualSourceReferences,
        int nodeCount,int recordCount,int[] starts,byte[] compact,Func<int,int> arcCount,
        LegacyMainOrdinaryGlyphRowProduction ordinary,LegacyMainProtocolGlyphRowProduction protocol,
        ushort previousSymbol,uint preparedPacketAcBits,uint preparedPacketB0Bits,bool initialForceDuplicate,
        ReadOnlySpan<byte> allocatedRecordResidue40,ReadOnlySpan<ushort> labelStackResidue256,
        ReadOnlySpan<byte> requestStackResidue64,ulong conversionArgument,CancellationToken token)
    {
        if(preparedSegmentRows72.Length%72!=0||preparedSegmentRows72.Length>4096*72||
            allocatedRecordResidue40.Length!=40||labelStackResidue256.Length!=256||requestStackResidue64.Length!=64)
            throw new ArgumentException("Bounded complete segment and explicit record/label/request storage required.");
        if(!residualSourceReferences.IsEmpty)
            throw new NotSupportedException("Residual217DB0 association must be performed by its genuine source overlay before this executor.");
        var existing=word.CopyChain(token);ulong tail=existing.Length==0?ulong.MaxValue:existing[^1];
        if(existing.Length+preparedSegmentRows72.Length/72>4096)
            throw new NotSupportedException("Completed chain would exceed the WORD owner's portable4096-reference budget.");
        ulong previous=ulong.MaxValue,firstNew=ulong.MaxValue;bool forceCarry=initialForceDuplicate;
        int count=preparedSegmentRows72.Length/72,created=0,reused=0;
        for(int i=count-1;i>=0;i--)
        {
            token.ThrowIfCancellationRequested();var row=preparedSegmentRows72.Slice(i*72,72);
            int node=I32(row,0),index=I32(row,8),neighbor=I32(row,32);ushort arc=U16(row,4);
            if((uint)node>=(uint)nodeCount||arc>=arcCount(node)||
                (uint)index>=(uint)recordCount||index<starts[node]||index>=starts[node+1]||
                neighbor!=-1&&(uint)neighbor>=(uint)recordCount)
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
                    finalized=ordinary(row,scratch,conversionArgument,compact,token);
                else if(row[60]==3)
                    finalized=protocol(row,compact,scratch,preparedPacketAcBits,preparedPacketB0Bits,requestStackResidue64,token);
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
