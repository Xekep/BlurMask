using System.Buffers.Binary;
namespace InkRecognition;
// Definedness only. This helper never writes/promotes bytes or grants ownership.
internal static class LegacyEnglishCaseNode1ParentMasks
{
    internal static bool Require(ReadOnlySpan<byte> row,ReadOnlySpan<byte> rowMask,ReadOnlySpan<byte> alternatives,ReadOnlySpan<byte> alternativeMasks,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(row.Length!=88||rowMask.Length!=88||alternatives.Length!=80||alternativeMasks.Length!=80)
            throw new InvalidDataException("Exact parent88/primary80 values and bit masks required.");
        Known(rowMask,16,4); // Cost classification itself consumes these bytes.
        if(BinaryPrimitives.ReadInt32LittleEndian(row[16..])>=0x4000000)return false;
        // Exact downstream union: query producer,201270,parent request,
        // primary-parent recombination and conditional anchor/history tail.
        Known(rowMask,0,34);Known(rowMask,36,3);Known(rowMask,40,22);
        for(int index=0;index<2;index++)
        {
            token.ThrowIfCancellationRequested();int at=index*40;Known(alternativeMasks,at,4);
            if(BinaryPrimitives.ReadInt32LittleEndian(alternatives[at..])<0x4000000)
                Known(alternativeMasks,at+4,20);
        }
        return true;
    }
    private static void Known(ReadOnlySpan<byte> mask,int start,int count)
    {for(int i=start;i<start+count;i++)if(mask[i]!=255)throw new InvalidDataException("Consumed parent field has undefined bits.");}
}
