using InkRecognition;
using System.Text.Json;
if(args.Length is not (1 or 3 or 4))throw new ArgumentException("controls.json [AK.res separation-profile.json [actual-capture.json]]");
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[0]));var table=controls.RootElement.GetProperty("table").EnumerateArray().Select(v=>v.GetSingle()).ToArray();int tested=0;
foreach(var c in controls.RootElement.GetProperty("cases").EnumerateArray())
{
 int v=LegacyCursiveBlankGapCost.Evaluate(c.GetProperty("low").GetSingle(),c.GetProperty("high").GetSingle(),c.GetProperty("prior").GetSingle(),c.GetProperty("height").GetSingle(),c.GetProperty("threshold").GetSingle(),c.GetProperty("slope").GetSingle(),c.GetProperty("maximum").GetBoolean(),table);
 if(v!=c.GetProperty("cost").GetInt32())throw new Exception($"Independent blankcache10 control{tested} differed: {v}.");tested++;
}
if(LegacyCursiveBlankGapCost.Evaluate(float.MaxValue,float.MaxValue,-float.MaxValue,1,0,1,true,table)!=0x4000000||
 LegacyCursiveBlankGapCost.Evaluate(-float.MaxValue,-float.MaxValue,float.MaxValue,1,0,1,false,table)!=0)
 throw new Exception("Native infinite float-tail cap/zero branch changed.");
