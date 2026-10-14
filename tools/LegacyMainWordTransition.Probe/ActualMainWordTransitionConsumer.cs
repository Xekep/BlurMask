using InkRecognition;
using System.Buffers.Binary;

internal static class ActualMainWordTransitionConsumer
{
    private static void Require(bool value, string message)
    {
        if (!value) throw new Exception(message);
    }
    private static int I32(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadInt32LittleEndian(bytes[offset..]);
    private static ushort U16(ReadOnlySpan<byte> bytes, int offset) => BinaryPrimitives.ReadUInt16LittleEndian(bytes[offset..]);
    private static bool Plain(LegacyInkAlphabet alphabet, ushort label)
        => alphabet.TryGetText(label, out string text) && text.Length == 1 &&
            (text[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '\u0400' and <= '\u04ff' or >= '0' and <= '9');
    private static ushort[] ReadPath(LegacySymbolContextFactory contexts, ulong identity)
    {
        var result = new List<ushort>();
        while (identity != 0)
        {
            var node = contexts.ReadNode(identity);
            result.Add(node.Symbol);
            identity = node.ParentLink;
        }
        result.Reverse();
        return result.ToArray();
    }
    public static void Run(string[] args)
    {
        // Actual RU classifier/cache costs flow through both pool producers.
        var acoustic = File.ReadAllBytes(args[0]);
        var model = LegacyRussianModelBundle.Load(acoustic, File.ReadAllBytes(args[1]), File.ReadAllBytes(args[2]));
        var sourceProfile = LegacyOrdinarySourceInk.ReadV10dResourceProfile(acoustic);
        InkStroke Stroke(params (float X, float Y)[] points) =>
            new(points.Select(point => new InkPoint(point.X, point.Y)).ToArray());
        var raw = new[]
        {
            Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),
            Stroke((70,10),(70,90),(120,10),(120,90)),
            Stroke((140,90),(140,10),(180,10),(190,20),(190,40),(180,50),(140,50))
        };
        var source = LegacyOrdinaryCursiveCutSource.Generate(raw, sourceProfile, 90, 80);
        var seedProfile = LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(acoustic, 189379);
        var seeds = LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(source.Cuts, 80, seedProfile,
            new byte[raw.Length], Enumerable.Repeat(-1, raw.Length).ToArray());
        var seedGraph = LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds);
        var graph = LegacyOrdinaryCursiveCandidateGraph.Generate(seeds, seedGraph, model.CursiveSpanProfile.MaximumAtoms);
        int endingNode = 0;
        int arcIndex = Enumerable.Range(0, graph.GetNode(endingNode).Count)
            .First(index => graph.GetNode(endingNode)[index].Span == endingNode);
        var candidate = graph.GetNode(endingNode)[arcIndex];
        var evaluator = new LegacyPreparedWordGroupEvaluator(model, source, new(90, 80, 0, 1));
        var glyph = evaluator.Classify(graph.Groups[candidate.GroupId].Ranges.Span);
        var cache = new LegacyOrdinaryAcousticClassCache(
            LegacyOrdinaryAcousticCacheProfile.ReadV110Node(acoustic.AsSpan(189734)),
            model.AcousticAlphabet, acoustic.AsSpan(99, 16));
        var nativeScorer = LegacyPreparedAcousticCandidateScorer.ForAlphabet(
            LegacyAcousticCandidateThresholds.ReadV110Node(acoustic.AsSpan(189734), out _), model.AcousticAlphabet);
        var categories = model.CreateCursiveCategoryMapper(1);
        var bridge = new LegacyCursiveFinalCacheGroupScorer(cache, nativeScorer, categories,
            LegacyCursiveSeparationProfile.ReadV106Node(acoustic, 188157),
            LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3])),
            model.AcousticAlphabet, model.LexicalAlphabet,
            Enumerable.Repeat((byte)1, model.AcousticAlphabet.LabelCount).ToArray(),
            LegacyWideSymbolPairs.CreateUnrestricted(model.AcousticAlphabet), relatedFormMode: 0);
        var explicitScope = new LegacyOrdinaryAcousticCacheContext(true, true, true, false, false,
            null, false, ReadOnlyMemory<ushort>.Empty);
        var scored = bridge.Score(glyph, source.BindCacheProbability(explicitScope, glyph.Geometry),
            80, 0, false, false, false);
        var actualHeaps = new[] { new LegacyNativeWordRecordHeap(4), new LegacyNativeWordRecordHeap(2) };
        var actualRouter = new LegacyNativeWordHeapRouter(actualHeaps, new byte[] { 0 }, 0, uint.MaxValue);
        var sourceBoundaries = LegacyPreparedSourceBoundarySnapshot.CreateFreshOrdinary(source, graph);
        var expectedCosts = new List<int>();
        var actualContexts = new LegacySymbolContextFactory();
        int initializerCount = 0, actualPromotionAttempts = 0, actualPromotions = 0, primaryFallbacks = 0;
        LegacyInitialWordRecordFields? firstFields = null;

        for (ushort symbol = 0; symbol < model.AcousticAlphabet.LabelCount; symbol++)
        {
            int category = categories.GetSymbolCompositeCategory(symbol);
            if (category < 0)
                continue;
            var score = nativeScorer.Evaluate(scored.Cache.Costs.Span, symbol,
                scored.CompositeCategoryCosts.Span, category, 0, 0, LegacyAcousticFixedLog.ImpossibleCost);
            if (!score.Accepted || !Plain(model.AcousticAlphabet, symbol))
                continue;
            var fields = new LegacyInitialWordRecordFields(
                SourceTag: 0, Auxiliary08: actualContexts.ResolveInitial(symbol).Identity, CandidateCost10: score.TotalCost, IncrementalCost14: score.TotalCost,
                Label24: symbol, SourceNode18: (ushort)endingNode, PackedSourceArcIndex: (ushort)arcIndex,
                SourceArcIndex1c: (ushort)arcIndex, CompositeCategory: (byte)category,
                PreviousArcIndex1e: ushort.MaxValue, SourceSpan: (sbyte)candidate.Span,
                Count26: 0, HeaderFlag5: 0, HeaderFlag6: 0, RequestedSecondaryFlag: 1,
                PackedKind27: 0, SmallState21: 0, StateBit6: 0, StateBit7: 0,
                Auxiliary30: 0, Auxiliary34: 0, AnchorEndingNode38: endingNode, AnchorArcIndex3c: ushort.MaxValue);
            firstFields ??= fields;
            expectedCosts.Add(score.TotalCost);
            var allocation = actualRouter.TryInsertInitialRecord(0, false, fields, 1, false);
            initializerCount += allocation.Accepted ? 1 : 0;
            actualPromotionAttempts += allocation.PromotionAttempted ? 1 : 0;
            actualPromotions += allocation.PromotionAccepted ? 1 : 0;
            primaryFallbacks += allocation.HeapIndex == 0 ? 1 : 0;
        }



        var initialStorage = actualHeaps.Select(heap => heap.GetSlotStorageSnapshot().ToArray()).ToArray();
        var initialCounts = actualHeaps.Select(heap => heap.Count).ToArray();
        // One diagnostic duplicate in bucket0 proves the new root pools can feed the
        // native initial merge before their stored records become real parent inputs.
        initialStorage[0] = initialStorage[0].Concat(new byte[128]).ToArray();
        initialStorage[0].AsSpan(0, 128).CopyTo(initialStorage[0].AsSpan(initialCounts[0] * 128));
        BinaryPrimitives.WriteInt32LittleEndian(initialStorage[0].AsSpan(initialCounts[0] * 128 + 0x10),
            BinaryPrimitives.ReadInt32LittleEndian(initialStorage[0].AsSpan(0x10)) + 100);
        initialCounts[0]++;
        uint nextInitial = 0;
        for (int heap = 0; heap < initialStorage.Length; heap++)
        {
            var assigned = LegacyNativeWordAlternativeOrdinals.AssignInitial(initialStorage[heap], initialCounts[heap], nextInitial);
            initialStorage[heap] = assigned.SlotStorage.ToArray();
            nextInitial = assigned.NextOrdinal;
        }
        byte[] initialPrimary = Enumerable.Repeat((byte)0xa7, (int)nextInitial * 80).ToArray();
        byte[] initialSecondary = Enumerable.Repeat((byte)0x5b, (int)nextInitial * 80).ToArray();
        var actualConfiguration = LegacyNativeWordControllerConfiguration.CreateFresh20baa0();
        for (int heap = 0; heap < initialStorage.Length; heap++)
        {
            var produced = LegacyNativeRootWordAlternativeProducer.Create(initialStorage[heap], initialCounts[heap],
                (uint)endingNode, actualConfiguration, actualContexts, key => key, initialPrimary, initialSecondary);
            initialPrimary = produced.PrimaryStorage.ToArray();
            initialSecondary = produced.SecondaryStorage.ToArray();
        }
        var initialPrimaryPool = new LegacyNativeWordAlternativeBuffer(2, initialPrimary);
        var initialSecondaryPool = new LegacyNativeWordAlternativeBuffer(2, initialSecondary);
        var initialOrdered = initialStorage.Select((storage, heap) =>
            LegacyNativeWordAlternativeOrdering.Prepare(storage, initialCounts[heap], false, actualConfiguration,
                new uint[] { 0 }, false, initialPrimaryPool, initialSecondaryPool)).ToArray();
        int rootCount = initialOrdered.Sum(result => result.Heap.Count);
        actualConfiguration = actualConfiguration with { PrimaryArcLimit230a = (ushort)rootCount };
        var parentRows = new List<LegacyNativeStoredWordRecord>();
        foreach (var ordered in initialOrdered)
            for (int index = 0; index < ordered.Heap.Count; index++)
                parentRows.Add(LegacyNativeWordRowMaterializer.Materialize(ordered.Heap.GetSlotSnapshot(index).Span,
                    (ushort)parentRows.Count, actualConfiguration.PrimaryArcLimit230a, 2, 2,
                    initialPrimaryPool.GetSnapshot().Span, initialSecondaryPool.GetSnapshot().Span));
        Require(parentRows.Count > 1, "Actual root pools/initial duplicate merge lost expected parents.");
        int parentLookups = 0;
        LegacyNativeStoredWordRecord Parent(int node, ushort arc)
        {
            Require(node == endingNode && arc < parentRows.Count, "Continuation queried fabricated node/arc.");
            parentLookups++;
            return parentRows[arc];
        }
        ushort nextArc = (ushort)Enumerable.Range(0, graph.GetNode(1).Count).First(i => graph.GetNode(1)[i].Span == 0 && graph.GetNode(1)[i].PredecessorArcIndices.Span.Contains(arcIndex));
        var nextGlyph = evaluator.Classify(graph.Groups[graph.GetNode(1)[nextArc].GroupId].Ranges.Span);
        var nextScored = bridge.Score(nextGlyph, source.BindCacheProbability(explicitScope, nextGlyph.Geometry),
            80, 0, false, false, false);
        var increments = new List<(ushort Symbol, int Cost)>();
        for (ushort symbol = 0; symbol < model.AcousticAlphabet.LabelCount; symbol++)
        {
            int category = categories.GetSymbolCompositeCategory(symbol);
            if (category < 0) continue;
            var score = nativeScorer.Evaluate(nextScored.Cache.Costs.Span, symbol,
                nextScored.CompositeCategoryCosts.Span, category, 0, 0, LegacyAcousticFixedLog.ImpossibleCost);
            if (score.Accepted && Plain(model.AcousticAlphabet, symbol)) increments.Add((symbol, score.TotalCost));
        }

        uint currentNode = (uint)(endingNode + 1);
        Require(increments.Count > 0, "Actual main NN returned no scoped plain increments.");
        // Actual source graph connections and actual NN/cache costs, with diagnostic
        // prepared mode1 and full-packet118/140 callbacks. These callbacks deliberately
        // supply explicit cost changes; they do not claim recovered geometry policy.
        var connections = new LegacyOrdinaryCandidateConnections(graph);
        var higherRows = new List<LegacyNativeStoredWordRecord>(parentRows);
        var continuedSlots = new List<byte[]>();
        for (ushort parentArc = 0; parentArc < parentRows.Count; parentArc++)
        {
            byte[] slot = new byte[128];
            int parentCost = I32(parentRows[parentArc].Record.Span, 16);
            var continuationFields = new LegacyContinuationWordRecordFields(0,
                unchecked(parentCost + increments[0].Cost), increments[0].Cost, ushort.MaxValue,
                new uint[5], 0, 0, 0, 1, 0);
            LegacyContinuationWordRecordProducer.InitializeAllocatedSlot(slot, parentRows[parentArc].Record.Span,
                parentArc, continuationFields, 1, true);
            continuedSlots.Add(slot);
        }
        byte[] continuedStorage = continuedSlots.SelectMany(value => value).ToArray();
        var continuedOrdinals = LegacyNativeWordAlternativeOrdinals.AssignContinuation(continuedStorage, continuedSlots.Count, 0);
        continuedStorage = continuedOrdinals.SlotStorage.ToArray();
        byte[] continuedSecondary = Enumerable.Repeat((byte)0x6b, continuedSlots.Count * 80).ToArray();
        var continuedAlternatives = LegacyNativeContinuationWordAlternativeProducer.CreateUnanchored(continuedStorage,
            continuedSlots.Count, endingNode, 0, 0, 0, actualConfiguration, Parent, actualContexts, key => key, continuedSecondary);
        Require(continuedAlternatives.Succeeded, "Actual higher-arc continuation pool producer failed.");
        for (ushort index = 0; index < continuedSlots.Count; index++)
        {
            ushort destination = (ushort)(parentRows.Count + index);
            var row = LegacyNativeWordRowMaterializer.Materialize(continuedStorage.AsSpan(index * 128, 128), destination,
                actualConfiguration.PrimaryArcLimit230a, 2, 2, default, continuedAlternatives.SecondaryStorage.Span);
            Require(row.PrimaryAlternatives.IsEmpty && row.SecondaryAlternatives.Length == 80,
                "Actual stored higher row incorrectly retained a primary pool.");
            higherRows.Add(row);
        }
        byte[] lk = File.ReadAllBytes(args[1]);
        var dictionaryPrefix = LegacyPreparedDictionarySourceDescriptorFactory.ReadV300Prefix(lk.AsSpan(606964, 4565174 - 606964));
        uint languageOrder = LegacyPreparedDictionarySourceDescriptorFactory.ReadLanguageOrder(lk.AsSpan(2513123, 4565174 - 2513123));
        var dictionaryDescriptor = LegacyPreparedDictionarySourceDescriptorFactory.CreateFresh(dictionaryPrefix, model.Lexicon, languageOrder, true);
        int actualCallbacks = 0, actualQueries = 0, actualMerges = 0;
        var mainProfile = LegacyMainWordTransitionProfile.ReadV106Node(acoustic, 188157);
        var sourceFacts = Enumerable.Range(0, graph.NodeCount).Select(node => (ReadOnlyMemory<LegacyMainWordArcSourceFacts>)
            Enumerable.Repeat(new LegacyMainWordArcSourceFacts(0, 0, 0x10, -1, 0, 0, 0), graph.GetNode(node).Count).ToArray()).ToArray();
        var mainGeometry = new LegacyMainWordTransitionGeometry(mainProfile, model.AcousticAlphabet,
            source.Cuts.PreparedInk, graph, sourceFacts, 80, -1, LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3])));
        var actualOperations = LegacyNativeHigherArcTransitionOperations.ForOrdinaryConnections(connections,
            (request, token) => { actualCallbacks++; return mainGeometry.Calculate118(request, token); },
            (request, token) => { actualCallbacks++; return mainGeometry.Validate140(request, token); });
        var trackedOperations = actualOperations with
        {
            SuccessorsF0F8 = (request, token) => { actualQueries++; return actualOperations.SuccessorsF0F8(request, token); }
        };
        for (ushort original = (ushort)parentRows.Count; original < higherRows.Count; original++)
        {
            var parent = higherRows[original];
            var increment = increments[0];
            var fields = new LegacyInitialWordRecordFields(0, actualContexts.ResolveInitial(increment.Symbol).Identity,
                unchecked(I32(parent.Record.Span, 16) + increment.Cost), increment.Cost,
                increment.Symbol, (ushort)currentNode, nextArc, nextArc, (byte)categories.GetSymbolCompositeCategory(increment.Symbol),
                original, 0, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, endingNode, original);
            byte[] fullSlot = new byte[128];
            LegacyInitialWordRecordProducer.InitializeAllocatedPrefix(fields, 1).CopyTo(fullSlot, 0);
            var combined = LegacyNativeHigherArcRecombination.Create(fullSlot, currentNode,
                actualConfiguration with { Mode22f6 = 1 }, higherRows,
                (ownerNode, arc) => { Require(ownerNode == endingNode, "Actual request fabricated an owner node."); return higherRows[arc]; },
                _ => dictionaryDescriptor.Flags, model.CursiveCategoryProfiles[1], actualContexts, key => key,
                new(ushort.MaxValue, int.MaxValue, false, 0, 0), new(65527, 65531, 65532, ushort.MaxValue),
                trackedOperations, 0x1122334455667788, Enumerable.Repeat((byte)0x9d, 80).ToArray(), new byte[80], new byte[80]);
            Require(combined.InitialAccepted && combined.MergedCandidates == parentRows.Count - 1,
                "Actual source graph successors did not reach every compatible higher row.");
            actualMerges += combined.MergedCandidates;
            for (int alternative = 0; alternative < 2; alternative++)
            {
                var choice = combined.SecondaryStorage.Span.Slice(alternative * 40, 40);
                if (I32(choice, 0) >= 0x4000000) continue;
                var prefix = higherRows[U16(choice, 24)].SecondaryAlternatives.Span.Slice((choice[27] & 127) * 40, 40);
                Require(ReadPath(actualContexts, BinaryPrimitives.ReadUInt64LittleEndian(choice[8..])).SequenceEqual(
                    ReadPath(actualContexts, BinaryPrimitives.ReadUInt64LittleEndian(prefix[8..])).Concat(new[] { increment.Symbol })),
                    "Final higher-arc refresh lost native chosen parent/suffix lineage.");
            }
            var finalOrdinal = LegacyNativeWordAlternativeOrdinals.AssignInitial(combined.HeapSlot.Span, 1, 0);
            var stored = LegacyNativeWordRowMaterializer.Materialize(finalOrdinal.SlotStorage.Span, 0, 1, 2, 2,
                combined.PrimaryStorage.Span, combined.SecondaryStorage.Span);
            Require(stored.SecondaryAlternatives.Span.SequenceEqual(combined.SecondaryStorage.Span) &&
                stored.Record.Span.SequenceEqual(combined.HeapSlot.Span[..88]), "Full higher-arc result did not feed native materialization intact.");
        }
        Console.WriteLine($"ACTUAL RU source graph/NN/cache -> {parentRows.Count} real continuation higher rows -> " +
            $"{actualQueries} native connection queries / {actualCallbacks} native full80 callbacks / {actualMerges} merges -> materialization PASS. " +
            "Actual main118/140 costs used; mode1/frame/gap/source facts are explicit diagnostics, complete source scheduling/winner remain unfinished.");
    }
}
