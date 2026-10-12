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

int pathCount = 0;
foreach (var test in document.RootElement.GetProperty("paths").EnumerateArray())
{
    var contexts = new LegacySymbolContextFactory();
    ulong prefix = Path(contexts, Keys(test.GetProperty("prefix")));
    ulong suffix = Path(contexts, Keys(test.GetProperty("suffix")));
    var calls = new List<ushort>();
    ushort Map(ushort key) { calls.Add(key); return (ushort)(key ^ 0x55); }
    ulong result = test.GetProperty("mapped").GetBoolean()
        ? LegacyNativeWordContextComposition.CombineMapped(contexts, prefix, suffix, Map)
        : LegacyNativeWordContextComposition.CombineRaw(contexts, prefix, suffix);
    Require(ReadPath(contexts, result).SequenceEqual(Keys(test.GetProperty("expected"))),
        $"Symbol composition differs at {pathCount}.");
    Require(calls.SequenceEqual(Keys(test.GetProperty("calls"))),
        $"Native map callback dispatch differs at {pathCount}.");
    // Repeated composition resolves the same canonical interned identity.
    ulong repeat = test.GetProperty("mapped").GetBoolean()
        ? LegacyNativeWordContextComposition.CombineMapped(contexts, prefix, suffix, Map)
        : LegacyNativeWordContextComposition.CombineRaw(contexts, prefix, suffix);
    Require(result == repeat, "Repeated context composition changed identity.");
    pathCount++;
}

int rootCount = 0;
foreach (var test in document.RootElement.GetProperty("roots").EnumerateArray())
{
    var contexts = new LegacySymbolContextFactory();
    byte[] slots = Convert.FromHexString(test.GetProperty("slots").GetString()!);
    byte[] primary = Convert.FromHexString(test.GetProperty("primary").GetString()!);
    byte[] secondary = Convert.FromHexString(test.GetProperty("secondary").GetString()!);
    byte[] expectedPrimary = Convert.FromHexString(test.GetProperty("expectedP").GetString()!);
    byte[] expectedSecondary = Convert.FromHexString(test.GetProperty("expectedS").GetString()!);
    int count = test.GetProperty("count").GetInt32();
    ushort pc = test.GetProperty("pc").GetUInt16(), sc = test.GetProperty("sc").GetUInt16();
    for (int index = 0; index < count; index++)
    {
        var slot = slots.AsSpan(index * 128, 128);
        ulong suffix = Path(contexts, Keys(test.GetProperty("suffixes")[index]));
        BinaryPrimitives.WriteUInt64LittleEndian(slot[8..], suffix);
        ulong mapped = Path(contexts, Keys(test.GetProperty("mapped")[index]));
        ulong rawIdentity = Path(contexts, Keys(test.GetProperty("raw")[index]));
        int ordinal = BinaryPrimitives.ReadInt32LittleEndian(slot[0x58..]);
        foreach (var pair in new[] { (pc, expectedPrimary), (sc, expectedSecondary) })
        {
            if (pair.Item1 == 1) continue;
            int at = ordinal * pair.Item1 * 40;
            BinaryPrimitives.WriteUInt64LittleEndian(pair.Item2.AsSpan(at + 8), mapped);
            BinaryPrimitives.WriteUInt64LittleEndian(pair.Item2.AsSpan(at + 16), rawIdentity);
        }
    }
    byte[] before = slots.ToArray();
    var config = LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with
    {
        PrimaryAlternativeCount2402 = pc, SecondaryAlternativeCount2404 = sc
    };
    var produced = LegacyNativeRootWordAlternativeProducer.Create(slots, count,
        test.GetProperty("node").GetUInt32(), config, contexts, key => (ushort)(key ^ 0x55), primary, secondary);
    Require(produced.PrimaryStorage.Span.SequenceEqual(expectedPrimary), $"Primary full pool bytes differ at {rootCount}.");
    Require(produced.SecondaryStorage.Span.SequenceEqual(expectedSecondary), $"Secondary full pool bytes differ at {rootCount}.");
    Require(slots.SequenceEqual(before), "Root pool producer modified 128-byte records.");
    rootCount++;
}

