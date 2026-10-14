using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 5) throw new ArgumentException("controls AK LK engine separation required");
byte[] ak = File.ReadAllBytes(args[1]);
var categories = LegacyCursiveCategoryProfile.Load(ak.AsSpan(188222), 188222, out _);
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int packets = 0;
foreach (var row in controls.RootElement.EnumerateArray())
{
    byte[] projectedParent = Hex(row, "parent"), residue = Hex(row, "residue");
    byte[] parentBefore = projectedParent.ToArray(), residueBefore = residue.ToArray();
    var cell = new LegacyNativeWordTransitionCostCell(0x1122334455667788, 73);
    var packet = LegacyNativeWordFlagPrepassRequestFactory.Create(row.GetProperty("current").GetInt32(),
        row.GetProperty("span").GetByte(), row.GetProperty("previous").GetInt32(), projectedParent,
        row.GetProperty("arc").GetUInt16(), row.GetProperty("header").GetUInt32(), categories, cell, residue);
    Require(packet.GetSnapshot().Span.SequenceEqual(Hex(row, "expected")) &&
        cell.Value == row.GetProperty("cost").GetInt32(), "Original full80 prepass producer differs.");
    Require(projectedParent.AsSpan().SequenceEqual(parentBefore) && residue.AsSpan().SequenceEqual(residueBefore),
        "Prepass packet construction changed parent/residue inputs.");
    packets++;
}
Console.WriteLine($"PASS{packets} independent complete80 main flag-prepass packets.");
var fixture = MainFixture.Create(ak, File.ReadAllBytes(args[2]), File.ReadAllBytes(args[3]), File.ReadAllBytes(args[4]));
var model = fixture.Model;
var source = fixture.Sampled;
var graph = source.SourceRanges.Graph;
var geometry = new LegacyMainBlankTransitionGeometry(ak, source, fixture.Table);
Require(geometry.Threshold70 == -.5f && !geometry.RequireWholePhysicalStarts10c && source.SourceHeight == 40,
    "Actual main-owned flag/gap configuration differs.");
var ordinary = source.CreatePrepared240TransitionGeometry(fixture.Table);
int gaps = 0;
for (int node = 0; node < graph.NodeCount; node++)
{
    var arcs = graph.GetNode(node);
    for (ushort ordinal = 0; ordinal < arcs.Count; ordinal++)
    {
        if (node == arcs[ordinal].Span)
        { Require(geometry.ReadPreparedBlankCost(node, ordinal) == 0x4000000, "Root gap cache is notIMP."); continue; }
        int previous = node - arcs[ordinal].Span - 1;
        float prefix = -float.MaxValue;
        foreach (int parentArc in arcs[ordinal].PredecessorArcIndices.Span)
            prefix = Math.Max(prefix, ordinary.GetPreparedState(previous, parentArc).PrefixMaximumX);
        var box = ordinary.GetPreparedState(node, ordinal).Bounds;
        if (prefix == -float.MaxValue) prefix = box.MaximumX;
        int expected = OriginalGap(box.MinimumX, prefix, source.SourceHeight, -.5f, 3, fixture.Table.Values.Span);
        Require(geometry.ReadPreparedBlankCost(node, ordinal) == expected,
            "Independent actual main bbox/prefix gap cost differs.");
        gaps++;
    }
}
Require(gaps > 0, "Real main source has no nonroot kind5 cache rows.");
Console.WriteLine($"PASS{gaps} actual main minX/all-parents-prefix/guide40 gap cache controls.");

//The actual direct dictionary supplies the original tag, symbol and shared
//context. Only a supported plain query is selected for this bounded consumer.
var query = new LegacyPreparedDictionaryQuery { Node = 0, Capacity = 256,
    ContextRoot = fixture.Dictionary.Contexts.ContainerIdentity };
