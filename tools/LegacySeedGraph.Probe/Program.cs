using System.Text.Json;
using InkRecognition;
if(args.Length!=2)throw new ArgumentException("controls.json acoustic.res");
var acoustic=File.ReadAllBytes(args[1]);var profile=LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(acoustic,189379);
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[0]));int cases=0,arcsChecked=0,pathsChecked=0;
InkStroke Stroke(float minX,float maxX)=>new(new[]{new InkPoint(minX,10),new InkPoint(maxX,90)});
LegacyOrdinaryCursiveSeedResult Seeds(IReadOnlyList<InkStroke> source,bool actualCuts=false)
{
 var prepared=LegacyCursivePreparedInk.Create(source);
 var markers=Enumerable.Range(0,source.Count).Select(i=>new LegacyCursiveCutMarker(0,i,0)).ToArray();
 var cuts=actualCuts ? LegacyCursiveCutProducer.GenerateRussianVersion106(source,90,80,0) :
    new LegacyCursiveCutResult(prepared,markers,LegacyCursiveInkAtoms.Build(prepared.StrokePointCounts.Span,markers));
 return LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,80,profile,new byte[source.Count],Enumerable.Repeat(-1,source.Count).ToArray());
}
foreach(var control in controls.RootElement.EnumerateArray())
{
 int count=control.GetProperty("count").GetInt32(),selected=control.GetProperty("selected").GetInt32();
 var source=Enumerable.Range(0,count).Select(i=>Stroke(100*i,100*i+20)).Append(Stroke(100*selected+9,100*selected+9)).ToArray();
 var seeds=Seeds(source);Require(seeds.DeferredCount==1 && seeds.Seeds[selected+1].Deferred,"Controlled actual geometry insertion slot differs.");
 var graph=LegacyOrdinaryCursiveSeedGraph.CreateSingleDeferred(seeds);
 var reference=control.GetProperty("arcs");Require(graph.NodeCount==reference.GetArrayLength(),"Graph count differs.");
 var refs=new byte[graph.NodeCount];
 for(int node=0;node<graph.NodeCount;node++)
 {
  var expected=reference[node];var actual=graph.GetNode(node);Require(actual.Count==expected.GetArrayLength(),"Native seed arc count differs.");
  for(int ordinal=0;ordinal<actual.Count;ordinal++)
  {
   var arc=actual[ordinal];var test=expected[ordinal];refs[arc.SeedGroupIndex]++;
   Require(arc.SeedGroupIndex==test.GetProperty("group").GetInt32() && arc.Flags==test.GetProperty("flags").GetInt32() &&
    arc.StartContextId==test.GetProperty("start").GetUInt32() && arc.EndContextId==test.GetProperty("end").GetUInt32(),"Native graph group/flags/context differs.");
   Require(arc.PredecessorArcIndices.Span.ToArray().Order().SequenceEqual(test.GetProperty("pred").EnumerateArray().Select(x=>x.GetInt32())) &&
    arc.SuccessorArcIndices.Span.ToArray().Order().SequenceEqual(test.GetProperty("succ").EnumerateArray().Select(x=>x.GetInt32())),"Native reciprocal seed adjacency differs.");
   if(arc.SeedGroupIndex==selected+1)Require(arc.PlacementOffset==node-(selected+1),"Native signed placement offset differs.");
   arcsChecked++;
  }
 }
 Require(graph.SeedReferenceCounts.Span.SequenceEqual(refs),"Native seed reference counts differ.");
 var found=Enumerate(graph).Order().ToArray();var expectedPaths=control.GetProperty("paths").EnumerateArray().Select(p=>string.Join(",",p.EnumerateArray().Select(x=>x.GetInt32()))).Order().ToArray();
 Require(found.SequenceEqual(expectedPaths),"Native graph complete paths differ from independent stable moves.");pathsChecked+=found.Length;cases++;
}
var own=new[]{new InkStroke(new[]{new InkPoint(0,90),new InkPoint(0,10),new InkPoint(25,60),new InkPoint(50,10),new InkPoint(50,90)}),
 new InkStroke(new[]{new InkPoint(70,10),new InkPoint(70,90),new InkPoint(120,10),new InkPoint(120,90)}),
 new InkStroke(new[]{new InkPoint(140,90),new InkPoint(140,10),new InkPoint(180,10),new InkPoint(190,20),new InkPoint(190,40),new InkPoint(180,50),new InkPoint(140,50)}),Stroke(10,20)};
