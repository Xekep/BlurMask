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
static byte[] Bytes(JsonElement element, string property) => Convert.FromHexString(element.GetProperty(property).GetString()!);
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

int cases = 0, independentParentLookups = 0, mappingCalls = 0;
foreach (var test in document.RootElement.GetProperty("cases").EnumerateArray())
{
    var contexts = new LegacySymbolContextFactory();
    ushort pc = test.GetProperty("pc").GetUInt16(), sc = test.GetProperty("sc").GetUInt16();
    byte[] slot = Bytes(test, "slot"), primary = Bytes(test, "primary"), secondary = Bytes(test, "secondary");
    byte[] expectedSlot = Bytes(test, "expectedSlot"), expectedPrimary = Bytes(test, "expectedPrimary"), expectedSecondary = Bytes(test, "expectedSecondary");
    ulong suffix = Path(contexts, Keys(test.GetProperty("suffix")));
    BinaryPrimitives.WriteUInt64LittleEndian(slot.AsSpan(8), suffix);
    BinaryPrimitives.WriteUInt64LittleEndian(expectedSlot.AsSpan(8), suffix);
    for (int index = 0; index < sc; index++)
        for (int kind = 0; kind < 2; kind++)
        {
            BinaryPrimitives.WriteUInt64LittleEndian(secondary.AsSpan(index * 40 + 8 + kind * 8),
                Path(contexts, Keys(test.GetProperty("prefixes")[index][kind])));
            BinaryPrimitives.WriteUInt64LittleEndian(expectedSecondary.AsSpan(index * 40 + 8 + kind * 8),
                Path(contexts, Keys(test.GetProperty("outputs")[index][kind])));
        }
    var parents = new LegacyNativeStoredWordRecord[3];
    for (ushort arc = 0; arc < 3; arc++)
    {
        var parent = test.GetProperty("parents")[arc];
        byte[] parentPool = Bytes(parent, "secondary");
        for (int index = 0; index < sc; index++)
            for (int kind = 0; kind < 2; kind++)
                BinaryPrimitives.WriteUInt64LittleEndian(parentPool.AsSpan(index * 40 + 8 + kind * 8),
                    Path(contexts, Keys(parent.GetProperty("prefixes")[index][kind])));
        parents[arc] = LegacyNativeWordRowMaterializer.Materialize(Bytes(parent, "slot"), arc, 3, 1, sc, default, parentPool);
    }
    byte[] beforeSlot = slot.ToArray(), beforePrimary = primary.ToArray(), beforeSecondary = secondary.ToArray();
    var actualLookups = new List<(int, ushort)>();
    var actualMapping = new List<ushort>();
    var configuration = LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with
    { PrimaryAlternativeCount2402 = pc, SecondaryAlternativeCount2404 = sc };
    var projected = LegacyNativeSecondaryRecombinationProjection.Project(slot, test.GetProperty("node").GetUInt32(),
        configuration, primary, secondary, (node, arc) =>
        {
            actualLookups.Add((node, arc));
            return parents[arc];
        }, contexts, key => { actualMapping.Add(key); return (ushort)(key ^ 0x55); });
    Require(projected.HeapSlot.Span.SequenceEqual(expectedSlot) && projected.PrimaryStorage.Span.SequenceEqual(expectedPrimary) &&
        projected.SecondaryStorage.Span.SequenceEqual(expectedSecondary), $"Native projection full bytes differ at {cases}.");
    Require(actualLookups.SequenceEqual(test.GetProperty("lookups").EnumerateArray()
        .Select(value => (value[0].GetInt32(), value[1].GetUInt16()))) && actualMapping.SequenceEqual(Keys(test.GetProperty("calls"))),
        "Projection parent ownership or mapping callback order changed.");
    Require(slot.SequenceEqual(beforeSlot) && primary.SequenceEqual(beforePrimary) && secondary.SequenceEqual(beforeSecondary),
        "Projection changed caller-owned heap/pools.");
    independentParentLookups += actualLookups.Count;
    mappingCalls += actualMapping.Count;
    cases++;
}
Console.WriteLine($"PASS {cases} full128 heap/primary/secondary projections / {independentParentLookups} exact parent lookups / {mappingCalls} mapping calls.");

