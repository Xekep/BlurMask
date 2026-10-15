using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 4) throw new ArgumentException("controls AK LK engine required");
byte[] ak = File.ReadAllBytes(args[1]), lk = File.ReadAllBytes(args[2]);
var model = LegacyRussianModelBundle.Load(ak, lk, File.ReadAllBytes(args[3]));
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int fixtures = 0, rows = 0;
foreach (var test in controls.RootElement.EnumerateArray())
{
    var nodes = test.GetProperty("nodes").EnumerateArray()
        .Select(node => node.EnumerateArray().Select(row => Convert.FromHexString(row.GetString()!)).ToArray()).ToArray();
    var contexts = new LegacySymbolContextFactory();
    LegacyAggregateDictionaryQueryPage Read(ulong tag, LegacyPreparedDictionaryQuery request, CancellationToken token)
    {
        Require(request.Node == (uint)tag && request.Capacity == 128 && request.ContextRoot == contexts.ContainerIdentity,
            "Auxiliary backend request lost owned tag/context/capacity.");
        byte[][] input = nodes[request.Node];
        int count = Math.Min(128, Math.Max(0, input.Length - request.Skip));
        byte[] bytes = new byte[count * 32];
        for (int i = 0; i < count; i++)
        {
            byte[] row = input[request.Skip + i].ToArray();
            ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(20));
            var child = request.ParentContext == 0 ? contexts.ResolveInitial(symbol, token) :
                contexts.ResolveChild(request.ParentContext, symbol, token);
            if (child.Created && request.ParentContext != 0) contexts.LinkCreatedContext(child.Identity, request.ParentContext);
            BinaryPrimitives.WriteUInt64LittleEndian(row.AsSpan(8), child.Identity);
            row.CopyTo(bytes.AsSpan(i * 32));
        }
        return new(count, bytes, ReadOnlyMemory<byte>.Empty, 0, request.Skip, null);
    }
    var source = new LegacyNativeAuxiliaryDelimiterQuerySource(model.AcousticAlphabet, contexts, Read,
        test.GetProperty("node_count").GetInt32());
    ulong tag = test.GetProperty("tag").GetUInt64();
    var query = new LegacyPreparedDictionaryQuery
    {
        Node = (int)(uint)tag, Capacity = 512, ContextRoot = contexts.ContainerIdentity,
        Parameter1c = test.GetProperty("first").GetUInt32(), Flags24 = test.GetProperty("flags").GetUInt16()
    };
    var expected = test.GetProperty("expected").EnumerateArray().ToArray();
    var page = source.Read(tag, query);
    Require(page.ReturnedCount == expected.Length, $"Auxiliary count differs in fixture{fixtures}.");
    for (int i = 0; i < expected.Length; i++)
    {
        byte[] row = page.Records.Slice(i * 32, 32).ToArray();
        ulong context = BinaryPrimitives.ReadUInt64LittleEndian(row.AsSpan(8));
        Require(Path(contexts, context).SequenceEqual(expected[i].GetProperty("path").EnumerateArray().Select(x => x.GetUInt16())),
            $"Auxiliary stripped context differs fixture{fixtures}/row{i}.");
        row.AsSpan(8, 8).Clear();
        Require(row.SequenceEqual(Convert.FromHexString(expected[i].GetProperty("record").GetString()!)),
            $"Auxiliary full32 payload differs fixture{fixtures}/row{i}.");
        rows++;
    }
    fixtures++;
}

