using InkRecognition;
using System.Text.Json;

if(args.Length!=8)throw new ArgumentException("AK engine separation case publication line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var activation=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(activation.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),activation.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);
var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
var seedProfile=LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(ak,189379);
var pruneProfile=LegacyOrdinarySamplerGroupPruningProfile.ReadV106Node(ak,188157);
int prepared=0,actual=0,removed=0,guards=0,deferred=0,allocations=0;using var output=File.Create(args[7]);using var w=new Utf8JsonWriter(output);
w.WriteStartObject();Floats("table",table.Values.ToArray());w.WriteStartArray("cases");
for(int n=0;n<96;n++)
{
    int physical=2+n%4;var raw=new InkStroke[physical];var markers=new List<LegacyCursiveCutMarker>();
    for(int s=0;s<physical;s++)
    {
        int count=2+(n+s)%4;float x=s*80;
        if(s==physical-1&&n%3!=0)x=10+n%3*25;
        raw[s]=new(Enumerable.Range(0,count).Select(p=>new InkPoint(x+p*7,10+p%2*70)).ToArray());
        for(int p=0;p<count-1;p++)markers.Add(new(0,s,p));
    }
    var ink=LegacyCursivePreparedInk.Create(raw);var cuts=new LegacyCursiveCutResult(ink,markers,LegacyCursiveInkAtoms.Build(ink.StrokePointCounts.Span,markers.ToArray()));
    var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,80,seedProfile,new byte[physical],Enumerable.Repeat(-1,physical).ToArray());
    var seedGraph=LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds);int cap=2+n%7,shift=n%9,descriptor=n%16;
    var metadata=new LegacyMainLocalGraphMetadata(seeds,(ushort)descriptor,shift,Enumerable.Range(0,n%3).ToArray());
    var graph=LegacyOrdinaryCursiveCandidateGraph.GenerateWithAllocations(seeds,seedGraph,cap,131072,metadata);
    var old=LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,seedGraph,cap);
    if(!GraphBytes(graph).SequenceEqual(GraphBytes(old)))throw new Exception("Allocation observers changed existing graph bytes.");
    int threshold=new[]{0,20480,0x4000000}[n%3];var profile=pruneProfile with{RejectionThreshold=threshold};
    var pruning=LegacyOrdinarySamplerGroupPruning.Prepare(graph,ink,80,profile,table);var state=metadata.Complete(pruning);
    for(int g=0;g<graph.Groups.Count;g++)if(!state.Groups[g].CopyRanges().SequenceEqual(graph.Groups[g].Ranges.ToArray().Select(r=>r with{StrokeIndex=r.StrokeIndex+shift})))throw new Exception("Metadata history composition changed actual group geometry.");
    WriteCase(seeds,ink,pruning,metadata,state,descriptor,shift,n%3,80,profile,false);prepared++;
}
w.WriteEndArray();w.WriteStartArray("actual");
InkStroke[] Shape(float y)=>[new(new InkPoint[]{new(0,90+y),new(0,10+y),new(25,60+y),new(50,10+y),new(50,90+y)})];
var mir=new InkStroke[]{Shape(0)[0],new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})};
var inputs=new[]{mir,mir.Concat(mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+160)).ToArray()))).ToArray(),
    Shape(0).Concat(Shape(140)).ToArray(),Shape(0).Concat(Shape(250)).ToArray(),
    new InkStroke[]{new(new InkPoint[]{new(0,10),new(0,90)}),new(new InkPoint[]{new(0,150),new(0,230)})}};
