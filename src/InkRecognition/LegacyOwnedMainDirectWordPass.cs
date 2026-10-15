using System.Buffers.Binary;

namespace InkRecognition;

public enum LegacyMainDirectStorageKind
{
    HeapRecords, PrimaryAlternativePool, NodeRecords, NodePrimaryAlternatives,
    InitialRequestResidue, ParentRequestResidue, FlagRequestResidue, AuxiliaryScratch, LexicalResidue
}

public readonly record struct LegacyMainDirectStorageRequest(LegacyMainDirectStorageKind Kind,
    int EndingNode, int RequiredBytes, ReadOnlyMemory<byte> PreviousStorage, ulong PreviousIdentity);
public readonly record struct LegacyMainDirectStorageAllocation(ReadOnlyMemory<byte> Storage, ulong Identity);
public delegate LegacyMainDirectStorageAllocation LegacyMainDirectStorageProvider(LegacyMainDirectStorageRequest request);

public sealed record LegacyMainDirectWordNodeResult(int EndingNode, int RootQueries, int ParentQueries,
    int FlagAllocations, int PrimaryCandidates, bool LanguageRan, int LanguageQueries, int LanguageAllocations,
    ushort SourceEnd23f0, ushort RetainedEnd23f8, ReadOnlyMemory<byte> Records, ReadOnlyMemory<byte> PrimaryAlternatives,
    int CumulativePathCost2520 = 0, int AppliedPeriodicOffset = 0);

/// <summary>Owned nontracing DIRECT main240 node/span loop. This consumes the
///prepared main source, real dictionary permissions and actual four-family
///caches. Public constructors retain the DIRECT scope; the typed case facade
///provides an internal verified fresh292 source policy. Grouped
///1FA300 row-pointer allocation or native final winner/acceptance.</summary>
public sealed class LegacyOwnedMainDirectWordPass
{
    private const int Impossible = 0x4000000;
    private readonly LegacyOwnedFreshMainWordControllerPolicy policy;
    private readonly LegacyFreshCaseMainSourcePolicy? caseSourcePolicy;
    private readonly LegacyOwnedMainWordSpanPreparation owner;
    private readonly LegacyNativeWordControllerConfiguration configuration;
    private readonly LegacyNativeWordHeapPreparation preparation;
    private readonly LegacyMainDirectStorageProvider allocator;
    private readonly LegacyNativeMainInitialSpecialCandidates initial;
    private readonly LegacyNativeOrdinaryParentWordCandidates parents;
    private readonly LegacyNativeMainWordFlagPrepass flagPrepass;
    private readonly LegacyNativeDelimiterQuerySource delimiter;
    private readonly LegacyNativeAuxiliaryDelimiterQuerySource auxiliaryDelimiter;
    private readonly LegacyNativeAuxiliaryWordQueryPages auxiliaryPages;
    private readonly LegacyNativeWordLanguageGate languageGate;
    private readonly uint[] headers;
    private readonly Dictionary<byte, uint> headerMap;
    private readonly Dictionary<int, LegacyOwnedFreshMainWordNodeBuffer> nodes = [];
    private int[] cumulative;
    private readonly byte[] lexicalResidue;
    private LegacyNativeWordHeapDescriptor descriptor;
    private LegacyOwnedMainWordSpan? currentSpan;
    private LegacyOwnedMainWordSpanPreparation.LivePublicationEpoch? livePublicationEpoch;
    private byte[] primaryPool;
    private ulong primaryPoolIdentity;
    private int started;
    public int LastPreparedEndingNode => currentSpan?.EndingNode ?? -1;
    public int LastPreparedSpan => currentSpan?.Span ?? -1;

    public LegacyOwnedMainDirectWordPass(ReadOnlySpan<byte> actualResource,
        LegacyOwnedFreshMainWordControllerPolicy policy, LegacyCursiveSeparationTable table,
        LegacyMainDirectStorageProvider allocator, CancellationToken cancellationToken = default)
        : this(actualResource, policy, null, table, allocator, cancellationToken) { }

    /// <summary>Actual multi-physical single-descriptor main source. The
    ///acoustic owner must be the exact one prepared by this span lifecycle.</summary>
    public LegacyOwnedMainDirectWordPass(ReadOnlySpan<byte> actualResource,
        LegacyOwnedFreshMainWordControllerPolicy policy, LegacyPreparedFreshMainLine240AcousticScorer? actualLineAcoustic,
        LegacyCursiveSeparationTable table, LegacyMainDirectStorageProvider allocator,
        CancellationToken cancellationToken = default)
        : this(actualResource,policy,actualLineAcoustic,table,allocator,null,cancellationToken) { }

