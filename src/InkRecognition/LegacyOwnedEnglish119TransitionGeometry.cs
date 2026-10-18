using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Same owned English108 first1941B0 and118/140 ordinary0/2/5/6
/// corrections. Source9C=-1, first210=1 and absent134/1D0 are scoped native
/// construction facts, not caller-selected flags.</summary>
public sealed class LegacyOwnedEnglish119TransitionGeometry
{
    public LegacyOwnedEnglish119FirstPolicy Policy {get;}
    private readonly LegacyOrdinaryCursiveCandidateGraph graph;
    private readonly LegacyCursiveCategoryProfile categories;
    private readonly LegacyFirstTransitionBounds[][] bounds;
    private readonly float[][] future,prefix;
    private readonly int[][] join,blank;
    private readonly int[][][] matrix;
    private readonly bool[] wholeStarts;
    private readonly float height,slope,translation,pairScale,pairOffset,pairMaximum;
    internal LegacyOwnedEnglish119TransitionGeometry(LegacyOwnedEnglish119FirstPolicy policy,
        LegacyCursiveSeparationTable nativeTable,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(policy);ArgumentNullException.ThrowIfNull(nativeTable);token.ThrowIfCancellationRequested();
        Policy=policy;graph=policy.SourceRanges.Graph;categories=policy.Categories;
        var candidates=policy.Owner.Owner.Owner.Owner;height=candidates.Preparation.Line.Height;
        byte[] B(string name)=>candidates.Sampler.Fields.Single(f=>f.Destination==name).CopyBytes();
        float F(string name)=>BinaryPrimitives.ReadSingleLittleEndian(B(name));int I(string name)=>BinaryPrimitives.ReadInt32LittleEndian(B(name));
        if(B("50")[0]!=1||B("134.present")[0]!=0||B("1D0.present")[0]!=0||F("60")!=0||!float.IsFinite(height)||height<=0)
            throw new NotSupportedException("First ordinary108 same-descriptor source without custom boundary payload required.");
        slope=F("108");translation=F("74");pairScale=F("54");pairOffset=F("58");pairMaximum=F("5C");
        float gap=F("6C"),threshold=F("70");int joinCap=I("120"),matrixCap=I("124");bool useMaximum=B("10C")[0]!=0;
        if(slope<0||pairScale<0||pairMaximum<0||joinCap is <0 or >0x4000000||matrixCap is <0 or >0x4000000)
            throw new InvalidDataException("Checked ordinary geometry parameters required.");
        float[] table=nativeTable.Values.ToArray();
        if(!SHA256.HashData(MemoryMarshal.AsBytes(table.AsSpan())).AsSpan().SequenceEqual(Convert.FromHexString("B23ED52A3A10808EAB597A8F371A7BBB66BDE2FAB5FE2666431B1B192ABC1BAF")))
            throw new ArgumentException("Pinned native geometry table required.",nameof(nativeTable));
        var margins=new float[36];for(int row=0;row<6;row++)for(int col=0;col<6;col++)
            margins[row*6+col]=BinaryPrimitives.ReadSingleLittleEndian(B($"{0x78+row*24:X}.matrixRow").AsSpan(col*4));
        var strokes=candidates.CopyCutStrokes();var boxes=new LegacyFirstTransitionBounds[graph.Groups.Count];wholeStarts=new bool[boxes.Length];
        foreach(int group in candidates.CopyLiveGroupIds())
        {
            token.ThrowIfCancellationRequested();var ranges=candidates.CopyGroupRanges(group);
            float minX=float.MaxValue,minY=float.MaxValue,maxX=-float.MaxValue,maxY=-float.MaxValue;bool whole=true;
            foreach(var range in ranges)
            {
                whole&=range.StartPoint==0;
                for(int p=range.StartPoint;p<range.EndPointExclusive;p++)
                {token.ThrowIfCancellationRequested();var point=strokes[range.StrokeIndex].Points[p];minX=Math.Min(minX,point.X);minY=Math.Min(minY,point.Y);maxX=Math.Max(maxX,point.X);maxY=Math.Max(maxY,point.Y);}
            }
            boxes[group]=new(minX,minY,maxX,maxY);wholeStarts[group]=whole;
        }
        var connections=new LegacyOrdinaryCandidateConnections(graph,token);int count=graph.NodeCount;
        bounds=new LegacyFirstTransitionBounds[count][];future=new float[count][];prefix=new float[count][];join=new int[count][];blank=new int[count][];matrix=new int[count][][];
        for(int n=0;n<count;n++)
        {
            token.ThrowIfCancellationRequested();var arcs=graph.GetNode(n);bounds[n]=new LegacyFirstTransitionBounds[arcs.Count];future[n]=new float[arcs.Count];prefix[n]=new float[arcs.Count];join[n]=new int[arcs.Count];blank[n]=new int[arcs.Count];matrix[n]=new int[arcs.Count][];
            for(int a=0;a<arcs.Count;a++)
            {
                if(arcs[a].Span>7||(arcs[a].Flags&8)!=0)throw new NotSupportedException("Nonsuppressed ordinary first arcs required.");
                bounds[n][a]=boxes[arcs[a].GroupId];matrix[n][a]=new int[36];
            }
        }
        for(int n=count-1;n>=0;n--)for(ushort a=0;a<bounds[n].Length;a++)
        {
            token.ThrowIfCancellationRequested();float low=bounds[n][a].MinimumX;
            if(n!=graph.GetNode(n)[a].Span&&n+1<count)foreach(int next in connections.GetSuccessorOrdinals(n,a,0,token).Span)low=Math.Min(low,future[n+1][next]);
            future[n][a]=low;
        }
        for(int n=0;n<count;n++)for(int a=0;a<bounds[n].Length;a++)
        {
            token.ThrowIfCancellationRequested();var arc=graph.GetNode(n)[a];var box=bounds[n][a];int previous=n-arc.Span-1;
            if(previous<0){prefix[n][a]=box.MaximumX;join[n][a]=blank[n][a]=0x4000000;Array.Fill(matrix[n][a],0x4000000);continue;}
            float prior=-float.MaxValue;foreach(int p in arc.PredecessorArcIndices.Span)prior=Math.Max(prior,prefix[previous][p]);
            if(prior==-float.MaxValue)prior=box.MaximumX;prefix[n][a]=Math.Max(box.MaximumX,prior);
            float normalized=(future[n][a]-prior)/height;
            join[n][a]=Math.Min(LegacyFirstOrdinaryTransitionGeometry.LogCost((normalized-gap)*-slope,table,true),joinCap);
            blank[n][a]=LegacyCursiveBlankGapCost.Evaluate(box.MinimumX,box.MaximumX,prior,height,threshold,slope,useMaximum,table);
            for(int i=0;i<36;i++)
            {
                int value=LegacyFirstOrdinaryTransitionGeometry.LogCost((normalized-margins[i])*slope,table,false);
                matrix[n][a][i]=i/6==5?checked(value-1):i%6==5?value:Math.Min(value,matrixCap);
            }
        }
        UseMaximumX=useMaximum;
    }
    private bool UseMaximumX {get;}
    internal LegacyFirstTransitionBounds ReadBounds(int node,int arc)
    {if((uint)node>=(uint)bounds.Length||(uint)arc>=(uint)bounds[node].Length)throw new ArgumentOutOfRangeException(nameof(node));return bounds[node][arc];}
    internal (float Future,float Prefix,int Join,int Blank,int[] Matrix) CopyState(int node,int arc)
    {_=ReadBounds(node,arc);return(future[node][arc],prefix[node][arc],join[node][arc],blank[node][arc],matrix[node][arc].ToArray());}
    internal LegacyFirstOrdinaryTransitionResult Correct118(LegacyFirstAncestorTransitionRequest r,int incoming,CancellationToken token)
    {
        Check(r,token);if(r.Kind==6)return new(true,incoming);var previous=r.Previous.GeometryRecord;
        if(r.Kind==5)
        {
            if(UseMaximumX&&(!Whole(r.CurrentNode,r.CurrentArc)||r.CurrentNode+1<graph.NodeCount&&!Whole(r.CurrentNode+1,0)))return new(false,0x4000000);
            int cost=checked(incoming+blank[r.CurrentNode][r.CurrentArc]);return new(cost<0x4000000,cost);
        }
        bool xor=(r.CurrentSymbol==Policy.Replacement)!=(previous.Symbol==Policy.Replacement);
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
        if(r.CurrentSymbol==Policy.Replacement||previous.Symbol==Policy.Replacement)return new(incoming<0x4000000,incoming);
        int c=categories.CompositeCategories.Span[r.CurrentCategory].GeometryIndex,p=categories.CompositeCategories.Span[previous.CompositeCategory].GeometryIndex;
        int delta=LegacyFirstOrdinaryTransitionGeometry.PairCost(ReadBounds(r.CurrentNode,r.CurrentArc),ReadBounds(r.Previous.GeometryNode,previous.ArcOrdinal),
            categories.Prototypes.Span[c],categories.Prototypes.Span[p],height,pairScale,pairOffset,pairMaximum);
        int result=checked(incoming+delta);return new(result<0x4000000,result);
    }
    private bool Whole(int node,int arc)=>wholeStarts[graph.GetNode(node)[arc].GroupId];
    private void Check(LegacyFirstAncestorTransitionRequest r,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var p=r.Previous;
        if(r.Kind is not(0 or 2 or 5 or 6)||p.OriginalNode!=r.CurrentNode-r.CurrentSpan-1||p.GeometryNode<0||p.GeometryNode>p.OriginalNode||
            r.CurrentSymbol>Policy.Blank+2||p.GeometryRecord.Symbol>Policy.Blank+2||r.CurrentCategory>=categories.CompositeCategories.Length||p.GeometryRecord.CompositeCategory>=categories.CompositeCategories.Length)
            throw new ArgumentException("Same owned ordinary original/ancestor request required.");
        _=ReadBounds(r.CurrentNode,r.CurrentArc);_=ReadBounds(p.OriginalNode,p.OriginalRecord.ArcOrdinal);_=ReadBounds(p.GeometryNode,p.GeometryRecord.ArcOrdinal);
        var arc=graph.GetNode(r.CurrentNode)[r.CurrentArc];
        if(arc.Span!=r.CurrentSpan||!arc.PredecessorArcIndices.Span.Contains((int)p.OriginalRecord.ArcOrdinal))throw new ArgumentException("Original acoustic parent does not match actual graph compatibility.");
    }
}
