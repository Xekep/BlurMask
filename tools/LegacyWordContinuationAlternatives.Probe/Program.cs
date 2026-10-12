using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if (args.Length != 5)
    throw new ArgumentException("controls.json AK LK engine.json separation.json required");
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0]));
static void Require(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
static ushort[] Keys(JsonElement value) => value.EnumerateArray().Select(item => item.GetUInt16()).ToArray();
static ulong Path(LegacySymbolContextFactory factory, ReadOnlySpan<ushort> keys)
{
    ulong current = 0;
    foreach (ushort key in keys)
    {
        var resolved = current == 0 ? factory.ResolveInitial(key) : factory.ResolveChild(current, key);
        if (resolved.Created && current != 0) factory.LinkCreatedContext(resolved.Identity, current);
        current = resolved.Identity;
    }
    return current;
}
static ushort[] ReadPath(LegacySymbolContextFactory factory, ulong identity)
{
    var reversed = new List<ushort>();
    while (identity != 0)
    {
        var node = factory.ReadNode(identity);
        reversed.Add(node.Symbol);
        identity = node.ParentLink;
    }
    reversed.Reverse();
    return reversed.ToArray();
}


int ordinalCount = 0;
foreach (var test in document.RootElement.GetProperty("ordinals").EnumerateArray())
{
    byte[] input = Convert.FromHexString(test.GetProperty("storage").GetString()!);
    uint first = test.GetProperty("first").GetUInt32();
    int count = test.GetProperty("n").GetInt32();
    var result = test.GetProperty("cont").GetBoolean()
        ? LegacyNativeWordAlternativeOrdinals.AssignContinuation(input, count, (ushort)first)
        : LegacyNativeWordAlternativeOrdinals.AssignInitial(input, count, first);
    Require(result.SlotStorage.Span.SequenceEqual(Convert.FromHexString(test.GetProperty("expected").GetString()!)) &&
        result.NextOrdinal == test.GetProperty("next").GetUInt32(), $"Full ordinal bytes/next cursor differ at {ordinalCount}.");
    ordinalCount++;
}

int poolCount = 0, prepassCount = 0, failureCount = 0, lookupCount = 0;
foreach (var test in document.RootElement.GetProperty("cases").EnumerateArray())
{
    var contexts = new LegacySymbolContextFactory();
    int count = test.GetProperty("count").GetInt32(), n = test.GetProperty("n").GetInt32();
    byte[] slots = Convert.FromHexString(test.GetProperty("slots").GetString()!);
    byte[] pool = Convert.FromHexString(test.GetProperty("pool").GetString()!);
    byte[] expected = Convert.FromHexString(test.GetProperty("expected").GetString()!);
    var parents = new LegacyNativeStoredWordRecord[3];
    var parentAlternatives = new byte[3][];
    for (int index = 0; index < parents.Length; index++)
    {
        var data = test.GetProperty("parents")[index];
        byte[] record = Convert.FromHexString(data.GetProperty("record").GetString()!);
        byte[] alternatives = Convert.FromHexString(data.GetProperty("alts").GetString()!);
        for (int k = 0; k < count; k++)
        {
            var prefixes = test.GetProperty("prefixes")[index][k];
            BinaryPrimitives.WriteUInt64LittleEndian(alternatives.AsSpan(k * 40 + 8), Path(contexts, Keys(prefixes[0])));
            BinaryPrimitives.WriteUInt64LittleEndian(alternatives.AsSpan(k * 40 + 16), Path(contexts, Keys(prefixes[1])));
        }
        parentAlternatives[index] = alternatives;
        parents[index] = LegacyNativeWordRowMaterializer.Materialize(record, (ushort)index,
            0, 1, (ushort)count, default, alternatives);
    }
    bool succeeds = test.GetProperty("succeeds").GetBoolean();
    for (int index = 0; index < n; index++)
    {
        var slot = slots.AsSpan(index * 128, 128);
        ulong suffix = Path(contexts, Keys(test.GetProperty("suffixes")[index]));
        BinaryPrimitives.WriteUInt64LittleEndian(slot[8..], suffix);
        int parent = BinaryPrimitives.ReadUInt16LittleEndian(slot[0x1e..]);
        int ordinal = BinaryPrimitives.ReadInt32LittleEndian(slot[0x58..]);
        if (!succeeds || count == 1) continue;
        for (int k = 0; k < count; k++)
        {
            if (BinaryPrimitives.ReadInt32LittleEndian(parentAlternatives[parent].AsSpan(k * 40)) > 0x3ffffff)
                break;
            ulong mapped = Path(contexts, Keys(test.GetProperty("mapped")[index][k]));
            ulong rawIdentity = Path(contexts, Keys(test.GetProperty("raw")[index][k]));
            int at = (ordinal * count + k) * 40;
            BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(at + 8), mapped);
            BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(at + 16), rawIdentity);
        }
    }
    var configuration = LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with
    {
        SecondaryAlternativeCount2404 = (ushort)count,
        PackedSource22e8 = test.GetProperty("packed").GetUInt32()
    };
    uint heap = test.GetProperty("heap").GetUInt32(), headerValue = test.GetProperty("header").GetUInt32();
    int sourceIndex = test.GetProperty("sourceindex").GetInt32(), node = test.GetProperty("node").GetInt32();
    bool prepass = test.GetProperty("prepass").GetBoolean(), didPrepass = false;
    Require(LegacyNativeContinuationWordAlternativeProducer.RequiresRejectionPrepass(configuration,
        heap, sourceIndex, headerValue) == prepass, "Native prepass predicate differs.");
    byte[] before = slots.ToArray();
    int localLookups = 0;
    LegacyNativeStoredWordRecord Lookup(int actualNode, ushort arc)
    {
        Require(actualNode == node && arc < 3 && (!prepass || didPrepass),
            "Continuation queried invented parent or queried before required prepass.");
        localLookups++;
        return parents[arc];
    }
    bool Prepass() { prepassCount++; didPrepass = true; return succeeds; }
    var produced = LegacyNativeContinuationWordAlternativeProducer.CreateUnanchored(slots, n, node,
        heap, sourceIndex, headerValue, configuration, Lookup, contexts,
        key => (ushort)(key ^ 0x55), pool, Prepass);
    Require(produced.Succeeded == succeeds && produced.SecondaryStorage.Span.SequenceEqual(expected),
        $"Complete continuation pool bytes/failure differ at {poolCount}.");
    Require(didPrepass == prepass && localLookups == (succeeds && count > 1 ? n : 0),
        "Prepass/parent callback invocation count differs.");
    Require(slots.SequenceEqual(before), "Continuation pools changed full128-byte heap records.");
    lookupCount += localLookups;
    if (!succeeds) failureCount++;
    poolCount++;
}

