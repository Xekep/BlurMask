using InkRecognition;
using System.Text.Json;

internal static class PreparedControls
{
    internal static void Run(string path)
    {
        //Only unchanged generic metadata/pool/graph kernels execute here.
        //No model, full FIRST owner, B7 Prepare/Run, target or NN is loaded.
        InkStroke[] raw=[new([new(0,0),new(0,0),new(1,1),new(2,1),new(2,1),new(3,2)]),new([new(10,2),new(11,3),new(11,3)])];
        var original=LegacyOrdinarySourceInk.Prepare(raw,new(0x135,false,0,0));
        var keys=original.Strokes.SelectMany((s,si)=>s.Points.Select((_,pi)=>original.Chronology.Resolve(si,pi).Key)).ToArray();
        var chronology=LegacySourcePointChronology.CreatePrepared([5,3],keys,Enumerable.Range(0,keys.Length).ToArray(),[0,100,200,400,600,800,900,1000],200);
        InkStroke[] prepared=[new([new(0,0),new(.5f,.5f),new(1,1),new(2,1),new(3,2)]),new([new(10,2),new(10.5f,2.5f),new(11,3)])];
        var ink=LegacyCursivePreparedInk.Create(prepared);LegacyCursiveCutMarker[] markers=[new(0,0,0),new(0,0,2),new(0,1,0)];
        var atoms=LegacyCursiveInkAtoms.Build([5,3],markers);var cuts=new LegacyCursiveCutResult(ink,markers,atoms);
        var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,20,new(false,1,0,0),[0,0],[-1,-1]);
        var graph=LegacyOrdinaryGraphSourceRanges.Prepare(seeds,chronology,original,7);
        var nodes=LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds);
        LegacyInkRange[][] paths=[ [new(0,0,3),new(0,2,5),new(1,0,3)], [new(1,0,3),new(0,2,5),new(0,0,3)],
            [new(0,1,2),new(0,2,3)], [new(0,3,5)], [new(1,1,3),new(0,0,2)] ];
        using var stream=File.Create(path);using var w=new Utf8JsonWriter(stream);w.WriteStartObject();w.WriteString("Scope","PREPARED unchanged generic source/pool only; no B7 owner execution");w.WriteBoolean("NNEvaluated",false);w.WriteBoolean("SDKExecuted",false);w.WriteBoolean("B7Executed",false);
        w.WriteStartObject("Source");SourceReceipt.Metadata(w,raw,original,chronology);
        w.WriteStartArray("Seeds");foreach(var s in seeds.Seeds){w.WriteStartObject();w.WriteNumber("Atom",s.SourceAtomIndex);w.WriteBoolean("Deferred",s.Deferred);w.WriteNumber("Displacement",s.InsertionDisplacement);w.WritePropertyName("Range");SourceReceipt.Ints(w,[s.Atom.Range.StrokeIndex,s.Atom.Range.StartPoint,s.Atom.Range.EndPointExclusive]);w.WriteEndObject();}w.WriteEndArray();
        w.WriteStartArray("SeedNodes");for(int n=0;n<nodes.NodeCount;n++){w.WriteStartArray();foreach(var a in nodes.GetNode(n)){w.WriteStartObject();w.WriteNumber("Group",a.SeedGroupIndex);w.WriteNumber("Start",a.StartContextId);w.WriteNumber("End",a.EndContextId);w.WritePropertyName("Succ");SourceReceipt.Ints(w,a.SuccessorArcIndices.ToArray());w.WriteEndObject();}w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
        w.WriteStartArray("ProjectionControls");foreach(var p in paths)
        {
            var result=LegacyOrdinaryGraphSourceRanges.ResolveOwnedRangePath(chronology,original,p,keys);
            w.WriteStartObject();w.WriteStartArray("Path");foreach(var r in p)SourceReceipt.Ints(w,[r.StrokeIndex,r.StartPoint,r.EndPointExclusive]);w.WriteEndArray();w.WriteStartArray("Ranges");foreach(var r in result)SourceReceipt.Ints(w,[r.First.First,r.First.Second,r.First.Third,r.Last.First,r.Last.Second,r.Last.Third]);w.WriteEndArray();w.WriteEndObject();
        }w.WriteEndArray();
        w.WriteStartArray("GraphQueries");for(int n=0;n<graph.Graph.NodeCount;n++)for(int ai=0;ai<graph.Graph.GetNode(n).Count;ai++)
        {var a=graph.Graph.GetNode(n)[ai];var count=graph.ResolveCandidate(n,ai);var fill=graph.ResolveCandidate(n,ai);if(!count.SequenceEqual(fill))throw new Exception("Generic count/fill changed");w.WriteStartObject();w.WriteNumber("First",a.StartContextId);w.WriteNumber("Last",a.EndContextId);w.WriteStartArray("Ranges");foreach(var r in fill)SourceReceipt.Ints(w,SourceReceipt.Range(r));w.WriteEndArray();w.WriteEndObject();}w.WriteEndArray();
        var storage=Enumerable.Range(0,40).Select(i=>(byte)(17+41*i)).ToArray();var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
        var first=pool.AppendDescriptor(0,10,false,storage);var before=pool.CopyRecord(first.Reference);var hit=pool.AppendDescriptor(0,10,false,new byte[40]);if(!hit.ExistingDescriptor||hit.Reference!=first.Reference||!pool.CopyRecord(first.Reference).SequenceEqual(before))throw new Exception("Generic interner hit changed row");
        var forced=pool.AppendDescriptor(0,10,true,storage);var again=pool.AppendDescriptor(0,10,false,storage);if(forced.ExistingDescriptor||again.ExistingDescriptor)throw new Exception("Generic NoDedup latch missing");
        w.WriteBase64String("Allocator",storage);w.WriteStartArray("PoolOwners");foreach(var o in pool.CopyOwnerNodes()){w.WriteStartObject();w.WriteNumber("Key",o.Key);w.WriteNumber("PackedCount",o.PackedCount);w.WriteBase64String("Records",o.Records.Span);w.WriteEndObject();}w.WriteEndArray();
        //Explicit native-SUBSS intermediate controls, NOT vendor execution.
        (string Label,float A,float B)[] precision=[("zero",0,0),("one-ulp",BitConverter.Int32BitsToSingle(0x3f800001),1),
            ("two-ulp",BitConverter.Int32BitsToSingle(0x3f800002),1),("three-ulp",BitConverter.Int32BitsToSingle(0x3f800003),1),
            ("negative-three-ulp",-BitConverter.Int32BitsToSingle(0x3f800003),-1),
            ("large-opposite-rounded",1000000,-999999.9375f),("negative-large-opposite",-1000000,999999.9375f),
            ("large-adjacent",1000000,999999.9375f),("subnormal",float.Epsilon,0),("signedzero",-0f,0)];
        w.WriteStartArray("RawPrecisionControls");foreach(var (label,a,b) in precision)
        {
            var check=LegacyOrdinarySourceInk.Prepare([new InkStroke([new(b,0),new(a,0)])],new(0x135,false,0,0));
            w.WriteStartObject();w.WriteString("Label",label);w.WriteNumber("A",a);w.WriteNumber("B",b);w.WriteNumber("ABits",BitConverter.SingleToInt32Bits(a));w.WriteNumber("BBits",BitConverter.SingleToInt32Bits(b));
            w.WriteNumber("SubssBits",BitConverter.SingleToInt32Bits(a-b));w.WritePropertyName("RetainedIndices");SourceReceipt.Ints(w,check.OriginalPointIndices[0].ToArray());w.WriteEndObject();
        }w.WriteEndArray();
        bool canceled=false;try{_=LegacyOrdinaryGraphSourceRanges.ResolveOwnedRangePath(chronology,original,paths[0],keys,new CancellationToken(true));}catch(OperationCanceledException){canceled=true;}if(!canceled)throw new Exception("Generic preentry cancellation missing");
        w.WriteNumber("GenericCancelControls",1);w.WriteNumber("GenericPoolControls",3);w.WriteEndObject();Console.WriteLine("PASS prepared unchanged generic source projection/DFS/pool; B7 not executed");
    }
}
