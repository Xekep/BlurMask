using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;

if(args.Length!=8)throw new ArgumentException("AK engine separation case publication line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var activation=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(activation.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),activation.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);
var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
var seedProfile=LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(ak,189379);var actualProfile=LegacyOrdinarySamplerGroupPruningProfile.ReadV106Node(ak,188157);
int prepared=0,actual=0,created=0,removed=0,guards=0,actualCreated=0;using var stream=File.Create(args[7]);using var w=new Utf8JsonWriter(stream);
w.WriteStartObject();w.WriteStartArray("table");foreach(float value in table.Values.Span)w.WriteNumberValue(value);w.WriteEndArray();w.WriteStartArray("cases");
for(int n=0;n<96;n++)
{
    int cap=3+n%6,descriptorCount=2+n%2;var descriptors=new List<byte[]>();var guides=new List<byte[]>();var localStates=new List<LegacyMainGraphAppendState>();var allStrokes=new List<InkStroke>();
    int physicalBase=0;
    int threshold=new[]{0,8192,0x4000000}[n%3];var profile=actualProfile with{RejectionThreshold=threshold};
    for(int d=0;d<descriptorCount;d++)
    {
        int physical=1+(n+d)%2;var strokes=new InkStroke[physical];var markers=new List<LegacyCursiveCutMarker>();
        for(int s=0;s<physical;s++)
        {
            int points=2+(n+d+s)%3;float x=(d*physical+s)*40+(n%8==0?d*900:n%7==0?-d*200:0);float y=d*100;
            if(n%10==0){x+=d*400;y=d*1000;}
            strokes[s]=new(Enumerable.Range(0,points).Select(p=>new InkPoint(x+p*9,y+(p%2)*50+10)).ToArray());
            for(int p=0;p<points-1;p++)markers.Add(new(0,s,p));
        }
        var ink=LegacyCursivePreparedInk.Create(strokes);var cuts=new LegacyCursiveCutResult(ink,markers,LegacyCursiveInkAtoms.Build(ink.StrokePointCounts.Span,markers.ToArray()));
        float height=40+n%4*20;var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,height,seedProfile,new byte[physical],Enumerable.Repeat(-1,physical).ToArray());
        var seedsGraph=LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds);var metadata=new LegacyMainLocalGraphMetadata(seeds,(ushort)d,physicalBase,d==0?Array.Empty<int>():new[]{d});
        var graph=LegacyOrdinaryCursiveCandidateGraph.GenerateWithAllocations(seeds,seedsGraph,cap,131072,metadata);
        var prune=LegacyOrdinarySamplerGroupPruning.Prepare(graph,ink,height,profile,table);localStates.Add(metadata.Complete(prune));
        var descriptor=new byte[88];I(descriptor,4,int.MinValue);I(descriptor,8,-1);I(descriptor,16,d);I(descriptor,36,physicalBase);I(descriptor,40,physicalBase+physical-1);
        if(n%11==0)I(descriptor,4,17+d);
        if(n%13==0){I(descriptor,8,d);}
        if(n%17==0&&d>0)I(descriptor,16,d+2);
        var guide=new byte[40];F(guide,0,d*100+80);F(guide,4,height);F(guide,12,d*(n%10==0?1400:75)+60);
        descriptors.Add(descriptor);while(guides.Count<=BinaryPrimitives.ReadInt32LittleEndian(descriptor.AsSpan(16)))guides.Add(guide.ToArray());
        guides[BinaryPrimitives.ReadInt32LittleEndian(descriptor.AsSpan(16))]=guide;
        allStrokes.AddRange(strokes);physicalBase+=physical;
    }
    var facts=descriptors.Select(d=>new LegacyMainCrossDescriptor(d,guides)).ToArray();var all=LegacyCursivePreparedInk.Create(allStrokes);
    var state=Run(localStates.ToArray(),facts,all,profile,out var events);
    Write(localStates.ToArray(),descriptors.ToArray(),guides.ToArray(),all,profile,state,events,false);prepared++;
}
w.WriteEndArray();w.WriteStartArray("boundaryFixtures");
for(int test=0;test<8;test++)
{
    var strokes=Enumerable.Range(0,5).Select(s=>new InkStroke(new InkPoint[]{new(s*100,0),new(s*100+1,20),new(s*100+2,40)})).ToArray();var ink=LegacyCursivePreparedInk.Create(strokes);
    var descriptors=new[]{new byte[88],new byte[88]};var guides=new[]{new byte[40],new byte[40]};
    for(int d=0;d<2;d++){I(descriptors[d],4,int.MinValue);I(descriptors[d],8,-1);I(descriptors[d],16,d);I(descriptors[d],36,d*2);I(descriptors[d],40,d*2+1);F(guides[d],0,80+d*100);F(guides[d],4,80);F(guides[d],12,80+d*100);}
    LegacyInkRange[] ranges=test switch{0=>[new(0,1,2),new(1,1,2)],1=>[new(0,1,2),new(2,0,3),new(3,0,3)],2=>[new(2,1,3),new(3,0,3)],
        3=>[new(3,0,3)],4=>[new(2,0,3),new(3,0,2)],5=>[new(2,0,3),new(4,0,3)],6=>[new(2,0,3),new(3,0,3)],_=>[new(0,0,3),new(2,1,3)]};
    var groups=new List<LegacyMainGraphAppendGroup>();for(int g=0;g<3;g++)groups.Add(new(new((ushort)(g==2?1:0),65535,65535,1,1,0,0,0),new(g*100,0,g*100+2,40),[new(g,0,3)],[new[]{new LegacyInkRange(g,0,3)}]));
    groups.Add(new(new(0,1,65535,1,0,0,0,0),new(ranges.Min(r=>r.StrokeIndex)*100,0,ranges.Max(r=>r.StrokeIndex)*100+2,40),ranges,ranges.Select(r=>new[]{r}).ToArray()));
    int[][] Empty()=>Enumerable.Range(0,4).Select(_=>Array.Empty<int>()).ToArray();
    var a0=new LegacyMainGraphAppendArc(0,0,0,4,0,3,65535,0,0,[-1],new[]{new[]{0},Array.Empty<int>(),Array.Empty<int>(),Array.Empty<int>()});
    var a1=new LegacyMainGraphAppendArc(1,0,0,4,128,131,65535,0,0,[0],new[]{new[]{0},Array.Empty<int>(),Array.Empty<int>(),Array.Empty<int>()});
    var a2=new LegacyMainGraphAppendArc(2,0,0,4,256,259,65535,0,0,[0],Empty());var combined=new LegacyMainGraphAppendArc(3,2,0,4,0,259,65535,0,0,[-1],Empty());
    var root=new LegacyMainGraphAppendArc(0,0,0,4,0,0,65535,0,0,[],new[]{new[]{0},Array.Empty<int>(),new[]{1},Array.Empty<int>()});
    var state=LegacyMainGraphAppendState.Create(4,groups,[],new[]{new[]{a0},new[]{a1},new[]{a2,combined}},root);var profile=actualProfile with{RejectionThreshold=test%2==0?0:0x4000000};
    var kernel=new LegacyMainCrossSourceGraph(state,descriptors.Select(d=>new LegacyMainCrossDescriptor(d,guides)).ToArray(),ink,profile,table);kernel.PruneAll(new[]{2,1});
    Write([state],descriptors,guides,ink,profile,kernel.Snapshot(),kernel.Events.ToArray(),false,new[]{2,1});
}
w.WriteEndArray();w.WriteStartArray("actual");
InkStroke M(float y)=>new(new InkPoint[]{new(0,y+90),new(0,y+10),new(25,y+60),new(50,y+10),new(50,y+90)});
InkStroke V(float y)=>new(new InkPoint[]{new(0,y+10),new(0,y+90)});
var mir=new InkStroke[]{M(0),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})};
var twoPhysicalRows=new[]{V(0),new InkStroke(V(0).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray()),V(140),new InkStroke(V(140).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray())};
var inputs=new[]{new[]{M(0),M(140)},new[]{M(0),M(250)},new[]{V(0),V(140)},new[]{V(0),V(250)},mir,mir.Concat(mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+160)).ToArray()))).ToArray(),twoPhysicalRows};
foreach(var raw in inputs)
{
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd).Run();
    var source=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,source,table);
    var result=LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table);var descriptors=source.CopyDescriptors();var guides=source.CopyGuides();var all=LegacyCursivePreparedInk.Create(source.CopyStrokes());
    if(!ReferenceEquals(result.LocalGraphs,local)||!ReferenceEquals(result.Source,source))throw new Exception("Global source owner substituted.");
    Write(local.Steps.Select(s=>s.LocalState).ToArray(),descriptors,guides,all,actualProfile,result.State,result.Events.ToArray(),true);actual++;
    actualCreated+=result.Events.Count(e=>e.Kind==0);
    for(int g=0;g<result.State.GroupCount;g++)
    {
        var pair=result.ResolveGroupSource(g);var f=result.State.Groups[g].Fields;
        if(pair.Primary.DescriptorOrdinal!=f.DescriptorOrdinal||(pair.Secondary?.DescriptorOrdinal??65535)!=f.SecondaryDescriptorOrdinal)throw new Exception("Cross group lost actual primary/secondary source identities.");
    }
    Reject(()=>LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table,new(true)));var bad=ak.ToArray();bad[0]^=1;Reject(()=>LegacyFreshMainGlobalOrdinaryState.Prepare(bad,local,table));
}
w.WriteEndArray();
//A valid, heavily reused group must exhaust the portable point/work budget
//before millions of repeated ranges can turn a bounded graph into unbounded
//point scanning. No large source or unsafe native allocation is reproduced.
{
    var strokes=new[]{new InkStroke(Enumerable.Range(0,2048).Select(p=>new InkPoint(p,0)).ToArray()),new InkStroke(Enumerable.Range(0,2048).Select(p=>new InkPoint(p+4000,0)).ToArray())};
    var ink=LegacyCursivePreparedInk.Create(strokes);var d=new byte[88];I(d,4,int.MinValue);I(d,8,-1);I(d,16,0);I(d,36,0);I(d,40,1);var guide=new byte[40];F(guide,0,80);F(guide,4,80);
    int[][] Empty()=>new[]{Array.Empty<int>(),Array.Empty<int>()};
    var groups=new[]{new LegacyMainGraphAppendGroup(new(0,65535,65535,1,1,0,0,0),new(0,0,2047,0),[new(0,0,2048)],[]),
        new LegacyMainGraphAppendGroup(new(0,65535,65535,200,0,0,0,0),new(0,0,6047,0),[new(0,0,2048),new(1,0,2048)],[])};
    var single=new LegacyMainGraphAppendArc(0,0,0,4,0,3,65535,0,0,[-1],Empty());var repeated=new LegacyMainGraphAppendArc(1,1,0,4,0,131,65535,0,0,[-1],Empty());
    var root=new LegacyMainGraphAppendArc(0,0,0,4,0,0,65535,0,0,[],new[]{new[]{0},Enumerable.Range(0,325).ToArray()});
    var state=LegacyMainGraphAppendState.Create(2,groups,[],new[]{new[]{single},Enumerable.Repeat(repeated,325).ToArray()},root);
    var kernel=new LegacyMainCrossSourceGraph(state,new[]{new LegacyMainCrossDescriptor(d,new[]{guide})},ink,actualProfile with{RejectionThreshold=0x4000000},table);
    try{kernel.PruneAll(new[]{2});throw new Exception("Repeated point work budget ignored.");}catch(ArgumentException e)when(e.Message.Contains("budget")){guards++;}
}
w.WriteNumber("prepared",prepared);w.WriteNumber("actualCount",actual);w.WriteNumber("created",created);w.WriteNumber("removed",removed);w.WriteNumber("actualCreated",actualCreated);w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
if(created==0||removed==0||actualCreated==0)throw new Exception("Cross allocation/prune/actual cross paths unexercised.");
Console.WriteLine($"PASS {prepared} prepared cross states +{actual} actual source owners; {created} cross groups ({actualCreated} actual), {removed} boundary/final-node removals, {guards} guards. No compact-global NN/recognition claim.");

