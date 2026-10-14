using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyNativeOrdinaryWordStartupResult(bool Succeeded, int RowsVisited,
    int QualifiedRows, int ScoringCalls, int AcceptedAllocations);

/// <summary>
/// Bounded ordinary single-symbol, nontracing1fd240 query-page consumer.
/// Source rows, qualifier, source headers and final caches are actual prepared
/// owners. It restores category/arc enumeration and all1f8d60 initializer fields
/// in this branch. Case rewrites, punctuation extras and protocol queries are
/// rejected rather than silently mapped to ordinary symbols.
/// </summary>
public sealed class LegacyNativeOrdinaryWordStartup
{
    private readonly LegacyOrdinaryCursiveCandidateGraph graph;
    private readonly LegacyOrdinaryCandidateConnections connections;
    private readonly LegacyInkAlphabet alphabet;
    private readonly LegacyCursiveCategoryProfile categories;
    private readonly LegacyCursiveCategoryMapper categoryMap;
    private readonly LegacySymbolContextFactory contexts;
    private readonly LegacyNativeWordAcousticRequestScorer scorer;
    private readonly LegacyNativeWordTransitionCostCell cell;
    private readonly byte[] residue;
    private readonly ushort excludedLoop;
    private readonly ushort excludedShadda;
    private readonly HashSet<ushort> special;
    private bool valid = true;
    private int active;

