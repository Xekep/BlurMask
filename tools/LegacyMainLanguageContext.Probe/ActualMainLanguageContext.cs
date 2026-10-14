using System.Buffers.Binary;
using InkRecognition;

internal static class ActualMainLanguageContext
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
        int node = Enumerable.Range(0, graph.NodeCount).Last(index => graph.GetNode(index).Any(arc =>
            arc.Span == 0 && graph.Groups[arc.GroupId].Ranges.Span[0].StrokeIndex == 0));
        Require(node < model.CursiveSpanProfile.MaximumAtoms, "Actual whole initial physical stroke exceeds the resource root span.");
        var prepared = session.Prepare(node, checked((byte)node), false);
        Require(prepared.PermissionPass.Succeeded && prepared.Qualifier is not null, "Actual finite-cost qualification preparation failed.");
        var qualifier = prepared.Qualifier!;
        var qualifiedBytes = qualifier.GetEligibilitySnapshot().ToArray();
        int computed = producer.ClassifiedGroups;
        var reused = session.Prepare(node, checked((byte)node), true);
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

        var metadata = new LegacyPreparedDictionaryWordMetadata(model.Lexicon, model.WordIndex);
        var recordProducer = new LegacyNativeKnownWordLanguageRecord(model.LanguageModel,
            model.LanguageProfiles[model.ResourceLanguageProfileIndex], model.AuxiliaryProfiles[model.ResourceLanguageProfileIndex], prefix.RuntimeLanguageScale);
        var completion = new LegacyNativeFirstDictionaryWordCompletion(model,
            metadata, recordProducer, LegacyPreparedDictionaryRestartTags.CreateFresh(), 1, descriptor.Flags, false, 0, false, false);
        var output = new LegacyNativeWordRecordHeap(256);
        var completionRouter = new LegacyNativeWordHeapRouter([output, new(128)], new byte[] { 0, 0 }, 0, uint.MaxValue);
        var gate = new LegacyNativeWordLanguageGate(model, graph);
        uint[] sourceHeaders = [0, descriptor.Flags];
        var refresh = LegacyPreparedLinguisticRefreshPolicy.Summarize(sourceHeaders, false, true, -1, -1);
        Require((refresh.Flags & 8) == 0 && gate.HasFollowingBoundary(node),
            "Actual physical boundary must produce the language gate, independently of an aggregate bypass bit.");
        Require(node > 0 && !gate.HasFollowingBoundary(node - 1),
            "A partial physical stroke incorrectly acquired the following-boundary language gate.");
        byte[] actualRows = parents.SelectMany(row => row).ToArray();
        int completed = 0;
        for (ushort ordinal = 0; ordinal < parents.Length; ordinal++)
        {
            var candidate = LegacyNativeFirstDictionaryWordCompletion.CreateAlreadyTerminalQuery(parents[ordinal], new byte[32]);
            if (!candidate.HasValue) continue;
            Require(gate.ShouldRun(node, -1, refresh.Flags, (ushort)parents.Length, actualRows, 0, sourceHeaders),
                "Actual refreshed source header rejected its first-word language stage.");
            var result = completion.ProcessCandidate(candidate.Value.Span, node, ordinal, parents[ordinal], completionRouter,
                false, Enumerable.Repeat((byte)0xa7, 40).ToArray(),
                (owner, arc) => { Require(owner == node && arc < parents.Length, "Actual LM anchor ordinal lost row ownership."); return parents[arc]; }, true);
            Require(result.LanguageAccepted && result.Allocation.Accepted, "Actual first-word LM cache did not allocate.");
            int wordNode = unchecked((int)BinaryPrimitives.ReadUInt32LittleEndian(candidate.Value.Span));
            Require(model.WordIndex.TryGetWordId(wordNode, out int id), "Actual source70 metadata does not own a terminal.");
            int cls = model.LanguageModel.GetWordClass(id);
            var lexical = result.Language.Record40.Span;
            Require((BinaryPrimitives.ReadUInt32LittleEndian(lexical[12..]) & 0xffffff) == cls &&
                BinaryPrimitives.ReadUInt32LittleEndian(lexical[24..]) == uint.MaxValue &&
                BinaryPrimitives.ReadUInt32LittleEndian(lexical[28..]) == id,
                "Actual first-word vocabulary/history output differs from flags3.");
            Require((result.RestartTag28 & (255UL << 32)) == 0 && (result.RestartTag28 & (255UL << 40)) == (8UL << 40),
                "Actual first-word source50 root/header restart was truncated or source-stamped.");
            completed++;
        }
        Require(completed > 0 && output.Count == completed, "No actual NN terminal reached the native language cache writer.");
        for (int i = 0; i < output.Count; i++)
        {
            var slot = output.GetSlotSnapshot(i).Span;
            Require(slot[64] == 1 && BinaryPrimitives.ReadUInt16LittleEndian(slot[62..]) == ushort.MaxValue &&
                BinaryPrimitives.ReadUInt64LittleEndian(slot[40..]) == (8UL << 40) && slot[32] == 255,
                "1f9080 lost language payload, state, computed restart or continuation span.");
        }
        var previousRows = Enumerable.Range(0, heaps[0].Count).Select(i =>
            LegacyNativeWordRowMaterializer.Materialize(heaps[0].GetSlotSnapshot(i).Span, (ushort)i, (ushort)parents.Length, 1, 1)).ToList();
        for (int i = 0; i < output.Count; i++)
            previousRows.Add(LegacyNativeWordRowMaterializer.Materialize(output.GetSlotSnapshot(i).Span,
                (ushort)previousRows.Count, (ushort)parents.Length, 1, 1));
        int currentNode = Enumerable.Range(0, graph.NodeCount).Last(index => graph.GetNode(index).Any(arc =>
            arc.Span == 0 && graph.Groups[arc.GroupId].Ranges.Span[0].StrokeIndex == 1));
        byte currentSpan = checked((byte)(currentNode - node - 1));
        var currentPass = session.Prepare(currentNode, currentSpan, false);
        var currentCaches = new Dictionary<ushort, LegacyNativeWordAcousticGroupCosts>();
        foreach (var selected in currentPass.PermissionPass.SelectedRows)
        {
            var glyph = evaluator.Classify(graph.Groups[selected.GroupId].Ranges.Span);
            var full = groupScorer.Score(glyph, source.BindCacheProbability(policy, glyph.Geometry), 80, 0, false, false, false);
            Require(selected.Costs.Span.SequenceEqual(full.Cache.Costs.Span), "Current whole physical stroke NN cache lost ownership.");
            currentCaches.Add((ushort)selected.Position.ArcIndex, new(selected.Costs.Span, full.CompositeCategoryCosts.Span));
        }
        var sourceFacts = Enumerable.Range(0, graph.NodeCount).Select(n =>
            (ReadOnlyMemory<LegacyMainWordArcSourceFacts>)Enumerable.Repeat(new LegacyMainWordArcSourceFacts(0, 0, 0, -1, 0, 0, 0),
                graph.GetNode(n).Count).ToArray()).ToArray();
        var geometry = new LegacyMainWordTransitionGeometry(LegacyMainWordTransitionProfile.ReadV106Node(ak, 188157),
            model.AcousticAlphabet, source.Cuts.PreparedInk, graph, sourceFacts, 80, -1,
            LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[4])));
        var groups = LegacyNativeAlternativeWordQueryPreparation.GroupSecondaryRows(
            previousRows.Select(row => row.Record).ToArray(), (ushort)parents.Length, currentSpan, false, 0);
        var currentDictionary = new LegacyPreparedDictionaryQuerySource(model.Lexicon,
            currentPass.PermissionPass.PermissionResult.WorkingMap!, initial);
        var currentSource = new LegacyPreparedAggregateDictionaryQuerySource([LegacyAggregateDictionaryChild.BindDictionary(currentDictionary, contexts)]);
        var currentHeap = new LegacyNativeWordRecordHeap(1024);
        var currentRouter = new LegacyNativeWordHeapRouter([currentHeap, new(512)], new byte[] { 0, 0 }, 0, uint.MaxValue);
        var alternative = new LegacyNativeOrdinaryAlternativeWordCandidates(graph, model.AcousticAlphabet, model.CursiveCategoryProfiles[1],
            contexts, geometry, scorer, new(0x1122334455667788, 0), new byte[80]);
        int alternativeAllocations = 0;
        foreach (var group in groups)
        {
            var queryRequest = new LegacyPreparedDictionaryQuery { Capacity = 128, ContextRoot = contexts.ContainerIdentity };
            ulong restart = LegacyNativeAlternativeWordQueryPreparation.Apply(queryRequest, default, group, (uint)currentNode, currentSpan, 0)!.Value;
            while (true)
            {
                var page = currentSource.Read(restart, queryRequest, clearExtraCosts: true);
                Require(page.ReturnedCount >= 0, "Actual later word query failed.");
                if (page.ReturnedCount == 0) break;
                for (int offset = 0; offset < page.Records.Length; offset += 32)
                {
                    var result = alternative.ProcessQuery(page.Records.Span.Slice(offset, 32), group,
                        previousRows.Select(row => row.Record).ToArray(), (ushort)currentNode, currentSpan, node, 0,
                        currentPass.Qualifier!, new Dictionary<byte, uint> { [1] = descriptor.Flags },
                        (owner, arc) => { Require(owner == currentNode && currentCaches.ContainsKey(arc), "Later NN cache ownership changed."); return currentCaches[arc]; },
                        (owner, ordinal) => { Require(owner == node && ordinal < previousRows.Count, "Later related row lost original-node ownership."); return previousRows[ordinal].Record; },
                        currentRouter, 0, false, false, false, false, 0, true, false, null);
                    Require(result.Succeeded, "Actual later word allocator failed.");
                    alternativeAllocations += result.AcceptedAllocations;
                }
                queryRequest.Skip += page.ReturnedCount;
            }
        }
        Require(alternativeAllocations > 0, "No actual source50/profile2 cache row reached a later native word.");
        var currentRows = Enumerable.Range(0, currentHeap.Count).Select(i =>
            LegacyNativeWordRowMaterializer.Materialize(currentHeap.GetSlotSnapshot(i).Span, (ushort)i, (ushort)currentHeap.Count, 1, 1)).ToArray();
        var operations = geometry.CreateOperations();
        int candidateContexts = 0, matchedContexts = 0, initialContexts = 0;
        for (ushort ordinal = 0; ordinal < currentRows.Length; ordinal++)
        {
            var result = LegacyNativeLanguageContextRecombination.Collect(currentRows[ordinal].Record.Span, ordinal,
                (owner, arc) => { Require(owner == currentNode && arc < currentRows.Length, "Current word anchor lost node/ordinal ownership."); return currentRows[arc]; },
                owner => { Require(owner == node, "Previous language context rows lost anchor-derived node ownership."); return previousRows; },
                id => { Require(id == 1, "Later language source ID lost dictionary ownership."); return descriptor.Flags; },
                model.CursiveCategoryProfiles[1], contexts, new(0, 0, 0, 0),
                new(false, 0, false, false, 0, (ushort)parents.Length, (ushort)previousRows.Count),
                operations, 1001, 1002, new byte[80], 8);
            candidateContexts += result.Candidates16.Length / 16;
            matchedContexts += result.MatchedRows;
            if (result.InitialGeometryAccepted) initialContexts++;
            for (int offset = 0; offset < result.Candidates16.Length; offset += 16)
            {
                var candidate = result.Candidates16.Span.Slice(offset, 16);
                ushort previousOrdinal = BinaryPrimitives.ReadUInt16LittleEndian(candidate[12..]);
                Require(previousOrdinal >= parents.Length && previousOrdinal < previousRows.Count &&
                    BinaryPrimitives.ReadUInt16LittleEndian(candidate[14..]) == ushort.MaxValue &&
                    BinaryPrimitives.ReadInt32LittleEndian(candidate) < 0x4000000,
                    "Actual language context collector lost secondary ordinal/full geometry provenance.");
            }
        }
        Require(initialContexts > 0 && matchedContexts > 0 && candidateContexts > 0,
            "Actual native later-word geometry produced no language context candidates.");
        Console.WriteLine($"ACTUAL {parents.Length} whole-M NN parents -> {completed} profile2 cached words -> {currentRows.Length} native whole-I alternatives -> " +
            $"{matchedContexts} full80 context matches/{candidateContexts} owned16 candidates PASS; frames and source facts remain explicit diagnostic snapshots.");
    }

    private static InkStroke Draw(params (float X, float Y)[] points) => new(points.Select(p => new InkPoint(p.X, p.Y)).ToArray());
    private static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
}