//Flat terminal pages independently check128-row backend refills and output
//capacities1/17/128 without conflating recursive cache-resume semantics.
int paginationRows = 0;
foreach (int capacity in new[] { 1, 17, 128 })
{
    var contexts = new LegacySymbolContextFactory();
    var source = new LegacyNativeAuxiliaryDelimiterQuerySource(model.AcousticAlphabet, contexts,
        (_, query, token) =>
        {
            int count = Math.Min(128, Math.Max(0, 259 - query.Skip));
            byte[] bytes = new byte[count * 32];
            for (int i = 0; i < count; i++)
            {
                var row = bytes.AsSpan(i * 32, 32);
                BinaryPrimitives.WriteUInt32LittleEndian(row, (uint)(query.Skip + i));
                row[5] = 2; row[23] = 0x10;
                BinaryPrimitives.WriteUInt16LittleEndian(row[20..], 7);
                BinaryPrimitives.WriteUInt64LittleEndian(row[8..], contexts.ResolveInitial(7, token).Identity);
            }
            return new(count, bytes, ReadOnlyMemory<byte>.Empty, 0, query.Skip, null);
        }, 4);
    var request = new LegacyPreparedDictionaryQuery { Capacity = capacity, ContextRoot = contexts.ContainerIdentity };
    while (true)
    {
        var page = source.Read(0, request);
        if (page.ReturnedCount == 0) break;
        for (int i = 0; i < page.ReturnedCount; i++)
        {
            var row = page.Records.Span.Slice(i * 32, 32);
            Require(BinaryPrimitives.ReadUInt32LittleEndian(row) == request.Skip + i && row[5] == 2 && row[23] == 0x10 &&
                BinaryPrimitives.ReadUInt64LittleEndian(row[8..]) == 0, "Flat auxiliary paging/drop order differs.");
            paginationRows++;
        }
        request.Skip += page.ReturnedCount;
    }
    Require(request.Skip == 259, "Auxiliary global accepted count differs.");
}
//A recursiveFFF6 terminal fills capacity before its own terminal is emitted.
//The next call resumes the child, then emits the parent in its original order.
int recursiveRows = 0;
foreach (int capacity in new[] { 1, 2, 128 })
{
    var contexts = new LegacySymbolContextFactory();
    var source = new LegacyNativeAuxiliaryDelimiterQuerySource(model.AcousticAlphabet, contexts,
        (_, query, token) =>
        {
            if (query.Skip != 0) return new(0, ReadOnlyMemory<byte>.Empty, ReadOnlyMemory<byte>.Empty, 0, query.Skip, null);
            byte[] row = new byte[32];
            ushort label = query.Node == 0 ? (ushort)0xfff6 : (ushort)7;
            BinaryPrimitives.WriteUInt32LittleEndian(row, 1); row[5] = 2;
            BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(20), label);
            BinaryPrimitives.WriteInt32LittleEndian(row.AsSpan(16), query.Node == 0 ? 10 : 4);
            row[22] = query.Node == 0 ? (byte)2 : (byte)3; row[23] = query.Node == 0 ? (byte)0 : (byte)0x10;
            var leaf = query.ParentContext == 0 ? contexts.ResolveInitial(label, token) : contexts.ResolveChild(query.ParentContext, label, token);
            if (leaf.Created && query.ParentContext != 0) contexts.LinkCreatedContext(leaf.Identity, query.ParentContext);
            BinaryPrimitives.WriteUInt64LittleEndian(row.AsSpan(8), leaf.Identity);
            return new(1, row, ReadOnlyMemory<byte>.Empty, 0, query.Skip, null);
        }, 3);
    var request = new LegacyPreparedDictionaryQuery { Capacity = capacity, ContextRoot = contexts.ContainerIdentity };
    var actual = new List<byte[]>();
    while (true)
    {
        var page = source.Read(0, request);
        if (page.ReturnedCount == 0) break;
        for (int i = 0; i < page.ReturnedCount; i++) actual.Add(page.Records.Slice(i * 32, 32).ToArray());
        request.Skip += page.ReturnedCount;
        Require(request.Skip <= 2, "Recursive auxiliary query duplicated its terminal rows.");
    }
    Require(actual.Count == 2, "Recursive auxiliary query lost a terminal.");
    for (int i = 0; i < 2; i++)
    {
        byte[] row = actual[i];
        Require(BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(20)) == (i == 0 ? 7 : 0xfff6) &&
            BinaryPrimitives.ReadInt32LittleEndian(row.AsSpan(16)) == (i == 0 ? 14 : 10) &&
            row[22] == (i == 0 ? 5 : 2) && row[23] == (i == 0 ? 0x18 : 8) &&
            Path(contexts, BinaryPrimitives.ReadUInt64LittleEndian(row.AsSpan(8))).SequenceEqual(new ushort[] { 0xfff6 }),
            "Literal recursive auxiliary resume/cost/context ordering differs.");
        recursiveRows++;
    }
}

