using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMainCrossGraph.Probe")]

namespace InkRecognition;

internal sealed class LegacyMainCrossDescriptor
{
    internal readonly byte[] Bytes;
    internal readonly byte[]? Guide;
    internal int At(int offset)=>BinaryPrimitives.ReadInt32LittleEndian(Bytes.AsSpan(offset));
    internal int Line=>At(16);
    internal int First=>At(36);
    internal int Last=>At(40);
    internal float Metric(int offset)=>Guide is null?throw new InvalidDataException("Assigned source guide required."):BinaryPrimitives.ReadSingleLittleEndian(Guide.AsSpan(offset));
    internal LegacyMainCrossDescriptor(byte[] descriptor,IReadOnlyList<byte[]> guides)
    {
        if(descriptor.Length!=88)throw new ArgumentException("Complete source descriptor required.");Bytes=descriptor.ToArray();
        if(Line!=-1)
        {
            if((uint)Line>=(uint)guides.Count||guides[Line].Length!=40)throw new ArgumentException("Actual descriptor guide required.");
            Guide=guides[Line].ToArray();
        }
    }
}

internal readonly record struct LegacyMainCrossGraphEvent(byte Kind,int Node,int Ordinal,int Group,int OtherNode,int OtherOrdinal);

/// <summary>Ordinary cross-source1A3B30/1A37B0 and19E8B0 pruning over richer
/// append state. Does not fabricate compact source records or NN context IDs.</summary>
internal sealed class LegacyMainCrossSourceGraph
{
    private sealed class Arc
    {
        internal uint Group;internal byte Span,Flags;internal sbyte Placement;internal uint Start,End,Scalar;
        internal ushort Auxiliary,Extra;internal List<int> Previous;internal List<int>[] Next;
        internal Arc(LegacyMainGraphAppendArc a)
        {Group=a.GroupId;Span=a.Span;Flags=a.Flags;Placement=a.Placement;Start=a.StartContextId;End=a.EndContextId;Auxiliary=a.AuxiliaryContextIndex;Extra=a.Extra1a;Scalar=a.Scalar1cBits;Previous=a.CopyPredecessors().ToList();Next=a.CopySuccessors().Select(v=>v.ToList()).ToArray();}
        internal LegacyMainGraphAppendArc Snapshot()=>new(Group,Span,Placement,Flags,Start,End,Auxiliary,Extra,Scalar,Previous.ToArray(),Next.Select(v=>v.ToArray()).ToArray());
    }
    private readonly int cap;
    private readonly List<LegacyMainGraphAppendGroup> groups;
    private readonly Dictionary<string,int> rangeIds=[];
    private readonly LegacyMainGraphAppendContext[] contexts;
    private readonly List<Arc>[] nodes;
    private readonly Arc root;
    private readonly LegacyMainCrossDescriptor[] descriptors;
    private readonly LegacyCursivePreparedInk ink;
    private readonly LegacyOrdinarySamplerGroupPruningProfile profile;
    private readonly LegacyCursiveSeparationTable table;
    private readonly CancellationToken token;
    private int work,arcCount,rangeWork;
    private readonly List<LegacyMainCrossGraphEvent> events=[];
    internal IReadOnlyList<LegacyMainCrossGraphEvent> Events=>events.AsReadOnly();
    internal LegacyMainCrossSourceGraph(LegacyMainGraphAppendState state,LegacyMainCrossDescriptor[] descriptors,
        LegacyCursivePreparedInk ink,LegacyOrdinarySamplerGroupPruningProfile profile,LegacyCursiveSeparationTable table,CancellationToken token=default)
    {
        this.token=token;token.ThrowIfCancellationRequested();cap=state.MaximumAtoms;groups=state.Groups.Select(g=>g.Clone()).ToList();
        contexts=state.Contexts.Select(c=>c.Clone()).ToArray();nodes=Enumerable.Range(0,state.NodeCount).Select(n=>state.GetNode(n).Select(a=>new Arc(a)).ToList()).ToArray();
        root=new(state.Root);this.descriptors=descriptors;this.ink=ink;this.profile=profile;this.table=table;
        arcCount=nodes.Sum(v=>v.Count);rangeWork=groups.Sum(g=>g.RangeCount+g.HistoryRangeCount);
        foreach(var g in groups)
            if(g.Fields.DescriptorOrdinal>=descriptors.Length||g.Fields.SecondaryDescriptorOrdinal!=ushort.MaxValue&&g.Fields.SecondaryDescriptorOrdinal>=descriptors.Length)
                throw new ArgumentException("Actual source descriptor owners required.");
        for(int id=0;id<groups.Count;id++)rangeIds.TryAdd(RangeKey(groups[id].CopyRanges()),id);
    }
    private void Tick(){token.ThrowIfCancellationRequested();if(++work>1_048_576)throw new ArgumentException("Portable cross-source work budget exceeded.");}

