using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyNativeContinuationWordAlternatives(bool Succeeded,
    ReadOnlyMemory<byte> SecondaryStorage);

/// <summary>Owned stored88-byte row plus its actual alternatives, addressed by
/// the native211eb0 node/arc lookup. Returned records are copied before use.</summary>
public delegate LegacyNativeStoredWordRecord LegacyNativeWordParentRowLookup(int node, ushort arc);

/// <summary>Literal unanchored(slot3e==FFFF) nontracing211eb0 branch. The
/// rejection prepass is an explicit callback when its native predicate applies.
/// Anchored reconstruction, scheduling and winner/confidence remain separate.</summary>
public static class LegacyNativeContinuationWordAlternativeProducer
{
    public static bool RequiresRejectionPrepass(LegacyNativeWordControllerConfiguration configuration,
        uint heapIndex, int sourceIndex30, uint sourceDescriptorHeader)
        => heapIndex == 0 ? (configuration.PackedSource22e8 & 0x10) != 0
            : sourceIndex30 >= 0 && (sourceDescriptorHeader & 0x20) != 0;

    public static LegacyNativeContinuationWordAlternatives CreateUnanchored(
        ReadOnlySpan<byte> heapStorage, int activeCount, int parentRowNode,
        uint heapIndex, int sourceIndex30, uint sourceDescriptorHeader,
        LegacyNativeWordControllerConfiguration configuration,
        LegacyNativeWordParentRowLookup parentRowLookup, LegacySymbolContextFactory contexts,
        Func<ushort, ushort> mapSymbol2360, ReadOnlySpan<byte> secondaryStorage,
        Func<bool>? runRejectionPrepass = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parentRowLookup);
        ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(mapSymbol2360);
        cancellationToken.ThrowIfCancellationRequested();
        if (configuration.Trace2368 != 0)
            throw new NotSupportedException("Trace640-byte clone branch is not covered.");
        ushort count = configuration.SecondaryAlternativeCount2404;
        if (count is < 1 or > 16)
            throw new NotSupportedException("Checked alternative counts are 1..16.");
        if (heapStorage.Length % 128 != 0 || heapStorage.Length / 128 > 4096 ||
            (uint)activeCount > (uint)(heapStorage.Length / 128) || parentRowNode < 0)
            throw new ArgumentException("Complete bounded heap and explicit nonnegative parent row node required.");
        int rowBytes = count * 40;
        if (secondaryStorage.Length % rowBytes != 0)
            throw new ArgumentException("Complete secondary alternative rows required.");
        bool prepass = RequiresRejectionPrepass(configuration, heapIndex, sourceIndex30, sourceDescriptorHeader);
        if (prepass && runRejectionPrepass is null)
            throw new NotSupportedException("Required210870/211210/2113f0 rejection prepass must be supplied.");
        if (count > 1)
        {
            for (int index = 0; index < activeCount; index++)
            {
                var slot = heapStorage.Slice(index * 128, 128);
                if (BinaryPrimitives.ReadUInt16LittleEndian(slot[0x3e..]) != ushort.MaxValue)
                    throw new NotSupportedException("Anchored secondary reconstruction remains separate.");
                int ordinal = BinaryPrimitives.ReadInt32LittleEndian(slot[0x58..]);
                if (ordinal < 0 || ordinal >= secondaryStorage.Length / rowBytes)
                    throw new ArgumentException("Exact ordinal58 must address the supplied secondary pool.");
                ulong suffix = BinaryPrimitives.ReadUInt64LittleEndian(slot[8..]);
                if (suffix != 0) _ = contexts.ReadNode(suffix);
            }
        }
        var storage = secondaryStorage.ToArray();
        if (prepass)
        {
            bool succeeded = runRejectionPrepass!();
            cancellationToken.ThrowIfCancellationRequested();
            if (!succeeded)
                return new(false, storage);
        }
        if (count == 1)
            return new(true, storage);

        for (int index = 0; index < activeCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var slot = heapStorage.Slice(index * 128, 128);
            ushort parentArc = BinaryPrimitives.ReadUInt16LittleEndian(slot[0x1e..]);
            var parent = parentRowLookup(parentRowNode, parentArc)
                ?? throw new ArgumentException("Parent lookup returned no owned stored row.");
            if (parent.Record.Length != 0x58 || parent.SecondaryAlternatives.Length != rowBytes)
                throw new ArgumentException("Parent requires its exact stored88-byte row and configured secondary alternatives.");
            // Preserve live per-slot lookup order, but own this record's bytes.
            byte[] parentRecord = parent.Record.ToArray();
            byte[] alternatives = parent.SecondaryAlternatives.ToArray();
            int ordinal = BinaryPrimitives.ReadInt32LittleEndian(slot[0x58..]);
            var destination = storage.AsSpan(ordinal * rowBytes, rowBytes);
            ulong suffix = BinaryPrimitives.ReadUInt64LittleEndian(slot[8..]);
            for (int alternativeIndex = 0; alternativeIndex < count; alternativeIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var source = alternatives.AsSpan(alternativeIndex * 40, 40);
                if (BinaryPrimitives.ReadInt32LittleEndian(source) > 0x3ffffff)
                {
                    // Only cost00 is invalidated; increment and all other bytes
                    // keep their explicit previous pool contents.
                    for (int tail = alternativeIndex; tail < count; tail++)
                        BinaryPrimitives.WriteInt32LittleEndian(destination[(tail * 40)..], 0x4000000);
                    break;
                }
                var output = destination.Slice(alternativeIndex * 40, 40);
                int cost = unchecked(BinaryPrimitives.ReadInt32LittleEndian(slot[0x10..]) -
                    BinaryPrimitives.ReadInt32LittleEndian(parentRecord.AsSpan(0x10)) +
                    BinaryPrimitives.ReadInt32LittleEndian(source));
                int increment = unchecked(BinaryPrimitives.ReadInt32LittleEndian(source[4..]) -
                    BinaryPrimitives.ReadInt32LittleEndian(parentRecord.AsSpan(0x14)) +
                    BinaryPrimitives.ReadInt32LittleEndian(slot[0x14..]));
                BinaryPrimitives.WriteInt32LittleEndian(output, cost);
                BinaryPrimitives.WriteInt32LittleEndian(output[4..], increment);
                BinaryPrimitives.WriteUInt16LittleEndian(output[0x18..], parentArc);
                output[0x1a] = 0xff;
                output[0x1b] = (byte)(alternativeIndex & 0x7f);
                ulong mapped = BinaryPrimitives.ReadUInt64LittleEndian(source[8..]);
                ulong raw = BinaryPrimitives.ReadUInt64LittleEndian(source[0x10..]);
                if (suffix != 0)
                {
                    mapped = LegacyNativeWordContextComposition.CombineMapped(contexts, mapped, suffix,
                        mapSymbol2360, cancellationToken);
                    raw = LegacyNativeWordContextComposition.CombineRaw(contexts, raw, suffix, cancellationToken);
                }
                BinaryPrimitives.WriteUInt64LittleEndian(output[8..], mapped);
                BinaryPrimitives.WriteUInt64LittleEndian(output[0x10..], raw);
                BinaryPrimitives.WriteUInt16LittleEndian(output[0x1c..], ushort.MaxValue);
                output[0x1e] = 0;
                output[0x1f] = (byte)((output[0x1f] & 0xe0) | (slot[0x21] & 0x1f));
                BinaryPrimitives.WriteUInt16LittleEndian(output[0x20..], ushort.MaxValue);
                output[0x22] = 0xff;
            }
        }
        return new(true, storage);
    }
}
