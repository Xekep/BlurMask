using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 5) throw new ArgumentException("controls AK LK engine separation required");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int controlCount = 0, extras = 0;
foreach (var row in controls.RootElement.EnumerateArray())
{
    var f = row.GetProperty("fields").EnumerateArray().ToArray();
    var fields = new LegacyInitialWordRecordFields(f[0].GetUInt64(), f[1].GetUInt64(), f[2].GetInt32(), f[3].GetInt32(),
        f[4].GetUInt16(), f[5].GetUInt16(), f[6].GetUInt16(), f[7].GetUInt16(), f[8].GetByte(), f[9].GetUInt16(),
        f[10].GetSByte(), f[11].GetByte(), f[12].GetByte(), f[13].GetByte(), f[14].GetByte(), f[15].GetByte(),
        f[16].GetByte(), f[17].GetByte(), f[18].GetByte(), f[19].GetInt32(), f[20].GetInt32(), f[21].GetInt32(), f[22].GetUInt16());
    bool emitted = LegacyNativeInitialPrimitiveExtraRecord.TryCreate(fields, row.GetProperty("base").GetInt32(),
        row.GetProperty("kind").GetByte(), row.GetProperty("mode").GetBoolean(), row.GetProperty("flags").GetByte(),
        [187, 188, ushort.MaxValue, ushort.MaxValue], out var extra);
    Require(emitted == row.GetProperty("accepted").GetBoolean(), "Independent primitive eligibility differs.");
    if (emitted)
    {
        byte[] slot = new byte[128];
        Convert.FromHexString(row.GetProperty("residue").GetString()!).CopyTo(slot, 120);
        LegacyInitialWordRecordProducer.InitializeAllocatedPrefix(extra!, 0).CopyTo(slot, 0);
        LegacyInitialWordHeapTail.Initialize(slot, false);
        Require(slot.AsSpan().SequenceEqual(Convert.FromHexString(row.GetProperty("expected").GetString()!)),
            "Independent signed-wrap/full128 primitive extra differs.");
        extras++;
    }
    controlCount++;
}
Console.WriteLine($"PASS{controlCount} independent primitive-extra decisions/{extras} signed-wrap complete128 records; explicit scored controls.");
byte[] ak = File.ReadAllBytes(args[1]);
byte[] lk = File.ReadAllBytes(args[2]);
byte[] engine = File.ReadAllBytes(args[3]);
byte[] tableBytes = File.ReadAllBytes(args[4]);
var fixture = MainFixture.Create(ak, lk, engine, tableBytes);
var owner = new LegacyOwnedMainWordSpanPreparation(ak, lk, engine, fixture.Sampled, fixture.Table);
var graph = fixture.Sampled.SourceRanges.Graph;
var native = new LegacyNativeMainInitialSpecialCandidates(ak, owner, new(0x8877665544332211, 0), new byte[80]);
int records = 0;
LegacyOwnedMainWordSpan? stale = null;
foreach (int node in Enumerable.Range(0, graph.NodeCount))
{
    if (owner.GetSpanExtent(node) <= node) continue;
    var span = owner.Prepare(node, (byte)node);
    stale ??= span;
    //20BAA0 actual initial query capacity1684=128; this differs from125
    //source row budget. Extra-cost pointer1678 is present, so clear its12 bytes.
    var query = new LegacyPreparedDictionaryQuery { Node = 0, Capacity = 128,
        Parameter1c = (uint)node, Parameter20 = (uint)node, ContextRoot = owner.Contexts.ContainerIdentity };
    var page = owner.Read(span, 8UL << 40, query, clearExtraCosts: true);
    Require(page.ReturnedCount < 128 && page.ExtraCosts.Length == page.ReturnedCount * 12 &&
        page.ExtraCosts.Span.IndexOfAnyExcept((byte)0) == -1, "Actual fresh128 query storage/extra-cost clearing differs.");
    var newHeap = new LegacyNativeWordRecordHeap(125);
    var newRouter = Router(newHeap);
    var produced = native.ProcessPreparedPage(span, page.Records.Span, 0, newRouter, 0, false, false);
    var oldHeap = new LegacyNativeWordRecordHeap(125);
    var oldRouter = Router(oldHeap);
    var model = owner.DictionaryOwner.Model;
    var old = new LegacyNativeOrdinaryWordStartup(graph, model.AcousticAlphabet,
        fixture.Sampled.Owner.TransitionProfile.Categories, owner.Contexts,
        LegacyNativeWordAcousticRequestScorer.ForAlphabet(LegacyAcousticCandidateThresholds.ReadV110Node(ak.AsSpan(189734), out _), model.AcousticAlphabet),
        new(0x8877665544332211, 0), new byte[80]);
    uint[] headers = owner.DictionaryOwner.CopySourceHeaders();
    var previous = old.ProcessPage(page.Records.Span, (ushort)node, (byte)node, 0, span.Qualifier,
        new Dictionary<byte, uint> { [0] = headers[0], [1] = headers[1] },
        (n, arc) => owner.GetCosts(span, n, arc), oldRouter, 0, false, 0, false, false, null);
    Require(produced.VisitedQueries == previous.RowsVisited && produced.QualifiedQueries == previous.QualifiedRows &&
        produced.PrimaryAllocations == previous.AcceptedAllocations && produced.PrimitiveExtraAllocations == 0 &&
        newHeap.GetSlotStorageSnapshot().Span.SequenceEqual(oldHeap.GetSlotStorageSnapshot().Span),
        "Actual full unfiltered page regression changed complete native128 storage.");
    records += produced.PrimaryAllocations;
    Console.WriteLine($"Actual node{node}: {page.ReturnedCount} full queries -> {produced.PrimaryAllocations} native128 records; quote/primitive eligibility=" +
        string.Join(",", new[] { "'", "\"", "\U000f0002", "\U000f0003" }.Select(text =>
            model.AcousticAlphabet.TryGetLabel(text, out ushort symbol) ? $"{symbol}:{span.Qualifier.GetEligibilitySnapshot().Span[symbol]}" : "absent")));
}
Require(records > 0, "Actual full initial pages emitted no ordinary records.");
Console.WriteLine($"PASS{records} actual main native128 regression records, exact fresh128 query buffer/12-byte costs, direct-source scope.");
var staleConsumer = new LegacyNativeMainInitialSpecialCandidates(ak, owner, new(1, 0), new byte[80]);
Reject(() => staleConsumer.ProcessPreparedPage(stale!, [], 0, Router(new(0)), 0, false, false));
var foreign = new LegacyOwnedMainWordSpanPreparation(ak, lk, engine, fixture.Sampled, fixture.Table);
var foreignSpan = foreign.Prepare(0, 0);
var foreignConsumer = new LegacyNativeMainInitialSpecialCandidates(ak, owner, new(1, 0), new byte[80]);
Reject(() => foreignConsumer.ProcessPreparedPage(foreignSpan, [], 0, Router(new(0)), 0, false, false));
Console.WriteLine("PASS2 stale/foreign guards before any empty-page acoustic query.");
var currentSpan = owner.Prepare(0, 0);
var emptyConsumer = new LegacyNativeMainInitialSpecialCandidates(ak, owner, new(1, 0), new byte[80]);
Require(emptyConsumer.ProcessPreparedPage(currentSpan, [], 0, Router(new(0)), 0, false, false).VisitedQueries == 0,
    "Valid owned empty page unexpectedly requires an acoustic callback.");
