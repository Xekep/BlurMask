namespace InkRecognition;

/// <summary>DATA-only ink bounds in the exact input coordinate system.</summary>
public readonly record struct RecognizedInkBounds(float Left,float Top,float Right,float Bottom);
/// <summary>Detached ordered WORD DATA. It carries no source/query authority.</summary>
public sealed class PositionedWordRun
{
    private readonly LegacyNativeInkPositionRange[] ranges;
    public string Text{get;}
    public float CandidateScore{get;}
    public RecognizedInkBounds InkBounds{get;}
    public LegacyCompletedGlobalTextWords.PreparedLine Line{get;}
    public IReadOnlyList<LegacyNativeInkPositionRange> OriginalRanges{get;}
    public PositionedWordRun(string text,float candidateScore,RecognizedInkBounds inkBounds,
        LegacyCompletedGlobalTextWords.PreparedLine line,IReadOnlyList<LegacyNativeInkPositionRange> originalRanges)
    {
        ArgumentNullException.ThrowIfNull(text);ArgumentNullException.ThrowIfNull(originalRanges);
        if(text.Length>1024||originalRanges.Count is<1 or>4096)throw new ArgumentOutOfRangeException(nameof(originalRanges));
        Text=text;CandidateScore=candidateScore;InkBounds=inkBounds;Line=line;
        ranges=originalRanges.ToArray();OriginalRanges=Array.AsReadOnly(ranges);
    }
}
/// <summary>Opt-in whole-input ordered WORD delivery. A consumer must not
/// manufacture a flat string from the WORD vector.</summary>
public interface IPositionedStrokeTextRecognizer:IStrokeTextRecognizer
{bool ProducesPositionedWordRuns{get;}}

/// <summary>Geometry projection only; DATA does not become a source issuer.
/// Reuses exact selected-object descriptors for fractional/source boundaries.</summary>
public readonly record struct RecognizedInkBoundsProjection(RecognizedInkBounds Bounds,long PointReads);
public static class RecognizedWordInkBounds
{
    public const long MaximumWholeRunPointReads=1_048_576;
    public static bool CoversFullInput(IReadOnlyList<InkStroke> input,IReadOnlyList<LegacyNativeInkPositionRange> ranges,CancellationToken token=default)
    {
        if(input.Count is<1 or>256||ranges.Count is<1 or>4096||input.Sum(s=>(long)s.Points.Count)>20000) return false;
        var document=LegacyOwnedRawInkDocument.Create(input,token);
        return ExperimentalGlobalTextRecognizer.HasFullOriginalCoverage(document,ranges.ToArray(),token);
    }
    public static RecognizedInkBounds FromOriginalRanges(IReadOnlyList<InkStroke> input,
        IReadOnlyList<LegacyNativeInkPositionRange> ranges,CancellationToken token=default)
        =>Project(input,ranges,MaximumWholeRunPointReads,token).Bounds;
    /// <summary>Bound actual input point-indexer reads BEFORE each selected
    /// part; framework/scalar CPU and prior inference are separate.</summary>
    public static RecognizedInkBoundsProjection Project(IReadOnlyList<InkStroke> input,
        IReadOnlyList<LegacyNativeInkPositionRange> ranges,long maximumPointReads,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(input);ArgumentNullException.ThrowIfNull(ranges);token.ThrowIfCancellationRequested();
        if(maximumPointReads is<1 or>MaximumWholeRunPointReads)throw new NotSupportedException("Whole WORD projection point-read allowance exhausted.");
        if(input.Count is<1 or>256||ranges.Count is<1 or>4096||input.Sum(s=>(long)s.Points.Count)>20000)
            throw new NotSupportedException("Bounded same original input and WORD ranges required.");
        var objects=input.Select(s=>new LegacyNativeSourceInkObject(s.Points.Count)).ToArray();
        var selected=LegacyNativeSelectedInkProducer.Select(ranges.ToArray(),objects,token);
        float left=float.PositiveInfinity,top=left,right=float.NegativeInfinity,bottom=right;long visits=0;
        foreach(var part in selected)
        {
            token.ThrowIfCancellationRequested();var points=input[(int)part.SourceIndex].Points;
            int first=part.FirstPoint,last=part.LastPoint<0?points.Count-1:part.LastPoint;
            if(first<0||last<first||last>=points.Count||(first==last&&part.FirstFraction>part.LastFraction)||part.FirstFraction is<0 or>=1||part.LastFraction is<0 or>=1)
                throw new ArgumentException("Invalid original source range extent.");
            long reads=checked(last-first+2+(part.FirstFraction>0?1:0)+(part.LastFraction>0?1:0));
            if(reads>maximumPointReads-visits)throw new NotSupportedException("Whole WORD bound projection point-read ceiling exceeded before access.");
            visits+=reads;
            Add(At(first,part.FirstFraction));
            for(int p=first+1;p<=last;p++)Add(points[p]);
            Add(At(last,part.LastFraction));
            InkPoint At(int p,float fraction)
            {
                var a=points[p];if(fraction==0)return a;
                if(p+1>=points.Count)throw new ArgumentException("Fractional source endpoint has no next point.");
                var b=points[p+1];return new(a.X+(b.X-a.X)*fraction,a.Y+(b.Y-a.Y)*fraction);
            }
        }
        if(!float.IsFinite(left)||!float.IsFinite(top)||!float.IsFinite(right)||!float.IsFinite(bottom)||right<=left||bottom<=top)
            throw new NotSupportedException("A positioned WORD requires finite nonzero original ink extent.");
        return new(new(left,top,right,bottom),visits);
        void Add(InkPoint p)
        {
            token.ThrowIfCancellationRequested();if(!float.IsFinite(p.X)||!float.IsFinite(p.Y))throw new ArgumentException("Finite original points required.");
            left=Math.Min(left,p.X);right=Math.Max(right,p.X);top=Math.Min(top,p.Y);bottom=Math.Max(bottom,p.Y);
        }
    }
}
