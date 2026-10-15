using System.Buffers.Binary;
using InkRecognition;

const int Impossible = 0x4000000;
if (args.Length != 4) throw new ArgumentException("AK LK engine.json separation.json required");
byte[] ak = File.ReadAllBytes(args[0]), lk = File.ReadAllBytes(args[1]);
byte[] engine = File.ReadAllBytes(args[2]), separation = File.ReadAllBytes(args[3]);
var fixture = MainFixture.Create(ak, lk, engine, separation);
var owner = new LegacyOwnedMainWordSpanPreparation(ak, lk, engine, fixture.Sampled, fixture.Table);
var policy = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, owner);
var configuration = policy.Configuration;
var graph = owner.Source.SourceRanges.Graph;
var alphabet = owner.DictionaryOwner.Model.AcousticAlphabet;
var categories = fixture.Sampled.Owner.TransitionProfile.Categories;
uint[] headers = owner.DictionaryOwner.CopySourceHeaders();
var headerMap = headers.Select((header, index) => (header, index))
    .ToDictionary(item => checked((byte)item.index), item => item.header);
var preparation = LegacyNativeWordHeapPreparation.PrepareFresh(
    [new(0, 0, 1), LegacyNativeWordPreparedSource.FromDictionary(owner.DictionaryOwner.Descriptor)], configuration, 0);
int capacity = checked((int)preparation.Limits[0].PrimaryInitialCapacity20);
Require(capacity == 250 && configuration.PrimaryArcLimit230a == 125, "Actual prepared heap/primary quotas changed.");

LegacyNativeWordHeapRouter Router(LegacyNativeWordRecordHeap heap)
    => new([heap], preparation.CopySourceBuckets().Span, -1, uint.MaxValue);
LegacyOwnedFreshMainWordNodeBuffer Buffer(int node, byte rows = 0x4a, byte alternatives = 0x71)
{
    int extent = configuration.PrimaryArcLimit230a + (int)(configuration.ExpansionFactor230c * configuration.PrimaryArcLimit230a);
    return new(policy, node, Enumerable.Repeat(rows, extent * 88).ToArray(),
        Enumerable.Repeat(alternatives, configuration.PrimaryArcLimit230a * 80).ToArray());
}

//Independent byte-level2144F0 initialization, including untouched allocation
//residue and the second40-byte alternative. Logical rows are not native pointers.
int initializationControls = 0;
for (int seed = 0; seed < 64; seed++)
{
    byte rowFill = (byte)(seed * 31), alternativeFill = (byte)(seed * 17);
    var buffer = Buffer(0, rowFill, alternativeFill);
    byte[] expectedRows = Enumerable.Repeat(rowFill, buffer.Capacity * 88).ToArray();
    byte[] expectedAlternatives = Enumerable.Repeat(alternativeFill, 125 * 80).ToArray();
    for (int i = 0; i < 125; i++)
    {
        W32(expectedRows, i * 88 + 16, Impossible);
        W32(expectedRows, i * 88 + 20, Impossible);
        W32(expectedAlternatives, i * 80, Impossible);
        W32(expectedAlternatives, i * 80 + 4, Impossible);
    }
    Require(buffer.CopyRecordStorage().Span.SequenceEqual(expectedRows) &&
        buffer.CopyPrimaryStorage().Span.SequenceEqual(expectedAlternatives) &&
        buffer.SourceEnd23f0 == 125 && buffer.RetainedEnd23f8 == 125, "Full allocation initialization differs.");
    buffer.BeginLanguage();
    for (int i = 0; i < 125; i++) W16(expectedRows, i * 88 + 62, ushort.MaxValue);
    Require(buffer.CopyRecordStorage().Span.SequenceEqual(expectedRows), "BeginLanguage touched other allocation bytes.");
    initializationControls += 2;
}

