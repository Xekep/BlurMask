namespace InkRecognition;

/// <summary>Destination-node offset and its local native arc ordinal.</summary>
public readonly record struct LegacyCursiveCandidateSuccessor(int NodeOffset,int ArcIndex);

/// <summary>One native21b ordinary candidate arc, ending at NodeIndex.</summary>
public sealed record LegacyCursiveCandidateGroup(int GroupId, int NodeIndex, int FirstAtomIndex,
    byte Span, byte Flags, ReadOnlyMemory<int> OrderedAtomIndices,
    ReadOnlyMemory<LegacyInkRange> Ranges, ReadOnlyMemory<int> PredecessorArcIndices,
    ReadOnlyMemory<LegacyCursiveCandidateSuccessor> Successors);

/// <summary>
/// Checked single-source ordinary-chain branch of1a5920/1a3b30/1a37b0.
/// Native atom order and physical-boundary IDs are supplied upstream data.
/// Enumerates variable glyph groups; does not choose a fixed partition or infer
/// physical-boundary IDs from stroke indices. Special-token, reordered/skipping
/// seed arcs and cross-source merge branches are outside this endpoint.
/// </summary>
public sealed class LegacyCursiveCandidateLattice
{
    private readonly LegacyCursiveCandidateGroup[][] nodes;
    public int NodeCount => nodes.Length;
    public int CandidateCount { get; }
    private LegacyCursiveCandidateLattice(LegacyCursiveCandidateGroup[][] nodes)
    {this.nodes=nodes;CandidateCount=nodes.Sum(n=>n.Length);}
    public IReadOnlyList<LegacyCursiveCandidateGroup> GetNode(int nodeIndex)
    {
        if((uint)nodeIndex>=(uint)nodes.Length)throw new ArgumentOutOfRangeException(nameof(nodeIndex));
        return Array.AsReadOnly(nodes[nodeIndex]);
    }

