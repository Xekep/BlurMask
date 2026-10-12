using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 5)
    throw new ArgumentException("controls.json acoustic.res lexical.res engine-profile.json separation-profile.json");

using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int comparisons = 0, orderingStreams = 0, drops = 0;
foreach (var test in controls.RootElement.GetProperty("pairs").EnumerateArray())
{
    var comparer = ReadComparer(test);
    var left = Convert.FromBase64String(test.GetProperty("left").GetString()!);
    var right = Convert.FromBase64String(test.GetProperty("right").GetString()!);
    Require(comparer.Compare(left, right) == test.GetProperty("expected").GetInt32(),
        $"Literal native comparator differs at control {comparisons}, variant {comparer.Comparison}.");
    comparisons++;
}
foreach (var test in controls.RootElement.GetProperty("streams").EnumerateArray())
{
    var comparer = ReadComparer(test);
    var prepared = LegacyNativeWordRecordOrdering.Prepare(
        Convert.FromBase64String(test.GetProperty("storage").GetString()!),
        test.GetProperty("count").GetInt32(), comparer, 1, 1);
    Require(prepared.SortedSlotStorage.Span.SequenceEqual(Convert.FromBase64String(test.GetProperty("sorted").GetString()!)) &&
        prepared.CompactedSlotStorage.Span.SequenceEqual(Convert.FromBase64String(test.GetProperty("compact").GetString()!)) &&
        prepared.UniqueCount == test.GetProperty("unique").GetInt32(),
        "Native quicksort swap order or complete-slot duplicate compaction differs.");
    CheckHeap(prepared.Heap, test.GetProperty("heap"));
    var removed = new List<ReadOnlyMemory<byte>>();
    var pruned = LegacyNativeWordHeapPruning.ReduceCapacity(prepared.Heap,
        test.GetProperty("newCapacity").GetInt32(), removed.Add);
    CheckHeap(pruned, test.GetProperty("pruned"));
    var expectedDropped = test.GetProperty("dropped");
    Require(removed.Count == expectedDropped.GetArrayLength(), "Native resize callback count differs.");
    for (int index = 0; index < removed.Count; index++)
        Require(removed[index].Span.SequenceEqual(Convert.FromBase64String(expectedDropped[index].GetString()!)),
            "Native resize rejected-source versus evicted-root callback identity/order differs.");
    drops += removed.Count;
    orderingStreams++;
}

// Native priority is deliberately not an antisymmetric IComparer ordering.
var specialLeft = new byte[0x80];
var specialRight = new byte[0x80];
specialLeft[4] = 0;
specialRight[4] = 1;
var specialComparer = new LegacyNativeWordRecordComparer(
    LegacyNativeWordRecordComparison.ContinuationFull1fcb50, new uint[] { 0x1400, 0x1400 });
Require(specialComparer.Compare(specialLeft, specialRight) == -1 &&
    specialComparer.Compare(specialRight, specialLeft) == -1,
    "Special differing source IDs were incorrectly repaired into an antisymmetric order.");
Require(new LegacyNativeWordRecordComparer(LegacyNativeWordRecordComparison.ContinuationFull1fcb50,
    new uint[] { 0x1400 }).Compare(specialLeft, specialRight) == -1,
    "Native left-special priority unnecessarily read the unused right source descriptor.");
var payloadLeft = new byte[20];
var payloadRight = new byte[20];
BinaryPrimitives.WriteInt32LittleEndian(payloadLeft.AsSpan(16), int.MinValue);
BinaryPrimitives.WriteInt32LittleEndian(payloadRight.AsSpan(16), int.MaxValue);
Require(LegacyNativeWordRecordComparer.ComparePayload(payloadLeft, 0, payloadRight, 0) == 1 &&
    LegacyNativeWordRecordComparer.ComparePayload(payloadRight, 0, payloadLeft, 0) == -1,
    "Native signed32 subtraction overflow was normalized into mathematical order.");
for (int continuation = 0; continuation < 2; continuation++)
for (int mode = 0; mode < 2; mode++)
for (int flag = 0; flag < 2; flag++)
for (int packed = 0; packed < 256; packed++)
{
    bool history = mode == 0 && (packed & 7) > 1;
    int expected = continuation * 4 + (history ? 2 : 0) + flag;
    Require((int)LegacyNativeWordRecordComparer.Select(continuation != 0, mode != 0, (byte)packed, flag != 0) == expected,
        "2144f0 comparator dispatch differs.");
}

