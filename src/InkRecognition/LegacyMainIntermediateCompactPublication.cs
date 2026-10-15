namespace InkRecognition;

/// <summary>Immutable copies of one actual intermediate207CB0 invocation.
/// This describes publication and live writeback, not a selected terminal or
/// text. Only the covered2478/2488 table prefix is exposed.</summary>
public sealed class LegacyMainIntermediateCompactPublication
{
    private readonly LegacyMainDirectWordNodeResult[] before, after;
    private readonly byte[] compactStorage;
    private readonly int[] starts, prefixOwners;
    public int StartingNode2490 { get; }
    public int EndingNode { get; }
    public int PreviousRecordCount2470 { get; }
    public int RecordCount2470 { get; }
    public int QueryChecks { get; }
    public int BackendCalls { get; }

    internal LegacyMainIntermediateCompactPublication(LegacyMainCompactPublicationSource source,
        int previousCount, LegacyMainCompactPublicationState state,
        LegacyOwnedMainCompactWordMaterialization.PublicationStage stage)
    {
        StartingNode2490 = source.StartingNode; EndingNode = source.EndingNode;
        PreviousRecordCount2470 = previousCount; RecordCount2470 = state.Count;
        QueryChecks = stage.Reachability.QueryChecks; BackendCalls = stage.Reachability.BackendCalls;
        before = Enumerable.Range(0, EndingNode + 1).Select(source.Snapshot).ToArray();
        after = stage.Nodes.Select(Copy).ToArray();
        compactStorage = state.Storage.ToArray();
        starts = state.Starts.AsSpan(0, EndingNode + 2).ToArray();
        prefixOwners = state.PrefixOwners.AsSpan(0, EndingNode + 2).ToArray();
    }

    public LegacyMainDirectWordNodeResult GetInputNodeSnapshot(int node) => Copy(Read(before, node));
    public LegacyMainDirectWordNodeResult GetPublishedNodeSnapshot(int node) => Copy(Read(after, node));
    public byte[] CopyCompactStorage() => compactStorage.ToArray();
    public int[] CopyNodeStarts2478() => starts.ToArray();
    public int[] CopyPrefixOwners2488() => prefixOwners.ToArray();

    private static LegacyMainDirectWordNodeResult Read(LegacyMainDirectWordNodeResult[] nodes, int node)
    {
        if ((uint)node >= nodes.Length) throw new ArgumentOutOfRangeException(nameof(node));
        return nodes[node];
    }
    private static LegacyMainDirectWordNodeResult Copy(LegacyMainDirectWordNodeResult node)
        => node with { Records = node.Records.ToArray(), PrimaryAlternatives = node.PrimaryAlternatives.ToArray() };
}
