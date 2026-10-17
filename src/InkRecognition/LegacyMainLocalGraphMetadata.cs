using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMainLocalMetadata.Probe")]

namespace InkRecognition;

internal readonly record struct LegacyMainLocalAllocation(byte Kind,int Node,int Ordinal,int Group,int LeftNode,int LeftOrdinal);

/// <summary>Allocation-time ordinary19F850/1A1850/1A35A0/1A37B0 metadata.
/// Initial headers belong to supplied ordered seeds; merged headers inherit
/// their actual left allocation, not the first physical range after sorting.</summary>
internal sealed class LegacyMainLocalGraphMetadata : ILegacyOrdinaryGraphAllocations
{
    private readonly List<LegacyMainGraphAppendGroup> groups=[];
    private readonly List<byte> references=[];
    private readonly List<LegacyMainGraphAppendContext> contexts=[];
    private readonly Dictionary<(int,int),(sbyte Placement,ushort Auxiliary)> arcs=[];
    private readonly List<LegacyMainLocalAllocation> allocations=[];
    private readonly List<(int Group,int Left,int Right)> mergedGroups=[];
    private readonly List<(int Node,int Ordinal,int Group)> removals=[];
    private readonly CancellationToken token;
    private int work;
    internal IReadOnlyList<LegacyMainLocalAllocation> Allocations=>allocations.AsReadOnly();
    internal IReadOnlyList<(int Group,int Left,int Right)> MergedGroups=>mergedGroups.AsReadOnly();
    internal IReadOnlyList<(int Node,int Ordinal,int Group)> Removals=>removals.AsReadOnly();

    internal LegacyMainLocalGraphMetadata(LegacyOrdinaryCursiveSeedResult seeds,ushort descriptor,
        int physicalOffset,ReadOnlySpan<int> pendingDescriptors,CancellationToken token=default)
    {
        this.token=token;token.ThrowIfCancellationRequested();
        if(descriptor==ushort.MaxValue||physicalOffset is <0 or >255||pendingDescriptors.Length>512)
            throw new ArgumentException("Bounded actual descriptor and physical interval required.");
        byte[] prefix=new byte[16];if(pendingDescriptors.Length!=0)prefix[0]=1;
        ushort pending=LegacyMainGraphAppendContext.AppendPending(contexts,prefix,
            Enumerable.Repeat(unchecked((int)0xfffffff9),pendingDescriptors.Length).ToArray());
        foreach(var seed in seeds.Seeds)
        {
            token.ThrowIfCancellationRequested();var r=seed.Atom.Range with{StrokeIndex=checked(seed.Atom.Range.StrokeIndex+physicalOffset)};
            ushort aux=seed.SourceAtomIndex==0?pending:ushort.MaxValue;
            groups.Add(new(new(descriptor,ushort.MaxValue,aux,0,1,0,0,0),
                new(seed.MinimumX,seed.MinimumY,seed.MaximumX,seed.MaximumY),[r],[new[]{r}]));
            references.Add(0);
            work+=2;
        }
    }

    public void SeedArc(int node,int ordinal,LegacyOrdinaryCursiveSeedArc arc)
    {
        token.ThrowIfCancellationRequested();ushort aux=groups[arc.SeedGroupIndex].Fields.AuxiliaryContextIndex;
        arcs.Add((node,ordinal),(arc.PlacementOffset,aux));Increment(arc.SeedGroupIndex);
        allocations.Add(new(0,node,ordinal,arc.SeedGroupIndex,-1,-1));
    }
    public void MergedGroup(int group,int leftGroup,int rightGroup)
    {
        token.ThrowIfCancellationRequested();if(group!=groups.Count)throw new InvalidDataException("Nonsequential native group allocation.");
        var left=groups[leftGroup];var right=groups[rightGroup];
        var history=left.CopyHistory().Concat(right.CopyHistory()).ToArray();
        SortHistory(history,token);
        var ranges=LegacyCursiveInkAtoms.Compose(history.SelectMany(v=>v).ToArray(),token);
        int needed=history.Sum(v=>v.Length)+ranges.Length;
        if(work>1_048_576-needed)throw new ArgumentException("Portable local metadata range/history budget exceeded.");work+=needed;
        var a=left.Bounds;var b=right.Bounds;
        groups.Add(new(left.Fields with{SecondaryDescriptorOrdinal=ushort.MaxValue,ReferenceCounter=0,HistoryOwned08=0,
                Flags50=0,Flags51=(byte)(left.Fields.Flags51|right.Fields.Flags51),Flags52=0},
            new(Math.Min(a.MinimumX,b.MinimumX),Math.Min(a.MinimumY,b.MinimumY),Math.Max(a.MaximumX,b.MaximumX),Math.Max(a.MaximumY,b.MaximumY)),ranges,history));
        mergedGroups.Add((group,leftGroup,rightGroup));
        references.Add(0);
    }
    public void MergedArc(int node,int ordinal,int group,int leftNode,int leftOrdinal)
    {
        token.ThrowIfCancellationRequested();arcs.Add((node,ordinal),(0,arcs[(leftNode,leftOrdinal)].Auxiliary));Increment(group);
        allocations.Add(new(1,node,ordinal,group,leftNode,leftOrdinal));
    }
    private void Increment(int group)
    {
        byte count=references[group];if(count<200)references[group]=(byte)(count+1);
    }

