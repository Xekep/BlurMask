using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyNativeHigherArcTransitionOperations(
    Func<LegacyNativeWordRecombinationRequest, CancellationToken, bool> Calculate118,
    Func<LegacyNativeWordRecombinationRequest, CancellationToken, bool> Validate140,
    Func<LegacyNativeWordRecombinationRequest, CancellationToken, ReadOnlyMemory<ushort>> SuccessorsF0F8)
{
    /// <summary>152140/152190 -> ordinary21b50/58 bridge. Request08 is
    /// previous ending node, request10 previous graph arc and request20 CURRENT
    /// span selector. Required full-packet118/140 implementations stay explicit.</summary>
    public static LegacyNativeHigherArcTransitionOperations ForOrdinaryConnections(
        LegacyOrdinaryCandidateConnections connections,
        Func<LegacyNativeWordRecombinationRequest, CancellationToken, bool> calculate118,
        Func<LegacyNativeWordRecombinationRequest, CancellationToken, bool> validate140)
    {
        ArgumentNullException.ThrowIfNull(connections);
        ArgumentNullException.ThrowIfNull(calculate118);
        ArgumentNullException.ThrowIfNull(validate140);
        return new(calculate118, validate140, (request, token) =>
        {
            int span = request.ReadInt32(32);
            if (span is < 0 or > 14)
                throw new NotSupportedException("Ordinary source successor selectors are bounded0..14.");
            var source = connections.GetSuccessorOrdinals(request.ReadInt32(8), request.ReadUInt16(16), (byte)span, token);
            var output = new ushort[source.Length];
            for (int index = 0; index < output.Length; index++)
            {
                token.ThrowIfCancellationRequested();
                output[index] = checked((ushort)source.Span[index]);
            }
            token.ThrowIfCancellationRequested();
            return output;
        });
    }
}

public readonly record struct LegacyNativeHigherArcRecombinationPolicy(ushort ResetSpecialLabel24ea,
    int LongSpanThreshold54, bool CountCapEnabled2601, byte CountCap2602, byte Permission2309);

public sealed record LegacyNativeHigherArcRecombinationResult(bool InitialAccepted,
    ReadOnlyMemory<byte> HeapSlot, ReadOnlyMemory<byte> PrimaryStorage, ReadOnlyMemory<byte> SecondaryStorage,
    ReadOnlyMemory<byte> LastRequest, int FilteredCandidates, int MergedCandidates);

