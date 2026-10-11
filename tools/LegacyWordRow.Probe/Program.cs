using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 5)
    throw new ArgumentException("controls.json acoustic.res lexical.res engine-profile.json separation-profile.json");

using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases = 0, primaryBlocks = 0, secondaryBlocks = 0;
foreach (var test in controls.RootElement.EnumerateArray())
{
    var slot = Convert.FromBase64String(test.GetProperty("slot").GetString()!);
    var primary = Convert.FromBase64String(test.GetProperty("primary").GetString()!);
    var secondary = Convert.FromBase64String(test.GetProperty("secondary").GetString()!);
    var materialized = LegacyNativeWordRowMaterializer.Materialize(slot,
        test.GetProperty("arc").GetUInt16(), test.GetProperty("limit").GetUInt16(),
        test.GetProperty("primaryCount").GetUInt16(), test.GetProperty("secondaryCount").GetUInt16(),
        primary, secondary);
    Require(materialized.Record.Span.SequenceEqual(Convert.FromBase64String(test.GetProperty("expected").GetString()!)) &&
        materialized.PrimaryAlternatives.Span.SequenceEqual(Convert.FromBase64String(test.GetProperty("expectedPrimary").GetString()!)) &&
        materialized.SecondaryAlternatives.Span.SequenceEqual(Convert.FromBase64String(test.GetProperty("expectedSecondary").GetString()!)),
        "Native stored-row prefix/anchor or alternative matrix block differs.");
    var ownedRecord = materialized.Record.ToArray();
    var ownedPrimary = materialized.PrimaryAlternatives.ToArray();
    var ownedSecondary = materialized.SecondaryAlternatives.ToArray();
    Array.Fill(slot, (byte)0);
    Array.Fill(primary, (byte)0);
    Array.Fill(secondary, (byte)0);
    Require(materialized.Record.Span.SequenceEqual(ownedRecord) &&
        materialized.PrimaryAlternatives.Span.SequenceEqual(ownedPrimary) &&
        materialized.SecondaryAlternatives.Span.SequenceEqual(ownedSecondary),
        "Stored word row retained mutable source slot/table ownership.");
    primaryBlocks += materialized.PrimaryAlternatives.IsEmpty ? 0 : 1;
    secondaryBlocks += materialized.SecondaryAlternatives.IsEmpty ? 0 : 1;
    cases++;
}

var guardSlot = new byte[0x80];
BinaryPrimitives.WriteInt32LittleEndian(guardSlot.AsSpan(0x58), -1);
Require(LegacyNativeWordRowMaterializer.Materialize(guardSlot, 0, 1, 1, 1).Record.Length == 0x58,
    "Inactive optional alternatives incorrectly read the unassigned slot ordinal.");
try
{
    LegacyNativeWordRowMaterializer.Materialize(guardSlot, 0, 1, 2, 1, new byte[80]);
    throw new Exception("Missing enabled alternative ordinal accepted.");
}
catch (ArgumentException) { }
BinaryPrimitives.WriteInt32LittleEndian(guardSlot.AsSpan(0x58), 2);
try
{
    LegacyNativeWordRowMaterializer.Materialize(guardSlot, 0, 1, 2, 1, new byte[80]);
    throw new Exception("Truncated alternative matrix accepted.");
}
catch (ArgumentException) { }
try
{
    LegacyNativeWordRowMaterializer.Materialize(guardSlot, 0, 1, 1, 1,
        cancellationToken: new CancellationToken(true));
    throw new Exception("Cancelled word-row materialization accepted.");
}
catch (OperationCanceledException) { }

// Native row writer closes the previously explicit heap-slot -> parent-row gap.
// Heap/row enumeration order remains an explicit diagnostic driver.
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

var rows = retained.Select((record, index) =>
    LegacyNativeWordRowMaterializer.Materialize(record.Span, (ushort)index, (ushort)retained.Length, 1, 1)
        .Record.ToArray()).ToArray();
for (int index = 0; index < rows.Length; index++)
    Require(BinaryPrimitives.ReadUInt16LittleEndian(rows[index].AsSpan(0x3c)) == index,
        "Root row materialization did not replace the missing anchor with its actual stored arc.");
