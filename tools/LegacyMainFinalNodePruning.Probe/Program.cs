using InkRecognition;
using System.Text.Json;

if(args.Length!=8)throw new ArgumentException("AK engine separation case publication line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var activation=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(activation.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),activation.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
var seedProfile=LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(ak,189379);var actualProfile=LegacyOrdinarySamplerGroupPruningProfile.ReadV106Node(ak,188157);
int cases=0,actual=0,sourceChecks=0,guards=0,finalRemoved=0;using var output=File.Create(args[7]);using var w=new Utf8JsonWriter(output);
w.WriteStartObject();w.WriteStartArray("table");foreach(float v in table.Values.Span)w.WriteNumberValue(v);w.WriteEndArray();w.WriteStartArray("cases");
for(int n=0;n<24;n++)foreach(int threshold in new[]{0,20480,0x4000000})
{
    var raw=Enumerable.Range(0,2+n%4).Select(s=>new InkStroke(Enumerable.Range(0,3).Select(p=>new InkPoint(s*80+(n%4)*40+p*8,p%2*80+10)).ToArray())).ToArray();
    var ink=LegacyCursivePreparedInk.Create(raw);var markers=Enumerable.Range(0,raw.Length).SelectMany(s=>new[]{new LegacyCursiveCutMarker(0,s,0),new LegacyCursiveCutMarker(0,s,1)}).ToArray();
    var cuts=new LegacyCursiveCutResult(ink,markers,LegacyCursiveInkAtoms.Build(ink.StrokePointCounts.Span,markers));
    var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,80,seedProfile,new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());
    var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds),3+n%6);
    var profile=actualProfile with{RejectionThreshold=threshold};var local=LegacyOrdinarySamplerGroupPruning.Prepare(graph,ink,80,profile,table);
    var complete=LegacyOrdinarySamplerGroupPruning.PrepareSingleDescriptor(graph,ink,80,profile,table);
    Write(graph,ink,80,profile,local,complete,false);cases++;
}
w.WriteEndArray();w.WriteStartArray("actual");
foreach(float distance in new[]{70f,100f,160f,260f})
{
    var raw=Enumerable.Range(0,2).Select(n=>new InkStroke(new InkPoint[]{new(n*distance,90),new(n*distance,10),new(n*distance+25,60),new(n*distance+50,10),new(n*distance+50,90)})).ToArray();
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd).Run();
    var sampler=LegacyFreshMainSingleLineSampler.Prepare(ak,input.Smoothed);var sampled=sampler.Sample(table);var cuts=sampled.Cuts;
    var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,sampler.SourceHeight,seedProfile,new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());
    var source=sampler.Preparation.Input.Input.Input.Owner.Controller.Source;
    var before=LegacyOrdinaryGraphSourceRanges.Prepare(seeds,sampled.Chronology,source,sampler.SpanProfile.MaximumAtoms);
    var local=LegacyOrdinarySamplerGroupPruning.Prepare(before.Graph,cuts.PreparedInk,sampler.SourceHeight,actualProfile,table);
    var complete=LegacyOrdinarySamplerGroupPruning.PrepareSingleDescriptor(before.Graph,cuts.PreparedInk,sampler.SourceHeight,actualProfile,table);
    if(!GraphBytes(complete.Graph).SequenceEqual(GraphBytes(sampled.SourceRanges.Graph))||sampled.Pruning.RemovedArcCount!=complete.RemovedArcCount)
        throw new Exception("Owning sampler did not use the native complete single-descriptor pass.");
    if(complete.Graph.Groups.Count!=before.Graph.Groups.Count||!ReferenceEquals(sampled.Pruning.Graph,sampled.SourceRanges.Graph))throw new Exception("Original group/source graph identity changed.");
    for(int g=0;g<before.Graph.Groups.Count;g++)if(!before.Graph.Groups[g].Ranges.Span.SequenceEqual(complete.Graph.Groups[g].Ranges.Span)||
        !before.Graph.Groups[g].SeedGroupIndices.Span.SequenceEqual(complete.Graph.Groups[g].SeedGroupIndices.Span))throw new Exception("Group IDs/membership were remapped.");
    var rebound=before.WithSamplerPruning(complete);
    for(int node=0;node<complete.Graph.NodeCount;node++)for(int arc=0;arc<complete.Graph.GetNode(node).Count;arc++)
    {
        int old=complete.OriginalArcOrdinals[node].Span[arc];
        if(!before.ResolveCandidate(node,old).SequenceEqual(rebound.ResolveCandidate(node,arc))||
            !before.ResolveCandidate(node,old).SequenceEqual(sampled.SourceRanges.ResolveCandidate(node,arc)))throw new Exception("Retained source references changed.");sourceChecks++;
    }
    int removed=complete.RemovedArcCount-local.RemovedArcCount;finalRemoved+=removed;
    if(distance==70&&removed!=0)throw new Exception("Below-threshold actual graph changed.");
    if(distance>=100&&(removed!=4||sampler.SourceHeight!=40||actualProfile.RejectionThreshold!=20480))throw new Exception("Actual final-node rejection was lost.");
    Write(before.Graph,cuts.PreparedInk,sampler.SourceHeight,actualProfile,local,complete,true);actual++;
    Reject(()=>LegacyOrdinarySamplerGroupPruning.PrepareSingleDescriptor(before.Graph,cuts.PreparedInk,sampler.SourceHeight,actualProfile,table,new(true)));
    Reject(()=>before.WithSamplerPruning(sampled.Pruning));
}
w.WriteEndArray();w.WriteNumber("prepared",cases);w.WriteNumber("actualCount",actual);w.WriteNumber("actualFinalRemoved",finalRemoved);w.WriteNumber("sourceChecks",sourceChecks);w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS {cases} prepared full/local prune pairs, {actual} actual inputs/{finalRemoved} genuine final removals, {sourceChecks} retained source refs/{guards} guards; groupIDs unchanged, old local API retained.");

