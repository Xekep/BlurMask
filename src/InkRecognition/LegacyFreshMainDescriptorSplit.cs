using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Actual fresh15B260 descriptor publication and16AA00 physical/point
/// reordering before first249 and later15F050. Provider metadata is derived from
/// the owned unknown-line2E3 result, not caller strings or guessed row IDs.
/// No later physical insertion, line guide, first winner or main decoding is added.</summary>
public sealed class LegacyFreshMainDescriptorSplit
{
    private readonly byte[][] descriptors;
    private readonly InkStroke[] strokes;
    private readonly int[] originalPhysical;
    private readonly float[] probabilities;
    public LegacyFreshMainPhysicalGrouping Input {get;}
    public IReadOnlyList<InkStroke> Strokes {get;}
    public ReadOnlyMemory<int> OriginalPhysicalStrokeIndices=>originalPhysical;
    public ReadOnlyMemory<float> SourceStrokeProbabilities=>probabilities;
    public LegacySourcePointChronology Chronology {get;}
    public int DescriptorCount=>descriptors.Length;
    public int OrdinaryDescriptorCount=>(descriptors.Length+1)/2;
    public byte[][] CopyDescriptors()=>descriptors.Select(d=>d.ToArray()).ToArray();
    private LegacyFreshMainDescriptorSplit(LegacyFreshMainPhysicalGrouping input,byte[][] descriptors,
        InkStroke[] strokes,int[] originalPhysical,float[] probabilities,LegacySourcePointChronology chronology)
    {Input=input;this.descriptors=descriptors;this.strokes=strokes;Strokes=Array.AsReadOnly(strokes);
        this.originalPhysical=originalPhysical;this.probabilities=probabilities;Chronology=chronology;}

