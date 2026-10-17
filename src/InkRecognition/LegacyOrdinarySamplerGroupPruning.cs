using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>The sampler297/v106 fields used by 194140. The rejection threshold
/// at11C is distinct from the118 cap applied later by195520.</summary>
public readonly record struct LegacyOrdinarySamplerGroupPruningProfile(
    LegacyCursiveSeparationProfile Separation, int RejectionThreshold)
{
    public static LegacyOrdinarySamplerGroupPruningProfile ReadV106Node(ReadOnlySpan<byte> resource, int samplerOffset)
    {
        var separation = LegacyCursiveSeparationProfile.ReadV106Node(resource, samplerOffset);
        int end = checked(samplerOffset + 8 + (int)BinaryPrimitives.ReadUInt32LittleEndian(resource[(samplerOffset + 4)..]));
        _ = LegacyCursiveCategoryProfile.Load(resource[(samplerOffset + 65)..end], samplerOffset + 65, out int categoryBytes);
        int fields = checked(samplerOffset + 65 + categoryBytes);
        if (fields + 214 != end) throw new InvalidDataException("Exact sampler106 scalar tail required.");
        int threshold = BinaryPrimitives.ReadInt32LittleEndian(resource[(fields + 202)..]);
        if (threshold < 0 || threshold > LegacyCursiveGroupSeparationCost.ImpossibleCost)
            throw new NotSupportedException("Finite nonnegative sampler11C rejection threshold required.");
        return new(separation, threshold);
    }
}

public sealed class LegacyOrdinarySamplerGroupPruningResult
{
    internal LegacyOrdinaryCursiveCandidateGraph OriginalGraph { get; }
    public LegacyOrdinaryCursiveCandidateGraph Graph { get; }
    public IReadOnlyList<ReadOnlyMemory<int>> OriginalArcOrdinals { get; }
    public int RemovedArcCount { get; }
    internal LegacyOrdinarySamplerGroupPruningResult(LegacyOrdinaryCursiveCandidateGraph original,
        LegacyOrdinaryCursiveCandidateGraph graph, IReadOnlyList<ReadOnlyMemory<int>> ordinals, int removed)
    { OriginalGraph = original; Graph = graph; OriginalArcOrdinals = ordinals; RemovedArcCount = removed; }
}

/// <summary>The ordinary no-token branch of1A61C0/1A5B00. Sampler194140
/// rejects a nonsingleton arc when its raw193860 separation exceeds11C.
/// Nodes are visited in ascending order, arcs in descending order; the final
/// node is not visited. Removing arcs also removes reciprocal adjacency.
/// This does not supply seed direction flags or replace the word scheduler.</summary>
public static class LegacyOrdinarySamplerGroupPruning
{
    public static LegacyOrdinarySamplerGroupPruningResult Prepare(
        LegacyOrdinaryCursiveCandidateGraph graph, LegacyCursivePreparedInk preparedInk,
        float actualLineHeight, LegacyOrdinarySamplerGroupPruningProfile profile,
        LegacyCursiveSeparationTable table, CancellationToken cancellationToken = default)
        => PrepareCore(graph,preparedInk,actualLineHeight,profile,table,false,cancellationToken);

    /// <summary>One ordinary descriptor's complete1A61C0→19E8B0 eligibility
    /// pass: local mode0 nodes0..N−2, no-op global mode1 for secondaryFFFF,
    /// then final-node mode0. Keeps the supplied group allocation IDs and
    /// original arc provenance; does not claim native group compaction.</summary>
    public static LegacyOrdinarySamplerGroupPruningResult PrepareSingleDescriptor(
        LegacyOrdinaryCursiveCandidateGraph graph,LegacyCursivePreparedInk preparedInk,
        float actualLineHeight,LegacyOrdinarySamplerGroupPruningProfile profile,
        LegacyCursiveSeparationTable table,CancellationToken cancellationToken=default)
        => PrepareCore(graph,preparedInk,actualLineHeight,profile,table,true,cancellationToken);