//Real source-owned whole-prefix startup and204290 completion at every owning
//node. All returned query records are consumed, including punctuation.
var buffers = new Dictionary<int, LegacyOwnedFreshMainWordNodeBuffer>();
byte[]? firstLanguageSlot = null;
var initial = new LegacyNativeMainInitialSpecialCandidates(ak, owner, new(1, 0), new byte[80]);
int rootQueries = 0, terminalQueries = 0, terminalAllocations = 0;
for (int node = 0; node < graph.NodeCount; node++)
{
    if (owner.GetSpanExtent(node) <= node) continue;
    var span = owner.Prepare(node, checked((byte)node));
    var request = new LegacyPreparedDictionaryQuery
    {
        Capacity = 128, Parameter1c = (uint)node, Parameter20 = (uint)node,
        ContextRoot = owner.Contexts.ContainerIdentity
    };
    var heap = new LegacyNativeWordRecordHeap(capacity);
    while (true)
    {
        var page = owner.Read(span, 8UL << 40, request, clearExtraCosts: true);
        Require(page.ReturnedCount >= 0, "Actual initial query failed.");
        if (page.ReturnedCount == 0) break;
        rootQueries += page.ReturnedCount;
        initial.ProcessPreparedPage(span, page.Records.Span, 0, Router(heap), 0,
            policy.Refresh.ConsecutiveBlank22f7 != 0, false);
        request.Skip += page.ReturnedCount;
    }
    var ordinals = LegacyNativeWordAlternativeOrdinals.AssignInitial(heap.GetSlotStorageSnapshot().Span, heap.Count);
    var pools = LegacyNativeRootWordAlternativeProducer.Create(ordinals.SlotStorage.Span, heap.Count,
        (uint)node, configuration, owner.Contexts, symbol => symbol, new byte[heap.Capacity * 80], []);
    var ordered = LegacyNativeWordAlternativeOrdering.Prepare(ordinals.SlotStorage.Span, heap.Count, false,
        configuration, headers, false, new(2, pools.PrimaryStorage.Span), null);
    var buffer = Buffer(node);
    for (int ordinal = 0; ordinal < ordered.Heap.Count; ordinal++)
        buffer.WritePrimary(ordered.Heap.GetSlotSnapshot(ordinal).Span, ordered.PrimaryAlternativeStorage.Span);
    buffer.BeginLanguage();
    var rows = buffer.CopyRows();
    Require(rows.Count == 125 && ordered.Heap.Count < rows.Count, "Native IMP sentinels were collapsed into active row count.");
    var languageHeap = new LegacyNativeWordRecordHeap(capacity);
    var completion = new LegacyNativeFirstDictionaryWordCompletion(owner.DictionaryOwner.Model,
        owner.DictionaryOwner.WordMetadata, owner.DictionaryOwner.LanguageRecords, owner.DictionaryOwner.RestartTags,
        1, headers[1], false, 0, false, false);
    var auxiliary = new LegacyNativeAuxiliaryWordQueryPages(owner.DictionaryOwner.DictionaryQueries,
        owner.Contexts, 128, new byte[4096]);
    for (ushort ordinal = 0; ordinal < rows.Count; ordinal++)
    {
        ushort parentOrdinal = ordinal;
        var parent = rows[ordinal].Record;
        if (I32(parent.Span, 16) >= Impossible) break;
        var result = auxiliary.Run(parent.Span, node, new(0, 0, false, false, 120), 0, false, false,
            (page, token) =>
            {
                for (int offset = 0; offset < page.Records32.Length; offset += 32)
                {
                    terminalQueries++;
                    var completed = completion.ProcessCandidate(page.Records32.Span.Slice(offset, 32), node,
                        parentOrdinal, parent.Span, Router(languageHeap), false, new byte[40],
                        (owningNode, index) =>
                        {
                            Require(owningNode == node, "Foreign completion row owner.");
                            return rows[index].Record;
                        }, false, token);
                    if (completed.Allocation.Accepted) terminalAllocations++;
                }
                return true;
            }, 100);
        Require(result.Completed, "Unfiltered actual terminal page was not completely consumed.");
    }
    foreach (int ordinal in Enumerable.Range(0, languageHeap.Count))
    {
        firstLanguageSlot ??= languageHeap.GetSlotSnapshot(ordinal).ToArray();
        buffer.WriteLanguage(languageHeap.GetSlotSnapshot(ordinal).Span);
    }
    buffer.CompactLanguage(new(headers));
    Require(buffer.SourceEnd23f0 == 125 + languageHeap.Count && buffer.RetainedEnd23f8 <= buffer.SourceEnd23f0,
        "Source and retained counts lost their separate native meanings.");
    buffers.Add(node, buffer);
}

