using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;
if(args.Length==3&&args[0]=="--prepared")
{
    using var fixtures=JsonDocument.Parse(File.ReadAllBytes(args[1]));var results=new List<object>();
    var configuration=LegacyNativeWordControllerConfiguration.CreateFresh20baa0()with{PackedSource22e8=3,PrimaryArcLimit230a=125,PrimaryAlternativeCount2402=2,SecondaryAlternativeCount2404=1};
    foreach(var f in fixtures.RootElement.GetProperty("Fixtures").EnumerateArray())
    {
        var slots=Convert.FromHexString(f.GetProperty("Slots").GetString()!);var pool=new LegacyNativeWordAlternativeBuffer(2,Convert.FromHexString(f.GetProperty("Pool").GetString()!));
        int active=f.GetProperty("Active").GetInt32();
        var ordered=LegacyNativeWordAlternativeOrdering.Prepare(slots,active,false,configuration,[0,0xc0e],false,pool,null);
        var rebuilt=ordered.Heap.GetSlotStorageSnapshot();
        var limits=new LegacyNativeWordHeapQuotaLimits(uint.MaxValue,uint.MaxValue,250,uint.MaxValue,uint.MaxValue,124,-1);
        var descriptor=new LegacyNativeWordHeapDescriptor(rebuilt.Span,slots.Length/128,ordered.Heap.Count,0,limits);
        if(!LegacyNativeWordQuotaController.Apply([descriptor],configuration,0,0))throw new Exception("Prepared phase0 quota failure.");
        var retained=descriptor.GetBackingStorageSnapshot();var rows=new List<object>();
        for(int i=0;i<descriptor.Count;i++)
        {
            var slot=retained.Span.Slice(i*128,128);var row=LegacyNativeWordRowMaterializer.Materialize(slot,(ushort)i,125,2,1,ordered.PrimaryAlternativeStorage.Span);
            rows.Add(JsonOutput.D(("Record",Hex(row.Record)),("Primary",Hex(row.PrimaryAlternatives)),("Ordinal",BinaryPrimitives.ReadInt32LittleEndian(slot[88..]))));
        }
        results.Add(JsonOutput.D(("Name",f.GetProperty("Name").GetString()),("Sorted",Hex(ordered.SortedSlotStorage)),("Compacted",Hex(ordered.CompactedSlotStorage)),("Pool",Hex(ordered.PrimaryAlternativeStorage)),("Unique",ordered.UniqueCount),
            ("Rebuilt",Hex(rebuilt)),("Retained",Hex(retained)),("Count",descriptor.Count),("Rows",rows)));
    }
    var contextControls=new List<object>();
    foreach(int n in new[]{0,1,125,126,217})
    {
        var contexts=new LegacySymbolContextFactory(2048);var marker=contexts.ResolveInitial(0xfffa).Identity;
        var slots=new byte[n*128];var residue=Enumerable.Range(0,n*80).Select(i=>(byte)(i*7+11)).ToArray();
        for(int i=0;i<n;i++)
        {
            var child=contexts.ResolveChild(marker,(ushort)i);if(child.Created)contexts.LinkCreatedContext(child.Identity,marker);
            var s=slots.AsSpan(i*128,128);BinaryPrimitives.WriteUInt64LittleEndian(s[8..],child.Identity);
            BinaryPrimitives.WriteInt32LittleEndian(s[16..],i*3);BinaryPrimitives.WriteInt32LittleEndian(s[20..],-i);
            BinaryPrimitives.WriteUInt16LittleEndian(s[30..],65535);s[32]=0;s[33]=(byte)(i&31);s[38]=(byte)i;
            BinaryPrimitives.WriteInt32LittleEndian(s[88..],i);
        }
        var beforeNodes=contexts.CopyNodes().ToArray();
        var result=LegacyNativeRootWordAlternativeProducer.Create(slots,n,0,configuration,contexts,static x=>x,residue,[]);
        var nodes=contexts.CopyNodes();var canonical=nodes.Select((x,i)=>(x.Identity,Ordinal:(ulong)i+1)).ToDictionary(x=>x.Identity,x=>x.Ordinal);
        var bytes=result.PrimaryStorage.ToArray();
        for(int i=0;i<n;i++)foreach(int off in new[]{8,16})
        {var s=bytes.AsSpan(i*80+off);BinaryPrimitives.WriteUInt64LittleEndian(s,canonical[BinaryPrimitives.ReadUInt64LittleEndian(s)]);}
        var canonicalSlots=slots.ToArray();for(int i=0;i<n;i++){var s=canonicalSlots.AsSpan(i*128+8);BinaryPrimitives.WriteUInt64LittleEndian(s,canonical[BinaryPrimitives.ReadUInt64LittleEndian(s)]);}
        long[] Node(LegacySymbolContextNode x)=>[(long)canonical[x.Identity],x.ParentLink==0?0:(long)canonical[x.ParentLink],x.ChildTreeRoot==0?0:(long)canonical[x.ChildTreeRoot],x.Left==0?0:(long)canonical[x.Left],x.Right==0?0:(long)canonical[x.Right],x.Symbol,x.Balance,x.PageIndex,x.PageSlot];
        contextControls.Add(JsonOutput.D(("N",n),("Nodes",contexts.NodeCount),("Primary",Convert.ToHexStringLower(bytes)),("Input",Convert.ToHexStringLower(canonicalSlots)),("BeforeFields",beforeNodes.Select(Node).ToArray()),("AfterFields",nodes.Select(Node).ToArray()),("Paths",nodes.Select(x=>new ulong[]{canonical[x.Identity],x.ParentLink==0?0:canonical[x.ParentLink],x.Symbol}).ToArray())));
    }
    JsonOutput.Save(args[2],JsonOutput.D(("Scope","PREPARED EXISTING KERNELS ONLY; new owner never invoked"),("Results",results),("ContextControls",contextControls)));
    Console.WriteLine($"Prepared generic controls {results.Count}; root/context controls {contextControls.Count}; new owner not invoked.");
}
else if(args.Length==4&&args[0]=="--owner")
{
    //Compile-only until root approves this exact owning execution and corpus.
    OwnerCapture.Run(args[1],args[2],args[3]);
}
else throw new ArgumentException("--prepared fixtures output OR separately approved --owner models raw-input output");
static string Hex(ReadOnlyMemory<byte> value)=>Convert.ToHexStringLower(value.Span);