foreach(var raw in inputs)
{
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd).Run();
    var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);var result=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table);
    if(!ReferenceEquals(result.Source,cuts)||result.Steps.Count!=cuts.Steps.Count(s=>!s.IsPendingSeparator))throw new Exception("Actual descriptor owner was replaced.");
    w.WriteStartObject();w.WriteStartArray("steps");
    foreach(var step in result.Steps)
    {
        var s=step.Source;var view=LegacyFreshMainDescriptorGraphAppend.CreateLocalGeometryView(s);
        WriteCase(step.Seeds,view.PreparedInk,step.LocalPruning,step.Metadata,step.LocalState,s.DescriptorOrdinal,s.FirstPhysical,s.CopyPendingDescriptorOrdinals().Length,s.Height,pruneProfile,true);
        foreach(var group in step.LocalState.Groups)foreach(var r in group.CopyRanges())
            if(r.StrokeIndex<s.FirstPhysical||r.StrokeIndex>s.LastPhysical)throw new Exception("Global physical source range lost.");
    }
    w.WriteEndArray();w.WritePropertyName("appended");WriteState(result.State);w.WriteEndObject();actual++;
    for(int g=0;g<result.State.GroupCount;g++)if(result.ResolveGroupSource(g).DescriptorOrdinal!=result.State.Groups[g].Fields.DescriptorOrdinal)throw new Exception("Descriptor identity mismatch.");
    Reject(()=>LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table,new(true)));
    var wrong=ak.ToArray();wrong[^1]^=1;Reject(()=>LegacyFreshMainDescriptorGraphAppend.Prepare(wrong,cuts,table));
    if(raw.Length==2&&raw.All(s=>s.Points.Count==2))
    {
        var old=LegacyFreshMainSingleAtomDescriptorAppend.Prepare(ak,cuts);
        if(!StateBytes(old.State).SequenceEqual(StateBytes(result.State)))throw new Exception("Whole single-atom append bytes changed.");
    }
}
w.WriteEndArray();w.WriteStartArray("referenceLifecycle");
foreach(int allocated in new[]{199,200,201})foreach(int remove in new[]{0,1,allocated-1,allocated})
{
    //Explicit prepared allocation/deletion stream, not the local final-node
    //prune policy. Exercises the real metadata recorder on both sides of200.
    var ink=LegacyCursivePreparedInk.Create([new InkStroke(new InkPoint[]{new(0,0),new(1,1)})]);
    var marker=new LegacyCursiveCutMarker(0,0,0);var source=new LegacyCursiveCutResult(ink,new[]{marker},new[]{new LegacyCursiveInkAtom(marker,new(0,0,2))});
    var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(source,80,seedProfile,new byte[1],new[]{-1});
    var metadata=new LegacyMainLocalGraphMetadata(seeds,0,0,[]);
    var seedArc=new LegacyOrdinaryCursiveSeedArc(0,0,4,new[]{-1},Array.Empty<int>(),0,3);
    for(int i=0;i<allocated;i++)metadata.SeedArc(0,i,seedArc);
    var arc=new LegacyOrdinaryCursiveCandidateArc(0,0,4,0,3,new[]{-1},Array.Empty<LegacyCursiveCandidateSuccessor>());
    var group=new LegacyOrdinaryCursiveCandidateInk(0,new[]{0},new[]{new LegacyInkRange(0,0,2)});
    var before=new LegacyOrdinaryCursiveCandidateGraph([Enumerable.Repeat(arc,allocated).ToArray()],[group],2);
    int retained=allocated-remove;var after=new LegacyOrdinaryCursiveCandidateGraph([Enumerable.Repeat(arc,retained).ToArray()],[group],2);
    var pruning=new LegacyOrdinarySamplerGroupPruningResult(before,after,new ReadOnlyMemory<int>[]{Enumerable.Range(0,retained).ToArray()},remove);
    var state=metadata.Complete(pruning);int expected=allocated>=200?200:retained;
    if(state.Groups[0].Fields.ReferenceCounter!=expected)throw new Exception("Sticky200 allocation/removal recorder changed.");
    w.WriteStartObject();w.WriteNumber("allocated",allocated);w.WriteNumber("removed",remove);w.WriteNumber("counter",state.Groups[0].Fields.ReferenceCounter);w.WriteNumber("retained",state.GetNode(0).Count);w.WriteEndObject();
}
w.WriteEndArray();w.WriteStartArray("historySort");
for(int n=0;n<64;n++)
{
    var rows=Enumerable.Range(0,2+n%13).Select(i=>new[]{new LegacyInkRange((i*7+n)%4,(i*3+n)%3,20+i)}).Reverse().ToArray();
    var before=rows.Select(r=>r.ToArray()).ToArray();LegacyMainLocalGraphMetadata.SortHistory(rows);
    w.WriteStartObject();History("input",before);History("output",rows);w.WriteEndObject();
}
w.WriteEndArray();w.WriteNumber("prepared",prepared);w.WriteNumber("actualCount",actual);w.WriteNumber("removed",removed);w.WriteNumber("deferred",deferred);w.WriteNumber("allocations",allocations);w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
if(removed==0||deferred==0)throw new Exception("Prune/deferred allocation paths unexercised.");
Console.WriteLine($"PASS {prepared} prepared allocation/pruning cases, {actual} actual owned sources, {allocations} allocations/{removed} ordered removals/{deferred} deferred seeds; old graph and single-atom bytes unchanged. Append-stage only.");

