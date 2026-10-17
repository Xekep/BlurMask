using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Source-owned first phase of207CB0, before primary candidate
/// expansion, compact40 allocation or row22 publication. Public snapshots copy
/// the complete node/alternative storage; bit changes have native provenance.</summary>
public sealed class LegacyOwnedMainCompactWordReachability
{
    private const int Impossible = 0x4000000;
    private readonly LegacyMainDirectWordNodeResult[] nodes;
    private readonly byte[] selectedNodes;
    private readonly Func<int, ushort, CancellationToken, bool> languageProbe;
    private readonly LegacyPreparedMainCompactReachability.QueryCounters counters;
    private int activeQuery;
    private readonly LegacyMainCompactPublicationSource source;
    public LegacyOwnedMainDirectWordPassResult PassResult => source.FinalPass ??
        throw new InvalidOperationException("Intermediate publication has no manufactured final pass result.");
    public int FinalizedPrefixEnd { get; }
    public int QueryChecks => counters.Checks;
    public int BackendCalls => counters.Calls;

    private LegacyOwnedMainCompactWordReachability(LegacyMainCompactPublicationSource source,
        LegacyMainDirectWordNodeResult[] nodes, byte[] selectedNodes, int prefixEnd,
        LegacyPreparedMainCompactReachability.QueryCounters counters, Func<int, ushort, CancellationToken, bool> languageProbe)
    {
        this.source = source; this.nodes = nodes; this.selectedNodes = selectedNodes;
        FinalizedPrefixEnd = prefixEnd; this.counters = counters; this.languageProbe = languageProbe;
    }

    /// <summary>The fresh DIRECT mode0/2/2/1 path. Reads the producing source's
    /// same post-V80 backend; it never refreshes permissions or replays D0.
    /// This does not produce a compact record or a reconstructed word.</summary>
    public static LegacyOwnedMainCompactWordReachability Prepare(ReadOnlySpan<byte> resource,
        LegacyOwnedMainTerminalResult terminal, CancellationToken cancellationToken = default)
        => PrepareCore(resource, terminal, null, cancellationToken);

    internal static LegacyOwnedMainCompactWordReachability PrepareCase(ReadOnlySpan<byte> resource,
        LegacyOwnedMainTerminalResult terminal, LegacyFreshCaseMainSourcePolicy sourcePolicy,
        CancellationToken cancellationToken)
        => PrepareCore(resource, terminal, sourcePolicy, cancellationToken);

    private static LegacyOwnedMainCompactWordReachability PrepareCore(ReadOnlySpan<byte> resource,
        LegacyOwnedMainTerminalResult terminal, LegacyFreshCaseMainSourcePolicy? casePolicy,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        cancellationToken.ThrowIfCancellationRequested();
        var pass = terminal.PassResult;
        //Reject foreign bytes before parsing coordinate mode or touching the
        //producing query lease. Invalid resources must not consume result state.
        if (resource.Length != 572763 || !StringComparer.OrdinalIgnoreCase.Equals(
            Convert.ToHexString(SHA256.HashData(resource)), pass.ControllerOwner.Owner.SelectedSourceOwner.SourceFingerprint))
            throw new ArgumentException("Reachability must use its exact selected main resource.", nameof(resource));
        pass.ValidateResultQueryEpoch(cancellationToken);
        var source = LegacyMainCompactPublicationSource.Final(resource, pass, casePolicy, 0, cancellationToken);
        return PrepareSource(resource, source, cancellationToken);
    }

