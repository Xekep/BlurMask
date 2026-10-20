using System.Buffers.Binary;
namespace InkRecognition;

//Shared numeric kernels only. The public RU and EN owning adapters validate
//their respective resource/source identities before calling these methods.
internal sealed record LegacyFreshPhysicalBuffer(byte[][] Descriptors,InkStroke[] Strokes,
    int[] OriginalPhysical,float[] Probabilities,LegacySourcePointChronology Chronology);
internal sealed record LegacyFreshPhysicalOrder(LegacyFreshPhysicalBuffer Buffer,
    LegacyFreshMainPhysicalReceiverCall[] Calls);
internal static class LegacyFreshMainPhysicalKernels
{
    internal static LegacyFreshPhysicalBuffer Split(LegacyFreshOrdinaryRawPacket packet,
        LegacyUnknownLineGroups selected,CancellationToken cancellationToken)
    {
        var source=packet.Source;
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
        cancellationToken.ThrowIfCancellationRequested();return new(descriptors,strokes,order,probabilities,chronology);
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
    internal static LegacyFreshPhysicalOrder Reorder(LegacyFreshPhysicalBuffer input,
        LegacyOrdinaryLineScaleSnapshot scale,LegacyOrdinaryNeuralLineGrouping grouping,float physicalResolution,
        ReadOnlySpan<byte> spatial30Resource,ReadOnlySpan<byte> physical50Resource,CancellationToken cancellationToken)
    {
        var descriptors=input.Descriptors.Select(d=>d.ToArray()).ToArray();
        using var stream30=new MemoryStream(spatial30Resource.ToArray(),false);
        using var stream50=new MemoryStream(physical50Resource.ToArray(),false);
        var spatial=PreparedLineBoundaryClassifier.Load(stream30);var physical=PreparedLineBoundaryClassifier.Load(stream50);
        var order=new List<int>(input.Strokes.Length);var calls=new List<LegacyFreshMainPhysicalReceiverCall>();
        float carry=grouping.NeuralScale;
        for(int d=0;d<descriptors.Length;d++)
        {
            cancellationToken.ThrowIfCancellationRequested();var descriptor=descriptors[d];
            int kind=BinaryPrimitives.ReadUInt16LittleEndian(descriptor)&15;
            if(kind==3)continue;
            if(kind!=0||BinaryPrimitives.ReadInt32LittleEndian(descriptor.AsSpan(16))!=-1||
                BinaryPrimitives.ReadInt32LittleEndian(descriptor.AsSpan(32))!=-1||descriptor[72]!=1)
                throw new InvalidDataException("Actual fresh unknown horizontal descriptor owner required.");
            int first=BinaryPrimitives.ReadInt32LittleEndian(descriptor.AsSpan(36)),last=BinaryPrimitives.ReadInt32LittleEndian(descriptor.AsSpan(40));
            if(first<0||last<first||last>=input.Strokes.Length||order.Count!=first)
                throw new InvalidDataException("Actual contiguous split physical ranges required.");
            var points=Enumerable.Range(first,last-first+1).Select(i=>input.Strokes[i]).ToArray();
            if(first==last){order.Add(first);continue;}
            //A checked numeric receiver view: its metadata/probabilities are
            //NOT exported as the parent source. Native15F050 feeds this retained
            //buffer to2E3, without constructing a new source219 or source keys.
            //Reject if the reusable ingress helper would change any current point.
            var numeric=LegacyOrdinarySourceInk.Prepare(points,scale.Source.Profile,cancellationToken);
            for(int p=0;p<points.Length;p++)
                if(!numeric.Strokes[p].Points.SequenceEqual(points[p].Points))
                    throw new NotSupportedException("Current retained buffer requires a direct2E3 receiver view; secondary source filtering would change it.");
            var receiverScale=new LegacyOrdinaryLineScaleSnapshot(numeric,scale.Profile,ReadOnlyMemory<float>.Empty,
                new LegacyLineScales(carry,scale.Scales.Secondary));
            float initial=carry;
            var groups=LegacyOrdinaryNeuralLineInferenceFactory.InferGrouping(receiverScale,physicalResolution,
                new bool[points.Length],true,.8f,
                spatial,physical,5,.75f,cancellationToken);
            carry=groups.NeuralScale;
            //15F050 uses source+B0 primary scale for insertion tolerance,
            //not the carried/possibly re-estimated receiver neural scale.
            int[] local=LegacyNativeUnknownPhysicalIntervalOrder.Order(points,groups.Selected.Groups,
                scale.Scales.Primary,.25f,cancellationToken);
            order.AddRange(local.Select(i=>first+i));
            calls.Add(new(d,initial,carry,points.Length,groups.Selected.Groups.Count,groups.VerticalStripCount));
        }
        if(order.Count!=input.Strokes.Length||order.Distinct().Count()!=order.Count)
            throw new InvalidDataException("Native physical reorder lost/duplicated source membership.");
        int[] original=order.Select(i=>input.OriginalPhysical[i]).ToArray();
        var strokes=order.Select(i=>new InkStroke(input.Strokes[i].Points.ToArray())).ToArray();
        var probabilities=order.Select(i=>input.Probabilities[i]).ToArray();
        int[] counts=input.Strokes.Select(s=>s.Points.Count).ToArray();int[] starts=new int[counts.Length];
        for(int i=1;i<counts.Length;i++)starts[i]=starts[i-1]+counts[i-1];
        uint[] incoming=order.SelectMany(i=>Enumerable.Range(starts[i],counts[i]).Select(p=>(uint)p)).ToArray();
        var chronology=input.Chronology.RemapPreparedPoints(strokes.Select(s=>s.Points.Count).ToArray(),incoming,1,cancellationToken);
        return new(new(descriptors,strokes,original,probabilities,chronology),calls.ToArray());
    }
}

