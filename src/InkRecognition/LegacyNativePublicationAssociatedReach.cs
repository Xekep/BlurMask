using System.Buffers.Binary;
namespace InkRecognition;

//Numeric1FD190 predecessor walk only; no source lease or tag authority.
internal static class LegacyNativePublicationAssociatedReach
{
    internal static bool HasAssociatedPrimary(IReadOnlyList<LegacyMainDirectWordNodeResult> rows,int node,
        ushort ordinal,ushort primaryLimit,int maximumVisits,CancellationToken token)
    {
        if(maximumVisits<1)throw new ArgumentOutOfRangeException(nameof(maximumVisits));
        var initial=Row(node,ordinal);int cutoff=BinaryPrimitives.ReadInt32LittleEndian(initial.Span[0x38..]);
        for(int visits=0;node>=0&&cutoff<=node;visits++)
        {
            token.ThrowIfCancellationRequested();
            if(visits>=maximumVisits)throw new NotSupportedException("Explicit1FD190 structural visit budget exhausted.");
            var row=Row(node,ordinal).Span;
            if(ordinal<primaryLimit&&(row[7]&1)!=0)return true;
            ordinal=BinaryPrimitives.ReadUInt16LittleEndian(row[0x1e..]);
            node=unchecked(node-1-(sbyte)row[0x20]);
        }
        return false;
        ReadOnlyMemory<byte> Row(int at,ushort index)
        {
            if((uint)at>=(uint)rows.Count||rows[at].Records.Length%88!=0||index>=rows[at].Records.Length/88)
                throw new ArgumentException("Actual bounded88 predecessor row required.");
            return rows[at].Records.Slice(index*88,88);
        }
    }
}
