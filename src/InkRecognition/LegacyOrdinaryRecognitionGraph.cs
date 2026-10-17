namespace InkRecognition;

/// <summary>Internal ordinary21B graph ownership shared by source queries and
/// cache lifecycle. It accepts only existing graph producers; it does not
/// construct a same-source candidate graph or infer seed membership.</summary>
internal sealed class LegacyOrdinaryRecognitionGraph
{
    internal readonly record struct Arc(int GroupId, byte Span,byte Flags,uint StartContextId,uint EndContextId);
    private readonly IReadOnlyList<Arc>[] nodes;
    internal object Identity { get; }
    internal LegacyOrdinaryCursiveCandidateGraph? SameSource { get; }
    internal LegacyFreshMainCompactOrdinaryGraph? GlobalSource { get; }
    internal int NodeCount => nodes.Length;
    internal int MaximumAtoms=>SameSource?.MaximumAtoms??GlobalSource!.State.MaximumAtoms;
    private LegacyOrdinaryCandidateConnections? oldConnections;
    internal int GroupCount { get; }
    private LegacyOrdinaryRecognitionGraph(object identity, LegacyOrdinaryCursiveCandidateGraph? sameSource,
        LegacyFreshMainCompactOrdinaryGraph? globalSource, int groupCount, IReadOnlyList<Arc>[] nodes)
    {
        Identity = identity; SameSource = sameSource; GlobalSource = globalSource;
        GroupCount = groupCount; this.nodes = nodes;
        foreach(var row in nodes)
            foreach(var arc in row)
                if((uint)arc.GroupId>=(uint)groupCount)
                    throw new InvalidDataException("Ordinary graph arc refers to an unowned group.");
    }
    internal static LegacyOrdinaryRecognitionGraph FromSameSource(LegacyOrdinaryCursiveCandidateGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);
        var groups=graph.Groups;
        for (int g = 0; g < groups.Count; g++)
            if (groups[g].GroupId != g) throw new InvalidDataException("Ordinary graph allocation identity differs.");
        return new(graph, graph, null, groups.Count, Enumerable.Range(0, graph.NodeCount)
            .Select(n => (IReadOnlyList<Arc>)Array.AsReadOnly(graph.GetNode(n).Select(a => new Arc(a.GroupId,a.Span,a.Flags,a.StartContextId,a.EndContextId)).ToArray())).ToArray());
    }
    internal static LegacyOrdinaryRecognitionGraph FromGlobal(LegacyFreshMainCompactOrdinaryGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph);
        var groups=graph.State.Groups;
        for (int g = 0; g < groups.Count; g++)
            if (groups[g].AllocationId != g || groups[g].Kind07 != 0)
                throw new InvalidDataException("Global ordinary graph allocation identity/kind differs.");
        return new(graph, null, graph, groups.Count, Enumerable.Range(0, graph.State.NodeCount)
            .Select(n => (IReadOnlyList<Arc>)Array.AsReadOnly(graph.State.GetNode(n).Select(a => new Arc(checked((int)a.GroupId),a.Span,a.Flags,a.StartContextId,a.EndContextId)).ToArray())).ToArray());
    }
    internal IReadOnlyList<Arc> GetNode(int node)
    {
        if ((uint)node >= (uint)nodes.Length) throw new ArgumentOutOfRangeException(nameof(node));
        return nodes[node];
    }
    internal ReadOnlyMemory<int> GetSuccessorOrdinals(int previousNode,ushort previousArc,int span,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();
        if(span<0||span>=MaximumAtoms)throw new ArgumentOutOfRangeException(nameof(span));
        if(SameSource is {} old)
        {
            oldConnections??=new(old);
            return oldConnections.GetSuccessorOrdinals(previousNode,previousArc,checked((byte)span),token);
        }
        var state=GlobalSource!.State;
        var arc=previousNode==-1?state.Root:state.GetNode(previousNode)[previousArc];
        var result=arc.CopySelector(span+1).Select(i=>(int)i).ToArray();
        token.ThrowIfCancellationRequested();return result;
    }
    internal LegacyOrdinaryCursiveCandidateGraph RequireSameSource()
        => SameSource ?? throw new InvalidOperationException("This source owns a global graph, not a same-source candidate graph.");
}
