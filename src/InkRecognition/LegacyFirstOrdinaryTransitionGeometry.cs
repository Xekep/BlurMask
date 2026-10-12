namespace InkRecognition;

public readonly record struct LegacyFirstTransitionBounds(float MinimumX,float MinimumY,float MaximumX,float MaximumY);

/// <summary>Actual fresh one-descriptor line0 ordinary118/140 callbacks.
/// No Arabic/primitive/opaque/cross-descriptor/special-boundary transitions are
/// inferred. Native1941b0 prefix/suffix caches precede195c40/195b30 updates.</summary>
public sealed class LegacyFirstOrdinaryTransitionGeometry
{
    private readonly LegacyFirstTransitionGeometryProfile profile;
    private readonly LegacyOrdinaryCursiveCandidateGraph graph;
    private readonly LegacyFirstTransitionBounds[][] bounds;
    private readonly float[][] futureMinimum,prefixMaximum;
    private readonly int[][] joinCosts;
    private readonly int[][][] matrixCosts;
    private readonly bool[] wholeStarts;
    private readonly float height;
    public int NodeCount=>bounds.Length;

    public LegacyFirstOrdinaryTransitionGeometry(LegacyFirstTransitionGeometryProfile profile,
        LegacyFreshOrdinaryPacketFrame frame,LegacySmoothedRowCutResult cuts,LegacyOrdinaryCursiveCandidateGraph graph,
        LegacyCursiveSeparationTable table,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(profile);ArgumentNullException.ThrowIfNull(frame);ArgumentNullException.ThrowIfNull(cuts);
        ArgumentNullException.ThrowIfNull(graph);ArgumentNullException.ThrowIfNull(table);cancellationToken.ThrowIfCancellationRequested();
        if(!ReferenceEquals(cuts.Owner,frame.Row)||frame.Row.Profile!=profile.RowProfile||frame.Descriptor.CurrentLineId!=0||
            frame.Descriptor.ScopeId!=-1||frame.Descriptor.Flags!=0x10||frame.Packet.DescriptorCount!=1||
            cuts.Cuts.PreparedInk.StrokePointCounts.Length!=frame.Packet.PhysicalStrokeCount||graph.NodeCount>4096||graph.CandidateCount>131072)
            throw new ArgumentException("Owned fresh unscoped raw packet/frame/cuts and matching first resource required.");
        this.profile=profile;this.graph=graph;height=frame.Row.Frame.Line.Height;
        if(!float.IsFinite(height)||height<=0)throw new ArgumentException("Positive native source line height required.");
        var connections=new LegacyOrdinaryCandidateConnections(graph,cancellationToken);
        bounds=new LegacyFirstTransitionBounds[graph.NodeCount][];futureMinimum=new float[graph.NodeCount][];
        prefixMaximum=new float[graph.NodeCount][];joinCosts=new int[graph.NodeCount][];matrixCosts=new int[graph.NodeCount][][];
        var geometry=graph.Groups.Select(g=>Box(cuts.Cuts.PreparedInk,g.Ranges.Span,cancellationToken)).ToArray();
        wholeStarts=graph.Groups.Select(g=>g.Ranges.ToArray().All(r=>r.StartPoint==0)).ToArray();
        foreach(var group in graph.Groups)if(frame.Packet.GetDescriptorOrdinalForGroup(group.Ranges.Span,cancellationToken)!=0)
            throw new NotSupportedException("Cross-descriptor groups are outside ordinary same-packet callbacks.");
        for(int node=0;node<graph.NodeCount;node++)
        {
            cancellationToken.ThrowIfCancellationRequested();var arcs=graph.GetNode(node);
            bounds[node]=new LegacyFirstTransitionBounds[arcs.Count];futureMinimum[node]=new float[arcs.Count];
            prefixMaximum[node]=new float[arcs.Count];joinCosts[node]=new int[arcs.Count];matrixCosts[node]=new int[arcs.Count][];
            for(int i=0;i<arcs.Count;i++)
            {
                if(arcs[i].Span>7||(arcs[i].Flags&8)!=0)throw new NotSupportedException("Only nonsuppressed bounded ordinary arcs supported.");
                bounds[node][i]=geometry[arcs[i].GroupId];matrixCosts[node][i]=new int[36];
            }
        }
        //1941b0 reverse traversal, future minX through immediate successors.
        for(int node=graph.NodeCount-1;node>=0;node--)
            for(ushort ordinal=0;ordinal<bounds[node].Length;ordinal++)
            {
                cancellationToken.ThrowIfCancellationRequested();var arc=graph.GetNode(node)[ordinal];float low=bounds[node][ordinal].MinimumX;
                if(node!=arc.Span&&node+1<graph.NodeCount)
                    foreach(int next in connections.GetSuccessorOrdinals(node,ordinal,0,cancellationToken).Span)
                        low=Math.Min(low,futureMinimum[node+1][next]);
                futureMinimum[node][ordinal]=low;
            }
        //Fresh secondary9c=-1 (16a130), sampler210=1 (first23f flag),
        //sampler1d0=NULL(v106) and242=0: no custom boundary network/probability.
        for(int node=0;node<graph.NodeCount;node++)
            for(int ordinal=0;ordinal<bounds[node].Length;ordinal++)
            {
                cancellationToken.ThrowIfCancellationRequested();var arc=graph.GetNode(node)[ordinal];var box=bounds[node][ordinal];
                if(node==arc.Span)
                {
                    prefixMaximum[node][ordinal]=box.MaximumX;joinCosts[node][ordinal]=0x4000000;
                    Array.Fill(matrixCosts[node][ordinal],0x4000000);continue;
                }
                int previous=node-arc.Span-1;float prior=-float.MaxValue;
                foreach(int p in arc.PredecessorArcIndices.Span)
                {
                    cancellationToken.ThrowIfCancellationRequested();if(p<0||p>=bounds[previous].Length)throw new InvalidDataException("Invalid native preceding arc.");
                    prior=Math.Max(prior,prefixMaximum[previous][p]);
                }
                if(prior==-float.MaxValue)prior=box.MaximumX;
                prefixMaximum[node][ordinal]=Math.Max(box.MaximumX,prior);
                float gap=(futureMinimum[node][ordinal]-prior)/height;
                joinCosts[node][ordinal]=Math.Min(LogCost((gap-profile.DefaultGap)*-profile.GapSlope,table.Values.Span,true),profile.JoinMaximumCost);
                for(int i=0;i<36;i++)
                {
                    int cost=LogCost((gap-profile.GapMatrix.Span[i])*profile.GapSlope,table.Values.Span,false);
                    matrixCosts[node][ordinal][i]=i/6==5?checked(cost-1):i%6==5?cost:Math.Min(cost,profile.MatrixMaximumCost);
                }
            }
    }

