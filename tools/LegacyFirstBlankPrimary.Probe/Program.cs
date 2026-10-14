using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
if(args.Length!=4)throw new ArgumentException("AK.res engine-profile.json separation-profile.json captures.json");
byte[] resource=File.ReadAllBytes(args[0]);using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var activation=engine.RootElement.GetProperty("activations");var activations=new LegacyInkActivations(activation.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),activation.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));
InkStroke[][] fixtures=[
 [Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((65,90),(65,10)),Draw((65,90),(115,10)),Draw((115,90),(115,10)),Draw((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))],
 [Draw((0,0),(60,0),(60,80),(0,80),(0,0)),Draw((20,30),(30,50))],
 [Draw((0,0),(60,0),(60,80),(0,80),(0,0))],
 [Draw((0,0),(0,80)),Draw((200,0),(200,80)),Draw((400,0),(400,80))]
];
using var stream=File.Create(args[3]);using var w=new Utf8JsonWriter(stream);w.WriteStartArray();int groupCount=0,blankCount=0,skipped=0,selectedBlanks=0;
using var contextStream=File.Create(args[3]+".contexts.json");using var cw=new Utf8JsonWriter(contextStream);cw.WriteStartArray();int contextCases=0;
foreach(var raw in fixtures)
{
 var factory=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,6120,raw);var row=factory.Frame.Row;var cuts=row.GenerateRussianCuts();var profile=LegacyFirstTransitionGeometryProfile.ReadSource(resource,6120);int go=profile.SamplerOffset+8+BitConverter.ToInt32(resource,profile.SamplerOffset+4);
 var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,go),new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());var prepared=LegacyFreshFirstCandidateGraphPreparer.Prepare(resource,factory,cuts,seeds,table);var graph=prepared.SourceRanges.Graph;var links=new LegacyOrdinaryCandidateConnections(graph);
 var policy=LegacyFreshFirstControllerPolicy.ReadFirstSource(resource,6120);var scope=new LegacyFirstRowPrimitiveScope(row,cuts,raw.Select(s=>s.Points.Count).ToArray());var scorer=LegacyFirstOrdinaryCacheScorer.Prepare(resource,6120,row,activations);var mask=Enumerable.Repeat((byte)1,scorer.Alphabet.LabelCount).ToArray();var scores=new LegacyFirstOrdinaryCacheScores[graph.Groups.Count];
 for(int g=0;g<scores.Length;g++)
 {
  var ranges=graph.Groups[g].Ranges.ToArray();var order=cuts.Chronology.OrderRangeIndicesForNormalization(ranges);var boundary=LegacyCursiveGlyphBoundaryPolicy.Resolve(order.Select(i=>ranges[i]).ToArray(),cuts.Cuts.PreparedInk.StrokePointCounts.Span,0,false,false);
  scores[g]=scorer.Score(cuts,ranges,boundary.Start,boundary.End,15,Math.Min(256,policy.MaximumNormalizedStrokes),mask,glyph=>policy.CreateContext(scope.Evaluate(ranges,glyph.MergedGeometry.Strokes.Count),[]),[]);
 }
 groupCount+=scores.Length;var separation=LegacyCursiveSeparationProfile.ReadV106Node(resource,profile.SamplerOffset);
 foreach(bool protocol in new[]{false,true})
 {
  var plan=LegacyFreshFirstBlankPrimary.Prepare(resource,factory,cuts,prepared,scores,table,protocol);if(plan.NativeConsecutiveBlankExclusion)throw new Exception("Actual source B8=0 changed.");
  if(!protocol)
  {
   var foreign=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,6120,raw);
   try{LegacyFreshFirstBlankPrimary.Prepare(resource,foreign,cuts,prepared,scores,table,false);throw new Exception("Equal-valued foreign factory accepted.");}catch(ArgumentException){}
   using var canceled=new CancellationTokenSource();canceled.Cancel();
   try{LegacyFreshFirstBlankPrimary.Prepare(resource,factory,cuts,prepared,scores,table,false,canceled.Token);throw new Exception("Prepare cancellation ignored.");}catch(OperationCanceledException){}
   try{plan.RunSupportedNonSkippedContext(canceled.Token);throw new Exception("Owning context cancellation ignored.");}catch(OperationCanceledException){}
   try{plan.ReadGroupCost(graph.NodeCount,0,22,0);throw new Exception("Foreign node query accepted.");}catch(ArgumentOutOfRangeException){}
  }
  static int[] Capture(int stage,LegacyFirstAncestorTransitionRequest r,int value,LegacyFirstOrdinaryTransitionResult result)
  {var p=r.Previous;return [stage,r.CurrentNode,r.CurrentArc,r.CurrentSymbol,r.CurrentCategory,r.CurrentSpan,r.Kind,p.OriginalNode,p.OriginalAlternative,p.OriginalRecord.ArcOrdinal,p.OriginalRecord.Symbol,p.GeometryNode,p.GeometryAlternative,p.GeometryRecord.ArcOrdinal,p.GeometryRecord.Symbol,p.GeometryRecord.CompositeCategory,p.SkippedCount,value,result.Cost,result.Accepted?1:0];}
  var calls=new List<int[]>();var primary=plan.RunWithObserver((stage,r,value,result)=>
  {
   calls.Add(Capture(stage,r,value,result));skipped+=r.Previous.SkippedCount;
  });
  //Prepared geometry controls cover ancestor distances not observed in these
  //actual retained beams. Their explicit record costs are not NN outputs and
  //are not inserted into the actual producing kernel or winning trace.
  var geometryControls=new List<int[]>();var bridge=new LegacyFirstAncestorTransitionGeometry(resource,factory,cuts,prepared,table);
  for(int n=0;n<graph.NodeCount;n++)for(ushort a=0;a<graph.GetNode(n).Count;a++)
  {
   var current=graph.GetNode(n)[a];int parent=n-current.Span-1;if(parent<1)continue;
   foreach(int pa in current.PredecessorArcIndices.Span)
   {
    var originalArc=graph.GetNode(parent)[pa];int an=parent-originalArc.Span-1;if(an<0)continue;
    foreach(int aa in originalArc.PredecessorArcIndices.Span)foreach(ushort gs in new ushort[]{22,189})foreach(byte kind in new byte[]{0,2,5,6})
    {
     var ancestorArc=graph.GetNode(an)[aa];var original=new LegacyFirstAcousticRecordFields(101,99,0,0,(ushort)pa,originalArc.Span,192,false,0,33,0);
     var ancestor=new LegacyFirstAcousticRecordFields(51,49,255,0,(ushort)aa,ancestorArc.Span,gs,true,0,0,0);
     var owner=new LegacyFirstGeometricAncestor(parent,0,original,an,0,ancestor,1);ushort symbol=kind==5?(ushort)192:kind==6?(ushort)191:(ushort)22;byte category=kind==5?(byte)33:kind==6?(byte)24:(byte)0;
     var request=new LegacyFirstAncestorTransitionRequest(n,a,symbol,category,current.Span,kind,owner);
     var v118=bridge.Correct118(request,0);geometryControls.Add(Capture(118,request,0,v118));
     if(v118.Accepted){var v140=bridge.Correct140(request,v118.Cost);geometryControls.Add(Capture(140,request,v118.Cost,v140));}
    }
   }
  }
  var repeat=plan.Run();for(int n=0;n<graph.NodeCount;n++)if(!primary.GetRowCopy(n).CopyStorage().SequenceEqual(repeat.GetRowCopy(n).CopyStorage()))throw new Exception("Read-only observation changed native rows.");
  using(var canceled=new CancellationTokenSource())
  {
   try{plan.RunWithObserver((s,r,c,v)=>canceled.Cancel(),canceled.Token);throw new Exception("Observer cancellation ignored.");}catch(OperationCanceledException){}
  }
  int blanks=plan.Requests.Sum(rs=>rs.Count(r=>r.Symbol==192));blankCount+=blanks;
  for(int n=0;n<graph.NodeCount;n++)for(int a=0;a<primary.GetRowCopy(n).Count;a++)if(primary.GetRowCopy(n).Get(a).Symbol==192)selectedBlanks++;
  Console.WriteLine($"Actual{scores.Length} firstNN groups/nativepruned cap7/F8=5/protocol194={protocol}: {plan.Requests.Sum(rs=>rs.Count)} requests including{blanks} realfallbacks; lastnode[{string.Join(',',Enumerable.Range(0,primary.GetRowCopy(graph.NodeCount-1).Count).Select(a=>primary.GetRowCopy(graph.NodeCount-1).Get(a).Symbol+":"+primary.GetRowCopy(graph.NodeCount-1).Get(a).TotalCost))}].");
  //The existing context implementation is used only when every retained row
  //is genuinely inside its proved no-skipped/nonreplacement literal scope.
  //Never drop a blank/unsupported row to obtain a successful context result.
  var literal=LegacyFreshFirstLiteralContextPolicy.ReadSource(resource,6120,factory);
  bool compatible=true;for(int n=0;n<primary.NodeCount;n++)for(int a=0;a<primary.GetRowCopy(n).Count;a++)
   if(!literal.SupportsSymbol(primary.GetRowCopy(n).Get(a).Symbol)||primary.GetRowCopy(n).Get(a).Symbol==189)compatible=false;
  if(compatible)
  {
   var contextGeometry=new LegacyFirstOrdinaryTransitionGeometry(profile,factory.Frame,cuts,graph,table);var contextCalls=new List<int[]>();
   LegacyFirstOrdinaryTransitionResult Apply(int stage,LegacyFirstOrdinaryTransitionRequest r,int value,CancellationToken token)
   {
    var result=stage==118?contextGeometry.CorrectLiteral118(r,value,token):contextGeometry.CorrectLiteral140(r,value,token);
    contextCalls.Add([stage,r.CurrentNode,r.CurrentArc,r.CurrentSymbol,r.CurrentCategory,r.PreviousNode,r.PreviousArc,r.PreviousSymbol,r.PreviousCategory,r.Kind,value,result.Cost,result.Accepted?1:0]);return result;
   }
   var context=LegacyFirstOrdinaryContextPasses.RunForFreshLiteralSymbols(primary,links,literal,plan.ReadGroupCost,
    (r,v,t)=>Apply(118,r,v,t),(r,v,t)=>Apply(140,r,v,t),new());
   var owningContext=plan.RunSupportedNonSkippedContext();
   var leftNodes=context.Contexts.CopyNodes().ToDictionary(n=>n.Identity);var rightNodes=owningContext.Contexts.CopyNodes().ToDictionary(n=>n.Identity);
   string Path(ulong identity,bool right)
   {var nodes=right?rightNodes:leftNodes;var result=new List<ushort>();while(identity!=0){var node=nodes[identity];result.Add(node.Symbol);identity=node.ParentLink;}result.Reverse();return string.Join(',',result);}
   bool SameRecords(ReadOnlyMemory<byte>[] left,ReadOnlyMemory<byte>[] right)
   {
    if(left.Length!=right.Length)return false;
    for(int n=0;n<left.Length;n++)
    {
     byte[] a=left[n].ToArray(),b=right[n].ToArray();if(a.Length!=b.Length)return false;
     for(int p=0;p<a.Length;p+=40)
     {if(Path(BinaryPrimitives.ReadUInt64LittleEndian(a.AsSpan(p+24)),false)!=Path(BinaryPrimitives.ReadUInt64LittleEndian(b.AsSpan(p+24)),true))return false;a.AsSpan(p+24,8).Clear();b.AsSpan(p+24,8).Clear();}
     if(!a.SequenceEqual(b))return false;
    }return true;
   }
   if(!SameRecords(context.CopyB0Storage(),owningContext.CopyB0Storage())||!SameRecords(context.CopyB8Storage(),owningContext.CopyB8Storage()))throw new Exception("Owning primary/context endpoint changed scalar records or symbolic paths.");
   var grid=new LegacyFirstMergedRecordGrid(context.ColumnCountB8,context.BeamWidth,context.CopyB8Storage());
   var terminal=LegacyFirstTerminalSelection.Select(grid,new(factory.InvocationPolicy.Binding.TrailingSymbolE6,0xa000,
    factory.InvocationPolicy.Invocation.Configuration99,factory.InvocationPolicy.Invocation.ConfigurationA8,factory.InvocationPolicy.Binding.SourceKind,0));
   var trace=LegacyFirstSelectedGlyphTrace.Read(grid,terminal.Choices[0]);
   Console.WriteLine($"NEW owned blank-primary->actualGroupCost->A8/B0/B8: {terminal.Choices[0].TotalCost}, labels[{string.Join(',',trace.Select(r=>r.Symbol))}]. No blank/ancestor rows discarded; scoped diagnostic, no final linguistic winner.");
   cw.WriteStartObject();cw.WriteBoolean("modeE0",literal.Binding.NativeModeE0);cw.WriteNumber("closedSymbol",literal.ClosedSymbolE4);cw.WriteNumber("beamWidth",context.BeamWidth);cw.WriteNumber("columnCount",context.ColumnCountB8);
   void CStorage(string name,IEnumerable<ReadOnlyMemory<byte>> rows){cw.WriteStartArray(name);foreach(var b in rows)cw.WriteStringValue(Convert.ToHexString(b.Span));cw.WriteEndArray();}
   void CArrays(string name,IEnumerable<int[]> rows){cw.WriteStartArray(name);foreach(var values in rows){cw.WriteStartArray();foreach(int v in values)cw.WriteNumberValue(v);cw.WriteEndArray();}cw.WriteEndArray();}
   CStorage("primary",Enumerable.Range(0,primary.NodeCount).Select(n=>(ReadOnlyMemory<byte>)primary.GetRowCopy(n).CopyStorage()));CStorage("nodes",context.CopyB8Storage());CStorage("b0",context.CopyB0Storage());
   CArrays("costs",plan.Requests.SelectMany((rs,n)=>rs.Select(r=>new[]{n,(int)r.ArcOrdinal,(int)r.Symbol,(int)r.CompositeCategory,plan.ReadGroupCost(n,r.ArcOrdinal,r.Symbol,r.CompositeCategory)})));
   var successors=new List<int[]>();for(int n=0;n<graph.NodeCount;n++)for(ushort a=0;a<graph.GetNode(n).Count;a++)for(byte s=0;s<8;s++)
    if(n+s+1<graph.NodeCount)successors.Add(new[]{n,(int)a,(int)s}.Concat(links.GetSuccessorOrdinals(n,a,s).ToArray()).ToArray());
   CArrays("successors",successors);CArrays("calls",contextCalls);cw.WriteStartArray("contexts");foreach(var node in context.Contexts.CopyNodes()){cw.WriteStartObject();cw.WriteNumber("identity",node.Identity);cw.WriteNumber("parent",node.ParentLink);cw.WriteNumber("symbol",node.Symbol);cw.WriteEndObject();}cw.WriteEndArray();cw.WriteEndObject();contextCases++;
  }
  else Console.WriteLine("Existing literal B0/B8 refused entire unsupported primary; no row filtering or guessed ancestor context.");
  w.WriteStartObject();w.WriteBoolean("protocolExcluded",protocol);w.WriteBoolean("excludeConsecutive",plan.NativeConsecutiveBlankExclusion);w.WriteNumber("closed",191);w.WriteNumber("replacement",189);w.WriteNumber("blank",192);w.WriteNumber("beam",factory.PrimaryBeamWidthF8);
  void Arrays(string name,IEnumerable<int[]> data){w.WriteStartArray(name);foreach(var values in data){w.WriteStartArray();foreach(int v in values)w.WriteNumberValue(v);w.WriteEndArray();}w.WriteEndArray();}
  void Floats(string name,IEnumerable<float> data){w.WriteStartArray(name);foreach(float v in data)w.WriteNumberValue(v);w.WriteEndArray();}
  Arrays("requests",plan.Requests.SelectMany((rs,n)=>rs.Select(r=>new[]{n,(int)r.ArcOrdinal,(int)r.Span,(int)r.Symbol,(int)r.CompositeCategory,r.GroupCost,(int)r.LikelihoodByte})));
  Arrays("arcs",Enumerable.Range(0,graph.NodeCount).SelectMany(n=>graph.GetNode(n).Select((a,i)=>new[]{n,i,(int)a.Span,a.GroupId})));
  Arrays("pred",Enumerable.Range(0,graph.NodeCount).SelectMany(n=>graph.GetNode(n).Select((a,i)=>new[]{n,i}.Concat(a.PredecessorArcIndices.ToArray()).ToArray())));
  Arrays("root",Enumerable.Range(0,8).Select(s=>new[]{s}.Concat(links.GetSuccessorOrdinals(-1,ushort.MaxValue,(byte)s).ToArray()).ToArray()));
  Arrays("firstIds",profile.Categories.CompositeCategories.ToArray().Select((c,i)=>new[]{i,c.FirstId}));Arrays("geometryIds",profile.Categories.CompositeCategories.ToArray().Select((c,i)=>new[]{i,c.GeometryIndex}));Arrays("start",profile.Categories.StartCompositeMap.ToArray().Select((c,i)=>new[]{i,c}));Arrays("end",profile.Categories.EndCompositeMap.ToArray().Select((c,i)=>new[]{i,c}));
  w.WriteNumber("height",row.Frame.Line.Height);w.WriteNumber("pairScale",profile.PairScale);w.WriteNumber("pairOffset",profile.PairOffset);w.WriteNumber("pairMaximum",profile.PairMaximum);w.WriteNumber("gap",profile.DefaultGap);w.WriteNumber("translation",profile.TranslationThreshold);w.WriteNumber("slope",profile.GapSlope);w.WriteNumber("joinCap",profile.JoinMaximumCost);w.WriteNumber("matrixCap",profile.MatrixMaximumCost);w.WriteNumber("blankThreshold",-.5f);w.WriteBoolean("useMaxX",true);Floats("table",table.Values.ToArray());Floats("matrix",profile.GapMatrix.ToArray());
  w.WriteStartArray("prototypes");foreach(var p in profile.Categories.Prototypes.Span){w.WriteStartArray();w.WriteNumberValue(p.First);w.WriteNumberValue(p.Second);w.WriteEndArray();}w.WriteEndArray();
  w.WriteStartArray("points");foreach(var p in cuts.Cuts.PreparedInk.Points.Span){w.WriteStartArray();w.WriteNumberValue(p.X);w.WriteNumberValue(p.Y);w.WriteEndArray();}w.WriteEndArray();Arrays("counts",[cuts.Cuts.PreparedInk.StrokePointCounts.ToArray()]);
  w.WriteStartArray("groups");for(int g=0;g<scores.Length;g++)
  {
   int sep=LegacyCursiveGroupSeparationCost.Evaluate(cuts.Cuts.PreparedInk,graph.Groups[g].Ranges.Span,row.Frame.Line.Height,separation.GapOffset,separation.Slope,table.Values.Span,separation.MaximumCost);
   var ranked=new LegacyOrdinaryRankedAcousticGroup(scores[g].Cache,scorer.Alphabet,resource.AsSpan(scorer.AlphabetOffset,16),Enumerable.Repeat(sep,profile.Categories.CompositeCategories.Length).ToArray(),1);
   w.WriteStartObject();w.WriteNumber("fallback",scores[g].Cache.FallbackCost);w.WriteNumber("geometry",sep);w.WriteStartArray("ranges");foreach(var r in graph.Groups[g].Ranges.Span){w.WriteStartArray();w.WriteNumberValue(r.StrokeIndex);w.WriteNumberValue(r.StartPoint);w.WriteNumberValue(r.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();
   w.WriteStartArray("top");foreach(var r in ranked.Entries.Span[..Math.Min(20,ranked.Entries.Length)]){w.WriteStartArray();w.WriteNumberValue(r.Symbol);w.WriteNumberValue(r.Cost);w.WriteNumberValue(ranked.GetFirstControllerLikelihoodByte(r.Symbol));w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
  }w.WriteEndArray();Arrays("calls",calls);Arrays("geometryControls",geometryControls);w.WriteStartArray("primary");for(int n=0;n<primary.NodeCount;n++)w.WriteStringValue(Convert.ToHexString(primary.GetRowCopy(n).CopyStorage()));w.WriteEndArray();w.WriteEndObject();
 }
}
w.WriteEndArray();w.Flush();if(blankCount==0)throw new Exception("No native blank fallback requests exercised.");
cw.WriteEndArray();cw.Flush();if(contextCases==0)throw new Exception("New real primary was not consumed by the supported actual context pipeline.");
Console.WriteLine($"PASS actual{groupCount} NN groups->{blankCount} fallback proposals; {selectedBlanks} retainedblank records/{skipped} observed skippedancestor steps. Independent proposal/geometry/primary audit required; no fullB8/winner/Russian claim.");
static InkStroke Draw(params(float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