var guardComparer = new LegacyNativeWordRecordComparer(LegacyNativeWordRecordComparison.InitialFull1fc940);
try
{
    LegacyNativeWordRecordOrdering.Prepare(new byte[128], 1, guardComparer, 2, 1);
    throw new Exception("Unported multi-alternative merge accepted.");
}
catch (NotSupportedException) { }
try
{
    LegacyNativeWordRecordOrdering.Prepare(new byte[128], 1, guardComparer, 1, 1, new CancellationToken(true));
    throw new Exception("Cancelled ordering accepted.");
}
catch (OperationCanceledException) { }
var guardHeap = new LegacyNativeWordRecordHeap(1);
Require(guardHeap.TryAllocate(1, _ => { }), "Guard heap did not allocate.");
try
{
    LegacyNativeWordHeapPruning.ReduceCapacity(guardHeap, 0, cancellationToken: new CancellationToken(true));
    throw new Exception("Cancelled reduction accepted.");
}
catch (OperationCanceledException) { }
Require(guardHeap.Count == 1, "Cancelled snapshot operation mutated the source heap.");

// Actual RU composed path retains the existing source/NN/cache lifecycle and
// adds comparator -> native quicksort -> one-alternative dedup -> heap rebuild
// -> exact shrinking and row/anchor consumers.
var acoustic = File.ReadAllBytes(args[1]);
var model = LegacyRussianModelBundle.Load(acoustic, File.ReadAllBytes(args[2]), File.ReadAllBytes(args[3]));
var sourceProfile = LegacyOrdinarySourceInk.ReadV10dResourceProfile(acoustic);
InkStroke Stroke(params (float X, float Y)[] points) =>
    new(points.Select(point => new InkPoint(point.X, point.Y)).ToArray());
var raw = new[]
{
    Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),
    Stroke((70,10),(70,90),(120,10),(120,90)),
    Stroke((140,90),(140,10),(180,10),(190,20),(190,40),(180,50),(140,50))
};
var source = LegacyOrdinaryCursiveCutSource.Generate(raw, sourceProfile, 90, 80);
var seedProfile = LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(acoustic, 189379);
var seeds = LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(source.Cuts, 80, seedProfile,
    new byte[raw.Length], Enumerable.Repeat(-1, raw.Length).ToArray());
var seedGraph = LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds);
var graph = LegacyOrdinaryCursiveCandidateGraph.Generate(seeds, seedGraph, model.CursiveSpanProfile.MaximumAtoms);
int endingNode = graph.NodeCount - 1;
int arcIndex = Enumerable.Range(0, graph.GetNode(endingNode).Count)
    .First(index => graph.GetNode(endingNode)[index].Span == endingNode);
var candidate = graph.GetNode(endingNode)[arcIndex];
var evaluator = new LegacyPreparedWordGroupEvaluator(model, source, new(90, 80, 0, 1));
var glyph = evaluator.Classify(graph.Groups[candidate.GroupId].Ranges.Span);
var cache = new LegacyOrdinaryAcousticClassCache(
    LegacyOrdinaryAcousticCacheProfile.ReadV110Node(acoustic.AsSpan(189734)),
    model.AcousticAlphabet, acoustic.AsSpan(99, 16));
var nativeScorer = LegacyPreparedAcousticCandidateScorer.ForAlphabet(
    LegacyAcousticCandidateThresholds.ReadV110Node(acoustic.AsSpan(189734), out _), model.AcousticAlphabet);
var categories = model.CreateCursiveCategoryMapper(1);
var bridge = new LegacyCursiveFinalCacheGroupScorer(cache, nativeScorer, categories,
    LegacyCursiveSeparationProfile.ReadV106Node(acoustic, 188157),
    LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[4])),
    model.AcousticAlphabet, model.LexicalAlphabet,
    Enumerable.Repeat((byte)1, model.AcousticAlphabet.LabelCount).ToArray(),
    LegacyWideSymbolPairs.CreateUnrestricted(model.AcousticAlphabet), relatedFormMode: 0);
var explicitScope = new LegacyOrdinaryAcousticCacheContext(true, true, true, false, false,
    null, false, ReadOnlyMemory<ushort>.Empty);
var scored = bridge.Score(glyph, source.BindCacheProbability(explicitScope, glyph.Geometry),
    80, 0, false, false, false);
