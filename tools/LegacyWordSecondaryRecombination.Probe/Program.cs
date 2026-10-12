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

int cases = 0, aliasSensitive = 0;
foreach (var test in document.RootElement.GetProperty("cases").EnumerateArray())
{
    byte[] left = Convert.FromHexString(test.GetProperty("left").GetString()!),
        right = Convert.FromHexString(test.GetProperty("right").GetString()!);
    byte[] beforeLeft = left.ToArray(), beforeRight = right.ToArray();
    var result = LegacyNativeSecondaryRecombinationMerger.Merge(left, right, test.GetProperty("count").GetUInt16());
    Require(result.LeftStorage.Span.SequenceEqual(Convert.FromHexString(test.GetProperty("expected").GetString()!)) &&
        result.WrittenCount == test.GetProperty("written").GetInt32(),
        $"Literal 1f87c0 full-byte reverse-write control differs at {cases}.");
    Require(left.SequenceEqual(beforeLeft) && right.SequenceEqual(beforeRight), "Merge changed caller-owned buffers.");
    if (test.GetProperty("aliasSensitive").GetBoolean()) aliasSensitive++;
    cases++;
}
Require(aliasSensitive == document.RootElement.GetProperty("liveAliasDifferences").GetInt32(), "Live left-alias cases disappeared.");
var guardStorage = new byte[80];
try
{
    LegacyNativeSecondaryRecombinationMerger.Merge(guardStorage, guardStorage, 2);
    throw new Exception("Aliased incoming native temporary was accepted.");
}
catch (NotSupportedException) { }
try
{
    LegacyNativeSecondaryRecombinationMerger.Merge(guardStorage, new byte[80], 2, tracingEnabled2368: true);
    throw new Exception("Missing native trace side tables were accepted.");
}
catch (NotSupportedException) { }
try
{
    LegacyNativeSecondaryRecombinationMerger.Merge(guardStorage, new byte[80], 2, new CancellationToken(true));
    throw new Exception("Cancelled secondary merge was accepted.");
}
catch (OperationCanceledException) { }
Console.WriteLine($"PASS {cases} complete secondary block controls / {aliasSensitive} live reverse-alias cases / trace, overlap and cancellation guards.");

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
