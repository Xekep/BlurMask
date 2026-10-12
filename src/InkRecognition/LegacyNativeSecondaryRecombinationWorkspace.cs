using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>
/// The literal 800-byte zeroed temporary at 20e6a0, reused across incoming
/// candidates. Transition acceptance and the two adjusted offsets are explicit.
/// Unavailable source alternatives change cost alone and retain prior residue.
/// </summary>
public sealed class LegacyNativeSecondaryRecombinationWorkspace
{
    private readonly byte[] storage = new byte[800]; // Native 25ee50(local378, 0, 800).
    public ushort SecondaryAlternativeCount { get; }

    public LegacyNativeSecondaryRecombinationWorkspace(ushort secondaryAlternativeCount2404)
    {
        if (secondaryAlternativeCount2404 is < 2 or > 20)
            throw new ArgumentOutOfRangeException(nameof(secondaryAlternativeCount2404));
        SecondaryAlternativeCount = secondaryAlternativeCount2404;
    }

    public void Populate(ReadOnlySpan<byte> actualSourceSecondaryBlock, ushort candidateArc,
        int adjustedCostOffset, int incrementalOffset, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (actualSourceSecondaryBlock.Length != SecondaryAlternativeCount * 40)
            throw new ArgumentException("Exact owning source configured secondary block required.");
        for (int index = 0; index < SecondaryAlternativeCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = actualSourceSecondaryBlock.Slice(index * 40, 40);
            var target = storage.AsSpan(index * 40, 40);
            int cost = BinaryPrimitives.ReadInt32LittleEndian(source);
            if (cost >= 0x4000000)
            {
                BinaryPrimitives.WriteInt32LittleEndian(target, 0x4000000);
                continue; // Literal stale increment/context/metadata preservation.
            }
            BinaryPrimitives.WriteInt32LittleEndian(target, unchecked(adjustedCostOffset + cost));
            BinaryPrimitives.WriteInt32LittleEndian(target[4..], unchecked(incrementalOffset +
                BinaryPrimitives.ReadInt32LittleEndian(source[4..])));
            target[30] = 0;
            BinaryPrimitives.WriteUInt16LittleEndian(target[24..], candidateArc);
            target[27] = (byte)((target[27] & 0x80) | (index & 0x7f));
            BinaryPrimitives.WriteUInt64LittleEndian(target[8..], BinaryPrimitives.ReadUInt64LittleEndian(source[8..]));
        }
        cancellationToken.ThrowIfCancellationRequested();
    }

    public ReadOnlyMemory<byte> GetStorageSnapshot() => storage.ToArray();
}