    public LegacyFirstOrdinaryTransitionResult Correct118(LegacyFirstOrdinaryTransitionRequest request,int incomingCost,
        CancellationToken cancellationToken=default)
    {
        Check(request,cancellationToken);int cost;
        if(request.Kind==0)cost=joinCosts[request.CurrentNode][request.CurrentArc];
        else
        {
            //Frontend1525e0 rejects kind2 when graph188/1a7d10 finds any
            //nonzero physical range start. Its primitive predecessor exception
            //does not apply to this API's checked plain-symbol groups.
            if(!wholeStarts[graph.GetNode(request.CurrentNode)[request.CurrentArc].GroupId])return new(false,0x4000000);
            int current=profile.Categories.CompositeCategories.Span[request.CurrentCategory].FirstId;
            int previous=profile.Categories.CompositeCategories.Span[request.PreviousCategory].FirstId;
            if(current is <0 or >5||previous is <0 or >5)throw new NotSupportedException("Ordinary six-column category matrix required.");
            cost=matrixCosts[request.CurrentNode][request.CurrentArc][current+previous*6];
        }
        var a=bounds[request.CurrentNode][request.CurrentArc];var b=bounds[request.PreviousNode][request.PreviousArc];
        float delta=Math.Max(a.MinimumX-b.MinimumX,a.MaximumX-b.MaximumX)/height;
        float value=0;
        if(profile.GapSlope>=0?delta<=profile.TranslationThreshold:delta>=profile.TranslationThreshold)
            value=(delta-profile.TranslationThreshold)*profile.GapSlope;
        if(!float.IsFinite(value)||value>=2147483648f||value<int.MinValue)throw new NotSupportedException("Transition translation exceeds checked integer domain.");
        int result=checked(incomingCost+cost-checked((int)value*8192));
        return new(result<0x4000000,result);
    }

