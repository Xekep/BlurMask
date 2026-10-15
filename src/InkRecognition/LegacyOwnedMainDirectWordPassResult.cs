namespace InkRecognition;

/// <summary>Deep-owned final storage from one successful DIRECT pass. Public
/// snapshots are copies, including their ReadOnlyMemory backing arrays.</summary>
public sealed class LegacyOwnedMainDirectWordPassResult
{
    private readonly LegacyMainDirectWordNodeResult[] nodes;
    internal LegacyOwnedFreshMainWordControllerPolicy ControllerOwner { get; }
    public int NodeCount => nodes.Length;

    private LegacyOwnedMainDirectWordPassResult(LegacyOwnedFreshMainWordControllerPolicy owner,
        LegacyMainDirectWordNodeResult[] nodes)
    { ControllerOwner = owner; this.nodes = nodes; }

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
        return new(owner, owned);
    }

    public LegacyMainDirectWordNodeResult GetNodeSnapshot(int endingNode)
    {
        if ((uint)endingNode >= (uint)nodes.Length)
            throw new ArgumentOutOfRangeException(nameof(endingNode));
        return Copy(nodes[endingNode]);
    }

    public int[] CopyCumulativePathCosts()
        => nodes.Select(node => node.CumulativePathCost2520).ToArray();

    internal ReadOnlySpan<byte> FinalRecords => nodes[^1].Records.Span;
    internal ushort FinalSourceEnd => nodes[^1].SourceEnd23f0;
    internal int FinalCumulativeCost => nodes[^1].CumulativePathCost2520;

    private static LegacyMainDirectWordNodeResult Copy(LegacyMainDirectWordNodeResult node)
        => node with { Records = node.Records.ToArray(), PrimaryAlternatives = node.PrimaryAlternatives.ToArray() };
}

/// <summary>Owned numeric terminal selection. No text reconstruction or
/// acceptance confidence is inferred from the selected beam row.</summary>
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
