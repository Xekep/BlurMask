using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text.Json;
using InkRecognition;

if (args.Length != 5)
    throw new ArgumentException("controls.json acoustic.res lexical.res engine-profile.json separation-profile.json");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int quotaCases = 0, allocations = 0, failures = 0, promotionAttempts = 0, views = 0;
foreach (var test in controls.RootElement.GetProperty("views").EnumerateArray())
{
    var read = LegacyNativeWordHeapQuotaLimits.ReadDescriptorSnapshot(
        Convert.FromBase64String(test.GetProperty("image").GetString()!));
    Require(read == ReadLimits(test.GetProperty("expected")), "Native quota descriptor width/offset differs.");
    views++;
}
foreach (var test in controls.RootElement.GetProperty("cases").EnumerateArray())
{
    var initial = test.GetProperty("initial");
    var descriptors = initial.EnumerateArray().Select(value => new LegacyNativeWordHeapDescriptor(
        Convert.FromBase64String(value.GetProperty("backing").GetString()!), value.GetProperty("cap").GetInt32(),
        value.GetProperty("count").GetInt32(), value.GetProperty("tag").GetUInt64(),
        ReadLimits(value.GetProperty("limits")))).ToArray();
    var requests = test.GetProperty("growth");
    int requestIndex = 0;
    LegacyNativeWordHeapGrowthResult? Grow(int descriptorIndex, LegacyNativeWordHeapGrowthRequest request)
    {
        Require(requestIndex < requests.GetArrayLength(), "Native growth made an extra allocator request.");
        var expected = requests[requestIndex++];
        Require(descriptorIndex == expected.GetProperty("index").GetInt32() &&
            request.RequiredAllocatedSlots == expected.GetProperty("capacity").GetInt32() &&
            request.PreviousStorageTag10 == expected.GetProperty("previousTag").GetUInt64() &&
            request.PreviousStorage.Span.SequenceEqual(Convert.FromBase64String(expected.GetProperty("previous").GetString()!)),
            "Native allocator request index, prior opaque tag or complete retained backing differs.");
        Require(descriptors[descriptorIndex].Capacity == request.RequiredAllocatedSlots,
            "Native descriptor capacity was not written before the allocator call.");
        allocations++;
        if (expected.GetProperty("failed").GetBoolean()) { failures++; return null; }
        return new(Convert.FromBase64String(expected.GetProperty("storage").GetString()!), expected.GetProperty("tag").GetUInt64());
    }
    var attempts = new List<(int Target, ReadOnlyMemory<byte> Record)>();
    var configuration = LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with
    {
        PrimaryArcLimit230a = test.GetProperty("arcLimit").GetUInt16(),
        ExpansionFactor230c = test.GetProperty("factor").GetSingle(),
        PackedSource22e8 = test.GetProperty("flag").GetUInt32()
    };
    bool success = LegacyNativeWordQuotaController.Apply(descriptors, configuration,
        test.GetProperty("share").GetSingle(), test.GetProperty("phase").GetInt32(), Grow,
        observePromotionAttempt: (index, record) => attempts.Add((index, record)));
    Require(success == test.GetProperty("success").GetBoolean() && requestIndex == requests.GetArrayLength(),
        "Native quota result or allocation failure stopping point differs.");
    var expectedDescriptors = test.GetProperty("expected");
    for (int index = 0; index < descriptors.Length; index++)
    {
        var descriptor = descriptors[index];
        var expected = expectedDescriptors[index];
        var expectedBacking = Convert.FromBase64String(expected.GetProperty("backing").GetString()!);
        Require(descriptor.Capacity == expected.GetProperty("cap").GetInt32() &&
            descriptor.Count == expected.GetProperty("count").GetInt32() &&
            descriptor.StorageTag10 == expected.GetProperty("tag").GetUInt64() &&
            descriptor.AllocatedCapacity * 128 == expectedBacking.Length &&
            descriptor.GetBackingStorageSnapshot().Span.SequenceEqual(expectedBacking),
            $"Native quota descriptor {index} capacity/count/backing/provenance differs in case {quotaCases}.");
        if (descriptor.Capacity <= descriptor.AllocatedCapacity)
        {
            var heap = descriptor.GetHeapSnapshot();
            Require(heap.Count == descriptor.Count && heap.Capacity == descriptor.Capacity &&
                heap.GetSlotStorageSnapshot().Span.SequenceEqual(expectedBacking.AsSpan(0, heap.Capacity * 128)),
                "Owned descriptor snapshot did not retain inactive capacity bytes.");
        }
    }
    var expectedAttempts = test.GetProperty("drops");
    Require(attempts.Count == expectedAttempts.GetArrayLength(), "Native promotion callback count differs.");
    for (int index = 0; index < attempts.Count; index++)
        Require(attempts[index].Target == expectedAttempts[index].GetProperty("target").GetInt32() &&
            attempts[index].Record.Span.SequenceEqual(Convert.FromBase64String(expectedAttempts[index].GetProperty("record").GetString()!)),
            "Native corresponding/general bucket promotion or root/incoming fallback identity/order differs.");
    promotionAttempts += attempts.Count;
    quotaCases++;
}