var page = fixture.Dictionary.DictionaryQueries.Read(8UL << 40, query);
var caches = new Dictionary<ushort, LegacyNativeWordAcousticGroupCosts>();
var separation = LegacyCursiveSeparationProfile.ReadV106Node(ak, source.Owner.CategoryOwner.SamplerNodeOffset);
for (ushort ordinal = 0; ordinal < graph.GetNode(0).Count; ordinal++)
{
    var arc = graph.GetNode(0)[ordinal];
    var scores = fixture.Acoustic.Score(arc.GroupId);
    int cost = LegacyCursiveGroupSeparationCost.Evaluate(source.Cuts.PreparedInk, graph.Groups[arc.GroupId].Ranges.Span,
        source.SourceHeight, separation.GapOffset, separation.Slope, fixture.Table.Values.Span, separation.MaximumCost);
    caches.Add(ordinal, new(scores.Cache.Costs.Span, Enumerable.Repeat(cost, categories.CompositeCategories.Length).ToArray()));
}
var qualifier = new LegacyNativeWordSymbolQualifier(model.AcousticAlphabet.LabelCount, caches.Values.ToArray());
int queryIndex = Enumerable.Range(0, page.ReturnedCount).First(i =>
{
    var candidate = page.Records.Span.Slice(i * 32, 32);
    ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(candidate[20..]);
    return model.AcousticAlphabet.TryGetText(symbol, out string text) && text.Length == 1 &&
        (char.IsAsciiLetterOrDigit(text[0]) || text[0] is >= '\u0400' and <= '\u04ff') &&
        qualifier.Qualifies(symbol, candidate[22], BinaryPrimitives.ReadUInt64LittleEndian(candidate[8..]), fixture.Dictionary.Contexts);
});
byte[] selectedQuery = page.Records.Slice(queryIndex * 32, 32).ToArray();
ushort selectedSymbol = BinaryPrimitives.ReadUInt16LittleEndian(selectedQuery.AsSpan(20));
var thresholds = LegacyAcousticCandidateThresholds.ReadV110Node(ak.AsSpan(189734), out _);
var startup = new LegacyNativeOrdinaryWordStartup(graph, model.AcousticAlphabet, categories, fixture.Dictionary.Contexts,
    LegacyNativeWordAcousticRequestScorer.ForAlphabet(thresholds, model.AcousticAlphabet), new(0x1122334455667788, 0), new byte[80]);
var initialHeap = new LegacyNativeWordRecordHeap(64);
var initialRouter = new LegacyNativeWordHeapRouter([initialHeap, new(0)], new byte[] { 0, 0 }, 0, uint.MaxValue);
uint[] headers = fixture.Dictionary.CopySourceHeaders();
var initial = startup.ProcessPage(selectedQuery, 0, 0, 0, qualifier,
    new Dictionary<byte, uint> { [0] = headers[0], [1] = headers[1] }, (_, arc) => caches[arc],
    initialRouter, 0, false, 0, false, false, null);
Require(initial.Succeeded && initialHeap.Count > 0, "Actual main NN/dictionary startup emitted no owned parent.");
var parent = LegacyNativeWordRowMaterializer.Materialize(initialHeap.GetSlotSnapshot(0).Span, 0, 1, 1, 1);
byte[] parentBeforePrepass = parent.Record.ToArray();
bool originalFlag = (parent.Record.Span[5] & 128) != 0;
byte[] reused = new byte[80];
BinaryPrimitives.WriteInt32LittleEndian(reused.AsSpan(52), 0x4000000);
var prepass = new LegacyNativeMainWordFlagPrepass(ak, fixture.Acoustic, fixture.Table,
    new(0x1122334455667788, 0), reused);
byte[] initialStorage = Enumerable.Range(0, 128).Select(i => unchecked((byte)(i * 43 + 19))).ToArray();
var heap = new LegacyNativeWordRecordHeap(1, initialStorage);
var router = new LegacyNativeWordHeapRouter([heap, new(0)], new byte[] { 0, 0 }, 0, uint.MaxValue);
ReadOnlyMemory<byte> Lookup(int node, ushort ordinal)
{
    Require(node == 0 && ordinal == 0, "Actual root parent row lookup lost its owner.");
    return parent.Record;
}
var skipped = prepass.Process(1, 0, 0, 0, Lookup, headers, fixture.Dictionary.Contexts, router, originalFlag, 32, false);
Require(!skipped.Applied && heap.Count == 0, "22F7 polarity did not skip the actual stored parent flag.");
var result = prepass.Process(1, 0, 0, 0, Lookup, headers, fixture.Dictionary.Contexts, router, !originalFlag, 32, false);
Require(result.Applied && result.Qualified && result.GeometryCalls > 0 && result.AcousticCalls > 0 && result.AcceptedAllocations > 0,
    "Actual current-span/cache/flag prepass produced no native kind5 candidates.");
Require(parent.Record.Span.SequenceEqual(parentBeforePrepass) && reused.AsSpan(0, 52).IndexOfAnyExcept((byte)0) == -1 &&
    result.LastRequest80.Length == 80 && BinaryPrimitives.ReadInt32LittleEndian(result.LastRequest80.Span[52..]) == 0x4000000,
    "Prepass changed its parent/residue inputs or reused request limit.");
