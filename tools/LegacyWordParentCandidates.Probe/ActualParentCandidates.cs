using System.Buffers.Binary;
using InkRecognition;

internal static class ActualParentCandidates
{
    public static void Run(string[] args)
    {
        byte[] ak = File.ReadAllBytes(args[1]), lk = File.ReadAllBytes(args[2]);
        var model = LegacyRussianModelBundle.Load(ak, lk, File.ReadAllBytes(args[3]));
        var prefix = LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lk.AsSpan(606964, 4565174 - 606964));
        uint order = LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lk.AsSpan(2513123, 4565174 - 2513123));
        var descriptor = LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(prefix, model.Lexicon, order, true);
        var dictionaryMasks = LegacyLinguisticSymbolMaskController.CreateDictionarySource(model.AcousticAlphabet, model.LexicalAlphabet);
        var masks = new LegacyPreparedOrdinaryLinguisticMaskBinding(model.AcousticAlphabet, ak.AsSpan(99, 16), [dictionaryMasks])
            .Build(false, true, [], [], [], []).Masks;
        var working = new LegacyPreparedDictionaryWorkingMap(model.AcousticAlphabet.LabelCount, descriptor.Flags, dictionaryMasks.DictionaryMap.Span, []);
        var retained = LegacyPreparedAcousticDictionaryPermissions.ReadReservedSlots(model.AcousticAlphabet, ak.AsSpan(99, 16));
        var raw = new[]
        {
            Draw((0,90),(0,10),(25,60),(50,10),(50,90)),
            Draw((70,10),(70,90),(120,10),(120,90)),
            Draw((140,90),(140,10),(180,10),(190,20),(190,40),(180,50),(140,50))
        };
        var source = LegacyOrdinaryCursiveCutSource.Generate(raw, LegacyOrdinarySourceInk.ReadV10dResourceProfile(ak), 90, 80);
        var seeds = LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(source.Cuts, 80,
            LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(ak, 189379), new byte[raw.Length], Enumerable.Repeat(-1, raw.Length).ToArray());
        var graph = LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,
            LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds), model.CursiveSpanProfile.MaximumAtoms);
        var connections = new LegacyOrdinaryCandidateConnections(graph);
        var options = new LegacyCursiveWordSearchOptions(90, 80, float.NaN, prefix.RuntimeLanguageScale, MaximumScoredGroups: 512);
        var policy = new LegacyOrdinaryAcousticCacheContext(true, true, true, false, false, null, false, ReadOnlyMemory<ushort>.Empty);
        var producer = new LegacyPreparedOrdinaryGraphAcousticProducer(model, ak, source,
            raw.Select(s => s.Points.Count).ToArray(), options, masks.Primary.Span, policy);
        var session = new LegacyNativeWordSymbolPreparationSession(graph, working, retained, producer.Compute);
        int node = 0;
        var prepared = session.Prepare(node, checked((byte)node), false);
        Require(prepared.PermissionPass.Succeeded && prepared.Qualifier is not null, "Actual finite-cost qualification preparation failed.");
        var qualifier = prepared.Qualifier!;
        var qualifiedBytes = qualifier.GetEligibilitySnapshot().ToArray();
        int computed = producer.ClassifiedGroups;
        var reused = session.Prepare(0, 0, true);
        Require(!reused.EligibilityRebuilt && reused.Qualifier!.GetEligibilitySnapshot().Span.SequenceEqual(qualifiedBytes),
            "Actual reuse branch rebuilt frontend88 from another node.");
        var refreshed = session.Prepare(node, checked((byte)node), false);
        Require(refreshed.Qualifier!.GetEligibilitySnapshot().Span.SequenceEqual(qualifiedBytes), "Actual refresh changed a deterministic union.");

        var contexts = new LegacySymbolContextFactory();
        ulong[] initial = contexts.CreateInitialSourceMap(dictionaryMasks.DictionaryMap.Span);
        var dictionary = new LegacyPreparedDictionaryQuerySource(model.Lexicon, prepared.PermissionPass.PermissionResult.WorkingMap!, initial);
        var aggregate = new LegacyPreparedAggregateDictionaryQuerySource([LegacyAggregateDictionaryChild.BindDictionary(dictionary, contexts)]);
        var query = new LegacyPreparedDictionaryQuery { Capacity = 128, ContextRoot = contexts.ContainerIdentity };
        ulong tag = LegacyOrdinaryDictionaryQueryPreparation.ApplyInitial(query, default, (uint)node, (uint)node, 0, false)!.Value;
        var thresholds = LegacyAcousticCandidateThresholds.ReadV110Node(ak.AsSpan(189734), out _);
        var scorer = LegacyNativeWordAcousticRequestScorer.ForAlphabet(thresholds, model.AcousticAlphabet);
        var categories = model.CreateCursiveCategoryMapper(1);
        var evaluator = new LegacyPreparedWordGroupEvaluator(model, source, options);
        var cache = new LegacyOrdinaryAcousticClassCache(LegacyOrdinaryAcousticCacheProfile.ReadV110Node(ak.AsSpan(189734)), model.AcousticAlphabet, ak.AsSpan(99, 16));
        var groupScorer = new LegacyCursiveFinalCacheGroupScorer(cache, LegacyPreparedAcousticCandidateScorer.ForAlphabet(thresholds, model.AcousticAlphabet),
            categories, LegacyCursiveSeparationProfile.ReadV106Node(ak, 188157), LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[4])),
            model.AcousticAlphabet, model.LexicalAlphabet, masks.Primary.Span, LegacyWideSymbolPairs.CreateUnrestricted(model.AcousticAlphabet), 0);
        var caches = new Dictionary<ushort, LegacyNativeWordAcousticGroupCosts>();
        foreach (var selected in prepared.PermissionPass.SelectedRows)
        {
            var glyph = evaluator.Classify(graph.Groups[selected.GroupId].Ranges.Span);
            var full = groupScorer.Score(glyph, source.BindCacheProbability(policy, glyph.Geometry), 80, 0, false, false, false);
            Require(selected.Costs.Span.SequenceEqual(full.Cache.Costs.Span), "Producer/cache ownership changed during qualification bridge.");
            caches.Add(checked((ushort)selected.Position.ArcIndex), new(selected.Costs.Span, full.CompositeCategoryCosts.Span));
        }
        var roots = connections.GetSuccessorOrdinals(-1, ushort.MaxValue, checked((byte)node)).ToArray();
        Require(roots.SequenceEqual(prepared.PermissionPass.SelectedRows.Select(r => r.Position.ArcIndex)), "Source-root native arc order differs from qualification selection.");
        var heaps = new[] { new LegacyNativeWordRecordHeap(64), new LegacyNativeWordRecordHeap(32) };
        var router = new LegacyNativeWordHeapRouter(heaps, new byte[] { 0, 0 }, 0, uint.MaxValue);
        var startup = new LegacyNativeOrdinaryWordStartup(graph, model.AcousticAlphabet, model.CursiveCategoryProfiles[1],
            contexts, scorer, new(0x1122334455667788, 0), new byte[80]);
        int pages = 0, queried = 0, qualified = 0, acousticCalls = 0, accepted = 0;
        while (true)
        {
            var page = aggregate.Read(tag, query, clearExtraCosts: true);
            if (page.ReturnedCount == 0) break;
            Require(page.ReturnedCount > 0, "Actual prepared-source query failed.");
            pages++;
            var result = startup.ProcessPage(page.Records.Span, (ushort)node, (byte)node, 0, qualifier,
                new Dictionary<byte, uint> { [1] = descriptor.Flags }, (owner, arc) =>
                {
                    Require(owner == node && caches.ContainsKey(arc), "Actual initial source row lost owning NN cache.");
                    return caches[arc];
                }, router, 1, false, 0, false, false, null);
            Require(result.Succeeded, "Actual native startup stopped on allocation failure.");
            queried += result.RowsVisited;
            qualified += result.QualifiedRows;
            acousticCalls += result.ScoringCalls;
            accepted += result.AcceptedAllocations;
            query.Skip += page.ReturnedCount;
        }
        int stored = 0;
        foreach (var heap in heaps)
            for (int i = 0; i < heap.Count; i++)
            {
                var slot = heap.GetSlotSnapshot(i);
                var row = LegacyNativeWordRowMaterializer.Materialize(slot.Span, (ushort)stored++, 1, 1, 1);
                Require(row.Record.Span[..60].SequenceEqual(slot.Span[..60]), "Qualified native startup row changed in materialization.");
            }
        var parents = Enumerable.Range(0, heaps[0].Count)
            .Select(i => LegacyNativeWordRowMaterializer.Materialize(heaps[0].GetSlotSnapshot(i).Span, (ushort)i, 1, 1, 1).Record.ToArray()).ToArray();
        Require(parents.Length > 0, "Actual initial page produced no stored parent rows.");
        var currentPass = session.Prepare(1, 0, false);
        var currentCaches = new Dictionary<ushort, LegacyNativeWordAcousticGroupCosts>();
        foreach (var selected in currentPass.PermissionPass.SelectedRows)
        {
            var glyph = evaluator.Classify(graph.Groups[selected.GroupId].Ranges.Span);
            var full = groupScorer.Score(glyph, source.BindCacheProbability(policy, glyph.Geometry), 80, 0, false, false, false);
            Require(selected.Costs.Span.SequenceEqual(full.Cache.Costs.Span), "Actual parent cache lost computed ownership.");
            currentCaches.Add((ushort)selected.Position.ArcIndex, new(selected.Costs.Span, full.CompositeCategoryCosts.Span));
        }
        var sourceFacts = Enumerable.Range(0, graph.NodeCount).Select(n =>
            (ReadOnlyMemory<LegacyMainWordArcSourceFacts>)Enumerable.Repeat(new LegacyMainWordArcSourceFacts(0, 0, 0, -1, 0, 0, 0), graph.GetNode(n).Count).ToArray()).ToArray();
        var geometry = new LegacyMainWordTransitionGeometry(LegacyMainWordTransitionProfile.ReadV106Node(ak, 188157),
            model.AcousticAlphabet, source.Cuts.PreparedInk, graph, sourceFacts, 80, -1,
            LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[4])));
        var continuationDictionary = new LegacyPreparedDictionaryQuerySource(model.Lexicon,
            currentPass.PermissionPass.PermissionResult.WorkingMap!, initial);
        var continuationSource = new LegacyPreparedAggregateDictionaryQuerySource([LegacyAggregateDictionaryChild.BindDictionary(continuationDictionary, contexts)]);
        var continuationHeaps = new[] { new LegacyNativeWordRecordHeap(512), new LegacyNativeWordRecordHeap(256) };
        var continuationRouter = new LegacyNativeWordHeapRouter(continuationHeaps, new byte[] { 0, 0 }, 0, uint.MaxValue);
        int parentPages = 0, parentQueries = 0, parentCalls = 0, parentAllocations = 0;
        for (ushort ordinal = 0; ordinal < parents.Length; ordinal++)
        {
            var parent = parents[ordinal];
            var requestQuery = new LegacyPreparedDictionaryQuery { Capacity = 128, ContextRoot = contexts.ContainerIdentity };
            var queryTag = LegacyOrdinaryDictionaryQueryPreparation.ApplyContinuation(requestQuery,
                new(0, 0, false, false, 99), parent, 1, 0, 0, false);
            Require(queryTag.HasValue, "Actual parent query preparation unexpectedly rejected its adjacent source.");
            ulong requestTag = queryTag ?? throw new InvalidDataException("Missing actual prepared continuation query tag.");
            var parentProducer = new LegacyNativeOrdinaryParentWordCandidates(graph, model.AcousticAlphabet,
                model.CursiveCategoryProfiles[1], contexts, geometry, scorer, new(0x1122334455667788, 0), new byte[80]);
            while (true)
            {
                var page = continuationSource.Read(requestTag, requestQuery, clearExtraCosts: true);
                if (page.ReturnedCount == 0) break;
                Require(page.ReturnedCount > 0, "Actual parent dictionary query failed.");
                parentPages++;
                var result = parentProducer.ProcessPage(page.Records.Span, 1, 0, 0, ordinal, parent, 0,
                    currentPass.Qualifier!, new Dictionary<byte, uint> { [1] = descriptor.Flags },
                    (owner, arc) => { Require(owner == 1 && currentCaches.ContainsKey(arc), "Parent128 cache ownership differs."); return currentCaches[arc]; },
                    (owner, arc) => { Require(owner == 0 && arc < parents.Length, "Parent heap-tail row ownership differs."); return parents[arc]; },
                    continuationRouter, 1, false, false, 0, false, false, null);
                Require(result.Succeeded, "Actual regular parent pass stopped on allocation failure.");
                parentQueries += result.RowsVisited;
                parentCalls += result.ScoringCalls;
                parentAllocations += result.AcceptedAllocations;
                requestQuery.Skip += page.ReturnedCount;
            }
        }
        Require(parentPages > 0 && parentCalls > 0 && parentAllocations > 0, "Actual parent producer emitted no continuation records.");
        int guards = 0;
        model.AcousticAlphabet.TryGetLabel("a", out ushort guardSymbol);
        byte[] guardQuery = new byte[32];
        BinaryPrimitives.WriteUInt64LittleEndian(guardQuery, 1UL << 32);
        BinaryPrimitives.WriteUInt64LittleEndian(guardQuery.AsSpan(8), contexts.ResolveInitial(guardSymbol).Identity);
        BinaryPrimitives.WriteUInt16LittleEndian(guardQuery.AsSpan(20), guardSymbol);
        var guardCosts = new LegacyNativeWordAcousticGroupCosts(new int[model.AcousticAlphabet.LabelCount + 3], new int[35]);
        var guardQualifier = new LegacyNativeWordSymbolQualifier(model.AcousticAlphabet.LabelCount, [guardCosts]);
        LegacyNativeOrdinaryParentWordCandidates NewGuard() => new(graph, model.AcousticAlphabet, model.CursiveCategoryProfiles[1],
            contexts, geometry, scorer, new(0x1122334455667788, 0), new byte[80]);
        LegacyNativeWordHeapRouter GuardRouter(int capacity = 16) => new([new(capacity), new(capacity)], new byte[] { 0, 0 }, 0, uint.MaxValue);
        LegacyNativeOrdinaryWordStartupResult RunGuard(LegacyNativeOrdinaryParentWordCandidates target,
            byte[] queryBytes, byte[] parentBytes, Func<int, ushort, LegacyNativeWordAcousticGroupCosts>? lookup = null,
            Func<int, ushort, ReadOnlyMemory<byte>>? rows = null, LegacyNativeWordHeapRouter? guardRouter = null,
            CancellationToken token = default)
            => target.ProcessPage(queryBytes, 1, 0, 0, 0, parentBytes, 0, guardQualifier,
                new Dictionary<byte, uint> { [1] = descriptor.Flags }, lookup ?? ((_, _) => guardCosts),
                rows ?? ((_, _) => parents[0]), guardRouter ?? GuardRouter(), 1, false, false, 0, false, false, null, token);
        void Reject(Action action)
        {
            try { action(); }
            catch (Exception error) when (error is ArgumentException or InvalidDataException or InvalidOperationException or NotSupportedException or OperationCanceledException)
            { guards++; return; }
            throw new Exception("Invalid parent candidate lifecycle accepted.");
        }
        Reject(() => RunGuard(NewGuard(), new byte[31], parents[0]));
        Reject(() => RunGuard(NewGuard(), guardQuery, new byte[87]));
        var specialParent = parents[0].ToArray();
        specialParent[6] |= 1;
        Reject(() => RunGuard(NewGuard(), guardQuery, specialParent));
        var specialQuery = guardQuery.ToArray();
        specialQuery[23] = 7;
        Reject(() => RunGuard(NewGuard(), specialQuery, parents[0]));
        foreach (bool cancelLookup in new[] { false, true })
        {
            using var canceled = new CancellationTokenSource();
            var target = NewGuard();
            Reject(() => RunGuard(target, guardQuery, parents[0], lookup: (_, _) =>
            {
                if (!cancelLookup) canceled.Cancel();
                return guardCosts;
            }, rows: (_, _) => { canceled.Cancel(); return parents[0]; }, token: canceled.Token));
            Reject(() => RunGuard(target, guardQuery, parents[0]));
        }
        var failed = NewGuard();
        Require(!RunGuard(failed, guardQuery, parents[0], guardRouter: GuardRouter(0)).Succeeded,
            "Native parent allocation failure did not stop.");
        Reject(() => RunGuard(failed, guardQuery, parents[0]));
        var mutatedParent = parents[0].ToArray();
        var mutatedQuery = guardQuery.ToArray();
        var ownerHeap = new LegacyNativeWordRecordHeap(16);
        var ownerRouter = new LegacyNativeWordHeapRouter([ownerHeap, new(16)], new byte[] { 0, 0 }, 0, uint.MaxValue);
        var copied = RunGuard(NewGuard(), mutatedQuery, mutatedParent, lookup: (_, _) =>
        {
            Array.Clear(mutatedParent);
            Array.Clear(mutatedQuery);
            return guardCosts;
        }, guardRouter: ownerRouter);
        Require(copied.Succeeded && copied.AcceptedAllocations > 0 && Enumerable.Range(0, ownerHeap.Count).All(i =>
            BinaryPrimitives.ReadUInt16LittleEndian(ownerHeap.GetSlotSnapshot(i).Span[0x24..]) == guardSymbol),
            "Parent/query input snapshots changed during a cost callback.");
        guards++;
        Console.WriteLine($"PASS {guards} parent callback cancellation/ownership/unsupported/invalidation/input-snapshot guards.");
        var flaggedLabel = Enumerable.Range(0, model.AcousticAlphabet.LabelCount).Select(i => (ushort)i)
            .First(i => Plain(model.AcousticAlphabet, i) && model.AcousticAlphabet.IsSymbolFlagged(i));
        var unflaggedLabel = Enumerable.Range(0, model.AcousticAlphabet.LabelCount).Select(i => (ushort)i)
            .First(i => Plain(model.AcousticAlphabet, i) && !model.AcousticAlphabet.IsSymbolFlagged(i));
        var flagRaw = new[] { Draw((0, 10), (20, 90)), Draw((100, 10), (120, 90)) };
        var flagInk = LegacyCursivePreparedInk.Create(flagRaw);
        var flagMarkers = new[] { new LegacyCursiveCutMarker(0, 0, 0), new LegacyCursiveCutMarker(0, 1, 0) };
        var flagCuts = new LegacyCursiveCutResult(flagInk, flagMarkers, LegacyCursiveInkAtoms.Build(flagInk.StrokePointCounts.Span, flagMarkers));
        var flagSeeds = LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(flagCuts, 80, new(false, 1, 0, .5f), new byte[2], new[] { -1, -1 });
        var flagGraph = LegacyOrdinaryCursiveCandidateGraph.Generate(flagSeeds, LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(flagSeeds), 2);
        var flagFacts = Enumerable.Range(0, flagGraph.NodeCount).Select(n =>
            (ReadOnlyMemory<LegacyMainWordArcSourceFacts>)Enumerable.Repeat(new LegacyMainWordArcSourceFacts(0, 0, 0, -1, 0, 0, 0), flagGraph.GetNode(n).Count).ToArray()).ToArray();
        var flagGeometry = new LegacyMainWordTransitionGeometry(LegacyMainWordTransitionProfile.ReadV106Node(ak, 188157),
            model.AcousticAlphabet, flagInk, flagGraph, flagFacts, 80, -1, LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[4])));
        int flagControls = 0;
        // Explicit native truth table: kind0 current flag still applies even
        //when parent flag is clear; kind2 does not inherit the parent's flag.
        foreach (var test in new[]
        {
            (Kind: (byte)0, Parent: false, Label: flaggedLabel, Expected: true),
            (Kind: (byte)0, Parent: true, Label: unflaggedLabel, Expected: true),
            (Kind: (byte)0, Parent: false, Label: unflaggedLabel, Expected: false),
            (Kind: (byte)2, Parent: true, Label: unflaggedLabel, Expected: false)
        })
        {
            var row = guardQuery.ToArray();
            row[23] = test.Kind;
            BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(20), test.Label);
            BinaryPrimitives.WriteUInt64LittleEndian(row.AsSpan(8), contexts.ResolveInitial(test.Label).Identity);
            var parent = parents[0].ToArray();
            parent[5] = (byte)((parent[5] & 0x7f) | (test.Parent ? 0x80 : 0));
            var target = new LegacyNativeOrdinaryParentWordCandidates(flagGraph, model.AcousticAlphabet, model.CursiveCategoryProfiles[1],
                contexts, flagGeometry, scorer, new(0x1122334455667788, 0), new byte[80]);
            var output = new LegacyNativeWordRecordHeap(16);
            var flagRouter = new LegacyNativeWordHeapRouter([output, new(16)], new byte[] { 0, 0 }, 0, uint.MaxValue);
            var result = target.ProcessPage(row, 1, 0, 0, 0, parent, 0, guardQualifier,
                new Dictionary<byte, uint> { [1] = descriptor.Flags }, (_, _) => guardCosts, (_, _) => parent,
                flagRouter, 1, false, false, 0, false, false, null);
            Require(result.Succeeded && result.AcceptedAllocations > 0 && Enumerable.Range(0, output.Count).All(i =>
                ((output.GetSlotSnapshot(i).Span[5] & 0x80) != 0) == test.Expected), "Independent native parent/ALflag truth table differs.");
            flagControls++;
        }
        Console.WriteLine($"PASS {flagControls} independent native parent/current alphabet headerflag5 controls.");
        for (int i = 0; i < continuationHeaps[0].Count; i++)
        {
            var slot = continuationHeaps[0].GetSlotSnapshot(i).Span;
            ushort previousRow = BinaryPrimitives.ReadUInt16LittleEndian(slot[0x1e..]);
            Require(previousRow < parents.Length && slot[0x20] == 0 && BinaryPrimitives.ReadUInt16LittleEndian(slot[0x18..]) == 1,
                "Actual continuation source/previous-row lineage differs.");
            Require(BinaryPrimitives.ReadInt32LittleEndian(slot[0x38..]) == BinaryPrimitives.ReadInt32LittleEndian(parents[previousRow].AsSpan(0x38)) &&
                BinaryPrimitives.ReadUInt16LittleEndian(slot[0x3c..]) == BinaryPrimitives.ReadUInt16LittleEndian(parents[previousRow].AsSpan(0x3c)),
                "Actual continuation lost its inherited owning anchor.");
        }
        Console.WriteLine($"ACTUAL parent88 -> {parentPages} typed continuation pages / {parentQueries} query rows -> {parentCalls} full201270118/140/128 chains -> {parentAllocations} native allocations / {continuationHeaps[0].Count} owning rows PASS. " +
            "Source facts/frame are explicit diagnostic inputs; no native winner/acceptance claim.");
        Require(queried > 0 && qualified > 0 && accepted > 0 && stored > 0, "Actual qualifier/query/cache/writer pipeline emitted no native rows.");
        Console.WriteLine($"ACTUAL RU {prepared.PermissionPass.SelectedRows.Count} native selected NN/cache rows ({computed} initial computations), " +
            $"{pages} permission-filtered query pages / {queried} query labels / {qualified} frontend110 qualifiers / {acousticCalls} exact128 calls / {accepted} accepted / {stored} materialized heap rows PASS. " +
            "Guide/cache policy, source headers and main source frame remain explicit; writer fields now come from the checked ordinary startup branch; scheduler unfinished; no native winner or acceptance confidence claim.");
    }

    private static InkStroke Draw(params (float X, float Y)[] points) => new(points.Select(p => new InkPoint(p.X, p.Y)).ToArray());
    private static bool Plain(LegacyInkAlphabet alphabet, ushort label) => alphabet.TryGetText(label, out string text) && text.Length == 1 &&
        text[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '\u0400' and <= '\u04ff' or >= '0' and <= '9';
    private static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
}
