using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMainGraphAppend.Probe")]

namespace InkRecognition;

public readonly record struct LegacyMainGraphAppendBounds(float MinimumX,float MinimumY,float MaximumX,float MaximumY);
public readonly record struct LegacyMainGraphAppendGroupFields(ushort DescriptorOrdinal,ushort SecondaryDescriptorOrdinal,
    ushort AuxiliaryContextIndex,byte ReferenceCounter,byte HistoryOwned08,byte Flags50,byte Flags51,byte Flags52);

/// <summary>Owned ordinary kind0 group fields and both native range vectors.
/// No raw96-byte pointer layout or unspecified padding is manufactured.</summary>
public sealed class LegacyMainGraphAppendGroup
{
    private readonly LegacyInkRange[] ranges;
    private readonly LegacyInkRange[][] history;
    public LegacyMainGraphAppendGroupFields Fields {get;}
    public LegacyMainGraphAppendBounds Bounds {get;}
    internal int HistoryCount=>history.Length;
    internal int RangeCount=>ranges.Length;
    internal int HistoryRangeCount=>history.Sum(v=>v.Length);
    internal LegacyMainGraphAppendGroup(LegacyMainGraphAppendGroupFields fields,LegacyMainGraphAppendBounds bounds,
        ReadOnlySpan<LegacyInkRange> ranges,IReadOnlyList<LegacyInkRange[]> history)
    {
        ArgumentNullException.ThrowIfNull(history);int total=ranges.Length;
        if(ranges.Length>4096||history.Count>4096)throw new ArgumentException("Bounded ordinary range/history vectors required.");
        foreach(var h in history){if(h is null||h.Length>4096||total>1_048_576-h.Length)throw new ArgumentException("Range/history work budget exceeded.");total+=h.Length;}
        Fields=fields;Bounds=bounds;this.ranges=ranges.ToArray();this.history=history.Select(v=>v.ToArray()).ToArray();
    }
    public LegacyInkRange[] CopyRanges()=>ranges.ToArray();
    public LegacyInkRange[][] CopyHistory()=>history.Select(v=>v.ToArray()).ToArray();
    internal LegacyMainGraphAppendGroup Clone()=>new(Fields,Bounds,ranges,history);
    internal LegacyMainGraphAppendGroup ShiftAuxiliary(int offset)=>new(Fields with{
        AuxiliaryContextIndex=Fields.AuxiliaryContextIndex==ushort.MaxValue?ushort.MaxValue:checked((ushort)(Fields.AuxiliaryContextIndex+offset))},Bounds,ranges,[]);
    internal LegacyMainGraphAppendGroup WithHistory(LegacyMainGraphAppendGroup source)=>new(Fields with{HistoryOwned08=1},Bounds,ranges,source.history);
}

/// <summary>19EB10's pointer-free16-byte prefix and ordered control vector.
/// Native vector pointers/capacity are represented by owned managed storage.</summary>
public sealed class LegacyMainGraphAppendContext
{
    private readonly byte[] prefix;
    private readonly int[] controls;
    internal int ControlCount=>controls.Length;
    internal LegacyMainGraphAppendContext(ReadOnlySpan<byte> prefix,ReadOnlySpan<int> controls)
    {if(prefix.Length!=16||controls.Length>4096)throw new ArgumentException("Bounded native context prefix/control vector required.");this.prefix=prefix.ToArray();this.controls=controls.ToArray();}
    public byte[] CopyPrefix()=>prefix.ToArray();
    public int[] CopyControls()=>controls.ToArray();
    internal LegacyMainGraphAppendContext Clone()=>new(prefix,controls);
    internal static ushort AppendPending(List<LegacyMainGraphAppendContext> target,ReadOnlySpan<byte> pendingPrefix,
        ReadOnlySpan<int> controls)
    {
        if(pendingPrefix.Length!=16)throw new ArgumentException("Complete pointer-free pending prefix required.");
        if(pendingPrefix[0]==0&&pendingPrefix[1]==0&&pendingPrefix[2]==0&&pendingPrefix[3]==0)return ushort.MaxValue;
        if(target.Count>=65535)throw new ArgumentException("Portable auxiliary ordinal budget exceeded.");
        //19EB10 zeroes its output, then copies flags0..5 and scalar8..B.
        byte[] prefix=new byte[16];pendingPrefix[..6].CopyTo(prefix);pendingPrefix.Slice(8,4).CopyTo(prefix.AsSpan(8));
        ushort result=checked((ushort)target.Count);target.Add(new(prefix,controls));return result;
    }
}

