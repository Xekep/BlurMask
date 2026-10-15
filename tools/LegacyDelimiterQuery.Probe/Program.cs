using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 4) throw new ArgumentException("controls.json AK LK engine.json required");
byte[] acoustic = File.ReadAllBytes(args[1]), lexical = File.ReadAllBytes(args[2]);
var model = LegacyRussianModelBundle.Load(acoustic, lexical, File.ReadAllBytes(args[3]));
Require(model.AcousticAlphabet.LabelCount == 192 && model.AcousticAlphabet.TryGetLabel("\u200d", out ushort zwj) && zwj == 184,
    "Pinned actual alphabet required by independent fixture protocol classification.");
Require(LegacyNativeDelimiterQuerySource.ReadV10dCoordinateMode(acoustic) == 2,
    "Actual1D4950/1D60C0 source coordinate-mode producer differs.");
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int fixtures = 0, records = 0, pages = 0, backendRequests = 0;
foreach (var test in document.RootElement.EnumerateArray())
{
    var nodes = test.GetProperty("nodes").EnumerateArray()
        .Select(node => node.EnumerateArray().Select(row => Convert.FromHexString(row.GetString()!)).ToArray()).ToArray();
    var expected = test.GetProperty("expected").EnumerateArray().ToArray();
    var contexts = new LegacySymbolContextFactory();
    int requests = 0;
    LegacyAggregateDictionaryQueryPage Backend(ulong tag, LegacyPreparedDictionaryQuery query, CancellationToken token)
    {
        token.ThrowIfCancellationRequested(); requests++;
        Require(query.Node == (uint)tag && query.Capacity == 128 && query.ContextRoot == contexts.ContainerIdentity,
            "Native delimiter backend request lost source tag, block capacity or context ownership.");
        var input = nodes[query.Node];
        int count = Math.Min(query.Capacity, Math.Max(0, input.Length - query.Skip));
        byte[] bytes = new byte[count * 32];
        for (int index = 0; index < count; index++)
        {
            var row = input[query.Skip + index].ToArray();
            ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(20));
            var context = query.ParentContext == 0 ? contexts.ResolveInitial(symbol, token) : contexts.ResolveChild(query.ParentContext, symbol, token);
            if (context.Created && query.ParentContext != 0) contexts.LinkCreatedContext(context.Identity, query.ParentContext);
            BinaryPrimitives.WriteUInt64LittleEndian(row.AsSpan(8), context.Identity);
            row.CopyTo(bytes.AsSpan(index * 32));
        }
        return new(count, bytes, ReadOnlyMemory<byte>.Empty, 0, query.Skip, null);
    }
    var source = new LegacyNativeDelimiterQuerySource(model.AcousticAlphabet, contexts, Backend, test.GetProperty("mode").GetInt32());
    var query = new LegacyPreparedDictionaryQuery
    {
        Node = unchecked((int)(uint)test.GetProperty("tag").GetUInt64()),
        Capacity = test.GetProperty("capacity").GetInt32(), ContextRoot = contexts.ContainerIdentity,
        Flags24 = test.GetProperty("flags").GetUInt16(), Parameter1c = test.GetProperty("first").GetUInt32(),
        Parameter20 = test.GetProperty("span").GetUInt32()
    };
    var wrapped = new LegacyNativeDelimiterQuery { SourceTag = test.GetProperty("tag").GetUInt64(), Request = query };
    int ordinal = 0;
    for (int iteration = 0; ; iteration++)
    {
        Require(iteration < 100000, "Delimiter pagination did not terminate.");
        var page = source.Read(wrapped);
        pages++;
        Require(page.ReturnedCount >= 0 && page.Records.Length == page.ReturnedCount * 32, "Delimiter result extent differs.");
        if (page.ReturnedCount == 0) break;
        for (int index = 0; index < page.ReturnedCount; index++)
        {
            Require(ordinal < expected.Length, "Delimiter emitted extra original-oracle rows.");
            var row = page.Records.Span.Slice(index * 32, 32);
            var control = expected[ordinal++];
            byte[] expectedBytes = Convert.FromHexString(control.GetProperty("record").GetString()!);
            var actualPath = Path(contexts, BinaryPrimitives.ReadUInt64LittleEndian(row[8..]));
            Require(actualPath.SequenceEqual(control.GetProperty("path").EnumerateArray().Select(value => value.GetUInt16())),
                $"Literal source context differs at fixture{fixtures}/row{ordinal}.");
            var comparable = row.ToArray(); comparable.AsSpan(8, 8).Clear();
            if (control.GetProperty("extra").ValueKind != JsonValueKind.Null)
            {
                Require(Path(contexts, BinaryPrimitives.ReadUInt64LittleEndian(row[24..]))
                    .SequenceEqual(control.GetProperty("extra").EnumerateArray().Select(value => value.GetUInt16())),
                    "Flags100 must transport the literal control root in24 instead of grafting context08.");
                comparable.AsSpan(24, 8).Clear(); expectedBytes.AsSpan(24, 8).Clear();
            }
            Require(comparable.SequenceEqual(expectedBytes), $"Full32 delimiter row differs at fixture{fixtures}/row{ordinal}.");
            records++;
        }
        query.Skip += page.ReturnedCount;
    }
    Require(ordinal == expected.Length, $"Delimiter omitted original-oracle rows in fixture{fixtures}: {ordinal}/{expected.Length}.");
    //Rewind uses the same outer slot and resets exact backend offsets.
    query.Skip = 0;
    var rewind = source.Read(wrapped);
    Require(rewind.ReturnedCount == Math.Min(query.Capacity, expected.Length), "Native cache rewind count differs.");
    backendRequests += requests; fixtures++;
}
//Actual2B4/2B7 returns an FFFB protocol edge. Its parent is found from
//the validated tree itself; no expected word or handwritten label is supplied.
var actualContexts = new LegacySymbolContextFactory();
var maps = LegacyDictionaryInitialSymbolMaps.Prepare(model.AcousticAlphabet, model.LexicalAlphabet, actualContexts, []);
ushort[] primary = maps.CopyPrimary();
int actualParent = -1;
for (int node = 1; node < model.Lexicon.NodeCount; node++)
{
    var range = model.Lexicon.GetChildRange(node);
    if (Enumerable.Range(range.Start, range.End - range.Start).Any(child => primary[model.Lexicon.GetLabel(child)] == 0xfffb))
    { actualParent = node; break; }
}
Require(actualParent >= 0, "Actual dictionary no longer contains the scoped protocol leaf.");
var descriptor = LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(
    LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lexical.AsSpan(606964, 4565174 - 606964)),
    model.Lexicon, LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lexical.AsSpan(2513123, 4565174 - 2513123)), true);