    public LegacyNativeOrdinaryWordStartup(LegacyOrdinaryCursiveCandidateGraph graph,
        LegacyInkAlphabet alphabet, LegacyCursiveCategoryProfile categories,
        LegacySymbolContextFactory contexts, LegacyNativeWordAcousticRequestScorer scorer,
        LegacyNativeWordTransitionCostCell cell, ReadOnlySpan<byte> stackResidue80)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(alphabet);
        ArgumentNullException.ThrowIfNull(categories);
        ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(scorer);
        ArgumentNullException.ThrowIfNull(cell);
        if (stackResidue80.Length != 80 || graph.NodeCount > ushort.MaxValue || alphabet.LabelCount > 4093)
            throw new ArgumentException("Bounded owned graph/alphabet and full native stack residue required.");
        this.graph = graph;
        this.alphabet = alphabet;
        this.categories = categories;
        this.contexts = contexts;
        this.scorer = scorer;
        this.cell = cell;
        connections = new(graph);
        categoryMap = new(categories);
        residue = stackResidue80.ToArray();
        excludedLoop = Resolve("\U000f0001");
        excludedShadda = Resolve("\u0651");
        special = new[] { "'", "\"", "\U000f0002", "\U000f0003", "\U000f0004", "\U000f0008" }
            .Select(Resolve).Where(s => s != ushort.MaxValue).ToHashSet();
    }

    /// <summary>
    /// This branch has ordinary frontend280=false, controller2601=false and no
    /// protocol/case rewrite. With frontend280=false the native600 input bits do
    /// not enable1f7080. SuppressHeaderFlag22f7, permission2309, secondary
    /// heap scale and suppressPreviousContext2609 are explicit controller fields.
    /// Source headers are indexed by real aggregate source ID(tag byte04), not
    /// a fabricated source ordinal. Ordinary heap-budget rejection skips a
    /// candidate: nontracing1f8d60 still returns native success without a slot.
    /// </summary>
    public LegacyNativeOrdinaryWordStartupResult ProcessPage(ReadOnlySpan<byte> queryRecords32,
        ushort endingNode, byte sourceSpan, ushort inputFlags,
        LegacyNativeWordSymbolQualifier qualifier, IReadOnlyDictionary<byte, uint> preparedSourceHeaders,
        Func<int, ushort, LegacyNativeWordAcousticGroupCosts> getOwnedGroupCosts,
        LegacyNativeWordHeapRouter router, float secondaryScale,
        bool suppressHeaderFlag22f7, byte permission2309, bool suppressPreviousContext2609,
        bool frontendSuppressionEnabled, Func<int, ushort, bool>? isSuppressedE8,
        CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0)
            throw new InvalidOperationException("Native startup page calls must be serialized.");
        try
        {
            if (!valid) throw new InvalidOperationException("Failed startup page requires a fresh producer.");
            var result = ProcessOwnedPage(queryRecords32, endingNode, sourceSpan, inputFlags, qualifier,
                preparedSourceHeaders, getOwnedGroupCosts, router, secondaryScale, suppressHeaderFlag22f7,
                permission2309, suppressPreviousContext2609, frontendSuppressionEnabled, isSuppressedE8, cancellationToken);
            if (!result.Succeeded) valid = false;
            return result;
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }

    private LegacyNativeOrdinaryWordStartupResult ProcessOwnedPage(ReadOnlySpan<byte> queryRecords32,
        ushort endingNode, byte sourceSpan, ushort inputFlags,
        LegacyNativeWordSymbolQualifier qualifier, IReadOnlyDictionary<byte, uint> preparedSourceHeaders,
        Func<int, ushort, LegacyNativeWordAcousticGroupCosts> getOwnedGroupCosts,
        LegacyNativeWordHeapRouter router, float secondaryScale,
        bool suppressHeaderFlag22f7, byte permission2309, bool suppressPreviousContext2609,
        bool frontendSuppressionEnabled, Func<int, ushort, bool>? isSuppressedE8, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(qualifier);
        ArgumentNullException.ThrowIfNull(preparedSourceHeaders);
        ArgumentNullException.ThrowIfNull(getOwnedGroupCosts);
        ArgumentNullException.ThrowIfNull(router);
        cancellationToken.ThrowIfCancellationRequested();
        if (queryRecords32.Length % 32 != 0 || queryRecords32.Length > 512 * 32 ||
            endingNode >= graph.NodeCount || sourceSpan > 14 || sourceSpan != endingNode ||
            qualifier.AcousticLabelCount != alphabet.LabelCount || !float.IsFinite(secondaryScale))
            throw new ArgumentException("Actual bounded initial query page, whole-prefix source span and matching qualifier required.");
        if (frontendSuppressionEnabled && isSuppressedE8 is null)
            throw new ArgumentException("Enabled native frontend suppression requires its owned graphE8 predicate.");
        var arcs = connections.GetSuccessorOrdinals(-1, ushort.MaxValue, sourceSpan).ToArray();
        int visited = 0, qualified = 0, calls = 0, accepted = 0;
        for (int offset = 0; offset < queryRecords32.Length; offset += 32)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var row = queryRecords32.Slice(offset, 32);
            visited++;
            ulong tag = BinaryPrimitives.ReadUInt64LittleEndian(row);
            ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(row[20..]);
            byte length = row[22], flags = row[23];
            if (symbol == excludedLoop || symbol == excludedShadda) continue;
            if (symbol >= alphabet.LabelCount || special.Contains(symbol))
                throw new NotSupportedException("Protocol/quote/primitive-extra startup branch is not ordinary single-symbol enumeration.");
            byte sourceId = (byte)(tag >> 32);
            if (sourceId == 0 || !preparedSourceHeaders.TryGetValue(sourceId, out uint header))
                throw new InvalidDataException("Actual aggregate source ID has no prepared owning header.");
            ulong queryContext = BinaryPrimitives.ReadUInt64LittleEndian(row[8..]);
            if (queryContext == 0 || contexts.ReadNode(queryContext).Symbol != symbol)
                throw new InvalidDataException("Actual query context symbol differs from its row label.");
            ulong context = LegacyNativeWordStartupRequestFactory.GraftOrdinaryInitialContext(contexts, queryContext, cancellationToken);
            if (!qualifier.Qualifies(symbol, length, context, contexts, cancellationToken)) continue;
            qualified++;
            int variantCount = (inputFlags & 0x10) == 0 ? 2 : 1;
            for (int variant = 0; variant < variantCount; variant++)
            {
                int category = categoryMap.GetSymbolCompositeCategory(symbol, endBoundary: variant != 0);
                if (category < 0)
                {
                    if (variant == 0) throw new NotSupportedException("Missing initial composite mapping has no bounded native geometry lookup.");
                    break;
                }
                if (category > 63) throw new NotSupportedException("Native ordinary word category requires six bits.");
                foreach (int ordinal in arcs)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ushort arc = checked((ushort)ordinal);
                    var owned = graph.GetNode(endingNode)[ordinal];
                    var request = LegacyNativeWordStartupRequestFactory.Create(endingNode, sourceSpan, arc, symbol, category,
                        tag, length, flags, header, permission2309, categories, cell,
                        BinaryPrimitives.ReadInt32LittleEndian(row[16..]), residue, cancellationToken);
                    calls++;
                    if (!scorer.Score(request, getOwnedGroupCosts, frontendSuppressionEnabled, isSuppressedE8, cancellationToken)) continue;
                    int auxiliary34 = unchecked((int)owned.StartContextId);
                    var fields = new LegacyInitialWordRecordFields(tag, context, cell.Value, request.ReadInt32(0x30), symbol,
                        endingNode, arc, arc, checked((byte)category), ushort.MaxValue, unchecked((sbyte)sourceSpan),
                        0, !suppressHeaderFlag22f7 && alphabet.IsSymbolFlagged(symbol) ? (byte)1 : (byte)0,
                        0, 0, (byte)(flags & 7), unchecked((byte)((flags >> 5 & 3) + length)),
                        0, 0, (flags & 8) != 0 ? auxiliary34 : -1, auxiliary34, endingNode, ushort.MaxValue);
                    var allocation = router.TryInsertInitialRecord(sourceId, false, fields, secondaryScale,
                        suppressPreviousContext2609, cancellationToken: cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                    if (allocation.Accepted) accepted++;
                }
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(true, visited, qualified, calls, accepted);
    }

    private ushort Resolve(string text) => alphabet.TryGetLabel(text, out ushort value) ? value : ushort.MaxValue;
}
