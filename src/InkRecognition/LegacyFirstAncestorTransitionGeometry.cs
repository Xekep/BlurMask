namespace InkRecognition;

/// <summary>First1EE3B0 transition with both original acoustic parent and its
/// geometric ancestor. GraphF0/F8 use OriginalNode/OriginalRecord.ArcOrdinal;
/// sampler118/140 use GeometryNode/GeometryRecord.ArcOrdinal.</summary>
public readonly record struct LegacyFirstAncestorTransitionRequest(int CurrentNode,ushort CurrentArc,
    ushort CurrentSymbol,byte CurrentCategory,byte CurrentSpan,byte Kind,LegacyFirstGeometricAncestor Previous);

/// <summary>Scoped fresh one-descriptor zero-source-flag first sampler bridge.
/// Ordinary0/2, blank5 and first6 preserve native distinct parent endpoints.
/// Opaque/protocol/cross-descriptor/primitive-provider exceptions are refused
/// by the owned ordinary graph scope, not filled with default corrections.</summary>
public sealed class LegacyFirstAncestorTransitionGeometry
{
    private readonly LegacyFirstOrdinaryTransitionGeometry ordinary;
    private readonly LegacyFirstBlankTransitionGeometry blank;
    private readonly LegacyFirstTransitionGeometryProfile profile;
    private readonly LegacyOrdinaryCursiveCandidateGraph graph;
    private readonly int[][][] matrices;
    private readonly float height;
    private readonly ushort replacement;
    public LegacyOrdinaryGraphSourceRanges SourceRanges {get;}

    //Existing no-skipped literal context callbacks stay inside their original
    //narrow parent validation. The producer checks the entire primary scope
    //before using these; these methods do not erase a geometric ancestor.
    internal LegacyFirstOrdinaryTransitionResult CorrectAdjacentLiteral118(LegacyFirstOrdinaryTransitionRequest r,int cost,CancellationToken token)
        =>ordinary.CorrectLiteral118(r,cost,token);
    internal LegacyFirstOrdinaryTransitionResult CorrectAdjacentLiteral140(LegacyFirstOrdinaryTransitionRequest r,int cost,CancellationToken token)
        =>ordinary.CorrectLiteral140(r,cost,token);

    public LegacyFirstAncestorTransitionGeometry(ReadOnlySpan<byte> resource,LegacyFreshFirstResultOwnershipFactory factory,
        LegacySmoothedRowCutResult cuts,LegacyFreshFirstCandidateGraphPreparation prepared,LegacyCursiveSeparationTable table,
        CancellationToken cancellationToken=default)
    {
        blank=new(resource,factory,cuts,prepared,table,cancellationToken);SourceRanges=prepared.SourceRanges;graph=SourceRanges.Graph;
        profile=LegacyFirstTransitionGeometryProfile.ReadSource(resource,factory.InvocationPolicy.Binding.SourceOffset,cancellationToken);
        ordinary=new(profile,factory.Frame,cuts,graph,table,cancellationToken);height=factory.Frame.Row.Frame.Line.Height;
        replacement=profile.Alphabet.TryGetLabel("\ufffd",out ushort id)?id:ushort.MaxValue;
        matrices=new int[graph.NodeCount][][];
        for(int n=0;n<matrices.Length;n++)
        {
            cancellationToken.ThrowIfCancellationRequested();var arcs=graph.GetNode(n);matrices[n]=new int[arcs.Count][];
            for(int a=0;a<arcs.Count;a++)
            {
                matrices[n][a]=new int[36];int parent=n-arcs[a].Span-1;if(parent<0){Array.Fill(matrices[n][a],0x4000000);continue;}
                float prior=-float.MaxValue;foreach(int p in arcs[a].PredecessorArcIndices.Span)prior=Math.Max(prior,ordinary.GetPreparedState(parent,p).PrefixMaximumX);
                if(prior==-float.MaxValue)prior=ordinary.GetBounds(n,a).MaximumX;
                float gap=(ordinary.GetPreparedState(n,a).FutureMinimumX-prior)/height;
                for(int i=0;i<36;i++)
                {
                    int cost=LegacyFirstOrdinaryTransitionGeometry.LogCost((gap-profile.GapMatrix.Span[i])*profile.GapSlope,table.Values.Span,false);
                    matrices[n][a][i]=i/6==5?checked(cost-1):i%6==5?cost:Math.Min(cost,profile.MatrixMaximumCost);
                }
            }
        }
    }

