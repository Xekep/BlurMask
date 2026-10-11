namespace InkRecognition;

/// <summary>Owned whole-input Russian106 geometry output; atom ranges address PreparedInk.</summary>
public sealed record LegacyCursiveCutResult(LegacyCursivePreparedInk PreparedInk,
    IReadOnlyList<LegacyCursiveCutMarker> Markers,IReadOnlyList<LegacyCursiveInkAtom> Atoms);

/// <summary>Confirmed old Russian297 whole-buffer191950 controller with explicit runtime metrics.</summary>
public static class LegacyCursiveCutProducer
{
    /// <summary>Convenience overload when the caller explicitly supplies one confidence for all physical strokes.</summary>
    public static LegacyCursiveCutResult GenerateRussianVersion106(IReadOnlyList<InkStroke> source,
        float? baseline,float lineHeight,float baselineConfidence,bool wholeStrokeOnly=false,
        int? closedStrokeCategoryLimit=null,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(source);
        return GenerateRussianVersion106(source,baseline,lineHeight,Enumerable.Repeat(baselineConfidence,source.Count).ToArray(),wholeStrokeOnly,closedStrokeCategoryLimit,cancellationToken);
    }

    /// <summary>The native service virtual60 receives the current physical stroke index; its outputs are explicit.</summary>
    public static LegacyCursiveCutResult GenerateRussianVersion106(IReadOnlyList<InkStroke> source,
        float? baseline,float lineHeight,IReadOnlyList<float> baselineConfidences,bool wholeStrokeOnly=false,
        int? closedStrokeCategoryLimit=null,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(source);ArgumentNullException.ThrowIfNull(baselineConfidences);
        if(baselineConfidences.Count!=source.Count || baselineConfidences.Any(c=>!float.IsFinite(c)))throw new ArgumentException("Each physical stroke requires an explicit finite native service confidence.");
        if(!float.IsFinite(lineHeight) || lineHeight<=0 ||
            baseline is float b && !float.IsFinite(b))throw new ArgumentException("Cut producer requires explicit finite line metrics.");
        var ink=LegacyCursivePreparedInk.Create(source,cancellationToken);
        float previousMaximum=-float.MaxValue;
        for(int stroke=0;stroke<source.Count;stroke++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var raw=source[stroke].Points;
            float minX=raw.Min(p=>p.X),maxX=raw.Max(p=>p.X),minY=raw.Min(p=>p.Y),maxY=raw.Max(p=>p.Y);
            if(!wholeStrokeOnly)
            {
                bool normal=baseline is null || baseline==float.MaxValue;
                if(!normal && baselineConfidences[stroke]<.5f)
                {
                    normal=previousMaximum-lineHeight*.3f<maxX;
                    if(!normal && lineHeight+lineHeight<maxX-minX && lineHeight<maxY-minY)
                    {
                        int start=0;for(int i=0;i<stroke;i++)start+=ink.StrokePointCounts.Span[i];
                        int end=start+ink.StrokePointCounts.Span[stroke];int direction=0,reversals=0;
                        var points=ink.Points.Span;
                        for(int i=start;i<end-1;i++)
                        {
                            if(points[i].DeltaY<0){if(direction==-1)reversals++;direction=1;}
                            else if(points[i].DeltaY>0){if(direction==1 && i>0 && points[i-1].DeltaX>0)reversals++;direction=-1;}
                            if(reversals>3){normal=true;break;}
                        }
                    }
                }
                if(normal)
                {
                    int start=0;for(int i=0;i<stroke;i++)start+=ink.StrokePointCounts.Span[i];
                    int count=ink.StrokePointCounts.Span[stroke];var rows=ink.Points.Slice(start,count).ToArray();
                    var local=LegacyCursiveMarkerGeometry.MarkVerticalReversals([count],rows.Select(p=>p.DeltaX).ToArray(),rows.Select(p=>p.DeltaY).ToArray(),rows.Select(p=>p.Tag).ToArray(),cancellationToken);
                    var all=ink.Points.ToArray().Select(p=>p.Tag).ToArray();local.CopyTo(all,start);ink=ink.WithTags(all);
                    ink=LegacyCursiveLoopCuts.Apply(ink,stroke,lineHeight,LegacyCursiveLoopCuts.RussianReversalSpacing,cancellationToken);
                    ink=LegacyCursiveHorizontalCuts.Apply(ink,stroke,lineHeight,LegacyCursiveHorizontalCutSettings.RussianVersion106,false,cancellationToken);
                    ink=LegacyCursiveControllerFilters.BeforeBaseline(ink,stroke,cancellationToken);
                    ink=LegacyCursiveBaselineCuts.Apply(ink,stroke,baseline,lineHeight,LegacyCursiveBaselineCuts.RussianGapRadius,cancellationToken);
                    ink=LegacyCursiveControllerFilters.AfterBaselineRussianVersion106(ink,stroke,baseline,lineHeight,cancellationToken);
                }
                // The native success path invokes this after normal stages too.
                ink=LegacyCursiveStrokeOverride.ApplyRussianVersion106(ink,source,stroke,baseline,lineHeight,0,source.Count-1,cancellationToken);
            }
            previousMaximum=Math.Max(previousMaximum,maxX);
        }
        var records=ink.Points.ToArray();
        var pruned=LegacyCursiveMarkerGeometry.PruneNearPenLifts(ink.StrokePointCounts.Span,records.Select(p=>p.SegmentLength).ToArray(),records.Select(p=>p.Tag).ToArray(),lineHeight,.5f,cancellationToken);
        ink=LegacyCursiveMarkerLimit.Apply(ink.WithTags(pruned),source,lineHeight,LegacyCursiveMarkerLimitOptions.RussianVersion106(closedStrokeCategoryLimit),cancellationToken);
        var markers=LegacyCursiveMarkerGeometry.ProjectMarkers(ink.StrokePointCounts.Span,ink.Points.ToArray().Select(p=>p.Tag).ToArray(),cancellationToken);
        var atoms=LegacyCursiveInkAtoms.Build(ink.StrokePointCounts.Span,markers,cancellationToken);
        return new(ink,Array.AsReadOnly(markers),Array.AsReadOnly(atoms));
    }
}
