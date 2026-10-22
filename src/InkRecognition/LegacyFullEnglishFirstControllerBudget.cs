namespace InkRecognition;

//A separate B2 logical admission/work reservation; not CLR/process RSS.
internal sealed class LegacyFullEnglishFirstControllerBudget
{
    internal const long Limit=32L*1024*1024;
    internal long Bytes {get;}
    internal int Groups {get;}
    internal int Arcs {get;}
    internal int Nodes {get;}
    internal int Connections {get;}
    internal int Points {get;}
    internal static long Envelope(int groups,int arcs,int nodes,int connections,int points)
    {
        if(groups is <0 or >4096||arcs is <0 or >4096||nodes is <0 or >4096||connections is <0 or >1048576||points is <0 or >8192)
            throw new ArgumentException("B2 cumulative parent-wide count budget exceeded.");
        long n=checked(524288L+2048L*groups+1024L*arcs+256L*nodes+16L*connections+64L*points);
        if(n>Limit)throw new ArgumentException("B2 additional logical32MiB envelope exceeded.");return n;
    }
    internal LegacyFullEnglishFirstControllerBudget(IReadOnlyList<LegacyFullEnglishFirstAcousticCache> children,CancellationToken token)
    {
        int groups=0,arcs=0,nodes=0,links=0,points=0;
        foreach(var cache in children)
        {
            token.ThrowIfCancellationRequested();var c=cache.Owner;groups=checked(groups+c.CopyLiveGroupIds().Length);nodes=checked(nodes+c.NodeCount);
            foreach(int n in c.CopyCutCounts()){token.ThrowIfCancellationRequested();points=checked(points+n);}
            for(int n=0;n<c.NodeCount;n++)foreach(var a in c.SourceRanges.Graph.GetNode(n))
            {
                token.ThrowIfCancellationRequested();arcs=checked(arcs+1);links=checked(links+a.Successors.Count);
                //Owned FIRST arcs have span<=node: previous<0 iff node==span,
                //and that root's previous index is exactly-1, never below-1.
                if(a.Span>7||a.Span>n)throw new NotSupportedException("Actual ordinary FIRST arc/root domain required.");
                int previous=n-a.Span-1;
                foreach(int p in a.PredecessorArcIndices.Span)
                {token.ThrowIfCancellationRequested();if(previous<0?p!=-1:p<0||p>=c.SourceRanges.Graph.GetNode(previous).Count)throw new InvalidDataException("Invalid actual predecessor ordinal.");}
                foreach(var edge in a.Successors)
                {token.ThrowIfCancellationRequested();if(edge.NodeOffset<1||n+edge.NodeOffset>=c.NodeCount||edge.ArcIndex<0||edge.ArcIndex>=c.SourceRanges.Graph.GetNode(n+edge.NodeOffset).Count)throw new InvalidDataException("Invalid actual successor ordinal.");}
                if(n==a.Span&&a.PredecessorArcIndices.Span.Contains(-1))links=checked(links+1);
                _=Envelope(groups,arcs,nodes,links,points);
            }
            _=Envelope(groups,arcs,nodes,links,points);
        }
        Groups=groups;Arcs=arcs;Nodes=nodes;Connections=links;Points=points;Bytes=Envelope(groups,arcs,nodes,links,points);
    }
}