// Exact ushort wrap spans physical heap boundaries. Tail5c..5f and inactive
// storage are retained by the low-dword58 writer.
ushort cursor = 0;
uint written = 0;
for (int heap = 0; heap < 17; heap++)
{
    int count = heap < 16 ? 4096 : 1;
    byte[] storage = Enumerable.Repeat((byte)0xab, count * 128).ToArray();
    var assigned = LegacyNativeWordAlternativeOrdinals.AssignContinuation(storage, count, cursor);
    for (int index = 0; index < count; index++)
    {
        var slot = assigned.SlotStorage.Span.Slice(index * 128, 128);
        Require(BinaryPrimitives.ReadUInt32LittleEndian(slot[0x58..]) == unchecked((ushort)written) &&
            BinaryPrimitives.ReadUInt32LittleEndian(slot[0x5c..]) == 0xabababab,
            "Cross-heap ushort ordinal wrap or adjacent5c bytes differ.");
        written++;
    }
    cursor = (ushort)assigned.NextOrdinal;
}
Require(written == 65537 && cursor == 1, "Native continuation counter did not wrap to zero at65536.");
var guardContexts = new LegacySymbolContextFactory();
var guardSlot = new byte[128];
BinaryPrimitives.WriteUInt16LittleEndian(guardSlot.AsSpan(0x3e), ushort.MaxValue);
int guardCallbacks = 0;
LegacyNativeStoredWordRecord NeverParent(int _, ushort __)
    => throw new Exception("Unexpected parent lookup.");