    internal void MergeBoundary(int leftNode,int requiredSpan,int rightNode)
    {
        if(leftNode<0||rightNode<=leftNode||rightNode>=nodes.Length||requiredSpan<0)throw new ArgumentException("Actual appended source boundary required.");
        if(rightNode-leftNode+requiredSpan>=cap)return;
        for(int leftOrdinal=0;leftOrdinal<nodes[leftNode].Count;leftOrdinal++)
        {
            Tick();var left=nodes[leftNode][leftOrdinal];if(left.Span!=requiredSpan)continue;
            var lg=groups[(int)left.Group];var ld=descriptors[lg.Fields.DescriptorOrdinal];
            if((BinaryPrimitives.ReadUInt16LittleEndian(ld.Bytes)&15) is not (0 or 6)||ld.At(32)==0x2000000)continue;
            //Native walks this selector's linked list. Its contents do not
            //change when a new arc is attached to an earlier predecessor.
            foreach(int rightOrdinal in left.Next[rightNode-leftNode-1].ToArray())
            {
                Tick();var right=nodes[rightNode][rightOrdinal];var rg=groups[(int)right.Group];
                if((right.Flags&8)!=0||lg.Fields.DescriptorOrdinal==rg.Fields.DescriptorOrdinal)continue;
                var rd=descriptors[rg.Fields.DescriptorOrdinal];
                if((BinaryPrimitives.ReadUInt16LittleEndian(rd.Bytes)&15) is not (0 or 6)||rd.At(32)==0x2000000)continue;
                if(!GeometryAllowed(lg,rg,ld,rd))continue;
                var history=lg.CopyHistory().Concat(rg.CopyHistory()).ToArray();
                if(history.Length==0)throw new NotSupportedException("Native cross candidates require retained boundary histories.");
                LegacyMainLocalGraphMetadata.SortHistory(history,token);
                var ranges=LegacyCursiveInkAtoms.Compose(history.SelectMany(v=>v).ToArray(),token);
                string key=RangeKey(ranges);
                if(!rangeIds.TryGetValue(key,out int group))
                {
                    int extra=ranges.Length+history.Sum(v=>v.Length);
                    if(groups.Count>=131072||rangeWork>1_048_576-extra)throw new ArgumentException("Portable cross group/range budget exceeded.");rangeWork+=extra;
                    group=groups.Count;var a=lg.Bounds;var b=rg.Bounds;
                    groups.Add(new(lg.Fields with{SecondaryDescriptorOrdinal=rg.Fields.DescriptorOrdinal,ReferenceCounter=0,HistoryOwned08=0,Flags50=0,
                        Flags51=(byte)((lg.Fields.Flags51|rg.Fields.Flags51)!=0?1:0),Flags52=0},
                        new(Math.Min(a.MinimumX,b.MinimumX),Math.Min(a.MinimumY,b.MinimumY),Math.Max(a.MaximumX,b.MaximumX),Math.Max(a.MaximumY,b.MaximumY)),ranges,history));
                    rangeIds.Add(key,group);
                    events.Add(new(0,leftNode,leftOrdinal,group,rightNode,rightOrdinal));
                }
                //1A41C7 searches leftNode+1, even when a cross allocation will
                //end at rightNode. Preserve that observable native distinction.
                Arc? existing=null;
                foreach(var candidate in nodes[leftNode+1])
                    if(candidate.Group==(uint)group&&left.Previous.All(candidate.Previous.Contains)&&right.Next[0].All(candidate.Next[0].Contains)){existing=candidate;break;}
                if(existing is not null)
                {if((left.Flags&right.Flags&1)==0)existing.Flags&=0xfe;continue;}
                if(++arcCount>131072||nodes[rightNode].Count>=65535)throw new ArgumentException("Portable native arc/ordinal budget exceeded.");
                var next=Enumerable.Range(0,cap).Select(_=>Array.Empty<int>()).ToArray();
                var merged=new Arc(new((uint)group,checked((byte)(left.Span+rightNode-leftNode)),0,
                    (byte)((left.Flags&0x1e)|(left.Flags&right.Flags&1)),left.Start,right.End,left.Auxiliary,0,0,left.Previous.ToArray(),next));
                int ordinal=nodes[rightNode].Count;nodes[rightNode].Add(merged);Increment(group);
                int copied=Math.Min(cap,nodes.Length-rightNode-1);
                for(int selector=0;selector<copied;selector++)
                {
                    merged.Next[selector].AddRange(right.Next[selector]);
                    foreach(int target in right.Next[selector])nodes[rightNode+selector+1][target].Previous.Add(ordinal);
                }
                int previous=leftNode-left.Span-1;
                foreach(int p in left.Previous)(previous<0?root:nodes[previous][p]).Next[merged.Span].Add(ordinal);
                events.Add(new(1,rightNode,ordinal,group,leftNode,leftOrdinal));
            }
        }
    }

