namespace InkRecognition;

/// <summary>One bounded private snapshot of an actual completed MAIN local graph;
///not a copied parent per glyph and not a native mutable pooled-cache session.</summary>
internal sealed class LegacyEnglishMainInkSnapshot
{
    internal LegacyEnglishMainAcousticInputs Inputs{get;}
    internal LegacyEnglish119MainLocalCandidates Owner=>Inputs.Knowledge.Owner;
    internal InkStroke[] Strokes{get;}
    internal int[] Counts{get;}
    internal float[] Probabilities{get;}
    internal LegacyCursivePreparedInk Prepared{get;}
    internal LegacyInkRange[][] Ranges{get;}
    internal int[] Spans{get;}
    internal (int Group,int Span)[][] Arcs{get;}
    internal long LogicalReservationBytes{get;}
    internal LegacyEnglishMainInkSnapshot(LegacyEnglishMainOnlySourceMasks masks,CancellationToken token)
        :this(LegacyEnglishMainAcousticInputs.FromIndependent(masks),token){}
    internal LegacyEnglishMainInkSnapshot(LegacyEnglishMainAcousticInputs inputs,CancellationToken token)
    {
        inputs.RequireCurrent();Inputs=inputs;token.ThrowIfCancellationRequested();var owner=Owner;
        if(!owner.IsComplete||owner.IsFaulted)throw new InvalidOperationException("Complete actual MAIN local graph required.");
        LogicalReservationBytes=Preflight(owner.GroupCount,owner.NodeCount,owner.CandidateCount);
        //The typed completed local owner has already bounded TOTAL graph ranges
        //and adjacency131072. Reserve that certified ceiling before any getter
        //copy; then retain only live group's <=15 ranges, never all node copies.
        Spans=Enumerable.Repeat(-1,owner.GroupCount).ToArray();Arcs=new (int Group,int Span)[owner.NodeCount][];int arcCount=0;
        for(int node=0;node<owner.NodeCount;node++)
        {
            token.ThrowIfCancellationRequested();var actualArcs=owner.CopyNode(node);if(actualArcs.Length>131072-arcCount)throw new ArgumentException("Whole actual arc budget exceeded.");arcCount+=actualArcs.Length;Arcs[node]=actualArcs.Select(a=>(a.GroupId,(int)a.Span)).ToArray();foreach(var arc in actualArcs)
            {
                if((uint)arc.GroupId>=(uint)Spans.Length||arc.Span>=8)throw new NotSupportedException("Actual shift1/cap8 ordinary group required.");
                if(Spans[arc.GroupId]!=-1&&Spans[arc.GroupId]!=arc.Span)throw new NotSupportedException("Multiple group spans require the separate native pooled alias lifecycle.");Spans[arc.GroupId]=arc.Span;
            }
        }
        Ranges=new LegacyInkRange[owner.GroupCount][];int rangeCount=0;
        for(int g=0;g<Ranges.Length;g++)
        {
            token.ThrowIfCancellationRequested();if(Spans[g]<0){Ranges[g]=[];continue;}var ranges=owner.CopyGroupRanges(g);
            if(ranges.Length is<1 or>15||ranges.Length>131072-rangeCount)throw new NotSupportedException("Bounded actual live group ranges required.");rangeCount+=ranges.Length;Ranges[g]=ranges;
        }
        Counts=owner.CopyCutCounts();if(Counts.Length is<1 or>256)throw new ArgumentException("Bounded physical cut counts required.");int points=0;foreach(int n in Counts){token.ThrowIfCancellationRequested();if(n<1||n>4096-points)throw new ArgumentException("Whole source point budget exceeded.");points+=n;}
        Strokes=owner.CopyCutStrokes();if(Strokes.Length!=Counts.Length||Strokes.Where((s,i)=>s.Points.Count!=Counts[i]).Any())throw new InvalidDataException("Actual whole cut snapshot required.");
        Probabilities=owner.Owner.CopyPhysicalProbabilities();if(Probabilities.Length!=Counts.Length)throw new InvalidDataException("Same current physical probability rows required.");
        Prepared=LegacyCursivePreparedInk.Create(Strokes,token);token.ThrowIfCancellationRequested();
    }
    internal static long Preflight(int groups,int nodes,int candidates)
    {
        if(groups is<1 or>4096||nodes is<1 or>4096||candidates is<1 or>4096)throw new ArgumentException("Whole actual MAIN graph dimensions required.");
        long bytes=checked(65536L+1048576+groups*1024L+131072L*12+131072L*16+4096L*128+2097152);
        if(bytes>16L*1024*1024)throw new ArgumentException("Logical MAIN copied payload/work envelope exceeded.");return bytes;
    }
}

