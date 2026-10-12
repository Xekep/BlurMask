using InkRecognition;
using System.Buffers.Binary;
using System.Runtime.InteropServices;
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

// Literal signed subtraction includes overflow and does not become CompareTo.
int comparisons = 0;
foreach (var test in document.RootElement.GetProperty("comparisons").EnumerateArray())
{
    Require(LegacyNativeRejectionCandidateSelection.Compare(Bytes(test, "left"), Bytes(test, "right")) ==
        test.GetProperty("expected").GetInt32(), "Native 210540 signed comparison changed.");
    comparisons++;
}

// Whole 36-byte sorting, including residual bytes and inactive capacity.
int selections = 0, unsafeRefusals = 0;
foreach (var test in document.RootElement.GetProperty("selections").EnumerateArray())
{
    byte[] storage = Bytes(test, "storage"), before = storage.ToArray();
    var expected = test.GetProperty("expected");
    try
    {
        byte[] result = LegacyNativeRejectionCandidateSelection.Prepare(storage,
            test.GetProperty("count").GetInt32(), test.GetProperty("pc").GetUInt16());
        Require(expected.ValueKind != JsonValueKind.Null && result.SequenceEqual(Convert.FromHexString(expected.GetString()!)),
            $"Native 210570 / 211210 full-byte control differs at {selections}.");
    }
    catch (NotSupportedException)
    {
        Require(expected.ValueKind == JsonValueKind.Null, "A valid bounded quicksort partition was refused.");
        unsafeRefusals++;
    }
    Require(storage.SequenceEqual(before), "Selection changed caller storage.");
    selections++;
}

int writers = 0;
foreach (var test in document.RootElement.GetProperty("writers").EnumerateArray())
{
    byte[] residue = Bytes(test, "residue"), prepared = Bytes(test, "prepared");
    var fields = new LegacyNativePreparedRejectionFields(
        BinaryPrimitives.ReadInt32LittleEndian(prepared), BinaryPrimitives.ReadInt32LittleEndian(prepared.AsSpan(4)),
        BinaryPrimitives.ReadInt32LittleEndian(prepared.AsSpan(8)), BinaryPrimitives.ReadInt32LittleEndian(prepared.AsSpan(12)),
        BinaryPrimitives.ReadInt32LittleEndian(prepared.AsSpan(16)), BinaryPrimitives.ReadUInt16LittleEndian(prepared.AsSpan(20)),
        BinaryPrimitives.ReadUInt16LittleEndian(prepared.AsSpan(22)), BinaryPrimitives.ReadUInt16LittleEndian(prepared.AsSpan(24)),
        BinaryPrimitives.ReadUInt16LittleEndian(prepared.AsSpan(26)), BinaryPrimitives.ReadUInt16LittleEndian(prepared.AsSpan(28)),
        prepared[30], prepared[31]);
    Require(LegacyNativePreparedRejectionProducer.Create(residue, fields).SequenceEqual(prepared),
        "Prepared ordinary 210870 append lost stack residue or scalar fields.");
    var fallbackFields = new LegacyNativeRejectionFallbackFields(fields.StartNode08, fields.EndNode0c,
        fields.PreviousNode10, fields.GroupKey14, fields.EndArc16, fields.SourceAlternative1e);
    Require(LegacyNativeRejectionFallbackProducer.Create(residue, test.GetProperty("limit").GetUInt16(), fallbackFields)
        .SequenceEqual(Bytes(test, "fallback")), "Native 210720 fallback bytes changed.");
    writers++;
}

