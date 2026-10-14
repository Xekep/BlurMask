using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if (args.Length != 3) throw new ArgumentException("controls.json AK LK required");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int streams = 0, pageCount = 0, requestCount = 0;
foreach (var test in controls.RootElement.EnumerateArray())
{
    var contexts = new LegacySymbolContextFactory();
    int readIndex = 0, consumerIndex = 0, capacity = test.GetProperty("capacity").GetInt32();
    var expectedReads = test.GetProperty("reads").EnumerateArray().ToArray();
    var supplied = test.GetProperty("supplied").EnumerateArray().Select(x => Bytes(x.GetString()!)).ToArray();
    var expectedPages = test.GetProperty("pages").EnumerateArray().ToArray();
    var child = new LegacyAggregateDictionaryChild(Read, _ => true);
    var aggregate = new LegacyPreparedAggregateDictionaryQuerySource([child]);
    var session = new LegacyNativeAuxiliaryWordQueryPages(aggregate, contexts, capacity,
        Bytes(test.GetProperty("residue").GetString()!));
    var result = session.Run(Bytes(test.GetProperty("row").GetString()!), test.GetProperty("owner").GetInt32(),
        new((byte)test.GetProperty("mode").GetInt32(), (byte)test.GetProperty("limit").GetInt32(),
            test.GetProperty("boundary").GetBoolean(), false, 0),
        test.GetProperty("flags").GetUInt16(), test.GetProperty("frontend").GetBoolean(),
        test.GetProperty("sourceMode").GetBoolean(), Consume, 8);
    Require(result.Completed == test.GetProperty("completed").GetBoolean() &&
        result.FinalAcceptedSkip == test.GetProperty("finalSkip").GetInt32() &&
        result.SourceReadCount == test.GetProperty("readCount").GetInt32() &&
        result.ConsumedPages.Count == expectedPages.Length && consumerIndex == expectedPages.Length &&
        readIndex == expectedReads.Length, "Original stream lifecycle differs.");
    streams++; pageCount += consumerIndex; requestCount += readIndex;

    LegacyPreparedDictionaryQueryPage Read(LegacyPreparedDictionaryQuery query, bool auxiliary,
        bool extraCosts, CancellationToken token)
    {
        Require(auxiliary && !extraCosts && query.ContextRoot == contexts.ContainerIdentity,
            "Wrong virtual48/shared-container/cost-buffer binding.");
        long[] actual = [query.Node, query.Skip, query.Capacity, query.Category18, query.Parameter19,
            query.Parameter1c, query.Parameter20, query.Flags24, query.Flags26, (long)query.ParentContext];
        long[] expected = expectedReads[readIndex].EnumerateArray().Select(x => x.GetInt64()).ToArray();
        Require(actual.AsSpan().SequenceEqual(expected), "Original auxiliary request fields/skip differ.");
        byte[] records = supplied[readIndex++];
        return new(records.Length / 32, records, 0, 0, 0, ReadOnlyMemory<byte>.Empty);
    }
    bool Consume(LegacyNativeAuxiliaryWordQueryPage page, CancellationToken token)
    {
        var expected = expectedPages[consumerIndex++];
        Require(page.SourceReturnedCount == expected.GetProperty("returned").GetInt32() &&
            page.AcceptedSkip == expected.GetProperty("skip").GetInt32() &&
            page.InjectedCurrentTerminal == expected.GetProperty("injected").GetBoolean() &&
            page.Records32.Span.SequenceEqual(Bytes(expected.GetProperty("records").GetString()!)) &&
            page.BufferSnapshot.Span.SequenceEqual(Bytes(expected.GetProperty("buffer").GetString()!)),
            "Original full32/full-buffer page or deferred one-time injection differs.");
        int abortAt = test.GetProperty("abortAt").GetInt32();
        return abortAt == 0 || consumerIndex != abortAt;
    }
}
Console.WriteLine($"PASS{streams} original auxiliary streams/{requestCount} exact requests/{pageCount} full-buffer pages.");
RunGuards();
RunActual(File.ReadAllBytes(args[1]), File.ReadAllBytes(args[2]));