Reject(() => staleConsumer.ProcessPreparedPage(currentSpan, [], 0, Router(new(0)), 0, false, false));
using (var cancellation = new CancellationTokenSource())
{
    cancellation.Cancel();
    var canceled = new LegacyNativeMainInitialSpecialCandidates(ak, owner, new(1, 0), new byte[80]);
    Reject(() => canceled.ProcessPreparedPage(currentSpan, [], 0, Router(new(0)), 0, false, false, cancellation.Token));
    Reject(() => canceled.ProcessPreparedPage(currentSpan, [], 0, Router(new(0)), 0, false, false));
}
Console.WriteLine("PASS4 valid-empty/invalidated-reuse/cancellation guards without fabricated source queries.");

//A quote-like prefix before a normal-height path supplies real baseline/size
//context. No class cost, model mask or prepared eligibility is replaced.
var quoteFixture = SpecialInputMainFixture.Create(ak, lk, engine, tableBytes, new InkStroke(new InkPoint[]
{ new(-15, 10), new(-16, 20), new(-12, 10), new(-10, 20), new(0, 90), new(0, 10), new(25, 60), new(50, 10), new(50, 90) }));
var quoteOwner = new LegacyOwnedMainWordSpanPreparation(ak, lk, engine, quoteFixture.Sampled, quoteFixture.Table);
var quoteSpan = quoteOwner.Prepare(0, 0);
Require(quoteSpan.Qualifier.GetEligibilitySnapshot().Span[129] != 0 &&
    quoteSpan.Qualifier.GetEligibilitySnapshot().Span[134] != 0, "Real quote prefix did not produce quote/apostrophe eligibility.");