LegacyMainGraphAppendState Run(LegacyMainGraphAppendState[] locals,LegacyMainCrossDescriptor[] facts,LegacyCursivePreparedInk ink,LegacyOrdinarySamplerGroupPruningProfile profile,out LegacyMainCrossGraphEvent[] events)
{
    var state=LegacyMainGraphAppendState.Empty(locals[0].MaximumAtoms);var log=new List<LegacyMainCrossGraphEvent>();int previous=-1;
    foreach(var local in locals)
    {
        int old=state.NodeCount-1;state=state.Append(local).State;
        if(previous>=0&&previous+local.NodeCount-1<state.MaximumAtoms)
        {var kernel=new LegacyMainCrossSourceGraph(state,facts,ink,profile,table);kernel.MergeBoundary(old,previous-1,state.NodeCount-1);state=kernel.Snapshot();log.AddRange(kernel.Events);}
        previous=local.NodeCount;
    }
    var prune=new LegacyMainCrossSourceGraph(state,facts,ink,profile,table);prune.PruneAll(locals.Select(s=>s.NodeCount).ToArray());state=prune.Snapshot();log.AddRange(prune.Events);events=log.ToArray();return state;
}
void Write(LegacyMainGraphAppendState[] locals,byte[][] descriptors,byte[][] guides,LegacyCursivePreparedInk ink,LegacyOrdinarySamplerGroupPruningProfile profile,LegacyMainGraphAppendState state,LegacyMainCrossGraphEvent[] events,bool actualCase,int[]? pruneOnly=null)
{
    w.WriteStartObject();w.WriteBoolean("actual",actualCase);w.WriteNumber("threshold",profile.RejectionThreshold);w.WriteNumber("gap",profile.Separation.GapOffset);w.WriteNumber("slope",profile.Separation.Slope);
    if(pruneOnly is not null){w.WriteStartArray("pruneOnly");foreach(int value in pruneOnly)w.WriteNumberValue(value);w.WriteEndArray();}
    w.WriteStartArray("descriptors");foreach(var d in descriptors)w.WriteBase64StringValue(d);w.WriteEndArray();w.WriteStartArray("guides");foreach(var g in guides)w.WriteBase64StringValue(g);w.WriteEndArray();
    w.WriteStartArray("locals");foreach(var s in locals)Capture.State(w,s);w.WriteEndArray();
    w.WriteStartArray("strokes");int at=0;foreach(int count in ink.StrokePointCounts.Span){w.WriteStartArray();for(int p=0;p<count;p++){var v=ink.Points.Span[at++];w.WriteStartArray();w.WriteNumberValue(v.X);w.WriteNumberValue(v.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("events");foreach(var e in events){w.WriteStartArray();foreach(int v in new[]{e.Kind,e.Node,e.Ordinal,e.Group,e.OtherNode,e.OtherOrdinal})w.WriteNumberValue(v);w.WriteEndArray();if(e.Kind==0)created++;if(e.Kind==2)removed++;}w.WriteEndArray();
    w.WritePropertyName("output");Capture.State(w,state);w.WriteEndObject();
}
static void I(byte[] b,int offset,int value)=>BinaryPrimitives.WriteInt32LittleEndian(b.AsSpan(offset),value);
static void F(byte[] b,int offset,float value)=>BinaryPrimitives.WriteSingleLittleEndian(b.AsSpan(offset),value);
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Foreign/canceled actual owner accepted.");}