    public LegacyFirstOrdinaryTransitionResult Correct118(LegacyFirstAncestorTransitionRequest request,int incomingCost,
        CancellationToken cancellationToken=default)
    {
        Check(request,cancellationToken);if(request.Kind==6)return new(true,incomingCost);
        var previous=request.Previous.GeometryRecord;
        if(request.Kind==5)return blank.Correct118(request.CurrentNode,request.CurrentArc,incomingCost,cancellationToken);
        bool xor=(request.CurrentSymbol==replacement)!=(previous.Symbol==replacement);
        if(request.Kind==0&&xor)return new(false,0x4000000); //1A7F20 ->4.
        if(request.Kind==2)
        {
            //1525E0 checks whole starts BEFORE graph/sampler198. The supplied
            //ordinary groups have kind0, so its kind1 primitive exception fails.
            if(!ordinary.HasWholePhysicalStarts(request.CurrentNode,request.CurrentArc))return new(false,0x4000000);
            if(xor)return new(true,incomingCost); //1A8670 ->3, no matrix/translation.
        }
        int delta;
        if(request.Kind==0)delta=ordinary.GetPreparedState(request.CurrentNode,request.CurrentArc).JoinCost;
        else
        {
            int c=profile.Categories.CompositeCategories.Span[request.CurrentCategory].FirstId;
            int p=profile.Categories.CompositeCategories.Span[previous.CompositeCategory].FirstId;
            if(c is <0 or >5||p is <0 or >5)throw new NotSupportedException("Ordinary six-column category matrix required.");
            delta=matrices[request.CurrentNode][request.CurrentArc][c+p*6];
        }
        var current=ordinary.GetBounds(request.CurrentNode,request.CurrentArc);var prior=ordinary.GetBounds(request.Previous.GeometryNode,previous.ArcOrdinal);
        float displacement=Math.Max(current.MinimumX-prior.MinimumX,current.MaximumX-prior.MaximumX)/height;
        float value=0;
        if(profile.GapSlope>=0?displacement<=profile.TranslationThreshold:displacement>=profile.TranslationThreshold)
            value=(displacement-profile.TranslationThreshold)*profile.GapSlope;
        if(!float.IsFinite(value)||value>=2147483648f||value<int.MinValue)throw new NotSupportedException("Checked translation integer domain exceeded.");
        int cost=checked(incomingCost+delta-checked((int)value*8192));return new(cost<0x4000000,cost);
    }

    public LegacyFirstOrdinaryTransitionResult Correct140(LegacyFirstAncestorTransitionRequest request,int incomingCost,
        CancellationToken cancellationToken=default)
    {
        Check(request,cancellationToken);if(request.Kind==6)return new(true,incomingCost);
        var previous=request.Previous.GeometryRecord;
        if(request.CurrentSymbol==replacement||previous.Symbol==replacement)return new(incomingCost<0x4000000,incomingCost);
        int c=profile.Categories.CompositeCategories.Span[request.CurrentCategory].GeometryIndex;
        int p=profile.Categories.CompositeCategories.Span[previous.CompositeCategory].GeometryIndex;
        int delta=LegacyFirstOrdinaryTransitionGeometry.PairCost(ordinary.GetBounds(request.CurrentNode,request.CurrentArc),
            ordinary.GetBounds(request.Previous.GeometryNode,previous.ArcOrdinal),profile.Categories.Prototypes.Span[c],
            profile.Categories.Prototypes.Span[p],height,profile.PairScale,profile.PairOffset,profile.PairMaximum);
        int cost=checked(incomingCost+delta);return new(cost<0x4000000,cost);
    }

    private void Check(LegacyFirstAncestorTransitionRequest r,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var p=r.Previous;
        if(r.Kind is not (0 or 2 or 5 or 6)||p.OriginalNode!=r.CurrentNode-r.CurrentSpan-1||p.GeometryNode<0||
            p.GeometryNode>p.OriginalNode||r.CurrentSymbol>profile.Alphabet.LabelCount+2||p.GeometryRecord.Symbol>profile.Alphabet.LabelCount+2||
            r.CurrentCategory>=profile.Categories.CompositeCategories.Length||p.GeometryRecord.CompositeCategory>=profile.Categories.CompositeCategories.Length)
            throw new ArgumentException("Original and geometric parent fields outside owned first transition scope.");
        _=ordinary.GetBounds(r.CurrentNode,r.CurrentArc);_=ordinary.GetBounds(p.OriginalNode,p.OriginalRecord.ArcOrdinal);_=ordinary.GetBounds(p.GeometryNode,p.GeometryRecord.ArcOrdinal);
        var arc=graph.GetNode(r.CurrentNode)[r.CurrentArc];
        if(arc.Span!=r.CurrentSpan||!arc.PredecessorArcIndices.Span.Contains((int)p.OriginalRecord.ArcOrdinal))
            throw new ArgumentException("Original acoustic parent is not in native graphF0/F8 compatibility.");
    }
}