//Two identical real lexical rows retain one ordinal after212B10.23F0 still
//includes both original source rows;23F8 is the retained126, not127 or1.
var duplicates = Buffer(0, 0, 0);
duplicates.BeginLanguage();
duplicates.WriteLanguage(firstLanguageSlot!);
duplicates.WriteLanguage(firstLanguageSlot!);
duplicates.CompactLanguage(new(headers));
Require(duplicates.SourceEnd23f0 == 127 && duplicates.RetainedEnd23f8 == 126 &&
    duplicates.CopyRows().Count == 127 && duplicates.CopyRows(retained: true).Count == 126,
    "Deduplication overwrote source extent or discarded native125 primary allocation.");

//Primary alternative cleanup writes only cost00 for an unreset secondary
//parent. The second record's bit5 preserves its complete40-byte payload.
var cleanup = Buffer(0, 0, 0);
byte[] slot128 = new byte[128], pool80 = Enumerable.Repeat((byte)0x35, 80).ToArray();
W16(slot128, 60, ushort.MaxValue);
W32(pool80, 0, 1); W32(pool80, 40, 2);
W16(pool80, 24, 125); W16(pool80, 64, 125);
pool80[31] &= 0xdf; pool80[71] |= 32;
cleanup.WritePrimary(slot128, pool80);
cleanup.BeginLanguage();
cleanup.CompactLanguage(new(headers));
var expectedPool = pool80.ToArray(); W32(expectedPool, 0, Impossible);
Require(cleanup.CopyPrimaryStorage().Span[..80].SequenceEqual(expectedPool), "Alternative cleanup changed reset/other payload bytes.");

int lifecycleGuards = 0;
void Reject(Action action)
{
    try { action(); }
    catch (Exception error) when (error is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException)
    { lifecycleGuards++; return; }
    throw new InvalidDataException("Unsupported node/consumer scope was accepted.");
}
Reject(() => Buffer(0).WriteLanguage(firstLanguageSlot!));
Reject(() => cleanup.WriteLanguage(firstLanguageSlot!));
Reject(() => cleanup.CompactLanguage(new(headers)));
Reject(() => cleanup.BeginLanguage());
Reject(() => cleanup.WritePrimary(slot128, pool80));
Reject(() => cleanup.Read(125));
using (var canceled = new CancellationTokenSource())
{
    canceled.Cancel();
    Reject(() => new LegacyOwnedFreshMainWordNodeBuffer(policy, 0, [], [], canceled.Token));
}
Reject(() => new LegacyOwnedFreshMainWordNodeBuffer(policy, 0, new byte[1], new byte[1]));
Reject(() => new LegacyOwnedFreshMainWordNodeBuffer(policy, graph.NodeCount, [], []));