/// <summary>Transient genuine group geometry/four inputs. Production keeps only
///final small scores; raw arrays are internal detached diagnostic snapshots.</summary>
internal sealed class LegacyEnglishMainGlyphFeatures
{
    internal int Group{get;}
    internal LegacyInkRange[] Stored{get;}
    internal LegacyInkRange[] Ordered{get;}
    internal int[] Order{get;}
    internal LegacyInkStroke[] Normalized{get;}
    internal LegacyInkStroke[] Merged{get;}
    internal int[] RetainedPenUps{get;}
    internal short[][] Inputs{get;}
    internal LegacyDynamicAcousticWeightResult Dynamic{get;}
    internal LegacyCursiveGlyphBoundarySigns Boundary{get;}
    internal byte SourceCount{get;}
    internal bool IsDegenerate{get;}
    internal bool WholePhysicalStroke{get;}
    internal bool EndpointKeysZero{get;}
    internal float? SourceProbability{get;}
    private LegacyEnglishMainGlyphFeatures(int group,LegacyInkRange[] stored,LegacyInkRange[] ordered,int[] order,
        LegacyInkStroke[] normalized,LegacyNormalizedPenUpMergeResult merge,short[][] inputs,LegacyDynamicAcousticWeightResult dynamic,
        LegacyCursiveGlyphBoundarySigns boundary,byte sourceCount,bool degenerate,bool whole,bool zero,float? probability)
    {Group=group;Stored=stored;Ordered=ordered;Order=order;Normalized=normalized;Merged=merge.Strokes.ToArray();RetainedPenUps=merge.RetainedBoundaryIndices.ToArray();Inputs=inputs;Dynamic=dynamic;Boundary=boundary;SourceCount=sourceCount;IsDegenerate=degenerate;WholePhysicalStroke=whole;EndpointKeysZero=zero;SourceProbability=probability;}
    internal static LegacyEnglishMainGlyphFeatures Prepare(LegacyEnglishMainInkSnapshot snapshot,LegacyEnglishMainAcousticModel model,int group,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(!ReferenceEquals(snapshot.Inputs.Source,model.Source)||(uint)group>=(uint)snapshot.Ranges.Length||snapshot.Spans[group]<0)throw new ArgumentException("Actual same-source live group required.");
        var stored=snapshot.Ranges[group].ToArray();var keys=new LegacySourcePointKey[stored.Length];int pointCount=0;float minX=float.MaxValue;
        for(int i=0;i<stored.Length;i++)
        {
            var r=stored[i];if((uint)r.StrokeIndex>=(uint)snapshot.Counts.Length||r.StartPoint<0||r.EndPointExclusive<=r.StartPoint||r.EndPointExclusive>snapshot.Counts[r.StrokeIndex]||r.EndPointExclusive-r.StartPoint>4096-pointCount)throw new ArgumentException("Bounded actual range membership required.");pointCount+=r.EndPointExclusive-r.StartPoint;
            keys[i]=snapshot.Owner.ResolveCutPoint(r.StrokeIndex,r.StartPoint).Key;for(int p=r.StartPoint;p<r.EndPointExclusive;p++){token.ThrowIfCancellationRequested();minX=MathF.Min(minX,snapshot.Strokes[r.StrokeIndex].Points[p].X);}
        }
        //1B5BA0 creates one FLOAT stroke per STORED range for257, independent
        //of chronology sorting and of integer normalization/pen-up merging.
        var dynamicInk=new InkStroke[stored.Length];for(int i=0;i<stored.Length;i++){token.ThrowIfCancellationRequested();var r=stored[i];dynamicInk[i]=new(snapshot.Strokes[r.StrokeIndex].Points.Skip(r.StartPoint).Take(r.EndPointExclusive-r.StartPoint).ToArray());}
        var dynamic=LegacyDynamicAcousticWeights.Evaluate(dynamicInk,model.DynamicProfile,token);
        int[] order=Enumerable.Range(0,stored.Length).ToArray();Sort(0,order.Length-1);
        void Sort(int lo,int hi)
        {int left=lo,right=hi;var pivot=keys[order[(lo+hi)/2]];while(left<=right){token.ThrowIfCancellationRequested();while(Compare(keys[order[left]],pivot)<0)left++;while(Compare(keys[order[right]],pivot)>0)right--;if(left>right)break;(order[left],order[right])=(order[right],order[left]);left++;right--;}if(lo<right)Sort(lo,right);if(left<hi)Sort(left,hi);}
        var ordered=order.Select(i=>stored[i]).ToArray();var boundary=LegacyCursiveGlyphBoundaryPolicy.Resolve(stored,snapshot.Counts,0,false,false,token);
        var normalized=LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(snapshot.Prepared,ordered,minX,snapshot.Owner.Baseline,snapshot.Owner.Height,100,15,token).Strokes.ToArray();
        var merged=LegacyNormalizedPenUpMerge.Merge(normalized,10,token);var strokes=merged.Strokes;var first=strokes[0].Points[0];bool degenerate=true;foreach(var s in strokes)foreach(var p in s.Points)if(p.X!=first.X||p.Y!=first.Y)degenerate=false;
        short[][] inputs=new short[4][];short[] trajectory,raster;
        if(degenerate)
        {
            trajectory=new short[226];trajectory.AsSpan(6,30).Fill(1024);short[] stats=[1024,-768,0,0,-768,1024,-1024];stats.CopyTo(trajectory,210);var baseline=DegenerateBaseline(first.Y);baseline.CopyTo(trajectory,217);
            raster=new short[208];stats.CopyTo(raster,0);baseline.CopyTo(raster,7);raster.AsSpan(16,64).Fill(-1024);raster.AsSpan(80).Fill(1024);
        }
        else
        {
            var geometry=LegacyGlyphGeometry.Create(strokes,token);var pixels=LegacyRasterFeatures.Rasterize(geometry);var baseline=LegacyBaselineFeatures.Extract(geometry,new(0,100),pixels);
            trajectory=new short[226];LegacyStrokeFeatures.Compose(geometry.SampleX,geometry.SampleY,geometry.PenState,strokes.Count,geometry.Width,geometry.Height).CopyTo(trajectory,0);baseline.CopyTo(trajectory,217);
            raster=new short[208];trajectory.AsSpan(210,7).CopyTo(raster);baseline.CopyTo(raster,7);LegacyRasterFeatures.Project(pixels).CopyTo(raster,16);
        }
        inputs[0]=Append(trajectory,boundary);inputs[1]=Append(raster,boundary);
        var samples=LegacyDirectionalResampling.Prepare(strokes,100,false,token);
        if(samples.CumulativePhysicalLengths.Length is<1 or>255)throw new ArgumentException("Actual2B1 facade admits1..255 prepared samples before shared byte-indexed consumers.");
        var direction=LegacyDirectional2B1Landmarks.Extract(samples.Strokes,token);var intersections=LegacyDirectionalIntersections.Extract(samples,10,token);
        var global=LegacyDirectionalStatistics.Extract(samples.Strokes,samples.Width,samples.Height,samples.PhysicalArcLength,direction.Flags.Span,intersections.Flags.Span,intersections.SelfLoopCount,token);
        int[] baseStats=samples.SourceStepFixed==0?LegacyDirectionalBaselineStatistics.ExtractPrepared(samples.Strokes,samples.Width,samples.Height,samples.PhysicalArcLength,0,100,token):LegacyDirectionalBaselineStatistics.Extract(samples,new(0,100),token);
        var temporalMembership=LegacyArcLengthWindows.Partition(samples.CumulativePhysicalLengths.Span,16,token);int[][] temporal=new int[16][],spatial=new int[16][];
        var spatialMembership=LegacySpatialWindows.Partition(samples.Strokes.SelectMany(s=>s.Points.ToArray()).ToArray(),samples.Width,samples.Height,4,4,false,token);
        for(int i=0;i<16;i++){token.ThrowIfCancellationRequested();temporal[i]=LegacyDirectionalWindowStatistics.Extract(samples,temporalMembership[i].Span,direction.Flags.Span,intersections.Flags.Span,false,token);spatial[i]=LegacySpatialWindowStatistics.Extract(samples,spatialMembership[i].ToArray().Select(i=>checked((byte)i)).ToArray(),direction.Flags.Span,intersections.Flags.Span,false,token);}
        inputs[2]=Append(LegacyEnglishMainFeatureAssembly.Compose(global,temporal,baseStats,false,token),boundary);inputs[3]=Append(LegacyEnglishMainFeatureAssembly.Compose(global,spatial,baseStats,true,token),boundary);
        bool whole=strokes.Count==1&&stored[0].StartPoint==0&&stored[^1].EndPointExclusive==snapshot.Counts[stored[^1].StrokeIndex];bool zero=false;float? probability=null;
        if(whole){var start=snapshot.Owner.ResolveCutPoint(stored[0].StrokeIndex,stored[0].StartPoint);var end=snapshot.Owner.ResolveCutEndingMetadata(stored[^1].StrokeIndex,stored[^1].EndPointExclusive-1,token);zero=start.Key.Third==0&&end.Key.Third==0;probability=snapshot.Probabilities[stored[0].StrokeIndex];}
        byte sourceCount=LegacyPreparedSourceStrokeCount.Count(stored.Select(r=>r.StrokeIndex).ToArray(),0,token);token.ThrowIfCancellationRequested();return new(group,stored,ordered,order,normalized,merged,inputs,dynamic,boundary,sourceCount,degenerate,whole,zero,probability);
    }
    private static int Compare(LegacySourcePointKey a,LegacySourcePointKey b){int c=a.First.CompareTo(b.First);if(c!=0)return c;c=a.Second.CompareTo(b.Second);return c!=0?c:a.Third.CompareTo(b.Third);}
    private static short[] Append(short[] raw,LegacyCursiveGlyphBoundarySigns signs){var r=new short[raw.Length+2];raw.CopyTo(r,0);r[^2]=signs.Start;r[^1]=signs.End;return r;}
    //00A770 actual1111: baseline0,height100 andoriginal degenerate shortY.
    //This explicit branch differs from nondegenerate raster-baseline clamping.
    private static short[] DegenerateBaseline(short y)
    {
        const int unit=1024,height=100;var r=new short[9];r[0]=r[1]=r[3]=-unit;r[2]=(short)Math.Clamp((int)unchecked((short)(256*y/height)),-unit,unit);
        if(y>0){r[4]=r[5]=r[7]=-unit;r[6]=unit;r[8]=unchecked((short)(512*(-y-height)/height-unit));}
        else if(y>=height){r[4]=r[6]=r[7]=r[8]=-unit;r[5]=unit;}
        else{r[4]=unit;r[5]=r[6]=r[8]=-unit;r[7]=unchecked((short)(512*y/height-unit));}return r;
    }
}

/// <summary>Actual2B1 supplied-null/default274 branch, with both native
///saturation stages. The catalog and pure kernel are reusable across languages.</summary>
internal static class LegacyEnglishMainFeatureAssembly
{
    internal static short[] Compose(int[] global,int[][] local,int[] baseline,bool spatial,CancellationToken token)
        =>LegacySaturatedDirectionalFeatureAssembly.Compose(global,local,baseline,spatial,
            LegacyDirectional2B1DefaultCalibration.Minimum(spatial),LegacyDirectional2B1DefaultCalibration.Maximum(spatial),token);
}