foreach (string cancelledProvider in new[] { "parent", "mapper" })
{
    using var cts = new CancellationTokenSource();
    var guardContexts = new LegacySymbolContextFactory();
    ulong prefix = Path(guardContexts, new ushort[] { 70 });
    ulong suffix = Path(guardContexts, new ushort[] { 65 });
    var guardSlot = new byte[128];
    BinaryPrimitives.WriteUInt64LittleEndian(guardSlot.AsSpan(8), suffix);
    var guardPrimary = new byte[80];
    var guardSecondary = new byte[80];
    BinaryPrimitives.WriteInt32LittleEndian(guardSecondary.AsSpan(40), 0x4000000);
    var parentPool = new byte[80];
    BinaryPrimitives.WriteUInt64LittleEndian(parentPool.AsSpan(8), prefix);
    BinaryPrimitives.WriteUInt64LittleEndian(parentPool.AsSpan(16), prefix);
    var guardParent = LegacyNativeWordRowMaterializer.Materialize(new byte[128], 0, 1, 1, 2, default, parentPool);
    int callbacks = 0, mappings = 0;
    try
    {
        LegacyNativeSecondaryRecombinationProjection.Project(guardSlot, 1, LegacyNativeWordControllerConfiguration.CreateFresh20baa0(),
            guardPrimary, guardSecondary, (node, arc) =>
            {
                Require(node == 0 && arc == 0, "Guard projection fabricated parent lineage.");
                callbacks++;
                if (cancelledProvider == "parent") cts.Cancel();
                return guardParent;
            }, guardContexts, key =>
            {
                mappings++;
                if (cancelledProvider == "mapper") cts.Cancel();
                return key;
            }, cts.Token);
        throw new Exception("Projection callback cancellation was ignored.");
    }
    catch (OperationCanceledException) { }
    Require(callbacks == 1 && mappings == (cancelledProvider == "mapper" ? 1 : 0) && guardContexts.NodeCount == 2 &&
        BinaryPrimitives.ReadUInt64LittleEndian(guardSlot.AsSpan(8)) == suffix && guardPrimary.All(value => value == 0) &&
        BinaryPrimitives.ReadInt32LittleEndian(guardSecondary.AsSpan(40)) == 0x4000000,
        "Cancelled projection changed input storage or inserted a context after cancellation.");
}
try
{
    LegacyNativeSecondaryRecombinationProjection.Project(new byte[128], 1,
        LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with { Trace2368 = 1 }, new byte[80], new byte[80],
        (_, _) => throw new Exception("Trace refusal invoked parent lookup."), new LegacySymbolContextFactory(), key => key);
    throw new Exception("Missing trace tables were accepted by projection.");
}
catch (NotSupportedException) { }

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

var childHeaps = new[] { new LegacyNativeWordRecordHeap(4), new LegacyNativeWordRecordHeap(4) };
var childRouter = new LegacyNativeWordHeapRouter(childHeaps, new byte[] { 0 }, 0, uint.MaxValue);
uint currentNode = (uint)(endingNode + 1);
for (ushort parentArc = 0; parentArc < parentRows.Count; parentArc++)
    foreach (var increment in increments)
    {
        int parentCost = BinaryPrimitives.ReadInt32LittleEndian(parentRows[parentArc].Record.Span[0x10..]);
        ulong suffix = actualContexts.ResolveInitial(increment.Symbol).Identity;
        var fields = new LegacyInitialWordRecordFields(0, suffix, unchecked(parentCost + increment.Cost), increment.Cost,
            increment.Symbol, (ushort)currentNode, 0, 0, (byte)categories.GetSymbolCompositeCategory(increment.Symbol),
            parentArc, 0, 1, 0, 0, 1, 0, (byte)increment.Symbol, 0, 0, 0, 0,
            endingNode, parentArc);
        childRouter.TryInsertInitialRecord(0, false, fields, 1, false,
            (node, arc) => Parent(node, arc).Record);
    }
