using System.Buffers.Binary;
namespace InkRecognition;

//Literal207B00 numeric56-byte max heap, not a source issuer. Slot0 is the
//native cost sentinel; the caller fills an admitted slot before another call.
internal sealed class LegacyNativePublicationCandidateHeap
{
    private readonly byte[] storage;
    internal int Count {get;private set;}
    internal int Capacity=>storage.Length/56-1;
    internal int RetainedPayloadBytes=>storage.Length;
    internal LegacyNativePublicationCandidateHeap(ushort actualAlternativeCount,ReadOnlySpan<byte> allocatorResidue,
        int maximumRetainedPayloadBytes,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        //207CB0: ((2402+2402*4)*2)+1 slots, with one sentinel.
        int slots=checked(actualAlternativeCount*10+1),bytes=checked(slots*56);
        if(actualAlternativeCount<2||allocatorResidue.Length!=bytes||maximumRetainedPayloadBytes<bytes)
            throw new ArgumentException("Actual207CB0 alternative count and complete bounded56-byte allocator residue required.");
        storage=allocatorResidue.ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(storage.AsSpan(8),0x4000000);
    }
    private Span<byte> Slot(int index)=>storage.AsSpan(index*56,56);
    private int Cost(int index)=>BinaryPrimitives.ReadInt32LittleEndian(Slot(index)[8..]);
    //207CB0 resets local8A0 for each primary ordinal, reusing the same
    //allocation and its native moved-row/untouched field residue.
    internal void ResetOrdinal(CancellationToken token){token.ThrowIfCancellationRequested();Count=0;}
    //Returns0 on native rejection. No count change, fallback repair or tie
    //replacement occurs for >=IMP or cost>=the full heap's current maximum.
    internal int TryAllocate(int cost,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(cost>=0x4000000)return 0;
        int count=Count;
        if(count==Capacity)
        {
            if(Cost(1)<=cost)return 0;
            Span<byte> last=stackalloc byte[56];Slot(count).CopyTo(last);
            int lastCost=BinaryPrimitives.ReadInt32LittleEndian(last[8..]);
            count--;int hole=1;
            while(hole*2<=count)
            {
                int child=hole*2;
                if(child!=count&&Cost(child)<Cost(child+1))child++;
                if(Cost(child)<=lastCost)break;
                Slot(child).CopyTo(Slot(hole));hole=child;
            }
            last.CopyTo(Slot(hole));
        }
        int inserted=++count;Count=count;
        while(Cost(inserted>>1)<cost)
        {Slot(inserted>>1).CopyTo(Slot(inserted));inserted>>=1;}
        //Only this DWORD is written by207B00. The caller's subsequent writes
        //must preserve other allocator/moved-row bytes exactly as native.
        BinaryPrimitives.WriteInt32LittleEndian(Slot(inserted)[8..],cost);return inserted;
    }
    internal Span<byte> AdmittedRow(int index)
    {
        if(index<1||index>Count)throw new ArgumentOutOfRangeException(nameof(index));
        return Slot(index);
    }
}
