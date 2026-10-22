namespace InkRecognition;

//Actual complete B2 immutable initializer snapshot; no repeated callback copies.
internal sealed class LegacyFullEnglishFirstPrimaryGeometry
{
    private readonly LegacyFullEnglishFirstPrimaryPolicy policy;
    private readonly LegacyOrdinaryCursiveCandidateGraph graph;
    private readonly LegacyCursiveCategoryProfile categories;
    private readonly LegacyFirstTransitionBounds[][] bounds;
    private readonly int[][] join,blank;
    private readonly int[][][] matrix;
    private readonly Dictionary<int,bool> wholeStarts=new();
    private readonly float height,slope,translation,pairScale,pairOffset,pairMaximum;
    private bool UseMaximumX=>true;
    internal LegacyFullEnglishFirstPrimaryGeometry(LegacyFullEnglishFirstPrimaryPolicy policy,LegacyFullEnglishFirstControllerChild child,CancellationToken token)
    {
        this.policy=policy;var c=child.ActualCache.Owner;graph=c.SourceRanges.Graph;categories=policy.Categories;
        height=c.Preparation.Line.Height;slope=policy.Profile.F("108");translation=policy.Profile.F("74");pairScale=policy.Profile.F("54");pairOffset=policy.Profile.F("58");pairMaximum=policy.Profile.F("5C");
        var groups=graph.Groups;foreach(int id in c.CopyLiveGroupIds())
        {token.ThrowIfCancellationRequested();bool whole=true;foreach(var range in groups[id].Ranges.Span){token.ThrowIfCancellationRequested();whole&=range.StartPoint==0;}wholeStarts.Add(id,whole);}
        bounds=new LegacyFirstTransitionBounds[graph.NodeCount][];join=new int[graph.NodeCount][];blank=new int[graph.NodeCount][];matrix=new int[graph.NodeCount][][];
        for(int n=0;n<graph.NodeCount;n++)
        {
            token.ThrowIfCancellationRequested();var states=child.Geometry.CopyNode(n);int count=graph.GetNode(n).Count;
            if(states.Length!=count)throw new ArgumentException("Same B2 geometry graph required.");
            bounds[n]=new LegacyFirstTransitionBounds[count];join[n]=new int[count];blank[n]=new int[count];matrix[n]=new int[count][];
            for(int a=0;a<count;a++)
            {token.ThrowIfCancellationRequested();var state=states[a];bounds[n][a]=state.Bounds;join[n][a]=state.JoinCost;blank[n][a]=state.BlankCost;matrix[n][a]=state.CopyMarginCosts();if(matrix[n][a].Length!=36)throw new InvalidDataException("Actual6x6 B2 matrix required.");}
        }
    }
    private LegacyFirstTransitionBounds ReadBounds(int n,int a)
    {if((uint)n>=(uint)bounds.Length||(uint)a>=(uint)bounds[n].Length)throw new ArgumentOutOfRangeException(nameof(n));return bounds[n][a];}
    internal LegacyFirstOrdinaryTransitionResult Correct118(LegacyFirstAncestorTransitionRequest r,int incoming,CancellationToken token)
    {
        Check(r,token);if(r.Kind==6)return new(true,incoming);var previous=r.Previous.GeometryRecord;
        if(r.Kind==5)
        {
            if(UseMaximumX&&(!Whole(r.CurrentNode,r.CurrentArc)||r.CurrentNode+1<graph.NodeCount&&!Whole(r.CurrentNode+1,0)))return new(false,0x4000000);
            int cost=checked(incoming+blank[r.CurrentNode][r.CurrentArc]);return new(cost<0x4000000,cost);
        }
        bool xor=(r.CurrentSymbol==policy.Replacement)!=(previous.Symbol==policy.Replacement);
        if(r.Kind==0&&xor)return new(false,0x4000000);
        if(r.Kind==2){if(!Whole(r.CurrentNode,r.CurrentArc))return new(false,0x4000000);if(xor)return new(true,incoming);}
        int delta=join[r.CurrentNode][r.CurrentArc];
        if(r.Kind==2)
        {
            int c=categories.CompositeCategories.Span[r.CurrentCategory].FirstId,p=categories.CompositeCategories.Span[previous.CompositeCategory].FirstId;
            if(c is <0 or >5||p is <0 or >5)throw new NotSupportedException("Kind2 requires resolved first-ID values0..5; blank belongs to kind5.");
            delta=matrix[r.CurrentNode][r.CurrentArc][c+p*6];
        }
        var a=ReadBounds(r.CurrentNode,r.CurrentArc);var b=ReadBounds(r.Previous.GeometryNode,previous.ArcOrdinal);
        float displacement=Math.Max(a.MinimumX-b.MinimumX,a.MaximumX-b.MaximumX)/height;
        float value=displacement<=translation?(displacement-translation)*slope:0;
        if(!float.IsFinite(value)||value>=2147483648f||value<int.MinValue)throw new NotSupportedException("Checked transition translation domain exceeded.");
        int result=checked(incoming+delta-checked((int)value*8192));return new(result<0x4000000,result);
    }
    internal LegacyFirstOrdinaryTransitionResult Correct140(LegacyFirstAncestorTransitionRequest r,int incoming,CancellationToken token)
    {
        Check(r,token);if(r.Kind==6)return new(true,incoming);var previous=r.Previous.GeometryRecord;
        if(r.CurrentSymbol==policy.Replacement||previous.Symbol==policy.Replacement)return new(incoming<0x4000000,incoming);
        int c=categories.CompositeCategories.Span[r.CurrentCategory].GeometryIndex,p=categories.CompositeCategories.Span[previous.CompositeCategory].GeometryIndex;
        var sourceBounds=ReadBounds(r.Previous.GeometryNode,previous.ArcOrdinal);
        if(r.Previous.GeometryNode!=r.Previous.OriginalNode||previous.ArcOrdinal!=r.Previous.OriginalRecord.ArcOrdinal)
        {
            var endingBounds=ReadBounds(r.Previous.OriginalNode,r.Previous.OriginalRecord.ArcOrdinal);
            sourceBounds=new(Math.Min(sourceBounds.MinimumX,endingBounds.MinimumX),Math.Min(sourceBounds.MinimumY,endingBounds.MinimumY),
                Math.Max(sourceBounds.MaximumX,endingBounds.MaximumX),Math.Max(sourceBounds.MaximumY,endingBounds.MaximumY));
        }
        int delta=LegacyFirstOrdinaryTransitionGeometry.PairCost(ReadBounds(r.CurrentNode,r.CurrentArc),sourceBounds,
            categories.Prototypes.Span[c],categories.Prototypes.Span[p],height,pairScale,pairOffset,pairMaximum);
        int result=checked(incoming+delta);return new(result<0x4000000,result);
    }
    private bool Whole(int node,int arc)=>wholeStarts[graph.GetNode(node)[arc].GroupId];
    private void Check(LegacyFirstAncestorTransitionRequest r,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var p=r.Previous;
        if(r.Kind is not(0 or 2 or 5 or 6)||p.OriginalNode!=r.CurrentNode-r.CurrentSpan-1||p.GeometryNode<0||p.GeometryNode>p.OriginalNode||
            r.CurrentSymbol>policy.Blank+2||p.GeometryRecord.Symbol>policy.Blank+2||r.CurrentCategory>=categories.CompositeCategories.Length||p.GeometryRecord.CompositeCategory>=categories.CompositeCategories.Length)
            throw new ArgumentException("Same owned ordinary original/ancestor request required.");
        _=ReadBounds(r.CurrentNode,r.CurrentArc);_=ReadBounds(p.OriginalNode,p.OriginalRecord.ArcOrdinal);_=ReadBounds(p.GeometryNode,p.GeometryRecord.ArcOrdinal);
        var arc=graph.GetNode(r.CurrentNode)[r.CurrentArc];
        if(arc.Span!=r.CurrentSpan||!arc.PredecessorArcIndices.Span.Contains((int)p.OriginalRecord.ArcOrdinal))throw new ArgumentException("Original acoustic parent does not match actual graph compatibility.");
    }
}
