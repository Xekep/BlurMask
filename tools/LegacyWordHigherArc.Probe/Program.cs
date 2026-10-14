using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if (args.Length != 5)
    throw new ArgumentException("controls.json AK LK engine.json separation.json required");
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0]));
byte[] ak = File.ReadAllBytes(args[1]);

static void Require(bool value, string message)
{
    if (!value) throw new Exception(message);
}
static byte[] Bytes(JsonElement value, string name) => Convert.FromHexString(value.GetProperty(name).GetString()!);
static ushort[] Keys(JsonElement value) => value.EnumerateArray().Select(item => item.GetUInt16()).ToArray();
static ulong Path(LegacySymbolContextFactory factory, ReadOnlySpan<ushort> keys)
{
    ulong current = 0;
    foreach (ushort key in keys)
    {
        var next = current == 0 ? factory.ResolveInitial(key) : factory.ResolveChild(current, key);
        if (next.Created && current != 0) factory.LinkCreatedContext(next.Identity, current);
        current = next.Identity;
    }
    return current;
}
static ushort[] ReadPath(LegacySymbolContextFactory factory, ulong identity)
{
    var keys = new List<ushort>();
    while (identity != 0)
    {
        var node = factory.ReadNode(identity);
        keys.Add(node.Symbol);
        identity = node.ParentLink;
    }
    keys.Reverse();
    return keys.ToArray();
}
static void Write32(Span<byte> target, int offset, int value) => BinaryPrimitives.WriteInt32LittleEndian(target[offset..], value);
static ushort U16(ReadOnlySpan<byte> source, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(source[offset..]);
static int I32(ReadOnlySpan<byte> source, int offset) => BinaryPrimitives.ReadInt32LittleEndian(source[offset..]);

int cases = 0, requests = 0, incomingMerges = 0, callbacks = 0, lookups = 0, cancellationGuards = 0;
foreach (var test in document.RootElement.GetProperty("cases").EnumerateArray())
{
    var contexts = new LegacySymbolContextFactory();
    ulong[] identities = test.GetProperty("paths").EnumerateArray().Select(path => Path(contexts, Keys(path))).ToArray();
    byte[] Rebind(byte[] value, bool pool)
    {
        if (pool)
            for (int index = 0; index < value.Length / 40; index++)
                for (int kind = 0; kind < 2; kind++)
                {
                    int at = index * 40 + 8 + kind * 8;
                    ulong token = BinaryPrimitives.ReadUInt64LittleEndian(value.AsSpan(at));
                    BinaryPrimitives.WriteUInt64LittleEndian(value.AsSpan(at), identities[checked((int)token)]);
                }
        else
        {
            ulong token = BinaryPrimitives.ReadUInt64LittleEndian(value.AsSpan(8));
            BinaryPrimitives.WriteUInt64LittleEndian(value.AsSpan(8), identities[checked((int)token)]);
        }
        return value;
    }
    ushort pc = test.GetProperty("pc").GetUInt16(), sc = test.GetProperty("sc").GetUInt16();
    ushort limit = test.GetProperty("limit").GetUInt16();
    uint node = test.GetProperty("node").GetUInt32();
    byte[] slot = Rebind(Bytes(test, "slot"), false);
    byte[] primary = Rebind(Bytes(test, "primary"), true), secondary = Rebind(Bytes(test, "secondary"), true);
    var rows = new List<LegacyNativeStoredWordRecord>();
    var jsonRecords = test.GetProperty("records");
    var jsonPools = test.GetProperty("pools");
    for (ushort arc = 0; arc < jsonRecords.GetArrayLength(); arc++)
    {
        byte[] rawSlot = Convert.FromHexString(jsonRecords[arc].GetString()!);
        byte[] pool = Rebind(Convert.FromHexString(jsonPools[arc].GetString()!), true);
        // Row materialization selects ordinal zero. Source slots have caller
        // residue in slot08 that is unrelated to this test's symbolic suffix.
        Write32(rawSlot, 88, 0);
        rows.Add(LegacyNativeWordRowMaterializer.Materialize(rawSlot, arc, limit, 1, sc, default, pool));
    }
    int profileOffset = test.GetProperty("profileOffset").GetInt32();
    var profile = LegacyCursiveCategoryProfile.Load(ak.AsSpan(profileOffset), profileOffset, out _);
    var symbolsArray = Keys(test.GetProperty("symbols"));
    var symbols = new LegacyNativeWordRecombinationSymbols(symbolsArray[0], symbolsArray[1], symbolsArray[2], symbolsArray[3]);
    uint[] headers = test.GetProperty("headers").EnumerateArray().Select(value => value.GetUInt32()).ToArray();
    var configuration = LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with
    {
        PrimaryAlternativeCount2402 = pc, SecondaryAlternativeCount2404 = sc,
        PrimaryArcLimit230a = limit, Mode22f6 = 1
    };
    var policy = new LegacyNativeHigherArcRecombinationPolicy(65534, test.GetProperty("threshold").GetInt32(),
        test.GetProperty("capEnabled").GetBoolean(), test.GetProperty("cap").GetByte(), test.GetProperty("permission").GetByte());
    byte[] residue = Bytes(test, "residue");
    ulong costCell = test.GetProperty("costCell").GetUInt64();
    ushort originalArc = U16(slot, 30);

    var initial = LegacyNativeWordRecombinationRequestFactory.Create(slot, node, rows[originalArc].Record.Span,
        profile, (_, arc) => rows[arc], tag => headers[tag], contexts, symbols, policy.Permission2309,
        new LegacyNativeWordTransitionCostCell(costCell, 0), residue);
    Require(initial.GetSnapshot().Span.SequenceEqual(Bytes(test, "expectedInitialRequest")) &&
        initial.Cost.Value == test.GetProperty("expectedInitialCost").GetInt32(), $"Initial80 request differs at {cases}.");
    byte[] detached = initial.GetSnapshot().ToArray();
    detached[0] ^= 255;
    Require(!initial.GetSnapshot().Span.SequenceEqual(detached), "Request snapshot aliases live packet.");
    requests++;

    var observedLookups = new List<(int, ushort)>();
    var observedMapping = new List<ushort>();
    var expectedCallbacks = test.GetProperty("callbacks");
    int callbackIndex = 0;
    JsonElement Observe(int kind, LegacyNativeWordRecombinationRequest request)
    {
        Require(callbackIndex < expectedCallbacks.GetArrayLength(), "Unexpected extra native callback.");
        var expected = expectedCallbacks[callbackIndex++];
        Require(expected.GetProperty("kind").GetInt32() == kind &&
            request.GetSnapshot().Span.SequenceEqual(Bytes(expected, "request")) && request.Cost.Value == expected.GetProperty("cost").GetInt32(),
            $"Callback full80/cost ordering differs at stream{cases}/callback{callbackIndex-1}.");
        int arc = request.ReadUInt16(16) - 100;
        return test.GetProperty("scripts")[arc];
    }
    var operations = new LegacyNativeHigherArcTransitionOperations(
        (request, _) =>
        {
            var script = Observe(0, request);
            request.Cost.Value = unchecked(request.Cost.Value + script.GetProperty("d118").GetInt32());
            request.WriteByte(60, (byte)(request.ReadByte(60) ^ 0x11));
            return script.GetProperty("a118").GetBoolean();
        },
        (request, _) =>
        {
            var script = Observe(1, request);
            request.Cost.Value = unchecked(request.Cost.Value + script.GetProperty("d140").GetInt32());
            request.WriteByte(61, unchecked((byte)(request.ReadByte(61) + 3)));
            return script.GetProperty("a140").GetBoolean();
        },
        (request, _) => Keys(Observe(2, request).GetProperty("successors")));
    byte[] originalSlot = slot.ToArray(), originalPrimary = primary.ToArray(), originalSecondary = secondary.ToArray();
    var result = LegacyNativeHigherArcRecombination.Create(slot, node, configuration, rows,
        (ownerNode, arc) => { observedLookups.Add((ownerNode, arc)); return rows[arc]; }, tag => headers[tag],
        profile, contexts, key => { observedMapping.Add(key); return (ushort)(key ^ 0x55); }, policy, symbols,
        operations, costCell, residue, primary, secondary);
    Require(result.HeapSlot.Span.SequenceEqual(Rebind(Bytes(test, "expectedSlot"), false)) &&
        result.PrimaryStorage.Span.SequenceEqual(Rebind(Bytes(test, "expectedPrimary"), true)) &&
        result.SecondaryStorage.Span.SequenceEqual(Rebind(Bytes(test, "expectedSecondary"), true)) &&
        result.LastRequest.Span.SequenceEqual(Bytes(test, "expectedRequest")), $"Full controller buffers differ at {cases}.");
    Require(result.InitialAccepted == test.GetProperty("accepted").GetBoolean() &&
        result.FilteredCandidates == test.GetProperty("filtered").GetInt32() && result.MergedCandidates == test.GetProperty("merged").GetInt32(),
        "Controller status/candidate counts changed.");
    Require(callbackIndex == expectedCallbacks.GetArrayLength() && observedMapping.SequenceEqual(Keys(test.GetProperty("mapping"))) &&
        observedLookups.SequenceEqual(test.GetProperty("lookups").EnumerateArray().Select(pair => (pair[0].GetInt32(), pair[1].GetUInt16()))),
        "Callback, symbolic mapping or owning row lookup order differs.");
    Require(slot.SequenceEqual(originalSlot) && primary.SequenceEqual(originalPrimary) && secondary.SequenceEqual(originalSecondary),
        "Controller changed caller-owned full storage.");

    if (cases == 2)
    {
        // Every callback boundary must propagate cancellation even when the
        // callback returns false and would otherwise terminate successfully.
        foreach (int boundary in Enumerable.Range(0, 6))
            foreach (bool callbackResult in new[] { false, true })
            {
                using var canceled = new CancellationTokenSource();
                bool Cancel() { canceled.Cancel(); return callbackResult; }
                var cancelOperations = new LegacyNativeHigherArcTransitionOperations(
                    (_, _) => boundary == 0 ? Cancel() : true,
                    (_, _) => boundary == 1 ? Cancel() : true,
                    (_, _) => { if (boundary == 2) Cancel(); return new ushort[] { U16(slot, 28) }; });
                try
                {
                    _ = LegacyNativeHigherArcRecombination.Create(slot, node, configuration, rows,
                        (_, arc) => { if (boundary == 3) Cancel(); return rows[arc]; },
                        tag => { if (boundary == 4) Cancel(); return headers[tag]; },
                        profile, contexts, key => { if (boundary == 5) Cancel(); return key; },
                        policy with { CountCapEnabled2601 = false, LongSpanThreshold54 = int.MaxValue },
                        symbols with { Symbol24f4 = ushort.MaxValue }, cancelOperations, costCell, residue, primary, secondary, canceled.Token);
                    throw new Exception($"Callback boundary{boundary} swallowed cancellation.");
                }
                catch (OperationCanceledException) { cancellationGuards++; }
                Require(slot.SequenceEqual(originalSlot) && primary.SequenceEqual(originalPrimary) && secondary.SequenceEqual(originalSecondary),
                    "Canceled producer changed caller-owned buffers.");
            }
        foreach (int boundary in Enumerable.Range(0, 2))
        {
            using var canceled = new CancellationTokenSource();
            try
            {
                _ = LegacyNativeWordRecombinationRequestFactory.Create(slot, node, rows[originalArc].Record.Span,
                    profile, (_, arc) => { if (boundary == 0) canceled.Cancel(); return rows[arc]; },
                    tag => { if (boundary == 1) canceled.Cancel(); return headers[tag]; }, contexts, symbols,
                    policy.Permission2309, new(costCell, 0), residue, canceled.Token);
                throw new Exception("Request callback cancellation was swallowed.");
            }
            catch (OperationCanceledException) { cancellationGuards++; }
        }
    }
    incomingMerges += result.MergedCandidates;
    callbacks += callbackIndex;
    lookups += observedLookups.Count;
    cases++;
}
Console.WriteLine($"PASS {cases} full128/controller streams; {requests} exact80 requests; {callbacks} full packet/cost callbacks; " +
    $"{lookups} owning-row lookups; {incomingMerges} persistent workspace merges; {cancellationGuards} post-callback cancellation guards. Both real35 category profiles.");

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
int endingNode = 0;
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
ushort nextArc = (ushort)Enumerable.Range(0, graph.GetNode(1).Count).First(i => graph.GetNode(1)[i].Span == 0 && graph.GetNode(1)[i].PredecessorArcIndices.Span.Contains(arcIndex));
var nextGlyph = evaluator.Classify(graph.Groups[graph.GetNode(1)[nextArc].GroupId].Ranges.Span);
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
            increment.Symbol, (ushort)currentNode, nextArc, nextArc, (byte)categories.GetSymbolCompositeCategory(increment.Symbol),
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
Console.WriteLine($"ACTUAL RU NN/cache -> 6 stored root parents x {increments.Count} acoustic increments -> " +
    $"{nextChildOrdinal} native initial ordinals/primary-parent pools -> {childArc} default2/2 stored children; " +
    $"{parentLookups} exact parent lookups. Explicit frames,1f8300 mapping and diagnostic scheduling.");

// Actual source graph connections and actual NN/cache costs, with diagnostic
// prepared mode1 and full-packet118/140 callbacks. These callbacks deliberately
// supply explicit cost changes; they do not claim recovered geometry policy.
var connections = new LegacyOrdinaryCandidateConnections(graph);
var higherRows = new List<LegacyNativeStoredWordRecord>(parentRows);
var continuedSlots = new List<byte[]>();
for (ushort parentArc = 0; parentArc < parentRows.Count; parentArc++)
{
    byte[] slot = new byte[128];
    int parentCost = I32(parentRows[parentArc].Record.Span, 16);
    var continuationFields = new LegacyContinuationWordRecordFields(0,
        unchecked(parentCost + increments[0].Cost), increments[0].Cost, ushort.MaxValue,
        new uint[5], 0, 0, 0, 1, 0);
    LegacyContinuationWordRecordProducer.InitializeAllocatedSlot(slot, parentRows[parentArc].Record.Span,
        parentArc, continuationFields, 1, true);
    continuedSlots.Add(slot);
}
byte[] continuedStorage = continuedSlots.SelectMany(value => value).ToArray();
var continuedOrdinals = LegacyNativeWordAlternativeOrdinals.AssignContinuation(continuedStorage, continuedSlots.Count, 0);
continuedStorage = continuedOrdinals.SlotStorage.ToArray();
byte[] continuedSecondary = Enumerable.Repeat((byte)0x6b, continuedSlots.Count * 80).ToArray();
var continuedAlternatives = LegacyNativeContinuationWordAlternativeProducer.CreateUnanchored(continuedStorage,
    continuedSlots.Count, endingNode, 0, 0, 0, actualConfiguration, Parent, actualContexts, key => key, continuedSecondary);
Require(continuedAlternatives.Succeeded, "Actual higher-arc continuation pool producer failed.");
for (ushort index = 0; index < continuedSlots.Count; index++)
{
    ushort destination = (ushort)(parentRows.Count + index);
    var row = LegacyNativeWordRowMaterializer.Materialize(continuedStorage.AsSpan(index * 128, 128), destination,
        actualConfiguration.PrimaryArcLimit230a, 2, 2, default, continuedAlternatives.SecondaryStorage.Span);
    Require(row.PrimaryAlternatives.IsEmpty && row.SecondaryAlternatives.Length == 80,
        "Actual stored higher row incorrectly retained a primary pool.");
    higherRows.Add(row);
}
byte[] lk = File.ReadAllBytes(args[2]);
var dictionaryPrefix = LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lk.AsSpan(606964, 4565174 - 606964));
uint languageOrder = LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lk.AsSpan(2513123, 4565174 - 2513123));
var dictionaryDescriptor = LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(dictionaryPrefix, model.Lexicon, languageOrder, true);
int actualCallbacks = 0, actualQueries = 0, actualMerges = 0;
var actualOperations = LegacyNativeHigherArcTransitionOperations.ForOrdinaryConnections(connections,
    (request, _) =>
    {
        Require(request.ReadUInt32(0) == currentNode && request.ReadInt32(8) == endingNode &&
            request.ReadUInt16(12) == nextArc && request.ReadUInt16(16) == arcIndex &&
            request.ReadUInt32(64) == ((dictionaryDescriptor.Flags >> 9) & 15) && request.ReadUInt32(68) == request.ReadUInt32(64),
            "Actual source/graph/header lineage was lost before118 callback.");
        request.Cost.Value = unchecked(request.Cost.Value + 17);
        actualCallbacks++;
        return true;
    },
    (request, _) => { request.Cost.Value = unchecked(request.Cost.Value - 3); actualCallbacks++; return true; });