// The narrow frozen-heap addition captures inactive storage and is owned.
var storageFixture = Enumerable.Repeat((byte)0x9a, 4 * 128).ToArray();
var snapshotHeap = new LegacyNativeWordRecordHeap(4, storageFixture);
Require(snapshotHeap.TryAllocate(1, _ => { }), "Snapshot guard failed to allocate.");
var snapshot = snapshotHeap.GetSlotStorageSnapshot();
Require(snapshot.Span[128..].SequenceEqual(storageFixture.AsSpan(128)), "Inactive bytes were inferred as zero.");
Require(MemoryMarshal.TryGetArray(snapshot, out var extracted), "Owned snapshot fixture has no array.");
extracted.Array![extracted.Offset] = 123;
Require(snapshotHeap.GetSlotSnapshot(0).Span[0] == 0, "Full snapshot leaked mutable heap ownership.");

var guardLimits = new LegacyNativeWordHeapQuotaLimits(0, 1, 1, 0, 1, 1, 0);
var guardDescriptor = new LegacyNativeWordHeapDescriptor(snapshotHeap.GetSlotStorageSnapshot().Span, 4, 1, 7, guardLimits);
var guardBacking = guardDescriptor.GetBackingStorageSnapshot();
try
{
    LegacyNativeWordQuotaController.Apply(new[] { guardDescriptor },
        LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with { PrimaryArcLimit230a = 1 }, 0, 0,
        cancellationToken: new CancellationToken(true));
    throw new Exception("Cancelled quota accepted.");
}
catch (OperationCanceledException) { }
Require(guardDescriptor.GetBackingStorageSnapshot().Span.SequenceEqual(guardBacking.Span), "Cancelled quota mutated native storage.");

var unavailableGrowth = new LegacyNativeWordHeapDescriptor(new byte[128], 1, 0, 9, guardLimits);
Require(!unavailableGrowth.Resize(2, 0, (_, _) => null) && unavailableGrowth.Capacity == 2 &&
    unavailableGrowth.AllocatedCapacity == 1 && unavailableGrowth.StorageTag10 == 9,
    "Allocation failure rolled back native prewritten capacity or changed storage provenance.");
try
{
    unavailableGrowth.TryInsertRecord(new byte[128]);
    throw new Exception("Incomplete failed native allocation accepted a new record.");
}
catch (InvalidOperationException) { }

var sentinelStorage = new byte[128];
BinaryPrimitives.WriteInt32LittleEndian(sentinelStorage.AsSpan(0x10), int.MinValue);
var sentinels = new[]
{
    new LegacyNativeWordHeapDescriptor(new byte[128], 1, 0, 1, guardLimits),
    new LegacyNativeWordHeapDescriptor(sentinelStorage, 1, 1, 2,
        new(uint.MaxValue, uint.MaxValue, 1, uint.MaxValue, uint.MaxValue, 1, 1))
};
try
{
    LegacyNativeWordQuotaController.Apply(sentinels, LegacyNativeWordControllerConfiguration.CreateFresh20baa0(), 0, 0);
    throw new Exception("Native out-of-range sentinel selection invented a bucket.");
}
catch (NotSupportedException) { }

