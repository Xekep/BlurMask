using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

//This is an actual owned mainNN/cache/D0 acoustic-prefix consumer, not a
//complete2144F0 run: the intervening language/history phase is not scheduled.
internal static class ActualCasePrefix
{
    internal static void Run(string[] args)
    {
        byte[] ak = File.ReadAllBytes(args[1]), lk = File.ReadAllBytes(args[2]), engine = File.ReadAllBytes(args[3]);
        var model = LegacyRussianModelBundle.Load(ak, lk, engine); var table = LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[4]));
        using var c = File.OpenRead(args[5]); using var p = File.OpenRead(args[6]);
        var cases = LegacyNativeCaseData.Load(c); var publication = LegacyNativeTextPublicationData.Load(p);
        var sampled = MainFixture.Create(ak, engine, table);
        var dictionary = LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak, lk, model, sampled, cases, publication);
        var owner = new LegacyOwnedMainWordSpanPreparation(ak, dictionary, sampled, table);
        var policy = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, owner); var config = policy.Configuration;
        var headers = owner.CopySourceHeaders(); var headerMap = headers.Select((h, i) => (h, i)).ToDictionary(v => (byte)v.i, v => v.h);
        var geometry = sampled.CreatePrepared240TransitionGeometry(table);
        var categories = owner.SelectedSourceOwner.Categories;
        var cell = new LegacyNativeWordTransitionCostCell(0x1234, 0);
        var initial = new LegacyNativeMainInitialSpecialCandidates(ak, owner, cell, new byte[80]);
        var scorer = LegacyNativeWordAcousticRequestScorer.ForAlphabet(LegacyAcousticCandidateThresholds.ReadV110Node(ak.AsSpan(owner.OrdinaryAcousticOffset), out _), model.AcousticAlphabet);
        var parentConsumer = new LegacyNativeOrdinaryParentWordCandidates(owner.SourceGraph, model.AcousticAlphabet, categories,
            owner.Contexts, geometry, scorer, cell, new byte[80]);
        LegacyOwnedMainWordSpan? span = null;
        var delimiter = new LegacyNativeDelimiterQuerySource(model.AcousticAlphabet, owner.Contexts,
            (tag, q, token) => owner.Read(span ?? throw new Exception("Missing live D0."), tag, q, false, false, token),
            LegacyNativeDelimiterQuerySource.ReadV10dCoordinateMode(ak));
        int capacity = config.PrimaryArcLimit230a;
        LegacyNativeWordHeapRouter Router(LegacyNativeWordRecordHeap heap) => new([heap], new byte[] { 0, 0 }, -1, uint.MaxValue);
        owner.AdvanceNodeEpoch(-1); span = owner.Prepare(0, 0);
        var rootHeap = new LegacyNativeWordRecordHeap(capacity); var rootRouter = Router(rootHeap);
        var rootQuery = new LegacyNativeDelimiterQuery { SourceTag = 8UL << 40, Request = new() { Capacity = 128, Parameter1c = 0, Parameter20 = 0, ContextRoot = owner.Contexts.ContainerIdentity } };
        int rootQueries = ReadAll(rootQuery, page => initial.ProcessPreparedPage(span, page.Records.Span, 0, rootRouter, 0, false, false));
        var rootSlots = LegacyNativeWordAlternativeOrdinals.AssignInitial(rootHeap.GetSlotStorageSnapshot().Span, rootHeap.Count).SlotStorage.ToArray();
        byte[] rootPrimary = Residue(capacity * 80, 17), secondary = [];
        var reset = new LegacyNativeHigherArcResetParameters(65535, uint.MaxValue);
        var recombination = new LegacyNativeHigherArcRecombinationPolicy(65535, 120, false, 0, 0);
        var root = LegacyNativePrimaryWordRecombination.Apply(rootSlots, rootHeap.Count, 0, config,
            _ => throw new Exception("Root prefix queried a predecessor."), s => headers[s], categories, owner.Contexts, s => s, reset, recombination,
            default, null, 0, new byte[80], rootPrimary, secondary);
        var rootReference = LegacyNativeRootWordAlternativeProducer.Create(rootSlots, rootHeap.Count, 0, config,
            owner.Contexts, s => s, rootPrimary, secondary);
        if (!root.PrimaryStorage40.Span.SequenceEqual(rootReference.PrimaryStorage.Span)) throw new Exception("Actual root regression differs.");
        int extent = capacity + (int)(((config.PackedSource22e8 & 16) != 0 ? 2f : 1f) * config.ExpansionFactor230c * capacity);
        var rootNode = new LegacyOwnedFreshMainWordNodeBuffer(policy, 0, new byte[extent * 88], new byte[capacity * 80]);
        for (int i = 0; i < rootHeap.Count; i++) rootNode.WritePrimary(root.HeapStorage128.Span.Slice(i * 128, 128), root.PrimaryStorage40.Span);
        var rows = rootNode.CopyRows();
        delimiter.ResetForControllerNode(); owner.AdvanceNodeEpoch(0); span = owner.Prepare(1, 0);
        var childHeap = new LegacyNativeWordRecordHeap(capacity); var childRouter = Router(childHeap); int childQueries = 0;
        for (ushort ordinal = 0; ordinal < rootHeap.Count; ordinal++)
        {
            var parent = rows[ordinal].Record; ushort parentOrdinal = ordinal;
            var query = new LegacyPreparedDictionaryQuery { Capacity = 128, ContextRoot = owner.Contexts.ContainerIdentity };
            ulong? tag = LegacyOrdinaryDictionaryQueryPreparation.ApplyContinuation(query, new(0, 0, false, false, 120), parent.Span, 1, 0, 0, false);
            if (!tag.HasValue) continue;
            childQueries += ReadAll(new() { SourceTag = tag.Value, Request = query }, page =>
            {
                var consumed = parentConsumer.ProcessPage(page.Records.Span, 1, 0, 0, parentOrdinal, parent.Span, 0,
                    span.Qualifier, headerMap, (node, arc) => owner.GetCosts(span, node, arc), (node, arc) =>
                    node == 0 ? rows[arc].Record : throw new Exception("Foreign actual parent node."), childRouter, 0, false, false, 0, false, false, null);
                if (!consumed.Succeeded) throw new Exception("Actual parent query failed.");
            });
        }
        var childSlots = LegacyNativeWordAlternativeOrdinals.AssignInitial(childHeap.GetSlotStorageSnapshot().Span, childHeap.Count).SlotStorage.ToArray();
        byte[] childPrimary = Residue(capacity * 80, 29);
        var child = LegacyNativePrimaryWordRecombination.Apply(childSlots, childHeap.Count, 1, config,
            node => node == 0 ? rows : throw new Exception("Foreign source count owner."), s => headers[s], categories, owner.Contexts, s => s,
            reset, recombination, default, null, 0, new byte[80], childPrimary, secondary);
        var childReference = LegacyNativeInitialParentWordAlternativeProducer.Create(childSlots, childHeap.Count, 1, config,
            (node, arc) => node == 0 ? rows[arc] : throw new Exception("Foreign row."), owner.Contexts, s => s, childPrimary, secondary);
        if (rootHeap.Count == 0 || childHeap.Count == 0 || !child.PrimaryStorage40.Span.SequenceEqual(childReference.PrimaryStorage.Span) ||
            root.Steps.Any(s => s.Route != LegacyNativePrimaryWordRecombinationRoute.Root) ||
            child.Steps.Any(s => s.Route != LegacyNativePrimaryWordRecombinationRoute.PrimaryParent))
            throw new Exception("Actual D0 acoustic-prefix path did not exercise root/ordinary-parent owners.");
        using var file = File.Create(args[7]); using var w = new Utf8JsonWriter(file);
        w.WriteStartObject(); w.WriteString("scope", "actual CASE D0 acoustic prefixes; no language/history or whole scheduler claim");
        w.WriteStartArray("stages"); Write("root", 0, rootSlots, rootPrimary, root); Write("primary", 1, childSlots, childPrimary, child); w.WriteEndArray();
        w.WriteStartArray("parents"); foreach (var row in rows)
        { w.WriteStartObject(); w.WriteBase64String("row", row.Record.Span); w.WriteBase64String("primary", row.PrimaryAlternatives.Span); w.WriteEndObject(); }
        w.WriteEndArray();
        w.WriteStartArray("contexts"); foreach (var node in owner.Contexts.CopyNodes())
        { w.WriteStartArray(); w.WriteNumberValue(node.Identity); w.WriteNumberValue(node.ParentLink); w.WriteNumberValue(node.Symbol); w.WriteEndArray(); }
        w.WriteEndArray();
        w.WriteNumber("rootQueries", rootQueries); w.WriteNumber("childQueries", childQueries); w.WriteNumber("sourceEnd", rootNode.SourceEnd23f0); w.WriteEndObject(); w.Flush();
        Console.WriteLine($"PASS actual CASE owned D0/mainNN/cache: {rootQueries} root queries -> {rootHeap.Count} root slots; {childQueries} continuation queries -> {childHeap.Count} ordinary-parent slots; source count {rootNode.SourceEnd23f0}. No higher inference branch claimed.");
        void Write(string kind, int ending, byte[] beforeHeap, byte[] beforeP, LegacyNativePrimaryWordRecombinationResult result)
        {
            w.WriteStartObject(); w.WriteString("kind", kind); w.WriteNumber("ending", ending); w.WriteNumber("active", result.Steps.Count);
            w.WriteBase64String("heapBefore", beforeHeap); w.WriteBase64String("heapAfter", result.HeapStorage128.Span);
            w.WriteBase64String("primaryBefore", beforeP); w.WriteBase64String("primaryAfter", result.PrimaryStorage40.Span); w.WriteEndObject();
        }
        int ReadAll(LegacyNativeDelimiterQuery query, Action<LegacyNativeDelimiterQueryPage> consume)
        {
            int read = 0; for (int i = 0; i < 16384; i++)
            { var page = delimiter.Read(query); if (page.ReturnedCount == 0) return read; if (page.ReturnedCount < 0) throw new Exception("Native page failure."); consume(page); read += page.ReturnedCount; query.Request.Skip += page.ReturnedCount; }
            throw new Exception("Explicit native paging budget exceeded.");
        }
    }
    private static byte[] Residue(int count, int stride) => Enumerable.Range(0, count).Select(i => (byte)(i * stride + 3)).ToArray();
}