/// <summary>
/// Nontracing remaining higher-arc 20e6a0 controller. Actual row ordinals,
/// prepared source headers, full request callbacks and runtime policy are
/// required. It never substitutes the narrower first23f callback contract.
/// </summary>
public static class LegacyNativeHigherArcRecombination
{
    public static LegacyNativeHigherArcRecombinationResult Create(ReadOnlySpan<byte> heapSlot, uint currentNode,
        LegacyNativeWordControllerConfiguration configuration, IReadOnlyList<LegacyNativeStoredWordRecord> previousNodeRows,
        LegacyNativeWordParentRowLookup owningRowLookup, Func<byte, uint> preparedSourceHeader,
        LegacyCursiveCategoryProfile categoryProfile, LegacySymbolContextFactory contexts,
        Func<ushort, ushort> mapSymbol2360, LegacyNativeHigherArcRecombinationPolicy policy,
        LegacyNativeWordRecombinationSymbols symbols, LegacyNativeHigherArcTransitionOperations operations,
        ulong costCellIdentity, ReadOnlySpan<byte> stackResidue80, ReadOnlySpan<byte> primaryStorage,
        ReadOnlySpan<byte> secondaryStorage, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(previousNodeRows);
        ArgumentNullException.ThrowIfNull(owningRowLookup);
        ArgumentNullException.ThrowIfNull(preparedSourceHeader);
        ArgumentNullException.ThrowIfNull(categoryProfile);
        ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(mapSymbol2360);
        ArgumentNullException.ThrowIfNull(operations);
        ArgumentNullException.ThrowIfNull(operations.Calculate118);
        ArgumentNullException.ThrowIfNull(operations.Validate140);
        ArgumentNullException.ThrowIfNull(operations.SuccessorsF0F8);
        cancellationToken.ThrowIfCancellationRequested();
        ushort pc = configuration.PrimaryAlternativeCount2402, sc = configuration.SecondaryAlternativeCount2404;
        if (configuration.Trace2368 != 0) throw new NotSupportedException("Native tracing side tables remain separate.");
        if (heapSlot.Length != 128 || pc is < 1 or > 20 || sc is < 2 or > 20 || previousNodeRows.Count > ushort.MaxValue ||
            primaryStorage.Length % 40 != 0 || secondaryStorage.Length % 40 != 0 ||
            pc > 1 && primaryStorage.Length < pc * 40 || secondaryStorage.Length < sc * 40)
            throw new ArgumentException("Actual bounded row collection and configured candidate blocks required.");
        ushort originalArc = U16(heapSlot, 30);
        uint previous = unchecked(currentNode - (uint)(int)(sbyte)heapSlot[32] - 1);
        if (previous > int.MaxValue || originalArc < configuration.PrimaryArcLimit230a || originalArc >= previousNodeRows.Count)
            throw new NotSupportedException("Actual nonroot higher-arc predecessor and complete ordinal row collection required.");
        foreach (var row in previousNodeRows)
            if (row is null || row.Record.Length != 88 || row.SecondaryAlternatives.Length != sc * 40)
                throw new ArgumentException("Actual stored88/configured secondary rows must cover every supplied ordinal.");
        uint currentHeader = preparedSourceHeader(heapSlot[4]);
        cancellationToken.ThrowIfCancellationRequested();
        if (configuration.Mode22f6 == 0 && (currentHeader & 0x1c) > 7 && U16(heapSlot, 36) != policy.ResetSpecialLabel24ea)
            throw new NotSupportedException("Use the separately ported higher-arc reset branch for this source.");

        var slot = heapSlot.ToArray();
        var primary = primaryStorage.ToArray();
        var secondary = secondaryStorage.ToArray();
        var original = previousNodeRows[originalArc];
        var parentRecord = original.Record.Span;
        int originalCost = I32(slot, 16), originalIncrement = I32(slot, 20);
        int physicalCostDelta = unchecked(originalCost - I32(parentRecord, 16));
        int incrementalDelta = unchecked(originalIncrement - I32(parentRecord, 20));
        if (pc > 1)
        {
            var first = primary.AsSpan(0, 40);
            Metadata(first);
            first[27] &= 0x80;
            BinaryPrimitives.WriteUInt64LittleEndian(first[8..],
                LegacyNativeWordContextComposition.CombineMapped(contexts, 0, U64(slot, 8), mapSymbol2360, cancellationToken));
            BinaryPrimitives.WriteUInt64LittleEndian(first[16..],
                LegacyNativeWordContextComposition.CombineRaw(contexts, 0, U64(slot, 8), cancellationToken));
            Write32(first, 0, physicalCostDelta);
            Write32(first, 4, incrementalDelta);
            first[30] = slot[38];
            first[31] |= 0x20;
            for (int index = 1; index < pc; index++)
            {
                var tail = primary.AsSpan(index * 40, 40);
                Write32(tail, 0, 0x4000000);
                Write32(tail, 4, 0x4000000);
                tail[30] = 0;
            }
        }
        var initialSecondary = secondary.AsSpan(0, 40);
        Metadata(initialSecondary);
        initialSecondary[27] &= 0x80;
        Write32(initialSecondary, 0, originalCost);
        Write32(initialSecondary, 4, originalIncrement);
        initialSecondary[30] = slot[38];

        var cell = new LegacyNativeWordTransitionCostCell(costCellIdentity, 0);
        var request = LegacyNativeWordRecombinationRequestFactory.Create(slot, currentNode, parentRecord,
            categoryProfile, owningRowLookup, preparedSourceHeader, contexts, symbols, policy.Permission2309,
            cell, stackResidue80, cancellationToken);
        uint parentHeader = preparedSourceHeader(parentRecord[4]);
        cancellationToken.ThrowIfCancellationRequested();
        uint parentKind = parentHeader & 0x1e00;
        bool bypass118 = U16(parentRecord, 36) == symbols.Symbol24f4 ||
            unchecked((int)previous - I32(parentRecord, 56)) >= policy.LongSpanThreshold54 && parentKind is >= 0x200 and <= 0xa00;
        if ((!bypass118 && !Call(operations.Calculate118)) || !Call(operations.Validate140))
            return new(false, slot, primary, secondary, request.GetSnapshot(), 0, 0);
        int adjustedDelta = unchecked(originalCost - cell.Value);

        // Native resets local428 to the unadjusted physical delta before
        // building tails and before the first successor count/list query.
        cell.Value = physicalCostDelta;

        // The initial branch uses unadjusted physical deltas; the corrected
        // delta is retained for subsequent request callbacks, not recomputed.
        BinaryPrimitives.WriteUInt64LittleEndian(initialSecondary[8..], U64(original.SecondaryAlternatives.Span, 8));
        for (int index = 1; index < sc; index++)
        {
            var source = original.SecondaryAlternatives.Span.Slice(index * 40, 40);
            var target = secondary.AsSpan(index * 40, 40);
            target[30] = 0;
            if (I32(source, 0) < 0x4000000)
            {
                Write32(target, 0, unchecked(physicalCostDelta + I32(source, 0)));
                Write32(target, 4, unchecked(incrementalDelta + I32(source, 4)));
                BinaryPrimitives.WriteUInt16LittleEndian(target[24..], originalArc);
                target[27] = (byte)((target[27] & 0x80) | (index & 0x7f));
                BinaryPrimitives.WriteUInt64LittleEndian(target[8..], U64(source, 8));
            }
            else
            {
                Write32(target, 0, 0x4000000);
                Write32(target, 4, 0x4000000);
            }
        }
        var workspace = new LegacyNativeSecondaryRecombinationWorkspace(sc);
        int filtered = 0, merged = 0, span = (sbyte)slot[32];
        ushort currentArc = U16(slot, 28);
        for (int candidateArc = configuration.PrimaryArcLimit230a; candidateArc < previousNodeRows.Count; candidateArc++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (candidateArc == originalArc) continue;
            var candidate = previousNodeRows[candidateArc];
            var record = candidate.Record.Span;
            if (((record[33] ^ parentRecord[33]) & 0x3f) != 0 || U64(record, 40) != U64(parentRecord, 40) ||
                policy.CountCapEnabled2601 && unchecked(record[38] + span) > policy.CountCap2602) continue;
            BinaryPrimitives.WriteUInt16LittleEndian(request.Storage[16..], U16(record, 28));
            var successors = operations.SuccessorsF0F8(request, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            if (successors.Length > ushort.MaxValue) throw new ArgumentException("Native unsigned16 successor count exceeded.");
            if (!successors.Span.Contains(currentArc)) continue;
            filtered++;
            cell.Value = adjustedDelta;
            LegacyNativeWordRecombinationRequestFactory.UpdatePrevious(request, record, (int)previous,
                categoryProfile, owningRowLookup, preparedSourceHeader, cancellationToken);
            if (!Call(operations.Calculate118) || !Call(operations.Validate140)) continue;
            workspace.Populate(candidate.SecondaryAlternatives.Span, (ushort)candidateArc, cell.Value, incrementalDelta, cancellationToken);
            secondary = LegacyNativeSecondaryRecombinationMerger.Merge(secondary, workspace.GetStorageSnapshot().Span, sc,
                cancellationToken).LeftStorage.ToArray();
            var best = secondary.AsSpan(0, 40);
            Write32(slot, 16, I32(best, 0));
            Write32(slot, 20, I32(best, 4));
            slot[38] = best[30];
            BinaryPrimitives.WriteUInt16LittleEndian(slot.AsSpan(30), U16(best, 24));
            if (pc > 1) primary[27] = (byte)((primary[27] & 0x80) | (best[27] & 0x7f));
            merged++;
        }
        // Refresh occurs once after ALL incoming candidates. Refreshing inside
        // the loop would change mapped IDs used by subsequent native dedup.
        var final = LegacyNativeSecondaryRecombinationProjection.Project(slot, currentNode, configuration,
            primary, secondary, owningRowLookup, contexts, mapSymbol2360, cancellationToken);
        return new(true, final.HeapSlot, final.PrimaryStorage, final.SecondaryStorage, request.GetSnapshot(), filtered, merged);

        bool Call(Func<LegacyNativeWordRecombinationRequest, CancellationToken, bool> callback)
        {
            bool accepted = callback(request, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
            return accepted;
        }
        void Metadata(Span<byte> target)
        {
            BinaryPrimitives.WriteUInt16LittleEndian(target[24..], originalArc);
            target[26] = slot[32];
            target[31] = (byte)((target[31] & 0xe0) | (slot[33] & 31));
        }
    }

    private static int I32(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadInt32LittleEndian(value[offset..]);
    private static ushort U16(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(value[offset..]);
    private static ulong U64(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(value[offset..]);
    private static void Write32(Span<byte> value, int offset, int number) => BinaryPrimitives.WriteInt32LittleEndian(value[offset..], number);
}