    /// <summary>
    /// Inputs must already be in native21b line order and belong to one ordinary
    /// source ID. Explicit maximumAtoms is the native positive span cap, bounded
    /// here to15 because the downstream checked range composer supports15 pen
    /// boundaries. boundaryIds are source-service virtualf0 outputs, including
    /// unknown=-1; they are not cut tags, categories or physical stroke indices.
    /// </summary>
    public static LegacyCursiveCandidateLattice GenerateOrdinaryChain(
        ReadOnlySpan<LegacyCursiveInkAtom> orderedAtoms, ReadOnlySpan<int> boundaryIds,
        int maximumAtoms,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(orderedAtoms.Length is <1 or >4096)throw new ArgumentException("Expected1..4096 ordered atoms.",nameof(orderedAtoms));
        if(boundaryIds.Length!=orderedAtoms.Length)throw new ArgumentException("One explicit native boundary ID required per atom.",nameof(boundaryIds));
        if(maximumAtoms is <1 or >15)throw new ArgumentOutOfRangeException(nameof(maximumAtoms));
        var atoms=orderedAtoms.ToArray();var ids=boundaryIds.ToArray();
        var identities=new HashSet<(int,int)>();
        for(int i=0;i<atoms.Length;i++)
        {
            var r=atoms[i].Range;
            if(r.StrokeIndex is <0 or >255 || r.StartPoint<0 || r.EndPointExclusive<=r.StartPoint || r.EndPointExclusive>4096)
                throw new ArgumentException("Malformed prepared atom range.",nameof(orderedAtoms));
            if(!identities.Add((r.StrokeIndex,r.StartPoint)))
                throw new ArgumentException("Ambiguous native atom sort keys.",nameof(orderedAtoms));
            if(ids[i]<-1)throw new ArgumentException("Native boundary ID requires -1 or nonnegative value.",nameof(boundaryIds));
        }
        // Ordinary prepared atoms partition physical ink with at most the
        // one shared cut endpoint. Arbitrary overlapping user ranges would
        // enter native group deduplication branches outside this endpoint.
        var physicalAtoms=atoms.Select(a=>a.Range).OrderBy(r=>r.StrokeIndex).ThenBy(r=>r.StartPoint).ToArray();
        for(int i=1;i<physicalAtoms.Length;i++)
            if(physicalAtoms[i-1].StrokeIndex==physicalAtoms[i].StrokeIndex &&
                physicalAtoms[i-1].EndPointExclusive>physicalAtoms[i].StartPoint+1)
                throw new ArgumentException("Ordinary atoms may overlap only at a shared cut endpoint.",nameof(orderedAtoms));
        var starts=new List<int>[atoms.Length];var flags=new byte[atoms.Length];
        for(int i=0;i<atoms.Length;i++)
        {
            starts[i]=[i];flags[i]=4;
            if(i>0 && ids[i]!=-1)
            {
                if(ids[i]!=ids[i-1])flags[i]|=8;
                if((long)ids[i-1]+1<ids[i])flags[i]|=16;
            }
        }
        // Native1a5920 visits left nodes forward;1a3b30 appends each compatible
        // left arc to its ordinary successor, retaining encounter order.
        var groupIds=new List<int>[atoms.Length];
        for(int i=0;i<atoms.Length;i++)groupIds[i]=[i];
        int nextGroupId=atoms.Length;
        var internedRanges=new Dictionary<string,int>();
        //1a5920 initializes per-stroke merge-search hints to current group
        // count on first merge: initial singleton records are not interned.
        LegacyInkRange[] Compose(int first,int last)
        {
            var physicalOrder=Enumerable.Range(first,last-first+1).OrderBy(i=>atoms[i].Range.StrokeIndex).ThenBy(i=>atoms[i].Range.StartPoint).ToArray();
            return LegacyCursiveInkAtoms.Compose(physicalOrder.Select(i=>atoms[i].Range).ToArray(),cancellationToken);
        }
        for(int left=0;left<atoms.Length-1;left++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            int right=left+1;
            if((flags[right]&8)!=0)continue;
            foreach(int first in starts[left])
            {
                int span=right-first;
                if(span>=maximumAtoms)continue;
                var key=RangeKey(Compose(first,right));
                if(!internedRanges.TryGetValue(key,out int groupId))
                {groupId=nextGroupId++;internedRanges.Add(key,groupId);}
                starts[right].Add(first);groupIds[right].Add(groupId);
            }
        }
        var result=new LegacyCursiveCandidateGroup[atoms.Length][];
        for(int node=0;node<atoms.Length;node++)
        {
            result[node]=new LegacyCursiveCandidateGroup[starts[node].Count];
            for(int arc=0;arc<starts[node].Count;arc++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int first=starts[node][arc];int[] physicalOrder=Enumerable.Range(first,node-first+1).ToArray();
                Array.Sort(physicalOrder,(a,b)=>
                {var x=atoms[a].Range;var y=atoms[b].Range;int c=x.StrokeIndex.CompareTo(y.StrokeIndex);return c!=0?c:x.StartPoint.CompareTo(y.StartPoint);});
                var ranges=LegacyCursiveInkAtoms.Compose(physicalOrder.Select(i=>atoms[i].Range).ToArray(),cancellationToken);
                int[] predecessors=first==0 ? [-1] : Enumerable.Range(0,starts[first-1].Count).ToArray();
                // Final native successor vectors span several later nodes. A
                // linear ordinary chain links to every candidate starting just
                // after this group, using each destination node's arc ordinal.
                var successors=new List<LegacyCursiveCandidateSuccessor>();
                for(int next=node+1;next<Math.Min(atoms.Length,node+1+maximumAtoms);next++)
                    for(int a=0;a<starts[next].Count;a++)if(starts[next][a]==node+1)
                    successors.Add(new(next-node,a));
                result[node][arc]=new(groupIds[node][arc],node,first,(byte)(node-first),flags[first],
                    physicalOrder,ranges,predecessors,successors.ToArray());
            }
        }
        return new(result);
    }
    private static string RangeKey(LegacyInkRange[] ranges) =>
        string.Join(";",ranges.Select(r=>$"{r.StrokeIndex},{r.StartPoint},{r.EndPointExclusive}"));
}
