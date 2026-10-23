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

int cases = 0, generatedCandidates = 0, comparedRows = 0, headerLookups = 0, higherLookups = 0;
foreach (var test in document.RootElement.GetProperty("cases").EnumerateArray())
{
    var contexts = new LegacySymbolContextFactory();
    ushort pc = test.GetProperty("pc").GetUInt16(), limit = test.GetProperty("limit").GetUInt16();
    var supplied = new List<LegacyNativeOwnedWordRow>();
    var rows = new Dictionary<(int Node, ushort Arc), LegacyNativeStoredWordRecord>();
    var intermediate = new Dictionary<(int, ushort), byte[]>();
    var expectedRows = new Dictionary<(int, ushort), byte[]>();
    foreach (var row in test.GetProperty("rows").EnumerateArray())
    {
        int node = row.GetProperty("node").GetInt32();
        ushort arc = row.GetProperty("arc").GetUInt16();
        byte[] slot = Bytes(row, "slot"), primary = Bytes(row, "primary"), collected = Bytes(row, "collected"), expected = Bytes(row, "expected");
        BinaryPrimitives.WriteUInt64LittleEndian(slot.AsSpan(8), Path(contexts, Keys(row.GetProperty("suffix"))));
        for (int index = 0; index < (arc < limit ? pc : 0); index++)
            for (int kind = 0; kind < 2; kind++)
            {
                ulong original = Path(contexts, Keys(row.GetProperty("prefixes")[index][kind]));
                BinaryPrimitives.WriteUInt64LittleEndian(primary.AsSpan(index * 40 + 8 + kind * 8), original);
                BinaryPrimitives.WriteUInt64LittleEndian(collected.AsSpan(index * 40 + 8 + kind * 8), original);
                BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(index * 40 + 8 + kind * 8),
                    Path(contexts, Keys(row.GetProperty("outputs")[index][kind])));
            }
        var stored = LegacyNativeWordRowMaterializer.Materialize(slot, arc, limit, pc, 2, primary, Bytes(row, "secondary"));
        rows.Add((node, arc), stored);
        if (arc < limit) supplied.Add(new(node, arc, stored));
        else Require(stored.PrimaryAlternatives.Length == 0, "Higher-arc input invented a primary block.");
        intermediate.Add((node, arc), collected);
        expectedRows.Add((node, arc), expected);
    }
    var owner = new LegacyNativePrimaryWordRowOwner(pc, supplied);
    byte[] heap = Bytes(test, "heap"), heapBefore = heap.ToArray();
    var headerCalls = new List<byte>();
    var higherCalls = new List<(int, ushort)>();
    var residueCalls = new List<int>();
    LegacyNativeStoredWordRecord Higher(int node, ushort arc)
    {
        Require(arc >= limit, "Owning higher callback was used for a primary record.");
        higherCalls.Add((node, arc));
        return rows[(node, arc)];
    }
    var collectedResult = LegacyNativeWordRejectionCollector.Collect(heap, test.GetProperty("count").GetInt32(),
        test.GetProperty("node").GetInt32(), limit, owner, contexts,
        source => { headerCalls.Add(source); return test.GetProperty("headers")[source].GetUInt32(); },
        test.GetProperty("mode").GetBoolean(), index =>
        {
            residueCalls.Add(index);
            return Convert.FromHexString(test.GetProperty("residues")[index].GetString()!);
        }, Higher);
    Require(collectedResult.CandidateStorage.Span.SequenceEqual(Bytes(test, "expected")) &&
        collectedResult.CandidateStorage.Length == collectedResult.Count * 36,
        $"Native 210870 candidate fields/residue/order differ at {cases}.");
    Require(headerCalls.SequenceEqual(test.GetProperty("headerCalls").EnumerateArray().Select(value => value.GetByte())),
        "Native source header lookup order changed.");
    Require(higherCalls.SequenceEqual(test.GetProperty("higherCalls").EnumerateArray()
        .Select(value => (value[0].GetInt32(), value[1].GetUInt16()))), "Anchored correction record ownership/order changed.");
    Require(residueCalls.SequenceEqual(Enumerable.Range(0, collectedResult.Count)), "Candidate append residue ownership/order changed.");
    foreach (var suppliedRow in supplied)
        Require(owner.CopyRow(suppliedRow.Node, suppliedRow.Arc).PrimaryAlternatives.Span
            .SequenceEqual(intermediate[(suppliedRow.Node, suppliedRow.Arc)]), "210870 raw-parent invalidation differs.");

    var selected = LegacyNativeRejectionCandidateSelection.Prepare(collectedResult.CandidateStorage.Span, collectedResult.Count, pc);
    Require(selected.SequenceEqual(Bytes(test, "selected")), "Collected candidates did not feed exact native selection.");
    var mapCalls = new List<ushort>();
    LegacyNativePrimaryRejectionPathStitcher.Apply(selected, collectedResult.Count, limit, owner, contexts,
        key => { mapCalls.Add(key); return (ushort)(key ^ 0x55); }, higherArcRowLookup: Higher);
    Require(mapCalls.SequenceEqual(Keys(test.GetProperty("mapCalls"))), "Collected path context mapping order changed.");
    foreach (var pair in rows)
    {
        var actual = pair.Key.Arc < limit ? owner.CopyRow(pair.Key.Node, pair.Key.Arc) : pair.Value;
        Require(actual.PrimaryAlternatives.Span.SequenceEqual(expectedRows[pair.Key]) &&
            actual.Record.Span.SequenceEqual(pair.Value.Record.Span) &&
            actual.SecondaryAlternatives.Span.SequenceEqual(pair.Value.SecondaryAlternatives.Span),
            $"Collector→selection→stitch full-row bytes differ at {cases}, node {pair.Key.Node}, arc {pair.Key.Arc}.");
        comparedRows++;
    }
    var composedOwner = new LegacyNativePrimaryWordRowOwner(pc, supplied);
    var composedMapping = new List<ushort>();
    var composed = LegacyNativeWordRejectionCollector.CollectAndStitch(heap, test.GetProperty("count").GetInt32(),
        test.GetProperty("node").GetInt32(), limit, composedOwner, contexts,
        source => test.GetProperty("headers")[source].GetUInt32(), test.GetProperty("mode").GetBoolean(),
        index => Convert.FromHexString(test.GetProperty("residues")[index].GetString()!),
        key => { composedMapping.Add(key); return (ushort)(key ^ 0x55); }, (node, arc) => rows[(node, arc)]);
    Require(composed.CandidateStorage.Span.SequenceEqual(selected) && composed.Count == collectedResult.Count &&
        composedMapping.SequenceEqual(mapCalls), "Standalone collector-to-stitch composition changed candidates or context calls.");
    foreach (var suppliedRow in supplied)
        Require(composedOwner.CopyRow(suppliedRow.Node, suppliedRow.Arc).PrimaryAlternatives.Span
            .SequenceEqual(expectedRows[(suppliedRow.Node, suppliedRow.Arc)]), "Standalone composed primary storage differs.");
    Require(heap.SequenceEqual(heapBefore), "Collector changed full heap storage.");
    generatedCandidates += collectedResult.Count;
    headerLookups += headerCalls.Count;
    higherLookups += higherCalls.Count;
    cases++;
}
Console.WriteLine($"PASS {cases} complete collector/selection/stitch controls / {generatedCandidates} native36 candidates / " +
    $"{comparedRows} complete stored rows / {headerLookups} source-header calls / {higherLookups} actual higher-row calls.");