var children = childHeaps.Select(heap => heap.GetSlotStorageSnapshot().ToArray()).ToArray();
uint nextChildOrdinal = 0;
for (int heap = 0; heap < children.Length; heap++)
{
    var assigned = LegacyNativeWordAlternativeOrdinals.AssignInitial(children[heap], childHeaps[heap].Count, nextChildOrdinal);
    children[heap] = assigned.SlotStorage.ToArray();
    nextChildOrdinal = assigned.NextOrdinal;
}
byte[] childPrimary = Enumerable.Repeat((byte)0xa7, (int)nextChildOrdinal * 80).ToArray();
byte[] childSecondary = Enumerable.Repeat((byte)0x5b, (int)nextChildOrdinal * 80).ToArray();
for (int heap = 0; heap < children.Length; heap++)
{
    var produced = LegacyNativeInitialParentWordAlternativeProducer.Create(children[heap], childHeaps[heap].Count,
        currentNode, actualConfiguration, Parent, actualContexts, key => key, childPrimary, childSecondary);
    childPrimary = produced.PrimaryStorage.ToArray();
    childSecondary = produced.SecondaryStorage.ToArray();
}
var childPrimaryPool = new LegacyNativeWordAlternativeBuffer(2, childPrimary);
var childSecondaryPool = new LegacyNativeWordAlternativeBuffer(2, childSecondary);
var childOrdered = children.Select((storage, heap) => LegacyNativeWordAlternativeOrdering.Prepare(storage,
    childHeaps[heap].Count, false, actualConfiguration, new uint[] { 0 }, false, childPrimaryPool, childSecondaryPool)).ToArray();
var childConfiguration = actualConfiguration with { PrimaryArcLimit230a = (ushort)childOrdered.Sum(result => result.Heap.Count) };
int childArc = 0;
var storedChildren = new List<LegacyNativeStoredWordRecord>();
var storedChildSlots = new List<byte[]>();
foreach (var ordered in childOrdered)
    for (int index = 0; index < ordered.Heap.Count; index++)
    {
        var slot = ordered.Heap.GetSlotSnapshot(index);
        ushort parentArc = BinaryPrimitives.ReadUInt16LittleEndian(slot.Span[0x1e..]);
        var row = LegacyNativeWordRowMaterializer.Materialize(slot.Span, (ushort)childArc++,
            childConfiguration.PrimaryArcLimit230a, 2, 2, childPrimaryPool.GetSnapshot().Span, childSecondaryPool.GetSnapshot().Span);
        storedChildren.Add(row);
        storedChildSlots.Add(slot.ToArray());
        Require(row.PrimaryAlternatives.Length == 80 && row.SecondaryAlternatives.Length == 80,
            "Primary-parent pools did not feed their exact default2/2 materializer blocks.");
        ulong parentIdentity = BinaryPrimitives.ReadUInt64LittleEndian(parentRows[parentArc].PrimaryAlternatives.Span[8..]);
        ulong suffix = BinaryPrimitives.ReadUInt64LittleEndian(slot.Span[8..]);
        var expectedPath = ReadPath(actualContexts, parentIdentity).Concat(ReadPath(actualContexts, suffix));
        Require(ReadPath(actualContexts, BinaryPrimitives.ReadUInt64LittleEndian(row.PrimaryAlternatives.Span[8..])).SequenceEqual(expectedPath),
            "Initial child did not inherit its actual stored parent symbol lineage.");
        Require(BinaryPrimitives.ReadInt32LittleEndian(row.PrimaryAlternatives.Span) == BinaryPrimitives.ReadInt32LittleEndian(slot.Span[0x10..]),
            "Initial parent delta changed the actual neural path cost.");
        Require((row.PrimaryAlternatives.Span[31] & 32) == 0 &&
            (row.SecondaryAlternatives.Span[31] & 32) == (0x5b & 32),
            "Parent branch failed to clear primary bit5/preserve secondary bit5.");
    }
Console.WriteLine($"ACTUAL RU NN/cache ? 6 stored root parents x {increments.Count} acoustic increments ? " +
    $"{nextChildOrdinal} nativeinitialordinals/primary-parent pools ? {childArc} default2/2 stored children; " +
    $"{parentLookups} exact parent lookups. Explicit frames,1f8300 mapping and diagnostic scheduling.");

