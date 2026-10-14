using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Owned/pruned fresh first regular+blank primary branch1EF070/1EE3B0.
/// Selected-source247 B8 supplies consecutive-blank exclusion. Protocol194
/// remains explicit until its invocation ownership is proved. No script filter.</summary>
public sealed class LegacyFreshFirstBlankPrimary
{
    private readonly LegacyFirstOrdinaryGlyphRequest[][] requests;
    private readonly IReadOnlyList<IReadOnlyList<LegacyFirstOrdinaryGlyphRequest>> requestView;
    private readonly LegacyFirstAncestorTransitionGeometry geometry;
    private readonly LegacyOrdinaryCandidateConnections connections;
    private readonly LegacyOrdinaryRankedAcousticGroup[] rankedGroups;
    private readonly LegacyFreshFirstLiteralContextPolicy literal;
    private readonly int width;
    private readonly ushort blank,replacement,closed,shadda,newline,space,nbsp;
    private readonly bool excludeConsecutiveBlank;
    public LegacyOrdinaryGraphSourceRanges SourceRanges {get;}
    public bool NativeConsecutiveBlankExclusion=>excludeConsecutiveBlank;
    public IReadOnlyList<IReadOnlyList<LegacyFirstOrdinaryGlyphRequest>> Requests=>requestView;
    private LegacyFreshFirstBlankPrimary(LegacyOrdinaryGraphSourceRanges source,LegacyFirstOrdinaryGlyphRequest[][] requests,
        LegacyFirstAncestorTransitionGeometry geometry,int width,ushort blank,ushort replacement,ushort closed,ushort shadda,
        ushort newline,ushort space,ushort nbsp,bool exclude,LegacyOrdinaryRankedAcousticGroup[] ranked,
        LegacyFreshFirstLiteralContextPolicy literal,CancellationToken token)
    {SourceRanges=source;this.requests=requests;this.geometry=geometry;this.width=width;this.blank=blank;this.replacement=replacement;
        this.closed=closed;this.shadda=shadda;this.newline=newline;this.space=space;this.nbsp=nbsp;excludeConsecutiveBlank=exclude;
        connections=new(source.Graph,token);rankedGroups=ranked;this.literal=literal;
        requestView=Array.AsReadOnly(requests.Select(r=>(IReadOnlyList<LegacyFirstOrdinaryGlyphRequest>)Array.AsReadOnly(r)).ToArray());}

    public static LegacyFreshFirstBlankPrimary Prepare(ReadOnlySpan<byte> resource,LegacyFreshFirstResultOwnershipFactory factory,
        LegacySmoothedRowCutResult cuts,LegacyFreshFirstCandidateGraphPreparation prepared,
        IReadOnlyList<LegacyFirstOrdinaryCacheScores> scores,LegacyCursiveSeparationTable table,bool nativeProtocolExclusion194,
        CancellationToken cancellationToken=default)
    {
        var geometry=new LegacyFirstAncestorTransitionGeometry(resource,factory,cuts,prepared,table,cancellationToken);
        var graph=prepared.SourceRanges.Graph;int sourceOffset=factory.InvocationPolicy.Binding.SourceOffset;
        var regular=LegacyFreshFirstRegularPrimary.Prepare(resource,sourceOffset,factory,cuts,graph,scores,table,nativeProtocolExclusion194,cancellationToken);
        var profile=LegacyFirstTransitionGeometryProfile.ReadSource(resource,sourceOffset,cancellationToken);
        var separation=LegacyCursiveSeparationProfile.ReadV106Node(resource,profile.SamplerOffset);
        var alphabet=factory.InvocationPolicy.Binding.Alphabet;ushort blank=checked((ushort)alphabet.LabelCount);
        int b8=profile.RowProfile.Source.ParametersOffset-6;
        if(b8<sourceOffset||b8>=resource.Length||resource[b8]>1)throw new InvalidDataException("Owned selected-source247 B8 required.");
        bool nativeConsecutiveBlankExclusion=resource[b8]!=0;
        int category=profile.Categories.StartCompositeMap.Span[blank];if(category<0)throw new NotSupportedException("Owned blank category required.");
        int p=checked(sourceOffset+20+4+56);int marker=BinaryPrimitives.ReadInt32LittleEndian(resource[p..]);p+=4;if(marker==-1)p+=4;p+=11;
        var groups=new LegacyOrdinaryRankedAcousticGroup[graph.Groups.Count];
        for(int g=0;g<groups.Length;g++)
        {
            cancellationToken.ThrowIfCancellationRequested();var ranges=graph.Groups[g].Ranges.ToArray();
            int cost=LegacyCursiveGroupSeparationCost.Evaluate(cuts.Cuts.PreparedInk,ranges,factory.Frame.Row.Frame.Line.Height,
                separation.GapOffset,separation.Slope,table.Values.Span,separation.MaximumCost,cancellationToken:cancellationToken);
            groups[g]=new(scores[g].Cache,alphabet,resource.Slice(p,16),Enumerable.Repeat(cost,profile.Categories.CompositeCategories.Length).ToArray(),1);
        }
        var ordered=new LegacyFirstOrdinaryGlyphRequest[graph.NodeCount][];var pending=regular.PendingBlankProposals.ToArray().ToLookup(b=>(b.Node,b.Arc));
        int total=0;
        for(int n=0;n<ordered.Length;n++)
        {
            cancellationToken.ThrowIfCancellationRequested();var entries=new List<LegacyFirstOrdinaryGlyphRequest>();var arcs=graph.GetNode(n);
            var byArc=regular.Requests[n].ToLookup(r=>r.ArcOrdinal);int extent=arcs.Count==0?0:arcs.Max(a=>(int)a.Span)+1;
            for(byte span=0;span<extent;span++)for(ushort a=0;a<arcs.Count;a++)
            {
                if(arcs[a].Span!=span)continue;entries.AddRange(byArc[a]);
                if(pending.Contains((n,a)))
                {
                    var ranked=groups[arcs[a].GroupId];int cost=ranked.Query(blank,ushort.MaxValue,category,true,false,cancellationToken);
                    entries.Add(new(a,span,blank,(byte)category,cost,ranked.GetFirstControllerLikelihoodByte(blank,cancellationToken)));
                }
            }
            total=checked(total+entries.Count);if(total>1_000_000)throw new ArgumentException("Portable first primary proposal budget exceeded.");ordered[n]=entries.ToArray();
        }
        ushort replacement=alphabet.TryGetLabel("\ufffd",out ushort r)?r:ushort.MaxValue;
        ushort closed=alphabet.TryGetLabel("\U000f0001",out ushort c)?c:ushort.MaxValue;
        return new(prepared.SourceRanges,ordered,geometry,factory.PrimaryBeamWidthF8,blank,replacement,closed,factory.InvocationPolicy.Binding.TrailingSymbolE6,
            BinaryPrimitives.ReadUInt16LittleEndian(resource[(p+8)..]),BinaryPrimitives.ReadUInt16LittleEndian(resource[(p+10)..]),
            BinaryPrimitives.ReadUInt16LittleEndian(resource[(p+12)..]),nativeConsecutiveBlankExclusion,groups,
            LegacyFreshFirstLiteralContextPolicy.ReadSource(resource,sourceOffset,factory,cancellationToken),cancellationToken);
    }