static void RunGuards()
{
    int controls = 0;
    byte[] row = new byte[88]; row[4] = 1; row[5] = 2;
    byte[] residue = Enumerable.Range(0, 64).Select(x => (byte)x).ToArray();
    var contexts = new LegacySymbolContextFactory();
    LegacyPreparedAggregateDictionaryQuerySource Aggregate(LegacyAggregateDictionaryReader read)
        => new([new(read, _ => true)]);
    LegacyPreparedDictionaryQueryPage Empty(LegacyPreparedDictionaryQuery q, bool a, bool e, CancellationToken c)
        => new(0, ReadOnlyMemory<byte>.Empty, 0, 0, 0, ReadOnlyMemory<byte>.Empty);
    Reject(() => new LegacyNativeAuxiliaryWordQueryPages(Aggregate(Empty), contexts, 0, [])); controls++;
    Reject(() => new LegacyNativeAuxiliaryWordQueryPages(Aggregate(Empty), contexts, 2, new byte[63])); controls++;
    foreach (int malformed in new[] { 0, 87, 89 })
    {
        var session = new LegacyNativeAuxiliaryWordQueryPages(Aggregate(Empty), contexts, 2, residue);
        Reject(() => session.Run(new byte[malformed], 0, default, 0, false, false, (_, _) => true, 4)); controls++;
    }
    foreach (int iterations in new[] { 0, 2_000_002 })
    {
        var session = new LegacyNativeAuxiliaryWordQueryPages(Aggregate(Empty), contexts, 2, residue);
        Reject(() => session.Run(row, 0, default, 0, false, false, (_, _) => true, iterations)); controls++;
    }
    var foreignFlags = new LegacyNativeAuxiliaryWordQueryPages(Aggregate(Empty), contexts, 2, residue);
    Reject(() => foreignFlags.Run(row, 0, default, 1, false, false, (_, _) => true, 4)); controls++;
    var aborted = new LegacyNativeAuxiliaryWordQueryPages(Aggregate(Empty), contexts, 2, residue);
    Require(!aborted.Run(row, 0, default, 0, false, false, (_, _) => false, 4).Completed,
        "Consumer rejection did not abort its owned query session."); controls++;
    Reject(() => aborted.Run(row, 0, default, 0, false, false, (_, _) => true, 4)); controls++;
    var reentrant = new LegacyNativeAuxiliaryWordQueryPages(Aggregate(Empty), contexts, 2, residue);
    reentrant.Run(row, 0, default, 0, false, false, (_, _) =>
    { Reject(() => reentrant.Run(row, 0, default, 0, false, false, (_, _) => true, 4)); return true; }, 4);
    controls++;
    foreach (bool cancelInReader in new[] { false, true })
    {
        using var cancellation = new CancellationTokenSource();
        LegacyPreparedDictionaryQueryPage Reader(LegacyPreparedDictionaryQuery q, bool a, bool e, CancellationToken t)
        { if (cancelInReader) cancellation.Cancel(); return Empty(q, a, e, t); }
        var session = new LegacyNativeAuxiliaryWordQueryPages(Aggregate(Reader), contexts, 2, residue);
        Reject(() => session.Run(row, 0, default, 0, false, false,
            (_, _) => { cancellation.Cancel(); return false; }, 4, cancellation.Token)); controls++;
        Reject(() => session.Run(row, 0, default, 0, false, false, (_, _) => true, 4)); controls++;
    }
    var negative = new LegacyNativeAuxiliaryWordQueryPages(Aggregate((q, a, e, c) =>
        new(-1, ReadOnlyMemory<byte>.Empty, 0, 0, 0, ReadOnlyMemory<byte>.Empty)), contexts, 2, residue);
    Reject(() => negative.Run(row, 0, default, 0, false, false, (_, _) => true, 4)); controls++;
    var endless = new LegacyNativeAuxiliaryWordQueryPages(Aggregate((q, a, e, c) =>
        new(2, new byte[64], 0, 0, 0, ReadOnlyMemory<byte>.Empty)), contexts, 2, residue);
    Reject(() => endless.Run(row, 0, default, 0, false, false, (_, _) => true, 1)); controls++;
    byte[] originalRow = row.ToArray();
    var mutating = new LegacyNativeAuxiliaryWordQueryPages(Aggregate((q, a, e, c) =>
    { Array.Fill(row, (byte)255); Array.Fill(residue, (byte)255); return Empty(q, a, e, c); }), contexts, 2, residue);
    var snapshot = mutating.Run(row, 0, default, 0, false, false, (_, _) => true, 4);
    Require(snapshot.ConsumedPages[0].Records32.Span[..8].SequenceEqual(originalRow.AsSpan(0, 8)) &&
        snapshot.ConsumedPages[0].Records32.Span[24..].SequenceEqual(Enumerable.Range(24, 8).Select(x => (byte)x).ToArray()),
        "Owner row/allocation input snapshot changed inside source callback."); controls++;
    Console.WriteLine($"PASS{controls} auxiliary extents/cancellation/negative-count/iteration/lifecycle/snapshot guards.");
}

