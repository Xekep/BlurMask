using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("LegacyMainAugmentedWordCosts.Probe")]
namespace InkRecognition;

/// <summary>The219580 cost transfer after the actual218560 chain succeeds.
/// This calculation alone neither selects a compact candidate nor interns a WORD.</summary>
internal static class LegacyPreparedMainAugmentedWordCosts
{
    internal readonly record struct Result(float Score10,float Cost18,float Cost1C);

    internal static Result Calculate(ReadOnlySpan<byte> compact40,int activeCount,int candidateIndex,
        int lastDirectParentIndex,int firstSourceNode,int lastSourceNode,int sourceKind,
        CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(compact40.Length%40!=0||activeCount<1||activeCount>compact40.Length/40||activeCount>131072)
            throw new ArgumentException("Complete bounded compact40 storage and its actual active extent required.");
        if((uint)candidateIndex>=(uint)activeCount||lastDirectParentIndex < -1||lastDirectParentIndex>=activeCount)
            throw new ArgumentOutOfRangeException(nameof(candidateIndex));
        if(firstSourceNode < -1||lastSourceNode<firstSourceNode||lastSourceNode>4095)
            throw new NotSupportedException("A forward owned main-source node span is required.");
        int primary=Read(compact40,candidateIndex,20),secondary=Read(compact40,candidateIndex,24);
        if(lastDirectParentIndex!=-1)
        {
            primary=unchecked(primary-Read(compact40,lastDirectParentIndex,20));
            secondary=unchecked(secondary-Read(compact40,lastDirectParentIndex,24));
        }
        int denominator=unchecked((lastSourceNode+1-firstSourceNode)<<13);
        float cost18=(float)primary/(float)denominator;
        float score10=-(sourceKind==0x135?5f:1f)*cost18;
        float cost1c=(float)secondary*(1f/8192f);
        return new(score10,cost18,cost1c);
    }
    private static int Read(ReadOnlySpan<byte> rows,int row,int offset)
        =>BinaryPrimitives.ReadInt32LittleEndian(rows.Slice(row*40+offset,4));
}
