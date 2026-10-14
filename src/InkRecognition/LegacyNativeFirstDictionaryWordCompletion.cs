using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyNativeFirstDictionaryWordCompletionResult(bool LanguageAccepted,
    LegacyNativeWordHeapAllocation Allocation, int PreLexicalCost,
    LegacyNativeKnownWordLanguageRecordResult Language, ulong RestartTag28);

/// <summary>Regular204290 first-word branch: owning word/anchor rows and actual
/// terminal query -> source70 -> source50 -> category/cost/budget ->100090 record
/// ->1f9080. This does not synthesize auxiliary queries, later-word context search,
/// scheduler state, geometry frames or a native final winner.</summary>
public sealed class LegacyNativeFirstDictionaryWordCompletion
{
    private readonly LegacyInkAlphabet alphabet;
    private readonly LegacyCursiveCategoryMapper categories;
    private readonly LegacyOrdinaryWordCategoryPolicy categoryPolicy;
    private readonly LegacyPreparedDictionaryWordMetadata metadata;
    private readonly LegacyNativeKnownWordLanguageRecord language;
    private readonly LegacyPreparedDictionaryRestartTags restart;
    private readonly byte sourceId;
    private readonly uint sourceHeader;
    private readonly ushort replacement;
    private bool valid = true;
    private int active;

    /// <summary>204290 local318's current-terminal cache injection. Only its
    /// assigned bytes are overwritten; extra pointer/residue24..31 survives.</summary>
    public static ReadOnlyMemory<byte>? CreateAlreadyTerminalQuery(ReadOnlySpan<byte> storedParent88,
        ReadOnlySpan<byte> queryCacheResidue32, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (storedParent88.Length != 88 || queryCacheResidue32.Length != 32)
            throw new ArgumentException("Actual parent88 and full query-cache residue32 required.");
        if ((storedParent88[5] & 2) == 0 || (storedParent88[7] & 1) != 0) return null;
        byte[] query = queryCacheResidue32.ToArray();
        storedParent88[..8].CopyTo(query);
        query.AsSpan(8, 12).Clear(); //Native context8 and cost10 stores are zero.
        storedParent88.Slice(36, 2).CopyTo(query.AsSpan(20));
        query[22] = query[23] = 0;
        return query;
    }

    public LegacyNativeFirstDictionaryWordCompletion(LegacyRussianModelBundle selectedModel,
        LegacyPreparedDictionaryWordMetadata metadata,
        LegacyNativeKnownWordLanguageRecord language, LegacyPreparedDictionaryRestartTags restart,
        byte selectedSourceId, uint preparedSourceHeader, bool nativeMode22f6,
        float knowledgeScaleEc, bool frontend280, bool trace2368)
    {
        ArgumentNullException.ThrowIfNull(selectedModel);
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(language);
        if (selectedSourceId == 0 || (preparedSourceHeader & 0x1e00) != 0xc00 ||
            (preparedSourceHeader & 0x1c) <= 7 || !metadata.IsForModel(selectedModel) ||
            !language.IsForModel(selectedModel) || metadata.WordCount != language.WordCount)
            throw new ArgumentException("Owned selected-model dictionary/index/language profiles and actual C00 source binding required.");
        if (nativeMode22f6 || !float.IsFinite(knowledgeScaleEc) || knowledgeScaleEc > 0 || frontend280 || trace2368)
            throw new NotSupportedException("Proved first-word mode0, nonpositive ec, ordinary frontend and nontracing branch required.");
        alphabet = selectedModel.AcousticAlphabet;
        categories = selectedModel.CreateCursiveCategoryMapper(1);
        this.metadata = metadata;
        this.language = language;
        this.restart = restart;
        sourceId = selectedSourceId;
        sourceHeader = preparedSourceHeader;
        categoryPolicy = new(categories);
        replacement = alphabet.TryGetLabel("\ufffd", out ushort label) ? label : ushort.MaxValue;
    }

