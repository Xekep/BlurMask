namespace InkRecognition;

/// <summary>A caller-prepared native1ee3b0 glyph request, in native enumeration
/// order. Regular labels only: no Arabic traits, primitive/special symbols or
/// source scope correction. Geometry/category is already included in GroupCost.</summary>
public readonly record struct LegacyFirstOrdinaryGlyphRequest(ushort ArcOrdinal,byte Span,ushort Symbol,
    byte CompositeCategory,int GroupCost,byte LikelihoodByte);

/// <summary>Explicit frontend118 then140 request. Kind0 and kind2 are distinct
/// native continuation alternatives. Previous/current categories and graph
/// ordinals are retained so a real sampler correction can be supplied.</summary>
public readonly record struct LegacyFirstOrdinaryTransitionRequest(int CurrentNode,ushort CurrentArc,
    ushort CurrentSymbol,byte CurrentCategory,int PreviousNode,ushort PreviousArc,ushort PreviousSymbol,
    byte PreviousCategory,byte CurrentSpan,byte Kind);
public readonly record struct LegacyFirstOrdinaryTransitionResult(bool Accepted,int Cost);
public delegate LegacyFirstOrdinaryTransitionResult LegacyFirstOrdinaryTransitionCorrection(
    LegacyFirstOrdinaryTransitionRequest request,int incomingCost,CancellationToken cancellationToken);

/// <summary>Plain same-source, zero-Arabic-trait branch of native23f1ee3b0,
/// backed by native graph50/58 and1ee030 records. Mandatory native geometry
/// corrections are explicit; there is deliberately no zero-cost default.
/// The three later text-context passes and final SDK winner remain separate.</summary>
public sealed class LegacyFirstOrdinaryAcousticRecurrence
{
    private readonly LegacyFirstAcousticRecordTable[] rows;
    public int NodeCount=>rows.Length;
    public int BeamWidth {get;}
    private LegacyFirstOrdinaryAcousticRecurrence(LegacyFirstAcousticRecordTable[] rows,int width)
    {this.rows=rows;BeamWidth=width;}

    //Internal bridge for independently verified owning primary producers. It
    //copies storage; callers cannot mutate the returned recurrence's tables.
    internal static LegacyFirstOrdinaryAcousticRecurrence FromOwnedRows(IReadOnlyList<LegacyFirstAcousticRecordTable> source,
        int width,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(source);cancellationToken.ThrowIfCancellationRequested();
        if(source.Count is <1 or >4096||width is <1 or >256)throw new ArgumentException("Bounded owning primary rows and width required.");
        var owned=new LegacyFirstAcousticRecordTable[source.Count];
        for(int n=0;n<owned.Length;n++)
        {
            cancellationToken.ThrowIfCancellationRequested();var row=source[n]??throw new ArgumentException("Null owned primary row.");
            if(row.Capacity!=width)throw new ArgumentException("Primary row capacity differs from native width.");
            owned[n]=new(row.CopyStorage(),row.Count);
            for(int a=0;a<owned[n].Count;a++)
            {
                var r=owned[n].Get(a);int parent=n-r.Span-1;
                if(r.TotalCost>=0x4000000||parent<-1||parent==-1&&r.PreviousAlternative!=255||
                    parent>=0&&r.PreviousAlternative>=owned[parent].Count)
                    throw new InvalidDataException("Invalid finite primary source/predecessor record.");
            }
        }
        return new(owned,width);
    }
    public LegacyFirstAcousticRecordTable GetRowCopy(int node)
    {
        if((uint)node>=(uint)rows.Length)throw new ArgumentOutOfRangeException(nameof(node));
        return new(rows[node].CopyStorage(),rows[node].Count);
    }