/// <summary>Owned32-byte arc's semantic fields and ordered vectors. On the root
/// record GroupId is the opaque native field00, not a claimed ink-group owner.</summary>
public sealed class LegacyMainGraphAppendArc
{
    private readonly int[] predecessors;
    private readonly int[][] successors;
    public uint GroupId {get;}
    public byte Span {get;}
    public sbyte Placement {get;}
    public byte Flags {get;}
    public uint StartContextId {get;}
    public uint EndContextId {get;}
    public ushort AuxiliaryContextIndex {get;}
    public ushort Extra1a {get;}
    public uint Scalar1cBits {get;}
    internal int[] Predecessors=>predecessors;
    internal int[][] Successors=>successors;
    internal LegacyMainGraphAppendArc(uint group,byte span,sbyte placement,byte flags,uint start,uint end,
        ushort auxiliary,ushort extra,uint scalar,ReadOnlySpan<int> predecessors,IReadOnlyList<int[]> successors)
    {
        ArgumentNullException.ThrowIfNull(successors);int entries=predecessors.Length;
        if(entries>1_048_576||successors.Count>15)throw new ArgumentException("Bounded graph vectors required.");
        foreach(var v in successors){if(v is null||entries>1_048_576-v.Length)throw new ArgumentException("Graph vector budget exceeded.");entries+=v.Length;}
        GroupId=group;Span=span;Placement=placement;Flags=flags;StartContextId=start;EndContextId=end;
        AuxiliaryContextIndex=auxiliary;Extra1a=extra;Scalar1cBits=scalar;this.predecessors=predecessors.ToArray();this.successors=successors.Select(v=>v.ToArray()).ToArray();}
    public int[] CopyPredecessors()=>predecessors.ToArray();
    public int[][] CopySuccessors()=>successors.Select(v=>v.ToArray()).ToArray();
    internal LegacyMainGraphAppendArc Copy(uint group,int endpointOffset,int auxiliaryOffset)=>new(group,Span,Placement,Flags,
        unchecked(StartContextId+(uint)endpointOffset),unchecked(EndContextId+(uint)endpointOffset),
        AuxiliaryContextIndex==ushort.MaxValue?ushort.MaxValue:checked((ushort)(AuxiliaryContextIndex+auxiliaryOffset)),
        Extra1a,Scalar1cBits,predecessors,successors);
    internal LegacyMainGraphAppendArc WithConnections(int[] previous,int[][] next)=>new(GroupId,Span,Placement,Flags,StartContextId,EndContextId,
        AuxiliaryContextIndex,Extra1a,Scalar1cBits,previous,next);
}

/// <summary>Append-stage ordinary state. It deliberately has no classifier-facing
/// Graph projection: cross-input merge and boundary pruning have not run.</summary>
public sealed class LegacyMainGraphAppendState
{
    private readonly LegacyMainGraphAppendGroup[] groups;
    private readonly LegacyMainGraphAppendContext[] contexts;
    private readonly LegacyMainGraphAppendArc[][] nodes;
    public int MaximumAtoms {get;}
    public int NodeCount=>nodes.Length;
    public int GroupCount=>groups.Length;
    public int ContextCount=>contexts.Length;
    public LegacyMainGraphAppendArc Root {get;}
    public IReadOnlyList<LegacyMainGraphAppendGroup> Groups=>Array.AsReadOnly(groups);
    public IReadOnlyList<LegacyMainGraphAppendContext> Contexts=>Array.AsReadOnly(contexts);
    private LegacyMainGraphAppendState(int maximumAtoms,LegacyMainGraphAppendGroup[] groups,
        LegacyMainGraphAppendContext[] contexts,LegacyMainGraphAppendArc[][] nodes,LegacyMainGraphAppendArc root)
    {MaximumAtoms=maximumAtoms;this.groups=groups;this.contexts=contexts;this.nodes=nodes;Root=root;}
    public IReadOnlyList<LegacyMainGraphAppendArc> GetNode(int node)
    {if((uint)node>=(uint)nodes.Length)throw new ArgumentOutOfRangeException(nameof(node));return Array.AsReadOnly(nodes[node]);}