var actualHeaps = new[] { new LegacyNativeWordRecordHeap(4), new LegacyNativeWordRecordHeap(2) };
var actualRouter = new LegacyNativeWordHeapRouter(actualHeaps, new byte[] { 0 }, 0, uint.MaxValue);
var sourceBoundaries = LegacyPreparedSourceBoundarySnapshot.CreateFreshOrdinary(source, graph);
var expectedCosts = new List<int>();
int initializerCount = 0, actualPromotionAttempts = 0, actualPromotions = 0, primaryFallbacks = 0;
LegacyInitialWordRecordFields? firstFields = null;

for (ushort symbol = 0; symbol < model.AcousticAlphabet.LabelCount; symbol++)
{
    int category = categories.GetSymbolCompositeCategory(symbol);
    if (category < 0)
        continue;
    var score = nativeScorer.Evaluate(scored.Cache.Costs.Span, symbol,
        scored.CompositeCategoryCosts.Span, category, 0, 0, LegacyAcousticFixedLog.ImpossibleCost);
    if (!score.Accepted)
        continue;
    var fields = new LegacyInitialWordRecordFields(
        SourceTag: 0, Auxiliary08: 0, CandidateCost10: score.TotalCost, IncrementalCost14: score.TotalCost,
        Label24: symbol, SourceNode18: (ushort)endingNode, PackedSourceArcIndex: (ushort)arcIndex,
        SourceArcIndex1c: (ushort)arcIndex, CompositeCategory: (byte)category,
        PreviousArcIndex1e: ushort.MaxValue, SourceSpan: (sbyte)candidate.Span,
        Count26: 0, HeaderFlag5: 0, HeaderFlag6: 0, RequestedSecondaryFlag: 1,
        PackedKind27: 0, SmallState21: 0, StateBit6: 0, StateBit7: 0,
        Auxiliary30: 0, Auxiliary34: 0, AnchorEndingNode38: endingNode, AnchorArcIndex3c: ushort.MaxValue);
    firstFields ??= fields;
    expectedCosts.Add(score.TotalCost);
    var allocation = actualRouter.TryInsertInitialRecord(0, false, fields, 1, false);
    initializerCount += allocation.Accepted ? 1 : 0;
    actualPromotionAttempts += allocation.PromotionAttempted ? 1 : 0;
    actualPromotions += allocation.PromotionAccepted ? 1 : 0;
    primaryFallbacks += allocation.HeapIndex == 0 ? 1 : 0;
}

// Explicit initial comparator selection; native sort/dedup/rebuild occurs
// within each bucket, before its records become parent rows.
var initialComparer = new LegacyNativeWordRecordComparer(
    LegacyNativeWordRecordComparer.Select(false, false, 0, false), new uint[] { 0 });
for (int heapIndex = 0; heapIndex < actualHeaps.Length; heapIndex++)
{
    var heap = actualHeaps[heapIndex];
    var storage = new byte[heap.Capacity * 128];
    for (int slot = 0; slot < heap.Count; slot++)
        heap.GetSlotSnapshot(slot).Span.CopyTo(storage.AsSpan(slot * 128));
    actualHeaps[heapIndex] = LegacyNativeWordRecordOrdering.Prepare(storage, heap.Count, initialComparer, 1, 1).Heap;
}

var retained = actualHeaps.SelectMany(heap => Enumerable.Range(0, heap.Count).Select(heap.GetSlotSnapshot)).ToArray();
var retainedCosts = retained.Select(record => BinaryPrimitives.ReadInt32LittleEndian(record.Span[0x10..])).Order();
Require(retainedCosts.SequenceEqual(expectedCosts.Order().Take(actualHeaps.Sum(heap => heap.Capacity))),
    "Actual native primary/secondary routed union differs from best-K across both capacities.");
foreach (var record in retained)
{
    ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(record.Span[0x24..]);
    var reference = LegacyPreparedWordPathReference.Read(record.Span[..0x58], endingNode);
    var projection = LegacyWordPathBoundaryCost.Evaluate(reference, categories, graph.NodeCount,
        false, 8, sourceBoundaries.GetBoundary);
    Require(projection.MappedCategory == categories.GetSymbolFirstId(symbol),
        "Routed/promoted native record lost its real resource category/reference.");
    Require((record.Span[0x27] & 1) == 1 &&
        BinaryPrimitives.ReadUInt64LittleEndian(record.Span[0x58..]) == ulong.MaxValue &&
        BinaryPrimitives.ReadInt32LittleEndian(record.Span[0x68..]) == -1 &&
        BinaryPrimitives.ReadUInt64LittleEndian(record.Span[0x70..]) == 0,
        "Native primary fallback/promotion changed secondary flags or root history.");
}
Require(initializerCount > 0 && actualPromotionAttempts > 0 && actualPromotions > 0 && primaryFallbacks > 0,
    "Actual resource stream did not exercise promotion and fallback.");