    internal static LegacyOwnedMainCompactWordReachability PrepareSource(ReadOnlySpan<byte> resource,
        LegacyMainCompactPublicationSource source, CancellationToken cancellationToken, bool configuredSecondary = false)
    {
        source.Validate(cancellationToken);
        var policy = source.Controller;
        var owner = source.Owner;
        var casePolicy = source.CasePolicy;
        var configuration = policy.Configuration;
        if (casePolicy is null && owner.IsCaseAware)
            throw new NotSupportedException("The DIRECT reachability entrypoint does not accept a CASE source.");
        if (owner.IsCaseAware != (casePolicy is not null) ||
            casePolicy is not null && (!ReferenceEquals(casePolicy.Controller, policy) ||
                !ReferenceEquals(casePolicy.DictionaryOwner, owner.CaseDictionaryOwner)))
            throw new ArgumentException("The result requires its actual typed DIRECT or fresh CASE source policy.");
        if (configuration.Mode22f6 != 0 || configuration.Trace2368 != 0 ||
            configuration.PrimaryAlternativeCount2402 != 2 ||
            configuration.SecondaryAlternativeCount2404 != (configuredSecondary ? 2 : 1) ||
            (configuration.PackedSource22e8 & 7) <= 1 || policy.SecondaryScaleEc != 0)
            throw new NotSupportedException(configuredSecondary
                ? "Fresh mode0/2/2/2 ordinary configured language reachability required."
                : "Fresh mode0/2/2/1 ordinary language reachability required.");
        //The exact typed source is checked again before its coordinate-mode
        //reader can transport this resource into a result query wrapper.
        if (configuredSecondary && (!owner.IsCaseAware || policy.SecondaryCountProperty12d0003 != 2))
            throw new NotSupportedException("The configured CASE branch requires its actual preconstruction property2 owner.");
        var observed = configuredSecondary
            ? LegacyOwnedFreshMainWordControllerPolicy.PrepareWithSecondaryCountProperty(resource, owner, 2, cancellationToken)
            : LegacyOwnedFreshMainWordControllerPolicy.Prepare(resource, owner, cancellationToken);
        if (observed.Configuration != configuration || observed.Refresh != policy.Refresh)
            throw new ArgumentException("The result's exact selected main resource is required.");
        var prepared=LegacyPreparedMainCompactReachability.Run(configuration,owner.SourceGraph.MaximumAtoms,
            source.StartingNode,source.EndingNode,source.Snapshot,source.Reader,owner.Contexts.ContainerIdentity,
            source.Validate,configuredSecondary,cancellationToken);
        return new(source,prepared.Nodes,prepared.SelectedNodes,prepared.PrefixEnd,prepared.Counters,prepared.LanguageProbe);
    }

    public LegacyMainDirectWordNodeResult GetNodeSnapshot(int endingNode)
    {
        if ((uint)endingNode >= nodes.Length) throw new ArgumentOutOfRangeException(nameof(endingNode));
        var node = nodes[endingNode];
        return node with { Records = node.Records.ToArray(), PrimaryAlternatives = node.PrimaryAlternatives.ToArray(),
            SecondaryAlternatives = node.SecondaryAlternatives.ToArray() };
    }
    public byte[] CopySelectedNodes() => (byte[])selectedNodes.Clone();
    internal bool HasLanguageContinuation(int node, ushort primaryOrdinal, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if ((uint)node >= nodes.Length || primaryOrdinal >= source.Controller.Configuration.PrimaryArcLimit230a ||
            I32(nodes[node].Records.Span.Slice(primaryOrdinal * 88, 88), 0x10) >= Impossible)
            throw new ArgumentException("A finite primary row owned by this preparation is required.");
        if (Interlocked.CompareExchange(ref activeQuery, 1, 0) != 0)
            throw new InvalidOperationException("Result query calls must remain serialized.");
        try { return languageProbe(node, primaryOrdinal, token); }
        finally { Volatile.Write(ref activeQuery, 0); }
    }
    private static int I32(ReadOnlySpan<byte> bytes, int at) => BinaryPrimitives.ReadInt32LittleEndian(bytes[at..]);
    private static ushort U16(ReadOnlySpan<byte> bytes, int at) => BinaryPrimitives.ReadUInt16LittleEndian(bytes[at..]);
}
