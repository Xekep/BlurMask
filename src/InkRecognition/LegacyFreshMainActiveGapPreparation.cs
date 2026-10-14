using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Actual fresh main24D -> complete29B statistical/mixture publication.
/// Source720 items must originate at the fresh156950 producer (+10=-1), not
/// pre-existing recognized-input physical records requiring the separate gate.</summary>
public sealed class LegacyFreshMainActiveGapPreparation
{
    public LegacyFreshMainShearPreparation Input {get;}
    public LegacyOrdinaryGapResult Output {get;}
    public LegacyOrdinaryGapMixtureResult? Mixture {get;}
    public IReadOnlyList<float> QualifyingGaps {get;}
    public float Blend {get;}
    public int Processor29BOffset {get;}
    private LegacyFreshMainActiveGapPreparation(LegacyFreshMainShearPreparation input,LegacyOrdinaryGapResult output,
        LegacyOrdinaryGapMixtureResult? mixture,float[] qualifying,float blend,int offset)
    {Input=input;Output=output;Mixture=mixture;QualifyingGaps=Array.AsReadOnly(qualifying);Blend=blend;Processor29BOffset=offset;}

    public static LegacyFreshMainActiveGapPreparation Prepare(ReadOnlySpan<byte> resource,LegacyFreshMainShearPreparation actualInput,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualInput);cancellationToken.ThrowIfCancellationRequested();
        if(!Convert.ToHexString(SHA256.HashData(resource)).Equals(actualInput.Input.Owner.Input.Input.Input.SourceFingerprint,StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Exact fresh main source owner required.");
        int offset=actualInput.Processor24DOffset+38;var profile=LegacyOrdinaryGapProfile.ReadV100Node(resource,offset);
        var gaussian=LegacyOrdinaryGapMixtureProfile.ReadV100Node(resource,offset);
        var d=actualInput.Input.CopyDescriptors().Select(b=>new ReadOnlyMemory<byte>(b)).ToArray();
        var g=actualInput.Input.CopyGuides().Select(b=>new ReadOnlyMemory<byte>(b)).ToArray();
        var result=LegacyNativeFreshGapPreparation.Apply(profile,gaussian,d,g,actualInput.Output.Rows,-1,cancellationToken);
        return new(actualInput,result.Output,result.Mixture,result.QualifyingGaps.ToArray(),result.Blend,offset);
    }

    /// <summary>Continue the exact owned active29B result through173C10/265;
    /// this never invokes the older no-mixture29B helper or replaces9C.</summary>
    public LegacyFreshMainActiveGapSmoothed Smooth(ReadOnlySpan<byte> resource,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var line=Input.Input;var source=line.Owner.Controller.Source;
        if(!Convert.ToHexString(SHA256.HashData(resource)).Equals(line.Owner.Input.Input.Input.SourceFingerprint,StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Exact fresh main source owner required.");
        var profile=LegacyOrientationSmoothingProfile.ReadV102Node(resource,Processor29BOffset+58);
        var metrics=LegacySourceOrientationPreparation.ExtractMetrics(source,cancellationToken);
        var points=Input.Output.CopyPhysicalPoints();var guides=line.CopyGuides();var strokes=new List<InkStroke>();
        var keys=new List<uint>();int next=0,offset=0;
        foreach(var d in line.CopyDescriptors())
        {
            int kind=BinaryPrimitives.ReadUInt16LittleEndian(d)&15;if(kind==3)continue;
            if(kind!=0)throw new NotSupportedException("Fresh ordinary physical smoothing lineage required.");
            int first=I(d,36),last=I(d,40),guide=I(d,16);
            if(first!=next||last<first||last>=points.Length||guide<0||guide>=guides.Length)
                throw new ArgumentException("Complete owned physical partition required.");
            float height=BinaryPrimitives.ReadSingleLittleEndian(guides[guide].AsSpan(4));
            var inputs=points.Skip(first).Take(last-first+1).Select(p=>new InkStroke(p)).ToArray();
            var smoothed=LegacyOrientationStrokePreparation.PrepareWithPointKeys(inputs,profile,height,metrics,cancellationToken);
            strokes.AddRange(smoothed.Strokes);foreach(int key in smoothed.PointKeys200.Span)keys.Add(checked((uint)(key+offset*200)));
            offset+=inputs.Sum(s=>s.Points.Count);next=last+1;
            if(keys.Count>4096)throw new InvalidDataException("Bounded full265 output exceeded.");
        }
        if(next!=points.Length||strokes.Count!=points.Length)throw new ArgumentException("Incomplete owned physical265 coverage.");
        var chronology=line.Chronology.RemapPreparedPoints(strokes.Select(s=>s.Points.Count).ToArray(),keys.ToArray(),200,cancellationToken);
        return new(this,profile,metrics,strokes.ToArray(),keys.ToArray(),chronology);
    }
    private static int I(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
}

public sealed class LegacyFreshMainActiveGapSmoothed
{
    public LegacyFreshMainActiveGapPreparation Input {get;}
    public LegacyOrientationSmoothingProfile Profile {get;}
    public LegacyOrientationStrokeMetrics OriginalSourceMetrics {get;}
    public IReadOnlyList<InkStroke> Strokes {get;}
    public LegacySourcePointChronology Chronology {get;}
    public float SourceGap=>Input.Output.SourceGap;
    private readonly uint[] keys;
    public ReadOnlyMemory<uint> InputPointKeys200=>keys.ToArray();
    internal LegacyFreshMainActiveGapSmoothed(LegacyFreshMainActiveGapPreparation input,LegacyOrientationSmoothingProfile profile,
        LegacyOrientationStrokeMetrics metrics,InkStroke[] strokes,uint[] keys,LegacySourcePointChronology chronology)
    {Input=input;Profile=profile;OriginalSourceMetrics=metrics;Strokes=Array.AsReadOnly(strokes);this.keys=keys;Chronology=chronology;}
}

public sealed record LegacyNativeFreshGapResult(LegacyOrdinaryGapResult Output,LegacyOrdinaryGapMixtureResult? Mixture,
    IReadOnlyList<float> QualifyingGaps,float Blend);

/// <summary>175350 fresh-source unknown720 item branch (+10=-1). Restores
/// actual qualifying observation production, all fixed-variance EM retries and
/// blended9C output. Reused recognized-input physical gate remains excluded.</summary>
public static class LegacyNativeFreshGapPreparation
{
    public static LegacyNativeFreshGapResult Apply(LegacyOrdinaryGapProfile profile,LegacyOrdinaryGapMixtureProfile gaussian,
        IReadOnlyList<ReadOnlyMemory<byte>> descriptors,IReadOnlyList<ReadOnlyMemory<byte>> guides,
        IReadOnlyList<LegacyFirstResultLineRow> rows,float incomingSourceGap,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(profile);ArgumentNullException.ThrowIfNull(gaussian);ArgumentNullException.ThrowIfNull(descriptors);
        ArgumentNullException.ThrowIfNull(guides);ArgumentNullException.ThrowIfNull(rows);profile.Validate();gaussian.Validate();
        cancellationToken.ThrowIfCancellationRequested();
        if(descriptors.Count is <1 or >512||guides.Count>512||rows.Count>4096||!float.IsFinite(incomingSourceGap))
            throw new ArgumentException("Bounded fresh720/guide owner required.");
        var intervals=new List<(int First,int End,float Height)>();var used=new bool[rows.Count];int total=0,qualified=0;
        foreach(var memory in descriptors)
        {
            var b=memory.Span;if(b.Length!=88)throw new ArgumentException("Complete88-byte descriptor required.");
            int kind=BinaryPrimitives.ReadUInt16LittleEndian(b)&15;if(kind==3)continue;
            if(kind is not(0 or 6))throw new NotSupportedException("Ordinary fresh720 descriptor required.");
            int line=I(b,16),first=I(b,52),end=I(b,56);
            if(line<0||line>=guides.Count||guides[line].Length!=40||first<0||end<=first||end>rows.Count)
                throw new ArgumentException("Assigned actual nonempty source720 interval required.");
            float height=F(guides[line].Span,4);if(!float.IsFinite(height)||height<=0)throw new ArgumentException("Finite positive guide required.");
            int n=end-first-1;total=checked(total+n);if(n>profile.MinimumGaps)qualified=checked(qualified+n);
            for(int r=first;r<end;r++)
            {
                if(used[r]||rows[r] is null)throw new ArgumentException("Disjoint complete source720 intervals required.");used[r]=true;
                var row=rows[r].CopyStorage().Span;
                if(I(row,16)!=-1)throw new NotSupportedException("Recognized-input720 source item requires actual physical category/nibble gate.");
                float left=F(row,24),right=F(row,28);
                if(!float.IsFinite(left)||!float.IsFinite(right)||right<left||!float.IsFinite((left+right)*.5f))throw new ArgumentException("Finite720 horizontal bounds required.");
            }
            intervals.Add((first,end,height));
        }
        float blend=Math.Clamp(((float)qualified-profile.MinimumGaps)/(profile.MaximumGaps-profile.MinimumGaps),0,1);
        if(blend==0)
        {
            var simple=LegacyNativeOrdinaryGapPreparation.ApplyWithoutMixture(profile,descriptors,guides,rows,incomingSourceGap,cancellationToken);
            return new(simple,null,Array.Empty<float>(),blend);
        }
        var all=new List<float>(total);var eligible=new List<float>(qualified);var sorted=new List<int[]>();
        foreach(var interval in intervals)
        {
            var order=Enumerable.Range(interval.First,interval.End-interval.First).Select(r=>(Center:(F(rows[r].CopyStorage().Span,24)+F(rows[r].CopyStorage().Span,28))*.5f,Row:r)).ToArray();
            if(order.Length>1)Sort(order,0,order.Length-1);sorted.Add(order.Select(v=>v.Row).ToArray());
            float frontier=-float.MaxValue;bool qualifying=order.Length-1>profile.MinimumGaps;
            for(int r=0;r<order.Length;r++)
            {
                cancellationToken.ThrowIfCancellationRequested();var b=rows[order[r].Row].CopyStorage().Span;float right=F(b,28);
                if(r!=0)
                {
                    float distance=(F(b,24)-frontier)/interval.Height;
                    if(!float.IsFinite(distance))throw new InvalidDataException("Native gap overflow.");
                    float gap=(float)Math.Clamp((double)distance,0,4);all.Add(gap);if(qualifying)eligible.Add(gap);
                }
                if(frontier<=right)frontier=right;
            }
        }
        if(all.Count!=total||eligible.Count!=qualified)throw new InvalidDataException("Native qualifying gap cardinality differs.");
        var mixture=LegacyNativeOrdinaryGapMixture.Fit(gaussian,eligible.ToArray(),cancellationToken);
        double sum=0;foreach(float x in all)sum+=x;double mean=sum/all.Count,variance=0;
        foreach(float x in all){double d=x-mean;variance+=d*d;}variance/=all.Count;
        bool assigned=all.Count>=profile.MinimumGaps||variance>=(double).1f;float output=incomingSourceGap;
        if(assigned)
        {
            double ordinary=mean+((double)profile.MeanOffset-Math.Sqrt(variance)*(double)profile.StandardDeviationMultiplier);
            double blended=(double)(1-blend)*ordinary+(double)((float)mixture.Intersection*blend);
            output=(float)Math.Min((double)profile.MaximumGap,blended);
            if(!float.IsFinite(output))throw new InvalidDataException("Native active29B output overflow.");
        }
        return new(new(output,assigned,all.ToArray(),sorted.ToArray(),mean,variance),mixture,Array.AsReadOnly(eligible.ToArray()),blend);
        void Sort((float Center,int Row)[] values,int first,int last)
        {
            do
            {
                float pivot=values[(first+last)>>1].Center;int a=first,b=last;
                do{while(values[a].Center<pivot)a++;while(pivot<values[b].Center)b--;if(b<a)break;(values[a],values[b])=(values[b],values[a]);a++;b--;}while(a<=b);
                if(first<b)Sort(values,first,b);first=a;
            }while(last>first);
        }
    }
    private static int I(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
    private static float F(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadSingleLittleEndian(b[at..]);
}