var rawSource = maps.CreateQuerySource(model.Lexicon, descriptor.Flags);
var aggregate = new LegacyPreparedAggregateDictionaryQuerySource([LegacyAggregateDictionaryChild.BindDictionary(rawSource, actualContexts)]);
int seenProtocol = 0;
var actualWrapper = new LegacyNativeDelimiterQuerySource(model.AcousticAlphabet, actualContexts,
    (tag, request, token) =>
    {
        var page = aggregate.Read(tag, request, cancellationToken: token);
        for (int offset = 0; offset < page.Records.Length; offset += 32)
            if (BinaryPrimitives.ReadUInt16LittleEndian(page.Records.Span[(offset + 20)..]) == 0xfffb) seenProtocol++;
        return page;
    }, LegacyNativeDelimiterQuerySource.ReadV10dCoordinateMode(acoustic));
var actualQuery = new LegacyNativeDelimiterQuery
{
    SourceTag = (uint)actualParent | 1UL << 32,
    Request = new() { Node = actualParent, Capacity = 128, ContextRoot = actualContexts.ContainerIdentity }
};
int actualFlattened = 0;
while (true)
{
    var actualResult = actualWrapper.Read(actualQuery);
    Require(actualResult.ReturnedCount >= 0, "Actual protocol traversal failed.");
    for (int offset = 0; offset < actualResult.Records.Length; offset += 32)
    {
        ushort label = BinaryPrimitives.ReadUInt16LittleEndian(actualResult.Records.Span[(offset + 20)..]);
        Require(label < model.AcousticAlphabet.LabelCount || unchecked((ushort)(label + 29)) < 8,
            "Protocol traversal emitted an unconsumed OOB alphabet label.");
    }
    actualFlattened += actualResult.ReturnedCount;
    if (actualResult.ReturnedCount == 0) break;
    actualQuery.Request.Skip += actualResult.ReturnedCount;
}
Require(seenProtocol > 0, "Actual protocol edge was filtered before its native traversal.");
int CountActualTree(int node, int depth)
{
    int result = 0;
    var range = model.Lexicon.GetChildRange(node);
    for (int child = range.Start; child < range.End; child++)
    {
        ushort label = primary[model.Lexicon.GetLabel(child)];
        if (label is ushort.MaxValue or 184) continue;
        if (label >= 192 && unchecked((ushort)(label + 29)) >= 8)
        {
            if (label is not (0xfff7 or 0xfff9) && depth < 7) result += CountActualTree(child, depth + 1);
        }
        else result++;
    }
    return result;
}
Require(actualFlattened == CountActualTree(actualParent, 0), "Actual raw tree traversal differs from native query wrapper output count.");

