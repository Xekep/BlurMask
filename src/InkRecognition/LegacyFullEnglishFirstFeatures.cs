using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Transient actual full-FIRST post-cut live-group features:
/// FF/FF normalization,217/199 basic families and calibrated baseline-free274
/// directional families, each followed by two actual boundary signs.</summary>
public sealed class LegacyFullEnglishFirstFeatures
{
    public LegacyFullEnglishFirstCandidates SourceOwner {get;}
    public LegacyFirstAcoustic119WireModel Model=>SourceOwner.Model;
    public LegacyCursiveGlyphBoundarySigns Boundary {get;}
    public int MaximumMergedStrokes {get;}
    public bool DegenerateIntegerGeometry {get;}
    private readonly LegacyInkRange[] stored,ordered;
    private readonly int[] order,retained;
    private readonly LegacyInkStroke[] normalized,merged;
    private readonly short[] trajectory,raster;
    private short[] temporal=[],spatial=[];
    internal short[][] Inputs=>[trajectory,raster,temporal,spatial];
    private LegacyFullEnglishFirstFeatures(LegacyFullEnglishFirstCandidates owner,
        LegacyInkRange[] stored,LegacyInkRange[] ordered,int[] order,LegacyCursiveGlyphBoundarySigns boundary,
        int cap,LegacyInkStroke[] normalized,LegacyNormalizedPenUpMergeResult merged,bool degenerate,short[] trajectory,short[] raster)
    {SourceOwner=owner;this.stored=stored;this.ordered=ordered;this.order=order;Boundary=boundary;MaximumMergedStrokes=cap;
        this.normalized=normalized;this.merged=merged.Strokes.ToArray();retained=merged.RetainedBoundaryIndices.ToArray();
        DegenerateIntegerGeometry=degenerate;this.trajectory=trajectory;this.raster=raster;}

