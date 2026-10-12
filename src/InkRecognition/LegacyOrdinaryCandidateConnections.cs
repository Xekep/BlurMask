namespace InkRecognition;

/// <summary>Ordinary21b virtual50/58 successor vectors for native selector span,
/// including node-1 source root. It wraps the separately validated noNN candidate
/// graph; special/cross-source and compact-table construction are not inferred.</summary>
public sealed class LegacyOrdinaryCandidateConnections
{
    private readonly int[][][][] successors;
    private readonly int[][] root;
    private readonly int[] arcCounts;
    public int NodeCount=>arcCounts.Length;
    public LegacyOrdinaryCandidateConnections(LegacyOrdinaryCursiveCandidateGraph graph,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(graph);cancellationToken.ThrowIfCancellationRequested();
        if(graph.NodeCount is <1 or >4096||graph.CandidateCount>1_000_000)
            throw new ArgumentException("Bounded ordinary same-source candidate graph required.");
        arcCounts=new int[graph.NodeCount];successors=new int[graph.NodeCount][][][];
        var roots=Enumerable.Range(0,15).Select(_=>new List<int>()).ToArray();int entries=0;
        for(int node=0;node<graph.NodeCount;node++)
        {
            cancellationToken.ThrowIfCancellationRequested();var arcs=graph.GetNode(node);
            if(arcs.Count>ushort.MaxValue)throw new NotSupportedException("Native graph ordinal count exceeds unsigned16.");
            arcCounts[node]=arcs.Count;successors[node]=new int[arcs.Count][][];
            for(int ordinal=0;ordinal<arcs.Count;ordinal++)
            {
                var arc=arcs[ordinal];if(arc.Span>14)throw new NotSupportedException("Only the existing maximum15-atom ordinary graph is supported.");
                var vectors=Enumerable.Range(0,15).Select(_=>new List<int>()).ToArray();
                foreach(var target in arc.Successors)
                {
                    cancellationToken.ThrowIfCancellationRequested();int selector=target.NodeOffset-1;
                    if(selector is <0 or >14||node+target.NodeOffset>=graph.NodeCount||target.ArcIndex<0||
                        target.ArcIndex>=graph.GetNode(node+target.NodeOffset).Count||++entries>1_000_000)
                        throw new ArgumentException("Invalid/bounded native successor selector or ordinal.");
                    vectors[selector].Add(target.ArcIndex);
                }
                successors[node][ordinal]=vectors.Select(v=>v.ToArray()).ToArray();
                if(node==arc.Span&&arc.PredecessorArcIndices.Span.Contains(-1))
                {
                    if(++entries>1_000_000)throw new ArgumentException("Native source-root successor budget exceeded.");
                    //1a37b0 adds the newly allocated node ordinal to root when
                    //previousNode<0. Allocation ordinals increase within each
                    //node/selector; no sort or set deduplication is introduced.
                    roots[arc.Span].Add(ordinal);
                }
            }
        }
        root=roots.Select(v=>v.ToArray()).ToArray();
    }

    /// <summary>Node-1 ignores the supplied arc ordinal, exactly like the root
    /// getter. Native selector0 corresponds a future offset1, not a predecessor.</summary>
    public ReadOnlyMemory<int> GetSuccessorOrdinals(int previousNode,ushort previousArcOrdinal,
        byte currentSpanSelector,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(currentSpanSelector>14||previousNode<-1||previousNode>=NodeCount)
            throw new ArgumentOutOfRangeException(nameof(currentSpanSelector));
        if(previousNode==-1)return root[currentSpanSelector];
        if(previousArcOrdinal>=arcCounts[previousNode])throw new ArgumentOutOfRangeException(nameof(previousArcOrdinal));
        return successors[previousNode][previousArcOrdinal][currentSpanSelector];
    }

    public int GetSuccessorCount(int previousNode,ushort previousArcOrdinal,byte currentSpanSelector,
        CancellationToken cancellationToken=default)
        =>GetSuccessorOrdinals(previousNode,previousArcOrdinal,currentSpanSelector,cancellationToken).Length;

    /// <summary>1a7700 missing nth ordinal sentinelFFFF, with native list order.</summary>
    public ushort GetSuccessorOrdinal(int previousNode,ushort previousArcOrdinal,byte currentSpanSelector,
        ushort nth,CancellationToken cancellationToken=default)
    {
        var values=GetSuccessorOrdinals(previousNode,previousArcOrdinal,currentSpanSelector,cancellationToken).Span;
        return nth<values.Length?checked((ushort)values[nth]):ushort.MaxValue;
    }
}