for (int index = 0; index < heap.Count; index++)
{
    var slot = heap.GetSlotSnapshot(index).Span;
    ushort currentArc = BinaryPrimitives.ReadUInt16LittleEndian(slot[28..]);
    int group = graph.GetNode(1)[currentArc].GroupId;
    var finalCache = fixture.Acoustic.Score(group).Cache.Costs.Span;
    int separationCost = LegacyCursiveGroupSeparationCost.Evaluate(source.Cuts.PreparedInk,
        graph.Groups[group].Ranges.Span, source.SourceHeight, separation.GapOffset,
        separation.Slope, fixture.Table.Values.Span, separation.MaximumCost);
    int delta = unchecked(geometry.ReadPreparedBlankCost(1, currentArc) +
        finalCache[model.AcousticAlphabet.LabelCount] + separationCost);
    int expectedTotal = unchecked(BinaryPrimitives.ReadInt32LittleEndian(parent.Record.Span[16..]) + delta);
    int expectedIncrement = unchecked(BinaryPrimitives.ReadInt32LittleEndian(parent.Record.Span[20..]) + delta);
    byte[] expectedSlot = OriginalSlot(parent.Record.Span, initialStorage, currentArc, expectedTotal, expectedIncrement);
    Require(slot.SequenceEqual(expectedSlot), "Independent actual numeric/full128 prepass output differs.");
    Require(BinaryPrimitives.ReadUInt64LittleEndian(slot) == (BinaryPrimitives.ReadUInt64LittleEndian(parent.Record.Span) | 128UL << 40) &&
        BinaryPrimitives.ReadUInt16LittleEndian(slot[36..]) == selectedSymbol &&
        BinaryPrimitives.ReadUInt16LittleEndian(slot[30..]) == 0 && (slot[5] & 128) != 0 &&
        ((slot[39] >> 1) & 7) == 5 && slot[38] == parent.Record.Span[38],
        "Actual prepass1F8D60 lost parent tag/label/ordinal/count or forced native flags.");
}
Console.WriteLine($"ACTUAL post720 main source→4NN195→owned qualified dictionary parent→{result.GeometryCalls} kind5 gap118/140→{result.AcousticCalls} owned128 scores→{result.AcceptedAllocations} full128 prepass rows PASS. Live22F7/distance32 explicit; first proposals limited; full scheduler/winner excluded.");

int guardCount = 0;
Reject(() => new LegacyNativeMainWordFlagPrepass(ak, fixture.Acoustic, fixture.Table, new(1, 0), new byte[79])); guardCount++;
Reject(() => new LegacyNativeMainWordFlagPrepass(ak, fixture.Acoustic, fixture.Table, new(1, 0), new byte[80])); guardCount++;
foreach (bool lookupCancellation in new[] { false, true })
{
    using var cancellation = new CancellationTokenSource();
    var target = new LegacyNativeMainWordFlagPrepass(ak, fixture.Acoustic, fixture.Table, new(1, 0), reused);
    Reject(() => target.Process(1, 0, 0, 0, (_, _) =>
    { if (lookupCancellation) cancellation.Cancel(); return parent.Record; }, headers,
        fixture.Dictionary.Contexts, router, !originalFlag, 32, false,
        lookupCancellation ? cancellation.Token : new CancellationToken(true))); guardCount++;
    Reject(() => target.Process(1, 0, 0, 0, Lookup, headers, fixture.Dictionary.Contexts, router, !originalFlag, 32, false)); guardCount++;
}
var noSlots = new LegacyNativeWordHeapRouter([new(0), new(0)], new byte[] { 0, 0 }, 0, uint.MaxValue);
Require(prepass.Process(1, 0, 0, 0, Lookup, headers, fixture.Dictionary.Contexts, noSlots, !originalFlag, 32, false).AcceptedAllocations == 0,
    "Ordinary zero-capacity prepass unexpectedly accepted a slot."); guardCount++;
Require(prepass.Process(1, 0, 0, 0, Lookup, headers, fixture.Dictionary.Contexts, router, !originalFlag, 1, false).Applied == false,
    "Knowledge58 equality did not take the native no-op."); guardCount++;