var guardedConfig = LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with { PackedSource22e8 = 0x10 };
try
{
    LegacyNativeContinuationWordAlternativeProducer.CreateUnanchored(guardSlot, 1, 0,
        0, -1, 0, guardedConfig, NeverParent, guardContexts, key => key, new byte[80]);
    throw new Exception("Missing required prepass invented the rejection policy.");
}
catch (NotSupportedException) { }
try
{
    LegacyNativeContinuationWordAlternativeProducer.CreateUnanchored(guardSlot, 1, 0,
        0, -1, 0, guardedConfig, NeverParent, guardContexts, key => key, new byte[80],
        () => { guardCallbacks++; return true; }, new CancellationToken(true));
    throw new Exception("Pre-cancelled continuation accepted.");
}
catch (OperationCanceledException) { }
Require(guardCallbacks == 0, "Pre-cancellation ran external prepass.");
BinaryPrimitives.WriteUInt16LittleEndian(guardSlot.AsSpan(0x3e), 0);
try
{
    LegacyNativeContinuationWordAlternativeProducer.CreateUnanchored(guardSlot, 1, 0,
        0, -1, 0, guardedConfig, NeverParent, guardContexts, key => key, new byte[80],
        () => { guardCallbacks++; return true; });
    throw new Exception("Unsupported anchored reconstruction accepted.");
}
catch (NotSupportedException) { }
Require(guardCallbacks == 0, "Unsupported anchored branch ran external prepass.");
try
{
    LegacyNativeWordAlternativeOrdinals.AssignInitial(new byte[128], 1, 0, new CancellationToken(true));
    throw new Exception("Cancelled ordinal assignment accepted.");
}
catch (OperationCanceledException) { }
// External preparation may cancel and still return either boolean. Both
// configured counts must observe that cancellation before any early return.
foreach (ushort alternativeCount in new ushort[] { 1, 2 })
    foreach (bool callbackResult in new[] { false, true })
    {
        using var callbackCancellation = new CancellationTokenSource();
        int callbacks = 0;
        BinaryPrimitives.WriteUInt16LittleEndian(guardSlot.AsSpan(0x3e), ushort.MaxValue);
        var callbackConfig = guardedConfig with { SecondaryAlternativeCount2404 = alternativeCount };
        byte[] callbackPool = Enumerable.Repeat((byte)0x5b, alternativeCount * 40).ToArray();
        try
        {
            LegacyNativeContinuationWordAlternativeProducer.CreateUnanchored(guardSlot, 1, 0,
                0, -1, 0, callbackConfig, NeverParent, guardContexts, key => key, callbackPool,
                () =>
                {
                    callbacks++;
                    callbackCancellation.Cancel();
                    return callbackResult;
                }, callbackCancellation.Token);
            throw new Exception("Cancellation inside external prepass bypassed an early return.");
        }
        catch (OperationCanceledException) { }
        Require(callbacks == 1 && guardContexts.NodeCount == 0 &&
            callbackPool.All(value => value == 0x5b),
            "Cancelled external prepass performed lookup/composition or modified caller pool.");
    }

// Required preprocessing owns live parent storage. It must run before each
// subsequent lookup; here it invalidates the parent alternative before copying.
BinaryPrimitives.WriteUInt16LittleEndian(guardSlot.AsSpan(0x3e), ushort.MaxValue);
var parentSlot = new byte[128];
var parentPool = new byte[80];
LegacyNativeStoredWordRecord liveParent = LegacyNativeWordRowMaterializer.Materialize(parentSlot, 0, 0, 1, 2, default, parentPool);
byte[] previousPool = Enumerable.Repeat((byte)0x5b, 80).ToArray();
var liveResult = LegacyNativeContinuationWordAlternativeProducer.CreateUnanchored(guardSlot, 1, 0,
    0, -1, 0, guardedConfig, (_, _) => liveParent, guardContexts, key => key, previousPool,
    () =>
    {
        BinaryPrimitives.WriteInt32LittleEndian(parentPool, 0x4000000);
        liveParent = LegacyNativeWordRowMaterializer.Materialize(parentSlot, 0, 0, 1, 2, default, parentPool);
        return true;
    });
byte[] expectedLive = previousPool.ToArray();
BinaryPrimitives.WriteInt32LittleEndian(expectedLive, 0x4000000);
BinaryPrimitives.WriteInt32LittleEndian(expectedLive.AsSpan(40), 0x4000000);
Require(liveResult.Succeeded && liveResult.SecondaryStorage.Span.SequenceEqual(expectedLive),
    "Continuation copied stale parent bytes before required preprocessing or overwrote unavailable metadata.");

Console.WriteLine($"PASS {ordinalCount} full-byte ordinal views / {poolCount} continuation pools / " +
    $"{prepassCount} rejection prepasses / {failureCount} failures / {lookupCount} exact parent lookups; " +
    "65537 cross-heap ordinal assignments with ushort wrap.");

// Actual RU classifier/cache costs flow through both pool producers.
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
var actualContexts = new LegacySymbolContextFactory();
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
        SourceTag: 0, Auxiliary08: actualContexts.ResolveInitial(symbol).Identity, CandidateCost10: score.TotalCost, IncrementalCost14: score.TotalCost,
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



