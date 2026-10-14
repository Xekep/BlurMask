namespace InkRecognition;

/// <summary>Ordinary no-protocol21B.virtual120 mode0 forward source range path.
/// Owns the seed/candidate graphs; reconstructs actual source metadata rather
/// than treating the acoustic group's normalized ranges as raw source points.</summary>
public sealed class LegacyOrdinaryGraphSourceRanges
{
    private readonly LegacyOrdinaryCursiveSeedResult seeds;
    private readonly LegacyOrdinaryCursiveSeedGraph seedGraph;
    private readonly LegacySourcePointChronology prepared;
    private readonly LegacyOrdinarySourceInk original;
    private readonly LegacySourcePointKey[] keys;
    public LegacyOrdinaryCursiveCandidateGraph Graph { get; }
    internal LegacySourcePointChronology PreparedChronology=>prepared;
    internal LegacyOrdinarySourceInk OriginalSource=>original;

    /// <summary>Keep actual seed/chronology/raw-source ownership after the
    /// native sampler eligibility pass removes and renumbers graph arcs.</summary>
    public LegacyOrdinaryGraphSourceRanges WithSamplerPruning(LegacyOrdinarySamplerGroupPruningResult pruning,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(pruning);cancellationToken.ThrowIfCancellationRequested();
        if(!ReferenceEquals(pruning.OriginalGraph,Graph))
            throw new ArgumentException("Sampler pruning must belong to this exact candidate graph.");
        return new(seeds,seedGraph,pruning.Graph,prepared,original,keys);
    }

    private LegacyOrdinaryGraphSourceRanges(LegacyOrdinaryCursiveSeedResult seeds,
        LegacyOrdinaryCursiveSeedGraph seedGraph, LegacyOrdinaryCursiveCandidateGraph graph,
        LegacySourcePointChronology prepared, LegacyOrdinarySourceInk original, LegacySourcePointKey[] keys)
    { this.seeds=seeds;this.seedGraph=seedGraph;Graph=graph;this.prepared=prepared;this.original=original;this.keys=keys; }

