using InkRecognition;
using System.Text.Json;
using System.Security.Cryptography;
if(args.Length!=3)throw new ArgumentException("MODELS BLIND OUTPUT");
byte[] resource=File.ReadAllBytes(Path.Combine(args[0],"ru-RU/ru_RU-ak-cur.lite.res"));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(args[0],"ru-RU/legacy-separation-profile.json")));
byte[] input=File.ReadAllBytes(args[1]);using var doc=JsonDocument.Parse(input);using var file=File.Create(args[2]);using var w=new Utf8JsonWriter(file);
w.WriteStartObject();w.WriteString("scope","Recorded Latin geometry fed actual Russian FIRST owner; no Russian targets or accuracy claim");w.WriteBoolean("expectedLabelsRead",false);w.WriteString("inputSha256",Convert.ToHexStringLower(SHA256.HashData(input)));w.WriteStartArray("table");foreach(float v in table.Values.Span)w.WriteNumberValue(v);w.WriteEndArray();w.WriteStartArray("samples");int samples=0,active=0,totalTail=0;
foreach(var sample in doc.RootElement.GetProperty("samples").EnumerateArray())
{
 var raw=sample.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
 using var cancel=new CancellationTokenSource(TimeSpan.FromSeconds(30));var token=cancel.Token;
 var factory=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,6120,raw,cancellationToken:token);var row=factory.Frame.Row;var cuts=row.GenerateRussianCuts(cancellationToken:token);
 var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,12727),new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray(),cancellationToken:token);
 var original=LegacyOrdinaryGraphSourceRanges.Prepare(seeds,cuts.Chronology,row.OriginalSource,7,cancellationToken:token);
 var profile=LegacyOrdinarySamplerGroupPruningProfile.ReadV106Node(resource,11506);
 var local=LegacyOrdinarySamplerGroupPruning.Prepare(original.Graph,cuts.Cuts.PreparedInk,row.Frame.Line.Height,profile,table,token);
 var full=LegacyOrdinarySamplerGroupPruning.PrepareSingleDescriptor(original.Graph,cuts.Cuts.PreparedInk,row.Frame.Line.Height,profile,table,token);
 var actual=LegacyFreshFirstCandidateGraphPreparer.Prepare(resource,factory,cuts,seeds,table,token);
 int last=original.Graph.NodeCount-1;
 for(int n=0;n<original.Graph.NodeCount;n++)if(!actual.Pruning.OriginalArcOrdinals[n].Span.SequenceEqual(full.OriginalArcOrdinals[n].Span))throw new Exception("Production FIRST differs from complete native pass");
 Reject(()=>LegacyFreshFirstCandidateGraphPreparer.Prepare(resource,factory,cuts,seeds,table,new CancellationToken(true)));
 var foreign=resource.ToArray();foreign[12727+16]^=1;Reject(()=>LegacyFreshFirstCandidateGraphPreparer.Prepare(foreign,factory,cuts,seeds,table));var removed=Enumerable.Range(0,local.Graph.GetNode(last).Count).Where(i=>!full.OriginalArcOrdinals[last].Span.Contains(i)).ToArray();
 for(int n=0;n<last;n++)if(!local.OriginalArcOrdinals[n].Span.SequenceEqual(full.OriginalArcOrdinals[n].Span))throw new Exception("Non-tail changed");
 w.WriteStartObject();w.WriteString("id",sample.GetProperty("id").GetString());w.WriteNumber("strokeCount",raw.Length);w.WriteNumber("height",row.Frame.Line.Height);w.WriteNumber("threshold",profile.RejectionThreshold);w.WriteNumber("gap",profile.Separation.GapOffset);w.WriteNumber("slope",profile.Separation.Slope);w.WriteNumber("nodes",original.Graph.NodeCount);w.WriteNumber("localRemoved",local.RemovedArcCount);w.WriteNumber("fullRemoved",full.RemovedArcCount);
 w.WriteStartArray("groups");foreach(var g in original.Graph.Groups){w.WriteStartArray();foreach(var rg in g.Ranges.Span){w.WriteStartArray();w.WriteNumberValue(rg.StrokeIndex);w.WriteNumberValue(rg.StartPoint);w.WriteNumberValue(rg.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();WriteGraph("before",original.Graph);WriteGraph("local",local.Graph);WriteGraph("full",full.Graph);WriteGraph("actual",actual.SourceRanges.Graph);w.WriteStartArray("fullOrdinals");foreach(var rowOrd in full.OriginalArcOrdinals){w.WriteStartArray();foreach(int n in rowOrd.Span)w.WriteNumberValue(n);w.WriteEndArray();}w.WriteEndArray();w.WriteStartArray("tail");foreach(int ordinal in removed){var arc=original.Graph.GetNode(last)[ordinal];w.WriteStartObject();w.WriteNumber("ordinal",ordinal);w.WriteNumber("group",arc.GroupId);w.WriteNumber("span",arc.Span);w.WriteStartArray("ranges");foreach(var range in original.Graph.Groups[arc.GroupId].Ranges.Span){w.WriteStartArray();w.WriteNumberValue(range.StrokeIndex);w.WriteNumberValue(range.StartPoint);w.WriteNumberValue(range.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();w.WriteNumber("cost",LegacyCursiveGroupSeparationCost.Evaluate(cuts.Cuts.PreparedInk,original.Graph.Groups[arc.GroupId].Ranges.Span,row.Frame.Line.Height,profile.Separation.GapOffset,profile.Separation.Slope,table.Values.Span,0x4000000,cancellationToken:token));w.WriteEndObject();}w.WriteEndArray();
 w.WriteStartArray("prepared");int point=0;foreach(int count in cuts.Cuts.PreparedInk.StrokePointCounts.Span){w.WriteStartArray();for(int i=0;i<count;i++){var p=cuts.Cuts.PreparedInk.Points.Span[point++];w.WriteStartArray();w.WriteNumberValue(p.X);w.WriteNumberValue(p.Y);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
 w.WriteEndObject();samples++;if(removed.Length>0)active++;totalTail+=removed.Length;
}
w.WriteEndArray();w.WriteNumber("samplesCount",samples);w.WriteNumber("activeSamples",active);w.WriteNumber("removedTailArcs",totalTail);w.WriteEndObject();w.Flush();Console.WriteLine($"PASS {samples} real geometries, {active} active tail cases, {totalTail} tail removals; labels not read");

void WriteGraph(string name,LegacyOrdinaryCursiveCandidateGraph graph){w.WriteStartArray(name);for(int n=0;n<graph.NodeCount;n++){w.WriteStartArray();foreach(var a in graph.GetNode(n)){w.WriteStartArray();w.WriteNumberValue(a.GroupId);w.WriteNumberValue(a.Span);w.WriteNumberValue(a.Flags);w.WriteNumberValue(a.StartContextId);w.WriteNumberValue(a.EndContextId);w.WriteStartArray();foreach(int p in a.PredecessorArcIndices.Span)w.WriteNumberValue(p);w.WriteEndArray();w.WriteStartArray();foreach(var t in a.Successors){w.WriteStartArray();w.WriteNumberValue(t.NodeOffset);w.WriteNumberValue(t.ArcIndex);w.WriteEndArray();}w.WriteEndArray();w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();}

void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException or InvalidDataException){return;}throw new Exception("Foreign/cancelled FIRST accepted");}
