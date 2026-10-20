using System.Collections;
using System.Text.Json;
using InkRecognition;

//Prepared diagnostics are kept outside all actual thirteen inference captures.
internal static class GeometryGuards
{
    internal static byte[] Capture(Func<InkStroke[],LegacyLibraryGlobalTextPublicationScope> scopeFactory,
        Func<LegacyLibraryGlobalTextPublicationScope,LegacyFreshMainGlobalWordTextPublication> parentFactory,
        Func<LegacyLibraryGlobalTextPublicationScope,LegacyPhysicalRawInkIngress> ingressOf)
    {
        int guards=0;
        void Reject(Action action)
        {
            try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OverflowException or OperationCanceledException){guards++;return;}
            throw new Exception("Expected prepared geometry refusal.");
        }
        InkStroke Stroke()=>new(new InkPoint[]{new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)});
        LegacyPhysicalRawInkIngress Uniform(float rate=1000,long start=0)=>LegacyPhysicalRawInkIngress.Create([Stroke()],[LegacyPhysicalStrokeConstructor.Uniform(rate,start)]);
        foreach(float rate in new[]{float.NaN,float.PositiveInfinity,0f,-1f,float.Epsilon,float.MaxValue})Reject(()=>Uniform(rate));
        Reject(()=>Uniform(1000,LegacyPhysicalRawInkTiming.MaximumAbsoluteMicroseconds));
        Reject(()=>Uniform(1000,long.MinValue));Reject(()=>Uniform(1000,long.MaxValue));
        Reject(()=>LegacyPhysicalRawInkIngress.Create([Stroke()],[]));
        Reject(()=>LegacyPhysicalRawInkIngress.Create([],[]));
        Reject(()=>LegacyPhysicalRawInkIngress.Create([new(new InkPoint[]{new(float.NaN,0)})],[LegacyPhysicalStrokeConstructor.Uniform(1000,0)]));
        Reject(()=>LegacyPhysicalRawInkIngress.Create([Stroke()],[LegacyPhysicalStrokeConstructor.MeasuredMilliseconds(1000,0,[0,1])]));
        Reject(()=>LegacyPhysicalRawInkIngress.Create([Stroke()],[LegacyPhysicalStrokeConstructor.MeasuredMilliseconds(1000,0,[0,1,0,2,3])]));
        Reject(()=>LegacyPhysicalRawInkIngress.Create([Stroke()],[LegacyPhysicalStrokeConstructor.Uniform(1000,0)],new(true)));
        Reject(()=>LegacyPhysicalStrokeConstructor.MeasuredMilliseconds(1000,0,new ChangingCount()));
        //A valid custom IReadOnlyList is never enumerated by bounded ingestion.
        _=LegacyPhysicalRawInkIngress.Create([new(new NoEnumeration<InkPoint>([new(0,0),new(1,1)]))],
            new NoEnumeration<LegacyPhysicalStrokeConstructor>([LegacyPhysicalStrokeConstructor.Uniform(1000,0)]));
        int[] channel=[-4,-3,-2,-1,0];var constructor=LegacyPhysicalStrokeConstructor.MeasuredMilliseconds(1000,-500,channel);channel[0]=999;
        var owned=LegacyPhysicalRawInkIngress.Create([Stroke()],[constructor]);
        if(owned.TimingReceipt.ReadOriginalSample(0,0)!=-4500)throw new Exception("Caller timing was not owned.");
        int[] copy=owned.TimingReceipt.CopyMeasuredMilliseconds(0)!;copy[0]=777;
        if(owned.TimingReceipt.ReadOriginalSample(0,0)!=-4500)throw new Exception("Copied channel modified actual receipt.");
        Reject(()=>owned.TimingReceipt.ReadOriginalSample(-1,0));Reject(()=>owned.TimingReceipt.ReadOriginalSample(0,5));
        Reject(()=>owned.TimingReceipt.CopySources(new(true)));
        var prefixes=new List<(int Boundary,bool Cancel,LegacyOrdinaryGeometrySnapshot Snapshot)>();
        foreach(bool cancel in new[]{false,true})for(int boundary=1;boundary<=6;boundary++)
        {
            using var scope=scopeFactory([Stroke()]);var parent=parentFactory(scope);parent.Publish(new byte[40],new byte[8160]);
            var cropped=LegacyGlobalOrdinaryCroppedSource.Prepare(parent);var ingress=ingressOf(scope);
            var phase=LegacyFreshMainGlobalOrdinaryGeometry.Prepare(parent,cropped,ingress.TimingReceipt);
            if(!ReferenceEquals(phase,LegacyFreshMainGlobalOrdinaryGeometry.Prepare(parent,cropped,ingress.TimingReceipt)))throw new Exception("Geometry owner identity changed.");
            var before=phase.CopySnapshot();if(before.Stage!="prepared"||before.Ready||before.Faulted)throw new Exception("Preclaim geometry changed.");
            Reject(()=>phase.Run(new(true)));Reject(()=>LegacyFreshMainGlobalOrdinaryGeometry.Prepare(parent,cropped,ingress.TimingReceipt,new(true)));
            if(phase.CopySnapshot().Faulted||phase.CopySnapshot().Stage!="prepared")throw new Exception("Preclaim cancellation consumed numerical ownership.");
            Reject(()=>phase.RunPrepared(boundary,cancel));var snapshot=phase.CopySnapshot();
            if(snapshot.Ready||!snapshot.Faulted)throw new Exception("Faulted prefix incorrectly became ready.");
            Reject(()=>phase.Run());prefixes.Add((boundary,cancel,snapshot));
            if(boundary==1&&!cancel)
            {
                var foreign=Uniform();Reject(()=>LegacyFreshMainGlobalOrdinaryGeometry.Prepare(parent,cropped,foreign.TimingReceipt));
                Reject(()=>foreign.TimingReceipt.ReadViewStart(cropped.Views[0]));
                scope.Dispose();Reject(()=>phase.CopySnapshot());Reject(()=>ingress.TimingReceipt.ReadViewStart(cropped.Views[0]));
                //Raw immutable ingress intentionally lives independently of an inference scope.
                if(ingress.TimingReceipt.ReadOriginalSample(0,1)!=1000)throw new Exception("Raw receipt lifetime changed with unrelated scope.");
            }
        }
        using(var scope=scopeFactory([Stroke()]))
        {
            var parent=parentFactory(scope);parent.Publish(new byte[40],new byte[8160]);var cropped=LegacyGlobalOrdinaryCroppedSource.Prepare(parent);
            var phase=LegacyFreshMainGlobalOrdinaryGeometry.Prepare(parent,cropped,ingressOf(scope).TimingReceipt);
            long outputId=-1;_=scope.SharedState.Selections.CreatePrepared(parent.OriginalWordSource.CopyOriginalKey(),0,ref outputId);
            Reject(()=>phase.Validate());Reject(()=>phase.Run());
        }
        using var output=new MemoryStream();using var w=new Utf8JsonWriter(output);w.WriteStartObject();w.WriteNumber("guards",guards);
        w.WriteStartArray("prefixes");foreach(var p in prefixes)
        {
            w.WriteStartObject();w.WriteNumber("boundary",p.Boundary);w.WriteBoolean("cancel",p.Cancel);
            w.WritePropertyName("snapshot");w.WriteRawValue(JsonSerializer.SerializeToUtf8Bytes(p.Snapshot,GeometryJson.Default.LegacyOrdinaryGeometrySnapshot));w.WriteEndObject();
        }w.WriteEndArray();
        w.WriteStartArray("fractionalControls");
        foreach(long first in new long[]{-4000,-1001,-1,0,1,1001,4000})foreach(long delta in new long[]{0,1,2,999,1000})foreach(float fraction in new[]{0f,.25f,.5f,.75f,MathF.BitDecrement(1f)})
        {
            long next=first+delta;long actual=LegacyPhysicalRawInkTiming.NativeFractionalStart(first,next,fraction);
            w.WriteStartArray();w.WriteNumberValue(first);w.WriteNumberValue(next);w.WriteNumberValue(BitConverter.SingleToInt32Bits(fraction));w.WriteNumberValue(actual);w.WriteEndArray();
        }w.WriteEndArray();
        w.WriteStartArray("periodControls");foreach(float rate in new[]{.001f,1f,3f,59.94f,120f,1000f,1_000_000f,2_000_000f})
        {w.WriteStartArray();w.WriteNumberValue(BitConverter.SingleToInt32Bits(rate));w.WriteNumberValue(LegacyPhysicalRawInkTiming.NativePeriod(rate));w.WriteEndArray();}w.WriteEndArray();
        w.WriteStartArray("weightedControls");for(int n=0;n<=29;n++)
        {
            var points=Enumerable.Range(0,n).Select(i=>new InkPoint((i%7-3)*.375f,(i%9-4)*.5f)).ToArray();var state=new LegacyOrdinaryGeometryScratch();
            LegacyGlobalOrdinaryGeometryKernels.WeightedMoments(state,points,.125f,.5f,1,0,default);
            w.WriteStartObject();w.WriteNumber("n",n);w.WriteNumber("count",state.Count);w.WriteNumber("sum",state.Sum);w.WriteNumber("sum2",state.Sum2);w.WriteNumber("min",state.Min);w.WriteNumber("max",state.Max);w.WriteEndObject();
        }w.WriteEndArray();
        w.WriteStartArray("gaussianControls");
        foreach(double left in new[]{-10.0,0.0,10.0})foreach(double right in new[]{-5.0,0.0,20.0})
        foreach(double sd1 in new[]{0.0,Math.ScaleB(1,-24),Math.ScaleB(1,-23),.125,1.0,3.0})
        foreach(double sd2 in new[]{0.0,Math.ScaleB(1,-24),Math.ScaleB(1,-23),.25,1.0,4.0})
        {
            w.WriteStartArray();w.WriteNumberValue(left);w.WriteNumberValue(sd1);w.WriteNumberValue(right);w.WriteNumberValue(sd2);
            w.WriteNumberValue(LegacyGlobalOrdinaryGeometryKernels.Gaussian(left,sd1,right,sd2));w.WriteEndArray();
        }w.WriteEndArray();
        w.WriteStartArray("endpointControls");
        foreach(uint metadata in new uint[]{0,0x1000000})foreach(uint selected in new uint[]{0,0x10000,0x40000,0x50000,0xc0000})
        foreach(double mean in new[]{-10.0,0.0,10.0})foreach(double sd in new[]{0.0,.125,1.0,5.0})
        {
            var state=new LegacyOrdinaryGeometryScratch{Mean=mean,Std=sd,Min=-2,Max=2};
            w.WriteStartArray();w.WriteNumberValue(metadata);w.WriteNumberValue(selected);w.WriteNumberValue(mean);w.WriteNumberValue(sd);
            w.WriteNumberValue(LegacyGlobalOrdinaryGeometryKernels.Left(state,2,metadata,selected));
            w.WriteNumberValue(LegacyGlobalOrdinaryGeometryKernels.Right(state,2,metadata,selected));w.WriteEndArray();
        }w.WriteEndArray();
        //Primitive prepared order controls: no synthetic source owner or production Ready receipt.
        uint random=0x74123;int Next(){random=unchecked(random*1664525+1013904223);return(int)(random>>8);}
        w.WriteStartArray("orderControls");for(int n=1;n<=12;n++)for(int sample=0;sample<12;sample++)
        {
            var rows=Enumerable.Range(0,n).Select(_=>new LegacyOrdinaryGeometryScratch{Mean=(Next()%2001-1000)/16.0,Std=Next()%81/32.0}).ToArray();
            int[] order=LegacyGlobalOrdinaryGeometryKernels.BuildOrdinaryOrder(rows,out bool reversed,default);
            w.WriteStartObject();w.WriteStartArray("rows");foreach(var row in rows){w.WriteStartArray();w.WriteNumberValue(row.Mean);w.WriteNumberValue(row.Std);w.WriteEndArray();}w.WriteEndArray();
            w.WriteBoolean("reversed",reversed);w.WriteStartArray("order");foreach(int i in order)w.WriteNumberValue(i);w.WriteEndArray();
            w.WriteStartArray("links");foreach(var row in rows){w.WriteStartArray();w.WriteNumberValue(row.Previous);w.WriteNumberValue(row.Next);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
        }w.WriteEndArray();
        w.WriteStartArray("preparedHorizontalControls");
        using(var scope=scopeFactory([Stroke()]))
        {
            var parent=parentFactory(scope);parent.Publish(new byte[40],new byte[8160]);var cropped=LegacyGlobalOrdinaryCroppedSource.Prepare(parent);
            var link=parent.WordSource.CopyCharacterLinks()[0];var source=link.Source;var record=source.CopyRecords()[link.Marker];
            using var foreignScope=scopeFactory([Stroke()]);var foreignParent=parentFactory(foreignScope);foreignParent.Publish(new byte[40],new byte[8160]);
            var foreignLink=foreignParent.WordSource.CopyCharacterLinks()[0];var foreignSource=foreignLink.Source;var foreignCropped=LegacyGlobalOrdinaryCroppedSource.Prepare(foreignParent);
            if(ReferenceEquals(source,foreignSource)||source.Node.Identity!=foreignSource.Node.Identity||!source.Node.CopyRanges16().AsSpan().SequenceEqual(foreignSource.Node.CopyRanges16()))
                throw new Exception("Prepared negative alias needs distinct real payload refs with equal numeric node IDs/keys.");
            foreach(string kind in new[]{"equal-threshold","strict-greater","no-eligible","zero-equality","reverse-insertion","alias-equal","alias-write","alias-reverse","distinct-equal-node-key"})foreach(bool fail in new[]{false,true})
            {
                float[][] xs=kind switch{"equal-threshold"or"alias-equal"=>[[-7,7],[-1,1]],"strict-greater"or"alias-write"or"distinct-equal-node-key"=>[[-7.25f,7.25f],[-1,1]],
                    "no-eligible"=>[[-1,1]],"zero-equality"=>[[0,0],[0,0],[0,0]],_=>[[9,11],[-17,-3]]};
                uint[] selected=kind switch{"no-eligible"=>[0x1000],"zero-equality"=>[0,0x1000,0],"reverse-insertion"or"alias-reverse"=>[0,0x1000],_=>[0x1000,0]};
                var states=xs.Select((points,i)=>
                {
                    bool foreign=kind=="distinct-equal-node-key"&&i==1;var actualLink=foreign?foreignLink:link;var actual=actualLink.Source;var row=actual.CopyRecords()[actualLink.Marker];
                    return new LegacyOrdinaryGeometryCharacterState
                    {Link=actualLink,Scalars=actual.CopyAssignedScalars(),Key=actual.Node.CopyRanges16(),Row=row.CopyStorage40(),Text=row.Text!,Utf8=row.Text!.ReadBytes(),
                     Initial=0x41000000,Flags=0x41000000,Selected=selected[i],Marker=actualLink.Marker,
                     Views=[new((foreign?foreignCropped:cropped).Views[0],points.Select(x=>new InkPoint(x,0)).ToArray(),i*1000)]};
                }).ToArray();
                bool aliases=kind.StartsWith("alias-",StringComparison.Ordinal);
                if(aliases||kind=="distinct-equal-node-key")
                {
                    var cells=new LegacyOrdinaryGeometryMetadataMap();
                    for(int i=0;i<states.Length;i++)
                    {
                        states[i].Metadata=cells.Get(states[i].Link.Source);states[i].Flags=0x41000000;
                    }
                    if(ReferenceEquals(states[0].Metadata,states[1].Metadata)!=aliases)throw new Exception("Phase metadata cell alias is not actual payload identity.");
                }
                var scratch=xs.Select(_=>new LegacyOrdinaryGeometryScratch()).ToArray();var boundaries=new LegacyOrdinaryGeometryBoundary[xs.Length+1];
                var writes=new List<(int Index,uint Before,uint After,string Stage)>();var stages=new List<string>();var lazyFlags=new List<uint[]>();float? width=null,space=null;bool reversed=false;
                try
                {
                    _=LegacyGlobalOrdinaryGeometryKernels.Horizontal(states,[],new(0,0,0,10,0),new(0,0,0,0,1,0,0,0,0,0,0,0,0),1,0,scratch,boundaries,
                        (index,flags,stage)=>{writes.Add((index,states[index].Flags,flags,stage));states[index].Flags=flags;},
                        value=>reversed=value,(a,b)=>{if(a is{} x)width=x;if(b is{} y)space=y;},
                        stage=>{stages.Add(stage);if(stage is"04DA20"or"04DD50")lazyFlags.Add(states.Select(s=>s.Flags).ToArray());if(fail&&stage=="04DA20")throw new PreparedKernelFailure();},default);
                    if(fail)throw new Exception("Prepared post-write failure was not reached.");
                }catch(PreparedKernelFailure)when(fail){}
                w.WriteStartObject();w.WriteString("kind",kind);w.WriteBoolean("failAfterWidth",fail);w.WriteBoolean("reversed",reversed);w.WriteBoolean("aliasesExactPayload",aliases);
                w.WriteNumber("meanWidth",width!.Value);w.WriteNumber("meanSpacing",space!.Value);
                w.WriteStartArray("points");foreach(var points in xs){w.WriteStartArray();foreach(float x in points)w.WriteNumberValue(x);w.WriteEndArray();}w.WriteEndArray();
                w.WriteStartArray("selected");foreach(uint flags in selected)w.WriteNumberValue(flags);w.WriteEndArray();
                w.WriteStartArray("effectiveMetadata");foreach(var state in states)w.WriteNumberValue(state.Flags);w.WriteEndArray();
                w.WriteStartArray("historicalInitialMetadata");foreach(var state in states)w.WriteNumberValue(state.Initial);w.WriteEndArray();
                w.WriteStartArray("lazyStageMetadata");foreach(var flags in lazyFlags){w.WriteStartArray();foreach(uint flag in flags)w.WriteNumberValue(flag);w.WriteEndArray();}w.WriteEndArray();
                w.WriteStartArray("boundaries");foreach(var b in boundaries){w.WriteStartArray();w.WriteNumberValue(b.Coordinate!.Value);w.WriteNumberValue(b.CharacterIndex!.Value);w.WriteEndArray();}w.WriteEndArray();
                w.WriteStartArray("characterMeans");foreach(var state in states)w.WriteNumberValue(state.Mean!.Value);w.WriteEndArray();
                w.WriteStartArray("stages");foreach(string stage in stages)w.WriteStringValue(stage);w.WriteEndArray();
                w.WriteStartArray("writes");foreach(var item in writes){w.WriteStartArray();w.WriteNumberValue(item.Index);w.WriteNumberValue(item.Before);w.WriteNumberValue(item.After);w.WriteStringValue(item.Stage);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
            }
        }w.WriteEndArray();w.WriteEndObject();w.Flush();return output.ToArray();
    }
    private sealed class NoEnumeration<T>(T[] items):IReadOnlyList<T>
    {
        public int Count=>items.Length;public T this[int index]=>items[index];
        public IEnumerator<T> GetEnumerator()=>throw new Exception("Unbounded caller enumeration attempted.");IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();
    }
    private sealed class ChangingCount:IReadOnlyList<int>
    {
        private int reads;public int Count=>++reads==1?2:3;public int this[int index]=>index;
        public IEnumerator<int> GetEnumerator()=>throw new Exception("Caller enumeration attempted.");IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();
    }
    private sealed class PreparedKernelFailure:Exception;
}
