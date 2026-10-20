namespace InkRecognition;

public enum LegacyPhysicalRawTimingKind { Uniform, MeasuredMilliseconds }

/// <summary>Explicit host physical constructor parameters. Neither mode
/// infers acquisition times from source indices or SDK defaults.</summary>
public sealed class LegacyPhysicalStrokeConstructor
{
    private readonly int[]? samples;
    public LegacyPhysicalRawTimingKind Kind{get;}
    public float FormatRateHz{get;}
    public long BaseTagMicroseconds{get;}
    private LegacyPhysicalStrokeConstructor(LegacyPhysicalRawTimingKind kind,float rate,long start,int[]? samples)
    {Kind=kind;FormatRateHz=rate;BaseTagMicroseconds=start;this.samples=samples;}
    public static LegacyPhysicalStrokeConstructor Uniform(float explicitRateHz,long baseTagMicroseconds)
        =>new(LegacyPhysicalRawTimingKind.Uniform,explicitRateHz,baseTagMicroseconds,null);
    public static LegacyPhysicalStrokeConstructor MeasuredMilliseconds(float explicitFormatRateHz,
        long baseTagMicroseconds,IReadOnlyList<int> actualSampleTimesMilliseconds)
    {
        ArgumentNullException.ThrowIfNull(actualSampleTimesMilliseconds);
        int count=actualSampleTimesMilliseconds.Count;
        if(count is<1 or>4096)throw new ArgumentException("Bounded complete actual time channel required.");
        var copy=new int[count];for(int i=0;i<count;i++)copy[i]=actualSampleTimesMilliseconds[i];
        if(actualSampleTimesMilliseconds.Count!=count)throw new InvalidOperationException("Time channel count changed during copy.");
        return new(LegacyPhysicalRawTimingKind.MeasuredMilliseconds,explicitFormatRateHz,baseTagMicroseconds,
            copy);
    }
    internal int Count=>samples?.Length??0;
    internal int[]? CopySamples()=>samples?.ToArray();
    internal int Sample(int index)=>samples![index];
}

/// <summary>A distinct raw root constructed with explicit physical metadata
/// before inference. Old XY roots cannot be retroactively attached here.</summary>
public sealed class LegacyPhysicalRawInkIngress
{
    public LegacyOwnedRawInkDocument Document{get;}
    public LegacyFreshRawInkCurrentView CurrentView{get;}
    public LegacyPhysicalRawInkTiming TimingReceipt{get;}
    private LegacyPhysicalRawInkIngress(LegacyOwnedRawInkDocument document,
        LegacyPhysicalTimingSource[] sources,CancellationToken token)
    {Document=document;CurrentView=LegacyFreshRawInkCurrentView.Prepare(document,token);
     TimingReceipt=new(this,sources);TimingReceipt.Validate(token);}

    public static LegacyPhysicalRawInkIngress Create(IReadOnlyList<InkStroke> strokes,
        IReadOnlyList<LegacyPhysicalStrokeConstructor> parameters,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(strokes);ArgumentNullException.ThrowIfNull(parameters);
        token.ThrowIfCancellationRequested();
        int sourceCount=strokes.Count;
        if(sourceCount is<1 or>256||parameters.Count!=sourceCount)
            throw new ArgumentException("One explicit constructor per actual raw source required.");
        var actualStrokes=new InkStroke[sourceCount];
        var actualParameters=new LegacyPhysicalStrokeConstructor[sourceCount];
        var pointCounts=new int[sourceCount];
        for(int i=0;i<sourceCount;i++)
        {
            actualStrokes[i]=strokes[i]??throw new ArgumentException("Null raw source.");
            actualParameters[i]=parameters[i]??throw new ArgumentException("Null physical constructor.");
        }
        long points=0;
        //All source/channel/numerical bounds precede Document creation/copies.
        for(int i=0;i<sourceCount;i++)
        {
            token.ThrowIfCancellationRequested();var p=actualParameters[i];
            int n=actualStrokes[i].Points?.Count??0;points+=n;
            if(n is<1 or>4096||points>4096)throw new NotSupportedException("Complete raw root point budget exceeded.");
            LegacyPhysicalRawInkTiming.ValidateConstructor(p,n,token);
            pointCounts[i]=n;
            for(int point=0;point<n;point++)
            {
                token.ThrowIfCancellationRequested();var q=actualStrokes[i].Points[point];
                if(!float.IsFinite(q.X)||!float.IsFinite(q.Y)||MathF.Abs(q.X)>1_000_000||MathF.Abs(q.Y)>1_000_000)
                    throw new ArgumentException("Finite bounded raw coordinates required.");
            }
        }
        var sources=new LegacyPhysicalTimingSource[sourceCount];
        var snapshots=new InkStroke[sourceCount];
        for(int i=0;i<sources.Length;i++)
        {
            token.ThrowIfCancellationRequested();var p=actualParameters[i];int n=pointCounts[i];
            if(actualStrokes[i].Points.Count!=n)throw new InvalidOperationException("Source point count changed before physical copy.");
            var copy=new InkPoint[n];
            for(int index=0;index<n;index++)copy[index]=actualStrokes[i].Points[index];
            if(actualStrokes[i].Points.Count!=n)throw new InvalidOperationException("Source point count changed during physical ingestion.");
            snapshots[i]=new(Array.AsReadOnly(copy));
            sources[i]=new(i,n,p.Kind,p.FormatRateHz,LegacyPhysicalRawInkTiming.NativePeriod(p.FormatRateHz),
                p.BaseTagMicroseconds,p.CopySamples());
        }
        var document=LegacyOwnedRawInkDocument.Create(snapshots,token);
        return new(document,sources,token);
    }
}
