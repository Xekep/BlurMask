using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 5) throw new ArgumentException("controls.json AK LK engine.json separation.json required");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int qualificationCases = 0, preparationPasses = 0, guards = 0;
foreach (var test in controls.RootElement.GetProperty("cases").EnumerateArray())
{
    int labels = test.GetProperty("count").GetInt32();
    var costs = test.GetProperty("rows").EnumerateArray().Select(Ints).ToArray();
    byte[]? bitsStorage = test.GetProperty("bits").ValueKind == JsonValueKind.Null
        ? null : Convert.FromHexString(test.GetProperty("bits").GetString()!);
    ReadOnlyMemory<byte>? bits = bitsStorage is null ? (ReadOnlyMemory<byte>?)null : new ReadOnlyMemory<byte>(bitsStorage);
    var qualifier = new LegacyNativeWordSymbolQualifier(labels,
        costs.Select(c => new LegacyNativeWordAcousticGroupCosts(c, [])).ToArray(), bits);
    Require(qualifier.GetEligibilitySnapshot().Span.SequenceEqual(Bytes(test.GetProperty("union"))), "Independent count+3 finite-cost union differs.");
    var contexts = new LegacySymbolContextFactory();
    ulong current = 0;
    foreach (var key in test.GetProperty("path").EnumerateArray())
    {
        ushort symbol = key.GetUInt16();
        if (current == 0) current = contexts.ResolveInitial(symbol).Identity;
        else
        {
            ulong child = contexts.ResolveChild(current, symbol).Identity;
            contexts.LinkCreatedContext(child, current);
            current = child;
        }
    }
    bool qualified = qualifier.Qualifies(test.GetProperty("symbol").GetUInt16(), test.GetProperty("length").GetByte(), current, contexts);
    Require(qualified == test.GetProperty("accepted").GetBoolean(), "Independent word-bitset/parent qualification differs.");
    if (bitsStorage is not null) Array.Clear(bitsStorage);
    foreach (var row in costs) Array.Fill(row, 0x4000000);
    Require(qualifier.GetEligibilitySnapshot().Span.SequenceEqual(Bytes(test.GetProperty("union"))), "Qualification table retained foreign cache storage.");
    Require(qualifier.Qualifies(test.GetProperty("symbol").GetUInt16(), test.GetProperty("length").GetByte(), current, contexts) == qualified,
        "Qualifier retained foreign word-character storage.");
    qualificationCases++;
}

foreach (var trace in controls.RootElement.GetProperty("traces").EnumerateArray())
{
    var graph = OwnGraph(trace.GetProperty("n").GetInt32(), trace.GetProperty("cap").GetInt32());
    var source = new LegacyPreparedDictionaryWorkingMap(3, 0xc00, new ushort[] { 0, 1, 2 }, []);
    ushort[] reserved = [65535, 65535, 2, 65535, 65535, 65535];
    int calls = 0;
    var session = new LegacyNativeWordSymbolPreparationSession(graph, source, reserved, (group, token) =>
    {
        calls++;
        int[] values = [0x4000000, 39999, 40000, 40001, -1, 100, 0x4000001];
        return new(Enumerable.Range(0, 6).Select(s => values[(group.GroupId * 13 + calls * 5 + s * 11) % 7]).ToArray(), 1);
    }, existingEligibility88: new byte[] { 128, 0, 1, 2, 0, 255 });
    var snapshots = new List<(LegacyNativeWordSymbolQualifier Qualifier, byte[] Table)>();
    foreach (var op in trace.GetProperty("ops").EnumerateArray())
    {
        var result = session.Prepare(op.GetProperty("node").GetInt32(), op.GetProperty("scope").GetByte(), op.GetProperty("allow").GetBoolean());
        Require(result.PermissionPass.Succeeded && result.Qualifier is not null, "Owning source/cache preparation failed.");
        Require(result.EligibilityRebuilt == !op.GetProperty("allow").GetBoolean() &&
            result.Qualifier!.GetEligibilitySnapshot().Span.SequenceEqual(Bytes(op.GetProperty("table"))) &&
            result.PermissionPass.ComputedRows == op.GetProperty("computed").GetInt32() &&
            result.PermissionPass.ReusedRows == op.GetProperty("reused").GetInt32() && calls == op.GetProperty("calls").GetInt32(),
            "Independent cache lifecycle or frontend88 preservation differs.");
        var rows = op.GetProperty("rows").EnumerateArray().ToArray();
        Require(rows.Length == result.PermissionPass.SelectedRows.Count, "Actual span selection extent differs.");
        for (int i = 0; i < rows.Length; i++)
        {
            var actual = result.PermissionPass.SelectedRows[i];
            Require(actual.Position.ArcIndex == rows[i].GetProperty("ordinal").GetInt32() &&
                actual.GroupId == rows[i].GetProperty("group").GetInt32() && actual.Costs.Span.SequenceEqual(Ints(rows[i].GetProperty("costs"))),
                "Actual selected group identity/cache bytes differ.");
        }
        snapshots.Add((result.Qualifier!, result.Qualifier!.GetEligibilitySnapshot().ToArray()));
        preparationPasses++;
    }
    foreach (var snapshot in snapshots)
        Require(snapshot.Qualifier.GetEligibilitySnapshot().Span.SequenceEqual(snapshot.Table), "Published qualification snapshot changed after reuse.");
}

