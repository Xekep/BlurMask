using System.Buffers.Binary;

namespace InkRecognition;

public readonly record struct LegacyNativeAlternativeWordCandidateResult(bool Succeeded,
    int GeometryCalls, int AcousticCalls, int AcceptedAllocations);

/// <summary>Ordinary regular1fe740: actual secondary restart group/query ->
/// every eligible parent/arc118+140 -> best-parent reduction -> one128 per arc
/// -> native initial-record allocation. Position penalties, positive ec secondary
/// routing, special labels and tracing are not silently replaced with defaults.</summary>
public sealed class LegacyNativeOrdinaryAlternativeWordCandidates
{
    private readonly LegacyOrdinaryCursiveCandidateGraph graph;
    private readonly LegacyOrdinaryCandidateConnections connections;
    private readonly LegacyInkAlphabet alphabet;
    private readonly LegacyCursiveCategoryProfile categories;
    private readonly LegacyCursiveCategoryMapper map;
    private readonly LegacySymbolContextFactory contexts;
    private readonly LegacyMainWordTransitionGeometry geometry;
    private readonly LegacyNativeWordAcousticRequestScorer scorer;
    private readonly LegacyNativeWordTransitionCostCell cell;
    private readonly byte[] residue;
    private bool valid = true;
    private int active;

