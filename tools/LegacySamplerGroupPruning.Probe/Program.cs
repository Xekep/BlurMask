using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if(args.Length!=3)throw new ArgumentException("AK separation-profile.json audit-output.json");
byte[] resource=File.ReadAllBytes(args[0]);var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[1]));
using var stream=File.Create(args[2]);using var writer=new Utf8JsonWriter(stream);
writer.WriteStartObject();writer.WriteStartArray("table");foreach(float v in table.Values.Span)writer.WriteNumberValue(v);writer.WriteEndArray();
writer.WriteStartArray("cases");int cases=0,removed=0,queries=0,lastRetained=0;
for(int iteration=0;iteration<24;iteration++)foreach(int threshold in new[]{0,20480,0x4000000})
{
 byte[] ownedResource=resource.ToArray();int sampler=11506;
 int fields=sampler+8+BinaryPrimitives.ReadInt32LittleEndian(ownedResource.AsSpan(sampler+4))-214;
 BinaryPrimitives.WriteInt32LittleEndian(ownedResource.AsSpan(fields+202),threshold);
 //The later195520 cap must not hide a194140 rejection.
 BinaryPrimitives.WriteInt32LittleEndian(ownedResource.AsSpan(fields+198),iteration%2==0?0:0x4000000);
 ownedResource[12727+16]=(byte)(3+iteration%5);
 float scale=new[]{.5f,1f,2f}[iteration%3],distance=iteration%4*80;
 InkStroke[] raw=[Draw((0,90),(0,10),(25,60),(50,10),(50,90)),
  Draw((65+distance,90),(65+distance,10)),Draw((65+distance,90),(115+distance,10)),
  Draw((115+distance,90),(115+distance,10)),Draw((130+distance,90),(130+distance,10),(175+distance,10),(185+distance,20),(185+distance,45),(175+distance,55),(130+distance,55))];
 raw=raw.Select(s=>new InkStroke(s.Points.SelectMany(p=>iteration%2==0?new[]{p}:new[]{p,p})
  .Select(p=>new InkPoint(p.X*scale+iteration*10,p.Y*scale-iteration*7)).ToArray())).ToArray();
 var factory=LegacyFreshFirstResultOwnershipFactory.Prepare(ownedResource,6120,raw);var row=factory.Frame.Row;var cuts=row.GenerateRussianCuts();
 var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,
  LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(ownedResource,12727),new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());
 var output=LegacyFreshFirstCandidateGraphPreparer.Prepare(ownedResource,factory,cuts,seeds,table);
 var original=LegacyOrdinaryGraphSourceRanges.Prepare(seeds,cuts.Chronology,row.OriginalSource,output.SpanProfile.MaximumAtoms);
 var before=original.Graph;var after=output.SourceRanges.Graph;
 Require(after.MaximumAtoms==3+iteration%5&&output.ResourceGraphOffset==12727,"Owned resource span cap was replaced with a diagnostic constant.");
 var profile=LegacyOrdinarySamplerGroupPruningProfile.ReadV106Node(ownedResource,sampler);
 var local=LegacyOrdinarySamplerGroupPruning.Prepare(before,cuts.Cuts.PreparedInk,row.Frame.Line.Height,profile,table);
 Require(local.Graph.GetNode(before.NodeCount-1).Count==before.GetNode(before.NodeCount-1).Count,"Explicit legacy local API must retain final node.");
 Require(after.GetNode(after.NodeCount-1).Count<=local.Graph.GetNode(before.NodeCount-1).Count,"Complete pass cannot add tail arcs.");
 for(int n=0;n<before.NodeCount-1;n++)Require(output.Pruning.OriginalArcOrdinals[n].Span.SequenceEqual(local.OriginalArcOrdinals[n].Span),"Complete pass changed earlier-node provenance.");
 for(int n=0;n<after.NodeCount;n++)for(int a=0;a<after.GetNode(n).Count;a++)
 {
  int old=output.Pruning.OriginalArcOrdinals[n].Span[a];
  Require(original.ResolveCandidate(n,old).SequenceEqual(output.SourceRanges.ResolveCandidate(n,a)),"Pruning lost native raw source-range ownership.");queries++;
 }
 Reject(()=>original.WithSamplerPruning(output.Pruning));
 Reject(()=>LegacyFreshFirstCandidateGraphPreparer.Prepare(ownedResource,factory,cuts,seeds,table,new CancellationToken(true)));
 var badResource=ownedResource.ToArray();badResource[12727+16]^=1;
 Reject(()=>LegacyFreshFirstCandidateGraphPreparer.Prepare(badResource,factory,cuts,seeds,table));
 writer.WriteStartObject();writer.WriteNumber("height",row.Frame.Line.Height);writer.WriteNumber("gap",profile.Separation.GapOffset);
 writer.WriteNumber("slope",profile.Separation.Slope);writer.WriteNumber("threshold",threshold);writer.WriteNumber("laterCap",profile.Separation.MaximumCost);
 writer.WriteNumber("maximumAtoms",after.MaximumAtoms);writer.WriteNumber("removed",output.Pruning.RemovedArcCount);
 writer.WriteStartArray("strokes");int at=0;
 foreach(int count in cuts.Cuts.PreparedInk.StrokePointCounts.Span)
 {writer.WriteStartArray();for(int p=0;p<count;p++){var point=cuts.Cuts.PreparedInk.Points.Span[at++];writer.WriteStartArray();writer.WriteNumberValue(point.X);writer.WriteNumberValue(point.Y);writer.WriteEndArray();}writer.WriteEndArray();}writer.WriteEndArray();
 writer.WriteStartArray("groups");foreach(var g in before.Groups){writer.WriteStartArray();foreach(var r in g.Ranges.Span){writer.WriteStartArray();writer.WriteNumberValue(r.StrokeIndex);writer.WriteNumberValue(r.StartPoint);writer.WriteNumberValue(r.EndPointExclusive);writer.WriteEndArray();}writer.WriteEndArray();}writer.WriteEndArray();
 WriteGraph("before",before);WriteGraph("after",after);WriteGraph("local",local.Graph);
 writer.WriteNumber("localRemoved",local.RemovedArcCount);writer.WriteStartArray("localOrdinals");foreach(var list in local.OriginalArcOrdinals){writer.WriteStartArray();foreach(int ordinal in list.Span)writer.WriteNumberValue(ordinal);writer.WriteEndArray();}writer.WriteEndArray();
 writer.WriteStartArray("originalOrdinals");foreach(var list in output.Pruning.OriginalArcOrdinals){writer.WriteStartArray();foreach(int ordinal in list.Span)writer.WriteNumberValue(ordinal);writer.WriteEndArray();}writer.WriteEndArray();
 writer.WriteEndObject();cases++;removed+=output.Pruning.RemovedArcCount;lastRetained+=after.GetNode(after.NodeCount-1).Count;
}
writer.WriteEndArray();writer.WriteEndObject();writer.Flush();
Require(removed>0,"Actual native eligibility pass was never exercised.");
Console.WriteLine($"Prepared{cases} actual first owned graph/cap/raw-separation prune cases; {removed} rejected arcs, {queries} raw-source callbacks, {lastRetained} final-node arcs retained. Run independent mutable-graph/scalar audit; this is not a completed OCR winner.");
void WriteGraph(string name,LegacyOrdinaryCursiveCandidateGraph graph)
{
 writer.WriteStartArray(name);for(int n=0;n<graph.NodeCount;n++){writer.WriteStartArray();foreach(var a in graph.GetNode(n))
 {writer.WriteStartArray();writer.WriteNumberValue(a.GroupId);writer.WriteNumberValue(a.Span);writer.WriteNumberValue(a.Flags);writer.WriteNumberValue(a.StartContextId);writer.WriteNumberValue(a.EndContextId);
 writer.WriteStartArray();foreach(int p in a.PredecessorArcIndices.Span)writer.WriteNumberValue(p);writer.WriteEndArray();
 writer.WriteStartArray();foreach(var s in a.Successors){writer.WriteStartArray();writer.WriteNumberValue(s.NodeOffset);writer.WriteNumberValue(s.ArcIndex);writer.WriteEndArray();}writer.WriteEndArray();writer.WriteEndArray();}writer.WriteEndArray();}writer.WriteEndArray();
}
void Require(bool value,string message){if(!value)throw new Exception(message);}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException or InvalidDataException){return;}throw new Exception("Foreign/unsupported owner admitted.");}
static InkStroke Draw(params(float X,float Y)[] p)=>new(p.Select(x=>new InkPoint(x.X,x.Y)).ToArray());
