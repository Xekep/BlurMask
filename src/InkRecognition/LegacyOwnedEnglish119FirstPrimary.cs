namespace InkRecognition;

/// <summary>Actual fresh119 native1EF070/1EE3B0 primary98 producer. Uses the
/// same owned cuts, integer cache, ranked weights and108 geometry. B0/B8,
/// terminal273 and text publication are not implemented by this endpoint.</summary>
public sealed class LegacyOwnedEnglish119FirstPrimary
{
    public LegacyOwnedEnglish119FirstPolicy Policy {get;}
    public LegacyOwnedEnglish119ControllerCosts Owner=>Policy.Owner;
    public int NodeCount=>requests.Length;
    public int RequestCount {get;}
    public int FallbackProposalCount {get;}
    private readonly LegacyFirstOrdinaryGlyphRequest[][] requests;
    private readonly ushort[][] shortlistOrdinals;
    private readonly LegacyOrdinaryCandidateConnections connections;
    internal LegacyOwnedEnglish119TransitionGeometry Geometry {get;}
    private readonly object gate=new();
    private LegacyEnglish119PrimaryResult? completed;
    private LegacyOwnedEnglish119FirstPrimary(LegacyOwnedEnglish119FirstPolicy policy,
        LegacyOwnedEnglish119TransitionGeometry geometry,LegacyFirstOrdinaryGlyphRequest[][] requests,ushort[][] shortlistOrdinals,int count,int blanks,
        CancellationToken token)
    {Policy=policy;Geometry=geometry;this.requests=requests;this.shortlistOrdinals=shortlistOrdinals;RequestCount=count;FallbackProposalCount=blanks;connections=new(policy.SourceRanges.Graph,token);}

