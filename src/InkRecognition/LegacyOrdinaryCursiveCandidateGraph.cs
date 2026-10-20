namespace InkRecognition;

public sealed record LegacyOrdinaryCursiveCandidateInk(int GroupId, ReadOnlyMemory<int> SeedGroupIndices,
    ReadOnlyMemory<LegacyInkRange> Ranges);

public sealed record LegacyOrdinaryCursiveCandidateArc(int GroupId, byte Span, byte Flags,
    uint StartContextId, uint EndContextId, ReadOnlyMemory<int> PredecessorArcIndices,
    IReadOnlyList<LegacyCursiveCandidateSuccessor> Successors);

/// <summary>
/// Ordinary same-source noNN span-merging consumer1a5920/1a3b30/1a37b0 over
/// checked seed graph topology. Placement offsets do not replace glyph spans.
/// Special/cross-source token and geometry branches remain outside this API.
/// </summary>
public sealed class LegacyOrdinaryCursiveCandidateGraph
{
    private readonly LegacyOrdinaryCursiveCandidateArc[][] nodes;
    private readonly LegacyOrdinaryCursiveCandidateInk[] groups;
    public int NodeCount => nodes.Length;
    public int CandidateCount { get; }
    /// <summary>Owned span cap used by this graph's Generate call. Native
    /// graph30 lookahead must use the same cap, not a caller's unrelated limit.</summary>
    public int MaximumAtoms { get; }
    public IReadOnlyList<LegacyOrdinaryCursiveCandidateInk> Groups => Array.AsReadOnly(groups);
    internal LegacyOrdinaryCursiveCandidateGraph(LegacyOrdinaryCursiveCandidateArc[][] nodes,
        LegacyOrdinaryCursiveCandidateInk[] groups, int maximumAtoms)
    { this.nodes = nodes; this.groups = groups; MaximumAtoms = maximumAtoms; CandidateCount = nodes.Sum(n => n.Length); }
    public IReadOnlyList<LegacyOrdinaryCursiveCandidateArc> GetNode(int nodeIndex)
    {
        if ((uint)nodeIndex >= (uint)nodes.Length) throw new ArgumentOutOfRangeException(nameof(nodeIndex));
        return Array.AsReadOnly(nodes[nodeIndex]);
    }

    public static LegacyOrdinaryCursiveCandidateGraph Generate(LegacyOrdinaryCursiveSeedResult source,
        LegacyOrdinaryCursiveSeedGraph seedGraph, int maximumAtoms, int maximumCandidates = 131072,
        CancellationToken cancellationToken = default)
        => GenerateWithAllocations(source,seedGraph,maximumAtoms,maximumCandidates,null,cancellationToken);

