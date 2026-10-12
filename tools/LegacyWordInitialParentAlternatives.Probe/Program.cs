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



var resetTokens = document.RootElement.GetProperty("resetTokens").EnumerateArray().Select(x => x.GetUInt16()).ToHashSet();
for (int token = 0; token < 65536; token++)
    Require(LegacyNativeInitialParentWordAlternativeProducer.ResetsPrimaryContext((ushort)token) == resetTokens.Contains((ushort)token),
        "Native signed source-token predicate differs.");
int cases = 0, lookups = 0;
foreach (var test in document.RootElement.GetProperty("cases").EnumerateArray())
{
    var contexts = new LegacySymbolContextFactory();
    int pc = test.GetProperty("pc").GetInt32(), sc = test.GetProperty("sc").GetInt32(), n = test.GetProperty("n").GetInt32();
    byte[] slots = Convert.FromHexString(test.GetProperty("slots").GetString()!);
    byte[] primary = Convert.FromHexString(test.GetProperty("primary").GetString()!);
    byte[] secondary = Convert.FromHexString(test.GetProperty("secondary").GetString()!);
    byte[] expectedPrimary = Convert.FromHexString(test.GetProperty("expectedP").GetString()!);
    byte[] expectedSecondary = Convert.FromHexString(test.GetProperty("expectedS").GetString()!);
    var parents = new LegacyNativeStoredWordRecord[3];
    for (int parent = 0; parent < 3; parent++)
    {
        var record = Convert.FromHexString(test.GetProperty("parents")[parent].GetProperty("record").GetString()!);
        BinaryPrimitives.WriteUInt64LittleEndian(record.AsSpan(8),
            Path(contexts, new ushort[] { test.GetProperty("tokens")[parent].GetUInt16() }));
        var pools = new[]
        {
            Convert.FromHexString(test.GetProperty("parents")[parent].GetProperty("primary").GetString()!),
            Convert.FromHexString(test.GetProperty("parents")[parent].GetProperty("secondary").GetString()!)
        };
        for (int kind = 0; kind < 2; kind++)
            for (int k = 0; k < (kind == 0 ? pc : sc); k++)
            {
                var prefixes = test.GetProperty("prefixes")[parent][kind][k];
                BinaryPrimitives.WriteUInt64LittleEndian(pools[kind].AsSpan(k * 40 + 8), Path(contexts, Keys(prefixes[0])));
                BinaryPrimitives.WriteUInt64LittleEndian(pools[kind].AsSpan(k * 40 + 16), Path(contexts, Keys(prefixes[1])));
            }
        parents[parent] = LegacyNativeWordRowMaterializer.Materialize(record, (ushort)parent, 3,
            (ushort)pc, (ushort)sc, pools[0], pools[1]);
    }
    for (int index = 0; index < n; index++)
    {
        var slot = slots.AsSpan(index * 128, 128);
        BinaryPrimitives.WriteUInt64LittleEndian(slot[8..], Path(contexts, Keys(test.GetProperty("suffixes")[index])));
        int ordinal = BinaryPrimitives.ReadInt32LittleEndian(slot[0x58..]);
        for (int kind = 0; kind < 2; kind++)
        {
            int count = kind == 0 ? pc : sc;
            byte[] target = kind == 0 ? expectedPrimary : expectedSecondary;
            foreach (var write in test.GetProperty("outputs")[index][kind].EnumerateArray())
            {
                int at = (ordinal * count + write.GetProperty("index").GetInt32()) * 40;
                BinaryPrimitives.WriteUInt64LittleEndian(target.AsSpan(at + 8), Path(contexts, Keys(write.GetProperty("mapped"))));
                BinaryPrimitives.WriteUInt64LittleEndian(target.AsSpan(at + 16), Path(contexts, Keys(write.GetProperty("raw"))));
            }
        }
    }
    byte[] before = slots.ToArray();
    int localLookups = 0;
    LegacyNativeStoredWordRecord ReferenceParent(int node, ushort arc)
    {
        Require(node == test.GetProperty("expectedNodes")[localLookups].GetInt32() && arc < 3,
            "Initial parent lookup changed actual signed-span lineage or arc.");
        localLookups++;
        return parents[arc];
    }
    var config = LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with
    { PrimaryAlternativeCount2402 = (ushort)pc, SecondaryAlternativeCount2404 = (ushort)sc, PrimaryArcLimit230a = 3 };
    var produced = LegacyNativeInitialParentWordAlternativeProducer.Create(slots, n,
        test.GetProperty("node").GetUInt32(), config, ReferenceParent, contexts, key => (ushort)(key ^ 0x55), primary, secondary);
    Require(produced.PrimaryStorage.Span.SequenceEqual(expectedPrimary), $"Primary parent pool full bytes differ at {cases}.");
    Require(produced.SecondaryStorage.Span.SequenceEqual(expectedSecondary), $"Secondary parent pool full bytes differ at {cases}.");
    Require(localLookups == n && slots.SequenceEqual(before), "Physical lookup count or unchanged full128 storage differs.");
    lookups += localLookups;
    cases++;
}
var guardContext = new LegacySymbolContextFactory();
var unsupported = new byte[128];
int guardLookups = 0;
LegacyNativeStoredWordRecord NoParent(int _, ushort __)
{
    guardLookups++;
    throw new Exception("Unsupported branch queried a parent.");
}
var guardConfiguration = LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with { PrimaryArcLimit230a = 1 };
try
{
    LegacyNativeInitialParentWordAlternativeProducer.Create(unsupported, 1, 0, guardConfiguration,
        NoParent, guardContext, key => key, new byte[80], new byte[80]);
    throw new Exception("Root branch accepted in primary-parent endpoint.");
}
catch (NotSupportedException) { }
BinaryPrimitives.WriteUInt16LittleEndian(unsupported.AsSpan(0x1e), 1);
try
{
    LegacyNativeInitialParentWordAlternativeProducer.Create(unsupported, 1, 1, guardConfiguration,
        NoParent, guardContext, key => key, new byte[80], new byte[80]);
    throw new Exception("Higher-arc secondary-parent branch accepted.");
}
catch (NotSupportedException) { }
try
{
    LegacyNativeInitialParentWordAlternativeProducer.Create(unsupported, 1, 1, guardConfiguration,
        NoParent, guardContext, key => key, new byte[80], new byte[80], new CancellationToken(true));
    throw new Exception("Cancelled parent endpoint accepted.");
}
catch (OperationCanceledException) { }
Require(guardLookups == 0 && guardContext.NodeCount == 0, "Unsupported/cancelled branch queried or created parent state.");
var noOp = LegacyNativeInitialParentWordAlternativeProducer.Create(unsupported, 1, 0,
    guardConfiguration with { PrimaryAlternativeCount2402 = 1, SecondaryAlternativeCount2404 = 1 },
    NoParent, guardContext, key => throw new Exception("Disabled pools called mapper."), new byte[40], new byte[40]);