    /// <summary>Caller must establish ordinary labels and empty native Arabic
    /// traits before supplying requests. Callback118 runs before140 for each
    /// compatible previous record; rejection short-circuits140. Signed integer
    /// arithmetic is checked rather than silently reproducing native overflow.</summary>
    public static LegacyFirstOrdinaryAcousticRecurrence Run(LegacyOrdinaryCandidateConnections connections,
        IReadOnlyList<IReadOnlyList<LegacyFirstOrdinaryGlyphRequest>> requests,int beamWidth,
        LegacyFirstOrdinaryTransitionCorrection correction118,LegacyFirstOrdinaryTransitionCorrection correction140,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(connections);ArgumentNullException.ThrowIfNull(requests);
        ArgumentNullException.ThrowIfNull(correction118);ArgumentNullException.ThrowIfNull(correction140);
        cancellationToken.ThrowIfCancellationRequested();
        if(beamWidth is <1 or >256||requests.Count!=connections.NodeCount||requests.Count>4096)
            throw new ArgumentException("Bounded native beam and owned node requests required.");
        var copied=new LegacyFirstOrdinaryGlyphRequest[requests.Count][];int requestCount=0;
        for(int node=0;node<copied.Length;node++)
        {
            cancellationToken.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(requests[node]);
            copied[node]=requests[node].ToArray();requestCount=checked(requestCount+copied[node].Length);
            if(requestCount>1_000_000)throw new ArgumentException("Glyph request budget exceeded.");
            var likelihoods=new Dictionary<(ushort Arc,ushort Symbol),byte>();
            foreach(var g in copied[node])
            {
                if(g.Span>7||g.Span>node||g.CompositeCategory>63||g.LikelihoodByte==0)
                    throw new ArgumentException("Native40-byte ordinary request fields exceed supported bounds.");
                if(likelihoods.TryGetValue((g.ArcOrdinal,g.Symbol),out byte previous)&&previous!=g.LikelihoodByte)
                    throw new ArgumentException("Native likelihood lookup must be consistent for a symbol/arc.");
                likelihoods[(g.ArcOrdinal,g.Symbol)]=g.LikelihoodByte;
            }
        }
        var rows=new LegacyFirstAcousticRecordTable[copied.Length];
        for(int node=0;node<rows.Length;node++)
        {
            cancellationToken.ThrowIfCancellationRequested();var row=rows[node]=new(beamWidth);
            foreach(var g in copied[node])
            {
                cancellationToken.ThrowIfCancellationRequested();
                if(g.GroupCost>=LegacyFirstAcousticRecordTable.ImpossibleCost)continue;
                int previousNode=node-g.Span-1;
                if(previousNode==-1)
                {
                    foreach(int ordinal in connections.GetSuccessorOrdinals(-1,ushort.MaxValue,g.Span,cancellationToken).Span)
                        if(ordinal==g.ArcOrdinal)
                            row.Insert(new(g.GroupCost,g.GroupCost,255,0,g.ArcOrdinal,g.Span,g.Symbol,true,0,g.CompositeCategory,0),cancellationToken);
                }
                else
                {
                    var previous=rows[previousNode];int bestCost=LegacyFirstAcousticRecordTable.ImpossibleCost,bestAlternative=-1;byte bestKind=0;
                    for(int i=0;i<previous.Count;i++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();var p=previous.Get(i);
                        if(p.TotalCost>=LegacyFirstAcousticRecordTable.ImpossibleCost)break;
                        foreach(int ordinal in connections.GetSuccessorOrdinals(previousNode,p.ArcOrdinal,g.Span,cancellationToken).Span)
                        {
                            if(ordinal!=g.ArcOrdinal)continue;
                            int first=Cost(0),second=Cost(2);byte kind=second<first?(byte)1:(byte)0;
                            int delta=Math.Min(first,second);
                            if(delta>=LegacyFirstAcousticRecordTable.ImpossibleCost)continue;
                            int subtotal=checked(p.TotalCost+delta);
                            if(subtotal>=LegacyFirstAcousticRecordTable.ImpossibleCost)continue;
                            int total=checked(subtotal+g.GroupCost);
                            //Native arrays retain the first previous alternative on ties.
                            if(total<bestCost){bestCost=total;bestAlternative=i;bestKind=kind;}
                            int Cost(byte requestKind)
                            {
                                var r=new LegacyFirstOrdinaryTransitionRequest(node,g.ArcOrdinal,g.Symbol,g.CompositeCategory,
                                    previousNode,p.ArcOrdinal,p.Symbol,p.CompositeCategory,g.Span,requestKind);
                                cancellationToken.ThrowIfCancellationRequested();var a=correction118(r,0,cancellationToken);
                                cancellationToken.ThrowIfCancellationRequested();
                                if(!a.Accepted)return LegacyFirstAcousticRecordTable.ImpossibleCost;
                                var b=correction140(r,a.Cost,cancellationToken);cancellationToken.ThrowIfCancellationRequested();
                                return b.Accepted?b.Cost:LegacyFirstAcousticRecordTable.ImpossibleCost;
                            }
                        }
                    }
                    if(bestAlternative>=0)
                    {
                        var p=previous.Get(bestAlternative);
                        row.Insert(new(bestCost,checked(g.GroupCost+p.AcousticCost),checked((byte)bestAlternative),0,
                            g.ArcOrdinal,g.Span,g.Symbol,bestKind==1,0,g.CompositeCategory,0),cancellationToken);
                    }
                }
            }
            //1ef070 queries likelihood after all insertions. It is keyed by the
            //stored symbol/arc, not by insertion destination or previous path.
            for(int i=0;i<row.Count;i++)
            {
                var record=row.Get(i);var match=copied[node].First(g=>g.ArcOrdinal==record.ArcOrdinal&&g.Symbol==record.Symbol);
                row.SetLikelihoodByte(i,match.LikelihoodByte);
            }
        }
        return new(rows,beamWidth);
    }

    /// <summary>Diagnostic primary path only, not the final273 text winner.
    /// Backtracks native previous-alternative and packed span fields.</summary>
    public LegacyFirstAcousticRecordFields[] TracePrimary(int endNode,int alternative,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if((uint)endNode>=(uint)rows.Length)throw new ArgumentOutOfRangeException(nameof(endNode));
        var result=new List<LegacyFirstAcousticRecordFields>();
        for(int node=endNode;node>=0;)
        {
            cancellationToken.ThrowIfCancellationRequested();var item=rows[node].Get(alternative);
            if(item.TotalCost>=LegacyFirstAcousticRecordTable.ImpossibleCost)throw new InvalidDataException("Cannot trace an impossible native record.");
            result.Add(item);node-=item.Span+1;
            if(node<-1||node==-1&&item.PreviousAlternative!=255)throw new InvalidDataException("Invalid native source-root identity.");
            alternative=item.PreviousAlternative;
        }
        result.Reverse();return result.ToArray();
    }
}