    internal LegacyOwnedMainDirectWordPass(ReadOnlySpan<byte> actualResource,
        LegacyOwnedFreshMainWordControllerPolicy policy, LegacyPreparedFreshMainLine240AcousticScorer? actualLineAcoustic,
        LegacyCursiveSeparationTable table, LegacyMainDirectStorageProvider allocator,
        LegacyFreshCaseMainSourcePolicy? caseSourcePolicy, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(table);
        ArgumentNullException.ThrowIfNull(allocator);
        cancellationToken.ThrowIfCancellationRequested();
        this.policy = policy;
        this.caseSourcePolicy = caseSourcePolicy;
        owner = policy.Owner;
        configuration = policy.Configuration;
        this.allocator = allocator;
        cumulative = new int[owner.SourceGraph.NodeCount];
        if (owner.IsCaseAware != (caseSourcePolicy is not null) ||
            caseSourcePolicy is not null && (!ReferenceEquals(caseSourcePolicy.Controller,policy) ||
                !ReferenceEquals(caseSourcePolicy.DictionaryOwner,owner.CaseDictionaryOwner)) ||
            configuration.Mode22f6 != 0 || configuration.Trace2368 != 0 ||
            configuration.PrimaryAlternativeCount2402 != 2 || configuration.SecondaryAlternativeCount2404 != 1 ||
            configuration.SuppressPreviousContext2609 != 0 || policy.SecondaryScaleEc != 0 ||
            policy.Refresh.ConsecutiveBlank22f7 != 0 || policy.KnowledgeF5)
            throw new NotSupportedException("Matching fresh DIRECT or typed case policy, mode0/2/2/1 without associated/trace state required.");
        if (actualLineAcoustic is not null && !ReferenceEquals(actualLineAcoustic, owner.MainLineAcoustic))
            throw new ArgumentException("Main line acoustic caches must belong to the exact supplied span owner.");
        if (actualResource.Length != 572763)
            throw new ArgumentException("Actual selected main resource/source ownership required.");
        headers = owner.CopySourceHeaders();
        headerMap = headers.Select((header, index) => (header, index))
            .ToDictionary(item => checked((byte)item.index), item => item.header);
        preparation = LegacyNativeWordHeapPreparation.PrepareFresh(
            [new(0, 0, 1), LegacyNativeWordPreparedSource.FromDictionary(owner.CommonDescriptor)], configuration, 0, cancellationToken);
        if (headers.Length != 2 || preparation.Limits.Count != 1 ||
            preparation.Sources.OverrideSourceIndex22f0 != -1 || preparation.Sources.SubwordSourceIndex22ec != -1)
            throw new NotSupportedException("Fresh implicit0 plus one ordinary C00 direct source required.");
        //2B7.V60=05A400 and290.V60=05A400 are false, so freshDIRECT does not
        //schedule1FE740. This is a source-class proof, not a filtered candidate.
        var heapAllocation = Allocate(LegacyMainDirectStorageKind.HeapRecords, -1,
            checked((int)preparation.Limits[0].PrimaryInitialCapacity20 * 128), ReadOnlyMemory<byte>.Empty, 0, cancellationToken);
        descriptor = new(heapAllocation.Storage.Span, heapAllocation.Storage.Length / 128, 0,
            heapAllocation.Identity, preparation.Limits[0]);
        var poolAllocation = Allocate(LegacyMainDirectStorageKind.PrimaryAlternativePool, -1,
            checked(descriptor.Capacity * 80), ReadOnlyMemory<byte>.Empty, 0, cancellationToken);
        primaryPool = poolAllocation.Storage.ToArray(); primaryPoolIdentity = poolAllocation.Identity;
        var cell = new LegacyNativeWordTransitionCostCell(1, 0);
        byte[] initialResidue = Allocate(LegacyMainDirectStorageKind.InitialRequestResidue, -1, 80, ReadOnlyMemory<byte>.Empty, 0, cancellationToken).Storage.ToArray();
        byte[] parentResidue = Allocate(LegacyMainDirectStorageKind.ParentRequestResidue, -1, 80, ReadOnlyMemory<byte>.Empty, 0, cancellationToken).Storage.ToArray();
        byte[] flagResidue = Allocate(LegacyMainDirectStorageKind.FlagRequestResidue, -1, 80, ReadOnlyMemory<byte>.Empty, 0, cancellationToken).Storage.ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(flagResidue.AsSpan(52), Impossible);
        lexicalResidue = Allocate(LegacyMainDirectStorageKind.LexicalResidue, -1, 40, ReadOnlyMemory<byte>.Empty, 0, cancellationToken).Storage.ToArray();
        var auxiliaryScratch = Allocate(LegacyMainDirectStorageKind.AuxiliaryScratch, -1, 4096, ReadOnlyMemory<byte>.Empty, 0, cancellationToken).Storage;
        initial = new(actualResource, owner, cell, initialResidue);
        var scorer = LegacyNativeWordAcousticRequestScorer.ForAlphabet(
            LegacyAcousticCandidateThresholds.ReadV110Node(actualResource[owner.OrdinaryAcousticOffset..], out _),
            owner.CommonModel.AcousticAlphabet);
        var geometry = actualLineAcoustic is null ? owner.Source.CreatePrepared240TransitionGeometry(table) :
            actualLineAcoustic.Source.CreatePrepared240TransitionGeometry(table);
        parents = new(owner.SourceGraph, owner.CommonModel.AcousticAlphabet, owner.SelectedSourceOwner.Categories,
            owner.Contexts, geometry, scorer, cell, parentResidue);
        flagPrepass = actualLineAcoustic is null
            ? new(actualResource, owner.Acoustic, table, cell, flagResidue, cancellationToken)
            : new(actualResource, actualLineAcoustic, table, cell, flagResidue, cancellationToken);
        delimiter = new(owner.CommonModel.AcousticAlphabet, owner.Contexts,
            (tag, request, token) => livePublicationEpoch is { } live
                ? owner.ReadLivePublicationQuery(live, tag, request, false, token)
                : owner.Read(currentSpan ?? throw new InvalidOperationException("D0 must precede290 reads."),
                    tag, request, clearExtraCosts: false, cancellationToken: token),
            LegacyNativeDelimiterQuerySource.ReadV10dCoordinateMode(actualResource));
        auxiliaryDelimiter = new(owner.CommonModel.AcousticAlphabet, owner.Contexts,
            (tag, request, token) => owner.Read(currentSpan ?? throw new InvalidOperationException("D0 must precede auxiliary reads."),
                tag, request, auxiliary: true, clearExtraCosts: false, cancellationToken: token), owner.SourceGraph.NodeCount);
        auxiliaryPages = new((tag, request, token) =>
        {
            var page = auxiliaryDelimiter.Read(tag, request, token);
            return new(page.ReturnedCount, page.Records, ReadOnlyMemory<byte>.Empty, -1, request.Skip, null);
        }, owner.Contexts, 128, auxiliaryScratch.Span);
        languageGate = new(owner.CommonModel, owner.SourceGraph);
    }