var rows = retained.Select((record, index) =>
    LegacyNativeWordRowMaterializer.Materialize(record.Span, (ushort)index, (ushort)retained.Length, 1, 1)
        .Record.ToArray()).ToArray();
for (int index = 0; index < rows.Length; index++)
    Require(BinaryPrimitives.ReadUInt16LittleEndian(rows[index].AsSpan(0x3c)) == index,
        "Root row materialization did not replace the missing anchor with its actual stored arc.");
int actualParentLookups = 0, actualHistoryLookups = 0;
ReadOnlyMemory<byte> RowLookup(int node, ushort arc)
{
    Require(node == endingNode && arc < rows.Length, "Continuation requested an invented row or arc.");
    actualHistoryLookups++;
    return rows[arc];
}

// A second actual classification provides explicit acoustic increments. The
// Cartesian parent/increment enumeration is a diagnostic driver, not native
// word-search scheduling or inferred language-state costs.
var nextGlyph = evaluator.Classify(new LegacyInkRange[]
{
    new(2, 0, source.Cuts.PreparedInk.StrokePointCounts.Span[2])
});
var nextScored = bridge.Score(nextGlyph, source.BindCacheProbability(explicitScope, nextGlyph.Geometry),
    80, 0, false, false, false);
var increments = new List<(ushort Symbol, int Cost)>();
for (ushort symbol = 0; symbol < model.AcousticAlphabet.LabelCount; symbol++)
{
    int category = categories.GetSymbolCompositeCategory(symbol);
    if (category < 0)
        continue;
    var score = nativeScorer.Evaluate(nextScored.Cache.Costs.Span, symbol,
        nextScored.CompositeCategoryCosts.Span, category, 0, 0, LegacyAcousticFixedLog.ImpossibleCost);
    if (score.Accepted)
        increments.Add((symbol, score.TotalCost));
}
Require(increments.Count > 0, "Second actual glyph produced no acoustic increment.");

var continuationHeaps = new[] { new LegacyNativeWordRecordHeap(4), new LegacyNativeWordRecordHeap(4) };
var continuationRouter = new LegacyNativeWordHeapRouter(continuationHeaps, new byte[] { 0 }, 0, uint.MaxValue);
var continuationCosts = new List<int>();
int continuationAccepted = 0, continuationPromotions = 0;
for (ushort parentArc = 0; parentArc < rows.Length; parentArc++)
{
    int parentCost = BinaryPrimitives.ReadInt32LittleEndian(rows[parentArc].AsSpan(0x10));
    foreach (var increment in increments)
    {
        int totalCost = unchecked(parentCost + increment.Cost);
        continuationCosts.Add(totalCost);
        var fields = new LegacyContinuationWordRecordFields(
            SourceTag: 0, CandidateCost10: totalCost, IncrementalCost14: increment.Cost,
            State3e: increment.Symbol, Payload40: new uint[] { increment.Symbol, (uint)parentCost,
                (uint)increment.Cost, (uint)endingNode, parentArc },
            Auxiliary08: parentArc, Context28: increment.Symbol, SmallState21: (byte)increment.Symbol,
            RequestedSecondaryFlag: 1, Auxiliary5c: 0x1234567800000000ul | parentArc);
        var allocation = LegacyContinuationWordRecordProducer.TryInsert(continuationRouter, endingNode,
            parentArc, (node, arc) =>
            {
                actualParentLookups++;
                return RowLookup(node, arc);
            }, fields, 1, false);
        continuationAccepted += allocation.Accepted ? 1 : 0;
        continuationPromotions += allocation.PromotionAccepted ? 1 : 0;
    }
}
var continuations = continuationHeaps.SelectMany(heap =>
    Enumerable.Range(0, heap.Count).Select(heap.GetSlotSnapshot)).ToArray();
Require(continuations.Select(record => BinaryPrimitives.ReadInt32LittleEndian(record.Span[0x10..])).Order()
    .SequenceEqual(continuationCosts.Order().Take(continuationHeaps.Sum(heap => heap.Capacity))),
    "Continuation route/promotion lost a better explicit acoustic path.");