    public static LegacyOrdinaryGraphSourceRanges Prepare(LegacyOrdinaryCursiveSeedResult actualSeeds,
        LegacySourcePointChronology actualPreparedChronology, LegacyOrdinarySourceInk actualOriginalSource,
        int maximumAtoms, int maximumCandidates=131072, CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualSeeds);ArgumentNullException.ThrowIfNull(actualPreparedChronology);
        ArgumentNullException.ThrowIfNull(actualOriginalSource);cancellationToken.ThrowIfCancellationRequested();
        var counts=actualPreparedChronology.StrokePointCounts.Span;
        if(counts.Length!=actualOriginalSource.Strokes.Count)
            throw new ArgumentException("Same actual ordinary physical source and prepared metadata required.");
        for(int s=0;s<counts.Length;s++)for(int p=0;p<counts[s];p++)
            _=actualOriginalSource.ResolveEndingMetadata(actualPreparedChronology.Resolve(s,p),cancellationToken);
        foreach(var seed in actualSeeds.Seeds)
        {
            cancellationToken.ThrowIfCancellationRequested();var r=seed.Atom.Range;
            if((uint)r.StrokeIndex>=(uint)counts.Length||r.StartPoint<0||r.EndPointExclusive<=r.StartPoint||r.EndPointExclusive>counts[r.StrokeIndex])
                throw new ArgumentException("Actual seed range must address the supplied prepared source.");
        }
        var seedGraph=actualSeeds.InsertionShift==0?LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(actualSeeds,cancellationToken):
            LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(actualSeeds,cancellationToken);
        var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(actualSeeds,seedGraph,maximumAtoms,maximumCandidates,cancellationToken);
        var keys=new List<LegacySourcePointKey>();
        for(int s=0;s<actualOriginalSource.Strokes.Count;s++)for(int p=0;p<actualOriginalSource.Strokes[s].Points.Count;p++)
        {cancellationToken.ThrowIfCancellationRequested();keys.Add(actualOriginalSource.Chronology.Resolve(s,p).Key);}
        return new(actualSeeds,seedGraph,graph,actualPreparedChronology,actualOriginalSource,keys.ToArray());
    }

    public LegacyFirstResultSourceRange[] ResolveCandidate(int endingNode,int arcOrdinal,
        CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();var node=Graph.GetNode(endingNode);
        if((uint)arcOrdinal>=(uint)node.Count)throw new ArgumentOutOfRangeException(nameof(arcOrdinal));
        var arc=node[arcOrdinal];return ResolveForward(arc.StartContextId,arc.EndContextId,cancellationToken);
    }

    /// <summary>Only the successful ordinary forward mode0 branch1A9AE0 is
    /// supported. Reverse endpoint, protocol and exceptional source rows are
    /// separate native branches; this method never fabricates their membership.</summary>
    public LegacyFirstResultSourceRange[] ResolveForward(uint firstContext,uint lastContext,
        CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(firstContext>int.MaxValue||lastContext>int.MaxValue||firstContext>lastContext||
            (firstContext>>7)>=(uint)seedGraph.NodeCount||(lastContext>>7)>=(uint)seedGraph.NodeCount)
            throw new NotSupportedException("Bounded ordinary forward graph context interval required.");
        var frames=new List<(int Node,int Ordinal)>();int visits=0;bool found=false;
        int firstNode=(int)(firstContext>>7);
        for(int ordinal=0;ordinal<seedGraph.GetNode(firstNode).Count&&!found;ordinal++)
        {
            var arc=seedGraph.GetNode(firstNode)[ordinal];if(arc.StartContextId!=firstContext)continue;
            frames.Clear();var stack=new Stack<(int Node,int Ordinal,int Child)>();stack.Push((firstNode,ordinal,-1));
            while(stack.Count!=0)
            {
                cancellationToken.ThrowIfCancellationRequested();if(++visits>1_048_576)throw new ArgumentException("Source path visit budget exceeded.");
                var frame=stack.Pop();var current=seedGraph.GetNode(frame.Node)[frame.Ordinal];
                if(frame.Child==-1)
                {
                    if(lastContext<current.EndContextId)continue;
                    frames.Add((frame.Node,frame.Ordinal));
                    if(current.EndContextId==lastContext){found=true;break;}
                    stack.Push((frame.Node,frame.Ordinal,0));
                }
                else if(frame.Child<current.SuccessorArcIndices.Length)
                {
                    stack.Push((frame.Node,frame.Ordinal,frame.Child+1));
                    int next=current.SuccessorArcIndices.Span[frame.Child];
                    if(frame.Node+1>=seedGraph.NodeCount||next<0||next>=seedGraph.GetNode(frame.Node+1).Count)
                        throw new InvalidDataException("Actual seed successor ownership required.");
                    stack.Push((frame.Node+1,next,-1));
                }
                else frames.RemoveAt(frames.Count-1);
            }
        }
        if(!found)throw new InvalidDataException("Actual ordinary source path does not reach its requested ending context.");
        var rows=new List<Row>();
        foreach(var frame in frames)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var range=seeds.Seeds[seedGraph.GetNode(frame.Node)[frame.Ordinal].SeedGroupIndex].Atom.Range;
            var first=prepared.Resolve(range.StrokeIndex,range.StartPoint);
            var lastStart=prepared.Resolve(range.StrokeIndex,range.EndPointExclusive-1);
            var last=original.ResolveEndingMetadata(lastStart,cancellationToken);
            bool tail=range.EndPointExclusive==prepared.StrokePointCounts.Span[range.StrokeIndex];
            rows.Add(new(first,last,lastStart,tail));
        }
        //1A9570 sorts start keys ascending, then remainder bytes descending,
        //then end keys ascending and end remainders descending. Stable ties.
        rows=rows.OrderBy(r=>r,RowComparer.Instance).ToList();
        //1A9AE0 reverse merge uses the F8 endpoint pointer and its next record,
        //not raw ordinal+1 (which loses dropped duplicate runs).
        for(int i=rows.Count-1;i>0;i--)
        {
            cancellationToken.ThrowIfCancellationRequested();var current=rows[i];var previous=rows[i-1];
            bool equal=current.First.Key==previous.LastStart.Key&&current.First.RemainderByte==previous.LastStart.RemainderByte;
            int nextIndex=previous.LastStart.KeyRecordIndex+1;
            bool adjacent=previous.Tail&&nextIndex<keys.Length&&current.First.Key==keys[nextIndex];
            if(equal||adjacent){rows[i-1]=previous with{Last=current.Last,LastStart=current.LastStart,Tail=current.Tail};rows.RemoveAt(i);}
        }
        return rows.Select(r=>new LegacyFirstResultSourceRange(r.First.Key,r.Tail?r.Last.Key:r.LastStart.Key)).ToArray();
    }

    private readonly record struct Row(LegacySourcePointIdentity First,LegacySourcePointIdentity Last,
        LegacySourcePointIdentity LastStart,bool Tail);
    private sealed class RowComparer:IComparer<Row>
    {
        public static readonly RowComparer Instance=new();
        public int Compare(Row a,Row b)
        {
            int c=Key(a.First.Key,b.First.Key);if(c!=0)return c;
            c=b.First.RemainderByte.CompareTo(a.First.RemainderByte);if(c!=0)return c;
            c=Key(a.Last.Key,b.Last.Key);return c!=0?c:b.Last.RemainderByte.CompareTo(a.Last.RemainderByte);
        }
        private static int Key(LegacySourcePointKey a,LegacySourcePointKey b)
        {int c=a.First.CompareTo(b.First);if(c!=0)return c;c=a.Second.CompareTo(b.Second);return c!=0?c:a.Third.CompareTo(b.Third);}
    }
}