    /// <summary>Run the same single-use pass and retain immutable, source-bound
    /// final row storage for the owned terminal invocation.</summary>
    public LegacyOwnedMainDirectWordPassResult RunOwned(CancellationToken cancellationToken = default)
        => LegacyOwnedMainDirectWordPassResult.Create(policy, Run(cancellationToken), cancellationToken);

    /// <summary>Opt-in2144F0 intermediate207CB0 lifecycle. Uses the same live
    /// D0 map and outer290 caches, writes actual publication marks/indices back
    /// into the owning rows, and retains2460/2478/2488/2490 for final publication.
    /// Allocator residue is explicit; unsupported native publication branches
    /// fail this single-use pass rather than manufacturing a terminal.</summary>
    public LegacyOwnedMainDirectWordPassResult RunOwnedWithIntermediatePublication(ReadOnlySpan<byte> actualResource,
        LegacyOwnedRussianMainGlyphConfidence candidateOwner, ReadOnlySpan<byte> compactAllocatorResidue40,
        Action<LegacyMainIntermediateCompactPublication>? publicationObserver = null, CancellationToken cancellationToken = default)
        => RunOwnedWithPublicationCore(actualResource, candidateOwner, compactAllocatorResidue40,
            publicationObserver, null, cancellationToken);

    //Prepared consumer control, deliberately internal: an actual short source
    //can exercise nonempty207CB0 before V80 without pretending its native120
    //gate would fire there. Public scheduling never supplies this override.
    internal LegacyOwnedMainDirectWordPassResult RunOwnedWithPreparedPublicationAtNode(ReadOnlySpan<byte> actualResource,
        LegacyOwnedRussianMainGlyphConfidence candidateOwner, ReadOnlySpan<byte> compactAllocatorResidue40,
        int endingNode, Action<LegacyMainIntermediateCompactPublication>? publicationObserver = null,
        CancellationToken cancellationToken = default)
    {
        if (endingNode < 0 || endingNode >= owner.SourceGraph.NodeCount - 1 ||
            owner.SourceGraph.NodeCount > policy.MaximumParentDistance54)
            throw new ArgumentOutOfRangeException(nameof(endingNode));
        return RunOwnedWithPublicationCore(actualResource, candidateOwner, compactAllocatorResidue40,
            publicationObserver, endingNode, cancellationToken);
    }

