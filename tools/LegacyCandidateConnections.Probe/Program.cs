using InkRecognition;
using System.Text.Json;
if(args.Length is not (1 or 4))throw new ArgumentException("Usage: LegacyCandidateConnections.Probe independent-candidate-controls.json [ru-ak.res engine-profile.json separation-profile.json]");
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[0]));var toyProfile=new LegacyOrdinaryCursiveSeedProfile(true,1,.3f,.3f);
int cases=0,links=0,paths=0;
foreach(var item in controls.RootElement.GetProperty("cases").EnumerateArray())
{
 int n=item.GetProperty("count").GetInt32(),selected=item.GetProperty("selected").GetInt32(),cap=item.GetProperty("cap").GetInt32();
 var raw=Enumerable.Range(0,n).Select(i=>Stroke(i*100,i*100+20)).Append(Stroke(selected*100+9,selected*100+9)).ToArray();
 var prepared=LegacyCursivePreparedInk.Create(raw);var markers=Enumerable.Range(0,raw.Length).Select(i=>new LegacyCursiveCutMarker(0,i,0)).ToArray();
 var cuts=new LegacyCursiveCutResult(prepared,markers,LegacyCursiveInkAtoms.Build(prepared.StrokePointCounts.Span,markers));
 var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,80,toyProfile,new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());
 var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds),cap);
 var connection=new LegacyOrdinaryCandidateConnections(graph);
 var expected=item.GetProperty("partitions").EnumerateArray().Select(v=>v.GetString()!).ToHashSet();var actual=new HashSet<string>();var parts=new List<string>();
 void Walk(int node,ushort ordinal)
 {
  var arc=graph.GetNode(node)[ordinal];parts.Add(string.Join(",",graph.Groups[arc.GroupId].SeedGroupIndices.ToArray().Order()));
  if(node==graph.NodeCount-1)actual.Add(string.Join("|",parts));
  else for(byte span=0;span<cap&&node+span+1<graph.NodeCount;span++)
   for(ushort nth=0;nth<connection.GetSuccessorCount(node,ordinal,span);nth++)Walk(node+span+1,connection.GetSuccessorOrdinal(node,ordinal,span,nth));
  parts.RemoveAt(parts.Count-1);
 }
 for(byte span=0;span<cap&&span<graph.NodeCount;span++)
 {
  int count=connection.GetSuccessorCount(-1,ushort.MaxValue,span);
  if(count!=connection.GetSuccessorCount(-1,17,span))throw new Exception("Source root incorrectly used supplied arc ordinal.");
  for(ushort nth=0;nth<count;nth++)Walk(span,connection.GetSuccessorOrdinal(-1,ushort.MaxValue,span,nth));
  if(connection.GetSuccessorOrdinal(-1,0,span,(ushort)count)!=ushort.MaxValue)throw new Exception("Missing nth root ordinal lost FFFF.");
 }
 if(!actual.SetEquals(expected))throw new Exception("Successor selector/root paths differ from exhaustive independent stable partitions.");
 for(int node=0;node<graph.NodeCount;node++)for(ushort ordinal=0;ordinal<graph.GetNode(node).Count;ordinal++)for(byte span=0;span<15;span++)
 {
  var wanted=graph.GetNode(node)[ordinal].Successors.Where(v=>v.NodeOffset==span+1).Select(v=>v.ArcIndex).ToArray();
  var found=connection.GetSuccessorOrdinals(node,ordinal,span);
  if(!found.Span.SequenceEqual(wanted)||connection.GetSuccessorOrdinal(node,ordinal,span,(ushort)wanted.Length)!=ushort.MaxValue)throw new Exception("Native append list order or missing nth sentinel changed.");
  links+=found.Length;
 }
 using var cancel=new CancellationTokenSource();cancel.Cancel();try{connection.GetSuccessorCount(-1,0,0,cancel.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
 ExpectInvalid(()=>connection.GetSuccessorCount(-2,0,0));ExpectInvalid(()=>connection.GetSuccessorCount(-1,0,15));ExpectInvalid(()=>connection.GetSuccessorCount(0,ushort.MaxValue,0));
 cases++;paths+=actual.Count;
}
Console.WriteLine($"PASS {cases} independent full partition references through native successorselector/span +source root; {links} ordered links/{paths} complete partitions.");
if(args.Length==4)
{
 var resource=File.ReadAllBytes(args[1]);using var engine=JsonDocument.Parse(File.ReadAllBytes(args[2]));var a=engine.RootElement.GetProperty("activations");
 var activation=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
 InkStroke[] mir=[Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((65,90),(65,10)),Draw((65,90),(115,10)),Draw((115,90),(115,10)),Draw((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))];
 var session=LegacyFreshFirstGlyphSession.Prepare(resource,6120,mir,activation);
 var profile=LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,12727);
 var source=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(session.Cuts.Cuts,session.Row.Frame.Line.Height,profile,new byte[mir.Length],Enumerable.Repeat(-1,mir.Length).ToArray());
 var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(source,LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(source),5);
 var connection=new LegacyOrdinaryCandidateConnections(graph);var mask=Enumerable.Repeat((byte)1,session.Scorer.Alphabet.LabelCount).ToArray();
 var sep=LegacyCursiveSeparationProfile.ReadV106Node(resource,11506);var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
 var categories=new LegacyCursiveCategoryMapper(LegacyCursiveCategoryProfile.Load(resource.AsSpan(11571),11571,out _));
 int rows=0;
 for(byte span=0;span<5&&span<graph.NodeCount;span++)
  for(ushort index=0;index<connection.GetSuccessorCount(-1,ushort.MaxValue,span);index++)
  {
   ushort ordinal=connection.GetSuccessorOrdinal(-1,ushort.MaxValue,span,index);var arc=graph.GetNode(span)[ordinal];var ranges=graph.Groups[arc.GroupId].Ranges;
   var scores=session.Score(ranges.Span,0,15,mask,[]);
   int geometry=LegacyCursiveGroupSeparationCost.Evaluate(session.Cuts.Cuts.PreparedInk,ranges.Span,session.Row.Frame.Line.Height,sep.GapOffset,sep.Slope,table.Values.Span,sep.MaximumCost);
   var ranked=new LegacyOrdinaryRankedAcousticGroup(scores.Cache,session.Scorer.Alphabet,resource.AsSpan(session.Scorer.AlphabetOffset,16),Enumerable.Repeat(geometry,categories.CompositeCategoryCount).ToArray(),1);
   foreach(var entry in ranked.Entries.Span)
   {
    int cat=categories.GetSymbolCompositeCategory(entry.Symbol);if(cat<0)continue;
    if(ranked.Query(entry.Symbol,ushort.MaxValue,cat,false,false)!=entry.Cost+geometry)throw new Exception("Actual initial root glyph query didn't consume prepared owned group/cache/category.");
   }
   rows++;
  }
 if(rows<5)throw new Exception("Actual root selector didn't reach variable prepared first-model groups.");
 Console.WriteLine($"PASS actual first24724a/265/297->first21b12727 seeds/candidates->root selector->context/NN/cache/ranked query {rows} variable root glyph groups; no wholetext beam/winner.");
}
static InkStroke Stroke(float x,float max)=>new(new[]{new InkPoint(x,10),new InkPoint(max,90)});
static InkStroke Draw(params(float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
static void ExpectInvalid(Action action){try{action();}catch(ArgumentException){return;}throw new Exception("Invalid successor getter accepted.");}
