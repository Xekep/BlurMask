using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 5)
    throw new ArgumentException("controls.json acoustic.res lexical.res engine-profile.json separation-profile.json");

using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases = 0, routes = 0, promotions = 0, acceptedPromotions = 0;
foreach (var test in controls.RootElement.EnumerateArray())
{
    var capacities = test.GetProperty("capacities");
    var initial = test.GetProperty("initial");
    var heaps = Enumerable.Range(0, capacities.GetArrayLength()).Select(index =>
        new LegacyNativeWordRecordHeap(capacities[index].GetInt32(),
            Convert.FromBase64String(initial[index].GetString()!))).ToArray();
    var buckets = test.GetProperty("buckets").EnumerateArray().Select(value => value.GetByte()).ToArray();
    var router = new LegacyNativeWordHeapRouter(heaps, buckets,
        test.GetProperty("overrideState").GetInt32(), test.GetProperty("overrideHeap").GetUInt32());
    int index = 0;
    foreach (var step in test.GetProperty("steps").EnumerateArray())
    {
        int state = step.GetProperty("state").GetInt32();
        byte secondary = step.GetProperty("secondary").GetByte();
        bool initialized = false;
        var allocation = router.TryAllocate(state, secondary, step.GetProperty("override").GetBoolean(),
            step.GetProperty("cost").GetInt32(), slot =>
            {
                BinaryPrimitives.WriteUInt32LittleEndian(slot[4..], (uint)index);
                slot[0x27] = (byte)(secondary & 1);
                BinaryPrimitives.WriteUInt32LittleEndian(slot[0x50..], 0xfedc0000u | (uint)state);
                initialized = true;
            });
        Require(allocation.Accepted == step.GetProperty("accepted").GetBoolean() &&
            initialized == allocation.Accepted && allocation.HeapIndex == step.GetProperty("target").GetInt32() &&
            allocation.PromotionAttempted == step.GetProperty("promotion").GetBoolean() &&
            allocation.PromotionAccepted == step.GetProperty("promoted").GetBoolean(),
            "Native bucket route, override, promotion or rejection differs.");
        var expected = step.GetProperty("slots");
        for (int heapIndex = 0; heapIndex < heaps.Length; heapIndex++)
        {
            Require(heaps[heapIndex].Count == expected[heapIndex].GetArrayLength(), "Heap count differs after routing.");
            for (int slot = 0; slot < heaps[heapIndex].Count; slot++)
                Require(heaps[heapIndex].GetSlotSnapshot(slot).Span.SequenceEqual(
                    Convert.FromBase64String(expected[heapIndex][slot].GetString()!)),
                    "Whole-record promotion, native heap order or retained slot tail differs.");
        }
        routes++;
        promotions += allocation.PromotionAttempted ? 1 : 0;
        acceptedPromotions += allocation.PromotionAccepted ? 1 : 0;
        index++;
    }
    cases++;
}

// Fail before mutation for invalid live metadata or cancellation.
var guardHeaps = new[] { new LegacyNativeWordRecordHeap(1), new LegacyNativeWordRecordHeap(1) };
var guardRouter = new LegacyNativeWordHeapRouter(guardHeaps, new byte[] { 0, 255 }, 0, 0xffffffff);
try
{
    guardRouter.TryAllocate(0, 1, false, 0, _ => throw new Exception("Cancelled initializer executed."),
        new CancellationToken(true));
    throw new Exception("Cancellation accepted.");
}
catch (OperationCanceledException) { }
try
{
    guardRouter.TryAllocate(1, 1, false, 0, _ => throw new Exception("Invalid index initializer executed."));
    throw new Exception("Invalid bucket accepted.");
}
catch (ArgumentException) { }
try
{
    _ = new LegacyNativeWordHeapRouter(new[] { guardHeaps[0], guardHeaps[0] }, new byte[] { 0 }, 0, 0);
    throw new Exception("Aliased native heap storage accepted.");
}
catch (ArgumentException) { }
Require(guardHeaps.All(heap => heap.Count == 0), "Rejected metadata or cancellation mutated a heap.");

// Actual resource output uses explicit diagnostic bucket metadata/capacities.
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

// Fresh knowledge property scale zero disables both route and stored flag.
var zeroHeaps = new[] { new LegacyNativeWordRecordHeap(1), new LegacyNativeWordRecordHeap(1) };
var zeroRouter = new LegacyNativeWordHeapRouter(zeroHeaps, new byte[] { 0 }, 0, uint.MaxValue);
var forced = zeroRouter.TryInsertInitialRecord(0, false, firstFields!, -0f, false);
Require(forced.Accepted && forced.HeapIndex == 0 && zeroHeaps[1].Count == 0 &&
    (zeroHeaps[0].GetSlotSnapshot(0).Span[0x27] & 1) == 0,
    "Zero scale did not force the native route and stored secondary flag to zero.");

Console.WriteLine($"ACTUAL {expectedCosts.Count} real acoustic candidates -> {initializerCount} routed allocations; " +
    $"{actualPromotionAttempts} promotion attempts/{actualPromotions} accepted, {primaryFallbacks} primary routes, " +
    $"{retained.Length} retained records with category/source and history transport. Explicit bucket metadata/capacities, " +
    "scale 1 exercises secondary routing; zero scale verified separately. No native winner or quality claim.");
Console.WriteLine($"PASS {cases} independent router streams/{routes} routes; {promotions} promotions/" +
    $"{acceptedPromotions} accepted; full record movement, overrides, odd descriptor counts, zero capacities, " +
    "signed/tied costs, primary fallback, cancellation, invalid metadata and aliased-storage guards.");

static void Require(bool value, string message)
{
    if (!value)
        throw new InvalidDataException(message);
}