void WriteCase(LegacyOrdinaryCursiveSeedResult seeds,LegacyCursivePreparedInk ink,LegacyOrdinarySamplerGroupPruningResult pruning,
    LegacyMainLocalGraphMetadata metadata,LegacyMainGraphAppendState state,int descriptor,int offset,int pending,float height,LegacyOrdinarySamplerGroupPruningProfile profile,bool actualCase)
{
    w.WriteStartObject();w.WriteBoolean("actual",actualCase);w.WriteNumber("descriptor",descriptor);w.WriteNumber("offset",offset);w.WriteNumber("pending",pending);
    w.WriteNumber("height",height);w.WriteNumber("gap",profile.Separation.GapOffset);w.WriteNumber("slope",profile.Separation.Slope);w.WriteNumber("threshold",profile.RejectionThreshold);
    w.WriteStartArray("seeds");foreach(var s in seeds.Seeds){w.WriteStartObject();w.WriteNumber("sourceAtom",s.SourceAtomIndex);w.WriteBoolean("deferred",s.Deferred);Range("range",s.Atom.Range);Floats("bounds",[s.MinimumX,s.MinimumY,s.MaximumX,s.MaximumY]);w.WriteEndObject();}w.WriteEndArray();
    w.WriteStartArray("strokes");int p=0;foreach(int count in ink.StrokePointCounts.Span){w.WriteStartArray();for(int i=0;i<count;i++){var point=ink.Points.Span[p++];w.WriteStartArray();w.WriteNumberValue(point.X);w.WriteNumberValue(point.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("allocations");foreach(var e in metadata.Allocations){Ints(null,[e.Kind,e.Node,e.Ordinal,e.Group,e.LeftNode,e.LeftOrdinal]);allocations++;}w.WriteEndArray();
    w.WriteStartArray("mergedGroups");foreach(var e in metadata.MergedGroups)Ints(null,[e.Group,e.Left,e.Right]);w.WriteEndArray();
    w.WriteStartArray("removed");foreach(var e in metadata.Removals){Ints(null,[e.Node,e.Ordinal,e.Group]);removed++;}w.WriteEndArray();
    WriteGraph("before",pruning.OriginalGraph);WriteGraph("after",pruning.Graph);
    w.WriteStartArray("originalOrdinals");foreach(var v in pruning.OriginalArcOrdinals)Ints(null,v.ToArray());w.WriteEndArray();
    w.WritePropertyName("state");WriteState(state);w.WriteEndObject();deferred+=seeds.DeferredCount;
}
void WriteGraph(string name,LegacyOrdinaryCursiveCandidateGraph graph){w.WritePropertyName(name);Graph(w,graph);}
static void Graph(Utf8JsonWriter x,LegacyOrdinaryCursiveCandidateGraph graph)
{
    x.WriteStartArray();for(int n=0;n<graph.NodeCount;n++){x.WriteStartArray();foreach(var a in graph.GetNode(n))
    {x.WriteStartArray();x.WriteNumberValue(a.GroupId);x.WriteNumberValue(a.Span);x.WriteNumberValue(a.Flags);x.WriteNumberValue(a.StartContextId);x.WriteNumberValue(a.EndContextId);x.WriteStartArray();foreach(int p in a.PredecessorArcIndices.Span)x.WriteNumberValue(p);x.WriteEndArray();x.WriteStartArray();foreach(var p in a.Successors){x.WriteStartArray();x.WriteNumberValue(p.NodeOffset);x.WriteNumberValue(p.ArcIndex);x.WriteEndArray();}x.WriteEndArray();x.WriteEndArray();}x.WriteEndArray();}x.WriteEndArray();
}
void WriteState(LegacyMainGraphAppendState state)=>State(w,state);
static void State(Utf8JsonWriter x,LegacyMainGraphAppendState state)
{
    x.WriteStartObject();x.WriteNumber("cap",state.MaximumAtoms);x.WriteStartArray("groups");foreach(var g in state.Groups)
    {
        x.WriteStartObject();var f=g.Fields;WriteInts(x,"fields",[f.DescriptorOrdinal,f.SecondaryDescriptorOrdinal,f.AuxiliaryContextIndex,f.ReferenceCounter,f.HistoryOwned08,f.Flags50,f.Flags51,f.Flags52]);
        var b=g.Bounds;WriteInts(x,"boundsBits",new[]{b.MinimumX,b.MinimumY,b.MaximumX,b.MaximumY}.Select(BitConverter.SingleToInt32Bits));
        x.WriteStartArray("ranges");foreach(var r in g.CopyRanges())WriteInts(x,null,[r.StrokeIndex,r.StartPoint,r.EndPointExclusive]);x.WriteEndArray();
        x.WriteStartArray("history");foreach(var h in g.CopyHistory()){x.WriteStartArray();foreach(var r in h)WriteInts(x,null,[r.StrokeIndex,r.StartPoint,r.EndPointExclusive]);x.WriteEndArray();}x.WriteEndArray();x.WriteEndObject();
    }x.WriteEndArray();x.WriteStartArray("contexts");foreach(var c in state.Contexts){x.WriteStartObject();x.WriteBase64String("prefix",c.CopyPrefix());WriteInts(x,"controls",c.CopyControls());x.WriteEndObject();}x.WriteEndArray();
    x.WriteStartArray("nodes");for(int n=0;n<state.NodeCount;n++){x.WriteStartArray();foreach(var a in state.GetNode(n))Arc(x,a);x.WriteEndArray();}x.WriteEndArray();x.WritePropertyName("root");Arc(x,state.Root);x.WriteEndObject();
}
static void Arc(Utf8JsonWriter x,LegacyMainGraphAppendArc a){x.WriteStartObject();x.WriteStartArray("header");foreach(long value in new long[]{a.GroupId,a.Span,a.Placement,a.Flags,a.StartContextId,a.EndContextId,a.AuxiliaryContextIndex,a.Extra1a,a.Scalar1cBits})x.WriteNumberValue(value);x.WriteEndArray();WriteInts(x,"previous",a.CopyPredecessors());x.WriteStartArray("next");foreach(var v in a.CopySuccessors())WriteInts(x,null,v);x.WriteEndArray();x.WriteEndObject();}
static void WriteInts(Utf8JsonWriter x,string? name,IEnumerable<int> values){if(name is not null)x.WritePropertyName(name);x.WriteStartArray();foreach(int v in values)x.WriteNumberValue(v);x.WriteEndArray();}
void Ints(string? name,IEnumerable<int> v)=>WriteInts(w,name,v);
void Floats(string name,IEnumerable<float> v){w.WriteStartArray(name);foreach(float f in v)w.WriteNumberValue(f);w.WriteEndArray();}
void Range(string name,LegacyInkRange r)=>Ints(name,[r.StrokeIndex,r.StartPoint,r.EndPointExclusive]);
void History(string name,LegacyInkRange[][] values){w.WriteStartArray(name);foreach(var row in values){w.WriteStartArray();foreach(var r in row)Ints(null,[r.StrokeIndex,r.StartPoint,r.EndPointExclusive]);w.WriteEndArray();}w.WriteEndArray();}
static byte[] GraphBytes(LegacyOrdinaryCursiveCandidateGraph graph){using var s=new MemoryStream();using var x=new Utf8JsonWriter(s);Graph(x,graph);x.Flush();return s.ToArray();}
static byte[] StateBytes(LegacyMainGraphAppendState state){using var s=new MemoryStream();using var x=new Utf8JsonWriter(s);State(x,state);x.Flush();return s.ToArray();}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidDataException or NotSupportedException){guards++;return;}throw new Exception("Foreign/canceled owner admitted.");}
