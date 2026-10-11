using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 5)
    throw new ArgumentException("controls.json acoustic.res lexical.res engine-profile.json separation-profile.json");

using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases = 0, allocations = 0, accepted = 0, evicted = 0;

foreach (var test in controls.RootElement.GetProperty("heaps").EnumerateArray())
{
    var initial = Convert.FromBase64String(test.GetProperty("initial").GetString()!);
    var heap = new LegacyNativeWordRecordHeap(test.GetProperty("capacity").GetInt32(), initial);
    int index = 0;

    foreach (var step in test.GetProperty("steps").EnumerateArray())
    {
        byte[]? removed = null;
        bool initialized = false;
        bool allocated = heap.TryAllocate(step.GetProperty("cost").GetInt32(), slot =>
        {
            BinaryPrimitives.WriteUInt32LittleEndian(slot[4..], (uint)index);
            initialized = true;
        }, root =>
        {
            Require(root.Span.SequenceEqual(heap.GetSlotSnapshot(0).Span),
                "Eviction callback ran after native root movement.");
            removed = root.ToArray();
        });

        Require(allocated == step.GetProperty("accepted").GetBoolean() && initialized == allocated,
            "Native allocation rejection or initializer activation differs.");
        var expectedRemoved = step.GetProperty("evicted");
        Require(expectedRemoved.ValueKind == JsonValueKind.Null ? removed is null :
            removed is not null && removed.AsSpan().SequenceEqual(
                Convert.FromBase64String(expectedRemoved.GetString()!)),
            "Native evicted root identity differs.");

        var expectedSlots = step.GetProperty("slots");
        Require(heap.Count == expectedSlots.GetArrayLength(), "Native retained heap count differs.");
        for (int slot = 0; slot < heap.Count; slot++)
        {
            var expected = Convert.FromBase64String(expectedSlots[slot].GetString()!);
            Require(heap.GetSlotSnapshot(slot).Span.SequenceEqual(expected),
                "Native heap order, whole-slot movement, zeroing or retained78..7f differs.");
        }
        var actualCosts = Enumerable.Range(0, heap.Count).Select(slot =>
            BinaryPrimitives.ReadInt32LittleEndian(heap.GetSlotSnapshot(slot).Span[0x10..])).Order();
        var best = step.GetProperty("best").EnumerateArray().Select(value => value.GetInt32());
        Require(actualCosts.SequenceEqual(best), "Native retained costs differ from mathematical best-K.");

        index++;
        allocations++;
        accepted += allocated ? 1 : 0;
        evicted += removed is null ? 0 : 1;
    }
    cases++;
}

// Native initialized-prefix anchor/history lookup to the nontracing heap tail.
int tailCases = 0;
foreach (var test in controls.RootElement.GetProperty("tails").EnumerateArray())
{
    var slot = Convert.FromBase64String(test.GetProperty("slot").GetString()!);
    var records = test.GetProperty("records");
    var calls = new List<string>();
    ReadOnlyMemory<byte> Lookup(int node, ushort arc)
    {
        string key = $"{node}:{arc}";
        calls.Add(key);
        return Convert.FromBase64String(records.GetProperty(key).GetString()!);
    }
    LegacyInitialWordHeapTail.Initialize(slot, test.GetProperty("suppressed").GetBoolean(), Lookup);
    Require(slot.AsSpan().SequenceEqual(Convert.FromBase64String(test.GetProperty("expected").GetString()!)),
        "Native anchor/history tail fields or untouched heap bytes differ.");
    var expectedCalls = test.GetProperty("calls").EnumerateArray().Select(value => value.GetString());
    Require(calls.SequenceEqual(expectedCalls), "Native anchor/history lookup order differs.");
    tailCases++;
}

// Cancellation and cost ownership are explicit adapter guards.
var guarded = new LegacyNativeWordRecordHeap(1);
try
{
    guarded.TryAllocate(0, _ => { }, cancellationToken: new CancellationToken(true));
    throw new Exception("Cancellation ignored.");
}
catch (OperationCanceledException) { }
Require(guarded.Count == 0, "Cancelled allocation mutated heap state.");
try
{
    guarded.TryAllocate(0, slot => BinaryPrimitives.WriteInt32LittleEndian(slot[0x10..], 1));
    throw new Exception("Initializer changed heap cost without rejection.");
}
catch (InvalidOperationException) { }