    internal static LegacyFullEnglishFirstFeatures Prepare(LegacyFullEnglishFirstCandidates owner,int group,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(owner);if(!owner.IsLive(group))throw new ArgumentException("Actual live full-FIRST group required.");
        var preparedRanges=owner.CopyGroupRanges(group).AsSpan();var physical=owner.Physical;int[] counts=owner.Counts;
        LegacySourcePointIdentity resolve(int stroke,int point)=>owner.ResolveCutPoint(stroke,point);
        ArgumentNullException.ThrowIfNull(owner);token.ThrowIfCancellationRequested();var model=owner.Model;
        byte[] Field(string scope,string name)=>model.Fields.Single(f=>f.Owner==scope&&f.Destination==name).CopyBytes();
        ushort U16(string scope,string name)=>BinaryPrimitives.ReadUInt16LittleEndian(Field(scope,name));
        if(Field("21D","08")[0]!=255||Field("21D","09")[0]!=255||model.RawNormalizationTarget!=100||
            U16("profile0","14")!=0x1011||BinaryPrimitives.ReadUInt32LittleEndian(Field("profile0","18"))!=0x3f||
            U16("profile0","1C")!=30||U16("profile0","1E")!=16||U16("profile0","20")!=8||U16("profile0","22")!=10)
            throw new NotSupportedException("Exact actual full-English FF/FF100 and1011/3F/30/16/8/10 feature settings required.");
        //Actual raw lists configure temporal16 and spatial4x4 grids; they
        //are not optional feature calibration. The lite profile has no grids.
        if(U16("profile0","48")!=1||U16("profile0","50")!=16||U16("profile0","58")!=1||U16("profile0","70")!=0||
            !Field("profile0","60/68").AsSpan().SequenceEqual(new byte[]{4,0,4,0}))
            throw new NotSupportedException("Actual full temporal16/spatial4x4 directional grids required.");
        if(model.Slots[0].Networks.Count!=1||model.Slots[1].Networks.Count!=1||
            model.Slots[0].Networks[0].InputWidth!=219||model.Slots[1].Networks[0].InputWidth!=201)
            throw new NotSupportedException("Actual full basic219/201 network slots required.");
        //Portable prepared-range cap. It is not a manufactured native graph.
        if(preparedRanges.Length is <1 or >15)throw new ArgumentException("Prepared glyph requires1..15 ranges.",nameof(preparedRanges));
        var stored=preparedRanges.ToArray();
        var keys=new LegacySourcePointKey[stored.Length];float minimumX=float.MaxValue;
        for(int i=0;i<stored.Length;i++)
        {
            var r=stored[i];
            if((uint)r.StrokeIndex>=(uint)counts.Length||r.StartPoint<0||r.EndPointExclusive<=r.StartPoint||r.EndPointExclusive>counts[r.StrokeIndex])
                throw new ArgumentException("Range is outside this preprocessing owner.",nameof(preparedRanges));
            keys[i]=resolve(r.StrokeIndex,r.StartPoint).Key;
            for(int p=r.StartPoint;p<r.EndPointExclusive;p++){token.ThrowIfCancellationRequested();minimumX=MathF.Min(minimumX,physical[r.StrokeIndex].Points[p].X);}
        }
        //1B7F70 consumes the STORED vector.1B4E40's source-key quicksort is a
        //separate operation and must not rewrite those first/last endpoints.
        var boundary=LegacyCursiveGlyphBoundaryPolicy.Resolve(stored,counts,0,false,false,token);
        int[] order=Enumerable.Range(0,stored.Length).ToArray();Sort(0,order.Length-1);
        void Sort(int first,int last)
        {
            int left=first,right=last;var pivot=keys[order[(first+last)/2]];
            while(left<=right)
            {
                token.ThrowIfCancellationRequested();
                while(Compare(keys[order[left]],pivot)<0)left++;
                while(Compare(keys[order[right]],pivot)>0)right--;
                if(left>right)break;(order[left],order[right])=(order[right],order[left]);left++;right--;
            }
            if(first<right)Sort(first,right);if(left<last)Sort(left,last);
        }
        var ordered=order.Select(i=>stored[i]).ToArray();
        //This is a private XY container for the existing numeric normalizer,
        //not an assertion that297 has generated cuts/inserted samples.
        var container=owner.PreparedInk;
        var normalized=LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(container,ordered,minimumX,owner.Preparation.Line.Baseline,
            owner.Preparation.Line.Height,model.RawNormalizationTarget,15,token).Strokes.ToArray();
        int cap=U16("profile0","1C")/3;
        var merged=LegacyNormalizedPenUpMerge.Merge(normalized,cap,token);
        var all=merged.Strokes.SelectMany(s=>s.Points.ToArray()).ToArray();
        bool degenerate=all.All(p=>p.X==all[0].X&&p.Y==all[0].Y);
        short[] t,raster;
        if(degenerate)
        {
            //00A9F0 actual1011/3F degenerate assignments, not missing features.
            t=new short[217];t.AsSpan(6,30).Fill(1024);
            short[] statistics=[1024,-768,0,0,-768,1024,-1024];statistics.CopyTo(t,210);
            raster=new short[199];statistics.CopyTo(raster,0);raster.AsSpan(7,64).Fill(-1024);raster.AsSpan(71).Fill(1024);
        }
        else
        {
            //Actual1011 combines these channels from ONE normalized/resampled
            //geometry state. Reuse the shared numeric kernels without running
            //their two independent public convenience pipelines.
            var ink=LegacyGlyphGeometry.Create(merged.Strokes,token);
            t=LegacyStrokeFeatures.Compose(ink.SampleX,ink.SampleY,ink.PenState,merged.Strokes.Count,ink.Width,ink.Height);
            raster=new short[199];t.AsSpan(210,7).CopyTo(raster);
            LegacyRasterFeatures.Project(LegacyRasterFeatures.Rasterize(ink)).CopyTo(raster,7);
        }
        var ti=new short[219];t.CopyTo(ti,0);ti[^2]=boundary.Start;ti[^1]=boundary.End;
        var ri=new short[201];raster.CopyTo(ri,0);ri[^2]=boundary.Start;ri[^1]=boundary.End;
        var samples=LegacyDirectionalResampling.Prepare(merged.Strokes,100,false,token);
        if(samples.CumulativePhysicalLengths.Length is<1 or>255)throw new ArgumentException("Actual2B1 shared byte-indexed domain admits1..255 prepared samples.");
        var direction=LegacyDirectional2B1Landmarks.Extract(samples.Strokes,token);var intersections=LegacyDirectionalIntersections.Extract(samples,10,token);
        var global=LegacyDirectionalStatistics.Extract(samples.Strokes,samples.Width,samples.Height,samples.PhysicalArcLength,direction.Flags.Span,intersections.Flags.Span,intersections.SelfLoopCount,token);
        var temporalMembership=LegacyArcLengthWindows.Partition(samples.CumulativePhysicalLengths.Span,16,token);
        var spatialMembership=LegacySpatialWindows.Partition(samples.Strokes.SelectMany(s=>s.Points.ToArray()).ToArray(),samples.Width,samples.Height,4,4,false,token);
        int[][] temporalWindows=new int[16][],spatialWindows=new int[16][];
        for(int i=0;i<16;i++)
        {
            token.ThrowIfCancellationRequested();temporalWindows[i]=LegacyDirectionalWindowStatistics.Extract(samples,temporalMembership[i].Span,direction.Flags.Span,intersections.Flags.Span,false,token);
            spatialWindows[i]=LegacySpatialWindowStatistics.Extract(samples,spatialMembership[i].ToArray().Select(i=>checked((byte)i)).ToArray(),direction.Flags.Span,intersections.Flags.Span,false,token);
        }
        short[] Direction(int[][] windows,bool spatial)
        {
            var raw=LegacyBaselineFreeDirectional2B1Assembly.Compose(global,windows,spatial,LegacyDirectional2B1DefaultCalibration.Minimum(spatial),LegacyDirectional2B1DefaultCalibration.Maximum(spatial),token);
            var result=new short[276];raw.CopyTo(result,0);result[^2]=boundary.Start;result[^1]=boundary.End;return result;
        }
        var value=new LegacyFullEnglishFirstFeatures(owner,stored,ordered,order,boundary,cap,normalized,merged,degenerate,ti,ri){temporal=Direction(temporalWindows,false),spatial=Direction(spatialWindows,true)};
        token.ThrowIfCancellationRequested();return value;
    }
    private static int Compare(LegacySourcePointKey a,LegacySourcePointKey b)
    {int c=a.First.CompareTo(b.First);if(c!=0)return c;c=a.Second.CompareTo(b.Second);return c!=0?c:a.Third.CompareTo(b.Third);}
    public LegacyInkRange[] CopyStoredRanges()=>stored.ToArray();
    public LegacyInkRange[] CopyOrderedRanges()=>ordered.ToArray();
    public int[] CopyNormalizationOrder()=>order.ToArray();
    public int[] CopyRetainedPenUpBoundaries()=>retained.ToArray();
    public LegacyInkStroke[] CopyNormalizedStrokes()=>normalized.Select(s=>new LegacyInkStroke(s.Points)).ToArray();
    public LegacyInkStroke[] CopyMergedStrokes()=>merged.Select(s=>new LegacyInkStroke(s.Points)).ToArray();
    public short[] CopyTrajectoryInput()=>trajectory.ToArray();
    public short[] CopyRasterInput()=>raster.ToArray();
    public short[] CopyTemporalInput()=>temporal.ToArray();
    public short[] CopySpatialInput()=>spatial.ToArray();
}
