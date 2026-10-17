using System.Buffers.Binary;
namespace InkRecognition;

//Native2188F0 numeric accumulator; factories retain the old/global source owners.
internal static class LegacyMainSourceWidthKernel
{
    internal static LegacyMainSourceWidthStatistics Accumulate(ReadOnlySpan<byte> rows72,ReadOnlySpan<byte> compact40,
        int compactCount,LegacyMainSourceWidthStatistics seed,Func<int,ushort,LegacyNativeResidualBounds> readBounds,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(readBounds);token.ThrowIfCancellationRequested();
        if(rows72.Length%72!=0||rows72.Length/72>65536||compactCount<0||compactCount>compact40.Length/40)
            throw new ArgumentException("Complete bounded segment rows and compact storage required.");
        double sum=seed.WidthSum;int count=seed.Count;
        for(int i=rows72.Length/72-1;i>=0;i--)
        {
            token.ThrowIfCancellationRequested();var row=rows72.Slice(i*72,72);
            int index=BinaryPrimitives.ReadInt32LittleEndian(row[8..]);
            if(index<0)throw new NotSupportedException("Negative compact source index is outside safe native storage.");
            if(index>=compactCount)continue;
            int subtype=compact40[index*40+0x20]&7;
            if(subtype==2)continue;
            ushort marker=BinaryPrimitives.ReadUInt16LittleEndian(row[0x38..]);
            ushort symbol=BinaryPrimitives.ReadUInt16LittleEndian(row[0x3a..]);
            if(subtype is not (4 or 6)&&marker!=ushort.MaxValue&&unchecked((ushort)(symbol+6))<=2)continue;
            var bounds=readBounds(BinaryPrimitives.ReadInt32LittleEndian(row),BinaryPrimitives.ReadUInt16LittleEndian(row[4..]));
            float width=bounds.MaximumX-bounds.MinimumX;
            count=unchecked(count+1);sum+=(double)width;
        }
        return new(sum,count);
    }
}
