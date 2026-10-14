using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Literal203520/203640 bounded16-byte candidate maxheap. Full
/// records move together. Equal-cost candidates do not displace a full root.</summary>
public sealed class LegacyNativeLanguageContextCandidateHeap
{
    private readonly byte[] storage;
    public int Capacity { get; }
    public int Count { get; private set; }
    public LegacyNativeLanguageContextCandidateHeap(int capacity)
    {
        if (capacity is < 0 or > ushort.MaxValue) throw new ArgumentOutOfRangeException(nameof(capacity));
        Capacity = capacity;
        storage = new byte[capacity * 16]; //Inactive bytes are never read.
    }
    public ReadOnlyMemory<byte> GetSnapshot() => storage.AsSpan(0, Count * 16).ToArray();

    public bool TryInsert(int cost, int increment, ushort previousOrdinal, int geometryAdjustment,
        ushort currentOrdinal, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Capacity == 0 || cost > 0x3ffffff) return false;
        int hole;
        if (Count == Capacity)
        {
            if (Cost(0) <= cost) return false;
            hole = 0;
            if (Count != 1)
            {
                do
                {
                    int child = hole * 2 + 1;
                    if (child < Count - 1 && Cost(child) <= Cost(child + 1)) child++;
                    if (Cost(child) <= cost) break;
                    storage.AsSpan(child * 16, 16).CopyTo(storage.AsSpan(hole * 16, 16));
                    hole = child;
                } while (hole * 2 < Count - 1);
            }
        }
        else
        {
            hole = Count++;
            while (hole != 0)
            {
                int parent = (hole - 1) >> 1;
                if (cost <= Cost(parent)) break;
                storage.AsSpan(parent * 16, 16).CopyTo(storage.AsSpan(hole * 16, 16));
                hole = parent;
            }
        }
        var output = storage.AsSpan(hole * 16, 16);
        output.Clear(); //203520 explicitly writes two zero qwords here.
        BinaryPrimitives.WriteInt32LittleEndian(output, cost);
        BinaryPrimitives.WriteInt32LittleEndian(output[4..], increment);
        BinaryPrimitives.WriteInt32LittleEndian(output[8..], geometryAdjustment);
        BinaryPrimitives.WriteUInt16LittleEndian(output[12..], previousOrdinal);
        BinaryPrimitives.WriteUInt16LittleEndian(output[14..], currentOrdinal);
        return true;
    }
    private int Cost(int ordinal) => BinaryPrimitives.ReadInt32LittleEndian(storage.AsSpan(ordinal * 16));
}