void Write(LegacyOrdinaryCursiveCandidateGraph before,LegacyCursivePreparedInk ink,float height,LegacyOrdinarySamplerGroupPruningProfile profile,LegacyOrdinarySamplerGroupPruningResult local,LegacyOrdinarySamplerGroupPruningResult complete,bool actualCase)
{
    w.WriteStartObject();w.WriteBoolean("actual",actualCase);w.WriteNumber("height",height);w.WriteNumber("gap",profile.Separation.GapOffset);w.WriteNumber("slope",profile.Separation.Slope);w.WriteNumber("threshold",profile.RejectionThreshold);
    w.WriteStartArray("strokes");int at=0;foreach(int count in ink.StrokePointCounts.Span){w.WriteStartArray();for(int p=0;p<count;p++){var v=ink.Points.Span[at++];w.WriteStartArray();w.WriteNumberValue(v.X);w.WriteNumberValue(v.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("groups");foreach(var g in before.Groups){w.WriteStartArray();foreach(var r in g.Ranges.Span){w.WriteStartArray();w.WriteNumberValue(r.StrokeIndex);w.WriteNumberValue(r.StartPoint);w.WriteNumberValue(r.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
    w.WritePropertyName("before");Graph(w,before);w.WritePropertyName("local");Graph(w,local.Graph);w.WritePropertyName("complete");Graph(w,complete.Graph);
    w.WriteNumber("localRemoved",local.RemovedArcCount);w.WriteNumber("completeRemoved",complete.RemovedArcCount);w.WriteStartArray("ordinals");foreach(var row in complete.OriginalArcOrdinals){w.WriteStartArray();foreach(int value in row.Span)w.WriteNumberValue(value);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
}
static void Graph(Utf8JsonWriter x,LegacyOrdinaryCursiveCandidateGraph graph)
{
    x.WriteStartArray();for(int n=0;n<graph.NodeCount;n++){x.WriteStartArray();foreach(var a in graph.GetNode(n))
    {x.WriteStartArray();x.WriteNumberValue(a.GroupId);x.WriteNumberValue(a.Span);x.WriteNumberValue(a.Flags);x.WriteNumberValue(a.StartContextId);x.WriteNumberValue(a.EndContextId);x.WriteStartArray();foreach(int p in a.PredecessorArcIndices.Span)x.WriteNumberValue(p);x.WriteEndArray();x.WriteStartArray();foreach(var p in a.Successors){x.WriteStartArray();x.WriteNumberValue(p.NodeOffset);x.WriteNumberValue(p.ArcIndex);x.WriteEndArray();}x.WriteEndArray();x.WriteEndArray();}x.WriteEndArray();}x.WriteEndArray();
}
static byte[] GraphBytes(LegacyOrdinaryCursiveCandidateGraph graph){using var s=new MemoryStream();using var x=new Utf8JsonWriter(s);Graph(x,graph);x.Flush();return s.ToArray();}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException){guards++;return;}throw new Exception("Foreign/canceled pruning admitted.");}