    internal static LegacyMainGraphAppendState Empty(int maximumAtoms)=>Create(maximumAtoms,[],[],[],
        new(0,0,0,4,0,0,ushort.MaxValue,0,0,[],Enumerable.Range(0,maximumAtoms).Select(_=>Array.Empty<int>()).ToArray()));

    internal static LegacyMainGraphAppendState Create(int maximumAtoms,IReadOnlyList<LegacyMainGraphAppendGroup> groups,
        IReadOnlyList<LegacyMainGraphAppendContext> contexts,IReadOnlyList<LegacyMainGraphAppendArc[]> nodes,
        LegacyMainGraphAppendArc root,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(groups);ArgumentNullException.ThrowIfNull(contexts);ArgumentNullException.ThrowIfNull(nodes);ArgumentNullException.ThrowIfNull(root);
        token.ThrowIfCancellationRequested();
        if(maximumAtoms is <1 or >15||groups.Count>131072||contexts.Count>65535||nodes.Count>4096)
            throw new ArgumentException("Portable ordinary graph extents exceeded.");
        if(contexts.Any(c=>c is null))throw new ArgumentException("Null auxiliary context.");
        if(contexts.Sum(c=>(long)c.ControlCount)>1_048_576)throw new ArgumentException("Auxiliary control work budget exceeded.");
        int arcs=0,entries=0,ranges=0;var counts=new int[groups.Count];
        var previousEdges=new Dictionary<(int,int,int,int),int>();var nextEdges=new Dictionary<(int,int,int,int),int>();
        foreach(var row in nodes){if(row is null||row.Length>65535||arcs>131072-row.Length)throw new ArgumentException("Bounded complete arc rows required.");arcs+=row.Length;}
        for(int node=0;node<nodes.Count;node++)for(int ordinal=0;ordinal<nodes[node].Length;ordinal++)
        {
            var arc=nodes[node][ordinal];
            token.ThrowIfCancellationRequested();if(arc is null||arc.GroupId>=(uint)groups.Count||arc.Span>=maximumAtoms||node-arc.Span-1< -1)
                throw new ArgumentException("Ordinary arc owner/span invalid.");
            counts[arc.GroupId]++;CheckArc(arc,node,ordinal,false);
        }
        CheckArc(root,-1,-1,true);
        if(previousEdges.Count!=nextEdges.Count||previousEdges.Any(v=>!nextEdges.TryGetValue(v.Key,out int count)||count!=v.Value))
            throw new ArgumentException("Predecessor/successor multiplicities differ.");
        for(int g=0;g<groups.Count;g++)
        {
            token.ThrowIfCancellationRequested();var group=groups[g]??throw new ArgumentException("Null group.");var f=group.Fields;
            if(f.DescriptorOrdinal==ushort.MaxValue||f.ReferenceCounter>200||f.HistoryOwned08>1||
                f.ReferenceCounter!=200&&f.ReferenceCounter!=counts[g])throw new ArgumentException("Native sticky reference byte/group ownership differs.");
            CheckAux(f.AuxiliaryContextIndex);var box=group.Bounds;
            if(!float.IsFinite(box.MinimumX)||!float.IsFinite(box.MinimumY)||!float.IsFinite(box.MaximumX)||!float.IsFinite(box.MaximumY)||box.MaximumX<box.MinimumX||box.MaximumY<box.MinimumY)
                throw new ArgumentException("Finite ordered group bbox required.");
            CheckRanges(group.CopyRanges(),true);foreach(var history in group.CopyHistory())CheckRanges(history,false);
        }
        if(nodes.Count==0&&(groups.Count!=0||contexts.Count!=0))throw new ArgumentException("Empty source graph cannot own allocated ordinary groups/contexts.");
        return new(maximumAtoms,groups.Select(g=>g.Clone()).ToArray(),contexts.Select(c=>c.Clone()).ToArray(),
            nodes.Select(row=>row.Select(a=>a.Copy(a.GroupId,0,0)).ToArray()).ToArray(),root.Copy(root.GroupId,0,0));

        void CheckAux(ushort index){if(index!=ushort.MaxValue&&index>=contexts.Count)throw new ArgumentException("Auxiliary context ordinal escapes its owner.");}
        void CheckRanges(LegacyInkRange[] values,bool required)
        {
            if(required&&values.Length==0||values.Length>4096||ranges>1_048_576-values.Length)throw new ArgumentException("Bounded actual range vectors required.");ranges+=values.Length;
            foreach(var r in values)if(r.StrokeIndex is <0 or >255||r.StartPoint<0||r.EndPointExclusive<=r.StartPoint||r.EndPointExclusive>4096)throw new ArgumentException("Malformed physical range.");
        }
        void CheckArc(LegacyMainGraphAppendArc arc,int node,int ordinal,bool isRoot)
        {
            CheckAux(arc.AuxiliaryContextIndex);
            if(arc.Successors.Length!=maximumAtoms||isRoot&&arc.Predecessors.Length!=0)throw new ArgumentException("Complete per-selector vectors and empty root predecessors required.");
            entries=checked(entries+arc.Predecessors.Length);if(entries>1_048_576)throw new ArgumentException("Graph connection budget exceeded.");
            int previous=node-arc.Span-1;
            if(!isRoot&&previous==-1&&arc.Predecessors.Length>1)
                throw new NotSupportedException("The ordinary append scope requires the native sole-root predecessor form.");
            if(!isRoot)foreach(int p in arc.Predecessors)
            {
                if(previous!=-1&&((uint)previous>=(uint)nodes.Count||(uint)p>=(uint)nodes[previous].Length))throw new ArgumentException("Invalid predecessor ordinal.");
                if(previous==-1&&p!=-1)throw new ArgumentException("Native root sentinel required.");
                var key=(previous,p,node,ordinal);previousEdges[key]=previousEdges.GetValueOrDefault(key)+1;
            }
            for(int selector=0;selector<maximumAtoms;selector++)
            {
                int target=node+selector+1;var values=arc.Successors[selector];entries=checked(entries+values.Length);
                if(entries>1_048_576)throw new ArgumentException("Graph connection budget exceeded.");
                foreach(int index in values)
                {
                    if((uint)target>=(uint)nodes.Count||(uint)index>=(uint)nodes[target].Length||nodes[target][index].Span!=selector)
                        throw new ArgumentException("Invalid successor selector/ordinal.");
                    var key=(node,ordinal,target,index);nextEdges[key]=nextEdges.GetValueOrDefault(key)+1;
                }
            }
        }
    }

