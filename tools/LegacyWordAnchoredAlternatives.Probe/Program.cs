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



int paths = 0, rejectedReferences = 0;
foreach (var test in document.RootElement.GetProperty("paths").EnumerateArray())
{
    var contexts = new LegacySymbolContextFactory();
    ulong current = Path(contexts, Keys(test.GetProperty("current")));
    ulong reference = Path(contexts, Keys(test.GetProperty("reference")));
    var expected = test.GetProperty("expected");
    if (expected.ValueKind == JsonValueKind.Null)
    {
        try
        {
            LegacyNativeAnchoredContextProjection.Extract(contexts, current, reference);
            throw new Exception("Null reference invariant was silently replaced by an empty node.");
        }
        catch (NotSupportedException) { }
        rejectedReferences++;
    }
    else
    {
        var projection = LegacyNativeAnchoredContextProjection.Extract(contexts, current, reference);
        Require(ReadPath(contexts, projection.ReferenceNode).SequenceEqual(Keys(expected.GetProperty("reference"))) &&
            ReadPath(contexts, projection.ReferenceParent).SequenceEqual(Keys(expected.GetProperty("parent"))) &&
            ReadPath(contexts, projection.MatchedNode).SequenceEqual(Keys(expected.GetProperty("matched"))) &&
            projection.KeysInParentOrder.Span.SequenceEqual(Keys(expected.GetProperty("keys"))),
            $"Native reference-pair/tail differs at {paths}.");
    }
    paths++;
}
int cases = 0, lookups = 0;
foreach (var test in document.RootElement.GetProperty("cases").EnumerateArray())
{
    var contexts = new LegacySymbolContextFactory();
    ushort count = test.GetProperty("count").GetUInt16();
    byte[] slots = Convert.FromHexString(test.GetProperty("slots").GetString()!);
    byte[] previous = Convert.FromHexString(test.GetProperty("previous").GetString()!);
    byte[] expected = Convert.FromHexString(test.GetProperty("expected").GetString()!);
    var records = test.GetProperty("records").EnumerateArray().Select(x => Convert.FromHexString(x.GetString()!)).ToArray();
    var pools = test.GetProperty("pools").EnumerateArray().Select(x => Convert.FromHexString(x.GetString()!)).ToArray();
    BinaryPrimitives.WriteUInt64LittleEndian(pools[0].AsSpan(8), Path(contexts, Keys(test.GetProperty("current"))));
    BinaryPrimitives.WriteUInt64LittleEndian(pools[0].AsSpan(16), Path(contexts, Keys(test.GetProperty("raw"))));
    BinaryPrimitives.WriteUInt64LittleEndian(pools[2].AsSpan(8), Path(contexts, Keys(test.GetProperty("reference"))));
    for (int k = 0; k < count; k++)
    {
        BinaryPrimitives.WriteUInt64LittleEndian(pools[3].AsSpan(k * 40 + 8), Path(contexts, Keys(test.GetProperty("prefixes")[k])));
        // Different opaque raw IDs prove the producer uses current parent's raw
        // alternative0, rather than copying replacement alternative[k].raw.
        BinaryPrimitives.WriteUInt64LittleEndian(pools[3].AsSpan(k * 40 + 16), Path(contexts, new ushort[] { (ushort)(500 + k) }));
    }
    BinaryPrimitives.WriteUInt64LittleEndian(slots.AsSpan(8), Path(contexts, Keys(test.GetProperty("suffix"))));
    foreach (var write in test.GetProperty("writes").EnumerateArray())
    {
        int at = write.GetProperty("index").GetInt32() * 40;
        BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(at + 8), Path(contexts, Keys(write.GetProperty("mapped"))));
        BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(at + 16), Path(contexts, Keys(write.GetProperty("raw"))));
    }
    var rows = records.Select((record, index) => LegacyNativeWordRowMaterializer.Materialize(record,
        (ushort)index, 0, 1, count, default, pools[index])).ToArray();
    int localLookups = 0, anchorNode = test.GetProperty("anchorNode").GetInt32(), ancestor = test.GetProperty("ancestor").GetInt32();
    LegacyNativeStoredWordRecord Read(int node, ushort arc)
    {
        var address = localLookups switch
        {
            0 => (10, (ushort)0), 1 => (anchorNode, (ushort)1),
            2 => (ancestor, (ushort)2), 3 => (ancestor, (ushort)3),
            _ => throw new Exception("Extra anchored parent lookup.")
        };
        Require((node, arc) == address, "Anchor owner/span/reference/replacement provenance differs.");
        return rows[localLookups++];
    }
    byte[] before = slots.ToArray();
    var config = LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with { SecondaryAlternativeCount2404 = count };
    var produced = LegacyNativeAnchoredWordAlternativeProducer.Create(slots, 1, 10, 0, -1, 0,
        config, Read, contexts, key => (ushort)(key ^ 0x55), previous);
    Require(produced.Succeeded && produced.SecondaryStorage.Span.SequenceEqual(expected),
        $"Complete anchored pool bytes differ at {cases}.");
    Require(localLookups == 4 && slots.SequenceEqual(before), "Anchored lookup count or unchanged full128 input differs.");
    lookups += localLookups;
    if (cases == 0)
    {
        for (int cancelledLookup = 1; cancelledLookup <= 4; cancelledLookup++)
        {
            using var cts = new CancellationTokenSource();
            localLookups = 0;
            int originalNodes = contexts.NodeCount;
            try
            {
                LegacyNativeAnchoredWordAlternativeProducer.Create(slots, 1, 10, 0, -1, 0, config,
                    (node, arc) =>
                    {
                        var row = Read(node, arc);
                        if (localLookups == cancelledLookup) cts.Cancel();
                        return row;
                    }, contexts, key => (ushort)(key ^ 0x55), previous, cancellationToken: cts.Token);
                throw new Exception("Owning anchored row callback cancellation bypassed a return.");
            }
            catch (OperationCanceledException) { }
            Require(localLookups == cancelledLookup && contexts.NodeCount == originalNodes,
                "Cancelled owning row callback made extra lookups or context mutations.");
        }
    }
    cases++;
}
// Direct replay must preserve controls that would otherwise fold in1f7ff0.
var literal = new LegacySymbolContextFactory();
ulong prefix = Path(literal, new ushort[] { 0xfffc });
ulong replayed = LegacyNativeAnchoredContextProjection.Replay(literal, prefix, new ushort[] { 0xfffa });
Require(ReadPath(literal, replayed).SequenceEqual(new ushort[] { 0xfffc, 0xfffa }), "Retained keys were incorrectly control-composed.");
ulong folded = LegacyNativeWordContextComposition.CombineRaw(literal, prefix, Path(literal, new ushort[] { 0xfffa }));
Require(folded == prefix, "Literal replay regression does not distinguish native control composition.");
// All prepass return branches must observe cancellation inside the callback.
foreach (ushort count in new ushort[] { 1, 2 })
    foreach (bool success in new[] { false, true })
    {
        using var cts = new CancellationTokenSource();
        var guardSlot = new byte[128];
        var guardPool = Enumerable.Repeat((byte)0x5b, count * 40).ToArray();
        int called = 0;
        try
        {
            LegacyNativeAnchoredWordAlternativeProducer.Create(guardSlot, 1, 0, 0, -1, 0,
                LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with
                { SecondaryAlternativeCount2404 = count, PackedSource22e8 = 0x10 },
                (_, _) => throw new Exception("Cancelled prepass queried parent."), new LegacySymbolContextFactory(),
                key => key, guardPool, () => { called++; cts.Cancel(); return success; }, cts.Token);
            throw new Exception("Cancellation inside anchored prepass bypassed early return.");
        }
        catch (OperationCanceledException) { }
        Require(called == 1 && guardPool.All(value => value == 0x5b), "Anchored cancellation changed caller-owned pool.");
    }