    public LegacyFirstOrdinaryTransitionResult Correct140(LegacyFirstOrdinaryTransitionRequest request,int incomingCost,
        CancellationToken cancellationToken=default)
    {
        Check(request,cancellationToken);
        int current=profile.Categories.CompositeCategories.Span[request.CurrentCategory].GeometryIndex;
        int previous=profile.Categories.CompositeCategories.Span[request.PreviousCategory].GeometryIndex;
        int delta=PairCost(bounds[request.CurrentNode][request.CurrentArc],bounds[request.PreviousNode][request.PreviousArc],
            profile.Categories.Prototypes.Span[current],profile.Categories.Prototypes.Span[previous],height,
            profile.PairScale,profile.PairOffset,profile.PairMaximum);
        int result=checked(incomingCost+delta);return new(result<0x4000000,result);
    }

    public LegacyFirstTransitionBounds GetBounds(int node,int arcOrdinal)
    {if((uint)node>=(uint)bounds.Length||(uint)arcOrdinal>=(uint)bounds[node].Length)throw new ArgumentOutOfRangeException(nameof(node));return bounds[node][arcOrdinal];}
    public (float FutureMinimumX,float PrefixMaximumX,int JoinCost) GetPreparedState(int node,int arcOrdinal)
    {_=GetBounds(node,arcOrdinal);return(futureMinimum[node][arcOrdinal],prefixMaximum[node][arcOrdinal],joinCosts[node][arcOrdinal]);}
    public bool HasWholePhysicalStarts(int node,int arcOrdinal)
    {_=GetBounds(node,arcOrdinal);return wholeStarts[graph.GetNode(node)[arcOrdinal].GroupId];}

