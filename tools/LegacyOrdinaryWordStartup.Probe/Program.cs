using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 5) throw new ArgumentException("controls.json AK LK engine.json separation.json required");
byte[] ak = File.ReadAllBytes(args[1]), lk = File.ReadAllBytes(args[2]);
var model = LegacyRussianModelBundle.Load(ak, lk, File.ReadAllBytes(args[3]));
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases = 0, calls = 0, allocations = 0;
foreach (var test in controls.RootElement.EnumerateArray())
{
    int ending = test.GetProperty("node").GetInt32();
    var graph = OwnGraph(ending + 1);
    ushort symbol = test.GetProperty("symbol").GetUInt16();
    byte length = test.GetProperty("length").GetByte(), flags = test.GetProperty("flags").GetByte();
    ulong tag = test.GetProperty("tag").GetUInt64();
    byte[] query = MakeQuery(tag, symbol, length, flags, test.GetProperty("before").GetInt32(), out var contexts);
    ulong queryContext = BinaryPrimitives.ReadUInt64LittleEndian(query.AsSpan(8));
    ulong expectedContext = LegacyNativeWordStartupRequestFactory.GraftOrdinaryInitialContext(contexts, queryContext);
    var costs = Enumerable.Repeat(0x4000000, model.AcousticAlphabet.LabelCount + 3).ToArray();
    costs[symbol] = test.GetProperty("classCost").GetInt32();
    var geometry = test.GetProperty("geometry").EnumerateArray().Select(v => v.GetInt32()).ToArray();
    var group = new LegacyNativeWordAcousticGroupCosts(costs, geometry);
    byte[] words = Enumerable.Repeat((byte)255, (model.AcousticAlphabet.LabelCount + 7) / 8).ToArray();
    if (!test.GetProperty("gate").GetBoolean()) words[symbol >> 3] &= (byte)~(1 << (symbol & 7));
    var qualifier = new LegacyNativeWordSymbolQualifier(model.AcousticAlphabet.LabelCount, [group], words);
    var primary = new LegacyNativeWordRecordHeap(16);
    var secondary = new LegacyNativeWordRecordHeap(16);
    var router = new LegacyNativeWordHeapRouter([primary, secondary], new byte[] { 0, 0, 0 }, 0, uint.MaxValue);
    var producer = new LegacyNativeOrdinaryWordStartup(graph, model.AcousticAlphabet, model.CursiveCategoryProfiles[1], contexts,
        new(new(10, 20, 30), new ushort[] { 179, 180, 181, 182, 183 }), new(0x1122334455667788, 0), new byte[80]);
    int cacheCalls = 0;
    var result = producer.ProcessPage(query, (ushort)ending, (byte)ending, test.GetProperty("inputFlags").GetUInt16(), qualifier,
        new Dictionary<byte, uint> { [test.GetProperty("sourceId").GetByte()] = test.GetProperty("header").GetUInt32() },
        (owner, arc) =>
        {
            Require(owner == ending && arc == ending, "Real whole-prefix source arc ownership differs.");
            cacheCalls++;
            return group;
        }, router, 1, test.GetProperty("suppressHeader").GetBoolean(), 0, false,
        test.GetProperty("suppressFront").GetBoolean(), (_, _) => test.GetProperty("suppressed").GetBoolean());
    Require(result.Succeeded && result.RowsVisited == 1 && result.QualifiedRows == test.GetProperty("qualified").GetInt32() &&
        result.ScoringCalls == test.GetProperty("calls").GetInt32(), "Independent startup enumeration/qualification differs.");
    var expected = test.GetProperty("rows").EnumerateArray().Select(v => Convert.FromHexString(v.GetString()!)).ToArray();
    Require(primary.Count == expected.Length && secondary.Count == 0 && result.AcceptedAllocations == expected.Length,
        "Independent startup accepted/allocation count differs.");
    foreach (var row in expected) BinaryPrimitives.WriteUInt64LittleEndian(row.AsSpan(8), expectedContext);
    var actual = Enumerable.Range(0, primary.Count).Select(i => primary.GetSlotSnapshot(i).ToArray()).ToArray();
    Require(actual.Select(Convert.ToHexString).Order().SequenceEqual(expected.Select(Convert.ToHexString).Order()),
        "Independent full128 source-row→native writer records differ.");
    int lookups = test.GetProperty("suppressFront").GetBoolean() && test.GetProperty("suppressed").GetBoolean() ? 0 : result.ScoringCalls;
    Require(cacheCalls == lookups, "Suppressed frontend128 read a foreign cache.");
    cases++;
    calls += result.ScoringCalls;
    allocations += result.AcceptedAllocations;
}
Console.WriteLine($"PASS {cases} independent ordinary source-row/category/full128 controls; {calls} native scoring requests; {allocations} owning allocations.");
int guards = 0;
model.AcousticAlphabet.TryGetLabel("a", out ushort guardSymbol);
var guardCosts = new LegacyNativeWordAcousticGroupCosts(new int[model.AcousticAlphabet.LabelCount + 3], new int[35]);
var guardQualifier = new LegacyNativeWordSymbolQualifier(model.AcousticAlphabet.LabelCount, [guardCosts]);
var guardHeaders = new Dictionary<byte, uint> { [1] = 0xc00 };
var guardGraph = OwnGraph(1);
var guardQuery = MakeQuery(1UL << 32, guardSymbol, 0, 0, 0, out var guardContexts);
LegacyNativeOrdinaryWordStartup NewGuard() => new(guardGraph, model.AcousticAlphabet, model.CursiveCategoryProfiles[1], guardContexts,
    new(new(10, 20, 30), new ushort[] { 179, 180, 181, 182, 183 }), new(0x1122334455667788, 0), new byte[80]);
