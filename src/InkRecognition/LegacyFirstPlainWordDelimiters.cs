namespace InkRecognition;

public readonly record struct LegacyFirstPlainDelimiterResult(int EmittedSymbolCount,
    int RemainingCapacity, byte ComponentFlag, byte ExternalVariant);

/// <summary>Fresh ordinary graph21b endpoint subset of frontend1a0 (152da0
/// ->1aae70). The supplied graph is the owned no-protocol ordinary producer,
/// not a snapshot with custom source tokens. Interior separators require the
/// native geometry/protocol branches and are deliberately unavailable here.</summary>
public sealed class LegacyFirstPlainWordDelimiters
{
    private readonly LegacyOrdinaryCursiveCandidateGraph graph;

    public LegacyFirstPlainWordDelimiters(LegacyOrdinaryCursiveCandidateGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);
        if (graph.NodeCount is < 1 or > 4096 || graph.CandidateCount > 131072)
            throw new ArgumentException("Bounded nonempty owned ordinary graph required.", nameof(graph));
        this.graph = graph;
    }

    /// <summary>Leading request has previousNode=-1; trailing request has
    /// currentNode=NodeCount and therefore no current cell. Ordinary metadata
    /// indices are -1 and reset fallback.count208=0: flagsC/D become zero and
    /// no token buffer write occurs. Capacity follows actual caller storage.</summary>
    public LegacyFirstPlainDelimiterResult ReadEndpoint(int previousNode, ushort previousArc,
        int currentNode, ushort currentArc, int capacity,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (capacity is < 0 or > 4096) throw new ArgumentOutOfRangeException(nameof(capacity));
        bool leading = previousNode == -1;
        bool trailing = currentNode == graph.NodeCount;
        if (!leading && !trailing)
            throw new NotSupportedException("Interior ordinary delimiter geometry requires its native producer.");
        if (leading && trailing) throw new ArgumentException("A nonempty word endpoint must retain an actual graph cell.");
        int actualNode = leading ? currentNode : previousNode;
        ushort actualArc = leading ? currentArc : previousArc;
        if ((uint)actualNode >= (uint)graph.NodeCount || actualArc >= graph.GetNode(actualNode).Count)
            throw new ArgumentException("Endpoint must belong to an actual owned ordinary graph arc.");
        return new(0, capacity, 0, 0);
    }

    /// <summary>Fresh23f ctor1edb60 clears I32FC at1edc52. Descriptor IDs in
    /// the rich materializer start from this base; subsequent increments and
    /// source-key dedup are owned by the materializer, not by this callback.</summary>
    public static int FreshDescriptorIdBase => 0;
}
