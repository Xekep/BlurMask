using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if (args.Length != 5) throw new ArgumentException("controls.json AK LK engine.json separation.json required");
static void Require(bool value, string message) { if (!value) throw new Exception(message); }
static byte[] Bytes(JsonElement test, string name) => Convert.FromHexString(test.GetProperty(name).GetString()!);
static int[] Ints(JsonElement test, string name) => test.GetProperty(name).EnumerateArray().Select(v => v.GetInt32()).ToArray();
byte[] ak = File.ReadAllBytes(args[1]);
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int count = 0, providerCalls = 0, cancellationControls = 0;
foreach (var test in controls.RootElement.GetProperty("cases").EnumerateArray())
{
    int offset = test.GetProperty("offset").GetInt32();
    var profile = LegacyCursiveCategoryProfile.Load(ak.AsSpan(offset), offset, out _);
    var thresholds = Ints(test, "thresholds");
    var scorer = new LegacyNativeWordAcousticRequestScorer(new(thresholds[0], thresholds[1], thresholds[2]),
        test.GetProperty("bypass").EnumerateArray().Select(v => v.GetUInt16()).ToArray());
    var cell = new LegacyNativeWordTransitionCostCell(0x1122334455667788, 555);
    var request = LegacyNativeWordStartupRequestFactory.Create(test.GetProperty("node").GetUInt32(),
        test.GetProperty("span").GetUInt32(), test.GetProperty("arc").GetUInt16(), test.GetProperty("symbol").GetUInt16(),
        test.GetProperty("category").GetInt32(), test.GetProperty("tag").GetUInt64(), test.GetProperty("length").GetByte(),
        test.GetProperty("flags").GetByte(), test.GetProperty("header").GetUInt32(), test.GetProperty("permission").GetByte(),
        profile, cell, test.GetProperty("before").GetInt32(), Bytes(test, "residue"));
    Require(request.GetSnapshot().Span.SequenceEqual(Bytes(test, "initial")) && cell.Value == test.GetProperty("before").GetInt32(),
        $"Independent full80 startup request differs at{count}.");
    var group = new LegacyNativeWordAcousticGroupCosts(Ints(test, "classes"), Ints(test, "geometry"));
    int called = 0;
    bool accepted = scorer.Score(request, (node, arc) =>
    {
        Require(node == test.GetProperty("node").GetInt32() && arc == test.GetProperty("arc").GetUInt16(), "Owned group cache addressed wrongly.");
        called++; return group;
    }, test.GetProperty("suppressionEnabled").GetBoolean(), (_, _) => test.GetProperty("suppressed").GetBoolean());
    Require(accepted == test.GetProperty("accepted").GetBoolean() && cell.Value == test.GetProperty("after").GetInt32() &&
        request.GetSnapshot().Span.SequenceEqual(Bytes(test, "expected")) && called == (test.GetProperty("lookup").GetBoolean() ? 1 : 0),
        $"Independent sourcekind/geometry/wrap/full80 score differs at{count}.");
    providerCalls += called;
    if (count < 8)
    {
        using var canceled = new CancellationTokenSource();
        try
        {
            scorer.Score(request, (_, _) => { canceled.Cancel(); return group; }, false, cancellationToken: canceled.Token);
            throw new Exception("Canceled acoustic cache provider returned successfully.");
        }
        catch (OperationCanceledException) { cancellationControls++; }
    }
    count++;
}
Console.WriteLine($"PASS {count} full80 startup/scoring controls / {providerCalls} exact cache owners / {cancellationControls} provider cancel guards; " +
    $"{controls.RootElement.GetProperty("differentIndexCases").GetInt32()} nonuniform geometry-index mismatches /" +
    $"{controls.RootElement.GetProperty("wrappedCases").GetInt32()} native signed32 wraps. Sourcekind40 versus transitionkind1c verified.");

// Real typed query pages and real final NN cache feed the exact request/scoring
//consumer and existing native writer/router. This is scoped transport, not
//the missing general110 qualifier or complete1fd240/2144f0 scheduler.
var lk = File.ReadAllBytes(args[2]);
var model = LegacyRussianModelBundle.Load(ak, lk, File.ReadAllBytes(args[3]));
InkStroke Stroke(params (float X, float Y)[] points) => new(points.Select(p => new InkPoint(p.X, p.Y)).ToArray());
var raw = new[]
{
    Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),
    Stroke((70,10),(70,90),(120,10),(120,90)),
    Stroke((140,90),(140,10),(180,10),(190,20),(190,40),(180,50),(140,50))
};
var cutSource = LegacyOrdinaryCursiveCutSource.Generate(raw, LegacyOrdinarySourceInk.ReadV10dResourceProfile(ak), 90, 80);
var seeds = LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cutSource.Cuts, 80,
    LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(ak, 189379), new byte[raw.Length], Enumerable.Repeat(-1, raw.Length).ToArray());
