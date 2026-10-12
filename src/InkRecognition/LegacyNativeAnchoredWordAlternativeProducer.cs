using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Nontracing anchored(slot3e!=FFFF)211eb0 branch using actual
/// stored-row anchor ownership and reference-pair context reconstruction.</summary>
public static class LegacyNativeAnchoredWordAlternativeProducer
{
    public static LegacyNativeContinuationWordAlternatives Create(ReadOnlySpan<byte> heapStorage,
        int activeCount, int parentRowNode, uint heapIndex, int sourceIndex30, uint sourceDescriptorHeader,
        LegacyNativeWordControllerConfiguration configuration, LegacyNativeWordParentRowLookup parentRowLookup,
        LegacySymbolContextFactory contexts, Func<ushort, ushort> mapSymbol2360,
        ReadOnlySpan<byte> secondaryStorage, Func<bool>? runRejectionPrepass = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parentRowLookup);
        ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(mapSymbol2360);
        cancellationToken.ThrowIfCancellationRequested();
        if (configuration.Trace2368 != 0)
            throw new NotSupportedException("Trace640-byte copying remains separate.");
        ushort count = configuration.SecondaryAlternativeCount2404;
        if (count is < 1 or > 16 || heapStorage.Length % 128 != 0 || heapStorage.Length / 128 > 4096 ||
            (uint)activeCount > (uint)(heapStorage.Length / 128) || parentRowNode < 0 ||
            secondaryStorage.Length % (count * 40) != 0)
            throw new ArgumentException("Complete bounded heap/secondary storage and explicit parent node required.");
        bool prepass = LegacyNativeContinuationWordAlternativeProducer.RequiresRejectionPrepass(configuration,
            heapIndex, sourceIndex30, sourceDescriptorHeader);
        if (prepass && runRejectionPrepass is null)
            throw new NotSupportedException("Required native rejection prepass must be supplied.");
        if (count > 1)
            for (int index = 0; index < activeCount; index++)
            {
                var slot = heapStorage.Slice(index * 128, 128);
                int ordinal = BinaryPrimitives.ReadInt32LittleEndian(slot[0x58..]);
                if (BinaryPrimitives.ReadUInt16LittleEndian(slot[0x3e..]) == ushort.MaxValue)
                    throw new NotSupportedException("Unanchored continuation has a separate producer.");
                if (ordinal < 0 || ordinal >= secondaryStorage.Length / (count * 40))
                    throw new ArgumentException("Actual ordinal58 must address the supplied secondary pool.");
            }
        var storage = secondaryStorage.ToArray();
        if (prepass)
        {
            bool succeeded = runRejectionPrepass!();
            cancellationToken.ThrowIfCancellationRequested();
            if (!succeeded) return new(false, storage);
        }
        if (count == 1) return new(true, storage);

        for (int index = 0; index < activeCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var slot = heapStorage.Slice(index * 128, 128);
            ushort parentArc = BinaryPrimitives.ReadUInt16LittleEndian(slot[0x1e..]);
            ushort replacementArc = BinaryPrimitives.ReadUInt16LittleEndian(slot[0x3e..]);
            var parent = Read(parentRowNode, parentArc);
            int anchorNode = I32(parent.Record.Span, 0x38);
            ushort anchorArc = BinaryPrimitives.ReadUInt16LittleEndian(parent.Record.Span[0x3c..]);
            var anchor = Read(anchorNode, anchorArc);
            long ancestorValue = (long)anchorNode - (sbyte)anchor.Record.Span[0x20] - 1;
            if (ancestorValue is < 0 or > int.MaxValue)
                throw new NotSupportedException("Actual anchor span must address a nonnegative stored predecessor.");
            int ancestor = (int)ancestorValue;
            ushort referenceArc = BinaryPrimitives.ReadUInt16LittleEndian(anchor.Record.Span[0x1e..]);
            var reference = Read(ancestor, referenceArc);
            ulong currentContext = U64(parent.SecondaryAlternatives.Span, 8);
            ulong referenceContext = U64(reference.SecondaryAlternatives.Span, 8);
            var retained = LegacyNativeAnchoredContextProjection.Extract(contexts, currentContext,
                referenceContext, cancellationToken);
            var replacement = Read(ancestor, replacementArc);
            int ordinal = I32(slot, 0x58);
            var destination = storage.AsSpan(ordinal * count * 40, count * 40);
            ulong suffix = U64(slot, 8);
            ulong parentRaw = U64(parent.SecondaryAlternatives.Span, 0x10);
            int delta = unchecked(I32(slot, 0x10) - I32(replacement.Record.Span, 0x10));
            int incrementDelta = unchecked(I32(slot, 0x14) - I32(replacement.Record.Span, 0x14));
            for (int k = 0; k < count; k++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var source = replacement.SecondaryAlternatives.Span.Slice(k * 40, 40);
                if (I32(source, 0) > 0x3ffffff)
                {
                    for (int tail = k; tail < count; tail++)
                        BinaryPrimitives.WriteInt32LittleEndian(destination[(tail * 40)..], 0x4000000);
                    break;
                }
                var output = destination.Slice(k * 40, 40);
                BinaryPrimitives.WriteInt32LittleEndian(output, unchecked(I32(source, 0) + delta));
                BinaryPrimitives.WriteInt32LittleEndian(output[4..], unchecked(I32(source, 4) + incrementDelta));
                BinaryPrimitives.WriteUInt16LittleEndian(output[0x18..], parentArc);
                BinaryPrimitives.WriteUInt16LittleEndian(output[0x1a..], 0x00ff);
                ulong raw = suffix == 0 ? parentRaw : LegacyNativeWordContextComposition.CombineRaw(contexts,
                    parentRaw, suffix, cancellationToken);
                BinaryPrimitives.WriteUInt64LittleEndian(output[0x10..], raw);
                BinaryPrimitives.WriteUInt16LittleEndian(output[0x1c..], ushort.MaxValue);
                output[0x1e] = 0;
                output[0x1f] = (byte)((output[0x1f] & 0xe0) | (slot[0x21] & 0x1f));
                BinaryPrimitives.WriteUInt16LittleEndian(output[0x20..], replacementArc);
                output[0x22] = (byte)k;
                ulong mapped = LegacyNativeAnchoredContextProjection.Replay(contexts, U64(source, 8),
                    retained.KeysInParentOrder.Span, cancellationToken);
                if (suffix != 0)
                    mapped = LegacyNativeWordContextComposition.CombineMapped(contexts, mapped, suffix,
                        mapSymbol2360, cancellationToken);
                BinaryPrimitives.WriteUInt64LittleEndian(output[8..], mapped);
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(true, storage);

        LegacyNativeStoredWordRecord Read(int node, ushort arc)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (node < 0) throw new NotSupportedException("Native stored-row owner node must be nonnegative.");
            var row = parentRowLookup(node, arc)
                ?? throw new ArgumentException("Actual owned stored row is required.");
            cancellationToken.ThrowIfCancellationRequested();
            if (row.Record.Length != 0x58 || row.SecondaryAlternatives.Length != count * 40)
                throw new ArgumentException("Stored88 record and configured secondary alternatives are required.");
            // Take an owned snapshot at the actual callback invocation.
            return new LegacyNativeStoredWordRecord(row.Record.ToArray(),
                row.PrimaryAlternatives.ToArray(), row.SecondaryAlternatives.ToArray());
        }
    }

    private static int I32(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadInt32LittleEndian(value[offset..]);
    private static ulong U64(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(value[offset..]);
}