// Every external provider must stop immediately on callback cancellation.
foreach (string provider in new[] { "header", "residue", "higher" })
{
    using var cts = new CancellationTokenSource();
    var guardContexts = new LegacySymbolContextFactory();
    var guardParentSlot = new byte[128];
    var guardChildSlot = new byte[128];
    BinaryPrimitives.WriteInt32LittleEndian(guardParentSlot.AsSpan(16), 7);
    BinaryPrimitives.WriteInt32LittleEndian(guardParentSlot.AsSpan(56), -1);
    BinaryPrimitives.WriteInt32LittleEndian(guardChildSlot.AsSpan(16), 55);
    BinaryPrimitives.WriteUInt16LittleEndian(guardChildSlot.AsSpan(30), provider == "higher" ? (ushort)1 : (ushort)0);
    var guardParentPrimary = new byte[80];
    var guardChildPrimary = new byte[80];
    guardChildPrimary[31] = 0x40;
    BinaryPrimitives.WriteInt32LittleEndian(guardChildPrimary.AsSpan(40), 0x4000000);
    BinaryPrimitives.WriteUInt64LittleEndian(guardChildPrimary.AsSpan(16), Path(guardContexts, new ushort[] { 65 }));
    var guardParent = LegacyNativeWordRowMaterializer.Materialize(guardParentSlot, 0, 1, 2, 2, guardParentPrimary, new byte[80]);
    var guardChild = LegacyNativeWordRowMaterializer.Materialize(guardChildSlot, 0, 1, 2, 2, guardChildPrimary, new byte[80]);
    var guardOwner = new LegacyNativePrimaryWordRowOwner(2,
        new[] { new LegacyNativeOwnedWordRow(0, 0, guardParent), new LegacyNativeOwnedWordRow(1, 0, guardChild) });
    var higherSlot = new byte[128];
    var guardHigher = LegacyNativeWordRowMaterializer.Materialize(higherSlot, 1, 1, 2, 2, default, new byte[80]);
    var guardHeap = new byte[128];
    int headers = 0, residues = 0, higher = 0;
    try
    {
        LegacyNativeWordRejectionCollector.Collect(guardHeap, 1, 1, 1, guardOwner, guardContexts,
            _ => { headers++; if (provider == "header") cts.Cancel(); return 0x20; }, false,
            _ => { residues++; if (provider == "residue") cts.Cancel(); return new byte[36]; },
            (_, _) => { higher++; if (provider == "higher") cts.Cancel(); return guardHigher; }, cts.Token);
        throw new Exception("Cancellation inside collector owning callback was ignored.");
    }
    catch (OperationCanceledException) { }
    Require(headers == 1 && residues == (provider == "residue" ? 1 : 0) && higher == (provider == "higher" ? 1 : 0),
        "Collector cancellation crossed an external provider boundary.");
    Require(BinaryPrimitives.ReadInt32LittleEndian(guardOwner.CopyRow(1, 0).PrimaryAlternatives.Span) ==
        (provider == "header" ? 0 : 0x4000000) && BinaryPrimitives.ReadInt32LittleEndian(guardChild.PrimaryAlternatives.Span) == 0,
        "Collector cancellation rolled back live invalidation or changed the input snapshot.");
}

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
foreach (var ordered in childOrdered)
    for (int index = 0; index < ordered.Heap.Count; index++)
    {
        var slot = ordered.Heap.GetSlotSnapshot(index);
        ushort parentArc = BinaryPrimitives.ReadUInt16LittleEndian(slot.Span[0x1e..]);
        var row = LegacyNativeWordRowMaterializer.Materialize(slot.Span, (ushort)childArc++,
            childConfiguration.PrimaryArcLimit230a, 2, 2, childPrimaryPool.GetSnapshot().Span, childSecondaryPool.GetSnapshot().Span);
        storedChildren.Add(row);
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

// The prepared ancestry/correction fields remain explicit. Their consumers
// operate on actual NN-derived stored records and feed another native producer.
// PREPARED inactive storage tail, NOT an NN-produced child. The owning row
// table covers the controller's full primary cap even when fewer children
// survive ordering. Collector210870 reads record16 and stops at Impossible;
// retain the real root cap/parent arc5 rather than shrinking it to childcount5.
var preparedInactiveTail = new List<LegacyNativeOwnedWordRow>();
for (int arc = storedChildren.Count; arc < actualConfiguration.PrimaryArcLimit230a; arc++)
{
    byte[] inactiveSlot = new byte[128], inactiveAlternatives = new byte[80];
    BinaryPrimitives.WriteInt32LittleEndian(inactiveSlot.AsSpan(16), 0x4000000);
    BinaryPrimitives.WriteInt32LittleEndian(inactiveSlot.AsSpan(20), 0x4000000);
    BinaryPrimitives.WriteUInt16LittleEndian(inactiveSlot.AsSpan(0x3c), ushort.MaxValue);
    for (int alternative = 0; alternative < 2; alternative++)
        BinaryPrimitives.WriteInt32LittleEndian(inactiveAlternatives.AsSpan(alternative * 40), 0x4000000);
    var inactive = LegacyNativeWordRowMaterializer.Materialize(inactiveSlot, (ushort)arc,
        actualConfiguration.PrimaryArcLimit230a, 2, 2, inactiveAlternatives, inactiveAlternatives);
    preparedInactiveTail.Add(new((int)currentNode, (ushort)arc, inactive));
}
var realRows = parentRows.Select((row, arc) => new LegacyNativeOwnedWordRow(endingNode, (ushort)arc, row))
    .Concat(storedChildren.Select((row, arc) => new LegacyNativeOwnedWordRow((int)currentNode, (ushort)arc, row)))
    .Concat(preparedInactiveTail).ToArray();
Require(actualConfiguration.PrimaryArcLimit230a == parentRows.Count && parentRows.Count == 6 &&
    realRows.Any(row => row.Node == endingNode && row.Arc == 5) &&
    realRows.Count(row => row.Node == currentNode) == actualConfiguration.PrimaryArcLimit230a,
    "Prepared inactive tail lost the real six-parent cap/parent arc5 or complete current storage.");
foreach (var tail in preparedInactiveTail)
{
    Require(BinaryPrimitives.ReadInt32LittleEndian(tail.Row.Record.Span[16..]) == 0x4000000 &&
        BinaryPrimitives.ReadInt32LittleEndian(tail.Row.Record.Span[20..]) == 0x4000000,
        "Prepared inactive record costs must explicitly terminate the collector scan.");
    for (int alternative = 0; alternative < 2; alternative++)
        Require(BinaryPrimitives.ReadInt32LittleEndian(tail.Row.PrimaryAlternatives.Span[(alternative * 40)..]) == 0x4000000 &&
            BinaryPrimitives.ReadInt32LittleEndian(tail.Row.SecondaryAlternatives.Span[(alternative * 40)..]) == 0x4000000,
            "Prepared inactive alternative tables must remain impossible.");
}
var realOwner = new LegacyNativePrimaryWordRowOwner(2, realRows);
ushort originalParent = BinaryPrimitives.ReadUInt16LittleEndian(storedChildren[0].Record.Span[0x1e..]);
ushort replacementParent = (ushort)((originalParent + 1) % parentRows.Count);
int childRecordCost = BinaryPrimitives.ReadInt32LittleEndian(storedChildren[0].Record.Span[0x10..]);
int originalRecordCost = BinaryPrimitives.ReadInt32LittleEndian(parentRows[originalParent].Record.Span[0x10..]);
int replacementCost = BinaryPrimitives.ReadInt32LittleEndian(parentRows[replacementParent].PrimaryAlternatives.Span);
int correctedPathCost = unchecked(replacementCost + childRecordCost - originalRecordCost);
byte[] realResidue = Enumerable.Repeat((byte)0xd7, 36).ToArray();
byte[] realCandidate = LegacyNativePreparedRejectionProducer.Create(realResidue,
    new(childRecordCost, correctedPathCost, (int)currentNode, (int)currentNode, endingNode,
        0, 0, replacementParent, replacementParent, 0xffff, 0, 0));
var selectedReal = LegacyNativeRejectionCandidateSelection.Prepare(realCandidate, 1, 2);
Require(selectedReal[32] == 1 && selectedReal.AsSpan(33).SequenceEqual(realResidue.AsSpan(33)),
    "Actual prepared rejection was not selected with its stack residue intact.");
LegacyNativePrimaryRejectionPathStitcher.Apply(selectedReal, 1, actualConfiguration.PrimaryArcLimit230a,
    realOwner, actualContexts, key => key);
var stitchedChild = realOwner.CopyRow((int)currentNode, 0);
var stitchedPrimary = stitchedChild.PrimaryAlternatives.Span;
Require(BinaryPrimitives.ReadInt32LittleEndian(stitchedPrimary) == correctedPathCost &&
    BinaryPrimitives.ReadUInt16LittleEndian(stitchedPrimary[0x18..]) == replacementParent &&
    (stitchedPrimary[0x1f] & 0xe0) == 0xc0,
    "Actual selected rejection did not repair cost, lineage or bit40/bit80/cleared bit20.");
ulong replacementPrefix = BinaryPrimitives.ReadUInt64LittleEndian(parentRows[replacementParent].PrimaryAlternatives.Span[8..]);
ulong realSuffix = BinaryPrimitives.ReadUInt64LittleEndian(stitchedChild.Record.Span[8..]);
var expectedRepairedPath = ReadPath(actualContexts, replacementPrefix).Concat(ReadPath(actualContexts, realSuffix));
Require(ReadPath(actualContexts, BinaryPrimitives.ReadUInt64LittleEndian(stitchedPrimary[8..])).SequenceEqual(expectedRepairedPath),
    "Actual rejection repair substituted a fabricated context instead of the replacement stored parent.");
foreach (var row in realRows)
{
    var after = realOwner.CopyRow(row.Node, row.Arc);
    Require(after.Record.Span.SequenceEqual(row.Row.Record.Span) &&
        after.SecondaryAlternatives.Span.SequenceEqual(row.Row.SecondaryAlternatives.Span),
        "Actual rejection altered stored record or secondary pool bytes.");
}

var nextIncrement = increments[0];
int grandchildRecordCost = unchecked(childRecordCost + nextIncrement.Cost);
var grandchildFields = new LegacyInitialWordRecordFields(0, actualContexts.ResolveInitial(nextIncrement.Symbol).Identity,
    grandchildRecordCost, nextIncrement.Cost, nextIncrement.Symbol, (ushort)(currentNode + 1), 0, 0,
    (byte)categories.GetSymbolCompositeCategory(nextIncrement.Symbol), 0, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0,
    (int)currentNode, 0);
var grandchildHeap = new LegacyNativeWordRecordHeap(1);
var grandchildRouter = new LegacyNativeWordHeapRouter(new[] { grandchildHeap }, new byte[] { 0 }, 0, uint.MaxValue);
grandchildRouter.TryInsertInitialRecord(0, false, grandchildFields, 1, false,
    (node, arc) => realOwner.CopyRow(node, arc).Record);
Require(grandchildHeap.Count == 1, "Actual descendant record allocation failed.");
var assignedGrandchild = LegacyNativeWordAlternativeOrdinals.AssignInitial(grandchildHeap.GetSlotStorageSnapshot().Span, 1, 0);
var descendantPools = LegacyNativeInitialParentWordAlternativeProducer.Create(assignedGrandchild.SlotStorage.Span, 1,
    currentNode + 1, actualConfiguration, (node, arc) => realOwner.CopyRow(node, arc), actualContexts, key => key,
    Enumerable.Repeat((byte)0xa7, 80).ToArray(), Enumerable.Repeat((byte)0x5b, 80).ToArray());
var descendant = LegacyNativeWordRowMaterializer.Materialize(assignedGrandchild.SlotStorage.Span[..128], 0,
    actualConfiguration.PrimaryArcLimit230a, 2, 2, descendantPools.PrimaryStorage.Span, descendantPools.SecondaryStorage.Span);
Require(BinaryPrimitives.ReadInt32LittleEndian(descendant.PrimaryAlternatives.Span) ==
    unchecked(correctedPathCost + nextIncrement.Cost), "Actual descendant did not consume repaired primary cost.");
var descendantExpectedPath = expectedRepairedPath.Concat(new[] { nextIncrement.Symbol });
Require(ReadPath(actualContexts, BinaryPrimitives.ReadUInt64LittleEndian(descendant.PrimaryAlternatives.Span[8..]))
    .SequenceEqual(descendantExpectedPath), "Actual descendant did not consume repaired native context lineage.");
Console.WriteLine("ACTUAL RU NN/cache → stored root+child rows → prepared36 append/selection → live primary repair → " +
    "owning repaired parent → next native initial alternatives/materialized descendant PASS. " +
    "Prepared baseline correction is explicit here; the following collector test derives it from rows.");

// Produce the input 1e/3e relationship through the existing continuation
// record writer; do not patch a made-up candidate buffer after allocation.
var rejectionHeap = new LegacyNativeWordRecordHeap(1);
var rejectionRouter = new LegacyNativeWordHeapRouter(new[] { rejectionHeap }, new byte[] { 0 }, 0, uint.MaxValue);
var rejectionFields = new LegacyContinuationWordRecordFields(0, childRecordCost,
    BinaryPrimitives.ReadInt32LittleEndian(stitchedChild.Record.Span[20..]), replacementParent,
    new uint[5], realSuffix, BinaryPrimitives.ReadUInt64LittleEndian(stitchedChild.Record.Span[40..]), 0, 1, 0);
var rejectionAllocation = LegacyContinuationWordRecordProducer.TryInsert(rejectionRouter, (int)currentNode, 0,
    (node, arc) => realOwner.CopyRow(node, arc).Record, rejectionFields, 1, true);
Require(rejectionAllocation.Accepted && rejectionHeap.Count == 1, "Actual rejection continuation record was not allocated.");
int liveSourceHeaderCalls = 0;
var liveCollected = LegacyNativeWordRejectionCollector.Collect(rejectionHeap.GetSlotStorageSnapshot().Span, rejectionHeap.Count,
    (int)currentNode, actualConfiguration.PrimaryArcLimit230a, realOwner, actualContexts,
    _ => { liveSourceHeaderCalls++; return 0x20; }, false, _ => realResidue);
Require(liveSourceHeaderCalls == storedChildren.Count,
    "Collector must stop on the explicit inactive tail before its source-header callback.");
foreach (var tail in preparedInactiveTail)
{
    var unchanged = realOwner.CopyRow(tail.Node, tail.Arc);
    Require(unchanged.Record.Span.SequenceEqual(tail.Row.Record.Span) &&
        unchanged.PrimaryAlternatives.Span.SequenceEqual(tail.Row.PrimaryAlternatives.Span) &&
        unchanged.SecondaryAlternatives.Span.SequenceEqual(tail.Row.SecondaryAlternatives.Span),
        "Collector mutated the explicitly prepared inactive record/primary/secondary tail.");
}
Console.WriteLine($"PREPARED inactive collector tail: {preparedInactiveTail.Count} full stored rows; " +
    $"real root cap {actualConfiguration.PrimaryArcLimit230a}/parent arc5 preserved; " +
    $"{liveSourceHeaderCalls} active source-header calls, tail not an NN row.");
Require(liveCollected.Count > 0, "Native ancestry collector did not produce an actual protected path candidate.");
var firstCollected = liveCollected.CandidateStorage.Span[..36];
int replacementRecordCost = BinaryPrimitives.ReadInt32LittleEndian(parentRows[replacementParent].Record.Span[16..]);
Require(BinaryPrimitives.ReadInt32LittleEndian(firstCollected) == unchecked(replacementRecordCost + childRecordCost) &&
    BinaryPrimitives.ReadInt32LittleEndian(firstCollected[4..]) == unchecked(replacementCost - replacementRecordCost + childRecordCost) &&
    BinaryPrimitives.ReadInt32LittleEndian(firstCollected[8..]) == currentNode &&
    BinaryPrimitives.ReadInt32LittleEndian(firstCollected[16..]) == endingNode &&
    BinaryPrimitives.ReadUInt16LittleEndian(firstCollected[24..]) == replacementParent &&
    BinaryPrimitives.ReadUInt16LittleEndian(firstCollected[26..]) == replacementParent &&
    firstCollected[30] == 0 && firstCollected[31] == 0 && firstCollected[32] == 0 &&
    firstCollected[33..].SequenceEqual(realResidue.AsSpan(33)),
    "Actual 210870 ancestry/parent/cost provenance or stack residue changed.");
var liveSelected = LegacyNativeRejectionCandidateSelection.Prepare(liveCollected.CandidateStorage.Span, liveCollected.Count, 2);
LegacyNativePrimaryRejectionPathStitcher.Apply(liveSelected, liveCollected.Count, actualConfiguration.PrimaryArcLimit230a,
    realOwner, actualContexts, key => key);
var collectedParent = realOwner.CopyRow((int)currentNode, 0);
Require((collectedParent.PrimaryAlternatives.Span[31] & 0x40) != 0 &&
    BinaryPrimitives.ReadInt32LittleEndian(collectedParent.PrimaryAlternatives.Span) == correctedPathCost,
    "Collector→selection→stitch did not preserve the already matching protected native path.");
var finalDescendantPools = LegacyNativeInitialParentWordAlternativeProducer.Create(assignedGrandchild.SlotStorage.Span, 1,
    currentNode + 1, actualConfiguration, (node, arc) => realOwner.CopyRow(node, arc), actualContexts, key => key,
    Enumerable.Repeat((byte)0xa7, 80).ToArray(), Enumerable.Repeat((byte)0x5b, 80).ToArray());
var finalDescendant = LegacyNativeWordRowMaterializer.Materialize(assignedGrandchild.SlotStorage.Span[..128], 0,
    actualConfiguration.PrimaryArcLimit230a, 2, 2, finalDescendantPools.PrimaryStorage.Span, finalDescendantPools.SecondaryStorage.Span);
Require(BinaryPrimitives.ReadInt32LittleEndian(finalDescendant.PrimaryAlternatives.Span) ==
    unchecked(correctedPathCost + nextIncrement.Cost) &&
    ReadPath(actualContexts, BinaryPrimitives.ReadUInt64LittleEndian(finalDescendant.PrimaryAlternatives.Span[8..]))
        .SequenceEqual(descendantExpectedPath), "Next native parent consumer lost the collector-selected protected path.");
Console.WriteLine($"ACTUAL RU NN/cache → stored root+child rows → native continuation record → 210870 collector ({liveCollected.Count} candidates) " +
    "→ 211210 selection → 2113f0 live primary storage → next parent-alternative producer/materialized descendant PASS. " +
    "Source headers, scheduling, runtime marker mode and stack residue are explicit diagnostic inputs; final winner remains unfinished.");
