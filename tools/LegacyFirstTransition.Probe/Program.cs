using InkRecognition;
using System.Text.Json;
if(args.Length is not (1 or 4 or 5))throw new ArgumentException("Usage: LegacyFirstTransition.Probe controls.json [ru-ak.res engine-profile.json separation-profile.json [actual-audit.json]]");
using var json=JsonDocument.Parse(File.ReadAllBytes(args[0]));var table=json.RootElement.GetProperty("table").EnumerateArray().Select(v=>v.GetSingle()).ToArray();
int pairCases=0,negative=0,logs=0;
foreach(var c in json.RootElement.GetProperty("pairs").EnumerateArray())
{
 var pa=c.GetProperty("pa");var pb=c.GetProperty("pb");
 int cost=LegacyFirstOrdinaryTransitionGeometry.PairCost(Box(c.GetProperty("a")),Box(c.GetProperty("b")),new(pa[0].GetSingle(),pa[1].GetSingle(),0,false),new(pb[0].GetSingle(),pb[1].GetSingle(),0,false),c.GetProperty("height").GetSingle(),c.GetProperty("scale").GetSingle(),c.GetProperty("offset").GetSingle(),c.GetProperty("cap").GetSingle());
 if(cost!=c.GetProperty("cost").GetInt32())throw new Exception($"Independent pair residual/nearest rounding differed at{pairCases}: {cost} !={c.GetProperty("cost")}.");
 pairCases++;if(cost<0)negative++;
}
foreach(var c in json.RootElement.GetProperty("logs").EnumerateArray())
{
 foreach(bool bitRounded in new[]{false,true})
  if(LegacyFirstOrdinaryTransitionGeometry.LogCost(c.GetProperty("x").GetSingle(),table,bitRounded)!=c.GetProperty("cost").GetInt32())throw new Exception("Independent float/double lookup/nearest round/tail differed.");
 logs++;
}
if(negative==0)throw new Exception("Own fixtures missed native negative pair cost.");
try{LegacyFirstOrdinaryTransitionGeometry.LogCost(float.NaN,table,true);throw new Exception("Nonfinite gap accepted.");}catch(ArgumentException){}
try{LegacyFirstOrdinaryTransitionGeometry.PairCost(default,default,default,default,0,1,0,1);throw new Exception("Zero height accepted.");}catch(ArgumentException){}
Console.WriteLine($"PASS {pairCases} independent geometry pair controls ({negative} negative), {logs} lookup/tail controls through both rounding paths.");
if(args.Length>=4)
{
 var resource=File.ReadAllBytes(args[1]);using var engine=JsonDocument.Parse(File.ReadAllBytes(args[2]));var activationJson=engine.RootElement.GetProperty("activations");
 var activations=new LegacyInkActivations(activationJson.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),activationJson.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
 var geometryProfile=LegacyFirstTransitionGeometryProfile.ReadSource(resource,6120);var separation=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
 if(geometryProfile.SamplerOffset!=11506||geometryProfile.PairScale!=1||geometryProfile.PairOffset!=.5f||geometryProfile.PairMaximum!=1.5f||geometryProfile.DefaultGap!=1.5f||geometryProfile.GapSlope!=3)throw new Exception("Actual first sampler ownership/tail field offsets changed.");
 var invalid=(byte[])resource.Clone();invalid[12513+8]=0;
 try{LegacyFirstTransitionGeometryProfile.ReadSource(invalid,6120);throw new Exception("Disabled pair geometry accepted as the enabled ordinary branch.");}catch(NotSupportedException){}
 invalid=(byte[])resource.Clone();System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(invalid.AsSpan(12513+206),-1);
 try{LegacyFirstTransitionGeometryProfile.ReadSource(invalid,6120);throw new Exception("Negative geometry cap accepted.");}catch(InvalidDataException){}
 try{LegacyFirstTransitionGeometryProfile.ReadSource(resource.AsSpan(0,12000),6120);throw new Exception("Truncated parent source accepted.");}catch(InvalidDataException){}
 InkStroke[] ownMir=[Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((65,90),(65,10)),Draw((65,90),(115,10)),Draw((115,90),(115,10)),Draw((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))];
 using var auditStream=args.Length==5?File.Create(args[4]):null;
 using var audit=auditStream==null?null:new Utf8JsonWriter(auditStream);
 if(audit!=null){audit.WriteStartArray();}
 int fixtureCount=0,totalCallbacks=0,totalContinuations=0;
 foreach((float scale,float dx,float dy) in new[]{(1f,0f,0f),(1.75f,-100f,250f),(.7f,70f,-20f)})
 {
  var raw=ownMir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*scale+dx,p.Y*scale+dy)).ToArray())).ToArray();
  var frame=LegacyFreshOrdinaryPacketFrame.PrepareFirstSource(resource,6120,raw);var row=frame.Row;var cuts=row.GenerateRussianCuts();
  var policy=LegacyFreshFirstControllerPolicy.ReadFirstSource(resource,6120);var scope=new LegacyFirstRowPrimitiveScope(row,cuts,raw.Select(s=>s.Points.Count).ToArray());
  var scorer=LegacyFirstOrdinaryCacheScorer.Prepare(resource,6120,row,activations);var mask=Enumerable.Repeat((byte)1,scorer.Alphabet.LabelCount).ToArray();
  var seed=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,12727),new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());
  var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(seed,LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(seed),5);var links=new LegacyOrdinaryCandidateConnections(graph);
  var geometry=new LegacyFirstOrdinaryTransitionGeometry(geometryProfile,frame,cuts,graph,separation);var categories=new LegacyCursiveCategoryMapper(geometryProfile.Categories);
  var sep=LegacyCursiveSeparationProfile.ReadV106Node(resource,11506);var rankedGroups=new LegacyOrdinaryRankedAcousticGroup[graph.Groups.Count];
  for(int id=0;id<graph.Groups.Count;id++)
  {
   var ranges=graph.Groups[id].Ranges.ToArray();int[] indices=cuts.Chronology.OrderRangeIndicesForNormalization(ranges);var norm=indices.Select(i=>ranges[i]).ToArray();
   var boundary=LegacyCursiveGlyphBoundaryPolicy.Resolve(norm,cuts.Cuts.PreparedInk.StrokePointCounts.Span,0,false,false);
   var scores=scorer.Score(cuts,ranges,boundary.Start,boundary.End,15,Math.Min(256,policy.MaximumNormalizedStrokes),mask,glyph=>policy.CreateContext(scope.Evaluate(ranges,glyph.MergedGeometry.Strokes.Count),[]),[]);
   int categoryCost=LegacyCursiveGroupSeparationCost.Evaluate(cuts.Cuts.PreparedInk,ranges,row.Frame.Line.Height,sep.GapOffset,sep.Slope,separation.Values.Span,sep.MaximumCost);
   rankedGroups[id]=new(scores.Cache,scorer.Alphabet,resource.AsSpan(scorer.AlphabetOffset,16),Enumerable.Repeat(categoryCost,categories.CompositeCategoryCount).ToArray(),1);
  }
  var requests=Enumerable.Range(0,graph.NodeCount).Select(_=>new List<LegacyFirstOrdinaryGlyphRequest>()).ToArray();
  //1ef070: descending span order; native start/end categories are separate.
  for(int node=0;node<graph.NodeCount;node++)for(byte span=(byte)Math.Min(4,node);;span--)
  {
   var arcs=graph.GetNode(node);
   for(int ordinal=0;ordinal<arcs.Count;ordinal++)if(arcs[ordinal].Span==span)
   {
    var ranked=rankedGroups[arcs[ordinal].GroupId];
    foreach(var entry in ranked.Entries.Span[..Math.Min(20,ranked.Entries.Length)])
    {
     if(!scorer.Alphabet.TryGetText(entry.Symbol,out string text)||text.Length!=1||!(text[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '\u0400' and <= '\u04ff' or >= '0' and <= '9'))continue;
     foreach(bool end in new[]{false,true})
     {
      int category=categories.GetSymbolCompositeCategory(entry.Symbol,end);if(category<0||categories.GetFirstId(category)>5)continue;
      int cost=ranked.Query(entry.Symbol,ushort.MaxValue,category,false,false);
      requests[node].Add(new((ushort)ordinal,span,entry.Symbol,(byte)category,cost,ranked.GetFirstControllerLikelihoodByte(entry.Symbol)));
     }
    }
   }
   if(span==0)break;
  }
  var calls=new List<(int Stage,LegacyFirstOrdinaryTransitionRequest Request,int Incoming,LegacyFirstOrdinaryTransitionResult Result)>();
  int callbacks=0;var result=LegacyFirstOrdinaryAcousticRecurrence.Run(links,requests,5,(r,c,t)=>{callbacks++;var v=geometry.Correct118(r,c,t);calls.Add((118,r,c,v));return v;},(r,c,t)=>{var v=geometry.Correct140(r,c,t);calls.Add((140,r,c,v));return v;});
  int continuations=0;
  for(int node=0;node<result.NodeCount;node++)
  {
   var rowCopy=result.GetRowCopy(node);for(int i=0;i<rowCopy.Count;i++)
   {
    var path=result.TracePrimary(node,i);if(path.Length>1)continuations++;
    int tracedNode=-1;long acoustic=0;foreach(var f in path){tracedNode+=f.Span+1;acoustic+=requests[tracedNode].First(r=>r.ArcOrdinal==f.ArcOrdinal&&r.Symbol==f.Symbol&&r.CompositeCategory==f.CompositeCategory).GroupCost;}
    if(path[^1].TotalCost!=rowCopy.Get(i).TotalCost||path[^1].AcousticCost!=acoustic)throw new Exception("Native record trace lost cumulative acoustic/correction distinction.");
   }
  }
  if(callbacks==0||continuations==0||!calls.Any(c=>c.Stage==118&&c.Request.Kind==2&&!c.Result.Accepted))throw new Exception("Actual first NN did not reach nonroot geometry recurrence/internal-cut kind2 rejection.");
  var last=result.GetRowCopy(graph.NodeCount-1);var outputs=Enumerable.Range(0,last.Count).Select(i=>string.Concat(result.TracePrimary(graph.NodeCount-1,i).Select(f=>scorer.Alphabet.TryGetText(f.Symbol,out string s)?s:"?"))+":"+last.Get(i).TotalCost);
  Console.WriteLine($"Actual own MIR scale={scale}: {graph.Groups.Count} NN groups/{callbacks} callback118 calls/{continuations} nonroot traces, raw primary alternatives[{string.Join(", ",outputs)}]. Not final native winner/accuracy claim.");
  if(audit!=null)
  {
   audit.WriteStartObject();audit.WriteNumber("height",row.Frame.Line.Height);
   audit.WriteStartArray("profile");foreach(float v in new[]{geometryProfile.PairScale,geometryProfile.PairOffset,geometryProfile.PairMaximum,geometryProfile.DefaultGap,geometryProfile.TranslationThreshold,geometryProfile.GapSlope})audit.WriteNumberValue(v);audit.WriteEndArray();
   audit.WriteStartArray("table");foreach(float v in separation.Values.Span)audit.WriteNumberValue(v);audit.WriteEndArray();
   audit.WriteStartArray("matrix");foreach(float v in geometryProfile.GapMatrix.Span)audit.WriteNumberValue(v);audit.WriteEndArray();
   audit.WriteStartArray("categories");foreach(var c in geometryProfile.Categories.CompositeCategories.Span){audit.WriteStartArray();audit.WriteNumberValue(c.FirstId);audit.WriteNumberValue(c.GeometryIndex);audit.WriteEndArray();}audit.WriteEndArray();
   audit.WriteStartArray("prototypes");foreach(var p in geometryProfile.Categories.Prototypes.Span){audit.WriteStartArray();audit.WriteNumberValue(p.First);audit.WriteNumberValue(p.Second);audit.WriteEndArray();}audit.WriteEndArray();
   audit.WriteStartArray("graph");for(int node=0;node<graph.NodeCount;node++)
   {
    audit.WriteStartArray();var arcs=graph.GetNode(node);for(int ordinal=0;ordinal<arcs.Count;ordinal++)
    {
     var a=arcs[ordinal];audit.WriteStartObject();audit.WriteNumber("span",a.Span);audit.WriteBoolean("wholeStarts",geometry.HasWholePhysicalStarts(node,ordinal));var box=geometry.GetBounds(node,ordinal);
     audit.WriteStartArray("box");foreach(float v in new[]{box.MinimumX,box.MinimumY,box.MaximumX,box.MaximumY})audit.WriteNumberValue(v);audit.WriteEndArray();
     audit.WriteStartArray("pred");foreach(int v in a.PredecessorArcIndices.Span)audit.WriteNumberValue(v);audit.WriteEndArray();
     audit.WriteStartArray("succ");foreach(int v in links.GetSuccessorOrdinals(node,(ushort)ordinal,0).Span)audit.WriteNumberValue(v);audit.WriteEndArray();
     var state=geometry.GetPreparedState(node,ordinal);audit.WriteStartArray("state");audit.WriteNumberValue(state.FutureMinimumX);audit.WriteNumberValue(state.PrefixMaximumX);audit.WriteNumberValue(state.JoinCost);audit.WriteEndArray();audit.WriteEndObject();
    }audit.WriteEndArray();
   }audit.WriteEndArray();
   audit.WriteStartArray("calls");foreach(var call in calls)
   {
    var r=call.Request;audit.WriteStartArray();foreach(int v in new[]{call.Stage,r.CurrentNode,(int)r.CurrentArc,(int)r.CurrentCategory,r.PreviousNode,(int)r.PreviousArc,(int)r.PreviousCategory,(int)r.Kind,call.Incoming,call.Result.Cost,call.Result.Accepted?1:0})audit.WriteNumberValue(v);audit.WriteEndArray();
   }audit.WriteEndArray();audit.WriteEndObject();
  }
  using var cancelled=new CancellationTokenSource();cancelled.Cancel();try{geometry.Correct118(default,0,cancelled.Token);throw new Exception("Geometry cancellation ignored.");}catch(OperationCanceledException){}
  totalCallbacks+=callbacks;totalContinuations+=continuations;fixtureCount++;
 }
 if(audit!=null){audit.WriteEndArray();audit.Flush();}
 Console.WriteLine($"PASS {fixtureCount} actual first247 raw packet/24a/265/297/firstNN/final195/ranked/category195520/118+140/native primary recurrence diagnostics; {totalCallbacks} callback118 calls/{totalContinuations} nonroot traces. Remaining A8/B0/B8 and terminal winner explicit.");
}
static LegacyFirstTransitionBounds Box(JsonElement a)=>new(a[0].GetSingle(),a[1].GetSingle(),a[2].GetSingle(),a[3].GetSingle());
static InkStroke Draw(params(float X,float Y)[] points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