// Actual resource identities/costs participate in merging; the retained native
// destination residue remains observable when the result is materialized.
int actualMerges = 0, changedBest = 0;
for (int leftIndex = 0; leftIndex < storedChildren.Count; leftIndex++)
    for (int incomingIndex = 0; incomingIndex < storedChildren.Count; incomingIndex++)
    {
        var leftRow = storedChildren[leftIndex];
        byte[] incomingPool = storedChildren[incomingIndex].SecondaryAlternatives.ToArray();
        var merged = LegacyNativeSecondaryRecombinationMerger.Merge(leftRow.SecondaryAlternatives.Span, incomingPool, 2);
        var leftPool = leftRow.SecondaryAlternatives.Span;
        bool firstIncoming = BinaryPrimitives.ReadInt32LittleEndian(incomingPool) <= BinaryPrimitives.ReadInt32LittleEndian(leftPool);
        ReadOnlySpan<byte> best = firstIncoming ? incomingPool.AsSpan(0, 40) : leftPool[..40];
        Require(merged.LeftStorage.Span[..8].SequenceEqual(best[..8]) &&
            merged.LeftStorage.Span[8..16].SequenceEqual(best[8..16]) &&
            merged.LeftStorage.Span[24..28].SequenceEqual(best[24..28]) && merged.LeftStorage.Span[30] == best[30],
            "Actual neural merge changed first cost/increment/context/lineage or count.");
        for (int index = 0; index < 2; index++)
        {
            var original = leftPool.Slice(index * 40, 40);
            var result = merged.LeftStorage.Span.Slice(index * 40, 40);
            Require(result[16..24].SequenceEqual(original[16..24]) && result[28..30].SequenceEqual(original[28..30]) &&
                result[31..].SequenceEqual(original[31..]), "Actual recombination replaced raw context/state/residue before final refresh.");
        }
        var ordinal = LegacyNativeWordAlternativeOrdinals.AssignInitial(storedChildSlots[leftIndex], 1, 0);
        var materialized = LegacyNativeWordRowMaterializer.Materialize(ordinal.SlotStorage.Span[..128], 0,
            actualConfiguration.PrimaryArcLimit230a, 2, 2, leftRow.PrimaryAlternatives.Span, merged.LeftStorage.Span);
        Require(materialized.SecondaryAlternatives.Span.SequenceEqual(merged.LeftStorage.Span) &&
            materialized.PrimaryAlternatives.Span.SequenceEqual(leftRow.PrimaryAlternatives.Span) &&
            materialized.Record.Span.SequenceEqual(leftRow.Record.Span),
            "Actual secondary merge did not feed owned native row materialization intact.");
        changedBest += !merged.LeftStorage.Span[..16].SequenceEqual(leftPool[..16]) ? 1 : 0;
        actualMerges++;
    }
Require(changedBest > 0, "Actual resource control never changed the best native secondary alternative.");
Console.WriteLine($"ACTUAL RU NN/cache → 6 owning stored children → {actualMerges} secondary recombinations ({changedBest} best changes) " +
    "→ exact native row materialization PASS. Eligibility, incoming-cost production, heap-field/context refresh and final winner remain separate.");