    private LegacyOwnedMainDirectWordPassResult RunOwnedWithPublicationCore(ReadOnlySpan<byte> actualResource,
        LegacyOwnedRussianMainGlyphConfidence candidateOwner, ReadOnlySpan<byte> compactAllocatorResidue40,
        Action<LegacyMainIntermediateCompactPublication>? publicationObserver, int? preparedEndingNode,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(candidateOwner);
        cancellationToken.ThrowIfCancellationRequested();
        LegacyOwnedMainCompactWordMaterialization.ValidatePublicationOwner(actualResource, policy, caseSourcePolicy, candidateOwner);
        var publication = new IntermediatePublication(actualResource.ToArray(), candidateOwner,
            new(policy, compactAllocatorResidue40), publicationObserver, preparedEndingNode);
        var result = LegacyOwnedMainDirectWordPassResult.Create(policy, RunCore(cancellationToken, publication), cancellationToken);
        result.BindIntermediatePublicationState(publication.State);
        return result;
    }

    /// <summary>One owning ascending node/span pass. All backend pages are
    ///consumed; unsupported source/geometry/history branches fail the pass.
    ///Allocation bytes are caller-owned residue, not native zero assumptions.</summary>
    public IReadOnlyList<LegacyMainDirectWordNodeResult> Run(CancellationToken cancellationToken = default)
        => RunCore(cancellationToken, null);

