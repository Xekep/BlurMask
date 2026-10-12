using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyNativeWordRejectionCollection(ReadOnlyMemory<byte> CandidateStorage, int Count);

/// <summary>
/// Checked, allocation-success 210870 ancestry collector over actual stored
/// rows and full 128-byte heap records. Source descriptor headers, runtime
/// marker mode and stack residue are explicit; no ancestry IDs are invented.
/// </summary>
public static class LegacyNativeWordRejectionCollector
{
    private const int Impossible = 0x4000000;
    private readonly record struct Stop(int ParentNode, int Node, ushort Arc, bool Protected, bool Repair);

    /// <summary>
    /// Producer-to-consumer composition on the same live row owner. It returns
    /// the selected candidate storage after literal sorting/marking, with no
    /// winner selection or inferred callback/header policy.
    /// </summary>
    public static LegacyNativeWordRejectionCollection CollectAndStitch(ReadOnlySpan<byte> heapStorage, int heapCount,
        int currentNode, ushort primaryArcLimit230a, LegacyNativePrimaryWordRowOwner owner,
        LegacySymbolContextFactory contexts, Func<byte, uint> sourceHeader22e0,
        bool extendedMarkerMode22f8, Func<int, ReadOnlyMemory<byte>> appendStackResidue,
        Func<ushort, ushort> mapSymbol2360, LegacyNativeWordParentRowLookup? higherArcRowLookup = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(mapSymbol2360);
        var collected = Collect(heapStorage, heapCount, currentNode, primaryArcLimit230a, owner, contexts,
            sourceHeader22e0, extendedMarkerMode22f8, appendStackResidue, higherArcRowLookup, cancellationToken);
        var selected = LegacyNativeRejectionCandidateSelection.Prepare(collected.CandidateStorage.Span,
            collected.Count, owner.PrimaryAlternativeCount, cancellationToken);
        LegacyNativePrimaryRejectionPathStitcher.Apply(selected, collected.Count, primaryArcLimit230a,
            owner, contexts, mapSymbol2360, cancellationToken, higherArcRowLookup: higherArcRowLookup);
        return new(selected, collected.Count);
    }