var actualSeeds=Seeds(own,true);Require(actualSeeds.DeferredCount==1,"Real late-cross fixture lacks one deferred seed.");
var actualGraph=LegacyOrdinaryCursiveSeedGraph.CreateSingleDeferred(actualSeeds);
var actualPaths=Enumerate(actualGraph);Require(actualPaths.Count is >=2 and <=3,"Real late-cross native graph path count differs.");
foreach(string path in actualPaths)Require(path.Split(',').Select(int.Parse).Order().SequenceEqual(Enumerable.Range(0,actualGraph.NodeCount)),"Native late-cross path lost/duplicated prepared ink.");
int overlap=0;
for(int node=0;node<actualGraph.NodeCount;node++)foreach(var arc in actualGraph.GetNode(node))if((arc.Flags&4)==0)overlap++;
Require(overlap>0,"Native physical-stroke overlap stage was not reached on actual cut atoms.");
var ordinarySeeds=Seeds(own.Take(3).ToArray(),true);
var ordinaryGraph=LegacyOrdinaryCursiveSeedGraph.CreateSingleDeferred(ordinarySeeds);
Require(ordinarySeeds.DeferredCount==0 && ordinaryGraph.ArcCount==ordinaryGraph.NodeCount && Enumerate(ordinaryGraph).Count==1,"Native nondeferred seed graph changed linear topology.");
for(int node=0;node<ordinaryGraph.NodeCount;node++)
{
 var arc=ordinaryGraph.GetNode(node).Single();
 var prefix=ordinarySeeds.Seeds.Take(node).Select(s=>s.Atom.Range.StrokeIndex).ToHashSet();
 var suffix=ordinarySeeds.Seeds.Skip(node).Select(s=>s.Atom.Range.StrokeIndex).ToHashSet();
 Require(arc.Flags==(prefix.Overlaps(suffix)?0:4) && arc.StartContextId==(uint)(node*128) && arc.EndContextId==(uint)(node*128+3),"Native nondeferred overlap/context identities differ.");
}
var multiple=Seeds(new[]{Stroke(0,20),Stroke(100,120),Stroke(200,220),Stroke(1,2),Stroke(3,4)});Require(multiple.DeferredCount>1,"Multi-deferred rejection fixture invalid.");
try{LegacyOrdinaryCursiveSeedGraph.CreateSingleDeferred(multiple);throw new Exception("Unverified multiple deferred graph accepted.");}catch(NotSupportedException){}
try{LegacyOrdinaryCursiveSeedGraph.CreateSingleDeferred(actualSeeds,new CancellationToken(true));throw new Exception("Graph cancellation ignored.");}catch(OperationCanceledException){}
Console.WriteLine($"PASS {cases} independent path-oracle controls, {arcsChecked} arcs, {pathsChecked} complete paths; actual RU cut→seed→single-deferred graph ownership and overlap ({overlap} flags cleared). Native merged-glyph candidate search remains separate.");
static List<string> Enumerate(LegacyOrdinaryCursiveSeedGraph graph)
{
 var result=new List<string>();var prefix=new List<int>();
 void Walk(int node,int ordinal)
 {
  var arc=graph.GetNode(node)[ordinal];prefix.Add(arc.SeedGroupIndex);
  if(node==graph.NodeCount-1)result.Add(string.Join(",",prefix));
  else foreach(int successor in arc.SuccessorArcIndices.Span)Walk(node+1,successor);
  prefix.RemoveAt(prefix.Count-1);
 }
 for(int i=0;i<graph.GetNode(0).Count;i++)Walk(0,i);
 return result;
}
static void Require(bool okay,string message){if(!okay)throw new InvalidDataException(message);}