var quotePage = quoteOwner.Read(quoteSpan, 8UL << 40, new LegacyPreparedDictionaryQuery
{ Node = 0, Capacity = 128, ContextRoot = quoteOwner.Contexts.ContainerIdentity }, clearExtraCosts: true);
int quoteRecords = 0;
foreach (ushort flags in new ushort[] { 0, 0x10 })
{
    var target = new LegacyNativeMainInitialSpecialCandidates(ak, quoteOwner, new(17, 0), new byte[80]);
    var heap = new LegacyNativeWordRecordHeap(125);
    var produced = target.ProcessPreparedPage(quoteSpan, quotePage.Records.Span, flags, Router(heap), 0, false, false);
    var expected = new List<(int Cost, ushort Symbol, int Category)>();
    var categoryMap = new LegacyCursiveCategoryMapper(quoteFixture.Sampled.Owner.TransitionProfile.Categories);
    for (int i = 0; i < quotePage.ReturnedCount; i++)
    {
        var queryRow = quotePage.Records.Span.Slice(i * 32, 32);
        ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(queryRow[20..]);
        int variantCount = flags == 0 || symbol is 129 or 134 ? 2 : 1;
        for (int variant = 0; variant < variantCount; variant++)
        {
            int category = categoryMap.GetSymbolCompositeCategory(symbol, variant != 0);
            if (category < 0) break;
            foreach (var position in quoteSpan.PermissionPass.SelectedPositions)
            {
                var cache = quoteOwner.GetCosts(quoteSpan, position.NodeIndex, (ushort)position.ArcIndex);
                int cost = unchecked(BinaryPrimitives.ReadInt32LittleEndian(queryRow[16..]) + cache.ClassCosts.Span[symbol] +
                    cache.GeometryCosts.Span[quoteFixture.Sampled.Owner.TransitionProfile.Categories.CompositeCategories.Span[category].GeometryIndex]);
                if (cost < 0x4000000) expected.Add((cost, symbol, category));
            }
        }
    }
    Require(produced.PrimaryAllocations == expected.Count && produced.AcousticCalls == expected.Count &&
        produced.PrimitiveExtraAllocations == 0, "Real full source quote initial extent differs.");
    foreach (int index in Enumerable.Range(0, heap.Count))
    {
        var record = heap.GetSlotSnapshot(index).Span;
        ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(record[36..]);
        var key = (BinaryPrimitives.ReadInt32LittleEndian(record[16..]), symbol, (int)(record[26] & 63));
        int matching = expected.IndexOf(key);
        Require(matching >= 0, "Real quote source candidate changed its actual cost/category/symbol.");
        expected.RemoveAt(matching);
        if (symbol is 129 or 134)
        {
            quoteRecords++;
            var context = quoteOwner.Contexts.ReadNode(BinaryPrimitives.ReadUInt64LittleEndian(record[8..]));
            Require(context.Symbol == symbol && quoteOwner.Contexts.ReadNode(context.ParentLink).Symbol == 0xfffa,
                "Real quote source lost its actual FFFA context lineage.");
        }
    }
    Require(expected.Count == 0, "Real quote initial record was dropped.");
}
Require(quoteRecords == 4, "Both apostrophe and quote were not retained under both input10 modes.");
Console.WriteLine($"PASS{quoteRecords} actual quote/apostrophe records from unfiltered source pages; real main NN/masks/FFFA owner retained.");

static LegacyNativeWordHeapRouter Router(LegacyNativeWordRecordHeap heap) => new([heap, new(0)], new byte[] { 0, 0 }, 0, uint.MaxValue);
static void Require(bool condition, string message) { if (!condition) throw new InvalidDataException(message); }
static void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or OperationCanceledException) { return; }
    throw new InvalidDataException("Expected initial scope refusal.");
}
