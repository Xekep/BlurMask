using System.Buffers.Binary;
using InkRecognition;

internal static class ActualAlternativeCandidates
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

        // Checked source50 getter -> existing1f9080 cache writer. Linguistic
        //cost/state values are explicit fixtures, not a claimed204290 stage.
        var restartChildren = new[] { LegacyPreparedDictionaryRestartTags.CreateFresh() };
        var secondary = new ReadOnlyMemory<byte>[parents.Length];
        for (ushort i = 0; i < parents.Length; i++)
        {
            var parent = parents[i];
            ulong parentTag = BinaryPrimitives.ReadUInt64LittleEndian(parent);
            ulong restart = LegacyPreparedDictionaryRestartTags.ResolveAggregate(parentTag, restartChildren);
            uint[] payload = Enumerable.Range(0, 5).Select(j => BinaryPrimitives.ReadUInt32LittleEndian(parent.AsSpan(64 + j * 4))).ToArray();
            var fields = new LegacyContinuationWordRecordFields(parentTag,
                BinaryPrimitives.ReadInt32LittleEndian(parent.AsSpan(16)), BinaryPrimitives.ReadInt32LittleEndian(parent.AsSpan(20)),
                BinaryPrimitives.ReadUInt16LittleEndian(parent.AsSpan(62)), payload,
                BinaryPrimitives.ReadUInt64LittleEndian(parent.AsSpan(8)), restart, (byte)(parent[33] & 63), 0, 0);
            byte[] slot = new byte[128];
            LegacyContinuationWordRecordProducer.InitializeAllocatedSlot(slot, parent, i, fields, 0, true);
            secondary[i] = LegacyNativeWordRowMaterializer.Materialize(slot, i, 0, 1, 1).Record;
            Require(BinaryPrimitives.ReadUInt64LittleEndian(secondary[i].Span[40..]) == restart &&
                BinaryPrimitives.ReadUInt16LittleEndian(secondary[i].Span[30..]) == i, "Actual source50/cache writer lost restart or related-row lineage.");
        }
        var groups = LegacyNativeAlternativeWordQueryPreparation.GroupSecondaryRows(secondary, 0, 0, false, 0);
        var sourceDictionary = new LegacyPreparedDictionaryQuerySource(model.Lexicon,
            currentPass.PermissionPass.PermissionResult.WorkingMap!, initial);
        var querySource = new LegacyPreparedAggregateDictionaryQuerySource([LegacyAggregateDictionaryChild.BindDictionary(sourceDictionary, contexts)]);
        var output = new LegacyNativeWordRecordHeap(1024);
        var outputRouter = new LegacyNativeWordHeapRouter([output, new(512)], new byte[] { 0, 0 }, 0, uint.MaxValue);
        var alternative = new LegacyNativeOrdinaryAlternativeWordCandidates(graph, model.AcousticAlphabet, model.CursiveCategoryProfiles[1],
            contexts, geometry, scorer, new(0x1122334455667788, 0), new byte[80]);
        int actualPages = 0, actualQueries = 0, geometryCalls = 0, finalCalls = 0, allocations = 0;
        byte[]? firstQuery = null;
        LegacyNativeAlternativeWordQueryGroup? firstGroup = null;
        foreach (var group in groups)
        {
            var request = new LegacyPreparedDictionaryQuery { Capacity = 128, ContextRoot = contexts.ContainerIdentity };
            ulong restart = LegacyNativeAlternativeWordQueryPreparation.Apply(request, default, group, 1, 0, 0)!.Value;
            while (true)
            {
                var page = querySource.Read(restart, request, clearExtraCosts: true);
                Require(page.ReturnedCount >= 0, "Actual alternative query failed.");
                if (page.ReturnedCount == 0) break;
                actualPages++;
                for (int offset = 0; offset < page.Records.Length; offset += 32)
                {
                    firstQuery ??= page.Records.Span.Slice(offset, 32).ToArray();
                    firstGroup ??= group;
                    var result = alternative.ProcessQuery(page.Records.Span.Slice(offset, 32), group, secondary, 1, 0, 0, 0,
                        currentPass.Qualifier!, new Dictionary<byte, uint> { [1] = descriptor.Flags },
                        (owner, arc) => { Require(owner == 1 && currentCaches.ContainsKey(arc), "Alternative128 cache lost node/arc ownership."); return currentCaches[arc]; },
                        (owner, ordinal) => { Require(owner == 0 && ordinal < parents.Length, "Alternative related/history ordinal lost source ownership."); return parents[ordinal]; },
                        outputRouter, 0, false, false, false, false, 0, true, false, null);
                    Require(result.Succeeded, "Actual alternative allocator stopped unexpectedly.");
                    actualQueries++;
                    geometryCalls += result.GeometryCalls;
                    finalCalls += result.AcousticCalls;
                    allocations += result.AcceptedAllocations;
                }
                request.Skip += page.ReturnedCount;
            }
        }
        Require(actualQueries > 0 && geometryCalls > finalCalls && finalCalls > 0 && allocations > 0,
            "Actual per-arc reduction did not reduce real multiple-parent geometry into fewer acoustic calls.");
        for (int i = 0; i < output.Count; i++)
        {
            var row = output.GetSlotSnapshot(i).Span;
            ushort parentOrdinal = BinaryPrimitives.ReadUInt16LittleEndian(row[30..]);
            Require(parentOrdinal < secondary.Length && BinaryPrimitives.ReadInt32LittleEndian(row[56..]) == 1 &&
                BinaryPrimitives.ReadUInt16LittleEndian(row[60..]) == ushort.MaxValue && row[32] == 0,
                "Alternative writer lost best-parent ordinal, new anchor or source span.");
        }
        Console.WriteLine($"ACTUAL {parents.Length} NN-derived parent88 -> source50/1f9080 -> {groups.Count} restart groups / {actualPages} typed pages / {actualQueries} query labels -> " +
            $"{geometryCalls} real main118/140 calls -> {finalCalls} reduced128 calls -> {allocations} native allocations PASS. " +
            "Geometry frame, secondary LM cache costs/state and one-alternative storage are explicit diagnostic fixtures;204290/2144f0 not claimed.");

        int guards = 0;
        void Reject(Action action)
        {
            try { action(); }
            catch (Exception error) when (error is ArgumentException or InvalidDataException or InvalidOperationException or NotSupportedException or OperationCanceledException)
            { guards++; return; }
            throw new InvalidDataException("Invalid alternative lifecycle accepted.");
        }
        var guardCosts = new LegacyNativeWordAcousticGroupCosts(new int[model.AcousticAlphabet.LabelCount + 3], new int[35]);
        var guardQualifier = new LegacyNativeWordSymbolQualifier(model.AcousticAlphabet.LabelCount, [guardCosts]);
        LegacyNativeOrdinaryAlternativeWordCandidates NewGuard() => new(graph, model.AcousticAlphabet, model.CursiveCategoryProfiles[1],
            contexts, geometry, scorer, new(0x1122334455667788, 0), new byte[80]);
        LegacyNativeAlternativeWordCandidateResult GuardRun(LegacyNativeOrdinaryAlternativeWordCandidates target,
            byte[]? queryBytes = null, LegacyNativeAlternativeWordQueryGroup? group = null,
            Func<int, ushort, LegacyNativeWordAcousticGroupCosts>? costs = null,
            Func<int, ushort, ReadOnlyMemory<byte>>? lookup = null, int capacity = 512,
            float ec = 0, bool penalty = false, bool limited = false, CancellationToken token = default,
            IReadOnlyList<ReadOnlyMemory<byte>>? rows = null)
            => target.ProcessQuery(queryBytes ?? firstQuery!, group ?? firstGroup!, rows ?? secondary, 1, 0, 0, 0,
                guardQualifier, new Dictionary<byte, uint> { [1] = descriptor.Flags }, costs ?? ((_, _) => guardCosts),
                lookup ?? ((_, ordinal) => parents[ordinal]), new([new(capacity), new(capacity)], new byte[] { 0, 0 }, 0, uint.MaxValue),
                ec, penalty, limited, false, false, 0, true, false, null, token);
        Reject(() => GuardRun(NewGuard(), new byte[31]));
        Reject(() => GuardRun(NewGuard(), group: firstGroup! with { SmallState21 = 64 }));
        Reject(() => GuardRun(NewGuard(), ec: 1));
        Reject(() => GuardRun(NewGuard(), penalty: true));
        Reject(() => GuardRun(NewGuard(), limited: true));
        Reject(() => GuardRun(NewGuard(), lookup: (_, _) => new byte[87]));
        var specialRows = secondary.Select(row => (ReadOnlyMemory<byte>)row.ToArray()).ToArray();
        var specialParent = specialRows[firstGroup!.FirstRowOrdinal].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(specialParent.AsSpan(36), ushort.MaxValue);
        specialRows[firstGroup.FirstRowOrdinal] = specialParent;
        Reject(() => GuardRun(NewGuard(), rows: specialRows));
        Reject(() => GuardRun(NewGuard(), group: firstGroup with { RestartTag28 = firstGroup.RestartTag28 ^ 1 }));
        foreach (bool cancelCost in new[] { false, true })
        {
            using var canceled = new CancellationTokenSource();
            var target = NewGuard();
            Reject(() => GuardRun(target, costs: (_, _) => { if (cancelCost) canceled.Cancel(); return guardCosts; },
                lookup: (_, ordinal) => { if (!cancelCost) canceled.Cancel(); return parents[ordinal]; }, token: canceled.Token));
            Reject(() => GuardRun(target));
        }
        var failed = NewGuard();
        Require(!GuardRun(failed, capacity: 0).Succeeded, "Alternative allocation failure did not stop the producer.");
        Reject(() => GuardRun(failed));
        Console.WriteLine($"PASS {guards} alternative consumer ownership, cancellation, unsupported policy and invalidated allocation guards.");
    }

    private static InkStroke Draw(params (float X, float Y)[] points) => new(points.Select(p => new InkPoint(p.X, p.Y)).ToArray());
    private static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
}