LegacyNativeWordHeapRouter NewRouter(int capacity = 16) => new([new(capacity), new(capacity)], new byte[] { 0, 0 }, 0, uint.MaxValue);
LegacyNativeOrdinaryWordStartupResult RunGuard(LegacyNativeOrdinaryWordStartup producer, byte[] row,
    Func<int, ushort, LegacyNativeWordAcousticGroupCosts>? cache = null, LegacyNativeWordHeapRouter? router = null,
    bool suppressed = false, Func<int, ushort, bool>? suppress = null, CancellationToken token = default)
    => producer.ProcessPage(row, 0, 0, 0, guardQualifier, guardHeaders, cache ?? ((_, _) => guardCosts), router ?? NewRouter(),
        1, false, 0, false, suppressed, suppress, token);
var bounded = NewGuard();
var full = RunGuard(bounded, guardQuery, router: NewRouter(0));
Require(full.Succeeded && full.RowsVisited == 1 && full.ScoringCalls > 0 && full.AcceptedAllocations == 0,
    "Native ordinary heap-slot rejection did not preserve startup success.");
Require(RunGuard(bounded, guardQuery).Succeeded, "Heap-slot rejection invalidated ordinary startup reuse.");
foreach (bool suppression in new[] { false, true })
{
    using var canceled = new CancellationTokenSource();
    var producer = NewGuard();
    Reject(() => RunGuard(producer, guardQuery, cache: (_, _) => { canceled.Cancel(); return guardCosts; },
        suppressed: suppression, suppress: (_, _) => { canceled.Cancel(); return false; }, token: canceled.Token));
    Reject(() => RunGuard(producer, guardQuery));
}
Reject(() => RunGuard(NewGuard(), guardQuery, suppressed: true));
Reject(() => RunGuard(NewGuard(), new byte[31]));
Reject(() => RunGuard(NewGuard(), new byte[513 * 32]));
var missingHeader = guardQuery.ToArray();
missingHeader[4] = 2;
Reject(() => RunGuard(NewGuard(), missingHeader));
var wrongContext = guardQuery.ToArray();
BinaryPrimitives.WriteUInt64LittleEndian(wrongContext.AsSpan(8), ulong.MaxValue);
Reject(() => RunGuard(NewGuard(), wrongContext));
foreach (string unsupported in new[] { "'", "\"", "\U000f0002", "\U000f0003", "\U000f0004", "\U000f0008" })
{
    if (!model.AcousticAlphabet.TryGetLabel(unsupported, out ushort label)) continue;
    var row = guardQuery.ToArray();
    BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(20), label);
    Reject(() => RunGuard(NewGuard(), row));
}
foreach (string excluded in new[] { "\U000f0001", "\u0651" })
{
    if (!model.AcousticAlphabet.TryGetLabel(excluded, out ushort label)) continue;
    var row = new byte[32];
    BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(20), label);
    var skipped = RunGuard(NewGuard(), row, cache: (_, _) => throw new Exception("Excluded row accessed cache."));
    Require(skipped.Succeeded && skipped.RowsVisited == 1 && skipped.ScoringCalls == 0 && skipped.QualifiedRows == 0,
        "Native excluded label was not skipped before context/header lookup.");
    guards++;
}
Console.WriteLine($"PASS {guards} startup failure/callback cancellation/ownership/unsupported/excluded-label guards.");
ActualStartup.Run(args);

void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidDataException or InvalidOperationException or NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new Exception("Invalid startup page/lifecycle silently accepted.");
}

static byte[] MakeQuery(ulong tag, ushort symbol, byte length, byte flags, int cost, out LegacySymbolContextFactory contexts)
{
    contexts = new();
    var query = new byte[32];
    BinaryPrimitives.WriteUInt64LittleEndian(query, tag);
    BinaryPrimitives.WriteUInt64LittleEndian(query.AsSpan(8), contexts.ResolveInitial(symbol).Identity);
    BinaryPrimitives.WriteInt32LittleEndian(query.AsSpan(16), cost);
    BinaryPrimitives.WriteUInt16LittleEndian(query.AsSpan(20), symbol);
    query[22] = length;
    query[23] = flags;
    return query;
}
static LegacyOrdinaryCursiveCandidateGraph OwnGraph(int count)
{
    var raw = Enumerable.Range(0, count).Select(i => new InkStroke(new[] { new InkPoint(i * 100, 10), new InkPoint(i * 100 + 20, 90) })).ToArray();
    var prepared = LegacyCursivePreparedInk.Create(raw);
    var markers = Enumerable.Range(0, count).Select(i => new LegacyCursiveCutMarker(0, i, 0)).ToArray();
    var cuts = new LegacyCursiveCutResult(prepared, markers, LegacyCursiveInkAtoms.Build(prepared.StrokePointCounts.Span, markers));
    var seeds = LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts, 80, new(false, 1, 0, .5f), new byte[count], Enumerable.Repeat(-1, count).ToArray());
    return LegacyOrdinaryCursiveCandidateGraph.Generate(seeds, LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds), count);
}
static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
