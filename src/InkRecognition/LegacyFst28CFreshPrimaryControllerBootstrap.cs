using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

[assembly: InternalsVisibleTo("LegacyFst28CFreshPrimaryControllerBootstrap.Probe")]
namespace InkRecognition;

public readonly record struct LegacyFst28CNumberSourceMetadata(uint Header, float Weight24, float Weight28,
    int SourceIndex, uint Priority, uint AcceptedSkipBase, byte AggregateNamespace);
public readonly record struct LegacyFst28CBootstrapConstructorState(bool Primary, bool HeaderPresent,
    bool TreeRootAbsent, bool ParentAbsent, uint State, byte Source, byte EnumerationFlags,
    int Take, int Skip, bool CachedOwnerAbsent, bool ContextIssued);

/// <summary>Explicit direct-selected-number fresh collection/frontend binding.
/// Constructor prefix only: no issued context, query, history, Ready or OCR.</summary>
public sealed class LegacyFst28CFreshPrimaryControllerBootstrap
{
    public const int MaximumNodes = 65535, MaximumGroups = 65535, MaximumArcs = 262144;
    public const int LogicalByteEnvelope = 580473, FixedWork = 575901, MaximumWork = 969115;
    private const string AcousticHash = "7A5F08809E81FB4A2D4C78B3EFD251D0986503B0D30D21DE3695DAB1F8A89978";
    private sealed class ContextHeader { internal object? Root => null; }
    private sealed class FrontendBinding(object cohort, LegacyFreshMainCompactOrdinaryGraph graph,
        LegacyFst28CTargetPreparation policy)
    {
        internal readonly object Cohort = cohort;
        internal readonly LegacyFreshMainCompactOrdinaryGraph Graph = graph;
        internal readonly LegacyFst28CTargetPreparation PolicyIssuer = policy;
    }
    private sealed class ConstructorRequest(object cohort, FrontendBinding frontend, ContextHeader header)
    {
        internal readonly object Cohort = cohort;
        internal readonly FrontendBinding Frontend = frontend;
        internal readonly ContextHeader Header = header;
        internal object? Parent => null;
        internal int? ContextStart => null;
        internal int? ContextLength => null;
    }
    private readonly object cohort;
    private readonly FrontendBinding frontend;
    private readonly ContextHeader mapHeader;
    private readonly ConstructorRequest request;
    private readonly object sourceOwner;
    private readonly LegacyFst28CNumberSourceMetadata metadata;
    public LegacyFst28CInitialState InitialState { get; }
    public LegacyFreshMainCompactOrdinaryGraph Graph { get; }
    public int WorkReservation { get; }
    public bool HasIssuedQuery => false;
    public int AdmittedSourceCount => 1;
    public int SelectedSourceCount => 1;
    public int SelectedOrdinal => 0;
    private LegacyFst28CFreshPrimaryControllerBootstrap(LegacyFst28CInitialState initial,
        LegacyFreshMainCompactOrdinaryGraph graph, int work, LegacyFst28CNumberSourceMetadata metadata)
    {
        InitialState = initial; Graph = graph; WorkReservation = work; this.metadata = metadata;
        sourceOwner = graph.Source.Source; cohort = new(); mapHeader = new();
        frontend = new(cohort, graph, initial.TargetPreparation);
        request = new(cohort, frontend, mapHeader);
    }
    public static LegacyFst28CFreshPrimaryControllerBootstrap CreateDirectSelectedNumberFresh(
        LegacyFst28CInitialState initial, LegacyFreshMainCompactOrdinaryGraph graph,
        ReadOnlySpan<byte> acousticResource, CancellationToken token = default)
        => CreateBounded(initial, graph, acousticResource, LogicalByteEnvelope, MaximumWork, token);

