using System.Buffers.Binary;

namespace InkRecognition;

public readonly record struct LegacyFirstPendingBlankProposal(int Node,ushort Arc,byte Span);

/// <summary>Fresh23f native regular-label proposal order1EF070 and special
/// ordinary1EE3B0 callback guards. Blank5 proposals are reported separately;
/// this endpoint does not silently stand in for the complete native primary
/// controller. Protocol exclusion194 remains explicit until owner60 binding
/// is independently established. Scores/masks are actual supplied NN owners.</summary>
public sealed class LegacyFreshFirstRegularPrimary
{
    private readonly IReadOnlyList<LegacyFirstOrdinaryGlyphRequest>[] requests;
    private readonly LegacyOrdinaryCandidateConnections connections;
    private readonly LegacyFirstOrdinaryTransitionGeometry geometry;
    private readonly LegacyFreshFirstLiteralContextPolicy literal;
    private readonly ushort replacement,nbsp;
    private readonly LegacyFirstPendingBlankProposal[] blanks;
    private readonly int width;
    public LegacyOrdinaryCursiveCandidateGraph Graph {get;}
    public IReadOnlyList<IReadOnlyList<LegacyFirstOrdinaryGlyphRequest>> Requests=>Array.AsReadOnly(requests);
    public ReadOnlyMemory<LegacyFirstPendingBlankProposal> PendingBlankProposals=>blanks;
    public bool NativeProtocolExclusion194 {get;}
    private LegacyFreshFirstRegularPrimary(LegacyOrdinaryCursiveCandidateGraph graph,
        IReadOnlyList<LegacyFirstOrdinaryGlyphRequest>[] requests,LegacyFirstPendingBlankProposal[] blanks,
        LegacyFirstOrdinaryTransitionGeometry geometry,LegacyFreshFirstLiteralContextPolicy literal,
        ushort replacement,ushort nbsp,int width,bool exclusion,CancellationToken token)
    {Graph=graph;this.requests=requests;this.blanks=blanks;this.geometry=geometry;this.literal=literal;
        this.replacement=replacement;this.nbsp=nbsp;this.width=width;NativeProtocolExclusion194=exclusion;
        connections=new(graph,token);}

