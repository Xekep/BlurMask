using System.Buffers.Binary;
using InkRecognition;

//Original procedural transcription of the checked2144F0 DIRECT control flow.
//It does not call LegacyOwnedMainDirectWordPass, own its descriptor/reset
//implementation, or reuse its row results. Lower native components have their
//own independent scalar/full-record probes. Allocator residue is explicitly0.
internal static class OriginalDirectSchedulerOracle
{
    internal static IReadOnlyList<LegacyMainDirectWordNodeResult> Run(byte[] ak,
        LegacyOwnedMainWordSpanPreparation owner, LegacyCursiveSeparationTable table, bool lineSource = false)
    {
        const int imp = 0x4000000;
        var policy = LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak, owner);
        var cfg = policy.Configuration;
        var graph = owner.SourceGraph;
        uint[] headers = owner.DictionaryOwner.CopySourceHeaders();
        var headerMap = headers.Select((value, index) => (value, index))
            .ToDictionary(item => (byte)item.index, item => item.value);
        var heapPlan = LegacyNativeWordHeapPreparation.PrepareFresh(
            [new(0, 0, 1), LegacyNativeWordPreparedSource.FromDictionary(owner.DictionaryOwner.Descriptor)], cfg, 0);
        var buffers = new Dictionary<int, LegacyOwnedFreshMainWordNodeBuffer>();
        var output = new List<LegacyMainDirectWordNodeResult>();
        var geometry = lineSource ? owner.MainLineSource.CreatePrepared240TransitionGeometry(table) :
            owner.Source.CreatePrepared240TransitionGeometry(table);
        var cell = new LegacyNativeWordTransitionCostCell(1, 0);
        var acoustic = LegacyNativeWordAcousticRequestScorer.ForAlphabet(
            LegacyAcousticCandidateThresholds.ReadV110Node(ak.AsSpan(189734), out _), owner.DictionaryOwner.Model.AcousticAlphabet);
        var startup = new LegacyNativeMainInitialSpecialCandidates(ak, owner, cell, new byte[80]);
        byte[] flagRequest = new byte[80]; W32(flagRequest, 52, imp);
        var flags = lineSource ? new LegacyNativeMainWordFlagPrepass(ak, owner.MainLineAcoustic, table, cell, flagRequest) :
            new LegacyNativeMainWordFlagPrepass(ak, owner.Acoustic, table, cell, flagRequest);
        var gate = new LegacyNativeWordLanguageGate(owner.DictionaryOwner.Model, graph);
        byte[] pool = new byte[heapPlan.Limits[0].PrimaryInitialCapacity20 * 80];
        LegacyNativeWordHeapRouter Router(LegacyNativeWordRecordHeap heap)
            => new([heap], heapPlan.CopySourceBuckets().Span, -1, uint.MaxValue);
        int[] offsets = new int[graph.NodeCount], cumulative = new int[graph.NodeCount];
        LegacyNativeStoredWordRecord Stored(int node, ushort ordinal)
        {
            var stored = buffers[node].Read(ordinal);
            if (offsets[node] == 0 || I32(stored.Record.Span, 16) >= imp) return stored;
            byte[] slot = new byte[128]; stored.Record.Span.CopyTo(slot);
            W32(slot, 16, unchecked(I32(slot, 16) - offsets[node]));
            return LegacyNativeWordRowMaterializer.Materialize(slot, ordinal, cfg.PrimaryArcLimit230a,
                2, 1, stored.PrimaryAlternatives.Span);
        }
        ReadOnlyMemory<byte> Read(int node, ushort ordinal) => Stored(node, ordinal).Record;
        owner.AdvanceNodeEpoch(-1);

