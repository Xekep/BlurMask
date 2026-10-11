namespace InkRecognition;

public readonly record struct LegacyGraphGlyphPosition(int NodeIndex,int ArcIndex);
public sealed record LegacyGraphWordMatch(ReadOnlyMemory<ushort> Labels,
    IReadOnlyList<LegacyGraphGlyphPosition> Glyphs,int AcousticCost,int Cost,int TerminalState);
public sealed record LegacyGraphWordSearchResult(IReadOnlyList<LegacyGraphWordMatch> Matches,
    int ScoredCandidates,long ExpandedChoices);

/// <summary>
/// Original bounded dictionary beam over native ordinary candidate topology.
/// Prefixes follow explicit predecessor arcs, including deferred placements;
/// they are not collapsed to a linear covered-atom frontier. Candidate costs
/// must be prefix-independent. Native word beam/winner/posterior policy is not
/// claimed; terminal additive scoring occurs before final pruning.
/// </summary>
public static class LegacyGraphWordSearch
{
    public static LegacyGraphWordSearchResult Search(LegacyPackedLexicon lexicon,
        LegacyOrdinaryCursiveCandidateGraph graph,
        Func<LegacyGraphGlyphPosition,LegacyOrdinaryCursiveCandidateArc,
            LegacyOrdinaryCursiveCandidateInk,ReadOnlyMemory<LegacyIntegerLexicalCandidate>> scoreCandidate,
        int beamWidth=64,long maximumExpandedChoices=2_000_000,
        Func<int,int,int?>? scoreTerminal=null,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(lexicon);ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(scoreCandidate);cancellationToken.ThrowIfCancellationRequested();
        if(beamWidth is <1 or >256)throw new ArgumentOutOfRangeException(nameof(beamWidth));
        if(maximumExpandedChoices is <1 or >100_000_000)throw new ArgumentOutOfRangeException(nameof(maximumExpandedChoices));
        var frontiers=new List<Path>[graph.NodeCount][];int scored=0;long expanded=0;
        var root=new Path(0,0,null,null,0,0,0);
        for(int node=0;node<graph.NodeCount;node++)
        {
            var arcs=graph.GetNode(node);frontiers[node]=new List<Path>[arcs.Count];bool last=node==graph.NodeCount-1;
            for(int ordinal=0;ordinal<arcs.Count;ordinal++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var arc=arcs[ordinal];var position=new LegacyGraphGlyphPosition(node,ordinal);
                int predecessorNode=node-arc.Span-1;var prefixes=new List<Path>();
                foreach(int predecessor in arc.PredecessorArcIndices.Span)
                {
                    if(predecessorNode<0)
                    {
                        if(predecessor!=-1 || predecessorNode!=-1)throw new InvalidDataException("Invalid native source-root span.");
                        prefixes.Add(root);
                    }
                    else prefixes.AddRange(frontiers[predecessorNode][predecessor]);
                }
                var next=new Dictionary<int,Path>();
                if(prefixes.Count!=0)
                {
                    var choices=scoreCandidate(position,arc,graph.Groups[arc.GroupId]).ToArray();scored++;
                    if(choices.Length>512)throw new ArgumentException("Candidate exceeds512 explicit lexical choices.",nameof(scoreCandidate));
                    foreach(var choice in choices)
                        if(choice.Cost is <-LegacyAcousticFixedLog.ImpossibleCost or >LegacyAcousticFixedLog.ImpossibleCost)
                            throw new ArgumentException("Candidate costs exceed the signed search domain.",nameof(scoreCandidate));
                    foreach(var prefix in prefixes)
                    {
                        if(prefix.Depth>=lexicon.MaximumWordLength)continue;
                        foreach(var choice in choices)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            if(++expanded>maximumExpandedChoices)throw new InvalidOperationException("Graph word search exceeded its explicit expansion budget.");
                            if(choice.Cost==LegacyAcousticFixedLog.ImpossibleCost ||
                                !lexicon.TryAdvance(prefix.State,choice.Label,out int state) || last && !lexicon.IsTerminal(state))continue;
                            long cumulative=(long)prefix.Acoustic+choice.Cost;
                            if(cumulative < -LegacyAcousticFixedLog.ImpossibleCost || cumulative>=LegacyAcousticFixedLog.ImpossibleCost)continue;
                            int acoustic=(int)cumulative;
                            int? total=last && scoreTerminal is not null ? scoreTerminal(state,acoustic) : acoustic;
                            if(total is null)continue;
                            if(next.TryGetValue(state,out var previous) && (previous.Cost<total || previous.Cost==total && previous.Acoustic<=acoustic))continue;
                            next[state]=new(state,choice.Label,prefix,position,acoustic,total.Value,prefix.Depth+1);
                        }
                    }
                }
                frontiers[node][ordinal]=next.Values.OrderBy(p=>p.Cost).ThenBy(p=>p.State).Take(beamWidth).ToList();
            }
        }
        var terminals=new Dictionary<int,Path>();
        foreach(var frontier in frontiers[^1])foreach(var path in frontier)
            if(!terminals.TryGetValue(path.State,out var previous) || path.Cost<previous.Cost || path.Cost==previous.Cost && path.Acoustic<previous.Acoustic)terminals[path.State]=path;
        var matches=terminals.Values.OrderBy(p=>p.Cost).ThenBy(p=>p.State).Take(beamWidth).Select(path=>
        {
            var labels=new ushort[path.Depth];var glyphs=new LegacyGraphGlyphPosition[path.Depth];Path? current=path;
            for(int i=path.Depth-1;i>=0;i--){labels[i]=current!.Label;glyphs[i]=current.Glyph!.Value;current=current.Parent;}
            return new LegacyGraphWordMatch(labels,Array.AsReadOnly(glyphs),path.Acoustic,path.Cost,path.State);
        }).ToArray();
        return new(Array.AsReadOnly(matches),scored,expanded);
    }
    private sealed record Path(int State,ushort Label,Path? Parent,LegacyGraphGlyphPosition? Glyph,
        int Acoustic,int Cost,int Depth);
}