// Actual RU NN/cache -> alternative merges -> native global quota -> row pools.
var configImage = new byte[0x260a];
BinaryPrimitives.WriteUInt32LittleEndian(configImage.AsSpan(0x2400), 0x00020002);
BinaryPrimitives.WriteUInt16LittleEndian(configImage.AsSpan(0x2404), 2);
BinaryPrimitives.WriteUInt32LittleEndian(configImage.AsSpan(0x230c), 0x3e800000);
BinaryPrimitives.WriteUInt32LittleEndian(configImage.AsSpan(0x2458), uint.MaxValue);
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
// Literal constructor2/2, with an explicit prepared primary arc limit written
// into the controller scalar snapshot. No diagnostic1/1 substitution occurs.
BinaryPrimitives.WriteUInt16LittleEndian(configImage.AsSpan(0x230a), (ushort)continuations.Length);
var preparedConfiguration = LegacyNativeWordControllerConfiguration.ReadPreparedSnapshot(configImage);
int pathCount = continuations.Length * 2;
var pathStorage = new byte[pathCount * 128];
var primaryStorage = new byte[pathCount * preparedConfiguration.PrimaryAlternativeCount2402 * 40];
var secondaryStorage = new byte[pathCount * preparedConfiguration.SecondaryAlternativeCount2404 * 40];
for (int index = 0; index < pathCount; index++)
{
    var path = pathStorage.AsSpan(index * 128, 128);
    continuations[index / 2].Span.CopyTo(path);
    if ((index & 1) != 0)
    {
        BinaryPrimitives.WriteInt32LittleEndian(path[0x10..], BinaryPrimitives.ReadInt32LittleEndian(path[0x10..]) + 1);
        BinaryPrimitives.WriteInt32LittleEndian(path[0x14..], BinaryPrimitives.ReadInt32LittleEndian(path[0x14..]) + 1);
    }
    // The controller assigns ordinal58 before sorting; source pool identities
    // here are explicit opaque NN-symbol keys, not manufactured native pointers.
    BinaryPrimitives.WriteInt32LittleEndian(path[0x58..], index);
    WriteDiagnosticAlternatives(primaryStorage.AsSpan(index * 80, 80), path, primary: true);
    WriteDiagnosticAlternatives(secondaryStorage.AsSpan(index * 80, 80), path, primary: false);
}
var primaryPool = new LegacyNativeWordAlternativeBuffer(preparedConfiguration.PrimaryAlternativeCount2402, primaryStorage);
var secondaryPool = new LegacyNativeWordAlternativeBuffer(preparedConfiguration.SecondaryAlternativeCount2404, secondaryStorage);
var merged = LegacyNativeWordAlternativeOrdering.Prepare(pathStorage, pathCount, true,
    preparedConfiguration, new uint[] { 0 }, false, primaryPool, secondaryPool);
Require(merged.UniqueCount == continuations.Length && merged.Heap.Count == continuations.Length,
    "Default2/2 actual paths did not merge their duplicate alternative identities.");
var alternativeRetainedCosts = continuations.Select(path => BinaryPrimitives.ReadInt32LittleEndian(path.Span[0x10..])).Order();
Require(Enumerable.Range(0, merged.Heap.Count).Select(index =>
    BinaryPrimitives.ReadInt32LittleEndian(merged.Heap.GetSlotSnapshot(index).Span[0x10..])).Order()
    .SequenceEqual(alternativeRetainedCosts), "Alternative-cost producer/scalar updater changed actual best path costs.");
var quotaConfiguration = preparedConfiguration with { PrimaryArcLimit230a = 3 };
// Literal general bucket sentinels from1fbb90; allocation sizes remain explicit
// prepared fixture values. Whole quotas are computed by212ec0, not ReduceCapacity.
var generalLimits = new LegacyNativeWordHeapQuotaLimits(uint.MaxValue, uint.MaxValue,
    (uint)merged.Heap.Capacity, uint.MaxValue, uint.MaxValue, 8, -1);
var actualDescriptor = new LegacyNativeWordHeapDescriptor(merged.Heap.GetSlotStorageSnapshot().Span,
    merged.Heap.Capacity, merged.Heap.Count, 0x12345678, generalLimits);
Require(LegacyNativeWordQuotaController.Apply(new[] { actualDescriptor }, quotaConfiguration, 0, 0),
    "Actual native global general quota failed.");
var pruned = actualDescriptor.GetHeapSnapshot();
Require(pruned.Count == 3 && pruned.Capacity == 3 && actualDescriptor.AllocatedCapacity == merged.Heap.Capacity &&
    Enumerable.Range(0, pruned.Count).Select(index => BinaryPrimitives.ReadInt32LittleEndian(
        pruned.GetSlotSnapshot(index).Span[0x10..])).Order()
    .SequenceEqual(alternativeRetainedCosts.Take(3)),
    "Native global quota did not preserve best actual costs or retained backing allocation.");
var storedRows = Enumerable.Range(0, pruned.Count).Select(index =>
    LegacyNativeWordRowMaterializer.Materialize(pruned.GetSlotSnapshot(index).Span, (ushort)index,
        quotaConfiguration.PrimaryArcLimit230a, preparedConfiguration.PrimaryAlternativeCount2402,
        preparedConfiguration.SecondaryAlternativeCount2404,
        merged.PrimaryAlternativeStorage.Span, merged.SecondaryAlternativeStorage.Span)).ToArray();