var currentSpan = owner.Prepare(1, 0);
var previousRows = buffers[0].CopyRows(retained: true).Select(row => row.Record).ToArray();
var groups = LegacyNativeAlternativeWordQueryPreparation.GroupSecondaryRows(previousRows, 125, 0, false, 0);
var geometry = fixture.Sampled.CreatePrepared240TransitionGeometry(fixture.Table);
var thresholds = LegacyAcousticCandidateThresholds.ReadV110Node(ak.AsSpan(189734), out _);
var acoustic = LegacyNativeWordAcousticRequestScorer.ForAlphabet(thresholds, alphabet);
var oracle = new OrdinaryGeometryOracle(fixture.Sampled, fixture.Table);
int actualQueries = 0, fullRecords = 0, parentFullRecords = 0, geometryControls = 0, quotes = 0;
byte[]? quoteQuery = null;
foreach (var group in groups)
{
    var request = new LegacyPreparedDictionaryQuery { Capacity = 128, ContextRoot = owner.Contexts.ContainerIdentity };
    ulong tag = LegacyNativeAlternativeWordQueryPreparation.Apply(request, new(0, 0, false, false, 120), group, 1, 0, 0)!.Value;
    while (true)
    {
        var page = owner.Read(currentSpan, tag, request, clearExtraCosts: true);
        Require(page.ReturnedCount >= 0, "Actual alternative query failed.");
        if (page.ReturnedCount == 0) break;
        for (int offset = 0; offset < page.Records.Length; offset += 32)
        {
            var query = page.Records.Slice(offset, 32);
            ushort symbol = U16(query.Span, 20);
            bool quote = alphabet.TryGetText(symbol, out string text) && text is "'" or "\"";
            if (quote) { quotes++; quoteQuery ??= query.ToArray(); }
            actualQueries++;
            foreach (ushort inputFlags in new ushort[] { 0, 16 })
            {
                var heap = new LegacyNativeWordRecordHeap(capacity);
                var consumer = new LegacyNativeOrdinaryAlternativeWordCandidates(graph, alphabet, categories,
                    owner.Contexts, geometry, acoustic, new(1, 0), new byte[80]);
                var actual = consumer.ProcessQuery(query.Span, group, previousRows, 1, 0, 0, inputFlags,
                    currentSpan.Qualifier, headerMap, (_, arc) => owner.GetCosts(currentSpan, 1, arc),
                    (node, ordinal) => buffers[node].Read(ordinal).Record, Router(heap),
                    0, false, false, false, false, 0, false, false, null);
                var expected = ExpectedAlternative(query.Span, group, inputFlags);
                Require(actual.Succeeded && actual.AcceptedAllocations == expected.Count && heap.Count == expected.Count,
                    "Actual unfiltered alternative allocation count differs.");
                var actualBytes = Enumerable.Range(0, heap.Count).Select(i => Convert.ToHexString(heap.GetSlotSnapshot(i).Span)).Order().ToArray();
                Require(actualBytes.SequenceEqual(expected.Select(Convert.ToHexString).Order()), "Independent alternative full128 record differs.");
                fullRecords += expected.Count;
                //The same actually returned quote is also an explicit201270
                //consumer control over its owning real LM parent. This does
                //not claim that2144F0 schedules secondary rows through201270.
                if (quote)
                {
                    ushort parentOrdinal = group.FirstRowOrdinal;
                    var parent = previousRows[parentOrdinal];
                    var parentHeap = new LegacyNativeWordRecordHeap(capacity);
                    var parentConsumer = new LegacyNativeOrdinaryParentWordCandidates(graph, alphabet, categories,
                        owner.Contexts, geometry, acoustic, new(1, 0), new byte[80]);
                    var parentActual = parentConsumer.ProcessPage(query.Span, 1, 0, 0, parentOrdinal, parent.Span,
                        inputFlags, currentSpan.Qualifier, headerMap, (_, arc) => owner.GetCosts(currentSpan, 1, arc),
                        (node, ordinal) => buffers[node].Read(ordinal).Record, Router(parentHeap), 0, false,
                        false, 0, false, false, null);
                    var parentExpected = ExpectedParent(query.Span, parentOrdinal, inputFlags);
                    Require(parentActual.Succeeded && parentActual.AcceptedAllocations == parentExpected.Count,
                        "Actual quote parent allocation count differs.");
                    Require(Enumerable.Range(0, parentHeap.Count).Select(i => Convert.ToHexString(parentHeap.GetSlotSnapshot(i).Span)).Order()
                        .SequenceEqual(parentExpected.Select(Convert.ToHexString).Order()), "Independent parent quote full128 differs.");
                    parentFullRecords += parentExpected.Count;
                }
            }
        }
        request.Skip += page.ReturnedCount;
    }
}
Require(quotes > 0 && actualQueries > quotes && fullRecords > 0, "Actual source failed to exercise quote and other query branches.");
Require(parentFullRecords > 0, "Actual quote parent consumer emitted no records.");
var guardGroup = groups[0];
var guardParent = previousRows[guardGroup.FirstRowOrdinal];
foreach (string unsupported in new[] { "\ufffd", "\u200c", "\u0651", "\U000f0001", "\U000f0002" })
{
    if (!alphabet.TryGetLabel(unsupported, out ushort label)) continue;
    byte[] query = quoteQuery!.ToArray();
    W16(query, 20, label); W64(query, 8, owner.Contexts.ResolveInitial(label).Identity);
    Reject(() => new LegacyNativeOrdinaryAlternativeWordCandidates(graph, alphabet, categories, owner.Contexts,
        geometry, acoustic, new(1, 0), new byte[80]).ProcessQuery(query, guardGroup, previousRows, 1, 0, 0, 0,
        currentSpan.Qualifier, headerMap, (_, arc) => owner.GetCosts(currentSpan, 1, arc),
        (node, ordinal) => buffers[node].Read(ordinal).Record, Router(new(capacity)), 0, false, false,
        false, false, 0, false, false, null));
    LegacyNativeOrdinaryWordStartupResult ParentGuard() => new LegacyNativeOrdinaryParentWordCandidates(graph, alphabet, categories, owner.Contexts,
        geometry, acoustic, new(1, 0), new byte[80]).ProcessPage(query, 1, 0, 0, guardGroup.FirstRowOrdinal,
        guardParent.Span, 0, currentSpan.Qualifier, headerMap, (_, arc) => owner.GetCosts(currentSpan, 1, arc),
        (node, ordinal) => buffers[node].Read(ordinal).Record, Router(new(capacity)), 0, false,
        false, 0, false, false, null);
    if (unsupported == "\U000f0001" && I32(guardParent.Span, 48) == -1)
        Require(ParentGuard().AcceptedAllocations == 0, "Native loop/absent auxiliary skip was lost.");
    else Reject(() => ParentGuard());
    var packet = LegacyNativeWordParentRequestFactory.Create(1, 0, 0, guardParent.Span,
        0, label, 0, U64(query, 0), 0, 0, headers[1], headers[1], 0,
        categories, new(1, 0), 0, new byte[80]);
    Reject(() => geometry.Validate140(packet));
}
if (alphabet.TryGetLabel("\u200c", out ushort joiner))
{
    var protocol = owner.Contexts.ResolveInitial(0xfffb);
    var previous = owner.Contexts.ResolveChild(protocol.Identity, joiner);
    owner.Contexts.LinkCreatedContext(previous.Identity, protocol.Identity);
    var apostrophe = owner.Contexts.ResolveChild(previous.Identity, U16(quoteQuery!, 20));
    owner.Contexts.LinkCreatedContext(apostrophe.Identity, previous.Identity);
    byte[] query = quoteQuery!.ToArray(); W64(query, 8, apostrophe.Identity);
    Reject(() => new LegacyNativeOrdinaryAlternativeWordCandidates(graph, alphabet, categories, owner.Contexts,
        geometry, acoustic, new(1, 0), new byte[80]).ProcessQuery(query, guardGroup, previousRows, 1, 0, 0, 0,
        currentSpan.Qualifier, headerMap, (_, arc) => owner.GetCosts(currentSpan, 1, arc),
        (node, ordinal) => buffers[node].Read(ordinal).Record, Router(new(capacity)), 0, false, false,
        false, false, 0, false, false, null));
}
Console.WriteLine($"PASS {initializationControls} full-allocation controls; source127/retained126, alternative cleanup and {lifecycleGuards} lifecycle guards; {rootQueries} root queries / {terminalQueries} terminal queries -> {terminalAllocations} completions; {actualQueries} unfiltered alternative queries ({quotes} quotes), {fullRecords} alternative + {parentFullRecords} parent independent full128 records / {geometryControls} geometry controls.");