    private IReadOnlyList<LegacyMainDirectWordNodeResult> RunCore(CancellationToken cancellationToken,
        IntermediatePublication? publication)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (Interlocked.Exchange(ref started, 1) != 0)
            throw new InvalidOperationException("A main pass is single-use, including failed/canceled passes.");
        var output = new List<LegacyMainDirectWordNodeResult>();
        var graph = owner.SourceGraph;
        ResetSources(-1, cancellationToken);
        for (int node = 0; node < graph.NodeCount; node++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Reset(0, node, cancellationToken);
            var heap = descriptor.GetHeapSnapshot();
            var router = Router(heap);
            int rootQueries = 0, parentQueries = 0, flagAllocations = 0;
            for (int spanIndex = 0; spanIndex < owner.GetSpanExtent(node); spanIndex++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                byte span = checked((byte)spanIndex);
                currentSpan = owner.Prepare(node, span, cancellationToken);
                int previous = currentSpan.PreviousEndingNode;
                if (previous < 0)
                {
                    var request = new LegacyPreparedDictionaryQuery
                    {
                        Capacity = 128, Parameter1c = (uint)node, Parameter20 = span,
                        ContextRoot = owner.Contexts.ContainerIdentity
                    };
                    var query = new LegacyNativeDelimiterQuery { SourceTag = 8UL << 40, Request = request };
                    ReadAll(query, page =>
                    {
                        rootQueries += page.ReturnedCount;
                        initial.ProcessPreparedPage(currentSpan, page.Records.Span, 0, router, 0, false, false, cancellationToken);
                    }, cancellationToken);
                }
                else
                {
                    for (ushort ordinal = 0; ordinal < configuration.PrimaryArcLimit230a; ordinal++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var parent = nodes[previous].Read(ordinal).Record;
                        if (I32(parent.Span, 16) >= Impossible) break;
                        if (!WithinWordLength(node, parent.Span)) continue;
                        flagAllocations += flagPrepass.Process(node, span, previous, ordinal, LookupRecord,
                            headers, owner.Contexts, router, false, policy.FlagPrepassDistance58, false, cancellationToken).AcceptedAllocations;
                        var request = new LegacyPreparedDictionaryQuery { Capacity = 128, ContextRoot = owner.Contexts.ContainerIdentity };
                        ulong? tag = LegacyOrdinaryDictionaryQueryPreparation.ApplyContinuation(request,
                            QueryPolicy(), parent.Span, (uint)node, span, 0, false);
                        if (!tag.HasValue) continue;
                        var query = new LegacyNativeDelimiterQuery { SourceTag = tag.Value, Request = request };
                        ReadAll(query, page =>
                        {
                            parentQueries += page.ReturnedCount;
                            var result = parents.ProcessPage(page.Records.Span, checked((ushort)node), span, previous, ordinal,
                                parent.Span, 0, currentSpan.Qualifier, headerMap,
                                (n, arc) => owner.GetCosts(currentSpan, n, arc, cancellationToken), LookupRecord,
                                router, 0, false, false, 0, false, false, null, cancellationToken);
                            if (!result.Succeeded) throw new InvalidOperationException("Native201270 source consumer failed.");
                        }, cancellationToken);
                    }
                }
            }
            var ordered = OrderPrimary(heap, node, cancellationToken);
            ReplaceDescriptor(ordered.Heap);
            if (!LegacyNativeWordQuotaController.Apply([descriptor], configuration, 0, 0,
                (index, request) => Grow(index, request, node, cancellationToken), cancellationToken: cancellationToken))
                throw new InvalidOperationException("Native primary quota allocation failed.");
            var primaryHeap = descriptor.GetHeapSnapshot();
            int extent = NodeExtent();
            var nodeRecords = Allocate(LegacyMainDirectStorageKind.NodeRecords, node, extent * 88, ReadOnlyMemory<byte>.Empty, 0, cancellationToken);
            var nodeAlternatives = Allocate(LegacyMainDirectStorageKind.NodePrimaryAlternatives, node,
                configuration.PrimaryArcLimit230a * 80, ReadOnlyMemory<byte>.Empty, 0, cancellationToken);
            var buffer = new LegacyOwnedFreshMainWordNodeBuffer(policy, node, nodeRecords.Storage.Span, nodeAlternatives.Storage.Span, cancellationToken);
            for (int ordinal = 0; ordinal < primaryHeap.Count; ordinal++)
                buffer.WritePrimary(primaryHeap.GetSlotSnapshot(ordinal).Span, ordered.PrimaryAlternativeStorage.Span, cancellationToken);
            buffer.BeginLanguage(cancellationToken);
            nodes.Add(node, buffer);
            bool language = node == graph.NodeCount - 1 || languageGate.ShouldRun(node, -1,
                configuration.PackedSource22e8, configuration.PrimaryArcLimit230a,
                buffer.CopyRecordStorage().Span[..(configuration.PrimaryArcLimit230a * 88)],
                policy.MaximumParentDistance54, headers);
            int languageQueries = 0, languageAllocations = 0;
            int minimum = Enumerable.Range(0, primaryHeap.Count)
                .Select(ordinal => I32(primaryHeap.GetSlotSnapshot(ordinal).Span, 16)).DefaultIfEmpty(Impossible).Min();
            if (language)
            {
                RunLanguage(node, buffer, out languageQueries, out languageAllocations, cancellationToken);
                minimum = Math.Min(minimum, Enumerable.Range(configuration.PrimaryArcLimit230a,
                    buffer.SourceEnd23f0 - configuration.PrimaryArcLimit230a)
                    .Select(ordinal => I32(buffer.Read((ushort)ordinal).Record.Span, 16)).DefaultIfEmpty(Impossible).Min());
                buffer.CompactLanguage(new(headers), cancellationToken);
            }
            else buffer.FinishWithoutLanguage(cancellationToken);
            cumulative[node] = node == 0 ? 0 : cumulative[node - 1];
            int periodicOffset = 0;
            //NEW00C86099/A8 are zero and no property overrides exist in the
            //owned fresh policy. Trace is zero above. Positive finite minima
            //use the exact native (minimum-1U)<0x3FFFFFF guard every50 nodes.
            if ((node + 1) % 50 == 0 && unchecked((uint)(minimum - 1)) < 0x3ffffff)
            {
                int backward = Math.Min(node + 1, graph.MaximumAtoms); //151FF0 ->21B.V30/1A75D0.
                for (int prior = node; prior >= Math.Max(0, node - backward); prior--)
                    nodes[prior].ApplyPeriodicCostOffset(minimum, cancellationToken);
                cumulative = LegacyNativeWordPeriodicCostNormalization.AddCumulative(cumulative, node, backward,
                    minimum, cancellationToken).ToArray();
                periodicOffset = minimum;
            }
            output.Add(new(node, rootQueries, parentQueries, flagAllocations, primaryHeap.Count, language,
                languageQueries, languageAllocations, buffer.SourceEnd23f0, buffer.RetainedEnd23f8,
                buffer.CopyRecordStorage(), buffer.CopyPrimaryStorage(), cumulative[node], periodicOffset));
            //215DC0..215DDE: intermediate207CB0 follows periodic cost changes
            //and precedes the actual sourceV80. Both comparisons are signed.
            if (publication is not null && (publication.PreparedEndingNode == node ||
                publication.PreparedEndingNode is null && node + 1 < graph.NodeCount &&
                unchecked(policy.MaximumParentDistance54 + publication.State.NextNode) <= node + 1))
                PublishIntermediate(publication, node, output, cancellationToken);
            ResetSources(node, cancellationToken);
        }
        cancellationToken.ThrowIfCancellationRequested();
        //A later50-node normalization also mutates earlier owned source rows
        //and2520 entries. Final snapshots must expose those actual mutations.
        return output.Select(result => result with
        {
            Records = nodes[result.EndingNode].CopyRecordStorage(),
            PrimaryAlternatives = nodes[result.EndingNode].CopyPrimaryStorage(),
            CumulativePathCost2520 = cumulative[result.EndingNode]
        }).ToList().AsReadOnly();
    }

    private sealed record IntermediatePublication(byte[] Resource, LegacyOwnedRussianMainGlyphConfidence Confidence,
        LegacyMainCompactPublicationState State, Action<LegacyMainIntermediateCompactPublication>? Observer,
        int? PreparedEndingNode);

    private void PublishIntermediate(IntermediatePublication publication, int endingNode,
        IReadOnlyList<LegacyMainDirectWordNodeResult> completed, CancellationToken token)
    {
        var span = currentSpan ?? throw new InvalidOperationException("Intermediate publication requires the actual last D0 span.");
        if (span.EndingNode != endingNode || livePublicationEpoch is not null)
            throw new InvalidOperationException("Intermediate publication cannot replace another active node/query scope.");
        livePublicationEpoch = owner.CaptureLivePublicationEpoch(span, token);
        var lease = livePublicationEpoch;
        try
        {
            LegacyMainDirectWordNodeResult Snapshot(int node)
            {
                var result = completed[node];
                return result with
                {
                    Records = nodes[node].CopyRecordStorage(), PrimaryAlternatives = nodes[node].CopyPrimaryStorage(),
                    CumulativePathCost2520 = cumulative[node]
                };
            }
            var source = new LegacyMainCompactPublicationSource(policy, caseSourcePolicy, publication.State.NextNode,
                endingNode, Snapshot, delimiter, cancellation => owner.ValidateLivePublicationEpoch(lease, cancellation));
            int previousCount = publication.State.Count;
            var stage = LegacyOwnedMainCompactWordMaterialization.Publish(publication.Resource, source,
                publication.Confidence, publication.State, token);
            var observed = publication.Observer is null ? null :
                new LegacyMainIntermediateCompactPublication(source, previousCount, publication.State, stage);
            if (observed is not null) publication.Observer!(observed);
            owner.ValidateLivePublicationEpoch(lease, token);
            for (int node = source.StartingNode; node <= endingNode; node++)
                nodes[node].ApplyCompactPublication(policy, stage.Nodes[node], token);
            owner.EndLivePublicationEpoch(lease, token);
        }
        finally { livePublicationEpoch = null; }
    }

    private void RunLanguage(int node, LegacyOwnedFreshMainWordNodeBuffer buffer,
        out int queries, out int allocations, CancellationToken token)
    {
        Reset(1, node, token);
        var heap = descriptor.GetHeapSnapshot();
        var router = Router(heap);
        var completion = caseSourcePolicy is not null
            ? LegacyNativeFirstDictionaryWordCompletion.ForFreshCaseSpan(caseSourcePolicy,
                currentSpan ?? throw new InvalidOperationException("Actual last D0 case lease required."),token)
            : new LegacyNativeFirstDictionaryWordCompletion(owner.DictionaryOwner.Model, owner.DictionaryOwner.WordMetadata,
                owner.DictionaryOwner.LanguageRecords, owner.DictionaryOwner.RestartTags, 1, headers[1], false, 0, false, false);
        int queryCount = 0, allocationCount = 0;
        for (ushort ordinal = 0; ordinal < configuration.PrimaryArcLimit230a; ordinal++)
        {
            token.ThrowIfCancellationRequested();
            ushort parentOrdinal = ordinal;
            var parent = buffer.Read(ordinal).Record;
            if (I32(parent.Span, 16) >= Impossible) break;
            var result = auxiliaryPages.Run(parent.Span, node, QueryPolicy(), 0, false, false,
                (page, cancellationToken) =>
                {
                    for (int offset = 0; offset < page.Records32.Length; offset += 32)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        queryCount++;
                        var completed = completion.ProcessCandidate(page.Records32.Span.Slice(offset, 32), node,
                            parentOrdinal, parent.Span, router, false, lexicalResidue, LookupRecord, false, cancellationToken);
                        if (completed.Allocation.Accepted) allocationCount++;
                    }
                    return true;
                }, 100, token);
            if (!result.Completed) throw new InvalidOperationException("Native language query failed or exceeded the explicit page budget100.");
        }
        var ordered = LegacyNativeWordAlternativeOrdering.Prepare(heap.GetSlotStorageSnapshot().Span, heap.Count,
            true, configuration, headers, false, null, null, token);
        ReplaceDescriptor(ordered.Heap);
        if (!LegacyNativeWordQuotaController.Apply([descriptor], configuration, 0, 1,
            (index, request) => Grow(index, request, node, token), cancellationToken: token))
            throw new InvalidOperationException("Native language quota allocation failed.");
        var live = descriptor.GetHeapSnapshot();
        for (int ordinal = 0; ordinal < live.Count; ordinal++) buffer.WriteLanguage(live.GetSlotSnapshot(ordinal).Span, token);
        queries = queryCount; allocations = allocationCount;
    }

    private LegacyNativeWordAlternativeOrderingResult OrderPrimary(LegacyNativeWordRecordHeap heap, int node, CancellationToken token)
    {
        var slots = LegacyNativeWordAlternativeOrdinals.AssignInitial(heap.GetSlotStorageSnapshot().Span, heap.Count, cancellationToken: token).SlotStorage;
        for (int index = 0; index < heap.Count; index++)
        {
            token.ThrowIfCancellationRequested();
            var slot = slots.Slice(index * 128, 128);
            int previous = node - unchecked((sbyte)slot.Span[32]) - 1;
            primaryPool = previous < 0
                ? LegacyNativeRootWordAlternativeProducer.Create(slot.Span, 1, (uint)node, configuration,
                    owner.Contexts, symbol => symbol, primaryPool, [], token).PrimaryStorage.ToArray()
                : LegacyNativeInitialParentWordAlternativeProducer.Create(slot.Span, 1, (uint)node, configuration,
                    (n, ordinal) => nodes[n].Read(ordinal), owner.Contexts, symbol => symbol, primaryPool, [], token).PrimaryStorage.ToArray();
        }
        var ordered = LegacyNativeWordAlternativeOrdering.Prepare(slots.Span, heap.Count, false, configuration,
            headers, false, new(2, primaryPool), null, token);
        primaryPool = ordered.PrimaryAlternativeStorage.ToArray();
        return ordered;
    }

    private bool WithinWordLength(int currentNode, ReadOnlySpan<byte> parent)
    {
        byte source = parent[4];
        if (source >= headers.Length) throw new InvalidDataException("Parent source lies outside the owned header table.");
        if ((headers[source] & 0x1c) < 8) return true;
        int anchorNode = I32(parent, 56);
        ushort anchorOrdinal = BinaryPrimitives.ReadUInt16LittleEndian(parent[60..]);
        var anchor = LookupRecord(anchorNode, anchorOrdinal).Span;
        return unchecked(currentNode + 1 + (unchecked((sbyte)anchor[32]) - anchorNode)) < policy.Refresh.MaximumWordLength68;
    }

    private void ReadAll(LegacyNativeDelimiterQuery query, Action<LegacyNativeDelimiterQueryPage> consume, CancellationToken token)
    {
        for (int pageIndex = 0; pageIndex < 16384; pageIndex++)
        {
            token.ThrowIfCancellationRequested();
            var page = delimiter.Read(query, token);
            if (page.ReturnedCount < 0) throw new InvalidOperationException("Native290 backend returned failure.");
            if (page.ReturnedCount == 0) return;
            consume(page);
            token.ThrowIfCancellationRequested();
            query.Request.Skip = checked(query.Request.Skip + page.ReturnedCount);
        }
        throw new NotSupportedException("Native direct query exceeded the explicit16384-page work budget.");
    }

    private void Reset(int phase, int node, CancellationToken token)
    {
        if (!LegacyNativeWordPassReset.Reset([descriptor], configuration, phase,
            (index, request) => Grow(index, request, node, token), cancellationToken: token))
            throw new InvalidOperationException("Native pass-reset allocation failed.");
        if (primaryPool.Length < descriptor.Capacity * 80)
        {
            var allocation = Allocate(LegacyMainDirectStorageKind.PrimaryAlternativePool, node,
                descriptor.Capacity * 80, primaryPool, primaryPoolIdentity, token);
            primaryPool = allocation.Storage.ToArray(); primaryPoolIdentity = allocation.Identity;
        }
    }

    private void ResetSources(int completedNode, CancellationToken token)
    {
        delimiter.ResetForControllerNode(token);
        auxiliaryDelimiter.ResetForControllerNode(token);
        owner.AdvanceNodeEpoch(completedNode, token);
    }

    private LegacyNativeWordHeapGrowthResult Grow(int index, LegacyNativeWordHeapGrowthRequest request, int node, CancellationToken token)
    {
        if (index != 0) throw new ArgumentOutOfRangeException(nameof(index));
        var allocation = Allocate(LegacyMainDirectStorageKind.HeapRecords, node, request.RequiredAllocatedSlots * 128,
            request.PreviousStorage, request.PreviousStorageTag10, token);
        return new(allocation.Storage, allocation.Identity);
    }

    private void ReplaceDescriptor(LegacyNativeWordRecordHeap heap)
    {
        byte[] backing = descriptor.GetBackingStorageSnapshot().ToArray();
        if (heap.Capacity != descriptor.Capacity) throw new InvalidDataException("Ordering changed the descriptor's logical capacity.");
        heap.GetSlotStorageSnapshot().Span.CopyTo(backing);
        descriptor = new(backing, heap.Capacity, heap.Count, descriptor.StorageTag10, descriptor.Limits);
    }

    private LegacyMainDirectStorageAllocation Allocate(LegacyMainDirectStorageKind kind, int node, int bytes,
        ReadOnlyMemory<byte> previous, ulong identity, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (bytes < previous.Length || bytes > 2_000_000) throw new NotSupportedException("Bounded preserving allocation required.");
        var allocation = allocator(new(kind, node, bytes, previous.ToArray(), identity));
        token.ThrowIfCancellationRequested();
        if (allocation.Storage.Length != bytes || !allocation.Storage.Span[..previous.Length].SequenceEqual(previous.Span))
            throw new ArgumentException("Allocator must provide the exact extent and preserve every previous byte.");
        return new(allocation.Storage.ToArray(), allocation.Identity);
    }

    private LegacyOrdinaryDictionaryQueryPolicy QueryPolicy() => new(0, 0, false, false, policy.MaximumParentDistance54);
    private LegacyNativeWordHeapRouter Router(LegacyNativeWordRecordHeap heap)
        => new([heap], preparation.CopySourceBuckets().Span, -1, uint.MaxValue);
    private ReadOnlyMemory<byte> LookupRecord(int node, ushort ordinal)
        => nodes.TryGetValue(node, out var buffer) ? buffer.Read(ordinal).Record :
            throw new ArgumentOutOfRangeException(nameof(node), "Actual owning previous row is required.");
    private int NodeExtent() => configuration.PrimaryArcLimit230a + checked((int)(
        ((configuration.PackedSource22e8 & 0x10) != 0 ? 2f : 1f) * configuration.ExpansionFactor230c * configuration.PrimaryArcLimit230a));
    private static int I32(ReadOnlySpan<byte> bytes, int at) => BinaryPrimitives.ReadInt32LittleEndian(bytes[at..]);
}