try{LegacyCursiveBlankGapCost.Evaluate(0,1,0,0,0,1,false,table);throw new Exception("Zero line height accepted.");}catch(ArgumentException){}
try{LegacyCursiveBlankGapCost.Evaluate(1,0,0,1,0,1,false,table);throw new Exception("Inverted geometry accepted.");}catch(ArgumentException){}
Console.WriteLine($"PASS {tested} independent blank cache10 scalar controls +extreme float tails; min/max selector, float lookup, rounding and impossible cap.");
if(args.Length>=3)
{
 byte[] resource=File.ReadAllBytes(args[1]);var separation=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));
 using var capture=args.Length==4?File.Create(args[3]):null;using var w=capture==null?null:new Utf8JsonWriter(capture);w?.WriteStartArray();
 InkStroke[][] cases=[[Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((65,90),(65,10)),Draw((65,90),(115,10)),Draw((115,90),(115,10)),Draw((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))],[Draw((0,0),(60,0),(60,80),(0,80),(0,0)),Draw((20,30),(30,50))],[Draw((0,0),(0,80)),Draw((10000,0),(10000,80)),Draw((20000,0),(20000,80))]];
 int arcCount=0,accepted=0,rejected=0;
 foreach(var raw in cases)
 {
  var factory=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,6120,raw);var cuts=factory.Frame.Row.GenerateRussianCuts();var profile=LegacyFirstTransitionGeometryProfile.ReadSource(resource,6120);
  int go=profile.SamplerOffset+8+BitConverter.ToInt32(resource,profile.SamplerOffset+4);var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,factory.Frame.Row.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,go),new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());
  var prepared=LegacyFreshFirstCandidateGraphPreparer.Prepare(resource,factory,cuts,seeds,separation);var graph=prepared.SourceRanges.Graph;
  var geometry=new LegacyFirstBlankTransitionGeometry(resource,factory,cuts,prepared,separation);
  var foreign=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,6120,raw);
  try{_ = new LegacyFirstBlankTransitionGeometry(resource,foreign,cuts,prepared,separation);throw new Exception("Equal-valued foreign owned frame accepted.");}catch(ArgumentException){}
  using(var canceled=new CancellationTokenSource()){canceled.Cancel();try{_ = new LegacyFirstBlankTransitionGeometry(resource,factory,cuts,prepared,separation,canceled.Token);throw new Exception("Prepare cancellation ignored.");}catch(OperationCanceledException){}}
  Console.WriteLine($"Actual blank70={geometry.Threshold70},10C={geometry.RequireWholePhysicalStarts10C},category={geometry.BlankCompositeCategory}.");
  if(geometry.Threshold70!=-.5f||!geometry.RequireWholePhysicalStarts10C||profile.Categories.CompositeCategories.Span[geometry.BlankCompositeCategory].FirstId!=6)throw new Exception("Actual first blank profile70/10C/category changed.");
  if(w!=null)
  {
   w.WriteStartObject();w.WriteNumber("height",factory.Frame.Row.Frame.Line.Height);w.WriteNumber("threshold",geometry.Threshold70);w.WriteNumber("slope",profile.GapSlope);w.WriteBoolean("maximum",geometry.RequireWholePhysicalStarts10C);
   w.WriteNumber("pairScale",profile.PairScale);w.WriteNumber("pairOffset",profile.PairOffset);w.WriteNumber("pairMaximum",profile.PairMaximum);
   void Floats(string name,IEnumerable<float> values){w.WriteStartArray(name);foreach(float v in values)w.WriteNumberValue(v);w.WriteEndArray();}
   Floats("table",separation.Values.ToArray());var cg=profile.Categories.Prototypes.Span[profile.Categories.CompositeCategories.Span[geometry.BlankCompositeCategory].GeometryIndex];var pg=profile.Categories.Prototypes.Span[profile.Categories.CompositeCategories.Span[0].GeometryIndex];Floats("cp",[cg.First,cg.Second]);Floats("pp",[pg.First,pg.Second]);
   w.WriteStartArray("points");foreach(var p in cuts.Cuts.PreparedInk.Points.Span){w.WriteStartArray();w.WriteNumberValue(p.X);w.WriteNumberValue(p.Y);w.WriteEndArray();}w.WriteEndArray();
   w.WriteStartArray("counts");foreach(int c in cuts.Cuts.PreparedInk.StrokePointCounts.Span)w.WriteNumberValue(c);w.WriteEndArray();
   w.WriteStartArray("groups");foreach(var g in graph.Groups){w.WriteStartArray();foreach(var r in g.Ranges.Span){w.WriteStartArray();w.WriteNumberValue(r.StrokeIndex);w.WriteNumberValue(r.StartPoint);w.WriteNumberValue(r.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();
   w.WriteStartArray("nodes");for(int n=0;n<graph.NodeCount;n++){w.WriteStartArray();for(ushort a=0;a<graph.GetNode(n).Count;a++){var arc=graph.GetNode(n)[a];w.WriteStartObject();w.WriteNumber("span",arc.Span);w.WriteNumber("group",arc.GroupId);w.WriteNumber("blankCost",geometry.ReadPreparedBlankCost(n,a));w.WriteStartArray("pred");foreach(int p in arc.PredecessorArcIndices.Span)w.WriteNumberValue(p);w.WriteEndArray();w.WriteEndObject();}w.WriteEndArray();}w.WriteEndArray();w.WriteStartArray("calls");
  }
  for(int n=0;n<graph.NodeCount;n++)for(ushort a=0;a<graph.GetNode(n).Count;a++)
  {
   var arc=graph.GetNode(n)[a];if(n==arc.Span)continue;
   var result=geometry.Correct118(n,a,0);arcCount++;if(result.Accepted)accepted++;else rejected++;
   using(var canceled=new CancellationTokenSource()){canceled.Cancel();try{geometry.Correct118(n,a,0,canceled.Token);throw new Exception("Kind5 cancellation ignored.");}catch(OperationCanceledException){}}
   if(w!=null){w.WriteStartArray();w.WriteNumberValue(118);w.WriteNumberValue(n);w.WriteNumberValue(a);w.WriteNumberValue(-1);w.WriteNumberValue(0);w.WriteNumberValue(result.Cost);w.WriteBooleanValue(result.Accepted);w.WriteEndArray();}
   if(result.Accepted&&result.Cost!=geometry.ReadPreparedBlankCost(n,a))throw new Exception("Blank118 introduced an ordinary translation term.");
   int pn=n-arc.Span-1;
   foreach(int p in arc.PredecessorArcIndices.Span)
   {
    var pair=geometry.Correct140(n,a,pn,(ushort)p,22,0,0);
    if(w!=null){w.WriteStartArray();w.WriteNumberValue(140);w.WriteNumberValue(n);w.WriteNumberValue(a);w.WriteNumberValue(p);w.WriteNumberValue(22);w.WriteNumberValue(pair.Cost);w.WriteBooleanValue(pair.Accepted);w.WriteEndArray();}
    foreach(int v in new[]{int.MinValue,-100,0,0x4000000-1,0x4000000,int.MaxValue})
      if(geometry.Correct140(n,a,pn,(ushort)p,189,0,v)!=new LegacyFirstOrdinaryTransitionResult(v<0x4000000,v))throw new Exception("PreviousFFFD bypass/limit lost.");
    try{geometry.Correct140(n,a,pn,(ushort)p,ushort.MaxValue,0,0);throw new Exception("Foreign symbol label accepted.");}catch(ArgumentException){}
   }
  }
  if(w!=null){w.WriteEndArray();w.WriteEndObject();}
  Console.WriteLine($"Actual first pruned source cap{prepared.SpanProfile.MaximumAtoms}: {prepared.Pruning.RemovedArcCount} removedarcs, {graph.CandidateCount} remaining; kind5 preparedprofile/maxX+whole-start dispatch.");
 }
 w?.WriteEndArray();w?.Flush();
 if(arcCount==0||accepted==0||rejected==0)throw new Exception("Actual blank profile controls did not exercise both whole-start outcomes.");
 Console.WriteLine($"PASS actual{arcCount} nonroot pruned blank118 controls: {accepted} accepted/{rejected} rejected; pair140 evaluated with explicit previousgeometry. Not blank fallback/ancestor/nativewinner orchestration.");
}
static InkStroke Draw(params(float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
