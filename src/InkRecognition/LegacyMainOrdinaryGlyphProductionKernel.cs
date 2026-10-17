using System.Buffers.Binary;
namespace InkRecognition;

//Bindings are created only by the actual old/global owning factories. These
//delegates do not grant a public prepared caller access to replacement costs.
internal sealed record LegacyMainOrdinaryGlyphProductionOperations(
    int AlphabetCount,ushort UnitScore,ushort Apostrophe,ushort Quote,
    Func<int,ushort,CancellationToken,int> ReadGroup,
    Func<ushort,ulong,int,ushort,CancellationToken,LegacyRussianMainReplacementResolution> ResolveSymbol,
    Func<int,CancellationToken,LegacyNativeGlyphScoreCache> ReadCache,
    Func<int,ushort,ushort,bool,byte[]> CreateRequest);

internal static class LegacyMainOrdinaryGlyphProductionKernel
{
    internal static LegacyPreparedGlyphFinalizationInput Produce(LegacyMainOrdinaryGlyphProductionOperations operations,ReadOnlySpan<byte> preparedRow72,
        LegacyOrdinaryGlyphProductionScratch scratch,ulong conversionArgument=0,
        ReadOnlySpan<byte> preparedCompact40=default,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(scratch);token.ThrowIfCancellationRequested();
        if(preparedRow72.Length!=72)throw new ArgumentException("Complete prepared backtrack row72 required.");
        if(preparedRow72[0x3c]!=0)throw new NotSupportedException("Only ordinary subtype0 is restored.");
        int node=BinaryPrimitives.ReadInt32LittleEndian(preparedRow72);ushort arc=U16(preparedRow72,4);
        int group=operations.ReadGroup(node,arc,token);
        int compact=BinaryPrimitives.ReadInt32LittleEndian(preparedRow72[0x20..]);
        //21709C..2170BC resolves this neighbor before216860, even though
        //the bounded no-wide resolver never consumes its value. Keep the
        //native read in bounds instead of silently accepting an invalid row.
        if(compact!=-1&&(preparedCompact40.Length%40!=0||(uint)compact>=(uint)(preparedCompact40.Length/40)))
            throw new ArgumentException("Prepared row requires its actual in-bounds compact40 neighbor.");
        if(compact!=-1)_=U16(preparedCompact40,compact*40+0x1c);
        ushort symbol=U16(preparedRow72,0x3a);
        if(unchecked((ushort)(symbol+6))<3)
            throw new NotSupportedException("FFFA/FFFB/FFFC require the native delimiter/multi-output callback.");
        if(symbol>=operations.AlphabetCount+3&&unchecked((ushort)(symbol+9))>2)
            throw new NotSupportedException("This portable ordinary scope excludes other unchecked synthetic labels.");
        scratch.ComponentFlag=0;scratch.ExternalVariant=0;
        scratch.RemainingCursor=unchecked(scratch.RemainingCursor-1);
        token.ThrowIfCancellationRequested();
        var resolution=operations.ResolveSymbol(symbol,conversionArgument,node,arc,token);
        scratch.ExternalVariant=resolution.ExternalVariant;
        ushort emitted=resolution.EmittedSymbol;
        scratch.Write(emitted);
        token.ThrowIfCancellationRequested();
        if(symbol==operations.UnitScore||unchecked((ushort)(symbol+9))<=5)
        {scratch.Score0CBits=0x3f800000;scratch.Score10Bits=0x3f800000;scratch.Cost14=0;}
        else
        {
            scratch.Score0CBits=BitConverter.SingleToUInt32Bits(preparedRow72[0x3e]/255f);
            ushort scoreSymbol=symbol==operations.AlphabetCount+1?operations.Apostrophe:
                symbol==operations.AlphabetCount+2?operations.Quote:symbol;
            var cache=operations.ReadCache(group,token);
            scratch.Score10Bits=BitConverter.SingleToUInt32Bits(cache.ReadSymbolScore(scoreSymbol,0,token));
            var request=operations.CreateRequest(node,arc,scoreSymbol,preparedRow72[0x3d]!=0);
            scratch.Cost14=cache.ReadRequestCost(request,token);
        }
        token.ThrowIfCancellationRequested();scratch.PublicationFlags06=0;
        return scratch.Snapshot();
    }

    private static ushort U16(ReadOnlySpan<byte> bytes,int at)=>BinaryPrimitives.ReadUInt16LittleEndian(bytes[at..]);
}
