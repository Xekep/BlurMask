using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=4)throw new ArgumentException("AK.res engine-profile.json separation-profile.json captures.json");
byte[] resource=File.ReadAllBytes(args[0]);using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=engine.RootElement.GetProperty("activations");var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));
void Require(bool condition,string message){if(!condition)throw new Exception(message);}
InkStroke[] raw=[new(new InkPoint[]{new(0,0),new(60,0),new(60,80),new(0,80),new(0,0)}),new(new InkPoint[]{new(20,30),new(30,50)})];
var factory=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,6120,raw);var frame=factory.Frame;var row=frame.Row;var cuts=row.GenerateRussianCuts();var profile=LegacyFirstTransitionGeometryProfile.ReadSource(resource,6120);var categories=new LegacyCursiveCategoryMapper(profile.Categories);
var policy=LegacyFreshFirstControllerPolicy.ReadFirstSource(resource,6120);var scope=new LegacyFirstRowPrimitiveScope(row,cuts,raw.Select(s=>s.Points.Count).ToArray());var scorer=LegacyFirstOrdinaryCacheScorer.Prepare(resource,6120,row,activations);var mask=Enumerable.Repeat((byte)1,scorer.Alphabet.LabelCount).ToArray();
var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,12727),new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(seeds),5);var links=new LegacyOrdinaryCandidateConnections(graph);
var geometry=new LegacyFirstOrdinaryTransitionGeometry(profile,frame,cuts,graph,table);var separation=LegacyCursiveSeparationProfile.ReadV106Node(resource,profile.SamplerOffset);var scores=new LegacyFirstOrdinaryCacheScores[graph.Groups.Count];var ranked=new LegacyOrdinaryRankedAcousticGroup[graph.Groups.Count];
for(int g=0;g<scores.Length;g++)
{
 var ranges=graph.Groups[g].Ranges.ToArray();var order=cuts.Chronology.OrderRangeIndicesForNormalization(ranges);var clip=LegacyCursiveGlyphBoundaryPolicy.Resolve(order.Select(i=>ranges[i]).ToArray(),cuts.Cuts.PreparedInk.StrokePointCounts.Span,0,false,false);
 scores[g]=scorer.Score(cuts,ranges,clip.Start,clip.End,15,Math.Min(256,policy.MaximumNormalizedStrokes),mask,glyph=>policy.CreateContext(scope.Evaluate(ranges,glyph.MergedGeometry.Strokes.Count),[]),[]);
 int cost=LegacyCursiveGroupSeparationCost.Evaluate(cuts.Cuts.PreparedInk,ranges,row.Frame.Line.Height,separation.GapOffset,separation.Slope,table.Values.Span,separation.MaximumCost);
 ranked[g]=new(scores[g].Cache,scorer.Alphabet,resource.AsSpan(scorer.AlphabetOffset,16),Enumerable.Repeat(cost,categories.CompositeCategoryCount).ToArray(),1);
}
var requests=Enumerable.Range(0,graph.NodeCount).Select(_=>new List<LegacyFirstOrdinaryGlyphRequest>()).ToArray();int primitiveRequests=0;
Require(scorer.Alphabet.TryGetLabel("\U000f0001",out ushort closed),"Actual primitive missing.");
for(int node=0;node<graph.NodeCount;node++)for(byte span=(byte)Math.Min(4,node);;span--)
{
 var arcs=graph.GetNode(node);
 for(int ordinal=0;ordinal<arcs.Count;ordinal++)if(arcs[ordinal].Span==span)
 {
  var group=ranked[arcs[ordinal].GroupId];
  foreach(var entry in group.Entries.Span[..Math.Min(20,group.Entries.Length)])foreach(bool end in new[]{false,true})
  {
   int category=categories.GetSymbolCompositeCategory(entry.Symbol,end);if(category<0||categories.GetFirstId(category)>5)continue;
   requests[node].Add(new((ushort)ordinal,span,entry.Symbol,(byte)category,group.Query(entry.Symbol,ushort.MaxValue,category,false,false),group.GetFirstControllerLikelihoodByte(entry.Symbol)));if(entry.Symbol==closed)primitiveRequests++;
  }
 }
 if(span==0)break;
}
Require(primitiveRequests>0,"Actual NN/cache primitive must enter supplied six-category proposals.");
Require(factory.PrimaryBeamWidthF8==5,"Actual source BA/F8 capacity changed.");
var primary=LegacyFirstOrdinaryAcousticRecurrence.Run(links,requests,factory.PrimaryBeamWidthF8,geometry.CorrectLiteral118,geometry.CorrectLiteral140);
int GlyphCost(int n,ushort arc,ushort symbol,byte category,CancellationToken token)=>ranked[graph.GetNode(n)[arc].GroupId].Query(symbol,ushort.MaxValue,category,true,false,token);
var literalPolicy=LegacyFreshFirstLiteralContextPolicy.ReadSource(resource,6120,factory);
using var captureFile=File.Create(args[3]);using var captureWriter=new Utf8JsonWriter(captureFile);
captureWriter.WriteStartArray();int specialCalls=0;
LegacyFirstOrdinaryContextPasses Capture(LegacyFirstOrdinaryAcousticRecurrence source,
 IReadOnlyList<IReadOnlyList<LegacyFirstOrdinaryGlyphRequest>> supplied,LegacyFirstOrdinaryGlyphCost cost,
 LegacyFirstOrdinaryTransitionCorrection c118,LegacyFirstOrdinaryTransitionCorrection c140)
{
 var calls=new List<int[]>();
 LegacyFirstOrdinaryTransitionResult Apply(int stage,LegacyFirstOrdinaryTransitionRequest r,int incoming,CancellationToken token)
 {
  var v=(stage==118?c118:c140)(r,incoming,token);
  calls.Add([stage,r.CurrentNode,r.CurrentArc,r.CurrentSymbol,r.CurrentCategory,r.PreviousNode,r.PreviousArc,r.PreviousSymbol,r.PreviousCategory,r.Kind,incoming,v.Cost,v.Accepted?1:0]);
  if(r.Kind==6)specialCalls++;return v;
 }
 var result=LegacyFirstOrdinaryContextPasses.RunForFreshLiteralSymbols(source,links,literalPolicy,cost,
  (r,v,t)=>Apply(118,r,v,t),(r,v,t)=>Apply(140,r,v,t),new());
 var successors=new List<int[]>();
 for(int n=0;n<graph.NodeCount;n++)for(ushort arc=0;arc<graph.GetNode(n).Count;arc++)for(byte span=0;span<8;span++)
 {
  if(n+span+1>=graph.NodeCount)break;
  successors.Add(new[]{n,(int)arc,(int)span}.Concat(links.GetSuccessorOrdinals(n,arc,span).ToArray()).ToArray());
 }
 captureWriter.WriteStartObject();captureWriter.WriteBoolean("modeE0",false);captureWriter.WriteNumber("closedSymbol",closed);
 captureWriter.WriteNumber("beamWidth",result.BeamWidth);captureWriter.WriteNumber("columnCount",1);
 captureWriter.WriteStartArray("primary");for(int n=0;n<source.NodeCount;n++)captureWriter.WriteStringValue(Convert.ToHexString(source.GetRowCopy(n).CopyStorage()));captureWriter.WriteEndArray();
 void Storage(string name,IEnumerable<ReadOnlyMemory<byte>> data){captureWriter.WriteStartArray(name);foreach(var b in data)captureWriter.WriteStringValue(Convert.ToHexString(b.Span));captureWriter.WriteEndArray();}
 Storage("nodes",result.CopyB8Storage());Storage("b0",result.CopyB0Storage());
 void Arrays(string name,IEnumerable<int[]> data){captureWriter.WriteStartArray(name);foreach(var row in data){captureWriter.WriteStartArray();foreach(int v in row)captureWriter.WriteNumberValue(v);captureWriter.WriteEndArray();}captureWriter.WriteEndArray();}
 Arrays("costs",supplied.SelectMany((gs,n)=>gs.Select(g=>new[]{n,(int)g.ArcOrdinal,(int)g.Symbol,(int)g.CompositeCategory,cost(n,g.ArcOrdinal,g.Symbol,g.CompositeCategory,default)})));
 Arrays("successors",successors);Arrays("calls",calls);
 captureWriter.WriteStartArray("contexts");foreach(var c in result.Contexts.CopyNodes())
 {captureWriter.WriteStartObject();captureWriter.WriteNumber("identity",c.Identity);captureWriter.WriteNumber("parent",c.ParentLink);captureWriter.WriteNumber("symbol",c.Symbol);captureWriter.WriteEndObject();}
 captureWriter.WriteEndArray();captureWriter.WriteEndObject();
 return result;
}
var merged=Capture(primary,requests,GlyphCost,geometry.CorrectLiteral118,geometry.CorrectLiteral140);
var grid=new LegacyFirstMergedRecordGrid(1,merged.BeamWidth,merged.CopyB8Storage());var selected=LegacyFirstTerminalSelection.Select(grid,new(factory.InvocationPolicy.Binding.TrailingSymbolE6,0xa000,false,false,factory.InvocationPolicy.Binding.SourceKind,0));
var choice=selected.Choices[0];var trace=LegacyFirstSelectedGlyphTrace.Read(grid,choice);Console.WriteLine($"prepared B8 cost{choice.TotalCost}: {string.Join(',',trace.Select(c=>c.Symbol))}; primitive proposals{primitiveRequests}");
var materializer=new LegacyFirstPlainWordMaterializer(resource,6120,factory,table);var literal=materializer.MaterializeLiteralSymbols(resource,6120,cuts,graph,primary,merged,scores);
var allocation=LegacyFreshFirstSelectedGlyphPublication.Create(factory);var emitted=trace.Where(c=>allocation.ShouldAllocate(c.Symbol)).ToArray();
Require(literal.GlyphReferences.Count==emitted.Length&&literal.PublishedWord.Word.GetSymbolSpan()==emitted.Length,"Native allocation guard/source ownership differs.");
var confidence=new LegacyFirstOwnedGlyphConfidence(LegacyFirstOrdinaryGlyphScoreProducer.ReadFreshSelectedSource(resource,6120),factory,cuts,graph,scores);int closedSelected=0;
foreach(var cell in trace.Where(c=>c.Symbol==closed))
{Require(confidence.ReadSymbolScore(graph.GetNode(cell.Node)[cell.Arc].GroupId,closed)>=0,"Selected primitive confidence lost its source owner.");closedSelected++;}
for(int i=0;i<emitted.Length;i++)
{
 var cell=emitted[i];byte[] glyph=literal.Result.GlyphPool.CopyRecord(literal.GlyphReferences[i]);ushort published=BinaryPrimitives.ReadUInt16LittleEndian(glyph.AsSpan(8));
 Require(published==cell.Symbol&&published!=closed,"Fresh99=false E4 unexpectedly allocated a glyph.");
}
Require(closedSelected>0&&emitted.Length<trace.Count,"Own prepared B8 diagnostic did not exercise actual selected E4 allocation suppression.");
try{materializer.Materialize(cuts,graph,primary,merged,scores);throw new Exception("Original strict API unexpectedly accepted primitive.");}catch(NotSupportedException){}
Console.WriteLine($"PASS actual source/frame/NN/{scores.Length} groups ->supplied actual-cache proposals ->native98/A8/B0/B8 ->first275 ({closedSelected} selected F0001 suppressed by native99=false guard). Proposal enumeration is explicit bounded six-category fixture scope; not complete native automatic273 controller/Russian accuracy.");