    /// <summary>Native increment cap200 followed by deletion's below200 test.</summary>
    internal static byte ReplayReferenceCounter(int allocated,int removed)
    {if(allocated<0||removed<0||removed>allocated)throw new ArgumentException("Corresponding allocation/removal counts required.");return allocated>=200?(byte)200:checked((byte)(allocated-removed));}

    internal LegacyMainGraphAppendResult Append(LegacyMainGraphAppendState local,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(local);token.ThrowIfCancellationRequested();
        if(local.MaximumAtoms!=MaximumAtoms)throw new NotSupportedException("The ordinary same-selected-profile append scope requires one span capacity.");
        if(local.NodeCount==0)return new(this,Enumerable.Repeat(-1,local.GroupCount).ToArray(),NodeCount,GroupCount,ContextCount,true);
        if(NodeCount>4096-local.NodeCount||ContextCount>65535-local.ContextCount)
            throw new ArgumentException("Shared node/auxiliary ordinal budget exceeded before append.");
        int live=local.groups.Count(g=>g.Fields.ReferenceCounter!=0);
        if(GroupCount>131072-live)throw new ArgumentException("Shared group budget exceeded before append.");
        long arcCount=nodes.Sum(n=>(long)n.Length)+local.nodes.Sum(n=>(long)n.Length);
        int roots=local.nodes.Take(MaximumAtoms).Sum(n=>n.Count(a=>a.Predecessors.Length==1&&a.Predecessors[0]==-1));
        long edgeCount=Edges(this)+Edges(local)+(NodeCount==0?0:2L*roots*(nodes[^1].Length-1));
        var historyGroups=local.nodes.SelectMany((row,n)=>n<MaximumAtoms||n==local.NodeCount-1?row.Select(a=>a.GroupId):Array.Empty<uint>()).ToHashSet();
        long rangeCount=groups.Sum(g=>(long)g.RangeCount+g.HistoryRangeCount)+local.groups.Select((g,n)=>(g,n))
            .Where(v=>v.g.Fields.ReferenceCounter!=0).Sum(v=>(long)v.g.RangeCount+(historyGroups.Contains((uint)v.n)?v.g.HistoryRangeCount:0));
        if(arcCount>131072||edgeCount>1_048_576||rangeCount>1_048_576||
            contexts.Sum(c=>(long)c.ControlCount)+local.contexts.Sum(c=>(long)c.ControlCount)>1_048_576)
            throw new ArgumentException("Combined append allocation/work budget exceeded before copying or root products.");
        int oldNodes=NodeCount,oldGroups=GroupCount,oldContexts=ContextCount,endpointOffset=(oldNodes&0xffffff)<<7;
        var map=Enumerable.Repeat(-1,local.GroupCount).ToArray();var nextGroups=groups.Select(g=>g.Clone()).ToList();
        for(int g=0;g<local.GroupCount;g++)if(local.groups[g].Fields.ReferenceCounter!=0)
        {token.ThrowIfCancellationRequested();map[g]=nextGroups.Count;nextGroups.Add(local.groups[g].ShiftAuxiliary(oldContexts));}
        var nextContexts=contexts.Concat(local.contexts).Select(c=>c.Clone()).ToArray();
        var nextNodes=nodes.Select(row=>row.Select(a=>a.Copy(a.GroupId,0,0)).ToArray()).ToList();
        for(int n=0;n<local.NodeCount;n++)
        {
            token.ThrowIfCancellationRequested();var row=local.nodes[n].Select(a=>a.Copy(checked((uint)map[a.GroupId]),endpointOffset,oldContexts)).ToArray();nextNodes.Add(row);
            //19D470 materializes history only in the boundary lookahead/last
            //node window, after the initial ordinary copy cleared both vectors.
            if(n<MaximumAtoms||n==local.NodeCount-1)foreach(var arc in local.nodes[n])
            {
                int destination=map[arc.GroupId];if(nextGroups[destination].HistoryCount==0)
                    nextGroups[destination]=nextGroups[destination].WithHistory(local.groups[arc.GroupId]);
            }
        }
        if(oldNodes>0)
        {
            int terminal=oldNodes-1;var rootsBySelector=Enumerable.Range(0,MaximumAtoms).Select(_=>new List<int>()).ToArray();
            for(int n=0;n<Math.Min(local.NodeCount,MaximumAtoms);n++)for(int ordinal=0;ordinal<nextNodes[oldNodes+n].Length;ordinal++)
            {
                token.ThrowIfCancellationRequested();var arc=nextNodes[oldNodes+n][ordinal];
                if(arc.Predecessors.Length!=1||arc.Predecessors[0]!=-1)continue;
                arc=arc.WithConnections(Enumerable.Range(0,nextNodes[terminal].Length).ToArray(),arc.CopySuccessors());nextNodes[oldNodes+n][ordinal]=arc;
                rootsBySelector[n].Add(ordinal);
            }
            for(int p=0;p<nextNodes[terminal].Length;p++)
            {
                token.ThrowIfCancellationRequested();var previous=nextNodes[terminal][p];var vectors=previous.CopySuccessors();
                for(int selector=0;selector<MaximumAtoms;selector++)vectors[selector]=vectors[selector].Concat(rootsBySelector[selector]).ToArray();
                nextNodes[terminal][p]=previous.WithConnections(previous.CopyPredecessors(),vectors);
            }
        }
        var nextRoot=oldNodes==0?local.Root.Copy(local.Root.GroupId,0,0):Root.Copy(Root.GroupId,0,0);
        var state=Create(MaximumAtoms,nextGroups,nextContexts,nextNodes,nextRoot,token);
        return new(state,map,oldNodes,oldGroups,oldContexts,false);
        static long Edges(LegacyMainGraphAppendState state)=>state.nodes.SelectMany(n=>n).Append(state.Root)
            .Sum(a=>(long)a.Predecessors.Length+a.Successors.Sum(v=>(long)v.Length));
    }
}

public sealed class LegacyMainGraphAppendResult
{
    private readonly int[] groups;
    public LegacyMainGraphAppendState State {get;}
    public int NodeOffset {get;}
    public int GroupOffset {get;}
    public int ContextOffset {get;}
    public bool EmptySource {get;}
    internal LegacyMainGraphAppendResult(LegacyMainGraphAppendState state,int[] groups,int nodes,int groupOffset,int contexts,bool empty)
    {State=state;this.groups=groups;NodeOffset=nodes;GroupOffset=groupOffset;ContextOffset=contexts;EmptySource=empty;}
    public int[] CopyLocalGroupMap()=>groups.ToArray();
}