    private static LegacyOrdinarySamplerGroupPruningResult PrepareCore(
        LegacyOrdinaryCursiveCandidateGraph graph,LegacyCursivePreparedInk preparedInk,
        float actualLineHeight,LegacyOrdinarySamplerGroupPruningProfile profile,
        LegacyCursiveSeparationTable table,bool includeFinalNode,CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(preparedInk);
        ArgumentNullException.ThrowIfNull(table);
        cancellationToken.ThrowIfCancellationRequested();
        if (graph.NodeCount is < 1 or > 4096 || graph.CandidateCount > 131072 ||
            !float.IsFinite(actualLineHeight) || actualLineHeight <= 0 ||
            profile.RejectionThreshold < 0 || profile.RejectionThreshold > 0x4000000 ||
            !float.IsFinite(profile.Separation.GapOffset) || !float.IsFinite(profile.Separation.Slope) || profile.Separation.Slope < 0)
            throw new ArgumentException("Bounded ordinary graph, positive actual guide and sampler11C required.");
        _ = new LegacyOrdinaryCandidateConnections(graph, cancellationToken);
        var keep = new bool[graph.NodeCount][];
        var remap = new int[graph.NodeCount][];
        var originalOrdinals = new ReadOnlyMemory<int>[graph.NodeCount];
        var costs = new Dictionary<int, int>();
        int removed = 0;
        for (int node = 0; node < graph.NodeCount; node++)
        {
            var arcs = graph.GetNode(node);
            keep[node] = Enumerable.Repeat(true, arcs.Count).ToArray();
            remap[node] = Enumerable.Repeat(-1, arcs.Count).ToArray();
            //The complete native pass decrements an unsigned16 count and
            //tests signed16 at1A5BE6. Preserve the legacy prepared local API
            //outside that new complete-pass contract.
            bool visit=includeFinalNode||node+1<graph.NodeCount;
            if(includeFinalNode&&arcs.Count>32768)visit=false;
            if (visit)
                for (int ordinal = arcs.Count - 1; ordinal >= 0; ordinal--)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var arc = arcs[ordinal];
                    if (arc.Span == 0) continue; //194140's explicit zero-span return.
                    if (!costs.TryGetValue(arc.GroupId, out int cost))
                    {
                        if ((uint)arc.GroupId >= (uint)graph.Groups.Count)
                            throw new InvalidDataException("Ordinary arc must own an existing group.");
                        var s = profile.Separation;
                        cost = LegacyCursiveGroupSeparationCost.Evaluate(preparedInk,
                            graph.Groups[arc.GroupId].Ranges.Span, actualLineHeight,
                            s.GapOffset, s.Slope, table.Values.Span, 0x4000000,
                            cancellationToken: cancellationToken);
                        costs.Add(arc.GroupId, cost);
                    }
                    if (cost > profile.RejectionThreshold) { keep[node][ordinal] = false; removed++; }
                }
            var retained = new List<int>();
            for (int ordinal = 0; ordinal < arcs.Count; ordinal++)
                if (keep[node][ordinal]) { remap[node][ordinal] = retained.Count; retained.Add(ordinal); }
            originalOrdinals[node] = retained.ToArray();
        }
        var nodes = new LegacyOrdinaryCursiveCandidateArc[graph.NodeCount][];
        for (int node = 0; node < graph.NodeCount; node++)
        {
            var output = new List<LegacyOrdinaryCursiveCandidateArc>();
            foreach (int ordinal in originalOrdinals[node].Span)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var arc = graph.GetNode(node)[ordinal];
                int previous = node - arc.Span - 1;
                if (previous < -1) throw new InvalidDataException("Ordinary arc span precedes the native root.");
                var predecessors = new List<int>();
                foreach (int p in arc.PredecessorArcIndices.Span)
                    if (previous < 0) { if (p != -1) throw new InvalidDataException("Native root ordinal required."); predecessors.Add(-1); }
                    else if ((uint)p >= (uint)remap[previous].Length) throw new InvalidDataException("Invalid native predecessor ordinal.");
                    else if (remap[previous][p] >= 0) predecessors.Add(remap[previous][p]);
                var successors = new List<LegacyCursiveCandidateSuccessor>();
                foreach (var target in arc.Successors)
                {
                    int next = node + target.NodeOffset;
                    if ((uint)next >= (uint)remap.Length || (uint)target.ArcIndex >= (uint)remap[next].Length)
                        throw new InvalidDataException("Invalid native successor ordinal.");
                    if (remap[next][target.ArcIndex] >= 0)
                        successors.Add(new(target.NodeOffset, remap[next][target.ArcIndex]));
                }
                output.Add(arc with { PredecessorArcIndices = predecessors.ToArray(), Successors = successors.AsReadOnly() });
            }
            nodes[node] = output.ToArray();
        }
        var pruned = new LegacyOrdinaryCursiveCandidateGraph(nodes, graph.Groups.ToArray(), graph.MaximumAtoms);
        _ = new LegacyOrdinaryCandidateConnections(pruned, cancellationToken);
        return new(graph, pruned, Array.AsReadOnly(originalOrdinals), removed);
    }
}