        for (int node = 0; node < graph.NodeCount; node++)
        {
            var heap = new LegacyNativeWordRecordHeap((int)heapPlan.Limits[0].PrimaryInitialCapacity20);
            var router = Router(heap);
            int roots = 0, parents = 0, flagAllocations = 0;
            for (byte span = 0; span < owner.GetSpanExtent(node); span++)
            {
                var selected = owner.Prepare(node, span);
                //An independent per-span query instance isolates the main
                //pass's persistent290 ownership without sharing its cache.
                var source = new LegacyNativeDelimiterQuerySource(owner.DictionaryOwner.Model.AcousticAlphabet,
                    owner.Contexts, (tag, request, token) => owner.Read(selected, tag, request, cancellationToken: token),
                    LegacyNativeDelimiterQuerySource.ReadV10dCoordinateMode(ak));
                int previous = selected.PreviousEndingNode;
                if (previous == -1)
                {
                    var request = new LegacyPreparedDictionaryQuery
                    {
                        Capacity = 128, Parameter1c = (uint)node, Parameter20 = span, ContextRoot = owner.Contexts.ContainerIdentity
                    };
                    var query = new LegacyNativeDelimiterQuery { SourceTag = 8UL << 40, Request = request };
                    for (;;)
                    {
                        var page = source.Read(query);
                        if (page.ReturnedCount < 0) throw new InvalidDataException("Oracle startup source failed.");
                        if (page.ReturnedCount == 0) break;
                        roots += page.ReturnedCount;
                        startup.ProcessPreparedPage(selected, page.Records.Span, 0, router, 0, false, false);
                        request.Skip += page.ReturnedCount;
                    }
                }
                else
                {
                    var parentConsumer = new LegacyNativeOrdinaryParentWordCandidates(graph, owner.DictionaryOwner.Model.AcousticAlphabet,
                        owner.SelectedSourceOwner.Categories, owner.Contexts, geometry, acoustic, cell, new byte[80]);
                    for (ushort ordinal = 0; ordinal < cfg.PrimaryArcLimit230a; ordinal++)
                    {
                        var parent = Read(previous, ordinal);
                        if (I32(parent.Span, 16) > imp - 1) break;
                        if ((headers[parent.Span[4]] & 0x1c) >= 8)
                        {
                            int anchorNode = I32(parent.Span, 56);
                            var anchor = Read(anchorNode, BinaryPrimitives.ReadUInt16LittleEndian(parent.Span[60..]));
                            if (unchecked(node + 1 + ((sbyte)anchor.Span[32] - anchorNode)) >= policy.Refresh.MaximumWordLength68) continue;
                        }
                        flagAllocations += flags.Process(node, span, previous, ordinal, Read, headers, owner.Contexts,
                            router, false, policy.FlagPrepassDistance58, false).AcceptedAllocations;
                        var request = new LegacyPreparedDictionaryQuery { Capacity = 128, ContextRoot = owner.Contexts.ContainerIdentity };
                        ulong? tag = LegacyOrdinaryDictionaryQueryPreparation.ApplyContinuation(request,
                            new(0, 0, false, false, policy.MaximumParentDistance54), parent.Span, (uint)node, span, 0, false);
                        if (!tag.HasValue) continue;
                        var query = new LegacyNativeDelimiterQuery { SourceTag = tag.Value, Request = request };
                        for (;;)
                        {
                            var page = source.Read(query);
                            if (page.ReturnedCount < 0) throw new InvalidDataException("Oracle parent source failed.");
                            if (page.ReturnedCount == 0) break;
                            parents += page.ReturnedCount;
                            parentConsumer.ProcessPage(page.Records.Span, (ushort)node, span, previous, ordinal, parent.Span,
                                0, selected.Qualifier, headerMap, (n, arc) => owner.GetCosts(selected, n, arc), Read,
                                router, 0, false, false, 0, false, false, null);
                            request.Skip += page.ReturnedCount;
                        }
                    }
                }
            }
            var slots = LegacyNativeWordAlternativeOrdinals.AssignInitial(heap.GetSlotStorageSnapshot().Span, heap.Count).SlotStorage;
            for (int index = 0; index < heap.Count; index++)
            {
                var slot = slots.Slice(index * 128, 128);
                int previous = node - (sbyte)slot.Span[32] - 1;
                pool = previous == -1
                    ? LegacyNativeRootWordAlternativeProducer.Create(slot.Span, 1, (uint)node, cfg, owner.Contexts,
                        value => value, pool, []).PrimaryStorage.ToArray()
                    : LegacyNativeInitialParentWordAlternativeProducer.Create(slot.Span, 1, (uint)node, cfg,
                        Stored, owner.Contexts, value => value, pool, []).PrimaryStorage.ToArray();
            }
            var ordered = LegacyNativeWordAlternativeOrdering.Prepare(slots.Span, heap.Count, false, cfg, headers, false, new(2, pool), null);
            pool = ordered.PrimaryAlternativeStorage.ToArray();
            var descriptor = new LegacyNativeWordHeapDescriptor(ordered.Heap.GetSlotStorageSnapshot().Span,
                ordered.Heap.Capacity, ordered.Heap.Count, 1, heapPlan.Limits[0]);
            if (!LegacyNativeWordQuotaController.Apply([descriptor], cfg, 0, 0)) throw new InvalidDataException("Oracle primary quota failed.");
            var primary = descriptor.GetHeapSnapshot();
            int extent = cfg.PrimaryArcLimit230a + (int)(cfg.ExpansionFactor230c * cfg.PrimaryArcLimit230a);
            var buffer = new LegacyOwnedFreshMainWordNodeBuffer(policy, node, new byte[extent * 88], new byte[cfg.PrimaryArcLimit230a * 80]);
            foreach (int index in Enumerable.Range(0, primary.Count)) buffer.WritePrimary(primary.GetSlotSnapshot(index).Span, pool);
            buffer.BeginLanguage(); buffers.Add(node, buffer);
            bool language = node == graph.NodeCount - 1 || gate.ShouldRun(node, -1, cfg.PackedSource22e8,
                cfg.PrimaryArcLimit230a, buffer.CopyRecordStorage().Span[..(cfg.PrimaryArcLimit230a * 88)], policy.MaximumParentDistance54, headers);
            int languageQueries = 0, languageAllocations = 0;
            int minimum = Enumerable.Range(0, primary.Count).Select(index => I32(primary.GetSlotSnapshot(index).Span, 16))
                .DefaultIfEmpty(imp).Min();
            if (language)
            {
                var languageHeap = new LegacyNativeWordRecordHeap((int)heapPlan.Limits[0].SecondaryInitialCapacity2c);
                var completion = new LegacyNativeFirstDictionaryWordCompletion(owner.DictionaryOwner.Model,
                    owner.DictionaryOwner.WordMetadata, owner.DictionaryOwner.LanguageRecords, owner.DictionaryOwner.RestartTags,
                    1, headers[1], false, 0, false, false);
                //The independent real-resource audit proves this DIRECT map
                //has no auxiliary terminal edges. This oracle uses its raw
                //aggregate plus native current-terminal injection; the main
                //pass additionally runs the separately verified290.V48 layer.
                var auxiliary = new LegacyNativeAuxiliaryWordQueryPages(owner.DictionaryOwner.DictionaryQueries,
                    owner.Contexts, 128, new byte[4096]);
                for (ushort ordinal = 0; ordinal < cfg.PrimaryArcLimit230a; ordinal++)
                {
                    var parent = buffer.Read(ordinal).Record;
                    if (I32(parent.Span, 16) >= imp) break;
                    ushort currentOrdinal = ordinal;
                    auxiliary.Run(parent.Span, node, new(0, 0, false, false, policy.MaximumParentDistance54), 0, false, false,
                        (page, token) =>
                        {
                            for (int offset = 0; offset < page.Records32.Length; offset += 32)
                            {
                                languageQueries++;
                                var result = completion.ProcessCandidate(page.Records32.Span.Slice(offset, 32), node, currentOrdinal,
                                    parent.Span, Router(languageHeap), false, new byte[40], Read, false, token);
                                if (result.Allocation.Accepted) languageAllocations++;
                            }
                            return true;
                        }, 100);
                }
                var languageOrdered = LegacyNativeWordAlternativeOrdering.Prepare(languageHeap.GetSlotStorageSnapshot().Span,
                    languageHeap.Count, true, cfg, headers, false, null, null);
                var languageDescriptor = new LegacyNativeWordHeapDescriptor(languageOrdered.Heap.GetSlotStorageSnapshot().Span,
                    languageOrdered.Heap.Capacity, languageOrdered.Heap.Count, 2, heapPlan.Limits[0]);
                if (!LegacyNativeWordQuotaController.Apply([languageDescriptor], cfg, 0, 1)) throw new InvalidDataException("Oracle language quota failed.");
                var live = languageDescriptor.GetHeapSnapshot();
                foreach (int index in Enumerable.Range(0, live.Count)) buffer.WriteLanguage(live.GetSlotSnapshot(index).Span);
                minimum = Math.Min(minimum, Enumerable.Range(0, live.Count).Select(index => I32(live.GetSlotSnapshot(index).Span, 16))
                    .DefaultIfEmpty(imp).Min());
                buffer.CompactLanguage(new(headers));
            }
            else buffer.FinishWithoutLanguage();
            cumulative[node] = node == 0 ? 0 : cumulative[node - 1];
            int periodic = 0;
            if ((node + 1) % 50 == 0 && minimum > 0 && minimum < imp)
            {
                //Independent delayed offset representation: lower consumer
                //lookups materialize shifted snapshots; underlying NodeBuffer
                //is not mutated and the implementation's shift helper is unused.
                int previousCount = Math.Min(node + 1, graph.MaximumAtoms);
                for (int distance = 0; distance <= previousCount && distance <= node; distance++)
                {
                    int previous = node - distance;
                    offsets[previous] = unchecked(offsets[previous] + minimum);
                    cumulative[previous] = unchecked(cumulative[previous] + minimum);
                }
                periodic = minimum;
            }
            output.Add(new(node, roots, parents, flagAllocations, primary.Count, language, languageQueries, languageAllocations,
                buffer.SourceEnd23f0, buffer.RetainedEnd23f8, buffer.CopyRecordStorage(), buffer.CopyPrimaryStorage(), cumulative[node], periodic));
            owner.AdvanceNodeEpoch(node);
        }
        return output.Select(result =>
        {
            var buffer = buffers[result.EndingNode];
            byte[] records = buffer.CopyRecordStorage().ToArray();
            if (offsets[result.EndingNode] != 0)
                for (int ordinal = 0; ordinal < buffer.SourceEnd23f0; ordinal++)
                {
                    int at = ordinal * 88 + 16, old = I32(records, at);
                    if (old < imp) W32(records, at, unchecked(old - offsets[result.EndingNode]));
                }
            return result with { Records = records, CumulativePathCost2520 = cumulative[result.EndingNode] };
        }).ToList().AsReadOnly();
    }

    private static int I32(ReadOnlySpan<byte> bytes, int at) => BinaryPrimitives.ReadInt32LittleEndian(bytes[at..]);
    private static void W32(Span<byte> bytes, int at, int value) => BinaryPrimitives.WriteInt32LittleEndian(bytes[at..], value);
}