//2391DB resets the OUTER cache tag to-1 on parameter refresh. The second
//page replays once from the backend, while the third uses the cached cursor.
var replayContexts = new LegacySymbolContextFactory();
var replayLog = new List<int>();
var replaySource = new LegacyNativeAuxiliaryDelimiterQuerySource(model.AcousticAlphabet, replayContexts,
    (_, request, token) =>
    {
        replayLog.Add(request.Skip);
        int count = Math.Max(0, 3 - request.Skip);
        byte[] bytes = new byte[count * 32];
        for (int i = 0; i < count; i++)
        {
            var row = bytes.AsSpan(i * 32, 32);
            BinaryPrimitives.WriteUInt32LittleEndian(row, (uint)(request.Skip + i));
            row[5] = 2; row[23] = 0x10;
            BinaryPrimitives.WriteUInt16LittleEndian(row[20..], 7);
            BinaryPrimitives.WriteUInt64LittleEndian(row[8..], replayContexts.ResolveInitial(7, token).Identity);
        }
        return new(count, bytes, ReadOnlyMemory<byte>.Empty, 0, request.Skip, null);
    }, 5);
var replayQuery = new LegacyPreparedDictionaryQuery { Capacity = 1, ContextRoot = replayContexts.ContainerIdentity };
for (int i = 0; i < 3; i++)
{
    var page = replaySource.Read(0, replayQuery);
    Require(page.ReturnedCount == 1 && BinaryPrimitives.ReadUInt32LittleEndian(page.Records.Span) == i,
        "Auxiliary parameter-refresh replay changed accepted output order.");
    Require(replayLog.SequenceEqual(i == 0 ? new[] { 0 } : new[] { 0, 0 }),
        "Auxiliary first/second/third-page backend replay log differs.");
    replayQuery.Skip++;
}
Require(replaySource.Read(0, replayQuery).ReturnedCount == 0 && replayLog.SequenceEqual(new[] { 0, 0, 3 }),
    "Auxiliary exhausted replay cache read differs.");

//Real2B7 auxiliary60 contains only the actual ZWJ and FFFB labels here.
//Derive expected terminal records directly from packed trie flags/labels;
//neither expected text nor a language winner is supplied to recognition.
var actualContexts = new LegacySymbolContextFactory();
var maps = LegacyDictionaryInitialSymbolMaps.Prepare(model.AcousticAlphabet, model.LexicalAlphabet, actualContexts, []);
ushort[] auxiliaryMap = maps.CopyAuxiliary();
Require(auxiliaryMap.Where(label => label != ushort.MaxValue).All(label => label is 184 or 0xfffb),
    "Actual auxiliary alphabet scope changed.");
int actualParent = -1;
int mappedTerminalEdges = 0;
for (int node = 1; node < model.Lexicon.NodeCount; node++)
{
    var children = model.Lexicon.GetChildRange(node);
    foreach (int child in Enumerable.Range(children.Start, children.End - children.Start))
        if (auxiliaryMap[model.Lexicon.GetLabel(child)] != ushort.MaxValue)
        {
            if (actualParent == -1) actualParent = node;
            if (model.Lexicon.IsTerminal(child)) mappedTerminalEdges++;
        }
}
Require(actualParent > 0 && mappedTerminalEdges == 0,
    "Checked direct auxiliary map/tree no-terminal-edge scope changed.");
var sourceDescriptor = LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(
    LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lk.AsSpan(606964, 4565174 - 606964)),
    model.Lexicon, LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lk.AsSpan(2513123, 4565174 - 2513123)), true);
var raw = maps.CreateQuerySource(model.Lexicon, sourceDescriptor.Flags);
var aggregate = new LegacyPreparedAggregateDictionaryQuerySource([LegacyAggregateDictionaryChild.BindDictionary(raw, actualContexts)]);
int actualCalls = 0;
var actualSource = new LegacyNativeAuxiliaryDelimiterQuerySource(model.AcousticAlphabet, actualContexts,
    (tag, query, token) => { actualCalls++; return aggregate.Read(tag, query, auxiliary: true, cancellationToken: token); }, 5);