// Build canonical context IDs from independent input and expected symbol paths.
// Numeric identities are filled before executing the mutable native operation.
int mutations = 0, rowComparisons = 0, mappingCalls = 0;
foreach (var test in document.RootElement.GetProperty("mutations").EnumerateArray())
{
    var contexts = new LegacySymbolContextFactory();
    ushort pc = test.GetProperty("pc").GetUInt16();
    var supplied = new List<LegacyNativeOwnedWordRow>();
    var expectedRows = new List<(int Node, ushort Arc, byte[] Primary, LegacyNativeStoredWordRecord Original)>();
    foreach (var row in test.GetProperty("rows").EnumerateArray())
    {
        int node = row.GetProperty("node").GetInt32();
        ushort arc = row.GetProperty("arc").GetUInt16();
        byte[] slot = Bytes(row, "slot"), primary = Bytes(row, "primary"), expected = Bytes(row, "expected");
        BinaryPrimitives.WriteUInt64LittleEndian(slot.AsSpan(8), Path(contexts, Keys(row.GetProperty("suffix"))));
        for (int index = 0; index < pc; index++)
            for (int kind = 0; kind < 2; kind++)
            {
                BinaryPrimitives.WriteUInt64LittleEndian(primary.AsSpan(index * 40 + 8 + kind * 8),
                    Path(contexts, Keys(row.GetProperty("prefixes")[index][kind])));
                BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(index * 40 + 8 + kind * 8),
                    Path(contexts, Keys(row.GetProperty("outputs")[index][kind])));
            }
        var stored = LegacyNativeWordRowMaterializer.Materialize(slot, arc, 3, pc, 2, primary, Bytes(row, "secondary"));
        supplied.Add(new(node, arc, stored));
        expectedRows.Add((node, arc, expected, stored));
    }
    var owner = new LegacyNativePrimaryWordRowOwner(pc, supplied);
    byte[] candidates = Bytes(test, "candidates"), candidateBefore = candidates.ToArray();
    var actualCalls = new List<ushort>();
    LegacyNativePrimaryRejectionPathStitcher.Apply(candidates, candidates.Length / 36, 3, owner, contexts,
        key => { actualCalls.Add(key); return (ushort)(key ^ 0x55); });
    Require(actualCalls.SequenceEqual(Keys(test.GetProperty("calls"))), "Live native context mapping order differs.");
    foreach (var row in expectedRows)
    {
        var actual = owner.CopyRow(row.Node, row.Arc);
        Require(actual.PrimaryAlternatives.Span.SequenceEqual(row.Primary),
            $"Native 2113f0 full primary bytes differ at mutation {mutations}, node {row.Node}, arc {row.Arc}.");
        Require(actual.Record.Span.SequenceEqual(row.Original.Record.Span) &&
            actual.SecondaryAlternatives.Span.SequenceEqual(row.Original.SecondaryAlternatives.Span),
            "Primary path stitch changed stored 88-byte rows or secondary blocks.");
        rowComparisons++;
    }
    Require(candidates.SequenceEqual(candidateBefore), "Path mutation changed candidate storage.");
    mappingCalls += actualCalls.Count;
    mutations++;
}

// Owning snapshots remain detached, including mutable backing array exposure.
var ownershipSlot = new byte[128];
var ownershipPrimary = Enumerable.Repeat((byte)0xab, 80).ToArray();
var ownershipRow = LegacyNativeWordRowMaterializer.Materialize(ownershipSlot, 0, 1, 2, 2,
    ownershipPrimary, new byte[80]);
var ownership = new LegacyNativePrimaryWordRowOwner(2, new[] { new LegacyNativeOwnedWordRow(0, 0, ownershipRow) });
MemoryMarshal.TryGetArray(ownershipRow.PrimaryAlternatives, out var sourceArray);
sourceArray.Array![sourceArray.Offset] = 0x12;
var copy = ownership.CopyRow(0, 0);
MemoryMarshal.TryGetArray(copy.PrimaryAlternatives, out var copiedArray);
copiedArray.Array![copiedArray.Offset] = 0x34;
Require(ownership.CopyRow(0, 0).PrimaryAlternatives.Span[0] == 0xab, "Owned row snapshot shares caller or output storage.");

// Pre-cancellation must occur before any live mutation or mapper invocation.
byte[] guardCandidate = new byte[36];
guardCandidate[32] = 1;
try
{
    LegacyNativePrimaryRejectionPathStitcher.Apply(guardCandidate, 1, 1, ownership,
        new LegacySymbolContextFactory(), key => throw new Exception("Cancelled mapper called."), new CancellationToken(true));
    throw new Exception("Cancelled live stitch was accepted.");
}
catch (OperationCanceledException) { }
Require(ownership.CopyRow(0, 0).PrimaryAlternatives.Span[0] == 0xab, "Pre-cancellation mutated primary storage.");
try
{
    LegacyNativeRejectionCandidateSelection.Prepare(new byte[36], 1, 21);
    throw new Exception("Native 20-slot selection capacity exceeded.");
}
catch (ArgumentException) { }
try
{
    LegacyNativePrimaryRejectionPathStitcher.Apply(guardCandidate, 1, 1, ownership,
        new LegacySymbolContextFactory(), key => key, tracingEnabled2368: true);
    throw new Exception("Missing native trace side tables were accepted.");
}
catch (NotSupportedException) { }

