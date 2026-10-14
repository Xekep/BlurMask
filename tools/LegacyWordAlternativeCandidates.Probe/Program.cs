using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 5) throw new ArgumentException("controls.json AK LK engine.json separation.json required");
byte[] ak = File.ReadAllBytes(args[1]);
var categories = LegacyCursiveCategoryProfile.Load(ak.AsSpan(188222), 188222, out _);
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0]));
var controls = document.RootElement;
int packets = 0;
foreach (var test in controls.GetProperty("packets").EnumerateArray())
{
    var cell = new LegacyNativeWordTransitionCostCell(0x1122334455667788, 55);
    var request = LegacyNativeWordAlternativeRequestFactory.Create(U32(test, "current"), U32(test, "span"),
        test.GetProperty("previous").GetInt32(), Hex(test, "parent"), Hex(test, "related"),
        test.GetProperty("arc").GetUInt16(), test.GetProperty("symbol").GetUInt16(),
        test.GetProperty("category").GetInt32(), test.GetProperty("tag").GetUInt64(),
        test.GetProperty("length").GetByte(), test.GetProperty("flags").GetByte(),
        U32(test, "header"), U32(test, "priorHeader"), test.GetProperty("permission").GetByte(),
        categories, cell, Hex(test, "residue"));
    Require(request.GetSnapshot().Span.SequenceEqual(Hex(test, "expected")) &&
        cell.Value == test.GetProperty("expectedCost").GetInt32(), "Independent full80 alternative request differs.");
    packets++;
}
Console.WriteLine($"PASS {packets} independent full80 alternative request controls: actual FirstIds, related-row flag and parent-only cost.");

int restarts = 0;
foreach (var test in controls.GetProperty("restarts").EnumerateArray())
{
    var children = test.GetProperty("children").EnumerateArray().Select(row =>
        new LegacyPreparedDictionaryRestartTags(row[0].GetUInt64(), row[1].GetUInt64())).ToArray();
    Require(LegacyPreparedDictionaryRestartTags.ResolveAggregate(test.GetProperty("incoming").GetUInt64(), children) ==
        test.GetProperty("expected").GetUInt64(), "Independent aggregate source50 full64 restart tag differs.");
    restarts++;
}
Require(LegacyPreparedDictionaryRestartTags.CreateFresh() == new LegacyPreparedDictionaryRestartTags(8UL << 40, 1UL | 12UL << 40),
    "Literal fresh source78/80 writes differ.");
Console.WriteLine($"PASS {restarts} independent aggregate restart tags and fresh source constructor.");

int groupings = 0;
foreach (var test in controls.GetProperty("groups").EnumerateArray())
{
    var rows = test.GetProperty("rows").EnumerateArray().Select(row => (ReadOnlyMemory<byte>)Convert.FromHexString(row.GetString()!)).ToArray();
    var groups = LegacyNativeAlternativeWordQueryPreparation.GroupSecondaryRows(rows, test.GetProperty("primary").GetUInt16(),
        U32(test, "span"), test.GetProperty("limited").GetBoolean(), test.GetProperty("limit").GetByte());
    var expected = test.GetProperty("expected").EnumerateArray().ToArray();
    Require(groups.Count == expected.Length, "Independent eligible group count differs.");
    for (int i = 0; i < groups.Count; i++)
    {
        Require(groups[i].RestartTag28 == expected[i].GetProperty("key").GetUInt64() &&
            groups[i].SmallState21 == expected[i].GetProperty("state").GetByte() &&
            groups[i].EligibleRowOrdinals.Span.SequenceEqual(expected[i].GetProperty("ordinals").EnumerateArray().Select(x => x.GetUInt16()).ToArray()),
            "Independent consecutive grouping/order or signed limit differs.");
        var query = new LegacyPreparedDictionaryQuery { Capacity = 19, ContextRoot = 7, Skip = 33, ParentContext = 9 };
        ulong? tag = LegacyNativeAlternativeWordQueryPreparation.Apply(query,
            new(0, 0, true, false, 99), groups[i], 123, U32(test, "span"), 0x610);
        Require(tag == groups[i].RestartTag28 && query.Node == unchecked((int)(uint)tag!.Value) && query.ParentContext == 0 &&
            query.Category18 == groups[i].SmallState21 && query.Parameter19 == 0 && query.Parameter1c == 123 &&
            query.Parameter20 == U32(test, "span") && query.Flags24 == 0x104 && query.Flags26 == 0x610 &&
            query.Skip == 0 && query.Capacity == 19 && query.ContextRoot == 7, "Alternative query fields or preserved owner fields differ.");
    }
    groupings++;
}
Console.WriteLine($"PASS {groupings} independent secondary grouping/query controls including signed span overflow.");