    public int ReadGroupCost(int node,ushort arc,ushort symbol,byte category,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();var graph=SourceRanges.Graph;
        if((uint)node>=(uint)graph.NodeCount||arc>=graph.GetNode(node).Count)throw new ArgumentOutOfRangeException(nameof(node));
        return rankedGroups[graph.GetNode(node)[arc].GroupId].Query(symbol,ushort.MaxValue,category,true,false,cancellationToken);
    }

    /// <summary>Consume this actual producing kernel's complete primary when
    /// every retained row satisfies the restored no-skipped/nonreplacement
    /// literal context scope. Refuses the whole input otherwise; no filtering
    /// of blank/header/replacement rows and no fabricated context corrections.</summary>
    public LegacyFirstOrdinaryContextPasses RunSupportedNonSkippedContext(CancellationToken cancellationToken=default)
    {
        var primary=Run(cancellationToken);
        for(int n=0;n<primary.NodeCount;n++)
        {
            cancellationToken.ThrowIfCancellationRequested();var row=primary.GetRowCopy(n);
            for(int a=0;a<row.Count;a++)
            {
                var r=row.Get(a);
                if(!literal.SupportsSymbol(r.Symbol)||r.Symbol==replacement||r.Traits!=0)
                    throw new NotSupportedException("Entire primary requires native blank/ancestor/replacement context handling; rows cannot be discarded.");
            }
        }
        return LegacyFirstOrdinaryContextPasses.RunForFreshLiteralSymbols(primary,connections,literal,ReadGroupCost,
            geometry.CorrectAdjacentLiteral118,geometry.CorrectAdjacentLiteral140,new(),cancellationToken);
    }

    /// <summary>Runs real prepared ranked costs plus fallback queries and sampler
    /// callbacks. This produces the primary98 table, not the final text winner.</summary>
    public LegacyFirstOrdinaryAcousticRecurrence Run(CancellationToken cancellationToken=default)=>RunCore(null,cancellationToken);

    /// <summary>Read-only observation of actual computed sampler corrections.
    /// Observer cannot provide success flags/costs; cancellation is checked on
    /// both sides of each observer invocation.</summary>
    public LegacyFirstOrdinaryAcousticRecurrence RunWithObserver(
        Action<int,LegacyFirstAncestorTransitionRequest,int,LegacyFirstOrdinaryTransitionResult> observer,
        CancellationToken cancellationToken=default)
    {ArgumentNullException.ThrowIfNull(observer);return RunCore(observer,cancellationToken);}

