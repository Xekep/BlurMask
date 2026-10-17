using System.Buffers.Binary;

namespace InkRecognition;

public sealed record LegacyNativeMainWordFlagPrepassResult(bool Applied, bool Qualified,
    int GeometryCalls, int AcousticCalls, int AcceptedAllocations, ReadOnlyMemory<byte> LastRequest80);

/// <summary>Concrete2136D0 ordinary main source -> count-symbol qualifier ->
/// owned kind5 gap118/140 -> real four-family195 cache128 ->1F8D60 consumer.
/// The actual240 caller's flag/distance policy and selected word rows/headers
/// remain explicit; no case-wrapper factory or native final winner is inferred.</summary>
public sealed class LegacyNativeMainWordFlagPrepass
{
    private readonly AcousticBinding? acoustic;
    private readonly LegacyOrdinaryRecognitionGraph graph;
    private readonly LegacyCursiveCategoryProfile categories;
    private readonly int cacheWidth;
    private readonly LegacyNativeHigherArcTransitionOperations? globalGeometry;
    private readonly Func<int,ushort,CancellationToken,LegacyNativeWordAcousticGroupCosts>? globalCosts;
    private readonly LegacyMainBlankTransitionGeometry? geometry;
    private readonly LegacyNativeWordAcousticRequestScorer scorer;
    private readonly LegacyCursiveSeparationProfile separation;
    private readonly LegacyCursiveSeparationTable? table;
    private readonly Dictionary<int, LegacyNativeWordAcousticGroupCosts> cache = [];
    private readonly LegacyNativeWordTransitionCostCell cell;
    private readonly byte[] residue;
    private bool valid = true;
    private int active;

    public LegacyNativeMainWordFlagPrepass(ReadOnlySpan<byte> actualResource,
        LegacyPreparedFreshMain240AcousticScorer actualAcoustic,LegacyCursiveSeparationTable table,
        LegacyNativeWordTransitionCostCell ownedCostCell,ReadOnlySpan<byte> existingRequestResidue80,
        CancellationToken cancellationToken=default)
        :this(actualResource,AcousticBinding.FromSingle(actualAcoustic),table,ownedCostCell,existingRequestResidue80,cancellationToken) { }

    /// <summary>The same2136D0 path over actual multi-physical main NN/context
    /// caches. Previous rows, caller flags and request residue retain their
    /// explicit ownership; no single-glyph proxy or zero context is created.</summary>
    public LegacyNativeMainWordFlagPrepass(ReadOnlySpan<byte> actualResource,
        LegacyPreparedFreshMainLine240AcousticScorer actualAcoustic,LegacyCursiveSeparationTable table,
        LegacyNativeWordTransitionCostCell ownedCostCell,ReadOnlySpan<byte> existingRequestResidue80,
        CancellationToken cancellationToken=default)
        :this(actualResource,AcousticBinding.FromLine(actualAcoustic),table,ownedCostCell,existingRequestResidue80,cancellationToken) { }

    private LegacyNativeMainWordFlagPrepass(ReadOnlySpan<byte> actualResource,AcousticBinding actualAcoustic,
        LegacyCursiveSeparationTable table,LegacyNativeWordTransitionCostCell ownedCostCell,
        ReadOnlySpan<byte> existingRequestResidue80,CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(ownedCostCell);
        cancellationToken.ThrowIfCancellationRequested();
        if (existingRequestResidue80.Length != 80 ||
            BinaryPrimitives.ReadInt32LittleEndian(existingRequestResidue80[52..]) != 0x4000000)
            throw new ArgumentException("Actual2144F0 reused request80 with initialized limit34=IMP required.");
        acoustic = actualAcoustic;graph=LegacyOrdinaryRecognitionGraph.FromSameSource(actualAcoustic.Graph);
        categories=actualAcoustic.CompositeCategories;cacheWidth=actualAcoustic.CacheWidth;
        this.table = table;
        cell = ownedCostCell;
        residue = existingRequestResidue80.ToArray();
        geometry = actualAcoustic.CreateGeometry(actualResource,table,cancellationToken);
        separation = LegacyCursiveSeparationProfile.ReadV106Node(actualResource,
            actualAcoustic.Categories.SamplerNodeOffset);
        scorer = LegacyNativeWordAcousticRequestScorer.ForAlphabet(
            LegacyAcousticCandidateThresholds.ReadV110Node(actualResource[actualAcoustic.AcousticOffset..], out _),
            actualAcoustic.Alphabet);
    }