//Independent exact callback log for237610's do/while: a NEW empty level
//fetches twice, while the same exhausted query fetches once on its next call.
var emptyContexts = new LegacySymbolContextFactory();
var emptyLog = new List<(ulong Tag, int Skip)>();
var emptySource = new LegacyNativeDelimiterQuerySource(model.AcousticAlphabet, emptyContexts,
    (tag, request, token) =>
    {
        emptyLog.Add((tag, request.Skip));
        return new(0, ReadOnlyMemory<byte>.Empty, ReadOnlyMemory<byte>.Empty, 0, request.Skip, null);
    }, 2);
var emptyQuery = new LegacyNativeDelimiterQuery { SourceTag = 0, Request = new() { ContextRoot = emptyContexts.ContainerIdentity } };
Require(emptySource.Read(emptyQuery).ReturnedCount == 0 && emptyLog.SequenceEqual(new[] { (0UL, 0), (0UL, 0) }),
    "New empty290 source must preserve native duplicate initial read.");
emptySource.Read(emptyQuery);
Require(emptyLog.SequenceEqual(new[] { (0UL, 0), (0UL, 0), (0UL, 0) }), "Exhausted same-query backend read differs.");

//A live initialized context owner with a null QUERY context root must not
//allocate a concatenated suffix. Native2382B0 explicitly storesNULL in08.
var nullContexts = new LegacySymbolContextFactory();
ulong initialLeaf = nullContexts.ResolveInitial(7).Identity;
var nullSource = new LegacyNativeDelimiterQuerySource(model.AcousticAlphabet, nullContexts,
    (_, request, token) =>
    {
        byte[] row = request.Skip == 0 ? new byte[32] : [];
        if (row.Length != 0) { BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(20), 7); BinaryPrimitives.WriteUInt64LittleEndian(row.AsSpan(8), initialLeaf); }
        return new(row.Length / 32, row, ReadOnlyMemory<byte>.Empty, 0, request.Skip, null);
    }, 2);
var nullQuery = new LegacyNativeDelimiterQuery
{
    SourceTag = 8UL << 40, Request = new() { Capacity = 1, Parameter1c = 1 }
};
var nullPage = nullSource.Read(nullQuery);
Require(nullPage.ReturnedCount == 1 && BinaryPrimitives.ReadUInt64LittleEndian(nullPage.Records.Span[8..]) == 0,
    "Null query context must preserve native null suffix result.");

//The ninth independent outer request evicts slot0. Resuming the original
//request must restart its backend traversal and skip its already emitted row.
var ringContexts = new LegacySymbolContextFactory();
int ringCalls = 0;
var ringSource = new LegacyNativeDelimiterQuerySource(model.AcousticAlphabet, ringContexts,
    (_, request, token) =>
    {
        ringCalls++;
        int count = request.Skip < 2 ? 2 - request.Skip : 0;
        byte[] bytes = new byte[count * 32];
        for (int i = 0; i < count; i++)
        {
            ushort symbol = (ushort)(request.Skip + i + 7);
            BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(i * 32 + 20), symbol);
            BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(i * 32 + 8), ringContexts.ResolveInitial(symbol, token).Identity);
        }
        return new(count, bytes, ReadOnlyMemory<byte>.Empty, 0, request.Skip, null);
    }, 2);