foreach (var continuation in continuations)
{
    ushort parentArc = BinaryPrimitives.ReadUInt16LittleEndian(continuation.Span[0x1e..]);
    var parent = rows[parentArc];
    Require(continuation.Span[0x20] == 255 &&
        BinaryPrimitives.ReadInt32LittleEndian(continuation.Span[0x68..]) == -1 &&
        BinaryPrimitives.ReadUInt64LittleEndian(continuation.Span[0x70..]) ==
            0 &&
        BinaryPrimitives.ReadUInt32LittleEndian(continuation.Span[0x58..]) == uint.MaxValue &&
        BinaryPrimitives.ReadUInt64LittleEndian(continuation.Span[0x5c..]) == (0x1234567800000000ul | parentArc),
        "Continuation did not consume the real parent row/history or lost its overlapping opaque 5c field.");
    ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(parent.AsSpan(0x24));
    var projection = LegacyWordPathBoundaryCost.Evaluate(
        LegacyPreparedWordPathReference.Read(continuation.Span[..0x58], endingNode), categories,
        graph.NodeCount, false, 8, sourceBoundaries.GetBoundary);
    Require(projection.MappedCategory == categories.GetSymbolFirstId(symbol),
        "Continuation lost its inherited native category/state reference.");
}
Require(actualParentLookups > continuationCosts.Count && continuations.Length > 0 && continuationPromotions > 0,
    "Actual continuation failed to consume its materialized root anchor or exercise root promotion.");
Console.WriteLine($"ACTUAL {rows.Length} initialized real-resource parent rows x {increments.Count} second-glyph " +
    $"acoustic increments -> {continuationCosts.Count} explicit continuations/{continuationAccepted} allocations/" +
    $"{continuationPromotions} promotions -> {continuations.Length} retained; " +
    $"{actualHistoryLookups} parent/history row lookups, inherited category and exact 5c transport. " +
    "Cartesian scheduling, tag/payload/context inputs and bucket capacities are explicit diagnostics; no native winner claim.");
// Duplicate diagnostic paths reuse actual NN records with an explicit +1 cost
// alternative. This tests native path deduplication without inventing a native
// language prior or claiming the SDK emits that diagnostic duplicate.
var continuationStorage = new byte[continuations.Length * 2 * 128];
for (int index = 0; index < continuations.Length; index++)
{
    continuations[index].Span.CopyTo(continuationStorage.AsSpan(index * 256, 128));
    var duplicate = continuationStorage.AsSpan(index * 256 + 128, 128);
    continuations[index].Span.CopyTo(duplicate);
    BinaryPrimitives.WriteInt32LittleEndian(duplicate[0x10..],
        BinaryPrimitives.ReadInt32LittleEndian(duplicate[0x10..]) + 1);
    BinaryPrimitives.WriteInt32LittleEndian(duplicate[0x14..],
        BinaryPrimitives.ReadInt32LittleEndian(duplicate[0x14..]) + 1);
}
var continuationComparer = new LegacyNativeWordRecordComparer(
    LegacyNativeWordRecordComparer.Select(true, false, 0, false), new uint[] { 0 });
var orderedContinuations = LegacyNativeWordRecordOrdering.Prepare(continuationStorage,
    continuations.Length * 2, continuationComparer, 1, 1);
Require(orderedContinuations.UniqueCount == continuations.Length &&
    orderedContinuations.Heap.Count == continuations.Length,
    "Actual duplicate paths did not collapse to their native equivalence classes.");
var promotedHeap = new LegacyNativeWordRecordHeap(2);
int actualDrops = 0;
var selected = LegacyNativeWordHeapPruning.ReduceCapacity(orderedContinuations.Heap, 3, dropped =>
{
    actualDrops++;
    int cost = BinaryPrimitives.ReadInt32LittleEndian(dropped.Span[0x10..]);
    promotedHeap.TryAllocate(cost, slot => dropped.Span.CopyTo(slot));
});
var expectedSelected = continuations.Select(record =>
    BinaryPrimitives.ReadInt32LittleEndian(record.Span[0x10..])).Order().Take(3);
Require(Enumerable.Range(0, selected.Count).Select(index =>
    BinaryPrimitives.ReadInt32LittleEndian(selected.GetSlotSnapshot(index).Span[0x10..])).Order()
    .SequenceEqual(expectedSelected), "Native comparator/dedup/rebuild/shrink lost a better actual acoustic path.");