    internal LegacyNativeMainWordFlagPrepass(ReadOnlySpan<byte> resource,LegacyOwnedMainWordSourceServices source,
        Func<int,ushort,CancellationToken,LegacyNativeWordAcousticGroupCosts> getOwnedCosts,
        LegacyNativeWordTransitionCostCell cell,ReadOnlySpan<byte> residue,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(source);ArgumentNullException.ThrowIfNull(getOwnedCosts);ArgumentNullException.ThrowIfNull(cell);
        token.ThrowIfCancellationRequested();
        if(!source.IsGlobal||residue.Length!=80||BinaryPrimitives.ReadInt32LittleEndian(residue[52..])!=0x4000000||
            LegacySourceSamplerCategoryOwner.ReadV10dSource(resource,0).SourceFingerprint!=source.Categories.SourceFingerprint)
            throw new ArgumentException("Actual global source, current D0 costs and complete2136D0 residue required.");
        graph=source.Graph;categories=source.Categories.Categories;cacheWidth=source.RequireGlobalSource().Acoustic.CacheWidth;
        globalGeometry=source.GlobalTransitions!.CreateOperations();globalCosts=getOwnedCosts;this.cell=cell;this.residue=residue.ToArray();
        scorer=LegacyNativeWordAcousticRequestScorer.ForAlphabet(
            LegacyAcousticCandidateThresholds.ReadV110Node(resource[source.OrdinaryAcousticOffset..],out _),source.Model.AcousticAlphabet);
    }

