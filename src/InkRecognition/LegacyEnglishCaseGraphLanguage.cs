using System.Buffers.Binary;
namespace InkRecognition;
// Value-only decomposition of204290's current38/3C -> anchor20 path.
// A result is not a query/source issuer; the generic owning stage will retain
// SAME ring, dictionary and context lease while consuming it synchronously.
internal readonly record struct LegacyEnglishCaseLanguageAnchor(int CurrentNode,int CurrentOrdinal,int AnchorNode,int AnchorOrdinal,int PreviousWordNode,ulong AnchorTag)
{
    internal bool IsFirst=>PreviousWordNode==-1;
    internal static LegacyEnglishCaseLanguageAnchor Resolve(LegacyEnglishCaseNode0Ring ring,int node,int ordinal,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(node!=ring.CurrentNode||(uint)ordinal>=(uint)ring.SourceEndAt(node))
            throw new InvalidOperationException("Current owned SourceEnd row required, not retained primary prefix.");
        var current=ring.OwnedRowsAt(node).Slice(checked(ordinal*88),88);var mask=ring.OwnedRowMaskAt(node).Slice(checked(ordinal*88),88);
        Known(mask,16,4);Known(mask,56,6);
        if(BinaryPrimitives.ReadInt32LittleEndian(current[16..])>=0x4000000)throw new InvalidDataException("Finite current language row required.");
        int anchorNode=BinaryPrimitives.ReadInt32LittleEndian(current[56..]);int anchorOrdinal=BinaryPrimitives.ReadUInt16LittleEndian(current[60..]);
        if((uint)anchorNode>(uint)node||(uint)anchorOrdinal>=(uint)ring.SourceEndAt(anchorNode))throw new InvalidDataException("Actual current38/3C anchor outside live SourceEnd.");
        var anchor=ring.OwnedRowsAt(anchorNode).Slice(checked(anchorOrdinal*88),88);var anchorMask=ring.OwnedRowMaskAt(anchorNode).Slice(checked(anchorOrdinal*88),88);
        Known(anchorMask,0,8);Known(anchorMask,16,4);Known(anchorMask,32,1);
        if(BinaryPrimitives.ReadInt32LittleEndian(anchor[16..])>=0x4000000)throw new InvalidDataException("Finite anchor language row required.");
        int previous=checked(anchorNode-(sbyte)anchor[32]-1);
        if(previous< -1||previous>node)throw new NotSupportedException("Actual FIRST or retained previous-word branch required.");
        if(previous>=0)_=ring.SourceEndAt(previous); // retain-live guard; never lookup a public snapshot
        token.ThrowIfCancellationRequested();return new(node,ordinal,anchorNode,anchorOrdinal,previous,BinaryPrimitives.ReadUInt64LittleEndian(anchor));
    }
    private static void Known(ReadOnlySpan<byte> mask,int at,int count)
    {for(int i=at;i<at+count;i++)if(mask[i]!=255)throw new NotSupportedException("Consumed language anchor byte is opaque.");}
}