int reductions = 0;
foreach (var test in controls.GetProperty("reductions").EnumerateArray())
{
    var reduction = new LegacyNativeAlternativeWordArcReduction();
    foreach (var op in test.GetProperty("operations").EnumerateArray())
        reduction.TryImprove(reduction.FindOrAdd(op[0].GetUInt16()), op[1].GetInt32(), op[2].GetInt32(),
            op[3].GetUInt16(), op[4].GetByte(), op[5].GetByte(), test.GetProperty("boundary").GetBoolean());
    var expected = test.GetProperty("expected").EnumerateArray().ToArray();
    var actual = reduction.GetSnapshot().Span;
    Require(actual.Length == expected.Length, "Native reducer distinct-arc count differs.");
    for (int i = 0; i < actual.Length; i++)
        Require(actual[i] == new LegacyNativeAlternativeWordArcBest(expected[i][0].GetUInt16(), expected[i][1].GetInt32(),
            expected[i][2].GetInt32(), expected[i][3].GetUInt16(), expected[i][4].GetByte()), "Independent strict signed/tie-preserving arc reduction differs.");
    reductions++;
}
Console.WriteLine($"PASS {reductions} independent strict signed reductions, first-parent ties and callback flag scopes.");

int guards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidDataException or InvalidOperationException or NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new InvalidDataException("Malformed alternative state accepted.");
}
var full = new LegacyNativeAlternativeWordArcReduction();
for (ushort i = 0; i < 1024; i++) full.FindOrAdd(i);
Require(full.Full && full.FindOrAdd(7) == 7, "Native arc capacity or existing identity lookup differs.");
Reject(() => full.FindOrAdd(1024));
Reject(() => full.TryImprove(1024, 0, 0, 0, 0, 0, false));
Reject(() => LegacyPreparedDictionaryRestartTags.ResolveAggregate(0, [LegacyPreparedDictionaryRestartTags.CreateFresh()]));
Reject(() => LegacyPreparedDictionaryRestartTags.ResolveAggregate(2UL << 32, [LegacyPreparedDictionaryRestartTags.CreateFresh()]));
Reject(() => LegacyNativeAlternativeWordQueryPreparation.GroupSecondaryRows([new byte[87]], 0, 0, false, 0));
Reject(() => LegacyNativeAlternativeWordQueryPreparation.GroupSecondaryRows([], 1, 0, false, 0));
var unprepared = new byte[88];
BinaryPrimitives.WriteUInt64LittleEndian(unprepared.AsSpan(40), ulong.MaxValue);
Reject(() => LegacyNativeAlternativeWordQueryPreparation.GroupSecondaryRows([unprepared], 0, 0, false, 0));
Reject(() => LegacyNativeAlternativeWordQueryPreparation.GroupSecondaryRows([], 0, 0, false, 0, new CancellationToken(true)));
var queryGuard = new LegacyPreparedDictionaryQuery { Node = 67, Skip = 13 };
var groupGuard = new LegacyNativeAlternativeWordQueryGroup(0, new ushort[] { 0 }, 8UL << 40, 0);
Require(LegacyNativeAlternativeWordQueryPreparation.Apply(queryGuard, new(1, 2, false, false, 99), groupGuard, 1, 3, 0) is null &&
    queryGuard.Node == 67 && queryGuard.Skip == 13, "Early native mode limit mutated query state.");
Reject(() => LegacyNativeAlternativeWordQueryPreparation.Apply(queryGuard, default, groupGuard with { SmallState21 = 64 }, 1, 0, 0));
Reject(() => LegacyNativeWordAlternativeRequestFactory.Create(1, 0, 0, new byte[88], new byte[87], 0, 0, 0, 0, 0, 0, 0, 0, 0,
    categories, new(1, 0), new byte[80]));
Console.WriteLine($"PASS {guards} capacity, ownership, unprepared restart and cancellation guards.");
ActualAlternativeCandidates.Run(args);

static byte[] Hex(JsonElement value, string name) => Convert.FromHexString(value.GetProperty(name).GetString()!);
static uint U32(JsonElement value, string name) => value.GetProperty(name).GetUInt32();
static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