var ringQueries = Enumerable.Range(0, 9).Select(i => new LegacyNativeDelimiterQuery
{
    SourceTag = (uint)i,
    Request = new() { Node = i, Capacity = 1, ContextRoot = ringContexts.ContainerIdentity }
}).ToArray();
for (int i = 0; i < ringQueries.Length; i++)
{
    var page = ringSource.Read(ringQueries[i]);
    Require(page.SlotIndex == (i & 7) && page.ReturnedCount == 1 &&
        BinaryPrimitives.ReadUInt16LittleEndian(page.Records.Span[20..]) == 7,
        "Eight-slot native ring allocation/order differs.");
}
ringQueries[0].Request.Skip = 1;
var resumed = ringSource.Read(ringQueries[0]);
Require(resumed.SlotIndex == 0 && resumed.ReturnedCount == 1 && ringCalls == 10 &&
    BinaryPrimitives.ReadUInt16LittleEndian(resumed.Records.Span[20..]) == 8 && resumed.AcceptedCount == 2,
    "Evicted native ring query did not restart and preserve global accepted skip.");

int guards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new InvalidDataException("Invalid native delimiter lifecycle accepted.");
}
Reject(() => new LegacyNativeDelimiterQuerySource(model.AcousticAlphabet, nullContexts, (_, _, _) => throw new Exception(), 2, true));
Reject(() => new LegacyNativeDelimiterQuerySource(model.AcousticAlphabet, nullContexts, (_, _, _) => throw new Exception(), -1));
var failedContexts = new LegacySymbolContextFactory();
var failedSource = new LegacyNativeDelimiterQuerySource(model.AcousticAlphabet, failedContexts,
    (_, request, _) => new(-1, ReadOnlyMemory<byte>.Empty, ReadOnlyMemory<byte>.Empty, 0, request.Skip, null), 2);
var failedQuery = new LegacyNativeDelimiterQuery { SourceTag = 0, Request = new() { ContextRoot = failedContexts.ContainerIdentity } };
Require(failedSource.Read(failedQuery).ReturnedCount == -1, "Backend failure must retain the native negative return channel.");
Reject(() => failedSource.Read(failedQuery));
var foreignSource = new LegacyNativeDelimiterQuerySource(model.AcousticAlphabet, nullContexts,
    (_, _, _) => throw new InvalidDataException("Foreign context reached backend."), 2);
Reject(() => foreignSource.Read(new() { SourceTag = 0, Request = new() { ContextRoot = failedContexts.ContainerIdentity } }));
using (var canceled = new CancellationTokenSource())
{
    var canceledContexts = new LegacySymbolContextFactory();
    var canceledSource = new LegacyNativeDelimiterQuerySource(model.AcousticAlphabet, canceledContexts,
        (_, request, _) => { canceled.Cancel(); return new(0, ReadOnlyMemory<byte>.Empty, ReadOnlyMemory<byte>.Empty, 0, request.Skip, null); }, 2);
    var canceledQuery = new LegacyNativeDelimiterQuery { SourceTag = 0, Request = new() { ContextRoot = canceledContexts.ContainerIdentity } };
    Reject(() => canceledSource.Read(canceledQuery, canceled.Token));
    Reject(() => canceledSource.Read(canceledQuery));
}
Console.WriteLine($"PASS {fixtures} independent290 trees / {records} full32 rows and literal contexts / {pages} paginated outputs / {backendRequests} backend blocks of128 rows; actual sourceMode2 and {seenProtocol} dictionary FFFB edges -> {actualFlattened} flattened labels.");
Console.WriteLine($"PASS empty-source exact backend logs, null query-context output, 9-query ring eviction and {guards} lifecycle guards.");

static ushort[] Path(LegacySymbolContextFactory contexts, ulong identity)
{
    var result = new List<ushort>();
    while (identity != 0)
    {
        Require(result.Count < 4096, "Context lineage must terminate.");
        var node = contexts.ReadNode(identity); result.Add(node.Symbol); identity = node.ParentLink;
    }
    result.Reverse(); return result.ToArray();
}
static void Require(bool condition, string message) { if (!condition) throw new InvalidDataException(message); }