List<byte[]> ExpectedParent(ReadOnlySpan<byte> query, ushort parentOrdinal, ushort inputFlags)
{
    var expected = new List<byte[]>();
    var parent = previousRows[parentOrdinal].Span;
    ushort symbol = U16(query, 20);
    if ((query[23] & 8) != 0 && I32(parent, 48) != -1 ||
        !currentSpan.Qualifier.Qualifies(symbol, query[22], U64(query, 8), owner.Contexts)) return expected;
    int variants = (inputFlags & 16) == 0 || alphabet.TryGetText(symbol, out string text) && text is "'" or "\"" ? 2 : 1;
    for (int variant = 0; variant < variants; variant++)
    {
        int category = (variant == 0 ? categories.StartCompositeMap : categories.EndCompositeMap).Span[symbol];
        if (category < 0) break;
        foreach (int arcIndex in Enumerable.Range(0, graph.GetNode(1).Count))
        {
            var arc = graph.GetNode(1)[arcIndex];
            if (arc.Span != 0 || !arc.PredecessorArcIndices.Span.Contains((int)U16(parent, 28))) continue;
            ushort ordinal = checked((ushort)arcIndex);
            int correction = oracle.Correction(1, ordinal, U16(parent, 24),
                checked((ushort)(BinaryPrimitives.ReadUInt32LittleEndian(parent[24..]) >> 22)), query[23] & 7, 0, 0);
            geometryControls++;
            int before = unchecked(I32(parent, 16) + I32(query, 16) + correction);
            var costs = owner.GetCosts(currentSpan, 1, ordinal);
            int classCost = costs.ClassCosts.Span[symbol];
            int geometryCost = costs.GeometryCosts.Span[categories.CompositeCategories.Span[category].GeometryIndex];
            int result = unchecked(before + classCost + geometryCost);
            if (before >= Impossible || classCost == Impossible || result >= Impossible) continue;
            byte[] record = new byte[128];
            W64(record, 0, U64(query, 0));
            bool flagged = (query[23] & 7) == 0 && (parent[5] & 128) != 0 || alphabet.IsSymbolFlagged(symbol);
            record[5] = (byte)((query[5] & 0x7d) | (flagged ? 128 : 0) | ((U64(query, 0) & 0x20000000000UL) != 0 ? 2 : 0));
            record[6] = (byte)(query[6] & 0xfc);
            W64(record, 8, U64(query, 8)); W32(record, 16, result);
            W32(record, 20, unchecked(I32(parent, 20) + correction + classCost + geometryCost));
            W32(record, 24, unchecked((int)(1U | (uint)category << 16 | (uint)ordinal << 22)));
            W16(record, 28, ordinal); W16(record, 30, parentOrdinal);
            record[33] = (byte)(((query[23] >> 5 & 3) + query[22]) & 63);
            W16(record, 34, ushort.MaxValue); W16(record, 36, symbol); record[39] = (byte)((query[23] & 7) << 1);
            W64(record, 40, ulong.MaxValue);
            int firstId = unchecked((int)arc.StartContextId);
            W32(record, 48, (query[23] & 8) != 0 ? firstId : I32(parent, 48));
            W32(record, 52, (query[23] & 7) == 2 ? firstId : I32(parent, 52));
            W32(record, 56, I32(parent, 56)); W16(record, 60, U16(parent, 60)); W16(record, 62, ushort.MaxValue);
            W64(record, 88, ulong.MaxValue);
            var anchor = U16(parent, 60) == ushort.MaxValue ? record.AsSpan(0, 88) : buffers[I32(parent, 56)].Read(U16(parent, 60)).Record.Span;
            int previousNode = I32(parent, 56) - (sbyte)anchor[32] - 1;
            W32(record, 104, previousNode);
            W64(record, 112, previousNode < 0 ? 0 : U64(buffers[previousNode].Read(U16(anchor, 30)).Record.Span, 40));
            expected.Add(record);
        }
    }
    return expected;
}

