namespace InkRecognition;

public sealed record LegacyOrdinaryCursiveSeedArc(int SeedGroupIndex, sbyte PlacementOffset, byte Flags,
    ReadOnlyMemory<int> PredecessorArcIndices, ReadOnlyMemory<int> SuccessorArcIndices,
    uint StartContextId, uint EndContextId);

/// <summary>
/// Native1a1ef0/1a1850 seed-arc topology and1a4f10 physical-overlap flags, before
/// merged glyph candidates. One ordinary LTR source, noNN reorder, shift1,
/// unknown physical boundaries. Checked ordinary deferred interactions retain
/// their linked placement alternatives. This graph is not a linear atom lattice.
/// </summary>
public sealed class LegacyOrdinaryCursiveSeedGraph
{
    private readonly LegacyOrdinaryCursiveSeedArc[][] nodes;
    private readonly byte[] references;
    public int NodeCount => nodes.Length;
    public int ArcCount { get; }
    public ReadOnlyMemory<byte> SeedReferenceCounts => references;
    private LegacyOrdinaryCursiveSeedGraph(LegacyOrdinaryCursiveSeedArc[][] nodes, byte[] references)
    { this.nodes = nodes; this.references = references; ArcCount = nodes.Sum(n => n.Length); }
    public IReadOnlyList<LegacyOrdinaryCursiveSeedArc> GetNode(int nodeIndex)
    {
        if ((uint)nodeIndex >= (uint)nodes.Length) throw new ArgumentOutOfRangeException(nameof(nodeIndex));
        return Array.AsReadOnly(nodes[nodeIndex]);
    }

    public static LegacyOrdinaryCursiveSeedGraph CreateSingleDeferred(LegacyOrdinaryCursiveSeedResult source,
        CancellationToken cancellationToken = default)
        => Create(source, true, cancellationToken);

    /// <summary>
    /// Checked noNN/shift1 ordinary expansion, including adjacent and multiple
    /// deferred seeds. Relative ordinary/deferred orders and linked placement
    /// alternatives are retained; this does not select a winning source path.
    /// </summary>
    public static LegacyOrdinaryCursiveSeedGraph CreateOrdinaryDeferred(LegacyOrdinaryCursiveSeedResult source,
        CancellationToken cancellationToken = default)
        => Create(source, false, cancellationToken);