//Independent scalar callbacks distinguish request kind6 from record kind0,
//and compare subsequent symbolic paths with the Python tuple oracle.
ushort[] literals=Enumerable.Range(0,scorer.Alphabet.LabelCount).Select(i=>(ushort)i).Where(literalPolicy.SupportsSymbol).ToArray();
for(int test=0;test<128;test++)
{
 var own=Enumerable.Range(0,graph.NodeCount).Select(_=>new List<LegacyFirstOrdinaryGlyphRequest>()).ToArray();
 for(int n=0;n<graph.NodeCount;n++)for(ushort arc=0;arc<graph.GetNode(n).Count;arc++)
 {
  byte span=checked((byte)graph.GetNode(n)[arc].Span);
  foreach(ushort symbol in new[]{closed,literals[(test*13+n*19+arc*7)%literals.Length]}.Distinct())
   own[n].Add(new(arc,span,symbol,(byte)((test+n+arc)%4),OwnCost(n,arc,symbol,(byte)((test+n+arc)%4),default),
    (byte)(1+(symbol%254))));
 }
 LegacyFirstOrdinaryTransitionResult Own118(LegacyFirstOrdinaryTransitionRequest r,int v,CancellationToken t)
 {t.ThrowIfCancellationRequested();return (r.CurrentSymbol+r.PreviousSymbol+r.Kind+test)%13==0?new(false,0):new(true,v+(r.Kind*7+r.CurrentNode+r.PreviousSymbol)%19-9);}
 LegacyFirstOrdinaryTransitionResult Own140(LegacyFirstOrdinaryTransitionRequest r,int v,CancellationToken t)
 {t.ThrowIfCancellationRequested();return new(true,v+(r.CurrentArc+r.PreviousArc*3+r.Kind*11+test)%17-8);}
 var p=LegacyFirstOrdinaryAcousticRecurrence.Run(links,own,factory.PrimaryBeamWidthF8,Own118,Own140);
 Capture(p,own,OwnCost,Own118,Own140);
}
Require(specialCalls>0,"No nonroot E4 correction-kind6 request was exercised.");
foreach(ushort forbidden in new[]{(ushort)scorer.Alphabet.LabelCount,(ushort)180,(ushort)181,(ushort)182,ushort.MaxValue})
 Require(!literalPolicy.SupportsSymbol(forbidden),"Unsupported blank/ancestor-skip label admitted.");