    internal LegacyMainGraphAppendState Complete(LegacyOrdinarySamplerGroupPruningResult pruning,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();var original=pruning.OriginalGraph;var graph=pruning.Graph;
        if(original.Groups.Count!=groups.Count||original.CandidateCount!=arcs.Count)throw new ArgumentException("Same allocation/pruning owner required.");
        //1A5B00 deletes in node-ascending/ordinal-descending order. The prune
        //projection preserves each retained original ordinal; no set sorting
        //or post-hoc reference recount can erase sticky200.
        for(int node=0;node<original.NodeCount;node++)
        {
            var keep=pruning.OriginalArcOrdinals[node].ToArray().ToHashSet();
            for(int ordinal=original.GetNode(node).Count-1;ordinal>=0;ordinal--)
                if(!keep.Contains(ordinal))
                {
                    cancellationToken.ThrowIfCancellationRequested();int id=original.GetNode(node)[ordinal].GroupId;
                    if(references[id]<200)
                    {
                        if(references[id]==0)throw new InvalidDataException("Reference byte underflow.");
                        references[id]--;
                    }
                    removals.Add((node,ordinal,id));
                }
        }
        var nodes=new LegacyMainGraphAppendArc[graph.NodeCount][];int cap=graph.MaximumAtoms;
        for(int node=0;node<nodes.Length;node++)
        {
            nodes[node]=new LegacyMainGraphAppendArc[graph.GetNode(node).Count];
            for(int ordinal=0;ordinal<nodes[node].Length;ordinal++)
            {
                cancellationToken.ThrowIfCancellationRequested();var a=graph.GetNode(node)[ordinal];
                var metadata=arcs[(node,pruning.OriginalArcOrdinals[node].Span[ordinal])];
                var next=Enumerable.Range(0,cap).Select(s=>a.Successors.Where(v=>v.NodeOffset==s+1).Select(v=>v.ArcIndex).ToArray()).ToArray();
                nodes[node][ordinal]=new(checked((uint)a.GroupId),a.Span,metadata.Placement,a.Flags,a.StartContextId,a.EndContextId,
                    metadata.Auxiliary,0,0,a.PredecessorArcIndices.Span,next);
            }
        }
        var connections=new LegacyOrdinaryCandidateConnections(graph,cancellationToken);
        var root=new LegacyMainGraphAppendArc(0,0,0,4,0,0,ushort.MaxValue,0,0,[],
            Enumerable.Range(0,cap).Select(s=>connections.GetSuccessorOrdinals(-1,0,(byte)s,cancellationToken).ToArray()).ToArray());
        var finalized=groups.Select((g,i)=>new LegacyMainGraphAppendGroup(g.Fields with{ReferenceCounter=references[i]},g.Bounds,g.CopyRanges(),g.CopyHistory())).ToArray();
        return LegacyMainGraphAppendState.Create(cap,finalized,contexts,nodes,root,cancellationToken);
    }

    //Native1A3450 compares the first range's signed physical/start pair and
    //swaps equal entries. Middle-pivot quicksort order is observable in history.
    internal static void SortHistory(LegacyInkRange[][] values,CancellationToken token=default)
    {
        if(values.Any(v=>v.Length==0))throw new ArgumentException("Nonempty history entries required.");
        if(values.Length>1)Sort(0,values.Length-1);
        void Sort(int first,int last)
        {
            do
            {
                token.ThrowIfCancellationRequested();int i=first,j=last;var pivot=values[(first+last)/2][0];
                do
                {
                    while(Compare(values[i][0],pivot)<0)i++;
                    while(Compare(values[j][0],pivot)>0)j--;
                    if(i>j)break;(values[i],values[j])=(values[j],values[i]);i++;j--;
                }while(i<=j);
                if(first<j)Sort(first,j);if(last<=i)return;first=i;
            }while(true);
        }
        static int Compare(LegacyInkRange a,LegacyInkRange b)=>a.StrokeIndex!=b.StrokeIndex?a.StrokeIndex.CompareTo(b.StrokeIndex):a.StartPoint.CompareTo(b.StartPoint);
    }
}