    public static LegacyNativeWordRejectionCollection Collect(ReadOnlySpan<byte> heapStorage, int heapCount,
        int currentNode, ushort primaryArcLimit230a, LegacyNativePrimaryWordRowOwner owner,
        LegacySymbolContextFactory contexts, Func<byte, uint> sourceHeader22e0,
        bool extendedMarkerMode22f8, Func<int, ReadOnlyMemory<byte>> appendStackResidue,
        LegacyNativeWordParentRowLookup? higherArcRowLookup = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(sourceHeader22e0);
        ArgumentNullException.ThrowIfNull(appendStackResidue);
        cancellationToken.ThrowIfCancellationRequested();
        if (currentNode < 0 || heapStorage.Length % 128 != 0 || heapStorage.Length / 128 > 65536 ||
            (uint)heapCount > (uint)(heapStorage.Length / 128))
            throw new ArgumentException("Bounded complete native heap storage and a nonnegative actual node required.");

        int primaryCount = owner.PrimaryAlternativeCount;
        var candidates = new List<byte[]>();
        for (ushort currentArc = 0; currentArc < primaryArcLimit230a; currentArc++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var currentRecord = Record(currentNode, currentArc);
            if (I32(currentRecord, 16) >= Impossible) break;
            uint header = sourceHeader22e0(currentRecord[4]);
            cancellationToken.ThrowIfCancellationRequested();
            if ((header & 0x20) == 0) continue;

            // Native temporary ushort vector preserves encounters and duplicates.
            var replacements = new List<ushort>();
            for (int index = 0; index < heapCount; index++)
            {
                var slot = heapStorage.Slice(index * 128, 128);
                ushort replacement = U16(slot, 62);
                if (U16(slot, 30) == currentArc && replacement != ushort.MaxValue)
                    replacements.Add(replacement);
            }
            if (primaryCount <= 1) continue;

            var stops = new Stop[primaryCount];
            for (int index = 0; index < primaryCount; index++)
            {
                stops[index] = new(-1, -1, ushort.MaxValue, false, false);
                if (Cost(owner.Alternative(currentNode, currentArc, index)) >= Impossible) continue;
                int node = currentNode, sourceNode = currentNode;
                ushort arc = currentArc, sourceArc = currentArc;
                int alternativeIndex = index, sourceAlternative = index;
                bool boundary = false, protect = false;
                var visited = new HashSet<(int, ushort, int)>();
                while (node >= 1 && !boundary && !protect)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (visited.Count == 4096 || !visited.Add((node, arc, alternativeIndex)))
                        throw new NotSupportedException("Bounded native rejection ancestry is cyclic or exceeds 4096 points.");
                    sourceNode = node;
                    sourceArc = arc;
                    sourceAlternative = alternativeIndex;
                    var source = owner.Alternative(node, arc, alternativeIndex);
                    alternativeIndex = source[27] & 0x7f;
                    arc = U16(source, 24);
                    node = PreviousNode(node, source[26]);
                    protect = (source[31] & 0x40) != 0;
                    boundary = true;
                    if (node >= 0 && arc < primaryArcLimit230a)
                    {
                        var previous = owner.Alternative(node, arc, alternativeIndex);
                        ulong raw = U64(previous, 16);
                        bool marker = false;
                        if (raw != 0)
                        {
                            ushort symbol = contexts.ReadNode(raw).Symbol;
                            marker = symbol is 0xfffb or 0xfff9 ||
                                (extendedMarkerMode22f8 && unchecked((ushort)(symbol + 0x15)) <= 8);
                        }
                        boundary = (previous[31] & 0x40) != 0 || marker;
                    }
                }
                var stopAlternative = owner.Alternative(sourceNode, sourceArc, sourceAlternative);
                int parent = PreviousNode(sourceNode, stopAlternative[26]);
                bool repair = protect;
                if (!protect && parent >= 0 && U16(stopAlternative, 24) != ushort.MaxValue)
                    repair = (Record(parent, U16(stopAlternative, 24))[5] & 4) != 0;
                stops[index] = new(parent, sourceNode, sourceArc, protect, repair);
            }

            if (replacements.Count == 0)
            {
                for (int index = 0; index < primaryCount; index++)
                {
                    var stop = stops[index];
                    if (stop.Repair)
                        AppendFallback(stop, currentArc, index);
                }
                continue;
            }

            foreach (ushort replacement in replacements)
                for (int index = 0; index < primaryCount; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var stop = stops[index];
                    if (stop.ParentNode < 0 || (!stop.Repair && !stop.Protected)) continue;

                    // Literal native indexing is original `index`, not the
                    // alternative ordinal encountered at the end of tracing.
                    var stopAlternative = owner.Alternative(stop.Node, stop.Arc, index);
                    ulong raw = U64(stopAlternative, 16);
                    if (raw == 0 || contexts.ReadNode(raw).ParentLink == 0)
                        BinaryPrimitives.WriteInt32LittleEndian(stopAlternative, Impossible);
                    for (int target = 0; target < primaryCount; target++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        ushort previousArc = stop.Protected ? U16(stopAlternative, 24)
                            : U16(Record(stop.ParentNode, replacement), 30);
                        int previousCost = Cost(owner.Alternative(stop.ParentNode, previousArc, target));
                        if (previousCost >= Impossible)
                        {
                            AppendFallback(stop, currentArc, index);
                            break;
                        }
                        var stopRecord = Record(stop.Node, stop.Arc);
                        var replacementRecord = Record(stop.ParentNode, replacement);
                        int stopCost = I32(stopRecord, 16);
                        int candidateCost = unchecked(I32(replacementRecord, 16) + stopCost);
                        if (candidateCost >= Impossible) break;
                        ushort predecessor = U16(stopRecord, 30);
                        int correction;
                        if (predecessor < primaryArcLimit230a)
                            correction = unchecked(previousCost - I32(Record(stop.ParentNode, previousArc), 16) + stopCost);
                        else
                        {
                            var higher = Record(stop.ParentNode, predecessor);
                            var reference = Record(stop.ParentNode, U16(higher, 30));
                            int offset = 0, anchorNode = I32(reference, 56);
                            if (anchorNode >= 0)
                            {
                                var anchor = Record(anchorNode, U16(reference, 60));
                                int ancestor = PreviousNode(anchorNode, anchor[32]);
                                if (ancestor >= 0)
                                    offset = unchecked(I32(Record(ancestor, U16(higher, 62)), 16) -
                                        I32(Record(ancestor, U16(anchor, 30)), 16));
                            }
                            correction = unchecked(stopCost - I32(reference, 16) - offset + previousCost);
                        }
                        AppendPrepared(new(candidateCost, correction, stop.Node, currentNode, stop.ParentNode,
                            stop.Arc, currentArc, replacement, previousArc, predecessor, (byte)index, (byte)target));
                    }
                }
        }
        cancellationToken.ThrowIfCancellationRequested();
        var storage = new byte[checked(candidates.Count * 36)];
        for (int index = 0; index < candidates.Count; index++) candidates[index].CopyTo(storage, index * 36);
        return new(storage, candidates.Count);

        ReadOnlySpan<byte> Record(int node, ushort arc)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (arc < primaryArcLimit230a) return owner.Record(node, arc);
            if (higherArcRowLookup is null)
                throw new NotSupportedException("Actual higher-arc stored record ownership is required.");
            var row = higherArcRowLookup(node, arc);
            cancellationToken.ThrowIfCancellationRequested();
            ArgumentNullException.ThrowIfNull(row);
            if (row.Record.Length != 88) throw new ArgumentException("Complete actual higher-arc stored record required.");
            // The owning provider returns a snapshot, not an address facade.
            return row.Record.ToArray();
        }
        ReadOnlySpan<byte> Residue()
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (candidates.Count == 65536)
                throw new NotSupportedException("Bounded native rejection candidate storage exceeds 65536 records.");
            var residue = appendStackResidue(candidates.Count);
            cancellationToken.ThrowIfCancellationRequested();
            if (residue.Length != 36) throw new ArgumentException("Exact stack residue for each candidate append required.");
            return residue.Span;
        }
        void AppendFallback(Stop stop, ushort currentArc, int sourceIndex)
            => candidates.Add(LegacyNativeRejectionFallbackProducer.Create(Residue(), primaryArcLimit230a,
                new(stop.Node, currentNode, stop.ParentNode, stop.Arc, currentArc, (byte)sourceIndex), cancellationToken));
        void AppendPrepared(LegacyNativePreparedRejectionFields fields)
            => candidates.Add(LegacyNativePreparedRejectionProducer.Create(Residue(), fields, cancellationToken));
    }

    private static int PreviousNode(int node, byte span) => unchecked(node - (sbyte)span - 1);
    private static int I32(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadInt32LittleEndian(data[offset..]);
    private static ushort U16(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(data[offset..]);
    private static ulong U64(ReadOnlySpan<byte> data, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(data[offset..]);
    private static int Cost(ReadOnlySpan<byte> data) => I32(data, 0);
}