    public LegacyNativeMainWordFlagPrepassResult Process(int currentEndingNode, byte currentSpan,
        int previousEndingNode, ushort previousRowOrdinal, Func<int, ushort, ReadOnlyMemory<byte>> owningRowLookup,
        ReadOnlySpan<uint> actualSourceHeaders, LegacySymbolContextFactory contexts, LegacyNativeWordHeapRouter router,
        bool suppressHeader22f7, int knowledgeDistance58, bool suppressPrevious2609,
        CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref active, 1, 0) != 0)
            throw new InvalidOperationException("Native main flag-prepass calls must be serialized.");
        try
        {
            if (!valid) throw new InvalidOperationException("Failed prepass requires a fresh producer.");
            return ProcessCore(currentEndingNode, currentSpan, previousEndingNode, previousRowOrdinal,
                owningRowLookup, actualSourceHeaders, contexts, router,
                suppressHeader22f7, knowledgeDistance58, suppressPrevious2609, cancellationToken);
        }
        catch { valid = false; throw; }
        finally { Volatile.Write(ref active, 0); }
    }

    private LegacyNativeMainWordFlagPrepassResult ProcessCore(int currentEndingNode, byte currentSpan,
        int previousEndingNode, ushort previousRowOrdinal, Func<int, ushort, ReadOnlyMemory<byte>> owningRowLookup,
        ReadOnlySpan<uint> actualSourceHeaders, LegacySymbolContextFactory contexts, LegacyNativeWordHeapRouter router,
        bool suppressHeader22f7, int knowledgeDistance58, bool suppressPrevious2609, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(owningRowLookup);
        ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(router);
        token.ThrowIfCancellationRequested();
        if ((uint)currentEndingNode >= graph.NodeCount || previousEndingNode < 0 ||
            currentSpan > 14 || currentEndingNode - currentSpan - 1 != previousEndingNode ||
            actualSourceHeaders.Length is < 1 or > 256)
            throw new ArgumentException("Owned adjacent main nodes, source headers and current-span qualifier required.");
        uint[] headers = actualSourceHeaders.ToArray();
        var parentMemory = owningRowLookup(previousEndingNode, previousRowOrdinal);
        token.ThrowIfCancellationRequested();
        if (parentMemory.Length != 88) throw new ArgumentException("Actual word ordinal must own its complete88-byte row.");
        byte[] parent = parentMemory.ToArray();
        byte sourceId = parent[4];
        if (sourceId >= headers.Length) throw new ArgumentException("Parent source ID is outside the actual prepared header table.");
        int firstNode = BinaryPrimitives.ReadUInt16LittleEndian(parent.AsSpan(24));
        if (firstNode > previousEndingNode)
            throw new ArgumentException("Parent's first source node must lie in its owned previous source prefix.");
        bool flagged = (parent[5] & 128) != 0;
        if (flagged == suppressHeader22f7 || unchecked(currentEndingNode - firstNode) >= knowledgeDistance58)
            return new(false, false, 0, 0, 0, ReadOnlyMemory<byte>.Empty);
        //D0's ordinary current-span finite union1C0F60: the very same owned
        //195 cache supplies both frontend110 eligibility and later128 costs.
        var spanRows = graph.GetNode(currentEndingNode).Select((arc, ordinal) => (arc, ordinal))
            .Where(item => item.arc.Span == currentSpan)
            .Select(item => GetCosts(currentEndingNode, checked((ushort)item.ordinal), token)).ToArray();
        var qualifier = new LegacyNativeWordSymbolQualifier(cacheWidth - 3, spanRows,
            cancellationToken: token);
        ushort symbol = checked((ushort)qualifier.AcousticLabelCount);
        if (!qualifier.Qualifies(symbol, 0, 0, contexts, token))
            return new(true, false, 0, 0, 0, ReadOnlyMemory<byte>.Empty);
        var arcs = graph.GetSuccessorOrdinals(previousEndingNode,
            BinaryPrimitives.ReadUInt16LittleEndian(parent.AsSpan(28)), currentSpan, token);
        int geometryCalls = 0, acousticCalls = 0, allocations = 0;
        ReadOnlyMemory<byte> last = ReadOnlyMemory<byte>.Empty;
        foreach (int ordinal in arcs.Span)
        {
            token.ThrowIfCancellationRequested();
            ushort arc = checked((ushort)ordinal);
            var request = LegacyNativeWordFlagPrepassRequestFactory.Create(currentEndingNode, currentSpan,
                previousEndingNode, parent, arc, headers[sourceId], categories,
                cell, residue, token);
            geometryCalls++;
            bool accepted = globalGeometry is {} callbacks?callbacks.Calculate118(request,token)&&callbacks.Validate140(request,token):
                geometry!.Calculate118(request,token)&&geometry.Validate140(request,token);
            if (accepted)
            {
                acousticCalls++;
                accepted = scorer.Score(request, (node, arc) => GetCosts(node, arc, token), false, cancellationToken: token);
            }
            last = request.GetSnapshot();
            if (!accepted) continue;
            var fields = new LegacyInitialWordRecordFields(BinaryPrimitives.ReadUInt64LittleEndian(parent),
                BinaryPrimitives.ReadUInt64LittleEndian(parent.AsSpan(8)), cell.Value, request.ReadInt32(48),
                BinaryPrimitives.ReadUInt16LittleEndian(parent.AsSpan(36)), checked((ushort)request.ReadInt32(4)),
                request.ReadUInt16(14), arc, (byte)(parent[26] & 63), previousRowOrdinal, unchecked((sbyte)currentSpan),
                parent[38], 1, 0, 0, 5, 0, 0, 0,
                BinaryPrimitives.ReadInt32LittleEndian(parent.AsSpan(48)), BinaryPrimitives.ReadInt32LittleEndian(parent.AsSpan(52)),
                BinaryPrimitives.ReadInt32LittleEndian(parent.AsSpan(56)), BinaryPrimitives.ReadUInt16LittleEndian(parent.AsSpan(60)));
            var allocation = router.TryInsertInitialRecord(sourceId, false, fields, 0, suppressPrevious2609,
                Lookup, token);
            token.ThrowIfCancellationRequested();
            if (allocation.Accepted) allocations++;
        }
        token.ThrowIfCancellationRequested();
        return new(true, true, geometryCalls, acousticCalls, allocations, last);

        ReadOnlyMemory<byte> Lookup(int node, ushort ordinal)
        {
            if (node == previousEndingNode && ordinal == previousRowOrdinal) return parent;
            var row = owningRowLookup(node, ordinal);
            token.ThrowIfCancellationRequested();
            if (row.Length != 88) throw new ArgumentException("Actual tail lookup must own its complete88-byte row.");
            return row;
        }
    }

    private LegacyNativeWordAcousticGroupCosts GetCosts(int node, ushort ordinal, CancellationToken token)
    {
        if(globalCosts is {} owned)return owned(node,ordinal,token);
        int group = graph.GetNode(node)[ordinal].GroupId;
        if (cache.TryGetValue(group, out var value)) return value;
        token.ThrowIfCancellationRequested();
        var scores = acoustic!.Score(group, token);
        var ranges = acoustic.Graph.Groups[group].Ranges.Span;
        int geometryCost = LegacyCursiveGroupSeparationCost.Evaluate(acoustic.Ink, ranges,
            acoustic.Height, separation.GapOffset, separation.Slope, table!.Values.Span, separation.MaximumCost,
            cancellationToken: token);
        value = new(scores.Cache.Costs.Span,
            Enumerable.Repeat(geometryCost, acoustic.CompositeCategories.CompositeCategories.Length).ToArray());
        cache.Add(group, value);
        return value;
    }

    private sealed record AcousticBinding(LegacyPreparedFreshMain240AcousticScorer? Single,LegacyPreparedFreshMainLine240AcousticScorer? Line)
    {
        public static AcousticBinding FromSingle(LegacyPreparedFreshMain240AcousticScorer acoustic)
        {ArgumentNullException.ThrowIfNull(acoustic);return new(acoustic,null);}
        public static AcousticBinding FromLine(LegacyPreparedFreshMainLine240AcousticScorer acoustic)
        {ArgumentNullException.ThrowIfNull(acoustic);return new(null,acoustic);}
        public LegacyOrdinaryCursiveCandidateGraph Graph=>Single is {} s?s.Source.SourceRanges.Graph:Line!.Source.SourceRanges.Graph;
        public LegacyCursivePreparedInk Ink=>Single is {} s?s.Source.Cuts.PreparedInk:Line!.Source.Cuts.PreparedInk;
        public LegacySourceSamplerCategoryOwner Categories=>Single is {} s?s.Source.Owner.CategoryOwner:Line!.Source.Owner.CategoryOwner;
        public LegacyCursiveCategoryProfile CompositeCategories=>Single is {} s?s.Source.Owner.TransitionProfile.Categories:Line!.Source.Owner.TransitionProfile.Categories;
        public int AcousticOffset=>Single is {} s?s.Source.Owner.AcousticOffset:Line!.Source.Owner.AcousticOffset;
        public LegacyInkAlphabet Alphabet=>Single is {} s?s.Source.Owner.AcousticAlphabet:Line!.Source.Owner.AcousticAlphabet;
        public float Height=>Single is {} s?s.Source.SourceHeight:Line!.Source.Owner.SourceHeight;
        public int CacheWidth=>Single is {} s?s.CacheWidth:Line!.CacheWidth;
        public LegacyPreparedFreshMain240AcousticScores Score(int group,CancellationToken token)
            =>Single is {} s?s.Score(group,token):Line!.Score(group,token);
        public LegacyMainBlankTransitionGeometry CreateGeometry(ReadOnlySpan<byte> resource,LegacyCursiveSeparationTable table,CancellationToken token)
            =>Single is {} s?new(resource,s.Source,table,token):new(resource,Line!.Source,table,token);
    }

}