foreach (var stored in storedRows)
{
    Require(stored.PrimaryAlternatives.Length == 80 && stored.SecondaryAlternatives.Length == 80,
        "Default2/2 materializer did not consume the owned native alternative pools.");
    int rootNode = BinaryPrimitives.ReadInt32LittleEndian(stored.Record.Span[0x38..]);
    ushort rootArc = BinaryPrimitives.ReadUInt16LittleEndian(stored.Record.Span[0x3c..]);
    Require(rootNode == endingNode && rootArc < rows.Length &&
        BinaryPrimitives.ReadInt32LittleEndian(stored.Record.Span[0x10..]) ==
            BinaryPrimitives.ReadInt32LittleEndian(stored.SecondaryAlternatives.Span),
        "Secondary best alternative or native root lineage failed to reach the stored row.");
    Require((stored.PrimaryAlternatives.Span[31] & 0x20) != 0,
        "Primary special provenance bit5 did not reach the stored alternative block.");
}
int thirdCalls = 0, nextNode = endingNode + 1;
ReadOnlyMemory<byte> ReadStored(int node, ushort arc)
{
    thirdCalls++;
    if (node == nextNode && arc < storedRows.Length) return storedRows[arc].Record;
    if (node == endingNode && arc < rows.Length) return rows[arc];
    throw new InvalidDataException("Default2/2 stored continuation requested an invented root row.");
}
var thirdHeaps = new[] { new LegacyNativeWordRecordHeap(1), new LegacyNativeWordRecordHeap(1) };
var thirdRouter = new LegacyNativeWordHeapRouter(thirdHeaps, new byte[] { 0 }, 0, uint.MaxValue);
int thirdCost = BinaryPrimitives.ReadInt32LittleEndian(storedRows[0].Record.Span[0x10..]) + increments[0].Cost;
var third = LegacyContinuationWordRecordProducer.TryInsert(thirdRouter, nextNode, 0, ReadStored,
    new LegacyContinuationWordRecordFields(0, thirdCost, increments[0].Cost, increments[0].Symbol,
        new uint[5], 0, 0, 0, 0, 0), 0, false);
Require(third.Accepted && thirdCalls == 2 &&
    BinaryPrimitives.ReadInt32LittleEndian(thirdHeaps[0].GetSlotSnapshot(0).Span[0x68..]) == -1,
    "Alternative merge/materializer did not feed the next continuation's exact root anchor.");
Console.WriteLine($"ACTUAL default2/2 RU NN/cache/alternative pool -> 212ec0 globalquota -> {pruned.Count} stored rows " +
    "with80byteprimary/80bytesecondary blocks -> exact-root next continuation. Budget3/generalbucket/storage tag " +
    "and alternative creation are explicit prepared diagnostics; no native winner/confidence claim.");
Console.WriteLine($"PASS {quotaCases} exact global quota streams/{allocations} explicit growth requests/" +
    $"{failures} native allocation failures/{promotionAttempts} full-record promotion attempts; {views} descriptor views, " +
    "positive/nonpositive shares, both phases, sentinel-root/tie order, logical/backing capacities, growth bytes/tags and ownership/cancel guards.");

static void WriteDiagnosticAlternatives(Span<byte> row, ReadOnlySpan<byte> path, bool primary)
{
    row.Clear();
    BinaryPrimitives.WriteInt32LittleEndian(row, BinaryPrimitives.ReadInt32LittleEndian(path[0x10..]));
    BinaryPrimitives.WriteInt32LittleEndian(row[4..], BinaryPrimitives.ReadInt32LittleEndian(path[0x14..]));
    BinaryPrimitives.WriteUInt64LittleEndian(row[8..], BinaryPrimitives.ReadUInt64LittleEndian(path[0x28..]));
    BinaryPrimitives.WriteUInt64LittleEndian(row[16..], BinaryPrimitives.ReadUInt64LittleEndian(path[8..]));
    BinaryPrimitives.WriteUInt16LittleEndian(row[24..], BinaryPrimitives.ReadUInt16LittleEndian(path[0x1e..]));
    row[26] = path[0x20];
    row[30] = path[0x26];
    row[31] = (byte)((path[0x21] & 31) | (primary ? 32 : 0));
    BinaryPrimitives.WriteUInt16LittleEndian(row[32..], BinaryPrimitives.ReadUInt16LittleEndian(path[0x3e..]));
    BinaryPrimitives.WriteInt32LittleEndian(row[40..], 0x4000000);
}


static LegacyNativeWordHeapQuotaLimits ReadLimits(JsonElement limits) => new(
    limits[0].GetUInt32(), limits[1].GetUInt32(), limits[2].GetUInt32(), limits[3].GetUInt32(),
    limits[4].GetUInt32(), limits[5].GetUInt32(), limits[6].GetInt32());

static void Require(bool value, string message)
{
    if (!value) throw new InvalidDataException(message);
}