var graph = LegacyOrdinaryCursiveCandidateGraph.Generate(seeds, LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds), model.CursiveSpanProfile.MaximumAtoms);
int endingNode = graph.NodeCount - 1;
ushort sourceArc = (ushort)Enumerable.Range(0, graph.GetNode(endingNode).Count).First(i => graph.GetNode(endingNode)[i].Span == endingNode);
var evaluator = new LegacyPreparedWordGroupEvaluator(model, cutSource, new(90, 80, 0, 1));
var glyph = evaluator.Classify(graph.Groups[graph.GetNode(endingNode)[sourceArc].GroupId].Ranges.Span);
var cache = new LegacyOrdinaryAcousticClassCache(LegacyOrdinaryAcousticCacheProfile.ReadV110Node(ak.AsSpan(189734)), model.AcousticAlphabet, ak.AsSpan(99, 16));
var threshold = LegacyAcousticCandidateThresholds.ReadV110Node(ak.AsSpan(189734), out _);
var scalar = LegacyPreparedAcousticCandidateScorer.ForAlphabet(threshold, model.AcousticAlphabet);
var categories = model.CreateCursiveCategoryMapper(1);
var bridge = new LegacyCursiveFinalCacheGroupScorer(cache, scalar, categories,
    LegacyCursiveSeparationProfile.ReadV106Node(ak, 188157), LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[4])),
    model.AcousticAlphabet, model.LexicalAlphabet, Enumerable.Repeat((byte)1, model.AcousticAlphabet.LabelCount).ToArray(),
    LegacyWideSymbolPairs.CreateUnrestricted(model.AcousticAlphabet), relatedFormMode: 0);
var scope = new LegacyOrdinaryAcousticCacheContext(true, true, true, false, false, null, false, ReadOnlyMemory<ushort>.Empty);
var scored = bridge.Score(glyph, cutSource.BindCacheProbability(scope, glyph.Geometry), 80, 0, false, false, false);
var actualGroup = new LegacyNativeWordAcousticGroupCosts(scored.Cache.Costs.Span, scored.CompositeCategoryCosts.Span);
var actualScorer = LegacyNativeWordAcousticRequestScorer.ForAlphabet(threshold, model.AcousticAlphabet);
var contexts = new LegacySymbolContextFactory();
ushort[] exactMap = LegacyLinguisticSymbolMaskController.CreateExactDictionaryMap(model.AcousticAlphabet, model.LexicalAlphabet);
ulong[] initial = contexts.CreateInitialSourceMap(exactMap);
int graftControls = 0;
for (ushort key = 0; key < 64; key++)
{
    ulong sourceContext = contexts.ResolveInitial(key).Identity;
    ulong firstGraft = LegacyNativeWordStartupRequestFactory.GraftOrdinaryInitialContext(contexts, sourceContext);
    int nodes = contexts.NodeCount;
    ulong secondGraft = LegacyNativeWordStartupRequestFactory.GraftOrdinaryInitialContext(contexts, sourceContext);
    var child = contexts.ReadNode(firstGraft); var root = contexts.ReadNode(child.ParentLink);
    Require(firstGraft == secondGraft && contexts.NodeCount == nodes && child.Symbol == key && root.Symbol == 0xfffa && root.ParentLink == 0,
        "Independent ordinary sentinel-graft ownership/dedup changed.");
    Require(LegacyNativeWordStartupRequestFactory.GraftOrdinaryInitialContext(contexts, firstGraft) == firstGraft &&
        contexts.NodeCount == nodes, "Prepared FFFA prefix/AL leaf was rewritten or allocated twice.");
    graftControls++;
    var foreignPrefix = contexts.ResolveInitial(0xfffb);
    var foreignLeaf = contexts.ResolveChild(foreignPrefix.Identity, key);
    if (foreignLeaf.Created) contexts.LinkCreatedContext(foreignLeaf.Identity, foreignPrefix.Identity);
    var secondLeaf = contexts.ResolveChild(firstGraft, key);
    if (secondLeaf.Created) contexts.LinkCreatedContext(secondLeaf.Identity, firstGraft);
    foreach (ulong unsupported in new[] { foreignLeaf.Identity, secondLeaf.Identity })
    {
        try
        {
            LegacyNativeWordStartupRequestFactory.GraftOrdinaryInitialContext(contexts, unsupported);
            throw new Exception("Complex source-context rewrite was silently treated as ordinary.");
        }
        catch (NotSupportedException) { graftControls++; }
    }
}
var prefix = LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lk.AsSpan(606964, 4565174 - 606964));
uint order = LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lk.AsSpan(2513123, 4565174 - 2513123));
var descriptor = LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(prefix, model.Lexicon, order, true);
var dictionary = new LegacyPreparedDictionaryQuerySource(model.Lexicon,
    new LegacyPreparedDictionaryWorkingMap(model.AcousticAlphabet.LabelCount, descriptor.Flags, exactMap, exactMap)
        .Refresh(Enumerable.Repeat((byte)7, model.AcousticAlphabet.LabelCount).ToArray()), initial);
