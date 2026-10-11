namespace InkRecognition;

public readonly record struct LegacyLineGuidelineMetadata(float AlignmentBaseline, float AlignmentHeight,
    int GuidelineIndex, int RowIndex);

/// <summary>
/// Class219 interface+88 (Text 180169750): adds guideline metadata, preserving
/// the computed baseline, height and slope. Explicit first user guideline only.
/// </summary>
public static class LegacyLineGuidelineAdjustment
{
    public static LegacyLineGuidelineMetadata Compute(IReadOnlyList<InkPoint[]> preparedStrokes,
        LegacyLinePair originalLine, float slope, bool enabled, LegacyLineCluster? firstUserGuideline=null,
        int rowIndex=int.MinValue, CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(preparedStrokes);cancellationToken.ThrowIfCancellationRequested();
        if(!enabled)return new(0,0,-1,int.MinValue);
        if(preparedStrokes.Count is <1 or >4096 || !float.IsFinite(originalLine.Baseline)||!float.IsFinite(originalLine.Height)||!float.IsFinite(slope))
            throw new ArgumentException("Invalid explicit line geometry.");
        float minX=float.MaxValue,maxX=-float.MaxValue;int count=0;
        foreach(var stroke in preparedStrokes)
        {
            if(stroke is null||stroke.Length==0||stroke.Length>1_000_000-count)throw new ArgumentException("Expected bounded nonempty strokes.");
            count+=stroke.Length;
            foreach(var point in stroke)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if(!float.IsFinite(point.X)||!float.IsFinite(point.Y))throw new ArgumentException("Nonfinite point.");
                minX=MathF.Min(minX,point.X);maxX=MathF.Max(maxX,point.X);
            }
        }
        float first=minX*slope+originalLine.Baseline,last=maxX*slope+originalLine.Baseline;
        float height=originalLine.Height,alignment;int guideline=-1,row=int.MinValue;
        if(firstUserGuideline is not{} g)alignment=(last+first)*.5f;
        else
        {
            if(g.RowCount<1||!float.IsFinite(g.Baseline)||!float.IsFinite(g.Pitch))throw new ArgumentException("Invalid user guideline.");
            guideline=0;row=0;alignment=g.Baseline;
            if(g.RowCount!=1)
            {
                if(g.Pitch==0)throw new ArgumentException("Multiple-row user guideline needs nonzero pitch.");
                row=rowIndex;
                if(row==int.MinValue)
                {
                    float position=(first-g.Baseline)/g.Pitch;
                    float rounded=position>0?position+.5f:position-.5f;
                    if(!float.IsFinite(rounded)||(double)rounded<int.MinValue+1.0||(double)rounded>int.MaxValue)
                        throw new InvalidDataException("User-guideline row overflow.");
                    row=(int)rounded;
                }
                float center=(last+first)*.5f;alignment=(float)row*g.Pitch+g.Baseline;
                if(center<alignment)height+=alignment-center;
            }
        }
        if(!float.IsFinite(alignment)||!float.IsFinite(height))throw new InvalidDataException("Guideline metadata overflow.");
        return new(alignment,height,guideline,row);
    }
}