// Missing context allocation is native null; a later ordinary symbol may still
// resolve an already interned root. FFFD empty/raw differs from mapped controls.
var limited = new LegacySymbolContextFactory(1);
ulong single = Path(limited, new ushort[] { 42 });
Require(LegacyNativeWordContextComposition.CombineMapped(limited, 0, single, _ => 43) == 0,
    "Allocation-bound failure synthesized a context identity.");
Require(LegacyNativeWordContextComposition.CombineRaw(limited, 0, single) == single,
    "Existing context did not resolve at allocator capacity.");
var cancelled = new LegacySymbolContextFactory();
ulong cancelledSuffix = Path(cancelled, new ushort[] { 10, 11 });
int beforeCancel = cancelled.NodeCount;
try
{
    LegacyNativeWordContextComposition.CombineMapped(cancelled, 0, cancelledSuffix, key => key,
        new CancellationToken(true));
    throw new Exception("Cancelled context composition accepted.");
}
catch (OperationCanceledException) { }
Require(cancelled.NodeCount == beforeCancel, "Pre-cancellation inserted context nodes.");
// Failed insertion does not halt the recursive traversal: the next token
// can resolve an existing root after the null result.
var recovery = new LegacySymbolContextFactory(2);
ulong recoverySuffix = Path(recovery, new ushort[] { 42, 43 });
Require(LegacyNativeWordContextComposition.CombineMapped(recovery, 0, recoverySuffix,
    key => key == 42 ? (ushort)44 : (ushort)42) == recovery.ResolveInitial(42).Identity,
    "Native null intermediate result did not resume from the empty prefix.");
var foreign = new LegacySymbolContextFactory();
try
{
    LegacyNativeWordContextComposition.CombineRaw(foreign, 0, single);
    throw new Exception("Foreign context identity accepted.");
}
catch (ArgumentException) { }
var noopSlot = new byte[128];
noopSlot[0x20] = 99;
BinaryPrimitives.WriteInt32LittleEndian(noopSlot.AsSpan(0x58), -1);
var noop = LegacyNativeRootWordAlternativeProducer.Create(noopSlot, 1, 0,
    LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with
    { PrimaryAlternativeCount2402 = 1, SecondaryAlternativeCount2404 = 1 },
    new LegacySymbolContextFactory(), _ => throw new Exception("Disabled pool called mapper."),
    new byte[40], new byte[40]);
Require(noop.PrimaryStorage.Span.SequenceEqual(new byte[40]), "Disabled pool branch mutated storage.");
var refuseContexts = new LegacySymbolContextFactory();
ulong known = Path(refuseContexts, new ushort[] { 8 });
var nonrootSlot = new byte[128];
BinaryPrimitives.WriteUInt64LittleEndian(nonrootSlot.AsSpan(8), known);
try
{
    LegacyNativeRootWordAlternativeProducer.Create(nonrootSlot, 1, 1,
        LegacyNativeWordControllerConfiguration.CreateFresh20baa0(), refuseContexts,
        _ => throw new Exception("Unsupported parent called mapper."), new byte[80], new byte[80]);
    throw new Exception("Unsupported parent alternative path accepted.");
}
catch (NotSupportedException) { }
Require(refuseContexts.NodeCount == 1, "Unsupported parent branch changed shared index.");
try
{
    LegacyNativeRootWordAlternativeProducer.Create(nonrootSlot, 1, 0,
        LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with { Trace2368 = 1 },
        refuseContexts, key => key, new byte[80], new byte[80]);
    throw new Exception("Unimplemented trace branch accepted.");
}
catch (NotSupportedException) { }

Console.WriteLine($"PASS {pathCount} independent symbolic paths / {rootCount} full-byte root pools; null allocation/recovery, stable identity, foreign IDs, no-op/parent/trace guards, cancellation.");

// Actual RU NN/cache ? native root pool creation ? duplicate merges ? stored rows.
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


var retained = actualHeaps.SelectMany(heap =>
    Enumerable.Range(0, heap.Count).Select(heap.GetSlotSnapshot)).ToArray();
Require(retained.Length > 0 && actualPromotions > 0, "Actual scored root stream did not route/promote.");
var actualStorage = new byte[(retained.Length + 1) * 128];
for (int index = 0; index < retained.Length; index++)
    retained[index].Span.CopyTo(actualStorage.AsSpan(index * 128));