try
{
    LegacyNativeAnchoredContextProjection.Extract(literal, prefix, 0);
    throw new Exception("Null reference input synthesized a node.");
}
catch (NotSupportedException) { }
var unrelated = new LegacySymbolContextFactory();
try
{
    LegacyNativeAnchoredContextProjection.Extract(unrelated, prefix, prefix);
    throw new Exception("Foreign reference/current context accepted.");
}
catch (ArgumentException) { }
try
{
    LegacyNativeAnchoredContextProjection.Replay(literal, prefix, new ushort[] { 1 }, new CancellationToken(true));
    throw new Exception("Cancelled literal context replay accepted.");
}
catch (OperationCanceledException) { }

Console.WriteLine($"PASS {paths} reference-pair controls/{rejectedReferences} explicit null-invariant refusals; " +
    $"{cases} complete anchored pools/{lookups} exact owner/span/replacement lookups; literal control-key replay.");
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
var actualChildren = new List<LegacyNativeStoredWordRecord>();
foreach (var ordered in childOrdered)
    for (int index = 0; index < ordered.Heap.Count; index++)
    {
        var slot = ordered.Heap.GetSlotSnapshot(index);
        ushort parentArc = BinaryPrimitives.ReadUInt16LittleEndian(slot.Span[0x1e..]);
        var row = LegacyNativeWordRowMaterializer.Materialize(slot.Span, (ushort)childArc++,
            childConfiguration.PrimaryArcLimit230a, 2, 2, childPrimaryPool.GetSnapshot().Span, childSecondaryPool.GetSnapshot().Span);
        actualChildren.Add(row);
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

int childNode = endingNode + 1;
// Explicit diagnostic self anchors use the actual stored child's node/arc. The
// upstream3e/anchor policy is not inferred from neural recognition confidence.
var anchoredParents = actualChildren.Select((row, arc) =>
{
    var record = new byte[128];
    row.Record.Span.CopyTo(record);
    BinaryPrimitives.WriteInt32LittleEndian(record.AsSpan(0x38), childNode);
    BinaryPrimitives.WriteUInt16LittleEndian(record.AsSpan(0x3c), (ushort)arc);
    return LegacyNativeWordRowMaterializer.Materialize(record, (ushort)arc, 0, 1, 2,
        default, row.SecondaryAlternatives.Span);
}).ToArray();
LegacyNativeStoredWordRecord AnchoredRow(int node, ushort arc)
{
    if (node == childNode && arc < anchoredParents.Length) return anchoredParents[arc];
    if (node == endingNode && arc < parentRows.Count) return parentRows[arc];
    throw new Exception("Anchored fixture requested invented owner/arc.");
}
var anchorInputHeap = new LegacyNativeWordRecordHeap(4);
var anchorInputRouter = new LegacyNativeWordHeapRouter(new[] { anchorInputHeap }, new byte[] { 0 }, 0, uint.MaxValue);
for (ushort parentArc = 0; parentArc < 2; parentArc++)
    for (ushort replacementArc = 0; replacementArc < 2; replacementArc++)
    {
        int cost = BinaryPrimitives.ReadInt32LittleEndian(anchoredParents[parentArc].Record.Span[0x10..]);
        var increment = increments[0];
        var fields = new LegacyContinuationWordRecordFields(0, unchecked(cost + increment.Cost), increment.Cost,
            replacementArc, new uint[] { 0, 0, 0, 0, 0 },
            actualContexts.ResolveInitial(increment.Symbol).Identity, 0, (byte)increment.Symbol, 0, 0);
        LegacyContinuationWordRecordProducer.TryInsert(anchorInputRouter, childNode, parentArc,
            (node, arc) => AnchoredRow(node, arc).Record, fields, 0, false);
    }
var assignedAnchors = LegacyNativeWordAlternativeOrdinals.AssignContinuation(anchorInputHeap.GetSlotStorageSnapshot().Span,
    anchorInputHeap.Count);
var anchoredPoolBytes = Enumerable.Repeat((byte)0x5b, anchorInputHeap.Count * 80).ToArray();
var anchoredPools = LegacyNativeAnchoredWordAlternativeProducer.Create(assignedAnchors.SlotStorage.Span,
    anchorInputHeap.Count, childNode, 0, -1, 0, actualConfiguration,
    AnchoredRow, actualContexts, key => key, anchoredPoolBytes);
Require(anchoredPools.Succeeded && anchorInputHeap.Count == 4, "Actual anchored resource paths did not produce their pool rows.");
for (int index = 0; index < anchorInputHeap.Count; index++)
{
    var slot = assignedAnchors.SlotStorage.Span.Slice(index * 128, 128);
    ushort parentArc = BinaryPrimitives.ReadUInt16LittleEndian(slot[0x1e..]);
    ushort replacementArc = BinaryPrimitives.ReadUInt16LittleEndian(slot[0x3e..]);
    var parent = anchoredParents[parentArc];
    ushort referenceArc = BinaryPrimitives.ReadUInt16LittleEndian(parent.Record.Span[0x1e..]);
    ushort[] currentPath = ReadPath(actualContexts, BinaryPrimitives.ReadUInt64LittleEndian(parent.SecondaryAlternatives.Span[8..]));
    ushort[] referencePath = ReadPath(actualContexts, BinaryPrimitives.ReadUInt64LittleEndian(parentRows[referenceArc].SecondaryAlternatives.Span[8..]));
    // In this actual resource fixture the anchor reference is a root symbol.
    // The tail contains the child symbol, or the full current path if its root
    // differs; this is independent of the implementation's pair-walk result.
    int matchedLength = currentPath.Length != 0 && currentPath[0] == referencePath[0] ? 1 : 0;
    ushort[] retainedKeys = currentPath[matchedLength..];
    ushort[] prefixPath = ReadPath(actualContexts, BinaryPrimitives.ReadUInt64LittleEndian(parentRows[replacementArc].SecondaryAlternatives.Span[8..]));
    ushort[] suffixPath = ReadPath(actualContexts, BinaryPrimitives.ReadUInt64LittleEndian(slot[8..]));
    var first = anchoredPools.SecondaryStorage.Span.Slice(index * 80, 40);
    Require(ReadPath(actualContexts, BinaryPrimitives.ReadUInt64LittleEndian(first[8..]))
        .SequenceEqual(prefixPath.Concat(retainedKeys).Concat(suffixPath)),
        "Actual anchored mapped path lost ancestor replacement or retained child/suffix symbols.");
    Require(ReadPath(actualContexts, BinaryPrimitives.ReadUInt64LittleEndian(first[16..]))
        .SequenceEqual(ReadPath(actualContexts, BinaryPrimitives.ReadUInt64LittleEndian(parent.SecondaryAlternatives.Span[16..])).Concat(suffixPath)),
        "Actual anchored raw path used replacement raw context instead of current parent's raw.");
    Require(BinaryPrimitives.ReadInt32LittleEndian(first) == BinaryPrimitives.ReadInt32LittleEndian(slot[0x10..]) &&
        BinaryPrimitives.ReadUInt16LittleEndian(first[0x20..]) == replacementArc && first[0x1b] == 0,
        "Anchored actual acoustic delta or replacement metadata differs.");
}
var anchoredOrdered = LegacyNativeWordAlternativeOrdering.Prepare(assignedAnchors.SlotStorage.Span,
    anchorInputHeap.Count, true, actualConfiguration, new uint[] { 0 }, false, null,
    new LegacyNativeWordAlternativeBuffer(2, anchoredPools.SecondaryStorage.Span));
for (int index = 0; index < anchoredOrdered.Heap.Count; index++)
{
    var row = LegacyNativeWordRowMaterializer.Materialize(anchoredOrdered.Heap.GetSlotSnapshot(index).Span,
        (ushort)(actualConfiguration.PrimaryArcLimit230a + index), actualConfiguration.PrimaryArcLimit230a,
        2, 2, default, anchoredOrdered.SecondaryAlternativeStorage.Span);
    Require(row.PrimaryAlternatives.IsEmpty && row.SecondaryAlternatives.Length == 80,
        "Anchored rows did not feed the exact secondary-only continuation materializer.");
}
Console.WriteLine($"ACTUAL RU NN/cache ? {actualChildren.Count} real stored children ? {anchorInputHeap.Count} owned anchored pools ? " +
    $"{anchoredOrdered.Heap.Count} continuation stored rows; explicit self-anchor/replacement policy,1f8300 mapping and scheduling.");