    private static LegacyOrdinaryCursiveSeedGraph Create(LegacyOrdinaryCursiveSeedResult source,
        bool singleDeferredOnly, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        cancellationToken.ThrowIfCancellationRequested();
        var seeds = source.Seeds.ToArray();
        if (seeds.Length is < 1 or > 4096) throw new ArgumentException("Expected1..4096 native source seeds.");
        if (singleDeferredOnly && source.DeferredCount > 1)
            throw new NotSupportedException("Multiple interacting deferred seed expansions have not been independently checked.");
        if (seeds[0].Deferred || seeds.Any(s => s.Deferred && s.InsertionDisplacement is not (-1 or 0)))
            throw new ArgumentException("Invalid checked ordinary seed placement.");
        var mutable = Enumerable.Range(0, seeds.Length).Select(_ => new List<Arc>()).ToArray();
        var referenceCounts = new byte[seeds.Length];
        Arc Add(int node, int group, sbyte offset, byte flags)
        {
            var arc = new Arc(group, offset, flags);
            mutable[node].Add(arc);
            if (referenceCounts[group] < 200) referenceCounts[group]++;
            return arc;
        }
        void Link(int node, int ordinal, int predecessor)
        {
            mutable[node][ordinal].Predecessors.Add(predecessor);
            if (node > 0) mutable[node - 1][predecessor].Successors.Add(ordinal);
        }
        void LinkPrevious(int node, int ordinal, bool previousMustBeOrdinary)
        {
            if (node == 0) { Link(node, ordinal, -1); return; }
            var previous = mutable[node - 1]; int last = previous.Count - 1;
            if (last < 0) throw new InvalidDataException("Missing native predecessor seed arc.");
            bool linkTwo = previous.Count != 1 &&
                (!previousMustBeOrdinary || !seeds[previous[last].Group].Deferred);
            Link(node, ordinal, last);
            if (linkTwo) Link(node, ordinal, last - 1);
        }
        for (int current = 0; current < seeds.Length; current++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!seeds[current].Deferred)
            {
                Add(current, current, 0, 4);
                LinkPrevious(current, mutable[current].Count - 1, false);
                continue;
            }
            //1a1ef0 visits offset0,-1,-2, ending early at the source boundary.
            for (int offset = 0; offset >= -2 && current + offset >= 0; offset--)
            {
                int node = current + offset; var arcs = mutable[node];
                if (arcs.Count != 0 && seeds[arcs[^1].Group].Deferred) break;
                byte flags = (byte)(4 | (offset == seeds[current].InsertionDisplacement ? 0 : 1));
                Add(node, current, (sbyte)offset, flags);
                LinkPrevious(node, arcs.Count - 1, true);
            }
            // Then restore displaced ordinary groups through the skip paths.
            for (int offset = 0; offset >= -1; offset--)
            {
                int node = current + offset;
                if (node <= 0) continue;
                var previous = mutable[node - 1];
                if (previous.Count == 0 || previous[^1].Group != current) continue;
                if (previous.Count < 2) throw new InvalidDataException("Missing displaced ordinary seed arc.");
                Add(node, previous[^2].Group, 0, 4); int ordinal = mutable[node].Count - 1;
                if (offset < 0) Link(node + 1, mutable[node + 1].Count - 1, ordinal);
                Link(node, ordinal, previous.Count - 1);
            }
        }
        //1a1ef0 context identities are shared by equal predecessor/successor
        // sets; they are not source timestamps, group IDs or lexical states.
        for (int node = 0; node < mutable.Length; node++)
        {
            var arcs = mutable[node]; uint next = 1;
            arcs[0].StartId = ((uint)node & 0xffffff) << 7;
            for (int ordinal = 1; ordinal < arcs.Count; ordinal++)
            {
                int same = SameSet(arcs, ordinal, true);
                arcs[ordinal].StartId = same >= 0 ? arcs[same].StartId : (((uint)node << 6 | next++ & 0x3f) * 2);
            }
            arcs[0].EndId = (((uint)node << 6 | next & 0x3f) * 2) | 1; next++;
            for (int ordinal = 1; ordinal < arcs.Count; ordinal++)
            {
                int same = SameSet(arcs, ordinal, false);
                arcs[ordinal].EndId = same >= 0 ? arcs[same].EndId : ((((uint)node << 6 | next++ & 0x3f) * 2) | 1);
            }
        }
        //1a4f10: prefix excludes this group; suffix includes it. These are
        // unions over all linked alternatives, not guessed single paths.
        var before = mutable.Select(n => n.Select(_ => new HashSet<int>()).ToArray()).ToArray();
        var after = mutable.Select(n => n.Select(_ => new HashSet<int>()).ToArray()).ToArray();
        for (int node = 1; node < mutable.Length; node++)
            for (int ordinal = 0; ordinal < mutable[node].Count; ordinal++)
                foreach (int predecessor in mutable[node][ordinal].Predecessors)
                {
                    before[node][ordinal].UnionWith(before[node - 1][predecessor]);
                    before[node][ordinal].Add(seeds[mutable[node - 1][predecessor].Group].Atom.Range.StrokeIndex);
                }
        for (int node = mutable.Length - 1; node >= 0; node--)
            for (int ordinal = 0; ordinal < mutable[node].Count; ordinal++)
            {
                after[node][ordinal].Add(seeds[mutable[node][ordinal].Group].Atom.Range.StrokeIndex);
                foreach (int successor in mutable[node][ordinal].Successors)
                    after[node][ordinal].UnionWith(after[node + 1][successor]);
            }
        for (int node = 0; node < mutable.Length; node++)
            for (int ordinal = 0; ordinal < mutable[node].Count; ordinal++)
                if (before[node][ordinal].Overlaps(after[node][ordinal])) mutable[node][ordinal].Flags &= 0xfb;
        return new(mutable.Select(n => n.Select(a => new LegacyOrdinaryCursiveSeedArc(a.Group, a.Offset, a.Flags,
            a.Predecessors.ToArray(), a.Successors.ToArray(), a.StartId, a.EndId)).ToArray()).ToArray(), referenceCounts);
    }
    private static int SameSet(List<Arc> arcs,int ordinal,bool predecessor)
    {
        var target = predecessor ? arcs[ordinal].Predecessors : arcs[ordinal].Successors;
        for (int earlier = 0; earlier < ordinal; earlier++)
        {
            var set = predecessor ? arcs[earlier].Predecessors : arcs[earlier].Successors;
            if (target.Count == set.Count && set.All(target.Contains)) return earlier;
        }
        return -1;
    }
    private sealed class Arc(int group,sbyte offset,byte flags)
    {
        public int Group { get; } = group;
        public sbyte Offset { get; } = offset;
        public byte Flags = flags;
        public List<int> Predecessors { get; } = [];
        public List<int> Successors { get; } = [];
        public uint StartId, EndId;
    }
}