    public static LegacyOwnedEnglish119FirstPrimary Prepare(LegacyOwnedEnglish119ControllerCosts owner,
        LegacyCursiveSeparationTable nativeTable,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(owner);ArgumentNullException.ThrowIfNull(nativeTable);token.ThrowIfCancellationRequested();
        var policy=LegacyOwnedEnglish119FirstPolicy.Prepare(owner,token);var graph=policy.SourceRanges.Graph;
        for(int n=0;n<graph.NodeCount;n++)if(graph.GetNode(n).Count>ushort.MaxValue)
            throw new NotSupportedException("Primary per-node arc count exceeds the U16 native ordinal domain.");
        var geometry=new LegacyOwnedEnglish119TransitionGeometry(policy,nativeTable,token);var categories=policy.Categories;
        var ordered=new LegacyFirstOrdinaryGlyphRequest[graph.NodeCount][];var ordinals=new ushort[graph.NodeCount][];int total=0,blanks=0;
        bool hasBlankHeader=owner.Model.Source.Alphabet.CopySerializedHeader().AsSpan(6,2).SequenceEqual(new byte[]{255,255})==false;
        for(int n=0;n<ordered.Length;n++)
        {
            token.ThrowIfCancellationRequested();var requests=new List<LegacyFirstOrdinaryGlyphRequest>();var shortlist=new List<ushort>();var arcs=graph.GetNode(n);
            int extent=arcs.Count==0?0:arcs.Max(a=>(int)a.Span)+1;
            for(byte span=0;span<extent;span++)for(ushort a=0;a<arcs.Count;a++)
            {
                token.ThrowIfCancellationRequested();if(arcs[a].Span!=span)continue;
                var group=owner.ScoreGroup(arcs[a].GroupId,token);var entries=group.CopyEntries();int top=Math.Min(20,entries.Length);
                bool containsSpace=false;for(int i=0;i<top;i++)if(entries[i].Symbol==policy.Space)containsSpace=true;
                for(int i=0;i<top;i++)
                {
                    ushort s=entries[i].Symbol;
                    if(s==policy.Shadda||s==policy.Newline||s==policy.Space||containsSpace&&s==policy.NonbreakingSpace)continue;
                    if(n==span&&s==policy.Closed)continue;
                    for(int end=0;end<2;end++)
                    {
                        int category=(end==0?categories.StartCompositeMap:categories.EndCompositeMap).Span[s];if(category<0)continue;
                        if(categories.CompositeCategories.Span[category].FirstId is <0 or >5)throw new NotSupportedException("Regular primary requires resolved first-ID0..5.");
                        Add(s,category,checked((ushort)i));
                    }
                }
                if(hasBlankHeader&&span<n)
                {
                    int category=categories.StartCompositeMap.Span[policy.Blank];if(category<0)throw new NotSupportedException("Actual blank map required.");
                    Add(policy.Blank,category,ushort.MaxValue);blanks++;
                }
                void Add(ushort symbol,int composite,ushort ordinal)
                {
                    token.ThrowIfCancellationRequested();if(++total>1_000_000)throw new ArgumentException("Portable primary proposal budget exceeded.");
                    int prototype=categories.CompositeCategories.Span[composite].GeometryIndex;
                    requests.Add(new(a,span,symbol,checked((byte)composite),group.ReadClassCost(symbol,ordinal,prototype,true,false,token),group.ReadLikelihoodByte(symbol,token)));
                    shortlist.Add(ordinal);
                }
            }
            ordered[n]=requests.ToArray();ordinals[n]=shortlist.ToArray();
        }
        token.ThrowIfCancellationRequested();return new(policy,geometry,ordered,ordinals,total,blanks,token);
    }
    public LegacyFirstOrdinaryGlyphRequest[] CopyRequests(int node)
    {if((uint)node>=(uint)requests.Length)throw new ArgumentOutOfRangeException(nameof(node));return requests[node].ToArray();}
    internal int[][] CopyClassRequests(int node)
    {
        var copy=CopyRequests(node);return copy.Select((r,i)=>new[]{(int)r.ArcOrdinal,r.Span,r.Symbol,shortlistOrdinals[node][i],r.CompositeCategory,Policy.Categories.CompositeCategories.Span[r.CompositeCategory].GeometryIndex,r.GroupCost}).ToArray();
    }
    public LegacyEnglish119PrimaryResult Run(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {token.ThrowIfCancellationRequested();if(completed is not null)return completed;var result=RunCore(null,token);token.ThrowIfCancellationRequested();completed=result;return result;}
    }
    /// <summary>Observer receives value snapshots only, cannot supply geometry
    /// success/costs. This fresh numeric run reuses already-cached NNs.</summary>
    public LegacyEnglish119PrimaryResult RunWithObserver(Action<int,LegacyFirstAncestorTransitionRequest,int,LegacyFirstOrdinaryTransitionResult> observer,
        CancellationToken token=default)
    {ArgumentNullException.ThrowIfNull(observer);return RunCore(observer,token);}
    private LegacyEnglish119PrimaryResult RunCore(Action<int,LegacyFirstAncestorTransitionRequest,int,LegacyFirstOrdinaryTransitionResult>? observer,CancellationToken token)
    {
        var p=Policy;var primary=LegacyFirstPrimaryRecurrenceKernel.Run(requests,p.PrimaryBeamWidth,p.Blank,p.Replacement,p.Closed,p.Shadda,p.Newline,p.Space,p.NonbreakingSpace,
            p.ExcludeConsecutiveBlank,connections,Geometry.Correct118,Geometry.Correct140,observer,token);
        return new(this,primary);
    }
}

/// <summary>Completed same-owner primary98 rows. Not the merged B8 table or
/// a final selected word. Copies of rows cannot mutate the owned recurrence.</summary>
public sealed class LegacyEnglish119PrimaryResult
{
    public LegacyOwnedEnglish119FirstPrimary Owner {get;}
    public LegacyFirstOrdinaryAcousticRecurrence Primary {get;}
    internal LegacyEnglish119PrimaryResult(LegacyOwnedEnglish119FirstPrimary owner,LegacyFirstOrdinaryAcousticRecurrence primary)
    {Owner=owner;Primary=primary;}
}