    private static bool GeometryAllowed(LegacyMainGraphAppendGroup left,LegacyMainGraphAppendGroup right,
        LegacyMainCrossDescriptor a,LegacyMainCrossDescriptor b)
    {
        if(a.At(8)!=-1&&b.At(8)!=-1&&a.At(8)!=b.At(8)||a.At(4)!=int.MinValue&&b.At(4)!=int.MinValue&&a.At(4)!=b.At(4)||
            a.Line!=-1&&b.Line!=-1&&(long)b.Line-a.Line>1)return false;
        if(a.Line==-1||b.Line==-1||a.Last-a.First+1<=1&&b.Last-b.First+1<=1)return true;
        var l=left.Bounds;var r=right.Bounds;float lowX=Math.Min(l.MinimumX,r.MinimumX),highX=Math.Max(l.MaximumX,r.MaximumX),
            lowY=Math.Min(l.MinimumY,r.MinimumY),highY=Math.Max(l.MaximumY,r.MaximumY);
        float delta=b.Metric(12)-a.Metric(12);if(delta<0)delta=-delta;
        float height=Math.Max(a.Metric(4),b.Metric(4));float allowance=Math.Max(l.MaximumY-l.MinimumY,height)+delta;
        if(highX-lowX<=allowance)
        {
            float gap=l.MinimumX-r.MaximumX;if(gap<0)gap=r.MinimumX-l.MaximumX;
            if(gap<=0||gap+gap<=r.MinimumY-l.MaximumY)return true;
        }
        return !(height*6<highY-lowY||height*6<highX-lowX);
    }