static void RunActual(byte[] ak, byte[] lk)
{
    var acoustic = LegacyInkAlphabet.Load(ak.AsSpan(99), 0x103);
    var lexical = LegacyInkAlphabet.Load(lk.AsSpan(607074), 0x103);
    var contexts = new LegacySymbolContextFactory();
    var maps = LegacyDictionaryInitialSymbolMaps.Prepare(acoustic, lexical, contexts, []);
    var graph = LegacyPackedLexicon.Load(lk.AsSpan(609980, 2513123 - 609980));
    ushort[] auxiliary = maps.CopyAuxiliary(), primary = maps.CopyPrimary();
    int candidateEdges = 0, terminalEdges = 0;
    for (int node = 1; node < graph.NodeCount; node++)
    {
        int lexicalLabel = graph.GetLabel(node);
        if (auxiliary[lexicalLabel] == ushort.MaxValue) continue;
        candidateEdges++;
        if (graph.IsTerminal(node)) terminalEdges++;
    }
    Require(candidateEdges == 17 && terminalEdges == 0,
        "Actual auxiliary-map/terminal packed-graph facts changed.");
    var dictionarySource = maps.CreateQuerySource(graph, 0xc0a);
    var child = LegacyAggregateDictionaryChild.BindDictionary(dictionarySource, contexts);
    var aggregate = new LegacyPreparedAggregateDictionaryQuerySource([child]);
    int tested = 0;
    for (int node = 1; node < graph.NodeCount && tested < 32; node++)
    {
        if (!graph.IsTerminal(node) || primary[graph.GetLabel(node)] >= acoustic.LabelCount) continue;
        byte[] row = new byte[88];
        BinaryPrimitives.WriteUInt64LittleEndian(row, (uint)node | 1UL << 32 | 2UL << 40);
        BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(36), primary[graph.GetLabel(node)]);
        byte[] buffer = new byte[128 * 32];
        buffer.AsSpan(24, 8).Fill(0x35);
        var session = new LegacyNativeAuxiliaryWordQueryPages(aggregate, contexts, 128, buffer);
        var result = session.Run(row, tested, default, 0, false, false, (_, _) => true, 4);
        Require(result.SourceReadCount == 2 && result.FinalAcceptedSkip == 0 && result.ConsumedPages.Count == 1 &&
            result.ConsumedPages[0].SourceReturnedCount == 0 && result.ConsumedPages[0].InjectedCurrentTerminal &&
            result.ConsumedPages[0].Records32.Span[..8].SequenceEqual(row.AsSpan(0, 8)) &&
            result.ConsumedPages[0].Records32.Span[24..].ToArray().All(x => x == 0x35),
            "Actual092990→093B50 auxiliary zero-page→204290 terminal injection differs.");
        tested++;
    }
    Require(tested == 32, "Actual test requires32 owned dictionary terminals.");
    Console.WriteLine($"ACTUAL {graph.NodeCount} packed nodes/{candidateEdges} auxiliary edges/{terminalEdges} auxiliary terminals ->32 owned terminal pages PASS; full scheduler not claimed.");
}

static byte[] Bytes(string hex) => Convert.FromHexString(hex);
static void Require(bool condition, string message) { if (!condition) throw new InvalidDataException(message); }
static void Reject(Action action)
{
    try { action(); }
    catch (Exception e) when (e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException or InvalidDataException) { return; }
    throw new InvalidDataException("Expected bounded auxiliary query rejection.");
}
