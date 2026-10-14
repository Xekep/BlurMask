using System.Buffers.Binary;
using InkRecognition;

internal static class ActualStartup
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
        int node = graph.NodeCount - 1;
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
        Require(queried > 0 && qualified > 0 && accepted > 0 && stored > 0, "Actual qualifier/query/cache/writer pipeline emitted no native rows.");
        Console.WriteLine($"ACTUAL RU {prepared.PermissionPass.SelectedRows.Count} native selected NN/cache rows ({computed} initial computations), " +
            $"{pages} permission-filtered query pages / {queried} query labels / {qualified} frontend110 qualifiers / {acousticCalls} exact128 calls / {accepted} accepted / {stored} materialized heap rows PASS. " +
            "Guide/cache policy, source headers and main source frame remain explicit; writer fields now come from the checked ordinary startup branch; scheduler unfinished; no native winner or acceptance confidence claim.");
    }

    private static InkStroke Draw(params (float X, float Y)[] points) => new(points.Select(p => new InkPoint(p.X, p.Y)).ToArray());
    private static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
}