var aggregate = new LegacyPreparedAggregateDictionaryQuerySource([LegacyAggregateDictionaryChild.BindDictionary(dictionary, contexts)]);
var query = new LegacyPreparedDictionaryQuery { Capacity = 128, ContextRoot = contexts.ContainerIdentity };
ulong startupTag = LegacyOrdinaryDictionaryQueryPreparation.ApplyInitial(query, default, (uint)endingNode, (uint)endingNode, 0, false)!.Value;
var heaps = new[] { new LegacyNativeWordRecordHeap(4), new LegacyNativeWordRecordHeap(2) };
var router = new LegacyNativeWordHeapRouter(heaps, new byte[] { 0, 0 }, 0, uint.MaxValue);
int pages = 0, actualQueries = 0, acceptedCandidates = 0, grafted = 0;
while (true)
{
    var page = aggregate.Read(startupTag, query, clearExtraCosts: true);
    if (page.ReturnedCount == 0) break;
    Require(page.ReturnedCount > 0, "Actual query page failed.");
    pages++;
    for (int i = 0; i < page.ReturnedCount; i++)
    {
        var record = page.Records.Span.Slice(i * 32, 32);
        ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(record[20..]);
        if (symbol >= model.AcousticAlphabet.LabelCount) continue;
        int category = categories.GetSymbolCompositeCategory(symbol);
        if (category < 0) continue;
        var cell = new LegacyNativeWordTransitionCostCell(0x1122334455667788, 0);
        var request = LegacyNativeWordStartupRequestFactory.Create((uint)endingNode, (uint)endingNode, sourceArc,
            symbol, category, BinaryPrimitives.ReadUInt64LittleEndian(record), record[22], record[23], descriptor.Flags, 0,
            model.CursiveCategoryProfiles[1], cell, BinaryPrimitives.ReadInt32LittleEndian(record[16..]), new byte[80]);
        bool accepted = actualScorer.Score(request, (node, arc) =>
        {
            Require(node == endingNode && arc == sourceArc, "Actual glyph cache ownership lost.");
            return actualGroup;
        }, false);
        actualQueries++;
        if (!accepted) continue;
        ulong context = LegacyNativeWordStartupRequestFactory.GraftOrdinaryInitialContext(contexts, BinaryPrimitives.ReadUInt64LittleEndian(record[8..]));
        var child = contexts.ReadNode(context); var sentinel = contexts.ReadNode(child.ParentLink);
        Require(child.Symbol == symbol && sentinel.Symbol == 0xfffa && sentinel.ParentLink == 0,
            "Actual initial context did not graft onto native FFFA prefix.");
        grafted++;
        var fields = new LegacyInitialWordRecordFields(BinaryPrimitives.ReadUInt64LittleEndian(record), context,
            cell.Value, request.ReadInt32(48), symbol, (ushort)endingNode, sourceArc, sourceArc, (byte)category,
            ushort.MaxValue, (sbyte)endingNode, 0, 0, 0, 1, (byte)(record[23] & 7), record[22], 0, 0, -1, 0, endingNode, ushort.MaxValue);
        router.TryInsertInitialRecord(0, false, fields, 1, false);
        acceptedCandidates++;
    }
    query.Skip += page.ReturnedCount;
}
Require(actualQueries > 0 && acceptedCandidates > 0 && grafted == acceptedCandidates && heaps.Sum(h => h.Count) > 0,
    "Actual query/cache/scoring transport emitted no owned records.");
int stored = 0;
foreach (var heap in heaps)
    for (int i = 0; i < heap.Count; i++)
    {
        var slot = heap.GetSlotSnapshot(i);
        var row = LegacyNativeWordRowMaterializer.Materialize(slot.Span, (ushort)stored++, 1, 1, 1);
        Require(row.Record.Span[..60].SequenceEqual(slot.Span[..60]), "Actual scored startup did not materialize intact.");
    }
Console.WriteLine($"ACTUAL RU {pages} typed query pages -> {actualQueries} real final-cache128 calls -> {grafted} nativeFFFA grafts -> {stored} owning native heap rows PASS; {graftControls} bounded graft controls. " +
    "Frame/permissions/qualification and scheduler are scoped diagnostic inputs; no complete startup/acceptance claim.");