    /// <summary>195b30 ordinary same-line category prototype residual. Signed
    /// nearest rounding allows the native negative pair offset.</summary>
    public static int PairCost(LegacyFirstTransitionBounds current,LegacyFirstTransitionBounds previous,
        LegacyCursiveCategoryPrototype currentPrototype,LegacyCursiveCategoryPrototype previousPrototype,
        float lineHeight,float scale,float offset,float maximum)
    {
        if(!float.IsFinite(lineHeight)||lineHeight<=0||!float.IsFinite(scale)||scale<0||!float.IsFinite(offset)||
            !float.IsFinite(maximum)||maximum<0)throw new ArgumentException("Finite same-line geometry profile required.");
        float bottom=currentPrototype.First-previousPrototype.First;
        float top=bottom-(currentPrototype.Second-previousPrototype.Second);float inverse=1f/lineHeight;
        float residual=MathF.Abs((current.MinimumY-previous.MinimumY)*inverse-top)+MathF.Abs((current.MaximumY-previous.MaximumY)*inverse-bottom);
        float value=Math.Min(residual*scale-offset,maximum)*8192f;
        return Truncate(value<=0?value-.5f:value+.5f);
    }
    /// <summary>1941b0 lookup uses float(x+4), then double*255+.5, divide8.
    /// Join uses24d8a0 bit rounding; category matrix uses signed+.5 truncation.</summary>
    public static int LogCost(float transformed,ReadOnlySpan<float> logTable,bool bitRounded)
    {
        if(!float.IsFinite(transformed)||logTable.Length!=256)throw new ArgumentException("Finite transformed gap and256 entries required.");
        float value=transformed;
        if(transformed>4)value=0;
        else if(transformed>=-4)value=logTable[((int)((double)(transformed+4f)*255d+.5d))>>3];
        if(!float.IsFinite(value)||value>0)throw new ArgumentException("Finite nonpositive log probability required.");
        float cost=-value*8192f;
        if(bitRounded)return Truncate(RoundBits(cost));
        return Truncate(cost<=0?cost-.5f:cost+.5f);
    }
    private void Check(LegacyFirstOrdinaryTransitionRequest r,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(r.Kind is not (0 or 2)||(uint)r.CurrentNode>=(uint)bounds.Length||r.PreviousNode<0||
            r.PreviousNode!=r.CurrentNode-r.CurrentSpan-1||(uint)r.CurrentArc>=(uint)bounds[r.CurrentNode].Length||
            (uint)r.PreviousArc>=(uint)bounds[r.PreviousNode].Length||graph.GetNode(r.CurrentNode)[r.CurrentArc].Span!=r.CurrentSpan||
            !graph.GetNode(r.CurrentNode)[r.CurrentArc].PredecessorArcIndices.Span.Contains((int)r.PreviousArc)||
            r.CurrentCategory>=profile.Categories.CompositeCategories.Length||r.PreviousCategory>=profile.Categories.CompositeCategories.Length)
            throw new ArgumentException("Compatible bounded ordinary transition required.");
        foreach(ushort symbol in new[]{r.CurrentSymbol,r.PreviousSymbol})
            if(!profile.Alphabet.TryGetText(symbol,out string text)||text.Length!=1||
                !(text[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '\u0400' and <= '\u04ff' or >= '0' and <= '9'))
                throw new NotSupportedException("Only proved plain Latin/Cyrillic/digit same-packet transitions supported.");
    }
    private static int Truncate(float value)
    {if(!float.IsFinite(value)||value>=2147483648f||value<int.MinValue)throw new NotSupportedException("Geometry arithmetic exceeds checked signed integer domain.");return(int)value;}
    private static float RoundBits(float value)
    {
        uint bits=BitConverter.SingleToUInt32Bits(value),magnitude=bits&0x7fffffffu;
        if(magnitude>=0x4b000000u)return value;
        if(magnitude<0x3f800000u)bits=(bits&0x80000000u)|(magnitude>=0x3f000000u?0x3f800000u:0u);
        else{int shift=150-(int)(magnitude>>23);bits=((bits>>shift)+((bits>>(shift-1))&1u))<<shift;}
        return BitConverter.UInt32BitsToSingle(bits);
    }
    private static LegacyFirstTransitionBounds Box(LegacyCursivePreparedInk ink,ReadOnlySpan<LegacyInkRange> ranges,CancellationToken token)
    {
        float minX=float.MaxValue,minY=float.MaxValue,maxX=-float.MaxValue,maxY=-float.MaxValue;
        var counts=ink.StrokePointCounts.Span;
        foreach(var range in ranges)
        {
            token.ThrowIfCancellationRequested();
            if((uint)range.StrokeIndex>=(uint)counts.Length||range.StartPoint<0||range.EndPointExclusive<=range.StartPoint||range.EndPointExclusive>counts[range.StrokeIndex])throw new ArgumentException("Owned prepared ink range required.");
            int start=0;for(int s=0;s<range.StrokeIndex;s++)start+=counts[s];
            foreach(var p in ink.Points.Span.Slice(start+range.StartPoint,range.EndPointExclusive-range.StartPoint))
            {minX=Math.Min(minX,p.X);minY=Math.Min(minY,p.Y);maxX=Math.Max(maxX,p.X);maxY=Math.Max(maxY,p.Y);}
        }
        return new(minX,minY,maxX,maxY);
    }
}