var initialStorage = actualHeaps.Select(heap => heap.GetSlotStorageSnapshot().ToArray()).ToArray();
var initialCounts = actualHeaps.Select(heap => heap.Count).ToArray();
// One diagnostic duplicate in bucket0 proves the new root pools can feed the
// native initial merge before their stored records become real parent inputs.
initialStorage[0] = initialStorage[0].Concat(new byte[128]).ToArray();
initialStorage[0].AsSpan(0, 128).CopyTo(initialStorage[0].AsSpan(initialCounts[0] * 128));
BinaryPrimitives.WriteInt32LittleEndian(initialStorage[0].AsSpan(initialCounts[0] * 128 + 0x10),
    BinaryPrimitives.ReadInt32LittleEndian(initialStorage[0].AsSpan(0x10)) + 100);
initialCounts[0]++;
uint nextInitial = 0;
for (int heap = 0; heap < initialStorage.Length; heap++)
{
    var assigned = LegacyNativeWordAlternativeOrdinals.AssignInitial(initialStorage[heap], initialCounts[heap], nextInitial);
    initialStorage[heap] = assigned.SlotStorage.ToArray();
    nextInitial = assigned.NextOrdinal;
}
byte[] initialPrimary = Enumerable.Repeat((byte)0xa7, (int)nextInitial * 80).ToArray();
byte[] initialSecondary = Enumerable.Repeat((byte)0x5b, (int)nextInitial * 80).ToArray();
var actualConfiguration = LegacyNativeWordControllerConfiguration.CreateFresh20baa0();
for (int heap = 0; heap < initialStorage.Length; heap++)
{
    var produced = LegacyNativeRootWordAlternativeProducer.Create(initialStorage[heap], initialCounts[heap],
        (uint)endingNode, actualConfiguration, actualContexts, key => key, initialPrimary, initialSecondary);
    initialPrimary = produced.PrimaryStorage.ToArray();
    initialSecondary = produced.SecondaryStorage.ToArray();
}
var initialPrimaryPool = new LegacyNativeWordAlternativeBuffer(2, initialPrimary);
var initialSecondaryPool = new LegacyNativeWordAlternativeBuffer(2, initialSecondary);
var initialOrdered = initialStorage.Select((storage, heap) =>
    LegacyNativeWordAlternativeOrdering.Prepare(storage, initialCounts[heap], false, actualConfiguration,
        new uint[] { 0 }, false, initialPrimaryPool, initialSecondaryPool)).ToArray();
int rootCount = initialOrdered.Sum(result => result.Heap.Count);
actualConfiguration = actualConfiguration with { PrimaryArcLimit230a = (ushort)rootCount };
var parentRows = new List<LegacyNativeStoredWordRecord>();
foreach (var ordered in initialOrdered)
    for (int index = 0; index < ordered.Heap.Count; index++)
        parentRows.Add(LegacyNativeWordRowMaterializer.Materialize(ordered.Heap.GetSlotSnapshot(index).Span,
            (ushort)parentRows.Count, actualConfiguration.PrimaryArcLimit230a, 2, 2,
            initialPrimaryPool.GetSnapshot().Span, initialSecondaryPool.GetSnapshot().Span));
Require(parentRows.Count == 6, "Actual root pools/initial duplicate merge lost expected parents.");
int parentLookups = 0;
LegacyNativeStoredWordRecord Parent(int node, ushort arc)
{
    Require(node == endingNode && arc < parentRows.Count, "Continuation queried fabricated node/arc.");
    parentLookups++;
    return parentRows[arc];
}
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
    if (category < 0) continue;
    var score = nativeScorer.Evaluate(nextScored.Cache.Costs.Span, symbol,
        nextScored.CompositeCategoryCosts.Span, category, 0, 0, LegacyAcousticFixedLog.ImpossibleCost);
    if (score.Accepted) increments.Add((symbol, score.TotalCost));
}
var continuationHeaps = new[] { new LegacyNativeWordRecordHeap(4), new LegacyNativeWordRecordHeap(4) };
var continuationRouter = new LegacyNativeWordHeapRouter(continuationHeaps, new byte[] { 0 }, 0, uint.MaxValue);
for (ushort parentArc = 0; parentArc < parentRows.Count; parentArc++)
    foreach (var increment in increments)
    {
        var parent = parentRows[parentArc];
        int parentCost = BinaryPrimitives.ReadInt32LittleEndian(parent.Record.Span[0x10..]);
        ulong suffix = actualContexts.ResolveInitial(increment.Symbol).Identity;
        ulong parentContext = BinaryPrimitives.ReadUInt64LittleEndian(parent.SecondaryAlternatives.Span[8..]);
        ulong joinedContext = LegacyNativeWordContextComposition.CombineMapped(actualContexts, parentContext, suffix, key => key);
        var fields = new LegacyContinuationWordRecordFields(0, unchecked(parentCost + increment.Cost), increment.Cost,
            ushort.MaxValue, new uint[] { 0, 0, 0, 0, 0 }, suffix, joinedContext,
            (byte)increment.Symbol, 1, 0x1234567800000000UL | parentArc);
        LegacyContinuationWordRecordProducer.TryInsert(continuationRouter, endingNode, parentArc,
            (node, arc) => Parent(node, arc).Record, fields, 1, false);
    }