List<byte[]> ExpectedAlternative(ReadOnlySpan<byte> query, LegacyNativeAlternativeWordQueryGroup group, ushort inputFlags)
{
    var expected = new List<byte[]>();
    ushort symbol = U16(query, 20);
    if (!currentSpan.Qualifier.Qualifies(symbol, query[22], U64(query, 8), owner.Contexts)) return expected;
    int variants = (inputFlags & 16) == 0 || alphabet.TryGetText(symbol, out string value) && value is "'" or "\"" ? 2 : 1;
    for (int variant = 0; variant < variants; variant++)
    {
        int category = (variant == 0 ? categories.StartCompositeMap : categories.EndCompositeMap).Span[symbol];
        if (category < 0) break;
        var best = new Dictionary<ushort, (int Cost, int Increment, ushort Parent)>();
        foreach (ushort parentOrdinal in group.EligibleRowOrdinals.Span)
        {
            var parent = previousRows[parentOrdinal].Span;
            ushort previousArc = U16(parent, 28);
            foreach (int ordinal in graph.GetNode(1).Select((arc, index) => (arc, index))
                .Where(item => item.arc.Span == 0 && item.arc.PredecessorArcIndices.Span.Contains((int)previousArc))
                .Select(item => item.index))
            {
                ushort arc = checked((ushort)ordinal);
                int delta = oracle.Correction(1, arc, U16(parent, 24), checked((ushort)(BinaryPrimitives.ReadUInt32LittleEndian(parent[24..]) >> 22)),
                    query[23] & 7, categories.CompositeCategories.Span[category].FirstId,
                    categories.CompositeCategories.Span[parent[26] & 63].FirstId);
                geometryControls++;
                int cost = unchecked(I32(parent, 16) + delta);
                if (cost < Impossible && (!best.TryGetValue(arc, out var old) || cost < old.Cost))
                    best[arc] = (cost, unchecked(I32(parent, 20) + delta), parentOrdinal);
            }
        }
        foreach (var item in best)
        {
            var costs = owner.GetCosts(currentSpan, 1, item.Key);
            int classCost = costs.ClassCosts.Span[symbol];
            int geometric = costs.GeometryCosts.Span[categories.CompositeCategories.Span[category].GeometryIndex];
            int result = unchecked(item.Value.Cost + I32(query, 16) + classCost + geometric);
            if (classCost == Impossible || result >= Impossible) continue;
            var parent = previousRows[item.Value.Parent].Span;
            byte[] record = new byte[128];
            W64(record, 0, U64(query, 0));
            bool flagged = alphabet.IsSymbolFlagged(symbol) || (group.RestartTag28 & (4UL << 40)) != 0 && (parent[5] & 128) != 0;
            record[5] = (byte)((query[5] & 0x7d) | (flagged ? 128 : 0) | ((U64(query, 0) & 0x20000000000UL) != 0 ? 2 : 0));
            record[6] = (byte)(query[6] & 0xfc);
            W64(record, 8, U64(query, 8));
            W32(record, 16, result);
            W32(record, 20, unchecked(item.Value.Increment + classCost + geometric));
            W32(record, 24, unchecked((int)(1U | (uint)category << 16 | (uint)item.Key << 22)));
            W16(record, 28, item.Key); W16(record, 30, item.Value.Parent);
            record[33] = (byte)(((query[23] >> 5 & 3) + query[22]) & 63);
            W16(record, 34, ushort.MaxValue); W16(record, 36, symbol);
            record[39] = (byte)((query[23] & 7) << 1);
            W64(record, 40, ulong.MaxValue);
            int firstId = unchecked((int)graph.GetNode(1)[item.Key].StartContextId);
            W32(record, 48, (query[23] & 8) != 0 ? firstId : -1); W32(record, 52, firstId);
            W32(record, 56, 1); W16(record, 60, ushort.MaxValue); W16(record, 62, ushort.MaxValue);
            W64(record, 88, ulong.MaxValue); W32(record, 104, 0); W64(record, 112, U64(parent, 40));
            expected.Add(record);
        }
    }
    return expected;
}

static void Require(bool condition, string message) { if (!condition) throw new InvalidDataException(message); }
static int I32(ReadOnlySpan<byte> bytes, int at) => BinaryPrimitives.ReadInt32LittleEndian(bytes[at..]);
static ushort U16(ReadOnlySpan<byte> bytes, int at) => BinaryPrimitives.ReadUInt16LittleEndian(bytes[at..]);
static ulong U64(ReadOnlySpan<byte> bytes, int at) => BinaryPrimitives.ReadUInt64LittleEndian(bytes[at..]);
static void W16(Span<byte> bytes, int at, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(bytes[at..], value);
static void W32(Span<byte> bytes, int at, int value) => BinaryPrimitives.WriteInt32LittleEndian(bytes[at..], value);
static void W64(Span<byte> bytes, int at, ulong value) => BinaryPrimitives.WriteUInt64LittleEndian(bytes[at..], value);
