using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyNativeSecondaryRecombination(ReadOnlyMemory<byte> LeftStorage, int WrittenCount);

/// <summary>
/// Literal nontracing 1f87c0 merge over configured secondary 40-byte blocks.
/// Incoming score production and higher-arc eligibility are separate. The
/// native reverse write reads live left storage and writes only selected fields.
/// </summary>
public static class LegacyNativeSecondaryRecombinationMerger
{
    private readonly record struct Selected(bool Incoming, int Ordinal, ulong MappedContext);

    public static LegacyNativeSecondaryRecombination Merge(ReadOnlySpan<byte> leftStorage,
        ReadOnlySpan<byte> incomingStorage, ushort secondaryCount2404,
        CancellationToken cancellationToken = default, bool tracingEnabled2368 = false)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (tracingEnabled2368)
            throw new NotSupportedException("Native 0x280-byte trace side tables are not supplied.");
        if (secondaryCount2404 is < 2 or > 20 || leftStorage.Length % 40 != 0 || incomingStorage.Length % 40 != 0 ||
            leftStorage.Length < secondaryCount2404 * 40 || incomingStorage.Length < secondaryCount2404 * 40)
            throw new ArgumentException("Complete enabled native secondary blocks and the bounded 20-slot vector required.");
        if (leftStorage.Overlaps(incomingStorage))
            throw new NotSupportedException("Native incoming temporary storage must be distinct from the destination block.");

        var output = leftStorage.ToArray();
        bool incomingFirst = Cost(incomingStorage[..40]) <= Cost(leftStorage[..40]);
        var selected = new List<Selected>(secondaryCount2404)
        {
            new(incomingFirst, 0, Context(incomingFirst ? incomingStorage[..40] : leftStorage[..40]))
        };
        int left = incomingFirst ? 0 : 1, incoming = incomingFirst ? 1 : 0;
        while (selected.Count < secondaryCount2404)
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool takeIncoming;
            if (left < secondaryCount2404 && Cost(leftStorage.Slice(left * 40, 40)) < 0x4000000)
                takeIncoming = incoming < secondaryCount2404 &&
                    Cost(leftStorage.Slice(left * 40, 40)) >= Cost(incomingStorage.Slice(incoming * 40, 40));
            else
            {
                if (incoming >= secondaryCount2404 || Cost(incomingStorage.Slice(incoming * 40, 40)) >= 0x4000000) break;
                takeIncoming = true;
            }
            int index = takeIncoming ? incoming++ : left++;
            ulong identity = Context((takeIncoming ? incomingStorage : leftStorage).Slice(index * 40, 40));
            if (!selected.Any(point => point.MappedContext == identity))
                selected.Add(new(takeIncoming, index, identity));
        }

        // Native source param4 is also the destination. Copy in reverse and
        // read it live, preserving even alias-sensitive duplicate-list effects.
        for (int index = selected.Count - 1; index >= 0; index--)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var point = selected[index];
            ReadOnlySpan<byte> source = point.Incoming ? incomingStorage.Slice(point.Ordinal * 40, 40)
                : output.AsSpan(point.Ordinal * 40, 40);
            var destination = output.AsSpan(index * 40, 40);
            BinaryPrimitives.WriteInt32LittleEndian(destination, Cost(source));
            BinaryPrimitives.WriteInt32LittleEndian(destination[4..], BinaryPrimitives.ReadInt32LittleEndian(source[4..]));
            destination[30] = source[30];
            BinaryPrimitives.WriteUInt32LittleEndian(destination[24..], BinaryPrimitives.ReadUInt32LittleEndian(source[24..]));
            BinaryPrimitives.WriteUInt64LittleEndian(destination[8..], Context(source));
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(output, selected.Count);
    }

    private static int Cost(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadInt32LittleEndian(source);
    private static ulong Context(ReadOnlySpan<byte> source) => BinaryPrimitives.ReadUInt64LittleEndian(source[8..]);
}
