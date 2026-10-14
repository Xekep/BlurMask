using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>29B/v100 serialized profile, including zero-gap output and
/// mean/variance correction. The two-Gaussian EM branch remains separate.</summary>
public sealed record LegacyOrdinaryGapProfile(byte MinimumGaps,byte MaximumGaps,float ZeroGap,float MeanOffset,
    float StandardDeviationMultiplier,float MaximumGap)
{
    public static LegacyOrdinaryGapProfile ReadV100Node(ReadOnlySpan<byte> resource,int offset)
    {
        if(offset<0||offset>resource.Length-58||I(resource,offset)!=0x101fe90||I(resource,offset+4)!=50||I(resource,offset+8)!=0x29b||
            BinaryPrimitives.ReadUInt16LittleEndian(resource[(offset+12)..])!=0||BinaryPrimitives.ReadUInt16LittleEndian(resource[(offset+14)..])!=0x100)
            throw new InvalidDataException("Complete unencoded29B/v100 node required.");
        for(int i=0;i<6;i++)if(!float.IsFinite(F(resource,offset+16+i*4)))throw new InvalidDataException("Finite29B Gaussian profile required.");
        var p=new LegacyOrdinaryGapProfile(resource[offset+40],resource[offset+41],F(resource,offset+42),F(resource,offset+46),F(resource,offset+50),F(resource,offset+54));
        p.Validate();return p;
    }
    internal void Validate()
    {if(MinimumGaps>=MaximumGaps||!float.IsFinite(ZeroGap)||!float.IsFinite(MeanOffset)||!float.IsFinite(StandardDeviationMultiplier)||
        StandardDeviationMultiplier<0||!float.IsFinite(MaximumGap))throw new ArgumentException("Finite29B bounded profile required.");}
    private static int I(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
    private static float F(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadSingleLittleEndian(b[at..]);
}

public sealed class LegacyOrdinaryGapResult
{
    public float SourceGap {get;}
    public bool Assigned {get;}
    public IReadOnlyList<float> NormalizedGaps {get;}
    public IReadOnlyList<ReadOnlyMemory<int>> SortedRowIndices {get;}
    public double Mean {get;}
    public double Variance {get;}
    internal LegacyOrdinaryGapResult(float gap,bool assigned,float[] observations,int[][] sorted,double mean,double variance)
    {SourceGap=gap;Assigned=assigned;NormalizedGaps=Array.AsReadOnly(observations);
        SortedRowIndices=Array.AsReadOnly(sorted.Select(v=>new ReadOnlyMemory<int>(v)).ToArray());Mean=mean;Variance=variance;}
}

/// <summary>Native175350 ordinary no-EM branch: center-sort175260 -> clamped
/// forward-frontier gaps -> double population mean/variance -> conditional9C
/// publication. Refuses an active Gaussian-mixture blend rather than replacing
/// it with zero/default gap. Incoming9C remains explicit for prepared callers.</summary>
public static class LegacyNativeOrdinaryGapPreparation
{
    public static LegacyOrdinaryGapResult ApplyWithoutMixture(LegacyOrdinaryGapProfile profile,
        IReadOnlyList<ReadOnlyMemory<byte>> descriptors,IReadOnlyList<ReadOnlyMemory<byte>> guides,
        IReadOnlyList<LegacyFirstResultLineRow> rows,float incomingSourceGap,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(profile);ArgumentNullException.ThrowIfNull(descriptors);ArgumentNullException.ThrowIfNull(guides);
        ArgumentNullException.ThrowIfNull(rows);profile.Validate();cancellationToken.ThrowIfCancellationRequested();
        if(descriptors.Count is <1 or >512||guides.Count>512||rows.Count>4096||!float.IsFinite(incomingSourceGap))
            throw new ArgumentException("Bounded finite current source29B required.");
        var intervals=new List<(int First,int End,float Height)>();var used=new bool[rows.Count];int gaps=0,qualified=0;
        foreach(var memory in descriptors)
        {
            var d=memory.Span;if(d.Length!=88)throw new ArgumentException("Complete88-byte descriptors required.");
            int kind=BinaryPrimitives.ReadUInt16LittleEndian(d)&15;if(kind is not (0 or 6)){if(kind!=3)throw new NotSupportedException("Ordinary/separator descriptors required.");continue;}
            int line=I(d,16),start=I(d,52),end=I(d,56);
            if(line<0||line>=guides.Count||guides[line].Length!=40||start<0||end<=start||end>rows.Count)
                throw new ArgumentException("Assigned nonempty actual720 descriptor intervals required.");
            float height=F(guides[line].Span,4);if(!float.IsFinite(height)||height<=0)throw new ArgumentException("Positive current guide height required.");
            int n=end-start-1;gaps=checked(gaps+n);if(n>profile.MinimumGaps)qualified=checked(qualified+n);
            for(int r=start;r<end;r++)
            {
                if(used[r])throw new ArgumentException("720 interval overlap.");used[r]=true;
                if(rows[r] is null)throw new ArgumentException("720 row missing.");var b=rows[r].CopyStorage().Span;
                float l=F(b,24),right=F(b,28);if(!float.IsFinite(l)||!float.IsFinite(right)||!float.IsFinite((right+l)*.5f)||right<l)
                    throw new ArgumentException("Finite720 horizontal bounds required.");
            }
            intervals.Add((start,end,height));
        }
        if(qualified>profile.MinimumGaps)throw new NotSupportedException("Actual29B nonzero mixture blend requires the native two-Gaussian EM producer.");
        if(gaps==0)return new(profile.ZeroGap,true,[],[],0,0);
        var sorted=new List<int[]>();var observations=new List<float>(gaps);
        foreach(var interval in intervals)
        {
            var order=Enumerable.Range(interval.First,interval.End-interval.First).Select(r=>(Center:(F(rows[r].CopyStorage().Span,24)+F(rows[r].CopyStorage().Span,28))*.5f,Row:r)).ToArray();
            if(order.Length>1)Sort(order,0,order.Length-1);sorted.Add(order.Select(v=>v.Row).ToArray());
            float frontier=-float.MaxValue;
            for(int i=0;i<order.Length;i++)
            {
                cancellationToken.ThrowIfCancellationRequested();var b=rows[order[i].Row].CopyStorage().Span;float right=F(b,28);
                if(i!=0)
                {
                    float difference=(F(b,24)-frontier)/interval.Height;
                    if(!float.IsFinite(difference))throw new InvalidDataException("Native29B normalized gap overflow.");
                    observations.Add((float)Math.Clamp((double)difference,0,4));
                }
                if(frontier<=right)frontier=right;
            }
        }
        if(observations.Count!=gaps)throw new InvalidDataException("Native29B gap cardinality differs.");
        double sum=0;foreach(float value in observations)sum+=value;double mean=sum/observations.Count,variance=0;
        foreach(float value in observations){double d=value-mean;variance+=d*d;}variance/=observations.Count;
        if(observations.Count<profile.MinimumGaps&&variance<(double).1f)
            return new(incomingSourceGap,false,observations.ToArray(),sorted.ToArray(),mean,variance);
        double adjusted=mean+((double)profile.MeanOffset-Math.Sqrt(variance)*(double)profile.StandardDeviationMultiplier);
        float result=(float)Math.Min((double)profile.MaximumGap,adjusted);
        if(!float.IsFinite(result))throw new InvalidDataException("Native29B gap publication overflow.");
        return new(result,true,observations.ToArray(),sorted.ToArray(),mean,variance);
        void Sort((float Center,int Row)[] values,int first,int last)
        {
            //175260 swaps full8-byte(center,rowID) records, including ties.
            do
            {
                float pivot=values[(first+last)>>1].Center;int a=first,b=last;
                do
                {
                    while(values[a].Center<pivot)a++;while(pivot<values[b].Center)b--;
                    if(b<a)break;(values[a],values[b])=(values[b],values[a]);a++;b--;
                }while(a<=b);
                if(first<b)Sort(values,first,b);first=a;
            }while(last>first);
        }
    }
    private static int I(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
    private static float F(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadSingleLittleEndian(b[at..]);
}
