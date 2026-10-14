using System.Buffers.Binary;
using InkRecognition;

internal static class ActualMainWordLanguage
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

        var metadata = new LegacyPreparedDictionaryWordMetadata(model.Lexicon, model.WordIndex);
        var recordProducer = new LegacyNativeKnownWordLanguageRecord(model.LanguageModel,
            model.LanguageProfiles[model.ResourceLanguageProfileIndex], model.AuxiliaryProfiles[model.ResourceLanguageProfileIndex], prefix.RuntimeLanguageScale);
        var completion = new LegacyNativeFirstDictionaryWordCompletion(model,
            metadata, recordProducer, LegacyPreparedDictionaryRestartTags.CreateFresh(), 1, descriptor.Flags, false, 0, false, false);
        var output = new LegacyNativeWordRecordHeap(256);
        var completionRouter = new LegacyNativeWordHeapRouter([output, new(128)], new byte[] { 0, 0 }, 0, uint.MaxValue);
        int completed = 0;
        for (ushort ordinal = 0; ordinal < parents.Length; ordinal++)
        {
            var candidate = LegacyNativeFirstDictionaryWordCompletion.CreateAlreadyTerminalQuery(parents[ordinal], new byte[32]);
            if (!candidate.HasValue) continue;
            var result = completion.ProcessCandidate(candidate.Value.Span, 0, ordinal, parents[ordinal], completionRouter,
                false, Enumerable.Repeat((byte)0xa7, 40).ToArray(),
                (owner, arc) => { Require(owner == 0 && arc < parents.Length, "Actual LM anchor ordinal lost row ownership."); return parents[arc]; }, true);
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
        Console.WriteLine($"ACTUAL {parents.Length} NN-native parent rows -> {completed} current-terminal injections -> source70/profile{model.ResourceLanguageProfileIndex} " +
            $"100090/0ffec0 ->1f9080 language-cache rows PASS. Source frame remains explicit; full204290 later-word/boundary scheduling and winner are unfinished.");

        int guards = 0;
        void Reject(Action action)
        {
            try { action(); }
            catch (Exception error) when (error is ArgumentException or InvalidDataException or InvalidOperationException or NotSupportedException or OperationCanceledException or OverflowException)
            { guards++; return; }
            throw new InvalidDataException("Invalid first-word ownership/lifecycle accepted.");
        }
        LegacyNativeFirstDictionaryWordCompletion NewGuard() => new(model, metadata,
            recordProducer, LegacyPreparedDictionaryRestartTags.CreateFresh(), 1, descriptor.Flags, false, 0, false, false);
        Require(metadata.IsForModel(model) && recordProducer.IsForModel(model),
            "Owned selected dictionary and resource language profiles lost identity.");
        LegacyNativeFirstDictionaryWordCompletion Bind(LegacyPreparedDictionaryWordMetadata wordMetadata,
            LegacyNativeKnownWordLanguageRecord wordLanguage) => new(model, wordMetadata, wordLanguage,
                LegacyPreparedDictionaryRestartTags.CreateFresh(), 1, descriptor.Flags, false, 0, false, false);
        var foreignDictionary = LegacyPackedLexicon.Load(lk.AsSpan(609980, 2513123 - 609980));
        var foreignMetadata = new LegacyPreparedDictionaryWordMetadata(foreignDictionary, new(foreignDictionary));
        Require(foreignMetadata.WordCount == metadata.WordCount && !foreignMetadata.IsForModel(model),
            "Same-sized foreign dictionary did not exercise the identity guard.");
        Reject(() => Bind(foreignMetadata, recordProducer));
        var foreignIndex = new LegacyPreparedDictionaryWordMetadata(model.Lexicon, new(model.Lexicon));
        Require(foreignIndex.WordCount == metadata.WordCount && !foreignIndex.IsForModel(model),
            "Same-dictionary separately owned index did not exercise the identity guard.");
        Reject(() => Bind(foreignIndex, recordProducer));
        var foreignLanguage = LegacyWordLanguageModel.Load(lk.AsSpan(2513123, 4565174 - 2513123));
        var foreignModelRecord = new LegacyNativeKnownWordLanguageRecord(foreignLanguage,
            model.LanguageProfiles[model.ResourceLanguageProfileIndex], model.AuxiliaryProfiles[model.ResourceLanguageProfileIndex],
            prefix.RuntimeLanguageScale);
        Require(foreignModelRecord.WordCount == recordProducer.WordCount && !foreignModelRecord.IsForModel(model),
            "Same-sized foreign language did not exercise the identity guard.");
        Reject(() => Bind(metadata, foreignModelRecord));
        var selectedProfile = model.LanguageProfiles[model.ResourceLanguageProfileIndex];
        var copiedProfile = new LegacyWordLanguageCostProfile(selectedProfile.WordSlope, selectedProfile.WordOffset,
            selectedProfile.UnigramSlope, selectedProfile.UnigramOffset, selectedProfile.BigramSlope, selectedProfile.BigramOffset);
        var foreignProfileRecord = new LegacyNativeKnownWordLanguageRecord(model.LanguageModel, copiedProfile,
            model.AuxiliaryProfiles[model.ResourceLanguageProfileIndex], prefix.RuntimeLanguageScale);
        Require(!foreignProfileRecord.IsForModel(model), "Equal foreign cost coefficients passed selected-profile identity.");
        Reject(() => Bind(metadata, foreignProfileRecord));
        var copiedAuxiliary = LegacyWordAuxiliaryProfile.LoadProfiles(lk.AsSpan(2513123, 4565174 - 2513123));
        var foreignAuxiliaryRecord = new LegacyNativeKnownWordLanguageRecord(model.LanguageModel, selectedProfile,
            copiedAuxiliary[model.ResourceLanguageProfileIndex], prefix.RuntimeLanguageScale);
        Require(!foreignAuxiliaryRecord.IsForModel(model), "Equal foreign auxiliary data passed selected-profile identity.");
        Reject(() => Bind(metadata, foreignAuxiliaryRecord));
        Reject(() => new LegacyNativeFirstDictionaryWordCompletion(null!, metadata, recordProducer,
            LegacyPreparedDictionaryRestartTags.CreateFresh(), 1, descriptor.Flags, false, 0, false, false));
        Reject(() => Bind(null!, recordProducer));
        Reject(() => Bind(metadata, null!));
        ushort selectedOrdinal = Enumerable.Range(0, parents.Length).Select(i => (ushort)i).First(i =>
            LegacyNativeFirstDictionaryWordCompletion.CreateAlreadyTerminalQuery(parents[i], new byte[32]).HasValue);
        byte[] selectedQuery = LegacyNativeFirstDictionaryWordCompletion.CreateAlreadyTerminalQuery(parents[selectedOrdinal], new byte[32])!.Value.ToArray();
        var emptyOwner = new LegacyNativeWordRecordHeap(64);
        var selectedRouter = new LegacyNativeWordHeapRouter([emptyOwner, new(64)], new byte[] { 0, 0 }, 0, uint.MaxValue);
        LegacyNativeFirstDictionaryWordCompletionResult RunGuard(LegacyNativeFirstDictionaryWordCompletion target,
            byte[]? queryBytes = null, byte[]? parentBytes = null, LegacyNativeWordHeapRouter? targetRouter = null,
            Func<int, ushort, ReadOnlyMemory<byte>>? lookup = null, CancellationToken token = default, byte[]? residue = null)
            => target.ProcessCandidate(queryBytes ?? selectedQuery, 0, selectedOrdinal, parentBytes ?? parents[selectedOrdinal],
                targetRouter ?? selectedRouter, false, residue ?? new byte[40], lookup ?? ((owner, ordinal) => parents[ordinal]), true, token);
        Reject(() => RunGuard(NewGuard(), queryBytes: new byte[31]));
        Reject(() => RunGuard(NewGuard(), parentBytes: new byte[87]));
        Reject(() => RunGuard(NewGuard(), lookup: (_, _) => new byte[87]));
        var foreignSource = selectedQuery.ToArray();
        foreignSource[4] = 2;
        Reject(() => RunGuard(NewGuard(), queryBytes: foreignSource));
        var unmaterialized = parents[selectedOrdinal].ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(unmaterialized.AsSpan(60), ushort.MaxValue);
        Reject(() => RunGuard(NewGuard(), parentBytes: unmaterialized));
        var foreignAnchor = parents[selectedOrdinal].ToArray();
        foreignAnchor[4] = 2;
        Reject(() => RunGuard(NewGuard(), lookup: (_, _) => foreignAnchor));
        Reject(() => metadata.ReadTerminal(ulong.MaxValue, 0));
        foreach (var flags in new[] { (Mode: true, Ec: 0f, Frontend: false, Trace: false),
            (Mode: false, Ec: 1f, Frontend: false, Trace: false), (Mode: false, Ec: 0f, Frontend: true, Trace: false),
            (Mode: false, Ec: 0f, Frontend: false, Trace: true) })
            Reject(() => new LegacyNativeFirstDictionaryWordCompletion(model, metadata, recordProducer,
                LegacyPreparedDictionaryRestartTags.CreateFresh(), 1, descriptor.Flags, flags.Mode, flags.Ec, flags.Frontend, flags.Trace));
        using (var canceled = new CancellationTokenSource())
        {
            var target = NewGuard();
            Reject(() => RunGuard(target, lookup: (_, ordinal) => { canceled.Cancel(); return parents[ordinal]; }, token: canceled.Token));
            Reject(() => RunGuard(target));
        }
        byte[] mutableQuery = selectedQuery.ToArray(), mutableParent = parents[selectedOrdinal].ToArray();
        var snapshotted = RunGuard(NewGuard(), mutableQuery, mutableParent, lookup: (_, ordinal) =>
        { Array.Clear(mutableQuery); Array.Clear(mutableParent); return parents[ordinal]; });
        Require(snapshotted.LanguageAccepted && snapshotted.Allocation.Accepted, "First-word inputs changed during anchor callback.");
        guards++;
        byte[] mutableResidue = Enumerable.Repeat((byte)0xab, 40).ToArray();
        var residueResult = RunGuard(NewGuard(), residue: mutableResidue, lookup: (_, ordinal) =>
        { Array.Clear(mutableResidue); return parents[ordinal]; });
        Require(residueResult.LanguageAccepted && residueResult.Language.Record40.Span[9] == 0xab &&
            residueResult.Language.Record40.Span[32] == 0xab, "Caller lexical heap residue changed during anchor callback.");
        guards++;

        //A foreign full heap would reject this word; budget is derived from the
        //router's actual primary owner and has no duplicate caller-heap argument.
        var foreignBudget = new LegacyNativeWordRecordHeap(1);
        foreignBudget.TryAllocate(int.MinValue, slot => BinaryPrimitives.WriteInt32LittleEndian(slot[16..], int.MinValue));
        Require(ReferenceEquals(selectedRouter.GetPrimaryHeap(1), emptyOwner) &&
            !ReferenceEquals(selectedRouter.GetPrimaryHeap(1), foreignBudget), "Primary budget accessor returned foreign storage.");
        var ownBudgetResult = RunGuard(NewGuard());
        Require(ownBudgetResult.LanguageAccepted && ownBudgetResult.Allocation.Accepted,
            "Unrelated full heap contaminated the native primary-owner budget.");
        var tight = new LegacyNativeWordRecordHeap(1);
        int rejectedLimit = checked(ownBudgetResult.PreLexicalCost + ownBudgetResult.Language.Score.LexicalSubtotal - 1);
        tight.TryAllocate(rejectedLimit, slot => BinaryPrimitives.WriteInt32LittleEndian(slot[16..], rejectedLimit));
        var tightRouter = new LegacyNativeWordHeapRouter([tight, new(1)], new byte[] { 0, 0 }, 0, uint.MaxValue);
        Require(!RunGuard(NewGuard(), targetRouter: tightRouter).LanguageAccepted,
            "Actual full primary heap's stricter budget was ignored.");
        guards += 2;
        Reject(() => selectedRouter.GetPrimaryHeap(2));
        var invalidBucket = new LegacyNativeWordHeapRouter([new(1)], new byte[] { 0, 1 }, 0, uint.MaxValue);
        Reject(() => invalidBucket.GetPrimaryHeap(1));
        Console.WriteLine($"PASS {guards} first-word callback cancellation, source/anchor ownership, same-router budget, input snapshots and unsupported-scope guards.");
    }

    private static InkStroke Draw(params (float X, float Y)[] points) => new(points.Select(p => new InkPoint(p.X, p.Y)).ToArray());
    private static void Require(bool value, string message) { if (!value) throw new InvalidDataException(message); }
}