    public LegacyNativeOrdinaryAlternativeWordCandidates(LegacyOrdinaryCursiveCandidateGraph graph,
        LegacyInkAlphabet alphabet, LegacyCursiveCategoryProfile categories,
        LegacySymbolContextFactory contexts, LegacyMainWordTransitionGeometry geometry,
        LegacyNativeWordAcousticRequestScorer scorer, LegacyNativeWordTransitionCostCell cell,
        ReadOnlySpan<byte> residue80)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(alphabet);
        ArgumentNullException.ThrowIfNull(categories);
        ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(geometry);
        ArgumentNullException.ThrowIfNull(scorer);
        ArgumentNullException.ThrowIfNull(cell);
        if (residue80.Length != 80 || graph.NodeCount > ushort.MaxValue)
            throw new ArgumentException("Actual bounded graph and complete request residue required.");
        this.graph = graph;
        this.alphabet = alphabet;
        this.categories = categories;
        this.contexts = contexts;
        this.geometry = geometry;
        this.scorer = scorer;
        this.cell = cell;
        residue = residue80.ToArray();
        connections = new(graph);
        map = new(categories);
    }

    public LegacyNativeAlternativeWordCandidateResult ProcessQuery(ReadOnlySpan<byte> query32,
        LegacyNativeAlternativeWordQueryGroup group, IReadOnlyList<ReadOnlyMemory<byte>> storedRows88,
        ushort endingNode, byte span, int previousEndingNode, ushort inputFlags,
        LegacyNativeWordSymbolQualifier qualifier, IReadOnlyDictionary<byte, uint> sourceHeaders,
        Func<int, ushort, LegacyNativeWordAcousticGroupCosts> getGroupCosts,
        Func<int, ushort, ReadOnlyMemory<byte>> owningRowLookup, LegacyNativeWordHeapRouter router,
        float knowledgeScaleEc, bool positionPenaltyEnabled, bool limitedMode2601, bool boundary254c,
        bool suppressHeader22f7, byte permission2309, bool suppressPrevious2609,
        bool frontendSuppressionEnabled, Func<int, ushort, bool>? isSuppressedE8,
        CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0)
            throw new InvalidOperationException("Alternative candidate calls must be serialized.");
        try
        {
            if (!valid) throw new InvalidOperationException("Failed alternative pass requires a fresh producer.");
            ArgumentNullException.ThrowIfNull(group);
            ArgumentNullException.ThrowIfNull(storedRows88);
            ArgumentNullException.ThrowIfNull(qualifier);
            ArgumentNullException.ThrowIfNull(sourceHeaders);
            ArgumentNullException.ThrowIfNull(getGroupCosts);
            ArgumentNullException.ThrowIfNull(owningRowLookup);
            ArgumentNullException.ThrowIfNull(router);
            cancellationToken.ThrowIfCancellationRequested();
            if (query32.Length != 32 || endingNode >= graph.NodeCount || span > 14 || previousEndingNode < 0 ||
                endingNode - span - 1 != previousEndingNode || group.EligibleRowOrdinals.IsEmpty ||
                group.SmallState21 > 63 || group.RestartTag28 == ulong.MaxValue ||
                qualifier.AcousticLabelCount != alphabet.LabelCount || !float.IsFinite(knowledgeScaleEc) ||
                storedRows88.Count > ushort.MaxValue || group.EligibleRowOrdinals.Length > storedRows88.Count)
                throw new ArgumentException("Actual bounded restart group, adjacent graph span and complete query32 required.");
            if (knowledgeScaleEc > 0 || positionPenaltyEnabled || limitedMode2601)
                throw new NotSupportedException("Positive ec routing, limited mode and active position penalties need their complete native policy.");
            if (frontendSuppressionEnabled && isSuppressedE8 is null)
                throw new ArgumentException("Enabled frontend suppression requires its actual graph predicate.");
            var query = query32.ToArray();
            ushort symbol = U16(query, 20);
            byte length = query[22], flags = query[23], kind = (byte)(flags & 7);
            if (kind is not (0 or 2) || !Plain(symbol) || U64(query, 24) != 0)
                throw new NotSupportedException("Ordinary plain kind0/2 query without extra context required.");
            ulong tag = U64(query, 0), context = U64(query, 8);
            if (context == 0 || contexts.ReadNode(context).Symbol != symbol)
                throw new InvalidDataException("Current query label differs from its owned symbol context.");
            byte sourceId = query[4];
            if (sourceId == 0 || !sourceHeaders.TryGetValue(sourceId, out uint currentHeader))
                throw new InvalidDataException("Query source ID has no prepared owner header.");
            // Validate extents and identity before copying or calling external code.
            var ordinals = group.EligibleRowOrdinals.ToArray();
            var parents = new byte[ordinals.Length][];
            for (int i = 0; i < ordinals.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (ordinals[i] >= storedRows88.Count || storedRows88[ordinals[i]].Length != 88 ||
                    i == 0 && ordinals[i] != group.FirstRowOrdinal || i > 0 && ordinals[i] <= ordinals[i - 1])
                    throw new ArgumentException("Ordered secondary ordinals must own complete stored88 rows.");
                var row = storedRows88[ordinals[i]].Span;
                if (U64(row, 40) != group.RestartTag28 || (row[33] & 63) != group.SmallState21 || (row[33] & 128) != 0)
                    throw new InvalidDataException("Restart group no longer matches its owning secondary rows.");
                if (!Plain(U16(row, 36)))
                    throw new NotSupportedException("Special previous symbols, including U+FFFD's kind override, require their native state path.");
                parents[i] = row.ToArray();
            }
            if (!qualifier.Qualifies(symbol, length, context, contexts, cancellationToken)) return new(true, 0, 0, 0);
            int geometryCalls = 0, acousticCalls = 0, allocations = 0;
            for (int variant = 0; variant < ((inputFlags & 16) == 0 ? 2 : 1); variant++)
            {
                int category = map.GetSymbolCompositeCategory(symbol, endBoundary: variant != 0);
                if (category < 0)
                {
                    if (variant == 0) throw new NotSupportedException("Initial symbol composite category is absent.");
                    break;
                }
                if (category > 63) throw new NotSupportedException("Composite category does not fit native six bits.");
                var reduction = new LegacyNativeAlternativeWordArcReduction();
                LegacyNativeWordRecombinationRequest? lastRequest = null;
                for (int i = 0; i < parents.Length && !reduction.Full; i++)
                {
                    var parent = parents[i];
                    if (!sourceHeaders.TryGetValue(parent[4], out uint parentHeader) || parent[4] == 0)
                        throw new InvalidDataException("Parent source ID has no prepared owner header.");
                    var related = owningRowLookup(previousEndingNode, U16(parent, 30));
                    cancellationToken.ThrowIfCancellationRequested();
                    if (related.Length != 88) throw new InvalidDataException("Parent related ordinal must own a stored88 row.");
                    byte[] relatedCopy = related.ToArray();
                    foreach (int ordinal in connections.GetSuccessorOrdinals(previousEndingNode, U16(parent, 28), span).Span)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (reduction.Full) break;
                        ushort arc = checked((ushort)ordinal);
                        int slot = reduction.FindOrAdd(arc);
                        lastRequest = LegacyNativeWordAlternativeRequestFactory.Create(endingNode, span, previousEndingNode,
                            parent, relatedCopy, arc, symbol, category, tag, length, flags, currentHeader, parentHeader,
                            permission2309, categories, cell, residue, cancellationToken);
                        geometryCalls++;
                        if (geometry.Calculate118(lastRequest, cancellationToken) && geometry.Validate140(lastRequest, cancellationToken))
                            reduction.TryImprove(slot, cell.Value, lastRequest.ReadInt32(48), ordinals[i],
                                lastRequest.ReadByte(60), lastRequest.ReadByte(61), boundary254c);
                    }
                }
                if (lastRequest is null) continue;
                foreach (var best in reduction.GetSnapshot().Span)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (best.Cost >= 0x4000000) continue;
                    if (best.CallbackFlags != 0)
                        throw new NotSupportedException("Geometry context rewrite flags require native symbolic context recombination.");
                    // Nontracing1fe740 leaves the last geometry packet's previous
                    // fields intact; it installs only this arc and reduced cost.
                    W16(lastRequest, 12, best.ArcOrdinal);
                    W32(lastRequest, 48, best.Increment);
                    cell.Value = unchecked(best.Cost + BinaryPrimitives.ReadInt32LittleEndian(query.AsSpan(16)));
                    acousticCalls++;
                    if (!scorer.Score(lastRequest, getGroupCosts, frontendSuppressionEnabled, isSuppressedE8, cancellationToken)) continue;
                    var bestParent = parents[Array.IndexOf(ordinals, best.ParentRowOrdinal)];
                    bool flagged = !suppressHeader22f7 && (alphabet.IsSymbolFlagged(symbol) ||
                        (group.RestartTag28 & (4UL << 40)) != 0 && (bestParent[5] & 128) != 0);
                    int firstId = unchecked((int)graph.GetNode(endingNode)[best.ArcOrdinal].StartContextId);
                    var fields = new LegacyInitialWordRecordFields(tag, context, cell.Value, lastRequest.ReadInt32(48), symbol,
                        endingNode, best.ArcOrdinal, best.ArcOrdinal, (byte)category, best.ParentRowOrdinal, unchecked((sbyte)span), 0,
                        flagged ? (byte)1 : (byte)0, 0, 0, kind, unchecked((byte)((flags >> 5 & 3) + length)),
                        0, 0, (flags & 8) != 0 ? firstId : -1, firstId, endingNode, ushort.MaxValue);
                    var allocation = router.TryInsertInitialRecord(sourceId, false, fields, knowledgeScaleEc,
                        suppressPrevious2609, Lookup, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!allocation.Accepted)
                    {
                        valid = false;
                        return new(false, geometryCalls, acousticCalls, allocations);
                    }
                    allocations++;
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
            return new(true, geometryCalls, acousticCalls, allocations);

            ReadOnlyMemory<byte> Lookup(int node, ushort ordinal)
            {
                var row = owningRowLookup(node, ordinal);
                cancellationToken.ThrowIfCancellationRequested();
                if (row.Length != 88) throw new InvalidDataException("Native history lookup must own a complete stored88 row.");
                return row;
            }
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }

    private bool Plain(ushort symbol) => alphabet.TryGetText(symbol, out string text) && text.Length == 1 &&
        text[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '\u0400' and <= '\u04ff' or >= '0' and <= '9';
    private static ushort U16(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(value[offset..]);
    private static ulong U64(ReadOnlySpan<byte> value, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(value[offset..]);
    private static void W16(LegacyNativeWordRecombinationRequest request, int offset, ushort value)
    { request.WriteByte(offset, (byte)value); request.WriteByte(offset + 1, (byte)(value >> 8)); }
    private static void W32(LegacyNativeWordRecombinationRequest request, int offset, int value)
    { for (int i = 0; i < 4; i++) request.WriteByte(offset + i, (byte)((uint)value >> (i * 8))); }
}
