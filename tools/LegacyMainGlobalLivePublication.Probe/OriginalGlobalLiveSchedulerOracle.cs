using System.Buffers.Binary;
using InkRecognition;

//Original procedural transcription of the checked2144F0 global CASE control flow.
//It does not call LegacyOwnedMainDirectWordPass, own its descriptor/reset
//implementation, or reuse its row results. Lower native components have their
//own independent scalar/full-record probes. Allocator residue is explicitly0.
internal static class OriginalGlobalLiveSchedulerOracle
{
    internal static IReadOnlyList<LegacyMainDirectWordNodeResult> Run(byte[] ak,
        LegacyFreshMainGlobalWordControllerPolicy policy,
        byte[] compactResidue, int? preparedEndingNode, List<Publication> publications,
        Action<LegacyMainSoftHyphenObservedCall>? observeSoftHyphen = null)
    {
        const int imp = 0x4000000;
        var owner = policy.Services; var binding = LegacyMainOwnedWordControllerBinding.FromGlobal(policy);
        LegacyOwnedMainWordSourceServices.SpanLease? lastSelected = null;
        var cfg = policy.Configuration;
        var graph = owner.Graph;
        uint[] headers = owner.CopySourceHeaders();
        var headerMap = headers.Select((value, index) => (value, index))
            .ToDictionary(item => (byte)item.index, item => item.value);
        var heapPlan = LegacyNativeWordHeapPreparation.PrepareFresh(
            [new(0, 0, 1), LegacyNativeWordPreparedSource.FromDictionary(owner.Descriptor)], cfg, 0);
        var buffers = new Dictionary<int, LegacyOwnedFreshMainWordNodeBuffer>();
        var output = new List<LegacyMainDirectWordNodeResult>();
        var retainedDescriptor = new LegacyNativeWordHeapDescriptor(
            new byte[checked((int)heapPlan.Limits[0].PrimaryInitialCapacity20 * 128)],
            (int)heapPlan.Limits[0].PrimaryInitialCapacity20, 0, 1, heapPlan.Limits[0]);
        var cell = new LegacyNativeWordTransitionCostCell(1, 0);
        var acoustic = LegacyNativeWordAcousticRequestScorer.ForAlphabet(
            LegacyAcousticCandidateThresholds.ReadV110Node(ak.AsSpan(189734), out _), owner.Model.AcousticAlphabet);
        var startup = new LegacyNativeMainInitialSpecialCandidates(ak, owner, cell, new byte[80]);
        byte[] flagRequest = new byte[80]; W32(flagRequest, 52, imp);
        var flags = new LegacyNativeMainWordFlagPrepass(ak, owner, (node, arc, token) => owner.GetCosts(lastSelected!, node, arc, token), cell, flagRequest, default);
        var sharedRequest = flags.BindSharedCallerFrame();
        var gate = new LegacyNativeWordLanguageGate(owner.Model, graph);
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
        LegacyFreshMainGlobalCaseSession.LiveQueryLease? liveLease = null;
        var source = new LegacyNativeDelimiterQuerySource(owner.Model.AcousticAlphabet, owner.Contexts,
            (tag, request, token) => liveLease is { } lease
                ? policy.Owner.ReadLiveQuery(lease, tag, request, false, token)
                : owner.Read(lastSelected!, tag, request, false, token),
            LegacyNativeDelimiterQuerySource.ReadV10dCoordinateMode(ak));
        byte[] compact = compactResidue.ToArray();
        int count = 0, nextNode = 0;
        int[] starts = new int[graph.NodeCount + 1], prefix = new int[graph.NodeCount + 1];
        var confidence = LegacyFreshMainGlobalGlyphConfidence.Prepare(ak, policy.Owner);
        // This procedural reference owns an explicit zero allocator, like its existing heap/pool buffers.
        // GLOBAL publication must use this reference policy's actual live source/language lease.
        // The independent raw-resource auditor still checks complete final bytes separately.
        ulong publicationAllocationIdentity=1;
        var publicationStorage=new LegacyFreshMainGlobalPublicationStorage(policy,request =>
        {
            byte[] storage=new byte[request.RequiredBytes];request.PreviousStorage.Span.CopyTo(storage);
            return new(storage,++publicationAllocationIdentity);
        });
        var invocation = LegacyFreshMainGlobalTerminalInvocation.Prepare(policy);
        source.ResetForControllerNode();
        owner.AdvanceNodeEpoch(-1, default);

        for (int node = 0; node < graph.NodeCount; node++)
        {
            if (!LegacyNativeWordPassReset.Reset([retainedDescriptor], cfg, 0, Grow))
                throw new Exception("Oracle primary reset failed.");
            var heap = retainedDescriptor.GetHeapSnapshot();
            var router = Router(heap);
            int roots = 0, parents = 0, flagAllocations = 0;
            int softCalls = 0, softQualified = 0, softSuccessors = 0, softAllocations = 0;
            for (byte span = 0; span < owner.GetSpanExtent(node); span++)
            {
                var selected = owner.Prepare(node, span, default); lastSelected = selected;
                //One persistent outer290; only actual V80 resets it.
                int previous = selected.PreviousEndingNode;
                if (previous == -1)
                {
                    var request = new LegacyPreparedDictionaryQuery
                    {
                        Capacity = 128,
                        Parameter1c = (uint)node,
                        Parameter20 = span,
                        ContextRoot = owner.Contexts.ContainerIdentity
                    };
                    var query = new LegacyNativeDelimiterQuery { SourceTag = 8UL << 40, Request = request };
                    for (; ; )
                    {
                        var page = source.Read(query);
                        if (page.ReturnedCount < 0) throw new InvalidDataException("Oracle startup source failed.");
                        if (page.ReturnedCount == 0) break;
                        roots += page.ReturnedCount;
                        startup.ProcessOwnedPage(selected, page.Records.Span, 0, router, 0, false, false);
                        request.Skip += page.ReturnedCount;
                    }
                }
                else
                {
                    var parentConsumer = new LegacyNativeOrdinaryParentWordCandidates(owner, acoustic, cell, new byte[80], policy.Refresh.KnowledgeMarkerA0);
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
                        if (node + 1 < graph.NodeCount && (parent.Span[6] & 1) == 0)
                        {
                            int currentLine = Line(node, 0), nextLine = Line(node + 1, 0);
                            int priorLine = Line(previous, BinaryPrimitives.ReadUInt16LittleEndian(parent.Span[28..]));
                            if (currentLine != -1 && unchecked(nextLine - currentLine) == 1 &&
                                (priorLine == -1 || priorLine == currentLine))
                            {
                                int beforeCount = heap.Count;
                                var before = heap.GetSlotStorageSnapshot();
                                var soft = OriginalSoftHyphenOracle.Run(policy, selected, ordinal, parent,
                                    Read, sharedRequest, router, acoustic);
                                softCalls++; if (soft.Qualified) softQualified++;
                                softSuccessors += soft.Successors; softAllocations += soft.AcceptedAllocations;
                                observeSoftHyphen?.Invoke(new(node, span, previous, ordinal, parent.ToArray(),
                                    beforeCount, before, heap.Count, heap.GetSlotStorageSnapshot(), soft));
                            }
                        }
                        var request = new LegacyPreparedDictionaryQuery { Capacity = 128, ContextRoot = owner.Contexts.ContainerIdentity };
                        ulong? tag = LegacyOrdinaryDictionaryQueryPreparation.ApplyContinuation(request,
                            new(0, 0, false, false, policy.MaximumParentDistance54), parent.Span, (uint)node, span, 0, false);
                        if (!tag.HasValue) continue;
                        var query = new LegacyNativeDelimiterQuery { SourceTag = tag.Value, Request = request };
                        for (; ; )
                        {
                            var page = source.Read(query);
                            if (page.ReturnedCount < 0) throw new InvalidDataException("Oracle parent source failed.");
                            if (page.ReturnedCount == 0) break;
                            parents += page.ReturnedCount;
                            parentConsumer.ProcessPage(page.Records.Span, (ushort)node, span, previous, ordinal, parent.Span,
                                0, selected.Qualifier, headerMap, (n, arc) => owner.GetCosts(selected, n, arc, default), Read,
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
            var descriptor = CarryStorage(retainedDescriptor, ordered.Heap);
            if (!LegacyNativeWordQuotaController.Apply([descriptor], cfg, 0, 0)) throw new InvalidDataException("Oracle primary quota failed.");
            var primary = descriptor.GetHeapSnapshot();
            retainedDescriptor = descriptor;
            int extent = cfg.PrimaryArcLimit230a + (int)(cfg.ExpansionFactor230c * cfg.PrimaryArcLimit230a);
            var buffer = new LegacyOwnedFreshMainWordNodeBuffer(binding, node, new byte[extent * 88], new byte[cfg.PrimaryArcLimit230a * 80], [], default);
            foreach (int index in Enumerable.Range(0, primary.Count)) buffer.WritePrimary(primary.GetSlotSnapshot(index).Span, pool);
            buffer.BeginLanguage(); buffers.Add(node, buffer);
            bool language = node == graph.NodeCount - 1 || gate.ShouldRun(node, -1, cfg.PackedSource22e8,
                cfg.PrimaryArcLimit230a, buffer.CopyRecordStorage().Span[..(cfg.PrimaryArcLimit230a * 88)], policy.MaximumParentDistance54, headers);
            int languageQueries = 0, languageAllocations = 0;
            int minimum = Enumerable.Range(0, primary.Count).Select(index => I32(primary.GetSlotSnapshot(index).Span, 16))
                .DefaultIfEmpty(imp).Min();
            if (language)
            {
                if (!LegacyNativeWordPassReset.Reset([retainedDescriptor], cfg, 1, Grow))
                    throw new Exception("Oracle language reset failed.");
                var languageHeap = retainedDescriptor.GetHeapSnapshot();
                var completion = LegacyNativeFirstDictionaryWordCompletion.ForGlobalSpan(policy, lastSelected!, default);
                //Independent raw aggregate292 V48 instead of the scheduler's
                //outer290. Actual auxiliary60 has17 edges and0terminal nodes.
                var auxiliary = new LegacyNativeAuxiliaryWordQueryPages((tag, request, token) =>
                    owner.Read(lastSelected!, tag, request, true, token), owner.Contexts, 128, new byte[4096]);
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
                var languageDescriptor = CarryStorage(retainedDescriptor, languageOrdered.Heap);
                if (!LegacyNativeWordQuotaController.Apply([languageDescriptor], cfg, 0, 1)) throw new InvalidDataException("Oracle language quota failed.");
                var live = languageDescriptor.GetHeapSnapshot();
                retainedDescriptor = languageDescriptor;
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
                buffer.SourceEnd23f0, buffer.RetainedEnd23f8, buffer.CopyRecordStorage(), buffer.CopyPrimaryStorage(), cumulative[node], periodic)
            {
                SoftHyphenCalls = softCalls,
                SoftHyphenQualified = softQualified,
                SoftHyphenSuccessors = softSuccessors,
                SoftHyphenAllocations = softAllocations
            });
            if (preparedEndingNode == node || preparedEndingNode is null && node + 1 < graph.NodeCount &&
                unchecked(policy.MaximumParentDistance54 + nextNode) <= node + 1)
            {
                //Resolve the independently delayed normalization before byte writeback.
                for (int prior = 0; prior <= node; prior++)
                    if (offsets[prior] != 0) { buffers[prior].ApplyPeriodicCostOffset(offsets[prior], default); offsets[prior] = 0; }
                LegacyMainDirectWordNodeResult Snapshot(int n) => output[n] with
                { Records = buffers[n].CopyRecordStorage(), PrimaryAlternatives = buffers[n].CopyPrimaryStorage(), CumulativePathCost2520 = cumulative[n] };
                var before = Enumerable.Range(0, node + 1).Select(Snapshot).ToArray();
                liveLease = policy.Owner.CaptureLiveQueryLease(lastSelected!.GlobalSource!, default);
                void Validate(CancellationToken token) => policy.Owner.ValidateLiveQueryLease(liveLease, token);
                var reach = LegacyPreparedMainCompactReachability.Run(cfg, graph.MaximumAtoms, nextNode, node, Snapshot,
                    source, owner.Contexts.ContainerIdentity, Validate, false, default);
                var published = LegacyPreparedMainCompactPublication.Run(cfg, graph, owner.Contexts, owner.Model.AcousticAlphabet,
                    owner.Categories.Categories, invocation.Options.ContextSentinels24b8, invocation.LanguagePenalty2534,
                    false, nextNode, node, reach.Nodes, reach.LanguageProbe,
                    group => LegacyMainCompactGroupScores.FromGlobal(confidence.ReadGroup(group)),
                    compact, count, starts, prefix, Validate, default, headers,
                    LegacyFreshMainGlobalPublicationContext.ForLive(policy,publicationStorage,liveLease,nextNode,default));
                publications.Add(new(nextNode, node, before, reach, published));
                for (int prior = nextNode; prior <= node; prior++) buffers[prior].ApplyCompactPublication(policy, published.Nodes[prior], default);
                compact = published.Storage; count = published.Count; starts = published.Starts; prefix = published.PrefixOwners; nextNode = node + 1;
                policy.Owner.EndLiveQueryLease(liveLease, default); liveLease = null;
            }
            source.ResetForControllerNode();
            owner.AdvanceNodeEpoch(node, default);
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
            return result with { Records = records, PrimaryAlternatives = buffer.CopyPrimaryStorage(), CumulativePathCost2520 = cumulative[result.EndingNode] };
        }).ToList().AsReadOnly();

        int Line(int node, int arc) => owner.Graph.GlobalSource!.ResolveGroupSource(
            graph.GetNode(node)[arc].GroupId, default).Primary.LineId;

        static LegacyNativeWordHeapGrowthResult? Grow(int _, LegacyNativeWordHeapGrowthRequest request)
        {
            byte[] storage = new byte[request.RequiredAllocatedSlots * 128];
            request.PreviousStorage.Span.CopyTo(storage);
            return new(storage, 1);
        }
        static LegacyNativeWordHeapDescriptor CarryStorage(LegacyNativeWordHeapDescriptor previous,
            LegacyNativeWordRecordHeap changed)
        {
            byte[] storage = previous.GetBackingStorageSnapshot().ToArray();
            changed.GetSlotStorageSnapshot().Span.CopyTo(storage);
            return new(storage, changed.Capacity, changed.Count, 1, previous.Limits);
        }
    }

    internal sealed record Publication(int Start, int End, LegacyMainDirectWordNodeResult[] Before,
        LegacyPreparedMainCompactReachability.Result Reachability, LegacyPreparedMainCompactPublication.Result Published);

    private static int I32(ReadOnlySpan<byte> bytes, int at) => BinaryPrimitives.ReadInt32LittleEndian(bytes[at..]);
    private static void W32(Span<byte> bytes, int at, int value) => BinaryPrimitives.WriteInt32LittleEndian(bytes[at..], value);
}
