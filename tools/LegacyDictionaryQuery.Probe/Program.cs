using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length is not (1 or 4)) throw new ArgumentException("controls [RU acoustic lexical engine]");
using var data = JsonDocument.Parse(File.ReadAllBytes(args[0]));
var root = data.RootElement;
var lexicon = LegacyPackedLexicon.Load(root.GetProperty("dictionary").GetBytesFromBase64());
ushort[] ReadMap(string name) => root.GetProperty(name).EnumerateArray().Select(v => v.GetUInt16()).ToArray();
var preserved = ReadMap("preserved"); var working = ReadMap("working"); var auxiliary = ReadMap("auxiliary");
var contexts = root.GetProperty("contexts").EnumerateArray().Select(v => v.GetUInt64()).ToArray();
var map = new LegacyPreparedDictionaryWorkingMap(600, 0xc00, preserved, auxiliary);
var permissions = Enumerable.Range(0, 600).Select(i => (byte)(i % 4 == 0 ? 0 : 1)).ToArray();
map = map.Refresh(permissions);
Require(map.CopySelectedMap(false, 0).SequenceEqual(working), "Fixture refresh differs.");
int operations = 0, records = 0;
foreach (var stream in root.GetProperty("cases").EnumerateArray())
{
    var source = new LegacyPreparedDictionaryQuerySource(lexicon, map, contexts);
    var queries = new Dictionary<int, LegacyPreparedDictionaryQuery>();
    foreach (var item in stream.EnumerateArray())
    {
        if (item.GetProperty("reset").GetBoolean()) source.ResetQueries();
        int identifier = item.GetProperty("id").GetInt32();
        if (!queries.TryGetValue(identifier, out var query)) queries[identifier] = query = new();
        query.Node = item.GetProperty("node").GetInt32(); query.Skip = item.GetProperty("skip").GetInt32();
        query.Capacity = item.GetProperty("capacity").GetInt32(); query.Category18 = item.GetProperty("category").GetByte();
        query.Parameter1c = item.GetProperty("p1").GetUInt32(); query.Parameter20 = item.GetProperty("p2").GetUInt32();
        query.Flags24 = item.GetProperty("flags").GetUInt16(); query.Flags26 = item.GetProperty("flags26").GetUInt16();
        query.ParentContext = item.GetProperty("parent").GetUInt64(); query.ContextRoot = item.GetProperty("root").GetUInt64();
        var expectedTrace = item.GetProperty("trace").EnumerateArray().ToArray();
        int called = 0, linked = 0;
        var page = source.Read(query, item.GetProperty("aux").GetBoolean(), (parent, symbol) =>
        {
            var expected = expectedTrace[called++];
            Require(parent == expected[0].GetUInt64() && symbol == expected[1].GetUInt16(), "Wrong context parent or acoustic symbol.");
            return new(expected[2].GetUInt64(), expected[3].GetBoolean());
        }, (child, parent) =>
        {
            linked++;
            Require(expectedTrace.Any(t => t[0].GetUInt64() == parent && t[2].GetUInt64() == child && t[3].GetBoolean()), "Wrong created context linkage.");
        });
        Require(page.ReturnedCount == item.GetProperty("count").GetInt32(), "Wrong signed return count.");
        Require(page.AcceptedCount == item.GetProperty("accepted").GetInt32(), "Wrong accepted counter.");
        Require(page.NextChild == item.GetProperty("nextChild").GetInt32(), $"Wrong next raw child: node={query.Node}, skip={query.Skip}, capacity={query.Capacity}, returned={page.ReturnedCount}, actual={page.NextChild}, expected={item.GetProperty("nextChild").GetInt32()}.");
        Require(page.SlotIndex == item.GetProperty("slot").GetInt32(), "Wrong eight-slot ring identity.");
        Require(page.Records.Span.SequenceEqual(item.GetProperty("records").GetBytesFromBase64()), "Complete source record differs.");
        Require(called == expectedTrace.Length && linked == expectedTrace.Count(t => t[3].GetBoolean()), "Context callback count differs.");
        operations++; records += Math.Max(0, page.ReturnedCount);
    }
}
Console.WriteLine($"PASS 128 independent source-query streams/{operations} operations/{records} complete records, three-block fanout, native root skip, eight-slot reuse/reset, map override, terminal gate, opaque context lookup/link and signed out-of-range returns.");