    public LegacyNativeFirstDictionaryWordCompletionResult ProcessCandidate(ReadOnlySpan<byte> query32,
        int endingNode, ushort parentRowOrdinal, ReadOnlySpan<byte> storedParent88,
        LegacyNativeWordHeapRouter router, bool knowledgeF5, ReadOnlySpan<byte> temporaryLexicalResidue40,
        Func<int, ushort, ReadOnlyMemory<byte>> owningRowLookup,
        bool suppressPrevious2609, CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0)
            throw new InvalidOperationException("Native first-word completion calls must be serialized.");
        try
        {
            if (!valid) throw new InvalidOperationException("Failed first-word completion requires a fresh producer.");
            return ProcessCore(query32, endingNode, parentRowOrdinal, storedParent88,
                router, knowledgeF5, temporaryLexicalResidue40, owningRowLookup, suppressPrevious2609, cancellationToken);
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }

    private LegacyNativeFirstDictionaryWordCompletionResult ProcessCore(ReadOnlySpan<byte> query32,
        int endingNode, ushort parentRowOrdinal, ReadOnlySpan<byte> storedParent88,
        LegacyNativeWordHeapRouter router,
        bool knowledgeF5, ReadOnlySpan<byte> temporaryLexicalResidue40,
        Func<int, ushort, ReadOnlyMemory<byte>> owningRowLookup, bool suppressPrevious2609,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(router);
        ArgumentNullException.ThrowIfNull(owningRowLookup);
        cancellationToken.ThrowIfCancellationRequested();
        if (endingNode < 0 || query32.Length != 32 || storedParent88.Length != 88 ||
            temporaryLexicalResidue40.Length != 40 || query32[4] != sourceId || storedParent88[4] != sourceId)
            throw new ArgumentException("Actual first-word owner rows, selected-source query and complete lexical residue required.");
        byte[] querySnapshot = query32.ToArray(), parent = storedParent88.ToArray(), lexicalResidueSnapshot = temporaryLexicalResidue40.ToArray();
        query32 = querySnapshot;
        storedParent88 = parent;
        temporaryLexicalResidue40 = lexicalResidueSnapshot;
        int anchorEnding = BinaryPrimitives.ReadInt32LittleEndian(storedParent88[56..]);
        ushort anchorOrdinal = BinaryPrimitives.ReadUInt16LittleEndian(storedParent88[60..]);
        if (anchorEnding < 0 || anchorEnding > endingNode || anchorOrdinal == ushort.MaxValue)
            throw new NotSupportedException("Actual materialized initial-word anchor required before lookup.");
        var anchor = owningRowLookup(anchorEnding, anchorOrdinal);
        cancellationToken.ThrowIfCancellationRequested();
        if (anchor.Length != 88) throw new InvalidDataException("Native anchor node/ordinal must own a complete stored88 row.");
        byte[] anchorSnapshot = anchor.ToArray();
        ReadOnlySpan<byte> ownedAnchor88 = anchorSnapshot;
        int firstNode = unchecked(anchorEnding - (sbyte)ownedAnchor88[32]);
        if (anchorEnding < 0 || firstNode != 0 || ownedAnchor88[4] != sourceId ||
            BinaryPrimitives.ReadUInt16LittleEndian(ownedAnchor88[24..]) != anchorEnding ||
            (ownedAnchor88[6] & 1) != 0 || (storedParent88[6] & 1) != 0 || (storedParent88[39] & 1) != 0 ||
            (query32[23] & 7) is not (0 or 2) || BinaryPrimitives.ReadUInt64LittleEndian(query32[24..]) != 0)
            throw new NotSupportedException("Regular nonspecial, mode22f6=false, ec<=0 first-word/root anchor branch required.");
        ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(query32[20..]);
        //204290 does not apply a Unicode letter/digit gate. Nonvirtual
        //ordinary AL punctuation uses the same category/resource language
        //path. FFFD/current-force and virtual protocol branches remain outside
        //this bounded first-word producer until their owning policy is supplied.
        if (symbol >= alphabet.LabelCount || alphabet.IsVirtual(symbol) ||
            !alphabet.TryGetText(symbol, out _) || symbol == replacement ||
            BinaryPrimitives.ReadUInt16LittleEndian(storedParent88[36..]) == replacement)
            throw new NotSupportedException("Ordinary nonvirtual AL terminal without replacement-force policy required.");
        var currentCategory = categoryPolicy.Resolve(storedParent88[26], symbol, query32[22],
            checked((ushort)alphabet.LabelCount), query32[23], replacement, false, cancellationToken);
        int previousCategory = categories.GetBeamFirstId(ownedAnchor88[26]);
        //Source C00 excludes the0200..0A00 distance/forced-category branch.
        int correction = LegacyOrdinaryWordCategoryPolicy.GetPreviousCategoryPathPenalty(false, sourceHeader,
            previousCategory, 0, 0);
        int parentCost = BinaryPrimitives.ReadInt32LittleEndian(storedParent88[16..]);
        int queryCost = BinaryPrimitives.ReadInt32LittleEndian(query32[16..]);
        //Preserve the checked scalar scorer's explicit int32 domain instead of
        //pretending a wrapped lexical query is a meaningful safe budget.
        int path = checked(checked(parentCost + queryCost) + correction);
        var ownedPrimaryHeap = router.GetPrimaryHeap(sourceId);
        int limit = ownedPrimaryHeap.Count != 0 && ownedPrimaryHeap.Count == ownedPrimaryHeap.Capacity
            ? BinaryPrimitives.ReadInt32LittleEndian(ownedPrimaryHeap.GetSlotSnapshot(0).Span[16..]) : 0x4000000;
        int lexicalBudget = checked(limit - path);
        ulong tag = BinaryPrimitives.ReadUInt64LittleEndian(query32);
        var token = metadata.ReadTerminal(tag & ~(255UL << 32), 0, cancellationToken);
        var evaluation = language.Evaluate(token.Span, [], currentCategory.WordCategory, knowledgeF5,
            lexicalBudget, temporaryLexicalResidue40, cancellationToken);
        ulong restartTag = restart.Resolve(tag & ~(255UL << 32));
        if ((restartTag & (4UL << 40)) != 0)
            restartTag = (restartTag & ~(255UL << 32)) | (ulong)sourceId << 32;
        //204290 installs candidate kind into restart header05.bits4..6.
        restartTag = (restartTag & ~(0x70UL << 40)) | (ulong)(query32[23] << 4 & 0x70) << 40;
        if (!evaluation.Accepted) return new(false, default, path, evaluation, restartTag);
        uint[] payload = new uint[5];
        for (int i = 0; i < payload.Length; i++)
            payload[i] = BinaryPrimitives.ReadUInt32LittleEndian(evaluation.Record40.Span[(8 + i * 4)..]);
        var fields = new LegacyContinuationWordRecordFields(tag, checked(path + evaluation.Score.LexicalSubtotal),
            BinaryPrimitives.ReadInt32LittleEndian(storedParent88[20..]),
            BinaryPrimitives.ReadUInt16LittleEndian(evaluation.Record40.Span[24..]), payload,
            BinaryPrimitives.ReadUInt64LittleEndian(query32[8..]), restartTag,
            unchecked((byte)((query32[23] >> 5 & 3) + query32[22])), 0,
            (0x4000000UL << 32) | uint.MaxValue);
        var allocation = LegacyContinuationWordRecordProducer.TryInsert(router, endingNode, parentRowOrdinal,
            Lookup, fields, 0, suppressPrevious2609, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return new(true, allocation, path, evaluation, restartTag);

        ReadOnlyMemory<byte> Lookup(int node, ushort ordinal)
        {
            if (node == endingNode && ordinal == parentRowOrdinal) return parent;
            var result = owningRowLookup(node, ordinal);
            cancellationToken.ThrowIfCancellationRequested();
            if (result.Length != 88) throw new InvalidDataException("History lookup must own a complete stored88 row.");
            return result;
        }
    }
}