// Native row mutation is live, so cancellation after the mapper callback
// does not roll back already written costs/metadata. It stops composition.
using (var callbackCancellation = new CancellationTokenSource())
{
    var callbackContexts = new LegacySymbolContextFactory();
    ulong prefix = Path(callbackContexts, new ushort[] { 65 });
    ulong suffix = Path(callbackContexts, new ushort[] { 66 });
    var parentSlot = new byte[128];
    var childSlot = new byte[128];
    BinaryPrimitives.WriteUInt64LittleEndian(childSlot.AsSpan(8), suffix);
    var parentPrimary = new byte[80];
    var callbackChildPrimary = new byte[80];
    BinaryPrimitives.WriteUInt64LittleEndian(parentPrimary.AsSpan(8), prefix);
    BinaryPrimitives.WriteUInt64LittleEndian(parentPrimary.AsSpan(16), prefix);
    var parentRow = LegacyNativeWordRowMaterializer.Materialize(parentSlot, 0, 1, 2, 2, parentPrimary, new byte[80]);
    var childRow = LegacyNativeWordRowMaterializer.Materialize(childSlot, 0, 1, 2, 2, callbackChildPrimary, new byte[80]);
    var callbackOwner = new LegacyNativePrimaryWordRowOwner(2,
        new[] { new LegacyNativeOwnedWordRow(0, 0, parentRow), new LegacyNativeOwnedWordRow(1, 0, childRow) });
    byte[] selected = LegacyNativePreparedRejectionProducer.Create(new byte[36],
        new(1, 77, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0));
    selected[32] = 1;
    int cancelledCallbacks = 0;
    try
    {
        LegacyNativePrimaryRejectionPathStitcher.Apply(selected, 1, 1, callbackOwner, callbackContexts,
            key => { cancelledCallbacks++; callbackCancellation.Cancel(); return key; }, callbackCancellation.Token);
        throw new Exception("Cancellation inside context mapper was ignored.");
    }
    catch (OperationCanceledException) { }
    var partial = callbackOwner.CopyRow(1, 0);
    Require(cancelledCallbacks == 1 && callbackContexts.NodeCount == 2 &&
        BinaryPrimitives.ReadInt32LittleEndian(partial.PrimaryAlternatives.Span) == 77 &&
        BinaryPrimitives.ReadInt32LittleEndian(childRow.PrimaryAlternatives.Span) == 0,
        "Live cancellation rolled back native writes, created context nodes or touched input row storage.");
}

// A physical predecessor above 230a has a real stored record but no primary
// block. The optional owning callback supplies record bytes only.
foreach (bool cancelHigherLookup in new[] { false, true })
{
    using var higherCancellation = new CancellationTokenSource();
    var higherContexts = new LegacySymbolContextFactory();
    var lowerRows = new List<LegacyNativeOwnedWordRow>();
    for (int node = 0; node <= 2; node++)
    {
        var slot = new byte[128];
        BinaryPrimitives.WriteInt32LittleEndian(slot.AsSpan(16), node == 2 ? 50 : 0);
        BinaryPrimitives.WriteUInt16LittleEndian(slot.AsSpan(30), node == 2 ? (ushort)1 : (ushort)0);
        var primary = new byte[80];
        lowerRows.Add(new(node, 0, LegacyNativeWordRowMaterializer.Materialize(slot, 0, 1, 2, 2, primary, new byte[80])));
    }
    var higherSlot = new byte[128];
    BinaryPrimitives.WriteInt32LittleEndian(higherSlot.AsSpan(16), 10);
    var higherRow = LegacyNativeWordRowMaterializer.Materialize(higherSlot, 1, 1, 2, 2, default, new byte[80]);
    Require(higherRow.PrimaryAlternatives.Length == 0, "Higher-row control fabricated primary alternative storage.");
    var higherOwner = new LegacyNativePrimaryWordRowOwner(2, lowerRows);
    byte[] selected = LegacyNativePreparedRejectionProducer.Create(new byte[36],
        new(1, 77, 1, 2, 0, 0, 0, 0, 0, 0, 0, 0));
    selected[32] = 1;
    int callbacks = 0;
    try
    {
        LegacyNativePrimaryRejectionPathStitcher.Apply(selected, 1, 1, higherOwner, higherContexts, key => key,
            higherCancellation.Token, higherArcRowLookup: (node, arc) =>
            {
                Require(node == 1 && arc == 1, "Physical higher predecessor lineage changed.");
                callbacks++;
                if (cancelHigherLookup) higherCancellation.Cancel();
                return higherRow;
            });
        Require(!cancelHigherLookup && BinaryPrimitives.ReadInt32LittleEndian(higherOwner.CopyRow(2, 0).PrimaryAlternatives.Span) == 117,
            "Later-point repair did not use the actual higher physical predecessor cost.");
    }
    catch (OperationCanceledException)
    {
        Require(cancelHigherLookup && BinaryPrimitives.ReadInt32LittleEndian(higherOwner.CopyRow(2, 0).PrimaryAlternatives.Span) == 0x4000000,
            "Higher-row callback cancellation wrote a later-point cost.");
    }
    Require(callbacks == 1 && higherRow.Record.Span.SequenceEqual(higherSlot.AsSpan(0, 88)),
        "Higher-row callback count or unchanged owned record differs.");
}

Console.WriteLine($"PASS {comparisons} comparisons / {selections} full36 selections ({unsafeRefusals} unsafe refusals) / " +
    $"{writers} prepared+fallback writers / {mutations} live primary mutations / {rowComparisons} full-row comparisons / " +
    $"{mappingCalls} exact mapping callbacks.");

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
var realRows = parentRows.Select((row, arc) => new LegacyNativeOwnedWordRow(endingNode, (ushort)arc, row))
    .Concat(storedChildren.Select((row, arc) => new LegacyNativeOwnedWordRow((int)currentNode, (ushort)arc, row))).ToArray();
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
    "210870 ancestry/correction production, source scheduling and final winner remain explicit boundaries.");