var trackedOperations = actualOperations with
{
    SuccessorsF0F8 = (request, token) => { actualQueries++; return actualOperations.SuccessorsF0F8(request, token); }
};
for (ushort original = (ushort)parentRows.Count; original < higherRows.Count; original++)
{
    var parent = higherRows[original];
    var increment = increments[0];
    var fields = new LegacyInitialWordRecordFields(0, actualContexts.ResolveInitial(increment.Symbol).Identity,
        unchecked(I32(parent.Record.Span, 16) + increment.Cost), increment.Cost,
        increment.Symbol, (ushort)currentNode, nextArc, nextArc, (byte)categories.GetSymbolCompositeCategory(increment.Symbol),
        original, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, endingNode, original);
    byte[] fullSlot = new byte[128];
    LegacyInitialWordRecordProducer.InitializeAllocatedPrefix(fields, 1).CopyTo(fullSlot, 0);
    var combined = LegacyNativeHigherArcRecombination.Create(fullSlot, currentNode,
        actualConfiguration with { Mode22f6 = 1 }, higherRows,
        (ownerNode, arc) => { Require(ownerNode == endingNode, "Actual request fabricated an owner node."); return higherRows[arc]; },
        _ => dictionaryDescriptor.Flags, model.CursiveCategoryProfiles[1], actualContexts, key => key,
        new(ushort.MaxValue, int.MaxValue, false, 0, 0), new(65527, 65531, 65532, ushort.MaxValue),
        trackedOperations, 0x1122334455667788, Enumerable.Repeat((byte)0x9d, 80).ToArray(), new byte[80], new byte[80]);
    Require(combined.InitialAccepted && combined.MergedCandidates == parentRows.Count - 1,
        "Actual source graph successors did not reach every compatible higher row.");
    actualMerges += combined.MergedCandidates;
    for (int alternative = 0; alternative < 2; alternative++)
    {
        var choice = combined.SecondaryStorage.Span.Slice(alternative * 40, 40);
        if (I32(choice, 0) >= 0x4000000) continue;
        var prefix = higherRows[U16(choice, 24)].SecondaryAlternatives.Span.Slice((choice[27] & 127) * 40, 40);
        Require(ReadPath(actualContexts, BinaryPrimitives.ReadUInt64LittleEndian(choice[8..])).SequenceEqual(
            ReadPath(actualContexts, BinaryPrimitives.ReadUInt64LittleEndian(prefix[8..])).Concat(new[] { increment.Symbol })),
            "Final higher-arc refresh lost native chosen parent/suffix lineage.");
    }
    var finalOrdinal = LegacyNativeWordAlternativeOrdinals.AssignInitial(combined.HeapSlot.Span, 1, 0);
    var stored = LegacyNativeWordRowMaterializer.Materialize(finalOrdinal.SlotStorage.Span, 0, 1, 2, 2,
        combined.PrimaryStorage.Span, combined.SecondaryStorage.Span);
    Require(stored.SecondaryAlternatives.Span.SequenceEqual(combined.SecondaryStorage.Span) &&
        stored.Record.Span.SequenceEqual(combined.HeapSlot.Span[..88]), "Full higher-arc result did not feed native materialization intact.");
}
Console.WriteLine($"ACTUAL RU source graph/NN/cache -> {parentRows.Count} real continuation higher rows -> " +
    $"{actualQueries} native connection queries / {actualCallbacks} explicit full80 callbacks / {actualMerges} merges -> materialization PASS. " +
    "Mode1/correction costs are diagnostic inputs; native118/140 and complete word-controller scheduling remain unfinished.");