var actualQuery = new LegacyPreparedDictionaryQuery { Node = actualParent, Capacity = 512, ContextRoot = actualContexts.ContainerIdentity };
var actualPage = actualSource.Read((uint)actualParent | 1UL << 32, actualQuery);
var actualExpected = new List<byte[]>();
void EnumerateActual(int node, int depth, byte inheritedKind)
{
    var children = model.Lexicon.GetChildRange(node);
    for (int child = children.Start; child < children.End; child++)
    {
        ushort label = auxiliaryMap[model.Lexicon.GetLabel(child)];
        if (!model.Lexicon.IsTerminal(child) || label == ushort.MaxValue) continue;
        byte kind = label == 0xfffb && inheritedKind != 1 ? (byte)2 : inheritedKind;
        if (depth < 7) EnumerateActual(child, depth + 1, kind);
        byte[] expected = new byte[32];
        BinaryPrimitives.WriteUInt32LittleEndian(expected, (uint)child);
        expected[4] = 1; expected[5] = (byte)((node == 0 ? 1 : 0) | (label == 184 ? 4 : 2));
        BinaryPrimitives.WriteUInt16LittleEndian(expected.AsSpan(20), label);
        expected[23] = kind;
        actualExpected.Add(expected);
    }
}
EnumerateActual(actualParent, 0, 0);
Require(actualExpected.Count == 0 && actualPage.ReturnedCount == actualExpected.Count && actualCalls == 2 &&
    actualPage.Records.Span.SequenceEqual(actualExpected.SelectMany(row => row).ToArray()),
    "Real auxiliary290 differs from the independently enumerated terminal-trie records.");
var outerPages = new LegacyNativeAuxiliaryWordQueryPages(
    (tag, request, token) =>
    {
        var page = actualSource.Read(tag, request, token);
        return new(page.ReturnedCount, page.Records, ReadOnlyMemory<byte>.Empty, -1, request.Skip, null);
    }, actualContexts, 128, new byte[4096]);
byte[] currentStored = new byte[88];
BinaryPrimitives.WriteUInt32LittleEndian(currentStored, (uint)actualParent); currentStored[4] = 1;
var outerResult = outerPages.Run(currentStored, 0, new(0, 0, false, false, 120), 0, false, false,
    (_, _) => throw new InvalidDataException("Empty actual auxiliary source emitted a fabricated terminal."), 10);
Require(outerResult.Completed && outerResult.SourceReadCount == 1 && outerResult.ConsumedPages.Count == 0,
    "V48 delegate consumer changed the actual no-terminal source result.");

int guards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException)
    { guards++; return; }
    throw new InvalidDataException("Unsupported auxiliary delimiter lifecycle accepted.");
}
Reject(() => new LegacyNativeAuxiliaryDelimiterQuerySource(model.AcousticAlphabet, actualContexts,
    (_, _, _) => throw new Exception(), 5, true));
var failedContexts = new LegacySymbolContextFactory();
var failed = new LegacyNativeAuxiliaryDelimiterQuerySource(model.AcousticAlphabet, failedContexts,
    (_, request, _) => new(-1, ReadOnlyMemory<byte>.Empty, ReadOnlyMemory<byte>.Empty, 0, request.Skip, null), 5);
var failedQuery = new LegacyPreparedDictionaryQuery { ContextRoot = failedContexts.ContainerIdentity };
Require(failed.Read(0, failedQuery).ReturnedCount == -1, "Auxiliary backend failure lost native negative return.");
Reject(() => failed.Read(0, failedQuery));
using (var canceled = new CancellationTokenSource())
{
    var context = new LegacySymbolContextFactory();
    var source = new LegacyNativeAuxiliaryDelimiterQuerySource(model.AcousticAlphabet, context,
        (_, request, _) => { canceled.Cancel(); return new(0, ReadOnlyMemory<byte>.Empty, ReadOnlyMemory<byte>.Empty, 0, request.Skip, null); }, 5);
    var query = new LegacyPreparedDictionaryQuery { ContextRoot = context.ContainerIdentity };
    Reject(() => source.Read(0, query, canceled.Token));
    Reject(() => source.Read(0, query));
}
Console.WriteLine($"PASS {fixtures} original auxiliary290 trees / {rows} full32 rows+source contexts; {paginationRows} independently ordered flat paginated and {recursiveRows} recursive resume rows; exact parameter-refresh backend replay; actual{actualExpected.Count} terminal records/{actualCalls} backend calls and {guards} lifecycle guards.");

static ushort[] Path(LegacySymbolContextFactory contexts, ulong identity)
{
    var result = new List<ushort>();
    while (identity != 0)
    {
        Require(result.Count < 4096, "Context cycle outside native bounded input.");
        var node = contexts.ReadNode(identity); result.Add(node.Symbol); identity = node.ParentLink;
    }
    result.Reverse(); return result.ToArray();
}
static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
