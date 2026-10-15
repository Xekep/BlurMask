using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyNativeLaterDictionaryWordCompletionResult(bool LanguageAccepted,
    LegacyNativeWordHeapAllocation Allocation, int PreLexicalCost,
    LegacyNativePreparedWordHistory History, LegacyNativeKnownWordLanguageRecordResult Language,
    ulong RestartTag28, ulong ContextualLineage5c, LegacyWordPathBoundaryProjection Boundary);

/// <summary>Bounded204290 terminal/context-history consumer. Actual context16,
///anchor/node-row owners and required source descriptor-line getter feed100090,
///computed source50 restart and1f9080. Full scheduling/winner remains separate.</summary>
public sealed partial class LegacyNativeLaterDictionaryWordCompletion
{
    private readonly LegacyInkAlphabet alphabet;
    private readonly LegacyCursiveCategoryMapper categories;
    private readonly LegacyOrdinaryWordCategoryPolicy categoryPolicy;
    private readonly LegacyPreparedDictionaryWordMetadata? metadata;
    private readonly LegacyNativeKnownWordLanguageRecord language;
    private readonly LegacyPreparedDictionaryRestartTags? restart;
    private readonly byte sourceId;
    private readonly uint sourceHeader;
    private readonly ushort replacement, dash;
    private bool valid = true;
    private int active;

    public LegacyNativeLaterDictionaryWordCompletion(LegacyRussianModelBundle selectedModel,
        LegacyPreparedDictionaryWordMetadata metadata, LegacyNativeKnownWordLanguageRecord language,
        LegacyPreparedDictionaryRestartTags restart, byte selectedSourceId, uint preparedSourceHeader,
        bool nativeMode22f6, float knowledgeScaleEc, bool frontend280, bool trace2368)
    {
        ArgumentNullException.ThrowIfNull(selectedModel);
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(language);
        if (selectedSourceId == 0 || (preparedSourceHeader & 0x1e00) != 0xc00 || (preparedSourceHeader & 0x1c) <= 7 ||
            !metadata.IsForModel(selectedModel) || !language.IsForModel(selectedModel))
            throw new ArgumentException("Owned selected-model C00 dictionary/index/language profiles required.");
        if (nativeMode22f6 || !float.IsFinite(knowledgeScaleEc) || knowledgeScaleEc > 0 || frontend280 || trace2368)
            throw new NotSupportedException("Regular nontracing mode0, ordinary frontend and nonpositive ec branch required.");
        alphabet = selectedModel.AcousticAlphabet;
        categories = selectedModel.CreateCursiveCategoryMapper(1);
        categoryPolicy = new(categories);
        if (!alphabet.TryGetLabel("\ufffd", out replacement) || !alphabet.TryGetLabel("-", out dash))
            throw new NotSupportedException("Actual20c1c0 replacement and24C0 dash labels required.");
        this.metadata = metadata; this.language = language; this.restart = restart;
        sourceId = selectedSourceId; sourceHeader = preparedSourceHeader;
    }

    public LegacyNativeLaterDictionaryWordCompletionResult ProcessCandidate(
        LegacyNativeLanguageContextRecombinationResult contextResult, int candidateIndex,
        int endingNode, ushort currentRowOrdinal, int availableSourceNodes,
        Func<int, IReadOnlyList<LegacyNativeStoredWordRecord>> owningNodeRowsLookup,
        Func<byte, uint> preparedSourceHeader, Func<int, int, int> descriptorLineE8,
        LegacySymbolContextFactory contexts, LegacyNativeWordHeapRouter router,
        bool knowledgeF5, int subwordSourceIndex22ec, bool extendedMarkers22f8,
        ReadOnlySpan<byte> queryCacheResidue32, ReadOnlySpan<byte> temporaryLexicalResidue40,
        bool suppressPrevious2609, CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0)
            throw new InvalidOperationException("Native later-word completion calls must be serialized.");
        try
        {
            if (!valid) throw new InvalidOperationException("Failed later-word completion requires a fresh producer.");
            return ProcessCore(contextResult, candidateIndex, endingNode, currentRowOrdinal, availableSourceNodes,
                owningNodeRowsLookup, preparedSourceHeader, descriptorLineE8, contexts, router,
                knowledgeF5, subwordSourceIndex22ec, extendedMarkers22f8, queryCacheResidue32,
                temporaryLexicalResidue40, suppressPrevious2609, cancellationToken);
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }

    private LegacyNativeLaterDictionaryWordCompletionResult ProcessCore(
        LegacyNativeLanguageContextRecombinationResult contextResult, int candidateIndex,
        int endingNode, ushort currentRowOrdinal, int availableSourceNodes,
        Func<int, IReadOnlyList<LegacyNativeStoredWordRecord>> owningNodeRowsLookup,
        Func<byte, uint> preparedSourceHeader, Func<int, int, int> descriptorLineE8,
        LegacySymbolContextFactory contexts, LegacyNativeWordHeapRouter router,
        bool knowledgeF5, int subwordSourceIndex22ec, bool extendedMarkers22f8,
        ReadOnlySpan<byte> queryCacheResidue32, ReadOnlySpan<byte> temporaryLexicalResidue40,
        bool suppressPrevious2609, CancellationToken cancellationToken, byte[]? actualQuery32 = null)
    {
        ArgumentNullException.ThrowIfNull(contextResult);
        ArgumentNullException.ThrowIfNull(owningNodeRowsLookup);
        ArgumentNullException.ThrowIfNull(preparedSourceHeader);
        ArgumentNullException.ThrowIfNull(descriptorLineE8);
        ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(router);
        cancellationToken.ThrowIfCancellationRequested();
        caseSpan?.Validate(endingNode, contexts, knowledgeF5, subwordSourceIndex22ec, extendedMarkers22f8, suppressPrevious2609, cancellationToken);
        caseSpan?.ValidateExtent(availableSourceNodes);
        if (availableSourceNodes is < 1 or > 1_000_000 || (uint)endingNode >= (uint)availableSourceNodes ||
            contextResult.LastRequest80.Length != 80 || contextResult.Candidates16.Length % 16 != 0 ||
            contextResult.Candidates16.Length / 16 > ushort.MaxValue ||
            (uint)candidateIndex >= (uint)(contextResult.Candidates16.Length / 16) ||
            queryCacheResidue32.Length != 32 || temporaryLexicalResidue40.Length != 40)
            throw new ArgumentException("Owned context result/current node and full query/lexical residue required.");
        byte[] context = contextResult.Candidates16.Slice(candidateIndex * 16, 16).ToArray();
        byte[] request = contextResult.LastRequest80.ToArray();
        byte[] queryResidue = queryCacheResidue32.ToArray(), lexicalResidue = temporaryLexicalResidue40.ToArray();
        var ownedRows = new Dictionary<int, byte[][]>();
        byte[] current = LookupBytes(endingNode, currentRowOrdinal);
        if (current[4] != sourceId || (current[6] & 1) != 0)
            throw new NotSupportedException("Regular current word row must own the selected dictionary source.");
        byte[] query;
        if (actualQuery32 is null)
        {
            var injected = LegacyNativeFirstDictionaryWordCompletion.CreateAlreadyTerminalQuery(current, queryResidue, cancellationToken);
            if (!injected.HasValue) throw new NotSupportedException("Actual current-terminal injection required; use ProcessActualQuery for a supplied source page.");
            query = injected.Value.ToArray();
        }
        else
        {
            if (actualQuery32.Length != 32) throw new ArgumentException("A complete source-owned query32 is required.");
            query = actualQuery32;
            if (query[4] != sourceId) throw new ArgumentException("Query must own the bound selected source.");
            ulong queryContext = U64(query, 8);
            if (queryContext != 0) _ = contexts.ReadNode(queryContext);
        }
        if (U64(query, 24) != 0 || (query[23] & 7) is not (0 or 2))
            throw new NotSupportedException("Ordinary cached terminal query without opaque extra context required.");
        ushort symbol = U16(query, 20);
        if (actualQuery32 is null
            ? !alphabet.TryGetText(symbol, out string text) || text.Length != 1 ||
                text[0] is not (>= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '\u0400' and <= '\u04ff' or >= '0' and <= '9')
            : symbol >= alphabet.LabelCount || alphabet.IsVirtual(symbol) || !alphabet.TryGetText(symbol, out _) ||
                symbol == replacement || U16(current, 36) == replacement)
            throw new NotSupportedException("Plain actual terminal label required.");
        int anchorEnding = I32(current, 56);
        ushort anchorOrdinal = U16(current, 60);
        if (anchorEnding < 0 || anchorEnding > endingNode || anchorOrdinal == ushort.MaxValue)
            throw new NotSupportedException("Actual materialized current-word anchor required.");
        byte[] anchor = LookupBytes(anchorEnding, anchorOrdinal);
        int span = (sbyte)anchor[32], previousNode = unchecked(anchorEnding - span - 1);
        if (span is < 0 or > 14 || previousNode < 0 || I32(request, 0) != anchorEnding || I32(request, 8) != previousNode ||
            I32(request, 32) != span || U16(request, 12) != U16(anchor, 28) || anchor[4] != sourceId)
            throw new ArgumentException("Context packet must belong to this current word's anchor/previous node.");
        ushort previousOrdinal = U16(context, 12);
        caseSpan?.ValidateSecondaryOrdinal(previousOrdinal);
        if (U16(context, 14) != ushort.MaxValue && U16(context, 14) != currentRowOrdinal)
            throw new ArgumentException("Fast context lineage must reference this current word ordinal.");
        byte[] previous = LookupBytes(previousNode, previousOrdinal);
        byte[] originalPrevious = LookupBytes(previousNode, U16(anchor, 30));
        if ((previous[33] & 128) != 0 || U64(previous, 40) != U64(originalPrevious, 40) || I32(context, 0) >= 0x4000000)
            throw new ArgumentException("Context candidate must own matching finite secondary-state ancestry.");
        //All selected current/anchor/previous rows are now owned snapshots.
        uint header = Header(sourceId);
        if (header != sourceHeader) throw new ArgumentException("Current source header differs from the bound selected owner.");
        var category = categoryPolicy.Resolve(current[26], symbol, query[22], checked((ushort)alphabet.LabelCount),
            query[23], replacement, false, cancellationToken);
        var boundary = LegacyWordPathBoundaryCost.Evaluate(LegacyPreparedWordPathReference.Read(anchor, anchorEnding),
            categories, availableSourceNodes, false, (byte)sourceHeader, Line, cancellationToken);
        int path = checked(checked(I32(context, 0) + I32(query, 16)) + boundary.AddedCost);
        var heap = router.GetPrimaryHeap(sourceId);
        int limit = heap.Count != 0 && heap.Count == heap.Capacity ? I32(heap.GetSlotSnapshot(0).Span, 16) : 0x4000000;
        ulong tag = U64(query, 0);
        var token = caseSpan is null
            ? metadata!.ReadTerminal(tag & ~(255UL << 32), 0, cancellationToken)
            : caseSpan.ReadTerminal(tag, symbol, cancellationToken);
        var history = LegacyNativeWordHistoryPreparation.Prepare(token.Span, sourceId, previous, previousNode,
            anchor, boundary.BoundaryDifference, subwordSourceIndex22ec, extendedMarkers22f8, knowledgeF5,
            dash, availableSourceNodes, contexts, LookupRecord, Header, Line, cancellationToken);
        //Reused-history local148 receives the live path-adjusted budget.
        //Fallback localF8 stays mode0/count0; its request28 is the literalIMP
        //initialized once at204546, not a synthesized first-word request.
        int budget = history.UsesPreviousHistory ? checked(limit - path) : 0x4000000;
        //205368/205BCD copy source70 into request18 without replacing
        //its category at20. Geometry category remains a separate path input.
        int languageCategory = BinaryPrimitives.ReadInt32LittleEndian(history.SourceToken16.Span[8..]);
        var evaluation = language.Evaluate(history.SourceToken16.Span, history.History.Span, languageCategory,
            history.ClearsFirstCategoryTag, budget, lexicalResidue, cancellationToken);
        ulong restartTag = caseSpan is null
            ? restart!.Value.Resolve(tag & ~(255UL << 32))
            : caseSpan.ResolveRestart(tag, cancellationToken);
        if ((restartTag & (4UL << 40)) != 0)
            restartTag = (restartTag & ~(255UL << 32)) | (ulong)sourceId << 32;
        restartTag = (restartTag & ~(0x70UL << 40)) | (ulong)(query[23] << 4 & 0x70) << 40;
        ulong lineage = (ulong)BinaryPrimitives.ReadUInt32LittleEndian(context.AsSpan(8)) << 32 |
            (ulong)U16(context, 12) << 16 | U16(context, 14);
        caseSpan?.Validate(endingNode, contexts, knowledgeF5, subwordSourceIndex22ec, extendedMarkers22f8, suppressPrevious2609, cancellationToken);
        if (!evaluation.Accepted) return new(false, default, path, history, evaluation, restartTag, lineage, boundary);
        uint[] payload = new uint[5];
        for (int index = 0; index < 5; index++) payload[index] = BinaryPrimitives.ReadUInt32LittleEndian(evaluation.Record40.Span[(8 + index * 4)..]);
        var fields = new LegacyContinuationWordRecordFields(tag, checked(path + evaluation.Score.LexicalSubtotal), I32(context, 4),
            U16(evaluation.Record40.Span, 24), payload, U64(query, 8), restartTag,
            unchecked((byte)((query[23] >> 5 & 3) + query[22])), (byte)(current[39] & previous[39] & 1), lineage);
        var allocation = LegacyContinuationWordRecordProducer.TryInsert(router, endingNode, currentRowOrdinal,
            (node, ordinal) => LookupBytes(node, ordinal), fields, 0, suppressPrevious2609, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        caseSpan?.Validate(endingNode, contexts, knowledgeF5, subwordSourceIndex22ec, extendedMarkers22f8, suppressPrevious2609, cancellationToken);
        return new(true, allocation, path, history, evaluation, restartTag, lineage, boundary);

        byte[] LookupBytes(int node, ushort ordinal)
        {
            if ((uint)node >= (uint)availableSourceNodes) throw new ArgumentException("Referenced node is outside the owned source graph.");
            if (!ownedRows.TryGetValue(node, out var snapshot))
            {
                var rows = owningNodeRowsLookup(node);
                cancellationToken.ThrowIfCancellationRequested();
                caseSpan?.Validate(endingNode, contexts, knowledgeF5, subwordSourceIndex22ec, extendedMarkers22f8, suppressPrevious2609, cancellationToken);
                if (rows is null || rows.Count > ushort.MaxValue) throw new ArgumentException("Node getter must return its actual bounded rows.");
                snapshot = new byte[rows.Count][];
                for (int index = 0; index < rows.Count; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var row = rows[index];
                    if (row is null || row.Record.Length != 88) throw new ArgumentException("Each owned word ordinal must contain88 bytes.");
                    snapshot[index] = row.Record.ToArray();
                }
                ownedRows.Add(node, snapshot);
            }
            if (ordinal >= snapshot.Length) throw new ArgumentException("Referenced word ordinal is outside its owning node.");
            return snapshot[ordinal];
        }
        LegacyNativeStoredWordRecord LookupRecord(int node, ushort ordinal)
        {
            //History preparation only reads stored88. This internal owned view
            //does not invent alternative pools or materialize another ordinal.
            return new(LookupBytes(node, ordinal), [], []);
        }
        uint Header(byte id)
        {
            uint value = preparedSourceHeader(id);
            cancellationToken.ThrowIfCancellationRequested();
            caseSpan?.ValidateHeader(id, value);
            caseSpan?.Validate(endingNode, contexts, knowledgeF5, subwordSourceIndex22ec, extendedMarkers22f8, suppressPrevious2609, cancellationToken);
            return value;
        }
        int Line(int node, int ordinal)
        {
            int value = descriptorLineE8(node, ordinal);
            cancellationToken.ThrowIfCancellationRequested();
            caseSpan?.Validate(endingNode, contexts, knowledgeF5, subwordSourceIndex22ec, extendedMarkers22f8, suppressPrevious2609, cancellationToken);
            return value;
        }
    }

    private static int I32(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadInt32LittleEndian(bytes[offset..]);
    private static ushort U16(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(bytes[offset..]);
    private static ulong U64(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadUInt64LittleEndian(bytes[offset..]);
}