var continuationStorage = continuationHeaps.Select(heap => heap.GetSlotStorageSnapshot().ToArray()).ToArray();
ushort nextContinuation = 0;
for (int heap = 0; heap < continuationStorage.Length; heap++)
{
    var assigned = LegacyNativeWordAlternativeOrdinals.AssignContinuation(continuationStorage[heap],
        continuationHeaps[heap].Count, nextContinuation);
    continuationStorage[heap] = assigned.SlotStorage.ToArray();
    nextContinuation = (ushort)assigned.NextOrdinal;
}
byte[] continuationSecondary = Enumerable.Repeat((byte)0x5b, nextContinuation * 80).ToArray();
for (uint heap = 0; heap < continuationStorage.Length; heap++)
{
    var produced = LegacyNativeContinuationWordAlternativeProducer.CreateUnanchored(continuationStorage[heap],
        continuationHeaps[heap].Count, endingNode, heap, -1, 0, actualConfiguration,
        Parent, actualContexts, key => key, continuationSecondary);
    Require(produced.Succeeded, "Actual unanchored continuation pool failed.");
    continuationSecondary = produced.SecondaryStorage.ToArray();
}
var continuationPool = new LegacyNativeWordAlternativeBuffer(2, continuationSecondary);
var continuationOrdered = continuationStorage.Select((storage, heap) =>
    LegacyNativeWordAlternativeOrdering.Prepare(storage, continuationHeaps[heap].Count, true,
        actualConfiguration, new uint[] { 0 }, false, null, continuationPool)).ToArray();
int destinationArc = actualConfiguration.PrimaryArcLimit230a;
int continuationRows = 0;
foreach (var ordered in continuationOrdered)
    for (int index = 0; index < ordered.Heap.Count; index++)
    {
        var slot = ordered.Heap.GetSlotSnapshot(index);
        var row = LegacyNativeWordRowMaterializer.Materialize(slot.Span, (ushort)destinationArc++,
            actualConfiguration.PrimaryArcLimit230a, 2, 2, default, continuationPool.GetSnapshot().Span);
        Require(row.PrimaryAlternatives.IsEmpty && row.SecondaryAlternatives.Length == 80,
            "Continuation phase incorrectly required/copied primary alternative rows.");
        var first = row.SecondaryAlternatives.Span[..40];
        Require(BinaryPrimitives.ReadInt32LittleEndian(first) == BinaryPrimitives.ReadInt32LittleEndian(slot.Span[0x10..]),
            "Current-parent delta did not reconstruct the real acoustic path cost.");
        ushort parentArc = BinaryPrimitives.ReadUInt16LittleEndian(slot.Span[0x1e..]);
        ulong parentId = BinaryPrimitives.ReadUInt64LittleEndian(parentRows[parentArc].SecondaryAlternatives.Span[8..]);
        ulong suffix = BinaryPrimitives.ReadUInt64LittleEndian(slot.Span[8..]);
        ushort[] expectedPath = ReadPath(actualContexts, parentId).Concat(ReadPath(actualContexts, suffix)).ToArray();
        ulong actualId = BinaryPrimitives.ReadUInt64LittleEndian(first[8..]);
        Require(ReadPath(actualContexts, actualId).SequenceEqual(expectedPath),
            "Continuation row did not inherit its real stored parent's symbol context lineage.");
        Require(BinaryPrimitives.ReadUInt32LittleEndian(slot.Span[0x5c..]) == (uint)(0x1234567800000000UL | parentArc),
            "Ordinal assignment changed overlapping auxiliary5c field.");
        continuationRows++;
    }
Console.WriteLine($"ACTUAL RU NN/cache ? {nextInitial} native initial ordinals/root pools ? {parentRows.Count} stored parents " +
    $"x {increments.Count} acoustic increments ? {nextContinuation} continuation ordinals/pools ? " +
    $"{continuationRows} stored secondary-only rows; {parentLookups} real parent/history lookups. " +
    "Unanchored scope, explicit1f8300 selection, frames/masks and scheduling remain diagnostics.");