foreach (int offset in new[] { -1, 0, 1 })
{
    int candidateCost = BinaryPrimitives.ReadInt32LittleEndian(heap.GetSlotSnapshot(0).Span[16..]);
    var budget = new LegacyNativeWordRecordHeap(1, initialStorage);
    Require(budget.TryAllocate(candidateCost + offset, slot =>
        BinaryPrimitives.WriteInt32LittleEndian(slot[16..], candidateCost + offset)), "Budget seed failed.");
    byte[] before = budget.GetSlotSnapshot(0).ToArray();
    var budgetRouter = new LegacyNativeWordHeapRouter([budget, new(0)], new byte[] { 0, 0 }, 0, uint.MaxValue);
    var decision = prepass.Process(1, 0, 0, 0, Lookup, headers, fixture.Dictionary.Contexts,
        budgetRouter, !originalFlag, 32, false);
    Require(decision.AcceptedAllocations == (offset > 0 ? 1 : 0), "Strict/equal/loose native prepass budget differs.");
    if (offset <= 0) Require(budget.GetSlotSnapshot(0).Span.SequenceEqual(before), "Rejected prepass changed full128 heap storage.");
    guardCount++;
}
foreach (int failure in Enumerable.Range(0, 5))
{
    var target = new LegacyNativeMainWordFlagPrepass(ak, fixture.Acoustic, fixture.Table, new(1, 0), reused);
    byte[] badParent = parent.Record.ToArray();
    if (failure == 2) badParent[4] = 255;
    if (failure == 3) BinaryPrimitives.WriteUInt16LittleEndian(badParent.AsSpan(24), 1);
    Reject(() => target.Process(failure == 0 ? graph.NodeCount : 1, 0, 0, 0,
        (_, _) => failure == 4 ? new byte[87] : badParent,
        failure == 1 ? Array.Empty<uint>() : headers, fixture.Dictionary.Contexts,
        router, !originalFlag, 32, false));
    guardCount++;
}
Console.WriteLine($"PASS{guardCount} main flag prepass resource/request/cancellation/lifecycle/zero-cap/distance controls.");

static byte[] OriginalSlot(ReadOnlySpan<byte> parent, ReadOnlySpan<byte> storage,
    ushort currentArc, int total, int increment)
{
    byte[] expected = new byte[128];
    storage[120..128].CopyTo(expected.AsSpan(120));
    parent[..16].CopyTo(expected);
    expected[5] = (byte)((parent[5] & 0x7d) | 128 | ((parent[5] & 2) != 0 ? 2 : 0));
    expected[6] = (byte)(parent[6] & 0xfc);
    BinaryPrimitives.WriteInt32LittleEndian(expected.AsSpan(16), total);
    BinaryPrimitives.WriteInt32LittleEndian(expected.AsSpan(20), increment);
    parent[24..28].CopyTo(expected.AsSpan(24));
    BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(28), currentArc);
    BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(30), 0);
    BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(34), ushort.MaxValue);
    parent[36..39].CopyTo(expected.AsSpan(36));
    expected[39] = 10;
    BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(40), ulong.MaxValue);
    parent[48..62].CopyTo(expected.AsSpan(48));
    BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(62), ushort.MaxValue);
    BinaryPrimitives.WriteUInt64LittleEndian(expected.AsSpan(88), ulong.MaxValue);
    int anchor = BinaryPrimitives.ReadInt32LittleEndian(parent[56..]);
    Require(BinaryPrimitives.ReadUInt16LittleEndian(parent[60..]) is 0 or ushort.MaxValue && anchor == 0,
        "Original bounded root fixture unexpectedly needs foreign anchor rows.");
    int previous = anchor + ~unchecked((sbyte)parent[32]);
    BinaryPrimitives.WriteInt32LittleEndian(expected.AsSpan(104), previous);
    Require(previous == -1, "Original root tail fixture unexpectedly needs prior linguistic context.");
    return expected;
}

static int OriginalGap(float current, float prefix, float height, float threshold, float slope, ReadOnlySpan<float> table)
{
    float transformed = ((current - prefix) / height - threshold) * -slope;
    if (transformed > 4) return 0;
    float log = transformed;
    if (transformed >= -4) log = table[(int)((double)(transformed + 4f) * 255 + .5) >> 3];
    float value = -log * 8192f;
    if (value >= 0x4000000) return 0x4000000;
    uint bits = unchecked((uint)BitConverter.SingleToInt32Bits(value));
    uint mantissa = (bits & 0x7fffff) | 0x800000;
    int shift = (int)(bits >> 23 & 255) - 127 - 23;
    if (value < .5f) return 0;
    return shift >= 0 ? checked((int)(mantissa << shift))
        : checked((int)((mantissa + (1u << (-shift - 1))) >> -shift));
}
static byte[] Hex(JsonElement row, string property) => Convert.FromHexString(row.GetProperty(property).GetString()!);
static void Require(bool condition, string message) { if (!condition) throw new InvalidDataException(message); }
static void Reject(Action action)
{
    try { action(); }
    catch (Exception e) when (e is ArgumentException or InvalidOperationException or NotSupportedException or InvalidDataException or OperationCanceledException) { return; }
    throw new InvalidDataException("Expected bounded flag-prepass refusal.");
}