int actualParentLookups = 0, actualHistoryLookups = 0;
ReadOnlyMemory<byte> RowLookup(int node, ushort arc)
{
    Require(node == endingNode && arc < rows.Length, "Continuation requested an invented row or arc.");
    actualHistoryLookups++;
    return rows[arc];
}

// A second actual classification provides explicit acoustic increments. The
// Cartesian parent/increment enumeration is a diagnostic driver, not native
// word-search scheduling or inferred language-state costs.
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
    if (category < 0)
        continue;
    var score = nativeScorer.Evaluate(nextScored.Cache.Costs.Span, symbol,
        nextScored.CompositeCategoryCosts.Span, category, 0, 0, LegacyAcousticFixedLog.ImpossibleCost);
    if (score.Accepted)
        increments.Add((symbol, score.TotalCost));
}
Require(increments.Count > 0, "Second actual glyph produced no acoustic increment.");

var continuationHeaps = new[] { new LegacyNativeWordRecordHeap(4), new LegacyNativeWordRecordHeap(4) };
var continuationRouter = new LegacyNativeWordHeapRouter(continuationHeaps, new byte[] { 0 }, 0, uint.MaxValue);
var continuationCosts = new List<int>();
int continuationAccepted = 0, continuationPromotions = 0;
for (ushort parentArc = 0; parentArc < rows.Length; parentArc++)
{
    int parentCost = BinaryPrimitives.ReadInt32LittleEndian(rows[parentArc].AsSpan(0x10));
    foreach (var increment in increments)
    {
        int totalCost = unchecked(parentCost + increment.Cost);
        continuationCosts.Add(totalCost);
        var fields = new LegacyContinuationWordRecordFields(
            SourceTag: 0, CandidateCost10: totalCost, IncrementalCost14: increment.Cost,
            State3e: increment.Symbol, Payload40: new uint[] { increment.Symbol, (uint)parentCost,
                (uint)increment.Cost, (uint)endingNode, parentArc },
            Auxiliary08: parentArc, Context28: increment.Symbol, SmallState21: (byte)increment.Symbol,
            RequestedSecondaryFlag: 1, Auxiliary5c: 0x1234567800000000ul | parentArc);
        var allocation = LegacyContinuationWordRecordProducer.TryInsert(continuationRouter, endingNode,
            parentArc, (node, arc) =>
            {
                actualParentLookups++;
                return RowLookup(node, arc);
            }, fields, 1, false);
        continuationAccepted += allocation.Accepted ? 1 : 0;
        continuationPromotions += allocation.PromotionAccepted ? 1 : 0;
    }
}
var continuations = continuationHeaps.SelectMany(heap =>
    Enumerable.Range(0, heap.Count).Select(heap.GetSlotSnapshot)).ToArray();
Require(continuations.Select(record => BinaryPrimitives.ReadInt32LittleEndian(record.Span[0x10..])).Order()
    .SequenceEqual(continuationCosts.Order().Take(continuationHeaps.Sum(heap => heap.Capacity))),
    "Continuation route/promotion lost a better explicit acoustic path.");
foreach (var continuation in continuations)
{
    ushort parentArc = BinaryPrimitives.ReadUInt16LittleEndian(continuation.Span[0x1e..]);
    var parent = rows[parentArc];
    Require(continuation.Span[0x20] == 255 &&
        BinaryPrimitives.ReadInt32LittleEndian(continuation.Span[0x68..]) == -1 &&
        BinaryPrimitives.ReadUInt64LittleEndian(continuation.Span[0x70..]) ==
            0 &&
        BinaryPrimitives.ReadUInt32LittleEndian(continuation.Span[0x58..]) == uint.MaxValue &&
        BinaryPrimitives.ReadUInt64LittleEndian(continuation.Span[0x5c..]) == (0x1234567800000000ul | parentArc),
        "Continuation did not consume the real parent row/history or lost its overlapping opaque 5c field.");
    ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(parent.AsSpan(0x24));
    var projection = LegacyWordPathBoundaryCost.Evaluate(
        LegacyPreparedWordPathReference.Read(continuation.Span[..0x58], endingNode), categories,
        graph.NodeCount, false, 8, sourceBoundaries.GetBoundary);
    Require(projection.MappedCategory == categories.GetSymbolFirstId(symbol),
        "Continuation lost its inherited native category/state reference.");
}
Require(actualParentLookups > continuationCosts.Count && continuations.Length > 0 && continuationPromotions > 0,
    "Actual continuation failed to consume its materialized root anchor or exercise root promotion.");
