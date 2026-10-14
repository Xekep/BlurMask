using System.Buffers.Binary;
using InkRecognition;

internal static class ActualQualification
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
        var heaps = new[] { new LegacyNativeWordRecordHeap(4), new LegacyNativeWordRecordHeap(2) };
        var router = new LegacyNativeWordHeapRouter(heaps, new byte[] { 0, 0 }, 0, uint.MaxValue);
        int pages = 0, queried = 0, qualified = 0, acousticCalls = 0, accepted = 0;
        while (true)
        {
            var page = aggregate.Read(tag, query, clearExtraCosts: true);
            if (page.ReturnedCount == 0) break;
            Require(page.ReturnedCount > 0, "Actual prepared-source query failed.");
            pages++;
            for (int i = 0; i < page.ReturnedCount; i++)
            {
                var row = page.Records.Span.Slice(i * 32, 32);
                ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(row[20..]);
                if (symbol >= model.AcousticAlphabet.LabelCount) continue;
                queried++;
                ulong context = LegacyNativeWordStartupRequestFactory.GraftOrdinaryInitialContext(contexts, BinaryPrimitives.ReadUInt64LittleEndian(row[8..]));
                if (!qualifier.Qualifies(symbol, row[22], context, contexts)) continue;
                qualified++;
                for (int variant = 0; variant < 2; variant++)
                {
                    int category = categories.GetSymbolCompositeCategory(symbol, endBoundary: variant != 0);
                    if (category < 0) break;
                    foreach (int ordinal in roots)
                    {
                        ushort arc = checked((ushort)ordinal);
                        var cell = new LegacyNativeWordTransitionCostCell(0x1122334455667788, 0);
                        var request = LegacyNativeWordStartupRequestFactory.Create((uint)node, (uint)node, arc, symbol, category,
                            BinaryPrimitives.ReadUInt64LittleEndian(row), row[22], row[23], descriptor.Flags, 0,
                            model.CursiveCategoryProfiles[1], cell, BinaryPrimitives.ReadInt32LittleEndian(row[16..]), new byte[80]);
                        acousticCalls++;
                        if (!scorer.Score(request, (owner, index) =>
                        {
                            Require(owner == node && index == arc, "Actual startup lost selected cache ownership.");
                            return caches[index];
                        }, false)) continue;
                        var fields = new LegacyInitialWordRecordFields(BinaryPrimitives.ReadUInt64LittleEndian(row), context,
                            cell.Value, request.ReadInt32(48), symbol, (ushort)node, arc, arc, (byte)category,
                            ushort.MaxValue, (sbyte)node, 0, 0, 0, 1, (byte)(row[23] & 7), row[22], 0, 0, -1, 0, node, ushort.MaxValue);
                        router.TryInsertInitialRecord(0, false, fields, 1, false);
                        accepted++;
                    }
                }
            }
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
            "Guide/cache policy, main source frame, full writer fields and scheduler remain explicitly diagnostic; no native winner or acceptance confidence claim.");
    }

    private static InkStroke Draw(params (float X, float Y)[] points) => new(points.Select(p => new InkPoint(p.X, p.Y)).ToArray());
    private static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
}
