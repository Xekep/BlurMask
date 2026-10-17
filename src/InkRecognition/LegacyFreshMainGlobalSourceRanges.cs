namespace InkRecognition;

/// <summary>Owning ordinary21B.V120 forward mode0 source references on the
/// shared compact graph. Traverses actual span0 arcs/selector1 in native order;
/// no same-source atom ID or independently sampled line graph is fabricated.</summary>
public sealed class LegacyFreshMainGlobalSourceRanges
{
    private readonly LegacySourcePointChronology chronology;
    private readonly LegacyOrdinarySourceInk original;
    private readonly LegacySourcePointKey[] keys;
    public LegacyFreshMainCompactOrdinaryGraph Graph {get;}
    private LegacyFreshMainGlobalSourceRanges(LegacyFreshMainCompactOrdinaryGraph graph,
        LegacySourcePointChronology chronology,LegacyOrdinarySourceInk original,LegacySourcePointKey[] keys)
    {Graph=graph;this.chronology=chronology;this.original=original;this.keys=keys;}

    public static LegacyFreshMainGlobalSourceRanges Prepare(LegacyFreshMainCompactOrdinaryGraph graph,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(graph);token.ThrowIfCancellationRequested();
        var source=graph.Source.Source;var chronology=source.Chronology;
        var original=source.Preparation.Input.Input.Input.Owner.Controller.Source;
        var counts=chronology.StrokePointCounts.Span;
        if(counts.Length!=original.Strokes.Count)throw new ArgumentException("Shared original/prepared physical owner differs.");
        for(int physical=0;physical<counts.Length;physical++)for(int p=0;p<counts[physical];p++)
        {token.ThrowIfCancellationRequested();_=original.ResolveEndingMetadata(chronology.Resolve(physical,p),token);}
        var keys=LegacyOrdinaryGraphSourceRanges.CopyOwnedKeyRecords(original,token);
        return new(graph,chronology,original,keys);
    }

    public LegacyOrdinaryGraphSourceReference[] ResolveCandidateWithResiduals(int endingNode,int arcOrdinal,
        CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();var row=Graph.State.GetNode(endingNode);
        if((uint)arcOrdinal>=(uint)row.Count)throw new ArgumentOutOfRangeException(nameof(arcOrdinal));
        var arc=row[arcOrdinal];return ResolveForwardWithResiduals(arc.StartContextId,arc.EndContextId,token);
    }
    public LegacyFirstResultSourceRange[] ResolveCandidate(int endingNode,int arcOrdinal,CancellationToken token=default)
        =>ResolveCandidateWithResiduals(endingNode,arcOrdinal,token).Select(r=>new LegacyFirstResultSourceRange(r.First,r.Last)).ToArray();

    public LegacyOrdinaryGraphSourceReference[] ResolveForwardWithResiduals(uint first,uint last,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();var state=Graph.State;
        if(first>int.MaxValue||last>int.MaxValue||first>last||(first>>7)>=(uint)state.NodeCount||(last>>7)>=(uint)state.NodeCount)
            throw new NotSupportedException("Bounded ordinary forward mode0 context interval required.");
        int firstNode=(int)(first>>7),work=0;bool found=false;var frames=new List<(int Node,int Arc)>();
        var selectors=new Dictionary<(int Node,int Arc),ushort[]>();
        var initial=state.GetNode(firstNode);
        for(int ordinal=0;ordinal<initial.Count&&!found;ordinal++)
        {
            var start=initial[ordinal];if(start.Span!=0||start.StartContextId!=first)continue;
            frames.Clear();var stack=new Stack<(int Node,int Arc,int Child)>();stack.Push((firstNode,ordinal,-1));
            while(stack.Count!=0)
            {
                token.ThrowIfCancellationRequested();if(++work>1_048_576)throw new ArgumentException("Ordinary global source DFS work budget exceeded.");
                var frame=stack.Pop();var arc=state.GetNode(frame.Node)[frame.Arc];
                if(arc.Span!=0||arc.StartContextId>>7!=(uint)frame.Node)
                    throw new InvalidDataException("Ordinary global source DFS requires an actual span0 source arc.");
                if(frame.Child==-1)
                {
                    if(last<arc.EndContextId)continue;frames.Add((frame.Node,frame.Arc));
                    if(arc.EndContextId==last){found=true;break;}stack.Push((frame.Node,frame.Arc,0));
                }
                else
                {
                    var key=(frame.Node,frame.Arc);
                    if(!selectors.TryGetValue(key,out var next)){next=arc.CopySelector(1);selectors.Add(key,next);}
                    if(frame.Child<next.Length)
                    {
                        stack.Push((frame.Node,frame.Arc,frame.Child+1));int target=frame.Node+1,index=next[frame.Child];
                        if(target>=state.NodeCount||index>=state.GetNode(target).Count)throw new InvalidDataException("Global ordinary successor escaped its owning graph.");
                        stack.Push((target,index,-1));
                    }
                    else frames.RemoveAt(frames.Count-1);
                }
            }
        }
        if(!found)throw new InvalidDataException("Actual shared ordinary source path does not reach its ending context.");
        var ranges=new List<LegacyInkRange>();
        foreach(var frame in frames)
        {
            token.ThrowIfCancellationRequested();var arc=state.GetNode(frame.Node)[frame.Arc];
            foreach(var range in state.Groups[(int)arc.GroupId].CopyRanges())
            {if(ranges.Count>=4096)throw new ArgumentException("Portable ordinary source range path budget exceeded.");ranges.Add(range);}
        }
        return LegacyOrdinaryGraphSourceRanges.ResolveOwnedRangePath(chronology,original,ranges,keys,token);
    }
}