Console.WriteLine($"ACTUAL {rows.Length} initialized real-resource parent rows x {increments.Count} second-glyph " +
    $"acoustic increments -> {continuationCosts.Count} explicit continuations/{continuationAccepted} allocations/" +
    $"{continuationPromotions} promotions -> {continuations.Length} retained; " +
    $"{actualHistoryLookups} parent/history row lookups, inherited category and exact 5c transport. " +
    "Cartesian scheduling, tag/payload/context inputs and bucket capacities are explicit diagnostics; no native winner claim.");
// Materializing a second row preserves the root anchor; a third continuation
// must follow that exact root, rather than inventing a prior text record.
int nextRowNode = endingNode + 1;
var nextRows = continuations.Select((record, index) =>
    LegacyNativeWordRowMaterializer.Materialize(record.Span, (ushort)index, (ushort)continuations.Length, 1, 1)
        .Record.ToArray()).ToArray();
int linkedCalls = 0;
ReadOnlyMemory<byte> LinkedLookup(int node, ushort arc)
{
    linkedCalls++;
    if (node == endingNode && arc < rows.Length)
        return rows[arc];
    if (node == nextRowNode && arc < nextRows.Length)
        return nextRows[arc];
    throw new InvalidDataException("Third continuation requested an unowned native row.");
}
var thirdHeaps = new[] { new LegacyNativeWordRecordHeap(2), new LegacyNativeWordRecordHeap(2) };
var thirdRouter = new LegacyNativeWordHeapRouter(thirdHeaps, new byte[] { 0 }, 0, uint.MaxValue);
var thirdFields = new LegacyContinuationWordRecordFields(0,
    BinaryPrimitives.ReadInt32LittleEndian(nextRows[0].AsSpan(0x10)) + increments[0].Cost,
    increments[0].Cost, increments[0].Symbol, new uint[5], 0, 0, 0, 0, 0);
var third = LegacyContinuationWordRecordProducer.TryInsert(thirdRouter, nextRowNode, 0,
    LinkedLookup, thirdFields, 0, false);
Require(third.Accepted && linkedCalls == 2, "Third continuation did not use parent plus original-root anchor.");
var thirdStored = LegacyNativeWordRowMaterializer.Materialize(thirdHeaps[0].GetSlotSnapshot(0).Span, 0, 1, 1, 1);
Require(BinaryPrimitives.ReadInt32LittleEndian(thirdStored.Record.Span[0x38..]) == endingNode &&
    BinaryPrimitives.ReadUInt16LittleEndian(thirdStored.Record.Span[0x3c..]) ==
        BinaryPrimitives.ReadUInt16LittleEndian(nextRows[0].AsSpan(0x3c)) &&
    BinaryPrimitives.ReadInt32LittleEndian(thirdHeaps[0].GetSlotSnapshot(0).Span[0x68..]) == -1,
    "Multiple stored rows changed the inherited native word-root anchor or invented previous history.");
Console.WriteLine("ACTUAL native materializer -> second continuation -> stored row -> third continuation: " +
    "exact inherited root arc/node and two owned parent/anchor lookups; previous context remains absent by root span.");
Console.WriteLine($"PASS {cases} independent complete stored-row controls, {primaryBlocks} primary/" +
    $"{secondaryBlocks} secondary alternative blocks; owned snapshots, anchor replacement/preservation, " +
    "primary arc-limit versus unconditional secondary copies, ordinal bounds and cancellation.");

static void Require(bool value, string message)
{
    if (!value)
        throw new InvalidDataException(message);
}