int actualProjections = 0, actualParentContexts = 0;
for (int leftIndex = 0; leftIndex < storedChildren.Count; leftIndex++)
    for (int incomingIndex = 0; incomingIndex < storedChildren.Count; incomingIndex++)
    {
        var leftRow = storedChildren[leftIndex];
        var merged = LegacyNativeSecondaryRecombinationMerger.Merge(leftRow.SecondaryAlternatives.Span,
            storedChildren[incomingIndex].SecondaryAlternatives.ToArray(), 2);
        int lookups = 0;
        var projected = LegacyNativeSecondaryRecombinationProjection.Project(storedChildSlots[leftIndex], currentNode,
            actualConfiguration, leftRow.PrimaryAlternatives.Span, merged.LeftStorage.Span, (node, arc) =>
            {
                Require(node == endingNode && arc < parentRows.Count, "Actual refreshed alternative queried a fabricated parent.");
                lookups++;
                return parentRows[arc];
            }, actualContexts, key => key);
        Require(BinaryPrimitives.ReadInt32LittleEndian(projected.HeapSlot.Span[16..]) ==
                BinaryPrimitives.ReadInt32LittleEndian(merged.LeftStorage.Span) &&
            BinaryPrimitives.ReadInt32LittleEndian(projected.HeapSlot.Span[20..]) ==
                BinaryPrimitives.ReadInt32LittleEndian(merged.LeftStorage.Span[4..]) &&
            projected.HeapSlot.Span[38] == merged.LeftStorage.Span[30] &&
            BinaryPrimitives.ReadUInt16LittleEndian(projected.HeapSlot.Span[30..]) ==
                BinaryPrimitives.ReadUInt16LittleEndian(merged.LeftStorage.Span[24..]),
            "Actual projected heap fields lost selected secondary provenance.");
        ulong currentSuffix = BinaryPrimitives.ReadUInt64LittleEndian(projected.HeapSlot.Span[8..]);
        int expectedLookups = 0;
        for (int index = 0; index < 2; index++)
        {
            var before = merged.LeftStorage.Span.Slice(index * 40, 40);
            var after = projected.SecondaryStorage.Span.Slice(index * 40, 40);
            if (BinaryPrimitives.ReadInt32LittleEndian(before) >= 0x4000000)
            {
                Require(after.SequenceEqual(before), "Unavailable actual secondary alternative was refreshed.");
                continue;
            }
            ushort arc = BinaryPrimitives.ReadUInt16LittleEndian(before[24..]);
            int alternative = before[27] & 0x7f;
            var parentPool = parentRows[arc].SecondaryAlternatives.Span.Slice(alternative * 40, 40);
            var expectedMapped = ReadPath(actualContexts, BinaryPrimitives.ReadUInt64LittleEndian(parentPool[8..]))
                .Concat(ReadPath(actualContexts, currentSuffix));
            var expectedRaw = ReadPath(actualContexts, BinaryPrimitives.ReadUInt64LittleEndian(parentPool[16..]))
                .Concat(ReadPath(actualContexts, currentSuffix));
            Require(ReadPath(actualContexts, BinaryPrimitives.ReadUInt64LittleEndian(after[8..])).SequenceEqual(expectedMapped) &&
                ReadPath(actualContexts, BinaryPrimitives.ReadUInt64LittleEndian(after[16..])).SequenceEqual(expectedRaw) &&
                (after[31] & 0x1f) == (projected.HeapSlot.Span[33] & 0x1f) && after[26] == projected.HeapSlot.Span[32],
                "Actual projection used incoming stale contexts instead of chosen parent + current suffix.");
            expectedLookups++;
        }
        Require(lookups == expectedLookups, "Actual refresh queried unavailable parent alternatives.");
        var assigned = LegacyNativeWordAlternativeOrdinals.AssignInitial(projected.HeapSlot.Span, 1, 0);
        var row = LegacyNativeWordRowMaterializer.Materialize(assigned.SlotStorage.Span[..128], 0,
            actualConfiguration.PrimaryArcLimit230a, 2, 2, projected.PrimaryStorage.Span, projected.SecondaryStorage.Span);
        Require(row.Record.Span.SequenceEqual(projected.HeapSlot.Span[..88]) &&
            row.SecondaryAlternatives.Span.SequenceEqual(projected.SecondaryStorage.Span) &&
            row.PrimaryAlternatives.Span.SequenceEqual(projected.PrimaryStorage.Span),
            "Projected actual heap/pools did not feed native materialization.");
        actualProjections++;
        actualParentContexts += lookups;
    }
Console.WriteLine($"ACTUAL RU NN/cache → 6 stored children → secondary merge → {actualProjections} heap/context projections " +
    $"({actualParentContexts} owning parent contexts) → nativeordinal/materialized rows PASS. " +
    "Higher-arc eligibility and adjusted incoming-cost producer remain separate from this exact consumer.");
