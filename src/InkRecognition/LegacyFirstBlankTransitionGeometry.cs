using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Owned first ordinary graph1941B0/195C40 kind5 cache and195B30
/// pair geometry. Does not generate blank proposals, cache fallback costs or
/// traverse primary ancestors; those remain separate callers.</summary>
public sealed class LegacyFirstBlankTransitionGeometry
{
    private readonly LegacyFirstOrdinaryTransitionGeometry ordinary;
    private readonly LegacyFirstTransitionGeometryProfile profile;
    private readonly LegacyOrdinaryCursiveCandidateGraph graph;
    private readonly int[][] costs;
    private readonly float height;
    private readonly ushort replacement;
    public float Threshold70 {get;}
    public bool RequireWholePhysicalStarts10C {get;}
    public int BlankCompositeCategory {get;}
    public LegacyOrdinaryGraphSourceRanges SourceRanges {get;}
    public LegacyFirstBlankTransitionGeometry(ReadOnlySpan<byte> resource,
        LegacyFreshFirstResultOwnershipFactory factory,LegacySmoothedRowCutResult cuts,
        LegacyFreshFirstCandidateGraphPreparation prepared,LegacyCursiveSeparationTable table,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(factory);ArgumentNullException.ThrowIfNull(cuts);ArgumentNullException.ThrowIfNull(prepared);
        ArgumentNullException.ThrowIfNull(table);cancellationToken.ThrowIfCancellationRequested();
        SourceRanges=prepared.SourceRanges;graph=SourceRanges.Graph;
        if(!ReferenceEquals(SourceRanges.PreparedChronology,cuts.Chronology)||
            !ReferenceEquals(SourceRanges.OriginalSource,factory.Frame.Row.OriginalSource)||
            !ReferenceEquals(prepared.Pruning.Graph,graph)||!ReferenceEquals(cuts.Owner,factory.Frame.Row))
            throw new ArgumentException("Actual first pruned graph, chronology, raw source and fresh factory required.");
        _=LegacyFreshFirstLiteralContextPolicy.ReadSource(resource,factory.InvocationPolicy.Binding.SourceOffset,factory,cancellationToken);
        profile=LegacyFirstTransitionGeometryProfile.ReadSource(resource,factory.InvocationPolicy.Binding.SourceOffset,cancellationToken);
        replacement=profile.Alphabet.TryGetLabel("\ufffd",out ushort id)?id:ushort.MaxValue;
        int end=checked(profile.SamplerOffset+8+(int)BinaryPrimitives.ReadUInt32LittleEndian(resource[(profile.SamplerOffset+4)..]));
        _=LegacyCursiveCategoryProfile.Load(resource[(profile.SamplerOffset+65)..end],profile.SamplerOffset+65,out int categoryBytes,cancellationToken);
        int tail=checked(profile.SamplerOffset+65+categoryBytes);
        if(tail+214!=end||resource[tail+189]>1)throw new InvalidDataException("Complete ordinary sampler106 tail required.");
        Threshold70=BinaryPrimitives.ReadSingleLittleEndian(resource[(tail+33)..]);RequireWholePhysicalStarts10C=resource[tail+189]!=0;
        BlankCompositeCategory=profile.Categories.StartCompositeMap.Span[profile.Alphabet.LabelCount];
        if(BlankCompositeCategory<0)throw new NotSupportedException("Actual blank composite category required.");
        ordinary=new(profile,factory.Frame,cuts,graph,table,cancellationToken);height=factory.Frame.Row.Frame.Line.Height;
        costs=new int[graph.NodeCount][];
        for(int n=0;n<graph.NodeCount;n++)
        {
            cancellationToken.ThrowIfCancellationRequested();var arcs=graph.GetNode(n);costs[n]=new int[arcs.Count];
            for(int a=0;a<arcs.Count;a++)
            {
                if(n==arcs[a].Span){costs[n][a]=0x4000000;continue;}
                int pn=n-arcs[a].Span-1;float prior=-float.MaxValue;
                foreach(int p in arcs[a].PredecessorArcIndices.Span)
                    prior=Math.Max(prior,ordinary.GetPreparedState(pn,p).PrefixMaximumX);
                var box=ordinary.GetBounds(n,a);if(prior==-float.MaxValue)prior=box.MaximumX;
                costs[n][a]=LegacyCursiveBlankGapCost.Evaluate(box.MinimumX,box.MaximumX,prior,height,
                    Threshold70,profile.GapSlope,RequireWholePhysicalStarts10C,table.Values.Span);
            }
        }
    }
    public int ReadPreparedBlankCost(int currentNode,ushort currentArc)
    {_=ordinary.GetBounds(currentNode,currentArc);return costs[currentNode][currentArc];}
    public LegacyFirstOrdinaryTransitionResult Correct118(int currentNode,ushort currentArc,int incomingCost,
        CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();_=ordinary.GetBounds(currentNode,currentArc);
        var arc=graph.GetNode(currentNode)[currentArc];if(currentNode==arc.Span)throw new ArgumentException("Nonroot blank transition required.");
        //Owned graph has no protocol slots/sourceType2 and no suppression8.
        //1A82A0 returns0 for this same-descriptor ordinary branch.
        if(RequireWholePhysicalStarts10C&&(!ordinary.HasWholePhysicalStarts(currentNode,currentArc)||
            currentNode+1<graph.NodeCount&&!ordinary.HasWholePhysicalStarts(currentNode+1,0)))
            return new(false,0x4000000);
        int cost=checked(incomingCost+costs[currentNode][currentArc]);return new(cost<0x4000000,cost);
    }
    /// <summary>Pair geometry accepts the caller's already-resolved previous
    /// geometric ancestor, separately from its original primary parent. This
    /// method does not assert/produce that ancestor's backtracking ownership.</summary>
    public LegacyFirstOrdinaryTransitionResult Correct140(int currentNode,ushort currentArc,
        int previousGeometryNode,ushort previousGeometryArc,ushort previousSymbol,byte previousCompositeCategory,int incomingCost,
        CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();var current=ordinary.GetBounds(currentNode,currentArc);
        int parent=currentNode-graph.GetNode(currentNode)[currentArc].Span-1;
        if(previousGeometryNode<0||previousGeometryNode>parent||previousCompositeCategory>=profile.Categories.CompositeCategories.Length||
            previousSymbol>profile.Alphabet.LabelCount+2)
            throw new ArgumentException("Resolved earlier ordinary geometric ancestor required.");
        var previous=ordinary.GetBounds(previousGeometryNode,previousGeometryArc);
        //1ACFA0 graph1BE=FFFD makes20 true and bypasses195B30.
        if(previousSymbol==replacement)return new(incomingCost<0x4000000,incomingCost);
        int cg=profile.Categories.CompositeCategories.Span[BlankCompositeCategory].GeometryIndex;
        int pg=profile.Categories.CompositeCategories.Span[previousCompositeCategory].GeometryIndex;
        int delta=LegacyFirstOrdinaryTransitionGeometry.PairCost(current,previous,profile.Categories.Prototypes.Span[cg],
            profile.Categories.Prototypes.Span[pg],height,profile.PairScale,profile.PairOffset,profile.PairMaximum);
        int cost=checked(incomingCost+delta);return new(cost<0x4000000,cost);
    }
}