    public static LegacyFreshFirstRegularPrimary Prepare(ReadOnlySpan<byte> resource,int sourceOffset,
        LegacyFreshFirstResultOwnershipFactory factory,LegacySmoothedRowCutResult cuts,
        LegacyOrdinaryCursiveCandidateGraph graph,IReadOnlyList<LegacyFirstOrdinaryCacheScores> actualScores,
        LegacyCursiveSeparationTable table,bool nativeProtocolExclusion194,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(factory);ArgumentNullException.ThrowIfNull(cuts);ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(actualScores);ArgumentNullException.ThrowIfNull(table);cancellationToken.ThrowIfCancellationRequested();
        var literal=LegacyFreshFirstLiteralContextPolicy.ReadSource(resource,sourceOffset,factory,cancellationToken);
        var profile=LegacyFirstTransitionGeometryProfile.ReadSource(resource,sourceOffset,cancellationToken);
        int graphOffset=checked(profile.SamplerOffset+8+(int)BinaryPrimitives.ReadUInt32LittleEndian(resource[(profile.SamplerOffset+4)..]));
        var spanProfile=LegacyCursiveLatticeSpanProfile.ReadV10aResourceNode(resource,graphOffset);
        if(!ReferenceEquals(cuts.Owner,factory.Frame.Row)||actualScores.Count!=graph.Groups.Count||
            graph.MaximumAtoms!=spanProfile.MaximumAtoms||spanProfile.MaximumAtoms>8||graph.CandidateCount>131072)
            throw new ArgumentException("Owned actual first cuts/scores and native graph span cap required.");
        var geometry=new LegacyFirstOrdinaryTransitionGeometry(profile,factory.Frame,cuts,graph,table,cancellationToken);
        var separation=LegacyCursiveSeparationProfile.ReadV106Node(resource,profile.SamplerOffset);
        var categories=new LegacyCursiveCategoryMapper(profile.Categories);
        int p=checked(sourceOffset+20+4+56);int marker=BinaryPrimitives.ReadInt32LittleEndian(resource[p..]);p+=4;if(marker==-1)p+=4;p+=11;
        var alphabet=literal.Binding.Alphabet;ushort newline=U16(resource,p+8),space=U16(resource,p+10),nbsp=U16(resource,p+12);
        ushort replacement=alphabet.TryGetLabel("\ufffd",out ushort repl)?repl:ushort.MaxValue;
        var protocols=new HashSet<ushort>();
        foreach(string s in new[]{"\U000f0002","\U000f0003","\U000f0004","\U000f0008"})
            if(alphabet.TryGetLabel(s,out ushort id))protocols.Add(id);
        var ranked=new LegacyOrdinaryRankedAcousticGroup[graph.Groups.Count];
        for(int group=0;group<ranked.Length;group++)
        {
            cancellationToken.ThrowIfCancellationRequested();var scores=actualScores[group]??throw new ArgumentException("Null actual group scores.");
            factory.ValidateScores(scores.Glyph.Scores,cancellationToken);
            if(!ReferenceEquals(scores.Glyph.Geometry.PreparedInk,cuts.Cuts.PreparedInk))throw new ArgumentException("Foreign prepared ink.");
            var ranges=graph.Groups[group].Ranges.ToArray();int[] order=cuts.Chronology.OrderRangeIndicesForNormalization(ranges,cancellationToken);
            if(!scores.Glyph.Geometry.Ranges.Span.SequenceEqual(order.Select(i=>ranges[i]).ToArray()))throw new ArgumentException("Graph/NN group ranges differ.");
            int cost=LegacyCursiveGroupSeparationCost.Evaluate(cuts.Cuts.PreparedInk,ranges,factory.Frame.Row.Frame.Line.Height,
                separation.GapOffset,separation.Slope,table.Values.Span,separation.MaximumCost,cancellationToken:cancellationToken);
            ranked[group]=new(scores.Cache,alphabet,resource.Slice(p,16),Enumerable.Repeat(cost,categories.CompositeCategoryCount).ToArray(),1);
        }
        var requests=new IReadOnlyList<LegacyFirstOrdinaryGlyphRequest>[graph.NodeCount];var blanks=new List<LegacyFirstPendingBlankProposal>();int total=0;
        bool hasBlankHeader=U16(resource,p+6)!=ushort.MaxValue;
        for(int node=0;node<graph.NodeCount;node++)
        {
            cancellationToken.ThrowIfCancellationRequested();var entries=new List<LegacyFirstOrdinaryGlyphRequest>();var arcs=graph.GetNode(node);
            int extent=arcs.Count==0?0:arcs.Max(a=>(int)a.Span)+1;
            //151FE0->21b28=1A7580 is max(arc.span+1).1EF070 ascending
            //span then ascending ordinal;152030->21b70 tests this actual arc.
            for(byte span=0;span<extent;span++)for(ushort arc=0;arc<arcs.Count;arc++)
            {
                cancellationToken.ThrowIfCancellationRequested();if(arcs[arc].Span!=span)continue;
                var group=ranked[arcs[arc].GroupId];var top=group.Entries.Span[..Math.Min(20,group.Entries.Length)];
                bool containsSpace=false;foreach(var e in top)if(e.Symbol==space)containsSpace=true;
                foreach(var e in top)
                {
                    cancellationToken.ThrowIfCancellationRequested();ushort s=e.Symbol;
                    if(s==literal.Binding.TrailingSymbolE6||s==newline||s==space||containsSpace&&s==nbsp||nativeProtocolExclusion194&&protocols.Contains(s))continue;
                    //1EE3B0 root excludes closed E4 before graphF0/F8 callbacks.
                    if(node==span&&s==literal.ClosedSymbolE4)continue;
                    foreach(bool end in new[]{false,true})
                    {
                        int category=categories.GetSymbolCompositeCategory(s,end);
                        if(category<0)continue;
                        //Source280=false is exact ordinary21b.V1C8=05A400;
                        //both maps are visited, even when they yield duplicates.
                        if(categories.GetFirstId(category)>5)throw new NotSupportedException("Nonregular six-category proposal.");
                        if(++total>1_000_000)throw new ArgumentException("Portable primary proposal budget exceeded.");
                        entries.Add(new(arc,span,s,(byte)category,group.Query(s,ushort.MaxValue,category,true,false,cancellationToken),group.GetFirstControllerLikelihoodByte(s,cancellationToken)));
                    }
                }
                if(hasBlankHeader&&span<node)blanks.Add(new(node,arc,span));
            }
            requests[node]=entries.AsReadOnly();
        }
        cancellationToken.ThrowIfCancellationRequested();return new(graph,requests,blanks.ToArray(),geometry,literal,replacement,nbsp,
            factory.PrimaryBeamWidthF8,nativeProtocolExclusion194,cancellationToken);
    }

    /// <summary>Run the regular, nonblank/non-skipped subset. Pending blanks
    /// remain reported, not fabricated; NBSP ancestors are explicitly refused.
    /// This is not the full native primary winner. No Unicode script filter.</summary>
    public LegacyFirstOrdinaryAcousticRecurrence RunRegularLabels(CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(requests.Any(row=>row.Any(r=>r.Symbol==nbsp)))throw new NotSupportedException("Native NBSP ancestor traversal must be restored before running this input.");
        return LegacyFirstOrdinaryAcousticRecurrence.Run(connections,requests,width,
            Correct118,Correct140,cancellationToken);
    }
    public LegacyFirstOrdinaryTransitionResult Correct118(LegacyFirstOrdinaryTransitionRequest request,int value,
        CancellationToken cancellationToken=default)=>Correct(request,value,cancellationToken,false);
    public LegacyFirstOrdinaryTransitionResult Correct140(LegacyFirstOrdinaryTransitionRequest request,int value,
        CancellationToken cancellationToken=default)=>Correct(request,value,cancellationToken,true);
    private LegacyFirstOrdinaryTransitionResult Correct(LegacyFirstOrdinaryTransitionRequest request,int value,CancellationToken token,bool second)
    {
        token.ThrowIfCancellationRequested();literal.Validate(request.CurrentSymbol,0);literal.Validate(request.PreviousSymbol,0);
        if(request.Kind is not (0 or 2))throw new ArgumentException("Primary ordinary0/2 dispatch required.");
        if(request.Kind==0&&((request.CurrentSymbol==replacement)!=(request.PreviousSymbol==replacement)))return new(false,0x4000000);
        if(request.Kind==0&&request.CurrentSymbol==literal.ClosedSymbolE4)request=request with{Kind=6};
        return second?geometry.CorrectLiteral140(request,value,token):geometry.CorrectLiteral118(request,value,token);
    }
    private static ushort U16(ReadOnlySpan<byte> data,int p)=>BinaryPrimitives.ReadUInt16LittleEndian(data[p..]);
}