var reentrant = new LegacyNativeWordRecordHeap(1);
Require(reentrant.TryAllocate(1, _ => { }), "Guard fixture did not allocate its first slot.");
bool refusedReentry = false;
Require(reentrant.TryAllocate(0, _ => { }, _ =>
{
    try
    {
        reentrant.TryAllocate(-1, _ => { });
        throw new Exception("Reentrant mutation accepted.");
    }
    catch (InvalidOperationException) { refusedReentry = true; }
}), "Outer allocation did not retain its accepted slot after the guarded callback.");
Require(refusedReentry, "Reentrant mutation guard did not run.");

var unavailableAnchor = new byte[0x80];
BinaryPrimitives.WriteUInt16LittleEndian(unavailableAnchor.AsSpan(0x3c), 1);
try
{
    LegacyInitialWordHeapTail.Initialize(unavailableAnchor, false);
    throw new Exception("Unavailable native anchor was invented.");
}
catch (NotSupportedException) { }
try
{
    LegacyInitialWordHeapTail.Initialize(unavailableAnchor, false, (_, _) => new byte[0x57]);
    throw new Exception("Truncated native anchor accepted.");
}
catch (ArgumentException) { }

// Actual glyph/context/cache ->native record initializer ->native slot heap.
// Source tag/other record state remain explicit diagnostic fields. This tests
// lifecycle transport, not virtualF8 filtering or a native language winner.
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
var actualHeap = new LegacyNativeWordRecordHeap(4);
var sourceBoundaries = LegacyPreparedSourceBoundarySnapshot.CreateFreshOrdinary(source, graph);
var expectedCosts = new List<int>();
int initializerCount = 0;
int actualEvictions = 0;

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
    expectedCosts.Add(score.TotalCost);
    if (actualHeap.TryInsertInitialRecord(fields, 0, suppressPreviousContext2609: false,
        onEvictedRoot: _ => actualEvictions++))
        initializerCount++;
}

var retainedCosts = Enumerable.Range(0, actualHeap.Count).Select(index =>
    BinaryPrimitives.ReadInt32LittleEndian(actualHeap.GetSlotSnapshot(index).Span[0x10..])).Order();
Require(retainedCosts.SequenceEqual(expectedCosts.Order().Take(actualHeap.Capacity)),
    "Actual native glyph/record heap costs differ from retained best-K.");
for (int index = 0; index < actualHeap.Count; index++)
{
    var record = actualHeap.GetSlotSnapshot(index);
    ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(record.Span[0x24..]);
    var reference = LegacyPreparedWordPathReference.Read(record.Span[..0x58], endingNode);
    var projection = LegacyWordPathBoundaryCost.Evaluate(reference, categories, graph.NodeCount,
        false, 8, sourceBoundaries.GetBoundary);
    Require(projection.MappedCategory == categories.GetSymbolFirstId(symbol),
        "Native heap retained the wrong initialized category/reference.");
    Require(BinaryPrimitives.ReadUInt64LittleEndian(record.Span[0x58..]) == ulong.MaxValue &&
        BinaryPrimitives.ReadInt32LittleEndian(record.Span[0x68..]) == -1 &&
        BinaryPrimitives.ReadUInt64LittleEndian(record.Span[0x70..]) == 0,
        "Actual nontracing root initial-word tail was not initialized.");
}
Require(actualHeap.Count > 0 && initializerCount > 0, "Actual native record heap transport was empty.");
Console.WriteLine($"ACTUAL {expectedCosts.Count} native acoustic candidates ->{initializerCount} slot allocations " +
    $"/{actualEvictions} evictions ->{actualHeap.Count} retained initialized records, category/source references and nontracing root tails. " +
    "Explicit record tags/source span and original line frame; no native winner or handwriting accuracy claim.");

Console.WriteLine($"PASS {cases} independent native heap streams/{allocations} allocations, " +
    $"{accepted} accepted/{evicted} evicted roots; signed best-K, right-child ties, full0x80 movement, " +
    "0..77 zeroing/78..7f retention, rejection/initializer order and cancel/cost guards. " +
    $"{tailCases} independent native anchor/history tails. " +
    "Heap bucket/winner policy remains separate.");

static void Require(bool value, string message)
{
    if (!value)
        throw new InvalidDataException(message);
}