for(int stage=0;stage<3;stage++)
{
 using var cancel=new CancellationTokenSource();int subsequent=0;
 try
 {
  LegacyFirstOrdinaryContextPasses.RunForFreshLiteralSymbols(primary,links,literalPolicy,
   (n,a,s,c,t)=>{if(stage==0)cancel.Cancel();return GlyphCost(n,a,s,c,t);},
   (r,v,t)=>{if(stage==1)subsequent++;if(stage==2)cancel.Cancel();return new(true,v);},
   (r,v,t)=>{if(stage==1)cancel.Cancel();return new(true,v);},new(),cancel.Token);
  throw new Exception("Literal context ignored callback cancellation.");
 }catch(OperationCanceledException){}
 Require(stage!=1||subsequent==0,"118 ran after canceled140.");
}
//Native first sampler6 returns true with unchanged signed cost, including
//the sentinel; the context caller performs its separate budget comparison.
int checks=0;
for(int n=0;n<graph.NodeCount;n++)for(ushort arc=0;arc<graph.GetNode(n).Count;arc++)
{
 var g=graph.GetNode(n)[arc];int pn=n-g.Span-1;if(pn<0)continue;
 foreach(int pa in g.PredecessorArcIndices.Span)foreach(int v in new[]{int.MinValue,-100,0,100,0x4000000,int.MaxValue})
 {
  var request=new LegacyFirstOrdinaryTransitionRequest(n,arc,closed,24,pn,(ushort)pa,closed,24,(byte)g.Span,6);
  Require(geometry.CorrectLiteral118(request,v)==new LegacyFirstOrdinaryTransitionResult(true,v)&&geometry.CorrectLiteral140(request,v)==new LegacyFirstOrdinaryTransitionResult(true,v),"Exact first sampler6 modified signed cost.");
  try{geometry.Correct118(request,v);throw new Exception("Old strict geometry admitted kind6.");}catch(ArgumentException){}
  checks++;
 }
}
Require(checks>0,"No nonroot unchanged-cost controls.");
captureWriter.WriteEndArray();captureWriter.Flush();
Console.WriteLine($"PASS 128 own literal context captures +actual NN capture, {checks} signed kind6 controls, skipped-label bounds and cancellation; audit with independent audit_contexts.py.");
static int OwnCost(int n,ushort a,ushort s,byte c,CancellationToken t){t.ThrowIfCancellationRequested();return(n*11+a*3+s*7+c*13)%67-20;}
