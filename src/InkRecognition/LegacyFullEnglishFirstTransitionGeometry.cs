using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
namespace InkRecognition;

/// <summary>Actual same-source FIRST1941B0 initializer state only. No request,
/// primary row or previous/ending authority is manufactured by this receipt.</summary>
public sealed class LegacyFullEnglishFirstTransitionGeometry
{
    private readonly LegacyFullEnglishFirstControllerCosts owner;
    private readonly LegacyOrdinaryCursiveCandidateGraph graph;
    private readonly LegacyCursiveCategoryProfile categories;
    private readonly LegacyFirstTransitionBounds[][] bounds;
    private readonly float[][] future,prefix;
    private readonly int[][] join,blank;
    private readonly int[][][] matrix;
    private readonly float height,slope,translation,pairScale,pairOffset,pairMaximum;
    public int NodeCount=>bounds.Length;
    internal LegacyFullEnglishFirstTransitionGeometry(LegacyFullEnglishFirstControllerCosts owner,
        LegacyFullEnglishFirstPolicy policy,LegacyFullEnglishFirstCandidates candidates,
        float[] nativeTable,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(policy);ArgumentNullException.ThrowIfNull(nativeTable);token.ThrowIfCancellationRequested();
        this.owner=owner;graph=candidates.SourceRanges.Graph;categories=policy.Categories;
        height=candidates.Preparation.Line.Height;
        byte[] B(string name)=>candidates.Sampler.Fields.Single(f=>f.Destination==name).CopyBytes();
        float F(string name)=>BinaryPrimitives.ReadSingleLittleEndian(B(name));int I(string name)=>BinaryPrimitives.ReadInt32LittleEndian(B(name));
        if(B("50")[0]!=1||B("134.present")[0]!=0||B("1D0.present")[0]!=0||F("60")!=0||!float.IsFinite(height)||height<=0)
            throw new NotSupportedException("First ordinary108 same-descriptor source without custom boundary payload required.");
        slope=F("108");translation=F("74");pairScale=F("54");pairOffset=F("58");pairMaximum=F("5C");
        float gap=F("6C"),threshold=F("70");int joinCap=I("120"),matrixCap=I("124");bool useMaximum=B("10C")[0]!=0;
        if(slope<0||pairScale<0||pairMaximum<0||joinCap is <0 or >0x4000000||matrixCap is <0 or >0x4000000)
            throw new InvalidDataException("Checked ordinary geometry parameters required.");
        float[] table=nativeTable;
        if(!SHA256.HashData(MemoryMarshal.AsBytes(table.AsSpan())).AsSpan().SequenceEqual(Convert.FromHexString("B23ED52A3A10808EAB597A8F371A7BBB66BDE2FAB5FE2666431B1B192ABC1BAF")))
            throw new ArgumentException("Pinned native geometry table required.",nameof(nativeTable));
        var margins=new float[36];for(int row=0;row<6;row++)for(int col=0;col<6;col++)
            margins[row*6+col]=BinaryPrimitives.ReadSingleLittleEndian(B($"{0x78+row*24:X}.matrixRow").AsSpan(col*4));
        var strokes=candidates.CopyCutStrokes();var boxes=new Dictionary<int,LegacyFirstTransitionBounds>();
        foreach(int group in candidates.CopyLiveGroupIds())
        {
            token.ThrowIfCancellationRequested();var ranges=candidates.CopyGroupRanges(group);
            float minX=float.MaxValue,minY=float.MaxValue,maxX=-float.MaxValue,maxY=-float.MaxValue;
            foreach(var range in ranges)
            {
                for(int p=range.StartPoint;p<range.EndPointExclusive;p++)
                {token.ThrowIfCancellationRequested();var point=strokes[range.StrokeIndex].Points[p];minX=Math.Min(minX,point.X);minY=Math.Min(minY,point.Y);maxX=Math.Max(maxX,point.X);maxY=Math.Max(maxY,point.Y);}
            }
            boxes.Add(group,new(minX,minY,maxX,maxY));
        }
        //Only selector0 (offset1) is consumed by this initializer. Read the
        //already-owned ordered links directly; no15-selector connection clone.
        int count=graph.NodeCount;
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
            if(n!=graph.GetNode(n)[a].Span&&n+1<count)foreach(var next in graph.GetNode(n)[a].Successors)
            {token.ThrowIfCancellationRequested();if(next.NodeOffset==1)low=Math.Min(low,future[n+1][next.ArcIndex]);}
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

    }
    public LegacyFullEnglishFirstGeometryState[] CopyNode(int node)
    {
        if((uint)node>=(uint)bounds.Length)throw new ArgumentOutOfRangeException(nameof(node));
        return Enumerable.Range(0,bounds[node].Length).Select(a=>new LegacyFullEnglishFirstGeometryState(
            bounds[node][a],future[node][a],prefix[node][a],join[node][a],blank[node][a],matrix[node][a])).ToArray();
    }
}

public sealed class LegacyFullEnglishFirstGeometryState
{
    public LegacyFirstTransitionBounds Bounds {get;}
    public float FutureMinimumX {get;}
    public float PrefixMaximumX {get;}
    public int JoinCost {get;}
    public int BlankCost {get;}
    private readonly int[] margins;
    public int[] CopyMarginCosts()=>margins.ToArray();
    internal LegacyFullEnglishFirstGeometryState(LegacyFirstTransitionBounds bounds,float future,float prefix,int join,int blank,int[] margins)
    {Bounds=bounds;FutureMinimumX=future;PrefixMaximumX=prefix;JoinCost=join;BlankCost=blank;this.margins=margins.ToArray();}
}