// Sequential paging keeps the same signature and tests retained block cursors.
var paging = new LegacyPreparedDictionaryQuerySource(lexicon, map, contexts);
var cursor = new LegacyPreparedDictionaryQuery { Node = 0, Capacity = 7 };
var all = paging.Read(new() { Node = 0, Capacity = 512 }, false).Records.ToArray();
var reconstructed = new List<byte>();
while (true)
{
    var page = paging.Read(cursor, false, clearExtraCosts: true);
    Require(page.ClearedExtraCosts.Span.SequenceEqual(new byte[Math.Max(0, page.ReturnedCount) * 12]), "Extra costs not cleared.");
    if (page.ReturnedCount <= 0) break;
    reconstructed.AddRange(page.Records.ToArray()); cursor.Skip += page.ReturnedCount;
}
Require(reconstructed.SequenceEqual(all), "Sequential pages did not reconstruct the exact source order.");
var liveMaps = new LegacyPreparedDictionaryQuerySource(lexicon, map, contexts);
var liveRequest = new LegacyPreparedDictionaryQuery { Node = 0, Capacity = 5 };
var before = liveMaps.Read(liveRequest, false);
liveRequest.Skip = before.ReturnedCount;
var restoredMap = map.Refresh(Enumerable.Repeat((byte)1, 600).ToArray());
liveMaps.SetWorkingMap(restoredMap);
var retainedCursor = liveMaps.Read(liveRequest, false);
var scratch = new LegacyPreparedDictionaryQuerySource(lexicon, restoredMap, contexts);
var freshRequest = new LegacyPreparedDictionaryQuery { Node = 0, Capacity = 5, Skip = before.ReturnedCount };
var recounted = scratch.Read(freshRequest, false);
Require(!retainedCursor.Records.Span.SequenceEqual(recounted.Records.Span), "A permission refresh silently recounted the native cached cursor.");
liveMaps.ResetQueries();
Require(liveMaps.Read(liveRequest, false).Records.Span.SequenceEqual(recounted.Records.Span), "Explicit source reset failed to recount permissions.");
liveRequest.Skip += recounted.ReturnedCount;
liveRequest.Category18 = 199; liveRequest.ParentContext = 10; liveRequest.ContextRoot = 0;
var metadataPage = liveMaps.Read(liveRequest, false);
for (int i = 0; i < metadataPage.ReturnedCount; i++)
{
    var record = metadataPage.Records.Span.Slice(i * 32, 32);
    Require(record[22] == 199 && BinaryPrimitives.ReadUInt64LittleEndian(record[8..]) == 0,
        "Non-key category/context metadata was not forwarded at continuation.");
}
Throws<ArgumentException>(() => paging.Read(new() { Capacity = 0 }, false));
Throws<ArgumentException>(() => paging.Read(new() { Node = lexicon.NodeCount }, false));
Throws<ArgumentException>(() => paging.Read(new() { Node = 1, Skip = 1 }, false, clearExtraCosts: true));
Throws<OperationCanceledException>(() => paging.Read(cursor, false, cancellationToken: new(true)));
Throws<NotSupportedException>(() => paging.Read(new() { Node = 2, ParentContext = 1, ContextRoot = 1 }, false));
var reentrant = new LegacyPreparedDictionaryQuery { Node = 2, ParentContext = 1, ContextRoot = 1 };
paging.Read(reentrant, false, (_, _) =>
{
    Throws<InvalidOperationException>(() => paging.ResetQueries());
    Throws<InvalidOperationException>(() => paging.Read(cursor, false));
    return new(0, false);
}, (_, _) => { });
Console.WriteLine("PASS retained page/block cursors, permission refresh versus explicit recount, non-key category/context forwarding, extra-cost zeroing, bounds, missing context callbacks, precancellation and reentrant mutation guards.");

if (args.Length == 4)
{
    var bundle = LegacyRussianModelBundle.Load(File.ReadAllBytes(args[1]), File.ReadAllBytes(args[2]), File.ReadAllBytes(args[3]));
    var initial = LegacyLinguisticSymbolMaskController.CreateExactDictionaryMap(bundle.AcousticAlphabet, bundle.LexicalAlphabet);
    var actualMap = new LegacyPreparedDictionaryWorkingMap(bundle.AcousticAlphabet.LabelCount, 0xc00, initial, []);
    var native = new LegacyPreparedDictionaryQuerySource(bundle.Lexicon, actualMap, new ulong[initial.Length]);
    var request = new LegacyPreparedDictionaryQuery { Node = 0, Capacity = 13, Category18 = 7 };
    var candidates = new List<int>();
    while (true)
    {
        var page = native.Read(request, false);
        if (page.ReturnedCount <= 0) break;
        for (int i = 0; i < page.ReturnedCount; i++)
        {
            var record = page.Records.Span.Slice(i * 32, 32);
            int next = BinaryPrimitives.ReadInt32LittleEndian(record);
            Require(record[5] == (byte)(1 | (bundle.Lexicon.IsTerminal(next) ? 2 : 0)) && record[22] == 7, "Actual candidate flag/category changed.");
            Require(BinaryPrimitives.ReadUInt16LittleEndian(record[20..]) == initial[bundle.Lexicon.GetLabel(next)], "Actual selected map changed.");
            candidates.Add(next);
        }
        request.Skip += page.ReturnedCount;
    }
    var range = bundle.Lexicon.GetChildRange(0);
    var expected = Enumerable.Range(range.Start + 1, range.End - range.Start - 1)
        .Where(node => initial[bundle.Lexicon.GetLabel(node)] != ushort.MaxValue).ToArray();
    Require(candidates.SequenceEqual(expected), "Actual packed RU root pagination differs from native root interval/gate.");
    Require(native.Read(new() { Node = 1, Capacity = 1 }, false).ReturnedCount == 0, "Actual native descriptor probe node1 is not a leaf.");
    Console.WriteLine($"PASS actual RU single-packed backend -> native root skip -> owned source32 records: {candidates.Count} candidates; explicit category7 and zero context identities, no native word-winner claim.");
}

static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
static void Throws<T>(Action action) where T : Exception
{
    try { action(); } catch (T) { return; }
    throw new InvalidDataException($"Expected {typeof(T).Name}.");
}