Require(actualDrops == continuations.Length - 3 && promotedHeap.Count == 2,
    "Actual pruning failed to transport dropped records into the explicit secondary consumer.");

var selectedRows = Enumerable.Range(0, selected.Count).Select(index =>
    LegacyNativeWordRowMaterializer.Materialize(selected.GetSlotSnapshot(index).Span,
        (ushort)index, (ushort)selected.Count, 1, 1).Record.ToArray()).ToArray();
foreach (var row in selectedRows)
{
    int rootNode = BinaryPrimitives.ReadInt32LittleEndian(row.AsSpan(0x38));
    ushort rootArc = BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(0x3c));
    Require(rootNode == endingNode && rootArc < rows.Length,
        "Native sorting/duplicate swaps/pruning changed the stored root lineage.");
    ushort rootSymbol = BinaryPrimitives.ReadUInt16LittleEndian(rows[rootArc].AsSpan(0x24));
    var projection = LegacyWordPathBoundaryCost.Evaluate(
        LegacyPreparedWordPathReference.Read(row, endingNode + 1), categories, graph.NodeCount + 1,
        false, 8, sourceBoundaries.GetBoundary);
    Require(projection.MappedCategory == categories.GetSymbolFirstId(rootSymbol),
        "Ordered stored row lost its actual resource category source.");
}
int nextNode = endingNode + 1, actualRootLookups = 0;
ReadOnlyMemory<byte> OrderedRows(int node, ushort arc)
{
    actualRootLookups++;
    if (node == nextNode && arc < selectedRows.Length) return selectedRows[arc];
    if (node == endingNode && arc < rows.Length) return rows[arc];
    throw new InvalidDataException("Ordered continuation requested an invented native row.");
}
var thirdHeaps = new[] { new LegacyNativeWordRecordHeap(1), new LegacyNativeWordRecordHeap(1) };
var thirdRouter = new LegacyNativeWordHeapRouter(thirdHeaps, new byte[] { 0 }, 0, uint.MaxValue);
int thirdCost = BinaryPrimitives.ReadInt32LittleEndian(selectedRows[0].AsSpan(0x10)) + increments[0].Cost;
var third = LegacyContinuationWordRecordProducer.TryInsert(thirdRouter, nextNode, 0, OrderedRows,
    new LegacyContinuationWordRecordFields(0, thirdCost, increments[0].Cost, increments[0].Symbol,
        new uint[5], 0, 0, 0, 0, 0), 0, false);
Require(third.Accepted && actualRootLookups == 2 &&
    BinaryPrimitives.ReadInt32LittleEndian(thirdHeaps[0].GetSlotSnapshot(0).Span[0x68..]) == -1,
    "Ordered/pruned rows did not feed the next native continuation's exact original-root anchor.");
Console.WriteLine($"ACTUAL {continuations.Length * 2} real-resource explicit duplicate paths -> " +
    $"{orderedContinuations.UniqueCount} native key groups -> {selected.Count} shrunk records/" +
    $"{actualDrops} dropped callbacks -> {promotedHeap.Count} promoted consumers -> stored rows -> exact-root third continuation. " +
    "Comparator dispatch, +1 duplicate diagnostic prior and capacity3 remain explicit; no native winner/confidence claim.");
Console.WriteLine($"PASS {comparisons} literal comparator controls; {orderingStreams} whole-slot native quicksort/" +
    $"dedup/rebuild/shrink streams/{drops} dropped identities; non-antisymmetric special priority, signed wrap, " +
    "2048 dispatch combinations and refusal/cancel guards. Multi-alternative merges and global212ec0 capacity choice remain separate.");

static LegacyNativeWordRecordComparer ReadComparer(JsonElement test) => new(
    (LegacyNativeWordRecordComparison)test.GetProperty("variant").GetInt32(),
    test.GetProperty("headers").EnumerateArray().Select(value => value.GetUInt32()).ToArray());

static void CheckHeap(LegacyNativeWordRecordHeap heap, JsonElement expected)
{
    Require(heap.Count == expected.GetArrayLength(), "Rebuilt/shrunk native heap count differs.");
    for (int index = 0; index < heap.Count; index++)
        Require(heap.GetSlotSnapshot(index).Span.SequenceEqual(Convert.FromBase64String(expected[index].GetString()!)),
            "Rebuilt/shrunk native heap whole-slot order or ownership differs.");
}

static void Require(bool value, string message)
{
    if (!value) throw new InvalidDataException(message);
}
