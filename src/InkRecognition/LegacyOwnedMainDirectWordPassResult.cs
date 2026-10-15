namespace InkRecognition;

/// <summary>Deep-owned final storage from one successful DIRECT pass. Public
/// snapshots are copies, including their ReadOnlyMemory backing arrays.</summary>
public sealed class LegacyOwnedMainDirectWordPassResult
{
    private readonly LegacyMainDirectWordNodeResult[] nodes;
    private readonly long resultQueryEpoch;
    private int compactPublicationStarted;
    internal LegacyOwnedFreshMainWordControllerPolicy ControllerOwner { get; }
    internal LegacyMainCompactPublicationState? IntermediatePublicationState { get; private set; }
    public int NodeCount => nodes.Length;
    public int IntermediatePublicationCount => IntermediatePublicationState?.PublicationCount ?? 0;
    public int NextUnpublishedNode2490 => IntermediatePublicationState?.NextNode ?? 0;
    public byte[] CopyIntermediateCompactStorage() => IntermediatePublicationState?.Storage.ToArray() ?? [];
    public int[] CopyIntermediateNodeStarts2478() => IntermediatePublicationState is { } state
        ? state.Starts.AsSpan(0, state.NextNode + 1).ToArray() : [];
    public int[] CopyIntermediatePrefixOwners2488() => IntermediatePublicationState is { } state
        ? state.PrefixOwners.AsSpan(0, state.NextNode + 1).ToArray() : [];

    private LegacyOwnedMainDirectWordPassResult(LegacyOwnedFreshMainWordControllerPolicy owner,
        LegacyMainDirectWordNodeResult[] nodes, long resultQueryEpoch)
    { ControllerOwner = owner; this.nodes = nodes; this.resultQueryEpoch = resultQueryEpoch; }

    internal static LegacyOwnedMainDirectWordPassResult Create(
        LegacyOwnedFreshMainWordControllerPolicy owner, IReadOnlyList<LegacyMainDirectWordNodeResult> completed,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (completed.Count != owner.Owner.SourceGraph.NodeCount || completed.Count == 0)
            throw new InvalidOperationException("Completed pass must cover its actual source graph.");
        var owned = new LegacyMainDirectWordNodeResult[completed.Count];
        for (int node = 0; node < owned.Length; node++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (completed[node].EndingNode != node)
                throw new InvalidOperationException("Completed node storage must retain native node order.");
            owned[node] = Copy(completed[node]);
        }
        cancellationToken.ThrowIfCancellationRequested();
        return new(owner, owned, owner.Owner.CaptureResultQueryEpoch(cancellationToken));
    }

    public LegacyMainDirectWordNodeResult GetNodeSnapshot(int endingNode)
    {
        if ((uint)endingNode >= (uint)nodes.Length)
            throw new ArgumentOutOfRangeException(nameof(endingNode));
        return Copy(nodes[endingNode]);
    }

    public int[] CopyCumulativePathCosts()
        => nodes.Select(node => node.CumulativePathCost2520).ToArray();

    internal void BindIntermediatePublicationState(LegacyMainCompactPublicationState state)
    {
        if (!ReferenceEquals(state.Controller, ControllerOwner) || IntermediatePublicationState is not null ||
            state.NextNode >= NodeCount)
            throw new ArgumentException("The completed pass requires its exact unfinished publication state.");
        IntermediatePublicationState = state;
    }

    internal ReadOnlySpan<byte> FinalRecords => nodes[^1].Records.Span;
    internal ushort FinalSourceEnd => nodes[^1].SourceEnd23f0;
    internal int FinalCumulativeCost => nodes[^1].CumulativePathCost2520;

    internal LegacyAggregateDictionaryQueryPage ReadResultQuery(ulong tag,
        LegacyPreparedDictionaryQuery query, bool auxiliary, CancellationToken token)
        => ControllerOwner.Owner.ReadResultQuery(resultQueryEpoch, tag, query, auxiliary, token);

    internal void ValidateResultQueryEpoch(CancellationToken token)
        => ControllerOwner.Owner.ValidateResultQueryEpoch(resultQueryEpoch, token);

    internal void BeginCompactWordPublication(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        ValidateResultQueryEpoch(token);
        if (Interlocked.Exchange(ref compactPublicationStarted, 1) != 0)
            throw new InvalidOperationException("Compact publication is single-use, including failed/canceled reconstruction.");
    }

    private static LegacyMainDirectWordNodeResult Copy(LegacyMainDirectWordNodeResult node)
        => node with { Records = node.Records.ToArray(), PrimaryAlternatives = node.PrimaryAlternatives.ToArray() };
}

/// <summary>Owned numeric terminal selection. No text reconstruction or
/// acceptance confidence is inferred from the selected pre-207CB0 beam row.
/// In particular Winner.RowField22 is not a restored compact record index.</summary>
public sealed class LegacyOwnedMainTerminalResult
{
    private readonly byte[] selectedRecord;
    public LegacyOwnedMainDirectWordPassResult PassResult { get; }
    public LegacyPreparedMainTerminalWinner Winner { get; }

    private LegacyOwnedMainTerminalResult(LegacyOwnedMainDirectWordPassResult pass,
        LegacyPreparedMainTerminalWinner winner, byte[] selectedRecord)
    { PassResult = pass; Winner = winner; this.selectedRecord = selectedRecord; }

    internal static LegacyOwnedMainTerminalResult Create(LegacyOwnedMainDirectWordPassResult pass,
        LegacyPreparedMainTerminalWinner winner, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        byte[] row = winner.HasWinner ? pass.FinalRecords.Slice(checked(winner.Ordinal * 88), 88).ToArray() : [];
        cancellationToken.ThrowIfCancellationRequested();
        return new(pass, winner, row);
    }

    public byte[] CopySelectedRecord() => (byte[])selectedRecord.Clone();
}