    private LegacyFirstOrdinaryAcousticRecurrence RunCore(
        Action<int,LegacyFirstAncestorTransitionRequest,int,LegacyFirstOrdinaryTransitionResult>? observer,CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();var rows=new LegacyFirstAcousticRecordTable[requests.Length];
        for(int n=0;n<rows.Length;n++)
        {
            cancellationToken.ThrowIfCancellationRequested();var target=rows[n]=new(width);
            foreach(var g in requests[n])
            {
                cancellationToken.ThrowIfCancellationRequested();if(g.GroupCost>=0x4000000)continue;int parent=n-g.Span-1;
                if(parent==-1)
                {
                    if(g.Symbol==closed)continue;
                    if(connections.GetSuccessorOrdinals(-1,ushort.MaxValue,g.Span,cancellationToken).Span.Contains((int)g.ArcOrdinal))
                        target.Insert(new(g.GroupCost,g.GroupCost,255,0,g.ArcOrdinal,g.Span,g.Symbol,true,0,g.CompositeCategory,0),cancellationToken);
                    continue;
                }
                var prior=rows[parent];int best=0x4000000,bestAlternative=-1;byte bestType=0;
                for(int a=0;a<prior.Count;a++)
                {
                    cancellationToken.ThrowIfCancellationRequested();var original=prior.Get(a);if(original.TotalCost>=0x4000000)break;
                    if(excludeConsecutiveBlank&&g.Symbol==blank&&original.Symbol==blank)continue;
                    //F0/F8 use original parent+arc even after the geometric walk.
                    if(!connections.GetSuccessorOrdinals(parent,original.ArcOrdinal,g.Span,cancellationToken).Span.Contains((int)g.ArcOrdinal))continue;
                    int an=parent,aa=a,skipped=0;var ancestor=original;
                    while(ancestor.Symbol==blank||ancestor.Symbol==shadda||ancestor.Symbol==newline||ancestor.Symbol==space||ancestor.Symbol==nbsp)
                    {
                        cancellationToken.ThrowIfCancellationRequested();int previous=an-ancestor.Span-1;if(previous==-1)break;
                        aa=ancestor.PreviousAlternative;an=previous;ancestor=rows[an].Get(aa);skipped++;
                    }
                    var owners=new LegacyFirstGeometricAncestor(parent,a,original,an,aa,ancestor,skipped);
                    int first=0x4000000,second=0x4000000;
                    if(g.Symbol!=newline&&g.Symbol!=space&&g.Symbol!=nbsp&&original.Symbol!=newline&&original.Symbol!=space&&original.Symbol!=nbsp&&
                        (g.Symbol==replacement)==(original.Symbol==replacement))
                        first=Cost(g.Symbol==blank?(byte)5:g.Symbol==closed?(byte)6:(byte)0);
                    if(g.Symbol!=blank&&g.Symbol!=shadda)second=Cost(2);
                    int delta=Math.Min(first,second);if(delta>=0x4000000)continue;
                    int subtotal=checked(original.TotalCost+delta);if(subtotal>=0x4000000)continue;
                    int value=checked(subtotal+g.GroupCost);if(value<best){best=value;bestAlternative=a;bestType=second<first?(byte)1:(byte)0;}
                    int Cost(byte kind)
                    {
                        var request=new LegacyFirstAncestorTransitionRequest(n,g.ArcOrdinal,g.Symbol,g.CompositeCategory,g.Span,kind,owners);
                        var a118=geometry.Correct118(request,0,cancellationToken);Observe(118,request,0,a118);
                        if(!a118.Accepted)return 0x4000000;
                        var a140=geometry.Correct140(request,a118.Cost,cancellationToken);Observe(140,request,a118.Cost,a140);return a140.Accepted?a140.Cost:0x4000000;
                    }
                }
                if(bestAlternative>=0)
                {
                    var original=prior.Get(bestAlternative);
                    target.Insert(new(best,checked(original.AcousticCost+g.GroupCost),(byte)bestAlternative,0,g.ArcOrdinal,g.Span,g.Symbol,bestType==1,0,g.CompositeCategory,0),cancellationToken);
                }
            }
            for(int a=0;a<target.Count;a++)
            {
                var r=target.Get(a);target.SetLikelihoodByte(a,requests[n].First(g=>g.ArcOrdinal==r.ArcOrdinal&&g.Symbol==r.Symbol).LikelihoodByte);
            }
        }
        return LegacyFirstOrdinaryAcousticRecurrence.FromOwnedRows(rows,width,cancellationToken);

        void Observe(int stage,LegacyFirstAncestorTransitionRequest request,int incoming,LegacyFirstOrdinaryTransitionResult result)
        {cancellationToken.ThrowIfCancellationRequested();observer?.Invoke(stage,request,incoming,result);cancellationToken.ThrowIfCancellationRequested();}
    }
}
