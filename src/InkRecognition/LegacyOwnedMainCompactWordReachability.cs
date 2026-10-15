using System.Buffers.Binary;

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
    private readonly QueryCounters counters;
    private int activeQuery;
    public LegacyOwnedMainDirectWordPassResult PassResult { get; }
    public int FinalizedPrefixEnd { get; }
    public int QueryChecks => counters.Checks;
    public int BackendCalls => counters.Calls;

    private LegacyOwnedMainCompactWordReachability(LegacyOwnedMainDirectWordPassResult pass,
        LegacyMainDirectWordNodeResult[] nodes, byte[] selectedNodes, int prefixEnd,
        QueryCounters counters, Func<int, ushort, CancellationToken, bool> languageProbe)
    {
        PassResult = pass; this.nodes = nodes; this.selectedNodes = selectedNodes;
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
        pass.ValidateResultQueryEpoch(cancellationToken);
        var policy = pass.ControllerOwner;
        var owner = policy.Owner;
        var configuration = policy.Configuration;
        if (casePolicy is null && owner.IsCaseAware)
            throw new NotSupportedException("The DIRECT reachability entrypoint does not accept a CASE source.");
        if (owner.IsCaseAware != (casePolicy is not null) ||
            casePolicy is not null && (!ReferenceEquals(casePolicy.Controller, policy) ||
                !ReferenceEquals(casePolicy.DictionaryOwner, owner.CaseDictionaryOwner)))
            throw new ArgumentException("The result requires its actual typed DIRECT or fresh CASE source policy.");
        if (configuration.Mode22f6 != 0 || configuration.Trace2368 != 0 ||
            configuration.PrimaryAlternativeCount2402 != 2 || configuration.SecondaryAlternativeCount2404 != 1 ||
            (configuration.PackedSource22e8 & 7) <= 1 || policy.SecondaryScaleEc != 0)
            throw new NotSupportedException("Fresh mode0/2/2/1 ordinary language reachability required.");
        //The exact typed source is checked again before its coordinate-mode
        //reader can transport this resource into a result query wrapper.
        var observed = LegacyOwnedFreshMainWordControllerPolicy.Prepare(resource, owner, cancellationToken);
        if (observed.Configuration != configuration || observed.Refresh != policy.Refresh)
            throw new ArgumentException("The result's exact selected main resource is required.");
        int primaryLimit = configuration.PrimaryArcLimit230a;
        int lastNode = pass.NodeCount - 1;
        //222.VA0/151FF0 ->21B.V30/1A75D0=min(node+1, graph8).
        //The owning DIRECT pass supports fresh2601=0, and no earlier207CB0
        //has advanced2490. Thus the initial materialization boundary is0.
        int prefixEnd = lastNode - Math.Min(pass.NodeCount, owner.SourceGraph.MaximumAtoms);
        var snapshots = new LegacyMainDirectWordNodeResult[pass.NodeCount];
        var activeNodes = new byte[pass.NodeCount];
        for (int node = 0; node < snapshots.Length; node++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var snapshot = pass.GetNodeSnapshot(node);
            if (snapshot.Records.Length % 88 != 0 || snapshot.SourceEnd23f0 > snapshot.Records.Length / 88 ||
                snapshot.PrimaryAlternatives.Length != checked(primaryLimit * 2 * 40))
                throw new InvalidDataException("Result rows must retain their full native source/pool extents.");
            byte[] records = snapshot.Records.ToArray(), alternatives = snapshot.PrimaryAlternatives.ToArray();
            for (int ordinal = 0; ordinal < snapshot.SourceEnd23f0; ordinal++)
                records[ordinal * 88 + 6] &= 0xfd;
            for (int ordinal = 0; ordinal < primaryLimit * 2; ordinal++)
                alternatives[ordinal * 40 + 0x1b] &= 0x7f;
            snapshots[node] = snapshot with { Records = records, PrimaryAlternatives = alternatives };
            activeNodes[node] = node > prefixEnd ? (byte)1 : (byte)0;
        }

        var reader = new LegacyNativeDelimiterQuerySource(owner.CommonModel.AcousticAlphabet, owner.Contexts,
            (tag, query, token) => pass.ReadResultQuery(tag, query, false, token),
            LegacyNativeDelimiterQuerySource.ReadV10dCoordinateMode(resource));
        //The pass has just forwarded its final sourceV80. Mirror that actual
        //wrapper reset state without resetting/replacing the shared backend.
        reader.ResetForControllerNode(cancellationToken);
        var request = new LegacyNativeDelimiterQuery
        {
            Request = new() { Capacity = 128, ContextRoot = owner.Contexts.ContainerIdentity }
        };
        var counters = new QueryCounters();
        for (int node = lastNode; node >= 0; node--)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (activeNodes[node] == 0) continue;
            for (int ordinal = 0; ordinal < snapshots[node].SourceEnd23f0; ordinal++)
            {
                var row = snapshots[node].Records.Span.Slice(ordinal * 88, 88);
                if (I32(row, 0x10) >= Impossible) continue;
                if (ordinal < primaryLimit && node <= prefixEnd && !AcceptsLanguage(node, row, cancellationToken)) continue;
                if (ordinal < primaryLimit)
                {
                    for (int alternative = 0; alternative < 2; alternative++)
                    {
                        var entry = snapshots[node].PrimaryAlternatives.Span.Slice((ordinal * 2 + alternative) * 40, 40);
                        if (I32(entry, 0) >= Impossible) break;
                        int previousNode = node, previousOrdinal = ordinal, previousAlternative = alternative;
                        while (previousNode >= 0 && previousOrdinal < primaryLimit)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            byte[] pool = GetAlternativeStorage(previousNode, previousOrdinal, previousAlternative);
                            int at = (previousOrdinal * 2 + previousAlternative) * 40;
                            if ((sbyte)pool[at + 0x1b] < 0) break;
                            pool[at + 0x1b] |= 0x80;
                            if ((pool[at + 0x1f] & 0x20) != 0) break;
                            previousOrdinal = U16(pool.AsSpan(at), 0x18);
                            previousNode = unchecked(previousNode - 1 - (sbyte)pool[at + 0x1a]);
                            previousAlternative = pool[at + 0x1b] & 0x7f;
                        }
                    }
                }
                //Packed22E8.low3>1 takes the ordinary secondary branch;
                //2404=1 selects the literal beam-chain marking loop2084BB.
                int parentNode = node, parentOrdinal = ordinal;
                while (parentNode >= 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    byte[] records = GetRecordStorage(parentNode, parentOrdinal);
                    int at = parentOrdinal * 88;
                    if (parentOrdinal < primaryLimit)
                    {
                        if ((records[at + 6] & 2) != 0) break;
                    }
                    else activeNodes[parentNode] = 1;
                    records[at + 6] |= 2;
                    parentOrdinal = U16(records.AsSpan(at), 0x1e);
                    parentNode = unchecked(parentNode - 1 - (sbyte)records[at + 0x20]);
                }
            }
        }
        pass.ValidateResultQueryEpoch(cancellationToken);
        return new(pass, snapshots, activeNodes, prefixEnd, counters,
            (node, ordinal, token) => AcceptsLanguage(node,
                snapshots[node].Records.Span.Slice(ordinal * 88, 88), token));

        byte[] GetRecordStorage(int node, int ordinal)
        {
            if ((uint)node >= snapshots.Length || (uint)ordinal >= snapshots[node].SourceEnd23f0)
                throw new InvalidDataException("Native predecessor must name an owned source row.");
            //Only private snapshot arrays are exposed to this local algorithm.
            return System.Runtime.InteropServices.MemoryMarshal.TryGetArray(snapshots[node].Records,
                out ArraySegment<byte> storage) ? storage.Array! : throw new InvalidDataException("Owned record storage absent.");
        }
        byte[] GetAlternativeStorage(int node, int ordinal, int alternative)
        {
            if ((uint)node >= snapshots.Length || (uint)ordinal >= primaryLimit || (uint)alternative >= 2)
                throw new InvalidDataException("Native primary predecessor must name its owned alternative slot.");
            return System.Runtime.InteropServices.MemoryMarshal.TryGetArray(snapshots[node].PrimaryAlternatives,
                out ArraySegment<byte> storage) ? storage.Array! : throw new InvalidDataException("Owned alternative storage absent.");
        }
        bool AcceptsLanguage(int node, ReadOnlySpan<byte> row, CancellationToken token)
        {
            pass.ValidateResultQueryEpoch(token);
            counters.Checks++;
            if ((row[6] & 1) != 0 || I32(row, 0x30) != -1) return false;
            if (((row[5] >> 1) & 1) != 0) return true;
            request.SourceTag = BinaryPrimitives.ReadUInt64LittleEndian(row);
            request.Request.Node = unchecked((int)(uint)request.SourceTag);
            request.Request.ParentContext = 0;
            request.Request.Category18 = 0;
            request.Request.Parameter1c = (uint)node;
            request.Request.Parameter20 = unchecked((uint)(int)(sbyte)row[0x20]);
            request.Request.Flags24 = 0x100;
            request.Request.Flags26 = 0; //Fresh ordinary graph kind0: no special input16 flags.
            request.Request.Skip = 0;
            while (true)
            {
                var page = reader.Read(request, token);
                counters.Calls += page.BackendCalls;
                if (page.ReturnedCount <= 0) return false;
                for (int i = 0; i < page.ReturnedCount; i++)
                {
                    //207780 starts at controller5F: query output48+17,
                    //not the distinct packed tag/extra field at row1F.
                    int kind = page.Records.Span[i * 32 + 0x17] & 7;
                    if (kind is 1 or 2) return true;
                }
                request.Request.Skip = checked(request.Request.Skip + page.ReturnedCount);
            }
        }
    }

    public LegacyMainDirectWordNodeResult GetNodeSnapshot(int endingNode)
    {
        if ((uint)endingNode >= nodes.Length) throw new ArgumentOutOfRangeException(nameof(endingNode));
        var node = nodes[endingNode];
        return node with { Records = node.Records.ToArray(), PrimaryAlternatives = node.PrimaryAlternatives.ToArray() };
    }
    public byte[] CopySelectedNodes() => (byte[])selectedNodes.Clone();
    internal bool HasLanguageContinuation(int node, ushort primaryOrdinal, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if ((uint)node >= nodes.Length || primaryOrdinal >= PassResult.ControllerOwner.Configuration.PrimaryArcLimit230a ||
            I32(nodes[node].Records.Span.Slice(primaryOrdinal * 88, 88), 0x10) >= Impossible)
            throw new ArgumentException("A finite primary row owned by this preparation is required.");
        if (Interlocked.CompareExchange(ref activeQuery, 1, 0) != 0)
            throw new InvalidOperationException("Result query calls must remain serialized.");
        try { return languageProbe(node, primaryOrdinal, token); }
        finally { Volatile.Write(ref activeQuery, 0); }
    }
    private sealed class QueryCounters { public int Checks, Calls; }
    private static int I32(ReadOnlySpan<byte> bytes, int at) => BinaryPrimitives.ReadInt32LittleEndian(bytes[at..]);
    private static ushort U16(ReadOnlySpan<byte> bytes, int at) => BinaryPrimitives.ReadUInt16LittleEndian(bytes[at..]);
}
