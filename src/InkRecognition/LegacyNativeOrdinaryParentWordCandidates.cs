using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>
/// Regular201270 parent row/query page -> native main118/140 ->128 -> ordinary
/// path guard -> full1f8d60 allocation. This is not the outer2144f0 controller.
/// The provided main geometry owns the same prepared graph/source facts; source
/// frame and controller configuration must come from their actual producers.
/// </summary>
public sealed class LegacyNativeOrdinaryParentWordCandidates
{
    private readonly LegacyOrdinaryCursiveCandidateGraph graph;
    private readonly LegacyOrdinaryCandidateConnections connections;
    private readonly LegacyInkAlphabet alphabet;
    private readonly LegacyCursiveCategoryProfile categories;
    private readonly LegacyCursiveCategoryMapper categoryMap;
    private readonly LegacySymbolContextFactory contexts;
    private readonly LegacyMainWordTransitionGeometry geometry;
    private readonly LegacyNativeWordAcousticRequestScorer scorer;
    private readonly LegacyNativeWordTransitionCostCell cell;
    private readonly byte[] residue;
    private readonly ushort loop;
    private bool valid = true;
    private int active;

    public LegacyNativeOrdinaryParentWordCandidates(LegacyOrdinaryCursiveCandidateGraph graph,
        LegacyInkAlphabet alphabet, LegacyCursiveCategoryProfile categories, LegacySymbolContextFactory contexts,
        LegacyMainWordTransitionGeometry geometry, LegacyNativeWordAcousticRequestScorer scorer,
        LegacyNativeWordTransitionCostCell cell, ReadOnlySpan<byte> stackResidue80)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(alphabet);
        ArgumentNullException.ThrowIfNull(categories);
        ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(geometry);
        ArgumentNullException.ThrowIfNull(scorer);
        ArgumentNullException.ThrowIfNull(cell);
        if (stackResidue80.Length != 80 || graph.NodeCount > ushort.MaxValue || alphabet.LabelCount > 4093)
            throw new ArgumentException("Bounded owning graph/alphabet and full80 residue required.");
        this.graph = graph;
        this.alphabet = alphabet;
        this.categories = categories;
        this.contexts = contexts;
        this.geometry = geometry;
        this.scorer = scorer;
        this.cell = cell;
        connections = new(graph);
        categoryMap = new(categories);
        residue = stackResidue80.ToArray();
        loop = alphabet.TryGetLabel("\U000f0001", out ushort label) ? label : ushort.MaxValue;
    }

    /// <summary>
    /// Call only after ApplyContinuation/query preparation accepted this actual
    /// parent row. Ordinary frontend280=false,2601=false and2368=false. Knowledge
    /// F5 and other controller/cache policy values are explicit, never guessed.
    /// The incoming parent row is copied before callbacks. Ordinary heap-budget
    /// rejection skips that candidate; throwing/canceled preparation invalidates
    /// this serialized producer.
    /// </summary>
    public LegacyNativeOrdinaryWordStartupResult ProcessPage(ReadOnlySpan<byte> queryRecords32,
        ushort endingNode, byte sourceSpan, int previousEndingNode, ushort previousRowOrdinal,
        ReadOnlySpan<byte> storedParent88, ushort inputFlags, LegacyNativeWordSymbolQualifier qualifier,
        IReadOnlyDictionary<byte, uint> preparedSourceHeaders,
        Func<int, ushort, LegacyNativeWordAcousticGroupCosts> getOwnedGroupCosts,
        Func<int, ushort, ReadOnlyMemory<byte>> owningRowLookup,
        LegacyNativeWordHeapRouter router, float secondaryScale, bool knowledgeF5,
        bool suppressHeaderFlag22f7, byte permission2309, bool suppressPreviousContext2609,
        bool frontendSuppressionEnabled, Func<int, ushort, bool>? isSuppressedE8,
        CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0)
            throw new InvalidOperationException("Ordinary parent page calls must be serialized.");
        try
        {
            if (!valid) throw new InvalidOperationException("Failed parent page requires a fresh producer.");
            ArgumentNullException.ThrowIfNull(qualifier);
            ArgumentNullException.ThrowIfNull(preparedSourceHeaders);
            ArgumentNullException.ThrowIfNull(getOwnedGroupCosts);
            ArgumentNullException.ThrowIfNull(owningRowLookup);
            ArgumentNullException.ThrowIfNull(router);
            cancellationToken.ThrowIfCancellationRequested();
            if (storedParent88.Length != 88 || queryRecords32.Length % 32 != 0 || queryRecords32.Length > 512 * 32 ||
                endingNode >= graph.NodeCount || previousEndingNode < 0 || sourceSpan > 14 ||
                endingNode - sourceSpan - 1 != previousEndingNode || qualifier.AcousticLabelCount != alphabet.LabelCount ||
                !float.IsFinite(secondaryScale))
                throw new ArgumentException("Actual bounded adjacent source span, complete parent88 and query page required.");
            if (frontendSuppressionEnabled && isSuppressedE8 is null)
                throw new ArgumentException("Enabled frontend suppression needs its actual graphE8 predicate.");
            var parent = storedParent88.ToArray();
            var queryBytes = queryRecords32.ToArray();
            if ((parent[6] & 1) != 0)
                throw new NotSupportedException("Parent header6.bit0 selects kind7 and requires its full native source/context path guard.");
            byte parentSourceId = parent[4];
            if (parentSourceId == 0 || !preparedSourceHeaders.TryGetValue(parentSourceId, out uint parentHeader))
                throw new InvalidDataException("Actual parent source ID has no prepared header.");
            int parentAuxiliary30 = BinaryPrimitives.ReadInt32LittleEndian(parent.AsSpan(0x30));
            int parentAuxiliary34 = BinaryPrimitives.ReadInt32LittleEndian(parent.AsSpan(0x34));
            int anchorNode = BinaryPrimitives.ReadInt32LittleEndian(parent.AsSpan(0x38));
            ushort anchorArc = BinaryPrimitives.ReadUInt16LittleEndian(parent.AsSpan(0x3c));
            var arcs = connections.GetSuccessorOrdinals(previousEndingNode,
                BinaryPrimitives.ReadUInt16LittleEndian(parent.AsSpan(0x1c)), sourceSpan).ToArray();
            int visited = 0, qualified = 0, calls = 0, accepted = 0;
            for (int offset = 0; offset < queryBytes.Length; offset += 32)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var row = queryBytes.AsSpan(offset, 32);
                visited++;
                ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(row[20..]);
                byte length = row[22], flags = row[23];
                if ((flags & 8) != 0 && parentAuxiliary30 != -1 || symbol == loop && parentAuxiliary30 == -1) continue;
                byte kind = (byte)(flags & 7);
                if (kind is not (0 or 2) || !Plain(symbol))
                    throw new NotSupportedException("Plain ordinary kind0/2 source query required; special/type7/category policies remain unported.");
                ulong tag = BinaryPrimitives.ReadUInt64LittleEndian(row);
                byte sourceId = (byte)(tag >> 32);
                if (sourceId == 0 || !preparedSourceHeaders.TryGetValue(sourceId, out uint header))
                    throw new InvalidDataException("Actual current source ID has no prepared header.");
                if (knowledgeF5 && (header >> 9 & 15) == 2) continue;
                ulong context = BinaryPrimitives.ReadUInt64LittleEndian(row[8..]);
                if (context == 0 || contexts.ReadNode(context).Symbol != symbol)
                    throw new InvalidDataException("Actual parent-query context symbol differs from its current label.");
                if (!qualifier.Qualifies(symbol, length, context, contexts, cancellationToken)) continue;
                qualified++;
                for (int variant = 0; variant < ((inputFlags & 0x10) == 0 ? 2 : 1); variant++)
                {
                    int category = categoryMap.GetSymbolCompositeCategory(symbol, endBoundary: variant != 0);
                    if (category < 0)
                    {
                        if (variant == 0) throw new NotSupportedException("Missing initial composite mapping has no bounded native geometry lookup.");
                        break;
                    }
                    if (category > 63) throw new NotSupportedException("Native composite category requires six bits.");
                    foreach (int ordinal in arcs)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        ushort arc = checked((ushort)ordinal);
                        var request = LegacyNativeWordParentRequestFactory.Create(endingNode, sourceSpan, previousEndingNode,
                            parent, arc, symbol, category, tag, length, flags, header, parentHeader, permission2309,
                            categories, cell, BinaryPrimitives.ReadInt32LittleEndian(row[16..]), residue, cancellationToken);
                        calls++;
                        if (!geometry.Calculate118(request, cancellationToken) || !geometry.Validate140(request, cancellationToken) ||
                            !scorer.Score(request, getOwnedGroupCosts, frontendSuppressionEnabled, isSuppressedE8, cancellationToken)) continue;
                        //1fcf40: for kind!=7 and low request3c==0, neither the
                        //source boundary exception nor symbolic control-root guard applies.
                        if (request.ReadByte(28) is not (0 or 2) || request.ReadByte(60) != 0)
                            throw new NotSupportedException("Native ordinary path guard requires the proved unmarked kind0/2 branch.");
                        int start = unchecked((int)graph.GetNode(endingNode)[ordinal].StartContextId);
                        bool flagged = !suppressHeaderFlag22f7 &&
                            (kind == 0 && (parent[5] & 0x80) != 0 || alphabet.IsSymbolFlagged(symbol));
                        var fields = new LegacyInitialWordRecordFields(tag, context, cell.Value, request.ReadInt32(0x30), symbol,
                            endingNode, arc, arc, checked((byte)category), previousRowOrdinal, unchecked((sbyte)sourceSpan), 0,
                            flagged ? (byte)1 : (byte)0, 0, 0, kind, unchecked((byte)((flags >> 5 & 3) + length)),
                            0, 0, (flags & 8) != 0 ? start : parentAuxiliary30, kind == 2 ? start : parentAuxiliary34,
                            anchorNode, anchorArc);
                        var allocation = router.TryInsertInitialRecord(sourceId, false, fields, secondaryScale,
                            suppressPreviousContext2609, LookupRow, cancellationToken);
                        cancellationToken.ThrowIfCancellationRequested();
                        if (allocation.Accepted) accepted++;
                    }
                }
            }
            cancellationToken.ThrowIfCancellationRequested();
            return new(true, visited, qualified, calls, accepted);

            ReadOnlyMemory<byte> LookupRow(int node, ushort ordinal)
            {
                var row = owningRowLookup(node, ordinal);
                cancellationToken.ThrowIfCancellationRequested();
                if (row.Length != 88) throw new InvalidDataException("Owning word-row lookup must return its complete materialized88-byte row.");
                return row;
            }
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }

    private bool Plain(ushort symbol) => alphabet.TryGetText(symbol, out string text) && text.Length == 1 &&
        text[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '\u0400' and <= '\u04ff' or >= '0' and <= '9';
}