Require(guardLookups == 0 && noOp.PrimaryStorage.Length == 40, "Both-disabled branch did not preserve native no-op.");

// Owning row callbacks may cancel before returning their valid snapshot.
// The appendfalse path has no context calls, so it needs an explicit check.
foreach (bool appendSuffix in new[] { false, true })
{
    using var cts = new CancellationTokenSource();
    var callbackContexts = new LegacySymbolContextFactory();
    ulong parentSource = Path(callbackContexts, new ushort[] { 44 });
    ulong suffix = Path(callbackContexts, new ushort[] { 55 });
    var callbackParentSlot = new byte[128];
    BinaryPrimitives.WriteUInt64LittleEndian(callbackParentSlot.AsSpan(8), parentSource);
    var callbackParent = LegacyNativeWordRowMaterializer.Materialize(callbackParentSlot, 0, 1, 2, 2,
        new byte[80], new byte[80]);
    var callbackSlot = new byte[128];
    callbackSlot[5] = 0x80;
    BinaryPrimitives.WriteUInt64LittleEndian(callbackSlot.AsSpan(8), suffix);
    BinaryPrimitives.WriteUInt16LittleEndian(callbackSlot.AsSpan(0x18), appendSuffix ? (ushort)1 : (ushort)0);
    int callbacks = 0, mapCalls = 0;
    var callbackPrimary = Enumerable.Repeat((byte)0xa7, 80).ToArray();
    var callbackSecondary = Enumerable.Repeat((byte)0x5b, 80).ToArray();
    try
    {
        LegacyNativeInitialParentWordAlternativeProducer.Create(callbackSlot, 1, 1, guardConfiguration,
            (_, _) => { callbacks++; cts.Cancel(); return callbackParent; }, callbackContexts,
            key => { mapCalls++; return key; }, callbackPrimary, callbackSecondary, cts.Token);
        throw new Exception("Cancellation inside parent callback bypassed append suffix branch.");
    }
    catch (OperationCanceledException) { }
    Require(callbacks == 1 && mapCalls == 0 && callbackContexts.NodeCount == 2 &&
        callbackPrimary.All(value => value == 0xa7) && callbackSecondary.All(value => value == 0x5b),
        "Cancelled owning callback performed composition or changed caller pools.");
}

Console.WriteLine($"PASS {cases} independent full primary/secondary parent pool controls / {lookups} exact lineage lookups / 65536 source-token predicates.");
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
foreach (var ordered in childOrdered)
    for (int index = 0; index < ordered.Heap.Count; index++)
    {
        var slot = ordered.Heap.GetSlotSnapshot(index);
        ushort parentArc = BinaryPrimitives.ReadUInt16LittleEndian(slot.Span[0x1e..]);
        var row = LegacyNativeWordRowMaterializer.Materialize(slot.Span, (ushort)childArc++,
            childConfiguration.PrimaryArcLimit230a, 2, 2, childPrimaryPool.GetSnapshot().Span, childSecondaryPool.GetSnapshot().Span);
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