// Diagnostic duplicate exercises the proven root pools as actual merge inputs.
retained[0].Span.CopyTo(actualStorage.AsSpan(retained.Length * 128));
BinaryPrimitives.WriteInt32LittleEndian(actualStorage.AsSpan(retained.Length * 128 + 0x10),
    BinaryPrimitives.ReadInt32LittleEndian(retained[0].Span[0x10..]) + 100);
for (int index = 0; index <= retained.Length; index++)
    BinaryPrimitives.WriteInt32LittleEndian(actualStorage.AsSpan(index * 128 + 0x58), index);
byte[] actualPrimary = Enumerable.Repeat((byte)0xa7, (retained.Length + 1) * 80).ToArray();
byte[] actualSecondary = Enumerable.Repeat((byte)0x5b, (retained.Length + 1) * 80).ToArray();
var actualConfig = LegacyNativeWordControllerConfiguration.CreateFresh20baa0() with
{
    PrimaryArcLimit230a = checked((ushort)(retained.Length + 1))
};
// This explicit mapper equals the literal1f8300 callback branch (movzx eax,dx;
// ret). Automatic2144f0 callback selection is a separate producer, not inferred.
var actualPools = LegacyNativeRootWordAlternativeProducer.Create(actualStorage, retained.Length + 1,
    (uint)endingNode, actualConfig, actualContexts, key => key, actualPrimary, actualSecondary);
for (int index = 0; index <= retained.Length; index++)
{
    var slot = actualStorage.AsSpan(index * 128, 128);
    ulong originalContext = BinaryPrimitives.ReadUInt64LittleEndian(slot[8..]);
    var first = actualPools.PrimaryStorage.Span.Slice(index * 80, 40);
    var second = actualPools.SecondaryStorage.Span.Slice(index * 80, 40);
    Require(BinaryPrimitives.ReadUInt64LittleEndian(first[8..]) == originalContext &&
        BinaryPrimitives.ReadUInt64LittleEndian(first[16..]) == originalContext &&
        BinaryPrimitives.ReadUInt64LittleEndian(second[8..]) == originalContext,
        "Actual root alternatives did not use canonical source context identities.");
    Require(BinaryPrimitives.ReadInt32LittleEndian(first) == BinaryPrimitives.ReadInt32LittleEndian(slot[0x10..]) &&
        (first[31] & 32) != 0 && (second[31] & 32) == (actualSecondary[index * 80 + 31] & 32),
        "Actual root cost or primary/secondary flag provenance differs.");
}
var ordered = LegacyNativeWordAlternativeOrdering.Prepare(actualStorage, retained.Length + 1,
    false, actualConfig, new uint[] { 0 }, false,
    new LegacyNativeWordAlternativeBuffer(2, actualPools.PrimaryStorage.Span),
    new LegacyNativeWordAlternativeBuffer(2, actualPools.SecondaryStorage.Span));
Require(ordered.UniqueCount == retained.Length, "Actual root duplicate did not merge through produced pools.");
for (int index = 0; index < ordered.Heap.Count; index++)
{
    var slot = ordered.Heap.GetSlotSnapshot(index);
    var row = LegacyNativeWordRowMaterializer.Materialize(slot.Span, (ushort)index,
        actualConfig.PrimaryArcLimit230a, 2, 2, ordered.PrimaryAlternativeStorage.Span, ordered.SecondaryAlternativeStorage.Span);
    Require(row.PrimaryAlternatives.Length == 80 && row.SecondaryAlternatives.Length == 80,
        "Default2/2 root pool did not survive stored row materialization.");
    Require(BinaryPrimitives.ReadUInt64LittleEndian(row.PrimaryAlternatives.Span[8..]) ==
        BinaryPrimitives.ReadUInt64LittleEndian(slot.Span[8..]),
        "Materialized root row lost its true interned context identity.");
}
Console.WriteLine($"ACTUAL RU NN/cache ? {retained.Length + 1} canonical-context root pools ? duplicate merge ? " +
    $"{ordered.Heap.Count} stored rows with default2/2 alternatives. Explicit ordinal/callback selection.");
