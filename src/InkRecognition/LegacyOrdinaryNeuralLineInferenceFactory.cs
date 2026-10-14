namespace InkRecognition;

public sealed record LegacyOrdinaryNeuralLineInference(LegacyPrepared2e3LineFrameSnapshot Frames,
    float NeuralScale, int VerticalStripCount, ReadOnlyMemory<int> TinyPhysicalStrokeIndices);

/// <summary>Owned grouping result before any separately selected24A fitting
/// callback. Native physical memberships/NN pairs are preserved; no line frame
/// or guessed geometric profile is attached by this stage.</summary>
public sealed record LegacyOrdinaryNeuralLineGrouping(LegacyOrdinaryLineScaleSnapshot Source,
    LegacyUnknownLineGroups Selected, float NeuralScale, int VerticalStripCount,
    ReadOnlyMemory<int> TinyPhysicalStrokeIndices, IReadOnlyList<LegacyNeuralLinePair> NeuralPairs);

/// <summary>
/// Ordinary135 unknown-row07d7c0 vertical sweep, explicit2e3 scalar configuration,
/// native deferred-pair continuation, singleton fallback,07a850 and24a callback.
/// Half-horizontal source record flags are explicit; custom/known/guideline and
/// incremental modes are not reconstructed by this endpoint.
/// </summary>
public static class LegacyOrdinaryNeuralLineInferenceFactory
{
    public static LegacyOrdinaryNeuralLineInference Infer(LegacyOrdinaryLineScaleSnapshot source,
        float inputResolution, ReadOnlySpan<bool> halfHorizontalWeights, bool quarterVerticalWhenHalfHorizontal, float internalScaleQuantile,
        PreparedLineBoundaryClassifier spatial30, PreparedLineBoundaryClassifier physical50,
        LegacyAutomaticLineProfile geometryProfile, float windowWidthMultiplier, float overlap,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(source);ArgumentNullException.ThrowIfNull(source.Source);
        ArgumentNullException.ThrowIfNull(spatial30);ArgumentNullException.ThrowIfNull(physical50);
        geometryProfile.Validate();cancellationToken.ThrowIfCancellationRequested();
        var grouping=InferGrouping(source,inputResolution,halfHorizontalWeights,quarterVerticalWhenHalfHorizontal,
            internalScaleQuantile,spatial30,physical50,windowWidthMultiplier,overlap,cancellationToken);
        var strokes=source.Source.Strokes.Select(s=>s.Points.ToArray()).ToArray();
        var frames=new List<LegacyPrepared2e3LineFrame>();
        foreach(var group in grouping.Selected.Groups)
        {
            int[] ids=group.ToArray();Array.Sort(ids);var row=ids.Select(i=>strokes[i]).ToArray();
            var estimate=LegacyAutomaticLineEstimator.Estimate(row,geometryProfile,cancellationToken);
            frames.Add(new(ids,estimate,LegacyLineDeskew.Apply(row,estimate.Line,(float)estimate.Slope,cancellationToken)));
        }
        return new(new(source,frames.AsReadOnly(),grouping.Selected.StrokeGroupIndices,grouping.NeuralPairs),grouping.NeuralScale,grouping.VerticalStripCount,grouping.TinyPhysicalStrokeIndices);
    }
    /// <summary>Same restored unknown-row grouping producer as Infer, stopping
    /// before the explicit geometry callback. No geometry-profile default is
    /// selected. This permits the fresh15B260 membership stage to consume real groups
    /// independently of the later24A main guide publication. Descriptor splitting
    /// and the subsequent15F050 physical reorder remain separate producers.</summary>
    public static LegacyOrdinaryNeuralLineGrouping InferGrouping(LegacyOrdinaryLineScaleSnapshot source,
        float inputResolution, ReadOnlySpan<bool> halfHorizontalWeights, bool quarterVerticalWhenHalfHorizontal,
        float internalScaleQuantile, PreparedLineBoundaryClassifier spatial30,
        PreparedLineBoundaryClassifier physical50, float windowWidthMultiplier, float overlap,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(source);ArgumentNullException.ThrowIfNull(source.Source);
        ArgumentNullException.ThrowIfNull(spatial30);ArgumentNullException.ThrowIfNull(physical50);
        cancellationToken.ThrowIfCancellationRequested();
        var strokes=source.Source.Strokes.Select(s=>s.Points.ToArray()).ToArray();
        if(source.Source.Profile.SourceKind!=0x135 || halfHorizontalWeights.Length!=strokes.Length ||
            spatial30.HiddenWidth!=30 || physical50.HiddenWidth!=50 || !float.IsFinite(inputResolution) || inputResolution<0 || inputResolution>1_000_000 ||
            !float.IsFinite(internalScaleQuantile) || internalScaleQuantile<0 || internalScaleQuantile>1 ||
            !float.IsFinite(windowWidthMultiplier) || windowWidthMultiplier<=0 || windowWidthMultiplier>100 ||
            !float.IsFinite(overlap) || overlap<0 || overlap>=1 || !float.IsFinite(source.PrimaryScale) || source.PrimaryScale<=0)
            throw new ArgumentException("Expected ordinary135 source and explicit bounded2e3 configuration.");
        var records=strokes.Select(s=>LegacyLineStrokeStatistics.Extract(s,cancellationToken)).ToArray();
        var samples=new List<float>();
        for(int i=0;i<strokes.Length;i++)samples.AddRange(InternalSamples(strokes[i],halfHorizontalWeights[i],quarterVerticalWhenHalfHorizontal,cancellationToken));
        float scale=source.PrimaryScale;
        if(strokes.Length>1 && samples.Count>9)
        {
            var sorted=samples.ToArray();Array.Sort(sorted);
            float at=(sorted.Length-1)*internalScaleQuantile;
            int index=Math.Min(sorted.Length-1,(int)(at<=0?at-.5f:at+.5f));
            float resolutionTolerance=MathF.Max(MathF.Abs(inputResolution)*2.384185791015625e-7f,1.1920928955078125e-7f);
            float floor=resolutionTolerance<MathF.Abs(inputResolution)?inputResolution/25.4f:0;
            scale=MathF.Max(sorted[index],floor);
        }
        var tiny=strokes.Length==1?Array.Empty<int>():Enumerable.Range(0,records.Length).Where(i=>records[i].Values[3]-records[i].Values[1]<=scale*.1f || records[i].Values[6]<=scale*.25f).ToArray();
        LegacyUnknownLineGroups selected=new(Array.Empty<ReadOnlyMemory<int>>(),Enumerable.Repeat(-1,strokes.Length).ToArray());
        LegacyNeuralLinePair[] all=[];int strips=0;
        if(strokes.Length==1)selected=new(new ReadOnlyMemory<int>[] {new int[]{0}},new int[]{0});
        else
        {
            var batch=LegacyNeuralLineProposals.Prepare(records,Enumerable.Repeat(true,strokes.Length).ToArray(),scale*windowWidthMultiplier,overlap,cancellationToken);
            float height=scale*40f,margin=height*.2f;
            float top=records.Min(r=>r.Values[1]),bottom=records.Max(r=>r.Values[3]);
            var pending=new List<LegacyNeuralLinePair>();
            if(!float.IsFinite(height)||height<=0)throw new ArgumentException("Invalid vertical sweep scale.");
            while(top<bottom)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if(++strips>4096)throw new ArgumentException("Vertical sweep exceeds4096 bounded strips.");
                float end=top+height;bool final=bottom-margin<=end;if(final)end=bottom;
                bool[] future=new bool[records.Length];var current=new List<LegacyNeuralLinePair>();
                var currentIndexes=new Dictionary<(int,int),int>();
                foreach(var proposal in batch.Proposals)
                {
                    if(proposal.StrokeIndices.Length<=1)continue;
                    var ids=new List<int>();
                    foreach(int id in proposal.StrokeIndices.Span)
                    {
                        var r=records[id].Values;
                        if(r[3]<top || end<r[1]) {if(end<r[1])future[id]=true;continue;}
                        ids.Add(id);if(!future[id] && end-margin<r[3])future[id]=true;
                    }
                    if(ids.Count<=1)continue;
                    var features=LegacyLinePairFeatures.Extract(records,ids,batch.WindowWidth,scale,tiny,cancellationToken);
                    var net=proposal.UsesSpatialClassifier?spatial30:physical50;
                    var probabilities=net.Score(features.AsSpan(LegacyLinePairFeatures.Width),ids.Count-1,cancellationToken:cancellationToken);
                    for(int row=0;row<ids.Count-1;row++)
                    {
                        bool one=probabilities[row*2]<probabilities[row*2+1];
                        int a=ids[row],b=ids[row+1];if(a>b)(a,b)=(b,a);
                        float confidence=one?probabilities[row*2+1]:probabilities[row*2];
                        if(currentIndexes.TryGetValue((a,b),out int at))
                        {if(current[at].Confidence<confidence)current[at]=new(a,b,confidence,one);}
                        else {currentIndexes.Add((a,b),current.Count);current.Add(new(a,b,confidence,one));}
                    }
                }
                all=LegacyNeuralLineProposals.Aggregate(all.Concat(current).ToArray(),cancellationToken);
                var combined=LegacyNeuralLineProposals.Aggregate(pending.Concat(current).ToArray(),cancellationToken);
                var stage=LegacyUnknownLineSweepSelection.Select(records,combined,scale,selected,future,final,cancellationToken);
                selected=stage.Selected;pending=stage.DeferredPairs.ToList();
                if(final)break;
                float next=end-margin;if(next<=top)throw new ArgumentException("Vertical sweep cannot progress.");top=next;
            }
            //07e792 executes after the successful sweep even for a flat global
            //height, which has no NN strip. Physical IDs are appended in order.
            var groups=selected.Groups.Select(g=>g.ToArray()).ToList();var membership=selected.StrokeGroupIndices.ToArray();
            for(int i=0;i<membership.Length;i++)if(membership[i]<0){membership[i]=groups.Count;groups.Add(new[]{i});}
            selected=new(groups.Select(g=>new ReadOnlyMemory<int>(g)).ToArray(),membership);
        }
        selected=LegacyUnknownLinePostselection.Apply(records,selected,scale,cancellationToken);
        return new(source,selected,scale,strips,tiny,Array.AsReadOnly(all));
    }
    private static float[] InternalSamples(InkPoint[] raw,bool half,bool quarter,CancellationToken cancellationToken)
    {
        if(!half || !quarter)return LegacyLineScaleSamples.PrepareAndExtract(raw,half,cancellationToken);
        var points=new List<InkPoint>();
        foreach(var p in raw)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if(points.Count!=0)
            {
                var prev=points[^1];
                float tx=MathF.Max(MathF.Max(MathF.Abs(p.X),MathF.Abs(prev.X))*2.384185791015625e-7f,1.1920928955078125e-7f);
                float ty=MathF.Max(MathF.Max(MathF.Abs(p.Y),MathF.Abs(prev.Y))*2.384185791015625e-7f,1.1920928955078125e-7f);
                if(MathF.Abs(p.X-prev.X)<=tx && MathF.Abs(p.Y-prev.Y)<=ty)continue;
            }
            points.Add(p);
        }
        var samples=new List<float>();var anchor=points[0];float olderY=anchor.Y;
        for(int i=1;i<points.Count;i++)
        {
            var prev=points[i-1];var current=points[i];if(current.Y==prev.Y)continue;
            bool extremum=(prev.Y>olderY&&prev.Y>current.Y)||(prev.Y<olderY&&prev.Y<current.Y);olderY=prev.Y;
            if(!extremum)continue;
            samples.Add(MathF.Max(MathF.Abs(prev.X-anchor.X)*.5f,MathF.Abs(prev.Y-anchor.Y)*.75f));anchor=prev;
        }
        var last=points[^1];if(last.Y!=anchor.Y)samples.Add(MathF.Max(MathF.Abs(last.X-anchor.X)*.5f,MathF.Abs(last.Y-anchor.Y)));
        return samples.ToArray();
    }

}