    // Pure bound calculator: never grants ownership or constructs a graph.
    internal static int CalculateWork(int nodes, int groups, int arcs, int cap)
    {
        if (nodes is < 1 or > MaximumNodes || groups is < 1 or > MaximumGroups ||
            arcs is < 1 or > MaximumArcs || cap is < 1 or > 8)
            throw new NotSupportedException("Direct bootstrap graph cardinality exceeds the library domain.");
        return checked((int)((long)FixedWork + nodes + groups + arcs));
    }
    internal static LegacyFst28CFreshPrimaryControllerBootstrap CreateBounded(
        LegacyFst28CInitialState initial, LegacyFreshMainCompactOrdinaryGraph graph,
        ReadOnlySpan<byte> acousticResource, int byteBudget, int workBudget, CancellationToken token = default, Action<string>? checkpoint = null)
    {
        token.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(initial); ArgumentNullException.ThrowIfNull(graph);
        if (byteBudget is < 0 or > LogicalByteEnvelope) throw new ArgumentOutOfRangeException(nameof(byteBudget));
        if (workBudget is < 0 or > MaximumWork) throw new ArgumentOutOfRangeException(nameof(workBudget));
        var ownedGroups = graph.State.Groups;
        int nodes = graph.State.NodeCount, groups = ownedGroups.Count, arcs = graph.State.CandidateCount;
        int work = CalculateWork(nodes, groups, arcs, graph.State.MaximumAtoms);
        if (byteBudget < LogicalByteEnvelope || workBudget < work)
            throw new InvalidDataException("Whole bootstrap copy/work envelope exceeds budget.");
        var primary = initial.TargetPreparation.PrimaryMap; var outgoing = primary.Source;
        var root = outgoing.SourceRoot; var child = outgoing.SourceChild; var source = graph.Source.Source;
        if (!outgoing.HasActualFreshMain135Authority || outgoing.ActualAcousticSourceKind != 0x135 ||
            outgoing.ActualAcousticFingerprint != AcousticHash || root is null || child is null ||
            !ReferenceEquals(root.Children[5], child) || child.Ordinal != 5 || child.Offset != 299000 || child.End != 301580 ||
            child.Kind != 0x13a || child.Version != 0x202 || outgoing.StateCount != 98 || outgoing.TransitionWordCount != 299 ||
            source.AcousticOffset != 189734 || source.CategoryOwner.SelectedSourceOffset != 0 ||
            !source.SourceFingerprint.Equals(AcousticHash, StringComparison.OrdinalIgnoreCase) ||
            source.SpanProfile.MaximumAtoms != 8 || initial.Capacity != 12 || primary.SourceLabelCount != 16)
            throw new ArgumentException("Exact retained number and genuine ordinary MAIN source required.");
        if (acousticResource.Length != 572763) throw new ArgumentException("Exact AK length required.", nameof(acousticResource));
        void Checkpoint(string stage) { token.ThrowIfCancellationRequested(); checkpoint?.Invoke(stage); token.ThrowIfCancellationRequested(); }
        Checkpoint("before-snapshot");
        var snapshot = new byte[572763];
        for (int at = 0; at < snapshot.Length; at += 65536)
        {
            token.ThrowIfCancellationRequested(); acousticResource.Slice(at, Math.Min(65536, snapshot.Length - at)).CopyTo(snapshot.AsSpan(at));
        }
        token.ThrowIfCancellationRequested();
        Checkpoint("after-snapshot");
        if (Convert.ToHexString(SHA256.HashData(snapshot)) != AcousticHash || BinaryPrimitives.ReadUInt32LittleEndian(snapshot.AsSpan(5233)) != 0x135)
            throw new ArgumentException("Authenticated same MAIN135 snapshot required.", nameof(acousticResource));
        Checkpoint("after-hash");
        CryptographicOperations.ZeroMemory(snapshot);
        // Snapshot is not retained. Existing typed owners retain the source data.
        var envelope = root.CopyChildEnvelope(child); token.ThrowIfCancellationRequested();
        if (envelope.Length != 2580 || BinaryPrimitives.ReadUInt32LittleEndian(envelope.AsSpan(92)) != 0 ||
            BinaryPrimitives.ReadUInt32LittleEndian(envelope.AsSpan(96)) != 0x101 ||
            initial.DecodedSourceName != "ru_RU-lk-text.number") throw new InvalidDataException("Number singleton source metadata differs.");
        for (int g = 0; g < groups; g++)
        {
            token.ThrowIfCancellationRequested(); var group = ownedGroups[g];
            if (group.AllocationId != g || group.Kind07 != 0) throw new InvalidDataException("Ordinary group owner/kind differs.");
        }
        long observedArcs = 0;
        for (int n = 0; n < nodes; n++)
        {
            token.ThrowIfCancellationRequested(); var row = graph.State.GetNode(n); observedArcs = checked(observedArcs + row.Count);
            if (observedArcs > arcs) throw new InvalidDataException("Declared arc cardinality differs.");
            foreach (var arc in row)
            {
                token.ThrowIfCancellationRequested();
                if (arc.GroupId >= groups) throw new InvalidDataException("Foreign group reference.");
            }
        }
        if (observedArcs != arcs) throw new InvalidDataException("Incomplete graph arc domain.");
        Checkpoint("after-graph");
        for (ushort i = 0; i < 16; i++)
        {
            token.ThrowIfCancellationRequested();
            if (initial.ReadAuxiliarySlot(i) != (i == 0 ? null : (int?)i - 1))
                throw new InvalidDataException("Missing NUL versus retained auxiliary slots differs.");
        }
        // Original primitive owns descriptor/tag/cost interpretation; no new raw decoder.
        var seen = new bool[98]; int total = 0; bool hasNegative = false;
        for (uint state = 0; state < 98; state++)
        {
            token.ThrowIfCancellationRequested(); var rows = outgoing.Query(state, 12, 192, token);
            total = checked(total + rows.Count); if (total > 299) throw new InvalidDataException("Outgoing cardinality differs.");
            for (int i = 0; i < rows.Count; i++)
            {
                token.ThrowIfCancellationRequested(); var row = rows.GetRow(i);
                if (row.TargetState is 0 or >= 98) throw new InvalidDataException("Actual descriptor-state coverage differs.");
                seen[row.TargetState] = true; hasNegative |= row.Cost < 0;
            }
        }
        for (int state = 1; state < 98; state++)
        { token.ThrowIfCancellationRequested(); if (!seen[state]) throw new InvalidDataException("Incomplete V60 state-cost scan."); }
        if (total != 299 || hasNegative) throw new InvalidDataException("Actual number V60/cost envelope differs.");
        Checkpoint("after-scan");
        token.ThrowIfCancellationRequested();
        Checkpoint("before-publish");
        uint header = 1u | (hasNegative ? 2u : 0u);
        return new(initial, graph, work, new(header, 0, 1, 0, 1, uint.MaxValue, 1));
    }
    public LegacyFst28CNumberSourceMetadata ReadSourceMetadata(CancellationToken token = default)
    { token.ThrowIfCancellationRequested(); return metadata; }
    public LegacyFst28CBootstrapConstructorState ReadConstructorState(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        if (!ReferenceEquals(request.Cohort, cohort) || !ReferenceEquals(frontend.Cohort, cohort) ||
            !ReferenceEquals(request.Frontend, frontend) || !ReferenceEquals(frontend.Graph, Graph) ||
            !ReferenceEquals(sourceOwner, Graph.Source.Source) || !ReferenceEquals(frontend.PolicyIssuer, InitialState.TargetPreparation) ||
            !ReferenceEquals(request.Header, mapHeader)) throw new InvalidOperationException("Bootstrap cohort differs.");
        return new(true, true, mapHeader.Root is null, request.Parent is null, 0, 0, 8, 128, 0, true,
            request.ContextStart.HasValue || request.ContextLength.HasValue);
    }
    // Trusted readonly seam for the new owning active-context overlay. Existing
    // constructor views remain historical; this method does not mutate them.
    internal object RetainInitialNumberHeaderIdentity(LegacyFst28CNumberInitialD0 owner,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested(); ArgumentNullException.ThrowIfNull(owner);
        if (!ReferenceEquals(owner.Bootstrap, this) || !ReferenceEquals(owner.Graph, Graph) ||
            !ReferenceEquals(owner.InitialState, InitialState) ||
            !ReferenceEquals(owner.Policy, frontend.PolicyIssuer)) throw new ArgumentException("Exact same initial D0 owner required.");
        _ = ReadConstructorState(token); // Validates private header/frontend/source/cohort.
        return mapHeader;
    }

    internal bool IsSameIssuer(LegacyFst28CFreshPrimaryControllerBootstrap other)
        => ReferenceEquals(this, other) && ReferenceEquals(cohort, other.cohort);
}
