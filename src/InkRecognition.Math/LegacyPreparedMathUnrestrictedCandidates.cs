using System.Buffers.Binary;

namespace InkRecognition.Math;

/// <summary>Prepared unrestricted038610 candidate rows through the retained
///0351E0 heap and035F70 ordering. EEF cost and per-beam metadata producers,
///configured aliases, EOS handling and complete beam execution are not joined.</summary>
internal sealed class LegacyPreparedMathUnrestrictedCandidates
{
    private readonly LegacyPreparedMathCandidateHeap heap;
    internal int ActiveBeams {get;}
    internal int BeamWidth {get;}
    internal int VocabularyCount {get;}
    internal byte[] CopyHeapStorage()=>heap.CopyStorage();
    internal byte[] CopySortedStorage()=>LegacyPreparedMathRawCandidateOrdering.Sort(heap.CopyStorage());
    private LegacyPreparedMathUnrestrictedCandidates(LegacyPreparedMathCandidateHeap actualHeap,int active,int width,int vocabulary)
    {heap=actualHeap;ActiveBeams=active;BeamWidth=width;VocabularyCount=vocabulary;}
    internal static LegacyPreparedMathUnrestrictedCandidates Expand(
        ReadOnlySpan<byte> cumulativeCostsF32,int activeBeams,int vocabularyCount,int requestedBeamWidth,
        ReadOnlySpan<byte> preparedSourceInformation64,ReadOnlySpan<byte> preparedConditional32,
        CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(requestedBeamWidth<1||requestedBeamWidth>16||activeBeams<1||activeBeams>requestedBeamWidth||
            vocabularyCount<1||vocabularyCount>65536||activeBeams*vocabularyCount<requestedBeamWidth*2||
            cumulativeCostsF32.Length!=activeBeams*vocabularyCount*4||
            preparedSourceInformation64.Length!=activeBeams*8||preparedConditional32.Length!=activeBeams*4)
            throw new ArgumentException("Complete prepared cost rows and same-sized beam sidecars must fill2*width slots.");
        int capacity=requestedBeamWidth*2;
        //All slots become initialized by actual offers before any caller sort.
        //No unused allocation bytes can reach this supported result.
        var heap=new LegacyPreparedMathCandidateHeap(capacity,new byte[capacity*48]);
        Span<byte> payload=stackalloc byte[44];
        for(int beam=0;beam<activeBeams;beam++)
        {
            token.ThrowIfCancellationRequested();
            for(int symbol=0;symbol<vocabularyCount;symbol++)
            {
                float cost=BinaryPrimitives.ReadSingleLittleEndian(cumulativeCostsF32[((beam*vocabularyCount+symbol)*4)..]);
                int slot=heap.TryOffer(cost,token);if(slot<0)continue;
                payload.Clear();
                BinaryPrimitives.WriteUInt64LittleEndian(payload[4..],(ulong)symbol);
                preparedSourceInformation64.Slice(beam*8,8).CopyTo(payload[12..]);
                preparedConditional32.Slice(beam*4,4).CopyTo(payload[20..]);
                BinaryPrimitives.WriteUInt64LittleEndian(payload[28..],ulong.MaxValue);
                BinaryPrimitives.WriteInt32LittleEndian(payload[36..],beam);
                heap.WritePreparedPayload(slot,payload);
            }
        }
        if(heap.Count!=capacity)throw new InvalidOperationException("Prepared native sort range is not fully initialized.");
        return new(heap,activeBeams,requestedBeamWidth,vocabularyCount);
    }
}
