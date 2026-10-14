using InkRecognition;
using System.Text.Json;
if(args.Length!=4)throw new ArgumentException("AK.res engine-profile.json separation-profile.json captures.json");
byte[] resource=File.ReadAllBytes(args[0]);using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=engine.RootElement.GetProperty("activations");var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));
void Require(bool c,string m){if(!c)throw new Exception(m);}
using var output=File.Create(args[3]);using var w=new Utf8JsonWriter(output);w.WriteStartArray();int groupCount=0,requestCount=0,closedCount=0;
InkStroke[][] cases=[
 [Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((65,90),(65,10)),Draw((65,90),(115,10)),Draw((115,90),(115,10)),Draw((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))],
 [Draw((0,0),(60,0),(60,80),(0,80),(0,0)),Draw((20,30),(30,50))],
 [Draw((0,0),(60,0),(60,80),(0,80),(0,0))]
];
foreach(var raw in cases)
{
 var factory=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,6120,raw);var frame=factory.Frame;var row=frame.Row;var cuts=row.GenerateRussianCuts();var profile=LegacyFirstTransitionGeometryProfile.ReadSource(resource,6120);
 var categories=new LegacyCursiveCategoryMapper(profile.Categories);var policy=LegacyFreshFirstControllerPolicy.ReadFirstSource(resource,6120);var scope=new LegacyFirstRowPrimitiveScope(row,cuts,raw.Select(s=>s.Points.Count).ToArray());var scorer=LegacyFirstOrdinaryCacheScorer.Prepare(resource,6120,row,activations);var mask=Enumerable.Repeat((byte)1,scorer.Alphabet.LabelCount).ToArray();
 int go=profile.SamplerOffset+8+BitConverter.ToInt32(resource,profile.SamplerOffset+4);var cap=LegacyCursiveLatticeSpanProfile.ReadV10aResourceNode(resource,go);
 Require(cap.MaximumAtoms==7&&factory.PrimaryBeamWidthF8==5,"Actual graph cap7/primary F8=5 changed.");
 var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,go),new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(seeds),cap.MaximumAtoms);var connections=new LegacyOrdinaryCandidateConnections(graph);
 var scores=new LegacyFirstOrdinaryCacheScores[graph.Groups.Count];
 for(int g=0;g<scores.Length;g++)
 {
  var ranges=graph.Groups[g].Ranges.ToArray();var order=cuts.Chronology.OrderRangeIndicesForNormalization(ranges);var clip=LegacyCursiveGlyphBoundaryPolicy.Resolve(order.Select(i=>ranges[i]).ToArray(),cuts.Cuts.PreparedInk.StrokePointCounts.Span,0,false,false);
  scores[g]=scorer.Score(cuts,ranges,clip.Start,clip.End,15,Math.Min(256,policy.MaximumNormalizedStrokes),mask,glyph=>policy.CreateContext(scope.Evaluate(ranges,glyph.MergedGeometry.Strokes.Count),[]),[]);
 }
 groupCount+=scores.Length;var literal=LegacyFreshFirstLiteralContextPolicy.ReadSource(resource,6120,factory);
 var foreign=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,6120,raw);
 try{LegacyFreshFirstRegularPrimary.Prepare(resource,6120,foreign,cuts,graph,scores,table,false);throw new Exception("Equal-valued foreign fresh frame accepted.");}catch(ArgumentException){}
 var wrongCap=LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(seeds),5);
 try{LegacyFreshFirstRegularPrimary.Prepare(resource,6120,factory,cuts,wrongCap,scores,table,false);throw new Exception("Nonnative graphcap accepted.");}catch(ArgumentException){}
 using(var cancel=new CancellationTokenSource())
 {cancel.Cancel();try{LegacyFreshFirstRegularPrimary.Prepare(resource,6120,factory,cuts,graph,scores,table,false,cancel.Token);throw new Exception("Prepare cancellation ignored.");}catch(OperationCanceledException){}}
 foreach(bool excluded in new[]{false,true})
 {
  //Explicit prepared owner194 control; the fresh20e allocation/counter
  //binding proof is not replaced by an assumed protocol boolean.
  var plan=LegacyFreshFirstRegularPrimary.Prepare(resource,6120,factory,cuts,graph,scores,table,excluded);
  var calls=new List<int[]>();
  LegacyFirstOrdinaryTransitionResult Apply(int stage,LegacyFirstOrdinaryTransitionRequest r,int v,CancellationToken t)
  {
   var result=stage==118?plan.Correct118(r,v,t):plan.Correct140(r,v,t);
   calls.Add([stage,r.CurrentNode,r.CurrentArc,r.CurrentSymbol,r.CurrentCategory,r.PreviousNode,r.PreviousArc,r.PreviousSymbol,r.PreviousCategory,r.Kind,v,result.Cost,result.Accepted?1:0]);return result;
  }
  var primary=LegacyFirstOrdinaryAcousticRecurrence.Run(connections,plan.Requests,factory.PrimaryBeamWidthF8,(r,v,t)=>Apply(118,r,v,t),(r,v,t)=>Apply(140,r,v,t));
  var automatic=plan.RunRegularLabels();for(int n=0;n<graph.NodeCount;n++)Require(primary.GetRowCopy(n).CopyStorage().SequenceEqual(automatic.GetRowCopy(n).CopyStorage()),"Plan runner changed audited requests/metadata.");
  using(var cancel=new CancellationTokenSource())
  {cancel.Cancel();try{plan.RunRegularLabels(cancel.Token);throw new Exception("Run cancellation ignored.");}catch(OperationCanceledException){}}
  for(int n=0;n<plan.Requests.Count;n++)foreach(var r in plan.Requests[n]){Require(!(r.Symbol==literal.ClosedSymbolE4&&r.Span==n),"Native root E4 exclusion lost.");requestCount++;if(r.Symbol==literal.ClosedSymbolE4)closedCount++;}
  //Full native special primary guards, independent of geometry residuals.
  for(int n=0;n<graph.NodeCount;n++)for(ushort arc=0;arc<graph.GetNode(n).Count;arc++)
  {
   var g=graph.GetNode(n)[arc];int pn=n-g.Span-1;if(pn<0)continue;
   foreach(int pa in g.PredecessorArcIndices.Span)
   {
    var r=new LegacyFirstOrdinaryTransitionRequest(n,arc,literal.ClosedSymbolE4,24,pn,(ushort)pa,22,0,(byte)g.Span,0);
    foreach(int v in new[]{-20,0,500})Require(plan.Correct118(r,v)==new LegacyFirstOrdinaryTransitionResult(true,v)&&plan.Correct140(r,v)==new LegacyFirstOrdinaryTransitionResult(true,v),"Primary E4 ordinary6 modified cost.");
    r=r with{CurrentSymbol=189};Require(!plan.Correct118(r,0).Accepted&&!plan.Correct140(r,0).Accepted,"Current-only E8 acceptedkind0.");
    r=r with{CurrentSymbol=22,PreviousSymbol=189};Require(!plan.Correct118(r,0).Accepted&&!plan.Correct140(r,0).Accepted,"Prior-only E8 acceptedkind0.");
   }
  }
  w.WriteStartObject();w.WriteBoolean("protocolExcluded",excluded);w.WriteNumber("closed",literal.ClosedSymbolE4);w.WriteNumber("count",scorer.Alphabet.LabelCount);w.WriteNumber("beam",factory.PrimaryBeamWidthF8);
  void Arrays(string name,IEnumerable<int[]> rs){w.WriteStartArray(name);foreach(var r in rs){w.WriteStartArray();foreach(int v in r)w.WriteNumberValue(v);w.WriteEndArray();}w.WriteEndArray();}
  Arrays("requests",plan.Requests.SelectMany((rs,n)=>rs.Select(r=>new[]{n,(int)r.ArcOrdinal,(int)r.Span,(int)r.Symbol,(int)r.CompositeCategory,r.GroupCost,(int)r.LikelihoodByte})));
  Arrays("arcs",Enumerable.Range(0,graph.NodeCount).SelectMany(n=>graph.GetNode(n).Select((r,i)=>new[]{n,i,(int)r.Span,r.GroupId})));
  Arrays("pred",Enumerable.Range(0,graph.NodeCount).SelectMany(n=>graph.GetNode(n).Select((r,i)=>new[]{n,i}.Concat(r.PredecessorArcIndices.ToArray()).ToArray())));
  Arrays("root",Enumerable.Range(0,8).Select(span=>new[]{span}.Concat(connections.GetSuccessorOrdinals(-1,ushort.MaxValue,(byte)span).ToArray()).ToArray()));
  Arrays("start",profile.Categories.StartCompositeMap.ToArray().Select((c,i)=>new[]{i,c}));Arrays("end",profile.Categories.EndCompositeMap.ToArray().Select((c,i)=>new[]{i,c}));
  Arrays("firstIds",profile.Categories.CompositeCategories.ToArray().Select((c,i)=>new[]{i,c.FirstId}));
  w.WriteStartArray("groups");for(int g=0;g<scores.Length;g++)
  {
   var sep=LegacyCursiveSeparationProfile.ReadV106Node(resource,profile.SamplerOffset);int cost=LegacyCursiveGroupSeparationCost.Evaluate(cuts.Cuts.PreparedInk,graph.Groups[g].Ranges.Span,row.Frame.Line.Height,sep.GapOffset,sep.Slope,table.Values.Span,sep.MaximumCost);
   var ranked=new LegacyOrdinaryRankedAcousticGroup(scores[g].Cache,scorer.Alphabet,resource.AsSpan(scorer.AlphabetOffset,16),Enumerable.Repeat(cost,categories.CompositeCategoryCount).ToArray(),1);
   w.WriteStartObject();w.WriteNumber("geometry",cost);w.WriteStartArray("top");foreach(var e in ranked.Entries.Span[..Math.Min(20,ranked.Entries.Length)]){w.WriteStartArray();w.WriteNumberValue(e.Symbol);w.WriteNumberValue(e.Cost);w.WriteNumberValue(ranked.GetFirstControllerLikelihoodByte(e.Symbol));w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
  }w.WriteEndArray();
  Arrays("calls",calls);w.WriteStartArray("primary");for(int n=0;n<graph.NodeCount;n++)w.WriteStringValue(Convert.ToHexString(primary.GetRowCopy(n).CopyStorage()));w.WriteEndArray();
  Arrays("blanks",plan.PendingBlankProposals.ToArray().Select(r=>new[]{r.Node,(int)r.Arc,(int)r.Span}));w.WriteEndObject();
  Console.WriteLine($"Actual regular primary cap7/F8=5, protocol194={excluded}, {scores.Length} NN groups/{plan.Requests.Sum(rs=>rs.Count)} requests/{plan.PendingBlankProposals.Length} pendingblankcalls; finalnode [{string.Join(',',Enumerable.Range(0,primary.GetRowCopy(graph.NodeCount-1).Count).Select(i=>primary.GetRowCopy(graph.NodeCount-1).Get(i).Symbol+":"+primary.GetRowCopy(graph.NodeCount-1).Get(i).TotalCost))}]. No complete primary/B8/273 accuracy claim.");
 }
}
w.WriteEndArray();w.Flush();Require(closedCount>0,"No actual nonroot retained primitive proposal.");
Console.WriteLine($"PASS actual{groupCount} first NN groups->{requestCount} native regular-order requests with explicitprotocol194bothvalues, {closedCount} nonrootE4 and native E8 guards. Blank5/ancestor and owner194 production remain separate.");
static InkStroke Draw(params(float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