    /// <summary>Fresh source has no existing strings/source720/page guide/prior
    /// source. Provider98 is null and knownCount zero, so07A850 supplies exact
    /// group metadata20=INT_MIN,24=?1,28=1. No opaque provider metadata is assumed.
    /// Physical source keys survive the actual ascending-ID group reorder.</summary>
    public static LegacyFreshMainDescriptorSplit Prepare(LegacyFreshMainPhysicalGrouping actualGrouping,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualGrouping);cancellationToken.ThrowIfCancellationRequested();
        var packet=actualGrouping.Packet;var source=packet.Source;var selected=actualGrouping.Grouping.Selected;
        if(!ReferenceEquals(actualGrouping.Scale.Source,source)||!ReferenceEquals(actualGrouping.Grouping.Source,actualGrouping.Scale)||
            packet.DescriptorCount!=1||packet.Descriptor.Flags!=0x10||packet.Descriptor.CurrentLineId!=-1||
            packet.Descriptor.FirstPhysicalStroke!=0||packet.Descriptor.LastPhysicalStroke!=source.Strokes.Count-1)
            throw new ArgumentException("Actual owned fresh main direct-source grouping required.");
        int count=source.Strokes.Count,groups=selected.Groups.Count;
        if(groups<1||groups>count||selected.StrokeGroupIndices.Length!=count)
            throw new InvalidDataException("Actual unknown-line groups must cover every physical stroke.");
        var visited=new bool[count];
        for(int g=0;g<groups;g++)foreach(int id in selected.Groups[g].Span)
        {
            if((uint)id>=(uint)count||visited[id]||selected.StrokeGroupIndices.Span[id]!=g)
                throw new InvalidDataException("Group ownership/membership disagrees with fresh source.");
            visited[id]=true;
        }
        if(visited.Any(v=>!v)||selected.Groups.Any(g=>g.Length==0))throw new InvalidDataException("Missing physical group membership.");
        byte[] initial=Initial(packet);byte[][] descriptors;int[] order;
        if(groups==1)
        {
            //15B260's single-provider-group path keeps physical ingress order;
            //it does not sort that group's returned vector or split descriptors.
            order=Enumerable.Range(0,count).ToArray();WI(initial,24,0x02000002);
            WI(initial,4,int.MinValue);WI(initial,8,count==1?source.Strokes[0].Points.Count:-1);initial[72]=1;
            descriptors=[initial];
        }
        else
        {
            descriptors=new byte[groups*2-1][];var physicalOrder=new List<int>(count);byte[] current=initial;
            for(int g=0;g<groups;g++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int[] ids=selected.Groups[g].ToArray();Array.Sort(ids);
                WI(current,24,0x02000003);ushort flags=BinaryPrimitives.ReadUInt16LittleEndian(current);
                BinaryPrimitives.WriteUInt16LittleEndian(current,(ushort)((flags&0xffef)|(g==0?0x10:0)));
                WI(current,36,physicalOrder.Count);physicalOrder.AddRange(ids);WI(current,40,physicalOrder.Count-1);
                WI(current,4,int.MinValue);WI(current,8,-1);current[72]=1;current.AsSpan(76,8).Fill(255);
                descriptors[g*2]=current;
                if(g+1<groups)
                {
                    //1598E0 zeroes new88-byte storage.15B260 initializes an
                    //intermediate kind3 separator, then copies the entire current
                    //ordinary descriptor into the next ordinary slot.
                    byte[] separator=new byte[88];WI(separator,24,0x02000003);
                    WI(separator,32,-1);WI(separator,36,-1);
                    BinaryPrimitives.WriteUInt16LittleEndian(separator,3);
                    BinaryPrimitives.WriteSingleLittleEndian(separator.AsSpan(40),.5f);
                    WI(separator,16,-1);WI(separator,20,-1);WI(separator,4,int.MinValue);WI(separator,8,-1);
                    descriptors[g*2+1]=separator;current=current.ToArray();
                }
            }
            order=physicalOrder.ToArray();
        }
        //16AA00 copies all28-byte physical records in this exact order and
        //builds old-global-point*source128 ticks;16A130's source factor is200.
        var strokes=order.Select(i=>new InkStroke(source.Strokes[i].Points.ToArray())).ToArray();
        var probabilities=order.Select(i=>source.SourceStrokeProbabilities.Span[i]).ToArray();
        int[] counts=source.Strokes.Select(s=>s.Points.Count).ToArray();
        var keys=new List<LegacySourcePointKey>();foreach(int i in Enumerable.Range(0,count))
            for(int p=0;p<counts[i];p++)keys.Add(source.Chronology.Resolve(i,p).Key);
        var original=LegacySourcePointChronology.CreatePrepared(counts,keys.ToArray(),Enumerable.Range(0,keys.Count).ToArray(),sourceFactor:200);
        int[] starts=new int[count];for(int i=1;i<count;i++)starts[i]=starts[i-1]+counts[i-1];
        uint[] incoming=order.SelectMany(i=>Enumerable.Range(starts[i],counts[i]).Select(v=>(uint)v)).ToArray();
        var chronology=original.RemapPreparedPoints(strokes.Select(s=>s.Points.Count).ToArray(),incoming,1,cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();return new(actualGrouping,descriptors,strokes,order,probabilities,chronology);
    }

    /// <summary>Native descriptors24/28 are current physical ranges after the
    /// source reorder. Separators are not ink owners and are never returned.</summary>
    public int GetDescriptorOrdinalForPhysicalStroke(int physicalStroke)
    {
        if((uint)physicalStroke>=(uint)strokes.Length)throw new ArgumentOutOfRangeException(nameof(physicalStroke));
        for(int d=0;d<descriptors.Length;d+=2)
            if(physicalStroke>=BinaryPrimitives.ReadInt32LittleEndian(descriptors[d].AsSpan(36))&&
                physicalStroke<=BinaryPrimitives.ReadInt32LittleEndian(descriptors[d].AsSpan(40)))return d;
        throw new InvalidDataException("Physical stroke lacks ordinary descriptor ownership.");
    }
    private static byte[] Initial(LegacyFreshOrdinaryRawPacket packet)
    {
        var b=new byte[88];BinaryPrimitives.WriteUInt16LittleEndian(b,packet.Descriptor.Flags);
        WI(b,4,int.MinValue);foreach(int at in new[]{8,12,16,20,24,32})WI(b,at,-1);
        WI(b,36,0);WI(b,40,packet.PhysicalStrokeCount-1);
        BinaryPrimitives.WriteSingleLittleEndian(b.AsSpan(44),packet.Descriptor.FirstX);
        BinaryPrimitives.WriteSingleLittleEndian(b.AsSpan(48),packet.Descriptor.FirstY);b[72]=1;b.AsSpan(76,8).Fill(255);return b;
    }
    private static void WI(Span<byte> b,int at,int value)=>BinaryPrimitives.WriteInt32LittleEndian(b[at..],value);
}