    internal void PruneAll(ReadOnlySpan<int> localNodeCounts)
    {
        if(localNodeCounts.ToArray().Sum()!=nodes.Length||localNodeCounts.Contains(0))throw new ArgumentException("Complete nonempty source-node partition required.");
        int start=0;
        foreach(int count in localNodeCounts)
        {
            if(count<1)throw new ArgumentException("Positive actual source node counts required.");
            for(int n=start;n<Math.Min(nodes.Length,start+cap);n++)PruneNode(n,true);
            PruneNode(start+count-1,false);start+=count;
        }
    }
    private void PruneNode(int node,bool boundary)
    {
        //1A5BD0 decrementsAX, then1A5BE6 tests its sign. Counts above32768
        //therefore visit no arc; retain that native unsigned16/signed16 gate.
        if(nodes[node].Count>32768)return;
        for(int ordinal=nodes[node].Count-1;ordinal>=0;ordinal--)
        {
            Tick();var arc=nodes[node][ordinal];var g=groups[(int)arc.Group];bool reject;
            if(boundary)
            {
                //1A5C28 jumps directly to the next ordinal forFFFF. Mode1
                //does NOT retest ordinary same-source groups with194140.
                if(g.Fields.SecondaryDescriptorOrdinal==ushort.MaxValue)continue;
                var primary=descriptors[g.Fields.DescriptorOrdinal];var secondary=descriptors[g.Fields.SecondaryDescriptorOrdinal];var ranges=g.CopyRanges();
                int first=Array.FindIndex(ranges,r=>r.StrokeIndex==primary.First);
                if(first>=0&&ranges.Skip(first).Any(r=>r.StrokeIndex==primary.Last))continue;
                int at=Array.FindIndex(ranges,r=>r.StrokeIndex==secondary.First),physical=secondary.First;
                bool complete=at>=0;
                if(complete)for(int i=at;i<ranges.Length;i++,physical++)
                {
                    var r=ranges[i];if(r.StrokeIndex!=physical||r.StartPoint!=0||physical==secondary.Last&&r.EndPointExclusive!=ink.StrokePointCounts.Span[secondary.Last]){complete=false;break;}
                }
                if(complete&&physical-1>=secondary.Last)reject=SamplerReject(g,arc.Span);else reject=true;
            }
            else reject=SamplerReject(g,arc.Span);
            if(!reject)continue;
            //Equivalent ordered linked-list deletion: remove all reciprocal
            //occurrences, then renumber larger ordinals without sorting them.
            for(int n=0;n<nodes.Length;n++)foreach(var other in nodes[n])
            {
                Tick();if(n-other.Span-1==node)Remap(other.Previous,ordinal);
            }
            for(int previous=-1;previous<nodes.Length;previous++)
            {
                int selector=node-previous-1;if(selector<0||selector>=cap)continue;
                if(previous<0)Remap(root.Next[selector],ordinal);
                else foreach(var other in nodes[previous])Remap(other.Next[selector],ordinal);
            }
            int id=(int)arc.Group;var old=groups[id];byte refs=old.Fields.ReferenceCounter;
            if(refs<200){if(refs==0)throw new InvalidDataException("Native reference underflow.");groups[id]=new(old.Fields with{ReferenceCounter=(byte)(refs-1)},old.Bounds,old.CopyRanges(),old.CopyHistory());}
            nodes[node].RemoveAt(ordinal);arcCount--;events.Add(new(2,node,ordinal,id,boundary?1:0,0));
        }
    }
    private void Remap(List<int> list,int removed)
    {for(int i=list.Count-1;i>=0;i--){Tick();if(list[i]==removed)list.RemoveAt(i);else if(list[i]>removed)list[i]--;}}
    private static string RangeKey(LegacyInkRange[] ranges)=>string.Join(";",ranges.Select(r=>$"{r.StrokeIndex},{r.StartPoint},{r.EndPointExclusive}"));
    private bool SamplerReject(LegacyMainGraphAppendGroup group,byte span)
    {
        if(span==0)return false;var ranges=group.CopyRanges();if(ranges.Length<2)return false;
        var a=descriptors[group.Fields.DescriptorOrdinal];if(a.At(24)==0x2000000)return false;if(a.Line==-1)return profile.RejectionThreshold<0x4000000;
        float height=a.Metric(4);
        if(group.Fields.SecondaryDescriptorOrdinal!=ushort.MaxValue)
        {
            var b=descriptors[group.Fields.SecondaryDescriptorOrdinal];
            if(b.Line!=-1)height=Math.Max(a.Metric(0),b.Metric(0))-Math.Min(a.Metric(0)-a.Metric(4),b.Metric(0)-b.Metric(4));
            else height=0; //Native local_180 remains0 when secondary has no line.
        }
        if(!float.IsFinite(height)||height<=0)throw new NotSupportedException("Nonpositive cross-source separation height has no safe ordinary numeric port.");
        int visits=ranges.Sum(r=>r.EndPointExclusive-r.StartPoint);
        token.ThrowIfCancellationRequested();if(work>1_048_576-visits)throw new ArgumentException("Portable cross-source point/work budget exceeded.");work+=visits;
        var p=profile.Separation;int cost=LegacyCursiveGroupSeparationCost.Evaluate(ink,ranges,height,p.GapOffset,p.Slope,table.Values.Span,0x4000000,cancellationToken:token);
        return cost>profile.RejectionThreshold;
    }
    private void Increment(int group)
    {var g=groups[group];if(g.Fields.ReferenceCounter<200)groups[group]=new(g.Fields with{ReferenceCounter=(byte)(g.Fields.ReferenceCounter+1)},g.Bounds,g.CopyRanges(),g.CopyHistory());}
    internal LegacyMainGraphAppendState Snapshot()=>LegacyMainGraphAppendState.Create(cap,groups,contexts,
        nodes.Select(v=>v.Select(a=>a.Snapshot()).ToArray()).ToArray(),root.Snapshot(),token);
}