var guardGraph = OwnGraph(2, 2);
var guardSource = new LegacyPreparedDictionaryWorkingMap(3, 0xc00, new ushort[] { 0, 1, 2 }, []);
ushort[] emptySlots = [65535, 65535, 65535, 65535, 65535, 65535];
var missing = new LegacyNativeWordSymbolPreparationSession(guardGraph, guardSource, emptySlots, (_, _) => new(new int[6], 1));
Reject(() => missing.Prepare(0, 0, true));
Reject(() => missing.Prepare(0, 0, false));
var failed = new LegacyNativeWordSymbolPreparationSession(guardGraph, guardSource, emptySlots, (_, _) => null);
Require(!failed.Prepare(0, 0, false).PermissionPass.Succeeded, "Failed cache became successful qualification.");
Reject(() => failed.Prepare(0, 0, false));
using (var cancellation = new CancellationTokenSource())
{
    var canceled = new LegacyNativeWordSymbolPreparationSession(guardGraph, guardSource, emptySlots, (_, _) =>
    {
        cancellation.Cancel();
        return new(new int[6], 1);
    });
    Reject(() => canceled.Prepare(0, 0, false, cancellation.Token));
    Reject(() => canceled.Prepare(0, 0, false));
}
Reject(() => new LegacyNativeWordSymbolQualifier(3, [new(new int[5], [])]));
Reject(() => new LegacyNativeWordSymbolQualifier(3, [], new byte[2]));
Reject(() => new LegacyNativeWordSymbolPreparationSession(guardGraph, guardSource, emptySlots, (_, _) => new(new int[6], 1), existingEligibility88: new byte[5]));
var directGuard = new LegacyNativeWordSymbolQualifier(3, [new(new int[6], [])]);
Reject(() => directGuard.Qualifies(6, 0, 0, new()));
Reject(() => directGuard.Qualifies(0, 0, 0, new(), new CancellationToken(true)));
var wordGuard = new LegacyNativeWordSymbolQualifier(3, [new(new int[6], [])], new byte[] { 255 });
Reject(() => wordGuard.Qualifies(0, 1, ulong.MaxValue, new()));
Console.WriteLine($"PASS {qualificationCases} independent finite-cost/bitset/context controls; {preparationPasses} owning cache/source preparation passes; {guards} invalidation/cancellation/storage guards.");

ActualQualification.Run(args);

static int[] Ints(JsonElement array) => array.EnumerateArray().Select(v => v.GetInt32()).ToArray();
static byte[] Bytes(JsonElement array) => array.EnumerateArray().Select(v => v.GetByte()).ToArray();
static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidDataException or InvalidOperationException or NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new Exception("Unsupported qualification ownership/lifecycle accepted.");
}
static LegacyOrdinaryCursiveCandidateGraph OwnGraph(int count, int maximumAtoms)
{
    var raw = Enumerable.Range(0, count).Select(i => new InkStroke(new[] { new InkPoint(i * 100, 10), new InkPoint(i * 100 + 20, 90) })).ToArray();
    var prepared = LegacyCursivePreparedInk.Create(raw);
    var markers = Enumerable.Range(0, count).Select(i => new LegacyCursiveCutMarker(0, i, 0)).ToArray();
    var cuts = new LegacyCursiveCutResult(prepared, markers, LegacyCursiveInkAtoms.Build(prepared.StrokePointCounts.Span, markers));
    var seeds = LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts, 80, new(false, 1, 0, .5f), new byte[count], Enumerable.Repeat(-1, count).ToArray());
    return LegacyOrdinaryCursiveCandidateGraph.Generate(seeds, LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds), maximumAtoms);
}