    internal static LegacyOrdinaryCursiveCandidateGraph GenerateWithAllocations(LegacyOrdinaryCursiveSeedResult source,
        LegacyOrdinaryCursiveSeedGraph seedGraph,int maximumAtoms,int maximumCandidates,
        ILegacyOrdinaryGraphAllocations? allocations,CancellationToken cancellationToken=default,
        LegacyOrdinaryGraphWorkBudget? workBudget=null)
    {
        ArgumentNullException.ThrowIfNull(source); ArgumentNullException.ThrowIfNull(seedGraph);
        cancellationToken.ThrowIfCancellationRequested();
        if (maximumAtoms is < 1 or > 15 || maximumCandidates is < 1 or > 1_000_000)
            throw new ArgumentOutOfRangeException(nameof(maximumAtoms));
        var seeds = source.Seeds.ToArray();
        if (seeds.Length != seedGraph.NodeCount) throw new ArgumentException("Source seed ownership does not match the supplied graph.");
        workBudget?.InitialGroups(seeds.Length);
        var inkGroups = seeds.Select((s, i) => new LegacyOrdinaryCursiveCandidateInk(i, new[] { i }, new[] { s.Atom.Range })).ToList();
        var mutable = new List<Arc>[seedGraph.NodeCount]; int candidateCount = seedGraph.ArcCount;
        if (candidateCount > maximumCandidates) throw new InvalidOperationException("Candidate graph exceeds its explicit budget.");
        if(workBudget is not null)
        {
            long initialEdges=0;for(int node=0;node<seedGraph.NodeCount;node++)foreach(var arc in seedGraph.GetNode(node))
            {cancellationToken.ThrowIfCancellationRequested();initialEdges=checked(initialEdges+arc.PredecessorArcIndices.Length+arc.SuccessorArcIndices.Length);}
            workBudget.InitialEdges(initialEdges);
        }
        for (int node = 0; node < mutable.Length; node++)
        {
            mutable[node] = [];
            foreach (var seed in seedGraph.GetNode(node))
            {
                if ((uint)seed.SeedGroupIndex >= (uint)seeds.Length) throw new ArgumentException("Invalid source group identity.");
                var arc = new Arc(seed.SeedGroupIndex, 0, seed.Flags, seed.StartContextId, seed.EndContextId);
                arc.Predecessors.AddRange(seed.PredecessorArcIndices.ToArray());
                arc.Successors.Add(1, seed.SuccessorArcIndices.ToArray().ToList()); mutable[node].Add(arc);
            }
        }
        if(allocations is not null)
            foreach(var entry in seedGraph.AllocationOrder)
                allocations.SeedArc(entry.Node,entry.Ordinal,seedGraph.GetNode(entry.Node)[entry.Ordinal]);
        // Native per-physical merge hints skip initial singleton records. Only
        // newly composed groups participate in this range-identity interning.
        var mergedIds = new Dictionary<string, int>();
        for (int leftNode = 0; leftNode < mutable.Length - 1; leftNode++)
            for (int leftOrdinal = 0; leftOrdinal < mutable[leftNode].Count; leftOrdinal++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var left = mutable[leftNode][leftOrdinal];
                if (left.Span + 1 >= maximumAtoms) continue;
                int rightNode = leftNode + 1;
                if (!left.Successors.TryGetValue(1, out var children)) continue;
                foreach (int rightOrdinal in children.ToArray())
                {
                    var right = mutable[rightNode][rightOrdinal];
                    if ((right.Flags & 8) != 0) continue;
                    if ((right.Flags & 2) != 0 || right.Span != 0)
                        throw new NotSupportedException("Checked ordinary merge requires an unsuppressed singleton right seed.");
                    var leftInk = inkGroups[left.Group]; var rightInk = inkGroups[right.Group];
                    workBudget?.Composition(checked(leftInk.SeedGroupIndices.Length+rightInk.SeedGroupIndices.Length));
                    int[] ownedSeeds = leftInk.SeedGroupIndices.Span.ToArray().Concat(rightInk.SeedGroupIndices.Span.ToArray())
                        .OrderBy(i => seeds[i].Atom.Range.StrokeIndex).ThenBy(i => seeds[i].Atom.Range.StartPoint).ToArray();
                    if (ownedSeeds.Distinct().Count() != ownedSeeds.Length)
                        throw new InvalidDataException("Native graph candidate repeated source atom ownership.");
                    var ranges = LegacyCursiveInkAtoms.Compose(ownedSeeds.Select(i => seeds[i].Atom.Range).ToArray(), cancellationToken);
                    string key = string.Join(";", ranges.Select(r => $"{r.StrokeIndex},{r.StartPoint},{r.EndPointExclusive}"));
                    if (!mergedIds.TryGetValue(key, out int groupId))
                    {
                        workBudget?.Groups(ownedSeeds.Length,ranges.Length);
                        groupId = inkGroups.Count; mergedIds.Add(key, groupId);
                        inkGroups.Add(new(groupId, ownedSeeds, ranges));
                        allocations?.MergedGroup(groupId,left.Group,right.Group);
                    }
                    var rightSuccessors = right.Successors.GetValueOrDefault(1) ?? [];
                    Arc? existing = null;
                    foreach (var arc in mutable[rightNode])
                        if (arc.Group == groupId && left.Predecessors.All(arc.Predecessors.Contains) &&
                            rightSuccessors.All((arc.Successors.GetValueOrDefault(1) ?? []).Contains)) { existing = arc; break; }
                    if (existing is not null)
                    {
                        if ((left.Flags & 1) == 0 || (right.Flags & 1) == 0) existing.Flags &= 0xfe;
                        continue;
                    }
                    if (++candidateCount > maximumCandidates)
                        throw new InvalidOperationException("Candidate graph exceeded its explicit merge budget.");
                    byte flags = (byte)((left.Flags & 0x1e) | ((left.Flags & right.Flags) & 1));
                    workBudget?.MergedEdges(left.Predecessors.Count,rightSuccessors.Count);
                    var merged = new Arc(groupId, checked((byte)(left.Span + 1)), flags, left.StartId, right.EndId);
                    merged.Predecessors.AddRange(left.Predecessors); merged.Successors.Add(1, rightSuccessors.ToList());
                    int mergedOrdinal = mutable[rightNode].Count; mutable[rightNode].Add(merged);
                    allocations?.MergedArc(rightNode,mergedOrdinal,groupId,leftNode,leftOrdinal);
                    if (rightNode + 1 < mutable.Length)
                        foreach (int next in rightSuccessors) mutable[rightNode + 1][next].Predecessors.Add(mergedOrdinal);
                    int precedingNode = leftNode - left.Span - 1;
                    if (precedingNode >= 0)
                        foreach (int preceding in left.Predecessors)
                        {
                            var vectors = mutable[precedingNode][preceding].Successors;
                            int offset = merged.Span + 1;
                            if (!vectors.TryGetValue(offset, out var successors)) { successors = []; vectors.Add(offset, successors); }
                            successors.Add(mergedOrdinal);
                        }
                }
            }
        workBudget?.FinalCopies(mutable.Length,inkGroups.Count,candidateCount);
        var result = mutable.Select(n => n.Select(a => new LegacyOrdinaryCursiveCandidateArc(a.Group, a.Span, a.Flags,
            a.StartId, a.EndId, a.Predecessors.ToArray(), Array.AsReadOnly(a.Successors.OrderBy(v => v.Key)
                .SelectMany(v => v.Value.Select(i => new LegacyCursiveCandidateSuccessor(v.Key, i))).ToArray()))).ToArray()).ToArray();
        return new(result, inkGroups.ToArray(), maximumAtoms);
    }
    private sealed class Arc(int group,byte span,byte flags,uint start,uint end)
    {
        public int Group { get; } = group;
        public byte Span { get; } = span;
        public byte Flags = flags;
        public uint StartId { get; } = start;
        public uint EndId { get; } = end;
        public List<int> Predecessors { get; } = [];
        public Dictionary<int,List<int>> Successors { get; } = [];
    }
}
